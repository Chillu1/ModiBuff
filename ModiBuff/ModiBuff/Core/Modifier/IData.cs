using System;

namespace ModiBuff.Core
{
	public abstract record Data : IData;

	public abstract record ModifierData : Data;

	public sealed record ModifierIntervalData(float Interval) : ModifierData;

	public sealed record ModifierDurationData(float Duration) : ModifierData;

	//TODO EffectId/EffectType, EffectId 
	public sealed record EffectData<TValue>(TValue Value, Type EffectType, int EffectNumber = 0) : Data;

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