using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000124 RID: 292
	[NullableContext(1)]
	[Nullable(0)]
	public class IgnoredHints
	{
		// Token: 0x060027CC RID: 10188 RVA: 0x001BBB33 File Offset: 0x001B9D33
		private IgnoredHints()
		{
		}

		// Token: 0x060027CD RID: 10189 RVA: 0x001BBB46 File Offset: 0x001B9D46
		private IgnoredHints(XElement element)
		{
			this.identifiers = element.GetAttributeIdentifierArray("identifiers", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
		}

		// Token: 0x060027CE RID: 10190 RVA: 0x001BBB75 File Offset: 0x001B9D75
		[NullableContext(2)]
		public static void Init(XElement element)
		{
			if (element == null)
			{
				return;
			}
			IgnoredHints.Instance = new IgnoredHints(element);
		}

		// Token: 0x060027CF RID: 10191 RVA: 0x001BBB86 File Offset: 0x001B9D86
		public void SaveTo(XElement element)
		{
			element.SetAttributeValue("identifiers", string.Join<Identifier>(",", this.identifiers));
		}

		// Token: 0x060027D0 RID: 10192 RVA: 0x001BBBA8 File Offset: 0x001B9DA8
		public bool Contains(Identifier identifier)
		{
			return this.identifiers.Contains(identifier);
		}

		// Token: 0x060027D1 RID: 10193 RVA: 0x001BBBB6 File Offset: 0x001B9DB6
		public void Add(Identifier identifier)
		{
			this.identifiers.Add(identifier);
		}

		// Token: 0x060027D2 RID: 10194 RVA: 0x001BBBC5 File Offset: 0x001B9DC5
		public void Remove(Identifier identifier)
		{
			this.identifiers.Remove(identifier);
		}

		// Token: 0x060027D3 RID: 10195 RVA: 0x001BBBD4 File Offset: 0x001B9DD4
		public void Clear()
		{
			this.identifiers.Clear();
		}

		// Token: 0x17000A51 RID: 2641
		// (get) Token: 0x060027D4 RID: 10196 RVA: 0x001BBBE1 File Offset: 0x001B9DE1
		// (set) Token: 0x060027D5 RID: 10197 RVA: 0x001BBBE8 File Offset: 0x001B9DE8
		public static IgnoredHints Instance { get; private set; } = new IgnoredHints();

		// Token: 0x04001440 RID: 5184
		private readonly HashSet<Identifier> identifiers = new HashSet<Identifier>();
	}
}
