using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000460 RID: 1120
	public class AssemblyResourceInfo : BaseResourceInfo, IAssemblyResourceInfo, IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<AssemblyResourceInfo>
	{
		// Token: 0x17000FF7 RID: 4087
		// (get) Token: 0x06003CFC RID: 15612 RVA: 0x0018D50F File Offset: 0x0018B70F
		[Nullable(1)]
		[CompilerGenerated]
		protected override Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(AssemblyResourceInfo);
			}
		}

		// Token: 0x17000FF8 RID: 4088
		// (get) Token: 0x06003CFD RID: 15613 RVA: 0x0018D51B File Offset: 0x0018B71B
		// (set) Token: 0x06003CFE RID: 15614 RVA: 0x0018D523 File Offset: 0x0018B723
		public string FriendlyName { get; set; }

		// Token: 0x17000FF9 RID: 4089
		// (get) Token: 0x06003CFF RID: 15615 RVA: 0x0018D52C File Offset: 0x0018B72C
		// (set) Token: 0x06003D00 RID: 15616 RVA: 0x0018D534 File Offset: 0x0018B734
		public bool IsScript { get; set; }

		// Token: 0x17000FFA RID: 4090
		// (get) Token: 0x06003D01 RID: 15617 RVA: 0x0018D53D File Offset: 0x0018B73D
		// (set) Token: 0x06003D02 RID: 15618 RVA: 0x0018D545 File Offset: 0x0018B745
		public bool UseInternalAccessName { get; set; }

		// Token: 0x17000FFB RID: 4091
		// (get) Token: 0x06003D03 RID: 15619 RVA: 0x0018D54E File Offset: 0x0018B74E
		// (set) Token: 0x06003D04 RID: 15620 RVA: 0x0018D556 File Offset: 0x0018B756
		public bool IsReferenceModeOnly { get; set; }

		// Token: 0x06003D05 RID: 15621 RVA: 0x0018D560 File Offset: 0x0018B760
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("AssemblyResourceInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003D06 RID: 15622 RVA: 0x0018D5AC File Offset: 0x0018B7AC
		[NullableContext(1)]
		[CompilerGenerated]
		protected override bool PrintMembers(StringBuilder builder)
		{
			if (base.PrintMembers(builder))
			{
				builder.Append(", ");
			}
			builder.Append("FriendlyName = ");
			builder.Append(this.FriendlyName);
			builder.Append(", IsScript = ");
			builder.Append(this.IsScript.ToString());
			builder.Append(", UseInternalAccessName = ");
			builder.Append(this.UseInternalAccessName.ToString());
			builder.Append(", IsReferenceModeOnly = ");
			builder.Append(this.IsReferenceModeOnly.ToString());
			return true;
		}

		// Token: 0x06003D07 RID: 15623 RVA: 0x0018D65D File Offset: 0x0018B85D
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(AssemblyResourceInfo left, AssemblyResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003D08 RID: 15624 RVA: 0x0018D669 File Offset: 0x0018B869
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(AssemblyResourceInfo left, AssemblyResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003D09 RID: 15625 RVA: 0x0018D680 File Offset: 0x0018B880
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((base.GetHashCode() * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<FriendlyName>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsScript>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<UseInternalAccessName>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsReferenceModeOnly>k__BackingField);
		}

		// Token: 0x06003D0A RID: 15626 RVA: 0x0018D6EF File Offset: 0x0018B8EF
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as AssemblyResourceInfo);
		}

		// Token: 0x06003D0B RID: 15627 RVA: 0x0018D6FD File Offset: 0x0018B8FD
		[NullableContext(2)]
		[CompilerGenerated]
		public sealed override bool Equals(BaseResourceInfo other)
		{
			return this.Equals(other);
		}

		// Token: 0x06003D0C RID: 15628 RVA: 0x0018D708 File Offset: 0x0018B908
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(AssemblyResourceInfo other)
		{
			return this == other || (base.Equals(other) && EqualityComparer<string>.Default.Equals(this.<FriendlyName>k__BackingField, other.<FriendlyName>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsScript>k__BackingField, other.<IsScript>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<UseInternalAccessName>k__BackingField, other.<UseInternalAccessName>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsReferenceModeOnly>k__BackingField, other.<IsReferenceModeOnly>k__BackingField));
		}

		// Token: 0x06003D0E RID: 15630 RVA: 0x0018D78C File Offset: 0x0018B98C
		[CompilerGenerated]
		protected AssemblyResourceInfo([Nullable(1)] AssemblyResourceInfo original) : base(original)
		{
			this.FriendlyName = original.<FriendlyName>k__BackingField;
			this.IsScript = original.<IsScript>k__BackingField;
			this.UseInternalAccessName = original.<UseInternalAccessName>k__BackingField;
			this.IsReferenceModeOnly = original.<IsReferenceModeOnly>k__BackingField;
		}

		// Token: 0x06003D0F RID: 15631 RVA: 0x0018D7C5 File Offset: 0x0018B9C5
		public AssemblyResourceInfo()
		{
		}
	}
}
