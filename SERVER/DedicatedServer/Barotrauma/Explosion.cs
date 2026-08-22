using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x0200022D RID: 557
	internal class Explosion
	{
		// Token: 0x17000AEE RID: 2798
		// (get) Token: 0x06002617 RID: 9751 RVA: 0x000F7714 File Offset: 0x000F5914
		// (set) Token: 0x06002618 RID: 9752 RVA: 0x000F771C File Offset: 0x000F591C
		public float CameraShake { get; set; }

		// Token: 0x17000AEF RID: 2799
		// (get) Token: 0x06002619 RID: 9753 RVA: 0x000F7725 File Offset: 0x000F5925
		// (set) Token: 0x0600261A RID: 9754 RVA: 0x000F772D File Offset: 0x000F592D
		public float CameraShakeRange { get; set; }

		// Token: 0x17000AF0 RID: 2800
		// (get) Token: 0x0600261B RID: 9755 RVA: 0x000F7736 File Offset: 0x000F5936
		// (set) Token: 0x0600261C RID: 9756 RVA: 0x000F773E File Offset: 0x000F593E
		public bool IgnoreCover { get; set; }

		// Token: 0x17000AF1 RID: 2801
		// (get) Token: 0x0600261D RID: 9757 RVA: 0x000F7747 File Offset: 0x000F5947
		// (set) Token: 0x0600261E RID: 9758 RVA: 0x000F774F File Offset: 0x000F594F
		public bool DistanceFalloff { get; set; } = true;

		// Token: 0x17000AF2 RID: 2802
		// (get) Token: 0x0600261F RID: 9759 RVA: 0x000F7758 File Offset: 0x000F5958
		// (set) Token: 0x06002620 RID: 9760 RVA: 0x000F7760 File Offset: 0x000F5960
		public float EmpStrength { get; set; }

		// Token: 0x17000AF3 RID: 2803
		// (get) Token: 0x06002621 RID: 9761 RVA: 0x000F7769 File Offset: 0x000F5969
		// (set) Token: 0x06002622 RID: 9762 RVA: 0x000F7771 File Offset: 0x000F5971
		public float BallastFloraDamage { get; set; }

		// Token: 0x06002623 RID: 9763 RVA: 0x000F777C File Offset: 0x000F597C
		public Explosion(float range, float force, float damage, float structureDamage, float itemDamage, float empStrength = 0f, float ballastFloraStrength = 0f)
		{
			this.Attack = new Attack(damage, 0f, 0f, structureDamage, itemDamage, Math.Min(range, 1000000f))
			{
				SeverLimbsProbability = 1f
			};
			this.force = force;
			this.EmpStrength = empStrength;
			this.BallastFloraDamage = ballastFloraStrength;
			this.sparks = true;
			this.debris = true;
			this.shockwave = true;
			this.smoke = true;
			this.flames = true;
			this.underwaterBubble = true;
			this.ignoreFireEffectsForTags = Array.Empty<Identifier>();
		}

		// Token: 0x06002624 RID: 9764 RVA: 0x000F7828 File Offset: 0x000F5A28
		public Explosion(ContentXElement element, string parentDebugName)
		{
			this.Attack = new Attack(element, parentDebugName + ", Explosion");
			this.force = element.GetAttributeFloat("force", 0f);
			bool showEffects = !element.GetAttributeBool("abilityexplosion", false) && element.GetAttributeBool("showeffects", true);
			this.sparks = element.GetAttributeBool("sparks", showEffects);
			this.shockwave = element.GetAttributeBool("shockwave", showEffects);
			this.flames = element.GetAttributeBool("flames", showEffects);
			this.underwaterBubble = element.GetAttributeBool("underwaterbubble", showEffects);
			this.smoke = element.GetAttributeBool("smoke", showEffects);
			this.debris = element.GetAttributeBool("debris", false);
			this.playTinnitus = element.GetAttributeBool("playtinnitus", showEffects);
			this.applyFireEffects = element.GetAttributeBool("applyfireeffects", this.flames && showEffects);
			this.ignoreFireEffectsForTags = element.GetAttributeIdentifierArray("ignorefireeffectsfortags", Array.Empty<Identifier>(), true);
			this.IgnoreCover = element.GetAttributeBool("ignorecover", false);
			this.OnlyInside = element.GetAttributeBool("onlyinside", false);
			this.OnlyOutside = element.GetAttributeBool("onlyoutside", false);
			this.DistanceFalloff = element.GetAttributeBool("DistanceFalloff", true);
			this.flash = element.GetAttributeBool("flash", showEffects);
			this.flashDuration = element.GetAttributeFloat("flashduration", 0.05f);
			if (element.GetAttribute("flashrange") != null)
			{
				this.flashRange = new float?(element.GetAttributeFloat("flashrange", 100f));
			}
			string key = "flashcolor";
			Color color = Color.LightYellow;
			this.flashColor = element.GetAttributeColor(key, color);
			this.PlayDamageSounds = element.GetAttributeBool("PlayDamageSounds", false);
			this.EmpStrength = element.GetAttributeFloat("empstrength", 0f);
			this.BallastFloraDamage = element.GetAttributeFloat("ballastfloradamage", 0f);
			this.itemRepairStrength = element.GetAttributeFloat("itemrepairstrength", 0f);
			this.decal = element.GetAttributeString("decal", "");
			this.decalSize = element.GetAttributeFloat(1f, new string[]
			{
				"decalSize",
				"decalsize"
			});
			this.CameraShake = element.GetAttributeFloat("camerashake", showEffects ? (this.Attack.Range * 0.1f) : 0f);
			this.CameraShakeRange = element.GetAttributeFloat("camerashakerange", showEffects ? this.Attack.Range : 0f);
			this.screenColorRange = element.GetAttributeFloat("screencolorrange", showEffects ? (this.Attack.Range * 0.1f) : 0f);
			string key2 = "screencolor";
			color = Color.Transparent;
			this.screenColor = element.GetAttributeColor(key2, color);
			this.screenColorDuration = element.GetAttributeFloat("screencolorduration", 0.1f);
		}

		// Token: 0x06002625 RID: 9765 RVA: 0x000F7B40 File Offset: 0x000F5D40
		public void DisableParticles()
		{
			this.sparks = false;
			this.shockwave = false;
			this.smoke = false;
			this.flash = false;
			this.debris = false;
			this.flames = false;
			this.underwaterBubble = false;
		}

		// Token: 0x06002626 RID: 9766 RVA: 0x000F7B74 File Offset: 0x000F5D74
		public void Explode(Vector2 worldPosition, Entity damageSource, Character attacker = null)
		{
			Hull hull = Hull.FindHull(worldPosition, null, true, true);
			if (hull != null && !string.IsNullOrWhiteSpace(this.decal) && this.decalSize > 0f)
			{
				hull.AddDecal(this.decal, worldPosition, this.decalSize, false, null);
			}
			this.Attack.DamageMultiplier = 1f;
			float displayRange = this.Attack.Range;
			Item sourceItem = damageSource as Item;
			if (sourceItem != null)
			{
				Projectile component = sourceItem.GetComponent<Projectile>();
				Item launcher = (component != null) ? component.Launcher : null;
				displayRange *= 1f + sourceItem.GetQualityModifier(Quality.StatType.ExplosionRadius) + ((launcher != null) ? launcher.GetQualityModifier(Quality.StatType.ExplosionRadius) : 0f);
				this.Attack.DamageMultiplier *= 1f + sourceItem.GetQualityModifier(Quality.StatType.ExplosionDamage) + ((launcher != null) ? launcher.GetQualityModifier(Quality.StatType.ExplosionDamage) : 0f);
				Attack attack = this.Attack;
				if (attack.SourceItem == null)
				{
					attack.SourceItem = sourceItem;
				}
			}
			if (attacker != null)
			{
				displayRange *= 1f + attacker.GetStatValue(StatTypes.ExplosionRadiusMultiplier, true);
				this.Attack.DamageMultiplier *= 1f + attacker.GetStatValue(StatTypes.ExplosionDamageMultiplier, true);
			}
			Vector2 cameraPos = GameMain.GameScreen.Cam.Position;
			float cameraDist = Vector2.Distance(cameraPos, worldPosition) / 2f;
			GameMain.GameScreen.Cam.Shake = this.CameraShake * Math.Max((this.CameraShakeRange - cameraDist) / this.CameraShakeRange, 0f);
			if (displayRange < 0.1f)
			{
				return;
			}
			if (!MathUtils.NearlyEqual(this.Attack.GetStructureDamage(1f), 0f, 0.0001f) || !MathUtils.NearlyEqual(this.Attack.GetLevelWallDamage(1f), 0f, 0.0001f))
			{
				Explosion.RangedStructureDamage(worldPosition, displayRange, this.Attack.GetStructureDamage(1f), this.Attack.GetLevelWallDamage(1f), attacker, this.IgnoredSubmarines, this.Attack.EmitStructureDamageParticles, this.Attack.CreateWallDamageProjectiles, this.DistanceFalloff);
			}
			if (this.BallastFloraDamage > 0f)
			{
				Explosion.RangedBallastFloraDamage(worldPosition, displayRange, this.BallastFloraDamage, attacker, this.DistanceFalloff);
			}
			if (this.EmpStrength > 0f)
			{
				float displayRangeSqr = displayRange * displayRange;
				foreach (Item item3 in Item.ItemList)
				{
					float distSqr = Vector2.DistanceSquared(item3.WorldPosition, worldPosition);
					if (distSqr <= displayRangeSqr)
					{
						float distFactor = this.DistanceFalloff ? Explosion.<Explode>g__CalculateDistanceFactor|54_0(distSqr, displayRange) : 1f;
						Powered powered = item3.GetComponent<Powered>();
						if (powered != null && powered.VulnerableToEMP)
						{
							if (item3.Repairables.Any<Repairable>())
							{
								item3.Condition -= item3.MaxCondition * this.EmpStrength * distFactor;
							}
							LightComponent lightComponent = item3.GetComponent<LightComponent>();
							if (lightComponent != null)
							{
								lightComponent.TemporaryFlickerTimer = Math.Min(this.EmpStrength * distFactor * 10f, 10f);
							}
							PowerContainer powerContainer = item3.GetComponent<PowerContainer>();
							if (powerContainer != null)
							{
								powerContainer.Charge -= powerContainer.GetCapacity() * this.EmpStrength * distFactor;
							}
						}
					}
				}
			}
			if (this.itemRepairStrength > 0f)
			{
				float displayRangeSqr2 = displayRange * displayRange;
				foreach (Item item2 in Item.ItemList)
				{
					float distSqr2 = Vector2.DistanceSquared(item2.WorldPosition, worldPosition);
					if (distSqr2 <= displayRangeSqr2)
					{
						float distFactor2 = this.DistanceFalloff ? (1f - (float)Math.Sqrt((double)distSqr2) / displayRange) : 1f;
						if (item2.Repairables.Any<Repairable>())
						{
							item2.Condition += this.itemRepairStrength * distFactor2;
						}
					}
				}
			}
			if (this.Attack.Afflictions.None(null) && MathUtils.NearlyEqual(this.force, 0f, 0.0001f) && MathUtils.NearlyEqual(this.Attack.Stun, 0f, 0.0001f) && MathUtils.NearlyEqual(this.Attack.ItemDamage, 0f, 0.0001f) && MathUtils.NearlyEqual(this.Attack.StructureDamage, 0f, 0.0001f))
			{
				return;
			}
			this.DamageCharacters(worldPosition, this.Attack, this.force, damageSource, attacker, displayRange);
			if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
			{
				using (List<Item>.Enumerator enumerator3 = Item.ItemList.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Item item = enumerator3.Current;
						if (item.Condition > 0f)
						{
							float dist = Vector2.Distance(item.WorldPosition, worldPosition);
							float itemRadius = (item.body == null) ? 0f : item.body.GetMaxExtent();
							dist = Math.Max(0f, dist - ConvertUnits.ToDisplayUnits(itemRadius));
							if (dist <= displayRange)
							{
								if (dist < displayRange * 0.5f && this.applyFireEffects && !item.FireProof && this.ignoreFireEffectsForTags.None((Identifier t) => item.HasTag(t)))
								{
									Item container = item.Container;
									bool fireProof = false;
									while (container != null)
									{
										if (container.FireProof)
										{
											fireProof = true;
											break;
										}
										container = container.Container;
									}
									if (!fireProof)
									{
										item.ApplyStatusEffects(ActionType.OnFire, 1f, null, null, null, false, null);
										if (item.Condition <= 0f)
										{
											NetworkMember networkMember = GameMain.NetworkMember;
											if (networkMember != null && networkMember.IsServer)
											{
												GameMain.NetworkMember.CreateEntityEvent(item, new Item.ApplyStatusEffectEventData(ActionType.OnFire, null, null, null, null, null));
											}
										}
									}
								}
								if (!item.Indestructible && (item.Prefab.DamagedByExplosions || (item.Prefab.DamagedByContainedItemExplosions && item.ContainedItems.Contains(damageSource))))
								{
									float distFactor3 = this.DistanceFalloff ? (1f - dist / displayRange) : 1f;
									float damageAmount = this.Attack.GetItemDamage(1f, item.Prefab.ExplosionDamageMultiplier);
									Vector2 explosionPos = worldPosition;
									if (item.Submarine != null)
									{
										explosionPos -= item.Submarine.Position;
									}
									damageAmount *= Explosion.GetObstacleDamageMultiplier(ConvertUnits.ToSimUnits(explosionPos), worldPosition, item.SimPosition, this.IgnoredCover);
									item.Condition -= damageAmount * distFactor3;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06002627 RID: 9767 RVA: 0x000F8300 File Offset: 0x000F6500
		private void DamageCharacters(Vector2 worldPosition, Attack attack, float force, Entity damageSource, Character attacker, float range)
		{
			if (range <= 0f)
			{
				return;
			}
			float broadRange = Math.Max(range * 10f, 10000f);
			foreach (Character c in Character.CharacterList)
			{
				if ((!attack.OnlyHumans || c.IsHuman) && !this.IgnoredCharacters.Contains(c) && c.Enabled && Math.Abs(c.WorldPosition.X - worldPosition.X) <= broadRange && Math.Abs(c.WorldPosition.Y - worldPosition.Y) <= broadRange && (!this.OnlyInside || c.Submarine != null) && (!this.OnlyOutside || c.Submarine == null))
				{
					Vector2 explosionPos = worldPosition;
					if (c.Submarine != null)
					{
						explosionPos -= c.Submarine.Position;
					}
					Hull hull = Hull.FindHull(explosionPos, null, false, true);
					bool underWater = hull == null || explosionPos.Y < hull.Surface;
					explosionPos = ConvertUnits.ToSimUnits(explosionPos);
					Dictionary<Limb, float> distFactors = new Dictionary<Limb, float>();
					Dictionary<Limb, float> damages = new Dictionary<Limb, float>();
					List<Affliction> modifiedAfflictions = new List<Affliction>();
					Limb closestLimb = null;
					float closestDistFactor = 0f;
					foreach (Limb limb in c.AnimController.Limbs)
					{
						if (!limb.IsSevered && !limb.IgnoreCollisions && limb.body.Enabled)
						{
							float dist = Vector2.Distance(limb.WorldPosition, worldPosition);
							float limbRadius = limb.body.GetMaxExtent();
							dist = Math.Max(0f, dist - ConvertUnits.ToDisplayUnits(limbRadius));
							if (dist <= range)
							{
								float distFactor = this.DistanceFalloff ? (1f - dist / attack.Range) : 1f;
								if (!this.IgnoreCover)
								{
									distFactor *= Explosion.GetObstacleDamageMultiplier(explosionPos, worldPosition, limb.SimPosition, this.IgnoredCover);
								}
								if (distFactor > 0f)
								{
									distFactors.Add(limb, distFactor);
									if (distFactor > closestDistFactor)
									{
										closestLimb = limb;
										closestDistFactor = distFactor;
									}
								}
							}
						}
					}
					foreach (Limb limb2 in distFactors.Keys)
					{
						float distFactor2;
						if (distFactors.TryGetValue(limb2, out distFactor2))
						{
							modifiedAfflictions.Clear();
							foreach (Affliction affliction in attack.Afflictions.Keys)
							{
								float dmgMultiplier = distFactor2;
								if (affliction.DivideByLimbCount)
								{
									float limbCountFactor = (float)distFactors.Count;
									if (affliction.Prefab.LimbSpecific && affliction.Prefab.AfflictionType == AfflictionPrefab.DamageType)
									{
										limbCountFactor = (float)Math.Min(distFactors.Count, 15);
									}
									dmgMultiplier /= limbCountFactor;
								}
								modifiedAfflictions.Add(affliction.CreateMultiplied(dmgMultiplier, affliction));
							}
							c.LastDamageSource = damageSource;
							if (attacker == null)
							{
								Item item = damageSource as Item;
								if (item != null)
								{
									Projectile component = item.GetComponent<Projectile>();
									attacker = ((component != null) ? component.User : null);
									if (attacker == null)
									{
										MeleeWeapon component2 = item.GetComponent<MeleeWeapon>();
										attacker = ((component2 != null) ? component2.User : null);
									}
								}
							}
							if ((attack.Afflictions.Any<KeyValuePair<Affliction, XElement>>() || attack.Stun > 0f) && (!attack.OnlyHumans || c.IsHuman))
							{
								AbilityAttackData attackData = new AbilityAttackData(this.Attack, c, attacker);
								if (attackData.Afflictions != null)
								{
									modifiedAfflictions.AddRange(attackData.Afflictions);
								}
								Vector2 dir = worldPosition - limb2.WorldPosition;
								Vector2 hitPos = limb2.WorldPosition + ((dir.LengthSquared() <= 0.001f) ? Rand.Vector(1f, Rand.RandSync.Unsynced) : Vector2.Normalize(dir)) * 0.01f;
								bool playSound = this.PlayDamageSounds && limb2 == closestLimb;
								Character character = c;
								Vector2 worldPosition2 = hitPos;
								IEnumerable<Affliction> afflictions = modifiedAfflictions;
								float stun = attack.Stun * distFactor2;
								bool playSound2 = playSound;
								Character attacker2 = attacker;
								float damageMultiplier = attack.DamageMultiplier * attackData.DamageMultiplier;
								damages.Add(limb2, character.AddDamage(worldPosition2, afflictions, stun, playSound2, null, attacker2, damageMultiplier).Damage);
							}
							if (attack.StatusEffects != null && attack.StatusEffects.Any<StatusEffect>())
							{
								attack.SetUser(attacker);
								List<ISerializableEntity> statusEffectTargets = new List<ISerializableEntity>();
								foreach (StatusEffect statusEffect in attack.StatusEffects)
								{
									statusEffectTargets.Clear();
									if (statusEffect.HasTargetType(StatusEffect.TargetType.Character))
									{
										statusEffectTargets.Add(c);
									}
									if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
									{
										statusEffectTargets.Add(limb2);
									}
									statusEffect.Apply(ActionType.OnUse, 1f, damageSource, statusEffectTargets, null);
									statusEffect.Apply(ActionType.Always, 1f, damageSource, statusEffectTargets, null);
									statusEffect.Apply(underWater ? ActionType.InWater : ActionType.NotInWater, 1f, damageSource, statusEffectTargets, null);
								}
							}
							if (limb2.WorldPosition != worldPosition && !MathUtils.NearlyEqual(force, 0f, 0.0001f))
							{
								Vector2 limbDiff = Vector2.Normalize(limb2.WorldPosition - worldPosition);
								if (!MathUtils.IsValid(limbDiff))
								{
									limbDiff = Rand.Vector(1f, Rand.RandSync.Unsynced);
								}
								Vector2 impulse = limbDiff * distFactor2 * force;
								Vector2 impulsePoint = limb2.SimPosition - limbDiff * limb2.body.GetMaxExtent();
								limb2.body.ApplyLinearImpulse(impulse, impulsePoint, 12.8f);
							}
						}
					}
					if (c == Character.Controlled && !c.IsDead && this.playTinnitus)
					{
						Limb head = c.AnimController.GetLimb(LimbType.Head, true, false, false);
						float headDamage;
						if (head != null && damages.TryGetValue(head, out headDamage) && headDamage > 0f)
						{
							float headFactor;
							distFactors.TryGetValue(head, out headFactor);
						}
					}
					if (attack.SeverLimbsProbability > 0f)
					{
						foreach (Limb limb3 in c.AnimController.Limbs)
						{
							float distFactor3;
							float damage;
							if (!limb3.character.Removed && !limb3.Removed && !limb3.IsSevered && (c.IsDead || limb3.CanBeSeveredAlive) && distFactors.TryGetValue(limb3, out distFactor3) && damages.TryGetValue(limb3, out damage))
							{
								c.TrySeverLimbJoints(limb3, attack.SeverLimbsProbability * distFactor3, damage, true, false, attacker);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002628 RID: 9768 RVA: 0x000F8A40 File Offset: 0x000F6C40
		public static Dictionary<Structure, float> RangedStructureDamage(Vector2 worldPosition, float worldRange, float damage, float levelWallDamage, Character attacker = null, IEnumerable<Submarine> ignoredSubmarines = null, bool emitWallDamageParticles = true, bool createWallDamageProjectiles = false, bool distanceFalloff = true)
		{
			float dist = 600f;
			Explosion.damagedStructures.Clear();
			foreach (Structure structure in Structure.WallList)
			{
				if ((ignoredSubmarines == null || structure.Submarine == null || !ignoredSubmarines.Contains(structure.Submarine)) && structure.HasBody && !structure.IsPlatform && Vector2.Distance(structure.WorldPosition, worldPosition) < dist * 3f)
				{
					for (int i = 0; i < structure.SectionCount; i++)
					{
						float distFactor = distanceFalloff ? (1f - Vector2.Distance(structure.SectionPosition(i, true), worldPosition) / worldRange) : 1f;
						if (distFactor > 0f)
						{
							structure.AddDamage(i, damage * distFactor, attacker, emitWallDamageParticles, createWallDamageProjectiles);
							if (Explosion.damagedStructures.ContainsKey(structure))
							{
								Dictionary<Structure, float> dictionary = Explosion.damagedStructures;
								Structure key = structure;
								dictionary[key] += damage * distFactor;
							}
							else
							{
								Explosion.damagedStructures.Add(structure, damage * distFactor);
							}
						}
					}
				}
			}
			if (Level.Loaded != null && !MathUtils.NearlyEqual(levelWallDamage, 0f, 0.0001f))
			{
				Level loaded = Level.Loaded;
				if (((loaded != null) ? loaded.LevelObjectManager : null) != null)
				{
					foreach (LevelObject levelObject in Level.Loaded.LevelObjectManager.GetAllObjects(worldPosition, worldRange))
					{
						if (levelObject.Prefab.TakeLevelWallDamage)
						{
							float distFactor2 = 1f - Vector2.Distance(levelObject.WorldPosition, worldPosition) / worldRange;
							if (distFactor2 > 0f)
							{
								levelObject.AddDamage(levelWallDamage * distFactor2, 1f, null, false);
							}
						}
					}
				}
				for (int j = Level.Loaded.ExtraWalls.Count - 1; j >= 0; j--)
				{
					DestructibleLevelWall destructibleWall = Level.Loaded.ExtraWalls[j] as DestructibleLevelWall;
					if (destructibleWall != null)
					{
						bool inRange = false;
						foreach (VoronoiCell cell in destructibleWall.Cells)
						{
							if (cell.IsPointInside(worldPosition))
							{
								inRange = true;
								break;
							}
							foreach (GraphEdge edge in cell.Edges)
							{
								if (MathUtils.LineSegmentToPointDistanceSquared((edge.Point1 + cell.Translation).ToPoint(), (edge.Point2 + cell.Translation).ToPoint(), worldPosition.ToPoint()) < (double)(worldRange * worldRange))
								{
									inRange = true;
									break;
								}
							}
							if (inRange)
							{
								break;
							}
						}
						if (inRange)
						{
							destructibleWall.AddDamage(levelWallDamage, worldPosition);
						}
					}
				}
			}
			return Explosion.damagedStructures;
		}

		// Token: 0x06002629 RID: 9769 RVA: 0x000F8D7C File Offset: 0x000F6F7C
		public static void RangedBallastFloraDamage(Vector2 worldPosition, float worldRange, float damage, Character attacker = null, bool distanceFalloff = true)
		{
			List<BallastFloraBehavior> ballastFlorae = new List<BallastFloraBehavior>();
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.BallastFlora != null)
				{
					ballastFlorae.Add(hull.BallastFlora);
				}
			}
			using (List<BallastFloraBehavior>.Enumerator enumerator2 = ballastFlorae.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					BallastFloraBehavior ballastFlora = enumerator2.Current;
					float resistanceMuliplier = ballastFlora.HasBrokenThrough ? 1f : (1f - ballastFlora.ExplosionResistance);
					ballastFlora.Branches.ForEachMod(delegate(BallastFloraBranch branch)
					{
						Vector2 branchWorldPos = ballastFlora.GetWorldPosition() + branch.Position;
						float branchDist = Vector2.Distance(branchWorldPos, worldPosition);
						if (branchDist < worldRange)
						{
							float distFactor = distanceFalloff ? (1f - branchDist / worldRange) : 1f;
							if (distFactor <= 0f)
							{
								return;
							}
							Vector2 explosionPos = worldPosition;
							Vector2 branchPos = branchWorldPos;
							Hull parent = ballastFlora.Parent;
							if (((parent != null) ? parent.Submarine : null) != null)
							{
								explosionPos -= ballastFlora.Parent.Submarine.Position;
								branchPos -= ballastFlora.Parent.Submarine.Position;
							}
							distFactor *= Explosion.GetObstacleDamageMultiplier(ConvertUnits.ToSimUnits(explosionPos), worldPosition, ConvertUnits.ToSimUnits(branchPos), null);
							ballastFlora.DamageBranch(branch, damage * distFactor * resistanceMuliplier, BallastFloraBehavior.AttackType.Explosives, attacker);
						}
					});
				}
			}
		}

		// Token: 0x0600262A RID: 9770 RVA: 0x000F8EA4 File Offset: 0x000F70A4
		private static float GetObstacleDamageMultiplier(Vector2 explosionSimPos, Vector2 explosionWorldPos, Vector2 targetSimPos, IEnumerable<Structure> ignoredCover = null)
		{
			float damageMultiplier = 1f;
			IEnumerable<Body> obstacles = Submarine.PickBodies(targetSimPos, explosionSimPos, null, new Category?(Category.Cat1 | Category.Cat5 | Category.Cat6), true, null, false);
			foreach (Body body in obstacles)
			{
				Item item = body.UserData as Item;
				if (item != null)
				{
					Door door = item.GetComponent<Door>();
					if (door != null && !door.IsOpen && !door.IsBroken)
					{
						damageMultiplier *= 0.01f;
					}
				}
				else
				{
					Structure structure = body.UserData as Structure;
					if (structure != null)
					{
						if (ignoredCover == null || !ignoredCover.Contains(structure))
						{
							int sectionIndex = structure.FindSectionIndex(explosionWorldPos, true, true);
							if (!structure.SectionBodyDisabled(sectionIndex))
							{
								if (structure.SectionIsLeaking(sectionIndex))
								{
									damageMultiplier *= 0.1f;
								}
								else
								{
									damageMultiplier *= 0.01f;
								}
							}
						}
					}
					else
					{
						damageMultiplier *= 0.1f;
					}
				}
			}
			return damageMultiplier;
		}

		// Token: 0x0600262C RID: 9772 RVA: 0x000F8FA8 File Offset: 0x000F71A8
		[CompilerGenerated]
		internal static float <Explode>g__CalculateDistanceFactor|54_0(float distSqr, float displayRange)
		{
			return 1f - MathF.Sqrt(distSqr) / displayRange;
		}

		// Token: 0x0400128A RID: 4746
		public readonly Attack Attack;

		// Token: 0x0400128B RID: 4747
		private readonly float force;

		// Token: 0x0400128E RID: 4750
		private readonly Color screenColor;

		// Token: 0x0400128F RID: 4751
		private readonly float screenColorRange;

		// Token: 0x04001290 RID: 4752
		private readonly float screenColorDuration;

		// Token: 0x04001291 RID: 4753
		private bool sparks;

		// Token: 0x04001292 RID: 4754
		private bool shockwave;

		// Token: 0x04001293 RID: 4755
		private bool flames;

		// Token: 0x04001294 RID: 4756
		private bool smoke;

		// Token: 0x04001295 RID: 4757
		private bool flash;

		// Token: 0x04001296 RID: 4758
		private bool debris;

		// Token: 0x04001297 RID: 4759
		private bool underwaterBubble;

		// Token: 0x04001298 RID: 4760
		private readonly Color flashColor;

		// Token: 0x04001299 RID: 4761
		private readonly bool playTinnitus;

		// Token: 0x0400129A RID: 4762
		private readonly bool applyFireEffects;

		// Token: 0x0400129B RID: 4763
		private readonly Identifier[] ignoreFireEffectsForTags;

		// Token: 0x0400129E RID: 4766
		public IEnumerable<Structure> IgnoredCover;

		// Token: 0x0400129F RID: 4767
		private readonly float flashDuration;

		// Token: 0x040012A0 RID: 4768
		private readonly float? flashRange;

		// Token: 0x040012A1 RID: 4769
		private readonly string decal;

		// Token: 0x040012A2 RID: 4770
		private readonly float decalSize;

		// Token: 0x040012A3 RID: 4771
		public bool OnlyInside;

		// Token: 0x040012A4 RID: 4772
		public bool OnlyOutside;

		// Token: 0x040012A5 RID: 4773
		public bool PlayDamageSounds;

		// Token: 0x040012A6 RID: 4774
		private readonly float itemRepairStrength;

		// Token: 0x040012A7 RID: 4775
		public readonly HashSet<Submarine> IgnoredSubmarines = new HashSet<Submarine>();

		// Token: 0x040012A8 RID: 4776
		public readonly HashSet<Character> IgnoredCharacters = new HashSet<Character>();

		// Token: 0x040012AB RID: 4779
		private static readonly Dictionary<Structure, float> damagedStructures = new Dictionary<Structure, float>();
	}
}
