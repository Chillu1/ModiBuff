namespace ModiBuff.Core.Units
{
	public sealed class CostCheck : IUsableCheck, IDataCheck<CostCheck.Data>
	{
		private readonly CostType _costType;
		private readonly float _cost;

		public CostCheck(CostType costType, float cost)
		{
			_costType = costType;
			_cost = cost;
		}

		public bool Check(IUnit unit)
		{
			switch (_costType)
			{
				case CostType.Health:
					if (!(unit is IDamagable<float, float> damagable) || !(unit is IHealthCost<float>))
					{
#if MODIBUFF_EFFECT_CHECK //TODO This might be an issue/unwanted
						EffectHelper.LogImplError(unit,
							nameof(IDamagable<float, float>) + "or " + nameof(IHealthCost<float>));
#endif
						return false;
					}

					return damagable.Health > _cost;
				case CostType.Mana:
					if (!(unit is IManaOwner<float, float> manaOwner))
					{
#if MODIBUFF_EFFECT_CHECK //TODO This might be an issue/unwanted
						EffectHelper.LogImplError(unit, nameof(IManaOwner<float, float>));
#endif
						return false;
					}

					return manaOwner.Mana >= _cost;
				default:
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("[ModiBuff.Units] Unknown cost type: " + _costType);
#endif
					return false;
			}
		}

		public Data GetData() => new Data(_costType, _cost);

		public void Use(IUnit unit)
		{
			switch (_costType)
			{
				case CostType.Health:
					((IHealthCost<float>)unit).UseHealth(_cost);
					return;
				case CostType.Mana:
					((IManaOwner<float, float>)unit).UseMana(_cost);
					return;
				default:
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("[ModiBuff.Units] Unknown cost type: " + _costType);
#endif
					return;
			}
		}

		public struct Data
		{
			public readonly CostType CostType;
			public readonly float Cost;

			public Data(CostType costType, float cost)
			{
				CostType = costType;
				Cost = cost;
			}
		}
	}
}