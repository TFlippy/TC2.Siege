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

			[Save.Ignore, Net.Ignore] public float t_next_restart;
			[Save.Ignore, Net.Ignore] public float t_last_notification;

			[Save.Ignore] public Siege.Gamemode.Flags flags;
			[Save.Ignore] public Siege.Gamemode.Status status;

			[Flags]
			public enum Flags: uint
			{
				None = 0,
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

				SetupLoadouts();
			}

			private static void SetupLoadouts()
			{
				Spawn.kits = new Loadout.Kit[]
				{
					default,

#region Soldier			
					new("Armor (Soldier)", "", origin: "soldier", flags: Loadout.Kit.Flags.Required)
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Armor", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip),
								[1] = Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip),
							}
						}
					},

					new("Shield", "", origin: "soldier")
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Shield", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("shield")
							}
						}
					},

					new("Machete", "", origin: "soldier")
					{
						cost = 0.20f,

						shipment = new Shipment.Data("Machete", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("machete")
							}
						}
					},

					new("Revolver", "", origin: "soldier")
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Revolver", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("revolver"),
								[1] = Shipment.Item.Resource("ammo_hc.match", 30),
							}
						}
					},

					new("Rifle", "", origin: "soldier")
					{
						cost = 1.00f,

						shipment = new Shipment.Data("Rifle", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("rifle", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_hc.hv", 45)
							}
						}
					},

					new("SMG", "", origin: "soldier")
					{
						cost = 2.50f,

						shipment = new Shipment.Data("SMG", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("smg", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_lc", 300)
							}
						}
					},

					new("Battle Rifle", "", origin: "soldier")
					{
						cost = 2.50f,

						shipment = new Shipment.Data("Battle Rifle", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("battle_rifle", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_hc", 90)
							}
						}
					},

					new("ABR 740", "", origin: "soldier")
					{
						cost = 7.00f,

						shipment = new Shipment.Data("ABR 740", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.abr.740", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_hc.hv", 150)
							}
						}
					},

					//new("R750 Automat", "", origin: "soldier")
					//{
					//	cost = 15.00f,

					//	shipment = new Shipment.Data("R750 Automat", Shipment.Flags.Unpack)
					//	{
					//		items =
					//		{
					//			[0] = Shipment.Item.Prefab("bp.r750", flags: Shipment.Item.Flags.Pickup),
					//			[1] = Shipment.Item.Resource("ammo_hc.arc.mt", 200)
					//		}
					//	}
					//},

					new("Majzl A-749", "", origin: "soldier")
					{
						cost = 5.00f,

						shipment = new Shipment.Data("Majzl A-749", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.majzl.a749", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_mg", 80)
							}
						}
					},

					new("Auto-Shotgun", "", origin: "soldier")
					{
						cost = 6.50f,

						shipment = new Shipment.Data("Auto-Shotgun", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("auto_shotgun"),
								[1] = Shipment.Item.Resource("ammo_sg.buck", 32),
							}
						}
					},

					new("Grenade", "", origin: "soldier")
					{
						cost = 1.50f,

						shipment = new Shipment.Data("Grenade", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("grenade")
							}
						}
					},
#endregion

