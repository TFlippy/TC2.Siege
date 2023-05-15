using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
		[Query]
		public delegate void GetAllTargetsQuery(ISystem.Info info, Entity entity, [Source.Owned] in Siege.Target.Data target, [Source.Owned] in Transform.Data transform, [Source.Owned, Optional] in Faction.Data faction);

		[Query]
		public delegate void GetAllUnitsQuery(ISystem.Info info, Entity entity, [Source.Owned] in Commandable.Data commandable, [Source.Owned, Override] in AI.Movement movement, [Source.Owned, Override] in AI.Behavior behavior, [Source.Owned] in Transform.Data transform, [Source.Owned] in Faction.Data faction);

		private struct FindTargetArgs
		{
			public Entity ent_search;
			public IFaction.Handle faction_id;
			public Vector2 position;
			public Entity ent_root;
			public Entity ent_target;
			public float target_dist_nearest_sq;
			public Vector2 target_position;

			public FindTargetArgs(Entity ent_search, IFaction.Handle faction_id, Vector2 position, Entity ent_root, Entity ent_target, float target_dist_nearest_sq, Vector2 target_position)
			{
				this.ent_search = ent_search;
				this.faction_id = faction_id;
				this.position = position;
				this.ent_root = ent_root;
				this.ent_target = ent_target;
				this.target_dist_nearest_sq = target_dist_nearest_sq;
				this.target_position = target_position;
			}
		}

		private struct GetAllUnitsQueryArgs
		{
			public Entity ent_search;
			public Entity ent_target;
			public IFaction.Handle faction_id;
			public Vector2 position;
			public Vector2 target_position;
			public int selection_count;
			public int wave_size_rem;
			public FixedArray4<EntRef<Commandable.Data>> selection;

			public GetAllUnitsQueryArgs(Entity ent_search, Entity ent_target, IFaction.Handle faction_id, Vector2 position, Vector2 target_position, int selection_count, int wave_size_rem, FixedArray4<EntRef<Commandable.Data>> selection)
			{
				this.ent_search = ent_search;
				this.ent_target = ent_target;
				this.faction_id = faction_id;
				this.position = position;
				this.target_position = target_position;
				this.selection_count = selection_count;
				this.wave_size_rem = wave_size_rem;
				this.selection = selection;
			}
		}
	}
}
