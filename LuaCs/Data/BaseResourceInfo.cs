using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200057E RID: 1406
	public class BaseResourceInfo : IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<BaseResourceInfo>
	{
		// Token: 0x17001551 RID: 5457
		// (get) Token: 0x06005647 RID: 22087 RVA: 0x002D1D33 File Offset: 0x002CFF33
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

		// Token: 0x17001552 RID: 5458
		// (get) Token: 0x06005648 RID: 22088 RVA: 0x002D1D3F File Offset: 0x002CFF3F
		// (set) Token: 0x06005649 RID: 22089 RVA: 0x002D1D47 File Offset: 0x002CFF47
		public Platform SupportedPlatforms { get; set; }

		// Token: 0x17001553 RID: 5459
		// (get) Token: 0x0600564A RID: 22090 RVA: 0x002D1D50 File Offset: 0x002CFF50
		// (set) Token: 0x0600564B RID: 22091 RVA: 0x002D1D58 File Offset: 0x002CFF58
		public Target SupportedTargets { get; set; }

		// Token: 0x17001554 RID: 5460
		// (get) Token: 0x0600564C RID: 22092 RVA: 0x002D1D61 File Offset: 0x002CFF61
		// (set) Token: 0x0600564D RID: 22093 RVA: 0x002D1D69 File Offset: 0x002CFF69
		public int LoadPriority { get; set; }

		// Token: 0x17001555 RID: 5461
		// (get) Token: 0x0600564E RID: 22094 RVA: 0x002D1D72 File Offset: 0x002CFF72
		// (set) Token: 0x0600564F RID: 22095 RVA: 0x002D1D7A File Offset: 0x002CFF7A
		public ImmutableArray<ContentPath> FilePaths { get; set; }

		// Token: 0x17001556 RID: 5462
		// (get) Token: 0x06005650 RID: 22096 RVA: 0x002D1D83 File Offset: 0x002CFF83
		// (set) Token: 0x06005651 RID: 22097 RVA: 0x002D1D8B File Offset: 0x002CFF8B
		public bool Optional { get; set; }

		// Token: 0x17001557 RID: 5463
		// (get) Token: 0x06005652 RID: 22098 RVA: 0x002D1D94 File Offset: 0x002CFF94
		// (set) Token: 0x06005653 RID: 22099 RVA: 0x002D1D9C File Offset: 0x002CFF9C
		public string InternalName { get; set; }

		// Token: 0x17001558 RID: 5464
		// (get) Token: 0x06005654 RID: 22100 RVA: 0x002D1DA5 File Offset: 0x002CFFA5
		// (set) Token: 0x06005655 RID: 22101 RVA: 0x002D1DAD File Offset: 0x002CFFAD
		public ContentPackage OwnerPackage { get; set; }

		// Token: 0x17001559 RID: 5465
		// (get) Token: 0x06005656 RID: 22102 RVA: 0x002D1DB6 File Offset: 0x002CFFB6
		// (set) Token: 0x06005657 RID: 22103 RVA: 0x002D1DBE File Offset: 0x002CFFBE
		public ImmutableArray<Identifier> RequiredPackages { get; set; }

		// Token: 0x1700155A RID: 5466
		// (get) Token: 0x06005658 RID: 22104 RVA: 0x002D1DC7 File Offset: 0x002CFFC7
		// (set) Token: 0x06005659 RID: 22105 RVA: 0x002D1DCF File Offset: 0x002CFFCF
		public ImmutableArray<Identifier> IncompatiblePackages { get; set; }

		// Token: 0x0600565A RID: 22106 RVA: 0x002D1DD8 File Offset: 0x002CFFD8
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

		// Token: 0x0600565B RID: 22107 RVA: 0x002D1E24 File Offset: 0x002D0024
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

		// Token: 0x0600565C RID: 22108 RVA: 0x002D1F7D File Offset: 0x002D017D
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(BaseResourceInfo left, BaseResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x0600565D RID: 22109 RVA: 0x002D1F89 File Offset: 0x002D0189
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(BaseResourceInfo left, BaseResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x0600565E RID: 22110 RVA: 0x002D1FA0 File Offset: 0x002D01A0
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((((((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<Platform>.Default.GetHashCode(this.<SupportedPlatforms>k__BackingField)) * -1521134295 + EqualityComparer<Target>.Default.GetHashCode(this.<SupportedTargets>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<LoadPriority>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<ContentPath>>.Default.GetHashCode(this.<FilePaths>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Optional>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<InternalName>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<OwnerPackage>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<RequiredPackages>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<IncompatiblePackages>k__BackingField);
		}

		// Token: 0x0600565F RID: 22111 RVA: 0x002D208C File Offset: 0x002D028C
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as BaseResourceInfo);
		}

		// Token: 0x06005660 RID: 22112 RVA: 0x002D209C File Offset: 0x002D029C
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(BaseResourceInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<Platform>.Default.Equals(this.<SupportedPlatforms>k__BackingField, other.<SupportedPlatforms>k__BackingField) && EqualityComparer<Target>.Default.Equals(this.<SupportedTargets>k__BackingField, other.<SupportedTargets>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<LoadPriority>k__BackingField, other.<LoadPriority>k__BackingField) && EqualityComparer<ImmutableArray<ContentPath>>.Default.Equals(this.<FilePaths>k__BackingField, other.<FilePaths>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Optional>k__BackingField, other.<Optional>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<InternalName>k__BackingField, other.<InternalName>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<OwnerPackage>k__BackingField, other.<OwnerPackage>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<RequiredPackages>k__BackingField, other.<RequiredPackages>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<IncompatiblePackages>k__BackingField, other.<IncompatiblePackages>k__BackingField));
		}

		// Token: 0x06005662 RID: 22114 RVA: 0x002D21B8 File Offset: 0x002D03B8
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

		// Token: 0x06005663 RID: 22115 RVA: 0x002D2237 File Offset: 0x002D0437
		public BaseResourceInfo()
		{
		}
	}
}
