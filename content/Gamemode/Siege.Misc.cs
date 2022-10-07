using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		[ISystem.AddFirst(ISystem.Mode.Single)]
		public static void OnAddGuninventory(ISystem.Info info, Entity entity,
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
