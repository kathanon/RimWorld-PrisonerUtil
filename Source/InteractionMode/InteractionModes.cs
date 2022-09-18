using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PrisonerUtil {
    public static class InteractionModes {
        public static readonly PrisonerInteractionModeDef Convert =
            PrisonerInteractionModeDefOf.Convert;
        public static readonly PrisonerInteractionModeDef ConvertThenRecruit =
            DefDatabase<PrisonerInteractionModeDef>.GetNamed("kathanon-PrisonerUtil-ConvertThenRecruit");

        public static void Update(Pawn pawn) {
            var guest = pawn.guest;
            var mode = guest.interactionMode;
            if (guest.ideoForConversion == null && (mode == ConvertThenRecruit || mode == Convert)) {
                guest.ideoForConversion = Faction.OfPlayer.ideos.PrimaryIdeo;
            }

        }
    }
}
