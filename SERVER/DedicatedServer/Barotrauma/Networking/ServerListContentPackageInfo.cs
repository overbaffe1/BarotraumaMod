using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020003C5 RID: 965
	public readonly struct ServerListContentPackageInfo : IEquatable<ServerListContentPackageInfo>
	{
		// Token: 0x060037E9 RID: 14313 RVA: 0x00176DA7 File Offset: 0x00174FA7
		public ServerListContentPackageInfo(string Name, string Hash, Option<ContentPackageId> Id)
		{
			this.Name = Name;
			this.Hash = Hash;
			this.Id = Id;
		}

		// Token: 0x17000F57 RID: 3927
		// (get) Token: 0x060037EA RID: 14314 RVA: 0x00176DBE File Offset: 0x00174FBE
		// (set) Token: 0x060037EB RID: 14315 RVA: 0x00176DC6 File Offset: 0x00174FC6
		public string Name { get; set; }

		// Token: 0x17000F58 RID: 3928
		// (get) Token: 0x060037EC RID: 14316 RVA: 0x00176DCF File Offset: 0x00174FCF
		// (set) Token: 0x060037ED RID: 14317 RVA: 0x00176DD7 File Offset: 0x00174FD7
		public string Hash { get; set; }

		// Token: 0x17000F59 RID: 3929
		// (get) Token: 0x060037EE RID: 14318 RVA: 0x00176DE0 File Offset: 0x00174FE0
		// (set) Token: 0x060037EF RID: 14319 RVA: 0x00176DE8 File Offset: 0x00174FE8
		public Option<ContentPackageId> Id { get; set; }

		// Token: 0x060037F0 RID: 14320 RVA: 0x00176DF1 File Offset: 0x00174FF1
		public ServerListContentPackageInfo(ContentPackage pkg)
		{
			this = new ServerListContentPackageInfo(pkg.Name, pkg.Hash.StringRepresentation, pkg.UgcId);
		}

		// Token: 0x060037F1 RID: 14321 RVA: 0x00176E10 File Offset: 0x00175010
		public static Option<ServerListContentPackageInfo> ParseSingleEntry(string singleEntry)
		{
			IReadOnlyList<string> split = singleEntry.SplitEscaped(',');
			if (split == null || split.Count != 3)
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			return Option.Some<ServerListContentPackageInfo>(new ServerListContentPackageInfo(split[0], split[1], ContentPackageId.Parse(split[2])));
		}

		// Token: 0x060037F2 RID: 14322 RVA: 0x00176E64 File Offset: 0x00175064
		public override string ToString()
		{
			string[] array = new string[3];
			array[0] = this.Name;
			array[1] = this.Hash;
			array[2] = (from id in this.Id
			select id.StringRepresentation).Fallback("");
			return array.JoinEscaped(',');
		}

		// Token: 0x060037F3 RID: 14323 RVA: 0x00176ED0 File Offset: 0x001750D0
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Name = ");
			builder.Append(this.Name);
			builder.Append(", Hash = ");
			builder.Append(this.Hash);
			builder.Append(", Id = ");
			builder.Append(this.Id.ToString());
			return true;
		}

		// Token: 0x060037F4 RID: 14324 RVA: 0x00176F37 File Offset: 0x00175137
		[CompilerGenerated]
		public static bool operator !=(ServerListContentPackageInfo left, ServerListContentPackageInfo right)
		{
			return !(left == right);
		}

		// Token: 0x060037F5 RID: 14325 RVA: 0x00176F43 File Offset: 0x00175143
		[CompilerGenerated]
		public static bool operator ==(ServerListContentPackageInfo left, ServerListContentPackageInfo right)
		{
			return left.Equals(right);
		}

		// Token: 0x060037F6 RID: 14326 RVA: 0x00176F4D File Offset: 0x0017514D
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<string>.Default.GetHashCode(this.<Name>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Hash>k__BackingField)) * -1521134295 + EqualityComparer<Option<ContentPackageId>>.Default.GetHashCode(this.<Id>k__BackingField);
		}

		// Token: 0x060037F7 RID: 14327 RVA: 0x00176F8D File Offset: 0x0017518D
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is ServerListContentPackageInfo && this.Equals((ServerListContentPackageInfo)obj);
		}

		// Token: 0x060037F8 RID: 14328 RVA: 0x00176FA8 File Offset: 0x001751A8
		[CompilerGenerated]
		public bool Equals(ServerListContentPackageInfo other)
		{
			return EqualityComparer<string>.Default.Equals(this.<Name>k__BackingField, other.<Name>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Hash>k__BackingField, other.<Hash>k__BackingField) && EqualityComparer<Option<ContentPackageId>>.Default.Equals(this.<Id>k__BackingField, other.<Id>k__BackingField);
		}

		// Token: 0x060037F9 RID: 14329 RVA: 0x00176FFD File Offset: 0x001751FD
		[CompilerGenerated]
		public void Deconstruct(out string Name, out string Hash, out Option<ContentPackageId> Id)
		{
			Name = this.Name;
			Hash = this.Hash;
			Id = this.Id;
		}
	}
}