#region Engineer
					new("Armor (Engineer)", "", origin: "engineer", flags: Loadout.Kit.Flags.Required)
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Armor", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("helmet.03", flags: Shipment.Item.Flags.Equip),
								[1] = Shipment.Item.Prefab("armor.02", flags: Shipment.Item.Flags.Equip),
							}
						}
					},

					new("Crowbar", "", origin: "engineer")
					{
						cost = 0.30f,

						shipment = new Shipment.Data("Crowbar", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("crowbar")
							}
						}
					},

					new("Pickaxe", "", origin: "engineer")
					{
						cost = 0.40f,

						shipment = new Shipment.Data("Pickaxe", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("pickaxe")
							}
						}
					},

					new("Drill", "", origin: "engineer")
					{
						cost = 6.50f,

						shipment = new Shipment.Data("Drill", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("drill")
							}
						}
					},

					new("Pistol", "", origin: "engineer")
					{
						cost = 0.30f,

						shipment = new Shipment.Data("Pistol", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("pistol", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_lc", 60)
							}
						}
					},

					new("Pump Shotgun", "", origin: "engineer")
					{
						cost = 2.50f,

						shipment = new Shipment.Data("Pump Shotgun", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("pump_shotgun"),
								[1] = Shipment.Item.Resource("ammo_sg.buck", 32),
							}
						}
					},

					new("Scattergun (Grenades)", "", origin: "engineer")
					{
						cost = 5.00f,

						shipment = new Shipment.Data("Scattergun", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("scattergun"),
								[1] = Shipment.Item.Resource("ammo_sg.grenade", 32),
							}
						}
					},

					new("Bazooka", "", origin: "engineer")
					{
						cost = 10.00f,

						shipment = new Shipment.Data("Bazooka", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bazooka"),
								[1] = Shipment.Item.Resource("ammo_rocket", 4),
							}
						}
					},

					new("Tools", "", origin: "engineer")
					{
						cost = 0.70f,

						shipment = new Shipment.Data("Tools", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("wrench"),
								[1] = Shipment.Item.Prefab("hammer"),
								[2] = Shipment.Item.Resource("wood", 500.00f)
							}
						}
					},

					new("Mamut B-738", "", origin: "engineer")
					{
						cost = 12.00f,

						shipment = new Shipment.Data("Mamut B-738", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.mamut.b738", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_ac", 60)
							}
						}
					},

					new("Machine Gun Kit", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 50.00f,

						shipment = new Shipment.Data("Machine Gun")
						{
							items =
							{
								[0] = Shipment.Item.Prefab("machine_gun"),
								[1] = Shipment.Item.Resource("ammo_mg", 500),
								[2] = Shipment.Item.Prefab("mount.tripod")
							}
						}
					},

					new("Binoculars", "", origin: "engineer")
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Binoculars", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("binoculars")
							}
						}
					},

					new("Artillery Shells (Explosive)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 15.00f,

						shipment = new Shipment.Data("Shells")
						{
							items =
							{
								[0] = Shipment.Item.Resource("ammo_shell", 8)
							}
						}
					},

					new("Artillery Shells (Shrapnel)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 20.00f,

						shipment = new Shipment.Data("Shells")
						{
							items =
							{
								[0] = Shipment.Item.Resource("ammo_shell.shrapnel", 8)
							}
						}
					},

					new("Artillery Shells (HV)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 20.00f,

						shipment = new Shipment.Data("Shells")
						{
							items =
							{
								[0] = Shipment.Item.Resource("ammo_shell.hv", 8)
							}
						}
					},

					new("Artillery Shells (HE)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 20.00f,

						shipment = new Shipment.Data("Shells")
						{
							items =
							{
								[0] = Shipment.Item.Resource("ammo_shell.he", 8)
							}
						}
					},

					new("Land Mines", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 10.00f,

						shipment = new Shipment.Data("Land Mines")
						{
							items =
							{
								[0] = Shipment.Item.Prefab("landmine", 4),
							}
						}
					},

					new("Dynamite", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 10.00f,

						shipment = new Shipment.Data("Dynamite", Shipment.Flags.None)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("dynamite", 4)
							}
						}
					},

					new("Supplies (Ammo)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 30.00f,

						shipment = new Shipment.Data("Supplies (Ammo)")
						{
							items =
							{
								[0] = Shipment.Item.Resource("ammo_lc.hv", 400),
								[1] = Shipment.Item.Resource("ammo_hc.hv", 400),
								[2] = Shipment.Item.Resource("ammo_sg.buck", 140),
								[3] = Shipment.Item.Resource("ammo_mg", 600),
							}
						}
					},
#endregion

