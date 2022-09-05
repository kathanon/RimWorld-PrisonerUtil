using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PrisonerUtil {
    public class WorkGiver_Warden_ConvertThenRecruit : Workgiver_Warden_CTRBase {
        protected override JobDef Job => JobDefOf.PrisonerConvert;

        protected override bool CorrectPhase(Pawn actor, Pawn target) =>
            target.Ideo != target.guest.ideoForConversion &&
            actor.Ideo == target.guest.ideoForConversion;
    }
}
