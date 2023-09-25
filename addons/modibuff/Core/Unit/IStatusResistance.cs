namespace ModiBuff.Core
{
	public interface IStatusResistance //TODO Not ideal name, since we already use "StatusEffect" for stun, silence, etc.
	{
		/// <summary>
		///		0-1, 1 means 0% resistance, 0 means infinite resistance (never can be 0)
		/// </summary>
		float StatusResistance { get; }
	}
}