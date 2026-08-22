using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004B6 RID: 1206
	internal struct FoliageConfig
	{
		// Token: 0x0600448F RID: 17551 RVA: 0x001B7538 File Offset: 0x001B5738
		public readonly int Serialize()
		{
			int variant = Math.Min(this.Variant + 1, 15);
			int scale = (int)(this.Scale * 10f);
			int rotation = (int)(this.Rotation / 6.2831855f * 10f);
			return variant | scale << 4 | rotation << 8;
		}

		// Token: 0x06004490 RID: 17552 RVA: 0x001B7580 File Offset: 0x001B5780
		public static FoliageConfig Deserialize(int value)
		{
			int variant = value & 15;
			int scale = (value & 240) >> 4;
			int rotation = (value & 3840) >> 8;
			return new FoliageConfig
			{
				Variant = variant - 1,
				Scale = (float)scale / 10f,
				Rotation = (float)rotation / 10f * 6.2831855f
			};
		}

		// Token: 0x06004491 RID: 17553 RVA: 0x001B75E0 File Offset: 0x001B57E0
		[NullableContext(2)]
		public static FoliageConfig CreateRandomConfig(int maxVariants, float minScale, float maxScale, Random random = null)
		{
			int flowerVariant = Growable.RandomInt(0, maxVariants, random);
			float flowerScale = (float)Growable.RandomDouble((double)minScale, (double)maxScale, random);
			float flowerRotation = (float)Growable.RandomDouble(0.0, 6.2831854820251465, random);
			return new FoliageConfig
			{
				Variant = flowerVariant,
				Scale = flowerScale,
				Rotation = flowerRotation
			};
		}

		// Token: 0x040020DE RID: 8414
		public static FoliageConfig EmptyConfig = new FoliageConfig
		{
			Variant = -1,
			Rotation = 0f,
			Scale = 1f
		};

		// Token: 0x040020DF RID: 8415
		public static readonly int EmptyConfigValue = FoliageConfig.EmptyConfig.Serialize();

		// Token: 0x040020E0 RID: 8416
		public int Variant;

		// Token: 0x040020E1 RID: 8417
		public float Rotation;

		// Token: 0x040020E2 RID: 8418
		public float Scale;
	}
}
