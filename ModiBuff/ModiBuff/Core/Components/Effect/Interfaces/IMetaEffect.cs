namespace ModiBuff.Core
{
	public interface IMetaEffect<TValue>
	{
		Targeting Targeting { get; }

		void Effect(TValue value, IUnit target, IUnit source, bool triggerEvents);
	}
}