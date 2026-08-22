using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A3 RID: 1187
	internal class Projectile : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x060041D2 RID: 16850 RVA: 0x001A6898 File Offset: 0x001A4A98
		public override bool ValidateEventData(NetEntityEvent.IData data)
		{
			Projectile.EventData eventData;
			return base.TryExtractEventData<Projectile.EventData>(data, out eventData);
		}

		// Token: 0x060041D3 RID: 16851 RVA: 0x001A68B0 File Offset: 0x001A4AB0
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Projectile.EventData eventData = base.ExtractEventData<Projectile.EventData>(extraData);
			bool launch = eventData.Launch;
			msg.WriteBoolean(launch);
			if (launch)
			{
				Character user = this.User;
				msg.WriteUInt16((user != null) ? user.ID : 0);
				msg.WriteSingle(this.launchPos.X);
				msg.WriteSingle(this.launchPos.Y);
				msg.WriteSingle(this.launchRot);
				msg.WriteByte(eventData.SpreadCounter);
				Submarine launchSub = this.LaunchSub;
				msg.WriteUInt16((launchSub != null) ? launchSub.ID : 0);
			}
			bool stuck = this.StickTarget != null && !this.item.Removed && !this.StickTargetRemoved();
			msg.WriteBoolean(stuck);
			if (!stuck)
			{
				return;
			}
			Submarine submarine = this.item.Submarine;
			msg.WriteUInt16((submarine != null) ? submarine.ID : 0);
			Hull currentHull = this.item.CurrentHull;
			msg.WriteUInt16((currentHull != null) ? currentHull.ID : 0);
			msg.WriteSingle(this.item.SimPosition.X);
			msg.WriteSingle(this.item.SimPosition.Y);
			msg.WriteSingle(this.jointAxis.X);
			msg.WriteSingle(this.jointAxis.Y);
			Structure structure = this.StickTarget.UserData as Structure;
			if (structure != null)
			{
				msg.WriteByte(0);
				msg.WriteUInt16(structure.ID);
				int bodyIndex = structure.Bodies.IndexOf(this.StickTarget);
				msg.WriteByte((byte)((bodyIndex == -1) ? 0 : bodyIndex));
				return;
			}
			Item item = this.StickTarget.UserData as Item;
			if (item != null)
			{
				msg.WriteByte(2);
				msg.WriteUInt16(item.ID);
				return;
			}
			Submarine sub = this.StickTarget.UserData as Submarine;
			if (sub != null)
			{
				msg.WriteByte(3);
				msg.WriteUInt16(sub.ID);
				return;
			}
			Limb limb = this.StickTarget.UserData as Limb;
			if (limb != null)
			{
				msg.WriteByte(1);
				msg.WriteUInt16(limb.character.ID);
				msg.WriteByte((byte)Array.IndexOf<Limb>(limb.character.AnimController.Limbs, limb));
				return;
			}
			VoronoiCell cell = this.StickTarget.UserData as VoronoiCell;
			if (cell != null)
			{
				msg.WriteByte(4);
				msg.WriteInt32(Level.Loaded.GetAllCells().IndexOf(cell));
				return;
			}
			msg.WriteByte(5);
			object userData = this.StickTarget.UserData;
			throw new NotImplementedException(((userData != null) ? userData.ToString() : null) ?? "null is not a valid projectile stick target.");
		}

		// Token: 0x060041D4 RID: 16852 RVA: 0x001A6B4C File Offset: 0x001A4D4C
		static Projectile()
		{
			MTRandom random = new MTRandom(0);
			Projectile.spreadPool = (from f in Enumerable.Range(0, 256)
			select (float)random.NextDouble() - 0.5f).ToImmutableArray<float>();
		}

		// Token: 0x17001185 RID: 4485
		// (get) Token: 0x060041D5 RID: 16853 RVA: 0x001A6B91 File Offset: 0x001A4D91
		// (set) Token: 0x060041D6 RID: 16854 RVA: 0x001A6B98 File Offset: 0x001A4D98
		public static byte SpreadCounter { get; private set; }

		// Token: 0x060041D7 RID: 16855 RVA: 0x001A6BA0 File Offset: 0x001A4DA0
		public static void ResetSpreadCounter()
		{
			Projectile.SpreadCounter = 0;
		}

		// Token: 0x17001186 RID: 4486
		// (get) Token: 0x060041D8 RID: 16856 RVA: 0x001A6BA8 File Offset: 0x001A4DA8
		// (set) Token: 0x060041D9 RID: 16857 RVA: 0x001A6BB0 File Offset: 0x001A4DB0
		public Attack Attack { get; private set; }

		// Token: 0x17001187 RID: 4487
		// (get) Token: 0x060041DA RID: 16858 RVA: 0x001A6BB9 File Offset: 0x001A4DB9
		// (set) Token: 0x060041DB RID: 16859 RVA: 0x001A6BC1 File Offset: 0x001A4DC1
		public Character User
		{
			get
			{
				return this._user;
			}
			set
			{
				this._user = value;
				Attack attack = this.Attack;
				if (attack == null)
				{
					return;
				}
				attack.SetUser(this._user);
			}
		}

		// Token: 0x17001188 RID: 4488
		// (get) Token: 0x060041DC RID: 16860 RVA: 0x001A6BE0 File Offset: 0x001A4DE0
		// (set) Token: 0x060041DD RID: 16861 RVA: 0x001A6BE8 File Offset: 0x001A4DE8
		public Character Attacker { get; set; }

		// Token: 0x17001189 RID: 4489
		// (get) Token: 0x060041DE RID: 16862 RVA: 0x001A6BF1 File Offset: 0x001A4DF1
		public IEnumerable<Body> Hits
		{
			get
			{
				return this.hits;
			}
		}

		// Token: 0x1700118A RID: 4490
		// (get) Token: 0x060041DF RID: 16863 RVA: 0x001A6BF9 File Offset: 0x001A4DF9
		// (set) Token: 0x060041E0 RID: 16864 RVA: 0x001A6C01 File Offset: 0x001A4E01
		[Serialize(10f, IsPropertySaveable.No, "The impulse applied to the physics body of the item when it's launched. Higher values make the projectile faster.", "", false)]
		public float LaunchImpulse { get; set; }

		// Token: 0x1700118B RID: 4491
		// (get) Token: 0x060041E1 RID: 16865 RVA: 0x001A6C0A File Offset: 0x001A4E0A
		// (set) Token: 0x060041E2 RID: 16866 RVA: 0x001A6C12 File Offset: 0x001A4E12
		[Serialize(0f, IsPropertySaveable.No, "The random percentage modifier used to add variance to the launch impulse.", "", false)]
		public float ImpulseSpread { get; set; }

		// Token: 0x1700118C RID: 4492
		// (get) Token: 0x060041E3 RID: 16867 RVA: 0x001A6C1B File Offset: 0x001A4E1B
		// (set) Token: 0x060041E4 RID: 16868 RVA: 0x001A6C28 File Offset: 0x001A4E28
		[Serialize(0f, IsPropertySaveable.No, "The rotation of the item relative to the rotation of the weapon when launched (in degrees).", "", false)]
		public float LaunchRotation
		{
			get
			{
				return MathHelper.ToDegrees(this.LaunchRotationRadians);
			}
			set
			{
				this.LaunchRotationRadians = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x1700118D RID: 4493
		// (get) Token: 0x060041E5 RID: 16869 RVA: 0x001A6C36 File Offset: 0x001A4E36
		// (set) Token: 0x060041E6 RID: 16870 RVA: 0x001A6C3E File Offset: 0x001A4E3E
		public float LaunchRotationRadians { get; private set; }

		// Token: 0x1700118E RID: 4494
		// (get) Token: 0x060041E7 RID: 16871 RVA: 0x001A6C47 File Offset: 0x001A4E47
		// (set) Token: 0x060041E8 RID: 16872 RVA: 0x001A6C4F File Offset: 0x001A4E4F
		[Serialize(false, IsPropertySaveable.No, "When set to true, the item can stick to any target it hits.", "", false)]
		public bool DoesStick { get; set; }

		// Token: 0x1700118F RID: 4495
		// (get) Token: 0x060041E9 RID: 16873 RVA: 0x001A6C58 File Offset: 0x001A4E58
		// (set) Token: 0x060041EA RID: 16874 RVA: 0x001A6C60 File Offset: 0x001A4E60
		[Serialize(false, IsPropertySaveable.No, "Can the projectile stick to characters.", "", false)]
		public bool StickToCharacters { get; set; }

		// Token: 0x17001190 RID: 4496
		// (get) Token: 0x060041EB RID: 16875 RVA: 0x001A6C69 File Offset: 0x001A4E69
		// (set) Token: 0x060041EC RID: 16876 RVA: 0x001A6C71 File Offset: 0x001A4E71
		[Serialize(false, IsPropertySaveable.No, "Can the projectile stick to walls.", "", false)]
		public bool StickToStructures { get; set; }

		// Token: 0x17001191 RID: 4497
		// (get) Token: 0x060041ED RID: 16877 RVA: 0x001A6C7A File Offset: 0x001A4E7A
		// (set) Token: 0x060041EE RID: 16878 RVA: 0x001A6C82 File Offset: 0x001A4E82
		[Serialize(false, IsPropertySaveable.No, "Can the projectile stick to items.", "", false)]
		public bool StickToItems { get; set; }

		// Token: 0x17001192 RID: 4498
		// (get) Token: 0x060041EF RID: 16879 RVA: 0x001A6C8B File Offset: 0x001A4E8B
		// (set) Token: 0x060041F0 RID: 16880 RVA: 0x001A6C93 File Offset: 0x001A4E93
		[Serialize(false, IsPropertySaveable.No, "Can the projectile stick to doors. Caution: may cause issues.", "", false)]
		public bool StickToDoors { get; set; }

		// Token: 0x17001193 RID: 4499
		// (get) Token: 0x060041F1 RID: 16881 RVA: 0x001A6C9C File Offset: 0x001A4E9C
		// (set) Token: 0x060041F2 RID: 16882 RVA: 0x001A6CA4 File Offset: 0x001A4EA4
		[Serialize(false, IsPropertySaveable.No, "Can the item stick even to deflective targets.", "", false)]
		public bool StickToDeflective { get; set; }

		// Token: 0x17001194 RID: 4500
		// (get) Token: 0x060041F3 RID: 16883 RVA: 0x001A6CAD File Offset: 0x001A4EAD
		// (set) Token: 0x060041F4 RID: 16884 RVA: 0x001A6CB5 File Offset: 0x001A4EB5
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool StickToLightTargets { get; set; }

		// Token: 0x17001195 RID: 4501
		// (get) Token: 0x060041F5 RID: 16885 RVA: 0x001A6CBE File Offset: 0x001A4EBE
		// (set) Token: 0x060041F6 RID: 16886 RVA: 0x001A6CC6 File Offset: 0x001A4EC6
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool GoThroughLightTargets { get; set; }

		// Token: 0x17001196 RID: 4502
		// (get) Token: 0x060041F7 RID: 16887 RVA: 0x001A6CCF File Offset: 0x001A4ECF
		// (set) Token: 0x060041F8 RID: 16888 RVA: 0x001A6CD7 File Offset: 0x001A4ED7
		[Serialize(-1f, IsPropertySaveable.No, "Minimum mass of targets to stick to when StickToLightTargets is disabled. Defaults to half of the projectile's mass.", "", false)]
		public float LightTargetMassThreshold { get; set; }

		// Token: 0x17001197 RID: 4503
		// (get) Token: 0x060041F9 RID: 16889 RVA: 0x001A6CE0 File Offset: 0x001A4EE0
		// (set) Token: 0x060041FA RID: 16890 RVA: 0x001A6CE8 File Offset: 0x001A4EE8
		[Serialize(false, IsPropertySaveable.No, "Hitscan projectiles cast a ray forwards and immediately hit whatever the ray hits. It is recommended to use hitscans for very fast-moving projectiles such as bullets, because using extremely fast launch velocities may cause physics glitches.", "", false)]
		public bool Hitscan { get; set; }

		// Token: 0x17001198 RID: 4504
		// (get) Token: 0x060041FB RID: 16891 RVA: 0x001A6CF1 File Offset: 0x001A4EF1
		// (set) Token: 0x060041FC RID: 16892 RVA: 0x001A6CF9 File Offset: 0x001A4EF9
		[Serialize(1, IsPropertySaveable.No, "How many hitscans should be done when the projectile is launched. Multiple hitscans can be used to simulate weapons that fire multiple projectiles at the same time without having to actually use multiple projectile items, for example shotguns.", "", false)]
		public int HitScanCount { get; set; }

		// Token: 0x17001199 RID: 4505
		// (get) Token: 0x060041FD RID: 16893 RVA: 0x001A6D02 File Offset: 0x001A4F02
		// (set) Token: 0x060041FE RID: 16894 RVA: 0x001A6D0A File Offset: 0x001A4F0A
		[Serialize(1, IsPropertySaveable.No, "How many targets the projectile can hit before it stops.", "", false)]
		public int MaxTargetsToHit { get; set; }

		// Token: 0x1700119A RID: 4506
		// (get) Token: 0x060041FF RID: 16895 RVA: 0x001A6D13 File Offset: 0x001A4F13
		// (set) Token: 0x06004200 RID: 16896 RVA: 0x001A6D1B File Offset: 0x001A4F1B
		[Serialize(false, IsPropertySaveable.No, "Should the item be deleted when it hits something.", "", false)]
		public bool RemoveOnHit { get; set; }

		// Token: 0x1700119B RID: 4507
		// (get) Token: 0x06004201 RID: 16897 RVA: 0x001A6D24 File Offset: 0x001A4F24
		// (set) Token: 0x06004202 RID: 16898 RVA: 0x001A6D2C File Offset: 0x001A4F2C
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the launch angle of the projectile (in degrees).", "", false)]
		public float Spread { get; set; }

		// Token: 0x1700119C RID: 4508
		// (get) Token: 0x06004203 RID: 16899 RVA: 0x001A6D35 File Offset: 0x001A4F35
		// (set) Token: 0x06004204 RID: 16900 RVA: 0x001A6D3D File Offset: 0x001A4F3D
		[Serialize(false, IsPropertySaveable.No, "Override random spread with static spread; projectiles are launched with an equal amount of angle between them. Only applies when firing multiple projectiles.", "", false)]
		public bool StaticSpread { get; set; }

		// Token: 0x1700119D RID: 4509
		// (get) Token: 0x06004205 RID: 16901 RVA: 0x001A6D46 File Offset: 0x001A4F46
		// (set) Token: 0x06004206 RID: 16902 RVA: 0x001A6D4E File Offset: 0x001A4F4E
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool FriendlyFire { get; set; }

		// Token: 0x1700119E RID: 4510
		// (get) Token: 0x06004207 RID: 16903 RVA: 0x001A6D57 File Offset: 0x001A4F57
		// (set) Token: 0x06004208 RID: 16904 RVA: 0x001A6D5F File Offset: 0x001A4F5F
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float DeactivationTime { get; set; }

		// Token: 0x1700119F RID: 4511
		// (get) Token: 0x06004209 RID: 16905 RVA: 0x001A6D68 File Offset: 0x001A4F68
		// (set) Token: 0x0600420A RID: 16906 RVA: 0x001A6D70 File Offset: 0x001A4F70
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float StickDuration { get; set; }

		// Token: 0x170011A0 RID: 4512
		// (get) Token: 0x0600420B RID: 16907 RVA: 0x001A6D79 File Offset: 0x001A4F79
		// (set) Token: 0x0600420C RID: 16908 RVA: 0x001A6D81 File Offset: 0x001A4F81
		[Serialize(-1f, IsPropertySaveable.No, "", "", false)]
		public float MaxJointTranslation { get; set; }

		// Token: 0x170011A1 RID: 4513
		// (get) Token: 0x0600420D RID: 16909 RVA: 0x001A6D8A File Offset: 0x001A4F8A
		// (set) Token: 0x0600420E RID: 16910 RVA: 0x001A6D92 File Offset: 0x001A4F92
		[Serialize(1000f, IsPropertySaveable.No, "", "", false)]
		public float JointBreakPoint { get; set; }

		// Token: 0x170011A2 RID: 4514
		// (get) Token: 0x0600420F RID: 16911 RVA: 0x001A6D9B File Offset: 0x001A4F9B
		// (set) Token: 0x06004210 RID: 16912 RVA: 0x001A6DA3 File Offset: 0x001A4FA3
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool Prismatic { get; set; }

		// Token: 0x170011A3 RID: 4515
		// (get) Token: 0x06004211 RID: 16913 RVA: 0x001A6DAC File Offset: 0x001A4FAC
		// (set) Token: 0x06004212 RID: 16914 RVA: 0x001A6DB4 File Offset: 0x001A4FB4
		[Serialize(false, IsPropertySaveable.No, "Enable only if you want to make the projectile ignore collisions with other projectiles when it's shot. Doesn't have any effect, if the item is not set to be damaged by projectiles.", "", false)]
		public bool IgnoreProjectilesWhileActive { get; set; }

		// Token: 0x170011A4 RID: 4516
		// (get) Token: 0x06004213 RID: 16915 RVA: 0x001A6DBD File Offset: 0x001A4FBD
		// (set) Token: 0x06004214 RID: 16916 RVA: 0x001A6DC5 File Offset: 0x001A4FC5
		public Body StickTarget { get; private set; }

		// Token: 0x170011A5 RID: 4517
		// (get) Token: 0x06004215 RID: 16917 RVA: 0x001A6DCE File Offset: 0x001A4FCE
		// (set) Token: 0x06004216 RID: 16918 RVA: 0x001A6DD6 File Offset: 0x001A4FD6
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamageDoors { get; set; }

		// Token: 0x170011A6 RID: 4518
		// (get) Token: 0x06004217 RID: 16919 RVA: 0x001A6DDF File Offset: 0x001A4FDF
		// (set) Token: 0x06004218 RID: 16920 RVA: 0x001A6DE7 File Offset: 0x001A4FE7
		[Serialize(false, IsPropertySaveable.No, "Can the projectile hit the user? Should generally be disabled, unless the projectile is for example something like shrapnel launched by a projectile impact.", "", false)]
		public bool DamageUser { get; set; }

		// Token: 0x170011A7 RID: 4519
		// (get) Token: 0x06004219 RID: 16921 RVA: 0x001A6DF0 File Offset: 0x001A4FF0
		public bool IsStuckToTarget
		{
			get
			{
				return this.StickTarget != null;
			}
		}

		// Token: 0x0600421A RID: 16922 RVA: 0x001A6DFC File Offset: 0x001A4FFC
		public Projectile(Item item, ContentXElement element) : base(item, element)
		{
			this.IgnoredBodies = new List<Body>();
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("attack", StringComparison.OrdinalIgnoreCase))
				{
					this.Attack = new Attack(subElement, item.Name + ", Projectile", item);
				}
			}
			if (item.body == null)
			{
				DebugConsole.ThrowError("Error in projectile definition (" + item.Name + "): No body defined!", null, element.ContentPackage, false, false);
				return;
			}
			this.spreadIndex = Projectile.SpreadCounter;
			Projectile.SpreadCounter += 1;
		}

		// Token: 0x0600421B RID: 16923 RVA: 0x001A6EFC File Offset: 0x001A50FC
		public override void OnItemLoaded()
		{
			if (this.item.body == null)
			{
				return;
			}
			if (this.Attack != null && this.Attack.DamageRange <= 0f)
			{
				switch (this.item.body.BodyShape)
				{
				case PhysicsBody.Shape.Circle:
					this.Attack.DamageRange = this.item.body.Radius;
					break;
				case PhysicsBody.Shape.Rectangle:
					this.Attack.DamageRange = new Vector2(this.item.body.Width / 2f, this.item.body.Height / 2f).Length();
					break;
				case PhysicsBody.Shape.Capsule:
					this.Attack.DamageRange = this.item.body.Height / 2f + this.item.body.Radius;
					break;
				}
				this.Attack.DamageRange = ConvertUnits.ToDisplayUnits(this.Attack.DamageRange);
			}
			this.originalCollisionCategories = this.item.body.CollisionCategories;
			this.originalCollisionTargets = this.item.body.CollidesWith;
		}

		// Token: 0x0600421C RID: 16924 RVA: 0x001A703B File Offset: 0x001A523B
		public float GetSpreadFromPool()
		{
			this.spreadIndex = (byte)MathUtils.PositiveModulo((int)this.spreadIndex, Projectile.spreadPool.Length);
			return Projectile.spreadPool[(int)this.spreadIndex];
		}

		// Token: 0x0600421D RID: 16925 RVA: 0x001A706C File Offset: 0x001A526C
		private void Launch(Character user, Vector2 simPosition, float rotation, float damageMultiplier = 1f, float launchImpulseModifier = 0f)
		{
			if (base.Item.body == null)
			{
				return;
			}
			base.Item.body.ResetDynamics();
			base.Item.SetTransform(simPosition, rotation, true, true, null);
			if (this.Attack != null)
			{
				this.Attack.DamageMultiplier = damageMultiplier;
			}
			foreach (StatusEffect statusEffect in base.Item.GetStatusEffectsOfType(ActionType.OnImpact))
			{
				foreach (Explosion explosion in statusEffect.Explosions)
				{
					explosion.Attack.DamageMultiplier = damageMultiplier;
				}
			}
			this.User = user;
			this.Use(null, launchImpulseModifier);
			this.User = user;
			if (base.Item.Removed)
			{
				return;
			}
			this.launchPos = simPosition;
			this.LaunchSub = this.item.Submarine;
			base.Item.SetTransform(simPosition, this.GetLaunchRotation(rotation), false, true, null);
			if (this.DeactivationTime > 0f)
			{
				this.deactivationTimer = this.DeactivationTime;
			}
		}

		// Token: 0x0600421E RID: 16926 RVA: 0x001A71AC File Offset: 0x001A53AC
		public void Shoot(Character user, Vector2 weaponPos, Vector2 spawnPos, float rotation, List<Body> ignoredBodies, bool createNetworkEvent, float damageMultiplier = 1f, float launchImpulseModifier = 0f)
		{
			this.IgnoredBodies = ignoredBodies;
			Vector2 projectilePos = weaponPos;
			if (Submarine.PickBody(weaponPos, spawnPos, this.IgnoredBodies, new Category?(Category.Cat1 | Category.Cat6 | Category.Cat8), true, (Fixture f) => this.IgnoredBodies == null || !this.IgnoredBodies.Contains(f.Body), false) == null)
			{
				projectilePos = spawnPos;
			}
			else if ((weaponPos - spawnPos).LengthSquared() > 0.0001f)
			{
				Vector2 newPos = weaponPos - Vector2.Normalize(spawnPos - projectilePos) * Math.Max(base.Item.body.GetMaxExtent(), 0.1f);
				if (MathUtils.IsValid(newPos))
				{
					projectilePos = newPos;
				}
			}
			this.Launch(user, projectilePos, rotation, damageMultiplier, launchImpulseModifier);
			if (createNetworkEvent && !base.Item.Removed && GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				this.launchRot = rotation;
				base.Item.CreateServerEvent<Projectile>(this, new Projectile.EventData(true, this.spreadIndex - 1));
			}
		}

		// Token: 0x0600421F RID: 16927 RVA: 0x001A729C File Offset: 0x001A549C
		public bool Use(Character character = null, float launchImpulseModifier = 0f)
		{
			if (character != null && !this.characterUsable)
			{
				return false;
			}
			if (this.item.body == null)
			{
				return false;
			}
			if (this.StickTarget != null || this.IsActive)
			{
				return false;
			}
			Client owner = GameMain.Server.ConnectedClients.FirstOrDefault((Client c) => c.Character == this.User);
			if (owner != null)
			{
				Limb.SetLagCompensatedBodyPositions(owner);
			}
			float initialRotation = this.item.body.Rotation;
			if (this.item.body.Dir < 0f && !(this.item.ParentInventory is ItemInventory))
			{
				initialRotation -= 3.1415927f;
			}
			Submarine initialSubmarine = this.item.Submarine;
			for (int i = 0; i < this.HitScanCount; i++)
			{
				float launchAngle;
				if (this.StaticSpread)
				{
					launchAngle = initialRotation + MathHelper.ToRadians((float)i - (float)(this.HitScanCount - 1) / 2f) * this.Spread;
				}
				else
				{
					launchAngle = initialRotation + MathHelper.ToRadians(this.Spread * this.GetSpreadFromPool());
				}
				this.spreadIndex += 1;
				Vector2 launchDir = new Vector2((float)Math.Cos((double)launchAngle), (float)Math.Sin((double)launchAngle));
				Vector2 prevSimpos = this.item.SimPosition;
				this.item.body.SetTransformIgnoreContacts(this.item.body.SimPosition, launchAngle, true);
				this.item.Submarine = initialSubmarine;
				if (this.Hitscan)
				{
					this.DoHitscan(launchDir);
					if (i < this.HitScanCount - 1)
					{
						this.item.SetTransform(prevSimpos, this.item.body.Rotation, true, true, null);
					}
				}
				else
				{
					float modifiedLaunchImpulse = (this.LaunchImpulse + launchImpulseModifier) * (1f + Rand.Range(-this.ImpulseSpread, this.ImpulseSpread, Rand.RandSync.Unsynced));
					this.DoLaunch(launchDir * modifiedLaunchImpulse);
					this.item.SetTransform(this.item.body.SimPosition, this.GetLaunchRotation(launchAngle), false, true, null);
				}
			}
			this.User = character;
			base.ApplyStatusEffects(ActionType.OnUse, 1f, this.User, null, null, this.User, null, 1f);
			return true;
		}

		// Token: 0x06004220 RID: 16928 RVA: 0x001A74D0 File Offset: 0x001A56D0
		private float GetLaunchRotation(float unmodifiedLaunchRotation)
		{
			float launchRotation = unmodifiedLaunchRotation + this.item.body.Dir * this.LaunchRotationRadians;
			if (this.item.body.Dir < 0f)
			{
				launchRotation -= 3.1415927f;
			}
			return launchRotation;
		}

		// Token: 0x06004221 RID: 16929 RVA: 0x001A7517 File Offset: 0x001A5717
		public override bool Use(float deltaTime, Character character = null)
		{
			return this.Use(character, 0f);
		}

		// Token: 0x06004222 RID: 16930 RVA: 0x001A7528 File Offset: 0x001A5728
		private void DoLaunch(Vector2 impulse)
		{
			this.hits.Clear();
			Vector2 prevVelocity = this.item.body.LinearVelocity;
			if (this.item.AiTarget != null)
			{
				this.item.AiTarget.SightRange = this.item.AiTarget.MaxSightRange;
				this.item.AiTarget.SoundRange = this.item.AiTarget.MaxSoundRange;
			}
			Inventory prevInventory = this.item.ParentInventory;
			if (prevInventory != null)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsServer)
				{
					CoroutineManager.Invoke(delegate
					{
						if (this.item.Removed)
						{
							return;
						}
						prevInventory.CreateNetworkEvent();
					}, 0.5f);
				}
			}
			this.item.Drop(null, false, true);
			base.Item.WaterDragCoefficient = 0.1f;
			this.launchPos = this.item.SimPosition;
			this.LaunchSub = this.item.Submarine;
			this.item.body.Enabled = true;
			if (this.item.body.BodyType == BodyType.Kinematic)
			{
				this.item.body.LinearVelocity = impulse;
			}
			else if (impulse.LengthSquared() > 0.001f)
			{
				impulse *= this.item.body.Mass;
				this.item.body.ApplyLinearImpulse(impulse, 60.8f);
			}
			else
			{
				this.item.body.LinearVelocity = prevVelocity;
			}
			this.item.body.FarseerBody.OnCollision += this.OnProjectileCollision;
			this.item.body.FarseerBody.IsBullet = true;
			this.EnableProjectileCollisions();
			this.IsActive = true;
			if (this.stickJoint == null)
			{
				return;
			}
			this.StickTarget = null;
			GameMain.World.Remove(this.stickJoint);
			this.stickJoint = null;
		}

		// Token: 0x06004223 RID: 16931 RVA: 0x001A7720 File Offset: 0x001A5920
		private void DoHitscan(Vector2 dir)
		{
			Projectile.<>c__DisplayClass173_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			float rotation = this.item.body.Rotation;
			Vector2 simPositon = this.item.SimPosition;
			Vector2 rayStartWorld = this.item.WorldPosition;
			this.item.Drop(null, false, true);
			base.Item.WaterDragCoefficient = 0.1f;
			this.item.body.Enabled = true;
			this.item.body.LinearVelocity = dir;
			this.IsActive = true;
			Vector2 rayStart = simPositon;
			Vector2 rayEnd = rayStart + dir * 500f;
			float worldDist = 1000f;
			Vector2 rayEndWorld = rayStartWorld + dir * worldDist;
			CS$<>8__locals1.hits = new List<Projectile.HitscanResult>();
			CS$<>8__locals1.hits.AddRange(this.DoRayCast(rayStart, rayEnd, this.item.Submarine));
			if (this.item.Submarine != null)
			{
				CS$<>8__locals1.hits.AddRange(this.DoRayCast(rayStart + this.item.Submarine.SimPosition, rayEnd + this.item.Submarine.SimPosition, null));
				this.<DoHitscan>g__RayCastInOtherSubs|173_0(rayStart + this.item.Submarine.SimPosition, rayEnd + this.item.Submarine.SimPosition, ref CS$<>8__locals1);
			}
			else
			{
				this.<DoHitscan>g__RayCastInOtherSubs|173_0(rayStart, rayEnd, ref CS$<>8__locals1);
			}
			int hitCount = 0;
			Vector2 lastHitPos = this.item.WorldPosition;
			CS$<>8__locals1.hits = (from h in CS$<>8__locals1.hits
			orderby h.Fraction
			select h).ToList<Projectile.HitscanResult>();
			for (int i = 0; i < CS$<>8__locals1.hits.Count; i++)
			{
				Projectile.HitscanResult h2 = CS$<>8__locals1.hits[i];
				this.item.SetTransform(h2.Point, rotation, true, true, h2.Submarine);
				this.item.UpdateTransform();
				if (this.HandleProjectileCollision(h2.Fixture, h2.Normal, Vector2.Zero))
				{
					hitCount++;
					if (hitCount >= this.MaxTargetsToHit || i == CS$<>8__locals1.hits.Count - 1)
					{
						break;
					}
				}
			}
			if (hitCount < this.MaxTargetsToHit)
			{
				this.item.body.SetTransformIgnoreContacts(this.item.body.SimPosition, rotation, true);
				if (Entity.Spawner == null)
				{
					this.item.Remove();
					return;
				}
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					this.item.HiddenInGame = this.Hitscan;
					return;
				}
				Entity.Spawner.AddItemToRemoveQueue(this.item);
			}
		}

		// Token: 0x06004224 RID: 16932 RVA: 0x001A79DC File Offset: 0x001A5BDC
		private List<Projectile.HitscanResult> DoRayCast(Vector2 rayStart, Vector2 rayEnd, Submarine submarine)
		{
			List<Projectile.HitscanResult> hits = new List<Projectile.HitscanResult>();
			Vector2 dir = rayEnd - rayStart;
			dir = ((dir.LengthSquared() < 1E-05f) ? Vector2.UnitY : Vector2.Normalize(dir));
			AABB aabb = new AABB(rayStart - Vector2.One * 0.001f, rayStart + Vector2.One * 0.001f);
			GameMain.World.QueryAABB(delegate(Fixture fixture)
			{
				LevelObject levelObj = ((fixture != null) ? fixture.Body.UserData : null) as LevelObject;
				if (levelObj != null)
				{
					if (!levelObj.Prefab.TakeLevelWallDamage)
					{
						return true;
					}
				}
				else if (((fixture != null) ? fixture.Body : null) == null || fixture.IsSensor)
				{
					return true;
				}
				if (fixture.Body.UserData is VineTile)
				{
					return true;
				}
				if (fixture.CollidesWith == Category.None)
				{
					return true;
				}
				if (fixture.Body.UserData as string == "ruinroom" || fixture.Body.UserData is Hull || fixture.UserData is Hull)
				{
					return true;
				}
				if (submarine != null)
				{
					if (fixture.Body.UserData is VoronoiCell)
					{
						return true;
					}
					Entity entity = fixture.Body.UserData as Entity;
					if (entity != null && entity.Submarine != submarine)
					{
						return true;
					}
				}
				if (fixture.Body.UserData is VoronoiCell && (this.item.Submarine != null || submarine != null))
				{
					return true;
				}
				Item item = fixture.Body.UserData as Item;
				if (item != null)
				{
					if (item == this.Item)
					{
						return true;
					}
					if (item.Condition <= 0f)
					{
						return true;
					}
					if (!item.Prefab.DamagedByProjectiles && item.GetComponent<Door>() == null)
					{
						return true;
					}
				}
				else
				{
					if (fixture.Body.UserData is Gap)
					{
						return true;
					}
					Holdable holdable = fixture.Body.UserData as Holdable;
					if (holdable != null && !holdable.CanPush)
					{
						return true;
					}
					if (!fixture.CollisionCategories.HasFlag(Category.Cat2) && !fixture.CollisionCategories.HasFlag(Category.Cat1) && !fixture.CollisionCategories.HasFlag(Category.Cat8))
					{
						return true;
					}
				}
				Transform transform;
				fixture.Body.GetTransform(out transform);
				if (!fixture.Shape.TestPoint(ref transform, ref rayStart))
				{
					return true;
				}
				hits.Add(new Projectile.HitscanResult(fixture, rayStart, -dir, 0f, submarine));
				return true;
			}, ref aabb);
			GameMain.World.RayCast(delegate(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
			{
				LevelObject levelObj = ((fixture != null) ? fixture.Body.UserData : null) as LevelObject;
				if (levelObj != null)
				{
					if (!levelObj.Prefab.TakeLevelWallDamage)
					{
						return -1f;
					}
				}
				else if (((fixture != null) ? fixture.Body : null) == null || fixture.IsSensor)
				{
					return -1f;
				}
				if (fixture.Body.UserData is VineTile)
				{
					return -1f;
				}
				if (fixture.CollidesWith == Category.None && fixture.CollisionCategories != Category.Cat10)
				{
					return -1f;
				}
				Item item = fixture.Body.UserData as Item;
				if (item != null)
				{
					if (item.Condition <= 0f)
					{
						return -1f;
					}
					if (!item.Prefab.DamagedByProjectiles && item.GetComponent<Door>() == null)
					{
						return -1f;
					}
				}
				else if (fixture.Body.UserData is Gap)
				{
					return -1f;
				}
				if (!(fixture.Body.UserData as string == "ruinroom"))
				{
					Body body = fixture.Body;
					if (!(((body != null) ? body.UserData : null) is Hull) && !(fixture.UserData is Hull))
					{
						if (submarine != null)
						{
							if (fixture.Body.UserData is VoronoiCell)
							{
								return -1f;
							}
							Entity entity = fixture.Body.UserData as Entity;
							if (entity != null && entity.Submarine != submarine)
							{
								return -1f;
							}
							Limb limb = fixture.Body.UserData as Limb;
							if (limb != null)
							{
								Character character = limb.character;
								if (((character != null) ? character.Submarine : null) != submarine)
								{
									return -1f;
								}
							}
							Body body2 = fixture.Body;
							Level loaded = Level.Loaded;
							if (body2 != ((loaded != null) ? loaded.TopBarrier : null))
							{
								Body body3 = fixture.Body;
								Level loaded2 = Level.Loaded;
								if (body3 != ((loaded2 != null) ? loaded2.BottomBarrier : null))
								{
									goto IL_1D5;
								}
							}
							return -1f;
						}
						IL_1D5:
						Holdable holdable = fixture.Body.UserData as Holdable;
						if (holdable != null && !holdable.CanPush)
						{
							return -1f;
						}
						if (fixture.Body.UserData is VoronoiCell && Hull.FindHull(ConvertUnits.ToDisplayUnits(point), this.item.CurrentHull, true, true) != null && this.item.Submarine != null)
						{
							return -1f;
						}
						if (hits.Count > 50)
						{
							float furthestHit = 0f;
							int furthestHitIndex = -1;
							for (int i = 0; i < hits.Count; i++)
							{
								if (hits[i].Fraction > furthestHit)
								{
									furthestHitIndex = i;
									furthestHit = hits[i].Fraction;
								}
							}
							if (furthestHitIndex > -1)
							{
								hits.RemoveAt(furthestHitIndex);
							}
						}
						hits.Add(new Projectile.HitscanResult(fixture, point, normal, fraction, submarine));
						return 1f;
					}
				}
				return -1f;
			}, rayStart, rayEnd, Category.Cat1 | Category.Cat2 | Category.Cat6 | Category.Cat7 | Category.Cat8 | Category.Cat10);
			return hits;
		}

		// Token: 0x06004225 RID: 16933 RVA: 0x001A7AC6 File Offset: 0x001A5CC6
		public override void Drop(Character dropper, bool setTransform = true)
		{
			base.Item.ResetWaterDragCoefficient();
			if (dropper != null)
			{
				this.DisableProjectileCollisions();
				this.Unstick();
			}
			base.Drop(dropper, setTransform);
		}

		// Token: 0x06004226 RID: 16934 RVA: 0x001A7AEC File Offset: 0x001A5CEC
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.DeactivationTime > 0f)
			{
				this.deactivationTimer -= deltaTime;
				if (this.deactivationTimer < 0f)
				{
					this.DisableProjectileCollisions();
				}
			}
			while (this.impactQueue.Count > 0)
			{
				Projectile.Impact impact = this.impactQueue.Dequeue();
				this.HandleProjectileCollision(impact.Fixture, impact.Normal, impact.LinearVelocity);
			}
			if (!this.removePending)
			{
				Fixture fixture = this.lastTarget;
				Limb limb = ((fixture != null) ? fixture.Body.UserData : null) as Limb;
				Entity entity;
				if (limb == null)
				{
					Fixture fixture2 = this.lastTarget;
					entity = (((fixture2 != null) ? fixture2.Body.UserData : null) as Entity);
				}
				else
				{
					entity = limb.character;
				}
				Entity useTarget = entity;
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, useTarget, this._user, null, 1f);
			}
			if (this.item.body != null && this.item.body.FarseerBody.IsBullet && this.item.body.LinearVelocity.LengthSquared() < 25f)
			{
				this.item.body.FarseerBody.IsBullet = false;
			}
			if (this.stickJoint == null && !this.item.body.FarseerBody.IsBullet)
			{
				this.IsActive = false;
				if (this.DeactivationTime > 0f && this.deactivationTimer > 0f)
				{
					this.DisableProjectileCollisions();
				}
			}
			if (this.stickJoint == null)
			{
				return;
			}
			if (this.StickDuration > 0f && this.stickTimer > 0f)
			{
				this.stickTimer -= deltaTime;
				return;
			}
			float absoluteMaxTranslation = 100f;
			Body stickTarget = this.StickTarget;
			Limb target = ((stickTarget != null) ? stickTarget.UserData : null) as Limb;
			if (target == null || target.Submarine == this.item.Submarine)
			{
				PrismaticJoint prismaticJoint = this.stickJoint as PrismaticJoint;
				if (prismaticJoint == null || Math.Abs(prismaticJoint.JointTranslation) <= absoluteMaxTranslation)
				{
					goto IL_203;
				}
			}
			this.item.UpdateTransform();
			IL_203:
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				if (!this.StickTargetRemoved())
				{
					PrismaticJoint pJoint = this.stickJoint as PrismaticJoint;
					if ((pJoint == null || Math.Abs(pJoint.JointTranslation) <= this.maxJointTranslationInSimUnits) && this.stickJoint.Enabled)
					{
						return;
					}
				}
				this.Unstick();
				this.item.CreateServerEvent<Projectile>(this, new Projectile.EventData(false, 0));
			}
		}

		// Token: 0x06004227 RID: 16935 RVA: 0x001A7D68 File Offset: 0x001A5F68
		private bool StickTargetRemoved()
		{
			if (this.StickTarget == null)
			{
				return true;
			}
			Limb limb = this.StickTarget.UserData as Limb;
			if (limb != null)
			{
				return limb.character.Removed;
			}
			Entity entity = this.StickTarget.UserData as Entity;
			return entity != null && entity.Removed;
		}

		// Token: 0x06004228 RID: 16936 RVA: 0x001A7DBC File Offset: 0x001A5FBC
		private bool OnProjectileCollision(Fixture f1, Fixture target, Contact contact)
		{
			if (this.User != null && this.User.Removed)
			{
				this.User = null;
				return false;
			}
			if (this.IgnoredBodies != null && this.IgnoredBodies.Contains(target.Body))
			{
				return false;
			}
			if (this.originalCollisionCategories == Category.None && this.originalCollisionTargets == Category.None)
			{
				return false;
			}
			if (target.CollisionCategories == Category.Cat2 && target.Body.UserData is Character)
			{
				return false;
			}
			if (this.GoThroughLightTargets && target.Body.Mass < this.GetLightTargetMassThreshold())
			{
				return false;
			}
			if (target.IsSensor)
			{
				return false;
			}
			if (this.hits.Contains(target.Body))
			{
				return false;
			}
			if (target.Body.UserData is Submarine)
			{
				if (this.ShouldIgnoreSubmarineCollision(ref target, contact))
				{
					return false;
				}
			}
			else
			{
				Limb limb = target.Body.UserData as Limb;
				if (limb != null)
				{
					if (limb.IsSevered)
					{
						PhysicsBody body = limb.body;
						if (body != null)
						{
							body.ApplyLinearImpulse(this.item.body.LinearVelocity * this.item.body.Mass * 0.1f, this.item.SimPosition);
						}
						return false;
					}
					if (this.ShouldIgnoreCharacterCollision(limb.character))
					{
						return false;
					}
				}
				else
				{
					Item item = target.Body.UserData as Item;
					if (item != null)
					{
						if (item.Condition <= 0f)
						{
							return false;
						}
						if (!item.Prefab.DamagedByProjectiles && item.GetComponent<Door>() == null)
						{
							return false;
						}
					}
					else
					{
						Holdable holdable = target.Body.UserData as Holdable;
						if (holdable != null && !holdable.CanPush)
						{
							return false;
						}
					}
				}
			}
			if (target.CollisionCategories == Category.Cat2 && target.Body.UserData is Character)
			{
				return false;
			}
			this.hits.Add(target.Body);
			this.impactQueue.Enqueue(new Projectile.Impact(target, contact.Manifold.LocalNormal, this.item.body.LinearVelocity));
			this.IsActive = true;
			if (this.RemoveOnHit)
			{
				this.item.body.FarseerBody.ResetDynamics();
			}
			if (this.hits.Count >= this.MaxTargetsToHit || target.Body.UserData is VoronoiCell)
			{
				this.DisableProjectileCollisions();
				return true;
			}
			return false;
		}

		// Token: 0x06004229 RID: 16937 RVA: 0x001A8010 File Offset: 0x001A6210
		private bool ShouldIgnoreCharacterCollision(Character character)
		{
			Controller controller = this.item.GetComponent<Controller>();
			return (controller != null && controller.User == character && controller.IsAttachedUser(controller.User)) || (!this.FriendlyFire && this.User != null && character.IsFriendly(this.User));
		}

		// Token: 0x0600422A RID: 16938 RVA: 0x001A8066 File Offset: 0x001A6266
		public bool ShouldIgnoreSubmarineCollision(Fixture target, Contact contact)
		{
			return this.ShouldIgnoreSubmarineCollision(ref target, contact);
		}

		// Token: 0x0600422B RID: 16939 RVA: 0x001A8074 File Offset: 0x001A6274
		private bool ShouldIgnoreSubmarineCollision(ref Fixture target, Contact contact)
		{
			if (this.item.body.CollisionCategories != Category.Cat7)
			{
				return false;
			}
			Submarine sub = target.Body.UserData as Submarine;
			if (sub != null)
			{
				Item launcher = this.Launcher;
				if (((launcher != null) ? launcher.Submarine : null) != sub && target.UserData is Item)
				{
					return false;
				}
				Vector2 normalizedVel;
				Vector2 dir;
				if (this.item.body.LinearVelocity.LengthSquared() < 0.001f)
				{
					normalizedVel = Vector2.Zero;
					dir = contact.Manifold.LocalNormal;
				}
				else
				{
					dir = (normalizedVel = Vector2.Normalize(this.item.body.LinearVelocity));
				}
				Body wallBody = Submarine.PickBody(this.item.body.SimPosition - ConvertUnits.ToSimUnits(sub.Position) - dir, this.item.body.SimPosition - ConvertUnits.ToSimUnits(sub.Position) + dir, null, new Category?(Category.Cat1), true, (Fixture f) => this.IgnoredBodies == null || !this.IgnoredBodies.Contains(f.Body), false);
				Vector2 launchPosInCurrentCoordinateSpace = this.launchPos;
				if (this.item.body.Submarine == null && this.LaunchSub != null)
				{
					launchPosInCurrentCoordinateSpace += ConvertUnits.ToSimUnits(this.LaunchSub.Position);
				}
				bool flag;
				if (wallBody == null)
				{
					flag = (null != null);
				}
				else
				{
					List<Fixture> fixtureList = wallBody.FixtureList;
					flag = (((fixtureList != null) ? fixtureList.First<Fixture>() : null) != null);
				}
				if (!flag || (!(wallBody.UserData is Structure) && !(wallBody.UserData is Item)) || Vector2.Dot(this.item.body.SimPosition + normalizedVel - launchPosInCurrentCoordinateSpace, dir) <= 0f)
				{
					return true;
				}
				target = wallBody.FixtureList.First<Fixture>();
				if (this.hits.Contains(target.Body))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600422C RID: 16940 RVA: 0x001A8248 File Offset: 0x001A6448
		private bool HandleProjectileCollision(Fixture target, Vector2 collisionNormal, Vector2 velocity)
		{
			if (this.User != null && this.User.Removed)
			{
				this.User = null;
			}
			if (this.IgnoredBodies != null && this.IgnoredBodies.Contains(target.Body))
			{
				return false;
			}
			if (target.CollisionCategories == Category.Cat2 && target.Body.UserData is Character)
			{
				return false;
			}
			this.lastTarget = target;
			int remainingHits = Math.Max(this.MaxTargetsToHit - this.hits.Count, 0);
			float speedMultiplier = Math.Min(0.4f + (float)remainingHits * 0.1f, 1f);
			float deflectedSpeedMultiplier = 0.1f;
			AttackResult attackResult = default(AttackResult);
			Character character = null;
			Submarine submarine = target.Body.UserData as Submarine;
			if (submarine != null && !(target.UserData is Item))
			{
				this.item.Move(-submarine.Position, false);
				this.item.Submarine = submarine;
				this.item.body.Submarine = submarine;
				return !this.Hitscan;
			}
			Limb limb = target.Body.UserData as Limb;
			if (limb != null)
			{
				if (this.MaxTargetsToHit > 1)
				{
					speedMultiplier = 1f;
					deflectedSpeedMultiplier = 0.8f;
				}
				if (limb.IsSevered || limb.character == null || limb.character.Removed)
				{
					return false;
				}
				if (this.ShouldIgnoreCharacterCollision(limb.character))
				{
					return false;
				}
				limb.character.LastDamageSource = this.item;
				if (this.Attack != null)
				{
					attackResult = this.Attack.DoDamageToLimb(this.User ?? this.Attacker, limb, this.item.WorldPosition, 1f, true, null, null);
				}
				if (limb.character != null)
				{
					character = limb.character;
				}
			}
			else
			{
				Item item;
				if ((item = (target.Body.UserData as Item)) == null)
				{
					ItemComponent itemComponent = target.Body.UserData as ItemComponent;
					item = (((itemComponent != null) ? itemComponent.Item : null) ?? (target.UserData as Item));
				}
				Item targetItem = item;
				if (targetItem != null)
				{
					if (targetItem.Removed)
					{
						return false;
					}
					if (target.UserData is Item && targetItem.Submarine != null)
					{
						Submarine submarine2 = targetItem.Submarine;
						Item launcher = this.Launcher;
						if (submarine2 == ((launcher != null) ? launcher.Submarine : null))
						{
							return false;
						}
					}
					if (this.Attack != null && (targetItem.Prefab.DamagedByProjectiles || (this.DamageDoors && targetItem.GetComponent<Door>() != null)) && targetItem.Condition > 0f)
					{
						attackResult = this.Attack.DoDamage(this.User ?? this.Attacker, targetItem, this.item.WorldPosition, 1f, true, null, null);
					}
				}
				else
				{
					IDamageable damageable = target.Body.UserData as IDamageable;
					if (damageable != null)
					{
						if (this.Attack != null)
						{
							if (this.item.Submarine == null)
							{
								Structure structure = damageable as Structure;
								if (structure != null && structure.Submarine != null && Vector2.DistanceSquared(this.item.WorldPosition, structure.WorldPosition) > 100000000f)
								{
									this.item.Submarine = structure.Submarine;
								}
							}
							Vector2 pos = this.item.WorldPosition;
							attackResult = this.Attack.DoDamage(this.User ?? this.Attacker, damageable, pos, 1f, true, null, null);
						}
					}
					else
					{
						VoronoiCell voronoiCell = target.Body.UserData as VoronoiCell;
						if (voronoiCell != null && voronoiCell.IsDestructible && this.Attack != null && Math.Abs(this.Attack.LevelWallDamage) > 0f)
						{
							Level loaded = Level.Loaded;
							DestructibleLevelWall destructibleWall = ((loaded != null) ? loaded.ExtraWalls.Find((LevelWall w) => w.Body == target.Body) : null) as DestructibleLevelWall;
							if (destructibleWall != null)
							{
								attackResult = this.Attack.DoDamage(this.User ?? this.Attacker, destructibleWall, this.item.WorldPosition, 1f, true, null, null);
							}
						}
					}
				}
			}
			if (character != null)
			{
				character.LastDamageSource = this.item;
			}
			ActionType conditionalActionType = ActionType.OnSuccess;
			if (this.User != null && Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) > base.DegreeOfSuccess(this.User))
			{
				conditionalActionType = ActionType.OnFailure;
			}
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				Limb targetLimb = target.Body.UserData as Limb;
				if (targetLimb != null)
				{
					base.ApplyStatusEffects(conditionalActionType, 1f, character, targetLimb, character, this.User, null, 1f);
					base.ApplyStatusEffects(ActionType.OnImpact, 1f, character, targetLimb, character, this.User, null, 1f);
					Attack attack = targetLimb.attack;
					if (attack != null)
					{
						foreach (StatusEffect effect in attack.StatusEffects)
						{
							if (effect.type == ActionType.OnImpact)
							{
								if (effect.HasTargetType(StatusEffect.TargetType.This))
								{
									effect.Apply(effect.type, 1f, this.User, this.User, null);
								}
								if (effect.HasTargetType(StatusEffect.TargetType.Character) || effect.HasTargetType(StatusEffect.TargetType.UseTarget))
								{
									effect.Apply(effect.type, 1f, targetLimb.character, targetLimb.character, null);
								}
								if (effect.HasTargetType(StatusEffect.TargetType.Limb))
								{
									effect.Apply(effect.type, 1f, targetLimb.character, targetLimb, null);
								}
								if (effect.HasTargetType(StatusEffect.TargetType.NearbyItems) || effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
								{
									this.targets.Clear();
									effect.AddNearbyTargets(targetLimb.WorldPosition, this.targets);
									effect.Apply(effect.type, 1f, targetLimb.character, this.targets, null);
								}
							}
						}
					}
					NetworkMember server = GameMain.NetworkMember;
					if (server != null && server.IsServer)
					{
						server.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(conditionalActionType, this, targetLimb.character, targetLimb, targetLimb.character, new Vector2?(this.item.WorldPosition)));
						server.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(ActionType.OnImpact, this, targetLimb.character, targetLimb, targetLimb.character, new Vector2?(this.item.WorldPosition)));
					}
				}
				else
				{
					base.ApplyStatusEffects(conditionalActionType, 1f, null, null, target.Body.UserData as Entity, this.User, null, 1f);
					base.ApplyStatusEffects(ActionType.OnImpact, 1f, null, null, target.Body.UserData as Entity, this.User, null, 1f);
					NetworkMember server2 = GameMain.NetworkMember;
					if (server2 != null && server2.IsServer)
					{
						server2.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(conditionalActionType, this, null, null, target.Body.UserData as Entity, new Vector2?(this.item.WorldPosition)));
						server2.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(ActionType.OnImpact, this, null, null, target.Body.UserData as Entity, new Vector2?(this.item.WorldPosition)));
					}
				}
			}
			target.Body.ApplyLinearImpulse(velocity * this.item.body.Mass);
			target.Body.LinearVelocity = target.Body.LinearVelocity.ClampLength(32f);
			if (this.hits.Count < this.MaxTargetsToHit)
			{
				Body body = this.hits.LastOrDefault<Body>();
				if (!(((body != null) ? body.UserData : null) is VoronoiCell))
				{
					goto IL_8B5;
				}
			}
			this.DisableProjectileCollisions();
			IL_8B5:
			if (attackResult.AppliedDamageModifiers != null)
			{
				if (attackResult.AppliedDamageModifiers.Any((DamageModifier dm) => dm.DeflectProjectiles) && !this.StickToDeflective)
				{
					this.item.body.LinearVelocity *= deflectedSpeedMultiplier;
					goto IL_AFD;
				}
			}
			if (remainingHits > 0 || this.stickJoint != null || this.StickTarget != null || !this.StickToStructures || !(target.Body.UserData is Structure))
			{
				if (this.StickToLightTargets || target.Body.Mass >= this.GetLightTargetMassThreshold())
				{
					if (this.DoesStick || (this.StickToCharacters && (target.Body.UserData is Limb || target.Body.UserData is Character)))
					{
						goto IL_9E5;
					}
					Item i = target.Body.UserData as Item;
					if (i != null && ((i.GetComponent<Door>() != null) ? this.StickToDoors : this.StickToItems))
					{
						goto IL_9E5;
					}
				}
				this.item.body.LinearVelocity *= speedMultiplier;
				goto IL_AFD;
			}
			IL_9E5:
			Vector2 dir = new Vector2((float)Math.Cos((double)this.item.body.Rotation), (float)Math.Sin((double)this.item.body.Rotation));
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				Structure structure2 = target.Body.UserData as Structure;
				if (structure2 != null && structure2.Submarine != this.item.Submarine && structure2.Submarine != null)
				{
					this.StickToTarget(structure2.Submarine.PhysicsBody.FarseerBody, dir);
				}
				else
				{
					this.StickToTarget(target.Body, dir);
				}
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				this.item.CreateServerEvent<Projectile>(this, new Projectile.EventData(false, 0));
			}
			this.item.body.LinearVelocity *= speedMultiplier;
			return this.Hitscan;
			IL_AFD:
			ItemInventory ownInventory = this.item.OwnInventory;
			IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
			if (containedItems != null)
			{
				foreach (Item contained in containedItems)
				{
					if (contained.body != null)
					{
						contained.SetTransform(this.item.SimPosition, contained.body.Rotation, true, true, null);
					}
				}
			}
			if (this.RemoveOnHit)
			{
				this.removePending = true;
				this.item.HiddenInGame = true;
				this.item.body.FarseerBody.Enabled = false;
				CoroutineManager.Invoke(delegate
				{
					if (this.item.Removed)
					{
						return;
					}
					EntitySpawner spawner = Entity.Spawner;
					if (spawner == null)
					{
						return;
					}
					spawner.AddItemToRemoveQueue(this.item);
				}, 0.5f);
			}
			return true;
		}

		// Token: 0x0600422D RID: 16941 RVA: 0x001A8E44 File Offset: 0x001A7044
		private float GetLightTargetMassThreshold()
		{
			if (this.LightTargetMassThreshold >= 0f)
			{
				return this.LightTargetMassThreshold;
			}
			return this.item.body.Mass * 0.5f;
		}

		// Token: 0x0600422E RID: 16942 RVA: 0x001A8E70 File Offset: 0x001A7070
		private void EnableProjectileCollisions()
		{
			if (this.item.body.CollisionCategories != Category.None)
			{
				this.item.body.CollisionCategories = Category.Cat7;
				this.item.body.CollidesWith = (Category.Cat1 | Category.Cat2 | Category.Cat6 | Category.Cat8);
			}
			if (this.item.Prefab.DamagedByProjectiles && !this.IgnoreProjectilesWhileActive)
			{
				if (this.item.body.CollisionCategories == Category.None)
				{
					this.item.body.CollisionCategories = Category.Cat2;
				}
				this.item.body.CollidesWith |= Category.Cat7;
			}
		}

		// Token: 0x0600422F RID: 16943 RVA: 0x001A8F0C File Offset: 0x001A710C
		private void DisableProjectileCollisions()
		{
			Item item = this.item;
			bool flag;
			if (item == null)
			{
				flag = (null != null);
			}
			else
			{
				PhysicsBody body = item.body;
				flag = (((body != null) ? body.FarseerBody : null) != null);
			}
			if (!flag)
			{
				return;
			}
			this.item.body.FarseerBody.OnCollision -= this.OnProjectileCollision;
			if (this.originalCollisionCategories != Category.None && this.originalCollisionTargets != Category.None)
			{
				this.item.body.CollisionCategories = this.originalCollisionCategories;
				this.item.body.CollidesWith = this.originalCollisionTargets;
			}
			else if ((this.item.Prefab.DamagedByProjectiles || this.item.Prefab.DamagedByMeleeWeapons) && this.item.Condition > 0f)
			{
				this.item.body.CollisionCategories = Category.Cat2;
				this.item.body.CollidesWith = (Category.Cat1 | Category.Cat3 | Category.Cat7 | Category.Cat8);
			}
			else
			{
				this.item.body.CollisionCategories = Category.Cat5;
				this.item.body.CollidesWith = (Category.Cat1 | Category.Cat8);
			}
			List<Body> ignoredBodies = this.IgnoredBodies;
			if (ignoredBodies == null)
			{
				return;
			}
			ignoredBodies.Clear();
		}

		// Token: 0x06004230 RID: 16944 RVA: 0x001A9031 File Offset: 0x001A7231
		public bool IsAttachedTo(PhysicsBody body)
		{
			return this.stickJoint != null && (this.stickJoint.BodyA == ((body != null) ? body.FarseerBody : null) || this.stickJoint.BodyB == ((body != null) ? body.FarseerBody : null));
		}

		// Token: 0x06004231 RID: 16945 RVA: 0x001A9074 File Offset: 0x001A7274
		private void StickToTarget(Body targetBody, Vector2 axis)
		{
			if (this.stickJoint != null)
			{
				return;
			}
			this.jointAxis = axis;
			this.item.body.ResetDynamics();
			if (this.Prismatic)
			{
				this.stickJoint = new PrismaticJoint(targetBody, this.item.body.FarseerBody, this.item.body.SimPosition, axis, true)
				{
					MotorEnabled = true,
					MaxMotorForce = 30f,
					LimitEnabled = true,
					Breakpoint = this.JointBreakPoint
				};
				if (this.maxJointTranslationInSimUnits == -1f)
				{
					if (this.item.Sprite != null && this.MaxJointTranslation < 0f)
					{
						this.MaxJointTranslation = this.item.Sprite.size.X / 2f * this.item.Scale;
					}
					this.MaxJointTranslation = Math.Min(this.MaxJointTranslation, 1000f);
					this.maxJointTranslationInSimUnits = ConvertUnits.ToSimUnits(this.MaxJointTranslation);
				}
			}
			else
			{
				this.stickJoint = new WeldJoint(targetBody, this.item.body.FarseerBody, this.item.body.SimPosition, this.item.body.SimPosition, true)
				{
					FrequencyHz = 10f,
					DampingRatio = 0.5f
				};
			}
			this.stickTimer = this.StickDuration;
			this.StickTarget = targetBody;
			GameMain.World.Add(this.stickJoint);
			this.IsActive = true;
			Limb limb = targetBody.UserData as Limb;
			if (limb != null)
			{
				this.stickTargetCharacter = limb.character;
				this.stickTargetCharacter.AttachedProjectiles.Add(this);
			}
		}

		// Token: 0x06004232 RID: 16946 RVA: 0x001A922C File Offset: 0x001A742C
		public void Unstick()
		{
			this.StickTarget = null;
			if (this.stickJoint != null)
			{
				if (GameMain.World.JointList.Contains(this.stickJoint))
				{
					GameMain.World.Remove(this.stickJoint);
				}
				this.stickJoint = null;
			}
			if (!this.item.body.FarseerBody.IsBullet)
			{
				this.IsActive = false;
				if (this.DeactivationTime > 0f && this.deactivationTimer > 0f)
				{
					this.DisableProjectileCollisions();
				}
			}
			Rope component = this.item.GetComponent<Rope>();
			if (component != null)
			{
				component.Snap();
			}
			if (this.stickTargetCharacter != null)
			{
				this.stickTargetCharacter.AttachedProjectiles.Remove(this);
				this.stickTargetCharacter = null;
			}
		}

		// Token: 0x06004233 RID: 16947 RVA: 0x001A92EB File Offset: 0x001A74EB
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			if (this.IsStuckToTarget || this.stickJoint != null || this.stickTargetCharacter != null)
			{
				this.Unstick();
			}
		}

		// Token: 0x06004236 RID: 16950 RVA: 0x001A9344 File Offset: 0x001A7544
		[CompilerGenerated]
		private void <DoHitscan>g__RayCastInOtherSubs|173_0(Vector2 rayStart, Vector2 rayEnd, ref Projectile.<>c__DisplayClass173_0 A_3)
		{
			foreach (Submarine submarine in Submarine.Loaded)
			{
				if (submarine != this.item.Submarine)
				{
					List<Projectile.HitscanResult> inSubHits = this.DoRayCast(rayStart - submarine.SimPosition, rayEnd - submarine.SimPosition, submarine);
					for (int i = 0; i < inSubHits.Count; i++)
					{
						inSubHits[i] = new Projectile.HitscanResult(inSubHits[i].Fixture, inSubHits[i].Point + submarine.SimPosition, inSubHits[i].Normal, inSubHits[i].Fraction, null);
					}
					A_3.hits.AddRange(inSubHits);
				}
			}
		}

		// Token: 0x04001F9A RID: 8090
		private float launchRot;

		// Token: 0x04001F9B RID: 8091
		private static readonly ImmutableArray<float> spreadPool;

		// Token: 0x04001F9D RID: 8093
		public const float WaterDragCoefficient = 0.1f;

		// Token: 0x04001F9E RID: 8094
		private readonly Queue<Projectile.Impact> impactQueue = new Queue<Projectile.Impact>();

		// Token: 0x04001F9F RID: 8095
		private bool removePending;

		// Token: 0x04001FA0 RID: 8096
		private byte spreadIndex;

		// Token: 0x04001FA1 RID: 8097
		private const float ContinuousCollisionThreshold = 5f;

		// Token: 0x04001FA2 RID: 8098
		private Joint stickJoint;

		// Token: 0x04001FA3 RID: 8099
		private Vector2 jointAxis;

		// Token: 0x04001FA5 RID: 8101
		private Vector2 launchPos;

		// Token: 0x04001FA6 RID: 8102
		public Submarine LaunchSub;

		// Token: 0x04001FA7 RID: 8103
		private readonly HashSet<Body> hits = new HashSet<Body>();

		// Token: 0x04001FA8 RID: 8104
		public List<Body> IgnoredBodies;

		// Token: 0x04001FA9 RID: 8105
		public Item Launcher;

		// Token: 0x04001FAA RID: 8106
		private Character stickTargetCharacter;

		// Token: 0x04001FAB RID: 8107
		private Character _user;

		// Token: 0x04001FC0 RID: 8128
		private float deactivationTimer;

		// Token: 0x04001FC2 RID: 8130
		private float stickTimer;

		// Token: 0x04001FC5 RID: 8133
		private float maxJointTranslationInSimUnits = -1f;

		// Token: 0x04001FCC RID: 8140
		private Category originalCollisionCategories;

		// Token: 0x04001FCD RID: 8141
		private Category originalCollisionTargets;

		// Token: 0x04001FCE RID: 8142
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x04001FCF RID: 8143
		private Fixture lastTarget;

		// Token: 0x02000DC2 RID: 3522
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x06006848 RID: 26696 RVA: 0x00222351 File Offset: 0x00220551
			public EventData(bool launch, byte spreadCounter = 0)
			{
				this.Launch = launch;
				this.SpreadCounter = spreadCounter;
			}

			// Token: 0x0400409D RID: 16541
			public readonly bool Launch;

			// Token: 0x0400409E RID: 16542
			public readonly byte SpreadCounter;
		}

		// Token: 0x02000DC3 RID: 3523
		private readonly struct HitscanResult
		{
			// Token: 0x06006849 RID: 26697 RVA: 0x00222361 File Offset: 0x00220561
			public HitscanResult(Fixture fixture, Vector2 point, Vector2 normal, float fraction, Submarine sub)
			{
				this.Fixture = fixture;
				this.Point = point;
				this.Normal = normal;
				this.Fraction = fraction;
				this.Submarine = sub;
			}

			// Token: 0x0400409F RID: 16543
			public readonly Fixture Fixture;

			// Token: 0x040040A0 RID: 16544
			public readonly Vector2 Point;

			// Token: 0x040040A1 RID: 16545
			public readonly Vector2 Normal;

			// Token: 0x040040A2 RID: 16546
			public readonly float Fraction;

			// Token: 0x040040A3 RID: 16547
			public readonly Submarine Submarine;
		}

		// Token: 0x02000DC4 RID: 3524
		private struct Impact
		{
			// Token: 0x0600684A RID: 26698 RVA: 0x00222388 File Offset: 0x00220588
			public Impact(Fixture fixture, Vector2 normal, Vector2 velocity)
			{
				this.Fixture = fixture;
				this.Normal = normal;
				this.LinearVelocity = velocity;
			}

			// Token: 0x040040A4 RID: 16548
			public Fixture Fixture;

			// Token: 0x040040A5 RID: 16549
			public Vector2 Normal;

			// Token: 0x040040A6 RID: 16550
			public Vector2 LinearVelocity;
		}

		// Token: 0x02000DC5 RID: 3525
		private enum StickTargetType
		{
			// Token: 0x040040A8 RID: 16552
			Structure,
			// Token: 0x040040A9 RID: 16553
			Limb,
			// Token: 0x040040AA RID: 16554
			Item,
			// Token: 0x040040AB RID: 16555
			Submarine,
			// Token: 0x040040AC RID: 16556
			LevelWall,
			// Token: 0x040040AD RID: 16557
			Unknown
		}
	}
}
