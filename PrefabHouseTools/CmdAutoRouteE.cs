#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System.Windows.Forms;
#endregion

namespace PrefabHouseTools
{
    ///Important note:Here are some basic assumptions.
    ///If the project doesn't meet,unexpect result may occur.
    ///
    ///1-Room boundary are straight lines .The program use
    ///Curve class for develop convenient,the logic all assume
    ///they are all lines.
    ///
    ///2-The electrical line go above the ceilings.Rather then go 
    ///under the floor.
    ///
    ///3-All rooms have a common structural ceiling and floor height.
    ///In other words,houses with multiple elevation may not work 
    ///properly now.
    ///This is because the flat scenario apply for most of the 
    ///common houses,which make it workable most of the time.

    /// <summary>
    /// Filter for selecting rooms.
    /// </summary>
    class SelFilterRoom : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;
            try
            {
                if (elem.Category.Id.IntegerValue
                                == (int)BuiltInCategory.OST_Rooms)
                    return true;
            }
            catch
            {
                return false;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    /// <summary>
    /// The main command to calculate the electrical route.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CmdAutoRouteE : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            #region The basic from Revit datebase.
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.
                Application app = uiapp.Application;
            Document doc = uidoc.Document;
            #endregion

            List<Room> roomSelected = new List<Room>();
            ///Step1-Confirm the range of calculation.
            ///
            ///Ask the user for range of rooms to calculate.
            ///This is in case multiple houses or group of 
            ///houses are in the same rvt files while their
            ///electircal system are seperate.
            ///
            ///The result will be in roomSelected as a list.
            #region Code block of step-1
            ///Prepare the dialog to show later.
            TaskDialog td = new TaskDialog("RoomSelection");
            TaskDialogResult tdR;
            string roomNames ;
            ///Start selection.If retry button is hit,
            ///do the loop again.
            do
            {
                ///Cleare the result if retry is hit.
                roomSelected.Clear();
                roomNames = "";

                ///1-ask user to choose all rooms or not.
                td.CommonButtons = TaskDialogCommonButtons.Yes
                    | TaskDialogCommonButtons.No;
                td.MainInstruction = "Do you want to calculate " +
                    "electrical route for all rooms?\n" +
                    "If not,press no and select the rooms to " +
                    "calculate in the model afterward.\n" +
                    "是否为模型中所有房间计算电路路由？\n" +
                    "如果不是，选择no并随后在模型中选择需要计算的房间";
                tdR = td.Show();

                ///2-Filter out the rooms selected.
                try
                {///If yes, than select all rooms in the project.
                    if (tdR == TaskDialogResult.Yes)
                    {
                        FilteredElementCollector colR = 
                            new FilteredElementCollector(doc)
                            .WhereElementIsNotElementType()
                            .OfCategory(BuiltInCategory.OST_Rooms);
                        foreach (Element e in colR)
                        {
                            roomSelected.Add(e as Room);
                            roomNames += e.Name + "\n";
                        }
                    }
                    ///If no, prompt the user to select.
                    ///Using the SelFilterRoom to restrain
                    ///the type of selection to room.
                    else if (tdR == TaskDialogResult.No)
                    {
                        IList<Element> elems = uidoc.Selection.
                            PickElementsByRectangle
                            (new SelFilterRoom(),
                            "Choose the room to calculate route.");
                        foreach (Element e in elems)
                        {
                            roomSelected.Add(e as Room);
                            roomNames += e.Name + "\n";
                        }
                    }
                    ///If window is closed,stop the command.
                    else
                        return Result.Failed;
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Error", "Something went wrong." +
                        "Detail infomation:\n" +
                        "命令出错，详细信息：\n" + e.Message);
                    return Result.Failed;
                }
                
                ///3-Confirm selection with user.
                td.CommonButtons = TaskDialogCommonButtons.Yes
                    | TaskDialogCommonButtons.Retry
                    | TaskDialogCommonButtons.Cancel;
                td.MainInstruction = "Rooms selected 选中房间:\n" +
                    roomNames + "continue?是否继续?";
                td.DefaultButton = TaskDialogResult.Cancel;
                tdR = td.Show();
                ///Choose result:
                ///Yes-Confirm the selection,break the loop.
                ///Cancel-Stop the command.
                ///Retry-Restart the selection process.
                if (tdR == TaskDialogResult.Yes)
                    break;
                if (tdR == TaskDialogResult.Cancel) 
                    return Result.Failed;
            } while (tdR == TaskDialogResult.Retry);
            #endregion

