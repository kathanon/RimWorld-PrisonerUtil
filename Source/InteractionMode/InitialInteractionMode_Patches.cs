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
        private static bool menuAdditionOverridden = false;
        private static Pawn target = null;

        private static readonly Dictionary<Pawn,PrisonerInteractionModeDef> desiredModes = 
            new Dictionary<Pawn, PrisonerInteractionModeDef>();

        private static Regex arrestRegex;
        private static Regex captureRegex;
        private static readonly List<PrisonerInteractionModeDef> interactions =
            DefDatabase<PrisonerInteractionModeDef>.AllDefs
                .OrderBy(m => m.listOrder)
                .Where(FilterInteraction)
                .ToList();
        private static readonly List<PrisonerInteractionModeDef> interactionsNoBlood = 
            interactions
                .Where(x => !x.hideIfNoBloodfeeders)
                .ToList();

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
        public static void AddHumanlikeOrders_Post(List<FloatMenuOption> opts) {
            if (menuAdditionOverridden) return;

            for (int i = 0; i < opts.Count; i++) {
                var option = opts[i];
                string label = null;
                if (ArrestRegex.IsMatch(option.Label)) {
                    label = Strings.ArrestAndSet;
                } else if (CaptureRegex.IsMatch(option.Label)) {
                    label = Strings.CaptureAndSet;
                }
                if (label != null) {
                    opts.Insert(++i, InteractionSubMenu(label, option.action));
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

        public static void OverrideMenuAddition(bool overridden = true)
            => menuAdditionOverridden = overridden;

        public static IEnumerable<PrisonerInteractionModeDef> Interactions 
            => AnyBloodfeeders(Find.CurrentMap) ? interactions : interactionsNoBlood;

        private static bool AnyBloodfeeders(Map map)
            => map.mapPawns.FreeColonistsAndPrisoners.Any(x => x.IsBloodfeeder());

        public static FloatMenuOption InteractionSubMenu(
                string label, Action originalAction, Action<Rect> onMouseOver = null, Pawn capturee = null) 
            => FloatSubMenu.CompatMMMCreate(label, InteractionOptions(originalAction, onMouseOver, capturee));

        public static List<FloatMenuOption> InteractionOptions(
            Action originalAction, Action<Rect> onMouseOver = null, Pawn capturee = null) 
            => InteractionOptions(m => InteractionMenuOption(originalAction, m, onMouseOver, capturee));

        public static List<FloatMenuOption> InteractionOptions(
                Func<PrisonerInteractionModeDef, FloatMenuOption> makeOption)
            => Interactions.Select(makeOption).ToList();

        private static bool FilterInteraction(PrisonerInteractionModeDef mode) 
            => mode.allowInClassicIdeoMode || !Find.IdeoManager.classicMode;

        public static FloatMenuOption InteractionMenuOption(Action originalAction,
                                                            PrisonerInteractionModeDef mode, 
                                                            Action<Rect> onMouseOver = null, 
                                                            Pawn capturee = null) {
            return new FloatMenuOption(mode.LabelCap,
                                       (capturee == null) ? Action : ActionKnownPawn,
                                       mouseoverGuiAction: onMouseOver);

            void ActionKnownPawn() {
                originalAction();
                desiredModes[capturee] = mode;
            }

            void Action() {
                getTargetActive = true;
                originalAction();
                getTargetActive = false;
                if (target != null) {
                    desiredModes[target] = mode;
                }
            }
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
