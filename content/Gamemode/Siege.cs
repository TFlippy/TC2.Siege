using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		[IGamemode.Data("Siege", "")]
		public partial struct Gamemode: IGamemode
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,
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

#if SERVER
				Player.OnCreate += OnPlayerCreate;
				static void OnPlayerCreate(ref Region.Data region, ref Player.Data player)
				{
					player.SetFaction("defenders");

					Character.Create(ref region, "Soldier", prefab: "human.male", flags: Character.Flags.Human | Character.Flags.Military, origin: Character.Origin.Soldier, gender: Organic.Gender.Male, player_id: player.id, hair_frame: 5, beard_frame: 1);
					Character.Create(ref region, "Engineer", prefab: "human.male", flags: Character.Flags.Human | Character.Flags.Engineering | Character.Flags.Military, origin: Character.Origin.Engineer, gender: Organic.Gender.Male, player_id: player.id, hair_frame: 2, beard_frame: 7);
					Character.Create(ref region, "Medic", prefab: "human.female", flags: Character.Flags.Human | Character.Flags.Medical | Character.Flags.Military, origin: Character.Origin.Doctor, gender: Organic.Gender.Female, player_id: player.id, hair_frame: 10);
				}
#endif

#if CLIENT
				Character.CreationGUI.enabled = false;
				Character.CharacterHUD.enabled = false;

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
					new("Armor", "", origin: Character.Origin.Soldier)
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

					new("Shield", "", origin: Character.Origin.Soldier)
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

					new("Machete", "", origin: Character.Origin.Soldier)
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

					new("Revolver", "", origin: Character.Origin.Soldier)
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

					new("Rifle", "", origin: Character.Origin.Soldier)
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

					new("SMG", "", origin: Character.Origin.Soldier)
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

					new("Battle Rifle", "", origin: Character.Origin.Soldier)
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

					new("ABR 740", "", origin: Character.Origin.Soldier)
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

					new("Majzl A-749", "", origin: Character.Origin.Soldier)
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

					new("Auto-Shotgun", "", origin: Character.Origin.Soldier)
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

					new("Grenade", "", origin: Character.Origin.Soldier)
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

					new("Arc Lance", "", origin: Character.Origin.Soldier)
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
#endregion

