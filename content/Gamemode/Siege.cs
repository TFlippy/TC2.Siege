﻿using Keg.Engine.Game;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		[Query]
		public delegate void GetAllTargetsQuery(ISystem.Info info, Entity entity, [Source.Owned] in Siege.Target target, [Source.Owned] in Transform.Data transform);

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
				Constants.Respawn.token_count_max = 50.00f;
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
						cost = 2.50f,

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
								[1] = Shipment.Item.Resource("ammo_hc", 60)
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
								[1] = Shipment.Item.Resource("ammo_lc", 150)
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
								[1] = Shipment.Item.Resource("ammo_hc.hv", 90)
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
								[1] = Shipment.Item.Resource("ammo_mg", 40)
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
						cost = 2.50f,

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
								[1] = Shipment.Item.Resource("ammo_ac", 30)
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
						cost = 2.50f,

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

				Searching,
				Approaching,
				Attacking,
			}

			public Siege.Planner.Flags flags;
			public Siege.Planner.Status status;

			[Save.Ignore, Net.Ignore] public float next_update;
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

			for (int i = 0; i < selection.units.Length; i++)
			{
				ref var unit = ref selection.units[i];
				if (!unit.IsAlive() || unit.entity.HasTag("dead"))
				{
					unit.Set(data.ent_target);

					if (i == selection.units.Length - 1)
					{
						//planner.flags.SetFlag(Siege.Planner.Flags.Ready, true);
						planner.status = Siege.Planner.Status.Searching;
					}

					break;
				}
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Spawner.Data spawner,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Selection.Data selection, [Source.Owned] ref Siege.Planner planner, [Source.Owned, Optional] in Faction.Data faction)
		{
			if (info.WorldTime > planner.next_update)
			{
				planner.next_update = info.WorldTime + 1.00f;

				ref var region = ref info.GetRegion();

				//App.WriteLine(region.GetTotalTagCount("kobold"));

				switch (planner.status)
				{
					case Siege.Planner.Status.Searching:
					{
						//if (planner.flags.HasAll(Siege.Planner.Flags.Ready))
						{
							//App.WriteLine("raid ready");
							var arg = (ent_search: entity, faction_id: faction.id, position: transform.position, ent_root: default(Entity), ent_target: default(Entity), target_dist_nearest_sq: float.MaxValue, target_position: default(Vector2));

							region.Query<Siege.GetAllTargetsQuery>(Func).Execute(ref arg);
							static void Func(ISystem.Info info, Entity entity, in Siege.Target target, in Transform.Data transform)
							{
								ref var arg = ref info.GetParameter<(Entity ent_search, IFaction.Handle faction_id, Vector2 position, Entity ent_root, Entity ent_target, float target_dist_nearest_sq, Vector2 target_position)>();
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

							if (arg.ent_target.IsValid())
							{
								selection.order_type = Commandable.OrderType.Attack;

								control.mouse.position = arg.target_position; // Maths.MoveTowards(control.mouse.position, arg.target_position, new Vector2(4.00f));
								control.mouse.SetKeyPressed(Mouse.Key.Right, true);

								planner.next_update = info.WorldTime + 5.00f;

								for (int i = 0; i < selection.units.Length; i++)
								{
									ref var unit = ref selection.units[i];
									if (unit.TryGetHandle(out var handle))
									{
										ref var transform_unit = ref unit.entity.GetComponent<Transform.Data>();
										if (!transform_unit.IsNull())
										{
											if (Vector2.DistanceSquared(transform_unit.position, control.mouse.position) <= (16 * 16))
											{
												unit = default;
											}
										}
									}
								}

								//selection.units = default;
							}

							//planner.status = Siege.Planner.Status.Attacking;
							//planner.status = Siege.Planner.Status.Forming;
						}
					}
					break;

					case Planner.Status.Attacking:
					{
						//if (arg.ent_target.IsValid())
						//{
						//	selection.order_type = Commandable.OrderType.Attack;
						//	control.mouse.position = arg.target_position; // Maths.MoveTowards(control.mouse.position, arg.target_position, new Vector2(4.00f));
						//	control.mouse.SetKeyPressed(Mouse.Key.Right, true);

						//	planner.next_update = info.WorldTime + 5.00f;
						//}

						planner.status = Siege.Planner.Status.Forming;

						//selection.units = default;
					}
					break;

					case Planner.Status.Forming:
					{

					}
					break;
				}
			}
		}
#endif
	}
}
