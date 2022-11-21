using System.Collections.Generic;

namespace ModifierLibraryLite.Core
{
	public class TestModifiers
	{
		private readonly IDictionary<string, Modifier> _modifiers;

		public TestModifiers()
		{
			_modifiers = new Dictionary<string, Modifier>();

			{
				var parameters = new ModifierParameters();
				var target = new TargetComponent();
				parameters.SetTimeComponents(
					new TimeComponent(1f, target, new DamageEffect(5)),
					new TimeComponent(5f, target, new RemoveEffect())
				);
				_modifiers.Add("DoTModifier", new Modifier(parameters));
			}
			
			//Refreshable DoT
			{
				var parameters = new ModifierParameters();
				var target = new TargetComponent();
				parameters.SetTimeComponents(
					new TimeComponent(1f, target, new DamageEffect(5)),
					new TimeComponent(5f, target, new RemoveEffect())
				);
				parameters.SetRefreshable();
				_modifiers.Add("RefreshableDoTModifier", new Modifier(parameters));
			}
		}
	}
}