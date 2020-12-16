using ProtoBuf;
using System;
using System.Collections.Generic;

namespace LOU
{
    public enum CommandType
    {
        NOP = 0,
        // EasyUO - Client Commands
        //ChooseSkill,
        //Click,
        //CmpPix,
        //ContPos,
        //DeleteJournal,
        FindItem,
        FindPermanent,
        //GetShopInfo,
        //GetUOTitle,
        //HideItem,
        //IgnoreItem,
        Key,
        Move,
        Stop, // Newly introduced
        //Msg,
        //NextCPos,
        //OnHotKey,
        //SavePix,
        //SetShopItem,
        //SetUOTitle,
        ScanJournal,
        //Sleep,
        //Target,
        //Terminate,
        //UoXL,
        //Wait,
        // EasyUO - Event Commands
        //Drag,
        //ExMsg,
        Macro,
        Say,
        SayCustom, // Newly introduced
        //Emote,
        //Whisper,
        //Yell,
        //WalkNorthWest,
        //WalkNorth,
        //WalkNorthEast,
        //WalkEast,
        //WalkSouthEast,
        //WalkSouth,
        //WalkSouthWest,
        //WalkWest,
        ToggleWarPeace,
        //Paste,
        //OpenConfiguration,
        //OpenPaperdoll,
        //OpenStatus,
        //OpenJournal,
        //OpenSkills,
        //OpenSpellbook,
        //OpenChat,
        //OpenBackpack,
        //OpenOverview,
        //OpenMail,
        //OpenPartyManifest,
        //OpenPartyChat,
        //OpenGuild,
        //OpenQuestlog,
        //CloseConfiguration,
        //ClosePaperdoll,
        //CloseStatus,
        //CloseJournal,
        //CloseSkills,
        //CloseSpellbook,
        //CloseChat,
        //CloseBackpack,
        //CloseOverview,
        //CloseMail,
        //ClosePartyManifest,
        //ClosePartyChat,
        //CloseGuild,
        //MinimizePaperdoll,
        //MinimizeStatus,
        //MinimizeJournal,
        //MinimizeSkills,
        //MinimizeSpellbook,
        //MinimizeChat,
        //MinimizeBackpack,
        //MinimizeOverview,
        //MinimizeMail,
        //MinimizePartyManifest,
        //MinimizePartyChat,
        //MinimizeGuild,
        //MaximizePaperdoll,
        //MaximizeStatus,
        //MaximizeJournal,
        //MaximizeSkills,
        //MaximizeSpellbook,
        //MaximizeChat,
        //MaximizeBackpack,
        //MaximizeOverview,
        //MaximizeMail,
        //MaximizePartyManifest,
        //MaximizePartyChat,
        //MaximizeGuild,
        //Opendoor,
        //UseSkillAnatomy,
        //UseSkillAnimalLore,
        //UseSkillAnimalTaming,
        //UseSkillArmsLore,
        //UseSkillBegging,
        //UseSkillCartography,
        //UseSkillDetectingHidden,
        //UseSkillDiscordance,
        //UseSkillEvaluatingIntelligence,
        //UseSkillForensicEvaluation,
        //UseSkillHiding,
        //UseSkillInscription,
        //UseSkillItemIdentification,
        //UseSkillMeditation,
        //UseSkillPeacemaking,
        //UseSkillPoisoning,
        //UseSkillProvocation,
        //UseSkillRemoveTrap,
        //UseSkillSpiritSpeak,
        //UseSkillStealing,
        //UseSkillStealth,
        //UseSkillTasteIdentification,
        //UseSkillTracking,
        //UseSkillImbuing,
        //LastSkill,
        //CastSpellClumsy,
        //CastSpellCreateFood,
        //CastSpellFeeblemind,
        //CastSpellHeal,
        //CastSpellMagicArrow,
        //CastSpellNightSight,
        //CastSpellReactiveArmor,
        //CastSpellWeaken,
        //CastSpellAgility,
        //CastSpellCunning,
        //CastSpellCure,
        //CastSpellHarm,
        //CastSpellMagicTrap,
        //CastSpellMagicUntrap,
        //CastSpellProtection,
        //CastSpellStrength,
        //CastSpellBless,
        //CastSpellFireball,
        //CastSpellMagicLock,
        //CastSpellPoison,
        //CastSpellTelekinesis,
        //CastSpellTeleport,
        //CastSpellUnlock,
        //CastSpellWallOfStone,
        //CastSpellArchCure,
        //CastSpellArchProtection,
        //CastSpellCurse,
        //CastSpellFireField,
        //CastSpellGreaterHeal,
        //CastSpellLightning,
        //CastSpellManaDrain,
        //CastSpellRecall,
        //CastSpellBladeSpirits,
        //CastSpellDispelField,
        //CastSpellIncognito,
        //CastSpellMagicReflection,
        //CastSpellMindBlast,
        //CastSpellParalyze,
        //CastSpellPoisonField,
        //CastSpellSummonCreature,
        //CastSpellDispel,
        //CastSpellEnergyBolt,
        //CastSpellExplosion,
        //CastSpellInvisibility,
        //CastSpellMark,
        //CastSpellMassCurse,
        //CastSpellParalyzeField,
        //CastSpellReveal,
        //CastSpellChainLightning,
        //CastSpellEnergyField,
        //CastSpellFlameStrike,
        //CastSpellGateTravel,
        //CastSpellManaVampire,
        //CastSpellMassDispel,
        //CastSpellMeteorSwarm,
        //CastSpellPolymorph,
        //CastSpellEarthquake,
        //CastSpellEnergyVortex,
        //CastSpellResurrection,
        //CastSpellAirElemental,
        //CastSpellSummonDaemon,
        //CastSpellEarthElemental,
        //CastSpellFireElemental,
        //CastSpellWaterElemental,
        //LastSpell,
        //LastObject,
        //Bow,
        //Salute,
        //QuitGame,
        //AllNames,
        TargetDynamic, // Newly introduced
        TargetPermanent, // Newly introduced
        TargetLoc, // Newly introduced
        LastTarget,
        TargetSelf,
        //ArmDisarmLeft,
        //ArmDisarmRight,
        WaitForTarget, // This is implemented client side! See ScriptDebugger.cs in EasyLOU project
        //TargetNext,
        //AttackLast,
        //Delay,
        //Circletrans,
        //CloseGumps,
        //AlwaysRun,
        //SaveDesktop,
        //KillGumpOpen,
        //PrimaryAbility,
        //SecondaryAbility,
        //EquipLastWeapon,
        //SetUpdateRange,
        //ModifyUpdateRange,
        //IncreaseUpdateRange,
        //DecreaseUpdateRange,
        //MaximumUpdateRange,
        //MinimumUpdateRange,
        //DefaultUpdateRange,
        //UpdateUpdateRange,
        //EnableUpdateRangeColor,
        //DisableUpdateRangeColor,
        //ToggleUpdateRangeColor,
        //SelectNext,
        //SelectPrevious,
        //SelectNearest,
        AttackSelected,
        UseSelected,
        ContextMenu, // Newly Introduced
        //CurrentTarget,
        //TargetingSystemOnOff,
        //ToggleBuffWindow,
        //BandageSelf,
        //BandageTarget,
        //PathFind,
        //Property,
        //SkillLock,
        //Sleep,
        //SysMessage,
        // EasyUO - ExEvent Commands
        Drag,
        Dropc,
        //Droppd,
        Dropg,
        //Popup,
        //RenamePet,
        //SkillLock,
        //StatLock,
        FindMobile,
        SetUsername,
        SetPassword,
        Login,
        SelectServer,
        CharacterSelect,
        FindPanel,  // Newly introduced
        FindButton, // Newly introduced
        FindInput, // Newly introduced
        FindLabel, // Newly introduced
        ClickButton,
        SetInput,
        SetTargetFrameRate,
        SetVSyncCount,
        SetMainCameraMask,
        GetTooltip,
        ResetVars,
        SetCustomVar,
        ClearCustomVar,
        Logout,
        ClosePanel
    }

    [ProtoContract]
    public class ClientCommand
    {
        [ProtoMember(1)]
        public long TimeStamp;

        [ProtoMember(2)]
        public CommandType CommandType;

        [ProtoContract]
        public enum CommandParamTypeEnum
        {
            [ProtoEnum]
            Void  = 0,
            [ProtoEnum]
            Boolean,
            [ProtoEnum]
            Number,
            [ProtoEnum]
            String
        }
        [ProtoContract]
        public struct CommandParamStruct
        {
            [ProtoMember(1)]
            public CommandParamTypeEnum CommandParamType;
            [ProtoMember(2)]
            public bool Boolean;
            [ProtoMember(3)]
            public double Number;
            [ProtoMember(4)]
            public string String;
        }
        [ProtoMember(3)]
        public Dictionary<String, CommandParamStruct> CommandParams = new Dictionary<String, CommandParamStruct>();

        public ClientCommand()
        {
            this.TimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
        public ClientCommand(CommandType CommandType)
        {
            this.TimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            this.CommandType = CommandType;
        }
    }
}
