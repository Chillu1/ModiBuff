using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public sealed class MultiTargetComponent : ITargetComponent, ISavable<MultiTargetComponent.SaveData>
	{
		public IUnit Source { get; set; }
		public IList<IUnit> Targets { get; private set; }

		public MultiTargetComponent()
		{
			Targets = new List<IUnit>(Config.MultiTargetComponentInitialCapacity);
		}

		public MultiTargetComponent(IList<IUnit> targets, IUnit source)
		{
			Source = source;
			Targets = targets;
		}

		public void UpdateTargets(IList<IUnit> targets) => Targets = targets;

		public void ResetState()
		{
			Source = null;
			Targets = null;
		}

		public object SaveState() => new SaveData(
			Targets.Select(t => t switch
			{
				IIdOwner<ulong> idOwner => (object)idOwner.Id,
				IIdOwner<long> idOwner => idOwner.Id,
				IIdOwner<uint> idOwner => idOwner.Id,
				IIdOwner<int> idOwner => idOwner.Id,
				IIdOwner<short> idOwner => idOwner.Id,
				IIdOwner<ushort> idOwner => idOwner.Id,
				IIdOwner<sbyte> idOwner => idOwner.Id,
				IIdOwner<byte> idOwner => idOwner.Id,
				_ => -1
			}).ToArray(),
			Source switch
			{
				IIdOwner<ulong> idOwner => idOwner.Id,
				IIdOwner<long> idOwner => idOwner.Id,
				IIdOwner<uint> idOwner => idOwner.Id,
				IIdOwner<int> idOwner => idOwner.Id,
				IIdOwner<short> idOwner => idOwner.Id,
				IIdOwner<ushort> idOwner => idOwner.Id,
				IIdOwner<sbyte> idOwner => idOwner.Id,
				IIdOwner<byte> idOwner => idOwner.Id,
				_ => -1
			}
		);

		public void LoadState(object saveData)
		{
			//Might want to clear in case we add ourselves as aura target on load
			//Then we add it back after
			Targets.Clear();

			var data = (SaveData)saveData;
			for (int i = 0; i < data.targetIds.Count; i++)
				Targets.Add(UnitHelper.GetUnit(data.targetIds[i])!);
			Source = UnitHelper.GetUnit(data.sourceId)!;
		}

		public readonly struct SaveData
		{
			public readonly IReadOnlyList<object> targetIds;
			public readonly object sourceId;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(IReadOnlyList<object> targetIds, object sourceId)
			{
				this.targetIds = targetIds;
				this.sourceId = sourceId;
			}
		}
	}
}