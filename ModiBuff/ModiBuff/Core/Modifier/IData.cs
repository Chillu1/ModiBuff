using System;

namespace ModiBuff.Core
{
	public interface IData
	{
	}

	public abstract record Data : IData;

	public abstract record ModifierData : Data;

	public sealed record ModifierIntervalData(float Interval) : ModifierData;

	public sealed record ModifierDurationData(float Duration) : ModifierData;

	public sealed record ModifierStartingStacksData(int Stacks) : ModifierData;

	public abstract record EffectData(Type EffectType, int EffectNumber = 0) : Data;

	//TODO EffectId/EffectType, EffectId 
	public sealed record EffectData<TValue>(TValue Value, Type EffectType, int EffectNumber = 0)
		: EffectData(EffectType, EffectNumber);
}

namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}