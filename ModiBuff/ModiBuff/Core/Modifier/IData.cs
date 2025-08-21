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

	public abstract record EffectData((Type EffectType, int EffectNumber)? Data) : Data;

	public sealed record EffectData<TValue>(TValue Value, (Type EffectType, int EffectNumber)? Data = null)
		: EffectData(Data);
}

namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}