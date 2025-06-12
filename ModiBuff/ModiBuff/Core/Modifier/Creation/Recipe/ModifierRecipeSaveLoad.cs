using System;
using System.Linq;
using System.Reflection;

namespace ModiBuff.Core
{
	public partial class ModifierRecipe
	{
		public SaveData SaveState()
		{
			if (_unsavableEffects.Count > 0)
			{
				var unsupportedInstructionEffects = _unsavableEffects
					.Where(SpecialInstructionEffects.IsUnsupportedEffectType).ToArray();
				var nonSpecialInstructionEffects = _unsavableEffects
					.Where(e => !SpecialInstructionEffects.IsSpecialInstructionEffect(e)).ToArray();

				if (unsupportedInstructionEffects.Length > 0)
					Logger.LogError("[ModiBuff] Saving recipe with unsupported effects: " +
					                string.Join(", ", unsupportedInstructionEffects.Select(e => e.Name)));

				if (nonSpecialInstructionEffects.Length > 0)
					Logger.LogWarning("[ModiBuff] Saving recipe with unsavable effects, please implement " +
					                  $"{nameof(ISaveableRecipeEffect)} for the following effects: " +
					                  string.Join(", ", nonSpecialInstructionEffects.Select(e => e.Name)));
			}

			return new SaveData(_saveInstructions.ToArray());
		}

