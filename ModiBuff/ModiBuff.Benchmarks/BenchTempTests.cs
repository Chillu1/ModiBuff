using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchTempTests : ModifierBenches
	{
		private const int Iterations = (int)1e5;

		private Unit _unit;
		private StatType _statType;
		private Func<IUnit, float> _checkStat;

		private ITargetComponent[] _targetComponents;
		private bool _hasTarget;
		private ITimeComponent[] _timeComponents;
		private bool _hasTime;

		private const float Delta = 0.0167f;
		private Dictionary<int, ModifierCheck> _modifierCastChecksAppliers;
		private Dictionary<int, ModifierCheck> _modifierAttackChecksAppliers;

		private int _timeComponentCount;
		private int _timeComponentIndex;
		private Func<ITimeComponent[]> _timeComponentFunc;

		private bool _isMulti;
		private ITargetComponent _targetComponent;
		private IEffect _noOpEffect;

		private Unit _callbackTarget, _callbackSource;
		private CallbackUnitRegisterEffect<CallbackUnitType> _callbackUnitRegisterEffect;
		private IEffect _callbackNoOpEffect;
		private UnitCallback _unitCallbacks;

		private int _id = 100;
		private int _genId = 5000;
		private int StatusEffectTypeInt = 128;

		private IStackEffect[] _stackEffects;

		private EmptyUnit _emptyUnit;

		private bool _condition, _conditionTwo;

		private List<Dictionary<EnumTest, float>> _all;

		private enum EnumTest
		{
			One,
			Two,
			Three,
		}

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			/*_unit = new Unit();
			_statType = StatType.Mana;

			_checkStat = (unit) =>
			{
				switch (_statType)
				{
					case StatType.Health:
						var damagable = (IDamagable<float, float>)unit;
						return damagable.Health / damagable.MaxHealth;
					case StatType.Mana:
						var manaOwner = (IManaOwner<float, float>)unit;
						return manaOwner.Mana / manaOwner.MaxMana;
					default:
						throw new ArgumentOutOfRangeException();
				}
			};

			_targetComponents = null; // new ITargetComponent[] { new SingleTargetComponent(), new MultiTargetComponent() };
			_hasTarget = false;
			_timeComponents = new ITimeComponent[]
			{
				new IntervalComponent(5, false, null, null, false), new IntervalComponent(5, false, null, null, false),
				new DurationComponent(5, false, null),
			};
			_hasTime = true;

			int applierId = IdManager.GetId("InitDamage_CostMana"),
				applierTwoId = IdManager.GetId("InitDamage_ApplyCondition_HealthAbove100");
			_modifierCastChecksAppliers = new Dictionary<int, ModifierCheck>();
			_modifierAttackChecksAppliers = new Dictionary<int, ModifierCheck>();
			_modifierAttackChecksAppliers.Add(applierId, Pool.RentModifierCheck(applierId));
			_modifierAttackChecksAppliers.Add(applierTwoId, Pool.RentModifierCheck(applierTwoId));

			_timeComponentCount = 0; //2
			_timeComponentFunc = () => null;
			//_timeComponentFunc = () =>
			//{
			//_timeComponentIndex = 0;
			//return new ITimeComponent[_timeComponentCount];
			//};

			_targetComponent = new MultiTargetComponent();
			_isMulti = true;
			_noOpEffect = new NoOpEffect();

			_callbackTarget = new Unit();
			_callbackSource = new Unit();
			_callbackRegisterEffect = new CallbackRegisterEffect<CallbackType>(CallbackType.StrongHit);
			_callbackNoOpEffect = new NoOpEffect();

			_stackEffects = new IStackEffect[]
			{
				new AddDamageEffect(5), new AddDamageEffect(5, true)
			};

			_emptyUnit = new EmptyUnit();

			_condition = false;
			_conditionTwo = true;*/

			_all = new List<Dictionary<EnumTest, float>>();
			for (int i = 0; i < (int)EnumTest.Three; i++)
			{
				var faction = new Dictionary<EnumTest, float>();
				faction.Add(EnumTest.One, i);
				faction.Add(EnumTest.Two, i + 1);
				faction.Add(EnumTest.Three, i + 2);
				_all.Add(faction);
			}
		}

		//[Benchmark(Baseline = true)]
		public void TestSwitch()
		{
			float value;

			switch (_statType)
			{
				case StatType.Health:
					var damagable = (IDamagable<float, float>)_unit;
					value = damagable.Health / damagable.MaxHealth;
					break;
				case StatType.Mana:
					var manaOwner = (IManaOwner<float, float>)_unit;
					value = manaOwner.Mana / manaOwner.MaxMana;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			float test = value;
		}

		//[Benchmark]
		public void TestDelegate()
		{
			float value = _checkStat(_unit);

			float test = value;
		}

		//[Benchmark(Baseline = true)]
		public void ForLoopNotNull()
		{
			if (_targetComponents != null)
				for (int i = 0; i < _targetComponents.Length; i++)
				{
					var targetComponent = _targetComponents[i];
				}

			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
				{
					var timeComponent = _timeComponents[i];
				}
		}

		//[Benchmark]
		public void ForLoopBoolCheck()
		{
			if (_hasTarget)
				for (int i = 0; i < _targetComponents.Length; i++)
				{
					var targetComponent = _targetComponents[i];
				}

			if (_hasTime)
				for (int i = 0; i < _timeComponents.Length; i++)
				{
					var timeComponent = _timeComponents[i];
				}
		}

		//[Benchmark(Baseline = true)]
		public void ForEachLoopNoCheck()
		{
			foreach (var check in _modifierCastChecksAppliers.Values)
				check.Update(Delta);

			foreach (var check in _modifierAttackChecksAppliers.Values)
				check.Update(Delta);
		}

		//[Benchmark]
		public void ForEachLoopCheck()
		{
			if (_modifierCastChecksAppliers.Count > 0)
				foreach (var check in _modifierCastChecksAppliers.Values)
					check.Update(Delta);

			if (_modifierAttackChecksAppliers.Count > 0)
				foreach (var check in _modifierAttackChecksAppliers.Values)
					check.Update(Delta);
		}

		//[Benchmark]
		public void ForEachLoopNoCheckKey()
		{
			foreach (var check in _modifierCastChecksAppliers)
				check.Value.Update(Delta);

			foreach (var check in _modifierAttackChecksAppliers)
				check.Value.Update(Delta);
		}

		//[Benchmark(Baseline = true)]
		public void IfTimeComponentCount()
		{
			ITimeComponent[] timeComponents = null;
			if (_timeComponentCount > 0)
			{
				_timeComponentIndex = 0;
				timeComponents = new ITimeComponent[_timeComponentCount];
			}

			var test = timeComponents;
		}

		//[Benchmark]
		public void FuncTimeComponentCount()
		{
			var timeComponents = _timeComponentFunc();

			var test = timeComponents;
		}

		//[Benchmark]
		public void SwitchMatchClass()
		{
			switch (_targetComponent)
			{
				case SingleTargetComponent single:
					_noOpEffect.Effect(single.Target, single.Source);
					break;
				case MultiTargetComponent multi:
					_noOpEffect.Effect(multi.Targets, multi.Source);
					break;
			}
		}

		//[Benchmark]
		public void SafeCastIsCheckClass()
		{
			if (_targetComponent is SingleTargetComponent single)
			{
				_noOpEffect.Effect(single.Target, single.Source);
			}
			else if (_targetComponent is MultiTargetComponent multi)
			{
				_noOpEffect.Effect(multi.Targets, multi.Source);
			}
		}

		//[Benchmark]
		public void IfCheckSafeCastClass()
		{
			if (!_isMulti)
			{
				var single = (SingleTargetComponent)_targetComponent;
				_noOpEffect.Effect(single.Target, single.Source);
			}
			else
			{
				var multi = (MultiTargetComponent)_targetComponent;
				_noOpEffect.Effect(multi.Targets, multi.Source);
			}
		}

		//[Benchmark]
		public void IfCheckUnSafeCastClass()
		{
			if (!_isMulti)
			{
				var single = Unsafe.As<SingleTargetComponent>(_targetComponent);
				_noOpEffect.Effect(single.Target, single.Source);
			}
			else
			{
				var multi = Unsafe.As<MultiTargetComponent>(_targetComponent);
				_noOpEffect.Effect(multi.Targets, multi.Source);
			}
		}

		//[Benchmark]
		public void CallbackDelegate()
		{
			_unitCallbacks += (target, source) => _callbackNoOpEffect.Effect(target, source);
			_unitCallbacks += (target, source) => _callbackNoOpEffect.Effect(target, source);
			//_callbackRegisterEffect.SetCallback(_unitCallbacks);
			_callbackUnitRegisterEffect.Effect(_callbackTarget, _callbackSource);

			//_callbackTarget.Test();

			_unitCallbacks = null;
		}

		//[Benchmark]
		public void CallbackLocalDelegate()
		{
			UnitCallback unitCallbacks = (target, source) => _callbackNoOpEffect.Effect(target, source);
			unitCallbacks += (target, source) => _callbackNoOpEffect.Effect(target, source);
			//_callbackRegisterEffect.SetCallback(unitCallbacks);
			_callbackUnitRegisterEffect.Effect(_callbackTarget, _callbackSource);

			//_callbackTarget.Test();
		}

		//[Benchmark]
		public void CallbackArray()
		{
			var callbackEffects = new IEffect[2];
			callbackEffects[0] = _callbackNoOpEffect;
			callbackEffects[1] = _callbackNoOpEffect;
			//_callbackRegisterEffect.SetCallbackArray(callbackEffects);
			//_callbackRegisterEffect.EffectTest(_callbackTarget, _callbackSource);

			//_callbackTarget.Test();
		}

		//[Benchmark(OperationsPerInvoke = 1000)]
		public void UsualHash()
		{
			for (int i = 0; i < 1000; i++)
			{
				unchecked
				{
					int hash = _id;
					hash = (hash * 397) ^ _genId;
					hash = (hash * 397) ^ StatusEffectTypeInt;
					int test = hash;
				}
			}
		}

		//[Benchmark(OperationsPerInvoke = 1000)]
		public void CentorHash()
		{
			for (int i = 0; i < 1000; i++)
			{
				unchecked
				{
					int centorOne = (_id + _genId) * (_id + _genId + 1) / 2 + _genId;
					int hash = (centorOne + StatusEffectTypeInt) * (centorOne + StatusEffectTypeInt + 1) / 2 +
					           StatusEffectTypeInt;
					int test = hash;
				}
			}
		}

		//[Benchmark(OperationsPerInvoke = 1000)]
		public void StringLiteralInterpolation()
		{
			float lifeStealPercent = 0.5f;
			for (int i = 0; i < 1000; i++)
			{
				string test = $"Life steal: {lifeStealPercent * 100f}%";
			}
		}

		//[Benchmark(OperationsPerInvoke = 1000)]
		public void StringLiteralAdd()
		{
			float lifeStealPercent = 0.5f;
			for (int i = 0; i < 1000; i++)
			{
				string test = "Life steal: " + lifeStealPercent * 100f + "%";
			}
		}

		//[Benchmark]
		public void LinqWhere()
		{
			var revertEffects = _stackEffects
				.Where(x => x is IStackRevertEffect stackRevertEffect && stackRevertEffect.IsStackRevertible)
				.Cast<IStackRevertEffect>().ToArray();
		}

		//[Benchmark]
		public void TempList()
		{
			var revertEffectsList = new List<IStackRevertEffect>();
			for (int i = 0; i < _stackEffects.Length; i++)
			{
				if (_stackEffects[i] is IStackRevertEffect stackRevertEffect && stackRevertEffect.IsStackRevertible)
					revertEffectsList.Add(stackRevertEffect);
			}

			var revertEffects = revertEffectsList.ToArray();
		}

		//[Benchmark]
		public void ArrayPool()
		{
			var revertEffectsTemp = ArrayPool<IStackRevertEffect>.Shared.Rent(_stackEffects.Length);
			int revertEffectIndex = 0;
			for (int i = 0; i < _stackEffects.Length; i++)
			{
				if (_stackEffects[i] is IStackRevertEffect stackRevertEffect && stackRevertEffect.IsStackRevertible)
					revertEffectsTemp[revertEffectIndex++] = stackRevertEffect;
			}

			var revertEffects = revertEffectsTemp.AsSpan(0, revertEffectIndex).ToArray();
			ArrayPool<IStackRevertEffect>.Shared.Return(revertEffectsTemp);
		}

		//[Benchmark]
		public void IfElse()
		{
			bool exists;
			int index;
			if (_condition)
			{
				index = _conditionTwo ? 1 : -1;
				exists = index != -1;
			}
			else
			{
				index = _conditionTwo ? 2 : -1;
				exists = index != -1;
			}

			if (exists)
			{
				return;
			}
		}

		//[Benchmark]
		public void OverWriteIf()
		{
			int index;
			if (_condition)
				index = _conditionTwo ? 1 : -1;
			else
				index = _conditionTwo ? 2 : -1;

			if (index != -1)
				return;
		}

		[Benchmark]
		public void ForeachLoopDictCheck()
		{
			(Dictionary<EnumTest, float>, float)? factionWithHighestResource = null;
			float highestResource = float.MinValue;
			EnumTest enumTest = EnumTest.Three;

			foreach (var dict in _all)
			{
				if (dict[enumTest] > highestResource)
				{
					highestResource = dict[enumTest];
					factionWithHighestResource = (dict, highestResource);
				}
			}

			var test = factionWithHighestResource;
		}

		[Benchmark]
		public void ForLoopDictCheck()
		{
			(Dictionary<EnumTest, float>, float)? factionWithHighestResource = null;
			float highestResource = float.MinValue;
			EnumTest enumTest = EnumTest.Three;

			for (int i = 0; i < _all.Count; i++)
			{
				Dictionary<EnumTest, float> dict = _all[i];
				if (dict[enumTest] > highestResource)
				{
					highestResource = dict[enumTest];
					factionWithHighestResource = (dict, highestResource);
				}
			}

			var test = factionWithHighestResource;
		}

		[Benchmark]
		public void AggregateDictCheck()
		{
			var factionWithHighestFood = _all.Aggregate((f1, f2) =>
				f1[EnumTest.Three] > f2[EnumTest.Three] ? f1 : f2);

			var test = factionWithHighestFood;
		}
	}
}