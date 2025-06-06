using System;

namespace ModiBuff.Core
{
	public enum DataType
	{
		Modifier = 1,
		Effect,
	}

	public abstract record Data : IData;

	public sealed record ModifierData : Data;

	//TODO EffectId/EffectType, EffectId 
	public sealed record EffectData<TValue>(TValue Value, Type EffectType, int EffectNumber = 0) : Data;

	public interface IData<TData> : IData
	{
		(DataType, TData) Data { get; }
	}

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