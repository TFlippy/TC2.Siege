using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		[IGamemode.Data("Siege", "")]
		public partial struct Gamemode: IGamemode
		{
			[Save.Ignore] public IFaction.Handle faction_defenders = "defenders";
			[Save.Ignore] public IFaction.Handle faction_attackers = "attackers";

			[Save.Ignore] public uint target_count;
			[Save.Ignore] public float match_time;

			/// <summary>
			/// Loot sharing ratio based on player count.
			/// <code>
			/// Maths.Lerp(1.00f, 1.00f / player_count, loot_share_ratio);
			/// </code>
			/// </summary>
			[Save.Ignore] public float loot_share_ratio = 0.50f;

			/// <summary>
			/// Current match difficulty.
			/// </summary>
			[Save.Ignore] public float difficulty = 1.00f;

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
			[Save.Ignore] public float difficulty_player_mult = 0.10f;

			[Save.Ignore] public float difficulty_wave_mult = 0.10f;

			/// <summary>
			/// Final multiplier for the per-wave difficulty step. 
			/// </summary>
			[Save.Ignore] public float difficulty_mult = 1.00f;

			[Save.Ignore] public float difficulty_max = 100.00f;

			[Save.Ignore] public float wave_interval = 60.00f;
			[Save.Ignore] public float wave_interval_difficulty_mult = 1.00f;
			[Save.Ignore] public int wave_current;

			[Save.Ignore] public float t_next_wave;
			[Save.Ignore, Net.Ignore] public float t_next_restart;
			[Save.Ignore, Net.Ignore] public float t_last_notification;

			[Save.Ignore] public Siege.Gamemode.Flags flags;
			[Save.Ignore] public Siege.Gamemode.Status status;

			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Active = 1 << 0
			}

			public enum Status: uint
			{
				Undefined = 0,

				Preparing,
				Running,
				Ended,

				Restarting,
			}

			public Gamemode()
			{

			}

			public static void Configure()
			{
				App.TryGetModInfo<SiegeMod>(out var mod_siege);

				Shop.AddAssetFilter((string path, string identifier, ModInfo mod_info) =>
				{
					if (mod_info == mod_siege) return true;
					else if (identifier.StartsWith("gunsmith.", StringComparison.OrdinalIgnoreCase)) return true;
					else if (identifier.StartsWith("munitions.", StringComparison.OrdinalIgnoreCase)) return true;
					else return false;
				});

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

#if SERVER
				Player.OnCreate += OnPlayerCreate;
				static void OnPlayerCreate(ref Region.Data region, ref Player.Data player)
				{
					player.SetFaction("defenders");

					if (!player.GetControlledCharacter().IsValid())
					{
						var ent_character_soldier = Character.Create(ref region, "Soldier", prefab: "human.male", flags: Character.Flags.Human | Character.Flags.Military, origin: "soldier", gender: Organic.Gender.Male, player_id: player.id, hair_frame: 5, beard_frame: 1);
						var ent_character_engineer = Character.Create(ref region, "Engineer", prefab: "human.male", flags: Character.Flags.Human | Character.Flags.Engineering | Character.Flags.Military, origin: "engineer", gender: Organic.Gender.Male, player_id: player.id, hair_frame: 2, beard_frame: 7);
						var ent_character_medic = Character.Create(ref region, "Medic", prefab: "human.female", flags: Character.Flags.Human | Character.Flags.Medical | Character.Flags.Military, origin: "doctor", gender: Organic.Gender.Female, player_id: player.id, hair_frame: 10);

						player.SetControlledCharacter(ent_character_soldier);
					}
				}
#endif

#if CLIENT
				Character.CreationGUI.enabled = false;
				//Character.CharacterHUD.enabled = false;

				Spawn.RespawnGUI.enabled = true;
#endif
			}

			public static void Init()
			{
				App.WriteLine("Siege Init!", App.Color.Magenta);
			}
		}

		[IGlobal.Data(false, Net.SendType.Reliable)]
		public partial struct State: IGlobal
		{
			[Save.Ignore] public IFaction.Handle faction_defenders = "defenders";
			[Save.Ignore] public IFaction.Handle faction_attackers = "attackers";

			[Save.Ignore] public uint target_count;
			[Save.Ignore] public float match_time;

			/// <summary>
			/// Current match difficulty.
			/// </summary>
			[Save.Ignore] public float difficulty = 1.00f;
			[Save.Ignore] public int wave_current;

			[Save.Ignore] public float t_next_wave;
			[Save.Ignore, Net.Ignore] public float t_next_restart;
			[Save.Ignore, Net.Ignore] public float t_last_notification;

			[Save.Ignore] public Siege.Gamemode.Flags flags;
			[Save.Ignore] public Siege.Gamemode.Status status;

			public State()
			{

			}
		}

			//public static float GetDifficulty(ref this Siege.Gamemode siege, ref Region.Data region)
			//{
			//	var difficulty = siege.difficulty;
			//	difficulty *= 1.00f + ((region.GetConnectedPlayerCount() - 1) * 0.10f * siege.difficulty_player_mult);

			//	return difficulty;
			//}

			[ISystem.PreUpdate.Reset(ISystem.Mode.Single)]
		public static void UpdateSiegeReset(ISystem.Info info, [Source.Global] ref Siege.Gamemode siege)
		{
			siege.target_count = 0;
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSiegeTargets(ISystem.Info info, Entity entity, [Source.Global] ref Siege.Gamemode siege, [Source.Owned] ref Siege.Target.Data siege_target, [Source.Owned] in Faction.Data faction)
		{
			if (faction.id == siege.faction_defenders)
			{
				siege.target_count++;
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSiegeLate(ISystem.Info info, [Source.Global] ref Siege.Gamemode siege)
		{
			//App.WriteLine(siege.target_count);

			ref var region = ref info.GetRegion();

#if SERVER
			var sync = false;
			var player_count = region.GetConnectedPlayerCount();
			sync |= siege.flags.TrySetFlag(Siege.Gamemode.Flags.Active, player_count > 0);
#endif

			if (siege.flags.HasAny(Siege.Gamemode.Flags.Active))
			{
				var time = siege.match_time;

#if SERVER
				const float prep_time = 60.00f;
				switch (siege.status)
				{
					case Gamemode.Status.Undefined:
					{
						if (!Constants.World.disable_gameplay)
						{
							siege.status = Gamemode.Status.Preparing;
						}
					}
					break;

					case Gamemode.Status.Preparing:
					{
						if (time >= prep_time - 30.00f && siege.t_last_notification < prep_time - 30.00f)
						{
							siege.t_last_notification = time;
							Notification.Push(ref region, $"Match starting in 30 seconds!", Color32BGRA.Yellow, lifetime: 30.00f, "ui.alert.07", volume: 0.10f, pitch: 0.60f);
						}
						else if (time >= prep_time - 15.00f && siege.t_last_notification < prep_time - 15.00f)
						{
							siege.t_last_notification = time;
							Notification.Push(ref region, $"Match starting in 15 seconds!", Color32BGRA.Yellow, lifetime: 15.00f, "ui.alert.07", volume: 0.20f, pitch: 0.80f);
						}
						else if (time >= prep_time - 5.00f && siege.t_last_notification < prep_time - 5.00f)
						{
							siege.t_last_notification = time;
							Notification.Push(ref region, $"Match starting in 5 seconds!", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.07", volume: 0.30f, pitch: 1.00f);
						}
						else if (time >= prep_time)
						{
							siege.status = Gamemode.Status.Running;
						}
					}
					break;

					case Gamemode.Status.Running:
					{
						if (siege.target_count == 0)
						{
							siege.status = Gamemode.Status.Ended;
						}
						else
						{
							if (time >= siege.t_next_wave)
							{
								siege.wave_current++;

								var difficulty_step = siege.difficulty_step;
								difficulty_step *= 1.00f + (player_count * siege.difficulty_player_mult);
								difficulty_step *= 1.00f + (siege.wave_current * siege.difficulty_wave_mult);
								difficulty_step *= siege.difficulty_mult;
								difficulty_step = Maths.SnapCeil(difficulty_step, 0.25f);

								siege.difficulty = Maths.Clamp(siege.difficulty + difficulty_step, 1.00f, siege.difficulty_max);

								siege.t_next_wave = time + Maths.Snap(siege.wave_interval + Maths.Clamp(siege.difficulty * 5.00f * siege.wave_interval_difficulty_mult, 0.00f, 300.00f), 15.00f);

								sync |= true;

								//Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.02", volume: 0.60f, pitch: 0.75f);
								Notification.Push(ref region, $"Wave #{siege.wave_current}!", Color32BGRA.Red, lifetime: 30.00f, "ui.alert.11", volume: 0.60f, pitch: 0.80f);
							}
						}
					}
					break;

					case Gamemode.Status.Ended:
					{
						if (siege.t_next_restart == 0.00f)
						{
							siege.t_last_notification = time;
							Notification.Push(ref region, $"Defeat! Restarting in 10 seconds...", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.09", volume: 0.40f, pitch: 1.00f);

							siege.t_next_restart = time + 10.00f;
						}

						if (time >= siege.t_next_restart)
						{
							ChangeMap(ref region, default);
							siege.status = Gamemode.Status.Restarting;
						}
					}
					break;

					case Gamemode.Status.Restarting:
					{

					}
					break;
				}
#endif

				siege.match_time += info.DeltaTime;
			}

#if SERVER
			if (sync)
			{
				region.SyncGlobal(ref siege);
				App.WriteLine("Synced Siege");
			}
#endif
		}
	}
}
