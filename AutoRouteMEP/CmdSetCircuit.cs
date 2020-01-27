#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Electrical;
#endregion

namespace AutoRouteMEP
{
    [Transaction(TransactionMode.Manual)]
    public class CmdSetCircuit : IExternalCommand
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

            #region Retrieve elements from database
            List<ElementId> LightIds = new List<ElementId>();
            List<ElementId> OutletIds = new List<ElementId>();
            List<ElementId> HVACIds = new List<ElementId>();

            FilteredElementCollector colLight
              = new FilteredElementCollector(doc)
              .OfCategory(BuiltInCategory.OST_LightingFixtures)
              .WhereElementIsNotElementType();
            FilteredElementCollector colOutlet
              = new FilteredElementCollector(doc)
              .OfCategory(BuiltInCategory.OST_ElectricalFixtures)
              .WhereElementIsNotElementType();
            FilteredElementCollector colHVAC
              = new FilteredElementCollector(doc)
              .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
              .WhereElementIsNotElementType();

            foreach (Element e in colLight)
            {
                LightIds.Add(e.Id);
            }
            foreach (Element e in colOutlet)
            {
                OutletIds.Add(e.Id);
            }
            foreach (Element e in colHVAC)
            {
                HVACIds.Add(e.Id);
            }

            #endregion Retrieve elements from databa

            //Locate the electrical main box.
            FamilyInstance ElecBox = null;
            try
            {
                Selection sel = uidoc.Selection;
                TaskDialog.Show("Choose", "Please select one electrical box after closing the dialog.\n" +
                    "请在关闭窗口后选择一个配电箱。");
                ElementId ElecBoxId = sel.PickObject(ObjectType.Element, "Select the main box").ElementId;
                ElecBox = doc.GetElement(ElecBoxId) as FamilyInstance;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "Something went wrong.\n" + ex.Message);
                return Result.Failed;
            }

            
            // Create the electrical system
            using (Transaction tx = new Transaction(doc))
            {      
                tx.Start("Create ElectricalSystem");
                ElectricalSystem LightSystem =
                    ElectricalSystem.Create(doc, LightIds, ElectricalSystemType.PowerCircuit);
                LightSystem.SelectPanel(ElecBox);
                ElectricalSystem OutletSystem =
                    ElectricalSystem.Create(doc, OutletIds, ElectricalSystemType.PowerCircuit);
                OutletSystem.SelectPanel(ElecBox);
                ElectricalSystem HVACSystem =
                    ElectricalSystem.Create(doc, HVACIds, ElectricalSystemType.PowerCircuit);
                HVACSystem.SelectPanel(ElecBox);
                tx.Commit();
            }
            TaskDialog.Show("Result", "Default systems have been created.\n" +
                "已创建默认系统");

            return Result.Succeeded;
        }
    }
}
