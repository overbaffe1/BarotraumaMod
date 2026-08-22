using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Data;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004FA RID: 1274
	internal class NetworkingIdProvider : INetworkIdProvider, IService, IDisposable
	{
		// Token: 0x06005276 RID: 21110 RVA: 0x002C338B File Offset: 0x002C158B
		public void Dispose()
		{
		}

		// Token: 0x170014E2 RID: 5346
		// (get) Token: 0x06005277 RID: 21111 RVA: 0x002C338D File Offset: 0x002C158D
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06005278 RID: 21112 RVA: 0x002C3390 File Offset: 0x002C1590
		private Guid GetNetworkIdFromStringMd5(string id)
		{
			return new Guid(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(id)));
		}

		// Token: 0x06005279 RID: 21113 RVA: 0x002C33AC File Offset: 0x002C15AC
		public Guid GetNetworkIdForInstance(IDataInfo instance)
		{
			string str = instance.OwnerPackage.Name + "." + instance.InternalName;
			return this.GetNetworkIdFromStringMd5(str);
		}

		// Token: 0x0600527A RID: 21114 RVA: 0x002C33DC File Offset: 0x002C15DC
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

		// Token: 0x0600527B RID: 21115 RVA: 0x002C3464 File Offset: 0x002C1664
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
