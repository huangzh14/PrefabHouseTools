#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System.Windows.Forms;
#endregion

namespace PrefabHouseTools
{
    #region Intro
    ///Important note:Here are some basic assumptions.
    ///If the project doesn't meet,unexpect result may occur.
    ///
    ///1-Room boundary are lines not curves.The program use
    ///Curve class for develop convenient,the logic all assume
    ///they are all lines.
    ///
    ///2-The electrical line go above the ceilings.
    ///
    ///3-The room have a common structural ceiling floor height.
    #endregion
    /// <summary>
    /// The filter for selecting rooms.
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
    /// The main command
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CmdAutoRouteE : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.
                Application app = uiapp.Application;
            Document doc = uidoc.Document;

            List<Room> roomSelected = new List<Room>();

            #region Step1-Retrieve rooms from database
            TaskDialog td = new TaskDialog("Start");
            TaskDialogResult tdR;
            string roomNames ;
            ///Start selection.If retry is choose at last,
            ///do the loop again.
            do
            {
                ///First ask choose all rooms or not.
                roomSelected.Clear();
                roomNames = "";
                td.CommonButtons = TaskDialogCommonButtons.Yes
                    | TaskDialogCommonButtons.No;
                td.MainInstruction = "Do you want to calculate " +
                    "electrical route for all room?\n" +
                    "If not,press no and select the rooms to calculate" +
                    " in the model afterward.\n" +
                    "是否为模型中所有房间计算电路路由？\n" +
                    "如果不是，选择no并随后在模型中选择需要计算的房间";
                tdR = td.Show();
                
                ///Filter out the rooms selected.
                try
                {
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
                    else
                    {
                        return Result.Failed;
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Error", "Something went wrong.\n" +
                        "Detail infomation:" + e.Message);
                    return Result.Failed;
                }
                
                ///Confirm selection with user.
                td.CommonButtons = TaskDialogCommonButtons.Yes
                    | TaskDialogCommonButtons.Retry
                    | TaskDialogCommonButtons.Cancel;
                td.MainInstruction = "Rooms selected 选中房间:\n" +
                    roomNames + "continue?是否继续?";
                td.DefaultButton = TaskDialogResult.Cancel;
                tdR = td.Show();
                if (tdR == TaskDialogResult.Cancel) 
                    return Result.Failed;
                if (tdR == TaskDialogResult.Yes)
                    break;
            } while (tdR == TaskDialogResult.Retry);
            #endregion

            List<RoomInfoElec> roomInfoList 
                = new List<RoomInfoElec>();
            List<ElecSystemInfo> systemInfoList 
                = new List<ElecSystemInfo>();
            #region Step2-Initialize the roominfos and systemInfo.

