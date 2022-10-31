using HugsLib.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

using ModeDef = RimWorld.PrisonerInteractionModeDef;

namespace PrisonerUtil {
    public static class Options {
        private static readonly Dictionary<string, ModeDef> interactionModes = 
            new Dictionary<string, ModeDef>();
        private static readonly List<ModeDef> interactionModeOrder = 
            new List<ModeDef>();

        private static SettingHandle<string> initialModeStranger;
        private static SettingHandle<string> initialModeColonist;
        private static SettingHandle<string> initialModeSlave;

        private static ModeDef ModeFor(SettingHandle<string> name) => 
            (name == null || name.Value == DefaultInteraction) ? null : interactionModes[name];

        public static ModeDef InitialInteractionModeStranger => ModeFor(initialModeStranger);
        public static ModeDef InitialInteractionModeColonist => ModeFor(initialModeColonist);
        public static ModeDef InitialInteractionModeSlave    => ModeFor(initialModeSlave);

        private const string DefaultInteraction = "NoInteraction";

        public static void Setup(ModSettingsPack settings) {
            interactionModes.Clear();
            interactionModeOrder.Clear();
            DefDatabase<ModeDef>.AllDefsListForReading
                .ForEach(def => { 
                    interactionModes.Add(def.defName, def);
                    interactionModeOrder.Add(def);
                });
            interactionModeOrder.SortBy(def => def.listOrder);

            initialModeStranger = settings.GetHandle(
                "initialModeStranger",
                Strings.InitModeStranger_title,
                Strings.InitModeStranger_desc,
                DefaultInteraction);
            initialModeStranger.CustomDrawer = 
                rect => DrawInitialModeOption(initialModeStranger, rect);

            initialModeColonist = settings.GetHandle(
                "initialModeColonist",
                Strings.InitModeColonist_title,
                Strings.InitModeColonist_desc,
                DefaultInteraction);
            initialModeColonist.CustomDrawer = 
                rect => DrawInitialModeOption(initialModeColonist, rect);

            initialModeSlave = settings.GetHandle(
                "initialModeSlave",
                Strings.InitModeSlave_title,
                Strings.InitModeSlave_desc,
                DefaultInteraction);
            initialModeSlave.CustomDrawer = 
                rect => DrawInitialModeOption(initialModeSlave, rect);
        }

        private const float Margin = 10f;

        private static bool DrawInitialModeOption(SettingHandle<string> option, Rect rect) {
            string value = option;
            if (!interactionModes.ContainsKey(value)) {
                value = DefaultInteraction;
            }

            Rect buttonRect = rect.ContractedBy(Margin, 0f);
            if (Widgets.ButtonText(buttonRect, interactionModes[value].LabelCap)) {
                FloatMenuUtility.MakeMenu(interactionModeOrder, def => def.LabelCap, actions);
            }

            return false;

            Action actions(ModeDef def) {
                var name = def.defName;
                if (name == value) {
                    return () => { };
                } else {
                    return () => option.Value = def.defName;
                }
            }
        }
    }
}
