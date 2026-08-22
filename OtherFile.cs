using System;

namespace Barotrauma
{
	// Token: 0x02000239 RID: 569
	[AlternativeContentTypeNames(new string[]
	{
		"None"
	})]
	public class OtherFile : HashlessFile
	{
		// Token: 0x06003718 RID: 14104 RVA: 0x00214485 File Offset: 0x00212685
		public OtherFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003719 RID: 14105 RVA: 0x0021448F File Offset: 0x0021268F
		public sealed override void LoadFile()
		{
		}

		// Token: 0x0600371A RID: 14106 RVA: 0x00214491 File Offset: 0x00212691
		public sealed override void UnloadFile()
		{
		}

		// Token: 0x0600371B RID: 14107 RVA: 0x00214493 File Offset: 0x00212693
		public sealed override void Sort()
		{
		}
	}
}
