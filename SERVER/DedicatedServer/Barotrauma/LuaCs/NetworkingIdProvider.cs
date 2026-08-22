using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Data;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003E3 RID: 995
	internal class NetworkingIdProvider : INetworkIdProvider, IService, IDisposable
	{
		// Token: 0x06003921 RID: 14625 RVA: 0x0017D04B File Offset: 0x0017B24B
		public void Dispose()
		{
		}

		// Token: 0x17000F98 RID: 3992
		// (get) Token: 0x06003922 RID: 14626 RVA: 0x0017D04D File Offset: 0x0017B24D
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003923 RID: 14627 RVA: 0x0017D050 File Offset: 0x0017B250
		private Guid GetNetworkIdFromStringMd5(string id)
		{
			return new Guid(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(id)));
		}

		// Token: 0x06003924 RID: 14628 RVA: 0x0017D06C File Offset: 0x0017B26C
		public Guid GetNetworkIdForInstance(IDataInfo instance)
		{
			string str = instance.OwnerPackage.Name + "." + instance.InternalName;
			return this.GetNetworkIdFromStringMd5(str);
		}

		// Token: 0x06003925 RID: 14629 RVA: 0x0017D09C File Offset: 0x0017B29C
		public Guid GetNetworkIdForInstance<TEntity>(IDataInfo instance, TEntity attachedEntity) where TEntity : Entity
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 4);
			defaultInterpolatedStringHandler.AppendFormatted("TEntity");
			defaultInterpolatedStringHandler.AppendLiteral("(");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(attachedEntity.ID);
			defaultInterpolatedStringHandler.AppendLiteral(").");
			defaultInterpolatedStringHandler.AppendFormatted(instance.OwnerPackage.Name);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			defaultInterpolatedStringHandler.AppendFormatted(instance.InternalName);
			string str = defaultInterpolatedStringHandler.ToStringAndClear();
			return this.GetNetworkIdFromStringMd5(str);
		}

		// Token: 0x06003926 RID: 14630 RVA: 0x0017D124 File Offset: 0x0017B324
		public Guid GetNetworkIdForInstance(IDataInfo instance, ItemComponent attachedItemComponent)
		{
			Item attachedEntity = attachedItemComponent.Item;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 5);
			defaultInterpolatedStringHandler.AppendFormatted(attachedEntity.GetType().Name);
			defaultInterpolatedStringHandler.AppendLiteral("(");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(attachedEntity.ID);
			defaultInterpolatedStringHandler.AppendLiteral(").ComponentId(");
			defaultInterpolatedStringHandler.AppendFormatted<int>(attachedEntity.Components.IndexOf(attachedItemComponent));
			defaultInterpolatedStringHandler.AppendLiteral(").");
			defaultInterpolatedStringHandler.AppendFormatted(instance.OwnerPackage.Name);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			defaultInterpolatedStringHandler.AppendFormatted(instance.InternalName);
			string str = defaultInterpolatedStringHandler.ToStringAndClear();
			return this.GetNetworkIdFromStringMd5(str);
		}
	}
}
