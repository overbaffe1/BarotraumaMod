using System;

namespace Barotrauma.Networking
{
	// Token: 0x020004C9 RID: 1225
	internal class VoipQueue : IDisposable
	{
		// Token: 0x17001455 RID: 5205
		// (get) Token: 0x06004FD0 RID: 20432 RVA: 0x002AFB60 File Offset: 0x002ADD60
		public int EnqueuedTotalLength
		{
			get
			{
				int enqueuedTotalLength = 0;
				for (int i = 0; i < 8; i++)
				{
					enqueuedTotalLength += this.bufferLengths[i];
				}
				return enqueuedTotalLength;
			}
		}

		// Token: 0x17001456 RID: 5206
		// (get) Token: 0x06004FD1 RID: 20433 RVA: 0x002AFB87 File Offset: 0x002ADD87
		// (set) Token: 0x06004FD2 RID: 20434 RVA: 0x002AFB8F File Offset: 0x002ADD8F
		public byte[] BufferToQueue { get; protected set; }

		// Token: 0x17001457 RID: 5207
		// (get) Token: 0x06004FD3 RID: 20435 RVA: 0x002AFB98 File Offset: 0x002ADD98
		// (set) Token: 0x06004FD4 RID: 20436 RVA: 0x002AFBA0 File Offset: 0x002ADDA0
		public virtual byte QueueID { get; protected set; }

		// Token: 0x17001458 RID: 5208
		// (get) Token: 0x06004FD5 RID: 20437 RVA: 0x002AFBA9 File Offset: 0x002ADDA9
		// (set) Token: 0x06004FD6 RID: 20438 RVA: 0x002AFBB1 File Offset: 0x002ADDB1
		public ushort LatestBufferID { get; protected set; }

		// Token: 0x17001459 RID: 5209
		// (get) Token: 0x06004FD7 RID: 20439 RVA: 0x002AFBBA File Offset: 0x002ADDBA
		// (set) Token: 0x06004FD8 RID: 20440 RVA: 0x002AFBC2 File Offset: 0x002ADDC2
		public bool CanSend { get; protected set; }

		// Token: 0x1700145A RID: 5210
		// (get) Token: 0x06004FD9 RID: 20441 RVA: 0x002AFBCB File Offset: 0x002ADDCB
		// (set) Token: 0x06004FDA RID: 20442 RVA: 0x002AFBD3 File Offset: 0x002ADDD3
		public bool CanReceive { get; protected set; }

		// Token: 0x1700145B RID: 5211
		// (get) Token: 0x06004FDB RID: 20443 RVA: 0x002AFBDC File Offset: 0x002ADDDC
		// (set) Token: 0x06004FDC RID: 20444 RVA: 0x002AFBE4 File Offset: 0x002ADDE4
		public bool ForceLocal { get; set; }

		// Token: 0x1700145C RID: 5212
		// (get) Token: 0x06004FDD RID: 20445 RVA: 0x002AFBED File Offset: 0x002ADDED
		// (set) Token: 0x06004FDE RID: 20446 RVA: 0x002AFBF5 File Offset: 0x002ADDF5
		public DateTime LastReadTime { get; private set; }

		// Token: 0x06004FDF RID: 20447 RVA: 0x002AFC00 File Offset: 0x002ADE00
		public VoipQueue(byte id, bool canSend, bool canReceive)
		{
			this.BufferToQueue = new byte[40];
			this.newestBufferInd = 7;
			this.bufferLengths = new int[8];
			this.buffers = new byte[8][];
			for (int i = 0; i < 8; i++)
			{
				this.buffers[i] = new byte[40];
			}
			this.QueueID = id;
			this.CanSend = canSend;
			this.CanReceive = canReceive;
			this.LatestBufferID = 7;
			this.firstRead = true;
			this.LastReadTime = DateTime.Now;
		}

