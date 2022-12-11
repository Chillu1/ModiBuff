using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo("ModifierLibraryLite.Tests")]

namespace ModifierLibraryLite.Core
{
	[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	public sealed class Modifier : IModifier
	{
		public string Id { get; }
		public bool ToRemove { get; private set; }

		private readonly bool _init, _time, _refresh, _stack;

		[CanBeNull]
		private readonly IInitComponent _initComponent;

		[CanBeNull]
		private readonly ITimeComponent[] _timeComponents;

		[CanBeNull]
		private readonly IStackComponent _stackComponent;

		public Modifier(ModifierInternalRecipe recipe) : this(recipe.Id, recipe.InitComponent, recipe.TimeComponents, recipe.StackComponent,
			recipe.RemoveEffect)
		{
		}

		internal Modifier(string id, IInitComponent initComponent, ITimeComponent[] timeComponents, IStackComponent stackComponent,
			RemoveEffect removeEffect = null)
		{
			Id = id;

			if (initComponent != null)
			{
				_initComponent = initComponent;
				_init = true;
			}

			if (timeComponents != null && timeComponents.Length > 0)
			{
				_timeComponents = new ITimeComponent[timeComponents.Length];
				for (int i = 0; i < timeComponents.Length; i++)
				{
					var timeComponent = timeComponents[i];
					_timeComponents[i] = timeComponent.DeepClone();
					_refresh = _refresh || timeComponent.IsRefreshable;
				}

				_time = true;
			}

			if (stackComponent != null)
			{
				_stackComponent = stackComponent;
				_stack = true;
			}

			removeEffect?.Setup(this);
		}

		public void SetTargets(IUnit target, IUnit owner, IUnit sender)
		{
			var targetComponent = new TargetComponent(sender, owner, target);

			if (_init)
				_initComponent.SetupTarget(targetComponent);

			if (_time)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].SetupTarget(targetComponent);
		}

		public void Init()
		{
			if (!_init)
				return;

			_initComponent.Init();
		}

		public void Update(float deltaTime)
		{
			if (!_time)
				return;

			int length = _timeComponents.Length;
			for (int i = 0; i < length; i++)
				_timeComponents[i].Update(deltaTime);
		}

		public void Refresh()
		{
			if (!_refresh || !_time)
				return;

			int length = _timeComponents.Length;
			for (int i = 0; i < length; i++)
				_timeComponents[i].Refresh();
		}

		public void Stack()
		{
			if (!_stack)
				return;

			_stackComponent.Stack();
		}

		public void SetForRemoval()
		{
			ToRemove = true;
		}

		public void ResetState()
		{
			ToRemove = false;
			if (_time)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].ResetState();
			//No need to reset targetComponent references, because we overwrite them on SetTargets
		}
	}
}