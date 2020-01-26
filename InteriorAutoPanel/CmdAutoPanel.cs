#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.IFC;
using System.Linq;
using System.Windows.Forms;
#endregion

namespace InteriorAutoPanel
{
    public class PtEqualComparer : IEqualityComparer<XYZ>
    {
        public bool Equals(XYZ x, XYZ y)
        {
            if ((Math.Abs(x.X - y.X)<0.001)&&(Math.Abs( x.Y - y.Y)<0.001))
            {return true;}
            return false;
        }

        public int GetHashCode(XYZ obj)
        {
            return 0;
        }
    }

    public class PtComparer : IComparer<XYZ>
    {
        public int Compare(XYZ x, XYZ y)
        {
            if (x.X - y.X > 0.000001)
                return 1;
            else if (x.X - y.X < 0.000001)
                return -1;
            else if (x.Y - y.Y > 0.000001)
                return 1;
            else if (x.Y - y.Y < 0.000001)
                return -1;
            else
                return 0;
        }
    }
    ///Use proximity not equal.Errors may mount up causing points having tiny different.
    ///And those tinny difference may end up in == not working.

    [Transaction(TransactionMode.Manual)]
    public class CmdAutoPanel : IExternalCommand
    {
        //The helper method
        public static Element FindElementByName
            (Document doc, Type targetType, string targetName)
        {
            return new FilteredElementCollector(doc)
                .OfClass(targetType).FirstOrDefault<Element>(
                currentRoom => currentRoom.Name.Equals(targetName));
        }

        //define the family name and path(need to be changed later)
        public const string panelFamilyName = "PanelAuto";
        const string family_ext = "rfa";
        string family_folder = "";
        string family_path = null;
        string FamilyPath
        {
            get
            {
                if(null == family_path)
                {
                    family_path = Path.Combine(family_folder, panelFamilyName);
                    family_path = Path.ChangeExtension(family_path, family_ext);
                }
                return family_path;
            }
        }


        //The main method to excute.
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            #region Build the basic and filter the selection
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // Access current selection
            Selection sel = uidoc.Selection;
            FilteredElementCollector col;
            // Retrieve elements from database£¨only choose room)
            try
            {
                col= new FilteredElementCollector(doc, sel.GetElementIds())
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Rooms);
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException ae)
            {
                TaskDialog.Show("Warning", "Please select at least one room.\n"
                    +"Detail:"+ae.Message);
                return Result.Failed;
            }
            
            #endregion

            #region Confirm selection.result in tdResult.
            // Write all the roomname into a string to confirm with the user.
            if (col.Count() == 0)
            {
                TaskDialog.Show("Warning", "No room is selected." +
                    "This commend operates based on room.");
                return Result.Failed;
            }

            string confirmMessage = "The following rooms are choosen, confirm?\n";
            foreach (Element currentRoom in col){
                Room roome = currentRoom as Room;
                confirmMessage = confirmMessage + roome.Name +"\n";}

