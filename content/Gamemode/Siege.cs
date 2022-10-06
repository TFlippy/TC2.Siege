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

				//SetupLoadouts();
			}

//			private static void SetupLoadouts()
//			{
//				Spawn.kits = new Loadout.Kit[]
//				{
//					default,

//#region Soldier			
//					new("Armor (Soldier)", "", origin: "soldier", flags: Loadout.Kit.Flags.Required)
//					{
//						cost = 0.50f,

//						shipment = new Shipment.Data("Armor", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip),
//								[1] = Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip),
//							}
//						}
//					},

//					new("Shield", "", origin: "soldier")
//					{
//						cost = 0.50f,

//						shipment = new Shipment.Data("Shield", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("shield")
//							}
//						}
//					},

//					new("Machete", "", origin: "soldier")
//					{
//						cost = 0.20f,

//						shipment = new Shipment.Data("Machete", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("machete")
//							}
//						}
//					},

//					new("Revolver", "", origin: "soldier")
//					{
//						cost = 0.50f,

//						shipment = new Shipment.Data("Revolver", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("revolver"),
//								[1] = Shipment.Item.Resource("ammo_hc.match", 30),
//							}
//						}
//					},

//					new("Rifle", "", origin: "soldier")
//					{
//						cost = 1.00f,

//						shipment = new Shipment.Data("Rifle", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("rifle", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_hc.hv", 45)
//							}
//						}
//					},

//					new("SMG", "", origin: "soldier")
//					{
//						cost = 2.50f,

//						shipment = new Shipment.Data("SMG", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("smg", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_lc", 300)
//							}
//						}
//					},

//					new("Battle Rifle", "", origin: "soldier")
//					{
//						cost = 2.50f,

//						shipment = new Shipment.Data("Battle Rifle", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("battle_rifle", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_hc", 90)
//							}
//						}
//					},

//					new("ABR 740", "", origin: "soldier")
//					{
//						cost = 7.00f,

//						shipment = new Shipment.Data("ABR 740", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.abr.740", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_hc.hv", 150)
//							}
//						}
//					},

//					//new("R750 Automat", "", origin: "soldier")
//					//{
//					//	cost = 15.00f,

//					//	shipment = new Shipment.Data("R750 Automat", Shipment.Flags.Unpack)
//					//	{
//					//		items =
//					//		{
//					//			[0] = Shipment.Item.Prefab("bp.r750", flags: Shipment.Item.Flags.Pickup),
//					//			[1] = Shipment.Item.Resource("ammo_hc.arc.mt", 200)
//					//		}
//					//	}
//					//},

//					new("Majzl A-749", "", origin: "soldier")
//					{
//						cost = 5.00f,

//						shipment = new Shipment.Data("Majzl A-749", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.majzl.a749", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_mg", 80)
//							}
//						}
//					},

//					new("Auto-Shotgun", "", origin: "soldier")
//					{
//						cost = 6.50f,

//						shipment = new Shipment.Data("Auto-Shotgun", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("auto_shotgun"),
//								[1] = Shipment.Item.Resource("ammo_sg.buck", 32),
//							}
//						}
//					},

//					new("Grenade", "", origin: "soldier")
//					{
//						cost = 1.50f,

//						shipment = new Shipment.Data("Grenade", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("grenade")
//							}
//						}
//					},
//#endregion

//#region Engineer
//					new("Armor (Engineer)", "", origin: "engineer", flags: Loadout.Kit.Flags.Required)
//					{
//						cost = 0.50f,

//						shipment = new Shipment.Data("Armor", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("helmet.03", flags: Shipment.Item.Flags.Equip),
//								[1] = Shipment.Item.Prefab("armor.02", flags: Shipment.Item.Flags.Equip),
//							}
//						}
//					},

//					new("Crowbar", "", origin: "engineer")
//					{
//						cost = 0.30f,

//						shipment = new Shipment.Data("Crowbar", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("crowbar")
//							}
//						}
//					},

//					new("Pickaxe", "", origin: "engineer")
//					{
//						cost = 0.40f,

//						shipment = new Shipment.Data("Pickaxe", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("pickaxe")
//							}
//						}
//					},

