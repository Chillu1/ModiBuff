namespace ModiBuff.Core
{
	public interface IEventEffect
	{
		void Effect(IUnit target, IUnit source, float data);
	}
}