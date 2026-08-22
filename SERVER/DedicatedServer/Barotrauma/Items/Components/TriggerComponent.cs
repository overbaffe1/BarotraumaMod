using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004B0 RID: 1200
	internal class TriggerComponent : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x060043F5 RID: 17397 RVA: 0x001B43AE File Offset: 0x001B25AE
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedSingle(this.CurrentForceFluctuation, 0f, 1f, 8);
		}

		// Token: 0x1700120F RID: 4623
		// (get) Token: 0x060043F6 RID: 17398 RVA: 0x001B43C7 File Offset: 0x001B25C7
		// (set) Token: 0x060043F7 RID: 17399 RVA: 0x001B43CF File Offset: 0x001B25CF
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "The maximum amount of force applied to the triggering entitites.", "", true)]
		public float Force { get; set; }

		// Token: 0x17001210 RID: 4624
		// (get) Token: 0x060043F8 RID: 17400 RVA: 0x001B43D8 File Offset: 0x001B25D8
		// (set) Token: 0x060043F9 RID: 17401 RVA: 0x001B43E0 File Offset: 0x001B25E0
		[Editable]
		[Serialize("0,0", IsPropertySaveable.Yes, "The maximum amount of directional force applied to the triggering entitites.", "", true)]
		public Vector2 DirectionalForce { get; set; }

		// Token: 0x17001211 RID: 4625
		// (get) Token: 0x060043FA RID: 17402 RVA: 0x001B43E9 File Offset: 0x001B25E9
		// (set) Token: 0x060043FB RID: 17403 RVA: 0x001B43F1 File Offset: 0x001B25F1
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If true, DirectionalForce is relative to the angle between the target and the item, Similar to Force.\nIf false, it always pushes in the same direction, with respect to the item's rotation.", "", true)]
		public bool RelativeDirectionalForce { get; set; }

		// Token: 0x17001212 RID: 4626
		// (get) Token: 0x060043FC RID: 17404 RVA: 0x001B43FA File Offset: 0x001B25FA
		// (set) Token: 0x060043FD RID: 17405 RVA: 0x001B4402 File Offset: 0x001B2602
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "If false, no vertical force will be applied.", "", true)]
		public bool VerticalForce { get; set; }

		// Token: 0x17001213 RID: 4627
		// (get) Token: 0x060043FE RID: 17406 RVA: 0x001B440B File Offset: 0x001B260B
		// (set) Token: 0x060043FF RID: 17407 RVA: 0x001B4413 File Offset: 0x001B2613
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "If false, no horizontal force will be applied.", "", true)]
		public bool HorizontalForce { get; set; }

		// Token: 0x17001214 RID: 4628
		// (get) Token: 0x06004400 RID: 17408 RVA: 0x001B441C File Offset: 0x001B261C
		// (set) Token: 0x06004401 RID: 17409 RVA: 0x001B4424 File Offset: 0x001B2624
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Determines if the force gets higher the closer the triggerer is to the center of the trigger.", "", true)]
		public bool DistanceBasedForce { get; set; }

		// Token: 0x17001215 RID: 4629
		// (get) Token: 0x06004402 RID: 17410 RVA: 0x001B442D File Offset: 0x001B262D
		// (set) Token: 0x06004403 RID: 17411 RVA: 0x001B4435 File Offset: 0x001B2635
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Determines if the force fluctuates over time or if it stays constant.", "", true)]
		public bool ForceFluctuation { get; set; }

		// Token: 0x17001216 RID: 4630
		// (get) Token: 0x06004404 RID: 17412 RVA: 0x001B443E File Offset: 0x001B263E
		// (set) Token: 0x06004405 RID: 17413 RVA: 0x001B4446 File Offset: 0x001B2646
		[Serialize(1f, IsPropertySaveable.Yes, "How much the fluctuation affects the force. 1 is the maximum fluctuation, 0 is no fluctuation.", "", true)]
		public float ForceFluctuationStrength
		{
			get
			{
				return this.forceFluctuationStrength;
			}
			set
			{
				this.forceFluctuationStrength = Math.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17001217 RID: 4631
		// (get) Token: 0x06004406 RID: 17414 RVA: 0x001B445E File Offset: 0x001B265E
		// (set) Token: 0x06004407 RID: 17415 RVA: 0x001B4466 File Offset: 0x001B2666
		[Serialize(1f, IsPropertySaveable.Yes, "How fast (cycles per second) the force fluctuates.", "", true)]
		public float ForceFluctuationFrequency
		{
			get
			{
				return this.forceFluctuationFrequency;
			}
			set
			{
				this.forceFluctuationFrequency = Math.Max(value, 0.01f);
			}
		}

		// Token: 0x17001218 RID: 4632
		// (get) Token: 0x06004408 RID: 17416 RVA: 0x001B4479 File Offset: 0x001B2679
		// (set) Token: 0x06004409 RID: 17417 RVA: 0x001B4481 File Offset: 0x001B2681
		[Serialize(0.01f, IsPropertySaveable.Yes, "How often (in seconds) the force fluctuation is calculated.", "", true)]
		public float ForceFluctuationInterval
		{
			get
			{
				return this.forceFluctuationInterval;
			}
			set
			{
				this.forceFluctuationInterval = Math.Max(value, 0.01f);
			}
		}

		// Token: 0x17001219 RID: 4633
		// (get) Token: 0x0600440A RID: 17418 RVA: 0x001B4494 File Offset: 0x001B2694
		// (set) Token: 0x0600440B RID: 17419 RVA: 0x001B449C File Offset: 0x001B269C
		public PhysicsBody PhysicsBody { get; private set; }

		// Token: 0x1700121A RID: 4634
		// (get) Token: 0x0600440C RID: 17420 RVA: 0x001B44A5 File Offset: 0x001B26A5
		// (set) Token: 0x0600440D RID: 17421 RVA: 0x001B44AD File Offset: 0x001B26AD
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Radius
		{
			get
			{
				return this.radius;
			}
			set
			{
				if (this.radius == value)
				{
					return;
				}
				this.radius = value;
				if (this.PhysicsBody != null)
				{
					this.RefreshPhysicsBodySize();
				}
			}
		}

		// Token: 0x1700121B RID: 4635
		// (get) Token: 0x0600440E RID: 17422 RVA: 0x001B44CE File Offset: 0x001B26CE
		// (set) Token: 0x0600440F RID: 17423 RVA: 0x001B44D6 File Offset: 0x001B26D6
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Width
		{
			get
			{
				return this.width;
			}
			set
			{
				if (this.width == value)
				{
					return;
				}
				this.width = value;
				if (this.PhysicsBody != null)
				{
					this.RefreshPhysicsBodySize();
				}
			}
		}

		// Token: 0x1700121C RID: 4636
		// (get) Token: 0x06004410 RID: 17424 RVA: 0x001B44F7 File Offset: 0x001B26F7
		// (set) Token: 0x06004411 RID: 17425 RVA: 0x001B44FF File Offset: 0x001B26FF
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Height
		{
			get
			{
				return this.height;
			}
			set
			{
				if (this.height == value)
				{
					return;
				}
				this.height = value;
				if (this.PhysicsBody != null)
				{
					this.RefreshPhysicsBodySize();
				}
			}
		}

		// Token: 0x1700121D RID: 4637
		// (get) Token: 0x06004412 RID: 17426 RVA: 0x001B4520 File Offset: 0x001B2720
		// (set) Token: 0x06004413 RID: 17427 RVA: 0x001B4528 File Offset: 0x001B2728
		[Editable]
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		public Vector2 BodyOffset
		{
			get
			{
				return this.bodyOffset;
			}
			set
			{
				if (this.bodyOffset == value)
				{
					return;
				}
				this.bodyOffset = value;
				if (this.PhysicsBody != null)
				{
					this.SetPhysicsBodyPosition(true);
				}
			}
		}

		// Token: 0x1700121E RID: 4638
		// (get) Token: 0x06004414 RID: 17428 RVA: 0x001B454F File Offset: 0x001B274F
		// (set) Token: 0x06004415 RID: 17429 RVA: 0x001B4557 File Offset: 0x001B2757
		private float RadiusInDisplayUnits { get; set; }

		// Token: 0x1700121F RID: 4639
		// (get) Token: 0x06004416 RID: 17430 RVA: 0x001B4560 File Offset: 0x001B2760
		// (set) Token: 0x06004417 RID: 17431 RVA: 0x001B4568 File Offset: 0x001B2768
		private bool TriggeredOnce { get; set; }

		// Token: 0x17001220 RID: 4640
		// (get) Token: 0x06004418 RID: 17432 RVA: 0x001B4571 File Offset: 0x001B2771
		// (set) Token: 0x06004419 RID: 17433 RVA: 0x001B4579 File Offset: 0x001B2779
		private float CurrentForceFluctuation { get; set; } = 1f;

		// Token: 0x17001221 RID: 4641
		// (get) Token: 0x0600441A RID: 17434 RVA: 0x001B4582 File Offset: 0x001B2782
		// (set) Token: 0x0600441B RID: 17435 RVA: 0x001B458A File Offset: 0x001B278A
		public bool TriggerActive { get; private set; }

		// Token: 0x17001222 RID: 4642
		// (get) Token: 0x0600441C RID: 17436 RVA: 0x001B4593 File Offset: 0x001B2793
		// (set) Token: 0x0600441D RID: 17437 RVA: 0x001B459B File Offset: 0x001B279B
		private float ForceFluctuationTimer { get; set; }

		// Token: 0x17001223 RID: 4643
		// (get) Token: 0x0600441E RID: 17438 RVA: 0x001B45A4 File Offset: 0x001B27A4
		private static float TimeInLevel
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession == null)
				{
					return 0f;
				}
				return gameSession.RoundDuration;
			}
		}

		// Token: 0x17001224 RID: 4644
		// (get) Token: 0x0600441F RID: 17439 RVA: 0x001B45BA File Offset: 0x001B27BA
		// (set) Token: 0x06004420 RID: 17440 RVA: 0x001B45C2 File Offset: 0x001B27C2
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool ApplyEffectsToCharactersInsideSub { get; set; }

		// Token: 0x17001225 RID: 4645
		// (get) Token: 0x06004421 RID: 17441 RVA: 0x001B45CB File Offset: 0x001B27CB
		// (set) Token: 0x06004422 RID: 17442 RVA: 0x001B45D3 File Offset: 0x001B27D3
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool MoveOutsideSub { get; set; }

		// Token: 0x17001226 RID: 4646
		// (get) Token: 0x06004423 RID: 17443 RVA: 0x001B45DC File Offset: 0x001B27DC
		// (set) Token: 0x06004424 RID: 17444 RVA: 0x001B45E4 File Offset: 0x001B27E4
		public override bool IsActive
		{
			get
			{
				return base.IsActive;
			}
			set
			{
				bool wasActive = base.IsActive;
				base.IsActive = value;
				if (!this.IsActive)
				{
					this.TriggerActive = false;
					this.triggerers.Clear();
					return;
				}
				if (!wasActive)
				{
					PhysicsBody physicsBody = this.PhysicsBody;
					if (((physicsBody != null) ? physicsBody.FarseerBody : null) != null)
					{
						ContactEdge ce = this.PhysicsBody.FarseerBody.ContactList;
						while (ce != null && ce.Contact != null)
						{
							if (ce.Contact.Enabled)
							{
								Fixture thisFixture = (ce.Contact.FixtureA.Body == this.PhysicsBody.FarseerBody) ? ce.Contact.FixtureA : ce.Contact.FixtureB;
								Fixture otherFixture = (ce.Contact.FixtureA.Body == this.PhysicsBody.FarseerBody) ? ce.Contact.FixtureB : ce.Contact.FixtureA;
								this.OnCollision(thisFixture, otherFixture, ce.Contact);
							}
							ce = ce.Next;
						}
					}
				}
			}
		}

		// Token: 0x06004425 RID: 17445 RVA: 0x001B46EC File Offset: 0x001B28EC
		public TriggerComponent(Item item, ContentXElement element) : base(item, element)
		{
			string triggeredByString = element.GetAttributeString("triggeredby", "Character");
			if (!Enum.TryParse<LevelTrigger.TriggererType>(triggeredByString, out this.triggeredBy))
			{
				Identifier speciesOrGroup = triggeredByString.ToIdentifier();
				if (CharacterPrefab.Prefabs.Any((CharacterPrefab p) => p.MatchesSpeciesNameOrGroup(speciesOrGroup)))
				{
					this.triggerSpeciesOrGroup = speciesOrGroup;
					this.triggeredBy = LevelTrigger.TriggererType.Character;
				}
				else
				{
					DebugConsole.ThrowError("Error in ForceComponent config: \"" + triggeredByString + "\" is not a valid triggerer type.", null, element.ContentPackage, false, false);
				}
			}
			this.triggerOnce = element.GetAttributeBool("triggeronce", false);
			string parentDebugName = "TriggerComponent in " + item.Name;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "statuseffect"))
				{
					if (a == "attack" || a == "damage")
					{
						LevelTrigger.LoadAttack(subElement, parentDebugName, this.triggerOnce, this.attacks);
					}
				}
				else
				{
					LevelTrigger.LoadStatusEffect(this.statusEffects, subElement, parentDebugName);
				}
			}
			this.conditionals = PropertyConditional.LoadConditionals(element, PropertyConditional.LogicalOperatorType.And);
			this.IsActive = true;
		}

		// Token: 0x06004426 RID: 17446 RVA: 0x001B4888 File Offset: 0x001B2A88
		public override void OnItemLoaded()
		{
			this.RefreshPhysicsBodySize();
		}

		// Token: 0x06004427 RID: 17447 RVA: 0x001B4890 File Offset: 0x001B2A90
		private void RefreshPhysicsBodySize()
		{
			PhysicsBody physicsBody = this.PhysicsBody;
			if (physicsBody != null)
			{
				physicsBody.Remove();
			}
			this.currentWidth = ConvertUnits.ToSimUnits(this.Width * this.item.Scale);
			this.currentHeight = ConvertUnits.ToSimUnits(this.Height * this.item.Scale);
			if (this.currentWidth > 0f && this.currentHeight > 0f)
			{
				this.PhysicsBody = new PhysicsBody(this.currentWidth, this.currentHeight, 0f, 1.5f, BodyType.Static, Category.Cat1, LevelTrigger.GetCollisionCategories(this.triggeredBy), true)
				{
					UserData = this
				};
			}
			else
			{
				this.currentRadius = Math.Max(ConvertUnits.ToSimUnits(this.Radius * this.item.Scale), 0.01f);
				this.PhysicsBody = new PhysicsBody(0f, 0f, this.currentRadius, 1.5f, BodyType.Static, Category.Cat1, LevelTrigger.GetCollisionCategories(this.triggeredBy), true)
				{
					UserData = this
				};
			}
			this.SetPhysicsBodyPosition(true);
			this.PhysicsBody.FarseerBody.SetIsSensor(this.originalElement.GetAttributeBool("sensor", true));
			this.PhysicsBody.FarseerBody.OnCollision += this.OnCollision;
			this.PhysicsBody.FarseerBody.OnSeparation += this.OnSeparation;
			this.RadiusInDisplayUnits = ConvertUnits.ToDisplayUnits(this.PhysicsBody.Radius);
		}

		// Token: 0x06004428 RID: 17448 RVA: 0x001B4A0C File Offset: 0x001B2C0C
		public void SetPhysicsBodyPosition(bool ignoreContacts = true)
		{
			if (this.PhysicsBody == null)
			{
				return;
			}
			Vector2 offset = ConvertUnits.ToSimUnits(this.BodyOffset * this.item.Scale);
			if (this.item.FlippedX)
			{
				offset.X = -offset.X;
			}
			if (this.item.FlippedY)
			{
				offset.Y = -offset.Y;
			}
			if (!MathUtils.NearlyEqual(this.item.RotationRad, 0f, 0.0001f))
			{
				Matrix transform = Matrix.CreateRotationZ(-this.item.RotationRad);
				offset = Vector2.Transform(offset, transform);
			}
			if (ignoreContacts)
			{
				this.PhysicsBody.SetTransformIgnoreContacts(this.item.SimPosition + offset, -this.item.RotationRad, true);
			}
			else
			{
				this.PhysicsBody.SetTransform(this.item.SimPosition + offset, -this.item.RotationRad, true);
			}
			this.PhysicsBody.UpdateDrawPosition(true);
		}

		// Token: 0x06004429 RID: 17449 RVA: 0x001B4B0F File Offset: 0x001B2D0F
		public override void FlipX(bool relativeToSub)
		{
			this.SetPhysicsBodyPosition(true);
		}

		// Token: 0x0600442A RID: 17450 RVA: 0x001B4B18 File Offset: 0x001B2D18
		public override void FlipY(bool relativeToSub)
		{
			this.SetPhysicsBodyPosition(true);
		}

		// Token: 0x0600442B RID: 17451 RVA: 0x001B4B21 File Offset: 0x001B2D21
		public override void OnMapLoaded()
		{
			base.OnMapLoaded();
			this.SetPhysicsBodyPosition(true);
			this.PhysicsBody.Submarine = this.item.Submarine;
		}

		// Token: 0x0600442C RID: 17452 RVA: 0x001B4B48 File Offset: 0x001B2D48
		private bool OnCollision(Fixture sender, Fixture other, Contact contact)
		{
			Entity entity = LevelTrigger.GetEntity(other);
			if (entity == null)
			{
				return false;
			}
			if (!LevelTrigger.IsTriggeredByEntity(entity, this.triggeredBy, this.triggerSpeciesOrGroup, this.conditionals, new ValueTuple<bool, Submarine>(!this.MoveOutsideSub, this.item.Submarine), false))
			{
				return false;
			}
			this.triggerers.Add(entity);
			return true;
		}

		// Token: 0x0600442D RID: 17453 RVA: 0x001B4BA8 File Offset: 0x001B2DA8
		private void OnSeparation(Fixture sender, Fixture other, Contact contact)
		{
			Entity entity = LevelTrigger.GetEntity(other);
			if (entity == null)
			{
				return;
			}
			Character character = entity as Character;
			if (character != null && (!character.Enabled || character.Removed) && this.triggerers.Contains(entity))
			{
				this.triggerers.Remove(entity);
				return;
			}
			if (LevelTrigger.CheckContactsForOtherFixtures(this.PhysicsBody, other, entity))
			{
				return;
			}
			this.triggerers.Remove(entity);
		}

		// Token: 0x0600442E RID: 17454 RVA: 0x001B4C14 File Offset: 0x001B2E14
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.item.Submarine != null && this.MoveOutsideSub)
			{
				this.item.SetTransform(ConvertUnits.ToSimUnits(this.item.WorldPosition), this.item.Rotation, true, true, null);
				this.item.CurrentHull = null;
				this.item.Submarine = null;
				this.SetPhysicsBodyPosition(true);
				this.PhysicsBody.Submarine = this.item.Submarine;
			}
			else
			{
				PhysicsBody body = this.item.body;
				if (body != null && body.BodyType == BodyType.Dynamic)
				{
					this.SetPhysicsBodyPosition(true);
					this.PhysicsBody.Submarine = this.item.Submarine;
				}
			}
			LevelTrigger.RemoveInActiveTriggerers(this.PhysicsBody, this.triggerers);
			if (this.triggerOnce)
			{
				if (this.TriggeredOnce)
				{
					return;
				}
				if (this.triggerers.Count > 0)
				{
					this.TriggeredOnce = true;
					this.IsActive = false;
				}
			}
			this.TriggerActive = this.triggerers.Any<Entity>();
			if (this.TriggerActive && this.conditionals != null)
			{
				PropertyConditional.LogicalOperatorType logicalOperator = this.conditionals.LogicalOperator;
				if (logicalOperator != PropertyConditional.LogicalOperatorType.And)
				{
					if (logicalOperator == PropertyConditional.LogicalOperatorType.Or)
					{
						if (this.triggerers.None((Entity t) => !PropertyConditional.CheckConditionals((ISerializableEntity)t, this.conditionals.Conditionals, this.conditionals.LogicalOperator)))
						{
							this.IsActive = false;
						}
					}
				}
				else if (this.triggerers.Any((Entity t) => !PropertyConditional.CheckConditionals((ISerializableEntity)t, this.conditionals.Conditionals, this.conditionals.LogicalOperator)))
				{
					this.IsActive = false;
				}
			}
			if (this.ForceFluctuation && this.TriggerActive && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer))
			{
				this.ForceFluctuationTimer += deltaTime;
				if (this.ForceFluctuationTimer >= this.ForceFluctuationInterval)
				{
					float v = MathF.Sin(6.2831855f * this.ForceFluctuationFrequency * TriggerComponent.TimeInLevel);
					float amount = MathUtils.InverseLerp(-1f, 1f, v);
					this.CurrentForceFluctuation = MathHelper.Lerp(1f - this.ForceFluctuationStrength, 1f, amount);
					this.ForceFluctuationTimer = 0f;
					this.item.CreateServerEvent<TriggerComponent>(this);
				}
			}
			foreach (Entity triggerer in this.triggerers)
			{
				LevelTrigger.ApplyStatusEffects(this.statusEffects, this.item.WorldPosition, triggerer, deltaTime, this.statusEffectTargets, base.Item);
				IDamageable damageable = triggerer as IDamageable;
				if (damageable != null)
				{
					LevelTrigger.ApplyAttacks(this.attacks, damageable, this.item.WorldPosition, deltaTime);
				}
				else
				{
					Submarine submarine = triggerer as Submarine;
					if (submarine != null)
					{
						LevelTrigger.ApplyAttacks(this.attacks, this.item.WorldPosition, deltaTime);
						foreach (Character c2 in Character.CharacterList)
						{
							if (c2.Submarine == submarine)
							{
								LevelTrigger.ApplyAttacks(this.attacks, c2, this.item.WorldPosition, deltaTime);
							}
						}
					}
				}
				if (this.Force >= 0.01f || this.DirectionalForce.LengthSquared() >= 0.0001f)
				{
					Character c3 = triggerer as Character;
					if (c3 != null)
					{
						if (c3.AnimController.Collider.BodyType == BodyType.Dynamic)
						{
							if (c3.AnimController.Collider.Enabled)
							{
								this.ApplyForce(c3.AnimController.Collider, 1f);
							}
							foreach (Limb limb in c3.AnimController.Limbs)
							{
								this.ApplyForce(limb.body, limb.Mass * c3.AnimController.Collider.Mass / c3.AnimController.Mass);
							}
						}
					}
					else
					{
						Submarine s = triggerer as Submarine;
						if (s != null)
						{
							this.ApplyForce(s.SubBody.Body, 1f);
						}
						else
						{
							Item i = triggerer as Item;
							if (i != null && i.body != null)
							{
								this.ApplyForce(i.body, 1f);
							}
						}
					}
				}
			}
			this.item.SendSignal(this.IsActive ? "1" : "0", "state_out");
		}

		// Token: 0x0600442F RID: 17455 RVA: 0x001B509C File Offset: 0x001B329C
		private void ApplyForce(PhysicsBody body, float multiplier = 1f)
		{
			Vector2 diff = ConvertUnits.ToDisplayUnits(this.item.SimPosition - body.SimPosition);
			if (diff.LengthSquared() < 0.0001f)
			{
				return;
			}
			float distanceFactor = this.DistanceBasedForce ? LevelTrigger.GetDistanceFactor(body, this.PhysicsBody, this.RadiusInDisplayUnits) : 1f;
			if (distanceFactor <= 0f)
			{
				return;
			}
			Vector2 radialForce = this.Force * Vector2.Normalize(diff);
			Vector2 directionalForce;
			if (this.RelativeDirectionalForce)
			{
				directionalForce = this.DirectionalForce * new Vector2((float)Math.Sign(diff.X), (float)Math.Sign(diff.Y));
			}
			else
			{
				Vector2 flippedForce = this.DirectionalForce;
				if (this.item.FlippedX)
				{
					flippedForce.X = -flippedForce.X;
				}
				if (this.item.FlippedY)
				{
					flippedForce.Y = -flippedForce.Y;
				}
				directionalForce = MathUtils.RotatePoint(flippedForce, -this.item.RotationRad);
			}
			Vector2 force = (radialForce + directionalForce) * this.CurrentForceFluctuation * distanceFactor * multiplier;
			if (!this.HorizontalForce)
			{
				force.Y = 0f;
			}
			if (!this.VerticalForce)
			{
				force.Y = 0f;
			}
			if (force.LengthSquared() < 0.01f)
			{
				return;
			}
			if (body.Mass < 1f)
			{
				force *= body.Mass;
			}
			body.ApplyForce(force, 64f);
		}

		// Token: 0x06004430 RID: 17456 RVA: 0x001B5218 File Offset: 0x001B3418
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			if (this.PhysicsBody != null)
			{
				this.SetPhysicsBodyPosition(ignoreContacts);
				this.PhysicsBody.Submarine = this.item.Submarine;
			}
		}

		// Token: 0x06004431 RID: 17457 RVA: 0x001B523F File Offset: 0x001B343F
		protected override void RemoveComponentSpecific()
		{
			if (this.PhysicsBody != null)
			{
				this.PhysicsBody.Remove();
				this.PhysicsBody = null;
			}
		}

		// Token: 0x06004432 RID: 17458 RVA: 0x001B525C File Offset: 0x001B345C
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			base.ReceiveSignal(signal, connection);
			string name = connection.Name;
			float force;
			if (!(name == "set_force"))
			{
				bool distanceBasedForce;
				if (!(name == "set_distancebasedforce"))
				{
					bool forceFluctuation;
					if (!(name == "set_forcefluctuation"))
					{
						float forceFluctuationStrength;
						if (!(name == "set_forcefluctuationstrength"))
						{
							float forceFluctuationFrequency;
							if (!(name == "set_forcefluctuationfrequency"))
							{
								if (!(name == "set_forcefluctuationinterval"))
								{
									return;
								}
								float forceFluctuationInterval;
								if (TriggerComponent.<ReceiveSignal>g__FloatTryParse|118_0(signal, out forceFluctuationInterval))
								{
									this.ForceFluctuationInterval = forceFluctuationInterval;
								}
							}
							else if (TriggerComponent.<ReceiveSignal>g__FloatTryParse|118_0(signal, out forceFluctuationFrequency))
							{
								this.ForceFluctuationFrequency = forceFluctuationFrequency;
								return;
							}
						}
						else if (TriggerComponent.<ReceiveSignal>g__FloatTryParse|118_0(signal, out forceFluctuationStrength))
						{
							this.ForceFluctuationStrength = forceFluctuationStrength;
							return;
						}
					}
					else if (bool.TryParse(signal.value, out forceFluctuation))
					{
						this.ForceFluctuation = forceFluctuation;
						return;
					}
				}
				else if (bool.TryParse(signal.value, out distanceBasedForce))
				{
					this.DistanceBasedForce = distanceBasedForce;
					return;
				}
			}
			else if (TriggerComponent.<ReceiveSignal>g__FloatTryParse|118_0(signal, out force))
			{
				this.Force = force;
				return;
			}
		}

		// Token: 0x06004435 RID: 17461 RVA: 0x001B539B File Offset: 0x001B359B
		[CompilerGenerated]
		internal static bool <ReceiveSignal>g__FloatTryParse|118_0(Signal signal, out float value)
		{
			return float.TryParse(signal.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
		}

		// Token: 0x0400207F RID: 8319
		private float radius;

		// Token: 0x04002080 RID: 8320
		private float width;

		// Token: 0x04002081 RID: 8321
		private float height;

		// Token: 0x04002082 RID: 8322
		private float currentRadius;

		// Token: 0x04002083 RID: 8323
		private float currentWidth;

		// Token: 0x04002084 RID: 8324
		private float currentHeight;

		// Token: 0x04002085 RID: 8325
		private Vector2 bodyOffset;

		// Token: 0x0400208D RID: 8333
		private readonly LevelTrigger.TriggererType triggeredBy;

		// Token: 0x0400208E RID: 8334
		private readonly Identifier triggerSpeciesOrGroup;

		// Token: 0x0400208F RID: 8335
		private readonly PropertyConditional.LogicalComparison conditionals;

		// Token: 0x04002090 RID: 8336
		private readonly HashSet<Entity> triggerers = new HashSet<Entity>();

		// Token: 0x04002091 RID: 8337
		private readonly bool triggerOnce;

		// Token: 0x04002092 RID: 8338
		private readonly List<ISerializableEntity> statusEffectTargets = new List<ISerializableEntity>();

		// Token: 0x04002093 RID: 8339
		private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();

		// Token: 0x04002094 RID: 8340
		private readonly List<Attack> attacks = new List<Attack>();

		// Token: 0x04002095 RID: 8341
		private float forceFluctuationStrength;

		// Token: 0x04002096 RID: 8342
		private float forceFluctuationFrequency;

		// Token: 0x04002097 RID: 8343
		private float forceFluctuationInterval;
	}
}
