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
						items_span.Add(Shipment.Item.Prefab("sledgehammer", flags: Shipment.Item.Flags.Pickup));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("drill", flags: Shipment.Item.Flags.Pickup));
					}
					else if (random.NextBool(0.50f))
					{
						items_span.Add(Shipment.Item.Prefab("chainsaw", flags: Shipment.Item.Flags.Pickup));
					}
					else
					{
						items_span.Add(Shipment.Item.Prefab("shield", flags: Shipment.Item.Flags.Pickup));
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
						items_span.Add(Shipment.Item.Prefab("helmet.01", flags: Shipment.Item.Flags.Equip));
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

			foreach (var h_inventory in data.ent_target.GetInventories())
			{
				h_inventory.Flags |= Inventory.Flags.Unlimited | Inventory.Flags.No_Drop;
			}

			ref var marker = ref data.ent_target.GetOrAddComponent<Minimap.Marker.Data>(sync: true);
			if (!marker.IsNull())
			{
				marker.sprite = new Sprite("ui_icons_minimap", 16, 16, 0, 0);
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
	}
}
