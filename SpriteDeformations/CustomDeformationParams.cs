using System;
using System.Xml.Linq;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x0200042F RID: 1071
	internal class CustomDeformationParams : SpriteDeformationParams
	{
		// Token: 0x17001244 RID: 4676
		// (get) Token: 0x060047EB RID: 18411 RVA: 0x00278843 File Offset: 0x00276A43
		// (set) Token: 0x060047EC RID: 18412 RVA: 0x0027884B File Offset: 0x00276A4B
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the deformation \"oscillates\" back and forth. For example, if the sprite is stretched up, setting this value above zero would make it do a wave-like movement up and down.", "", false)]
		public override float Frequency { get; set; } = 1f;

		// Token: 0x17001245 RID: 4677
		// (get) Token: 0x060047ED RID: 18413 RVA: 0x00278854 File Offset: 0x00276A54
		// (set) Token: 0x060047EE RID: 18414 RVA: 0x0027885C File Offset: 0x00276A5C
		[Serialize(1f, IsPropertySaveable.Yes, "The \"strength\" of the deformation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float Amplitude { get; set; }

		// Token: 0x060047EF RID: 18415 RVA: 0x00278865 File Offset: 0x00276A65
		public CustomDeformationParams(XElement element) : base(element)
		{
		}
	}
}
