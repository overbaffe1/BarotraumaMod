using System;

namespace Barotrauma
{
	// Token: 0x02000379 RID: 889
	public static class LanguageIdentifierExtensions
	{
		// Token: 0x060043D6 RID: 17366 RVA: 0x00254B0F File Offset: 0x00252D0F
		public static LanguageIdentifier ToLanguageIdentifier(this Identifier identifier)
		{
			return new LanguageIdentifier(identifier);
		}

		// Token: 0x060043D7 RID: 17367 RVA: 0x00254B17 File Offset: 0x00252D17
		public static LanguageIdentifier ToLanguageIdentifier(this string str)
		{
			return str.ToIdentifier().ToLanguageIdentifier();
		}
	}
}
