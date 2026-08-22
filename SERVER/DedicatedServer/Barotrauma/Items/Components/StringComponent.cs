using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004FE RID: 1278
	internal abstract class StringComponent : ItemComponent
	{
		// Token: 0x17001348 RID: 4936
		// (get) Token: 0x060047C6 RID: 18374 RVA: 0x001C8BEF File Offset: 0x001C6DEF
		// (set) Token: 0x060047C7 RID: 18375 RVA: 0x001C8BF8 File Offset: 0x001C6DF8
		[InGameEditable(DecimalCount = 2)]
		[Serialize(0f, IsPropertySaveable.Yes, "The item must have received signals to both inputs within this timeframe to output the result. If set to 0, the inputs must be received at the same time.", "", true)]
		public float TimeFrame
		{
			get
			{
				return this.timeFrame;
			}
			set
			{
				if (value > this.timeFrame)
				{
					this.timeSinceReceived[0] = (this.timeSinceReceived[1] = Math.Max(value * 2f, 0.1f));
				}
				this.timeFrame = Math.Max(0f, value);
			}
		}

		// Token: 0x060047C8 RID: 18376 RVA: 0x001C8C44 File Offset: 0x001C6E44
		public StringComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.timeSinceReceived = new float[]
			{
				Math.Max(this.timeFrame * 2f, 0.1f),
				Math.Max(this.timeFrame * 2f, 0.1f)
			};
			this.receivedSignal = new string[2];
		}

		// Token: 0x060047C9 RID: 18377 RVA: 0x001C8CA4 File Offset: 0x001C6EA4
		public sealed override void Update(float deltaTime, Camera cam)
		{
			bool deactivate = true;
			bool earlyReturn = false;
			for (int i = 0; i < this.timeSinceReceived.Length; i++)
			{
				deactivate &= (this.timeSinceReceived[i] > this.timeFrame);
				earlyReturn |= (this.timeSinceReceived[i] > this.timeFrame);
				this.timeSinceReceived[i] += deltaTime;
			}
			this.IsActive = !deactivate;
			if (earlyReturn)
			{
				return;
			}
			string output = this.Calculate(this.receivedSignal[0], this.receivedSignal[1]);
			this.item.SendSignal(output, "signal_out");
		}

		// Token: 0x060047CA RID: 18378
		protected abstract string Calculate(string signal1, string signal2);

		// Token: 0x060047CB RID: 18379 RVA: 0x001C8D38 File Offset: 0x001C6F38
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "signal_in1")
			{
				this.receivedSignal[0] = signal.value;
				this.timeSinceReceived[0] = 0f;
				this.IsActive = true;
				return;
			}
			if (!(name == "signal_in2"))
			{
				return;
			}
			this.receivedSignal[1] = signal.value;
			this.timeSinceReceived[1] = 0f;
			this.IsActive = true;
		}

		// Token: 0x040022AF RID: 8879
		protected float[] timeSinceReceived;

		// Token: 0x040022B0 RID: 8880
		protected string[] receivedSignal;

		// Token: 0x040022B1 RID: 8881
		protected float timeFrame;
	}
}
