#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.IO;
using System.Windows.Forms;
#endregion

namespace TransferData_XJ
{
    [Transaction(TransactionMode.Manual)]
    public class CmdReadJson : IExternalCommand
    {
        /// <summary>
        /// Store the current house object.
        /// Contain all the info from input json file.
        /// </summary>
        private HouseObjects CurrentHouse 
        { 
            get { return currentHouse; } 
            set 
            { 
                currentHouse = value;
                this.TransferDataUnits(currentHouse);
            } 
        }
        private HouseObjects currentHouse;
        /// <summary>
        /// Store all the WallTypes already created.
        /// </summary>
        private List<WallType> AutoWallTypes { get; set; }

        #region Unit conversion part.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mms"></param>
        /// <returns></returns>
        public double MmToInch(double mms)
        {
            return UnitUtils.ConvertToInternalUnits
                (mms, DisplayUnitType.DUT_MILLIMETERS);
        }
        public double? MmToInch(double? mms)
        {
            if (mms == null) return null;
            return MmToInch((double)mms);
        }
        /// <summary>
        /// The method to convert all mm to inch in the house object.
        /// </summary>
        /// <param name="house"></param>
        private void TransferDataUnits(HouseObjects house)
        {
            foreach (A_Floor floor in house.Floors)
            {
                foreach (A_Wall wall in floor.Walls)
                {
                    wall.P1.X = MmToInch(wall.P1.X);
                    wall.P1.Y = MmToInch(wall.P1.Y);
                    wall.P2.X = MmToInch(wall.P2.X);
                    wall.P2.Y = MmToInch(wall.P2.Y);
                    wall.Thickness = MmToInch(wall.Thickness);
                    wall.Height = MmToInch(wall.Height);
                }
                foreach (A_Door door in floor.Doors)
                {
                    door.P1.X = MmToInch(door.P1.X);
                    door.P1.Y = MmToInch(door.P1.Y);
                    door.P2.X = MmToInch(door.P2.X);
                    door.P2.Y = MmToInch(door.P2.Y);
                    door.Height = MmToInch(door.Height);
                    door.SillHeight = MmToInch(door.SillHeight);
                }
                foreach (A_Window window in floor.Windows)
                {
                    window.P1.X = MmToInch(window.P1.X);
                    window.P1.Y = MmToInch(window.P1.Y);
                    window.P2.X = MmToInch(window.P2.X);
                    window.P2.Y = MmToInch(window.P2.Y);
                    window.Height = MmToInch(window.Height);
                    window.SillHeight = MmToInch(window.SillHeight);
                    window.BayDepth = MmToInch(window.BayDepth);
                }
                foreach (A_Cube cube in floor.Cubes)
                {
                    cube.X = MmToInch(cube.X);
                    cube.Y = MmToInch(cube.Y);
                    cube.XSize = MmToInch(cube.XSize);
                    cube.YSize = MmToInch(cube.YSize);
                    cube.Z = MmToInch(cube.Z);
                    cube.ZSize = MmToInch(cube.ZSize);
                }
                foreach (A_Room room in floor.Rooms)
                {
                    foreach (A_Contour contour in room.Meta.Contours)
                    {
                        contour.P1.X = MmToInch(contour.P1.X);
                        contour.P1.Y = MmToInch(contour.P1.Y);
                        contour.P2.X = MmToInch(contour.P2.X);
                        contour.P2.Y = MmToInch(contour.P2.Y);
                    }
                    foreach (A_Contour contour in room.Meta.CubeContours)
                    {
                        contour.P1.X = MmToInch(contour.P1.X);
                        contour.P1.Y = MmToInch(contour.P1.Y);
                        contour.P2.X = MmToInch(contour.P2.X);
                        contour.P2.Y = MmToInch(contour.P2.Y);
                    }
                }
                foreach (A_Label label in floor.Labels)
                {
                    label.Position.X = MmToInch(label.Position.X);
                    label.Position.Y = MmToInch(label.Position.Y);
                }
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

            WallType baseWt = null;
            Level baseLevel = null;
            ///Using input form to read json file into current house object.
            using (InputForm InputJsonForm = new InputForm())
            {
                ///List wall type.
                FilteredElementCollector baseWtCol = 
                    new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .OfCategory(BuiltInCategory.OST_Walls);
                foreach (Element e in baseWtCol)
                {
                    WallType wt = e as WallType;
                    InputJsonForm.WallTypeBox.Items.Add(wt.Name);
                }
                ///List levels.
                FilteredElementCollector levelCol =
                    new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_Levels);
                foreach (Element e in levelCol)
                {
                    Level l = e as Level;
                    InputJsonForm.LevelBox.Items.Add(l.Name);
                }
                do
                {
                    if (InputJsonForm.ShowDialog() == DialogResult.OK)
                    {
                        CurrentHouse = InputJsonForm.CurrentHouse;
                        if ((InputJsonForm.WallTypeBox.SelectedItem != null)&&
                                (InputJsonForm.LevelBox.SelectedItem != null))
                        {
                            foreach (Element e in baseWtCol)
                            {
                                WallType wt = e as WallType;
                                if (wt.Name == InputJsonForm
                                    .WallTypeBox.SelectedItem as string)
                                {
                                    baseWt = wt;
                                    break;
                                }
                            }
                            foreach (Element e in levelCol)
                            {
                                Level l = e as Level;
                                if (l.Name == InputJsonForm
                                    .LevelBox.SelectedItem as string)
                                {
                                    baseLevel = l;
                                    break;
                                }
                            }
                            InputJsonForm.Close();
                        }
                        else
                        {
                            MessageBox.Show("Please choose a wall type and a level");
                        }
                    }
                } while (InputJsonForm.IsAccessible);
                
            }
  
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");

                ///Create the default wall type.
                AutoWallTypes = new List<WallType>();
                baseWt = baseWt.Duplicate("AutoWall-240") as WallType;
                ElementId autoMaterial = Material.Create(doc, "AutoWallMaterial");
                baseWt.SetCompoundStructure(
                    CompoundStructure.CreateSingleLayerCompoundStructure
                    (MaterialFunctionAssignment.Structure,
                    UnitUtils.ConvertToInternalUnits(240, DisplayUnitType.DUT_MILLIMETERS),
                    autoMaterial)
                    );
                ///Create the wallType list and add the base type.
                AutoWallTypes = new List<WallType>();
                AutoWallTypes.Add(baseWt);


                foreach (A_Floor floor in CurrentHouse.Floors)
                {
                    foreach (A_Wall wall in floor.Walls)
                    {
                        WallType currentWt = null;
                        XYZ p1 = new XYZ(wall.P1.X, wall.P1.Y, 0);
                        XYZ p2 = new XYZ(wall.P2.X, wall.P2.Y, 0);
                        Curve c = Line.CreateBound(p1, p2);
                        ///Find the right wall type.
                        foreach (WallType wt in AutoWallTypes)
                        {
                            if (wt.Width == wall.Thickness)
                            {
                                currentWt = wt;
                                break;
                            }
                        }
                        ///If the wall type doesnot exist,create it.
                        if (currentWt == null)
                        {
                            ///Duplicate a new walltype;
                            double wallWidthMm = UnitUtils.ConvertFromInternalUnits
                                (wall.Thickness, DisplayUnitType.DUT_MILLIMETERS);
                            currentWt = AutoWallTypes[0].Duplicate
                                ("AutoWall-"+wallWidthMm) as WallType;
                            ///Set the width of the new type;
                            CompoundStructure cStru = CompoundStructure
                                .CreateSingleLayerCompoundStructure
                                (MaterialFunctionAssignment.Structure,
                                wall.Thickness,autoMaterial);
                            currentWt.SetCompoundStructure(cStru);
                            ///Add it to collection.
                            AutoWallTypes.Add(currentWt);
                        }
                        Wall.Create(doc,new List<Curve> { c }, currentWt.Id, baseLevel.Id, false);

                    }
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }
        private void CreateWall(Document doc,A_Wall wallData)
        {
            List<Curve> wallCurve = new List<Curve>();
            XYZ p1 = new XYZ(wallData.P1.X, wallData.P2.Y, 0);
            XYZ p2 = new XYZ();
            foreach (WallType wType in AutoWallTypes)
            {
                if (wallData.Thickness == wType.Width)
                {
                }
                
            }
            return ;
        }
    }
}
