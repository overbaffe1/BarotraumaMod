using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Particles;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B0 RID: 1456
	internal class Sprayer : RangedWeapon, IDrawableComponent
	{
		// Token: 0x17001674 RID: 5748
		// (get) Token: 0x0600598B RID: 22923 RVA: 0x002E1224 File Offset: 0x002DF424
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x0600598C RID: 22924 RVA: 0x002E122C File Offset: 0x002DF42C
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (character == null || !character.IsKeyDown(InputType.Aim))
			{
				return;
			}
			if (PlayerInput.KeyHit(InputType.PreviousFireMode))
			{
				if (this.spraySetting > 0)
				{
					this.spraySetting--;
				}
				else
				{
					this.spraySetting = 2;
				}
				this.targetSections.Clear();
			}
			if (PlayerInput.KeyHit(InputType.NextFireMode))
			{
				if (this.spraySetting < 2)
				{
					this.spraySetting++;
				}
				else
				{
					this.spraySetting = 0;
				}
				this.targetSections.Clear();
			}
			this.crosshairPointerPos = PlayerInput.MousePosition;
			Vector2 sourcePos = (((character != null) ? character.AnimController : null) == null) ? this.item.SimPosition : character.AnimController.AimSourceSimPos;
			Vector2 barrelPos = this.item.SimPosition + base.TransformedBarrelPos;
			if (Submarine.PickBody(sourcePos, barrelPos, null, new Category?(Category.Cat1 | Category.Cat5 | Category.Cat6), true, null, false) != null)
			{
				this.targetHull = null;
				this.targetSections.Clear();
				return;
			}
			Vector2 rayStart = ConvertUnits.ToSimUnits(this.item.WorldPosition) + base.TransformedBarrelPos;
			Vector2 pos = character.CursorWorldPosition;
			Vector2 rayEnd = ConvertUnits.ToSimUnits(pos);
			this.rayStartWorldPosition = ConvertUnits.ToDisplayUnits(rayStart);
			if (Vector2.Distance(this.rayStartWorldPosition, pos) > this.Range)
			{
				this.targetHull = null;
				this.targetSections.Clear();
				return;
			}
			Submarine parentSub = character.Submarine ?? this.item.Submarine;
			if (parentSub != null)
			{
				rayStart -= parentSub.SimPosition;
				rayEnd -= parentSub.SimPosition;
			}
			IEnumerable<Body> obstacles = Submarine.PickBodies(rayStart, rayEnd, null, new Category?(Category.Cat1 | Category.Cat5 | Category.Cat6), true, null, false);
			foreach (Body body in obstacles)
			{
				Item item = body.UserData as Item;
				if (item != null)
				{
					Door door = item.GetComponent<Door>();
					if (door != null && (door.IsOpen || door.IsBroken))
					{
						continue;
					}
				}
				this.targetHull = null;
				this.targetSections.Clear();
				return;
			}
			this.targetHull = Hull.GetCleanTarget(pos);
			if (this.targetHull == null)
			{
				this.targetSections.Clear();
				return;
			}
			BackgroundSection mousedOverSection = this.targetHull.GetBackgroundSection(pos);
			if (mousedOverSection == null)
			{
				this.targetSections.Clear();
				return;
			}
			if (this.targetSections.Count > 0 && mousedOverSection == this.targetSections[0])
			{
				return;
			}
			this.targetSections.Clear();
			this.targetSections.Add(mousedOverSection);
			int mousedOverIndex = (int)mousedOverSection.Index;
			if (this.spraySetting > 0)
			{
				this.sprayArray[0].X = mousedOverIndex + 1;
				this.sprayArray[0].Y = (int)mousedOverSection.RowIndex;
				this.sprayArray[1].X = mousedOverIndex + this.targetHull.xBackgroundMax;
				this.sprayArray[1].Y = (int)(mousedOverSection.RowIndex + 1);
				this.sprayArray[2].X = this.sprayArray[1].X + 1;
				this.sprayArray[2].Y = this.sprayArray[1].Y;
				for (int i = 0; i < 3; i++)
				{
					if (this.targetHull.DoesSectionMatch(this.sprayArray[i].X, this.sprayArray[i].Y))
					{
						this.targetSections.Add(this.targetHull.BackgroundSections[this.sprayArray[i].X]);
					}
				}
				if (this.spraySetting == 2)
				{
					this.sprayArray[3].X = mousedOverIndex - 1;
					this.sprayArray[3].Y = (int)mousedOverSection.RowIndex;
					this.sprayArray[4].X = this.sprayArray[1].X - 1;
					this.sprayArray[4].Y = this.sprayArray[1].Y;
					this.sprayArray[5].X = this.sprayArray[3].X - this.targetHull.xBackgroundMax;
					this.sprayArray[5].Y = this.sprayArray[3].Y - 1;
					this.sprayArray[6].X = this.sprayArray[5].X + 1;
					this.sprayArray[6].Y = this.sprayArray[5].Y;
					this.sprayArray[7].X = this.sprayArray[6].X + 1;
					this.sprayArray[7].Y = this.sprayArray[6].Y;
					for (int j = 3; j < this.sprayArray.Length; j++)
					{
						if (this.targetHull.DoesSectionMatch(this.sprayArray[j].X, this.sprayArray[j].Y))
						{
							this.targetSections.Add(this.targetHull.BackgroundSections[this.sprayArray[j].X]);
						}
					}
				}
			}
		}

		// Token: 0x0600598D RID: 22925 RVA: 0x002E17C8 File Offset: 0x002DF9C8
		public override void DrawHUD(SpriteBatch spriteBatch, Character character)
		{
			if (character == null || !character.IsKeyDown(InputType.Aim))
			{
				return;
			}
			base.DrawHUD(spriteBatch, character);
			GUI.HideCursor = (this.targetSections.Count > 0);
		}

		// Token: 0x0600598E RID: 22926 RVA: 0x002E17F2 File Offset: 0x002DF9F2
		public override bool Use(float deltaTime, Character character = null)
		{
			if (character == null)
			{
				return false;
			}
			if (character == Character.Controlled)
			{
				this.Spray(character, deltaTime, this.targetSections.Count > 0);
				return true;
			}
			this.Spray(character, deltaTime, false);
			return true;
		}

		// Token: 0x0600598F RID: 22927 RVA: 0x002E1824 File Offset: 0x002DFA24
		public void Spray(Character user, float deltaTime, bool applyColors)
		{
			ItemContainer liquidContainer = this.LiquidContainer;
			Item liquidItem = (liquidContainer != null) ? liquidContainer.Inventory.FirstOrDefault() : null;
			if (liquidItem == null)
			{
				return;
			}
			bool isCleaning = false;
			this.LiquidColors.TryGetValue(liquidItem.Prefab.Identifier, out this.color);
			if (applyColors && this.targetSections.Any<BackgroundSection>())
			{
				if (this.color.A == 0)
				{
					isCleaning = true;
				}
				float sizeAdjustedSprayStrength = this.SprayStrength / (float)this.targetSections.Count;
				if (!isCleaning)
				{
					for (int i = 0; i < this.targetSections.Count; i++)
					{
						this.targetHull.IncreaseSectionColorOrStrength(this.targetSections[i], new Color?(this.color), new float?(sizeAdjustedSprayStrength * deltaTime), true, false);
					}
					if (GameMain.GameSession != null)
					{
						GameMain.GameSession.TimeSpentCleaning += (double)deltaTime;
					}
				}
				else
				{
					for (int j = 0; j < this.targetSections.Count; j++)
					{
						this.targetHull.CleanSection(this.targetSections[j], -sizeAdjustedSprayStrength * deltaTime, true);
					}
					if (GameMain.GameSession != null)
					{
						GameMain.GameSession.TimeSpentPainting += (double)deltaTime;
					}
				}
			}
			Vector2 particleStartPos = this.item.WorldPosition + ConvertUnits.ToDisplayUnits(base.TransformedBarrelPos);
			Vector2 particleEndPos = user.CursorWorldPosition;
			float dist = Math.Min(Vector2.Distance(particleStartPos, particleEndPos), this.Range * 0.5f);
			foreach (ParticleEmitter particleEmitter in this.particleEmitters)
			{
				float particleAngle = this.item.body.Rotation + ((this.item.body.Dir > 0f) ? 0f : 3.1415927f);
				float particleRange = particleEmitter.Prefab.Properties.VelocityMax * particleEmitter.Prefab.ParticlePrefab.LifeTime;
				particleEmitter.Emit(deltaTime, particleStartPos, this.item.CurrentHull, particleAngle, particleEmitter.Prefab.Properties.CopyEntityAngle ? (-particleAngle) : 0f, dist / particleRange * 1.5f, 1f, 1f, new Color?(new Color(this.color.R, this.color.G, this.color.B, byte.MaxValue)), null, false, null);
			}
		}

		// Token: 0x06005990 RID: 22928 RVA: 0x002E1ABC File Offset: 0x002DFCBC
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (Character.Controlled == null || !Character.Controlled.HasEquippedItem(this.item, null, null) || !Character.Controlled.IsKeyDown(InputType.Aim) || this.targetHull == null || this.targetSections.Count == 0)
			{
				return;
			}
			Vector2 drawOffset = (this.targetHull.Submarine == null) ? Vector2.Zero : this.targetHull.Submarine.DrawPosition;
			Point sectionSize = this.targetSections[0].Rect.Size;
			Rectangle drawPositionRect = new Rectangle((int)(drawOffset.X + (float)this.targetHull.Rect.X), (int)(drawOffset.Y + (float)this.targetHull.Rect.Y), sectionSize.X, sectionSize.Y);
			if (this.crosshairSprite == null && this.crosshairPointerSprite == null)
			{
				for (int i = 0; i < this.targetSections.Count; i++)
				{
					GUI.DrawRectangle(spriteBatch, new Vector2((float)(drawPositionRect.X + this.targetSections[i].Rect.X), (float)(-(float)(drawPositionRect.Y + this.targetSections[i].Rect.Y))), new Vector2((float)sectionSize.X, (float)sectionSize.Y), Color.White, false, 0f, 1f);
				}
				return;
			}
			if (this.targetSections.Count > 0)
			{
				Vector2 drawPos = Vector2.Zero;
				for (int j = 0; j < this.targetSections.Count; j++)
				{
					drawPos += new Vector2((float)(drawPositionRect.X + this.targetSections[j].Rect.X + sectionSize.X / 2), (float)(-(float)(drawPositionRect.Y + this.targetSections[j].Rect.Y - sectionSize.Y / 2)));
				}
				drawPos /= (float)this.targetSections.Count;
				Sprite crosshairSprite = this.crosshairSprite;
				if (crosshairSprite != null)
				{
					crosshairSprite.Draw(spriteBatch, drawPos, 0f, (float)(sectionSize.X * 3) / this.crosshairSprite.size.X, SpriteEffects.None);
				}
				Sprite crosshairPointerSprite = this.crosshairPointerSprite;
				if (crosshairPointerSprite == null)
				{
					return;
				}
				crosshairPointerSprite.Draw(spriteBatch, drawPos, 0f, (float)(sectionSize.X * (this.spraySetting + 1)) / this.crosshairPointerSprite.size.X, SpriteEffects.None);
			}
		}

		// Token: 0x17001675 RID: 5749
		// (get) Token: 0x06005991 RID: 22929 RVA: 0x002E1D45 File Offset: 0x002DFF45
		// (set) Token: 0x06005992 RID: 22930 RVA: 0x002E1D4D File Offset: 0x002DFF4D
		[Serialize(0f, IsPropertySaveable.No, "The distance at which the item can spray walls.", "", false)]
		public float Range { get; set; }

		// Token: 0x17001676 RID: 5750
		// (get) Token: 0x06005993 RID: 22931 RVA: 0x002E1D56 File Offset: 0x002DFF56
		// (set) Token: 0x06005994 RID: 22932 RVA: 0x002E1D5E File Offset: 0x002DFF5E
		[Serialize(1f, IsPropertySaveable.No, "How fast the item changes the color of the walls.", "", false)]
		public float SprayStrength { get; set; }

		// Token: 0x17001677 RID: 5751
		// (get) Token: 0x06005995 RID: 22933 RVA: 0x002E1D67 File Offset: 0x002DFF67
		// (set) Token: 0x06005996 RID: 22934 RVA: 0x002E1D6F File Offset: 0x002DFF6F
		public ItemContainer LiquidContainer { get; private set; }

		// Token: 0x06005997 RID: 22935 RVA: 0x002E1D78 File Offset: 0x002DFF78
		public Sprayer(Item item, ContentXElement element) : base(item, element)
		{
			item.IsShootable = true;
			item.RequireAimToUse = true;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "paintcolors")
				{
					Dictionary<Identifier, Color> liquidColors = new Dictionary<Identifier, Color>();
					foreach (ContentXElement cxe in subElement.Elements())
					{
						XElement paintElement = cxe;
						Identifier paintName = paintElement.GetAttributeIdentifier("paintitem", Identifier.Empty);
						Color paintColor = paintElement.GetAttributeColor("color", Color.Transparent);
						if (paintName != string.Empty)
						{
							liquidColors.Add(paintName, paintColor);
						}
					}
					this.LiquidColors = liquidColors.ToImmutableDictionary<Identifier, Color>();
				}
			}
			this.InitProjSpecific(element);
		}

		// Token: 0x06005998 RID: 22936 RVA: 0x002E1EB4 File Offset: 0x002E00B4
		public override void OnItemLoaded()
		{
			this.LiquidContainer = this.item.GetComponent<ItemContainer>();
		}

		// Token: 0x06005999 RID: 22937 RVA: 0x002E1EC8 File Offset: 0x002E00C8
		private void InitProjSpecific(ContentXElement element)
		{
			this.currentCrossHairPointerScale = element.GetAttributeFloat("crosshairscale", 0.1f);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "particleemitter")
				{
					this.particleEmitters.Add(new ParticleEmitter(subElement));
				}
			}
		}

		// Token: 0x04002DAD RID: 11693
		private readonly List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>();

		// Token: 0x04002DAE RID: 11694
		private Hull targetHull;

		// Token: 0x04002DAF RID: 11695
		private Vector2 rayStartWorldPosition;

		// Token: 0x04002DB0 RID: 11696
		private Color color;

		// Token: 0x04002DB1 RID: 11697
		private readonly List<BackgroundSection> targetSections = new List<BackgroundSection>();

		// Token: 0x04002DB2 RID: 11698
		private int spraySetting;

		// Token: 0x04002DB3 RID: 11699
		private readonly Point[] sprayArray = new Point[8];

		// Token: 0x04002DB6 RID: 11702
		public readonly ImmutableDictionary<Identifier, Color> LiquidColors;
	}
}
