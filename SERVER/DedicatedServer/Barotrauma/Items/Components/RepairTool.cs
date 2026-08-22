using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.MapCreatures.Behavior;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C0 RID: 1216
	internal class RepairTool : ItemComponent
	{
		// Token: 0x1700127D RID: 4733
		// (get) Token: 0x06004530 RID: 17712 RVA: 0x001BA88E File Offset: 0x001B8A8E
		// (set) Token: 0x06004531 RID: 17713 RVA: 0x001BA896 File Offset: 0x001B8A96
		[Serialize("Both", IsPropertySaveable.No, "Can the item be used in air, water or both.", "", false)]
		public RepairTool.UseEnvironment UsableIn { get; set; }

		// Token: 0x1700127E RID: 4734
		// (get) Token: 0x06004532 RID: 17714 RVA: 0x001BA89F File Offset: 0x001B8A9F
		// (set) Token: 0x06004533 RID: 17715 RVA: 0x001BA8A7 File Offset: 0x001B8AA7
		[Serialize(0f, IsPropertySaveable.No, "The distance at which the item can repair targets.", "", false)]
		public float Range { get; set; }

		// Token: 0x1700127F RID: 4735
		// (get) Token: 0x06004534 RID: 17716 RVA: 0x001BA8B0 File Offset: 0x001B8AB0
		// (set) Token: 0x06004535 RID: 17717 RVA: 0x001BA8B8 File Offset: 0x001B8AB8
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle when used by a character with sufficient skills to use the tool (in degrees).", "", false)]
		public float Spread { get; set; }

		// Token: 0x17001280 RID: 4736
		// (get) Token: 0x06004536 RID: 17718 RVA: 0x001BA8C1 File Offset: 0x001B8AC1
		// (set) Token: 0x06004537 RID: 17719 RVA: 0x001BA8C9 File Offset: 0x001B8AC9
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle when used by a character with insufficient skills to use the tool (in degrees).", "", false)]
		public float UnskilledSpread { get; set; }

		// Token: 0x17001281 RID: 4737
		// (get) Token: 0x06004538 RID: 17720 RVA: 0x001BA8D2 File Offset: 0x001B8AD2
		// (set) Token: 0x06004539 RID: 17721 RVA: 0x001BA8DA File Offset: 0x001B8ADA
		[Serialize(0f, IsPropertySaveable.No, "How many units of damage the item removes from structures per second.", "", false)]
		public float StructureFixAmount { get; set; }

		// Token: 0x17001282 RID: 4738
		// (get) Token: 0x0600453A RID: 17722 RVA: 0x001BA8E3 File Offset: 0x001B8AE3
		// (set) Token: 0x0600453B RID: 17723 RVA: 0x001BA8EB File Offset: 0x001B8AEB
		[Serialize(0f, IsPropertySaveable.No, "How much damage is applied to ballast flora.", "", false)]
		public float FireDamage { get; set; }

		// Token: 0x17001283 RID: 4739
		// (get) Token: 0x0600453C RID: 17724 RVA: 0x001BA8F4 File Offset: 0x001B8AF4
		// (set) Token: 0x0600453D RID: 17725 RVA: 0x001BA8FC File Offset: 0x001B8AFC
		[Serialize(0f, IsPropertySaveable.No, "How many units of damage the item removes from destructible level walls per second.", "", false)]
		public float LevelWallFixAmount { get; set; }

		// Token: 0x17001284 RID: 4740
		// (get) Token: 0x0600453E RID: 17726 RVA: 0x001BA905 File Offset: 0x001B8B05
		// (set) Token: 0x0600453F RID: 17727 RVA: 0x001BA90D File Offset: 0x001B8B0D
		[Serialize(0f, IsPropertySaveable.No, "How much the item decreases the size of fires per second.", "", false)]
		public float ExtinguishAmount { get; set; }

		// Token: 0x17001285 RID: 4741
		// (get) Token: 0x06004540 RID: 17728 RVA: 0x001BA916 File Offset: 0x001B8B16
		// (set) Token: 0x06004541 RID: 17729 RVA: 0x001BA91E File Offset: 0x001B8B1E
		[Serialize(0f, IsPropertySaveable.No, "How much water the item provides to planters per second.", "", false)]
		public float WaterAmount { get; set; }

		// Token: 0x17001286 RID: 4742
		// (get) Token: 0x06004542 RID: 17730 RVA: 0x001BA927 File Offset: 0x001B8B27
		// (set) Token: 0x06004543 RID: 17731 RVA: 0x001BA92F File Offset: 0x001B8B2F
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The position of the barrel as an offset from the item's center (in pixels).", "", false)]
		public Vector2 BarrelPos { get; set; }

		// Token: 0x17001287 RID: 4743
		// (get) Token: 0x06004544 RID: 17732 RVA: 0x001BA938 File Offset: 0x001B8B38
		// (set) Token: 0x06004545 RID: 17733 RVA: 0x001BA940 File Offset: 0x001B8B40
		[Serialize(false, IsPropertySaveable.No, "Can the item repair things through walls.", "", false)]
		public bool RepairThroughWalls { get; set; }

		// Token: 0x17001288 RID: 4744
		// (get) Token: 0x06004546 RID: 17734 RVA: 0x001BA949 File Offset: 0x001B8B49
		// (set) Token: 0x06004547 RID: 17735 RVA: 0x001BA951 File Offset: 0x001B8B51
		[Serialize(false, IsPropertySaveable.No, "Can the item repair multiple things at once, or will it only affect the first thing the ray from the barrel hits.", "", false)]
		public bool RepairMultiple { get; set; }

		// Token: 0x17001289 RID: 4745
		// (get) Token: 0x06004548 RID: 17736 RVA: 0x001BA95A File Offset: 0x001B8B5A
		// (set) Token: 0x06004549 RID: 17737 RVA: 0x001BA962 File Offset: 0x001B8B62
		[Serialize(true, IsPropertySaveable.No, "Can the item repair multiple walls at once? Only relevant if RepairMultiple is true.", "", false)]
		public bool RepairMultipleWalls { get; set; }

		// Token: 0x1700128A RID: 4746
		// (get) Token: 0x0600454A RID: 17738 RVA: 0x001BA96B File Offset: 0x001B8B6B
		// (set) Token: 0x0600454B RID: 17739 RVA: 0x001BA973 File Offset: 0x001B8B73
		[Serialize(false, IsPropertySaveable.No, "Can the item repair things through holes in walls.", "", false)]
		public bool RepairThroughHoles { get; set; }

		// Token: 0x1700128B RID: 4747
		// (get) Token: 0x0600454C RID: 17740 RVA: 0x001BA97C File Offset: 0x001B8B7C
		// (set) Token: 0x0600454D RID: 17741 RVA: 0x001BA984 File Offset: 0x001B8B84
		[Serialize(100f, IsPropertySaveable.No, "How far two walls need to not be considered overlapping and to stop the ray.", "", false)]
		public float MaxOverlappingWallDist { get; set; }

		// Token: 0x1700128C RID: 4748
		// (get) Token: 0x0600454E RID: 17742 RVA: 0x001BA98D File Offset: 0x001B8B8D
		// (set) Token: 0x0600454F RID: 17743 RVA: 0x001BA995 File Offset: 0x001B8B95
		[Serialize(1f, IsPropertySaveable.No, "How fast the tool detaches level resources (e.g. minerals). Acts as a multiplier on the speed: with a value of 2, detaching an item whose DeattachDuration is set to 30 seconds would take 15 seconds.", "", false)]
		public float DeattachSpeed { get; set; }

		// Token: 0x1700128D RID: 4749
		// (get) Token: 0x06004550 RID: 17744 RVA: 0x001BA99E File Offset: 0x001B8B9E
		// (set) Token: 0x06004551 RID: 17745 RVA: 0x001BA9A6 File Offset: 0x001B8BA6
		[Serialize(true, IsPropertySaveable.No, "Can the item hit doors.", "", false)]
		public bool HitItems { get; set; }

		// Token: 0x1700128E RID: 4750
		// (get) Token: 0x06004552 RID: 17746 RVA: 0x001BA9AF File Offset: 0x001B8BAF
		// (set) Token: 0x06004553 RID: 17747 RVA: 0x001BA9B7 File Offset: 0x001B8BB7
		[Serialize(false, IsPropertySaveable.No, "Can the item hit broken doors.", "", false)]
		public bool HitBrokenDoors { get; set; }

		// Token: 0x1700128F RID: 4751
		// (get) Token: 0x06004554 RID: 17748 RVA: 0x001BA9C0 File Offset: 0x001B8BC0
		// (set) Token: 0x06004555 RID: 17749 RVA: 0x001BA9C8 File Offset: 0x001B8BC8
		[Serialize(false, IsPropertySaveable.No, "Should the tool ignore characters? Enabled e.g. for fire extinguisher.", "", false)]
		public bool IgnoreCharacters { get; set; }

		// Token: 0x17001290 RID: 4752
		// (get) Token: 0x06004556 RID: 17750 RVA: 0x001BA9D1 File Offset: 0x001B8BD1
		// (set) Token: 0x06004557 RID: 17751 RVA: 0x001BA9D9 File Offset: 0x001B8BD9
		[Serialize(0f, IsPropertySaveable.No, "The probability of starting a fire somewhere along the ray fired from the barrel (for example, 0.1 = 10% chance to start a fire during a second of use).", "", false)]
		public float FireProbability { get; set; }

		// Token: 0x17001291 RID: 4753
		// (get) Token: 0x06004558 RID: 17752 RVA: 0x001BA9E2 File Offset: 0x001B8BE2
		// (set) Token: 0x06004559 RID: 17753 RVA: 0x001BA9EA File Offset: 0x001B8BEA
		[Serialize(0f, IsPropertySaveable.No, "Force applied to the entity the ray hits.", "", false)]
		public float TargetForce { get; set; }

		// Token: 0x17001292 RID: 4754
		// (get) Token: 0x0600455A RID: 17754 RVA: 0x001BA9F3 File Offset: 0x001B8BF3
		// (set) Token: 0x0600455B RID: 17755 RVA: 0x001BA9FB File Offset: 0x001B8BFB
		[Serialize(0f, IsPropertySaveable.No, "Rotation of the barrel in degrees.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 360f, VectorComponentLabels = new string[]
		{
			"editable.minvalue",
			"editable.maxvalue"
		})]
		public float BarrelRotation { get; set; }

		// Token: 0x17001293 RID: 4755
		// (get) Token: 0x0600455C RID: 17756 RVA: 0x001BAA04 File Offset: 0x001B8C04
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

		// Token: 0x0600455D RID: 17757 RVA: 0x001BAA7C File Offset: 0x001B8C7C
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
		}

		// Token: 0x0600455E RID: 17758 RVA: 0x001BAC78 File Offset: 0x001B8E78
		public override void Update(float deltaTime, Camera cam)
		{
			this.activeTimer -= deltaTime;
			if (this.activeTimer <= 0f)
			{
				this.IsActive = false;
			}
		}

		// Token: 0x0600455F RID: 17759 RVA: 0x001BAC9C File Offset: 0x001B8E9C
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
			return true;
		}

		// Token: 0x06004560 RID: 17760 RVA: 0x001BB20C File Offset: 0x001B940C
		private void Repair(Vector2 rayStart, Vector2 rayEnd, float deltaTime, Character user, float degreeOfSuccess, List<Body> ignoredBodies)
		{
			Category collisionCategories = Category.Cat1 | Category.Cat5 | Category.Cat6 | Category.Cat8 | Category.Cat9;
			if (!this.IgnoreCharacters)
			{
				collisionCategories |= Category.Cat2;
			}
			if (this.statusEffectLists != null && (RepairTool.<Repair>g__CanSeverJoints|106_0(ActionType.OnUse, this.statusEffectLists) || RepairTool.<Repair>g__CanSeverJoints|106_0(ActionType.OnSuccess, this.statusEffectLists)))
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
					if (!(fs3 is DummyFireSource))
					{
						GameMain.Server.KarmaManager.OnExtinguishingFire(user, deltaTime);
					}
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

		// Token: 0x06004561 RID: 17761 RVA: 0x001BBA34 File Offset: 0x001B9C34
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
						return true;
					}
				}
			}
		}

		// Token: 0x06004562 RID: 17762 RVA: 0x001BC0E0 File Offset: 0x001BA2E0
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			RepairTool.<>c__DisplayClass115_0 CS$<>8__locals1 = new RepairTool.<>c__DisplayClass115_0();
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

		// Token: 0x06004563 RID: 17763 RVA: 0x001BC76C File Offset: 0x001BA96C
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
			}
		}

		// Token: 0x06004565 RID: 17765 RVA: 0x001BC8C0 File Offset: 0x001BAAC0
		[CompilerGenerated]
		internal static bool <Repair>g__CanSeverJoints|106_0(ActionType type, Dictionary<ActionType, List<StatusEffect>> effectList)
		{
			List<StatusEffect> effects;
			if (effectList.TryGetValue(type, out effects))
			{
				return effects.Any((StatusEffect e) => e.SeverLimbsProbability > 0f);
			}
			return false;
		}

		// Token: 0x04002135 RID: 8501
		private readonly HashSet<Identifier> fixableEntities;

		// Token: 0x04002136 RID: 8502
		private readonly HashSet<Identifier> nonFixableEntities;

		// Token: 0x04002137 RID: 8503
		private Vector2 pickedPosition;

		// Token: 0x04002138 RID: 8504
		private float activeTimer;

		// Token: 0x04002139 RID: 8505
		private Vector2 debugRayStartPos;

		// Token: 0x0400213A RID: 8506
		private Vector2 debugRayEndPos;

		// Token: 0x0400213B RID: 8507
		private readonly List<Body> ignoredBodies = new List<Body>();

		// Token: 0x04002152 RID: 8530
		private static readonly List<Body> hitBodies = new List<Body>();

		// Token: 0x04002153 RID: 8531
		private readonly HashSet<Character> hitCharacters = new HashSet<Character>();

		// Token: 0x04002154 RID: 8532
		private readonly List<FireSource> fireSourcesInRange = new List<FireSource>();

		// Token: 0x04002155 RID: 8533
		private float sinTime;

		// Token: 0x04002156 RID: 8534
		private float repairTimer;

		// Token: 0x04002157 RID: 8535
		private Gap previousGap;

		// Token: 0x04002158 RID: 8536
		private readonly float repairTimeOut = 5f;

		// Token: 0x04002159 RID: 8537
		private static List<ISerializableEntity> currentTargets = new List<ISerializableEntity>();

		// Token: 0x02000E09 RID: 3593
		public enum UseEnvironment
		{
			// Token: 0x0400418E RID: 16782
			Air,
			// Token: 0x0400418F RID: 16783
			Water,
			// Token: 0x04004190 RID: 16784
			Both,
			// Token: 0x04004191 RID: 16785
			None
		}
	}
}
