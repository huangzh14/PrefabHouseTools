#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Windows.Forms;
#endregion

namespace PrefabHouseTools
{
    [Transaction(TransactionMode.Manual)]
    public class CmdBatchRename : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            CmdBatchRenameForm renameForm = new CmdBatchRenameForm();
            if (renameForm.ShowDialog() == DialogResult.OK)
                return Result.Succeeded;
                // Modify document within a transaction

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");

                
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