//					new("Drill", "", origin: "engineer")
//					{
//						cost = 6.50f,

//						shipment = new Shipment.Data("Drill", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("drill")
//							}
//						}
//					},

//					new("Pistol", "", origin: "engineer")
//					{
//						cost = 0.30f,

//						shipment = new Shipment.Data("Pistol", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("pistol", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_lc", 60)
//							}
//						}
//					},

//					new("Pump Shotgun", "", origin: "engineer")
//					{
//						cost = 2.50f,

//						shipment = new Shipment.Data("Pump Shotgun", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("pump_shotgun"),
//								[1] = Shipment.Item.Resource("ammo_sg.buck", 32),
//							}
//						}
//					},

//					new("Scattergun (Grenades)", "", origin: "engineer")
//					{
//						cost = 5.00f,

//						shipment = new Shipment.Data("Scattergun", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("scattergun"),
//								[1] = Shipment.Item.Resource("ammo_sg.grenade", 32),
//							}
//						}
//					},

//					new("Bazooka", "", origin: "engineer")
//					{
//						cost = 10.00f,

//						shipment = new Shipment.Data("Bazooka", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bazooka"),
//								[1] = Shipment.Item.Resource("ammo_rocket", 4),
//							}
//						}
//					},

//					new("Tools", "", origin: "engineer")
//					{
//						cost = 0.70f,

//						shipment = new Shipment.Data("Tools", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("wrench"),
//								[1] = Shipment.Item.Prefab("hammer"),
//								[2] = Shipment.Item.Resource("wood", 500.00f)
//							}
//						}
//					},

//					new("Mamut B-738", "", origin: "engineer")
//					{
//						cost = 12.00f,

//						shipment = new Shipment.Data("Mamut B-738", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.mamut.b738", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_ac", 60)
//							}
//						}
//					},

//					new("Machine Gun Kit", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 50.00f,

//						shipment = new Shipment.Data("Machine Gun")
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("machine_gun"),
//								[1] = Shipment.Item.Resource("ammo_mg", 500),
//								[2] = Shipment.Item.Prefab("mount.tripod")
//							}
//						}
//					},

//					new("Binoculars", "", origin: "engineer")
//					{
//						cost = 0.50f,

//						shipment = new Shipment.Data("Binoculars", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("binoculars")
//							}
//						}
//					},

//					new("Artillery Shells (Explosive)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 15.00f,

//						shipment = new Shipment.Data("Shells")
//						{
//							items =
//							{
//								[0] = Shipment.Item.Resource("ammo_shell", 8)
//							}
//						}
//					},

//					new("Artillery Shells (Shrapnel)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 20.00f,

//						shipment = new Shipment.Data("Shells")
//						{
//							items =
//							{
//								[0] = Shipment.Item.Resource("ammo_shell.shrapnel", 8)
//							}
//						}
//					},

//					new("Artillery Shells (HV)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 20.00f,

//						shipment = new Shipment.Data("Shells")
//						{
//							items =
//							{
//								[0] = Shipment.Item.Resource("ammo_shell.hv", 8)
//							}
//						}
//					},

//					new("Artillery Shells (HE)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 20.00f,

//						shipment = new Shipment.Data("Shells")
//						{
//							items =
//							{
//								[0] = Shipment.Item.Resource("ammo_shell.he", 8)
//							}
//						}
//					},

//					new("Land Mines", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 10.00f,

//						shipment = new Shipment.Data("Land Mines")
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("landmine", 4),
//							}
//						}
//					},

//					new("Dynamite", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 10.00f,

//						shipment = new Shipment.Data("Dynamite", Shipment.Flags.None)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("dynamite", 4)
//							}
//						}
//					},

//					new("Supplies (Ammo)", "", origin: "engineer", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 30.00f,

//						shipment = new Shipment.Data("Supplies (Ammo)")
//						{
//							items =
//							{
//								[0] = Shipment.Item.Resource("ammo_lc.hv", 400),
//								[1] = Shipment.Item.Resource("ammo_hc.hv", 400),
//								[2] = Shipment.Item.Resource("ammo_sg.buck", 140),
//								[3] = Shipment.Item.Resource("ammo_mg", 600),
//							}
//						}
//					},
//#endregion

