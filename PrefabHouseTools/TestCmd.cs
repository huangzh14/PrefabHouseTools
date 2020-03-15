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
#endregion

namespace PrefabHouseTools
{
    [Transaction(TransactionMode.Manual)]
    public class TestCmd : IExternalCommand
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

            // Access current selection

            Selection sel = uidoc.Selection;

            // Retrieve elements from database

            FilteredElementCollector col
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Wall));

            // Filtered element collector is iterable

            foreach (Element e in col)
            {
                Debug.Print(e.Name);
            }

            // Modify document within a transaction

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");

                Assembly a = Assembly.GetExecutingAssembly();
                string furnitureFolder =
                    Path.GetDirectoryName(a.Location)
                    + "\\CmdReadJsonFiles\\FurnitureDWG";
                string currentFurPath = string.Empty;

                string[] dwgNames =
                    new DirectoryInfo(furnitureFolder)
                    .GetFiles("*.dwg")
                    .Select(f => f.Name).ToArray();

                Categories cats = doc.Settings.Categories;
                Category lineCat = cats
                    .get_Item(BuiltInCategory.OST_Lines);
                Category newLineStyleCat = cats.NewSubcategory
                    (lineCat, "TestLinestyle");
                doc.Regenerate();
                newLineStyleCat.SetLineWeight
                    (7, GraphicsStyleType.Projection);
                newLineStyleCat.LineColor = 
                    new Color(255, 0, 0);
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
