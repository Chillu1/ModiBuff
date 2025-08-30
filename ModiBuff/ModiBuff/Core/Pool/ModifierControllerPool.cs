using System;

namespace ModiBuff.Core
{
	public sealed class ModifierControllerPool
	{
		public static ModifierControllerPool? Instance { get; private set; }

		public static readonly int MaxPoolSize = Config.MaxModifierControllerPoolSize;

		private ModifierController[] _pool;
		private int _poolTop;

		public ModifierControllerPool()
		{
			if (Instance != null)
				return;

			Instance = this;

			int initialSize = Math.Max(Config.ModifierControllerPoolSize, 1);
			_pool = new ModifierController[initialSize];
			for (int i = 0; i < initialSize; i++)
				_pool[i] = new ModifierController();
			_poolTop = initialSize;
		}

		public ModifierController Rent()
		{
			if (_poolTop == 0)
				AllocateDouble();

			return _pool[--_poolTop];
		}

		public void Return(ModifierController modifierController)
		{
			modifierController.Clear();

			if (_poolTop == _pool.Length)
				Array.Resize(ref _pool, _pool.Length << 1);

			_pool[_poolTop++] = modifierController;
		}

		private void AllocateDouble()
		{
			if (_poolTop == _pool.Length)
				Array.Resize(ref _pool, _pool.Length << 1);

			for (int i = _poolTop; i < _pool.Length; i++)
				_pool[i] = new ModifierController();
			_poolTop = _pool.Length;

			if (_poolTop > MaxPoolSize)
				throw new Exception($"ModifierControllerPool reached max size of {MaxPoolSize}");
		}

		internal void Clear()
		{
			Array.Clear(_pool, 0, _pool.Length);
			_poolTop = 0;
		}

		public void Reset()
		{
			if (_pool == null)
				return;

			Clear();

			Instance = null;
		}
	}
}