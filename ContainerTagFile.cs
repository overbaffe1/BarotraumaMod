using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000222 RID: 546
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class ContainerTagFile : GenericPrefabFile<ContainerTagPrefab>
	{
		// Token: 0x060036A6 RID: 13990 RVA: 0x00213576 File Offset: 0x00211776
		public ContainerTagFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036A7 RID: 13991 RVA: 0x00213580 File Offset: 0x00211780
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "containertag";
		}

		// Token: 0x060036A8 RID: 13992 RVA: 0x0021358E File Offset: 0x0021178E
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "containertags";
		}

		// Token: 0x17000E82 RID: 3714
		// (get) Token: 0x060036A9 RID: 13993 RVA: 0x0021359C File Offset: 0x0021179C
		protected override PrefabCollection<ContainerTagPrefab> Prefabs
		{
			get
			{
				return ContainerTagPrefab.Prefabs;
			}
		}

		// Token: 0x060036AA RID: 13994 RVA: 0x002135A3 File Offset: 0x002117A3
		protected override ContainerTagPrefab CreatePrefab(ContentXElement element)
		{
			return new ContainerTagPrefab(element, this);
		}
	}
}
