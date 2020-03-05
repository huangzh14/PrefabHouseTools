#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.IO;
using System.Windows.Forms;
#endregion

namespace TransferData_XJ
{
    [Transaction(TransactionMode.Manual)]
    public class CmdReadJson : IExternalCommand
    {

        private HouseObjects CurrentHouse { get; set; }
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

            using (InputForm InputJsonForm = new InputForm())
            {
                if (InputJsonForm.ShowDialog() == DialogResult.OK)
                {
                    CurrentHouse = InputJsonForm.CurrentHouse;
                }
            }
                
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
