using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000482 RID: 1154
	internal abstract class NetEntityEvent
	{
		// Token: 0x170013F7 RID: 5111
		// (get) Token: 0x06004DD6 RID: 19926 RVA: 0x002ABC22 File Offset: 0x002A9E22
		public ushort EntityID
		{
			get
			{
				return this.Entity.ID;
			}
		}

		// Token: 0x170013F8 RID: 5112
		// (get) Token: 0x06004DD7 RID: 19927 RVA: 0x002ABC2F File Offset: 0x002A9E2F
		// (set) Token: 0x06004DD8 RID: 19928 RVA: 0x002ABC37 File Offset: 0x002A9E37
		public NetEntityEvent.IData Data { get; private set; }

		// Token: 0x06004DD9 RID: 19929 RVA: 0x002ABC40 File Offset: 0x002A9E40
		protected NetEntityEvent(INetSerializable serializableEntity, ushort id)
		{
			this.ID = id;
			this.Entity = (serializableEntity as Entity);
		}

		// Token: 0x06004DDA RID: 19930 RVA: 0x002ABC5B File Offset: 0x002A9E5B
		public void SetData(NetEntityEvent.IData data)
		{
			this.Data = data;
		}

		// Token: 0x06004DDB RID: 19931 RVA: 0x002ABC64 File Offset: 0x002A9E64
		public bool IsDuplicate(NetEntityEvent other)
		{
			return other.Entity == this.Entity && object.Equals(this.Data, other.Data);
		}

		// Token: 0x0400291A RID: 10522
		public readonly Entity Entity;

		// Token: 0x0400291B RID: 10523
		public readonly ushort ID;

		// Token: 0x0400291D RID: 10525
		public bool Sent;

		// Token: 0x0200123B RID: 4667
		public interface IData
		{
		}
	}
}
