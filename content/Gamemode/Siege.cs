using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		[IGamemode.Data("Siege", "")]
		public partial struct Gamemode: IGamemode
		{
			/// <summary>
			/// Reward sharing ratio based on player count.
			/// <code>
			/// Maths.Lerp(1.00f, 1.00f / player_count, reward_share_ratio);
			/// </code>
			/// </summary>
			[Save.Ignore] public float reward_share_ratio = 0.50f;

			[Save.Ignore] public float reward_mult = 1.50f;

			/// <summary>
			/// Base value of per-wave difficulty step.
			/// </summary>
			[Save.Ignore] public float difficulty_step = 1.00f;

			/// <summary>
			/// Player multiplier for the per-wave difficulty step.
			/// <code>
			/// difficulty_step *= 1.00f + (player_count * difficulty_player_mult);
			/// </code>
			/// </summary>
			[Save.Ignore] public float difficulty_player_mult = 0.20f;

			[Save.Ignore] public float difficulty_wave_mult = 0.10f;

			/// <summary>
			/// Final multiplier for the per-wave difficulty step. 
			/// </summary>
			[Save.Ignore] public float difficulty_mult = 1.00f;

			[Save.Ignore] public float difficulty_max = 500.00f;

			[Save.Ignore] public int wave_size_base = 2;
			[Save.Ignore] public int wave_size_max = 50;
			[Save.Ignore] public float wave_size_mult = 1.00f;

			[Save.Ignore] public float wave_interval = 60.00f;
			[Save.Ignore] public float wave_interval_difficulty_mult = 1.00f;

			[Save.Ignore] public uint max_npc_count = 32;

			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Active = 1 << 0,
				Paused = 1 << 1,

				Sound_Pending = 1 << 2
			}

			public enum Status: uint
			{
				Undefined = 0,

				Preparing,
				Running,
				Ended,

				Restarting,
			}

			[IGlobal.Data(false, Net.SendType.Reliable)]
			public partial struct State: IGlobal
			{
				[Save.Ignore] public IFaction.Handle faction_defenders = "defenders";
				[Save.Ignore] public IFaction.Handle faction_attackers = "attackers";

				[Save.Ignore] public ushort wave_current;
				[Save.Ignore] public byte target_count;
				[Save.Ignore] public byte player_count;

				[Save.Ignore] public IScenario.Handle scenario = "bouda.00";
				[Save.Ignore] public int? scenario_wave_index_current;

				/// <summary>
				/// Current match difficulty.
				/// </summary>
				[Save.Ignore] public float difficulty = 1.00f;

				[Save.Ignore] public Siege.Gamemode.Flags flags;
				[Save.Ignore] public Siege.Gamemode.Status status;

				[Save.Ignore] public float t_match_elapsed;
				[Save.Ignore] public float t_last_wave;
				[Save.Ignore] public float t_next_wave;
				[Save.Ignore, Net.Ignore] public float t_next_restart;
				[Save.Ignore, Net.Ignore] public float t_last_notification;

				public State()
				{

				}
			}

			public Gamemode()
			{

			}

			public static void Configure()
			{
				Constants.Materials.global_yield_modifier = 0.00f;
				Constants.Harvestable.global_yield_modifier = 0.00f;
				Constants.Block.global_yield_modifier = 0.00f;

				Constants.Organic.rotting_speed *= 10.00f;

				Constants.World.save_factions = false;
				Constants.World.save_players = true;
				Constants.World.save_characters = false;

				Constants.World.load_factions = false;
				Constants.World.load_players = true;
				Constants.World.load_characters = false;

				Constants.World.enable_autosave = false;

				Constants.Respawn.token_count_min = 10.00f;
				Constants.Respawn.token_count_max = 500.00f;
				Constants.Respawn.token_count_default = 30.00f;
				Constants.Respawn.token_refill_amount = 0.50f;

				Constants.Respawn.respawn_cooldown_base = 5.00f;
				Constants.Respawn.respawn_cooldown_token_modifier = 0.00f;
				Constants.Respawn.respawn_cost_base = 0.00f;

				Constants.Characters.allow_custom_characters = false;
				Constants.Characters.allow_switching = true;

				Constants.Equipment.enable_equip = true;
				Constants.Equipment.enable_unequip = false;

				Constants.Factions.enable_join_faction = false;
				Constants.Factions.enable_leave_faction = false;
				Constants.Factions.enable_found_faction = false;
				Constants.Factions.enable_disband_faction = false;
				Constants.Factions.enable_kick = false;
				Constants.Factions.enable_leadership = false;

				Constants.Questing.enable_quests = false;

				App.TryGetModInfo<SiegeMod>(out var mod_siege);

				IRecipe.Database.AddAssetFilter((string path, string identifier, ModInfo mod_info) =>
				{
					if (mod_info == mod_siege) return true;
					else if (identifier.StartsWith("gunsmith.", StringComparison.OrdinalIgnoreCase)) return true;
					else if (identifier.StartsWith("munitions.", StringComparison.OrdinalIgnoreCase)) return true;
					else if (identifier.StartsWith("forge.", StringComparison.OrdinalIgnoreCase)) return true;
					else if (identifier.StartsWith("manufactory.", StringComparison.OrdinalIgnoreCase)) return true;
					else return false;
				});

				IRecipe.Database.AddAssetPostProcessor((IRecipe.Definition definition, ref IRecipe.Data data) =>
				{
					//App.WriteLine($"{definition.Identifier}: {definition.mod_info?.Identifier}");

					if (definition.mod_info != mod_siege)
					{
						if (data.type == Crafting.Recipe.Type.Workshop)
						{
							data.flags.SetFlag(Crafting.Recipe.Flags.Hidden, true);
							data.type = Crafting.Recipe.Type.Buy;
							data.tags = Crafting.Recipe.Tags.Manufactory;
						}
					}
				});

				IOrigin.Database.AddAssetFilter((string path, string identifier, ModInfo mod_info) =>
				{
					if (mod_info == mod_siege) return true;
					else return false;
				});

				Augment.AddPostProcessor((ref IBlueprint.Data blueprint, ref Augment.Context context) =>
				{
					Span<Crafting.Requirement> requirements_tmp = stackalloc Crafting.Requirement[context.requirements_new.Length];
					context.requirements_new.CopyTo(requirements_tmp);
					context.requirements_new.Clear();

					var price = 0.00f;
					var complexity = 1.00f;

					foreach (ref var req in requirements_tmp)
					{
						switch (req.type)
						{
							case Crafting.Requirement.Type.Work:
							{
								price += MathF.Sqrt(req.amount * req.difficulty * 0.50f) * (req.difficulty * 0.10f);
								complexity += 0.05f;
							}
							break;

							case Crafting.Requirement.Type.Resource:
							{
								ref var material = ref req.material.GetData();
								if (!material.IsNull())
								{
									price += material.market_price * req.amount;
									complexity += 0.15f;
								}
							}
							break;
						}
					}

					var price_final = Money.ToBataPrice(price * complexity);
					if (price_final >= 0.00f)
					{
						context.requirements_new.Add(Crafting.Requirement.Money(price_final));

						ref var recipe_new = ref context.GetRecipeNew();

						if (recipe_new.tags.TrySetFlag(Crafting.Recipe.Tags.Gunsmith | Crafting.Recipe.Tags.Forge | Crafting.Recipe.Tags.Munitions, false))
						{
							recipe_new.tags.SetFlag(Crafting.Recipe.Tags.Manufactory, true);
						}

						recipe_new.type = Crafting.Recipe.Type.Buy;

						return true;
					}
					else
					{
						return false;
					}
				});

#if SERVER
				Player.OnCreate += OnPlayerCreate;
				static void OnPlayerCreate(ref Region.Data region, ref Player.Data player)
				{
					player.SetFaction("defenders");

					if (!player.GetControlledCharacter().IsValid())
					{
						var ent_character_soldier = Character.Create(ref region, "Soldier", prefab: "human.male", flags: Character.Flags.Human | Character.Flags.Military, origin: "imperial.soldier", gender: Organic.Gender.Male, player_id: player.id, hair_frame: 5, beard_frame: 1);
						var ent_character_sapper = Character.Create(ref region, "Sapper", prefab: "human.male", flags: Character.Flags.Human | Character.Flags.Engineering | Character.Flags.Military, origin: "imperial.sapper", gender: Organic.Gender.Male, player_id: player.id, hair_frame: 2, beard_frame: 7);
						var ent_character_medic = Character.Create(ref region, "Medic", prefab: "human.female", flags: Character.Flags.Human | Character.Flags.Medical | Character.Flags.Military, origin: "imperial.medic", gender: Organic.Gender.Female, player_id: player.id, hair_frame: 10);

						player.SetControlledCharacter(ent_character_soldier);
					}
				}
#endif

#if CLIENT
				Character.CreationGUI.enabled = false;
				//Character.CharacterHUD.enabled = false;

				Spawn.RespawnGUI.enabled = true;
				Spawn.RespawnGUI.window_offset = new Vector2(0, 132);
#endif
			}

			public static void Init()
			{
				App.WriteLine("Siege Init!", App.Color.Magenta);
			}
		}

		[ISystem.PreUpdate.Reset(ISystem.Mode.Single)]
		public static void UpdateSiegeReset(ISystem.Info info, [Source.Global] ref Siege.Gamemode.State g_siege_state)
		{
			g_siege_state.target_count = 0;
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSiegeTargets(ISystem.Info info, Entity entity, [Source.Global] ref Siege.Gamemode.State g_siege_state, [Source.Owned] ref Siege.Target.Data siege_target, [Source.Owned] in Faction.Data faction)
		{
			if (faction.id == g_siege_state.faction_defenders)
			{
				g_siege_state.target_count++;
			}
		}

		private struct WaveInfo
		{
			public int index;
			public int priority;
			public float weight;

			public WaveInfo(int index, int priority, float weight)
			{
				this.index = index;
				this.priority = priority;
				this.weight = weight;
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSiegeLate(ISystem.Info info, [Source.Global] ref Siege.Gamemode g_siege, [Source.Global] ref Siege.Gamemode.State g_siege_state)
		{
			//App.WriteLine(siege.target_count);

			ref var region = ref info.GetRegion();

#if SERVER
			var sync = false;
			var player_count = region.GetConnectedPlayerCount();

			sync |= g_siege_state.player_count.TrySet((byte)player_count);
			sync |= g_siege_state.flags.TrySetFlag(Siege.Gamemode.Flags.Active, player_count > 0 && !g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Paused));
#endif

			if (g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Active))
			{
				var time = g_siege_state.t_match_elapsed;

#if SERVER
				const float prep_time = 60.00f;
				switch (g_siege_state.status)
				{
					case Gamemode.Status.Undefined:
					{
						if (!Constants.World.disable_gameplay)
						{
							g_siege_state.status = Gamemode.Status.Preparing;
						}
					}
					break;

					case Gamemode.Status.Preparing:
					{
						if (time >= prep_time - 30.00f && g_siege_state.t_last_notification < prep_time - 30.00f)
						{
							g_siege_state.t_last_notification = time;
							Notification.Push(ref region, $"Match starting in 30 seconds!", Color32BGRA.Yellow, lifetime: 30.00f, "ui.alert.07", volume: 0.10f, pitch: 0.60f, send_type: Net.SendType.Reliable);
						}
						else if (time >= prep_time - 15.00f && g_siege_state.t_last_notification < prep_time - 15.00f)
						{
							g_siege_state.t_last_notification = time;
							Notification.Push(ref region, $"Match starting in 15 seconds!", Color32BGRA.Yellow, lifetime: 15.00f, "ui.alert.07", volume: 0.20f, pitch: 0.80f, send_type: Net.SendType.Reliable);
						}
						else if (time >= prep_time - 5.00f && g_siege_state.t_last_notification < prep_time - 5.00f)
						{
							g_siege_state.t_last_notification = time;
							Notification.Push(ref region, $"Match starting in 5 seconds!", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.07", volume: 0.30f, pitch: 1.00f, send_type: Net.SendType.Reliable);
						}
						else if (time >= prep_time)
						{
							g_siege_state.status = Gamemode.Status.Running;
						}
					}
					break;

					case Gamemode.Status.Running:
					{
						if (g_siege_state.target_count == 0)
						{
							g_siege_state.status = Gamemode.Status.Ended;
						}
						else
						{
							if (time >= g_siege_state.t_next_wave)
							{
								g_siege_state.wave_current++;
								g_siege_state.t_last_wave = time;

								var difficulty_step = g_siege.difficulty_step;
								difficulty_step *= 1.00f + (player_count * g_siege.difficulty_player_mult);
								difficulty_step *= 1.00f + (g_siege_state.wave_current * g_siege.difficulty_wave_mult);
								difficulty_step *= g_siege.difficulty_mult;
								difficulty_step = Maths.SnapCeil(difficulty_step, 0.25f);

								g_siege_state.difficulty = Maths.Clamp(g_siege_state.difficulty + difficulty_step, 1.00f, g_siege.difficulty_max);
								g_siege_state.scenario_wave_index_current = null;

								var duration = g_siege.wave_interval;
						
								ref var scenario = ref g_siege_state.scenario.GetData(out var scenario_asset);
								if (!scenario.IsNull())
								{
									//App.WriteLine($"{scenario_asset.identifier}");

									var priority_max = 0;

									Span<WaveInfo> waves_tmp = stackalloc WaveInfo[scenario.waves.Length];
									var waves_tmp_count = 0;

									for (int i = 0; i < scenario.waves.Length; i++)
									{
										ref var wave = ref scenario.waves[i];

										if (wave.type == IScenario.Wave.Type.Single)
										{
											if (wave.period == g_siege_state.wave_current)
											{
												//waves_tmp[0] = new(i, wave.priority, wave.weight);
												//waves_tmp_count = 1;
											}
										}
										else if (wave.type == IScenario.Wave.Type.Recurrent)
										{
											if (g_siege_state.wave_current % wave.period == 0)
											{
												//waves_span.AddPrioritized(new WaveInfo(i, wave.priority, wave.weight), ref waves_span_count);

												waves_tmp.Add(new WaveInfo(i, wave.priority, wave.weight), ref waves_tmp_count);
												priority_max = Math.Max(priority_max, wave.priority);
											}
										}
									}

									if (waves_tmp_count > 0)
									{
										var waves_filtered = waves_tmp.Slice(0, waves_tmp_count).ToArray();
										var wave_info_current = waves_filtered.Where(x => x.priority >= priority_max).FirstOrDefault();
										g_siege_state.scenario_wave_index_current = wave_info_current.index;

										App.WriteLine(wave_info_current.index);
									}

									//Span<WaveInfo> waves_filtered = stackalloc WaveInfo[waves_tmp_count];
									//var waves_filtered_count = 0;

									//for (int i = 0; i < waves_tmp_count; i++)
									//{
									//	ref var wave_info = ref waves_tmp[i];
									//	if (wave_info.priority >= priority_max)
									//	{
									//		waves_filtered.Add(wave_info, ref waves_filtered_count);
									//	}
									//}
								}

								if (!scenario.IsNull() && g_siege_state.scenario_wave_index_current.TryGetValue(out var scenario_wave_index))
								{
									ref var wave = ref scenario.waves[scenario_wave_index];
									duration = wave.duration;

									App.WriteLine($"Wave: [{wave.name}] {wave.sound}; {wave.sound_volume}; {wave.sound_pitch}");

									if (wave.sound.id != 0)
									{
										g_siege_state.flags.SetFlag(Siege.Gamemode.Flags.Sound_Pending, true);
										//Sound.PlayGUI(ref region, wave.sound, volume: wave.sound_volume, wave.sound_pitch);
									}
								}
								else
								{
									if (g_siege_state.wave_current % 5 == 0)
									{
										duration *= 3.00f;
									}
								}

								//g_siege_state.t_next_wave = time + Maths.Snap(g_siege.wave_interval + Maths.Clamp(g_siege_state.difficulty * 5.00f * g_siege.wave_interval_difficulty_mult, 0.00f, 300.00f), 15.00f);
								g_siege_state.t_next_wave = time + Maths.Snap(duration, 15.00f);

								sync |= true;

								//Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.02", volume: 0.60f, pitch: 0.75f);
								//Notification.Push(ref region, $"Wave #{g_siege_state.wave_current}!", Color32BGRA.Red, lifetime: 30.00f, "ui.alert.11", volume: 0.60f, pitch: 0.80f);
								Notification.Push(ref region, $"Wave #{g_siege_state.wave_current}!", Color32BGRA.Red, lifetime: 30.00f, "siren.00", volume: 0.20f, pitch: 1.00f, send_type: Net.SendType.Reliable);
							}
							else
							{
								ref var scenario = ref g_siege_state.scenario.GetData(out var scenario_asset);
								if (!scenario.IsNull() && g_siege_state.scenario_wave_index_current.TryGetValue(out var scenario_wave_index))
								{
									ref var wave = ref scenario.waves[scenario_wave_index];

									if (wave.sound.id != 0 && g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Sound_Pending) && (time - g_siege_state.t_last_wave) >= wave.sound_delay)
									{
										g_siege_state.flags.SetFlag(Siege.Gamemode.Flags.Sound_Pending, false);
										Sound.PlayGUI(ref region, wave.sound, volume: wave.sound_volume, wave.sound_pitch);
									}
								}
							}
						}
					}
					break;

					case Gamemode.Status.Ended:
					{
						if (g_siege_state.t_next_restart == 0.00f)
						{
							g_siege_state.t_last_notification = time;
							Notification.Push(ref region, $"Defeat! Restarting in 10 seconds...", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.09", volume: 0.40f, pitch: 1.00f, send_type: Net.SendType.Reliable);

							g_siege_state.t_next_restart = time + 10.00f;
						}

						if (time >= g_siege_state.t_next_restart)
						{
							ChangeMap(ref region, default);
							g_siege_state.status = Gamemode.Status.Restarting;
						}
					}
					break;

					case Gamemode.Status.Restarting:
					{

					}
					break;
				}
#endif

				g_siege_state.t_match_elapsed += info.DeltaTime;
			}

#if SERVER
			if (sync)
			{
				region.SyncGlobal(ref g_siege_state);
				App.WriteLine("Synced Siege");
			}
#endif
		}
	}
}
