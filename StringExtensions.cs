using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002BC RID: 700
	[NullableContext(2)]
	[Nullable(0)]
	internal static class StringExtensions
	{
		// Token: 0x06003C54 RID: 15444 RVA: 0x00229D80 File Offset: 0x00227F80
		public static bool IsNullOrEmpty([NotNullWhen(false)] this ContentPath p)
		{
			return p == null || p.IsPathNullOrEmpty();
		}

		// Token: 0x06003C55 RID: 15445 RVA: 0x00229D8D File Offset: 0x00227F8D
		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this ContentPath p)
		{
			return p == null || p.IsPathNullOrWhiteSpace();
		}

		// Token: 0x06003C56 RID: 15446 RVA: 0x00229D9A File Offset: 0x00227F9A
		public static bool IsNullOrEmpty([NotNullWhen(false)] this LocalizedString s)
		{
			return s == null || string.IsNullOrEmpty(s.Value);
		}

		// Token: 0x06003C57 RID: 15447 RVA: 0x00229DAC File Offset: 0x00227FAC
		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this LocalizedString s)
		{
			return s == null || string.IsNullOrWhiteSpace(s.Value);
		}

		// Token: 0x06003C58 RID: 15448 RVA: 0x00229DBE File Offset: 0x00227FBE
		public static bool IsNullOrEmpty([NotNullWhen(false)] this RichString s)
		{
			return s == null || s.NestedStr.IsNullOrEmpty();
		}

		// Token: 0x06003C59 RID: 15449 RVA: 0x00229DD0 File Offset: 0x00227FD0
		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this RichString s)
		{
			return s == null || s.NestedStr.IsNullOrWhiteSpace();
		}
	}
}
