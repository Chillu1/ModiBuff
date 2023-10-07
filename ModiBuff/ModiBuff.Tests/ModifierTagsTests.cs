using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierTagsTests : ModifierTests
	{
		[Test]
		public void StatusResistanceTag_Duration()
		{
			var tag = TagType.DurationStatusResistance;
			AddGenerator("DurationDamage", (id, genId, name) =>
			{
				var damage = new DamageEffect(5f);
				var duration = new DurationComponent(1f, false, new IEffect[] { damage },
					tag.HasTag(TagType.DurationStatusResistance));

				return new Modifier(id, genId, name, null, new ITimeComponent[] { duration },
					null, null, new SingleTargetComponent(), null);
			}, new ModifierAddData(false, false, false, false), tag);
			Setup();

			Unit.ChangeStatusResistance(0.5f);
			Unit.AddModifierSelf("DurationDamage");

			Unit.Update(0.5f);
			Assert.AreEqual(UnitHealth - 5f, Unit.Health);
		}
	}
}