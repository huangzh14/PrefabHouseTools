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
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CmdReadJson : IExternalCommand
    {
        /// <summary>
        /// The names of the default family name of doors and windows.
        /// </summary>
        private string[] autoDoorFamilyNames = 
            { "auto-Door-PASS", "auto-Door-SINGLE","auto-Door-SLIDING" };
        private string[] autoWindowFamilyNames =
            {"auto-Window-SLIDING-FRENCH","auto-Window-HINGED",
             "auto-Window-BAY"};
        private string[] autoSocketsNames =
            {"auto-单相五孔插座","auto-单相五孔防水插座","auto-网络电视"};

        /// <summary>
        /// Store the current house object.
        /// Contain all the info from input json file.
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
        /// Store all the WallTypes already created.
        /// </summary>
        private List<WallType> AutoWallTypes { get; set; }
        private List<Level> AllLevels { get; set; }
        private Level BaseLevel { get; set; }
        private List<Family> AutoDoorFamilies { get; set; }
        private List<Family> AutoWindowFamilies { get; set; }
        private List<Family> AutoSocketFamilies { get; set; }
        private Document activeDoc { get; set; }
        CmdReadJsonForm activeForm { get; set; }

        public int TotalWorkLoad { get { return GetTotalWorkLoad(); } }

        #region Unit conversion part.
        /// <summary>
        /// The method to convert all mm to inch in the house object.
        /// </summary>
        /// <param name="house"></param>
        private void TransferDataUnits(HouseObject house)
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
                foreach (A_Socket socket in floor.Socket)
                {
                    socket.X = Helper.Mm2Feet(socket.X);
                    socket.Y = Helper.Mm2Feet(socket.Y);
                    socket.Z = Helper.Mm2Feet(socket.Z);
                }
            }
        }
        #endregion

        #region Progress calculating
        private int GetTotalWorkLoad()
        {
            int total = CurrentHouse.Floors
                     .SelectMany(f => f.Walls)
                     .Count();
            total += CurrentHouse.Floors
                     .SelectMany(f => f.Doors)
                     .Count();
            total += CurrentHouse.Floors
                     .SelectMany(f => f.Windows)
                     .Count();
            total += CurrentHouse.Floors
                     .SelectMany(f => f.Socket)
                     .Count();
            return total;
        }

        #endregion


        #region Set base values.
        public void Initialize(Document doc)
        {
            AutoWallTypes = new List<WallType>();
            AllLevels = new List<Level>();
            AutoDoorFamilies = new List<Family>();
            AutoWindowFamilies = new List<Family>();
            AutoSocketFamilies = new List<Family>();
            activeDoc = doc;
        }
        public void SetBaseLevel(string levelName)
        {
            BaseLevel = AllLevels
                .First(l => l.Name == levelName as string);
        }

        public void CreateBaseWallType()
        {
            ///Create the default material.
            ElementId baseMaterialid = null;
            Material baseMaterial = null;
            Color colorGrey = new Color(80, 80, 80);
            ///First check if the material already exist.
            Material existMa = new FilteredElementCollector(activeDoc)
                .OfClass(typeof(Material))
                .Select(e => e as Material).ToList()
                .Where(m => m.Name == "AutoWallMaterial")
                .FirstOrDefault();
            baseMaterialid = (existMa != null) ? existMa.Id :
                Material.Create(activeDoc, "AutoWallMaterial");
            baseMaterial = activeDoc.GetElement(baseMaterialid) as Material;
            ///Set the material color.
            baseMaterial.SurfaceForegroundPatternColor = colorGrey;
            baseMaterial.SurfaceBackgroundPatternColor = colorGrey;
            baseMaterial.Color = colorGrey;

            ///Create the default wall type.
            ///First check if it exist.
            WallType existWt = new FilteredElementCollector(activeDoc)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_Walls)
                .Select(e => e as WallType)
                .Where(w => w.Name == "AutoWall-240")
                .ToList().FirstOrDefault();
            ///If not exist,create a new one.
            WallType baseWt = (existWt != null) ?
                existWt : new FilteredElementCollector(activeDoc)
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
            AutoWallTypes = new List<WallType>();
            AutoWallTypes.Add(baseWt);
        }

        public bool LoadOpeningFamilies(Document doc)
        {
            Family doorFam = null;
            Family windowFam = null;
            
            ///Get the path.
            Assembly a = Assembly.GetExecutingAssembly();
            string rootFolder = Path.GetDirectoryName(a.Location);
            foreach (string doorName in autoDoorFamilyNames)
            {
                string DoorPath = rootFolder
                + "\\" + doorName + ".rfa";
                if (!Helper.LoadFamily(doc, doorName ,DoorPath, out doorFam))
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
                if (!Helper.LoadFamily(doc, windowName, windowPath, out windowFam))
                {
                    TaskDialog.Show("错误", "部分默认窗族丢失，请重新安装插件");
                    return false;
                }
                AutoWindowFamilies.Add(windowFam);
            }
            return true;
        }

        public bool LoadSocketsFamilies(Document doc)
        {
            Family socketFam = null;
            Assembly a = Assembly.GetExecutingAssembly();
            string rootFolder = Path.GetDirectoryName(a.Location);
            foreach (string socketName in autoSocketsNames)
            {
                string DoorPath = rootFolder
                + "\\" + socketName + ".rfa";
                if (!Helper.LoadFamily(doc, socketName, DoorPath, out socketFam))
                {
                    TaskDialog.Show("错误", "部分默认门族丢失，请重新安装插件");
                    return false;
                }
                AutoSocketFamilies.Add(socketFam);
            }
            return true;
        }
        #endregion

        #region The main work.
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
                        ///Duplicate a new walltype;
                        float wallWidthMm = Helper.Feet2Mm(wa.Thickness);
                        currentWt = AutoWallTypes[0].Duplicate
                            ("AutoWall-" + wallWidthMm) as WallType;
                        ///Set the width of the new type;
                        CompoundStructure cStru = CompoundStructure
                            .CreateSingleLayerCompoundStructure
                            (MaterialFunctionAssignment.Structure,
                            wa.Thickness, currentWt.GetCompoundStructure().GetMaterialId(0));
                        currentWt.SetCompoundStructure(cStru);
                        ///Add it to collection.
                        AutoWallTypes.Add(currentWt);
                    }

                    ///Create the individual wall
                    wa.Wall = Wall.Create(doc, c, currentWt.Id, baseLevel.Id,
                        floor.Height, 0, false, true);

                    activeForm.UpdateProgress(1);
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
                        (doc, f, d, BaseLevel, doorFam,
                        BuiltInCategory.OST_Doors);

                    ///Check the direction.
                    if (!d.Instance.FacingOrientation
                        .IsAlmostEqualTo(d.FacingOrientation))
                    {
                        d.Instance.flipFacing();
                        doc.Regenerate();
                    }
                    if (!d.Instance.HandOrientation
                        .IsAlmostEqualTo(d.HandOrientation))
                    {
                        d.Instance.flipHand();
                        doc.Regenerate();
                    }
                    doc.Regenerate();

                    activeForm.UpdateProgress(1);
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

                    activeForm.UpdateProgress(1);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
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
                openingSymbol.GetParameters("宽度")[0].Set(doorW);
                openingSymbol.GetParameters("高度")[0].Set(doorH);
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

            CreateWalls(activeDoc, CurrentHouse, AutoWallTypes, BaseLevel);

            return true;
        }
        public bool DoCreateOpenings()
        {
            if (!this.LoadOpeningFamilies(activeDoc))
                return false;

            CreateOpenings(activeDoc, CurrentHouse, BaseLevel,
                AutoDoorFamilies, AutoWindowFamilies);

            return true;
        }
        public bool DoCreateSockets()
        {
            this.LoadSocketsFamilies(activeDoc);

            Document doc = this.activeDoc;
            List<A_Socket> allSockets = 
                CurrentHouse.Floors
                .SelectMany(f => f.Socket)
                .ToList();
            List<A_Wall> allWalls =
                CurrentHouse.Floors
                .SelectMany(f => f.Walls)
                .ToList();

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

                List<Reference> sideFaces =
                    HostObjectUtils.GetSideFaces
                    (hostWall, ShellLayerType.Exterior)
                    .ToList();
                sideFaces.AddRange(
                    HostObjectUtils.GetSideFaces
                    (hostWall, ShellLayerType.Interior)
                    .ToList());

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
                FamilySymbol socSymbol = 
                    doc.GetElement
                    (socFam.GetFamilySymbolIds().First()) 
                    as FamilySymbol;

                socSymbol.Activate();
                soc.Instance = doc.Create
                    .NewFamilyInstance
                    (hostFace, centerPt,dirPt,socSymbol);

                doc.Regenerate();
                activeForm.UpdateProgress(1);
            }
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
                    this.activeForm = InputJsonForm;
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
