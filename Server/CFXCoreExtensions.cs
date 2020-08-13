using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using static CitizenFX.Core.Native.API;

namespace racing.Server
{
	public static class CFXCoreExtensions
	{
		public static void SetIntoVehicle(this Ped ped, Vehicle vehicle, int seat = -1)
		{
			SetPedIntoVehicle(ped.Handle, vehicle.Handle, seat);
		}

		// Using a conditional weak table should stop the tag storage from filling with players we don't even know about anymore.
		public static ConditionalWeakTable<Player, List<string>> PlayerTags = new ConditionalWeakTable<Player, List<string>>();

		public static List<string> GetTags(this Player player) => PlayerTags.GetOrCreateValue(player);

		/// <summary>
		/// Adds the <paramref name="tag"/> to the player.
		/// </summary>
		public static void AddTag(this Player player, string tag)
		{
			var playerTags = PlayerTags.GetOrCreateValue(player);
			if (!playerTags.Contains(tag))
				playerTags.Add(tag);
		}

		/// <summary>
		/// Removes the <paramref name="tag"/> from the player.
		/// </summary>
		public static void RemoveTag(this Player player, string tag)
		{
			var playerTags = player.GetTags();
			if (playerTags.Contains(tag))
				playerTags.Remove(tag);
		}

		public static bool HasTag(this Player player, string tag) => player.HasTags(tag);

		public static bool HasTags(this Player player, params string[] tags)
		{
			List<string> playerTags = PlayerTags.GetOrCreateValue(player);

			if (playerTags.Count == 0)
				return false;

			foreach (string tag in tags)
			{
				if (!playerTags.Contains(tag))
					return false;
			}

			return true;
		}

		public static List<Player> WithTag(this PlayerList playerList, string tag) => playerList.WithTags(tag);

		public static List<Player> WithTags(this PlayerList playerList, params string[] tags)
		{
			List<Player> playersWithTags = new List<Player>();

			foreach (Player player in playerList)
			{
				if (player.HasTags(tags))
					playersWithTags.Add(player);
			}

			return playersWithTags;
		}
	}
}
