﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.IO;

namespace PrefabHouseTools
{
    class Helper
    {
        #region Unit conversion method.
        public static float Mm2Feet(float mms)
        {
            return (float)UnitUtils.ConvertToInternalUnits
                (mms, DisplayUnitType.DUT_MILLIMETERS);
        }

        public static double Mm2Feet(double mms)
        {
            return UnitUtils.ConvertToInternalUnits
                (mms, DisplayUnitType.DUT_MILLIMETERS);
        }
        public static float? Mm2Feet(float? mms)
        {
            if (mms == null) return null;
            return Mm2Feet((float)mms);
        }
        public static double? Mm2Feet(double? mms)
        {
            if (mms == null) return null;
            return Mm2Feet((double)mms);
        }

        public static float Feet2Mm(float Feets)
        {
            return (float)UnitUtils.ConvertFromInternalUnits
                (Feets, DisplayUnitType.DUT_MILLIMETERS);
        }
        public static double Feet2Mm(double Feets)
        {
            return UnitUtils.ConvertFromInternalUnits
                (Feets, DisplayUnitType.DUT_MILLIMETERS);
        }
        public static float? Feet2Mm(float? feets)
        {
            if (feets == null) return null;
            return Feet2Mm((float)feets);
        }
        public static double? Feet2Mm(double? feets)
        {
            if (feets == null) return null;
            return Feet2Mm((double)feets);
        }
        #endregion

        #region Find element or element type by name.
        public static Element FindElement
           (Document doc, Type targetType, string targetName)
        {
            try
            {
                return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(targetType)
                .First(e => e.Name.Equals(targetName));
            }
            catch
            {
                return null;
            } 
        }
        public static Element FindElement
           (Document doc, BuiltInCategory category, string targetName)
        {
            try
            {
                return new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(category)
                    .First(e => e.Name.Equals(targetName));
            }
            catch
            {
                return null;
            }
        }
        public static ElementType FindElementType
           (Document doc, Type targetType, string targetName)
        {
            try
            {
                return new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .OfClass(targetType)
                    .First(e => e.Name.Equals(targetName)) as ElementType;
            }
            catch
            {
                return null;
            }
        }
        public static ElementType FindElementType
           (Document doc, BuiltInCategory category, string targetName)
        {
            try
            {
                return new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .OfCategory(category)
                    .First(e => e.Name.Equals(targetName)) as ElementType;
            }
            catch
            {
                return null;
            }
        }
        public static FamilySymbol FindFamilySymbol
           (Document doc, BuiltInCategory category, string symbolName,string familyName)
        {
            try
            {
                return new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .OfCategory(category)
                    .Where(e => e.Name.Equals(symbolName))
                    .Select(e => e as FamilySymbol)
                    .First(s => s.Family.Name.Equals(familyName));
            }
            catch
            {
                return null;
            }
        }
        public static List<ElementType> FindElementTypes
            (Document doc, BuiltInCategory category, string targetName)
        {
            try
            {
                return new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .OfCategory(category)
                    .Where(e => e.Name.Equals(targetName))
                    .Select(e => e as ElementType)
                    .ToList();
            }
            catch
            {
                return null;
            }
        }

        public static bool LoadFamily(Document doc,string familyName,string rfaPath,out Family fa)
        {
            fa = Helper.FindElement
                    (doc, typeof(Family), familyName) as Family;
            if (fa == null)
            {
                if (!doc.LoadFamily(rfaPath, out fa))
                    return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// 生成一个含有给定数目颜色的颜色序列
        /// Generate a pallete containing a given number of colors.
        /// </summary>
        /// <param name="colorNumber"></param>
        /// <returns></returns>
        public static List<Color> ColorPallet(int colorNumber)
        {
            List<Color> currentList = new List<Color>();
            double segNum = Math.Ceiling(Math.Pow(colorNumber,0.333333));

            List<byte> RGBindex = new List<byte>();
            for (int i = 0; i < segNum; i++)
            {
                RGBindex.Add((byte)Math.Floor(255 * i / (segNum - 1)));
            }

            int colorCounter = 0;
            for (int i = 0; i < segNum; i++)
            {
                for (int j = 0; j < segNum; j++)
                {
                    for (int k = 0; k < segNum; k++)
                    {
                        currentList.Add
                            (new Color(RGBindex[i], RGBindex[j], RGBindex[k]));
                        colorCounter++;
                        if (colorCounter == colorNumber) break;
                    }
                    if (colorCounter == colorNumber) break;
                }
                if (colorCounter == colorNumber) break;
            }
            return currentList;
        }
    }
}
