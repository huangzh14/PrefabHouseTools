#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion


namespace PrefabHouseTools
{
    /// <summary>
    /// Some basic rules.
    /// 1-�����Զ����ɵ�rfa���ļ�������С���ȡ��͡��߶ȡ��������������ڵ�����
    /// ��ߣ���Զ��Ӧ��λ�����������ߵĵ׶ˡ�
    /// 2-����ÿ���¼����������Ҫ���ӵ�λת������͹������ȸ��´���
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CmdReadJson : IExternalCommand
    {
        #region ��������/Constant define
        /// <summary>
        /// �Զ������������
        /// </summary>
        private string[] autoDoorFamilyNames = 
            { "auto-Door-PASS", "auto-Door-SINGLE","auto-Door-SLIDING" };
        private string[] autoWindowFamilyNames =
            {"auto-Window-SLIDING-FRENCH","auto-Window-HINGED",
             "auto-Window-BAY"};
        private string[] autoSocketsNames =
            {"auto-����-�������","auto-����-������׷�ˮ","auto-����-�������"};
        private string[] autoWaterSupplyNames =
            {"auto-��ˮ-���ַ�","auto-��ˮ-��ˮ��"};
        private string[] autoLightingNames =
            {"auto-����-LED����","auto-����-�ڵ�"};
        /// <summary>
        /// ���ڽ���������Ĺ�����ϵ��
        /// ���ڽ���������ʾ
        /// </summary>
        const int wallWorkLoad = 1;
        const int doorWorkLoad = 5;
        const int windowWorkLoad = 5;
        const int socketWorkLoad = 2;
        const int waterSupplyWorkLoad = 2;
        const int lightWorkLoad = 2;
        const int furnitureWorkLoad = 50;
        private int GetTotalWorkLoad()
        {
            int total = CurrentHouse.Floors
                     .SelectMany(f => f.Walls)
                     .Count() * wallWorkLoad;
            total += CurrentHouse.Floors
                     .SelectMany(f => f.Doors)
                     .Count() * doorWorkLoad;
            total += CurrentHouse.Floors
                     .SelectMany(f => f.Windows)
                     .Count() * windowWorkLoad;
            total += CurrentHouse.Floors
                     .SelectMany(f => f.Sockets)
                     .Count() * socketWorkLoad;
            total += CurrentHouse.Floors
                     .SelectMany(f => f.Feedwater)
                     .Count() * waterSupplyWorkLoad;
            total += CurrentHouse.Floors
                     .SelectMany(f => f.Lights)
                     .Count() * lightWorkLoad;
            total += RoomSoftDesigns
                     .SelectMany(r => r.Furniture)
                     .Count() * furnitureWorkLoad;
            return total;
        }
        #endregion

        #region ��������/Parameter define
        /// <summary>
        /// �����ʼ���������������ݺ����ӵ�ǰ�ļ�
        /// </summary>
        /// <param name="doc"></param>
        public void Initialize(Document doc)
        {
            AutoWallTypes = new List<WallType>();
            AutoWallTypes.AddRange
                (new FilteredElementCollector(doc)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_Walls)
                .Where(e => e.Name.Contains("AutoWall"))
                .Select(e => e as WallType)
                .ToList());
            AllLevels = new List<Level>();
            AutoDoorFamilies = new List<Family>();
            AutoWindowFamilies = new List<Family>();
            AutoSocketFamilies = new List<Family>();
            AutoWaterSupplyFamilies = new List<Family>();
            AutoLightingFamilies = new List<Family>();
            ActiveDoc = doc;
        }

        /// <summary>
        /// ��ǰ��HouseObject,���ڴ洢json�ļ��ж�ȡ����Ϣ
        /// </summary>
        public HouseObject CurrentHouse 
        { 
            get { return currentHouse; } 
            ///Convert the units on input.
            set 
            { 
                currentHouse = value;
                currentHouse.TransferMm2Feet();
            } 
        }
        private HouseObject currentHouse;
        /// <summary>
        /// ��ǰ��RoomSoftDesigh�����ڴ洢json�ļ��ж�ȡ����װ��Ϣ
        /// </summary>
        public List<RoomSoftDesign> RoomSoftDesigns
        {
            get { return roomSoftDesigns; }
            set
            {
                this.roomSoftDesigns = value;
                foreach (RoomSoftDesign roomSD in roomSoftDesigns)
                {
                    roomSD.TransferMm2Feet();
                }
            }
        }
        private List<RoomSoftDesign> roomSoftDesigns;

