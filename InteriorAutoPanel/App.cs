#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
#endregion

namespace InteriorAutoPanel
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            RibbonPanel ribbonPanel = a.CreateRibbonPanel("Auto Panel");

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData autoPanelData = new PushButtonData
                ("cmdAutoPanel", "Auto Panel", thisAssemblyPath, "InteriorAutoPanel.CmdAutoPanel");
            PushButton autoPanelButton = ribbonPanel.AddItem(autoPanelData) as PushButton;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
