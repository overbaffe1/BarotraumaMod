using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005EC RID: 1516
	internal class MeleeWeapon : Holdable
	{
		// Token: 0x17001919 RID: 6425
		// (get) Token: 0x06006344 RID: 25412 RVA: 0x0033B7CB File Offset: 0x003399CB
		// (set) Token: 0x06006345 RID: 25413 RVA: 0x0033B7D3 File Offset: 0x003399D3
		public Attack Attack { get; private set; }

		// Token: 0x1700191A RID: 6426
		// (get) Token: 0x06006346 RID: 25414 RVA: 0x0033B7DC File Offset: 0x003399DC
		// (set) Token: 0x06006347 RID: 25415 RVA: 0x0033B7E4 File Offset: 0x003399E4
		public Character User { get; private set; }

		// Token: 0x1700191B RID: 6427
		// (get) Token: 0x06006348 RID: 25416 RVA: 0x0033B7ED File Offset: 0x003399ED
		// (set) Token: 0x06006349 RID: 25417 RVA: 0x0033B7FA File Offset: 0x003399FA
		[Serialize(0f, IsPropertySaveable.No, "An estimation of how close the item has to be to the target for it to hit. Used by AI characters to determine when they're close enough to hit a target.", "", false)]
		public float Range
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.range);
			}
			set
			{
				this.range = ConvertUnits.ToSimUnits(value);
			}
		}

		// Token: 0x1700191C RID: 6428
		// (get) Token: 0x0600634A RID: 25418 RVA: 0x0033B808 File Offset: 0x00339A08
		// (set) Token: 0x0600634B RID: 25419 RVA: 0x0033B810 File Offset: 0x00339A10
		[Serialize(0.5f, IsPropertySaveable.No, "How long the user has to wait before they can hit with the weapon again (in seconds).", "", false)]
		public float Reload
		{
			get
			{
				return this.reload;
			}
			set
			{
				this.reload = Math.Max(0f, value);
			}
		}

		// Token: 0x1700191D RID: 6429
		// (get) Token: 0x0600634C RID: 25420 RVA: 0x0033B823 File Offset: 0x00339A23
		// (set) Token: 0x0600634D RID: 25421 RVA: 0x0033B82B File Offset: 0x00339A2B
		[Serialize(false, IsPropertySaveable.No, "Can the weapon hit multiple targets per swing.", "", false)]
		public bool AllowHitMultiple { get; set; }

		// Token: 0x1700191E RID: 6430
		// (get) Token: 0x0600634E RID: 25422 RVA: 0x0033B834 File Offset: 0x00339A34
		// (set) Token: 0x0600634F RID: 25423 RVA: 0x0033B83C File Offset: 0x00339A3C
		[Serialize(false, IsPropertySaveable.No, "Disable to make the weapon ignore all hit effects when it collides with walls, doors, or other items.", "", false)]
		public bool HitOnlyCharacters { get; set; }

		// Token: 0x1700191F RID: 6431
		// (get) Token: 0x06006350 RID: 25424 RVA: 0x0033B845 File Offset: 0x00339A45
		// (set) Token: 0x06006351 RID: 25425 RVA: 0x0033B84D File Offset: 0x00339A4D
		[Editable]
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool Swing { get; set; }

		// Token: 0x17001920 RID: 6432
		// (get) Token: 0x06006352 RID: 25426 RVA: 0x0033B856 File Offset: 0x00339A56
		// (set) Token: 0x06006353 RID: 25427 RVA: 0x0033B85E File Offset: 0x00339A5E
		[Editable]
		[Serialize("2.0, 0.0", IsPropertySaveable.No, "", "", false)]
		public Vector2 SwingPos { get; set; }

		// Token: 0x17001921 RID: 6433
		// (get) Token: 0x06006354 RID: 25428 RVA: 0x0033B867 File Offset: 0x00339A67
		// (set) Token: 0x06006355 RID: 25429 RVA: 0x0033B86F File Offset: 0x00339A6F
		[Editable]
		[Serialize("3.0, -1.0", IsPropertySaveable.No, "", "", false)]
		public Vector2 SwingForce { get; set; }

		// Token: 0x17001922 RID: 6434
		// (get) Token: 0x06006356 RID: 25430 RVA: 0x0033B878 File Offset: 0x00339A78
		public bool Hitting
		{
			get
			{
				return this.hitting;
			}
		}

		// Token: 0x06006357 RID: 25431 RVA: 0x0033B880 File Offset: 0x00339A80
		public MeleeWeapon(Item item, ContentXElement element) : base(item, element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("attack", StringComparison.OrdinalIgnoreCase))
				{
					this.Attack = new Attack(subElement, item.Name + ", MeleeWeapon", item)
					{
						DamageRange = ((item.body == null) ? 10f : ConvertUnits.ToDisplayUnits(item.body.GetMaxExtent()))
					};
				}
			}
			item.IsShootable = true;
			item.RequireAimToUse = element.Parent.GetAttributeBool("requireaimtouse", true);
			this.PreferredContainedItems = element.GetAttributeIdentifierArray("preferredcontaineditems", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x06006358 RID: 25432 RVA: 0x0033B978 File Offset: 0x00339B78
		public override void Equip(Character character)
		{
			base.Equip(character);
			this.reloadTimer = Math.Max(Math.Min(this.reload, 1f), this.reloadTimer);
			this.IsActive = true;
		}

		// Token: 0x06006359 RID: 25433 RVA: 0x0033B9AC File Offset: 0x00339BAC
		public override bool Use(float deltaTime, Character character = null)
		{
			if (character == null || this.reloadTimer > 0f)
			{
				return false;
			}
			if (!base.Item.RequireAimToUse && character.IsPlayer)
			{
				if (GUI.MouseOn == null)
				{
					if (!character.Inventory.visualSlots.Any((VisualSlot s) => s.MouseOn()) && !Inventory.DraggingItems.Any<Item>())
					{
						goto IL_6D;
					}
				}
				return false;
			}
			IL_6D:
			if ((base.Item.RequireAimToUse && !character.IsKeyDown(InputType.Aim)) || this.hitting)
			{
				return false;
			}
			foreach (Item heldItem in character.HeldItems)
			{
				MeleeWeapon otherWeapon = heldItem.GetComponent<MeleeWeapon>();
				if (otherWeapon != null && otherWeapon.hitting)
				{
					return false;
				}
			}
			this.SetUser(character);
			if (base.Item.RequireAimToUse && this.hitPos < 0.7853982f)
			{
				return false;
			}
			this.ActivateNearbySleepingCharacters();
			this.reloadTimer = this.reload;
			this.reloadTimer /= 1f + character.GetStatValue(StatTypes.MeleeAttackSpeed, true);
			this.reloadTimer /= 1f + this.item.GetQualityModifier(Quality.StatType.StrikingSpeedMultiplier);
			character.AnimController.LockFlipping(0.2f);
			this.item.body.FarseerBody.CollisionCategories = Category.Cat7;
			this.item.body.FarseerBody.CollidesWith = (Category.Cat1 | Category.Cat2 | Category.Cat6);
			this.item.body.FarseerBody.OnCollision += this.OnCollision;
			this.item.body.FarseerBody.IsBullet = true;
			this.item.body.PhysEnabled = true;
			if (this.Swing && !character.AnimController.InWater)
			{
				foreach (Limb i in character.AnimController.Limbs)
				{
					if (!i.IsSevered)
					{
						Vector2 force = new Vector2(character.AnimController.Dir * this.SwingForce.X, this.SwingForce.Y) * i.Mass;
						switch (i.type)
						{
						case LimbType.LeftLeg:
						case LimbType.RightLeg:
						case LimbType.LeftFoot:
						case LimbType.RightFoot:
						case LimbType.Legs:
						case LimbType.RightThigh:
						case LimbType.LeftThigh:
							force = Vector2.Zero;
							break;
						case LimbType.Torso:
							force *= 2f;
							break;
						}
						i.body.ApplyLinearImpulse(force);
					}
				}
			}
			this.hitting = true;
			this.hitTargets.Clear();
			this.IsActive = true;
			if (this.item.AiTarget != null)
			{
				this.item.AiTarget.SoundRange = this.item.AiTarget.MaxSoundRange;
				this.item.AiTarget.SightRange = this.item.AiTarget.MaxSightRange;
			}
			return false;
		}

		// Token: 0x0600635A RID: 25434 RVA: 0x0033BCE4 File Offset: 0x00339EE4
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			return this.characterUsable || character == null;
		}

		// Token: 0x0600635B RID: 25435 RVA: 0x0033BCF4 File Offset: 0x00339EF4
		public override void Drop(Character dropper, bool setTransform = true)
		{
			this.EndHit();
			this.item.body.PhysEnabled = true;
			base.Drop(dropper, setTransform);
		}

		// Token: 0x0600635C RID: 25436 RVA: 0x0033BD15 File Offset: 0x00339F15
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x0600635D RID: 25437 RVA: 0x0033BD20 File Offset: 0x00339F20
		public override void Update(float deltaTime, Camera cam)
		{
			if (!this.item.body.Enabled)
			{
				this.impactQueue.Clear();
				return;
			}
			if (this.picker == null || !this.picker.HeldItems.Contains(this.item))
			{
				this.impactQueue.Clear();
				this.IsActive = false;
			}
			while (this.impactQueue.Count > 0)
			{
				Fixture impact = this.impactQueue.Dequeue();
				this.HandleImpact(impact);
			}
			if (this.picker == null)
			{
				return;
			}
			this.reloadTimer -= deltaTime;
			if (this.reloadTimer < 0f)
			{
				this.reloadTimer = 0f;
			}
			if (!this.picker.IsKeyDown(InputType.Aim) && !this.hitting)
			{
				this.hitPos = 0f;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, this.picker, null, null, null, null, 1f);
			if (this.item.body.Dir != this.picker.AnimController.Dir)
			{
				this.item.FlipX(false, false);
			}
			AnimController ac = this.picker.AnimController;
			if (!this.hitting)
			{
				bool aim = this.item.RequireAimToUse && this.picker.AllowInput && this.picker.IsKeyDown(InputType.Aim) && this.reloadTimer <= 0f && this.picker.CanAim && !base.UsageDisabledByRangedWeapon(this.picker);
				if (!aim)
				{
					this.hitPos = 0f;
					ac.HoldItem(deltaTime, this.item, this.handlePos, this.holdPos, false, this.holdAngle, 0f, false, null);
					return;
				}
				Vector2 swingPos;
				base.UpdateSwingPos(deltaTime, out swingPos);
				this.hitPos = MathUtils.WrapAnglePi(Math.Min(this.hitPos + deltaTime * 3f, 0.7853982f));
				ac.HoldItem(deltaTime, this.item, this.handlePos, this.aimPos + swingPos, false, this.hitPos, this.holdAngle + this.hitPos + this.aimAngle, true, null);
				if (ac.InWater)
				{
					ac.LockFlipping(0.2f);
					return;
				}
			}
			else
			{
				this.hitPos -= deltaTime * 15f;
				if (this.Swing)
				{
					ac.HoldItem(deltaTime, this.item, this.handlePos, this.SwingPos, false, this.hitPos, this.holdAngle, false, null);
				}
				else
				{
					ac.HoldItem(deltaTime, this.item, this.handlePos, this.holdPos, false, this.holdAngle, 0f, false, null);
				}
				if (this.hitPos < -3.1415927f)
				{
					this.EndHit();
				}
			}
		}

		// Token: 0x0600635E RID: 25438 RVA: 0x0033C004 File Offset: 0x0033A204
		private void ActivateNearbySleepingCharacters()
		{
			foreach (Character c in Character.CharacterList)
			{
				if (c.Enabled && c.AnimController.BodyInRest && Math.Abs(c.WorldPosition.X - this.item.WorldPosition.X) <= 1000f && Math.Abs(c.WorldPosition.Y - this.item.WorldPosition.Y) <= 1000f)
				{
					foreach (Limb limb in c.AnimController.Limbs)
					{
						float hitRange = 2f;
						if (Vector2.DistanceSquared(limb.SimPosition, this.item.SimPosition) < hitRange * hitRange)
						{
							c.AnimController.BodyInRest = false;
							break;
						}
					}
				}
			}
		}

		// Token: 0x0600635F RID: 25439 RVA: 0x0033C114 File Offset: 0x0033A314
		private void SetUser(Character character)
		{
			if (this.User == character)
			{
				return;
			}
			if (this.User != null && this.User.Removed)
			{
				this.User = null;
			}
			this.User = character;
		}

		// Token: 0x06006360 RID: 25440 RVA: 0x0033C143 File Offset: 0x0033A343
		private void EndHit()
		{
			this.RestoreCollision();
			this.hitting = false;
			this.hitTargets.Clear();
			this.hitPos = 0f;
		}

		// Token: 0x06006361 RID: 25441 RVA: 0x0033C168 File Offset: 0x0033A368
		private void RestoreCollision()
		{
			this.impactQueue.Clear();
			this.item.body.FarseerBody.OnCollision -= this.OnCollision;
			this.item.body.CollisionCategories = Category.Cat5;
			this.item.body.CollidesWith = (Category.Cat1 | Category.Cat3 | Category.Cat8 | Category.Cat9);
			this.item.body.FarseerBody.IsBullet = false;
			this.item.body.PhysEnabled = false;
		}

		// Token: 0x06006362 RID: 25442 RVA: 0x0033C1F0 File Offset: 0x0033A3F0
		private bool OnCollision(Fixture f1, Fixture f2, Contact contact)
		{
			MeleeWeapon.<>c__DisplayClass55_0 CS$<>8__locals1 = new MeleeWeapon.<>c__DisplayClass55_0();
			CS$<>8__locals1.f2 = f2;
			CS$<>8__locals1.<>4__this = this;
			if (this.User == null || this.User.Removed)
			{
				this.impactQueue.Enqueue(CS$<>8__locals1.f2);
				return true;
			}
			Vector2 normal;
			FixedArray2<Vector2> points;
			contact.GetWorldManifold(out normal, out points);
			if (Submarine.PickBody(this.User.AnimController.AimSourceSimPos, points[0], null, new Category?(Category.Cat1 | Category.Cat6 | Category.Cat8), true, (Fixture fixture) => fixture.CollidesWith.HasFlag(Category.Cat5) && fixture.Body != CS$<>8__locals1.f2.Body, true) != null)
			{
				return false;
			}
			Limb targetLimb = CS$<>8__locals1.f2.Body.UserData as Limb;
			if (targetLimb != null)
			{
				if (targetLimb.IsSevered || targetLimb.character == null || targetLimb.character == this.User)
				{
					return false;
				}
				if (targetLimb.character.IgnoreMeleeWeapons)
				{
					return false;
				}
				Character targetCharacter = targetLimb.character;
				if (targetCharacter == this.picker)
				{
					return false;
				}
				if (CS$<>8__locals1.<OnCollision>g__HitFriendlyTarget|1(targetCharacter))
				{
					return false;
				}
				if (this.AllowHitMultiple)
				{
					if (this.hitTargets.Contains(targetCharacter))
					{
						return false;
					}
				}
				else if (this.hitTargets.Any((Entity t) => t is Character))
				{
					return false;
				}
				this.hitTargets.Add(targetCharacter);
			}
			else
			{
				Character targetCharacter2 = CS$<>8__locals1.f2.Body.UserData as Character;
				if (targetCharacter2 != null)
				{
					return false;
				}
				if (this.HitOnlyCharacters)
				{
					return false;
				}
				Structure targetStructure = (CS$<>8__locals1.f2.Body.UserData as Structure) ?? (CS$<>8__locals1.f2.UserData as Structure);
				if (targetStructure != null)
				{
					if (this.AllowHitMultiple)
					{
						if (this.hitTargets.Contains(targetStructure))
						{
							return true;
						}
					}
					else if (this.hitTargets.Any((Entity t) => t is Structure))
					{
						return true;
					}
					this.hitTargets.Add(targetStructure);
				}
				else
				{
					Item targetItem = (CS$<>8__locals1.f2.Body.UserData as Item) ?? (CS$<>8__locals1.f2.UserData as Item);
					if (targetItem != null)
					{
						if (this.AllowHitMultiple)
						{
							if (this.hitTargets.Contains(targetItem))
							{
								return true;
							}
						}
						else if (this.hitTargets.Any((Entity t) => t is Item))
						{
							return true;
						}
						this.hitTargets.Add(targetItem);
					}
					else
					{
						Holdable holdable = CS$<>8__locals1.f2.Body.UserData as Holdable;
						if (holdable != null && holdable.CanPush)
						{
							if (holdable.Item.GetRootInventoryOwner() == this.User)
							{
								return false;
							}
							this.hitTargets.Add(holdable.Item);
						}
					}
				}
			}
			this.impactQueue.Enqueue(CS$<>8__locals1.f2);
			return true;
		}

		// Token: 0x06006363 RID: 25443 RVA: 0x0033C4DC File Offset: 0x0033A6DC
		private void HandleImpact(Fixture targetFixture)
		{
			MeleeWeapon.<>c__DisplayClass57_0 CS$<>8__locals1 = new MeleeWeapon.<>c__DisplayClass57_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.target = targetFixture.Body;
			if (this.User == null || this.User.Removed || CS$<>8__locals1.target == null)
			{
				this.RestoreCollision();
				this.hitting = false;
				this.User = null;
				return;
			}
			float damageMultiplier = 1f + this.User.GetStatValue(StatTypes.MeleeAttackMultiplier, true);
			damageMultiplier *= 1f + this.item.GetQualityModifier(Quality.StatType.StrikingPowerMultiplier);
			CS$<>8__locals1.user = this.User;
			Limb targetLimb = CS$<>8__locals1.target.UserData as Limb;
			Character targetCharacter = ((targetLimb != null) ? targetLimb.character : null) ?? (CS$<>8__locals1.target.UserData as Character);
			Structure targetStructure = (CS$<>8__locals1.target.UserData as Structure) ?? (targetFixture.UserData as Structure);
			Holdable h = CS$<>8__locals1.target.UserData as Holdable;
			Item targetItem = (h != null) ? h.Item : ((CS$<>8__locals1.target.UserData as Item) ?? (targetFixture.UserData as Item));
			MeleeWeapon.<>c__DisplayClass57_0 CS$<>8__locals2 = CS$<>8__locals1;
			Character targetEntity;
			if ((targetEntity = targetCharacter) == null && (targetEntity = targetStructure) == null)
			{
				targetEntity = (targetItem ?? (CS$<>8__locals1.target.UserData as Entity));
			}
			CS$<>8__locals2.targetEntity = targetEntity;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventMeleeWeaponHandleImpact>(delegate(IEventMeleeWeaponHandleImpact x)
			{
				x.OnMeleeWeaponHandleImpact(CS$<>8__locals1.<>4__this, CS$<>8__locals1.target);
			});
			if (this.Attack != null)
			{
				this.Attack.SetUser(CS$<>8__locals1.user);
				bool applyAttack = true;
				if (this.Attack.Conditionals.Any((PropertyConditional c) => !c.TargetSelf && !c.Matches(CS$<>8__locals1.targetEntity as ISerializableEntity)) || this.Attack.Conditionals.Any((PropertyConditional c) => c.TargetSelf && !c.Matches(CS$<>8__locals1.user)))
				{
					applyAttack = false;
				}
				if (applyAttack)
				{
					this.Attack.DamageMultiplier = damageMultiplier;
					if (targetLimb != null)
					{
						if (targetLimb.character.Removed)
						{
							return;
						}
						targetLimb.character.LastDamageSource = this.item;
						this.Attack.DoDamageToLimb(CS$<>8__locals1.user, targetLimb, this.item.WorldPosition, 1f, true, null, null);
					}
					else if (targetCharacter != null)
					{
						if (targetCharacter.Removed)
						{
							return;
						}
						targetCharacter.LastDamageSource = this.item;
						this.Attack.DoDamage(CS$<>8__locals1.user, targetCharacter, this.item.WorldPosition, 1f, true, null, null);
					}
					else if (targetStructure != null)
					{
						if (targetStructure.Removed)
						{
							return;
						}
						this.Attack.DoDamage(CS$<>8__locals1.user, targetStructure, this.item.WorldPosition, 1f, true, null, null);
					}
					else if (targetItem != null && targetItem.Prefab.DamagedByMeleeWeapons && targetItem.Condition > 0f)
					{
						if (targetItem.Removed)
						{
							return;
						}
						if (this.Attack.DoDamage(CS$<>8__locals1.user, targetItem, this.item.WorldPosition, 1f, true, null, null).Damage > 0f && targetItem.Prefab.ShowHealthBar && Character.Controlled != null && (CS$<>8__locals1.user == Character.Controlled || Character.Controlled.CanSeeTarget(this.item, null, false, false)))
						{
							Character.Controlled.UpdateHUDProgressBar(targetItem, targetItem.WorldPosition, targetItem.Condition / targetItem.MaxCondition, GUIStyle.HealthBarColorLow, GUIStyle.HealthBarColorHigh, targetItem.Prefab.ShowNameInHealthBar ? targetItem.Name : string.Empty);
						}
					}
					else
					{
						Holdable holdable = CS$<>8__locals1.target.UserData as Holdable;
						if (holdable == null || !holdable.CanPush)
						{
							return;
						}
						if (holdable.Item.Removed)
						{
							return;
						}
						this.RestoreCollision();
						this.hitting = false;
						this.User = null;
					}
				}
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			ActionType conditionalActionType = ActionType.OnSuccess;
			if (CS$<>8__locals1.user != null && Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) > base.DegreeOfSuccess(CS$<>8__locals1.user))
			{
				conditionalActionType = ActionType.OnFailure;
			}
			NetworkMember server = GameMain.NetworkMember;
			if (server != null && server.IsServer && CS$<>8__locals1.targetEntity != null)
			{
				server.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(conditionalActionType, this, targetCharacter, targetLimb, CS$<>8__locals1.targetEntity, null));
				server.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(ActionType.OnUse, this, targetCharacter, targetLimb, CS$<>8__locals1.targetEntity, null));
				if (this.serverLogger == null)
				{
					this.serverLogger = new StringBuilder();
				}
				this.serverLogger.Clear();
				StringBuilder stringBuilder = this.serverLogger;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder);
				Character picker = this.picker;
				appendInterpolatedStringHandler.AppendFormatted((picker != null) ? picker.LogName : null);
				appendInterpolatedStringHandler.AppendLiteral(" used ");
				appendInterpolatedStringHandler.AppendFormatted(this.item.Name);
				stringBuilder2.Append(ref appendInterpolatedStringHandler);
				if (this.item.ContainedItems != null && this.item.ContainedItems.Any<Item>())
				{
					stringBuilder = this.serverLogger;
					StringBuilder stringBuilder3 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("(");
					appendInterpolatedStringHandler.AppendFormatted(string.Join(", ", this.item.ContainedItems.Select(delegate(Item i)
					{
						if (i == null)
						{
							return null;
						}
						return i.Name;
					})));
					appendInterpolatedStringHandler.AppendLiteral(")");
					stringBuilder3.Append(ref appendInterpolatedStringHandler);
				}
				string targetName;
				if (targetCharacter != null)
				{
					targetName = targetCharacter.LogName;
				}
				else if (targetItem != null)
				{
					targetName = targetItem.Name;
				}
				else if (targetStructure != null)
				{
					targetName = targetStructure.Name;
				}
				else
				{
					targetName = CS$<>8__locals1.targetEntity.ToString();
				}
				stringBuilder = this.serverLogger;
				StringBuilder stringBuilder4 = stringBuilder;
				appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral(" on ");
				appendInterpolatedStringHandler.AppendFormatted(targetName);
				appendInterpolatedStringHandler.AppendLiteral(".");
				stringBuilder4.Append(ref appendInterpolatedStringHandler);
			}
			if (CS$<>8__locals1.targetEntity != null)
			{
				ActionType type = conditionalActionType;
				float deltaTime = 1f;
				Character character = targetCharacter;
				Limb targetLimb2 = targetLimb;
				Entity targetEntity2 = CS$<>8__locals1.targetEntity;
				Character user = CS$<>8__locals1.user;
				float attackMultiplier = damageMultiplier;
				base.ApplyStatusEffects(type, deltaTime, character, targetLimb2, targetEntity2, user, null, attackMultiplier);
				ActionType type2 = ActionType.OnUse;
				float deltaTime2 = 1f;
				Character character2 = targetCharacter;
				Limb targetLimb3 = targetLimb;
				Entity targetEntity3 = CS$<>8__locals1.targetEntity;
				Character user2 = CS$<>8__locals1.user;
				attackMultiplier = damageMultiplier;
				base.ApplyStatusEffects(type2, deltaTime2, character2, targetLimb3, targetEntity3, user2, null, attackMultiplier);
			}
			if (base.DeleteOnUse)
			{
				Entity.Spawner.AddItemToRemoveQueue(this.item);
			}
		}

		// Token: 0x0400337D RID: 13181
		private float hitPos;

		// Token: 0x0400337E RID: 13182
		private bool hitting;

		// Token: 0x0400337F RID: 13183
		private float range;

		// Token: 0x04003380 RID: 13184
		private float reload;

		// Token: 0x04003381 RID: 13185
		private float reloadTimer;

		// Token: 0x04003383 RID: 13187
		private readonly HashSet<Entity> hitTargets = new HashSet<Entity>();

		// Token: 0x04003384 RID: 13188
		private readonly Queue<Fixture> impactQueue = new Queue<Fixture>();

		// Token: 0x0400338B RID: 13195
		public readonly ImmutableHashSet<Identifier> PreferredContainedItems;

		// Token: 0x0400338C RID: 13196
		private StringBuilder serverLogger;
	}
}