        /// <summary>
        /// �Ѵ����Ļ���ǽ����
        /// </summary>
        private List<WallType> AutoWallTypes { get; set; }
        /// <summary>
        /// �����Ϣ
        /// </summary>
        private List<Level> AllLevels { get; set; }
        private Level BaseLevel { get; set; }

        #region ����������Ļ�����
        private List<Family> AutoDoorFamilies { get; set; }
        private List<Family> AutoWindowFamilies { get; set; }
        private List<Family> AutoSocketFamilies { get; set; }
        private List<Family> AutoWaterSupplyFamilies { get; set; }
        private List<Family> AutoLightingFamilies { get; set; }
        #endregion

        /// <summary>
        /// ��ǰ�ļ�����洢�͵���
        /// </summary>
        private Document ActiveDoc { get; set; }
        /// <summary>
        /// ��ǰ�����Ӵ��洢�͵���
        /// </summary>
        CmdReadJsonForm ActiveForm { get; set; }

        /// <summary>
        /// �ܹ���������
        /// </summary>
        public int TotalWorkLoad { get { return GetTotalWorkLoad(); } }
        #endregion

        #region ����Ĭ��ֵ�͵���Ĭ����/Set base values.
        /// <summary>
        /// ָ�������ַ�����Ӧ�ı��Ϊ��׼���
        /// </summary>
        /// <param name="levelName"></param>
        public void SetBaseLevel(string levelName)
        {
            BaseLevel = AllLevels
                .First(l => l.Name == levelName as string);
        }

        /// <summary>
        /// ��������ǽ������
        /// </summary>
        public void CreateBaseWallType()
        {
            ///����Զ������б�������ǽ���ͣ������½�
            if (AutoWallTypes.Count > 0) return;
            ///Create the default material.
            ElementId baseMaterialid = null;
            Material baseMaterial = null;
            Color colorGrey = new Color(80, 80, 80);
            ///First check if the material already exist.
            Material existMa = new FilteredElementCollector(ActiveDoc)
                .OfClass(typeof(Material))
                .Select(e => e as Material).ToList()
                .Where(m => m.Name == "AutoWallMaterial")
                .FirstOrDefault();
            baseMaterialid = (existMa != null) ? existMa.Id :
                Material.Create(ActiveDoc, "AutoWallMaterial");
            baseMaterial = ActiveDoc.GetElement(baseMaterialid) as Material;
            ///Set the material color.
            baseMaterial.Color = colorGrey;

            ///Create the default wall type.
            ///First check if it exist.
            WallType existWt = new FilteredElementCollector(ActiveDoc)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_Walls)
                .Select(e => e as WallType)
                .Where(w => w.Name == "AutoWall-240")
                .ToList().FirstOrDefault();
            ///If not exist,create a new one.
            WallType baseWt = (existWt != null) ?
                existWt : new FilteredElementCollector(ActiveDoc)
                        .WhereElementIsElementType()
                        .OfCategory(BuiltInCategory.OST_Walls)
                        .Select(e => e as WallType)
                        .Where(w => w.Kind == WallKind.Basic)
                        .FirstOrDefault()
                        .Duplicate("AutoWall-240") as WallType;
            ///Set the structure.
            baseWt.SetCompoundStructure(
                CompoundStructure.CreateSingleLayerCompoundStructure
                (MaterialFunctionAssignment.Structure,
                UnitUtils.ConvertToInternalUnits(240, DisplayUnitType.DUT_MILLIMETERS),
                baseMaterialid)
                );

