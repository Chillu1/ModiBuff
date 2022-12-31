namespace ModiBuff.Core
{
	public interface IEffect
	{
		/// <param name="source">owner/acter</param>
		void Effect(IUnit target, IUnit source);
	}
}