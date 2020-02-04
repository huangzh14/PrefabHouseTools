using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace PrefabHouseTools
{
    public class RoomInfo
    {
        public Room Room { get; }
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
        public static List<Dictionary<Curve,ElementId>> GetBoundary(Room room,out List<List<XYZ>> vertexList)
        {
            List<Dictionary<Curve, ElementId>> boundaryList =
                new List<Dictionary<Curve, ElementId>>();
            List<List<BoundarySegment>> BSlist =
                room.GetBoundarySegments
                (new SpatialElementBoundaryOptions())
                as List<List<BoundarySegment>>;
            vertexList = new List<List<XYZ>>();
            foreach (List<BoundarySegment> BSegs in BSlist)
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
                boundaries.Clear();
                vertexList.Add(vertexes);
                vertexes.Clear();
            }
            return boundaryList;
        }
        /// <summary>
        /// Return whether two room are adjacent.
        /// Must calculate the boundary info 
        /// </summary>
        /// <param name="room1"></param>
        /// <param name="room2"></param>
        /// <returns></returns>
        public bool IsAdjacent(RoomInfo otherRoom)
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
                                Curve shortc;
                                Curve longc;
                                if (bound1.Key.Length > bound2.Key.Length)
                                {
                                    shortc = bound2.Key;
                                    longc = bound1.Key;
                                }
                                else
                                {
                                    shortc = bound1.Key;
                                    longc = bound2.Key;
                                }
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
}
