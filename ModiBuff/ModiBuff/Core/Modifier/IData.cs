using System;

namespace ModiBuff.Core
{
	public abstract record Data : IData;

	public abstract record ModifierData : Data;

	public sealed record ModifierIntervalData(float Interval) : ModifierData;

	public sealed record ModifierDurationData(float Duration) : ModifierData;

	public abstract record EffectData(Type EffectType, int EffectNumber = 0) : Data;

	//TODO EffectId/EffectType, EffectId 
	public sealed record EffectData<TValue>(TValue Value, Type EffectType, int EffectNumber = 0)
		: EffectData(EffectType, EffectNumber);

	public interface IData
	{
	}
}

namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}