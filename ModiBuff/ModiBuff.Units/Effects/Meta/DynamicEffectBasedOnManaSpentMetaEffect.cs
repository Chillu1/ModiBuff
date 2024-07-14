namespace ModiBuff.Core.Units
{
	public class DynamicEffectBasedOnManaSpentMetaEffect : IMetaEffect<float, float>
	{
		private readonly (float mana, float value)[] _values;
		private readonly Targeting _targeting;

		public DynamicEffectBasedOnManaSpentMetaEffect((float mana, float value)[] values,
			Targeting targeting = Targeting.TargetSource)
		{
			_values = values;
			_targeting = targeting;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);

			var manaOwner = (IManaOwner<float, float>)source;
			for (int i = _values.Length - 1; i >= 0; i--)
			{
				var valuePair = _values[i];
				if (valuePair.mana <= manaOwner.Mana)
				{
					manaOwner.UseMana(valuePair.mana);
					return value + valuePair.value;
				}
			}

			return value;
		}
	}
}