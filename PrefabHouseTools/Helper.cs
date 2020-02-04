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
    class Helper
    {
        struct Bcurve
        {
            public Curve Curve;
            public ElementId Id;
            public Bcurve(Curve curve,ElementId id)
            {
                Curve = curve;
                Id = id;
            }
        }
        /// <summary>
        /// Calculate the boundary of a room.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Dictionary<Curve,ElementId>> RoomBoundary(Room room)
        {
            List<Dictionary<Curve, ElementId>> boundaryList =
                new List<Dictionary<Curve, ElementId>>();
            Document doc = room.Document;
            List<List<BoundarySegment>> BSlist =
                room.GetBoundarySegments
                (new SpatialElementBoundaryOptions())
                as List<List<BoundarySegment>>;
            foreach (List<BoundarySegment> BSegs in BSlist)
            {
                Dictionary<Curve, ElementId> boundaries = 
                    new Dictionary<Curve, ElementId>();
                Stack<Bcurve> boundCurs = new Stack<Bcurve>();
                Queue<Bcurve> cursMerged = new Queue<Bcurve>();
                foreach (BoundarySegment Seg in BSegs)
                {
                    boundCurs.Push(Seg.GetCurve());
                    curId.Push(Seg.ElementId);
                }


            }
            return boundaryList;
        }
        private bool Merge ()
    }
}