            //Pop up a dialog to confirm selection.
            TaskDialogResult tdResult = 
                TaskDialog.Show("Revit", confirmMessage, 
                TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
            if (tdResult == TaskDialogResult.No)
            {
                return Result.Failed;
            }
            #endregion

            #region Load the family(into "panelRfa""basePanelSymbol")
            //Set directory for the family
            family_folder = Path.GetDirectoryName(doc.PathName);
            //check if the family already exist in the project
            Family panelRfa = FindElementByName
                (doc, typeof(Family), panelFamilyName) as Family;
            if (panelRfa == null){
                //check for the file to load.
                if (!File.Exists(FamilyPath)){
                    TaskDialog.Show("Waning", 
                        "Please ensure the PanelAuto.rfa is " +
                        "in the same folder with current project.");
                    return Result.Failed;}
                using (Transaction tx = new Transaction(doc)){
                    tx.Start("Load PanelAuto.Rfa");
                    doc.LoadFamily(FamilyPath, out panelRfa);
                    tx.Commit();}
            }
            ISet<ElementId> typeId = panelRfa.GetFamilySymbolIds();
            FamilySymbol basePanelSymbol = doc
                .GetElement(typeId.FirstOrDefault()) as FamilySymbol;
            #endregion

            #region Ask for custom input
            double DistanceToWall = 0;
            double UnitWidth = 0;
            //Show a dialog to ask input from user.
            using (InputForm input = new InputForm()){
                if (!(input.ShowDialog() == DialogResult.OK))
                {
                    return Result.Failed;
                }
                DistanceToWall = UnitUtils.ConvertToInternalUnits
                    ((double)input.DistanceToWall.Value,DisplayUnitType.DUT_MILLIMETERS);
                UnitWidth = UnitUtils.ConvertToInternalUnits
                    ((double)input.UnitWidth.Value,DisplayUnitType.DUT_MILLIMETERS);
            }
            #endregion

            #region The main work.
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create the panels");
                //Store all the boundary segments of a room.
                List<EdgeSegment> roomSegments = new List<EdgeSegment>();
                //Store all the Id of the panels created.
                List<ElementId> PanelIds = new List<ElementId>();

                //Loop through each room
                foreach (Element currentRoom in col)
                {
                    //Clear the data for the new room.
                    roomSegments.Clear();
                    PanelIds.Clear();

                    //Step1-Get the raw data of each wall.
                    roomSegments = GetRawData(currentRoom as Room,DistanceToWall,UnitWidth);
                    
                    foreach (EdgeSegment eSegment in roomSegments)
                    {
                        //Step2-Create ranges.In one range panels have same height 
                        //and are horizontally spread as evenly as possible
                        if (!eSegment.NoPanel)
                            eSegment.DivideToPanelRange();
                        //Step3-Create construction daga for each panel.
                        foreach (PanelRange pR in eSegment.ranges) {
                            eSegment.panels.AddRange
                                (pR.CreatePanelDatas(UnitWidth, DistanceToWall));}

                        #region for demostration(while working)
                        //XYZ normal = wall.normal;
                        //XYZ origin = wall.start;
                        //Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
                        //SketchPlane sP = SketchPlane.Create(doc, plane);
                        //foreach (PanelRange pRange in wall.ranges)
                        //{
                        //    doc.Create.NewModelCurveArray(pRange.GetRangeCurves(), sP);
                        //}
                        #endregion

                        //Step 4-Create the panel instances.
                        foreach (PanelData pData in eSegment.panels){
                            #region Only for demo while working
                            /*
                            Plane tempP = Plane.CreateByNormalAndOrigin(pData.PaNormal, pData.PaOrigin);
                            SketchPlane sP = SketchPlane.Create(doc, tempP);
                            XYZ T = new XYZ(-pData.PaNormal.Y, pData.PaNormal.X, 0);
                            XYZ dl = pData.PaOrigin + 0.5 * pData.PaWidth * T;
                            XYZ dr = pData.PaOrigin - 0.5 * pData.PaWidth * T;
                            Transform up = Transform.CreateTranslation(new XYZ(0, 0, pData.PaHeight));
                            XYZ ul = up.OfPoint(dl);
                            XYZ ur = up.OfPoint(dr);
                            Line l1 = Line.CreateBound(dl, ul);
                            Line l2 = Line.CreateBound(dr, ur);
                            doc.Create.NewModelCurve(l1, sP);
                            doc.Create.NewModelCurve(l2, sP);
                            */
                            #endregion
                            
                            #region Create the panels

                            //Convert the panelWidth and Height into mm
                            //And create the name for the panel.
                            int panelW_mm = (int)Math.Round(UnitUtils.ConvertFromInternalUnits
                                (pData.PaWidth, DisplayUnitType.DUT_MILLIMETERS));
                            int panelH_mm = (int)Math.Round(UnitUtils.ConvertFromInternalUnits
                                (pData.PaHeight, DisplayUnitType.DUT_MILLIMETERS));
                            string panelName = "P" + panelW_mm + "-" + panelH_mm;

                            //Check if this type already exist
                            FamilySymbol currentPanelSymbol = FindElementByName
                                (doc, typeof(FamilySymbol), panelName) as FamilySymbol;
                            if (currentPanelSymbol == null)
                            {
                                //Create a new one
                                currentPanelSymbol = basePanelSymbol.Duplicate(panelName) as FamilySymbol;
                                //modify the size of new symbol
                                currentPanelSymbol.GetParameters("Height")[0].Set(pData.PaHeight);
                                currentPanelSymbol.GetParameters("Width")[0].Set(pData.PaWidth);
                            }

                            //Create the actual instance
                            FamilyInstance currentInst = doc.Create.NewFamilyInstance
                                (pData.PaOrigin, currentPanelSymbol, StructuralType.NonStructural);
                            //Rotate the instance to correct direction
                            Line axis = Line.CreateBound(pData.PaOrigin,pData.PaOrigin+XYZ.BasisZ);
                            //The AngleOnPlaneTo will create the angle from 0 to 2Pi
                            double angle = new XYZ(0, -1, 0).AngleOnPlaneTo(pData.PaNormal,new XYZ(0,0,1));
                            ElementTransformUtils.RotateElement(doc, currentInst.Id, axis, angle);
                            //log the panel id
                            PanelIds.Add(currentInst.Id);
                            #endregion

                        }
                    }
                    //Group the panels of the same room.
                    Group group = doc.Create.NewGroup(PanelIds);
                }
                tx.Commit();
            }
            #endregion

