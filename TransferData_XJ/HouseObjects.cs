using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TransferData_XJ
{
    public class A_Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
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
        public double Thickness { get; set; }
        public A_WallKind Kind { get; set; }
        public double? Height { get; set; }
        public string Uid { get; set; }
    }

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
        public short Entrance { get; set; }
        public string Wall { get; set; }
    }
    public class A_Door
    {
        public A_Point P1 { get; set; }
        public A_Point P2 { get; set; }
        public double Height { get; set; }
        public double SillHeight { get; set; }
        public A_DoorKind Kind { get; set; }
        public A_OpenDirection OpenDirection { get; set; }
        public string Uid { get; set; }
        public A_DoorMeta Meta { get; set; }
    }

    public enum A_WindowKind
    {
        SLIDING,HINGED,FRENCH,BAY
    }
    public class A_WindowMeta
    {
        public string Wall { get; set; }
    }
    public class A_Window
    {
        public A_Point P1 { get; set; }
        public A_Point P2 { get; set; }
        public double Height { get; set; }
        public double SillHeight { get; set; }
        public A_WindowKind Kind { get; set; }
        public double BayDepth { get; set; }
        public string Uid { get; set; }
        public A_WindowMeta Meta { get; set; }
    }


    public enum A_CubeKind
    {
        PILLAR,FLUE,WATER,HEATING,STRONG_CURRENT,WEAK_CURRENT
    }
    public class A_Cube
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double XSize { get; set; }
        public double YSize { get; set; }
        public double Rotation { get; set; }
        public double? Z { get; set; }
        public double? ZSize { get; set; }
        public A_CubeKind Kind { get; set; }
        public string Uid { get; set; }
    }

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
        public double Height { get; set; }
        public string Uid { get; set; }
        public A_RoomMeta Meta { get; set; }
    }

    public class A_Label
    {
        public A_Point Position { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class A_Floor
    {
        public int Number { get; set; }
        public IList<A_Wall> Walls { get; set; }
        public IList<A_Door> Doors { get; set; }
        public IList<A_Window> Windows { get; set; }
        public IList<A_Cube> Cubes { get; set; }
        public IList<A_Room> Rooms { get; set; }
        public IList<A_Label> Labels { get; set; }
        public double Height { get; set; }
    }
    public class HouseObjects
    {
        public int Version { get; set; }
        public double Rotation { get; set; }
        public IList<A_Floor> Floors { get; set; }
        public HouseObjects()
        {
            Floors = new List<A_Floor>();
            Version = 0;
            Rotation = 0;
        }

    }
}
