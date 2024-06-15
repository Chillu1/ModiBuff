using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using IDamagable = ModiBuff.Core.Units.Interfaces.NonGeneric.IDamagable;

namespace ModiBuff.Examples.BasicConsole
{
	public static class UIExtensions
	{
		public static void PrintStateAndModifiers(this IModifierApplierOwner owner, IModifierRecipes modifierRecipes)
		{
			var modifierController = ((IModifierOwner)owner).ModifierController;
			var modifierApplierController = owner.ModifierApplierController;
			//Stats, ApplyModifiers, Normal modifiers.
			var damagable = (IDamagable)owner;
			var attacker = (IAttacker)owner;
			Console.GameMessage($"Player stats: {damagable.Health}/{damagable.MaxHealth} HP, " +
			                    $"{attacker.Damage} Damage");
			//Appliers
			//Name, description, checks, (states, like cooldown)
			var applierAttackIds = modifierApplierController.GetApplierAttackModifierIds();
			if (applierAttackIds != null && applierAttackIds.Count > 0)
			{
				Console.GameMessage("Player attack appliers:");
				for (int i = 0; i < applierAttackIds.Count; i++)
				{
					var modifierInfo = modifierRecipes.GetModifierInfo(applierAttackIds[i]);
					Console.GameMessage($"{i + 1} - {modifierInfo.DisplayName} - {modifierInfo.Description}");
				}
			}

			var applierCastIds = modifierApplierController.GetApplierCastModifierIds();
			if (applierCastIds != null && applierCastIds.Count > 0)
			{
				Console.GameMessage("Player cast appliers:");
				for (int i = 0; i < applierCastIds.Count; i++)
				{
					var modifierInfo = modifierRecipes.GetModifierInfo(applierCastIds[i]);
					Console.GameMessage($"{i + 1} - {modifierInfo.DisplayName} - {modifierInfo.Description}");
				}
			}

			var applierAttackChecks = modifierApplierController.GetApplierAttackCheckModifiers();
			if (applierAttackChecks != null && applierAttackChecks.Count > 0)
			{
				Console.GameMessage("Player attack applier checks:");
				foreach (var applierAttackCheck in applierAttackChecks)
				{
					var modifierInfo = modifierRecipes.GetModifierInfo(applierAttackCheck.Id);
					Console.GameMessage($"{modifierInfo.DisplayName} - {modifierInfo.Description}");
					//TODO Need to get dummy state, probably from ModifierRecipes

					//Right now we're getting all the data from the instance, which might not be needed
					//Because most checks are without state
					//But we encounter a small problem with manual generation of modifiers
					var checks = applierAttackCheck.GetChecks();
					for (int i = 0; i < checks.Length; i++)
					{
						switch (checks[i])
						{
							case CostCheck costCheck:
								var data = costCheck.GetData();
								Console.GameMessage($"Cost: {data.Cost} {data.CostType}");
								break;
							case CooldownCheck cooldownCheck:
								var stateData = cooldownCheck.GetData();
								Console.GameMessage($"Cooldown: {stateData.Timer}/{stateData.Cooldown}");
								break;
							case ChanceCheck chanceCheck:
								Console.GameMessage($"Chance: {chanceCheck.GetData() * 100f}%");
								break;
						}
					}
				}
			}

			var applierCastChecks = modifierApplierController.GetApplierCastCheckModifiers();
			if (applierCastChecks != null && applierCastChecks.Count > 0)
			{
				Console.GameMessage("Player attack applier checks:");
				foreach (var applierCastCheck in applierCastChecks)
				{
					var modifierInfo = modifierRecipes.GetModifierInfo(applierCastCheck.Id);
					Console.GameMessage($"{modifierInfo.DisplayName} - {modifierInfo.Description}");
					//TODO Need to get dummy state, probably from ModifierRecipes

					//Right now we're getting all the data from the instance, which might not be needed
					//Because most checks are without state
					//But we encounter a small problem with manual generation of modifiers
					var checks = applierCastCheck.GetChecks();
					for (int i = 0; i < checks.Length; i++)
					{
						switch (checks[i])
						{
							case CostCheck costCheck:
								var data = costCheck.GetData();
								Console.GameMessage($"Cost: {data.Cost} {data.CostType}");
								break;
							case CooldownCheck cooldownCheck:
								var stateData = cooldownCheck.GetData();
								Console.GameMessage($"Cooldown: {stateData.Timer}/{stateData.Cooldown}");
								break;
							case ChanceCheck chanceCheck:
								Console.GameMessage($"Chance: {chanceCheck.GetData() * 100f}%");
								break;
						}
					}
				}
			}

			//Normal modifiers
			var references = modifierController.GetModifierReferences();
			if (references != null && references.Length > 0)
			{
				Console.GameMessage("Player modifiers:");
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
}