using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000386 RID: 902
	internal abstract class NetEntityEvent
	{
		// Token: 0x17000F06 RID: 3846
		// (get) Token: 0x0600361A RID: 13850 RVA: 0x0017325A File Offset: 0x0017145A
		public ushort EntityID
		{
			get
			{
				return this.Entity.ID;
			}
		}

		// Token: 0x17000F07 RID: 3847
		// (get) Token: 0x0600361B RID: 13851 RVA: 0x00173267 File Offset: 0x00171467
		// (set) Token: 0x0600361C RID: 13852 RVA: 0x0017326F File Offset: 0x0017146F
		public NetEntityEvent.IData Data { get; private set; }

		// Token: 0x0600361D RID: 13853 RVA: 0x00173278 File Offset: 0x00171478
		protected NetEntityEvent(INetSerializable serializableEntity, ushort id)
		{
			this.ID = id;
			this.Entity = (serializableEntity as Entity);
		}

		// Token: 0x0600361E RID: 13854 RVA: 0x00173293 File Offset: 0x00171493
		public void SetData(NetEntityEvent.IData data)
		{
			this.Data = data;
		}

		// Token: 0x0600361F RID: 13855 RVA: 0x0017329C File Offset: 0x0017149C
		public bool IsDuplicate(NetEntityEvent other)
		{
			return other.Entity == this.Entity && object.Equals(this.Data, other.Data);
		}

		// Token: 0x04001B2F RID: 6959
		public readonly Entity Entity;

		// Token: 0x04001B30 RID: 6960
		public readonly ushort ID;

		// Token: 0x04001B32 RID: 6962
		public bool Sent;

		// Token: 0x02000C3B RID: 3131
		public interface IData
		{
		}
	}
}
