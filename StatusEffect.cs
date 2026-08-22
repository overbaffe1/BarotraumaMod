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
using Barotrauma.Particles;
using Barotrauma.Sounds;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200013E RID: 318
	internal class StatusEffect
	{
		// Token: 0x17000A9B RID: 2715
		// (get) Token: 0x0600294F RID: 10575 RVA: 0x001C8171 File Offset: 0x001C6371
		public IEnumerable<RoundSound> Sounds
		{
			get
			{
				return this.sounds;
			}
		}

		// Token: 0x06002950 RID: 10576 RVA: 0x001C817C File Offset: 0x001C637C
		private void PlaySound(Entity entity, Hull hull, Vector2 worldPosition)
		{
			StatusEffect.<>c__DisplayClass15_0 CS$<>8__locals1 = new StatusEffect.<>c__DisplayClass15_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.worldPosition = worldPosition;
			CS$<>8__locals1.hull = hull;
			CS$<>8__locals1.entity = entity;
			if (this.sounds.Count == 0)
			{
				return;
			}
			if (CS$<>8__locals1.entity != null)
			{
				Submarine submarine = CS$<>8__locals1.entity.Submarine;
				if (submarine != null && submarine.Loading)
				{
					return;
				}
			}
			if (this.soundChannel == null || !this.soundChannel.IsPlaying || this.forcePlaySounds)
			{
				if (this.soundChannel != null && this.soundChannel.IsPlaying)
				{
					this.soundChannel.FadeOutAndDispose();
				}
				if (this.soundSelectionMode == SoundSelectionMode.All)
				{
					using (List<RoundSound>.Enumerator enumerator = this.sounds.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							RoundSound sound = enumerator.Current;
							if (((sound != null) ? sound.Sound : null) == null)
							{
								string errorMsg = "Error in StatusEffect.ApplyProjSpecific1 (sound \"" + (((sound != null) ? sound.Filename : null) ?? "unknown") + "\" was null)\n" + Environment.StackTrace.CleanupStackTrace();
								GameAnalyticsManager.AddErrorEventOnce("StatusEffect.ApplyProjSpecific:SoundNull1" + Environment.StackTrace.CleanupStackTrace(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
								return;
							}
							CS$<>8__locals1.<PlaySound>g__PlaySoundOrDelayIfNotLoaded|0(sound);
						}
						goto IL_26C;
					}
				}
				int selectedSoundIndex;
				if (this.soundSelectionMode == SoundSelectionMode.ItemSpecific)
				{
					Item item = CS$<>8__locals1.entity as Item;
					if (item != null)
					{
						selectedSoundIndex = (int)item.ID % this.sounds.Count;
						goto IL_1A9;
					}
				}
				if (this.soundSelectionMode == SoundSelectionMode.CharacterSpecific)
				{
					Character user = CS$<>8__locals1.entity as Character;
					if (user != null)
					{
						selectedSoundIndex = (int)user.ID % this.sounds.Count;
						goto IL_1A9;
					}
				}
				selectedSoundIndex = Rand.Int(this.sounds.Count, Rand.RandSync.Unsynced);
				IL_1A9:
				RoundSound selectedSound = this.sounds[selectedSoundIndex];
				if (((selectedSound != null) ? selectedSound.Sound : null) == null)
				{
					string errorMsg2 = "Error in StatusEffect.ApplyProjSpecific2 (sound \"" + (((selectedSound != null) ? selectedSound.Filename : null) ?? "unknown") + "\" was null)\n" + Environment.StackTrace.CleanupStackTrace();
					GameAnalyticsManager.AddErrorEventOnce("StatusEffect.ApplyProjSpecific:SoundNull2" + Environment.StackTrace.CleanupStackTrace(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
					return;
				}
				CS$<>8__locals1.<PlaySound>g__PlaySoundOrDelayIfNotLoaded|0(selectedSound);
			}
			else
			{
				this.soundChannel.Position = new Vector3?(new Vector3(CS$<>8__locals1.worldPosition, 0f));
				if (this.lastPlayingSound != null && this.lastPlayingSound.Stream)
				{
					this.lastPlayingSound.LastStreamSeekPos = this.soundChannel.StreamSeekPos;
				}
			}
			IL_26C:
			CS$<>8__locals1.<PlaySound>g__KeepLoopingSoundAlive|3(this.soundChannel);
		}

		// Token: 0x06002951 RID: 10577 RVA: 0x001C8414 File Offset: 0x001C6614
		static StatusEffect()
		{
			StatusEffect.FieldNames = (from f in typeof(StatusEffect).GetFields().AsEnumerable<FieldInfo>()
			select f.Name.ToIdentifier()).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x17000A9C RID: 2716
		// (get) Token: 0x06002952 RID: 10578 RVA: 0x001C8472 File Offset: 0x001C6672
		public bool HasConditions
		{
			get
			{
				return this.propertyConditionals != null && this.propertyConditionals.Any<PropertyConditional>();
			}
		}

		// Token: 0x17000A9D RID: 2717
		// (get) Token: 0x06002953 RID: 10579 RVA: 0x001C848C File Offset: 0x001C668C
		public IEnumerable<Explosion> Explosions
		{
			get
			{
				IEnumerable<Explosion> enumerable = this.explosions;
				return enumerable ?? Enumerable.Empty<Explosion>();
			}
		}

		// Token: 0x17000A9E RID: 2718
		// (get) Token: 0x06002954 RID: 10580 RVA: 0x001C84AA File Offset: 0x001C66AA
		// (set) Token: 0x06002955 RID: 10581 RVA: 0x001C84B2 File Offset: 0x001C66B2
		public List<Affliction> Afflictions { get; private set; } = new List<Affliction>();

		// Token: 0x17000A9F RID: 2719
		// (get) Token: 0x06002956 RID: 10582 RVA: 0x001C84BC File Offset: 0x001C66BC
		public IEnumerable<StatusEffect.CharacterSpawnInfo> SpawnCharacters
		{
			get
			{
				IEnumerable<StatusEffect.CharacterSpawnInfo> enumerable = this.spawnCharacters;
				return enumerable ?? Enumerable.Empty<StatusEffect.CharacterSpawnInfo>();
			}
		}

		// Token: 0x17000AA0 RID: 2720
		// (get) Token: 0x06002957 RID: 10583 RVA: 0x001C84DA File Offset: 0x001C66DA
		// (set) Token: 0x06002958 RID: 10584 RVA: 0x001C84E2 File Offset: 0x001C66E2
		public float Range { get; private set; }

		// Token: 0x17000AA1 RID: 2721
		// (get) Token: 0x06002959 RID: 10585 RVA: 0x001C84EB File Offset: 0x001C66EB
		// (set) Token: 0x0600295A RID: 10586 RVA: 0x001C84F3 File Offset: 0x001C66F3
		public Vector2 Offset { get; private set; }

		// Token: 0x17000AA2 RID: 2722
		// (get) Token: 0x0600295B RID: 10587 RVA: 0x001C84FC File Offset: 0x001C66FC
		// (set) Token: 0x0600295C RID: 10588 RVA: 0x001C8504 File Offset: 0x001C6704
		public bool OffsetCopiesEntityTransform { get; private set; }

		// Token: 0x17000AA3 RID: 2723
		// (get) Token: 0x0600295D RID: 10589 RVA: 0x001C850D File Offset: 0x001C670D
		// (set) Token: 0x0600295E RID: 10590 RVA: 0x001C8515 File Offset: 0x001C6715
		public float RandomOffset { get; private set; }

		// Token: 0x17000AA4 RID: 2724
		// (get) Token: 0x0600295F RID: 10591 RVA: 0x001C851E File Offset: 0x001C671E
		// (set) Token: 0x06002960 RID: 10592 RVA: 0x001C8530 File Offset: 0x001C6730
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

		// Token: 0x17000AA5 RID: 2725
		// (get) Token: 0x06002961 RID: 10593 RVA: 0x001C8594 File Offset: 0x001C6794
		// (set) Token: 0x06002962 RID: 10594 RVA: 0x001C859C File Offset: 0x001C679C
		public bool Disabled { get; private set; }

		// Token: 0x06002963 RID: 10595 RVA: 0x001C85A5 File Offset: 0x001C67A5
		public static StatusEffect Load(ContentXElement element, string parentDebugName)
		{
			if (element.GetAttribute("delay") != null || element.GetAttribute("delaytype") != null)
			{
				return new DelayedEffect(element, parentDebugName);
			}
			return new StatusEffect(element, parentDebugName);
		}

		// Token: 0x06002964 RID: 10596 RVA: 0x001C85D0 File Offset: 0x001C67D0
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
			this.playSoundOnRequiredItemFailure = element.GetAttributeBool("playsoundonrequireditemfailure", false);
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
								goto IL_9D9;
							}
							if (!(text == "type"))
							{
								goto IL_9D9;
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
								goto IL_9D9;
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
									goto IL_9D9;
								}
								if (!(text == "sound"))
								{
									goto IL_9D9;
								}
								DebugConsole.ThrowError("Error in StatusEffect (" + parentDebugName + "): sounds should be defined as child elements of the StatusEffect, not as attributes.", null, element.ContentPackage, false, false);
								continue;
							}
							else
							{
								if (!(text == "range"))
								{
									goto IL_9D9;
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
								goto IL_9D9;
							}
							continue;
						}
						break;
					}
					case 6:
						if (!(text == "target"))
						{
							goto IL_9D9;
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
									goto IL_9D9;
								}
								if (!(text == "targets"))
								{
									goto IL_9D9;
								}
								continue;
							}
							else
							{
								if (!(text == "settags"))
								{
									goto IL_9D9;
								}
								propertyAttributes.Add(attribute);
								continue;
							}
						}
						else
						{
							if (!(text == "oneshot"))
							{
								goto IL_9D9;
							}
							this.oneShot = attribute.GetAttributeBool(false);
							continue;
						}
						break;
					}
					case 8:
						if (!(text == "interval"))
						{
							goto IL_9D9;
						}
						continue;
					case 9:
					case 12:
					case 13:
					case 14:
					case 15:
					case 16:
					case 20:
						goto IL_9D9;
					case 10:
					{
						char c = text[7];
						if (c <= 'i')
						{
							if (c != 'a')
							{
								if (c != 'i')
								{
									goto IL_9D9;
								}
								if (!(text == "targetlimb"))
								{
									goto IL_9D9;
								}
								continue;
							}
							else
							{
								if (!(text == "targettags"))
								{
									goto IL_9D9;
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
									goto IL_9D9;
								}
								if (!(text == "targettype"))
								{
									goto IL_9D9;
								}
								continue;
							}
							else
							{
								if (!(text == "comparison"))
								{
									goto IL_9D9;
								}
								goto IL_8AC;
							}
						}
						else
						{
							if (!(text == "severlimbs"))
							{
								goto IL_9D9;
							}
							continue;
						}
						break;
					}
					case 11:
						if (!(text == "targetnames"))
						{
							goto IL_9D9;
						}
						continue;
					case 17:
						if (!(text == "targetidentifiers"))
						{
							goto IL_9D9;
						}
						continue;
					case 18:
						if (!(text == "allowedafflictions"))
						{
							goto IL_9D9;
						}
						break;
					case 19:
						if (!(text == "requiredafflictions"))
						{
							goto IL_9D9;
						}
						break;
					case 21:
						if (!(text == "conditionalcomparison"))
						{
							goto IL_9D9;
						}
						goto IL_8AC;
					default:
						goto IL_9D9;
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
					IL_8AC:
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
				IL_9D9:
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
							goto IL_1755;
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
						goto IL_1035;
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
							goto IL_1755;
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
							goto IL_1035;
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
							goto IL_15B8;
						}
						else
						{
							if (!(text2 == "requireditem"))
							{
								continue;
							}
							goto IL_10AB;
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
								goto IL_10AB;
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
								goto IL_15B8;
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
								if (c != 's')
								{
									continue;
								}
								if (!(text2 == "steamtimelineevent"))
								{
									continue;
								}
								this.steamTimeLineEventToTrigger = new StatusEffect.SteamTimeLineEvent(subElement.GetAttributeString("title", string.Empty), subElement.GetAttributeString("description", string.Empty), subElement.GetAttributeString("icon", string.Empty));
								if (this.steamTimeLineEventToTrigger.title.IsNullOrWhiteSpace())
								{
									DebugConsole.ThrowError("Error in StatusEffect (" + parentDebugName + ") - steam timeline event has no title.", null, element.ContentPackage, false, false);
								}
								if (this.steamTimeLineEventToTrigger.description.IsNullOrWhiteSpace())
								{
									DebugConsole.ThrowError("Error in StatusEffect (" + parentDebugName + ") - steam timeline event has no description.", null, element.ContentPackage, false, false);
								}
								if (this.steamTimeLineEventToTrigger.icon.IsNullOrWhiteSpace())
								{
									DebugConsole.ThrowError("Error in StatusEffect (" + parentDebugName + ") - steam timeline event has no icon.", null, element.ContentPackage, false, false);
									continue;
								}
								continue;
							}
							else
							{
								if (!(text2 == "requiredaffliction"))
								{
									continue;
								}
								goto IL_10F7;
							}
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
						goto IL_10F7;
					default:
						continue;
					}
					this.useItemCount++;
					continue;
					IL_1035:
					this.removeItem = true;
					continue;
					IL_10AB:
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
					IL_10F7:
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
					IL_15B8:
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
					IL_1755:
					if (this.luaHook == null)
					{
						this.luaHook = new List<ValueTuple<string, ContentXElement>>();
					}
					this.luaHook.Add(new ValueTuple<string, ContentXElement>(subElement.GetAttributeString("name", ""), subElement));
				}
			}
			this.InitProjSpecific(element, parentDebugName);
		}

		// Token: 0x06002965 RID: 10597 RVA: 0x001CA000 File Offset: 0x001C8200
		private void InitProjSpecific(ContentXElement element, string parentDebugName)
		{
			this.particleEmitters = new List<ParticleEmitter>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "particleemitter"))
				{
					if (a == "sound")
					{
						RoundSound sound = RoundSound.Load(subElement);
						if (((sound != null) ? sound.Sound : null) != null)
						{
							this.loopSound = subElement.GetAttributeBool("loop", false);
							SoundSelectionMode selectionMode;
							if (subElement.GetAttribute("selectionmode") != null && Enum.TryParse<SoundSelectionMode>(subElement.GetAttributeString("selectionmode", "Random"), out selectionMode))
							{
								this.soundSelectionMode = selectionMode;
							}
							this.sounds.Add(sound);
						}
					}
				}
				else
				{
					this.particleEmitters.Add(new ParticleEmitter(subElement));
				}
			}
			this.forcePlaySounds = element.GetAttributeBool("forcePlaySounds", false);
		}

		// Token: 0x06002966 RID: 10598 RVA: 0x001CA10C File Offset: 0x001C830C
		public bool HasTargetType(StatusEffect.TargetType targetType)
		{
			return (this.targetTypes & targetType) > (StatusEffect.TargetType)0;
		}

		// Token: 0x06002967 RID: 10599 RVA: 0x001CA11C File Offset: 0x001C831C
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

		// Token: 0x06002968 RID: 10600 RVA: 0x001CA1A8 File Offset: 0x001C83A8
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

		// Token: 0x06002969 RID: 10601 RVA: 0x001CA230 File Offset: 0x001C8430
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

		// Token: 0x0600296A RID: 10602 RVA: 0x001CA280 File Offset: 0x001C8480
		public bool MatchesTagConditionals(ItemPrefab itemPrefab)
		{
			return itemPrefab != null && this.HasConditions && itemPrefab.Tags.Any((Identifier t) => this.propertyConditionals.Any((PropertyConditional pc) => pc.TargetTagMatchesTagCondition(t)));
		}

		// Token: 0x0600296B RID: 10603 RVA: 0x001CA2A6 File Offset: 0x001C84A6
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

		// Token: 0x0600296C RID: 10604 RVA: 0x001CA2D8 File Offset: 0x001C84D8
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

		// Token: 0x0600296D RID: 10605 RVA: 0x001CA368 File Offset: 0x001C8568
		public void AddNearbyTargets(Vector2 worldPosition, List<ISerializableEntity> targets)
		{
			StatusEffect.<>c__DisplayClass139_0 CS$<>8__locals1;
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
					if (c.Enabled && !c.Removed && this.<AddNearbyTargets>g__CheckDistance|139_0(c, ref CS$<>8__locals1) && this.IsValidTarget(c))
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
								if (!item.Removed && this.<AddNearbyTargets>g__CheckDistance|139_0(item, ref CS$<>8__locals1) && this.IsValidTarget(item))
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
					if (!item2.Removed && this.<AddNearbyTargets>g__CheckDistance|139_0(item2, ref CS$<>8__locals1) && this.IsValidTarget(item2))
					{
						targets.AddRange(item2.AllPropertyObjects);
					}
				}
			}
		}

		// Token: 0x0600296E RID: 10606 RVA: 0x001CA550 File Offset: 0x001C8750
		public bool HasRequiredConditions(IReadOnlyList<ISerializableEntity> targets)
		{
			return this.HasRequiredConditions(targets, this.propertyConditionals, false);
		}

		// Token: 0x0600296F RID: 10607 RVA: 0x001CA560 File Offset: 0x001C8760
		private static bool ShouldShortCircuitLogicalOrOperator(bool condition, out bool valueToReturn)
		{
			valueToReturn = true;
			return condition;
		}

		// Token: 0x06002970 RID: 10608 RVA: 0x001CA566 File Offset: 0x001C8766
		private static bool ShouldShortCircuitLogicalAndOperator(bool condition, out bool valueToReturn)
		{
			valueToReturn = false;
			return !condition;
		}

		// Token: 0x06002971 RID: 10609 RVA: 0x001CA570 File Offset: 0x001C8770
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
					if (shouldShortCircuit(StatusEffect.<HasRequiredConditions>g__AnyTargetMatches|144_1(targets, pc.TargetItemComponent, pc), out valueToReturn))
					{
						return valueToReturn;
					}
				}
				else
				{
					ISerializableEntity target = StatusEffect.<HasRequiredConditions>g__FindTargetItemOrComponent|144_2(targets);
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
							else if (shouldShortCircuit(StatusEffect.<HasRequiredConditions>g__AnyTargetMatches|144_1(container.AllPropertyObjects, pc.TargetItemComponent, pc), out valueToReturn))
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

		// Token: 0x06002972 RID: 10610 RVA: 0x001CA788 File Offset: 0x001C8988
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

		// Token: 0x06002973 RID: 10611 RVA: 0x001CA834 File Offset: 0x001C8A34
		protected bool IsValidTarget(ItemComponent itemComponent)
		{
			return (!this.OnlyInside || itemComponent.Item.CurrentHull != null) && (!this.OnlyOutside || itemComponent.Item.CurrentHull == null) && (this.TargetItemComponent.IsNullOrEmpty() || itemComponent.Name.Equals(this.TargetItemComponent, StringComparison.OrdinalIgnoreCase)) && (this.TargetIdentifiers == null || this.TargetIdentifiers.Contains("itemcomponent") || itemComponent.Item.HasTag(this.TargetIdentifiers) || this.TargetIdentifiers.Contains(itemComponent.Item.Prefab.Identifier));
		}

		// Token: 0x06002974 RID: 10612 RVA: 0x001CA8E8 File Offset: 0x001C8AE8
		protected bool IsValidTarget(Item item)
		{
			return (!this.OnlyInside || item.CurrentHull != null) && (!this.OnlyOutside || item.CurrentHull == null) && (this.TargetIdentifiers == null || this.TargetIdentifiers.Contains("item") || item.HasTag(this.TargetIdentifiers) || this.TargetIdentifiers.Contains(item.Prefab.Identifier));
		}

		// Token: 0x06002975 RID: 10613 RVA: 0x001CA964 File Offset: 0x001C8B64
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

		// Token: 0x06002976 RID: 10614 RVA: 0x001CAA00 File Offset: 0x001C8C00
		public void SetUser(Character user)
		{
			this.user = user;
			foreach (Affliction affliction in this.Afflictions)
			{
				affliction.Source = user;
			}
		}

		// Token: 0x06002977 RID: 10615 RVA: 0x001CAA5C File Offset: 0x001C8C5C
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

		// Token: 0x06002978 RID: 10616 RVA: 0x001CAB58 File Offset: 0x001C8D58
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

		// Token: 0x06002979 RID: 10617 RVA: 0x001CAC38 File Offset: 0x001C8E38
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
				if (!hasRequiredItems && this.playSoundOnRequiredItemFailure)
				{
					this.PlaySound(entity, this.GetHull(entity), this.GetPosition(entity, targets, worldPosition));
				}
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

		// Token: 0x0600297A RID: 10618 RVA: 0x001CAD6C File Offset: 0x001C8F6C
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

		// Token: 0x0600297B RID: 10619 RVA: 0x001CADA8 File Offset: 0x001C8FA8
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

		// Token: 0x0600297C RID: 10620 RVA: 0x001CAF78 File Offset: 0x001C9178
		protected void Apply(float deltaTime, Entity entity, IReadOnlyList<ISerializableEntity> targets, Vector2? worldPosition = null)
		{
			StatusEffect.<>c__DisplayClass157_0 CS$<>8__locals1 = new StatusEffect.<>c__DisplayClass157_0();
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
							StatusEffect.<>c__DisplayClass157_1 CS$<>8__locals2;
							CS$<>8__locals2.targetCharacter = StatusEffect.GetCharacterFromTarget(target3);
							if (CS$<>8__locals2.targetCharacter != null && !CS$<>8__locals2.targetCharacter.Removed)
							{
								foreach (StatusEffect.GiveSkill giveSkill in this.giveSkills)
								{
									Identifier skillIdentifier = (giveSkill.SkillIdentifier == "randomskill") ? StatusEffect.<Apply>g__GetRandomSkill|157_1(ref CS$<>8__locals2) : giveSkill.SkillIdentifier;
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
								goto IL_118E;
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
									StatusEffect.<>c__DisplayClass157_3 CS$<>8__locals4 = new StatusEffect.<>c__DisplayClass157_3();
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
				IL_118E:;
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
								StatusEffect.<>c__DisplayClass157_6 CS$<>8__locals7;
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
											characterTeamType = StatusEffect.<Apply>g__GetTeamFromSubmarine|157_7(e2, ref CS$<>8__locals7);
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
										characterTeamType = ((owner != null) ? new CharacterTeamType?(owner.TeamID) : StatusEffect.<Apply>g__GetTeamFromSubmarine|157_7(it, ref CS$<>8__locals7));
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
														if (characterSpawnInfo.TransferControl && Character.Controlled == target4)
														{
															Character.Controlled = newCharacter;
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
			this.ApplyProjSpecific(deltaTime, CS$<>8__locals1.entity, CS$<>8__locals1.targets, hull, CS$<>8__locals1.position, true);
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

		// Token: 0x0600297D RID: 10621 RVA: 0x001CC8D0 File Offset: 0x001CAAD0
		private bool IsValidTargetLimb(Limb limb)
		{
			return limb != null && !limb.Removed && !limb.IsSevered && (this.targetLimbs == null || this.targetLimbs.Contains(limb.type));
		}

		// Token: 0x0600297E RID: 10622 RVA: 0x001CC908 File Offset: 0x001CAB08
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

		// Token: 0x0600297F RID: 10623 RVA: 0x001CC93C File Offset: 0x001CAB3C
		private void RemoveCharacter(Character character)
		{
			StatusEffect.<>c__DisplayClass160_0 CS$<>8__locals1 = new StatusEffect.<>c__DisplayClass160_0();
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

		// Token: 0x06002980 RID: 10624 RVA: 0x001CCA34 File Offset: 0x001CAC34
		private void SpawnItem(StatusEffect.ItemSpawnInfo chosenItemSpawnInfo, Entity entity, PhysicsBody sourceBody, Vector2 position, Entity targetEntity)
		{
			StatusEffect.<>c__DisplayClass161_0 CS$<>8__locals1 = new StatusEffect.<>c__DisplayClass161_0();
			CS$<>8__locals1.entity = entity;
			CS$<>8__locals1.sourceBody = sourceBody;
			CS$<>8__locals1.chosenItemSpawnInfo = chosenItemSpawnInfo;
			CS$<>8__locals1.position = position;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.parentItem = (CS$<>8__locals1.entity as Item);
			StatusEffect.<>c__DisplayClass161_0 CS$<>8__locals2 = CS$<>8__locals1;
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
					StatusEffect.<>c__DisplayClass161_0 CS$<>8__locals3 = CS$<>8__locals1;
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

		// Token: 0x06002981 RID: 10625 RVA: 0x001CCF28 File Offset: 0x001CB128
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

		// Token: 0x06002982 RID: 10626 RVA: 0x001CCFFC File Offset: 0x001CB1FC
		private void ApplyProjSpecific(float deltaTime, Entity entity, IReadOnlyList<ISerializableEntity> targets, Hull currentHull, Vector2 worldPosition, bool playSound)
		{
			if (this.steamTimeLineEventToTrigger != default(StatusEffect.SteamTimeLineEvent))
			{
				SteamTimelineManager.AddTimelineEvent(this.steamTimeLineEventToTrigger.title, this.steamTimeLineEventToTrigger.description, this.steamTimeLineEventToTrigger.icon, 1U, (entity != null) ? entity.Submarine : null);
			}
			if (playSound)
			{
				this.PlaySound(entity, currentHull, worldPosition);
			}
			foreach (ParticleEmitter emitter in this.particleEmitters)
			{
				float angle = 0f;
				float particleRotation = 0f;
				bool mirrorAngle = false;
				if (emitter.Prefab.Properties.CopyEntityAngle || emitter.Prefab.Properties.CopyTargetAngle)
				{
					bool entityAngleAssigned = false;
					Limb targetLimb = null;
					Item item = entity as Item;
					if (item != null)
					{
						if (item.body != null)
						{
							angle = item.body.Rotation + ((item.body.Dir > 0f) ? 0f : 3.1415927f);
							particleRotation = -item.body.Rotation;
							if (emitter.Prefab.Properties.CopyEntityDir && item.body.Dir < 0f)
							{
								particleRotation += 3.1415927f;
								mirrorAngle = true;
							}
						}
						else
						{
							angle = -item.RotationRad;
							if (item.FlippedX)
							{
								angle += 3.1415927f;
							}
							particleRotation = item.RotationRad;
						}
						entityAngleAssigned = true;
					}
					if (emitter.Prefab.Properties.CopyTargetAngle || !entityAngleAssigned)
					{
						Character c = entity as Character;
						if (c != null && !c.Removed)
						{
							LimbType[] array = this.targetLimbs;
							LimbType? limbType;
							if (array == null)
							{
								limbType = null;
							}
							else
							{
								limbType = new LimbType?(array.FirstOrDefault((LimbType l) => l > LimbType.None));
							}
							LimbType? limbType2 = limbType;
							if (limbType2 != null)
							{
								LimbType i = limbType2.GetValueOrDefault();
								targetLimb = c.AnimController.GetLimb(i, true, false, false);
								goto IL_220;
							}
						}
						for (int j = 0; j < targets.Count; j++)
						{
							Limb limb = targets[j] as Limb;
							if (limb != null)
							{
								targetLimb = limb;
								break;
							}
						}
					}
					IL_220:
					if (targetLimb != null && !targetLimb.Removed)
					{
						angle = targetLimb.body.Rotation + ((targetLimb.body.Dir > 0f) ? 0f : 3.1415927f);
						particleRotation = -targetLimb.body.Rotation;
						float offset = targetLimb.Params.GetSpriteOrientation() - 1.5707964f;
						particleRotation += offset;
						if (emitter.Prefab.Properties.CopyEntityDir && targetLimb.body.Dir < 0f)
						{
							angle = targetLimb.body.Rotation + ((targetLimb.body.Dir > 0f) ? 0f : 3.1415927f);
							particleRotation = -targetLimb.body.Rotation;
							if (targetLimb.body.Dir < 0f)
							{
								particleRotation += 3.1415927f;
								mirrorAngle = true;
							}
						}
					}
				}
				ParticleEmitter particleEmitter = emitter;
				float angle2 = angle;
				float particleRotation2 = particleRotation;
				float velocityMultiplier = 1f;
				float sizeMultiplier = 1f;
				float amountMultiplier = 1f;
				bool mirrorAngle2 = mirrorAngle;
				particleEmitter.Emit(deltaTime, worldPosition, currentHull, angle2, particleRotation2, velocityMultiplier, sizeMultiplier, amountMultiplier, null, null, mirrorAngle2, null);
			}
		}

		// Token: 0x06002983 RID: 10627 RVA: 0x001CD37C File Offset: 0x001CB57C
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

		// Token: 0x06002984 RID: 10628 RVA: 0x001CD41D File Offset: 0x001CB61D
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

		// Token: 0x06002985 RID: 10629 RVA: 0x001CD440 File Offset: 0x001CB640
		public static void UpdateAll(float deltaTime)
		{
			StatusEffect.UpdateAllProjSpecific(deltaTime);
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
										if (element.User != null && element.Parent.CanGiveMedicalSkill)
										{
											targetCharacter.TryAdjustHealerSkill(element.User, healthChange, null);
										}
									}
								}
							}
							element.Parent.TryTriggerAnimation(target, element.Entity);
						}
						element.Parent.ApplyProjSpecific(deltaTime, element.Entity, element.Targets, element.Parent.GetHull(element.Entity), element.Parent.GetPosition(element.Entity, element.Targets, null), element.Timer >= element.Duration);
						element.Timer -= deltaTime;
						if (element.Timer <= 0f)
						{
							StatusEffect.DurationList.Remove(element);
						}
					}
				}
			}
		}

		// Token: 0x06002986 RID: 10630 RVA: 0x001CD984 File Offset: 0x001CBB84
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

		// Token: 0x06002987 RID: 10631 RVA: 0x001CDA24 File Offset: 0x001CBC24
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

		// Token: 0x06002988 RID: 10632 RVA: 0x001CDB40 File Offset: 0x001CBD40
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

		// Token: 0x06002989 RID: 10633 RVA: 0x001CDC74 File Offset: 0x001CBE74
		private static void UpdateAllProjSpecific(float deltaTime)
		{
			bool doMuffleCheck = Timing.TotalTime > StatusEffect.LastMuffleCheckTime + 0.2;
			if (doMuffleCheck)
			{
				StatusEffect.LastMuffleCheckTime = Timing.TotalTime;
			}
			using (HashSet<StatusEffect>.Enumerator enumerator = StatusEffect.ActiveLoopingSounds.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					StatusEffect statusEffect = enumerator.Current;
					if (statusEffect.soundChannel != null)
					{
						if (Timing.TotalTime > statusEffect.loopStartTime + 0.1 && !StatusEffect.DurationList.Any((DurationListElement e) => e.Parent == statusEffect))
						{
							statusEffect.soundChannel.FadeOutAndDispose();
							statusEffect.soundChannel = null;
						}
						else
						{
							Entity entity = statusEffect.soundEmitter;
							if (entity != null && !entity.Removed)
							{
								statusEffect.soundChannel.Position = new Vector3?(new Vector3(statusEffect.soundEmitter.WorldPosition, 0f));
								if (doMuffleCheck && !statusEffect.ignoreMuffling)
								{
									SoundChannel soundChannel = statusEffect.soundChannel;
									Character controlled = Character.Controlled;
									Vector2 worldPosition = statusEffect.soundEmitter.WorldPosition;
									float far = statusEffect.soundChannel.Far;
									Character controlled2 = Character.Controlled;
									soundChannel.Muffled = SoundPlayer.ShouldMuffleSound(controlled, worldPosition, far, (controlled2 != null) ? controlled2.CurrentHull : null);
								}
							}
						}
					}
				}
			}
			StatusEffect.ActiveLoopingSounds.RemoveWhere((StatusEffect s) => s.soundChannel == null);
		}

		// Token: 0x0600298A RID: 10634 RVA: 0x001CDE38 File Offset: 0x001CC038
		public static void StopAll()
		{
			CoroutineManager.StopCoroutines("statuseffect");
			DelayedEffect.DelayList.Clear();
			StatusEffect.DurationList.Clear();
		}

		// Token: 0x0600298B RID: 10635 RVA: 0x001CDE58 File Offset: 0x001CC058
		public void AddTag(Identifier tag)
		{
			if (this.statusEffectTags.Contains(tag))
			{
				return;
			}
			this.statusEffectTags.Add(tag);
		}

		// Token: 0x0600298C RID: 10636 RVA: 0x001CDE76 File Offset: 0x001CC076
		public bool HasTag(Identifier tag)
		{
			return tag == null || this.statusEffectTags.Contains(tag);
		}

		// Token: 0x0600298F RID: 10639 RVA: 0x001CDEF8 File Offset: 0x001CC0F8
		[CompilerGenerated]
		private bool <AddNearbyTargets>g__CheckDistance|139_0(ISpatialEntity e, ref StatusEffect.<>c__DisplayClass139_0 A_2)
		{
			float xDiff = Math.Abs(e.WorldPosition.X - A_2.worldPosition.X);
			if (xDiff > this.Range)
			{
				return false;
			}
			float yDiff = Math.Abs(e.WorldPosition.Y - A_2.worldPosition.Y);
			return yDiff <= this.Range && xDiff * xDiff + yDiff * yDiff < this.Range * this.Range;
		}

		// Token: 0x06002990 RID: 10640 RVA: 0x001CDF70 File Offset: 0x001CC170
		[CompilerGenerated]
		internal static bool <HasRequiredConditions>g__AnyTargetMatches|144_1(IReadOnlyList<ISerializableEntity> targets, string targetItemComponentName, PropertyConditional conditional)
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

		// Token: 0x06002991 RID: 10641 RVA: 0x001CDFC8 File Offset: 0x001CC1C8
		[CompilerGenerated]
		internal static ISerializableEntity <HasRequiredConditions>g__FindTargetItemOrComponent|144_2(IReadOnlyList<ISerializableEntity> targets)
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

		// Token: 0x06002993 RID: 10643 RVA: 0x001CE02C File Offset: 0x001CC22C
		[CompilerGenerated]
		internal static Identifier <Apply>g__GetRandomSkill|157_1(ref StatusEffect.<>c__DisplayClass157_1 A_0)
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

		// Token: 0x06002994 RID: 10644 RVA: 0x001CE0A4 File Offset: 0x001CC2A4
		[CompilerGenerated]
		internal static CharacterTeamType? <Apply>g__GetTeamFromSubmarine|157_7(MapEntity e, ref StatusEffect.<>c__DisplayClass157_6 A_1)
		{
			if (e.Submarine == null)
			{
				return null;
			}
			return new CharacterTeamType?((!A_1.isPvP && e.Submarine.Info.IsOutpost && e.Submarine.TeamID == CharacterTeamType.FriendlyNPC) ? CharacterTeamType.Team1 : e.Submarine.TeamID);
		}

		// Token: 0x04001520 RID: 5408
		private List<ParticleEmitter> particleEmitters;

		// Token: 0x04001521 RID: 5409
		private static readonly HashSet<StatusEffect> ActiveLoopingSounds = new HashSet<StatusEffect>();

		// Token: 0x04001522 RID: 5410
		private static double LastMuffleCheckTime;

		// Token: 0x04001523 RID: 5411
		private readonly List<RoundSound> sounds = new List<RoundSound>();

		// Token: 0x04001524 RID: 5412
		private SoundSelectionMode soundSelectionMode;

		// Token: 0x04001525 RID: 5413
		private SoundChannel soundChannel;

		// Token: 0x04001526 RID: 5414
		private Entity soundEmitter;

		// Token: 0x04001527 RID: 5415
		private double loopStartTime;

		// Token: 0x04001528 RID: 5416
		private bool loopSound;

		// Token: 0x04001529 RID: 5417
		private bool forcePlaySounds;

		// Token: 0x0400152A RID: 5418
		private CoroutineHandle playSoundAfterLoadedCoroutine;

		// Token: 0x0400152B RID: 5419
		private bool ignoreMuffling;

		// Token: 0x0400152C RID: 5420
		private RoundSound lastPlayingSound;

		// Token: 0x0400152D RID: 5421
		private static readonly ImmutableHashSet<Identifier> FieldNames;

		// Token: 0x0400152E RID: 5422
		private readonly StatusEffect.TargetType targetTypes;

		// Token: 0x0400152F RID: 5423
		public int TargetSlot = -1;

		// Token: 0x04001530 RID: 5424
		private readonly List<RelatedItem> requiredItems;

		// Token: 0x04001531 RID: 5425
		[TupleElementNames(new string[]
		{
			"propertyName",
			"value"
		})]
		public readonly ImmutableArray<ValueTuple<Identifier, object>> PropertyEffects;

		// Token: 0x04001532 RID: 5426
		private readonly PropertyConditional.LogicalOperatorType conditionalLogicalOperator = PropertyConditional.LogicalOperatorType.Or;

		// Token: 0x04001533 RID: 5427
		private readonly List<PropertyConditional> propertyConditionals;

		// Token: 0x04001534 RID: 5428
		private readonly bool setValue;

		// Token: 0x04001535 RID: 5429
		private readonly bool disableDeltaTime;

		// Token: 0x04001536 RID: 5430
		private readonly HashSet<Identifier> statusEffectTags;

		// Token: 0x04001537 RID: 5431
		private readonly float lifeTime;

		// Token: 0x04001538 RID: 5432
		private float lifeTimer;

		// Token: 0x04001539 RID: 5433
		private Dictionary<Entity, float> intervalTimers;

		// Token: 0x0400153A RID: 5434
		private readonly bool oneShot;

		// Token: 0x0400153B RID: 5435
		public static readonly List<DurationListElement> DurationList = new List<DurationListElement>();

		// Token: 0x0400153C RID: 5436
		public readonly bool CheckConditionalAlways;

		// Token: 0x0400153D RID: 5437
		public readonly bool Stackable;

		// Token: 0x0400153E RID: 5438
		public readonly bool ResetDurationWhenReapplied;

		// Token: 0x0400153F RID: 5439
		public readonly float Interval;

		// Token: 0x04001540 RID: 5440
		private readonly bool playSoundOnRequiredItemFailure;

		// Token: 0x04001541 RID: 5441
		private readonly StatusEffect.SteamTimeLineEvent steamTimeLineEventToTrigger;

		// Token: 0x04001542 RID: 5442
		private readonly int useItemCount;

		// Token: 0x04001543 RID: 5443
		private readonly bool removeItem;

		// Token: 0x04001544 RID: 5444
		private readonly bool dropContainedItems;

		// Token: 0x04001545 RID: 5445
		private readonly bool dropItem;

		// Token: 0x04001546 RID: 5446
		private readonly bool removeCharacter;

		// Token: 0x04001547 RID: 5447
		private readonly bool breakLimb;

		// Token: 0x04001548 RID: 5448
		private readonly bool hideLimb;

		// Token: 0x04001549 RID: 5449
		private readonly float hideLimbTimer;

		// Token: 0x0400154A RID: 5450
		private readonly Identifier containerForItemsOnCharacterRemoval;

		// Token: 0x0400154B RID: 5451
		public readonly ActionType type = ActionType.OnActive;

		// Token: 0x0400154C RID: 5452
		private readonly List<Explosion> explosions;

		// Token: 0x0400154D RID: 5453
		private readonly List<StatusEffect.ItemSpawnInfo> spawnItems;

		// Token: 0x0400154E RID: 5454
		private readonly Identifier forceSayIdentifier = Identifier.Empty;

		// Token: 0x0400154F RID: 5455
		private readonly bool forceSayInRadio;

		// Token: 0x04001550 RID: 5456
		private readonly bool spawnItemRandomly;

		// Token: 0x04001551 RID: 5457
		private readonly List<StatusEffect.CharacterSpawnInfo> spawnCharacters;

		// Token: 0x04001552 RID: 5458
		public readonly bool refundTalents;

		// Token: 0x04001553 RID: 5459
		public readonly List<StatusEffect.GiveTalentInfo> giveTalentInfos;

		// Token: 0x04001554 RID: 5460
		private readonly List<StatusEffect.AITrigger> aiTriggers;

		// Token: 0x04001555 RID: 5461
		private readonly List<EventPrefab> triggeredEvents;

		// Token: 0x04001556 RID: 5462
		private readonly Identifier triggeredEventTargetTag = "statuseffecttarget".ToIdentifier();

		// Token: 0x04001557 RID: 5463
		private readonly Identifier triggeredEventEntityTag = "statuseffectentity".ToIdentifier();

		// Token: 0x04001558 RID: 5464
		private readonly Identifier triggeredEventUserTag = "statuseffectuser".ToIdentifier();

		// Token: 0x04001559 RID: 5465
		[TupleElementNames(new string[]
		{
			"eventIdentifier",
			"tag"
		})]
		private readonly List<ValueTuple<Identifier, Identifier>> eventTargetTags;

		// Token: 0x0400155A RID: 5466
		public readonly ImmutableHashSet<Identifier> UnlockRecipes;

		// Token: 0x0400155B RID: 5467
		private Character user;

		// Token: 0x0400155C RID: 5468
		public readonly float FireSize;

		// Token: 0x0400155D RID: 5469
		public readonly LimbType[] targetLimbs;

		// Token: 0x0400155E RID: 5470
		public readonly float SeverLimbsProbability;

		// Token: 0x0400155F RID: 5471
		private readonly Vector2 randomCondition;

		// Token: 0x04001560 RID: 5472
		public PhysicsBody sourceBody;

		// Token: 0x04001561 RID: 5473
		public readonly bool OnlyInside;

		// Token: 0x04001562 RID: 5474
		public readonly bool OnlyOutside;

		// Token: 0x04001563 RID: 5475
		public readonly bool OnlyWhenDamagedByPlayer;

		// Token: 0x04001564 RID: 5476
		public readonly bool AllowWhenBroken;

		// Token: 0x04001565 RID: 5477
		public readonly ImmutableHashSet<Identifier> TargetIdentifiers;

		// Token: 0x04001566 RID: 5478
		public readonly string TargetItemComponent;

		// Token: 0x04001567 RID: 5479
		[TupleElementNames(new string[]
		{
			"affliction",
			"strength"
		})]
		private readonly HashSet<ValueTuple<Identifier, float>> requiredAfflictions;

		// Token: 0x04001568 RID: 5480
		public float AttackMultiplier = 1f;

		// Token: 0x0400156A RID: 5482
		private readonly bool multiplyAfflictionsByMaxVitality;

		// Token: 0x0400156B RID: 5483
		[TupleElementNames(new string[]
		{
			"AfflictionIdentifier",
			"ReduceAmount"
		})]
		public readonly List<ValueTuple<Identifier, float>> ReduceAffliction = new List<ValueTuple<Identifier, float>>();

		// Token: 0x0400156C RID: 5484
		public readonly bool CanGiveMedicalSkill;

		// Token: 0x0400156D RID: 5485
		private readonly List<Identifier> talentTriggers;

		// Token: 0x0400156E RID: 5486
		private readonly List<int> giveExperiences;

		// Token: 0x0400156F RID: 5487
		private readonly List<StatusEffect.GiveSkill> giveSkills;

		// Token: 0x04001570 RID: 5488
		private readonly List<ValueTuple<string, ContentXElement>> luaHook;

		// Token: 0x04001571 RID: 5489
		[TupleElementNames(new string[]
		{
			"targetCharacter",
			"anim"
		})]
		private HashSet<ValueTuple<Character, StatusEffect.AnimLoadInfo>> failedAnimations;

		// Token: 0x04001572 RID: 5490
		private readonly List<StatusEffect.AnimLoadInfo> animationsToTrigger;

		// Token: 0x04001573 RID: 5491
		public readonly float Duration;

		// Token: 0x04001579 RID: 5497
		private static readonly List<Entity> intervalsToRemove = new List<Entity>();

		// Token: 0x0400157A RID: 5498
		protected readonly List<ISerializableEntity> currentTargets = new List<ISerializableEntity>();

		// Token: 0x02000D89 RID: 3465
		[Flags]
		public enum TargetType
		{
			// Token: 0x04004FAC RID: 20396
			This = 1,
			// Token: 0x04004FAD RID: 20397
			Parent = 2,
			// Token: 0x04004FAE RID: 20398
			Character = 4,
			// Token: 0x04004FAF RID: 20399
			Contained = 8,
			// Token: 0x04004FB0 RID: 20400
			NearbyCharacters = 16,
			// Token: 0x04004FB1 RID: 20401
			NearbyItems = 32,
			// Token: 0x04004FB2 RID: 20402
			UseTarget = 64,
			// Token: 0x04004FB3 RID: 20403
			Hull = 128,
			// Token: 0x04004FB4 RID: 20404
			Limb = 256,
			// Token: 0x04004FB5 RID: 20405
			AllLimbs = 512,
			// Token: 0x04004FB6 RID: 20406
			LastLimb = 1024,
			// Token: 0x04004FB7 RID: 20407
			LinkedEntities = 2048
		}

		// Token: 0x02000D8A RID: 3466
		private class ItemSpawnInfo
		{
			// Token: 0x17001ADF RID: 6879
			// (get) Token: 0x0600815C RID: 33116 RVA: 0x00397FCA File Offset: 0x003961CA
			// (set) Token: 0x0600815D RID: 33117 RVA: 0x00397FD2 File Offset: 0x003961D2
			public bool InheritEventTags { get; private set; }

			// Token: 0x0600815E RID: 33118 RVA: 0x00397FDC File Offset: 0x003961DC
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

			// Token: 0x0600815F RID: 33119 RVA: 0x0039833D File Offset: 0x0039653D
			public int GetCount(Rand.RandSync randSync)
			{
				return Rand.Range(this.MinCount, this.MaxCount + 1, randSync);
			}

			// Token: 0x04004FB8 RID: 20408
			public readonly ItemPrefab ItemPrefab;

			// Token: 0x04004FB9 RID: 20409
			public readonly StatusEffect.ItemSpawnInfo.SpawnPositionType SpawnPosition;

			// Token: 0x04004FBA RID: 20410
			public readonly bool SpawnIfInventoryFull;

			// Token: 0x04004FBB RID: 20411
			public readonly bool SpawnIfNotInInventory;

			// Token: 0x04004FBC RID: 20412
			public readonly bool SpawnIfCantBeContained;

			// Token: 0x04004FBD RID: 20413
			public readonly float Impulse;

			// Token: 0x04004FBE RID: 20414
			public readonly float RotationRad;

			// Token: 0x04004FBF RID: 20415
			public readonly int MinCount;

			// Token: 0x04004FC0 RID: 20416
			public readonly int MaxCount;

			// Token: 0x04004FC1 RID: 20417
			public readonly float Probability;

			// Token: 0x04004FC2 RID: 20418
			public readonly float Spread;

			// Token: 0x04004FC3 RID: 20419
			public readonly StatusEffect.ItemSpawnInfo.SpawnRotationType RotationType;

			// Token: 0x04004FC4 RID: 20420
			public readonly float AimSpreadRad;

			// Token: 0x04004FC5 RID: 20421
			public readonly bool Equip;

			// Token: 0x04004FC6 RID: 20422
			public readonly float Condition;

			// Token: 0x0200154B RID: 5451
			public enum SpawnPositionType
			{
				// Token: 0x040067FE RID: 26622
				This,
				// Token: 0x040067FF RID: 26623
				ThisInventory,
				// Token: 0x04006800 RID: 26624
				SameInventory,
				// Token: 0x04006801 RID: 26625
				ContainedInventory,
				// Token: 0x04006802 RID: 26626
				Target
			}

			// Token: 0x0200154C RID: 5452
			public enum SpawnRotationType
			{
				// Token: 0x04006804 RID: 26628
				None,
				// Token: 0x04006805 RID: 26629
				This,
				// Token: 0x04006806 RID: 26630
				Target,
				// Token: 0x04006807 RID: 26631
				Limb,
				// Token: 0x04006808 RID: 26632
				MainLimb,
				// Token: 0x04006809 RID: 26633
				Collider,
				// Token: 0x0400680A RID: 26634
				Random
			}
		}

		// Token: 0x02000D8B RID: 3467
		public class AbilityStatusEffectIdentifier : AbilityObject
		{
			// Token: 0x06008160 RID: 33120 RVA: 0x00398353 File Offset: 0x00396553
			public AbilityStatusEffectIdentifier(Identifier effectIdentifier)
			{
				this.EffectIdentifier = effectIdentifier;
			}

			// Token: 0x17001AE0 RID: 6880
			// (get) Token: 0x06008161 RID: 33121 RVA: 0x00398362 File Offset: 0x00396562
			// (set) Token: 0x06008162 RID: 33122 RVA: 0x0039836A File Offset: 0x0039656A
			public Identifier EffectIdentifier { get; set; }
		}

		// Token: 0x02000D8C RID: 3468
		public class GiveTalentInfo
		{
			// Token: 0x06008163 RID: 33123 RVA: 0x00398373 File Offset: 0x00396573
			public GiveTalentInfo(XElement element, string _)
			{
				this.TalentIdentifiers = element.GetAttributeIdentifierArray("talentidentifiers", Array.Empty<Identifier>(), true);
				this.GiveRandom = element.GetAttributeBool("giverandom", false);
			}

			// Token: 0x04004FC9 RID: 20425
			public Identifier[] TalentIdentifiers;

			// Token: 0x04004FCA RID: 20426
			public bool GiveRandom;
		}

		// Token: 0x02000D8D RID: 3469
		public class GiveSkill
		{
			// Token: 0x06008164 RID: 33124 RVA: 0x003983A4 File Offset: 0x003965A4
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

			// Token: 0x04004FCB RID: 20427
			public readonly Identifier SkillIdentifier;

			// Token: 0x04004FCC RID: 20428
			public readonly float Amount;

			// Token: 0x04004FCD RID: 20429
			public readonly bool TriggerTalents;

			// Token: 0x04004FCE RID: 20430
			public readonly bool UseDeltaTime;

			// Token: 0x04004FCF RID: 20431
			public readonly bool Proportional;

			// Token: 0x04004FD0 RID: 20432
			public readonly bool AlwayShowNotification;
		}

		// Token: 0x02000D8E RID: 3470
		public class CharacterSpawnInfo : ISerializableEntity
		{
			// Token: 0x17001AE1 RID: 6881
			// (get) Token: 0x06008165 RID: 33125 RVA: 0x0039845C File Offset: 0x0039665C
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

			// Token: 0x17001AE2 RID: 6882
			// (get) Token: 0x06008166 RID: 33126 RVA: 0x0039849F File Offset: 0x0039669F
			// (set) Token: 0x06008167 RID: 33127 RVA: 0x003984A7 File Offset: 0x003966A7
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

			// Token: 0x17001AE3 RID: 6883
			// (get) Token: 0x06008168 RID: 33128 RVA: 0x003984B0 File Offset: 0x003966B0
			// (set) Token: 0x06008169 RID: 33129 RVA: 0x003984B8 File Offset: 0x003966B8
			[Serialize("", IsPropertySaveable.No, "The species name (identifier) of the character to spawn.", "", false)]
			public Identifier SpeciesName { get; private set; }

			// Token: 0x17001AE4 RID: 6884
			// (get) Token: 0x0600816A RID: 33130 RVA: 0x003984C1 File Offset: 0x003966C1
			// (set) Token: 0x0600816B RID: 33131 RVA: 0x003984C9 File Offset: 0x003966C9
			[Serialize(1, IsPropertySaveable.No, "How many characters to spawn.", "", false)]
			public int Count { get; private set; }

			// Token: 0x17001AE5 RID: 6885
			// (get) Token: 0x0600816C RID: 33132 RVA: 0x003984D2 File Offset: 0x003966D2
			// (set) Token: 0x0600816D RID: 33133 RVA: 0x003984DA File Offset: 0x003966DA
			[Serialize(false, IsPropertySaveable.No, "Should the buffs of the character executing the effect be transferred to the spawned character? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool TransferBuffs { get; private set; }

			// Token: 0x17001AE6 RID: 6886
			// (get) Token: 0x0600816E RID: 33134 RVA: 0x003984E3 File Offset: 0x003966E3
			// (set) Token: 0x0600816F RID: 33135 RVA: 0x003984EB File Offset: 0x003966EB
			[Serialize(false, IsPropertySaveable.No, "Should the afflictions of the character executing the effect be transferred to the spawned character? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool TransferAfflictions { get; private set; }

			// Token: 0x17001AE7 RID: 6887
			// (get) Token: 0x06008170 RID: 33136 RVA: 0x003984F4 File Offset: 0x003966F4
			// (set) Token: 0x06008171 RID: 33137 RVA: 0x003984FC File Offset: 0x003966FC
			[Serialize(false, IsPropertySaveable.No, "Should the the items from the character executing the effect be transferred to the spawned character? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool TransferInventory { get; private set; }

			// Token: 0x17001AE8 RID: 6888
			// (get) Token: 0x06008172 RID: 33138 RVA: 0x00398505 File Offset: 0x00396705
			// (set) Token: 0x06008173 RID: 33139 RVA: 0x0039850D File Offset: 0x0039670D
			[Serialize(0, IsPropertySaveable.No, "The maximum number of creatures of the given species and team that can exist per team in the current level before this status effect stops spawning any more.", "", false)]
			public int TotalMaxCount { get; private set; }

			// Token: 0x17001AE9 RID: 6889
			// (get) Token: 0x06008174 RID: 33140 RVA: 0x00398516 File Offset: 0x00396716
			// (set) Token: 0x06008175 RID: 33141 RVA: 0x0039851E File Offset: 0x0039671E
			[Serialize(0, IsPropertySaveable.No, "Amount of stun to apply on the spawned character.", "", false)]
			public int Stun { get; private set; }

			// Token: 0x17001AEA RID: 6890
			// (get) Token: 0x06008176 RID: 33142 RVA: 0x00398527 File Offset: 0x00396727
			// (set) Token: 0x06008177 RID: 33143 RVA: 0x0039852F File Offset: 0x0039672F
			[Serialize("", IsPropertySaveable.No, "An affliction to apply on the spawned character.", "", false)]
			public Identifier AfflictionOnSpawn { get; private set; }

			// Token: 0x17001AEB RID: 6891
			// (get) Token: 0x06008178 RID: 33144 RVA: 0x00398538 File Offset: 0x00396738
			// (set) Token: 0x06008179 RID: 33145 RVA: 0x00398540 File Offset: 0x00396740
			[Serialize(1, IsPropertySaveable.No, "The strength of the affliction applied on the spawned character. Only relevant if AfflictionOnSpawn is defined.", "", false)]
			public int AfflictionStrength { get; private set; }

			// Token: 0x17001AEC RID: 6892
			// (get) Token: 0x0600817A RID: 33146 RVA: 0x00398549 File Offset: 0x00396749
			// (set) Token: 0x0600817B RID: 33147 RVA: 0x00398551 File Offset: 0x00396751
			[Serialize(false, IsPropertySaveable.No, "Should the player controlling the character that executes the effect gain control of the spawned character? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool TransferControl { get; private set; }

			// Token: 0x17001AED RID: 6893
			// (get) Token: 0x0600817C RID: 33148 RVA: 0x0039855A File Offset: 0x0039675A
			// (set) Token: 0x0600817D RID: 33149 RVA: 0x00398562 File Offset: 0x00396762
			[Serialize(false, IsPropertySaveable.No, "Should the character that executes the effect be removed when the effect executes? Useful for effects that \"transform\" a character to something else by deleting the character and spawning a new one on its place.", "", false)]
			public bool RemovePreviousCharacter { get; private set; }

			// Token: 0x17001AEE RID: 6894
			// (get) Token: 0x0600817E RID: 33150 RVA: 0x0039856B File Offset: 0x0039676B
			// (set) Token: 0x0600817F RID: 33151 RVA: 0x00398573 File Offset: 0x00396773
			[Serialize(0f, IsPropertySaveable.No, "Amount of random spread to add to the spawn position. Can be used to prevent all the characters from spawning at the exact same position if the effect spawns multiple ones.", "", false)]
			public float Spread { get; private set; }

			// Token: 0x17001AEF RID: 6895
			// (get) Token: 0x06008180 RID: 33152 RVA: 0x0039857C File Offset: 0x0039677C
			// (set) Token: 0x06008181 RID: 33153 RVA: 0x00398584 File Offset: 0x00396784
			[Serialize("0,0", IsPropertySaveable.No, "Offset added to the spawn position. Can be used to for example spawn a character a bit up from the center of an item executing the effect.", "", false)]
			public Vector2 Offset { get; private set; }

			// Token: 0x17001AF0 RID: 6896
			// (get) Token: 0x06008182 RID: 33154 RVA: 0x0039858D File Offset: 0x0039678D
			// (set) Token: 0x06008183 RID: 33155 RVA: 0x00398595 File Offset: 0x00396795
			[Serialize(false, IsPropertySaveable.No, "", "", false)]
			public bool InheritEventTags { get; private set; }

			// Token: 0x17001AF1 RID: 6897
			// (get) Token: 0x06008184 RID: 33156 RVA: 0x0039859E File Offset: 0x0039679E
			// (set) Token: 0x06008185 RID: 33157 RVA: 0x003985A6 File Offset: 0x003967A6
			[Serialize(false, IsPropertySaveable.No, "Should the character team be inherited from the entity that owns the status effect?", "", false)]
			public bool InheritTeam { get; private set; }

			// Token: 0x06008186 RID: 33158 RVA: 0x003985B0 File Offset: 0x003967B0
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

		// Token: 0x02000D8F RID: 3471
		public class AITrigger : ISerializableEntity
		{
			// Token: 0x17001AF2 RID: 6898
			// (get) Token: 0x06008187 RID: 33159 RVA: 0x00398651 File Offset: 0x00396851
			public string Name
			{
				get
				{
					return "ai trigger";
				}
			}

			// Token: 0x17001AF3 RID: 6899
			// (get) Token: 0x06008188 RID: 33160 RVA: 0x00398658 File Offset: 0x00396858
			// (set) Token: 0x06008189 RID: 33161 RVA: 0x00398660 File Offset: 0x00396860
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

			// Token: 0x17001AF4 RID: 6900
			// (get) Token: 0x0600818A RID: 33162 RVA: 0x00398669 File Offset: 0x00396869
			// (set) Token: 0x0600818B RID: 33163 RVA: 0x00398671 File Offset: 0x00396871
			[Serialize(AIState.Idle, IsPropertySaveable.No, "The AI state the character should switch to.", "", false)]
			public AIState State { get; private set; }

			// Token: 0x17001AF5 RID: 6901
			// (get) Token: 0x0600818C RID: 33164 RVA: 0x0039867A File Offset: 0x0039687A
			// (set) Token: 0x0600818D RID: 33165 RVA: 0x00398682 File Offset: 0x00396882
			[Serialize(0f, IsPropertySaveable.No, "How long should the character stay in the specified state? If 0, the effect is permanent (unless overridden by another AITrigger).", "", false)]
			public float Duration { get; private set; }

			// Token: 0x17001AF6 RID: 6902
			// (get) Token: 0x0600818E RID: 33166 RVA: 0x0039868B File Offset: 0x0039688B
			// (set) Token: 0x0600818F RID: 33167 RVA: 0x00398693 File Offset: 0x00396893
			[Serialize(1f, IsPropertySaveable.No, "How likely is the AI to change the state when this effect executes? 1 = always, 0.5 = 50% chance, 0 = never.", "", false)]
			public float Probability { get; private set; }

			// Token: 0x17001AF7 RID: 6903
			// (get) Token: 0x06008190 RID: 33168 RVA: 0x0039869C File Offset: 0x0039689C
			// (set) Token: 0x06008191 RID: 33169 RVA: 0x003986A4 File Offset: 0x003968A4
			[Serialize(0f, IsPropertySaveable.No, "How much damage the character must receive for this AITrigger to become active? Checks the amount of damage the latest attack did to the character.", "", false)]
			public float MinDamage { get; private set; }

			// Token: 0x17001AF8 RID: 6904
			// (get) Token: 0x06008192 RID: 33170 RVA: 0x003986AD File Offset: 0x003968AD
			// (set) Token: 0x06008193 RID: 33171 RVA: 0x003986B5 File Offset: 0x003968B5
			[Serialize(true, IsPropertySaveable.No, "Can this AITrigger override other active AITriggers?", "", false)]
			public bool AllowToOverride { get; private set; }

			// Token: 0x17001AF9 RID: 6905
			// (get) Token: 0x06008194 RID: 33172 RVA: 0x003986BE File Offset: 0x003968BE
			// (set) Token: 0x06008195 RID: 33173 RVA: 0x003986C6 File Offset: 0x003968C6
			[Serialize(true, IsPropertySaveable.No, "Can this AITrigger be overridden by other AITriggers?", "", false)]
			public bool AllowToBeOverridden { get; private set; }

			// Token: 0x17001AFA RID: 6906
			// (get) Token: 0x06008196 RID: 33174 RVA: 0x003986CF File Offset: 0x003968CF
			// (set) Token: 0x06008197 RID: 33175 RVA: 0x003986D7 File Offset: 0x003968D7
			public bool IsTriggered { get; private set; }

			// Token: 0x17001AFB RID: 6907
			// (get) Token: 0x06008198 RID: 33176 RVA: 0x003986E0 File Offset: 0x003968E0
			// (set) Token: 0x06008199 RID: 33177 RVA: 0x003986E8 File Offset: 0x003968E8
			public float Timer { get; private set; }

			// Token: 0x17001AFC RID: 6908
			// (get) Token: 0x0600819A RID: 33178 RVA: 0x003986F1 File Offset: 0x003968F1
			// (set) Token: 0x0600819B RID: 33179 RVA: 0x003986F9 File Offset: 0x003968F9
			public bool IsActive { get; private set; }

			// Token: 0x17001AFD RID: 6909
			// (get) Token: 0x0600819C RID: 33180 RVA: 0x00398702 File Offset: 0x00396902
			// (set) Token: 0x0600819D RID: 33181 RVA: 0x0039870A File Offset: 0x0039690A
			public bool IsPermanent { get; private set; }

			// Token: 0x0600819E RID: 33182 RVA: 0x00398713 File Offset: 0x00396913
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

			// Token: 0x0600819F RID: 33183 RVA: 0x0039874D File Offset: 0x0039694D
			public void Reset()
			{
				this.IsTriggered = false;
				this.IsActive = false;
				this.Timer = 0f;
			}

			// Token: 0x060081A0 RID: 33184 RVA: 0x00398768 File Offset: 0x00396968
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

			// Token: 0x060081A1 RID: 33185 RVA: 0x003987A0 File Offset: 0x003969A0
			public AITrigger(XElement element)
			{
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			}
		}

		// Token: 0x02000D90 RID: 3472
		public readonly struct SteamTimeLineEvent : IEquatable<StatusEffect.SteamTimeLineEvent>
		{
			// Token: 0x060081A2 RID: 33186 RVA: 0x003987B5 File Offset: 0x003969B5
			public SteamTimeLineEvent(string title, string description, string icon)
			{
				this.title = title;
				this.description = description;
				this.icon = icon;
			}

			// Token: 0x17001AFE RID: 6910
			// (get) Token: 0x060081A3 RID: 33187 RVA: 0x003987CC File Offset: 0x003969CC
			// (set) Token: 0x060081A4 RID: 33188 RVA: 0x003987D4 File Offset: 0x003969D4
			public string title { get; set; }

			// Token: 0x17001AFF RID: 6911
			// (get) Token: 0x060081A5 RID: 33189 RVA: 0x003987DD File Offset: 0x003969DD
			// (set) Token: 0x060081A6 RID: 33190 RVA: 0x003987E5 File Offset: 0x003969E5
			public string description { get; set; }

			// Token: 0x17001B00 RID: 6912
			// (get) Token: 0x060081A7 RID: 33191 RVA: 0x003987EE File Offset: 0x003969EE
			// (set) Token: 0x060081A8 RID: 33192 RVA: 0x003987F6 File Offset: 0x003969F6
			public string icon { get; set; }

			// Token: 0x060081A9 RID: 33193 RVA: 0x00398800 File Offset: 0x00396A00
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("SteamTimeLineEvent");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060081AA RID: 33194 RVA: 0x0039884C File Offset: 0x00396A4C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("title = ");
				builder.Append(this.title);
				builder.Append(", description = ");
				builder.Append(this.description);
				builder.Append(", icon = ");
				builder.Append(this.icon);
				return true;
			}

			// Token: 0x060081AB RID: 33195 RVA: 0x003988A5 File Offset: 0x00396AA5
			[CompilerGenerated]
			public static bool operator !=(StatusEffect.SteamTimeLineEvent left, StatusEffect.SteamTimeLineEvent right)
			{
				return !(left == right);
			}

			// Token: 0x060081AC RID: 33196 RVA: 0x003988B1 File Offset: 0x00396AB1
			[CompilerGenerated]
			public static bool operator ==(StatusEffect.SteamTimeLineEvent left, StatusEffect.SteamTimeLineEvent right)
			{
				return left.Equals(right);
			}

			// Token: 0x060081AD RID: 33197 RVA: 0x003988BB File Offset: 0x00396ABB
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<string>.Default.GetHashCode(this.<title>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<description>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<icon>k__BackingField);
			}

			// Token: 0x060081AE RID: 33198 RVA: 0x003988FB File Offset: 0x00396AFB
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is StatusEffect.SteamTimeLineEvent && this.Equals((StatusEffect.SteamTimeLineEvent)obj);
			}

			// Token: 0x060081AF RID: 33199 RVA: 0x00398914 File Offset: 0x00396B14
			[CompilerGenerated]
			public bool Equals(StatusEffect.SteamTimeLineEvent other)
			{
				return EqualityComparer<string>.Default.Equals(this.<title>k__BackingField, other.<title>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<description>k__BackingField, other.<description>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<icon>k__BackingField, other.<icon>k__BackingField);
			}

			// Token: 0x060081B0 RID: 33200 RVA: 0x00398969 File Offset: 0x00396B69
			[CompilerGenerated]
			public void Deconstruct(out string title, out string description, out string icon)
			{
				title = this.title;
				description = this.description;
				icon = this.icon;
			}
		}

		// Token: 0x02000D91 RID: 3473
		public readonly struct AnimLoadInfo : IEquatable<StatusEffect.AnimLoadInfo>
		{
			// Token: 0x060081B1 RID: 33201 RVA: 0x00398983 File Offset: 0x00396B83
			public AnimLoadInfo(AnimationType Type, Either<string, ContentPath> File, float Priority, ImmutableArray<Identifier> ExpectedSpeciesNames)
			{
				this.Type = Type;
				this.File = File;
				this.Priority = Priority;
				this.ExpectedSpeciesNames = ExpectedSpeciesNames;
			}

			// Token: 0x17001B01 RID: 6913
			// (get) Token: 0x060081B2 RID: 33202 RVA: 0x003989A2 File Offset: 0x00396BA2
			// (set) Token: 0x060081B3 RID: 33203 RVA: 0x003989AA File Offset: 0x00396BAA
			public AnimationType Type { get; set; }

			// Token: 0x17001B02 RID: 6914
			// (get) Token: 0x060081B4 RID: 33204 RVA: 0x003989B3 File Offset: 0x00396BB3
			// (set) Token: 0x060081B5 RID: 33205 RVA: 0x003989BB File Offset: 0x00396BBB
			public Either<string, ContentPath> File { get; set; }

			// Token: 0x17001B03 RID: 6915
			// (get) Token: 0x060081B6 RID: 33206 RVA: 0x003989C4 File Offset: 0x00396BC4
			// (set) Token: 0x060081B7 RID: 33207 RVA: 0x003989CC File Offset: 0x00396BCC
			public float Priority { get; set; }

			// Token: 0x17001B04 RID: 6916
			// (get) Token: 0x060081B8 RID: 33208 RVA: 0x003989D5 File Offset: 0x00396BD5
			// (set) Token: 0x060081B9 RID: 33209 RVA: 0x003989DD File Offset: 0x00396BDD
			public ImmutableArray<Identifier> ExpectedSpeciesNames { get; set; }

			// Token: 0x060081BA RID: 33210 RVA: 0x003989E8 File Offset: 0x00396BE8
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

			// Token: 0x060081BB RID: 33211 RVA: 0x00398A34 File Offset: 0x00396C34
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

			// Token: 0x060081BC RID: 33212 RVA: 0x00398AD0 File Offset: 0x00396CD0
			[CompilerGenerated]
			public static bool operator !=(StatusEffect.AnimLoadInfo left, StatusEffect.AnimLoadInfo right)
			{
				return !(left == right);
			}

			// Token: 0x060081BD RID: 33213 RVA: 0x00398ADC File Offset: 0x00396CDC
			[CompilerGenerated]
			public static bool operator ==(StatusEffect.AnimLoadInfo left, StatusEffect.AnimLoadInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x060081BE RID: 33214 RVA: 0x00398AE8 File Offset: 0x00396CE8
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<AnimationType>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<Either<string, ContentPath>>.Default.GetHashCode(this.<File>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Priority>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<ExpectedSpeciesNames>k__BackingField);
			}

			// Token: 0x060081BF RID: 33215 RVA: 0x00398B4A File Offset: 0x00396D4A
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is StatusEffect.AnimLoadInfo && this.Equals((StatusEffect.AnimLoadInfo)obj);
			}

			// Token: 0x060081C0 RID: 33216 RVA: 0x00398B64 File Offset: 0x00396D64
			[CompilerGenerated]
			public bool Equals(StatusEffect.AnimLoadInfo other)
			{
				return EqualityComparer<AnimationType>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<Either<string, ContentPath>>.Default.Equals(this.<File>k__BackingField, other.<File>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Priority>k__BackingField, other.<Priority>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<ExpectedSpeciesNames>k__BackingField, other.<ExpectedSpeciesNames>k__BackingField);
			}

			// Token: 0x060081C1 RID: 33217 RVA: 0x00398BD1 File Offset: 0x00396DD1
			[CompilerGenerated]
			public void Deconstruct(out AnimationType Type, out Either<string, ContentPath> File, out float Priority, out ImmutableArray<Identifier> ExpectedSpeciesNames)
			{
				Type = this.Type;
				File = this.File;
				Priority = this.Priority;
				ExpectedSpeciesNames = this.ExpectedSpeciesNames;
			}
		}

		// Token: 0x02000D92 RID: 3474
		// (Invoke) Token: 0x060081C3 RID: 33219
		private delegate bool ShouldShortCircuit(bool condition, out bool valueToReturn);

		// Token: 0x02000D93 RID: 3475
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004FF3 RID: 20467
			public static StatusEffect.ShouldShortCircuit <0>__ShouldShortCircuitLogicalOrOperator;

			// Token: 0x04004FF4 RID: 20468
			public static StatusEffect.ShouldShortCircuit <1>__ShouldShortCircuitLogicalAndOperator;

			// Token: 0x04004FF5 RID: 20469
			public static Func<ISerializableEntity, Character> <2>__GetCharacterFromTarget;
		}
	}
}