            return Result.Succeeded;
        }
        
        public List<EdgeSegment> GetRawData(Room room,
            double DistanceToWall,double UnitWidth)
        {
            //Used to store the wall object.
            List<EdgeSegment> rSegments = new List<EdgeSegment>();

            #region The basics and datas
            const double er = 0.0001;//used for error correction.
            Document doc = room.Document;
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            //get the level height of floor and ceiling
            //!!!!!current not consider different ceiling height or floor.Need improving.
            double levelC = room.ClosedShell.GetBoundingBox().Max.Z;
            double levelF = room.ClosedShell.GetBoundingBox().Min.Z;
            //Custom input data.
            double dist2W = DistanceToWall;
            double unitW = UnitWidth;
            #endregion

            //get the boundary segments list
            IList<IList<BoundarySegment>> boundarys =
                room.GetBoundarySegments(new SpatialElementBoundaryOptions());

            //Loop through all the wall segment needed to oprate on
            //first layer is each boundary loop(in case a room has multiple boundaries)
            foreach (IList<BoundarySegment> groupSegments in boundarys)
            {
                //The stack is for the segments(one wall can be several segments)
                //The queue is for the wall after merging the segments.
                Stack<EdgeSegment> eSegments = new Stack<EdgeSegment>();
                List<EdgeSegment> eSegmentsMerged = new List<EdgeSegment>();
                
                //Second layer is the actual segment
                //in this loop process each segment opening info and log them
                foreach (BoundarySegment segment in groupSegments)
                {
                    #region Create the object,get the inner edge of a wall segment
                    EdgeSegment theSeg = new EdgeSegment();
                    Curve segCrv = segment.GetCurve();
                    XYZ start = segCrv.GetEndPoint(0);
                    XYZ end = segCrv.GetEndPoint(1);
                    XYZ normal;
                    theSeg.levelC = levelC;
                    theSeg.levelF = levelF;
                    theSeg.start = new XYZ(start.X, start.Y, levelF);
                    theSeg.end = new XYZ(end.X, end.Y, levelF);
                    #endregion

                    //The boundary segment may not be a wall
                    Element wallOrNot = doc.GetElement(segment.ElementId);

                    #region Seperate different category(Wall,Column or others)
                    Categories docCat = doc.Settings.Categories;
                    ElementId idColumn = docCat.get_Item(BuiltInCategory.OST_Columns).Id;
                    ElementId idWall = docCat.get_Item(BuiltInCategory.OST_Walls).Id;
                    //Case 1-A column or the end of a single wall.
                    if ((wallOrNot == null)||(wallOrNot.Category.Id.Equals(idColumn)))
                    {
                        //The room segments always search counterclockwise
                        //Thus compute the normal as follow
                        //Other data is not needed.Treat it as a full wall.
                        XYZ line = end - start;
                        normal = new XYZ(-line.Y, line.X, 0);
                        theSeg.normal = normal / normal.GetLength();
                        eSegments.Push(theSeg);
                        continue;
                    }
                    //Case 2-Not column or wall.(Most likely curtain)Mark as noPanel.
                    if (!(wallOrNot.Category.Id.Equals(idWall)))
                    {
                        theSeg.NoPanel = true;
                        eSegments.Push(theSeg);
                        continue;
                    }
                    //Case 3-Walls
                    Wall theWall = wallOrNot as Wall;
                    #endregion

                    #region Get the sideface of a wall and get the profile curves
                    IList<Reference> sideFaces = 
                        HostObjectUtils.GetSideFaces(theWall, ShellLayerType.Exterior);
                    //get the real face(why so long???)
                    Face face = doc.GetElement(sideFaces[0])
                        .GetGeometryObjectFromReference(sideFaces[0]) as Face;
                    //get edge loops as curveloops
                    IList<CurveLoop> openLoops = face.GetEdgesAsCurveLoops();
                    #endregion

                    //Some basic properties of the face.
                    normal = face.ComputeNormal(new UV(0, 0));
                    Plane plane = Plane.CreateByNormalAndOrigin(normal, start);

                    #region Check if curves are on the inner plane or not.
                    //(Might be on the other side of the wall)
                    //Log the correction vector in correction.
                    Curve checkCurve = openLoops[0].GetEnumerator().Current;
                    plane.Project(checkCurve.GetEndPoint(1), out UV uv, out double dist);
                    Transform correction = Transform.CreateTranslation(new XYZ(0,0,0));
                    if (dist > er )//Same wired reason.See "const double er" up front.
                    {
                        normal = -normal;
                        correction = Transform.CreateTranslation(dist * normal);
                        plane = Plane.CreateByNormalAndOrigin(normal, start);
                    }
                    #endregion

                    //Store all the endpoints and horizontal curves.
                    List<XYZ> points = new List<XYZ>();
                    List<Curve> hoCrvs = new List<Curve>();
                    foreach (CurveLoop curveloop in openLoops)
                    {
                        foreach (Curve curve in curveloop)
                        {
                            points.Add(correction.OfPoint(curve.GetEndPoint(0)));
                            //If curve is horizontal,and in the middle range,log it
                            double cSz = curve.GetEndPoint(0).Z;
                            double cEz = curve.GetEndPoint(1).Z;
                            if ((Math.Abs( cSz-cEz)<er)&&(cSz> levelF+er)&&(cSz < levelC-er)){
                                hoCrvs.Add(curve.CreateTransformed(correction));}
                        }
                    }

                    #region Sort pts according to curve direction 
                    var tempPts = from point in points
                                  where ((point.Z > levelF + er) && (point.Z < levelC - er))
                                  select new XYZ(point.X, point.Y, levelF);
                    List<XYZ> relayPts = tempPts.ToList<XYZ>();
                    relayPts.Add(start); relayPts.Add(end);
                    var sortPt = from point in relayPts
                                 where (segCrv.Distance(point) < er)
                                 orderby (point - start).GetLength()
                                 select point;
                    List<XYZ> sortPtList = sortPt
                        .Distinct(new PtEqualComparer())
                        .ToList<XYZ>();
                    if (!(sortPtList[0] == start))
                    {
                        sortPtList.Reverse();
                    }
                    sortPtList.RemoveAt(sortPtList.Count - 1);
                    sortPtList.RemoveAt(0);
                    #endregion

                    #region log the data
                    theSeg.crvList = hoCrvs;
                    theSeg.ptList = sortPtList;
                    theSeg.normal = normal;
                    #endregion

                    //Find insert element(and project on plane)
                    List<ElementId> insertIds = theWall
                        .FindInserts(true,true,true,true).Distinct().ToList();
                    foreach (ElementId eId in insertIds)
                    {
                        Element currentRoom = doc.GetElement(eId);
                        BoundingBoxXYZ box = currentRoom.get_Geometry(new Options()).GetBoundingBox();
                        XYZ ept = 0.5 * (box.Max + box.Min);
                        XYZ v = ept - plane.Origin;
                        double signedDist = plane.Normal.DotProduct(v);
                        XYZ eptnew = ept - signedDist * normal;
                        theSeg.insertPts.Add(eptnew);
                    }

                    //Push to the stack.
                    eSegments.Push(theSeg);
                }

                #region Merge from segments to walls...
                //Merge the segments that are on same curve.
                int segNum = eSegments.Count;
                EdgeSegment sNew;
                for (int i = 1; i < segNum; i++){
                    EdgeSegment s1 = eSegments.Pop();
                    EdgeSegment s2 = eSegments.Pop();
                    if (!(s2.MergeWith(s1,out sNew)))
                        eSegmentsMerged.Add(s1);
                    eSegments.Push(sNew);}

                //Compare the final one to the first one again
                EdgeSegment sf1 = eSegmentsMerged[0];
                EdgeSegment sf2 = eSegments.Pop();
                if (sf2.MergeWith(sf1, out sNew))
                    eSegmentsMerged.RemoveAt(0);
                eSegmentsMerged.Add(sNew);
                //Because the use of stack,the order need to be reversed back.
                eSegmentsMerged.Reverse();
                #endregion

                #region Set the offset for each corner according to dist2W
                int wallNum = eSegmentsMerged.Count;
                eSegmentsMerged.Add(eSegmentsMerged[0]);
                for (int i = 0; i < wallNum; i++)
                {
                    EdgeSegment SegCur = eSegmentsMerged[i];
                    EdgeSegment SegAft = eSegmentsMerged[i + 1];
                    double angle = SegCur.unitV.AngleOnPlaneTo(SegAft.unitV, XYZ.BasisZ);
                    SegCur.end -= Math.Tan(0.5 * angle) * dist2W * SegCur.unitV;
                    SegAft.start += Math.Tan(0.5 * angle) *
                        (dist2W + UnitUtils.ConvertToInternalUnits(10,DisplayUnitType.DUT_MILLIMETERS) )*
                        SegAft.unitV;
                }
                eSegmentsMerged.RemoveAt(eSegmentsMerged.Count-1);
                #endregion

                rSegments.AddRange(eSegmentsMerged);
            }

            return rSegments;
        }
    }
}
