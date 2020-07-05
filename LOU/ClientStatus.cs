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
        [ProtoMember(2)]
        public Dictionary<String, String> CharacterInfo = new Dictionary<string, String>();
        [ProtoMember(3)]
        public Dictionary<String, String> StatusBar = new Dictionary<string, String>();
        [ProtoMember(4)]
        public Dictionary<String, String> ContainerInfo = new Dictionary<string, String>();
        [ProtoMember(5)]
        public Dictionary<String, String> LastAction = new Dictionary<string, String>();
        [ProtoMember(6)]
        public Dictionary<String, String> Find = new Dictionary<string, String>();
        [ProtoMember(7)]
        public Dictionary<String, String> ShopInfo = new Dictionary<string, String>();
        [ProtoMember(8)]
        public Dictionary<String, String> ExtendedInfo = new Dictionary<string, String>();
        [ProtoMember(9)]
        public Dictionary<String, String> ClientInfo = new Dictionary<string, String>();
        [ProtoMember(10)]
        public Dictionary<String, String> CombatInfo = new Dictionary<string, String>();
        [ProtoMember(11)]
        public Dictionary<String, String> TileInfo = new Dictionary<string, String>();
        [ProtoMember(12)]
        public Dictionary<String, String> TimeInfo = new Dictionary<string, String>();
        [ProtoMember(13)]
        public Dictionary<String, String> Miscellaneous = new Dictionary<string, String>();
        [ProtoMember(14)]
        public Dictionary<String, String> ResultVariables = new Dictionary<string, String>();
    }
}
