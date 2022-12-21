namespace ModiBuff.Core
{
	public interface IEffect
	{
		/// <param name="acter">owner</param>
		void Effect(IUnit target, IUnit acter);
	}
}