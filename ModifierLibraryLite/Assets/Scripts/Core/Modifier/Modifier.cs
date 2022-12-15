using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo("ModifierLibraryLite.Tests")]

namespace ModifierLibraryLite.Core
{
	[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	public sealed class Modifier : IModifier
	{
		public int Id { get; }
		public string Name { get; }

		private readonly bool _init, _time, _refresh, _stack;

		[CanBeNull]
		private readonly IInitComponent _initComponent;

		[CanBeNull]
		private readonly ITimeComponent[] _timeComponents;

		[CanBeNull]
		private readonly IStackComponent _stackComponent;

		[CanBeNull]
		private readonly StackEffects _stackEffects;

		private TargetComponent _targetComponent;
		private IRemoveModifier _removeModifier;

		public Modifier(ModifierInternalRecipe recipe) : this(recipe.Id, recipe.Name, recipe.InitComponent, recipe.TimeComponents,
			recipe.StackComponent, recipe.StackEffects)
		{
		}

		internal Modifier(int id, string name, InitComponent initComponent, ITimeComponent[] timeComponents,
			IStackComponent stackComponent, StackEffects stackEffects)
		{
			Id = id;
			Name = name;

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
				_stackComponent = stackComponent.ShallowClone();
				_stackEffects = stackEffects;
				_stack = true;
			}
		}

		public void SetupModifierRemove(IRemoveModifier removeModifier)
		{
			_removeModifier = removeModifier;
		}

		public void SetTargets(IUnit target, IUnit owner, IUnit sender)
		{
			var targetComponent = new TargetComponent(sender, owner, target);

			if (_init)
				_initComponent.SetupTarget(targetComponent);

			if (_time)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].SetupTarget(_targetComponent);
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

			if (_stackComponent.Stack())
				_stackEffects.StackEffect(_stackComponent.Stacks, _targetComponent.Target, _targetComponent.Owner);
		}

		public void SetForRemoval()
		{
			_removeModifier?.PrepareRemove(this);
		}

		public void ResetState()
		{
			//_removeModifier = null;
			if (_time)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].ResetState();
			//No need to reset targetComponent references, because we overwrite them on SetTargets
		}
	}
}