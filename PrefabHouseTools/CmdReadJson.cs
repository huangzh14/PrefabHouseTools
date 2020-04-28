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
        /// <summary>
        /// ���ڽ���������Ĺ�����ϵ��
        /// ���ڽ���������ʾ
        /// </summary>
        const int wallWorkLoad = 1;
        const int doorWorkLoad = 5;
        const int windowWorkLoad = 5;
        const int socketWorkLoad = 2;
        const int furnitureWorkLoad = 50;
        #endregion

        #region ��������/Parameter define
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
                this.TransferDataUnits(currentHouse);
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
                this.TransferDataUnits(roomSoftDesigns);
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
        /// <summary>
        /// ��������Զ�����
        /// </summary>
        private List<Family> AutoDoorFamilies { get; set; }
        /// <summary>
        /// ��������Զ�����
        /// </summary>
        private List<Family> AutoWindowFamilies { get; set; }
        /// <summary>
        /// ��������Զ�������
        /// </summary>
        private List<Family> AutoSocketFamilies { get; set; }

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

        #region ��λת��������-Ӣ��,�Ƕ�-���ȣ�/Unit conversion part.
        /// <summary>
        /// The method to convert all mm to inch in the house object.
        /// </summary>
        /// <param name="house"></param>
        private void TransferDataUnits(HouseObject house)
        {
            try
            {
                foreach (A_Floor floor in house.Floors)
                {
                    foreach (A_Wall wa in floor.Walls)
                    {
                        wa.P1.X = Helper.Mm2Feet(wa.P1.X);
                        wa.P1.Y = Helper.Mm2Feet(wa.P1.Y);
                        wa.P2.X = Helper.Mm2Feet(wa.P2.X);
                        wa.P2.Y = Helper.Mm2Feet(wa.P2.Y);
                        wa.Thickness = Helper.Mm2Feet(wa.Thickness);
                        wa.Height = Helper.Mm2Feet(wa.Height);
                    }
                    foreach (A_Door door in floor.Doors)
                    {
                        door.P1.X = Helper.Mm2Feet(door.P1.X);
                        door.P1.Y = Helper.Mm2Feet(door.P1.Y);
                        door.P2.X = Helper.Mm2Feet(door.P2.X);
                        door.P2.Y = Helper.Mm2Feet(door.P2.Y);
                        door.Height = Helper.Mm2Feet(door.Height);
                        door.SillHeight = Helper.Mm2Feet(door.SillHeight);
                    }
                    foreach (A_Window window in floor.Windows)
                    {
                        window.P1.X = Helper.Mm2Feet(window.P1.X);
                        window.P1.Y = Helper.Mm2Feet(window.P1.Y);
                        window.P2.X = Helper.Mm2Feet(window.P2.X);
                        window.P2.Y = Helper.Mm2Feet(window.P2.Y);
                        window.Height = Helper.Mm2Feet(window.Height);
                        window.SillHeight = Helper.Mm2Feet(window.SillHeight);
                        window.BayDepth = Helper.Mm2Feet(window.BayDepth);
                    }
                    foreach (A_Cube cube in floor.Cubes)
                    {
                        cube.X = Helper.Mm2Feet(cube.X);
                        cube.Y = Helper.Mm2Feet(cube.Y);
                        cube.XSize = Helper.Mm2Feet(cube.XSize);
                        cube.YSize = Helper.Mm2Feet(cube.YSize);
                        cube.Z = Helper.Mm2Feet(cube.Z);
                        cube.ZSize = Helper.Mm2Feet(cube.ZSize);
                    }
                    foreach (A_Room room in floor.Rooms)
                    {
                        foreach (A_Contour contour in room.Meta.Contours)
                        {
                            contour.P1.X = Helper.Mm2Feet(contour.P1.X);
                            contour.P1.Y = Helper.Mm2Feet(contour.P1.Y);
                            contour.P2.X = Helper.Mm2Feet(contour.P2.X);
                            contour.P2.Y = Helper.Mm2Feet(contour.P2.Y);
                        }
                        foreach (A_Contour contour in room.Meta.CubeContours)
                        {
                            contour.P1.X = Helper.Mm2Feet(contour.P1.X);
                            contour.P1.Y = Helper.Mm2Feet(contour.P1.Y);
                            contour.P2.X = Helper.Mm2Feet(contour.P2.X);
                            contour.P2.Y = Helper.Mm2Feet(contour.P2.Y);
                        }
                    }
                    foreach (A_Room outer in floor.Outers)
                    {
                        foreach (A_Contour contour in outer.Meta.Contours)
                        {
                            contour.P1.X = Helper.Mm2Feet(contour.P1.X);
                            contour.P1.Y = Helper.Mm2Feet(contour.P1.Y);
                            contour.P2.X = Helper.Mm2Feet(contour.P2.X);
                            contour.P2.Y = Helper.Mm2Feet(contour.P2.Y);
                        }
                        foreach (A_Contour contour in outer.Meta.CubeContours)
                        {
                            contour.P1.X = Helper.Mm2Feet(contour.P1.X);
                            contour.P1.Y = Helper.Mm2Feet(contour.P1.Y);
                            contour.P2.X = Helper.Mm2Feet(contour.P2.X);
                            contour.P2.Y = Helper.Mm2Feet(contour.P2.Y);

                        }
                    }
                    foreach (A_Label label in floor.Labels)
                    {
                        label.Position.X = Helper.Mm2Feet(label.Position.X);
                        label.Position.Y = Helper.Mm2Feet(label.Position.Y);
                    }
                    floor.Height = Helper.Mm2Feet(floor.Height);


                    if (floor.Socket == null)
                    {
                        floor.Socket = new List<A_Socket>();
                    }
                    foreach (A_Socket socket in floor.Socket)
                    {
                        socket.X = Helper.Mm2Feet(socket.X);
                        socket.Y = Helper.Mm2Feet(socket.Y);
                        socket.Z = Helper.Mm2Feet(socket.Z);
                    }
                }
            }
            catch
            {
                throw new Exception("Json���ݲ������������𻵣����������ļ���");
            }
            
        }

        private void TransferDataUnits(List<RoomSoftDesign> softDesignsInput)
        {
            foreach (RoomSoftDesign rsf in softDesignsInput)
            {
                foreach (A_Furniture fur in rsf.Furniture)
                {
                    fur.X = Helper.Mm2Feet(fur.X);
                    fur.Y = Helper.Mm2Feet(fur.Y);
                    fur.XSize = Helper.Mm2Feet(fur.XSize);
                    fur.YSize = Helper.Mm2Feet(fur.YSize);
                    fur.ZSize = Helper.Mm2Feet(fur.ZSize);
                    fur.Z = Helper.Mm2Feet(fur.Z);
                    fur.Rotation = (float)Math.PI * fur.Rotation / 180;
                }
            }
        }
        #endregion

        #region ��ģ���ȸ���/Progress calculating
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
                     .SelectMany(f => f.Socket)
                     .Count() * socketWorkLoad;
            total += RoomSoftDesigns
                     .SelectMany(r => r.Furniture)
                     .Count() * furnitureWorkLoad;
            return total;
        }

        #endregion

        #region ����Ĭ��ֵ�͵���Ĭ����/Set base values.
        /// <summary>
        /// �����ʼ���������������ݺ����ӵ�ǰ�ļ�
        /// </summary>
        /// <param name="doc"></param>
        public void Initialize(Document doc)
        {
            AutoWallTypes = new List<WallType>();
            AllLevels = new List<Level>();
            AutoDoorFamilies = new List<Family>();
            AutoWindowFamilies = new List<Family>();
            AutoSocketFamilies = new List<Family>();
            ActiveDoc = doc;
        }
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
                            .First(at => at.Width - wa.Thickness < 0.0001);
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
                openingSymbol.GetParameters("���")[0].Set(doorW);
                openingSymbol.GetParameters("�߶�")[0].Set(doorH);
            }

            ///Create the door.
            openingSymbol.Activate();
            return doc.Create.NewFamilyInstance
                (centerPt, openingSymbol, hostW,
                StructuralType.NonStructural);
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
            if (!this.LoadSocketsFamilies(ActiveDoc))
                return false;

            Document doc = this.ActiveDoc;
            List<A_Socket> allSockets = CurrentHouse.Floors
                .SelectMany(f => f.Socket).ToList();
            List<A_Wall> allWalls = CurrentHouse.Floors
                .SelectMany(f => f.Walls).ToList();

            if (allSockets.Count() == 0)
                return false;

            foreach (A_Socket soc in allSockets)
            {
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

                ///Choose the type.
                Family socFam ;
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
                FamilySymbol socSymbol = 
                    doc.GetElement
                    (socFam.GetFamilySymbolIds().First()) 
                    as FamilySymbol;

                socSymbol.Activate();
                soc.Instance = doc.Create
                    .NewFamilyInstance
                    (hostFace, centerPt,dirPt,socSymbol);

                ActiveForm.UpdateProgress(socketWorkLoad);
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
                    TaskDialog.Show("Error", "Something went wrong," +
                        "details as follow:\n" + e.Message);
                    return Result.Failed;
                }

                tx.Commit();
            }
            

            ///In case the user close the form.
            if (CurrentHouse == null) return Result.Failed;

            return Result.Succeeded;
        }

    }
}
