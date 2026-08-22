using System;

namespace Barotrauma.Networking
{
	// Token: 0x0200036E RID: 878
	internal class ServerEntityEvent : NetEntityEvent
	{
		// Token: 0x17000E7C RID: 3708
		// (get) Token: 0x06003466 RID: 13414 RVA: 0x00168B75 File Offset: 0x00166D75
		public double CreateTime
		{
			get
			{
				return this.createTime;
			}
		}

		// Token: 0x06003467 RID: 13415 RVA: 0x00168B7D File Offset: 0x00166D7D
		public void ResetCreateTime()
		{
			this.createTime = Timing.TotalTime;
		}

		// Token: 0x06003468 RID: 13416 RVA: 0x00168B8A File Offset: 0x00166D8A
		public ServerEntityEvent(IServerSerializable serializableEntity, ushort id) : base(serializableEntity, id)
		{
			this.serializable = serializableEntity;
			this.createTime = Timing.TotalTime;
		}

		// Token: 0x06003469 RID: 13417 RVA: 0x00168BA6 File Offset: 0x00166DA6
		public void Write(IWriteMessage msg, Client recipient)
		{
			this.serializable.ServerEventWrite(msg, recipient, base.Data);
		}

		// Token: 0x04001A13 RID: 6675
		private IServerSerializable serializable;

		// Token: 0x04001A14 RID: 6676
		private double createTime;
	}
}
