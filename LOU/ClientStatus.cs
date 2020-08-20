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
            public ulong BACKPACKID;
            [ProtoMember(2)]
            public int CHARDIR;
            [ProtoMember(3)]
            public bool CHARGHOST;
            [ProtoMember(4)]
            public ulong CHARID;
            [ProtoMember(5)]
            public string CHARNAME;
            [ProtoMember(6)]
            public float CHARPOSX;
            [ProtoMember(7)]
            public float CHARPOSY;
            [ProtoMember(8)]
            public float CHARPOSZ;
            [ProtoMember(9)]
            public string CHARSTATUS;
            [ProtoMember(10)]
            public double CHARWEIGHT;
            [ProtoMember(11)]
            public ulong CHESTID;
            [ProtoMember(12)]
            public string CHESTNAME;
            [ProtoMember(13)]
            public ulong HEADID;
            [ProtoMember(14)]
            public string HEADNAME;
            [ProtoMember(15)]
            public ulong LEFTHANDID;
            [ProtoMember(16)]
            public string LEFTHANDNAME;
            [ProtoMember(17)]
            public ulong LEGSID;
            [ProtoMember(18)]
            public string LEGSNAME;
            [ProtoMember(19)]
            public ulong RIGHTHANDID;
            [ProtoMember(20)]
            public string RIGHTHANDNAME;
        }
        [ProtoMember(2)]
        public CharacterInfoStruct CharacterInfo;

        [ProtoContract]
        public struct StatusBarStruct
        {
            [ProtoMember(1)]
            public double AGI;
            [ProtoMember(2)]
            public double ATTACKSPEED;
            [ProtoMember(3)]
            public double DEFENSE;
            [ProtoMember(4)]
            public double HEALTH;
            [ProtoMember(5)]
            public double INT;
            [ProtoMember(6)]
            public double MANA;
            [ProtoMember(7)]
            public double PRESTIGEXPMAX;
            [ProtoMember(8)]
            public double STAMINA;
            [ProtoMember(9)]
            public double STEALTH;
            [ProtoMember(10)]
            public double STR;
            [ProtoMember(11)]
            public double VITALITY;
        }
        [ProtoMember(3)]
        public StatusBarStruct StatusBar;

        [ProtoContract]
        public struct LastActionStruct
        {
            [ProtoMember(1)]
            public ulong COBJECTID;
            [ProtoMember(2)]
            public ulong LOBJECTID;
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
        public struct FINDITEMStruct
        {
            [ProtoMember(1)]
            public ulong CNTID;
            [ProtoMember(2)]
            public ulong ID;
            [ProtoMember(3)]
            public string NAME;
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
            public float DISTANCE;
            [ProtoMember(2)]
            public double HP;
            [ProtoMember(3)]
            public ulong ID;
            [ProtoMember(4)]
            public string NAME;
            [ProtoMember(5)]
            public string TYPE;
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
            public float DISTANCE;
            [ProtoMember(3)]
            public int ID;
            [ProtoMember(4)]
            public string NAME;
            [ProtoMember(5)]
            public string TEXTURE;
        }
        [ProtoContract]
        public struct FindStruct
        {
            [ProtoMember(1)]
            public FINDBUTTONStruct[] FINDBUTTON;
            [ProtoMember(2)]
            public FINDITEMStruct[] FINDITEM;
            [ProtoMember(3)]
            public FINDLABELStruct[] FINDLABEL;
            [ProtoMember(4)]
            public FINDMOBILEStruct[] FINDMOBILE;
            [ProtoMember(5)]
            public FINDPANELStruct[] FINDPANEL;
            [ProtoMember(6)]
            public FINDPERMANENTStruct[] FINDPERMANENT;
        }
        [ProtoMember(5)]
        public FindStruct Find;

        [ProtoContract]
        public struct ClientInfoStruct
        {
            [ProtoMember(1)]
            public string CLIGAMESTATE;
            [ProtoMember(2)]
            public int CLIID;
            [ProtoMember(3)]
            public string CLIVER;
            [ProtoMember(4)]
            public int CLIXRES;
            [ProtoMember(5)]
            public int CLIYRES;
            [ProtoMember(6)]
            public bool FULLSCREEN;
            [ProtoMember(7)]
            public int MAINCAMERAMASK;
            [ProtoMember(8)]
            public string SERVER;
            [ProtoMember(9)]
            public int TARGETFRAMERATE;
            [ProtoMember(10)]
            public int VSYNCCOUNT;
        }
        [ProtoMember(6)]
        public ClientInfoStruct ClientInfo;

        [ProtoContract]
        public struct NEARBYMONSTERStruct
        {
            [ProtoMember(1)]
            public double DISTANCE;
            [ProtoMember(2)]
            public double HP;
            [ProtoMember(3)]
            public ulong ID;
            [ProtoMember(4)]
            public string NAME;
        }
        [ProtoContract]
        public struct MiscellaneousStruct
        {
            [ProtoMember(1)]
            public string CLICKDISPLAYNAME;
            [ProtoMember(2)]
            public string CLICKOBJCNTID;
            [ProtoMember(3)]
            public string CLICKOBJID;
            [ProtoMember(4)]
            public string CLICKOBJNAME;
            [ProtoMember(5)]
            public string CLICKPERID;
            [ProtoMember(6)]
            public string CLICKWINDOWX;
            [ProtoMember(7)]
            public string CLICKWINDOWY;
            [ProtoMember(8)]
            public string CLICKWINDOWZ;
            [ProtoMember(9)]
            public string CLICKWORLDX;
            [ProtoMember(10)]
            public string CLICKWORLDY;
            [ProtoMember(11)]
            public string CLICKWORLDZ;
            [ProtoMember(12)]
            public string COMMANDID;
            [ProtoMember(13)]
            public string MONSTERSNEARBY;
            [ProtoMember(14)]
            public NEARBYMONSTERStruct[] NEARBYMONSTERS;
            [ProtoMember(15)]
            public string MOUSEOVERCNTID;
            [ProtoMember(16)]
            public string MOUSEOVERDISPLAYNAME;
            [ProtoMember(17)]
            public string MOUSEOVEROBJCNTID;
            [ProtoMember(18)]
            public string MOUSEOVEROBJID;
            [ProtoMember(19)]
            public string MOUSEOVEROBJNAME;
            [ProtoMember(20)]
            public string MOUSEOVERPERID;
            [ProtoMember(21)]
            public string MOUSEOVERUINAME;
            [ProtoMember(22)]
            public string MOUSEOVERUIX;
            [ProtoMember(23)]
            public string MOUSEOVERUIY;
            [ProtoMember(24)]
            public string MOUSEWINDOWX;
            [ProtoMember(25)]
            public string MOUSEWINDOWY;
            [ProtoMember(26)]
            public string MOUSEWINDOWZ;
            [ProtoMember(27)]
            public string MOUSEWORLDX;
            [ProtoMember(28)]
            public string MOUSEWORLDY;
            [ProtoMember(29)]
            public string MOUSEWORLDZ;
            [ProtoMember(30)]
            public string RANDOM;
            [ProtoMember(31)]
            public string SCANJOURNALMESSAGE;
            [ProtoMember(32)]
            public string SCANJOURNALTIME;
            [ProtoMember(33)]
            public string TARGETLOADING;
            [ProtoMember(34)]
            public string TARGETTYPE;
            [ProtoMember(35)]
            public string TIME;
            [ProtoMember(36)]
            public string TOOLTIPTEXT;
        }
        [ProtoMember(7)]
        public MiscellaneousStruct Miscellaneous;
    }
}
