using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		public static partial class Platoon
		{
			[IComponent.Data(Net.SendType.Unreliable), IComponent.AddTo<Player.Data>()]
			public struct Data: IComponent
			{
				public FixedArray16<ICharacter.Handle> characters;

				public Data()
				{

				}
			}

#if SERVER
			[ISystem.AddFirst(ISystem.Mode.Single)]
			public static void OnAdd(ISystem.Info info, Entity entity, ref XorRandom random,
			[Source.Owned] in Player.Data player, [Source.Owned] ref Respawn.Data respawn, [Source.Owned] ref Siege.Platoon.Data platoon,
			[Source.Global] in Siege.Gamemode g_siege, [Source.Global] in Siege.Gamemode.State g_siege_state)
			{
				ref var region = ref info.GetRegion();

				var count = Math.Min(g_siege.max_characters_per_player, (uint)platoon.characters.Length);
				var species_tmp = new ISpecies.Handle("human");

				Span<IOrigin.Handle> origins = stackalloc IOrigin.Handle[32];
				IOrigin.Database.GetHandles(ref origins, (x) =>
				{
					return x.data.species == species_tmp;
				});

				for (int i = 0; i < count; i++)
				{
					//var h_origin = default(IOrigin.Handle);

					ref var h_character = ref platoon.characters[i];
					h_character = Dormitory.CreateCharacter(ref region, ref random, origins.GetRandom(ref random));

					var definition = h_character.GetDefinition();
					if (definition != null)
					{
						definition.Flags.SetFlag(Asset.Flags.No_Save, true);

						ref var character_data = ref definition.GetData();
						if (character_data.IsNotNull())
						{
							ref var origin_data = ref character_data.origin.GetData();
							if (origin_data.IsNotNull())
							{
								//character_data.shipments = new Shipment.Item[16];
								var items_span = character_data.items.AsSpan();

								ref var kit_data = ref origin_data.kit_default.GetData();
								if (kit_data.IsNotNull())
								{
									foreach (ref var item in kit_data.shipment.items)
									{
										items_span.Add(item);
									}
								}
							}
						}
					}

					App.WriteLine($"Platoon [{i}]: {h_character}", App.Color.Magenta);
				}

				respawn.Sync(entity);	
			}
#endif
		}
	}
}
