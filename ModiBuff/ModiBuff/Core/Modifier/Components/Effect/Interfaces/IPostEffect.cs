namespace ModiBuff.Core
{
	/// <summary>
	///		Marker interface
	/// </summary>
	public interface IPostEffect
	{
	}

	public interface IPostEffect<in TValue> : IPostEffect
	{
		void Effect(TValue value, IUnit target, IUnit source);
	}

	public interface IPostEffect<in TValue, in TValue2> : IPostEffect
	{
		void Effect(TValue value, TValue2 value2, IUnit target, IUnit source);
	}
}