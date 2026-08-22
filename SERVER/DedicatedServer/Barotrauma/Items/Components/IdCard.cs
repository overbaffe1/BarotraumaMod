using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004B9 RID: 1209
	internal class IdCard : Pickable
	{
		// Token: 0x17001247 RID: 4679
		// (get) Token: 0x060044A0 RID: 17568 RVA: 0x001B7BCB File Offset: 0x001B5DCB
		// (set) Token: 0x060044A1 RID: 17569 RVA: 0x001B7BD3 File Offset: 0x001B5DD3
		[Serialize(CharacterTeamType.None, IsPropertySaveable.Yes, "", "", true)]
		public CharacterTeamType TeamID { get; set; }

		// Token: 0x17001248 RID: 4680
		// (get) Token: 0x060044A2 RID: 17570 RVA: 0x001B7BDC File Offset: 0x001B5DDC
		// (set) Token: 0x060044A3 RID: 17571 RVA: 0x001B7BE4 File Offset: 0x001B5DE4
		[Serialize(0, IsPropertySaveable.Yes, "", "", true)]
		public int SubmarineSpecificID { get; set; }

		// Token: 0x17001249 RID: 4681
		// (get) Token: 0x060044A4 RID: 17572 RVA: 0x001B7BED File Offset: 0x001B5DED
		// (set) Token: 0x060044A5 RID: 17573 RVA: 0x001B7BFC File Offset: 0x001B5DFC
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

		// Token: 0x1700124A RID: 4682
		// (get) Token: 0x060044A6 RID: 17574 RVA: 0x001B7C14 File Offset: 0x001B5E14
		// (set) Token: 0x060044A7 RID: 17575 RVA: 0x001B7C1C File Offset: 0x001B5E1C
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string Description { get; set; }

		// Token: 0x1700124B RID: 4683
		// (get) Token: 0x060044A8 RID: 17576 RVA: 0x001B7C25 File Offset: 0x001B5E25
		// (set) Token: 0x060044A9 RID: 17577 RVA: 0x001B7C2D File Offset: 0x001B5E2D
		public ImmutableHashSet<Identifier> OwnerTagSet { get; set; }

		// Token: 0x1700124C RID: 4684
		// (get) Token: 0x060044AA RID: 17578 RVA: 0x001B7C36 File Offset: 0x001B5E36
		// (set) Token: 0x060044AB RID: 17579 RVA: 0x001B7C3E File Offset: 0x001B5E3E
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string OwnerName { get; set; }

		// Token: 0x1700124D RID: 4685
		// (get) Token: 0x060044AC RID: 17580 RVA: 0x001B7C47 File Offset: 0x001B5E47
		// (set) Token: 0x060044AD RID: 17581 RVA: 0x001B7C4F File Offset: 0x001B5E4F
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

		// Token: 0x1700124E RID: 4686
		// (get) Token: 0x060044AE RID: 17582 RVA: 0x001B7C7E File Offset: 0x001B5E7E
		// (set) Token: 0x060044AF RID: 17583 RVA: 0x001B7C86 File Offset: 0x001B5E86
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public Identifier OwnerJobId { get; set; }

		// Token: 0x1700124F RID: 4687
		// (get) Token: 0x060044B0 RID: 17584 RVA: 0x001B7C90 File Offset: 0x001B5E90
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

		// Token: 0x17001250 RID: 4688
		// (get) Token: 0x060044B1 RID: 17585 RVA: 0x001B7CB4 File Offset: 0x001B5EB4
		// (set) Token: 0x060044B2 RID: 17586 RVA: 0x001B7CBC File Offset: 0x001B5EBC
		[Serialize(-1, IsPropertySaveable.Yes, "", "", true)]
		public int OwnerHairIndex { get; set; }

		// Token: 0x17001251 RID: 4689
		// (get) Token: 0x060044B3 RID: 17587 RVA: 0x001B7CC5 File Offset: 0x001B5EC5
		// (set) Token: 0x060044B4 RID: 17588 RVA: 0x001B7CCD File Offset: 0x001B5ECD
		[Serialize(-1, IsPropertySaveable.Yes, "", "", true)]
		public int OwnerBeardIndex { get; set; }

		// Token: 0x17001252 RID: 4690
		// (get) Token: 0x060044B5 RID: 17589 RVA: 0x001B7CD6 File Offset: 0x001B5ED6
		// (set) Token: 0x060044B6 RID: 17590 RVA: 0x001B7CDE File Offset: 0x001B5EDE
		[Serialize(-1, IsPropertySaveable.Yes, "", "", true)]
		public int OwnerMoustacheIndex { get; set; }

		// Token: 0x17001253 RID: 4691
		// (get) Token: 0x060044B7 RID: 17591 RVA: 0x001B7CE7 File Offset: 0x001B5EE7
		// (set) Token: 0x060044B8 RID: 17592 RVA: 0x001B7CEF File Offset: 0x001B5EEF
		[Serialize(-1, IsPropertySaveable.Yes, "", "", true)]
		public int OwnerFaceAttachmentIndex { get; set; }

		// Token: 0x17001254 RID: 4692
		// (get) Token: 0x060044B9 RID: 17593 RVA: 0x001B7CF8 File Offset: 0x001B5EF8
		// (set) Token: 0x060044BA RID: 17594 RVA: 0x001B7D00 File Offset: 0x001B5F00
		[Serialize("#ffffff", IsPropertySaveable.Yes, "", "", true)]
		public Color OwnerHairColor { get; set; }

		// Token: 0x17001255 RID: 4693
		// (get) Token: 0x060044BB RID: 17595 RVA: 0x001B7D09 File Offset: 0x001B5F09
		// (set) Token: 0x060044BC RID: 17596 RVA: 0x001B7D11 File Offset: 0x001B5F11
		[Serialize("#ffffff", IsPropertySaveable.Yes, "", "", true)]
		public Color OwnerFacialHairColor { get; set; }

		// Token: 0x17001256 RID: 4694
		// (get) Token: 0x060044BD RID: 17597 RVA: 0x001B7D1A File Offset: 0x001B5F1A
		// (set) Token: 0x060044BE RID: 17598 RVA: 0x001B7D22 File Offset: 0x001B5F22
		[Serialize("#ffffff", IsPropertySaveable.Yes, "", "", true)]
		public Color OwnerSkinColor { get; set; }

		// Token: 0x17001257 RID: 4695
		// (get) Token: 0x060044BF RID: 17599 RVA: 0x001B7D2B File Offset: 0x001B5F2B
		// (set) Token: 0x060044C0 RID: 17600 RVA: 0x001B7D33 File Offset: 0x001B5F33
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", true)]
		public Vector2 OwnerSheetIndex { get; set; }

		// Token: 0x060044C1 RID: 17601 RVA: 0x001B7D3C File Offset: 0x001B5F3C
		public IdCard(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060044C2 RID: 17602 RVA: 0x001B7D48 File Offset: 0x001B5F48
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

		// Token: 0x060044C3 RID: 17603 RVA: 0x001B7F10 File Offset: 0x001B6110
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

		// Token: 0x060044C4 RID: 17604 RVA: 0x001B7F2B File Offset: 0x001B612B
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

		// Token: 0x060044C5 RID: 17605 RVA: 0x001B7F46 File Offset: 0x001B6146
		public override void OnItemLoaded()
		{
			if (!string.IsNullOrEmpty(this.Description))
			{
				this.item.Description = this.Description;
			}
		}

		// Token: 0x040020FA RID: 8442
		private string ownerNameLocalized;
	}
}
