using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public readonly struct ModifierRecipeData
	{
		public readonly int Id;
		public readonly string Name;
		public readonly List<EffectWrapper> EffectWrappers;
		public readonly EffectWrapper? RemoveEffectWrapper;
		public readonly EffectWrapper? DispelRegisterWrapper;
		public readonly EffectWrapper[] CallbackUnitRegisterWrappers;
		public readonly EffectWrapper[] CallbackEffectRegisterWrappers;
		public readonly EffectWrapper[] CallbackEffectUnitsRegisterWrappers;
		public readonly bool HasApplyChecks;
		public readonly List<ICheck>? ApplyCheckList;
		public readonly bool HasEffectChecks;
		public readonly List<ICheck>? EffectCheckList;
		public readonly List<Func<IUnit, bool>>? ApplyFuncCheckList;
		public readonly List<Func<IUnit, bool>>? EffectFuncCheckList;
		public readonly bool IsAura;
		public readonly TagType Tag;
		public readonly float Interval;
		public readonly float Duration;
		public readonly bool RefreshDuration;
		public readonly bool RefreshInterval;
		public readonly WhenStackEffect WhenStackEffect;
		public readonly int? MaxStacks;
		public readonly int? EveryXStacks;
		public readonly float? SingleStackTime;
		public readonly float? IndependentStackTime;

		public ModifierRecipeData(int id, string name, List<EffectWrapper> effectWrappers,
			EffectWrapper? removeEffectWrapper, EffectWrapper? dispelRegisterWrapper,
			EffectWrapper[] callbackUnitRegisterWrappers, EffectWrapper[] callbackEffectRegisterWrappers,
			EffectWrapper[] callbackEffectUnitsRegisterWrappers, bool hasApplyChecks, List<ICheck>? applyCheckList,
			bool hasEffectChecks, List<ICheck>? effectCheckList, List<Func<IUnit, bool>>? applyFuncCheckList,
			List<Func<IUnit, bool>>? effectFuncCheckList, bool isAura, TagType tag, float interval,
			float duration, bool refreshDuration, bool refreshInterval, WhenStackEffect whenStackEffect, int? maxStacks,
			int? everyXStacks, float? singleStackTime, float? independentStackTime)
		{
			Id = id;
			Name = name;
			EffectWrappers = effectWrappers;
			RemoveEffectWrapper = removeEffectWrapper;
			DispelRegisterWrapper = dispelRegisterWrapper;
			CallbackUnitRegisterWrappers = callbackUnitRegisterWrappers;
			CallbackEffectRegisterWrappers = callbackEffectRegisterWrappers;
			CallbackEffectUnitsRegisterWrappers = callbackEffectUnitsRegisterWrappers;
			HasApplyChecks = hasApplyChecks;
			ApplyCheckList = applyCheckList;
			HasEffectChecks = hasEffectChecks;
			EffectCheckList = effectCheckList;
			ApplyFuncCheckList = applyFuncCheckList;
			EffectFuncCheckList = effectFuncCheckList;
			IsAura = isAura;
			Tag = tag;
			Interval = interval;
			Duration = duration;
			RefreshDuration = refreshDuration;
			RefreshInterval = refreshInterval;
			WhenStackEffect = whenStackEffect;
			MaxStacks = maxStacks;
			EveryXStacks = everyXStacks;
			SingleStackTime = singleStackTime;
			IndependentStackTime = independentStackTime;
		}
	}
}