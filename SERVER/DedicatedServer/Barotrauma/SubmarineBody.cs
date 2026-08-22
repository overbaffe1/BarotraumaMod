using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x0200025D RID: 605
	internal class SubmarineBody
	{
		// Token: 0x17000D00 RID: 3328
		// (get) Token: 0x06002B80 RID: 11136 RVA: 0x0011CCB5 File Offset: 0x0011AEB5
		// (set) Token: 0x06002B81 RID: 11137 RVA: 0x0011CCBD File Offset: 0x0011AEBD
		public List<Vector2> HullVertices { get; private set; }

		// Token: 0x17000D01 RID: 3329
		// (get) Token: 0x06002B82 RID: 11138 RVA: 0x0011CCC6 File Offset: 0x0011AEC6
		// (set) Token: 0x06002B83 RID: 11139 RVA: 0x0011CCCE File Offset: 0x0011AECE
		public Rectangle Borders { get; private set; }

		// Token: 0x17000D02 RID: 3330
		// (get) Token: 0x06002B84 RID: 11140 RVA: 0x0011CCD7 File Offset: 0x0011AED7
		// (set) Token: 0x06002B85 RID: 11141 RVA: 0x0011CCDF File Offset: 0x0011AEDF
		public Rectangle VisibleBorders { get; private set; }

		// Token: 0x17000D03 RID: 3331
		// (get) Token: 0x06002B86 RID: 11142 RVA: 0x0011CCE8 File Offset: 0x0011AEE8
		// (set) Token: 0x06002B87 RID: 11143 RVA: 0x0011CCF5 File Offset: 0x0011AEF5
		public Vector2 Velocity
		{
			get
			{
				return this.Body.LinearVelocity;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.Body.LinearVelocity = value;
			}
		}

		// Token: 0x17000D04 RID: 3332
		// (get) Token: 0x06002B88 RID: 11144 RVA: 0x0011CD0C File Offset: 0x0011AF0C
		public Vector2 Position
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.Body.SimPosition);
			}
		}

		// Token: 0x17000D05 RID: 3333
		// (get) Token: 0x06002B89 RID: 11145 RVA: 0x0011CD1E File Offset: 0x0011AF1E
		public List<PosInfo> PositionBuffer
		{
			get
			{
				return this.positionBuffer;
			}
		}

		// Token: 0x17000D06 RID: 3334
		// (get) Token: 0x06002B8A RID: 11146 RVA: 0x0011CD26 File Offset: 0x0011AF26
		public Submarine Submarine
		{
			get
			{
				return this.submarine;
			}
		}

		// Token: 0x06002B8B RID: 11147 RVA: 0x0011CD30 File Offset: 0x0011AF30
		public SubmarineBody(Submarine sub, bool showErrorMessages = true)
		{
			this.submarine = sub;
			SubmarineBody.<>c__DisplayClass42_1 CS$<>8__locals2;
			CS$<>8__locals2.minExtents = Vector2.Zero;
			CS$<>8__locals2.maxExtents = Vector2.Zero;
			CS$<>8__locals2.visibleMinExtents = Vector2.Zero;
			CS$<>8__locals2.visibleMaxExtents = Vector2.Zero;
			Body farseerBody = null;
			if (!Hull.HullList.Any((Hull h) => h.Submarine == sub))
			{
				farseerBody = GameMain.World.CreateRectangle(1f, 1f, 1f, default(Vector2), 0f, BodyType.Static, Category.Cat1, Category.All, true);
				if (showErrorMessages)
				{
					DebugConsole.ThrowError("No hulls found in the submarine \"" + sub.Info.Name + "\". Generating a physics body for the submarine failed.", null, null, false, false);
				}
			}
			else
			{
				List<Vector2> convexHull = this.GenerateConvexHull();
				for (int i = 0; i < convexHull.Count; i++)
				{
					convexHull[i] = ConvertUnits.ToSimUnits(convexHull[i]);
				}
				this.HullVertices = convexHull;
				farseerBody = GameMain.World.CreateBody(default(Vector2), 0f, BodyType.Dynamic, false);
				Category collisionCategory = Category.Cat1;
				Category collidesWith = Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7 | Category.Cat8;
				farseerBody.CollisionCategories = collisionCategory;
				farseerBody.CollidesWith = collidesWith;
				farseerBody.Enabled = false;
				farseerBody.UserData = this;
				if (sub.Info.IsOutpost)
				{
					farseerBody.BodyType = BodyType.Static;
				}
				foreach (MapEntity mapEntity in MapEntity.MapEntityList)
				{
					if (mapEntity.Submarine == this.submarine)
					{
						Structure wall = mapEntity as Structure;
						if (wall != null)
						{
							bool hasCollider = wall.HasBody && !wall.IsPlatform && wall.StairDirection == Direction.None;
							Rectangle rect = wall.Rect;
							Quad2D transformedQuad = wall.GetTransformedQuad();
							SubmarineBody.<.ctor>g__AddPointToExtents|42_1(transformedQuad.A, hasCollider, ref CS$<>8__locals2);
							SubmarineBody.<.ctor>g__AddPointToExtents|42_1(transformedQuad.B, hasCollider, ref CS$<>8__locals2);
							SubmarineBody.<.ctor>g__AddPointToExtents|42_1(transformedQuad.C, hasCollider, ref CS$<>8__locals2);
							SubmarineBody.<.ctor>g__AddPointToExtents|42_1(transformedQuad.D, hasCollider, ref CS$<>8__locals2);
							if (hasCollider)
							{
								farseerBody.CreateRectangle(ConvertUnits.ToSimUnits(wall.BodyWidth), ConvertUnits.ToSimUnits(wall.BodyHeight), 50f, -wall.BodyRotation, ConvertUnits.ToSimUnits(new Vector2((float)(rect.X + rect.Width / 2), (float)(rect.Y - rect.Height / 2)) + wall.BodyOffset), collisionCategory, collidesWith).UserData = wall;
							}
						}
					}
				}
				foreach (Hull hull in Hull.HullList)
				{
					if (hull.Submarine == this.submarine && !hull.IdFreed)
					{
						Rectangle rect2 = hull.Rect;
						SubmarineBody.<.ctor>g__AddPointToExtents|42_1(new Vector2((float)rect2.X, (float)(rect2.Y - rect2.Height)), true, ref CS$<>8__locals2);
						SubmarineBody.<.ctor>g__AddPointToExtents|42_1(new Vector2((float)rect2.Right, (float)rect2.Y), true, ref CS$<>8__locals2);
						farseerBody.CreateRectangle(ConvertUnits.ToSimUnits(rect2.Width), ConvertUnits.ToSimUnits(rect2.Height), 100f, ConvertUnits.ToSimUnits(new Vector2((float)(rect2.X + rect2.Width / 2), (float)(rect2.Y - rect2.Height / 2))), collisionCategory, collidesWith).UserData = hull;
					}
				}
				using (List<Item>.Enumerator enumerator3 = Item.ItemList.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Item item = enumerator3.Current;
						if (item.Submarine == this.submarine)
						{
							Vector2 simPos = ConvertUnits.ToSimUnits(item.Position);
							if (sub.FlippedX)
							{
								simPos.X = -simPos.X;
							}
							Door door = item.GetComponent<Door>();
							if (door != null)
							{
								door.OutsideSubmarineFixture = farseerBody.CreateRectangle(door.Body.Width, door.Body.Height, 5f, simPos, collisionCategory, collidesWith);
								door.OutsideSubmarineFixture.UserData = item;
							}
							if (item.StaticBodyConfig != null)
							{
								float radius = item.StaticBodyConfig.GetAttributeFloat("radius", 0f) * item.Scale;
								float width = item.StaticBodyConfig.GetAttributeFloat("width", 0f) * item.Scale;
								float height = item.StaticBodyConfig.GetAttributeFloat("height", 0f) * item.Scale;
								float simRadius = ConvertUnits.ToSimUnits(radius);
								float simWidth = ConvertUnits.ToSimUnits(width);
								float simHeight = ConvertUnits.ToSimUnits(height);
								if (radius > 0f || (width > 0f && height > 0f))
								{
									Quad2D transformedQuad2 = item.GetTransformedQuad();
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(transformedQuad2.A, true, ref CS$<>8__locals2);
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(transformedQuad2.B, true, ref CS$<>8__locals2);
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(transformedQuad2.C, true, ref CS$<>8__locals2);
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(transformedQuad2.D, true, ref CS$<>8__locals2);
								}
								if (width > 0f && height > 0f)
								{
									item.StaticFixtures.Add(farseerBody.CreateRectangle(simWidth, simHeight, 5f, simPos, collisionCategory, collidesWith));
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(item.Position - new Vector2(width, height) / 2f, true, ref CS$<>8__locals2);
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(item.Position + new Vector2(width, height) / 2f, true, ref CS$<>8__locals2);
								}
								else if (radius > 0f && width > 0f)
								{
									item.StaticFixtures.Add(farseerBody.CreateRectangle(simWidth, simRadius * 2f, 5f, simPos, collisionCategory, collidesWith));
									item.StaticFixtures.Add(farseerBody.CreateCircle(simRadius, 5f, simPos - Vector2.UnitX * simWidth / 2f, collisionCategory, collidesWith));
									item.StaticFixtures.Add(farseerBody.CreateCircle(simRadius, 5f, simPos + Vector2.UnitX * simWidth / 2f, collisionCategory, collidesWith));
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(item.Position - new Vector2(width / 2f + radius, height / 2f), true, ref CS$<>8__locals2);
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(item.Position + new Vector2(width / 2f + radius, height / 2f), true, ref CS$<>8__locals2);
								}
								else if (radius > 0f && height > 0f)
								{
									item.StaticFixtures.Add(farseerBody.CreateRectangle(simRadius * 2f, height, 5f, simPos, collisionCategory, collidesWith));
									item.StaticFixtures.Add(farseerBody.CreateCircle(simRadius, 5f, simPos - Vector2.UnitY * simHeight / 2f, collisionCategory, collidesWith));
									item.StaticFixtures.Add(farseerBody.CreateCircle(simRadius, 5f, simPos + Vector2.UnitY * simHeight / 2f, collisionCategory, collidesWith));
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(item.Position - new Vector2(width / 2f, height / 2f + radius), true, ref CS$<>8__locals2);
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(item.Position + new Vector2(width / 2f, height / 2f + radius), true, ref CS$<>8__locals2);
								}
								else if (radius > 0f)
								{
									item.StaticFixtures.Add(farseerBody.CreateCircle(simRadius, 5f, simPos, collisionCategory, collidesWith));
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(item.Position - new Vector2(radius, radius), true, ref CS$<>8__locals2);
									SubmarineBody.<.ctor>g__AddPointToExtents|42_1(item.Position + new Vector2(radius, radius), true, ref CS$<>8__locals2);
								}
								item.StaticFixtures.ForEach(delegate(Fixture f)
								{
									f.UserData = item;
								});
							}
						}
					}
				}
				this.Borders = new Rectangle((int)CS$<>8__locals2.minExtents.X, (int)CS$<>8__locals2.maxExtents.Y, (int)(CS$<>8__locals2.maxExtents.X - CS$<>8__locals2.minExtents.X), (int)(CS$<>8__locals2.maxExtents.Y - CS$<>8__locals2.minExtents.Y));
				this.VisibleBorders = new Rectangle((int)CS$<>8__locals2.visibleMinExtents.X, (int)CS$<>8__locals2.visibleMaxExtents.Y, (int)(CS$<>8__locals2.visibleMaxExtents.X - CS$<>8__locals2.visibleMinExtents.X), (int)(CS$<>8__locals2.visibleMaxExtents.Y - CS$<>8__locals2.visibleMinExtents.Y));
			}
			farseerBody.Enabled = true;
			farseerBody.Restitution = 0f;
			farseerBody.Friction = 0.2f;
			farseerBody.FixedRotation = true;
			farseerBody.Awake = true;
			farseerBody.SleepingAllowed = false;
			farseerBody.IgnoreGravity = true;
			farseerBody.OnCollision += this.OnCollision;
			farseerBody.UserData = this.submarine;
			this.Body = new PhysicsBody(farseerBody);
		}

		// Token: 0x06002B8C RID: 11148 RVA: 0x0011D7D0 File Offset: 0x0011B9D0
		private List<Vector2> GenerateConvexHull()
		{
			List<Structure> subWalls = Structure.WallList.FindAll((Structure wall) => wall.Submarine == this.submarine);
			if (subWalls.Count == 0)
			{
				return new List<Vector2>
				{
					new Vector2(-1f, 1f),
					new Vector2(1f, 1f),
					new Vector2(0f, -1f)
				};
			}
			List<Vector2> points = new List<Vector2>();
			foreach (Structure wall2 in subWalls)
			{
				points.Add(new Vector2((float)wall2.Rect.X, (float)wall2.Rect.Y));
				points.Add(new Vector2((float)(wall2.Rect.X + wall2.Rect.Width), (float)wall2.Rect.Y));
				points.Add(new Vector2((float)wall2.Rect.X, (float)(wall2.Rect.Y - wall2.Rect.Height)));
				points.Add(new Vector2((float)(wall2.Rect.X + wall2.Rect.Width), (float)(wall2.Rect.Y - wall2.Rect.Height)));
			}
			return MathUtils.GiftWrap(points);
		}

		// Token: 0x06002B8D RID: 11149 RVA: 0x0011D958 File Offset: 0x0011BB58
		public void Update(float deltaTime)
		{
			while (this.impactQueue.Count > 0)
			{
				SubmarineBody.Impact impact = this.impactQueue.Dequeue();
				VoronoiCell cell = impact.Target.UserData as VoronoiCell;
				if (cell != null)
				{
					this.HandleLevelCollision(impact, cell);
				}
				else if (impact.Target.Body.UserData is Structure)
				{
					this.HandleLevelCollision(impact, null);
				}
				else
				{
					Submarine otherSub = impact.Target.Body.UserData as Submarine;
					if (otherSub != null)
					{
						this.HandleSubCollision(impact, otherSub);
					}
					else
					{
						Limb limb = impact.Target.Body.UserData as Limb;
						if (limb != null)
						{
							this.HandleLimbCollision(impact, limb);
						}
					}
				}
			}
			if (this.Body.FarseerBody.BodyType == BodyType.Static)
			{
				return;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			Vector2 totalForce = this.CalculateBuoyancy();
			if (Level.Loaded != null && (this.Position.X < 0f || this.Position.X > (float)Level.Loaded.Size.X))
			{
				Rectangle worldBorders = this.Borders;
				worldBorders.Location += MathUtils.ToPoint(this.Position);
				if (worldBorders.Y > Level.Loaded.Size.Y)
				{
					this.Body.LinearVelocity = new Vector2(this.Body.LinearVelocity.X, Math.Min(this.Body.LinearVelocity.Y, ConvertUnits.ToSimUnits(Level.Loaded.Size.Y - worldBorders.Y)));
				}
				else if (worldBorders.Y - worldBorders.Height < Level.Loaded.BottomPos)
				{
					this.Body.LinearVelocity = new Vector2(this.Body.LinearVelocity.X, Math.Max(this.Body.LinearVelocity.Y, ConvertUnits.ToSimUnits(Level.Loaded.BottomPos - (worldBorders.Y - worldBorders.Height))));
				}
				float distance = (this.Position.X < -30000f) ? Math.Abs(this.Position.X + 30000f) : (this.Position.X - ((float)Level.Loaded.Size.X + 30000f));
				if (distance > 0f)
				{
					if (distance > 200000f)
					{
						if (this.Position.X < 0f)
						{
							this.Body.LinearVelocity = new Vector2(Math.Max(0f, this.Body.LinearVelocity.X), this.Body.LinearVelocity.Y);
						}
						else
						{
							this.Body.LinearVelocity = new Vector2(Math.Min(0f, this.Body.LinearVelocity.X), this.Body.LinearVelocity.Y);
						}
					}
					if (distance > 150000f)
					{
						distance += (float)Math.Pow((double)((distance - 150000f) * 0.01f), 2.0);
					}
					float force = distance * 0.5f;
					totalForce += ((this.Position.X < 0f) ? Vector2.UnitX : (-Vector2.UnitX)) * force;
					if (Character.Controlled != null && Character.Controlled.Submarine == this.submarine)
					{
						GameMain.GameScreen.Cam.Shake = Math.Max(GameMain.GameScreen.Cam.Shake, Math.Min(force * 0.0001f, 5f));
					}
				}
			}
			Vector2 vector;
			if (totalForce.Y > 0f)
			{
				PhysicsBody body = this.Body;
				ContactEdge contactEdge2;
				if (body == null)
				{
					contactEdge2 = null;
				}
				else
				{
					Body farseerBody = body.FarseerBody;
					contactEdge2 = ((farseerBody != null) ? farseerBody.ContactList : null);
				}
				ContactEdge contactEdge = contactEdge2;
				Action<Submarine> <>9__1;
				while (((contactEdge != null) ? contactEdge.Contact : null) != null)
				{
					if (contactEdge.Contact.Enabled)
					{
						object userData = contactEdge.Other.UserData;
						Submarine otherSubmarine = userData as Submarine;
						if (otherSubmarine != null && otherSubmarine.TeamID != this.Submarine.TeamID && contactEdge.Contact.IsTouching)
						{
							FixedArray2<Vector2> points;
							contactEdge.Contact.GetWorldManifold(out vector, out points);
							if (points[0].Y > this.Body.SimPosition.Y && !Character.CharacterList.Any((Character c) => c.Submarine == otherSubmarine && !c.IsIncapacitated && c.TeamID == otherSubmarine.TeamID))
							{
								IEnumerable<Submarine> connectedSubs = otherSubmarine.GetConnectedSubs();
								Action<Submarine> action;
								if ((action = <>9__1) == null)
								{
									action = (<>9__1 = delegate(Submarine s)
									{
										s.SubBody.forceUpwardsTimer += deltaTime;
									});
								}
								connectedSubs.ForEach(action);
								break;
							}
						}
					}
					contactEdge = contactEdge.Next;
				}
			}
			vector = this.Body.LinearVelocity;
			if (vector.LengthSquared() > 0.0001f)
			{
				float attachedMass = 0f;
				for (JointEdge jointEdge = this.Body.FarseerBody.JointList; jointEdge != null; jointEdge = jointEdge.Next)
				{
					Body otherBody = (jointEdge.Joint.BodyA == this.Body.FarseerBody) ? jointEdge.Joint.BodyB : jointEdge.Joint.BodyA;
					Limb limb2 = otherBody.UserData as Limb;
					Character character = (limb2 != null) ? limb2.character : null;
					if (character != null)
					{
						attachedMass += character.Mass;
					}
				}
				float horizontalDragCoefficient = MathHelper.Clamp(0.01f + attachedMass / 5000f, 0f, 0.1f);
				totalForce.X -= (float)Math.Sign(this.Body.LinearVelocity.X) * this.Body.LinearVelocity.X * this.Body.LinearVelocity.X * horizontalDragCoefficient * this.Body.Mass;
				float verticalDragCoefficient = MathHelper.Clamp(0.05f + attachedMass / 5000f, 0f, 0.1f);
				totalForce.Y -= (float)Math.Sign(this.Body.LinearVelocity.Y) * this.Body.LinearVelocity.Y * this.Body.LinearVelocity.Y * verticalDragCoefficient * this.Body.Mass;
			}
			this.ApplyForce(totalForce);
			vector = this.Velocity;
			if (vector.LengthSquared() < 0.01f)
			{
				this.levelContacts.Clear();
				this.levelContacts.AddRange(SubmarineBody.GetLevelContacts(this.Body));
				for (int i = 0; i < this.levelContacts.Count; i++)
				{
					for (int j = i + 1; j < this.levelContacts.Count; j++)
					{
						Vector2 normal;
						FixedArray2<Vector2> fixedArray;
						this.levelContacts[i].GetWorldManifold(out normal, out fixedArray);
						Vector2 normal2;
						this.levelContacts[j].GetWorldManifold(out normal2, out fixedArray);
						if (Vector2.Dot(normal, normal2) < 0f)
						{
							this.ApplyForce(totalForce * 100f);
							i = this.levelContacts.Count;
							break;
						}
					}
				}
			}
			this.UpdateDepthDamage(deltaTime);
			this.forceUpwardsTimer = MathHelper.Clamp(this.forceUpwardsTimer - deltaTime * 0.1f, 0f, 30f);
		}

		// Token: 0x06002B8E RID: 11150 RVA: 0x0011E0F4 File Offset: 0x0011C2F4
		private void DisplaceCharacters(Vector2 subTranslation)
		{
			Rectangle worldBorders = this.Borders;
			worldBorders.Location += MathUtils.ToPoint(ConvertUnits.ToDisplayUnits(this.Body.SimPosition));
			Vector2 translateDir = Vector2.Normalize(subTranslation);
			if (!MathUtils.IsValid(translateDir))
			{
				translateDir = Vector2.UnitY;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c.Submarine == null)
				{
					foreach (Limb limb in c.AnimController.Limbs)
					{
						Vector2 intersection;
						if (!limb.IsSevered && Submarine.RectContains(worldBorders, limb.WorldPosition, false) && MathUtils.GetLineWorldRectangleIntersection(limb.WorldPosition, limb.WorldPosition + translateDir * 100000f, worldBorders, out intersection))
						{
							c.AnimController.SetPosition(ConvertUnits.ToSimUnits(c.WorldPosition + (intersection - limb.WorldPosition)) + translateDir, false, true, false, true);
							break;
						}
					}
				}
			}
		}

		// Token: 0x06002B8F RID: 11151 RVA: 0x0011E238 File Offset: 0x0011C438
		private Vector2 CalculateBuoyancy()
		{
			if (Submarine.LockY)
			{
				return Vector2.Zero;
			}
			IEnumerable<Submarine> connectedSubs = this.submarine.GetConnectedSubs();
			float waterVolume = 0f;
			float volume = 0f;
			float totalMass = connectedSubs.Sum((Submarine s) => s.SubBody.Body.Mass);
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine != null && connectedSubs.Contains(hull.Submarine))
				{
					PhysicsBody physicsBody = hull.Submarine.PhysicsBody;
					if (physicsBody != null && physicsBody.BodyType == BodyType.Dynamic)
					{
						waterVolume += hull.WaterVolume;
						volume += hull.Volume;
					}
				}
			}
			float waterPercentage = (volume <= 0f) ? 0f : (waterVolume / volume);
			float buoyancy = 0.07f - waterPercentage;
			float massRatio = this.Body.Mass / totalMass;
			if (buoyancy > 0f)
			{
				buoyancy *= 2f;
			}
			else
			{
				buoyancy = Math.Max(buoyancy, -0.5f);
			}
			if (this.forceUpwardsTimer > 0f)
			{
				buoyancy = MathHelper.Lerp(buoyancy, 0.1f, this.forceUpwardsTimer / 30f);
			}
			return new Vector2(0f, buoyancy * totalMass * 10f) * massRatio;
		}

		// Token: 0x06002B90 RID: 11152 RVA: 0x0011E3AC File Offset: 0x0011C5AC
		public void ApplyForce(Vector2 force)
		{
			this.Body.ApplyForce(force, 64f);
		}

		// Token: 0x06002B91 RID: 11153 RVA: 0x0011E3BF File Offset: 0x0011C5BF
		public void SetPosition(Vector2 position)
		{
			this.Body.SetTransform(ConvertUnits.ToSimUnits(position), 0f, true);
		}

		// Token: 0x06002B92 RID: 11154 RVA: 0x0011E3DC File Offset: 0x0011C5DC
		private void UpdateDepthDamage(float deltaTime)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) is TestGameMode)
			{
				return;
			}
			if (Level.Loaded == null)
			{
				return;
			}
			if (!this.Submarine.AtCosmeticDamageDepth)
			{
				return;
			}
			this.damageSoundTimer -= deltaTime;
			if (this.damageSoundTimer <= 0f)
			{
				float closenessToCrushDepthRatio = Math.Clamp((this.Submarine.RealWorldDepth - (this.Submarine.RealWorldCrushDepth + -500f)) / 500f, 0f, 1f);
				this.damageSoundTimer = Rand.Range(5f, 10f, Rand.RandSync.Unsynced);
			}
			this.depthDamageTimer -= deltaTime;
			if (this.depthDamageTimer <= 0f && (GameMain.GameSession == null || GameMain.GameSession.RoundDuration > 60f))
			{
				foreach (Structure wall in Structure.WallList)
				{
					if (wall.Submarine == this.submarine)
					{
						float wallCrushDepth = wall.CrushDepth;
						float pastCrushDepth = this.submarine.RealWorldDepth - wallCrushDepth;
						float pastCrushDepthRatio = Math.Clamp(pastCrushDepth / 500f, 0f, 1f);
						if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) <= MathHelper.Lerp(0.1f, 1f, pastCrushDepthRatio))
						{
							float damage = MathHelper.Lerp(50f, 500f, pastCrushDepthRatio);
							if (pastCrushDepth > 0f)
							{
								Explosion.RangedStructureDamage(wall.WorldPosition, 100f, damage, 0f, null, null, true, false, true);
							}
							if (Character.Controlled != null && Character.Controlled.Submarine == this.submarine)
							{
								GameMain.GameScreen.Cam.Shake = Math.Max(GameMain.GameScreen.Cam.Shake, MathHelper.Lerp(10f, 50f, pastCrushDepthRatio));
							}
						}
					}
				}
				this.depthDamageTimer = Rand.Range(5f, 10f, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x06002B93 RID: 11155 RVA: 0x0011E608 File Offset: 0x0011C808
		public void FlipX()
		{
			List<Vector2> convexHull = this.GenerateConvexHull();
			for (int i = 0; i < convexHull.Count; i++)
			{
				convexHull[i] = ConvertUnits.ToSimUnits(convexHull[i]);
			}
			this.HullVertices = convexHull;
		}

		// Token: 0x06002B94 RID: 11156 RVA: 0x0011E648 File Offset: 0x0011C848
		public bool OnCollision(Fixture f1, Fixture f2, Contact contact)
		{
			Limb limb = f2.Body.UserData as Limb;
			if (limb != null)
			{
				bool collision = this.CheckCharacterCollision(contact, limb.character);
				if (collision)
				{
					Queue<SubmarineBody.Impact> obj = this.impactQueue;
					lock (obj)
					{
						this.impactQueue.Enqueue(new SubmarineBody.Impact(f1, f2, contact));
					}
				}
				return collision;
			}
			Character character = f2.Body.UserData as Character;
			if (character != null)
			{
				return this.CheckCharacterCollision(contact, character);
			}
			if (f1.UserData is DockingPort || f2.UserData is DockingPort)
			{
				return false;
			}
			Queue<SubmarineBody.Impact> obj2 = this.impactQueue;
			lock (obj2)
			{
				this.impactQueue.Enqueue(new SubmarineBody.Impact(f1, f2, contact));
			}
			return true;
		}

		// Token: 0x06002B95 RID: 11157 RVA: 0x0011E73C File Offset: 0x0011C93C
		private bool CheckCharacterCollision(Contact contact, Character character)
		{
			if (character.Submarine != null)
			{
				return false;
			}
			CanEnterSubmarine canEnterSubmarine = character.AnimController.CanEnterSubmarine;
			if (canEnterSubmarine == CanEnterSubmarine.False)
			{
				return true;
			}
			if (canEnterSubmarine == CanEnterSubmarine.Partial)
			{
				if (contact.FixtureB.Body == character.AnimController.Collider.FarseerBody)
				{
					return true;
				}
				Limb limb = contact.FixtureB.Body.UserData as Limb;
				if (limb != null && !limb.Params.CanEnterSubmarine)
				{
					return true;
				}
			}
			Vector2 contactNormal;
			FixedArray2<Vector2> points;
			contact.GetWorldManifold(out contactNormal, out points);
			Vector2 normalizedVel = (character.AnimController.Collider.LinearVelocity == Vector2.Zero) ? Vector2.Zero : Vector2.Normalize(character.AnimController.Collider.LinearVelocity);
			Vector2 targetPos = ConvertUnits.ToDisplayUnits(points[0] - contactNormal * 0.1f);
			Hull newHull = Hull.FindHull(targetPos, null, true, true);
			if (newHull == null)
			{
				targetPos = ConvertUnits.ToDisplayUnits(points[0] - contactNormal);
				newHull = Hull.FindHull(targetPos, null, true, true);
			}
			if (newHull == null)
			{
				targetPos = ConvertUnits.ToDisplayUnits(points[0] + normalizedVel);
				newHull = Hull.FindHull(targetPos, null, true, true);
			}
			Structure wall = contact.FixtureA.UserData as Structure;
			if (wall == null || !wall.AllSectionBodiesDisabled())
			{
				Hull newHull2 = newHull;
				IEnumerable<Gap> enumerable = (newHull2 != null) ? newHull2.ConnectedGaps : null;
				IEnumerable<Gap> gaps = enumerable ?? (from g in Gap.GapList
				where g.Submarine == this.submarine
				select g);
				if (Gap.FindAdjacent(gaps, ConvertUnits.ToDisplayUnits(points[0]), 200f, false) == null)
				{
					return true;
				}
			}
			if (character.AnimController.CanEnterSubmarine == CanEnterSubmarine.Partial)
			{
				return contact.FixtureB.Body == character.AnimController.Collider.FarseerBody;
			}
			if (newHull != null)
			{
				CoroutineManager.Invoke(delegate
				{
					if (character != null && !character.Removed)
					{
						character.AnimController.FindHull(new Vector2?(newHull.WorldPosition), true, false);
					}
				}, 0f);
			}
			return false;
		}

		// Token: 0x06002B96 RID: 11158 RVA: 0x0011E97C File Offset: 0x0011CB7C
		private void HandleLimbCollision(SubmarineBody.Impact collision, Limb limb)
		{
			bool flag;
			if (limb == null)
			{
				flag = (null != null);
			}
			else
			{
				PhysicsBody body = limb.body;
				flag = (((body != null) ? body.FarseerBody : null) != null);
			}
			if (!flag || limb.character == null)
			{
				return;
			}
			float impactMass = limb.Mass;
			EnemyAIController enemyAI = limb.character.AIController as EnemyAIController;
			float attackMultiplier = 1f;
			if (((enemyAI != null) ? enemyAI.ActiveAttack : null) != null)
			{
				impactMass = Math.Max(Math.Max(limb.Mass, limb.character.AnimController.MainLimb.Mass), limb.character.AnimController.Collider.Mass);
				attackMultiplier = enemyAI.ActiveAttack.SubmarineImpactMultiplier;
			}
			if (impactMass * attackMultiplier > 10f && this.Body.BodyType != BodyType.Static)
			{
				Vector2 normal = (Vector2.DistanceSquared(this.Body.SimPosition, limb.SimPosition) < 0.0001f) ? Vector2.UnitY : Vector2.Normalize(this.Body.SimPosition - limb.SimPosition);
				float impact = Math.Min(Vector2.Dot(collision.Velocity, -normal), 50f) * Math.Min(impactMass / 300f, 1f);
				impact *= attackMultiplier;
				this.ApplyImpact(impact, normal, collision.ImpactPos, false);
				foreach (Submarine dockedSub in this.submarine.DockedTo)
				{
					dockedSub.SubBody.ApplyImpact(impact, normal, collision.ImpactPos, false);
				}
			}
			IEnumerable<Contact> levelContacts = SubmarineBody.GetLevelContacts(limb.body);
			int levelContactCount = levelContacts.Count<Contact>();
			if (levelContactCount == 0)
			{
				return;
			}
			Vector2 avgContactNormal = Vector2.Zero;
			foreach (Contact levelContact in levelContacts)
			{
				Vector2 contactNormal;
				FixedArray2<Vector2> temp;
				levelContact.GetWorldManifold(out contactNormal, out temp);
				VoronoiCell cell = (levelContact.FixtureB.UserData is VoronoiCell) ? ((VoronoiCell)levelContact.FixtureB.UserData) : ((VoronoiCell)levelContact.FixtureA.UserData);
				Vector2 cellDiff = ConvertUnits.ToDisplayUnits(limb.body.SimPosition) - cell.Center;
				if (Vector2.Dot(contactNormal, cellDiff) < 0f)
				{
					contactNormal = -contactNormal;
				}
				avgContactNormal += contactNormal;
				this.ApplyImpact(Vector2.Dot(-collision.Velocity, contactNormal) / 2f / (float)levelContactCount, contactNormal, collision.ImpactPos, false);
			}
			avgContactNormal /= (float)levelContactCount;
			float contactDot = Vector2.Dot(this.Body.LinearVelocity, -avgContactNormal);
			if (contactDot > 0.001f)
			{
				Vector2 velChange = Vector2.Normalize(this.Body.LinearVelocity) * contactDot;
				if (!MathUtils.IsValid(velChange))
				{
					string identifier = "SubmarineBody.HandleLimbCollision:" + this.submarine.ID.ToString();
					GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
					string[] array = new string[9];
					array[0] = "Invalid velocity change in SubmarineBody.HandleLimbCollision (submarine velocity: ";
					array[1] = this.Body.LinearVelocity.ToString();
					array[2] = ", avgContactNormal: ";
					int num = 3;
					Vector2 vector = avgContactNormal;
					array[num] = vector.ToString();
					array[4] = ", contactDot: ";
					array[5] = contactDot.ToString();
					array[6] = ", velChange: ";
					int num2 = 7;
					vector = velChange;
					array[num2] = vector.ToString();
					array[8] = ")";
					GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, string.Concat(array));
					return;
				}
				this.Body.LinearVelocity -= velChange;
				if (contactDot > 0.1f)
				{
					float damageAmount = contactDot * this.Body.Mass / limb.character.Mass;
					limb.character.LastDamageSource = this.submarine;
					limb.character.DamageLimb(ConvertUnits.ToDisplayUnits(collision.ImpactPos), limb, AfflictionPrefab.ImpactDamage.Instantiate(damageAmount, null).ToEnumerable<Affliction>(), 0f, true, Vector2.Zero, null, 1f, true, 0f, false, false, true);
					if (limb.character.IsDead)
					{
						foreach (LimbJoint limbJoint in limb.character.AnimController.LimbJoints)
						{
							if (!limbJoint.IsSevered && (limbJoint.LimbA == limb || limbJoint.LimbB == limb))
							{
								limb.character.AnimController.SeverLimbJoint(limbJoint);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002B97 RID: 11159 RVA: 0x0011EE24 File Offset: 0x0011D024
		private static IEnumerable<Contact> GetLevelContacts(PhysicsBody body)
		{
			SubmarineBody.<GetLevelContacts>d__56 <GetLevelContacts>d__ = new SubmarineBody.<GetLevelContacts>d__56(-2);
			<GetLevelContacts>d__.<>3__body = body;
			return <GetLevelContacts>d__;
		}

		// Token: 0x06002B98 RID: 11160 RVA: 0x0011EE34 File Offset: 0x0011D034
		private void HandleLevelCollision(SubmarineBody.Impact impact, VoronoiCell cell = null)
		{
			if (GameMain.GameSession != null && GameMain.GameSession.RoundDuration < 10f)
			{
				return;
			}
			float wallImpact = Vector2.Dot(impact.Velocity, -impact.Normal);
			if (wallImpact < 3f)
			{
				return;
			}
			wallImpact *= 3f;
			this.ApplyImpact(wallImpact, -impact.Normal, impact.ImpactPos, true);
			foreach (Submarine dockedSub in this.submarine.DockedTo)
			{
				dockedSub.SubBody.ApplyImpact(wallImpact, -impact.Normal, impact.ImpactPos, true);
			}
			if (cell != null && cell.IsDestructible && wallImpact > 0f)
			{
				Level loaded = Level.Loaded;
				LevelWall hitWall = (loaded != null) ? loaded.ExtraWalls.Find((LevelWall w) => w.Cells.Contains(cell)) : null;
				if (hitWall != null && hitWall.WallDamageOnTouch > 0f)
				{
					Dictionary<Structure, float> damagedStructures = Explosion.RangedStructureDamage(ConvertUnits.ToDisplayUnits(impact.ImpactPos), 500f, hitWall.WallDamageOnTouch, 0f, null, null, true, false, true);
				}
			}
		}

		// Token: 0x06002B99 RID: 11161 RVA: 0x0011EF80 File Offset: 0x0011D180
		private void HandleSubCollision(SubmarineBody.Impact impact, Submarine otherSub)
		{
			if (this.submarine.IsAboveLevel)
			{
				return;
			}
			Vector2 normal = impact.Normal;
			if (impact.Target.Body == otherSub.SubBody.Body.FarseerBody)
			{
				normal = -normal;
			}
			float thisMass = this.Body.Mass + this.submarine.DockedTo.Sum((Submarine s) => s.PhysicsBody.Mass);
			float otherMass = otherSub.PhysicsBody.Mass + otherSub.DockedTo.Sum((Submarine s) => s.PhysicsBody.Mass);
			float massRatio = otherMass / (thisMass + otherMass);
			float impulse = Vector2.Dot(impact.Velocity, normal) / 2f * massRatio;
			this.ApplyImpact(impulse, normal, impact.ImpactPos, true);
			foreach (Submarine dockedSub in this.submarine.DockedTo)
			{
				dockedSub.SubBody.ApplyImpact(impulse, normal, impact.ImpactPos, true);
			}
			IEnumerable<Contact> levelContacts = SubmarineBody.GetLevelContacts(this.Body);
			int levelContactCount = levelContacts.Count<Contact>();
			if (levelContactCount == 0)
			{
				return;
			}
			Vector2 avgContactNormal = Vector2.Zero;
			foreach (Contact levelContact in levelContacts)
			{
				Vector2 contactNormal;
				FixedArray2<Vector2> temp;
				levelContact.GetWorldManifold(out contactNormal, out temp);
				VoronoiCell cell = (levelContact.FixtureB.UserData as VoronoiCell) ?? (levelContact.FixtureA.UserData as VoronoiCell);
				Vector2 cellDiff = ConvertUnits.ToDisplayUnits(this.Body.SimPosition) - cell.Center;
				if (Vector2.Dot(contactNormal, cellDiff) < 0f)
				{
					contactNormal = -contactNormal;
				}
				avgContactNormal += contactNormal;
				this.ApplyImpact(Vector2.Dot(impact.Velocity, contactNormal) / 2f * massRatio / (float)levelContactCount, contactNormal, impact.ImpactPos, true);
			}
			avgContactNormal /= (float)levelContactCount;
			float contactDot = Vector2.Dot(otherSub.PhysicsBody.LinearVelocity, -avgContactNormal);
			if (contactDot > 0f)
			{
				if (otherSub.PhysicsBody.LinearVelocity.LengthSquared() > 0.0001f)
				{
					otherSub.PhysicsBody.LinearVelocity -= Vector2.Normalize(otherSub.PhysicsBody.LinearVelocity) * contactDot;
				}
				impulse = Vector2.Dot(otherSub.Velocity, normal);
				otherSub.SubBody.ApplyImpact(impulse, normal, impact.ImpactPos, true);
				foreach (Submarine dockedSub2 in otherSub.DockedTo)
				{
					dockedSub2.SubBody.ApplyImpact(impulse, normal, impact.ImpactPos, true);
				}
			}
		}

		// Token: 0x06002B9A RID: 11162 RVA: 0x0011F2A8 File Offset: 0x0011D4A8
		private void ApplyImpact(float impact, Vector2 direction, Vector2 impactPos, bool applyDamage = true)
		{
			if (impact < 3f)
			{
				return;
			}
			Vector2 impulse = direction * impact * 0.5f;
			impulse = impulse.ClampLength(5f);
			float impulseMagnitude = impulse.Length();
			if (!MathUtils.IsValid(impulse))
			{
				string[] array = new string[9];
				array[0] = "Invalid impulse in SubmarineBody.ApplyImpact: ";
				int num = 1;
				Vector2 vector = impulse;
				array[num] = vector.ToString();
				array[2] = ". Direction: ";
				int num2 = 3;
				vector = direction;
				array[num2] = vector.ToString();
				array[4] = ", body position: ";
				array[5] = this.Body.SimPosition.ToString();
				array[6] = ", impact: ";
				array[7] = impact.ToString();
				array[8] = ".";
				string errorMsg = string.Concat(array);
				if (GameMain.NetworkMember != null)
				{
					errorMsg += (GameMain.NetworkMember.IsClient ? " Playing as a client." : " Hosting a server.");
				}
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
				GameAnalyticsManager.AddErrorEventOnce("SubmarineBody.ApplyImpact:InvalidImpulse", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c.Submarine == this.submarine && c.KnockbackCooldownTimer <= 0f)
				{
					c.KnockbackCooldownTimer = 5f;
					foreach (Limb limb in c.AnimController.Limbs)
					{
						if (!limb.IsSevered)
						{
							limb.body.ApplyLinearImpulse(limb.Mass * impulse, 10f);
						}
					}
					bool holdingOntoSomething = false;
					if (c.SelectedSecondaryItem != null)
					{
						bool flag;
						if (!c.SelectedSecondaryItem.IsLadder)
						{
							Controller component = c.SelectedSecondaryItem.GetComponent<Controller>();
							flag = (component != null && component.LimbPositions.Any<LimbPos>());
						}
						else
						{
							flag = true;
						}
						holdingOntoSomething = flag;
					}
					if (!holdingOntoSomething && c.SelectedItem != null)
					{
						Controller component2 = c.SelectedItem.GetComponent<Controller>();
						holdingOntoSomething = (component2 != null && component2.LimbPositions.Any<LimbPos>());
					}
					if (!holdingOntoSomething)
					{
						c.AnimController.Collider.ApplyLinearImpulse(c.AnimController.Collider.Mass * impulse, 10f);
						if (impact >= 5f)
						{
							float impactDamage = c.AnimController.GetImpactDamage(impact, null);
							c.AddDamage(impactPos, AfflictionPrefab.ImpactDamage.Instantiate(impactDamage, null).ToEnumerable<Affliction>(), Math.Min(impulse.Length() * 0.2f, 2f), true, null, null, 1f);
						}
					}
				}
			}
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine == this.submarine && Timing.TotalTimeUnpaused >= item.LastSubmarineImpactTime + 0.10000000149011612)
				{
					PhysicsBody body = item.body;
					if (body == null || body.BodyType != BodyType.Dynamic)
					{
						if (!item.Prefab.ReceiveSubmarineImpacts)
						{
							continue;
						}
						item.ReceiveImpact(impact, false);
						item.LastSubmarineImpactTime = Timing.TotalTimeUnpaused;
					}
					if (item.body.Enabled && item.CurrentHull != null && item.body.Mass <= impulseMagnitude)
					{
						item.body.ApplyLinearImpulse(impulse, 10f);
						item.PositionUpdateInterval = 0f;
						item.LastSubmarineImpactTime = Timing.TotalTimeUnpaused;
					}
				}
			}
			float dmg = applyDamage ? (impact * 3f) : 0f;
			Dictionary<Structure, float> damagedStructures = Explosion.RangedStructureDamage(ConvertUnits.ToDisplayUnits(impactPos), impact * 50f, dmg, dmg, null, null, true, false, true);
		}

		// Token: 0x06002B9B RID: 11163 RVA: 0x0011F6C4 File Offset: 0x0011D8C4
		public void Remove()
		{
			this.Body.Remove();
		}

		// Token: 0x06002B9C RID: 11164 RVA: 0x0011F6D4 File Offset: 0x0011D8D4
		[CompilerGenerated]
		internal static void <.ctor>g__AddPointToExtents|42_1(Vector2 point, bool hasCollider, ref SubmarineBody.<>c__DisplayClass42_1 A_2)
		{
			A_2.visibleMinExtents.X = Math.Min(point.X, A_2.visibleMinExtents.X);
			A_2.visibleMinExtents.Y = Math.Min(point.Y, A_2.visibleMinExtents.Y);
			A_2.visibleMaxExtents.X = Math.Max(point.X, A_2.visibleMaxExtents.X);
			A_2.visibleMaxExtents.Y = Math.Max(point.Y, A_2.visibleMaxExtents.Y);
			if (hasCollider)
			{
				A_2.minExtents.X = Math.Min(point.X, A_2.minExtents.X);
				A_2.minExtents.Y = Math.Min(point.Y, A_2.minExtents.Y);
				A_2.maxExtents.X = Math.Max(point.X, A_2.maxExtents.X);
				A_2.maxExtents.Y = Math.Max(point.Y, A_2.maxExtents.Y);
			}
		}

		// Token: 0x04001558 RID: 5464
		public const float NeutralBallastPercentage = 0.07f;

		// Token: 0x04001559 RID: 5465
		public const Category CollidesWith = Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7 | Category.Cat8;

		// Token: 0x0400155A RID: 5466
		private const float HorizontalDrag = 0.01f;

		// Token: 0x0400155B RID: 5467
		private const float VerticalDrag = 0.05f;

		// Token: 0x0400155C RID: 5468
		private const float MaxDrag = 0.1f;

		// Token: 0x0400155D RID: 5469
		private const float ImpactDamageMultiplier = 3f;

		// Token: 0x0400155E RID: 5470
		private const float MinImpactLimbMass = 10f;

		// Token: 0x0400155F RID: 5471
		private const float MinCollisionImpact = 3f;

		// Token: 0x04001560 RID: 5472
		private const float MaxCollisionImpact = 5f;

		// Token: 0x04001561 RID: 5473
		private const float Friction = 0.2f;

		// Token: 0x04001562 RID: 5474
		private const float Restitution = 0f;

		// Token: 0x04001563 RID: 5475
		private readonly List<Contact> levelContacts = new List<Contact>();

		// Token: 0x04001565 RID: 5477
		private float depthDamageTimer = 10f;

		// Token: 0x04001566 RID: 5478
		private float damageSoundTimer = 10f;

		// Token: 0x04001567 RID: 5479
		private readonly Submarine submarine;

		// Token: 0x04001568 RID: 5480
		public readonly PhysicsBody Body;

		// Token: 0x04001569 RID: 5481
		private readonly List<PosInfo> positionBuffer = new List<PosInfo>();

		// Token: 0x0400156A RID: 5482
		private readonly Queue<SubmarineBody.Impact> impactQueue = new Queue<SubmarineBody.Impact>();

		// Token: 0x0400156B RID: 5483
		private float forceUpwardsTimer;

		// Token: 0x0400156C RID: 5484
		private const float ForceUpwardsDelay = 30f;

		// Token: 0x0400156F RID: 5487
		public const float CosmeticDamageEffectThreshold = -500f;

		// Token: 0x02000AB8 RID: 2744
		private struct Impact
		{
			// Token: 0x06005E2A RID: 24106 RVA: 0x00204CA8 File Offset: 0x00202EA8
			public Impact(Fixture f1, Fixture f2, Contact contact)
			{
				this.Target = f2;
				Vector2 contactNormal;
				FixedArray2<Vector2> points;
				contact.GetWorldManifold(out contactNormal, out points);
				if (contact.FixtureA.Body == f1.Body)
				{
					contactNormal = -contactNormal;
				}
				this.ImpactPos = points[0];
				this.Normal = contactNormal;
				this.Velocity = f1.Body.LinearVelocity - f2.Body.LinearVelocity;
			}

			// Token: 0x04003701 RID: 14081
			public Fixture Target;

			// Token: 0x04003702 RID: 14082
			public Vector2 Velocity;

			// Token: 0x04003703 RID: 14083
			public Vector2 ImpactPos;

			// Token: 0x04003704 RID: 14084
			public Vector2 Normal;
		}
	}
}
