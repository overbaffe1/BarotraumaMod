using System;

namespace Barotrauma
{
	// Token: 0x0200022E RID: 558
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class ItemFile : GenericPrefabFile<ItemPrefab>
	{
		// Token: 0x060036E1 RID: 14049 RVA: 0x00213C49 File Offset: 0x00211E49
		public ItemFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036E2 RID: 14050 RVA: 0x00213C53 File Offset: 0x00211E53
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x060036E3 RID: 14051 RVA: 0x00213C5F File Offset: 0x00211E5F
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "items";
		}

		// Token: 0x17000E8A RID: 3722
		// (get) Token: 0x060036E4 RID: 14052 RVA: 0x00213C6D File Offset: 0x00211E6D
		protected override PrefabCollection<ItemPrefab> Prefabs
		{
			get
			{
				return ItemPrefab.Prefabs;
			}
		}

		// Token: 0x060036E5 RID: 14053 RVA: 0x00213C74 File Offset: 0x00211E74
		protected override ItemPrefab CreatePrefab(ContentXElement element)
		{
			return new ItemPrefab(element, this);
		}
	}
}
