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
            InteractionModes.Convert;
        private static readonly PrisonerInteractionModeDef ConvertThenRecruit =
            InteractionModes.ConvertThenRecruit;

        private static bool replaceConvert = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ITab_Pawn_Visitor), "FillTab")]
        public static void FillTab_Pre() {
            if (Find.Selector.SingleSelectedThing is Pawn pawn
                    && pawn.guest.interactionMode == ConvertThenRecruit) {
                PrisonerInteractionModeDefOf.Convert = ConvertThenRecruit;
                replaceConvert = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ITab_Pawn_Visitor), "FillTab")]
        public static void FillTab_Post() {
            if (replaceConvert) {
                PrisonerInteractionModeDefOf.Convert = Convert;
                replaceConvert = false;
            }
            if (Find.Selector.SingleSelectedThing is Pawn pawn) {
                InteractionModes.Update(pawn);
            }
        }
    }
}
