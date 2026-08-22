using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Barotrauma.Particles;
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
	// Token: 0x020005CF RID: 1487
	internal class Projectile : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06005E8B RID: 24203 RVA: 0x00314930 File Offset: 0x00312B30
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			bool launch = msg.ReadBoolean();
			if (launch)
			{
				ushort userId = msg.ReadUInt16();
				this.User = (Entity.FindEntityByID(userId) as Character);
				Vector2 simPosition = new Vector2(msg.ReadSingle(), msg.ReadSingle());
				float rotation = msg.ReadSingle();
				this.spreadIndex = msg.ReadByte();
				ushort submarineID = msg.ReadUInt16();
				if (this.User != null)
				{
					this.Shoot(this.User, simPosition, simPosition, rotation, (from l in this.User.AnimController.Limbs
					where !l.IsSevered
					select l.body.FarseerBody).ToList<Body>(), false, 1f, 0f);
					this.item.Submarine = (Entity.FindEntityByID(submarineID) as Submarine);
				}
				else
				{
					this.Launch(this.User, simPosition, rotation, 1f, 0f);
				}
			}
			bool isStuck = msg.ReadBoolean();
			if (isStuck)
			{
				ushort submarineID2 = msg.ReadUInt16();
				ushort hullID = msg.ReadUInt16();
				Vector2 simPosition2 = new Vector2(msg.ReadSingle(), msg.ReadSingle());
				Vector2 axis = new Vector2(msg.ReadSingle(), msg.ReadSingle());
				Projectile.StickTargetType targetType = (Projectile.StickTargetType)msg.ReadByte();
				Submarine submarine = Entity.FindEntityByID(submarineID2) as Submarine;
				Hull hull = Entity.FindEntityByID(hullID) as Hull;
				this.item.Submarine = submarine;
				this.item.CurrentHull = hull;
				this.item.body.SetTransformIgnoreContacts(simPosition2, this.item.body.Rotation, true);
				switch (targetType)
				{
				case Projectile.StickTargetType.Structure:
				{
					ushort structureId = msg.ReadUInt16();
					byte bodyIndex = msg.ReadByte();
					Structure structure = Entity.FindEntityByID(structureId) as Structure;
					if (structure == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
						defaultInterpolatedStringHandler.AppendLiteral("\"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\" failed to stick to a structure. Could not find a structure with the ID ");
						defaultInterpolatedStringHandler.AppendFormatted<ushort>(structureId);
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						return;
					}
					if (bodyIndex == 255)
					{
						bodyIndex = 0;
					}
					if ((int)bodyIndex >= structure.Bodies.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(102, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Failed to read a projectile update from the server. Structure body index out of bounds (");
						defaultInterpolatedStringHandler2.AppendFormatted<byte>(bodyIndex);
						defaultInterpolatedStringHandler2.AppendLiteral(", structure: ");
						defaultInterpolatedStringHandler2.AppendFormatted<Structure>(structure);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
						return;
					}
					Body body = structure.Bodies[(int)bodyIndex];
					this.StickToTarget(body, axis);
					return;
				}
				case Projectile.StickTargetType.Limb:
				{
					ushort characterId = msg.ReadUInt16();
					byte limbIndex = msg.ReadByte();
					Character character = Entity.FindEntityByID(characterId) as Character;
					if (character == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(69, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("\"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
						defaultInterpolatedStringHandler3.AppendLiteral("\" failed to stick to a limb. Could not find a character with the ID ");
						defaultInterpolatedStringHandler3.AppendFormatted<ushort>(characterId);
						DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
						return;
					}
					if ((int)limbIndex >= character.AnimController.Limbs.Length)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(92, 2);
						defaultInterpolatedStringHandler4.AppendLiteral("Failed to read a projectile update from the server. Limb index out of bounds (");
						defaultInterpolatedStringHandler4.AppendFormatted<byte>(limbIndex);
						defaultInterpolatedStringHandler4.AppendLiteral(", character: ");
						defaultInterpolatedStringHandler4.AppendFormatted<Character>(character);
						defaultInterpolatedStringHandler4.AppendLiteral(")");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
						return;
					}
					if (character.Removed)
					{
						return;
					}
					Limb limb = character.AnimController.Limbs[(int)limbIndex];
					this.StickToTarget(limb.body.FarseerBody, axis);
					return;
				}
				case Projectile.StickTargetType.Item:
				{
					ushort itemID = msg.ReadUInt16();
					Item targetItem = Entity.FindEntityByID(itemID) as Item;
					if (targetItem == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(65, 2);
						defaultInterpolatedStringHandler5.AppendLiteral("\"");
						defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
						defaultInterpolatedStringHandler5.AppendLiteral("\" failed to stick to an item. Could not find n item with the ID ");
						defaultInterpolatedStringHandler5.AppendFormatted<ushort>(itemID);
						DebugConsole.AddWarning(defaultInterpolatedStringHandler5.ToStringAndClear(), null);
						return;
					}
					if (targetItem.Removed)
					{
						return;
					}
					Door door = targetItem.GetComponent<Door>();
					if (door != null)
					{
						this.StickToTarget(door.Body.FarseerBody, axis);
						return;
					}
					if (targetItem.body != null)
					{
						this.StickToTarget(targetItem.body.FarseerBody, axis);
						return;
					}
					break;
				}
				case Projectile.StickTargetType.Submarine:
				{
					ushort targetSubmarineId = msg.ReadUInt16();
					Submarine targetSub = Entity.FindEntityByID(targetSubmarineId) as Submarine;
					if (targetSub != null)
					{
						this.StickToTarget(targetSub.PhysicsBody.FarseerBody, axis);
						return;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(74, 2);
					defaultInterpolatedStringHandler6.AppendLiteral("\"");
					defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
					defaultInterpolatedStringHandler6.AppendLiteral("\" failed to stick to a submarine. Could not find a structure with the ID ");
					defaultInterpolatedStringHandler6.AppendFormatted<ushort>(targetSubmarineId);
					DebugConsole.AddWarning(defaultInterpolatedStringHandler6.ToStringAndClear(), null);
					return;
				}
				case Projectile.StickTargetType.LevelWall:
				{
					int levelWallIndex = msg.ReadInt32();
					List<VoronoiCell> allCells = Level.Loaded.GetAllCells();
					if (levelWallIndex >= 0 && levelWallIndex < allCells.Count)
					{
						this.StickToTarget(allCells[levelWallIndex].Body, axis);
						return;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(99, 2);
					defaultInterpolatedStringHandler7.AppendLiteral("Failed to read a projectile update from the server. Level wall index out of bounds (");
					defaultInterpolatedStringHandler7.AppendFormatted<int>(levelWallIndex);
					defaultInterpolatedStringHandler7.AppendLiteral(", wall count: ");
					defaultInterpolatedStringHandler7.AppendFormatted<int>(allCells.Count);
					defaultInterpolatedStringHandler7.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler7.ToStringAndClear(), null, null, false, false);
					return;
				}
				default:
					return;
				}
			}
			else
			{
				this.Unstick();
			}
		}

		// Token: 0x06005E8C RID: 24204 RVA: 0x00314EDC File Offset: 0x003130DC
		static Projectile()
		{
			MTRandom random = new MTRandom(0);
			Projectile.spreadPool = (from f in Enumerable.Range(0, 256)
			select (float)random.NextDouble() - 0.5f).ToImmutableArray<float>();
		}

		// Token: 0x170017CE RID: 6094
		// (get) Token: 0x06005E8D RID: 24205 RVA: 0x00314F21 File Offset: 0x00313121
		// (set) Token: 0x06005E8E RID: 24206 RVA: 0x00314F28 File Offset: 0x00313128
		public static byte SpreadCounter { get; private set; }

		// Token: 0x06005E8F RID: 24207 RVA: 0x00314F30 File Offset: 0x00313130
		public static void ResetSpreadCounter()
		{
			Projectile.SpreadCounter = 0;
		}

		// Token: 0x170017CF RID: 6095
		// (get) Token: 0x06005E90 RID: 24208 RVA: 0x00314F38 File Offset: 0x00313138
		// (set) Token: 0x06005E91 RID: 24209 RVA: 0x00314F40 File Offset: 0x00313140
		public Attack Attack { get; private set; }

		// Token: 0x170017D0 RID: 6096
		// (get) Token: 0x06005E92 RID: 24210 RVA: 0x00314F49 File Offset: 0x00313149
		// (set) Token: 0x06005E93 RID: 24211 RVA: 0x00314F51 File Offset: 0x00313151
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

		// Token: 0x170017D1 RID: 6097
		// (get) Token: 0x06005E94 RID: 24212 RVA: 0x00314F70 File Offset: 0x00313170
		// (set) Token: 0x06005E95 RID: 24213 RVA: 0x00314F78 File Offset: 0x00313178
		public Character Attacker { get; set; }

		// Token: 0x170017D2 RID: 6098
		// (get) Token: 0x06005E96 RID: 24214 RVA: 0x00314F81 File Offset: 0x00313181
		public IEnumerable<Body> Hits
		{
			get
			{
				return this.hits;
			}
		}

		// Token: 0x170017D3 RID: 6099
		// (get) Token: 0x06005E97 RID: 24215 RVA: 0x00314F89 File Offset: 0x00313189
		// (set) Token: 0x06005E98 RID: 24216 RVA: 0x00314F91 File Offset: 0x00313191
		[Serialize(10f, IsPropertySaveable.No, "The impulse applied to the physics body of the item when it's launched. Higher values make the projectile faster.", "", false)]
		public float LaunchImpulse { get; set; }

		// Token: 0x170017D4 RID: 6100
		// (get) Token: 0x06005E99 RID: 24217 RVA: 0x00314F9A File Offset: 0x0031319A
		// (set) Token: 0x06005E9A RID: 24218 RVA: 0x00314FA2 File Offset: 0x003131A2
		[Serialize(0f, IsPropertySaveable.No, "The random percentage modifier used to add variance to the launch impulse.", "", false)]
		public float ImpulseSpread { get; set; }

		// Token: 0x170017D5 RID: 6101
		// (get) Token: 0x06005E9B RID: 24219 RVA: 0x00314FAB File Offset: 0x003131AB
		// (set) Token: 0x06005E9C RID: 24220 RVA: 0x00314FB8 File Offset: 0x003131B8
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

		// Token: 0x170017D6 RID: 6102
		// (get) Token: 0x06005E9D RID: 24221 RVA: 0x00314FC6 File Offset: 0x003131C6
		// (set) Token: 0x06005E9E RID: 24222 RVA: 0x00314FCE File Offset: 0x003131CE
		public float LaunchRotationRadians { get; private set; }

		// Token: 0x170017D7 RID: 6103
		// (get) Token: 0x06005E9F RID: 24223 RVA: 0x00314FD7 File Offset: 0x003131D7
		// (set) Token: 0x06005EA0 RID: 24224 RVA: 0x00314FDF File Offset: 0x003131DF
		[Serialize(false, IsPropertySaveable.No, "When set to true, the item can stick to any target it hits.", "", false)]
		public bool DoesStick { get; set; }

		// Token: 0x170017D8 RID: 6104
		// (get) Token: 0x06005EA1 RID: 24225 RVA: 0x00314FE8 File Offset: 0x003131E8
		// (set) Token: 0x06005EA2 RID: 24226 RVA: 0x00314FF0 File Offset: 0x003131F0
		[Serialize(false, IsPropertySaveable.No, "Can the projectile stick to characters.", "", false)]
		public bool StickToCharacters { get; set; }

		// Token: 0x170017D9 RID: 6105
		// (get) Token: 0x06005EA3 RID: 24227 RVA: 0x00314FF9 File Offset: 0x003131F9
		// (set) Token: 0x06005EA4 RID: 24228 RVA: 0x00315001 File Offset: 0x00313201
		[Serialize(false, IsPropertySaveable.No, "Can the projectile stick to walls.", "", false)]
		public bool StickToStructures { get; set; }

		// Token: 0x170017DA RID: 6106
		// (get) Token: 0x06005EA5 RID: 24229 RVA: 0x0031500A File Offset: 0x0031320A
		// (set) Token: 0x06005EA6 RID: 24230 RVA: 0x00315012 File Offset: 0x00313212
		[Serialize(false, IsPropertySaveable.No, "Can the projectile stick to items.", "", false)]
		public bool StickToItems { get; set; }

		// Token: 0x170017DB RID: 6107
		// (get) Token: 0x06005EA7 RID: 24231 RVA: 0x0031501B File Offset: 0x0031321B
		// (set) Token: 0x06005EA8 RID: 24232 RVA: 0x00315023 File Offset: 0x00313223
		[Serialize(false, IsPropertySaveable.No, "Can the projectile stick to doors. Caution: may cause issues.", "", false)]
		public bool StickToDoors { get; set; }

		// Token: 0x170017DC RID: 6108
		// (get) Token: 0x06005EA9 RID: 24233 RVA: 0x0031502C File Offset: 0x0031322C
		// (set) Token: 0x06005EAA RID: 24234 RVA: 0x00315034 File Offset: 0x00313234
		[Serialize(false, IsPropertySaveable.No, "Can the item stick even to deflective targets.", "", false)]
		public bool StickToDeflective { get; set; }

		// Token: 0x170017DD RID: 6109
		// (get) Token: 0x06005EAB RID: 24235 RVA: 0x0031503D File Offset: 0x0031323D
		// (set) Token: 0x06005EAC RID: 24236 RVA: 0x00315045 File Offset: 0x00313245
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool StickToLightTargets { get; set; }

		// Token: 0x170017DE RID: 6110
		// (get) Token: 0x06005EAD RID: 24237 RVA: 0x0031504E File Offset: 0x0031324E
		// (set) Token: 0x06005EAE RID: 24238 RVA: 0x00315056 File Offset: 0x00313256
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool GoThroughLightTargets { get; set; }

		// Token: 0x170017DF RID: 6111
		// (get) Token: 0x06005EAF RID: 24239 RVA: 0x0031505F File Offset: 0x0031325F
		// (set) Token: 0x06005EB0 RID: 24240 RVA: 0x00315067 File Offset: 0x00313267
		[Serialize(-1f, IsPropertySaveable.No, "Minimum mass of targets to stick to when StickToLightTargets is disabled. Defaults to half of the projectile's mass.", "", false)]
		public float LightTargetMassThreshold { get; set; }

		// Token: 0x170017E0 RID: 6112
		// (get) Token: 0x06005EB1 RID: 24241 RVA: 0x00315070 File Offset: 0x00313270
		// (set) Token: 0x06005EB2 RID: 24242 RVA: 0x00315078 File Offset: 0x00313278
		[Serialize(false, IsPropertySaveable.No, "Hitscan projectiles cast a ray forwards and immediately hit whatever the ray hits. It is recommended to use hitscans for very fast-moving projectiles such as bullets, because using extremely fast launch velocities may cause physics glitches.", "", false)]
		public bool Hitscan { get; set; }

		// Token: 0x170017E1 RID: 6113
		// (get) Token: 0x06005EB3 RID: 24243 RVA: 0x00315081 File Offset: 0x00313281
		// (set) Token: 0x06005EB4 RID: 24244 RVA: 0x00315089 File Offset: 0x00313289
		[Serialize(1, IsPropertySaveable.No, "How many hitscans should be done when the projectile is launched. Multiple hitscans can be used to simulate weapons that fire multiple projectiles at the same time without having to actually use multiple projectile items, for example shotguns.", "", false)]
		public int HitScanCount { get; set; }

		// Token: 0x170017E2 RID: 6114
		// (get) Token: 0x06005EB5 RID: 24245 RVA: 0x00315092 File Offset: 0x00313292
		// (set) Token: 0x06005EB6 RID: 24246 RVA: 0x0031509A File Offset: 0x0031329A
		[Serialize(1, IsPropertySaveable.No, "How many targets the projectile can hit before it stops.", "", false)]
		public int MaxTargetsToHit { get; set; }

		// Token: 0x170017E3 RID: 6115
		// (get) Token: 0x06005EB7 RID: 24247 RVA: 0x003150A3 File Offset: 0x003132A3
		// (set) Token: 0x06005EB8 RID: 24248 RVA: 0x003150AB File Offset: 0x003132AB
		[Serialize(false, IsPropertySaveable.No, "Should the item be deleted when it hits something.", "", false)]
		public bool RemoveOnHit { get; set; }

		// Token: 0x170017E4 RID: 6116
		// (get) Token: 0x06005EB9 RID: 24249 RVA: 0x003150B4 File Offset: 0x003132B4
		// (set) Token: 0x06005EBA RID: 24250 RVA: 0x003150BC File Offset: 0x003132BC
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the launch angle of the projectile (in degrees).", "", false)]
		public float Spread { get; set; }

		// Token: 0x170017E5 RID: 6117
		// (get) Token: 0x06005EBB RID: 24251 RVA: 0x003150C5 File Offset: 0x003132C5
		// (set) Token: 0x06005EBC RID: 24252 RVA: 0x003150CD File Offset: 0x003132CD
		[Serialize(false, IsPropertySaveable.No, "Override random spread with static spread; projectiles are launched with an equal amount of angle between them. Only applies when firing multiple projectiles.", "", false)]
		public bool StaticSpread { get; set; }

		// Token: 0x170017E6 RID: 6118
		// (get) Token: 0x06005EBD RID: 24253 RVA: 0x003150D6 File Offset: 0x003132D6
		// (set) Token: 0x06005EBE RID: 24254 RVA: 0x003150DE File Offset: 0x003132DE
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool FriendlyFire { get; set; }

		// Token: 0x170017E7 RID: 6119
		// (get) Token: 0x06005EBF RID: 24255 RVA: 0x003150E7 File Offset: 0x003132E7
		// (set) Token: 0x06005EC0 RID: 24256 RVA: 0x003150EF File Offset: 0x003132EF
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float DeactivationTime { get; set; }

		// Token: 0x170017E8 RID: 6120
		// (get) Token: 0x06005EC1 RID: 24257 RVA: 0x003150F8 File Offset: 0x003132F8
		// (set) Token: 0x06005EC2 RID: 24258 RVA: 0x00315100 File Offset: 0x00313300
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float StickDuration { get; set; }

		// Token: 0x170017E9 RID: 6121
		// (get) Token: 0x06005EC3 RID: 24259 RVA: 0x00315109 File Offset: 0x00313309
		// (set) Token: 0x06005EC4 RID: 24260 RVA: 0x00315111 File Offset: 0x00313311
		[Serialize(-1f, IsPropertySaveable.No, "", "", false)]
		public float MaxJointTranslation { get; set; }

		// Token: 0x170017EA RID: 6122
		// (get) Token: 0x06005EC5 RID: 24261 RVA: 0x0031511A File Offset: 0x0031331A
		// (set) Token: 0x06005EC6 RID: 24262 RVA: 0x00315122 File Offset: 0x00313322
		[Serialize(1000f, IsPropertySaveable.No, "", "", false)]
		public float JointBreakPoint { get; set; }

		// Token: 0x170017EB RID: 6123
		// (get) Token: 0x06005EC7 RID: 24263 RVA: 0x0031512B File Offset: 0x0031332B
		// (set) Token: 0x06005EC8 RID: 24264 RVA: 0x00315133 File Offset: 0x00313333
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool Prismatic { get; set; }

		// Token: 0x170017EC RID: 6124
		// (get) Token: 0x06005EC9 RID: 24265 RVA: 0x0031513C File Offset: 0x0031333C
		// (set) Token: 0x06005ECA RID: 24266 RVA: 0x00315144 File Offset: 0x00313344
		[Serialize(false, IsPropertySaveable.No, "Enable only if you want to make the projectile ignore collisions with other projectiles when it's shot. Doesn't have any effect, if the item is not set to be damaged by projectiles.", "", false)]
		public bool IgnoreProjectilesWhileActive { get; set; }

		// Token: 0x170017ED RID: 6125
		// (get) Token: 0x06005ECB RID: 24267 RVA: 0x0031514D File Offset: 0x0031334D
		// (set) Token: 0x06005ECC RID: 24268 RVA: 0x00315155 File Offset: 0x00313355
		public Body StickTarget { get; private set; }

		// Token: 0x170017EE RID: 6126
		// (get) Token: 0x06005ECD RID: 24269 RVA: 0x0031515E File Offset: 0x0031335E
		// (set) Token: 0x06005ECE RID: 24270 RVA: 0x00315166 File Offset: 0x00313366
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamageDoors { get; set; }

		// Token: 0x170017EF RID: 6127
		// (get) Token: 0x06005ECF RID: 24271 RVA: 0x0031516F File Offset: 0x0031336F
		// (set) Token: 0x06005ED0 RID: 24272 RVA: 0x00315177 File Offset: 0x00313377
		[Serialize(false, IsPropertySaveable.No, "Can the projectile hit the user? Should generally be disabled, unless the projectile is for example something like shrapnel launched by a projectile impact.", "", false)]
		public bool DamageUser { get; set; }

		// Token: 0x170017F0 RID: 6128
		// (get) Token: 0x06005ED1 RID: 24273 RVA: 0x00315180 File Offset: 0x00313380
		public bool IsStuckToTarget
		{
			get
			{
				return this.StickTarget != null;
			}
		}

		// Token: 0x06005ED2 RID: 24274 RVA: 0x0031518C File Offset: 0x0031338C
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
			this.InitProjSpecific(element);
		}

		// Token: 0x06005ED3 RID: 24275 RVA: 0x0031529C File Offset: 0x0031349C
		private void InitProjSpecific(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "particleemitter")
				{
					ParticleEmitter emitter = new ParticleEmitter(subElement);
					emitter.Prefab.Properties.UseTracerPoints = subElement.GetAttributeBool("UseTracerPoints", true);
					this.particleEmitters.Add(emitter);
				}
			}
		}

		// Token: 0x06005ED4 RID: 24276 RVA: 0x00315330 File Offset: 0x00313530
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

		// Token: 0x06005ED5 RID: 24277 RVA: 0x0031546F File Offset: 0x0031366F
		public float GetSpreadFromPool()
		{
			this.spreadIndex = (byte)MathUtils.PositiveModulo((int)this.spreadIndex, Projectile.spreadPool.Length);
			return Projectile.spreadPool[(int)this.spreadIndex];
		}

		// Token: 0x06005ED6 RID: 24278 RVA: 0x003154A0 File Offset: 0x003136A0
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

		// Token: 0x06005ED7 RID: 24279 RVA: 0x003155E0 File Offset: 0x003137E0
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
			if (createNetworkEvent && !base.Item.Removed && GameMain.NetworkMember != null)
			{
				bool isServer = GameMain.NetworkMember.IsServer;
			}
		}

		// Token: 0x06005ED8 RID: 24280 RVA: 0x003156A8 File Offset: 0x003138A8
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

		// Token: 0x06005ED9 RID: 24281 RVA: 0x003158B0 File Offset: 0x00313AB0
		private float GetLaunchRotation(float unmodifiedLaunchRotation)
		{
			float launchRotation = unmodifiedLaunchRotation + this.item.body.Dir * this.LaunchRotationRadians;
			if (this.item.body.Dir < 0f)
			{
				launchRotation -= 3.1415927f;
			}
			return launchRotation;
		}

		// Token: 0x06005EDA RID: 24282 RVA: 0x003158F7 File Offset: 0x00313AF7
		public override bool Use(float deltaTime, Character character = null)
		{
			return this.Use(character, 0f);
		}

		// Token: 0x06005EDB RID: 24283 RVA: 0x00315908 File Offset: 0x00313B08
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

		// Token: 0x06005EDC RID: 24284 RVA: 0x00315B00 File Offset: 0x00313D00
		private void DoHitscan(Vector2 dir)
		{
			Projectile.<>c__DisplayClass171_0 CS$<>8__locals1;
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
			Screen selected = Screen.Selected;
			float? num;
			if (selected == null)
			{
				num = null;
			}
			else
			{
				Camera cam = selected.Cam;
				num = ((cam != null) ? new int?(cam.WorldView.Width) : null);
			}
			float worldDist = num ?? ((float)GameMain.GraphicsWidth);
			Vector2 rayEndWorld = rayStartWorld + dir * worldDist;
			CS$<>8__locals1.hits = new List<Projectile.HitscanResult>();
			CS$<>8__locals1.hits.AddRange(this.DoRayCast(rayStart, rayEnd, this.item.Submarine));
			if (this.item.Submarine != null)
			{
				CS$<>8__locals1.hits.AddRange(this.DoRayCast(rayStart + this.item.Submarine.SimPosition, rayEnd + this.item.Submarine.SimPosition, null));
				this.<DoHitscan>g__RayCastInOtherSubs|171_0(rayStart + this.item.Submarine.SimPosition, rayEnd + this.item.Submarine.SimPosition, ref CS$<>8__locals1);
			}
			else
			{
				this.<DoHitscan>g__RayCastInOtherSubs|171_0(rayStart, rayEnd, ref CS$<>8__locals1);
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
						this.LaunchProjSpecific(rayStartWorld, this.item.WorldPosition);
						break;
					}
				}
			}
			if (hitCount < this.MaxTargetsToHit)
			{
				this.item.body.SetTransformIgnoreContacts(this.item.body.SimPosition, rotation, true);
				this.LaunchProjSpecific(rayStartWorld, rayEndWorld);
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

		// Token: 0x06005EDD RID: 24285 RVA: 0x00315E30 File Offset: 0x00314030
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

		// Token: 0x06005EDE RID: 24286 RVA: 0x00315F1A File Offset: 0x0031411A
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

		// Token: 0x06005EDF RID: 24287 RVA: 0x00315F40 File Offset: 0x00314140
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
			}
		}

		// Token: 0x06005EE0 RID: 24288 RVA: 0x003161A4 File Offset: 0x003143A4
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

		// Token: 0x06005EE1 RID: 24289 RVA: 0x003161F8 File Offset: 0x003143F8
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

		// Token: 0x06005EE2 RID: 24290 RVA: 0x0031644C File Offset: 0x0031464C
		private bool ShouldIgnoreCharacterCollision(Character character)
		{
			Controller controller = this.item.GetComponent<Controller>();
			return (controller != null && controller.User == character && controller.IsAttachedUser(controller.User)) || (!this.FriendlyFire && this.User != null && character.IsFriendly(this.User));
		}

		// Token: 0x06005EE3 RID: 24291 RVA: 0x003164A2 File Offset: 0x003146A2
		public bool ShouldIgnoreSubmarineCollision(Fixture target, Contact contact)
		{
			return this.ShouldIgnoreSubmarineCollision(ref target, contact);
		}

		// Token: 0x06005EE4 RID: 24292 RVA: 0x003164B0 File Offset: 0x003146B0
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

		// Token: 0x06005EE5 RID: 24293 RVA: 0x00316684 File Offset: 0x00314884
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
						if (attackResult.Damage > 0f && targetItem.Prefab.ShowHealthBar && Character.Controlled != null && (this.User == Character.Controlled || Character.Controlled.CanSeeTarget(this.item, null, false, false)))
						{
							Character.Controlled.UpdateHUDProgressBar(targetItem, targetItem.WorldPosition, targetItem.Condition / targetItem.MaxCondition, GUIStyle.HealthBarColorLow, GUIStyle.HealthBarColorHigh, targetItem.Prefab.ShowNameInHealthBar ? targetItem.Name : string.Empty);
						}
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
			base.PlaySound(conditionalActionType, this.User);
			base.PlaySound(ActionType.OnImpact, this.User);
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
					goto IL_975;
				}
			}
			this.DisableProjectileCollisions();
			IL_975:
			if (attackResult.AppliedDamageModifiers != null)
			{
				if (attackResult.AppliedDamageModifiers.Any((DamageModifier dm) => dm.DeflectProjectiles) && !this.StickToDeflective)
				{
					this.item.body.LinearVelocity *= deflectedSpeedMultiplier;
					goto IL_B92;
				}
			}
			if (remainingHits > 0 || this.stickJoint != null || this.StickTarget != null || !this.StickToStructures || !(target.Body.UserData is Structure))
			{
				if (this.StickToLightTargets || target.Body.Mass >= this.GetLightTargetMassThreshold())
				{
					if (this.DoesStick || (this.StickToCharacters && (target.Body.UserData is Limb || target.Body.UserData is Character)))
					{
						goto IL_AA5;
					}
					Item i = target.Body.UserData as Item;
					if (i != null && ((i.GetComponent<Door>() != null) ? this.StickToDoors : this.StickToItems))
					{
						goto IL_AA5;
					}
				}
				this.item.body.LinearVelocity *= speedMultiplier;
				goto IL_B92;
			}
			IL_AA5:
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
			this.item.body.LinearVelocity *= speedMultiplier;
			return this.Hitscan;
			IL_B92:
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

		// Token: 0x06005EE6 RID: 24294 RVA: 0x00317314 File Offset: 0x00315514
		private float GetLightTargetMassThreshold()
		{
			if (this.LightTargetMassThreshold >= 0f)
			{
				return this.LightTargetMassThreshold;
			}
			return this.item.body.Mass * 0.5f;
		}

		// Token: 0x06005EE7 RID: 24295 RVA: 0x00317340 File Offset: 0x00315540
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

		// Token: 0x06005EE8 RID: 24296 RVA: 0x003173DC File Offset: 0x003155DC
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

		// Token: 0x06005EE9 RID: 24297 RVA: 0x00317501 File Offset: 0x00315701
		public bool IsAttachedTo(PhysicsBody body)
		{
			return this.stickJoint != null && (this.stickJoint.BodyA == ((body != null) ? body.FarseerBody : null) || this.stickJoint.BodyB == ((body != null) ? body.FarseerBody : null));
		}

		// Token: 0x06005EEA RID: 24298 RVA: 0x00317544 File Offset: 0x00315744
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

		// Token: 0x06005EEB RID: 24299 RVA: 0x003176FC File Offset: 0x003158FC
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

		// Token: 0x06005EEC RID: 24300 RVA: 0x003177BB File Offset: 0x003159BB
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			if (this.IsStuckToTarget || this.stickJoint != null || this.stickTargetCharacter != null)
			{
				this.Unstick();
			}
		}

		// Token: 0x06005EED RID: 24301 RVA: 0x003177E4 File Offset: 0x003159E4
		private void LaunchProjSpecific(Vector2 startLocation, Vector2 endLocation)
		{
			Vector2 particlePos = this.item.WorldPosition;
			float rotation = -this.item.body.Rotation;
			if (this.item.body.Dir < 0f)
			{
				rotation += 3.1415927f;
			}
			particlePos = Projectile.<LaunchProjSpecific>g__ConvertToWorldCoordinates|190_0(particlePos);
			startLocation = Projectile.<LaunchProjSpecific>g__ConvertToWorldCoordinates|190_0(startLocation);
			endLocation = Projectile.<LaunchProjSpecific>g__ConvertToWorldCoordinates|190_0(endLocation);
			Tuple<Vector2, Vector2> tracerPoints = new Tuple<Vector2, Vector2>(startLocation, endLocation);
			foreach (ParticleEmitter emitter in this.particleEmitters)
			{
				emitter.Emit(1f, particlePos, null, rotation, rotation, 1f, 1f, 1f, new Color?(emitter.Prefab.Properties.ColorMultiplier), null, false, tracerPoints);
			}
		}

		// Token: 0x06005EEF RID: 24303 RVA: 0x003178E8 File Offset: 0x00315AE8
		[CompilerGenerated]
		private void <DoHitscan>g__RayCastInOtherSubs|171_0(Vector2 rayStart, Vector2 rayEnd, ref Projectile.<>c__DisplayClass171_0 A_3)
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

		// Token: 0x06005EF1 RID: 24305 RVA: 0x003179F0 File Offset: 0x00315BF0
		[CompilerGenerated]
		internal static Vector2 <LaunchProjSpecific>g__ConvertToWorldCoordinates|190_0(Vector2 position)
		{
			Submarine containing = Submarine.FindContainingInLocalCoordinates(position, 500f);
			if (containing != null)
			{
				position += containing.Position;
			}
			return position;
		}

		// Token: 0x040030D8 RID: 12504
		private readonly List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>();

		// Token: 0x040030D9 RID: 12505
		private static readonly ImmutableArray<float> spreadPool;

		// Token: 0x040030DB RID: 12507
		public const float WaterDragCoefficient = 0.1f;

		// Token: 0x040030DC RID: 12508
		private readonly Queue<Projectile.Impact> impactQueue = new Queue<Projectile.Impact>();

		// Token: 0x040030DD RID: 12509
		private bool removePending;

		// Token: 0x040030DE RID: 12510
		private byte spreadIndex;

		// Token: 0x040030DF RID: 12511
		private const float ContinuousCollisionThreshold = 5f;

		// Token: 0x040030E0 RID: 12512
		private Joint stickJoint;

		// Token: 0x040030E1 RID: 12513
		private Vector2 jointAxis;

		// Token: 0x040030E3 RID: 12515
		private Vector2 launchPos;

		// Token: 0x040030E4 RID: 12516
		public Submarine LaunchSub;

		// Token: 0x040030E5 RID: 12517
		private readonly HashSet<Body> hits = new HashSet<Body>();

		// Token: 0x040030E6 RID: 12518
		public List<Body> IgnoredBodies;

		// Token: 0x040030E7 RID: 12519
		public Item Launcher;

		// Token: 0x040030E8 RID: 12520
		private Character stickTargetCharacter;

		// Token: 0x040030E9 RID: 12521
		private Character _user;

		// Token: 0x040030FE RID: 12542
		private float deactivationTimer;

		// Token: 0x04003100 RID: 12544
		private float stickTimer;

		// Token: 0x04003103 RID: 12547
		private float maxJointTranslationInSimUnits = -1f;

		// Token: 0x0400310A RID: 12554
		private Category originalCollisionCategories;

		// Token: 0x0400310B RID: 12555
		private Category originalCollisionTargets;

		// Token: 0x0400310C RID: 12556
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x0400310D RID: 12557
		private Fixture lastTarget;

		// Token: 0x0200143D RID: 5181
		private readonly struct HitscanResult
		{
			// Token: 0x06009A48 RID: 39496 RVA: 0x003E2A5A File Offset: 0x003E0C5A
			public HitscanResult(Fixture fixture, Vector2 point, Vector2 normal, float fraction, Submarine sub)
			{
				this.Fixture = fixture;
				this.Point = point;
				this.Normal = normal;
				this.Fraction = fraction;
				this.Submarine = sub;
			}

			// Token: 0x04006502 RID: 25858
			public readonly Fixture Fixture;

			// Token: 0x04006503 RID: 25859
			public readonly Vector2 Point;

			// Token: 0x04006504 RID: 25860
			public readonly Vector2 Normal;

			// Token: 0x04006505 RID: 25861
			public readonly float Fraction;

			// Token: 0x04006506 RID: 25862
			public readonly Submarine Submarine;
		}

		// Token: 0x0200143E RID: 5182
		private struct Impact
		{
			// Token: 0x06009A49 RID: 39497 RVA: 0x003E2A81 File Offset: 0x003E0C81
			public Impact(Fixture fixture, Vector2 normal, Vector2 velocity)
			{
				this.Fixture = fixture;
				this.Normal = normal;
				this.LinearVelocity = velocity;
			}

			// Token: 0x04006507 RID: 25863
			public Fixture Fixture;

			// Token: 0x04006508 RID: 25864
			public Vector2 Normal;

			// Token: 0x04006509 RID: 25865
			public Vector2 LinearVelocity;
		}

		// Token: 0x0200143F RID: 5183
		private enum StickTargetType
		{
			// Token: 0x0400650B RID: 25867
			Structure,
			// Token: 0x0400650C RID: 25868
			Limb,
			// Token: 0x0400650D RID: 25869
			Item,
			// Token: 0x0400650E RID: 25870
			Submarine,
			// Token: 0x0400650F RID: 25871
			LevelWall,
			// Token: 0x04006510 RID: 25872
			Unknown
		}
	}
}
