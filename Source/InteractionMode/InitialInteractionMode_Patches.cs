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
    public static class InitialInteractionMode_Patches {
        private static bool takeToBedActive = false;
        private static bool setGuestStatusActive = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(JobDriver_TakeToBed), "CheckMakeTakeePrisoner")]
        public static void CheckMakeTakeePrisoner_Pre(JobDriver_TakeToBed __instance) {
            Job job = __instance.job;
            Pawn target = (Pawn) job.GetTarget(TargetIndex.A).Thing;
            if (job.def.makeTargetPrisoner && target.guest.Released) {
                takeToBedActive = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(JobDriver_TakeToBed), "CheckMakeTakeePrisoner")]
        public static void CheckMakeTakeePrisoner_Post(JobDriver_TakeToBed __instance) {
            if (takeToBedActive) {
                takeToBedActive = false;
                SetInitialInteractionMode((Pawn) __instance.job.GetTarget(TargetIndex.A).Thing);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
        public static void SetGuestStatus_Pre(Pawn_GuestTracker __instance, Faction newHost, GuestStatus guestStatus) {
            var tracker = __instance;
            bool wasNotPrisoner = newHost != tracker.HostFaction || !tracker.IsPrisoner;
            if (guestStatus == GuestStatus.Prisoner && wasNotPrisoner) {
                setGuestStatusActive = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
        public static void SetGuestStatus_Post(Pawn ___pawn) {
            if (setGuestStatusActive) {
                setGuestStatusActive = false;
                SetInitialInteractionMode(___pawn);
            }
        }

        private static void SetInitialInteractionMode(Pawn pawn) {
            PrisonerInteractionModeDef mode = null;
            if (pawn.IsSlave) {
                mode = Options.InitialInteractionModeSlave;
            } else if (pawn.IsColonist) {
                mode = Options.InitialInteractionModeColonist;
            } else {
                // Captured stranger
                mode = Options.InitialInteractionModeStranger;
            }
            if (mode != null) {
                pawn.guest.interactionMode = mode;
            }
        }
    }
}
