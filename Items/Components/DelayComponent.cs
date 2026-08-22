using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200060F RID: 1551
	internal class DelayComponent : ItemComponent
	{
		// Token: 0x17001951 RID: 6481
		// (get) Token: 0x0600640E RID: 25614 RVA: 0x0033F5A3 File Offset: 0x0033D7A3
		// (set) Token: 0x0600640F RID: 25615 RVA: 0x0033F5AC File Offset: 0x0033D7AC
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

		// Token: 0x17001952 RID: 6482
		// (get) Token: 0x06006410 RID: 25616 RVA: 0x0033F601 File Offset: 0x0033D801
		// (set) Token: 0x06006411 RID: 25617 RVA: 0x0033F609 File Offset: 0x0033D809
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the component discard previously received signals when a new one is received.", "", true)]
		public bool ResetWhenSignalReceived { get; set; }

		// Token: 0x17001953 RID: 6483
		// (get) Token: 0x06006412 RID: 25618 RVA: 0x0033F612 File Offset: 0x0033D812
		// (set) Token: 0x06006413 RID: 25619 RVA: 0x0033F61A File Offset: 0x0033D81A
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the component discard previously received signals when the incoming signal changes.", "", true)]
		public bool ResetWhenDifferentSignalReceived { get; set; }

		// Token: 0x06006414 RID: 25620 RVA: 0x0033F623 File Offset: 0x0033D823
		public DelayComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06006415 RID: 25621 RVA: 0x0033F640 File Offset: 0x0033D840
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

		// Token: 0x06006416 RID: 25622 RVA: 0x0033F744 File Offset: 0x0033D944
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

		// Token: 0x040033E9 RID: 13289
		private int signalQueueSize;

		// Token: 0x040033EA RID: 13290
		private int delayTicks;

		// Token: 0x040033EB RID: 13291
		private readonly Queue<DelayComponent.DelayedSignal> signalQueue = new Queue<DelayComponent.DelayedSignal>();

		// Token: 0x040033EC RID: 13292
		private DelayComponent.DelayedSignal prevQueuedSignal;

		// Token: 0x040033ED RID: 13293
		private float delay;

		// Token: 0x0200149C RID: 5276
		private class DelayedSignal
		{
			// Token: 0x06009B7D RID: 39805 RVA: 0x003E5609 File Offset: 0x003E3809
			public DelayedSignal(Signal signal, int sendTimer)
			{
				this.Signal = signal;
				this.SendTimer = sendTimer;
			}

			// Token: 0x04006658 RID: 26200
			public readonly Signal Signal;

			// Token: 0x04006659 RID: 26201
			public int SendTimer;

			// Token: 0x0400665A RID: 26202
			public int SendDuration;
		}
	}
}
