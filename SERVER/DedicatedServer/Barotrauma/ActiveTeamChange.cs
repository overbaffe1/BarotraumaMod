using System;

namespace Barotrauma
{
	// Token: 0x020000B1 RID: 177
	internal class ActiveTeamChange
	{
		// Token: 0x1700062B RID: 1579
		// (get) Token: 0x06001548 RID: 5448 RVA: 0x000B8B1F File Offset: 0x000B6D1F
		public CharacterTeamType DesiredTeamId { get; }

		// Token: 0x1700062C RID: 1580
		// (get) Token: 0x06001549 RID: 5449 RVA: 0x000B8B27 File Offset: 0x000B6D27
		public ActiveTeamChange.TeamChangePriorities TeamChangePriority { get; }

		// Token: 0x1700062D RID: 1581
		// (get) Token: 0x0600154A RID: 5450 RVA: 0x000B8B2F File Offset: 0x000B6D2F
		public bool AggressiveBehavior { get; }

		// Token: 0x0600154B RID: 5451 RVA: 0x000B8B37 File Offset: 0x000B6D37
		public ActiveTeamChange(CharacterTeamType desiredTeamId, ActiveTeamChange.TeamChangePriorities teamChangePriority, bool aggressiveBehavior = false)
		{
			this.DesiredTeamId = desiredTeamId;
			this.TeamChangePriority = teamChangePriority;
			this.AggressiveBehavior = aggressiveBehavior;
		}

		// Token: 0x0200086B RID: 2155
		public enum TeamChangePriorities
		{
			// Token: 0x04002FA4 RID: 12196
			Base,
			// Token: 0x04002FA5 RID: 12197
			Willful,
			// Token: 0x04002FA6 RID: 12198
			Absolute
		}
	}
}