#region Engineer
					new("Armor", "", origin: Character.Origin.Engineer)
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

					new("Crowbar", "", origin: Character.Origin.Engineer)
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

					new("Pickaxe", "", origin: Character.Origin.Engineer)
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

					new("Drill", "", origin: Character.Origin.Engineer)
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

					new("Pistol", "", origin: Character.Origin.Engineer)
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

					new("Pump Shotgun", "", origin: Character.Origin.Engineer)
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

					new("Scattergun (Grenades)", "", origin: Character.Origin.Engineer)
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

					new("Bazooka", "", origin: Character.Origin.Engineer)
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

					new("Tools", "", origin: Character.Origin.Engineer)
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

					new("R750 Automat", "", origin: Character.Origin.Engineer)
					{
						cost = 15.00f,

						shipment = new Shipment.Data("R750 Automat", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.r750", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_hc.arc.mt", 200)
							}
						}
					},

					new("Mamut B-738", "", origin: Character.Origin.Engineer)
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

					new("Machine Gun Kit", "", origin: Character.Origin.Engineer)
					{
						cost = 25.00f,

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

					new("Binoculars", "", origin: Character.Origin.Engineer)
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

					new("Artillery Shells (HE)", "", origin: Character.Origin.Engineer)
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

					new("Artillery Shells (Basic)", "", origin: Character.Origin.Engineer)
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

					new("Land Mines", "", origin: Character.Origin.Engineer)
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

					new("Dynamite", "", origin: Character.Origin.Engineer)
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
#endregion

#region Medic
					new("Armor", "", origin: Character.Origin.Doctor)
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

					new("Shield", "", origin: Character.Origin.Doctor)
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

					new("Knife", "", origin: Character.Origin.Doctor)
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

					new("Pistol", "", origin: Character.Origin.Doctor)
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

					new("Carbine", "", origin: Character.Origin.Doctor)
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

					new("Machine Pistol", "", origin: Character.Origin.Doctor)
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

					new("Houser A-750", "", origin: Character.Origin.Doctor)
					{
						cost = 7.00f,

						shipment = new Shipment.Data("Houser A-750", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("bp.houser.a750", flags: Shipment.Item.Flags.Pickup),
								[1] = Shipment.Item.Resource("ammo_hc.hv", 90)
							}
						}
					},

					new("Medkit", "", origin: Character.Origin.Doctor)
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

					new("Grenade", "", origin: Character.Origin.Doctor)
					{
						cost = 1.50f,

						shipment = new Shipment.Data("Grenade", Shipment.Flags.None)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("grenade")
							}
						}
					},

					new("Morfitin-B", "", origin: Character.Origin.Doctor)
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

					new("Paralyx", "", origin: Character.Origin.Doctor)
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

					new("Codeine 20mg IR", "", origin: Character.Origin.Doctor)
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

					new("Pervitin 50mg ER", "", origin: Character.Origin.Doctor)
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

					new("Pervitin 100mg ER", "", origin: Character.Origin.Doctor)
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

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Target: IComponent
		{
			public IFaction.Handle faction_id;
		}

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

				Forming,

				Waiting,
				Searching,
				Dispatching,
				Approaching,
				Attacking,
			}

			public int wave_size;
			public int wave_size_rem;
			public float wave_interval = 60.00f;

			public Siege.Planner.Flags flags;
			public Siege.Planner.Status status;

			[Save.Ignore, Net.Ignore] public float next_update;
			[Save.Ignore, Net.Ignore] public float next_regroup;
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
			App.WriteLine($"spawn event {data.ent_target}");

			SetKoboldLoadout(data.ent_target);

			ref var ai = ref data.ent_target.GetComponent<AI.Data>();
			if (!ai.IsNull())
			{
				ai.stance = AI.Stance.Aggressive;
			}

			//for (int i = 0; i < selection.units.Length; i++)
			//{
			//	ref var unit = ref selection.units[i];
			//	if (!unit.IsAlive() || unit.entity.HasTag("dead"))
			//	{
			//		unit.Set(data.ent_target);

			//		if (i >= 1)
			//		{
			//			//planner.flags.SetFlag(Siege.Planner.Flags.Ready, true);
			//			planner.status = Siege.Planner.Status.Searching;
			//		}

			//		break;
			//	}
			//}

			//planner.wave_size_rem = Maths.Clamp(planner.wave_size_rem - 1, 0, planner.wave_size);
		}

		//[ISystem.LateUpdate(ISystem.Mode.Single)]
		//public static void OnUpdate2(ISystem.Info info, Entity entity, [Source.Owned] ref Transform.Data transform,
		//[Source.Owned] ref Control.Data control, [Source.Owned] ref Selection.Data selection, [Source.Owned] ref Siege.Planner planner, [Source.Owned, Optional] in Faction.Data faction)
		//{
		//	if (info.WorldTime > planner.next_update)
		//	{
		//		ref var region = ref info.GetRegion();
		//		var random = XorRandom.New();

		//		planner.next_regroup = info.WorldTime + random.NextFloatRange(5.00f, 10.00f);
		//	}
		//}

