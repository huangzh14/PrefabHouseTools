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
            a.CreateRibbonTab("Ԥ��װ�޹�����");
            RibbonPanel autoModelPanel =
                a.CreateRibbonPanel("Ԥ��װ�޹�����", "�Զ���ģ");
            RibbonPanel dataTransferPanel =
                a.CreateRibbonPanel("Ԥ��װ�޹�����", "���������ת��");
            string thisPath = Assembly
                .GetExecutingAssembly().Location;

            PushButtonData SetCircuitD = new PushButtonData
                ("CmdAutoSetCircuit", "�趨��·",
                thisPath, "PrefabHouseTools.CmdSetCircuit");
            PushButton SetCircuitB = autoModelPanel
                .AddItem(SetCircuitD) as PushButton;

            PushButtonData AutoRouteD = new PushButtonData
                ("CmdAutoRouteE", "�Զ����ɵ�·",
                thisPath, "PrefabHouseTools.CmdAutoRouteE");
            PushButton AutoRouteB = autoModelPanel
                .AddItem(AutoRouteD) as PushButton;

            PushButtonData ReadJsonD = new PushButtonData
                ("CmdReadJson", "Json\n����ת��",
                thisPath, "PrefabHouseTools.CmdReadJson");
            PushButton ReadJsonB = dataTransferPanel
                .AddItem(ReadJsonD) as PushButton;

            PushButtonData BatchRenameD = new PushButtonData
                ("CmdBatchRename", "����������",
                thisPath, "PrefabHouseTools.CmdBatchRename");
            PushButton BatchRenameB = dataTransferPanel
                .AddItem(BatchRenameD) as PushButton;

            PushButtonData TestCmdD = new PushButtonData
                ("TestCmd", "��������", thisPath,
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
