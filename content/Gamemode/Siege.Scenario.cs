﻿using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public interface IScenario: IAsset2<IScenario, IScenario.Data>
	{
		static void IAsset2<IScenario, IScenario.Data>.OnUpdate(IScenario.Definition definition, ref IScenario.Data data_new)
		{

		}

		static void IAsset2<IScenario, IScenario.Data>.OnInit(out string prefix, out string[] extensions, out int capacity_world, out int capacity_region, out int capacity_local)
		{
			prefix = "scenario.";
			extensions = new string[]
			{
				".hjson"
			};

			capacity_world = 64;
			capacity_region = 0;
			capacity_local = 0;
		}

		[Serializable]
		public struct Data
		{
			public IScenario.Wave[] waves;
		}

		[Serializable]
		public struct Wave
		{
			public enum Type: uint
			{
				Undefined = 0,

				Single,
				Recurrent
			}

			public float frequency;
			public float priority;

			public int wave;

			public float duration;
		}
	}
}