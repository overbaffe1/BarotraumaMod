using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000393 RID: 915
	public static class Rand
	{
		// Token: 0x060044A3 RID: 17571 RVA: 0x00264AFA File Offset: 0x00262CFA
		public static Random GetRNG(Rand.RandSync randSync)
		{
			Rand.CheckRandThreadSafety(randSync);
			if (randSync != Rand.RandSync.Unsynced)
			{
				return Rand.syncedRandom[randSync];
			}
			return Rand.localRandom;
		}

		// Token: 0x060044A4 RID: 17572 RVA: 0x00264B16 File Offset: 0x00262D16
		public static void SetLocalRandom(int seed)
		{
			Rand.localRandom = new Random(seed);
		}

		// Token: 0x060044A5 RID: 17573 RVA: 0x00264B23 File Offset: 0x00262D23
		public static void SetSyncedSeed(int seed)
		{
			Rand.syncedRandom[Rand.RandSync.ServerAndClient] = new MTRandom(seed);
			Rand.syncedRandom[Rand.RandSync.ClientOnly] = new MTRandom(seed);
		}

		// Token: 0x060044A6 RID: 17574 RVA: 0x00264B47 File Offset: 0x00262D47
		private static void CheckRandThreadSafety(Rand.RandSync sync)
		{
			if (Rand.ThreadId != 0 && sync == Rand.RandSync.ServerAndClient && Environment.CurrentManagedThreadId != Rand.ThreadId)
			{
				DebugConsole.ThrowError("Unauthorized multithreaded access to RandSync.ServerAndClient\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
			}
		}

		// Token: 0x060044A7 RID: 17575 RVA: 0x00264B7D File Offset: 0x00262D7D
		public static float Range(float minimum, float maximum, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			return Rand.GetRNG(sync).Range(minimum, maximum);
		}

		// Token: 0x060044A8 RID: 17576 RVA: 0x00264B8C File Offset: 0x00262D8C
		public static double Range(double minimum, double maximum, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			return Rand.GetRNG(sync).Range(minimum, maximum);
		}

		// Token: 0x060044A9 RID: 17577 RVA: 0x00264B9B File Offset: 0x00262D9B
		public static int Range(int minimum, int maximum, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			Rand.CheckRandThreadSafety(sync);
			return ((sync == Rand.RandSync.Unsynced) ? Rand.localRandom : Rand.syncedRandom[sync]).Next(maximum - minimum) + minimum;
		}

		// Token: 0x060044AA RID: 17578 RVA: 0x00264BC2 File Offset: 0x00262DC2
		public static int Int(int max, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			Rand.CheckRandThreadSafety(sync);
			return ((sync == Rand.RandSync.Unsynced) ? Rand.localRandom : Rand.syncedRandom[sync]).Next(max);
		}

		// Token: 0x060044AB RID: 17579 RVA: 0x00264BE8 File Offset: 0x00262DE8
		public static Vector2 Vector(float length, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			Vector2 randomVector = new Vector2(Rand.Range(-1f, 1f, sync), Rand.Range(-1f, 1f, sync));
			if (randomVector.LengthSquared() < 0.001f)
			{
				return new Vector2(0f, length);
			}
			return Vector2.Normalize(randomVector) * length;
		}

		// Token: 0x060044AC RID: 17580 RVA: 0x00264C42 File Offset: 0x00262E42
		public static float Value(Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			return Rand.Range(0f, 1f, sync);
		}

		// Token: 0x060044AD RID: 17581 RVA: 0x00264C54 File Offset: 0x00262E54
		public static Color Color(bool randomAlpha = false, Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			if (randomAlpha)
			{
				return new Color(Rand.Value(sync), Rand.Value(sync), Rand.Value(sync), Rand.Value(sync));
			}
			return new Color(Rand.Value(sync), Rand.Value(sync), Rand.Value(sync));
		}

		// Token: 0x060044AE RID: 17582 RVA: 0x00264C90 File Offset: 0x00262E90
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

		// Token: 0x040023EE RID: 9198
		private static Random localRandom = new Random();

		// Token: 0x040023EF RID: 9199
		private static readonly Dictionary<Rand.RandSync, Random> syncedRandom = new Dictionary<Rand.RandSync, Random>
		{
			{
				Rand.RandSync.ServerAndClient,
				new MTRandom()
			},
			{
				Rand.RandSync.ClientOnly,
				new MTRandom()
			}
		};

		// Token: 0x040023F0 RID: 9200
		public static int ThreadId = 0;

		// Token: 0x020010BA RID: 4282
		public enum RandSync
		{
			// Token: 0x04005985 RID: 22917
			Unsynced,
			// Token: 0x04005986 RID: 22918
			ServerAndClient,
			// Token: 0x04005987 RID: 22919
			ClientOnly
		}
	}
}
