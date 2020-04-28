using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PrefabHouseTools
{
    public class A_Point
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    #region Wall objects.
    public enum A_WallKind
    {
        NON_BEARING,
        BEARING,
        SHORT,
        RAILING
    }
    public class A_Wall
    {
        public A_Point P1 { get; set; }
        public A_Point P2 { get; set; }
        public float Thickness { get; set; }
        public A_WallKind Kind { get; set; }
        public float? Height { get; set; }
        public string Uid { get; set; }
        public Wall Wall { get; set; }
    }
    #endregion

    #region Open objects
    public class A_OpeningMeta
    {
        public int Entrance { get; set; }
        public string Wall { get; set; }
    }
    public class A_Opening
    {
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
    }
    #endregion

    #region Cube objects
    public enum A_CubeKind
    {
        PILLAR,FLUE,WATER,HEATING,STRONG_CURRENT,WEAK_CURRENT
    }
    public class A_Cube
    {
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
        NANNY,LAUNDRY,FREE,GARAGE,TERRACE,OTHER,WELL,OUTER
    }
    public struct ContourRelation
    {
        public string Kind { get; set; }
        public string Uid { get; set; }
    }
    public struct A_Contour
    {
        public A_Point P1 { get; set; }
        public A_Point P2 { get; set; }
        public string Uid { get; set; }
        public ContourRelation Related { get; set; }
    }
    public struct A_RoomMeta
    {
        public IList<string> EntranceDoors { get; set; }
        public IList<A_Contour> Contours { get; set; }
        public IList<A_Contour> CubeContours { get; set; }
    }
    public class A_Room
    {
        public IList<A_RoomObjectInfo> Walls { get; set; }
        public IList<A_RoomObjectInfo> Doors { get; set; }
        public IList<A_RoomObjectInfo> Windows { get; set; }
        public IList<A_RoomObjectInfo> Cubes { get; set; }
        public A_RoomKind Kind { get; set; }
        public float Height { get; set; }
        public string Uid { get; set; }
        public A_RoomMeta Meta { get; set; }
    }
    #endregion

    #region Others
    public class A_Label
    {
        public A_Point Position { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
    #endregion

    #region Sockets
    public class A_SocketRelate
    {
        public string Kind;
        public string Uid;
    }
    public class A_Socket
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public string Name { get; set; }
        public A_SocketRelate Related { get; set; }
        public string Tag { get; set; }
        public A_Point Orientation { get; set; }
        public FamilyInstance Instance { get; set; }
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
    public class A_Furniture
    {
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
    public class RoomSoftDesign
    {
        public IList<A_Furniture> Furniture;
        public string Room;
    }
    #endregion

    #region Floor and house objects
    public class A_Floor
    {
        public int Number { get; set; }
        public IList<A_Wall> Walls { get; set; }
        public IList<A_Door> Doors { get; set; }
        public IList<A_Window> Windows { get; set; }
        public IList<A_Cube> Cubes { get; set; }
        public IList<A_Room> Rooms { get; set; }
        public IList<A_Room> Outers { get; set; }
        public IList<A_Label> Labels { get; set; }
        public float Height { get; set; }

        public IList<A_Socket> Socket { get; set; }
    }
    public class HouseObject
    {
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

    #region 家具布局结构总
    public class HouseWithSoft
    {
        public HouseObject House;
        public IList<RoomSoftDesign> RoomSoftDesigns;
    }
    #endregion
}