//#region Medic
//					new("Armor (Medic)", "", origin: "doctor", flags: Loadout.Kit.Flags.Required)
//					{
//						cost = 0.50f,

//						shipment = new Shipment.Data("Armor", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("helmet.04", flags: Shipment.Item.Flags.Equip),
//								[1] = Shipment.Item.Prefab("armor.04", flags: Shipment.Item.Flags.Equip),
//							}
//						}
//					},

//					new("Shield", "", origin: "doctor")
//					{
//						cost = 0.50f,

//						shipment = new Shipment.Data("Shield", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("shield")
//							}
//						}
//					},

//					new("Knife", "", origin: "doctor")
//					{
//						cost = 0.20f,

//						shipment = new Shipment.Data("Knife", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("knife")
//							}
//						}
//					},

//					new("Pistol", "", origin: "doctor")
//					{
//						cost = 0.30f,

//						shipment = new Shipment.Data("Pistol", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("pistol"),
//								[1] = Shipment.Item.Resource("ammo_lc", 40)
//							}
//						}
//					},

//					new("Carbine", "", origin: "doctor")
//					{
//						cost = 1.00f,

//						shipment = new Shipment.Data("Carbine", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("carbine", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_hc.match", 40)
//							}
//						}
//					},

//					new("Machine Pistol", "", origin: "doctor")
//					{
//						cost = 2.50f,

//						shipment = new Shipment.Data("Machine Pistol", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("machine_pistol", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_lc", 150),
//							}
//						}
//					},

//					new("Mamut B-738", "", origin: "doctor")
//					{
//						cost = 12.00f,

//						shipment = new Shipment.Data("Mamut B-738", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.mamut.b738", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_ac", 60)
//							}
//						}
//					},

//					new("Houser A-750", "", origin: "doctor")
//					{
//						cost = 7.00f,

//						shipment = new Shipment.Data("Houser A-750", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.houser.a750", flags: Shipment.Item.Flags.Pickup),
//								[1] = Shipment.Item.Resource("ammo_hc.hv", 150)
//							}
//						}
//					},

//					new("Medkit", "", origin: "doctor")
//					{
//						cost = 0.50f,

//						shipment = new Shipment.Data("Medkit", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("medkit")
//							}
//						}
//					},

//					new("Arc Lance", "", origin: "doctor")
//					{
//						cost = 6.00f,

//						shipment = new Shipment.Data("Arc Lance", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("arc_lance")
//							}
//						}
//					},

//					new("Grenade", "", origin: "doctor")
//					{
//						cost = 1.50f,

//						shipment = new Shipment.Data("Grenade", Shipment.Flags.Unpack)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("grenade")
//							}
//						}
//					},

//					new("Morfitin-B", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 4.00f,

//						shipment = new Shipment.Data("Morfitin-B", Shipment.Flags.None)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.morfitin.b", 4)
//							}
//						}
//					},

//					new("Paralyx", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 3.00f,

//						shipment = new Shipment.Data("Paralyx", Shipment.Flags.None)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.paralyx", 4)
//							}
//						}
//					},

//					new("Codeine 20mg IR", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 2.00f,

//						shipment = new Shipment.Data("Codeine 20mg IR", Shipment.Flags.None)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.codeine.20mg.ir", 4)
//							}
//						}
//					},

//					new("Pervitin 50mg ER", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 4.00f,

//						shipment = new Shipment.Data("Pervitin 50mg ER", Shipment.Flags.None)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.pervitin.50mg.er", 4)
//							}
//						}
//					},

//					new("Pervitin 100mg ER", "", origin: "doctor", flags: Loadout.Kit.Flags.Unselect)
//					{
//						cost = 6.00f,

//						shipment = new Shipment.Data("Pervitin 100mg ER", Shipment.Flags.None)
//						{
//							items =
//							{
//								[0] = Shipment.Item.Prefab("bp.pervitin.100mg.er", 4)
//							}
//						}
//					},
//#endregion
//				};
//			}
		
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
	}
}
