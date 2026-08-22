using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Barotrauma.Particles;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005A8 RID: 1448
	internal class ElectricalDischarger : Powered, IServerSerializable, INetSerializable
	{
		// Token: 0x060057EA RID: 22506 RVA: 0x002D7DC8 File Offset: 0x002D5FC8
		public void DrawElectricity(SpriteBatch spriteBatch)
		{
			if (this.timer <= 0f)
			{
				Screen selected = Screen.Selected;
				if (selected != null && !selected.IsEditor)
				{
					return;
				}
			}
			for (int i = 0; i < this.nodes.Count; i++)
			{
				if (this.nodes[i].Length > 1f)
				{
					ElectricalDischarger.Node node = this.nodes[i];
					ElectricalDischarger.electricitySprite.Draw(spriteBatch, (i + this.frameOffset) % ElectricalDischarger.electricitySprite.FrameCount, new Vector2(node.WorldPosition.X, -node.WorldPosition.Y), Color.Lerp(Color.LightBlue, Color.White, Rand.Range(0f, 1f, Rand.RandSync.Unsynced)), ElectricalDischarger.electricitySprite.Origin, -node.Angle - 1.5707964f, new Vector2(Math.Min(node.Length / (float)ElectricalDischarger.electricitySprite.FrameSize.X, 1f) * Rand.Range(0.5f, 2f, Rand.RandSync.Unsynced), node.Length / (float)ElectricalDischarger.electricitySprite.FrameSize.Y) * Rand.Range(1f, 1.2f, Rand.RandSync.Unsynced), SpriteEffects.None, null);
				}
			}
			if (GameMain.DebugDraw)
			{
				for (int j = 1; j < this.nodes.Count; j++)
				{
					GUI.DrawLine(spriteBatch, new Vector2(this.nodes[j].WorldPosition.X, -this.nodes[j].WorldPosition.Y), new Vector2(this.nodes[this.nodes[j].ParentIndex].WorldPosition.X, -this.nodes[this.nodes[j].ParentIndex].WorldPosition.Y), Color.LightCyan, 0f, 3f);
					if (this.nodes[j].Length > 1f)
					{
						GUI.DrawRectangle(spriteBatch, new Vector2(this.nodes[j].WorldPosition.X, -this.nodes[j].WorldPosition.Y), Vector2.One * 10f, Color.LightCyan, true, 0f, 1f);
					}
				}
			}
		}

		// Token: 0x060057EB RID: 22507 RVA: 0x002D8050 File Offset: 0x002D6250
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			ushort userID = msg.ReadUInt16();
			if (userID != 0)
			{
				this.user = (Entity.FindEntityByID(userID) as Character);
			}
			base.CurrPowerConsumption = this.powerConsumption;
			this.charging = true;
			this.timer = this.Duration;
			this.IsActive = true;
		}

		// Token: 0x170015E7 RID: 5607
		// (get) Token: 0x060057EC RID: 22508 RVA: 0x002D809E File Offset: 0x002D629E
		public static IEnumerable<ElectricalDischarger> List
		{
			get
			{
				return ElectricalDischarger.list;
			}
		}

		// Token: 0x170015E8 RID: 5608
		// (get) Token: 0x060057ED RID: 22509 RVA: 0x002D80A5 File Offset: 0x002D62A5
		// (set) Token: 0x060057EE RID: 22510 RVA: 0x002D80AD File Offset: 0x002D62AD
		public override bool IsActive
		{
			get
			{
				return base.IsActive;
			}
			set
			{
				base.IsActive = value;
				if (!value)
				{
					this.nodes.Clear();
					this.charactersInRange.Clear();
				}
			}
		}

		// Token: 0x170015E9 RID: 5609
		// (get) Token: 0x060057EF RID: 22511 RVA: 0x002D80CF File Offset: 0x002D62CF
		// (set) Token: 0x060057F0 RID: 22512 RVA: 0x002D80D7 File Offset: 0x002D62D7
		[Serialize(500f, IsPropertySaveable.Yes, "How far the discharge can travel from the item.", "", true)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 5000f)]
		public float Range { get; set; }

		// Token: 0x170015EA RID: 5610
		// (get) Token: 0x060057F1 RID: 22513 RVA: 0x002D80E0 File Offset: 0x002D62E0
		// (set) Token: 0x060057F2 RID: 22514 RVA: 0x002D80E8 File Offset: 0x002D62E8
		[Serialize(25f, IsPropertySaveable.Yes, "How much further can the discharge be carried when moving across walls.", "", true)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float RangeMultiplierInWalls { get; set; }

		// Token: 0x170015EB RID: 5611
		// (get) Token: 0x060057F3 RID: 22515 RVA: 0x002D80F1 File Offset: 0x002D62F1
		// (set) Token: 0x060057F4 RID: 22516 RVA: 0x002D80F9 File Offset: 0x002D62F9
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float RaycastRange { get; set; }

		// Token: 0x170015EC RID: 5612
		// (get) Token: 0x060057F5 RID: 22517 RVA: 0x002D8102 File Offset: 0x002D6302
		// (set) Token: 0x060057F6 RID: 22518 RVA: 0x002D810A File Offset: 0x002D630A
		[Serialize(0.25f, IsPropertySaveable.Yes, "The duration of an individual discharge (in seconds).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 60f, ValueStep = 0.1f, DecimalCount = 2)]
		public float Duration { get; set; }

		// Token: 0x170015ED RID: 5613
		// (get) Token: 0x060057F7 RID: 22519 RVA: 0x002D8113 File Offset: 0x002D6313
		// (set) Token: 0x060057F8 RID: 22520 RVA: 0x002D811B File Offset: 0x002D631B
		[Serialize(0.25f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 60f, ValueStep = 0.1f, DecimalCount = 2)]
		public float Reload { get; set; }

		// Token: 0x170015EE RID: 5614
		// (get) Token: 0x060057F9 RID: 22521 RVA: 0x002D8124 File Offset: 0x002D6324
		// (set) Token: 0x060057FA RID: 22522 RVA: 0x002D812C File Offset: 0x002D632C
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, the discharge cannot travel inside the submarine nor shock anyone inside.", "", false)]
		[Editable]
		public bool OutdoorsOnly { get; set; }

		// Token: 0x170015EF RID: 5615
		// (get) Token: 0x060057FB RID: 22523 RVA: 0x002D8135 File Offset: 0x002D6335
		// (set) Token: 0x060057FC RID: 22524 RVA: 0x002D813D File Offset: 0x002D633D
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool IgnoreUser { get; set; }

		// Token: 0x170015F0 RID: 5616
		// (get) Token: 0x060057FD RID: 22525 RVA: 0x002D8146 File Offset: 0x002D6346
		public IEnumerable<ElectricalDischarger.Node> Nodes
		{
			get
			{
				return this.nodes;
			}
		}

		// Token: 0x060057FE RID: 22526 RVA: 0x002D8150 File Offset: 0x002D6350
		public ElectricalDischarger(Item item, ContentXElement element) : base(item, element)
		{
			ElectricalDischarger.list.Add(this);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "attack")
				{
					this.attack = new Attack(subElement, item.Name);
				}
			}
			this.InitProjSpecific();
		}

		// Token: 0x060057FF RID: 22527 RVA: 0x002D81F8 File Offset: 0x002D63F8
		private void InitProjSpecific()
		{
			if (ElectricalDischarger.electricitySprite == null)
			{
				ElectricalDischarger.electricitySprite = new SpriteSheet("Content/Lights/Electricity.png", 4, 4, new Vector2(0.5f, 0f), null);
			}
		}

		// Token: 0x06005800 RID: 22528 RVA: 0x002D8238 File Offset: 0x002D6438
		public override bool Use(float deltaTime, Character character = null)
		{
			if (this.IsActive)
			{
				return false;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return false;
			}
			if (character != null && !base.CharacterUsable)
			{
				return false;
			}
			this.charging = true;
			this.timer = this.Duration;
			this.IsActive = true;
			this.user = character;
			return false;
		}

		// Token: 0x06005801 RID: 22529 RVA: 0x002D8294 File Offset: 0x002D6494
		public override void Update(float deltaTime, Camera cam)
		{
			this.frameOffset = Rand.Int(ElectricalDischarger.electricitySprite.FrameCount, Rand.RandSync.Unsynced);
			if (this.timer > 0f)
			{
				this.timer -= deltaTime;
				if (this.charging)
				{
					bool hasPower;
					if (this.item.Connections == null)
					{
						hasPower = this.HasPower;
					}
					else
					{
						hasPower = (base.GetAvailableInstantaneousBatteryPower() >= base.PowerConsumption);
					}
					if (hasPower)
					{
						IEnumerable<PowerContainer> batteries = from b in base.GetDirectlyConnectedBatteries()
						where !b.OutputDisabled && b.Charge > 0.0001f && b.MaxOutPut > 0.0001f
						select b;
						float neededPower = base.PowerConsumption;
						while (neededPower > 0.0001f && batteries.Any<PowerContainer>())
						{
							float takePower = neededPower / (float)batteries.Count<PowerContainer>();
							takePower = Math.Min(takePower, batteries.Min((PowerContainer b) => Math.Min(b.Charge * 3600f, b.MaxOutPut)));
							foreach (PowerContainer battery in batteries)
							{
								neededPower -= takePower;
								battery.Charge -= takePower / 3600f;
							}
						}
						this.Discharge();
					}
				}
				return;
			}
			if (this.reloadTimer > 0f)
			{
				this.reloadTimer -= deltaTime;
				return;
			}
			this.IsActive = false;
		}

		// Token: 0x06005802 RID: 22530 RVA: 0x002D8408 File Offset: 0x002D6608
		public override float GetCurrentPowerConsumption(Connection conn = null)
		{
			return 0f;
		}

		// Token: 0x06005803 RID: 22531 RVA: 0x002D840F File Offset: 0x002D660F
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.nodes.Clear();
			this.charactersInRange.Clear();
		}

		// Token: 0x06005804 RID: 22532 RVA: 0x002D8430 File Offset: 0x002D6630
		private void Discharge()
		{
			this.reloadTimer = this.Reload;
			base.ApplyStatusEffects(ActionType.OnUse, 1f, null, null, null, null, null, 1f);
			this.FindNodes(this.item.WorldPosition, this.Range);
			if (this.attack != null)
			{
				foreach (ValueTuple<Character, ElectricalDischarger.Node> valueTuple in this.charactersInRange)
				{
					Character character = valueTuple.Item1;
					ElectricalDischarger.Node node = valueTuple.Item2;
					if (character != null && !character.Removed)
					{
						character.ApplyAttack(this.user, node.WorldPosition, this.attack, MathHelper.Clamp(base.Voltage, 1f, 2f), character.WorldPosition - node.WorldPosition, false, null, 0f);
					}
				}
			}
			this.DischargeProjSpecific();
			this.charging = false;
		}

		// Token: 0x06005805 RID: 22533 RVA: 0x002D8534 File Offset: 0x002D6734
		private void DischargeProjSpecific()
		{
			base.PlaySound(ActionType.OnUse, null);
			foreach (ElectricalDischarger.Node node in this.nodes)
			{
				GameMain.ParticleManager.CreateParticle("swirlysmoke", node.WorldPosition, Vector2.Zero, 0f, null, 0f, null);
				if (node.ParentIndex > -1)
				{
					ElectricalDischarger.<DischargeProjSpecific>g__CreateParticlesBetween|57_0(this.nodes[node.ParentIndex].WorldPosition, node.WorldPosition);
				}
			}
			foreach (ValueTuple<Character, ElectricalDischarger.Node> character in this.charactersInRange)
			{
				ElectricalDischarger.<DischargeProjSpecific>g__CreateParticlesBetween|57_0(character.Item1.WorldPosition, character.Item2.WorldPosition);
			}
		}

		// Token: 0x06005806 RID: 22534 RVA: 0x002D8630 File Offset: 0x002D6830
		public void FindNodes(Vector2 worldPosition, float range)
		{
			if (this.RaycastRange > 0f)
			{
				float angle = 0f;
				float dir = 1f;
				if (this.item.body != null)
				{
					angle += this.item.body.Rotation;
					dir = this.item.body.Dir;
				}
				worldPosition += new Vector2((float)Math.Cos((double)angle), (float)Math.Sin((double)angle)) * this.RaycastRange * dir;
			}
			List<Submarine> submarinesInRange = new List<Submarine>();
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (this.item.Submarine == sub)
				{
					submarinesInRange.Add(sub);
				}
				else if (sub != null)
				{
					Rectangle subBorders = new Rectangle(sub.Borders.X - (int)range, sub.Borders.Y + (int)range, sub.Borders.Width + (int)(range * 2f), sub.Borders.Height + (int)(range * 2f));
					subBorders.Location += MathUtils.ToPoint(sub.SubBody.Position);
					if (Submarine.RectContains(subBorders, worldPosition, false))
					{
						submarinesInRange.Add(sub);
					}
				}
			}
			List<Entity> entitiesInRange = new List<Entity>(100);
			foreach (Structure structure in Structure.WallList)
			{
				if (structure.HasBody && !structure.IsPlatform && (structure.Submarine == null || submarinesInRange.Contains(structure.Submarine)))
				{
					Rectangle structureWorldRect = structure.WorldRect;
					if (worldPosition.X >= (float)structureWorldRect.X - range && worldPosition.X <= (float)structureWorldRect.Right + range && worldPosition.Y <= (float)structureWorldRect.Y + range && worldPosition.Y >= (float)(structureWorldRect.Y - structureWorldRect.Height) - range)
					{
						if (structure.Submarine != null)
						{
							if (!submarinesInRange.Contains(structure.Submarine))
							{
								continue;
							}
							if (this.OutdoorsOnly)
							{
								Vector2 normal = new Vector2((float)(-(float)Math.Sin((double)(structure.IsHorizontal ? (-(double)structure.BodyRotation) : (1.5707964f - structure.BodyRotation)))), (float)Math.Cos((double)(structure.IsHorizontal ? (-(double)structure.BodyRotation) : (1.5707964f - structure.BodyRotation))));
								Vector2 structurePos = structure.Position;
								float offsetAmount = Submarine.GridSize.X * 2f;
								if (structure.HasBody)
								{
									structurePos = ConvertUnits.ToDisplayUnits(structure.Bodies.First<Body>().Position);
									offsetAmount = Math.Max(offsetAmount, structure.IsHorizontal ? structure.BodyHeight : structure.BodyWidth);
								}
								if (Hull.FindHull(structurePos + normal * offsetAmount, null, false, true) != null && Hull.FindHull(structurePos - normal * offsetAmount, null, false, true) != null)
								{
									continue;
								}
							}
						}
						entitiesInRange.Add(structure);
					}
				}
			}
			this.nodes.Clear();
			if (this.RaycastRange > 0f)
			{
				this.nodes.Add(new ElectricalDischarger.Node(this.item.WorldPosition, -1, 0f, 0f));
				int parentNodeIndex = 0;
				this.AddNodesBetweenPoints(this.item.WorldPosition, worldPosition, 0.5f, ref parentNodeIndex);
			}
			else
			{
				this.nodes.Add(new ElectricalDischarger.Node(worldPosition, -1, 0f, 0f));
			}
			float totalRange = this.RaycastRange + range;
			foreach (Character character in Character.CharacterList)
			{
				if (character.Enabled && (!this.IgnoreUser || character != this.user) && (!this.OutdoorsOnly || character.Submarine == null) && (character.Submarine == null || submarinesInRange.Contains(character.Submarine)))
				{
					if (Vector2.DistanceSquared(character.WorldPosition, worldPosition) < totalRange * totalRange * this.RangeMultiplierInWalls)
					{
						entitiesInRange.Add(character);
					}
					if (this.RaycastRange > 0f)
					{
						float distSqr = MathUtils.LineSegmentToPointDistanceSquared(worldPosition, this.item.WorldPosition, character.WorldPosition);
						if (distSqr < range * range * this.RangeMultiplierInWalls)
						{
							if (!entitiesInRange.Contains(character))
							{
								entitiesInRange.Add(character);
							}
							this.charactersInRange.Add(new ValueTuple<Character, ElectricalDischarger.Node>(character, this.nodes.First<ElectricalDischarger.Node>()));
						}
					}
				}
			}
			this.FindNodes(entitiesInRange, worldPosition, this.nodes.Count - 1, range);
			for (int i = 0; i < this.nodes.Count; i++)
			{
				if (this.nodes[i].ParentIndex >= 0)
				{
					ElectricalDischarger.Node parentNode = this.nodes[this.nodes[i].ParentIndex];
					float length = Vector2.Distance(this.nodes[i].WorldPosition, parentNode.WorldPosition) * Rand.Range(1f, 1.25f, Rand.RandSync.Unsynced);
					float angle2 = MathUtils.VectorToAngle(parentNode.WorldPosition - this.nodes[i].WorldPosition);
					this.nodes[i] = new ElectricalDischarger.Node(this.nodes[i].WorldPosition, this.nodes[i].ParentIndex, length, angle2);
				}
			}
		}

		// Token: 0x06005807 RID: 22535 RVA: 0x002D8C60 File Offset: 0x002D6E60
		private void FindNodes(List<Entity> entitiesInRange, Vector2 currPos, int parentNodeIndex, float currentRange)
		{
			if (currentRange <= 0f || this.nodes.Count >= 100)
			{
				return;
			}
			int closestIndex = -1;
			float closestDist = float.MaxValue;
			for (int i = 0; i < entitiesInRange.Count; i++)
			{
				float dist = float.MaxValue;
				Structure structure = entitiesInRange[i] as Structure;
				if (structure != null)
				{
					if (structure.IsHorizontal)
					{
						dist = Math.Abs(structure.WorldPosition.Y - currPos.Y);
						if (currPos.X < (float)structure.WorldRect.X)
						{
							dist += (float)structure.WorldRect.X - currPos.X;
						}
						else if (currPos.X > (float)structure.WorldRect.Right)
						{
							dist += currPos.X - (float)structure.WorldRect.Right;
						}
					}
					else
					{
						dist = Math.Abs(structure.WorldPosition.X - currPos.X);
						if (currPos.Y < (float)(structure.WorldRect.Y - structure.Rect.Height))
						{
							dist += (float)(structure.WorldRect.Y - structure.Rect.Height) - currPos.Y;
						}
						else if (currPos.Y > (float)structure.WorldRect.Y)
						{
							dist += currPos.Y - (float)structure.WorldRect.Y;
						}
					}
				}
				else
				{
					Character character2 = entitiesInRange[i] as Character;
					if (character2 != null)
					{
						dist = MathF.Sqrt(MathUtils.LineSegmentToPointDistanceSquared(currPos, this.nodes[parentNodeIndex].WorldPosition, character2.WorldPosition));
					}
				}
				if (dist < closestDist)
				{
					closestIndex = i;
					closestDist = dist;
				}
			}
			if (closestIndex == -1 || closestDist > currentRange)
			{
				for (int j = 0; j < Rand.Int(4, Rand.RandSync.Unsynced); j++)
				{
					Vector2 targetPos = currPos + Rand.Vector(150f * Rand.Range(0.5f, 1.5f, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced);
					this.nodes.Add(new ElectricalDischarger.Node(targetPos, parentNodeIndex, 0f, 0f));
				}
				return;
			}
			currentRange -= closestDist;
			Structure targetStructure = entitiesInRange[closestIndex] as Structure;
			Character character;
			if (targetStructure != null)
			{
				if (targetStructure.IsHorizontal)
				{
					int yDir = (this.OutdoorsOnly && targetStructure.Submarine != null) ? Math.Sign(targetStructure.WorldPosition.Y - targetStructure.Submarine.WorldPosition.Y) : Math.Sign(currPos.Y - targetStructure.WorldPosition.Y);
					int sectionIndex = targetStructure.FindSectionIndex(currPos, true, true);
					if (sectionIndex == -1)
					{
						return;
					}
					Vector2 sectionPos = targetStructure.SectionPosition(sectionIndex, true);
					Vector2 targetPos2 = new Vector2(MathHelper.Clamp(sectionPos.X, (float)targetStructure.WorldRect.X, (float)targetStructure.WorldRect.Right), sectionPos.Y + targetStructure.BodyHeight / 2f * (float)yDir);
					this.AddNodesBetweenPoints(currPos, targetPos2, 0.25f, ref parentNodeIndex);
					this.nodes.Add(new ElectricalDischarger.Node(targetPos2, parentNodeIndex, 0f, 0f));
					int nodeIndex = this.nodes.Count - 1;
					entitiesInRange.RemoveAt(closestIndex);
					float newRange = currentRange - (float)(targetStructure.Rect.Width / 2) * (1f / this.RangeMultiplierInWalls);
					int leftNodeIndex = nodeIndex;
					Vector2 leftPos = targetStructure.SectionPosition(0, true);
					leftPos.Y += targetStructure.BodyHeight / 2f * (float)yDir;
					this.AddNodesBetweenPoints(targetPos2, leftPos, 0.05f, ref leftNodeIndex);
					this.nodes.Add(new ElectricalDischarger.Node(leftPos, leftNodeIndex, 0f, 0f));
					this.FindNodes(entitiesInRange, leftPos, this.nodes.Count - 1, newRange);
					int rightNodeIndex = nodeIndex;
					Vector2 rightPos = targetStructure.SectionPosition(targetStructure.SectionCount - 1, true);
					leftPos.Y += targetStructure.BodyHeight / 2f * (float)yDir;
					this.AddNodesBetweenPoints(targetPos2, rightPos, 0.05f, ref rightNodeIndex);
					this.nodes.Add(new ElectricalDischarger.Node(rightPos, rightNodeIndex, 0f, 0f));
					this.FindNodes(entitiesInRange, rightPos, this.nodes.Count - 1, newRange);
				}
				else
				{
					int xDir = (this.OutdoorsOnly && targetStructure.Submarine != null) ? Math.Sign(targetStructure.WorldPosition.X - targetStructure.Submarine.WorldPosition.X) : Math.Sign(currPos.X - targetStructure.WorldPosition.X);
					int sectionIndex2 = targetStructure.FindSectionIndex(currPos, true, true);
					if (sectionIndex2 == -1)
					{
						return;
					}
					Vector2 sectionPos2 = targetStructure.SectionPosition(sectionIndex2, true);
					Vector2 targetPos3 = new Vector2(sectionPos2.X + targetStructure.BodyWidth / 2f * (float)xDir, MathHelper.Clamp(sectionPos2.Y, (float)(targetStructure.WorldRect.Y - targetStructure.Rect.Height), (float)targetStructure.WorldRect.Y));
					this.AddNodesBetweenPoints(currPos, targetPos3, 0.25f, ref parentNodeIndex);
					this.nodes.Add(new ElectricalDischarger.Node(targetPos3, parentNodeIndex, 0f, 0f));
					int nodeIndex2 = this.nodes.Count - 1;
					entitiesInRange.RemoveAt(closestIndex);
					float newRange2 = currentRange - (float)(targetStructure.Rect.Height / 2) * (1f / this.RangeMultiplierInWalls);
					int topNodeIndex = nodeIndex2;
					Vector2 topPos = targetStructure.SectionPosition(0, true);
					topPos.X += targetStructure.BodyWidth / 2f * (float)xDir;
					this.AddNodesBetweenPoints(targetPos3, topPos, 0.05f, ref topNodeIndex);
					this.nodes.Add(new ElectricalDischarger.Node(topPos, topNodeIndex, 0f, 0f));
					this.FindNodes(entitiesInRange, topPos, this.nodes.Count - 1, newRange2);
					int bottomNodeIndex = nodeIndex2;
					Vector2 bottomBos = targetStructure.SectionPosition(targetStructure.SectionCount - 1, true);
					bottomBos.X += targetStructure.BodyWidth / 2f * (float)xDir;
					this.AddNodesBetweenPoints(targetPos3, bottomBos, 0.05f, ref bottomNodeIndex);
					this.nodes.Add(new ElectricalDischarger.Node(bottomBos, bottomNodeIndex, 0f, 0f));
					this.FindNodes(entitiesInRange, bottomBos, this.nodes.Count - 1, newRange2);
				}
				for (int k = 0; k < entitiesInRange.Count; k++)
				{
					Entity otherEntity = entitiesInRange[k];
					Character character = otherEntity as Character;
					if (character != null && (!this.IgnoreUser || character != this.user) && (!this.OutdoorsOnly || character.Submarine == null))
					{
						Vector2 characterMin = new Vector2(character.AnimController.Limbs.Min((Limb l) => l.WorldPosition.X), character.AnimController.Limbs.Min((Limb l) => l.WorldPosition.Y));
						Vector2 characterMax = new Vector2(character.AnimController.Limbs.Max((Limb l) => l.WorldPosition.X), character.AnimController.Limbs.Max((Limb l) => l.WorldPosition.Y));
						if (targetStructure.IsHorizontal)
						{
							if (characterMax.X < (float)targetStructure.WorldRect.X || characterMin.X > (float)targetStructure.WorldRect.Right)
							{
								goto IL_93F;
							}
							if (Math.Abs(characterMin.Y - targetStructure.WorldPosition.Y) > currentRange && Math.Abs(characterMax.Y - targetStructure.WorldPosition.Y) > currentRange)
							{
								goto IL_93F;
							}
						}
						else if (characterMax.Y < (float)(targetStructure.WorldRect.Y - targetStructure.Rect.Height) || characterMin.Y > (float)targetStructure.WorldRect.Y || (Math.Abs(characterMin.X - targetStructure.WorldPosition.X) > currentRange && Math.Abs(characterMax.X - targetStructure.WorldPosition.X) > currentRange))
						{
							goto IL_93F;
						}
						if (!this.charactersInRange.Any(([TupleElementNames(new string[]
						{
							"character",
							"node"
						})] ValueTuple<Character, ElectricalDischarger.Node> c) => c.Item1 == character))
						{
							this.charactersInRange.Add(new ValueTuple<Character, ElectricalDischarger.Node>(character, this.nodes[parentNodeIndex]));
						}
						float closestNodeDistSqr = float.MaxValue;
						int closestNodeIndex = -1;
						for (int m = 0; m < this.nodes.Count; m++)
						{
							float distSqr = Vector2.DistanceSquared(character.WorldPosition, this.nodes[m].WorldPosition);
							if (distSqr < closestNodeDistSqr)
							{
								closestNodeDistSqr = distSqr;
								closestNodeIndex = m;
							}
						}
						if (closestNodeIndex > -1)
						{
							this.FindNodes(entitiesInRange, this.nodes[closestNodeIndex].WorldPosition, closestNodeIndex, currentRange - (float)Math.Sqrt((double)closestNodeDistSqr));
						}
					}
					IL_93F:;
				}
				return;
			}
			Entity entity = entitiesInRange[closestIndex];
			character = (entity as Character);
			if (character != null)
			{
				Vector2 targetPos4 = character.WorldPosition;
				this.AddNodesBetweenPoints(currPos, targetPos4, 0.25f, ref parentNodeIndex);
				this.nodes.Add(new ElectricalDischarger.Node(targetPos4, parentNodeIndex, 0f, 0f));
				entitiesInRange.RemoveAt(closestIndex);
				if (!this.charactersInRange.Any(([TupleElementNames(new string[]
				{
					"character",
					"node"
				})] ValueTuple<Character, ElectricalDischarger.Node> c) => c.Item1 == character))
				{
					this.charactersInRange.Add(new ValueTuple<Character, ElectricalDischarger.Node>(character, this.nodes[parentNodeIndex]));
				}
				this.FindNodes(entitiesInRange, targetPos4, this.nodes.Count - 1, currentRange);
			}
		}

		// Token: 0x06005808 RID: 22536 RVA: 0x002D9684 File Offset: 0x002D7884
		private void AddNodesBetweenPoints(Vector2 currPos, Vector2 targetPos, float variance, ref int parentNodeIndex)
		{
			Vector2 diff = targetPos - currPos;
			float dist = diff.Length();
			Vector2 normal = new Vector2(-diff.Y, diff.X) / dist;
			for (float x = 150f; x < dist - 150f; x += 150f * Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced))
			{
				float normalOffset = (0.5f - Math.Abs(x / dist - 0.5f)) * 2f;
				normalOffset *= variance * dist * Rand.Range(-1f, 1f, Rand.RandSync.Unsynced);
				this.nodes.Add(new ElectricalDischarger.Node(currPos + diff / dist * x + normal * normalOffset, parentNodeIndex, 0f, 0f));
				parentNodeIndex = this.nodes.Count - 1;
			}
		}

		// Token: 0x06005809 RID: 22537 RVA: 0x002D9770 File Offset: 0x002D7970
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if ((name == "activate" || name == "use" || name == "trigger_in") && signal.value != "0")
			{
				this.item.Use(1f, null, null, null, null);
			}
		}

		// Token: 0x0600580A RID: 22538 RVA: 0x002D97D1 File Offset: 0x002D79D1
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			ElectricalDischarger.list.Remove(this);
		}

		// Token: 0x0600580B RID: 22539 RVA: 0x002D97E5 File Offset: 0x002D79E5
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Character character = this.user;
			msg.WriteUInt16((character != null) ? character.ID : 0);
		}

		// Token: 0x0600580D RID: 22541 RVA: 0x002D980C File Offset: 0x002D7A0C
		[CompilerGenerated]
		internal static void <DischargeProjSpecific>g__CreateParticlesBetween|57_0(Vector2 start, Vector2 end)
		{
			Vector2 diff = end - start;
			float dist = diff.Length();
			Vector2 normalizedDiff = MathUtils.NearlyEqual(dist, 0f, 0.0001f) ? Vector2.Zero : (diff / dist);
			for (float x = 0f; x < dist; x += 50f)
			{
				Particle spark = GameMain.ParticleManager.CreateParticle("ElectricShock", start + normalizedDiff * x, Vector2.Zero, 0f, null, 0f, null);
				if (spark != null)
				{
					spark.Size *= 0.3f;
				}
			}
		}

		// Token: 0x04002CE4 RID: 11492
		private static SpriteSheet electricitySprite;

		// Token: 0x04002CE5 RID: 11493
		private int frameOffset;

		// Token: 0x04002CE6 RID: 11494
		private static readonly List<ElectricalDischarger> list = new List<ElectricalDischarger>();

		// Token: 0x04002CE7 RID: 11495
		private const int MaxNodes = 100;

		// Token: 0x04002CE8 RID: 11496
		private const float MaxNodeDistance = 150f;

		// Token: 0x04002CF0 RID: 11504
		private readonly List<ElectricalDischarger.Node> nodes = new List<ElectricalDischarger.Node>();

		// Token: 0x04002CF1 RID: 11505
		[TupleElementNames(new string[]
		{
			"character",
			"node"
		})]
		private readonly List<ValueTuple<Character, ElectricalDischarger.Node>> charactersInRange = new List<ValueTuple<Character, ElectricalDischarger.Node>>();

		// Token: 0x04002CF2 RID: 11506
		private bool charging;

		// Token: 0x04002CF3 RID: 11507
		private float timer;

		// Token: 0x04002CF4 RID: 11508
		private readonly Attack attack;

		// Token: 0x04002CF5 RID: 11509
		private Character user;

		// Token: 0x04002CF6 RID: 11510
		private float reloadTimer;

		// Token: 0x0200139F RID: 5023
		public struct Node
		{
			// Token: 0x060097E4 RID: 38884 RVA: 0x003DC6D0 File Offset: 0x003DA8D0
			public Node(Vector2 worldPosition, int parentIndex, float length = 0f, float angle = 0f)
			{
				this.WorldPosition = worldPosition;
				this.ParentIndex = parentIndex;
				this.Length = length;
				this.Angle = angle;
			}

			// Token: 0x04006301 RID: 25345
			public Vector2 WorldPosition;

			// Token: 0x04006302 RID: 25346
			public int ParentIndex;

			// Token: 0x04006303 RID: 25347
			public float Length;

			// Token: 0x04006304 RID: 25348
			public float Angle;
		}
	}
}
