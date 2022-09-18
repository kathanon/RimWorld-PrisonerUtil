using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace PrisonerUtil {
    public class JobGiver_AttackDresser : ThinkNode_JobGiver {
        protected override Job TryGiveJob(Pawn pawn) {
            if (pawn.MentalState is MentalState_AttackDresser state
                && state.IsTargetStillValidAndReachable()) {
                var attack = state.target.SpawnedParentOrMe;
                Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, attack);
                if (attack != state.target) {
                    job.maxNumMeleeAttacks = 2;
                }
                return job;
            }

            return null;
        }
    }
}
