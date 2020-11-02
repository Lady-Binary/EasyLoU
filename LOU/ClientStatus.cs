using ProtoBuf;
using System;
using System.Collections.Generic;

namespace LOU
{
    [ProtoContract]
    public class ClientStatus
    {
        [ProtoMember(1)]
        public long TimeStamp;

        [ProtoContract]
        public struct CharacterInfoStruct
        {
            [ProtoMember(1)]
            public ulong? BACKPACKID;
            [ProtoMember(2)]
            public string[] CHARBUFFS;
            [ProtoMember(3)]
            public int? CHARDIR;
            [ProtoMember(4)]
            public bool? CHARGHOST;
            [ProtoMember(5)]
            public ulong? CHARID;
            [ProtoMember(6)]
            public string CHARNAME;
            [ProtoMember(7)]
            public float? CHARPOSX;
            [ProtoMember(8)]
            public float? CHARPOSY;
            [ProtoMember(9)]
            public float? CHARPOSZ;
            [ProtoMember(10)]
            public string CHARSTATUS;
            [ProtoMember(11)]
            public double? CHARWEIGHT;
            [ProtoMember(12)]
            public ulong? CHESTID;
            [ProtoMember(13)]
            public string CHESTNAME;
            [ProtoMember(14)]
            public ulong? HEADID;
            [ProtoMember(15)]
            public string HEADNAME;
            [ProtoMember(16)]
            public ulong? LEFTHANDID;
            [ProtoMember(17)]
            public string LEFTHANDNAME;
            [ProtoMember(18)]
            public ulong? LEGSID;
            [ProtoMember(19)]
            public string LEGSNAME;
            [ProtoMember(20)]
            public ulong? RIGHTHANDID;
            [ProtoMember(21)]
            public string RIGHTHANDNAME;
        }
        [ProtoMember(2)]
        public CharacterInfoStruct CharacterInfo;

        [ProtoContract]
        public struct StatusBarStruct
        {
            [ProtoMember(1)]
            public double? AGI;
            [ProtoMember(2)]
            public double? ATTACKSPEED;
            [ProtoMember(3)]
            public double? DEFENSE;
            [ProtoMember(4)]
            public double? HEALTH;
            [ProtoMember(5)]
            public double? INT;
            [ProtoMember(6)]
            public double? MANA;
            [ProtoMember(7)]
            public double? PRESTIGEXPMAX;
            [ProtoMember(8)]
            public double? STAMINA;
            [ProtoMember(9)]
            public double? STEALTH;
            [ProtoMember(10)]
            public double? STR;
            [ProtoMember(11)]
            public double? VITALITY;
        }
        [ProtoMember(3)]
        public StatusBarStruct StatusBar;

        [ProtoContract]
        public struct LastActionStruct
        {
            [ProtoMember(1)]
            public ulong? COBJECTID;
            [ProtoMember(2)]
            public ulong? LOBJECTID;
        }
        [ProtoMember(4)]
        public LastActionStruct LastAction;

        [ProtoContract]
        public struct FINDBUTTONStruct
        {
            [ProtoMember(1)]
            public string NAME;
            [ProtoMember(2)]
            public string TEXT;
        }
        [ProtoContract]
        public struct FINDINPUTStruct
        {
            [ProtoMember(1)]
            public string ID;
        }
        [ProtoContract]
        public struct FINDITEMStruct
        {
            [ProtoMember(1)]
            public double? DISTANCE;
            [ProtoMember(2)]
            public ulong? CNTID;
            [ProtoMember(3)]
            public ulong? ID;
            [ProtoMember(4)]
            public string NAME;
            [ProtoMember(5)]
            public float? X;
            [ProtoMember(6)]
            public float? Y;
            [ProtoMember(7)]
            public float? Z;
        }
        [ProtoContract]
        public struct FINDLABELStruct
        {
            [ProtoMember(1)]
            public string NAME;
            [ProtoMember(2)]
            public string TEXT;
        }
        [ProtoContract]
        public struct FINDMOBILEStruct
        {
            [ProtoMember(1)]
            public float? DISTANCE;
            [ProtoMember(2)]
            public double? HP;
            [ProtoMember(3)]
            public ulong? ID;
            [ProtoMember(4)]
            public string NAME;
            [ProtoMember(5)]
            public string TYPE;
            [ProtoMember(6)]
            public float? X;
            [ProtoMember(7)]
            public float? Y;
            [ProtoMember(8)]
            public float? Z;
        }
        [ProtoContract]
        public struct FINDPANELStruct
        {
            [ProtoMember(1)]
            public string ID;
        }
        [ProtoContract]
        public struct FINDPERMANENTStruct
        {
            [ProtoMember(1)]
            public string COLOR;
            [ProtoMember(2)]
            public float? DISTANCE;
            [ProtoMember(3)]
            public string HUE;
            [ProtoMember(4)]
            public int? ID;
            [ProtoMember(5)]
            public string NAME;
            [ProtoMember(6)]
            public int? STONESTATE;
            [ProtoMember(7)]
            public string TEXTURE;
            [ProtoMember(8)]
            public int? TREESTATE;
            [ProtoMember(9)]
            public float? X;
            [ProtoMember(10)]
            public float? Y;
            [ProtoMember(11)]
            public float? Z;
            
        }
        [ProtoContract]
        public struct FindStruct
        {
            [ProtoMember(1)]
            public FINDBUTTONStruct[] FINDBUTTON;
            [ProtoMember(2)]
            public FINDITEMStruct[] FINDITEM;
            [ProtoMember(3)]
            public FINDINPUTStruct[] FINDINPUT;
            [ProtoMember(4)]
            public FINDLABELStruct[] FINDLABEL;
            [ProtoMember(5)]
            public FINDMOBILEStruct[] FINDMOBILE;
            [ProtoMember(6)]
            public FINDPANELStruct[] FINDPANEL;
            [ProtoMember(7)]
            public FINDPERMANENTStruct[] FINDPERMANENT;
        }
        [ProtoMember(5)]
        public FindStruct Find;

