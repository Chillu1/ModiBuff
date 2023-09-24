namespace ModiBuff.Core
{
	public readonly struct ModifierAddData
	{
		public readonly bool HasInit;
		public readonly bool HasRefresh;
		public readonly bool HasStack;
		public readonly bool IsInstanceStackable;

		public ModifierAddData(bool hasInit, bool hasRefresh, bool hasStack, bool isInstanceStackable)
		{
			HasInit = hasInit;
			HasRefresh = hasRefresh;
			HasStack = hasStack;
			IsInstanceStackable = isInstanceStackable;
		}
	}
}