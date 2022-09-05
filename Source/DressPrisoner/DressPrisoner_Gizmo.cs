using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PrisonerUtil {
    public static class DressPrisoner_Gizmo {
        private static readonly ThingDef defaultThing =
            DefDatabase<ThingDef>.GetNamed("Apparel_Pants");
        private static readonly TargetingParameters targetPrisoner =
            new TargetingParameters {
                validator = t => (t.Thing as Pawn)?.IsPrisonerOfColony ?? false,
            };
        private static readonly TargetingParameters targetApparel =
            new TargetingParameters {
                mapObjectTargetsMustBeAutoAttackable = false,
                canTargetPawns = false,
                canTargetItems = true,
                validator = t => t.Thing?.def.IsApparel ?? false,
            };

        public static DressPrisoner_Gizmo<Apparel> For(CompDressPrisoner comp) =>
            new DressPrisoner_Gizmo<Apparel>(
                targetApparel,
                comp.Add,
                comp.ApparelMenu(),
                defaultThing);

        public static DressPrisoner_Gizmo<Apparel> For(Apparel apparel) =>
            new DressPrisoner_Gizmo<Apparel>(
                targetPrisoner,
                null,
                CompDressPrisoner.PrisonerMenu(apparel),
                apparel.def);
    }

    public class DressPrisoner_Gizmo<T> : Command_Target where T : Thing {
        private readonly ThingDef item;
        private readonly IEnumerable<FloatMenuOption> menu;

        public DressPrisoner_Gizmo(TargetingParameters target,
                                         Action<T> action,
                                         IEnumerable<FloatMenuOption> menu,
                                         ThingDef item) {
            icon = Resources.PrisonerIcon;
            defaultLabel = "Dress Prisoner";
            defaultDesc = "TODO tooltip";
            this.action = t => action(t.Thing as T);
            targetingParams = target;
            this.item = item;
            this.menu = menu;
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => menu;

        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms) {
            base.DrawIcon(rect, buttonMat, parms);
            rect.size *= .5f;
            Widgets.DefIcon(rect, item);
        }
    }
}
