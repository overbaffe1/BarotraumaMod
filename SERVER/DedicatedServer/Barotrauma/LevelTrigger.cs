using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200023F RID: 575
	internal class LevelTrigger
	{
		// Token: 0x17000BC9 RID: 3017
		// (get) Token: 0x0600283A RID: 10298 RVA: 0x001048D5 File Offset: 0x00102AD5
		public IEnumerable<StatusEffect> StatusEffects
		{
			get
			{
				return this.statusEffects;
			}
		}

		// Token: 0x17000BCA RID: 3018
		// (get) Token: 0x0600283B RID: 10299 RVA: 0x001048DD File Offset: 0x00102ADD
		// (set) Token: 0x0600283C RID: 10300 RVA: 0x001048E5 File Offset: 0x00102AE5
		public Dictionary<Entity, Vector2> TriggererPosition { get; private set; }

		// Token: 0x17000BCB RID: 3019
		// (get) Token: 0x0600283D RID: 10301 RVA: 0x001048EE File Offset: 0x00102AEE
		// (set) Token: 0x0600283E RID: 10302 RVA: 0x001048F6 File Offset: 0x00102AF6
		public Vector2 WorldPosition
		{
			get
			{
				return this.worldPosition;
			}
			set
			{
				this.worldPosition = value;
				PhysicsBody physicsBody = this.PhysicsBody;
				if (physicsBody == null)
				{
					return;
				}
				physicsBody.SetTransform(ConvertUnits.ToSimUnits(value), this.PhysicsBody.Rotation, true);
			}
		}

		// Token: 0x17000BCC RID: 3020
		// (get) Token: 0x0600283F RID: 10303 RVA: 0x00104922 File Offset: 0x00102B22
		// (set) Token: 0x06002840 RID: 10304 RVA: 0x0010493D File Offset: 0x00102B3D
		public float Rotation
		{
			get
			{
				if (this.PhysicsBody != null)
				{
					return this.PhysicsBody.Rotation;
				}
				return 0f;
			}
			set
			{
				if (this.PhysicsBody == null)
				{
					return;
				}
				this.PhysicsBody.SetTransform(this.PhysicsBody.Position, value, true);
				this.CalculateDirectionalForce();
			}
		}

		// Token: 0x17000BCD RID: 3021
		// (get) Token: 0x06002841 RID: 10305 RVA: 0x00104967 File Offset: 0x00102B67
		// (set) Token: 0x06002842 RID: 10306 RVA: 0x0010496F File Offset: 0x00102B6F
		public PhysicsBody PhysicsBody { get; private set; }

		// Token: 0x17000BCE RID: 3022
		// (get) Token: 0x06002843 RID: 10307 RVA: 0x00104978 File Offset: 0x00102B78
		// (set) Token: 0x06002844 RID: 10308 RVA: 0x00104980 File Offset: 0x00102B80
		public float TriggerOthersDistance { get; private set; }

		// Token: 0x17000BCF RID: 3023
		// (get) Token: 0x06002845 RID: 10309 RVA: 0x00104989 File Offset: 0x00102B89
		public IEnumerable<Entity> Triggerers
		{
			get
			{
				return this.triggerers.AsEnumerable<Entity>();
			}
		}

		// Token: 0x17000BD0 RID: 3024
		// (get) Token: 0x06002846 RID: 10310 RVA: 0x00104996 File Offset: 0x00102B96
		public bool IsTriggered
		{
			get
			{
				return (this.triggerers.Count > 0 || this.triggeredTimer > 0f) && (this.ParentTrigger == null || this.ParentTrigger.IsTriggered);
			}
		}

		// Token: 0x17000BD1 RID: 3025
		// (get) Token: 0x06002847 RID: 10311 RVA: 0x001049CA File Offset: 0x00102BCA
		// (set) Token: 0x06002848 RID: 10312 RVA: 0x001049D2 File Offset: 0x00102BD2
		public Vector2 Force { get; private set; }

		// Token: 0x17000BD2 RID: 3026
		// (get) Token: 0x06002849 RID: 10313 RVA: 0x001049DB File Offset: 0x00102BDB
		// (set) Token: 0x0600284A RID: 10314 RVA: 0x001049E3 File Offset: 0x00102BE3
		public bool ForceFalloff { get; private set; }

		// Token: 0x17000BD3 RID: 3027
		// (get) Token: 0x0600284B RID: 10315 RVA: 0x001049EC File Offset: 0x00102BEC
		// (set) Token: 0x0600284C RID: 10316 RVA: 0x001049F4 File Offset: 0x00102BF4
		public float ForceFluctuationInterval { get; private set; }

		// Token: 0x17000BD4 RID: 3028
		// (get) Token: 0x0600284D RID: 10317 RVA: 0x001049FD File Offset: 0x00102BFD
		// (set) Token: 0x0600284E RID: 10318 RVA: 0x00104A05 File Offset: 0x00102C05
		public float ForceFluctuationStrength { get; private set; }

		// Token: 0x17000BD5 RID: 3029
		// (get) Token: 0x0600284F RID: 10319 RVA: 0x00104A0E File Offset: 0x00102C0E
		// (set) Token: 0x06002850 RID: 10320 RVA: 0x00104A16 File Offset: 0x00102C16
		public float GlobalForceDecreaseInterval { get; private set; }

		// Token: 0x17000BD6 RID: 3030
		// (get) Token: 0x06002851 RID: 10321 RVA: 0x00104A1F File Offset: 0x00102C1F
		public LevelTrigger.TriggerForceMode ForceMode
		{
			get
			{
				return this.forceMode;
			}
		}

		// Token: 0x17000BD7 RID: 3031
		// (get) Token: 0x06002852 RID: 10322 RVA: 0x00104A27 File Offset: 0x00102C27
		// (set) Token: 0x06002853 RID: 10323 RVA: 0x00104A2F File Offset: 0x00102C2F
		public float ForceVelocityLimit { get; private set; }

		// Token: 0x17000BD8 RID: 3032
		// (get) Token: 0x06002854 RID: 10324 RVA: 0x00104A38 File Offset: 0x00102C38
		// (set) Token: 0x06002855 RID: 10325 RVA: 0x00104A40 File Offset: 0x00102C40
		public float ColliderRadius { get; private set; }

		// Token: 0x17000BD9 RID: 3033
		// (get) Token: 0x06002856 RID: 10326 RVA: 0x00104A49 File Offset: 0x00102C49
		// (set) Token: 0x06002857 RID: 10327 RVA: 0x00104A51 File Offset: 0x00102C51
		public bool UseNetworkSyncing { get; private set; }

		// Token: 0x17000BDA RID: 3034
		// (get) Token: 0x06002858 RID: 10328 RVA: 0x00104A5A File Offset: 0x00102C5A
		// (set) Token: 0x06002859 RID: 10329 RVA: 0x00104A62 File Offset: 0x00102C62
		public bool NeedsNetworkSyncing { get; set; }

		// Token: 0x17000BDB RID: 3035
		// (get) Token: 0x0600285A RID: 10330 RVA: 0x00104A6B File Offset: 0x00102C6B
		// (set) Token: 0x0600285B RID: 10331 RVA: 0x00104A73 File Offset: 0x00102C73
		public Identifier InfectIdentifier { get; set; }

		// Token: 0x17000BDC RID: 3036
		// (get) Token: 0x0600285C RID: 10332 RVA: 0x00104A7C File Offset: 0x00102C7C
		// (set) Token: 0x0600285D RID: 10333 RVA: 0x00104A84 File Offset: 0x00102C84
		public float InfectionChance { get; set; }

		// Token: 0x0600285E RID: 10334 RVA: 0x00104A90 File Offset: 0x00102C90
		public LevelTrigger(ContentXElement element, Vector2 position, float rotation, float scale = 1f, string parentDebugName = "")
		{
			this.TriggererPosition = new Dictionary<Entity, Vector2>();
			this.worldPosition = position;
			if (element.Attributes("radius").Any<XAttribute>() || element.Attributes("width").Any<XAttribute>() || element.Attributes("height").Any<XAttribute>())
			{
				this.PhysicsBody = new PhysicsBody(element, scale, true)
				{
					CollisionCategories = Category.Cat8,
					CollidesWith = (Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7)
				};
				this.PhysicsBody.FarseerBody.OnCollision += this.PhysicsBody_OnCollision;
				this.PhysicsBody.FarseerBody.OnSeparation += this.PhysicsBody_OnSeparation;
				this.PhysicsBody.FarseerBody.SetIsSensor(element.GetAttributeBool("sensor", true));
				this.PhysicsBody.FarseerBody.BodyType = BodyType.Static;
				this.ColliderRadius = ConvertUnits.ToDisplayUnits(Math.Max(Math.Max(this.PhysicsBody.Radius, this.PhysicsBody.Width / 2f), this.PhysicsBody.Height / 2f));
				this.PhysicsBody.SetTransform(ConvertUnits.ToSimUnits(position), rotation, true);
			}
			this.cameraShake = element.GetAttributeFloat("camerashake", 0f);
			this.InfectIdentifier = element.GetAttributeIdentifier("infectidentifier", Identifier.Empty);
			this.InfectionChance = element.GetAttributeFloat("infectionchance", 0.05f);
			this.triggerOnce = element.GetAttributeBool("triggeronce", false);
			this.stayTriggeredDelay = element.GetAttributeFloat("staytriggereddelay", 0f);
			this.randomTriggerInterval = element.GetAttributeFloat("randomtriggerinterval", 0f);
			this.randomTriggerProbability = element.GetAttributeFloat("randomtriggerprobability", 0f);
			this.UseNetworkSyncing = element.GetAttributeBool("networksyncing", false);
			Vector2 vector;
			if (element.GetAttribute("force") == null || !element.GetAttribute("force").Value.Contains(','))
			{
				vector = new Vector2(element.GetAttributeFloat("force", 0f), 0f);
			}
			else
			{
				string key = "force";
				Vector2 zero = Vector2.Zero;
				vector = element.GetAttributeVector2(key, zero);
			}
			this.unrotatedForce = vector;
			this.ForceFluctuationInterval = element.GetAttributeFloat("forcefluctuationinterval", 0.01f);
			this.ForceFluctuationStrength = Math.Max(element.GetAttributeFloat("forcefluctuationstrength", 0f), 0f);
			this.ForceFalloff = element.GetAttributeBool("forcefalloff", true);
			this.GlobalForceDecreaseInterval = element.GetAttributeFloat("globalforcedecreaseinterval", 0f);
			this.ForceVelocityLimit = ConvertUnits.ToSimUnits(element.GetAttributeFloat("forcevelocitylimit", float.MaxValue));
			string forceModeStr = element.GetAttributeString("forcemode", "Force");
			if (!Enum.TryParse<LevelTrigger.TriggerForceMode>(forceModeStr, out this.forceMode))
			{
				DebugConsole.ThrowError("Error in LevelTrigger config: \"" + forceModeStr + "\" is not a valid force mode.", null, null, false, false);
			}
			this.CalculateDirectionalForce();
			string triggeredByStr = element.GetAttributeString("triggeredby", "Character");
			if (!Enum.TryParse<LevelTrigger.TriggererType>(triggeredByStr, out this.triggeredBy))
			{
				Identifier speciesOrGroup = triggeredByStr.ToIdentifier();
				if (CharacterPrefab.Prefabs.Any((CharacterPrefab p) => p.MatchesSpeciesNameOrGroup(speciesOrGroup)))
				{
					this.triggerSpeciesOrGroup = speciesOrGroup;
					this.triggeredBy = LevelTrigger.TriggererType.Character;
				}
				else
				{
					DebugConsole.ThrowError("Error in LevelTrigger config: \"" + triggeredByStr + "\" is not a valid triggerer type.", null, null, false, false);
				}
			}
			if (this.PhysicsBody != null)
			{
				this.PhysicsBody.CollidesWith = LevelTrigger.GetCollisionCategories(this.triggeredBy);
			}
			this.TriggerOthersDistance = element.GetAttributeFloat("triggerothersdistance", 0f);
			string[] tagsArray = element.GetAttributeStringArray("tags", Array.Empty<string>(), false);
			foreach (string tag in tagsArray)
			{
				this.tags.Add(tag.ToLowerInvariant());
			}
			if (this.triggeredBy.HasFlag(LevelTrigger.TriggererType.OtherTrigger))
			{
				string[] otherTagsArray = element.GetAttributeStringArray("allowedothertriggertags", Array.Empty<string>(), false);
				foreach (string tag2 in otherTagsArray)
				{
					this.allowedOtherTriggerTags.Add(tag2.ToLowerInvariant());
				}
			}
			string debugName = string.IsNullOrEmpty(parentDebugName) ? "LevelTrigger" : ("LevelTrigger in " + parentDebugName);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "statuseffect"))
				{
					if (a == "attack" || a == "damage")
					{
						LevelTrigger.LoadAttack(subElement, debugName, this.triggerOnce, this.attacks);
					}
				}
				else
				{
					LevelTrigger.LoadStatusEffect(this.statusEffects, subElement, debugName);
				}
			}
			this.conditionals = PropertyConditional.LoadConditionals(element, PropertyConditional.LogicalOperatorType.And);
			this.forceFluctuationTimer = Rand.Range(0f, this.ForceFluctuationInterval, Rand.RandSync.Unsynced);
			this.randomTriggerTimer = Rand.Range(0f, this.randomTriggerInterval, Rand.RandSync.Unsynced);
		}

		// Token: 0x0600285F RID: 10335 RVA: 0x00105020 File Offset: 0x00103220
		public static Category GetCollisionCategories(LevelTrigger.TriggererType triggeredBy)
		{
			Category collidesWith = Category.None;
			if (triggeredBy.HasFlag(LevelTrigger.TriggererType.Human) || triggeredBy.HasFlag(LevelTrigger.TriggererType.Creature))
			{
				collidesWith |= Category.Cat2;
			}
			if (triggeredBy.HasFlag(LevelTrigger.TriggererType.Item))
			{
				collidesWith |= (Category.Cat5 | Category.Cat7);
			}
			if (triggeredBy.HasFlag(LevelTrigger.TriggererType.Submarine))
			{
				collidesWith |= Category.Cat1;
			}
			return collidesWith;
		}

		// Token: 0x06002860 RID: 10336 RVA: 0x0010508C File Offset: 0x0010328C
		private void CalculateDirectionalForce()
		{
			float ca = (float)Math.Cos((double)(-(double)this.Rotation));
			float sa = (float)Math.Sin((double)(-(double)this.Rotation));
			this.Force = new Vector2(ca * this.unrotatedForce.X + sa * this.unrotatedForce.Y, -sa * this.unrotatedForce.X + ca * this.unrotatedForce.Y);
		}

		// Token: 0x06002861 RID: 10337 RVA: 0x001050F9 File Offset: 0x001032F9
		public static void LoadStatusEffect(List<StatusEffect> statusEffects, ContentXElement element, string parentDebugName)
		{
			statusEffects.Add(StatusEffect.Load(element, parentDebugName));
		}

		// Token: 0x06002862 RID: 10338 RVA: 0x00105108 File Offset: 0x00103308
		public static void LoadAttack(ContentXElement element, string parentDebugName, bool triggerOnce, List<Attack> attacks)
		{
			Attack attack = new Attack(element, parentDebugName);
			if (!triggerOnce)
			{
				List<Affliction> multipliedAfflictions = attack.GetMultipliedAfflictions(0.016666668f);
				attack.Afflictions.Clear();
				foreach (Affliction affliction in multipliedAfflictions)
				{
					attack.Afflictions.Add(affliction, null);
				}
			}
			attacks.Add(attack);
		}

		// Token: 0x06002863 RID: 10339 RVA: 0x00105188 File Offset: 0x00103388
		private bool PhysicsBody_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			Entity entity = LevelTrigger.GetEntity(fixtureB);
			if (entity == null)
			{
				return false;
			}
			if (!LevelTrigger.IsTriggeredByEntity(entity, this.triggeredBy, this.triggerSpeciesOrGroup, this.conditionals, default(ValueTuple<bool, Submarine>), true))
			{
				return false;
			}
			if (!this.triggerers.Contains(entity))
			{
				if (!this.IsTriggered)
				{
					Action<LevelTrigger, Entity> onTriggered = this.OnTriggered;
					if (onTriggered != null)
					{
						onTriggered(this, entity);
					}
				}
				this.TriggererPosition[entity] = entity.WorldPosition;
				this.triggerers.Add(entity);
			}
			return true;
		}

		// Token: 0x06002864 RID: 10340 RVA: 0x00105210 File Offset: 0x00103410
		public static bool IsTriggeredByEntity(Entity entity, LevelTrigger.TriggererType triggeredBy, Identifier triggerSpeciesOrGroup, PropertyConditional.LogicalComparison conditionals, [TupleElementNames(new string[]
		{
			"mustBe",
			"sub"
		})] ValueTuple<bool, Submarine> mustBeOnSpecificSub = default(ValueTuple<bool, Submarine>), bool mustBeOutside = false)
		{
			Character character = entity as Character;
			if (character != null)
			{
				if (mustBeOutside && character.CurrentHull != null)
				{
					return false;
				}
				if (mustBeOnSpecificSub.Item1 && character.Submarine != mustBeOnSpecificSub.Item2)
				{
					return false;
				}
				if (!triggerSpeciesOrGroup.IsEmpty)
				{
					Identifier speciesName = character.SpeciesName;
					if (speciesName != triggerSpeciesOrGroup)
					{
						Identifier group = character.Group;
						if (group != triggerSpeciesOrGroup)
						{
							return false;
						}
					}
				}
				if (character.IsHuman)
				{
					if (!triggeredBy.HasFlag(LevelTrigger.TriggererType.Human))
					{
						return false;
					}
				}
				else if (!triggeredBy.HasFlag(LevelTrigger.TriggererType.Creature))
				{
					return false;
				}
			}
			else
			{
				Item item = entity as Item;
				if (item != null)
				{
					if (mustBeOutside && item.CurrentHull != null)
					{
						return false;
					}
					if (mustBeOnSpecificSub.Item1 && item.Submarine != mustBeOnSpecificSub.Item2)
					{
						return false;
					}
					if (!triggeredBy.HasFlag(LevelTrigger.TriggererType.Item))
					{
						return false;
					}
				}
				else if (entity is Submarine && !triggeredBy.HasFlag(LevelTrigger.TriggererType.Submarine))
				{
					return false;
				}
			}
			if (conditionals != null)
			{
				ISerializableEntity serializableEntity = entity as ISerializableEntity;
				if (serializableEntity != null && !PropertyConditional.CheckConditionals(serializableEntity, conditionals.Conditionals, conditionals.LogicalOperator))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002865 RID: 10341 RVA: 0x00105344 File Offset: 0x00103544
		private void PhysicsBody_OnSeparation(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			Entity entity = LevelTrigger.GetEntity(fixtureB);
			if (entity == null)
			{
				return;
			}
			Character character = entity as Character;
			if (character != null && (!character.Enabled || character.Removed) && this.triggerers.Contains(entity))
			{
				this.TriggererPosition.Remove(entity);
				this.triggerers.Remove(entity);
				return;
			}
			if (LevelTrigger.CheckContactsForOtherFixtures(this.PhysicsBody, fixtureB, entity))
			{
				return;
			}
			if (this.triggerers.Contains(entity))
			{
				this.TriggererPosition.Remove(entity);
				this.triggerers.Remove(entity);
			}
		}

		// Token: 0x06002866 RID: 10342 RVA: 0x001053D8 File Offset: 0x001035D8
		public static bool CheckContactsForOtherFixtures(PhysicsBody triggerBody, Fixture separatingFixture, Entity separatingEntity)
		{
			foreach (Fixture triggerFixture in triggerBody.FarseerBody.FixtureList)
			{
				for (ContactEdge contactEdge = triggerFixture.Body.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
				{
					if (contactEdge.Contact != null && contactEdge.Contact.Enabled && contactEdge.Contact.IsTouching)
					{
						Fixture otherFixture = (contactEdge.Contact.FixtureA == triggerFixture) ? contactEdge.Contact.FixtureB : contactEdge.Contact.FixtureA;
						if (otherFixture != separatingFixture)
						{
							Entity otherEntity = LevelTrigger.GetEntity(otherFixture);
							if (otherEntity == separatingEntity)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06002867 RID: 10343 RVA: 0x001054A8 File Offset: 0x001036A8
		public static bool CheckContactsForEntity(PhysicsBody triggerBody, Entity targetEntity)
		{
			foreach (Fixture fixture in triggerBody.FarseerBody.FixtureList)
			{
				for (ContactEdge contactEdge = fixture.Body.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
				{
					if (contactEdge.Contact != null && contactEdge.Contact.Enabled && contactEdge.Contact.IsTouching && ((contactEdge.Contact.FixtureA.Body == triggerBody.FarseerBody && LevelTrigger.GetEntity(contactEdge.Contact.FixtureB) == targetEntity) || (contactEdge.Contact.FixtureB.Body == triggerBody.FarseerBody && LevelTrigger.GetEntity(contactEdge.Contact.FixtureA) == targetEntity)))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002868 RID: 10344 RVA: 0x0010559C File Offset: 0x0010379C
		public static Entity GetEntity(Fixture fixture)
		{
			if (fixture.Body == null || fixture.Body.UserData == null)
			{
				return null;
			}
			Entity entity = fixture.Body.UserData as Entity;
			if (entity != null)
			{
				return entity;
			}
			Limb limb = fixture.Body.UserData as Limb;
			if (limb != null)
			{
				return limb.character;
			}
			SubmarineBody subBody = fixture.Body.UserData as SubmarineBody;
			if (subBody != null)
			{
				return subBody.Submarine;
			}
			return null;
		}

		// Token: 0x06002869 RID: 10345 RVA: 0x00105610 File Offset: 0x00103810
		public void OtherTriggered(LevelTrigger otherTrigger, Entity triggerer)
		{
			if (!this.triggeredBy.HasFlag(LevelTrigger.TriggererType.OtherTrigger) || this.stayTriggeredDelay <= 0f)
			{
				return;
			}
			if (this.allowedOtherTriggerTags.Count > 0 && !this.allowedOtherTriggerTags.Any((string t) => otherTrigger.tags.Contains(t)))
			{
				return;
			}
			if (Vector2.DistanceSquared(this.WorldPosition, otherTrigger.WorldPosition) <= otherTrigger.TriggerOthersDistance * otherTrigger.TriggerOthersDistance)
			{
				bool wasAlreadyTriggered = this.IsTriggered;
				this.triggeredTimer = this.stayTriggeredDelay;
				if (!wasAlreadyTriggered)
				{
					if (!LevelTrigger.IsTriggeredByEntity(triggerer, this.triggeredBy, this.triggerSpeciesOrGroup, this.conditionals, default(ValueTuple<bool, Submarine>), true))
					{
						return;
					}
					if (!this.triggerers.Contains(triggerer))
					{
						if (!this.IsTriggered)
						{
							Action<LevelTrigger, Entity> onTriggered = this.OnTriggered;
							if (onTriggered != null)
							{
								onTriggered(this, triggerer);
							}
						}
						this.TriggererPosition[triggerer] = triggerer.WorldPosition;
						this.triggerers.Add(triggerer);
					}
				}
			}
		}

		// Token: 0x0600286A RID: 10346 RVA: 0x00105730 File Offset: 0x00103930
		public void Update(float deltaTime)
		{
			if (this.ParentTrigger != null && !this.ParentTrigger.IsTriggered)
			{
				return;
			}
			bool isNotClient = true;
			if (!this.UseNetworkSyncing || isNotClient)
			{
				if (this.GlobalForceDecreaseInterval > 0f)
				{
					Level loaded = Level.Loaded;
					if (((loaded != null) ? loaded.LevelObjectManager : null) != null && Level.Loaded.LevelObjectManager.GlobalForceDecreaseTimer % (this.GlobalForceDecreaseInterval * 2f) < this.GlobalForceDecreaseInterval)
					{
						this.NeedsNetworkSyncing |= (this.currentForceFluctuation > 0f);
						this.currentForceFluctuation = 0f;
						goto IL_101;
					}
				}
				if (this.ForceFluctuationStrength > 0f && (this.forceMode != LevelTrigger.TriggerForceMode.LimitVelocity || this.triggerers.Any<Entity>()))
				{
					this.forceFluctuationTimer += deltaTime;
					if (this.forceFluctuationTimer > this.ForceFluctuationInterval)
					{
						this.NeedsNetworkSyncing = true;
						this.currentForceFluctuation = Rand.Range(1f - this.ForceFluctuationStrength, 1f, Rand.RandSync.Unsynced);
						this.forceFluctuationTimer = 0f;
					}
				}
				IL_101:
				if (this.randomTriggerProbability > 0f)
				{
					this.randomTriggerTimer += deltaTime;
					if (this.randomTriggerTimer > this.randomTriggerInterval)
					{
						if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.randomTriggerProbability)
						{
							this.NeedsNetworkSyncing = true;
							this.triggeredTimer = this.stayTriggeredDelay;
						}
						this.randomTriggerTimer = 0f;
					}
				}
			}
			LevelTrigger.RemoveInActiveTriggerers(this.PhysicsBody, this.triggerers);
			if (this.stayTriggeredDelay > 0f)
			{
				if (this.triggerers.Count == 0)
				{
					this.triggeredTimer -= deltaTime;
				}
				else
				{
					this.triggeredTimer = this.stayTriggeredDelay;
				}
			}
			if (this.triggerOnce && this.triggeredOnce)
			{
				return;
			}
			if (this.PhysicsBody != null)
			{
				if (this.currentForceFluctuation <= 0f && this.statusEffects.None(null) && this.attacks.None(null))
				{
					this.PhysicsBody.Enabled = false;
					return;
				}
				this.PhysicsBody.Enabled = true;
			}
			foreach (Entity triggerer in this.triggerers)
			{
				if (!triggerer.Removed)
				{
					LevelTrigger.ApplyStatusEffects(this.statusEffects, this.worldPosition, triggerer, deltaTime, this.targets, null);
					IDamageable damageable = triggerer as IDamageable;
					if (damageable != null)
					{
						LevelTrigger.ApplyAttacks(this.attacks, damageable, this.worldPosition, deltaTime);
					}
					else
					{
						Submarine submarine = triggerer as Submarine;
						if (submarine != null)
						{
							LevelTrigger.ApplyAttacks(this.attacks, this.worldPosition, deltaTime);
							if (!this.InfectIdentifier.IsEmpty)
							{
								submarine.AttemptBallastFloraInfection(this.InfectIdentifier, deltaTime, this.InfectionChance);
							}
						}
					}
					if (this.Force.LengthSquared() > 0.01f)
					{
						Character character = triggerer as Character;
						if (character != null)
						{
							this.ApplyForce(character.AnimController.Collider);
							foreach (Limb limb in character.AnimController.Limbs)
							{
								if (!limb.IsSevered)
								{
									this.ApplyForce(limb.body);
								}
							}
						}
						else
						{
							Submarine submarine2 = triggerer as Submarine;
							if (submarine2 != null)
							{
								this.ApplyForce(submarine2.SubBody.Body);
							}
						}
					}
					if (triggerer != Character.Controlled)
					{
						Submarine submarine3 = triggerer;
						Character controlled = Character.Controlled;
						if (submarine3 != ((controlled != null) ? controlled.Submarine : null))
						{
							continue;
						}
					}
					GameMain.GameScreen.Cam.Shake = Math.Max(GameMain.GameScreen.Cam.Shake, this.cameraShake);
				}
			}
			if (this.triggerOnce && this.triggerers.Count > 0)
			{
				this.PhysicsBody.Enabled = false;
				this.triggeredOnce = true;
			}
		}

		// Token: 0x0600286B RID: 10347 RVA: 0x00105B18 File Offset: 0x00103D18
		public static void RemoveInActiveTriggerers(PhysicsBody physicsBody, HashSet<Entity> triggerers)
		{
			if (physicsBody == null)
			{
				return;
			}
			LevelTrigger.triggerersToRemove.Clear();
			foreach (Entity triggerer in triggerers)
			{
				if (triggerer.Removed)
				{
					LevelTrigger.triggerersToRemove.Add(triggerer);
				}
				else if (!LevelTrigger.CheckContactsForEntity(physicsBody, triggerer))
				{
					LevelTrigger.triggerersToRemove.Add(triggerer);
				}
			}
			foreach (Entity triggerer2 in LevelTrigger.triggerersToRemove)
			{
				triggerers.Remove(triggerer2);
			}
		}

		// Token: 0x0600286C RID: 10348 RVA: 0x00105BD8 File Offset: 0x00103DD8
		public static void ApplyStatusEffects(List<StatusEffect> statusEffects, Vector2 worldPosition, Entity triggerer, float deltaTime, List<ISerializableEntity> targets, Item targetItem = null)
		{
			foreach (StatusEffect effect in statusEffects)
			{
				if (effect.type == ActionType.OnBroken)
				{
					break;
				}
				Vector2? position = null;
				if (effect.HasTargetType(StatusEffect.TargetType.This))
				{
					position = new Vector2?(worldPosition);
					if (targetItem != null)
					{
						effect.Apply(effect.type, deltaTime, triggerer, targetItem.AllPropertyObjects, position);
					}
				}
				Character character = triggerer as Character;
				if (character == null)
				{
					goto IL_10D;
				}
				effect.Apply(effect.type, deltaTime, triggerer, character, position);
				if (effect.HasTargetType(StatusEffect.TargetType.Contained) && character.Inventory != null)
				{
					using (IEnumerator<Item> enumerator2 = character.Inventory.AllItemsMod.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Item item = enumerator2.Current;
							if (item.ContainedItems != null)
							{
								foreach (Item containedItem in item.ContainedItems)
								{
									effect.Apply(effect.type, deltaTime, triggerer, containedItem.AllPropertyObjects, position);
								}
							}
						}
						goto IL_152;
					}
					goto IL_10D;
				}
				IL_152:
				if (effect.HasTargetType(StatusEffect.TargetType.NearbyItems) || effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
				{
					targets.Clear();
					effect.AddNearbyTargets(worldPosition, targets);
					effect.Apply(effect.type, deltaTime, triggerer, targets, null);
					continue;
				}
				continue;
				IL_10D:
				Item item2 = triggerer as Item;
				if (item2 != null)
				{
					effect.Apply(effect.type, deltaTime, triggerer, item2.AllPropertyObjects, position);
					goto IL_152;
				}
				Submarine sub = triggerer as Submarine;
				if (sub != null)
				{
					effect.Apply(effect.type, deltaTime, sub, Array.Empty<ISerializableEntity>(), position);
					goto IL_152;
				}
				goto IL_152;
			}
		}

		// Token: 0x0600286D RID: 10349 RVA: 0x00105DE0 File Offset: 0x00103FE0
		public static void ApplyAttacks(List<Attack> attacks, IDamageable damageable, Vector2 worldPosition, float deltaTime)
		{
			foreach (Attack attack in attacks)
			{
				attack.DoDamage(null, damageable, worldPosition, deltaTime, false, null, null);
			}
		}

		// Token: 0x0600286E RID: 10350 RVA: 0x00105E38 File Offset: 0x00104038
		public static void ApplyAttacks(List<Attack> attacks, Vector2 worldPosition, float deltaTime)
		{
			foreach (Attack attack in attacks)
			{
				float structureDamage = attack.GetStructureDamage(deltaTime);
				if (structureDamage > 0f)
				{
					Explosion.RangedStructureDamage(worldPosition, attack.DamageRange, structureDamage, 0f, null, null, attack.EmitStructureDamageParticles, false, true);
				}
			}
		}

		// Token: 0x0600286F RID: 10351 RVA: 0x00105EAC File Offset: 0x001040AC
		private void ApplyForce(PhysicsBody body)
		{
			if (body == null)
			{
				return;
			}
			float distFactor = 1f;
			if (this.ForceFalloff)
			{
				distFactor = LevelTrigger.GetDistanceFactor(body, this.PhysicsBody, this.ColliderRadius);
				if (distFactor < 0f)
				{
					return;
				}
			}
			if (MathUtils.NearlyEqual(this.currentForceFluctuation, 0f, 0.0001f))
			{
				return;
			}
			switch (this.ForceMode)
			{
			case LevelTrigger.TriggerForceMode.Force:
				if (this.ForceVelocityLimit < 1000f)
				{
					body.ApplyForce(this.Force * this.currentForceFluctuation * distFactor, this.ForceVelocityLimit);
					return;
				}
				body.ApplyForce(this.Force * this.currentForceFluctuation * distFactor, 64f);
				return;
			case LevelTrigger.TriggerForceMode.Acceleration:
				if (this.ForceVelocityLimit < 1000f)
				{
					body.ApplyForce(this.Force * body.Mass * this.currentForceFluctuation * distFactor, this.ForceVelocityLimit);
					return;
				}
				body.ApplyForce(this.Force * body.Mass * this.currentForceFluctuation * distFactor, 64f);
				return;
			case LevelTrigger.TriggerForceMode.Impulse:
				if (this.ForceVelocityLimit < 1000f)
				{
					body.ApplyLinearImpulse(this.Force * this.currentForceFluctuation * distFactor, this.ForceVelocityLimit);
					return;
				}
				body.ApplyLinearImpulse(this.Force * this.currentForceFluctuation * distFactor);
				return;
			case LevelTrigger.TriggerForceMode.LimitVelocity:
			{
				float maxVel = this.ForceVelocityLimit * this.currentForceFluctuation * distFactor;
				if (body.LinearVelocity.LengthSquared() > maxVel * maxVel)
				{
					body.ApplyForce(Vector2.Normalize(-body.LinearVelocity) * this.Force.Length() * body.Mass * this.currentForceFluctuation * distFactor, 64f);
				}
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x06002870 RID: 10352 RVA: 0x00106097 File Offset: 0x00104297
		public static float GetDistanceFactor(PhysicsBody triggererBody, PhysicsBody triggerBody, float colliderRadius)
		{
			return 1f - ConvertUnits.ToDisplayUnits(Vector2.Distance(triggererBody.SimPosition, triggerBody.SimPosition) - triggererBody.GetMaxExtent() / 2f) / colliderRadius;
		}

		// Token: 0x06002871 RID: 10353 RVA: 0x001060C4 File Offset: 0x001042C4
		public Vector2 GetWaterFlowVelocity(Vector2 viewPosition)
		{
			Vector2 baseVel = this.GetWaterFlowVelocity();
			if (baseVel.LengthSquared() < 0.1f)
			{
				return Vector2.Zero;
			}
			float triggerSize = ConvertUnits.ToDisplayUnits(Math.Max(Math.Max(this.PhysicsBody.Radius, this.PhysicsBody.Width / 2f), this.PhysicsBody.Height / 2f));
			float dist = Vector2.Distance(viewPosition, this.WorldPosition);
			if (dist > triggerSize)
			{
				return Vector2.Zero;
			}
			return baseVel * (1f - dist / triggerSize);
		}

		// Token: 0x06002872 RID: 10354 RVA: 0x00106150 File Offset: 0x00104350
		public Vector2 GetWaterFlowVelocity()
		{
			if (this.Force == Vector2.Zero || this.ForceMode == LevelTrigger.TriggerForceMode.LimitVelocity)
			{
				return Vector2.Zero;
			}
			Vector2 vel = this.Force;
			if (this.ForceMode == LevelTrigger.TriggerForceMode.Acceleration)
			{
				vel *= 1000f;
			}
			else if (this.ForceMode == LevelTrigger.TriggerForceMode.Impulse)
			{
				vel /= 0.016666668f;
			}
			return vel.ClampLength(ConvertUnits.ToDisplayUnits(this.ForceVelocityLimit)) * this.currentForceFluctuation;
		}

		// Token: 0x06002873 RID: 10355 RVA: 0x001061D0 File Offset: 0x001043D0
		public void ServerWrite(IWriteMessage msg, Client c)
		{
			if (this.ForceFluctuationStrength > 0f)
			{
				msg.WriteRangedSingle(MathHelper.Clamp(this.currentForceFluctuation, 0f, 1f), 0f, 1f, 8);
			}
			if (this.stayTriggeredDelay > 0f)
			{
				msg.WriteRangedSingle(MathHelper.Clamp(this.triggeredTimer, 0f, this.stayTriggeredDelay), 0f, this.stayTriggeredDelay, 16);
			}
		}

		// Token: 0x040013BC RID: 5052
		public Action<LevelTrigger, Entity> OnTriggered;

		// Token: 0x040013BD RID: 5053
		private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();

		// Token: 0x040013BE RID: 5054
		private readonly List<Attack> attacks = new List<Attack>();

		// Token: 0x040013BF RID: 5055
		private readonly float cameraShake;

		// Token: 0x040013C0 RID: 5056
		private Vector2 unrotatedForce;

		// Token: 0x040013C1 RID: 5057
		private float forceFluctuationTimer;

		// Token: 0x040013C2 RID: 5058
		private float currentForceFluctuation = 1f;

		// Token: 0x040013C3 RID: 5059
		private readonly HashSet<Entity> triggerers = new HashSet<Entity>();

		// Token: 0x040013C4 RID: 5060
		private readonly LevelTrigger.TriggererType triggeredBy;

		// Token: 0x040013C5 RID: 5061
		private readonly Identifier triggerSpeciesOrGroup;

		// Token: 0x040013C6 RID: 5062
		private readonly PropertyConditional.LogicalComparison conditionals;

		// Token: 0x040013C7 RID: 5063
		private readonly float randomTriggerInterval;

		// Token: 0x040013C8 RID: 5064
		private readonly float randomTriggerProbability;

		// Token: 0x040013C9 RID: 5065
		private float randomTriggerTimer;

		// Token: 0x040013CA RID: 5066
		private float triggeredTimer;

		// Token: 0x040013CB RID: 5067
		private readonly HashSet<string> tags = new HashSet<string>();

		// Token: 0x040013CC RID: 5068
		private readonly HashSet<string> allowedOtherTriggerTags = new HashSet<string>();

		// Token: 0x040013CD RID: 5069
		private readonly float stayTriggeredDelay;

		// Token: 0x040013CE RID: 5070
		public LevelTrigger ParentTrigger;

		// Token: 0x040013D0 RID: 5072
		private Vector2 worldPosition;

		// Token: 0x040013D8 RID: 5080
		private readonly LevelTrigger.TriggerForceMode forceMode;

		// Token: 0x040013DF RID: 5087
		private bool triggeredOnce;

		// Token: 0x040013E0 RID: 5088
		private readonly bool triggerOnce;

		// Token: 0x040013E1 RID: 5089
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x040013E2 RID: 5090
		private static readonly List<Entity> triggerersToRemove = new List<Entity>();

		// Token: 0x02000A25 RID: 2597
		[Flags]
		public enum TriggererType
		{
			// Token: 0x04003567 RID: 13671
			None = 0,
			// Token: 0x04003568 RID: 13672
			Human = 1,
			// Token: 0x04003569 RID: 13673
			Creature = 2,
			// Token: 0x0400356A RID: 13674
			Character = 3,
			// Token: 0x0400356B RID: 13675
			Submarine = 4,
			// Token: 0x0400356C RID: 13676
			Item = 8,
			// Token: 0x0400356D RID: 13677
			OtherTrigger = 16
		}

		// Token: 0x02000A26 RID: 2598
		public enum TriggerForceMode
		{
			// Token: 0x0400356F RID: 13679
			Force,
			// Token: 0x04003570 RID: 13680
			Acceleration,
			// Token: 0x04003571 RID: 13681
			Impulse,
			// Token: 0x04003572 RID: 13682
			LimitVelocity
		}
	}
}