        [ProtoContract]
        public enum CutomVarTypeEnum
        {
            [ProtoEnum]
            Void = 0,
            [ProtoEnum]
            Boolean,
            [ProtoEnum]
            Number,
            [ProtoEnum]
            String
        }
        [ProtoContract]
        public struct CustomVarStruct
        {
            public CustomVarStruct(object Value)
            {
                if (Value is Boolean)
                {
                    CommandParamType = CutomVarTypeEnum.Boolean;
                    Boolean = (bool)Value;
                    Number = null;
                    String = null;

                } else if (Value is sbyte
                    || Value is byte
                    || Value is short
                    || Value is ushort
                    || Value is int
                    || Value is uint
                    || Value is long
                    || Value is ulong
                    || Value is float
                    || Value is double
                    || Value is decimal)
                {
                    CommandParamType = CutomVarTypeEnum.Number;
                    Boolean = null;
                    Number = (double)Value;
                    String = null;
                } else if (Value is string)
                {
                    CommandParamType = CutomVarTypeEnum.String;
                    Boolean = null;
                    Number = null;
                    String = Value.ToString();
                } else
                {
                    CommandParamType = CutomVarTypeEnum.Void;
                    Boolean = null;
                    Number = null;
                    String = null;
                }

            }
            [ProtoMember(1)]
            public CutomVarTypeEnum CommandParamType;
            [ProtoMember(2)]
            public bool? Boolean;
            [ProtoMember(3)]
            public double? Number;
            [ProtoMember(4)]
            public string String;
        }
        [ProtoContract]
        public struct ClientInfoStruct
        {
            [ProtoMember(1)]
            public string CLIGAMESTATE;
            [ProtoMember(2)]
            public int? CLIID;
            [ProtoMember(3)]
            public string CLIVER;
            [ProtoMember(4)]
            public int? CLIXRES;
            [ProtoMember(5)]
            public int? CLIYRES;
            [ProtoMember(6)]
            public bool? FULLSCREEN;
            [ProtoMember(7)]
            public int? MAINCAMERAMASK;
            [ProtoMember(8)]
            public string SERVER;
            [ProtoMember(9)]
            public int? TARGETFRAMERATE;
            [ProtoMember(10)]
            public int? VSYNCCOUNT;
        }
        [ProtoMember(6)]
        public ClientInfoStruct ClientInfo;

        [ProtoContract]
        public struct OBJStruct
        {
            [ProtoMember(1)]
            public ulong? CNTID;
            [ProtoMember(2)]
            public string NAME;
            [ProtoMember(3)]
            public ulong? OBJECTID;
            [ProtoMember(4)]
            public int? PERMANENTID;
        }
        [ProtoContract]
        public struct UIStruct
        {
            [ProtoMember(1)]
            public string NAME;
            [ProtoMember(2)]
            public float? X;
            [ProtoMember(3)]
            public float? Y;
        }
        [ProtoContract]
        public struct NEARBYMONSTERStruct
        {
            [ProtoMember(1)]
            public double? DISTANCE;
            [ProtoMember(2)]
            public double? HP;
            [ProtoMember(3)]
            public ulong? ID;
            [ProtoMember(4)]
            public string NAME;
        }
        [ProtoContract]
        public struct MiscellaneousStruct
        {
            [ProtoMember(1)]
            public OBJStruct CLICKOBJ;
            [ProtoMember(2)]
            public float? CLICKWINDOWX;
            [ProtoMember(3)]
            public float? CLICKWINDOWY;
            [ProtoMember(4)]
            public float? CLICKWORLDX;
            [ProtoMember(5)]
            public float? CLICKWORLDY;
            [ProtoMember(6)]
            public float? CLICKWORLDZ;
            [ProtoMember(7)]
            public int? COMMANDID;
            [ProtoMember(8)]
            public Dictionary<string, CustomVarStruct> CUSTOMVARS;
            [ProtoMember(9)]
            public bool? MONSTERSNEARBY;
            [ProtoMember(10)]
            public OBJStruct MOUSEOVEROBJ;
            [ProtoMember(11)]
            public UIStruct MOUSEOVERUI;
            [ProtoMember(12)]
            public float? MOUSEWINDOWPOSX;
            [ProtoMember(13)]
            public float? MOUSEWINDOWPOSY;
            [ProtoMember(14)]
            public float? MOUSEWORLDPOSX;
            [ProtoMember(15)]
            public float? MOUSEWORLDPOSY;
            [ProtoMember(16)]
            public float? MOUSEWORLDPOSZ;
            [ProtoMember(17)]
            public NEARBYMONSTERStruct[] NEARBYMONSTERS;
            [ProtoMember(18)]
            public int? RANDOM;
            [ProtoMember(19)]
            public string SCANJOURNALMESSAGE;
            [ProtoMember(20)]
            public float? SCANJOURNALTIME;
            [ProtoMember(21)]
            public bool? TARGETLOADING;
            [ProtoMember(22)]
            public string TARGETTYPE;
            [ProtoMember(23)]
            public float? TIME;
            [ProtoMember(24)]
            public string TOOLTIPTEXT;
        }
        [ProtoMember(7)]
        public MiscellaneousStruct Miscellaneous;
    }
}
