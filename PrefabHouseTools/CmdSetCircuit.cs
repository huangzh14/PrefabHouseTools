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

namespace PrefabHouseTools
{
    [Transaction(TransactionMode.Manual)]
    public class CmdSetCircuit : IExternalCommand
    {
        private bool HasElectricalConnector(Element e)
        {
            if (!(e is FamilyInstance)) return false;
            FamilyInstance fa = e as FamilyInstance;
            if (fa.MEPModel.ConnectorManager == null) return false;
            ConnectorSet cs = fa.MEPModel.ConnectorManager.Connectors;
            foreach (Connector c in cs)
            {
                try
                {
                    if ((int)c.ElectricalSystemType >= 0)
                        return true;
                }
                catch
                {
                    continue;
                }
            }
            return false;
        }
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            TaskDialogResult r = TaskDialog.Show("Note", 
                "This will clear the existing " +
                "electrical system.Continue?",
                TaskDialogCommonButtons.Yes |
                TaskDialogCommonButtons.No);
            if (r == TaskDialogResult.No) return Result.Failed;
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Clear existing system.");
                FilteredElementCollector col =
                    new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_ElectricalCircuit);
                Stack<ElementId> sysId = new Stack<ElementId>();
                foreach (Element e in col)
                {
                    sysId.Push(e.Id);
                }
                while(sysId.Count > 0)
                {
                    doc.Delete(sysId.Pop());
                }
                tx.Commit();
            }
            # region Check the default settings.
            const string elecSettingName = 
                "DefaultElectricalSettingExcuted";
            GlobalParameter SettingP = doc.GetElement( 
                GlobalParametersManager.FindByName
                (doc, elecSettingName)) as GlobalParameter;
            if (SettingP == null)
            {
                //Set the voltage and distribution to default
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Autoset electrical setting");
                    ElectricalSetting ElecSet = ElectricalSetting
                        .GetElectricalSettings(doc);
                    VoltageType VtypeHome = ElecSet
                        .AddVoltageType("Home", 220, 200, 250);
                    ElecSet.AddDistributionSysType
                        ("Lighting", ElectricalPhase.SinglePhase,
                        ElectricalPhaseConfiguration.Undefined,
                        2, null, VtypeHome);
                    ElecSet.AddDistributionSysType
                        ("Outlet", ElectricalPhase.SinglePhase,
                        ElectricalPhaseConfiguration.Undefined,
                        2, null, VtypeHome);
                    GlobalParameter.Create
                    (doc, elecSettingName, ParameterType.Number);
                    tx.Commit();
                }
            }
            #endregion

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
                if (HasElectricalConnector(e))
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
                ElectricalSystem LightingCircuit =
                    ElectricalSystem.Create(doc, LightIds, 
                    ElectricalSystemType.PowerCircuit);
                LightingCircuit.Name = "LightingCircuit";
                LightingCircuit.SelectPanel(ElecBox);
                ElectricalSystem OutlietCircuit =
                    ElectricalSystem.Create(doc, OutletIds, 
                    ElectricalSystemType.PowerCircuit);
                OutlietCircuit.Name = "OutletCircuit";
                OutlietCircuit.SelectPanel(ElecBox);
                ElectricalSystem HVACCircuit =
                    ElectricalSystem.Create(doc, HVACIds, 
                    ElectricalSystemType.PowerCircuit);
                HVACCircuit.Name = "HVACCircuit";
                HVACCircuit.SelectPanel(ElecBox);
                tx.Commit();
            }
            TaskDialog.Show("Result",
                "Default systems have been created.\n" +
                "Please do not change system name.\n"+
                "已创建默认系统,请勿修改系统名，将影响后续计算\n"+
                "可手动调整系统内末端.");

            return Result.Succeeded;
        }
    }
}
