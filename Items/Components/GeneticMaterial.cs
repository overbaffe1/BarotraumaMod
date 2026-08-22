using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005AA RID: 1450
	internal class GeneticMaterial : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x17001606 RID: 5638
		// (get) Token: 0x06005845 RID: 22597 RVA: 0x002DA471 File Offset: 0x002D8671
		// (set) Token: 0x06005846 RID: 22598 RVA: 0x002DA479 File Offset: 0x002D8679
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float TooltipValueMin { get; set; }

		// Token: 0x17001607 RID: 5639
		// (get) Token: 0x06005847 RID: 22599 RVA: 0x002DA482 File Offset: 0x002D8682
		// (set) Token: 0x06005848 RID: 22600 RVA: 0x002DA48A File Offset: 0x002D868A
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float TooltipValueMax { get; set; }

		// Token: 0x06005849 RID: 22601 RVA: 0x002DA494 File Offset: 0x002D8694
		public override void AddTooltipInfo(ref LocalizedString name, ref LocalizedString description)
		{
			bool mergedMaterialTainted = false;
			if (!this.materialName.IsNullOrEmpty() && this.item.ContainedItems.Count<Item>() > 0)
			{
				LocalizedString mergedMaterialName = this.materialName;
				foreach (Item containedItem in this.item.ContainedItems)
				{
					GeneticMaterial containedMaterial = containedItem.GetComponent<GeneticMaterial>();
					if (containedMaterial != null)
					{
						mergedMaterialName += ", " + containedMaterial.materialName;
						if (containedMaterial.Tainted)
						{
							mergedMaterialTainted = true;
						}
					}
				}
				name = name.Replace(this.materialName, mergedMaterialName, StringComparison.Ordinal);
			}
			if (this.Tainted || mergedMaterialTainted)
			{
				name = TextManager.GetWithVariable("entityname.taintedgeneticmaterial", "[geneticmaterialname]", name, FormatCapitals.No);
			}
			if (TextManager.ContainsTag("entitydescription." + base.Item.Prefab.Identifier.ToString()))
			{
				int value = (int)MathHelper.Lerp(this.TooltipValueMin, this.TooltipValueMax, this.item.ConditionPercentage / 100f);
				description = TextManager.GetWithVariable("entitydescription." + base.Item.Prefab.Identifier.ToString(), "[value]", value.ToString(), FormatCapitals.No);
			}
			foreach (Item containedItem2 in this.item.ContainedItems)
			{
				GeneticMaterial containedGeneticMaterial = containedItem2.GetComponent<GeneticMaterial>();
				if (containedGeneticMaterial != null)
				{
					LocalizedString _ = string.Empty;
					LocalizedString containedDescription = containedItem2.Description;
					containedGeneticMaterial.AddTooltipInfo(ref _, ref containedDescription);
					if (!containedDescription.IsNullOrEmpty())
					{
						description += '\n' + containedDescription;
					}
				}
			}
			if (GameMain.DevMode && this.Tainted && this.selectedTaintedEffect != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 3);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(description);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.selectedTaintedEffect.Name);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.selectedTaintedEffect.GetDescription(0f, AfflictionPrefab.Description.TargetType.OtherCharacter));
				description = defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x0600584A RID: 22602 RVA: 0x002DA708 File Offset: 0x002D8908
		public void ModifyDeconstructInfo(Deconstructor deconstructor, ref LocalizedString buttonText, ref LocalizedString infoText)
		{
			if (deconstructor.InputContainer.Inventory.AllItems.Count<Item>() == 2)
			{
				Item otherItem = deconstructor.InputContainer.Inventory.AllItems.FirstOrDefault((Item it) => it != this.item);
				if (otherItem == null)
				{
					return;
				}
				GeneticMaterial otherGeneticMaterial = otherItem.GetComponent<GeneticMaterial>();
				if (otherGeneticMaterial == null)
				{
					return;
				}
				GeneticMaterial.CombineResult combineRefineResult = this.GetCombineRefineResult(otherGeneticMaterial);
				if (combineRefineResult == GeneticMaterial.CombineResult.None)
				{
					infoText = TextManager.Get("researchstation.novalidcombination");
					return;
				}
				if (combineRefineResult == GeneticMaterial.CombineResult.Refined)
				{
					buttonText = TextManager.Get("researchstation.refine");
					infoText = TextManager.GetWithVariable("researchstation.refine.infotext", "[taintedprobability]", ((int)(this.GetTaintedProbabilityOnRefine(otherGeneticMaterial, Character.Controlled) * 100f)).ToString(), FormatCapitals.No);
					return;
				}
				buttonText = TextManager.Get("researchstation.combine");
				infoText = TextManager.Get("researchstation.combine.infotext");
			}
		}

		// Token: 0x0600584B RID: 22603 RVA: 0x002DA7D4 File Offset: 0x002D89D4
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.Tainted = msg.ReadBoolean();
			if (this.Tainted)
			{
				uint selectedTaintedEffectId = msg.ReadUInt32();
				this.selectedTaintedEffect = AfflictionPrefab.Prefabs.Find((AfflictionPrefab a) => a.UintIdentifier == selectedTaintedEffectId);
				return;
			}
			uint selectedEffectId = msg.ReadUInt32();
			this.selectedEffect = AfflictionPrefab.Prefabs.Find((AfflictionPrefab a) => a.UintIdentifier == selectedEffectId);
		}

		// Token: 0x17001608 RID: 5640
		// (get) Token: 0x0600584C RID: 22604 RVA: 0x002DA852 File Offset: 0x002D8A52
		// (set) Token: 0x0600584D RID: 22605 RVA: 0x002DA85A File Offset: 0x002D8A5A
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string Effect { get; set; }

		// Token: 0x17001609 RID: 5641
		// (get) Token: 0x0600584E RID: 22606 RVA: 0x002DA863 File Offset: 0x002D8A63
		// (set) Token: 0x0600584F RID: 22607 RVA: 0x002DA86B File Offset: 0x002D8A6B
		[Serialize("geneticmaterialdebuff", IsPropertySaveable.Yes, "Either the identifier or the type for the tainted effect prefab", "", false)]
		public Identifier TaintedEffect { get; set; }

		// Token: 0x1700160A RID: 5642
		// (get) Token: 0x06005850 RID: 22608 RVA: 0x002DA874 File Offset: 0x002D8A74
		// (set) Token: 0x06005851 RID: 22609 RVA: 0x002DA87C File Offset: 0x002D8A7C
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool Tainted
		{
			get
			{
				return this.tainted;
			}
			set
			{
				this.tainted = value;
				if (this.tainted)
				{
					if (!this.TaintedEffect.IsEmpty)
					{
						this.selectedTaintedEffect = AfflictionPrefab.Prefabs.Where(delegate(AfflictionPrefab a)
						{
							Identifier taintedEffect = this.TaintedEffect;
							if (!(a.Identifier == taintedEffect))
							{
								Identifier taintedEffect2 = this.TaintedEffect;
								return a.AfflictionType == taintedEffect2;
							}
							return true;
						}).GetRandomUnsynced<AfflictionPrefab>();
						return;
					}
				}
				else
				{
					if (this.targetCharacter != null)
					{
						Affliction affliction = this.targetCharacter.CharacterHealth.GetAllAfflictions().FirstOrDefault((Affliction a) => a.Prefab == this.selectedEffect);
						if (affliction != null)
						{
							affliction.Strength = 0f;
						}
					}
					this.selectedTaintedEffect = null;
				}
			}
		}

		// Token: 0x1700160B RID: 5643
		// (get) Token: 0x06005852 RID: 22610 RVA: 0x002DA909 File Offset: 0x002D8B09
		// (set) Token: 0x06005853 RID: 22611 RVA: 0x002DA911 File Offset: 0x002D8B11
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool SetTaintedOnDeath { get; private set; }

		// Token: 0x1700160C RID: 5644
		// (get) Token: 0x06005854 RID: 22612 RVA: 0x002DA91A File Offset: 0x002D8B1A
		// (set) Token: 0x06005855 RID: 22613 RVA: 0x002DA922 File Offset: 0x002D8B22
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool CanBeUntainted { get; private set; }

		// Token: 0x1700160D RID: 5645
		// (get) Token: 0x06005856 RID: 22614 RVA: 0x002DA92B File Offset: 0x002D8B2B
		// (set) Token: 0x06005857 RID: 22615 RVA: 0x002DA944 File Offset: 0x002D8B44
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier SelectedTaintedEffect
		{
			get
			{
				AfflictionPrefab afflictionPrefab = this.selectedTaintedEffect;
				if (afflictionPrefab == null)
				{
					return Identifier.Empty;
				}
				return afflictionPrefab.Identifier;
			}
			private set
			{
				this.selectedTaintedEffect = ((!value.IsEmpty) ? AfflictionPrefab.Prefabs.Find((AfflictionPrefab a) => a.Identifier == value) : null);
			}
		}

		// Token: 0x06005858 RID: 22616 RVA: 0x002DA98C File Offset: 0x002D8B8C
		public GeneticMaterial(Item item, ContentXElement element) : base(item, element)
		{
			string nameId = element.GetAttributeString("nameidentifier", "");
			if (!string.IsNullOrEmpty(nameId))
			{
				this.materialName = TextManager.Get(nameId);
			}
			if (!string.IsNullOrEmpty(this.Effect))
			{
				this.selectedEffect = (from a in AfflictionPrefab.Prefabs
				where a.Identifier == this.Effect || a.AfflictionType == this.Effect
				select a).GetRandomUnsynced<AfflictionPrefab>();
			}
		}

		// Token: 0x1700160E RID: 5646
		// (get) Token: 0x06005859 RID: 22617 RVA: 0x002DA9F4 File Offset: 0x002D8BF4
		// (set) Token: 0x0600585A RID: 22618 RVA: 0x002DA9FC File Offset: 0x002D8BFC
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float ConditionIncreaseOnCombineMin { get; set; }

		// Token: 0x1700160F RID: 5647
		// (get) Token: 0x0600585B RID: 22619 RVA: 0x002DAA05 File Offset: 0x002D8C05
		// (set) Token: 0x0600585C RID: 22620 RVA: 0x002DAA0D File Offset: 0x002D8C0D
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float ConditionIncreaseOnCombineMax { get; set; }

		// Token: 0x17001610 RID: 5648
		// (get) Token: 0x0600585D RID: 22621 RVA: 0x002DAA16 File Offset: 0x002D8C16
		// (set) Token: 0x0600585E RID: 22622 RVA: 0x002DAA1E File Offset: 0x002D8C1E
		[Serialize(3f, IsPropertySaveable.No, "When refining, min value for condition increase bonus based on the quality of the worse gene.", "", false)]
		public float ConditionIncreaseOnLowQualityCombine { get; set; }

		// Token: 0x17001611 RID: 5649
		// (get) Token: 0x0600585F RID: 22623 RVA: 0x002DAA27 File Offset: 0x002D8C27
		// (set) Token: 0x06005860 RID: 22624 RVA: 0x002DAA2F File Offset: 0x002D8C2F
		[Serialize(25f, IsPropertySaveable.No, "When refining, max value for condition increase bonus based on the quality of the worse gene.", "", false)]
		public float ConditionIncreaseOnHighQualityCombine { get; set; }

		// Token: 0x06005861 RID: 22625 RVA: 0x002DAA38 File Offset: 0x002D8C38
		private bool SharesTypeWith(GeneticMaterial otherGeneticMaterial)
		{
			return this.GetSharedTypeOrDefault(otherGeneticMaterial) != null;
		}

		// Token: 0x06005862 RID: 22626 RVA: 0x002DAA44 File Offset: 0x002D8C44
		private ItemPrefab GetSharedTypeOrDefault(GeneticMaterial otherGeneticMaterial)
		{
			if (otherGeneticMaterial == null)
			{
				return null;
			}
			return this.AllMaterialTypes.FirstOrDefault((ItemPrefab materialType) => otherGeneticMaterial.AllMaterialTypes.Contains(materialType));
		}

		// Token: 0x17001612 RID: 5650
		// (get) Token: 0x06005863 RID: 22627 RVA: 0x002DAA80 File Offset: 0x002D8C80
		private IEnumerable<ItemPrefab> AllMaterialTypes
		{
			get
			{
				GeneticMaterial.<get_AllMaterialTypes>d__58 <get_AllMaterialTypes>d__ = new GeneticMaterial.<get_AllMaterialTypes>d__58(-2);
				<get_AllMaterialTypes>d__.<>4__this = this;
				return <get_AllMaterialTypes>d__;
			}
		}

		// Token: 0x17001613 RID: 5651
		// (get) Token: 0x06005864 RID: 22628 RVA: 0x002DAAA0 File Offset: 0x002D8CA0
		private GeneticMaterial NestedMaterial
		{
			get
			{
				if (this.item == null || this.item.OwnInventory == null)
				{
					return null;
				}
				Item nestedItemWithGeneticMaterial = this.item.OwnInventory.AllItems.FirstOrDefault((Item it) => it.GetComponent<GeneticMaterial>() != null);
				if (nestedItemWithGeneticMaterial == null)
				{
					return null;
				}
				return nestedItemWithGeneticMaterial.GetComponent<GeneticMaterial>();
			}
		}

		// Token: 0x17001614 RID: 5652
		// (get) Token: 0x06005865 RID: 22629 RVA: 0x002DAB04 File Offset: 0x002D8D04
		private bool IsCombined
		{
			get
			{
				if (this.NestedMaterial != null)
				{
					return true;
				}
				if (this.item.ParentInventory != null)
				{
					Item parentItem = this.item.ParentInventory.Owner as Item;
					if (parentItem != null)
					{
						GeneticMaterial component = parentItem.GetComponent<GeneticMaterial>();
						if (((component != null) ? component.NestedMaterial : null) == this)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		// Token: 0x06005866 RID: 22630 RVA: 0x002DAB5C File Offset: 0x002D8D5C
		private GeneticMaterial.CombineResult GetCombineRefineResult(GeneticMaterial otherGeneticMaterial)
		{
			if (otherGeneticMaterial == null)
			{
				return GeneticMaterial.CombineResult.None;
			}
			if (this.IsCombined && otherGeneticMaterial.IsCombined)
			{
				return GeneticMaterial.CombineResult.None;
			}
			if (!this.IsCombined && !otherGeneticMaterial.IsCombined)
			{
				if (!this.SharesTypeWith(otherGeneticMaterial))
				{
					return GeneticMaterial.CombineResult.Combined;
				}
				return GeneticMaterial.CombineResult.Refined;
			}
			else
			{
				if (!this.SharesTypeWith(otherGeneticMaterial))
				{
					return GeneticMaterial.CombineResult.None;
				}
				return GeneticMaterial.CombineResult.Refined;
			}
		}

		// Token: 0x06005867 RID: 22631 RVA: 0x002DABA9 File Offset: 0x002D8DA9
		public bool CanBeCombinedWith(GeneticMaterial otherGeneticMaterial)
		{
			return this.GetCombineRefineResult(otherGeneticMaterial) > GeneticMaterial.CombineResult.None;
		}

		// Token: 0x06005868 RID: 22632 RVA: 0x002DABB8 File Offset: 0x002D8DB8
		public override void Equip(Character character)
		{
			if (character == null)
			{
				return;
			}
			this.IsActive = true;
			if (this.targetCharacter != null)
			{
				return;
			}
			if (this.selectedEffect != null)
			{
				this.targetCharacter = character;
				base.ApplyStatusEffects(ActionType.OnWearing, 1f, this.targetCharacter, null, null, null, null, 1f);
				float selectedEffectStrength = this.GetCombinedEffectStrength();
				character.CharacterHealth.ApplyAffliction(null, this.selectedEffect.Instantiate(selectedEffectStrength, null), true, false, true);
				Affliction affliction = character.CharacterHealth.GetAllAfflictions().FirstOrDefault((Affliction a) => a.Prefab == this.selectedEffect);
				if (affliction != null)
				{
					affliction.Strength = selectedEffectStrength;
					affliction.SetStrength(selectedEffectStrength);
				}
			}
			if (this.tainted && this.selectedTaintedEffect != null)
			{
				float selectedTaintedEffectStrength = this.GetCombinedTaintedEffectStrength();
				character.CharacterHealth.ApplyAffliction(null, this.selectedTaintedEffect.Instantiate(selectedTaintedEffectStrength, null), true, false, true);
				Affliction affliction2 = character.CharacterHealth.GetAllAfflictions().FirstOrDefault((Affliction a) => a.Prefab == this.selectedTaintedEffect);
				if (affliction2 != null)
				{
					affliction2.Strength = selectedTaintedEffectStrength;
					affliction2.SetStrength(selectedTaintedEffectStrength);
				}
				this.targetCharacter = character;
			}
			foreach (Item containedItem in this.item.ContainedItems)
			{
				GeneticMaterial component = containedItem.GetComponent<GeneticMaterial>();
				if (component != null)
				{
					component.Equip(character);
				}
			}
		}

		// Token: 0x06005869 RID: 22633 RVA: 0x002DAD20 File Offset: 0x002D8F20
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			if (this.targetCharacter != null)
			{
				if (this.SetTaintedOnDeath && !this.tainted && this.targetCharacter.IsDead)
				{
					CauseOfDeath causeOfDeath = this.targetCharacter.CauseOfDeath;
					if (causeOfDeath == null || causeOfDeath.Type != CauseOfDeathType.Disconnected)
					{
						this.SetTainted(true, false);
					}
				}
				Item rootContainer = this.item.RootContainer;
				if (!this.targetCharacter.HasEquippedItem(this.item, null, null) && (rootContainer == null || !this.targetCharacter.HasEquippedItem(rootContainer, null, null) || !this.targetCharacter.Inventory.IsInLimbSlot(rootContainer, InvSlotType.HealthInterface)))
				{
					Character prevTargetCharacter = this.targetCharacter;
					this.Deactivate();
					if (rootContainer != null)
					{
						foreach (Item otherItem in rootContainer.ContainedItems)
						{
							if (otherItem != this.item)
							{
								GeneticMaterial otherGeneticMaterial = otherItem.GetComponent<GeneticMaterial>();
								if (otherGeneticMaterial != null && otherGeneticMaterial.IsActive)
								{
									otherGeneticMaterial.Deactivate();
								}
							}
						}
					}
					this.item.ApplyStatusEffects(ActionType.OnSevered, 1f, prevTargetCharacter, null, null, false, null);
				}
			}
		}

		// Token: 0x0600586A RID: 22634 RVA: 0x002DAE78 File Offset: 0x002D9078
		private void Deactivate()
		{
			this.IsActive = false;
			if (this.targetCharacter != null)
			{
				Affliction affliction = this.targetCharacter.CharacterHealth.GetAllAfflictions().FirstOrDefault((Affliction a) => a.Prefab == this.selectedEffect);
				if (affliction != null)
				{
					affliction.Strength = this.GetCombinedEffectStrength();
				}
				Affliction taintedAffliction = this.targetCharacter.CharacterHealth.GetAllAfflictions().FirstOrDefault((Affliction a) => a.Prefab == this.selectedTaintedEffect);
				if (taintedAffliction != null)
				{
					taintedAffliction.Strength = this.GetCombinedTaintedEffectStrength();
				}
			}
			GeneticMaterial nestedMaterial = this.NestedMaterial;
			if (nestedMaterial != null)
			{
				nestedMaterial.Deactivate();
			}
			this.targetCharacter = null;
		}

		// Token: 0x0600586B RID: 22635 RVA: 0x002DAF10 File Offset: 0x002D9110
		public GeneticMaterial.CombineResult Combine(GeneticMaterial otherGeneticMaterial, Character user, out Item itemToDestroy)
		{
			GeneticMaterial.CombineResult combineRefineResult = this.GetCombineRefineResult(otherGeneticMaterial);
			float randomQualityIncrease = Rand.Range(this.ConditionIncreaseOnCombineMin, this.ConditionIncreaseOnCombineMax, Rand.RandSync.Unsynced);
			float talentIncrease = (user != null) ? user.GetStatValue(StatTypes.GeneticMaterialRefineBonus, true) : 0f;
			bool perfectQuality = this.item.IsFullCondition || otherGeneticMaterial.item.IsFullCondition;
			itemToDestroy = otherGeneticMaterial.item;
			if (combineRefineResult == GeneticMaterial.CombineResult.Refined)
			{
				float maxQuality = Math.Max(this.item.Condition, otherGeneticMaterial.item.Condition);
				float minQuality = Math.Min(this.item.Condition, otherGeneticMaterial.item.Condition);
				bool oneIsCombined = this.IsCombined || otherGeneticMaterial.IsCombined;
				float minQualityProportionalIncreaseBonus = MathHelper.Lerp(this.ConditionIncreaseOnLowQualityCombine, this.ConditionIncreaseOnHighQualityCombine, Math.Clamp(minQuality / 80f, 0f, 1f));
				float totalQualityIncrease = minQualityProportionalIncreaseBonus + randomQualityIncrease + talentIncrease;
				if (oneIsCombined)
				{
					totalQualityIncrease /= 2f;
				}
				float newQuality = maxQuality + totalQualityIncrease;
				if (!this.IsCombined && otherGeneticMaterial.IsCombined)
				{
					if (this.item.Prefab == otherGeneticMaterial.item.Prefab)
					{
						ItemInventory ownInventory = this.item.OwnInventory;
						if (ownInventory != null)
						{
							ownInventory.TryPutItem(otherGeneticMaterial.NestedMaterial.item, null, null, true, false, true);
						}
					}
					else
					{
						ItemInventory ownInventory2 = this.item.OwnInventory;
						if (ownInventory2 != null)
						{
							ownInventory2.TryPutItem(otherGeneticMaterial.item, null, null, true, false, true);
						}
						Item otherNestedItem = otherGeneticMaterial.NestedMaterial.item;
						ItemInventory ownInventory3 = otherGeneticMaterial.item.OwnInventory;
						if (ownInventory3 != null)
						{
							ownInventory3.RemoveItem(otherNestedItem);
						}
						itemToDestroy = otherNestedItem;
					}
				}
				this.item.Condition = newQuality;
				if (this.IsCombined)
				{
					this.NestedMaterial.item.Condition = newQuality;
				}
				if (this.CanBeUntainted && perfectQuality)
				{
					this.SetTainted(false, true);
				}
				else if (this.GetTaintedProbabilityOnRefine(otherGeneticMaterial, user) >= Rand.Range(0f, 1f, Rand.RandSync.Unsynced))
				{
					this.SetTainted(true, false);
				}
			}
			else if (combineRefineResult == GeneticMaterial.CombineResult.Combined)
			{
				float averageQuality = (this.item.Condition + otherGeneticMaterial.Item.Condition) / 2f;
				this.item.Condition = (otherGeneticMaterial.Item.Condition = averageQuality + randomQualityIncrease + talentIncrease);
				ItemInventory ownInventory4 = this.item.OwnInventory;
				if (ownInventory4 != null)
				{
					ownInventory4.TryPutItem(otherGeneticMaterial.Item, null, null, true, false, true);
				}
				if (this.CanBeUntainted && perfectQuality)
				{
					this.SetTainted(false, true);
				}
				else if (GeneticMaterial.GetTaintedProbabilityOnCombine(user) >= Rand.Range(0f, 1f, Rand.RandSync.Unsynced))
				{
					this.SetTainted(true, false);
				}
			}
			return combineRefineResult;
		}

		// Token: 0x0600586C RID: 22636 RVA: 0x002DB1B4 File Offset: 0x002D93B4
		private float GetCombinedEffectStrength()
		{
			float effectStrength = 0f;
			foreach (Item otherItem in this.targetCharacter.Inventory.FindAllItems(null, true, null))
			{
				GeneticMaterial geneticMaterial = otherItem.GetComponent<GeneticMaterial>();
				if (geneticMaterial != null && geneticMaterial.IsActive && geneticMaterial.selectedEffect == this.selectedEffect)
				{
					effectStrength += otherItem.ConditionPercentage / 100f * this.selectedEffect.MaxStrength;
				}
			}
			return effectStrength;
		}

		// Token: 0x0600586D RID: 22637 RVA: 0x002DB250 File Offset: 0x002D9450
		private float GetCombinedTaintedEffectStrength()
		{
			float taintedEffectStrength = 0f;
			foreach (Item otherItem in this.targetCharacter.Inventory.FindAllItems(null, true, null))
			{
				GeneticMaterial geneticMaterial = otherItem.GetComponent<GeneticMaterial>();
				if (geneticMaterial != null && geneticMaterial.IsActive && this.selectedTaintedEffect != null && geneticMaterial.selectedTaintedEffect == this.selectedTaintedEffect)
				{
					taintedEffectStrength += otherItem.ConditionPercentage / 100f * this.selectedTaintedEffect.MaxStrength;
				}
			}
			return taintedEffectStrength;
		}

		// Token: 0x0600586E RID: 22638 RVA: 0x002DB2F4 File Offset: 0x002D94F4
		private float GetTaintedProbabilityOnRefine(GeneticMaterial otherGeneticMaterial, Character user)
		{
			if (user == null)
			{
				return 1f;
			}
			float probability = MathHelper.Lerp(0f, 0.99f, Math.Max(this.item.Condition, otherGeneticMaterial.Item.Condition) / 100f);
			probability *= MathHelper.Lerp(1f, 0.25f, base.DegreeOfSuccess(user));
			return MathHelper.Clamp(probability, 0f, 1f);
		}

		// Token: 0x0600586F RID: 22639 RVA: 0x002DB364 File Offset: 0x002D9564
		private static float GetTaintedProbabilityOnCombine(Character user)
		{
			if (user == null)
			{
				return 1f;
			}
			float probability = 1f - user.GetStatValue(StatTypes.GeneticMaterialTaintedProbabilityReductionOnCombine, true);
			return MathHelper.Clamp(probability, 0f, 1f);
		}

		// Token: 0x06005870 RID: 22640 RVA: 0x002DB39A File Offset: 0x002D959A
		public void SetTainted(bool newValue, bool affectsNestedGene = false)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			this.Tainted = newValue;
			if (affectsNestedGene && this.NestedMaterial != null)
			{
				this.NestedMaterial.SetTainted(newValue, false);
			}
		}

		// Token: 0x06005871 RID: 22641 RVA: 0x002DB3D0 File Offset: 0x002D95D0
		public static LocalizedString TryCreateName(ItemPrefab prefab, XElement element)
		{
			foreach (XElement subElement in element.Elements())
			{
				Identifier identifier = subElement.NameAsIdentifier();
				if (identifier == "GeneticMaterial")
				{
					Identifier nameId = subElement.GetAttributeIdentifier("nameidentifier", "");
					if (!nameId.IsEmpty)
					{
						return prefab.Name.Replace("[type]", TextManager.Get(nameId).Fallback(nameId.Value, true), StringComparison.Ordinal);
					}
				}
			}
			return prefab.Name;
		}

		// Token: 0x04002D11 RID: 11537
		private readonly LocalizedString materialName;

		// Token: 0x04002D12 RID: 11538
		private Character targetCharacter;

		// Token: 0x04002D13 RID: 11539
		private AfflictionPrefab selectedEffect;

		// Token: 0x04002D14 RID: 11540
		private AfflictionPrefab selectedTaintedEffect;

		// Token: 0x04002D17 RID: 11543
		private bool tainted;

		// Token: 0x020013A4 RID: 5028
		public enum CombineResult
		{
			// Token: 0x04006312 RID: 25362
			None,
			// Token: 0x04006313 RID: 25363
			Refined,
			// Token: 0x04006314 RID: 25364
			Combined
		}
	}
}
