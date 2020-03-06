using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

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
        #endregion
    }
}
