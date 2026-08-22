using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200022E RID: 558
	internal class FireSource : ISpatialEntity
	{
		// Token: 0x17000AF4 RID: 2804
		// (get) Token: 0x0600262D RID: 9773 RVA: 0x000F8FB8 File Offset: 0x000F71B8
		protected virtual float SpreadToOtherHullsProbability
		{
			get
			{
				return 0.15f;
			}
		}

		// Token: 0x17000AF5 RID: 2805
		// (get) Token: 0x0600262E RID: 9774 RVA: 0x000F8FBF File Offset: 0x000F71BF
		public Submarine Submarine
		{
			get
			{
				return this.submarine;
			}
		}

		// Token: 0x17000AF6 RID: 2806
		// (get) Token: 0x0600262F RID: 9775 RVA: 0x000F8FC7 File Offset: 0x000F71C7
		// (set) Token: 0x06002630 RID: 9776 RVA: 0x000F8FCF File Offset: 0x000F71CF
		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.position = value;
			}
		}

		// Token: 0x17000AF7 RID: 2807
		// (get) Token: 0x06002631 RID: 9777 RVA: 0x000F8FE1 File Offset: 0x000F71E1
		public Vector2 WorldPosition
		{
			get
			{
				if (this.Submarine != null)
				{
					return this.Submarine.Position + this.position;
				}
				return this.position;
			}
		}

		// Token: 0x17000AF8 RID: 2808
		// (get) Token: 0x06002632 RID: 9778 RVA: 0x000F9008 File Offset: 0x000F7208
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x17000AF9 RID: 2809
		// (get) Token: 0x06002633 RID: 9779 RVA: 0x000F9015 File Offset: 0x000F7215
		// (set) Token: 0x06002634 RID: 9780 RVA: 0x000F9020 File Offset: 0x000F7220
		public Vector2 Size
		{
			get
			{
				return this.size;
			}
			set
			{
				if (value == this.size)
				{
					return;
				}
				Vector2 sizeChange = value - this.size;
				this.size = value;
				this.position.X = this.position.X - sizeChange.X * 0.5f;
				this.LimitSize();
			}
		}

		// Token: 0x17000AFA RID: 2810
		// (get) Token: 0x06002635 RID: 9781 RVA: 0x000F9071 File Offset: 0x000F7271
		public virtual float DamageRange
		{
			get
			{
				return Math.Min((float)Math.Sqrt((double)this.size.X) * 10f, 250f);
			}
		}

		// Token: 0x17000AFB RID: 2811
		// (get) Token: 0x06002636 RID: 9782 RVA: 0x000F9095 File Offset: 0x000F7295
		public float FlameHeight
		{
			get
			{
				return MathHelper.Clamp(this.size.X * 3f, 50f, 400f);
			}
		}

		// Token: 0x17000AFC RID: 2812
		// (get) Token: 0x06002637 RID: 9783 RVA: 0x000F90B7 File Offset: 0x000F72B7
		// (set) Token: 0x06002638 RID: 9784 RVA: 0x000F90BF File Offset: 0x000F72BF
		public bool DamagesItems { get; set; } = true;

		// Token: 0x17000AFD RID: 2813
		// (get) Token: 0x06002639 RID: 9785 RVA: 0x000F90C8 File Offset: 0x000F72C8
		// (set) Token: 0x0600263A RID: 9786 RVA: 0x000F90D0 File Offset: 0x000F72D0
		public bool DamagesCharacters { get; set; } = true;

		// Token: 0x17000AFE RID: 2814
		// (get) Token: 0x0600263B RID: 9787 RVA: 0x000F90D9 File Offset: 0x000F72D9
		public bool Removed
		{
			get
			{
				return this.removed;
			}
		}

		// Token: 0x17000AFF RID: 2815
		// (get) Token: 0x0600263C RID: 9788 RVA: 0x000F90E1 File Offset: 0x000F72E1
		public Hull Hull
		{
			get
			{
				return this.hull;
			}
		}

		// Token: 0x0600263D RID: 9789 RVA: 0x000F90EC File Offset: 0x000F72EC
		public FireSource(Vector2 worldPosition, Hull spawningHull = null, Character sourceCharacter = null, bool isNetworkMessage = false)
		{
			this.hull = Hull.FindHull(worldPosition, spawningHull, true, true);
			if (this.hull == null || worldPosition.Y < this.hull.WorldSurface)
			{
				return;
			}
			this.hull.AddFireSource(this);
			this.position = worldPosition - new Vector2(-5f, 5f);
			if (this.hull.Submarine != null)
			{
				this.submarine = this.hull.Submarine;
				this.position -= this.Submarine.Position;
			}
			this.SourceCharacter = sourceCharacter;
			this.size = new Vector2(10f, 10f);
		}

		// Token: 0x0600263E RID: 9790 RVA: 0x000F91C0 File Offset: 0x000F73C0
		protected virtual void LimitSize()
		{
			if (this.hull == null)
			{
				return;
			}
			this.position.X = Math.Max((float)this.hull.Rect.X, this.position.X);
			this.position.Y = Math.Min((float)this.hull.Rect.Y, this.position.Y);
			this.size.X = Math.Min((float)this.hull.Rect.Width - (this.position.X - (float)this.hull.Rect.X), this.size.X);
			this.size.Y = Math.Min((float)this.hull.Rect.Height - ((float)this.hull.Rect.Y - this.position.Y), this.size.Y);
		}

		// Token: 0x0600263F RID: 9791 RVA: 0x000F92C4 File Offset: 0x000F74C4
		public static void UpdateAll(List<FireSource> fireSources, float deltaTime)
		{
			for (int i = fireSources.Count - 1; i >= 0; i--)
			{
				fireSources[i].Update(deltaTime);
			}
			for (int j = fireSources.Count - 1; j >= 0; j--)
			{
				for (int k = j - 1; k >= 0; k--)
				{
					j = Math.Min(j, fireSources.Count - 1);
					k = Math.Min(k, j - 1);
					if (fireSources[j].CheckOverLap(fireSources[k]))
					{
						float leftEdge = Math.Min(fireSources[j].position.X, fireSources[k].position.X);
						fireSources[k].size.X = Math.Max(fireSources[j].position.X + fireSources[j].size.X, fireSources[k].position.X + fireSources[k].size.X) - leftEdge;
						fireSources[k].position.X = leftEdge;
						fireSources[k].burnDecals.AddRange(fireSources[j].burnDecals);
						fireSources[k].burnDecals.Sort((Decal d1, Decal d2) => Math.Sign(d1.WorldPosition.X - d2.WorldPosition.X));
						fireSources[j].Remove();
					}
				}
			}
		}

		// Token: 0x06002640 RID: 9792 RVA: 0x000F9440 File Offset: 0x000F7640
		public static void UpdateAll(List<DummyFireSource> fireSources, float deltaTime)
		{
			for (int i = fireSources.Count - 1; i >= 0; i--)
			{
				fireSources[i].Update(deltaTime);
			}
			for (int j = fireSources.Count - 1; j >= 0; j--)
			{
				for (int k = j - 1; k >= 0; k--)
				{
					j = Math.Min(j, fireSources.Count - 1);
					k = Math.Min(k, j - 1);
					if (fireSources[j].CheckOverLap(fireSources[k]))
					{
						float leftEdge = Math.Min(fireSources[j].position.X, fireSources[k].position.X);
						fireSources[k].size.X = Math.Max(fireSources[j].position.X + fireSources[j].size.X, fireSources[k].position.X + fireSources[k].size.X) - leftEdge;
						fireSources[k].position.X = leftEdge;
						fireSources[j].Remove();
					}
				}
			}
		}

		// Token: 0x06002641 RID: 9793 RVA: 0x000F9570 File Offset: 0x000F7770
		private bool CheckOverLap(FireSource fireSource)
		{
			return this is DummyFireSource == fireSource is DummyFireSource && this.position.X <= fireSource.position.X + fireSource.size.X && this.position.X + this.size.X >= fireSource.position.X;
		}

		// Token: 0x06002642 RID: 9794 RVA: 0x000F95E0 File Offset: 0x000F77E0
		public void Update(float deltaTime)
		{
			float growModifier = Math.Min(this.hull.OxygenPercentage / 10f - 1f, 1f);
			if (this.DamagesCharacters)
			{
				this.DamageCharacters(deltaTime);
			}
			if (this.DamagesItems)
			{
				this.DamageItems(deltaTime);
			}
			if (this.hull.WaterVolume > 0f)
			{
				this.HullWaterExtinguish(deltaTime);
				if (this.removed)
				{
					return;
				}
			}
			if (!(this is DummyFireSource))
			{
				this.ReduceOxygen(deltaTime);
			}
			this.AdjustXPos(growModifier, deltaTime);
			this.size.X = this.size.X + 20f * growModifier * deltaTime;
			this.size.Y = MathHelper.Clamp(this.size.Y + 20f * growModifier * deltaTime, 10f, 50f);
			if (this.size.X > 50f)
			{
				this.position.Y = MathHelper.Lerp(this.position.Y, (float)(this.hull.Rect.Y - this.hull.Rect.Height) + this.size.Y, deltaTime);
			}
			this.LimitSize();
			if (this.SpreadToOtherHullsProbability > 0f)
			{
				this.spreadToOtherHullsTimer -= deltaTime;
				if (this.spreadToOtherHullsTimer <= 0f)
				{
					this.TrySpreadToNearbyHulls();
					this.spreadToOtherHullsTimer = 5f;
				}
			}
			if (this.size.X > 256f && !(this is DummyFireSource))
			{
				if (this.burnDecals.Count == 0)
				{
					Decal newDecal = this.hull.AddDecal("burnt", this.WorldPosition + this.size / 2f, 1f, false, null);
					if (newDecal != null)
					{
						this.burnDecals.Add(newDecal);
					}
				}
				else if (this.WorldPosition.X < this.burnDecals[0].WorldPosition.X - 256f)
				{
					Decal newDecal2 = this.hull.AddDecal("burnt", this.WorldPosition, 1f, false, null);
					if (newDecal2 != null)
					{
						this.burnDecals.Insert(0, newDecal2);
					}
				}
				else if (this.WorldPosition.X + this.size.X > this.burnDecals[this.burnDecals.Count - 1].WorldPosition.X + 256f)
				{
					Decal newDecal3 = this.hull.AddDecal("burnt", this.WorldPosition + Vector2.UnitX * this.size.X, 1f, false, null);
					if (newDecal3 != null)
					{
						this.burnDecals.Add(newDecal3);
					}
				}
			}
			foreach (Decal d in this.burnDecals)
			{
				d.ForceRefreshFadeTimer(Math.Min(d.FadeTimer, d.FadeInTime));
			}
			if (this.size.X < 1f && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer))
			{
				this.Remove();
			}
		}

		// Token: 0x06002643 RID: 9795 RVA: 0x000F9948 File Offset: 0x000F7B48
		private void TrySpreadToNearbyHulls()
		{
			foreach (Gap gap in this.hull.ConnectedGaps)
			{
				if (gap.IsRoomToRoom && gap.Open > 0f && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) <= this.SpreadToOtherHullsProbability)
				{
					Hull otherHull = (from h in gap.linkedTo
					where h != this.hull
					select h).FirstOrDefault<MapEntity>() as Hull;
					if (otherHull != null && this.position.X <= (float)gap.Rect.Right && this.position.X + this.size.X >= (float)gap.Rect.X && this.position.Y + this.FlameHeight >= (float)(gap.Rect.Y - gap.Rect.Height) && this.position.Y - this.size.Y <= (float)gap.Rect.Y)
					{
						float spawnOffset = 20f;
						Vector2 fireSourcePos = gap.WorldPosition;
						if (gap.IsHorizontal)
						{
							if (otherHull.Position.X < this.hull.Position.X)
							{
								fireSourcePos.X = (float)otherHull.WorldRect.Right - spawnOffset;
							}
							else if (otherHull.Position.X > this.hull.Position.X)
							{
								fireSourcePos.X = (float)otherHull.WorldRect.X + spawnOffset;
							}
						}
						else
						{
							fireSourcePos.X = MathHelper.Clamp(fireSourcePos.X, this.position.X, this.position.X + this.size.X);
							if (otherHull.Position.Y > this.hull.Position.Y)
							{
								fireSourcePos.Y = (float)(otherHull.WorldRect.Y - otherHull.WorldRect.Height) + spawnOffset;
							}
							else if (otherHull.Position.Y < this.hull.Position.Y)
							{
								fireSourcePos.Y = (float)otherHull.WorldRect.Y - spawnOffset;
							}
						}
						new FireSource(fireSourcePos, otherHull, null, false);
					}
				}
			}
		}

		// Token: 0x06002644 RID: 9796 RVA: 0x000F9BE8 File Offset: 0x000F7DE8
		protected virtual void ReduceOxygen(float deltaTime)
		{
			this.hull.Oxygen -= this.size.X * deltaTime * 50f;
		}

		// Token: 0x06002645 RID: 9797 RVA: 0x000F9C0F File Offset: 0x000F7E0F
		protected virtual void AdjustXPos(float growModifier, float deltaTime)
		{
			this.position.X = this.position.X - 20f * growModifier * 0.5f * deltaTime;
		}

		// Token: 0x06002646 RID: 9798 RVA: 0x000F9C30 File Offset: 0x000F7E30
		private void DamageCharacters(float deltaTime)
		{
			if (this.size.X <= 0f)
			{
				return;
			}
			for (int i = 0; i < Character.CharacterList.Count; i++)
			{
				Character c = Character.CharacterList[i];
				if (c.CurrentHull != null && !c.IsDead && this.IsInDamageRange(c, this.DamageRange) && this.hull.GetApproximateDistance(this.Position, c.Position, c.CurrentHull, 10000f, 0f, 0.75f) <= this.size.X + this.DamageRange + this.FlameHeight)
				{
					float dmg = (float)Math.Sqrt((double)Math.Min(500f, this.size.X)) * deltaTime / (float)c.AnimController.Limbs.Count((Limb l) => !l.IsSevered && !l.Hidden);
					foreach (Limb limb in c.AnimController.Limbs)
					{
						if (!limb.IsSevered)
						{
							c.LastDamageSource = this.SourceCharacter;
							c.DamageLimb(this.WorldPosition, limb, AfflictionPrefab.Burn.Instantiate(dmg, null).ToEnumerable<Affliction>(), 0f, false, Vector2.Zero, this.SourceCharacter, 1f, true, 0f, false, false, true);
						}
					}
					c.ApplyStatusEffects(ActionType.OnFire, deltaTime);
				}
			}
		}

		// Token: 0x06002647 RID: 9799 RVA: 0x000F9DBC File Offset: 0x000F7FBC
		public bool IsInDamageRange(Character c, float damageRange)
		{
			return c.Position.X >= this.position.X - damageRange && c.Position.X <= this.position.X + this.size.X + damageRange && c.Position.Y >= this.position.Y - this.size.Y && c.Position.Y <= Math.Max((float)this.hull.Rect.Y, this.position.Y + this.FlameHeight);
		}

		// Token: 0x06002648 RID: 9800 RVA: 0x000F9E68 File Offset: 0x000F8068
		public bool IsInDamageRange(Vector2 worldPosition, float damageRange)
		{
			return worldPosition.X >= this.WorldPosition.X - damageRange && worldPosition.X <= this.WorldPosition.X + this.size.X + damageRange && worldPosition.Y >= this.WorldPosition.Y - this.size.Y && worldPosition.Y <= Math.Max((float)this.hull.WorldRect.Y, this.WorldPosition.Y + this.FlameHeight);
		}

		// Token: 0x06002649 RID: 9801 RVA: 0x000F9F00 File Offset: 0x000F8100
		private void DamageItems(float deltaTime)
		{
			if (this.size.X <= 0f)
			{
				return;
			}
			foreach (Item item in Item.ItemList)
			{
				if (item.CurrentHull == this.hull && !item.FireProof && item.Condition > 0f)
				{
					Item container = item.Container;
					bool fireProof = false;
					while (container != null)
					{
						if (container.FireProof)
						{
							fireProof = true;
							break;
						}
						container = container.Container;
					}
					if (!fireProof)
					{
						float range = (float)Math.Sqrt((double)this.size.X) * 10f;
						if (item.Position.X >= this.position.X - range && item.Position.X <= this.position.X + this.size.X + range && item.Position.Y >= this.position.Y - this.size.Y && item.Position.Y <= (float)this.hull.Rect.Y)
						{
							item.ApplyStatusEffects(ActionType.OnFire, deltaTime, null, null, null, false, null);
							if (item.Condition <= 0f)
							{
								NetworkMember networkMember = GameMain.NetworkMember;
								if (networkMember != null && networkMember.IsServer)
								{
									GameMain.NetworkMember.CreateEntityEvent(item, new Item.ApplyStatusEffectEventData(ActionType.OnFire, null, null, null, null, null));
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600264A RID: 9802 RVA: 0x000FA0C4 File Offset: 0x000F82C4
		private void HullWaterExtinguish(float deltaTime)
		{
			float extinguishAmount = (this.hull.Surface - (this.position.Y - this.size.Y)) * deltaTime;
			if (extinguishAmount < 0f)
			{
				return;
			}
			extinguishAmount = Math.Min(this.size.X, extinguishAmount);
			this.position.X = this.position.X + extinguishAmount / 2f;
			this.size.X = this.size.X - extinguishAmount;
			this.hull.WaterVolume -= extinguishAmount;
			if (this.size.X < 1f && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer))
			{
				this.Remove();
			}
		}

		// Token: 0x0600264B RID: 9803 RVA: 0x000FA178 File Offset: 0x000F8378
		public void Extinguish(float deltaTime, float amount)
		{
			float extinguishAmount = amount * deltaTime;
			extinguishAmount = Math.Min(this.size.X, extinguishAmount);
			this.position.X = this.position.X + extinguishAmount / 2f;
			this.size.X = this.size.X - extinguishAmount;
			this.hull.WaterVolume -= extinguishAmount;
			if (this.size.X < 1f && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer))
			{
				this.Remove();
			}
		}

		// Token: 0x0600264C RID: 9804 RVA: 0x000FA1FF File Offset: 0x000F83FF
		public void Extinguish(float deltaTime, float amount, Vector2 worldPosition)
		{
			if (this.IsInDamageRange(worldPosition, 100f))
			{
				this.Extinguish(deltaTime, amount);
			}
		}

		// Token: 0x0600264D RID: 9805 RVA: 0x000FA218 File Offset: 0x000F8418
		public void Remove()
		{
			foreach (Decal d in this.burnDecals)
			{
				d.StopFadeIn();
			}
			Hull hull = this.hull;
			if (hull != null)
			{
				hull.RemoveFire(this);
			}
			this.removed = true;
		}

		// Token: 0x040012AC RID: 4780
		private const float OxygenConsumption = 50f;

		// Token: 0x040012AD RID: 4781
		private const float GrowSpeed = 20f;

		// Token: 0x040012AE RID: 4782
		private const float MaxDamageRange = 250f;

		// Token: 0x040012AF RID: 4783
		private const float SpreadToOtherHullsInterval = 5f;

		// Token: 0x040012B0 RID: 4784
		protected Hull hull;

		// Token: 0x040012B1 RID: 4785
		protected Vector2 position;

		// Token: 0x040012B2 RID: 4786
		protected Vector2 size;

		// Token: 0x040012B3 RID: 4787
		private readonly Submarine submarine;

		// Token: 0x040012B4 RID: 4788
		protected bool removed;

		// Token: 0x040012B5 RID: 4789
		private readonly List<Decal> burnDecals = new List<Decal>();

		// Token: 0x040012B8 RID: 4792
		public readonly Character SourceCharacter;

		// Token: 0x040012B9 RID: 4793
		private float spreadToOtherHullsTimer;
	}
}
