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
#endregion

namespace PrefabHouseTools
{
    [Transaction(TransactionMode.Manual)]
    public class CmdReadJson : IExternalCommand
    {
        /// <summary>
        /// Store the current house object.
        /// Contain all the info from input json file.
        /// </summary>
        private HouseObject CurrentHouse 
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
            }
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

            #region Step1 Get input
            Level baseLevel = null;
            ///Using input form to read json file into current house object.
            try
            {
                using (CmdReadJsonForm InputJsonForm = new CmdReadJsonForm())
                {
                    ///List levels.
                    List<Level> levels =
                        new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .OfCategory(BuiltInCategory.OST_Levels)
                        .Select(e => e as Level).ToList();
                    foreach (Level l in levels)
                    {
                        InputJsonForm.LevelBox.Items.Add(l.Name);
                    }

                    if (InputJsonForm.ShowDialog() == DialogResult.OK)
                    {
                     ///Get the house info data.
                        CurrentHouse = InputJsonForm.CurrentHouse;
                        ///Set the base walltype and level according 
                        ///to form selection.
                        baseLevel = levels
                            .Where(l => l.Name ==
                            InputJsonForm.LevelBox.SelectedItem as string)
                            .First();
                        ///Close the form.
                        InputJsonForm.Close();
                    }

                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", "Something went wrong," +
                    "details as follow:\n" + e.Message);
            }
            #endregion

            ///In case the user close the form.
            if (CurrentHouse == null) return Result.Failed;

            #region Step2 Create walls and floors.
            ///The base info used for wa creation.
            WallType baseWt = new FilteredElementCollector(doc)
                        .WhereElementIsElementType()
                        .OfCategory(BuiltInCategory.OST_Walls)
                        .Select(e => e as WallType)
                        .Where(w => w.Kind == WallKind.Basic)
                        .FirstOrDefault(); ;
            ElementId baseMaterialid = null;
            Material baseMaterial = null;
            Color colorGrey = new Color(80, 80, 80);
            ///Create walls.
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create walls.");

                ///Create the default material.
                Material existMa = new FilteredElementCollector(doc)
                    .OfClass(typeof(Material))
                    .Select(e => e as Material).ToList()
                    .Where(m => m.Name == "AutoWallMaterial")
                    .FirstOrDefault();
                baseMaterialid = (existMa != null) ? existMa.Id :
                    Material.Create(doc, "AutoWallMaterial");
                baseMaterial = doc.GetElement(baseMaterialid) as Material;

                baseMaterial.SurfaceForegroundPatternColor = colorGrey;
                baseMaterial.SurfaceBackgroundPatternColor = colorGrey;
                baseMaterial.Color = colorGrey;

                ///Create the default wa type.
                AutoWallTypes = new List<WallType>();
                WallType wt = new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .Select(e => e as WallType)
                    .Where(w => w.Name == "AutoWall-240")
                    .ToList().FirstOrDefault();
                baseWt = (wt != null) ? 
                    wt : baseWt.Duplicate("AutoWall-240") as WallType;
                baseWt.SetCompoundStructure(
                    CompoundStructure.CreateSingleLayerCompoundStructure
                    (MaterialFunctionAssignment.Structure,
                    UnitUtils.ConvertToInternalUnits(240, DisplayUnitType.DUT_MILLIMETERS),
                    baseMaterialid)
                    );

                ///Create the wallType list and add the base type.
                AutoWallTypes = new List<WallType>();
                AutoWallTypes.Add(baseWt);

                foreach (A_Floor floor in CurrentHouse.Floors)
                {
                    ///Create the walls.
                    foreach (A_Wall wa in floor.Walls)
                    {
                        WallType currentWt = null;
                        XYZ p1 = new XYZ(wa.P1.X, wa.P1.Y, 0);
                        XYZ p2 = new XYZ(wa.P2.X, wa.P2.Y, 0);
                        Curve c = Line.CreateBound(p1, p2);

                        ///Find the right wall type.
                        foreach (WallType wallT in AutoWallTypes)
                        {
                            if ((wallT.Width - wa.Thickness)<0.0001)
                            {
                                currentWt = wallT;
                                break;
                            }
                        }
                        ///If the wall type doesnot exist,create it.
                        if (currentWt == null)
                        {
                            ///Duplicate a new walltype;
                            float wallWidthMm = Helper.Feet2Mm(wa.Thickness);
                            currentWt = AutoWallTypes[0].Duplicate
                                ("AutoWall-"+wallWidthMm) as WallType;
                            ///Set the width of the new type;
                            CompoundStructure cStru = CompoundStructure
                                .CreateSingleLayerCompoundStructure
                                (MaterialFunctionAssignment.Structure,
                                wa.Thickness,baseMaterialid);
                            currentWt.SetCompoundStructure(cStru);
                            ///Add it to collection.
                            AutoWallTypes.Add(currentWt);
                        }

                        ///Create the individual wall
                        wa.Wall = Wall.Create(doc, c, currentWt.Id, baseLevel.Id,
                            floor.Height, 0, false, true);  
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

                
                tx.Commit();
            }
            #endregion

            #region Step3 Create doors

            Family a_DoorF = null;
            const string doorFamilyName = "auto-Door";
            Family a_WindowF = null;
            const string windowFamilyName = "auto-Window";

            ///Get the path.
            Assembly a = Assembly.GetExecutingAssembly();
            string rootFolder = Path.GetDirectoryName(a.Location);
            string autoDoorPath = rootFolder 
                + "\\" + doorFamilyName + ".rfa";
            string autoWindowPath = rootFolder 
                + "\\" + windowFamilyName + ".rfa";

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create Doors");
                ///Load the family file.
                if (!Helper.LoadFamily(doc,doorFamilyName,
                    autoDoorPath,out a_DoorF))
                    TaskDialog.Show("错误",
                        "部分默认族丢失，请重新安装插件");
                if (!Helper.LoadFamily(doc,windowFamilyName,
                    autoWindowPath,out a_WindowF))
                    TaskDialog.Show("错误", 
                        "部分默认族丢失，请重新安装插件");

                foreach (A_Floor f in CurrentHouse.Floors)
                {
                    foreach(A_Door d in f.Doors)
                    {
                        d.Instance = CreateOpening
                            (doc, f, d, baseLevel, a_DoorF, 
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
                    }
                    foreach(A_Window w in f.Windows)
                    {
                        w.Instance = CreateOpening
                            (doc, f, w, baseLevel, a_WindowF,
                            BuiltInCategory.OST_Windows);
                        doc.Regenerate();
                        w.Instance.flipFacing();
                        doc.Regenerate();
                        w.Instance.flipFacing();
                    }
                }
                tx.Commit();
            }
                
            #endregion

            return Result.Succeeded;
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
        public FamilyInstance CreateOpening
            (Document doc,A_Floor floor,A_Opening opening,Level baseLevel
            ,Family baseFamily,BuiltInCategory category)
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
    }
}
