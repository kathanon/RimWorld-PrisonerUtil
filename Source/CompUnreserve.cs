using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PrisonerUtil {
    public class CompUnreserve : ThingComp {
        private bool reserved = true;
        private bool init = false;

        public bool Reserved {
            get {
                if (!init) {
                    if (parent.IsInPrisonCell()) {
                        var room = parent.GetRoom();
                        reserved = parent.Map.mapPawns.PrisonersOfColony
                            .Where(x => x.GetRoom() == room)
                            .Select(x => x.foodRestriction.CurrentFoodRestriction)
                            .Any(x => x.Allows(parent.def));
                    }
                    init = true;
                }
                return reserved;
            }
        }

        public bool IsReserved() => Reserved;

        public void Toggle() {
            reserved = !reserved;
            init = true;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            if (!respawningAfterLoad) init = false;
        }

        public override void PostExposeData() {
            if (Scribe.mode != LoadSaveMode.Saving || !reserved) {
                Scribe_Values.Look(ref reserved, "reserved", true);
                Scribe_Values.Look(ref init, "init", true);
            }
        }

        // TODO: Reset if hauled out of prison.
    }
}
