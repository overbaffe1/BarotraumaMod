using System;

namespace Barotrauma
{
	// Token: 0x020000BD RID: 189
	internal sealed class SavedStatValue
	{
		// Token: 0x17000648 RID: 1608
		// (get) Token: 0x060015A1 RID: 5537 RVA: 0x000B96A6 File Offset: 0x000B78A6
		// (set) Token: 0x060015A2 RID: 5538 RVA: 0x000B96AE File Offset: 0x000B78AE
		public Identifier StatIdentifier { get; set; }

		// Token: 0x17000649 RID: 1609
		// (get) Token: 0x060015A3 RID: 5539 RVA: 0x000B96B7 File Offset: 0x000B78B7
		// (set) Token: 0x060015A4 RID: 5540 RVA: 0x000B96BF File Offset: 0x000B78BF
		public float StatValue { get; set; }

		// Token: 0x1700064A RID: 1610
		// (get) Token: 0x060015A5 RID: 5541 RVA: 0x000B96C8 File Offset: 0x000B78C8
		// (set) Token: 0x060015A6 RID: 5542 RVA: 0x000B96D0 File Offset: 0x000B78D0
		public bool RemoveOnDeath { get; set; }

		// Token: 0x060015A7 RID: 5543 RVA: 0x000B96D9 File Offset: 0x000B78D9
		public SavedStatValue(Identifier statIdentifier, float value, bool removeOnDeath)
		{
			this.StatValue = value;
			this.RemoveOnDeath = removeOnDeath;
			this.StatIdentifier = statIdentifier;
		}
	}
}
