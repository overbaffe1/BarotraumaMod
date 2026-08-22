using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Particles;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D3 RID: 1491
	internal class RepairTool : ItemComponent
	{
		// Token: 0x06005F62 RID: 24418 RVA: 0x0031AC14 File Offset: 0x00318E14
		private void EmitParticle(ParticleEmitter emitter, float deltaTime, Vector2 simPosition, Submarine targetSub)
		{
			Vector2 particlePos = ConvertUnits.ToDisplayUnits(simPosition);
			if (targetSub != null)
			{
				particlePos += targetSub.DrawPosition;
			}
			float particleAngle = this.item.body.Rotation + MathHelper.ToRadians(this.BarrelRotation) + ((this.item.body.Dir > 0f) ? 0f : 3.1415927f);
			Vector2 position = particlePos;
			Hull currentHull = this.item.CurrentHull;
			float angle = particleAngle + 3.1415927f;
			float particleRotation = -particleAngle + 3.1415927f;
			float velocityMultiplier = 1f;
			float sizeMultiplier = 1f;
			float amountMultiplier = 1f;
			Tuple<Vector2, Vector2> tracerPoints = new Tuple<Vector2, Vector2>(this.item.WorldPosition + this.TransformedBarrelPos, particlePos);
			emitter.Emit(deltaTime, position, currentHull, angle, particleRotation, velocityMultiplier, sizeMultiplier, amountMultiplier, null, null, false, tracerPoints);
		}

		// Token: 0x17001812 RID: 6162
		// (get) Token: 0x06005F63 RID: 24419 RVA: 0x0031ACD5 File Offset: 0x00318ED5
		// (set) Token: 0x06005F64 RID: 24420 RVA: 0x0031ACDD File Offset: 0x00318EDD
		[Serialize("Both", IsPropertySaveable.No, "Can the item be used in air, water or both.", "", false)]
		public RepairTool.UseEnvironment UsableIn { get; set; }

		// Token: 0x17001813 RID: 6163
		// (get) Token: 0x06005F65 RID: 24421 RVA: 0x0031ACE6 File Offset: 0x00318EE6
		// (set) Token: 0x06005F66 RID: 24422 RVA: 0x0031ACEE File Offset: 0x00318EEE
		[Serialize(0f, IsPropertySaveable.No, "The distance at which the item can repair targets.", "", false)]
		public float Range { get; set; }

		// Token: 0x17001814 RID: 6164
		// (get) Token: 0x06005F67 RID: 24423 RVA: 0x0031ACF7 File Offset: 0x00318EF7
		// (set) Token: 0x06005F68 RID: 24424 RVA: 0x0031ACFF File Offset: 0x00318EFF
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle when used by a character with sufficient skills to use the tool (in degrees).", "", false)]
		public float Spread { get; set; }

		// Token: 0x17001815 RID: 6165
		// (get) Token: 0x06005F69 RID: 24425 RVA: 0x0031AD08 File Offset: 0x00318F08
		// (set) Token: 0x06005F6A RID: 24426 RVA: 0x0031AD10 File Offset: 0x00318F10
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle when used by a character with insufficient skills to use the tool (in degrees).", "", false)]
		public float UnskilledSpread { get; set; }

		// Token: 0x17001816 RID: 6166
		// (get) Token: 0x06005F6B RID: 24427 RVA: 0x0031AD19 File Offset: 0x00318F19
		// (set) Token: 0x06005F6C RID: 24428 RVA: 0x0031AD21 File Offset: 0x00318F21
		[Serialize(0f, IsPropertySaveable.No, "How many units of damage the item removes from structures per second.", "", false)]
		public float StructureFixAmount { get; set; }

		// Token: 0x17001817 RID: 6167
		// (get) Token: 0x06005F6D RID: 24429 RVA: 0x0031AD2A File Offset: 0x00318F2A
		// (set) Token: 0x06005F6E RID: 24430 RVA: 0x0031AD32 File Offset: 0x00318F32
		[Serialize(0f, IsPropertySaveable.No, "How much damage is applied to ballast flora.", "", false)]
		public float FireDamage { get; set; }

		// Token: 0x17001818 RID: 6168
		// (get) Token: 0x06005F6F RID: 24431 RVA: 0x0031AD3B File Offset: 0x00318F3B
		// (set) Token: 0x06005F70 RID: 24432 RVA: 0x0031AD43 File Offset: 0x00318F43
		[Serialize(0f, IsPropertySaveable.No, "How many units of damage the item removes from destructible level walls per second.", "", false)]
		public float LevelWallFixAmount { get; set; }

		// Token: 0x17001819 RID: 6169
		// (get) Token: 0x06005F71 RID: 24433 RVA: 0x0031AD4C File Offset: 0x00318F4C
		// (set) Token: 0x06005F72 RID: 24434 RVA: 0x0031AD54 File Offset: 0x00318F54
		[Serialize(0f, IsPropertySaveable.No, "How much the item decreases the size of fires per second.", "", false)]
		public float ExtinguishAmount { get; set; }

		// Token: 0x1700181A RID: 6170
		// (get) Token: 0x06005F73 RID: 24435 RVA: 0x0031AD5D File Offset: 0x00318F5D
		// (set) Token: 0x06005F74 RID: 24436 RVA: 0x0031AD65 File Offset: 0x00318F65
		[Serialize(0f, IsPropertySaveable.No, "How much water the item provides to planters per second.", "", false)]
		public float WaterAmount { get; set; }

		// Token: 0x1700181B RID: 6171
		// (get) Token: 0x06005F75 RID: 24437 RVA: 0x0031AD6E File Offset: 0x00318F6E
		// (set) Token: 0x06005F76 RID: 24438 RVA: 0x0031AD76 File Offset: 0x00318F76
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The position of the barrel as an offset from the item's center (in pixels).", "", false)]
		public Vector2 BarrelPos { get; set; }

		// Token: 0x1700181C RID: 6172
		// (get) Token: 0x06005F77 RID: 24439 RVA: 0x0031AD7F File Offset: 0x00318F7F
		// (set) Token: 0x06005F78 RID: 24440 RVA: 0x0031AD87 File Offset: 0x00318F87
		[Serialize(false, IsPropertySaveable.No, "Can the item repair things through walls.", "", false)]
		public bool RepairThroughWalls { get; set; }

		// Token: 0x1700181D RID: 6173
		// (get) Token: 0x06005F79 RID: 24441 RVA: 0x0031AD90 File Offset: 0x00318F90
		// (set) Token: 0x06005F7A RID: 24442 RVA: 0x0031AD98 File Offset: 0x00318F98
		[Serialize(false, IsPropertySaveable.No, "Can the item repair multiple things at once, or will it only affect the first thing the ray from the barrel hits.", "", false)]
		public bool RepairMultiple { get; set; }

		// Token: 0x1700181E RID: 6174
		// (get) Token: 0x06005F7B RID: 24443 RVA: 0x0031ADA1 File Offset: 0x00318FA1
		// (set) Token: 0x06005F7C RID: 24444 RVA: 0x0031ADA9 File Offset: 0x00318FA9
		[Serialize(true, IsPropertySaveable.No, "Can the item repair multiple walls at once? Only relevant if RepairMultiple is true.", "", false)]
		public bool RepairMultipleWalls { get; set; }

		// Token: 0x1700181F RID: 6175
		// (get) Token: 0x06005F7D RID: 24445 RVA: 0x0031ADB2 File Offset: 0x00318FB2
		// (set) Token: 0x06005F7E RID: 24446 RVA: 0x0031ADBA File Offset: 0x00318FBA
		[Serialize(false, IsPropertySaveable.No, "Can the item repair things through holes in walls.", "", false)]
		public bool RepairThroughHoles { get; set; }

		// Token: 0x17001820 RID: 6176
		// (get) Token: 0x06005F7F RID: 24447 RVA: 0x0031ADC3 File Offset: 0x00318FC3
		// (set) Token: 0x06005F80 RID: 24448 RVA: 0x0031ADCB File Offset: 0x00318FCB
		[Serialize(100f, IsPropertySaveable.No, "How far two walls need to not be considered overlapping and to stop the ray.", "", false)]
		public float MaxOverlappingWallDist { get; set; }

		// Token: 0x17001821 RID: 6177
		// (get) Token: 0x06005F81 RID: 24449 RVA: 0x0031ADD4 File Offset: 0x00318FD4
		// (set) Token: 0x06005F82 RID: 24450 RVA: 0x0031ADDC File Offset: 0x00318FDC
		[Serialize(1f, IsPropertySaveable.No, "How fast the tool detaches level resources (e.g. minerals). Acts as a multiplier on the speed: with a value of 2, detaching an item whose DeattachDuration is set to 30 seconds would take 15 seconds.", "", false)]
		public float DeattachSpeed { get; set; }

		// Token: 0x17001822 RID: 6178
		// (get) Token: 0x06005F83 RID: 24451 RVA: 0x0031ADE5 File Offset: 0x00318FE5
		// (set) Token: 0x06005F84 RID: 24452 RVA: 0x0031ADED File Offset: 0x00318FED
		[Serialize(true, IsPropertySaveable.No, "Can the item hit doors.", "", false)]
		public bool HitItems { get; set; }

		// Token: 0x17001823 RID: 6179
		// (get) Token: 0x06005F85 RID: 24453 RVA: 0x0031ADF6 File Offset: 0x00318FF6
		// (set) Token: 0x06005F86 RID: 24454 RVA: 0x0031ADFE File Offset: 0x00318FFE
		[Serialize(false, IsPropertySaveable.No, "Can the item hit broken doors.", "", false)]
		public bool HitBrokenDoors { get; set; }

		// Token: 0x17001824 RID: 6180
		// (get) Token: 0x06005F87 RID: 24455 RVA: 0x0031AE07 File Offset: 0x00319007
		// (set) Token: 0x06005F88 RID: 24456 RVA: 0x0031AE0F File Offset: 0x0031900F
		[Serialize(false, IsPropertySaveable.No, "Should the tool ignore characters? Enabled e.g. for fire extinguisher.", "", false)]
		public bool IgnoreCharacters { get; set; }

		// Token: 0x17001825 RID: 6181
		// (get) Token: 0x06005F89 RID: 24457 RVA: 0x0031AE18 File Offset: 0x00319018
		// (set) Token: 0x06005F8A RID: 24458 RVA: 0x0031AE20 File Offset: 0x00319020
		[Serialize(0f, IsPropertySaveable.No, "The probability of starting a fire somewhere along the ray fired from the barrel (for example, 0.1 = 10% chance to start a fire during a second of use).", "", false)]
		public float FireProbability { get; set; }

		// Token: 0x17001826 RID: 6182
		// (get) Token: 0x06005F8B RID: 24459 RVA: 0x0031AE29 File Offset: 0x00319029
		// (set) Token: 0x06005F8C RID: 24460 RVA: 0x0031AE31 File Offset: 0x00319031
		[Serialize(0f, IsPropertySaveable.No, "Force applied to the entity the ray hits.", "", false)]
		public float TargetForce { get; set; }

		// Token: 0x17001827 RID: 6183
		// (get) Token: 0x06005F8D RID: 24461 RVA: 0x0031AE3A File Offset: 0x0031903A
		// (set) Token: 0x06005F8E RID: 24462 RVA: 0x0031AE42 File Offset: 0x00319042
		[Serialize(0f, IsPropertySaveable.No, "Rotation of the barrel in degrees.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 360f, VectorComponentLabels = new string[]
		{
			"editable.minvalue",
			"editable.maxvalue"
		})]
		public float BarrelRotation { get; set; }

		// Token: 0x17001828 RID: 6184
		// (get) Token: 0x06005F8F RID: 24463 RVA: 0x0031AE4C File Offset: 0x0031904C
		public Vector2 TransformedBarrelPos
		{
			get
			{
				if (this.item.body == null)
				{
					return this.BarrelPos;
				}
				Matrix bodyTransform = Matrix.CreateRotationZ(this.item.body.Rotation + MathHelper.ToRadians(this.BarrelRotation));
				Vector2 flippedPos = this.BarrelPos;
				if (this.item.body.Dir < 0f)
				{
					flippedPos.X = -flippedPos.X;
				}
				return Vector2.Transform(flippedPos, bodyTransform);
			}
		}

		// Token: 0x06005F90 RID: 24464 RVA: 0x0031AEC4 File Offset: 0x003190C4
		public RepairTool(Item item, ContentXElement element) : base(item, element)
		{
			this.item = item;
			if (element.GetAttribute("limbfixamount") != null)
			{
				DebugConsole.ThrowError("Error in item \"" + item.Name + "\" - RepairTool damage should be configured using a StatusEffect with Afflictions, not the limbfixamount attribute.", null, element.ContentPackage, false, false);
			}
			this.fixableEntities = new HashSet<Identifier>();
			this.nonFixableEntities = new HashSet<Identifier>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "fixable"))
				{
					if (a == "nonfixable")
					{
						foreach (Identifier id in subElement.GetAttributeIdentifierArray("identifier", Array.Empty<Identifier>(), true))
						{
							this.nonFixableEntities.Add(id);
						}
					}
				}
				else if (subElement.GetAttribute("name") != null)
				{
					DebugConsole.ThrowError("Error in RepairTool " + item.Name + " - use identifiers instead of names to configure fixable entities.", null, element.ContentPackage, false, false);
					this.fixableEntities.Add(subElement.GetAttribute("name").Value.ToIdentifier());
				}
				else
				{
					foreach (Identifier id2 in subElement.GetAttributeIdentifierArray("identifier", Array.Empty<Identifier>(), true))
					{
						this.fixableEntities.Add(id2);
					}
				}
			}
			item.IsShootable = true;
			item.RequireAimToUse = element.Parent.GetAttributeBool("RequireAimToUse", true);
			this.InitProjSpecific(element);
		}

		// Token: 0x06005F91 RID: 24465 RVA: 0x0031B0FC File Offset: 0x003192FC
		private void InitProjSpecific(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "particleemitter"))
				{
					if (!(a == "particleemitterhititem"))
					{
						if (!(a == "particleemitterhitstructure"))
						{
							if (a == "particleemitterhitcharacter")
							{
								this.particleEmitterHitCharacter.Add(new ParticleEmitter(subElement));
							}
						}
						else
						{
							this.particleEmitterHitStructure.Add(new ParticleEmitter(subElement));
						}
					}
					else
					{
						Identifier[] identifiers = subElement.GetAttributeIdentifierArray("identifiers", Array.Empty<Identifier>(), true);
						if (identifiers.Length == 0)
						{
							identifiers = subElement.GetAttributeIdentifierArray("identifier", Array.Empty<Identifier>(), true);
						}
						Identifier[] excludedIdentifiers = subElement.GetAttributeIdentifierArray("excludedidentifiers", Array.Empty<Identifier>(), true);
						if (excludedIdentifiers.Length == 0)
						{
							excludedIdentifiers = subElement.GetAttributeIdentifierArray("excludedidentifier", Array.Empty<Identifier>(), true);
						}
						this.particleEmitterHitItem.Add(new ValueTuple<RelatedItem, ParticleEmitter>(new RelatedItem(identifiers, excludedIdentifiers), new ParticleEmitter(subElement)));
					}
				}
				else
				{
					this.particleEmitters.Add(new ParticleEmitter(subElement));
				}
			}
		}

		// Token: 0x06005F92 RID: 24466 RVA: 0x0031B254 File Offset: 0x00319454
		public override void Update(float deltaTime, Camera cam)
		{
			this.activeTimer -= deltaTime;
			if (this.activeTimer <= 0f)
			{
				this.IsActive = false;
			}
		}

		// Token: 0x06005F93 RID: 24467 RVA: 0x0031B278 File Offset: 0x00319478
		public override bool Use(float deltaTime, Character character = null)
		{
			if (character != null && this.item.RequireAimToUse && !character.IsKeyDown(InputType.Aim))
			{
				return false;
			}
			float degreeOfSuccess = (character == null) ? 0.5f : base.DegreeOfSuccess(character);
			bool failed = false;
			if (Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) > degreeOfSuccess)
			{
				base.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character, null, null, null, null, 1f);
				failed = true;
			}
			if (this.UsableIn == RepairTool.UseEnvironment.None)
			{
				base.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character, null, null, null, null, 1f);
				failed = true;
			}
			if (this.item.InWater)
			{
				if (this.UsableIn == RepairTool.UseEnvironment.Air)
				{
					base.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character, null, null, null, null, 1f);
					failed = true;
				}
			}
			else if (this.UsableIn == RepairTool.UseEnvironment.Water)
			{
				base.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character, null, null, null, null, 1f);
				failed = true;
			}
			if (failed)
			{
				base.ApplyStatusEffects(ActionType.OnUse, deltaTime, character, null, null, null, null, 1f);
				return false;
			}
			Vector2 sourcePos = (((character != null) ? character.AnimController : null) == null) ? this.item.SimPosition : character.AnimController.AimSourceSimPos;
			Vector2 barrelPos = this.item.SimPosition + ConvertUnits.ToSimUnits(this.TransformedBarrelPos);
			Vector2 rayStart;
			Vector2 rayStartWorld;
			if (Submarine.PickBody(sourcePos, barrelPos, null, new Category?(Category.Cat1 | Category.Cat6 | Category.Cat8), true, null, false) == null)
			{
				rayStart = ConvertUnits.ToSimUnits(this.item.Position + this.TransformedBarrelPos);
				rayStartWorld = ConvertUnits.ToSimUnits(this.item.WorldPosition + this.TransformedBarrelPos);
			}
			else
			{
				rayStartWorld = (rayStart = Submarine.LastPickedPosition + Submarine.LastPickedNormal * 0.1f);
				if (this.item.Submarine != null)
				{
					rayStartWorld += this.item.Submarine.SimPosition;
				}
			}
			if (this.item.CurrentHull != null)
			{
				Hull barrelHull = Hull.FindHull(ConvertUnits.ToDisplayUnits(rayStartWorld), this.item.CurrentHull, true, true);
				if (barrelHull != null && barrelHull != this.item.CurrentHull)
				{
					Vector2 hullIntersection;
					if (MathUtils.GetLineWorldRectangleIntersection(ConvertUnits.ToDisplayUnits(sourcePos), ConvertUnits.ToDisplayUnits(rayStart), this.item.CurrentHull.Rect, out hullIntersection) && !this.item.CurrentHull.ConnectedGaps.Any((Gap g) => g.Open > 0f && Submarine.RectContains(g.Rect, hullIntersection, false)))
					{
						Vector2 rayDir = rayStart.NearlyEquals(sourcePos) ? Vector2.Zero : Vector2.Normalize(rayStart - sourcePos);
						rayStartWorld = ConvertUnits.ToSimUnits(hullIntersection - rayDir * 5f);
						if (this.item.Submarine != null)
						{
							rayStartWorld += this.item.Submarine.SimPosition;
						}
					}
				}
			}
			float spread = MathHelper.ToRadians(MathHelper.Lerp(this.UnskilledSpread, this.Spread, degreeOfSuccess));
			float angle = MathHelper.ToRadians(this.BarrelRotation) + spread * Rand.Range(-0.5f, 0.5f, Rand.RandSync.Unsynced);
			float dir = 1f;
			if (this.item.body != null)
			{
				angle += this.item.body.Rotation;
				dir = this.item.body.Dir;
			}
			Vector2 rayEnd = rayStartWorld + ConvertUnits.ToSimUnits(new Vector2((float)Math.Cos((double)angle), (float)Math.Sin((double)angle)) * this.Range * dir);
			this.ignoredBodies.Clear();
			if (character != null)
			{
				foreach (Limb limb in character.AnimController.Limbs)
				{
					if (Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) <= degreeOfSuccess)
					{
						this.ignoredBodies.Add(limb.body.FarseerBody);
					}
				}
				this.ignoredBodies.Add(character.AnimController.Collider.FarseerBody);
			}
			this.IsActive = true;
			this.activeTimer = 0.1f;
			this.debugRayStartPos = ConvertUnits.ToDisplayUnits(rayStartWorld);
			this.debugRayEndPos = ConvertUnits.ToDisplayUnits(rayEnd);
			Submarine parentSub = ((character != null) ? character.Submarine : null) ?? this.item.Submarine;
			if (parentSub == null)
			{
				foreach (Submarine sub in Submarine.Loaded)
				{
					Rectangle subBorders = sub.Borders;
					subBorders.Location += new Point((int)sub.WorldPosition.X, (int)sub.WorldPosition.Y - sub.Borders.Height);
					if (MathUtils.CircleIntersectsRectangle(this.item.WorldPosition, this.Range * 5f, subBorders))
					{
						this.Repair(rayStartWorld - sub.SimPosition, rayEnd - sub.SimPosition, deltaTime, character, degreeOfSuccess, this.ignoredBodies);
					}
				}
				this.Repair(rayStartWorld, rayEnd, deltaTime, character, degreeOfSuccess, this.ignoredBodies);
			}
			else
			{
				this.Repair(rayStartWorld - parentSub.SimPosition, rayEnd - parentSub.SimPosition, deltaTime, character, degreeOfSuccess, this.ignoredBodies);
			}
			this.UseProjSpecific(deltaTime, rayStartWorld);
			return true;
		}

		// Token: 0x06005F94 RID: 24468 RVA: 0x0031B7F0 File Offset: 0x003199F0
		private void UseProjSpecific(float deltaTime, Vector2 raystart)
		{
			foreach (ParticleEmitter particleEmitter in this.particleEmitters)
			{
				float particleAngle = MathHelper.ToRadians(this.BarrelRotation);
				if (this.item.body != null)
				{
					particleAngle += this.item.body.Rotation + ((this.item.body.Dir > 0f) ? 0f : 3.1415927f);
				}
				particleEmitter.Emit(deltaTime, ConvertUnits.ToDisplayUnits(raystart), this.item.CurrentHull, particleAngle, particleEmitter.Prefab.Properties.CopyEntityAngle ? (-particleAngle) : 0f, 1f, 1f, 1f, null, null, false, null);
			}
		}

		// Token: 0x06005F95 RID: 24469 RVA: 0x0031B8E4 File Offset: 0x00319AE4
		private void Repair(Vector2 rayStart, Vector2 rayEnd, float deltaTime, Character user, float degreeOfSuccess, List<Body> ignoredBodies)
		{
			Category collisionCategories = Category.Cat1 | Category.Cat5 | Category.Cat6 | Category.Cat8 | Category.Cat9;
			if (!this.IgnoreCharacters)
			{
				collisionCategories |= Category.Cat2;
			}
			if (this.statusEffectLists != null && (RepairTool.<Repair>g__CanSeverJoints|113_0(ActionType.OnUse, this.statusEffectLists) || RepairTool.<Repair>g__CanSeverJoints|113_0(ActionType.OnSuccess, this.statusEffectLists)))
			{
				float rangeSqr = ConvertUnits.ToSimUnits(this.Range);
				rangeSqr *= rangeSqr;
				foreach (Character c in Character.CharacterList)
				{
					if (c.Enabled && c.AnimController.BodyInRest && Math.Abs(c.WorldPosition.X - this.item.WorldPosition.X) <= 1000f && Math.Abs(c.WorldPosition.Y - this.item.WorldPosition.Y) <= 1000f)
					{
						foreach (Limb limb in c.AnimController.Limbs)
						{
							if (Vector2.DistanceSquared(limb.SimPosition, this.item.SimPosition) < rangeSqr && Vector2.Dot(rayEnd - rayStart, limb.SimPosition - rayStart) > 0f)
							{
								c.AnimController.BodyInRest = false;
								break;
							}
						}
					}
				}
			}
			float lastPickedFraction = 0f;
			if (this.RepairMultiple)
			{
				IEnumerable<Body> bodies = Submarine.PickBodies(rayStart, rayEnd, ignoredBodies, new Category?(collisionCategories), false, delegate(Fixture f)
				{
					if (f.IsSensor)
					{
						if (this.RepairThroughHoles)
						{
							Body body2 = f.Body;
							if (((body2 != null) ? body2.UserData : null) is Structure)
							{
								return false;
							}
						}
						Body body3 = f.Body;
						if (((body3 != null) ? body3.UserData : null) is PhysicsBody)
						{
							return false;
						}
					}
					Body body4 = f.Body;
					Item it2 = ((body4 != null) ? body4.UserData : null) as Item;
					if (it2 != null && it2.GetComponent<Planter>() != null)
					{
						return false;
					}
					Body body5 = f.Body;
					if (((body5 != null) ? body5.UserData : null) as string == "ruinroom")
					{
						return false;
					}
					Body body6 = f.Body;
					return !(((body6 != null) ? body6.UserData : null) is VineTile) || this.FireDamage > 0f;
				}, true);
				RepairTool.hitBodies.Clear();
				RepairTool.hitBodies.AddRange(bodies.Distinct<Body>());
				lastPickedFraction = Submarine.LastPickedFraction;
				Type lastHitType = null;
				this.hitCharacters.Clear();
				using (List<Body>.Enumerator enumerator2 = RepairTool.hitBodies.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Body body = enumerator2.Current;
						object userData = body.UserData;
						Type bodyType = (userData != null) ? userData.GetType() : null;
						if (!this.RepairThroughWalls && bodyType != null && bodyType != lastHitType)
						{
							if (lastHitType == typeof(Item))
							{
								break;
							}
							if (lastHitType == typeof(Structure))
							{
								break;
							}
						}
						if (!this.RepairMultipleWalls)
						{
							if (bodyType == typeof(Structure))
							{
								break;
							}
							Item item = body.UserData as Item;
							if (((item != null) ? item.GetComponent<Door>() : null) != null)
							{
								break;
							}
						}
						Character hitCharacter = null;
						Limb limb2 = body.UserData as Limb;
						if (limb2 != null)
						{
							hitCharacter = limb2.character;
						}
						else
						{
							Character character = body.UserData as Character;
							if (character != null)
							{
								hitCharacter = character;
							}
						}
						if (hitCharacter != null)
						{
							if (this.hitCharacters.Contains(hitCharacter))
							{
								continue;
							}
							this.hitCharacters.Add(hitCharacter);
						}
						float thisBodyFraction = Submarine.LastPickedBodyDist(body);
						if (!this.RepairThroughWalls && lastHitType == typeof(Structure) && this.Range * (thisBodyFraction - lastPickedFraction) > this.MaxOverlappingWallDist)
						{
							break;
						}
						this.pickedPosition = rayStart + (rayEnd - rayStart) * thisBodyFraction;
						if (this.FixBody(user, this.pickedPosition, deltaTime, degreeOfSuccess, body))
						{
							lastPickedFraction = thisBodyFraction;
							if (bodyType != null)
							{
								lastHitType = bodyType;
							}
						}
					}
					goto IL_3B1;
				}
			}
			Body pickedBody = Submarine.PickBody(rayStart, rayEnd, ignoredBodies, new Category?(collisionCategories), false, delegate(Fixture f)
			{
				if (f.IsSensor)
				{
					if (this.RepairThroughHoles)
					{
						Body body2 = f.Body;
						if (((body2 != null) ? body2.UserData : null) is Structure)
						{
							return false;
						}
					}
					Body body3 = f.Body;
					if (((body3 != null) ? body3.UserData : null) is PhysicsBody)
					{
						return false;
					}
				}
				Body body4 = f.Body;
				if (((body4 != null) ? body4.UserData : null) as string == "ruinroom")
				{
					return false;
				}
				Body body5 = f.Body;
				if (((body5 != null) ? body5.UserData : null) is VineTile && this.FireDamage <= 0f)
				{
					return false;
				}
				Body body6 = f.Body;
				Item targetItem = ((body6 != null) ? body6.UserData : null) as Item;
				if (targetItem != null)
				{
					if (!this.HitItems)
					{
						return false;
					}
					if (this.HitBrokenDoors)
					{
						if (targetItem.GetComponent<Door>() == null && targetItem.Condition <= 0f)
						{
							return false;
						}
					}
					else if (targetItem.Condition <= 0f)
					{
						return false;
					}
				}
				Body body7 = f.Body;
				return ((body7 != null) ? body7.UserData : null) != null;
			}, true);
			this.pickedPosition = Submarine.LastPickedPosition;
			this.FixBody(user, this.pickedPosition, deltaTime, degreeOfSuccess, pickedBody);
			lastPickedFraction = Submarine.LastPickedFraction;
			IL_3B1:
			if (this.ExtinguishAmount > 0f && this.item.CurrentHull != null)
			{
				this.fireSourcesInRange.Clear();
				for (float x = 0f; x <= lastPickedFraction; x += 0.1f)
				{
					Vector2 displayPos = ConvertUnits.ToDisplayUnits(rayStart + (rayEnd - rayStart) * x);
					if (this.item.CurrentHull.Submarine != null)
					{
						displayPos += this.item.CurrentHull.Submarine.Position;
					}
					Hull hull = Hull.FindHull(displayPos, this.item.CurrentHull, true, true);
					if (hull != null)
					{
						foreach (FireSource fs in hull.FireSources)
						{
							if (fs.IsInDamageRange(displayPos, 100f) && !this.fireSourcesInRange.Contains(fs))
							{
								this.fireSourcesInRange.Add(fs);
							}
						}
						foreach (FireSource fs2 in hull.FakeFireSources)
						{
							if (fs2.IsInDamageRange(displayPos, 100f) && !this.fireSourcesInRange.Contains(fs2))
							{
								this.fireSourcesInRange.Add(fs2);
							}
						}
					}
				}
				foreach (FireSource fs3 in this.fireSourcesInRange)
				{
					fs3.Extinguish(deltaTime, this.ExtinguishAmount);
				}
			}
			if (this.WaterAmount > 0f && this.item.Submarine != null)
			{
				Vector2 pos = ConvertUnits.ToDisplayUnits(rayStart + this.item.Submarine.SimPosition);
				foreach (Item it in Item.ItemList)
				{
					if (it.Submarine == this.item.Submarine)
					{
						Planter planter = it.GetComponent<Planter>();
						if (planter != null)
						{
							Holdable holdable = it.GetComponent<Holdable>();
							if (holdable == null || !holdable.Attachable || holdable.Attached)
							{
								Rectangle collisionRect = it.WorldRect;
								collisionRect.Y -= collisionRect.Height;
								if ((float)collisionRect.Left < pos.X && (float)collisionRect.Right > pos.X && (float)collisionRect.Bottom < pos.Y && Submarine.PickBody(rayStart, it.SimPosition, ignoredBodies, new Category?(collisionCategories), true, null, false) == null)
								{
									for (int i = 0; i < planter.GrowableSeeds.Length; i++)
									{
										Growable seed = planter.GrowableSeeds[i];
										if (seed != null && !seed.Decayed)
										{
											seed.Health += this.WaterAmount * deltaTime;
											float barOffset = 10f * GUI.Scale;
											Vector2 offset = planter.PlantSlots.ContainsKey(i) ? planter.PlantSlots[i].Offset : Vector2.Zero;
											if (user != null)
											{
												user.UpdateHUDProgressBar(planter, planter.Item.DrawPosition + new Vector2(barOffset, 0f) + offset, seed.Health / seed.MaxWater, GUIStyle.Blue, GUIStyle.Blue, "progressbar.watering");
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if ((GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.FireProbability * deltaTime && this.item.CurrentHull != null)
			{
				Vector2 displayPos2 = ConvertUnits.ToDisplayUnits(rayStart + (rayEnd - rayStart) * lastPickedFraction * 0.9f);
				if (this.item.CurrentHull.Submarine != null)
				{
					displayPos2 += this.item.CurrentHull.Submarine.Position;
				}
				new FireSource(displayPos2, null, user, false);
			}
		}

		// Token: 0x06005F96 RID: 24470 RVA: 0x0031C19C File Offset: 0x0031A39C
		private bool FixBody(Character user, Vector2 hitPosition, float deltaTime, float degreeOfSuccess, Body targetBody)
		{
			if (targetBody == null || targetBody.UserData == null)
			{
				return false;
			}
			object userData = targetBody.UserData;
			Structure targetStructure = userData as Structure;
			if (targetStructure != null)
			{
				if (targetStructure.IsPlatform)
				{
					return false;
				}
				int sectionIndex = targetStructure.FindSectionIndex(ConvertUnits.ToDisplayUnits(this.pickedPosition), false, false);
				if (sectionIndex < 0)
				{
					return false;
				}
				if (!this.fixableEntities.Contains("structure") && !this.fixableEntities.Contains(targetStructure.Prefab.Identifier))
				{
					return true;
				}
				if (this.nonFixableEntities.Contains(targetStructure.Prefab.Identifier) || this.nonFixableEntities.Any((Identifier t) => targetStructure.Tags.Contains(t)))
				{
					return false;
				}
				this.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnUse, null, null, null, targetStructure);
				this.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnSuccess, null, null, null, targetStructure);
				this.FixStructureProjSpecific(user, deltaTime, targetStructure, sectionIndex);
				float structureFixAmount = this.StructureFixAmount;
				if (structureFixAmount >= 0f)
				{
					structureFixAmount *= 1f + user.GetStatValue(StatTypes.RepairToolStructureRepairMultiplier, true);
					structureFixAmount *= 1f + this.item.GetQualityModifier(Quality.StatType.RepairToolStructureRepairMultiplier);
				}
				else
				{
					structureFixAmount *= 1f + user.GetStatValue(StatTypes.RepairToolStructureDamageMultiplier, true);
					structureFixAmount *= 1f + this.item.GetQualityModifier(Quality.StatType.RepairToolStructureDamageMultiplier);
				}
				bool didLeak = targetStructure.SectionIsLeakingFromOutside(sectionIndex);
				targetStructure.AddDamage(sectionIndex, -structureFixAmount * degreeOfSuccess, user, true, false);
				if (didLeak && !targetStructure.SectionIsLeakingFromOutside(sectionIndex))
				{
					user.CheckTalents(AbilityEffectType.OnRepairedOutsideLeak);
				}
				for (int i = -1; i < 2; i += 2)
				{
					int nextSectionLength = targetStructure.SectionLength(sectionIndex + i);
					if ((sectionIndex == 1 && i == -1) || (sectionIndex == targetStructure.SectionCount - 2 && i == 1) || (nextSectionLength > 0 && (float)nextSectionLength < 28.800001f))
					{
						targetStructure.AddDamage(sectionIndex + i, -structureFixAmount * degreeOfSuccess, null, true, false);
					}
				}
				return true;
			}
			else
			{
				userData = targetBody.UserData;
				VoronoiCell cell = userData as VoronoiCell;
				if (cell != null && cell.IsDestructible)
				{
					Level loaded = Level.Loaded;
					DestructibleLevelWall levelWall = ((loaded != null) ? loaded.ExtraWalls.Find((LevelWall w) => w.Body == cell.Body) : null) as DestructibleLevelWall;
					if (levelWall != null)
					{
						levelWall.AddDamage(-this.LevelWallFixAmount * deltaTime, ConvertUnits.ToDisplayUnits(hitPosition));
					}
					return true;
				}
				LevelObject levelObject = targetBody.UserData as LevelObject;
				if (levelObject != null && levelObject.Prefab.TakeLevelWallDamage)
				{
					levelObject.AddDamage(-this.LevelWallFixAmount, deltaTime, this.item, false);
					return true;
				}
				Character targetCharacter = targetBody.UserData as Character;
				if (targetCharacter != null)
				{
					if (targetCharacter.Removed)
					{
						return false;
					}
					targetCharacter.LastDamageSource = this.item;
					Limb closestLimb = null;
					float closestDist = float.MaxValue;
					foreach (Limb limb in targetCharacter.AnimController.Limbs)
					{
						if (!limb.Removed && !limb.IgnoreCollisions && !limb.Hidden && !limb.IsSevered)
						{
							float dist = Vector2.DistanceSquared(this.item.SimPosition, limb.SimPosition);
							if (dist < closestDist)
							{
								closestLimb = limb;
								closestDist = dist;
							}
						}
					}
					if (closestLimb != null && !MathUtils.NearlyEqual(this.TargetForce, 0f, 0.0001f))
					{
						Vector2 dir = closestLimb.WorldPosition - this.item.WorldPosition;
						dir = ((dir.LengthSquared() < 0.0001f) ? Vector2.UnitY : Vector2.Normalize(dir));
						closestLimb.body.ApplyForce(dir * this.TargetForce, 10f);
					}
					this.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnUse, null, targetCharacter, closestLimb, null);
					this.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnSuccess, null, targetCharacter, closestLimb, null);
					this.FixCharacterProjSpecific(user, deltaTime, targetCharacter);
					return true;
				}
				else
				{
					Limb targetLimb = targetBody.UserData as Limb;
					if (targetLimb != null)
					{
						if (targetLimb.character == null || targetLimb.character.Removed)
						{
							return false;
						}
						if (!MathUtils.NearlyEqual(this.TargetForce, 0f, 0.0001f))
						{
							Vector2 dir2 = targetLimb.WorldPosition - this.item.WorldPosition;
							dir2 = ((dir2.LengthSquared() < 0.0001f) ? Vector2.UnitY : Vector2.Normalize(dir2));
							targetLimb.body.ApplyForce(dir2 * this.TargetForce, 10f);
						}
						targetLimb.character.LastDamageSource = this.item;
						this.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnUse, null, targetLimb.character, targetLimb, null);
						this.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnSuccess, null, targetLimb.character, targetLimb, null);
						this.FixCharacterProjSpecific(user, deltaTime, targetLimb.character);
						return true;
					}
					else
					{
						userData = targetBody.UserData;
						bool flag = userData is Item || userData is Holdable;
						if (!flag)
						{
							BallastFloraBranch branch = targetBody.UserData as BallastFloraBranch;
							if (branch != null)
							{
								BallastFloraBehavior ballastFlora = branch.ParentBallastFlora;
								if (ballastFlora != null)
								{
									ballastFlora.DamageBranch(branch, this.FireDamage * deltaTime, BallastFloraBehavior.AttackType.Fire, user);
								}
							}
							return false;
						}
						Holdable holdable = targetBody.UserData as Holdable;
						Item targetItem = (holdable != null) ? holdable.Item : ((Item)targetBody.UserData);
						if (!this.HitItems || !targetItem.IsInteractable(user))
						{
							return false;
						}
						LevelResource levelResource = targetItem.GetComponent<LevelResource>();
						if (levelResource != null && levelResource.Attached && levelResource.RequiredItems.Any<KeyValuePair<RelatedItem.RelationType, List<RelatedItem>>>() && levelResource.HasRequiredItems(user, false, null))
						{
							float addedDetachTime = deltaTime * this.DeattachSpeed * (1f + user.GetStatValue(StatTypes.RepairToolDeattachTimeMultiplier, true)) * (1f + this.item.GetQualityModifier(Quality.StatType.RepairToolDeattachTimeMultiplier));
							levelResource.DeattachTimer += addedDetachTime;
							if (targetItem.Prefab.ShowHealthBar && Character.Controlled != null && (user == Character.Controlled || Character.Controlled.CanSeeTarget(this.item, null, false, false)))
							{
								Character.Controlled.UpdateHUDProgressBar(this, targetItem.WorldPosition, levelResource.DeattachTimer / levelResource.DeattachDuration, GUIStyle.Red, GUIStyle.Green, "progressbar.deattaching");
							}
							this.FixItemProjSpecific(user, deltaTime, targetItem, false);
							return true;
						}
						if (!targetItem.Prefab.DamagedByRepairTools)
						{
							return false;
						}
						if (this.HitBrokenDoors)
						{
							if (targetItem.GetComponent<Door>() == null && targetItem.Condition <= 0f)
							{
								return false;
							}
						}
						else if (targetItem.Condition <= 0f)
						{
							return false;
						}
						targetItem.IsHighlighted = true;
						this.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnUse, targetItem, null, null, null);
						this.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnSuccess, targetItem, null, null, null);
						if (targetItem.body != null && !MathUtils.NearlyEqual(this.TargetForce, 0f, 0.0001f))
						{
							Vector2 dir3 = targetItem.WorldPosition - this.item.WorldPosition;
							dir3 = ((dir3.LengthSquared() < 0.0001f) ? Vector2.UnitY : Vector2.Normalize(dir3));
							targetItem.body.ApplyForce(dir3 * this.TargetForce, 10f);
						}
						this.FixItemProjSpecific(user, deltaTime, targetItem, true);
						return true;
					}
				}
			}
		}

		// Token: 0x06005F97 RID: 24471 RVA: 0x0031C900 File Offset: 0x0031AB00
		private void FixStructureProjSpecific(Character user, float deltaTime, Structure targetStructure, int sectionIndex)
		{
			Vector2 progressBarPos = targetStructure.SectionPosition(sectionIndex, false);
			if (targetStructure.Submarine != null)
			{
				progressBarPos += targetStructure.Submarine.DrawPosition;
			}
			HUDProgressBar progressBar = user.UpdateHUDProgressBar((int)(targetStructure.ID * 1000) + sectionIndex, progressBarPos, MathUtils.InverseLerp(targetStructure.Prefab.MinHealth, targetStructure.Health, targetStructure.Health - targetStructure.SectionDamage(sectionIndex)), GUIStyle.Red, GUIStyle.Green, "");
			if (progressBar != null)
			{
				progressBar.Size = new Vector2(60f, 20f);
			}
			foreach (ParticleEmitter emitter in this.particleEmitterHitStructure)
			{
				this.EmitParticle(emitter, deltaTime, this.pickedPosition, targetStructure.Submarine);
			}
		}

		// Token: 0x06005F98 RID: 24472 RVA: 0x0031C9F8 File Offset: 0x0031ABF8
		private void FixCharacterProjSpecific(Character user, float deltaTime, Character targetCharacter)
		{
			foreach (ParticleEmitter emitter in this.particleEmitterHitCharacter)
			{
				this.EmitParticle(emitter, deltaTime, this.pickedPosition, targetCharacter.Submarine);
			}
		}

		// Token: 0x06005F99 RID: 24473 RVA: 0x0031CA58 File Offset: 0x0031AC58
		private void FixItemProjSpecific(Character user, float deltaTime, Item targetItem, bool showProgressBar)
		{
			if (showProgressBar)
			{
				float progressBarState = targetItem.ConditionPercentage / 100f;
				if (!MathUtils.NearlyEqual(progressBarState, this.prevProgressBarState, 0.0001f) || this.prevProgressBarTarget != targetItem)
				{
					Door door = targetItem.GetComponent<Door>();
					if (door == null || door.Stuck <= 0f)
					{
						Vector2 progressBarPos = targetItem.DrawPosition;
						HUDProgressBar progressBar = (user != null) ? user.UpdateHUDProgressBar(targetItem, progressBarPos, progressBarState, GUIStyle.Red, GUIStyle.Green, (progressBarState < this.prevProgressBarState) ? "progressbar.cutting" : "") : null;
						if (progressBar != null)
						{
							progressBar.Size = new Vector2(60f, 20f);
						}
					}
					this.prevProgressBarState = progressBarState;
					this.prevProgressBarTarget = targetItem;
				}
			}
			foreach (ValueTuple<RelatedItem, ParticleEmitter> valueTuple in this.particleEmitterHitItem)
			{
				RelatedItem relatedItem = valueTuple.Item1;
				ParticleEmitter emitter = valueTuple.Item2;
				if (relatedItem.MatchesItem(targetItem))
				{
					this.EmitParticle(emitter, deltaTime, this.pickedPosition, targetItem.Submarine);
				}
			}
		}

		// Token: 0x06005F9A RID: 24474 RVA: 0x0031CB80 File Offset: 0x0031AD80
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			RepairTool.<>c__DisplayClass122_0 CS$<>8__locals1 = new RepairTool.<>c__DisplayClass122_0();
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			Gap leak = objective.OperateTarget as Gap;
			if (leak == null)
			{
				CS$<>8__locals1.<CrewAIOperate>g__Reset|3();
				return true;
			}
			if (leak.Submarine == null || leak.Submarine != CS$<>8__locals1.character.Submarine)
			{
				CS$<>8__locals1.<CrewAIOperate>g__Reset|3();
				return true;
			}
			if (leak != this.previousGap)
			{
				CS$<>8__locals1.<CrewAIOperate>g__Reset|3();
				this.previousGap = leak;
			}
			Vector2 fromCharacterToLeak = leak.WorldPosition - CS$<>8__locals1.character.AnimController.AimSourceWorldPos;
			float dist = fromCharacterToLeak.Length();
			float reach = AIObjectiveFixLeak.CalculateReach(this, CS$<>8__locals1.character);
			if (dist > reach * 2f)
			{
				CS$<>8__locals1.<CrewAIOperate>g__Reset|3();
				return true;
			}
			CS$<>8__locals1.character.AIController.SteeringManager.Reset();
			IndoorsSteeringManager pathSteering = CS$<>8__locals1.character.AIController.SteeringManager as IndoorsSteeringManager;
			if (pathSteering != null)
			{
				pathSteering.ResetPath();
			}
			if (!CS$<>8__locals1.character.AnimController.InWater && !CS$<>8__locals1.character.AnimController.InWater)
			{
				HumanoidAnimController humanAnim = CS$<>8__locals1.character.AnimController as HumanoidAnimController;
				if (humanAnim != null && Math.Abs(fromCharacterToLeak.X) < 100f && fromCharacterToLeak.Y < 0f && fromCharacterToLeak.Y > -150f)
				{
					humanAnim.Crouch();
				}
			}
			if (!CS$<>8__locals1.character.IsClimbing)
			{
				if (dist <= reach * 0.8f)
				{
					if (dist > reach * 0.5f)
					{
						if (CS$<>8__locals1.character.AnimController.Limbs.Any((Limb l) => l.InWater))
						{
							goto IL_1A9;
						}
					}
					if (dist < reach * 0.25f && !CS$<>8__locals1.character.IsClimbing)
					{
						CS$<>8__locals1.character.AIController.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(CS$<>8__locals1.character.SimPosition - leak.SimPosition));
						goto IL_22D;
					}
					goto IL_22D;
				}
				IL_1A9:
				Vector2 dir = Vector2.Normalize(fromCharacterToLeak);
				if (!CS$<>8__locals1.character.InWater)
				{
					dir.Y = 0f;
				}
				CS$<>8__locals1.character.AIController.SteeringManager.SteeringManual(deltaTime, dir);
			}
			IL_22D:
			if (dist <= reach || CS$<>8__locals1.character.IsClimbing)
			{
				CS$<>8__locals1.character.CursorPosition = leak.WorldPosition;
				if (CS$<>8__locals1.character.Submarine != null)
				{
					CS$<>8__locals1.character.CursorPosition -= CS$<>8__locals1.character.Submarine.Position;
				}
				CS$<>8__locals1.character.CursorPosition += VectorExtensions.Forward(base.Item.body.TransformedRotation + (float)Math.Sin((double)this.sinTime) / 2f, dist / 2f);
				if (CS$<>8__locals1.character.AnimController.InWater)
				{
					Limb torso = CS$<>8__locals1.character.AnimController.GetLimb(LimbType.Torso, true, false, false);
					Vector2 mousePos = ConvertUnits.ToSimUnits(CS$<>8__locals1.character.CursorPosition);
					Vector2 diff = (mousePos - torso.SimPosition) * CS$<>8__locals1.character.AnimController.Dir;
					float newRotation = MathUtils.VectorToAngle(diff);
					CS$<>8__locals1.character.AnimController.Collider.SmoothRotate(newRotation, 5f, true);
					if (VectorExtensions.Forward(torso.body.TransformedRotation, 1f).Angle(fromCharacterToLeak) < 0.7853982f)
					{
						Vector2 moveDir = leak.IsHorizontal ? Vector2.UnitY : Vector2.UnitX;
						moveDir *= CS$<>8__locals1.character.AnimController.Dir;
						CS$<>8__locals1.character.AIController.SteeringManager.SteeringManual(deltaTime, moveDir);
					}
				}
				if (this.item.RequireAimToUse)
				{
					CS$<>8__locals1.character.SetInput(InputType.Aim, false, true);
					this.sinTime += deltaTime * 5f;
				}
				Vector2 fromItemToLeak = leak.WorldPosition - this.item.WorldPosition;
				float angle = VectorExtensions.Forward(this.item.body.TransformedRotation, 1f).Angle(fromItemToLeak);
				bool repair = true;
				if (angle < 0.7853982f)
				{
					Body body = Submarine.PickBody(this.item.SimPosition, leak.SimPosition, null, new Category?(Category.Cat1), true, null, true);
					Item i = ((body != null) ? body.UserData : null) as Item;
					if (i != null)
					{
						Door door = i.GetComponent<Door>();
						if (door != null && !door.CanBeTraversed)
						{
							if (door.Stuck > 90f)
							{
								return false;
							}
							if (door.Stuck > 50f)
							{
								repair = false;
							}
						}
					}
					if (repair && Submarine.PickBodies(this.item.SimPosition, leak.SimPosition, null, new Category?(Category.Cat2), true, null, false).None(delegate(Body hit)
					{
						Character c = hit.UserData as Character;
						return c != null && c != CS$<>8__locals1.character && HumanAIController.IsFriendly(CS$<>8__locals1.character, c, false, false);
					}))
					{
						CS$<>8__locals1.character.SetInput(InputType.Shoot, false, true);
						this.Use(deltaTime, CS$<>8__locals1.character);
					}
					this.repairTimer += deltaTime;
					if (this.repairTimer > this.repairTimeOut)
					{
						CS$<>8__locals1.<CrewAIOperate>g__Reset|3();
						return true;
					}
				}
			}
			else
			{
				this.repairTimer = 0f;
			}
			bool flag;
			if (leak.Open <= 0f || leak.Removed)
			{
				if (leak.ConnectedWall != null)
				{
					flag = (leak.ConnectedWall.Sections.Max((WallSection s) => s.damage) < 0.1f);
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = false;
			}
			bool leakFixed = flag;
			if (leakFixed)
			{
				Hull flowTargetHull = leak.FlowTargetHull;
				if (((flowTargetHull != null) ? flowTargetHull.DisplayName : null) != null && CS$<>8__locals1.character.IsOnPlayerTeam)
				{
					if (!leak.FlowTargetHull.ConnectedGaps.Any((Gap g) => !g.IsRoomToRoom && g.Open > 0f))
					{
						CS$<>8__locals1.character.Speak(TextManager.GetWithVariable("DialogLeaksFixed", "[roomname]", leak.FlowTargetHull.DisplayName, FormatCapitals.Yes).Value, null, 0f, "leaksfixed".ToIdentifier(), 10f);
					}
					else
					{
						CS$<>8__locals1.character.Speak(TextManager.GetWithVariable("DialogLeakFixed", "[roomname]", leak.FlowTargetHull.DisplayName, FormatCapitals.Yes).Value, null, 0f, "leakfixed".ToIdentifier(), 10f);
					}
				}
			}
			return leakFixed;
		}

		// Token: 0x06005F9B RID: 24475 RVA: 0x0031D20C File Offset: 0x0031B40C
		private void ApplyStatusEffectsOnTarget(Character user, float deltaTime, ActionType actionType, Item targetItem = null, Character character = null, Limb limb = null, Structure structure = null)
		{
			if (this.statusEffectLists == null)
			{
				return;
			}
			List<StatusEffect> statusEffects;
			if (!this.statusEffectLists.TryGetValue(actionType, out statusEffects))
			{
				return;
			}
			foreach (StatusEffect effect in statusEffects)
			{
				RepairTool.currentTargets.Clear();
				effect.SetUser(user);
				if (effect.HasTargetType(StatusEffect.TargetType.UseTarget))
				{
					if (targetItem != null)
					{
						RepairTool.currentTargets.AddRange(targetItem.AllPropertyObjects);
					}
					if (structure != null)
					{
						RepairTool.currentTargets.Add(structure);
					}
					if (character != null)
					{
						RepairTool.currentTargets.Add(character);
					}
					effect.Apply(actionType, deltaTime, this.item, RepairTool.currentTargets, null);
				}
				else if (effect.HasTargetType(StatusEffect.TargetType.Character))
				{
					RepairTool.currentTargets.Add(user);
					effect.Apply(actionType, deltaTime, this.item, RepairTool.currentTargets, null);
				}
				else if (effect.HasTargetType(StatusEffect.TargetType.Limb))
				{
					RepairTool.currentTargets.Add(limb);
					effect.Apply(actionType, deltaTime, this.item, RepairTool.currentTargets, null);
				}
				if (user == null)
				{
					break;
				}
				foreach (ISerializableEntity target in RepairTool.currentTargets)
				{
					Door door = target as Door;
					if (door != null && door.CanBeWelded && door.Item.IsInteractable(user))
					{
						foreach (ValueTuple<Identifier, object> propertyEffect in effect.PropertyEffects)
						{
							SerializableProperty property;
							if (!(propertyEffect.Item1 != "stuck") && door.SerializableProperties != null && door.SerializableProperties.TryGetValue(propertyEffect.Item1, out property))
							{
								object value = property.GetValue(target);
								if (door.Stuck > 0f)
								{
									object item = propertyEffect.Item2;
									bool isCutting = item is float && (float)item < 0f;
									HUDProgressBar progressBar = user.UpdateHUDProgressBar(door, door.Item.WorldPosition, door.Stuck / 100f, Color.DarkGray * 0.5f, Color.White, isCutting ? "progressbar.cutting" : "progressbar.welding");
									if (progressBar != null)
									{
										progressBar.Size = new Vector2(60f, 20f);
									}
									if (!isCutting)
									{
										HintManager.OnWeldingDoor(user, door);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06005F9D RID: 24477 RVA: 0x0031D504 File Offset: 0x0031B704
		[CompilerGenerated]
		internal static bool <Repair>g__CanSeverJoints|113_0(ActionType type, Dictionary<ActionType, List<StatusEffect>> effectList)
		{
			List<StatusEffect> effects;
			if (effectList.TryGetValue(type, out effects))
			{
				return effects.Any((StatusEffect e) => e.SeverLimbsProbability > 0f);
			}
			return false;
		}

		// Token: 0x0400314F RID: 12623
		private readonly List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>();

		// Token: 0x04003150 RID: 12624
		private readonly List<ParticleEmitter> particleEmitterHitStructure = new List<ParticleEmitter>();

		// Token: 0x04003151 RID: 12625
		private readonly List<ParticleEmitter> particleEmitterHitCharacter = new List<ParticleEmitter>();

		// Token: 0x04003152 RID: 12626
		[TupleElementNames(new string[]
		{
			"relatedItem",
			"emitter"
		})]
		private readonly List<ValueTuple<RelatedItem, ParticleEmitter>> particleEmitterHitItem = new List<ValueTuple<RelatedItem, ParticleEmitter>>();

		// Token: 0x04003153 RID: 12627
		private float prevProgressBarState = 1f;

		// Token: 0x04003154 RID: 12628
		private Item prevProgressBarTarget;

		// Token: 0x04003155 RID: 12629
		private readonly HashSet<Identifier> fixableEntities;

		// Token: 0x04003156 RID: 12630
		private readonly HashSet<Identifier> nonFixableEntities;

		// Token: 0x04003157 RID: 12631
		private Vector2 pickedPosition;

		// Token: 0x04003158 RID: 12632
		private float activeTimer;

		// Token: 0x04003159 RID: 12633
		private Vector2 debugRayStartPos;

		// Token: 0x0400315A RID: 12634
		private Vector2 debugRayEndPos;

		// Token: 0x0400315B RID: 12635
		private readonly List<Body> ignoredBodies = new List<Body>();

		// Token: 0x04003172 RID: 12658
		private static readonly List<Body> hitBodies = new List<Body>();

		// Token: 0x04003173 RID: 12659
		private readonly HashSet<Character> hitCharacters = new HashSet<Character>();

		// Token: 0x04003174 RID: 12660
		private readonly List<FireSource> fireSourcesInRange = new List<FireSource>();

		// Token: 0x04003175 RID: 12661
		private float sinTime;

		// Token: 0x04003176 RID: 12662
		private float repairTimer;

		// Token: 0x04003177 RID: 12663
		private Gap previousGap;

		// Token: 0x04003178 RID: 12664
		private readonly float repairTimeOut = 5f;

		// Token: 0x04003179 RID: 12665
		private static List<ISerializableEntity> currentTargets = new List<ISerializableEntity>();

		// Token: 0x0200144B RID: 5195
		public enum UseEnvironment
		{
			// Token: 0x0400653E RID: 25918
			Air,
			// Token: 0x0400653F RID: 25919
			Water,
			// Token: 0x04006540 RID: 25920
			Both,
			// Token: 0x04006541 RID: 25921
			None
		}
	}
}
