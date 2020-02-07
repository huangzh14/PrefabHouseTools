using System;
using System.Collections.Generic;
using System.Collections;
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
    public struct Bcurve
    {
        public Curve Curve { get; }
        public Curve BaseWallCurve { get; }
        public bool BaseIsWall { get; }
        public ElementId Id { get; }
        public double Length { get; }
        public Bcurve(Curve curve, ElementId id, bool baseIsWall, Curve baseCurve)
        {
            Curve = curve;
            Id = id;
            BaseIsWall = baseIsWall;
            BaseWallCurve = baseCurve;
            Length = curve.Length;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RoomInfo
    {
        public Room Room { get; }
        ///Hold the adjacent rooms to this one.
        public List<RoomInfo> AdjacentRooms { get; set; }
        public override string ToString()
        {
            return Room.Name;
        }
        public List<List<Bcurve>> BoundaryList { get; set; }
        public List<List<XYZ>> VertexList { get; set; }
        
        public RoomInfo(Room room)
        {
            Room = room;
            BoundaryList = GetBoundary
                (room,out List<List<XYZ>>vList);
            VertexList = vList;
            AdjacentRooms = new List<RoomInfo>();
        }

        #region The method for boundary calculation.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cur1"></param>
        /// <param name="cur2"></param>
        /// <param name="mergedCur"></param>
        /// <returns></returns>
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
                            cur1.Id,cur1.BaseIsWall,
                            cur1.BaseWallCurve);
                        return true;
                    }
                    else if (lin1e.IsAlmostEqualTo(lin2s))
                    {
                        mergedCur = new Bcurve(
                            Line.CreateBound(lin1s, lin2e),
                            cur1.Id,cur1.BaseIsWall,
                            cur1.BaseWallCurve);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Calculate the boundary of a room with elementId attached to it.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<List<Bcurve>> GetBoundary
            (Room room,out List<List<XYZ>> vertexList)
        {
            List<List<Bcurve>> boundaryList =
                new List<List<Bcurve>>();
            vertexList = new List<List<XYZ>>();

            IList<IList<BoundarySegment>> BSlist =
                room.GetBoundarySegments
                (new SpatialElementBoundaryOptions())
                as IList<IList<BoundarySegment>>;
            
            
            foreach (IList<BoundarySegment> BSegs in BSlist)
            {
                List<Bcurve> boundaries =
                    new List<Bcurve>();
                List<XYZ> vertexes = new List<XYZ>();
                Stack<Bcurve> boundCurs = new Stack<Bcurve>();
                Queue<Bcurve> cursMerged = new Queue<Bcurve>();
                foreach (BoundarySegment Seg in BSegs)
                {
                    Document doc = room.Document;
                    Element wallOrNo = doc.GetElement(Seg.ElementId);
                    ElementId wallId = doc.Settings.Categories
                        .get_Item(BuiltInCategory.OST_Walls).Id;
                    if (wallOrNo.Category.Id.Equals(wallId))
                    {
                        Wall w = wallOrNo as Wall;
                        LocationCurve wLoc = w.Location as LocationCurve;
                        boundCurs.Push(new Bcurve
                        (Seg.GetCurve(), Seg.ElementId,true,wLoc.Curve));
                    }
                    else
                    {
                        boundCurs.Push(new Bcurve
                        (Seg.GetCurve(), Seg.ElementId,false,Seg.GetCurve()));
                    }
                    
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
                cursMerged.Reverse();
                #endregion 
                foreach (Bcurve bcur in cursMerged)
                {
                    boundaries.Add(bcur);
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
            foreach(List<Bcurve> boundary in BoundaryList)
            {
                foreach (Bcurve c in boundary)
                {
                    curves.Append(c.Curve);
                }
            }
            return curves;
        }
        #endregion

        #region Methods for solving adjacency.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="baseCurve"></param>
        /// <param name="interCurve"></param>
        /// <returns></returns>
        public bool ProjectIntersectCurve(Curve c1,Curve c2,Curve baseCurve,out Curve interCurve)
        {
            interCurve = baseCurve.Clone();
            double[] rcp1 = {baseCurve.Project(c1.GetEndPoint(0)).Parameter
                           ,baseCurve.Project(c1.GetEndPoint(1)).Parameter };
            double[] rcp2 = {baseCurve.Project(c2.GetEndPoint(0)).Parameter
                           ,baseCurve.Project(c2.GetEndPoint(1)).Parameter };
            var cp1 = rcp1.OrderBy(n => n).ToArray();
            var cp2 = rcp2.OrderBy(n => n).ToArray();
            if (cp1[0] > cp2[0])
            {
                double[] cpt = cp1.Clone() as double[];
                cp1 = cp2;
                cp2 = cpt;
            }
            if ((cp1[0] == cp1[1])||(cp2[0] == cp2[1])
                || (cp1[1] <= cp2[0]))
                return false;
            else if (cp1[1] <= cp2[1])
                interCurve.MakeBound(cp2[0], cp1[1]);
            else
                interCurve.MakeBound(cp2[0], cp2[1]);
            return true;

        }

        /// <summary>
        /// Return whether two room are adjacent.
        /// Must calculate the boundary info 
        /// </summary>
        /// <param name="room1"></param>
        /// <param name="room2"></param>
        /// <returns></returns>
        public bool IsAdjacentTo(RoomInfo otherRoom,out CurveArray adjCurves)
        {
            adjCurves = new CurveArray();
            bool isAdjacent = false;
            foreach(List<Bcurve> boundLoop1 in BoundaryList){
                foreach(List<Bcurve> boundLoop2 
                    in otherRoom.BoundaryList){
                    //First iterate through each boundary loop.
                    foreach(Bcurve bound1 in boundLoop1){
                        foreach(Bcurve bound2 in boundLoop2){
                            ///Then iterate through each boundary segment.
                            ///If they are the same element carry on.
                            if ((bound1.Id == bound2.Id)&&(bound1.BaseIsWall))
                            {
                                #region The old method.
                                /*
                                ///Project the two endpoint of the shorter one
                                ///onto the long one.If any projection point is
                                ///inbetween the vertexs of the longer one than
                                ///we can say this two rooms are adjacent.
                                Curve shortc = 
                                    (bound1.Length > bound2.Length)?
                                    bound2.Curve:bound1.Curve;
                                Curve longc = 
                                    (bound1.Length > bound2.Length)?
                                    bound1.Curve:bound2.Curve;
                                List<XYZ> ptsL = new List<XYZ>
                                    {shortc.GetEndPoint(0),shortc.GetEndPoint(1)};
                                ptsL.Add(0.5 * (ptsL[0] + ptsL[1]));
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
                                */
                                #endregion
                                Curve baseCurve = bound1.BaseWallCurve;
                                Curve c1 = bound1.Curve;
                                Curve c2 = bound2.Curve;
                                if (ProjectIntersectCurve(c1,c2,baseCurve,out Curve adjC))
                                {
                                    isAdjacent = true;
                                    adjCurves.Append(adjC);
                                }   
                            }
                        }
                    }
                }
            }
            return isAdjacent;
        }
        #endregion

        /// <summary>
        /// Calculate the adjacent relationship between several rooms.
        /// This will clear the existing adjacency relationship first.
        /// </summary>
        /// <param name="rooms"></param>
        public virtual void SolveAdjacency(List<RoomInfo> rooms)
        {
            int num = rooms.Count;
            for (int i = 0; i < num; i++)
            {
                rooms[i].AdjacentRooms.Clear();
            }
            for (int i = 0; i < num; i++)
            {
                for (int j = i + 1; j < num; j++)
                {
                    if (rooms[i].IsAdjacentTo(rooms[j],out CurveArray ca))
                    {
                        rooms[i].AdjacentRooms.Add(rooms[j]);
                        rooms[j].AdjacentRooms.Add(rooms[i]);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class RoomInfoElec : RoomInfo
    {
        
        ///The electrical fixtures in the room.
        public List<FixtureE> ElecFixtures { get; set; }
        ///The average centroid of all the fixtures
        ///in the room
        public XYZ FixCentroid { get; set; }

        public RoomInfoElec(Room room) : base(room)
        {
            ElecFixtures = new List<FixtureE>();
            FixCentroid = new XYZ();
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
            foreach (List<Bcurve> boundLoop
                in BoundaryList)
            {
                foreach (Bcurve bCur in boundLoop)
                {
                    if (IsPlanIntersect(c, bCur.Curve))
                        return true;
                }
            }
            return false;
        }

        public void CalculateFixCentroid()
        {
            if (ElecFixtures.Count > 0)
            {
                double aX = ElecFixtures.Average(ef => ef.Origin.X);
                double aY = ElecFixtures.Average(ef => ef.Origin.Y);
                FixCentroid = new XYZ(aX, aY, 0);
            }
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

    public class ElecSystemInfo
    {
        public string Name { get; }
        public List<FixtureE> ElecFixtures { get; set; }
        public ElecSystemInfo(ElectricalSystem system)
        {
            Name = system.Name;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class FixtureE
    {
        public Room Room { get; }
        public ElementId ElementId { get; }
        public MEPModel MepModel { get; }
        public XYZ Origin { get; }
        public Connector EConnector { get; }
        public FixtureE(FamilyInstance fa)
        {
            this.Room = fa.Room;
            this.ElementId = fa.Id;
            this.MepModel = fa.MEPModel;
            IEnumerator connectors = this.MepModel
                .ConnectorManager.Connectors
                .GetEnumerator();
            while (connectors.MoveNext())
            {
                Connector c = connectors
                    .Current as Connector;
                try
                {
                    ElectricalSystemType est =
                        c.ElectricalSystemType;
                    EConnector = c;
                    Origin = c.Origin;
                    break;
                }
                catch
                {
                    continue;
                }
            }
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public class PathE
    {
        public FixtureE Begin { get; }
        public FixtureE End { get; }
        public List<XYZ> Vertices { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class PathEcrossWall : PathE
    {
        public void MoveNext()
        {

        }
    }

}
