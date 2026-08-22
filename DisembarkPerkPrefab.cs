using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Barotrauma.PerkBehaviors;

namespace Barotrauma
{
	// Token: 0x02000264 RID: 612
	internal sealed class DisembarkPerkPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000EBC RID: 3772
		// (get) Token: 0x06003852 RID: 14418 RVA: 0x002179F4 File Offset: 0x00215BF4
		public LocalizedString Name { get; }

		// Token: 0x17000EBD RID: 3773
		// (get) Token: 0x06003853 RID: 14419 RVA: 0x002179FC File Offset: 0x00215BFC
		public LocalizedString Description { get; }

		// Token: 0x17000EBE RID: 3774
		// (get) Token: 0x06003854 RID: 14420 RVA: 0x00217A04 File Offset: 0x00215C04
		public Identifier SortCategory { get; }

		// Token: 0x17000EBF RID: 3775
		// (get) Token: 0x06003855 RID: 14421 RVA: 0x00217A0C File Offset: 0x00215C0C
		public int SortKey { get; }

		// Token: 0x17000EC0 RID: 3776
		// (get) Token: 0x06003856 RID: 14422 RVA: 0x00217A14 File Offset: 0x00215C14
		public Identifier Prerequisite { get; }

		// Token: 0x17000EC1 RID: 3777
		// (get) Token: 0x06003857 RID: 14423 RVA: 0x00217A1C File Offset: 0x00215C1C
		public ImmutableHashSet<Identifier> MutuallyExclusivePerks { get; }

		// Token: 0x17000EC2 RID: 3778
		// (get) Token: 0x06003858 RID: 14424 RVA: 0x00217A24 File Offset: 0x00215C24
		public int Cost { get; }

		// Token: 0x17000EC3 RID: 3779
		// (get) Token: 0x06003859 RID: 14425 RVA: 0x00217A2C File Offset: 0x00215C2C
		public ImmutableArray<PerkBase> PerkBehaviors { get; }

		// Token: 0x0600385A RID: 14426 RVA: 0x00217A34 File Offset: 0x00215C34
		public DisembarkPerkPrefab(ContentXElement element, DisembarkPerkFile prefabFile) : base(prefabFile, element.GetAttributeIdentifier("identifier", ""))
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
			defaultInterpolatedStringHandler.AppendLiteral("disembarkperk.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			this.Name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(this.Identifier.ToString(), true);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("disembarkperkdescription.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
			this.Description = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()).Fallback("", true);
			this.Cost = element.GetAttributeInt("cost", 0);
			this.SortCategory = element.GetAttributeIdentifier("sortcategory", this.Identifier);
			this.Prerequisite = element.GetAttributeIdentifier("prerequisite", Identifier.Empty);
			this.MutuallyExclusivePerks = element.GetAttributeIdentifierImmutableHashSet("mutuallyexclusiveperks", ImmutableHashSet<Identifier>.Empty, true);
			this.SortKey = element.GetAttributeInt("sortkey", 0);
			ImmutableArray<PerkBase>.Builder builder = ImmutableArray.CreateBuilder<PerkBase>();
			foreach (ContentXElement child in element.Elements())
			{
				PerkBase perk;
				if (PerkBase.TryLoadFromXml(child, this, out perk))
				{
					builder.Add(perk);
				}
			}
			this.PerkBehaviors = builder.ToImmutable();
		}

		// Token: 0x0600385B RID: 14427 RVA: 0x00217BB8 File Offset: 0x00215DB8
		public override void Dispose()
		{
		}

		// Token: 0x04001C51 RID: 7249
		public static readonly PrefabCollection<DisembarkPerkPrefab> Prefabs = new PrefabCollection<DisembarkPerkPrefab>();
	}
}
