using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
#if SERVER
		[ChatCommand.Region("kobold", "", creative: true)]
		public static void KoboldCommand(ref ChatCommand.Context context, byte? faction_id = null)
		{
			ref var region = ref context.GetRegion();
			ref var player = ref context.GetPlayer();
			var random = XorRandom.New(true);

			var h_species = new ISpecies.Handle("kobold");

			Span<IOrigin.Handle> origins = stackalloc IOrigin.Handle[16];
			IOrigin.Database.GetHandlesFiltered(ref origins, in h_species, (IOrigin.Definition d_origin, in ISpecies.Handle h_species) =>
			{
				return d_origin.data.species == h_species;
			});

			var h_character = Spawner.CreateCharacter(ref region, ref random, h_origin: origins.GetRandom(ref random), h_faction: faction_id ?? player.faction_id);

			Spawner.TryGenerateKits(ref random, h_character);
			Spawner.SpawnCharacter(ref region, h_character, player.control.mouse.position, h_faction: faction_id ?? player.faction_id).ContinueWith((ent) =>
			{
				ref var character = ref h_character.GetData();
				if (character.IsNotNull())
				{
					Loadout.Spawn(ent, character.kits);
				}
			});
		}

		[ChatCommand.Region("human", "", creative: true)]
		public static void HumanCommand(ref ChatCommand.Context context, byte? faction_id = null)
		{
			ref var region = ref context.GetRegion();
			ref var player = ref context.GetPlayer();
			var random = XorRandom.New(true);

			var h_species = new ISpecies.Handle("human");

			Span<IOrigin.Handle> origins = stackalloc IOrigin.Handle[16];
			IOrigin.Database.GetHandlesFiltered(ref origins, in h_species, (IOrigin.Definition d_origin, in ISpecies.Handle h_species) =>
			{
				return d_origin.data.species == h_species;
			});

			var h_character = Spawner.CreateCharacter(ref region, ref random, h_origin: origins.GetRandom(ref random), h_faction: faction_id ?? player.faction_id);

			Spawner.TryGenerateKits(ref random, h_character);
			Spawner.SpawnCharacter(ref region, h_character, player.control.mouse.position, h_faction: faction_id ?? player.faction_id).ContinueWith((ent) =>
			{
				ref var character = ref h_character.GetData();
				if (character.IsNotNull())
				{
					Loadout.Spawn(ent, character.kits);
				}
			});
		}

		[ChatCommand.Region("giant", "", creative: true)]
		public static void GiantCommand(ref ChatCommand.Context context, byte? faction_id = null)
		{
			ref var region = ref context.GetRegion();
			ref var player = ref context.GetPlayer();
			var random = XorRandom.New(true);

			var h_species = new ISpecies.Handle("giant");

			Span<IOrigin.Handle> origins = stackalloc IOrigin.Handle[16];
			IOrigin.Database.GetHandlesFiltered(ref origins, in h_species, (IOrigin.Definition d_origin, in ISpecies.Handle h_species) =>
			{
				return d_origin.data.species == h_species;
			});

			var h_character = Spawner.CreateCharacter(ref region, ref random, h_origin: origins.GetRandom(ref random), h_faction: faction_id ?? player.faction_id);

			Spawner.TryGenerateKits(ref random, h_character);
			Spawner.SpawnCharacter(ref region, h_character, player.control.mouse.position, h_faction: faction_id ?? player.faction_id).ContinueWith((ent) =>
			{
				ref var character = ref h_character.GetData();
				if (character.IsNotNull())
				{
					Loadout.Spawn(ent, character.kits);
				}
			});
		}

		[ChatCommand.Region("tank", "", creative: true)]
		public static void TankCommand(ref ChatCommand.Context context, string prefab_name, byte? faction_id = null)
		{
			ref var region = ref context.GetRegion();
			ref var player = ref context.GetPlayer();

			//prefab.tractor.tank

			var pos_tmp = player.control.mouse.position;
			var h_faction_tmp = (IFaction.Handle)(faction_id ?? player.faction_id);

			//region.SpawnPrefab("tank.00.combined", pos_tmp, faction_id: h_faction_tmp).ContinueWith((ent_vehicle) =>
			region.SpawnPrefab(prefab_name, pos_tmp, faction_id: h_faction_tmp).ContinueWith((ent_vehicle) =>
			{
				ref var region = ref ent_vehicle.GetRegion();
				var random = XorRandom.New(true);

				var h_character = Spawner.CreateCharacter(ref region, ref random, "kobold.gunner", h_faction: h_faction_tmp);
				Spawner.SpawnCharacter(ref region, h_character, pos_tmp, h_faction: h_faction_tmp).ContinueWith((ent_kobold) =>
				{
					ref var region = ref ent_vehicle.GetRegion();

					//SetKoboldLoadout(ent_kobold);

					//ref var ai = ref ent_kobold.GetComponent<AI.Data>();
					//if (!ai.IsNull())
					//{
					//	ai.stance = AI.Stance.Aggressive;
					//}

					ref var vehicle = ref ent_vehicle.GetComponent<Vehicle.Data>();
					if (vehicle.IsNotNull())
					{
						App.WriteLine(vehicle.ent_seat_test);

						Vehicle.AddPassenger(vehicle.ent_seat_test, ent_kobold);
					}
				});
			});
		}

		[ChatCommand.Region("tractor", "", creative: true)]
		public static void TractorCommand(ref ChatCommand.Context context, byte? faction_id = null)
		{
			ref var region = ref context.GetRegion();
			ref var player = ref context.GetPlayer();

			//prefab.tractor.tank

			var pos_tmp = player.control.mouse.position;
			var h_faction_tmp = (IFaction.Handle)(faction_id ?? player.faction_id);

			//region.SpawnPrefab("tank.00.combined", pos_tmp, faction_id: h_faction_tmp).ContinueWith((ent_vehicle) =>
			region.SpawnPrefab("cmb.tractor.00.tank", pos_tmp, faction_id: h_faction_tmp).ContinueWith((ent_vehicle) =>
			{
				ref var region = ref ent_vehicle.GetRegion();

				region.SpawnPrefab("kobold.male", pos_tmp, faction_id: h_faction_tmp).ContinueWith((ent_kobold) =>
				{
					ref var region = ref ent_vehicle.GetRegion();

					//SetKoboldLoadout(ent_kobold);

					//ref var ai = ref ent_kobold.GetComponent<AI.Data>();
					//if (!ai.IsNull())
					//{
					//	ai.stance = AI.Stance.Aggressive;
					//}

					ref var vehicle = ref ent_vehicle.GetComponent<Vehicle.Data>();
					if (vehicle.IsNotNull())
					{
						App.WriteLine(vehicle.ent_seat_test);

						Vehicle.AddPassenger(vehicle.ent_seat_test, ent_kobold);
					}
				});
			});
		}

		[ChatCommand.Region("nextmap", "", admin: true)]
		public static void NextMapCommand(ref ChatCommand.Context context, string map)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				//var map_handle = new Map.Handle(map);
				//App.WriteLine($"nextmapping {map}; {map_handle}; {map_handle.id}");

				//if (map_handle.id != 0)
				{
					Siege.ChangeMap(ref region, map);
				}
			}
		}

		[ChatCommand.Region("reward", "", admin: true)]
		public static void RewardCommand(ref ChatCommand.Context context, float amount)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_bounty = ref region.GetSingletonComponent<Siege.Bounty.Global>();
				if (!g_siege_bounty.IsNull())
				{
					var rewards_span = g_siege_bounty.rewards.AsSpan();
					rewards_span.Add(Crafting.Product.Money(amount));
					region.SyncGlobal(ref g_siege_bounty);

					if (context.GetConnection().IsNotNull())
					{
						Server.SendChatMessage($"Added {amount:0.00} {Money.symbol} to payout.", channel: Chat.Channel.System, target_player_id: context.GetConnection().GetPlayerID());
					}

					//else
					//{
					//	Server.SendChatMessage($"Current difficulty: {g_siege_state.difficulty:0.00}.", channel: Chat.Channel.System, target_player_id: context.GetConnection().GetPlayerID());
					//}
				}
			}
		}

		[ChatCommand.Region("difficulty", "", admin: true)]
		public static void DifficultyCommand(ref ChatCommand.Context context, float difficulty)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					var difficulty_old = g_siege_state.difficulty;
					g_siege_state.difficulty = difficulty;

					region.SyncGlobal(ref g_siege_state);

					if (context.GetConnection().IsNotNull())
					{
						Server.SendChatMessage($"Set difficulty from {difficulty_old:0.00} to {g_siege_state.difficulty:0.00}.", channel: Chat.Channel.System, target_player_id: context.GetConnection().GetPlayerID());
					}

					//else
					//{
					//	Server.SendChatMessage($"Current difficulty: {g_siege_state.difficulty:0.00}.", channel: Chat.Channel.System, target_player_id: context.GetConnection().GetPlayerID());
					//}
				}
			}
		}

		[ChatCommand.Region("nextwave", "", admin: true)]
		public static void NextWaveCommand(ref ChatCommand.Context context)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					if (g_siege_state.status == Gamemode.Status.Preparing)
					{
						g_siege_state.status = Gamemode.Status.Running;
					}
					else
					{
						g_siege_state.t_next_wave = g_siege_state.t_match_elapsed;
						Server.SendChatMessage($"Forced next wave.", channel: Chat.Channel.System);
					}

					region.SyncGlobal(ref g_siege_state);
				}
			}
		}

		[ChatCommand.Region("setwave", "", admin: true)]
		public static void SetWaveCommand(ref ChatCommand.Context context, int wave)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					if (g_siege_state.status == Gamemode.Status.Preparing)
					{
						g_siege_state.status = Gamemode.Status.Running;
					}
					
					g_siege_state.wave_current = (ushort)(wave - 1);
					g_siege_state.t_next_wave = g_siege_state.t_match_elapsed;
					Server.SendChatMessage($"Set wave to {wave}.", channel: Chat.Channel.System);
					
					region.SyncGlobal(ref g_siege_state);
				}
			}
		}

		[ChatCommand.Region("pause", "", admin: true)]
		public static void PauseCommand(ref ChatCommand.Context context, bool? value = null)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					var sync = false;
					sync |= g_siege_state.flags.TrySetFlag(Siege.Gamemode.Flags.Paused, value ?? !g_siege_state.flags.HasAll(Siege.Gamemode.Flags.Paused));
					Server.SendChatMessage(g_siege_state.flags.HasAll(Siege.Gamemode.Flags.Paused) ? "Paused Siege." : "Unpaused Siege.", channel: Chat.Channel.System);

					if (sync)
					{
						region.SyncGlobal(ref g_siege_state);
					}
				}
			}
		}

		[ChatCommand.Region("dispatcher", "", admin: true)]
		public static void DispatcherCommand(ref ChatCommand.Context context, bool? value = null)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					var sync = false;
					sync |= g_siege_state.flags.TrySetFlag(Siege.Gamemode.Flags.No_Dispatcher, value ?? !g_siege_state.flags.HasAll(Siege.Gamemode.Flags.No_Dispatcher));
					Server.SendChatMessage(g_siege_state.flags.HasAll(Siege.Gamemode.Flags.No_Dispatcher) ? "Disabled Dispatcher AI." : "Enabled Dispatcher AI.", channel: Chat.Channel.System);

					if (sync)
					{
						region.SyncGlobal(ref g_siege_state);
					}
				}
			}
		}

		public static void ChangeMap(ref Region.Data region, MapInfo map)
		{
			//ref var region = ref world.GetAnyRegion();
			if (!region.IsNull())
			{
				var region_id_old = region.GetID();

				if (map == null)
				{
					map = App.GetModContext().GetMaps().GetRandom();
				}

				ref var world = ref Server.GetWorld();
				if (world.TryGetFirstAvailableRegionID(out var region_id_new))
				{
					region.Wait().ContinueWith(() =>
					{
						Net.SetActiveRegionForAllPlayers(0);

						ref var world = ref Server.GetWorld();
						world.UnloadRegion(region_id_old).ContinueWith(() =>
						{
							ref var world = ref Server.GetWorld();

							ref var region_new = ref world.ImportRegion2(region_id_new, map);
							if (!region_new.IsNull())
							{
								world.SetContinueRegionID(region_id_new);

								region_new.Wait().ContinueWith(() =>
								{
									Net.SetActiveRegionForAllPlayers(region_id_new);
								});
							}
						});
					});
				}
			}
		}
#endif
	}
}
