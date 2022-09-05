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
    public static class AssignedBed_Patches {
        // ----- Allow bed assignment for prisoners. -----
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "ShouldShowAssignmentGizmo")]
        public static bool ShouldShowAssignmentGizmo_Post(bool result, Thing ___parent) =>
            result || IsPrisonerBed(___parent);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "GetAssignmentGizmoDesc")]
        public static string GetAssignmentGizmoDesc_Post(string result, Thing ___parent) =>
            IsPrisonerBed(___parent) ? Strings.SetOwnerPrisoner : result;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CompAssignableToPawn_Bed), 
                      nameof(CompAssignableToPawn_Bed.AssigningCandidates), 
                      MethodType.Getter)]
        public static bool AssigningCandidates_Get_Pre(ref IEnumerable<Pawn> __result, Thing ___parent) {
            bool prisoner = IsPrisonerBed(___parent);
            if (prisoner) {
                __result = ___parent.Map.mapPawns.PrisonersOfColony;
            }
            return !prisoner;
        }

        private static bool IsPrisonerBed(Thing thing) =>
            thing is Building_Bed bed &&
            bed.Faction == Faction.OfPlayer &&
            bed.ForPrisoners &&
           !bed.Medical;


        // ----- Escort prisoner to assigned bed even if there is a free bed in same room. -----
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RestUtility),
                      nameof(RestUtility.FindBedFor),
                      new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(GuestStatus?) })]
        public static bool FindBedFor_Pre(Pawn sleeper, Pawn traveler, ref Thing __result) {
            if (sleeper.IsPrisoner 
                && sleeper == traveler 
                && sleeper.ownership?.OwnedBed != null
                && sleeper.ownership.OwnedBed.GetRoom() != sleeper.GetRoom()) {
                __result = null;
                return false;
            }
            return true;
        }


        // ----- Do not unset player-set owned bed if interrupted while escorting, -----
        //       and abort escorting if player changes assignment.
        private static Job blockFinishActionFor = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(JobDriver_TakeToBed), "MakeNewToils")]
        public static void MakeNewToils_Pre(JobDriver_TakeToBed __instance) {
            var job = __instance.job;
            var pawn = (Pawn) job.GetTarget(TargetIndex.A).Thing;
            var bed = (Building_Bed) job.GetTarget(TargetIndex.B).Thing;
            if (job.def.makeTargetPrisoner && pawn.ownership.OwnedBed == bed) {
                blockFinishActionFor = job;
                __instance.AddFailCondition(() => pawn.ownership?.OwnedBed != bed);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.AddFinishAction))]
        public static bool AddFinishAction_Pre(JobDriver __instance) {
            var job = __instance.job;
            bool res = true;
            if (blockFinishActionFor == job) {
                res = false;
                blockFinishActionFor = null;
            }
            return res;
        }
    }
}
