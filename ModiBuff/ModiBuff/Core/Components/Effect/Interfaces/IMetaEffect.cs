namespace ModiBuff.Core
{
	public interface IMetaEffect<in TIn, out TOut> //TODO Rename
	{
		TOut Effect(TIn value, IUnit target, IUnit source);
	}
}