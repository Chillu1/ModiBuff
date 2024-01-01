namespace ModiBuff.Core
{
	public static class UnitExtensions
	{
		public static void ApplyEffect(this IUnit target, int id, IUnit source)
		{
			ModifierLessEffects.Instance.Apply(id, target, source);
		}
	}
}