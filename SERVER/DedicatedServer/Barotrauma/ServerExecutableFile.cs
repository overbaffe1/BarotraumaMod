using System;
using System.Runtime.CompilerServices;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x0200014A RID: 330
	internal sealed class ServerExecutableFile : OtherFile
	{
		// Token: 0x06001C4F RID: 7247 RVA: 0x000CED36 File Offset: 0x000CCF36
		public ServerExecutableFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C50 RID: 7248 RVA: 0x000CED40 File Offset: 0x000CCF40
		public static ContentPath MutateContentPath(ContentPath path)
		{
			ServerExecutableFile.<>c__DisplayClass1_0 CS$<>8__locals1;
			CS$<>8__locals1.path = path;
			if (File.Exists(CS$<>8__locals1.path.FullPath))
			{
				return CS$<>8__locals1.path;
			}
			CS$<>8__locals1.path = ContentPath.FromRaw(CS$<>8__locals1.path.ContentPackage, ServerExecutableFile.<MutateContentPath>g__rawValueWithoutExtension|1_0(ref CS$<>8__locals1));
			if (File.Exists(CS$<>8__locals1.path.FullPath))
			{
				return CS$<>8__locals1.path;
			}
			CS$<>8__locals1.path = ContentPath.FromRaw(CS$<>8__locals1.path.ContentPackage, ServerExecutableFile.<MutateContentPath>g__rawValueWithoutExtension|1_0(ref CS$<>8__locals1) + ".exe");
			return CS$<>8__locals1.path;
		}

		// Token: 0x06001C51 RID: 7249 RVA: 0x000CEDD4 File Offset: 0x000CCFD4
		[CompilerGenerated]
		internal static string <MutateContentPath>g__rawValueWithoutExtension|1_0(ref ServerExecutableFile.<>c__DisplayClass1_0 A_0)
		{
			return Barotrauma.IO.Path.Combine(new string[]
			{
				Barotrauma.IO.Path.GetDirectoryName(A_0.path.RawValue ?? ""),
				Barotrauma.IO.Path.GetFileNameWithoutExtension(A_0.path.RawValue ?? "")
			}).CleanUpPath();
		}
	}
}
