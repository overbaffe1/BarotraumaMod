using System;

namespace Barotrauma
{
	// Token: 0x020001BE RID: 446
	internal sealed class SavedStatValue
	{
		// Token: 0x17000CEE RID: 3310
		// (get) Token: 0x06003161 RID: 12641 RVA: 0x002044AE File Offset: 0x002026AE
		// (set) Token: 0x06003162 RID: 12642 RVA: 0x002044B6 File Offset: 0x002026B6
		public Identifier StatIdentifier { get; set; }

		// Token: 0x17000CEF RID: 3311
		// (get) Token: 0x06003163 RID: 12643 RVA: 0x002044BF File Offset: 0x002026BF
		// (set) Token: 0x06003164 RID: 12644 RVA: 0x002044C7 File Offset: 0x002026C7
		public float StatValue { get; set; }

		// Token: 0x17000CF0 RID: 3312
		// (get) Token: 0x06003165 RID: 12645 RVA: 0x002044D0 File Offset: 0x002026D0
		// (set) Token: 0x06003166 RID: 12646 RVA: 0x002044D8 File Offset: 0x002026D8
		public bool RemoveOnDeath { get; set; }

		// Token: 0x06003167 RID: 12647 RVA: 0x002044E1 File Offset: 0x002026E1
		public SavedStatValue(Identifier statIdentifier, float value, bool removeOnDeath)
		{
			this.StatValue = value;
			this.RemoveOnDeath = removeOnDeath;
			this.StatIdentifier = statIdentifier;
		}
	}
}
