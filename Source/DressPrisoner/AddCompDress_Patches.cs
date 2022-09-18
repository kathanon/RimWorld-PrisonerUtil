using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PrisonerUtil {
    [HarmonyPatch]
    public static class AddCompDress_Patches {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
        public static void SetGuestStatus_Post(Pawn ___pawn) => Update(___pawn);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Map), nameof(Map.FinalizeLoading))]
        public static void FinalizeLoading_Post(MapPawns ___mapPawns) => 
            ___mapPawns.AllPawnsSpawned.ForEach(Update);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static void Kill_Post(Pawn __instance) => Update(__instance);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.DeSpawn))]
        public static void DeSpawn_Post(Pawn __instance) => Update(__instance);

        private static void Update(Pawn pawn) {
            var comp = pawn.TryGetComp<CompDressPrisoner>();
            if (pawn.IsPrisonerOfColony && !pawn.Dead) {
                if (comp == null) {
                    pawn.AllComps.Add(new CompDressPrisoner { parent = pawn });
                }
            } else {
                if (comp != null) {
                    comp.Clear();
                    pawn.AllComps.Remove(comp);
                }
            }
        }
    }
}
