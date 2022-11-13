using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public partial interface IUnit: IAsset2<IUnit, IUnit.Data>
	{
		static void IAsset2<IUnit, IUnit.Data>.OnUpdate(IUnit.Definition definition, ref IUnit.Data data_new)
		{

		}

		static void IAsset2<IUnit, IUnit.Data>.OnInit(out string prefix, out string[] extensions, out int capacity_world, out int capacity_region, out int capacity_local)
		{
			prefix = "unit.";
			extensions = new string[]
			{
				".hjson"
			};

			capacity_world = 64;
			capacity_region = 64;
			capacity_local = 64;
		}

		[Serializable]
		public partial struct Data
		{
			public string name;
			public string desc;
			public Sprite icon;

			public int price;
			public int reward;

			public Prefab.Handle creature;

			public Prefab.Handle[] items;
			public Prefab.Handle[] equipment;
			[Save.NewLine]
			public IUnit.ResourceStruct[] resource;
		}
		[Serializable]
		public struct ResourceStruct
		{
			public string material;
			public int quantity;
		}

	}
}
