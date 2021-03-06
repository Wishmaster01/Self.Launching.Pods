using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace WM.SelfLaunchingPods
{
	public static class TravelingPodsUtils
	{
		public static void RemoveAllPawnsFromWorldPawns(IEnumerable<Pawn> pawns)
		{
			foreach (var item in pawns)
			{
				if (item.IsWorldPawn())
				{
					Find.WorldPawns.RemovePawn(item);
				}
			}
		}

		internal static WorldTraveler CreateWorldTraveler(int tile, IEnumerable<ActiveDropPodInfo> podsInfo, IEnumerable<Thing> podsLandedThings)
		{
			var hopper = (WorldTraveler)WorldObjectMaker.MakeWorldObject(WM.SelfLaunchingPods.WorldObjectDefOf.WM_TravelingTransportPods);

			hopper.Tile = tile;
			hopper.SetFaction(Faction.OfPlayer);

			var max = podsInfo.Count();

			for (int i = 0; i < max; i++)
			{
				hopper.AddPod(podsLandedThings.ElementAt(i), podsInfo.ElementAt(i));
			}

			Find.WorldObjects.Add(hopper);

			return (hopper);
		}

		internal static void ToCaravan(Caravan caravan, IEnumerable<Thing> thingsToTransfer)
		{
			var list = thingsToTransfer.ToList();

			foreach (var item in list)
			{
				item.holdingOwner.Remove(item);
				var pawn = item as Pawn;
				if (pawn != null)
				{
					caravan.AddPawn(pawn, true);
					if (!pawn.IsWorldPawn())
						Find.WorldPawns.PassToWorld(pawn);
				}
				else
					CaravanInventoryUtility.GiveThing(caravan, item);
			}
		}

		internal static bool FromCaravan(WorldTraveler traveler, Caravan caravan, IEnumerable<Thing> thingsToTransfer)
		{
			if (MissingMassCapacity(traveler, thingsToTransfer.ToList()) > 0)
			{
				return (false);
			}

			DistributeThingsForPods(traveler, thingsToTransfer);

			if (!caravan.pawns.Any && Find.WorldObjects.Contains(caravan))
			{
				Find.WorldObjects.Remove(caravan);
			}

			return (true);
		}

		static void DistributeThingsForPods(WorldTraveler traveler, IEnumerable<Thing> thingsToTransfer)
		{
			var list = thingsToTransfer.InRandomOrder().ToList();
			int thingsCountPerPod = Mathf.CeilToInt(list.Count() / traveler.PodsCount);
			var podsEnumerator = traveler.Pods.GetEnumerator();
			int i = 0;

			podsEnumerator.MoveNext();
			foreach (var item in list)
			{
				if (item is Pawn)
				{
					var pawn = (Pawn)item;
					if (pawn.IsWorldPawn())
						Find.WorldPawns.RemovePawn(pawn);
				}
				if (!item.holdingOwner.TryTransferToContainer(item, podsEnumerator.Current.PodInfo.innerContainer))
					Log.Warning($"Could not transfer {item}");
				i++;
				if (i > thingsCountPerPod)
				{
					i = 0;
					if (!podsEnumerator.MoveNext())
						podsEnumerator.Reset();
				}
			}
		}

		internal static void MergeTravelers(WorldTraveler alpha, WorldTraveler beta)
		{
			var list = beta.Pods.ToList();

			foreach (var item in list)
			{
				beta.RemovePod(item);
				alpha.AddPod(item);
			}

			Find.WorldObjects.Remove(beta);
		}

		internal static float MissingMassCapacity(WorldTraveler traveler, List<Thing> thingsToLoad)
		{
			return (CollectionsMassCalculator.MassUsage<Thing>(thingsToLoad, IgnorePawnsInventoryMode.Ignore, true) - (traveler.MassCapacity - traveler.MassUsage));
		}

		internal static float CaravanMass(List<Thing> thingsToLoad)
		{
			return CollectionsMassCalculator.MassUsage<Thing>(thingsToLoad, IgnorePawnsInventoryMode.Ignore, true);
		}

		internal static int RequiredPodsCountForMass(float mass)
		{
			return Mathf.CeilToInt((mass / ((CompProperties_Transporter)ThingDefOf.WM_TransportPod.comps.Find((obj) => obj is CompProperties_Transporter)).massCapacity));
		}
	}
}