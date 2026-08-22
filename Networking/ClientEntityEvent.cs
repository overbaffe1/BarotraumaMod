using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000460 RID: 1120
	internal class ClientEntityEvent : NetEntityEvent
	{
		// Token: 0x06004BA6 RID: 19366 RVA: 0x0029BD19 File Offset: 0x00299F19
		public ClientEntityEvent(IClientSerializable entity, ushort eventId, ushort characterStateId) : base(entity, eventId)
		{
			this.serializable = entity;
			this.CharacterStateID = characterStateId;
		}

		// Token: 0x06004BA7 RID: 19367 RVA: 0x0029BD31 File Offset: 0x00299F31
		public void Write(IWriteMessage msg)
		{
			msg.WriteUInt16(this.CharacterStateID);
			this.serializable.ClientEventWrite(msg, base.Data);
		}

		// Token: 0x04002795 RID: 10133
		private readonly IClientSerializable serializable;

		// Token: 0x04002796 RID: 10134
		public readonly ushort CharacterStateID;
	}
}
