using System;
using System.Collections.Generic;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004FD RID: 1277
	internal class SmokeDetector : ItemComponent
	{
		// Token: 0x17001344 RID: 4932
		// (get) Token: 0x060047BB RID: 18363 RVA: 0x001C8986 File Offset: 0x001C6B86
		// (set) Token: 0x060047BC RID: 18364 RVA: 0x001C898E File Offset: 0x001C6B8E
		public bool FireInRange { get; private set; }

		// Token: 0x17001345 RID: 4933
		// (get) Token: 0x060047BD RID: 18365 RVA: 0x001C8997 File Offset: 0x001C6B97
		// (set) Token: 0x060047BE RID: 18366 RVA: 0x001C899F File Offset: 0x001C6B9F
		[Editable]
		[Serialize(200, IsPropertySaveable.No, "The maximum length of the output strings. Warning: Large values can lead to large memory usage or networking issues.", "", false)]
		public int MaxOutputLength
		{
			get
			{
				return this.maxOutputLength;
			}
			set
			{
				this.maxOutputLength = Math.Max(value, 0);
			}
		}

		// Token: 0x17001346 RID: 4934
		// (get) Token: 0x060047BF RID: 18367 RVA: 0x001C89AE File Offset: 0x001C6BAE
		// (set) Token: 0x060047C0 RID: 18368 RVA: 0x001C89B8 File Offset: 0x001C6BB8
		[InGameEditable]
		[Serialize("1", IsPropertySaveable.Yes, "The signal the item outputs when it has detected a fire.", "", true)]
		public string Output
		{
			get
			{
				return this.output;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.output = value;
				if (this.output.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.output = this.output.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x17001347 RID: 4935
		// (get) Token: 0x060047C1 RID: 18369 RVA: 0x001C8A1A File Offset: 0x001C6C1A
		// (set) Token: 0x060047C2 RID: 18370 RVA: 0x001C8A24 File Offset: 0x001C6C24
		[InGameEditable]
		[Serialize("0", IsPropertySaveable.Yes, "The signal the item outputs when it has not detected a fire.", "", true)]
		public string FalseOutput
		{
			get
			{
				return this.falseOutput;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.falseOutput = value;
				if (this.falseOutput.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.falseOutput = this.falseOutput.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x060047C3 RID: 18371 RVA: 0x001C8A86 File Offset: 0x001C6C86
		public SmokeDetector(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x060047C4 RID: 18372 RVA: 0x001C8A98 File Offset: 0x001C6C98
		private bool IsFireInRange()
		{
			if (this.item.CurrentHull == null || this.item.InWater)
			{
				return false;
			}
			IEnumerable<Hull> connectedHulls = this.item.CurrentHull.GetConnectedHulls(true, new int?(10), true);
			foreach (Hull hull in connectedHulls)
			{
				foreach (FireSource fireSource in hull.FireSources)
				{
					if (fireSource.IsInDamageRange(this.item.WorldPosition, Math.Max(fireSource.DamageRange * 2f, 500f)))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060047C5 RID: 18373 RVA: 0x001C8B80 File Offset: 0x001C6D80
		public override void Update(float deltaTime, Camera cam)
		{
			this.fireCheckTimer -= deltaTime;
			if (this.fireCheckTimer <= 0f)
			{
				this.FireInRange = this.IsFireInRange();
				this.fireCheckTimer = 1f;
			}
			string signalOut = this.FireInRange ? this.Output : this.FalseOutput;
			if (!string.IsNullOrEmpty(signalOut))
			{
				this.item.SendSignal(signalOut, "signal_out");
			}
		}

		// Token: 0x040022A9 RID: 8873
		private const float FireCheckInterval = 1f;

		// Token: 0x040022AA RID: 8874
		private float fireCheckTimer;

		// Token: 0x040022AC RID: 8876
		private int maxOutputLength;

		// Token: 0x040022AD RID: 8877
		private string output;

		// Token: 0x040022AE RID: 8878
		private string falseOutput;
	}
}
