using System;
using System.Xml.Linq;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000435 RID: 1077
	internal class NoiseDeformationParams : SpriteDeformationParams
	{
		// Token: 0x17001251 RID: 4689
		// (get) Token: 0x06004811 RID: 18449 RVA: 0x002792D6 File Offset: 0x002774D6
		// (set) Token: 0x06004812 RID: 18450 RVA: 0x002792DE File Offset: 0x002774DE
		[Serialize(0f, IsPropertySaveable.Yes, "The frequency of the noise.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2, ValueStep = 1f)]
		public override float Frequency { get; set; }

		// Token: 0x17001252 RID: 4690
		// (get) Token: 0x06004813 RID: 18451 RVA: 0x002792E7 File Offset: 0x002774E7
		// (set) Token: 0x06004814 RID: 18452 RVA: 0x002792EF File Offset: 0x002774EF
		[Serialize(1f, IsPropertySaveable.Yes, "How much the noise distorts the sprite.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2, ValueStep = 0.01f)]
		public float Amplitude { get; set; }

		// Token: 0x17001253 RID: 4691
		// (get) Token: 0x06004815 RID: 18453 RVA: 0x002792F8 File Offset: 0x002774F8
		// (set) Token: 0x06004816 RID: 18454 RVA: 0x00279300 File Offset: 0x00277500
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the noise changes.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2, ValueStep = 0.01f)]
		public float ChangeSpeed { get; set; }

		// Token: 0x06004817 RID: 18455 RVA: 0x00279309 File Offset: 0x00277509
		public NoiseDeformationParams(XElement element) : base(element)
		{
		}
	}
}
