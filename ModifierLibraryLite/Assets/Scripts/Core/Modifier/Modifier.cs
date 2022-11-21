using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ModifierLibraryLite.Utilities;
using UnityEngine;

[assembly: InternalsVisibleTo("ModifierLibraryLite.Tests")]

namespace ModifierLibraryLite.Core
{
	//Basic mods
	//DoT
	//DoT refreshable duration

	public class Modifier : IModifier
	{
		public string Id { get; }

		private readonly bool _init, _time, _refresh, _stack;

		[CanBeNull]
		private readonly IInitComponent _initComponent;

		[CanBeNull]
		private readonly ITimeComponent[] _timeComponents;

		[CanBeNull]
		private readonly IRefreshComponent _refreshComponent;

		[CanBeNull]
		private readonly IStackComponent _stackComponent;

		public TargetComponent TargetComponent { get; }


		public Modifier(ModifierInternalRecipe internalRecipe) : this(internalRecipe.Id, internalRecipe.TargetComponent,
			internalRecipe.InitComponent, internalRecipe.TimeComponents, internalRecipe.RefreshComponent, internalRecipe.StackComponent)
		{
		}

		internal Modifier(string id, TargetComponent targetComponent, IInitComponent initComponent, ITimeComponent[] timeComponents,
			IRefreshComponent refreshComponent, IStackComponent stackComponent)
		{
			Id = id;

			TargetComponent = targetComponent;
			_initComponent = initComponent;
			_timeComponents = timeComponents;
			_refreshComponent = refreshComponent;
			_stackComponent = stackComponent;

			_init = _initComponent != null;
			_time = _timeComponents != null && _timeComponents.Length > 0;
			_refresh = _refreshComponent != null;
			_stack = _stackComponent != null;
		}

		//TODO Temporary for testing
		public void SetupTarget(IUnit target)
		{
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
	}
}