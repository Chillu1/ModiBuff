namespace ModiBuff.Core
{
	public interface IMetaEffect<in TIn, out TOut> //TODO Rename
	{
		TOut Effect(TIn value, IUnit target, IUnit source);
	}

	public interface IMetaEffect<in TIn, in TIn2, out TOut> //TODO Rename
	{
		TOut Effect(TIn value, TIn2 value2, IUnit target, IUnit source);
	}
}