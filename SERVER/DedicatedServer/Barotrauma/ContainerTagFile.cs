using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200012C RID: 300
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class ContainerTagFile : GenericPrefabFile<ContainerTagPrefab>
	{
		// Token: 0x06001BC7 RID: 7111 RVA: 0x000CDADD File Offset: 0x000CBCDD
		public ContainerTagFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BC8 RID: 7112 RVA: 0x000CDAE7 File Offset: 0x000CBCE7
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "containertag";
		}

		// Token: 0x06001BC9 RID: 7113 RVA: 0x000CDAF5 File Offset: 0x000CBCF5
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "containertags";
		}

		// Token: 0x17000821 RID: 2081
		// (get) Token: 0x06001BCA RID: 7114 RVA: 0x000CDB03 File Offset: 0x000CBD03
		protected override PrefabCollection<ContainerTagPrefab> Prefabs
		{
			get
			{
				return ContainerTagPrefab.Prefabs;
			}
		}

		// Token: 0x06001BCB RID: 7115 RVA: 0x000CDB0A File Offset: 0x000CBD0A
		protected override ContainerTagPrefab CreatePrefab(ContentXElement element)
		{
			return new ContainerTagPrefab(element, this);
		}
	}
}
