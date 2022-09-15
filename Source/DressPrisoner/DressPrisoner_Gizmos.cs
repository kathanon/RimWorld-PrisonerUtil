using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PrisonerUtil {
    public static class DressPrisoner_Gizmos {
        private static readonly Thing defaultThing =
            DefDatabase<ThingDef>.GetNamed("Apparel_Pants").GetConcreteExample();
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

        public static IEnumerable<Gizmo> For(Apparel apparel) {
            yield return AddFor(apparel);
            if (CompDressPrisoner.QueuedIn(apparel, out var comp)) {
                yield return RemoveFor(comp, apparel);
            }
        }

        public static DressPrisoner_Gizmo<Apparel> AddFor(CompDressPrisoner comp) =>
            new DressPrisoner_Gizmo<Apparel>(
                target:  targetApparel,
                action:  comp.Add,
                menu:    comp.ApparelMenu,
                item:    defaultThing,
                tooltip: Strings.SelectForPrisoner(comp.parent),
                status:  comp.Status);

        public static DressPrisoner_Gizmo<Pawn> AddFor(Apparel item) =>
            new DressPrisoner_Gizmo<Pawn>(
                target:  targetPrisoner,
                action:  pawn => CompDressPrisoner.AddTo(item, pawn),
                menu:    () => CompDressPrisoner.PrisonerMenu(item),
                item:    item,
                tooltip: Strings.SelectForApparel(item),
                status:  () => CompDressPrisoner.Status(item));

        public static DressPrisoner_RemoveGizmo RemoveFor(CompDressPrisoner comp) =>
            new DressPrisoner_RemoveGizmo(
                action:  comp.Clear,
                menu:    comp.RemoveMenu,
                item:    defaultThing,
                tooltip: Strings.CancelForPrisoner(comp.parent),
                status:  comp.Status);

        public static DressPrisoner_RemoveGizmo RemoveFor(CompDressPrisoner comp, Apparel item) =>
            new DressPrisoner_RemoveGizmo(
                action:  () => comp.Remove(item),
                menu:    NoMenu,
                item:    item,
                tooltip: Strings.CancelForApparel(comp.parent, item),
                status:  () => CompDressPrisoner.Status(item));

        private static IEnumerable<FloatMenuOption> NoMenu() {
            yield break;
        }

        public static void DrawIconOverlay(Rect rect, Thing item, bool remove) {
            rect.size *= 0.5f;
            rect.y += rect.height;
            Widgets.ThingIcon(rect, item);
            rect.x += rect.width * 0.9f;
            rect.y -= rect.height * 0.2f;
            rect.size *= 0.85f;
            Widgets.DrawTextureFitted(rect, Resources.Arrow, 1f);
            if (remove) {
                rect.size *= 0.5f;
                rect.x += rect.width * 0.85f;
                rect.y += rect.height * 0.55f;
                Widgets.DrawTextureFitted(rect, Widgets.CheckboxOffTex, 1f);
            }
        }
    }

    public class DressPrisoner_Gizmo<T> : Command_Target where T : Thing {
        private readonly Thing item;
        private readonly Func<IEnumerable<FloatMenuOption>> menu;
        private readonly Func<string> status;

        public DressPrisoner_Gizmo(TargetingParameters target,
                                   Action<T> action,
                                   Func<IEnumerable<FloatMenuOption>> menu,
                                   Thing item,
                                   string tooltip,
                                   Func<string> status) {
            icon = Resources.PrisonerIcon;
            defaultLabel = Strings.DressPrisoner;
            defaultDesc = tooltip;
            this.action = t => action(t.Thing as T);
            targetingParams = target;
            this.item = item;
            this.menu = menu;
            this.status = status;
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => menu();

        public override string DescPostfix => status();

        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms) {
            base.DrawIcon(rect, buttonMat, parms);
            DressPrisoner_Gizmos.DrawIconOverlay(rect, item, false);
        }
    }

    public class DressPrisoner_RemoveGizmo : Command {
        private readonly Action action;
        private readonly Thing item;
        private readonly Func<IEnumerable<FloatMenuOption>> menu;
        private readonly Func<string> status;

        public DressPrisoner_RemoveGizmo(Action action,
                                         Func<IEnumerable<FloatMenuOption>> menu,
                                         Thing item,
                                         string tooltip,
                                         Func<string> status) {
            icon = Resources.PrisonerIcon;
            defaultLabel = Strings.CancelDressing;
            defaultDesc = tooltip;
            this.action = action;
            this.item = item;
            this.menu = menu;
            this.status = status;
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => menu();

        public override string DescPostfix => status();

        public override void ProcessInput(Event ev) {
            base.ProcessInput(ev);
            action();
        }

        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms) {
            base.DrawIcon(rect, buttonMat, parms);
            DressPrisoner_Gizmos.DrawIconOverlay(rect, item, true);
        }
    }
}
