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
	// Token: 0x020000DE RID: 222
	internal class LevelTrigger
	{
		// Token: 0x06001ECF RID: 7887 RVA: 0x00130A94 File Offset: 0x0012EC94
		public void ClientRead(IReadMessage msg)
		{
			if (this.ForceFluctuationStrength > 0f)
			{
				this.currentForceFluctuation = msg.ReadRangedSingle(0f, 1f, 8);
			}
			if (this.stayTriggeredDelay > 0f)
			{
				this.triggeredTimer = msg.ReadRangedSingle(0f, this.stayTriggeredDelay, 16);
			}
		}

		// Token: 0x17000847 RID: 2119
		// (get) Token: 0x06001ED0 RID: 7888 RVA: 0x00130AEB File Offset: 0x0012ECEB
		public IEnumerable<StatusEffect> StatusEffects
		{
			get
			{
				return this.statusEffects;
			}
		}

		// Token: 0x17000848 RID: 2120
		// (get) Token: 0x06001ED1 RID: 7889 RVA: 0x00130AF3 File Offset: 0x0012ECF3
		// (set) Token: 0x06001ED2 RID: 7890 RVA: 0x00130AFB File Offset: 0x0012ECFB
		public Dictionary<Entity, Vector2> TriggererPosition { get; private set; }

		// Token: 0x17000849 RID: 2121
		// (get) Token: 0x06001ED3 RID: 7891 RVA: 0x00130B04 File Offset: 0x0012ED04
		// (set) Token: 0x06001ED4 RID: 7892 RVA: 0x00130B0C File Offset: 0x0012ED0C
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

		// Token: 0x1700084A RID: 2122
		// (get) Token: 0x06001ED5 RID: 7893 RVA: 0x00130B38 File Offset: 0x0012ED38
		// (set) Token: 0x06001ED6 RID: 7894 RVA: 0x00130B53 File Offset: 0x0012ED53
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

		// Token: 0x1700084B RID: 2123
		// (get) Token: 0x06001ED7 RID: 7895 RVA: 0x00130B7D File Offset: 0x0012ED7D
		// (set) Token: 0x06001ED8 RID: 7896 RVA: 0x00130B85 File Offset: 0x0012ED85
		public PhysicsBody PhysicsBody { get; private set; }

		// Token: 0x1700084C RID: 2124
		// (get) Token: 0x06001ED9 RID: 7897 RVA: 0x00130B8E File Offset: 0x0012ED8E
		// (set) Token: 0x06001EDA RID: 7898 RVA: 0x00130B96 File Offset: 0x0012ED96
		public float TriggerOthersDistance { get; private set; }

		// Token: 0x1700084D RID: 2125
		// (get) Token: 0x06001EDB RID: 7899 RVA: 0x00130B9F File Offset: 0x0012ED9F
		public IEnumerable<Entity> Triggerers
		{
			get
			{
				return this.triggerers.AsEnumerable<Entity>();
			}
		}

		// Token: 0x1700084E RID: 2126
		// (get) Token: 0x06001EDC RID: 7900 RVA: 0x00130BAC File Offset: 0x0012EDAC
		public bool IsTriggered
		{
			get
			{
				return (this.triggerers.Count > 0 || this.triggeredTimer > 0f) && (this.ParentTrigger == null || this.ParentTrigger.IsTriggered);
			}
		}

		// Token: 0x1700084F RID: 2127
		// (get) Token: 0x06001EDD RID: 7901 RVA: 0x00130BE0 File Offset: 0x0012EDE0
		// (set) Token: 0x06001EDE RID: 7902 RVA: 0x00130BE8 File Offset: 0x0012EDE8
		public Vector2 Force { get; private set; }

		// Token: 0x17000850 RID: 2128
		// (get) Token: 0x06001EDF RID: 7903 RVA: 0x00130BF1 File Offset: 0x0012EDF1
		// (set) Token: 0x06001EE0 RID: 7904 RVA: 0x00130BF9 File Offset: 0x0012EDF9
		public bool ForceFalloff { get; private set; }

		// Token: 0x17000851 RID: 2129
		// (get) Token: 0x06001EE1 RID: 7905 RVA: 0x00130C02 File Offset: 0x0012EE02
		// (set) Token: 0x06001EE2 RID: 7906 RVA: 0x00130C0A File Offset: 0x0012EE0A
		public float ForceFluctuationInterval { get; private set; }

		// Token: 0x17000852 RID: 2130
		// (get) Token: 0x06001EE3 RID: 7907 RVA: 0x00130C13 File Offset: 0x0012EE13
		// (set) Token: 0x06001EE4 RID: 7908 RVA: 0x00130C1B File Offset: 0x0012EE1B
		public float ForceFluctuationStrength { get; private set; }

		// Token: 0x17000853 RID: 2131
		// (get) Token: 0x06001EE5 RID: 7909 RVA: 0x00130C24 File Offset: 0x0012EE24
		// (set) Token: 0x06001EE6 RID: 7910 RVA: 0x00130C2C File Offset: 0x0012EE2C
		public float GlobalForceDecreaseInterval { get; private set; }

		// Token: 0x17000854 RID: 2132
		// (get) Token: 0x06001EE7 RID: 7911 RVA: 0x00130C35 File Offset: 0x0012EE35
		public LevelTrigger.TriggerForceMode ForceMode
		{
			get
			{
				return this.forceMode;
			}
		}

		// Token: 0x17000855 RID: 2133
		// (get) Token: 0x06001EE8 RID: 7912 RVA: 0x00130C3D File Offset: 0x0012EE3D
		// (set) Token: 0x06001EE9 RID: 7913 RVA: 0x00130C45 File Offset: 0x0012EE45
		public float ForceVelocityLimit { get; private set; }

		// Token: 0x17000856 RID: 2134
		// (get) Token: 0x06001EEA RID: 7914 RVA: 0x00130C4E File Offset: 0x0012EE4E
		// (set) Token: 0x06001EEB RID: 7915 RVA: 0x00130C56 File Offset: 0x0012EE56
		public float ColliderRadius { get; private set; }

		// Token: 0x17000857 RID: 2135
		// (get) Token: 0x06001EEC RID: 7916 RVA: 0x00130C5F File Offset: 0x0012EE5F
		// (set) Token: 0x06001EED RID: 7917 RVA: 0x00130C67 File Offset: 0x0012EE67
		public bool UseNetworkSyncing { get; private set; }

		// Token: 0x17000858 RID: 2136
		// (get) Token: 0x06001EEE RID: 7918 RVA: 0x00130C70 File Offset: 0x0012EE70
		// (set) Token: 0x06001EEF RID: 7919 RVA: 0x00130C78 File Offset: 0x0012EE78
		public bool NeedsNetworkSyncing { get; set; }

		// Token: 0x17000859 RID: 2137
		// (get) Token: 0x06001EF0 RID: 7920 RVA: 0x00130C81 File Offset: 0x0012EE81
		// (set) Token: 0x06001EF1 RID: 7921 RVA: 0x00130C89 File Offset: 0x0012EE89
		public Identifier InfectIdentifier { get; set; }

		// Token: 0x1700085A RID: 2138
		// (get) Token: 0x06001EF2 RID: 7922 RVA: 0x00130C92 File Offset: 0x0012EE92
		// (set) Token: 0x06001EF3 RID: 7923 RVA: 0x00130C9A File Offset: 0x0012EE9A
		public float InfectionChance { get; set; }

		// Token: 0x06001EF4 RID: 7924 RVA: 0x00130CA4 File Offset: 0x0012EEA4
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

		// Token: 0x06001EF5 RID: 7925 RVA: 0x00131234 File Offset: 0x0012F434
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

		// Token: 0x06001EF6 RID: 7926 RVA: 0x001312A0 File Offset: 0x0012F4A0
		private void CalculateDirectionalForce()
		{
			float ca = (float)Math.Cos((double)(-(double)this.Rotation));
			float sa = (float)Math.Sin((double)(-(double)this.Rotation));
			this.Force = new Vector2(ca * this.unrotatedForce.X + sa * this.unrotatedForce.Y, -sa * this.unrotatedForce.X + ca * this.unrotatedForce.Y);
		}

		// Token: 0x06001EF7 RID: 7927 RVA: 0x0013130D File Offset: 0x0012F50D
		public static void LoadStatusEffect(List<StatusEffect> statusEffects, ContentXElement element, string parentDebugName)
		{
			statusEffects.Add(StatusEffect.Load(element, parentDebugName));
		}

		// Token: 0x06001EF8 RID: 7928 RVA: 0x0013131C File Offset: 0x0012F51C
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

		// Token: 0x06001EF9 RID: 7929 RVA: 0x0013139C File Offset: 0x0012F59C
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

		// Token: 0x06001EFA RID: 7930 RVA: 0x00131424 File Offset: 0x0012F624
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

		// Token: 0x06001EFB RID: 7931 RVA: 0x00131558 File Offset: 0x0012F758
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

		// Token: 0x06001EFC RID: 7932 RVA: 0x001315EC File Offset: 0x0012F7EC
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

		// Token: 0x06001EFD RID: 7933 RVA: 0x001316BC File Offset: 0x0012F8BC
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

		// Token: 0x06001EFE RID: 7934 RVA: 0x001317B0 File Offset: 0x0012F9B0
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

		// Token: 0x06001EFF RID: 7935 RVA: 0x00131824 File Offset: 0x0012FA24
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

		// Token: 0x06001F00 RID: 7936 RVA: 0x00131944 File Offset: 0x0012FB44
		public void Update(float deltaTime)
		{
			if (this.ParentTrigger != null && !this.ParentTrigger.IsTriggered)
			{
				return;
			}
			bool isNotClient = GameMain.Client == null;
			if (!this.UseNetworkSyncing || isNotClient)
			{
				if (this.GlobalForceDecreaseInterval > 0f)
				{
					Level loaded = Level.Loaded;
					if (((loaded != null) ? loaded.LevelObjectManager : null) != null && Level.Loaded.LevelObjectManager.GlobalForceDecreaseTimer % (this.GlobalForceDecreaseInterval * 2f) < this.GlobalForceDecreaseInterval)
					{
						this.NeedsNetworkSyncing |= (this.currentForceFluctuation > 0f);
						this.currentForceFluctuation = 0f;
						goto IL_10A;
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
				IL_10A:
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

		// Token: 0x06001F01 RID: 7937 RVA: 0x00131D34 File Offset: 0x0012FF34
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

		// Token: 0x06001F02 RID: 7938 RVA: 0x00131DF4 File Offset: 0x0012FFF4
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

		// Token: 0x06001F03 RID: 7939 RVA: 0x00131FFC File Offset: 0x001301FC
		public static void ApplyAttacks(List<Attack> attacks, IDamageable damageable, Vector2 worldPosition, float deltaTime)
		{
			foreach (Attack attack in attacks)
			{
				attack.DoDamage(null, damageable, worldPosition, deltaTime, false, null, null);
			}
		}

		// Token: 0x06001F04 RID: 7940 RVA: 0x00132054 File Offset: 0x00130254
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

		// Token: 0x06001F05 RID: 7941 RVA: 0x001320C8 File Offset: 0x001302C8
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

		// Token: 0x06001F06 RID: 7942 RVA: 0x001322B3 File Offset: 0x001304B3
		public static float GetDistanceFactor(PhysicsBody triggererBody, PhysicsBody triggerBody, float colliderRadius)
		{
			return 1f - ConvertUnits.ToDisplayUnits(Vector2.Distance(triggererBody.SimPosition, triggerBody.SimPosition) - triggererBody.GetMaxExtent() / 2f) / colliderRadius;
		}

		// Token: 0x06001F07 RID: 7943 RVA: 0x001322E0 File Offset: 0x001304E0
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

		// Token: 0x06001F08 RID: 7944 RVA: 0x0013236C File Offset: 0x0013056C
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

		// Token: 0x06001F09 RID: 7945 RVA: 0x001323EC File Offset: 0x001305EC
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

		// Token: 0x04000FB3 RID: 4019
		public Action<LevelTrigger, Entity> OnTriggered;

		// Token: 0x04000FB4 RID: 4020
		private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();

		// Token: 0x04000FB5 RID: 4021
		private readonly List<Attack> attacks = new List<Attack>();

		// Token: 0x04000FB6 RID: 4022
		private readonly float cameraShake;

		// Token: 0x04000FB7 RID: 4023
		private Vector2 unrotatedForce;

		// Token: 0x04000FB8 RID: 4024
		private float forceFluctuationTimer;

		// Token: 0x04000FB9 RID: 4025
		private float currentForceFluctuation = 1f;

		// Token: 0x04000FBA RID: 4026
		private readonly HashSet<Entity> triggerers = new HashSet<Entity>();

		// Token: 0x04000FBB RID: 4027
		private readonly LevelTrigger.TriggererType triggeredBy;

		// Token: 0x04000FBC RID: 4028
		private readonly Identifier triggerSpeciesOrGroup;

		// Token: 0x04000FBD RID: 4029
		private readonly PropertyConditional.LogicalComparison conditionals;

		// Token: 0x04000FBE RID: 4030
		private readonly float randomTriggerInterval;

		// Token: 0x04000FBF RID: 4031
		private readonly float randomTriggerProbability;

		// Token: 0x04000FC0 RID: 4032
		private float randomTriggerTimer;

		// Token: 0x04000FC1 RID: 4033
		private float triggeredTimer;

		// Token: 0x04000FC2 RID: 4034
		private readonly HashSet<string> tags = new HashSet<string>();

		// Token: 0x04000FC3 RID: 4035
		private readonly HashSet<string> allowedOtherTriggerTags = new HashSet<string>();

		// Token: 0x04000FC4 RID: 4036
		private readonly float stayTriggeredDelay;

		// Token: 0x04000FC5 RID: 4037
		public LevelTrigger ParentTrigger;

		// Token: 0x04000FC7 RID: 4039
		private Vector2 worldPosition;

		// Token: 0x04000FCF RID: 4047
		private readonly LevelTrigger.TriggerForceMode forceMode;

		// Token: 0x04000FD6 RID: 4054
		private bool triggeredOnce;

		// Token: 0x04000FD7 RID: 4055
		private readonly bool triggerOnce;

		// Token: 0x04000FD8 RID: 4056
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x04000FD9 RID: 4057
		private static readonly List<Entity> triggerersToRemove = new List<Entity>();

		// Token: 0x02000B49 RID: 2889
		[Flags]
		public enum TriggererType
		{
			// Token: 0x04004736 RID: 18230
			None = 0,
			// Token: 0x04004737 RID: 18231
			Human = 1,
			// Token: 0x04004738 RID: 18232
			Creature = 2,
			// Token: 0x04004739 RID: 18233
			Character = 3,
			// Token: 0x0400473A RID: 18234
			Submarine = 4,
			// Token: 0x0400473B RID: 18235
			Item = 8,
			// Token: 0x0400473C RID: 18236
			OtherTrigger = 16
		}

		// Token: 0x02000B4A RID: 2890
		public enum TriggerForceMode
		{
			// Token: 0x0400473E RID: 18238
			Force,
			// Token: 0x0400473F RID: 18239
			Acceleration,
			// Token: 0x04004740 RID: 18240
			Impulse,
			// Token: 0x04004741 RID: 18241
			LimitVelocity
		}
	}
}
