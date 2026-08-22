using System;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005DC RID: 1500
	internal class MemoryComponent : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06006109 RID: 24841 RVA: 0x00328D36 File Offset: 0x00326F36
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.Value = msg.ReadString();
		}

		// Token: 0x17001876 RID: 6262
		// (get) Token: 0x0600610A RID: 24842 RVA: 0x00328D44 File Offset: 0x00326F44
		// (set) Token: 0x0600610B RID: 24843 RVA: 0x00328D4C File Offset: 0x00326F4C
		[Editable]
		[Serialize(200, IsPropertySaveable.No, "The maximum length of the stored value. Warning: Large values can lead to large memory usage or networking issues.", "", false)]
		public int MaxValueLength
		{
			get
			{
				return this.maxValueLength;
			}
			set
			{
				this.maxValueLength = Math.Max(value, 0);
			}
		}

		// Token: 0x17001877 RID: 6263
		// (get) Token: 0x0600610C RID: 24844 RVA: 0x00328D5B File Offset: 0x00326F5B
		// (set) Token: 0x0600610D RID: 24845 RVA: 0x00328D64 File Offset: 0x00326F64
		[InGameEditable]
		[Serialize("", IsPropertySaveable.Yes, "The currently stored signal the item outputs.", "", true)]
		public string Value
		{
			get
			{
				return this.value;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.value = value;
				if (this.value.Length > this.MaxValueLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.value = this.value.Substring(0, this.MaxValueLength);
				}
			}
		}

		// Token: 0x17001878 RID: 6264
		// (get) Token: 0x0600610E RID: 24846 RVA: 0x00328DC6 File Offset: 0x00326FC6
		// (set) Token: 0x0600610F RID: 24847 RVA: 0x00328DCE File Offset: 0x00326FCE
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the value stored in the memory component be changed via signals.", "", true)]
		public bool Writeable { get; set; }

		// Token: 0x06006110 RID: 24848 RVA: 0x00328DD7 File Offset: 0x00326FD7
		public MemoryComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06006111 RID: 24849 RVA: 0x00328DE8 File Offset: 0x00326FE8
		public override void Update(float deltaTime, Camera cam)
		{
			this.item.SendSignal(this.Value, "signal_out");
		}

		// Token: 0x06006112 RID: 24850 RVA: 0x00328E00 File Offset: 0x00327000
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "signal_in"))
			{
				if (!(name == "signal_store") && !(name == "lock_state"))
				{
					return;
				}
				this.Writeable = (signal.value == "1");
			}
			else if (this.Writeable)
			{
				string prevValue = this.Value;
				this.Value = signal.value;
				this.Value != prevValue;
				return;
			}
		}

		// Token: 0x0400320D RID: 12813
		private int maxValueLength;

		// Token: 0x0400320E RID: 12814
		private string value;
	}
}
