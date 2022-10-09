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
				public FixedArray8<Crafting.Product> rewards;

				public int last_wave;
			}

#if SERVER
			[ISystem.LateUpdate(ISystem.Mode.Single)]
			public static void OnUpdateWave(ISystem.Info info, [Source.Global] in Siege.Gamemode siege, [Source.Global] ref Siege.Bounty.Global g_bounty)
			{
				ref var region = ref info.GetRegion();

				var connected_count = region.GetConnectedPlayerCount();
				if (siege.status == Gamemode.Status.Running && connected_count > 0)
				{
					if (siege.wave_current != g_bounty.last_wave)
					{
						g_bounty.last_wave = siege.wave_current;

						var rewards_tmp = g_bounty.rewards;
						var multiplier = Maths.Lerp(1.00f, 1.00f / connected_count, siege.loot_share_ratio);

						for (var i = 0u; i < connected_count; i++)
						{
							var ent_player = region.GetConnectedPlayerEntityByIndex(i);
							Crafting.Produce(ref region, ent_player, ref rewards_tmp, amount_multiplier: multiplier);
						}

						g_bounty.rewards = default;

						//Notification.Push(ref region, $"Group of {planner.wave_size} kobolds approaching from the {((transform.position.X / region.GetTerrain().GetWidth()) < 0.50f ? "west" : "east")}!", Color32BGRA.Red, lifetime: 10.00f);

					}
				}
			}

			[ISystem.RemoveLast(ISystem.Mode.Single)]
			public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Bounty.Data bounty, [Source.Global] ref Siege.Bounty.Global g_bounty)
			{
				ref var region = ref info.GetRegion();
				App.WriteLine("on remove");

				var rewards_total = g_bounty.rewards.AsSpan();
				foreach (ref var reward in bounty.rewards.AsSpan())
				{
					if (reward.type != Crafting.Product.Type.Undefined)
					{
						App.WriteLine($"add reward {reward.type} {reward.amount}");
						rewards_total.Add(reward);
					}
				}

				region.SyncGlobal(ref g_bounty);
			}
#endif
		}
	}
}
