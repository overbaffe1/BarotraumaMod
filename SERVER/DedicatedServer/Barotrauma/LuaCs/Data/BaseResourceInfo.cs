using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200045F RID: 1119
	public class BaseResourceInfo : IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<BaseResourceInfo>
	{
		// Token: 0x17000FED RID: 4077
		// (get) Token: 0x06003CDF RID: 15583 RVA: 0x0018D001 File Offset: 0x0018B201
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(BaseResourceInfo);
			}
		}

		// Token: 0x17000FEE RID: 4078
		// (get) Token: 0x06003CE0 RID: 15584 RVA: 0x0018D00D File Offset: 0x0018B20D
		// (set) Token: 0x06003CE1 RID: 15585 RVA: 0x0018D015 File Offset: 0x0018B215
		public Platform SupportedPlatforms { get; set; }

		// Token: 0x17000FEF RID: 4079
		// (get) Token: 0x06003CE2 RID: 15586 RVA: 0x0018D01E File Offset: 0x0018B21E
		// (set) Token: 0x06003CE3 RID: 15587 RVA: 0x0018D026 File Offset: 0x0018B226
		public Target SupportedTargets { get; set; }

		// Token: 0x17000FF0 RID: 4080
		// (get) Token: 0x06003CE4 RID: 15588 RVA: 0x0018D02F File Offset: 0x0018B22F
		// (set) Token: 0x06003CE5 RID: 15589 RVA: 0x0018D037 File Offset: 0x0018B237
		public int LoadPriority { get; set; }

		// Token: 0x17000FF1 RID: 4081
		// (get) Token: 0x06003CE6 RID: 15590 RVA: 0x0018D040 File Offset: 0x0018B240
		// (set) Token: 0x06003CE7 RID: 15591 RVA: 0x0018D048 File Offset: 0x0018B248
		public ImmutableArray<ContentPath> FilePaths { get; set; }

		// Token: 0x17000FF2 RID: 4082
		// (get) Token: 0x06003CE8 RID: 15592 RVA: 0x0018D051 File Offset: 0x0018B251
		// (set) Token: 0x06003CE9 RID: 15593 RVA: 0x0018D059 File Offset: 0x0018B259
		public bool Optional { get; set; }

		// Token: 0x17000FF3 RID: 4083
		// (get) Token: 0x06003CEA RID: 15594 RVA: 0x0018D062 File Offset: 0x0018B262
		// (set) Token: 0x06003CEB RID: 15595 RVA: 0x0018D06A File Offset: 0x0018B26A
		public string InternalName { get; set; }

		// Token: 0x17000FF4 RID: 4084
		// (get) Token: 0x06003CEC RID: 15596 RVA: 0x0018D073 File Offset: 0x0018B273
		// (set) Token: 0x06003CED RID: 15597 RVA: 0x0018D07B File Offset: 0x0018B27B
		public ContentPackage OwnerPackage { get; set; }

		// Token: 0x17000FF5 RID: 4085
		// (get) Token: 0x06003CEE RID: 15598 RVA: 0x0018D084 File Offset: 0x0018B284
		// (set) Token: 0x06003CEF RID: 15599 RVA: 0x0018D08C File Offset: 0x0018B28C
		public ImmutableArray<Identifier> RequiredPackages { get; set; }

		// Token: 0x17000FF6 RID: 4086
		// (get) Token: 0x06003CF0 RID: 15600 RVA: 0x0018D095 File Offset: 0x0018B295
		// (set) Token: 0x06003CF1 RID: 15601 RVA: 0x0018D09D File Offset: 0x0018B29D
		public ImmutableArray<Identifier> IncompatiblePackages { get; set; }

		// Token: 0x06003CF2 RID: 15602 RVA: 0x0018D0A8 File Offset: 0x0018B2A8
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("BaseResourceInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003CF3 RID: 15603 RVA: 0x0018D0F4 File Offset: 0x0018B2F4
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("SupportedPlatforms = ");
			builder.Append(this.SupportedPlatforms.ToString());
			builder.Append(", SupportedTargets = ");
			builder.Append(this.SupportedTargets.ToString());
			builder.Append(", LoadPriority = ");
			builder.Append(this.LoadPriority.ToString());
			builder.Append(", FilePaths = ");
			builder.Append(this.FilePaths.ToString());
			builder.Append(", Optional = ");
			builder.Append(this.Optional.ToString());
			builder.Append(", InternalName = ");
			builder.Append(this.InternalName);
			builder.Append(", OwnerPackage = ");
			builder.Append(this.OwnerPackage);
			builder.Append(", RequiredPackages = ");
			builder.Append(this.RequiredPackages.ToString());
			builder.Append(", IncompatiblePackages = ");
			builder.Append(this.IncompatiblePackages.ToString());
			return true;
		}

		// Token: 0x06003CF4 RID: 15604 RVA: 0x0018D24D File Offset: 0x0018B44D
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(BaseResourceInfo left, BaseResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003CF5 RID: 15605 RVA: 0x0018D259 File Offset: 0x0018B459
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(BaseResourceInfo left, BaseResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003CF6 RID: 15606 RVA: 0x0018D270 File Offset: 0x0018B470
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((((((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<Platform>.Default.GetHashCode(this.<SupportedPlatforms>k__BackingField)) * -1521134295 + EqualityComparer<Target>.Default.GetHashCode(this.<SupportedTargets>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<LoadPriority>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<ContentPath>>.Default.GetHashCode(this.<FilePaths>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Optional>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<InternalName>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<OwnerPackage>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<RequiredPackages>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<IncompatiblePackages>k__BackingField);
		}

		// Token: 0x06003CF7 RID: 15607 RVA: 0x0018D35C File Offset: 0x0018B55C
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as BaseResourceInfo);
		}

		// Token: 0x06003CF8 RID: 15608 RVA: 0x0018D36C File Offset: 0x0018B56C
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(BaseResourceInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<Platform>.Default.Equals(this.<SupportedPlatforms>k__BackingField, other.<SupportedPlatforms>k__BackingField) && EqualityComparer<Target>.Default.Equals(this.<SupportedTargets>k__BackingField, other.<SupportedTargets>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<LoadPriority>k__BackingField, other.<LoadPriority>k__BackingField) && EqualityComparer<ImmutableArray<ContentPath>>.Default.Equals(this.<FilePaths>k__BackingField, other.<FilePaths>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Optional>k__BackingField, other.<Optional>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<InternalName>k__BackingField, other.<InternalName>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<OwnerPackage>k__BackingField, other.<OwnerPackage>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<RequiredPackages>k__BackingField, other.<RequiredPackages>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<IncompatiblePackages>k__BackingField, other.<IncompatiblePackages>k__BackingField));
		}

		// Token: 0x06003CFA RID: 15610 RVA: 0x0018D488 File Offset: 0x0018B688
		[CompilerGenerated]
		protected BaseResourceInfo([Nullable(1)] BaseResourceInfo original)
		{
			this.SupportedPlatforms = original.<SupportedPlatforms>k__BackingField;
			this.SupportedTargets = original.<SupportedTargets>k__BackingField;
			this.LoadPriority = original.<LoadPriority>k__BackingField;
			this.FilePaths = original.<FilePaths>k__BackingField;
			this.Optional = original.<Optional>k__BackingField;
			this.InternalName = original.<InternalName>k__BackingField;
			this.OwnerPackage = original.<OwnerPackage>k__BackingField;
			this.RequiredPackages = original.<RequiredPackages>k__BackingField;
			this.IncompatiblePackages = original.<IncompatiblePackages>k__BackingField;
		}

		// Token: 0x06003CFB RID: 15611 RVA: 0x0018D507 File Offset: 0x0018B707
		public BaseResourceInfo()
		{
		}
	}
}
