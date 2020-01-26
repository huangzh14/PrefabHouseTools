using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.IFC;
using System.Text;
using System.Threading.Tasks;

namespace InteriorAutoPanel
{
    /// <summary>
    /// Define each panel.Including the coordinate of origin,the width and height.
    /// Also define the normal vector for direction.
    /// </summary>
    public struct PanelData
    {
        public XYZ PaOrigin { get; set; }
        public XYZ PaNormal { get; set; }
        public double PaWidth { get; set; }
        public double PaHeight { get; set; }
    }

    /// <summary>
    /// Define the position of a panelrange.
    /// This affect the placement of the non-standard panels.
    /// </summary>
    public enum LocStyle
    {
        Left, Middle, Right, Fill
    }

    /// <summary>
    /// This class containing some panels in which panels have same height and 
    /// width are distributed as evenly as possible inside this range.
    /// Usually created according to the position of doors and windows.
    /// </summary>
    public class PanelRange
    {
        //The constructor takes two coordinate and height length to define the range
        //Also take the location description and the normal vector for further use.
        public PanelRange(XYZ bottomLeft,XYZ bottomRight,double height,
            LocStyle localStyle,XYZ normal)
        {
            BottomL = bottomLeft;
            BottomR = bottomRight;
            H = height;
            LocS = localStyle;
            Normal = normal;
        }

        # region The properties
        public XYZ BottomL { get; set; }
        public XYZ BottomR { get; set; }
        public double H { get; set; }
        public LocStyle LocS { get; set; }
        public XYZ Normal { get; set; }

        //This store the panels inside this range.
        private List<PanelData> PanelDatas = new List<PanelData>();
        #endregion

        //This method is for demostration of the range.
        //Return a CurveArray containing the diagonals of the current range.
        //Can be demostrated by ModelCurves.
        public CurveArray GetRangeCurves()
        {
            XYZ uL = new XYZ(BottomL.X, BottomL.Y, BottomL.Z + H);
            XYZ uR = new XYZ(BottomR.X, BottomR.Y, BottomR.Z + H);
            Line l1 = Line.CreateBound(BottomL, uR);
            Line l2 = Line.CreateBound(uL, BottomR);
            CurveArray cA = new CurveArray();
            cA.Append(l1);cA.Append(l2);
            //cA.Append(l3);cA.Append(l4);
            return cA;
        }

        /// <summary>
        /// The main method to create the panels from a given range.
        /// With some other requirement that can vary among projects.
        /// </summary>
        /// <param name="Unit_Width">The standard panel width.No panel can exceed this width.
        ///              And standard width panel will be used as much as possible</param>
        /// <param name="DistToWall">The distance from panels' back to structural wall.</param>
        /// <returns>Return the panels data in a list.</returns>
        public List<PanelData> CreatePanelDatas(double Unit_Width,double DistToWall)
        {
            //Cleare the data.
            PanelDatas.Clear();

            //Get the total width and unit vector along the wall.
            double totalW = (BottomL - BottomR).GetLength();
            XYZ uVec = (BottomR - BottomL) / totalW;

            double tail;//The left-over width for non-unit part
            int roundNum;//The number of standard unit parts
            XYZ currentO;//Current panel origin localtion(using middle point)
            Queue<double> qWidth = new Queue<double>();//The queue for panel width.

            //Start calculating panels
            //case 1 only one panel needed
            if (totalW <= Unit_Width)
            {
                qWidth.Enqueue(totalW);
            }
            //case 2 two non-unit panels needed 
            else if (totalW % Unit_Width < Unit_Width/2 )
            {
                //The tail is less than half a standard panel.
                //In order to not have a panel too narrorw(less than half the standard) 
                //using two non-unit panels to equal the tail width plus a full unit width.
                tail = (totalW % Unit_Width)+Unit_Width;
                roundNum = (int)Math.Floor(totalW / Unit_Width) - 1;
                //the first panel is a non standard one
                qWidth.Enqueue(0.5 * tail);
                //than all standard panels
                for (int i = 0; i < roundNum; i++){
                    qWidth.Enqueue(Unit_Width);}
                //the last panel again a non standard ond
                qWidth.Enqueue(0.5 * tail);
            }
            //case 3 one non-unit panel
            else
            {
                //The tail is more than half a standard panel.
                //So the tail can form a non-standard panel itself.
                tail = (totalW % Unit_Width);
                roundNum = (int)Math.Floor(totalW / Unit_Width);
                //If the range is at the right corner of a wall,
                //then put the non-unit to the right.(Close to corner)
                if (LocS == LocStyle.Right){
                    for (int i = 0; i < roundNum; i++){
                        qWidth.Enqueue(Unit_Width);}
                    qWidth.Enqueue(tail);}
                else{//Otherwise put the non-unit to the left
                    qWidth.Enqueue(tail);
                    for (int i = 0; i < roundNum; i++){
                        qWidth.Enqueue(Unit_Width);}} 
            }

            //Create the panelsdata.
            //Set the pointer(currentO) to the origin of first panel 
            currentO = BottomL + 0.5 * qWidth.Peek() * uVec;
            //Set the loop number and offset vector.
            int qNum = qWidth.Count;
            XYZ offsetP = DistToWall * Normal;
            //Enqueue a 0 width.So that we can move the pointer the last time successfully.
            qWidth.Enqueue(0);
            for (int i = 0; i < qNum; i++)
            {
                PanelData panelNow = new PanelData(){
                    PaOrigin = currentO+offsetP,PaNormal = Normal,
                    PaHeight = H,PaWidth = qWidth.Peek()};
                PanelDatas.Add(panelNow);
                //Move the pointer to the origin of next panel.
                currentO = currentO + 
                    0.5 * (qWidth.Dequeue() + qWidth.Peek()) * uVec;
            }

            return PanelDatas;
        }
    }

