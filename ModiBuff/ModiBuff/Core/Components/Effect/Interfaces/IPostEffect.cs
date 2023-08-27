namespace ModiBuff.Core
{
	public interface IPostEffect<TValue>
	{
		void Effect(TValue value, IUnit target, IUnit source, bool triggerEvents);
	}
}