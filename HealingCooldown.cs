using System;

namespace Barotrauma
{
	// Token: 0x0200002E RID: 46
	internal static class HealingCooldown
	{
		// Token: 0x17000224 RID: 548
		// (get) Token: 0x060007C9 RID: 1993 RVA: 0x00047E9C File Offset: 0x0004609C
		public static float NormalizedCooldown
		{
			get
			{
				return MathF.Min((float)(DateTimeOffset.UtcNow - HealingCooldown.OnCooldownUntil).TotalSeconds / 0.5f, 0f);
			}
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x060007CA RID: 1994 RVA: 0x00047ED1 File Offset: 0x000460D1
		public static bool IsOnCooldown
		{
			get
			{
				return DateTimeOffset.UtcNow < HealingCooldown.OnCooldownUntil;
			}
		}

		// Token: 0x060007CB RID: 1995 RVA: 0x00047EE4 File Offset: 0x000460E4
		public static void PutOnCooldown()
		{
			HealingCooldown.OnCooldownUntil = DateTimeOffset.UtcNow.AddSeconds(0.5);
		}

		// Token: 0x0400040E RID: 1038
		private static DateTimeOffset OnCooldownUntil = DateTimeOffset.MinValue;

		// Token: 0x0400040F RID: 1039
		private const float CooldownDuration = 0.5f;
	}
}