#region Medic
					new("Armor (Medic)", "", origin: "doctor", flags: Loadout.Kit.Flags.Required)
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Armor", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("helmet.04", flags: Shipment.Item.Flags.Equip),
								[1] = Shipment.Item.Prefab("armor.04", flags: Shipment.Item.Flags.Equip),
							}
						}
					},

					new("Shield", "", origin: "doctor")
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Shield", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("shield")
							}
						}
					},

					new("Knife", "", origin: "doctor")
					{
						cost = 0.20f,

						shipment = new Shipment.Data("Knife", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("knife")
							}
						}
					},

					new("Pistol", "", origin: "doctor")
					{
						cost = 0.30f,

						shipment = new Shipment.Data("Pistol", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("pistol"),
								[1] = Shipment.Item.Resource("ammo_lc", 40)
							}
						}
					},

					new("Carbine", "", origin: "doctor")
					{
						cost = 1.00f,

						shipment = new Shipment.Data("Carbine", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("carbine", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_hc.match", 40)
							}
						}
					},

					new("Machine Pistol", "", origin: "doctor")
					{
						cost = 2.50f,

						shipment = new Shipment.Data("Machine Pistol", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("machine_pistol", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_lc", 150),
							}
						}
					},

					new("Mamut B-738", "", origin: "doctor")
					{
						cost = 12.00f,

						shipment = new Shipment.Data("Mamut B-738", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.mamut.b738", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_ac", 60)
							}
						}
					},

					new("Houser A-750", "", origin: "doctor")
					{
						cost = 7.00f,

						shipment = new Shipment.Data("Houser A-750", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.houser.a750", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_hc.hv", 150)
							}
						}
					},

					new("Medkit", "", origin: "doctor")
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Medkit", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("medkit")
							}
						}
					},

					new("Arc Lance", "", origin: "doctor")
					{
						cost = 6.00f,

						shipment = new Shipment.Data("Arc Lance", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("arc_lance")
							}
						}
					},

					new("Grenade", "", origin: "doctor")
					{
						cost = 1.50f,

						shipment = new Shipment.Data("Grenade", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("grenade")
							}
						}
					},

					new("Morfitin-B", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 4.00f,

						shipment = new Shipment.Data("Morfitin-B", Shipment.Flags.None)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.morfitin.b", 4)
							}
						}
					},

					new("Paralyx", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 3.00f,

						shipment = new Shipment.Data("Paralyx", Shipment.Flags.None)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.paralyx", 4)
							}
						}
					},

					new("Codeine 20mg IR", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 2.00f,

						shipment = new Shipment.Data("Codeine 20mg IR", Shipment.Flags.None)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.codeine.20mg.ir", 4)
							}
						}
					},

					new("Pervitin 50mg ER", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 4.00f,

						shipment = new Shipment.Data("Pervitin 50mg ER", Shipment.Flags.None)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.pervitin.50mg.er", 4)
							}
						}
					},

					new("Pervitin 100mg ER", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
					{
						cost = 6.00f,

						shipment = new Shipment.Data("Pervitin 100mg ER", Shipment.Flags.None)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.pervitin.100mg.er", 4)
							}
						}
					},
#endregion
				};
			}
		}

#if SERVER
		[ISystem.PreUpdate.Reset(ISystem.Mode.Single)]
		public static void UpdateSiegeReset(ISystem.Info info, [Source.Global] ref Siege.Gamemode siege)
		{
			siege.target_count = 0;
		}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSiegeTargets(ISystem.Info info, Entity entity, [Source.Global] ref Siege.Gamemode siege, [Source.Owned] ref Siege.Target siege_target, [Source.Owned] in Faction.Data faction)
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
			if (region.GetConnectedPlayerCount() > 0)
			{
				var time = siege.match_time;

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

				siege.match_time += App.fixed_update_interval_s;
			}
		}
