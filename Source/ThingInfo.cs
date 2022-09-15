using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace PrisonerUtil {
    [HarmonyPatch]
    public static class ThingInfo {
        private static readonly Type[] Types = new Type[] {
            typeof(Thing),
            typeof(Plant),
            typeof(DeadPlant),
            typeof(MinifiedThing),
            typeof(MinifiedTree),
            typeof(MonumentMarker),
        };
        private const string MethodName = nameof(Thing.GetInspectString);

        private static readonly List<Action<Thing,StringBuilder>> InfoLines =
            new List<Action<Thing, StringBuilder>>();

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> Methods() =>
            Types.Select(t => t.GetMethod(MethodName));

        [HarmonyPostfix]
        public static string GetInspectString(string original, Thing __instance) {
            if (!InfoLines.Any()) return original;
            var buf = new StringBuilder();
            foreach (var line in InfoLines) {
                line.Invoke(__instance, buf);
            }
            if (buf.Length > 0) {
                buf.AppendLine("------------");
            }
            if (original.Length > 0) {
                buf.AppendLine(original);
            }
            return buf.ToString().TrimEndNewlines();
        }

        public static void Add<T>(Func<T, string> info, Predicate<T> select = null) where T : Thing {
            if (select == null)
                select = t => true;
            InfoLines.Add((Thing thing, StringBuilder buf) => {
                if (thing is T t && select.Invoke(t)) {
                    var str = info.Invoke(t);
                    if (!str.NullOrEmpty()) {
                        if (buf.Length == 0) {
                            buf.AppendLine("------------");
                        }
                        buf.AppendLine(str);
                    }
                }
            });
        }
    }
}
