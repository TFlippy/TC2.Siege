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

			public static readonly Texture.Handle tex_icons_minimap = "ui_icons_minimap";
			public static Entity? ent_selected_spawn_new;

			public Entity ent_respawn;
			public IFaction.Handle faction_id;
			public Respawn.Data respawn;

			public static ICharacter.Handle h_selected_character;

			public void Draw()
			{
				ref var player = ref Client.GetPlayer();
				ref var region = ref Client.GetRegion();

				var window_pos = (GUI.CanvasSize * new Vector2(0.50f, 0.00f)) + new Vector2(0, 64 + 4);
				using (var window = GUI.Window.Standalone("Siege", position: window_pos, size: new Vector2(400, 52), pivot: new Vector2(0.50f, 0.00f), padding: new(4)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						//GUI.DrawWindowBackground();

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

				//var rem_height = MathF.Max(GUI.CanvasSize.Y - total_height, 0.00f);

				//var rem_height = GUI.CanvasSize.Y - RespawnGUI.window_offset.Y

				//RespawnGUI.window_size.Y = Maths.Clamp(RespawnGUI.window_size.Y, 0, GUI.CanvasSize.Y - RespawnGUI.window_offset.Y - 40);

				if (!player.flags.HasAll(Player.Flags.Alive) && !(player.flags.HasAll(Player.Flags.Editor) && !Editor.show_respawn_menu))
				{
					var max_height = GUI.CanvasSize.Y - Spawn.RespawnGUI.window_offset.Y - 12;

					Spawn.RespawnGUI.window_size.Y = Maths.Clamp(Spawn.RespawnGUI.window_size.Y, 0, max_height);

					Spawn.RespawnGUI.ent_selected_spawn = this.respawn.ent_selected_spawn;

					using (var window = GUI.Window.Standalone("Respawn", position: new Vector2(GUI.CanvasSize.X * 0.50f, 0) + Spawn.RespawnGUI.window_offset, pivot: Spawn.RespawnGUI.window_pivot, size: Spawn.RespawnGUI.window_size))
					{
						Spawn.RespawnGUI.window_size = new Vector2(800, 700);

						this.StoreCurrentWindowTypeID();
						if (window.show)
						{
							GUI.DrawWindowBackground(GUI.tex_window_menu, padding: new Vector4(8, 8, 8, 8));

							using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight()), padding: new(8, 8)))
							{
								if (!Spawn.RespawnGUI.ent_selected_spawn.IsAlive())
								{
									region.Query<Region.GetSpawnsQuery>(Func).Execute(ref this);
									static void Func(ISystem.Info info, Entity entity, in Spawn.Data spawn, in Nameable.Data nameable, in Transform.Data transform, in Faction.Data faction)
									{
										ref var player = ref Client.GetPlayer();
										if (faction.id == 0 || (faction.id == player.faction_id))
										{
											var random = XorRandom.New();

											if (Spawn.RespawnGUI.ent_selected_spawn.id == 0 || random.NextBool(0.30f)) ent_selected_spawn_new = entity;
										}
									}
								}

								{
									ref var info = ref region.GetMapInfo();

									using (GUI.Wrap.Push(GUI.GetRemainingWidth()))
									{
										//using (GUI.Group.New(size: new(GUI.GetRemainingWidth(), 0), padding: new(4)))
										//{
										//	using (GUI.Wrap.Push(GUI.GetRemainingWidth()))
										//	{
										//		if (!info.name.IsEmpty()) GUI.Title(info.name, size: 32);
										//		if (!info.desc.IsEmpty()) GUI.Text(info.desc);
										//	}
										//}

										//var ts = Timestamp.Now();
										using (var group = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 0), padding: new(4)))
										{
											//ref var minimap = ref Minimap.MinimapHUD.minimaps[region.GetID()];
											//if (minimap != null)
											//{
											//	var map_frame_size = minimap.GetFrameSize(2);
											//	map_frame_size = map_frame_size.ScaleToSize(new Vector2(GUI.GetRemainingWidth(), 80));

											//	Minimap.DrawMap(ref region, minimap, map_frame_size, map_scale: 1.00f);
											//}

											ref var minimap = ref Minimap.MinimapHUD.minimaps[region.GetID()];
											if (minimap != null)
											{
												var map_frame_size = minimap.GetFrameSize(2);
												map_frame_size = map_frame_size.ScaleToSize(new Vector2(GUI.GetRemainingWidth(), 80));

												using (var map = GUI.Map.New(ref region, minimap, size: map_frame_size, map_scale: 1.00f, draw_markers: false))
												{
													foreach (ref var row in region.IterateQuery<Region.GetSpawnsQuery>())
													{
														var selected = row.Entity == Spawn.RespawnGUI.ent_selected_spawn;

														var transform_copy = default(Transform.Data);
														var nameable_copy = default(Nameable.Data);
														var color = selected ? Color32BGRA.White : new Color32BGRA(0xff9a7f7f);

														row.Run((ISystem.Info info, Entity entity, in Spawn.Data spawn, in Nameable.Data nameable, in Transform.Data transform, in Faction.Data faction) =>
														{
															transform_copy = transform;
															nameable_copy = nameable;

															if (faction.id.TryGetData(out var ref_faction))
															{
																color = ref_faction.value.color_a;
																//sprite.frame.X = 1;
															}
														});

														using (var node = map.DrawNode(new Sprite(tex_icons_minimap, 16, 16, 3, 0), transform_copy.GetInterpolatedPosition() + new Vector2(0, -3), color: color, color_hovered: Color32BGRA.White))
														{
															//GUI.DrawTextCentered(nameable_copy.name, node.rect.GetPosition() + new Vector2(16, 0), piv layer: GUI.Layer.Window, font: GUI.Font.Superstar, size: 16);

															if (node.is_hovered)
															{
																GUI.SetCursor(App.CursorType.Hand, 100);

																if (GUI.GetMouse().GetKeyDown(Mouse.Key.Left))
																{
																	//App.WriteLine("press");
																	ent_selected_spawn_new = row.Entity;
																}

																using (GUI.Tooltip.New())
																{
																	GUI.Title(nameable_copy.name, font: GUI.Font.Superstar, size: 16);
																}
															}
														}
													}
												}
											}
										}
									}
								}

								GUI.SeparatorThick();

								this.DrawSpawns(ref region, size: new(GUI.GetRemainingWidth(), (24 * 4.50f) + 8));

								GUI.SeparatorThick();

								//var h_character = default(ICharacter.Handle);

								using (GUI.Group.New(size: GUI.GetRemainingSpace() with { X = 400 }, padding: new(0, 0)))
								{
									using (var scrollable = GUI.Scrollbox.New("Platoon", size: GUI.GetRemainingSpace(), padding: new(4, 4), force_scrollbar: true))
									{
										ref var platoon = ref player.ent_player.GetComponent<Siege.Platoon.Data>();
										if (platoon.IsNotNull())
										{
											foreach (ref var h_character in platoon.characters)
											{
												ref var character_data = ref h_character.GetData();
												if (character_data.IsNotNull())
												{
													using (GUI.ID.Push(h_character.id))
													{
														using (var group_row = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 40)))
														{
															Dormitory.DormitoryGUI.DrawCharacterSmall(h_character);

															var selected = h_selected_character == h_character;
															if (GUI.Selectable3("select", group_row.GetOuterRect(), selected: selected))
															{
																h_selected_character = selected ? default : h_character;
															}
														}
													}
												}
											}
										}
									}
								}

								GUI.SameLine();

								using (GUI.Group.New(size: GUI.GetRemainingSpace(), padding: new(8, 8)))
								{

								}

								//using (var group_row = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 80)))
								//{
								//	//Dormitory.DormitoryGUI.DrawCharacterSmall()

								//	//RespawnExt.DrawCharacter(ref spawn_info);
								//}

								//using (var scrollable = GUI.Scrollbox.New("Platoon", size: GUI.GetRemainingSpace(y: -48), padding: new(4, 4), force_scrollbar: true))
								//{
								//	ref var platoon = ref player.ent_player.GetComponent<Siege.Platoon.Data>();
								//	if (platoon.IsNotNull())
								//	{
								//		foreach (ref var h_character in platoon.characters)
								//		{
								//			ref var character_data = ref h_character.GetData();
								//			if (character_data.IsNotNull())
								//			{
								//				using (GUI.ID.Push(h_character.id))
								//				{
								//					using (var group_row = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 40)))
								//					{
								//						Dormitory.DormitoryGUI.DrawCharacterSmall(h_character);

								//						if (GUI.Selectable3("select", group_row.GetOuterRect(), false))
								//						{

								//						}
								//					}
								//				}
								//			}
								//		}
								//	}
								//}

								//using (var scrollbox = GUI.Scrollbox.New("levels", size: new(GUI.GetRemainingWidth(), GUI.GetRemainingHeight() - 48), padding: new(0)))
								//{
								//	//GUI.Title("Experience", size: 20);

								//	var h_character = spawn_info.character;
								//	ref var character = ref h_character.GetData();
								//	if (character.IsNotNull())
								//	{
								//		Experience.DrawTableSmall2(ref character.experience);
								//	}
								//}

								//GUI.SeparatorThick();


								//ref var character_data = ref h_character.GetData();
								//if (character_data.IsNotNull())
								//{
								//	ref var origin_data = ref character_data.origin.GetData();
								//	if (origin_data.IsNotNull())
								//	{
								//		Experience.DrawTableSmall(ref origin_data.experience);
								//	}
								//}

								//if (Spawn.RespawnGUI.ent_selected_spawn.IsAlive())
								//{
								//	ref var faction = ref Spawn.RespawnGUI.ent_selected_spawn.GetComponent<Faction.Data>();
								//	if (faction.IsNull() || faction.id == player.faction_id)
								//	{
								//		ref var spawn = ref Spawn.RespawnGUI.ent_selected_spawn.GetComponent<Spawn.Data>();
								//		if (!spawn.IsNull())
								//		{
								//			if (GUI.DrawButton((this.respawn.cooldown > 0.00f ? $"Respawn ({MathF.Floor(this.respawn.cooldown):0}s)" : "Respawn"), new Vector2(168, 48), enabled: this.respawn.cooldown <= float.Epsilon && h_selected_character.id != 0, font_size: 24, color: GUI.font_color_green_b))
								//			{
								//				var rpc = new RespawnExt.SpawnRPC
								//				{
								//					ent_spawn = Spawn.RespawnGUI.ent_selected_spawn
								//				};
								//				rpc.Send(player.ent_player);
								//			}
								//			if (GUI.IsItemHovered())
								//			{
								//				using (GUI.Tooltip.New())
								//				{
								//					GUI.Text("Respawn as this character at the selected spawn point.");
								//				}
								//			}
								//		}
								//		else
								//		{

								//		}
								//	}
								//	else
								//	{
								//		Spawn.RespawnGUI.ent_selected_spawn = default;
								//	}
								//}

								if (ent_selected_spawn_new.HasValue && ent_selected_spawn_new != Spawn.RespawnGUI.ent_selected_spawn)
								{
									var rpc = new RespawnExt.SetSpawnRPC()
									{
										ent_spawn = ent_selected_spawn_new.Value
									};
									rpc.Send(player.ent_player);
									ent_selected_spawn_new = default;
								}
							}
						}
					}
				}
			}

			private void DrawSpawns(ref Region.Data region, Vector2 size)
			{
				//GUI.Title("Spawns", size: 32);
				//GUI.SeparatorThick();

				using (var scrollable = GUI.Scrollbox.New("Spawns", size: size, padding: new(4, 4), force_scrollbar: true))
				{
					using (var table = GUI.Table.New("Spawns.Table", 2, new(GUI.GetRemainingWidth(), 0)))
					{
						if (table.show)
						{
							table.SetupColumnFixed(24);
							table.SetupColumnFlex(1);
							//table.SetupColumnFixed(48);


							static bool DrawSpawnsRow(Entity entity, in Spawn.Data spawn, in Nameable.Data nameable, in Faction.Data faction)
							{
								var pressed = false;

								using (var row = GUI.Table.Row.New(new Vector2(GUI.GetRemainingWidth(), 24)))
								{
									using (GUI.ID.Push(entity))
									{
										var spawn_name = nameable.name;
										if (spawn_name.IsEmpty()) spawn_name = "Unknown";

										var color = GUI.font_color_default;
										if (faction.id.TryGetData(out var ref_faction))
										{
											color = ref_faction.value.color_a;
										}

										//var text = ZString.Format("{0} {1}", spawn_name, (faction.ent_faction != 0 ? $"({faction.name})" : ""));

										using (row.Column(0, padding: new(4, 0)))
										{
											GUI.TitleCentered($"{spawn.respawn_counter}", size: 16, color: Color32BGRA.Lerp(GUI.font_color_default_dark, GUI.font_color_default, Maths.Clamp01(spawn.respawn_counter * 0.01f)), pivot: new(0.50f, 0.50f));
										}
										GUI.DrawHoverTooltip("Popularity");

										using (row.Column(1, padding: new(4, 0)))
										{
											//if (faction.id != 0)
											//{
											//	GUI.TitleCentered($"[{faction.tag}]", color: faction.color_a, pivot: new(0.00f, 0.50f));
											//	GUI.ResetLine(32);
											//}
											GUI.TitleCentered(spawn_name, size: 24, color: faction.id != 0 ? color : GUI.font_color_default, pivot: new(0.00f, 0.50f));

											if (entity.TryGetPrefab(out var prefab))
											{
												var prefab_name = (Utf8String)prefab.GetName();

												//var text_size = default(Vector2);
												//ImGuiNative.igCalcTextSize2(prefab_name, null, 1, 0, 16, GUI.Font.Superstar.ptr, &text_size);

												//GUI.OffsetLine(GUI.GetRemainingWidth() - text_size.X);
												GUI.TitleCentered(prefab_name, color: faction.id != 0 ? color.WithColorMult(0.50f) : GUI.font_color_default.WithColorMult(0.50f), pivot: new(1.00f, 0.50f));
												////GUI.TextShadedCentered(prefab_name, pivot: new(1.00f, 0.50f), size: 16);
											}
										}

										//using (row.Column(1))
										//{

										//}

										GUI.SameLine();
										if (GUI.Selectable("", Spawn.RespawnGUI.ent_selected_spawn.id == entity.id, size: GUI.GetRemainingSpace(), same_line: false))
										{
											SiegeDefenderGUI.ent_selected_spawn_new = entity;
											pressed = true;
										}
									}
								}

								return pressed;
							}

							//App.WriteLine(faction.id);
							if (this.faction_id != 0)
							{
								region.Query<Region.GetSpawnsQuery>(FuncA).Execute(ref this);
								static void FuncA(ISystem.Info info, Entity entity, in Spawn.Data spawn, in Nameable.Data nameable, in Transform.Data transform, in Faction.Data faction)
								{
									ref var data = ref info.GetParameter<SiegeDefenderGUI>();
									if (!data.IsNull())
									{
										if (faction.id != 0 && faction.id == data.faction_id)
										{
											//GUI.DrawBackground(GUI.tex_panel_white, GUI.GetRemainingRect(), new(4), faction.color_a);

											DrawSpawnsRow(entity, in spawn, in nameable, in faction);
										}
									}
								}

								//GUI.NewLine(4);
								//GUI.Separator(faction.color_a.WithAlphaMult(0.50f));
								//GUI.NewLine(4);
							}

							region.Query<Region.GetSpawnsQuery>(FuncB).Execute(ref this);
							static void FuncB(ISystem.Info info, Entity entity, in Spawn.Data spawn, in Nameable.Data nameable, in Transform.Data transform, in Faction.Data faction)
							{
								ref var data = ref info.GetParameter<SiegeDefenderGUI>();
								if (!data.IsNull())
								{
									if (faction.id == 0)
									{
										var pressed = DrawSpawnsRow(entity, in spawn, in nameable, in faction);
										if (pressed)
										{
											//var rpc = new RespawnExt.SetSpawnRPC()
											//{
											//	ent_spawn = entity
											//};
											//rpc.Send(data.ent_respawn);
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
		public static void OnGUIDefender(Entity entity,
		[Source.Owned] in Player.Data player, [Source.Owned] in Respawn.Data respawn,
		[Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state, [Source.Global] in Siege.Bounty.Global g_bounty)
		{
			if (player.IsLocal() && player.faction_id == g_siege_state.faction_defenders)
			{
				Spawn.RespawnGUI.enabled = true;

				var gui = new SiegeDefenderGUI()
				{
					ent_respawn = entity,
					faction_id = player.faction_id,
					respawn = respawn,

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

			public void Draw()
			{
				var window_pos = (GUI.CanvasSize * new Vector2(0.50f, 0.00f)) + new Vector2(0, 64);
				using (var window = GUI.Window.Standalone("Siege2", position: window_pos, size: new Vector2(100, 100), pivot: new Vector2(0.50f, 0.00f), padding: new(6, 6)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						GUI.DrawWindowBackground();

						ref var region = ref Client.GetRegion();
						ref var world = ref Client.GetWorld();
						ref var game_info = ref Client.GetGameInfo();

						GUI.Title($"{this.g_siege_state.faction_defenders.id}");
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Owned)]
		public static void OnGUIAttacker(Entity entity, [Source.Owned] in Player.Data player, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state)
		{
			if (player.IsLocal() && player.faction_id == g_siege_state.faction_attackers)
			{
				Spawn.RespawnGUI.enabled = false;

				var gui = new SiegeAttackerGUI()
				{
					g_siege = g_siege,
					g_siege_state = g_siege_state
				};
				gui.Submit();
			}
		}
#endif
	}
}
