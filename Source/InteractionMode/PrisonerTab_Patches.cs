using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PrisonerUtil {
    [HarmonyPatch]
    public class PrisonerTab_Patches {
        private static readonly PrisonerInteractionModeDef Convert = 
            PrisonerInteractionModeDefOf.Convert;
        private static readonly PrisonerInteractionModeDef ConvertThenRecruit =
            DefDatabase<PrisonerInteractionModeDef>.GetNamed("kathanon-PrisonerUtil-ConvertThenRecruit");

        private static bool replaceConvert = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ITab_Pawn_Visitor), "FillTab")]
        public static void FillTab_Pre() {
            Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
            if (pawn.guest.interactionMode == ConvertThenRecruit) {
                PrisonerInteractionModeDefOf.Convert = ConvertThenRecruit;
                replaceConvert = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ITab_Pawn_Visitor), "FillTab")]
        public static void FillTab_Post() {
            if (replaceConvert) {
                PrisonerInteractionModeDefOf.Convert = Convert;
            }
        }
    }
}
