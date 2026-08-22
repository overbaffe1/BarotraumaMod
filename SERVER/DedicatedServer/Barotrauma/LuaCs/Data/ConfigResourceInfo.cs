using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000461 RID: 1121
	public class ConfigResourceInfo : BaseResourceInfo, IConfigResourceInfo, IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<ConfigResourceInfo>
	{
		// Token: 0x17000FFC RID: 4092
		// (get) Token: 0x06003D10 RID: 15632 RVA: 0x0018D7CD File Offset: 0x0018B9CD
		[Nullable(1)]
		[CompilerGenerated]
		protected override Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(ConfigResourceInfo);
			}
		}

		// Token: 0x06003D11 RID: 15633 RVA: 0x0018D7DC File Offset: 0x0018B9DC
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ConfigResourceInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003D12 RID: 15634 RVA: 0x0018D828 File Offset: 0x0018BA28
		[NullableContext(1)]
		[CompilerGenerated]
		protected override bool PrintMembers(StringBuilder builder)
		{
			return base.PrintMembers(builder);
		}

		// Token: 0x06003D13 RID: 15635 RVA: 0x0018D831 File Offset: 0x0018BA31
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ConfigResourceInfo left, ConfigResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003D14 RID: 15636 RVA: 0x0018D83D File Offset: 0x0018BA3D
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ConfigResourceInfo left, ConfigResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003D15 RID: 15637 RVA: 0x0018D851 File Offset: 0x0018BA51
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		// Token: 0x06003D16 RID: 15638 RVA: 0x0018D859 File Offset: 0x0018BA59
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ConfigResourceInfo);
		}

		// Token: 0x06003D17 RID: 15639 RVA: 0x0018D867 File Offset: 0x0018BA67
		[NullableContext(2)]
		[CompilerGenerated]
		public sealed override bool Equals(BaseResourceInfo other)
		{
			return this.Equals(other);
		}

		// Token: 0x06003D18 RID: 15640 RVA: 0x0018D870 File Offset: 0x0018BA70
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ConfigResourceInfo other)
		{
			return this == other || base.Equals(other);
		}

		// Token: 0x06003D1A RID: 15642 RVA: 0x0018D887 File Offset: 0x0018BA87
		[CompilerGenerated]
		protected ConfigResourceInfo([Nullable(1)] ConfigResourceInfo original) : base(original)
		{
		}

		// Token: 0x06003D1B RID: 15643 RVA: 0x0018D890 File Offset: 0x0018BA90
		public ConfigResourceInfo()
		{
		}
	}
}
