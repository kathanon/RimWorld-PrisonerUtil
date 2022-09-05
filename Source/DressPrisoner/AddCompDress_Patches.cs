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
    public static class AddCompDress_Patches {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
        public static void SetGuestStatus_Post(Pawn ___pawn) => Update(___pawn);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Map), nameof(Map.FinalizeLoading))]
        public static void FinalizeLoading_Post(MapPawns ___mapPawns) => 
            ___mapPawns.AllPawnsSpawned.ForEach(Update);

        private static void Update(Pawn pawn) {
            var comp = pawn.TryGetComp<CompDressPrisoner>();
            if (pawn.IsPrisonerOfColony) {
                if (comp == null) {
                    pawn.AllComps.Add(new CompDressPrisoner { parent = pawn });
                }
            } else {
                if (comp != null) {
                    pawn.AllComps.Remove(comp);
                }
            }
        }
    }
}
