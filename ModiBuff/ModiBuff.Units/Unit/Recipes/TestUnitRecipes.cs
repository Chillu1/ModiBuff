namespace ModiBuff.Core.Units
{
	public sealed class TestUnitRecipes : UnitRecipes
	{
		public TestUnitRecipes(ModifierRecipes modifierRecipes) : base(modifierRecipes)
		{
		}

		protected override void SetupRecipes()
		{
			Add("Player")
				.Health(100)
				.Damage(10)
				.Modifiers(
					//TODO Maybe we could only supply name of the modifier?
					new ModifierAddReference(ModifierRecipes.GetRecipe("DisarmChance"), ApplierType.Attack)
				);

			Add("Slime")
				.Health(50)
				.Damage(5)
				.Modifiers(
					new ModifierAddReference(ModifierRecipes.GetRecipe("DoT"), ApplierType.Attack)
				);

			Add("FireSlime")
				.Health(100)
				.Damage(5)
				.Modifiers(
					new ModifierAddReference(ModifierRecipes.GetRecipe("DoT"), ApplierType.Attack),
					new ModifierAddReference(ModifierRecipes.GetRecipe("FireSlimeSelfDoT"))
				);
		}
	}
}