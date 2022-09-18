using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PrisonerUtil
{
    public class Main : HugsLib.ModBase {
        public override string ModIdentifier => Strings.MOD_IDENTIFIER;

        public static bool VUIE_Active = 
            LoadedModManager.RunningMods
            .Where(m => m.PackageId == Strings.VUIE_ID)
            .Any();

        private static readonly CompProperties Unreserve = 
            new CompProperties(typeof(CompUnreserve));

        public override void DefsLoaded() {
            Options.Setup(Settings);
            //SetupThingInfo();
            SetupDefs();
        }

        private static void SetupDefs() {
            var defs = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.ingestible != null);
            foreach (var def in defs) { 
                def.comps.Add(Unreserve);
            }
        }

        private static void SetupThingInfo() {
            ThingInfo.Add<Thing>(t => $"{t.def.defName} at {t.Position}");
        }
    }
}
