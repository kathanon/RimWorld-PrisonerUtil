using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PrisonerUtil {
    public class JobDriver_DressPrisoner : JobDriver {
        private const TargetIndex PrisonerInd = TargetIndex.A;

        private const TargetIndex ClothingInd = TargetIndex.B;

        private int unequipTicks = 0;

        protected Pawn Prisoner => job.GetTarget(PrisonerInd).Pawn;

        protected Apparel Clothing => job.GetTarget(ClothingInd).Thing as Apparel;

        protected CompDressPrisoner DressComp => Prisoner.TryGetComp<CompDressPrisoner>();

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            if (pawn.Reserve(Prisoner, job, errorOnFailed: errorOnFailed) && 
                pawn.Reserve(Clothing, job, errorOnFailed: errorOnFailed)) { 
                return true;
            }

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            var target = Prisoner;
            var item = Clothing;
            var comp = DressComp;

            this.FailOnDestroyedOrNull(ClothingInd);
            this.FailOnBurningImmobile(ClothingInd);
            this.FailOnForbidden(ClothingInd);
            this.FailOnDestroyedOrNull(PrisonerInd);
            this.FailOnAggroMentalState(PrisonerInd);
            this.FailOnForbidden(PrisonerInd);
            this.FailOn(() => !(DressComp?.Has(item) ?? false));

            yield return Toils_Goto.GotoThing(ClothingInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(ClothingInd);
            yield return Toils_Haul.StartCarryThing(ClothingInd);
            yield return Toils_Goto.GotoThing(PrisonerInd, PathEndMode.ClosestTouch);

            yield return new Toil {
                tickAction = UnequipAndDelay,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = Duration,
            }.WithProgressBarToilDelay(PrisonerInd);

            yield return Toils_General.Do(delegate {
                pawn.carryTracker.TryDropCarriedThing(target.Position, ThingPlaceMode.Near, out var _);
                target.apparel.Wear(item);
                comp.Remove(item);
            });

            // TODO: Potential negative reaction?
            // TODO: Negative thought for prisoner
        }

        private Apparel ApparelToRemove {
            get {
                var item = Clothing.def;
                var prisoner = Prisoner;
                var prisonerBody = prisoner.RaceProps.body;
                foreach (var worn in prisoner.apparel.WornApparel) {
                    if (!ApparelUtility.CanWearTogether(item, worn.def, prisonerBody)) {
                        return worn;
                    }
                }
                return null;
            }
        }

        private IEnumerable<Apparel> AllApparelToRemove {
            get {
                var item = Clothing.def;
                var prisoner = Prisoner;
                var prisonerBody = prisoner.RaceProps.body;
                foreach (var worn in prisoner.apparel.WornApparel) {
                    if (!ApparelUtility.CanWearTogether(item, worn.def, prisonerBody)) {
                        yield return worn;
                    }
                }
            }
        }

        private static int DurationFor(Apparel item) => 
            (int) (item.GetStatValue(StatDefOf.EquipDelay) * 60f);

        private int Duration => AllApparelToRemove.Sum(DurationFor) + DurationFor(Clothing);

        private void UnequipAndDelay() {
            Pawn prisoner = Prisoner;
            if (prisoner.jobs.curJob?.def != JobDefOf.Wait) {
                var job = JobMaker.MakeJob(JobDefOf.Wait);
                job.expiryInterval = 10;
                prisoner.jobs.StartJob(job, JobCondition.InterruptOptional, resumeCurJobAfterwards: true);
            }

            var item = ApparelToRemove;
            if (item != null) {
                unequipTicks++;
                if (unequipTicks >= DurationFor(item)) {
                    unequipTicks = 0;
                    if (!prisoner.apparel.TryDrop(item, out var _, prisoner.PositionHeld, false)) {
                        Log.Error($"{prisoner} could not drop {item.ToStringSafe()}");
                        EndJobWith(JobCondition.Errored);
                    }
                }
            }
        }
    }
}
