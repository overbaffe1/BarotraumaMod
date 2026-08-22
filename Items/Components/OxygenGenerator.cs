using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005FA RID: 1530
	internal class OxygenGenerator : Powered
	{
		// Token: 0x17001941 RID: 6465
		// (get) Token: 0x060063BD RID: 25533 RVA: 0x0033E4BC File Offset: 0x0033C6BC
		// (set) Token: 0x060063BE RID: 25534 RVA: 0x0033E4C4 File Offset: 0x0033C6C4
		public float CurrFlow { get; private set; }

		// Token: 0x17001942 RID: 6466
		// (get) Token: 0x060063BF RID: 25535 RVA: 0x0033E4CD File Offset: 0x0033C6CD
		// (set) Token: 0x060063C0 RID: 25536 RVA: 0x0033E4D5 File Offset: 0x0033C6D5
		[Editable]
		[Serialize(400f, IsPropertySaveable.Yes, "How much oxygen the machine generates when operating at full power.", "", true)]
		public float GeneratedAmount
		{
			get
			{
				return this.generatedAmount;
			}
			set
			{
				this.generatedAmount = MathHelper.Clamp(value, -10000f, 10000f);
			}
		}

		// Token: 0x060063C1 RID: 25537 RVA: 0x0033E4ED File Offset: 0x0033C6ED
		public OxygenGenerator(Item item, ContentXElement element) : base(item, element)
		{
			this.ventUpdateTimer = Rand.Range(0f, 5f, Rand.RandSync.Unsynced);
			this.IsActive = true;
		}

		// Token: 0x060063C2 RID: 25538 RVA: 0x0033E514 File Offset: 0x0033C714
		public override void Update(float deltaTime, Camera cam)
		{
			base.UpdateOnActiveEffects(deltaTime);
			this.CurrFlow = 0f;
			if (this.item.CurrentHull == null)
			{
				return;
			}
			if (!this.HasPower && base.PowerConsumption > 0f)
			{
				return;
			}
			this.CurrFlow = Math.Min((base.PowerConsumption > 0f) ? base.Voltage : 1f, 2f) * this.generatedAmount * 100f;
			float conditionMult = this.item.Condition / this.item.MaxCondition;
			this.CurrFlow *= conditionMult * conditionMult;
			this.UpdateVents(this.CurrFlow, deltaTime);
		}

		// Token: 0x060063C3 RID: 25539 RVA: 0x0033E5C4 File Offset: 0x0033C7C4
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			float consumption = this.powerConsumption;
			Repairable component = this.item.GetComponent<Repairable>();
			if (component != null)
			{
				component.AdjustPowerConsumption(ref consumption);
			}
			return consumption;
		}

		// Token: 0x060063C4 RID: 25540 RVA: 0x0033E608 File Offset: 0x0033C808
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.CurrFlow = 0f;
		}

		// Token: 0x060063C5 RID: 25541 RVA: 0x0033E620 File Offset: 0x0033C820
		public void GetVents()
		{
			this.totalHullVolume = 0f;
			if (this.ventList == null)
			{
				this.ventList = new List<ValueTuple<Vent, float>>();
			}
			this.ventList.Clear();
			foreach (MapEntity entity in this.item.linkedTo)
			{
				Item linkedItem = entity as Item;
				if (linkedItem != null)
				{
					Vent vent2 = linkedItem.GetComponent<Vent>();
					if (((vent2 != null) ? vent2.Item.CurrentHull : null) != null)
					{
						this.totalHullVolume += vent2.Item.CurrentHull.Volume;
						this.ventList.Add(new ValueTuple<Vent, float>(vent2, vent2.Item.CurrentHull.Volume));
					}
				}
			}
			for (int i = 0; i < this.ventList.Count; i++)
			{
				Vent vent = this.ventList[i].Item1;
				using (IEnumerator<Hull> enumerator2 = vent.Item.CurrentHull.GetConnectedHulls(false, new int?(3), true).GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Hull connectedHull = enumerator2.Current;
						if (!this.ventList.Any(([TupleElementNames(new string[]
						{
							"vent",
							"hullVolume"
						})] ValueTuple<Vent, float> v) => v.Item1 != vent && v.Item1.Item.CurrentHull == connectedHull))
						{
							this.totalHullVolume += connectedHull.Volume;
							this.ventList[i] = new ValueTuple<Vent, float>(this.ventList[i].Item1, this.ventList[i].Item2 + connectedHull.Volume);
						}
					}
				}
			}
		}

		// Token: 0x060063C6 RID: 25542 RVA: 0x0033E820 File Offset: 0x0033CA20
		private void UpdateVents(float deltaOxygen, float deltaTime)
		{
			if (this.ventList == null || this.ventUpdateTimer < 0f)
			{
				this.GetVents();
				this.ventUpdateTimer = 5f;
			}
			this.ventUpdateTimer -= deltaTime;
			if (!this.ventList.Any<ValueTuple<Vent, float>>() || this.totalHullVolume <= 0f)
			{
				return;
			}
			foreach (ValueTuple<Vent, float> valueTuple in this.ventList)
			{
				Vent vent = valueTuple.Item1;
				float hullVolume = valueTuple.Item2;
				if (vent.Item.CurrentHull != null)
				{
					vent.OxygenFlow = deltaOxygen * (hullVolume / this.totalHullVolume);
					vent.IsActive = true;
				}
			}
		}

		// Token: 0x060063C7 RID: 25543 RVA: 0x0033E8EC File Offset: 0x0033CAEC
		public float GetVentOxygenFlow(Vent targetVent)
		{
			if (this.ventList == null)
			{
				this.GetVents();
			}
			foreach (ValueTuple<Vent, float> valueTuple in this.ventList)
			{
				Vent vent = valueTuple.Item1;
				float hullVolume = valueTuple.Item2;
				if (vent == targetVent)
				{
					return this.generatedAmount * 100f * (hullVolume / this.totalHullVolume);
				}
			}
			return 0f;
		}

		// Token: 0x040033B3 RID: 13235
		private float generatedAmount;

		// Token: 0x040033B4 RID: 13236
		[TupleElementNames(new string[]
		{
			"vent",
			"hullVolume"
		})]
		private List<ValueTuple<Vent, float>> ventList;

		// Token: 0x040033B5 RID: 13237
		private float totalHullVolume;

		// Token: 0x040033B6 RID: 13238
		private float ventUpdateTimer;

		// Token: 0x040033B7 RID: 13239
		private const float VentUpdateInterval = 5f;
	}
}