            List<RoomInfoElec> roomInfoList 
                = new List<RoomInfoElec>();
            List<SystemInfoElec> systemInfoList 
                = new List<SystemInfoElec>();
            ///Step2-Initialize the roominfos and systeminfos.
            ///
            ///RoomInfoElec is a custom built type to pre calculate
            ///and then store the geometry and node infomation 
            ///of each room and contain function relative to 
            ///electrical route calculation.
            ///
            ///SystemInfoElec is a cusstom built type to store
            ///the node and route infomation of an electrical circuit.
            ///
            ///This part initialize those info based on rooms selected.
            /// 
            ///The result is stored in the above two list.
            #region Code block of step-2

            ///1-Ask the user for structural levels.
            ///
            ///This infomation is not able to acquire directly from
            ///the Room type.
            ///The program assume all room share same structural
            ///floor and ceiling level.
            ///
            double structCeilingH = 0;
            double structFloorH = 0;
            double interiorCeilingH = 0;
            ///Using a form to display all the reference levels 
            ///for user to choose from.
            using (CmdAutoRouteEform input = new CmdAutoRouteEform())
            {
                ///Filter out all the levels.
                FilteredElementCollector levels =
                    new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_Levels);
                ///Acquire level names and height.
                string[] levelNames =
                    levels.Select(l => l.Name).ToArray();
                double[] levelH = 
                    levels.Select(l => l as Level)
                          .Select(l => l.ProjectElevation)
                          .ToArray();
                ///Display the levels on the form.
                input.InputLevels(levelNames);
                DialogResult dr = input.ShowDialog();
                ///Result OK-Log the height choosen.
                ///Result Cancel or other-Cancel the command.
                if (dr == DialogResult.OK)
                {
                    structCeilingH = levelH
                        [input.listCeilingLevel.SelectedIndex];
                    structFloorH = levelH
                        [input.listFloorLevel.SelectedIndex];
                    interiorCeilingH = levelH
                        [input.listInteriorCeilingLevels.SelectedIndex];
                }  
                else return Result.Cancelled;
            }

            ///2-Create roomInfo object for each room selected.
            foreach (Room r in roomSelected)
            {///The constructor of RoomInfoElec will do the job automaticly.
                roomInfoList.Add(new RoomInfoElec
                    (r,structCeilingH,structFloorH,interiorCeilingH));
            }

            ///3-Create systemInfo object for eacy electrical circuit.
            ///  Add the fixtures info to roomInfo and systemInfo object.
            try
            {
                ///Filter out all the electrical circuits.
                FilteredElementCollector col =
                new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_ElectricalCircuit);
                ///Create systemInfo object for each circuit.
                foreach (Element elecCir in col)
                {
                    ///Transfer the result to electricalsystem type.
                    ElectricalSystem elecSys
                        = elecCir as ElectricalSystem;
                    ///The constructor of SystemInfoElec will do the 
                    ///basic automaticly.
                    SystemInfoElec systemInfo
                        = new SystemInfoElec(elecSys);
                    ///Add each normal fixtures in this system.
                    foreach (Element f in elecSys.Elements)
                    {
                        ///If the fixture don't have room which it 
                        ///belong to,abandon it.
                        FamilyInstance fixture = f as FamilyInstance;
                        if (fixture.Room == null) continue;
                        ///Find the roomInfo object for the room.
                        var roomInfoE = roomInfoList
                            .Where(r => r.Room.Id == fixture.Room.Id)
                            .First();
                        if (roomInfoE == null)
                            continue;
                        ///Create the fixture object.
                        ///FixtureE is a custom built type that will
                        ///be used in the calculation later on.
                        FixtureE fE = new FixtureE(fixture);
                        ///The accessnode of a fixture is where it will
                        ///access the circuit.Refer to the object for details.
                        fE.SetAccessNode(roomInfoE);
                        ///Add this fixture to both roominfo and systeminfo.
                        roomInfoE.AddFixture(fE, systemInfo);
                        systemInfo.AddFixture(fE, roomInfoE);
                    }
                    ///Add electrical panel as another fixture.
                    Room room = systemInfo.BaseEquipment.Room;
                    RoomInfoElec rInfo = roomInfoList
                        .Where(r => r.Room.Name == room.Name)
                        .FirstOrDefault()
                        as RoomInfoElec;
                    systemInfo.AddFixture
                        (systemInfo.BaseEquipment, rInfo);
                    rInfo.AddFixture
                        (systemInfo.BaseEquipment, systemInfo);
                    
                    ///Finish adding fixture and enlist the object
                    systemInfoList.Add(systemInfo);
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return Result.Failed;
            }
            #endregion

            ///Step3-Calculate the place to cross the wall.
            ///
            /// In order to avoid some unnecessary calculation,
            /// this command calculate the room topology first
            /// and find the place for electrical wires to cross
            /// the wall.
            /// 
            /// The crossing point at eachside of a wall will be
            /// added as a special fixtures to the system and room.
            /// 
            /// And they will be connected in the systemGraph.
            /// While other fixtures will only be allowed to connect 
            /// with the fixtures in the same room in the following
            /// systemGraph.
            ///
            #region Code block of step3
            ///1-Initialize the vertices and the roomgraph.
            List<Vertex> vRooms = new List<Vertex>();
            foreach (RoomInfoElec r in roomInfoList)
            {
                vRooms.Add(new Vertex(r));
            }
            Graph roomGraph = new Graph(vRooms);
            int vNum = roomGraph.VertexCount;

            ///2-Calculate the fixtures centroid of each room.
            foreach (RoomInfoElec r in roomInfoList)
            {
                r.CalculateFixCentroid();
            }
            ///Find the room where the panel are located.
            ///And change the centroid of this room to panel location.
            Room panelRoom = systemInfoList[0].BaseEquipment.Room;
            Vertex panelVertex = roomGraph.Vertices
                .Where(v => v.Object.ToString() == panelRoom.Name)
                .ToArray().First() as Vertex;
            RoomInfoElec panelRoomInfo = panelVertex.Object 
                as RoomInfoElec;
            panelRoomInfo.AssignFixCentroid
                (systemInfoList[0].BaseEquipment.OriginNode);
            ///3-Initialize the edges for roomGraph.
            for (int i = 0; i < vNum; i++)
            {
                for (int j = i + 1; j < vNum; j++)
                {
                    ///Find the vertex and object first.
                    Vertex v1 = roomGraph.Vertices.ElementAt(i);
                    Vertex v2 = roomGraph.Vertices.ElementAt(j);
                    RoomInfoElec r1 = v1.Object as RoomInfoElec;
                    RoomInfoElec r2 = v2.Object as RoomInfoElec;
                    ///If this two room is adjacent and have a 
                    ///path to cross the wall,create the edge in 
                    ///the roomGraph.
                    if (r1.AdjacentPathTo
                        (r2,r2.StructCeilingLevel
                        ,out PathExWall path,out double roughL))
                    {
                        Edge e = new Edge(v1, v2, path, roughL);
                        roomGraph.AddEdge(e);
                    }
                }
            }
            ///Using the DijkstraTree algorithm to calculate the 
            ///crosswall path tree in which each room can have a 
            ///shortest path to the panel.
            Edge[] mstRoomDij = roomGraph.DijkstraTree(panelVertex);
            #endregion

            ///Step4-Create systemGraph and add cross wall point.
            ///
            ///The systemGraph contains all the fixtures of one system
            ///as vertex in the graph.And the routes between fixtures
            ///are stored as edges in the graph.
            ///
            ///Later will use mininum span tree to decided which route
            ///to use and which to abandon.
            List<Graph> sysGraphList = new List<Graph>();
            List<PathExWall> crossWallPaths = new List<PathExWall>();
            #region Code block of step4
            ///1-Correct all the direction of crosspoint.
            ///Make them start from panelroom to the other room.
            foreach (Vertex v in vRooms)
            {
                ///Skip the root room.
                if (v == v.Parent) continue;
                ///Find the path from this room to its parent.
                Edge e = roomGraph.FindEdge(v, v.Parent);
                PathExWall p = e.Object as PathExWall;
                ///If direction is wront,reverse it.
                if (e.Begin != v.Parent)
                {
                    e.Reverse();
                    p.Reverse();
                }
                ///Acquire the path for further use.
                crossWallPaths.Add(p);
            }

            ///2-Add vertex for each system the graph.
            foreach (SystemInfoElec sys in systemInfoList)
            {
                ///sysGraph is the graph of fixtures for this system.
                Graph sysGraph = new Graph();
                ///sysRooms contains all the rooms in which there are
                ///fixtures of this system.
                List<RoomInfoElec> sysRooms
                    = sys.ElecFixturesDic.Keys.ToList();
                ///sysRoomsV are all the roomVertex that system
                ///will need to path through.
                ///It may not be all rooms but may also be
                ///more than sysRooms.(Non adjacent rooms need to
                ///use other room to connect)
                List<Vertex> sysRoomsV = roomGraph.UpTrace
                    (vRooms.Where(v => sysRooms
                    .Contains(v.Object as RoomInfoElec)));

                ///First add all the normal fixture as vertex.
                foreach (FixtureE f in sys.ElecFixtures)
                {
                    sysGraph.AddVertex(new Vertex(f));
                }
                ///Second add the crossing paths.
                ///(Both vertex and edge)
                foreach (Vertex v in sysRoomsV)
                {
                    ///Skip the root room.
                    if (v.Parent == v) continue;
                    ///Find the rooms.
                    ///toR is the one further to the root(panel).
                    RoomInfoElec toR = v.Object 
                                       as RoomInfoElec;
                    RoomInfoElec fromR = v.Parent.Object 
                                         as RoomInfoElec;
                    ///Find the edge that connect two vertices.
                    Edge e = roomGraph.FindEdge(v, v.Parent);
                    PathExWall p = e.Object as PathExWall;
                    ///Add the crossing point in eachroom
                    ///as a fixture to roomInfo object.
                    fromR.AddFixture(p.CurrentBegin, sys);
                    toR.AddFixture(p.CurrentEnd, sys);
                    ///Add them also to the systemInfo.
                    sys.AddFixture(p.CurrentBegin, fromR);
                    sys.AddFixture(p.CurrentEnd, toR);
                    ///Add crossing point as vertex to systemGraph.
                    ///And the path connect them as edge.
                    Vertex v1 = new Vertex(p.CurrentBegin);
                    Vertex v2 = new Vertex(p.CurrentEnd);
                    Edge enew = new Edge
                        (v1, v2, p.CloneCurrent(), p.Cost);
                    sysGraph.AddVertex(v1);
                    sysGraph.AddVertex(v2);
                    sysGraph.AddEdge(enew);
                }
                ///Generate next crossing path for next system.
                ///Including the paths which this system didnt use.
                foreach (PathExWall p in crossWallPaths)
                    p.MoveNext();

                ///Add sysGraph to list.
                sysGraphList.Add(sysGraph);
            }
            #endregion

            ///Step5-Calculate the final route.
            ///
            ///Use the mininum span tree to calculate the 
            ///final route combination.
            #region Code block of step5.
            ///1-Add all the route inside rooms to the graph.
            int sysIndex = 0;
            foreach (Graph sysGraph in sysGraphList)
            {
                foreach (RoomInfoElec r in roomInfoList)
                {
                    ///Get all the fixtures inside this room.
                    List<Vertex> fixs = sysGraph.Vertices
                        .Where(v => (v.Object as FixtureE)
                                    .Room.Name == r.Room.Name)
                        .ToList();
                    int fixsNum = fixs.Count();
                    ///Calculate path for each fixture pair.
                    ///And add it to the graph
                    for (int i = 0; i < fixsNum; i++)
                    {
                        for (int j = i + 1; j < fixsNum; j++)
                        {
                            PathE p = r.FindPath
                                (fixs[i].Object as FixtureE, 
                                fixs[j].Object as FixtureE);
                            sysGraph.AddEdge
                                (new Edge(fixs[i], fixs[j], 
                                          p, p.Cost));
                        }
                    }
                }
                ///2-Using KruskalMinTree to generate the sysPaths
                Edge[] sysPaths = sysGraph.KruskalMinTree();

                ///3-Store the path to systemInfo.
                ///Find the corresponding systemInfo object.
                SystemInfoElec eSys = systemInfoList[sysIndex];
                ///Select the lines in the paths and add them
                ///as wires to the systemInfo object.
                eSys.AddWires(sysPaths
                    .Select(e => e.Object as PathE)
                    .Select(p => p.Lines)
                    .SelectMany(c => c)
                    .ToList());
                ///Increment the index
                sysIndex++;
            }
            #endregion

            ///Step6-Create the line
            ///
            ///Open a transaction and create the line as modelcurve.
            ///Each system will have a seperate linestyle with 
            ///different color to seperate.
            ///And all the modelcurves in one system will be grouped.
            #region Code block of step6
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create Wires");
                ///1-Prepare new linestyle color.
                Categories cats = doc.Settings.Categories;
                Category lineCat = cats
                    .get_Item(BuiltInCategory.OST_Lines);
                CategoryNameMap lineCatsNames = lineCat.SubCategories;
                

                int sysNum = systemInfoList.Count();
                List<Color> colors = Helper.ColorPallet(sysNum);
                short counter = 1;
                foreach (SystemInfoElec eSys in systemInfoList)
                {
                    Category newLineStyleCat = null;
                    Element newLineStyle = null;
                    ///2-Create the new linestyle for this system.
                    ///If having naming conflict,result will be null.
                    ///Then redo the loop.
                    try
                    {
                        newLineStyleCat = cats.NewSubcategory
                            (lineCat, "ElectricalLine-" + counter);
                    }
                    catch
                    {
                        newLineStyleCat = lineCatsNames.get_Item("ElectricalLine-" + counter);
                    }
                    newLineStyleCat.SetLineWeight
                        (7, GraphicsStyleType.Projection);
                    newLineStyleCat.LineColor = colors[counter - 1];
                    newLineStyle =
                        doc.GetElement(newLineStyleCat.Id);
                    
                    ///3-Create the model line and set linestyle.
                    XYZ x0 = new XYZ();
                    List<ElementId> modelC = new List<ElementId>();
                    ///If no wires is generated,skip.
                    if (eSys.Wires.Count == 0) continue;
                    foreach (Curve c in eSys.Wires)
                    {
                        XYZ x1 = c.GetEndPoint(0);
                        XYZ x2 = c.GetEndPoint(1);
                        SketchPlane sp = SketchPlane.Create
                            (doc,Plane.CreateByThreePoints(x1, x2, x0));
                        ModelCurve mc = doc.Create.NewModelCurve(c, sp);
                        mc.LineStyle = newLineStyle;
                        modelC.Add(mc.Id);
                    }
                    ///4-Group all the curves.
                    doc.Create.NewGroup(modelC);
                    counter++;
                }
                tx.Commit();
            }
            #endregion

            return Result.Succeeded;
        }
    }
}
