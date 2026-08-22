using System;

namespace Barotrauma
{
	// Token: 0x02000378 RID: 888
	public readonly struct LanguageIdentifier
	{
		// Token: 0x170011B7 RID: 4535
		// (get) Token: 0x060043CE RID: 17358 RVA: 0x00254A6C File Offset: 0x00252C6C
		public int ValueHash
		{
			get
			{
				return this.Value.GetHashCode();
			}
		}

		// Token: 0x060043CF RID: 17359 RVA: 0x00254A7F File Offset: 0x00252C7F
		public LanguageIdentifier(Identifier value)
		{
			this.Value = value;
		}

		// Token: 0x060043D0 RID: 17360 RVA: 0x00254A88 File Offset: 0x00252C88
		public override bool Equals(object obj)
		{
			if (obj is LanguageIdentifier)
			{
				LanguageIdentifier other = (LanguageIdentifier)obj;
				return this == other;
			}
			return base.Equals(obj);
		}

		// Token: 0x060043D1 RID: 17361 RVA: 0x00254AC2 File Offset: 0x00252CC2
		public override int GetHashCode()
		{
			return this.ValueHash;
		}

		// Token: 0x060043D2 RID: 17362 RVA: 0x00254ACA File Offset: 0x00252CCA
		public static bool operator ==(LanguageIdentifier a, LanguageIdentifier b)
		{
			return a.Value == b.Value;
		}

		// Token: 0x060043D3 RID: 17363 RVA: 0x00254ADF File Offset: 0x00252CDF
		public static bool operator !=(LanguageIdentifier a, LanguageIdentifier b)
		{
			return !(a == b);
		}

		// Token: 0x060043D4 RID: 17364 RVA: 0x00254AEB File Offset: 0x00252CEB
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04002381 RID: 9089
		public static readonly LanguageIdentifier None = "None".ToLanguageIdentifier();

		// Token: 0x04002382 RID: 9090
		public readonly Identifier Value;
	}
}
