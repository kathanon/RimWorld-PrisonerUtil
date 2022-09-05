using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PrisonerUtil {
    [HarmonyPatch]
    public static class UnreserveMeals_Patches {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Thing), nameof(Thing.GetGizmos))]
        public static IEnumerable<Gizmo> GetGizmos_Post(IEnumerable<Gizmo> result, Thing __instance) {
            foreach (var gizmo in result) {
                yield return gizmo;
            }

            var thing = __instance;
            var comp = thing.TryGetComp<CompUnreserve>();
            if (comp != null && thing.Position.IsInPrisonCell(thing.Map)) {
                yield return new Command_Toggle {
                    icon = Resources.PrisonerIcon,
                    isActive = comp.IsReserved,
                    toggleAction = comp.Toggle,
                    defaultLabel = Strings.ReservedForPrisoner_title,
                    defaultDesc = Strings.ReservedForPrisoner_desc,
                };
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SocialProperness), 
                      nameof(SocialProperness.IsSociallyProper),
                      new Type[] { typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) })]
        public static bool IsSociallyProper_Post(bool result, Thing t, bool forPrisoner) {
            if (!result && !forPrisoner) {
                var comp = t.TryGetComp<CompUnreserve>();
                if (comp != null) {
                    return !comp.reserved;
                }
            }
            return result;
        }
    }
}
