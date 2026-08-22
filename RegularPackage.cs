using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000256 RID: 598
	public class RegularPackage : ContentPackage
	{
		// Token: 0x060037C3 RID: 14275 RVA: 0x0021640F File Offset: 0x0021460F
		public RegularPackage(XDocument doc, string path) : base(doc, path)
		{
			base.AssertCondition(!doc.Root.GetAttributeBool("corepackage", false), "Expected a regular package, got a core package");
		}
	}
}
