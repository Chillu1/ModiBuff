namespace ModiBuff.Core
{
	public readonly struct DispelReference
	{
		public readonly RemoveEffect Effect;
		public readonly DispelType Type;

		public DispelReference(RemoveEffect effect, DispelType type)
		{
			Effect = effect;
			Type = type;
		}
	}
}