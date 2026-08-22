using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000123 RID: 291
	[NullableContext(1)]
	[Nullable(0)]
	public class CompletedTutorials
	{
		// Token: 0x060027C2 RID: 10178 RVA: 0x001BB9D6 File Offset: 0x001B9BD6
		private CompletedTutorials()
		{
		}

		// Token: 0x060027C3 RID: 10179 RVA: 0x001BB9EC File Offset: 0x001B9BEC
		private CompletedTutorials(XElement element)
		{
			foreach (XElement subElement in element.Elements())
			{
				this.identifiers.Add(subElement.GetAttributeIdentifier("name", Identifier.Empty));
			}
		}

		// Token: 0x060027C4 RID: 10180 RVA: 0x001BBA60 File Offset: 0x001B9C60
		[NullableContext(2)]
		public static void Init(XElement element)
		{
			if (element == null)
			{
				return;
			}
			CompletedTutorials.Instance = new CompletedTutorials(element);
		}

		// Token: 0x060027C5 RID: 10181 RVA: 0x001BBA74 File Offset: 0x001B9C74
		public void SaveTo(XElement element)
		{
			foreach (Identifier id in this.identifiers)
			{
				element.Add(new XElement("Tutorial", new XAttribute("name", id.Value)));
			}
		}

		// Token: 0x060027C6 RID: 10182 RVA: 0x001BBAEC File Offset: 0x001B9CEC
		public bool Contains(Identifier identifier)
		{
			return this.identifiers.Contains(identifier);
		}

		// Token: 0x060027C7 RID: 10183 RVA: 0x001BBAFA File Offset: 0x001B9CFA
		public void Add(Identifier identifier)
		{
			this.identifiers.Add(identifier);
		}

		// Token: 0x060027C8 RID: 10184 RVA: 0x001BBB09 File Offset: 0x001B9D09
		public void Remove(Identifier identifier)
		{
			this.identifiers.Remove(identifier);
		}

		// Token: 0x17000A50 RID: 2640
		// (get) Token: 0x060027C9 RID: 10185 RVA: 0x001BBB18 File Offset: 0x001B9D18
		// (set) Token: 0x060027CA RID: 10186 RVA: 0x001BBB1F File Offset: 0x001B9D1F
		public static CompletedTutorials Instance { get; private set; } = new CompletedTutorials();

		// Token: 0x0400143E RID: 5182
		private readonly HashSet<Identifier> identifiers = new HashSet<Identifier>();
	}
}
