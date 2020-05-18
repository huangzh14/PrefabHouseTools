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
    /// 1-用于自动生成的rfa族文件必须具有“宽度”和“高度”这两个参数用于调整其
    /// 宽高，且远点应当位于左右中心线的底端。
    /// 2-对于每个新加入的任务，需要增加单位转换代码和工作进度更新代码
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CmdReadJson : IExternalCommand
    {
        #region 常量设置/Constant define
        /// <summary>
        /// 自动载入组的名称
        /// </summary>
        private string[] autoDoorFamilyNames = 
            { "auto-Door-PASS", "auto-Door-SINGLE","auto-Door-SLIDING" };
        private string[] autoWindowFamilyNames =
            {"auto-Window-SLIDING-FRENCH","auto-Window-HINGED",
             "auto-Window-BAY"};
        private string[] autoSocketsNames =
            {"auto-插座-单相五孔","auto-插座-单相五孔防水","auto-插座-网络电视"};
        private string[] autoWaterSupplyNames =
            {"auto-给水-八字阀","auto-给水-混水器"};
        private string[] autoLightingNames =
            {"auto-照明-LED顶灯","auto-照明-壁灯"};
        /// <summary>
        /// 关于建立各项构件的工作量系数
        /// 用于进度条的显示
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

        #region 参数设置/Parameter define
        /// <summary>
        /// 命令初始化，重新设置数据和连接当前文件
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
        /// 当前的HouseObject,用于存储json文件中读取的信息
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
        /// 当前的RoomSoftDesigh，用于存储json文件中读取的软装信息
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
        /// 已创建的基本墙类型
        /// </summary>
        private List<WallType> AutoWallTypes { get; set; }
        /// <summary>
        /// 标高信息
        /// </summary>
        private List<Level> AllLevels { get; set; }
        private Level BaseLevel { get; set; }

        #region 各项已载入的基本族
        private List<Family> AutoDoorFamilies { get; set; }
        private List<Family> AutoWindowFamilies { get; set; }
        private List<Family> AutoSocketFamilies { get; set; }
        private List<Family> AutoWaterSupplyFamilies { get; set; }
        private List<Family> AutoLightingFamilies { get; set; }
        #endregion

        /// <summary>
        /// 当前文件对象存储和调用
        /// </summary>
        private Document ActiveDoc { get; set; }
        /// <summary>
        /// 当前交互视窗存储和调用
        /// </summary>
        CmdReadJsonForm ActiveForm { get; set; }

        /// <summary>
        /// 总工作量参数
        /// </summary>
        public int TotalWorkLoad { get { return GetTotalWorkLoad(); } }
        #endregion

        #region 设置默认值和导入默认族/Set base values.
        /// <summary>
        /// 指定输入字符串对应的标高为基准标高
        /// </summary>
        /// <param name="levelName"></param>
        public void SetBaseLevel(string levelName)
        {
            BaseLevel = AllLevels
                .First(l => l.Name == levelName as string);
        }

        /// <summary>
        /// 创建基本墙面类型
        /// </summary>
        public void CreateBaseWallType()
        {
            ///如果自动类型列表内已有墙类型，则不再新建
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
        /// 向文件中载入默认门窗族
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
                    TaskDialog.Show("错误","部分默认门族丢失，请重新安装插件");
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
                    TaskDialog.Show("错误", "部分默认窗族丢失，请重新安装插件");
                    return false;
                }
                AutoWindowFamilies.Add(windowFam);
            }
            return true;
        }

        /// <summary>
        /// 向文件中载入默认插座族
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
                    TaskDialog.Show("错误", "部分默认插座族丢失，请重新安装插件");
                    return false;
                }
                AutoSocketFamilies.Add(socketFam);
            }
            return true;
        }
        /// <summary>
        /// 用于加载常量中包含的默认族（并检查默认族是否完备）
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
                    TaskDialog.Show("错误", "部分默认族丢失，请修复或重新安装插件");
                    return false;
                }
                familyList.Add(loadedFam);
            }
            return true;
        }
        #endregion

        #region 【核心】自动建模/The main work.
        /// <summary>
        /// 创建一个新的基本墙类型
        /// </summary>
        /// <param name="width_inches">用inch/英寸表示的墙体宽度</param>
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
        /// 在revit中自动创建墙体
        /// </summary>
        /// <param name="doc">当前文件</param>
        /// <param name="house">当前HouseObject，包含json读取信息</param>
        /// <param name="autoTypes">已载入的墙体类型链表</param>
        /// <param name="baseLevel">用户选定的基准标高</param>
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
        /// 在revit中自动创建所有门窗
        /// </summary>
        /// <param name="doc">当前文件</param>
        /// <param name="house">当前HouseObject，包含json读取信息</param>
        /// <param name="baseLevel">用户选定的基准标高</param>
        /// <param name="doorFamilies">已载入的自动门族链表</param>
        /// <param name="windowFamilies">已载入的自动窗族链表</param>
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
        /// 在revit中创建单个门窗
        /// </summary>
        /// <param name="doc">当前文件</param>
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
                    openingSymbol.GetParameters("宽度")[0].Set(doorW);
                    openingSymbol.GetParameters("高度")[0].Set(doorH);
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
                    case "五孔":
                        socFam = AutoSocketFamilies
                            .First(s =>
                            (s.Name.Contains("五孔")) &&
                            (!s.Name.Contains("防水")));
                        break;
                    case "五孔防水":
                        socFam = AutoSocketFamilies
                            .First(s => s.Name.Contains("五孔防水"));
                        break;
                    case "网络电视":
                        socFam = AutoSocketFamilies
                            .First(s => s.Name.Contains("网络电视"));
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
                    case "壁灯":
                        A_Wall lightWall = allWalls
                            .OrderBy(w =>
                            Line.CreateBound
                            (new XYZ(w.P1.X, w.P1.Y, light.Z), new XYZ(w.P2.X, w.P2.Y, light.Z))
                            .Project(lightCenterPt).XYZPoint.DistanceTo(lightCenterPt))
                            .First();
                        light.Instance = doc.Create.NewFamilyInstance
                            (lightCenterPt, lightSymbol, lightWall.Wall, StructuralType.NonStructural);
                        break;
                    case "顶灯":
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

            ///获取0标高平面视图以及所有家具物体
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

            ///确定执行路径，找到家具模型库，获取所有dwg文件名
            Assembly a = Assembly.GetExecutingAssembly();
            string furnitureFolder = 
                Path.GetDirectoryName(a.Location)
                +"\\CmdReadJsonFiles\\FurnitureDWG";
            string currentFurPath = string.Empty;
            string[] dwgNames = 
                new DirectoryInfo(furnitureFolder)
                .GetFiles("*.dwg")
                .Select(f => f.Name).ToArray();

            ///开始导入家具dwg
            int furNumber = allFurniture.Count;
            A_Furniture fur;///当前家具
            A_Furniture lastFur;///上一个家具
            Element furElement;///家具导入后的element
            string confirmedFurniture;///需要导入的家具名称

            for (int i = 0; i < furNumber; i++)
            {
                fur = allFurniture[i];
                ActiveForm.UpdateProgress(furnitureWorkLoad);

                ///如果数据种类不是家具，或家具对应的模型代码为空，进入下一个
                if (!fur.Kind.Contains("FURNITURE") || fur.Item == null)
                    continue;

                ///设定模型插入位置，basic层级的家具不携带z坐标，需要根据z尺寸推算中心点坐标
                ///如果有z坐标则直接使用其z坐标，并记录到家具物体中。
                inOpt.ReferencePoint = fur.Z == null ?
                    new XYZ(fur.X, fur.Y, BaseLevel.Elevation + fur.ZSize * 0.5) :
                    fur.Layer == A_FurLayer.wall ?
                    new XYZ(fur.X, fur.Y, BaseLevel.Elevation + (double)fur.Z - fur.ZSize * 0.5) :
                    new XYZ(fur.X, fur.Y, (double)fur.Z + BaseLevel.Elevation);
                fur.RefPoint = inOpt.ReferencePoint;
                ///设定模型插入参考位置，普通为中心点，靠墙为原点
                inOpt.Placement = fur.Layer == A_FurLayer.wall ?
                    ImportPlacement.Origin : ImportPlacement.Centered;
                ///设定家具旋转轴，以备使用
                XYZ axis1 = new XYZ(fur.X, fur.Y, BaseLevel.Elevation);
                XYZ axis2 = new XYZ(fur.X, fur.Y, BaseLevel.Elevation + 10);
                Line rotateAxis = Line.CreateBound(axis1, axis2);

                ///如果该家具已经导入过，直接复制。
                ///由于前期已按照item排序，所以只需查找上一个即可
                ///复制旋转后直接进入下一个循环
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

                ///进行到此说明家具未导入过，查找家具名称
                try
                {
                    confirmedFurniture = dwgNames
                        .First(s => s.Contains(fur.Item));
                }
                catch
                {///该家具未找到对应模型，进入下一个循环
                    continue;
                }
                if (confirmedFurniture == null) continue;
                ///组合家具模型文件路径
                currentFurPath = furnitureFolder + "\\" + confirmedFurniture;

                ///插入dwg文件，并解除锁定.
                doc.Import
                    (currentFurPath, inOpt, inView, 
                    out fur.ElementId);
                doc.Regenerate();
                furElement = doc.GetElement(fur.ElementId);
                if (furElement.Pinned) furElement.Pinned = false;

                ///旋转家具到指定角度
                ElementTransformUtils.RotateElement
                    (doc, fur.ElementId, rotateAxis, fur.Rotation);
            }

            ///完成导入，返回
            return true;
        }
        #endregion

        #region 主执行模块/Excution module
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
                    TaskDialog.Show("错误", "程序执行出现未知错误，请联系开发者，" +
                        "细节信息如下\n" + e.Message +"\n" + e.StackTrace);
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
