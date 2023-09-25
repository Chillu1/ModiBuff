using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public readonly struct ModifierRecipeData
	{
		public readonly int Id;
		public readonly string Name;
		public readonly List<EffectWrapper> EffectWrappers;
		public readonly EffectWrapper RemoveEffectWrapper;
		public readonly EffectWrapper CallbackRegisterWrapper;
		public readonly bool HasApplyChecks;
		public readonly List<ICheck> ApplyCheckList;
		public readonly bool HasEffectChecks;
		public readonly List<ICheck> EffectCheckList;
		public readonly List<Func<IUnit, bool>> ApplyFuncCheckList;
		public readonly List<Func<IUnit, bool>> EffectFuncCheckList;
		public readonly bool IsAura;
		public readonly bool OneTimeInit;
		public readonly float Interval;
		public readonly bool IntervalAffectedByStatusResistance;
		public readonly float Duration;
		public readonly bool RefreshDuration;
		public readonly bool RefreshInterval;
		public readonly WhenStackEffect WhenStackEffect;
		public readonly float StackValue;
		public readonly int MaxStacks;
		public readonly int EveryXStacks;

		public ModifierRecipeData(int id, string name, List<EffectWrapper> effectWrappers, EffectWrapper removeEffectWrapper,
			EffectWrapper callbackRegisterWrapper, bool hasApplyChecks, List<ICheck> applyCheckList, bool hasEffectChecks,
			List<ICheck> effectCheckList, List<Func<IUnit, bool>> applyFuncCheckList, List<Func<IUnit, bool>> effectFuncCheckList,
			bool isAura, bool oneTimeInit, float interval, bool intervalAffectedByStatusResistance, float duration, bool refreshDuration,
			bool refreshInterval, WhenStackEffect whenStackEffect, float stackValue, int maxStacks, int everyXStacks)
		{
			Id = id;
			Name = name;
			EffectWrappers = effectWrappers;
			RemoveEffectWrapper = removeEffectWrapper;
			CallbackRegisterWrapper = callbackRegisterWrapper;
			HasApplyChecks = hasApplyChecks;
			ApplyCheckList = applyCheckList;
			HasEffectChecks = hasEffectChecks;
			EffectCheckList = effectCheckList;
			ApplyFuncCheckList = applyFuncCheckList;
			EffectFuncCheckList = effectFuncCheckList;
			IsAura = isAura;
			OneTimeInit = oneTimeInit;
			Interval = interval;
			IntervalAffectedByStatusResistance = intervalAffectedByStatusResistance;
			Duration = duration;
			RefreshDuration = refreshDuration;
			RefreshInterval = refreshInterval;
			WhenStackEffect = whenStackEffect;
			StackValue = stackValue;
			MaxStacks = maxStacks;
			EveryXStacks = everyXStacks;
		}
	}
}