using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PrisonerUtil
{
    public class Main : HugsLib.ModBase {
        public override string ModIdentifier => Strings.MOD_IDENTIFIER;

        private static readonly CompProperties Unreserve = 
            new CompProperties(typeof(CompUnreserve));

        public override void DefsLoaded() {
            Options.Setup(Settings);
            SetupThingInfo();
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
            ThingInfo.Add<Pawn>(p => $"DressPrisoner: {p.TryGetComp<CompDressPrisoner>()}");
        }

        /* TODO: 
         * - Option to select default interaction for new prisoners - done
         * - Assign prisoner beds - done
         * - Unmark prisoner meals - done
         * - Put apparel on prisoner
         */
    }
}
