using Keg.Engine.Game;
using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
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

			public EntRef<Transform.Data> ref_target;

			public int last_wave;

			public int wave_size;
			public int wave_size_rem;
			//public float wave_interval = 60.00f;

			public Siege.Planner.Flags flags;
			public Siege.Planner.Status status;

			[Save.Ignore, Net.Ignore] public float next_dispatch;
			[Save.Ignore, Net.Ignore] public float next_spawn;
			[Save.Ignore, Net.Ignore] public float next_search;
			//[Save.Ignore, Net.Ignore] public float next_wave;

			public Planner()
			{

			}
		}

#if SERVER
		public static void SetKoboldLoadout(Entity ent_kobold, float weapon_mult = 1.00f, float armor_mult = 1.00f)
		{
			//App.WriteLine($"weapon mult: {weapon_mult}; armor mult: {armor_mult}");

			var random = XorRandom.New();
			var loadout = new Loadout.Data();
			var bounty = new Siege.Bounty.Data();

			ref var shipment = ref loadout.shipments[0];
			shipment.flags.SetFlag(Shipment.Flags.Unpack, true);

			var items_span = shipment.items.AsSpan();
			var rewards_span = bounty.rewards.AsSpan();

			ref var gunner = ref ent_kobold.GetComponent<Gunner.Data>();

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

			ref var ai = ref ent_kobold.GetComponent<AI.Data>();
			if (!ai.IsNull())
			{
				ai.stance = AI.Stance.Aggressive;
			}

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
			var random = XorRandom.New();
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

			ref var ai = ref ent_hoob.GetComponent<AI.Data>();
			if (!ai.IsNull())
			{
				ai.stance = AI.Stance.Aggressive;
			}

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

		public static bool TryFindTarget(ref Region.Data region, Entity ent_planner, IFaction.Handle faction, Vector2 position_src, out Entity ent_target, out Vector2 position_target)
		{
			var arg = new FindTargetArgs(ent_planner, faction.id, position_src, default, default, float.MaxValue, default);

			region.Query<Siege.GetAllTargetsQuery>(Func).Execute(ref arg);
			static void Func(ISystem.Info info, Entity entity, in Siege.Target.Data target, in Transform.Data transform, in Faction.Data faction)
			{
				ref var arg = ref info.GetParameter<FindTargetArgs>();
				if (!arg.IsNull())
				{
					ref var region = ref info.GetRegion();

					var dist_sq = Vector2.DistanceSquared(transform.position, arg.position);
					if ((faction.id == 0 || faction.id != arg.faction_id) && dist_sq < arg.target_dist_nearest_sq)
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

			region.Query<Region.GetSpawnsQuery>(Func).Execute(ref arg);
			static void Func(ISystem.Info info, Entity entity, [Source.Owned] in Spawn.Data spawn, [Source.Owned, Optional] in Nameable.Data nameable, [Source.Owned] in Transform.Data transform, [Source.Owned, Optional] in Faction.Data faction)
			{
				ref var arg = ref info.GetParameter<FindNearestSpawnArgs>();
				if (!arg.IsNull())
				{
					ref var region = ref info.GetRegion();

					var dist_sq = Vector2.DistanceSquared(transform.position, arg.position_target);
					if (faction.id == arg.faction_id && dist_sq < arg.target_dist_nearest_sq)
					{
						arg.ent_spawn = entity;
						arg.target_dist_nearest_sq = dist_sq;
						arg.position_spawn = transform.position;
					}
				}
			}

			ent_spawn = arg.ent_spawn;
			position_spawn = arg.position_spawn;

			return arg.ent_spawn.IsAlive();
		}

		[ISystem.Update(ISystem.Mode.Single, interval: 0.10f)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref Transform.Data transform,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Selection.Data selection, [Source.Owned] ref Siege.Planner planner, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state, [Source.Owned, Optional] in Faction.Data faction)
		{
			ref var region = ref info.GetRegion();

			if (Constants.World.enable_npc_spawning && Constants.World.enable_ai && g_siege_state.status == Gamemode.Status.Running && g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Active))
			{
				var time = g_siege_state.t_match_elapsed;
				if (g_siege_state.wave_current != planner.last_wave)
				{
					planner.last_wave = g_siege_state.wave_current;

					//planner.next_wave = time + planner.wave_interval + Maths.Clamp(difficulty * 10.00f, 0.00f, 120.00f);
					planner.wave_size = (int)Maths.Clamp(g_siege.wave_size_base + (MathF.Floor(MathF.Pow(g_siege_state.difficulty, 0.80f)) * g_siege.wave_size_mult), 0, g_siege.wave_size_max);
					planner.wave_size_rem = planner.wave_size;

					planner.status = Planner.Status.Dispatching;

					//Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.02", volume: 0.60f, pitch: 0.75f);
					//Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Red, lifetime: 10.00f);
					Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Red, lifetime: 10.00f, send_type: Net.SendType.Reliable);
				}

				var random = XorRandom.New();

				switch (planner.status)
				{
					case Planner.Status.Undefined:
					{
						//planner.next_wave = time + 60.00f;
						planner.status = Planner.Status.Waiting;
					}
					break;

					case Planner.Status.Waiting:
					{
						//if (siege.wave_current != planner.last_wave)
						//{
						//	planner.last_wave = siege.wave_current;

						//	//planner.next_wave = time + planner.wave_interval + Maths.Clamp(difficulty * 10.00f, 0.00f, 120.00f);
						//	planner.wave_size = (int)Maths.Clamp(3 + MathF.Floor(MathF.Pow(siege.difficulty, 0.80f)) * 2.00f, 0, 40);
						//	planner.wave_size_rem = planner.wave_size;

						//	planner.status = Planner.Status.Dispatching;

						//	//Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Yellow, lifetime: 10.00f, "ui.alert.02", volume: 0.60f, pitch: 0.75f);
						//	Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Red, lifetime: 10.00f);

						//}
					}
					break;

					case Siege.Planner.Status.Dispatching:
					{
						if ((g_siege_state.t_next_wave - time) >= 30.00f)
						{
							if (time >= planner.next_search)
							{
								if (TryFindTarget(ref region, entity, faction.id, transform.position, out var ent_target, out var target_position))
								{
									planner.ref_target.Set(ent_target);
								}
								else
								{

								}

								planner.next_search = time + random.NextFloatRange(10.00f, 15.00f);
							}

							if (time >= planner.next_dispatch)
							{
								planner.next_dispatch = time + random.NextFloatRange(5.00f, 10.00f);

								if (planner.ref_target.IsAlive() && planner.ref_target.TryGetHandle(out var h_target_transform))
								{
									var ent_target = h_target_transform.entity;
									var target_position = h_target_transform.data.position;

									var arg = new GetAllUnitsQueryArgs(entity, ent_target, faction.id, transform.position, target_position, 0, planner.wave_size_rem, default);

									region.Query<Siege.GetAllUnitsQuery>(Func2).Execute(ref arg);
									static void Func2(ISystem.Info info, Entity entity, [Source.Owned] in Commandable.Data commandable, [Source.Owned, Override] in AI.Movement movement, [Source.Owned, Override] in AI.Behavior behavior, [Source.Owned] in Transform.Data transform, [Source.Owned] in Faction.Data faction)
									{
										ref var arg = ref info.GetParameter<GetAllUnitsQueryArgs>();
										if (!arg.IsNull() && arg.selection_count < arg.selection.Length)
										{
											//App.WriteLine(behavior.idle_timer);
											if (faction.id == arg.faction_id && !commandable.flags.HasAny(Commandable.Data.Flags.No_Select) && (behavior.idle_timer >= 2.00f || !behavior.ref_target_body.entity.IsValid() || behavior.type == AI.Behavior.Type.None || movement.type == AI.Movement.Type.None))
											{
												//if (Vector2.DistanceSquared(transform.position, arg.target_position) <= (128 * 128))
												//{
												//	if (arg.wave_size_rem > 0)
												//	{
												//		arg.wave_size_rem--;
												//	}
												//	else
												//	{
												//		return;
												//	}
												//}

												//ref var region = ref info.GetRegion();
												arg.selection[arg.selection_count++].Set(entity);
												//App.WriteLine(entity);
											}
										}
									}

									//planner.wave_size_rem = arg.wave_size_rem;

									if (arg.selection_count > 0)
									{
										selection.units = arg.selection;

										selection.order_type = Commandable.OrderType.Attack;

										control.mouse.position = target_position;
										control.mouse.SetKeyPressed(Mouse.Key.Right, true);
									}
								}
								else
								{
									planner.ref_target.Set(default);
								}
							}

							if (planner.wave_size_rem > 0 && time >= planner.next_spawn)
							{
								planner.next_spawn = time + random.NextFloatRange(4.00f, 8.00f);

								//var total_count = region.GetTotalTagCount("kobold", "dead");
								var total_count = region.GetTotalTagCount("kobold");
								if (total_count < g_siege.max_npc_count)
								{
									var target_position = transform.position;

									if (planner.ref_target.IsAlive() && planner.ref_target.TryGetHandle(out var h_target_transform))
									{
										target_position = h_target_transform.data.position;
									}

									if (TryFindNearestSpawn(ref region, faction.id, target_position, out var ent_spawn, out var pos_spawn))
									{
										var weapon_mult = 1.00f;
										var armor_mult = 1.00f;

										armor_mult = g_siege_state.difficulty * 0.02f;

										var group_size_tmp = 1 + random.NextIntRange(0, 2);
										for (int i = 0; i < group_size_tmp && total_count + i < g_siege.max_npc_count; i++)
										{
											var h_character = Dormitory.CreateCharacter(ref region, ref random, "kobold.gunner");
											Dormitory.SpawnCharacter(ref region, h_character, pos_spawn + new Vector2(random.NextFloatRange(-5, 5), 0.00f), h_faction: faction.id).ContinueWith((ent) =>
											{
												SetKoboldLoadout(ent, weapon_mult: weapon_mult, armor_mult: armor_mult);
											});


											//var ent_spawner = entity;
											//region.SpawnPrefab("kobold.male", pos_spawn + new Vector2(random.NextFloatRange(-5, 5), 0.00f), faction_id: faction.id).ContinueWith((ent) =>
											//{
											//	SetKoboldLoadout(ent, weapon_mult: weapon_mult, armor_mult: armor_mult);
											//});

											planner.wave_size_rem = Math.Max(planner.wave_size_rem - 1, 0);
										}

										planner.next_dispatch = time + 0.50f;
									}
									else
									{
										App.WriteLine("Failed to find an NPC spawn point!");
									}
								}

								//App.WriteLine($"Spawning reinforcements... ({planner.wave_size_rem} left)");
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
#endif
	}
}