		// Token: 0x06004FE0 RID: 20448 RVA: 0x002AFC88 File Offset: 0x002ADE88
		public void EnqueueBuffer(int length)
		{
			if (length > 255)
			{
				return;
			}
			this.newestBufferInd = (this.newestBufferInd + 1) % 8;
			int enqueuedTotalLength = this.EnqueuedTotalLength;
			this.bufferLengths[this.newestBufferInd] = length;
			this.BufferToQueue.CopyTo(this.buffers[this.newestBufferInd], 0);
			if (enqueuedTotalLength + length > 0)
			{
				ushort latestBufferID = this.LatestBufferID;
				this.LatestBufferID = latestBufferID + 1;
			}
		}

		// Token: 0x06004FE1 RID: 20449 RVA: 0x002AFCF4 File Offset: 0x002ADEF4
		public void RetrieveBuffer(int id, out int outSize, out byte[] outBuf)
		{
			byte[][] obj = this.buffers;
			lock (obj)
			{
				if (id >= (int)(this.LatestBufferID - 7) && id <= (int)this.LatestBufferID)
				{
					int index = this.newestBufferInd - ((int)this.LatestBufferID - id);
					if (index < 0)
					{
						index += 8;
					}
					outSize = this.bufferLengths[index];
					outBuf = this.buffers[index];
					return;
				}
			}
			outSize = -1;
			outBuf = null;
		}

		// Token: 0x06004FE2 RID: 20450 RVA: 0x002AFD78 File Offset: 0x002ADF78
		public virtual void Write(IWriteMessage msg)
		{
			if (!this.CanSend)
			{
				throw new Exception("Called Write on a VoipQueue not set up for sending");
			}
			msg.WriteUInt16(this.LatestBufferID);
			msg.WriteBoolean(this.ForceLocal);
			msg.WritePadBits();
			byte[][] obj = this.buffers;
			lock (obj)
			{
				for (int i = 0; i < 8; i++)
				{
					int index = (this.newestBufferInd + i + 1) % 8;
					msg.WriteByte((byte)this.bufferLengths[index]);
					msg.WriteBytes(this.buffers[index], 0, this.bufferLengths[index]);
				}
			}
		}

		// Token: 0x06004FE3 RID: 20451 RVA: 0x002AFE24 File Offset: 0x002AE024
		public virtual bool Read(IReadMessage msg, bool discardData = false)
		{
			if (!this.CanReceive)
			{
				throw new Exception("Called Read on a VoipQueue not set up for receiving");
			}
			ushort incLatestBufferID = msg.ReadUInt16();
			if ((this.firstRead || NetIdUtils.IdMoreRecent(incLatestBufferID, this.LatestBufferID)) && !discardData)
			{
				this.ForceLocal = msg.ReadBoolean();
				msg.ReadPadBits();
				this.firstRead = false;
				byte[][] obj = this.buffers;
				lock (obj)
				{
					for (int i = 0; i < 8; i++)
					{
						this.bufferLengths[i] = (int)msg.ReadByte();
						this.buffers[i] = msg.ReadBytes(this.bufferLengths[i]);
					}
				}
				this.newestBufferInd = 7;
				this.LatestBufferID = incLatestBufferID;
				this.LastReadTime = DateTime.Now;
				return true;
			}
			msg.ReadBoolean();
			msg.ReadPadBits();
			for (int j = 0; j < 8; j++)
			{
				byte len = msg.ReadByte();
				msg.BitPosition += (int)(len * 8);
			}
			return false;
		}

		// Token: 0x06004FE4 RID: 20452 RVA: 0x002AFF34 File Offset: 0x002AE134
		public virtual void Dispose()
		{
		}

		// Token: 0x04002A0E RID: 10766
		public const int BUFFER_COUNT = 8;

		// Token: 0x04002A0F RID: 10767
		protected int[] bufferLengths;

		// Token: 0x04002A10 RID: 10768
		protected byte[][] buffers;

		// Token: 0x04002A11 RID: 10769
		protected int newestBufferInd;

		// Token: 0x04002A12 RID: 10770
		protected bool firstRead;
	}
}
