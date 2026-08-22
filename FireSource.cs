using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Lights;
using Barotrauma.Networking;
using Barotrauma.Particles;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000CE RID: 206
	internal class FireSource : ISpatialEntity
	{
		// Token: 0x06001B3B RID: 6971 RVA: 0x0010D324 File Offset: 0x0010B524
		public void EmitParticles(Vector2 size, Vector2 worldPosition, float deltaTime, Hull hull, float growModifier)
		{
			ParticlePrefab particlePrefab = ParticleManager.FindPrefab("flame");
			if (particlePrefab == null)
			{
				return;
			}
			float particlesPerSecond = MathHelper.Clamp(size.X / 2f, 10f, 200f);
			float particleInterval = 1f / particlesPerSecond;
			this.particleTimer += deltaTime;
			while (this.particleTimer > particleInterval)
			{
				this.particleTimer -= particleInterval;
				Vector2 particlePos = new Vector2(worldPosition.X + Rand.Range(0f, size.X, Rand.RandSync.Unsynced), worldPosition.Y - size.Y + particlePrefab.CollisionRadius);
				Vector2 particleVel = new Vector2(particlePos.X - (worldPosition.X + size.X / 2f), Math.Max((float)Math.Sqrt((double)size.X) * Rand.Range(0f, 15f, Rand.RandSync.Unsynced) * growModifier, 0f));
				particleVel.X = MathHelper.Clamp(particleVel.X, -200f, 200f);
				Particle particle = GameMain.ParticleManager.CreateParticle(particlePrefab, particlePos, particleVel, 0f, hull, ParticleDrawOrder.Default, 0f, 1f, null);
				if (particle != null)
				{
					particle.Size *= MathHelper.Clamp(size.X / 60f * Math.Max(hull.Oxygen / hull.Volume, 0.4f), 0.5f, 1f);
					if (Rand.Int(5, Rand.RandSync.Unsynced) == 1)
					{
						Particle smokeParticle = GameMain.ParticleManager.CreateParticle("smoke", particlePos, new Vector2(particleVel.X, particleVel.Y * 0.1f), 0f, hull, 0f, null);
						if (smokeParticle != null)
						{
							smokeParticle.Size *= MathHelper.Clamp(size.X / 100f * Math.Max(hull.Oxygen / hull.Volume, 0.4f), 0.5f, 1f);
						}
					}
				}
			}
		}

		// Token: 0x170006FB RID: 1787
		// (get) Token: 0x06001B3C RID: 6972 RVA: 0x0010D52E File Offset: 0x0010B72E
		protected virtual float SpreadToOtherHullsProbability
		{
			get
			{
				return 0.15f;
			}
		}

		// Token: 0x170006FC RID: 1788
		// (get) Token: 0x06001B3D RID: 6973 RVA: 0x0010D535 File Offset: 0x0010B735
		public Submarine Submarine
		{
			get
			{
				return this.submarine;
			}
		}

		// Token: 0x170006FD RID: 1789
		// (get) Token: 0x06001B3E RID: 6974 RVA: 0x0010D53D File Offset: 0x0010B73D
		// (set) Token: 0x06001B3F RID: 6975 RVA: 0x0010D545 File Offset: 0x0010B745
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

		// Token: 0x170006FE RID: 1790
		// (get) Token: 0x06001B40 RID: 6976 RVA: 0x0010D557 File Offset: 0x0010B757
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

		// Token: 0x170006FF RID: 1791
		// (get) Token: 0x06001B41 RID: 6977 RVA: 0x0010D57E File Offset: 0x0010B77E
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x17000700 RID: 1792
		// (get) Token: 0x06001B42 RID: 6978 RVA: 0x0010D58B File Offset: 0x0010B78B
		// (set) Token: 0x06001B43 RID: 6979 RVA: 0x0010D594 File Offset: 0x0010B794
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

		// Token: 0x17000701 RID: 1793
		// (get) Token: 0x06001B44 RID: 6980 RVA: 0x0010D5E5 File Offset: 0x0010B7E5
		public virtual float DamageRange
		{
			get
			{
				return Math.Min((float)Math.Sqrt((double)this.size.X) * 10f, 250f);
			}
		}

		// Token: 0x17000702 RID: 1794
		// (get) Token: 0x06001B45 RID: 6981 RVA: 0x0010D609 File Offset: 0x0010B809
		public float FlameHeight
		{
			get
			{
				return MathHelper.Clamp(this.size.X * 3f, 50f, 400f);
			}
		}

		// Token: 0x17000703 RID: 1795
		// (get) Token: 0x06001B46 RID: 6982 RVA: 0x0010D62B File Offset: 0x0010B82B
		// (set) Token: 0x06001B47 RID: 6983 RVA: 0x0010D633 File Offset: 0x0010B833
		public bool DamagesItems { get; set; } = true;

		// Token: 0x17000704 RID: 1796
		// (get) Token: 0x06001B48 RID: 6984 RVA: 0x0010D63C File Offset: 0x0010B83C
		// (set) Token: 0x06001B49 RID: 6985 RVA: 0x0010D644 File Offset: 0x0010B844
		public bool DamagesCharacters { get; set; } = true;

		// Token: 0x17000705 RID: 1797
		// (get) Token: 0x06001B4A RID: 6986 RVA: 0x0010D64D File Offset: 0x0010B84D
		public bool Removed
		{
			get
			{
				return this.removed;
			}
		}

		// Token: 0x17000706 RID: 1798
		// (get) Token: 0x06001B4B RID: 6987 RVA: 0x0010D655 File Offset: 0x0010B855
		public Hull Hull
		{
			get
			{
				return this.hull;
			}
		}

		// Token: 0x06001B4C RID: 6988 RVA: 0x0010D660 File Offset: 0x0010B860
		public FireSource(Vector2 worldPosition, Hull spawningHull = null, Character sourceCharacter = null, bool isNetworkMessage = false)
		{
			this.hull = Hull.FindHull(worldPosition, spawningHull, true, true);
			if (this.hull == null || worldPosition.Y < this.hull.WorldSurface)
			{
				return;
			}
			if (!isNetworkMessage && GameMain.Client != null)
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
			Vector2 vector = this.position;
			float range = 50f;
			Color color = new Color(1f, 0.9f, 0.7f);
			Hull hull = this.hull;
			this.lightSource = new LightSource(vector, range, color, (hull != null) ? hull.Submarine : null, true);
			this.size = new Vector2(10f, 10f);
		}

		// Token: 0x06001B4D RID: 6989 RVA: 0x0010D780 File Offset: 0x0010B980
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

		// Token: 0x06001B4E RID: 6990 RVA: 0x0010D884 File Offset: 0x0010BA84
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

		// Token: 0x06001B4F RID: 6991 RVA: 0x0010DA00 File Offset: 0x0010BC00
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

		// Token: 0x06001B50 RID: 6992 RVA: 0x0010DB30 File Offset: 0x0010BD30
		private bool CheckOverLap(FireSource fireSource)
		{
			return this is DummyFireSource == fireSource is DummyFireSource && this.position.X <= fireSource.position.X + fireSource.size.X && this.position.X + this.size.X >= fireSource.position.X;
		}

		// Token: 0x06001B51 RID: 6993 RVA: 0x0010DBA0 File Offset: 0x0010BDA0
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
			this.UpdateProjSpecific(growModifier, deltaTime);
			if (this.size.X < 1f && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer))
			{
				this.Remove();
			}
		}

		// Token: 0x06001B52 RID: 6994 RVA: 0x0010DF10 File Offset: 0x0010C110
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

		// Token: 0x06001B53 RID: 6995 RVA: 0x0010E1B0 File Offset: 0x0010C3B0
		protected virtual void ReduceOxygen(float deltaTime)
		{
			this.hull.Oxygen -= this.size.X * deltaTime * 50f;
		}

		// Token: 0x06001B54 RID: 6996 RVA: 0x0010E1D7 File Offset: 0x0010C3D7
		protected virtual void AdjustXPos(float growModifier, float deltaTime)
		{
			this.position.X = this.position.X - 20f * growModifier * 0.5f * deltaTime;
		}

		// Token: 0x06001B55 RID: 6997 RVA: 0x0010E1F8 File Offset: 0x0010C3F8
		private void UpdateProjSpecific(float growModifier, float deltaTime)
		{
			this.EmitParticles(this.size, this.WorldPosition, deltaTime, this.hull, growModifier);
			this.lightSource.Color = new Color(1f, 0.45f, 0.3f) * Rand.Range(0.8f, 1f, Rand.RandSync.Unsynced);
			if (Math.Abs(this.lightSource.Range * 0.2f - Math.Max(this.size.X, this.size.Y)) > 1f)
			{
				this.lightSource.Range = Math.Max(this.size.X, this.size.Y) * 5f;
			}
			if (Vector2.DistanceSquared(this.lightSource.Position, this.position) > 5f)
			{
				this.lightSource.Position = this.position + Vector2.UnitY * 30f;
			}
		}

		// Token: 0x06001B56 RID: 6998 RVA: 0x0010E2FC File Offset: 0x0010C4FC
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
					c.CharacterHealth.DisplayedVitality = c.Vitality;
					c.ApplyStatusEffects(ActionType.OnFire, deltaTime);
				}
			}
		}

		// Token: 0x06001B57 RID: 6999 RVA: 0x0010E498 File Offset: 0x0010C698
		public bool IsInDamageRange(Character c, float damageRange)
		{
			return c.Position.X >= this.position.X - damageRange && c.Position.X <= this.position.X + this.size.X + damageRange && c.Position.Y >= this.position.Y - this.size.Y && c.Position.Y <= Math.Max((float)this.hull.Rect.Y, this.position.Y + this.FlameHeight);
		}

		// Token: 0x06001B58 RID: 7000 RVA: 0x0010E544 File Offset: 0x0010C744
		public bool IsInDamageRange(Vector2 worldPosition, float damageRange)
		{
			return worldPosition.X >= this.WorldPosition.X - damageRange && worldPosition.X <= this.WorldPosition.X + this.size.X + damageRange && worldPosition.Y >= this.WorldPosition.Y - this.size.Y && worldPosition.Y <= Math.Max((float)this.hull.WorldRect.Y, this.WorldPosition.Y + this.FlameHeight);
		}

		// Token: 0x06001B59 RID: 7001 RVA: 0x0010E5DC File Offset: 0x0010C7DC
		private void DamageItems(float deltaTime)
		{
			if (this.size.X <= 0f)
			{
				return;
			}
			if (GameMain.Client != null)
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

		// Token: 0x06001B5A RID: 7002 RVA: 0x0010E7A8 File Offset: 0x0010C9A8
		private void HullWaterExtinguish(float deltaTime)
		{
			float extinguishAmount = (this.hull.Surface - (this.position.Y - this.size.Y)) * deltaTime;
			if (extinguishAmount < 0f)
			{
				return;
			}
			float steamCount = Rand.Range(-5f, Math.Min(extinguishAmount * 100f, 10f), Rand.RandSync.Unsynced);
			int i = 0;
			while ((float)i < steamCount)
			{
				Vector2 spawnPos = new Vector2(this.WorldPosition.X + Rand.Range(0f, this.size.X, Rand.RandSync.Unsynced), this.WorldPosition.Y + 10f);
				Vector2 speed = new Vector2(spawnPos.X - (this.WorldPosition.X + this.size.X / 2f), (float)Math.Sqrt((double)this.size.X) * Rand.Range(20f, 25f, Rand.RandSync.Unsynced));
				Particle particle = GameMain.ParticleManager.CreateParticle("steam", spawnPos, speed, 0f, this.hull, 0f, null);
				i++;
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

		// Token: 0x06001B5B RID: 7003 RVA: 0x0010E938 File Offset: 0x0010CB38
		public void Extinguish(float deltaTime, float amount)
		{
			float extinguishAmount = amount * deltaTime;
			float steamCount = Rand.Range(-5f, (float)Math.Sqrt((double)amount), Rand.RandSync.Unsynced);
			int i = 0;
			while ((float)i < steamCount)
			{
				Vector2 spawnPos = new Vector2(Rand.Range(this.position.X, this.position.X + this.size.X, Rand.RandSync.Unsynced), Rand.Range(this.position.Y - this.size.Y, this.position.Y, Rand.RandSync.Unsynced) + 10f);
				Vector2 speed = new Vector2(spawnPos.X - (this.position.X + this.size.X / 2f), (float)Math.Sqrt((double)this.size.X) * Rand.Range(20f, 25f, Rand.RandSync.Unsynced));
				Particle particle = GameMain.ParticleManager.CreateParticle("steam", spawnPos, speed, 0f, this.hull, 0f, null);
				i++;
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

		// Token: 0x06001B5C RID: 7004 RVA: 0x0010EAB6 File Offset: 0x0010CCB6
		public void Extinguish(float deltaTime, float amount, Vector2 worldPosition)
		{
			if (this.IsInDamageRange(worldPosition, 100f))
			{
				this.Extinguish(deltaTime, amount);
			}
		}

		// Token: 0x06001B5D RID: 7005 RVA: 0x0010EAD0 File Offset: 0x0010CCD0
		public void Remove()
		{
			LightSource lightSource = this.lightSource;
			if (lightSource != null)
			{
				lightSource.Remove();
			}
			this.lightSource = null;
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

		// Token: 0x04000DF2 RID: 3570
		private LightSource lightSource;

		// Token: 0x04000DF3 RID: 3571
		private float particleTimer;

		// Token: 0x04000DF4 RID: 3572
		private const float OxygenConsumption = 50f;

		// Token: 0x04000DF5 RID: 3573
		private const float GrowSpeed = 20f;

		// Token: 0x04000DF6 RID: 3574
		private const float MaxDamageRange = 250f;

		// Token: 0x04000DF7 RID: 3575
		private const float SpreadToOtherHullsInterval = 5f;

		// Token: 0x04000DF8 RID: 3576
		protected Hull hull;

		// Token: 0x04000DF9 RID: 3577
		protected Vector2 position;

		// Token: 0x04000DFA RID: 3578
		protected Vector2 size;

		// Token: 0x04000DFB RID: 3579
		private readonly Submarine submarine;

		// Token: 0x04000DFC RID: 3580
		protected bool removed;

		// Token: 0x04000DFD RID: 3581
		private readonly List<Decal> burnDecals = new List<Decal>();

		// Token: 0x04000E00 RID: 3584
		public readonly Character SourceCharacter;

		// Token: 0x04000E01 RID: 3585
		private float spreadToOtherHullsTimer;
	}
}
