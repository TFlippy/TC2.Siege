using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{

#if CLIENT
		public partial struct SiegeDefenderGUI: IGUICommand
		{
			public Siege.Gamemode g_siege;
			public Siege.Gamemode.State g_siege_state;
			public Siege.Bounty.Global g_bounty;

			public void Draw()
			{
				var window_pos = (GUI.CanvasSize * new Vector2(0.50f, 0.00f)) + new Vector2(0, 64 + 4);
				using (var window = GUI.Window.Standalone("Siege", position: window_pos, size: new Vector2(400, 52), pivot: new Vector2(0.50f, 0.00f), padding: new(4)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						//GUI.DrawWindowBackground();

						ref var region = ref Client.GetRegion();
						ref var world = ref Client.GetWorld();
						ref var game_info = ref Client.GetGameInfo();

						var time_left = MathF.Max(this.g_siege_state.t_next_wave - this.g_siege_state.t_match_elapsed, 0.00f);

						using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() - 140, GUI.GetRemainingHeight())))
						{
							if (this.g_siege_state.status == Gamemode.Status.Running && this.g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Active))
							{
								GUI.Title($"Incoming in {(time_left):0} s", size: 22, color: time_left > 10.00f ? GUI.font_color_title : GUI.font_color_yellow);

								if (this.g_bounty.t_next_payout != 0.00f)
								{
									var rewards = this.g_bounty.rewards.AsSpan();
									if (rewards.HasAny())
									{
										var multiplier = Maths.Lerp(1.00f, 1.00f / (float)g_siege_state.player_count, g_siege.reward_share_ratio) * g_siege.reward_mult;

										foreach (ref var reward in rewards)
										{
											if (reward.type == Crafting.Product.Type.Money)
											{
												var amount = Money.ToBataPrice(reward.amount * multiplier);
												GUI.Title($"Payout of {amount:0.00} coins in {(MathF.Max(this.g_bounty.t_next_payout - this.g_siege_state.t_match_elapsed, 0.00f)):0}s", font: GUI.Font.Monaco, size: 14, color: GUI.font_color_green);

												break;
											}
										}
										//GUI.Title($"+{this.g_bounty.} {(MathF.Max(this.g_bounty.t_next_update - this.g_siege_state.t_match_elapsed, 0.00f)):0} s", size: 22);
									}
								}
								//GUI.Title($"Difficulty: {this.g_siege_state.difficulty:0.0}", size: 22);
								//Shop.DrawProducts(ref region, default, default, default, this.g_bounty.rewards.AsSpan(), 1);
							}
							else
							{
								GUI.TitleCentered(this.g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Paused) ? "Paused" : $"{this.g_siege_state.status}", size: 32, pivot: new(0.00f, 0.50f), color: GUI.font_color_yellow);
							}
						}

						GUI.SameLine();

						using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight())))
						{
							GUI.TitleCentered($"Wave: {this.g_siege_state.wave_current}", size: 32, pivot: new(1.00f, 0.00f));
							GUI.TitleCentered($"Hazard: {this.g_siege_state.difficulty:0.0}", size: 20, pivot: new(1.00f, 1.00f));
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Owned)]
		public static void OnGUIDefender(Entity entity, [Source.Owned] in Player.Data player, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state, [Source.Global] in Siege.Bounty.Global g_bounty)
		{
			if (player.IsLocal() && player.faction_id == g_siege_state.faction_defenders)
			{
				var gui = new SiegeDefenderGUI()
				{
					g_siege = g_siege,
					g_siege_state = g_siege_state,
					g_bounty = g_bounty
				};
				gui.Submit();
			}
		}
#endif



