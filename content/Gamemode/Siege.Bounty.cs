using Keg.Engine.Game;
using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		public static partial class Bounty
		{
			[IComponent.Data(Net.SendType.Unreliable)]
			public partial struct Data: IComponent
			{
				[Net.Ignore]
				public FixedArray4<Crafting.Product> rewards;
			}

			[IGlobal.Data(false, Net.SendType.Unreliable)]
			public partial struct Global: IGlobal
			{
				[Save.Ignore] public FixedArray4<Crafting.Product> rewards;

				public float update_interval = 10.00f;
				public float payout_interval = 30.00f;

				[Save.Ignore] public int last_wave;
				[Save.Ignore] public float t_next_update;
				[Save.Ignore] public float t_next_payout;

				public Global()
				{

				}
			}

#if SERVER
			[ISystem.RemoveLast(ISystem.Mode.Single)]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			//public static void OnCharacterRemove(ISystem.Info info, Entity entity, [Source.Shared] ref Character.Data character, [Source.Shared] ref Money.Data money, [Source.Owned] in Organic.State organic_state)
			public static void OnCharacterRemove(ISystem.Info info, Entity ent_money_character, Entity ent_money_squad, [Source.Shared] ref Character.Data character, [Source.Parent<Squad.Relation>] ref Money.Data money_squad, [Source.Shared] ref Money.Data money_character)
			{
				var amount = money_character.amount;
				money_character.amount -= amount;
				money_squad.amount += amount;

				money_character.Sync(ent_money_character, true);
				money_squad.Sync(ent_money_squad, true);

				//App.WriteLine($"OnCharacterRemove; {ent_money_character.GetName()} to {ent_money_squad.GetName()}; {amount} coins");
			}

			[ISystem.LateUpdate(ISystem.Mode.Single, interval: 0.50f)]
			public static void OnUpdateRewards(ref Region.Data region, ISystem.Info info, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state, [Source.Global] ref Siege.Bounty.Global g_bounty)
			{
				if (g_siege_state.status == Gamemode.Status.Running && g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Active) && g_siege_state.player_count > 0)
				{
					if (g_siege_state.t_match_elapsed >= g_bounty.t_next_update)
					{
						g_bounty.t_next_update = g_siege_state.t_match_elapsed + g_bounty.update_interval;

						if (g_siege_state.wave_current != g_bounty.last_wave)
						{
							g_bounty.last_wave = g_siege_state.wave_current;
						}

						if (g_bounty.t_next_payout == 0.00f && g_bounty.rewards.AsSpan().HasAny())
						{
							g_bounty.t_next_payout = g_siege_state.t_match_elapsed + g_bounty.payout_interval;
							region.SyncGlobal(ref g_bounty);
						}
					}

					if (g_bounty.t_next_payout != 0.00f && g_siege_state.t_match_elapsed >= g_bounty.t_next_payout)
					{
						g_bounty.t_next_payout = 0.00f;

						var rewards_span = g_bounty.rewards.AsSpan();
						if (rewards_span.HasAny())
						{
							//var multiplier = Maths.Lerp(1.00f, 1.00f / (float)g_siege_state.player_count, g_siege.reward_share_ratio) * g_siege.reward_mult;

							Span<Entity> span_squads = stackalloc Entity[16];

							var ts = Timestamp.Now();
							region.GetEntsWithComponent<Squad.Data>(ref span_squads, h_faction: g_siege_state.faction_defenders);
							var ts_elapsed = ts.GetMilliseconds();

							//GUI.Title($"Squads {span_squads.Length}; {ts_elapsed:0.0000} ms");

							if (span_squads.Length > 0)
							{
								var multiplier = Maths.Lerp(1.00f, 1.00f / (float)span_squads.Length, g_siege.reward_share_ratio) * g_siege.reward_mult;

								foreach (ref var reward in rewards_span)
								{
									if (reward.type == Crafting.Product.Type.Money)
									{
										reward.amount = Money.ToBataPrice(reward.amount * multiplier);
										Notification.Push(ref region, $"Received payment of {reward.amount:0.00} {Money.symbol}.", Color32BGRA.Green, lifetime: 10.00f, sound: "quest_complete", volume: 0.25f, pitch: 0.90f);
									}
								}

								foreach (var ent_squad in span_squads)
								{
									Crafting.Context.New(ref region, ent_squad, ent_squad, out var context, money: ent_squad);
									Crafting.Produce(ref context, rewards_span);
									//Notification.Push(ref region, $"Received payment of {reward.amount:0.00} coins.", Color32BGRA.Green, lifetime: 10.00f, sound: "quest_complete", volume: 0.25f, pitch: 0.90f);
								}
							}


							//foreach (ref var reward in rewards_tmp.AsSpan())
							//{
							//	if (reward.type == Crafting.Product.Type.Money)
							//	{
							//		reward.amount = Money.ToBataPrice(reward.amount * multiplier);
							//		Notification.Push(ref region, $"Received payment of {reward.amount:0.00} coins.", Color32BGRA.Green, lifetime: 10.00f, sound: "quest_complete", volume: 0.25f, pitch: 0.90f);
							//	}
							//}

							//for (var i = 0u; i < g_siege_state.player_count; i++)
							//{
							//	var ent_player = region.GetConnectedPlayerEntityByIndex(i);
							//	Crafting.Produce(ref region, ent_player, ref rewards_tmp);
							//}

							g_bounty.rewards = default;
							region.SyncGlobal(ref g_bounty);
						}
					}
				}
			}

			[ISystem.RemoveLast(ISystem.Mode.Single)]
			public static void OnRemove(ref Region.Data region, ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Bounty.Data bounty, [Source.Global] ref Siege.Bounty.Global g_bounty)
			{
				var rewards_total = g_bounty.rewards.AsSpan();
				foreach (ref var reward in bounty.rewards.AsSpan())
				{
					if (reward.type != Crafting.Product.Type.Undefined)
					{
						rewards_total.Add(reward);
					}
				}

				region.SyncGlobal(ref g_bounty);
			}

			[ISystem.Event<Despawn.DespawnEvent>(ISystem.Mode.Single, order: -10)]
			public static void OnDespawn(ISystem.Info info, Entity entity, ref Region.Data region, ref Despawn.DespawnEvent data, [Source.Owned] in Transform.Data transform, [Source.Global] ref Siege.Bounty.Global g_bounty)
			{
				if (entity.TryGetPrefab(out var prefab))
				{
					var node = Claim.GetNodeAtWorldPos(ref region.GetTerrain(), transform.position);
					var faction_id = node.Faction;

					var reward = 0.00f;

					reward += prefab.cost_materials * 1.00f ?? 0.00f;
					reward += prefab.cost_work * 0.75f ?? 0.00f;
					reward += prefab.cost_extra * 0.90f ?? 0.00f;

					if (reward > 0.00f)
					{
						reward = MathF.Pow(reward * 25.00f, 0.65f);
						reward = Money.ToBataPrice(reward);

						if (reward > 10.00f)
						{
							var rewards_span = g_bounty.rewards.AsSpan();
							rewards_span.Add(Crafting.Product.Money(reward));

							region.SyncGlobal(ref g_bounty);

							WorldNotification.Push(ref region, $"Salvaged!\n+{reward:0.00} {Money.symbol}", color: Color32BGRA.Yellow, transform.position, force: new Vector2(0.00f, -1.00f), velocity: Vector2.Zero, lifetime: 5.00f, faction_id: faction_id);
						}
					}

					//App.WriteLine($"Despawned {prefab.GetName()}; territory: {node.Faction}; reward: {reward:0.00} money");
				}
			}
#endif
		}
	}
}
