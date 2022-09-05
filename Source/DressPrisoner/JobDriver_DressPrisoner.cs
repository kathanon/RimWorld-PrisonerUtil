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

        protected Pawn Prisoner => job.GetTarget(PrisonerInd).Pawn;

        protected Apparel Clothing => job.GetTarget(ClothingInd).Thing as Apparel;

        protected CompDressPrisoner DressComp => Prisoner.TryGetComp<CompDressPrisoner>();

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            if (pawn.Reserve(Prisoner, job, errorOnFailed: errorOnFailed) && 
                pawn.Reserve(Clothing, job, errorOnFailed: errorOnFailed)) { 
                DressComp.Reserve(Clothing);
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
            this.FailOnDestroyedOrNull(PrisonerInd);
            this.FailOnAggroMentalState(PrisonerInd);
            this.FailOn(() => !(DressComp?.IsReserved(item) ?? false));

            yield return Toils_Goto.GotoThing(ClothingInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(ClothingInd);
            yield return Toils_Haul.StartCarryThing(ClothingInd);
            yield return Toils_Goto.GotoThing(PrisonerInd, PathEndMode.ClosestTouch);

            yield return new Toil {
                initAction = delegate {
                    // TODO: Potential negative reaction?
                    // TODO: Negative thought for prisoner
                    target.equipment.MakeRoomFor(item);
                    item.DeSpawn();
                    target.equipment.AddEquipment(item);
                    if (item.def.soundInteract != null) {
                        item.def.soundInteract.PlayOneShot(new TargetInfo(target.Position, target.Map));
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
