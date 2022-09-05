using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PrisonerUtil {
    public class WorkGiver_Warden_Dress : WorkGiver_Warden {
        private static readonly JobDef Job = 
            DefDatabase<JobDef>.GetNamed("kathanon-PrisonerUtil-DressPrisoner");

        public override int MaxRegionsToScanBeforeGlobalSearch => 0;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => 
            pawn.Map.mapPawns.PrisonersOfColony
                .Select(p => p.GetComp<CompDressPrisoner>())
                .SelectMany(TargetsFor);

        private static IEnumerable<Thing> TargetsFor(CompDressPrisoner comp) {
            if (comp?.HasUnreserved ?? false) {
                yield return comp.parent;
                foreach (var item in comp.Unreserved) {
                    yield return item;
                }
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) => 
            CompDressPrisoner.JobOn(t);

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
            if (t is Pawn prisoner) {
                Apparel item = prisoner.GetComp<CompDressPrisoner>().Next;
                return JobMaker.MakeJob(Job, prisoner, item);
            } else if (t is Apparel item) {
                var match = pawn.Map.mapPawns.PrisonersOfColony
                    .Select(p => p.GetComp<CompDressPrisoner>())
                    .Where(p => p.IsUnreserved(item));
                if (match.Any()) {
                    return JobMaker.MakeJob(Job, match.First().parent, item);
                }
            }
            return null;
        }
    }
}
