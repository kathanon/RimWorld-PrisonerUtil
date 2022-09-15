using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PrisonerUtil {
    public class CompDressPrisoner : ThingComp {
        private List<Apparel> queue = new List<Apparel>();

        private static readonly Dictionary<Apparel, CompDressPrisoner> allQueued = 
            new Dictionary<Apparel, CompDressPrisoner>();
        private static readonly HashSet<Thing> jobOn = new HashSet<Thing>();
        private static bool firstInit = true;

        public bool HasItems => queue.Count > 0;

        public IEnumerable<Apparel> Items => queue;

        public Apparel Next => queue.FirstOrDefault();

        public static bool JobOn(Pawn pawn, Thing t) {
            if (jobOn.Contains(t)) {
                Thing t2 = null;
                if (t is Pawn prisoner) {
                    t2 = prisoner.TryGetComp<CompDressPrisoner>()?.Next;
                } else if (t is Apparel apparel && allQueued.TryGetValue(apparel, out var comp)) {
                    t2 = comp.parent;
                }
                var manager = t.Map.reservationManager;
                return t2 != null &&
                    manager.CanReserve(pawn, t) &&
                    manager.CanReserve(pawn, t2);
            }
            return false;
        }

        public static bool QueuedIn(Apparel item, out CompDressPrisoner comp) => 
            allQueued.TryGetValue(item, out comp);

        public bool Has(Apparel item) => queue.Contains(item);

        public void Add(Apparel item) {
            queue.Add(item);
            jobOn.Add(item);
            jobOn.Add(parent);
            if (allQueued.ContainsKey(item)) {
                allQueued[item].Remove(item);
            }
            allQueued.Add(item, this);
        }

        public void Remove(Apparel item) {
            queue    .Remove(item);
            jobOn    .Remove(item);
            allQueued.Remove(item);
            if (!HasItems) jobOn.Remove(parent);
        }

        public void Clear() {
            jobOn.Remove(parent);
            foreach (var item in queue) {
                jobOn.Remove(item);
                allQueued.Remove(item);
            }
            queue.Clear();
        }

        public IEnumerable<FloatMenuOption> ApparelMenu() {
            var map = parent.Map;
            var resevations = map.reservationManager;
            var faction = Faction.OfPlayer;
            var items = map.listerThings
                .ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Apparel))
                .Where(a => !resevations.IsReservedByAnyoneOf(a, faction));
            return MakeMenu(items, Add);
        }

        public IEnumerable<FloatMenuOption> RemoveMenu() => 
            MakeMenu(queue, Remove);

        private IEnumerable<FloatMenuOption> MakeMenu<T>(
            IEnumerable<T> items, Action<Apparel> action) where T : Thing {
            return items.Select(a => MakeOption(a as Apparel));

            FloatMenuOption MakeOption(Apparel a) =>
                new FloatMenuOption(a.Label, () => action(a), a.def);
        }

        public string PawnName => (parent as Pawn)?.Name.ToStringFull;

        public static readonly Color StatusColor = Color.yellow;

        public string Status() => HasItems ? $"\n{StatusDesc}\n{LineList}" : "";

        public static string Status(Apparel item) => 
            allQueued.TryGetValue(item, out var comp) 
                ? $"\n{StatusApparelDesc}\n{comp.PawnName.Colorize(StatusColor)}"
                : "";

        private static readonly string StatusDesc        = Strings.CurrentDressPrisoner;
        private static readonly string StatusApparelDesc = Strings.CurrentDressApparel;

        private string LineList =>
            queue.Select(a => a.LabelCap).ToLineList().Colorize(StatusColor);

        public static void AddTo(Apparel apparel, Pawn pawn) =>
            pawn.TryGetComp<CompDressPrisoner>()?.Add(apparel);

        public static IEnumerable<FloatMenuOption> PrisonerMenu(Apparel apparel) =>
            apparel.Map.mapPawns.PrisonersOfColony
                .Select(p => new FloatMenuOption(
                    p.Name.ToStringFull,
                    () => AddTo(apparel, p)
                ));

        public Gizmo AddGizmo => DressPrisoner_Gizmos.AddFor(this);

        public Gizmo RemoveGizmo => DressPrisoner_Gizmos.RemoveFor(this);

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            yield return AddGizmo;
            if (HasItems) {
                yield return RemoveGizmo;
            }
        }

        public static IEnumerable<Gizmo> GizmosFor(CompDressPrisoner comp) =>
            comp.CompGetGizmosExtra();

        public override void PostExposeData() {
            if (Scribe.mode != LoadSaveMode.Saving || HasItems) {
                Scribe_Collections.Look(ref queue, "queue", LookMode.Reference);
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                if (firstInit) {
                    firstInit = false;
                    jobOn.Clear();
                    allQueued.Clear();
                }
                jobOn.Add(parent);
                jobOn.AddRange(queue);
                queue.ForEach(a => allQueued.Add(a, this));
            } else {
                firstInit = true;
            }
        }
    }
}