#if CLIENT
		public partial struct SiegeAttackerGUI: IGUICommand
		{
			public Siege.Gamemode g_siege;
			public Siege.Gamemode.State g_siege_state;
			public Entity entity;
			public static List<uint> unit_indices = new List<uint>(64);
			public static float scale = 4;
			public Region.Data region;
			public Player.Data player;

			public void Draw()
			{

				using (var widget = Sidebar.Widget.New("attacker_build", "Recruit", "Rifleman_Armor", new Vector2(440, 600), lockable: false))
				{
					if (widget.show)
					{
						var time_left = MathF.Max(this.g_siege_state.t_next_wave - this.g_siege_state.t_match_elapsed, 0.00f);

						GUI.Title("Purchase units");
						GUI.NewLine(8);
						GUI.DrawMoney(GetMoney(ref region), new Vector2(64, 16));
						GUI.SameLine(8);
						GUI.TitleCentered($"Next wave {(time_left):0} s", size: 16, color: time_left > 10.00f ? GUI.font_color_title : GUI.font_color_yellow);
						GUI.NewLine(26);
						GUI.Title("Catalog");
						GUI.SameLine(170);
						GUI.Title("Queued");
						GUI.NewLine();
						using (GUI.Scrollbox.New("attacker_unitshop", new Vector2(GUI.GetAvailableWidth() / 2, GUI.GetAvailableHeight())))
						{
							using (var grid = GUI.Grid.New(size: GUI.GetRemainingSpace()))
							{
								unit_indices.Clear();
								var recipes = IUnit.Database.GetAssets();
								foreach (var d_recipe in recipes)
								{
									ref var recipe = ref d_recipe.GetData();
									if (recipe.IsNotNull())
									{
										unit_indices.Add(d_recipe.id);
									}
								}

								foreach (var pair in unit_indices)
								{
									//GUI.Text($"{pair.rank}");

									ref var recipe = ref IUnit.Database.GetData(pair);
									var unit = new IUnit.Handle(pair);

									if (new IUnit.Handle(pair).id != 0)
									{
										var frame_size = recipe.icon.GetFrameSize(scale);
										frame_size += new Vector2(8, 8);
										frame_size = frame_size.ScaleToNearestMultiple(new Vector2(48, 48));

										grid.Push(frame_size);
										using (var button = GUI.CustomButton.New(recipe.name, frame_size, sound: GUI.sound_select, sound_volume: 0.10f))
										{
											GUI.Draw9Slice(GUI.tex_slot_simple, new Vector4(4), button.bb);
											GUI.DrawSpriteCentered(recipe.icon, button.bb, scale: scale);
											
											if (button.pressed && GetMoney(ref region) >= recipe.price)
											{
												PurchaseUnit(ref region, unit);
											}
										}
										if (GUI.IsItemHovered())
										{
											using (GUI.Tooltip.New())
											{
												using (GUI.Wrap.Push(325))
												{
													GUI.Title(recipe.name);
													GUI.Text(recipe.desc, color: GUI.font_color_default);
													GUI.NewLine(10);
													GUI.DrawMoney(recipe.price, new Vector2(32, 8));
													GUI.NewLine(10);
													GUI.Text("Inventory");
													foreach (var item in recipe.items)
													{
														var itemIcon = item.GetPrefab().GetIcon();
														GUI.DrawSprite(itemIcon);
														GUI.SameLine();
													}
													GUI.NewLine();
													foreach (var item in recipe.equipment)
													{
														var itemIcon = item.GetPrefab().GetIcon();
														GUI.DrawSprite(itemIcon);
														GUI.SameLine();
													}
													GUI.NewLine();
													foreach (var item in recipe.resource)
													{
														var itemIcon = IMaterial.Database.GetData(item.material).icon;
														GUI.DrawSprite(itemIcon);
														GUI.SameLine();
													}

												}
											}
										}
									}
								}
							}
						}
						GUI.SameLine();
						using (GUI.Scrollbox.New("attacker_wavelist", GUI.GetAvailableSize()))
						{
							FixedArray32<IUnit.Handle> units = GetUnits(ref region);
							using (var grid = GUI.Grid.New(size: GUI.GetRemainingSpace()))
							{
								for (int i = 0; i < units.Length; i++)
								{
									if (units[i].id != 0)
									{
										IUnit.Data unit = units[i].GetData();
										var frame_size = unit.icon.GetFrameSize(scale);
										frame_size += new Vector2(8, 8);
										frame_size = frame_size.ScaleToNearestMultiple(new Vector2(48, 48));
										grid.Push(frame_size);

										using (var button = GUI.CustomButton.New(unit.name + i, frame_size, sound: GUI.sound_select, sound_volume: 0.10f))
										{
											GUI.Draw9Slice(GUI.tex_slot_simple, new Vector4(4), button.bb);
											GUI.DrawSpriteCentered(unit.icon, button.bb, scale: scale);

											if (button.pressed)
											{
												RemoveUnit(ref region, i);
											}

										}
										if (GUI.IsItemHovered())
										{
											using (GUI.Tooltip.New())
											{
												using (GUI.Wrap.Push(325))
												{
													GUI.Title("Remove: " + unit.name);
													GUI.Text(unit.desc, color: GUI.font_color_default);
													GUI.DrawMoney(unit.price, new Vector2(8, 8));
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		[Query]
		public delegate void GetPlannnerQuery(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Planner planner);

		private struct PurchaseUnitArgs
		{
			public IUnit.Handle unit_id;

			public PurchaseUnitArgs(IAsset2<IUnit, IUnit.Data>.Handle unit_id)
			{
				this.unit_id = unit_id;
			}
		}

		public static void PurchaseUnit(ref Region.Data region, IUnit.Handle unit)
		{
			var arg = new PurchaseUnitArgs(unit);
			region.Query<GetPlannnerQuery>(Func).Execute(ref arg);
			static void Func(ISystem.Info info, Entity ent_planner, [Source.Owned] ref Siege.Planner planner)
			{
				ref var arg = ref info.GetParameter<PurchaseUnitArgs>();
				if (!arg.IsNull())
				{
					var rpc = new Siege.BuyUnitRPC
					{
						unit = arg.unit_id,
					};
					rpc.Send(ent_planner);
				}
			}
		}

		[Query]
		public delegate void GetPlannnerUnitsQuery(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Planner planner);

		private struct GetUnitArgs
		{
			public FixedArray32<IUnit.Handle> units = new FixedArray32<IAsset2<IUnit, IUnit.Data>.Handle>();

			public GetUnitArgs()
			{
			}
		}

		public static FixedArray32<IUnit.Handle> GetUnits(ref Region.Data region)
		{
			var arg = new GetUnitArgs()
			{
				units = new FixedArray32<IUnit.Handle>()
			};
			region.Query<GetPlannnerUnitsQuery>(Func).Execute(ref arg);
			static void Func(ISystem.Info info, Entity ent_planner, [Source.Owned] ref Siege.Planner planner)
			{
				ref var arg = ref info.GetParameter<GetUnitArgs>();
				if (!arg.IsNull())
				{
					arg.units = planner.orderedUnits;
				}
			}
			return arg.units;
		}

		[Query]
		public delegate void GetPlannnerMoneyQuery(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Planner planner);

		private struct GetMoneyArgs
		{
			public int money = 0;

			public GetMoneyArgs()
			{
			}
		}

		public static int GetMoney(ref Region.Data region)
		{
			var arg = new GetMoneyArgs();
			region.Query<GetPlannnerMoneyQuery>(Func).Execute(ref arg);
			static void Func(ISystem.Info info, Entity ent_planner, [Source.Owned] ref Siege.Planner planner)
			{
				ref var arg = ref info.GetParameter<GetMoneyArgs>();
				if (!arg.IsNull())
				{
					arg.money = planner.money;
				}
			}
			return arg.money;
		}

		[Query]
		public delegate void RemovePlannnerUnitQuery(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Planner planner);

		private struct RemoveUnitArgs
		{
			public int index;

			public RemoveUnitArgs(int index)
			{
				this.index = index;
			}
		}

		public static void RemoveUnit(ref Region.Data region, int index)
		{
			var arg = new RemoveUnitArgs(index);
			region.Query<RemovePlannnerUnitQuery>(Func).Execute(ref arg);
			static void Func(ISystem.Info info, Entity ent_planner, [Source.Owned] ref Siege.Planner planner)
			{
				ref var arg = ref info.GetParameter<RemoveUnitArgs>();
				if (!arg.IsNull())
				{
					var rpc = new RemoveUnitRPC
					{
						index = arg.index,
					};
					rpc.Send(ent_planner);
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Owned)]
		public static void OnGUIAttacker(Entity entity, ISystem.Info info, [Source.Owned] in Player.Data player, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state)
		{
			if (player.IsLocal() && player.faction_id == g_siege_state.faction_attackers)
			{
				var gui = new SiegeAttackerGUI()
				{
					g_siege = g_siege,
					g_siege_state = g_siege_state,
					entity = entity,
					player = player,
					region = info.GetRegion(),
				};
				gui.Submit();
			}
		}
#endif
	}
}
