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
    public static class DressPrisoner_Patches {
        /* TODO: 
         * - Gizmo on prisoner - target for clothing, right-click menu   -> needs polish
         * - Gizmo on clothing - target for prisoners, right-click menu  -> needs polish
         */

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
        public static IEnumerable<Gizmo> PawnGizmos(IEnumerable<Gizmo> result, Pawn __instance) =>
            result.Concat(
                __instance.AllComps
                .OfType<CompDressPrisoner>()
                .SelectMany(CompDressPrisoner.GizmosFor));

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.GetGizmos))]
        public static IEnumerable<Gizmo> ApparelGizmos(IEnumerable<Gizmo> result, ThingWithComps __instance) {
            if (__instance is Apparel a && a.Map.mapPawns.PrisonersOfColonyCount > 0) {
                result = result.Concat(DressPrisoner_Gizmos.For(a));
            }
            return result;
        }
    }
}
