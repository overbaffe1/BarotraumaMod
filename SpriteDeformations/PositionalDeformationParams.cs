using System;
using System.Xml.Linq;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000437 RID: 1079
	internal class PositionalDeformationParams : SpriteDeformationParams
	{
		// Token: 0x17001255 RID: 4693
		// (get) Token: 0x0600481D RID: 18461 RVA: 0x0027948B File Offset: 0x0027768B
		// (set) Token: 0x0600481E RID: 18462 RVA: 0x00279493 File Offset: 0x00277693
		[Serialize(0f, IsPropertySaveable.Yes, "0 = no falloff, the entire sprite is stretched, 1 = stretching the center of the sprite has no effect at the edges.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float Falloff { get; set; }

		// Token: 0x17001256 RID: 4694
		// (get) Token: 0x0600481F RID: 18463 RVA: 0x0027949C File Offset: 0x0027769C
		// (set) Token: 0x06004820 RID: 18464 RVA: 0x002794A4 File Offset: 0x002776A4
		[Serialize(1f, IsPropertySaveable.Yes, "Maximum stretch per vertex (1 = the size of the sprite)", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float MaxDeformation { get; set; }

		// Token: 0x17001257 RID: 4695
		// (get) Token: 0x06004821 RID: 18465 RVA: 0x002794AD File Offset: 0x002776AD
		// (set) Token: 0x06004822 RID: 18466 RVA: 0x002794B5 File Offset: 0x002776B5
		[Serialize(10f, IsPropertySaveable.Yes, "How fast the sprite reacts to being stretched", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float ReactionSpeed { get; set; }

		// Token: 0x17001258 RID: 4696
		// (get) Token: 0x06004823 RID: 18467 RVA: 0x002794BE File Offset: 0x002776BE
		// (set) Token: 0x06004824 RID: 18468 RVA: 0x002794C6 File Offset: 0x002776C6
		[Serialize(0.05f, IsPropertySaveable.Yes, "How fast the sprite returns back to normal after stretching ends", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float RecoverSpeed { get; set; }

		// Token: 0x06004825 RID: 18469 RVA: 0x002794CF File Offset: 0x002776CF
		public PositionalDeformationParams(XElement element) : base(element)
		{
		}
	}
}
