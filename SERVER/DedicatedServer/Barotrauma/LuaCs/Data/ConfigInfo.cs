using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000463 RID: 1123
	public class ConfigInfo : IConfigInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ConfigInfo>
	{
		// Token: 0x17001000 RID: 4096
		// (get) Token: 0x06003D2C RID: 15660 RVA: 0x0018DA71 File Offset: 0x0018BC71
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(ConfigInfo);
			}
		}

		// Token: 0x17001001 RID: 4097
		// (get) Token: 0x06003D2D RID: 15661 RVA: 0x0018DA7D File Offset: 0x0018BC7D
		// (set) Token: 0x06003D2E RID: 15662 RVA: 0x0018DA85 File Offset: 0x0018BC85
		public string InternalName { get; set; }

		// Token: 0x17001002 RID: 4098
		// (get) Token: 0x06003D2F RID: 15663 RVA: 0x0018DA8E File Offset: 0x0018BC8E
		// (set) Token: 0x06003D30 RID: 15664 RVA: 0x0018DA96 File Offset: 0x0018BC96
		public ContentPackage OwnerPackage { get; set; }

		// Token: 0x17001003 RID: 4099
		// (get) Token: 0x06003D31 RID: 15665 RVA: 0x0018DA9F File Offset: 0x0018BC9F
		// (set) Token: 0x06003D32 RID: 15666 RVA: 0x0018DAA7 File Offset: 0x0018BCA7
		public string DataType { get; set; }

		// Token: 0x17001004 RID: 4100
		// (get) Token: 0x06003D33 RID: 15667 RVA: 0x0018DAB0 File Offset: 0x0018BCB0
		// (set) Token: 0x06003D34 RID: 15668 RVA: 0x0018DAB8 File Offset: 0x0018BCB8
		public XElement Element { get; set; }

		// Token: 0x17001005 RID: 4101
		// (get) Token: 0x06003D35 RID: 15669 RVA: 0x0018DAC1 File Offset: 0x0018BCC1
		// (set) Token: 0x06003D36 RID: 15670 RVA: 0x0018DAC9 File Offset: 0x0018BCC9
		public RunState EditableStates { get; set; }

		// Token: 0x17001006 RID: 4102
		// (get) Token: 0x06003D37 RID: 15671 RVA: 0x0018DAD2 File Offset: 0x0018BCD2
		// (set) Token: 0x06003D38 RID: 15672 RVA: 0x0018DADA File Offset: 0x0018BCDA
		public NetSync NetSync { get; set; }

		// Token: 0x06003D39 RID: 15673 RVA: 0x0018DAE4 File Offset: 0x0018BCE4
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ConfigInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003D3A RID: 15674 RVA: 0x0018DB30 File Offset: 0x0018BD30
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("InternalName = ");
			builder.Append(this.InternalName);
			builder.Append(", OwnerPackage = ");
			builder.Append(this.OwnerPackage);
			builder.Append(", DataType = ");
			builder.Append(this.DataType);
			builder.Append(", Element = ");
			builder.Append(this.Element);
			builder.Append(", EditableStates = ");
			builder.Append(this.EditableStates.ToString());
			builder.Append(", NetSync = ");
			builder.Append(this.NetSync.ToString());
			return true;
		}

		// Token: 0x06003D3B RID: 15675 RVA: 0x0018DBF5 File Offset: 0x0018BDF5
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ConfigInfo left, ConfigInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003D3C RID: 15676 RVA: 0x0018DC01 File Offset: 0x0018BE01
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ConfigInfo left, ConfigInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003D3D RID: 15677 RVA: 0x0018DC18 File Offset: 0x0018BE18
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<InternalName>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<OwnerPackage>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<DataType>k__BackingField)) * -1521134295 + EqualityComparer<XElement>.Default.GetHashCode(this.<Element>k__BackingField)) * -1521134295 + EqualityComparer<RunState>.Default.GetHashCode(this.<EditableStates>k__BackingField)) * -1521134295 + EqualityComparer<NetSync>.Default.GetHashCode(this.<NetSync>k__BackingField);
		}

		// Token: 0x06003D3E RID: 15678 RVA: 0x0018DCBF File Offset: 0x0018BEBF
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ConfigInfo);
		}

		// Token: 0x06003D3F RID: 15679 RVA: 0x0018DCD0 File Offset: 0x0018BED0
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ConfigInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.<InternalName>k__BackingField, other.<InternalName>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<OwnerPackage>k__BackingField, other.<OwnerPackage>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<DataType>k__BackingField, other.<DataType>k__BackingField) && EqualityComparer<XElement>.Default.Equals(this.<Element>k__BackingField, other.<Element>k__BackingField) && EqualityComparer<RunState>.Default.Equals(this.<EditableStates>k__BackingField, other.<EditableStates>k__BackingField) && EqualityComparer<NetSync>.Default.Equals(this.<NetSync>k__BackingField, other.<NetSync>k__BackingField));
		}

		// Token: 0x06003D41 RID: 15681 RVA: 0x0018DD9C File Offset: 0x0018BF9C
		[CompilerGenerated]
		protected ConfigInfo([Nullable(1)] ConfigInfo original)
		{
			this.InternalName = original.<InternalName>k__BackingField;
			this.OwnerPackage = original.<OwnerPackage>k__BackingField;
			this.DataType = original.<DataType>k__BackingField;
			this.Element = original.<Element>k__BackingField;
			this.EditableStates = original.<EditableStates>k__BackingField;
			this.NetSync = original.<NetSync>k__BackingField;
		}

		// Token: 0x06003D42 RID: 15682 RVA: 0x0018DDF7 File Offset: 0x0018BFF7
		public ConfigInfo()
		{
		}
	}
}
