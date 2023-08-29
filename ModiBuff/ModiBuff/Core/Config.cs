namespace ModiBuff.Core
{
	public static class Config
	{
		public const int DefaultPoolSize = 64;
		private const int DefaultMaxPoolSize = 16_384;
		private const int DefaultModifierArraySize = 32;
		private const int DefaultModifierRemoveSize = 4;

		private const int DefaultAttackApplierSize = 4,
			DefaultCastApplierSize = 4,
			DefaultAttackCheckApplierSize = 4,
			DefaultCastCheckApplierSize = 4;

		private const float DefaultDeltaTolerance = 0.001f;

		public static int PoolSize = DefaultPoolSize;
		public static int MaxPoolSize = DefaultMaxPoolSize;
		public static int ModifierArraySize = DefaultModifierArraySize;
		public static int AttackApplierSize = DefaultAttackApplierSize;
		public static int CastApplierSize = DefaultCastApplierSize;
		public static int AttackCheckApplierSize = DefaultAttackCheckApplierSize;
		public static int CastCheckApplierSize = DefaultCastCheckApplierSize;
		public static int ModifierRemoveSize = DefaultModifierRemoveSize;

		public static float DeltaTolerance = DefaultDeltaTolerance;
	}
}