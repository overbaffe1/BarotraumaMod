using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005DD RID: 1501
	internal class MotionSensor : ItemComponent, IDrawableComponent
	{
		// Token: 0x17001879 RID: 6265
		// (get) Token: 0x06006113 RID: 24851 RVA: 0x00328E7B File Offset: 0x0032707B
		public Vector2 DrawSize
		{
			get
			{
				return new Vector2(this.rangeX, this.rangeY) * 2f;
			}
		}

		// Token: 0x06006114 RID: 24852 RVA: 0x00328E98 File Offset: 0x00327098
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (!editing || !MapEntity.SelectedList.Contains(this.item))
			{
				if (!ConnectionPanel.ShouldDebugDrawWiring)
				{
					return;
				}
				Character controlled = Character.Controlled;
				if (((controlled != null) ? controlled.SelectedItem : null) != this.item)
				{
					return;
				}
			}
			Vector2 pos = this.item.WorldPosition + this.TransformedDetectOffset;
			pos.Y = -pos.Y;
			GUI.DrawRectangle(spriteBatch, pos - new Vector2(this.rangeX, this.rangeY), new Vector2(this.rangeX, this.rangeY) * 2f, Color.Cyan * 0.5f, false, 0f, 2f);
		}

		// Token: 0x1700187A RID: 6266
		// (get) Token: 0x06006115 RID: 24853 RVA: 0x00328F54 File Offset: 0x00327154
		// (set) Token: 0x06006116 RID: 24854 RVA: 0x00328F5C File Offset: 0x0032715C
		[Serialize(false, IsPropertySaveable.No, "Has the item currently detected movement. Intended to be used by StatusEffect conditionals (setting this value in XML has no effect).", "", false)]
		public bool MotionDetected { get; set; }

		// Token: 0x1700187B RID: 6267
		// (get) Token: 0x06006117 RID: 24855 RVA: 0x00328F65 File Offset: 0x00327165
		// (set) Token: 0x06006118 RID: 24856 RVA: 0x00328F70 File Offset: 0x00327170
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

		// Token: 0x1700187C RID: 6268
		// (get) Token: 0x06006119 RID: 24857 RVA: 0x00328FE1 File Offset: 0x003271E1
		// (set) Token: 0x0600611A RID: 24858 RVA: 0x00328FF3 File Offset: 0x003271F3
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

		// Token: 0x1700187D RID: 6269
		// (get) Token: 0x0600611B RID: 24859 RVA: 0x0032900B File Offset: 0x0032720B
		// (set) Token: 0x0600611C RID: 24860 RVA: 0x00329013 File Offset: 0x00327213
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the sensor ignore the bodies of dead characters?", "", true)]
		public bool IgnoreDead { get; set; }

		// Token: 0x1700187E RID: 6270
		// (get) Token: 0x0600611D RID: 24861 RVA: 0x0032901C File Offset: 0x0032721C
		// (set) Token: 0x0600611E RID: 24862 RVA: 0x00329024 File Offset: 0x00327224
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
				this.item.ResetCachedVisibleSize();
			}
		}

		// Token: 0x1700187F RID: 6271
		// (get) Token: 0x0600611F RID: 24863 RVA: 0x00329047 File Offset: 0x00327247
		// (set) Token: 0x06006120 RID: 24864 RVA: 0x0032904F File Offset: 0x0032724F
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

		// Token: 0x17001880 RID: 6272
		// (get) Token: 0x06006121 RID: 24865 RVA: 0x00329067 File Offset: 0x00327267
		// (set) Token: 0x06006122 RID: 24866 RVA: 0x00329070 File Offset: 0x00327270
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

		// Token: 0x17001881 RID: 6273
		// (get) Token: 0x06006123 RID: 24867 RVA: 0x003290CC File Offset: 0x003272CC
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

		// Token: 0x17001882 RID: 6274
		// (get) Token: 0x06006124 RID: 24868 RVA: 0x00329117 File Offset: 0x00327317
		// (set) Token: 0x06006125 RID: 24869 RVA: 0x0032911F File Offset: 0x0032731F
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 2)]
		[Serialize(0.1f, IsPropertySaveable.Yes, "How often the sensor checks if there's something moving near it. Higher values are better for performance.", "", true)]
		public float UpdateInterval { get; set; }

		// Token: 0x17001883 RID: 6275
		// (get) Token: 0x06006126 RID: 24870 RVA: 0x00329128 File Offset: 0x00327328
		// (set) Token: 0x06006127 RID: 24871 RVA: 0x00329130 File Offset: 0x00327330
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

		// Token: 0x17001884 RID: 6276
		// (get) Token: 0x06006128 RID: 24872 RVA: 0x0032913F File Offset: 0x0032733F
		// (set) Token: 0x06006129 RID: 24873 RVA: 0x00329148 File Offset: 0x00327348
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

		// Token: 0x17001885 RID: 6277
		// (get) Token: 0x0600612A RID: 24874 RVA: 0x003291AA File Offset: 0x003273AA
		// (set) Token: 0x0600612B RID: 24875 RVA: 0x003291B4 File Offset: 0x003273B4
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

		// Token: 0x17001886 RID: 6278
		// (get) Token: 0x0600612C RID: 24876 RVA: 0x00329216 File Offset: 0x00327416
		// (set) Token: 0x0600612D RID: 24877 RVA: 0x0032921E File Offset: 0x0032741E
		[InGameEditable(DecimalCount = 3)]
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the objects within the detector's range have to be moving (in m/s).", "", true)]
		public float MinimumVelocity { get; set; }

		// Token: 0x17001887 RID: 6279
		// (get) Token: 0x0600612E RID: 24878 RVA: 0x00329227 File Offset: 0x00327427
		// (set) Token: 0x0600612F RID: 24879 RVA: 0x0032922F File Offset: 0x0032742F
		[Serialize(true, IsPropertySaveable.Yes, "Should the sensor trigger when the item itself moves.", "", false)]
		public bool DetectOwnMotion { get; set; }

		// Token: 0x06006130 RID: 24880 RVA: 0x00329238 File Offset: 0x00327438
		public MotionSensor(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			if (element.GetAttribute("range") != null)
			{
				this.rangeX = (this.rangeY = element.GetAttributeFloat("range", 0f));
			}
			this.updateTimer = Rand.Range(0f, this.UpdateInterval, Rand.RandSync.Unsynced);
		}

		// Token: 0x06006131 RID: 24881 RVA: 0x003292AC File Offset: 0x003274AC
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			if (componentElement.GetAttributeBool("onlyhumans", false))
			{
				this.Target = MotionSensor.TargetType.Human;
			}
		}

		// Token: 0x06006132 RID: 24882 RVA: 0x003292D0 File Offset: 0x003274D0
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

		// Token: 0x06006133 RID: 24883 RVA: 0x003299A0 File Offset: 0x00327BA0
		public bool TriggersOn(Character character)
		{
			return (this.triggerFromHumans || this.triggerFromPets || this.triggerFromMonsters) && this.TriggersOn(character, this.triggerFromHumans, this.triggerFromPets, this.triggerFromMonsters);
		}

		// Token: 0x06006134 RID: 24884 RVA: 0x003299E8 File Offset: 0x00327BE8
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

		// Token: 0x06006135 RID: 24885 RVA: 0x00329AA8 File Offset: 0x00327CA8
		public override XElement Save(XElement parentElement)
		{
			Vector2 prevDetectOffset = this.detectOffset;
			XElement element = base.Save(parentElement);
			this.detectOffset = prevDetectOffset;
			return element;
		}

		// Token: 0x04003210 RID: 12816
		private float rangeX;

		// Token: 0x04003211 RID: 12817
		private float rangeY;

		// Token: 0x04003212 RID: 12818
		private Vector2 detectOffset;

		// Token: 0x04003213 RID: 12819
		private float updateTimer;

		// Token: 0x04003215 RID: 12821
		private bool triggerFromHumans = true;

		// Token: 0x04003216 RID: 12822
		private bool triggerFromPets = true;

		// Token: 0x04003217 RID: 12823
		private bool triggerFromMonsters = true;

		// Token: 0x04003218 RID: 12824
		private MotionSensor.TargetType _target;

		// Token: 0x04003219 RID: 12825
		private HashSet<Identifier> targetCharacters;

		// Token: 0x0400321C RID: 12828
		private int maxOutputLength;

		// Token: 0x0400321D RID: 12829
		private string output;

		// Token: 0x0400321E RID: 12830
		private string falseOutput;

		// Token: 0x02001473 RID: 5235
		[Flags]
		public enum TargetType
		{
			// Token: 0x040065CA RID: 26058
			Human = 1,
			// Token: 0x040065CB RID: 26059
			Monster = 2,
			// Token: 0x040065CC RID: 26060
			Wall = 4,
			// Token: 0x040065CD RID: 26061
			Pet = 8,
			// Token: 0x040065CE RID: 26062
			Any = 15
		}
	}
}
