using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000013 RID: 19
	internal class CharacterInfo
	{
		// Token: 0x06000224 RID: 548 RVA: 0x0001195B File Offset: 0x0000FB5B
		public void ApplyDeathEffects()
		{
			RespawnManager.ReduceCharacterSkillsOnDeath(this, false);
			this.RemoveSavedStatValuesOnDeath();
			this.CauseOfDeath = null;
		}

		// Token: 0x06000225 RID: 549 RVA: 0x00011974 File Offset: 0x0000FB74
		public void ServerWrite(IWriteMessage msg)
		{
			msg.WriteUInt16(this.ID);
			msg.WriteString(this.Name);
			msg.WriteString(this.OriginalName);
			msg.WriteBoolean(this.RenamingEnabled);
			msg.WriteByte((byte)this.BotStatus);
			msg.WriteInt32(this.Salary);
			msg.WriteByte((byte)this.Head.Preset.TagSet.Count);
			foreach (Identifier tag in this.Head.Preset.TagSet)
			{
				msg.WriteIdentifier(tag);
			}
			msg.WriteByte((byte)this.Head.HairIndex);
			msg.WriteByte((byte)this.Head.BeardIndex);
			msg.WriteByte((byte)this.Head.MoustacheIndex);
			msg.WriteByte((byte)this.Head.FaceAttachmentIndex);
			msg.WriteColorR8G8B8(this.Head.SkinColor);
			msg.WriteColorR8G8B8(this.Head.HairColor);
			msg.WriteColorR8G8B8(this.Head.FacialHairColor);
			msg.WriteIdentifier(this.HumanPrefabIds.Item2);
			msg.WriteIdentifier(this.MinReputationToHire.Item1);
			if (!this.MinReputationToHire.Item1.IsEmpty)
			{
				msg.WriteSingle(this.MinReputationToHire.Item2);
			}
			if (this.Job != null)
			{
				msg.WriteUInt32(this.Job.Prefab.UintIdentifier);
				msg.WriteByte((byte)this.Job.Variant);
				IOrderedEnumerable<Skill> skills = from s in this.Job.GetSkills()
				orderby s.Identifier
				select s;
				msg.WriteByte((byte)skills.Count<Skill>());
				using (IEnumerator<Skill> enumerator2 = skills.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Skill skill = enumerator2.Current;
						msg.WriteIdentifier(skill.Identifier);
						msg.WriteSingle(skill.Level);
					}
					goto IL_222;
				}
			}
			msg.WriteUInt32(0U);
			msg.WriteByte(0);
			IL_222:
			msg.WriteInt32(this.ExperiencePoints);
			msg.WriteRangedInteger(this.AdditionalTalentPoints, 0, 100);
			msg.WriteBoolean(this.PermanentlyDead);
			msg.WriteInt32(this.TalentRefundPoints);
			msg.WriteInt32(this.TalentResetCount);
		}

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000226 RID: 550 RVA: 0x00011C00 File Offset: 0x0000FE00
		// (set) Token: 0x06000227 RID: 551 RVA: 0x00011C08 File Offset: 0x0000FE08
		public CharacterInfo.HeadInfo Head
		{
			get
			{
				return this.head;
			}
			set
			{
				if (this.head != value && value != null)
				{
					this.head = value;
					this.HeadSprite = null;
					this.AttachmentSprites = null;
					this.hairs = null;
					this.beards = null;
					this.moustaches = null;
					this.faceAttachments = null;
				}
			}
		}

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x06000228 RID: 552 RVA: 0x00011C48 File Offset: 0x0000FE48
		public bool IsMale
		{
			get
			{
				CharacterInfo.HeadInfo headInfo = this.head;
				bool? flag;
				if (headInfo == null)
				{
					flag = null;
				}
				else
				{
					CharacterInfo.HeadPreset preset = headInfo.Preset;
					if (preset == null)
					{
						flag = null;
					}
					else
					{
						ImmutableHashSet<Identifier> tagSet = preset.TagSet;
						flag = ((tagSet != null) ? new bool?(tagSet.Contains(this.maleIdentifier)) : null);
					}
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
		}

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000229 RID: 553 RVA: 0x00011CAC File Offset: 0x0000FEAC
		public bool IsFemale
		{
			get
			{
				CharacterInfo.HeadInfo headInfo = this.head;
				bool? flag;
				if (headInfo == null)
				{
					flag = null;
				}
				else
				{
					CharacterInfo.HeadPreset preset = headInfo.Preset;
					if (preset == null)
					{
						flag = null;
					}
					else
					{
						ImmutableHashSet<Identifier> tagSet = preset.TagSet;
						flag = ((tagSet != null) ? new bool?(tagSet.Contains(this.femaleIdentifier)) : null);
					}
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x0600022A RID: 554 RVA: 0x00011D0E File Offset: 0x0000FF0E
		public CharacterInfoPrefab Prefab
		{
			get
			{
				return CharacterPrefab.Prefabs[this.SpeciesName].CharacterInfoPrefab;
			}
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x0600022B RID: 555 RVA: 0x00011D25 File Offset: 0x0000FF25
		// (set) Token: 0x0600022C RID: 556 RVA: 0x00011D2D File Offset: 0x0000FF2D
		public BotStatus BotStatus
		{
			get
			{
				return this.botStatus;
			}
			set
			{
				this.botStatus = value;
				if (this.botStatus == BotStatus.ActiveService && this.character == null)
				{
					this.PendingSpawnToActiveService = true;
				}
			}
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x0600022D RID: 557 RVA: 0x00011D4E File Offset: 0x0000FF4E
		public bool IsOnReserveBench
		{
			get
			{
				return this.BotStatus == BotStatus.ReserveBench;
			}
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x0600022E RID: 558 RVA: 0x00011D59 File Offset: 0x0000FF59
		public bool HasNickname
		{
			get
			{
				return this.Name != this.OriginalName;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x0600022F RID: 559 RVA: 0x00011D6C File Offset: 0x0000FF6C
		// (set) Token: 0x06000230 RID: 560 RVA: 0x00011D74 File Offset: 0x0000FF74
		public string OriginalName { get; private set; }

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x06000231 RID: 561 RVA: 0x00011D80 File Offset: 0x0000FF80
		public HumanPrefab HumanPrefab
		{
			get
			{
				ValueTuple<Identifier, Identifier> humanPrefabIds = this.HumanPrefabIds;
				Identifier identifier = default(Identifier);
				if (humanPrefabIds.Item1 == identifier)
				{
					Identifier identifier2 = default(Identifier);
					if (humanPrefabIds.Item2 == identifier2)
					{
						return null;
					}
				}
				if (this._humanPrefab == null)
				{
					this._humanPrefab = NPCSet.Get(this.HumanPrefabIds.Item1, this.HumanPrefabIds.Item2, true, null);
				}
				return this._humanPrefab;
			}
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x06000232 RID: 562 RVA: 0x00011DF8 File Offset: 0x0000FFF8
		public string DisplayName
		{
			get
			{
				if (this.Character == null || !this.Character.HideFace)
				{
					this.IsDisguised = (this.IsDisguisedAsAnother = false);
					return this.Name;
				}
				if (GameMain.NetworkMember != null && !GameMain.NetworkMember.ServerSettings.AllowDisguises)
				{
					this.IsDisguised = (this.IsDisguisedAsAnother = false);
					return this.Name;
				}
				if (this.Character.Inventory != null)
				{
					Item idCard = this.Character.Inventory.GetItemInLimbSlot(InvSlotType.Card);
					string text;
					if (idCard == null)
					{
						text = null;
					}
					else
					{
						IdCard component = idCard.GetComponent<IdCard>();
						text = ((component != null) ? component.OwnerName : null);
					}
					return text ?? "???";
				}
				return "???";
			}
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x06000233 RID: 563 RVA: 0x00011EAA File Offset: 0x000100AA
		public Identifier SpeciesName { get; }

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x06000234 RID: 564 RVA: 0x00011EB2 File Offset: 0x000100B2
		// (set) Token: 0x06000235 RID: 565 RVA: 0x00011EBA File Offset: 0x000100BA
		public Character Character
		{
			get
			{
				return this.character;
			}
			set
			{
				this.character = value;
				if (this.character != null)
				{
					this.PendingSpawnToActiveService = false;
				}
			}
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x06000236 RID: 566 RVA: 0x00011ED2 File Offset: 0x000100D2
		// (set) Token: 0x06000237 RID: 567 RVA: 0x00011EDA File Offset: 0x000100DA
		public int ExperiencePoints { get; private set; }

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000238 RID: 568 RVA: 0x00011EE3 File Offset: 0x000100E3
		// (set) Token: 0x06000239 RID: 569 RVA: 0x00011EEB File Offset: 0x000100EB
		public int TalentRefundPoints
		{
			get
			{
				return this.talentRefundPoints;
			}
			set
			{
				this.talentRefundPoints = MathHelper.Max(value, 0);
			}
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x0600023A RID: 570 RVA: 0x00011EFA File Offset: 0x000100FA
		// (set) Token: 0x0600023B RID: 571 RVA: 0x00011F02 File Offset: 0x00010102
		public HashSet<Identifier> UnlockedTalents { get; private set; } = new HashSet<Identifier>();

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x0600023C RID: 572 RVA: 0x00011F0B File Offset: 0x0001010B
		// (set) Token: 0x0600023D RID: 573 RVA: 0x00011F13 File Offset: 0x00010113
		public HashSet<Identifier> ResettableExtraTalents { get; private set; } = new HashSet<Identifier>();

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x0600023E RID: 574 RVA: 0x00011F1C File Offset: 0x0001011C
		// (set) Token: 0x0600023F RID: 575 RVA: 0x00011F24 File Offset: 0x00010124
		public int TalentResetCount
		{
			get
			{
				return this.talentResetCount;
			}
			set
			{
				this.talentResetCount = MathHelper.Max(value, 0);
			}
		}

		// Token: 0x06000240 RID: 576 RVA: 0x00011F34 File Offset: 0x00010134
		public IEnumerable<Identifier> GetUnlockedTalentsInTree()
		{
			TalentTree talentTree;
			if (!TalentTree.JobTalentTrees.TryGet(this.Job.Prefab.Identifier, out talentTree))
			{
				return Enumerable.Empty<Identifier>();
			}
			return from t in this.UnlockedTalents
			where talentTree.TalentIsInTree(t)
			select t;
		}

		// Token: 0x06000241 RID: 577 RVA: 0x00011F88 File Offset: 0x00010188
		public IEnumerable<Identifier> GetUnlockedTalentsOutsideTree()
		{
			TalentTree talentTree;
			if (!TalentTree.JobTalentTrees.TryGet(this.Job.Prefab.Identifier, out talentTree))
			{
				return Enumerable.Empty<Identifier>();
			}
			return from t in this.UnlockedTalents
			where !talentTree.TalentIsInTree(t)
			select t;
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x06000242 RID: 578 RVA: 0x00011FDA File Offset: 0x000101DA
		// (set) Token: 0x06000243 RID: 579 RVA: 0x00011FE2 File Offset: 0x000101E2
		public int AdditionalTalentPoints
		{
			get
			{
				return this.additionalTalentPoints;
			}
			set
			{
				this.additionalTalentPoints = MathHelper.Clamp(value, 0, 100);
			}
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000244 RID: 580 RVA: 0x00011FF3 File Offset: 0x000101F3
		// (set) Token: 0x06000245 RID: 581 RVA: 0x00012009 File Offset: 0x00010209
		public Sprite HeadSprite
		{
			get
			{
				if (this._headSprite == null)
				{
					this.LoadHeadSprite();
				}
				return this._headSprite;
			}
			private set
			{
				if (this._headSprite != null)
				{
					this._headSprite.Remove();
				}
				this._headSprite = value;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000246 RID: 582 RVA: 0x00012025 File Offset: 0x00010225
		// (set) Token: 0x06000247 RID: 583 RVA: 0x0001203B File Offset: 0x0001023B
		public Sprite Portrait
		{
			get
			{
				if (this.portrait == null)
				{
					this.LoadHeadSprite();
				}
				return this.portrait;
			}
			private set
			{
				if (this.portrait != null)
				{
					this.portrait.Remove();
				}
				this.portrait = value;
			}
		}

		// Token: 0x06000248 RID: 584 RVA: 0x00012058 File Offset: 0x00010258
		public void CheckDisguiseStatus(bool handleBuff, IdCard idCard = null)
		{
			if (this.Character == null)
			{
				return;
			}
			string currentlyDisplayedName = this.DisplayName;
			this.IsDisguised = (currentlyDisplayedName == "???");
			this.IsDisguisedAsAnother = (!this.IsDisguised && currentlyDisplayedName != this.Name);
			if (this.IsDisguisedAsAnother)
			{
				AfflictionPrefab afflictionPrefab;
				if (handleBuff && AfflictionPrefab.Prefabs.TryGet("disguised", out afflictionPrefab))
				{
					this.Character.CharacterHealth.ApplyAffliction(null, afflictionPrefab.Instantiate(100f, null), true, false, true);
				}
				if (idCard == null)
				{
					CharacterInventory inventory = this.Character.Inventory;
					IdCard idCard2;
					if (inventory == null)
					{
						idCard2 = null;
					}
					else
					{
						Item itemInLimbSlot = inventory.GetItemInLimbSlot(InvSlotType.Card);
						idCard2 = ((itemInLimbSlot != null) ? itemInLimbSlot.GetComponent<IdCard>() : null);
					}
					idCard = idCard2;
				}
				if (idCard != null)
				{
					return;
				}
			}
			if (handleBuff)
			{
				this.Character.CharacterHealth.ReduceAfflictionOnAllLimbs("disguised".ToIdentifier(), 100f, null, null);
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000249 RID: 585 RVA: 0x0001213E File Offset: 0x0001033E
		// (set) Token: 0x0600024A RID: 586 RVA: 0x0001214D File Offset: 0x0001034D
		public List<WearableSprite> AttachmentSprites
		{
			get
			{
				List<WearableSprite> list = this.attachmentSprites;
				return this.attachmentSprites;
			}
			private set
			{
				if (this.attachmentSprites != null)
				{
					this.attachmentSprites.ForEach(delegate(WearableSprite s)
					{
						Sprite sprite = s.Sprite;
						if (sprite == null)
						{
							return;
						}
						sprite.Remove();
					});
				}
				this.attachmentSprites = value;
			}
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x0600024B RID: 587 RVA: 0x00012188 File Offset: 0x00010388
		// (set) Token: 0x0600024C RID: 588 RVA: 0x00012190 File Offset: 0x00010390
		public ContentXElement CharacterConfigElement { get; set; }

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x0600024D RID: 589 RVA: 0x00012199 File Offset: 0x00010399
		// (set) Token: 0x0600024E RID: 590 RVA: 0x000121A1 File Offset: 0x000103A1
		public NPCPersonalityTrait PersonalityTrait { get; private set; }

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x0600024F RID: 591 RVA: 0x000121AA File Offset: 0x000103AA
		public static int HighestManualOrderPriority
		{
			get
			{
				return 3;
			}
		}

		// Token: 0x06000250 RID: 592 RVA: 0x000121B0 File Offset: 0x000103B0
		public int GetManualOrderPriority(Order order)
		{
			if (order != null && order.AssignmentPriority < 100 && this.CurrentOrders.Any<Order>())
			{
				int orderPriority = CharacterInfo.HighestManualOrderPriority;
				int i = 0;
				while (i < this.CurrentOrders.Count && order.AssignmentPriority < this.CurrentOrders[i].AssignmentPriority)
				{
					orderPriority--;
					i++;
				}
				return Math.Max(orderPriority, 1);
			}
			return CharacterInfo.HighestManualOrderPriority;
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000251 RID: 593 RVA: 0x0001221D File Offset: 0x0001041D
		public List<Order> CurrentOrders { get; } = new List<Order>();

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x06000252 RID: 594 RVA: 0x00012225 File Offset: 0x00010425
		// (set) Token: 0x06000253 RID: 595 RVA: 0x0001222D File Offset: 0x0001042D
		public List<Identifier> SpriteTags { get; private set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000254 RID: 596 RVA: 0x00012238 File Offset: 0x00010438
		// (set) Token: 0x06000255 RID: 597 RVA: 0x000122C4 File Offset: 0x000104C4
		public RagdollParams Ragdoll
		{
			get
			{
				if (this.ragdoll == null)
				{
					Identifier speciesName = this.SpeciesName;
					this.ragdoll = (this.CharacterConfigElement.GetAttributeBool("humanoid", speciesName == CharacterPrefab.HumanSpeciesName) ? RagdollParams.GetDefaultRagdollParams<HumanRagdollParams>(this.SpeciesName, this.CharacterConfigElement, this.CharacterConfigElement.ContentPackage) : RagdollParams.GetDefaultRagdollParams<FishRagdollParams>(this.SpeciesName, this.CharacterConfigElement, this.CharacterConfigElement.ContentPackage));
				}
				return this.ragdoll;
			}
			set
			{
				this.ragdoll = value;
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000256 RID: 598 RVA: 0x000122CD File Offset: 0x000104CD
		public bool IsAttachmentsLoaded
		{
			get
			{
				return this.Head.HairIndex > -1 && this.Head.BeardIndex > -1 && this.Head.MoustacheIndex > -1 && this.Head.FaceAttachmentIndex > -1;
			}
		}

		// Token: 0x06000257 RID: 599 RVA: 0x00012309 File Offset: 0x00010509
		public IEnumerable<ContentXElement> GetValidAttachmentElements(IEnumerable<ContentXElement> elements, CharacterInfo.HeadPreset headPreset, WearableType? wearableType = null)
		{
			return this.FilterElements(elements, headPreset.TagSet, wearableType);
		}

		// Token: 0x06000258 RID: 600 RVA: 0x00012319 File Offset: 0x00010519
		public int CountValidAttachmentsOfType(WearableType wearableType)
		{
			return this.GetValidAttachmentElements(this.Wearables, this.Head.Preset, new WearableType?(wearableType)).Count<ContentXElement>();
		}

		// Token: 0x06000259 RID: 601 RVA: 0x00012340 File Offset: 0x00010540
		private void GetName(Rand.RandSync randSync, out string name)
		{
			ContentXElement nameElement = this.CharacterConfigElement.GetChildElement("names") ?? this.CharacterConfigElement.GetChildElement("name");
			ContentPath namesXmlFile = ((nameElement != null) ? nameElement.GetAttributeContentPath("path") : null) ?? ContentPath.Empty;
			XElement namesXml;
			if (!namesXmlFile.IsNullOrEmpty())
			{
				XDocument doc = XMLExtensions.TryLoadXml(namesXmlFile);
				namesXml = doc.Root;
			}
			else
			{
				namesXml = new XElement("names", new XAttribute("format", "[firstname] [lastname]"));
				ContentXElement contentXElement = null;
				string text;
				if (!(nameElement == contentXElement))
				{
					ContentPath attributeContentPath = nameElement.GetAttributeContentPath("firstname");
					text = this.ReplaceVars(((attributeContentPath != null) ? attributeContentPath.Value : null) ?? "");
				}
				else
				{
					text = string.Empty;
				}
				string firstNamesPath = text;
				contentXElement = null;
				string text2;
				if (!(nameElement == contentXElement))
				{
					ContentPath attributeContentPath2 = nameElement.GetAttributeContentPath("lastname");
					text2 = this.ReplaceVars(((attributeContentPath2 != null) ? attributeContentPath2.Value : null) ?? "");
				}
				else
				{
					text2 = string.Empty;
				}
				string lastNamesPath = text2;
				if (File.Exists(firstNamesPath) && File.Exists(lastNamesPath))
				{
					string[] firstNames = File.ReadAllLines(firstNamesPath, null, true);
					string[] lastNames = File.ReadAllLines(lastNamesPath, null, true);
					namesXml.Add(from n in firstNames
					select new XElement("firstname", new XAttribute("value", n)));
					namesXml.Add(from n in lastNames
					select new XElement("lastname", new XAttribute("value", n)));
				}
				else
				{
					XDocument doc2 = XMLExtensions.TryLoadXml("Content/Characters/Human/names.xml");
					namesXml = doc2.Root;
				}
			}
			name = namesXml.GetAttributeString("format", "");
			Dictionary<Identifier, List<string>> entries = new Dictionary<Identifier, List<string>>();
			foreach (XElement subElement in namesXml.Elements())
			{
				Identifier elemName = subElement.NameAsIdentifier();
				if (!entries.ContainsKey(elemName))
				{
					entries.Add(elemName, new List<string>());
				}
				ImmutableHashSet<Identifier> identifiers = subElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
				if (identifiers.IsSubsetOf(this.Head.Preset.TagSet))
				{
					entries[elemName].Add(subElement.GetAttributeString("value", ""));
				}
			}
			foreach (Identifier i in entries.Keys)
			{
				string text3 = name;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(i);
				defaultInterpolatedStringHandler.AppendLiteral("]");
				name = text3.Replace(defaultInterpolatedStringHandler.ToStringAndClear(), entries[i].GetRandom(randSync), StringComparison.OrdinalIgnoreCase);
			}
		}

		// Token: 0x0600025A RID: 602 RVA: 0x00012630 File Offset: 0x00010830
		private static void LoadTagsBackwardsCompatibility(XElement element, HashSet<Identifier> tags)
		{
			Identifier gender = element.GetAttributeIdentifier("gender", "");
			int headSpriteId = element.GetAttributeInt("headspriteid", -1);
			if (!gender.IsEmpty)
			{
				tags.Add(gender);
			}
			if (headSpriteId > 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
				defaultInterpolatedStringHandler.AppendLiteral("head");
				defaultInterpolatedStringHandler.AppendFormatted<int>(headSpriteId);
				tags.Add(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
			}
		}

		// Token: 0x0600025B RID: 603 RVA: 0x000126A0 File Offset: 0x000108A0
		private static bool ElementHasSpecifierTags(XElement element)
		{
			return element.GetAttributeBool("specifiertags", element.GetAttributeBool("genders", element.GetAttributeBool("races", false)));
		}

		// Token: 0x0600025C RID: 604 RVA: 0x000126C4 File Offset: 0x000108C4
		public CharacterInfo(Identifier speciesName, string name = "", string originalName = "", Either<Job, JobPrefab> jobOrJobPrefab = null, int variant = 0, Rand.RandSync randSync = Rand.RandSync.Unsynced, Identifier npcIdentifier = default(Identifier))
		{
			Option.UnspecifiedNone none = Option.None;
			this.LastRewardDistribution = none;
			this.SavedStatValues = new Dictionary<StatTypes, List<SavedStatValue>>();
			this.LastResistanceMultiplierSkillLossDeath = 1f;
			this.LastResistanceMultiplierSkillLossRespawn = 1f;
			base..ctor();
			JobPrefab jobPrefab = null;
			Job job = null;
			if (jobOrJobPrefab != null)
			{
				jobOrJobPrefab.TryGet(out job);
				jobOrJobPrefab.TryGet(out jobPrefab);
			}
			this.ID = CharacterInfo.idCounter;
			CharacterInfo.idCounter += 1;
			if (CharacterInfo.idCounter == 0)
			{
				CharacterInfo.idCounter += 1;
			}
			this.SpeciesName = speciesName;
			this.SpriteTags = new List<Identifier>();
			CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(this.SpeciesName);
			this.CharacterConfigElement = ((characterPrefab != null) ? characterPrefab.ConfigElement : null);
			ContentXElement characterConfigElement = this.CharacterConfigElement;
			ContentXElement contentXElement = null;
			if (characterConfigElement == contentXElement)
			{
				return;
			}
			this.HasSpecifierTags = CharacterInfo.ElementHasSpecifierTags(this.CharacterConfigElement);
			if (this.HasSpecifierTags)
			{
				ContentXElement characterConfigElement2 = this.CharacterConfigElement;
				string key = "haircolors";
				ValueTuple<Color, float>[] array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(Color.WhiteSmoke, 100f)
				};
				this.HairColors = characterConfigElement2.GetAttributeTupleArray<Color, float>(key, array).ToImmutableArray<ValueTuple<Color, float>>();
				ContentXElement characterConfigElement3 = this.CharacterConfigElement;
				string key2 = "facialhaircolors";
				array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(Color.WhiteSmoke, 100f)
				};
				this.FacialHairColors = characterConfigElement3.GetAttributeTupleArray<Color, float>(key2, array).ToImmutableArray<ValueTuple<Color, float>>();
				ContentXElement characterConfigElement4 = this.CharacterConfigElement;
				string key3 = "skincolors";
				array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(new Color(255, 215, 200, 255), 100f)
				};
				this.SkinColors = characterConfigElement4.GetAttributeTupleArray<Color, float>(key3, array).ToImmutableArray<ValueTuple<Color, float>>();
				CharacterInfoPrefab prefab = this.Prefab;
				CharacterInfo.HeadPreset headPreset = (prefab != null) ? prefab.Heads.GetRandom(randSync) : null;
				if (headPreset == null)
				{
					DebugConsole.ThrowError("Failed to find a head preset!", null, null, false, false);
				}
				this.Head = new CharacterInfo.HeadInfo(this, headPreset, 0, 0, 0, 0);
				this.SetAttachments(randSync);
				this.SetColors(randSync);
				this.Job = (job ?? ((jobPrefab == null) ? Job.Random(false, Rand.RandSync.Unsynced) : new Job(jobPrefab, false, randSync, variant, Array.Empty<Skill>())));
				if (!string.IsNullOrEmpty(name))
				{
					this.Name = name;
				}
				else
				{
					this.Name = this.GetRandomName(randSync);
				}
				this.TryLoadNameAndTitle(npcIdentifier);
				this.SetPersonalityTrait();
				this.Salary = this.CalculateSalary(0, 1f);
			}
			this.OriginalName = ((!string.IsNullOrEmpty(originalName)) ? originalName : this.Name);
		}

		// Token: 0x0600025D RID: 605 RVA: 0x000129B8 File Offset: 0x00010BB8
		private void SetPersonalityTrait()
		{
			this.PersonalityTrait = NPCPersonalityTrait.GetRandom(this.Name + string.Concat<Identifier>(from tag in this.Head.Preset.TagSet
			orderby tag
			select tag));
		}

		// Token: 0x0600025E RID: 606 RVA: 0x00012A14 File Offset: 0x00010C14
		public string GetRandomName(Rand.RandSync randSync)
		{
			string name;
			this.GetName(randSync, out name);
			return name;
		}

		// Token: 0x0600025F RID: 607 RVA: 0x00012A2B File Offset: 0x00010C2B
		public void SetNameBasedOnJob()
		{
			if (this.Job == null)
			{
				return;
			}
			this.Name = this.Job.Name.Value;
			this.OriginalName = this.Name;
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00012A58 File Offset: 0x00010C58
		public static Color SelectRandomColor([TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})] in ImmutableArray<ValueTuple<Color, float>> array, Rand.RandSync randSync)
		{
			return ToolBox.SelectWeightedRandom<ValueTuple<Color, float>>(array, (from p in array
			select p.Item2).ToArray<float>(), randSync).Item1;
		}

		// Token: 0x06000261 RID: 609 RVA: 0x00012AAC File Offset: 0x00010CAC
		private void SetAttachments(Rand.RandSync randSync)
		{
			CharacterInfo.<>c__DisplayClass145_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.randSync = randSync;
			this.LoadHeadAttachments();
			this.Head.HairIndex = this.<SetAttachments>g__pickRandomIndex|145_0(this.Hairs, ref CS$<>8__locals1);
			this.Head.BeardIndex = this.<SetAttachments>g__pickRandomIndex|145_0(this.Beards, ref CS$<>8__locals1);
			this.Head.MoustacheIndex = this.<SetAttachments>g__pickRandomIndex|145_0(this.Moustaches, ref CS$<>8__locals1);
			this.Head.FaceAttachmentIndex = this.<SetAttachments>g__pickRandomIndex|145_0(this.FaceAttachments, ref CS$<>8__locals1);
		}

		// Token: 0x06000262 RID: 610 RVA: 0x00012B34 File Offset: 0x00010D34
		private void SetColors(Rand.RandSync randSync)
		{
			this.Head.HairColor = CharacterInfo.SelectRandomColor(this.HairColors, randSync);
			this.Head.FacialHairColor = CharacterInfo.SelectRandomColor(this.FacialHairColors, randSync);
			this.Head.SkinColor = CharacterInfo.SelectRandomColor(this.SkinColors, randSync);
		}

		// Token: 0x06000263 RID: 611 RVA: 0x00012B88 File Offset: 0x00010D88
		private bool IsColorValid(in Color clr)
		{
			Color color = clr;
			if (color.R == 0)
			{
				color = clr;
				if (color.G == 0)
				{
					color = clr;
					return color.B > 0;
				}
			}
			return true;
		}

		// Token: 0x06000264 RID: 612 RVA: 0x00012BC8 File Offset: 0x00010DC8
		public void CheckColors()
		{
			if (!this.IsColorValid(this.Head.HairColor))
			{
				this.Head.HairColor = CharacterInfo.SelectRandomColor(this.HairColors, Rand.RandSync.Unsynced);
			}
			if (!this.IsColorValid(this.Head.FacialHairColor))
			{
				this.Head.FacialHairColor = CharacterInfo.SelectRandomColor(this.FacialHairColors, Rand.RandSync.Unsynced);
			}
			if (!this.IsColorValid(this.Head.SkinColor))
			{
				this.Head.SkinColor = CharacterInfo.SelectRandomColor(this.SkinColors, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x06000265 RID: 613 RVA: 0x00012C54 File Offset: 0x00010E54
		public CharacterInfo(ContentXElement infoElement, Identifier npcIdentifier = default(Identifier))
		{
			Option.UnspecifiedNone none = Option.None;
			this.LastRewardDistribution = none;
			this.SavedStatValues = new Dictionary<StatTypes, List<SavedStatValue>>();
			this.LastResistanceMultiplierSkillLossDeath = 1f;
			this.LastResistanceMultiplierSkillLossRespawn = 1f;
			base..ctor();
			this.ID = CharacterInfo.idCounter;
			CharacterInfo.idCounter += 1;
			this.Name = infoElement.GetAttributeString("name", "");
			this.OriginalName = infoElement.GetAttributeString("originalname", null);
			this.Salary = infoElement.GetAttributeInt("salary", 1000);
			this.ExperiencePoints = infoElement.GetAttributeInt("experiencepoints", 0);
			this.AdditionalTalentPoints = infoElement.GetAttributeInt("additionaltalentpoints", 0);
			this.TalentResetCount = infoElement.GetAttributeInt("talentResetCount", 0);
			HashSet<Identifier> tags = infoElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			CharacterInfo.LoadTagsBackwardsCompatibility(infoElement, tags);
			this.SpeciesName = infoElement.GetAttributeIdentifier("speciesname", "");
			this.PermanentlyDead = infoElement.GetAttributeBool("permanentlydead", false);
			this.BotStatus = (infoElement.GetAttributeBool("IsOnReserveBench", false) ? BotStatus.ReserveBench : BotStatus.ActiveService);
			this.RenamingEnabled = infoElement.GetAttributeBool("renamingenabled", false);
			Identifier identifier = this.SpeciesName;
			if (identifier.IsEmpty)
			{
				throw new InvalidOperationException("SpeciesName not defined");
			}
			CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(this.SpeciesName);
			ContentXElement element = (characterPrefab != null) ? characterPrefab.ConfigElement : null;
			ContentXElement contentXElement = null;
			if (element == contentXElement)
			{
				return;
			}
			this.CharacterConfigElement = element;
			this.HasSpecifierTags = CharacterInfo.ElementHasSpecifierTags(this.CharacterConfigElement);
			if (this.HasSpecifierTags)
			{
				this.RecreateHead(tags.ToImmutableHashSet<Identifier>(), infoElement.GetAttributeInt("hairindex", -1), infoElement.GetAttributeInt("beardindex", -1), infoElement.GetAttributeInt("moustacheindex", -1), infoElement.GetAttributeInt("faceattachmentindex", -1));
				ContentXElement characterConfigElement = this.CharacterConfigElement;
				string key = "haircolors";
				ValueTuple<Color, float>[] array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(Color.WhiteSmoke, 100f)
				};
				this.HairColors = characterConfigElement.GetAttributeTupleArray<Color, float>(key, array).ToImmutableArray<ValueTuple<Color, float>>();
				ContentXElement characterConfigElement2 = this.CharacterConfigElement;
				string key2 = "facialhaircolors";
				array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(Color.WhiteSmoke, 100f)
				};
				this.FacialHairColors = characterConfigElement2.GetAttributeTupleArray<Color, float>(key2, array).ToImmutableArray<ValueTuple<Color, float>>();
				ContentXElement characterConfigElement3 = this.CharacterConfigElement;
				string key3 = "skincolors";
				array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(new Color(255, 215, 200, 255), 100f)
				};
				this.SkinColors = characterConfigElement3.GetAttributeTupleArray<Color, float>(key3, array).ToImmutableArray<ValueTuple<Color, float>>();
				CharacterInfo.HeadInfo headInfo = this.Head;
				string key4 = "skincolor";
				Color transparent = Color.Transparent;
				headInfo.SkinColor = infoElement.GetAttributeColor(key4, transparent);
				CharacterInfo.HeadInfo headInfo2 = this.Head;
				string key5 = "haircolor";
				transparent = Color.Transparent;
				headInfo2.HairColor = infoElement.GetAttributeColor(key5, transparent);
				CharacterInfo.HeadInfo headInfo3 = this.Head;
				string key6 = "facialhaircolor";
				transparent = Color.Transparent;
				headInfo3.FacialHairColor = infoElement.GetAttributeColor(key6, transparent);
				this.CheckColors();
				this.TryLoadNameAndTitle(npcIdentifier);
				if (string.IsNullOrEmpty(this.Name))
				{
					ContentXElement nameElement = this.CharacterConfigElement.GetChildElement("names");
					contentXElement = null;
					if (nameElement != contentXElement)
					{
						this.GetName(Rand.RandSync.ServerAndClient, out this.Name);
					}
				}
			}
			if (string.IsNullOrEmpty(this.OriginalName))
			{
				this.OriginalName = this.Name;
			}
			this.StartItemsGiven = infoElement.GetAttributeBool("startitemsgiven", false);
			Identifier personalityName = infoElement.GetAttributeIdentifier("personality", "");
			if (personalityName != Identifier.Empty)
			{
				NPCPersonalityTrait trait;
				if (!NPCPersonalityTrait.Traits.TryGet(personalityName, out trait))
				{
					PrefabCollection<NPCPersonalityTrait> traits = NPCPersonalityTrait.Traits;
					identifier = " ".ToIdentifier();
					if (!traits.TryGet(personalityName.Replace(identifier, Identifier.Empty), out trait))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error in CharacterInfo \"");
						defaultInterpolatedStringHandler.AppendFormatted(this.OriginalName);
						defaultInterpolatedStringHandler.AppendLiteral("\": could not find a personality trait with the identifier \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(personalityName);
						defaultInterpolatedStringHandler.AppendLiteral("\".");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						goto IL_490;
					}
				}
				this.PersonalityTrait = trait;
			}
			IL_490:
			this.HumanPrefabIds = new ValueTuple<Identifier, Identifier>(infoElement.GetAttributeIdentifier("npcsetid", Identifier.Empty), infoElement.GetAttributeIdentifier("npcid", Identifier.Empty));
			this.MissionsCompletedSinceDeath = infoElement.GetAttributeInt("missionscompletedsincedeath", 0);
			this.UnlockedTalents = new HashSet<Identifier>();
			this.MinReputationToHire = new ValueTuple<Identifier, float>(infoElement.GetAttributeIdentifier("factionId", Identifier.Empty), infoElement.GetAttributeFloat("minreputation", 0f));
			foreach (ContentXElement subElement in infoElement.Elements())
			{
				bool jobCreated = false;
				Identifier elementName = subElement.Name.ToIdentifier<XName>();
				if (elementName == "job" && !jobCreated)
				{
					this.Job = new Job(subElement);
				}
				else
				{
					if (elementName == "savedstatvalues")
					{
						using (IEnumerator<ContentXElement> enumerator2 = subElement.Elements().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								ContentXElement cxe = enumerator2.Current;
								XElement savedStat = cxe;
								string statTypeString = savedStat.GetAttributeString("stattype", "").ToLowerInvariant();
								StatTypes statType;
								if (!Enum.TryParse<StatTypes>(statTypeString, true, out statType))
								{
									DebugConsole.ThrowError("Invalid stat type type \"" + statTypeString + "\" when loading character data in CharacterInfo!", null, null, false, false);
								}
								else
								{
									float value = savedStat.GetAttributeFloat("statvalue", 0f);
									if (value != 0f)
									{
										Identifier statIdentifier = savedStat.GetAttributeIdentifier("statidentifier", Identifier.Empty);
										if (statIdentifier.IsEmpty)
										{
											DebugConsole.ThrowError("Stat identifier not specified for Stat Value when loading character data in CharacterInfo!", null, null, false, false);
											return;
										}
										bool removeOnDeath = savedStat.GetAttributeBool("removeondeath", true);
										this.ChangeSavedStatValue(statType, value, statIdentifier, removeOnDeath, float.MaxValue, false);
									}
								}
							}
							continue;
						}
					}
					if (elementName == "talents")
					{
						Version version = subElement.GetAttributeVersion("version", GameMain.Version);
						foreach (ContentXElement cxe2 in subElement.Elements())
						{
							XElement talentElement = cxe2;
							identifier = talentElement.Name.ToIdentifier<XName>();
							if (!(identifier != "talent"))
							{
								Identifier talentIdentifier = talentElement.GetAttributeIdentifier("identifier", Identifier.Empty);
								if (!(talentIdentifier == Identifier.Empty))
								{
									TalentPrefab prefab;
									if (TalentPrefab.TalentPrefabs.TryGet(talentIdentifier, out prefab))
									{
										foreach (TalentMigration migration in prefab.Migrations)
										{
											migration.TryApply(version, this);
										}
									}
									this.UnlockedTalents.Add(talentIdentifier);
									if (talentElement.GetAttributeBool("resettable", false))
									{
										this.ResettableExtraTalents.Add(talentIdentifier);
									}
								}
							}
						}
					}
				}
			}
			this.TalentRefundPoints = infoElement.GetAttributeInt("refundpoints", 0);
			int loadedLastRewardDistribution = infoElement.GetAttributeInt("lastrewarddistribution", -1);
			if (loadedLastRewardDistribution >= 0)
			{
				this.LastRewardDistribution = Option.Some<int>(loadedLastRewardDistribution);
			}
			this.LoadHeadAttachments();
		}

		// Token: 0x06000266 RID: 614 RVA: 0x00013484 File Offset: 0x00011684
		private void TryLoadNameAndTitle(Identifier npcIdentifier)
		{
			if (!npcIdentifier.IsEmpty)
			{
				this.Title = TextManager.Get("npctitle." + npcIdentifier.ToString());
				string nameTag = "charactername." + npcIdentifier.ToString();
				if (TextManager.ContainsTag(nameTag))
				{
					this.Name = TextManager.Get(nameTag).Value;
				}
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x06000267 RID: 615 RVA: 0x000134ED File Offset: 0x000116ED
		public IReadOnlyList<ContentXElement> Hairs
		{
			get
			{
				return this.hairs;
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x06000268 RID: 616 RVA: 0x000134F5 File Offset: 0x000116F5
		public IReadOnlyList<ContentXElement> Beards
		{
			get
			{
				return this.beards;
			}
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000269 RID: 617 RVA: 0x000134FD File Offset: 0x000116FD
		public IReadOnlyList<ContentXElement> Moustaches
		{
			get
			{
				return this.moustaches;
			}
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x0600026A RID: 618 RVA: 0x00013505 File Offset: 0x00011705
		public IReadOnlyList<ContentXElement> FaceAttachments
		{
			get
			{
				return this.faceAttachments;
			}
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x0600026B RID: 619 RVA: 0x00013510 File Offset: 0x00011710
		public IEnumerable<ContentXElement> Wearables
		{
			get
			{
				if (this.wearables == null)
				{
					ContentXElement attachments = this.CharacterConfigElement.GetChildElement("HeadAttachments");
					ContentXElement contentXElement = null;
					if (attachments != contentXElement)
					{
						this.wearables = attachments.GetChildElements("Wearable");
					}
				}
				return this.wearables;
			}
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0001355A File Offset: 0x0001175A
		public int GetIdentifier()
		{
			return this.GetIdentifierHash(this.Name);
		}

		// Token: 0x0600026D RID: 621 RVA: 0x00013568 File Offset: 0x00011768
		public int GetIdentifierUsingOriginalName()
		{
			return this.GetIdentifierHash(this.OriginalName);
		}

		// Token: 0x0600026E RID: 622 RVA: 0x00013578 File Offset: 0x00011778
		private int GetIdentifierHash(string name)
		{
			int id = ToolBox.StringToInt(name + string.Join<Identifier>("", from s in this.Head.Preset.TagSet
			orderby s
			select s));
			id ^= this.Head.HairIndex << 12;
			id ^= this.Head.BeardIndex << 18;
			id ^= this.Head.MoustacheIndex << 24;
			id ^= this.Head.FaceAttachmentIndex << 30;
			if (this.Job != null)
			{
				id ^= ToolBox.StringToInt(this.Job.Prefab.Identifier.Value);
			}
			return id;
		}

		// Token: 0x0600026F RID: 623 RVA: 0x0001363C File Offset: 0x0001183C
		public IEnumerable<ContentXElement> FilterElements(IEnumerable<ContentXElement> elements, ImmutableHashSet<Identifier> tags, WearableType? targetType = null)
		{
			if (elements == null)
			{
				return null;
			}
			return elements.Where(delegate(ContentXElement w)
			{
				WearableType type;
				if (targetType != null && Enum.TryParse<WearableType>(w.GetAttributeString("type", ""), true, out type))
				{
					WearableType wearableType = type;
					WearableType? targetType2 = targetType;
					if (!(wearableType == targetType2.GetValueOrDefault() & targetType2 != null))
					{
						return false;
					}
				}
				HashSet<Identifier> t = w.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
				CharacterInfo.LoadTagsBackwardsCompatibility(w, t);
				return t.IsSubsetOf(tags);
			});
		}

		// Token: 0x06000270 RID: 624 RVA: 0x00013674 File Offset: 0x00011874
		public void RecreateHead(ImmutableHashSet<Identifier> tags, int hairIndex, int beardIndex, int moustacheIndex, int faceAttachmentIndex)
		{
			CharacterInfo.HeadPreset headPreset = this.Prefab.Heads.FirstOrDefault((CharacterInfo.HeadPreset h) => h.TagSet.SetEquals(tags));
			if (headPreset == null)
			{
				if (tags.Count == 1)
				{
					headPreset = this.Prefab.Heads.FirstOrDefault((CharacterInfo.HeadPreset h) => h.TagSet.Contains(tags.First<Identifier>()));
				}
				if (headPreset == null)
				{
					headPreset = this.Prefab.Heads.GetRandomUnsynced<CharacterInfo.HeadPreset>();
				}
			}
			this.head = new CharacterInfo.HeadInfo(this, headPreset, hairIndex, beardIndex, moustacheIndex, faceAttachmentIndex);
			this.ReloadHeadAttachments();
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0001370B File Offset: 0x0001190B
		public string ReplaceVars(string str)
		{
			if (this.Head == null)
			{
				return str;
			}
			return this.Prefab.ReplaceVars(str, this.Head.Preset);
		}

		// Token: 0x06000272 RID: 626 RVA: 0x00013730 File Offset: 0x00011930
		public void RecreateHead(CharacterInfo.HeadInfo headInfo)
		{
			this.RecreateHead(headInfo.Preset.TagSet, headInfo.HairIndex, headInfo.BeardIndex, headInfo.MoustacheIndex, headInfo.FaceAttachmentIndex);
			this.Head.SkinColor = headInfo.SkinColor;
			this.Head.HairColor = headInfo.HairColor;
			this.Head.FacialHairColor = headInfo.FacialHairColor;
			this.CheckColors();
		}

		// Token: 0x06000273 RID: 627 RVA: 0x0001379F File Offset: 0x0001199F
		public void RefreshHead()
		{
			this.ReloadHeadAttachments();
			this.RefreshHeadSprites();
		}

		// Token: 0x06000274 RID: 628 RVA: 0x000137AD File Offset: 0x000119AD
		public void VerifySpriteTagsLoaded()
		{
			if (!this.spriteTagsLoaded)
			{
				this.LoadSpriteTags();
			}
		}

		// Token: 0x06000275 RID: 629 RVA: 0x000137BD File Offset: 0x000119BD
		private void LoadHeadSprite()
		{
			this.LoadHeadElement(true, true);
		}

		// Token: 0x06000276 RID: 630 RVA: 0x000137C7 File Offset: 0x000119C7
		private void LoadSpriteTags()
		{
			this.LoadHeadElement(false, true);
		}

		// Token: 0x06000277 RID: 631 RVA: 0x000137D4 File Offset: 0x000119D4
		private void LoadHeadElement(bool loadHeadSprite, bool loadHeadSpriteTags)
		{
			RagdollParams ragdollParams = this.Ragdoll;
			ContentXElement contentXElement = (ragdollParams != null) ? ragdollParams.MainElement : null;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				return;
			}
			foreach (ContentXElement limbElement in this.Ragdoll.MainElement.Elements())
			{
				if (limbElement.GetAttributeString("type", string.Empty).Equals("head", StringComparison.OrdinalIgnoreCase))
				{
					ContentXElement spriteElement = limbElement.GetChildElement("sprite");
					contentXElement = null;
					if (!(spriteElement == contentXElement))
					{
						ContentPath attributeContentPath = spriteElement.GetAttributeContentPath("texture");
						string spritePath = (attributeContentPath != null) ? attributeContentPath.Value : null;
						if (!string.IsNullOrEmpty(spritePath))
						{
							spritePath = this.ReplaceVars(spritePath);
							string fileName = Path.GetFileNameWithoutExtension(spritePath);
							if (!string.IsNullOrEmpty(fileName))
							{
								foreach (string file in Directory.GetFiles(Path.GetDirectoryName(spritePath)))
								{
									if (file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
									{
										string fileWithoutTags = Path.GetFileNameWithoutExtension(file);
										fileWithoutTags = fileWithoutTags.Split(new char[]
										{
											'[',
											']'
										}).First<string>();
										if (!(fileWithoutTags != fileName))
										{
											if (loadHeadSprite)
											{
												this.HeadSprite = new Sprite(spriteElement, "", file, false, 1f);
												this.Portrait = new Sprite(spriteElement, "", file, false, 1f)
												{
													RelativeOrigin = Vector2.Zero
												};
											}
											if (loadHeadSpriteTags)
											{
												this.SpriteTags = (from id in file.Split(new char[]
												{
													'[',
													']'
												}).Skip(1)
												select id.ToIdentifier()).ToList<Identifier>();
												if (this.SpriteTags.Any<Identifier>())
												{
													this.SpriteTags.RemoveAt(this.SpriteTags.Count - 1);
												}
												this.spriteTagsLoaded = true;
												break;
											}
											break;
										}
									}
								}
								if (loadHeadSprite)
								{
									break;
								}
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000278 RID: 632 RVA: 0x00013A18 File Offset: 0x00011C18
		public void LoadHeadAttachments()
		{
			if (this.Wearables != null)
			{
				if (this.hairs == null)
				{
					float commonness = 0.1f;
					this.hairs = CharacterInfo.AddEmpty(this.FilterElements(this.wearables, this.head.Preset.TagSet, new WearableType?(WearableType.Hair)), WearableType.Hair, commonness);
				}
				if (this.beards == null)
				{
					this.beards = CharacterInfo.AddEmpty(this.FilterElements(this.wearables, this.head.Preset.TagSet, new WearableType?(WearableType.Beard)), WearableType.Beard, 1f);
				}
				if (this.moustaches == null)
				{
					this.moustaches = CharacterInfo.AddEmpty(this.FilterElements(this.wearables, this.head.Preset.TagSet, new WearableType?(WearableType.Moustache)), WearableType.Moustache, 1f);
				}
				if (this.faceAttachments == null)
				{
					this.faceAttachments = CharacterInfo.AddEmpty(this.FilterElements(this.wearables, this.head.Preset.TagSet, new WearableType?(WearableType.FaceAttachment)), WearableType.FaceAttachment, 1f);
				}
			}
		}

		// Token: 0x06000279 RID: 633 RVA: 0x00013B20 File Offset: 0x00011D20
		public static List<ContentXElement> AddEmpty(IEnumerable<ContentXElement> elements, WearableType type, float commonness = 1f)
		{
			ContentXElement emptyElement = new XElement("EmptyWearable", new object[]
			{
				type.ToString(),
				new XAttribute("commonness", commonness)
			}).FromPackage(null);
			List<ContentXElement> list = new List<ContentXElement>
			{
				emptyElement
			};
			list.AddRange(elements);
			return list;
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00013B88 File Offset: 0x00011D88
		public ContentXElement GetRandomElement(IEnumerable<ContentXElement> elements)
		{
			IEnumerable<ContentXElement> filtered = elements.Where(new Func<ContentXElement, bool>(this.IsWearableAllowed));
			if (filtered.Count<ContentXElement>() == 0)
			{
				return null;
			}
			ContentXElement element = ToolBox.SelectWeightedRandom<ContentXElement>(filtered.ToList<ContentXElement>(), CharacterInfo.GetWeights(filtered).ToList<float>(), Rand.RandSync.Unsynced);
			ContentXElement contentXElement = null;
			if (!(element == contentXElement))
			{
				Identifier identifier = element.NameAsIdentifier();
				if (!(identifier == "Empty"))
				{
					return element;
				}
			}
			return null;
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00013BF0 File Offset: 0x00011DF0
		private bool IsWearableAllowed(ContentXElement element)
		{
			string spriteName = element.GetChildElement("sprite").GetAttributeString("name", string.Empty);
			return this.IsAllowed(this.Head.HairElement, spriteName) && this.IsAllowed(this.Head.BeardElement, spriteName) && this.IsAllowed(this.Head.MoustacheElement, spriteName) && this.IsAllowed(this.Head.FaceAttachment, spriteName);
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00013C7C File Offset: 0x00011E7C
		private bool IsAllowed(XElement element, string spriteName)
		{
			if (element != null)
			{
				string[] disallowed = element.GetAttributeStringArray("disallow", Array.Empty<string>(), true, false);
				if (disallowed.Any((string s) => spriteName.Contains(s)))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00013CC3 File Offset: 0x00011EC3
		public static bool IsValidIndex(int index, List<ContentXElement> list)
		{
			return index >= 0 && index < list.Count;
		}

		// Token: 0x0600027E RID: 638 RVA: 0x00013CD4 File Offset: 0x00011ED4
		private static IEnumerable<float> GetWeights(IEnumerable<ContentXElement> elements)
		{
			return from h in elements
			select h.GetAttributeFloat("commonness", 1f);
		}

		// Token: 0x0600027F RID: 639 RVA: 0x00013CFC File Offset: 0x00011EFC
		public int CalculateSalary(int baseSalary = 0, float salaryMultiplier = 1f)
		{
			if (this.Name == null || this.Job == null)
			{
				return 0;
			}
			int salary = 0;
			foreach (Skill skill in this.Job.GetSkills())
			{
				salary += (int)(skill.Level * skill.PriceMultiplier);
			}
			return (int)((float)baseSalary + (float)salary * this.Job.Prefab.PriceMultiplier * salaryMultiplier);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x00013D88 File Offset: 0x00011F88
		public void ApplySkillGain(Identifier skillIdentifier, float baseGain, bool gainedFromAbility = false, float maxGain = 2f, bool forceNotification = false)
		{
			float skillLevel = this.Job.GetSkillLevel(skillIdentifier);
			float skillDivider = MathF.Pow(Math.Max(skillLevel, 15f), SkillSettings.Current.SkillIncreaseExponent);
			this.IncreaseSkillLevel(skillIdentifier, Math.Min(baseGain / skillDivider, maxGain), gainedFromAbility, forceNotification);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x00013DD4 File Offset: 0x00011FD4
		public void IncreaseSkillLevel(Identifier skillIdentifier, float increase, bool gainedFromAbility = false, bool forceNotification = false)
		{
			if (this.Job == null || (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient) || this.Character == null)
			{
				return;
			}
			if (this.Job.Prefab.Identifier == "assistant")
			{
				increase *= SkillSettings.Current.AssistantSkillIncreaseMultiplier;
			}
			increase *= 1f + this.Character.GetStatValue(StatTypes.SkillGainSpeed, true);
			increase = this.GetSkillSpecificGain(increase, skillIdentifier);
			float prevLevel = this.Job.GetSkillLevel(skillIdentifier);
			this.Job.IncreaseSkillLevel(skillIdentifier, increase, this.Character.HasAbilityFlag(AbilityFlags.GainSkillPastMaximum));
			float newLevel = this.Job.GetSkillLevel(skillIdentifier);
			if ((int)newLevel > (int)prevLevel)
			{
				float extraLevel = this.Character.GetStatValue(StatTypes.ExtraLevelGain, true);
				this.Job.IncreaseSkillLevel(skillIdentifier, extraLevel, this.Character.HasAbilityFlag(AbilityFlags.GainSkillPastMaximum));
				float increaseSinceLastSkillPoint = MathHelper.Max(increase, 1f);
				AbilitySkillGain abilitySkillGain = new AbilitySkillGain(increaseSinceLastSkillPoint, skillIdentifier, this.Character, gainedFromAbility);
				this.Character.CheckTalents(AbilityEffectType.OnGainSkillPoint, abilitySkillGain);
				foreach (Character character in Character.GetFriendlyCrew(this.Character))
				{
					character.CheckTalents(AbilityEffectType.OnAllyGainSkillPoint, abilitySkillGain);
				}
			}
			this.OnSkillChanged(skillIdentifier, prevLevel, newLevel, forceNotification);
		}

		// Token: 0x06000282 RID: 642 RVA: 0x00013F40 File Offset: 0x00012140
		private float GetSkillSpecificGain(float increase, Identifier skillIdentifier)
		{
			StatTypes statType;
			if (CharacterInfo.skillGainStatValues.TryGetValue(skillIdentifier, out statType))
			{
				increase *= 1f + this.Character.GetStatValue(statType, true);
			}
			return increase;
		}

		// Token: 0x06000283 RID: 643 RVA: 0x00013F74 File Offset: 0x00012174
		public void SetSkillLevel(Identifier skillIdentifier, float level, bool forceNotification = false)
		{
			if (this.Job == null)
			{
				return;
			}
			Skill skill = this.Job.GetSkill(skillIdentifier);
			if (skill == null)
			{
				this.Job.IncreaseSkillLevel(skillIdentifier, level, false);
				this.OnSkillChanged(skillIdentifier, 0f, level, forceNotification);
				return;
			}
			float prevLevel = skill.Level;
			skill.Level = level;
			this.OnSkillChanged(skillIdentifier, prevLevel, skill.Level, forceNotification);
		}

		// Token: 0x06000284 RID: 644 RVA: 0x00013FD4 File Offset: 0x000121D4
		private void OnSkillChanged(Identifier skillIdentifier, float prevLevel, float newLevel, bool forceNotification)
		{
			if (this.Character == null || this.Character.Removed)
			{
				return;
			}
			if (!this.prevSentSkill.ContainsKey(skillIdentifier))
			{
				this.prevSentSkill[skillIdentifier] = prevLevel;
			}
			if (Math.Abs(this.prevSentSkill[skillIdentifier] - newLevel) > 0.1f || forceNotification)
			{
				GameMain.NetworkMember.CreateEntityEvent(this.Character, new Character.UpdateSkillsEventData(skillIdentifier, forceNotification));
				this.prevSentSkill[skillIdentifier] = newLevel;
			}
		}

		// Token: 0x06000285 RID: 645 RVA: 0x0001405C File Offset: 0x0001225C
		public void GiveExperience(int amount)
		{
			int prevAmount = this.ExperiencePoints;
			AbilityExperienceGainMultiplier experienceGainMultiplier = new AbilityExperienceGainMultiplier(1f);
			AbilityExperienceGainMultiplier abilityExperienceGainMultiplier = experienceGainMultiplier;
			float value = abilityExperienceGainMultiplier.Value;
			Character character = this.Character;
			abilityExperienceGainMultiplier.Value = value + ((character != null) ? character.GetStatValue(StatTypes.ExperienceGainMultiplier, true) : 0f);
			amount = (int)((float)amount * experienceGainMultiplier.Value);
			if (amount < 0)
			{
				return;
			}
			this.ExperiencePoints += amount;
			this.OnExperienceChanged(prevAmount, this.ExperiencePoints);
		}

		// Token: 0x06000286 RID: 646 RVA: 0x000140D0 File Offset: 0x000122D0
		public void SetExperience(int newExperience)
		{
			if (newExperience < 0)
			{
				return;
			}
			int prevAmount = this.ExperiencePoints;
			this.ExperiencePoints = newExperience;
			this.OnExperienceChanged(prevAmount, this.ExperiencePoints);
		}

		// Token: 0x06000287 RID: 647 RVA: 0x000140FD File Offset: 0x000122FD
		public int GetTotalTalentPoints()
		{
			return this.GetCurrentLevel() + this.AdditionalTalentPoints;
		}

		// Token: 0x06000288 RID: 648 RVA: 0x0001410C File Offset: 0x0001230C
		public int GetAvailableTalentPoints()
		{
			return Math.Max(this.GetTotalTalentPoints() - this.GetUnlockedTalentsInTree().Count<Identifier>(), 0);
		}

		// Token: 0x06000289 RID: 649 RVA: 0x00014126 File Offset: 0x00012326
		public float GetProgressTowardsNextLevel()
		{
			return (float)(this.ExperiencePoints - this.GetExperienceRequiredForCurrentLevel()) / (float)(this.GetExperienceRequiredToLevelUp() - this.GetExperienceRequiredForCurrentLevel());
		}

		// Token: 0x0600028A RID: 650 RVA: 0x00014148 File Offset: 0x00012348
		public int GetExperienceRequiredForCurrentLevel()
		{
			int experienceRequired;
			this.GetCurrentLevel(out experienceRequired);
			return experienceRequired;
		}

		// Token: 0x0600028B RID: 651 RVA: 0x00014160 File Offset: 0x00012360
		public int GetExperienceRequiredToLevelUp()
		{
			int experienceRequired;
			int level = this.GetCurrentLevel(out experienceRequired);
			return experienceRequired + CharacterInfo.ExperienceRequiredPerLevel(level);
		}

		// Token: 0x0600028C RID: 652 RVA: 0x00014180 File Offset: 0x00012380
		public int GetExperienceRequiredForLevel(int level)
		{
			int currentLevel = this.GetCurrentLevel();
			if (currentLevel >= level)
			{
				return 0;
			}
			int required = 0;
			for (int i = 0; i < level; i++)
			{
				required += CharacterInfo.ExperienceRequiredPerLevel(i);
			}
			return required - this.ExperiencePoints;
		}

		// Token: 0x0600028D RID: 653 RVA: 0x000141BC File Offset: 0x000123BC
		public int GetCurrentLevel()
		{
			int num;
			return this.GetCurrentLevel(out num);
		}

		// Token: 0x0600028E RID: 654 RVA: 0x000141D4 File Offset: 0x000123D4
		private int GetCurrentLevel(out int experienceRequired)
		{
			int level = 0;
			experienceRequired = 0;
			while (experienceRequired + CharacterInfo.ExperienceRequiredPerLevel(level) <= this.ExperiencePoints)
			{
				experienceRequired += CharacterInfo.ExperienceRequiredPerLevel(level);
				level++;
			}
			return Math.Max(level, 0);
		}

		// Token: 0x0600028F RID: 655 RVA: 0x0001420F File Offset: 0x0001240F
		public static int ExperienceRequiredPerLevel(int level)
		{
			return 450 + 500 * level;
		}

		// Token: 0x06000290 RID: 656 RVA: 0x00014220 File Offset: 0x00012420
		private void OnExperienceChanged(int prevAmount, int newAmount)
		{
			if (this.Character == null || this.Character.Removed)
			{
				return;
			}
			if (prevAmount != newAmount)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 4);
				defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(this.Character));
				defaultInterpolatedStringHandler.AppendLiteral(" has gained ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(newAmount - prevAmount);
				defaultInterpolatedStringHandler.AppendLiteral(" experience (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(prevAmount);
				defaultInterpolatedStringHandler.AppendLiteral(" -> ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(newAmount);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Talent);
				GameMain.NetworkMember.CreateEntityEvent(this.Character, default(Character.UpdateExperienceEventData));
			}
		}

		// Token: 0x06000291 RID: 657 RVA: 0x000142DC File Offset: 0x000124DC
		private void OnPermanentStatChanged(StatTypes statType)
		{
			if (this.Character == null || this.Character.Removed)
			{
				return;
			}
			GameMain.NetworkMember.CreateEntityEvent(this.Character, new Character.UpdatePermanentStatsEventData(statType));
		}

		// Token: 0x06000292 RID: 658 RVA: 0x00014310 File Offset: 0x00012510
		public void RefundTalents()
		{
			if (this.TalentRefundPoints <= 0)
			{
				return;
			}
			List<Identifier> talentsFromOutsideTree = this.GetUnlockedTalentsOutsideTree().ToList<Identifier>();
			foreach (Identifier resettableExtraTalent in this.ResettableExtraTalents)
			{
				talentsFromOutsideTree.Remove(resettableExtraTalent);
			}
			this.UnlockedTalents.Clear();
			this.SavedStatValues.Clear();
			Character character = this.Character;
			if (character != null)
			{
				character.ResetTalents(this.talentResetCount);
			}
			int num = this.TalentRefundPoints;
			this.TalentRefundPoints = num - 1;
			this.talentResetCount++;
			if (this.Character == null)
			{
				talentsFromOutsideTree.ForEach(delegate(Identifier talentId)
				{
					this.UnlockedTalents.Add(talentId);
				});
			}
			else
			{
				talentsFromOutsideTree.ForEach(delegate(Identifier talentId)
				{
					this.Character.GiveTalent(talentId, true);
				});
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null)
			{
				return;
			}
			networkMember.CreateEntityEvent(this.Character, default(Character.ConfirmRefundEventData));
		}

		// Token: 0x06000293 RID: 659 RVA: 0x00014418 File Offset: 0x00012618
		public void AddRefundPoints(int newRefundPoints)
		{
			this.TalentRefundPoints += newRefundPoints;
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null)
			{
				return;
			}
			networkMember.CreateEntityEvent(this.Character, default(Character.UpdateRefundPointsEventData));
		}

		// Token: 0x06000294 RID: 660 RVA: 0x00014458 File Offset: 0x00012658
		public void Rename(string newName)
		{
			if (string.IsNullOrEmpty(newName))
			{
				return;
			}
			newName = Client.SanitizeName(newName, 32);
			foreach (Item item in Item.ItemList)
			{
				if (item.HasTag("identitycard".ToIdentifier()) || item.HasTag("despawncontainer".ToIdentifier()))
				{
					string[] array = item.Tags.Split(',', StringSplitOptions.None);
					int i = 0;
					while (i < array.Length)
					{
						string tag = array[i];
						string[] splitTag = tag.Split(":", StringSplitOptions.None);
						if (splitTag.Length >= 2 && !(splitTag[0] != "name") && !(splitTag[1] != this.Name))
						{
							item.ReplaceTag(tag, "name:" + newName);
							IdCard idCard = item.GetComponent<IdCard>();
							if (idCard != null)
							{
								idCard.OwnerName = newName;
								break;
							}
							break;
						}
						else
						{
							i++;
						}
					}
				}
			}
			this.Name = newName;
		}

		// Token: 0x06000295 RID: 661 RVA: 0x0001456C File Offset: 0x0001276C
		public void ResetName()
		{
			this.Name = this.OriginalName;
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0001457C File Offset: 0x0001277C
		public XElement Save(XElement parentElement)
		{
			XElement charElement = new XElement("Character");
			XContainer xcontainer = charElement;
			object[] array = new object[22];
			array[0] = new XAttribute("name", this.Name);
			array[1] = new XAttribute("originalname", this.OriginalName);
			array[2] = new XAttribute("speciesname", this.SpeciesName);
			array[3] = new XAttribute("tags", string.Join<Identifier>(",", this.Head.Preset.TagSet));
			array[4] = new XAttribute("salary", this.Salary);
			array[5] = new XAttribute("experiencepoints", this.ExperiencePoints);
			array[6] = new XAttribute("additionaltalentpoints", this.AdditionalTalentPoints);
			array[7] = new XAttribute("talentResetCount", this.TalentResetCount);
			array[8] = new XAttribute("hairindex", this.Head.HairIndex);
			array[9] = new XAttribute("beardindex", this.Head.BeardIndex);
			array[10] = new XAttribute("moustacheindex", this.Head.MoustacheIndex);
			array[11] = new XAttribute("faceattachmentindex", this.Head.FaceAttachmentIndex);
			array[12] = new XAttribute("skincolor", XMLExtensions.ColorToString(this.Head.SkinColor));
			array[13] = new XAttribute("haircolor", XMLExtensions.ColorToString(this.Head.HairColor));
			array[14] = new XAttribute("facialhaircolor", XMLExtensions.ColorToString(this.Head.FacialHairColor));
			array[15] = new XAttribute("startitemsgiven", this.StartItemsGiven);
			int num = 16;
			XName name = "personality";
			NPCPersonalityTrait personalityTrait = this.PersonalityTrait;
			array[num] = new XAttribute(name, (personalityTrait != null) ? personalityTrait.Identifier : Identifier.Empty);
			array[17] = new XAttribute("refundpoints", this.TalentRefundPoints);
			array[18] = new XAttribute("lastrewarddistribution", this.LastRewardDistribution.Match((int value) => value, () => -1).ToString());
			array[19] = new XAttribute("permanentlydead", this.PermanentlyDead);
			array[20] = new XAttribute("IsOnReserveBench", this.IsOnReserveBench);
			array[21] = new XAttribute("renamingenabled", this.RenamingEnabled);
			xcontainer.Add(array);
			ValueTuple<Identifier, Identifier> humanPrefabIds = this.HumanPrefabIds;
			Identifier identifier = default(Identifier);
			if (!(humanPrefabIds.Item1 != identifier))
			{
				Identifier identifier2 = default(Identifier);
				if (!(humanPrefabIds.Item2 != identifier2))
				{
					goto IL_3AC;
				}
			}
			charElement.Add(new object[]
			{
				new XAttribute("npcsetid", this.HumanPrefabIds.Item1),
				new XAttribute("npcid", this.HumanPrefabIds.Item2)
			});
			IL_3AC:
			charElement.Add(new XAttribute("missionscompletedsincedeath", this.MissionsCompletedSinceDeath));
			if (!this.MinReputationToHire.Item1.IsEmpty)
			{
				charElement.Add(new object[]
				{
					new XAttribute("factionId", this.MinReputationToHire.Item1),
					new XAttribute("minreputation", this.MinReputationToHire.Item2)
				});
			}
			if (this.Character != null && this.Character.AnimController.CurrentHull != null)
			{
				charElement.Add(new XAttribute("hull", this.Character.AnimController.CurrentHull.ID));
			}
			this.Job.Save(charElement);
			XElement savedStatElement = new XElement("savedstatvalues");
			foreach (KeyValuePair<StatTypes, List<SavedStatValue>> statValuePair in this.SavedStatValues)
			{
				foreach (SavedStatValue savedStat in statValuePair.Value)
				{
					if (savedStat.StatValue != 0f)
					{
						savedStatElement.Add(new XElement("savedstatvalue", new object[]
						{
							new XAttribute("stattype", statValuePair.Key.ToString()),
							new XAttribute("statidentifier", savedStat.StatIdentifier),
							new XAttribute("statvalue", savedStat.StatValue),
							new XAttribute("removeondeath", savedStat.RemoveOnDeath)
						}));
					}
				}
			}
			XElement talentElement = new XElement("Talents");
			talentElement.Add(new XAttribute("version", GameMain.Version.ToString()));
			foreach (Identifier talentIdentifier in this.UnlockedTalents)
			{
				talentElement.Add(new XElement("Talent", new object[]
				{
					new XAttribute("identifier", talentIdentifier),
					new XAttribute("resettable", this.ResettableExtraTalents.Contains(talentIdentifier))
				}));
			}
			charElement.Add(savedStatElement);
			charElement.Add(talentElement);
			if (parentElement != null)
			{
				parentElement.Add(charElement);
			}
			return charElement;
		}

		// Token: 0x06000297 RID: 663 RVA: 0x00014C58 File Offset: 0x00012E58
		public static void SaveOrders(XElement parentElement, params Order[] orders)
		{
			if (parentElement == null || orders == null || orders.None(null))
			{
				return;
			}
			int priorityIncrease = 0;
			List<LinkedSubmarine> linkedSubs = CharacterInfo.GetLinkedSubmarines();
			int j = 0;
			while (j < orders.Length)
			{
				Order orderInfo = orders[j];
				Order order = orderInfo;
				if (order == null)
				{
					goto IL_45;
				}
				Identifier identifier = order.Identifier;
				if (identifier == Identifier.Empty)
				{
					goto IL_45;
				}
				int? linkedSubIndex = null;
				bool targetAvailableInNextLevel = true;
				if (order.TargetSpatialEntity != null)
				{
					Submarine entitySub = order.TargetSpatialEntity.Submarine;
					bool isOutside = entitySub == null;
					bool canBeOnLinkedSub = !isOutside && Submarine.MainSub != null && entitySub != Submarine.MainSub && linkedSubs.Any<LinkedSubmarine>();
					bool isOnConnectedLinkedSub = false;
					if (canBeOnLinkedSub)
					{
						for (int i = 0; i < linkedSubs.Count; i++)
						{
							LinkedSubmarine ls = linkedSubs[i];
							if (ls.LoadSub && ls.Sub == entitySub)
							{
								linkedSubIndex = new int?(i);
								isOnConnectedLinkedSub = Submarine.MainSub.GetConnectedSubs().Contains(entitySub);
								break;
							}
						}
					}
					if (isOutside)
					{
						goto IL_13F;
					}
					GameSession gameSession = GameMain.GameSession;
					CampaignMode campaignMode = (gameSession != null) ? gameSession.Campaign : null;
					if (campaignMode != null && campaignMode.SwitchedSubsThisRound)
					{
						goto IL_13F;
					}
					bool flag = isOnConnectedLinkedSub || (Submarine.MainSub != null && entitySub == Submarine.MainSub);
					IL_140:
					targetAvailableInNextLevel = flag;
					if (targetAvailableInNextLevel)
					{
						goto IL_1DE;
					}
					if (!order.Prefab.CanBeGeneralized)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(155, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Trying to save an order (");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(order.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(") targeting an entity that won't be connected to the main sub in the next level. The order requires a target so it won't be saved.");
						DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
						priorityIncrease++;
						goto IL_507;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(147, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Saving an order (");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(order.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral(") targeting an entity that won't be connected to the main sub in the next level. The order will be saved as a generalized version.");
					DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
					goto IL_1DE;
					IL_13F:
					flag = false;
					goto IL_140;
				}
				IL_1DE:
				if (orderInfo.ManualPriority < 1)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(60, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("Error saving an order (");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(order.Identifier);
					defaultInterpolatedStringHandler3.AppendLiteral(") - the order priority is less than 1");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
					priorityIncrease++;
				}
				else
				{
					XElement orderElement = new XElement("order", new object[]
					{
						new XAttribute("id", order.Identifier),
						new XAttribute("priority", orderInfo.ManualPriority + priorityIncrease),
						new XAttribute("targettype", (int)order.TargetType)
					});
					if (orderInfo.Option != Identifier.Empty)
					{
						orderElement.Add(new XAttribute("option", orderInfo.Option));
					}
					if (order.OrderGiver != null)
					{
						XContainer xcontainer = orderElement;
						XName name = "ordergiver";
						CharacterInfo info = order.OrderGiver.Info;
						xcontainer.Add(new XAttribute(name, (info != null) ? new int?(info.GetIdentifier()) : null));
					}
					ISpatialEntity targetSpatialEntity = order.TargetSpatialEntity;
					Submarine targetSub = (targetSpatialEntity != null) ? targetSpatialEntity.Submarine : null;
					if (targetSub != null)
					{
						if (Submarine.MainSub != null && targetSub == Submarine.MainSub)
						{
							orderElement.Add(new XAttribute("onmainsub", true));
						}
						else if (linkedSubIndex != null)
						{
							orderElement.Add(new XAttribute("linkedsubindex", linkedSubIndex));
						}
					}
					switch (order.TargetType)
					{
					case Order.OrderTargetType.Entity:
						if (targetAvailableInNextLevel)
						{
							Entity e = order.TargetEntity;
							if (e != null)
							{
								orderElement.Add(new XAttribute("targetid", (uint)e.ID));
							}
						}
						break;
					case Order.OrderTargetType.Position:
						if (targetAvailableInNextLevel)
						{
							OrderTarget ot = order.TargetSpatialEntity as OrderTarget;
							if (ot != null)
							{
								XElement orderTargetElement = new XElement("ordertarget");
								Vector2 position = ot.WorldPosition;
								if (ot.Hull != null)
								{
									orderTargetElement.Add(new XAttribute("hullid", (uint)ot.Hull.ID));
									position -= ot.Hull.WorldPosition;
								}
								orderTargetElement.Add(new XAttribute("position", XMLExtensions.Vector2ToString(position)));
								orderElement.Add(orderTargetElement);
							}
						}
						break;
					case Order.OrderTargetType.WallSection:
						if (targetAvailableInNextLevel)
						{
							Structure s = order.TargetEntity as Structure;
							if (s != null && order.WallSectionIndex != null)
							{
								orderElement.Add(new XAttribute("structureid", s.ID));
								orderElement.Add(new XAttribute("wallsectionindex", order.WallSectionIndex.Value));
							}
						}
						break;
					}
					parentElement.Add(orderElement);
				}
				IL_507:
				j++;
				continue;
				IL_45:
				DebugConsole.ThrowError("Error saving an order - the order or its identifier is null", null, null, false, false);
				priorityIncrease++;
				goto IL_507;
			}
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0001517C File Offset: 0x0001337C
		public static void SaveOrderData(CharacterInfo characterInfo, XElement parentElement)
		{
			List<Order> currentOrders = new List<Order>(characterInfo.CurrentOrders);
			currentOrders.Sort((Order x, Order y) => y.ManualPriority.CompareTo(x.ManualPriority));
			CharacterInfo.SaveOrders(parentElement, currentOrders.ToArray());
		}

		// Token: 0x06000299 RID: 665 RVA: 0x000151C6 File Offset: 0x000133C6
		public void SaveOrderData()
		{
			this.OrderData = new XElement("orders");
			CharacterInfo.SaveOrderData(this, this.OrderData);
		}

		// Token: 0x0600029A RID: 666 RVA: 0x000151EC File Offset: 0x000133EC
		public static void ApplyOrderData(Character character, XElement orderData)
		{
			if (character == null)
			{
				return;
			}
			List<Order> orders = CharacterInfo.LoadOrders(orderData);
			foreach (Order order in orders)
			{
				character.SetOrder(order, true, false, true);
			}
		}

		// Token: 0x0600029B RID: 667 RVA: 0x00015248 File Offset: 0x00013448
		public void ApplyOrderData()
		{
			CharacterInfo.ApplyOrderData(this.Character, this.OrderData);
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0001525C File Offset: 0x0001345C
		public static List<Order> LoadOrders(XElement ordersElement)
		{
			List<Order> orders = new List<Order>();
			if (ordersElement == null)
			{
				return orders;
			}
			CharacterInfo.<>c__DisplayClass220_0 CS$<>8__locals1;
			CS$<>8__locals1.priorityIncrease = 0;
			CS$<>8__locals1.linkedSubs = CharacterInfo.GetLinkedSubmarines();
			foreach (XElement orderElement in ordersElement.GetChildElements("order", StringComparison.OrdinalIgnoreCase))
			{
				CharacterInfo.<>c__DisplayClass220_1 CS$<>8__locals2;
				CS$<>8__locals2.orderElement = orderElement;
				Order order = null;
				CharacterInfo.<>c__DisplayClass220_2 CS$<>8__locals3;
				CS$<>8__locals3.orderIdentifier = CS$<>8__locals2.orderElement.GetAttributeString("id", "");
				if (!OrderPrefab.Prefabs.TryGet(CS$<>8__locals3.orderIdentifier, out CS$<>8__locals3.orderPrefab))
				{
					DebugConsole.ThrowError("Error loading a previously saved order - can't find an order prefab with the identifier \"" + CS$<>8__locals3.orderIdentifier + "\"", null, null, false, false);
					int priorityIncrease = CS$<>8__locals1.priorityIncrease;
					CS$<>8__locals1.priorityIncrease = priorityIncrease + 1;
				}
				else
				{
					Order.OrderTargetType targetType = (Order.OrderTargetType)CS$<>8__locals2.orderElement.GetAttributeInt("targettype", 0);
					Character orderGiver = null;
					XAttribute orderGiverIdAttribute = CS$<>8__locals2.orderElement.GetAttribute("ordergiver", StringComparison.OrdinalIgnoreCase);
					if (orderGiverIdAttribute != null)
					{
						int orderGiverInfoId = orderGiverIdAttribute.GetAttributeInt(0);
						orderGiver = Character.CharacterList.FirstOrDefault(delegate(Character c)
						{
							CharacterInfo info = c.Info;
							return info != null && info.GetIdentifier() == orderGiverInfoId;
						});
					}
					Entity targetEntity = null;
					switch (targetType)
					{
					case Order.OrderTargetType.Entity:
					{
						ushort targetId = (ushort)CS$<>8__locals2.orderElement.GetAttributeUInt("targetid", 0U);
						if (!CharacterInfo.<LoadOrders>g__GetTargetEntity|220_0(targetId, out targetEntity, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3))
						{
							continue;
						}
						ItemComponent targetComponent = CS$<>8__locals3.orderPrefab.GetTargetItemComponent(targetEntity as Item);
						order = new Order(CS$<>8__locals3.orderPrefab, targetEntity, targetComponent, orderGiver, false);
						break;
					}
					case Order.OrderTargetType.Position:
					{
						XElement orderTargetElement = CS$<>8__locals2.orderElement.GetChildElement("ordertarget", StringComparison.OrdinalIgnoreCase);
						Vector2 position = orderTargetElement.GetAttributeVector2("position", Vector2.Zero);
						ushort hullId = (ushort)orderTargetElement.GetAttributeUInt("hullid", 0U);
						if (!CharacterInfo.<LoadOrders>g__GetTargetEntity|220_0(hullId, out targetEntity, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3))
						{
							continue;
						}
						Hull targetPositionHull = targetEntity as Hull;
						if (targetPositionHull == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Error loading a previously saved order (");
							defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals3.orderIdentifier);
							defaultInterpolatedStringHandler.AppendLiteral(") - entity with the ID ");
							defaultInterpolatedStringHandler.AppendFormatted<ushort>(hullId);
							defaultInterpolatedStringHandler.AppendLiteral(" is of type ");
							defaultInterpolatedStringHandler.AppendFormatted<Type>((targetEntity != null) ? targetEntity.GetType() : null);
							defaultInterpolatedStringHandler.AppendLiteral(" instead of Hull");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
							int priorityIncrease = CS$<>8__locals1.priorityIncrease;
							CS$<>8__locals1.priorityIncrease = priorityIncrease + 1;
							continue;
						}
						OrderTarget orderTarget = new OrderTarget(targetPositionHull.WorldPosition + position, targetPositionHull, false);
						order = new Order(CS$<>8__locals3.orderPrefab, orderTarget, orderGiver);
						break;
					}
					case Order.OrderTargetType.WallSection:
					{
						ushort structureId = (ushort)CS$<>8__locals2.orderElement.GetAttributeInt("structureid", 0);
						if (!CharacterInfo.<LoadOrders>g__GetTargetEntity|220_0(structureId, out targetEntity, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3))
						{
							continue;
						}
						int wallSectionIndex = CS$<>8__locals2.orderElement.GetAttributeInt("wallsectionindex", 0);
						Structure targetStructure = targetEntity as Structure;
						if (targetStructure == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(96, 3);
							defaultInterpolatedStringHandler2.AppendLiteral("Error loading a previously saved order (");
							defaultInterpolatedStringHandler2.AppendFormatted(CS$<>8__locals3.orderIdentifier);
							defaultInterpolatedStringHandler2.AppendLiteral(") - entity with the ID ");
							defaultInterpolatedStringHandler2.AppendFormatted<ushort>(structureId);
							defaultInterpolatedStringHandler2.AppendLiteral(" is of type ");
							defaultInterpolatedStringHandler2.AppendFormatted<Type>((targetEntity != null) ? targetEntity.GetType() : null);
							defaultInterpolatedStringHandler2.AppendLiteral(" instead of Structure");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
							int priorityIncrease = CS$<>8__locals1.priorityIncrease;
							CS$<>8__locals1.priorityIncrease = priorityIncrease + 1;
							continue;
						}
						order = new Order(CS$<>8__locals3.orderPrefab, targetStructure, new int?(wallSectionIndex), orderGiver);
						break;
					}
					}
					Identifier orderOption = CS$<>8__locals2.orderElement.GetAttributeIdentifier("option", "");
					int manualPriority = CS$<>8__locals2.orderElement.GetAttributeInt("priority", 0) + CS$<>8__locals1.priorityIncrease;
					Order orderInfo = order.WithOption(orderOption).WithManualPriority(manualPriority);
					orders.Add(orderInfo);
				}
			}
			return orders;
		}

		// Token: 0x0600029D RID: 669 RVA: 0x00015670 File Offset: 0x00013870
		private static List<LinkedSubmarine> GetLinkedSubmarines()
		{
			return (from ls in Entity.GetEntities().OfType<LinkedSubmarine>()
			where ls.Submarine == Submarine.MainSub
			select ls into e
			orderby e.ID
			select e).ToList<LinkedSubmarine>();
		}

		// Token: 0x0600029E RID: 670 RVA: 0x000156D4 File Offset: 0x000138D4
		private static ushort GetOffsetId(Submarine parentSub, ushort id)
		{
			if (parentSub != null)
			{
				IdRemap idRemap = new IdRemap(parentSub.Info.SubmarineElement, (int)parentSub.IdOffset);
				return idRemap.GetOffsetId((int)id);
			}
			return id;
		}

		// Token: 0x0600029F RID: 671 RVA: 0x00015704 File Offset: 0x00013904
		public static void ApplyHealthData(Character character, XElement healthData, Func<AfflictionPrefab, bool> afflictionPredicate = null)
		{
			if (healthData != null && character != null)
			{
				character.CharacterHealth.Load(healthData, afflictionPredicate);
			}
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x00015719 File Offset: 0x00013919
		public void ReloadHeadAttachments()
		{
			this.ResetLoadedAttachments();
			this.LoadHeadAttachments();
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x00015727 File Offset: 0x00013927
		private void ResetAttachmentIndices()
		{
			this.Head.ResetAttachmentIndices();
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x00015734 File Offset: 0x00013934
		private void ResetLoadedAttachments()
		{
			this.hairs = null;
			this.beards = null;
			this.moustaches = null;
			this.faceAttachments = null;
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x00015752 File Offset: 0x00013952
		public void ClearCurrentOrders()
		{
			this.CurrentOrders.Clear();
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x0001575F File Offset: 0x0001395F
		public void Remove()
		{
			this.Character = null;
			this.HeadSprite = null;
			this.Portrait = null;
			this.AttachmentSprites = null;
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0001577D File Offset: 0x0001397D
		private void RefreshHeadSprites()
		{
			this._headSprite = null;
			this.LoadHeadSprite();
			List<WearableSprite> list = this.attachmentSprites;
			if (list == null)
			{
				return;
			}
			list.Clear();
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x0001579C File Offset: 0x0001399C
		public void ClearSavedStatValues()
		{
			foreach (StatTypes statType in this.SavedStatValues.Keys)
			{
				this.OnPermanentStatChanged(statType);
			}
			this.SavedStatValues.Clear();
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x00015800 File Offset: 0x00013A00
		public void ClearSavedStatValues(StatTypes statType)
		{
			this.SavedStatValues.Remove(statType);
			this.OnPermanentStatChanged(statType);
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x00015818 File Offset: 0x00013A18
		public void RemoveSavedStatValuesOnDeath()
		{
			foreach (StatTypes statType in this.SavedStatValues.Keys)
			{
				foreach (SavedStatValue savedStatValue in this.SavedStatValues[statType])
				{
					if (savedStatValue.RemoveOnDeath && !MathUtils.NearlyEqual(savedStatValue.StatValue, 0f, 0.0001f))
					{
						savedStatValue.StatValue = 0f;
					}
				}
			}
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x000158D4 File Offset: 0x00013AD4
		public void ResetSavedStatValue(Identifier statIdentifier)
		{
			foreach (StatTypes statType in this.SavedStatValues.Keys)
			{
				bool changed = false;
				foreach (SavedStatValue savedStatValue in this.SavedStatValues[statType])
				{
					if (CharacterInfo.<ResetSavedStatValue>g__MatchesIdentifier|234_0(savedStatValue.StatIdentifier, statIdentifier) && !MathUtils.NearlyEqual(savedStatValue.StatValue, 0f, 0.0001f))
					{
						savedStatValue.StatValue = 0f;
						changed = true;
					}
				}
				if (changed)
				{
					this.OnPermanentStatChanged(statType);
				}
			}
		}

		// Token: 0x060002AA RID: 682 RVA: 0x000159AC File Offset: 0x00013BAC
		public float GetSavedStatValue(StatTypes statType)
		{
			List<SavedStatValue> statValues;
			if (this.SavedStatValues.TryGetValue(statType, out statValues))
			{
				return statValues.Sum((SavedStatValue v) => v.StatValue);
			}
			return 0f;
		}

		// Token: 0x060002AB RID: 683 RVA: 0x000159F4 File Offset: 0x00013BF4
		public float GetSavedStatValue(StatTypes statType, Identifier statIdentifier)
		{
			List<SavedStatValue> statValues;
			if (this.SavedStatValues.TryGetValue(statType, out statValues))
			{
				return (from value in statValues
				where ToolBox.StatIdentifierMatches(value.StatIdentifier, statIdentifier)
				select value).Sum((SavedStatValue v) => v.StatValue);
			}
			return 0f;
		}

		// Token: 0x060002AC RID: 684 RVA: 0x00015A5A File Offset: 0x00013C5A
		public float GetSavedStatValueWithAll(StatTypes statType, Identifier statIdentifier)
		{
			return this.GetSavedStatValue(statType, Tags.StatIdentifierTargetAll) + this.GetSavedStatValue(statType, statIdentifier);
		}

		// Token: 0x060002AD RID: 685 RVA: 0x00015A71 File Offset: 0x00013C71
		public float GetSavedStatValueWithBotsInMp(StatTypes statType, Identifier statIdentifier)
		{
			return this.GetSavedStatValueWithBotsInMp(statType, statIdentifier, GameSession.GetSessionCrewCharacters(CharacterType.Bot));
		}

		// Token: 0x060002AE RID: 686 RVA: 0x00015A84 File Offset: 0x00013C84
		public float GetSavedStatValueWithBotsInMp(StatTypes statType, Identifier statIdentifier, IReadOnlyCollection<Character> bots)
		{
			float statValue = this.GetSavedStatValue(statType, statIdentifier);
			if (GameMain.NetworkMember == null)
			{
				return statValue;
			}
			foreach (Character bot in bots)
			{
				int botStatValue = (int)bot.Info.GetSavedStatValue(statType, statIdentifier);
				statValue = Math.Max(statValue, (float)botStatValue);
			}
			return statValue;
		}

		// Token: 0x060002AF RID: 687 RVA: 0x00015AF0 File Offset: 0x00013CF0
		public void ChangeSavedStatValue(StatTypes statType, float value, Identifier statIdentifier, bool removeOnDeath, float maxValue = 3.4028235E+38f, bool setValue = false)
		{
			if (!this.SavedStatValues.ContainsKey(statType))
			{
				this.SavedStatValues.Add(statType, new List<SavedStatValue>());
			}
			SavedStatValue savedStat = this.SavedStatValues[statType].FirstOrDefault(delegate(SavedStatValue s)
			{
				Identifier statIdentifier2 = s.StatIdentifier;
				return statIdentifier2 == statIdentifier;
			});
			bool changed;
			if (savedStat != null)
			{
				float prevValue = savedStat.StatValue;
				savedStat.StatValue = (setValue ? value : MathHelper.Min(savedStat.StatValue + value, maxValue));
				changed = !MathUtils.NearlyEqual(savedStat.StatValue, prevValue, 0.0001f);
			}
			else
			{
				this.SavedStatValues[statType].Add(new SavedStatValue(statIdentifier, MathHelper.Min(value, maxValue), removeOnDeath));
				changed = true;
			}
			if (changed)
			{
				this.OnPermanentStatChanged(statType);
			}
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x00015C34 File Offset: 0x00013E34
		[CompilerGenerated]
		private int <SetAttachments>g__pickRandomIndex|145_0(IReadOnlyList<ContentXElement> list, ref CharacterInfo.<>c__DisplayClass145_0 A_2)
		{
			ContentXElement[] elems = this.GetValidAttachmentElements(list, this.Head.Preset, null).ToArray<ContentXElement>();
			float[] weights = CharacterInfo.GetWeights(elems).ToArray<float>();
			return list.IndexOf(ToolBox.SelectWeightedRandom<ContentXElement>(elems, weights, A_2.randSync));
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x00015CA0 File Offset: 0x00013EA0
		[CompilerGenerated]
		internal static bool <LoadOrders>g__GetTargetEntity|220_0(ushort targetId, out Entity targetEntity, ref CharacterInfo.<>c__DisplayClass220_0 A_2, ref CharacterInfo.<>c__DisplayClass220_1 A_3, ref CharacterInfo.<>c__DisplayClass220_2 A_4)
		{
			targetEntity = null;
			if (targetId == 0)
			{
				return true;
			}
			Submarine parentSub = null;
			if (A_3.orderElement.GetAttributeBool("onmainsub", false))
			{
				parentSub = Submarine.MainSub;
			}
			else
			{
				int linkedSubIndex = A_3.orderElement.GetAttributeInt("linkedsubindex", -1);
				if (linkedSubIndex >= 0 && linkedSubIndex < A_2.linkedSubs.Count)
				{
					LinkedSubmarine linkedSub = A_2.linkedSubs[linkedSubIndex];
					if (linkedSub != null && linkedSub.LoadSub)
					{
						parentSub = linkedSub.Sub;
					}
				}
			}
			if (parentSub != null)
			{
				targetId = CharacterInfo.GetOffsetId(parentSub, targetId);
				targetEntity = Entity.FindEntityByID(targetId);
				return targetEntity != null;
			}
			if (!A_4.orderPrefab.CanBeGeneralized)
			{
				DebugConsole.ThrowError("Error loading a previously saved order (" + A_4.orderIdentifier + "). Can't find the parent sub of the target entity. The order requires a target so it can't be loaded at all.", null, null, false, false);
				int priorityIncrease = A_2.priorityIncrease;
				A_2.priorityIncrease = priorityIncrease + 1;
				return false;
			}
			DebugConsole.AddWarning("Trying to load a previously saved order (" + A_4.orderIdentifier + "). Can't find the parent sub of the target entity. The order doesn't require a target so a more generic version of the order will be loaded instead.", null);
			return true;
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x00015D8C File Offset: 0x00013F8C
		[CompilerGenerated]
		internal static bool <ResetSavedStatValue>g__MatchesIdentifier|234_0(Identifier statIdentifier, Identifier identifier)
		{
			if (statIdentifier == identifier)
			{
				return true;
			}
			int index = identifier.IndexOf('*');
			return index > -1 && statIdentifier.StartsWith(identifier[new Range(0, index)]);
		}

		// Token: 0x04000108 RID: 264
		private readonly Dictionary<Identifier, float> prevSentSkill = new Dictionary<Identifier, float>();

		// Token: 0x04000109 RID: 265
		public bool Discarded;

		// Token: 0x0400010A RID: 266
		private CharacterInfo.HeadInfo head;

		// Token: 0x0400010B RID: 267
		private readonly Identifier maleIdentifier = "Male".ToIdentifier();

		// Token: 0x0400010C RID: 268
		private readonly Identifier femaleIdentifier = "Female".ToIdentifier();

		// Token: 0x0400010D RID: 269
		public XElement InventoryData;

		// Token: 0x0400010E RID: 270
		public XElement HealthData;

		// Token: 0x0400010F RID: 271
		public XElement OrderData;

		// Token: 0x04000110 RID: 272
		public bool PermanentlyDead;

		// Token: 0x04000111 RID: 273
		public bool RenamingEnabled;

		// Token: 0x04000112 RID: 274
		private BotStatus botStatus = BotStatus.ActiveService;

		// Token: 0x04000113 RID: 275
		public bool PendingSpawnToActiveService;

		// Token: 0x04000114 RID: 276
		private static ushort idCounter = 1;

		// Token: 0x04000115 RID: 277
		private const string disguiseName = "???";

		// Token: 0x04000117 RID: 279
		public string Name;

		// Token: 0x04000118 RID: 280
		public LocalizedString Title;

		// Token: 0x04000119 RID: 281
		[TupleElementNames(new string[]
		{
			"NpcSetIdentifier",
			"NpcIdentifier"
		})]
		public ValueTuple<Identifier, Identifier> HumanPrefabIds;

		// Token: 0x0400011A RID: 282
		private HumanPrefab _humanPrefab;

		// Token: 0x0400011C RID: 284
		private Character character;

		// Token: 0x0400011D RID: 285
		public Job Job;

		// Token: 0x0400011E RID: 286
		public int Salary;

		// Token: 0x04000120 RID: 288
		private int talentRefundPoints;

		// Token: 0x04000123 RID: 291
		private int talentResetCount;

		// Token: 0x04000124 RID: 292
		[TupleElementNames(new string[]
		{
			"factionId",
			"reputation"
		})]
		public ValueTuple<Identifier, float> MinReputationToHire;

		// Token: 0x04000125 RID: 293
		public const int MaxAdditionalTalentPoints = 100;

		// Token: 0x04000126 RID: 294
		private int additionalTalentPoints;

		// Token: 0x04000127 RID: 295
		private Sprite _headSprite;

		// Token: 0x04000128 RID: 296
		public bool OmitJobInMenus;

		// Token: 0x04000129 RID: 297
		private Sprite portrait;

		// Token: 0x0400012A RID: 298
		public bool IsDisguised;

		// Token: 0x0400012B RID: 299
		public bool IsDisguisedAsAnother;

		// Token: 0x0400012C RID: 300
		private List<WearableSprite> attachmentSprites;

		// Token: 0x0400012E RID: 302
		public bool StartItemsGiven;

		// Token: 0x0400012F RID: 303
		public bool IsNewHire;

		// Token: 0x04000130 RID: 304
		public CauseOfDeath CauseOfDeath;

		// Token: 0x04000131 RID: 305
		public CharacterTeamType TeamID;

		// Token: 0x04000133 RID: 307
		public const int MaxCurrentOrders = 3;

		// Token: 0x04000135 RID: 309
		public ushort ID;

		// Token: 0x04000137 RID: 311
		public readonly bool HasSpecifierTags;

		// Token: 0x04000138 RID: 312
		private RagdollParams ragdoll;

		// Token: 0x04000139 RID: 313
		[TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})]
		public readonly ImmutableArray<ValueTuple<Color, float>> HairColors;

		// Token: 0x0400013A RID: 314
		[TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})]
		public readonly ImmutableArray<ValueTuple<Color, float>> FacialHairColors;

		// Token: 0x0400013B RID: 315
		[TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})]
		public readonly ImmutableArray<ValueTuple<Color, float>> SkinColors;

		// Token: 0x0400013C RID: 316
		public int MissionsCompletedSinceDeath;

		// Token: 0x0400013D RID: 317
		public Option<int> LastRewardDistribution;

		// Token: 0x0400013E RID: 318
		private List<ContentXElement> hairs;

		// Token: 0x0400013F RID: 319
		private List<ContentXElement> beards;

		// Token: 0x04000140 RID: 320
		private List<ContentXElement> moustaches;

		// Token: 0x04000141 RID: 321
		private List<ContentXElement> faceAttachments;

		// Token: 0x04000142 RID: 322
		private IEnumerable<ContentXElement> wearables;

		// Token: 0x04000143 RID: 323
		private bool spriteTagsLoaded;

		// Token: 0x04000144 RID: 324
		private static readonly ImmutableDictionary<Identifier, StatTypes> skillGainStatValues = new Dictionary<Identifier, StatTypes>
		{
			{
				new Identifier("helm"),
				StatTypes.HelmSkillGainSpeed
			},
			{
				new Identifier("weapons"),
				StatTypes.WeaponsSkillGainSpeed
			},
			{
				new Identifier("medical"),
				StatTypes.MedicalSkillGainSpeed
			},
			{
				new Identifier("electrical"),
				StatTypes.ElectricalSkillGainSpeed
			},
			{
				new Identifier("mechanical"),
				StatTypes.MechanicalSkillGainSpeed
			}
		}.ToImmutableDictionary<Identifier, StatTypes>();

		// Token: 0x04000145 RID: 325
		private const int BaseExperienceRequired = 450;

		// Token: 0x04000146 RID: 326
		private const int AddedExperienceRequiredPerLevel = 500;

		// Token: 0x04000147 RID: 327
		public readonly Dictionary<StatTypes, List<SavedStatValue>> SavedStatValues;

		// Token: 0x04000148 RID: 328
		public float LastResistanceMultiplierSkillLossDeath;

		// Token: 0x04000149 RID: 329
		public float LastResistanceMultiplierSkillLossRespawn;

		// Token: 0x02000542 RID: 1346
		public class HeadInfo
		{
			// Token: 0x170013AB RID: 5035
			// (get) Token: 0x06004921 RID: 18721 RVA: 0x001CEE9B File Offset: 0x001CD09B
			// (set) Token: 0x06004922 RID: 18722 RVA: 0x001CEEA3 File Offset: 0x001CD0A3
			public int HairIndex { get; set; }

			// Token: 0x06004923 RID: 18723 RVA: 0x001CEEAC File Offset: 0x001CD0AC
			public void SetHairWithHatIndex()
			{
				if (this.CharacterInfo.Hairs == null)
				{
					if (this.HairIndex == -1)
					{
						DebugConsole.AddWarning("Setting \"hairWithHatIndex\" before \"Hairs\" are defined!", null);
					}
					this.hairWithHatIndex = new int?(this.HairIndex);
					return;
				}
				ContentXElement hairElement = this.HairElement;
				this.hairWithHatIndex = new int?((hairElement != null) ? hairElement.GetAttributeInt("replacewhenwearinghat", this.HairIndex) : -1);
				int? num = this.hairWithHatIndex;
				int num2 = 0;
				if (!(num.GetValueOrDefault() < num2 & num != null))
				{
					num = this.hairWithHatIndex;
					num2 = this.CharacterInfo.Hairs.Count;
					if (!(num.GetValueOrDefault() >= num2 & num != null))
					{
						return;
					}
				}
				this.hairWithHatIndex = new int?(this.HairIndex);
			}

			// Token: 0x170013AC RID: 5036
			// (get) Token: 0x06004924 RID: 18724 RVA: 0x001CEF71 File Offset: 0x001CD171
			public Vector2 SheetIndex
			{
				get
				{
					return this.Preset.SheetIndex;
				}
			}

			// Token: 0x170013AD RID: 5037
			// (get) Token: 0x06004925 RID: 18725 RVA: 0x001CEF80 File Offset: 0x001CD180
			public ContentXElement HairElement
			{
				get
				{
					if (this.CharacterInfo.Hairs == null)
					{
						return null;
					}
					if (this.HairIndex >= this.CharacterInfo.Hairs.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Hair index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.HairIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.Hairs.ElementAtOrDefault(this.HairIndex);
				}
			}

			// Token: 0x170013AE RID: 5038
			// (get) Token: 0x06004926 RID: 18726 RVA: 0x001CF034 File Offset: 0x001CD234
			public ContentXElement HairWithHatElement
			{
				get
				{
					if (this.hairWithHatIndex == null)
					{
						this.SetHairWithHatIndex();
					}
					if (this.CharacterInfo.Hairs == null)
					{
						return null;
					}
					int? num = this.hairWithHatIndex;
					int count = this.CharacterInfo.Hairs.Count;
					if (num.GetValueOrDefault() >= count & num != null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Hair with hat index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int?>(this.hairWithHatIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.Hairs.ElementAtOrDefault(this.hairWithHatIndex.Value);
				}
			}

			// Token: 0x170013AF RID: 5039
			// (get) Token: 0x06004927 RID: 18727 RVA: 0x001CF118 File Offset: 0x001CD318
			public ContentXElement BeardElement
			{
				get
				{
					if (this.CharacterInfo.Beards == null)
					{
						return null;
					}
					if (this.BeardIndex >= this.CharacterInfo.Beards.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Beard index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.BeardIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.Beards.ElementAtOrDefault(this.BeardIndex);
				}
			}

			// Token: 0x170013B0 RID: 5040
			// (get) Token: 0x06004928 RID: 18728 RVA: 0x001CF1CC File Offset: 0x001CD3CC
			public ContentXElement MoustacheElement
			{
				get
				{
					if (this.CharacterInfo.Moustaches == null)
					{
						return null;
					}
					if (this.MoustacheIndex >= this.CharacterInfo.Moustaches.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Moustache index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.MoustacheIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.Moustaches.ElementAtOrDefault(this.MoustacheIndex);
				}
			}

			// Token: 0x170013B1 RID: 5041
			// (get) Token: 0x06004929 RID: 18729 RVA: 0x001CF280 File Offset: 0x001CD480
			public ContentXElement FaceAttachment
			{
				get
				{
					if (this.CharacterInfo.FaceAttachments == null)
					{
						return null;
					}
					if (this.FaceAttachmentIndex >= this.CharacterInfo.FaceAttachments.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Face attachment index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.FaceAttachmentIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.FaceAttachments.ElementAtOrDefault(this.FaceAttachmentIndex);
				}
			}

			// Token: 0x0600492A RID: 18730 RVA: 0x001CF334 File Offset: 0x001CD534
			public HeadInfo(CharacterInfo characterInfo, CharacterInfo.HeadPreset headPreset, int hairIndex = 0, int beardIndex = 0, int moustacheIndex = 0, int faceAttachmentIndex = 0)
			{
				this.CharacterInfo = characterInfo;
				this.Preset = headPreset;
				this.HairIndex = hairIndex;
				this.BeardIndex = beardIndex;
				this.MoustacheIndex = moustacheIndex;
				this.FaceAttachmentIndex = faceAttachmentIndex;
			}

			// Token: 0x0600492B RID: 18731 RVA: 0x001CF369 File Offset: 0x001CD569
			public void ResetAttachmentIndices()
			{
				this.HairIndex = -1;
				this.BeardIndex = -1;
				this.MoustacheIndex = -1;
				this.FaceAttachmentIndex = -1;
			}

			// Token: 0x04002576 RID: 9590
			public readonly CharacterInfo CharacterInfo;

			// Token: 0x04002577 RID: 9591
			public readonly CharacterInfo.HeadPreset Preset;

			// Token: 0x04002579 RID: 9593
			private int? hairWithHatIndex;

			// Token: 0x0400257A RID: 9594
			public int BeardIndex;

			// Token: 0x0400257B RID: 9595
			public int MoustacheIndex;

			// Token: 0x0400257C RID: 9596
			public int FaceAttachmentIndex;

			// Token: 0x0400257D RID: 9597
			public Color HairColor;

			// Token: 0x0400257E RID: 9598
			public Color FacialHairColor;

			// Token: 0x0400257F RID: 9599
			public Color SkinColor;
		}

		// Token: 0x02000543 RID: 1347
		public class HeadPreset : ISerializableEntity
		{
			// Token: 0x170013B2 RID: 5042
			// (get) Token: 0x0600492C RID: 18732 RVA: 0x001CF387 File Offset: 0x001CD587
			public Identifier MenuCategory
			{
				get
				{
					return this.TagSet.First((Identifier t) => this.characterInfoPrefab.VarTags[this.characterInfoPrefab.MenuCategoryVar].Contains(t));
				}
			}

			// Token: 0x170013B3 RID: 5043
			// (get) Token: 0x0600492D RID: 18733 RVA: 0x001CF3A0 File Offset: 0x001CD5A0
			// (set) Token: 0x0600492E RID: 18734 RVA: 0x001CF3A8 File Offset: 0x001CD5A8
			public ImmutableHashSet<Identifier> TagSet { get; private set; }

			// Token: 0x170013B4 RID: 5044
			// (get) Token: 0x0600492F RID: 18735 RVA: 0x001CF3B1 File Offset: 0x001CD5B1
			// (set) Token: 0x06004930 RID: 18736 RVA: 0x001CF3C4 File Offset: 0x001CD5C4
			[Serialize("", IsPropertySaveable.No, "", "", false)]
			public string Tags
			{
				get
				{
					return string.Join<Identifier>(",", this.TagSet);
				}
				private set
				{
					this.TagSet = (from s in value.Split(",", StringSplitOptions.None)
					select s.ToIdentifier() into id
					where !id.IsEmpty
					select id).ToImmutableHashSet<Identifier>();
				}
			}

			// Token: 0x170013B5 RID: 5045
			// (get) Token: 0x06004931 RID: 18737 RVA: 0x001CF430 File Offset: 0x001CD630
			// (set) Token: 0x06004932 RID: 18738 RVA: 0x001CF438 File Offset: 0x001CD638
			[Serialize("0,0", IsPropertySaveable.No, "", "", false)]
			public Vector2 SheetIndex { get; private set; }

			// Token: 0x170013B6 RID: 5046
			// (get) Token: 0x06004933 RID: 18739 RVA: 0x001CF441 File Offset: 0x001CD641
			public string Name
			{
				get
				{
					return "Head Preset " + this.Tags;
				}
			}

			// Token: 0x170013B7 RID: 5047
			// (get) Token: 0x06004934 RID: 18740 RVA: 0x001CF453 File Offset: 0x001CD653
			// (set) Token: 0x06004935 RID: 18741 RVA: 0x001CF45B File Offset: 0x001CD65B
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

			// Token: 0x06004936 RID: 18742 RVA: 0x001CF464 File Offset: 0x001CD664
			public HeadPreset(CharacterInfoPrefab charInfoPrefab, XElement element)
			{
				this.characterInfoPrefab = charInfoPrefab;
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
				this.DetermineTagsFromLegacyFormat(element);
			}

			// Token: 0x06004937 RID: 18743 RVA: 0x001CF488 File Offset: 0x001CD688
			private void DetermineTagsFromLegacyFormat(XElement element)
			{
				string headId = element.GetAttributeString("id", "");
				string gender = element.GetAttributeString("gender", "");
				string race = element.GetAttributeString("race", "");
				if (!headId.IsNullOrEmpty())
				{
					this.<DetermineTagsFromLegacyFormat>g__addTag|21_0("head" + headId);
				}
				if (!gender.IsNullOrEmpty())
				{
					this.<DetermineTagsFromLegacyFormat>g__addTag|21_0(gender);
				}
				if (!race.IsNullOrEmpty())
				{
					this.<DetermineTagsFromLegacyFormat>g__addTag|21_0(race);
				}
			}

			// Token: 0x06004939 RID: 18745 RVA: 0x001CF522 File Offset: 0x001CD722
			[CompilerGenerated]
			private void <DetermineTagsFromLegacyFormat>g__addTag|21_0(string tag)
			{
				this.TagSet = this.TagSet.Add(tag.ToIdentifier());
			}

			// Token: 0x04002580 RID: 9600
			private readonly CharacterInfoPrefab characterInfoPrefab;
		}
	}
}