    /// <summary>
    /// This class contain the infos for a piece of wall in a room.
    /// </summary>
    public class EdgeSegment
    {
        #region The properties
        //The basic parameter.
        const double er = 0.001;
        public double levelC { get; set; }
        public double levelF { get; set; }
        public XYZ normal { get; set; }
        public XYZ start = new XYZ();
        public XYZ end = new XYZ();
        public XYZ unitV
        {
            get
            {
                XYZ uV = end - start;
                uV = uV / uV.GetLength();
                return uV;
            }
        }

        public bool NoPanel { get { return noPanel; }set { noPanel = value; } }
        private bool noPanel = false;

        //The raw data.
        public List<XYZ> ptList= new List<XYZ>();
        public List<Curve> crvList = new List<Curve>();
        public List<XYZ> insertPts = new List<XYZ>();

        //The data processed.
        public List<PanelRange> ranges = new List<PanelRange>();
        public List<PanelData> panels = new List<PanelData>();

        #endregion

        //Merge two segment wall into one if they 
        public bool MergeWith(EdgeSegment wallToMerge,out EdgeSegment wallMerged)
        {
            EdgeSegment w1 ;
            EdgeSegment w2 ;
            wallMerged = this;
            if (this.unitV.AngleTo(wallToMerge.unitV) > er)
                return false;
            //decide merge direction
            if (this.end.IsAlmostEqualTo(wallToMerge.start))
            {
                w1 = this; w2 = wallToMerge;
            }
            else if (this.start.IsAlmostEqualTo(wallToMerge.end))
            {
                w1 = wallToMerge; w2 = this;
            }
            else
            {
                return false;
            }
            w1.end = w2.end;
            w1.ptList.AddRange(w2.ptList);
            w1.crvList.AddRange(w2.crvList);
            w1.insertPts.AddRange(w2.insertPts);
            w1.insertPts.Distinct();
            wallMerged = w1;
            return true;
        }

        //using the current data to create panelranges
        public void DivideToPanelRange()
        {
            //set standard height
            double staH = levelC - levelF;

            //1-set the standard range
            if (ptList.Count == 0)
            {
                ranges.Add(new PanelRange(start, end, staH, LocStyle.Fill,normal));
                return;//if a full wall, return directly.
            }
            else
            {
                //set the first panel first
                ranges.Add(new PanelRange
                    (start, ptList[0], staH, LocStyle.Left,normal));
                //set the standard range
                for (int i = 0; i < ptList.Count - 1; i++){
                    ranges.Add(new PanelRange
                        (ptList[i], ptList[i + 1], staH, LocStyle.Middle,normal));
                }
                ranges.Add(new PanelRange
                    (ptList[ptList.Count - 1], end, staH, LocStyle.Right,normal));
            }

            //2-divide by horizontal lines
            foreach (Curve curve in crvList)
            {
                //get the ref point to compare
                XYZ crvL = curve.GetEndPoint(0);
                XYZ crvR = curve.GetEndPoint(1);
                XYZ mid = 0.5 * (crvL+crvR);

                //find the right range and create new range
                foreach (PanelRange pRange in ranges)
                {
                    XYZ rCent = 0.5 * (pRange.BottomL + pRange.BottomR);
                    bool hori = rCent.IsAlmostEqualTo
                        (new XYZ(mid.X, mid.Y, rCent.Z));
                    bool ver = (mid.Z > pRange.BottomL.Z) &&
                        (mid.Z < pRange.BottomL.Z + pRange.H);
                    if (hori&&ver)
                    {
                        double newH = pRange.BottomL.Z + pRange.H - crvL.Z;
                        pRange.H = pRange.H - newH;
                        XYZ bLnew = new XYZ(pRange.BottomL.X, pRange.BottomL.Y, mid.Z);
                        XYZ bRnew = new XYZ(pRange.BottomR.X, pRange.BottomR.Y, mid.Z);
                        PanelRange pRangeNew = new PanelRange
                            (bLnew,bRnew,
                            newH, pRange.LocS,normal);
                        ranges.Insert(ranges.IndexOf(pRange), pRangeNew);
                        break;
                    }
                }
            }

            //3-elimate void ranges
            List<int> toMove = new List<int>();
            foreach (XYZ pt in insertPts)
            {
                int toRemove = 0;
                bool move = false;
                foreach (PanelRange range in ranges)
                {
                    bool ver = (pt.Z > range.BottomL.Z)
                    && (pt.Z < range.BottomL.Z + range.H);
                    XYZ pt1 = new XYZ(pt.X, pt.Y, range.BottomL.Z);
                    XYZ v1 = pt1 - range.BottomL;
                    XYZ v2 = pt1 - range.BottomR;
                    XYZ v3 = range.BottomL - range.BottomR;
                    bool hori = (v1.GetLength() < v3.GetLength())
                        && (v2.GetLength() < v3.GetLength());
                    if (ver && hori)
                    {
                        toRemove = ranges.IndexOf(range);
                        move = true;
                        break;
                    }
                }
                if (move) { ranges.RemoveAt(toRemove); }
            } 
        }
    }
}
