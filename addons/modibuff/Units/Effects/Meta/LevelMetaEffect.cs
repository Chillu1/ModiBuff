namespace ModiBuff.Core.Units
{
	//First level approach prototype
	/*public sealed class LevelMetaEffect : IMetaEffect<float, float>
	{
		private readonly float[] _addedValues;
		private readonly Targeting _targeting;

		public LevelMetaEffect(float[] addedValues, Targeting targeting)
		{
			_addedValues = addedValues;
			_targeting = targeting;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);
			//modifier id would need to be fed into the here through the outside effect on generation
			//still probably the best approach to modifier/ability "levels"?
			int level = target.GetLevel(modifierId);
			return value + _addedValues[level];
		}
	}*/
}