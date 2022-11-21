using System.Collections.Generic;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierController : IModifierController
	{
		//TODO Array mapping?
		private readonly Dictionary<string, Modifier> _modifiers;

		public ModifierController()
		{
			_modifiers = new Dictionary<string, Modifier>();
		}

		public void Update(in float delta)
		{
			//int length = _modifiers.Count;
			//TODO Array for loop mapping
			foreach (var modifier in _modifiers.Values)
				modifier.Update(delta);
		}

		//TODO do appliers make sense? Should we just store the id, what kind of state do appliers have?
		public (bool, Modifier) TryAdd(Modifier modifier)
		{
			//TODO We should call the original modifier's check component here or before
			return (true, Add(modifier));
		}

		private Modifier Add(Modifier modifier)
		{
			//TODO We should call the original modifier's check component before

			if (_modifiers.TryGetValue(modifier.Id, out var existingModifier))
			{
				existingModifier.Init();
				existingModifier.Refresh();
				existingModifier.Stack();
				return existingModifier;
			}

			_modifiers.Add(modifier.Id, modifier);
			modifier.Init();
			modifier.Refresh();
			modifier.Stack();
			return modifier;
		}
	}
}