            ///Create the wallType list and add the base type.
            AutoWallTypes = new List<WallType> { baseWt };
        }

        /// <summary>
        /// ���ļ�������Ĭ���Ŵ���
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public bool LoadOpeningFamilies(Document doc)
        {
            ///Get the path.
            Assembly a = Assembly.GetExecutingAssembly();
            string rootFolder = Path.GetDirectoryName(a.Location);
            foreach (string doorName in autoDoorFamilyNames)
            {
                string DoorPath = rootFolder
                + "\\" + doorName + ".rfa";
                if (!Helper.LoadFamily(doc, doorName ,DoorPath, out Family doorFam))
                {
                    TaskDialog.Show("����","����Ĭ�����嶪ʧ�������°�װ���");
                    return false;
                }
                AutoDoorFamilies.Add(doorFam);
            }
            foreach (string windowName in autoWindowFamilyNames)
            {
                string windowPath = rootFolder
                + "\\" + windowName + ".rfa";
                if (!Helper.LoadFamily(doc, windowName, windowPath, out Family windowFam))
                {
                    TaskDialog.Show("����", "����Ĭ�ϴ��嶪ʧ�������°�װ���");
                    return false;
                }
                AutoWindowFamilies.Add(windowFam);
            }
            return true;
        }

        /// <summary>
        /// ���ļ�������Ĭ�ϲ�����
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public bool LoadSocketsFamilies(Document doc)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string rootFolder = Path.GetDirectoryName(a.Location);
            foreach (string socketName in autoSocketsNames)
            {
                string DoorPath = rootFolder
                + "\\" + socketName + ".rfa";
                if (!Helper.LoadFamily(doc, socketName, DoorPath, out Family socketFam))
                {
                    TaskDialog.Show("����", "����Ĭ�ϲ����嶪ʧ�������°�װ���");
                    return false;
                }
                AutoSocketFamilies.Add(socketFam);
            }
            return true;
        }
        /// <summary>
        /// ���ڼ��س����а�����Ĭ���壨�����Ĭ�����Ƿ��걸��
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="familyNames"></param>
        /// <param name="familyList"></param>
        /// <returns></returns>
        public bool LoadBaseFamilies(Document doc,
            string[] familyNames, List<Family> familyList)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string rootFolder = Path.GetDirectoryName(a.Location) 
                + "\\CmdReadJsonFiles\\BaseFamily";
            foreach (string currentBaseFamily in familyNames)
            {
                string DoorPath = rootFolder
                + "\\" + currentBaseFamily + ".rfa";
                if (!Helper.LoadFamily(doc, currentBaseFamily, DoorPath, out Family loadedFam))
                {
                    TaskDialog.Show("����", "����Ĭ���嶪ʧ�����޸������°�װ���");
                    return false;
                }
                familyList.Add(loadedFam);
            }
            return true;
        }
        #endregion

        #region �����ġ��Զ���ģ/The main work.
        /// <summary>
        /// ����һ���µĻ���ǽ����
        /// </summary>
        /// <param name="width_inches">��inch/Ӣ���ʾ��ǽ����</param>
        /// <returns></returns>
        private WallType CreateWallType(float width_inches)
        {
            ///Duplicate a new walltype;
            float wallWidthMm = Helper.Feet2Mm(width_inches);
            WallType currentWt = AutoWallTypes[0].Duplicate
                ("AutoWall-" + wallWidthMm) as WallType;
            ///Set the width of the new type;
            CompoundStructure cStru = CompoundStructure
                .CreateSingleLayerCompoundStructure
                (MaterialFunctionAssignment.Structure,
                width_inches, currentWt.GetCompoundStructure().GetMaterialId(0));
            currentWt.SetCompoundStructure(cStru);
            ///Add it to collection.
            AutoWallTypes.Add(currentWt);
            return currentWt;
        }

        /// <summary>
        /// ��revit���Զ�����ǽ��
        /// </summary>
        /// <param name="doc">��ǰ�ļ�</param>
        /// <param name="house">��ǰHouseObject������json��ȡ��Ϣ</param>
        /// <param name="autoTypes">�������ǽ����������</param>
        /// <param name="baseLevel">�û�ѡ���Ļ�׼���</param>
        public void CreateWalls
            (Document doc,HouseObject house,
            List<WallType> autoTypes,Level baseLevel)
        {
            foreach (A_Floor floor in house.Floors)
            {
                ///Create the walls.
                foreach (A_Wall wa in floor.Walls)
                {
                    WallType currentWt = null;
                    XYZ p1 = new XYZ(wa.P1.X, wa.P1.Y, 0);
                    XYZ p2 = new XYZ(wa.P2.X, wa.P2.Y, 0);
                    Curve c = Line.CreateBound(p1, p2);

                    ///Find the right wall type.
                    ///If the type doesnt exist,create a new one.
                    try
                    {
                        currentWt = autoTypes
                            .First(at => at.Width - wa.Thickness < 0.001);
                    }
                    catch
                    {
                        currentWt = this.CreateWallType(wa.Thickness);
                    }

                    ///Create the individual wall
                    wa.Wall = Wall.Create(doc, c, currentWt.Id, baseLevel.Id,
                        floor.Height, 0, false, true);
                    ///Update progress.
                    ActiveForm.UpdateProgress(wallWorkLoad);
                }

                ///Create the floor.
                CurveArray floorCrv = new CurveArray();
                foreach (A_Room outer in floor.Outers)
                {
                    foreach (A_Contour con in outer.Meta.Contours)
                    {
                        floorCrv.Append(Line.CreateBound
                            (new XYZ(con.P1.X, con.P1.Y, baseLevel.Elevation),
                             new XYZ(con.P2.X, con.P2.Y, baseLevel.Elevation)));
                    }
                    doc.Create.NewFloor(floorCrv, false);
                }
            }
        }

        /// <summary>
        /// ��revit���Զ����������Ŵ�
        /// </summary>
        /// <param name="doc">��ǰ�ļ�</param>
        /// <param name="house">��ǰHouseObject������json��ȡ��Ϣ</param>
        /// <param name="baseLevel">�û�ѡ���Ļ�׼���</param>
        /// <param name="doorFamilies">��������Զ���������</param>
        /// <param name="windowFamilies">��������Զ���������</param>
        public void CreateOpenings
            (Document doc, HouseObject house,
            Level baseLevel,List<Family>doorFamilies,
            List<Family>windowFamilies)
        {
            foreach (A_Floor f in house.Floors)
            {
                foreach (A_Door d in f.Doors)
                {
                    ///Find the right doorfamily.
                    Family doorFam;
                    switch (d.Kind)
                    {
                        case A_DoorKind.PASS:
                            doorFam = doorFamilies
                                .First(fa => fa.Name.Contains("PASS"));
                            break;
                        case A_DoorKind.SLIDING:
                            doorFam = doorFamilies
                                .First(fa => fa.Name.Contains("SLIDING"));
                            break;
                        default:
                            doorFam = doorFamilies
                                .First(fa => fa.Name.Contains("SINGLE"));
                            break;
                    }

                    ///Create the door.
                    d.Instance = CreateSingleOpening
                        (doc, f, d, baseLevel, doorFam,
                        BuiltInCategory.OST_Doors);

                    bool fliped = false;
                    ///Check the direction.
                    doc.Regenerate();
                    if (!d.Instance.FacingOrientation
                        .IsAlmostEqualTo(d.FacingOrientation))
                    {
                        d.Instance.flipFacing();
                        fliped = true;
                    }
                    if (!d.Instance.HandOrientation
                        .IsAlmostEqualTo(d.HandOrientation))
                    {
                        d.Instance.flipHand();
                        fliped = true;
                    }
                    if (!fliped)
                    {
                        d.Instance.flipFacing();
                        d.Instance.flipFacing();
                    }

                    ///Update the progress.
                    ActiveForm.UpdateProgress(doorWorkLoad);
                }

                foreach (A_Window w in f.Windows)
                {
                    ///Find the right window family.
                    string test = w.Kind.ToString();
                    Family windowFam = windowFamilies
                        .First(wf => wf.Name.Contains(w.Kind.ToString()));

                    ///Create the window.
                    w.Instance = CreateSingleOpening
                        (doc, f, w, BaseLevel, windowFam,
                        BuiltInCategory.OST_Windows);
                    doc.Regenerate();
                    w.Instance.flipFacing();
                    doc.Regenerate();
                    w.Instance.flipFacing();

                    ActiveForm.UpdateProgress(windowWorkLoad);
                }
            }
        }


        /// <summary>
        /// ��revit�д��������Ŵ�
        /// </summary>
        /// <param name="doc">��ǰ�ļ�</param>
        /// <param name="floor"></param>
        /// <param name="opening"></param>
        /// <param name="baseLevel"></param>
        /// <param name="baseFamily"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public FamilyInstance CreateSingleOpening
            (Document doc, A_Floor floor, A_Opening opening, Level baseLevel
            , Family baseFamily, BuiltInCategory category)
        {
            ///Get the default family symbol.
            FamilySymbol openingSymbol = doc.GetElement
                    (baseFamily.GetFamilySymbolIds().First())
                    as FamilySymbol; ;
            ///Find the host wall.
            Wall hostW = floor.Walls
                .First(w => w.Uid == opening.Meta.Wall).Wall;
            ///Find the central point.
            XYZ centerPt = new XYZ((opening.P1.X + opening.P2.X) / 2,
                            (opening.P1.Y + opening.P2.Y) / 2,
                            baseLevel.Elevation + opening.SillHeight);

            ///Calculate the width and height of the door.
            float doorW = opening.Width;
            float doorH = opening.Height;
            int doorWmm = (int)Math.Round(Helper.Feet2Mm(doorW));
            int doorHmm = (int)Math.Round(Helper.Feet2Mm(doorH));
            string typeName = doorWmm + "-" + doorHmm;

            ///Check if that symbol already exist.
            FamilySymbol existSymbol = Helper.FindFamilySymbol
                (doc, category, typeName, baseFamily.Name);
            if (existSymbol != null)
            {
                openingSymbol = existSymbol;
            }
            else
            {
                openingSymbol = (FamilySymbol)
                    openingSymbol.Duplicate(typeName);
                try
                {
                    openingSymbol.GetParameters("���")[0].Set(doorW);
                    openingSymbol.GetParameters("�߶�")[0].Set(doorH);
                }
                catch
                {
                    openingSymbol.GetParameters("Width")[0].Set(doorW);
                    openingSymbol.GetParameters("Height")[0].Set(doorH);
                }
            }

            ///Create the door.
            openingSymbol.Activate();
            return doc.Create.NewFamilyInstance
                (centerPt, openingSymbol, hostW,
                StructuralType.NonStructural);
        }

        public Reference FindClosestFaceOnWall(Document doc,XYZ pt,Wall hostWall,out XYZ dirPt)
        {
            dirPt = new XYZ();
            List<Reference> sideFaces = HostObjectUtils.GetSideFaces
                (hostWall, ShellLayerType.Exterior).ToList();
            sideFaces.AddRange(HostObjectUtils.GetSideFaces
                (hostWall, ShellLayerType.Interior).ToList());
            Reference resultRef = sideFaces
                      .OrderBy(f => pt.DistanceTo
                                    ((doc.GetElement(f)
                                    .GetGeometryObjectFromReference(f)
                                    as Face)
                                    .Project(pt).XYZPoint))
                      .First();
            if (resultRef == null) return null;
            Face resultFace = doc.GetElement(resultRef)
                .GetGeometryObjectFromReference(resultRef) as Face;
            dirPt = pt - resultFace.Project(pt).XYZPoint;
            return resultRef;

        }
        public FamilyInstance CreateSystemTerminal
            (Document doc, A_SystemTerminal sysTer, Family baseFamily,
            IList<A_Wall> allWalls)
        {
            XYZ centerPt = new XYZ
                    (sysTer.X, sysTer.Y, sysTer.Z + BaseLevel.Elevation);
            XYZ dirPt = new XYZ
                (0 - sysTer.Orientation.Y, sysTer.Orientation.X, 0);

            ///Get all the face of the host wall.
            Wall hostWall = allWalls
                .First(w => w.Uid == sysTer.Related.Uid).Wall;
            List<Reference> sideFaces = HostObjectUtils.GetSideFaces
                (hostWall, ShellLayerType.Exterior).ToList();
            sideFaces.AddRange(HostObjectUtils.GetSideFaces
                (hostWall, ShellLayerType.Interior).ToList());

            ///Find the face where the socket is located.
            Reference hostFace = sideFaces
                      .OrderBy(f => centerPt.DistanceTo
                                    ((doc.GetElement(f)
                                    .GetGeometryObjectFromReference(f)
                                    as Face)
                                    .Project(centerPt).XYZPoint))
                      .First();
            if (hostFace == null) return null;


            FamilySymbol terSymbol =
                doc.GetElement
                (baseFamily.GetFamilySymbolIds().First())
                as FamilySymbol;
            terSymbol.Activate();
            FamilyInstance currentTerminal;

            try
            {
                currentTerminal = doc.Create.NewFamilyInstance
                (hostFace, centerPt, dirPt, terSymbol);
            }
            catch
            {
                currentTerminal = doc.Create.NewFamilyInstance
                    (centerPt, terSymbol, StructuralType.NonStructural );
                XYZ dir = new XYZ(sysTer.Orientation.X, sysTer.Orientation.Y, 0);
                XYZ originalDir = new XYZ(0, -1, 0);
                double angle = originalDir.AngleTo(dir);
                Line axis = Line.CreateBound(centerPt, centerPt + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc, currentTerminal.Id, axis, angle);
                
            }
            return currentTerminal;
        }


        public bool DoCreateWalls()
        {
            CreateBaseWallType();

            CreateWalls(ActiveDoc, CurrentHouse, AutoWallTypes, BaseLevel);

            return true;
        }
        public bool DoCreateOpenings()
        {
            if (!this.LoadOpeningFamilies(ActiveDoc))
                return false;

            CreateOpenings(ActiveDoc, CurrentHouse, BaseLevel,
                AutoDoorFamilies, AutoWindowFamilies);

            return true;
        }
        public bool DoCreateSockets()
        {
            Document doc = this.ActiveDoc;
            if (!this.LoadBaseFamilies(doc,autoSocketsNames,AutoSocketFamilies))
                return false;

            List<A_Socket> allSockets = CurrentHouse.Floors
                .SelectMany(f => f.Sockets).ToList();
            List<A_Wall> allWalls = CurrentHouse.Floors
                .SelectMany(f => f.Walls).ToList();

            if (allSockets.Count() == 0)
                return false;

            foreach (A_Socket soc in allSockets)
            {
                ///Choose the type.
                Family socFam;
                switch (soc.Tag)
                {
                    case "���":
                        socFam = AutoSocketFamilies
                            .First(s =>
                            (s.Name.Contains("���")) &&
                            (!s.Name.Contains("��ˮ")));
                        break;
                    case "��׷�ˮ":
                        socFam = AutoSocketFamilies
                            .First(s => s.Name.Contains("��׷�ˮ"));
                        break;
                    case "�������":
                        socFam = AutoSocketFamilies
                            .First(s => s.Name.Contains("�������"));
                        break;
                    default:
                        socFam = AutoSocketFamilies
                            .First(s => s.Name.Contains(soc.Tag));
                        break;
                }

                soc.Instance = 
                    CreateSystemTerminal(doc, soc, socFam, allWalls);

                #region
                /*
                XYZ centerPt = new XYZ
                    (soc.X, soc.Y, soc.Z + BaseLevel.Elevation);
                XYZ dirPt = new XYZ
                    (0-soc.Orientation.Y, soc.Orientation.X, 0);

                ///Get all the face of the host wall.
                Wall hostWall = allWalls
                    .First(w => w.Uid == soc.Related.Uid).Wall;
                List<Reference> sideFaces = HostObjectUtils.GetSideFaces
                    (hostWall, ShellLayerType.Exterior).ToList();
                sideFaces.AddRange(HostObjectUtils.GetSideFaces
                    (hostWall, ShellLayerType.Interior).ToList());

                ///Find the face where the socket is located.
                Reference hostFace = sideFaces
                          .OrderBy(f => centerPt.DistanceTo
                                        ((doc.GetElement(f)
                                        .GetGeometryObjectFromReference(f)
                                        as Face)
                                        .Project(centerPt).XYZPoint))
                          .First();
                if (hostFace == null) continue;

                
                FamilySymbol socSymbol = 
                    doc.GetElement
                    (socFam.GetFamilySymbolIds().First()) 
                    as FamilySymbol;

                socSymbol.Activate();
                soc.Instance = doc.Create
                    .NewFamilyInstance
                    (hostFace, centerPt,dirPt,socSymbol);
                    */
                #endregion Old Module

                ActiveForm.UpdateProgress(socketWorkLoad);
            }
            return true;
        }
        public bool DoCreateWaterSupplys()
        {
            Document doc = this.ActiveDoc;
            if (!this.LoadBaseFamilies(doc, autoWaterSupplyNames, AutoWaterSupplyFamilies))
                return false;

            List<A_WaterSupply> allWaterSupplies = CurrentHouse.Floors
                .SelectMany(f => f.Feedwater).ToList();
            List<A_Wall> allWalls = CurrentHouse.Floors
                .SelectMany(f => f.Walls).ToList();

            if (allWaterSupplies.Count() == 0)
                return false;

            foreach (A_WaterSupply waterSupTer in allWaterSupplies)
            {
                ///Choose the type.
                Family waterSupFam;
                switch (waterSupTer.Tag)
                {
                    default:
                        waterSupFam = AutoWaterSupplyFamilies
                            .First(s => s.Name.Contains(waterSupTer.Tag));
                        break;
                }

                waterSupTer.Instance =
                    CreateSystemTerminal(doc, waterSupTer, waterSupFam, allWalls);

                ActiveForm.UpdateProgress(waterSupplyWorkLoad);
            }
            return true;
        }
        public bool DoCreateLights()
        {
            Document doc = this.ActiveDoc;
            List<A_Lighting> allLights = CurrentHouse.Floors
                .SelectMany(f => f.Lights).ToList();
            List<A_Wall> allWalls = CurrentHouse.Floors
                .SelectMany(f => f.Walls).ToList();
            if (allLights.Count == 0) return false;

            if (!LoadBaseFamilies(doc, autoLightingNames, AutoLightingFamilies))
                return false;
            foreach (A_Lighting light in allLights)
            {
                Family lightFam = AutoLightingFamilies
                    .First(f => f.Name.Contains(light.Name));
                FamilySymbol lightSymbol = doc.GetElement
                    (lightFam.GetFamilySymbolIds().First())
                    as FamilySymbol;
                lightSymbol.Activate();

                XYZ lightCenterPt = new XYZ(light.X, light.Y, light.Z);
                switch (light.Name)
                {
                    case "�ڵ�":
                        A_Wall lightWall = allWalls
                            .OrderBy(w =>
                            Line.CreateBound
                            (new XYZ(w.P1.X, w.P1.Y, light.Z), new XYZ(w.P2.X, w.P2.Y, light.Z))
                            .Project(lightCenterPt).XYZPoint.DistanceTo(lightCenterPt))
                            .First();
                        light.Instance = doc.Create.NewFamilyInstance
                            (lightCenterPt, lightSymbol, lightWall.Wall, StructuralType.NonStructural);
                        break;
                    case "����":
                        light.Instance = doc.Create.NewFamilyInstance
                            (new XYZ(light.X, light.Y, light.Z), 
                            lightSymbol, StructuralType.NonStructural);
                        break;
                    default:
                        break;
                }
                ActiveForm.UpdateProgress(lightWorkLoad);
            }
            return true;
        }
        public bool DoCreateFurniture()
        {
            Document doc = ActiveDoc;
            DWGImportOptions inOpt = new DWGImportOptions();

            ///��ȡ0���ƽ����ͼ�Լ����мҾ�����
            Autodesk.Revit.DB.View inView = 
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .Select(e => e as Autodesk.Revit.DB.View)
                .First(v => v.GenLevel.Name == BaseLevel.Name);
            List<A_Furniture> allFurniture = 
                RoomSoftDesigns
                .SelectMany(r => r.Furniture)
                .OrderBy(f => f.Item)
                .ToList();

            ///ȷ��ִ��·�����ҵ��Ҿ�ģ�Ϳ⣬��ȡ����dwg�ļ���
            Assembly a = Assembly.GetExecutingAssembly();
            string furnitureFolder = 
                Path.GetDirectoryName(a.Location)
                +"\\CmdReadJsonFiles\\FurnitureDWG";
            string currentFurPath = string.Empty;
            string[] dwgNames = 
                new DirectoryInfo(furnitureFolder)
                .GetFiles("*.dwg")
                .Select(f => f.Name).ToArray();

            ///��ʼ����Ҿ�dwg
            int furNumber = allFurniture.Count;
            A_Furniture fur;///��ǰ�Ҿ�
            A_Furniture lastFur;///��һ���Ҿ�
            Element furElement;///�Ҿߵ�����element
            string confirmedFurniture;///��Ҫ����ļҾ�����

            for (int i = 0; i < furNumber; i++)
            {
                fur = allFurniture[i];
                ActiveForm.UpdateProgress(furnitureWorkLoad);

                ///����������಻�ǼҾߣ���Ҿ߶�Ӧ��ģ�ʹ���Ϊ�գ�������һ��
                if (!fur.Kind.Contains("FURNITURE") || fur.Item == null)
                    continue;

                ///�趨ģ�Ͳ���λ�ã�basic�㼶�ļҾ߲�Я��z���꣬��Ҫ����z�ߴ��������ĵ�����
                ///�����z������ֱ��ʹ����z���꣬����¼���Ҿ������С�
                inOpt.ReferencePoint = fur.Z == null ?
                    new XYZ(fur.X, fur.Y, BaseLevel.Elevation + fur.ZSize * 0.5) :
                    fur.Layer == A_FurLayer.wall ?
                    new XYZ(fur.X, fur.Y, BaseLevel.Elevation + (double)fur.Z - fur.ZSize * 0.5) :
                    new XYZ(fur.X, fur.Y, (double)fur.Z + BaseLevel.Elevation);
                fur.RefPoint = inOpt.ReferencePoint;
                ///�趨ģ�Ͳ���ο�λ�ã���ͨΪ���ĵ㣬��ǽΪԭ��
                inOpt.Placement = fur.Layer == A_FurLayer.wall ?
                    ImportPlacement.Origin : ImportPlacement.Centered;
                ///�趨�Ҿ���ת�ᣬ�Ա�ʹ��
                XYZ axis1 = new XYZ(fur.X, fur.Y, BaseLevel.Elevation);
                XYZ axis2 = new XYZ(fur.X, fur.Y, BaseLevel.Elevation + 10);
                Line rotateAxis = Line.CreateBound(axis1, axis2);

                ///����üҾ��Ѿ��������ֱ�Ӹ��ơ�
                ///����ǰ���Ѱ���item��������ֻ�������һ������
                ///������ת��ֱ�ӽ�����һ��ѭ��
                if (i > 0 && fur.Item == allFurniture[i - 1].Item)
                {
                    lastFur = allFurniture[i - 1];
                    fur.ElementId = ElementTransformUtils
                        .CopyElement(doc, lastFur.ElementId, fur.RefPoint - lastFur.RefPoint)
                        .First();
                    ElementTransformUtils.RotateElement
                        (doc, fur.ElementId,rotateAxis,fur.Rotation-lastFur.Rotation);
                    continue;
                }

                ///���е���˵���Ҿ�δ����������ҼҾ�����
                try
                {
                    confirmedFurniture = dwgNames
                        .First(s => s.Contains(fur.Item));
                }
                catch
                {///�üҾ�δ�ҵ���Ӧģ�ͣ�������һ��ѭ��
                    continue;
                }
                if (confirmedFurniture == null) continue;
                ///��ϼҾ�ģ���ļ�·��
                currentFurPath = furnitureFolder + "\\" + confirmedFurniture;

                ///����dwg�ļ������������.
                doc.Import
                    (currentFurPath, inOpt, inView, 
                    out fur.ElementId);
                doc.Regenerate();
                furElement = doc.GetElement(fur.ElementId);
                if (furElement.Pinned) furElement.Pinned = false;

                ///��ת�Ҿߵ�ָ���Ƕ�
                ElementTransformUtils.RotateElement
                    (doc, fur.ElementId, rotateAxis, fur.Rotation);
            }

            ///��ɵ��룬����
            return true;
        }
        #endregion

        #region ��ִ��ģ��/Excution module
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

            this.Initialize(doc);
            BaseLevel = null;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("AutoModel");
                ///Using input form to read json file into current house object.
                try
                {
                    CmdReadJsonForm InputJsonForm = new CmdReadJsonForm(this);
                    this.ActiveForm = InputJsonForm;
                    ///List levels.
                    AllLevels = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .OfCategory(BuiltInCategory.OST_Levels)
                        .Select(e => e as Level).ToList();
                    foreach (Level l in AllLevels)
                    {
                        InputJsonForm.LevelBox.Items.Add(l.Name);
                    }
                    InputJsonForm.ShowDialog();
                    ///if (InputJsonForm.ShowDialog() != DialogResult.OK)
                    ///    return Result.Failed;
                }
                catch (Exception e)
                {
                    TaskDialog.Show("����", "����ִ�г���δ֪��������ϵ�����ߣ�" +
                        "ϸ����Ϣ����\n" + e.Message +"\n" + e.StackTrace);
                    return Result.Failed;
                }

                tx.Commit();
            }
            
            ///In case the user close the form.
            if (CurrentHouse == null) return Result.Failed;

            return Result.Succeeded;
        }
        #endregion

    }
}
