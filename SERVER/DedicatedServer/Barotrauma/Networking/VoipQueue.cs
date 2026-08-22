using System;

namespace Barotrauma.Networking
{
	// Token: 0x020003CE RID: 974
	internal class VoipQueue : IDisposable
	{
		// Token: 0x17000F5B RID: 3931
		// (get) Token: 0x06003802 RID: 14338 RVA: 0x00177498 File Offset: 0x00175698
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

		// Token: 0x17000F5C RID: 3932
		// (get) Token: 0x06003803 RID: 14339 RVA: 0x001774BF File Offset: 0x001756BF
		// (set) Token: 0x06003804 RID: 14340 RVA: 0x001774C7 File Offset: 0x001756C7
		public byte[] BufferToQueue { get; protected set; }

		// Token: 0x17000F5D RID: 3933
		// (get) Token: 0x06003805 RID: 14341 RVA: 0x001774D0 File Offset: 0x001756D0
		// (set) Token: 0x06003806 RID: 14342 RVA: 0x001774D8 File Offset: 0x001756D8
		public virtual byte QueueID { get; protected set; }

		// Token: 0x17000F5E RID: 3934
		// (get) Token: 0x06003807 RID: 14343 RVA: 0x001774E1 File Offset: 0x001756E1
		// (set) Token: 0x06003808 RID: 14344 RVA: 0x001774E9 File Offset: 0x001756E9
		public ushort LatestBufferID { get; protected set; }

		// Token: 0x17000F5F RID: 3935
		// (get) Token: 0x06003809 RID: 14345 RVA: 0x001774F2 File Offset: 0x001756F2
		// (set) Token: 0x0600380A RID: 14346 RVA: 0x001774FA File Offset: 0x001756FA
		public bool CanSend { get; protected set; }

		// Token: 0x17000F60 RID: 3936
		// (get) Token: 0x0600380B RID: 14347 RVA: 0x00177503 File Offset: 0x00175703
		// (set) Token: 0x0600380C RID: 14348 RVA: 0x0017750B File Offset: 0x0017570B
		public bool CanReceive { get; protected set; }

		// Token: 0x17000F61 RID: 3937
		// (get) Token: 0x0600380D RID: 14349 RVA: 0x00177514 File Offset: 0x00175714
		// (set) Token: 0x0600380E RID: 14350 RVA: 0x0017751C File Offset: 0x0017571C
		public bool ForceLocal { get; set; }

		// Token: 0x17000F62 RID: 3938
		// (get) Token: 0x0600380F RID: 14351 RVA: 0x00177525 File Offset: 0x00175725
		// (set) Token: 0x06003810 RID: 14352 RVA: 0x0017752D File Offset: 0x0017572D
		public DateTime LastReadTime { get; private set; }

		// Token: 0x06003811 RID: 14353 RVA: 0x00177538 File Offset: 0x00175738
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

		// Token: 0x06003812 RID: 14354 RVA: 0x001775C0 File Offset: 0x001757C0
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

		// Token: 0x06003813 RID: 14355 RVA: 0x0017762C File Offset: 0x0017582C
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

		// Token: 0x06003814 RID: 14356 RVA: 0x001776B0 File Offset: 0x001758B0
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

		// Token: 0x06003815 RID: 14357 RVA: 0x0017775C File Offset: 0x0017595C
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

		// Token: 0x06003816 RID: 14358 RVA: 0x0017786C File Offset: 0x00175A6C
		public virtual void Dispose()
		{
		}

		// Token: 0x04001C23 RID: 7203
		public const int BUFFER_COUNT = 8;

		// Token: 0x04001C24 RID: 7204
		protected int[] bufferLengths;

		// Token: 0x04001C25 RID: 7205
		protected byte[][] buffers;

		// Token: 0x04001C26 RID: 7206
		protected int newestBufferInd;

		// Token: 0x04001C27 RID: 7207
		protected bool firstRead;
	}
}
