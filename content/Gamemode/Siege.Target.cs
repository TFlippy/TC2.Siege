﻿using Keg.Extensions;
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
				public IFaction.Handle faction_id;

				[Save.Ignore, Net.Ignore] public float next_notification;

#if SERVER
				[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single)]
				public static void OnPostDamage(ISystem.Info info, Entity entity, ref Health.PostDamageEvent data,
				[Source.Owned] ref Health.Data health, [Source.Owned] ref Siege.Target.Data siege_target, [Source.Owned, Optional] in Faction.Data faction)
				{
					ref var region = ref info.GetRegion();

					if (data.damage.faction_id == 0 || data.damage.faction_id != faction.id)
					{
						if (info.WorldTime >= siege_target.next_notification)
						{
							siege_target.next_notification = info.WorldTime + 2.00f;

							Notification.Push(ref region, $"{entity.GetFullName()} is under attack! ({(health.integrity * 100.00f):0}% left)", Color32BGRA.Yellow, lifetime: 7.00f, "buzzer.02", volume: 0.20f, pitch: 0.90f);
						}
					}
				}

				[ISystem.Remove(ISystem.Mode.Single)]
				public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] ref Siege.Target.Data siege_target)
				{
					ref var region = ref info.GetRegion();

					Notification.Push(ref region, $"{entity.GetFullName()} has been destroyed!", Color32BGRA.Red, lifetime: 10.00f, "buzzer.02", volume: 0.20f, pitch: 0.80f);
				}
#endif
			}
		}
	}
}
