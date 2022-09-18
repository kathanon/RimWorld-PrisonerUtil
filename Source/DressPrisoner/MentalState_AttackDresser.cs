using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PrisonerUtil {
    public class MentalState_AttackDresser : MentalState_MurderousRage {
        public override void PreStart() {
            target = pawn.TryGetComp<CompDressPrisoner>()?.lastDresser;
        }

        public override void MentalStateTick() {
            var old = target;
            if (target == null || target.Downed || target.Dead
                || (pawn.IsHashIntervalTick(120) && !IsTargetStillValidAndReachable())
                || !pawn.Awake() || pawn.Downed) {
                RecoverFromState();
            } else {
                base.MentalStateTick();
            }
        }
    }
}
