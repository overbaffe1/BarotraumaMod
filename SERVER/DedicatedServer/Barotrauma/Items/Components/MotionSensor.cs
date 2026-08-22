using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F3 RID: 1267
	internal class MotionSensor : ItemComponent
	{
		// Token: 0x17001320 RID: 4896
		// (get) Token: 0x0600474A RID: 18250 RVA: 0x001C697A File Offset: 0x001C4B7A
		// (set) Token: 0x0600474B RID: 18251 RVA: 0x001C6982 File Offset: 0x001C4B82
		[Serialize(false, IsPropertySaveable.No, "Has the item currently detected movement. Intended to be used by StatusEffect conditionals (setting this value in XML has no effect).", "", false)]
		public bool MotionDetected { get; set; }

		// Token: 0x17001321 RID: 4897
		// (get) Token: 0x0600474C RID: 18252 RVA: 0x001C698B File Offset: 0x001C4B8B
		// (set) Token: 0x0600474D RID: 18253 RVA: 0x001C6994 File Offset: 0x001C4B94
		[InGameEditable]
		[Serialize(MotionSensor.TargetType.Any, IsPropertySaveable.Yes, "Which kind of targets can trigger the sensor?", "", true)]
		public MotionSensor.TargetType Target
		{
			get
			{
				return this._target;
			}
			set
			{
				if (this._target != value)
				{
					this._target = value;
					this.triggerFromHumans = this.Target.HasFlag(MotionSensor.TargetType.Human);
					this.triggerFromPets = this.Target.HasFlag(MotionSensor.TargetType.Pet);
					this.triggerFromMonsters = this.Target.HasFlag(MotionSensor.TargetType.Monster);
				}
			}
		}

		// Token: 0x17001322 RID: 4898
		// (get) Token: 0x0600474E RID: 18254 RVA: 0x001C6A05 File Offset: 0x001C4C05
		// (set) Token: 0x0600474F RID: 18255 RVA: 0x001C6A17 File Offset: 0x001C4C17
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Does the sensor react only to certain characters (species names, groups or tags)? Doesn't have an effect, if the Target Type is incorrect.", "", true)]
		public string TargetCharacters
		{
			get
			{
				return this.targetCharacters.ConvertToString(",");
			}
			set
			{
				this.targetCharacters = value.ToIdentifiers(",").ToHashSet<Identifier>();
			}
		}

		// Token: 0x17001323 RID: 4899
		// (get) Token: 0x06004750 RID: 18256 RVA: 0x001C6A2F File Offset: 0x001C4C2F
		// (set) Token: 0x06004751 RID: 18257 RVA: 0x001C6A37 File Offset: 0x001C4C37
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the sensor ignore the bodies of dead characters?", "", true)]
		public bool IgnoreDead { get; set; }

		// Token: 0x17001324 RID: 4900
		// (get) Token: 0x06004752 RID: 18258 RVA: 0x001C6A40 File Offset: 0x001C4C40
		// (set) Token: 0x06004753 RID: 18259 RVA: 0x001C6A48 File Offset: 0x001C4C48
		[InGameEditable]
		[Serialize(0f, IsPropertySaveable.Yes, "Horizontal detection range.", "", true)]
		public float RangeX
		{
			get
			{
				return this.rangeX;
			}
			set
			{
				this.rangeX = MathHelper.Clamp(value, 0f, 1000f);
			}
		}

		// Token: 0x17001325 RID: 4901
		// (get) Token: 0x06004754 RID: 18260 RVA: 0x001C6A60 File Offset: 0x001C4C60
		// (set) Token: 0x06004755 RID: 18261 RVA: 0x001C6A68 File Offset: 0x001C4C68
		[InGameEditable]
		[Serialize(0f, IsPropertySaveable.Yes, "Vertical movement detection range.", "", true)]
		public float RangeY
		{
			get
			{
				return this.rangeY;
			}
			set
			{
				this.rangeY = MathHelper.Clamp(value, 0f, 1000f);
			}
		}

		// Token: 0x17001326 RID: 4902
		// (get) Token: 0x06004756 RID: 18262 RVA: 0x001C6A80 File Offset: 0x001C4C80
		// (set) Token: 0x06004757 RID: 18263 RVA: 0x001C6A88 File Offset: 0x001C4C88
		[InGameEditable]
		[Serialize("0,0", IsPropertySaveable.Yes, "The position to detect the movement at relative to the item. For example, 0,100 would detect movement 100 units above the item.", "", false)]
		public Vector2 DetectOffset
		{
			get
			{
				return this.detectOffset;
			}
			set
			{
				this.detectOffset = value;
				this.detectOffset.X = MathHelper.Clamp(value.X, -this.rangeX, this.rangeX);
				this.detectOffset.Y = MathHelper.Clamp(value.Y, -this.rangeY, this.rangeY);
			}
		}

		// Token: 0x17001327 RID: 4903
		// (get) Token: 0x06004758 RID: 18264 RVA: 0x001C6AE4 File Offset: 0x001C4CE4
		public Vector2 TransformedDetectOffset
		{
			get
			{
				Vector2 transformedDetectOffset = this.detectOffset;
				if (this.item.FlippedX)
				{
					transformedDetectOffset.X = -transformedDetectOffset.X;
				}
				if (this.item.FlippedY)
				{
					transformedDetectOffset.Y = -transformedDetectOffset.Y;
				}
				return transformedDetectOffset;
			}
		}

		// Token: 0x17001328 RID: 4904
		// (get) Token: 0x06004759 RID: 18265 RVA: 0x001C6B2F File Offset: 0x001C4D2F
		// (set) Token: 0x0600475A RID: 18266 RVA: 0x001C6B37 File Offset: 0x001C4D37
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 2)]
		[Serialize(0.1f, IsPropertySaveable.Yes, "How often the sensor checks if there's something moving near it. Higher values are better for performance.", "", true)]
		public float UpdateInterval { get; set; }

		// Token: 0x17001329 RID: 4905
		// (get) Token: 0x0600475B RID: 18267 RVA: 0x001C6B40 File Offset: 0x001C4D40
		// (set) Token: 0x0600475C RID: 18268 RVA: 0x001C6B48 File Offset: 0x001C4D48
		[Editable]
		[Serialize(200, IsPropertySaveable.No, "The maximum length of the output strings. Warning: Large values can lead to large memory usage or networking issues.", "", false)]
		public int MaxOutputLength
		{
			get
			{
				return this.maxOutputLength;
			}
			set
			{
				this.maxOutputLength = Math.Max(value, 0);
			}
		}

		// Token: 0x1700132A RID: 4906
		// (get) Token: 0x0600475D RID: 18269 RVA: 0x001C6B57 File Offset: 0x001C4D57
		// (set) Token: 0x0600475E RID: 18270 RVA: 0x001C6B60 File Offset: 0x001C4D60
		[InGameEditable]
		[Serialize("1", IsPropertySaveable.Yes, "The signal the item outputs when it has detected movement.", "", true)]
		public string Output
		{
			get
			{
				return this.output;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.output = value;
				if (this.output.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.output = this.output.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x1700132B RID: 4907
		// (get) Token: 0x0600475F RID: 18271 RVA: 0x001C6BC2 File Offset: 0x001C4DC2
		// (set) Token: 0x06004760 RID: 18272 RVA: 0x001C6BCC File Offset: 0x001C4DCC
		[InGameEditable]
		[Serialize("0", IsPropertySaveable.Yes, "The signal the item outputs when it has not detected movement.", "", true)]
		public string FalseOutput
		{
			get
			{
				return this.falseOutput;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.falseOutput = value;
				if (this.falseOutput.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.falseOutput = this.falseOutput.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x1700132C RID: 4908
		// (get) Token: 0x06004761 RID: 18273 RVA: 0x001C6C2E File Offset: 0x001C4E2E
		// (set) Token: 0x06004762 RID: 18274 RVA: 0x001C6C36 File Offset: 0x001C4E36
		[InGameEditable(DecimalCount = 3)]
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the objects within the detector's range have to be moving (in m/s).", "", true)]
		public float MinimumVelocity { get; set; }

		// Token: 0x1700132D RID: 4909
		// (get) Token: 0x06004763 RID: 18275 RVA: 0x001C6C3F File Offset: 0x001C4E3F
		// (set) Token: 0x06004764 RID: 18276 RVA: 0x001C6C47 File Offset: 0x001C4E47
		[Serialize(true, IsPropertySaveable.Yes, "Should the sensor trigger when the item itself moves.", "", false)]
		public bool DetectOwnMotion { get; set; }

		// Token: 0x06004765 RID: 18277 RVA: 0x001C6C50 File Offset: 0x001C4E50
		public MotionSensor(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			if (element.GetAttribute("range") != null)
			{
				this.rangeX = (this.rangeY = element.GetAttributeFloat("range", 0f));
			}
			this.updateTimer = Rand.Range(0f, this.UpdateInterval, Rand.RandSync.Unsynced);
		}

		// Token: 0x06004766 RID: 18278 RVA: 0x001C6CC4 File Offset: 0x001C4EC4
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			if (componentElement.GetAttributeBool("onlyhumans", false))
			{
				this.Target = MotionSensor.TargetType.Human;
			}
		}

		// Token: 0x06004767 RID: 18279 RVA: 0x001C6CE8 File Offset: 0x001C4EE8
		public override void Update(float deltaTime, Camera cam)
		{
			string signalOut = this.MotionDetected ? this.Output : this.FalseOutput;
			if (!string.IsNullOrEmpty(signalOut))
			{
				this.item.SendSignal(new Signal(signalOut, 1, null, null, 0f, 1f), "state_out");
			}
			if (this.MotionDetected)
			{
				base.ApplyStatusEffects(ActionType.OnUse, deltaTime, null, null, null, null, null, 1f);
			}
			this.updateTimer -= deltaTime;
			if (this.updateTimer > 0f)
			{
				return;
			}
			this.MotionDetected = false;
			this.updateTimer = this.UpdateInterval;
			if (this.item.body != null && this.item.body.Enabled && this.DetectOwnMotion && (Math.Abs(this.item.body.LinearVelocity.X) > this.MinimumVelocity || Math.Abs(this.item.body.LinearVelocity.Y) > this.MinimumVelocity))
			{
				this.MotionDetected = true;
				return;
			}
			Vector2 detectPos = this.item.WorldPosition + this.TransformedDetectOffset;
			Rectangle detectRect = new Rectangle((int)(detectPos.X - this.rangeX), (int)(detectPos.Y - this.rangeY), (int)(this.rangeX * 2f), (int)(this.rangeY * 2f));
			float broadRangeX = Math.Max(this.rangeX * 2f, 500f);
			float broadRangeY = Math.Max(this.rangeY * 2f, 500f);
			if (this.item.CurrentHull == null && this.item.Submarine != null && this.Target.HasFlag(MotionSensor.TargetType.Wall))
			{
				if (Level.Loaded != null && (Math.Abs(this.item.Submarine.Velocity.X) > this.MinimumVelocity || Math.Abs(this.item.Submarine.Velocity.Y) > this.MinimumVelocity))
				{
					List<VoronoiCell> cells = Level.Loaded.GetCells(this.item.WorldPosition, 1);
					foreach (VoronoiCell cell in cells)
					{
						if (cell.IsPointInside(this.item.WorldPosition))
						{
							this.MotionDetected = true;
							return;
						}
						foreach (GraphEdge edge in cell.Edges)
						{
							Vector2 e = edge.Point1 + cell.Translation;
							Vector2 e2 = edge.Point2 + cell.Translation;
							if (MathUtils.LineSegmentsIntersect(e, e2, new Vector2((float)detectRect.X, (float)detectRect.Y), new Vector2((float)detectRect.Right, (float)detectRect.Y)) || MathUtils.LineSegmentsIntersect(e, e2, new Vector2((float)detectRect.X, (float)detectRect.Bottom), new Vector2((float)detectRect.Right, (float)detectRect.Bottom)) || MathUtils.LineSegmentsIntersect(e, e2, new Vector2((float)detectRect.X, (float)detectRect.Y), new Vector2((float)detectRect.X, (float)detectRect.Bottom)) || MathUtils.LineSegmentsIntersect(e, e2, new Vector2((float)detectRect.Right, (float)detectRect.Y), new Vector2((float)detectRect.Right, (float)detectRect.Bottom)))
							{
								this.MotionDetected = true;
								return;
							}
						}
					}
				}
				foreach (Submarine sub in Submarine.Loaded)
				{
					if (sub != this.item.Submarine)
					{
						Vector2 relativeVelocity = this.item.Submarine.Velocity - sub.Velocity;
						if (Math.Abs(relativeVelocity.X) >= this.MinimumVelocity || Math.Abs(relativeVelocity.Y) >= this.MinimumVelocity)
						{
							Rectangle worldBorders = new Rectangle(sub.Borders.X + (int)sub.WorldPosition.X, sub.Borders.Y + (int)sub.WorldPosition.Y - sub.Borders.Height, sub.Borders.Width, sub.Borders.Height);
							if (worldBorders.Intersects(detectRect))
							{
								foreach (Structure wall in Structure.WallList)
								{
									if (wall.Submarine == sub && wall.WorldRect.Intersects(detectRect))
									{
										this.MotionDetected = true;
										return;
									}
								}
							}
						}
					}
				}
			}
			if (!this.triggerFromHumans && !this.triggerFromPets && !this.triggerFromMonsters)
			{
				return;
			}
			foreach (Character character in Character.CharacterList)
			{
				if (character.SpawnTime <= Timing.TotalTime - 1.0 && this.TriggersOn(character) && Math.Abs(character.WorldPosition.X - detectPos.X) <= broadRangeX && Math.Abs(character.WorldPosition.Y - detectPos.Y) <= broadRangeY)
				{
					foreach (Limb limb in character.AnimController.Limbs)
					{
						if (!limb.IsSevered && limb.LinearVelocity.LengthSquared() >= this.MinimumVelocity * this.MinimumVelocity && MathUtils.CircleIntersectsRectangle(limb.WorldPosition, ConvertUnits.ToDisplayUnits(limb.body.GetMaxExtent()), detectRect))
						{
							this.MotionDetected = true;
							return;
						}
					}
				}
			}
		}

		// Token: 0x06004768 RID: 18280 RVA: 0x001C73B8 File Offset: 0x001C55B8
		public bool TriggersOn(Character character)
		{
			return (this.triggerFromHumans || this.triggerFromPets || this.triggerFromMonsters) && this.TriggersOn(character, this.triggerFromHumans, this.triggerFromPets, this.triggerFromMonsters);
		}

		// Token: 0x06004769 RID: 18281 RVA: 0x001C7400 File Offset: 0x001C5600
		private bool TriggersOn(Character character, bool triggerFromHumans, bool triggerFromPets, bool triggerFromMonsters)
		{
			if (this.IgnoreDead && character.IsDead)
			{
				return false;
			}
			if (character.IsPet)
			{
				if (!triggerFromPets)
				{
					return false;
				}
			}
			else if (character.IsHuman || CharacterParams.CompareGroup(character.Group, CharacterPrefab.HumanGroup))
			{
				if (!triggerFromHumans)
				{
					return false;
				}
			}
			else if (!triggerFromMonsters)
			{
				return false;
			}
			if (this.targetCharacters.Any<Identifier>())
			{
				bool matchFound = false;
				foreach (Identifier target in this.targetCharacters)
				{
					if (character.MatchesSpeciesNameOrGroup(target) || character.Params.HasTag(target))
					{
						matchFound = true;
						break;
					}
				}
				if (!matchFound)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600476A RID: 18282 RVA: 0x001C74C0 File Offset: 0x001C56C0
		public override XElement Save(XElement parentElement)
		{
			Vector2 prevDetectOffset = this.detectOffset;
			XElement element = base.Save(parentElement);
			this.detectOffset = prevDetectOffset;
			return element;
		}

		// Token: 0x04002267 RID: 8807
		private float rangeX;

		// Token: 0x04002268 RID: 8808
		private float rangeY;

		// Token: 0x04002269 RID: 8809
		private Vector2 detectOffset;

		// Token: 0x0400226A RID: 8810
		private float updateTimer;

		// Token: 0x0400226C RID: 8812
		private bool triggerFromHumans = true;

		// Token: 0x0400226D RID: 8813
		private bool triggerFromPets = true;

		// Token: 0x0400226E RID: 8814
		private bool triggerFromMonsters = true;

		// Token: 0x0400226F RID: 8815
		private MotionSensor.TargetType _target;

		// Token: 0x04002270 RID: 8816
		private HashSet<Identifier> targetCharacters;

		// Token: 0x04002273 RID: 8819
		private int maxOutputLength;

		// Token: 0x04002274 RID: 8820
		private string output;

		// Token: 0x04002275 RID: 8821
		private string falseOutput;

		// Token: 0x02000E35 RID: 3637
		[Flags]
		public enum TargetType
		{
			// Token: 0x0400421D RID: 16925
			Human = 1,
			// Token: 0x0400421E RID: 16926
			Monster = 2,
			// Token: 0x0400421F RID: 16927
			Wall = 4,
			// Token: 0x04004220 RID: 16928
			Pet = 8,
			// Token: 0x04004221 RID: 16929
			Any = 15
		}
	}
}
