#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
#endregion

namespace PrefabHouseTools
{
    class AppPrefabHouseTools : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            a.CreateRibbonTab("PrefabHouseTools");
            RibbonPanel ribPanel =
                a.CreateRibbonPanel("PrefabHouseTools","PrefabHouseTools");
            string thisPath = Assembly
                .GetExecutingAssembly().Location;

            PushButtonData SetCircuitD = new PushButtonData
                ("CmdAutoSetCircuit", "Set Circuit",
                thisPath, "PrefabHouseTools.CmdSetCircuit");
            PushButton SetCircuitB = ribPanel
                .AddItem(SetCircuitD) as PushButton;

            PushButtonData AutoRouteD = new PushButtonData
                ("CmdAutoRouteE", "Auto Route Electrical",
                thisPath, "PrefabHouseTools.CmdAutoRouteE");
            PushButton AutoRouteB = ribPanel
                .AddItem(AutoRouteD) as PushButton;

            PushButtonData ReadJsonD = new PushButtonData
                ("CmdReadJson", "Read Json",
                thisPath, "PrefabHouseTools.CmdReadJson");
            PushButton ReadJsonB = ribPanel
                .AddItem(ReadJsonD) as PushButton;

            PushButtonData TestCmdD = new PushButtonData
                ("TestCmd", "Test", thisPath,
                "PrefabHouseTools.TestCmd");
            PushButton TestCmdB = ribPanel
                .AddItem(TestCmdD) as PushButton;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
