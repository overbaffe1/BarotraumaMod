using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004AC RID: 1196
	internal class MemoryComponent : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06004369 RID: 17257 RVA: 0x001B12B6 File Offset: 0x001AF4B6
		private IEnumerable<CoroutineStatus> SendStateAfterDelay()
		{
			MemoryComponent.<SendStateAfterDelay>d__3 <SendStateAfterDelay>d__ = new MemoryComponent.<SendStateAfterDelay>d__3(-2);
			<SendStateAfterDelay>d__.<>4__this = this;
			return <SendStateAfterDelay>d__;
		}

		// Token: 0x0600436A RID: 17258 RVA: 0x001B12C6 File Offset: 0x001AF4C6
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteString(this.Value);
			this.lastSentValue = this.Value;
		}

		// Token: 0x170011F0 RID: 4592
		// (get) Token: 0x0600436B RID: 17259 RVA: 0x001B12E0 File Offset: 0x001AF4E0
		// (set) Token: 0x0600436C RID: 17260 RVA: 0x001B12E8 File Offset: 0x001AF4E8
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

		// Token: 0x170011F1 RID: 4593
		// (get) Token: 0x0600436D RID: 17261 RVA: 0x001B12F7 File Offset: 0x001AF4F7
		// (set) Token: 0x0600436E RID: 17262 RVA: 0x001B1300 File Offset: 0x001AF500
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

		// Token: 0x170011F2 RID: 4594
		// (get) Token: 0x0600436F RID: 17263 RVA: 0x001B1362 File Offset: 0x001AF562
		// (set) Token: 0x06004370 RID: 17264 RVA: 0x001B136A File Offset: 0x001AF56A
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the value stored in the memory component be changed via signals.", "", true)]
		public bool Writeable { get; set; }

		// Token: 0x06004371 RID: 17265 RVA: 0x001B1373 File Offset: 0x001AF573
		public MemoryComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004372 RID: 17266 RVA: 0x001B1384 File Offset: 0x001AF584
		public override void Update(float deltaTime, Camera cam)
		{
			this.item.SendSignal(this.Value, "signal_out");
		}

		// Token: 0x06004373 RID: 17267 RVA: 0x001B139C File Offset: 0x001AF59C
		private void OnStateChanged()
		{
			this.sendStateTimer = 0.5f;
			if (this.sendStateCoroutine == null)
			{
				this.sendStateCoroutine = CoroutineManager.StartCoroutine(this.SendStateAfterDelay(), "");
			}
		}

		// Token: 0x06004374 RID: 17268 RVA: 0x001B13C8 File Offset: 0x001AF5C8
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
				if (this.Value != prevValue)
				{
					this.OnStateChanged();
					return;
				}
			}
		}

		// Token: 0x0400203B RID: 8251
		private CoroutineHandle sendStateCoroutine;

		// Token: 0x0400203C RID: 8252
		private string lastSentValue;

		// Token: 0x0400203D RID: 8253
		private float sendStateTimer;

		// Token: 0x0400203E RID: 8254
		private int maxValueLength;

		// Token: 0x0400203F RID: 8255
		private string value;
	}
}
