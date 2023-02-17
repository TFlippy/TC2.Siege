using Keg.Extensions;
using System.Diagnostics;
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

					ref var ent_selected_spawn = ref Spawn.RespawnGUI.ent_selected_spawn;
					ent_selected_spawn = this.respawn.ent_selected_spawn;

					using (var window = GUI.Window.Standalone("Respawn", position: new Vector2(GUI.CanvasSize.X * 0.50f, 0) + Spawn.RespawnGUI.window_offset, pivot: Spawn.RespawnGUI.window_pivot, size: Spawn.RespawnGUI.window_size))
					{
						Spawn.RespawnGUI.window_size = new Vector2(800, 600);

						this.StoreCurrentWindowTypeID();
						if (window.show)
						{
							GUI.DrawWindowBackground(GUI.tex_window_menu, padding: new Vector4(8, 8, 8, 8));

							using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight()), padding: new(8, 8)))
							{
								if (!ent_selected_spawn.IsAlive())
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
													//ISystem.Info info, Entity entity, [Source.Owned] in Minimap.Marker.Data marker, [Source.Owned] in Transform.Data transform, [Source.Owned, Optional] in Faction.Data faction, [Source.Owned, Optional] in Nameable.Data nameable

													foreach (ref var row in region.IterateQuery<Minimap.GetMarkersQuery>())
													{
														var selected = row.Entity == ent_selected_spawn;

														var transform_copy = default(Transform.Data);
														//var nameable_copy = default(Nameable.Data);
														var color = selected ? Color32BGRA.White : new Color32BGRA(0xff9a7f7f);
														var faction_id_tmp = this.faction_id;

														var ok = false;

														row.Run((ISystem.Info info, Entity entity, [Source.Owned] in Minimap.Marker.Data marker, [Source.Owned] in Transform.Data transform, [Source.Owned, Optional] in Faction.Data faction, [Source.Owned, Optional] in Nameable.Data nameable) =>
														{
															transform_copy = transform;
															//nameable_copy = nameable;

															if (faction.id.TryGetData(out var ref_faction) && faction.id == faction_id_tmp && marker.flags.HasAny(Minimap.Marker.Flags.Faction))
															{
																color = ref_faction.value.color_a;
																//sprite.frame.X = 1;
																ok = true;
															}
														});

														if (ok)
														{
															using (var node = map.DrawNode(new Sprite(tex_icons_minimap, 16, 16, 3, 0), transform_copy.GetInterpolatedPosition() + new Vector2(0, -3), color: selected ? Color32BGRA.White : color, color_hovered: selected ? Color32BGRA.White : Color32BGRA.Lerp(color, Color32BGRA.White, 0.50f)))
															{
																//GUI.DrawTextCentered(nameable_copy.name, node.rect.GetPosition() + new Vector2(16, 0), piv layer: GUI.Layer.Window, font: GUI.Font.Superstar, size: 16);

																if (node.is_hovered && !selected)
																{
																	GUI.SetCursor(App.CursorType.Hand, 100);

																	if (GUI.GetMouse().GetKeyDown(Mouse.Key.Left))
																	{
																		//App.WriteLine("press");
																		ent_selected_spawn_new = row.Entity;
																	}

																	using (GUI.Tooltip.New())
																	{
																		//GUI.Title(nameable_copy.name, font: GUI.Font.Superstar, size: 16);
																		GUI.Title(row.Entity.GetFullName(), font: GUI.Font.Superstar, size: 16);
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

								GUI.SeparatorThick();

								//this.DrawSpawns(ref region, size: new(GUI.GetRemainingWidth(), (24 * 4.50f) + 8));

								//GUI.SeparatorThick();

								//var h_character = default(ICharacter.Handle);

								if (ent_selected_spawn.IsAlive())
								{
									ref var dormitory = ref ent_selected_spawn.GetComponent<Dormitory.Data>();

									var selected_items = Spawn.RespawnGUI.character_id_to_selected_items.GetOrAdd(h_selected_character);

									//var context = GUI.ItemContext.Begin();

									using (GUI.Group.New(size: GUI.GetRemainingSpace() with { X = 400 }, padding: new(0, 0)))
									{
										using (var scrollable = GUI.Scrollbox.New("characters", size: GUI.GetRemainingSpace(), padding: new(4, 4), force_scrollbar: true))
										{
											if (dormitory.IsNotNull())
											{
												var characters = dormitory.characters.AsSpan();

												var characters_count_max = Math.Min(characters.Length, dormitory.characters_capacity);
												for (var i = 0; i < characters_count_max; i++)
												{
													//DrawCharacter(characters[i].GetHandle());

													using (GUI.ID.Push(i - 100))
													{
														using (var group_row = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 40)))
														{
															var h_character = characters[i];
															Dormitory.DormitoryGUI.DrawCharacterSmall(h_character);

															var selected = h_selected_character.id != 0 && h_character == h_selected_character; // selected_index;
															if (GUI.Selectable3("selectable", group_row.GetOuterRect(), selected))
															{
																if (selected) h_selected_character = 0;
																else h_selected_character = h_character;
															}
														}
													}
												}
											}

											//ref var platoon = ref player.ent_player.GetComponent<Siege.Platoon.Data>();
											//if (platoon.IsNotNull())
											//{
											//	foreach (ref var h_character in platoon.characters)
											//	{
											//		ref var character_data = ref h_character.GetData();
											//		if (character_data.IsNotNull())
											//		{
											//			using (GUI.ID.Push(h_character.id))
											//			{
											//				using (var group_row = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 40)))
											//				{
											//					Dormitory.DormitoryGUI.DrawCharacterSmall(h_character);

											//					var selected = h_selected_character == h_character;
											//					if (GUI.Selectable3("select", group_row.GetOuterRect(), selected: selected))
											//					{
											//						h_selected_character = selected ? default : h_character;
											//					}
											//				}
											//			}
											//		}
											//	}
											//}
										}
									}

									GUI.SameLine();

									using (var group_character = GUI.Group.New(size: GUI.GetRemainingSpace()))
									{
										ref var character_data = ref h_selected_character.GetData();

										using (var group_title = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 24), padding: new(8, 8)))
										{
											if (character_data.IsNotNull())
											{
												GUI.TitleCentered(character_data.name, size: 24, pivot: new(0.00f, 0.00f));
											}
											else
											{
												GUI.TitleCentered("<no character selected>", size: 24, pivot: new(0.00f, 0.00f));
											}
										}

										GUI.SeparatorThick();

										using (var group_title = GUI.Group.New(size: GUI.GetRemainingSpace(y: -300), padding: new(8, 8)))
										{

										}

										GUI.SeparatorThick();

										using (var group_kits = GUI.Group.New(size: GUI.GetRemainingSpace(y: -48), padding: new(8, 8)))
										{
											GUI.DrawBackground(GUI.tex_panel, group_kits.GetOuterRect(), new(8, 8, 8, 8));

											ref var armory = ref ent_selected_spawn.GetComponent<Armory.Data>();
											if (armory.IsNotNull())
											{
												armory.inv_storage.TryGetHandle(out var h_inventory);
												ref var shipment = ref ent_selected_spawn.GetComponent<Shipment.Data>();

												using (var group_storage = GUI.Group.New(size: new(96 + 16, GUI.GetRemainingHeight()), padding: new(8, 8)))
												{
													GUI.DrawBackground(GUI.tex_frame, group_storage.GetOuterRect(), new(8, 8, 8, 8));


													if (h_inventory.IsValid())
													{
														using (GUI.Group.New(size: h_inventory.GetPreferedFrameSize()))
														{
															GUI.DrawInventory(h_inventory, is_readonly: true);
														}
													}

													if (shipment.IsNotNull())
													{
														var sameline = false;
														foreach (ref var item in shipment.items)
														{
															if (!item.IsValid()) continue;

															if (sameline) GUI.SameLine();
															sameline = true;

															GUI.DrawItem(ref item, is_readonly: true);
														}

														//GUI.DrawShipment(ref context, ent_selected_spawn, ref shipment, slot_size: new(48, 48));
													}
												}

												GUI.SameLine();

												//var ts = Timestamp.Now();
												var sw = new Stopwatch();

												using (var scrollable = GUI.Scrollbox.New("kits", size: GUI.GetRemainingSpace(), padding: new(4, 4), force_scrollbar: true))
												{
													if (character_data.IsNotNull() && shipment.IsNotNull() && h_inventory.IsValid())
													{
														var shipment_armory_span = shipment.items.AsSpan();

														foreach (var asset in IKit.Database.GetAssets())
														{
															if (asset.id == 0) continue;
															ref var kit_data = ref asset.GetData();
															var h_kit = asset.GetHandle();

															if (kit_data.character_flags.Evaluate(character_data.flags) < 0.50f) continue;

															var kit_items_span = kit_data.shipment.items.AsSpan();
															var valid = true;

															sw.Start();
															foreach (ref var item in kit_items_span)
															{
																if (!item.IsValid()) continue;

																var has_item = shipment_armory_span.Contains(item);
																if (!has_item && item.type == Shipment.Item.Type.Resource)
																{
																	has_item = h_inventory.GetQuantity(item.material) >= item.quantity;
																}

																if (!has_item)
																{
																	valid = false;
																	break;
																}
															}
															sw.Stop();

															if (true || valid)
															{
																using (GUI.ID.Push(asset.id))
																{
																	using (var group_row = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 48), padding: new(4, 4)))
																	{
																		GUI.DrawBackground(GUI.tex_panel, group_row.GetOuterRect(), new(8, 8, 8, 8));

																		using (var group_title = GUI.Group.New(size: new(128, GUI.GetRemainingHeight())))
																		{
																			GUI.TitleCentered(kit_data.name, pivot: new(0.00f, 0.00f), color: GUI.font_color_title.WithAlphaMult(valid ? 1.00f : 0.50f));
																		}

																		GUI.SameLine();

																		//GUI.DrawItems(shipment.items.AsSpan(), is_readonly: true);
																		//GUI.DrawItems(shipment.items.AsSpan(), is_readonly: true);

																		var sameline = false;
																		foreach (ref var item in kit_items_span)
																		{
																			if (!item.IsValid()) continue;

																			if (sameline) GUI.SameLine();
																			sameline = true;

																			sw.Start();
																			var has_item = shipment_armory_span.Contains(item);
																			if (!has_item && item.type == Shipment.Item.Type.Resource)
																			{
																				has_item = h_inventory.GetQuantity(item.material) >= item.quantity;
																			}
																			sw.Stop();

																			if (has_item)
																			{

																			}

																			GUI.DrawItem(ref item, is_readonly: true, text_color: has_item ? GUI.font_color_default : GUI.col_button_error.WithAlphaMult(0.50f), icon_color: has_item ? Color32BGRA.White : GUI.col_button_error.WithAlphaMult(0.50f));
																		}

																		var selected = selected_items.Contains(h_kit);
																		if (GUI.Selectable3("selectable", group_row.GetOuterRect(), selected, is_readonly: !valid))
																		{
																			if (selected) selected_items.Remove(h_kit);
																			else selected_items.Add(h_kit);
																		}
																	}
																}
															}
														}
													}
												}

												var ts_elapsed = sw.Elapsed.TotalMilliseconds; //.GetMilliseconds();
												//GUI.Text($"{ts_elapsed:0.0000} ms");
											}

											//using (var dropdown = GUI.Dropdown.Begin("armor", "Armor", size: new(200, 40)))
											//{
											//	if (dropdown.show)
											//	{
											//		foreach (var asset in IKit.Database.GetAssets())
											//		{
											//			if (asset.id == 0) continue;

											//			ref var kit_data = ref asset.GetData();

											//			if (kit_data.category != Kit.Category.Armor) continue;

											//			using (GUI.ID.Push(asset.id))
											//			{
											//				using (var group_row = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 32)))
											//				{
											//					GUI.TitleCentered(kit_data.name, pivot: new(0.00f, 0.00f));

											//					if (GUI.Selectable3("select", group_row.GetOuterRect(), false))
											//					{
											//						dropdown.Close();
											//					}
											//				}
											//			}
											//		}
											//	}
											//}

											//foreach (var asset in IKit.Database.GetAssets())
											//{
											//	if (asset.id == 0) continue;

											//	ref var kit_data = ref asset.GetData();

											//	using (GUI.ID.Push(asset.id))
											//	{
											//		using (var group_row = GUI.Group.New(size: new(GUI.GetRemainingWidth(), 32)))
											//		{
											//			GUI.TitleCentered(kit_data.name, pivot: new(0.00f, 0.50f));
											//		}
											//	}
											//}
										}

										if (ent_selected_spawn.IsAlive())
										{
											ref var faction = ref ent_selected_spawn.GetComponent<Faction.Data>();
											if (faction.IsNull() || faction.id == player.faction_id)
											{
												if (GUI.DrawButton("Respawn", size: new Vector2(GUI.GetRemainingWidth() - 100, 40), enabled: dormitory.IsNotNull()))
												{
													var rpc = new Dormitory.DEV_SpawnRPC()
													{
														h_character = h_selected_character
													};

													foreach (var h_kit in selected_items)
													{
														rpc.kits.TryAdd(in h_kit);
													}

													rpc.Send(ent_selected_spawn);
												}

												GUI.SameLine();

												if (GUI.DrawButton("DEV: Add", size: new Vector2(GUI.GetRemainingWidth(), 40), enabled: dormitory.IsNotNull()))
												{
													var rpc = new Dormitory.DEV_RerollRPC()
													{
														add = true
													};
													rpc.Send(ent_selected_spawn);
												}

												//ref var spawn = ref ent_selected_spawn.GetComponent<Spawn.Data>();
												//if (!spawn.IsNull())
												//{
												//	//if (GUI.DrawButton((this.respawn.cooldown > 0.00f ? $"Respawn ({MathF.Floor(this.respawn.cooldown):0}s)" : "Respawn"), new Vector2(168, 48), enabled: this.respawn.cooldown <= float.Epsilon && h_selected_character.id != 0, font_size: 24, color: GUI.font_color_green_b))
												//	if (GUI.DrawButton((this.respawn.cooldown > 0.00f ? $"Respawn ({MathF.Floor(this.respawn.cooldown):0}s)" : "Respawn"), new Vector2(168, 48), enabled: this.respawn.cooldown <= float.Epsilon, font_size: 24, color: GUI.font_color_green_b))
												//	{
												//		var rpc = new RespawnExt.SpawnRPC
												//		{
												//			ent_spawn = ent_selected_spawn
												//		};
												//		rpc.Send(player.ent_player);
												//	}
												//	if (GUI.IsItemHovered())
												//	{
												//		using (GUI.Tooltip.New())
												//		{
												//			GUI.Text("Respawn as this character at the selected spawn point.");
												//		}
												//	}
												//}
												//else
												//{

												//}
											}
											else
											{
												ent_selected_spawn = default;
											}
										}
									}
								}

								if (ent_selected_spawn_new.HasValue && ent_selected_spawn_new != ent_selected_spawn)
								{
									var rpc = new RespawnExt.SetSpawnRPC()
									{
										ent_spawn = ent_selected_spawn_new.Value
									};
									rpc.Send(player.ent_player);
									ent_selected_spawn_new = default;
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
							}
						}
					}
				}
			}

			//internal static void DrawKits(IOrigin.Handle origin, Character.Flags character_flags, Vector2 size, bool read_only = false, HashSet<IKit.Handle> selected_items = null)
			//{
			//	//GUI.NewLine();
			//	//GUI.Title("Kits", size: 32);
			//	//GUI.Separator();

			//	using (var scrollable = GUI.Scrollbox.New("Kits", size: size, padding: new(4, 4), force_scrollbar: true))
			//	{
			//		using (var table = GUI.Table.New("Kits.Table", 2))
			//		{
			//			if (table.show)
			//			{
			//				table.SetupColumnFixed(192);
			//				table.SetupColumnFlex(1);
			//				//table.SetupColumnFixed(100);

			//				//var selected_items = character_id_to_selected_items.GetOrAdd(character.id);

			//				//for (var kit_index = 1; kit_index < kits.Length; kit_index++)

			//				//using (var row = table.NextRow(24, header: true))
			//				//{
			//				//	using (row.Column(0, padding: new(4, 4)))
			//				//	{
			//				//		GUI.Title("Name");
			//				//	}

			//				//	using (row.Column(1, padding: new(8, 0)))
			//				//	{
			//				//		GUI.Title("Items");
			//				//	}

			//				//	using (row.Column(2, padding: new(8, 0)))
			//				//	{
			//				//		GUI.Title("Tickets", size: 20);
			//				//	}
			//				//}

			//				foreach (var kit_asset in IKit.Database.GetAssets())
			//				{
			//					if (kit_asset.id == 0) continue;

			//					ref var kit = ref kit_asset.data;
			//					//ref var kit = ref kits[kit_index];
			//					if (!kit.flags.HasAny(Kit.Flags.Hidden)) // && character_flags.HasAll(kit.character_flags) && (kit.origin == 0 || origin == kit.origin))
			//					{
			//						var is_selected = !read_only && (kit.flags.HasAny(Kit.Flags.Required) || (selected_items?.Contains(kit_asset.GetHandle()) ?? false));

			//						using (GUI.ID.Push(kit_asset.id))
			//						{
			//							using (var row = table.NextRow(40))
			//							{
			//								if (!read_only && is_selected)
			//								{
			//									current_cost += kit.cost;
			//								}

			//								using (row.Column(0, padding: new(4, 4)))
			//								{
			//									GUI.Title(kit.name);
			//									//GUI.Text($"Cost: {kit.cost:0.00}");

			//									if (kit.cost == 0.00f)
			//									{
			//										GUI.Text($"Free", color: GUI.font_color_default);
			//									}
			//									else if (kit.cost > 0.00f)
			//									{
			//										GUI.Text($"{kit.cost:0.00} tickets", color: GUI.font_color_default);
			//									}
			//									else
			//									{
			//										GUI.Text($"+{-kit.cost:0.00} tickets", color: GUI.font_color_default);
			//									}
			//								}

			//								using (row.Column(1, padding: new(8, 0)))
			//								{
			//									//using (GUI.Group.New(GUI.GetAvailableSize()))
			//									{
			//										ref var shipment = ref kit.shipment;
			//										for (int i = 0; i < shipment.items.Length; i++)
			//										{
			//											ref var item = ref shipment.items[i];
			//											if (item.type != Shipment.Item.Type.Undefined)
			//											{
			//												switch (item.type)
			//												{
			//													case Shipment.Item.Type.Resource:
			//													{
			//														var resource = new Resource.Data(item.material, item.quantity);
			//														GUI.DrawResourceSmall(ref resource, size: new Vector2(32));
			//														GUI.SameLine();
			//													}
			//													break;

			//													case Shipment.Item.Type.Prefab:
			//													{
			//														GUI.DrawPrefabSmall(item.prefab, (int)item.quantity);
			//														GUI.SameLine();
			//													}
			//													break;
			//												}
			//											}
			//										}
			//									}
			//								}

			//								//using (row.Column(2, padding: new(8, 0)))
			//								//{
			//								//	//if (kit.cost == 0.00f)
			//								//	//{
			//								//	//	GUI.TextShadedCentered($"Free", pivot: new(0.50f, 0.50f), color: GUI.font_color_default);
			//								//	//}
			//								//	//else if (kit.cost > 0.00f)
			//								//	//{
			//								//	//	GUI.TextShadedCentered($"{kit.cost:0.00} tickets", pivot: new(1.00f, 0.50f), color: GUI.font_color_default);
			//								//	//}
			//								//	//else
			//								//	//{
			//								//	//	GUI.TextShadedCentered($"{kit.cost:0.00} tickets", pivot: new(1.00f, 0.50f), color: GUI.font_color_default);
			//								//	//}
			//								//}

			//								if (!read_only)
			//								{
			//									GUI.SameLine();

			//									//if (GUI.Selectable($"##kit_{kit_index}", is_selected, size: new Vector2(0, lh)))
			//									if (GUI.Selectable2(is_selected, size: new Vector2(0, 40), enabled: (is_selected || selected_items.Count < max_item_count), is_readonly: read_only))
			//									{
			//										if (!read_only && selected_items != null && !kit.flags.HasAny(Kit.Flags.Required))
			//										{
			//											if (is_selected) selected_items.Remove(kit_asset.GetHandle());
			//											else selected_items.Add(kit_asset.GetHandle());
			//										}
			//									}
			//								}
			//							}
			//						}
			//					}
			//				}
			//			}
			//		}
			//	}
			//}

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
