#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace PrefabHouseTools
{
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
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            List<Room> roomSelected = new List<Room>();
            #region Retrieve rooms from database
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
                        FilteredElementCollector col = 
                            new FilteredElementCollector(doc)
                            .WhereElementIsNotElementType()
                            .OfCategory(BuiltInCategory.OST_Rooms);
                        foreach (Element e in col)
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

            #region Initialize the roominfos.
            List<RoomInfoElec> roomInfoList = new List<RoomInfoElec>();
            foreach(Room r in roomSelected)
            {
                roomInfoList.Add(new RoomInfoElec(r));
            }
            RoomInfoElec.SolveAdjacency(roomInfoList);
            #endregion
            // Modify document within a transaction

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

            return Result.Succeeded;
        }
    }
}
