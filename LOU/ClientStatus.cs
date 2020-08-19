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
            public string AGI;
            [ProtoMember(2)]
            public string ATTACKSPEED;
            [ProtoMember(3)]
            public string DEFENSE;
            [ProtoMember(4)]
            public string HEALTH;
            [ProtoMember(5)]
            public string INT;
            [ProtoMember(6)]
            public string MANA;
            [ProtoMember(7)]
            public string PRESTIGEXPMAX;
            [ProtoMember(8)]
            public string STAMINA;
            [ProtoMember(9)]
            public string STEALTH;
            [ProtoMember(10)]
            public string STR;
            [ProtoMember(11)]
            public string VITALITY;
        }
        [ProtoMember(3)]
        public StatusBarStruct StatusBar;

        [ProtoContract]
        public struct LastActionStruct
        {
            [ProtoMember(1)]
            public string COBJECTID;
            [ProtoMember(2)]
            public string LOBJECTID;
        }
        [ProtoMember(4)]
        public LastActionStruct LastAction;

        [ProtoContract]
        public struct FindStruct
        {
            [ProtoMember(1)]
            public string FINDBUTTONNAME;
            [ProtoMember(2)]
            public string FINDBUTTONTEXT;
            [ProtoMember(3)]
            public string FINDGAMEOBJECTID;
            [ProtoMember(4)]
            public string FINDITEMCNTID;
            [ProtoMember(5)]
            public string FINDITEMID;
            [ProtoMember(6)]
            public string FINDITEMNAME;
            [ProtoMember(7)]
            public string FINDLABELNAME;
            [ProtoMember(8)]
            public string FINDLABELTEXT;
            [ProtoMember(9)]
            public string FINDMOBILEDIST;
            [ProtoMember(10)]
            public string FINDMOBILEHP;
            [ProtoMember(11)]
            public string FINDMOBILEID;
            [ProtoMember(12)]
            public string FINDMOBILENAME;
            [ProtoMember(13)]
            public string FINDMOBILETYPE;
            [ProtoMember(14)]
            public string FINDPANELID;
            [ProtoMember(15)]
            public string FINDPERMACOLOR;
            [ProtoMember(16)]
            public string FINDPERMADIST;
            [ProtoMember(17)]
            public string FINDPERMAID;
            [ProtoMember(18)]
            public string FINDPERMANAME;
            [ProtoMember(19)]
            public string FINDPERMATEXTURE;
            [ProtoMember(20)]
            public string MONSTERSDIST;
            [ProtoMember(21)]
            public string MONSTERSHP;
            [ProtoMember(22)]
            public string MONSTERSID;
            [ProtoMember(23)]
            public string MONSTERSNAME;
        }
        [ProtoMember(5)]
        public FindStruct Find;

        [ProtoContract]
        public struct ClientInfoStruct
        {
            [ProtoMember(1)]
            public string CLIGAMESTATE;
            [ProtoMember(2)]
            public string CLIID;
            [ProtoMember(3)]
            public string CLIVER;
            [ProtoMember(4)]
            public string CLIXRES;
            [ProtoMember(5)]
            public string CLIYRES;
            [ProtoMember(6)]
            public string FULLSCREEN;
            [ProtoMember(7)]
            public string MAINCAMERAMASK;
            [ProtoMember(8)]
            public string SERVER;
            [ProtoMember(9)]
            public string TARGETFRAMERATE;
            [ProtoMember(10)]
            public string VSYNCCOUNT;
        }
        [ProtoMember(6)]
        public ClientInfoStruct ClientInfo;

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
            public string MOUSEOVERCNTID;
            [ProtoMember(15)]
            public string MOUSEOVERDISPLAYNAME;
            [ProtoMember(16)]
            public string MOUSEOVEROBJCNTID;
            [ProtoMember(17)]
            public string MOUSEOVEROBJID;
            [ProtoMember(18)]
            public string MOUSEOVEROBJNAME;
            [ProtoMember(19)]
            public string MOUSEOVERPERID;
            [ProtoMember(20)]
            public string MOUSEOVERUINAME;
            [ProtoMember(21)]
            public string MOUSEOVERUIX;
            [ProtoMember(22)]
            public string MOUSEOVERUIY;
            [ProtoMember(23)]
            public string MOUSEWINDOWX;
            [ProtoMember(24)]
            public string MOUSEWINDOWY;
            [ProtoMember(25)]
            public string MOUSEWINDOWZ;
            [ProtoMember(26)]
            public string MOUSEWORLDX;
            [ProtoMember(27)]
            public string MOUSEWORLDY;
            [ProtoMember(28)]
            public string MOUSEWORLDZ;
            [ProtoMember(29)]
            public string RANDOM;
            [ProtoMember(30)]
            public string SCANJOURNALMESSAGE;
            [ProtoMember(31)]
            public string SCANJOURNALTIME;
            [ProtoMember(32)]
            public string TARGETLOADING;
            [ProtoMember(33)]
            public string TARGETTYPE;
            [ProtoMember(34)]
            public string TIME;
            [ProtoMember(35)]
            public string TOOLTIPTEXT;
        }
        [ProtoMember(7)]
        public MiscellaneousStruct Miscellaneous;
    }
}
