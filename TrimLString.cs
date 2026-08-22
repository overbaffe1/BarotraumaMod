using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000372 RID: 882
	public class TrimLString : LocalizedString
	{
		// Token: 0x06004372 RID: 17266 RVA: 0x002534DC File Offset: 0x002516DC
		[NullableContext(1)]
		public TrimLString(LocalizedString nestedStr, TrimLString.Mode mode, [Nullable(2)] char[] trimCharacters = null)
		{
			this.nestedStr = nestedStr;
			this.mode = mode;
			this.trimCharacters = trimCharacters;
		}

		// Token: 0x170011AC RID: 4524
		// (get) Token: 0x06004373 RID: 17267 RVA: 0x002534F9 File Offset: 0x002516F9
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06004374 RID: 17268 RVA: 0x00253508 File Offset: 0x00251708
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

		// Token: 0x04002361 RID: 9057
		[Nullable(1)]
		private readonly LocalizedString nestedStr;

		// Token: 0x04002362 RID: 9058
		private readonly TrimLString.Mode mode;

		// Token: 0x04002363 RID: 9059
		[Nullable(2)]
		private readonly char[] trimCharacters;

		// Token: 0x0200108C RID: 4236
		[Flags]
		public enum Mode
		{
			// Token: 0x04005904 RID: 22788
			Start = 1,
			// Token: 0x04005905 RID: 22789
			End = 2,
			// Token: 0x04005906 RID: 22790
			Both = 3
		}
	}
}