#if SERVER
		[ChatCommand.Region("nextmap", "", creative: true)]
		public static void NextMapCommand(ref ChatCommand.Context context, string map)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				var map_handle = new Map.Handle(map);
				if (map_handle.id != 0)
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
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Selection.Data selection, [Source.Owned] ref Siege.Planner planner, [Source.Owned, Optional] in Faction.Data faction)
		{
			var time = info.WorldTime;
			if (time >= planner.next_update)
			{
				planner.next_update = time + 1.00f;

				ref var region = ref info.GetRegion();
				var random = XorRandom.New();

				//WorldNotification.Push(ref region, "target", 0xffffffff, control.mouse.position);

				////App.WriteLine(region.GetTotalTagCount("kobold"));


				var difficulty = (info.WorldTime / 60.00f);
				//spawner.group_size_extra = (int)Maths.Clamp(1 + MathF.Floor(MathF.Pow(difficulty * 0.50f, 0.50f)), 0, 6);

				//if (planner.next_wave <= 0.00f)
				//{
				//	planner.next_wave = time + planner.wave_interval;
				//}

				//if (time >= planner.next_wave)
				//{

				//}

				App.WriteLine($"next wave in {planner.next_wave - time}");

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
							planner.wave_size = (int)Maths.Clamp(3 + MathF.Floor(MathF.Pow(difficulty, 0.70f)) * 2.00f, 0, 40);
							planner.wave_size_rem = planner.wave_size;

							planner.status = Planner.Status.Dispatching;
						}


					}
					break;

					case Siege.Planner.Status.Dispatching:
					{
						if ((planner.next_wave - time) >= 30.00f)
						{
							//App.WriteLine("raid ready");
							//var arg = new GetAllTargetsQueryArgs(entity, faction.id, transform.position, default, default, float.MaxValue, default);

							//region.Query<Siege.GetAllTargetsQuery>(Func).Execute(ref arg);
							//static void Func(ISystem.Info info, Entity entity, in Siege.Target target, in Transform.Data transform)
							//{
							//	ref var arg = ref info.GetParameter<GetAllTargetsQueryArgs>();
							//	if (!arg.IsNull())
							//	{
							//		ref var region = ref info.GetRegion();

							//		var dist_sq = Vector2.DistanceSquared(transform.position, arg.position);
							//		if ((target.faction_id == 0 || target.faction_id != arg.faction_id) && dist_sq < arg.target_dist_nearest_sq)
							//		{
							//			if (arg.ent_root.id == 0)
							//			{
							//				arg.ent_root = arg.ent_search.GetRoot(Relation.Type.Child);
							//			}

							//			var ent_root = entity.GetRoot(Relation.Type.Child);
							//			if (ent_root != arg.ent_root && ent_root.GetRoot(Relation.Type.Instance) != arg.ent_root)
							//			{
							//				arg.ent_target = entity;
							//				arg.target_dist_nearest_sq = dist_sq;
							//				arg.target_position = transform.position;
							//			}
							//		}
							//	}
							//}

							if (TryFindTarget(ref region, entity, faction.id, transform.position, out var ent_target, out var target_position))
							{
								//var selection_units = selection.units;

								var arg = new GetAllUnitsQueryArgs(entity, ent_target, faction.id, transform.position, target_position, 0, planner.wave_size_rem, default);

								//if (info.WorldTime > planner.next_update)
								{
									//planner.next_regroup = info.WorldTime + random.NextFloatRange(5.00f, 10.00f);

									//var arg2 = new GetAllUnitsQueryArgs(entity, arg.ent_target, faction.id, transform.position, arg.target_position, 0, default);

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
								}

								planner.wave_size_rem = arg.wave_size_rem;

								if (arg.selection_count > 0)
								{
									selection.units = arg.selection;

									selection.order_type = Commandable.OrderType.Attack;

									control.mouse.position = target_position; // Maths.MoveTowards(control.mouse.position, arg.target_position, new Vector2(4.00f));
									control.mouse.SetKeyPressed(Mouse.Key.Right, true);

									planner.next_update = info.WorldTime + random.NextFloatRange(5.00f, 10.00f);
									//planner.wave_size_rem = Maths.Clamp(planner.wave_size_rem - arg.selection_count, 0, planner.wave_size);
								}
								else if (planner.wave_size_rem > 0)
								{
									planner.next_update = info.WorldTime + random.NextFloatRange(1.00f, 3.00f);
									spawner.next_spawn = time;
								}

								App.WriteLine($"Dispatched {arg.selection_count} ({planner.wave_size_rem}/{planner.wave_size})");

								//selection.units = default;

								//for (int i = 0; i < selection.units.Length; i++)
								//{
								//	ref var unit = ref selection.units[i];
								//	if (unit.TryGetHandle(out var handle))
								//	{
								//		ref var transform_unit = ref unit.entity.GetComponent<Transform.Data>();
								//		if (!transform_unit.IsNull())
								//		{
								//			if (Vector2.DistanceSquared(transform_unit.position, control.mouse.position) <= (16 * 16))
								//			{
								//				unit = default;
								//			}
								//		}
								//	}
								//}

								//selection.units = default;
							}


							//planner.status = Siege.Planner.Status.Attacking;
							//planner.status = Siege.Planner.Status.Forming;
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
#endif
	}
}
