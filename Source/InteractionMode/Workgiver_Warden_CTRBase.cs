using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PrisonerUtil {
    public abstract class Workgiver_Warden_CTRBase : WorkGiver_Warden {
        protected static readonly PrisonerInteractionModeDef ConvertThenRecruit =
            DefDatabase<PrisonerInteractionModeDef>.GetNamed("kathanon-PrisonerUtil-ConvertThenRecruit");

        public override Job JobOnThing(Pawn actor, Thing t, bool forced = false) =>
            ActiveFor(actor, t as Pawn) ? JobMaker.MakeJob(Job, t) : null;

        protected abstract JobDef Job { get; }

        protected bool ActiveFor(Pawn actor, Pawn target) =>
            target != null &&
            ShouldTakeCareOfPrisoner(actor, target) &&
            target.guest.interactionMode == ConvertThenRecruit &&
            target.guest.IsPrisoner &&
            target.guest.ScheduledForInteraction &&
            actor.health.capacities.CapableOf(PawnCapacityDefOf.Talking) &&
            (!target.Downed || target.InBed()) &&
            actor.CanReserve(target) &&
            target.Awake() &&
            CorrectPhase(actor, target);

        protected abstract bool CorrectPhase(Pawn actor, Pawn target);
    }
}
