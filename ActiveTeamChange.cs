using System;

namespace Barotrauma
{
	// Token: 0x020001B3 RID: 435
	internal class ActiveTeamChange
	{
		// Token: 0x17000CD1 RID: 3281
		// (get) Token: 0x0600310B RID: 12555 RVA: 0x00203933 File Offset: 0x00201B33
		public CharacterTeamType DesiredTeamId { get; }

		// Token: 0x17000CD2 RID: 3282
		// (get) Token: 0x0600310C RID: 12556 RVA: 0x0020393B File Offset: 0x00201B3B
		public ActiveTeamChange.TeamChangePriorities TeamChangePriority { get; }

		// Token: 0x17000CD3 RID: 3283
		// (get) Token: 0x0600310D RID: 12557 RVA: 0x00203943 File Offset: 0x00201B43
		public bool AggressiveBehavior { get; }

		// Token: 0x0600310E RID: 12558 RVA: 0x0020394B File Offset: 0x00201B4B
		public ActiveTeamChange(CharacterTeamType desiredTeamId, ActiveTeamChange.TeamChangePriorities teamChangePriority, bool aggressiveBehavior = false)
		{
			this.DesiredTeamId = desiredTeamId;
			this.TeamChangePriority = teamChangePriority;
			this.AggressiveBehavior = aggressiveBehavior;
		}

		// Token: 0x02000EA0 RID: 3744
		public enum TeamChangePriorities
		{
			// Token: 0x040052BF RID: 21183
			Base,
			// Token: 0x040052C0 RID: 21184
			Willful,
			// Token: 0x040052C1 RID: 21185
			Absolute
		}
	}
}
