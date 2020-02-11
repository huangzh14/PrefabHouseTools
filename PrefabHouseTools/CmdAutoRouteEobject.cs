#region Namespaces
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
#endregion

namespace PrefabHouseTools
{
    #region Intro
    ///Important note:Here are some basic assumptions.
    ///If the project doesn't meet,unexpect result may occur.
    ///
    ///1-Room boundary are lines not curves.The program use
    ///Curve class for develop convenient,the logic all assume
    ///they are all lines.
    ///
    ///2-
    #endregion

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
        public double FloorLevel { get; }
        
        public RoomInfo(Room room)
        {
            Room = room;
            BoundaryList = GetBoundary
                (room,out List<List<XYZ>>vList);
            VertexList = vList;
            AdjacentRooms = new List<RoomInfo>();
            FloorLevel = room.get_BoundingBox
                (room.Document.ActiveView).Min.Z;
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
        /// This method project two curves c1 and c2 onto baseCurve
        /// If the projection result of c1&c2 overlap,return ture.
        /// </summary>
        /// <param name="c1">The input curve1.</param>
        /// <param name="c2">The input curve2.</param>
        /// <param name="baseCurve">The base curve to project on.</param>
        /// <param name="interCurve">
        /// The overlap curve of input c1&c2 on the baseCurve</param>
        /// <param name="t">
        /// The vector needed to transform interCurve to the center
        /// of input c1&c2,if needed.</param>
        /// <returns>True if projection of c1&c2 on baseCurve
        /// have overlapped area.</returns>
        public bool ProjectCurvesOverlap(Curve c1,Curve c2,
            Curve baseCurve,out Curve interCurve,out XYZ t)
        {
            ///Create the output.
            interCurve = baseCurve.Clone();
            t = new XYZ();
            ///Project input curves onto base curve.
            ///Get the projection parameter.
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
            ///Compare two domain to decide if overlap area exist.
            ///And if overlap area exist,create the overlap curve.
            if ((cp1[0] == cp1[1])||(cp2[0] == cp2[1])
                || (cp1[1] <= cp2[0]))
                return false;
            else if (cp1[1] <= cp2[1])
                interCurve.MakeBound(cp2[0], cp1[1]);
            else
                interCurve.MakeBound(cp2[0], cp2[1]);
            ///Get the vector to move the basecurve to the center of c1&c2.
            XYZ basePt = interCurve.GetEndPoint(0);
            XYZ centPt = 0.5 * (c1.Project(basePt).XYZPoint 
                              + c2.Project(basePt).XYZPoint);
            t = centPt - basePt;
            return true;
        }

        /// <summary>
        /// This method match all the boundary line in each room
        /// to check if two boundary are from the same wall and 
        /// share a common projection on the basewall centerline.
        /// </summary>
        /// <param name="otherRoom">
        /// The otherRoom to calculate adjacency with.</param>
        /// <param name="adjCurves">
        /// The centerline of the adjacent walls</param>
        /// <param name="normVlist">
        /// The normal vector which point at inside this room.</param>
        /// <param name="wallWidth">
        /// The width of the adjacent wall.</param>
        /// <returns></returns>
        public bool IsAdjacentTo(RoomInfo otherRoom,
            out List<Curve> adjCurves,out List<XYZ> normVlist,
            out List<double>wallWidth)
        {
            adjCurves = new List<Curve>();
            normVlist = new List<XYZ>();
            wallWidth = new List<double>();
            bool isAdjacent = false;
            foreach(List<Bcurve> boundLoop1 in BoundaryList){
                foreach(List<Bcurve> boundLoop2 
                    in otherRoom.BoundaryList){
                    ///First iterate through each boundary loop.
                    foreach(Bcurve bound1 in boundLoop1){
                        foreach(Bcurve bound2 in boundLoop2){
                            ///Then iterate through each boundary segment.
                            ///If they are the same wall carry on.
                            if ((bound1.Id == bound2.Id)&&(bound1.BaseIsWall))
                            {
                                Curve baseCurve = bound1.BaseWallCurve;
                                Curve c1 = bound1.Curve;
                                Curve c2 = bound2.Curve;
                                if (ProjectCurvesOverlap(c1,c2,
                                    baseCurve,out Curve adjC,out XYZ t))
                                {
                                    isAdjacent = true;
                                    ///Move the curve to wall center line.
                                    Curve centC = adjC.CreateTransformed
                                        (Transform.CreateTranslation(t));
                                    XYZ pt = centC.GetEndPoint(0);
                                    XYZ normV = c1.Project(pt).XYZPoint - pt;
                                    adjCurves.Add(centC);
                                    normVlist.Add(normV.Normalize());
                                    wallWidth.Add(bound1.BaseWallWidth);
                                }   
                            }
                        }
                    }
                }
            }
            return isAdjacent;
        }
        /// <summary>
        /// The overload method that only 
        /// give answer whether adjacent.
        /// </summary>
        /// <param name="otherRoom"></param>
        /// <returns></returns>
        public bool IsAdjacentTo(RoomInfo otherRoom)
        {
            bool result = this.IsAdjacentTo(otherRoom, 
                out List<Curve> c,out List<XYZ> t, 
                out List<double> w);
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

        /// <summary>
        /// 
        /// </summary>
        public void CalculateFixCentroid()
        {
            if (ElecFixtures.Count > 0)
            {
                double aX = ElecFixtures.Average(ef => ef.Origin.X);
                double aY = ElecFixtures.Average(ef => ef.Origin.Y);
                FixCentroid = new XYZ(aX, aY, FloorLevel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherRoom"></param>
        /// <param name="height"></param>
        /// <param name="path"></param>
        /// <param name="roughL"></param>
        /// <returns></returns>
        public bool AdjacentPathTo(RoomInfoElec otherRoom, double height,
            out PathExWall path,out double roughL)
        {
            ///Use the base class method to decide whether this two room 
            ///are adjacent and get the needed info.
            bool boolResult = 
                base.IsAdjacentTo(otherRoom, 
                out List<Curve> adjCurves,
                out List<XYZ> normVectors,
                out List<double> widthList);
            ///If the two room are adjacent,calculate
            ///the intersect point of the two room fixtures centriod line.
            if (boolResult)
            {
                Curve c2c = Line.CreateBound
                    (this.FixCentroid, otherRoom.FixCentroid);
                ///Store the endpoint for the useful area of adjacent curve.
                Dictionary<XYZ,Curve> vList = new Dictionary<XYZ, Curve>();
                XYZ crossPt = new XYZ();//The real crossing point on the center line.
                XYZ normVec = new XYZ();//The normal vector
                double width = 0;//The width of the wall
                bool directLine = false;//If direct line exist.
                int n = widthList.Count;//Start iteration.
                for (int i = 0; i < n; i++)
                {
                    Curve adjC = adjCurves[i];
                    ///Trunk line need 200mm space.
                    double reserveW = UnitUtils
                        .ConvertToInternalUnits
                        (200, DisplayUnitType.DUT_MILLIMETERS);
                    /// If curve is not long enough,go to next one.
                    if (adjC.Length <= reserveW)
                        continue;
                    /// If curve is long enough,shorten each side 100mm.
                    XYZ p1 = adjC.GetEndPoint(0);
                    XYZ p2 = adjC.GetEndPoint(1);
                    XYZ dir = (p2 - p1).Normalize();
                    XYZ p1new = p1 + 0.5 * reserveW * dir;
                    XYZ p2new = p2 - 0.5 * reserveW * dir;
                    Line adjCuse = Line.CreateBound(p1new, p2new);
                    vList.Add(p1new, adjC);
                    vList.Add(p2new, adjC);
                    ///If intersect point found,stop searching.
                    if (IsPlanIntersect(adjCuse,c2c,out XYZ iPt))
                    {
                        crossPt = iPt;
                        normVec = normVectors[i];
                        width = widthList[i];
                        directLine = true;
                        break;
                    }
                }
                ///If no directline is formed,while avaiable curve exist,
                ///which means vList is not empty,find the closet point.
                if (!directLine && (vList.Count > 0))
                {
                    ///Sort the dictionary by distance to the curve.
                    var vListSorted = 
                        vList.OrderBy(v => c2c.Distance(v.Key));
                    int i = adjCurves.IndexOf(vListSorted.First().Value);
                    crossPt = vListSorted.First().Key;
                    normVec = normVectors[i];
                    width = widthList[i];
                }
                /// Only when width != 0 ,a crossing path can be created.
                /// Width =0 means no available route is generated.Go false.
                if (width != 0)
                {
                    //Create the crossing path.
                    XYZ ptHere = crossPt + 0.5 * width * normVec;
                    XYZ ptThere = crossPt - 0.5 * width * normVec;
                    FixtureE crossPtHere = new FixtureE(ptHere, height);
                    FixtureE crossPtThere = new FixtureE(ptThere, height);
                    path = new PathExWall(crossPtHere, crossPtThere);
                    roughL = width + (ptHere - FixCentroid).GetLength()
                        + (ptThere - otherRoom.FixCentroid).GetLength();
                    return true;
                }
            }
            path = new PathExWall();
            roughL = 0;
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

    public class ElecSystemInfo
    {
        public string Name { get; }
        public List<FixtureE> ElecFixtures { get; set; }
        public ElecSystemInfo(ElectricalSystem system)
        {
            Name = system.Name;
            ElecFixtures = new List<FixtureE>();
        }

    }

    
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
        public FixtureE(XYZ location,double height)
        {
            this.Origin = new XYZ(location.X, location.Y, height);
        }
        
    }

    
    public class PathE
    {
        public FixtureE Begin { get; }
        public FixtureE End { get; }
        public List<XYZ> Vertices { get; }
        public PathE(FixtureE begin,FixtureE end,List<XYZ> vertices)
        {
            this.Begin = begin;
            this.End = end;
            this.Vertices = vertices;
        }
    }
    
    public class PathExWall : PathE
    {
        public PathExWall(FixtureE begin, FixtureE end)
            : base(begin,end,new List<XYZ>())
        {
        }
        public PathExWall() : 
            base(new FixtureE(new XYZ(),0), 
                new FixtureE(new XYZ(),0), 
                new List<XYZ>())
        {
        }
        public void MoveNext()
        {

        }
    }

}
