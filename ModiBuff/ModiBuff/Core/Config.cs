using System.Collections.Generic;

namespace ModiBuff.Core
{
	public static class Config
	{
		public const int DefaultDefaultTag = (int)TagType.Default;

		private const int DefaultPoolSize = 64;
		private const int DefaultMaxPoolSize = 16_384;

		private const int DefaultModifierArraySize = 32;
		private const int DefaultModifierRemoveSize = 4;

		private const int DefaultMultiTargetComponentInitialCapacity = 4;

		private const int DefaultAttackApplierSize = 4,
			DefaultCastApplierSize = 4,
			DefaultAttackCheckApplierSize = 4,
			DefaultCastCheckApplierSize = 4;

		private const float DefaultDeltaTolerance = 0.001f;


		public static int DefaultTag = DefaultDefaultTag;

		public static int PoolSize = DefaultPoolSize;
		public static int MaxPoolSize = DefaultMaxPoolSize;

		public static int ModifierArraySize = DefaultModifierArraySize;
		public static int ModifierRemoveSize = DefaultModifierRemoveSize;

		public static int MultiTargetComponentInitialCapacity = DefaultMultiTargetComponentInitialCapacity;

		public static int AttackApplierSize = DefaultAttackApplierSize;
		public static int CastApplierSize = DefaultCastApplierSize;
		public static int AttackCheckApplierSize = DefaultAttackCheckApplierSize;
		public static int CastCheckApplierSize = DefaultCastCheckApplierSize;

		public static float DeltaTolerance = DefaultDeltaTolerance;

		/// <summary>
		///		How many modifiers should the pool allocate for each modifier type. Should be a power of 2.
		/// </summary>
		public static Dictionary<string, int> ModifierAllocationsCount;

		static Config()
		{
			ModifierAllocationsCount = new Dictionary<string, int>();
		}

		public static void Set(int defaultTag = DefaultDefaultTag, int poolSize = DefaultPoolSize,
			int maxPoolSize = DefaultMaxPoolSize, int modifierArraySize = DefaultModifierArraySize,
			int modifierRemoveSize = DefaultModifierRemoveSize,
			int multiTargetComponentInitialCapacity = DefaultMultiTargetComponentInitialCapacity,
			int attackApplierSize = DefaultAttackApplierSize, int castApplierSize = DefaultCastApplierSize,
			int attackCheckApplierSize = DefaultAttackCheckApplierSize,
			int castCheckApplierSize = DefaultCastCheckApplierSize, float deltaTolerance = DefaultDeltaTolerance)
		{
			DefaultTag = defaultTag;

			PoolSize = poolSize;
			MaxPoolSize = maxPoolSize;

			ModifierArraySize = modifierArraySize;
			ModifierRemoveSize = modifierRemoveSize;

			MultiTargetComponentInitialCapacity = multiTargetComponentInitialCapacity;

			AttackApplierSize = attackApplierSize;
			CastApplierSize = castApplierSize;
			AttackCheckApplierSize = attackCheckApplierSize;
			CastCheckApplierSize = castCheckApplierSize;

			DeltaTolerance = deltaTolerance;
		}

		public static void Reset()
		{
			DefaultTag = DefaultDefaultTag;

			PoolSize = DefaultPoolSize;
			MaxPoolSize = DefaultMaxPoolSize;

			ModifierArraySize = DefaultModifierArraySize;
			ModifierRemoveSize = DefaultModifierRemoveSize;

			MultiTargetComponentInitialCapacity = DefaultMultiTargetComponentInitialCapacity;

			AttackApplierSize = DefaultAttackApplierSize;
			CastApplierSize = DefaultCastApplierSize;
			AttackCheckApplierSize = DefaultAttackCheckApplierSize;
			CastCheckApplierSize = DefaultCastCheckApplierSize;

			DeltaTolerance = DefaultDeltaTolerance;
		}
	}
}