using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200046C RID: 1132
	public interface IDataInfo : IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>
	{
		// Token: 0x17001017 RID: 4119
		// (get) Token: 0x06003D60 RID: 15712
		[XmlAttribute("Name")]
		string InternalName { get; }

		// Token: 0x17001018 RID: 4120
		// (get) Token: 0x06003D61 RID: 15713
		ContentPackage OwnerPackage { get; }

		// Token: 0x06003D62 RID: 15714 RVA: 0x0018E02C File Offset: 0x0018C22C
		bool Equals(IDataInfo x, IDataInfo y)
		{
			if (x == null || y == null)
			{
				return false;
			}
			if (x.OwnerPackage == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ContentPackage not set for resource ");
				defaultInterpolatedStringHandler.AppendFormatted<IDataInfo>(x);
				defaultInterpolatedStringHandler.AppendLiteral("!");
				throw new NullReferenceException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (y.OwnerPackage == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(37, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("ContentPackage not set for resource ");
				defaultInterpolatedStringHandler2.AppendFormatted<IDataInfo>(y);
				defaultInterpolatedStringHandler2.AppendLiteral("!");
				throw new NullReferenceException(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			if (x.InternalName.IsNullOrWhiteSpace())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(35, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("InternalName not set for resource ");
				defaultInterpolatedStringHandler3.AppendFormatted<IDataInfo>(x);
				defaultInterpolatedStringHandler3.AppendLiteral("!");
				throw new NullReferenceException(defaultInterpolatedStringHandler3.ToStringAndClear());
			}
			if (y.InternalName.IsNullOrWhiteSpace())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(35, 1);
				defaultInterpolatedStringHandler4.AppendLiteral("InternalName not set for resource ");
				defaultInterpolatedStringHandler4.AppendFormatted<IDataInfo>(y);
				defaultInterpolatedStringHandler4.AppendLiteral("!");
				throw new NullReferenceException(defaultInterpolatedStringHandler4.ToStringAndClear());
			}
			return x.OwnerPackage == y.OwnerPackage && x.InternalName == y.InternalName;
		}

		// Token: 0x06003D63 RID: 15715 RVA: 0x0018E168 File Offset: 0x0018C368
		bool Equals(IDataInfo other)
		{
			return this.Equals(this, other);
		}

		// Token: 0x06003D64 RID: 15716 RVA: 0x0018E174 File Offset: 0x0018C374
		int GetHashCode(IDataInfo obj)
		{
			if (obj.OwnerPackage == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ContentPackage not set for resource ");
				defaultInterpolatedStringHandler.AppendFormatted<IDataInfo>(obj);
				defaultInterpolatedStringHandler.AppendLiteral("!");
				throw new NullReferenceException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (obj.InternalName.IsNullOrWhiteSpace())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(33, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("InternalName is null for object ");
				defaultInterpolatedStringHandler2.AppendFormatted<IDataInfo>(obj);
				defaultInterpolatedStringHandler2.AppendLiteral("!");
				throw new NullReferenceException(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			return obj.InternalName.GetHashCode() + obj.OwnerPackage.GetHashCode();
		}
	}
}
