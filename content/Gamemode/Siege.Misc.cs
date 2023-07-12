using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Base.Components
{
	using TC2.Siege;

	public static partial class Selection
	{

#if CLIENT
		static partial void Ext_DrawLeftGroup(ref Selection.SelectionGUI gui, in GUI.Group group, ref Region.Data region, ref Player.Data player, OwnedComponent<Squad.Data> oc_squad)
		{
			ref var player_ext = ref player.ent_player.GetComponent<Siege.PlayerExt.Data>();
			if (player_ext.IsNotNull())
			{
				var is_joined = player_ext.ent_squad == oc_squad.entity;

				if (GUI.DrawButton(is_joined ? "Unclaim" : "Claim", new Vector2(80, 40), color: is_joined ? GUI.col_button_error : GUI.col_button_ok))
				{
					var rpc = new Siege.PlayerExt.SetSquadRPC
					{
						ent_squad = is_joined ? default : oc_squad.entity
					};
					rpc.Send(player.ent_player);
				}
			}
		}

		//public partial struct SelectionGUI
		//{
		//	partial void DrawTest();

		//}
#endif
	}
}

namespace TC2.Siege
{
	public static partial class Siege
	{
		public static partial class PlayerExt
		{
			[IComponent.Data(Net.SendType.Unreliable), IComponent.AddTo<Player.Data>()]
			public partial struct Data: IComponent
			{
				public Entity ent_squad;
			}

			public partial struct SetSquadRPC: Net.IRPC<Siege.PlayerExt.Data>
			{
				public Entity ent_squad;

#if SERVER
				public void Invoke(ref NetConnection connection, Entity entity, ref Data data)
				{
					ref var region = ref connection.GetRegion();
					ref var player = ref connection.GetPlayer();

					if (region.IsNotNull() && player.IsNotNull() && entity == player.ent_player && (!this.ent_squad.IsAlive() || this.ent_squad.GetFaction() == player.faction_id))
					{
						data.ent_squad = this.ent_squad;
						data.Sync(entity, true);

						App.WriteLine($"Set player's squad to {this.ent_squad.GetName()}");
					}
				}
#endif
			}

		}

		[ISystem.AddFirst(ISystem.Mode.Single)]
		public static void OnAddGunInventory(ISystem.Info info, Entity entity,
		[Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory_magazine)
		{
			//App.WriteLine($"{inventory_magazine.Flags}; {entity.HasComponent<Entity.Data>()}");
			inventory_magazine.Flags |= Inventory.Flags.No_Drop;
			//App.WriteLine($"added gun no_drop {entity}; {inventory_magazine.Flags}");
		}

		//#if SERVER
		//		[ISystem.AddFirst(ISystem.Mode.Single)]
		//		public static void OnAddGun(ISystem.Info info, Entity entity, [Source.Owned] in Gun.Data gun)
		//		{
		//			ref var despawn = ref entity.GetOrAddComponent<Despawn.Data>(sync: true);
		//			if (despawn.IsNotNull())
		//			{
		//				despawn.interval = 10.00f;
		//				App.WriteLine($"added gun despawn to {entity}");
		//			}
		//		}
		//#endif
	}
}
