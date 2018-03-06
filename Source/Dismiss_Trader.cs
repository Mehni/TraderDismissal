using System;
using RimWorld;
using Verse;
using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse.AI;

namespace Dismiss_Trader
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.traderdismissal.main");

            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FloatMenuMakerMap_AddHumanlikeOrdersToDismissTraders_PostFix)));
        }

        private static void FloatMenuMakerMap_AddHumanlikeOrdersToDismissTraders_PostFix(ref Vector3 clickPos, ref Pawn pawn, ref List<FloatMenuOption> opts)
        {
            foreach (LocalTargetInfo target in GenUI.TargetsAt(clickPos, TargetingParameters.ForTrade(), true))
            {
                Pawn localpawn = pawn;
                LocalTargetInfo dest = target;
                if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)) return;
                else if (pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled) return;
                else
                {
                    Pawn pTarg = (Pawn)dest.Thing;
                    Action action4 = delegate
                    {
                        Job job = new Job(TraderDismissalJobDefs.DismissTrader, pTarg)
                        {
                            playerForced = true
                        };
                        localpawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    string str = string.Empty;
                    if (pTarg.Faction != null)
                    {
                        str = " (" + pTarg.Faction.Name + ")";
                    }
                    string label = "GETOUT".Translate(new object[]
                    {
                        pTarg.LabelShort + ", " + pTarg.TraderKind.label
                    }) + str;
                    Action action = action4;
                    MenuOptionPriority priority2 = MenuOptionPriority.InitiateSocial;
                    Thing thing = dest.Thing;
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, priority2, null, thing, 0f, null, null), pawn, pTarg, "ReservedBy"));
                }
            }
            return;
        }
    }

    [DefOf]
    public class TraderDismissalJobDefs
    {
        public static JobDef DismissTrader;
    }
}