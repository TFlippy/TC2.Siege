using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		public static partial class Target
		{
			[IComponent.Data(Net.SendType.Unreliable)]
			public partial struct Data: IComponent
			{
				[Save.Ignore, Net.Ignore] public float last_capture_progress;

				[Save.Ignore, Net.Ignore] public float t_next_notification;
				[Save.Ignore, Net.Ignore] public float t_next_notification_capture;

#if SERVER
				//[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single)]
				//public static void OnPostDamage(ISystem.Info info, Entity entity, ref Health.PostDamageEvent data,
				//[Source.Owned] ref Health.Data health, [Source.Owned] ref Siege.Target.Data siege_target, [Source.Owned, Optional] in Faction.Data faction)
				//{
				//	ref var region = ref info.GetRegion();

				//	if (data.damage.faction_id == 0 || data.damage.faction_id != faction.id)
				//	{
				//		if (info.WorldTime >= siege_target.t_next_notification)
				//		{
				//			siege_target.t_next_notification = info.WorldTime + 2.00f;

				//			Notification.Push(ref region, $"{entity.GetFullName()} is under attack! ({(health.integrity * 100.00f):0}% left)", Color32BGRA.Yellow, lifetime: 7.00f, "buzzer.02", volume: 0.20f, pitch: 0.90f);
				//		}
				//	}
				//}

				[ISystem.LateUpdate(ISystem.Mode.Single, interval: 1.00f)]
				public static void OnUpdateCapturable(ISystem.Info info, Entity entity,
				[Source.Owned] ref Capturable.Data capturable, [Source.Owned] ref Faction.Data faction, [Source.Owned] ref Siege.Target.Data siege_target, [Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state)
				{
					ref var region = ref info.GetRegion();

					if (info.WorldTime >= siege_target.t_next_notification_capture)
					{
						if (siege_target.t_next_notification_capture > 0.00f)
						{
							ref var work = ref capturable.order.work[0];
							var capture_progress = work.current;

							var delta = capture_progress - siege_target.last_capture_progress;
							if (MathF.Abs(delta) >= 10.00f)
							{
								siege_target.last_capture_progress = capture_progress;

								if (float.IsNegative(delta))
								{
									Notification.Push(ref region, $"{entity.GetFullName()} is being captured! ({(Maths.NormalizeClamp(work.current, work.required) * 100.00f):0}%)", Color32BGRA.Yellow, lifetime: 5.00f, "buzzer.02", volume: 0.20f, pitch: 0.90f);
								}
							}
						}

						siege_target.t_next_notification_capture = info.WorldTime + 2.00f;
					}
				}

				[ISystem.Remove(ISystem.Mode.Single)]
				public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Target.Data siege_target)
				{
					ref var region = ref info.GetRegion();

					Notification.Push(ref region, $"{entity.GetFullName()} has been destroyed!", Color32BGRA.Red, lifetime: 10.00f, "buzzer.02", volume: 0.20f, pitch: 0.80f, send_type: Net.SendType.Reliable);
				}
#endif
			}
		}
	}
}
