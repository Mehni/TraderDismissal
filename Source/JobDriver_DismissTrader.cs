﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using Verse.AI.Group;

namespace Dismiss_Trader
{
    class JobDriver_DismissTrader : JobDriver
    {
        private Pawn Trader
        {
            get
            {
                return (Pawn)base.TargetThingA;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.Trader, this.job, 1, -1, null);
        }

        //approach: find Lord transition that is the regular time-out and add another (very short) Trigger_TicksPassed. That'll then fire, and the traders will leave.

        //other (failed) approaches: 
        //- inheriting from LordJob_TradeWithColony and overriding the stategraph. Set a bool in the job, which works as a trigger. Still seems like the "correct" and OOP approach, but I suck at C#
        //- adding new LordToil_ExitMapAndEscortCarriers() & telling the lord to jump to it. (lord null, somehow not registered in graph?)
        //- Outright removing the lord. Works, but also removes the traderflag, defending at exit and the group behaviour. Bad.
        //- transpile all the things! (ain't noone got time for that)
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !this.Trader.CanTradeNow);
            Toil trade = new Toil();
            trade.initAction = delegate
            {
                Pawn actor = trade.actor;
                if (this.Trader.CanTradeNow)
				{
                    Lord lord = Trader.GetLord();
                    List<Transition> transitions = lord.Graph.transitions.ToList();
                    for (int i = 0; i < transitions.Count; i++)
                    {
                        if (i == 7)
                        {
                            transitions[i].triggers.Add(new Trigger_TicksPassed(20));
                        }
                    }
                }
            };
            yield return trade;
        }
    }
}
