using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200058A RID: 1418
	public interface IDataInfo : IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>
	{
		// Token: 0x1700157D RID: 5501
		// (get) Token: 0x060056D0 RID: 22224
		[XmlAttribute("Name")]
		string InternalName { get; }

		// Token: 0x1700157E RID: 5502
		// (get) Token: 0x060056D1 RID: 22225
		ContentPackage OwnerPackage { get; }

		// Token: 0x060056D2 RID: 22226 RVA: 0x002D2FD8 File Offset: 0x002D11D8
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

		// Token: 0x060056D3 RID: 22227 RVA: 0x002D3114 File Offset: 0x002D1314
		bool Equals(IDataInfo other)
		{
			return this.Equals(this, other);
		}

		// Token: 0x060056D4 RID: 22228 RVA: 0x002D3120 File Offset: 0x002D1320
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
