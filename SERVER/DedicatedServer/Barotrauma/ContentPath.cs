using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000162 RID: 354
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class ContentPath
	{
		// Token: 0x1700084D RID: 2125
		// (get) Token: 0x06001CE1 RID: 7393 RVA: 0x000D0844 File Offset: 0x000CEA44
		public string Value
		{
			get
			{
				ContentPath.<>c__DisplayClass9_0 CS$<>8__locals1 = new ContentPath.<>c__DisplayClass9_0();
				if (this.RawValue.IsNullOrEmpty())
				{
					return "";
				}
				if (!this.cachedValue.IsNullOrEmpty())
				{
					return this.cachedValue;
				}
				ContentPath.<>c__DisplayClass9_0 CS$<>8__locals2 = CS$<>8__locals1;
				ContentPackage contentPackage = this.ContentPackage;
				CS$<>8__locals2.modName = ((contentPackage != null) ? contentPackage.Name : null);
				Regex otherModDirRegex = ContentPath.OtherModDirRegex;
				string rawValue = this.RawValue;
				if (rawValue == null)
				{
					throw new NullReferenceException("RawValue is null.");
				}
				HashSet<Identifier> otherMods = (from id in (from m in otherModDirRegex.Matches(rawValue)
				select m.Groups[1].Value.Trim().ToIdentifier()).Distinct<Identifier>()
				where !id.IsEmpty && id != CS$<>8__locals1.modName
				select id).ToHashSet<Identifier>();
				this.cachedValue = this.RawValue;
				if (this.ContentPackage != null)
				{
					string modPath = Path.GetDirectoryName(this.ContentPackage.Path);
					this.cachedValue = this.cachedValue.Replace("%ModDir%", modPath, StringComparison.OrdinalIgnoreCase).Replace(string.Format("%ModDir:{0}%", this.ContentPackage.Name), modPath, StringComparison.OrdinalIgnoreCase);
					ContentPackageId ugcId2;
					if (this.ContentPackage.UgcId.TryUnwrap(out ugcId2))
					{
						this.cachedValue = this.cachedValue.Replace(string.Format("%ModDir:{0}%", ugcId2.StringRepresentation), modPath, StringComparison.OrdinalIgnoreCase);
					}
				}
				IEnumerable<ContentPackage> allPackages = ContentPackageManager.AllPackages;
				using (HashSet<Identifier>.Enumerator enumerator = otherMods.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Identifier otherModName = enumerator.Current;
						Option<ContentPackageId> ugcId = ContentPackageId.Parse(otherModName.Value);
						ContentPackage contentPackage2;
						if ((contentPackage2 = allPackages.FirstOrDefault((ContentPackage p) => ugcId.IsSome() && ugcId == p.UgcId)) == null && (contentPackage2 = allPackages.FirstOrDefault((ContentPackage p) => p.Name == otherModName)) == null && (contentPackage2 = allPackages.FirstOrDefault((ContentPackage p) => p.NameMatches(otherModName))) == null)
						{
							throw new MissingContentPackageException(this.ContentPackage, otherModName.Value);
						}
						ContentPackage otherMod = contentPackage2;
						this.cachedValue = this.cachedValue.Replace(string.Format("%ModDir:{0}%", otherModName.Value), Path.GetDirectoryName(otherMod.Path));
					}
				}
				this.cachedValue = this.cachedValue.CleanUpPath();
				return this.cachedValue;
			}
		}

		// Token: 0x1700084E RID: 2126
		// (get) Token: 0x06001CE2 RID: 7394 RVA: 0x000D0AA4 File Offset: 0x000CECA4
		public string FullPath
		{
			get
			{
				if (this.cachedFullPath.IsNullOrEmpty())
				{
					if (this.Value.IsNullOrEmpty())
					{
						return "";
					}
					this.cachedFullPath = Path.GetFullPath(this.Value).CleanUpPathCrossPlatform(false, "");
				}
				return this.cachedFullPath;
			}
		}

		// Token: 0x06001CE3 RID: 7395 RVA: 0x000D0AF3 File Offset: 0x000CECF3
		[NullableContext(2)]
		private ContentPath(ContentPackage contentPackage, string rawValue)
		{
			this.ContentPackage = contentPackage;
			this.RawValue = rawValue;
			this.cachedValue = null;
			this.cachedFullPath = null;
		}

		// Token: 0x06001CE4 RID: 7396 RVA: 0x000D0B17 File Offset: 0x000CED17
		public static ContentPath FromRaw([Nullable(2)] string rawValue)
		{
			return ContentPath.FromRaw(null, rawValue);
		}

		// Token: 0x06001CE5 RID: 7397 RVA: 0x000D0B20 File Offset: 0x000CED20
		[NullableContext(2)]
		[return: Nullable(1)]
		public static ContentPath FromRaw(ContentPackage contentPackage, string rawValue)
		{
			return new ContentPath(contentPackage, rawValue);
		}

		// Token: 0x06001CE6 RID: 7398 RVA: 0x000D0B38 File Offset: 0x000CED38
		[NullableContext(2)]
		private static bool StringEquality(string a, string b)
		{
			if (a.IsNullOrEmpty() || b.IsNullOrEmpty())
			{
				return a.IsNullOrEmpty() == b.IsNullOrEmpty();
			}
			return string.Equals(Path.GetFullPath(a.CleanUpPathCrossPlatform(false, "") ?? ""), Path.GetFullPath(b.CleanUpPathCrossPlatform(false, "") ?? ""), StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06001CE7 RID: 7399 RVA: 0x000D0B9E File Offset: 0x000CED9E
		public static bool operator ==(ContentPath a, ContentPath b)
		{
			return ContentPath.StringEquality((a != null) ? a.Value : null, (b != null) ? b.Value : null);
		}

		// Token: 0x06001CE8 RID: 7400 RVA: 0x000D0BBD File Offset: 0x000CEDBD
		public static bool operator !=(ContentPath a, ContentPath b)
		{
			return !(a == b);
		}

		// Token: 0x06001CE9 RID: 7401 RVA: 0x000D0BC9 File Offset: 0x000CEDC9
		public static bool operator ==(ContentPath a, [Nullable(2)] string b)
		{
			return ContentPath.StringEquality((a != null) ? a.Value : null, b);
		}

		// Token: 0x06001CEA RID: 7402 RVA: 0x000D0BDD File Offset: 0x000CEDDD
		public static bool operator !=(ContentPath a, [Nullable(2)] string b)
		{
			return !(a == b);
		}

		// Token: 0x06001CEB RID: 7403 RVA: 0x000D0BE9 File Offset: 0x000CEDE9
		public static bool operator ==([Nullable(2)] string a, ContentPath b)
		{
			return ContentPath.StringEquality(a, (b != null) ? b.Value : null);
		}

		// Token: 0x06001CEC RID: 7404 RVA: 0x000D0BFD File Offset: 0x000CEDFD
		public static bool operator !=([Nullable(2)] string a, ContentPath b)
		{
			return !(a == b);
		}

		// Token: 0x06001CED RID: 7405 RVA: 0x000D0C0C File Offset: 0x000CEE0C
		protected bool Equals(ContentPath other)
		{
			return this.RawValue == other.RawValue && object.Equals(this.ContentPackage, other.ContentPackage) && this.cachedValue == other.cachedValue && this.cachedFullPath == other.cachedFullPath;
		}

		// Token: 0x06001CEE RID: 7406 RVA: 0x000D0C65 File Offset: 0x000CEE65
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((ContentPath)obj)));
		}

		// Token: 0x06001CEF RID: 7407 RVA: 0x000D0C93 File Offset: 0x000CEE93
		public override int GetHashCode()
		{
			return HashCode.Combine<string, ContentPackage, string, string>(this.RawValue, this.ContentPackage, this.cachedValue, this.cachedFullPath);
		}

		// Token: 0x06001CF0 RID: 7408 RVA: 0x000D0CB2 File Offset: 0x000CEEB2
		public bool IsPathNullOrEmpty()
		{
			return string.IsNullOrEmpty(this.Value);
		}

		// Token: 0x06001CF1 RID: 7409 RVA: 0x000D0CBF File Offset: 0x000CEEBF
		public bool IsPathNullOrWhiteSpace()
		{
			return string.IsNullOrWhiteSpace(this.Value);
		}

		// Token: 0x06001CF2 RID: 7410 RVA: 0x000D0CCC File Offset: 0x000CEECC
		public bool EndsWith(string suffix)
		{
			return this.Value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06001CF3 RID: 7411 RVA: 0x000D0CDB File Offset: 0x000CEEDB
		[NullableContext(2)]
		public override string ToString()
		{
			return this.Value;
		}

		// Token: 0x04000D22 RID: 3362
		public static readonly ContentPath Empty = new ContentPath(null, "");

		// Token: 0x04000D23 RID: 3363
		public const string ModDirStr = "%ModDir%";

		// Token: 0x04000D24 RID: 3364
		public const string OtherModDirFmt = "%ModDir:{0}%";

		// Token: 0x04000D25 RID: 3365
		private static readonly Regex OtherModDirRegex = new Regex(string.Format("%ModDir:{0}%", "(.+?)"));

		// Token: 0x04000D26 RID: 3366
		[Nullable(2)]
		public readonly string RawValue;

		// Token: 0x04000D27 RID: 3367
		[Nullable(2)]
		public readonly ContentPackage ContentPackage;

		// Token: 0x04000D28 RID: 3368
		[Nullable(2)]
		private string cachedValue;

		// Token: 0x04000D29 RID: 3369
		[Nullable(2)]
		private string cachedFullPath;

		// Token: 0x04000D2A RID: 3370
		[Nullable(2)]
		private static ContentPath prevCreatedRaw;
	}
}
