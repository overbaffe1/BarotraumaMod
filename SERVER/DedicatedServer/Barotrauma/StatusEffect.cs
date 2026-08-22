using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000295 RID: 661
	internal class StatusEffect
	{
		// Token: 0x06002E36 RID: 11830 RVA: 0x0013231C File Offset: 0x0013051C
		static StatusEffect()
		{
			StatusEffect.FieldNames = (from f in typeof(StatusEffect).GetFields().AsEnumerable<FieldInfo>()
			select f.Name.ToIdentifier()).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x17000D8B RID: 3467
		// (get) Token: 0x06002E37 RID: 11831 RVA: 0x00132370 File Offset: 0x00130570
		public bool HasConditions
		{
			get
			{
				return this.propertyConditionals != null && this.propertyConditionals.Any<PropertyConditional>();
			}
		}

		// Token: 0x17000D8C RID: 3468
		// (get) Token: 0x06002E38 RID: 11832 RVA: 0x00132388 File Offset: 0x00130588
		public IEnumerable<Explosion> Explosions
		{
			get
			{
				IEnumerable<Explosion> enumerable = this.explosions;
				return enumerable ?? Enumerable.Empty<Explosion>();
			}
		}

		// Token: 0x17000D8D RID: 3469
		// (get) Token: 0x06002E39 RID: 11833 RVA: 0x001323A6 File Offset: 0x001305A6
		// (set) Token: 0x06002E3A RID: 11834 RVA: 0x001323AE File Offset: 0x001305AE
		public List<Affliction> Afflictions { get; private set; } = new List<Affliction>();

		// Token: 0x17000D8E RID: 3470
		// (get) Token: 0x06002E3B RID: 11835 RVA: 0x001323B8 File Offset: 0x001305B8
		public IEnumerable<StatusEffect.CharacterSpawnInfo> SpawnCharacters
		{
			get
			{
				IEnumerable<StatusEffect.CharacterSpawnInfo> enumerable = this.spawnCharacters;
				return enumerable ?? Enumerable.Empty<StatusEffect.CharacterSpawnInfo>();
			}
		}

		// Token: 0x17000D8F RID: 3471
		// (get) Token: 0x06002E3C RID: 11836 RVA: 0x001323D6 File Offset: 0x001305D6
		// (set) Token: 0x06002E3D RID: 11837 RVA: 0x001323DE File Offset: 0x001305DE
		public float Range { get; private set; }

		// Token: 0x17000D90 RID: 3472
		// (get) Token: 0x06002E3E RID: 11838 RVA: 0x001323E7 File Offset: 0x001305E7
		// (set) Token: 0x06002E3F RID: 11839 RVA: 0x001323EF File Offset: 0x001305EF
		public Vector2 Offset { get; private set; }

		// Token: 0x17000D91 RID: 3473
		// (get) Token: 0x06002E40 RID: 11840 RVA: 0x001323F8 File Offset: 0x001305F8
		// (set) Token: 0x06002E41 RID: 11841 RVA: 0x00132400 File Offset: 0x00130600
		public bool OffsetCopiesEntityTransform { get; private set; }

		// Token: 0x17000D92 RID: 3474
		// (get) Token: 0x06002E42 RID: 11842 RVA: 0x00132409 File Offset: 0x00130609
		// (set) Token: 0x06002E43 RID: 11843 RVA: 0x00132411 File Offset: 0x00130611
		public float RandomOffset { get; private set; }

		// Token: 0x17000D93 RID: 3475
		// (get) Token: 0x06002E44 RID: 11844 RVA: 0x0013241A File Offset: 0x0013061A
		// (set) Token: 0x06002E45 RID: 11845 RVA: 0x0013242C File Offset: 0x0013062C
		public string Tags
		{
			get
			{
				return string.Join<Identifier>(",", this.statusEffectTags);
			}
			set
			{
				this.statusEffectTags.Clear();
				if (value == null)
				{
					return;
				}
				string[] newTags = value.Split(',', StringSplitOptions.None);
				foreach (string tag in newTags)
				{
					Identifier newTag = tag.Trim().ToIdentifier();
					if (!this.statusEffectTags.Contains(newTag))
					{
						this.statusEffectTags.Add(newTag);
					}
				}
			}
		}

		// Token: 0x17000D94 RID: 3476
		// (get) Token: 0x06002E46 RID: 11846 RVA: 0x00132490 File Offset: 0x00130690
		// (set) Token: 0x06002E47 RID: 11847 RVA: 0x00132498 File Offset: 0x00130698
		public bool Disabled { get; private set; }

		// Token: 0x06002E48 RID: 11848 RVA: 0x001324A1 File Offset: 0x001306A1
		public static StatusEffect Load(ContentXElement element, string parentDebugName)
		{
			if (element.GetAttribute("delay") != null || element.GetAttribute("delaytype") != null)
			{
				return new DelayedEffect(element, parentDebugName);
			}
			return new StatusEffect(element, parentDebugName);
		}

		// Token: 0x06002E49 RID: 11849 RVA: 0x001324CC File Offset: 0x001306CC
		protected StatusEffect(ContentXElement element, string parentDebugName)
		{
			this.statusEffectTags = new HashSet<Identifier>(element.GetAttributeIdentifierArray("statuseffecttags", element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true), true));
			this.OnlyInside = element.GetAttributeBool("onlyinside", false);
			this.OnlyOutside = element.GetAttributeBool("onlyoutside", false);
			this.OnlyWhenDamagedByPlayer = element.GetAttributeBool("onlyplayertriggered", element.GetAttributeBool("onlywhendamagedbyplayer", false));
			this.AllowWhenBroken = element.GetAttributeBool("allowwhenbroken", false);
			this.Interval = element.GetAttributeFloat("interval", 0f);
			this.Duration = element.GetAttributeFloat("duration", 0f);
			this.disableDeltaTime = element.GetAttributeBool("disabledeltatime", false);
			this.setValue = element.GetAttributeBool("setvalue", false);
			this.Stackable = element.GetAttributeBool("stackable", true);
			this.ResetDurationWhenReapplied = element.GetAttributeBool("resetdurationwhenreapplied", true);
			this.lifeTime = (this.lifeTimer = element.GetAttributeFloat("lifetime", 0f));
			this.CheckConditionalAlways = element.GetAttributeBool("checkconditionalalways", false);
			this.TargetItemComponent = element.GetAttributeString("targetitemcomponent", string.Empty);
			this.TargetSlot = element.GetAttributeInt("targetslot", -1);
			this.Range = element.GetAttributeFloat("range", 0f);
			string key = "offset";
			Vector2 zero = Vector2.Zero;
			this.Offset = element.GetAttributeVector2(key, zero);
			this.OffsetCopiesEntityTransform = element.GetAttributeBool("OffsetCopiesEntityTransform", false);
			this.RandomOffset = element.GetAttributeFloat("randomoffset", 0f);
			string[] targetLimbNames = element.GetAttributeStringArray("targetlimb", null, false) ?? element.GetAttributeStringArray("targetlimbs", null, false);
			if (targetLimbNames != null)
			{
				List<LimbType> targetLimbs = new List<LimbType>();
				foreach (string targetLimbName in targetLimbNames)
				{
					LimbType targetLimb;
					if (Enum.TryParse<LimbType>(targetLimbName, true, out targetLimb))
					{
						targetLimbs.Add(targetLimb);
					}
				}
				if (targetLimbs.Count > 0)
				{
					this.targetLimbs = targetLimbs.ToArray();
				}
			}
			this.CanGiveMedicalSkill = element.GetAttributeBool("CanGiveMedicalSkill", true);
			this.SeverLimbsProbability = MathHelper.Clamp(element.GetAttributeFloat(0f, new string[]
			{
				"severlimbs",
				"severlimbsprobability"
			}), 0f, 1f);
			string key2 = "randomcondition";
			zero = Vector2.Zero;
			this.randomCondition = element.GetAttributeVector2(key2, zero);
			string[] targetTypesStr = element.GetAttributeStringArray("target", null, false) ?? element.GetAttributeStringArray("targettype", Array.Empty<string>(), false);
			foreach (string s in targetTypesStr)
			{
				StatusEffect.TargetType targetType;
				if (!Enum.TryParse<StatusEffect.TargetType>(s, true, out targetType))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Invalid target type \"");
					defaultInterpolatedStringHandler.AppendFormatted(s);
					defaultInterpolatedStringHandler.AppendLiteral("\" in StatusEffect (");
					defaultInterpolatedStringHandler.AppendFormatted(parentDebugName);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				else
				{
					this.targetTypes |= targetType;
				}
			}
			if (this.targetTypes == (StatusEffect.TargetType)0)
			{
				string errorMessage = "Potential error in StatusEffect (" + parentDebugName + "). Target not defined, the effect might not work correctly. Use target=\"This\" if you want the effect to target the entity it's defined in. Setting \"This\" as the target.";
				DebugConsole.AddSafeError(errorMessage);
			}
			Identifier[] targetIdentifiers = element.GetAttributeIdentifierArray(Array.Empty<Identifier>(), new string[]
			{
				"targetnames",
				"targets",
				"targetidentifiers",
				"targettags"
			});
			if (targetIdentifiers.Any<Identifier>())
			{
				this.TargetIdentifiers = targetIdentifiers.ToImmutableHashSet<Identifier>();
			}
			this.triggeredEventTargetTag = element.GetAttributeIdentifier("eventtargettag", this.triggeredEventTargetTag);
			this.triggeredEventEntityTag = element.GetAttributeIdentifier("evententitytag", this.triggeredEventEntityTag);
			this.triggeredEventUserTag = element.GetAttributeIdentifier("eventusertag", this.triggeredEventUserTag);
			this.spawnItemRandomly = element.GetAttributeBool("spawnitemrandomly", false);
			this.multiplyAfflictionsByMaxVitality = element.GetAttributeBool("multiplyAfflictionsByMaxVitality", false);
			this.UnlockRecipes = element.GetAttributeIdentifierImmutableHashSet("UnlockRecipes", element.GetAttributeIdentifierImmutableHashSet("UnlockRecipe", ImmutableHashSet<Identifier>.Empty, true), true);
			List<XAttribute> propertyAttributes = new List<XAttribute>();
			this.propertyConditionals = new List<PropertyConditional>();
			foreach (XAttribute attribute in element.Attributes())
			{
				string text = attribute.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 4:
					{
						char c = text[1];
						if (c != 'a')
						{
							if (c != 'y')
							{
								goto IL_9BC;
							}
							if (!(text == "type"))
							{
								goto IL_9BC;
							}
							if (!Enum.TryParse<ActionType>(attribute.Value, true, out this.type))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(41, 2);
								defaultInterpolatedStringHandler2.AppendLiteral("Invalid action type \"");
								defaultInterpolatedStringHandler2.AppendFormatted(attribute.Value);
								defaultInterpolatedStringHandler2.AppendLiteral("\" in StatusEffect (");
								defaultInterpolatedStringHandler2.AppendFormatted(parentDebugName);
								defaultInterpolatedStringHandler2.AppendLiteral(")");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
								continue;
							}
							continue;
						}
						else
						{
							if (!(text == "tags"))
							{
								goto IL_9BC;
							}
							if (this.Duration > 0f && !this.setValue)
							{
								continue;
							}
							propertyAttributes.Add(attribute);
							if (this.targetTypes.HasFlag(StatusEffect.TargetType.UseTarget))
							{
								DebugConsole.AddWarning("Potential error in StatusEffect (" + parentDebugName + "). The effect is configured to set the tags of the use target, which will not work on most kinds of targets (only if the target is an item). If you meant to configure the tags for the StatusEffect itself, please use the attribute 'statuseffecttags'. If you are sure you want to set the tags of the target, use the attribute 'settags'.", element.ContentPackage);
								continue;
							}
							continue;
						}
						break;
					}
					case 5:
					{
						char c = text[0];
						if (c != 'd')
						{
							if (c != 'r')
							{
								if (c != 's')
								{
									goto IL_9BC;
								}
								if (!(text == "sound"))
								{
									goto IL_9BC;
								}
								DebugConsole.ThrowError("Error in StatusEffect (" + parentDebugName + "): sounds should be defined as child elements of the StatusEffect, not as attributes.", null, element.ContentPackage, false, false);
								continue;
							}
							else
							{
								if (!(text == "range"))
								{
									goto IL_9BC;
								}
								if (!this.HasTargetType(StatusEffect.TargetType.NearbyCharacters) && !this.HasTargetType(StatusEffect.TargetType.NearbyItems))
								{
									propertyAttributes.Add(attribute);
									continue;
								}
								continue;
							}
						}
						else
						{
							if (!(text == "delay"))
							{
								goto IL_9BC;
							}
							continue;
						}
						break;
					}
					case 6:
						if (!(text == "target"))
						{
							goto IL_9BC;
						}
						continue;
					case 7:
					{
						char c = text[0];
						if (c != 'o')
						{
							if (c != 's')
							{
								if (c != 't')
								{
									goto IL_9BC;
								}
								if (!(text == "targets"))
								{
									goto IL_9BC;
								}
								continue;
							}
							else
							{
								if (!(text == "settags"))
								{
									goto IL_9BC;
								}
								propertyAttributes.Add(attribute);
								continue;
							}
						}
						else
						{
							if (!(text == "oneshot"))
							{
								goto IL_9BC;
							}
							this.oneShot = attribute.GetAttributeBool(false);
							continue;
						}
						break;
					}
					case 8:
						if (!(text == "interval"))
						{
							goto IL_9BC;
						}
						continue;
					case 9:
					case 12:
					case 13:
					case 14:
					case 15:
					case 16:
					case 20:
						goto IL_9BC;
					case 10:
					{
						char c = text[7];
						if (c <= 'i')
						{
							if (c != 'a')
							{
								if (c != 'i')
								{
									goto IL_9BC;
								}
								if (!(text == "targetlimb"))
								{
									goto IL_9BC;
								}
								continue;
							}
							else
							{
								if (!(text == "targettags"))
								{
									goto IL_9BC;
								}
								continue;
							}
						}
						else if (c != 'm')
						{
							if (c != 's')
							{
								if (c != 'y')
								{
									goto IL_9BC;
								}
								if (!(text == "targettype"))
								{
									goto IL_9BC;
								}
								continue;
							}
							else
							{
								if (!(text == "comparison"))
								{
									goto IL_9BC;
								}
								goto IL_88F;
							}
						}
						else
						{
							if (!(text == "severlimbs"))
							{
								goto IL_9BC;
							}
							continue;
						}
						break;
					}
					case 11:
						if (!(text == "targetnames"))
						{
							goto IL_9BC;
						}
						continue;
					case 17:
						if (!(text == "targetidentifiers"))
						{
							goto IL_9BC;
						}
						continue;
					case 18:
						if (!(text == "allowedafflictions"))
						{
							goto IL_9BC;
						}
						break;
					case 19:
						if (!(text == "requiredafflictions"))
						{
							goto IL_9BC;
						}
						break;
					case 21:
						if (!(text == "conditionalcomparison"))
						{
							goto IL_9BC;
						}
						goto IL_88F;
					default:
						goto IL_9BC;
					}
					string[] types = attribute.Value.Split(',', StringSplitOptions.None);
					if (this.requiredAfflictions == null)
					{
						this.requiredAfflictions = new HashSet<ValueTuple<Identifier, float>>();
					}
					for (int i = 0; i < types.Length; i++)
					{
						this.requiredAfflictions.Add(new ValueTuple<Identifier, float>(types[i].Trim().ToIdentifier(), 0f));
					}
					continue;
					IL_88F:
					if (!Enum.TryParse<PropertyConditional.LogicalOperatorType>(attribute.Value, true, out this.conditionalLogicalOperator))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(57, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Invalid conditional comparison type \"");
						defaultInterpolatedStringHandler3.AppendFormatted(attribute.Value);
						defaultInterpolatedStringHandler3.AppendLiteral("\" in StatusEffect (");
						defaultInterpolatedStringHandler3.AppendFormatted(parentDebugName);
						defaultInterpolatedStringHandler3.AppendLiteral(")");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, element.ContentPackage, false, false);
						continue;
					}
					continue;
				}
				IL_9BC:
				if (!StatusEffect.FieldNames.Contains(attribute.Name.ToIdentifier<XName>()))
				{
					propertyAttributes.Add(attribute);
				}
			}
			if (this.Duration > 0f && !this.setValue)
			{
				propertyAttributes.RemoveAll((XAttribute a) => a.Name.ToString().Equals("tags", StringComparison.OrdinalIgnoreCase));
			}
			List<ValueTuple<Identifier, object>> propertyEffects = new List<ValueTuple<Identifier, object>>();
			foreach (XAttribute attribute2 in propertyAttributes)
			{
				Identifier attributeName = attribute2.NameAsIdentifier();
				if (attributeName == "settags")
				{
					attributeName = "tags".ToIdentifier();
				}
				propertyEffects.Add(new ValueTuple<Identifier, object>(attributeName, XMLExtensions.GetAttributeObject(attribute2)));
			}
			this.PropertyEffects = propertyEffects.ToImmutableArray<ValueTuple<Identifier, object>>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string text2 = subElement.Name.ToString().ToLowerInvariant();
				if (text2 != null)
				{
					Identifier fallbackIdentifier;
					switch (text2.Length)
					{
					case 3:
						if (!(text2 == "use"))
						{
							continue;
						}
						break;
					case 4:
					{
						char c = text2[0];
						if (c != 'f')
						{
							if (c != 'h')
							{
								continue;
							}
							if (!(text2 == "hook"))
							{
								continue;
							}
							goto IL_1719;
						}
						else
						{
							if (!(text2 == "fire"))
							{
								continue;
							}
							this.FireSize = subElement.GetAttributeFloat("size", 10f);
							continue;
						}
						break;
					}
					case 5:
					case 17:
						continue;
					case 6:
						if (!(text2 == "remove"))
						{
							continue;
						}
						goto IL_FF9;
					case 7:
					{
						char c = text2[0];
						if (c != 'l')
						{
							if (c != 'u')
							{
								continue;
							}
							if (!(text2 == "useitem"))
							{
								continue;
							}
						}
						else
						{
							if (!(text2 == "luahook"))
							{
								continue;
							}
							goto IL_1719;
						}
						break;
					}
					case 8:
						switch (text2[0])
						{
						case 'd':
							if (!(text2 == "dropitem"))
							{
								continue;
							}
							this.dropItem = true;
							continue;
						case 'e':
						case 'g':
							continue;
						case 'f':
							if (!(text2 == "forcesay"))
							{
								continue;
							}
							this.forceSayIdentifier = subElement.GetAttributeIdentifier("message", Identifier.Empty);
							this.forceSayInRadio = subElement.GetAttributeBool("sayinradio", false);
							continue;
						case 'h':
							if (!(text2 == "hidelimb"))
							{
								continue;
							}
							this.hideLimb = true;
							this.hideLimbTimer = subElement.GetAttributeFloat("duration", 0f);
							continue;
						default:
							continue;
						}
						break;
					case 9:
					{
						char c = text2[0];
						switch (c)
						{
						case 'a':
							if (!(text2 == "aitrigger"))
							{
								continue;
							}
							if (this.aiTriggers == null)
							{
								this.aiTriggers = new List<StatusEffect.AITrigger>();
							}
							this.aiTriggers.Add(new StatusEffect.AITrigger(subElement));
							continue;
						case 'b':
							if (!(text2 == "breaklimb"))
							{
								continue;
							}
							this.breakLimb = true;
							continue;
						case 'c':
						case 'd':
						case 'f':
							continue;
						case 'e':
							if (!(text2 == "explosion"))
							{
								continue;
							}
							if (this.explosions == null)
							{
								this.explosions = new List<Explosion>();
							}
							this.explosions.Add(new Explosion(subElement, parentDebugName));
							continue;
						case 'g':
							if (!(text2 == "giveskill"))
							{
								continue;
							}
							if (this.giveSkills == null)
							{
								this.giveSkills = new List<StatusEffect.GiveSkill>();
							}
							this.giveSkills.Add(new StatusEffect.GiveSkill(subElement, parentDebugName));
							continue;
						default:
						{
							if (c != 's')
							{
								continue;
							}
							if (!(text2 == "spawnitem"))
							{
								continue;
							}
							StatusEffect.ItemSpawnInfo newSpawnItem = new StatusEffect.ItemSpawnInfo(subElement, parentDebugName);
							if (newSpawnItem.ItemPrefab != null)
							{
								if (this.spawnItems == null)
								{
									this.spawnItems = new List<StatusEffect.ItemSpawnInfo>();
								}
								this.spawnItems.Add(newSpawnItem);
								continue;
							}
							continue;
						}
						}
						break;
					}
					case 10:
					{
						char c = text2[0];
						if (c != 'a')
						{
							if (c != 'r')
							{
								continue;
							}
							if (!(text2 == "removeitem"))
							{
								continue;
							}
							goto IL_FF9;
						}
						else
						{
							if (!(text2 == "affliction"))
							{
								continue;
							}
							AfflictionPrefab afflictionPrefab;
							if (subElement.GetAttribute("name") != null)
							{
								DebugConsole.ThrowError("Error in StatusEffect (" + parentDebugName + ") - define afflictions using identifiers instead of names.", null, element.ContentPackage, false, false);
								string afflictionName = subElement.GetAttributeString("name", "");
								afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab ap) => ap.Name.Equals(afflictionName, StringComparison.OrdinalIgnoreCase));
								if (afflictionPrefab == null)
								{
									DebugConsole.ThrowError(string.Concat(new string[]
									{
										"Error in StatusEffect (",
										parentDebugName,
										") - Affliction prefab \"",
										afflictionName,
										"\" not found."
									}), null, element.ContentPackage, false, false);
									continue;
								}
							}
							else
							{
								Identifier afflictionIdentifier = subElement.GetAttributeIdentifier("identifier", "");
								afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab ap) => ap.Identifier == afflictionIdentifier);
								if (afflictionPrefab == null)
								{
									DebugConsole.ThrowError(string.Concat(new string[]
									{
										"Error in StatusEffect (",
										parentDebugName,
										") - Affliction prefab with the identifier \"",
										afflictionIdentifier.ToString(),
										"\" not found."
									}), null, element.ContentPackage, false, false);
									continue;
								}
							}
							Affliction afflictionInstance = afflictionPrefab.Instantiate(subElement.GetAttributeFloat(1f, new string[]
							{
								"amount",
								"Strength"
							}), null);
							afflictionInstance.Probability = subElement.GetAttributeFloat(1f, new string[]
							{
								"Probability"
							});
							afflictionInstance.MultiplyByMaxVitality = subElement.GetAttributeBool("MultiplyByMaxVitality", false);
							afflictionInstance.DivideByLimbCount = subElement.GetAttributeBool("DivideByLimbCount", false);
							afflictionInstance.Penetration = subElement.GetAttributeFloat(0f, new string[]
							{
								"Penetration"
							});
							this.Afflictions.Add(afflictionInstance);
							continue;
						}
						break;
					}
					case 11:
					{
						char c = text2[0];
						if (c != 'c')
						{
							if (c != 'e')
							{
								continue;
							}
							if (!(text2 == "eventtarget"))
							{
								continue;
							}
							if (this.eventTargetTags == null)
							{
								this.eventTargetTags = new List<ValueTuple<Identifier, Identifier>>();
							}
							this.eventTargetTags.Add(new ValueTuple<Identifier, Identifier>(subElement.GetAttributeIdentifier("eventidentifier", Identifier.Empty), subElement.GetAttributeIdentifier("tag", Identifier.Empty)));
							continue;
						}
						else
						{
							if (!(text2 == "conditional"))
							{
								continue;
							}
							this.propertyConditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
							continue;
						}
						break;
					}
					case 12:
					{
						char c = text2[0];
						if (c != 'r')
						{
							if (c != 't')
							{
								continue;
							}
							if (!(text2 == "triggerevent"))
							{
								continue;
							}
							if (this.triggeredEvents == null)
							{
								this.triggeredEvents = new List<EventPrefab>();
							}
							Identifier identifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
							if (!identifier.IsEmpty)
							{
								EventPrefab prefab = EventSet.GetEventPrefab(identifier);
								if (prefab != null)
								{
									this.triggeredEvents.Add(prefab);
								}
							}
							using (IEnumerator<ContentXElement> enumerator4 = subElement.Elements().GetEnumerator())
							{
								while (enumerator4.MoveNext())
								{
									ContentXElement eventElement = enumerator4.Current;
									fallbackIdentifier = eventElement.NameAsIdentifier();
									if (!(fallbackIdentifier != "ScriptedEvent"))
									{
										List<EventPrefab> list = this.triggeredEvents;
										ContentXElement element2 = eventElement;
										RandomEventsFile file2 = null;
										fallbackIdentifier = default(Identifier);
										list.Add(new EventPrefab(element2, file2, fallbackIdentifier));
									}
								}
								continue;
							}
							goto IL_157C;
						}
						else
						{
							if (!(text2 == "requireditem"))
							{
								continue;
							}
							goto IL_106F;
						}
						break;
					}
					case 13:
					{
						char c = text2[2];
						if (c != 'f')
						{
							if (c != 'l')
							{
								if (c != 'q')
								{
									continue;
								}
								if (!(text2 == "requireditems"))
								{
									continue;
								}
								goto IL_106F;
							}
							else
							{
								if (!(text2 == "talenttrigger"))
								{
									continue;
								}
								if (this.talentTriggers == null)
								{
									this.talentTriggers = new List<Identifier>();
								}
								this.talentTriggers.Add(subElement.GetAttributeIdentifier("effectidentifier", Identifier.Empty));
								continue;
							}
						}
						else
						{
							if (!(text2 == "refundtalents"))
							{
								continue;
							}
							this.refundTalents = true;
							continue;
						}
						break;
					}
					case 14:
					{
						char c = text2[4];
						if (c != 'e')
						{
							if (c != 'n')
							{
								if (c != 't')
								{
									continue;
								}
								if (!(text2 == "givetalentinfo"))
								{
									continue;
								}
								StatusEffect.GiveTalentInfo newGiveTalentInfo = new StatusEffect.GiveTalentInfo(subElement, parentDebugName);
								if (newGiveTalentInfo.TalentIdentifiers.Any<Identifier>())
								{
									if (this.giveTalentInfos == null)
									{
										this.giveTalentInfos = new List<StatusEffect.GiveTalentInfo>();
									}
									this.giveTalentInfos.Add(newGiveTalentInfo);
									continue;
								}
								continue;
							}
							else
							{
								if (!(text2 == "spawncharacter"))
								{
									continue;
								}
								goto IL_157C;
							}
						}
						else
						{
							if (!(text2 == "giveexperience"))
							{
								continue;
							}
							if (this.giveExperiences == null)
							{
								this.giveExperiences = new List<int>();
							}
							this.giveExperiences.Add(subElement.GetAttributeInt("amount", 0));
							continue;
						}
						break;
					}
					case 15:
						if (!(text2 == "removecharacter"))
						{
							continue;
						}
						this.removeCharacter = true;
						this.containerForItemsOnCharacterRemoval = subElement.GetAttributeIdentifier("moveitemstocontainer", Identifier.Empty);
						continue;
					case 16:
					{
						char c = text2[0];
						if (c != 'r')
						{
							if (c != 't')
							{
								continue;
							}
							if (!(text2 == "triggeranimation"))
							{
								continue;
							}
							ContentXElement contentXElement = subElement;
							string key3 = "type";
							AnimationType animationType = AnimationType.NotDefined;
							AnimationType animType = contentXElement.GetAttributeEnum<AnimationType>(key3, animationType);
							string fileName = subElement.GetAttributeString("filename", null) ?? subElement.GetAttributeString("file", null);
							Either<string, ContentPath> file = (fileName != null) ? fileName.ToLowerInvariant() : subElement.GetAttributeContentPath("path");
							string text3;
							ContentPath contentPath2;
							ContentPath contentPath;
							if (!file.TryGet(out text3) && (!file.TryGet(out contentPath2) || (file.TryGet(out contentPath) && contentPath.IsNullOrWhiteSpace())))
							{
								DebugConsole.ThrowError("Error in a <TriggerAnimation> element of " + subElement.ParseContentPathFromUri() + ": neither path nor filename defined!", null, subElement.ContentPackage, false, false);
								continue;
							}
							float priority = subElement.GetAttributeFloat("priority", 0f);
							Identifier[] expectedSpeciesNames = subElement.GetAttributeIdentifierArray("expectedspecies", Array.Empty<Identifier>(), true);
							if (this.animationsToTrigger == null)
							{
								this.animationsToTrigger = new List<StatusEffect.AnimLoadInfo>();
							}
							this.animationsToTrigger.Add(new StatusEffect.AnimLoadInfo(animType, file, priority, expectedSpeciesNames.ToImmutableArray<Identifier>()));
							continue;
						}
						else
						{
							if (!(text2 == "reduceaffliction"))
							{
								continue;
							}
							if (subElement.GetAttribute("name") != null)
							{
								DebugConsole.ThrowError("Error in StatusEffect (" + parentDebugName + ") - define afflictions using identifiers or types instead of names.", null, element.ContentPackage, false, false);
								this.ReduceAffliction.Add(new ValueTuple<Identifier, float>(subElement.GetAttributeIdentifier("name", ""), subElement.GetAttributeFloat(1f, new string[]
								{
									"amount",
									"strength",
									"reduceamount"
								})));
								continue;
							}
							Identifier name = subElement.GetAttributeIdentifier("identifier", subElement.GetAttributeIdentifier("type", Identifier.Empty));
							if (AfflictionPrefab.List.Any((AfflictionPrefab ap) => ap.Identifier == name || ap.AfflictionType == name))
							{
								this.ReduceAffliction.Add(new ValueTuple<Identifier, float>(name, subElement.GetAttributeFloat(1f, new string[]
								{
									"amount",
									"strength",
									"reduceamount"
								})));
								continue;
							}
							DebugConsole.ThrowError(string.Concat(new string[]
							{
								"Error in StatusEffect (",
								parentDebugName,
								") - Affliction prefab with the identifier or type \"",
								name.ToString(),
								"\" not found."
							}), null, element.ContentPackage, false, false);
							continue;
						}
						break;
					}
					case 18:
					{
						char c = text2[0];
						if (c != 'd')
						{
							if (c != 'r')
							{
								continue;
							}
							if (!(text2 == "requiredaffliction"))
							{
								continue;
							}
							goto IL_10BB;
						}
						else
						{
							if (!(text2 == "dropcontaineditems"))
							{
								continue;
							}
							this.dropContainedItems = true;
							continue;
						}
						break;
					}
					case 19:
						if (!(text2 == "requiredafflictions"))
						{
							continue;
						}
						goto IL_10BB;
					default:
						continue;
					}
					this.useItemCount++;
					continue;
					IL_FF9:
					this.removeItem = true;
					continue;
					IL_106F:
					if (this.requiredItems == null)
					{
						this.requiredItems = new List<RelatedItem>();
					}
					RelatedItem newRequiredItem = RelatedItem.Load(subElement, false, parentDebugName);
					if (newRequiredItem == null)
					{
						DebugConsole.ThrowError("Error in StatusEffect config - requires an item with no identifiers.", null, element.ContentPackage, false, false);
						continue;
					}
					this.requiredItems.Add(newRequiredItem);
					continue;
					IL_10BB:
					if (this.requiredAfflictions == null)
					{
						this.requiredAfflictions = new HashSet<ValueTuple<Identifier, float>>();
					}
					Identifier[] ids = subElement.GetAttributeIdentifierArray("identifier", null, true) ?? subElement.GetAttributeIdentifierArray("type", Array.Empty<Identifier>(), true);
					foreach (Identifier afflictionId in ids)
					{
						this.requiredAfflictions.Add(new ValueTuple<Identifier, float>(afflictionId, subElement.GetAttributeFloat("minstrength", 0f)));
					}
					continue;
					IL_157C:
					StatusEffect.CharacterSpawnInfo newSpawnCharacter = new StatusEffect.CharacterSpawnInfo(subElement, parentDebugName);
					fallbackIdentifier = newSpawnCharacter.SpeciesName;
					if (!fallbackIdentifier.IsEmpty)
					{
						if (this.spawnCharacters == null)
						{
							this.spawnCharacters = new List<StatusEffect.CharacterSpawnInfo>();
						}
						this.spawnCharacters.Add(newSpawnCharacter);
						continue;
					}
					continue;
					IL_1719:
					if (this.luaHook == null)
					{
						this.luaHook = new List<ValueTuple<string, ContentXElement>>();
					}
					this.luaHook.Add(new ValueTuple<string, ContentXElement>(subElement.GetAttributeString("name", ""), subElement));
				}
			}
		}

		// Token: 0x06002E4A RID: 11850 RVA: 0x00133DE0 File Offset: 0x00131FE0
		public bool HasTargetType(StatusEffect.TargetType targetType)
		{
			return (this.targetTypes & targetType) > (StatusEffect.TargetType)0;
		}

		// Token: 0x06002E4B RID: 11851 RVA: 0x00133DF0 File Offset: 0x00131FF0
		public bool ReducesItemCondition()
		{
			foreach (ValueTuple<Identifier, object> valueTuple in this.PropertyEffects)
			{
				Identifier propertyName = valueTuple.Item1;
				object value = valueTuple.Item2;
				float conditionValue;
				if (this.ChangesItemCondition(propertyName, value, out conditionValue))
				{
					return conditionValue < 0f || (this.setValue && conditionValue <= 0f);
				}
			}
			return this.randomCondition.X < 0f || this.randomCondition.Y < 0f;
		}

		// Token: 0x06002E4C RID: 11852 RVA: 0x00133E7C File Offset: 0x0013207C
		public bool IncreasesItemCondition()
		{
			foreach (ValueTuple<Identifier, object> valueTuple in this.PropertyEffects)
			{
				Identifier propertyName = valueTuple.Item1;
				object value = valueTuple.Item2;
				float conditionValue;
				if (this.ChangesItemCondition(propertyName, value, out conditionValue))
				{
					return conditionValue > 0f || (this.setValue && conditionValue > 0f);
				}
			}
			return this.randomCondition.X > 0f || this.randomCondition.Y > 0f;
		}

		// Token: 0x06002E4D RID: 11853 RVA: 0x00133F04 File Offset: 0x00132104
		private bool ChangesItemCondition(Identifier propertyName, object value, out float conditionValue)
		{
			if (propertyName == "condition")
			{
				if (value is float)
				{
					float f = (float)value;
					conditionValue = f;
					return true;
				}
				if (value is int)
				{
					int i = (int)value;
					conditionValue = (float)i;
					return true;
				}
			}
			conditionValue = 0f;
			return false;
		}

		// Token: 0x06002E4E RID: 11854 RVA: 0x00133F54 File Offset: 0x00132154
		public bool MatchesTagConditionals(ItemPrefab itemPrefab)
		{
			return itemPrefab != null && this.HasConditions && itemPrefab.Tags.Any((Identifier t) => this.propertyConditionals.Any((PropertyConditional pc) => pc.TargetTagMatchesTagCondition(t)));
		}

		// Token: 0x06002E4F RID: 11855 RVA: 0x00133F7A File Offset: 0x0013217A
		public bool HasRequiredAfflictions(AttackResult attackResult)
		{
			return this.requiredAfflictions == null || (attackResult.Afflictions != null && !attackResult.Afflictions.None((Affliction a) => this.requiredAfflictions.Any(delegate([TupleElementNames(new string[]
			{
				"affliction",
				"strength"
			})] ValueTuple<Identifier, float> a2)
			{
				if (a.Strength >= a2.Item2)
				{
					Identifier identifier = a.Identifier;
					return identifier == a2.Item1 || a.Prefab.AfflictionType == a2.Item1;
				}
				return false;
			})));
		}

		// Token: 0x06002E50 RID: 11856 RVA: 0x00133FAC File Offset: 0x001321AC
		public virtual bool HasRequiredItems(Entity entity)
		{
			if (entity == null || this.requiredItems == null)
			{
				return true;
			}
			foreach (RelatedItem requiredItem in this.requiredItems)
			{
				Item item = entity as Item;
				if (item != null)
				{
					if (!requiredItem.CheckRequirements(null, item))
					{
						return false;
					}
				}
				else
				{
					Character character = entity as Character;
					if (character != null && !requiredItem.CheckRequirements(character, null))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06002E51 RID: 11857 RVA: 0x0013403C File Offset: 0x0013223C
		public void AddNearbyTargets(Vector2 worldPosition, List<ISerializableEntity> targets)
		{
			StatusEffect.<>c__DisplayClass120_0 CS$<>8__locals1;
			CS$<>8__locals1.worldPosition = worldPosition;
			CS$<>8__locals1.<>4__this = this;
			if (this.Range <= 0f)
			{
				return;
			}
			if (this.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
			{
				foreach (Character c in Character.CharacterList)
				{
					if (c.Enabled && !c.Removed && this.<AddNearbyTargets>g__CheckDistance|120_0(c, ref CS$<>8__locals1) && this.IsValidTarget(c))
					{
						targets.Add(c);
					}
				}
			}
			if (this.HasTargetType(StatusEffect.TargetType.NearbyItems))
			{
				if (this.TargetIdentifiers != null && this.TargetIdentifiers.Count == 1 && (this.TargetIdentifiers.Contains("powered") || this.TargetIdentifiers.Contains("junctionbox") || this.TargetIdentifiers.Contains("relaycomponent")))
				{
					using (IEnumerator<Powered> enumerator2 = Powered.PoweredList.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Powered powered = enumerator2.Current;
							if (!targets.Contains(powered))
							{
								Item item = powered.Item;
								if (!item.Removed && this.<AddNearbyTargets>g__CheckDistance|120_0(item, ref CS$<>8__locals1) && this.IsValidTarget(item))
								{
									targets.AddRange(item.AllPropertyObjects);
								}
							}
						}
						return;
					}
				}
				foreach (Item item2 in Item.ItemList)
				{
					if (!item2.Removed && this.<AddNearbyTargets>g__CheckDistance|120_0(item2, ref CS$<>8__locals1) && this.IsValidTarget(item2))
					{
						targets.AddRange(item2.AllPropertyObjects);
					}
				}
			}
		}

		// Token: 0x06002E52 RID: 11858 RVA: 0x00134224 File Offset: 0x00132424
		public bool HasRequiredConditions(IReadOnlyList<ISerializableEntity> targets)
		{
			return this.HasRequiredConditions(targets, this.propertyConditionals, false);
		}

		// Token: 0x06002E53 RID: 11859 RVA: 0x00134234 File Offset: 0x00132434
		private static bool ShouldShortCircuitLogicalOrOperator(bool condition, out bool valueToReturn)
		{
			valueToReturn = true;
			return condition;
		}

		// Token: 0x06002E54 RID: 11860 RVA: 0x0013423A File Offset: 0x0013243A
		private static bool ShouldShortCircuitLogicalAndOperator(bool condition, out bool valueToReturn)
		{
			valueToReturn = false;
			return !condition;
		}

		// Token: 0x06002E55 RID: 11861 RVA: 0x00134244 File Offset: 0x00132444
		private bool HasRequiredConditions(IReadOnlyList<ISerializableEntity> targets, IReadOnlyList<PropertyConditional> conditionals, bool targetingContainer = false)
		{
			if (conditionals.Count == 0)
			{
				return true;
			}
			if (targets.Count == 0 && this.requiredItems != null)
			{
				if (this.requiredItems.All((RelatedItem ri) => ri.MatchOnEmpty))
				{
					return true;
				}
			}
			PropertyConditional.LogicalOperatorType logicalOperatorType = this.conditionalLogicalOperator;
			ValueTuple<StatusEffect.ShouldShortCircuit, bool> valueTuple;
			if (logicalOperatorType != PropertyConditional.LogicalOperatorType.And)
			{
				if (logicalOperatorType != PropertyConditional.LogicalOperatorType.Or)
				{
					throw new NotImplementedException();
				}
				StatusEffect.ShouldShortCircuit item;
				if ((item = StatusEffect.<>O.<0>__ShouldShortCircuitLogicalOrOperator) == null)
				{
					item = (StatusEffect.<>O.<0>__ShouldShortCircuitLogicalOrOperator = new StatusEffect.ShouldShortCircuit(StatusEffect.ShouldShortCircuitLogicalOrOperator));
				}
				valueTuple = new ValueTuple<StatusEffect.ShouldShortCircuit, bool>(item, false);
			}
			else
			{
				StatusEffect.ShouldShortCircuit item2;
				if ((item2 = StatusEffect.<>O.<1>__ShouldShortCircuitLogicalAndOperator) == null)
				{
					item2 = (StatusEffect.<>O.<1>__ShouldShortCircuitLogicalAndOperator = new StatusEffect.ShouldShortCircuit(StatusEffect.ShouldShortCircuitLogicalAndOperator));
				}
				valueTuple = new ValueTuple<StatusEffect.ShouldShortCircuit, bool>(item2, true);
			}
			ValueTuple<StatusEffect.ShouldShortCircuit, bool> shortCircuitMethodPair = valueTuple;
			ValueTuple<StatusEffect.ShouldShortCircuit, bool> valueTuple2 = shortCircuitMethodPair;
			StatusEffect.ShouldShortCircuit shouldShortCircuit = valueTuple2.Item1;
			bool didNotShortCircuit = valueTuple2.Item2;
			for (int i = 0; i < conditionals.Count; i++)
			{
				PropertyConditional pc = conditionals[i];
				if (!pc.TargetContainer || targetingContainer)
				{
					bool valueToReturn;
					if (shouldShortCircuit(StatusEffect.<HasRequiredConditions>g__AnyTargetMatches|125_1(targets, pc.TargetItemComponent, pc), out valueToReturn))
					{
						return valueToReturn;
					}
				}
				else
				{
					ISerializableEntity target = StatusEffect.<HasRequiredConditions>g__FindTargetItemOrComponent|125_2(targets);
					Item item3;
					if ((item3 = (target as Item)) == null)
					{
						ItemComponent itemComponent = target as ItemComponent;
						item3 = ((itemComponent != null) ? itemComponent.Item : null);
					}
					Item targetItem = item3;
					if (((targetItem != null) ? targetItem.ParentInventory : null) == null)
					{
						bool comparisonIsNeq = pc.ComparisonOperator == PropertyConditional.ComparisonOperatorType.NotEquals;
						bool valueToReturn;
						if (shouldShortCircuit(comparisonIsNeq, out valueToReturn))
						{
							return valueToReturn;
						}
					}
					else
					{
						Entity owner = targetItem.ParentInventory.Owner;
						if (pc.TargetGrandParent)
						{
							Item ownerItem = owner as Item;
							if (ownerItem != null)
							{
								Inventory parentInventory = ownerItem.ParentInventory;
								owner = ((parentInventory != null) ? parentInventory.Owner : null);
							}
						}
						Item container = owner as Item;
						bool valueToReturn;
						if (container != null)
						{
							if (pc.Type == PropertyConditional.ConditionType.HasTag)
							{
								if (shouldShortCircuit(pc.Matches(container), out valueToReturn))
								{
									return valueToReturn;
								}
							}
							else if (shouldShortCircuit(StatusEffect.<HasRequiredConditions>g__AnyTargetMatches|125_1(container.AllPropertyObjects, pc.TargetItemComponent, pc), out valueToReturn))
							{
								return valueToReturn;
							}
						}
						Character character = owner as Character;
						if (character != null && shouldShortCircuit(pc.Matches(character), out valueToReturn))
						{
							return valueToReturn;
						}
					}
				}
			}
			return didNotShortCircuit;
		}

		// Token: 0x06002E56 RID: 11862 RVA: 0x0013445C File Offset: 0x0013265C
		protected bool IsValidTarget(ISerializableEntity entity)
		{
			Item item = entity as Item;
			if (item != null)
			{
				return this.IsValidTarget(item);
			}
			ItemComponent itemComponent = entity as ItemComponent;
			if (itemComponent != null)
			{
				return this.IsValidTarget(itemComponent);
			}
			Structure structure = entity as Structure;
			if (structure != null)
			{
				if (this.TargetIdentifiers == null)
				{
					return true;
				}
				if (this.TargetIdentifiers.Contains("structure"))
				{
					return true;
				}
				if (this.TargetIdentifiers.Contains(structure.Prefab.Identifier))
				{
					return true;
				}
			}
			else
			{
				Character character = entity as Character;
				if (character != null)
				{
					return this.IsValidTarget(character);
				}
			}
			return this.TargetIdentifiers == null || this.TargetIdentifiers.Contains(entity.Name);
		}

		// Token: 0x06002E57 RID: 11863 RVA: 0x00134508 File Offset: 0x00132708
		protected bool IsValidTarget(ItemComponent itemComponent)
		{
			return (!this.OnlyInside || itemComponent.Item.CurrentHull != null) && (!this.OnlyOutside || itemComponent.Item.CurrentHull == null) && (this.TargetItemComponent.IsNullOrEmpty() || itemComponent.Name.Equals(this.TargetItemComponent, StringComparison.OrdinalIgnoreCase)) && (this.TargetIdentifiers == null || this.TargetIdentifiers.Contains("itemcomponent") || itemComponent.Item.HasTag(this.TargetIdentifiers) || this.TargetIdentifiers.Contains(itemComponent.Item.Prefab.Identifier));
		}

		// Token: 0x06002E58 RID: 11864 RVA: 0x001345BC File Offset: 0x001327BC
		protected bool IsValidTarget(Item item)
		{
			return (!this.OnlyInside || item.CurrentHull != null) && (!this.OnlyOutside || item.CurrentHull == null) && (this.TargetIdentifiers == null || this.TargetIdentifiers.Contains("item") || item.HasTag(this.TargetIdentifiers) || this.TargetIdentifiers.Contains(item.Prefab.Identifier));
		}

		// Token: 0x06002E59 RID: 11865 RVA: 0x00134638 File Offset: 0x00132838
		protected bool IsValidTarget(Character character)
		{
			if (this.OnlyInside && character.CurrentHull == null)
			{
				return false;
			}
			if (this.OnlyOutside && character.CurrentHull != null)
			{
				return false;
			}
			if (this.TargetIdentifiers == null)
			{
				return true;
			}
			if (this.TargetIdentifiers.Contains("character"))
			{
				return true;
			}
			if (!this.TargetIdentifiers.Contains("monster"))
			{
				return this.TargetIdentifiers.Contains(character.SpeciesName);
			}
			if (!character.IsHuman)
			{
				Identifier group = character.Group;
				return group != CharacterPrefab.HumanSpeciesName;
			}
			return false;
		}

		// Token: 0x06002E5A RID: 11866 RVA: 0x001346D4 File Offset: 0x001328D4
		public void SetUser(Character user)
		{
			this.user = user;
			foreach (Affliction affliction in this.Afflictions)
			{
				affliction.Source = user;
			}
		}

		// Token: 0x06002E5B RID: 11867 RVA: 0x00134730 File Offset: 0x00132930
		public bool ShouldWaitForInterval(Entity entity, float deltaTime)
		{
			if (this.Interval > 0f && entity != null && this.intervalTimers != null)
			{
				if (this.intervalTimers.ContainsKey(entity))
				{
					Dictionary<Entity, float> dictionary = this.intervalTimers;
					dictionary[entity] -= deltaTime;
					if (this.intervalTimers[entity] > 0f)
					{
						return true;
					}
				}
				StatusEffect.intervalsToRemove.Clear();
				StatusEffect.intervalsToRemove.AddRange(from e in this.intervalTimers.Keys
				where e.Removed
				select e);
				foreach (Entity toRemove in StatusEffect.intervalsToRemove)
				{
					this.intervalTimers.Remove(toRemove);
				}
			}
			return false;
		}

		// Token: 0x06002E5C RID: 11868 RVA: 0x0013482C File Offset: 0x00132A2C
		public virtual void Apply(ActionType type, float deltaTime, Entity entity, ISerializableEntity target, Vector2? worldPosition = null)
		{
			if (this.Disabled)
			{
				return;
			}
			if (this.type != type || !this.HasRequiredItems(entity))
			{
				return;
			}
			if (!this.IsValidTarget(target))
			{
				return;
			}
			if (this.Duration > 0f && !this.Stackable)
			{
				DurationListElement existingEffect = StatusEffect.DurationList.Find((DurationListElement d) => d.Parent == this && d.Targets.FirstOrDefault<ISerializableEntity>() == target);
				if (existingEffect != null)
				{
					if (this.ResetDurationWhenReapplied)
					{
						existingEffect.Reset(Math.Max(existingEffect.Timer, this.Duration), this.user);
					}
					return;
				}
			}
			this.currentTargets.Clear();
			this.currentTargets.Add(target);
			if (!this.HasRequiredConditions(this.currentTargets))
			{
				return;
			}
			this.Apply(deltaTime, entity, this.currentTargets, worldPosition);
		}

		// Token: 0x06002E5D RID: 11869 RVA: 0x0013490C File Offset: 0x00132B0C
		public virtual void Apply(ActionType type, float deltaTime, Entity entity, IReadOnlyList<ISerializableEntity> targets, Vector2? worldPosition = null)
		{
			if (this.Disabled)
			{
				return;
			}
			if (this.type != type)
			{
				return;
			}
			if (this.ShouldWaitForInterval(entity, deltaTime))
			{
				return;
			}
			this.currentTargets.Clear();
			foreach (ISerializableEntity target in targets)
			{
				if (this.IsValidTarget(target))
				{
					this.currentTargets.Add(target);
				}
			}
			if (this.TargetIdentifiers != null && this.currentTargets.Count == 0)
			{
				return;
			}
			bool hasRequiredItems = this.HasRequiredItems(entity);
			if (!hasRequiredItems || !this.HasRequiredConditions(this.currentTargets))
			{
				return;
			}
			if (this.Duration > 0f && !this.Stackable)
			{
				DurationListElement existingEffect = StatusEffect.DurationList.Find((DurationListElement d) => d.Parent == this && d.Targets.SequenceEqual(this.currentTargets));
				if (existingEffect != null)
				{
					if (existingEffect != null)
					{
						existingEffect.Reset(Math.Max(existingEffect.Timer, this.Duration), this.user);
					}
					return;
				}
			}
			this.Apply(deltaTime, entity, this.currentTargets, worldPosition);
		}

		// Token: 0x06002E5E RID: 11870 RVA: 0x00134A1C File Offset: 0x00132C1C
		private Hull GetHull(Entity entity)
		{
			Hull hull = null;
			Character character = entity as Character;
			if (character != null)
			{
				hull = character.AnimController.CurrentHull;
			}
			else
			{
				Item item = entity as Item;
				if (item != null)
				{
					hull = item.CurrentHull;
				}
			}
			return hull;
		}

		// Token: 0x06002E5F RID: 11871 RVA: 0x00134A58 File Offset: 0x00132C58
		protected Vector2 GetPosition(Entity entity, IReadOnlyList<ISerializableEntity> targets, Vector2? worldPosition = null)
		{
			Vector2 position = worldPosition ?? ((entity == null || entity.Removed) ? Vector2.Zero : entity.WorldPosition);
			if (worldPosition == null)
			{
				Character character = entity as Character;
				if (character != null && !character.Removed && this.targetLimbs != null)
				{
					foreach (LimbType targetLimbType in this.targetLimbs)
					{
						Limb limb = character.AnimController.GetLimb(targetLimbType, true, false, false);
						if (limb != null && !limb.Removed)
						{
							position = limb.WorldPosition;
							break;
						}
					}
				}
				else if (this.HasTargetType(StatusEffect.TargetType.Contained))
				{
					for (int i = 0; i < targets.Count; i++)
					{
						Item targetItem = targets[i] as Item;
						if (targetItem != null)
						{
							position = targetItem.WorldPosition;
							break;
						}
					}
				}
				else
				{
					for (int j = 0; j < targets.Count; j++)
					{
						Limb targetLimb = targets[j] as Limb;
						if (targetLimb != null && !targetLimb.Removed)
						{
							position = targetLimb.WorldPosition;
							break;
						}
					}
				}
			}
			Vector2 offset = this.Offset;
			if (this.OffsetCopiesEntityTransform)
			{
				Item item = entity as Item;
				if (item != null)
				{
					offset *= item.Scale;
					if (item.FlippedX)
					{
						offset.X *= -1f;
					}
					if (item.FlippedY)
					{
						offset.Y *= -1f;
					}
					Vector2 position2 = offset;
					PhysicsBody body = item.body;
					offset = Vector2.Transform(position2, Matrix.CreateRotationZ((body != null) ? body.Rotation : (-item.RotationRad)));
				}
			}
			position += offset;
			return position + Rand.Vector(Rand.Range(0f, this.RandomOffset, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced);
		}

		// Token: 0x06002E60 RID: 11872 RVA: 0x00134C28 File Offset: 0x00132E28
		protected void Apply(float deltaTime, Entity entity, IReadOnlyList<ISerializableEntity> targets, Vector2? worldPosition = null)
		{
			StatusEffect.<>c__DisplayClass138_0 CS$<>8__locals1 = new StatusEffect.<>c__DisplayClass138_0();
			CS$<>8__locals1.entity = entity;
			CS$<>8__locals1.targets = targets;
			CS$<>8__locals1.<>4__this = this;
			if (this.Disabled)
			{
				return;
			}
			if (this.lifeTime > 0f)
			{
				this.lifeTimer -= deltaTime;
				if (this.lifeTimer <= 0f)
				{
					return;
				}
			}
			if (this.ShouldWaitForInterval(CS$<>8__locals1.entity, deltaTime))
			{
				return;
			}
			Item item = CS$<>8__locals1.entity as Item;
			if (item != null)
			{
				bool? result = LuaCsSetup.Instance.Hook.Call<bool?>("statusEffect.apply." + item.Prefab.Identifier.ToString(), new object[]
				{
					this,
					deltaTime,
					CS$<>8__locals1.entity,
					CS$<>8__locals1.targets,
					worldPosition
				});
				if (result != null && result.Value)
				{
					return;
				}
			}
			Character character = CS$<>8__locals1.entity as Character;
			if (character != null)
			{
				bool? result2 = LuaCsSetup.Instance.Hook.Call<bool?>("statusEffect.apply." + character.SpeciesName.ToString(), new object[]
				{
					this,
					deltaTime,
					CS$<>8__locals1.entity,
					CS$<>8__locals1.targets,
					worldPosition
				});
				if (result2 != null && result2.Value)
				{
					return;
				}
			}
			if (this.luaHook != null)
			{
				foreach (ValueTuple<string, ContentXElement> valueTuple in this.luaHook)
				{
					string hookName = valueTuple.Item1;
					ContentXElement element = valueTuple.Item2;
					bool? result3 = LuaCsSetup.Instance.Hook.Call<bool?>(hookName, new object[]
					{
						this,
						deltaTime,
						CS$<>8__locals1.entity,
						CS$<>8__locals1.targets,
						worldPosition,
						element
					});
					if (result3 != null && result3.Value)
					{
						return;
					}
				}
			}
			Item parentItem = CS$<>8__locals1.entity as Item;
			PhysicsBody physicsBody = (parentItem != null) ? parentItem.body : null;
			Hull hull = this.GetHull(CS$<>8__locals1.entity);
			CS$<>8__locals1.position = this.GetPosition(CS$<>8__locals1.entity, CS$<>8__locals1.targets, worldPosition);
			if (this.useItemCount > 0)
			{
				Character useTargetCharacter = null;
				Limb useTargetLimb = null;
				int i = 0;
				while (i < CS$<>8__locals1.targets.Count)
				{
					Character character2 = CS$<>8__locals1.targets[i] as Character;
					if (character2 != null && !character2.Removed)
					{
						useTargetCharacter = character2;
						break;
					}
					Limb limb = CS$<>8__locals1.targets[i] as Limb;
					if (limb != null && limb.character != null && !limb.character.Removed)
					{
						useTargetLimb = limb;
						if (useTargetCharacter == null)
						{
							useTargetCharacter = limb.character;
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
				for (int j = 0; j < CS$<>8__locals1.targets.Count; j++)
				{
					Item item2 = CS$<>8__locals1.targets[j] as Item;
					if (item2 != null)
					{
						for (int k = 0; k < this.useItemCount; k++)
						{
							if (!item2.Removed)
							{
								item2.Use(deltaTime, null, useTargetLimb, useTargetCharacter, null);
							}
						}
					}
				}
			}
			if (this.dropItem)
			{
				for (int l = 0; l < CS$<>8__locals1.targets.Count; l++)
				{
					Item item3 = CS$<>8__locals1.targets[l] as Item;
					if (item3 != null)
					{
						item3.Drop(null, true, true);
					}
				}
			}
			if (this.dropContainedItems)
			{
				int m = 0;
				while (m < CS$<>8__locals1.targets.Count)
				{
					Item item4 = CS$<>8__locals1.targets[m] as Item;
					if (item4 != null)
					{
						using (IEnumerator<ItemContainer> enumerator2 = item4.GetComponents<ItemContainer>().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								ItemContainer itemContainer = enumerator2.Current;
								foreach (Item containedItem in itemContainer.Inventory.AllItemsMod)
								{
									containedItem.Drop(null, true, true);
								}
							}
							goto IL_47E;
						}
						goto IL_41E;
					}
					goto IL_41E;
					IL_47E:
					m++;
					continue;
					IL_41E:
					Character character3 = CS$<>8__locals1.targets[m] as Character;
					if (character3 != null && character3.Inventory != null)
					{
						foreach (Item containedItem2 in character3.Inventory.AllItemsMod)
						{
							containedItem2.Drop(null, true, true);
						}
						goto IL_47E;
					}
					goto IL_47E;
				}
			}
			if (this.removeItem)
			{
				for (int n = 0; n < CS$<>8__locals1.targets.Count; n++)
				{
					Item item5 = CS$<>8__locals1.targets[n] as Item;
					if (item5 != null)
					{
						EntitySpawner spawner = Entity.Spawner;
						if (spawner != null)
						{
							spawner.AddItemToRemoveQueue(item5);
						}
					}
				}
			}
			if (this.removeCharacter)
			{
				for (int i2 = 0; i2 < CS$<>8__locals1.targets.Count; i2++)
				{
					Character targetCharacter9 = StatusEffect.GetCharacterFromTarget(CS$<>8__locals1.targets[i2]);
					if (targetCharacter9 != null)
					{
						this.RemoveCharacter(targetCharacter9);
					}
				}
			}
			if (this.breakLimb || this.hideLimb)
			{
				for (int i3 = 0; i3 < CS$<>8__locals1.targets.Count; i3++)
				{
					ISerializableEntity target = CS$<>8__locals1.targets[i3];
					Limb targetLimb = target as Limb;
					if (targetLimb == null)
					{
						Character character4 = target as Character;
						if (character4 != null)
						{
							foreach (Limb limb2 in character4.AnimController.Limbs)
							{
								if (limb2.body == this.sourceBody)
								{
									targetLimb = limb2;
									break;
								}
							}
						}
					}
					if (targetLimb != null)
					{
						if (this.breakLimb)
						{
							targetLimb.character.TrySeverLimbJoints(targetLimb, 1f, -1f, true, true, this.user);
						}
						if (this.hideLimb)
						{
							targetLimb.HideAndDisable(this.hideLimbTimer, true);
						}
					}
				}
			}
			if (this.Duration > 0f)
			{
				StatusEffect.DurationList.Add(new DurationListElement(this, CS$<>8__locals1.entity, CS$<>8__locals1.targets, this.Duration, this.user));
			}
			else
			{
				for (int i4 = 0; i4 < CS$<>8__locals1.targets.Count; i4++)
				{
					ISerializableEntity target2 = CS$<>8__locals1.targets[i4];
					if (((target2 != null) ? target2.SerializableProperties : null) != null)
					{
						Entity targetEntity = target2 as Entity;
						if (targetEntity != null)
						{
							if (targetEntity.Removed)
							{
								goto IL_74D;
							}
							Item targetItem = targetEntity as Item;
							if (targetItem != null && this.randomCondition != Vector2.Zero)
							{
								float newCondition = Rand.Range(this.randomCondition.X, this.randomCondition.Y, Rand.RandSync.Unsynced);
								targetItem.Condition = this.GetModifiedValue(targetItem.Condition, newCondition, deltaTime);
							}
						}
						else
						{
							Limb limb3 = target2 as Limb;
							if (limb3 != null)
							{
								if (limb3.Removed)
								{
									goto IL_74D;
								}
								CS$<>8__locals1.position = limb3.WorldPosition + this.Offset;
							}
						}
						foreach (ValueTuple<Identifier, object> valueTuple2 in this.PropertyEffects)
						{
							Identifier propertyName = valueTuple2.Item1;
							object value = valueTuple2.Item2;
							SerializableProperty property;
							if (target2.SerializableProperties.TryGetValue(propertyName, out property))
							{
								this.ApplyToProperty(target2, property, value, deltaTime);
							}
						}
					}
					IL_74D:;
				}
			}
			if (this.explosions != null)
			{
				foreach (Explosion explosion in this.explosions)
				{
					explosion.Explode(CS$<>8__locals1.position, CS$<>8__locals1.entity, this.user);
				}
			}
			bool isNotClient = GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient;
			Character c;
			for (int i5 = 0; i5 < CS$<>8__locals1.targets.Count; i5++)
			{
				ISerializableEntity target3 = CS$<>8__locals1.targets[i5];
				if (this.Duration > 0f)
				{
					break;
				}
				if (target3 != null)
				{
					foreach (Affliction affliction in this.Afflictions)
					{
						Character character5 = target3 as Character;
						if (character5 != null)
						{
							if (!character5.Removed)
							{
								Affliction newAffliction = this.GetMultipliedAffliction(affliction, CS$<>8__locals1.entity, character5, deltaTime, this.multiplyAfflictionsByMaxVitality);
								character5.LastDamageSource = CS$<>8__locals1.entity;
								foreach (Limb limb4 in character5.AnimController.Limbs)
								{
									if (this.IsValidTargetLimb(limb4))
									{
										Character character7 = limb4.character;
										Vector2 position = CS$<>8__locals1.position;
										Limb hitLimb2 = limb4;
										IEnumerable<Affliction> afflictions = newAffliction.ToEnumerable<Affliction>();
										float stun = 0f;
										bool playSound = false;
										Vector2 zero = Vector2.Zero;
										Character source = affliction.Source;
										float damageMultiplier = 1f;
										float penetration = newAffliction.Penetration;
										AttackResult result4 = character7.DamageLimb(position, hitLimb2, afflictions, stun, playSound, zero, source, damageMultiplier, !this.setValue, penetration, false, false, true);
										limb4.character.TrySeverLimbJoints(limb4, this.SeverLimbsProbability, this.disableDeltaTime ? result4.Damage : (result4.Damage / deltaTime), true, false, affliction.Source);
										this.RegisterTreatmentResults(this.user, CS$<>8__locals1.entity as Item, limb4, affliction, result4);
										if (!affliction.Prefab.LimbSpecific)
										{
											break;
										}
									}
								}
							}
						}
						else
						{
							Limb limb5 = target3 as Limb;
							if (limb5 != null && this.IsValidTargetLimb(limb5))
							{
								Affliction newAffliction = this.GetMultipliedAffliction(affliction, CS$<>8__locals1.entity, limb5.character, deltaTime, this.multiplyAfflictionsByMaxVitality);
								Character character8 = limb5.character;
								Vector2 position2 = CS$<>8__locals1.position;
								Limb hitLimb3 = limb5;
								IEnumerable<Affliction> afflictions2 = newAffliction.ToEnumerable<Affliction>();
								float stun2 = 0f;
								bool playSound2 = false;
								Vector2 zero2 = Vector2.Zero;
								Character source2 = affliction.Source;
								float damageMultiplier2 = 1f;
								float penetration = newAffliction.Penetration;
								AttackResult result5 = character8.DamageLimb(position2, hitLimb3, afflictions2, stun2, playSound2, zero2, source2, damageMultiplier2, !this.setValue, penetration, false, false, true);
								limb5.character.TrySeverLimbJoints(limb5, this.SeverLimbsProbability, this.disableDeltaTime ? result5.Damage : (result5.Damage / deltaTime), true, false, affliction.Source);
								this.RegisterTreatmentResults(this.user, CS$<>8__locals1.entity as Item, limb5, affliction, result5);
							}
						}
					}
					foreach (ValueTuple<Identifier, float> valueTuple3 in this.ReduceAffliction)
					{
						Identifier affliction2 = valueTuple3.Item1;
						float amount = valueTuple3.Item2;
						Limb targetLimb2 = null;
						Character targetCharacter2 = null;
						Character character6 = target3 as Character;
						if (character6 != null)
						{
							targetCharacter2 = character6;
						}
						else
						{
							Limb limb6 = target3 as Limb;
							if (limb6 != null && !limb6.Removed)
							{
								targetLimb2 = limb6;
								targetCharacter2 = limb6.character;
							}
						}
						if (targetCharacter2 != null && !targetCharacter2.Removed)
						{
							ActionType? actionType = null;
							Item item6 = CS$<>8__locals1.entity as Item;
							if (item6 != null && item6.UseInHealthInterface)
							{
								actionType = new ActionType?(this.type);
							}
							float reduceAmount = amount * this.GetAfflictionMultiplier(CS$<>8__locals1.entity, targetCharacter2, deltaTime);
							float prevVitality = targetCharacter2.Vitality;
							if (targetLimb2 != null)
							{
								CharacterHealth characterHealth = targetCharacter2.CharacterHealth;
								Limb targetLimb5 = targetLimb2;
								Identifier afflictionIdOrType = affliction2;
								float amount3 = reduceAmount;
								Character attacker = this.user;
								characterHealth.ReduceAfflictionOnLimb(targetLimb5, afflictionIdOrType, amount3, actionType, attacker);
							}
							else
							{
								CharacterHealth characterHealth2 = targetCharacter2.CharacterHealth;
								Identifier afflictionIdOrType2 = affliction2;
								float amount4 = reduceAmount;
								Character attacker = this.user;
								characterHealth2.ReduceAfflictionOnAllLimbs(afflictionIdOrType2, amount4, actionType, attacker);
							}
							if (!targetCharacter2.IsDead)
							{
								float healthChange = targetCharacter2.Vitality - prevVitality;
								AIController aicontroller = targetCharacter2.AIController;
								if (aicontroller != null)
								{
									aicontroller.OnHealed(this.user, healthChange);
								}
								if (this.user != null && this.CanGiveMedicalSkill)
								{
									targetCharacter2.TryAdjustHealerSkill(this.user, healthChange, null);
									GameMain.Server.KarmaManager.OnCharacterHealthChanged(targetCharacter2, this.user, -healthChange, 0f, null);
								}
							}
						}
					}
					if (this.aiTriggers != null)
					{
						Character targetCharacter3 = target3 as Character;
						if (targetCharacter3 == null)
						{
							Limb targetLimb3 = target3 as Limb;
							if (targetLimb3 != null && !targetLimb3.Removed)
							{
								targetCharacter3 = targetLimb3.character;
							}
						}
						Character entityCharacter = CS$<>8__locals1.entity as Character;
						if (targetCharacter3 == null)
						{
							targetCharacter3 = entityCharacter;
						}
						if (targetCharacter3 != null && !targetCharacter3.Removed && !targetCharacter3.IsPlayer)
						{
							EnemyAIController enemyAI = targetCharacter3.AIController as EnemyAIController;
							if (enemyAI != null)
							{
								foreach (StatusEffect.AITrigger trigger in this.aiTriggers)
								{
									if (Rand.Value(Rand.RandSync.Unsynced) <= trigger.Probability)
									{
										if (entityCharacter != targetCharacter3)
										{
											Limb targetLimb4 = target3 as Limb;
											if (targetLimb4 != null)
											{
												Limb hitLimb = targetCharacter3.LastDamage.HitLimb;
												if (hitLimb != null && hitLimb != targetLimb4)
												{
													continue;
												}
											}
										}
										if (targetCharacter3.LastDamage.Damage >= trigger.MinDamage)
										{
											enemyAI.LaunchTrigger(trigger);
											break;
										}
									}
								}
							}
						}
					}
					if (this.talentTriggers != null)
					{
						Character targetCharacter4 = StatusEffect.GetCharacterFromTarget(target3);
						if (targetCharacter4 != null && !targetCharacter4.Removed)
						{
							foreach (Identifier talentTrigger in this.talentTriggers)
							{
								targetCharacter4.CheckTalents(AbilityEffectType.OnStatusEffectIdentifier, new StatusEffect.AbilityStatusEffectIdentifier(talentTrigger));
							}
						}
					}
					this.TryTriggerAnimation(target3, CS$<>8__locals1.entity);
					if (!this.forceSayIdentifier.IsEmpty)
					{
						LocalizedString messageToSay = TextManager.Get(this.forceSayIdentifier).Fallback(this.forceSayIdentifier.Value, true);
						if (!messageToSay.IsNullOrEmpty())
						{
							Character targetCharacter5 = target3 as Character;
							if (targetCharacter5 != null)
							{
								targetCharacter5.ForceSay(messageToSay, this.forceSayInRadio, false, 0f);
							}
						}
					}
					if (isNotClient)
					{
						if (this.giveExperiences != null)
						{
							foreach (int giveExperience in this.giveExperiences)
							{
								Character targetCharacter6 = StatusEffect.GetCharacterFromTarget(target3);
								if (targetCharacter6 != null && !targetCharacter6.Removed && targetCharacter6 != null)
								{
									CharacterInfo info = targetCharacter6.Info;
									if (info != null)
									{
										info.GiveExperience(giveExperience);
									}
								}
							}
						}
						if (this.giveSkills != null)
						{
							StatusEffect.<>c__DisplayClass138_1 CS$<>8__locals2;
							CS$<>8__locals2.targetCharacter = StatusEffect.GetCharacterFromTarget(target3);
							if (CS$<>8__locals2.targetCharacter != null && !CS$<>8__locals2.targetCharacter.Removed)
							{
								foreach (StatusEffect.GiveSkill giveSkill in this.giveSkills)
								{
									Identifier skillIdentifier = (giveSkill.SkillIdentifier == "randomskill") ? StatusEffect.<Apply>g__GetRandomSkill|138_1(ref CS$<>8__locals2) : giveSkill.SkillIdentifier;
									float amount2 = giveSkill.UseDeltaTime ? (giveSkill.Amount * deltaTime) : giveSkill.Amount;
									if (giveSkill.Proportional)
									{
										CharacterInfo info2 = CS$<>8__locals2.targetCharacter.Info;
										if (info2 != null)
										{
											info2.ApplySkillGain(skillIdentifier, amount2, !giveSkill.TriggerTalents, 2f, giveSkill.AlwayShowNotification);
										}
									}
									else
									{
										CharacterInfo info3 = CS$<>8__locals2.targetCharacter.Info;
										if (info3 != null)
										{
											info3.IncreaseSkillLevel(skillIdentifier, amount2, !giveSkill.TriggerTalents, giveSkill.AlwayShowNotification);
										}
									}
								}
							}
						}
						if (this.refundTalents)
						{
							c = StatusEffect.GetCharacterFromTarget(target3);
							if (c != null && !c.Removed)
							{
								CharacterInfo info4 = c.Info;
								if (info4 != null)
								{
									info4.AddRefundPoints(1);
								}
							}
						}
						if (this.giveTalentInfos != null)
						{
							Character targetCharacter = StatusEffect.GetCharacterFromTarget(target3);
							Character targetCharacter8 = targetCharacter;
							TalentTree characterTalentTree;
							if (((targetCharacter8 != null) ? targetCharacter8.Info : null) == null || !TalentTree.JobTalentTrees.TryGet(targetCharacter.Info.Job.Prefab.Identifier, out characterTalentTree))
							{
								goto IL_11AE;
							}
							Func<Identifier, bool> <>9__2;
							foreach (StatusEffect.GiveTalentInfo giveTalentInfo in this.giveTalentInfos)
							{
								if (giveTalentInfo.GiveRandom)
								{
									IEnumerable<Identifier> talentIdentifiers = giveTalentInfo.TalentIdentifiers;
									Func<Identifier, bool> predicate;
									if ((predicate = <>9__2) == null)
									{
										predicate = (<>9__2 = ((Identifier id) => !targetCharacter.Info.UnlockedTalents.Contains(id) && !characterTalentTree.AllTalentIdentifiers.Contains(id)));
									}
									IEnumerable<Identifier> viableTalents = talentIdentifiers.Where(predicate);
									if (!viableTalents.None(null))
									{
										targetCharacter.GiveTalent(viableTalents.GetRandomUnsynced<Identifier>(), true);
									}
								}
								else
								{
									foreach (Identifier id2 in giveTalentInfo.TalentIdentifiers)
									{
										if (!targetCharacter.Info.UnlockedTalents.Contains(id2) && !characterTalentTree.AllTalentIdentifiers.Contains(id2))
										{
											targetCharacter.GiveTalent(id2, true);
										}
									}
								}
							}
						}
						if (this.eventTargetTags != null)
						{
							using (List<ValueTuple<Identifier, Identifier>>.Enumerator enumerator14 = this.eventTargetTags.GetEnumerator())
							{
								while (enumerator14.MoveNext())
								{
									StatusEffect.<>c__DisplayClass138_3 CS$<>8__locals4 = new StatusEffect.<>c__DisplayClass138_3();
									ValueTuple<Identifier, Identifier> valueTuple4 = enumerator14.Current;
									CS$<>8__locals4.eventId = valueTuple4.Item1;
									CS$<>8__locals4.tag = valueTuple4.Item2;
									Event @event = GameMain.GameSession.EventManager.ActiveEvents.FirstOrDefault((Event e) => e.Prefab.Identifier == CS$<>8__locals4.eventId);
									ScriptedEvent ev = @event as ScriptedEvent;
									if (ev != null)
									{
										(from t in CS$<>8__locals1.targets
										where t is Entity
										select t).ForEach(delegate(ISerializableEntity t)
										{
											ev.AddTarget(CS$<>8__locals4.tag, (Entity)t);
										});
									}
								}
							}
						}
					}
				}
				IL_11AE:;
			}
			if (this.FireSize > 0f && CS$<>8__locals1.entity != null)
			{
				FireSource fire = new FireSource(CS$<>8__locals1.position, hull, this.user, false);
				fire.Size = new Vector2(this.FireSize, fire.Size.Y);
			}
			if (isNotClient && this.UnlockRecipes.Any<Identifier>())
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					foreach (Identifier unlockRecipe in this.UnlockRecipes)
					{
						Character character9;
						if ((character9 = this.user) == null)
						{
							IEnumerable<ISerializableEntity> targets2 = CS$<>8__locals1.targets;
							Func<ISerializableEntity, Character> selector;
							if ((selector = StatusEffect.<>O.<2>__GetCharacterFromTarget) == null)
							{
								selector = (StatusEffect.<>O.<2>__GetCharacterFromTarget = new Func<ISerializableEntity, Character>(StatusEffect.GetCharacterFromTarget));
							}
							character9 = targets2.Select(selector).NotNull<Character>().FirstOrDefault<Character>();
						}
						Character targetCharacter7 = character9;
						gameSession.UnlockRecipe((targetCharacter7 != null) ? targetCharacter7.TeamID : CharacterTeamType.Team1, unlockRecipe, true);
					}
				}
			}
			if (isNotClient && this.triggeredEvents != null)
			{
				GameSession gameSession2 = GameMain.GameSession;
				EventManager eventManager = (gameSession2 != null) ? gameSession2.EventManager : null;
				if (eventManager != null)
				{
					foreach (EventPrefab eventPrefab in this.triggeredEvents)
					{
						Event ev2 = eventPrefab.CreateInstance(eventManager.RandomSeed);
						if (ev2 != null)
						{
							eventManager.QueuedEvents.Enqueue(ev2);
							ScriptedEvent scriptedEvent = ev2 as ScriptedEvent;
							if (scriptedEvent != null)
							{
								if (!this.triggeredEventTargetTag.IsEmpty)
								{
									IEnumerable<ISerializableEntity> eventTargets = from t in CS$<>8__locals1.targets
									where t is Entity
									select t;
									if (eventTargets.Any<ISerializableEntity>())
									{
										scriptedEvent.Targets.Add(this.triggeredEventTargetTag, eventTargets.Cast<Entity>().ToList<Entity>());
									}
								}
								if (!this.triggeredEventEntityTag.IsEmpty && CS$<>8__locals1.entity != null)
								{
									scriptedEvent.Targets.Add(this.triggeredEventEntityTag, new List<Entity>
									{
										CS$<>8__locals1.entity
									});
								}
								if (!this.triggeredEventUserTag.IsEmpty && this.user != null)
								{
									scriptedEvent.Targets.Add(this.triggeredEventUserTag, new List<Entity>
									{
										this.user
									});
								}
							}
						}
					}
				}
			}
			if (isNotClient && CS$<>8__locals1.entity != null && Entity.Spawner != null)
			{
				if (this.spawnCharacters != null)
				{
					using (List<StatusEffect.CharacterSpawnInfo>.Enumerator enumerator17 = this.spawnCharacters.GetEnumerator())
					{
						while (enumerator17.MoveNext())
						{
							StatusEffect.CharacterSpawnInfo characterSpawnInfo = enumerator17.Current;
							List<Character> characters = new List<Character>();
							CharacterTeamType? inheritedTeam = null;
							if (characterSpawnInfo.InheritTeam)
							{
								GameSession gameSession3 = GameMain.GameSession;
								GameModePreset gameModePreset;
								if (gameSession3 == null)
								{
									gameModePreset = null;
								}
								else
								{
									GameMode gameMode = gameSession3.GameMode;
									gameModePreset = ((gameMode != null) ? gameMode.Preset : null);
								}
								StatusEffect.<>c__DisplayClass138_6 CS$<>8__locals7;
								CS$<>8__locals7.isPvP = (gameModePreset == GameModePreset.PvP);
								Character c2 = CS$<>8__locals1.entity as Character;
								CharacterTeamType? characterTeamType;
								if (c2 == null)
								{
									Item it = CS$<>8__locals1.entity as Item;
									if (it == null)
									{
										MapEntity e2 = CS$<>8__locals1.entity as MapEntity;
										if (e2 == null)
										{
											characterTeamType = null;
										}
										else
										{
											characterTeamType = StatusEffect.<Apply>g__GetTeamFromSubmarine|138_7(e2, ref CS$<>8__locals7);
										}
									}
									else
									{
										Character character10;
										if ((character10 = (it.GetRootInventoryOwner() as Character)) == null)
										{
											Inventory previousParentInventory = it.PreviousParentInventory;
											character10 = (((previousParentInventory != null) ? previousParentInventory.Owner : null) as Character);
										}
										Character owner = character10;
										characterTeamType = ((owner != null) ? new CharacterTeamType?(owner.TeamID) : StatusEffect.<Apply>g__GetTeamFromSubmarine|138_7(it, ref CS$<>8__locals7));
									}
								}
								else
								{
									characterTeamType = new CharacterTeamType?(c2.TeamID);
								}
								CharacterTeamType? characterTeamType2 = characterTeamType;
								inheritedTeam = new CharacterTeamType?(characterTeamType2 ?? (CS$<>8__locals7.isPvP ? CharacterTeamType.None : CharacterTeamType.Team1));
							}
							int i8;
							int i;
							Action<Character> <>9__8;
							for (int i = 0; i < characterSpawnInfo.Count; i = i8 + 1)
							{
								EntitySpawner spawner2 = Entity.Spawner;
								Identifier speciesName = characterSpawnInfo.SpeciesName;
								Vector2 worldPosition2 = CS$<>8__locals1.position + Rand.Vector(characterSpawnInfo.Spread, Rand.RandSync.Unsynced) + characterSpawnInfo.Offset;
								Action<Character> onSpawn;
								if ((onSpawn = <>9__8) == null)
								{
									onSpawn = (<>9__8 = delegate(Character newCharacter)
									{
										if (inheritedTeam != null)
										{
											newCharacter.SetOriginalTeamAndChangeTeam(inheritedTeam.Value, true);
										}
										if (characterSpawnInfo.TotalMaxCount <= 0 || Character.CharacterList.Count(delegate(Character c)
										{
											Identifier speciesName2 = c.SpeciesName;
											Identifier speciesName3 = characterSpawnInfo.SpeciesName;
											return speciesName2 == speciesName3 && c.TeamID == newCharacter.TeamID;
										}) <= characterSpawnInfo.TotalMaxCount)
										{
											EnemyAIController enemyAi = newCharacter.AIController as EnemyAIController;
											if (enemyAi != null && enemyAi.PetBehavior != null)
											{
												Item item7 = CS$<>8__locals1.entity as Item;
												if (item7 != null)
												{
													CharacterInventory inv = item7.ParentInventory as CharacterInventory;
													if (inv != null)
													{
														enemyAi.PetBehavior.Owner = (inv.Owner as Character);
													}
												}
											}
											characters.Add(newCharacter);
											if (characters.Count == characterSpawnInfo.Count)
											{
												SwarmBehavior.CreateSwarm(characters.Cast<AICharacter>());
											}
											if (!characterSpawnInfo.AfflictionOnSpawn.IsEmpty)
											{
												AfflictionPrefab afflictionPrefab;
												if (!AfflictionPrefab.Prefabs.TryGet(characterSpawnInfo.AfflictionOnSpawn, out afflictionPrefab))
												{
													DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(102, 1);
													defaultInterpolatedStringHandler.AppendLiteral("Could not apply an affliction to the spawned character(s). No affliction with the identifier \"");
													defaultInterpolatedStringHandler.AppendFormatted<Identifier>(characterSpawnInfo.AfflictionOnSpawn);
													defaultInterpolatedStringHandler.AppendLiteral("\" found.");
													DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
													return;
												}
												newCharacter.CharacterHealth.ApplyAffliction(newCharacter.AnimController.MainLimb, afflictionPrefab.Instantiate((float)characterSpawnInfo.AfflictionStrength, null), true, false, true);
											}
											if (characterSpawnInfo.Stun > 0)
											{
												newCharacter.SetStun((float)characterSpawnInfo.Stun, false, false);
											}
											foreach (ISerializableEntity target4 in CS$<>8__locals1.targets)
											{
												Character character11 = target4 as Character;
												if (character11 != null)
												{
													if (characterSpawnInfo.TransferInventory && character11.Inventory != null && newCharacter.Inventory != null)
													{
														if (character11.Inventory.Capacity != newCharacter.Inventory.Capacity)
														{
															return;
														}
														int i = 0;
														while (i < character11.Inventory.Capacity && i < newCharacter.Inventory.Capacity)
														{
															character11.Inventory.GetItemsAt(i).ForEachMod(delegate(Item item)
															{
																newCharacter.Inventory.TryPutItem(item, i, true, false, null, true, false, true);
															});
															int i9 = i;
															i = i9 + 1;
														}
													}
													if (characterSpawnInfo.TransferBuffs || characterSpawnInfo.TransferAfflictions)
													{
														foreach (Affliction affliction3 in character11.CharacterHealth.GetAllAfflictions())
														{
															if (affliction3.Prefab.IsBuff)
															{
																if (!characterSpawnInfo.TransferBuffs)
																{
																	continue;
																}
															}
															else if (!characterSpawnInfo.TransferAfflictions)
															{
																continue;
															}
															float afflictionStrength = affliction3.Strength * (newCharacter.MaxVitality / 100f);
															Limb newAfflictionLimb = newCharacter.AnimController.MainLimb;
															if (!character11.Removed)
															{
																Limb afflictionLimb = character11.CharacterHealth.GetAfflictionLimb(affliction3) ?? character11.AnimController.MainLimb;
																newAfflictionLimb = (newCharacter.AnimController.GetLimb(afflictionLimb.type, true, false, false) ?? newCharacter.AnimController.MainLimb);
															}
															newCharacter.CharacterHealth.ApplyAffliction(newAfflictionLimb, affliction3.Prefab.Instantiate(afflictionStrength, null), true, false, true);
														}
													}
													if (i == characterSpawnInfo.Count)
													{
														if (characterSpawnInfo.TransferControl)
														{
															foreach (Client c3 in GameMain.Server.ConnectedClients)
															{
																if (c3.Character == target4)
																{
																	GameMain.Server.SetClientCharacter(c3, newCharacter);
																}
															}
														}
														if (characterSpawnInfo.RemovePreviousCharacter)
														{
															EntitySpawner spawner3 = Entity.Spawner;
															if (spawner3 != null)
															{
																spawner3.AddEntityToRemoveQueue(character11);
															}
														}
													}
												}
											}
											if (characterSpawnInfo.InheritEventTags)
											{
												foreach (Event activeEvent in GameMain.GameSession.EventManager.ActiveEvents)
												{
													ScriptedEvent scriptedEvent2 = activeEvent as ScriptedEvent;
													if (scriptedEvent2 != null)
													{
														scriptedEvent2.InheritTags(CS$<>8__locals1.entity, newCharacter);
													}
												}
											}
											return;
										}
										EntitySpawner spawner4 = Entity.Spawner;
										if (spawner4 == null)
										{
											return;
										}
										spawner4.AddEntityToRemoveQueue(newCharacter);
									});
								}
								spawner2.AddCharacterToSpawnQueue(speciesName, worldPosition2, onSpawn);
								i8 = i;
							}
						}
					}
				}
				if (this.spawnItems != null && this.spawnItems.Count > 0)
				{
					if (this.spawnItemRandomly)
					{
						if (this.spawnItems.Count > 0)
						{
							StatusEffect.ItemSpawnInfo randomSpawn = this.spawnItems.GetRandomUnsynced<StatusEffect.ItemSpawnInfo>();
							int count = randomSpawn.GetCount(Rand.RandSync.Unsynced);
							if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < randomSpawn.Probability)
							{
								for (int i6 = 0; i6 < count; i6++)
								{
									CS$<>8__locals1.<Apply>g__ProcessItemSpawnInfo|0(randomSpawn);
								}
							}
						}
					}
					else
					{
						foreach (StatusEffect.ItemSpawnInfo itemSpawnInfo in this.spawnItems)
						{
							int count2 = itemSpawnInfo.GetCount(Rand.RandSync.Unsynced);
							if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < itemSpawnInfo.Probability)
							{
								for (int i7 = 0; i7 < count2; i7++)
								{
									CS$<>8__locals1.<Apply>g__ProcessItemSpawnInfo|0(itemSpawnInfo);
								}
							}
						}
					}
				}
			}
			if (this.oneShot)
			{
				this.Disabled = true;
			}
			if (this.Interval > 0f && CS$<>8__locals1.entity != null)
			{
				if (this.intervalTimers == null)
				{
					this.intervalTimers = new Dictionary<Entity, float>();
				}
				this.intervalTimers[CS$<>8__locals1.entity] = this.Interval;
			}
		}

		// Token: 0x06002E61 RID: 11873 RVA: 0x00136588 File Offset: 0x00134788
		private bool IsValidTargetLimb(Limb limb)
		{
			return limb != null && !limb.Removed && !limb.IsSevered && (this.targetLimbs == null || this.targetLimbs.Contains(limb.type));
		}

		// Token: 0x06002E62 RID: 11874 RVA: 0x001365C0 File Offset: 0x001347C0
		private static Character GetCharacterFromTarget(ISerializableEntity target)
		{
			Character targetCharacter = target as Character;
			if (targetCharacter == null)
			{
				Limb targetLimb = target as Limb;
				if (targetLimb != null && !targetLimb.Removed)
				{
					targetCharacter = targetLimb.character;
				}
			}
			return targetCharacter;
		}

		// Token: 0x06002E63 RID: 11875 RVA: 0x001365F4 File Offset: 0x001347F4
		private void RemoveCharacter(Character character)
		{
			StatusEffect.<>c__DisplayClass141_0 CS$<>8__locals1 = new StatusEffect.<>c__DisplayClass141_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.character = character;
			if (this.containerForItemsOnCharacterRemoval != Identifier.Empty)
			{
				ItemPrefab containerPrefab = ItemPrefab.Prefabs.Find((ItemPrefab me) => me.Tags.Contains(CS$<>8__locals1.<>4__this.containerForItemsOnCharacterRemoval)) ?? (MapEntityPrefab.FindByIdentifier(this.containerForItemsOnCharacterRemoval) as ItemPrefab);
				if (containerPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(104, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Could not spawn a container for a removed character's items. No item found with the identifier or tag \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.containerForItemsOnCharacterRemoval);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					EntitySpawner spawner = Entity.Spawner;
					if (spawner != null)
					{
						ItemPrefab itemPrefab = containerPrefab;
						Vector2 worldPosition = CS$<>8__locals1.character.WorldPosition;
						Action<Item> onSpawned = new Action<Item>(CS$<>8__locals1.<RemoveCharacter>g__OnItemContainerSpawned|1);
						spawner.AddItemToSpawnQueue(itemPrefab, worldPosition, null, null, onSpawned);
					}
				}
			}
			EntitySpawner spawner2 = Entity.Spawner;
			if (spawner2 == null)
			{
				return;
			}
			spawner2.AddEntityToRemoveQueue(CS$<>8__locals1.character);
		}

		// Token: 0x06002E64 RID: 11876 RVA: 0x001366EC File Offset: 0x001348EC
		private void SpawnItem(StatusEffect.ItemSpawnInfo chosenItemSpawnInfo, Entity entity, PhysicsBody sourceBody, Vector2 position, Entity targetEntity)
		{
			StatusEffect.<>c__DisplayClass142_0 CS$<>8__locals1 = new StatusEffect.<>c__DisplayClass142_0();
			CS$<>8__locals1.entity = entity;
			CS$<>8__locals1.sourceBody = sourceBody;
			CS$<>8__locals1.chosenItemSpawnInfo = chosenItemSpawnInfo;
			CS$<>8__locals1.position = position;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.parentItem = (CS$<>8__locals1.entity as Item);
			StatusEffect.<>c__DisplayClass142_0 CS$<>8__locals2 = CS$<>8__locals1;
			Item parentItem = CS$<>8__locals1.parentItem;
			CS$<>8__locals2.parentItemBody = ((parentItem != null) ? parentItem.body : null);
			if (this.user == null && CS$<>8__locals1.parentItem != null)
			{
				Projectile component = CS$<>8__locals1.parentItem.GetComponent<Projectile>();
				this.SetUser((component != null) ? component.User : null);
			}
			if (CS$<>8__locals1.chosenItemSpawnInfo.SpawnPosition == StatusEffect.ItemSpawnInfo.SpawnPositionType.Target && targetEntity != null)
			{
				CS$<>8__locals1.entity = targetEntity;
				CS$<>8__locals1.position = CS$<>8__locals1.entity.WorldPosition;
				Item it = CS$<>8__locals1.entity as Item;
				if (it != null && CS$<>8__locals1.sourceBody == null)
				{
					StatusEffect.<>c__DisplayClass142_0 CS$<>8__locals3 = CS$<>8__locals1;
					Item item5 = CS$<>8__locals1.entity as Item;
					PhysicsBody physicsBody;
					if ((physicsBody = ((item5 != null) ? item5.body : null)) == null)
					{
						Character character4 = CS$<>8__locals1.entity as Character;
						physicsBody = ((character4 != null) ? character4.AnimController.Collider : null);
					}
					CS$<>8__locals3.sourceBody = physicsBody;
				}
			}
			switch (CS$<>8__locals1.chosenItemSpawnInfo.SpawnPosition)
			{
			case StatusEffect.ItemSpawnInfo.SpawnPositionType.This:
			case StatusEffect.ItemSpawnInfo.SpawnPositionType.Target:
			{
				EntitySpawner spawner = Entity.Spawner;
				if (spawner == null)
				{
					return;
				}
				spawner.AddItemToSpawnQueue(CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab, CS$<>8__locals1.position + Rand.Vector(CS$<>8__locals1.chosenItemSpawnInfo.Spread, Rand.RandSync.Unsynced), null, null, delegate(Item newItem)
				{
					Projectile projectile = newItem.GetComponent<Projectile>();
					if (CS$<>8__locals1.entity != null)
					{
						Rope rope = newItem.GetComponent<Rope>();
						if (rope != null && CS$<>8__locals1.sourceBody != null)
						{
							Limb sourceLimb = CS$<>8__locals1.sourceBody.UserData as Limb;
							if (sourceLimb != null)
							{
								rope.Attach(sourceLimb, newItem);
								newItem.CreateServerEvent<Rope>(rope);
							}
						}
						float spread = Rand.Range(-CS$<>8__locals1.chosenItemSpawnInfo.AimSpreadRad, CS$<>8__locals1.chosenItemSpawnInfo.AimSpreadRad, Rand.RandSync.Unsynced);
						float rotation = CS$<>8__locals1.chosenItemSpawnInfo.RotationRad;
						Vector2 worldPos = CS$<>8__locals1.position;
						if (CS$<>8__locals1.sourceBody != null)
						{
							worldPos = CS$<>8__locals1.sourceBody.Position;
							Character character6 = CS$<>8__locals1.<>4__this.user;
							if (((character6 != null) ? character6.Submarine : null) != null)
							{
								worldPos += CS$<>8__locals1.<>4__this.user.Submarine.Position;
							}
						}
						else if (!CS$<>8__locals1.entity.Removed)
						{
							worldPos = CS$<>8__locals1.entity.WorldPosition;
						}
						switch (CS$<>8__locals1.chosenItemSpawnInfo.RotationType)
						{
						case StatusEffect.ItemSpawnInfo.SpawnRotationType.None:
							rotation = CS$<>8__locals1.chosenItemSpawnInfo.RotationRad;
							break;
						case StatusEffect.ItemSpawnInfo.SpawnRotationType.This:
							if (CS$<>8__locals1.sourceBody != null)
							{
								rotation = CS$<>8__locals1.sourceBody.TransformRotation(CS$<>8__locals1.chosenItemSpawnInfo.RotationRad);
							}
							else if (CS$<>8__locals1.parentItemBody != null)
							{
								rotation = CS$<>8__locals1.parentItemBody.TransformRotation(CS$<>8__locals1.chosenItemSpawnInfo.RotationRad);
							}
							else if (CS$<>8__locals1.parentItem != null)
							{
								rotation = PhysicsBody.TransformRotation(-CS$<>8__locals1.parentItem.RotationRad + CS$<>8__locals1.chosenItemSpawnInfo.RotationRad, CS$<>8__locals1.parentItem.FlippedX ? -1f : 1f);
							}
							break;
						case StatusEffect.ItemSpawnInfo.SpawnRotationType.Target:
							if (!CS$<>8__locals1.entity.Removed)
							{
								rotation = MathUtils.VectorToAngle(CS$<>8__locals1.entity.WorldPosition - worldPos);
							}
							break;
						case StatusEffect.ItemSpawnInfo.SpawnRotationType.Limb:
							if (CS$<>8__locals1.sourceBody != null)
							{
								rotation = CS$<>8__locals1.sourceBody.TransformedRotation;
							}
							break;
						case StatusEffect.ItemSpawnInfo.SpawnRotationType.MainLimb:
							if (CS$<>8__locals1.<>4__this.user != null)
							{
								rotation = CS$<>8__locals1.<>4__this.user.AnimController.MainLimb.body.TransformedRotation;
							}
							break;
						case StatusEffect.ItemSpawnInfo.SpawnRotationType.Collider:
							if (CS$<>8__locals1.parentItemBody != null)
							{
								rotation = CS$<>8__locals1.parentItemBody.TransformedRotation;
							}
							else if (CS$<>8__locals1.<>4__this.user != null)
							{
								rotation = CS$<>8__locals1.<>4__this.user.AnimController.Collider.Rotation + 1.5707964f;
							}
							break;
						case StatusEffect.ItemSpawnInfo.SpawnRotationType.Random:
							if (projectile != null)
							{
								DebugConsole.LogError("Random rotation is not supported for Projectiles.", null, null);
							}
							else
							{
								rotation = Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced);
							}
							break;
						default:
							throw new NotImplementedException("Item spawn rotation type not implemented: " + CS$<>8__locals1.chosenItemSpawnInfo.RotationType.ToString());
						}
						if (CS$<>8__locals1.<>4__this.user != null)
						{
							rotation += CS$<>8__locals1.chosenItemSpawnInfo.RotationRad * CS$<>8__locals1.<>4__this.user.AnimController.Dir;
						}
						rotation += spread;
						if (projectile != null)
						{
							PhysicsBody physicsBody2 = CS$<>8__locals1.sourceBody;
							ISpatialEntity sourceEntity = (((physicsBody2 != null) ? physicsBody2.UserData : null) as ISpatialEntity) ?? CS$<>8__locals1.entity;
							Vector2 spawnPos = sourceEntity.SimPosition;
							projectile.Item.Submarine = ((sourceEntity != null) ? sourceEntity.Submarine : null);
							List<Body> ignoredBodies = null;
							if (!projectile.DamageUser)
							{
								Character character7 = CS$<>8__locals1.<>4__this.user;
								List<Body> list;
								if (character7 == null)
								{
									list = null;
								}
								else
								{
									list = (from l in character7.AnimController.Limbs
									where !l.IsSevered
									select l.body.FarseerBody).ToList<Body>();
								}
								ignoredBodies = list;
							}
							float damageMultiplier = 1f;
							Character character5 = CS$<>8__locals1.entity as Character;
							if (character5 != null)
							{
								Limb limb = sourceEntity as Limb;
								if (limb != null)
								{
									Attack attack = limb.attack;
									if (attack != null)
									{
										attack.ResetDamageMultiplier();
										attack.DamageMultiplier *= 1f + character5.GetStatValue(StatTypes.NaturalRangedAttackMultiplier, true);
										damageMultiplier = attack.DamageMultiplier;
									}
								}
							}
							projectile.Shoot(CS$<>8__locals1.<>4__this.user, spawnPos, spawnPos, rotation, ignoredBodies, true, damageMultiplier, 0f);
							projectile.Item.Submarine = (projectile.LaunchSub = ((sourceEntity != null) ? sourceEntity.Submarine : null));
						}
						else if (newItem.body != null)
						{
							bool flip = (CS$<>8__locals1.parentItem != null && CS$<>8__locals1.parentItem.FlippedX) != (CS$<>8__locals1.parentItem != null && CS$<>8__locals1.parentItem.FlippedY);
							newItem.body.Dir = (float)(flip ? -1 : 1);
							newItem.body.SetTransform(newItem.SimPosition, flip ? (rotation - 3.1415927f) : rotation, true);
							Vector2 impulseDir = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
							newItem.body.ApplyLinearImpulse(impulseDir * CS$<>8__locals1.chosenItemSpawnInfo.Impulse);
						}
					}
					base.<SpawnItem>g__OnItemSpawned|0(newItem, CS$<>8__locals1.chosenItemSpawnInfo);
				});
				return;
			}
			case StatusEffect.ItemSpawnInfo.SpawnPositionType.ThisInventory:
			{
				Inventory inventory = null;
				Character character = CS$<>8__locals1.entity as Character;
				if (character != null && character.Inventory != null)
				{
					inventory = character.Inventory;
				}
				else
				{
					Item item6 = CS$<>8__locals1.entity as Item;
					if (item6 != null)
					{
						foreach (ItemContainer itemContainer in item6.GetComponents<ItemContainer>())
						{
							if (itemContainer.CanBeContained(CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab))
							{
								inventory = ((itemContainer != null) ? itemContainer.Inventory : null);
								break;
							}
						}
						if (!CS$<>8__locals1.chosenItemSpawnInfo.SpawnIfCantBeContained && inventory == null)
						{
							return;
						}
					}
				}
				if (inventory != null && (inventory.CanProbablyBePut(CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab, null, null) || CS$<>8__locals1.chosenItemSpawnInfo.SpawnIfInventoryFull))
				{
					EntitySpawner spawner2 = Entity.Spawner;
					ItemPrefab itemPrefab = CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab;
					Inventory inventory3 = inventory;
					bool spawnIfInventoryFull = CS$<>8__locals1.chosenItemSpawnInfo.SpawnIfInventoryFull;
					spawner2.AddItemToSpawnQueue(itemPrefab, inventory3, null, null, delegate(Item item)
					{
						base.<SpawnItem>g__OnItemSpawned|0(item, CS$<>8__locals1.chosenItemSpawnInfo);
					}, spawnIfInventoryFull, false, InvSlotType.None);
					return;
				}
				break;
			}
			case StatusEffect.ItemSpawnInfo.SpawnPositionType.SameInventory:
			{
				Inventory inventory2 = null;
				Character character2 = CS$<>8__locals1.entity as Character;
				if (character2 != null)
				{
					inventory2 = character2.Inventory;
				}
				else
				{
					Item item2 = CS$<>8__locals1.entity as Item;
					if (item2 != null)
					{
						inventory2 = item2.ParentInventory;
					}
				}
				if (inventory2 != null)
				{
					EntitySpawner spawner3 = Entity.Spawner;
					ItemPrefab itemPrefab2 = CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab;
					Inventory inventory4 = inventory2;
					bool spawnIfInventoryFull = CS$<>8__locals1.chosenItemSpawnInfo.SpawnIfInventoryFull;
					spawner3.AddItemToSpawnQueue(itemPrefab2, inventory4, null, null, delegate(Item newItem)
					{
						base.<SpawnItem>g__OnItemSpawned|0(newItem, CS$<>8__locals1.chosenItemSpawnInfo);
					}, spawnIfInventoryFull, false, InvSlotType.None);
					return;
				}
				if (CS$<>8__locals1.chosenItemSpawnInfo.SpawnIfNotInInventory)
				{
					Entity.Spawner.AddItemToSpawnQueue(CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab, CS$<>8__locals1.position, null, null, delegate(Item newItem)
					{
						base.<SpawnItem>g__OnItemSpawned|0(newItem, CS$<>8__locals1.chosenItemSpawnInfo);
					});
					return;
				}
				break;
			}
			case StatusEffect.ItemSpawnInfo.SpawnPositionType.ContainedInventory:
			{
				Inventory thisInventory = null;
				Character character3 = CS$<>8__locals1.entity as Character;
				if (character3 != null)
				{
					thisInventory = character3.Inventory;
				}
				else
				{
					Item item3 = CS$<>8__locals1.entity as Item;
					if (item3 != null)
					{
						ItemContainer itemContainer2 = item3.GetComponent<ItemContainer>();
						thisInventory = ((itemContainer2 != null) ? itemContainer2.Inventory : null);
						if (!CS$<>8__locals1.chosenItemSpawnInfo.SpawnIfCantBeContained && !itemContainer2.CanBeContained(CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab))
						{
							return;
						}
					}
				}
				if (thisInventory != null)
				{
					foreach (Item item4 in thisInventory.AllItems)
					{
						ItemContainer component2 = item4.GetComponent<ItemContainer>();
						Inventory containedInventory = (component2 != null) ? component2.Inventory : null;
						if (containedInventory != null && (containedInventory.CanProbablyBePut(CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab, null, null) || CS$<>8__locals1.chosenItemSpawnInfo.SpawnIfInventoryFull))
						{
							EntitySpawner spawner4 = Entity.Spawner;
							ItemPrefab itemPrefab3 = CS$<>8__locals1.chosenItemSpawnInfo.ItemPrefab;
							Inventory inventory5 = containedInventory;
							bool spawnIfInventoryFull = CS$<>8__locals1.chosenItemSpawnInfo.SpawnIfInventoryFull;
							float? condition = null;
							int? quality = null;
							Action<Item> onSpawned;
							if ((onSpawned = CS$<>8__locals1.<>9__7) == null)
							{
								onSpawned = (CS$<>8__locals1.<>9__7 = delegate(Item newItem)
								{
									base.<SpawnItem>g__OnItemSpawned|0(newItem, CS$<>8__locals1.chosenItemSpawnInfo);
								});
							}
							spawner4.AddItemToSpawnQueue(itemPrefab3, inventory5, condition, quality, onSpawned, spawnIfInventoryFull, false, InvSlotType.None);
							break;
						}
					}
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x06002E65 RID: 11877 RVA: 0x00136BE0 File Offset: 0x00134DE0
		private void TryTriggerAnimation(ISerializableEntity target, Entity entity)
		{
			if (this.animationsToTrigger == null)
			{
				return;
			}
			Character targetCharacter = StatusEffect.GetCharacterFromTarget(target) ?? (entity as Character);
			if (targetCharacter != null)
			{
				foreach (StatusEffect.AnimLoadInfo animLoadInfo in this.animationsToTrigger)
				{
					if ((this.failedAnimations == null || !this.failedAnimations.Contains(new ValueTuple<Character, StatusEffect.AnimLoadInfo>(targetCharacter, animLoadInfo))) && !targetCharacter.AnimController.TryLoadTemporaryAnimation(animLoadInfo, animLoadInfo.ExpectedSpeciesNames.Contains(targetCharacter.SpeciesName)))
					{
						if (this.failedAnimations == null)
						{
							this.failedAnimations = new HashSet<ValueTuple<Character, StatusEffect.AnimLoadInfo>>();
						}
						this.failedAnimations.Add(new ValueTuple<Character, StatusEffect.AnimLoadInfo>(targetCharacter, animLoadInfo));
					}
				}
			}
		}

		// Token: 0x06002E66 RID: 11878 RVA: 0x00136CB4 File Offset: 0x00134EB4
		private void ApplyToProperty(ISerializableEntity target, SerializableProperty property, object value, float deltaTime)
		{
			bool flag = value is int || value is float;
			if (flag)
			{
				float newValue = this.GetModifiedValue(property.GetFloatValue(target), Convert.ToSingle(value), deltaTime);
				if (property.PropertyType == typeof(float))
				{
					property.TrySetValue(target, newValue);
					return;
				}
				if (property.PropertyType == typeof(int))
				{
					property.TrySetValue(target, (int)newValue);
					return;
				}
			}
			else if (value is bool)
			{
				bool propertyValueBool = (bool)value;
				property.TrySetValue(target, propertyValueBool);
				return;
			}
			property.TrySetValue(target, value);
		}

		// Token: 0x06002E67 RID: 11879 RVA: 0x00136D55 File Offset: 0x00134F55
		private float GetModifiedValue(float currValue, float newValue, float deltaTime)
		{
			if (this.setValue)
			{
				return newValue;
			}
			if (this.disableDeltaTime)
			{
				deltaTime = 1f;
			}
			return currValue + newValue * deltaTime;
		}

		// Token: 0x06002E68 RID: 11880 RVA: 0x00136D78 File Offset: 0x00134F78
		public static void UpdateAll(float deltaTime)
		{
			DelayedEffect.Update(deltaTime);
			for (int i = StatusEffect.DurationList.Count - 1; i >= 0; i--)
			{
				DurationListElement element = StatusEffect.DurationList[i];
				if (element.Parent.CheckConditionalAlways && !element.Parent.HasRequiredConditions(element.Targets))
				{
					StatusEffect.DurationList.RemoveAt(i);
				}
				else
				{
					element.Targets.RemoveAll(delegate(ISerializableEntity t)
					{
						Entity entity = t as Entity;
						if (entity == null || !entity.Removed)
						{
							Limb limb3 = t as Limb;
							return limb3 != null && (limb3.character == null || limb3.character.Removed);
						}
						return true;
					});
					if (element.Targets.Count == 0)
					{
						StatusEffect.DurationList.RemoveAt(i);
					}
					else
					{
						foreach (ISerializableEntity target in element.Targets)
						{
							if (((target != null) ? target.SerializableProperties : null) != null)
							{
								foreach (ValueTuple<Identifier, object> valueTuple in element.Parent.PropertyEffects)
								{
									Identifier propertyName = valueTuple.Item1;
									object value = valueTuple.Item2;
									SerializableProperty property;
									if (target.SerializableProperties.TryGetValue(propertyName, out property))
									{
										element.Parent.ApplyToProperty(target, property, value, CoroutineManager.DeltaTime);
									}
								}
							}
							foreach (Affliction affliction in element.Parent.Afflictions)
							{
								Character character = target as Character;
								if (character != null)
								{
									if (!character.Removed)
									{
										Affliction newAffliction = element.Parent.GetMultipliedAffliction(affliction, element.Entity, character, deltaTime, element.Parent.multiplyAfflictionsByMaxVitality);
										Character character3 = character;
										Vector2 worldPosition = character.WorldPosition;
										IEnumerable<Affliction> afflictions = newAffliction.ToEnumerable<Affliction>();
										float stun = 0f;
										bool playSound = false;
										Character attacker = element.User;
										AttackResult result = character3.AddDamage(worldPosition, afflictions, stun, playSound, null, attacker, 1f);
										element.Parent.RegisterTreatmentResults(element.Parent.user, element.Entity as Item, result.HitLimb, affliction, result);
									}
								}
								else
								{
									Limb limb = target as Limb;
									if (limb != null && !limb.character.Removed && !limb.Removed)
									{
										Affliction newAffliction = element.Parent.GetMultipliedAffliction(affliction, element.Entity, limb.character, deltaTime, element.Parent.multiplyAfflictionsByMaxVitality);
										AttackResult result2 = limb.character.DamageLimb(limb.WorldPosition, limb, newAffliction.ToEnumerable<Affliction>(), 0f, false, Vector2.Zero, element.User, 1f, true, 0f, false, false, true);
										element.Parent.RegisterTreatmentResults(element.Parent.user, element.Entity as Item, limb, affliction, result2);
									}
								}
							}
							foreach (ValueTuple<Identifier, float> valueTuple2 in element.Parent.ReduceAffliction)
							{
								Identifier affliction2 = valueTuple2.Item1;
								float amount = valueTuple2.Item2;
								Limb targetLimb = null;
								Character targetCharacter = null;
								Character character2 = target as Character;
								if (character2 != null)
								{
									targetCharacter = character2;
								}
								else
								{
									Limb limb2 = target as Limb;
									if (limb2 != null)
									{
										targetLimb = limb2;
										targetCharacter = limb2.character;
									}
								}
								if (targetCharacter != null && !targetCharacter.Removed)
								{
									ActionType? actionType = null;
									Item item = element.Entity as Item;
									if (item != null && item.UseInHealthInterface)
									{
										actionType = new ActionType?(element.Parent.type);
									}
									float reduceAmount = amount * element.Parent.GetAfflictionMultiplier(element.Entity, targetCharacter, deltaTime);
									float prevVitality = targetCharacter.Vitality;
									if (targetLimb != null)
									{
										targetCharacter.CharacterHealth.ReduceAfflictionOnLimb(targetLimb, affliction2, reduceAmount, actionType, element.User);
									}
									else
									{
										targetCharacter.CharacterHealth.ReduceAfflictionOnAllLimbs(affliction2, reduceAmount, actionType, element.User);
									}
									if (!targetCharacter.IsDead)
									{
										float healthChange = targetCharacter.Vitality - prevVitality;
										AIController aicontroller = targetCharacter.AIController;
										if (aicontroller != null)
										{
											aicontroller.OnHealed(element.User, healthChange);
										}
										if (element.User != null)
										{
											if (element.Parent.CanGiveMedicalSkill)
											{
												targetCharacter.TryAdjustHealerSkill(element.User, healthChange, null);
											}
											GameMain.Server.KarmaManager.OnCharacterHealthChanged(targetCharacter, element.User, -healthChange, 0f, null);
										}
									}
								}
							}
							element.Parent.TryTriggerAnimation(target, element.Entity);
						}
						element.Timer -= deltaTime;
						if (element.Timer <= 0f)
						{
							StatusEffect.DurationList.Remove(element);
						}
					}
				}
			}
		}

		// Token: 0x06002E69 RID: 11881 RVA: 0x0013727C File Offset: 0x0013547C
		private float GetAfflictionMultiplier(Entity entity, Character targetCharacter, float deltaTime)
		{
			float afflictionMultiplier = (!this.setValue && !this.disableDeltaTime) ? deltaTime : 1f;
			Item sourceItem = entity as Item;
			if (sourceItem != null)
			{
				if (sourceItem.HasTag(Barotrauma.Tags.MedicalItem))
				{
					afflictionMultiplier *= 1f + targetCharacter.GetStatValue(StatTypes.MedicalItemEffectivenessMultiplier, true);
					if (this.user != null)
					{
						afflictionMultiplier *= 1f + this.user.GetStatValue(StatTypes.MedicalItemApplyingMultiplier, true);
					}
				}
				else if (sourceItem.HasTag(AfflictionPrefab.PoisonType) && this.user != null)
				{
					afflictionMultiplier *= 1f + this.user.GetStatValue(StatTypes.PoisonMultiplier, true);
				}
			}
			return afflictionMultiplier;
		}

		// Token: 0x06002E6A RID: 11882 RVA: 0x0013731C File Offset: 0x0013551C
		private Affliction GetMultipliedAffliction(Affliction affliction, Entity entity, Character targetCharacter, float deltaTime, bool multiplyByMaxVitality)
		{
			float afflictionMultiplier = this.GetAfflictionMultiplier(entity, targetCharacter, deltaTime);
			if (affliction.AffectedByAttackMultipliers)
			{
				afflictionMultiplier *= this.AttackMultiplier;
			}
			if (multiplyByMaxVitality)
			{
				afflictionMultiplier *= targetCharacter.MaxVitality / 100f;
			}
			if (this.user != null)
			{
				if (affliction.Prefab.IsBuff)
				{
					afflictionMultiplier *= 1f + this.user.GetStatValue(StatTypes.BuffItemApplyingMultiplier, true);
				}
				else if (affliction.Prefab.Identifier == "organdamage")
				{
					if (targetCharacter.CharacterHealth.GetActiveAfflictionTags().Any((Identifier t) => t == "poisoned"))
					{
						afflictionMultiplier *= 1f + this.user.GetStatValue(StatTypes.PoisonMultiplier, true);
					}
				}
			}
			if (affliction.DivideByLimbCount)
			{
				int limbCount = targetCharacter.AnimController.Limbs.Count((Limb limb) => this.IsValidTargetLimb(limb));
				if (limbCount > 0)
				{
					afflictionMultiplier *= 1f / (float)limbCount;
				}
			}
			if (!MathUtils.NearlyEqual(afflictionMultiplier, 1f, 0.0001f))
			{
				return affliction.CreateMultiplied(afflictionMultiplier, affliction);
			}
			return affliction;
		}

		// Token: 0x06002E6B RID: 11883 RVA: 0x00137438 File Offset: 0x00135638
		private void RegisterTreatmentResults(Character user, Item item, Limb limb, Affliction affliction, AttackResult result)
		{
			if (item == null)
			{
				return;
			}
			if (!item.UseInHealthInterface)
			{
				return;
			}
			if (limb == null)
			{
				return;
			}
			using (IEnumerator<Affliction> enumerator = limb.character.CharacterHealth.GetAllAfflictions().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Affliction limbAffliction = enumerator.Current;
					if (result.Afflictions != null)
					{
						Affliction resultAffliction = result.Afflictions.FirstOrDefault((Affliction a) => a.Prefab == limbAffliction.Prefab);
						if (resultAffliction != null && (!affliction.Prefab.LimbSpecific || limb.character.CharacterHealth.GetAfflictionLimb(affliction) == limb))
						{
							if (this.type == ActionType.OnUse || this.type == ActionType.OnSuccess)
							{
								limbAffliction.AppliedAsSuccessfulTreatmentTime = Timing.TotalTime;
								if (this.CanGiveMedicalSkill)
								{
									limb.character.TryAdjustHealerSkill(user, 0f, resultAffliction);
								}
							}
							else if (this.type == ActionType.OnFailure)
							{
								limbAffliction.AppliedAsFailedTreatmentTime = Timing.TotalTime;
								if (this.CanGiveMedicalSkill)
								{
									limb.character.TryAdjustHealerSkill(user, 0f, resultAffliction);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06002E6C RID: 11884 RVA: 0x0013756C File Offset: 0x0013576C
		public static void StopAll()
		{
			CoroutineManager.StopCoroutines("statuseffect");
			DelayedEffect.DelayList.Clear();
			StatusEffect.DurationList.Clear();
		}

		// Token: 0x06002E6D RID: 11885 RVA: 0x0013758C File Offset: 0x0013578C
		public void AddTag(Identifier tag)
		{
			if (this.statusEffectTags.Contains(tag))
			{
				return;
			}
			this.statusEffectTags.Add(tag);
		}

		// Token: 0x06002E6E RID: 11886 RVA: 0x001375AA File Offset: 0x001357AA
		public bool HasTag(Identifier tag)
		{
			return tag == null || this.statusEffectTags.Contains(tag);
		}

		// Token: 0x06002E71 RID: 11889 RVA: 0x0013762C File Offset: 0x0013582C
		[CompilerGenerated]
		private bool <AddNearbyTargets>g__CheckDistance|120_0(ISpatialEntity e, ref StatusEffect.<>c__DisplayClass120_0 A_2)
		{
			float xDiff = Math.Abs(e.WorldPosition.X - A_2.worldPosition.X);
			if (xDiff > this.Range)
			{
				return false;
			}
			float yDiff = Math.Abs(e.WorldPosition.Y - A_2.worldPosition.Y);
			return yDiff <= this.Range && xDiff * xDiff + yDiff * yDiff < this.Range * this.Range;
		}

		// Token: 0x06002E72 RID: 11890 RVA: 0x001376A4 File Offset: 0x001358A4
		[CompilerGenerated]
		internal static bool <HasRequiredConditions>g__AnyTargetMatches|125_1(IReadOnlyList<ISerializableEntity> targets, string targetItemComponentName, PropertyConditional conditional)
		{
			int i = 0;
			while (i < targets.Count)
			{
				if (string.IsNullOrEmpty(targetItemComponentName))
				{
					goto IL_2A;
				}
				ItemComponent ic = targets[i] as ItemComponent;
				if (ic != null && !(ic.Name != targetItemComponentName))
				{
					goto IL_2A;
				}
				IL_3B:
				i++;
				continue;
				IL_2A:
				if (conditional.Matches(targets[i]))
				{
					return true;
				}
				goto IL_3B;
			}
			return false;
		}

		// Token: 0x06002E73 RID: 11891 RVA: 0x001376FC File Offset: 0x001358FC
		[CompilerGenerated]
		internal static ISerializableEntity <HasRequiredConditions>g__FindTargetItemOrComponent|125_2(IReadOnlyList<ISerializableEntity> targets)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				if (targets[i] is Item || targets[i] is ItemComponent)
				{
					return targets[i];
				}
			}
			return null;
		}

		// Token: 0x06002E75 RID: 11893 RVA: 0x00137760 File Offset: 0x00135960
		[CompilerGenerated]
		internal static Identifier <Apply>g__GetRandomSkill|138_1(ref StatusEffect.<>c__DisplayClass138_1 A_0)
		{
			CharacterInfo info = A_0.targetCharacter.Info;
			Identifier? identifier;
			if (info == null)
			{
				identifier = null;
			}
			else
			{
				Job job = info.Job;
				if (job == null)
				{
					identifier = null;
				}
				else
				{
					Skill randomUnsynced = job.GetSkills().GetRandomUnsynced<Skill>();
					identifier = ((randomUnsynced != null) ? new Identifier?(randomUnsynced.Identifier) : null);
				}
			}
			Identifier? identifier2 = identifier;
			if (identifier2 == null)
			{
				return Identifier.Empty;
			}
			return identifier2.GetValueOrDefault();
		}

		// Token: 0x06002E76 RID: 11894 RVA: 0x001377D8 File Offset: 0x001359D8
		[CompilerGenerated]
		internal static CharacterTeamType? <Apply>g__GetTeamFromSubmarine|138_7(MapEntity e, ref StatusEffect.<>c__DisplayClass138_6 A_1)
		{
			if (e.Submarine == null)
			{
				return null;
			}
			return new CharacterTeamType?((!A_1.isPvP && e.Submarine.Info.IsOutpost && e.Submarine.TeamID == CharacterTeamType.FriendlyNPC) ? CharacterTeamType.Team1 : e.Submarine.TeamID);
		}

		// Token: 0x040016A5 RID: 5797
		private static readonly ImmutableHashSet<Identifier> FieldNames;

		// Token: 0x040016A6 RID: 5798
		private readonly StatusEffect.TargetType targetTypes;

		// Token: 0x040016A7 RID: 5799
		public int TargetSlot = -1;

		// Token: 0x040016A8 RID: 5800
		private readonly List<RelatedItem> requiredItems;

		// Token: 0x040016A9 RID: 5801
		[TupleElementNames(new string[]
		{
			"propertyName",
			"value"
		})]
		public readonly ImmutableArray<ValueTuple<Identifier, object>> PropertyEffects;

		// Token: 0x040016AA RID: 5802
		private readonly PropertyConditional.LogicalOperatorType conditionalLogicalOperator = PropertyConditional.LogicalOperatorType.Or;

		// Token: 0x040016AB RID: 5803
		private readonly List<PropertyConditional> propertyConditionals;

		// Token: 0x040016AC RID: 5804
		private readonly bool setValue;

		// Token: 0x040016AD RID: 5805
		private readonly bool disableDeltaTime;

		// Token: 0x040016AE RID: 5806
		private readonly HashSet<Identifier> statusEffectTags;

		// Token: 0x040016AF RID: 5807
		private readonly float lifeTime;

		// Token: 0x040016B0 RID: 5808
		private float lifeTimer;

		// Token: 0x040016B1 RID: 5809
		private Dictionary<Entity, float> intervalTimers;

		// Token: 0x040016B2 RID: 5810
		private readonly bool oneShot;

		// Token: 0x040016B3 RID: 5811
		public static readonly List<DurationListElement> DurationList = new List<DurationListElement>();

		// Token: 0x040016B4 RID: 5812
		public readonly bool CheckConditionalAlways;

		// Token: 0x040016B5 RID: 5813
		public readonly bool Stackable;

		// Token: 0x040016B6 RID: 5814
		public readonly bool ResetDurationWhenReapplied;

		// Token: 0x040016B7 RID: 5815
		public readonly float Interval;

		// Token: 0x040016B8 RID: 5816
		private readonly int useItemCount;

		// Token: 0x040016B9 RID: 5817
		private readonly bool removeItem;

		// Token: 0x040016BA RID: 5818
		private readonly bool dropContainedItems;

		// Token: 0x040016BB RID: 5819
		private readonly bool dropItem;

		// Token: 0x040016BC RID: 5820
		private readonly bool removeCharacter;

		// Token: 0x040016BD RID: 5821
		private readonly bool breakLimb;

		// Token: 0x040016BE RID: 5822
		private readonly bool hideLimb;

		// Token: 0x040016BF RID: 5823
		private readonly float hideLimbTimer;

		// Token: 0x040016C0 RID: 5824
		private readonly Identifier containerForItemsOnCharacterRemoval;

		// Token: 0x040016C1 RID: 5825
		public readonly ActionType type = ActionType.OnActive;

		// Token: 0x040016C2 RID: 5826
		private readonly List<Explosion> explosions;

		// Token: 0x040016C3 RID: 5827
		private readonly List<StatusEffect.ItemSpawnInfo> spawnItems;

		// Token: 0x040016C4 RID: 5828
		private readonly Identifier forceSayIdentifier = Identifier.Empty;

		// Token: 0x040016C5 RID: 5829
		private readonly bool forceSayInRadio;

		// Token: 0x040016C6 RID: 5830
		private readonly bool spawnItemRandomly;

		// Token: 0x040016C7 RID: 5831
		private readonly List<StatusEffect.CharacterSpawnInfo> spawnCharacters;

		// Token: 0x040016C8 RID: 5832
		public readonly bool refundTalents;

		// Token: 0x040016C9 RID: 5833
		public readonly List<StatusEffect.GiveTalentInfo> giveTalentInfos;

		// Token: 0x040016CA RID: 5834
		private readonly List<StatusEffect.AITrigger> aiTriggers;

		// Token: 0x040016CB RID: 5835
		private readonly List<EventPrefab> triggeredEvents;

		// Token: 0x040016CC RID: 5836
		private readonly Identifier triggeredEventTargetTag = "statuseffecttarget".ToIdentifier();

		// Token: 0x040016CD RID: 5837
		private readonly Identifier triggeredEventEntityTag = "statuseffectentity".ToIdentifier();

		// Token: 0x040016CE RID: 5838
		private readonly Identifier triggeredEventUserTag = "statuseffectuser".ToIdentifier();

		// Token: 0x040016CF RID: 5839
		[TupleElementNames(new string[]
		{
			"eventIdentifier",
			"tag"
		})]
		private readonly List<ValueTuple<Identifier, Identifier>> eventTargetTags;

		// Token: 0x040016D0 RID: 5840
		public readonly ImmutableHashSet<Identifier> UnlockRecipes;

		// Token: 0x040016D1 RID: 5841
		private Character user;

		// Token: 0x040016D2 RID: 5842
		public readonly float FireSize;

		// Token: 0x040016D3 RID: 5843
		public readonly LimbType[] targetLimbs;

		// Token: 0x040016D4 RID: 5844
		public readonly float SeverLimbsProbability;

		// Token: 0x040016D5 RID: 5845
		private readonly Vector2 randomCondition;

		// Token: 0x040016D6 RID: 5846
		public PhysicsBody sourceBody;

		// Token: 0x040016D7 RID: 5847
		public readonly bool OnlyInside;

		// Token: 0x040016D8 RID: 5848
		public readonly bool OnlyOutside;

		// Token: 0x040016D9 RID: 5849
		public readonly bool OnlyWhenDamagedByPlayer;

		// Token: 0x040016DA RID: 5850
		public readonly bool AllowWhenBroken;

		// Token: 0x040016DB RID: 5851
		public readonly ImmutableHashSet<Identifier> TargetIdentifiers;

		// Token: 0x040016DC RID: 5852
		public readonly string TargetItemComponent;

		// Token: 0x040016DD RID: 5853
		[TupleElementNames(new string[]
		{
			"affliction",
			"strength"
		})]
		private readonly HashSet<ValueTuple<Identifier, float>> requiredAfflictions;

		// Token: 0x040016DE RID: 5854
		public float AttackMultiplier = 1f;

		// Token: 0x040016E0 RID: 5856
		private readonly bool multiplyAfflictionsByMaxVitality;

		// Token: 0x040016E1 RID: 5857
		[TupleElementNames(new string[]
		{
			"AfflictionIdentifier",
			"ReduceAmount"
		})]
		public readonly List<ValueTuple<Identifier, float>> ReduceAffliction = new List<ValueTuple<Identifier, float>>();

		// Token: 0x040016E2 RID: 5858
		public readonly bool CanGiveMedicalSkill;

		// Token: 0x040016E3 RID: 5859
		private readonly List<Identifier> talentTriggers;

		// Token: 0x040016E4 RID: 5860
		private readonly List<int> giveExperiences;

		// Token: 0x040016E5 RID: 5861
		private readonly List<StatusEffect.GiveSkill> giveSkills;

		// Token: 0x040016E6 RID: 5862
		private readonly List<ValueTuple<string, ContentXElement>> luaHook;

		// Token: 0x040016E7 RID: 5863
		[TupleElementNames(new string[]
		{
			"targetCharacter",
			"anim"
		})]
		private HashSet<ValueTuple<Character, StatusEffect.AnimLoadInfo>> failedAnimations;

		// Token: 0x040016E8 RID: 5864
		private readonly List<StatusEffect.AnimLoadInfo> animationsToTrigger;

		// Token: 0x040016E9 RID: 5865
		public readonly float Duration;

		// Token: 0x040016EF RID: 5871
		private static readonly List<Entity> intervalsToRemove = new List<Entity>();

		// Token: 0x040016F0 RID: 5872
		protected readonly List<ISerializableEntity> currentTargets = new List<ISerializableEntity>();

		// Token: 0x02000B08 RID: 2824
		[Flags]
		public enum TargetType
		{
			// Token: 0x04003839 RID: 14393
			This = 1,
			// Token: 0x0400383A RID: 14394
			Parent = 2,
			// Token: 0x0400383B RID: 14395
			Character = 4,
			// Token: 0x0400383C RID: 14396
			Contained = 8,
			// Token: 0x0400383D RID: 14397
			NearbyCharacters = 16,
			// Token: 0x0400383E RID: 14398
			NearbyItems = 32,
			// Token: 0x0400383F RID: 14399
			UseTarget = 64,
			// Token: 0x04003840 RID: 14400
			Hull = 128,
			// Token: 0x04003841 RID: 14401
			Limb = 256,
			// Token: 0x04003842 RID: 14402
			AllLimbs = 512,
			// Token: 0x04003843 RID: 14403
			LastLimb = 1024,
			// Token: 0x04003844 RID: 14404
			LinkedEntities = 2048
		}

		// Token: 0x02000B09 RID: 2825
		private class ItemSpawnInfo
		{
			// Token: 0x170015B6 RID: 5558
			// (get) Token: 0x06005F51 RID: 24401 RVA: 0x0020718B File Offset: 0x0020538B
			// (set) Token: 0x06005F52 RID: 24402 RVA: 0x00207193 File Offset: 0x00205393
			public bool InheritEventTags { get; private set; }

			// Token: 0x06005F53 RID: 24403 RVA: 0x0020719C File Offset: 0x0020539C
			public ItemSpawnInfo(ContentXElement element, string parentDebugName)
			{
				if (element.GetAttribute("name") != null)
				{
					DebugConsole.ThrowError("Error in StatusEffect config (" + element.ToString() + ") - use item identifier instead of the name.", null, element.ContentPackage, false, false);
					string itemPrefabName = element.GetAttributeString("name", "");
					this.ItemPrefab = ItemPrefab.Prefabs.Find((ItemPrefab m) => m.NameMatches(itemPrefabName, StringComparison.InvariantCultureIgnoreCase) || m.Tags.Contains(itemPrefabName));
					if (this.ItemPrefab == null)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error in StatusEffect \"",
							parentDebugName,
							"\" - item prefab \"",
							itemPrefabName,
							"\" not found."
						}), null, element.ContentPackage, false, false);
					}
				}
				else
				{
					string itemPrefabIdentifier = element.GetAttributeString("identifier", "");
					if (string.IsNullOrEmpty(itemPrefabIdentifier))
					{
						itemPrefabIdentifier = element.GetAttributeString("identifiers", "");
					}
					if (string.IsNullOrEmpty(itemPrefabIdentifier))
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Invalid item spawn in StatusEffect \"",
							parentDebugName,
							"\" - identifier not found in the element \"",
							element.ToString(),
							"\"."
						}), null, element.ContentPackage, false, false);
					}
					this.ItemPrefab = ItemPrefab.Prefabs.Find((ItemPrefab m) => m.Identifier == itemPrefabIdentifier);
					if (this.ItemPrefab == null)
					{
						DebugConsole.ThrowError("Error in StatusEffect config - item prefab with the identifier \"" + itemPrefabIdentifier + "\" not found.", null, element.ContentPackage, false, false);
						return;
					}
				}
				this.SpawnIfInventoryFull = element.GetAttributeBool("SpawnIfInventoryFull", false);
				this.SpawnIfNotInInventory = element.GetAttributeBool("SpawnIfNotInInventory", false);
				this.SpawnIfCantBeContained = element.GetAttributeBool("SpawnIfCantBeContained", true);
				this.Impulse = element.GetAttributeFloat("impulse", element.GetAttributeFloat("launchimpulse", element.GetAttributeFloat("speed", 0f)));
				this.Condition = MathHelper.Clamp(element.GetAttributeFloat("condition", 1f), 0f, 1f);
				this.RotationRad = MathHelper.ToRadians(element.GetAttributeFloat("rotation", 0f));
				int fixedCount = element.GetAttributeInt("count", 1);
				this.MinCount = element.GetAttributeInt("MinCount", fixedCount);
				this.MaxCount = element.GetAttributeInt("MaxCount", fixedCount);
				if (this.MinCount > this.MaxCount)
				{
					DebugConsole.AddWarning("Potential error in a StatusEffect " + parentDebugName + ": mincount is larger than maxcount.", null);
				}
				this.Probability = element.GetAttributeFloat("Probability", 1f);
				this.Spread = element.GetAttributeFloat("spread", 0f);
				this.AimSpreadRad = MathHelper.ToRadians(element.GetAttributeFloat("aimspread", 0f));
				this.Equip = element.GetAttributeBool("equip", false);
				string key = "spawnposition";
				StatusEffect.ItemSpawnInfo.SpawnPositionType spawnPositionType = StatusEffect.ItemSpawnInfo.SpawnPositionType.This;
				this.SpawnPosition = element.GetAttributeEnum<StatusEffect.ItemSpawnInfo.SpawnPositionType>(key, spawnPositionType);
				if (element.GetAttributeString("rotationtype", string.Empty).Equals("Fixed", StringComparison.OrdinalIgnoreCase))
				{
					this.RotationType = StatusEffect.ItemSpawnInfo.SpawnRotationType.This;
				}
				else
				{
					string key2 = "rotationtype";
					StatusEffect.ItemSpawnInfo.SpawnRotationType spawnRotationType = (this.RotationRad != 0f) ? StatusEffect.ItemSpawnInfo.SpawnRotationType.This : StatusEffect.ItemSpawnInfo.SpawnRotationType.Target;
					this.RotationType = element.GetAttributeEnum<StatusEffect.ItemSpawnInfo.SpawnRotationType>(key2, spawnRotationType);
				}
				this.InheritEventTags = element.GetAttributeBool("InheritEventTags", false);
			}

			// Token: 0x06005F54 RID: 24404 RVA: 0x002074FD File Offset: 0x002056FD
			public int GetCount(Rand.RandSync randSync)
			{
				return Rand.Range(this.MinCount, this.MaxCount + 1, randSync);
			}

			// Token: 0x04003845 RID: 14405
			public readonly ItemPrefab ItemPrefab;

			// Token: 0x04003846 RID: 14406
			public readonly StatusEffect.ItemSpawnInfo.SpawnPositionType SpawnPosition;

			// Token: 0x04003847 RID: 14407
			public readonly bool SpawnIfInventoryFull;

			// Token: 0x04003848 RID: 14408
			public readonly bool SpawnIfNotInInventory;

			// Token: 0x04003849 RID: 14409
			public readonly bool SpawnIfCantBeContained;

			// Token: 0x0400384A RID: 14410
			public readonly float Impulse;

			// Token: 0x0400384B RID: 14411
			public readonly float RotationRad;

			// Token: 0x0400384C RID: 14412
			public readonly int MinCount;

			// Token: 0x0400384D RID: 14413
			public readonly int MaxCount;

			// Token: 0x0400384E RID: 14414
			public readonly float Probability;

			// Token: 0x0400384F RID: 14415
			public readonly float Spread;

			// Token: 0x04003850 RID: 14416
			public readonly StatusEffect.ItemSpawnInfo.SpawnRotationType RotationType;

			// Token: 0x04003851 RID: 14417
			public readonly float AimSpreadRad;

			// Token: 0x04003852 RID: 14418
			public readonly bool Equip;

			// Token: 0x04003853 RID: 14419
			public readonly float Condition;

			// Token: 0x02000EC5 RID: 3781
			public enum SpawnPositionType
			{
				// Token: 0x04004362 RID: 17250
				This,
				// Token: 0x04004363 RID: 17251
				ThisInventory,
				// Token: 0x04004364 RID: 17252
				SameInventory,
				// Token: 0x04004365 RID: 17253
				ContainedInventory,
				// Token: 0x04004366 RID: 17254
				Target
			}

			// Token: 0x02000EC6 RID: 3782
			public enum SpawnRotationType
			{
				// Token: 0x04004368 RID: 17256
				None,
				// Token: 0x04004369 RID: 17257
				This,
				// Token: 0x0400436A RID: 17258
				Target,
				// Token: 0x0400436B RID: 17259
				Limb,
				// Token: 0x0400436C RID: 17260
				MainLimb,
				// Token: 0x0400436D RID: 17261
				Collider,
				// Token: 0x0400436E RID: 17262
				Random
			}
		}

		// Token: 0x02000B0A RID: 2826
		public class AbilityStatusEffectIdentifier : AbilityObject
		{
			// Token: 0x06005F55 RID: 24405 RVA: 0x00207513 File Offset: 0x00205713
			public AbilityStatusEffectIdentifier(Identifier effectIdentifier)
			{
				this.EffectIdentifier = effectIdentifier;
			}

			// Token: 0x170015B7 RID: 5559
			// (get) Token: 0x06005F56 RID: 24406 RVA: 0x00207522 File Offset: 0x00205722
			// (set) Token: 0x06005F57 RID: 24407 RVA: 0x0020752A File Offset: 0x0020572A
			public Identifier EffectIdentifier { get; set; }
		}

		// Token: 0x02000B0B RID: 2827
		public class GiveTalentInfo
		{
			// Token: 0x06005F58 RID: 24408 RVA: 0x00207533 File Offset: 0x00205733
			public GiveTalentInfo(XElement element, string _)
			{
				this.TalentIdentifiers = element.GetAttributeIdentifierArray("talentidentifiers", Array.Empty<Identifier>(), true);
				this.GiveRandom = element.GetAttributeBool("giverandom", false);
			}

			// Token: 0x04003856 RID: 14422
			public Identifier[] TalentIdentifiers;

			// Token: 0x04003857 RID: 14423
			public bool GiveRandom;
		}

		// Token: 0x02000B0C RID: 2828
		public class GiveSkill
		{
			// Token: 0x06005F59 RID: 24409 RVA: 0x00207564 File Offset: 0x00205764
			public GiveSkill(ContentXElement element, string parentDebugName)
			{
				this.SkillIdentifier = element.GetAttributeIdentifier("SkillIdentifier", Identifier.Empty);
				this.Amount = element.GetAttributeFloat("Amount", 0f);
				this.TriggerTalents = element.GetAttributeBool("TriggerTalents", true);
				this.UseDeltaTime = element.GetAttributeBool("UseDeltaTime", false);
				this.Proportional = element.GetAttributeBool("Proportional", false);
				this.AlwayShowNotification = element.GetAttributeBool("AlwayShowNotification", false);
				if (this.SkillIdentifier == Identifier.Empty)
				{
					DebugConsole.ThrowError("GiveSkill StatusEffect did not have a skill identifier defined in " + parentDebugName + "!", null, element.ContentPackage, false, false);
				}
			}

			// Token: 0x04003858 RID: 14424
			public readonly Identifier SkillIdentifier;

			// Token: 0x04003859 RID: 14425
			public readonly float Amount;

			// Token: 0x0400385A RID: 14426
			public readonly bool TriggerTalents;

			// Token: 0x0400385B RID: 14427
			public readonly bool UseDeltaTime;

			// Token: 0x0400385C RID: 14428
			public readonly bool Proportional;

			// Token: 0x0400385D RID: 14429
			public readonly bool AlwayShowNotification;
		}

		// Token: 0x02000B0D RID: 2829
		public class CharacterSpawnInfo : ISerializableEntity
		{
			// Token: 0x170015B8 RID: 5560
			// (get) Token: 0x06005F5A RID: 24410 RVA: 0x0020761C File Offset: 0x0020581C
			public string Name
			{
				get
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Character Spawn Info (");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.SpeciesName);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
			}

			// Token: 0x170015B9 RID: 5561
			// (get) Token: 0x06005F5B RID: 24411 RVA: 0x0020765F File Offset: 0x0020585F
			// (set) Token: 0x06005F5C RID: 24412 RVA: 0x00207667 File Offset: 0x00205867
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

			// Token: 0x170015BA RID: 5562
			// (get) Token: 0x06005F5D RID: 24413 RVA: 0x00207670 File Offset: 0x00205870
			// (set) Token: 0x06005F5E RID: 24414 RVA: 0x00207678 File Offset: 0x00205878
			[Serialize("", IsPropertySaveable.No, "The species name (identifier) of the character to spawn.", "", false)]
			public Identifier SpeciesName { get; private set; }

			// Token: 0x170015BB RID: 5563
			// (get) Token: 0x06005F5F RID: 24415 RVA: 0x00207681 File Offset: 0x00205881
			// (set) Token: 0x06005F60 RID: 24416 RVA: 0x00207689 File Offset: 0x00205889
			[Serialize(1, IsPropertySaveable.No, "How many characters to spawn.", "", false)]
			public int Count { get; private set; }

			// Token: 0x170015BC RID: 5564
			// (get) Token: 0x06005F61 RID: 24417 RVA: 0x00207692 File Offset: 0x00205892
			// (set) Token: 0x06005F62 RID: 24418 RVA: 0x0020769A File Offset: 0x0020589A
			[Serialize(false, IsPropertySaveable.No, "Should the buffs of the character executing the effect be transferred to the spawned character? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool TransferBuffs { get; private set; }

			// Token: 0x170015BD RID: 5565
			// (get) Token: 0x06005F63 RID: 24419 RVA: 0x002076A3 File Offset: 0x002058A3
			// (set) Token: 0x06005F64 RID: 24420 RVA: 0x002076AB File Offset: 0x002058AB
			[Serialize(false, IsPropertySaveable.No, "Should the afflictions of the character executing the effect be transferred to the spawned character? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool TransferAfflictions { get; private set; }

			// Token: 0x170015BE RID: 5566
			// (get) Token: 0x06005F65 RID: 24421 RVA: 0x002076B4 File Offset: 0x002058B4
			// (set) Token: 0x06005F66 RID: 24422 RVA: 0x002076BC File Offset: 0x002058BC
			[Serialize(false, IsPropertySaveable.No, "Should the the items from the character executing the effect be transferred to the spawned character? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool TransferInventory { get; private set; }

			// Token: 0x170015BF RID: 5567
			// (get) Token: 0x06005F67 RID: 24423 RVA: 0x002076C5 File Offset: 0x002058C5
			// (set) Token: 0x06005F68 RID: 24424 RVA: 0x002076CD File Offset: 0x002058CD
			[Serialize(0, IsPropertySaveable.No, "The maximum number of creatures of the given species and team that can exist per team in the current level before this status effect stops spawning any more.", "", false)]
			public int TotalMaxCount { get; private set; }

			// Token: 0x170015C0 RID: 5568
			// (get) Token: 0x06005F69 RID: 24425 RVA: 0x002076D6 File Offset: 0x002058D6
			// (set) Token: 0x06005F6A RID: 24426 RVA: 0x002076DE File Offset: 0x002058DE
			[Serialize(0, IsPropertySaveable.No, "Amount of stun to apply on the spawned character.", "", false)]
			public int Stun { get; private set; }

			// Token: 0x170015C1 RID: 5569
			// (get) Token: 0x06005F6B RID: 24427 RVA: 0x002076E7 File Offset: 0x002058E7
			// (set) Token: 0x06005F6C RID: 24428 RVA: 0x002076EF File Offset: 0x002058EF
			[Serialize("", IsPropertySaveable.No, "An affliction to apply on the spawned character.", "", false)]
			public Identifier AfflictionOnSpawn { get; private set; }

			// Token: 0x170015C2 RID: 5570
			// (get) Token: 0x06005F6D RID: 24429 RVA: 0x002076F8 File Offset: 0x002058F8
			// (set) Token: 0x06005F6E RID: 24430 RVA: 0x00207700 File Offset: 0x00205900
			[Serialize(1, IsPropertySaveable.No, "The strength of the affliction applied on the spawned character. Only relevant if AfflictionOnSpawn is defined.", "", false)]
			public int AfflictionStrength { get; private set; }

			// Token: 0x170015C3 RID: 5571
			// (get) Token: 0x06005F6F RID: 24431 RVA: 0x00207709 File Offset: 0x00205909
			// (set) Token: 0x06005F70 RID: 24432 RVA: 0x00207711 File Offset: 0x00205911
			[Serialize(false, IsPropertySaveable.No, "Should the player controlling the character that executes the effect gain control of the spawned character? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool TransferControl { get; private set; }

			// Token: 0x170015C4 RID: 5572
			// (get) Token: 0x06005F71 RID: 24433 RVA: 0x0020771A File Offset: 0x0020591A
			// (set) Token: 0x06005F72 RID: 24434 RVA: 0x00207722 File Offset: 0x00205922
			[Serialize(false, IsPropertySaveable.No, "Should the character that executes the effect be removed when the effect executes? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool RemovePreviousCharacter { get; private set; }

			// Token: 0x170015C5 RID: 5573
			// (get) Token: 0x06005F73 RID: 24435 RVA: 0x0020772B File Offset: 0x0020592B
			// (set) Token: 0x06005F74 RID: 24436 RVA: 0x00207733 File Offset: 0x00205933
			[Serialize(0f, IsPropertySaveable.No, "Amount of random spread to add to the spawn position. Can be used to prevent all the characters from spawning at the exact same position if the effect spawns multiple ones.", "", false)]
			public float Spread { get; private set; }

			// Token: 0x170015C6 RID: 5574
			// (get) Token: 0x06005F75 RID: 24437 RVA: 0x0020773C File Offset: 0x0020593C
			// (set) Token: 0x06005F76 RID: 24438 RVA: 0x00207744 File Offset: 0x00205944
			[Serialize("0,0", IsPropertySaveable.No, "Offset added to the spawn position. Can be used to for example spawn a character a bit up from the center of an item executing the effect.", "", false)]
			public Vector2 Offset { get; private set; }

			// Token: 0x170015C7 RID: 5575
			// (get) Token: 0x06005F77 RID: 24439 RVA: 0x0020774D File Offset: 0x0020594D
			// (set) Token: 0x06005F78 RID: 24440 RVA: 0x00207755 File Offset: 0x00205955
			[Serialize(false, IsPropertySaveable.No, "", "", false)]
			public bool InheritEventTags { get; private set; }

			// Token: 0x170015C8 RID: 5576
			// (get) Token: 0x06005F79 RID: 24441 RVA: 0x0020775E File Offset: 0x0020595E
			// (set) Token: 0x06005F7A RID: 24442 RVA: 0x00207766 File Offset: 0x00205966
			[Serialize(false, IsPropertySaveable.No, "Should the character team be inherited from the entity that owns the status effect?", "", false)]
			public bool InheritTeam { get; private set; }

			// Token: 0x06005F7B RID: 24443 RVA: 0x00207770 File Offset: 0x00205970
			public CharacterSpawnInfo(ContentXElement element, string parentDebugName)
			{
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
				if (this.SpeciesName.IsEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Invalid character spawn (");
					defaultInterpolatedStringHandler.AppendFormatted(this.Name);
					defaultInterpolatedStringHandler.AppendLiteral(") in StatusEffect \"");
					defaultInterpolatedStringHandler.AppendFormatted(parentDebugName);
					defaultInterpolatedStringHandler.AppendLiteral("\" - identifier not found in the element \"");
					defaultInterpolatedStringHandler.AppendFormatted<ContentXElement>(element);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
			}
		}

		// Token: 0x02000B0E RID: 2830
		public class AITrigger : ISerializableEntity
		{
			// Token: 0x170015C9 RID: 5577
			// (get) Token: 0x06005F7C RID: 24444 RVA: 0x00207811 File Offset: 0x00205A11
			public string Name
			{
				get
				{
					return "ai trigger";
				}
			}

			// Token: 0x170015CA RID: 5578
			// (get) Token: 0x06005F7D RID: 24445 RVA: 0x00207818 File Offset: 0x00205A18
			// (set) Token: 0x06005F7E RID: 24446 RVA: 0x00207820 File Offset: 0x00205A20
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

			// Token: 0x170015CB RID: 5579
			// (get) Token: 0x06005F7F RID: 24447 RVA: 0x00207829 File Offset: 0x00205A29
			// (set) Token: 0x06005F80 RID: 24448 RVA: 0x00207831 File Offset: 0x00205A31
			[Serialize(AIState.Idle, IsPropertySaveable.No, "The AI state the character should switch to.", "", false)]
			public AIState State { get; private set; }

			// Token: 0x170015CC RID: 5580
			// (get) Token: 0x06005F81 RID: 24449 RVA: 0x0020783A File Offset: 0x00205A3A
			// (set) Token: 0x06005F82 RID: 24450 RVA: 0x00207842 File Offset: 0x00205A42
			[Serialize(0f, IsPropertySaveable.No, "How long should the character stay in the specified state? If 0, the effect is permanent (unless overridden by another AITrigger).", "", false)]
			public float Duration { get; private set; }

			// Token: 0x170015CD RID: 5581
			// (get) Token: 0x06005F83 RID: 24451 RVA: 0x0020784B File Offset: 0x00205A4B
			// (set) Token: 0x06005F84 RID: 24452 RVA: 0x00207853 File Offset: 0x00205A53
			[Serialize(1f, IsPropertySaveable.No, "How likely is the AI to change the state when this effect executes? 1 = always, 0.5 = 50% chance, 0 = never.", "", false)]
			public float Probability { get; private set; }

			// Token: 0x170015CE RID: 5582
			// (get) Token: 0x06005F85 RID: 24453 RVA: 0x0020785C File Offset: 0x00205A5C
			// (set) Token: 0x06005F86 RID: 24454 RVA: 0x00207864 File Offset: 0x00205A64
			[Serialize(0f, IsPropertySaveable.No, "How much damage the character must receive for this AITrigger to become active? Checks the amount of damage the latest attack did to the character.", "", false)]
			public float MinDamage { get; private set; }

			// Token: 0x170015CF RID: 5583
			// (get) Token: 0x06005F87 RID: 24455 RVA: 0x0020786D File Offset: 0x00205A6D
			// (set) Token: 0x06005F88 RID: 24456 RVA: 0x00207875 File Offset: 0x00205A75
			[Serialize(true, IsPropertySaveable.No, "Can this AITrigger override other active AITriggers?", "", false)]
			public bool AllowToOverride { get; private set; }

			// Token: 0x170015D0 RID: 5584
			// (get) Token: 0x06005F89 RID: 24457 RVA: 0x0020787E File Offset: 0x00205A7E
			// (set) Token: 0x06005F8A RID: 24458 RVA: 0x00207886 File Offset: 0x00205A86
			[Serialize(true, IsPropertySaveable.No, "Can this AITrigger be overridden by other AITriggers?", "", false)]
			public bool AllowToBeOverridden { get; private set; }

			// Token: 0x170015D1 RID: 5585
			// (get) Token: 0x06005F8B RID: 24459 RVA: 0x0020788F File Offset: 0x00205A8F
			// (set) Token: 0x06005F8C RID: 24460 RVA: 0x00207897 File Offset: 0x00205A97
			public bool IsTriggered { get; private set; }

			// Token: 0x170015D2 RID: 5586
			// (get) Token: 0x06005F8D RID: 24461 RVA: 0x002078A0 File Offset: 0x00205AA0
			// (set) Token: 0x06005F8E RID: 24462 RVA: 0x002078A8 File Offset: 0x00205AA8
			public float Timer { get; private set; }

			// Token: 0x170015D3 RID: 5587
			// (get) Token: 0x06005F8F RID: 24463 RVA: 0x002078B1 File Offset: 0x00205AB1
			// (set) Token: 0x06005F90 RID: 24464 RVA: 0x002078B9 File Offset: 0x00205AB9
			public bool IsActive { get; private set; }

			// Token: 0x170015D4 RID: 5588
			// (get) Token: 0x06005F91 RID: 24465 RVA: 0x002078C2 File Offset: 0x00205AC2
			// (set) Token: 0x06005F92 RID: 24466 RVA: 0x002078CA File Offset: 0x00205ACA
			public bool IsPermanent { get; private set; }

			// Token: 0x06005F93 RID: 24467 RVA: 0x002078D3 File Offset: 0x00205AD3
			public void Launch()
			{
				this.IsTriggered = true;
				this.IsActive = true;
				this.IsPermanent = (this.Duration <= 0f);
				if (!this.IsPermanent)
				{
					this.Timer = this.Duration;
				}
			}

			// Token: 0x06005F94 RID: 24468 RVA: 0x0020790D File Offset: 0x00205B0D
			public void Reset()
			{
				this.IsTriggered = false;
				this.IsActive = false;
				this.Timer = 0f;
			}

			// Token: 0x06005F95 RID: 24469 RVA: 0x00207928 File Offset: 0x00205B28
			public void UpdateTimer(float deltaTime)
			{
				if (this.IsPermanent)
				{
					return;
				}
				this.Timer -= deltaTime;
				if (this.Timer < 0f)
				{
					this.Timer = 0f;
					this.IsActive = false;
				}
			}

			// Token: 0x06005F96 RID: 24470 RVA: 0x00207960 File Offset: 0x00205B60
			public AITrigger(XElement element)
			{
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			}
		}

		// Token: 0x02000B0F RID: 2831
		public readonly struct AnimLoadInfo : IEquatable<StatusEffect.AnimLoadInfo>
		{
			// Token: 0x06005F97 RID: 24471 RVA: 0x00207975 File Offset: 0x00205B75
			public AnimLoadInfo(AnimationType Type, Either<string, ContentPath> File, float Priority, ImmutableArray<Identifier> ExpectedSpeciesNames)
			{
				this.Type = Type;
				this.File = File;
				this.Priority = Priority;
				this.ExpectedSpeciesNames = ExpectedSpeciesNames;
			}

			// Token: 0x170015D5 RID: 5589
			// (get) Token: 0x06005F98 RID: 24472 RVA: 0x00207994 File Offset: 0x00205B94
			// (set) Token: 0x06005F99 RID: 24473 RVA: 0x0020799C File Offset: 0x00205B9C
			public AnimationType Type { get; set; }

			// Token: 0x170015D6 RID: 5590
			// (get) Token: 0x06005F9A RID: 24474 RVA: 0x002079A5 File Offset: 0x00205BA5
			// (set) Token: 0x06005F9B RID: 24475 RVA: 0x002079AD File Offset: 0x00205BAD
			public Either<string, ContentPath> File { get; set; }

			// Token: 0x170015D7 RID: 5591
			// (get) Token: 0x06005F9C RID: 24476 RVA: 0x002079B6 File Offset: 0x00205BB6
			// (set) Token: 0x06005F9D RID: 24477 RVA: 0x002079BE File Offset: 0x00205BBE
			public float Priority { get; set; }

			// Token: 0x170015D8 RID: 5592
			// (get) Token: 0x06005F9E RID: 24478 RVA: 0x002079C7 File Offset: 0x00205BC7
			// (set) Token: 0x06005F9F RID: 24479 RVA: 0x002079CF File Offset: 0x00205BCF
			public ImmutableArray<Identifier> ExpectedSpeciesNames { get; set; }

			// Token: 0x06005FA0 RID: 24480 RVA: 0x002079D8 File Offset: 0x00205BD8
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("AnimLoadInfo");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06005FA1 RID: 24481 RVA: 0x00207A24 File Offset: 0x00205C24
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Type = ");
				builder.Append(this.Type.ToString());
				builder.Append(", File = ");
				builder.Append(this.File);
				builder.Append(", Priority = ");
				builder.Append(this.Priority.ToString());
				builder.Append(", ExpectedSpeciesNames = ");
				builder.Append(this.ExpectedSpeciesNames.ToString());
				return true;
			}

			// Token: 0x06005FA2 RID: 24482 RVA: 0x00207AC0 File Offset: 0x00205CC0
			[CompilerGenerated]
			public static bool operator !=(StatusEffect.AnimLoadInfo left, StatusEffect.AnimLoadInfo right)
			{
				return !(left == right);
			}

			// Token: 0x06005FA3 RID: 24483 RVA: 0x00207ACC File Offset: 0x00205CCC
			[CompilerGenerated]
			public static bool operator ==(StatusEffect.AnimLoadInfo left, StatusEffect.AnimLoadInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x06005FA4 RID: 24484 RVA: 0x00207AD8 File Offset: 0x00205CD8
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<AnimationType>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<Either<string, ContentPath>>.Default.GetHashCode(this.<File>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Priority>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<ExpectedSpeciesNames>k__BackingField);
			}

			// Token: 0x06005FA5 RID: 24485 RVA: 0x00207B3A File Offset: 0x00205D3A
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is StatusEffect.AnimLoadInfo && this.Equals((StatusEffect.AnimLoadInfo)obj);
			}

			// Token: 0x06005FA6 RID: 24486 RVA: 0x00207B54 File Offset: 0x00205D54
			[CompilerGenerated]
			public bool Equals(StatusEffect.AnimLoadInfo other)
			{
				return EqualityComparer<AnimationType>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<Either<string, ContentPath>>.Default.Equals(this.<File>k__BackingField, other.<File>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Priority>k__BackingField, other.<Priority>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<ExpectedSpeciesNames>k__BackingField, other.<ExpectedSpeciesNames>k__BackingField);
			}

			// Token: 0x06005FA7 RID: 24487 RVA: 0x00207BC1 File Offset: 0x00205DC1
			[CompilerGenerated]
			public void Deconstruct(out AnimationType Type, out Either<string, ContentPath> File, out float Priority, out ImmutableArray<Identifier> ExpectedSpeciesNames)
			{
				Type = this.Type;
				File = this.File;
				Priority = this.Priority;
				ExpectedSpeciesNames = this.ExpectedSpeciesNames;
			}
		}

		// Token: 0x02000B10 RID: 2832
		// (Invoke) Token: 0x06005FA9 RID: 24489
		private delegate bool ShouldShortCircuit(bool condition, out bool valueToReturn);

		// Token: 0x02000B11 RID: 2833
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400387D RID: 14461
			public static StatusEffect.ShouldShortCircuit <0>__ShouldShortCircuitLogicalOrOperator;

			// Token: 0x0400387E RID: 14462
			public static StatusEffect.ShouldShortCircuit <1>__ShouldShortCircuitLogicalAndOperator;

			// Token: 0x0400387F RID: 14463
			public static Func<ISerializableEntity, Character> <2>__GetCharacterFromTarget;
		}
	}
}
