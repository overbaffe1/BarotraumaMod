using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004EB RID: 1259
	internal class DelayComponent : ItemComponent
	{
		// Token: 0x17001314 RID: 4884
		// (get) Token: 0x0600471D RID: 18205 RVA: 0x001C5D70 File Offset: 0x001C3F70
		// (set) Token: 0x0600471E RID: 18206 RVA: 0x001C5D78 File Offset: 0x001C3F78
		[InGameEditable(MinValueFloat = 0f, MaxValueFloat = 60f, DecimalCount = 2)]
		[Serialize(1f, IsPropertySaveable.Yes, "How long the item delays the signals (in seconds).", "", true)]
		public float Delay
		{
			get
			{
				return this.delay;
			}
			set
			{
				if (value == this.delay)
				{
					return;
				}
				this.delay = value;
				this.delayTicks = (int)((double)this.delay / 0.016666666666666666);
				this.signalQueueSize = Math.Max(this.delayTicks, 1) * 2;
				this.signalQueue.Clear();
			}
		}

		// Token: 0x17001315 RID: 4885
		// (get) Token: 0x0600471F RID: 18207 RVA: 0x001C5DCD File Offset: 0x001C3FCD
		// (set) Token: 0x06004720 RID: 18208 RVA: 0x001C5DD5 File Offset: 0x001C3FD5
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the component discard previously received signals when a new one is received.", "", true)]
		public bool ResetWhenSignalReceived { get; set; }

		// Token: 0x17001316 RID: 4886
		// (get) Token: 0x06004721 RID: 18209 RVA: 0x001C5DDE File Offset: 0x001C3FDE
		// (set) Token: 0x06004722 RID: 18210 RVA: 0x001C5DE6 File Offset: 0x001C3FE6
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the component discard previously received signals when the incoming signal changes.", "", true)]
		public bool ResetWhenDifferentSignalReceived { get; set; }

		// Token: 0x06004723 RID: 18211 RVA: 0x001C5DEF File Offset: 0x001C3FEF
		public DelayComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004724 RID: 18212 RVA: 0x001C5E0C File Offset: 0x001C400C
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.signalQueue.Count == 0)
			{
				this.IsActive = false;
				return;
			}
			using (Queue<DelayComponent.DelayedSignal>.Enumerator enumerator = this.signalQueue.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DelayComponent.DelayedSignal val = enumerator.Current;
					val.SendTimer--;
				}
				goto IL_C0;
			}
			IL_52:
			DelayComponent.DelayedSignal signalOut = this.signalQueue.Peek();
			signalOut.SendDuration--;
			this.item.SendSignal(new Signal(signalOut.Signal.value, 0, signalOut.Signal.sender, null, 0f, signalOut.Signal.strength), "signal_out");
			if (signalOut.SendDuration > 0)
			{
				return;
			}
			DelayComponent.DelayedSignal delayedSignal;
			this.signalQueue.TryDequeue(out delayedSignal);
			IL_C0:
			if (this.signalQueue.Count > 0 && this.signalQueue.Peek().SendTimer <= 0)
			{
				goto IL_52;
			}
		}

		// Token: 0x06004725 RID: 18213 RVA: 0x001C5F10 File Offset: 0x001C4110
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "signal_in"))
			{
				if (!(name == "set_delay"))
				{
					return;
				}
				float newDelay;
				if (float.TryParse(signal.value, NumberStyles.Any, CultureInfo.InvariantCulture, out newDelay))
				{
					newDelay = MathHelper.Clamp(newDelay, 0f, 60f);
					if (this.signalQueue.Count > 0 && newDelay != this.Delay)
					{
						this.prevQueuedSignal = null;
						this.signalQueue.Clear();
					}
					this.Delay = newDelay;
				}
				return;
			}
			else
			{
				if (this.signalQueue.Count >= this.signalQueueSize)
				{
					return;
				}
				if (this.ResetWhenSignalReceived)
				{
					this.prevQueuedSignal = null;
					this.signalQueue.Clear();
				}
				if (this.ResetWhenDifferentSignalReceived && this.signalQueue.Count > 0 && this.signalQueue.Peek().Signal.value != signal.value)
				{
					this.prevQueuedSignal = null;
					this.signalQueue.Clear();
				}
				if (this.prevQueuedSignal != null && this.prevQueuedSignal.Signal.value == signal.value && MathUtils.NearlyEqual(this.prevQueuedSignal.Signal.strength, signal.strength, 0.0001f) && (this.prevQueuedSignal.SendTimer + this.prevQueuedSignal.SendDuration == this.delayTicks || (this.prevQueuedSignal.SendTimer <= 0 && this.prevQueuedSignal.SendDuration > 0)))
				{
					this.prevQueuedSignal.SendDuration++;
					return;
				}
				this.prevQueuedSignal = new DelayComponent.DelayedSignal(signal, this.delayTicks)
				{
					SendDuration = 1
				};
				this.signalQueue.Enqueue(this.prevQueuedSignal);
				this.IsActive = true;
				return;
			}
		}

		// Token: 0x04002254 RID: 8788
		private int signalQueueSize;

		// Token: 0x04002255 RID: 8789
		private int delayTicks;

		// Token: 0x04002256 RID: 8790
		private readonly Queue<DelayComponent.DelayedSignal> signalQueue = new Queue<DelayComponent.DelayedSignal>();

		// Token: 0x04002257 RID: 8791
		private DelayComponent.DelayedSignal prevQueuedSignal;

		// Token: 0x04002258 RID: 8792
		private float delay;

		// Token: 0x02000E32 RID: 3634
		private class DelayedSignal
		{
			// Token: 0x060069C9 RID: 27081 RVA: 0x00225369 File Offset: 0x00223569
			public DelayedSignal(Signal signal, int sendTimer)
			{
				this.Signal = signal;
				this.SendTimer = sendTimer;
			}

			// Token: 0x0400420F RID: 16911
			public readonly Signal Signal;

			// Token: 0x04004210 RID: 16912
			public int SendTimer;

			// Token: 0x04004211 RID: 16913
			public int SendDuration;
		}
	}
}
