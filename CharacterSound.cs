using System;
using System.Collections.Immutable;
using Barotrauma.Sounds;

namespace Barotrauma
{
	// Token: 0x0200002A RID: 42
	internal class CharacterSound
	{
		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x060006F7 RID: 1783 RVA: 0x0003D5F1 File Offset: 0x0003B7F1
		public CharacterSound.SoundType Type
		{
			get
			{
				return this.Params.State;
			}
		}

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x060006F8 RID: 1784 RVA: 0x0003D5FE File Offset: 0x0003B7FE
		public ImmutableHashSet<Identifier> TagSet
		{
			get
			{
				return this.Params.TagSet;
			}
		}

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x060006F9 RID: 1785 RVA: 0x0003D60B File Offset: 0x0003B80B
		public float Volume
		{
			get
			{
				if (this.roundSound != null)
				{
					return this.roundSound.Volume;
				}
				return 0f;
			}
		}

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x060006FA RID: 1786 RVA: 0x0003D626 File Offset: 0x0003B826
		public float Range
		{
			get
			{
				if (this.roundSound != null)
				{
					return this.roundSound.Range;
				}
				return 0f;
			}
		}

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x060006FB RID: 1787 RVA: 0x0003D641 File Offset: 0x0003B841
		public Sound Sound
		{
			get
			{
				RoundSound roundSound = this.roundSound;
				if (roundSound == null)
				{
					return null;
				}
				return roundSound.Sound;
			}
		}

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x060006FC RID: 1788 RVA: 0x0003D654 File Offset: 0x0003B854
		public bool IgnoreMuffling
		{
			get
			{
				RoundSound roundSound = this.roundSound;
				return roundSound != null && roundSound.IgnoreMuffling;
			}
		}

		// Token: 0x060006FD RID: 1789 RVA: 0x0003D667 File Offset: 0x0003B867
		public CharacterSound(CharacterParams.SoundParams soundParams)
		{
			this.Params = soundParams;
			this.roundSound = RoundSound.Load(soundParams.Element);
		}

		// Token: 0x04000393 RID: 915
		private readonly RoundSound roundSound;

		// Token: 0x04000394 RID: 916
		public readonly CharacterParams.SoundParams Params;

		// Token: 0x020006DE RID: 1758
		public enum SoundType
		{
			// Token: 0x04003824 RID: 14372
			Idle,
			// Token: 0x04003825 RID: 14373
			Attack,
			// Token: 0x04003826 RID: 14374
			Die,
			// Token: 0x04003827 RID: 14375
			Damage,
			// Token: 0x04003828 RID: 14376
			Happy,
			// Token: 0x04003829 RID: 14377
			Unhappy
		}
	}
}
