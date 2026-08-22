using System;

namespace Barotrauma
{
	// Token: 0x020001B0 RID: 432
	internal class CauseOfDeath
	{
		// Token: 0x060030FD RID: 12541 RVA: 0x00203750 File Offset: 0x00201950
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

		// Token: 0x04001988 RID: 6536
		public readonly CauseOfDeathType Type;

		// Token: 0x04001989 RID: 6537
		public readonly AfflictionPrefab Affliction;

		// Token: 0x0400198A RID: 6538
		public readonly Character Killer;

		// Token: 0x0400198B RID: 6539
		public readonly Entity DamageSource;
	}
}
