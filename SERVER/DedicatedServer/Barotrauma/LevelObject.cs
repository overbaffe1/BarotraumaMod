using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200023C RID: 572
	internal class LevelObject : ISpatialEntity, IDamageable, ISerializableEntity
	{
		// Token: 0x17000B96 RID: 2966
		// (get) Token: 0x060027BF RID: 10175 RVA: 0x0010179E File Offset: 0x000FF99E
		// (set) Token: 0x060027C0 RID: 10176 RVA: 0x001017A6 File Offset: 0x000FF9A6
		public Vector3 Position { get; set; }

		// Token: 0x17000B97 RID: 2967
		// (get) Token: 0x060027C1 RID: 10177 RVA: 0x001017AF File Offset: 0x000FF9AF
		// (set) Token: 0x060027C2 RID: 10178 RVA: 0x001017B7 File Offset: 0x000FF9B7
		public PhysicsBody PhysicsBody { get; private set; }

		// Token: 0x17000B98 RID: 2968
		// (get) Token: 0x060027C3 RID: 10179 RVA: 0x001017C0 File Offset: 0x000FF9C0
		// (set) Token: 0x060027C4 RID: 10180 RVA: 0x001017C8 File Offset: 0x000FF9C8
		public List<LevelTrigger> Triggers { get; private set; }

		// Token: 0x17000B99 RID: 2969
		// (get) Token: 0x060027C5 RID: 10181 RVA: 0x001017D1 File Offset: 0x000FF9D1
		// (set) Token: 0x060027C6 RID: 10182 RVA: 0x00101811 File Offset: 0x000FFA11
		public bool NeedsNetworkSyncing
		{
			get
			{
				if (this.tookDamage)
				{
					return true;
				}
				if (this.Triggers != null)
				{
					return this.Triggers.Any((LevelTrigger t) => t.NeedsNetworkSyncing);
				}
				return false;
			}
			set
			{
				if (this.Triggers == null)
				{
					return;
				}
				this.Triggers.ForEach(delegate(LevelTrigger t)
				{
					t.NeedsNetworkSyncing = false;
				});
				this.tookDamage = false;
			}
		}

		// Token: 0x17000B9A RID: 2970
		// (get) Token: 0x060027C7 RID: 10183 RVA: 0x0010184D File Offset: 0x000FFA4D
		// (set) Token: 0x060027C8 RID: 10184 RVA: 0x00101855 File Offset: 0x000FFA55
		public bool NeedsUpdate { get; private set; }

		// Token: 0x17000B9B RID: 2971
		// (get) Token: 0x060027C9 RID: 10185 RVA: 0x0010185E File Offset: 0x000FFA5E
		// (set) Token: 0x060027CA RID: 10186 RVA: 0x00101866 File Offset: 0x000FFA66
		public float Health { get; private set; }

		// Token: 0x17000B9C RID: 2972
		// (get) Token: 0x060027CB RID: 10187 RVA: 0x00101870 File Offset: 0x000FFA70
		public Sprite Sprite
		{
			get
			{
				LevelObjectPrefab activePrefab = this.ActivePrefab;
				LevelObjectPrefab prefab = (activePrefab != null && activePrefab.Sprites.Count > 0) ? this.ActivePrefab : this.Prefab;
				if (this.spriteIndex >= 0 && prefab.Sprites.Count != 0)
				{
					return prefab.Sprites[this.spriteIndex % prefab.Sprites.Count];
				}
				return null;
			}
		}

		// Token: 0x17000B9D RID: 2973
		// (get) Token: 0x060027CC RID: 10188 RVA: 0x001018DD File Offset: 0x000FFADD
		Vector2 ISpatialEntity.Position
		{
			get
			{
				return new Vector2(this.Position.X, this.Position.Y);
			}
		}

		// Token: 0x17000B9E RID: 2974
		// (get) Token: 0x060027CD RID: 10189 RVA: 0x001018FA File Offset: 0x000FFAFA
		public Vector2 WorldPosition
		{
			get
			{
				return new Vector2(this.Position.X, this.Position.Y);
			}
		}

		// Token: 0x17000B9F RID: 2975
		// (get) Token: 0x060027CE RID: 10190 RVA: 0x00101917 File Offset: 0x000FFB17
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.WorldPosition);
			}
		}

		// Token: 0x17000BA0 RID: 2976
		// (get) Token: 0x060027CF RID: 10191 RVA: 0x00101924 File Offset: 0x000FFB24
		public Submarine Submarine
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000BA1 RID: 2977
		// (get) Token: 0x060027D0 RID: 10192 RVA: 0x00101927 File Offset: 0x000FFB27
		public string Name
		{
			get
			{
				LevelObjectPrefab prefab = this.Prefab;
				return ((prefab != null) ? prefab.Name : null) ?? "LevelObject (null)";
			}
		}

		// Token: 0x17000BA2 RID: 2978
		// (get) Token: 0x060027D1 RID: 10193 RVA: 0x00101944 File Offset: 0x000FFB44
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; } = new Dictionary<Identifier, SerializableProperty>();

		// Token: 0x060027D2 RID: 10194 RVA: 0x0010194C File Offset: 0x000FFB4C
		public LevelObject(LevelObjectPrefab prefab, Vector3 position, float scale, float rotation = 0f)
		{
			this.Prefab = prefab;
			this.ActivePrefab = prefab;
			this.Position = position;
			this.Scale = scale;
			this.Rotation = rotation;
			this.Health = prefab.Health;
			this.spriteIndex = (this.ActivePrefab.Sprites.Any<Sprite>() ? Rand.Int(this.ActivePrefab.Sprites.Count, Rand.RandSync.ServerAndClient) : -1);
			if (this.Sprite != null && prefab.SpriteSpecificPhysicsBodyElements.ContainsKey(this.Sprite))
			{
				this.PhysicsBody = new PhysicsBody(prefab.SpriteSpecificPhysicsBodyElements[this.Sprite], ConvertUnits.ToSimUnits(new Vector2(position.X, position.Y)), this.Scale, null, Category.Cat5, Category.Cat1 | Category.Cat3 | Category.Cat8, true);
			}
			else if (prefab.PhysicsBodyElement != null)
			{
				this.PhysicsBody = new PhysicsBody(prefab.PhysicsBodyElement, ConvertUnits.ToSimUnits(new Vector2(position.X, position.Y)), this.Scale, null, Category.Cat5, Category.Cat1 | Category.Cat3 | Category.Cat8, true);
			}
			if (this.PhysicsBody != null)
			{
				this.PhysicsBody.UserData = this;
				this.PhysicsBody.SetTransformIgnoreContacts(this.PhysicsBody.SimPosition, -this.Rotation, true);
				this.PhysicsBody.BodyType = BodyType.Static;
				this.PhysicsBody.CollisionCategories = Category.Cat8;
				this.PhysicsBody.CollidesWith = (this.Prefab.TakeLevelWallDamage ? (Category.Cat1 | Category.Cat2 | Category.Cat7) : (Category.Cat1 | Category.Cat2));
			}
			foreach (ContentXElement triggerElement in prefab.LevelTriggerElements)
			{
				if (this.Triggers == null)
				{
					this.Triggers = new List<LevelTrigger>();
				}
				ContentXElement contentXElement = triggerElement;
				string key = "position";
				Vector2 zero = Vector2.Zero;
				Vector2 triggerPosition = contentXElement.GetAttributeVector2(key, zero) * scale;
				if (rotation != 0f)
				{
					float ca = (float)Math.Cos((double)rotation);
					float sa = (float)Math.Sin((double)rotation);
					triggerPosition = new Vector2(ca * triggerPosition.X + sa * triggerPosition.Y, -sa * triggerPosition.X + ca * triggerPosition.Y);
				}
				LevelTrigger newTrigger = new LevelTrigger(triggerElement, new Vector2(position.X, position.Y) + triggerPosition, -rotation, scale, prefab.Name);
				if (newTrigger.PhysicsBody != null)
				{
					newTrigger.PhysicsBody.UserData = this;
				}
				int parentTriggerIndex = prefab.LevelTriggerElements.IndexOf(triggerElement.Parent);
				if (parentTriggerIndex > -1)
				{
					newTrigger.ParentTrigger = this.Triggers[parentTriggerIndex];
				}
				this.Triggers.Add(newTrigger);
			}
			if (this.spriteIndex == -1)
			{
				foreach (LevelObjectPrefab overrideProperties in prefab.OverrideProperties)
				{
					if (overrideProperties != null && overrideProperties.Sprites.Count > 0)
					{
						this.spriteIndex = Rand.Int(overrideProperties.Sprites.Count, Rand.RandSync.ServerAndClient);
						break;
					}
				}
			}
			this.NeedsUpdate = (this.NeedsNetworkSyncing || (this.Triggers != null && this.Triggers.Any<LevelTrigger>()) || this.Prefab.PhysicsBodyTriggerIndex > -1);
		}

		// Token: 0x060027D3 RID: 10195 RVA: 0x00101CE8 File Offset: 0x000FFEE8
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true)
		{
			if (this.Health <= 0f)
			{
				return new AttackResult(0f, null);
			}
			float damage = 0f;
			if (this.Prefab.TakeLevelWallDamage)
			{
				damage += attack.GetLevelWallDamage(deltaTime);
			}
			damage = Math.Max(this.Health, damage);
			this.AddDamage(damage, deltaTime, attacker, false);
			return new AttackResult(damage, null);
		}

		// Token: 0x060027D4 RID: 10196 RVA: 0x00101D4C File Offset: 0x000FFF4C
		public void AddDamage(float damage, float deltaTime, Entity attacker, bool isNetworkEvent = false)
		{
			if (this.Health <= 0f)
			{
				return;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && !isNetworkEvent)
			{
				return;
			}
			this.tookDamage |= !MathUtils.NearlyEqual(damage, 0f, 0.0001f);
			this.Health -= damage;
			if (this.Health <= 0f)
			{
				if (this.PhysicsBody != null)
				{
					this.PhysicsBody.Enabled = false;
				}
				foreach (LevelTrigger trigger in this.Triggers)
				{
					trigger.PhysicsBody.Enabled = false;
					foreach (StatusEffect effect in trigger.StatusEffects)
					{
						if (effect.type == ActionType.OnBroken)
						{
							effect.Apply(effect.type, deltaTime, attacker, this, new Vector2?(this.WorldPosition));
						}
					}
				}
			}
		}

		// Token: 0x060027D5 RID: 10197 RVA: 0x00101E78 File Offset: 0x00100078
		public Vector2 LocalToWorld(Vector2 localPosition, float swingState = 0f)
		{
			Vector2 emitterPos = localPosition * this.Scale;
			if (this.Rotation != 0f || this.Prefab.SwingAmountRad != 0f)
			{
				float rot = this.Rotation + swingState * this.Prefab.SwingAmountRad;
				float ca = (float)Math.Cos((double)rot);
				float sa = (float)Math.Sin((double)rot);
				emitterPos = new Vector2(ca * emitterPos.X + sa * emitterPos.Y, -sa * emitterPos.X + ca * emitterPos.Y);
			}
			return new Vector2(this.Position.X, this.Position.Y) + emitterPos;
		}

		// Token: 0x060027D6 RID: 10198 RVA: 0x00101F23 File Offset: 0x00100123
		public void Remove()
		{
		}

		// Token: 0x060027D7 RID: 10199 RVA: 0x00101F25 File Offset: 0x00100125
		public override string ToString()
		{
			return "LevelObject (" + this.ActivePrefab.Name + ")";
		}

		// Token: 0x060027D8 RID: 10200 RVA: 0x00101F44 File Offset: 0x00100144
		public void ServerWrite(IWriteMessage msg, Client c)
		{
			if (this.Triggers == null)
			{
				return;
			}
			if (this.Prefab.TakeLevelWallDamage)
			{
				msg.WriteRangedSingle(MathHelper.Clamp(this.Health, 0f, this.Prefab.Health), 0f, this.Prefab.Health, 8);
			}
			for (int i = 0; i < this.Triggers.Count; i++)
			{
				if (this.Triggers[i].UseNetworkSyncing)
				{
					this.Triggers[i].ServerWrite(msg, c);
				}
			}
		}

		// Token: 0x04001382 RID: 4994
		public readonly LevelObjectPrefab Prefab;

		// Token: 0x04001384 RID: 4996
		public float NetworkUpdateTimer;

		// Token: 0x04001385 RID: 4997
		public float Scale;

		// Token: 0x04001386 RID: 4998
		public float Rotation;

		// Token: 0x04001387 RID: 4999
		private int spriteIndex;

		// Token: 0x04001388 RID: 5000
		protected bool tookDamage;

		// Token: 0x04001389 RID: 5001
		public LevelObjectPrefab ActivePrefab;

		// Token: 0x0400138F RID: 5007
		public Level.Cave ParentCave;
	}
}
