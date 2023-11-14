namespace ModiBuff.Core
{
	public sealed class SingleTargetComponent : ITargetComponent
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		public IUnit Source { get; set; }

		public IUnit Target { get; set; }

		public SingleTargetComponent()
		{
		}

		public SingleTargetComponent(IUnit target, IUnit source)
		{
			Target = target;
			Source = source;
		}

		public void ResetState()
		{
			Source = null;
			Target = null;
		}

		public object SaveState() => new SaveData(Target, Source);

		public void LoadState(object saveData)
		{
			var data = (SaveData)saveData;
			Target = data.Target;
			Source = data.Source;
		}

		public readonly struct SaveData
		{
			//public readonly int TargetId;
			//public readonly int SourceId;
			public readonly IUnit Target; //TODO Can't save references to file
			public readonly IUnit Source;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(IUnit target, IUnit source)
			{
				Target = target;
				Source = source;
			}
		}
	}
}