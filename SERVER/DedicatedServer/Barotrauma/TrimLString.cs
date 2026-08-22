using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A6 RID: 678
	public class TrimLString : LocalizedString
	{
		// Token: 0x06002EDD RID: 11997 RVA: 0x0013909C File Offset: 0x0013729C
		[NullableContext(1)]
		public TrimLString(LocalizedString nestedStr, TrimLString.Mode mode, [Nullable(2)] char[] trimCharacters = null)
		{
			this.nestedStr = nestedStr;
			this.mode = mode;
			this.trimCharacters = trimCharacters;
		}

		// Token: 0x17000DAA RID: 3498
		// (get) Token: 0x06002EDE RID: 11998 RVA: 0x001390B9 File Offset: 0x001372B9
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06002EDF RID: 11999 RVA: 0x001390C8 File Offset: 0x001372C8
		public override void RetrieveValue()
		{
			this.cachedValue = this.nestedStr.Value;
			if (this.mode.HasFlag(TrimLString.Mode.Start))
			{
				this.cachedValue = this.cachedValue.TrimStart(this.trimCharacters);
			}
			if (this.mode.HasFlag(TrimLString.Mode.End))
			{
				this.cachedValue = this.cachedValue.TrimEnd(this.trimCharacters);
			}
			base.UpdateLanguage();
		}

		// Token: 0x04001781 RID: 6017
		[Nullable(1)]
		private readonly LocalizedString nestedStr;

		// Token: 0x04001782 RID: 6018
		private readonly TrimLString.Mode mode;

		// Token: 0x04001783 RID: 6019
		[Nullable(2)]
		private readonly char[] trimCharacters;

		// Token: 0x02000B30 RID: 2864
		[Flags]
		public enum Mode
		{
			// Token: 0x040038D0 RID: 14544
			Start = 1,
			// Token: 0x040038D1 RID: 14545
			End = 2,
			// Token: 0x040038D2 RID: 14546
			Both = 3
		}
	}
}
