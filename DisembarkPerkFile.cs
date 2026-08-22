using System;

namespace Barotrauma
{
	// Token: 0x02000227 RID: 551
	internal sealed class DisembarkPerkFile : GenericPrefabFile<DisembarkPerkPrefab>
	{
		// Token: 0x060036C1 RID: 14017 RVA: 0x00213971 File Offset: 0x00211B71
		public DisembarkPerkFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036C2 RID: 14018 RVA: 0x0021397B File Offset: 0x00211B7B
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "disembarkperk";
		}

		// Token: 0x060036C3 RID: 14019 RVA: 0x00213989 File Offset: 0x00211B89
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "disembarkperks";
		}

		// Token: 0x17000E85 RID: 3717
		// (get) Token: 0x060036C4 RID: 14020 RVA: 0x00213997 File Offset: 0x00211B97
		protected override PrefabCollection<DisembarkPerkPrefab> Prefabs
		{
			get
			{
				return DisembarkPerkPrefab.Prefabs;
			}
		}

		// Token: 0x060036C5 RID: 14021 RVA: 0x0021399E File Offset: 0x00211B9E
		protected override DisembarkPerkPrefab CreatePrefab(ContentXElement element)
		{
			return new DisembarkPerkPrefab(element, this);
		}
	}
}
