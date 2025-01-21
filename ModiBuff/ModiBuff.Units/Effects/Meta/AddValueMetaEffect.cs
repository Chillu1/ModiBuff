using System;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public sealed class AddValueMetaEffect : IConditionEffect, IMetaEffect<float, float>, IMetaEffect<int, int>,
		IMetaEffect<float, int, float>, ISaveableRecipeEffect<AddValueMetaEffect.RecipeSaveData>
	{
		public Condition[] Conditions { get; set; }

		private readonly float _value;

		//TODO IMetaEffect<int> support
		private IMetaEffect<float, float>[] _metaEffects;

		public AddValueMetaEffect(float value) : this(value, null, null)
		{
		}

		private AddValueMetaEffect(float value, IMetaEffect<float, float>[] metaEffects, ICondition[] conditions)
		{
			_value = value;
			_metaEffects = metaEffects;
			Conditions = conditions?.Cast<Condition>().ToArray() ?? Array.Empty<Condition>();
		}

		public AddValueMetaEffect SetMetaEffects(params IMetaEffect<float, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			return this;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			return value + _metaEffects.TryApply(_value, target, source);
		}

		public int Effect(int value, IUnit target, IUnit source) => value + (int)_value;

		public float Effect(float value, int value2, IUnit target, IUnit source) => value + _value;

		public object SaveRecipeState() => new RecipeSaveData(_value, this.GetMetaSaveData(_metaEffects),
			this.GetConditionSaveData(Conditions));

		public readonly struct RecipeSaveData
		{
			public readonly float Value;
			public readonly object[] MetaEffects;
			public readonly object[] Conditions;

			public RecipeSaveData(float value, object[] metaEffects, object[] conditions)
			{
				Value = value;
				MetaEffects = metaEffects;
				Conditions = conditions;
			}
		}
	}
}