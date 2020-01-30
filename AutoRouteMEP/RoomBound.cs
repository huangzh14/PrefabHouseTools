#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Electrical;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#endregion

namespace AutoRouteMEP
{
    public struct SLine
    {
        public XYZ StartPt { get; }
        public XYZ EndPt { get; }
        public XYZ Vector { get { return (EndPt - StartPt); } }
        public SLine(XYZ start,XYZ end) { 
            StartPt = start;
            EndPt = end; }
    }
    class RoomBound
    {
        public List<XYZ> Vertexs { get; }
        List<List<SLine>> borderList;
        Room theRoom;
        
        /// <summary>
        /// Constructor from room.
        /// </summary>
        /// <param name="room"></param>
        public RoomBound(Room room)
        {
            theRoom = room;
            //Get the border.
            List<List<BoundarySegment>> BSlist = 
                room.GetBoundarySegments
                (new SpatialElementBoundaryOptions())
                as List<List<BoundarySegment>>;
            List<SLine> borders = new List<SLine>();
            foreach(List<BoundarySegment>BSegments in BSlist)
            {
                foreach(BoundarySegment BSeg in BSegments)
                {
                    Curve tempC = BSeg.GetCurve();
                    borders.Add(new SLine
                        (tempC.GetEndPoint(0), tempC.GetEndPoint(1)));
                    Vertexs.Add(tempC.GetEndPoint(0));
                }
                borderList.Add(borders);
                borders.Clear();
            } 
        }

        /// <summary>
        /// Define whether the room boundary is a convex polygon
        /// </summary>
        /// <returns>True if the boundary is a convex polygon</returns>
        public bool IsConvex()
        {
            if (borderList.Count > 1)
            {
                return false;
            }
            List<SLine> borders = borderList[0];
            int borderNum = borders.Count();
            int counter = 0;
            ///Add the first border to the end to create a loop.
            ///Delete after work done.
            borders.Add(borders[0]);
            for (int i = 0; i < borderNum; i++)
            {
                XYZ crossProduct =
                    crossP(borders[i].Vector, borders[i + 1].Vector);
                if (crossProduct.Z > 0)
                    counter++;
            }
            borders.RemoveAt(borderNum);
            if ((counter == 0) || (counter == borderNum))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cross Product Calculator
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        private XYZ crossP (XYZ vec1,XYZ vec2)
        {
            double x = vec1.Y * vec2.Z - vec2.Y * vec1.Z;
            double y = vec1.Z * vec2.X - vec2.Z * vec1.X;
            double z = vec1.X * vec2.Y - vec2.X * vec1.Y;
            return new XYZ(x,y,z); 
        }

        /// <summary>
        /// Whether two line intersect with each other.
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        private bool isIntersect(SLine l1, SLine l2)
        {
            int counterUp = 0;
            int counterDown = 0;
            List<XYZ> pts = new List<XYZ>
                {l1.StartPt, l2.StartPt, l1.EndPt, l2.EndPt};
            pts.Add(pts[0]);
            for (int i = 0; i < 4; i++)
            {
                XYZ c = crossP(pts[i], pts[i + 1]);
                if (c.Z > 0) counterUp++;
                else counterDown++;
            }
            if ((counterUp == 4) || (counterDown == 4))
                return true;
            return false;
        }

        /// <summary>
        /// Return whether a line intersect with this boundary
        /// </summary>
        /// <param name="line"></param>
        /// <returns>True if have intersection.</returns>
        public bool BorderIntersectWith(SLine line)
        {
            foreach(List<SLine> borders in borderList)
            {
                foreach(SLine border in borders)
                {
                    if (isIntersect(border, line))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Calculate the route within the room
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <returns></returns>
        public List<XYZ> FindPlanRoute(XYZ start,XYZ end)
        {
            List<XYZ> route = new List<XYZ>();
            //The simple situation.
            SLine line = new SLine(start, end);
            if ((IsConvex())||
                (!BorderIntersectWith(line)))
            {
                route.Add(start);
                route.Add(end);
                return route;
            }
            //The trick situation.
            do
            {

            } while ();

            return route;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<XYZ> FindRoute(XYZ start,XYZ end)
        {
            List<XYZ> route = new List<XYZ>();

            return route;
        }
    }
}