#endif

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Target: IComponent
		{
			public IFaction.Handle faction_id;

			[Save.Ignore, Net.Ignore] public float next_notification;
		}

#if SERVER
		[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single)]
		public static void OnPostDamage(ISystem.Info info, Entity entity, ref Health.PostDamageEvent data, [Source.Owned] ref Health.Data health, [Source.Owned] ref Siege.Target siege_target, [Source.Owned, Optional] in Faction.Data faction)
		{
			ref var region = ref info.GetRegion();

			if (data.damage.faction_id == 0 || data.damage.faction_id != faction.id)
			{
				if (info.WorldTime >= siege_target.next_notification)
				{
					siege_target.next_notification = info.WorldTime + 2.00f;

					Notification.Push(ref region, $"{entity.GetFullName()} is under attack! ({(health.integrity * 100.00f):0}% left)", Color32BGRA.Yellow, lifetime: 7.00f, "ui.alert.00", volume: 0.70f, pitch: 1.00f);
				}
			}
		}

		[ISystem.Remove(ISystem.Mode.Single)]
		public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Target siege_target)
		{
			ref var region = ref info.GetRegion();

			Notification.Push(ref region, $"{entity.GetFullName()} has been destroyed!", Color32BGRA.Red, lifetime: 10.00f, "ui.alert.02", volume: 0.80f, pitch: 0.80f);
		}
#endif

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Planner: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				//Ready = 1 << 0
			}

			public enum Status: uint
			{
				Undefined = 0,
				Waiting,
				Dispatching,
			}

			public int wave_size;
			public int wave_size_rem;
			public float wave_interval = 60.00f;

			public Siege.Planner.Flags flags;
			public Siege.Planner.Status status;

			[Save.Ignore, Net.Ignore] public float next_update;
			[Save.Ignore, Net.Ignore] public float next_dispatch;
			[Save.Ignore, Net.Ignore] public float next_wave;

			public Planner()
			{

			}
		}

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

		public static void SetKoboldLoadout(Entity ent_kobold)
		{
			var random = XorRandom.New();
			var loadout = new Loadout.Data();

			ref var shipment = ref loadout.shipments[0];
			shipment.flags.SetFlag(Shipment.Flags.Unpack, true);

			var items_span = shipment.items.AsSpan();

			// TODO: add proper .hjson loot tables
			switch (random.NextIntRange(0, 10))
			{
				// Melee
				case 0:
				case 1:
				{
					//if (random.NextBool(0.50f))
					//{
					//	items_span.Add(Shipment.Item.Prefab("club", flags: Shipment.Item.Flags.Pickup));
					//}
					//else if (random.NextBool(0.50f))
					//{
					//	items_span.Add(Shipment.Item.Prefab("axe", flags: Shipment.Item.Flags.Pickup));
					//}

					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("drill", flags: Shipment.Item.Flags.Pickup));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("shield", flags: Shipment.Item.Flags.Pickup));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("drill", flags: Shipment.Item.Flags.Pickup));
					}

					//if (random.NextBool(0.75f))
					{
						items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip));
					}
				}
				break;

				// Shotgunner
				case 2:
				case 3:
				{
					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("blunderbuss", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_musket.shot", 50));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("scattergun", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_sg.slug", 32));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("pump_shotgun", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_sg.buck", 32));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("auto_shotgun", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_sg.buck", 32));
					}

					if (random.NextBool(0.15f))
					{
						items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip));
					}

					if (random.NextBool(0.15f))
					{
						items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip));
					}
				}
				break;

				// Light
				case 4:
				case 5:
				case 6:
				case 7:
				{
					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("smg", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_lc", 200));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("carbine", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_hc", 50));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("rifle", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_hc", 50));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("machine_pistol", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_lc", 200));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("battle_rifle", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_hc", 80));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("revolver", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_lc", 50));
					}

					if (random.NextBool(0.20f))
					{
						items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip));
					}
				}
				break;

				// Heavy
				case 8:
				case 9:
				{
					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("auto_shotgun", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_sg.slug", 64));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("battle_rifle", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_hc.hv", 80));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("auto_shotgun", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_sg.grenade", 64));
					}

					if (random.NextBool(1.00f))
					{
						items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip));
						items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip));
					}
				}
				break;

				// Artillery
				default:
				{
					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("bazooka", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_rocket", 16));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("scattergun", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_sg.grenade", 32));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("pump_shotgun", flags: Shipment.Item.Flags.Pickup));
						items_span.Add(Shipment.Item.Resource("ammo_sg.grenade", 32));
					}

					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip));
						items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip));
					}
				}
				break;
			}

			ref var loadout_new = ref ent_kobold.GetOrAddComponent<Loadout.Data>(sync: false, ignore_mask: true);
			if (!loadout_new.IsNull())
			{
				loadout_new = loadout;
			}
		}

		[ISystem.Event<Spawner.SpawnEvent>(ISystem.Mode.Single)]
		public static void OnSpawn(ISystem.Info info, Entity entity, ref Spawner.SpawnEvent data,
		[Source.Owned] ref Spawner.Data spawner, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Siege.Planner planner, [Source.Owned] ref Selection.Data selection)
		{
			//App.WriteLine($"spawn event {data.ent_target}");

			SetKoboldLoadout(data.ent_target);

			ref var ai = ref data.ent_target.GetComponent<AI.Data>();
			if (!ai.IsNull())
			{
				ai.stance = AI.Stance.Aggressive;
			}

			ref var marker = ref data.ent_target.GetOrAddComponent<Minimap.Marker.Data>(sync: true);
			if (!marker.IsNull())
			{
				marker.sprite = new Sprite("ui_icons_minimap", 16, 16, 0, 0);
			}
		}

