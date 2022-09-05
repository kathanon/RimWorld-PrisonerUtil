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

        // Settings
        public static readonly string InitModeStranger_title = (PREFIX + "InitModeStranger.title").Translate();
        public static readonly string InitModeStranger_desc  = (PREFIX + "InitModeStranger.desc" ).Translate();
        public static readonly string InitModeColonist_title = (PREFIX + "InitModeColonist.title").Translate();
        public static readonly string InitModeColonist_desc  = (PREFIX + "InitModeColonist.desc" ).Translate();
        public static readonly string InitModeSlave_title    = (PREFIX + "InitModeSlave.title"   ).Translate();
        public static readonly string InitModeSlave_desc     = (PREFIX + "InitModeSlave.desc"    ).Translate();

        // Resources
        public static readonly string GhostTexturePath = MOD_IDENTIFIER + "/Ghost";
    }
}
