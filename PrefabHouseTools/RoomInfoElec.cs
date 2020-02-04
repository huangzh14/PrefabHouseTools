using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace PrefabHouseTools
{
    class RoomInfoElec : RoomInfo
    {
        public RoomInfoElec(Room room) :base(room)
        {
        }
        /// <summary>
        /// Decide whether two line intersect in planview.
        /// By default the input curves are stright line.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        private bool IsPlanIntersect (Curve c1,Curve c2)
        {
            int iUp = 0;
            int iDown = 0;
            List<XYZ> pts = new List<XYZ>
            {c1.GetEndPoint(0),c2.GetEndPoint(0),
                c1.GetEndPoint(1),c2.GetEndPoint(1)};
            for (int i = 0; i < 4; i++)
            {
                pts[i] = new XYZ(pts[i].X, pts[i].Y, 0);
             
            }
            pts.Add(pts[0]);
            for (int i = 0; i < 4; i++)
            {
                XYZ cp = pts[i].CrossProduct(pts[i + 1]);
                if (cp.Z > 0) iUp++;
                else iDown++;
            }
            if ((iUp == 4) || (iDown == 4))
                return true;
            return false;
        }
        /// <summary>
        /// To decide whether a stright route intersect 
        /// with the boundary.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool BoundaryIntersectWith(Curve c)
        {
            foreach(Dictionary<Curve,ElementId> boundLoop 
                in BoundaryList)
            {
                foreach(Curve bCur in boundLoop.Keys)
                {
                    if (IsPlanIntersect(c, bCur))
                        return true;
                }
            }
            return false;
        }

        private struct Vertex
        {
            XYZ pt;
            double d2s;
            double d2e;
        }
        private List<XYZ> FindVertexPath(XYZ start,XYZ end, List<XYZ> vertex)
        {
            List<XYZ> path = new List<XYZ>();

        }
        /// <summary>
        /// Find the planer route of two points at given height.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public List<XYZ> FindPlanerRoute(XYZ start,XYZ end,double h)
        {
            List<XYZ> route = new List<XYZ>();
            Curve sLine = Line.CreateBound(start, end);
            ///The simple situation when stright line is ok.
            if (!BoundaryIntersectWith(sLine))
            {
                route.Add(new XYZ(start.X, start.Y, h));
                route.Add(new XYZ(end.X, end.Y, h));
                return route;
            }
            ///Thi tricky situation when stright line is not ok.
            
            return route;
        }
    }
}
