namespace ModiBuff.Core
{
	public interface IMetaEffect<TValue>
	{
		void Effect(TValue value, IUnit target, IUnit source, bool triggerEvents);
	}
}