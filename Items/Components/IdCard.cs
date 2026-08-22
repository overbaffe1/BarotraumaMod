using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005AE RID: 1454
	internal class IdCard : Pickable
	{
		// Token: 0x1700164F RID: 5711
		// (get) Token: 0x0600592E RID: 22830 RVA: 0x002DFB77 File Offset: 0x002DDD77
		// (set) Token: 0x0600592F RID: 22831 RVA: 0x002DFB7F File Offset: 0x002DDD7F
		[Serialize(CharacterTeamType.None, IsPropertySaveable.Yes, "", "", true)]
		public CharacterTeamType TeamID { get; set; }

		// Token: 0x17001650 RID: 5712
		// (get) Token: 0x06005930 RID: 22832 RVA: 0x002DFB88 File Offset: 0x002DDD88
		// (set) Token: 0x06005931 RID: 22833 RVA: 0x002DFB90 File Offset: 0x002DDD90
		[Serialize(0, IsPropertySaveable.Yes, "", "", true)]
		public int SubmarineSpecificID { get; set; }

		// Token: 0x17001651 RID: 5713
		// (get) Token: 0x06005932 RID: 22834 RVA: 0x002DFB99 File Offset: 0x002DDD99
		// (set) Token: 0x06005933 RID: 22835 RVA: 0x002DFBA8 File Offset: 0x002DDDA8
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string OwnerTags
		{
			get
			{
				return string.Join<Identifier>(',', this.OwnerTagSet);
			}
			set
			{
				this.OwnerTagSet = value.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
			}
		}

		// Token: 0x17001652 RID: 5714
		// (get) Token: 0x06005934 RID: 22836 RVA: 0x002DFBC0 File Offset: 0x002DDDC0
		// (set) Token: 0x06005935 RID: 22837 RVA: 0x002DFBC8 File Offset: 0x002DDDC8
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string Description { get; set; }

		// Token: 0x17001653 RID: 5715
		// (get) Token: 0x06005936 RID: 22838 RVA: 0x002DFBD1 File Offset: 0x002DDDD1
		// (set) Token: 0x06005937 RID: 22839 RVA: 0x002DFBD9 File Offset: 0x002DDDD9
		public ImmutableHashSet<Identifier> OwnerTagSet { get; set; }

		// Token: 0x17001654 RID: 5716
		// (get) Token: 0x06005938 RID: 22840 RVA: 0x002DFBE2 File Offset: 0x002DDDE2
		// (set) Token: 0x06005939 RID: 22841 RVA: 0x002DFBEA File Offset: 0x002DDDEA
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string OwnerName { get; set; }

		// Token: 0x17001655 RID: 5717
		// (get) Token: 0x0600593A RID: 22842 RVA: 0x002DFBF3 File Offset: 0x002DDDF3
		// (set) Token: 0x0600593B RID: 22843 RVA: 0x002DFBFB File Offset: 0x002DDDFB
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string OwnerNameLocalized
		{
			get
			{
				return this.ownerNameLocalized;
			}
			set
			{
				if (value.IsNullOrWhiteSpace())
				{
					return;
				}
				this.ownerNameLocalized = value;
				this.OwnerName = TextManager.Get(value).Fallback(value, true).Value;
			}
		}

		// Token: 0x17001656 RID: 5718
		// (get) Token: 0x0600593C RID: 22844 RVA: 0x002DFC2A File Offset: 0x002DDE2A
		// (set) Token: 0x0600593D RID: 22845 RVA: 0x002DFC32 File Offset: 0x002DDE32
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public Identifier OwnerJobId { get; set; }

		// Token: 0x17001657 RID: 5719
		// (get) Token: 0x0600593E RID: 22846 RVA: 0x002DFC3C File Offset: 0x002DDE3C
		public JobPrefab OwnerJob
		{
			get
			{
				JobPrefab prefab;
				if (!JobPrefab.Prefabs.TryGet(this.OwnerJobId, out prefab))
				{
					return null;
				}
				return prefab;
			}
		}

		// Token: 0x17001658 RID: 5720
		// (get) Token: 0x0600593F RID: 22847 RVA: 0x002DFC60 File Offset: 0x002DDE60
		// (set) Token: 0x06005940 RID: 22848 RVA: 0x002DFC68 File Offset: 0x002DDE68
		[Serialize(-1, IsPropertySaveable.Yes, "", "", true)]
		public int OwnerHairIndex { get; set; }

		// Token: 0x17001659 RID: 5721
		// (get) Token: 0x06005941 RID: 22849 RVA: 0x002DFC71 File Offset: 0x002DDE71
		// (set) Token: 0x06005942 RID: 22850 RVA: 0x002DFC79 File Offset: 0x002DDE79
		[Serialize(-1, IsPropertySaveable.Yes, "", "", true)]
		public int OwnerBeardIndex { get; set; }

		// Token: 0x1700165A RID: 5722
		// (get) Token: 0x06005943 RID: 22851 RVA: 0x002DFC82 File Offset: 0x002DDE82
		// (set) Token: 0x06005944 RID: 22852 RVA: 0x002DFC8A File Offset: 0x002DDE8A
		[Serialize(-1, IsPropertySaveable.Yes, "", "", true)]
		public int OwnerMoustacheIndex { get; set; }

		// Token: 0x1700165B RID: 5723
		// (get) Token: 0x06005945 RID: 22853 RVA: 0x002DFC93 File Offset: 0x002DDE93
		// (set) Token: 0x06005946 RID: 22854 RVA: 0x002DFC9B File Offset: 0x002DDE9B
		[Serialize(-1, IsPropertySaveable.Yes, "", "", true)]
		public int OwnerFaceAttachmentIndex { get; set; }

		// Token: 0x1700165C RID: 5724
		// (get) Token: 0x06005947 RID: 22855 RVA: 0x002DFCA4 File Offset: 0x002DDEA4
		// (set) Token: 0x06005948 RID: 22856 RVA: 0x002DFCAC File Offset: 0x002DDEAC
		[Serialize("#ffffff", IsPropertySaveable.Yes, "", "", true)]
		public Color OwnerHairColor { get; set; }

		// Token: 0x1700165D RID: 5725
		// (get) Token: 0x06005949 RID: 22857 RVA: 0x002DFCB5 File Offset: 0x002DDEB5
		// (set) Token: 0x0600594A RID: 22858 RVA: 0x002DFCBD File Offset: 0x002DDEBD
		[Serialize("#ffffff", IsPropertySaveable.Yes, "", "", true)]
		public Color OwnerFacialHairColor { get; set; }

		// Token: 0x1700165E RID: 5726
		// (get) Token: 0x0600594B RID: 22859 RVA: 0x002DFCC6 File Offset: 0x002DDEC6
		// (set) Token: 0x0600594C RID: 22860 RVA: 0x002DFCCE File Offset: 0x002DDECE
		[Serialize("#ffffff", IsPropertySaveable.Yes, "", "", true)]
		public Color OwnerSkinColor { get; set; }

		// Token: 0x1700165F RID: 5727
		// (get) Token: 0x0600594D RID: 22861 RVA: 0x002DFCD7 File Offset: 0x002DDED7
		// (set) Token: 0x0600594E RID: 22862 RVA: 0x002DFCDF File Offset: 0x002DDEDF
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", true)]
		public Vector2 OwnerSheetIndex { get; set; }

		// Token: 0x0600594F RID: 22863 RVA: 0x002DFCE8 File Offset: 0x002DDEE8
		public IdCard(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06005950 RID: 22864 RVA: 0x002DFCF4 File Offset: 0x002DDEF4
		public void Initialize(WayPoint spawnPoint, Character character)
		{
			this.item.AddTag("name:" + character.Name);
			CharacterInfo info = character.Info;
			if (info == null)
			{
				return;
			}
			if (spawnPoint != null)
			{
				foreach (string s in spawnPoint.IdCardTags)
				{
					this.item.AddTag(s);
				}
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
				{
					Item item = this.item;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 1);
					defaultInterpolatedStringHandler.AppendLiteral("id_");
					defaultInterpolatedStringHandler.AppendFormatted<CharacterTeamType>(character.TeamID);
					item.AddTag(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
				}
				if (!string.IsNullOrWhiteSpace(spawnPoint.IdCardDesc))
				{
					this.item.Description = (this.Description = spawnPoint.IdCardDesc);
				}
			}
			this.TeamID = info.TeamID;
			CharacterInfo.HeadInfo head = info.Head;
			if (head == null)
			{
				return;
			}
			this.OwnerName = info.Name;
			Job job = info.Job;
			this.OwnerJobId = ((job != null) ? job.Prefab.Identifier : Identifier.Empty);
			Item item2 = this.item;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(6, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("jobid:");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.OwnerJobId);
			item2.AddTag(defaultInterpolatedStringHandler2.ToStringAndClear());
			this.OwnerTagSet = info.Head.Preset.TagSet;
			this.OwnerHairIndex = head.HairIndex;
			this.OwnerBeardIndex = head.BeardIndex;
			this.OwnerMoustacheIndex = head.MoustacheIndex;
			this.OwnerFaceAttachmentIndex = head.FaceAttachmentIndex;
			this.OwnerHairColor = head.HairColor;
			this.OwnerFacialHairColor = head.FacialHairColor;
			this.OwnerSkinColor = head.SkinColor;
			this.OwnerSheetIndex = head.SheetIndex;
		}

		// Token: 0x06005951 RID: 22865 RVA: 0x002DFEBC File Offset: 0x002DE0BC
		public override void Equip(Character character)
		{
			base.Equip(character);
			CharacterInfo info = character.Info;
			if (info == null)
			{
				return;
			}
			info.CheckDisguiseStatus(true, this);
		}

		// Token: 0x06005952 RID: 22866 RVA: 0x002DFED7 File Offset: 0x002DE0D7
		public override void Unequip(Character character)
		{
			base.Unequip(character);
			CharacterInfo info = character.Info;
			if (info == null)
			{
				return;
			}
			info.CheckDisguiseStatus(true, this);
		}

		// Token: 0x06005953 RID: 22867 RVA: 0x002DFEF2 File Offset: 0x002DE0F2
		public override void OnItemLoaded()
		{
			if (!string.IsNullOrEmpty(this.Description))
			{
				this.item.Description = this.Description;
			}
		}

		// Token: 0x04002D7B RID: 11643
		public IdCard.OwnerAppearance StoredOwnerAppearance;

		// Token: 0x04002D81 RID: 11649
		private string ownerNameLocalized;

		// Token: 0x020013B2 RID: 5042
		public struct OwnerAppearance
		{
			// Token: 0x0600981B RID: 38939 RVA: 0x003DCB28 File Offset: 0x003DAD28
			public void ExtractJobPrefab(IReadOnlyDictionary<Identifier, string> tags)
			{
				string jobId;
				if (!tags.TryGetValue("jobid".ToIdentifier(), out jobId))
				{
					return;
				}
				if (!jobId.IsNullOrEmpty())
				{
					this.JobPrefab = JobPrefab.Get(jobId.ToIdentifier());
				}
			}

			// Token: 0x0600981C RID: 38940 RVA: 0x003DCB64 File Offset: 0x003DAD64
			public void ExtractAppearance(CharacterInfo characterInfo, IdCard idCard)
			{
				IdCard.OwnerAppearance.<>c__DisplayClass8_0 CS$<>8__locals1;
				CS$<>8__locals1.characterInfo = characterInfo;
				int disguisedHairIndex = idCard.OwnerHairIndex;
				int disguisedBeardIndex = idCard.OwnerBeardIndex;
				int disguisedMoustacheIndex = idCard.OwnerMoustacheIndex;
				int disguisedFaceAttachmentIndex = idCard.OwnerFaceAttachmentIndex;
				Color hairColor = idCard.OwnerHairColor;
				Color facialHairColor = idCard.OwnerFacialHairColor;
				Color skinColor = idCard.OwnerSkinColor;
				CS$<>8__locals1.tags = idCard.OwnerTagSet;
				if (CS$<>8__locals1.characterInfo.HasSpecifierTags && !CS$<>8__locals1.tags.Any<Identifier>())
				{
					this.Portrait = null;
					this.Attachments = null;
					return;
				}
				ContentXElement mainElement = CS$<>8__locals1.characterInfo.Ragdoll.MainElement;
				IEnumerable<ContentXElement> limbElements = (mainElement != null) ? mainElement.Elements() : null;
				if (limbElements != null)
				{
					foreach (ContentXElement limbElement in limbElements)
					{
						if (limbElement.GetAttributeString("type", "").Equals("head", StringComparison.OrdinalIgnoreCase))
						{
							ContentXElement spriteElement = limbElement.GetChildElement("sprite");
							ContentXElement contentXElement = null;
							if (!(spriteElement == contentXElement))
							{
								ContentPath contentPath = spriteElement.GetAttributeContentPath("texture");
								string spritePath = CS$<>8__locals1.characterInfo.ReplaceVars(contentPath.Value);
								string fileName = Path.GetFileNameWithoutExtension(spritePath);
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
											this.Portrait = new Sprite(spriteElement, "", file, false, 1f)
											{
												RelativeOrigin = Vector2.Zero
											};
											break;
										}
									}
								}
								break;
							}
						}
					}
				}
				if (CS$<>8__locals1.characterInfo.Wearables != null)
				{
					float baldnessChance = 0.1f;
					List<ContentXElement> disguisedHairs = IdCard.OwnerAppearance.<ExtractAppearance>g__createElementList|8_0(WearableType.Hair, baldnessChance, ref CS$<>8__locals1);
					List<ContentXElement> disguisedBeards = IdCard.OwnerAppearance.<ExtractAppearance>g__createElementList|8_0(WearableType.Beard, 1f, ref CS$<>8__locals1);
					List<ContentXElement> disguisedMoustaches = IdCard.OwnerAppearance.<ExtractAppearance>g__createElementList|8_0(WearableType.Moustache, 1f, ref CS$<>8__locals1);
					List<ContentXElement> disguisedFaceAttachments = IdCard.OwnerAppearance.<ExtractAppearance>g__createElementList|8_0(WearableType.FaceAttachment, 1f, ref CS$<>8__locals1);
					ContentXElement disguisedHairElement = IdCard.OwnerAppearance.<ExtractAppearance>g__getElementFromList|8_1(disguisedHairs, disguisedHairIndex);
					ContentXElement disguisedBeardElement = IdCard.OwnerAppearance.<ExtractAppearance>g__getElementFromList|8_1(disguisedBeards, disguisedBeardIndex);
					ContentXElement disguisedMoustacheElement = IdCard.OwnerAppearance.<ExtractAppearance>g__getElementFromList|8_1(disguisedMoustaches, disguisedMoustacheIndex);
					ContentXElement disguisedFaceAttachmentElement = IdCard.OwnerAppearance.<ExtractAppearance>g__getElementFromList|8_1(disguisedFaceAttachments, disguisedFaceAttachmentIndex);
					this.Attachments = new List<WearableSprite>();
					IdCard.OwnerAppearance.<ExtractAppearance>g__loadAttachments|8_2(this.Attachments, disguisedFaceAttachmentElement, WearableType.FaceAttachment);
					IdCard.OwnerAppearance.<ExtractAppearance>g__loadAttachments|8_2(this.Attachments, disguisedBeardElement, WearableType.Beard);
					IdCard.OwnerAppearance.<ExtractAppearance>g__loadAttachments|8_2(this.Attachments, disguisedMoustacheElement, WearableType.Moustache);
					IdCard.OwnerAppearance.<ExtractAppearance>g__loadAttachments|8_2(this.Attachments, disguisedHairElement, WearableType.Hair);
				}
				this.HairColor = hairColor;
				this.FacialHairColor = facialHairColor;
				this.SkinColor = skinColor;
			}

			// Token: 0x0600981D RID: 38941 RVA: 0x003DCE28 File Offset: 0x003DB028
			[CompilerGenerated]
			internal static List<ContentXElement> <ExtractAppearance>g__createElementList|8_0(WearableType wearableType, float emptyCommonness = 1f, ref IdCard.OwnerAppearance.<>c__DisplayClass8_0 A_2)
			{
				return CharacterInfo.AddEmpty(A_2.characterInfo.FilterElements(A_2.characterInfo.Wearables, A_2.tags, new WearableType?(wearableType)), wearableType, emptyCommonness);
			}

			// Token: 0x0600981E RID: 38942 RVA: 0x003DCE53 File Offset: 0x003DB053
			[CompilerGenerated]
			internal static ContentXElement <ExtractAppearance>g__getElementFromList|8_1(List<ContentXElement> list, int index)
			{
				if (!CharacterInfo.IsValidIndex(index, list))
				{
					return null;
				}
				return list[index];
			}

			// Token: 0x0600981F RID: 38943 RVA: 0x003DCE68 File Offset: 0x003DB068
			[CompilerGenerated]
			internal static void <ExtractAppearance>g__loadAttachments|8_2(List<WearableSprite> attachments, ContentXElement element, WearableType wearableType)
			{
				IEnumerable<ContentXElement> enumerable;
				if ((enumerable = ((element != null) ? element.GetChildElements("sprite") : null)) == null)
				{
					IEnumerable<ContentXElement> enumerable2 = Enumerable.Empty<ContentXElement>();
					enumerable = enumerable2;
				}
				foreach (ContentXElement s in enumerable)
				{
					attachments.Add(new WearableSprite(s, wearableType));
				}
			}

			// Token: 0x04006334 RID: 25396
			public Sprite Portrait;

			// Token: 0x04006335 RID: 25397
			public Vector2 SheetIndex;

			// Token: 0x04006336 RID: 25398
			public JobPrefab JobPrefab;

			// Token: 0x04006337 RID: 25399
			public List<WearableSprite> Attachments;

			// Token: 0x04006338 RID: 25400
			public Color HairColor;

			// Token: 0x04006339 RID: 25401
			public Color FacialHairColor;

			// Token: 0x0400633A RID: 25402
			public Color SkinColor;
		}
	}
}
