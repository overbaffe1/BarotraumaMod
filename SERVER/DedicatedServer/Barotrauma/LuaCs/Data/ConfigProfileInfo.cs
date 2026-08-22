using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000464 RID: 1124
	public class ConfigProfileInfo : IConfigProfileInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ConfigProfileInfo>
	{
		// Token: 0x17001007 RID: 4103
		// (get) Token: 0x06003D43 RID: 15683 RVA: 0x0018DDFF File Offset: 0x0018BFFF
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

		// Token: 0x17001008 RID: 4104
		// (get) Token: 0x06003D44 RID: 15684 RVA: 0x0018DE0B File Offset: 0x0018C00B
		// (set) Token: 0x06003D45 RID: 15685 RVA: 0x0018DE13 File Offset: 0x0018C013
		public string InternalName { get; set; }

		// Token: 0x17001009 RID: 4105
		// (get) Token: 0x06003D46 RID: 15686 RVA: 0x0018DE1C File Offset: 0x0018C01C
		// (set) Token: 0x06003D47 RID: 15687 RVA: 0x0018DE24 File Offset: 0x0018C024
		public ContentPackage OwnerPackage { get; set; }

		// Token: 0x1700100A RID: 4106
		// (get) Token: 0x06003D48 RID: 15688 RVA: 0x0018DE2D File Offset: 0x0018C02D
		// (set) Token: 0x06003D49 RID: 15689 RVA: 0x0018DE35 File Offset: 0x0018C035
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

		// Token: 0x06003D4A RID: 15690 RVA: 0x0018DE40 File Offset: 0x0018C040
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

		// Token: 0x06003D4B RID: 15691 RVA: 0x0018DE8C File Offset: 0x0018C08C
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

		// Token: 0x06003D4C RID: 15692 RVA: 0x0018DEEA File Offset: 0x0018C0EA
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ConfigProfileInfo left, ConfigProfileInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003D4D RID: 15693 RVA: 0x0018DEF6 File Offset: 0x0018C0F6
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ConfigProfileInfo left, ConfigProfileInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003D4E RID: 15694 RVA: 0x0018DF0C File Offset: 0x0018C10C
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<InternalName>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<OwnerPackage>k__BackingField)) * -1521134295 + EqualityComparer<IReadOnlyList<ValueTuple<string, XElement>>>.Default.GetHashCode(this.<ProfileValues>k__BackingField);
		}

		// Token: 0x06003D4F RID: 15695 RVA: 0x0018DF6E File Offset: 0x0018C16E
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ConfigProfileInfo);
		}

		// Token: 0x06003D50 RID: 15696 RVA: 0x0018DF7C File Offset: 0x0018C17C
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ConfigProfileInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.<InternalName>k__BackingField, other.<InternalName>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<OwnerPackage>k__BackingField, other.<OwnerPackage>k__BackingField) && EqualityComparer<IReadOnlyList<ValueTuple<string, XElement>>>.Default.Equals(this.<ProfileValues>k__BackingField, other.<ProfileValues>k__BackingField));
		}

		// Token: 0x06003D52 RID: 15698 RVA: 0x0018DFF5 File Offset: 0x0018C1F5
		[CompilerGenerated]
		protected ConfigProfileInfo([Nullable(1)] ConfigProfileInfo original)
		{
			this.InternalName = original.<InternalName>k__BackingField;
			this.OwnerPackage = original.<OwnerPackage>k__BackingField;
			this.ProfileValues = original.<ProfileValues>k__BackingField;
		}

		// Token: 0x06003D53 RID: 15699 RVA: 0x0018E021 File Offset: 0x0018C221
		public ConfigProfileInfo()
		{
		}
	}
}
