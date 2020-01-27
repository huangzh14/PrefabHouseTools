using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Electrical;

namespace AutoRouteMEP
{
    class CmdSetElectricalSettings :IExternalCommand
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
            ///Create the default settings for electrical system.
            ///
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Autoset electrical setting");
                ElectricalSetting ElecSet = ElectricalSetting
                    .GetElectricalSettings(doc);
                VoltageType VtypeHome = ElecSet
                    .AddVoltageType("Home", 220, 200, 240);
                ElecSet.AddDistributionSysType
                    ("Lighting", ElectricalPhase.SinglePhase, 
                    ElectricalPhaseConfiguration.Undefined, 
                    2, null, VtypeHome);
                ElecSet.AddDistributionSysType
                    ("Outlet", ElectricalPhase.SinglePhase,
                    ElectricalPhaseConfiguration.Undefined,
                    2, null, VtypeHome);
                tx.Commit();
            }
            return Result.Succeeded;
        }

    }
}
