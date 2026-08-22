using System;

namespace Barotrauma
{
	// Token: 0x020002AC RID: 684
	public readonly struct LanguageIdentifier
	{
		// Token: 0x17000DB4 RID: 3508
		// (get) Token: 0x06002F36 RID: 12086 RVA: 0x0013A528 File Offset: 0x00138728
		public int ValueHash
		{
			get
			{
				return this.Value.GetHashCode();
			}
		}

		// Token: 0x06002F37 RID: 12087 RVA: 0x0013A53B File Offset: 0x0013873B
		public LanguageIdentifier(Identifier value)
		{
			this.Value = value;
		}

		// Token: 0x06002F38 RID: 12088 RVA: 0x0013A544 File Offset: 0x00138744
		public override bool Equals(object obj)
		{
			if (obj is LanguageIdentifier)
			{
				LanguageIdentifier other = (LanguageIdentifier)obj;
				return this == other;
			}
			return base.Equals(obj);
		}

		// Token: 0x06002F39 RID: 12089 RVA: 0x0013A57E File Offset: 0x0013877E
		public override int GetHashCode()
		{
			return this.ValueHash;
		}

		// Token: 0x06002F3A RID: 12090 RVA: 0x0013A586 File Offset: 0x00138786
		public static bool operator ==(LanguageIdentifier a, LanguageIdentifier b)
		{
			return a.Value == b.Value;
		}

		// Token: 0x06002F3B RID: 12091 RVA: 0x0013A59B File Offset: 0x0013879B
		public static bool operator !=(LanguageIdentifier a, LanguageIdentifier b)
		{
			return !(a == b);
		}

		// Token: 0x06002F3C RID: 12092 RVA: 0x0013A5A7 File Offset: 0x001387A7
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400179E RID: 6046
		public static readonly LanguageIdentifier None = "None".ToLanguageIdentifier();

		// Token: 0x0400179F RID: 6047
		public readonly Identifier Value;
	}
}
