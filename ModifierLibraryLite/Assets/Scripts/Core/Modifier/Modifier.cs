using System.Collections.Generic;
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
		private readonly IRefreshComponent _refreshComponent;

		[CanBeNull]
		private readonly IStackComponent _stackComponent;

		public Modifier(ModifierInternalRecipe recipe) : this(recipe.Id, recipe.InitComponent, recipe.TimeComponents,
			recipe.RefreshComponent, recipe.StackComponent, recipe.RemoveEffect)
		{
		}

		internal Modifier(string id, IInitComponent initComponent, ITimeComponent[] timeComponents,
			IRefreshComponent refreshComponent, IStackComponent stackComponent, RemoveEffect removeEffect = null)
		{
			Id = id;

			_initComponent = initComponent;
			var timeComponentList = new List<ITimeComponent>();
			for (int i = 0; i < timeComponents.Length; i++)
				timeComponentList.Add(timeComponents[i].DeepClone());
			_timeComponents = timeComponentList.ToArray();
			_refreshComponent = refreshComponent;
			_stackComponent = stackComponent;

			removeEffect?.Setup(this);

			_init = _initComponent != null;
			_time = _timeComponents != null && _timeComponents.Length > 0;
			_refresh = _refreshComponent != null;
			_stack = _stackComponent != null;
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
			if (!_refresh)
				return;

			_refreshComponent.Refresh();
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
	}
}