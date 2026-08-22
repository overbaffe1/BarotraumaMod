using System;

namespace Barotrauma
{
	// Token: 0x020002AD RID: 685
	public static class LanguageIdentifierExtensions
	{
		// Token: 0x06002F3E RID: 12094 RVA: 0x0013A5CB File Offset: 0x001387CB
		public static LanguageIdentifier ToLanguageIdentifier(this Identifier identifier)
		{
			return new LanguageIdentifier(identifier);
		}

		// Token: 0x06002F3F RID: 12095 RVA: 0x0013A5D3 File Offset: 0x001387D3
		public static LanguageIdentifier ToLanguageIdentifier(this string str)
		{
			return str.ToIdentifier().ToLanguageIdentifier();
		}
	}
}