#if SERVER
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

		[Query]
		public delegate void GetAllTargetsQuery(ISystem.Info info, Entity entity, [Source.Owned] in Siege.Target target, [Source.Owned] in Transform.Data transform);

		[Query]
		public delegate void GetAllUnitsQuery(ISystem.Info info, Entity entity, [Source.Owned] in Commandable.Data commandable, [Source.Owned, Override] in AI.Movement movement, [Source.Owned, Override] in AI.Behavior behavior, [Source.Owned] in Transform.Data transform, [Source.Owned] in Faction.Data faction);

		private struct GetAllTargetsQueryArgs
		{
			public Entity ent_search;
			public IFaction.Handle faction_id;
			public Vector2 position;
			public Entity ent_root;
			public Entity ent_target;
			public float target_dist_nearest_sq;
			public Vector2 target_position;

			public GetAllTargetsQueryArgs(Entity ent_search, IAsset2<IFaction, IFaction.Data>.Handle faction_id, Vector2 position, Entity ent_root, Entity ent_target, float target_dist_nearest_sq, Vector2 target_position)
			{
				this.ent_search = ent_search;
				this.faction_id = faction_id;
				this.position = position;
				this.ent_root = ent_root;
				this.ent_target = ent_target;
				this.target_dist_nearest_sq = target_dist_nearest_sq;
				this.target_position = target_position;
			}
		}

		private struct GetAllUnitsQueryArgs
		{
			public Entity ent_search;
			public Entity ent_target;
			public IFaction.Handle faction_id;
			public Vector2 position;
			public Vector2 target_position;
			public int selection_count;
			public int wave_size_rem;
			public FixedArray4<EntRef<Commandable.Data>> selection;

			public GetAllUnitsQueryArgs(Entity ent_search, Entity ent_target, IAsset2<IFaction, IFaction.Data>.Handle faction_id, Vector2 position, Vector2 target_position, int selection_count, int wave_size_rem, FixedArray4<EntRef<Commandable.Data>> selection)
			{
				this.ent_search = ent_search;
				this.ent_target = ent_target;
				this.faction_id = faction_id;
				this.position = position;
				this.target_position = target_position;
				this.selection_count = selection_count;
				this.wave_size_rem = wave_size_rem;
				this.selection = selection;
			}
		}

		public static bool TryFindTarget(ref Region.Data region, Entity ent_planner, IFaction.Handle faction, Vector2 position_src, out Entity ent_target, out Vector2 position_target)
		{
			var arg = new GetAllTargetsQueryArgs(ent_planner, faction.id, position_src, default, default, float.MaxValue, default);

			region.Query<Siege.GetAllTargetsQuery>(Func).Execute(ref arg);
			static void Func(ISystem.Info info, Entity entity, in Siege.Target target, in Transform.Data transform)
			{
				ref var arg = ref info.GetParameter<GetAllTargetsQueryArgs>();
				if (!arg.IsNull())
				{
					ref var region = ref info.GetRegion();

					var dist_sq = Vector2.DistanceSquared(transform.position, arg.position);
					if ((target.faction_id == 0 || target.faction_id != arg.faction_id) && dist_sq < arg.target_dist_nearest_sq)
					{
						if (arg.ent_root.id == 0)
						{
							arg.ent_root = arg.ent_search.GetRoot(Relation.Type.Child);
						}

						var ent_root = entity.GetRoot(Relation.Type.Child);
						if (ent_root != arg.ent_root && ent_root.GetRoot(Relation.Type.Instance) != arg.ent_root)
						{
							arg.ent_target = entity;
							arg.target_dist_nearest_sq = dist_sq;
							arg.target_position = transform.position;
						}
					}
				}
			}

			ent_target = arg.ent_target;
			position_target = arg.target_position;

			return arg.ent_target.IsAlive();
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Spawner.Data spawner,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Selection.Data selection, [Source.Owned] ref Siege.Planner planner, [Source.Global] ref Siege.Gamemode siege, [Source.Owned, Optional] in Faction.Data faction)
		{
			ref var region = ref info.GetRegion();

			if (Constants.World.enable_npc_spawning && Constants.World.enable_ai && region.GetConnectedPlayerCount() > 0)
			{
				var time = siege.match_time;
				if (time >= planner.next_update)
				{
					planner.next_update = time + 1.00f;

					var random = XorRandom.New();

					var difficulty = (time / 60.00f);

					switch (planner.status)
					{
						case Planner.Status.Undefined:
						{
							planner.next_wave = time + 60.00f;
							planner.status = Planner.Status.Waiting;
						}
						break;

						case Planner.Status.Waiting:
						{
							if (time >= planner.next_wave)
							{
								planner.next_wave = time + planner.wave_interval + Maths.Clamp(difficulty * 10.00f, 0.00f, 120.00f);
								planner.wave_size = (int)Maths.Clamp(3 + MathF.Floor(MathF.Pow(difficulty, 0.80f)) * 2.00f, 0, 40);
								planner.wave_size_rem = planner.wave_size;

								planner.status = Planner.Status.Dispatching;

								//Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.02", volume: 0.60f, pitch: 0.75f);
								Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Red, lifetime: 10.00f, "ui.alert.11", volume: 0.60f, pitch: 0.80f);

							}
						}
						break;

						case Siege.Planner.Status.Dispatching:
						{
							if ((planner.next_wave - time) >= 30.00f)
							{
								if (time >= planner.next_dispatch)
								{
									planner.next_dispatch = time + random.NextFloatRange(5.00f, 10.00f);

									if (TryFindTarget(ref region, entity, faction.id, transform.position, out var ent_target, out var target_position))
									{
										var arg = new GetAllUnitsQueryArgs(entity, ent_target, faction.id, transform.position, target_position, 0, planner.wave_size_rem, default);

										region.Query<Siege.GetAllUnitsQuery>(Func2).Execute(ref arg);
										static void Func2(ISystem.Info info, Entity entity, [Source.Owned] in Commandable.Data commandable, [Source.Owned, Override] in AI.Movement movement, [Source.Owned, Override] in AI.Behavior behavior, [Source.Owned] in Transform.Data transform, [Source.Owned] in Faction.Data faction)
										{
											ref var arg = ref info.GetParameter<GetAllUnitsQueryArgs>();
											if (!arg.IsNull() && arg.selection_count < arg.selection.Length)
											{
												//App.WriteLine(behavior.idle_timer);
												if (faction.id == arg.faction_id && (behavior.idle_timer >= 2.00f || behavior.type == AI.Behavior.Type.None || movement.type == AI.Movement.Type.None))
												{
													if (Vector2.DistanceSquared(transform.position, arg.position) <= (32 * 32))
													{
														if (arg.wave_size_rem > 0)
														{
															arg.wave_size_rem--;
														}
														else
														{
															return;
														}
													}

													//ref var region = ref info.GetRegion();
													arg.selection[arg.selection_count++].Set(entity);
													//App.WriteLine(entity);
												}
											}
										}

										planner.wave_size_rem = arg.wave_size_rem;

										if (arg.selection_count > 0)
										{
											selection.units = arg.selection;

											selection.order_type = Commandable.OrderType.Attack;

											control.mouse.position = target_position;
											control.mouse.SetKeyPressed(Mouse.Key.Right, true);
										}

										if (planner.wave_size_rem > 0)
										{
											planner.next_dispatch = time + random.NextFloatRange(1.00f, 3.00f);
											spawner.next_spawn = time;

											App.WriteLine($"Spawning reinforcements... ({planner.wave_size_rem} left)");
										}
									}
								}
							}
							else
							{
								planner.status = Planner.Status.Waiting;
							}
						}
						break;
					}
				}
			}
		}
