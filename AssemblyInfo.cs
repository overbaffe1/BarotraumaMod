using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

// Token: 0x02000006 RID: 6
public static class AssemblyInfo
{
	// Token: 0x06000017 RID: 23 RVA: 0x000023C0 File Offset: 0x000005C0
	static AssemblyInfo()
	{
		Assembly asm = typeof(AssemblyInfo).Assembly;
		IEnumerable<AssemblyMetadataAttribute> attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
		AssemblyMetadataAttribute assemblyMetadataAttribute = attrs.FirstOrDefault((AssemblyMetadataAttribute a) => a.Key == "GitRevision");
		AssemblyInfo.GitRevision = ((assemblyMetadataAttribute != null) ? assemblyMetadataAttribute.Value : null);
		AssemblyMetadataAttribute assemblyMetadataAttribute2 = attrs.FirstOrDefault((AssemblyMetadataAttribute a) => a.Key == "GitBranch");
		AssemblyInfo.GitBranch = ((assemblyMetadataAttribute2 != null) ? assemblyMetadataAttribute2.Value : null);
		AssemblyMetadataAttribute assemblyMetadataAttribute3 = attrs.FirstOrDefault((AssemblyMetadataAttribute a) => a.Key == "ProjectDir");
		AssemblyInfo.ProjectDir = ((assemblyMetadataAttribute3 != null) ? assemblyMetadataAttribute3.Value : null);
		if (AssemblyInfo.ProjectDir.Last<char>() == '/' || AssemblyInfo.ProjectDir.Last<char>() == '\\')
		{
			AssemblyInfo.ProjectDir = AssemblyInfo.ProjectDir.Substring(0, AssemblyInfo.ProjectDir.Length - 1);
		}
		string[] dirSplit = AssemblyInfo.ProjectDir.Split(new char[]
		{
			'/',
			'\\'
		});
		AssemblyInfo.ProjectDir = string.Join<string>(AssemblyInfo.ProjectDir.Contains('/') ? '/' : '\\', dirSplit.Take(dirSplit.Length - 2));
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
		defaultInterpolatedStringHandler.AppendFormatted<AssemblyInfo.Configuration>(AssemblyInfo.Configuration.Release);
		defaultInterpolatedStringHandler.AppendFormatted<AssemblyInfo.Platform>(AssemblyInfo.Platform.Windows);
		AssemblyInfo.BuildString = defaultInterpolatedStringHandler.ToStringAndClear();
	}

	// Token: 0x06000018 RID: 24 RVA: 0x000024FA File Offset: 0x000006FA
	public static string CleanupStackTrace(this string stackTrace)
	{
		return stackTrace.Replace(AssemblyInfo.ProjectDir, "<DEV>");
	}

	// Token: 0x04000008 RID: 8
	public static readonly string GitRevision;

	// Token: 0x04000009 RID: 9
	public static readonly string GitBranch;

	// Token: 0x0400000A RID: 10
	public static readonly string ProjectDir;

	// Token: 0x0400000B RID: 11
	public static readonly string BuildString;

	// Token: 0x0400000C RID: 12
	public const AssemblyInfo.Platform CurrentPlatform = AssemblyInfo.Platform.Windows;

	// Token: 0x0400000D RID: 13
	public const AssemblyInfo.Configuration CurrentConfiguration = AssemblyInfo.Configuration.Release;

	// Token: 0x02000631 RID: 1585
	public enum Platform
	{
		// Token: 0x04003614 RID: 13844
		Windows,
		// Token: 0x04003615 RID: 13845
		MacOS,
		// Token: 0x04003616 RID: 13846
		Linux
	}

	// Token: 0x02000632 RID: 1586
	public enum Configuration
	{
		// Token: 0x04003618 RID: 13848
		Release,
		// Token: 0x04003619 RID: 13849
		Unstable,
		// Token: 0x0400361A RID: 13850
		Debug
	}
}
