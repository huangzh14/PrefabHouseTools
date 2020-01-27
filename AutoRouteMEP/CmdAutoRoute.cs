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
    public class CmdAutoRoute : IExternalCommand
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

            //Get the electrical circuit of the project
            FilteredElementCollector colElecCir
              = new FilteredElementCollector(doc)
              .OfCategory(BuiltInCategory.OST_ElectricalCircuit)
              .WhereElementIsNotElementType();

            foreach (Element e in colElecCir)
            {
                //The system
                ElectricalSystem eElec = e as ElectricalSystem;
                //The fixtures of this terminal
                ElementSet fixtures = eElec.Elements;
                FamilyInstance baseE = eElec.BaseEquipment;
            }

            return Result.Succeeded;
        }
    }
}

