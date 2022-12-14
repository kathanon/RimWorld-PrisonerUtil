using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using FloatSubMenus;
using UnityEngine;

namespace PrisonerUtil {
    [HarmonyPatch]
    public static class InitialInteractionMode_Patches {
        private static bool takeToBedActive = false;
        private static bool setGuestStatusActive = false;
        private static bool getTargetActive = false;
        private static Pawn target = null;

        private static readonly Dictionary<Pawn,PrisonerInteractionModeDef> desiredModes = 
            new Dictionary<Pawn, PrisonerInteractionModeDef>();

        private static Regex arrestRegex;
        private static Regex captureRegex;
        private static readonly List<PrisonerInteractionModeDef> interactions =
            DefDatabase<PrisonerInteractionModeDef>.AllDefs.OrderBy(m => m.listOrder).ToList();

        private static Regex ArrestRegex  => MakeRegex("TryToArrest", ref arrestRegex);
        private static Regex CaptureRegex => MakeRegex("Capture",     ref captureRegex);

        private static Regex MakeRegex(string key, ref Regex variable) {
            if (variable == null) {
                var raw = key.Translate().RawText;
                int tag = raw.IndexOf("{");
                var pattern = (tag >= 0) ? raw.Substring(0, tag) : raw;
                variable = new Regex("^" + pattern);
            }
            return variable;
        }

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static void AddHumanlikeOrders_Post(List<FloatMenuOption> opts, Vector3 clickPos, Pawn pawn) {
            for (int i = 0; i < opts.Count; i++) {
                var option = opts[i];
                string label = null;
                if (ArrestRegex.IsMatch(option.Label)) {
                    label = Strings.ArrestAndSet;
                } else if (CaptureRegex.IsMatch(option.Label)) {
                    label = Strings.CaptureAndSet;
                }
                if (label != null) {
                    opts.Insert(++i, InteractionMenuOption(label, option.action));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(JobMaker), nameof(JobMaker.MakeJob), 
            typeof(JobDef), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
        public static void MakeJob_Post(LocalTargetInfo targetA) {
            if (getTargetActive) {
                target = targetA.Pawn;
            }
        }

        private static FloatMenuOption InteractionMenuOption(string label, Action action) {
            if (Main.VUIE_Active) {
                return new FloatMenuOption(label + "...", () => DoInteractionMenu(action));
            } else {
                return new FloatSubMenu(label, InteractionOptions(action));
            }
        }

        private static List<FloatMenuOption> InteractionOptions(Action action) =>
            interactions
                .Where(FilterInteraction)
                .Select(m => InteractionMenuOption(action, m))
                .ToList();

        private static bool FilterInteraction(PrisonerInteractionModeDef mode) =>
            mode.allowInClassicIdeoMode || !Find.IdeoManager.classicMode;

        private static void DoInteractionMenu(Action action) => 
            Find.WindowStack.Add(new FloatMenu(InteractionOptions(action)));

        private static FloatMenuOption InteractionMenuOption(Action originalAction,
                                                             PrisonerInteractionModeDef mode) {
            Action action = delegate {
                getTargetActive = true;
                originalAction();
                getTargetActive = false;
                if (target != null) {
                    desiredModes[target] = mode;
                }
            };
            return new FloatMenuOption(mode.LabelCap, action);
        }

        private static void SetInitialInteractionMode(Pawn pawn) {
            if (desiredModes.TryGetValue(pawn, out var mode)) {
                desiredModes.Remove(pawn);
            } else if (pawn.IsSlave) {
                mode = Options.InitialInteractionModeSlave;
            } else if (pawn.IsColonist) {
                mode = Options.InitialInteractionModeColonist;
            } else { // Captured stranger
                mode = Options.InitialInteractionModeStranger;
            }
            if (mode != null) {
                pawn.guest.interactionMode = mode;
                InteractionModes.Update(pawn);
            }
        }
    }
}