		public void LoadState<TUnitCallback>(SaveData saveData)
		{
			foreach (var instruction in saveData.Instructions)
			{
				switch (instruction.InstructionId)
				{
					case SaveInstruction.Initialize.Id:
						break;
					case SaveInstruction.InstanceStackable.Id:
						InstanceStackable();
						break;
					case SaveInstruction.Aura.Id:
						Aura();
						break;
					case SaveInstruction.Interval.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Interval(((SaveInstruction.Interval)instruction).Value);
#endif
						break;
					case SaveInstruction.Duration.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Duration(((SaveInstruction.Duration)instruction).Value);
#endif
						break;
					case SaveInstruction.Remove.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var remove = (SaveInstruction.Remove)instruction;
						switch (remove.RemoveType)
						{
							case SaveInstruction.Remove.Type.RemoveOn:
								Remove(remove.EffectOn.ToRemoveEffectOn());
								break;
							case SaveInstruction.Remove.Type.Duration:
								Remove(remove.Duration);
								break;
							default:
								Logger.LogError(
									$"[ModiBuff] Loaded remove instruction with unknown type {remove.RemoveType}");
								break;
						}
#endif
						break;
					case SaveInstruction.Refresh.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Refresh(((SaveInstruction.Refresh)instruction).RefreshType);
#endif
						break;
					case SaveInstruction.Stack.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var stack = (SaveInstruction.Stack)instruction;
						Stack(stack.WhenStackEffect, stack.MaxStacks, stack.EveryXStacks, stack.SingleStackTime,
							stack.IndependentStackTime);
#endif
						break;
					case SaveInstruction.Dispel.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Dispel(((SaveInstruction.Dispel)instruction).Type);
#endif
						break;
					case SaveInstruction.Tag.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var tag = (SaveInstruction.Tag)instruction;
						_ = tag.InstructionTagType switch
						{
							SaveInstruction.Tag.Type.Add => Tag(tag.TagType),
							SaveInstruction.Tag.Type.Remove => RemoveTag(tag.TagType),
							SaveInstruction.Tag.Type.Set => SetTag(tag.TagType),
							_ => throw new ArgumentOutOfRangeException()
						};
#endif
						break;
					case SaveInstruction.CallbackUnit.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						CallbackUnit(
							(TUnitCallback)(object)((SaveInstruction.CallbackUnit)instruction).CallbackUnitType);
#endif
						break;
					case SaveInstruction.Effect.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var (effect, effectOn) = HandleEffect(instruction);
						if (effect != null)
							Effect(effect, effectOn);
#endif
						break;
					case SaveInstruction.ModifierAction.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var action = (SaveInstruction.ModifierAction)instruction;
						ModifierAction(action.ModifierActionFlags, action.EffectOn);
#endif
						break;
					case SaveInstruction.Data.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var data = (SaveInstruction.Data)instruction;
						Data(data.SaveData);
#endif
						break;
					default:
						Logger.LogError($"Unknown instruction with id {instruction.InstructionId}");
						break;
				}
			}

#if MODIBUFF_SYSTEM_TEXT_JSON
			(IEffect?, EffectOn) HandleEffect(SaveInstruction instruction)
			{
				var effect = (SaveInstruction.Effect)instruction;
				bool failed = false;

				int effectId = effect.EffectId;
				EffectOn effectOn = effect.EffectOn;

				var effectType = _effectTypeIdManager.GetEffectType(effectId);
				//TODO Find constructor by saved types? Prob not worth
				var privateConstructors = effectType!.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
				var constructor = privateConstructors.Length > 0
					? privateConstructors[0]
					: effectType.GetConstructors()[0];

				var parameters = constructor.GetParameters();
				object[] effectStates = new object[parameters.Length];
				int i = 0;
				//TODO Refactor entire approach
				foreach (var property in ((System.Text.Json.JsonElement)effect.SaveData).EnumerateObject())
				{
					object? value = null;

					if (!parameters[i].ParameterType.IsArray)
					{
						bool success;
						(success, value) = property.Value.ToValue(parameters[i].ParameterType);
						if (!success)
						{
							Logger.LogError(
								$"[ModiBuff] Failed to load effect state from save data by {Name} for effect {effectId}");
							failed = true;
						}

						effectStates[i] = value;
						i++;
						continue;
					}

					//TODO Refactor
					if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Null)
					{
						effectStates[i] = null;
						i++;
						continue;
					}

					var states = HandleStates(parameters[i].ParameterType, property);
					if (states.Item1 != null)
						value = states.Item1;
					if (states.Item2 != null)
						value = states.Item2;
					if (states.Item3 != null)
						value = states.Item3;
					if (value == null)
					{
						Logger.LogError(
							$"[ModiBuff] Failed to load effect state from save data by {Name} for effect {effectId}");
						failed = true;
					}

					effectStates[i] = value;
					i++;
				}

				if (failed)
					return (null, EffectOn.None);

				return ((IEffect)constructor.Invoke(effectStates), effectOn);

				//TODO Shit's kinda wack, but I'm not bothered refactoring this approach right now
				//Should be restructured/have a better and easier standard than relying on constructors, if possible
				(IMetaEffect<float, float>[]?, IPostEffect<float>[]?, ICondition[]?) HandleStates(
					Type parameterType, System.Text.Json.JsonProperty property)
				{
					//TODO Refactor

					if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Null)
						return (null, null, null);

					object[] objects = new object[property.Value.GetArrayLength()];
					int count = 0;

					foreach (var element in property.Value.EnumerateArray())
					{
						Type conditionType;
						//TODO Add failsafe for missing id, graceful exit
						if (typeof(IMetaEffect[]).IsAssignableFrom(parameterType))
						{
							conditionType = _effectTypeIdManager.GetMetaEffectType(element
								.EnumerateObject()
								.First().Value.GetInt32());
						}
						else if (typeof(IPostEffect[]).IsAssignableFrom(parameterType))
						{
							conditionType = _effectTypeIdManager.GetPostEffectType(element
								.EnumerateObject()
								.First().Value.GetInt32());
						}
						else if (typeof(ICondition[]).IsAssignableFrom(parameterType))
						{
							conditionType = _effectTypeIdManager.GetConditionType(element
								.EnumerateObject()
								.First().Value.GetInt32());
						}
						else
						{
							Logger.LogError(
								$"[ModiBuff] Unknown parameter type {parameterType} for recursive test by {Name} for effect {effectId}");
							continue;
						}

						//TODO
						var recipeSaveData = element.EnumerateObject().ElementAt(1).Value;

						var privateConstructors =
							conditionType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
						var constructor = privateConstructors.Length > 0 &&
						                  //TODO Records have a hidden private constructor?
						                  conditionType.GetMethods().All(m => m.Name != "<Clone>$")
							? privateConstructors[0]
							: conditionType.GetConstructors()[0];
						var parameters = constructor.GetParameters();

						object[] states = new object[parameters.Length];
						int j = 0;
						//TODO recipeSaveData is null if SaveRecipeState() returns null
						foreach (var recipeProperty in recipeSaveData.EnumerateObject())
						{
							if (parameters[j].ParameterType.IsArray)
							{
								if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Null)
								{
									states[j] = null;
									j++;
									continue;
								}

								var innerState = HandleStates(parameters[j].ParameterType, recipeProperty);
								if (innerState.Item1 != null)
									states[j] = innerState.Item1;
								else if (innerState.Item2 != null)
									states[j] = innerState.Item2;
								else if (innerState.Item3 != null)
									states[j] = innerState.Item3;
								else
									states[j] = null;
								j++;
								continue;
							}

							bool success;
							object? value;
							(success, value) = recipeProperty.Value.ToValue(parameters[j].ParameterType);
							if (!success)
								Logger.LogError(
									$"[ModiBuff] Failed to load condition state from save data by {Name} for effect {effectId}");

							states[j] = value;
							j++;
						}

						if (typeof(IMetaEffect[]).IsAssignableFrom(parameterType))
						{
							objects[count] = (IMetaEffect)constructor.Invoke(states);
						}
						else if (typeof(IPostEffect[]).IsAssignableFrom(parameterType))
						{
							objects[count] = (IPostEffect)constructor.Invoke(states);
						}
						else if (typeof(ICondition[]).IsAssignableFrom(parameterType))
						{
							objects[count] = (ICondition)constructor.Invoke(states);
						}

						count++;
					}

