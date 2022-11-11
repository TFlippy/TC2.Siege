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


		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct WaveDesigner: IComponent
		{
			public FixedArray32<IUnit.Handle> units = new FixedArray32<IUnit.Handle>();

			public WaveDesigner()
			{

			}
		}

		public partial struct AddUnitRPC: Net.IRPC<Siege.WaveDesigner>
		{
			public IUnit.Handle unit;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Siege.WaveDesigner wave)
			{
				for (int i = 0; i < wave.units.Length; i++)
				{
					if (wave.units[i].IsNull())
					{
						wave.units[i] = this.unit;
					}
				}
				wave.Sync(entity);
			}
#endif
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct ControllerData: IComponent
		{
			public int money;

			public ControllerData(int money)
			{
				this.money = money;
			}
		}

		public partial struct ChangeMoney: Net.IRPC<Siege.ControllerData>
		{
			public int change;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Siege.ControllerData data)
			{
				data.money += this.change;
				data.Sync(entity);
			}
#endif
		}

#if CLIENT
		public partial struct SiegeAttackerGUI: IGUICommand
		{
			public Siege.Gamemode g_siege;
			public Siege.Gamemode.State g_siege_state;
			public Entity entity;
			public static List<uint> unit_indices = new List<uint>(64);
			public static float scale = 2;

			public void Draw()
			{

				using (var widget = Sidebar.Widget.New("attacker_build", "Recruit", "ui_icon_build", new Vector2(300, 600), lockable: false))
				{
					if (widget.show)
					{
						GUI.Title("Purchase units");
						using (GUI.Scrollbox.New("attacker_unitshop", GUI.GetAvailableSize()))
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
									if (recipe.IsNotNull())
									{
										var frame_size = recipe.icon.GetFrameSize(scale);
										frame_size += new Vector2(8, 8);
										frame_size = frame_size.ScaleToNearestMultiple(new Vector2(48, 48));

										grid.Push(frame_size);
										using (var button = GUI.CustomButton.New(recipe.name, frame_size, sound: GUI.sound_select, sound_volume: 0.10f))
										{
											GUI.Draw9Slice(GUI.tex_slot_simple, new Vector4(4), button.bb);
											GUI.DrawSpriteCentered(recipe.icon, button.bb, scale: scale);

											if (button.pressed)
											{
												var rpc = new Siege.AddUnitRPC
												{
													unit = new IUnit.Handle(pair),
												};
												rpc.Send(this.entity);

												var rpc1 = new Siege.ChangeMoney
												{
													change = -recipe.price,
												};
												rpc1.Send(this.entity);
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
													GUI.DrawMoney(recipe.price, new Vector2(8, 8));
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

		[ISystem.EarlyGUI(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Owned)]
		public static void OnGUIAttacker(Entity entity, [Source.Owned] in Player.Data player, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state)
		{
			if (player.IsLocal() && player.faction_id == g_siege_state.faction_attackers)
			{
				var gui = new SiegeAttackerGUI()
				{
					g_siege = g_siege,
					g_siege_state = g_siege_state,
					entity = entity,
				};
				gui.Submit();
			}
		}
#endif
	}
}
