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
				public FixedArray4<Crafting.Product> rewards;
			}

			[IGlobal.Data(false, Net.SendType.Unreliable)]
			public partial struct Global: IGlobal
			{
				[Save.Ignore] public FixedArray8<Crafting.Product> rewards;

				public float update_interval = 30.00f;
				[Save.Ignore] public int last_wave;
				[Save.Ignore] public float t_next_update;

				public Global()
				{

				}
			}

#if SERVER
			[ISystem.LateUpdate(ISystem.Mode.Single)]
			public static void OnUpdateRewards(ISystem.Info info, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state, [Source.Global] ref Siege.Bounty.Global g_bounty)
			{
				ref var region = ref info.GetRegion();

				if (g_siege_state.status == Gamemode.Status.Running && g_siege_state.flags.HasAny(Siege.Gamemode.Flags.Active) && g_siege_state.player_count > 0)
				{
					if (g_siege_state.t_match_elapsed >= g_bounty.t_next_update)
					{
						g_bounty.t_next_update = g_siege_state.t_match_elapsed + g_bounty.update_interval;

						if (g_siege_state.wave_current != g_bounty.last_wave)
						{
							g_bounty.last_wave = g_siege_state.wave_current;
						}

						if (g_bounty.rewards.AsSpan().HasAny())
						{
							var rewards_tmp = g_bounty.rewards;
							var multiplier = Maths.Lerp(1.00f, 1.00f / (float)g_siege_state.player_count, g_siege.loot_share_ratio);

							foreach (ref var reward in rewards_tmp.AsSpan())
							{
								if (reward.type == Crafting.Product.Type.Money)
								{
									reward.amount = Money.ToBataPrice(reward.amount * multiplier);
									Notification.Push(ref region, $"Received payment of {reward.amount:0.00} coins.", Color32BGRA.Green, lifetime: 10.00f, sound: "quest_complete", volume: 0.25f, pitch: 0.90f);
								}
							}

							for (var i = 0u; i < g_siege_state.player_count; i++)
							{
								var ent_player = region.GetConnectedPlayerEntityByIndex(i);
								Crafting.Produce(ref region, ent_player, ref rewards_tmp);
							}

							g_bounty.rewards = default;
							region.SyncGlobal(ref g_bounty);

							//Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Red, lifetime: 10.00f);

						}
					}
				}
			}

			[ISystem.RemoveLast(ISystem.Mode.Single)]
			public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Bounty.Data bounty, [Source.Global] ref Siege.Bounty.Global g_bounty)
			{
				ref var region = ref info.GetRegion();

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
#endif
		}
	}
}