            #region Ask for input for structural levels.
            double structCeilingH = 0;
            double structFloorH = 0;
            using (CmdAutoRouteEform input = new CmdAutoRouteEform())
            {
                FilteredElementCollector levels =
                    new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_Levels);
                List<string> levelNames = new List<string>();
                List<double> levelH = new List<double>();
                foreach (Level l in levels)
                {
                    levelNames.Add(l.Name);
                    levelH.Add(l.ProjectElevation);
                }
                input.InputLevels(levelNames);
                DialogResult dr = input.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    structCeilingH = levelH
                        [input.listCeilingLevel.SelectedIndex];
                    structFloorH = levelH
                        [input.listFloorLevel.SelectedIndex];
                }  
                else return Result.Cancelled;
            }
            #endregion

            //Create roomInfo object.
            foreach (Room r in roomSelected)
            {
                roomInfoList.Add(new RoomInfoElec
                    (r,structCeilingH,structFloorH));
            }
            #region (delete later)Solving adjacency
            int num = roomInfoList.Count;
            for (int i = 0; i < num; i++)
            {
                roomInfoList[i].AdjacentRooms.Clear();
            }
            for (int i = 0; i < num; i++)
            {
                for (int j = i + 1; j < num; j++)
                {
                    if (roomInfoList[i].IsAdjacentTo(roomInfoList[j]))
                    {
                        roomInfoList[i].AdjacentRooms.Add(roomInfoList[j]);
                        roomInfoList[j].AdjacentRooms.Add(roomInfoList[i]);
                    }
                }
            }
            #endregion 

            ///Get all the electrical system and fixture.
            try
            {
                FilteredElementCollector col =
                new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_ElectricalCircuit);
                foreach (Element elecCir in col)
                {
                    ElectricalSystem elecSys
                        = elecCir as ElectricalSystem;
                    ElecSystemInfo systemInfo
                        = new ElecSystemInfo(elecSys);
                    foreach (Element f in elecSys.Elements)
                    {
                        FamilyInstance fixture = f as FamilyInstance;
                        if (fixture.Room == null) continue;
                        var roomInfo = roomInfoList
                            .Where(r => r.Room.Id == fixture.Room.Id)
                            .First();

                        if (roomInfo == null)
                            continue;

                        FixtureE fE = new FixtureE(fixture);
                        roomInfo.AddFixture(fE, systemInfo);
                        systemInfo.AddFixture(fE, roomInfo);
                    }
                    systemInfoList.Add(systemInfo);
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return Result.Failed;
            }
            #endregion

            #region Step3-Calculate cross wall location.
            ///Initialize the vertices and the graph.
            List<Vertex> vRooms = new List<Vertex>();
            foreach (RoomInfoElec r in roomInfoList)
            {
                vRooms.Add(new Vertex(r));
            }
            Graph roomGraph = new Graph(vRooms);
            int vNum = roomGraph.VertexCount;
            ///Calculate the centroid of each room.
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
                (systemInfoList[0].BaseEquipment.Origin);
            ///Initialize the edges.
            for (int i = 0; i < vNum; i++)
            {
                for (int j = i + 1; j < vNum; j++)
                {
                    Vertex v1 = roomGraph.Vertices.ElementAt(i);
                    Vertex v2 = roomGraph.Vertices.ElementAt(j);
                    RoomInfoElec r1 = v1.Object as RoomInfoElec;
                    RoomInfoElec r2 = v2.Object as RoomInfoElec;
                    if (r1.AdjacentPathTo
                        (r2,Math.Min(r1.StructCeilingLevel,r2.StructCeilingLevel)
                        ,out PathXWall path,out double roughL))
                    {
                        Edge e = new Edge(v1, v2, path, roughL);
                        roomGraph.AddEdge(e);
                    }
                }
            }
            ///Using the DijkstraTree algorithm to calculate the 
            ///trunk path.
            Edge[] mstRoomD = roomGraph.DijkstraTree(panelVertex);
            #region _Demo Only
            string result = "";
            foreach (Vertex v in roomGraph.Vertices)
            {
                result += v.Object + " to " 
                    + v.Parent.Object + "\n";
            }
            TaskDialog.Show("Result", result);
            #endregion

            #endregion

            #region Step4-Add cross wall vertex to room.
            List<PathXWall> allPaths = new List<PathXWall>();
            ///Correct all the direction of crosspoint.
            ///Log all the paths to do movenext lateron.
            foreach (Vertex v in vRooms)
            {
                //Skip the root room.
                if (v == v.Parent) continue;
                Edge e = roomGraph.FindEdge(v, v.Parent);
                PathXWall p = e.Object as PathXWall;
                //Make sure all crosspoint are same direction.
                //From centre panel to fixtures.
                if (e.Begin != v.Parent)
                {
                    e.Reverse();
                    p.Reverse();
                }
                allPaths.Add(p);
            }
            ///Add vertex for each system to all the room needed.
            foreach (ElecSystemInfo sys in systemInfoList)
            {
                List<RoomInfoElec> sysRooms
                    = sys.ElecFixturesDic.Keys.ToList();
                List<Vertex> sysRoomsV = roomGraph.UpTrace
                    (vRooms.Where(v => sysRooms
                    .Contains(v.Object as RoomInfoElec)));
                foreach (Vertex v in sysRoomsV)
                {
                    RoomInfoElec toR = v.Object as RoomInfoElec;
                    RoomInfoElec fromR = v.Parent.Object as RoomInfoElec;
                    Edge e = roomGraph.FindEdge(v, v.Parent);
                    PathXWall p = e.Object as PathXWall;
                    fromR.AddFixture(p.CurrentBegin, sys);
                    toR.AddFixture(p.CurrentEnd, sys);
                }
                foreach (PathXWall p in allPaths)
                    p.MoveNext();
            }
            #endregion

            #region Step5-Calculate the final route.
            #endregion

            // Modify document within a transaction
            #region demo only
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Demo");
                
                string adjan = "";
                foreach(RoomInfoElec r in roomInfoList)
                {
                    ///Draw model line for demo.
                    CurveArray curves = r.GetBoundaryCurves();
                    XYZ originpt = curves.get_Item(0)
                                   .GetEndPoint(0);
                    SketchPlane sp = SketchPlane.Create
                        (doc, Plane.CreateByNormalAndOrigin
                        (new XYZ(0, 0, 1), originpt));
                    doc.Create.NewModelCurveArray(curves, sp);
                    ///Display adjancant info for demo.
                    adjan += "\n" + r + " is adjacent to:\n";
                    foreach(RoomInfoElec adjR in r.AdjacentRooms)
                    {
                        adjan += adjR + "\n";
                    }
                }
                TaskDialog.Show("demo", adjan);
                tx.Commit();
            }
            #endregion

            return Result.Succeeded;
        }
    }
}
