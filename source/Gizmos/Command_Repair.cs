﻿using RimWorld;
using Verse;

namespace WM.SelfLaunchingPods
{
	public class Command_Repair : Command_Traveler
	{
		public Command_Repair(WorldTraveler parent) : base(parent)
		{
			this.defaultLabel = "WM.RepairGizmo".Translate();
			this.defaultDesc = string.Format("WM.RepairGizmoDesc".Translate(), this.Parent.ComponentsCountNeededToRepair, this.Parent.ComponentsCountAvailable, ThingDefOf.ComponentIndustrial);
			this.icon = Resources.GizmoRepair;
			this.action = delegate
			{
				int num = this.Parent.ComponentsCountNeededToRepair;
				var state = parent.TryRepair();

				switch (state)
				{
					case RepairState.NoCapablePawns:
						Messages.Message(string.Format("WM.MessageNoCapableColonistsInFleet".Translate()), MessageTypeDefOf.RejectInput);
						break;

					case RepairState.NoMaterials:
						Messages.Message(string.Format("WM.MessageAtLeastComponentsNeeded".Translate(), 
														this.Parent.ComponentsCountNeededToRepair, 
														ThingDefOf.ComponentIndustrial.label, 
														this.Parent.ComponentsCountAvailable), 
														MessageTypeDefOf.NegativeEvent);
						break;

					case RepairState.Success:
						Messages.Message(string.Format("WM.MessagePodsRepairedComponentsUsed".Translate(),
														num,
						                               	ThingDefOf.ComponentIndustrial.label),
						                 				MessageTypeDefOf.NeutralEvent);
						break;
				}
			};
		}

		public override bool Visible
		{
			get
			{
				return Parent.AnythingToRepair();
			}
		}
	}
}
