using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B2 RID: 1458
	internal class ItemSound
	{
		// Token: 0x17001678 RID: 5752
		// (get) Token: 0x0600599A RID: 22938 RVA: 0x002E1F54 File Offset: 0x002E0154
		public float VolumeMultiplier
		{
			get
			{
				return this.RoundSound.Volume;
			}
		}

		// Token: 0x17001679 RID: 5753
		// (get) Token: 0x0600599B RID: 22939 RVA: 0x002E1F61 File Offset: 0x002E0161
		public float Range
		{
			get
			{
				return this.RoundSound.Range;
			}
		}

		// Token: 0x0600599C RID: 22940 RVA: 0x002E1F6E File Offset: 0x002E016E
		public ItemSound(RoundSound sound, ActionType type, bool loop = false, bool onlyPlayInSameSub = false)
		{
			this.RoundSound = sound;
			this.Type = type;
			this.Loop = loop;
			this.OnlyPlayInSameSub = onlyPlayInSameSub;
		}

		// Token: 0x04002DBE RID: 11710
		public readonly RoundSound RoundSound;

		// Token: 0x04002DBF RID: 11711
		public readonly ActionType Type;

		// Token: 0x04002DC0 RID: 11712
		public Identifier VolumeProperty;

		// Token: 0x04002DC1 RID: 11713
		public readonly bool Loop;

		// Token: 0x04002DC2 RID: 11714
		public readonly bool OnlyPlayInSameSub;
	}
}
