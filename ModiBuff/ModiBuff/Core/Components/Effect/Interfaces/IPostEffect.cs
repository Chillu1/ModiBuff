namespace ModiBuff.Core
{
	public interface IPostEffect<in TValue>
	{
		void Effect(TValue value, IUnit target, IUnit source, bool triggerEvents);
	}
}