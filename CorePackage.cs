using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000255 RID: 597
	public class CorePackage : ContentPackage
	{
		// Token: 0x060037C1 RID: 14273 RVA: 0x0021633C File Offset: 0x0021453C
		public CorePackage(XDocument doc, string path) : base(doc, path)
		{
			base.AssertCondition(doc.Root.GetAttributeBool("corepackage", false), "Expected a core package, got a regular package");
			IEnumerable<ContentFile.TypeInfo> missingFileTypes = from t in ContentFile.Types
			where t.RequiredByCorePackage && !base.Files.Any((ContentFile f) => t.Type == f.GetType() || t.AlternativeTypes.Contains(f.GetType()))
			select t;
			base.AssertCondition(!missingFileTypes.Any<ContentFile.TypeInfo>(), "Core package requires at least one of the following content types: " + string.Join(", ", from t in missingFileTypes
			select t.Type.Name));
		}
	}
}