#endif

#if CLIENT
		public partial struct SiegeDefenderGUI: IGUICommand
		{
			public Entity ent_siege;
			public Siege.Gamemode siege;

			public void Draw()
			{
				var window_pos = (GUI.CanvasSize * new Vector2(0.50f, 0.00f)) + new Vector2(100, 48);
				using (var window = GUI.Window.Standalone("Siege", size: new Vector2(400, 300), pivot: new Vector2(0.50f, 0.00f), padding: new(4)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						GUI.DrawWindowBackground();

						ref var region = ref Client.GetRegion();
						ref var world = ref Client.GetWorld();
						ref var game_info = ref Client.GetGameInfo();

						//GUI.Title("Siege");

						GUI.DrawButton("Test", new(120, 40));
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUIDefender(Entity entity, [Source.Owned] in Player.Data player, [Source.Global] in Siege.Gamemode siege)
		{
			if (player.IsLocal() && player.faction_id == siege.faction_defenders)
			{
				var gui = new SiegeDefenderGUI()
				{
					siege = siege
				};
				//gui.Submit();
			}
		}
#endif

#if CLIENT
		public partial struct SiegeAttackerGUI: IGUICommand
		{
			public Entity ent_siege;
			public Siege.Gamemode siege;

			public void Draw()
			{
				var window_pos = (GUI.CanvasSize * new Vector2(0.50f, 0.00f)) + new Vector2(100, 48);
				using (var window = GUI.Window.Standalone("Siege2", position: window_pos, size: new Vector2(700, 400), pivot: new Vector2(0.50f, 0.00f)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						ref var region = ref Client.GetRegion();
						ref var world = ref Client.GetWorld();
						ref var game_info = ref Client.GetGameInfo();

						GUI.Title($"{this.siege.faction_defenders.id}");
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUIAttacker(Entity entity, [Source.Owned] in Player.Data player, [Source.Global] in Siege.Gamemode siege)
		{
			if (player.IsLocal() && player.faction_id == siege.faction_attackers)
			{
				var gui = new SiegeAttackerGUI()
				{
					siege = siege
				};
				gui.Submit();
			}
		}
#endif
	}
}
