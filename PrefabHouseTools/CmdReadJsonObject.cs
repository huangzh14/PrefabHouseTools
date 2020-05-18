using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PrefabHouseTools
{
    public class A_Point : A_Object
    {
        public float X { get; set; }
        public float Y { get; set; }

        public void TransferMm2Feet()
        {
            this.X = Helper.Mm2Feet(this.X);
            this.Y = Helper.Mm2Feet(this.Y);
        }
    }

    /// <summary>
    /// 所有物体母界面，包含必须应用的方法
    /// </summary>
    public interface A_Object
    {
        void TransferMm2Feet();
    }

    #region Wall objects.
    public enum A_WallKind
    {
        NON_BEARING,
        BEARING,
        SHORT,
        RAILING
    }
    public class A_Wall : A_Object
    {
        public A_Point P1 { get; set; }
        public A_Point P2 { get; set; }
        public float Thickness { get; set; }
        public A_WallKind Kind { get; set; }
        public float? Height { get; set; }
        public string Uid { get; set; }
        public Wall Wall { get; set; }

        public void TransferMm2Feet()
        {
            this.P1.TransferMm2Feet();
            this.P2.TransferMm2Feet();
            this.Thickness = Helper.Mm2Feet(this.Thickness);
            this.Height = Helper.Mm2Feet(this.Height);
        }
    }
    #endregion

    #region Open objects
    public class A_OpeningMeta
    {
        public int Entrance { get; set; }
        public string Wall { get; set; }
    }
    public class A_Opening : A_Object
    {
        public virtual void TransferMm2Feet()
        {
            this.P1.TransferMm2Feet();
            this.P2.TransferMm2Feet();
            this.Height = Helper.Mm2Feet(this.Height);
            this.SillHeight = Helper.Mm2Feet(this.SillHeight);
        }
        public A_Point P1 { get; set; }
        public A_Point P2 { get; set; }
        public float Height { get; set; }
        public float SillHeight { get; set; }
        public string Uid { get; set; }
        public A_OpeningMeta Meta { get; set; }
        /// <summary>
        /// This property is not in json.
        /// </summary>
        public float Width
        {
            get
            {
                return (float)Math.Pow
                    ((Math.Pow((P1.X - P2.X), 2) + Math.Pow((P1.Y - P2.Y), 2)), 0.5);
            }
        }
        public FamilyInstance Instance { get; set; }
    }
    #endregion

    #region Door objects
    public enum A_DoorKind
    {
        PASS,SINGLE,SLIDING,EQUAL_DOUBLE,UNEQUAL_DOUBLE,FOLDING,BARN
    }
    public enum A_OpenDirection
    {
        WHATEVER,CLOCKWISE,ANTI_CLOCKWISE
    }
    public class A_DoorMeta
    {
        public int Entrance { get; set; }
        public string Wall { get; set; }
    }
    public class A_Door : A_Opening
    {
        public A_DoorKind Kind { get; set; }
        public A_OpenDirection OpenDirection { get; set; }

        
        /// <summary>
        /// Hand and facing orientation correspond to
        /// the property of door family in revit.
        /// </summary>
        public XYZ HandOrientation
        {
            get
            {
                return new XYZ(P1.X - P2.X, P1.Y - P2.Y, 0).Normalize();
            }
        }
        public XYZ FacingOrientation
        {
            get
            {
                if (this.OpenDirection == A_OpenDirection.CLOCKWISE)
                {
                    return new XYZ(P2.Y - P1.Y, P1.X - P2.X, 0).Normalize();
                }
                return new XYZ(P1.Y - P2.Y, P2.X - P1.X, 0).Normalize();
            }
        }
    }
    #endregion

    #region Window objects
    public enum A_WindowKind
    {
        SLIDING,HINGED,FRENCH,BAY
    }
    public class A_WindowMeta
    {
        public string Wall { get; set; }
    }
    public class A_Window : A_Opening
    {
        public A_WindowKind Kind { get; set; }
        public float BayDepth { get; set; }
        public override void TransferMm2Feet()
        {
            base.TransferMm2Feet();
            this.BayDepth = Helper.Mm2Feet(this.BayDepth);
        }
    }
    #endregion

    #region Cube objects
    public enum A_CubeKind
    {
        PILLAR,FLUE,WATER,HEATING,STRONG_CURRENT,WEAK_CURRENT
    }
    public class A_Cube : A_Object
    {
        public void TransferMm2Feet()
        {
            X = Helper.Mm2Feet(X);
            Y = Helper.Mm2Feet(Y);
            XSize = Helper.Mm2Feet(XSize);
            YSize = Helper.Mm2Feet(YSize);
            Z = Helper.Mm2Feet(Z);
            ZSize = Helper.Mm2Feet(ZSize);
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float XSize { get; set; }
        public float YSize { get; set; }
        public float Rotation { get; set; }
        public float? Z { get; set; }
        public float? ZSize { get; set; }
        public A_CubeKind Kind { get; set; }
        public string Uid { get; set; }
    }
    #endregion

    #region Room objects
    public struct A_RoomObjectInfo
    {
        public string Uid { get; set; }
        public bool Reversed { get; set; }
    }
    public enum A_RoomKind
    {
        UNDETERMINED,LIVING_ROOM,BEDROOM,CHILD_ROOM,ELDER_ROOM,BATHROOM,
        KITCHEN,BALCONY,STUDY,STOREROOM,CLOAKROOM,KITCHEN_BALCONY,
        BEDROOM_MASTER,PASSAGE,DINING,RECEPTION,STAIRCASE,YARD,EQUIPMENT,
        NANNY,LAUNDRY,FREE,GARAGE,TERRACE,OTHER,WELL,OUTER,ENTRANCE
    }
    public struct ContourRelation
    {
        public string Kind { get; set; }
        public string Uid { get; set; }
    }
    public class A_Contour : A_Object
    {
        public void TransferMm2Feet()
        {
            P1.TransferMm2Feet();
            P2.TransferMm2Feet();
        }
        public A_Point P1 { get; set; }
        public A_Point P2 { get; set; }
        public string Uid { get; set; }
        public ContourRelation Related { get; set; }

        
    }
    public class A_RoomMeta : A_Object
    {
        public IList<string> EntranceDoors { get; set; }
        public IList<A_Contour> Contours { get; set; }
        public IList<A_Contour> CubeContours { get; set; }

        public void TransferMm2Feet()
        {
            foreach(A_Contour contour in Contours)
            {
                contour.TransferMm2Feet();
            }
            foreach(A_Contour contour in CubeContours)
            {
                contour.TransferMm2Feet();
            }
        }
    }
    public class A_Room : A_Object
    {
        public IList<A_RoomObjectInfo> Walls { get; set; }
        public IList<A_RoomObjectInfo> Doors { get; set; }
        public IList<A_RoomObjectInfo> Windows { get; set; }
        public IList<A_RoomObjectInfo> Cubes { get; set; }
        public A_RoomKind Kind { get; set; }
        public float Height { get; set; }
        public string Uid { get; set; }
        public A_RoomMeta Meta { get; set; }

        public void TransferMm2Feet()
        {
            Height = Helper.Mm2Feet(Height);
            Meta.TransferMm2Feet();
        }
    }
    #endregion

    #region Others
    public class A_Label : A_Object
    {
        public A_Point Position { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public void TransferMm2Feet()
        {
            Position.TransferMm2Feet();
        }
    }
    #endregion

    #region MEP系统末端/MEP-SystemTerminals
    /// <summary>
    /// 所有MEP末端的母族
    /// </summary>
    public abstract class A_Terminal : A_Object
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public string Name { get; set; }
        public void TransferMm2Feet()
        {
            this.X = Helper.Mm2Feet(this.X);
            this.Y = Helper.Mm2Feet(this.Y);
            this.Z = Helper.Mm2Feet(this.Z);
        }

    }

    /// <summary>
    /// 水电系统末端的母类
    /// </summary>
    public abstract class A_SystemTerminal : A_Terminal
    {
        public A_TerminalRelate Related { get; set; }
        public string Tag { get; set; }
        public A_Point Orientation { get; set; }
        public FamilyInstance Instance { get; set; }
    }
    public class A_TerminalRelate
    {
        public string Kind;
        public string Uid;
    }

    /// <summary>
    /// 插座类
    /// </summary>
    public class A_Socket : A_SystemTerminal
    {
    }
    /// <summary>
    /// 给水末端类
    /// </summary>
    public class A_WaterSupply : A_SystemTerminal
    {

    }
    /// <summary>
    /// 照明类
    /// </summary>
    public class A_Lighting : A_Terminal
    {

    }
    #endregion

    #region SoftDesign
    public enum A_FurLayer
    {
        basic,floor,child,wall,ceiling
    }
    public struct A_FurItemType
    {
        public string Id;
        public string TypeName;
        public string Label;
        public A_FurLayer Layer;
        public string Category;
    }
    public class A_Furniture : A_Object
    {
        public void TransferMm2Feet()
        {
            X = Helper.Mm2Feet(X);
            Y = Helper.Mm2Feet(Y);
            Z = Helper.Mm2Feet(Z);
            XSize = Helper.Mm2Feet(XSize);
            YSize = Helper.Mm2Feet(YSize);
            ZSize = Helper.Mm2Feet(ZSize);
            Rotation = (float)Math.PI * Rotation / 180;
        }
        public A_FurLayer Layer;
        public float X;
        public float Y;
        public float Rotation;
        public float XSize;
        public float YSize;
        public float ZSize;
        public string Item;
        public float? Z;
        public string Parent;
        public string Uid;
        public string Kind;
        public bool Free;
        public bool Scalable;
        public bool Removable;
        public string Param;
        public IList<A_FurItemType> ItemTypes;
        /// <summary>
        /// 用于家具种类识别
        /// </summary>
        public ElementId ElementId;
        public XYZ RefPoint;

        
    }
    public class RoomSoftDesign : A_Object
    {
        public IList<A_Furniture> Furniture;
        public string Room;

        public void TransferMm2Feet()
        {
            foreach (A_Object obj in Furniture)
            {
                obj.TransferMm2Feet();
            }
        }
    }
    #endregion

    #region Floor and house objects
    public class A_Floor : A_Object
    {
        public void TransferMm2Feet()
        {
            Height = Helper.Mm2Feet(Height);
            List<A_Object> allObjects = new List<A_Object>();
            allObjects.AddRange(Walls);
            allObjects.AddRange(Doors);
            allObjects.AddRange(Windows);
            allObjects.AddRange(Cubes);
            allObjects.AddRange(Rooms);
            allObjects.AddRange(Outers);
            allObjects.AddRange(Labels);
            allObjects.AddRange(Sockets);
            allObjects.AddRange(Feedwater);
            allObjects.AddRange(Lights);
            foreach (A_Object obj in allObjects)
            {
                obj.TransferMm2Feet();
            }
        }
        public string Number { get; set; }
        public IList<A_Wall> Walls { get; set; }
        public IList<A_Door> Doors { get; set; }
        public IList<A_Window> Windows { get; set; }
        public IList<A_Cube> Cubes { get; set; }
        public IList<A_Room> Rooms { get; set; }
        public IList<A_Room> Outers { get; set; }
        public IList<A_Label> Labels { get; set; }
        public float Height { get; set; }

        public IList<A_Socket> Sockets { get; set; }
        public IList<A_WaterSupply> Feedwater { get; set; }
        public IList<A_Lighting> Lights { get; set; }

        
    }
    public class HouseObject : A_Object
    {
        public void TransferMm2Feet()
        {
            Rotation = (float)Math.PI * Rotation / 180;
            foreach (A_Object obj in Floors)
            {
                obj.TransferMm2Feet();
            }
        }
        public string Version { get; set; }
        public float Rotation { get; set; }
        public IList<A_Floor> Floors { get; set; }
        public HouseObject()
        {
            Floors = new List<A_Floor>();
            Version = "0";
            Rotation = 0;
        }

        
    }

    #endregion

    #region 家具布局结构合并
    public class HouseWithSoft : A_Object
    {
        public HouseObject House;
        public IList<RoomSoftDesign> Room_Soft_Designs;

        public void TransferMm2Feet()
        {
            House.TransferMm2Feet();
            foreach (A_Object obj in Room_Soft_Designs)
            {
                obj.TransferMm2Feet();
            }
        }
    }
    #endregion
}
