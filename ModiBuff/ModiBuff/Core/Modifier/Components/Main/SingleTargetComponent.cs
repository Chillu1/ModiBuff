namespace ModiBuff.Core
{
	public sealed class SingleTargetComponent : ITargetComponent, ISavable<SingleTargetComponent.SaveData>
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

		public object SaveState() => new SaveData(((IIdOwner)Target).Id, ((IIdOwner)Source).Id);

		public void LoadState(object saveData)
		{
			var data = (SaveData)saveData;
			Target = UnitHelper.GetUnit(data.TargetId);
			Source = UnitHelper.GetUnit(data.SourceId);
		}

		public readonly struct SaveData
		{
			public readonly int TargetId;
			public readonly int SourceId;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(int targetId, int sourceId)
			{
				TargetId = targetId;
				SourceId = sourceId;
			}
		}
	}
}