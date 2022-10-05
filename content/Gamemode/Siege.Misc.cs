using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		[ISystem.Add(ISystem.Mode.Single)]
		public static void OnAddGun(ISystem.Info info, Entity entity,
		[Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory_magazine)
		{
			inventory_magazine.Flags |= Inventory.Flags.No_Drop;
		}

	}
}
