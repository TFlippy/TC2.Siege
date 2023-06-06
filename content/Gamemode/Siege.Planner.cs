using Keg.Engine.Game;
using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		// TODO: rename to Coordinator
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Coordinator: IComponent
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

			public int last_wave;

			public int wave_size;
			public int wave_size_rem;

			public int pref_size_assault = 4;
			public int pref_size_defense = 3;
			public int pref_size_support = 6;

			//public float wave_interval = 60.00f;

			public Siege.Coordinator.Flags flags;
			public Siege.Coordinator.Status status;

			[Save.Ignore, Net.Ignore] public Vector2 pos_frontline;
			[Save.Ignore, Net.Ignore] public Vector2 pos_support;
			[Save.Ignore, Net.Ignore] public Vector2 pos_obstacle;
			[Save.Ignore, Net.Ignore] public Vector2 pos_goal;
			[Save.Ignore, Net.Ignore] public Vector2 pos_origin;

			[Save.Ignore, Net.Ignore] public EntRef<Transform.Data> ref_target;
			[Save.Ignore, Net.Ignore] public EntRef<Squad.Data> ref_squad_defense;
			[Save.Ignore, Net.Ignore] public EntRef<Squad.Data> ref_squad_assault;
			[Save.Ignore, Net.Ignore] public EntRef<Squad.Data> ref_squad_support;

			//[Save.Ignore, Net.Ignore] public float score_

			[Save.Ignore, Net.Ignore] public float next_dispatch;
			[Save.Ignore, Net.Ignore] public float next_order;
			[Save.Ignore, Net.Ignore] public float next_spawn;
			[Save.Ignore, Net.Ignore] public float next_search;
			[Save.Ignore, Net.Ignore] public float next_buy;
			//[Save.Ignore, Net.Ignore] public float next_wave;

			public Coordinator()
			{

			}
		}

		public partial struct DEV_SpawnUnitRPC: Net.IRPC<Dormitory.Data>
		{
			public ICharacter.Handle h_character;
			public FixedArray4<IKit.Handle> kits;
			public Entity ent_squad;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Dormitory.Data data)
			{
				ref var region = ref connection.GetRegion();
				ref var player = ref connection.GetPlayer();
				var random = XorRandom.New(true);

				//var characters = data.GetCharacterSpan();
				//var ts = Timestamp.Now();
				//characters.Compact();
				//var ts_elapsed = ts.GetMilliseconds();
				//App.WriteLine($"Compacted in {ts_elapsed:0.00000} ms");
				//data.Sync(entity, true);

				if (region.IsNotNull() && player.IsNotNull() && entity.GetFaction() == player.faction_id && this.ent_squad.GetFaction() == player.faction_id)
				{
					SpawnUnit(ref region, ref random, this.ent_squad, entity, ref data, this.kits.AsSpan(), this.h_character, player.faction_id);
				}
			}
#endif
		}

		public partial struct DEV_BuyUnitRPC: Net.IRPC<Dormitory.Data>
		{
			public IOrigin.Handle h_origin;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Dormitory.Data data)
			{
				ref var region = ref connection.GetRegion();
				ref var player = ref connection.GetPlayer();
				var random = XorRandom.New(true);

				if (region.IsNotNull() && player.IsNotNull() && entity.GetFaction() == player.faction_id)
				{
					BuyUnit(ref region, ref random, entity, ref data, this.h_origin, player.faction_id);
				}
			}
#endif
		}

#if SERVER
		public static void SetSquadOrder(ref Region.Data region, OwnedComponent<Squad.Data> oc_squad, Vector2 target_pos, Entity ent_target = default)
		{
			ref var squad = ref oc_squad.data;
			if (squad.IsNotNull())
			{
				var positions_span = squad.positions.AsSpan();

				var pos_b = squad.position_a ?? target_pos;

				//if () // && Vector2.DistanceSquared(pos_a, pos_b) > (6 * 6))
				{
					region.Repath(target_pos, pos_b, ref positions_span, out var repath_result, air: false, include: Terrain.PathFlags.Ground, require: Terrain.PathFlags.Ground, exclude: Terrain.PathFlags.Solid | Terrain.PathFlags.Ladder | Terrain.PathFlags.Wall, max_depth: 8);
				}
				//else
				//{
				//	region.RepathWander(pos_a, ref positions_span, out var repath_result, air: false, include: Terrain.PathFlags.Ground, require: Terrain.PathFlags.Ground, exclude: Terrain.PathFlags.Solid | Terrain.PathFlags.Ladder | Terrain.PathFlags.Wall, max_depth: 8);
				//}


				squad.position_a = target_pos;
				squad.position_b = target_pos;

				squad.ent_target = ent_target;
				squad.t_last_order = region.GetWorldTime();

				squad.Sync(oc_squad.entity, true);
			}
		}

		public static void SpawnUnit(ref Region.Data region, ref XorRandom random, Entity ent_squad, Entity ent_dormitory, ref Dormitory.Data dormitory, Span<IKit.Handle> kits, ICharacter.Handle h_character, IFaction.Handle h_faction = default)
		{
			ref var transform = ref ent_dormitory.GetComponent<Transform.Data>();
			if (transform.IsNotNull())
			{
				//if (Dormitory.TryGenerateKits(ref data, ref random, this.h_character))
				//{
				//	App.WriteLine("ok");
				//}

				Dormitory.SpawnCharacterWithKits(ref region, position: transform.position, ent_dormitory: ent_dormitory, h_character: h_character, kits: kits, h_faction: h_faction).ContinueWith((ent_unit) =>
				{
					ent_unit.AddRel<Squad.Relation>(ent_squad);

					foreach (var h_inventory in ent_unit.GetInventories())
					{
						h_inventory.Flags |= Inventory.Flags.Unlimited | Inventory.Flags.No_Drop;
					}
				});
			}
		}

		public static void BuyUnit(ref Region.Data region, ref XorRandom random, Entity ent_dormitory, ref Dormitory.Data dormitory, IOrigin.Handle h_origin, IFaction.Handle h_faction = default)
		{
			var span_characters = dormitory.GetCharacterSpan();
			if (span_characters.TryGetEmptyIndex(out var index))
			{
				var h_character = Dormitory.CreateCharacter(ref region, ref random, h_origin, h_faction: h_faction);

				if (Dormitory.TryGenerateKits(ref dormitory, ref random, h_character))
				{
					//App.WriteLine("ok");
				}

				span_characters[index] = h_character;

				dormitory.Sync(ent_dormitory, true);
			}
		}
