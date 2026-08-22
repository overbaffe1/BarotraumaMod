using System;

namespace Barotrauma
{
	// Token: 0x02000349 RID: 841
	public abstract class PrefabWithUintIdentifier : Prefab
	{
		// Token: 0x17001185 RID: 4485
		// (get) Token: 0x0600421F RID: 16927 RVA: 0x00248EE8 File Offset: 0x002470E8
		// (set) Token: 0x06004220 RID: 16928 RVA: 0x00248EF0 File Offset: 0x002470F0
		public uint UintIdentifier { get; set; }

		// Token: 0x06004221 RID: 16929 RVA: 0x00248EF9 File Offset: 0x002470F9
		protected PrefabWithUintIdentifier(ContentFile file, Identifier identifier) : base(file, identifier)
		{
		}

		// Token: 0x06004222 RID: 16930 RVA: 0x00248F03 File Offset: 0x00247103
		protected PrefabWithUintIdentifier(ContentFile file, ContentXElement element) : base(file, element)
		{
		}
	}
}
