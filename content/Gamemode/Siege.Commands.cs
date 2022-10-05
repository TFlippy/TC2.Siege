using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
#if SERVER
		[ChatCommand.Region("kobold", "", creative: true)]
		public static void KoboldCommand(ref ChatCommand.Context context)
		{
			ref var region = ref context.GetRegion();
			ref var player = ref context.GetPlayer();

			region.SpawnPrefab("kobold.male", player.control.mouse.position, faction_id: player.faction_id).ContinueWith((ent) =>
			{
				SetKoboldLoadout(ent);

				ref var ai = ref ent.GetComponent<AI.Data>();
				if (!ai.IsNull())
				{
					ai.stance = AI.Stance.Aggressive;
				}
			});
		}

		[ChatCommand.Region("nextmap", "", creative: true)]
		public static void NextMapCommand(ref ChatCommand.Context context, string map)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				var map_handle = new Map.Handle(map);
				//App.WriteLine($"nextmapping {map}; {map_handle}; {map_handle.id}");

				//if (map_handle.id != 0)
				{
					Siege.ChangeMap(ref region, map_handle);
				}
			}
		}

		public static void ChangeMap(ref Region.Data region, Map.Handle map)
		{
			ref var world = ref Server.GetWorld();

			//ref var region = ref world.GetAnyRegion();
			if (!region.IsNull())
			{
				var region_id_old = region.GetID();

				if (world.TryGetFirstAvailableRegionID(out var region_id_new))
				{
					world.UnloadRegion(region_id_old).ContinueWith(() =>
					{
						ref var world = ref Server.GetWorld();

						ref var region_new = ref world.ImportRegion(region_id_new, map);
						if (!region_new.IsNull())
						{
							world.SetContinueRegionID(region_id_new);

							region_new.Wait().ContinueWith(() =>
							{
								Net.SetActiveRegionForAllPlayers(region_id_new);
							});
						}
					});
				}
			}
		}
#endif
	}
}
