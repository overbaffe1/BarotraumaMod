using System;
using System.Xml.Linq;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000431 RID: 1073
	internal class InflateParams : SpriteDeformationParams
	{
		// Token: 0x17001248 RID: 4680
		// (get) Token: 0x060047F7 RID: 18423 RVA: 0x00278C15 File Offset: 0x00276E15
		// (set) Token: 0x060047F8 RID: 18424 RVA: 0x00278C1D File Offset: 0x00276E1D
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2, ValueStep = 1f)]
		public override float Frequency { get; set; } = 1f;

		// Token: 0x17001249 RID: 4681
		// (get) Token: 0x060047F9 RID: 18425 RVA: 0x00278C26 File Offset: 0x00276E26
		// (set) Token: 0x060047FA RID: 18426 RVA: 0x00278C2E File Offset: 0x00276E2E
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f, DecimalCount = 2, ValueStep = 0.1f)]
		public float Scale { get; set; }

		// Token: 0x060047FB RID: 18427 RVA: 0x00278C37 File Offset: 0x00276E37
		public InflateParams(XElement element) : base(element)
		{
		}
	}
}
