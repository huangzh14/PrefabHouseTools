using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace PrefabHouseTools
{
    /// <summary>
    /// 
    /// </summary>
    public class RoomInfo
    {
        public Room Room { get; }
        public override string ToString()
        {
            return Room.Name;
        }
        public List<Dictionary<Curve, ElementId>> BoundaryList { get; set; }
        public List<List<XYZ>> VertexList { get; set; }
        public RoomInfo(Room room)
        {
            Room = room;
            BoundaryList = GetBoundary
                (room,out List<List<XYZ>>vList);
            VertexList = vList;
        }
        
        #region Supporting method for the following method.
        public struct Bcurve
        {
            public Curve Curve;
            public ElementId Id;
            public Bcurve(Curve curve,ElementId id)
            {
                Curve = curve;
                Id = id;
            }
        }
        public static bool Merge(Bcurve cur1,Bcurve cur2,out Bcurve mergedCur)
        {
            mergedCur = new Bcurve();
            if ((cur1.Id == cur2.Id)&&(cur1.Curve is Line)
                    &&(cur2.Curve is Line))
            {
                Line lin1 = cur1.Curve as Line;
                Line lin2 = cur2.Curve as Line;
                if ((lin1.Direction.Normalize().IsAlmostEqualTo
                    (lin2.Direction.Normalize()))&&
                        (lin1.IsBound)&&(lin2.IsBound))
                {
                    XYZ lin1s = lin1.GetEndPoint(0);
                    XYZ lin1e = lin1.GetEndPoint(1);
                    XYZ lin2s = lin2.GetEndPoint(0);
                    XYZ lin2e = lin2.GetEndPoint(1);
                    if (lin1s.IsAlmostEqualTo(lin2e))
                    {
                        mergedCur = new Bcurve
                            (Line.CreateBound(lin2s, lin1e),
                            cur1.Id);
                        return true;
                    }
                    else if (lin1e.IsAlmostEqualTo(lin2s))
                    {
                        mergedCur = new Bcurve(
                            Line.CreateBound(lin1s, lin2e),
                            cur1.Id);
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion
        /// <summary>
        /// Calculate the boundary of a room with elementId attached to it.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Dictionary<Curve,ElementId>> GetBoundary
            (Room room,out List<List<XYZ>> vertexList)
        {
            List<Dictionary<Curve, ElementId>> boundaryList =
                new List<Dictionary<Curve, ElementId>>();
            vertexList = new List<List<XYZ>>();

            IList<IList<BoundarySegment>> BSlist =
                room.GetBoundarySegments
                (new SpatialElementBoundaryOptions())
                as IList<IList<BoundarySegment>>;
            
            
            foreach (IList<BoundarySegment> BSegs in BSlist)
            {
                Dictionary<Curve, ElementId> boundaries =
                    new Dictionary<Curve, ElementId>();
                List<XYZ> vertexes = new List<XYZ>();
                Stack<Bcurve> boundCurs = new Stack<Bcurve>();
                Queue<Bcurve> cursMerged = new Queue<Bcurve>();
                foreach (BoundarySegment Seg in BSegs)
                {
                    boundCurs.Push(new Bcurve
                        (Seg.GetCurve(),Seg.ElementId));
                }
                #region Merge curves
                while (boundCurs.Count > 1)
                {
                    Bcurve cur1 = boundCurs.Pop();
                    Bcurve cur2 = boundCurs.Pop();
                    if (Merge(cur1,cur2,out Bcurve curM))
                    {
                        boundCurs.Push(curM);
                        continue;
                    }
                    boundCurs.Push(cur2);
                    cursMerged.Enqueue(cur1);
                }
                Bcurve cur3 = boundCurs.Pop();
                Bcurve cur4 = cursMerged.Peek();
                if (Merge(cur3,cur4,out Bcurve curveM))
                {
                    cursMerged.Dequeue();
                    cursMerged.Enqueue(curveM);
                }
                else
                {
                    cursMerged.Enqueue(cur3);
                }
                #endregion 
                foreach (Bcurve bcur in cursMerged)
                {
                    boundaries.Add(bcur.Curve, bcur.Id);
                    vertexes.Add(bcur.Curve.GetEndPoint(0));
                }
                boundaryList.Add(boundaries);
                vertexList.Add(vertexes);
            }
            return boundaryList;
        }
        /// <summary>
        /// Get a single list of the curves.
        /// </summary>
        /// <returns></returns>
        public CurveArray GetBoundaryCurves()
        {
            CurveArray curves = new CurveArray();
            foreach(Dictionary<Curve,ElementId> boundary in BoundaryList)
            {
                foreach (Curve c in boundary.Keys)
                {
                    curves.Append(c);
                }
            }
            return curves;
        }

        /// <summary>
        /// Return whether two room are adjacent.
        /// Must calculate the boundary info 
        /// </summary>
        /// <param name="room1"></param>
        /// <param name="room2"></param>
        /// <returns></returns>
        public bool IsAdjacentTo(RoomInfo otherRoom)
        {
            foreach(Dictionary<Curve,ElementId> boundLoop1 in BoundaryList){
                foreach(Dictionary<Curve,ElementId> boundLoop2 
                    in otherRoom.BoundaryList){
                    //First iterate through each boundary loop.
                    foreach(var bound1 in boundLoop1){
                        foreach(var bound2 in boundLoop2){
                            ///Then iterate through each boundary segment.
                            ///If they are the same element carry on.
                            if (bound1.Value == bound2.Value)
                            {
                                ///Project the two endpoint of the shorter one
                                ///onto the long one.If any projection point is
                                ///inbetween the vertexs of the longer one than
                                ///we can say this two rooms are adjacent.
                                Curve shortc = 
                                    (bound1.Key.Length > bound2.Key.Length)?
                                    bound2.Key:bound1.Key;
                                Curve longc = 
                                    (bound1.Key.Length > bound2.Key.Length)?
                                    bound1.Key:bound2.Key;
                                List<XYZ> ptsL = new List<XYZ>
                                    {shortc.GetEndPoint(0),shortc.GetEndPoint(1)};
                                foreach (XYZ p in ptsL)
                                {
                                    XYZ pt = longc.Project(p).XYZPoint;
                                    XYZ pt2s = pt - longc.GetEndPoint(0);
                                    XYZ pt2e = longc.GetEndPoint(1) - pt;
                                    if (pt2s.Normalize().IsAlmostEqualTo
                                        (pt2e.Normalize()))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class RoomInfoElec : RoomInfo
    {
        ///Hold the adjacent rooms to this one.
        public List<RoomInfoElec> AdjacentRooms { get; set; }
        ///The electrical fixtures in the room.
        public List<FixtureE> ElecFixtures { get; set; }
        ///The average centroid of all the fixtures
        ///in the room
        public XYZ FixCentroid { get; set; }

        public RoomInfoElec(Room room) : base(room)
        {
            AdjacentRooms = new List<RoomInfoElec>();
            ElecFixtures = new List<FixtureE>();
            FixCentroid = new XYZ();
        }

        /// <summary>
        /// Calculate the adjacent relationship between several rooms.
        /// This will clear the existing adjacency relationship first.
        /// </summary>
        /// <param name="rooms"></param>
        public static void SolveAdjacency(List<RoomInfoElec> rooms)
        {
            int num = rooms.Count;
            for (int i = 0; i < num; i++)
            {
                rooms[i].AdjacentRooms.Clear();
            }
            for (int i = 0; i < num; i++)
            {
                for (int j = i+1; j < num; j++)
                {
                    if (rooms[i].IsAdjacentTo(rooms[j]))
                    {
                        rooms[i].AdjacentRooms.Add(rooms[j]);
                        rooms[j].AdjacentRooms.Add(rooms[i]);
                    }
                }
            }
        }
        
        /// <summary>
        /// Decide whether two line intersect in planview.
        /// By default the input curves are stright line.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        private bool IsPlanIntersect(Curve c1, Curve c2)
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
            foreach (Dictionary<Curve, ElementId> boundLoop
                in BoundaryList)
            {
                foreach (Curve bCur in boundLoop.Keys)
                {
                    if (IsPlanIntersect(c, bCur))
                        return true;
                }
            }
            return false;
        }

        private List<XYZ> FindVertexPath(XYZ start, XYZ end, List<XYZ> vertex)
        {
            List<XYZ> path = new List<XYZ>();
            return path;
        }
        /// <summary>
        /// Find the planer route of two points at given height.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public List<XYZ> FindPlanerRoute(XYZ start, XYZ end, double h)
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
            ///The tricky situation when stright line is not ok.

            return route;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FixtureE
    {
        public ElementId ElementId { get; }
        MEPModel model;
        
    }

    /// <summary>
    /// 
    /// </summary>
    public class PathE
    {

    }

}
