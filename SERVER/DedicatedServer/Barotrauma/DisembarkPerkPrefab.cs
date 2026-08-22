using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Barotrauma.PerkBehaviors;

namespace Barotrauma
{
	// Token: 0x02000170 RID: 368
	internal sealed class DisembarkPerkPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000868 RID: 2152
		// (get) Token: 0x06001D86 RID: 7558 RVA: 0x000D23BC File Offset: 0x000D05BC
		public LocalizedString Name { get; }

		// Token: 0x17000869 RID: 2153
		// (get) Token: 0x06001D87 RID: 7559 RVA: 0x000D23C4 File Offset: 0x000D05C4
		public LocalizedString Description { get; }

		// Token: 0x1700086A RID: 2154
		// (get) Token: 0x06001D88 RID: 7560 RVA: 0x000D23CC File Offset: 0x000D05CC
		public Identifier SortCategory { get; }

		// Token: 0x1700086B RID: 2155
		// (get) Token: 0x06001D89 RID: 7561 RVA: 0x000D23D4 File Offset: 0x000D05D4
		public int SortKey { get; }

		// Token: 0x1700086C RID: 2156
		// (get) Token: 0x06001D8A RID: 7562 RVA: 0x000D23DC File Offset: 0x000D05DC
		public Identifier Prerequisite { get; }

		// Token: 0x1700086D RID: 2157
		// (get) Token: 0x06001D8B RID: 7563 RVA: 0x000D23E4 File Offset: 0x000D05E4
		public ImmutableHashSet<Identifier> MutuallyExclusivePerks { get; }

		// Token: 0x1700086E RID: 2158
		// (get) Token: 0x06001D8C RID: 7564 RVA: 0x000D23EC File Offset: 0x000D05EC
		public int Cost { get; }

		// Token: 0x1700086F RID: 2159
		// (get) Token: 0x06001D8D RID: 7565 RVA: 0x000D23F4 File Offset: 0x000D05F4
		public ImmutableArray<PerkBase> PerkBehaviors { get; }

		// Token: 0x06001D8E RID: 7566 RVA: 0x000D23FC File Offset: 0x000D05FC
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

		// Token: 0x06001D8F RID: 7567 RVA: 0x000D2580 File Offset: 0x000D0780
		public override void Dispose()
		{
		}

		// Token: 0x04000D5A RID: 3418
		public static readonly PrefabCollection<DisembarkPerkPrefab> Prefabs = new PrefabCollection<DisembarkPerkPrefab>();
	}
}
