using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005E9 RID: 1513
	internal struct FoliageConfig
	{
		// Token: 0x06006333 RID: 25395 RVA: 0x0033B138 File Offset: 0x00339338
		public readonly int Serialize()
		{
			int variant = Math.Min(this.Variant + 1, 15);
			int scale = (int)(this.Scale * 10f);
			int rotation = (int)(this.Rotation / 6.2831855f * 10f);
			return variant | scale << 4 | rotation << 8;
		}

		// Token: 0x06006334 RID: 25396 RVA: 0x0033B180 File Offset: 0x00339380
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

		// Token: 0x06006335 RID: 25397 RVA: 0x0033B1E0 File Offset: 0x003393E0
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

		// Token: 0x04003366 RID: 13158
		public static FoliageConfig EmptyConfig = new FoliageConfig
		{
			Variant = -1,
			Rotation = 0f,
			Scale = 1f
		};

		// Token: 0x04003367 RID: 13159
		public static readonly int EmptyConfigValue = FoliageConfig.EmptyConfig.Serialize();

		// Token: 0x04003368 RID: 13160
		public int Variant;

		// Token: 0x04003369 RID: 13161
		public float Rotation;

		// Token: 0x0400336A RID: 13162
		public float Scale;
	}
}
