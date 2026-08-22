using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020002C8 RID: 712
	public static class Rand
	{
		// Token: 0x06003025 RID: 12325 RVA: 0x0014B056 File Offset: 0x00149256
		public static Random GetRNG(Rand.RandSync randSync)
		{
			Rand.CheckRandThreadSafety(randSync);
			if (randSync != Rand.RandSync.Unsynced)
			{
				return Rand.syncedRandom[randSync];
			}
			return Rand.localRandom;
		}

		// Token: 0x06003026 RID: 12326 RVA: 0x0014B072 File Offset: 0x00149272
		public static void SetLocalRandom(int seed)
		{
			Rand.localRandom = new Random(seed);
		}

		// Token: 0x06003027 RID: 12327 RVA: 0x0014B07F File Offset: 0x0014927F
		public static void SetSyncedSeed(int seed)
		{
			Rand.syncedRandom[Rand.RandSync.ServerAndClient] = new MTRandom(seed);
		}

		// Token: 0x06003028 RID: 12328 RVA: 0x0014B092 File Offset: 0x00149292
		private static void CheckRandThreadSafety(Rand.RandSync sync)
		{
			if (Rand.ThreadId != 0 && sync == Rand.RandSync.ServerAndClient && Environment.CurrentManagedThreadId != Rand.ThreadId)
			{
				DebugConsole.ThrowError("Unauthorized multithreaded access to RandSync.ServerAndClient\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
			}
		}

		// Token: 0x06003029 RID: 12329 RVA: 0x0014B0C8 File Offset: 0x001492C8
		public static float Range(float minimum, float maximum, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			return Rand.GetRNG(sync).Range(minimum, maximum);
		}

		// Token: 0x0600302A RID: 12330 RVA: 0x0014B0D7 File Offset: 0x001492D7
		public static double Range(double minimum, double maximum, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			return Rand.GetRNG(sync).Range(minimum, maximum);
		}

		// Token: 0x0600302B RID: 12331 RVA: 0x0014B0E6 File Offset: 0x001492E6
		public static int Range(int minimum, int maximum, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			Rand.CheckRandThreadSafety(sync);
			return ((sync == Rand.RandSync.Unsynced) ? Rand.localRandom : Rand.syncedRandom[sync]).Next(maximum - minimum) + minimum;
		}

		// Token: 0x0600302C RID: 12332 RVA: 0x0014B10D File Offset: 0x0014930D
		public static int Int(int max, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			Rand.CheckRandThreadSafety(sync);
			return ((sync == Rand.RandSync.Unsynced) ? Rand.localRandom : Rand.syncedRandom[sync]).Next(max);
		}

		// Token: 0x0600302D RID: 12333 RVA: 0x0014B130 File Offset: 0x00149330
		public static Vector2 Vector(float length, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			Vector2 randomVector = new Vector2(Rand.Range(-1f, 1f, sync), Rand.Range(-1f, 1f, sync));
			if (randomVector.LengthSquared() < 0.001f)
			{
				return new Vector2(0f, length);
			}
			return Vector2.Normalize(randomVector) * length;
		}

		// Token: 0x0600302E RID: 12334 RVA: 0x0014B18A File Offset: 0x0014938A
		public static float Value(Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			return Rand.Range(0f, 1f, sync);
		}

		// Token: 0x0600302F RID: 12335 RVA: 0x0014B19C File Offset: 0x0014939C
		public static Color Color(bool randomAlpha = false, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			if (randomAlpha)
			{
				return new Color(Rand.Value(sync), Rand.Value(sync), Rand.Value(sync), Rand.Value(sync));
			}
			return new Color(Rand.Value(sync), Rand.Value(sync), Rand.Value(sync));
		}

		// Token: 0x06003030 RID: 12336 RVA: 0x0014B1D8 File Offset: 0x001493D8
		public static DoubleVector2 Vector(double length, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			double x = Rand.Range(-1.0, 1.0, sync);
			double y = Rand.Range(-1.0, 1.0, sync);
			double len = Math.Sqrt(x * x + y * y);
			if (len < 1E-05)
			{
				return new DoubleVector2(0.0, length);
			}
			return new DoubleVector2(x / len * length, y / len * length);
		}

		// Token: 0x0400181C RID: 6172
		private static Random localRandom = new Random();

		// Token: 0x0400181D RID: 6173
		private static readonly Dictionary<Rand.RandSync, Random> syncedRandom = new Dictionary<Rand.RandSync, Random>
		{
			{
				Rand.RandSync.ServerAndClient,
				new MTRandom()
			}
		};

		// Token: 0x0400181E RID: 6174
		public static int ThreadId = 0;

		// Token: 0x02000B69 RID: 2921
		public enum RandSync
		{
			// Token: 0x04003968 RID: 14696
			Unsynced,
			// Token: 0x04003969 RID: 14697
			ServerAndClient
		}
	}
}
