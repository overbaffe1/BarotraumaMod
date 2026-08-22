using System;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A5 RID: 1189
	internal class Rope : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x0600427C RID: 17020 RVA: 0x001AAC44 File Offset: 0x001A8E44
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.Snapped);
			if (!this.Snapped)
			{
				Item item = this.target;
				msg.WriteUInt16((item != null) ? item.ID : 0);
				ISpatialEntity spatialEntity = this.source;
				Entity entity = spatialEntity as Entity;
				if (entity != null)
				{
					bool removed = entity.Removed;
					if (!removed)
					{
						msg.WriteUInt16(entity.ID);
						msg.WriteByte(0);
						return;
					}
				}
				else
				{
					Limb limb = spatialEntity as Limb;
					if (limb != null)
					{
						Character character = limb.character;
						if (character != null)
						{
							bool removed2 = character.Removed;
							if (!removed2)
							{
								msg.WriteUInt16(limb.character.ID);
								msg.WriteByte((byte)limb.character.AnimController.Limbs.IndexOf(limb));
								return;
							}
						}
					}
				}
				msg.WriteUInt16(0);
				msg.WriteByte(0);
			}
		}

		// Token: 0x0600427D RID: 17021 RVA: 0x001AAD14 File Offset: 0x001A8F14
		private void SetSource(ISpatialEntity source)
		{
			this.source = source;
			Limb sourceLimb = source as Limb;
			if (sourceLimb != null)
			{
				sourceLimb.AttachedRope = this;
				float offset = sourceLimb.Params.GetSpriteOrientation() - 1.5707964f;
				this.launchDir = new Vector2?(VectorExtensions.Forward(sourceLimb.body.TransformedRotation - offset * sourceLimb.character.AnimController.Dir, 1f));
			}
		}

		// Token: 0x0600427E RID: 17022 RVA: 0x001AAD80 File Offset: 0x001A8F80
		private void ResetSource()
		{
			Limb sourceLimb = this.source as Limb;
			if (sourceLimb != null && sourceLimb.AttachedRope == this)
			{
				sourceLimb.AttachedRope = null;
			}
			this.source = null;
		}

		// Token: 0x170011BF RID: 4543
		// (get) Token: 0x0600427F RID: 17023 RVA: 0x001AADB3 File Offset: 0x001A8FB3
		// (set) Token: 0x06004280 RID: 17024 RVA: 0x001AADBB File Offset: 0x001A8FBB
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float SnapAnimDuration { get; set; }

		// Token: 0x170011C0 RID: 4544
		// (get) Token: 0x06004281 RID: 17025 RVA: 0x001AADC4 File Offset: 0x001A8FC4
		// (set) Token: 0x06004282 RID: 17026 RVA: 0x001AADCC File Offset: 0x001A8FCC
		[Serialize(0f, IsPropertySaveable.No, "How much force is applied to pull the projectile the rope is attached to.", "", false)]
		public float ProjectilePullForce { get; set; }

		// Token: 0x170011C1 RID: 4545
		// (get) Token: 0x06004283 RID: 17027 RVA: 0x001AADD5 File Offset: 0x001A8FD5
		// (set) Token: 0x06004284 RID: 17028 RVA: 0x001AADDD File Offset: 0x001A8FDD
		[Serialize(0f, IsPropertySaveable.No, "How much force is applied to pull the target the rope is attached to.", "", false)]
		public float TargetPullForce { get; set; }

		// Token: 0x170011C2 RID: 4546
		// (get) Token: 0x06004285 RID: 17029 RVA: 0x001AADE6 File Offset: 0x001A8FE6
		// (set) Token: 0x06004286 RID: 17030 RVA: 0x001AADEE File Offset: 0x001A8FEE
		[Serialize(0f, IsPropertySaveable.No, "How much force is applied to pull the source the rope is attached to.", "", false)]
		public float SourcePullForce { get; set; }

		// Token: 0x170011C3 RID: 4547
		// (get) Token: 0x06004287 RID: 17031 RVA: 0x001AADF7 File Offset: 0x001A8FF7
		// (set) Token: 0x06004288 RID: 17032 RVA: 0x001AADFF File Offset: 0x001A8FFF
		[Serialize(1000f, IsPropertySaveable.No, "How far the source item can be from the projectile until the rope breaks.", "", false)]
		public float MaxLength { get; set; }

		// Token: 0x170011C4 RID: 4548
		// (get) Token: 0x06004289 RID: 17033 RVA: 0x001AAE08 File Offset: 0x001A9008
		// (set) Token: 0x0600428A RID: 17034 RVA: 0x001AAE10 File Offset: 0x001A9010
		[Serialize(200f, IsPropertySaveable.No, "At which distance the user stops pulling the target?", "", false)]
		public float MinPullDistance { get; set; }

		// Token: 0x170011C5 RID: 4549
		// (get) Token: 0x0600428B RID: 17035 RVA: 0x001AAE19 File Offset: 0x001A9019
		// (set) Token: 0x0600428C RID: 17036 RVA: 0x001AAE21 File Offset: 0x001A9021
		[Serialize(360f, IsPropertySaveable.No, "The maximum angle from the source to the target until the rope breaks.", "", false)]
		public float MaxAngle { get; set; }

		// Token: 0x170011C6 RID: 4550
		// (get) Token: 0x0600428D RID: 17037 RVA: 0x001AAE2A File Offset: 0x001A902A
		// (set) Token: 0x0600428E RID: 17038 RVA: 0x001AAE32 File Offset: 0x001A9032
		[Serialize(true, IsPropertySaveable.No, "Should the rope snap when it collides with a structure/submarine (if not, it will just go through it).", "", false)]
		public bool SnapOnCollision { get; set; }

		// Token: 0x170011C7 RID: 4551
		// (get) Token: 0x0600428F RID: 17039 RVA: 0x001AAE3B File Offset: 0x001A903B
		// (set) Token: 0x06004290 RID: 17040 RVA: 0x001AAE43 File Offset: 0x001A9043
		[Serialize(true, IsPropertySaveable.No, "Should the rope snap when the character drops the aim?", "", false)]
		public bool SnapWhenNotAimed { get; set; }

		// Token: 0x170011C8 RID: 4552
		// (get) Token: 0x06004291 RID: 17041 RVA: 0x001AAE4C File Offset: 0x001A904C
		// (set) Token: 0x06004292 RID: 17042 RVA: 0x001AAE54 File Offset: 0x001A9054
		[Serialize(true, IsPropertySaveable.No, "Should the rope snap when the weapon it was fired from is fired again? I.e. can there be multiple ropes coming from the weapon at the same time?", "", false)]
		public bool SnapWhenWeaponFiredAgain { get; set; }

		// Token: 0x170011C9 RID: 4553
		// (get) Token: 0x06004293 RID: 17043 RVA: 0x001AAE5D File Offset: 0x001A905D
		// (set) Token: 0x06004294 RID: 17044 RVA: 0x001AAE65 File Offset: 0x001A9065
		[Serialize(0.9f, IsPropertySaveable.No, "Multiplier for the length of the barrel when determining where the rope should start from.", "", false)]
		public float BarrelLengthMultiplier { get; set; }

		// Token: 0x170011CA RID: 4554
		// (get) Token: 0x06004295 RID: 17045 RVA: 0x001AAE6E File Offset: 0x001A906E
		// (set) Token: 0x06004296 RID: 17046 RVA: 0x001AAE76 File Offset: 0x001A9076
		[Serialize(30f, IsPropertySaveable.No, "How much mass is required for the target to pull the source towards it. Static and kinematic targets are always treated heavy enough.", "", false)]
		public float TargetMinMass { get; set; }

		// Token: 0x170011CB RID: 4555
		// (get) Token: 0x06004297 RID: 17047 RVA: 0x001AAE7F File Offset: 0x001A907F
		// (set) Token: 0x06004298 RID: 17048 RVA: 0x001AAE87 File Offset: 0x001A9087
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool LerpForces { get; set; }

		// Token: 0x170011CC RID: 4556
		// (get) Token: 0x06004299 RID: 17049 RVA: 0x001AAE90 File Offset: 0x001A9090
		// (set) Token: 0x0600429A RID: 17050 RVA: 0x001AAE98 File Offset: 0x001A9098
		[Serialize(true, IsPropertySaveable.No, "Should the force be dynamically adjusted to make it more difficult for targets to escape the pull?", "", false)]
		public bool IncreaseForceForEscapingTargets { get; set; }

		// Token: 0x170011CD RID: 4557
		// (get) Token: 0x0600429B RID: 17051 RVA: 0x001AAEA1 File Offset: 0x001A90A1
		// (set) Token: 0x0600429C RID: 17052 RVA: 0x001AAEAC File Offset: 0x001A90AC
		public bool Snapped
		{
			get
			{
				return this.snapped;
			}
			set
			{
				if (this.snapped == value)
				{
					return;
				}
				if (GameMain.NetworkMember != null)
				{
					if (GameMain.NetworkMember.IsClient)
					{
						return;
					}
					this.item.CreateServerEvent<Rope>(this);
				}
				this.snapped = value;
				if (!this.snapped)
				{
					this.snapTimer = 0f;
					return;
				}
				if (this.target != null && this.source != null)
				{
					Item item = this.target;
					ISpatialEntity spatialEntity = this.source;
				}
			}
		}

		// Token: 0x0600429D RID: 17053 RVA: 0x001AAF1C File Offset: 0x001A911C
		public Rope(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x0600429E RID: 17054 RVA: 0x001AAF26 File Offset: 0x001A9126
		public void Snap()
		{
			this.Snapped = true;
		}

		// Token: 0x0600429F RID: 17055 RVA: 0x001AAF30 File Offset: 0x001A9130
		public void Attach(ISpatialEntity source, Item target)
		{
			this.target = target;
			this.SetSource(source);
			this.Snapped = false;
			base.ApplyStatusEffects(ActionType.OnUse, 1f, null, null, null, null, new Vector2?(this.item.WorldPosition), 1f);
			this.IsActive = true;
		}

		// Token: 0x060042A0 RID: 17056 RVA: 0x001AAF80 File Offset: 0x001A9180
		public override void Update(float deltaTime, Camera cam)
		{
			this.isReelingIn = false;
			Projectile component = this.item.GetComponent<Projectile>();
			Character user = (component != null) ? component.User : null;
			if (this.source != null && this.target != null && !this.target.Removed)
			{
				Entity entity = this.source as Entity;
				if (entity == null || !entity.Removed)
				{
					Limb limb = this.source as Limb;
					if ((limb == null || !limb.Removed) && (user == null || !user.Removed))
					{
						if (this.Snapped)
						{
							this.snapTimer += deltaTime;
							if (this.snapTimer >= this.SnapAnimDuration)
							{
								this.IsActive = false;
							}
							return;
						}
						Vector2 diff = this.target.WorldPosition - this.GetSourcePos(false);
						float lengthSqr = diff.LengthSquared();
						if (lengthSqr > this.MaxLength * this.MaxLength)
						{
							this.Snap();
							return;
						}
						if (this.MaxAngle < 180f && lengthSqr > 2500f)
						{
							Vector2 value = this.launchDir.GetValueOrDefault();
							if (this.launchDir == null)
							{
								value = diff;
								this.launchDir = new Vector2?(value);
							}
							float angle = MathHelper.ToDegrees(this.launchDir.Value.Angle(diff));
							if (angle > this.MaxAngle)
							{
								this.Snap();
								return;
							}
						}
						Projectile projectile = this.target.GetComponent<Projectile>();
						if (projectile == null)
						{
							return;
						}
						if (this.SnapOnCollision)
						{
							this.raycastTimer += deltaTime;
							if (this.raycastTimer > 0.2f)
							{
								if (Submarine.PickBody(ConvertUnits.ToSimUnits(this.source.WorldPosition), ConvertUnits.ToSimUnits(this.target.WorldPosition), null, new Category?(Category.Cat1 | Category.Cat8), true, delegate(Fixture f)
								{
									foreach (Body body2 in projectile.Hits)
									{
										Submarine alreadyHitSub = null;
										Structure hitStructure = body2.UserData as Structure;
										if (hitStructure != null)
										{
											alreadyHitSub = hitStructure.Submarine;
										}
										else
										{
											Submarine hitSub = body2.UserData as Submarine;
											if (hitSub != null)
											{
												alreadyHitSub = hitSub;
											}
										}
										if (alreadyHitSub != null)
										{
											Body body3 = f.Body;
											MapEntity me = ((body3 != null) ? body3.UserData : null) as MapEntity;
											if (me != null && me.Submarine == alreadyHitSub)
											{
												return false;
											}
											Body body4 = f.Body;
											if (((body4 != null) ? body4.UserData : null) as Submarine == alreadyHitSub)
											{
												return false;
											}
										}
									}
									Body stickTarget = projectile.StickTarget;
									Submarine targetSub = (((stickTarget != null) ? stickTarget.UserData : null) as Submarine) ?? this.target.Submarine;
									Body body5 = f.Body;
									MapEntity mapEntity = ((body5 != null) ? body5.UserData : null) as MapEntity;
									if (mapEntity != null && mapEntity.Submarine != null)
									{
										if (mapEntity.Submarine == targetSub || mapEntity.Submarine == this.source.Submarine)
										{
											return false;
										}
									}
									else
									{
										Body body6 = f.Body;
										Submarine sub = ((body6 != null) ? body6.UserData : null) as Submarine;
										if (sub != null && (sub == targetSub || sub == this.source.Submarine))
										{
											return false;
										}
									}
									return true;
								}, false) != null)
								{
									this.Snap();
									return;
								}
								this.raycastTimer = 0f;
							}
						}
						Vector2 forceDir = diff;
						this.currentRopeLength = diff.Length();
						if (this.currentRopeLength > 0.001f)
						{
							forceDir = Vector2.Normalize(forceDir);
						}
						if (Math.Abs(this.ProjectilePullForce) > 0.001f)
						{
							Item item = projectile.Item;
							if (item != null)
							{
								PhysicsBody body = item.body;
								if (body != null)
								{
									body.ApplyForce(-forceDir * this.ProjectilePullForce, 64f);
								}
							}
						}
						if (projectile.StickTarget != null)
						{
							float targetMass = float.MaxValue;
							Character targetCharacter = null;
							object userData = projectile.StickTarget.UserData;
							Limb targetLimb = userData as Limb;
							if (targetLimb == null)
							{
								Character character = userData as Character;
								if (character == null)
								{
									if (userData is Item)
									{
										targetMass = projectile.StickTarget.Mass;
									}
								}
								else
								{
									targetCharacter = character;
									targetMass = character.Mass;
								}
							}
							else
							{
								targetCharacter = targetLimb.character;
								targetMass = targetLimb.ragdoll.Mass;
							}
							if (projectile.StickTarget.BodyType != BodyType.Dynamic)
							{
								targetMass = float.MaxValue;
							}
							if (user != null)
							{
								if (!this.snapped && (projectile.Launcher == null || projectile.Launcher.GetComponent<Holdable>() != null))
								{
									user.AnimController.HoldToRope();
									if (targetCharacter != null)
									{
										targetCharacter.AnimController.DragWithRope();
									}
									if (user.InWater)
									{
										user.AnimController.HangWithRope();
									}
								}
								if (Math.Abs(this.SourcePullForce) > 0.001f && targetMass > this.TargetMinMass)
								{
									PhysicsBody sourceBody = Rope.GetBodyToPull(this.source);
									if (sourceBody != null)
									{
										PhysicsBody targetBody = Rope.GetBodyToPull(this.target);
										if (sourceBody.UserData is Character)
										{
											this.isReelingIn = ((user.InWater && user.IsRagdolled) || (!user.InWater && targetCharacter != null && !targetCharacter.IsIncapacitated));
											if (this.isReelingIn)
											{
												float pullForce = this.SourcePullForce;
												if (!user.InWater)
												{
													pullForce *= 0.1f;
												}
												float lengthFactor = MathUtils.InverseLerp(0f, this.MaxLength / 2f, this.currentRopeLength);
												float force = this.LerpForces ? MathHelper.Lerp(0f, pullForce, lengthFactor) : pullForce;
												sourceBody.ApplyForce(forceDir * force, 64f);
												if (targetBody != null)
												{
													if (targetCharacter != null)
													{
														if (targetBody.LinearVelocity != Vector2.Zero && sourceBody.LinearVelocity != Vector2.Zero)
														{
															Vector2 targetDir = Vector2.Normalize(targetBody.LinearVelocity);
															float movementDot = Vector2.Dot(Vector2.Normalize(sourceBody.LinearVelocity), targetDir);
															if (movementDot < 0f)
															{
																float inverseLengthFactor = MathHelper.Lerp(1f, 0f, lengthFactor);
																sourceBody.ApplyForce(targetBody.LinearVelocity * Math.Min(targetBody.Mass * 5f, 250f) * sourceBody.Mass * -movementDot * inverseLengthFactor, 64f);
															}
															float forceDot = Vector2.Dot(forceDir, targetDir);
															if (forceDot > 0f)
															{
																float targetSpeed = targetBody.LinearVelocity.Length();
																sourceBody.ApplyForce(forceDir * targetSpeed * sourceBody.Mass * 25f * forceDot * lengthFactor, 64f);
															}
															float colliderMainLimbDistance = Vector2.Distance(sourceBody.SimPosition, user.AnimController.MainLimb.SimPosition);
															if (colliderMainLimbDistance > 1f && !(sourceBody.UserData is Submarine))
															{
																float correctionForce = MathHelper.Lerp(10f, 64f, MathUtils.InverseLerp(1f, 10f, colliderMainLimbDistance));
																Vector2 targetPos = sourceBody.SimPosition + new Vector2((float)Math.Sin((double)(-(double)sourceBody.Rotation)), (float)Math.Cos((double)(-(double)sourceBody.Rotation))) * 0.4f;
																user.AnimController.MainLimb.MoveToPos(targetPos, correctionForce, false);
															}
														}
													}
													else
													{
														sourceBody.ApplyForce(targetBody.LinearVelocity * sourceBody.Mass, 64f);
													}
												}
											}
										}
										else
										{
											float distance = Vector2.Distance(this.source.WorldPosition, this.target.WorldPosition);
											float force2 = this.LerpForces ? MathHelper.Lerp(0f, this.SourcePullForce, MathUtils.InverseLerp(0f, this.MaxLength / 2f, distance)) : this.SourcePullForce;
											sourceBody.ApplyForce(forceDir * force2, 64f);
										}
									}
								}
							}
							if (Math.Abs(this.TargetPullForce) > 0.001f && (user == null || !user.IsRagdolled))
							{
								PhysicsBody targetBody2 = Rope.GetBodyToPull(this.target);
								if (targetBody2 == null)
								{
									return;
								}
								bool lerpForces = this.LerpForces;
								float maxVelocity = 16f;
								float maxPullDistance = this.MaxLength / 3f;
								float minPullDistance = this.MinPullDistance;
								if (targetCharacter != null)
								{
									if (targetCharacter.IsRagdolled || targetCharacter.IsUnconscious)
									{
										if (!targetCharacter.InWater)
										{
											maxVelocity = 4.8f;
										}
									}
									else
									{
										minPullDistance = 50f;
										maxPullDistance = 200f;
									}
								}
								minPullDistance = MathHelper.Max(minPullDistance, 50f);
								if (this.currentRopeLength < minPullDistance)
								{
									return;
								}
								maxPullDistance = MathHelper.Max(minPullDistance * 2f, maxPullDistance);
								float force3 = lerpForces ? MathHelper.Lerp(0f, this.TargetPullForce, MathUtils.InverseLerp(minPullDistance, maxPullDistance, this.currentRopeLength)) : this.TargetPullForce;
								targetBody2.ApplyForce(-forceDir * force3, maxVelocity);
								AnimController targetRagdoll = (targetCharacter != null) ? targetCharacter.AnimController : null;
								if (((targetRagdoll != null) ? targetRagdoll.Collider : null) != null)
								{
									this.isReelingIn = true;
									if (targetRagdoll.InWater || targetRagdoll.OnGround)
									{
										float forceMultiplier = 1f;
										if (!targetCharacter.IsRagdolled && !targetCharacter.IsIncapacitated && this.IncreaseForceForEscapingTargets)
										{
											Vector2 targetMovement = targetCharacter.AnimController.TargetMovement;
											float dot = Vector2.Dot(Vector2.Normalize(targetMovement), forceDir);
											if (dot > 0f)
											{
												float targetVelocity = targetMovement.Length();
												float massFactor = Math.Max((float)Math.Log((double)(targetCharacter.Mass / 10f)), 1f);
												forceMultiplier = Math.Max(targetVelocity * massFactor * 2.5f * dot, 1f);
											}
										}
										targetRagdoll.Collider.ApplyForce(-forceDir * force3 * forceMultiplier, maxVelocity);
									}
								}
							}
						}
						return;
					}
				}
			}
			this.ResetSource();
			this.target = null;
			this.IsActive = false;
		}

		// Token: 0x060042A1 RID: 17057 RVA: 0x001AB83D File Offset: 0x001A9A3D
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			if (this.Snapped)
			{
				this.snapTimer += deltaTime;
				if (this.snapTimer >= this.SnapAnimDuration)
				{
					this.IsActive = false;
				}
			}
		}

		// Token: 0x060042A2 RID: 17058 RVA: 0x001AB874 File Offset: 0x001A9A74
		private Vector2 GetSourcePos(bool useDrawPosition = false)
		{
			Vector2 sourcePos = this.source.WorldPosition;
			Item sourceItem = this.source as Item;
			if (sourceItem != null)
			{
				if (useDrawPosition)
				{
					sourcePos = sourceItem.DrawPosition;
				}
				if (!sourceItem.Removed)
				{
					Turret turret = sourceItem.GetComponent<Turret>();
					if (turret != null)
					{
						sourcePos = new Vector2((float)sourceItem.WorldRect.X + turret.TransformedBarrelPos.X, (float)sourceItem.WorldRect.Y - turret.TransformedBarrelPos.Y);
					}
					else
					{
						RangedWeapon weapon = sourceItem.GetComponent<RangedWeapon>();
						if (weapon != null)
						{
							sourcePos += ConvertUnits.ToDisplayUnits(weapon.TransformedBarrelPos);
						}
					}
				}
			}
			else if (useDrawPosition)
			{
				Limb sourceLimb = this.source as Limb;
				if (sourceLimb != null && sourceLimb.body != null)
				{
					sourcePos = sourceLimb.body.DrawPosition;
				}
			}
			return sourcePos;
		}

		// Token: 0x060042A3 RID: 17059 RVA: 0x001AB940 File Offset: 0x001A9B40
		private static PhysicsBody GetBodyToPull(ISpatialEntity target)
		{
			Item targetItem = target as Item;
			if (targetItem != null)
			{
				Inventory parentInventory = targetItem.ParentInventory;
				if (parentInventory is CharacterInventory)
				{
					Character ownerCharacter = parentInventory.Owner as Character;
					if (ownerCharacter != null)
					{
						if (ownerCharacter.Removed)
						{
							return null;
						}
						return ownerCharacter.AnimController.Collider;
					}
				}
				Projectile projectile = targetItem.GetComponent<Projectile>();
				if (projectile != null && projectile.StickTarget != null)
				{
					object userData = projectile.StickTarget.UserData;
					Structure structure = userData as Structure;
					PhysicsBody result;
					if (structure == null)
					{
						Submarine sub = userData as Submarine;
						if (sub == null)
						{
							Item item = userData as Item;
							if (item == null)
							{
								Limb limb = userData as Limb;
								if (limb == null)
								{
									result = null;
								}
								else
								{
									result = limb.body;
								}
							}
							else
							{
								result = item.body;
							}
						}
						else
						{
							result = sub.PhysicsBody;
						}
					}
					else
					{
						Submarine submarine = structure.Submarine;
						result = ((submarine != null) ? submarine.PhysicsBody : null);
					}
					return result;
				}
				if (targetItem.body != null)
				{
					return targetItem.body;
				}
				if (targetItem.StaticFixtures.Any<Fixture>() && targetItem.Submarine != null)
				{
					return targetItem.Submarine.PhysicsBody;
				}
			}
			else
			{
				Limb targetLimb = target as Limb;
				if (targetLimb != null)
				{
					return targetLimb.body;
				}
			}
			return null;
		}

		// Token: 0x04001FF2 RID: 8178
		private ISpatialEntity source;

		// Token: 0x04001FF3 RID: 8179
		private Item target;

		// Token: 0x04001FF4 RID: 8180
		private Vector2? launchDir;

		// Token: 0x04001FF5 RID: 8181
		private float currentRopeLength;

		// Token: 0x04001FF6 RID: 8182
		private float snapTimer;

		// Token: 0x04001FF8 RID: 8184
		private float raycastTimer;

		// Token: 0x04001FF9 RID: 8185
		private const float RayCastInterval = 0.2f;

		// Token: 0x04002007 RID: 8199
		private bool isReelingIn;

		// Token: 0x04002008 RID: 8200
		private bool snapped;
	}
}
