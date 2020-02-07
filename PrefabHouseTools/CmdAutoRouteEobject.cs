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
    ///Important note:Here are some basic assumptions.
    ///If the project doesn't meet,unexpect result may occur.
    ///
    ///1-Room boundary are lines not curves.The program use
    ///Curve class for develop convenient,the logic all assume
    ///they are all lines.
    ///
    ///2-
  
    /// <summary>
    /// 
    /// </summary>
    public struct Bcurve
    {
        public Curve Curve { get; }
        public Curve BaseWallCurve { get; }
        public double BaseWallWidth { get; }
        public bool BaseIsWall { get; }
        public ElementId Id { get; }
        public double Length { get; }
        public Bcurve(Curve curve, ElementId id, bool baseIsWall, Curve baseCurve,double baseWith)
        {
            Curve = curve;
            Id = id;
            BaseIsWall = baseIsWall;
            BaseWallCurve = baseCurve;
            BaseWallWidth = baseWith;
            Length = curve.Length;
        }
        public Bcurve(Curve curve,Bcurve root)
        {
            Curve = curve;
            Length = curve.Length;
            Id = root.Id;
            BaseIsWall = root.BaseIsWall;
            BaseWallCurve = root.BaseWallCurve;
            BaseWallWidth = root.BaseWallWidth;
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

        #region The methodS for boundary calculation.
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
                            (Line.CreateBound(lin2s, lin1e),cur1);
                        return true;
                    }
                    else if (lin1e.IsAlmostEqualTo(lin2s))
                    {
                        mergedCur = new Bcurve(
                            Line.CreateBound(lin1s, lin2e),cur1);
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
                        (Seg.GetCurve(), Seg.ElementId,true,wLoc.Curve,w.Width));
                    }
                    else
                    {
                        boundCurs.Push(new Bcurve
                        (Seg.GetCurve(), Seg.ElementId,false,Seg.GetCurve(),-1));
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
        /// <param name="t"></param>
        /// <returns></returns>
        public bool ProjectIntersectCurve(Curve c1,Curve c2,Curve baseCurve,out Curve interCurve,out XYZ t)
        {
            interCurve = baseCurve.Clone();
            t = new XYZ();
            ///Project two curve onto base curve.Get the projection parameter.
            double[] rcp1 = {baseCurve.Project(c1.GetEndPoint(0)).Parameter
                           ,baseCurve.Project(c1.GetEndPoint(1)).Parameter };
            double[] rcp2 = {baseCurve.Project(c2.GetEndPoint(0)).Parameter
                           ,baseCurve.Project(c2.GetEndPoint(1)).Parameter };
            var cp1 = rcp1.OrderBy(n => n).ToArray();
            var cp2 = rcp2.OrderBy(n => n).ToArray();
            ///Make sure cp1 has a non-larger start.
            if (cp1[0] > cp2[0])
            {
                double[] cpt = cp1.Clone() as double[];
                cp1 = cp2;
                cp2 = cpt;
            }
            ///Compare two domain to decide if intersect area exist.
            if ((cp1[0] == cp1[1])||(cp2[0] == cp2[1])
                || (cp1[1] <= cp2[0]))
                return false;
            else if (cp1[1] <= cp2[1])
                interCurve.MakeBound(cp2[0], cp1[1]);
            else
                interCurve.MakeBound(cp2[0], cp2[1]);
            ///Get the vector to move the basecurve to the center or c1&c2.
            XYZ basePt = interCurve.GetEndPoint(0);
            XYZ centPt = 0.5*(c1.Project(basePt).XYZPoint+c2.Project(basePt).XYZPoint);
            t = centPt - basePt;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherRoom"></param>
        /// <param name="adjCurves"></param>
        /// <param name="transform"></param>
        /// <param name="wallWidth"></param>
        /// <returns></returns>
        public bool IsAdjacentTo(RoomInfo otherRoom,
            out CurveArray adjCurves,out List<XYZ> transform,
            out List<double>wallWidth)
        {
            adjCurves = new CurveArray();
            transform = new List<XYZ>();
            wallWidth = new List<double>();
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
                                Curve baseCurve = bound1.BaseWallCurve;
                                Curve c1 = bound1.Curve;
                                Curve c2 = bound2.Curve;
                                if (ProjectIntersectCurve(c1,c2,baseCurve,out Curve adjC,out XYZ t))
                                {
                                    isAdjacent = true;
                                    adjCurves.Append(adjC);
                                    transform.Add(t);
                                    wallWidth.Add(bound1.BaseWallWidth);
                                }   
                            }
                        }
                    }
                }
            }
            return isAdjacent;
        }
        public bool IsAdjacentTo(RoomInfo otherRoom)
        {
            bool result = this.IsAdjacentTo(otherRoom, out CurveArray c,
                out List<XYZ> t, out List<double> w);
            return result;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// Note:All the boundary segment use curve for convenient.
    /// In the real scenarios most boundary are lines.And all the 
    /// relative calculation only work for line.If curve wall exist,
    /// unexpect result may occur.
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

        #region Methods for calculating intersection.
        /// <summary>
        /// Transfer a bound line to a fomula.
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        private void LineToPlanFomula(Curve cur,out double A,out double B,out double C)
        {
            XYZ pt1 = cur.GetEndPoint(0);
            XYZ pt2 = cur.GetEndPoint(1);
            A = pt2.Y - pt1.Y;
            B = pt1.X - pt2.X;
            C = A * pt1.X + B * pt1.Y;
        }
        /// <summary>
        /// Determine whether a point is on a line
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool PointIsOnLine(XYZ pt,Curve c)
        {
            XYZ c1 = c.GetEndPoint(0);
            XYZ c2 = c.GetEndPoint(1);
            XYZ p2c1 = (pt - c1).Normalize();
            XYZ p2c2 = (c2 - pt).Normalize();
            if (p2c1.IsAlmostEqualTo(p2c2))
                return true;
            return false;
        }

        /// <summary>
        /// Decide whether two line intersect in planview.
        /// By default the input curves are stright line.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        private bool IsPlanIntersect(Curve cur1, Curve cur2,out XYZ intersecPt)
        {
            intersecPt = new XYZ();
            LineToPlanFomula(cur1, out double A1, out double B1, out double C1);
            LineToPlanFomula(cur2, out double A2, out double B2, out double C2);
            double det = A1 * B2 - A2 * B1;
            if (Math.Abs(det) < 0.00001)
            {
                //Lines are parallel or almost.
                return false;
            }
            double intX = (B2 * C1 - B1 * C2) / det;
            double intY = (A1 * C2 - A2 * C1) / det;
            intersecPt = new XYZ(intX, intY, 0);
            if (PointIsOnLine(intersecPt, cur1) && (PointIsOnLine(intersecPt, cur2)))
                return true;
            else return false;
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
                    if (IsPlanIntersect(c, bCur.Curve,out XYZ iPt))
                        return true;
                }
            }
            return false;
        }
        #endregion

        public void CalculateFixCentroid()
        {
            if (ElecFixtures.Count > 0)
            {
                double aX = ElecFixtures.Average(ef => ef.Origin.X);
                double aY = ElecFixtures.Average(ef => ef.Origin.Y);
                FixCentroid = new XYZ(aX, aY, 0);
            }
        }
        public bool AdjacentPathTo(RoomInfoElec otherRoom, 
            out PathExWall path,out double roughL)
        {
            path = new PathExWall();
            roughL = 0;
            bool boolResult = 
                base.IsAdjacentTo(otherRoom, 
                out CurveArray adjCurves,
                out List<XYZ> transfList,
                out List<double> widthList);
            if (boolResult)
            {
                Curve c2c = Line.CreateBound(this.FixCentroid, otherRoom.FixCentroid);
                int n = widthList.Count;


                foreach (Curve adjC in adjCurves)
                {
                    if (IsPlanIntersect(adjC,c2c,out XYZ iPt))
                    {
                        
                    }
                }
                return true;
            }
            return boolResult;
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
    public class PathExWall : PathE
    {
        public void MoveNext()
        {

        }
    }

}
