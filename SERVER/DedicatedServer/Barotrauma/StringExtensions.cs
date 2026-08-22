using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001CD RID: 461
	[NullableContext(2)]
	[Nullable(0)]
	internal static class StringExtensions
	{
		// Token: 0x0600222F RID: 8751 RVA: 0x000E5ED0 File Offset: 0x000E40D0
		public static bool IsNullOrEmpty([NotNullWhen(false)] this ContentPath p)
		{
			return p == null || p.IsPathNullOrEmpty();
		}

		// Token: 0x06002230 RID: 8752 RVA: 0x000E5EDD File Offset: 0x000E40DD
		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this ContentPath p)
		{
			return p == null || p.IsPathNullOrWhiteSpace();
		}

		// Token: 0x06002231 RID: 8753 RVA: 0x000E5EEA File Offset: 0x000E40EA
		public static bool IsNullOrEmpty([NotNullWhen(false)] this LocalizedString s)
		{
			return s == null || string.IsNullOrEmpty(s.Value);
		}

		// Token: 0x06002232 RID: 8754 RVA: 0x000E5EFC File Offset: 0x000E40FC
		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this LocalizedString s)
		{
			return s == null || string.IsNullOrWhiteSpace(s.Value);
		}

		// Token: 0x06002233 RID: 8755 RVA: 0x000E5F0E File Offset: 0x000E410E
		public static bool IsNullOrEmpty([NotNullWhen(false)] this RichString s)
		{
			return s == null || s.NestedStr.IsNullOrEmpty();
		}

		// Token: 0x06002234 RID: 8756 RVA: 0x000E5F20 File Offset: 0x000E4120
		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this RichString s)
		{
			return s == null || s.NestedStr.IsNullOrWhiteSpace();
		}
	}
}
