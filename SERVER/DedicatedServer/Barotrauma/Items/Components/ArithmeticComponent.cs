using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E3 RID: 1251
	internal abstract class ArithmeticComponent : ItemComponent
	{
		// Token: 0x17001301 RID: 4865
		// (get) Token: 0x060046D7 RID: 18135 RVA: 0x001C467B File Offset: 0x001C287B
		// (set) Token: 0x060046D8 RID: 18136 RVA: 0x001C4683 File Offset: 0x001C2883
		[Serialize(999999f, IsPropertySaveable.Yes, "The output of the item is restricted below this value.", "", true)]
		[InGameEditable(MinValueFloat = -999999f, MaxValueFloat = 999999f)]
		public float ClampMax { get; set; }

		// Token: 0x17001302 RID: 4866
		// (get) Token: 0x060046D9 RID: 18137 RVA: 0x001C468C File Offset: 0x001C288C
		// (set) Token: 0x060046DA RID: 18138 RVA: 0x001C4694 File Offset: 0x001C2894
		[Serialize(-999999f, IsPropertySaveable.Yes, "The output of the item is restricted above this value.", "", true)]
		[InGameEditable(MinValueFloat = -999999f, MaxValueFloat = 999999f)]
		public float ClampMin { get; set; }

		// Token: 0x17001303 RID: 4867
		// (get) Token: 0x060046DB RID: 18139 RVA: 0x001C469D File Offset: 0x001C289D
		// (set) Token: 0x060046DC RID: 18140 RVA: 0x001C46A8 File Offset: 0x001C28A8
		[InGameEditable(DecimalCount = 2)]
		[Serialize(0f, IsPropertySaveable.Yes, "The item must have received signals to both inputs within this timeframe to output the result. If set to 0, the inputs must be received at the same time.", "sp.", true)]
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

		// Token: 0x060046DD RID: 18141 RVA: 0x001C46F4 File Offset: 0x001C28F4
		public ArithmeticComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.timeSinceReceived = new float[]
			{
				Math.Max(this.timeFrame * 2f, 0.1f),
				Math.Max(this.timeFrame * 2f, 0.1f)
			};
			this.receivedSignal = new float[2];
		}

		// Token: 0x060046DE RID: 18142 RVA: 0x001C4760 File Offset: 0x001C2960
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
			float output = this.Calculate(this.receivedSignal[0], this.receivedSignal[1]);
			if (MathUtils.IsValid(output))
			{
				this.item.SendSignal(new Signal(MathHelper.Clamp(output, this.ClampMin, this.ClampMax).ToString("G", CultureInfo.InvariantCulture), 0, this.signalSender[0] ?? this.signalSender[1], null, 0f, 1f), "signal_out");
			}
		}

		// Token: 0x060046DF RID: 18143
		protected abstract float Calculate(float signal1, float signal2);

		// Token: 0x060046E0 RID: 18144 RVA: 0x001C4844 File Offset: 0x001C2A44
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "signal_in1")
			{
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[0]);
				this.signalSender[0] = signal.sender;
				this.timeSinceReceived[0] = 0f;
				this.IsActive = true;
				return;
			}
			if (!(name == "signal_in2"))
			{
				return;
			}
			float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[1]);
			this.signalSender[1] = signal.sender;
			this.timeSinceReceived[1] = 0f;
			this.IsActive = true;
		}

		// Token: 0x0400222C RID: 8748
		protected float[] timeSinceReceived;

		// Token: 0x0400222D RID: 8749
		protected float[] receivedSignal;

		// Token: 0x0400222E RID: 8750
		protected float timeFrame;

		// Token: 0x0400222F RID: 8751
		protected readonly Character[] signalSender = new Character[2];
	}
}
