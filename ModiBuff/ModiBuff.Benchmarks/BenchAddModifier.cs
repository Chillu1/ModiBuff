using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	/// <summary>
	///		Only tests for subsequent inits, not new modifier each time
	/// </summary>
	public class BenchAddModifier : ModifierBenches
	{
		private ModifierLessInitEffect[] _modifierLessEffects; //Simulated lookup by id
		private Unit _unit;

		private int _noOpModifierId;
		private int _initDamageModifierId;
		private int _modifierLessInitDamageEffectId;
		private int _initStackDamageModifierId;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_modifierLessEffects = new ModifierLessInitEffect[16];
			_modifierLessEffects[0] = new ModifierLessInitEffect(2);
			_modifierLessEffects[1] = new ModifierLessInitEffect(5);
			_modifierLessEffects[2] = new ModifierLessInitEffect(7);

			_unit = new Unit(1_000_000_000, 5);

			_noOpModifierId = IdManager.GetId("NoOpEffect");
			_initDamageModifierId = IdManager.GetId("InitDamage");
			_modifierLessInitDamageEffectId = 1;
			_initStackDamageModifierId = IdManager.GetId("InitStackDamage");
		}

		//[Benchmark]
		public void BenchAddNoOpEffectBench()
		{
			_unit.ModifierController.Add(_noOpModifierId, _unit, _unit);
		}

		[Benchmark]
		public void BenchAddInitDamageBench()
		{
			_unit.ModifierController.Add(_initDamageModifierId, _unit, _unit);
		}

		private sealed class ModifierLessInitEffect
		{
			private readonly float _damage;

			public ModifierLessInitEffect(float damage)
			{
				_damage = damage;
			}

			public void Effect(IUnit target, IUnit source)
			{
				((IDamagable<float, float, float, float>)target).TakeDamage(_damage, source);
			}
		}

		[Benchmark]
		public void BenchAddInitDamageModifierLessPrototypeBench()
		{
			_modifierLessEffects[_modifierLessInitDamageEffectId].Effect(_unit, _unit);
		}

		//[Benchmark]
		public void BenchAddInitStackDamage()
		{
			_unit.ModifierController.Add(_initStackDamageModifierId, _unit, _unit);
		}
	}
}