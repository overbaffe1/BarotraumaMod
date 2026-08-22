using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005E4 RID: 1508
	internal class Wearable : Pickable, IServerSerializable, INetSerializable
	{
		// Token: 0x060062D7 RID: 25303 RVA: 0x00336794 File Offset: 0x00334994
		private static void GetDamageModifierText(ref LocalizedString description, DamageModifier damageModifier, Identifier afflictionIdentifier)
		{
			int roundedValue = (int)Math.Round((double)((1f - Math.Min(damageModifier.DamageMultiplier, damageModifier.ProbabilityMultiplier)) * 100f));
			if (roundedValue == 0)
			{
				return;
			}
			string colorStr = GUIStyle.Green.ToStringHex();
			AfflictionPrefab afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab ap) => ap.Identifier == afflictionIdentifier);
			LocalizedString localizedString;
			if ((localizedString = ((afflictionPrefab != null) ? afflictionPrefab.Name : null)) == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("afflictiontype.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(afflictionIdentifier);
				localizedString = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(afflictionIdentifier.Value, true);
			}
			LocalizedString afflictionName = localizedString;
			if (!description.IsNullOrWhiteSpace())
			{
				description += '\n';
			}
			LocalizedString left = description;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 3);
			defaultInterpolatedStringHandler2.AppendLiteral("  ‖color:");
			defaultInterpolatedStringHandler2.AppendFormatted(colorStr);
			defaultInterpolatedStringHandler2.AppendLiteral("‖");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(roundedValue, "-0;+#");
			defaultInterpolatedStringHandler2.AppendLiteral("%‖color:end‖ ");
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(afflictionName);
			description = left + defaultInterpolatedStringHandler2.ToStringAndClear();
		}

		// Token: 0x060062D8 RID: 25304 RVA: 0x003368CC File Offset: 0x00334ACC
		public override void AddTooltipInfo(ref LocalizedString name, ref LocalizedString description)
		{
			Wearable.AddTooltipInfo(this.damageModifiers, this.SkillModifiers, ref description);
		}

		// Token: 0x060062D9 RID: 25305 RVA: 0x003368E0 File Offset: 0x00334AE0
		public static void AddTooltipInfo(IReadOnlyList<DamageModifier> damageModifiers, IReadOnlyDictionary<Identifier, float> skillModifiers, ref LocalizedString description)
		{
			if (damageModifiers.Any<DamageModifier>())
			{
				foreach (DamageModifier damageModifier in damageModifiers)
				{
					if (!MathUtils.NearlyEqual(damageModifier.DamageMultiplier * damageModifier.ProbabilityMultiplier, 1f, 0.0001f))
					{
						foreach (Identifier afflictionIdentifier in damageModifier.ParsedAfflictionIdentifiers)
						{
							Wearable.GetDamageModifierText(ref description, damageModifier, afflictionIdentifier);
						}
						foreach (Identifier afflictionType in damageModifier.ParsedAfflictionTypes)
						{
							Wearable.GetDamageModifierText(ref description, damageModifier, afflictionType);
						}
					}
				}
			}
			if (skillModifiers.Any<KeyValuePair<Identifier, float>>())
			{
				foreach (KeyValuePair<Identifier, float> skillModifier in skillModifiers)
				{
					string colorStr = GUIStyle.Green.ToStringHex();
					int roundedValue = (int)Math.Round((double)skillModifier.Value);
					if (roundedValue != 0)
					{
						if (!description.IsNullOrWhiteSpace())
						{
							description += '\n';
						}
						LocalizedString left = description;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
						defaultInterpolatedStringHandler.AppendLiteral("  ‖color:");
						defaultInterpolatedStringHandler.AppendFormatted(colorStr);
						defaultInterpolatedStringHandler.AppendLiteral("‖");
						defaultInterpolatedStringHandler.AppendFormatted(roundedValue.ToString("+0;-#"));
						defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖ ");
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("SkillName.");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(skillModifier.Key);
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()).Fallback(skillModifier.Key.Value, true));
						description = left + defaultInterpolatedStringHandler.ToStringAndClear();
					}
				}
			}
		}

		// Token: 0x17001901 RID: 6401
		// (get) Token: 0x060062DA RID: 25306 RVA: 0x00336AEC File Offset: 0x00334CEC
		public IEnumerable<DamageModifier> DamageModifiers
		{
			get
			{
				return this.damageModifiers;
			}
		}

		// Token: 0x17001902 RID: 6402
		// (get) Token: 0x060062DB RID: 25307 RVA: 0x00336AF4 File Offset: 0x00334CF4
		// (set) Token: 0x060062DC RID: 25308 RVA: 0x00336AFC File Offset: 0x00334CFC
		public bool AutoEquipWhenFull { get; private set; }

		// Token: 0x17001903 RID: 6403
		// (get) Token: 0x060062DD RID: 25309 RVA: 0x00336B05 File Offset: 0x00334D05
		// (set) Token: 0x060062DE RID: 25310 RVA: 0x00336B0D File Offset: 0x00334D0D
		public bool DisplayContainedStatus { get; private set; }

		// Token: 0x17001904 RID: 6404
		// (get) Token: 0x060062DF RID: 25311 RVA: 0x00336B16 File Offset: 0x00334D16
		// (set) Token: 0x060062E0 RID: 25312 RVA: 0x00336B1E File Offset: 0x00334D1E
		[Serialize(false, IsPropertySaveable.No, "Can the item be used (assuming it has components that are usable in some way) when worn.", "", false)]
		public bool AllowUseWhenWorn { get; set; }

		// Token: 0x17001905 RID: 6405
		// (get) Token: 0x060062E1 RID: 25313 RVA: 0x00336B27 File Offset: 0x00334D27
		// (set) Token: 0x060062E2 RID: 25314 RVA: 0x00336B30 File Offset: 0x00334D30
		public int Variant
		{
			get
			{
				return this.variant;
			}
			set
			{
				if (this.variant == value)
				{
					return;
				}
				Character character = this.picker;
				if (character != null)
				{
					this.Unequip(character);
				}
				for (int i = 0; i < this.wearableSprites.Length; i++)
				{
					ContentXElement subElement = this.wearableElements[i];
					WearableSprite wearableSprite = this.wearableSprites[i];
					if (wearableSprite != null)
					{
						Sprite sprite = wearableSprite.Sprite;
						if (sprite != null)
						{
							sprite.Remove();
						}
					}
					this.wearableSprites[i] = new WearableSprite(subElement, this, value);
				}
				if (character != null)
				{
					this.Equip(character);
				}
				this.variant = value;
			}
		}

		// Token: 0x060062E3 RID: 25315 RVA: 0x00336BB4 File Offset: 0x00334DB4
		public Wearable(Item item, ContentXElement element) : base(item, element)
		{
			this.item = item;
			this.damageModifiers = new List<DamageModifier>();
			this.SkillModifiers = new Dictionary<Identifier, float>();
			int spriteCount = element.Elements().Count((ContentXElement x) => x.Name.ToString().ToLowerInvariant() == "sprite");
			this.Variants = element.GetAttributeInt("variants", 0);
			this.variant = Rand.Range(1, this.Variants + 1, Rand.RandSync.ServerAndClient);
			this.wearableSprites = new WearableSprite[spriteCount];
			this.wearableElements = new ContentXElement[spriteCount];
			this.limbType = new LimbType[spriteCount];
			this.limb = new Limb[spriteCount];
			this.AutoEquipWhenFull = element.GetAttributeBool("autoequipwhenfull", true);
			this.DisplayContainedStatus = element.GetAttributeBool("displaycontainedstatus", false);
			int i = 0;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (!(a == "damagemodifier"))
					{
						if (!(a == "skillmodifier"))
						{
							if (!(a == "statvalue"))
							{
								if (a == "statuseffect")
								{
									if (subElement.GetAttributeString("Target", string.Empty).ToLowerInvariant().Contains("character"))
									{
										this.PressureProtection = Math.Max(subElement.GetAttributeFloat("PressureProtection", 0f), this.PressureProtection);
									}
								}
							}
							else
							{
								StatTypes statType = CharacterAbilityGroup.ParseStatType(subElement.GetAttributeString("stattype", ""), base.Name);
								float statValue = subElement.GetAttributeFloat("value", 0f);
								if (this.WearableStatValues.ContainsKey(statType))
								{
									Dictionary<StatTypes, float> wearableStatValues = this.WearableStatValues;
									StatTypes key = statType;
									wearableStatValues[key] += statValue;
								}
								else
								{
									this.WearableStatValues.TryAdd(statType, statValue);
								}
							}
						}
						else
						{
							Identifier skillIdentifier = subElement.GetAttributeIdentifier("skillidentifier", Identifier.Empty);
							float skillValue = subElement.GetAttributeFloat("skillvalue", 0f);
							if (this.SkillModifiers.ContainsKey(skillIdentifier))
							{
								Dictionary<Identifier, float> skillModifiers = this.SkillModifiers;
								Identifier key2 = skillIdentifier;
								skillModifiers[key2] += skillValue;
							}
							else
							{
								this.SkillModifiers.TryAdd(skillIdentifier, skillValue);
							}
						}
					}
					else
					{
						this.damageModifiers.Add(new DamageModifier(subElement, item.Name + ", Wearable", true));
					}
				}
				else
				{
					if (subElement.GetAttribute("texture") == null)
					{
						DebugConsole.ThrowError("Item \"" + item.Name + "\" doesn't have a texture specified!", null, element.ContentPackage, false, false);
						break;
					}
					this.limbType[i] = (LimbType)Enum.Parse(typeof(LimbType), subElement.GetAttributeString("limb", "Head"), true);
					this.wearableSprites[i] = new WearableSprite(subElement, this, this.variant);
					this.wearableElements[i] = subElement;
					foreach (ContentXElement lightElement in subElement.Elements())
					{
						if (lightElement.Name.ToString().Equals("lightcomponent", StringComparison.OrdinalIgnoreCase))
						{
							this.wearableSprites[i].LightComponents.Add(new LightComponent(item, lightElement)
							{
								Parent = this
							});
							foreach (LightComponent light in this.wearableSprites[i].LightComponents)
							{
								item.AddComponent(light);
							}
						}
					}
					i++;
				}
			}
		}

		// Token: 0x060062E4 RID: 25316 RVA: 0x00337008 File Offset: 0x00335208
		public override void Equip(Character character)
		{
			foreach (InvSlotType allowedSlot in this.allowedSlots)
			{
				if (allowedSlot != InvSlotType.Any)
				{
					foreach (object obj in Enum.GetValues(typeof(InvSlotType)))
					{
						Enum value = (Enum)obj;
						InvSlotType slotType = (InvSlotType)value;
						if (slotType != InvSlotType.Any && slotType != InvSlotType.None && allowedSlot.HasFlag(slotType) && !character.Inventory.IsInLimbSlot(this.item, slotType))
						{
							return;
						}
					}
				}
			}
			this.picker = character;
			for (int i = 0; i < this.wearableSprites.Length; i++)
			{
				WearableSprite wearableSprite = this.wearableSprites[i];
				if (!wearableSprite.IsInitialized)
				{
					wearableSprite.Init(this.picker);
				}
				wearableSprite.Picker = this.picker;
				Limb equipLimb = character.AnimController.GetLimb(this.limbType[i], true, false, false);
				if (equipLimb != null)
				{
					if (this.item.body != null)
					{
						this.item.body.Enabled = false;
					}
					this.IsActive = true;
					if (wearableSprite.LightComponent != null)
					{
						foreach (LightComponent light in wearableSprite.LightComponents)
						{
							light.ParentBody = equipLimb.body;
						}
					}
					this.limb[i] = equipLimb;
					if (!equipLimb.WearingItems.Contains(wearableSprite))
					{
						equipLimb.WearingItems.Add(wearableSprite);
						equipLimb.WearingItems.Sort(delegate(WearableSprite wearable, WearableSprite nextWearable)
						{
							float? num;
							if (wearable == null)
							{
								num = null;
							}
							else
							{
								Sprite sprite = wearable.Sprite;
								num = ((sprite != null) ? new float?(sprite.Depth) : null);
							}
							float? num2 = num;
							float depth = num2.GetValueOrDefault();
							float? num3;
							if (nextWearable == null)
							{
								num3 = null;
							}
							else
							{
								Sprite sprite2 = nextWearable.Sprite;
								num3 = ((sprite2 != null) ? new float?(sprite2.Depth) : null);
							}
							num2 = num3;
							return num2.GetValueOrDefault().CompareTo(depth);
						});
						equipLimb.WearingItems.Sort(delegate(WearableSprite wearable, WearableSprite nextWearable)
						{
							Wearable wearableComponent = (wearable != null) ? wearable.WearableComponent : null;
							Wearable nextWearableComponent = (nextWearable != null) ? nextWearable.WearableComponent : null;
							if (wearableComponent == null && nextWearableComponent == null)
							{
								return 0;
							}
							if (wearableComponent == null)
							{
								return -1;
							}
							if (nextWearableComponent == null)
							{
								return 1;
							}
							return wearableComponent.AllowedSlots.Contains(InvSlotType.OuterClothes).CompareTo(nextWearableComponent.AllowedSlots.Contains(InvSlotType.OuterClothes));
						});
					}
					equipLimb.UpdateWearableTypesToHide();
				}
			}
			character.OnWearablesChanged();
		}

		// Token: 0x060062E5 RID: 25317 RVA: 0x00337268 File Offset: 0x00335468
		public override void Drop(Character dropper, bool setTransform = true)
		{
			Character previousPicker = this.picker;
			this.Unequip(this.picker);
			base.Drop(dropper, setTransform);
			if (previousPicker != null)
			{
				previousPicker.OnWearablesChanged();
			}
			this.picker = null;
			this.IsActive = false;
		}

		// Token: 0x060062E6 RID: 25318 RVA: 0x003372A8 File Offset: 0x003354A8
		public override void Unequip(Character character)
		{
			if (character == null || character.Removed)
			{
				return;
			}
			if (this.picker == null)
			{
				return;
			}
			int i;
			Predicate<WearableSprite> <>9__0;
			int j;
			for (i = 0; i < this.wearableSprites.Length; i = j + 1)
			{
				Limb equipLimb = character.AnimController.GetLimb(this.limbType[i], true, false, false);
				if (equipLimb != null)
				{
					if (this.wearableSprites[i].LightComponent != null)
					{
						foreach (LightComponent light in this.wearableSprites[i].LightComponents)
						{
							light.ParentBody = null;
						}
					}
					List<WearableSprite> wearingItems = equipLimb.WearingItems;
					Predicate<WearableSprite> match;
					if ((match = <>9__0) == null)
					{
						match = (<>9__0 = ((WearableSprite w) => w != null && w == this.wearableSprites[i]));
					}
					wearingItems.RemoveAll(match);
					equipLimb.UpdateWearableTypesToHide();
					this.limb[i] = null;
				}
				j = i;
			}
			this.IsActive = false;
			base.StopSounds(ActionType.OnWearing);
		}

		// Token: 0x060062E7 RID: 25319 RVA: 0x003373E0 File Offset: 0x003355E0
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x060062E8 RID: 25320 RVA: 0x003373EC File Offset: 0x003355EC
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.picker == null || this.picker.Removed)
			{
				this.IsActive = false;
				return;
			}
			Holdable component = this.item.GetComponent<Holdable>();
			if (component == null || !component.IsActive)
			{
				this.item.SetTransform(this.picker.SimPosition, 0f, true, true, null);
			}
			this.item.ApplyStatusEffects(ActionType.OnWearing, deltaTime, this.picker, null, null, false, null);
			base.PlaySound(ActionType.OnWearing, this.picker);
		}

		// Token: 0x060062E9 RID: 25321 RVA: 0x00337478 File Offset: 0x00335678
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.Unequip(this.picker);
			foreach (WearableSprite wearableSprite in this.wearableSprites)
			{
				wearableSprite.Remove();
			}
		}

		// Token: 0x060062EA RID: 25322 RVA: 0x003374B8 File Offset: 0x003356B8
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			componentElement.Add(new XAttribute("variant", this.variant));
			return componentElement;
		}

		// Token: 0x060062EB RID: 25323 RVA: 0x003374EE File Offset: 0x003356EE
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.loadedVariant = componentElement.GetAttributeInt("variant", -1);
		}

		// Token: 0x060062EC RID: 25324 RVA: 0x0033750D File Offset: 0x0033570D
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			if (this.loadedVariant > 0 && this.loadedVariant < this.Variants + 1)
			{
				this.Variant = this.loadedVariant;
			}
		}

		// Token: 0x060062ED RID: 25325 RVA: 0x0033753A File Offset: 0x0033573A
		public override void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteByte((byte)this.Variant);
			base.ServerEventWrite(msg, c, extraData);
		}

		// Token: 0x060062EE RID: 25326 RVA: 0x00337552 File Offset: 0x00335752
		public override void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.Variant = (int)msg.ReadByte();
			base.ClientEventRead(msg, sendingTime);
		}

		// Token: 0x0400331C RID: 13084
		private readonly ContentXElement[] wearableElements;

		// Token: 0x0400331D RID: 13085
		private readonly WearableSprite[] wearableSprites;

		// Token: 0x0400331E RID: 13086
		private readonly LimbType[] limbType;

		// Token: 0x0400331F RID: 13087
		private readonly Limb[] limb;

		// Token: 0x04003320 RID: 13088
		private readonly List<DamageModifier> damageModifiers;

		// Token: 0x04003321 RID: 13089
		public readonly Dictionary<Identifier, float> SkillModifiers;

		// Token: 0x04003322 RID: 13090
		public readonly Dictionary<StatTypes, float> WearableStatValues = new Dictionary<StatTypes, float>();

		// Token: 0x04003326 RID: 13094
		public readonly int Variants;

		// Token: 0x04003327 RID: 13095
		private int variant;

		// Token: 0x04003328 RID: 13096
		public readonly float PressureProtection;

		// Token: 0x04003329 RID: 13097
		private int loadedVariant = -1;
	}
}
