using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020004C2 RID: 1218
	public readonly struct ServerListContentPackageInfo : IEquatable<ServerListContentPackageInfo>
	{
		// Token: 0x06004FBE RID: 20414 RVA: 0x002AF8E3 File Offset: 0x002ADAE3
		public ServerListContentPackageInfo(string Name, string Hash, Option<ContentPackageId> Id)
		{
			this.Name = Name;
			this.Hash = Hash;
			this.Id = Id;
		}

		// Token: 0x17001452 RID: 5202
		// (get) Token: 0x06004FBF RID: 20415 RVA: 0x002AF8FA File Offset: 0x002ADAFA
		// (set) Token: 0x06004FC0 RID: 20416 RVA: 0x002AF902 File Offset: 0x002ADB02
		public string Name { get; set; }

		// Token: 0x17001453 RID: 5203
		// (get) Token: 0x06004FC1 RID: 20417 RVA: 0x002AF90B File Offset: 0x002ADB0B
		// (set) Token: 0x06004FC2 RID: 20418 RVA: 0x002AF913 File Offset: 0x002ADB13
		public string Hash { get; set; }

		// Token: 0x17001454 RID: 5204
		// (get) Token: 0x06004FC3 RID: 20419 RVA: 0x002AF91C File Offset: 0x002ADB1C
		// (set) Token: 0x06004FC4 RID: 20420 RVA: 0x002AF924 File Offset: 0x002ADB24
		public Option<ContentPackageId> Id { get; set; }

		// Token: 0x06004FC5 RID: 20421 RVA: 0x002AF92D File Offset: 0x002ADB2D
		public ServerListContentPackageInfo(ContentPackage pkg)
		{
			this = new ServerListContentPackageInfo(pkg.Name, pkg.Hash.StringRepresentation, pkg.UgcId);
		}

		// Token: 0x06004FC6 RID: 20422 RVA: 0x002AF94C File Offset: 0x002ADB4C
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

		// Token: 0x06004FC7 RID: 20423 RVA: 0x002AF9A0 File Offset: 0x002ADBA0
		public override string ToString()
		{
			string[] array = new string[3];
			array[0] = this.Name;
			array[1] = this.Hash;
			array[2] = (from id in this.Id
			select id.StringRepresentation).Fallback("");
			return array.JoinEscaped(',');
		}

		// Token: 0x06004FC8 RID: 20424 RVA: 0x002AFA0C File Offset: 0x002ADC0C
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

		// Token: 0x06004FC9 RID: 20425 RVA: 0x002AFA73 File Offset: 0x002ADC73
		[CompilerGenerated]
		public static bool operator !=(ServerListContentPackageInfo left, ServerListContentPackageInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06004FCA RID: 20426 RVA: 0x002AFA7F File Offset: 0x002ADC7F
		[CompilerGenerated]
		public static bool operator ==(ServerListContentPackageInfo left, ServerListContentPackageInfo right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004FCB RID: 20427 RVA: 0x002AFA89 File Offset: 0x002ADC89
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<string>.Default.GetHashCode(this.<Name>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Hash>k__BackingField)) * -1521134295 + EqualityComparer<Option<ContentPackageId>>.Default.GetHashCode(this.<Id>k__BackingField);
		}

		// Token: 0x06004FCC RID: 20428 RVA: 0x002AFAC9 File Offset: 0x002ADCC9
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is ServerListContentPackageInfo && this.Equals((ServerListContentPackageInfo)obj);
		}

		// Token: 0x06004FCD RID: 20429 RVA: 0x002AFAE4 File Offset: 0x002ADCE4
		[CompilerGenerated]
		public bool Equals(ServerListContentPackageInfo other)
		{
			return EqualityComparer<string>.Default.Equals(this.<Name>k__BackingField, other.<Name>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Hash>k__BackingField, other.<Hash>k__BackingField) && EqualityComparer<Option<ContentPackageId>>.Default.Equals(this.<Id>k__BackingField, other.<Id>k__BackingField);
		}

		// Token: 0x06004FCE RID: 20430 RVA: 0x002AFB39 File Offset: 0x002ADD39
		[CompilerGenerated]
		public void Deconstruct(out string Name, out string Hash, out Option<ContentPackageId> Id)
		{
			Name = this.Name;
			Hash = this.Hash;
			Id = this.Id;
		}
	}
}
