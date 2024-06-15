namespace ModiBuff.Core.Units
{
	public sealed class TestUnitRecipes : UnitRecipes
	{
		public TestUnitRecipes(ModifierRecipes modifierRecipes) : base(modifierRecipes)
		{
		}

		protected override void SetupRecipes()
		{
			Add("Player", UnitType.Good)
				.Health(100)
				.Damage(10)
				.Modifiers(("DisarmChance", ApplierType.Attack));

			Add("Slime", UnitType.Bad)
				.Health(50)
				.Damage(5)
				.Modifiers(("DoT", ApplierType.Attack));

			Add("FireSlime", UnitType.Bad)
				.Health(100)
				.Damage(5)
				.Modifiers(
					("DoT", ApplierType.Attack),
					("FireSlimeSelfDoT", ApplierType.None)
				);
		}
	}
}