					if (typeof(IMetaEffect[]).IsAssignableFrom(parameterType))
					{
						IMetaEffect<float, float>[] metaEffects = new IMetaEffect<float, float>[count];
						for (int k = 0; k < count; k++)
						{
							metaEffects[k] = (IMetaEffect<float, float>)objects[k];
						}

						return (metaEffects, null, null);
					}

					if (typeof(IPostEffect[]).IsAssignableFrom(parameterType))
					{
						IPostEffect<float>[] postEffects = new IPostEffect<float>[count];
						for (int k = 0; k < count; k++)
						{
							postEffects[k] = (IPostEffect<float>)objects[k];
						}

						return (null, postEffects, null);
					}

					if (typeof(ICondition[]).IsAssignableFrom(parameterType))
					{
						ICondition[] conditions = new ICondition[count];
						for (int k = 0; k < count; k++)
						{
							conditions[k] = (ICondition)objects[k];
						}

						return (null, null, conditions);
					}

					Logger.LogError(
						$"[ModiBuff] Unknown parameter type {parameterType} for recursive test by {Name} for effect {effectId}");
					return (null, null, null);
				}
			}
#endif
		}

#if MODIBUFF_SYSTEM_TEXT_JSON
		[System.Text.Json.Serialization.JsonPolymorphic]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Initialize), Initialize.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(InstanceStackable), InstanceStackable.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Aura), Aura.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Interval), Interval.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Duration), Duration.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Remove), Remove.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Refresh), Refresh.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Stack), Stack.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Dispel), Dispel.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Tag), Tag.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(CallbackUnit), CallbackUnit.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Effect), Effect.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(ModifierAction), ModifierAction.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Data), Data.Id)]
