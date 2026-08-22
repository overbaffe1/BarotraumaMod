using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000257 RID: 599
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class ContentPath
	{
		// Token: 0x17000EAB RID: 3755
		// (get) Token: 0x060037C4 RID: 14276 RVA: 0x00216438 File Offset: 0x00214638
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
				ModDownloadScreen modDownloadScreen = GameMain.ModDownloadScreen;
				if (((modDownloadScreen != null) ? modDownloadScreen.DownloadedPackages : null) != null)
				{
					allPackages = allPackages.Concat(GameMain.ModDownloadScreen.DownloadedPackages);
				}
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

		// Token: 0x17000EAC RID: 3756
		// (get) Token: 0x060037C5 RID: 14277 RVA: 0x002166BC File Offset: 0x002148BC
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

		// Token: 0x060037C6 RID: 14278 RVA: 0x0021670B File Offset: 0x0021490B
		[NullableContext(2)]
		private ContentPath(ContentPackage contentPackage, string rawValue)
		{
			this.ContentPackage = contentPackage;
			this.RawValue = rawValue;
			this.cachedValue = null;
			this.cachedFullPath = null;
		}

		// Token: 0x060037C7 RID: 14279 RVA: 0x0021672F File Offset: 0x0021492F
		public static ContentPath FromRaw([Nullable(2)] string rawValue)
		{
			return ContentPath.FromRaw(null, rawValue);
		}

		// Token: 0x060037C8 RID: 14280 RVA: 0x00216738 File Offset: 0x00214938
		[NullableContext(2)]
		[return: Nullable(1)]
		public static ContentPath FromRaw(ContentPackage contentPackage, string rawValue)
		{
			return new ContentPath(contentPackage, rawValue);
		}

		// Token: 0x060037C9 RID: 14281 RVA: 0x00216750 File Offset: 0x00214950
		[NullableContext(2)]
		private static bool StringEquality(string a, string b)
		{
			if (a.IsNullOrEmpty() || b.IsNullOrEmpty())
			{
				return a.IsNullOrEmpty() == b.IsNullOrEmpty();
			}
			return string.Equals(Path.GetFullPath(a.CleanUpPathCrossPlatform(false, "") ?? ""), Path.GetFullPath(b.CleanUpPathCrossPlatform(false, "") ?? ""), StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060037CA RID: 14282 RVA: 0x002167B6 File Offset: 0x002149B6
		public static bool operator ==(ContentPath a, ContentPath b)
		{
			return ContentPath.StringEquality((a != null) ? a.Value : null, (b != null) ? b.Value : null);
		}

		// Token: 0x060037CB RID: 14283 RVA: 0x002167D5 File Offset: 0x002149D5
		public static bool operator !=(ContentPath a, ContentPath b)
		{
			return !(a == b);
		}

		// Token: 0x060037CC RID: 14284 RVA: 0x002167E1 File Offset: 0x002149E1
		public static bool operator ==(ContentPath a, [Nullable(2)] string b)
		{
			return ContentPath.StringEquality((a != null) ? a.Value : null, b);
		}

		// Token: 0x060037CD RID: 14285 RVA: 0x002167F5 File Offset: 0x002149F5
		public static bool operator !=(ContentPath a, [Nullable(2)] string b)
		{
			return !(a == b);
		}

		// Token: 0x060037CE RID: 14286 RVA: 0x00216801 File Offset: 0x00214A01
		public static bool operator ==([Nullable(2)] string a, ContentPath b)
		{
			return ContentPath.StringEquality(a, (b != null) ? b.Value : null);
		}

		// Token: 0x060037CF RID: 14287 RVA: 0x00216815 File Offset: 0x00214A15
		public static bool operator !=([Nullable(2)] string a, ContentPath b)
		{
			return !(a == b);
		}

		// Token: 0x060037D0 RID: 14288 RVA: 0x00216824 File Offset: 0x00214A24
		protected bool Equals(ContentPath other)
		{
			return this.RawValue == other.RawValue && object.Equals(this.ContentPackage, other.ContentPackage) && this.cachedValue == other.cachedValue && this.cachedFullPath == other.cachedFullPath;
		}

		// Token: 0x060037D1 RID: 14289 RVA: 0x0021687D File Offset: 0x00214A7D
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((ContentPath)obj)));
		}

		// Token: 0x060037D2 RID: 14290 RVA: 0x002168AB File Offset: 0x00214AAB
		public override int GetHashCode()
		{
			return HashCode.Combine<string, ContentPackage, string, string>(this.RawValue, this.ContentPackage, this.cachedValue, this.cachedFullPath);
		}

		// Token: 0x060037D3 RID: 14291 RVA: 0x002168CA File Offset: 0x00214ACA
		public bool IsPathNullOrEmpty()
		{
			return string.IsNullOrEmpty(this.Value);
		}

		// Token: 0x060037D4 RID: 14292 RVA: 0x002168D7 File Offset: 0x00214AD7
		public bool IsPathNullOrWhiteSpace()
		{
			return string.IsNullOrWhiteSpace(this.Value);
		}

		// Token: 0x060037D5 RID: 14293 RVA: 0x002168E4 File Offset: 0x00214AE4
		public bool EndsWith(string suffix)
		{
			return this.Value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060037D6 RID: 14294 RVA: 0x002168F3 File Offset: 0x00214AF3
		[NullableContext(2)]
		public override string ToString()
		{
			return this.Value;
		}

		// Token: 0x04001C27 RID: 7207
		public static readonly ContentPath Empty = new ContentPath(null, "");

		// Token: 0x04001C28 RID: 7208
		public const string ModDirStr = "%ModDir%";

		// Token: 0x04001C29 RID: 7209
		public const string OtherModDirFmt = "%ModDir:{0}%";

		// Token: 0x04001C2A RID: 7210
		private static readonly Regex OtherModDirRegex = new Regex(string.Format("%ModDir:{0}%", "(.+?)"));

		// Token: 0x04001C2B RID: 7211
		[Nullable(2)]
		public readonly string RawValue;

		// Token: 0x04001C2C RID: 7212
		[Nullable(2)]
		public readonly ContentPackage ContentPackage;

		// Token: 0x04001C2D RID: 7213
		[Nullable(2)]
		private string cachedValue;

		// Token: 0x04001C2E RID: 7214
		[Nullable(2)]
		private string cachedFullPath;

		// Token: 0x04001C2F RID: 7215
		[Nullable(2)]
		private static ContentPath prevCreatedRaw;
	}
}
