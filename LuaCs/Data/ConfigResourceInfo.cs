using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000580 RID: 1408
	public class ConfigResourceInfo : BaseResourceInfo, IConfigResourceInfo, IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<ConfigResourceInfo>
	{
		// Token: 0x17001560 RID: 5472
		// (get) Token: 0x06005678 RID: 22136 RVA: 0x002D24FD File Offset: 0x002D06FD
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

		// Token: 0x06005679 RID: 22137 RVA: 0x002D250C File Offset: 0x002D070C
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

		// Token: 0x0600567A RID: 22138 RVA: 0x002D2558 File Offset: 0x002D0758
		[NullableContext(1)]
		[CompilerGenerated]
		protected override bool PrintMembers(StringBuilder builder)
		{
			return base.PrintMembers(builder);
		}

		// Token: 0x0600567B RID: 22139 RVA: 0x002D2561 File Offset: 0x002D0761
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ConfigResourceInfo left, ConfigResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x0600567C RID: 22140 RVA: 0x002D256D File Offset: 0x002D076D
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ConfigResourceInfo left, ConfigResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x0600567D RID: 22141 RVA: 0x002D2581 File Offset: 0x002D0781
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		// Token: 0x0600567E RID: 22142 RVA: 0x002D2589 File Offset: 0x002D0789
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ConfigResourceInfo);
		}

		// Token: 0x0600567F RID: 22143 RVA: 0x002D2597 File Offset: 0x002D0797
		[NullableContext(2)]
		[CompilerGenerated]
		public sealed override bool Equals(BaseResourceInfo other)
		{
			return this.Equals(other);
		}

		// Token: 0x06005680 RID: 22144 RVA: 0x002D25A0 File Offset: 0x002D07A0
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ConfigResourceInfo other)
		{
			return this == other || base.Equals(other);
		}

		// Token: 0x06005682 RID: 22146 RVA: 0x002D25B7 File Offset: 0x002D07B7
		[CompilerGenerated]
		protected ConfigResourceInfo([Nullable(1)] ConfigResourceInfo original) : base(original)
		{
		}

		// Token: 0x06005683 RID: 22147 RVA: 0x002D25C0 File Offset: 0x002D07C0
		public ConfigResourceInfo()
		{
		}
	}
}
