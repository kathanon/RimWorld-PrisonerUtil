using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PrisonerUtil {
    public class CompUnreserve : ThingComp {
        public bool reserved = true;

        public bool IsReserved() => reserved;

        public void Toggle() => reserved = !reserved;

        public override void PostExposeData() {
            if (Scribe.mode != LoadSaveMode.Saving || !reserved) {
                Scribe_Values.Look(ref reserved, "reserved", true);
            }
        }

        // TODO: Reset if hauled out of prison.
    }
}