#endif
		public record SaveInstruction
		{
			public readonly int InstructionId;

			private const int BaseId = 0;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveInstruction(int instructionId)
			{
				InstructionId = instructionId;
			}

			public sealed record Initialize : SaveInstruction
			{
				public const int Id = BaseId;

				public readonly string Name;
				public readonly string DisplayName;
				public readonly string Description;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Initialize(string name, string displayName, string description) : base(Id)
				{
					Name = name;
					DisplayName = displayName;
					Description = description;
				}
			}

			public sealed record InstanceStackable() : SaveInstruction(Id)
			{
				public const int Id = Initialize.Id + 1;
			}

			public sealed record Aura() : SaveInstruction(Id)
			{
				public const int Id = InstanceStackable.Id + 1;
			}

			public sealed record Interval : SaveInstruction
			{
				public const int Id = Aura.Id + 1;

				public readonly float Value;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Interval(float value) : base(Id)
				{
					Value = value;
				}
			}

			public sealed record Duration : SaveInstruction
			{
				public const int Id = Interval.Id + 1;

				public readonly float Value;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Duration(float value) : base(Id)
				{
					Value = value;
				}
			}

			public sealed record Remove : SaveInstruction
			{
				public const int Id = SaveInstruction.Duration.Id + 1;

				public readonly Type RemoveType;
				public readonly EffectOn EffectOn;
				public readonly float Duration;

				public enum Type
				{
					RemoveOn,
					Duration,
				}

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Remove(Type removeType, EffectOn effectOn, float duration = 0) : base(Id)
				{
					RemoveType = removeType;
					EffectOn = effectOn;
					Duration = duration;
				}
			}

			public sealed record Refresh : SaveInstruction
			{
				public const int Id = Remove.Id + 1;

				public readonly RefreshType RefreshType;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Refresh(RefreshType refreshType) : base(Id)
				{
					RefreshType = refreshType;
				}
			}

			public sealed record Stack : SaveInstruction
			{
				public const int Id = Refresh.Id + 1;

				public readonly WhenStackEffect WhenStackEffect;
				public readonly int? MaxStacks;
				public readonly int? EveryXStacks;
				public readonly float? SingleStackTime;
				public readonly float? IndependentStackTime;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Stack(WhenStackEffect whenStackEffect, int? maxStacks, int? everyXStacks, float? singleStackTime,
					float? independentStackTime) : base(Id)
				{
					WhenStackEffect = whenStackEffect;
					MaxStacks = maxStacks;
					EveryXStacks = everyXStacks;
					SingleStackTime = singleStackTime;
					IndependentStackTime = independentStackTime;
				}
			}

			public sealed record Dispel : SaveInstruction
			{
				public const int Id = Stack.Id + 1;

				public readonly DispelType Type;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Dispel(DispelType type) : base(Id) => Type = type;
			}

			public sealed record Tag : SaveInstruction
			{
				public const int Id = Dispel.Id + 1;

				public enum Type
				{
					Add,
					Remove,
					Set,
				}

				public readonly Type InstructionTagType;
				public readonly TagType TagType;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Tag(Type instructionTagType, TagType tagType) : base(Id)
				{
					InstructionTagType = instructionTagType;
					TagType = tagType;
				}
			}

			public sealed record CallbackUnit : SaveInstruction
			{
				public const int Id = Tag.Id + 1;

				public readonly int CallbackUnitType;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public CallbackUnit(int callbackUnitType) : base(Id)
				{
					CallbackUnitType = callbackUnitType;
				}
			}

			public sealed record Effect : SaveInstruction
			{
				public const int Id = CallbackUnit.Id + 1;

				public readonly int EffectId;
				public readonly object SaveData;
				public readonly EffectOn EffectOn;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Effect(int effectId, object saveData, EffectOn effectOn) : base(Id)
				{
					EffectId = effectId;
					SaveData = saveData;
					EffectOn = effectOn;
				}
			}

			public sealed record ModifierAction : SaveInstruction
			{
				public const int Id = Effect.Id + 1;

				public readonly ModiBuff.Core.ModifierAction ModifierActionFlags;
				public readonly EffectOn EffectOn;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public ModifierAction(ModiBuff.Core.ModifierAction modifierActionFlags, EffectOn effectOn) : base(Id)
				{
					ModifierActionFlags = modifierActionFlags;
					EffectOn = effectOn;
				}
			}

			public sealed record Data : SaveInstruction
			{
				public const int Id = ModifierAction.Id + 1;

				public readonly object SaveData;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Data(object saveData) : base(Id) => SaveData = saveData;
			}
		}

		public readonly struct SaveData
		{
			public readonly SaveInstruction[] Instructions;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(SaveInstruction[] instructions) => Instructions = instructions;
		}
	}
}