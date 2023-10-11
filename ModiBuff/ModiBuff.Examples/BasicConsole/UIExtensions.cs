using ModiBuff.Core;
using ModiBuff.Core.Units.Interfaces.NonGeneric;

namespace ModiBuff.Examples.BasicConsole
{
	public static class UIExtensions
	{
		public static void PrintStateAndModifiers(this IModifierOwner owner, ModifierRecipes modifierRecipes)
		{
			var modifierController = owner.ModifierController;
			//Stats, ApplyModifiers, Normal modifiers.
			var damagable = (IDamagable)owner;
			var attacker = (IAttacker)owner;
			Console.GameMessage($"Player stats: {damagable.Health}/{damagable.MaxHealth} HP, " +
			                    $"{attacker.Damage} Damage");
			//Appliers
			//Name, description, checks, (states, like cooldown)
			Console.GameMessage("Player attack appliers:");
			var applierAttackIds = modifierController.GetApplierAttackModifierIds();
			for (int i = 0; i < applierAttackIds?.Count; i++)
			{
				var modifierInfo = modifierRecipes.GetModifierInfo(applierAttackIds[i]);
				Console.GameMessage($"{i + 1} - {modifierInfo.DisplayName} - {modifierInfo.Description}");
			}

			Console.GameMessage("Player cast appliers:");
			var applierCastIds = modifierController.GetApplierCastModifierIds();
			for (int i = 0; i < applierCastIds?.Count; i++)
			{
				var modifierInfo = modifierRecipes.GetModifierInfo(applierCastIds[i]);
				Console.GameMessage($"{i + 1} - {modifierInfo.DisplayName} - {modifierInfo.Description}");
			}

			var applierAttackChecks = modifierController.GetApplierAttackCheckModifiers();
			Console.GameMessage("Player attack applier checks:");
			if (applierAttackChecks != null)
				foreach (var applierAttackCheck in applierAttackChecks)
				{
					var modifierInfo = modifierRecipes.GetModifierInfo(applierAttackCheck.Id);
					Console.GameMessage($"{modifierInfo.DisplayName} - {modifierInfo.Description}");
					//TODO Need to get dummy state, probably from ModifierRecipes
				}

			//Normal modifiers
			Console.GameMessage("Player modifiers:");
			var references = modifierController.GetModifierReferences();
			for (int i = 0; i < references.Length; i++)
			{
				var reference = references[i];
				var modifierInfo = modifierRecipes.GetModifierInfo(reference.Id);
				Console.GameMessage($"{i + 1} - {modifierInfo.DisplayName} - {modifierInfo.Description}");
				var modifierDataReference =
					modifierController.GetModifierDataReference(reference.Id, reference.GenId);
				var timers = modifierDataReference.GetTimers();
				for (int j = 0; j < timers?.Length; j++)
				{
					switch (timers[j])
					{
						case IntervalComponent intervalComponent:
							Console.GameMessage(
								$"Interval timer: {intervalComponent.Timer}/{intervalComponent.Time}");
							break;
						case DurationComponent durationComponent:
							Console.GameMessage(
								$"Duration timer: {durationComponent.Timer}/{durationComponent.Time}");
							break;
					}
				}

				var stackReference = modifierDataReference.GetStackReference();
				if (stackReference != null)
					Console.GameMessage($"Stacks: {stackReference.Stacks}/{stackReference.MaxStacks}");
			}
		}
	}
}