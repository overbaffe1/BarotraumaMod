using System;

namespace Barotrauma
{
	// Token: 0x02000276 RID: 630
	public abstract class PrefabWithUintIdentifier : Prefab
	{
		// Token: 0x17000D5E RID: 3422
		// (get) Token: 0x06002CFC RID: 11516 RVA: 0x00128ADC File Offset: 0x00126CDC
		// (set) Token: 0x06002CFD RID: 11517 RVA: 0x00128AE4 File Offset: 0x00126CE4
		public uint UintIdentifier { get; set; }

		// Token: 0x06002CFE RID: 11518 RVA: 0x00128AED File Offset: 0x00126CED
		protected PrefabWithUintIdentifier(ContentFile file, Identifier identifier) : base(file, identifier)
		{
		}

		// Token: 0x06002CFF RID: 11519 RVA: 0x00128AF7 File Offset: 0x00126CF7
		protected PrefabWithUintIdentifier(ContentFile file, ContentXElement element) : base(file, element)
		{
		}
	}
}
