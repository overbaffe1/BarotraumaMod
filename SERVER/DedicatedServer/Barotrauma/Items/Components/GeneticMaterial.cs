using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000493 RID: 1171
	internal class GeneticMaterial : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06003EC8 RID: 16072 RVA: 0x00194AA4 File Offset: 0x00192CA4
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.tainted);
			if (this.tainted)
			{
				AfflictionPrefab afflictionPrefab = this.selectedTaintedEffect;
				msg.WriteUInt32((afflictionPrefab != null) ? afflictionPrefab.UintIdentifier : 0U);
				return;
			}
			AfflictionPrefab afflictionPrefab2 = this.selectedEffect;
			msg.WriteUInt32((afflictionPrefab2 != null) ? afflictionPrefab2.UintIdentifier : 0U);
		}

		// Token: 0x17001093 RID: 4243
		// (get) Token: 0x06003EC9 RID: 16073 RVA: 0x00194AF6 File Offset: 0x00192CF6
		// (set) Token: 0x06003ECA RID: 16074 RVA: 0x00194AFE File Offset: 0x00192CFE
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string Effect { get; set; }

		// Token: 0x17001094 RID: 4244
		// (get) Token: 0x06003ECB RID: 16075 RVA: 0x00194B07 File Offset: 0x00192D07
		// (set) Token: 0x06003ECC RID: 16076 RVA: 0x00194B0F File Offset: 0x00192D0F
		[Serialize("geneticmaterialdebuff", IsPropertySaveable.Yes, "Either the identifier or the type for the tainted effect prefab", "", false)]
		public Identifier TaintedEffect { get; set; }

		// Token: 0x17001095 RID: 4245
		// (get) Token: 0x06003ECD RID: 16077 RVA: 0x00194B18 File Offset: 0x00192D18
		// (set) Token: 0x06003ECE RID: 16078 RVA: 0x00194B20 File Offset: 0x00192D20
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

		// Token: 0x17001096 RID: 4246
		// (get) Token: 0x06003ECF RID: 16079 RVA: 0x00194BAD File Offset: 0x00192DAD
		// (set) Token: 0x06003ED0 RID: 16080 RVA: 0x00194BB5 File Offset: 0x00192DB5
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool SetTaintedOnDeath { get; private set; }

		// Token: 0x17001097 RID: 4247
		// (get) Token: 0x06003ED1 RID: 16081 RVA: 0x00194BBE File Offset: 0x00192DBE
		// (set) Token: 0x06003ED2 RID: 16082 RVA: 0x00194BC6 File Offset: 0x00192DC6
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool CanBeUntainted { get; private set; }

		// Token: 0x17001098 RID: 4248
		// (get) Token: 0x06003ED3 RID: 16083 RVA: 0x00194BCF File Offset: 0x00192DCF
		// (set) Token: 0x06003ED4 RID: 16084 RVA: 0x00194BE8 File Offset: 0x00192DE8
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

		// Token: 0x06003ED5 RID: 16085 RVA: 0x00194C30 File Offset: 0x00192E30
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

		// Token: 0x17001099 RID: 4249
		// (get) Token: 0x06003ED6 RID: 16086 RVA: 0x00194C98 File Offset: 0x00192E98
		// (set) Token: 0x06003ED7 RID: 16087 RVA: 0x00194CA0 File Offset: 0x00192EA0
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float ConditionIncreaseOnCombineMin { get; set; }

		// Token: 0x1700109A RID: 4250
		// (get) Token: 0x06003ED8 RID: 16088 RVA: 0x00194CA9 File Offset: 0x00192EA9
		// (set) Token: 0x06003ED9 RID: 16089 RVA: 0x00194CB1 File Offset: 0x00192EB1
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float ConditionIncreaseOnCombineMax { get; set; }

		// Token: 0x1700109B RID: 4251
		// (get) Token: 0x06003EDA RID: 16090 RVA: 0x00194CBA File Offset: 0x00192EBA
		// (set) Token: 0x06003EDB RID: 16091 RVA: 0x00194CC2 File Offset: 0x00192EC2
		[Serialize(3f, IsPropertySaveable.No, "When refining, min value for condition increase bonus based on the quality of the worse gene.", "", false)]
		public float ConditionIncreaseOnLowQualityCombine { get; set; }

		// Token: 0x1700109C RID: 4252
		// (get) Token: 0x06003EDC RID: 16092 RVA: 0x00194CCB File Offset: 0x00192ECB
		// (set) Token: 0x06003EDD RID: 16093 RVA: 0x00194CD3 File Offset: 0x00192ED3
		[Serialize(25f, IsPropertySaveable.No, "When refining, max value for condition increase bonus based on the quality of the worse gene.", "", false)]
		public float ConditionIncreaseOnHighQualityCombine { get; set; }

		// Token: 0x06003EDE RID: 16094 RVA: 0x00194CDC File Offset: 0x00192EDC
		private bool SharesTypeWith(GeneticMaterial otherGeneticMaterial)
		{
			return this.GetSharedTypeOrDefault(otherGeneticMaterial) != null;
		}

		// Token: 0x06003EDF RID: 16095 RVA: 0x00194CE8 File Offset: 0x00192EE8
		private ItemPrefab GetSharedTypeOrDefault(GeneticMaterial otherGeneticMaterial)
		{
			if (otherGeneticMaterial == null)
			{
				return null;
			}
			return this.AllMaterialTypes.FirstOrDefault((ItemPrefab materialType) => otherGeneticMaterial.AllMaterialTypes.Contains(materialType));
		}

		// Token: 0x1700109D RID: 4253
		// (get) Token: 0x06003EE0 RID: 16096 RVA: 0x00194D24 File Offset: 0x00192F24
		private IEnumerable<ItemPrefab> AllMaterialTypes
		{
			get
			{
				GeneticMaterial.<get_AllMaterialTypes>d__48 <get_AllMaterialTypes>d__ = new GeneticMaterial.<get_AllMaterialTypes>d__48(-2);
				<get_AllMaterialTypes>d__.<>4__this = this;
				return <get_AllMaterialTypes>d__;
			}
		}

		// Token: 0x1700109E RID: 4254
		// (get) Token: 0x06003EE1 RID: 16097 RVA: 0x00194D44 File Offset: 0x00192F44
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

		// Token: 0x1700109F RID: 4255
		// (get) Token: 0x06003EE2 RID: 16098 RVA: 0x00194DA8 File Offset: 0x00192FA8
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

		// Token: 0x06003EE3 RID: 16099 RVA: 0x00194E00 File Offset: 0x00193000
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

		// Token: 0x06003EE4 RID: 16100 RVA: 0x00194E4D File Offset: 0x0019304D
		public bool CanBeCombinedWith(GeneticMaterial otherGeneticMaterial)
		{
			return this.GetCombineRefineResult(otherGeneticMaterial) > GeneticMaterial.CombineResult.None;
		}

		// Token: 0x06003EE5 RID: 16101 RVA: 0x00194E5C File Offset: 0x0019305C
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
				this.item.CreateServerEvent<GeneticMaterial>(this);
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
				this.item.CreateServerEvent<GeneticMaterial>(this);
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

		// Token: 0x06003EE6 RID: 16102 RVA: 0x00194FE0 File Offset: 0x001931E0
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

		// Token: 0x06003EE7 RID: 16103 RVA: 0x00195138 File Offset: 0x00193338
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

		// Token: 0x06003EE8 RID: 16104 RVA: 0x001951D0 File Offset: 0x001933D0
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

		// Token: 0x06003EE9 RID: 16105 RVA: 0x00195474 File Offset: 0x00193674
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

		// Token: 0x06003EEA RID: 16106 RVA: 0x00195510 File Offset: 0x00193710
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

		// Token: 0x06003EEB RID: 16107 RVA: 0x001955B4 File Offset: 0x001937B4
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

		// Token: 0x06003EEC RID: 16108 RVA: 0x00195624 File Offset: 0x00193824
		private static float GetTaintedProbabilityOnCombine(Character user)
		{
			if (user == null)
			{
				return 1f;
			}
			float probability = 1f - user.GetStatValue(StatTypes.GeneticMaterialTaintedProbabilityReductionOnCombine, true);
			return MathHelper.Clamp(probability, 0f, 1f);
		}

		// Token: 0x06003EED RID: 16109 RVA: 0x0019565C File Offset: 0x0019385C
		public void SetTainted(bool newValue, bool affectsNestedGene = false)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			this.Tainted = newValue;
			this.item.CreateServerEvent<GeneticMaterial>(this);
			if (affectsNestedGene && this.NestedMaterial != null)
			{
				this.NestedMaterial.SetTainted(newValue, false);
			}
		}

		// Token: 0x06003EEE RID: 16110 RVA: 0x001956A8 File Offset: 0x001938A8
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

		// Token: 0x04001E0A RID: 7690
		private readonly LocalizedString materialName;

		// Token: 0x04001E0B RID: 7691
		private Character targetCharacter;

		// Token: 0x04001E0C RID: 7692
		private AfflictionPrefab selectedEffect;

		// Token: 0x04001E0D RID: 7693
		private AfflictionPrefab selectedTaintedEffect;

		// Token: 0x04001E10 RID: 7696
		private bool tainted;

		// Token: 0x02000D79 RID: 3449
		public enum CombineResult
		{
			// Token: 0x04003FD8 RID: 16344
			None,
			// Token: 0x04003FD9 RID: 16345
			Refined,
			// Token: 0x04003FDA RID: 16346
			Combined
		}
	}
}
