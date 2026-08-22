using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000583 RID: 1411
	public class ConfigProfileInfo : IConfigProfileInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ConfigProfileInfo>
	{
		// Token: 0x17001571 RID: 5489
		// (get) Token: 0x060056B7 RID: 22199 RVA: 0x002D2DAB File Offset: 0x002D0FAB
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(ConfigProfileInfo);
			}
		}

		// Token: 0x17001572 RID: 5490
		// (get) Token: 0x060056B8 RID: 22200 RVA: 0x002D2DB7 File Offset: 0x002D0FB7
		// (set) Token: 0x060056B9 RID: 22201 RVA: 0x002D2DBF File Offset: 0x002D0FBF
		public string InternalName { get; set; }

		// Token: 0x17001573 RID: 5491
		// (get) Token: 0x060056BA RID: 22202 RVA: 0x002D2DC8 File Offset: 0x002D0FC8
		// (set) Token: 0x060056BB RID: 22203 RVA: 0x002D2DD0 File Offset: 0x002D0FD0
		public ContentPackage OwnerPackage { get; set; }

		// Token: 0x17001574 RID: 5492
		// (get) Token: 0x060056BC RID: 22204 RVA: 0x002D2DD9 File Offset: 0x002D0FD9
		// (set) Token: 0x060056BD RID: 22205 RVA: 0x002D2DE1 File Offset: 0x002D0FE1
		[TupleElementNames(new string[]
		{
			"SettingName",
			"Element"
		})]
		public IReadOnlyList<ValueTuple<string, XElement>> ProfileValues { [return: TupleElementNames(new string[]
		{
			"SettingName",
			"Element"
		})] get; [param: TupleElementNames(new string[]
		{
			"SettingName",
			"Element"
		})] set; }

		// Token: 0x060056BE RID: 22206 RVA: 0x002D2DEC File Offset: 0x002D0FEC
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ConfigProfileInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060056BF RID: 22207 RVA: 0x002D2E38 File Offset: 0x002D1038
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("InternalName = ");
			builder.Append(this.InternalName);
			builder.Append(", OwnerPackage = ");
			builder.Append(this.OwnerPackage);
			builder.Append(", ProfileValues = ");
			builder.Append(this.ProfileValues);
			return true;
		}

		// Token: 0x060056C0 RID: 22208 RVA: 0x002D2E96 File Offset: 0x002D1096
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ConfigProfileInfo left, ConfigProfileInfo right)
		{
			return !(left == right);
		}

		// Token: 0x060056C1 RID: 22209 RVA: 0x002D2EA2 File Offset: 0x002D10A2
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ConfigProfileInfo left, ConfigProfileInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x060056C2 RID: 22210 RVA: 0x002D2EB8 File Offset: 0x002D10B8
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<InternalName>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<OwnerPackage>k__BackingField)) * -1521134295 + EqualityComparer<IReadOnlyList<ValueTuple<string, XElement>>>.Default.GetHashCode(this.<ProfileValues>k__BackingField);
		}

		// Token: 0x060056C3 RID: 22211 RVA: 0x002D2F1A File Offset: 0x002D111A
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ConfigProfileInfo);
		}

		// Token: 0x060056C4 RID: 22212 RVA: 0x002D2F28 File Offset: 0x002D1128
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ConfigProfileInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.<InternalName>k__BackingField, other.<InternalName>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<OwnerPackage>k__BackingField, other.<OwnerPackage>k__BackingField) && EqualityComparer<IReadOnlyList<ValueTuple<string, XElement>>>.Default.Equals(this.<ProfileValues>k__BackingField, other.<ProfileValues>k__BackingField));
		}

		// Token: 0x060056C6 RID: 22214 RVA: 0x002D2FA1 File Offset: 0x002D11A1
		[CompilerGenerated]
		protected ConfigProfileInfo([Nullable(1)] ConfigProfileInfo original)
		{
			this.InternalName = original.<InternalName>k__BackingField;
			this.OwnerPackage = original.<OwnerPackage>k__BackingField;
			this.ProfileValues = original.<ProfileValues>k__BackingField;
		}

		// Token: 0x060056C7 RID: 22215 RVA: 0x002D2FCD File Offset: 0x002D11CD
		public ConfigProfileInfo()
		{
		}
	}
}
