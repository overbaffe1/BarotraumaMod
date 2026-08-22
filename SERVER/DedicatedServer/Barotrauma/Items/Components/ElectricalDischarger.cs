using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004B1 RID: 1201
	internal class ElectricalDischarger : Powered, IServerSerializable, INetSerializable
	{
		// Token: 0x17001227 RID: 4647
		// (get) Token: 0x06004436 RID: 17462 RVA: 0x001B53B3 File Offset: 0x001B35B3
		public static IEnumerable<ElectricalDischarger> List
		{
			get
			{
				return ElectricalDischarger.list;
			}
		}

		// Token: 0x17001228 RID: 4648
		// (get) Token: 0x06004437 RID: 17463 RVA: 0x001B53BA File Offset: 0x001B35BA
		// (set) Token: 0x06004438 RID: 17464 RVA: 0x001B53C2 File Offset: 0x001B35C2
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

		// Token: 0x17001229 RID: 4649
		// (get) Token: 0x06004439 RID: 17465 RVA: 0x001B53E4 File Offset: 0x001B35E4
		// (set) Token: 0x0600443A RID: 17466 RVA: 0x001B53EC File Offset: 0x001B35EC
		[Serialize(500f, IsPropertySaveable.Yes, "How far the discharge can travel from the item.", "", true)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 5000f)]
		public float Range { get; set; }

		// Token: 0x1700122A RID: 4650
		// (get) Token: 0x0600443B RID: 17467 RVA: 0x001B53F5 File Offset: 0x001B35F5
		// (set) Token: 0x0600443C RID: 17468 RVA: 0x001B53FD File Offset: 0x001B35FD
		[Serialize(25f, IsPropertySaveable.Yes, "How much further can the discharge be carried when moving across walls.", "", true)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float RangeMultiplierInWalls { get; set; }

		// Token: 0x1700122B RID: 4651
		// (get) Token: 0x0600443D RID: 17469 RVA: 0x001B5406 File Offset: 0x001B3606
		// (set) Token: 0x0600443E RID: 17470 RVA: 0x001B540E File Offset: 0x001B360E
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float RaycastRange { get; set; }

		// Token: 0x1700122C RID: 4652
		// (get) Token: 0x0600443F RID: 17471 RVA: 0x001B5417 File Offset: 0x001B3617
		// (set) Token: 0x06004440 RID: 17472 RVA: 0x001B541F File Offset: 0x001B361F
		[Serialize(0.25f, IsPropertySaveable.Yes, "The duration of an individual discharge (in seconds).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 60f, ValueStep = 0.1f, DecimalCount = 2)]
		public float Duration { get; set; }

		// Token: 0x1700122D RID: 4653
		// (get) Token: 0x06004441 RID: 17473 RVA: 0x001B5428 File Offset: 0x001B3628
		// (set) Token: 0x06004442 RID: 17474 RVA: 0x001B5430 File Offset: 0x001B3630
		[Serialize(0.25f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 60f, ValueStep = 0.1f, DecimalCount = 2)]
		public float Reload { get; set; }

		// Token: 0x1700122E RID: 4654
		// (get) Token: 0x06004443 RID: 17475 RVA: 0x001B5439 File Offset: 0x001B3639
		// (set) Token: 0x06004444 RID: 17476 RVA: 0x001B5441 File Offset: 0x001B3641
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, the discharge cannot travel inside the submarine nor shock anyone inside.", "", false)]
		[Editable]
		public bool OutdoorsOnly { get; set; }

		// Token: 0x1700122F RID: 4655
		// (get) Token: 0x06004445 RID: 17477 RVA: 0x001B544A File Offset: 0x001B364A
		// (set) Token: 0x06004446 RID: 17478 RVA: 0x001B5452 File Offset: 0x001B3652
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool IgnoreUser { get; set; }

		// Token: 0x17001230 RID: 4656
		// (get) Token: 0x06004447 RID: 17479 RVA: 0x001B545B File Offset: 0x001B365B
		public IEnumerable<ElectricalDischarger.Node> Nodes
		{
			get
			{
				return this.nodes;
			}
		}

		// Token: 0x06004448 RID: 17480 RVA: 0x001B5464 File Offset: 0x001B3664
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
		}

		// Token: 0x06004449 RID: 17481 RVA: 0x001B5504 File Offset: 0x001B3704
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
			if (GameMain.Server != null)
			{
				this.item.CreateServerEvent<ElectricalDischarger>(this);
			}
			return false;
		}

		// Token: 0x0600444A RID: 17482 RVA: 0x001B5574 File Offset: 0x001B3774
		public override void Update(float deltaTime, Camera cam)
		{
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
								if (GameMain.Server != null)
								{
									battery.Item.CreateServerEvent<PowerContainer>(battery);
								}
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

		// Token: 0x0600444B RID: 17483 RVA: 0x001B56EC File Offset: 0x001B38EC
		public override float GetCurrentPowerConsumption(Connection conn = null)
		{
			return 0f;
		}

		// Token: 0x0600444C RID: 17484 RVA: 0x001B56F3 File Offset: 0x001B38F3
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.nodes.Clear();
			this.charactersInRange.Clear();
		}

		// Token: 0x0600444D RID: 17485 RVA: 0x001B5714 File Offset: 0x001B3914
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
			this.charging = false;
		}

		// Token: 0x0600444E RID: 17486 RVA: 0x001B5810 File Offset: 0x001B3A10
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

		// Token: 0x0600444F RID: 17487 RVA: 0x001B5E40 File Offset: 0x001B4040
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

		// Token: 0x06004450 RID: 17488 RVA: 0x001B6864 File Offset: 0x001B4A64
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

		// Token: 0x06004451 RID: 17489 RVA: 0x001B6950 File Offset: 0x001B4B50
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if ((name == "activate" || name == "use" || name == "trigger_in") && signal.value != "0")
			{
				this.item.Use(1f, null, null, null, null);
			}
		}

		// Token: 0x06004452 RID: 17490 RVA: 0x001B69B1 File Offset: 0x001B4BB1
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			ElectricalDischarger.list.Remove(this);
		}

		// Token: 0x06004453 RID: 17491 RVA: 0x001B69C5 File Offset: 0x001B4BC5
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Character character = this.user;
			msg.WriteUInt16((character != null) ? character.ID : 0);
		}

		// Token: 0x04002098 RID: 8344
		private static readonly List<ElectricalDischarger> list = new List<ElectricalDischarger>();

		// Token: 0x04002099 RID: 8345
		private const int MaxNodes = 100;

		// Token: 0x0400209A RID: 8346
		private const float MaxNodeDistance = 150f;

		// Token: 0x040020A2 RID: 8354
		private readonly List<ElectricalDischarger.Node> nodes = new List<ElectricalDischarger.Node>();

		// Token: 0x040020A3 RID: 8355
		[TupleElementNames(new string[]
		{
			"character",
			"node"
		})]
		private readonly List<ValueTuple<Character, ElectricalDischarger.Node>> charactersInRange = new List<ValueTuple<Character, ElectricalDischarger.Node>>();

		// Token: 0x040020A4 RID: 8356
		private bool charging;

		// Token: 0x040020A5 RID: 8357
		private float timer;

		// Token: 0x040020A6 RID: 8358
		private readonly Attack attack;

		// Token: 0x040020A7 RID: 8359
		private Character user;

		// Token: 0x040020A8 RID: 8360
		private float reloadTimer;

		// Token: 0x02000DFB RID: 3579
		public struct Node
		{
			// Token: 0x06006901 RID: 26881 RVA: 0x00223DD5 File Offset: 0x00221FD5
			public Node(Vector2 worldPosition, int parentIndex, float length = 0f, float angle = 0f)
			{
				this.WorldPosition = worldPosition;
				this.ParentIndex = parentIndex;
				this.Length = length;
				this.Angle = angle;
			}

			// Token: 0x0400415C RID: 16732
			public Vector2 WorldPosition;

			// Token: 0x0400415D RID: 16733
			public int ParentIndex;

			// Token: 0x0400415E RID: 16734
			public float Length;

			// Token: 0x0400415F RID: 16735
			public float Angle;
		}
	}
}
