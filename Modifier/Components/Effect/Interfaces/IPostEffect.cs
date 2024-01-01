namespace ModiBuff.Core
{
	public interface IPostEffect<in TValue>
	{
		void Effect(TValue value, IUnit target, IUnit source);
	}

	public interface IPostEffect<in TValue, in TValue2>
	{
		void Effect(TValue value, TValue2 value2, IUnit target, IUnit source);
	}
}