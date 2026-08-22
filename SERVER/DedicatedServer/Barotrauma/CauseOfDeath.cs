using System;

namespace Barotrauma
{
	// Token: 0x020000AE RID: 174
	internal class CauseOfDeath
	{
		// Token: 0x0600153A RID: 5434 RVA: 0x000B893C File Offset: 0x000B6B3C
		public CauseOfDeath(CauseOfDeathType type, AfflictionPrefab affliction, Character killer, Entity damageSource)
		{
			if (type == CauseOfDeathType.Affliction && affliction == null)
			{
				string errorMsg = "Invalid cause of death (the type of the cause of death was Affliction, but affliction was not specified).\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("InvalidCauseOfDeath", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				type = CauseOfDeathType.Unknown;
			}
			this.Type = type;
			this.Affliction = affliction;
			this.Killer = killer;
			this.DamageSource = damageSource;
		}

		// Token: 0x04000A1D RID: 2589
		public readonly CauseOfDeathType Type;

		// Token: 0x04000A1E RID: 2590
		public readonly AfflictionPrefab Affliction;

		// Token: 0x04000A1F RID: 2591
		public readonly Character Killer;

		// Token: 0x04000A20 RID: 2592
		public readonly Entity DamageSource;
	}
}
