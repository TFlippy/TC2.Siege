using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		[Query]
		public delegate void GetAllTargetsQuery(ISystem.Info info, Entity entity, [Source.Owned] in Siege.Target target, [Source.Owned] in Transform.Data transform);

		[IGamemode.Data("Siege", "")]
		public partial struct Gamemode : IGamemode
		{
			[Flags]
			public enum Flags : uint
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
				Constants.World.save_characters = true;

				Constants.World.load_factions = false;
				Constants.World.load_players = true;
				Constants.World.load_characters = true;

				Constants.World.enable_autosave = true;

				Constants.Respawn.token_count_min = 1.00f;
				Constants.Respawn.token_count_max = 20.00f;
				Constants.Respawn.token_count_default = 3.00f;
				Constants.Respawn.token_refill_amount = 0.05f;

				Constants.Respawn.respawn_cooldown_base = 5.00f;
				Constants.Respawn.respawn_cooldown_token_modifier = 0.00f;

				Constants.Characters.allow_custom_characters = true;
				Constants.Characters.allow_switching = true;
			}

			public static void Init()
			{
				App.WriteLine("Siege Init!", App.Color.Magenta);
			}

#if SERVER
			[ISystem.AddFirst(ISystem.Mode.Single)]
			public static void OnAdd(ISystem.Info info, [Source.Owned] ref Siege.Gamemode gamemode)
			{
				ref var region = ref info.GetRegion();

				ref var faction_1 = ref Faction.Create(ref region, 1, "Defenders", 0xff0000ff, 0xff0000ff);
				ref var faction_2 = ref Faction.Create(ref region, 2, "Attackers", 0xffff0000, 0xffff0000);
			}

			//[ISystem.AddFirst(ISystem.Mode.Single)]
			//public static void OnSpawn(ISystem.Info info, [Source.Owned] ref Kobold.Data kobold, [Source.Owned] ref AI.Behavior behavior)
			//{
			//	behavior.flags.SetFlag(AI.Behavior.Flags.Aggressive, true);
			//}
#endif
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Target : IComponent
		{
			public byte faction_id;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Planner : IComponent
		{
			[Flags]
			public enum Flags : uint
			{
				None = 0,

				Ready = 1 << 0
			}

			public Siege.Planner.Flags flags;

			[Save.Ignore, Net.Ignore] public float next_update;
		}

#if SERVER
		[ISystem.Event<Spawner.SpawnEvent>(ISystem.Mode.Single)]
		public static void OnSpawn(ISystem.Info info, Entity entity, ref Spawner.SpawnEvent data,
		[Source.Owned] ref Spawner.Data spawner, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Siege.Planner planner, [Source.Owned] ref Selection.Data selection)
		{
			App.WriteLine($"spawn event {data.ent_target}");

			var loadout = new Loadout.Data();

			ref var shipment = ref loadout.shipments[0];
			shipment.flags.SetFlag(Shipment.Flags.Unpack, true);

			var items = shipment.items.AsSpan();
			items.Add(Shipment.Item.Prefab("crowbar", flags: Shipment.Item.Flags.Pickup));

			//items.Add(Shipment.Item.Prefab("blunderbuss", flags: Shipment.Item.Flags.Pickup));
			//items.Add(Shipment.Item.Resource("ammo_musket.shot", 32));

			ref var loadout_new = ref data.ent_target.GetOrAddComponent<Loadout.Data>(sync: false, ignore_mask: true);
			if (!loadout_new.IsNull())
			{
				loadout_new = loadout;
			}

			for (int i = 0; i < selection.units.Length; i++)
			{
				ref var unit = ref selection.units[i];
				if (!unit.IsAlive())
				{
					unit.Set(data.ent_target);

					if (i == selection.units.Length - 1)
					{
						planner.flags.SetFlag(Siege.Planner.Flags.Ready, true);
					}

					break;
				}
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Spawner.Data spawner,
		[Source.Owned] ref Control.Data control, [Source.Owned] ref Selection.Data selection, [Source.Owned] ref Siege.Planner planner)
		{
			if (info.WorldTime > planner.next_update)
			{
				planner.next_update = info.WorldTime + 1.00f;

				ref var region = ref info.GetRegion();

				//App.WriteLine(info.GetRegion().GetTotalTagCount("kobold"));

				if (planner.flags.HasAll(Siege.Planner.Flags.Ready))
				{
					App.WriteLine("raid ready");
					var arg = (ent_search: entity, faction_id: (byte)2, position: transform.position, ent_root: default(Entity), ent_target: default(Entity), target_dist_nearest_sq: float.MaxValue, target_position: default(Vector2));

					region.Query<Siege.GetAllTargetsQuery>(Func).Execute(ref arg);
					static void Func(ISystem.Info info, Entity entity, in Siege.Target target, in Transform.Data transform)
					{
						ref var arg = ref info.GetParameter<(Entity ent_search, byte faction_id, Vector2 position, Entity ent_root, Entity ent_target, float target_dist_nearest_sq, Vector2 target_position)>();
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
					}
				}
			}
		}
#endif
	}
}
