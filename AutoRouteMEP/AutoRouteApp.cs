#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
#endregion

namespace AutoRouteMEP
{
    class AutoRouteApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            RibbonPanel ribPanel = 
                a.CreateRibbonPanel("AutoRouteMEP");
            string thisPath = Assembly
                .GetExecutingAssembly().Location;

            PushButtonData SetCircuitD = new PushButtonData
                ("CmdAutoSetCircuit", "Set Circuit", 
                thisPath, "AutoRouteMEP.CmdSetCircuit");
            PushButton SetCircuitB = ribPanel
                .AddItem(SetCircuitD) as PushButton;

            PushButtonData AutoRouteD = new PushButtonData
                ("CmdAutoRoute","Auto Route",
                thisPath,"AutoRouteMEP.CmdAutoRoute");
            PushButton AutoRouteB = ribPanel
                .AddItem(AutoRouteD) as PushButton;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
