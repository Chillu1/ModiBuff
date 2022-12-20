using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo("ModiBuff.Tests")]
[assembly: InternalsVisibleTo("ModiBuff.Core.Units")]

namespace ModiBuff.Core
{
	[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	public sealed class Modifier : IModifier
	{
		public int Id { get; }
		public string Name { get; }

		private readonly bool _init, _time, _refresh, _stack;

		[CanBeNull]
		private readonly InitComponent _initComponent;

		[CanBeNull]
		private readonly ITimeComponent[] _timeComponents;

		[CanBeNull]
		private readonly IStackComponent _stackComponent;

		private IRemoveModifier _removeModifier;
		private TargetComponent _targetComponent;

		public Modifier(ModifierInternalRecipe recipe) : this(recipe.Id, recipe.Name, recipe.InitComponent, recipe.TimeComponents,
			recipe.StackComponent)
		{
		}

		internal Modifier(int id, string name, InitComponent initComponent, ITimeComponent[] timeComponents, IStackComponent stackComponent)
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
				_stack = true;
			}
		}

		public void SetupModifierRemove(IRemoveModifier removeModifier)
		{
			_removeModifier = removeModifier;
		}

		public void SetTargets(IUnit target, IUnit owner, IUnit sender)
		{
			_targetComponent = new TargetComponent(sender, owner, target);

			if (_time)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].SetupTarget(_targetComponent);

			if (_stack)
				_stackComponent.SetupTarget(_targetComponent);
		}

		public void Init()
		{
			if (!_init)
				return;

			_initComponent.Init(_targetComponent.Target, _targetComponent.Owner);
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
			_removeModifier?.PrepareRemove(this);
		}

		public void ResetState()
		{
			//_removeModifier = null;
			if (_time)
				for (int i = 0; i < _timeComponents.Length; i++)
					_timeComponents[i].ResetState();
			if (_stack)
				_stackComponent.ResetState();
			//No need to reset targetComponent references, because we overwrite them on SetTargets
		}
	}
}