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

		public object SaveState()
		{
			switch (Target, Source)
			{
				case (IIdOwner<ulong> target, IIdOwner<ulong> source):
					return new SaveData(target.Id, source.Id);
				case (IIdOwner<long> target, IIdOwner<long> source):
					return new SaveData(target.Id, source.Id);
				case (IIdOwner<uint> target, IIdOwner<uint> source):
					return new SaveData(target.Id, source.Id);
				case (IIdOwner<int> target, IIdOwner<int> source):
					return new SaveData(target.Id, source.Id);
				case (IIdOwner<short> target, IIdOwner<short> source):
					return new SaveData(target.Id, source.Id);
				case (IIdOwner<ushort> target, IIdOwner<ushort> source):
					return new SaveData(target.Id, source.Id);
				case (IIdOwner<sbyte> target, IIdOwner<sbyte> source):
					return new SaveData(target.Id, source.Id);
				case (IIdOwner<byte> target, IIdOwner<byte> source):
					return new SaveData(target.Id, source.Id);
			}

			Logger.LogError(
				"[ModiBuff] SingleTargetComponent.SaveState: Target and Source must implement IIdOwner<TId> where TId is int or long.");
			return new SaveData(-1, -1);
		}

		public void LoadState(object saveData)
		{
			var data = (SaveData)saveData;
			Target = UnitHelper.GetUnit(data.TargetId)!;
			Source = UnitHelper.GetUnit(data.SourceId)!;
		}

		public readonly struct SaveData
		{
			public readonly object TargetId;
			public readonly object SourceId;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(object targetId, object sourceId)
			{
				TargetId = targetId;
				SourceId = sourceId;
			}
		}
	}
}