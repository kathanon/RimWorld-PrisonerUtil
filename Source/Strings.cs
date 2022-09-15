using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PrisonerUtil
{
    internal static class Strings
    {
        // Non-translated constants
        public const string MOD_IDENTIFIER = "kathanon.PrisonerUtil";
        public const string PREFIX = MOD_IDENTIFIER + ".";

        // UI
        public static readonly string SetOwnerPrisoner          = (PREFIX + "SetOwnerPrisoner.desc"    ).Translate();
        public static readonly string ReservedForPrisoner_title = (PREFIX + "ReservedForPrisoner.title").Translate();
        public static readonly string ReservedForPrisoner_desc  = (PREFIX + "ReservedForPrisoner.desc" ).Translate();
        public static readonly string ArrestAndSet              = (PREFIX + "ArrestAndSet"             ).Translate();
        public static readonly string CaptureAndSet             = (PREFIX + "CaptureAndSet"            ).Translate();
        public static readonly string CurrentDressPrisoner      = (PREFIX + "CurrentDressPrisoner"     ).Translate();
        public static readonly string CurrentDressApparel       = (PREFIX + "CurrentDressApparel"      ).Translate();
        public static readonly string DressPrisoner             = (PREFIX + "DressPrisoner"            ).Translate();
        public static readonly string CancelDressing            = (PREFIX + "CancelDressing"           ).Translate();

        // UI - parametrized
        public static string SelectForPrisoner(Thing pawn) => (PREFIX + "SelectForPrisoner").Translate(pawn);
        public static string SelectForApparel (Thing item) => (PREFIX + "SelectForApparel" ).Translate(item);
        public static string CancelForPrisoner(Thing pawn) => (PREFIX + "CancelForPrisoner").Translate(pawn);
        public static string DressingIn       (Thing pawn) => (PREFIX + "DressingIn"       ).Translate(pawn);

        public static string CancelForApparel(Thing pawn, Thing item) => 
            (PREFIX + "CancelForApparel").Translate(pawn, item);

        // Settings
        public static readonly string InitModeStranger_title = (PREFIX + "InitModeStranger.title").Translate();
        public static readonly string InitModeStranger_desc  = (PREFIX + "InitModeStranger.desc" ).Translate();
        public static readonly string InitModeColonist_title = (PREFIX + "InitModeColonist.title").Translate();
        public static readonly string InitModeColonist_desc  = (PREFIX + "InitModeColonist.desc" ).Translate();
        public static readonly string InitModeSlave_title    = (PREFIX + "InitModeSlave.title"   ).Translate();
        public static readonly string InitModeSlave_desc     = (PREFIX + "InitModeSlave.desc"    ).Translate();

        // Resources
        public static readonly string ArrowTexturePath = MOD_IDENTIFIER + "/Arrow";

        // Vanilla resources
        public static readonly string ForPrisonersTexturePath = "UI/Commands/ForPrisoners";
    }
}
