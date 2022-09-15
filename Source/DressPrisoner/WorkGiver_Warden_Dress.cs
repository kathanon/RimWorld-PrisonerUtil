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
        private const TargetIndex PrisonerInd = TargetIndex.A;

        private static readonly JobDef DressPrisonerDef = 
            DefDatabase<JobDef>.GetNamed("kathanon-PrisonerUtil-DressPrisoner");

        public override int MaxRegionsToScanBeforeGlobalSearch => 0;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => 
            pawn.Map.mapPawns.PrisonersOfColony
                .Select(p => p.GetComp<CompDressPrisoner>())
                .SelectMany(TargetsFor);

        private static IEnumerable<Thing> TargetsFor(CompDressPrisoner comp) {
            if (comp?.HasItems ?? false) {
                yield return comp.parent;
                foreach (var item in comp.Items) {
                    yield return item;
                }
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) => 
            CompDressPrisoner.JobOn(pawn, t);

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
            if (t is Pawn prisoner) {
                Apparel item = prisoner.GetComp<CompDressPrisoner>().Next;
                if (item != null) {
                    return CreateJob(prisoner, item, false);
                }
            } else if (t is Apparel item) {
                if (CompDressPrisoner.QueuedIn(item, out var comp)) {
                    return CreateJob(comp.parent, item, true);
                }
            }
            return null;
        }

        private bool lastJobFromItem;

        public override string PostProcessedGerund(Job job) {
            if (lastJobFromItem) {
                var prisoner = job.GetTarget(PrisonerInd).Pawn;
                return Strings.DressingIn(prisoner);
            } else {
                return base.PostProcessedGerund(job);
            }
        }

        private Job CreateJob(Thing pawn, Apparel item, bool fromItem) {
            lastJobFromItem = fromItem;
            Job job = JobMaker.MakeJob(DressPrisonerDef, pawn, item);
            job.count = 1;
            job.haulMode = HaulMode.ToCellNonStorage;
            job.haulDroppedApparel = true;
            return job;
        }
    }
}
