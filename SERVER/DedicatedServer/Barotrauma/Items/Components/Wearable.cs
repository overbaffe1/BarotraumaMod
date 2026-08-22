using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000505 RID: 1285
	internal class Wearable : Pickable, IServerSerializable, INetSerializable
	{
		// Token: 0x1700137F RID: 4991
		// (get) Token: 0x06004868 RID: 18536 RVA: 0x001CD54A File Offset: 0x001CB74A
		public IEnumerable<DamageModifier> DamageModifiers
		{
			get
			{
				return this.damageModifiers;
			}
		}

		// Token: 0x17001380 RID: 4992
		// (get) Token: 0x06004869 RID: 18537 RVA: 0x001CD552 File Offset: 0x001CB752
		// (set) Token: 0x0600486A RID: 18538 RVA: 0x001CD55A File Offset: 0x001CB75A
		public bool AutoEquipWhenFull { get; private set; }

		// Token: 0x17001381 RID: 4993
		// (get) Token: 0x0600486B RID: 18539 RVA: 0x001CD563 File Offset: 0x001CB763
		// (set) Token: 0x0600486C RID: 18540 RVA: 0x001CD56B File Offset: 0x001CB76B
		public bool DisplayContainedStatus { get; private set; }

		// Token: 0x17001382 RID: 4994
		// (get) Token: 0x0600486D RID: 18541 RVA: 0x001CD574 File Offset: 0x001CB774
		// (set) Token: 0x0600486E RID: 18542 RVA: 0x001CD57C File Offset: 0x001CB77C
		[Serialize(false, IsPropertySaveable.No, "Can the item be used (assuming it has components that are usable in some way) when worn.", "", false)]
		public bool AllowUseWhenWorn { get; set; }

		// Token: 0x17001383 RID: 4995
		// (get) Token: 0x0600486F RID: 18543 RVA: 0x001CD585 File Offset: 0x001CB785
		// (set) Token: 0x06004870 RID: 18544 RVA: 0x001CD58D File Offset: 0x001CB78D
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
				this.variant = value;
				Submarine submarine = this.item.Submarine;
				if (submarine == null || !submarine.Loading)
				{
					this.item.CreateServerEvent<Wearable>(this);
				}
			}
		}

		// Token: 0x06004871 RID: 18545 RVA: 0x001CD5C8 File Offset: 0x001CB7C8
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

		// Token: 0x06004872 RID: 18546 RVA: 0x001CDA1C File Offset: 0x001CBC1C
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
				}
			}
			character.OnWearablesChanged();
		}

		// Token: 0x06004873 RID: 18547 RVA: 0x001CDC74 File Offset: 0x001CBE74
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

		// Token: 0x06004874 RID: 18548 RVA: 0x001CDCB4 File Offset: 0x001CBEB4
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
					this.limb[i] = null;
				}
				j = i;
			}
			this.IsActive = false;
		}

		// Token: 0x06004875 RID: 18549 RVA: 0x001CDDE0 File Offset: 0x001CBFE0
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x06004876 RID: 18550 RVA: 0x001CDDEC File Offset: 0x001CBFEC
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
		}

		// Token: 0x06004877 RID: 18551 RVA: 0x001CDE6C File Offset: 0x001CC06C
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.Unequip(this.picker);
			foreach (WearableSprite wearableSprite in this.wearableSprites)
			{
				wearableSprite.Remove();
			}
		}

		// Token: 0x06004878 RID: 18552 RVA: 0x001CDEAC File Offset: 0x001CC0AC
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			componentElement.Add(new XAttribute("variant", this.variant));
			return componentElement;
		}

		// Token: 0x06004879 RID: 18553 RVA: 0x001CDEE2 File Offset: 0x001CC0E2
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.loadedVariant = componentElement.GetAttributeInt("variant", -1);
		}

		// Token: 0x0600487A RID: 18554 RVA: 0x001CDF01 File Offset: 0x001CC101
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			if (this.loadedVariant > 0 && this.loadedVariant < this.Variants + 1)
			{
				this.Variant = this.loadedVariant;
			}
		}

		// Token: 0x0600487B RID: 18555 RVA: 0x001CDF2E File Offset: 0x001CC12E
		public override void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteByte((byte)this.Variant);
			base.ServerEventWrite(msg, c, extraData);
		}

		// Token: 0x0600487C RID: 18556 RVA: 0x001CDF46 File Offset: 0x001CC146
		public override void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.Variant = (int)msg.ReadByte();
			base.ClientEventRead(msg, sendingTime);
		}

		// Token: 0x04002325 RID: 8997
		private readonly ContentXElement[] wearableElements;

		// Token: 0x04002326 RID: 8998
		private readonly WearableSprite[] wearableSprites;

		// Token: 0x04002327 RID: 8999
		private readonly LimbType[] limbType;

		// Token: 0x04002328 RID: 9000
		private readonly Limb[] limb;

		// Token: 0x04002329 RID: 9001
		private readonly List<DamageModifier> damageModifiers;

		// Token: 0x0400232A RID: 9002
		public readonly Dictionary<Identifier, float> SkillModifiers;

		// Token: 0x0400232B RID: 9003
		public readonly Dictionary<StatTypes, float> WearableStatValues = new Dictionary<StatTypes, float>();

		// Token: 0x0400232F RID: 9007
		public readonly int Variants;

		// Token: 0x04002330 RID: 9008
		private int variant;

		// Token: 0x04002331 RID: 9009
		public readonly float PressureProtection;

		// Token: 0x04002332 RID: 9010
		private int loadedVariant = -1;
	}
}
