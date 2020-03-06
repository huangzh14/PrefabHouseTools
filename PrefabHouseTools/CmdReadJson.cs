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

            #region Step2 Create walls.
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

                ///Create the walls.
                foreach (A_Floor floor in CurrentHouse.Floors)
                {
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
                }
                tx.Commit();
            }
            #endregion

            #region Step3 Create doors

            Family a_DoorF = null;
            FamilySymbol a_DoorSymbol = null;
            const string autoDoorName = "auto-Door";
            const string doorW_Para = "宽度";
            const string doorH_Para = "高度";

            ///Get the path.
            Assembly a = Assembly.GetExecutingAssembly();
            string rootFolder = Path.GetDirectoryName(a.Location);
            string autoDoorPath = rootFolder + "\\"+autoDoorName +".rfa";

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create Doors");
                ///Load the family file.First check if already in the file.
                a_DoorF = Helper.FindElement
                    (doc, typeof(Family), autoDoorName) as Family;
                if (a_DoorF == null)
                {//Check if the file exist.
                    if (!File.Exists(autoDoorPath))
                    {
                        TaskDialog.Show("错误", "无法找到默认门族，请重新安装插件");
                        return Result.Failed;
                    }
                    doc.LoadFamily(autoDoorPath, out a_DoorF);
                }
                ///Get the base symbol to work with.
                a_DoorSymbol = doc.GetElement
                    (a_DoorF.GetFamilySymbolIds().First()) 
                    as FamilySymbol;

                ///Create the door.
                float doorW, doorH;
                int doorWmm,doorHmm;
                string doorTypeName;
                foreach (A_Floor f in CurrentHouse.Floors)
                {
                    foreach(A_Door d in f.Doors)
                    {
                        ///Find the host wall.
                        Wall hostW = f.Walls
                            .First(w => w.Uid == d.Meta.Wall).Wall;
                        ///Find the central point.
                        XYZ centerPt = new XYZ((d.P1.X + d.P2.X) / 2, 
                                        (d.P1.Y + d.P2.Y) / 2, 
                                        baseLevel.Elevation+d.SillHeight);
                        ///Calculate the width and height of the wall.
                        doorW = d.Width;
                        doorH = d.Height;
                        doorWmm = (int)Math.Round(Helper.Feet2Mm(doorW));
                        doorHmm = (int)Math.Round(Helper.Feet2Mm(doorH));
                        doorTypeName = "autoDoor-" + doorWmm + "-" + doorHmm;

                        ///Check if that type already exist.
                        ElementType existType = Helper.FindElementType
                            (doc, BuiltInCategory.OST_Doors, doorTypeName);
                        if (existType != null)
                        {
                            a_DoorSymbol = existType as FamilySymbol;
                        }
                        else
                        {
                            a_DoorSymbol = (FamilySymbol)
                                a_DoorSymbol.Duplicate(doorTypeName);
                            a_DoorSymbol.GetParameters(doorW_Para)[0].Set(doorW);
                            a_DoorSymbol.GetParameters(doorH_Para)[0].Set(doorH);
                        }

                        ///Create the door.
                        a_DoorSymbol.Activate();
                        d.Door = doc.Create.NewFamilyInstance
                            (centerPt, a_DoorSymbol, hostW, 
                            StructuralType.NonStructural);

                        ///Check the direction.
                        if (!d.Door.FacingOrientation
                            .IsAlmostEqualTo(d.FacingOrientation))
                        {
                            d.Door.flipFacing();
                            doc.Regenerate();
                        }
                        if (!d.Door.HandOrientation
                            .IsAlmostEqualTo(d.HandOrientation))
                        {
                            d.Door.flipHand();
                            doc.Regenerate();
                        }
                        doc.Regenerate();
                    }
                }
                tx.Commit();
            }
                
            #endregion

            return Result.Succeeded;
        }
    }
}