#endif

#if SERVER
		[ISystem.Event<Despawn.DespawnEvent>(ISystem.Mode.Single, order: -10)]
		public static void OnDespawn(ISystem.Info info, Entity entity, ref Region.Data region, ref Despawn.DespawnEvent data)
		{
			App.WriteLine($"Despawned {entity.GetFullName()}");
		}

		public static void SetKoboldLoadout(Entity ent_kobold, float weapon_mult = 1.00f, float armor_mult = 1.00f)
		{
			App.WriteLine($"weapon mult: {weapon_mult}; armor mult: {armor_mult}");

			var random = XorRandom.New(true);
			var loadout = new Loadout.Data();
			var bounty = new Siege.Bounty.Data();

			ref var shipment = ref loadout.shipments[0];
			shipment.flags.SetFlag(Shipment.Flags.Unpack, true);

			var items_span = shipment.items.AsSpan();
			var rewards_span = bounty.rewards.AsSpan();

			ref var gunner = ref ent_kobold.GetComponent<AI.Gunner.Data>();

			// TODO: add proper .hjson loot tables
			var num = random.NextIntRange(0, 11);
			//num = 100;
			switch (num)
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
						items_span.Add(Shipment.Item.Prefab("sledgehammer", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(10));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("drill", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(30));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("chainsaw", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(20));
					}

					if (random.NextBool(0.25f * armor_mult))
					{
						items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(30));
					}

					if (random.NextBool(0.20f * armor_mult))
					{
						items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(30));
					}

					rewards_span.Add(Crafting.Product.Money(20));
				}
				break;

				// Shotgunner
				case 2:
				case 3:
				{
					if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("blunderbuss", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_musket.shot", 50));
						rewards_span.Add(Crafting.Product.Money(10));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("scattergun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_sg.slug", 32));
						rewards_span.Add(Crafting.Product.Money(30));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("pump_shotgun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_sg.buck", 32));
						rewards_span.Add(Crafting.Product.Money(20));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("auto_shotgun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_sg.buck", 32));
						rewards_span.Add(Crafting.Product.Money(30));
					}

					if (random.NextBool(0.20f * armor_mult))
					{
						items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(15));
					}

					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("crowbar", flags: Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(45));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("club", flags: Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(25));
					}

					if (random.NextBool(0.35f * armor_mult))
					{
						items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(20));
					}

					rewards_span.Add(Crafting.Product.Money(40));
				}
				break;

				// Light
				case 4:
				case 5:
				case 6:
				case 7:
				{
					if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("smg", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_lc", 200));
						rewards_span.Add(Crafting.Product.Money(30));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("carbine", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_hc", 50));
						rewards_span.Add(Crafting.Product.Money(50));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("rifle", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_hc", 50));
						rewards_span.Add(Crafting.Product.Money(50));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("machine_pistol", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_lc", 200));
						rewards_span.Add(Crafting.Product.Money(50));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("battle_rifle", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_hc", 80));
						rewards_span.Add(Crafting.Product.Money(60));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("revolver", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_hc", 50));
						rewards_span.Add(Crafting.Product.Money(30));
					}

					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("knife", flags: Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(50));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("crowbar", flags: Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(45));
					}

					if (random.NextBool(0.15f * armor_mult))
					{
						items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(20));
					}

					rewards_span.Add(Crafting.Product.Money(50));
				}
				break;

				// Heavy
				case 8:
				case 9:
				{
					if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("auto_shotgun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_sg.slug", 64));
						rewards_span.Add(Crafting.Product.Money(60));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("battle_rifle", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_hc.hv", 80));
						rewards_span.Add(Crafting.Product.Money(50));
					}
					else if (random.NextBool(0.40f))
					{
						items_span.Add(Shipment.Item.Prefab("auto_shotgun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_sg.grenade", 64));
						rewards_span.Add(Crafting.Product.Money(100));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("slugthrower", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_musket", 100));
						rewards_span.Add(Crafting.Product.Money(100));

						if (gunner.IsNotNull())
						{
							gunner.attack_burst_time += 3.00f;
						}
					}

					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("machete", flags: Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(75));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("crowbar", flags: Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(45));
					}

					//if (random.NextBool(1.00f))
					{
						if (random.NextBool(0.40f * armor_mult)) items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Prefab(random.NextBool(0.40f * armor_mult) ? "helmet.01" : "helmet.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(50));
					}

					rewards_span.Add(Crafting.Product.Money(80));
				}
				break;

				// Turtle
				case 10:
				{
					// Don't spawn turtles early-on
					if (!random.NextBool(0.70f * armor_mult))
					{
						goto case 0;
					}

					items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
					if (random.NextBool(0.80f * armor_mult)) items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));

					if (random.NextBool(0.70f * armor_mult))
					{
						items_span.Add(Shipment.Item.Prefab("shield.heavy", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(75));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("shield", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(30));
					}

					rewards_span.Add(Crafting.Product.Money(50));
				}
				break;

				// Artillery
				default:
				{
					if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("bazooka", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_rocket", 16));
						rewards_span.Add(Crafting.Product.Money(100));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("scattergun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_sg.grenade", 32));
						rewards_span.Add(Crafting.Product.Money(60));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("pump_shotgun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_sg.grenade", 32));
						rewards_span.Add(Crafting.Product.Money(70));
					}

					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("crowbar", flags: Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(45));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("club", flags: Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(25));
					}

					//if (random.NextBool(0.50f))
					{
						if (random.NextBool(0.30f * armor_mult)) items_span.Add(Shipment.Item.Prefab("armor.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						if (random.NextBool(0.30f * armor_mult)) items_span.Add(Shipment.Item.Prefab("helmet.00", flags: Shipment.Item.Flags.Equip | Shipment.Item.Flags.Despawn));
						rewards_span.Add(Crafting.Product.Money(50));
					}

					rewards_span.Add(Crafting.Product.Money(70));
				}
				break;
			}

			ref var loadout_new = ref ent_kobold.GetOrAddComponent<Loadout.Data>(sync: false, ignore_mask: true);
			if (!loadout_new.IsNull())
			{
				loadout_new = loadout;
			}

			ref var bounty_new = ref ent_kobold.GetOrAddComponent<Siege.Bounty.Data>(sync: false, ignore_mask: true);
			if (!bounty_new.IsNull())
			{
				bounty_new = bounty;
				//App.WriteLine($"add bounty {bounty_new.rewards[0].type} {bounty_new.rewards[0].amount}");
			}

			//ref var ai = ref ent_kobold.GetComponent<AI.Data>();
			//if (!ai.IsNull())
			//{
			//	ai.stance = AI.Stance.Aggressive;
			//}

			foreach (var h_inventory in ent_kobold.GetInventories())
			{
				h_inventory.Flags |= Inventory.Flags.Unlimited | Inventory.Flags.No_Drop;
			}

			ref var marker = ref ent_kobold.GetOrAddComponent<Minimap.Marker.Data>(sync: true);
			if (!marker.IsNull())
			{
				marker.sprite = new Sprite("ui_icons_minimap", 16, 16, 0, 0);
			}
		}

		public static void SetGiantLoadout(Entity ent_hoob, float weapon_mult = 1.00f, float armor_mult = 1.00f)
		{
			var random = XorRandom.New(true);
			var loadout = new Loadout.Data();
			var bounty = new Siege.Bounty.Data();

			ref var shipment = ref loadout.shipments[0];
			shipment.flags.SetFlag(Shipment.Flags.Unpack, true);

			var items_span = shipment.items.AsSpan();
			var rewards_span = bounty.rewards.AsSpan();

			// TODO: add proper .hjson loot tables

			var num = random.NextIntRange(0, 3);
			//num = 10;

			switch (num)
			{
				// Heavy
				case 0:
				case 1:
				case 2:
				{
					if (random.NextBool(0.40f))
					{
						items_span.Add(Shipment.Item.Prefab("machine_gun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_mg", 100));
						rewards_span.Add(Crafting.Product.Money(1000));
					}
					else if (random.NextBool(0.40f))
					{
						items_span.Add(Shipment.Item.Prefab("crankgun", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_hc.hv", 100));
						rewards_span.Add(Crafting.Product.Money(600));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("bp.hyperbobus", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_ac", 40));
						rewards_span.Add(Crafting.Product.Money(1500));
					}
					else if (random.NextBool(0.40f))
					{
						items_span.Add(Shipment.Item.Prefab("cannon.short", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));

						if (random.NextBool(0.50f))
						{
							items_span.Add(Shipment.Item.Resource("ammo_shell.shrapnel", 10));
						}
						else
						{
							items_span.Add(Shipment.Item.Resource("ammo_shell.kinetic", 20));
						}

						rewards_span.Add(Crafting.Product.Money(1500));
					}
					else if (random.NextBool(0.30f))
					{
						items_span.Add(Shipment.Item.Prefab("autocannon", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_ac", 40));
						rewards_span.Add(Crafting.Product.Money(800));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("slugthrower", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_musket", 100));
						rewards_span.Add(Crafting.Product.Money(400));
					}

					rewards_span.Add(Crafting.Product.Money(2500));
				}
				break;

				// Artillery
				default:
				{
					if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("cannon.short", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));

						if (random.NextBool(0.70f))
						{
							items_span.Add(Shipment.Item.Resource("ammo_shell", 10));
						}
						else
						{
							items_span.Add(Shipment.Item.Resource("ammo_shell.he", 10));
						}

						rewards_span.Add(Crafting.Product.Money(1500));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("bp.smrtec.a24", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
						items_span.Add(Shipment.Item.Resource("ammo_sg.grenade", 40));
						rewards_span.Add(Crafting.Product.Money(1000));
					}
					//else
					//{
					//	items_span.Add(Shipment.Item.Prefab("autocannon", flags: Shipment.Item.Flags.Pickup | Shipment.Item.Flags.Despawn));
					//	items_span.Add(Shipment.Item.Resource("ammo_ac.he", 40));
					//	rewards_span.Add(Crafting.Product.Money(1000));
					//}

					rewards_span.Add(Crafting.Product.Money(1500));
				}
				break;
			}

			ref var loadout_new = ref ent_hoob.GetOrAddComponent<Loadout.Data>(sync: false, ignore_mask: true);
			if (!loadout_new.IsNull())
			{
				loadout_new = loadout;
			}

			ref var bounty_new = ref ent_hoob.GetOrAddComponent<Siege.Bounty.Data>(sync: false, ignore_mask: true);
			if (!bounty_new.IsNull())
			{
				bounty_new = bounty;
				//App.WriteLine($"add bounty {bounty_new.rewards[0].type} {bounty_new.rewards[0].amount}");
			}

			//ref var ai = ref ent_hoob.GetComponent<AI.Data>();
			//if (!ai.IsNull())
			//{
			//	ai.stance = AI.Stance.Aggressive;
			//}

			foreach (var h_inventory in ent_hoob.GetInventories())
			{
				h_inventory.Flags |= Inventory.Flags.Unlimited | Inventory.Flags.No_Drop;
			}

			ref var marker = ref ent_hoob.GetOrAddComponent<Minimap.Marker.Data>(sync: true);
			if (!marker.IsNull())
			{
				marker.sprite = new Sprite("ui_icons_minimap", 16, 16, 0, 0);
			}
		}

		[ISystem.Event<Spawner.SpawnEvent>(ISystem.Mode.Single, order: 1000)]
		public static void OnSpawn(ISystem.Info info, Entity entity, ref Spawner.SpawnEvent data,
		[Source.Owned] ref Spawner.Data spawner, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state)
		{
			var weapon_mult = 1.00f;
			var armor_mult = 1.00f;

			armor_mult = g_siege_state.difficulty * 0.04f;

			//App.WriteLine($"spawn event {data.ent_target}");
			SetKoboldLoadout(data.ent_target, weapon_mult: weapon_mult, armor_mult: armor_mult);

			ref var commandable = ref data.ent_target.GetComponent<Commandable.Data>();
			if (!commandable.IsNull())
			{
				commandable.flags |= Commandable.Data.Flags.No_Select;

				//App.WriteLine(commandable.flags);
			}
		}

		public static bool TryFindTarget(ref Region.Data region, Entity ent_coordinator, IFaction.Handle faction, Vector2 position_src, out Entity ent_target, out Vector2 position_target)
		{
			var arg = new FindTargetArgs(ent_coordinator, faction.id, position_src, default, default, float.MaxValue, default);

			foreach (ref var row in region.IterateQuery<Siege.GetAllTargetsQuery>())
			{
				row.Run((ISystem.Info info, Entity entity, in Siege.Target.Data target, in Transform.Data transform, in Faction.Data faction) =>
				{
					var dist_sq = Vector2.DistanceSquared(transform.position, arg.position);
					if ((faction.id == 0 || faction.id != arg.faction_id) && dist_sq < arg.target_dist_nearest_sq)
					{
						arg.ent_target = entity;
						arg.target_dist_nearest_sq = dist_sq;
						arg.target_position = transform.position;

						//if (arg.ent_root.id == 0)
						//{
						//	arg.ent_root = arg.ent_search.GetRoot(Relation.Type.Child);
						//}

						//var ent_root = entity.GetRoot(Relation.Type.Child);
						//if (ent_root != arg.ent_root && ent_root.GetRoot(Relation.Type.Instance) != arg.ent_root)
						//{
						//	arg.ent_target = entity;
						//	arg.target_dist_nearest_sq = dist_sq;
						//	arg.target_position = transform.position;
						//}
					}
				});
			}

			ent_target = arg.ent_target;
			position_target = arg.target_position;

			return arg.ent_target.IsAlive();
		}

		public static bool TryFindAssaultTarget(ref Region.Data region, Entity ent_coordinator, IFaction.Handle faction, Vector2 position_src, out Entity ent_target, out Vector2 position_target)
		{
			var arg = new FindTargetArgs(ent_coordinator, faction.id, position_src, default, default, float.MaxValue, default);

			foreach (ref var row in region.IterateQuery<Siege.GetAllTargetsQuery>())
			{
				row.Run((ISystem.Info info, Entity entity, in Siege.Target.Data target, in Transform.Data transform, in Faction.Data faction) =>
				{
					var dist_sq = Vector2.DistanceSquared(transform.position, arg.position);
					if ((faction.id != arg.faction_id) && dist_sq < arg.target_dist_nearest_sq)
					{
						arg.ent_target = entity;
						arg.target_dist_nearest_sq = dist_sq;
						arg.target_position = transform.position;
					}
				});
			}

			ent_target = arg.ent_target;
			position_target = arg.target_position;

			return arg.ent_target.IsAlive();
		}

		public static bool TryFindSupportTarget(ref Region.Data region, Entity ent_coordinator, IFaction.Handle faction, Vector2 position_src, out Entity ent_target, out Vector2 position_target)
		{
			var arg = new FindTargetArgs(ent_coordinator, faction.id, position_src, default, default, float.MaxValue, default);

			foreach (ref var row in region.IterateQuery<Siege.GetAllTargetsQuery>())
			{
				row.Run((ISystem.Info info, Entity entity, in Siege.Target.Data target, in Transform.Data transform, in Faction.Data faction) =>
				{
					var dist_sq = Vector2.DistanceSquared(transform.position, arg.position);
					if (dist_sq < arg.target_dist_nearest_sq && (faction.id == arg.faction_id ? target.current_capture_progress_norm <= 0.25f : target.current_capture_progress_norm <= 0.50f))
					{
						arg.ent_target = entity;
						arg.target_dist_nearest_sq = dist_sq;
						arg.target_position = transform.position;
					}
				});
			}

			ent_target = arg.ent_target;
			position_target = arg.target_position;

			return arg.ent_target.IsAlive();
		}

		public static bool TryFindDefenseTarget(ref Region.Data region, Entity ent_coordinator, IFaction.Handle faction, Vector2 position_src, out Entity ent_target, out Vector2 position_target)
		{
			var arg = new FindTargetArgs(ent_coordinator, faction.id, position_src, default, default, float.MaxValue, default);

			foreach (ref var row in region.IterateQuery<Siege.GetAllTargetsQuery>())
			{
				row.Run((ISystem.Info info, Entity entity, in Siege.Target.Data target, in Transform.Data transform, in Faction.Data faction) =>
				{
					var dist_sq = Vector2.DistanceSquared(transform.position, arg.position);
					if (dist_sq < arg.target_dist_nearest_sq && ((faction.id == 0 && target.current_capture_progress_norm < 1.00f) || (faction.id == arg.faction_id && target.current_capture_progress_norm <= 0.90f)))
					{
						arg.ent_target = entity;
						arg.target_dist_nearest_sq = dist_sq;
						arg.target_position = transform.position;
					}
				});
			}

			ent_target = arg.ent_target;
			position_target = arg.target_position;

			return arg.ent_target.IsAlive();
		}

		private struct FindNearestSpawnArgs
		{
			public IFaction.Handle faction_id;

			public Vector2 position_target;
			public float target_dist_nearest_sq;

			public Entity ent_spawn;
			public Vector2 position_spawn;

			public FindNearestSpawnArgs(IFaction.Handle faction_id, Vector2 position_target)
			{
				this.faction_id = faction_id;

				this.position_target = position_target;
				this.target_dist_nearest_sq = float.MaxValue;

				this.ent_spawn = default;
				this.position_spawn = default;
			}
		}

		public static bool TryFindNearestSpawn(ref Region.Data region, IFaction.Handle faction, Vector2 position_target, out Entity ent_spawn, out Vector2 position_spawn)
		{
			var arg = new FindNearestSpawnArgs(faction, position_target);

			foreach (ref var row in region.IterateQuery<Region.GetSpawnsQuery>())
			{
				row.Run((ISystem.Info info, Entity entity, [Source.Owned] in Spawn.Data spawn, [Source.Owned, Optional] in Nameable.Data nameable, [Source.Owned] in Transform.Data transform, [Source.Owned, Optional] in Faction.Data faction) =>
				{
					var dist_sq = Vector2.DistanceSquared(transform.position, arg.position_target);
					if (faction.id == arg.faction_id && dist_sq < arg.target_dist_nearest_sq)
					{
						arg.ent_spawn = entity;
						arg.target_dist_nearest_sq = dist_sq;
						arg.position_spawn = transform.position;
					}
				});
			}

			ent_spawn = arg.ent_spawn;
			position_spawn = arg.position_spawn;

			return arg.ent_spawn.IsAlive();
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, interval: 0.10f)]
		public static void OnUpdate(ref Region.Data region, ref XorRandom random, ISystem.Info info, Entity entity, [Source.Owned] ref Transform.Data transform,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Selection.Data selection, [Source.Owned] ref Siege.Coordinator coordinator, [Source.Owned] ref Dormitory.Data dormitory, [Source.Owned] ref Stockpile.Data stockpile,
		[Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state,
		[Source.Owned, Optional] in Faction.Data faction)
		{
			if (Constants.World.enable_npc_spawning && Constants.World.enable_ai && g_siege_state.status == Gamemode.Status.Running && g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Active))
			{
				var match_time = g_siege_state.t_match_elapsed;
				var time = region.GetWorldTime();

				if (g_siege_state.wave_current != coordinator.last_wave)
				{
					coordinator.last_wave = g_siege_state.wave_current;

					//coordinator.next_wave = time + coordinator.wave_interval + Maths.Clamp(difficulty * 10.00f, 0.00f, 120.00f);
					coordinator.wave_size = (int)Maths.Clamp(g_siege.wave_size_base + (MathF.Floor(MathF.Pow(g_siege_state.difficulty, 0.80f)) * g_siege.wave_size_mult), 0, g_siege.wave_size_max);
					coordinator.wave_size_rem = coordinator.wave_size;

					coordinator.status = Coordinator.Status.Dispatching;

					//Notification.Push(ref region, $"Group of {coordinator.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.02", volume: 0.60f, pitch: 0.75f);
					//Notification.Push(ref region, $"Group of {coordinator.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Red, lifetime: 10.00f);
					Notification.Push(ref region, $"Group of {coordinator.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Red, lifetime: 10.00f, send_type: Net.SendType.Reliable);

					if (!stockpile.h_stockpile.IsValid())
					{
						var identifier = Asset.GenerateRandomIdentifier();
						App.WriteLine(identifier);

						var stockpile_data = new IStockpile.Data();
						stockpile_data.name = "Stuffpile";
						stockpile_data.items = new Shipment.Item[64];

						var asset = IStockpile.Database.RegisterOrUpdate(identifier, null, scope: Asset.Scope.Region, region_id: region.GetID(), data: ref stockpile_data);
						asset.Sync();

						stockpile.h_stockpile = asset;
						stockpile.Sync(entity, true);
					}

					if (!coordinator.ref_squad_defense.IsValid())
					{
						region.SpawnPrefab("squad", position: transform.position, faction_id: faction.id).ContinueWith((ent_squad) =>
						{
							ref var squad = ref ent_squad.GetComponent<Squad.Data>();
							if (squad.IsNotNull())
							{
								squad.color = 0xff1cc29a;
								squad.movement_flags = AI.Behavior.MovementFlags.Wander;
								squad.combat_flags = AI.Behavior.CombatFlags.Attack_Visible | AI.Behavior.CombatFlags.Attack_When_Hit | AI.Behavior.CombatFlags.Cover | AI.Behavior.CombatFlags.Evade | AI.Behavior.CombatFlags.Defend | AI.Behavior.CombatFlags.Flee | AI.Behavior.CombatFlags.Capture;
								//squad.flags.SetFlag(Commandable.OrderFlags.Hold, true);
								//squad.flags.SetFlag(Commandable.OrderFlags.Wander, false);
								//squad.order_type = Commandable.OrderType.Defend;
								squad.Sync(ent_squad, true);
							}

							ref var nameable = ref ent_squad.GetComponent<Nameable.Data>();
							if (nameable.IsNotNull())
							{
								nameable.name = "Defense";
								nameable.Sync(ent_squad, true);
							}

							ref var coordinator = ref entity.GetComponent<Siege.Coordinator>();
							if (coordinator.IsNotNull())
							{
								coordinator.ref_squad_defense.Set(ent_squad);
							}
						});
					}

					if (!coordinator.ref_squad_assault.IsValid())
					{
						region.SpawnPrefab("squad", position: transform.position, faction_id: faction.id).ContinueWith((ent_squad) =>
						{
							ref var squad = ref ent_squad.GetComponent<Squad.Data>();
							if (squad.IsNotNull())
							{
								squad.color = 0xffff2e1a;
								squad.movement_flags = AI.Behavior.MovementFlags.Wander | AI.Behavior.MovementFlags.Hold;
								squad.combat_flags = AI.Behavior.CombatFlags.Attack_Visible | AI.Behavior.CombatFlags.Attack_When_Hit | AI.Behavior.CombatFlags.Cover | AI.Behavior.CombatFlags.Evade | AI.Behavior.CombatFlags.Chase | AI.Behavior.CombatFlags.Capture;
								//squad.flags.SetFlag(Commandable.OrderFlags.Hold, false);
								//squad.flags.SetFlag(Commandable.OrderFlags.Wander, true);
								//squad.order_type = Commandable.OrderType.Capture;
								squad.Sync(ent_squad, true);
							}

							ref var nameable = ref ent_squad.GetComponent<Nameable.Data>();
							if (nameable.IsNotNull())
							{
								nameable.name = "Assault";
								nameable.Sync(ent_squad, true);
							}

							ref var coordinator = ref entity.GetComponent<Siege.Coordinator>();
							if (coordinator.IsNotNull())
							{
								coordinator.ref_squad_assault.Set(ent_squad);
							}
						});
					}

					if (!coordinator.ref_squad_support.IsValid())
					{
						region.SpawnPrefab("squad", position: transform.position, faction_id: faction.id).ContinueWith((ent_squad) =>
						{
							ref var squad = ref ent_squad.GetComponent<Squad.Data>();
							if (squad.IsNotNull())
							{
								squad.color = 0xffffcb00;
								squad.movement_flags = AI.Behavior.MovementFlags.Wander;
								squad.combat_flags = AI.Behavior.CombatFlags.Attack_Visible | AI.Behavior.CombatFlags.Attack_When_Hit | AI.Behavior.CombatFlags.Cover | AI.Behavior.CombatFlags.Evade | AI.Behavior.CombatFlags.Defend | AI.Behavior.CombatFlags.Flee | AI.Behavior.CombatFlags.Chase | AI.Behavior.CombatFlags.Capture;

								//squad.flags.SetFlag(Commandable.OrderFlags.Hold, true);
								//squad.flags.SetFlag(Commandable.OrderFlags.Wander, true);
								//squad.order_type = Commandable.OrderType.Attack;
								squad.Sync(ent_squad, true);
							}

							ref var nameable = ref ent_squad.GetComponent<Nameable.Data>();
							if (nameable.IsNotNull())
							{
								nameable.name = "Support";
								nameable.Sync(ent_squad, true);
							}

							ref var coordinator = ref entity.GetComponent<Siege.Coordinator>();
							if (coordinator.IsNotNull())
							{
								coordinator.ref_squad_support.Set(ent_squad);
							}
						});
					}
				}

				if (!g_siege_state.flags.HasAny(Siege.Gamemode.Flags.No_Dispatcher))
				{
					if (match_time >= coordinator.next_buy)
					{
						var characters_span = dormitory.GetCharacterSpan();
						var characters_empty_count = characters_span.GetEmptyCount();

						if (characters_empty_count > 0)
						{
							var h_species_kobold = new ISpecies.Handle("kobold");

							Span<IOrigin.Handle> origins = stackalloc IOrigin.Handle[16];
							IOrigin.Database.GetHandlesFiltered(ref origins, in h_species_kobold, (IOrigin.Definition d_origin, in ISpecies.Handle h_species) =>
							{
								return d_origin.data.species == h_species;
							});

							for (var i = 0; i < Math.Min(3, characters_empty_count); i++)
							{
								Siege.BuyUnit(ref region, ref random, entity, ref dormitory, h_origin: origins.GetRandom(ref random), h_faction: faction.id);
							}
						}

						coordinator.next_buy = match_time + random.NextFloatRange(5.00f, 15.00f);
					}
				}

				switch (coordinator.status)
				{
					case Coordinator.Status.Undefined:
					{
						//coordinator.next_wave = time + 60.00f;
						coordinator.status = Coordinator.Status.Waiting;
					}
					break;

					case Coordinator.Status.Waiting:
					{

					}
					break;

					case Siege.Coordinator.Status.Dispatching:
					{
						if (true) //(g_siege_state.t_next_wave - match_time) >= 30.00f)
						{
							if (!g_siege_state.flags.HasAny(Siege.Gamemode.Flags.No_Dispatcher))
							{
								if (match_time >= coordinator.next_dispatch)
								{
									coordinator.next_dispatch = match_time + random.NextFloatRange(1.00f, 3.00f);

									WorldNotification.Push(ref region, $"Wave: {coordinator.wave_size - coordinator.wave_size_rem}/{coordinator.wave_size};", Color32BGRA.Red, transform.position, lifetime: 4);

									var characters_span = dormitory.GetCharacterSpan();

									{
										ref var squad = ref coordinator.ref_squad_assault.GetValueOrNullRef();
										if (squad.IsNotNull())
										{
											//if ((time - squad.t_last_order) >= 15.00f)
											if (time >= squad.t_next_order)
											{
												if (squad.unit_count >= 2)
												{
													if (true) //squad.idle_count > squad.unit_count * 0.50f)
													{
														if (Siege.TryFindAssaultTarget(ref region, entity, faction.id, squad.position_a ?? transform.position, out var ent_target, out var pos_target))
														{
															Siege.SetSquadOrder(ref region, new OwnedComponent<Squad.Data>(ref squad, coordinator.ref_squad_assault.entity), target_pos: pos_target, ent_target: ent_target);
															squad.t_next_order = time + random.NextFloatRange(3.00f, 8.00f);
														}
														else
														{
															squad.t_next_order = time + random.NextFloatRange(5.00f, 10.00f);
														}
													}
													else
													{
														squad.t_next_order = time + random.NextFloatRange(10.00f, 25.00f);
													}
												}
												else
												{
													Siege.SetSquadOrder(ref region, new OwnedComponent<Squad.Data>(ref squad, coordinator.ref_squad_assault.entity), target_pos: transform.position, ent_target: default);
													squad.t_next_order = time + random.NextFloatRange(5.00f, 9.00f);
												}
											}

											var squad_size = squad.unit_count;

											var num = Maths.Clamp(random.NextIntRange(1, coordinator.wave_size_rem), 0, Math.Max(0, coordinator.pref_size_assault - squad_size));
											for (var i = 0; i < num; i++)
											{
												if (characters_span.TryGetValidIndex(out var character_index))
												{
													Siege.SpawnUnit(ref region, ref random, coordinator.ref_squad_assault.entity, entity, ref dormitory, default, h_character: characters_span[character_index], h_faction: faction.id);
													coordinator.wave_size_rem--;
												}
											}
										}
									}

									{
										ref var squad = ref coordinator.ref_squad_support.GetValueOrNullRef();
										if (squad.IsNotNull())
										{
											if (time >= squad.t_next_order)
											{
												// squad.idle_count > squad.unit_count * 0.50f && 

												//if (squad.unit_count >= 2)
												{
													if (squad.idle_count > squad.unit_count * 0.20f)
													{
														if (Siege.TryFindSupportTarget(ref region, entity, faction.id, squad.position_a ?? transform.position, out var ent_target, out var pos_target))
														{
															Siege.SetSquadOrder(ref region, new OwnedComponent<Squad.Data>(ref squad, coordinator.ref_squad_support.entity), target_pos: pos_target, ent_target: ent_target);
														}
														else
														{
															ref var squad_assault = ref coordinator.ref_squad_assault.GetValueOrNullRef();
															if (squad_assault.IsNotNull())
															{
																Span<Vector2> span_positions = stackalloc Vector2[4];
																region.RepathHigh(squad.position_a ?? transform.position, squad_assault.position_a ?? transform.position, ref span_positions, out var repath_result, include: Terrain.PathFlags.Ground | Terrain.PathFlags.Edge | Terrain.PathFlags.Ladder, max_depth: 4);

																if (repath_result != Terrain.RepathResult.Unreachable)
																{
																	Siege.SetSquadOrder(ref region, new OwnedComponent<Squad.Data>(ref squad, coordinator.ref_squad_support.entity), target_pos: span_positions[span_positions.Length - 1], ent_target: ent_target);
																}
															}
														}

														squad.t_next_order = time + random.NextFloatRange(3.00f, 5.00f);
													}
													else
													{
														squad.t_next_order = time + random.NextFloatRange(1.00f, 2.00f);
													}
												}

												//if (Siege.TryFindSupportTarget(ref region, entity, faction.id, squad.position_a ?? transform.position, out var ent_target, out var pos_target))
												//{
												//	Siege.SetSquadOrder(ref region, new OwnedComponent<Squad.Data>(ref squad, coordinator.ref_squad_support.entity), target_pos: pos_target, ent_target: ent_target);
												//}
												//else
												//{
												//	if (squad.unit_count >= 2)
												//	{
												//		ref var squad_assault = ref coordinator.ref_squad_assault.GetValueOrNullRef();
												//		if (squad_assault.IsNotNull())
												//		{
												//			Span<Vector2> span_positions = stackalloc Vector2[4];
												//			region.RepathHigh(squad.position_a ?? transform.position, squad_assault.position_a ?? transform.position, ref span_positions, out var repath_result, include: Terrain.PathFlags.Ground | Terrain.PathFlags.Edge | Terrain.PathFlags.Ladder, max_depth: 4);

												//			if (repath_result != Terrain.RepathResult.Unreachable)
												//			{
												//				Siege.SetSquadOrder(ref region, new OwnedComponent<Squad.Data>(ref squad, coordinator.ref_squad_support.entity), target_pos: span_positions[span_positions.Length - 1], ent_target: ent_target);
												//			}
												//		}
												//	}
												//	else
												//	{
												//		Siege.SetSquadOrder(ref region, new OwnedComponent<Squad.Data>(ref squad, coordinator.ref_squad_support.entity), target_pos: transform.position, ent_target: default);
												//	}
												//}
											}

											//squad_front.order_type = Commandable.OrderType.Capture;
											//squad_front.flags = Commandable.OrderFlags.None;

											var squad_size = squad.unit_count;

											var num = Maths.Clamp(random.NextIntRange(1, coordinator.wave_size_rem), 0, Math.Max(0, coordinator.pref_size_support - squad_size));
											for (var i = 0; i < num; i++)
											{
												if (characters_span.TryGetValidIndex(out var character_index))
												{
													Siege.SpawnUnit(ref region, ref random, coordinator.ref_squad_support.entity, entity, ref dormitory, default, h_character: characters_span[character_index], h_faction: faction.id);
													coordinator.wave_size_rem--;
												}
											}
										}
									}

									{
										ref var squad = ref coordinator.ref_squad_defense.GetValueOrNullRef();
										if (squad.IsNotNull())
										{
											if ((time - squad.t_last_order) >= 15.00f)
											{
												if (Siege.TryFindDefenseTarget(ref region, entity, faction.id, squad.position_a ?? transform.position, out var ent_target, out var pos_target))
												{
													Siege.SetSquadOrder(ref region, new OwnedComponent<Squad.Data>(ref squad, coordinator.ref_squad_defense.entity), target_pos: pos_target, ent_target: ent_target);
												}
											}

											//squad_front.order_type = Commandable.OrderType.Capture;
											//squad_front.flags = Commandable.OrderFlags.None;

											var squad_size = squad.unit_count;

											var num = Maths.Clamp(random.NextIntRange(1, coordinator.wave_size_rem), 0, Math.Max(0, coordinator.pref_size_defense - squad_size));
											for (var i = 0; i < num; i++)
											{
												if (characters_span.TryGetValidIndex(out var character_index))
												{
													Siege.SpawnUnit(ref region, ref random, coordinator.ref_squad_defense.entity, entity, ref dormitory, default, h_character: characters_span[character_index], h_faction: faction.id);
													coordinator.wave_size_rem--;
												}
											}
										}
									}
								}
							}
						}
						else
						{
							coordinator.status = Coordinator.Status.Waiting;
						}
					}
					break;
				}
			}
		}
#endif
	}
}
