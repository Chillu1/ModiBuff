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
			Target = UnitHelper.GetUnit(data.TargetId)!;
			Source = UnitHelper.GetUnit(data.SourceId)!;
		}

		public readonly struct SaveData
		{
			public readonly int TargetId;
			public readonly int SourceId;

#if MODIBUFF_SYSTEM_TEXT_JSON
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