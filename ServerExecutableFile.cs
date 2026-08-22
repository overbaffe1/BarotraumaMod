using System;
using System.Runtime.CompilerServices;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000240 RID: 576
	internal sealed class ServerExecutableFile : OtherFile
	{
		// Token: 0x06003733 RID: 14131 RVA: 0x00214883 File Offset: 0x00212A83
		public ServerExecutableFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003734 RID: 14132 RVA: 0x00214890 File Offset: 0x00212A90
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

		// Token: 0x06003735 RID: 14133 RVA: 0x00214924 File Offset: 0x00212B24
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
