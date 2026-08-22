using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000160 RID: 352
	public class RegularPackage : ContentPackage
	{
		// Token: 0x06001CD2 RID: 7378 RVA: 0x000D04A3 File Offset: 0x000CE6A3
		public RegularPackage(XDocument doc, string path) : base(doc, path)
		{
			base.AssertCondition(!doc.Root.GetAttributeBool("corepackage", false), "Expected a regular package, got a core package");
		}
	}
}
