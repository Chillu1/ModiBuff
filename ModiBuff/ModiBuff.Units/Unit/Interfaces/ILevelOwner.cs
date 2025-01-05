namespace ModiBuff.Core.Units
{
	public interface ILevelOwner
	{
		void AddModifierLevel(int modifierId, int level);
		void SetModifierLevel(int modifierId, int level);
		bool IsLevel(int modifierId, int level);
	}
}