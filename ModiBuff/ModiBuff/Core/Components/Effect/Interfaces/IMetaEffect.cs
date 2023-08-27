namespace ModiBuff.Core
{
	public interface IMetaEffect<TIn, TOut> //TODO Rename
	{
		TOut Effect(TIn value, IUnit target, IUnit source);
	}
}