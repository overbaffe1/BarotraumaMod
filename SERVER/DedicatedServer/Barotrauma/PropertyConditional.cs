using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000293 RID: 659
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class PropertyConditional
	{
		// Token: 0x06002E1C RID: 11804 RVA: 0x0013132D File Offset: 0x0012F52D
		public static IEnumerable<PropertyConditional> FromXElement(ContentXElement element, [Nullable(new byte[]
		{
			2,
			1
		})] Predicate<XAttribute> predicate = null)
		{
			PropertyConditional.<FromXElement>d__16 <FromXElement>d__ = new PropertyConditional.<FromXElement>d__16(-2);
			<FromXElement>d__.<>3__element = element;
			<FromXElement>d__.<>3__predicate = predicate;
			return <FromXElement>d__;
		}

		// Token: 0x06002E1D RID: 11805 RVA: 0x00131344 File Offset: 0x0012F544
		private static bool IsValid(XAttribute attribute)
		{
			string text = attribute.Name.ToString().ToLowerInvariant();
			if (text != null)
			{
				int length = text.Length;
				if (length != 10)
				{
					switch (length)
					{
					case 15:
						if (!(text == "targetcontainer"))
						{
							return true;
						}
						break;
					case 16:
						if (!(text == "skillrequirement"))
						{
							return true;
						}
						break;
					case 17:
						if (!(text == "targetgrandparent"))
						{
							return true;
						}
						break;
					case 18:
						return true;
					case 19:
					{
						char c = text[6];
						if (c != 'c')
						{
							if (c != 'i')
							{
								return true;
							}
							if (!(text == "targetitemcomponent"))
							{
								return true;
							}
						}
						else if (!(text == "targetcontaineditem"))
						{
							return true;
						}
						break;
					}
					default:
						return true;
					}
				}
				else
				{
					char c = text[7];
					if (c != 'e')
					{
						if (c != 'l')
						{
							return true;
						}
						if (!(text == "targetslot"))
						{
							return true;
						}
					}
					else if (!(text == "targetself"))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}

		// Token: 0x06002E1E RID: 11806 RVA: 0x00131428 File Offset: 0x0012F628
		private PropertyConditional(Identifier attributeName, PropertyConditional.ComparisonOperatorType comparisonOperator, string attributeValue, string targetItemComponent, PropertyConditional.LogicalOperatorType itemComponentComparison, bool targetSelf, bool targetContainer, bool targetGrandParent, bool targetContainedItem, PropertyConditional.ConditionType conditionType)
		{
			this.AttributeName = attributeName;
			this.TargetItemComponent = targetItemComponent;
			this.ItemComponentComparison = itemComponentComparison;
			this.TargetSelf = targetSelf;
			this.TargetContainer = targetContainer;
			this.TargetGrandParent = targetGrandParent;
			this.TargetContainedItem = targetContainedItem;
			this.Type = conditionType;
			this.ComparisonOperator = comparisonOperator;
			this.AttributeValue = attributeValue;
			this.AttributeValueAsTags = (from s in this.AttributeValue.Split(',', StringSplitOptions.None)
			select s.ToIdentifier()).ToImmutableArray<Identifier>();
			float value;
			if (float.TryParse(this.AttributeValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
			{
				this.FloatValue = new float?(value);
			}
			WorldHostilityOption hostilityValue;
			if (this.Type == PropertyConditional.ConditionType.WorldHostility && Enum.TryParse<WorldHostilityOption>(this.AttributeValue, true, out hostilityValue))
			{
				this.cachedHostilityValue = hostilityValue;
			}
		}

		// Token: 0x06002E1F RID: 11807 RVA: 0x0013150C File Offset: 0x0012F70C
		[return: TupleElementNames(new string[]
		{
			"ComparisonOperator",
			"ConditionStr"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static ValueTuple<PropertyConditional.ComparisonOperatorType, string> ExtractComparisonOperatorFromConditionString(string str)
		{
			if (str == null)
			{
				str = "";
			}
			PropertyConditional.ComparisonOperatorType op = PropertyConditional.ComparisonOperatorType.Equals;
			string conditionStr = str;
			int i = str.IndexOf(' ');
			if (i >= 0)
			{
				op = PropertyConditional.GetComparisonOperatorType(str.Substring(0, i));
				if (op != PropertyConditional.ComparisonOperatorType.None)
				{
					conditionStr = str.Substring(i + 1);
				}
				else
				{
					op = PropertyConditional.ComparisonOperatorType.Equals;
				}
			}
			return new ValueTuple<PropertyConditional.ComparisonOperatorType, string>(op, conditionStr);
		}

		// Token: 0x06002E20 RID: 11808 RVA: 0x0013155C File Offset: 0x0012F75C
		public static PropertyConditional.ComparisonOperatorType GetComparisonOperatorType(string op)
		{
			string text = op.ToLowerInvariant();
			if (text != null)
			{
				switch (text.Length)
				{
				case 1:
				{
					char c = text[0];
					if (c == '!')
					{
						return PropertyConditional.ComparisonOperatorType.NotEquals;
					}
					if (c != 'e')
					{
						return PropertyConditional.ComparisonOperatorType.None;
					}
					break;
				}
				case 2:
				{
					char c = text[0];
					if (c <= 'e')
					{
						if (c != '!')
						{
							if (c != 'e')
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
							if (!(text == "eq"))
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
						}
						else
						{
							if (!(text == "!e"))
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
							return PropertyConditional.ComparisonOperatorType.NotEquals;
						}
					}
					else if (c != 'g')
					{
						if (c != 'l')
						{
							if (c != 'n')
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
							if (!(text == "ne"))
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
							return PropertyConditional.ComparisonOperatorType.NotEquals;
						}
						else
						{
							if (!(text == "lt"))
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
							return PropertyConditional.ComparisonOperatorType.LessThan;
						}
					}
					else
					{
						if (!(text == "gt"))
						{
							return PropertyConditional.ComparisonOperatorType.None;
						}
						return PropertyConditional.ComparisonOperatorType.GreaterThan;
					}
					break;
				}
				case 3:
				{
					char c = text[0];
					if (c <= 'g')
					{
						if (c != '!')
						{
							if (c != 'g')
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
							if (!(text == "gte"))
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
							return PropertyConditional.ComparisonOperatorType.GreaterThanEquals;
						}
						else
						{
							if (!(text == "!eq"))
							{
								return PropertyConditional.ComparisonOperatorType.None;
							}
							return PropertyConditional.ComparisonOperatorType.NotEquals;
						}
					}
					else if (c != 'l')
					{
						if (c != 'n')
						{
							return PropertyConditional.ComparisonOperatorType.None;
						}
						if (!(text == "neq"))
						{
							return PropertyConditional.ComparisonOperatorType.None;
						}
						return PropertyConditional.ComparisonOperatorType.NotEquals;
					}
					else
					{
						if (!(text == "lte"))
						{
							return PropertyConditional.ComparisonOperatorType.None;
						}
						return PropertyConditional.ComparisonOperatorType.LessThanEquals;
					}
					break;
				}
				case 4:
				{
					char c = text[0];
					if (c != 'g')
					{
						if (c != 'l')
						{
							return PropertyConditional.ComparisonOperatorType.None;
						}
						if (!(text == "lteq"))
						{
							return PropertyConditional.ComparisonOperatorType.None;
						}
						return PropertyConditional.ComparisonOperatorType.LessThanEquals;
					}
					else
					{
						if (!(text == "gteq"))
						{
							return PropertyConditional.ComparisonOperatorType.None;
						}
						return PropertyConditional.ComparisonOperatorType.GreaterThanEquals;
					}
					break;
				}
				case 5:
				case 10:
				case 12:
				case 13:
				case 15:
				case 16:
					return PropertyConditional.ComparisonOperatorType.None;
				case 6:
					if (!(text == "equals"))
					{
						return PropertyConditional.ComparisonOperatorType.None;
					}
					break;
				case 7:
					if (!(text == "!equals"))
					{
						return PropertyConditional.ComparisonOperatorType.None;
					}
					return PropertyConditional.ComparisonOperatorType.NotEquals;
				case 8:
					if (!(text == "lessthan"))
					{
						return PropertyConditional.ComparisonOperatorType.None;
					}
					return PropertyConditional.ComparisonOperatorType.LessThan;
				case 9:
					if (!(text == "notequals"))
					{
						return PropertyConditional.ComparisonOperatorType.None;
					}
					return PropertyConditional.ComparisonOperatorType.NotEquals;
				case 11:
					if (!(text == "greaterthan"))
					{
						return PropertyConditional.ComparisonOperatorType.None;
					}
					return PropertyConditional.ComparisonOperatorType.GreaterThan;
				case 14:
					if (!(text == "lessthanequals"))
					{
						return PropertyConditional.ComparisonOperatorType.None;
					}
					return PropertyConditional.ComparisonOperatorType.LessThanEquals;
				case 17:
					if (!(text == "greaterthanequals"))
					{
						return PropertyConditional.ComparisonOperatorType.None;
					}
					return PropertyConditional.ComparisonOperatorType.GreaterThanEquals;
				default:
					return PropertyConditional.ComparisonOperatorType.None;
				}
				return PropertyConditional.ComparisonOperatorType.Equals;
			}
			return PropertyConditional.ComparisonOperatorType.None;
		}

		// Token: 0x17000D88 RID: 3464
		// (get) Token: 0x06002E21 RID: 11809 RVA: 0x001317CF File Offset: 0x0012F9CF
		private bool ComparisonOperatorIsNotEquals
		{
			get
			{
				return this.ComparisonOperator == PropertyConditional.ComparisonOperatorType.NotEquals;
			}
		}

		// Token: 0x06002E22 RID: 11810 RVA: 0x001317DA File Offset: 0x0012F9DA
		[NullableContext(2)]
		public bool Matches(ISerializableEntity target)
		{
			if (!this.TargetContainedItem)
			{
				return this.MatchesDirect(target);
			}
			return this.MatchesContained(target);
		}

		// Token: 0x06002E23 RID: 11811 RVA: 0x001317F4 File Offset: 0x0012F9F4
		[NullableContext(2)]
		private bool MatchesContained(ISerializableEntity target)
		{
			Item item = target as Item;
			IEnumerable<Item> enumerable;
			if (item == null)
			{
				ItemComponent ic = target as ItemComponent;
				if (ic == null)
				{
					Character character = target as Character;
					if (character != null)
					{
						CharacterInventory characterInventory = character.Inventory;
						if (characterInventory != null)
						{
							enumerable = characterInventory.AllItems;
							goto IL_57;
						}
					}
					enumerable = Enumerable.Empty<Item>();
				}
				else
				{
					enumerable = ic.Item.ContainedItems;
				}
			}
			else
			{
				enumerable = item.ContainedItems;
			}
			IL_57:
			IEnumerable<Item> containedItems = enumerable;
			foreach (Item containedItem in containedItems)
			{
				if (this.MatchesDirect(containedItem))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002E24 RID: 11812 RVA: 0x001318A8 File Offset: 0x0012FAA8
		[NullableContext(2)]
		private bool MatchesDirect(ISerializableEntity target)
		{
			Character targetChar = target as Character;
			Limb limb = target as Limb;
			if (limb != null)
			{
				targetChar = limb.character;
			}
			PropertyConditional.ConditionType type = this.Type;
			switch (type)
			{
			case PropertyConditional.ConditionType.PropertyValueOrAffliction:
				if (AfflictionPrefab.Prefabs.ContainsKey(this.AttributeName))
				{
					if (targetChar != null)
					{
						CharacterHealth health = targetChar.CharacterHealth;
						if (health != null)
						{
							Affliction affliction = health.GetAffliction(this.AttributeName, true);
							float afflictionStrength = (affliction != null) ? affliction.Strength : 0f;
							return this.NumberMatchesRequirement(afflictionStrength);
						}
					}
				}
				else
				{
					SerializableProperty property;
					if (((target != null) ? target.SerializableProperties : null) != null && target.SerializableProperties.TryGetValue(this.AttributeName, out property))
					{
						return this.PropertyMatchesRequirement(target, property);
					}
					SerializableProperty characterProperty;
					if (((targetChar != null) ? targetChar.SerializableProperties : null) != null && targetChar.SerializableProperties.TryGetValue(this.AttributeName, out characterProperty))
					{
						return this.PropertyMatchesRequirement(targetChar, characterProperty);
					}
					Dictionary<Identifier, SerializableProperty> dictionary;
					if (targetChar == null)
					{
						dictionary = null;
					}
					else
					{
						AnimController animController = targetChar.AnimController;
						dictionary = ((animController != null) ? animController.SerializableProperties : null);
					}
					Dictionary<Identifier, SerializableProperty> animControllerProperties = dictionary;
					SerializableProperty animControllerProperty;
					if (animControllerProperties != null && animControllerProperties.TryGetValue(this.AttributeName, out animControllerProperty))
					{
						return this.PropertyMatchesRequirement(targetChar.AnimController, animControllerProperty);
					}
				}
				return this.ComparisonOperatorIsNotEquals;
			case PropertyConditional.ConditionType.SkillRequirement:
				if (targetChar != null)
				{
					float skillLevel = targetChar.GetSkillLevel(this.AttributeName.ToIdentifier<Identifier>());
					return this.NumberMatchesRequirement(skillLevel);
				}
				return this.ComparisonOperatorIsNotEquals;
			case PropertyConditional.ConditionType.Name:
			case PropertyConditional.ConditionType.SpeciesName:
			case PropertyConditional.ConditionType.SpeciesGroup:
				break;
			case PropertyConditional.ConditionType.HasTag:
			{
				if (targetChar != null)
				{
					return this.CheckMatchingTags(new Func<Identifier, bool>(targetChar.Params.HasTag));
				}
				Item item = target as Item;
				if (item != null)
				{
					return this.CheckMatchingTags(new Func<Identifier, bool>(item.HasTag));
				}
				return this.ComparisonOperatorIsNotEquals;
			}
			case PropertyConditional.ConditionType.HasStatusTag:
			{
				if (target == null)
				{
					return this.ComparisonOperatorIsNotEquals;
				}
				int numTagsFound = 0;
				foreach (Identifier tag in this.AttributeValueAsTags)
				{
					bool tagFound = false;
					foreach (DurationListElement durationEffect in StatusEffect.DurationList)
					{
						if (durationEffect.Targets.Contains(target) && durationEffect.Parent.HasTag(tag))
						{
							tagFound = true;
							break;
						}
					}
					if (!tagFound)
					{
						foreach (DelayedListElement delayedEffect in DelayedEffect.DelayList)
						{
							if (delayedEffect.Targets.Contains(target) && delayedEffect.Parent.HasTag(tag))
							{
								tagFound = true;
								break;
							}
						}
					}
					if (tagFound)
					{
						numTagsFound++;
					}
				}
				if (!this.ComparisonOperatorIsNotEquals)
				{
					return numTagsFound >= this.AttributeValueAsTags.Length;
				}
				return numTagsFound < this.AttributeValueAsTags.Length;
			}
			default:
				if (type == PropertyConditional.ConditionType.WorldHostility)
				{
					GameSession gameSession = GameMain.GameSession;
					CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
					return campaign != null && PropertyConditional.Compare<WorldHostilityOption>(campaign.Settings.WorldHostility, this.cachedHostilityValue, this.ComparisonOperator);
				}
				if (type == PropertyConditional.ConditionType.LevelDifficulty)
				{
					Level level = Level.Loaded;
					return level != null && this.NumberMatchesRequirement(level.Difficulty);
				}
				break;
			}
			bool equals = this.CheckOnlyEquality(target);
			if (!this.ComparisonOperatorIsNotEquals)
			{
				return equals;
			}
			return !equals;
		}

		// Token: 0x06002E25 RID: 11813 RVA: 0x00131C04 File Offset: 0x0012FE04
		[NullableContext(2)]
		private bool CheckOnlyEquality(ISerializableEntity target)
		{
			switch (this.Type)
			{
			case PropertyConditional.ConditionType.Name:
				return target != null && target.Name == this.AttributeValue;
			case PropertyConditional.ConditionType.SpeciesName:
			{
				Character targetCharacter = target as Character;
				if (targetCharacter != null)
				{
					Identifier speciesName = targetCharacter.SpeciesName;
					return speciesName == this.AttributeValue;
				}
				Limb targetLimb = target as Limb;
				if (targetLimb != null)
				{
					Identifier speciesName = targetLimb.character.SpeciesName;
					return speciesName == this.AttributeValue;
				}
				return false;
			}
			case PropertyConditional.ConditionType.SpeciesGroup:
			{
				Character targetCharacter2 = target as Character;
				if (targetCharacter2 != null)
				{
					return CharacterParams.CompareGroup(this.AttributeValue.ToIdentifier(), targetCharacter2.Params.Group);
				}
				Limb targetLimb2 = target as Limb;
				return targetLimb2 != null && CharacterParams.CompareGroup(this.AttributeValue.ToIdentifier(), targetLimb2.character.Params.Group);
			}
			case PropertyConditional.ConditionType.HasSpecifierTag:
			{
				Character character = target as Character;
				if (character != null)
				{
					CharacterInfo characterInfo = character.Info;
					if (characterInfo != null)
					{
						return this.AttributeValueAsTags.All(new Func<Identifier, bool>(characterInfo.Head.Preset.TagSet.Contains));
					}
				}
				return false;
			}
			case PropertyConditional.ConditionType.EntityType:
			{
				string a = this.AttributeValue.ToLowerInvariant();
				bool result;
				if (!(a == "character"))
				{
					if (!(a == "limb"))
					{
						if (!(a == "item"))
						{
							if (!(a == "structure"))
							{
								result = (a == "null" && target == null);
							}
							else
							{
								result = (target is Structure);
							}
						}
						else
						{
							result = (target is Item);
						}
					}
					else
					{
						result = (target is Limb);
					}
				}
				else
				{
					result = (target is Character);
				}
				return result;
			}
			case PropertyConditional.ConditionType.LimbType:
			{
				Limb limb = target as Limb;
				LimbType attributeLimbType;
				return limb != null && Enum.TryParse<LimbType>(this.AttributeValue, true, out attributeLimbType) && attributeLimbType == limb.type;
			}
			}
			return false;
		}

		// Token: 0x06002E26 RID: 11814 RVA: 0x00131DFF File Offset: 0x0012FFFF
		private bool SufficientTagMatches(int matches)
		{
			if (!this.ComparisonOperatorIsNotEquals)
			{
				return matches >= this.AttributeValueAsTags.Length;
			}
			return matches <= 0;
		}

		// Token: 0x06002E27 RID: 11815 RVA: 0x00131E24 File Offset: 0x00130024
		private bool CheckMatchingTags(Func<Identifier, bool> predicate)
		{
			int matches = 0;
			foreach (Identifier tag in this.AttributeValueAsTags)
			{
				if (predicate(tag))
				{
					matches++;
				}
			}
			return this.SufficientTagMatches(matches);
		}

		// Token: 0x06002E28 RID: 11816 RVA: 0x00131E66 File Offset: 0x00130066
		public bool TargetTagMatchesTagCondition(Identifier targetTag)
		{
			return !targetTag.IsEmpty && this.Type == PropertyConditional.ConditionType.HasTag && this.CheckMatchingTags(new Func<Identifier, bool>(targetTag.Equals));
		}

		// Token: 0x06002E29 RID: 11817 RVA: 0x00131E94 File Offset: 0x00130094
		private bool NumberMatchesRequirement(float testedValue)
		{
			if (this.FloatValue == null)
			{
				return this.ComparisonOperatorIsNotEquals;
			}
			float value = this.FloatValue.Value;
			return PropertyConditional.CompareFloat(testedValue, value, this.ComparisonOperator);
		}

		// Token: 0x06002E2A RID: 11818 RVA: 0x00131ED0 File Offset: 0x001300D0
		private bool PropertyMatchesRequirement(ISerializableEntity target, SerializableProperty property)
		{
			Type type = property.PropertyType;
			if (type == typeof(float) || type == typeof(int))
			{
				float floatValue = property.GetFloatValue(target);
				return this.NumberMatchesRequirement(floatValue);
			}
			PropertyConditional.ComparisonOperatorType comparisonOperator = this.ComparisonOperator;
			if (comparisonOperator - PropertyConditional.ComparisonOperatorType.Equals > 1)
			{
				string[] array = new string[9];
				array[0] = "Couldn't compare ";
				array[1] = this.AttributeValue.ToString();
				array[2] = " (";
				int num = 3;
				Type type2 = this.AttributeValue.GetType();
				array[num] = ((type2 != null) ? type2.ToString() : null);
				array[4] = ") to property \"";
				array[5] = property.Name;
				array[6] = "\" (";
				int num2 = 7;
				Type type3 = type;
				array[num2] = ((type3 != null) ? type3.ToString() : null);
				array[8] = ")! Make sure the type of the value set in the config files matches the type of the property.";
				DebugConsole.ThrowError(string.Concat(array), null, null, false, false);
				return false;
			}
			bool equals;
			if (type == typeof(bool))
			{
				bool attributeValueBool = this.AttributeValue.IsTrueString();
				equals = (property.GetBoolValue(target) == attributeValueBool);
			}
			else
			{
				object value = property.GetValue(target);
				equals = PropertyConditional.<PropertyMatchesRequirement>g__AreValuesEquivalent|31_0(value, this.AttributeValue);
			}
			if (!this.ComparisonOperatorIsNotEquals)
			{
				return equals;
			}
			return !equals;
		}

		// Token: 0x06002E2B RID: 11819 RVA: 0x00131FF8 File Offset: 0x001301F8
		public static bool CompareFloat(float val1, float val2, PropertyConditional.ComparisonOperatorType op)
		{
			switch (op)
			{
			case PropertyConditional.ComparisonOperatorType.Equals:
				return MathUtils.NearlyEqual(val1, val2, 0.0001f);
			case PropertyConditional.ComparisonOperatorType.NotEquals:
				return !MathUtils.NearlyEqual(val1, val2, 0.0001f);
			case PropertyConditional.ComparisonOperatorType.LessThan:
				return val1 < val2;
			case PropertyConditional.ComparisonOperatorType.LessThanEquals:
				return val1 <= val2;
			case PropertyConditional.ComparisonOperatorType.GreaterThan:
				return val1 > val2;
			case PropertyConditional.ComparisonOperatorType.GreaterThanEquals:
				return val1 >= val2;
			default:
				return false;
			}
		}

		// Token: 0x06002E2C RID: 11820 RVA: 0x00132060 File Offset: 0x00130260
		public static bool Compare<[Nullable(0)] T>(T leftValue, T rightValue, PropertyConditional.ComparisonOperatorType comparisonOperator) where T : IComparable
		{
			bool result;
			switch (comparisonOperator)
			{
			case PropertyConditional.ComparisonOperatorType.NotEquals:
				result = (leftValue.CompareTo(rightValue) != 0);
				break;
			case PropertyConditional.ComparisonOperatorType.LessThan:
				result = (leftValue.CompareTo(rightValue) < 0);
				break;
			case PropertyConditional.ComparisonOperatorType.LessThanEquals:
				result = (leftValue.CompareTo(rightValue) <= 0);
				break;
			case PropertyConditional.ComparisonOperatorType.GreaterThan:
				result = (leftValue.CompareTo(rightValue) > 0);
				break;
			case PropertyConditional.ComparisonOperatorType.GreaterThanEquals:
				result = (leftValue.CompareTo(rightValue) >= 0);
				break;
			default:
				result = (leftValue.CompareTo(rightValue) == 0);
				break;
			}
			return result;
		}

		// Token: 0x06002E2D RID: 11821 RVA: 0x0013212C File Offset: 0x0013032C
		[return: Nullable(2)]
		public static PropertyConditional.LogicalComparison LoadConditionals(ContentXElement element, PropertyConditional.LogicalOperatorType defaultOperatorType = PropertyConditional.LogicalOperatorType.And)
		{
			IEnumerable<ContentXElement> conditionalElements = element.GetChildElements("conditional");
			if (conditionalElements.None(null))
			{
				return null;
			}
			List<PropertyConditional> conditionals = new List<PropertyConditional>();
			foreach (ContentXElement subElement in conditionalElements)
			{
				conditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
			}
			PropertyConditional.LogicalOperatorType logicalOperator = element.GetAttributeEnum<PropertyConditional.LogicalOperatorType>("comparison", defaultOperatorType);
			return new PropertyConditional.LogicalComparison(conditionals, logicalOperator);
		}

		// Token: 0x06002E2E RID: 11822 RVA: 0x001321B0 File Offset: 0x001303B0
		public static bool CheckConditionals(ISerializableEntity conditionalTarget, IEnumerable<PropertyConditional> conditionals, PropertyConditional.LogicalOperatorType logicalOperator)
		{
			if (conditionals == null)
			{
				return true;
			}
			if (conditionals.None(null))
			{
				return true;
			}
			if (logicalOperator == PropertyConditional.LogicalOperatorType.And)
			{
				foreach (PropertyConditional conditional in conditionals)
				{
					if (!conditional.Matches(conditionalTarget))
					{
						return false;
					}
				}
				return true;
			}
			if (logicalOperator != PropertyConditional.LogicalOperatorType.Or)
			{
				throw new NotSupportedException();
			}
			foreach (PropertyConditional conditional2 in conditionals)
			{
				if (conditional2.Matches(conditionalTarget))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002E2F RID: 11823 RVA: 0x00132264 File Offset: 0x00130464
		[CompilerGenerated]
		internal static bool <PropertyMatchesRequirement>g__AreValuesEquivalent|31_0([Nullable(2)] object value, string desiredValue)
		{
			if (value == null)
			{
				return desiredValue.Equals("null", StringComparison.OrdinalIgnoreCase);
			}
			return (value.ToString() ?? "").Equals(desiredValue);
		}

		// Token: 0x04001692 RID: 5778
		public readonly PropertyConditional.ConditionType Type;

		// Token: 0x04001693 RID: 5779
		public readonly PropertyConditional.ComparisonOperatorType ComparisonOperator;

		// Token: 0x04001694 RID: 5780
		public readonly Identifier AttributeName;

		// Token: 0x04001695 RID: 5781
		public readonly string AttributeValue;

		// Token: 0x04001696 RID: 5782
		[Nullable(0)]
		public readonly ImmutableArray<Identifier> AttributeValueAsTags;

		// Token: 0x04001697 RID: 5783
		public readonly float? FloatValue;

		// Token: 0x04001698 RID: 5784
		private readonly WorldHostilityOption cachedHostilityValue;

		// Token: 0x04001699 RID: 5785
		public readonly string TargetItemComponent;

		// Token: 0x0400169A RID: 5786
		public readonly PropertyConditional.LogicalOperatorType ItemComponentComparison;

		// Token: 0x0400169B RID: 5787
		public readonly bool TargetSelf;

		// Token: 0x0400169C RID: 5788
		public readonly bool TargetContainer;

		// Token: 0x0400169D RID: 5789
		public readonly bool TargetGrandParent;

		// Token: 0x0400169E RID: 5790
		public readonly bool TargetContainedItem;

		// Token: 0x02000B02 RID: 2818
		[NullableContext(0)]
		public enum ConditionType
		{
			// Token: 0x0400380E RID: 14350
			PropertyValueOrAffliction,
			// Token: 0x0400380F RID: 14351
			SkillRequirement,
			// Token: 0x04003810 RID: 14352
			Name,
			// Token: 0x04003811 RID: 14353
			SpeciesName,
			// Token: 0x04003812 RID: 14354
			SpeciesGroup,
			// Token: 0x04003813 RID: 14355
			HasTag,
			// Token: 0x04003814 RID: 14356
			HasStatusTag,
			// Token: 0x04003815 RID: 14357
			HasSpecifierTag,
			// Token: 0x04003816 RID: 14358
			EntityType,
			// Token: 0x04003817 RID: 14359
			LimbType,
			// Token: 0x04003818 RID: 14360
			WorldHostility,
			// Token: 0x04003819 RID: 14361
			LevelDifficulty
		}

		// Token: 0x02000B03 RID: 2819
		[NullableContext(0)]
		public enum LogicalOperatorType
		{
			// Token: 0x0400381B RID: 14363
			And,
			// Token: 0x0400381C RID: 14364
			Or
		}

		// Token: 0x02000B04 RID: 2820
		[NullableContext(0)]
		public enum ComparisonOperatorType
		{
			// Token: 0x0400381E RID: 14366
			None,
			// Token: 0x0400381F RID: 14367
			Equals,
			// Token: 0x04003820 RID: 14368
			NotEquals,
			// Token: 0x04003821 RID: 14369
			LessThan,
			// Token: 0x04003822 RID: 14370
			LessThanEquals,
			// Token: 0x04003823 RID: 14371
			GreaterThan,
			// Token: 0x04003824 RID: 14372
			GreaterThanEquals
		}

		// Token: 0x02000B05 RID: 2821
		[NullableContext(0)]
		public class LogicalComparison
		{
			// Token: 0x06005F44 RID: 24388 RVA: 0x00206DF0 File Offset: 0x00204FF0
			[NullableContext(1)]
			public LogicalComparison(IEnumerable<PropertyConditional> conditionals, PropertyConditional.LogicalOperatorType logicalOperator)
			{
				this.Conditionals = conditionals.ToImmutableArray<PropertyConditional>();
				this.LogicalOperator = logicalOperator;
			}

			// Token: 0x04003825 RID: 14373
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public readonly ImmutableArray<PropertyConditional> Conditionals;

			// Token: 0x04003826 RID: 14374
			public readonly PropertyConditional.LogicalOperatorType LogicalOperator;
		}
	}
}
