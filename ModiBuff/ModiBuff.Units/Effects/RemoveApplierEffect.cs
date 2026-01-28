namespace ModiBuff.Core.Units
{
	public sealed class RemoveApplierEffect : IEffect, IStackEffect, ISetDataEffect, IShallowClone<IEffect>
	{
		private int _id;
		private ApplierType? _applierType;
		private int? _applierGenId;

		public RemoveApplierEffect(string applierModifierName)
		{
			//Could ask the user to instead supply the id, but that isn't ideal
			int? id = ModifierIdManager.GetIdByName(applierModifierName);
			if (id == null)
				Logger.LogError("[ModiBuff.Units] Can't find modifier with name " + applierModifierName +
								". Either wrong order of modifier initialization or wrong modifier name.");
			_id = id ?? -1;
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static RemoveApplierEffect Create(int id, int? applierGenId, ApplierType? applierType)
		{
			var effect = new RemoveApplierEffect(id, applierGenId, applierType);
			return effect;
		}

		private RemoveApplierEffect(int id, int? applierGenId, ApplierType? applierType)
		{
			_id = id;
			_applierGenId = applierGenId;
			_applierType = applierType;
		}

		//TODO Enforce data setting, aka non-optional
		public void SetData(IData data)
		{
			switch (data)
			{
				case EffectData<ApplierType> effectData:
					_applierType = effectData.Value;
					break;
				case EffectData<int> effectData:
					_applierGenId = effectData.Value;
					break;
				case ModifierData:
					break;
				default:
					Logger.LogError($"[ModiBuff.Units] Unsupported data type: {data.GetType()}");
					break;
			}
		}

		public void Effect(IUnit target, IUnit source)
		{
			((IModifierApplierOwner)target).RemoveApplier(_id, _applierGenId, _applierType.Value);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source) => Effect(target, source);

		public IEffect ShallowClone() => new RemoveApplierEffect(_id, _applierGenId, _applierType);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}