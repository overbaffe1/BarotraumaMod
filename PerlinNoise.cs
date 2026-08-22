using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200034A RID: 842
	public static class PerlinNoise
	{
		// Token: 0x06004223 RID: 16931 RVA: 0x00248F10 File Offset: 0x00247110
		public static double OctavePerlin(double x, double y, double z, double frequency, int octaves, double persistence)
		{
			double total = 0.0;
			double amplitude = 3.0;
			for (int i = 0; i < octaves; i++)
			{
				total += PerlinNoise.CalculatePerlin(x * frequency, y * frequency, z * frequency) * amplitude;
				amplitude *= persistence;
				frequency *= 2.0;
			}
			return total;
		}

		// Token: 0x06004224 RID: 16932 RVA: 0x00248F64 File Offset: 0x00247164
		static PerlinNoise()
		{
			for (int x = 0; x < 512; x++)
			{
				PerlinNoise.p[x] = PerlinNoise.permutation[x % 256];
			}
			float minValue = float.MaxValue;
			float maxValue = float.MinValue;
			PerlinNoise.cachedNoise = new float[65536];
			for (int x2 = 0; x2 < 256; x2++)
			{
				for (int y = 0; y < 256; y++)
				{
					PerlinNoise.cachedNoise[x2 + 256 * y] = (float)PerlinNoise.OctavePerlin((double)x2 / 256.0, (double)y / 256.0, 0.5, 10.0, 4, 0.5);
				}
			}
			for (int i = 0; i < 65536; i++)
			{
				minValue = Math.Min(PerlinNoise.cachedNoise[i], minValue);
				maxValue = Math.Max(PerlinNoise.cachedNoise[i], maxValue);
			}
			for (int j = 0; j < 65536; j++)
			{
				PerlinNoise.cachedNoise[j] = (PerlinNoise.cachedNoise[j] - minValue) / (maxValue - minValue);
			}
		}

		// Token: 0x06004225 RID: 16933 RVA: 0x002490A4 File Offset: 0x002472A4
		public static float GetPerlin(float x, float y)
		{
			x = Math.Abs(x) % 1f;
			y = Math.Abs(y) % 1f;
			float xIndex = (x < 0.5f) ? (x * 2f * 256f) : (256f - (x - 0.5f) * 2f * 256f);
			xIndex = Math.Min(xIndex, 255f);
			float yIndex = (y < 0.5f) ? (y * 2f * 256f) : (256f - (y - 0.5f) * 2f * 256f);
			yIndex = Math.Min(yIndex, 255f);
			int minX = (int)xIndex;
			int maxX = (int)Math.Ceiling((double)xIndex);
			int minY = (int)yIndex;
			int maxY = (int)Math.Ceiling((double)yIndex);
			return MathHelper.Lerp(MathHelper.Lerp(PerlinNoise.cachedNoise[minX + minY * 256], PerlinNoise.cachedNoise[maxX + minY * 256], xIndex % 1f), MathHelper.Lerp(PerlinNoise.cachedNoise[minX + maxY * 256], PerlinNoise.cachedNoise[maxX + maxY * 256], xIndex % 1f), yIndex % 1f);
		}

		// Token: 0x06004226 RID: 16934 RVA: 0x002491C8 File Offset: 0x002473C8
		public static double CalculatePerlin(double x, double y, double z)
		{
			int xi = (int)x & 255;
			int yi = (int)y & 255;
			int zi = (int)z & 255;
			double xf = x - (double)((int)x);
			double yf = y - (double)((int)y);
			double zf = z - (double)((int)z);
			double u = PerlinNoise.Fade(xf);
			double v = PerlinNoise.Fade(yf);
			double w = PerlinNoise.Fade(zf);
			int a = PerlinNoise.p[xi] + yi;
			int aa = PerlinNoise.p[a] + zi;
			int ab = PerlinNoise.p[a + 1] + zi;
			int b = PerlinNoise.p[xi + 1] + yi;
			int ba = PerlinNoise.p[b] + zi;
			int bb = PerlinNoise.p[b + 1] + zi;
			double x2 = PerlinNoise.Lerp(PerlinNoise.Grad(PerlinNoise.p[aa], xf, yf, zf), PerlinNoise.Grad(PerlinNoise.p[ba], xf - 1.0, yf, zf), u);
			double x3 = PerlinNoise.Lerp(PerlinNoise.Grad(PerlinNoise.p[ab], xf, yf - 1.0, zf), PerlinNoise.Grad(PerlinNoise.p[bb], xf - 1.0, yf - 1.0, zf), u);
			double y2 = PerlinNoise.Lerp(x2, x3, v);
			x2 = PerlinNoise.Lerp(PerlinNoise.Grad(PerlinNoise.p[aa + 1], xf, yf, zf - 1.0), PerlinNoise.Grad(PerlinNoise.p[ba + 1], xf - 1.0, yf, zf - 1.0), u);
			x3 = PerlinNoise.Lerp(PerlinNoise.Grad(PerlinNoise.p[ab + 1], xf, yf - 1.0, zf - 1.0), PerlinNoise.Grad(PerlinNoise.p[bb + 1], xf - 1.0, yf - 1.0, zf - 1.0), u);
			double y3 = PerlinNoise.Lerp(x2, x3, v);
			return (PerlinNoise.Lerp(y2, y3, w) + 1.0) / 2.0;
		}

		// Token: 0x06004227 RID: 16935 RVA: 0x002493D8 File Offset: 0x002475D8
		public static double Grad(int hash, double x, double y, double z)
		{
			int h = hash & 15;
			double u = (h < 8) ? x : y;
			double v;
			if (h < 4)
			{
				v = y;
			}
			else if (h == 12 || h == 14)
			{
				v = x;
			}
			else
			{
				v = z;
			}
			return (((h & 1) == 0) ? u : (-u)) + (((h & 2) == 0) ? v : (-v));
		}

		// Token: 0x06004228 RID: 16936 RVA: 0x00249420 File Offset: 0x00247620
		public static double Fade(double t)
		{
			return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
		}

		// Token: 0x06004229 RID: 16937 RVA: 0x00249449 File Offset: 0x00247649
		public static double Lerp(double a, double b, double x)
		{
			return a + x * (b - a);
		}

		// Token: 0x04002267 RID: 8807
		private static readonly int[] permutation = new int[]
		{
			151,
			160,
			137,
			91,
			90,
			15,
			131,
			13,
			201,
			95,
			96,
			53,
			194,
			233,
			7,
			225,
			140,
			36,
			103,
			30,
			69,
			142,
			8,
			99,
			37,
			240,
			21,
			10,
			23,
			190,
			6,
			148,
			247,
			120,
			234,
			75,
			0,
			26,
			197,
			62,
			94,
			252,
			219,
			203,
			117,
			35,
			11,
			32,
			57,
			177,
			33,
			88,
			237,
			149,
			56,
			87,
			174,
			20,
			125,
			136,
			171,
			168,
			68,
			175,
			74,
			165,
			71,
			134,
			139,
			48,
			27,
			166,
			77,
			146,
			158,
			231,
			83,
			111,
			229,
			122,
			60,
			211,
			133,
			230,
			220,
			105,
			92,
			41,
			55,
			46,
			245,
			40,
			244,
			102,
			143,
			54,
			65,
			25,
			63,
			161,
			1,
			216,
			80,
			73,
			209,
			76,
			132,
			187,
			208,
			89,
			18,
			169,
			200,
			196,
			135,
			130,
			116,
			188,
			159,
			86,
			164,
			100,
			109,
			198,
			173,
			186,
			3,
			64,
			52,
			217,
			226,
			250,
			124,
			123,
			5,
			202,
			38,
			147,
			118,
			126,
			255,
			82,
			85,
			212,
			207,
			206,
			59,
			227,
			47,
			16,
			58,
			17,
			182,
			189,
			28,
			42,
			223,
			183,
			170,
			213,
			119,
			248,
			152,
			2,
			44,
			154,
			163,
			70,
			221,
			153,
			101,
			155,
			167,
			43,
			172,
			9,
			129,
			22,
			39,
			253,
			19,
			98,
			108,
			110,
			79,
			113,
			224,
			232,
			178,
			185,
			112,
			104,
			218,
			246,
			97,
			228,
			251,
			34,
			242,
			193,
			238,
			210,
			144,
			12,
			191,
			179,
			162,
			241,
			81,
			51,
			145,
			235,
			249,
			14,
			239,
			107,
			49,
			192,
			214,
			31,
			181,
			199,
			106,
			157,
			184,
			84,
			204,
			176,
			115,
			121,
			50,
			45,
			127,
			4,
			150,
			254,
			138,
			236,
			205,
			93,
			222,
			114,
			67,
			29,
			24,
			72,
			243,
			141,
			128,
			195,
			78,
			66,
			215,
			61,
			156,
			180
		};

		// Token: 0x04002268 RID: 8808
		private static readonly int[] p = new int[512];

		// Token: 0x04002269 RID: 8809
		private static readonly float[] cachedNoise;

		// Token: 0x0400226A RID: 8810
		private const int CacheResolution = 256;
	}
}
