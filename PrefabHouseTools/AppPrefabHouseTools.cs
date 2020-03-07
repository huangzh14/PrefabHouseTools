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
            RibbonPanel autoModelPanel =
                a.CreateRibbonPanel("PrefabHouseTools","自动建模");
            RibbonPanel dataTransferPanel =
                a.CreateRibbonPanel("PrefabHouseTools", "数据输入和转换");
            string thisPath = Assembly
                .GetExecutingAssembly().Location;

            PushButtonData SetCircuitD = new PushButtonData
                ("CmdAutoSetCircuit", "Set Circuit",
                thisPath, "PrefabHouseTools.CmdSetCircuit");
            PushButton SetCircuitB = autoModelPanel
                .AddItem(SetCircuitD) as PushButton;

            PushButtonData AutoRouteD = new PushButtonData
                ("CmdAutoRouteE", "Auto Route Electrical",
                thisPath, "PrefabHouseTools.CmdAutoRouteE");
            PushButton AutoRouteB = autoModelPanel
                .AddItem(AutoRouteD) as PushButton;

            PushButtonData ReadJsonD = new PushButtonData
                ("CmdReadJson", "Json\n户型转换",
                thisPath, "PrefabHouseTools.CmdReadJson");
            PushButton ReadJsonB = dataTransferPanel
                .AddItem(ReadJsonD) as PushButton;

            PushButtonData TestCmdD = new PushButtonData
                ("TestCmd", "Test", thisPath,
                "PrefabHouseTools.TestCmd");
            PushButton TestCmdB = autoModelPanel
                .AddItem(TestCmdD) as PushButton;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
