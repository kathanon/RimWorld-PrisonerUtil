using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PrisonerUtil {
    public class CompDressPrisoner : ThingComp {
        private List<Apparel> queue = new List<Apparel>();
        private HashSet<Apparel> reserved = new HashSet<Apparel>();

        private static readonly HashSet<Thing> jobOn = new HashSet<Thing>();
        private static bool firstInit = true;

        public bool HasItems => queue.Count > 0;

        public bool HasUnreserved => queue.Count > reserved.Count;

        public Apparel Next {
            get {
                for (int i = 0; i < queue.Count; i++) {
                    var next = queue[i];
                    if (!reserved.Contains(next)) {
                        return next;
                    }
                }
                return null;
            }
        }

        public IEnumerable<Apparel> Unreserved => 
            queue.Where(p => !reserved.Contains(p));

        public static bool JobOn(Thing t) => jobOn.Contains(t);

        public void Add(Apparel item) {
            queue.Add(item);
            jobOn.Add(item);
            jobOn.Add(parent);
        }

        public void Remove(Apparel item) {
            reserved.Remove(item);
            queue   .Remove(item);
            jobOn   .Remove(item);
            if (!HasUnreserved) jobOn.Remove(parent);
        }

        public bool IsReserved(Apparel item) => 
            reserved.Contains(item);

        public bool IsUnreserved(Apparel item) => 
            queue.Contains(item) && !reserved.Contains(item);

        public void Reserve(Apparel item) {
            reserved.Add(item);
            jobOn   .Remove(item);
            if (!HasUnreserved) jobOn.Remove(parent);
        }

        public void Unreserve(Apparel item) {
            reserved.Remove(item);
            jobOn   .Add(item);
            jobOn   .Add(parent);
        }

        public IEnumerable<FloatMenuOption> ApparelMenu() {
            var map = parent.Map;
            var resevations = map.reservationManager;
            var faction = Faction.OfPlayer;
            return map.listerThings
                .ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Apparel))
                .Where(a => !resevations.IsReservedByAnyoneOf(a, faction))
                .Select(a => MakeOption(a as Apparel));

            FloatMenuOption MakeOption(Apparel a) =>
                new FloatMenuOption(a.Label, () => Add(a), a.def);
        }

        public override string ToString() => 
            HasItems ? queue.Select(t => (reserved.Contains(t) ? "*" : "") + t.Label).ToCommaList() : "empty";

        public static IEnumerable<FloatMenuOption> PrisonerMenu(Apparel apparel) =>
            apparel.Map.mapPawns.PrisonersOfColony
                .Select(p => new FloatMenuOption(
                    p.Label,
                    () => p.TryGetComp<CompDressPrisoner>()?.Add(apparel)
                ));

        public Gizmo PrisonerGizmo => DressPrisoner_Gizmo.For(this);

        public override void PostExposeData() {
            if (Scribe.mode != LoadSaveMode.Saving || HasItems) {
                Scribe_Collections.Look(ref queue,    "queue",    LookMode.Reference);
                Scribe_Collections.Look(ref reserved, "reserved", LookMode.Reference);
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                if (firstInit) {
                    firstInit = false;
                    jobOn.Clear();
                }
                if (HasUnreserved) {
                    jobOn.Add(parent);
                    jobOn.AddRange(Unreserved);
                }
            } else {
                firstInit = true;
            }
        }
    }
}
