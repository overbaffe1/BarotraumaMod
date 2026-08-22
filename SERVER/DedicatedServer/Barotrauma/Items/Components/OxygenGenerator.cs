using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004CF RID: 1231
	internal class OxygenGenerator : Powered
	{
		// Token: 0x170012D3 RID: 4819
		// (get) Token: 0x06004628 RID: 17960 RVA: 0x001C047D File Offset: 0x001BE67D
		// (set) Token: 0x06004629 RID: 17961 RVA: 0x001C0485 File Offset: 0x001BE685
		public float CurrFlow { get; private set; }

		// Token: 0x170012D4 RID: 4820
		// (get) Token: 0x0600462A RID: 17962 RVA: 0x001C048E File Offset: 0x001BE68E
		// (set) Token: 0x0600462B RID: 17963 RVA: 0x001C0496 File Offset: 0x001BE696
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

		// Token: 0x0600462C RID: 17964 RVA: 0x001C04AE File Offset: 0x001BE6AE
		public OxygenGenerator(Item item, ContentXElement element) : base(item, element)
		{
			this.ventUpdateTimer = Rand.Range(0f, 5f, Rand.RandSync.Unsynced);
			this.IsActive = true;
		}

		// Token: 0x0600462D RID: 17965 RVA: 0x001C04D8 File Offset: 0x001BE6D8
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

		// Token: 0x0600462E RID: 17966 RVA: 0x001C0588 File Offset: 0x001BE788
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

		// Token: 0x0600462F RID: 17967 RVA: 0x001C05CC File Offset: 0x001BE7CC
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.CurrFlow = 0f;
		}

		// Token: 0x06004630 RID: 17968 RVA: 0x001C05E4 File Offset: 0x001BE7E4
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

		// Token: 0x06004631 RID: 17969 RVA: 0x001C07E4 File Offset: 0x001BE9E4
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

		// Token: 0x06004632 RID: 17970 RVA: 0x001C08B0 File Offset: 0x001BEAB0
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

		// Token: 0x040021B3 RID: 8627
		private float generatedAmount;

		// Token: 0x040021B4 RID: 8628
		[TupleElementNames(new string[]
		{
			"vent",
			"hullVolume"
		})]
		private List<ValueTuple<Vent, float>> ventList;

		// Token: 0x040021B5 RID: 8629
		private float totalHullVolume;

		// Token: 0x040021B6 RID: 8630
		private float ventUpdateTimer;

		// Token: 0x040021B7 RID: 8631
		private const float VentUpdateInterval = 5f;
	}
}
