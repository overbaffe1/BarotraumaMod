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
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005E2 RID: 1506
	internal class TriggerComponent : ItemComponent, IServerSerializable, INetSerializable, IDrawableComponent
	{
		// Token: 0x170018AF RID: 6319
		// (get) Token: 0x060061E6 RID: 25062 RVA: 0x0032F33F File Offset: 0x0032D53F
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.One * ((this.Radius > 0f) ? (this.Radius * 2f) : Math.Max(this.Width, this.Height));
			}
		}

		// Token: 0x060061E7 RID: 25063 RVA: 0x0032F377 File Offset: 0x0032D577
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (editing)
			{
				this.PhysicsBody.DebugDraw(spriteBatch, Color.LightGray * 0.7f, false);
			}
		}

		// Token: 0x060061E8 RID: 25064 RVA: 0x0032F398 File Offset: 0x0032D598
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.CurrentForceFluctuation = msg.ReadRangedSingle(0f, 1f, 8);
		}

		// Token: 0x170018B0 RID: 6320
		// (get) Token: 0x060061E9 RID: 25065 RVA: 0x0032F3B1 File Offset: 0x0032D5B1
		// (set) Token: 0x060061EA RID: 25066 RVA: 0x0032F3B9 File Offset: 0x0032D5B9
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "The maximum amount of force applied to the triggering entitites.", "", true)]
		public float Force { get; set; }

		// Token: 0x170018B1 RID: 6321
		// (get) Token: 0x060061EB RID: 25067 RVA: 0x0032F3C2 File Offset: 0x0032D5C2
		// (set) Token: 0x060061EC RID: 25068 RVA: 0x0032F3CA File Offset: 0x0032D5CA
		[Editable]
		[Serialize("0,0", IsPropertySaveable.Yes, "The maximum amount of directional force applied to the triggering entitites.", "", true)]
		public Vector2 DirectionalForce { get; set; }

		// Token: 0x170018B2 RID: 6322
		// (get) Token: 0x060061ED RID: 25069 RVA: 0x0032F3D3 File Offset: 0x0032D5D3
		// (set) Token: 0x060061EE RID: 25070 RVA: 0x0032F3DB File Offset: 0x0032D5DB
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If true, DirectionalForce is relative to the angle between the target and the item, Similar to Force.\nIf false, it always pushes in the same direction, with respect to the item's rotation.", "", true)]
		public bool RelativeDirectionalForce { get; set; }

		// Token: 0x170018B3 RID: 6323
		// (get) Token: 0x060061EF RID: 25071 RVA: 0x0032F3E4 File Offset: 0x0032D5E4
		// (set) Token: 0x060061F0 RID: 25072 RVA: 0x0032F3EC File Offset: 0x0032D5EC
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "If false, no vertical force will be applied.", "", true)]
		public bool VerticalForce { get; set; }

		// Token: 0x170018B4 RID: 6324
		// (get) Token: 0x060061F1 RID: 25073 RVA: 0x0032F3F5 File Offset: 0x0032D5F5
		// (set) Token: 0x060061F2 RID: 25074 RVA: 0x0032F3FD File Offset: 0x0032D5FD
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "If false, no horizontal force will be applied.", "", true)]
		public bool HorizontalForce { get; set; }

		// Token: 0x170018B5 RID: 6325
		// (get) Token: 0x060061F3 RID: 25075 RVA: 0x0032F406 File Offset: 0x0032D606
		// (set) Token: 0x060061F4 RID: 25076 RVA: 0x0032F40E File Offset: 0x0032D60E
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Determines if the force gets higher the closer the triggerer is to the center of the trigger.", "", true)]
		public bool DistanceBasedForce { get; set; }

		// Token: 0x170018B6 RID: 6326
		// (get) Token: 0x060061F5 RID: 25077 RVA: 0x0032F417 File Offset: 0x0032D617
		// (set) Token: 0x060061F6 RID: 25078 RVA: 0x0032F41F File Offset: 0x0032D61F
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Determines if the force fluctuates over time or if it stays constant.", "", true)]
		public bool ForceFluctuation { get; set; }

		// Token: 0x170018B7 RID: 6327
		// (get) Token: 0x060061F7 RID: 25079 RVA: 0x0032F428 File Offset: 0x0032D628
		// (set) Token: 0x060061F8 RID: 25080 RVA: 0x0032F430 File Offset: 0x0032D630
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

		// Token: 0x170018B8 RID: 6328
		// (get) Token: 0x060061F9 RID: 25081 RVA: 0x0032F448 File Offset: 0x0032D648
		// (set) Token: 0x060061FA RID: 25082 RVA: 0x0032F450 File Offset: 0x0032D650
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

		// Token: 0x170018B9 RID: 6329
		// (get) Token: 0x060061FB RID: 25083 RVA: 0x0032F463 File Offset: 0x0032D663
		// (set) Token: 0x060061FC RID: 25084 RVA: 0x0032F46B File Offset: 0x0032D66B
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

		// Token: 0x170018BA RID: 6330
		// (get) Token: 0x060061FD RID: 25085 RVA: 0x0032F47E File Offset: 0x0032D67E
		// (set) Token: 0x060061FE RID: 25086 RVA: 0x0032F486 File Offset: 0x0032D686
		public PhysicsBody PhysicsBody { get; private set; }

		// Token: 0x170018BB RID: 6331
		// (get) Token: 0x060061FF RID: 25087 RVA: 0x0032F48F File Offset: 0x0032D68F
		// (set) Token: 0x06006200 RID: 25088 RVA: 0x0032F497 File Offset: 0x0032D697
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

		// Token: 0x170018BC RID: 6332
		// (get) Token: 0x06006201 RID: 25089 RVA: 0x0032F4B8 File Offset: 0x0032D6B8
		// (set) Token: 0x06006202 RID: 25090 RVA: 0x0032F4C0 File Offset: 0x0032D6C0
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

		// Token: 0x170018BD RID: 6333
		// (get) Token: 0x06006203 RID: 25091 RVA: 0x0032F4E1 File Offset: 0x0032D6E1
		// (set) Token: 0x06006204 RID: 25092 RVA: 0x0032F4E9 File Offset: 0x0032D6E9
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

		// Token: 0x170018BE RID: 6334
		// (get) Token: 0x06006205 RID: 25093 RVA: 0x0032F50A File Offset: 0x0032D70A
		// (set) Token: 0x06006206 RID: 25094 RVA: 0x0032F512 File Offset: 0x0032D712
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

		// Token: 0x170018BF RID: 6335
		// (get) Token: 0x06006207 RID: 25095 RVA: 0x0032F539 File Offset: 0x0032D739
		// (set) Token: 0x06006208 RID: 25096 RVA: 0x0032F541 File Offset: 0x0032D741
		private float RadiusInDisplayUnits { get; set; }

		// Token: 0x170018C0 RID: 6336
		// (get) Token: 0x06006209 RID: 25097 RVA: 0x0032F54A File Offset: 0x0032D74A
		// (set) Token: 0x0600620A RID: 25098 RVA: 0x0032F552 File Offset: 0x0032D752
		private bool TriggeredOnce { get; set; }

		// Token: 0x170018C1 RID: 6337
		// (get) Token: 0x0600620B RID: 25099 RVA: 0x0032F55B File Offset: 0x0032D75B
		// (set) Token: 0x0600620C RID: 25100 RVA: 0x0032F563 File Offset: 0x0032D763
		private float CurrentForceFluctuation { get; set; } = 1f;

		// Token: 0x170018C2 RID: 6338
		// (get) Token: 0x0600620D RID: 25101 RVA: 0x0032F56C File Offset: 0x0032D76C
		// (set) Token: 0x0600620E RID: 25102 RVA: 0x0032F574 File Offset: 0x0032D774
		public bool TriggerActive { get; private set; }

		// Token: 0x170018C3 RID: 6339
		// (get) Token: 0x0600620F RID: 25103 RVA: 0x0032F57D File Offset: 0x0032D77D
		// (set) Token: 0x06006210 RID: 25104 RVA: 0x0032F585 File Offset: 0x0032D785
		private float ForceFluctuationTimer { get; set; }

		// Token: 0x170018C4 RID: 6340
		// (get) Token: 0x06006211 RID: 25105 RVA: 0x0032F58E File Offset: 0x0032D78E
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

		// Token: 0x170018C5 RID: 6341
		// (get) Token: 0x06006212 RID: 25106 RVA: 0x0032F5A4 File Offset: 0x0032D7A4
		// (set) Token: 0x06006213 RID: 25107 RVA: 0x0032F5AC File Offset: 0x0032D7AC
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool ApplyEffectsToCharactersInsideSub { get; set; }

		// Token: 0x170018C6 RID: 6342
		// (get) Token: 0x06006214 RID: 25108 RVA: 0x0032F5B5 File Offset: 0x0032D7B5
		// (set) Token: 0x06006215 RID: 25109 RVA: 0x0032F5BD File Offset: 0x0032D7BD
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool MoveOutsideSub { get; set; }

		// Token: 0x170018C7 RID: 6343
		// (get) Token: 0x06006216 RID: 25110 RVA: 0x0032F5C6 File Offset: 0x0032D7C6
		// (set) Token: 0x06006217 RID: 25111 RVA: 0x0032F5D0 File Offset: 0x0032D7D0
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

		// Token: 0x06006218 RID: 25112 RVA: 0x0032F6D8 File Offset: 0x0032D8D8
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

		// Token: 0x06006219 RID: 25113 RVA: 0x0032F874 File Offset: 0x0032DA74
		public override void OnItemLoaded()
		{
			this.RefreshPhysicsBodySize();
		}

		// Token: 0x0600621A RID: 25114 RVA: 0x0032F87C File Offset: 0x0032DA7C
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

		// Token: 0x0600621B RID: 25115 RVA: 0x0032F9F8 File Offset: 0x0032DBF8
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

		// Token: 0x0600621C RID: 25116 RVA: 0x0032FAFB File Offset: 0x0032DCFB
		public override void FlipX(bool relativeToSub)
		{
			this.SetPhysicsBodyPosition(true);
		}

		// Token: 0x0600621D RID: 25117 RVA: 0x0032FB04 File Offset: 0x0032DD04
		public override void FlipY(bool relativeToSub)
		{
			this.SetPhysicsBodyPosition(true);
		}

		// Token: 0x0600621E RID: 25118 RVA: 0x0032FB0D File Offset: 0x0032DD0D
		public override void OnMapLoaded()
		{
			base.OnMapLoaded();
			this.SetPhysicsBodyPosition(true);
			this.PhysicsBody.Submarine = this.item.Submarine;
		}

		// Token: 0x0600621F RID: 25119 RVA: 0x0032FB34 File Offset: 0x0032DD34
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

		// Token: 0x06006220 RID: 25120 RVA: 0x0032FB94 File Offset: 0x0032DD94
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

		// Token: 0x06006221 RID: 25121 RVA: 0x0032FC00 File Offset: 0x0032DE00
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

		// Token: 0x06006222 RID: 25122 RVA: 0x0033007C File Offset: 0x0032E27C
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

		// Token: 0x06006223 RID: 25123 RVA: 0x003301F8 File Offset: 0x0032E3F8
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			if (this.PhysicsBody != null)
			{
				this.SetPhysicsBodyPosition(ignoreContacts);
				this.PhysicsBody.Submarine = this.item.Submarine;
			}
		}

		// Token: 0x06006224 RID: 25124 RVA: 0x0033021F File Offset: 0x0032E41F
		protected override void RemoveComponentSpecific()
		{
			if (this.PhysicsBody != null)
			{
				this.PhysicsBody.Remove();
				this.PhysicsBody = null;
			}
		}

		// Token: 0x06006225 RID: 25125 RVA: 0x0033023C File Offset: 0x0032E43C
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
								if (TriggerComponent.<ReceiveSignal>g__FloatTryParse|121_0(signal, out forceFluctuationInterval))
								{
									this.ForceFluctuationInterval = forceFluctuationInterval;
								}
							}
							else if (TriggerComponent.<ReceiveSignal>g__FloatTryParse|121_0(signal, out forceFluctuationFrequency))
							{
								this.ForceFluctuationFrequency = forceFluctuationFrequency;
								return;
							}
						}
						else if (TriggerComponent.<ReceiveSignal>g__FloatTryParse|121_0(signal, out forceFluctuationStrength))
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
			else if (TriggerComponent.<ReceiveSignal>g__FloatTryParse|121_0(signal, out force))
			{
				this.Force = force;
				return;
			}
		}

		// Token: 0x06006228 RID: 25128 RVA: 0x0033037B File Offset: 0x0032E57B
		[CompilerGenerated]
		internal static bool <ReceiveSignal>g__FloatTryParse|121_0(Signal signal, out float value)
		{
			return float.TryParse(signal.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
		}

		// Token: 0x0400327E RID: 12926
		private float radius;

		// Token: 0x0400327F RID: 12927
		private float width;

		// Token: 0x04003280 RID: 12928
		private float height;

		// Token: 0x04003281 RID: 12929
		private float currentRadius;

		// Token: 0x04003282 RID: 12930
		private float currentWidth;

		// Token: 0x04003283 RID: 12931
		private float currentHeight;

		// Token: 0x04003284 RID: 12932
		private Vector2 bodyOffset;

		// Token: 0x0400328C RID: 12940
		private readonly LevelTrigger.TriggererType triggeredBy;

		// Token: 0x0400328D RID: 12941
		private readonly Identifier triggerSpeciesOrGroup;

		// Token: 0x0400328E RID: 12942
		private readonly PropertyConditional.LogicalComparison conditionals;

		// Token: 0x0400328F RID: 12943
		private readonly HashSet<Entity> triggerers = new HashSet<Entity>();

		// Token: 0x04003290 RID: 12944
		private readonly bool triggerOnce;

		// Token: 0x04003291 RID: 12945
		private readonly List<ISerializableEntity> statusEffectTargets = new List<ISerializableEntity>();

		// Token: 0x04003292 RID: 12946
		private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();

		// Token: 0x04003293 RID: 12947
		private readonly List<Attack> attacks = new List<Attack>();

		// Token: 0x04003294 RID: 12948
		private float forceFluctuationStrength;

		// Token: 0x04003295 RID: 12949
		private float forceFluctuationFrequency;

		// Token: 0x04003296 RID: 12950
		private float forceFluctuationInterval;
	}
}
