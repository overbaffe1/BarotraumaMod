using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Sounds;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma.CharacterEditor
{
	// Token: 0x0200044C RID: 1100
	internal class CharacterEditorScreen : EditorScreen
	{
		// Token: 0x170012A3 RID: 4771
		// (get) Token: 0x06004923 RID: 18723 RVA: 0x0027F15A File Offset: 0x0027D35A
		// (set) Token: 0x06004924 RID: 18724 RVA: 0x0027F161 File Offset: 0x0027D361
		public static CharacterEditorScreen Instance { get; private set; }

		// Token: 0x170012A4 RID: 4772
		// (get) Token: 0x06004925 RID: 18725 RVA: 0x0027F169 File Offset: 0x0027D369
		public override Camera Cam
		{
			get
			{
				if (this.cam == null)
				{
					this.cam = new Camera
					{
						MinZoom = 0.1f,
						MaxZoom = 5f
					};
				}
				return this.cam;
			}
		}

		// Token: 0x170012A5 RID: 4773
		// (get) Token: 0x06004926 RID: 18726 RVA: 0x0027F19A File Offset: 0x0027D39A
		private bool ShowExtraRagdollControls
		{
			get
			{
				return this.editLimbs || this.editJoints;
			}
		}

		// Token: 0x170012A6 RID: 4774
		// (get) Token: 0x06004927 RID: 18727 RVA: 0x0027F1AC File Offset: 0x0027D3AC
		public Character SpawnedCharacter
		{
			get
			{
				return this.character;
			}
		}

		// Token: 0x06004928 RID: 18728 RVA: 0x0027F1B4 File Offset: 0x0027D3B4
		private Rectangle CalculateSpritesheetRectangle()
		{
			if (this.Textures != null && !this.Textures.None(null))
			{
				return new Rectangle(30, 20, (int)((float)(from t in this.Textures
				orderby t.Width descending
				select t).First<Texture2D>().Width * this.spriteSheetZoom), (int)((float)this.Textures.Sum((Texture2D t) => t.Height) * this.spriteSheetZoom));
			}
			return Rectangle.Empty;
		}

		// Token: 0x06004929 RID: 18729 RVA: 0x0027F25C File Offset: 0x0027D45C
		public override void Select()
		{
			base.Select();
			GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryWaterAmbience, 0f, 0);
			GUI.ForceMouseOn(null);
			if (Submarine.MainSub == null)
			{
				this.ResetVariables();
				SubmarineInfo subInfo = new SubmarineInfo("Content/AnimEditor.sub", "", null, true, false);
				Submarine.MainSub = new Submarine(subInfo, false, null, null);
				if (Submarine.MainSub.PhysicsBody != null)
				{
					Submarine.MainSub.PhysicsBody.Enabled = false;
				}
				this.wallGroups[0] = new CharacterEditorScreen.WallGroup(new List<MapEntity>(MapEntity.MapEntityList));
				this.CloneWalls();
				this.CalculateMovementLimits();
				this.isEndlessRunner = true;
				GameMain.LightManager.LightingEnabled = false;
			}
			else if (CharacterEditorScreen.Instance == null)
			{
				this.ResetVariables();
			}
			Submarine.MainSub.GodMode = true;
			if (Character.Controlled == null)
			{
				Identifier humanSpeciesName = CharacterPrefab.HumanSpeciesName;
				if (humanSpeciesName.IsEmpty)
				{
					this.SpawnCharacter(this.VisibleSpecies.First<Identifier>(), null);
				}
				else
				{
					this.SpawnCharacter(humanSpeciesName, null);
				}
			}
			else
			{
				this.OnPreSpawn();
				this.character = Character.Controlled;
				this.OnPostSpawn();
			}
			this.OpenDoors();
			GameMain.Instance.ResolutionChanged += this.OnResolutionChanged;
			CharacterEditorScreen.Instance = this;
		}

		// Token: 0x0600492A RID: 18730 RVA: 0x0027F394 File Offset: 0x0027D594
		private void ResetVariables()
		{
			this.editCharacterInfo = false;
			this.editRagdoll = false;
			this.editAnimations = false;
			this.editLimbs = false;
			this.editJoints = false;
			this.editIK = false;
			this.drawSkeleton = false;
			this.drawDamageModifiers = false;
			this.showParamsEditor = false;
			this.showSpritesheet = false;
			this.isFrozen = false;
			this.autoFreeze = false;
			this.limbPairEditing = false;
			this.uniformScaling = true;
			this.lockSpriteOrigin = true;
			this.lockSpritePosition = false;
			this.lockSpriteSize = false;
			this.recalculateCollider = false;
			this.copyJointSettings = false;
			this.showColliders = false;
			this.displayWearables = true;
			this.displayBackgroundColor = false;
			this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
			this.isDrawingLimb = false;
			this.newLimbRect = Rectangle.Empty;
			this.cameraOffset = Vector2.Zero;
			this.jointEndLimb = null;
			this.anchor1Pos = null;
			this.jointStartLimb = null;
			this.visibleSpecies = null;
			this.onlyShowSourceRectForSelectedLimbs = false;
			this.unrestrictSpritesheet = false;
			this.editedCharacters.Clear();
			this.selectedJoints.Clear();
			this.selectedLimbs.Clear();
			if (this.character != null && this.character.AnimController != null && this.character.AnimController.Collider != null)
			{
				this.character.AnimController.Collider.PhysEnabled = true;
			}
			this.character = null;
			Wizard instance = Wizard.instance;
			if (instance == null)
			{
				return;
			}
			instance.Reset();
		}

		// Token: 0x0600492B RID: 18731 RVA: 0x0027F502 File Offset: 0x0027D702
		private void Reset(IEnumerable<Character> characters = null)
		{
			if (characters == null)
			{
				characters = this.editedCharacters;
			}
			characters.ForEach(delegate(Character c)
			{
				CharacterEditorScreen.ResetParams(c);
			});
			this.ResetVariables();
		}

		// Token: 0x0600492C RID: 18732 RVA: 0x0027F53C File Offset: 0x0027D73C
		private static void ResetParams(Character character)
		{
			character.Params.Reset(true);
			foreach (AnimationParams animation in character.AnimController.AllAnimParams)
			{
				animation.Reset(true);
				animation.ClearHistory();
			}
			character.AnimController.RagdollParams.Reset(true);
			character.AnimController.RagdollParams.ClearHistory();
			character.ForceRun = false;
			character.AnimController.ForceSelectAnimationType = AnimationType.NotDefined;
		}

		// Token: 0x0600492D RID: 18733 RVA: 0x0027F5E0 File Offset: 0x0027D7E0
		protected override void DeselectEditorSpecific()
		{
			SoundPlayer.OverrideMusicType = Identifier.Empty;
			GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryWaterAmbience, GameSettings.CurrentConfig.Audio.SoundVolume, 0);
			GUI.ForceMouseOn(null);
			if (this.isEndlessRunner)
			{
				Submarine mainSub = Submarine.MainSub;
				if (mainSub != null)
				{
					mainSub.Remove();
				}
				GameMain.World.ProcessChanges();
				this.isEndlessRunner = false;
				this.Reset(null);
				if (this.character != null && !this.character.Removed)
				{
					this.character.Remove();
				}
			}
			else
			{
				this.Reset(from c in Character.CharacterList
				where this.VanillaCharacters.Any((CharacterFile vchar) => vchar == c.Prefab.ContentFile)
				select c);
			}
			GameMain.Instance.ResolutionChanged -= this.OnResolutionChanged;
			if (!GameMain.DevMode)
			{
				GameMain.LightManager.LightingEnabled = true;
			}
			this.ClearWidgets();
			this.ClearSelection();
		}

		// Token: 0x0600492E RID: 18734 RVA: 0x0027F6BE File Offset: 0x0027D8BE
		private void OnResolutionChanged()
		{
			this.CreateGUI();
		}

		// Token: 0x0600492F RID: 18735 RVA: 0x0027F6C6 File Offset: 0x0027D8C6
		public static LocalizedString GetCharacterEditorTranslation(string tag)
		{
			return TextManager.Get("CharacterEditor." + tag);
		}

		// Token: 0x06004930 RID: 18736 RVA: 0x0027F6D8 File Offset: 0x0027D8D8
		public override void AddToGUIUpdateList()
		{
			if (this.rightArea == null || this.leftArea == null)
			{
				return;
			}
			this.rightArea.AddToGUIUpdateList(false, 0);
			this.leftArea.AddToGUIUpdateList(false, 0);
			Wizard instance = Wizard.instance;
			if (instance != null)
			{
				instance.AddToGUIUpdateList();
			}
			if (this.displayBackgroundColor)
			{
				this.backgroundColorPanel.AddToGUIUpdateList(false, 0);
			}
			if (this.editAnimations)
			{
				this.animationControls.AddToGUIUpdateList(false, 0);
			}
			if (this.showSpritesheet)
			{
				this.spriteSheetControls.AddToGUIUpdateList(false, 0);
				Limb lastLimb = this.selectedLimbs.LastOrDefault<Limb>();
				if (lastLimb == null)
				{
					LimbJoint lastJoint = this.selectedJoints.LastOrDefault<LimbJoint>();
					if (lastJoint != null)
					{
						lastLimb = (PlayerInput.KeyDown(Keys.LeftAlt) ? lastJoint.LimbB : lastJoint.LimbA);
					}
				}
				if (lastLimb != null)
				{
					this.resetSpriteOrientationButtonParent.AddToGUIUpdateList(false, 0);
				}
			}
			if (this.editRagdoll)
			{
				this.ragdollControls.AddToGUIUpdateList(false, 0);
			}
			if (this.editJoints)
			{
				this.jointControls.AddToGUIUpdateList(false, 0);
			}
			if (this.editLimbs && !this.unrestrictSpritesheet)
			{
				this.limbControls.AddToGUIUpdateList(false, 0);
			}
			if (this.ShowExtraRagdollControls)
			{
				this.createLimbButton.Enabled = this.editLimbs;
				this.duplicateLimbButton.Enabled = this.selectedLimbs.Any<Limb>();
				this.deleteSelectedButton.Enabled = (this.selectedLimbs.Any<Limb>() || this.selectedJoints.Any<LimbJoint>());
				this.createJointButton.Enabled = (this.selectedLimbs.Any<Limb>() || this.selectedJoints.Any<LimbJoint>());
				this.extraRagdollControls.AddToGUIUpdateList(false, 0);
				if (this.createLimbButton.Enabled)
				{
					if (this.isDrawingLimb)
					{
						this.createLimbButton.Color = Color.Yellow;
						this.createLimbButton.HoverColor = Color.Yellow;
					}
					else
					{
						this.createLimbButton.Color = Color.White;
						this.createLimbButton.HoverColor = Color.White;
					}
				}
				if (this.createJointButton.Enabled)
				{
					CharacterEditorScreen.JointCreationMode jointCreationMode = this.jointCreationMode;
					if (jointCreationMode - CharacterEditorScreen.JointCreationMode.Select <= 1)
					{
						this.createJointButton.HoverColor = Color.Yellow;
						this.createJointButton.Color = Color.Yellow;
					}
					else
					{
						this.createJointButton.HoverColor = Color.White;
						this.createJointButton.Color = Color.White;
					}
				}
			}
			if (this.showParamsEditor)
			{
				ParamsEditor.Instance.EditorBox.Parent.AddToGUIUpdateList(false, 0);
			}
		}

		// Token: 0x06004931 RID: 18737 RVA: 0x0027F94C File Offset: 0x0027DB4C
		public override void Update(double deltaTime)
		{
			base.Update(deltaTime);
			if (Wizard.instance != null)
			{
				return;
			}
			LightManager lightManager = GameMain.LightManager;
			if (lightManager != null)
			{
				lightManager.Update((float)deltaTime);
			}
			this.spriteSheetRect = this.CalculateSpritesheetRectangle();
			if (PlayerInput.KeyHit(Keys.F1))
			{
				this.SetToggle(this.paramsToggle, !this.paramsToggle.Selected);
			}
			if (PlayerInput.KeyHit(Keys.F5))
			{
				this.RecreateRagdoll(null);
			}
			if (GUI.KeyboardDispatcher.Subscriber == null)
			{
				if (PlayerInput.KeyHit(Keys.D1))
				{
					this.SetToggle(this.characterInfoToggle, !this.characterInfoToggle.Selected);
				}
				else if (PlayerInput.KeyHit(Keys.D2))
				{
					this.SetToggle(this.ragdollToggle, !this.ragdollToggle.Selected);
				}
				else if (PlayerInput.KeyHit(Keys.D3))
				{
					this.SetToggle(this.limbsToggle, !this.limbsToggle.Selected);
				}
				else if (PlayerInput.KeyHit(Keys.D4))
				{
					this.SetToggle(this.jointsToggle, !this.jointsToggle.Selected);
				}
				else if (PlayerInput.KeyHit(Keys.D5))
				{
					this.SetToggle(this.animsToggle, !this.animsToggle.Selected);
				}
				if (PlayerInput.KeyDown(Keys.LeftControl))
				{
					Character.DisableControls = true;
					Widget.EnableMultiSelect = !this.editAnimations;
					if (PlayerInput.KeyHit(Keys.Z))
					{
						if (this.editJoints || this.editLimbs || this.editIK)
						{
							this.RagdollParams.Undo();
							this.character.AnimController.ResetJoints();
							this.character.AnimController.ResetLimbs();
							this.ClearWidgets();
							this.CreateGUI();
							this.ResetParamsEditor();
						}
						if (this.editAnimations)
						{
							this.CurrentAnimation.Undo();
							this.ClearWidgets();
							this.ResetParamsEditor();
						}
					}
					else if (PlayerInput.KeyHit(Keys.R))
					{
						if (this.editJoints || this.editLimbs || this.editIK)
						{
							this.RagdollParams.Redo();
							this.character.AnimController.ResetJoints();
							this.character.AnimController.ResetLimbs();
							this.ClearWidgets();
							this.CreateGUI();
							this.ResetParamsEditor();
						}
						if (this.editAnimations)
						{
							this.CurrentAnimation.Redo();
							this.ClearWidgets();
							this.ResetParamsEditor();
						}
					}
				}
				else
				{
					Widget.EnableMultiSelect = false;
					if (PlayerInput.KeyHit(Keys.C))
					{
						this.SetToggle(this.showCollidersToggle, !this.showCollidersToggle.Selected);
					}
					if (PlayerInput.KeyHit(Keys.L))
					{
						this.SetToggle(this.lightsToggle, !this.lightsToggle.Selected);
					}
					if (PlayerInput.KeyHit(Keys.M))
					{
						this.SetToggle(this.damageModifiersToggle, !this.damageModifiersToggle.Selected);
					}
					if (PlayerInput.KeyHit(Keys.N))
					{
						this.SetToggle(this.skeletonToggle, !this.skeletonToggle.Selected);
					}
					if (PlayerInput.KeyHit(Keys.T))
					{
						this.SetToggle(this.spritesheetToggle, !this.spritesheetToggle.Selected);
					}
					if (PlayerInput.KeyHit(Keys.I))
					{
						this.SetToggle(this.ikToggle, !this.ikToggle.Selected);
					}
				}
				if (PlayerInput.KeyDown(InputType.Left) || PlayerInput.KeyDown(InputType.Right) || PlayerInput.KeyDown(InputType.Up) || PlayerInput.KeyDown(InputType.Down))
				{
					this.character.AnimController.Collider.PhysEnabled = true;
				}
				this.animTestPoseToggle.Enabled = this.CurrentAnimation.IsGroundedAnimation;
				if (this.animTestPoseToggle.Enabled)
				{
					if (PlayerInput.KeyHit(Keys.X))
					{
						this.SetToggle(this.animTestPoseToggle, !this.animTestPoseToggle.Selected);
					}
				}
				else
				{
					this.animTestPoseToggle.Selected = false;
				}
				if (PlayerInput.KeyHit(InputType.Run))
				{
					bool isSwimming = this.character.AnimController.ForceSelectAnimationType == AnimationType.SwimFast || this.character.AnimController.ForceSelectAnimationType == AnimationType.SwimSlow;
					bool isMovingFast = this.character.AnimController.ForceSelectAnimationType == AnimationType.Run || this.character.AnimController.ForceSelectAnimationType == AnimationType.SwimFast;
					int index;
					if (this.character.AnimController.CanWalk)
					{
						if (isMovingFast)
						{
							if (isSwimming)
							{
								index = 2;
							}
							else
							{
								index = 0;
							}
						}
						else if (isSwimming)
						{
							index = 3;
						}
						else
						{
							index = 1;
						}
					}
					else
					{
						index = ((!isMovingFast) ? 1 : 0);
					}
					if (this.animSelection.SelectedIndex != index)
					{
						this.CurrentAnimation.ClearHistory();
						this.animSelection.Select(index);
						this.CurrentAnimation.StoreSnapshot();
					}
				}
				if (!PlayerInput.KeyDown(Keys.LeftControl) && PlayerInput.KeyHit(Keys.E))
				{
					bool isSwimming2 = this.character.AnimController.ForceSelectAnimationType == AnimationType.SwimFast || this.character.AnimController.ForceSelectAnimationType == AnimationType.SwimSlow;
					if (isSwimming2)
					{
						this.animSelection.Select(0);
					}
					else
					{
						this.animSelection.Select(2);
					}
				}
				if (PlayerInput.SecondaryMouseButtonClicked() || PlayerInput.KeyHit(Keys.Escape))
				{
					bool reset = false;
					if (this.selectedLimbs.Any<Limb>())
					{
						this.selectedLimbs.Clear();
						reset = true;
					}
					if (this.selectedJoints.Any<LimbJoint>())
					{
						this.selectedJoints.Clear();
						foreach (Widget w2 in this.jointSelectionWidgets.Values)
						{
							w2.Refresh();
							Widget linkedWidget = w2.LinkedWidget;
							if (linkedWidget != null)
							{
								linkedWidget.Refresh();
							}
						}
						reset = true;
					}
					if (reset)
					{
						this.ResetParamsEditor();
					}
					this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
					this.isDrawingLimb = false;
				}
				if (PlayerInput.KeyHit(Keys.Delete))
				{
					this.DeleteSelected();
				}
				if (this.ShowExtraRagdollControls && PlayerInput.KeyDown(Keys.LeftControl) && PlayerInput.KeyHit(Keys.E))
				{
					this.ToggleJointCreationMode();
				}
				this.UpdateJointCreation();
				this.UpdateLimbCreation();
				if (PlayerInput.KeyHit(Keys.Left))
				{
					this.Nudge(Keys.Left);
				}
				if (PlayerInput.KeyHit(Keys.Right))
				{
					this.Nudge(Keys.Right);
				}
				if (PlayerInput.KeyHit(Keys.Down))
				{
					this.Nudge(Keys.Down);
				}
				if (PlayerInput.KeyHit(Keys.Up))
				{
					this.Nudge(Keys.Up);
				}
				if (PlayerInput.KeyDown(Keys.Left))
				{
					this.holdTimer += deltaTime;
					if (this.holdTimer > 0.20000000298023224)
					{
						this.Nudge(Keys.Left);
					}
				}
				else if (PlayerInput.KeyDown(Keys.Right))
				{
					this.holdTimer += deltaTime;
					if (this.holdTimer > 0.20000000298023224)
					{
						this.Nudge(Keys.Right);
					}
				}
				else if (PlayerInput.KeyDown(Keys.Down))
				{
					this.holdTimer += deltaTime;
					if (this.holdTimer > 0.20000000298023224)
					{
						this.Nudge(Keys.Down);
					}
				}
				else if (PlayerInput.KeyDown(Keys.Up))
				{
					this.holdTimer += deltaTime;
					if (this.holdTimer > 0.20000000298023224)
					{
						this.Nudge(Keys.Up);
					}
				}
				else
				{
					this.holdTimer = 0.0;
				}
				if (this.isFrozen)
				{
					float moveSpeed = (float)deltaTime * 300f / this.Cam.Zoom;
					if (PlayerInput.KeyDown(Keys.LeftShift))
					{
						moveSpeed *= 4f;
					}
					if (PlayerInput.KeyDown(Keys.W))
					{
						this.cameraOffset.Y = this.cameraOffset.Y + moveSpeed;
					}
					if (PlayerInput.KeyDown(Keys.A))
					{
						this.cameraOffset.X = this.cameraOffset.X - moveSpeed;
					}
					if (PlayerInput.KeyDown(Keys.S))
					{
						this.cameraOffset.Y = this.cameraOffset.Y - moveSpeed;
					}
					if (PlayerInput.KeyDown(Keys.D))
					{
						this.cameraOffset.X = this.cameraOffset.X + moveSpeed;
					}
					Vector2 max = new Vector2((float)GameMain.GraphicsWidth * 0.3f, (float)GameMain.GraphicsHeight * 0.38f) / this.Cam.Zoom;
					Vector2 min = -max;
					this.cameraOffset = Vector2.Clamp(this.cameraOffset, min, max);
				}
			}
			if (!this.isFrozen)
			{
				foreach (PhysicsBody body in PhysicsBody.List)
				{
					body.SetPrevTransform(body.SimPosition, body.Rotation);
					body.Update();
				}
				if (!Character.DisableControls)
				{
					this.character.IsRagdolled = PlayerInput.KeyDown(InputType.Ragdoll);
				}
				if (this.character.IsRagdolled)
				{
					this.character.AnimController.ResetPullJoints(null);
				}
				this.character.ControlLocalPlayer((float)deltaTime, this.Cam, false);
				this.character.Control((float)deltaTime, this.Cam);
				this.character.AnimController.UpdateAnimations((float)deltaTime);
				this.character.AnimController.UpdateRagdoll((float)deltaTime, this.Cam);
				this.character.CurrentHull = this.character.AnimController.CurrentHull;
				if (this.isEndlessRunner)
				{
					if (this.character.Position.X < (float)this.min)
					{
						this.UpdateWalls(false);
					}
					else if (this.character.Position.X > (float)this.max)
					{
						this.UpdateWalls(true);
					}
				}
				try
				{
					GameMain.World.Step(0.016666668f);
				}
				catch (WorldLockedException e)
				{
					string errorMsg = "Attempted to modify the state of the physics simulation while a time step was running.";
					DebugConsole.ThrowError(errorMsg, e, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("CharacterEditorScreen.Update:WorldLockedException" + e.Message, GameAnalyticsManager.ErrorSeverity.Critical, errorMsg);
				}
			}
			this.Cam.MoveCamera((float)deltaTime, false, GUI.MouseOn == null, true, null);
			Vector2 targetPos = this.character.WorldPosition;
			if (PlayerInput.MidButtonHeld())
			{
				Vector2 moveSpeed2 = PlayerInput.MouseSpeed * (float)deltaTime * 100f / this.Cam.Zoom;
				moveSpeed2.X = -moveSpeed2.X;
				this.cameraOffset += moveSpeed2;
				Vector2 max2 = new Vector2((float)GameMain.GraphicsWidth * 0.3f, (float)GameMain.GraphicsHeight * 0.38f) / this.Cam.Zoom;
				Vector2 min2 = -max2;
				this.cameraOffset = Vector2.Clamp(this.cameraOffset, min2, max2);
			}
			this.Cam.Position = targetPos + this.cameraOffset;
			MapEntity.ClearHighlightedEntities();
			this.jointSelectionWidgets.Values.ForEach(delegate(Widget w)
			{
				w.Update((float)deltaTime);
			});
			this.limbEditWidgets.Values.ForEach(delegate(Widget w)
			{
				w.Update((float)deltaTime);
			});
			this.animationWidgets.Values.ForEach(delegate(Widget w)
			{
				w.Update((float)deltaTime);
			});
			if (PlayerInput.PrimaryMouseButtonDown() && GUI.MouseOn == null && Widget.SelectedWidgets.None(null))
			{
				Limb[] limbs = this.character.AnimController.Limbs;
				for (int i = 0; i < limbs.Length; i++)
				{
					Limb limb = limbs[i];
					if (limb != null && limb.ActiveSprite != null && !this.selectedJoints.Any((LimbJoint j) => j.LimbA == limb || j.LimbB == limb))
					{
						if (this.editLimbs && !this.spriteSheetRect.Contains(PlayerInput.MousePosition) && MathUtils.RectangleContainsPoint(this.GetLimbPhysicRect(limb), PlayerInput.MousePosition))
						{
							this.HandleLimbSelection(limb);
						}
						if (this.GetLimbSpritesheetRect(limb).Contains(PlayerInput.MousePosition))
						{
							this.HandleLimbSelection(limb);
						}
					}
				}
			}
			CharacterEditorScreen.ToggleButton toggleButton = this.optionsToggle;
			if (toggleButton != null)
			{
				toggleButton.UpdateOpenState((float)deltaTime, new Vector2((float)(-(float)this.optionsPanel.Rect.Width - this.rightArea.RectTransform.AbsoluteOffset.X), 0f), this.optionsPanel.RectTransform);
			}
			CharacterEditorScreen.ToggleButton toggleButton2 = this.fileEditToggle;
			if (toggleButton2 != null)
			{
				toggleButton2.UpdateOpenState((float)deltaTime, new Vector2((float)(-(float)this.fileEditPanel.Rect.Width - this.rightArea.RectTransform.AbsoluteOffset.X), 0f), this.fileEditPanel.RectTransform);
			}
			CharacterEditorScreen.ToggleButton toggleButton3 = this.characterPanelToggle;
			if (toggleButton3 != null)
			{
				toggleButton3.UpdateOpenState((float)deltaTime, new Vector2((float)(-(float)this.characterSelectionPanel.Rect.Width - this.rightArea.RectTransform.AbsoluteOffset.X), 0f), this.characterSelectionPanel.RectTransform);
			}
			CharacterEditorScreen.ToggleButton toggleButton4 = this.minorModesToggle;
			if (toggleButton4 != null)
			{
				toggleButton4.UpdateOpenState((float)deltaTime, new Vector2((float)(-(float)this.minorModesPanel.Rect.Width - this.leftArea.RectTransform.AbsoluteOffset.X), 0f), this.minorModesPanel.RectTransform);
			}
			CharacterEditorScreen.ToggleButton toggleButton5 = this.modesToggle;
			if (toggleButton5 != null)
			{
				toggleButton5.UpdateOpenState((float)deltaTime, new Vector2((float)(-(float)this.modesPanel.Rect.Width - this.leftArea.RectTransform.AbsoluteOffset.X), 0f), this.modesPanel.RectTransform);
			}
			CharacterEditorScreen.ToggleButton toggleButton6 = this.buttonsPanelToggle;
			if (toggleButton6 != null)
			{
				toggleButton6.UpdateOpenState((float)deltaTime, new Vector2((float)(-(float)this.buttonsPanel.Rect.Width - this.leftArea.RectTransform.AbsoluteOffset.X), 0f), this.buttonsPanel.RectTransform);
			}
			this.totalMassText.Text = this.GetTotalMassText();
		}

		// Token: 0x06004932 RID: 18738 RVA: 0x00280794 File Offset: 0x0027E994
		private LocalizedString GetTotalMassText()
		{
			string tag = "CharacterEditor.totalmass";
			string varName = "[mass]";
			Character character = this.character;
			string text;
			if (character == null)
			{
				text = null;
			}
			else
			{
				AnimController animController = character.AnimController;
				text = ((animController != null) ? animController.Mass.FormatZeroDecimal() : null);
			}
			return TextManager.GetWithVariable(tag, varName, text ?? "0", FormatCapitals.No);
		}

		// Token: 0x06004933 RID: 18739 RVA: 0x002807E4 File Offset: 0x0027E9E4
		public CursorState GetMouseCursorState()
		{
			Limb[] limbs = this.character.AnimController.Limbs;
			for (int i = 0; i < limbs.Length; i++)
			{
				Limb limb = limbs[i];
				Limb limb2 = limb;
				if (((limb2 != null) ? limb2.ActiveSprite : null) != null && !this.selectedJoints.Any((LimbJoint j) => j.LimbA == limb || j.LimbB == limb))
				{
					if (this.editLimbs && !this.spriteSheetRect.Contains(PlayerInput.MousePosition) && MathUtils.RectangleContainsPoint(this.GetLimbPhysicRect(limb), PlayerInput.MousePosition))
					{
						return CursorState.Hand;
					}
					if (this.showSpritesheet && this.GetLimbSpritesheetRect(limb).Contains(PlayerInput.MousePosition))
					{
						return CursorState.Hand;
					}
				}
			}
			return CursorState.Default;
		}

		// Token: 0x06004934 RID: 18740 RVA: 0x002808AC File Offset: 0x0027EAAC
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			if (this.isFrozen)
			{
				Timing.Alpha = 0.0;
			}
			this.scaledMouseSpeed = PlayerInput.MouseSpeedPerSecond * (float)deltaTime;
			this.Cam.UpdateTransform(true, true);
			Submarine.CullEntities(this.Cam);
			Submarine mainSub = Submarine.MainSub;
			if (mainSub != null)
			{
				mainSub.UpdateTransform(true);
			}
			if (GameMain.LightManager.LightingEnabled && Character.Controlled != null)
			{
				GameMain.LightManager.ObstructVisionAmount = Character.Controlled.ObstructVisionAmount;
				GameMain.LightManager.RenderLightMap(graphics, spriteBatch, this.cam, null);
				GameMain.LightManager.UpdateObstructVision(graphics, spriteBatch, this.cam, Character.Controlled.CursorWorldPosition);
			}
			base.Draw(deltaTime, graphics, spriteBatch);
			graphics.Clear(this.backgroundColor);
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.Cam.Transform));
			Submarine.DrawBack(spriteBatch, this.isEndlessRunner, null);
			Submarine.DrawFront(spriteBatch, this.isEndlessRunner, null);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.Cam.Transform));
			Character.CharacterList.ForEach(delegate(Character c)
			{
				c.Draw(spriteBatch, this.Cam);
			});
			if (GameMain.DebugDraw)
			{
				this.character.AnimController.DebugDraw(spriteBatch);
			}
			else if (this.showColliders)
			{
				this.character.AnimController.Collider.DebugDraw(spriteBatch, Color.White, true);
				foreach (Limb limb in this.character.AnimController.Limbs)
				{
					if (!limb.Hide)
					{
						limb.body.DebugDraw(spriteBatch, GUIStyle.Green, true);
					}
				}
			}
			spriteBatch.End();
			if (GameMain.LightManager.LightingEnabled)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, CustomBlendStates.Multiplicative, null, DepthStencilState.None, null, null, null);
				spriteBatch.Draw(GameMain.LightManager.LightMap, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.White);
				spriteBatch.End();
			}
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			if (this.drawDamageModifiers)
			{
				foreach (Limb limb2 in this.character.AnimController.Limbs)
				{
					if (this.selectedLimbs.Contains(limb2) || this.selectedLimbs.None(null))
					{
						limb2.DrawDamageModifiers(spriteBatch, this.cam, this.cam.WorldToScreen(limb2.DrawPosition), true);
					}
				}
			}
			if (this.editAnimations)
			{
				this.DrawAnimationControls(spriteBatch, (float)deltaTime);
			}
			if (this.editLimbs)
			{
				this.DrawLimbEditor(spriteBatch);
			}
			if (this.drawSkeleton || this.editRagdoll || this.editJoints || this.editLimbs || this.editIK)
			{
				this.DrawRagdoll(spriteBatch, (float)deltaTime);
			}
			Limb head = this.character.AnimController.GetLimb(LimbType.Head, true, false, false);
			if (head != null && this.character.CanEat && this.selectedLimbs.Contains(head))
			{
				Vector2? mouthPos = this.character.AnimController.GetMouthPosition();
				if (mouthPos != null)
				{
					spriteBatch.DrawPoint(this.SimToScreen(mouthPos.Value), GUIStyle.Red, 8f);
				}
			}
			if (this.showSpritesheet)
			{
				this.DrawSpritesheetEditor(spriteBatch, (float)deltaTime);
			}
			if (this.isDrawingLimb)
			{
				GUI.DrawRectangle(spriteBatch, this.newLimbRect, Color.Yellow, false, 0f, 1f);
			}
			if (this.jointCreationMode != CharacterEditorScreen.JointCreationMode.None)
			{
				Vector2 textPos = new Vector2((float)(GameMain.GraphicsWidth / 2 - 240), (float)(GameMain.GraphicsHeight / 4));
				if (this.jointCreationMode == CharacterEditorScreen.JointCreationMode.Select)
				{
					SpriteBatch spriteBatch2 = spriteBatch;
					Vector2 pos = textPos;
					LocalizedString characterEditorTranslation = CharacterEditorScreen.GetCharacterEditorTranslation("SelectAnchor1Pos");
					Color yellow = Color.Yellow;
					GUIFont font = GUIStyle.LargeFont;
					GUI.DrawString(spriteBatch2, pos, characterEditorTranslation, yellow, null, 0, font, ForceUpperCase.Inherit);
				}
				else
				{
					SpriteBatch spriteBatch3 = spriteBatch;
					Vector2 pos2 = textPos;
					LocalizedString characterEditorTranslation2 = CharacterEditorScreen.GetCharacterEditorTranslation("SelectLimbToConnect");
					Color yellow2 = Color.Yellow;
					GUIFont font = GUIStyle.LargeFont;
					GUI.DrawString(spriteBatch3, pos2, characterEditorTranslation2, yellow2, null, 0, font, ForceUpperCase.Inherit);
				}
				if (this.jointStartLimb != null && this.jointStartLimb.ActiveSprite != null)
				{
					GUI.DrawRectangle(spriteBatch, this.GetLimbSpritesheetRect(this.jointStartLimb), Color.Yellow, false, 0f, 3f);
					GUI.DrawRectangle(spriteBatch, this.GetLimbPhysicRect(this.jointStartLimb), Color.Yellow, 0f, 3f);
				}
				if (this.jointEndLimb != null && this.jointEndLimb.ActiveSprite != null)
				{
					GUI.DrawRectangle(spriteBatch, this.GetLimbSpritesheetRect(this.jointEndLimb), GUIStyle.Green, false, 0f, 3f);
					GUI.DrawRectangle(spriteBatch, this.GetLimbPhysicRect(this.jointEndLimb), GUIStyle.Green, 0f, 3f);
				}
				if (this.spriteSheetRect.Contains(PlayerInput.MousePosition))
				{
					if (this.jointStartLimb != null)
					{
						Vector2 startPos = this.GetLimbSpritesheetRect(this.jointStartLimb).Center.ToVector2();
						Vector2 offset = this.anchor1Pos ?? Vector2.Zero;
						offset = -offset;
						startPos += offset;
						GUI.DrawLine(spriteBatch, startPos, PlayerInput.MousePosition, GUIStyle.Green, 0f, 3f);
					}
				}
				else if (this.jointStartLimb != null)
				{
					Vector2 offset2 = (this.anchor1Pos != null) ? Vector2.Transform(this.anchor1Pos.Value, Matrix.CreateRotationZ(this.jointStartLimb.Rotation)) : Vector2.Zero;
					Vector2 startPos2 = this.cam.WorldToScreen(this.jointStartLimb.DrawPosition + offset2);
					GUI.DrawLine(spriteBatch, startPos2, PlayerInput.MousePosition, GUIStyle.Green, 0f, 3f);
				}
			}
			if (this.isDrawingLimb)
			{
				Vector2 textPos2 = new Vector2((float)(GameMain.GraphicsWidth / 2 - 200), (float)(GameMain.GraphicsHeight / 4));
				SpriteBatch spriteBatch4 = spriteBatch;
				Vector2 pos3 = textPos2;
				LocalizedString characterEditorTranslation3 = CharacterEditorScreen.GetCharacterEditorTranslation("DrawLimbOnSpritesheet");
				Color yellow3 = Color.Yellow;
				GUIFont font = GUIStyle.LargeFont;
				GUI.DrawString(spriteBatch4, pos3, characterEditorTranslation3, yellow3, null, 0, font, ForceUpperCase.Inherit);
			}
			if (this.isEndlessRunner)
			{
				Vector2 indicatorPos = this.MiddleWall.Entities.First<MapEntity>().DrawPosition;
				GUI.DrawIndicator(spriteBatch, indicatorPos, this.Cam, 700f, GUIStyle.SubmarineLocationIcon.Value.Sprite, Color.White, true, 1f, null);
			}
			GUI.Draw(this.Cam, spriteBatch);
			if (this.isFrozen)
			{
				GUI.DrawString(spriteBatch, new Vector2((float)(GameMain.GraphicsWidth / 2 - 40), 200f), CharacterEditorScreen.GetCharacterEditorTranslation("Frozen"), Color.Blue, new Color?(Color.White * 0.5f), 10, GUIStyle.LargeFont, ForceUpperCase.Inherit);
			}
			if (this.animTestPoseToggle.Selected)
			{
				GUI.DrawString(spriteBatch, new Vector2((float)(GameMain.GraphicsWidth / 2 - 100), 300f), CharacterEditorScreen.GetCharacterEditorTranslation("AnimationTestPoseEnabled"), Color.White, new Color?(Color.Black * 0.5f), 10, GUIStyle.LargeFont, ForceUpperCase.Inherit);
			}
			if (this.selectedJoints.Count == 1)
			{
				SpriteBatch spriteBatch5 = spriteBatch;
				Vector2 pos4 = new Vector2((float)(GameMain.GraphicsWidth / 2), 20f);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("Selected"));
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted(this.selectedJoints.First<LimbJoint>().Params.Name);
				string text = defaultInterpolatedStringHandler.ToStringAndClear();
				Color white = Color.White;
				GUIFont font = GUIStyle.LargeFont;
				GUI.DrawString(spriteBatch5, pos4, text, white, null, 0, font, ForceUpperCase.Inherit);
			}
			if (this.selectedLimbs.Count == 1)
			{
				SpriteBatch spriteBatch6 = spriteBatch;
				Vector2 pos5 = new Vector2((float)(GameMain.GraphicsWidth / 2), 20f);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("Selected"));
				defaultInterpolatedStringHandler2.AppendLiteral(": ");
				defaultInterpolatedStringHandler2.AppendFormatted(this.selectedLimbs.First<Limb>().Params.Name);
				string text2 = defaultInterpolatedStringHandler2.ToStringAndClear();
				Color white2 = Color.White;
				GUIFont font = GUIStyle.LargeFont;
				GUI.DrawString(spriteBatch6, pos5, text2, white2, null, 0, font, ForceUpperCase.Inherit);
			}
			if (this.showSpritesheet)
			{
				Limb lastLimb = this.selectedLimbs.LastOrDefault<Limb>();
				if (lastLimb == null)
				{
					LimbJoint lastJoint = this.selectedJoints.LastOrDefault<LimbJoint>();
					if (lastJoint != null)
					{
						lastLimb = (PlayerInput.KeyDown(Keys.LeftAlt) ? lastJoint.LimbB : lastJoint.LimbA);
					}
				}
				if (lastLimb != null)
				{
					Point topLeft = this.spriteSheetControls.RectTransform.TopLeft;
					bool useSpritesheetOrientation = float.IsNaN(lastLimb.Params.SpriteOrientation);
					GUI.DrawString(spriteBatch, new Vector2((float)topLeft.X + 350f * GUI.xScale, (float)GameMain.GraphicsHeight - 95f * GUI.yScale), CharacterEditorScreen.GetCharacterEditorTranslation("SpriteOrientation") + ":", useSpritesheetOrientation ? Color.White : Color.Yellow, new Color?(Color.Gray * 0.5f), 10, GUIStyle.Font, ForceUpperCase.Inherit);
					float orientation = useSpritesheetOrientation ? this.RagdollParams.SpritesheetOrientation : lastLimb.Params.SpriteOrientation;
					this.DrawRadialWidget(spriteBatch, new Vector2((float)topLeft.X + 610f * GUI.xScale, (float)GameMain.GraphicsHeight - 75f * GUI.yScale), orientation, CharacterEditorScreen.GetCharacterEditorTranslation("spriteorientationtooltip") + "\n\n" + CharacterEditorScreen.GetCharacterEditorTranslation("generalorientationtooltip"), useSpritesheetOrientation ? Color.White : Color.Yellow, delegate(float angle)
					{
						this.TryUpdateSubParam(lastLimb.Params, "spriteorientation".ToIdentifier(), angle);
						this.selectedLimbs.ForEach(delegate(Limb l)
						{
							this.TryUpdateSubParam(l.Params, "spriteorientation".ToIdentifier(), angle);
						});
						if (this.limbPairEditing)
						{
							this.UpdateOtherLimbs(lastLimb, delegate(Limb l)
							{
								this.TryUpdateSubParam(l.Params, "spriteorientation".ToIdentifier(), angle);
							});
						}
					}, 40f, 15, 0f, true, true, new bool?(false), false, false, 10);
				}
				else
				{
					Point topLeft2 = this.spriteSheetControls.RectTransform.TopLeft;
					GUI.DrawString(spriteBatch, new Vector2((float)topLeft2.X + 350f * GUI.xScale, (float)GameMain.GraphicsHeight - 95f * GUI.yScale), CharacterEditorScreen.GetCharacterEditorTranslation("SpriteSheetOrientation") + ":", Color.White, new Color?(Color.Gray * 0.5f), 10, GUIStyle.Font, ForceUpperCase.Inherit);
					this.DrawRadialWidget(spriteBatch, new Vector2((float)topLeft2.X + 610f * GUI.xScale, (float)GameMain.GraphicsHeight - 75f * GUI.yScale), this.RagdollParams.SpritesheetOrientation, CharacterEditorScreen.GetCharacterEditorTranslation("spritesheetorientationtooltip") + "\n\n" + CharacterEditorScreen.GetCharacterEditorTranslation("generalorientationtooltip"), Color.White, delegate(float angle)
					{
						this.TryUpdateRagdollParam("spritesheetorientation", angle);
					}, 40f, 15, 0f, true, true, new bool?(false), false, false, 10);
				}
			}
			if (GameMain.DebugDraw)
			{
				foreach (Limb limb3 in this.character.AnimController.Limbs)
				{
					Vector2 limbDrawPos = this.Cam.WorldToScreen(limb3.WorldPosition);
					GUI.DrawLine(spriteBatch, limbDrawPos + Vector2.UnitY * 5f, limbDrawPos - Vector2.UnitY * 5f, Color.White, 0f, 1f);
					GUI.DrawLine(spriteBatch, limbDrawPos + Vector2.UnitX * 5f, limbDrawPos - Vector2.UnitX * 5f, Color.White, 0f, 1f);
				}
				SpriteBatch spriteBatch7 = spriteBatch;
				Vector2 pos6 = new Vector2((float)(GameMain.GraphicsWidth / 2), 0f);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Cursor World Pos: ");
				defaultInterpolatedStringHandler3.AppendFormatted<Vector2>(this.character.CursorWorldPosition);
				string text3 = defaultInterpolatedStringHandler3.ToStringAndClear();
				Color white3 = Color.White;
				GUIFont font = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch7, pos6, text3, white3, null, 0, font, ForceUpperCase.Inherit);
				SpriteBatch spriteBatch8 = spriteBatch;
				Vector2 pos7 = new Vector2((float)(GameMain.GraphicsWidth / 2), 20f);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler4.AppendLiteral("Cursor Pos: ");
				defaultInterpolatedStringHandler4.AppendFormatted<Vector2>(this.character.CursorPosition);
				string text4 = defaultInterpolatedStringHandler4.ToStringAndClear();
				Color white4 = Color.White;
				font = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch8, pos7, text4, white4, null, 0, font, ForceUpperCase.Inherit);
				SpriteBatch spriteBatch9 = spriteBatch;
				Vector2 pos8 = new Vector2((float)(GameMain.GraphicsWidth / 2), 40f);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("Cursor Screen Pos: ");
				defaultInterpolatedStringHandler5.AppendFormatted<Vector2>(PlayerInput.MousePosition);
				string text5 = defaultInterpolatedStringHandler5.ToStringAndClear();
				Color white5 = Color.White;
				font = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch9, pos8, text5, white5, null, 0, font, ForceUpperCase.Inherit);
				PhysicsBody collider = this.character.AnimController.Collider;
				Vector2 colliderDrawPos = this.SimToScreen(collider.SimPosition);
				Vector2 forward = Vector2.Transform(Vector2.UnitY, Matrix.CreateRotationZ(collider.Rotation));
				Vector2 endPos = this.SimToScreen(collider.SimPosition + forward * collider.Radius);
				GUI.DrawLine(spriteBatch, colliderDrawPos, endPos, GUIStyle.Green, 0f, 1f);
				GUI.DrawLine(spriteBatch, colliderDrawPos, this.SimToScreen(collider.SimPosition + forward * 0.25f), Color.Blue, 0f, 1f);
				Vector2 left = forward.Left();
				GUI.DrawLine(spriteBatch, colliderDrawPos, this.SimToScreen(collider.SimPosition + left * 0.25f), GUIStyle.Red, 0f, 1f);
				spriteBatch.DrawCircle(colliderDrawPos, (endPos - colliderDrawPos).Length(), 40, GUIStyle.Green, 1f);
				SpriteBatch spriteBatch10 = spriteBatch;
				Vector2 pos9 = new Vector2((float)(GameMain.GraphicsWidth - 300), 0f);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler6.AppendLiteral("Collider rotation: ");
				defaultInterpolatedStringHandler6.AppendFormatted<float>(MathHelper.ToDegrees(MathUtils.WrapAngleTwoPi(collider.Rotation)));
				string text6 = defaultInterpolatedStringHandler6.ToStringAndClear();
				Color white6 = Color.White;
				font = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch10, pos9, text6, white6, null, 0, font, ForceUpperCase.Inherit);
			}
			spriteBatch.End();
		}

		// Token: 0x06004935 RID: 18741 RVA: 0x002818AC File Offset: 0x0027FAAC
		private void UpdateJointCreation()
		{
			if (this.jointCreationMode == CharacterEditorScreen.JointCreationMode.None)
			{
				this.jointStartLimb = null;
				this.jointEndLimb = null;
				this.anchor1Pos = null;
				return;
			}
			if (this.editJoints)
			{
				LimbJoint selectedJoint = this.selectedJoints.LastOrDefault<LimbJoint>();
				if (selectedJoint == null)
				{
					this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
					return;
				}
				if (this.jointCreationMode == CharacterEditorScreen.JointCreationMode.Create)
				{
					if (this.spriteSheetRect.Contains(PlayerInput.MousePosition))
					{
						this.jointEndLimb = this.GetClosestLimbOnSpritesheet(PlayerInput.MousePosition, (Limb l) => l != null && l != this.jointStartLimb && l.ActiveSprite != null);
						if (this.jointEndLimb != null && PlayerInput.PrimaryMouseButtonClicked())
						{
							Vector2 anchor = (this.anchor1Pos != null) ? (this.anchor1Pos.Value / this.spriteSheetZoom) : Vector2.Zero;
							anchor.X = -anchor.X;
							Vector2 anchor2 = (this.GetLimbSpritesheetRect(this.jointEndLimb).Center.ToVector2() - PlayerInput.MousePosition) / this.spriteSheetZoom;
							anchor2.X = -anchor2.X;
							this.CreateJoint(this.jointStartLimb.Params.ID, this.jointEndLimb.Params.ID, new Vector2?(anchor), new Vector2?(anchor2));
							this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
							return;
						}
					}
					else
					{
						this.jointEndLimb = this.GetClosestLimbOnRagdoll(PlayerInput.MousePosition, (Limb l) => l != null && l != this.jointStartLimb && l.ActiveSprite != null);
						if (this.jointEndLimb != null && PlayerInput.PrimaryMouseButtonClicked())
						{
							Vector2 anchor3 = ConvertUnits.ToDisplayUnits(this.jointEndLimb.body.FarseerBody.GetLocalPoint(this.ScreenToSim(PlayerInput.MousePosition)));
							this.CreateJoint(this.jointStartLimb.Params.ID, this.jointEndLimb.Params.ID, this.anchor1Pos, new Vector2?(anchor3));
							this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
							return;
						}
					}
				}
				else
				{
					this.jointStartLimb = selectedJoint.LimbB;
					if (this.spriteSheetRect.Contains(PlayerInput.MousePosition))
					{
						this.anchor1Pos = new Vector2?(this.GetLimbSpritesheetRect(this.jointStartLimb).Center.ToVector2() - PlayerInput.MousePosition);
					}
					else
					{
						this.anchor1Pos = new Vector2?(ConvertUnits.ToDisplayUnits(this.jointStartLimb.body.FarseerBody.GetLocalPoint(this.ScreenToSim(PlayerInput.MousePosition))));
					}
					if (PlayerInput.PrimaryMouseButtonClicked())
					{
						this.jointCreationMode = CharacterEditorScreen.JointCreationMode.Create;
						return;
					}
				}
			}
			else if (this.editLimbs)
			{
				if (this.selectedLimbs.Any<Limb>())
				{
					if (this.spriteSheetRect.Contains(PlayerInput.MousePosition))
					{
						if (this.jointCreationMode == CharacterEditorScreen.JointCreationMode.Create)
						{
							this.jointEndLimb = this.GetClosestLimbOnSpritesheet(PlayerInput.MousePosition, (Limb l) => l != null && l != this.jointStartLimb && l.ActiveSprite != null && !l.Hidden);
							if (this.jointEndLimb != null && PlayerInput.PrimaryMouseButtonClicked())
							{
								Vector2 anchor4 = (this.anchor1Pos != null) ? (this.anchor1Pos.Value / this.spriteSheetZoom) : Vector2.Zero;
								anchor4.X = -anchor4.X;
								Vector2 anchor5 = (this.GetLimbSpritesheetRect(this.jointEndLimb).Center.ToVector2() - PlayerInput.MousePosition) / this.spriteSheetZoom;
								anchor5.X = -anchor5.X;
								this.CreateJoint(this.jointStartLimb.Params.ID, this.jointEndLimb.Params.ID, new Vector2?(anchor4), new Vector2?(anchor5));
								this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
								return;
							}
						}
						else if (PlayerInput.PrimaryMouseButtonClicked())
						{
							this.jointStartLimb = this.GetClosestLimbOnSpritesheet(PlayerInput.MousePosition, (Limb l) => this.selectedLimbs.Contains(l));
							if (this.jointStartLimb != null)
							{
								this.anchor1Pos = new Vector2?(this.GetLimbSpritesheetRect(this.jointStartLimb).Center.ToVector2() - PlayerInput.MousePosition);
								this.jointCreationMode = CharacterEditorScreen.JointCreationMode.Create;
								return;
							}
						}
					}
					else if (this.jointCreationMode == CharacterEditorScreen.JointCreationMode.Create)
					{
						this.jointEndLimb = this.GetClosestLimbOnRagdoll(PlayerInput.MousePosition, (Limb l) => l != null && l != this.jointStartLimb && l.ActiveSprite != null && !l.Hidden);
						if (this.jointEndLimb != null && PlayerInput.PrimaryMouseButtonClicked())
						{
							Vector2 anchor6 = this.anchor1Pos ?? Vector2.Zero;
							Vector2 anchor7 = ConvertUnits.ToDisplayUnits(this.jointEndLimb.body.FarseerBody.GetLocalPoint(this.ScreenToSim(PlayerInput.MousePosition)));
							this.CreateJoint(this.jointStartLimb.Params.ID, this.jointEndLimb.Params.ID, new Vector2?(anchor6), new Vector2?(anchor7));
							this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
							return;
						}
					}
					else if (PlayerInput.PrimaryMouseButtonClicked())
					{
						this.jointStartLimb = this.GetClosestLimbOnRagdoll(PlayerInput.MousePosition, (Limb l) => this.selectedLimbs.Contains(l) && !l.Hidden);
						if (this.jointStartLimb != null)
						{
							this.anchor1Pos = new Vector2?(ConvertUnits.ToDisplayUnits(this.jointStartLimb.body.FarseerBody.GetLocalPoint(this.ScreenToSim(PlayerInput.MousePosition))));
							this.jointCreationMode = CharacterEditorScreen.JointCreationMode.Create;
							return;
						}
					}
				}
				else
				{
					this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
				}
			}
		}

		// Token: 0x06004936 RID: 18742 RVA: 0x00281DF4 File Offset: 0x0027FFF4
		private void UpdateLimbCreation()
		{
			if (!this.isDrawingLimb)
			{
				this.newLimbRect = Rectangle.Empty;
				return;
			}
			if (!this.editLimbs)
			{
				this.SetToggle(this.limbsToggle, true);
			}
			if (PlayerInput.PrimaryMouseButtonHeld())
			{
				if (this.newLimbRect == Rectangle.Empty)
				{
					this.newLimbRect = new Rectangle((int)PlayerInput.MousePosition.X, (int)PlayerInput.MousePosition.Y, 0, 0);
				}
				else
				{
					this.newLimbRect.Size = new Point((int)PlayerInput.MousePosition.X - this.newLimbRect.X, (int)PlayerInput.MousePosition.Y - this.newLimbRect.Y);
				}
				this.newLimbRect.Size = new Point(Math.Max(this.newLimbRect.Width, 2), Math.Max(this.newLimbRect.Height, 2));
			}
			if (PlayerInput.PrimaryMouseButtonClicked())
			{
				this.newLimbRect.Location = new Point(this.newLimbRect.X - 30, this.newLimbRect.Y - 20);
				this.newLimbRect = this.newLimbRect.Divide(this.spriteSheetZoom);
				this.CreateNewLimb(this.newLimbRect);
				this.isDrawingLimb = false;
				this.newLimbRect = Rectangle.Empty;
			}
		}

		// Token: 0x06004937 RID: 18743 RVA: 0x00281F44 File Offset: 0x00280144
		private void CopyLimb(Limb limb)
		{
			if (limb == null)
			{
				return;
			}
			Rectangle rect = limb.ActiveSprite.SourceRect;
			RagdollParams.SpriteParams spriteParams = limb.Params.GetSprite();
			XName name = "limb";
			object[] array = new object[5];
			array[0] = new XAttribute("id", this.RagdollParams.Limbs.Last<RagdollParams.LimbParams>().ID + 1);
			array[1] = new XAttribute("radius", limb.Params.Radius);
			array[2] = new XAttribute("width", limb.Params.Width);
			array[3] = new XAttribute("height", limb.Params.Height);
			int num = 4;
			XName name2 = "sprite";
			object[] array2 = new object[2];
			array2[0] = new XAttribute("texture", spriteParams.Texture);
			int num2 = 1;
			XName name3 = "sourcerect";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 4);
			defaultInterpolatedStringHandler.AppendFormatted<int>(rect.X);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(rect.Y);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(rect.Size.X);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(rect.Size.Y);
			array2[num2] = new XAttribute(name3, defaultInterpolatedStringHandler.ToStringAndClear());
			array[num] = new XElement(name2, array2);
			ContentXElement newLimbElement = new XElement(name, array).FromPackage(this.character.Prefab.ContentPackage);
			this.CreateLimb(newLimbElement);
		}

		// Token: 0x06004938 RID: 18744 RVA: 0x002820EC File Offset: 0x002802EC
		private void CreateNewLimb(Rectangle sourceRect)
		{
			XName name = "limb";
			object[] array = new object[4];
			array[0] = new XAttribute("id", this.RagdollParams.Limbs.Last<RagdollParams.LimbParams>().ID + 1);
			array[1] = new XAttribute("width", (float)sourceRect.Width * this.RagdollParams.TextureScale);
			array[2] = new XAttribute("height", (float)sourceRect.Height * this.RagdollParams.TextureScale);
			int num = 3;
			XName name2 = "sprite";
			object[] array2 = new object[2];
			array2[0] = new XAttribute("texture", this.RagdollParams.Limbs.First<RagdollParams.LimbParams>().GetSprite().Texture);
			int num2 = 1;
			XName name3 = "sourcerect";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 4);
			defaultInterpolatedStringHandler.AppendFormatted<int>(sourceRect.X);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sourceRect.Y);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sourceRect.Width);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sourceRect.Height);
			array2[num2] = new XAttribute(name3, defaultInterpolatedStringHandler.ToStringAndClear());
			array[num] = new XElement(name2, array2);
			ContentXElement newLimbElement = new XElement(name, array).FromPackage(this.character.Prefab.ContentPackage);
			this.CreateLimb(newLimbElement);
			this.lockSpriteOriginToggle.Selected = false;
			this.recalculateColliderToggle.Selected = true;
		}

		// Token: 0x06004939 RID: 18745 RVA: 0x00282284 File Offset: 0x00280484
		private void CreateLimb(ContentXElement newElement)
		{
			ContentXElement contentXElement = this.RagdollParams.MainElement;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				DebugConsole.ThrowError("Main element null! Failed to create a limb.", null, null, false, false);
				return;
			}
			ContentXElement lastElement = this.RagdollParams.MainElement.GetChildElements("limb").LastOrDefault<ContentXElement>();
			contentXElement = null;
			if (lastElement != contentXElement)
			{
				lastElement.AddAfterSelf(newElement);
			}
			else
			{
				this.RagdollParams.MainElement.AddFirst(newElement);
			}
			RagdollParams.LimbParams newLimbParams = new RagdollParams.LimbParams(newElement, this.RagdollParams);
			this.RagdollParams.Limbs.Add(newLimbParams);
			this.character.AnimController.Recreate(null);
			this.CreateTextures();
			this.TeleportTo(this.spawnPosition);
			this.ClearWidgets();
			this.ClearSelection();
			this.selectedLimbs.Add(this.character.AnimController.Limbs.Single((Limb l) => l.Params == newLimbParams));
			this.ResetParamsEditor();
		}

		// Token: 0x0600493A RID: 18746 RVA: 0x0028238C File Offset: 0x0028058C
		private void CreateJoint(int fromLimb, int toLimb, Vector2? anchor1 = null, Vector2? anchor2 = null)
		{
			if (this.RagdollParams.Joints.Any((RagdollParams.JointParams j) => j.Limb1 == fromLimb && j.Limb2 == toLimb))
			{
				DebugConsole.ThrowErrorLocalized(CharacterEditorScreen.GetCharacterEditorTranslation("ExistingJointFound").Replace("[limbid1]", fromLimb.ToString(), StringComparison.Ordinal).Replace("[limbid2]", toLimb.ToString(), StringComparison.Ordinal), null, null, false, false);
				return;
			}
			ContentXElement contentXElement = this.RagdollParams.MainElement;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				DebugConsole.ThrowError("The main element of the ragdoll params is null! Failed to create a joint.", null, null, false, false);
				return;
			}
			Vector2 a = anchor1 ?? Vector2.Zero;
			Vector2 a2 = anchor2 ?? Vector2.Zero;
			ContentXElement newJointElement = new XElement("joint", new object[]
			{
				new XAttribute("limb1", fromLimb),
				new XAttribute("limb2", toLimb),
				new XAttribute("limb1anchor", a.X.Format(2) + ", " + a.Y.Format(2)),
				new XAttribute("limb2anchor", a2.X.Format(2) + ", " + a2.Y.Format(2))
			}).FromPackage(this.character.Prefab.ContentPackage);
			ContentXElement lastJointElement = this.RagdollParams.MainElement.GetChildElements("joint").LastOrDefault<ContentXElement>() ?? this.RagdollParams.MainElement.GetChildElements("limb").LastOrDefault<ContentXElement>();
			contentXElement = null;
			if (lastJointElement == contentXElement)
			{
				DebugConsole.ThrowErrorLocalized(CharacterEditorScreen.GetCharacterEditorTranslation("CantAddJointsNoLimbElements"), null, null, false, false);
				return;
			}
			lastJointElement.AddAfterSelf(newJointElement);
			RagdollParams.JointParams newJointParams = new RagdollParams.JointParams(newJointElement, this.RagdollParams);
			this.RagdollParams.Joints.Add(newJointParams);
			this.character.AnimController.Recreate(null);
			this.CreateTextures();
			this.TeleportTo(this.spawnPosition);
			this.ClearWidgets();
			this.ClearSelection();
			this.SetToggle(this.jointsToggle, true);
			this.selectedJoints.Add(this.character.AnimController.LimbJoints.Single((LimbJoint j) => j.Params == newJointParams));
		}

		// Token: 0x0600493B RID: 18747 RVA: 0x00282638 File Offset: 0x00280838
		private void DeleteSelected()
		{
			for (int i = 0; i < this.selectedJoints.Count; i++)
			{
				LimbJoint joint = this.selectedJoints[i];
				joint.Params.Element.Remove();
				this.RagdollParams.Joints.Remove(joint.Params);
			}
			List<int> removedIDs = new List<int>();
			for (int j = 0; j < this.selectedLimbs.Count; j++)
			{
				if (this.character.IsHumanoid)
				{
					DebugConsole.ThrowErrorLocalized(CharacterEditorScreen.GetCharacterEditorTranslation("HumanoidLimbDeletionDisabled"), null, null, false, false);
					break;
				}
				Limb limb = this.selectedLimbs[j];
				if (limb == this.character.AnimController.MainLimb)
				{
					DebugConsole.ThrowError("Can't remove the main limb, because it will cause unreveratable issues.", null, null, false, false);
				}
				else
				{
					removedIDs.Add(limb.Params.ID);
					limb.Params.Element.Remove();
					this.RagdollParams.Limbs.Remove(limb.Params);
				}
			}
			Dictionary<int, int> renamedIDs = new Dictionary<int, int>();
			for (int k = 0; k < this.RagdollParams.Limbs.Count; k++)
			{
				int oldID = this.RagdollParams.Limbs[k].ID;
				int newID = k;
				if (oldID != newID)
				{
					RagdollParams.LimbParams limbParams = this.RagdollParams.Limbs[k];
					limbParams.ID = newID;
					limbParams.Name = limbParams.GenerateName();
					renamedIDs.Add(oldID, newID);
				}
			}
			List<RagdollParams.JointParams> jointsToRemove = new List<RagdollParams.JointParams>();
			for (int l = 0; l < this.RagdollParams.Joints.Count; l++)
			{
				RagdollParams.JointParams joint2 = this.RagdollParams.Joints[l];
				if (removedIDs.Contains(joint2.Limb1) || removedIDs.Contains(joint2.Limb2))
				{
					jointsToRemove.Add(joint2);
				}
				else
				{
					bool rename = false;
					int newID2;
					if (renamedIDs.TryGetValue(joint2.Limb1, out newID2))
					{
						joint2.Limb1 = newID2;
						rename = true;
					}
					int newID3;
					if (renamedIDs.TryGetValue(joint2.Limb2, out newID3))
					{
						joint2.Limb2 = newID3;
						rename = true;
					}
					if (rename)
					{
						joint2.Name = joint2.GenerateName();
					}
				}
			}
			foreach (RagdollParams.JointParams jointParam in jointsToRemove)
			{
				jointParam.Element.Remove();
				this.RagdollParams.Joints.Remove(jointParam);
			}
			this.RecreateRagdoll(null);
		}

		// Token: 0x0600493C RID: 18748 RVA: 0x002828E0 File Offset: 0x00280AE0
		private void CalculateMovementLimits()
		{
			this.min = (from w in this.MiddleWall.Entities
			select w.Rect.Left into p
			orderby p
			select p).First<int>();
			this.max = (from w in this.MiddleWall.Entities
			select w.Rect.Right into p
			orderby p
			select p).Last<int>();
		}

		// Token: 0x170012A7 RID: 4775
		// (get) Token: 0x0600493D RID: 18749 RVA: 0x002829A9 File Offset: 0x00280BA9
		private CharacterEditorScreen.WallGroup MiddleWall
		{
			get
			{
				return this.wallGroups[1];
			}
		}

		// Token: 0x170012A8 RID: 4776
		// (get) Token: 0x0600493E RID: 18750 RVA: 0x002829B3 File Offset: 0x00280BB3
		private IEnumerable<MapEntity> AllStructures
		{
			get
			{
				return this.wallGroups.SelectMany((CharacterEditorScreen.WallGroup c) => c.Entities);
			}
		}

		// Token: 0x0600493F RID: 18751 RVA: 0x002829E0 File Offset: 0x00280BE0
		private void CloneWalls()
		{
			CharacterEditorScreen.WallGroup originalWall = this.wallGroups[0];
			int moveAmount = originalWall.Entities.FirstOrDefault((MapEntity e) => e is Structure).Rect.Width;
			for (int i = 1; i <= 2; i++)
			{
				this.wallGroups[i] = originalWall.Clone();
				foreach (MapEntity entity in this.wallGroups[i].Entities)
				{
					entity.Move(new Vector2((float)(moveAmount * i), 0f), true);
				}
			}
		}

		// Token: 0x06004940 RID: 18752 RVA: 0x00282AA4 File Offset: 0x00280CA4
		private void UpdateWalls(bool right)
		{
			int moveAmount = this.wallGroups[0].Entities.FirstOrDefault((MapEntity e) => e is Structure).Rect.Width;
			int amount = right ? moveAmount : (-moveAmount);
			foreach (CharacterEditorScreen.WallGroup wallGroup in this.wallGroups)
			{
				foreach (MapEntity entity in wallGroup.Entities)
				{
					entity.Move(new Vector2((float)amount, 0f), true);
				}
			}
			this.CalculateMovementLimits();
			GameMain.World.ProcessChanges();
		}

		// Token: 0x06004941 RID: 18753 RVA: 0x00282B78 File Offset: 0x00280D78
		private void SetWallCollisions(bool enabled)
		{
			if (!this.isEndlessRunner)
			{
				return;
			}
			this.wallCollisionsEnabled = enabled;
			Category collisionCategory = enabled ? Category.Cat1 : Category.None;
			this.AllStructures.ForEach(delegate(MapEntity w)
			{
				Structure structure = w as Structure;
				if (structure == null)
				{
					return;
				}
				structure.SetCollisionCategory(collisionCategory);
			});
			GameMain.World.ProcessChanges();
		}

		// Token: 0x170012A9 RID: 4777
		// (get) Token: 0x06004942 RID: 18754 RVA: 0x00282BCC File Offset: 0x00280DCC
		private List<Identifier> VisibleSpecies
		{
			get
			{
				if (this.visibleSpecies == null)
				{
					this.visibleSpecies = (from p in CharacterPrefab.Prefabs.Where(new Func<CharacterPrefab, bool>(this.ShowCreature))
					orderby p.Identifier
					select p.Identifier).ToList<Identifier>();
				}
				return this.visibleSpecies;
			}
		}

		// Token: 0x06004943 RID: 18755 RVA: 0x00282C50 File Offset: 0x00280E50
		private bool ShowCreature(CharacterPrefab prefab)
		{
			Identifier speciesName = prefab.Identifier;
			return speciesName == CharacterPrefab.HumanSpeciesName || !this.VanillaCharacters.Contains(prefab.ContentFile) || CreatureMetrics.UnlockAll || CreatureMetrics.Unlocked.Contains(speciesName);
		}

		// Token: 0x170012AA RID: 4778
		// (get) Token: 0x06004944 RID: 18756 RVA: 0x00282C9D File Offset: 0x00280E9D
		private IEnumerable<CharacterFile> VanillaCharacters
		{
			get
			{
				if (this.vanillaCharacters == null)
				{
					this.vanillaCharacters = GameMain.VanillaContent.GetFiles<CharacterFile>();
				}
				return this.vanillaCharacters;
			}
		}

		// Token: 0x06004945 RID: 18757 RVA: 0x00282CBD File Offset: 0x00280EBD
		private Identifier GetNextCharacterIdentifier()
		{
			this.GetCurrentCharacterIndex();
			this.IncreaseIndex();
			this.currentCharacterIdentifier = this.VisibleSpecies[this.characterIndex];
			return this.currentCharacterIdentifier;
		}

		// Token: 0x06004946 RID: 18758 RVA: 0x00282CE8 File Offset: 0x00280EE8
		private Identifier GetPreviousCharacterIdentifier()
		{
			this.GetCurrentCharacterIndex();
			this.ReduceIndex();
			this.currentCharacterIdentifier = this.VisibleSpecies[this.characterIndex];
			return this.currentCharacterIdentifier;
		}

		// Token: 0x06004947 RID: 18759 RVA: 0x00282D13 File Offset: 0x00280F13
		private void GetCurrentCharacterIndex()
		{
			this.characterIndex = this.VisibleSpecies.IndexOf(this.character.SpeciesName);
		}

		// Token: 0x06004948 RID: 18760 RVA: 0x00282D31 File Offset: 0x00280F31
		private void IncreaseIndex()
		{
			this.characterIndex++;
			if (this.characterIndex > this.VisibleSpecies.Count - 1)
			{
				this.characterIndex = 0;
			}
		}

		// Token: 0x06004949 RID: 18761 RVA: 0x00282D5D File Offset: 0x00280F5D
		private void ReduceIndex()
		{
			this.characterIndex--;
			if (this.characterIndex < 0)
			{
				this.characterIndex = this.VisibleSpecies.Count - 1;
			}
		}

		// Token: 0x0600494A RID: 18762 RVA: 0x00282D8C File Offset: 0x00280F8C
		public Character SpawnCharacter(Identifier speciesName, RagdollParams ragdoll = null)
		{
			DebugConsole.NewMessage(CharacterEditorScreen.GetCharacterEditorTranslation("TryingToSpawnCharacter").Replace("[config]", speciesName.ToString(), StringComparison.Ordinal), new Color?(Color.HotPink), false);
			this.OnPreSpawn();
			bool followCursor = false;
			if (this.character != null)
			{
				followCursor = this.character.FollowCursor;
				this.RagdollParams.ClearHistory();
				this.CurrentAnimation.ClearHistory();
				if (!this.character.Removed)
				{
					this.character.Remove();
				}
				this.character = null;
			}
			if (speciesName == CharacterPrefab.HumanSpeciesName && !this.selectedJob.IsEmpty)
			{
				CharacterInfo characterInfo = new CharacterInfo(speciesName, "", "", JobPrefab.Prefabs[this.selectedJob.Value], 0, Rand.RandSync.Unsynced, default(Identifier));
				this.character = Character.Create(speciesName, this.spawnPosition, ToolBox.RandomSeed(8), characterInfo, 0, false, false, true, ragdoll, true, true);
				this.character.GiveJobItems(false, null);
				this.HideWearables();
				if (this.displayWearables)
				{
					this.ShowWearables();
				}
				this.selectedJob = characterInfo.Job.Prefab.Identifier;
			}
			else
			{
				this.character = Character.Create(speciesName, this.spawnPosition, ToolBox.RandomSeed(8), null, 0, false, false, true, ragdoll, true, true);
				this.selectedJob = Identifier.Empty;
			}
			if (this.character != null)
			{
				this.character.FollowCursor = followCursor;
			}
			if (this.character == null)
			{
				if (this.currentCharacterIdentifier == speciesName)
				{
					return null;
				}
				this.SpawnCharacter(this.currentCharacterIdentifier, null);
			}
			this.OnPostSpawn();
			return this.character;
		}

		// Token: 0x0600494B RID: 18763 RVA: 0x00282F44 File Offset: 0x00281144
		private void OnPreSpawn()
		{
			this.cameraOffset = Vector2.Zero;
			WayPoint wayPoint = null;
			if (!this.isEndlessRunner)
			{
				wayPoint = WayPoint.GetRandom(SpawnType.Human, null, Submarine.MainSub, false, null, false);
			}
			if (wayPoint == null)
			{
				wayPoint = WayPoint.GetRandom(SpawnType.Human, null, Submarine.MainSub, false, null, false);
			}
			this.spawnPosition = wayPoint.WorldPosition;
		}

		// Token: 0x0600494C RID: 18764 RVA: 0x00282F98 File Offset: 0x00281198
		private void OnPostSpawn()
		{
			this.currentCharacterIdentifier = this.character.SpeciesName;
			this.GetCurrentCharacterIndex();
			this.character.Submarine = Submarine.MainSub;
			this.character.AnimController.forceStanding = this.character.AnimController.CanWalk;
			this.character.AnimController.ForceSelectAnimationType = (this.character.AnimController.CanWalk ? AnimationType.Walk : AnimationType.SwimSlow);
			Character.Controlled = this.character;
			this.SetWallCollisions(this.character.AnimController.forceStanding);
			this.CreateTextures();
			this.CreateGUI();
			this.ClearWidgets();
			this.ClearSelection();
			this.ResetParamsEditor();
			this.CurrentAnimation.StoreSnapshot();
			this.RagdollParams.StoreSnapshot();
			this.Cam.Position = this.character.WorldPosition;
			this.editedCharacters.Add(this.character);
		}

		// Token: 0x0600494D RID: 18765 RVA: 0x0028308F File Offset: 0x0028128F
		private void ClearWidgets()
		{
			Widget.SelectedWidgets.Clear();
			this.animationWidgets.Clear();
			this.jointSelectionWidgets.Clear();
			this.limbEditWidgets.Clear();
		}

		// Token: 0x0600494E RID: 18766 RVA: 0x002830BC File Offset: 0x002812BC
		private void ClearSelection()
		{
			this.selectedLimbs.Clear();
			this.selectedJoints.Clear();
			foreach (Widget w in this.jointSelectionWidgets.Values)
			{
				w.Refresh();
				Widget linkedWidget = w.LinkedWidget;
				if (linkedWidget != null)
				{
					linkedWidget.Refresh();
				}
			}
		}

		// Token: 0x0600494F RID: 18767 RVA: 0x00283144 File Offset: 0x00281344
		private void RecreateRagdoll(RagdollParams ragdoll = null)
		{
			this.RagdollParams.Apply();
			this.character.AnimController.Recreate(ragdoll);
			this.TeleportTo(this.spawnPosition);
			List<RagdollParams.JointParams> selectedJointParams = (from j in this.selectedJoints
			select j.Params).ToList<RagdollParams.JointParams>();
			List<RagdollParams.LimbParams> selectedLimbParams = (from l in this.selectedLimbs
			select l.Params).ToList<RagdollParams.LimbParams>();
			this.CreateTextures();
			this.ClearWidgets();
			this.ClearSelection();
			foreach (LimbJoint joint in this.character.AnimController.LimbJoints)
			{
				if (selectedJointParams.Contains(joint.Params))
				{
					this.selectedJoints.Add(joint);
				}
			}
			foreach (Limb limb in this.character.AnimController.Limbs)
			{
				if (selectedLimbParams.Contains(limb.Params))
				{
					this.selectedLimbs.Add(limb);
				}
			}
			this.ResetParamsEditor();
		}

		// Token: 0x06004950 RID: 18768 RVA: 0x0028327C File Offset: 0x0028147C
		private void TeleportTo(Vector2 position)
		{
			if (this.isEndlessRunner)
			{
				this.character.AnimController.SetPosition(ConvertUnits.ToSimUnits(position), false, true, false, true);
			}
			else
			{
				this.character.TeleportTo(position);
			}
			this.Cam.Position = this.character.WorldPosition;
		}

		// Token: 0x06004951 RID: 18769 RVA: 0x002832D0 File Offset: 0x002814D0
		public bool CreateCharacter(Identifier name, string mainFolder, bool isHumanoid, ContentPackage contentPackage, XElement ragdoll, XElement config = null, IEnumerable<AnimationParams> animations = null)
		{
			CharacterEditorScreen.<>c__DisplayClass116_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass116_0();
			CS$<>8__locals1.name = name;
			CS$<>8__locals1.mainFolder = mainFolder;
			if (CS$<>8__locals1.name.IsEmpty)
			{
				throw new ArgumentException("Name cannot be empty.");
			}
			CS$<>8__locals1.vanilla = GameMain.VanillaContent;
			if (contentPackage == null)
			{
				contentPackage = ContentPackageManager.EnabledPackages.All.LastOrDefault((ContentPackage cp) => cp != CS$<>8__locals1.vanilla);
			}
			if (contentPackage == null)
			{
				DebugConsole.ThrowErrorLocalized(CharacterEditorScreen.GetCharacterEditorTranslation("NoContentPackageSelected"), null, null, false, false);
				return false;
			}
			if (CS$<>8__locals1.vanilla != null && contentPackage == CS$<>8__locals1.vanilla)
			{
				LocalizedString characterEditorTranslation = CharacterEditorScreen.GetCharacterEditorTranslation("CannotEditVanillaCharacters");
				Color color = GUIStyle.Red;
				GUIFont largeFont = GUIStyle.LargeFont;
				GUI.AddMessage(characterEditorTranslation, color, null, true, largeFont);
				return false;
			}
			RegularPackage regular = contentPackage as RegularPackage;
			if (regular != null && !ContentPackageManager.EnabledPackages.Regular.Contains(regular))
			{
				ContentPackageManager.EnabledPackages.EnableRegular(regular);
			}
			GameSettings.SaveCurrentConfig();
			string[] array = new string[2];
			array[0] = CS$<>8__locals1.mainFolder;
			int num = 1;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.name);
			defaultInterpolatedStringHandler.AppendLiteral(".xml");
			array[num] = defaultInterpolatedStringHandler.ToStringAndClear();
			string configFilePath = Path.Combine(array).Replace("\\", "/");
			ContentXElement duplicate = CharacterPrefab.ConfigElements.FirstOrDefault(delegate(ContentXElement e)
			{
				Identifier attributeIdentifier = e.GetAttributeIdentifier("speciesname", Identifier.Empty);
				return attributeIdentifier == CS$<>8__locals1.name;
			});
			XElement overrideElement = null;
			ContentXElement contentXElement = null;
			if (duplicate != contentXElement)
			{
				this.visibleSpecies = null;
				if (!File.Exists(configFilePath))
				{
					overrideElement = new XElement("override");
				}
			}
			if (config == null)
			{
				config = new XElement("Character", new object[]
				{
					new XAttribute("speciesname", CS$<>8__locals1.name),
					new XAttribute("humanoid", isHumanoid),
					new XElement("ragdolls", CS$<>8__locals1.<CreateCharacter>g__CreateRagdollPath|2()),
					new XElement("animations", CS$<>8__locals1.<CreateCharacter>g__CreateAnimationPath|3()),
					new XElement("health"),
					new XElement("ai")
				});
			}
			else
			{
				config.TrySetAttributeValue("speciesname", CS$<>8__locals1.name, StringComparison.OrdinalIgnoreCase);
				config.TrySetAttributeValue("humanoid", isHumanoid, StringComparison.OrdinalIgnoreCase);
				XElement ragdollElement = config.GetChildElement("ragdolls", StringComparison.OrdinalIgnoreCase);
				if (ragdollElement == null)
				{
					config.Add(new XElement("ragdolls", CS$<>8__locals1.<CreateCharacter>g__CreateRagdollPath|2()));
				}
				else
				{
					string path = ragdollElement.GetAttributeString("folder", "");
					if (!string.IsNullOrEmpty(path) && !path.Equals("default", StringComparison.OrdinalIgnoreCase))
					{
						ragdollElement.ReplaceWith(new XElement("ragdolls", CS$<>8__locals1.<CreateCharacter>g__CreateRagdollPath|2()));
					}
				}
				XElement animationElement = config.GetChildElement("animations", StringComparison.OrdinalIgnoreCase);
				if (animationElement == null)
				{
					config.Add(new XElement("animations", CS$<>8__locals1.<CreateCharacter>g__CreateAnimationPath|3()));
				}
				else
				{
					string path2 = animationElement.GetAttributeString("folder", "");
					if (!string.IsNullOrEmpty(path2) && !path2.Equals("default", StringComparison.OrdinalIgnoreCase))
					{
						animationElement.ReplaceWith(new XElement("animations", CS$<>8__locals1.<CreateCharacter>g__CreateAnimationPath|3()));
					}
				}
			}
			if (overrideElement != null)
			{
				overrideElement.Add(config);
				config = overrideElement;
			}
			XDocument doc = new XDocument(new object[]
			{
				config
			});
			ContentPath configFileContentPath = ContentPath.FromRaw(contentPackage, configFilePath);
			Directory.CreateDirectory(Path.GetDirectoryName(configFileContentPath.Value), false);
			doc.SaveSafe(configFileContentPath.Value, SaveOptions.None, false, 0);
			ModProject modProject = new ModProject(contentPackage);
			ModProject.File newFile = ModProject.File.FromPath<CharacterFile>(configFilePath);
			modProject.AddFile(newFile);
			modProject.Save(contentPackage.Path, true);
			Result<ContentPackage, Exception> reloadResult = ContentPackageManager.ReloadContentPackage(contentPackage);
			ContentPackage newPackage;
			if (!reloadResult.TryUnwrapSuccess(out newPackage))
			{
				Exception exception;
				throw new Exception("Failed to reload package", reloadResult.TryUnwrapFailure(out exception) ? exception : null);
			}
			contentPackage = newPackage;
			DebugConsole.NewMessage(CharacterEditorScreen.GetCharacterEditorTranslation("ContentPackageSaved").Replace("[path]", contentPackage.Path, StringComparison.Ordinal), null, false);
			RagdollParams.ClearCache();
			string ragdollPath = RagdollParams.GetDefaultFile(CS$<>8__locals1.name);
			RagdollParams ragdollParams = isHumanoid ? RagdollParams.CreateDefault<HumanRagdollParams>(ragdollPath, CS$<>8__locals1.name, ragdoll) : RagdollParams.CreateDefault<FishRagdollParams>(ragdollPath, CS$<>8__locals1.name, ragdoll);
			AnimationParams.ClearCache();
			string animFolder = AnimationParams.GetFolder(CS$<>8__locals1.name);
			if (animations != null)
			{
				if (!Directory.Exists(animFolder))
				{
					Directory.CreateDirectory(animFolder, false);
				}
				using (IEnumerator<AnimationParams> enumerator = animations.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						AnimationParams animation = enumerator.Current;
						XElement element = animation.MainElement;
						if (element != null)
						{
							element.SetAttributeValue("type", CS$<>8__locals1.name);
							string fullPath = AnimationParams.GetDefaultFilePath(CS$<>8__locals1.name, animation.AnimationType);
							element.Name = AnimationParams.GetDefaultFileName(CS$<>8__locals1.name, animation.AnimationType);
							element.SaveSafe(fullPath, false);
						}
					}
					goto IL_5B0;
				}
			}
			foreach (object obj in Enum.GetValues(typeof(AnimationType)))
			{
				AnimationType animType = (AnimationType)obj;
				switch (animType)
				{
				case AnimationType.Walk:
				case AnimationType.Run:
					if (!ragdollParams.CanWalk)
					{
						continue;
					}
					break;
				case AnimationType.SwimSlow:
				case AnimationType.SwimFast:
					break;
				case AnimationType.Crouch:
					if (!ragdollParams.CanWalk || !isHumanoid)
					{
						continue;
					}
					break;
				default:
					continue;
				}
				Type type = AnimationParams.GetParamTypeFromAnimType(animType, isHumanoid);
				string fullPath2 = AnimationParams.GetDefaultFilePath(CS$<>8__locals1.name, animType);
				AnimationParams.Create(fullPath2, CS$<>8__locals1.name, animType, type);
			}
			IL_5B0:
			if (!this.VisibleSpecies.Contains(CS$<>8__locals1.name))
			{
				this.VisibleSpecies.Add(CS$<>8__locals1.name);
			}
			this.SpawnCharacter(CS$<>8__locals1.name, ragdollParams);
			this.limbPairEditing = false;
			this.limbsToggle.Selected = true;
			this.recalculateColliderToggle.Selected = true;
			this.lockSpriteOriginToggle.Selected = false;
			this.selectedLimbs.Add(this.character.AnimController.Limbs.First<Limb>());
			return true;
		}

		// Token: 0x06004952 RID: 18770 RVA: 0x00283928 File Offset: 0x00281B28
		private void ShowWearables()
		{
			if (this.character.Inventory == null)
			{
				return;
			}
			foreach (Item item in this.character.Inventory.AllItems)
			{
				if (!item.AllowedSlots.Contains(InvSlotType.Head) && !item.AllowedSlots.Contains(InvSlotType.Headset))
				{
					item.Equip(this.character);
				}
			}
		}

		// Token: 0x06004953 RID: 18771 RVA: 0x002839B0 File Offset: 0x00281BB0
		private void HideWearables()
		{
			CharacterInventory inventory = this.character.Inventory;
			if (inventory == null)
			{
				return;
			}
			inventory.AllItemsMod.ForEach(delegate(Item i)
			{
				i.Unequip(this.character);
			});
		}

		// Token: 0x06004954 RID: 18772 RVA: 0x002839D8 File Offset: 0x00281BD8
		private void CreateGUI()
		{
			if (this.rightArea != null)
			{
				this.rightArea.RectTransform.Parent = null;
			}
			if (this.centerArea != null)
			{
				this.centerArea.RectTransform.Parent = null;
			}
			if (this.leftArea != null)
			{
				this.leftArea.RectTransform.Parent = null;
			}
			this.rightArea = new GUILayoutGroup(new RectTransform(new Vector2(0.15f, 1f), this.Frame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), false, Anchor.BottomRight)
			{
				RelativeSpacing = 0.02f
			};
			this.centerArea = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.95f), this.Frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point((int)((float)this.rightArea.RectTransform.ScaledSize.X + this.rightArea.RectTransform.RelativeOffset.X * (float)this.rightArea.RectTransform.Parent.ScaledSize.X + (float)((int)(20f * GUI.xScale))), (int)(20f * GUI.yScale))
			}, null, null)
			{
				CanBeFocused = false
			};
			this.leftArea = new GUILayoutGroup(new RectTransform(new Vector2(0.15f, 0.95f), this.Frame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), false, Anchor.BottomLeft)
			{
				RelativeSpacing = 0.02f
			};
			Vector2 toggleSize = new Vector2(1f, 0.03f);
			this.CreateFileEditPanel();
			this.CreateOptionsPanel(toggleSize);
			this.CreateCharacterSelectionPanel();
			if (this.rightArea.RectTransform.Children.Sum((RectTransform c) => c.Rect.Height) > GameMain.GraphicsHeight)
			{
				(from c in this.fileEditPanel.GetAllChildren()
				where c is GUIButton
				select c).ForEach(delegate(GUIComponent b)
				{
					b.RectTransform.MinSize = (((GUIButton)b).Frame.RectTransform.MinSize = b.RectTransform.MinSize.Multiply(new Vector2(1f, 0.75f)));
				});
				this.fileEditPanel.RectTransform.MinSize = new Point(0, (int)((float)this.fileEditPanel.GetChild<GUILayoutGroup>().RectTransform.Children.Sum((RectTransform c) => c.Rect.Height) / CharacterEditorScreen.innerScale.Y));
				(from c in this.optionsPanel.GetAllChildren()
				where c is GUITickBox
				select c).ForEach(delegate(GUIComponent t)
				{
					t.RectTransform.MinSize = t.RectTransform.MinSize.Multiply(new Vector2(1f, 0.75f));
				});
				this.optionsPanel.RectTransform.MinSize = new Point(0, (int)((float)this.optionsPanel.GetChild<GUILayoutGroup>().RectTransform.Children.Sum((RectTransform c) => c.Rect.Height) / CharacterEditorScreen.innerScale.Y));
				this.rightArea.Recalculate();
			}
			this.CreateButtonsPanel();
			this.CreateModesPanel(toggleSize);
			this.CreateMinorModesPanel(toggleSize);
			this.CreateContextualControls();
			this.totalMassText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.01f), this.rightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.GetTotalMassText(), null, null, Alignment.Left, false, "", null);
		}

		// Token: 0x06004955 RID: 18773 RVA: 0x00283DF0 File Offset: 0x00281FF0
		private void CreateMinorModesPanel(Vector2 toggleSize)
		{
			this.minorModesPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.25f), this.leftArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup layoutGroup = new GUILayoutGroup(new RectTransform(CharacterEditorScreen.innerScale, this.minorModesPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = 2,
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(0.03f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CharacterEditorScreen.GetCharacterEditorTranslation("MinorModesTitle");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			this.paramsToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("ShowParameters"), null, "")
			{
				Selected = this.showParamsEditor
			};
			this.paramsToggle.OnSelected = delegate(GUITickBox box)
			{
				this.showParamsEditor = box.Selected;
				return true;
			};
			this.spritesheetToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("ShowSpriteSheet"), null, "")
			{
				Selected = this.showSpritesheet
			};
			this.spritesheetToggle.OnSelected = delegate(GUITickBox box)
			{
				this.showSpritesheet = box.Selected;
				return true;
			};
			this.showCollidersToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("ShowColliders"), null, "")
			{
				Selected = this.showColliders,
				OnSelected = delegate(GUITickBox box)
				{
					this.showColliders = box.Selected;
					return true;
				}
			};
			this.ikToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("EditIKTargets"), null, "")
			{
				Selected = this.editIK
			};
			this.ikToggle.OnSelected = delegate(GUITickBox box)
			{
				this.editIK = box.Selected;
				return true;
			};
			this.skeletonToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("DrawSkeleton"), null, "")
			{
				Selected = this.drawSkeleton
			};
			this.skeletonToggle.OnSelected = delegate(GUITickBox box)
			{
				this.drawSkeleton = box.Selected;
				return true;
			};
			this.lightsToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("EnableLights"), null, "")
			{
				Selected = GameMain.LightManager.LightingEnabled
			};
			this.lightsToggle.OnSelected = delegate(GUITickBox box)
			{
				GameMain.LightManager.LightingEnabled = box.Selected;
				return true;
			};
			this.damageModifiersToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("DrawDamageModifiers"), null, "")
			{
				Selected = this.drawDamageModifiers
			};
			this.damageModifiersToggle.OnSelected = delegate(GUITickBox box)
			{
				this.drawDamageModifiers = box.Selected;
				return true;
			};
			this.minorModesToggle = new CharacterEditorScreen.ToggleButton(new RectTransform(new Vector2(0.08f, 1f), this.minorModesPanel.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), CharacterEditorScreen.Direction.Left);
			this.minorModesPanel.RectTransform.MinSize = new Point(0, (int)((float)layoutGroup.RectTransform.Children.Sum((RectTransform c) => c.MinSize.Y + layoutGroup.AbsoluteSpacing) * 1.2f));
		}

		// Token: 0x06004956 RID: 18774 RVA: 0x002842AC File Offset: 0x002824AC
		private void CreateModesPanel(Vector2 toggleSize)
		{
			this.modesPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.2f), this.leftArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup layoutGroup = new GUILayoutGroup(new RectTransform(CharacterEditorScreen.innerScale, this.modesPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = 2,
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(0.03f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CharacterEditorScreen.GetCharacterEditorTranslation("ModesPanel");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			this.characterInfoToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("EditCharacter"), null, "")
			{
				Selected = this.editCharacterInfo
			};
			this.ragdollToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("EditRagdoll"), null, "")
			{
				Selected = this.editRagdoll
			};
			this.limbsToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("EditLimbs"), null, "")
			{
				Selected = this.editLimbs
			};
			this.jointsToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("EditJoints"), null, "")
			{
				Selected = this.editJoints
			};
			this.animsToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("EditAnimations"), null, "")
			{
				Selected = this.editAnimations
			};
			this.animsToggle.OnSelected = delegate(GUITickBox box)
			{
				this.editAnimations = box.Selected;
				if (this.editAnimations)
				{
					this.SetToggle(this.limbsToggle, false);
					this.SetToggle(this.jointsToggle, false);
					this.SetToggle(this.ragdollToggle, false);
					this.SetToggle(this.characterInfoToggle, false);
					this.spritesheetToggle.Selected = false;
				}
				this.ClearSelection();
				this.ResetParamsEditor();
				return true;
			};
			this.limbsToggle.OnSelected = delegate(GUITickBox box)
			{
				this.editLimbs = box.Selected;
				if (this.editLimbs)
				{
					this.SetToggle(this.animsToggle, false);
					this.SetToggle(this.jointsToggle, false);
					this.SetToggle(this.ragdollToggle, false);
					this.SetToggle(this.characterInfoToggle, false);
					this.spritesheetToggle.Selected = true;
				}
				this.ClearSelection();
				this.ResetParamsEditor();
				return true;
			};
			this.jointsToggle.OnSelected = delegate(GUITickBox box)
			{
				this.editJoints = box.Selected;
				if (this.editJoints)
				{
					this.SetToggle(this.limbsToggle, false);
					this.SetToggle(this.animsToggle, false);
					this.SetToggle(this.ragdollToggle, false);
					this.SetToggle(this.characterInfoToggle, false);
					this.ikToggle.Selected = false;
					this.spritesheetToggle.Selected = true;
				}
				this.ClearSelection();
				this.ResetParamsEditor();
				return true;
			};
			this.ragdollToggle.OnSelected = delegate(GUITickBox box)
			{
				this.editRagdoll = box.Selected;
				if (this.editRagdoll)
				{
					this.SetToggle(this.limbsToggle, false);
					this.SetToggle(this.animsToggle, false);
					this.SetToggle(this.jointsToggle, false);
					this.SetToggle(this.characterInfoToggle, false);
					this.paramsToggle.Selected = true;
				}
				this.ClearSelection();
				this.ResetParamsEditor();
				return true;
			};
			this.characterInfoToggle.OnSelected = delegate(GUITickBox box)
			{
				this.editCharacterInfo = box.Selected;
				if (this.editCharacterInfo)
				{
					this.SetToggle(this.limbsToggle, false);
					this.SetToggle(this.animsToggle, false);
					this.SetToggle(this.ragdollToggle, false);
					this.SetToggle(this.jointsToggle, false);
					this.paramsToggle.Selected = true;
				}
				this.ClearSelection();
				this.ResetParamsEditor();
				return true;
			};
			this.modesToggle = new CharacterEditorScreen.ToggleButton(new RectTransform(new Vector2(0.08f, 1f), this.modesPanel.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), CharacterEditorScreen.Direction.Left);
			this.modesPanel.RectTransform.MinSize = new Point(0, (int)((float)layoutGroup.RectTransform.Children.Sum((RectTransform c) => c.MinSize.Y + layoutGroup.AbsoluteSpacing) * 1.2f));
		}

		// Token: 0x06004957 RID: 18775 RVA: 0x0028467C File Offset: 0x0028287C
		private void SetToggle(GUITickBox toggle, bool value)
		{
			if (toggle.Selected != value)
			{
				if (value)
				{
					toggle.Box.Flash(new Color?(GUIStyle.Green), 1.5f, true, false, null);
				}
				else
				{
					toggle.Box.Flash(new Color?(GUIStyle.Red), 1.5f, true, false, null);
				}
			}
			toggle.Selected = value;
		}

		// Token: 0x06004958 RID: 18776 RVA: 0x002846F4 File Offset: 0x002828F4
		private void CreateButtonsPanel()
		{
			this.buttonsPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), this.leftArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			Vector2 buttonSize = new Vector2(1f, 0.45f);
			GUIFrame parent = new GUIFrame(new RectTransform(new Vector2(0.85f, 0.7f), this.buttonsPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GUIButton reloadTexturesButton = new GUIButton(new RectTransform(buttonSize, parent.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("ReloadTextures"), Alignment.Center, "", null);
			GUIButton guibutton = reloadTexturesButton;
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object userData)
			{
				foreach (Limb limb in this.character.AnimController.Limbs)
				{
					if (limb != null)
					{
						Sprite activeSprite = limb.ActiveSprite;
						if (activeSprite != null)
						{
							activeSprite.ReloadTexture();
						}
						limb.WearingItems.ForEach(delegate(WearableSprite i)
						{
							i.Sprite.ReloadTexture();
						});
						limb.OtherWearables.ForEach(delegate(WearableSprite w)
						{
							w.Sprite.ReloadTexture();
						});
					}
				}
				this.CreateTextures();
				return true;
			}));
			GUIButton recreateButton = new GUIButton(new RectTransform(buttonSize, parent.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("RecreateRagdoll"), Alignment.Center, "", null)
			{
				ToolTip = CharacterEditorScreen.GetCharacterEditorTranslation("RecreateRagdollTooltip"),
				OnClicked = delegate(GUIButton button, object data)
				{
					this.RecreateRagdoll(null);
					this.character.AnimController.ResetLimbs();
					return true;
				}
			};
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				reloadTexturesButton.TextBlock,
				recreateButton.TextBlock
			});
			this.buttonsPanelToggle = new CharacterEditorScreen.ToggleButton(new RectTransform(new Vector2(0.08f, 1f), this.buttonsPanel.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), CharacterEditorScreen.Direction.Left);
			this.buttonsPanel.RectTransform.MinSize = new Point(0, (int)((float)parent.RectTransform.Children.Sum((RectTransform c) => c.MinSize.Y) * 1.5f));
		}

		// Token: 0x06004959 RID: 18777 RVA: 0x00284950 File Offset: 0x00282B50
		private void CreateOptionsPanel(Vector2 toggleSize)
		{
			this.optionsPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.3f), this.rightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup layoutGroup = new GUILayoutGroup(new RectTransform(CharacterEditorScreen.innerScale, this.optionsPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = 2,
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(0.03f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CharacterEditorScreen.GetCharacterEditorTranslation("OptionsPanel");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			this.freezeToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("Freeze"), null, "")
			{
				Selected = this.isFrozen,
				OnSelected = delegate(GUITickBox box)
				{
					this.isFrozen = box.Selected;
					return true;
				}
			};
			GUITickBox guitickBox = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("AutoFreeze"), null, "");
			guitickBox.Selected = this.autoFreeze;
			guitickBox.OnSelected = delegate(GUITickBox box)
			{
				this.autoFreeze = box.Selected;
				return true;
			};
			GUITickBox guitickBox2 = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("LimbPairEditing"), null, "");
			guitickBox2.Selected = this.limbPairEditing;
			guitickBox2.Enabled = this.character.IsHumanoid;
			guitickBox2.OnSelected = delegate(GUITickBox box)
			{
				this.limbPairEditing = box.Selected;
				return true;
			};
			this.animTestPoseToggle = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("AnimationTestPose"), null, "")
			{
				Selected = this.character.AnimController.AnimationTestPose,
				Enabled = true,
				OnSelected = delegate(GUITickBox box)
				{
					this.character.AnimController.AnimationTestPose = box.Selected;
					return true;
				}
			};
			GUITickBox guitickBox3 = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("AutoMove"), null, "");
			guitickBox3.Selected = (this.character.OverrideMovement != null);
			guitickBox3.OnSelected = delegate(GUITickBox box)
			{
				this.character.OverrideMovement = (box.Selected ? new Vector2?(new Vector2(1f, 0f)) : null);
				return true;
			};
			GUITickBox guitickBox4 = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("FollowCursor"), null, "");
			guitickBox4.Selected = this.character.FollowCursor;
			guitickBox4.OnSelected = delegate(GUITickBox box)
			{
				this.character.FollowCursor = box.Selected;
				return true;
			};
			GUITickBox guitickBox5 = new GUITickBox(new RectTransform(toggleSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("EditBackgroundColor"), null, "");
			guitickBox5.Selected = this.displayBackgroundColor;
			guitickBox5.OnSelected = delegate(GUITickBox box)
			{
				this.displayBackgroundColor = box.Selected;
				return true;
			};
			this.optionsToggle = new CharacterEditorScreen.ToggleButton(new RectTransform(new Vector2(0.08f, 1f), this.optionsPanel.RectTransform, Anchor.CenterLeft, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal), CharacterEditorScreen.Direction.Right);
			this.optionsPanel.RectTransform.MinSize = new Point(0, (int)((float)layoutGroup.RectTransform.Children.Sum((RectTransform c) => c.MinSize.Y + layoutGroup.AbsoluteSpacing) * 1.2f));
		}

		// Token: 0x0600495A RID: 18778 RVA: 0x00284DE8 File Offset: 0x00282FE8
		private void CreateContextualControls()
		{
			CharacterEditorScreen.<>c__DisplayClass175_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass175_0();
			CS$<>8__locals1.<>4__this = this;
			Point elementSize = new Point(120, 20).Multiply(GUI.Scale);
			int textAreaHeight = 20;
			this.backgroundColorPanel = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.1f), this.centerArea.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(10, 0).Multiply(GUI.Scale)
			}, null, null)
			{
				CanBeFocused = false
			};
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(500, 80).Multiply(GUI.Scale), this.backgroundColorPanel.RectTransform, Anchor.TopRight, null, ScaleBasis.Normal, false), null, new Color?(Color.Black * 0.4f));
			new GUITextBlock(new RectTransform(new Vector2(0.2f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(80, 26)
			}, CharacterEditorScreen.GetCharacterEditorTranslation("BackgroundColor") + ":", new Color?(Color.WhiteSmoke), null, Alignment.Left, false, "", null);
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.7f, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(20, 0).Multiply(GUI.Scale)
			}, true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			GUIComponent[] fields = new GUIComponent[4];
			string[] colorComponentLabels = new string[]
			{
				"R",
				"G",
				"B"
			};
			for (int i = 2; i >= 0; i--)
			{
				GUIFrame element2 = new GUIFrame(new RectTransform(new Vector2(0.3f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(40, 0),
					MaxSize = new Point(100, 50)
				}, null, new Color?(Color.Black * 0.6f));
				RectTransform rectT = new RectTransform(new Vector2(0.3f, 1f), element2.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text = colorComponentLabels[i];
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock colorLabel = new GUITextBlock(rectT, text, null, smallFont, Alignment.CenterLeft, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element2.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, new float?(0.25f), GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				numberInput.MinValueInt = new int?(0);
				numberInput.MaxValueInt = new int?(255);
				numberInput.Font = GUIStyle.SmallFont;
				switch (i)
				{
				case 0:
				{
					colorLabel.TextColor = GUIStyle.Red;
					numberInput.IntValue = (int)this.backgroundColor.R;
					GUINumberInput guinumberInput = numberInput;
					Delegate onValueChanged = guinumberInput.OnValueChanged;
					GUINumberInput.OnValueChangedHandler b4;
					if ((b4 = CS$<>8__locals1.<>9__7) == null)
					{
						b4 = (CS$<>8__locals1.<>9__7 = delegate(GUINumberInput numInput)
						{
							CS$<>8__locals1.<>4__this.backgroundColor.R = (byte)numInput.IntValue;
						});
					}
					guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(onValueChanged, b4);
					break;
				}
				case 1:
				{
					colorLabel.TextColor = GUIStyle.Green;
					numberInput.IntValue = (int)this.backgroundColor.G;
					GUINumberInput guinumberInput2 = numberInput;
					Delegate onValueChanged2 = guinumberInput2.OnValueChanged;
					GUINumberInput.OnValueChangedHandler b2;
					if ((b2 = CS$<>8__locals1.<>9__8) == null)
					{
						b2 = (CS$<>8__locals1.<>9__8 = delegate(GUINumberInput numInput)
						{
							CS$<>8__locals1.<>4__this.backgroundColor.G = (byte)numInput.IntValue;
						});
					}
					guinumberInput2.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(onValueChanged2, b2);
					break;
				}
				case 2:
				{
					colorLabel.TextColor = Color.DeepSkyBlue;
					numberInput.IntValue = (int)this.backgroundColor.B;
					GUINumberInput guinumberInput3 = numberInput;
					Delegate onValueChanged3 = guinumberInput3.OnValueChanged;
					GUINumberInput.OnValueChangedHandler b3;
					if ((b3 = CS$<>8__locals1.<>9__9) == null)
					{
						b3 = (CS$<>8__locals1.<>9__9 = delegate(GUINumberInput numInput)
						{
							CS$<>8__locals1.<>4__this.backgroundColor.B = (byte)numInput.IntValue;
						});
					}
					guinumberInput3.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(onValueChanged3, b3);
					break;
				}
				}
			}
			this.spriteSheetControls = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.1f), this.centerArea.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, null, null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup layoutGroupSpriteSheet = new GUILayoutGroup(new RectTransform(Vector2.One, this.spriteSheetControls.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = 5,
				CanBeFocused = false
			};
			new GUITextBlock(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupSpriteSheet.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("SpriteSheetZoom") + ":", new Color?(Color.White), null, Alignment.Left, false, "", null);
			GUIFrame spriteSheetControlElement = new GUIFrame(new RectTransform(new Point(elementSize.X * 2, textAreaHeight), layoutGroupSpriteSheet.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
			this.CalculateSpritesheetZoom();
			this.spriteSheetZoomBar = new GUIScrollBar(new RectTransform(new Vector2(0.69f, 1f), spriteSheetControlElement.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), 0.2f, null, "GUISlider", null)
			{
				BarScroll = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(this.spriteSheetMinZoom, this.spriteSheetMaxZoom, this.spriteSheetZoom)),
				Step = 0.01f,
				OnMoved = delegate(GUIScrollBar scrollBar, float value)
				{
					CS$<>8__locals1.<>4__this.spriteSheetZoom = MathHelper.Lerp(CS$<>8__locals1.<>4__this.spriteSheetMinZoom, CS$<>8__locals1.<>4__this.spriteSheetMaxZoom, value);
					return true;
				}
			};
			new GUIButton(new RectTransform(new Vector2(0.3f, 1.25f), spriteSheetControlElement.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("Reset"), Alignment.Center, "GUIButtonFreeScale", null).OnClicked = delegate(GUIButton box, object data)
			{
				CS$<>8__locals1.<>4__this.spriteSheetZoom = Math.Min(1f, CS$<>8__locals1.<>4__this.spriteSheetMaxZoom);
				CS$<>8__locals1.<>4__this.spriteSheetZoomBar.BarScroll = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(CS$<>8__locals1.<>4__this.spriteSheetMinZoom, CS$<>8__locals1.<>4__this.spriteSheetMaxZoom, CS$<>8__locals1.<>4__this.spriteSheetZoom));
				return true;
			};
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupSpriteSheet.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("HideBodySprites"), null, "");
			guitickBox.TextColor = Color.White;
			guitickBox.Selected = this.hideBodySheet;
			guitickBox.OnSelected = delegate(GUITickBox box)
			{
				CS$<>8__locals1.<>4__this.hideBodySheet = box.Selected;
				return true;
			};
			GUITickBox guitickBox2 = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupSpriteSheet.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("ShowWearables"), null, "");
			guitickBox2.TextColor = Color.White;
			guitickBox2.Selected = this.displayWearables;
			guitickBox2.OnSelected = delegate(GUITickBox box)
			{
				CS$<>8__locals1.<>4__this.displayWearables = box.Selected;
				if (CS$<>8__locals1.<>4__this.displayWearables)
				{
					CS$<>8__locals1.<>4__this.ShowWearables();
				}
				else
				{
					CS$<>8__locals1.<>4__this.HideWearables();
				}
				return true;
			};
			GUITickBox guitickBox3 = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupSpriteSheet.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("Unrestrict"), null, "");
			guitickBox3.TextColor = Color.White;
			guitickBox3.Selected = this.unrestrictSpritesheet;
			guitickBox3.OnSelected = delegate(GUITickBox box)
			{
				CS$<>8__locals1.<>4__this.SetSpritesheetRestriction(box.Selected);
				return true;
			};
			this.resetSpriteOrientationButtonParent = new GUIFrame(new RectTransform(new Vector2(0.1f, 0.025f), this.centerArea.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(0, -5).Multiply(GUI.Scale),
				RelativeOffset = new Vector2(-0.05f, 0f)
			}, null, null)
			{
				CanBeFocused = false
			};
			new GUIButton(new RectTransform(Vector2.One, this.resetSpriteOrientationButtonParent.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("Reset"), Alignment.Center, "GUIButtonFreeScale", null).OnClicked = delegate(GUIButton box, object data)
			{
				IEnumerable<Limb> limbs = CS$<>8__locals1.<>4__this.selectedLimbs;
				if (limbs.None(null))
				{
					limbs = CS$<>8__locals1.<>4__this.selectedJoints.Select(delegate(LimbJoint j)
					{
						if (!PlayerInput.KeyDown(Keys.LeftAlt))
						{
							return j.LimbA;
						}
						return j.LimbB;
					});
				}
				foreach (Limb limb in limbs)
				{
					CS$<>8__locals1.<>4__this.TryUpdateSubParam(limb.Params, "spriteorientation".ToIdentifier(), float.NaN);
					if (CS$<>8__locals1.<>4__this.limbPairEditing)
					{
						CharacterEditorScreen <>4__this = CS$<>8__locals1.<>4__this;
						Limb limb2 = limb;
						Action<Limb> updateAction;
						if ((updateAction = CS$<>8__locals1.<>9__17) == null)
						{
							updateAction = (CS$<>8__locals1.<>9__17 = delegate(Limb l)
							{
								CS$<>8__locals1.<>4__this.TryUpdateSubParam(l.Params, "spriteorientation".ToIdentifier(), float.NaN);
							});
						}
						<>4__this.UpdateOtherLimbs(limb2, updateAction);
					}
				}
				return true;
			};
			this.limbControls = new GUIFrame(new RectTransform(Vector2.One, this.centerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup layoutGroupLimbControls = new GUILayoutGroup(new RectTransform(Vector2.One, this.limbControls.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = false
			};
			this.lockSpriteOriginToggle = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupLimbControls.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("LockSpriteOrigin"), null, "")
			{
				TextColor = Color.White,
				Selected = this.lockSpriteOrigin,
				OnSelected = delegate(GUITickBox box)
				{
					CS$<>8__locals1.<>4__this.lockSpriteOrigin = box.Selected;
					return true;
				}
			};
			GUITickBox guitickBox4 = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupLimbControls.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("LockSpritePosition"), null, "");
			guitickBox4.TextColor = Color.White;
			guitickBox4.Selected = this.lockSpritePosition;
			guitickBox4.OnSelected = delegate(GUITickBox box)
			{
				CS$<>8__locals1.<>4__this.lockSpritePosition = box.Selected;
				return true;
			};
			GUITickBox guitickBox5 = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupLimbControls.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("LockSpriteSize"), null, "");
			guitickBox5.TextColor = Color.White;
			guitickBox5.Selected = this.lockSpriteSize;
			guitickBox5.OnSelected = delegate(GUITickBox box)
			{
				CS$<>8__locals1.<>4__this.lockSpriteSize = box.Selected;
				return true;
			};
			this.recalculateColliderToggle = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupLimbControls.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("AdjustCollider"), null, "")
			{
				TextColor = Color.White,
				Selected = this.recalculateCollider,
				OnSelected = delegate(GUITickBox box)
				{
					CS$<>8__locals1.<>4__this.recalculateCollider = box.Selected;
					CS$<>8__locals1.<>4__this.showCollidersToggle.Selected = CS$<>8__locals1.<>4__this.recalculateCollider;
					return true;
				}
			};
			GUITickBox guitickBox6 = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupLimbControls.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("OnlyShowSelectedLimbs"), null, "");
			guitickBox6.TextColor = Color.White;
			guitickBox6.Selected = this.onlyShowSourceRectForSelectedLimbs;
			guitickBox6.OnSelected = delegate(GUITickBox box)
			{
				CS$<>8__locals1.<>4__this.onlyShowSourceRectForSelectedLimbs = box.Selected;
				return true;
			};
			Point sliderSize = new Point(300, 20).Multiply(GUI.Scale);
			this.jointControls = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.075f), this.centerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup layoutGroupJoints = new GUILayoutGroup(new RectTransform(Vector2.One, this.jointControls.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = false
			};
			this.copyJointsToggle = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupJoints.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("CopyJointSettings"), null, "")
			{
				ToolTip = CharacterEditorScreen.GetCharacterEditorTranslation("CopyJointSettingsTooltip"),
				Selected = this.copyJointSettings,
				TextColor = (this.copyJointSettings ? GUIStyle.Red : Color.White),
				OnSelected = delegate(GUITickBox box)
				{
					CS$<>8__locals1.<>4__this.copyJointSettings = box.Selected;
					box.TextColor = (CS$<>8__locals1.<>4__this.copyJointSettings ? GUIStyle.Red : Color.White);
					return true;
				}
			};
			this.ragdollControls = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.25f), this.centerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup layoutGroupRagdoll = new GUILayoutGroup(new RectTransform(Vector2.One, this.ragdollControls.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = false
			};
			GUITickBox uniformScalingToggle = new GUITickBox(new RectTransform(new Point(elementSize.X, textAreaHeight), layoutGroupRagdoll.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("UniformScale"), null, "")
			{
				Selected = this.uniformScaling,
				OnSelected = delegate(GUITickBox box)
				{
					CS$<>8__locals1.<>4__this.uniformScaling = box.Selected;
					return true;
				}
			};
			uniformScalingToggle.TextColor = Color.White;
			GUIFrame jointScaleElement = new GUIFrame(new RectTransform(sliderSize + new Point(0, textAreaHeight), layoutGroupRagdoll.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
			CharacterEditorScreen.<>c__DisplayClass175_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(new Point(elementSize.X, textAreaHeight), jointScaleElement.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("JointScale"));
			defaultInterpolatedStringHandler.AppendLiteral(": ");
			defaultInterpolatedStringHandler.AppendFormatted(this.RagdollParams.JointScale.FormatDoubleDecimal());
			CS$<>8__locals2.jointScaleText = new GUITextBlock(rectT2, defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.WhiteSmoke), null, Alignment.Center, false, "", null);
			GUIFrame limbScaleElement = new GUIFrame(new RectTransform(sliderSize + new Point(0, textAreaHeight), layoutGroupRagdoll.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
			CharacterEditorScreen.<>c__DisplayClass175_0 CS$<>8__locals3 = CS$<>8__locals1;
			RectTransform rectT3 = new RectTransform(new Point(elementSize.X, textAreaHeight), limbScaleElement.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 2);
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("LimbScale"));
			defaultInterpolatedStringHandler2.AppendLiteral(": ");
			defaultInterpolatedStringHandler2.AppendFormatted(this.RagdollParams.LimbScale.FormatDoubleDecimal());
			CS$<>8__locals3.limbScaleText = new GUITextBlock(rectT3, defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.WhiteSmoke), null, Alignment.Center, false, "", null);
			this.jointScaleBar = new GUIScrollBar(new RectTransform(sliderSize, jointScaleElement.RectTransform, Anchor.BottomLeft, null, ScaleBasis.Normal, false), 0.1f, null, "GUISlider", null)
			{
				BarScroll = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0.1f, 2f, this.RagdollParams.JointScale)),
				Step = 0.001f,
				OnMoved = delegate(GUIScrollBar scrollBar, float value)
				{
					float v = MathHelper.Lerp(0.1f, 2f, value);
					base.<CreateContextualControls>g__UpdateJointScale|0(v);
					if (CS$<>8__locals1.<>4__this.uniformScaling)
					{
						base.<CreateContextualControls>g__UpdateLimbScale|1(v);
						CS$<>8__locals1.<>4__this.limbScaleBar.BarScroll = value;
					}
					return true;
				}
			};
			this.limbScaleBar = new GUIScrollBar(new RectTransform(sliderSize, limbScaleElement.RectTransform, Anchor.BottomLeft, null, ScaleBasis.Normal, false), 0.1f, null, "GUISlider", null)
			{
				BarScroll = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0.1f, 2f, this.RagdollParams.LimbScale)),
				Step = 0.001f,
				OnMoved = delegate(GUIScrollBar scrollBar, float value)
				{
					float v = MathHelper.Lerp(0.1f, 2f, value);
					base.<CreateContextualControls>g__UpdateLimbScale|1(v);
					if (CS$<>8__locals1.<>4__this.uniformScaling)
					{
						base.<CreateContextualControls>g__UpdateJointScale|0(v);
						CS$<>8__locals1.<>4__this.jointScaleBar.BarScroll = value;
					}
					return true;
				}
			};
			GUIButton bar = this.limbScaleBar.Bar;
			bar.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(bar.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object data)
			{
				CS$<>8__locals1.<>4__this.RecreateRagdoll(null);
				CS$<>8__locals1.<>4__this.RagdollParams.StoreSnapshot();
				return true;
			}));
			GUIButton bar2 = this.jointScaleBar.Bar;
			bar2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(bar2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object data)
			{
				if (CS$<>8__locals1.<>4__this.uniformScaling)
				{
					CS$<>8__locals1.<>4__this.RecreateRagdoll(null);
				}
				CS$<>8__locals1.<>4__this.RagdollParams.StoreSnapshot();
				return true;
			}));
			Point buttonSize = new Point(200, 40).Multiply(GUI.Scale);
			this.extraRagdollControls = new GUIFrame(new RectTransform(new Point(buttonSize.X, buttonSize.Y * 4), this.centerArea.RectTransform, Anchor.BottomRight, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(30, 0).Multiply(GUI.Scale),
				MinSize = new Point(0, 120)
			}, null, new Color?(Color.Black))
			{
				CanBeFocused = false
			};
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(Vector2.One * 0.95f, this.extraRagdollControls.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = 5
			};
			List<GUIButton> buttons = GUI.CreateButtons(4, new Vector2(1f, 0.25f), paddedFrame.RectTransform, Anchor.TopCenter, null, null, null, 0, 0f, null, 0, 0f, false, Alignment.Center, "GUIButtonSmallFreeScale");
			this.deleteSelectedButton = buttons[0];
			this.deleteSelectedButton.Text = CharacterEditorScreen.GetCharacterEditorTranslation("DeleteSelected");
			this.deleteSelectedButton.OnClicked = delegate(GUIButton button, object data)
			{
				CS$<>8__locals1.<>4__this.DeleteSelected();
				return true;
			};
			this.duplicateLimbButton = buttons[1];
			this.duplicateLimbButton.Text = CharacterEditorScreen.GetCharacterEditorTranslation("DuplicateLimb");
			this.duplicateLimbButton.OnClicked = delegate(GUIButton button, object data)
			{
				CS$<>8__locals1.<>4__this.CopyLimb(CS$<>8__locals1.<>4__this.selectedLimbs.FirstOrDefault<Limb>());
				return true;
			};
			this.createJointButton = buttons[2];
			this.createJointButton.Text = CharacterEditorScreen.GetCharacterEditorTranslation("CreateJoint");
			this.createJointButton.OnClicked = delegate(GUIButton button, object data)
			{
				CS$<>8__locals1.<>4__this.ToggleJointCreationMode();
				return true;
			};
			this.createLimbButton = buttons[3];
			this.createLimbButton.Text = CharacterEditorScreen.GetCharacterEditorTranslation("CreateLimb");
			this.createLimbButton.OnClicked = delegate(GUIButton button, object data)
			{
				CS$<>8__locals1.<>4__this.ToggleLimbCreationMode();
				return true;
			};
			GUITextBlock.AutoScaleAndNormalize(from b in buttons
			select b.TextBlock, true, false, null);
			this.animationControls = new GUIFrame(new RectTransform(Vector2.One, this.centerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup layoutGroupAnimation = new GUILayoutGroup(new RectTransform(Vector2.One, this.animationControls.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = false
			};
			GUIFrame animationSelectionElement = new GUIFrame(new RectTransform(new Point(elementSize.X * 2 - (int)(5f * GUI.xScale), elementSize.Y), layoutGroupAnimation.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
			GUITextBlock animationSelectionText = new GUITextBlock(new RectTransform(new Point(elementSize.X, elementSize.Y), animationSelectionElement.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CharacterEditorScreen.GetCharacterEditorTranslation("SelectedAnimation"), new Color?(Color.WhiteSmoke), null, Alignment.CenterRight, false, "", null);
			this.animSelection = new GUIDropDown(new RectTransform(new Point((int)(150f * GUI.xScale), elementSize.Y), animationSelectionElement.RectTransform, Anchor.Center, new Pivot?(Pivot.CenterLeft), ScaleBasis.Normal, false), null, 5, "", false, false, Alignment.CenterLeft, 1f);
			if (this.character.AnimController.CanWalk)
			{
				this.animSelection.AddItem(AnimationType.Walk.ToString(), AnimationType.Walk, null, null, null);
				this.animSelection.AddItem(AnimationType.Run.ToString(), AnimationType.Run, null, null, null);
			}
			this.animSelection.AddItem(AnimationType.SwimSlow.ToString(), AnimationType.SwimSlow, null, null, null);
			this.animSelection.AddItem(AnimationType.SwimFast.ToString(), AnimationType.SwimFast, null, null, null);
			if (this.character.AnimController.CanWalk && this.character.IsHumanoid)
			{
				this.animSelection.AddItem(AnimationType.Crouch.ToString(), AnimationType.Crouch, null, null, null);
			}
			if (this.character.AnimController.ForceSelectAnimationType == AnimationType.NotDefined)
			{
				this.animSelection.SelectItem(this.character.AnimController.CanWalk ? AnimationType.Walk : AnimationType.SwimSlow);
			}
			else
			{
				this.animSelection.SelectItem(this.character.AnimController.ForceSelectAnimationType);
			}
			GUIDropDown guidropDown = this.animSelection;
			guidropDown.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(guidropDown.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent element, object data)
			{
				AnimationType previousAnim = CS$<>8__locals1.<>4__this.character.AnimController.ForceSelectAnimationType;
				CS$<>8__locals1.<>4__this.character.AnimController.ForceSelectAnimationType = (AnimationType)data;
				switch (CS$<>8__locals1.<>4__this.character.AnimController.ForceSelectAnimationType)
				{
				case AnimationType.Walk:
				case AnimationType.Run:
				case AnimationType.Crouch:
					CS$<>8__locals1.<>4__this.character.AnimController.forceStanding = true;
					CS$<>8__locals1.<>4__this.character.ForceRun = (CS$<>8__locals1.<>4__this.character.AnimController.ForceSelectAnimationType == AnimationType.Run);
					if (!CS$<>8__locals1.<>4__this.wallCollisionsEnabled)
					{
						CS$<>8__locals1.<>4__this.SetWallCollisions(true);
					}
					if (previousAnim != AnimationType.Walk && previousAnim != AnimationType.Run && previousAnim != AnimationType.Crouch)
					{
						CS$<>8__locals1.<>4__this.TeleportTo(CS$<>8__locals1.<>4__this.spawnPosition);
					}
					break;
				case AnimationType.SwimSlow:
					CS$<>8__locals1.<>4__this.character.AnimController.forceStanding = false;
					CS$<>8__locals1.<>4__this.character.ForceRun = false;
					if (CS$<>8__locals1.<>4__this.wallCollisionsEnabled)
					{
						CS$<>8__locals1.<>4__this.SetWallCollisions(false);
					}
					break;
				case AnimationType.SwimFast:
					CS$<>8__locals1.<>4__this.character.AnimController.forceStanding = false;
					CS$<>8__locals1.<>4__this.character.ForceRun = true;
					if (CS$<>8__locals1.<>4__this.wallCollisionsEnabled)
					{
						CS$<>8__locals1.<>4__this.SetWallCollisions(false);
					}
					break;
				default:
					throw new NotImplementedException();
				}
				CS$<>8__locals1.<>4__this.ResetParamsEditor();
				return true;
			}));
		}

		// Token: 0x0600495B RID: 18779 RVA: 0x002864F4 File Offset: 0x002846F4
		private void CreateCharacterSelectionPanel()
		{
			this.characterSelectionPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.2f), this.rightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(CharacterEditorScreen.innerScale, this.characterSelectionPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CharacterEditorScreen.GetCharacterEditorTranslation("CharacterPanel");
			GUIFont largeFont = GUIStyle.LargeFont;
			GUITextBlock characterLabel = new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			GUIDropDown characterDropDown = new GUIDropDown(new RectTransform(new Vector2(1f, 0.2f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.2f)
			}, null, 8, null, false, false, Alignment.CenterLeft, 1f);
			characterDropDown.ListBox.Color = new Color(characterDropDown.ListBox.Color.R, characterDropDown.ListBox.Color.G, characterDropDown.ListBox.Color.B, byte.MaxValue);
			foreach (CharacterPrefab prefab in from p in CharacterPrefab.Prefabs
			orderby p.Identifier descending
			select p)
			{
				Identifier speciesName = prefab.Identifier;
				if (this.ShowCreature(prefab))
				{
					characterDropDown.AddItem(speciesName.Value.CapitaliseFirstInvariant(), speciesName, null, null, null).SetAsFirstChild();
				}
				else if (!CreatureMetrics.Encountered.Contains(speciesName))
				{
					GUIDropDown guidropDown = characterDropDown;
					LocalizedString text2 = TextManager.Get("hiddensubmarines");
					object userData = Identifier.Empty;
					LocalizedString toolTip = null;
					Color? textColor = new Color?(Color.Gray * 0.75f);
					GUIComponent element = guidropDown.AddItem(text2, userData, toolTip, null, textColor);
					element.SetAsLastChild();
					element.Enabled = false;
				}
			}
			characterDropDown.SelectItem(this.currentCharacterIdentifier);
			characterDropDown.OnSelected = delegate(GUIComponent component, object data)
			{
				Identifier characterIdentifier = (Identifier)data;
				if (characterIdentifier.IsEmpty)
				{
					return true;
				}
				try
				{
					this.SpawnCharacter(characterIdentifier, null);
				}
				catch (Exception e)
				{
					this.<CreateCharacterSelectionPanel>g__HandleSpawnException|176_2(characterIdentifier, e);
				}
				return true;
			};
			if (this.currentCharacterIdentifier == CharacterPrefab.HumanSpeciesName)
			{
				GUIDropDown jobDropDown = new GUIDropDown(new RectTransform(new Vector2(1f, 0.15f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0f, 0.45f)
				}, null, 8, null, false, false, Alignment.CenterLeft, 1f);
				jobDropDown.ListBox.Color = new Color(jobDropDown.ListBox.Color.R, jobDropDown.ListBox.Color.G, jobDropDown.ListBox.Color.B, byte.MaxValue);
				jobDropDown.AddItem("None", null, null, null, null);
				(from j in JobPrefab.Prefabs
				where !j.HiddenJob
				select j).ForEach(delegate(JobPrefab j)
				{
					jobDropDown.AddItem(j.Name, j.Identifier, null, null, null);
				});
				jobDropDown.SelectItem(this.selectedJob);
				jobDropDown.OnSelected = delegate(GUIComponent component, object data)
				{
					Identifier identifier;
					if (data is Identifier)
					{
						Identifier jobIdentifier = (Identifier)data;
						identifier = jobIdentifier;
					}
					else
					{
						identifier = Identifier.Empty;
					}
					Identifier newJob = identifier;
					if (newJob != this.selectedJob)
					{
						this.selectedJob = newJob;
						this.SpawnCharacter(this.currentCharacterIdentifier, null);
					}
					return true;
				};
			}
			GUIFrame charButtons = new GUIFrame(new RectTransform(new Vector2(1f, 0.25f), content.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIButton prevCharacterButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), charButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("PreviousCharacter"), Alignment.Center, "", null);
			prevCharacterButton.TextBlock.AutoScaleHorizontal = true;
			GUIButton guibutton = prevCharacterButton;
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object obj)
			{
				Identifier characterIdentifier = this.GetPreviousCharacterIdentifier();
				try
				{
					this.SpawnCharacter(characterIdentifier, null);
				}
				catch (Exception e)
				{
					this.<CreateCharacterSelectionPanel>g__HandleSpawnException|176_2(characterIdentifier, e);
				}
				return true;
			}));
			GUIButton nextCharacterButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), charButtons.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("NextCharacter"), Alignment.Center, "", null);
			prevCharacterButton.TextBlock.AutoScaleHorizontal = true;
			GUIButton guibutton2 = nextCharacterButton;
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object obj)
			{
				Identifier characterIdentifier = this.GetNextCharacterIdentifier();
				try
				{
					this.SpawnCharacter(characterIdentifier, null);
				}
				catch (Exception e)
				{
					this.<CreateCharacterSelectionPanel>g__HandleSpawnException|176_2(characterIdentifier, e);
				}
				return true;
			}));
			charButtons.RectTransform.MinSize = new Point(0, prevCharacterButton.RectTransform.MinSize.Y);
			this.characterPanelToggle = new CharacterEditorScreen.ToggleButton(new RectTransform(new Vector2(0.08f, 1f), this.characterSelectionPanel.RectTransform, Anchor.CenterLeft, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal), CharacterEditorScreen.Direction.Right);
			this.characterSelectionPanel.RectTransform.MinSize = new Point(0, (int)((float)content.RectTransform.Children.Sum((RectTransform c) => c.MinSize.Y) * 1.2f));
		}

		// Token: 0x0600495C RID: 18780 RVA: 0x00286B94 File Offset: 0x00284D94
		private void CreateFileEditPanel()
		{
			CharacterEditorScreen.<>c__DisplayClass177_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass177_0();
			CS$<>8__locals1.<>4__this = this;
			Vector2 buttonSize = new Vector2(1f, 0.04f);
			this.fileEditPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.4f), this.rightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			CS$<>8__locals1.layoutGroup = new GUILayoutGroup(new RectTransform(CharacterEditorScreen.innerScale, this.fileEditPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = 1,
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(0.03f, 0f), CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CharacterEditorScreen.GetCharacterEditorTranslation("FileEditPanel");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			new GUIFrame(new RectTransform(buttonSize / 2f, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			GUIButton guibutton = new GUIButton(new RectTransform(buttonSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.saveall"), Alignment.Center, "", null)
			{
				Color = GUIStyle.Green
			};
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object userData)
			{
				GUIFont font;
				if (CS$<>8__locals1.<>4__this.VanillaCharacters.Contains(CharacterPrefab.Prefabs[CS$<>8__locals1.<>4__this.currentCharacterIdentifier].ContentFile))
				{
					LocalizedString characterEditorTranslation = CharacterEditorScreen.GetCharacterEditorTranslation("CannotEditVanillaCharacters");
					Color color = GUIStyle.Red;
					font = GUIStyle.LargeFont;
					GUI.AddMessage(characterEditorTranslation, color, null, true, font);
					return false;
				}
				ContentPath texturePath = ContentPath.FromRaw(CS$<>8__locals1.<>4__this.character.Prefab.ContentPackage, CS$<>8__locals1.<>4__this.RagdollParams.Texture);
				if (!CS$<>8__locals1.<>4__this.character.IsHuman && (texturePath.IsNullOrWhiteSpace() || !File.Exists(texturePath.Value)))
				{
					DebugConsole.ThrowError("Invalid texture path: " + CS$<>8__locals1.<>4__this.RagdollParams.Texture, null, null, false, false);
					return false;
				}
				CS$<>8__locals1.<>4__this.character.Params.Save(null);
				LocalizedString message = CharacterEditorScreen.GetCharacterEditorTranslation("CharacterSavedTo").Replace("[path]", CS$<>8__locals1.<>4__this.CharacterParams.Path.Value, StringComparison.Ordinal);
				Color color2 = GUIStyle.Green;
				font = GUIStyle.Font;
				GUI.AddMessage(message, color2, new float?((float)5), true, font);
				CS$<>8__locals1.<>4__this.character.AnimController.SaveRagdoll(null);
				LocalizedString message2 = CharacterEditorScreen.GetCharacterEditorTranslation("RagdollSavedTo").Replace("[path]", CS$<>8__locals1.<>4__this.RagdollParams.Path.Value, StringComparison.Ordinal);
				Color color3 = GUIStyle.Green;
				font = GUIStyle.Font;
				GUI.AddMessage(message2, color3, new float?((float)5), true, font);
				CS$<>8__locals1.<>4__this.AnimParams.ForEach(delegate(AnimationParams p)
				{
					p.Save(null, null);
				});
				return true;
			}));
			new GUIFrame(new RectTransform(buttonSize / 2f, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			CS$<>8__locals1.messageBoxRelSize = new Vector2(0.5f, 0.7f);
			GUIButton saveRagdollButton = new GUIButton(new RectTransform(buttonSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("SaveRagdoll"), Alignment.Center, "", null);
			GUIButton guibutton2 = saveRagdollButton;
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object userData)
			{
				CharacterEditorScreen.<>c__DisplayClass177_1 CS$<>8__locals2 = new CharacterEditorScreen.<>c__DisplayClass177_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CharacterEditorScreen.<>c__DisplayClass177_1 CS$<>8__locals3 = CS$<>8__locals2;
				RichString headerText = CharacterEditorScreen.GetCharacterEditorTranslation("SaveRagdoll");
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("ProvideFileName"));
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				CS$<>8__locals3.box = new GUIMessageBox(headerText, defaultInterpolatedStringHandler.ToStringAndClear(), new LocalizedString[]
				{
					TextManager.Get("Cancel"),
					TextManager.Get("Save")
				}, new Vector2?(CS$<>8__locals1.messageBoxRelSize), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				CS$<>8__locals2.inputField = new GUITextBox(new RectTransform(new Point(CS$<>8__locals2.box.Content.Rect.Width, (int)(30f * GUI.yScale)), CS$<>8__locals2.box.Content.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), CS$<>8__locals1.<>4__this.RagdollParams.Name.RemoveWhitespace(), null, null, Alignment.Left, false, "", null, false, true);
				GUIButton guibutton8 = CS$<>8__locals2.box.Buttons[0];
				guibutton8.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton8.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					CS$<>8__locals2.box.Close();
					return true;
				}));
				GUIButton guibutton9 = CS$<>8__locals2.box.Buttons[1];
				guibutton9.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton9.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					GUIFont font;
					if (CS$<>8__locals2.CS$<>8__locals1.<>4__this.VanillaCharacters.Contains(CharacterPrefab.Prefabs[CS$<>8__locals2.CS$<>8__locals1.<>4__this.currentCharacterIdentifier].ContentFile))
					{
						LocalizedString characterEditorTranslation = CharacterEditorScreen.GetCharacterEditorTranslation("CannotEditVanillaCharacters");
						Color color = GUIStyle.Red;
						font = GUIStyle.LargeFont;
						GUI.AddMessage(characterEditorTranslation, color, null, true, font);
						CS$<>8__locals2.box.Close();
						return false;
					}
					CS$<>8__locals2.CS$<>8__locals1.<>4__this.character.AnimController.SaveRagdoll(CS$<>8__locals2.inputField.Text);
					LocalizedString message = CharacterEditorScreen.GetCharacterEditorTranslation("RagdollSavedTo").Replace("[path]", CS$<>8__locals2.CS$<>8__locals1.<>4__this.RagdollParams.Path.Value, StringComparison.Ordinal);
					Color green = Color.Green;
					font = GUIStyle.Font;
					GUI.AddMessage(message, green, null, true, font);
					RagdollParams.ClearCache();
					CS$<>8__locals2.CS$<>8__locals1.<>4__this.ResetParamsEditor();
					CS$<>8__locals2.box.Close();
					return true;
				}));
				return true;
			}));
			GUIButton loadRagdollButton = new GUIButton(new RectTransform(buttonSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("LoadRagdoll"), Alignment.Center, "", null);
			GUIButton guibutton3 = loadRagdollButton;
			guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object userData)
			{
				CharacterEditorScreen.<>c__DisplayClass177_2 CS$<>8__locals2 = new CharacterEditorScreen.<>c__DisplayClass177_2();
				CS$<>8__locals2.CS$<>8__locals2 = CS$<>8__locals1;
				CS$<>8__locals2.loadBox = new GUIMessageBox(CharacterEditorScreen.GetCharacterEditorTranslation("LoadRagdoll"), "", new LocalizedString[]
				{
					TextManager.Get("Cancel"),
					TextManager.Get("Load"),
					TextManager.Get("Delete")
				}, new Vector2?(CS$<>8__locals1.messageBoxRelSize), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				GUIButton guibutton8 = CS$<>8__locals2.loadBox.Buttons[0];
				guibutton8.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton8.OnClicked, new GUIButton.OnClickedHandler(CS$<>8__locals2.loadBox.Close));
				CS$<>8__locals2.listBox = new GUIListBox(new RectTransform(new Vector2(0.9f, 0.6f), CS$<>8__locals2.loadBox.Content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					PlaySoundOnSelect = true
				};
				CS$<>8__locals2.deleteButton = CS$<>8__locals2.loadBox.Buttons[2];
				CS$<>8__locals2.deleteButton.Enabled = false;
				CS$<>8__locals2.<CreateFileEditPanel>g__PopulateListBox|10();
				CS$<>8__locals2.selectedFile = null;
				GUIListBox listBox = CS$<>8__locals2.listBox;
				listBox.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(listBox.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object data)
				{
					CS$<>8__locals2.selectedFile = (data as string);
					string fileName = Path.GetFileNameWithoutExtension(CS$<>8__locals2.selectedFile);
					CS$<>8__locals2.deleteButton.Enabled = (fileName != CS$<>8__locals2.CS$<>8__locals2.<>4__this.RagdollParams.Name && fileName != RagdollParams.GetDefaultFileName(CS$<>8__locals2.CS$<>8__locals2.<>4__this.character.SpeciesName));
					return true;
				}));
				GUIButton deleteButton = CS$<>8__locals2.deleteButton;
				deleteButton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(deleteButton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object data)
				{
					if (CS$<>8__locals2.selectedFile == null)
					{
						CS$<>8__locals2.loadBox.Close();
						return false;
					}
					GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("DeleteDialogLabel"), TextManager.GetWithVariable("DeleteDialogQuestion", "[file]", CS$<>8__locals2.selectedFile, FormatCapitals.No), new LocalizedString[]
					{
						TextManager.Get("Yes"),
						TextManager.Get("Cancel")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					GUIButton guibutton10 = msgBox.Buttons[0];
					guibutton10.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton10.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
					{
						try
						{
							File.Delete(CS$<>8__locals2.selectedFile, true);
							LocalizedString message = CharacterEditorScreen.GetCharacterEditorTranslation("RagdollDeletedFrom").Replace("[file]", CS$<>8__locals2.selectedFile, StringComparison.Ordinal);
							Color color = GUIStyle.Red;
							GUIFont font = GUIStyle.Font;
							GUI.AddMessage(message, color, null, true, font);
						}
						catch (Exception e)
						{
							DebugConsole.ThrowErrorLocalized(TextManager.Get("DeleteFileError").Replace("[file]", CS$<>8__locals2.selectedFile, StringComparison.Ordinal), e, null, false, false);
						}
						msgBox.Close();
						CS$<>8__locals2.listBox.ClearChildren();
						CS$<>8__locals2.<CreateFileEditPanel>g__PopulateListBox|10();
						CS$<>8__locals2.selectedFile = null;
						return true;
					}));
					GUIButton guibutton11 = msgBox.Buttons[1];
					guibutton11.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton11.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
					{
						msgBox.Close();
						return true;
					}));
					return true;
				}));
				GUIButton guibutton9 = CS$<>8__locals2.loadBox.Buttons[1];
				guibutton9.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton9.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object data)
				{
					string fileName = Path.GetFileNameWithoutExtension(CS$<>8__locals2.selectedFile);
					Identifier baseSpecies = CS$<>8__locals2.CS$<>8__locals2.<>4__this.character.GetBaseCharacterSpeciesName();
					RagdollParams ragdoll = CS$<>8__locals2.CS$<>8__locals2.<>4__this.character.IsHumanoid ? RagdollParams.GetRagdollParams<HumanRagdollParams>(CS$<>8__locals2.CS$<>8__locals2.<>4__this.character.SpeciesName, baseSpecies, fileName, CS$<>8__locals2.CS$<>8__locals2.<>4__this.character.Prefab.ContentPackage) : RagdollParams.GetRagdollParams<FishRagdollParams>(CS$<>8__locals2.CS$<>8__locals2.<>4__this.character.SpeciesName, baseSpecies, fileName, CS$<>8__locals2.CS$<>8__locals2.<>4__this.character.Prefab.ContentPackage);
					ragdoll.Reset(true);
					LocalizedString message = CharacterEditorScreen.GetCharacterEditorTranslation("RagdollLoadedFrom").Replace("[file]", CS$<>8__locals2.selectedFile, StringComparison.Ordinal);
					Color whiteSmoke = Color.WhiteSmoke;
					GUIFont font = GUIStyle.Font;
					GUI.AddMessage(message, whiteSmoke, null, true, font);
					CS$<>8__locals2.CS$<>8__locals2.<>4__this.RecreateRagdoll(ragdoll);
					CS$<>8__locals2.CS$<>8__locals2.<>4__this.CreateContextualControls();
					CS$<>8__locals2.loadBox.Close();
					return true;
				}));
				return true;
			}));
			GUIButton saveAnimationButton = new GUIButton(new RectTransform(buttonSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("SaveAnimation"), Alignment.Center, "", null);
			GUIButton guibutton4 = saveAnimationButton;
			guibutton4.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton4.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object userData)
			{
				GUIMessageBox box = new GUIMessageBox(CharacterEditorScreen.GetCharacterEditorTranslation("SaveAnimation"), string.Empty, new LocalizedString[]
				{
					TextManager.Get("Cancel"),
					TextManager.Get("Save")
				}, new Vector2?(CS$<>8__locals1.messageBoxRelSize), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				GUIFrame textArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), box.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(350, 30)
				}, null, null);
				RectTransform rectTransform = new RectTransform(new Vector2(0.3f, 1f), textArea.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				rectTransform.MinSize = new Point(250, 30);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("ProvideFileName"));
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				GUITextBlock inputLabel = new GUITextBlock(rectTransform, defaultInterpolatedStringHandler.ToStringAndClear(), null, null, Alignment.Left, false, "", null);
				GUITextBox inputField = new GUITextBox(new RectTransform(new Vector2(0.45f, 1f), textArea.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(100, 30)
				}, CS$<>8__locals1.<>4__this.CurrentAnimation.Name, null, null, Alignment.Left, false, "", null, false, true);
				GUIFrame typeSelectionArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), box.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 30)
				}, null, null);
				RectTransform rectT2 = new RectTransform(new Vector2(0.45f, 1f), typeSelectionArea.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("SelectAnimationType"));
				defaultInterpolatedStringHandler2.AppendLiteral(": ");
				GUITextBlock typeLabel = new GUITextBlock(rectT2, defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, Alignment.Left, false, "", null);
				GUIDropDown typeDropdown = new GUIDropDown(new RectTransform(new Vector2(0.45f, 1f), typeSelectionArea.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
				foreach (object enumValue in Enum.GetValues(typeof(AnimationType)))
				{
					if (!(enumValue is AnimationType) || (AnimationType)enumValue != AnimationType.NotDefined)
					{
						typeDropdown.AddItem(enumValue.ToString(), enumValue, null, null, null);
					}
				}
				AnimationType selectedType = CS$<>8__locals1.<>4__this.character.AnimController.ForceSelectAnimationType;
				typeDropdown.OnSelected = delegate(GUIComponent component, object data)
				{
					selectedType = (AnimationType)data;
					GUITextBox inputField = inputField;
					AnimationParams animationParamsFromType = CS$<>8__locals1.<>4__this.character.AnimController.GetAnimationParamsFromType(selectedType);
					inputField.Text = ((animationParamsFromType != null) ? animationParamsFromType.Name.RemoveWhitespace() : null);
					return true;
				};
				typeDropdown.SelectItem(selectedType);
				GUIButton guibutton8 = box.Buttons[0];
				guibutton8.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton8.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					box.Close();
					return true;
				}));
				GUIButton guibutton9 = box.Buttons[1];
				guibutton9.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton9.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					GUIFont font;
					if (CS$<>8__locals1.<>4__this.VanillaCharacters.Contains(CharacterPrefab.Prefabs[CS$<>8__locals1.<>4__this.currentCharacterIdentifier].ContentFile))
					{
						LocalizedString characterEditorTranslation = CharacterEditorScreen.GetCharacterEditorTranslation("CannotEditVanillaCharacters");
						Color color = GUIStyle.Red;
						font = GUIStyle.LargeFont;
						GUI.AddMessage(characterEditorTranslation, color, null, true, font);
						box.Close();
						return false;
					}
					AnimationParams animParams = CS$<>8__locals1.<>4__this.character.AnimController.GetAnimationParamsFromType(selectedType);
					if (animParams == null)
					{
						return true;
					}
					string fileName = inputField.Text;
					animParams.Save(fileName, null);
					string newPath = animParams.Path.ToString();
					LocalizedString message = CharacterEditorScreen.GetCharacterEditorTranslation("AnimationOfTypeSavedTo").Replace("[type]", selectedType.ToString(), StringComparison.Ordinal).Replace("[path]", newPath, StringComparison.Ordinal);
					Color green = Color.Green;
					font = GUIStyle.Font;
					GUI.AddMessage(message, green, null, true, font);
					AnimationParams.ClearCache();
					CS$<>8__locals1.<>4__this.ResetParamsEditor();
					box.Close();
					return true;
				}));
				return true;
			}));
			GUIButton loadAnimationButton = new GUIButton(new RectTransform(buttonSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("LoadAnimation"), Alignment.Center, "", null);
			GUIButton guibutton5 = loadAnimationButton;
			guibutton5.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton5.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object userData)
			{
				CharacterEditorScreen.<>c__DisplayClass177_5 CS$<>8__locals2 = new CharacterEditorScreen.<>c__DisplayClass177_5();
				CS$<>8__locals2.CS$<>8__locals5 = CS$<>8__locals1;
				CS$<>8__locals2.loadBox = new GUIMessageBox(CharacterEditorScreen.GetCharacterEditorTranslation("LoadAnimation"), "", new LocalizedString[]
				{
					TextManager.Get("Cancel"),
					TextManager.Get("Load"),
					TextManager.Get("Delete")
				}, new Vector2?(CS$<>8__locals1.messageBoxRelSize), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				GUIButton guibutton8 = CS$<>8__locals2.loadBox.Buttons[0];
				guibutton8.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton8.OnClicked, new GUIButton.OnClickedHandler(CS$<>8__locals2.loadBox.Close));
				CS$<>8__locals2.listBox = new GUIListBox(new RectTransform(new Vector2(0.9f, 0.6f), CS$<>8__locals2.loadBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					PlaySoundOnSelect = true
				};
				CS$<>8__locals2.deleteButton = CS$<>8__locals2.loadBox.Buttons[2];
				CS$<>8__locals2.deleteButton.Enabled = false;
				GUIFrame typeSelectionArea = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.1f), CS$<>8__locals2.loadBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 30)
				}, null, null);
				RectTransform rectT2 = new RectTransform(new Vector2(0.45f, 1f), typeSelectionArea.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("SelectAnimationType"));
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				GUITextBlock typeLabel = new GUITextBlock(rectT2, defaultInterpolatedStringHandler.ToStringAndClear(), null, null, Alignment.Left, false, "", null);
				GUIDropDown typeDropdown = new GUIDropDown(new RectTransform(new Vector2(0.45f, 1f), typeSelectionArea.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
				foreach (object enumValue in Enum.GetValues(typeof(AnimationType)))
				{
					if (!(enumValue is AnimationType) || (AnimationType)enumValue != AnimationType.NotDefined)
					{
						typeDropdown.AddItem(enumValue.ToString(), enumValue, null, null, null);
					}
				}
				CS$<>8__locals2.selectedType = CS$<>8__locals1.<>4__this.character.AnimController.ForceSelectAnimationType;
				typeDropdown.OnSelected = delegate(GUIComponent component, object data)
				{
					CS$<>8__locals2.selectedType = (AnimationType)data;
					base.<CreateFileEditPanel>g__PopulateListBox|22();
					return true;
				};
				typeDropdown.SelectItem(CS$<>8__locals2.selectedType);
				CS$<>8__locals2.<CreateFileEditPanel>g__PopulateListBox|22();
				CS$<>8__locals2.selectedFile = null;
				GUIListBox listBox = CS$<>8__locals2.listBox;
				listBox.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(listBox.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object data)
				{
					CS$<>8__locals2.selectedFile = (data as string);
					string fileName = Path.GetFileNameWithoutExtension(CS$<>8__locals2.selectedFile);
					CS$<>8__locals2.deleteButton.Enabled = (fileName != CS$<>8__locals2.CS$<>8__locals5.<>4__this.CurrentAnimation.Name && fileName != AnimationParams.GetDefaultFileName(CS$<>8__locals2.CS$<>8__locals5.<>4__this.character.SpeciesName, CS$<>8__locals2.CS$<>8__locals5.<>4__this.CurrentAnimation.AnimationType));
					return true;
				}));
				GUIButton deleteButton = CS$<>8__locals2.deleteButton;
				deleteButton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(deleteButton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object data)
				{
					if (CS$<>8__locals2.selectedFile == null)
					{
						CS$<>8__locals2.loadBox.Close();
						return false;
					}
					GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("DeleteDialogLabel"), TextManager.GetWithVariable("DeleteDialogQuestion", "[file]", CS$<>8__locals2.selectedFile, FormatCapitals.No), new LocalizedString[]
					{
						TextManager.Get("Yes"),
						TextManager.Get("Cancel")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					GUIButton guibutton10 = msgBox.Buttons[0];
					guibutton10.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton10.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
					{
						try
						{
							File.Delete(CS$<>8__locals2.selectedFile, true);
							LocalizedString message = CharacterEditorScreen.GetCharacterEditorTranslation("AnimationOfTypeDeleted").Replace("[type]", CS$<>8__locals2.selectedType.ToString(), StringComparison.Ordinal).Replace("[file]", CS$<>8__locals2.selectedFile, StringComparison.Ordinal);
							Color color = GUIStyle.Red;
							GUIFont font = GUIStyle.Font;
							GUI.AddMessage(message, color, null, true, font);
						}
						catch (Exception e)
						{
							DebugConsole.ThrowErrorLocalized(TextManager.GetWithVariable("DeleteFileError", "[file]", CS$<>8__locals2.selectedFile, FormatCapitals.No), e, null, false, false);
						}
						msgBox.Close();
						CS$<>8__locals2.<CreateFileEditPanel>g__PopulateListBox|22();
						CS$<>8__locals2.selectedFile = null;
						return true;
					}));
					GUIButton guibutton11 = msgBox.Buttons[1];
					guibutton11.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton11.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
					{
						msgBox.Close();
						return true;
					}));
					return true;
				}));
				GUIButton guibutton9 = CS$<>8__locals2.loadBox.Buttons[1];
				guibutton9.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton9.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object data)
				{
					AnimationParams animationParams;
					if (CS$<>8__locals2.CS$<>8__locals5.<>4__this.character.AnimController.TryLoadAnimation(CS$<>8__locals2.selectedType, Path.GetFileNameWithoutExtension(CS$<>8__locals2.selectedFile), out animationParams, true))
					{
						animationParams.Reset(true);
						LocalizedString message = CharacterEditorScreen.GetCharacterEditorTranslation("AnimationOfTypeLoaded").Replace("[type]", CS$<>8__locals2.selectedType.ToString(), StringComparison.Ordinal).Replace("[file]", animationParams.FileNameWithoutExtension, StringComparison.Ordinal);
						Color whiteSmoke = Color.WhiteSmoke;
						GUIFont font = GUIStyle.Font;
						GUI.AddMessage(message, whiteSmoke, null, true, font);
					}
					CS$<>8__locals2.CS$<>8__locals5.<>4__this.ResetParamsEditor();
					CS$<>8__locals2.loadBox.Close();
					return true;
				}));
				return true;
			}));
			new GUIFrame(new RectTransform(buttonSize / 2f, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			GUIButton guibutton6 = new GUIButton(new RectTransform(buttonSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("ResetButton"), Alignment.Center, "", null)
			{
				Color = GUIStyle.Red
			};
			guibutton6.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton6.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object userData)
			{
				CS$<>8__locals1.<>4__this.CharacterParams.Reset(true);
				CS$<>8__locals1.<>4__this.AnimParams.ForEach(delegate(AnimationParams p)
				{
					p.Reset(true);
				});
				CS$<>8__locals1.<>4__this.character.AnimController.ResetRagdoll();
				CS$<>8__locals1.<>4__this.RecreateRagdoll(null);
				CS$<>8__locals1.<>4__this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
				CS$<>8__locals1.<>4__this.isDrawingLimb = false;
				CS$<>8__locals1.<>4__this.newLimbRect = Rectangle.Empty;
				CS$<>8__locals1.<>4__this.jointStartLimb = null;
				CS$<>8__locals1.<>4__this.CreateGUI();
				return true;
			}));
			new GUIFrame(new RectTransform(buttonSize / 2f, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			new GUIButton(new RectTransform(buttonSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("CreateNewCharacter"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object data)
			{
				base.<CreateFileEditPanel>g__ResetView|2();
				Wizard.Instance.SelectTab(Wizard.Tab.Character);
				return true;
			};
			GUIButton guibutton7 = new GUIButton(new RectTransform(buttonSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CharacterEditorScreen.GetCharacterEditorTranslation("CopyCharacter"), Alignment.Center, "", null);
			guibutton7.ToolTip = CharacterEditorScreen.GetCharacterEditorTranslation("CopyCharacterToolTip");
			guibutton7.OnClicked = delegate(GUIButton button, object data)
			{
				base.<CreateFileEditPanel>g__ResetView|2();
				CS$<>8__locals1.<>4__this.PrepareCharacterCopy();
				Wizard.Instance.SelectTab(Wizard.Tab.Character);
				return true;
			};
			GUITextBlock.AutoScaleAndNormalize(from c in CS$<>8__locals1.layoutGroup.Children
			where c is GUIButton
			select ((GUIButton)c).TextBlock, true, false, null);
			this.fileEditToggle = new CharacterEditorScreen.ToggleButton(new RectTransform(new Vector2(0.08f, 1f), this.fileEditPanel.RectTransform, Anchor.CenterLeft, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal), CharacterEditorScreen.Direction.Right);
			this.fileEditPanel.RectTransform.MinSize = new Point(0, (int)((float)CS$<>8__locals1.layoutGroup.RectTransform.Children.Sum((RectTransform c) => c.MinSize.Y + CS$<>8__locals1.layoutGroup.AbsoluteSpacing) * 1.2f));
		}

		// Token: 0x0600495D RID: 18781 RVA: 0x002872E0 File Offset: 0x002854E0
		public void PrepareCharacterCopy()
		{
			this.CharacterParams.Serialize(null, true, true);
			this.RagdollParams.Serialize(null, true, true);
			this.AnimParams.ForEach(delegate(AnimationParams a)
			{
				a.Serialize();
			});
			Wizard.Instance.CopyExisting(this.CharacterParams, this.RagdollParams, this.AnimParams);
		}

		// Token: 0x170012AB RID: 4779
		// (get) Token: 0x0600495E RID: 18782 RVA: 0x00287351 File Offset: 0x00285551
		private CharacterParams CharacterParams
		{
			get
			{
				return this.character.Params;
			}
		}

		// Token: 0x170012AC RID: 4780
		// (get) Token: 0x0600495F RID: 18783 RVA: 0x0028735E File Offset: 0x0028555E
		private List<AnimationParams> AnimParams
		{
			get
			{
				return this.character.AnimController.AllAnimParams;
			}
		}

		// Token: 0x170012AD RID: 4781
		// (get) Token: 0x06004960 RID: 18784 RVA: 0x00287370 File Offset: 0x00285570
		private AnimationParams CurrentAnimation
		{
			get
			{
				return this.character.AnimController.CurrentAnimationParams;
			}
		}

		// Token: 0x170012AE RID: 4782
		// (get) Token: 0x06004961 RID: 18785 RVA: 0x00287382 File Offset: 0x00285582
		private RagdollParams RagdollParams
		{
			get
			{
				return this.character.AnimController.RagdollParams;
			}
		}

		// Token: 0x06004962 RID: 18786 RVA: 0x00287394 File Offset: 0x00285594
		private void ResetParamsEditor()
		{
			ParamsEditor.Instance.Clear();
			if (!this.editRagdoll && !this.editCharacterInfo && !this.editJoints && !this.editLimbs && !this.editAnimations)
			{
				this.paramsToggle.Selected = false;
				return;
			}
			if (this.editCharacterInfo)
			{
				ParamsEditor mainEditor = ParamsEditor.Instance;
				this.CharacterParams.AddToEditor(mainEditor, true, true, 10);
				SerializableEntityEditor characterEditor = this.CharacterParams.SerializableEntityEditor;
				characterEditor.AddCustomContent(new GUIFrame(new RectTransform(new Point(characterEditor.Rect.Width, (int)(10f * GUI.yScale)), characterEditor.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null)
				{
					CanBeFocused = false
				}, 1);
				if (this.CharacterParams.AI != null)
				{
					this.<ResetParamsEditor>g__CreateAddButton|189_6(this.CharacterParams.AI.SerializableEntityEditor, delegate
					{
						CharacterParams.TargetParams targetParams;
						this.CharacterParams.AI.TryAddEmptyTarget(out targetParams);
					}, CharacterEditorScreen.GetCharacterEditorTranslation("AddAITarget"));
					using (IEnumerator<CharacterParams.TargetParams> enumerator = this.CharacterParams.AI.Targets.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							CharacterParams.TargetParams target = enumerator.Current;
							this.<ResetParamsEditor>g__CreateCloseButton|189_4(target.SerializableEntityEditor, delegate
							{
								this.CharacterParams.AI.RemoveTarget(target);
							}, 0.8f);
						}
					}
				}
				using (List<CharacterParams.ParticleParams>.Enumerator enumerator2 = this.CharacterParams.BloodEmitters.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						CharacterParams.ParticleParams emitter = enumerator2.Current;
						this.<ResetParamsEditor>g__CreateCloseButton|189_4(emitter.SerializableEntityEditor, delegate
						{
							this.CharacterParams.RemoveBloodEmitter(emitter);
						}, 1f);
					}
				}
				using (List<CharacterParams.ParticleParams>.Enumerator enumerator3 = this.CharacterParams.GibEmitters.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						CharacterParams.ParticleParams emitter = enumerator3.Current;
						this.<ResetParamsEditor>g__CreateCloseButton|189_4(emitter.SerializableEntityEditor, delegate
						{
							this.CharacterParams.RemoveGibEmitter(emitter);
						}, 1f);
					}
				}
				using (List<CharacterParams.ParticleParams>.Enumerator enumerator4 = this.CharacterParams.DamageEmitters.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						CharacterParams.ParticleParams emitter = enumerator4.Current;
						this.<ResetParamsEditor>g__CreateCloseButton|189_4(emitter.SerializableEntityEditor, delegate
						{
							this.CharacterParams.RemoveDamageEmitter(emitter);
						}, 1f);
					}
				}
				using (List<CharacterParams.SoundParams>.Enumerator enumerator5 = this.CharacterParams.Sounds.GetEnumerator())
				{
					while (enumerator5.MoveNext())
					{
						CharacterParams.SoundParams sound = enumerator5.Current;
						this.<ResetParamsEditor>g__CreateCloseButton|189_4(sound.SerializableEntityEditor, delegate
						{
							this.CharacterParams.RemoveSound(sound);
						}, 1f);
					}
				}
				using (List<CharacterParams.InventoryParams>.Enumerator enumerator6 = this.CharacterParams.Inventories.GetEnumerator())
				{
					while (enumerator6.MoveNext())
					{
						CharacterParams.InventoryParams inventory = enumerator6.Current;
						SerializableEntityEditor editor = inventory.SerializableEntityEditor;
						this.<ResetParamsEditor>g__CreateCloseButton|189_4(editor, delegate
						{
							this.CharacterParams.RemoveInventory(inventory);
						}, 1f);
						using (List<CharacterParams.InventoryParams.InventoryItem>.Enumerator enumerator7 = inventory.Items.GetEnumerator())
						{
							while (enumerator7.MoveNext())
							{
								CharacterParams.InventoryParams.InventoryItem item = enumerator7.Current;
								this.<ResetParamsEditor>g__CreateCloseButton|189_4(item.SerializableEntityEditor, delegate
								{
									inventory.RemoveItem(item);
								}, 0.8f);
							}
						}
						this.<ResetParamsEditor>g__CreateAddButton|189_6(editor, delegate
						{
							inventory.AddItem(null);
						}, CharacterEditorScreen.GetCharacterEditorTranslation("AddInventoryItem"));
					}
				}
				this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor, delegate
				{
					this.CharacterParams.AddBloodEmitter();
				}, CharacterEditorScreen.GetCharacterEditorTranslation("AddBloodEmitter"));
				this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor, delegate
				{
					this.CharacterParams.AddGibEmitter();
				}, CharacterEditorScreen.GetCharacterEditorTranslation("AddGibEmitter"));
				this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor, delegate
				{
					this.CharacterParams.AddDamageEmitter();
				}, CharacterEditorScreen.GetCharacterEditorTranslation("AddDamageEmitter"));
				this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor, delegate
				{
					this.CharacterParams.AddSound();
				}, CharacterEditorScreen.GetCharacterEditorTranslation("AddSound"));
				this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor, delegate
				{
					this.CharacterParams.AddInventory();
				}, CharacterEditorScreen.GetCharacterEditorTranslation("AddInventory"));
				return;
			}
			if (this.editAnimations)
			{
				AnimationParams currentAnimationParams = this.character.AnimController.CurrentAnimationParams;
				if (currentAnimationParams == null)
				{
					return;
				}
				currentAnimationParams.AddToEditor(ParamsEditor.Instance, 10);
				return;
			}
			else
			{
				if (this.editRagdoll)
				{
					this.RagdollParams.AddToEditor(ParamsEditor.Instance, false, 10);
					this.RagdollParams.Colliders.ForEach(delegate(RagdollParams.ColliderParams c)
					{
						c.AddToEditor(ParamsEditor.Instance, false, 10);
					});
					return;
				}
				if (!this.editJoints)
				{
					if (this.editLimbs)
					{
						if (this.selectedLimbs.Any<Limb>())
						{
							using (List<Limb>.Enumerator enumerator8 = this.selectedLimbs.GetEnumerator())
							{
								while (enumerator8.MoveNext())
								{
									Limb limb = enumerator8.Current;
									ParamsEditor mainEditor2 = ParamsEditor.Instance;
									SerializableEntityEditor limbEditor = limb.Params.SerializableEntityEditor;
									limb.Params.AddToEditor(mainEditor2, true, 0);
									using (List<RagdollParams.DamageModifierParams>.Enumerator enumerator9 = limb.Params.DamageModifiers.GetEnumerator())
									{
										while (enumerator9.MoveNext())
										{
											RagdollParams.DamageModifierParams damageModifier = enumerator9.Current;
											this.<ResetParamsEditor>g__CreateCloseButton|189_4(damageModifier.SerializableEntityEditor, delegate
											{
												limb.Params.RemoveDamageModifier(damageModifier);
											}, 1f);
										}
									}
									if (limb.Params.Sound == null)
									{
										this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor2, delegate
										{
											limb.Params.AddSound();
										}, CharacterEditorScreen.GetCharacterEditorTranslation("AddSound"));
									}
									else
									{
										this.<ResetParamsEditor>g__CreateCloseButton|189_4(limb.Params.Sound.SerializableEntityEditor, delegate
										{
											limb.Params.RemoveSound();
										}, 1f);
									}
									if (limb.Params.LightSource == null)
									{
										this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor2, delegate
										{
											limb.Params.AddLight();
										}, CharacterEditorScreen.GetCharacterEditorTranslation("AddLightSource"));
									}
									else
									{
										this.<ResetParamsEditor>g__CreateCloseButton|189_4(limb.Params.LightSource.SerializableEntityEditor, delegate
										{
											limb.Params.RemoveLight();
										}, 1f);
									}
									if (limb.Params.Attack == null)
									{
										this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor2, delegate
										{
											limb.Params.AddAttack();
										}, CharacterEditorScreen.GetCharacterEditorTranslation("AddAttack"));
									}
									else
									{
										RagdollParams.AttackParams attackParams = limb.Params.Attack;
										using (Dictionary<Affliction, XElement>.Enumerator enumerator10 = attackParams.Attack.Afflictions.GetEnumerator())
										{
											while (enumerator10.MoveNext())
											{
												KeyValuePair<Affliction, XElement> affliction = enumerator10.Current;
												SerializableEntityEditor afflictionEditor;
												if (attackParams.AfflictionEditors.TryGetValue(affliction.Key, out afflictionEditor))
												{
													this.<ResetParamsEditor>g__CreateCloseButton|189_4(afflictionEditor, delegate
													{
														attackParams.RemoveAffliction(affliction.Value);
													}, 0.8f);
												}
											}
										}
										SerializableEntityEditor attackEditor = attackParams.SerializableEntityEditor;
										this.<ResetParamsEditor>g__CreateAddButton|189_6(attackEditor, delegate
										{
											attackParams.AddNewAffliction();
										}, CharacterEditorScreen.GetCharacterEditorTranslation("AddAffliction"));
										this.<ResetParamsEditor>g__CreateCloseButton|189_4(attackEditor, delegate
										{
											limb.Params.RemoveAttack();
										}, 1f);
										GUIFrame space = new GUIFrame(new RectTransform(new Point(attackEditor.RectTransform.Rect.Width, (int)(20f * GUI.yScale)), attackEditor.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(ParamsEditor.Color))
										{
											CanBeFocused = false
										};
										attackEditor.AddCustomContent(space, attackEditor.ContentCount);
									}
									this.<ResetParamsEditor>g__CreateAddButtonAtLast|189_5(mainEditor2, delegate
									{
										limb.Params.AddDamageModifier();
									}, CharacterEditorScreen.GetCharacterEditorTranslation("AddDamageModifier"));
								}
								return;
							}
						}
						this.character.AnimController.Limbs.ForEach(delegate(Limb l)
						{
							l.Params.AddToEditor(ParamsEditor.Instance, false, 10);
						});
					}
					return;
				}
				if (this.selectedJoints.Any<LimbJoint>())
				{
					this.selectedJoints.ForEach(delegate(LimbJoint j)
					{
						j.Params.AddToEditor(ParamsEditor.Instance, true, 10);
					});
					return;
				}
				this.RagdollParams.Joints.ForEach(delegate(RagdollParams.JointParams jp)
				{
					jp.AddToEditor(ParamsEditor.Instance, false, 10);
				});
				return;
			}
		}

		// Token: 0x06004963 RID: 18787 RVA: 0x00287DE0 File Offset: 0x00285FE0
		private void TryUpdateAnimParam(string name, object value)
		{
			this.TryUpdateAnimParam(name.ToIdentifier(), value);
		}

		// Token: 0x06004964 RID: 18788 RVA: 0x00287DEF File Offset: 0x00285FEF
		private void TryUpdateAnimParam(Identifier name, object value)
		{
			this.TryUpdateParam(this.character.AnimController.CurrentAnimationParams, name, value);
		}

		// Token: 0x06004965 RID: 18789 RVA: 0x00287E09 File Offset: 0x00286009
		private void TryUpdateRagdollParam(string name, object value)
		{
			this.TryUpdateRagdollParam(name.ToIdentifier(), value);
		}

		// Token: 0x06004966 RID: 18790 RVA: 0x00287E18 File Offset: 0x00286018
		private void TryUpdateRagdollParam(Identifier name, object value)
		{
			this.TryUpdateParam(this.RagdollParams, name, value);
		}

		// Token: 0x06004967 RID: 18791 RVA: 0x00287E28 File Offset: 0x00286028
		private void TryUpdateParam(EditableParams editableParams, Identifier name, object value)
		{
			if (editableParams.SerializableEntityEditor == null)
			{
				editableParams.AddToEditor(ParamsEditor.Instance, 0);
			}
			SerializableProperty p;
			if (editableParams.SerializableProperties.TryGetValue(name, out p))
			{
				editableParams.SerializableEntityEditor.UpdateValue(p, value, true);
			}
		}

		// Token: 0x06004968 RID: 18792 RVA: 0x00287E67 File Offset: 0x00286067
		private void TryUpdateJointParam(LimbJoint joint, string name, object value)
		{
			this.TryUpdateJointParam(joint, name.ToIdentifier(), value);
		}

		// Token: 0x06004969 RID: 18793 RVA: 0x00287E77 File Offset: 0x00286077
		private void TryUpdateJointParam(LimbJoint joint, Identifier name, object value)
		{
			this.TryUpdateSubParam(joint.Params, name, value);
		}

		// Token: 0x0600496A RID: 18794 RVA: 0x00287E87 File Offset: 0x00286087
		private void TryUpdateLimbParam(Limb limb, string name, object value)
		{
			this.TryUpdateLimbParam(limb, name.ToIdentifier(), value);
		}

		// Token: 0x0600496B RID: 18795 RVA: 0x00287E97 File Offset: 0x00286097
		private void TryUpdateLimbParam(Limb limb, Identifier name, object value)
		{
			this.TryUpdateSubParam(limb.Params, name, value);
		}

		// Token: 0x0600496C RID: 18796 RVA: 0x00287EA8 File Offset: 0x002860A8
		private void TryUpdateSubParam(RagdollParams.SubParam ragdollSubParams, Identifier name, object value)
		{
			if (ragdollSubParams.SerializableEntityEditor == null)
			{
				ragdollSubParams.AddToEditor(ParamsEditor.Instance, true, 0);
			}
			SerializableProperty p;
			if (ragdollSubParams.SerializableProperties.TryGetValue(name, out p))
			{
				ragdollSubParams.SerializableEntityEditor.UpdateValue(p, value, true);
				return;
			}
			RagdollParams.SubParam subParams = (from sp in ragdollSubParams.SubParams
			where sp.SerializableProperties.ContainsKey(name)
			select sp).FirstOrDefault<RagdollParams.SubParam>();
			if (subParams != null)
			{
				if (subParams.SerializableProperties.TryGetValue(name, out p))
				{
					if (subParams.SerializableEntityEditor == null)
					{
						subParams.AddToEditor(ParamsEditor.Instance, true, 0);
					}
					subParams.SerializableEntityEditor.UpdateValue(p, value, true);
					return;
				}
			}
			else
			{
				DebugConsole.ThrowErrorLocalized(CharacterEditorScreen.GetCharacterEditorTranslation("NoFieldForParameterFound").Replace("[parameter]", name.Value, StringComparison.Ordinal), null, null, false, false);
			}
		}

		// Token: 0x0600496D RID: 18797 RVA: 0x00287F82 File Offset: 0x00286182
		private Vector2 ScreenToSim(float x, float y)
		{
			return this.ScreenToSim(new Vector2(x, y));
		}

		// Token: 0x0600496E RID: 18798 RVA: 0x00287F91 File Offset: 0x00286191
		private Vector2 ScreenToSim(Vector2 p)
		{
			return ConvertUnits.ToSimUnits(this.Cam.ScreenToWorld(p)) + Submarine.MainSub.SimPosition;
		}

		// Token: 0x0600496F RID: 18799 RVA: 0x00287FB3 File Offset: 0x002861B3
		private Vector2 SimToScreen(float x, float y)
		{
			return this.SimToScreen(new Vector2(x, y));
		}

		// Token: 0x06004970 RID: 18800 RVA: 0x00287FC2 File Offset: 0x002861C2
		private Vector2 SimToScreen(Vector2 p)
		{
			return this.Cam.WorldToScreen(ConvertUnits.ToDisplayUnits(p + Submarine.MainSub.SimPosition));
		}

		// Token: 0x06004971 RID: 18801 RVA: 0x00287FE4 File Offset: 0x002861E4
		private bool IsMatchingLimb(Limb limb1, Limb limb2, LimbJoint joint1, LimbJoint joint2)
		{
			return (joint1.BodyA == limb1.body.FarseerBody && joint2.BodyA == limb2.body.FarseerBody) || (joint1.BodyB == limb1.body.FarseerBody && joint2.BodyB == limb2.body.FarseerBody);
		}

		// Token: 0x06004972 RID: 18802 RVA: 0x00288044 File Offset: 0x00286244
		private void ValidateJoint(LimbJoint limbJoint)
		{
			if (limbJoint.UpperLimit < limbJoint.LowerLimit)
			{
				if (limbJoint.LowerLimit > 0f)
				{
					limbJoint.LowerLimit -= 6.2831855f;
				}
				if (limbJoint.UpperLimit < 0f)
				{
					limbJoint.UpperLimit += 6.2831855f;
				}
			}
			limbJoint.LowerLimit = MathUtils.WrapAnglePi(limbJoint.LowerLimit);
			limbJoint.UpperLimit = MathUtils.WrapAnglePi(limbJoint.UpperLimit);
		}

		// Token: 0x06004973 RID: 18803 RVA: 0x002880C0 File Offset: 0x002862C0
		private Limb GetClosestLimbOnRagdoll(Vector2 targetPos, Func<Limb, bool> filter = null)
		{
			Limb closestLimb = null;
			float closestDistance = float.MaxValue;
			foreach (Limb i in this.character.AnimController.Limbs)
			{
				if (filter == null || filter(i))
				{
					float distance = Vector2.DistanceSquared(this.SimToScreen(i.SimPosition), targetPos);
					if (distance < closestDistance)
					{
						closestLimb = i;
						closestDistance = distance;
					}
				}
			}
			return closestLimb;
		}

		// Token: 0x06004974 RID: 18804 RVA: 0x0028812C File Offset: 0x0028632C
		private Limb GetClosestLimbOnSpritesheet(Vector2 targetPos, Func<Limb, bool> filter = null)
		{
			Limb closestLimb = null;
			float closestDistance = float.MaxValue;
			foreach (Limb i in this.character.AnimController.Limbs)
			{
				if (i != null && (filter == null || filter(i)))
				{
					float distance = Vector2.DistanceSquared(this.GetLimbSpritesheetRect(i).Center.ToVector2(), targetPos);
					if (distance < closestDistance)
					{
						closestLimb = i;
						closestDistance = distance;
					}
				}
			}
			return closestLimb;
		}

		// Token: 0x06004975 RID: 18805 RVA: 0x002881A8 File Offset: 0x002863A8
		private Rectangle GetLimbSpritesheetRect(Limb limb)
		{
			int offsetX = 30;
			int offsetY = 20;
			Rectangle rect = Rectangle.Empty;
			if (this.Textures != null)
			{
				for (int i = 0; i < this.Textures.Count; i++)
				{
					if (!(limb.ActiveSprite.FilePath != this.texturePaths[i]))
					{
						rect = limb.ActiveSprite.SourceRect;
						rect.Size = rect.MultiplySize(this.spriteSheetZoom);
						rect.Location = rect.Location.Multiply(this.spriteSheetZoom);
						rect.X += offsetX;
						rect.Y += offsetY;
						break;
					}
					offsetY += (int)((float)this.Textures[i].Height * this.spriteSheetZoom);
				}
			}
			return rect;
		}

		// Token: 0x06004976 RID: 18806 RVA: 0x00288278 File Offset: 0x00286478
		private void UpdateSourceRect(Limb limb, Rectangle newRect, bool resize)
		{
			CharacterEditorScreen.<>c__DisplayClass209_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass209_0();
			CS$<>8__locals1.newRect = newRect;
			CS$<>8__locals1.resize = resize;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.activeSprite = limb.ActiveSprite;
			CS$<>8__locals1.activeSprite.SourceRect = CS$<>8__locals1.newRect;
			if (limb.DamagedSprite != null)
			{
				limb.DamagedSprite.SourceRect = CS$<>8__locals1.activeSprite.SourceRect;
			}
			CS$<>8__locals1.colliderSize = new Vector2(ConvertUnits.ToSimUnits(CS$<>8__locals1.newRect.Width), ConvertUnits.ToSimUnits(CS$<>8__locals1.newRect.Height));
			if (CS$<>8__locals1.resize && this.recalculateCollider)
			{
				this.RecalculateCollider(limb, CS$<>8__locals1.colliderSize);
			}
			CS$<>8__locals1.spritePos = new Vector2(30f, (float)this.GetOffsetY(CS$<>8__locals1.activeSprite));
			CharacterEditorScreen.<>c__DisplayClass209_0 CS$<>8__locals2 = CS$<>8__locals1;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
			defaultInterpolatedStringHandler.AppendFormatted<int>(limb.Params.ID);
			defaultInterpolatedStringHandler.AppendLiteral("_origin");
			CS$<>8__locals2.originWidget = this.GetLimbEditWidget(defaultInterpolatedStringHandler.ToStringAndClear(), limb, 5, WidgetShape.Rectangle, null);
			if (!CS$<>8__locals1.resize && CS$<>8__locals1.originWidget != null)
			{
				Vector2 newOrigin = (CS$<>8__locals1.originWidget.DrawPos - CS$<>8__locals1.spritePos - CS$<>8__locals1.activeSprite.SourceRect.Location.ToVector2() * this.spriteSheetZoom) / this.spriteSheetZoom;
				this.RecalculateOrigin(limb, new Vector2?(newOrigin));
			}
			else
			{
				this.RecalculateOrigin(limb, null);
			}
			this.TryUpdateLimbParam(limb, "sourcerect", CS$<>8__locals1.newRect);
			if (this.limbPairEditing)
			{
				this.UpdateOtherLimbs(limb, delegate(Limb otherLimb)
				{
					otherLimb.ActiveSprite.SourceRect = CS$<>8__locals1.newRect;
					if (otherLimb.DamagedSprite != null)
					{
						otherLimb.DamagedSprite.SourceRect = CS$<>8__locals1.newRect;
					}
					if (CS$<>8__locals1.resize && CS$<>8__locals1.<>4__this.recalculateCollider)
					{
						CS$<>8__locals1.<>4__this.RecalculateCollider(otherLimb, CS$<>8__locals1.colliderSize);
					}
					if (!CS$<>8__locals1.resize && CS$<>8__locals1.originWidget != null)
					{
						Vector2 newOrigin2 = (CS$<>8__locals1.originWidget.DrawPos - CS$<>8__locals1.spritePos - CS$<>8__locals1.activeSprite.SourceRect.Location.ToVector2() * CS$<>8__locals1.<>4__this.spriteSheetZoom) / CS$<>8__locals1.<>4__this.spriteSheetZoom;
						CS$<>8__locals1.<>4__this.RecalculateOrigin(otherLimb, new Vector2?(newOrigin2));
					}
					else
					{
						CS$<>8__locals1.<>4__this.RecalculateOrigin(otherLimb, null);
					}
					CS$<>8__locals1.<>4__this.TryUpdateLimbParam(otherLimb, "sourcerect", CS$<>8__locals1.newRect);
				});
			}
		}

		// Token: 0x06004977 RID: 18807 RVA: 0x00288430 File Offset: 0x00286630
		private void CalculateSpritesheetZoom()
		{
			Texture2D texture = (from t in this.textures
			orderby t.Width descending
			select t).FirstOrDefault<Texture2D>();
			if (texture == null)
			{
				this.spriteSheetZoom = 1f;
				return;
			}
			float width = (float)texture.Width;
			float height = (float)this.textures.Sum((Texture2D t) => t.Height);
			float margin = 20f;
			if (this.unrestrictSpritesheet)
			{
				this.spriteSheetMaxZoom = ((float)(GameMain.GraphicsWidth - 60) - margin - (float)this.leftArea.Rect.Width) / width;
			}
			else if (height > width)
			{
				this.spriteSheetMaxZoom = ((float)(this.centerArea.Rect.Bottom - 20) - margin) / height;
			}
			else
			{
				this.spriteSheetMaxZoom = ((float)(this.centerArea.Rect.Left - 30) - margin) / width;
			}
			this.spriteSheetMinZoom = ((this.spriteSheetMinZoom > this.spriteSheetMaxZoom) ? this.spriteSheetMaxZoom : 0.25f);
			this.spriteSheetZoom = MathHelper.Clamp(1f, this.spriteSheetMinZoom, this.spriteSheetMaxZoom);
		}

		// Token: 0x06004978 RID: 18808 RVA: 0x0028856C File Offset: 0x0028676C
		private void HandleLimbSelection(Limb limb)
		{
			if (!this.editLimbs)
			{
				this.SetToggle(this.limbsToggle, true);
			}
			if (!this.selectedLimbs.Contains(limb))
			{
				if (!Widget.EnableMultiSelect)
				{
					this.selectedLimbs.Clear();
				}
				this.selectedLimbs.Add(limb);
				this.ResetParamsEditor();
				return;
			}
			if (Widget.EnableMultiSelect)
			{
				this.selectedLimbs.Remove(limb);
				this.ResetParamsEditor();
			}
		}

		// Token: 0x06004979 RID: 18809 RVA: 0x002885DC File Offset: 0x002867DC
		private void OpenDoors()
		{
			foreach (Item item in Item.ItemList)
			{
				foreach (ItemComponent component in item.Components)
				{
					Door door = component as Door;
					if (door != null)
					{
						door.IsOpen = true;
					}
				}
			}
		}

		// Token: 0x0600497A RID: 18810 RVA: 0x00288678 File Offset: 0x00286878
		private void SaveSnapshot()
		{
			if (this.editJoints || this.editLimbs || this.editIK)
			{
				this.RagdollParams.StoreSnapshot();
			}
			if (this.editAnimations)
			{
				this.CurrentAnimation.StoreSnapshot();
			}
		}

		// Token: 0x0600497B RID: 18811 RVA: 0x002886B0 File Offset: 0x002868B0
		private void ToggleJointCreationMode()
		{
			CharacterEditorScreen.JointCreationMode jointCreationMode = this.jointCreationMode;
			if (jointCreationMode == CharacterEditorScreen.JointCreationMode.None)
			{
				this.jointCreationMode = CharacterEditorScreen.JointCreationMode.Select;
				this.SetToggle(this.spritesheetToggle, true);
				return;
			}
			if (jointCreationMode - CharacterEditorScreen.JointCreationMode.Select > 1)
			{
				return;
			}
			this.jointCreationMode = CharacterEditorScreen.JointCreationMode.None;
		}

		// Token: 0x0600497C RID: 18812 RVA: 0x002886EA File Offset: 0x002868EA
		private void ToggleLimbCreationMode()
		{
			this.isDrawingLimb = !this.isDrawingLimb;
			if (this.isDrawingLimb)
			{
				this.SetToggle(this.spritesheetToggle, true);
			}
		}

		// Token: 0x0600497D RID: 18813 RVA: 0x00288710 File Offset: 0x00286910
		private void DrawAnimationControls(SpriteBatch spriteBatch, float deltaTime)
		{
			CharacterEditorScreen.<>c__DisplayClass216_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass216_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			CS$<>8__locals1.collider = this.character.AnimController.Collider;
			Vector2 colliderDrawPos = this.cam.WorldToScreen(CS$<>8__locals1.collider.DrawPosition);
			CS$<>8__locals1.animParams = this.character.AnimController.CurrentAnimationParams;
			CS$<>8__locals1.groundedParams = (CS$<>8__locals1.animParams as GroundedMovementParams);
			IHumanAnimation humanParams = CS$<>8__locals1.animParams as IHumanAnimation;
			CS$<>8__locals1.humanGroundedParams = (CS$<>8__locals1.animParams as HumanGroundedParams);
			CS$<>8__locals1.humanSwimParams = (CS$<>8__locals1.animParams as HumanSwimParams);
			CS$<>8__locals1.fishParams = (CS$<>8__locals1.animParams as IFishAnimation);
			FishGroundedParams fishGroundedParams = CS$<>8__locals1.animParams as FishGroundedParams;
			CS$<>8__locals1.fishSwimParams = (CS$<>8__locals1.animParams as FishSwimParams);
			CS$<>8__locals1.head = this.character.AnimController.GetLimb(LimbType.Head, true, false, false);
			CS$<>8__locals1.torso = this.character.AnimController.GetLimb(LimbType.Torso, true, false, false);
			Limb tail = this.character.AnimController.GetLimb(LimbType.Tail, true, false, false);
			Limb legs = this.character.AnimController.GetLimb(LimbType.Legs, true, false, false);
			Limb limb2 = this.character.AnimController.GetLimb(LimbType.RightThigh, true, false, false) ?? this.character.AnimController.GetLimb(LimbType.LeftThigh, true, false, false);
			Limb foot = this.character.AnimController.GetLimb(LimbType.RightFoot, true, false, false) ?? this.character.AnimController.GetLimb(LimbType.LeftFoot, true, false, false);
			Limb hand = this.character.AnimController.GetLimb(LimbType.RightHand, true, false, false) ?? this.character.AnimController.GetLimb(LimbType.LeftHand, true, false, false);
			Limb arm = this.character.AnimController.GetLimb(LimbType.RightArm, true, false, false) ?? this.character.AnimController.GetLimb(LimbType.LeftArm, true, false, false);
			float dir = this.character.AnimController.Dir;
			if (!PlayerInput.KeyDown(Keys.LeftAlt) && (CS$<>8__locals1.animParams is IHumanAnimation || CS$<>8__locals1.animParams is GroundedMovementParams))
			{
				GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)(GameMain.GraphicsWidth / 2 - 120), 150f), CharacterEditorScreen.GetCharacterEditorTranslation("HoldLeftAltToAdjustCycleSpeed"), Color.White, new Color?(Color.Black * 0.5f), 10, GUIStyle.Font, ForceUpperCase.Inherit);
			}
			Vector2 referencePoint = this.cam.WorldToScreen((CS$<>8__locals1.head != null) ? CS$<>8__locals1.head.DrawPosition : CS$<>8__locals1.collider.DrawPosition);
			if (CS$<>8__locals1.<DrawAnimationControls>g__ShowCycleWidget|2())
			{
				this.GetAnimationWidget("CycleSpeed", Color.MediumPurple, new Color?(Color.Black), 20, 1.5f, WidgetShape.Circle, delegate(Widget w)
				{
					float multiplier = 0.5f;
					w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("CycleSpeed");
					w.Refresh = delegate()
					{
						Vector2 refPoint = CS$<>8__locals1.<>4__this.cam.WorldToScreen((CS$<>8__locals1.head != null) ? CS$<>8__locals1.head.DrawPosition : CS$<>8__locals1.collider.DrawPosition);
						Widget w;
						w.DrawPos = refPoint + CS$<>8__locals1.<DrawAnimationControls>g__GetScreenSpaceForward|1() * ConvertUnits.ToDisplayUnits(CS$<>8__locals1.<>4__this.CurrentAnimation.CycleSpeed * multiplier) * CS$<>8__locals1.<>4__this.Cam.Zoom;
						w = w;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("CycleSpeed"));
						defaultInterpolatedStringHandler.AppendLiteral(": ");
						defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.<>4__this.CurrentAnimation.CycleSpeed.FormatDoubleDecimal());
						w.Tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
					};
					w.MouseHeld += delegate(float dTime)
					{
						float speed = CS$<>8__locals1.<>4__this.CurrentAnimation.CycleSpeed + ConvertUnits.ToSimUnits(Vector2.Multiply(PlayerInput.MouseSpeed / multiplier, CS$<>8__locals1.<DrawAnimationControls>g__GetScreenSpaceForward|1()).Combine()) / CS$<>8__locals1.<>4__this.Cam.Zoom;
						CS$<>8__locals1.<>4__this.TryUpdateAnimParam("cyclespeed", speed);
						Widget w = w;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("CycleSpeed"));
						defaultInterpolatedStringHandler.AppendLiteral(": ");
						defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.<>4__this.CurrentAnimation.CycleSpeed.FormatDoubleDecimal());
						w.Tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
					};
					w.PreUpdate += delegate(float dTime)
					{
						if (!CS$<>8__locals1.<DrawAnimationControls>g__ShowCycleWidget|2())
						{
							w.Enabled = false;
						}
					};
					w.PreDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsControlled)
						{
							w.Refresh();
						}
					};
					w.PostDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsSelected)
						{
							GUI.DrawLine(CS$<>8__locals1.spriteBatch, w.DrawPos, CS$<>8__locals1.<>4__this.cam.WorldToScreen((CS$<>8__locals1.head != null) ? CS$<>8__locals1.head.DrawPosition : CS$<>8__locals1.collider.DrawPosition), Color.MediumPurple, 0f, 1f);
						}
					};
				}).Draw(CS$<>8__locals1.spriteBatch, deltaTime);
			}
			else
			{
				this.GetAnimationWidget("MovementSpeed", Color.Turquoise, new Color?(Color.Black), 20, 1.5f, WidgetShape.Circle, delegate(Widget w)
				{
					float multiplier = 0.5f;
					w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("MovementSpeed");
					w.Refresh = delegate()
					{
						Vector2 refPoint = CS$<>8__locals1.<>4__this.cam.WorldToScreen((CS$<>8__locals1.head != null) ? CS$<>8__locals1.head.DrawPosition : CS$<>8__locals1.collider.DrawPosition);
						w.DrawPos = refPoint + CS$<>8__locals1.<DrawAnimationControls>g__GetScreenSpaceForward|1() * ConvertUnits.ToDisplayUnits(CS$<>8__locals1.<>4__this.CurrentAnimation.MovementSpeed * multiplier) * CS$<>8__locals1.<>4__this.Cam.Zoom;
					};
					w.MouseHeld += delegate(float dTime)
					{
						float speed = CS$<>8__locals1.<>4__this.CurrentAnimation.MovementSpeed + ConvertUnits.ToSimUnits(Vector2.Multiply(PlayerInput.MouseSpeed / multiplier, CS$<>8__locals1.<DrawAnimationControls>g__GetScreenSpaceForward|1()).Combine()) / CS$<>8__locals1.<>4__this.Cam.Zoom;
						CS$<>8__locals1.<>4__this.TryUpdateAnimParam("movementspeed", MathHelper.Clamp(speed, 0.1f, 20f));
						if (CS$<>8__locals1.humanSwimParams != null)
						{
							CS$<>8__locals1.<>4__this.TryUpdateAnimParam("cyclespeed", CS$<>8__locals1.<>4__this.character.AnimController.CurrentAnimationParams.MovementSpeed);
						}
						Widget w = w;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("MovementSpeed"));
						defaultInterpolatedStringHandler.AppendLiteral(": ");
						defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.<>4__this.CurrentAnimation.MovementSpeed.FormatSingleDecimal());
						w.Tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
					};
					w.PreUpdate += delegate(float dTime)
					{
						if (CS$<>8__locals1.<DrawAnimationControls>g__ShowCycleWidget|2())
						{
							w.Enabled = false;
						}
					};
					w.PreDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsControlled)
						{
							w.Refresh();
						}
					};
					w.PostDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsSelected)
						{
							GUI.DrawLine(CS$<>8__locals1.spriteBatch, w.DrawPos, CS$<>8__locals1.<>4__this.Cam.WorldToScreen((CS$<>8__locals1.head != null) ? CS$<>8__locals1.head.DrawPosition : CS$<>8__locals1.collider.DrawPosition), Color.Turquoise, 0f, 1f);
						}
					};
				}).Draw(CS$<>8__locals1.spriteBatch, deltaTime);
			}
			if (CS$<>8__locals1.head != null)
			{
				this.DrawRadialWidget(CS$<>8__locals1.spriteBatch, this.Cam.WorldToScreen(CS$<>8__locals1.head.DrawPosition), CS$<>8__locals1.animParams.HeadAngle, CharacterEditorScreen.GetCharacterEditorTranslation("HeadAngle"), Color.White, delegate(float angle)
				{
					CS$<>8__locals1.<>4__this.TryUpdateAnimParam("headangle", angle);
				}, 25f, 10, -CS$<>8__locals1.collider.Rotation + CS$<>8__locals1.head.Params.GetSpriteOrientation() * dir, dir < 0f, true, null, true, true, 1);
				Color color = GUIStyle.Red;
				if (CS$<>8__locals1.animParams.IsGroundedAnimation)
				{
					if (CS$<>8__locals1.humanGroundedParams != null)
					{
						AnimController animController = this.character.AnimController;
						HumanoidAnimController humanAnimController = animController as HumanoidAnimController;
						if (humanAnimController != null)
						{
							this.GetAnimationWidget("HeadPosition", color, new Color?(Color.Black), 10, 2f, WidgetShape.Rectangle, delegate(Widget w)
							{
								w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("Head");
								w.Refresh = delegate()
								{
									w.DrawPos = CS$<>8__locals1.<>4__this.Cam.WorldToScreen(new Vector2(CS$<>8__locals1.head.DrawPosition.X + ConvertUnits.ToDisplayUnits(humanAnimController.HeadLeanAmount * CS$<>8__locals1.<>4__this.character.AnimController.Dir), ConvertUnits.ToDisplayUnits(CS$<>8__locals1.head.PullJointWorldAnchorB.Y)));
								};
								bool isHorizontal = false;
								bool isDirectionSet = false;
								w.MouseDown += delegate()
								{
									isDirectionSet = false;
								};
								w.MouseHeld += delegate(float dTime)
								{
									if (PlayerInput.MouseSpeed.NearlyEquals(Vector2.Zero))
									{
										return;
									}
									if (!isDirectionSet)
									{
										isHorizontal = (Math.Abs(PlayerInput.MouseSpeed.X) > Math.Abs(PlayerInput.MouseSpeed.Y));
										isDirectionSet = true;
									}
									Vector2 scaledInput = ConvertUnits.ToSimUnits(PlayerInput.MouseSpeed) / CS$<>8__locals1.<>4__this.Cam.Zoom;
									if (!PlayerInput.KeyDown(Keys.LeftAlt))
									{
										CS$<>8__locals1.<>4__this.TryUpdateAnimParam("headleanamount", CS$<>8__locals1.humanGroundedParams.HeadLeanAmount + scaledInput.X * CS$<>8__locals1.<>4__this.character.AnimController.Dir);
										w.Refresh();
										w.DrawPos = new Vector2(PlayerInput.MousePosition.X, w.DrawPos.Y);
										CS$<>8__locals1.<>4__this.TryUpdateAnimParam("headposition", CS$<>8__locals1.humanGroundedParams.HeadPosition - scaledInput.Y / CS$<>8__locals1.<>4__this.RagdollParams.JointScale);
										w.Refresh();
										w.DrawPos = new Vector2(w.DrawPos.X, PlayerInput.MousePosition.Y);
										return;
									}
									if (isHorizontal)
									{
										CS$<>8__locals1.<>4__this.TryUpdateAnimParam("headleanamount", CS$<>8__locals1.humanGroundedParams.HeadLeanAmount + scaledInput.X * CS$<>8__locals1.<>4__this.character.AnimController.Dir);
										w.Refresh();
										w.DrawPos = new Vector2(PlayerInput.MousePosition.X, w.DrawPos.Y);
										return;
									}
									CS$<>8__locals1.<>4__this.TryUpdateAnimParam("headposition", CS$<>8__locals1.humanGroundedParams.HeadPosition - scaledInput.Y / CS$<>8__locals1.<>4__this.RagdollParams.JointScale);
									w.Refresh();
									w.DrawPos = new Vector2(w.DrawPos.X, PlayerInput.MousePosition.Y);
								};
								w.PostDraw += delegate(SpriteBatch sB, float dTime)
								{
									if (!(w.IsControlled & isDirectionSet))
									{
										if (w.IsSelected)
										{
											GUI.DrawLine(CS$<>8__locals1.spriteBatch, w.DrawPos, CS$<>8__locals1.<>4__this.cam.WorldToScreen(CS$<>8__locals1.head.DrawPosition), color, 0f, 1f);
										}
										return;
									}
									if (!PlayerInput.KeyDown(Keys.LeftAlt))
									{
										GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(0f, w.DrawPos.Y), new Vector2((float)GameMain.GraphicsWidth, w.DrawPos.Y), color, 0f, 1f);
										GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(w.DrawPos.X, 0f), new Vector2(w.DrawPos.X, (float)GameMain.GraphicsHeight), color, 0f, 1f);
										return;
									}
									if (isHorizontal)
									{
										GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(0f, w.DrawPos.Y), new Vector2((float)GameMain.GraphicsWidth, w.DrawPos.Y), color, 0f, 1f);
										return;
									}
									GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(w.DrawPos.X, 0f), new Vector2(w.DrawPos.X, (float)GameMain.GraphicsHeight), color, 0f, 1f);
								};
							}).Draw(CS$<>8__locals1.spriteBatch, deltaTime);
							goto IL_4FC;
						}
					}
					if (CS$<>8__locals1.groundedParams != null)
					{
						this.GetAnimationWidget("HeadPosition", color, new Color?(Color.Black), 10, 2f, WidgetShape.Rectangle, delegate(Widget w)
						{
							w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("HeadPosition");
							w.Refresh = delegate()
							{
								w.DrawPos = CS$<>8__locals1.<>4__this.cam.WorldToScreen(new Vector2(CS$<>8__locals1.head.DrawPosition.X, ConvertUnits.ToDisplayUnits(CS$<>8__locals1.head.PullJointWorldAnchorB.Y)));
							};
							w.MouseHeld += delegate(float dTime)
							{
								w.DrawPos = CS$<>8__locals1.<>4__this.cam.WorldToScreen(new Vector2(CS$<>8__locals1.head.DrawPosition.X, ConvertUnits.ToDisplayUnits(CS$<>8__locals1.head.PullJointWorldAnchorB.Y)));
								Vector2 scaledInput = ConvertUnits.ToSimUnits(PlayerInput.MouseSpeed) / CS$<>8__locals1.<>4__this.Cam.Zoom / CS$<>8__locals1.<>4__this.RagdollParams.JointScale;
								CS$<>8__locals1.<>4__this.TryUpdateAnimParam("headposition", CS$<>8__locals1.groundedParams.HeadPosition - scaledInput.Y);
							};
							w.PostDraw += delegate(SpriteBatch sB, float dTime)
							{
								if (w.IsControlled)
								{
									GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(w.DrawPos.X, 0f), new Vector2(w.DrawPos.X, (float)GameMain.GraphicsHeight), color, 0f, 1f);
								}
							};
						}).Draw(CS$<>8__locals1.spriteBatch, deltaTime);
					}
				}
			}
			IL_4FC:
			if (CS$<>8__locals1.torso != null)
			{
				referencePoint = CS$<>8__locals1.torso.DrawPosition;
				if (CS$<>8__locals1.animParams is HumanGroundedParams || CS$<>8__locals1.animParams is HumanSwimParams)
				{
					Vector2 f = Vector2.Transform(Vector2.UnitY, Matrix.CreateRotationZ(CS$<>8__locals1.collider.Rotation));
					referencePoint -= f * 25f;
				}
				this.DrawRadialWidget(CS$<>8__locals1.spriteBatch, this.cam.WorldToScreen(referencePoint), CS$<>8__locals1.animParams.TorsoAngle, CharacterEditorScreen.GetCharacterEditorTranslation("TorsoAngle"), Color.White, delegate(float angle)
				{
					CS$<>8__locals1.<>4__this.TryUpdateAnimParam("torsoangle", angle);
				}, 30f, 10, -CS$<>8__locals1.collider.Rotation + CS$<>8__locals1.torso.Params.GetSpriteOrientation() * dir, dir < 0f, true, null, true, true, 1);
				Color color = Color.DodgerBlue;
				if (CS$<>8__locals1.animParams.IsGroundedAnimation)
				{
					if (CS$<>8__locals1.humanGroundedParams != null)
					{
						AnimController animController = this.character.AnimController;
						HumanoidAnimController humanAnimController = animController as HumanoidAnimController;
						if (humanAnimController != null)
						{
							this.GetAnimationWidget("TorsoPosition", color, new Color?(Color.Black), 10, 2f, WidgetShape.Rectangle, delegate(Widget w)
							{
								w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("Torso");
								w.Refresh = delegate()
								{
									w.DrawPos = CS$<>8__locals1.<>4__this.cam.WorldToScreen(new Vector2(CS$<>8__locals1.torso.DrawPosition.X + ConvertUnits.ToDisplayUnits(humanAnimController.TorsoLeanAmount * CS$<>8__locals1.<>4__this.character.AnimController.Dir), ConvertUnits.ToDisplayUnits(CS$<>8__locals1.torso.PullJointWorldAnchorB.Y)));
								};
								bool isHorizontal = false;
								bool isDirectionSet = false;
								w.MouseDown += delegate()
								{
									isDirectionSet = false;
								};
								w.MouseHeld += delegate(float dTime)
								{
									if (PlayerInput.MouseSpeed.NearlyEquals(Vector2.Zero))
									{
										return;
									}
									if (!isDirectionSet)
									{
										isHorizontal = (Math.Abs(PlayerInput.MouseSpeed.X) > Math.Abs(PlayerInput.MouseSpeed.Y));
										isDirectionSet = true;
									}
									Vector2 scaledInput = ConvertUnits.ToSimUnits(PlayerInput.MouseSpeed) / CS$<>8__locals1.<>4__this.Cam.Zoom;
									if (!PlayerInput.KeyDown(Keys.LeftAlt))
									{
										CS$<>8__locals1.<>4__this.TryUpdateAnimParam("torsoleanamount", CS$<>8__locals1.humanGroundedParams.TorsoLeanAmount + scaledInput.X * CS$<>8__locals1.<>4__this.character.AnimController.Dir);
										w.Refresh();
										w.DrawPos = new Vector2(PlayerInput.MousePosition.X, w.DrawPos.Y);
										CS$<>8__locals1.<>4__this.TryUpdateAnimParam("torsoposition", CS$<>8__locals1.humanGroundedParams.TorsoPosition - scaledInput.Y / CS$<>8__locals1.<>4__this.RagdollParams.JointScale);
										w.Refresh();
										w.DrawPos = new Vector2(w.DrawPos.X, PlayerInput.MousePosition.Y);
										return;
									}
									if (isHorizontal)
									{
										CS$<>8__locals1.<>4__this.TryUpdateAnimParam("torsoleanamount", CS$<>8__locals1.humanGroundedParams.TorsoLeanAmount + scaledInput.X * CS$<>8__locals1.<>4__this.character.AnimController.Dir);
										w.Refresh();
										w.DrawPos = new Vector2(PlayerInput.MousePosition.X, w.DrawPos.Y);
										return;
									}
									CS$<>8__locals1.<>4__this.TryUpdateAnimParam("torsoposition", CS$<>8__locals1.humanGroundedParams.TorsoPosition - scaledInput.Y / CS$<>8__locals1.<>4__this.RagdollParams.JointScale);
									w.Refresh();
									w.DrawPos = new Vector2(w.DrawPos.X, PlayerInput.MousePosition.Y);
								};
								w.PostDraw += delegate(SpriteBatch sB, float dTime)
								{
									if (!(w.IsControlled & isDirectionSet))
									{
										if (w.IsSelected)
										{
											GUI.DrawLine(CS$<>8__locals1.spriteBatch, w.DrawPos, CS$<>8__locals1.<>4__this.cam.WorldToScreen(CS$<>8__locals1.torso.DrawPosition), color, 0f, 1f);
										}
										return;
									}
									if (!PlayerInput.KeyDown(Keys.LeftAlt))
									{
										GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(0f, w.DrawPos.Y), new Vector2((float)GameMain.GraphicsWidth, w.DrawPos.Y), color, 0f, 1f);
										GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(w.DrawPos.X, 0f), new Vector2(w.DrawPos.X, (float)GameMain.GraphicsHeight), color, 0f, 1f);
										return;
									}
									if (isHorizontal)
									{
										GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(0f, w.DrawPos.Y), new Vector2((float)GameMain.GraphicsWidth, w.DrawPos.Y), color, 0f, 1f);
										return;
									}
									GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(w.DrawPos.X, 0f), new Vector2(w.DrawPos.X, (float)GameMain.GraphicsHeight), color, 0f, 1f);
								};
							}).Draw(CS$<>8__locals1.spriteBatch, deltaTime);
							goto IL_73E;
						}
					}
					if (CS$<>8__locals1.groundedParams != null)
					{
						this.GetAnimationWidget("TorsoPosition", color, new Color?(Color.Black), 10, 2f, WidgetShape.Rectangle, delegate(Widget w)
						{
							w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("TorsoPosition");
							w.Refresh = delegate()
							{
								w.DrawPos = CS$<>8__locals1.<>4__this.SimToScreen(CS$<>8__locals1.torso.SimPosition.X, CS$<>8__locals1.torso.PullJointWorldAnchorB.Y);
							};
							w.MouseHeld += delegate(float dTime)
							{
								w.DrawPos = CS$<>8__locals1.<>4__this.SimToScreen(CS$<>8__locals1.torso.SimPosition.X, CS$<>8__locals1.torso.PullJointWorldAnchorB.Y);
								Vector2 scaledInput = ConvertUnits.ToSimUnits(PlayerInput.MouseSpeed) / CS$<>8__locals1.<>4__this.Cam.Zoom / CS$<>8__locals1.<>4__this.RagdollParams.JointScale;
								CS$<>8__locals1.<>4__this.TryUpdateAnimParam("torsoposition", CS$<>8__locals1.groundedParams.TorsoPosition - scaledInput.Y);
							};
							w.PostDraw += delegate(SpriteBatch sB, float dTime)
							{
								if (w.IsControlled)
								{
									GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(w.DrawPos.X, 0f), new Vector2(w.DrawPos.X, (float)GameMain.GraphicsHeight), color, 0f, 1f);
								}
							};
						}).Draw(CS$<>8__locals1.spriteBatch, deltaTime);
					}
				}
			}
			IL_73E:
			if (tail != null && CS$<>8__locals1.fishParams != null)
			{
				this.DrawRadialWidget(CS$<>8__locals1.spriteBatch, this.cam.WorldToScreen(tail.DrawPosition), CS$<>8__locals1.fishParams.TailAngle, CharacterEditorScreen.GetCharacterEditorTranslation("TailAngle"), Color.White, delegate(float angle)
				{
					CS$<>8__locals1.<>4__this.TryUpdateAnimParam("tailangle", angle);
				}, 25f, 10, -CS$<>8__locals1.collider.Rotation + tail.Params.GetSpriteOrientation() * dir, dir < 0f, true, null, true, true, 1);
			}
			if (foot != null)
			{
				if (CS$<>8__locals1.fishParams != null)
				{
					Vector2 colliderBottom = this.character.AnimController.GetColliderBottom();
					Limb[] limbs = this.character.AnimController.Limbs;
					for (int i = 0; i < limbs.Length; i++)
					{
						Limb limb = limbs[i];
						if (limb.type == LimbType.LeftFoot || limb.type == LimbType.RightFoot)
						{
							if (!CS$<>8__locals1.fishParams.FootAnglesInRadians.ContainsKey(limb.Params.ID))
							{
								CS$<>8__locals1.fishParams.FootAnglesInRadians[limb.Params.ID] = 0f;
							}
							this.DrawRadialWidget(CS$<>8__locals1.spriteBatch, this.cam.WorldToScreen(new Vector2(limb.DrawPosition.X, ConvertUnits.ToDisplayUnits(colliderBottom.Y))), MathHelper.ToDegrees(CS$<>8__locals1.fishParams.FootAnglesInRadians[limb.Params.ID]), CharacterEditorScreen.GetCharacterEditorTranslation("FootAngle"), Color.White, delegate(float angle)
							{
								CS$<>8__locals1.fishParams.FootAnglesInRadians[limb.Params.ID] = MathHelper.ToRadians(angle);
								CS$<>8__locals1.<>4__this.TryUpdateAnimParam("footangles", CS$<>8__locals1.fishParams.FootAngles);
							}, 25f, 10, -CS$<>8__locals1.collider.Rotation + limb.Params.GetSpriteOrientation() * dir, dir < 0f, true, new bool?(true), true, false, 1);
						}
					}
				}
				else if (humanParams != null)
				{
					this.DrawRadialWidget(CS$<>8__locals1.spriteBatch, this.cam.WorldToScreen(foot.DrawPosition), humanParams.FootAngle, CharacterEditorScreen.GetCharacterEditorTranslation("FootAngle"), Color.White, delegate(float angle)
					{
						CS$<>8__locals1.<>4__this.TryUpdateAnimParam("footangle", angle);
					}, 25f, 10, -CS$<>8__locals1.collider.Rotation + foot.Params.GetSpriteOrientation() * dir, dir > 0f, true, null, true, false, 1);
				}
				if (CS$<>8__locals1.groundedParams != null)
				{
					this.GetAnimationWidget("StepSize", Color.LimeGreen, new Color?(Color.Black), 10, 2f, WidgetShape.Rectangle, delegate(Widget w)
					{
						w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("StepSize");
						w.Refresh = delegate()
						{
							Vector2 refPoint = CS$<>8__locals1.<>4__this.cam.WorldToScreen(new Vector2(CS$<>8__locals1.<>4__this.character.AnimController.Collider.DrawPosition.X, CS$<>8__locals1.<>4__this.character.AnimController.GetColliderBottom().Y));
							Vector2 stepSize = ConvertUnits.ToDisplayUnits(CS$<>8__locals1.<>4__this.character.AnimController.StepSize.Value);
							w.DrawPos = refPoint + new Vector2(stepSize.X * CS$<>8__locals1.<>4__this.character.AnimController.Dir, -stepSize.Y) * CS$<>8__locals1.<>4__this.Cam.Zoom;
						};
						w.MouseHeld += delegate(float dTime)
						{
							Widget w;
							w.DrawPos = PlayerInput.MousePosition;
							Vector2 transformedInput = ConvertUnits.ToSimUnits(new Vector2(PlayerInput.MouseSpeed.X * CS$<>8__locals1.<>4__this.character.AnimController.Dir, -PlayerInput.MouseSpeed.Y)) / CS$<>8__locals1.<>4__this.Cam.Zoom / CS$<>8__locals1.<>4__this.RagdollParams.JointScale;
							CS$<>8__locals1.<>4__this.TryUpdateAnimParam("stepsize", CS$<>8__locals1.groundedParams.StepSize + transformedInput);
							w = w;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("StepSize"));
							defaultInterpolatedStringHandler.AppendLiteral(": ");
							defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.groundedParams.StepSize.FormatDoubleDecimal());
							w.Tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
						};
						w.PostDraw += delegate(SpriteBatch sp, float dTime)
						{
							if (w.IsSelected)
							{
								GUI.DrawLine(sp, w.DrawPos, CS$<>8__locals1.<>4__this.SimToScreen(CS$<>8__locals1.<>4__this.character.AnimController.GetColliderBottom()), Color.LimeGreen, 0f, 1f);
							}
						};
					}).Draw(CS$<>8__locals1.spriteBatch, deltaTime);
				}
			}
			if (CS$<>8__locals1.humanGroundedParams != null)
			{
				if (hand != null || arm != null)
				{
					this.GetAnimationWidget("HandMoveAmount", GUIStyle.Green, new Color?(Color.Black), 10, 2f, WidgetShape.Rectangle, delegate(Widget w)
					{
						w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("HandMoveAmount");
						float offset = 10f;
						w.Refresh = delegate()
						{
							Vector2 refPoint = CS$<>8__locals1.<>4__this.cam.WorldToScreen(CS$<>8__locals1.<>4__this.character.AnimController.Collider.DrawPosition + CS$<>8__locals1.<DrawAnimationControls>g__GetSimSpaceForward|0() * offset);
							Vector2 handMovement = ConvertUnits.ToDisplayUnits(CS$<>8__locals1.humanGroundedParams.HandMoveAmount);
							w.DrawPos = refPoint + new Vector2(handMovement.X * CS$<>8__locals1.<>4__this.character.AnimController.Dir, handMovement.Y) * CS$<>8__locals1.<>4__this.Cam.Zoom;
						};
						w.MouseHeld += delegate(float dTime)
						{
							Widget w;
							w.DrawPos = PlayerInput.MousePosition;
							Vector2 transformedInput = ConvertUnits.ToSimUnits(new Vector2(PlayerInput.MouseSpeed.X * CS$<>8__locals1.<>4__this.character.AnimController.Dir, PlayerInput.MouseSpeed.Y) / CS$<>8__locals1.<>4__this.Cam.Zoom);
							CS$<>8__locals1.<>4__this.TryUpdateAnimParam("handmoveamount", CS$<>8__locals1.humanGroundedParams.HandMoveAmount + transformedInput);
							w = w;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("HandMoveAmount"));
							defaultInterpolatedStringHandler.AppendLiteral(": ");
							defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.humanGroundedParams.HandMoveAmount.FormatDoubleDecimal());
							w.Tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
						};
						w.PostDraw += delegate(SpriteBatch sp, float dTime)
						{
							if (w.IsSelected)
							{
								GUI.DrawLine(sp, w.DrawPos, CS$<>8__locals1.<>4__this.cam.WorldToScreen(CS$<>8__locals1.<>4__this.character.AnimController.Collider.DrawPosition + CS$<>8__locals1.<DrawAnimationControls>g__GetSimSpaceForward|0() * offset), GUIStyle.Green, 0f, 1f);
							}
						};
					}).Draw(CS$<>8__locals1.spriteBatch, deltaTime);
				}
			}
			else if (tail != null && CS$<>8__locals1.fishSwimParams != null)
			{
				CharacterEditorScreen.<>c__DisplayClass216_14 CS$<>8__locals7 = new CharacterEditorScreen.<>c__DisplayClass216_14();
				CS$<>8__locals7.CS$<>8__locals14 = CS$<>8__locals1;
				CS$<>8__locals7.amplitudeMultiplier = 20f;
				CS$<>8__locals7.lengthMultiplier = 20f;
				int points = 1000;
				Widget lengthWidget = this.GetAnimationWidget("WaveLength", Color.NavajoWhite, new Color?(Color.Black), 15, 2f, WidgetShape.Circle, delegate(Widget w)
				{
					w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("TailMovementSpeed");
					w.Refresh = delegate()
					{
						w.DrawPos = CS$<>8__locals7.<DrawAnimationControls>g__GetDrawPos|49();
					};
					Widget w2 = w;
					Action<float> value;
					if ((value = CS$<>8__locals7.<>9__56) == null)
					{
						value = (CS$<>8__locals7.<>9__56 = delegate(float dTime)
						{
							float input = Vector2.Multiply(ConvertUnits.ToSimUnits(PlayerInput.MouseSpeed), CS$<>8__locals7.CS$<>8__locals14.<DrawAnimationControls>g__GetScreenSpaceForward|1()).Combine() / CS$<>8__locals7.CS$<>8__locals14.<>4__this.Cam.Zoom * CS$<>8__locals7.lengthMultiplier;
							CS$<>8__locals7.CS$<>8__locals14.<>4__this.TryUpdateAnimParam("wavelength", MathHelper.Clamp(CS$<>8__locals7.CS$<>8__locals14.fishSwimParams.WaveLength - input, 0f, 200f));
						});
					}
					w2.MouseHeld += value;
					w.PreDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsControlled)
						{
							w.Refresh();
						}
					};
				});
				Widget amplitudeWidget = this.GetAnimationWidget("WaveAmplitude", Color.NavajoWhite, new Color?(Color.Black), 15, 2f, WidgetShape.Circle, delegate(Widget w)
				{
					w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("TailMovementAmount");
					w.Refresh = delegate()
					{
						w.DrawPos = CS$<>8__locals7.<DrawAnimationControls>g__GetControlPoint|52();
					};
					Widget w2 = w;
					Action<float> value;
					if ((value = CS$<>8__locals7.<>9__59) == null)
					{
						value = (CS$<>8__locals7.<>9__59 = delegate(float dTime)
						{
							float input = Vector2.Multiply(ConvertUnits.ToSimUnits(PlayerInput.MouseSpeed), CS$<>8__locals7.CS$<>8__locals14.<DrawAnimationControls>g__GetScreenSpaceForward|1().Right()).Combine() * CS$<>8__locals7.CS$<>8__locals14.<>4__this.character.AnimController.Dir / CS$<>8__locals7.CS$<>8__locals14.<>4__this.Cam.Zoom * CS$<>8__locals7.amplitudeMultiplier;
							CS$<>8__locals7.CS$<>8__locals14.<>4__this.TryUpdateAnimParam("waveamplitude", MathHelper.Clamp(CS$<>8__locals7.CS$<>8__locals14.fishSwimParams.WaveAmplitude + input, -100f, 100f));
						});
					}
					w2.MouseHeld += value;
					w.PreDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsControlled)
						{
							w.Refresh();
						}
					};
				});
				if (lengthWidget.IsControlled || amplitudeWidget.IsControlled)
				{
					GUI.DrawSineWithDots(CS$<>8__locals7.CS$<>8__locals14.spriteBatch, CS$<>8__locals7.CS$<>8__locals14.<DrawAnimationControls>g__GetRefPoint|48(), -CS$<>8__locals7.<DrawAnimationControls>g__GetDir|50(), CS$<>8__locals7.<DrawAnimationControls>g__GetAmplitude|46(), CS$<>8__locals7.<DrawAnimationControls>g__GetWaveLength|47(), 5000f, points, Color.NavajoWhite, 2);
				}
				lengthWidget.Draw(CS$<>8__locals7.CS$<>8__locals14.spriteBatch, deltaTime);
				amplitudeWidget.Draw(CS$<>8__locals7.CS$<>8__locals14.spriteBatch, deltaTime);
			}
			else if (CS$<>8__locals1.humanSwimParams != null)
			{
				CharacterEditorScreen.<>c__DisplayClass216_17 CS$<>8__locals8 = new CharacterEditorScreen.<>c__DisplayClass216_17();
				CS$<>8__locals8.CS$<>8__locals17 = CS$<>8__locals1;
				CS$<>8__locals8.amplitudeMultiplier = 5f;
				CS$<>8__locals8.lengthMultiplier = 5f;
				int points2 = 1000;
				Widget lengthWidget2 = this.GetAnimationWidget("LegMovementSpeed", Color.NavajoWhite, new Color?(Color.Black), 15, 2f, WidgetShape.Circle, delegate(Widget w)
				{
					w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("LegMovementSpeed");
					w.Refresh = delegate()
					{
						w.DrawPos = CS$<>8__locals8.<DrawAnimationControls>g__GetDrawPos|64();
					};
					Widget w2 = w;
					Action<float> value;
					if ((value = CS$<>8__locals8.<>9__72) == null)
					{
						value = (CS$<>8__locals8.<>9__72 = delegate(float dTime)
						{
							float input = Vector2.Multiply(ConvertUnits.ToSimUnits(PlayerInput.MouseSpeed), CS$<>8__locals8.CS$<>8__locals17.<DrawAnimationControls>g__GetScreenSpaceForward|1()).Combine() / CS$<>8__locals8.CS$<>8__locals17.<>4__this.Cam.Zoom * CS$<>8__locals8.lengthMultiplier;
							CS$<>8__locals8.CS$<>8__locals17.<>4__this.TryUpdateAnimParam("legcyclelength", MathHelper.Clamp(CS$<>8__locals8.CS$<>8__locals17.humanSwimParams.LegCycleLength - input, 0f, 20f));
						});
					}
					w2.MouseHeld += value;
					w.PreDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsControlled)
						{
							w.Refresh();
						}
					};
				});
				Widget amplitudeWidget2 = this.GetAnimationWidget("LegMovementAmount", Color.NavajoWhite, new Color?(Color.Black), 15, 2f, WidgetShape.Circle, delegate(Widget w)
				{
					w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("LegMovementAmount");
					w.Refresh = delegate()
					{
						w.DrawPos = CS$<>8__locals8.<DrawAnimationControls>g__GetControlPoint|67();
					};
					Widget w2 = w;
					Action<float> value;
					if ((value = CS$<>8__locals8.<>9__75) == null)
					{
						value = (CS$<>8__locals8.<>9__75 = delegate(float dTime)
						{
							float input = Vector2.Multiply(ConvertUnits.ToSimUnits(PlayerInput.MouseSpeed), CS$<>8__locals8.CS$<>8__locals17.<DrawAnimationControls>g__GetScreenSpaceForward|1().Right()).Combine() * CS$<>8__locals8.CS$<>8__locals17.<>4__this.character.AnimController.Dir / CS$<>8__locals8.CS$<>8__locals17.<>4__this.Cam.Zoom * CS$<>8__locals8.amplitudeMultiplier;
							CS$<>8__locals8.CS$<>8__locals17.<>4__this.TryUpdateAnimParam("legmoveamount", MathHelper.Clamp(CS$<>8__locals8.CS$<>8__locals17.humanSwimParams.LegMoveAmount + input, -2f, 2f));
						});
					}
					w2.MouseHeld += value;
					w.PreDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsControlled)
						{
							w.Refresh();
						}
					};
				});
				if (lengthWidget2.IsControlled || amplitudeWidget2.IsControlled)
				{
					GUI.DrawSineWithDots(CS$<>8__locals8.CS$<>8__locals17.spriteBatch, CS$<>8__locals8.CS$<>8__locals17.<DrawAnimationControls>g__GetRefPoint|63(), -CS$<>8__locals8.<DrawAnimationControls>g__GetDir|65(), CS$<>8__locals8.<DrawAnimationControls>g__GetAmplitude|61(), CS$<>8__locals8.<DrawAnimationControls>g__GetWaveLength|62(), 5000f, points2, Color.NavajoWhite, 2);
				}
				lengthWidget2.Draw(CS$<>8__locals8.CS$<>8__locals17.spriteBatch, deltaTime);
				amplitudeWidget2.Draw(CS$<>8__locals8.CS$<>8__locals17.spriteBatch, deltaTime);
				this.GetAnimationWidget("HandMoveAmount", GUIStyle.Green, new Color?(Color.Black), 10, 2f, WidgetShape.Rectangle, delegate(Widget w)
				{
					w.Tooltip = CharacterEditorScreen.GetCharacterEditorTranslation("HandMoveAmount");
					float offset = 40f;
					w.Refresh = delegate()
					{
						Vector2 refPoint = CS$<>8__locals8.CS$<>8__locals17.<>4__this.cam.WorldToScreen(CS$<>8__locals8.CS$<>8__locals17.collider.DrawPosition + CS$<>8__locals8.CS$<>8__locals17.<DrawAnimationControls>g__GetSimSpaceForward|0() * offset);
						Vector2 handMovement = ConvertUnits.ToDisplayUnits(CS$<>8__locals8.CS$<>8__locals17.humanSwimParams.HandMoveAmount);
						w.DrawPos = refPoint + new Vector2(handMovement.X * CS$<>8__locals8.CS$<>8__locals17.<>4__this.character.AnimController.Dir, handMovement.Y) * CS$<>8__locals8.CS$<>8__locals17.<>4__this.Cam.Zoom;
					};
					w.MouseHeld += delegate(float dTime)
					{
						Widget w;
						w.DrawPos = PlayerInput.MousePosition;
						Vector2 transformedInput = ConvertUnits.ToSimUnits(new Vector2(PlayerInput.MouseSpeed.X * CS$<>8__locals8.CS$<>8__locals17.<>4__this.character.AnimController.Dir, PlayerInput.MouseSpeed.Y)) / CS$<>8__locals8.CS$<>8__locals17.<>4__this.Cam.Zoom;
						Vector2 handMovement = CS$<>8__locals8.CS$<>8__locals17.humanSwimParams.HandMoveAmount + transformedInput;
						CS$<>8__locals8.CS$<>8__locals17.<>4__this.TryUpdateAnimParam("handmoveamount", handMovement);
						CS$<>8__locals8.CS$<>8__locals17.<>4__this.TryUpdateAnimParam("handcyclespeed", handMovement.X * 4f);
						w = w;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("HandMoveAmount"));
						defaultInterpolatedStringHandler.AppendLiteral(": ");
						defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals8.CS$<>8__locals17.humanSwimParams.HandMoveAmount.FormatDoubleDecimal());
						w.Tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
					};
					w.PostDraw += delegate(SpriteBatch sp, float dTime)
					{
						if (w.IsSelected)
						{
							GUI.DrawLine(sp, w.DrawPos, CS$<>8__locals8.CS$<>8__locals17.<>4__this.cam.WorldToScreen(CS$<>8__locals8.CS$<>8__locals17.collider.DrawPosition + CS$<>8__locals8.CS$<>8__locals17.<DrawAnimationControls>g__GetSimSpaceForward|0() * offset), GUIStyle.Green, 0f, 1f);
						}
					};
				}).Draw(CS$<>8__locals8.CS$<>8__locals17.spriteBatch, deltaTime);
			}
			foreach (Limb limb3 in this.character.AnimController.Limbs)
			{
				if (limb3.type == LimbType.LeftFoot || limb3.type == LimbType.RightFoot)
				{
					GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, this.SimToScreen(limb3.DebugRefPos) - Vector2.One * 3f, Vector2.One * 6f, Color.White, true, 0f, 1f);
					GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, this.SimToScreen(limb3.DebugTargetPos) - Vector2.One * 3f, Vector2.One * 6f, GUIStyle.Green, true, 0f, 1f);
				}
			}
		}

		// Token: 0x0600497E RID: 18814 RVA: 0x00289518 File Offset: 0x00287718
		private Vector2[] GetLimbPhysicRect(Limb limb)
		{
			Vector2 size = ConvertUnits.ToDisplayUnits(limb.body.GetSize()) * this.Cam.Zoom;
			Vector2 up = VectorExtensions.BackwardFlipped(limb.Rotation, 1f);
			Vector2 limbScreenPos = this.cam.WorldToScreen(limb.DrawPosition);
			this.corners = MathUtils.GetImaginaryRect(this.corners, up, limbScreenPos, size);
			return this.corners;
		}

		// Token: 0x0600497F RID: 18815 RVA: 0x00289584 File Offset: 0x00287784
		private void DrawLimbEditor(SpriteBatch spriteBatch)
		{
			float inputMultiplier = 0.5f;
			foreach (Limb limb in this.character.AnimController.Limbs)
			{
				if (limb != null && limb.ActiveSprite != null)
				{
					Vector2 origin = limb.ActiveSprite.Origin;
					Rectangle sourceRect = limb.ActiveSprite.SourceRect;
					Vector2 limbScreenPos = this.cam.WorldToScreen(limb.DrawPosition);
					bool isSelected = this.selectedLimbs.Contains(limb);
					this.corners = this.GetLimbPhysicRect(limb);
					if (isSelected && this.jointStartLimb != limb && this.jointEndLimb != limb)
					{
						GUI.DrawRectangle(spriteBatch, this.corners, Color.Yellow, 0f, 3f);
					}
					if (GUI.MouseOn == null && Widget.SelectedWidgets.None(null) && !this.spriteSheetRect.Contains(PlayerInput.MousePosition) && MathUtils.RectangleContainsPoint(this.corners, PlayerInput.MousePosition))
					{
						if (isSelected)
						{
							if (!this.lockSpriteOrigin && PlayerInput.PrimaryMouseButtonHeld())
							{
								Vector2 forward = Vector2.Transform(Vector2.UnitY, Matrix.CreateRotationZ(limb.Rotation));
								Vector2 input = -this.scaledMouseSpeed * inputMultiplier / this.Cam.Zoom / limb.Scale / limb.TextureScale;
								Sprite sprite = limb.ActiveSprite;
								origin += input.TransformVector(forward);
								Vector2 max = new Vector2((float)sourceRect.Width, (float)sourceRect.Height);
								sprite.Origin = origin.Clamp(Vector2.Zero, max);
								if (limb.DamagedSprite != null)
								{
									limb.DamagedSprite.Origin = sprite.Origin;
								}
								if (this.character.AnimController.IsFlipped)
								{
									origin.X = Math.Abs(origin.X - (float)sourceRect.Width);
								}
								this.TryUpdateLimbParam(limb, "origin", limb.ActiveSprite.RelativeOrigin);
								if (this.limbPairEditing)
								{
									this.UpdateOtherLimbs(limb, delegate(Limb otherLimb)
									{
										otherLimb.ActiveSprite.Origin = sprite.Origin;
										if (otherLimb.DamagedSprite != null)
										{
											otherLimb.DamagedSprite.Origin = sprite.Origin;
										}
										this.TryUpdateLimbParam(otherLimb, "origin", otherLimb.ActiveSprite.RelativeOrigin);
									});
								}
								GUI.DrawString(spriteBatch, limbScreenPos + new Vector2(10f, -10f), limb.ActiveSprite.RelativeOrigin.FormatDoubleDecimal(), Color.Yellow, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
							}
						}
						else
						{
							GUI.DrawRectangle(spriteBatch, this.corners, Color.White, 0f, 1f);
							GUI.DrawString(spriteBatch, limbScreenPos + new Vector2(10f, -10f), limb.Name, Color.White, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
						}
					}
				}
			}
		}

		// Token: 0x06004980 RID: 18816 RVA: 0x00289884 File Offset: 0x00287A84
		private void DrawRagdoll(SpriteBatch spriteBatch, float deltaTime)
		{
			bool altDown = PlayerInput.KeyDown(Keys.LeftAlt);
			if (!altDown && this.editJoints && this.selectedJoints.Any<LimbJoint>() && this.jointCreationMode == CharacterEditorScreen.JointCreationMode.None)
			{
				GUI.DrawString(spriteBatch, new Vector2((float)(GameMain.GraphicsWidth / 2 - 180), 100f), CharacterEditorScreen.GetCharacterEditorTranslation("HoldLeftAltToManipulateJoint"), Color.White, new Color?(Color.Black * 0.5f), 10, GUIStyle.Font, ForceUpperCase.Inherit);
			}
			Limb[] limbs = this.character.AnimController.Limbs;
			for (int k = 0; k < limbs.Length; k++)
			{
				Limb limb = limbs[k];
				if (this.editIK && (limb.type == LimbType.LeftFoot || limb.type == LimbType.RightFoot || limb.type == LimbType.LeftHand || limb.type == LimbType.RightHand))
				{
					Vector2 pullJointWidgetSize = new Vector2(5f, 5f);
					Vector2 tformedPullPos = this.SimToScreen(limb.PullJointWorldAnchorA) + limb.body.DrawPositionOffset;
					GUI.DrawRectangle(spriteBatch, tformedPullPos - pullJointWidgetSize / 2f, pullJointWidgetSize, GUIStyle.Red, true, 0f, 1f);
					this.DrawWidget(spriteBatch, tformedPullPos, CharacterEditorScreen.WidgetType.Rectangle, 8, Color.Cyan, "IK (" + limb.Name + ")", delegate
					{
						if (!this.selectedLimbs.Contains(limb))
						{
							this.selectedLimbs.Add(limb);
							this.ResetParamsEditor();
						}
						limb.PullJointWorldAnchorA = this.ScreenToSim(PlayerInput.MousePosition);
						this.TryUpdateLimbParam(limb, "pullpos", ConvertUnits.ToDisplayUnits(limb.PullJointLocalAnchorA / limb.Params.Scale / limb.Params.Ragdoll.LimbScale));
						GUI.DrawLine(spriteBatch, this.SimToScreen(limb.SimPosition), tformedPullPos, Color.MediumPurple, 0f, 1f);
					}, null, false, null);
				}
				LimbJoint[] limbJoints = this.character.AnimController.LimbJoints;
				int l = 0;
				while (l < limbJoints.Length)
				{
					LimbJoint joint = limbJoints[l];
					Vector2 jointPos = Vector2.Zero;
					Vector2 otherPos = Vector2.Zero;
					Vector2 anchorPosA = ConvertUnits.ToDisplayUnits(joint.LocalAnchorA);
					Vector2 anchorPosB = ConvertUnits.ToDisplayUnits(joint.LocalAnchorB);
					if (joint.BodyA == limb.body.FarseerBody)
					{
						jointPos = anchorPosA;
						goto IL_2D3;
					}
					if (joint.BodyB == limb.body.FarseerBody)
					{
						jointPos = anchorPosB;
						goto IL_2D3;
					}
					IL_944:
					l++;
					continue;
					IL_2D3:
					Vector2 limbScreenPos = this.cam.WorldToScreen(limb.DrawPosition);
					Vector2 f = Vector2.Transform(jointPos, Matrix.CreateRotationZ(limb.Rotation));
					f.Y = -f.Y;
					Vector2 tformedJointPos = limbScreenPos + f * this.Cam.Zoom;
					if (this.drawSkeleton)
					{
						spriteBatch.DrawPoint(limbScreenPos, Color.Black, 5f);
						spriteBatch.DrawPoint(limbScreenPos, Color.White, 1f);
						GUI.DrawLine(spriteBatch, limbScreenPos, tformedJointPos, Color.Black, 0f, 3f);
						GUI.DrawLine(spriteBatch, limbScreenPos, tformedJointPos, Color.White, 0f, 1f);
					}
					if (!this.editJoints || (altDown && joint.BodyA == limb.body.FarseerBody) || (!altDown && joint.BodyB == limb.body.FarseerBody))
					{
						goto IL_944;
					}
					Widget selectionWidget = this.GetJointSelectionWidget(joint.Params.Name + " selection widget ragdoll", joint, null);
					selectionWidget.DrawPos = tformedJointPos;
					selectionWidget.Draw(spriteBatch, deltaTime);
					if (!this.selectedJoints.Contains(joint))
					{
						goto IL_944;
					}
					if (joint.LimitEnabled && this.jointCreationMode == CharacterEditorScreen.JointCreationMode.None)
					{
						Limb otherBody = (limb == joint.LimbA) ? joint.LimbB : joint.LimbA;
						float rotation = -otherBody.Rotation + limb.Params.GetSpriteOrientation();
						if (this.character.AnimController.Dir < 0f)
						{
							rotation -= 3.1415927f;
						}
						this.DrawJointLimitWidgets(spriteBatch, limb, joint, tformedJointPos, true, true, true, rotation);
					}
					Limb referenceLimb = altDown ? joint.LimbB : joint.LimbA;
					Vector2 to = tformedJointPos - VectorExtensions.ForwardFlipped(referenceLimb.Rotation - referenceLimb.Params.GetSpriteOrientation(), 150f);
					GUI.DrawLine(spriteBatch, tformedJointPos, to, Color.LightGray * 0.7f, 0f, 2f);
					Vector2 dotSize = new Vector2(5f, 5f);
					Rectangle rect = new Rectangle((tformedJointPos - dotSize / 2f).ToPoint(), dotSize.ToPoint());
					string tooltip = joint.Params.Name + " " + jointPos.FormatZeroDecimal();
					GUI.DrawString(spriteBatch, tformedJointPos - new Vector2(1.2f, 0.5f) * GUIStyle.Font.MeasureString(tooltip, false), tooltip, Color.White, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
					if (!PlayerInput.PrimaryMouseButtonHeld())
					{
						this.isFrozen = this.freezeToggle.Selected;
						this.character.AnimController.Collider.PhysEnabled = true;
						goto IL_944;
					}
					if (!selectionWidget.IsControlled || this.jointCreationMode != CharacterEditorScreen.JointCreationMode.None)
					{
						goto IL_944;
					}
					if (this.autoFreeze)
					{
						this.isFrozen = true;
					}
					else
					{
						this.character.AnimController.Collider.PhysEnabled = false;
					}
					Vector2 input = ConvertUnits.ToSimUnits(this.scaledMouseSpeed) / this.Cam.Zoom;
					input.Y = -input.Y;
					input = input.TransformVector(VectorExtensions.ForwardFlipped(limb.Rotation, 1f));
					if (joint.BodyA != limb.body.FarseerBody)
					{
						goto IL_817;
					}
					joint.LocalAnchorA += input;
					Vector2 transformedValue = ConvertUnits.ToDisplayUnits(joint.LocalAnchorA / joint.Scale);
					this.TryUpdateJointParam(joint, "limb1anchor", transformedValue);
					if (this.copyJointSettings)
					{
						using (List<LimbJoint>.Enumerator enumerator = this.selectedJoints.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								LimbJoint i = enumerator.Current;
								i.LocalAnchorA = joint.LocalAnchorA;
								this.TryUpdateJointParam(i, "limb1anchor", transformedValue);
							}
							goto IL_8F4;
						}
						goto IL_817;
					}
					IL_8F4:
					if (this.limbPairEditing)
					{
						this.UpdateOtherJoints(limb, delegate(Limb otherLimb, LimbJoint otherJoint)
						{
							if (joint.BodyA == limb.body.FarseerBody && otherJoint.BodyA == otherLimb.body.FarseerBody)
							{
								otherJoint.LocalAnchorA = joint.LocalAnchorA;
								this.TryUpdateJointParam(otherJoint, "limb1anchor", ConvertUnits.ToDisplayUnits(joint.LocalAnchorA / joint.Scale));
								return;
							}
							if (joint.BodyB == limb.body.FarseerBody && otherJoint.BodyB == otherLimb.body.FarseerBody)
							{
								otherJoint.LocalAnchorB = joint.LocalAnchorB;
								this.TryUpdateJointParam(otherJoint, "limb2anchor", ConvertUnits.ToDisplayUnits(joint.LocalAnchorB / joint.Scale));
							}
						});
						goto IL_944;
					}
					goto IL_944;
					IL_817:
					if (joint.BodyB != limb.body.FarseerBody)
					{
						goto IL_8F4;
					}
					joint.LocalAnchorB += input;
					Vector2 transformedValue2 = ConvertUnits.ToDisplayUnits(joint.LocalAnchorB / joint.Scale);
					this.TryUpdateJointParam(joint, "limb2anchor", transformedValue2);
					if (this.copyJointSettings)
					{
						foreach (LimbJoint j in this.selectedJoints)
						{
							j.LocalAnchorB = joint.LocalAnchorB;
							this.TryUpdateJointParam(j, "limb2anchor", transformedValue2);
						}
						goto IL_8F4;
					}
					goto IL_8F4;
				}
			}
		}

		// Token: 0x06004981 RID: 18817 RVA: 0x0028A210 File Offset: 0x00288410
		private void UpdateOtherLimbs(Limb limb, Action<Limb> updateAction)
		{
			CharacterEditorScreen.<>c__DisplayClass221_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass221_0();
			CS$<>8__locals1.updateAction = updateAction;
			if (this.limbPairEditing)
			{
				string limbType = limb.type.ToString();
				bool isLeft = limbType.Contains("Left");
				bool isRight = limbType.Contains("Right");
				if (isLeft || isRight)
				{
					if (this.character.AnimController.HasMultipleLimbsOfSameType)
					{
						IEnumerable<Limb> otherLimbs = this.GetOtherLimbs(limb);
						if (otherLimbs == null)
						{
							return;
						}
						otherLimbs.ForEach(delegate(Limb l)
						{
							base.<UpdateOtherLimbs>g__UpdateOtherLimbs|1(l);
						});
						return;
					}
					else
					{
						Limb otherLimb = this.GetOtherLimb(limbType, isLeft);
						if (otherLimb != null)
						{
							CS$<>8__locals1.<UpdateOtherLimbs>g__UpdateOtherLimbs|1(otherLimb);
						}
					}
				}
			}
		}

		// Token: 0x06004982 RID: 18818 RVA: 0x0028A2AC File Offset: 0x002884AC
		private void UpdateOtherJoints(Limb limb, Action<Limb, LimbJoint> updateAction)
		{
			CharacterEditorScreen.<>c__DisplayClass222_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass222_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.updateAction = updateAction;
			if (this.limbPairEditing)
			{
				string limbType = limb.type.ToString();
				bool isLeft = limbType.Contains("Left");
				bool isRight = limbType.Contains("Right");
				if (isLeft || isRight)
				{
					if (this.character.AnimController.HasMultipleLimbsOfSameType)
					{
						IEnumerable<Limb> otherLimbs = this.GetOtherLimbs(limb);
						if (otherLimbs == null)
						{
							return;
						}
						otherLimbs.ForEach(delegate(Limb l)
						{
							base.<UpdateOtherJoints>g__UpdateOtherJoints|1(l);
						});
						return;
					}
					else
					{
						Limb otherLimb = this.GetOtherLimb(limbType, isLeft);
						if (otherLimb != null)
						{
							CS$<>8__locals1.<UpdateOtherJoints>g__UpdateOtherJoints|1(otherLimb);
						}
					}
				}
			}
		}

		// Token: 0x06004983 RID: 18819 RVA: 0x0028A350 File Offset: 0x00288550
		private Limb GetOtherLimb(string limbType, bool isLeft)
		{
			string otherLimbType = isLeft ? limbType.Replace("Left", "Right") : limbType.Replace("Right", "Left");
			LimbType type;
			if (Enum.TryParse<LimbType>(otherLimbType, out type))
			{
				return this.character.AnimController.GetLimb(type, true, false, false);
			}
			return null;
		}

		// Token: 0x06004984 RID: 18820 RVA: 0x0028A3A4 File Offset: 0x002885A4
		private IEnumerable<Limb> GetOtherLimbs(Limb limb)
		{
			IEnumerable<Limb> otherLimbs = from l in this.character.AnimController.Limbs
			where l.type == limb.type && l != limb
			select l;
			string limbType = limb.type.ToString();
			string otherLimbType = limbType.Contains("Left") ? limbType.Replace("Left", "Right") : limbType.Replace("Right", "Left");
			LimbType type;
			if (Enum.TryParse<LimbType>(otherLimbType, out type))
			{
				otherLimbs = otherLimbs.Union(from l in this.character.AnimController.Limbs
				where l.type == type
				select l);
			}
			return otherLimbs;
		}

		// Token: 0x170012AF RID: 4783
		// (get) Token: 0x06004985 RID: 18821 RVA: 0x0028A462 File Offset: 0x00288662
		private List<Texture2D> Textures
		{
			get
			{
				if (this.textures == null)
				{
					this.CreateTextures();
				}
				return this.textures;
			}
		}

		// Token: 0x06004986 RID: 18822 RVA: 0x0028A478 File Offset: 0x00288678
		private void CreateTextures()
		{
			this.textures = new List<Texture2D>();
			this.texturePaths = new List<string>();
			foreach (Limb limb in this.character.AnimController.Limbs)
			{
				if (limb.ActiveSprite != null && !this.texturePaths.Contains(limb.ActiveSprite.FilePath.Value) && limb.ActiveSprite.Texture != null)
				{
					this.textures.Add(limb.ActiveSprite.Texture);
					this.texturePaths.Add(limb.ActiveSprite.FilePath.Value);
				}
			}
		}

		// Token: 0x06004987 RID: 18823 RVA: 0x0028A524 File Offset: 0x00288724
		private void DrawSpritesheetEditor(SpriteBatch spriteBatch, float deltaTime)
		{
			int offsetX = 30;
			int offsetY = 20;
			for (int i = 0; i < this.Textures.Count; i++)
			{
				Texture2D texture = this.Textures[i];
				if (!this.hideBodySheet)
				{
					Texture2D texture2 = texture;
					Vector2 position = new Vector2((float)offsetX, (float)offsetY);
					Vector2 origin2 = Vector2.Zero;
					Rectangle? sourceRectangle = null;
					float scale = this.spriteSheetZoom;
					spriteBatch.Draw(texture2, position, sourceRectangle, Color.White, 0f, origin2, scale, SpriteEffects.None, 0f);
				}
				GUI.DrawRectangle(spriteBatch, new Vector2((float)offsetX, (float)offsetY), texture.Bounds.Size.ToVector2() * this.spriteSheetZoom, Color.White, false, 0f, 1f);
				Limb[] limbs = this.character.AnimController.Limbs;
				for (int j = 0; j < limbs.Length; j++)
				{
					CharacterEditorScreen.<>c__DisplayClass230_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass230_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.limb = limbs[j];
					if (CS$<>8__locals1.limb.ActiveSprite != null && !(CS$<>8__locals1.limb.ActiveSprite.FilePath != this.texturePaths[i]))
					{
						Rectangle rect = CS$<>8__locals1.limb.ActiveSprite.SourceRect;
						rect.Size = rect.MultiplySize(this.spriteSheetZoom);
						rect.Location = rect.Location.Multiply(this.spriteSheetZoom);
						rect.X += offsetX;
						rect.Y += offsetY;
						Vector2 origin = CS$<>8__locals1.limb.ActiveSprite.Origin;
						Vector2 limbScreenPos = new Vector2((float)rect.X + origin.X * this.spriteSheetZoom, (float)rect.Y + origin.Y * this.spriteSheetZoom);
						foreach (WearableSprite wearable in CS$<>8__locals1.limb.WearingItems)
						{
							Vector2 orig = CS$<>8__locals1.limb.ActiveSprite.Origin;
							if (!wearable.InheritOrigin)
							{
								orig = wearable.Sprite.Origin;
								if (CS$<>8__locals1.limb.body.Dir == -1f)
								{
									orig.X = (float)wearable.Sprite.SourceRect.Width - orig.X;
								}
							}
							Texture2D texture3 = wearable.Sprite.Texture;
							Vector2 position2 = limbScreenPos;
							Vector2 origin2 = orig;
							Rectangle? sourceRectangle2 = new Rectangle?(wearable.InheritSourceRect ? CS$<>8__locals1.limb.ActiveSprite.SourceRect : wearable.Sprite.SourceRect);
							float scale = (wearable.InheritScale ? 1f : (wearable.Scale / this.RagdollParams.TextureScale)) * this.spriteSheetZoom;
							spriteBatch.Draw(texture3, position2, sourceRectangle2, Color.White, 0f, origin2, scale, SpriteEffects.None, 0f);
						}
						if (this.character.AnimController.Dir < 0f)
						{
							limbScreenPos.X = (float)(rect.X + rect.Width) - (float)Math.Round((double)(origin.X * this.spriteSheetZoom));
						}
						if (this.editJoints)
						{
							this.DrawSpritesheetJointEditor(spriteBatch, deltaTime, CS$<>8__locals1.limb, limbScreenPos, 0f);
						}
						bool isMouseOn = rect.Contains(PlayerInput.MousePosition);
						if (this.editLimbs)
						{
							int widgetSize = 8;
							CS$<>8__locals1.halfSize = widgetSize / 2;
							Vector2 stringOffset = new Vector2(5f, 14f);
							Vector2 topLeft = rect.Location.ToVector2();
							Vector2 topRight = new Vector2(topLeft.X + (float)rect.Width, topLeft.Y);
							Vector2 bottomRight = new Vector2(topRight.X, topRight.Y + (float)rect.Height);
							bool isSelected = this.selectedLimbs.Contains(CS$<>8__locals1.limb);
							if (this.jointStartLimb != CS$<>8__locals1.limb && this.jointEndLimb != CS$<>8__locals1.limb && (isSelected || !this.onlyShowSourceRectForSelectedLimbs))
							{
								GUI.DrawRectangle(spriteBatch, rect, isSelected ? Color.Yellow : (isMouseOn ? Color.White : GUIStyle.Red), false, 0f, 1f);
							}
							if (isSelected)
							{
								CharacterEditorScreen.<>c__DisplayClass230_1 CS$<>8__locals2 = new CharacterEditorScreen.<>c__DisplayClass230_1();
								CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
								CS$<>8__locals2.sprite = CS$<>8__locals2.CS$<>8__locals1.limb.ActiveSprite;
								CharacterEditorScreen.<>c__DisplayClass230_1 CS$<>8__locals3 = CS$<>8__locals2;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
								defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals2.CS$<>8__locals1.limb.Params.ID);
								defaultInterpolatedStringHandler.AppendLiteral("_origin");
								CS$<>8__locals3.originWidget = this.GetLimbEditWidget(defaultInterpolatedStringHandler.ToStringAndClear(), CS$<>8__locals2.CS$<>8__locals1.limb, widgetSize, WidgetShape.Cross, delegate(Widget w)
								{
									w.Refresh = delegate()
									{
										Widget w = w;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(2, 2);
										defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("Origin"));
										defaultInterpolatedStringHandler4.AppendLiteral(": ");
										defaultInterpolatedStringHandler4.AppendFormatted(CS$<>8__locals2.sprite.RelativeOrigin.FormatDoubleDecimal());
										w.Tooltip = defaultInterpolatedStringHandler4.ToStringAndClear();
									};
									w.Refresh();
									w.MouseHeld += delegate(float dTime)
									{
										Vector2 spritePos = new Vector2(30f, (float)CS$<>8__locals2.CS$<>8__locals1.<>4__this.GetOffsetY(CS$<>8__locals2.CS$<>8__locals1.limb.ActiveSprite));
										w.DrawPos = PlayerInput.MousePosition.Clamp(spritePos + CS$<>8__locals2.<DrawSpritesheetEditor>g__GetTopLeft|0() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom, spritePos + CS$<>8__locals2.<DrawSpritesheetEditor>g__GetBottomRight|2() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom);
										CS$<>8__locals2.sprite.Origin = (w.DrawPos - spritePos - CS$<>8__locals2.sprite.SourceRect.Location.ToVector2() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom;
										if (CS$<>8__locals2.CS$<>8__locals1.limb.DamagedSprite != null)
										{
											CS$<>8__locals2.CS$<>8__locals1.limb.DamagedSprite.RelativeOrigin = CS$<>8__locals2.sprite.RelativeOrigin;
										}
										CS$<>8__locals2.CS$<>8__locals1.<>4__this.TryUpdateLimbParam(CS$<>8__locals2.CS$<>8__locals1.limb, "origin", CS$<>8__locals2.sprite.RelativeOrigin);
										if (CS$<>8__locals2.CS$<>8__locals1.<>4__this.limbPairEditing)
										{
											CharacterEditorScreen <>4__this = CS$<>8__locals2.CS$<>8__locals1.<>4__this;
											Limb limb = CS$<>8__locals2.CS$<>8__locals1.limb;
											Action<Limb> updateAction;
											if ((updateAction = CS$<>8__locals2.<>9__8) == null)
											{
												updateAction = (CS$<>8__locals2.<>9__8 = delegate(Limb otherLimb)
												{
													otherLimb.ActiveSprite.RelativeOrigin = CS$<>8__locals2.sprite.RelativeOrigin;
													if (otherLimb.DamagedSprite != null)
													{
														otherLimb.DamagedSprite.RelativeOrigin = CS$<>8__locals2.sprite.RelativeOrigin;
													}
													CS$<>8__locals2.CS$<>8__locals1.<>4__this.TryUpdateLimbParam(otherLimb, "origin", CS$<>8__locals2.sprite.RelativeOrigin);
												});
											}
											<>4__this.UpdateOtherLimbs(limb, updateAction);
										}
									};
									w.PreUpdate += delegate(float dTime)
									{
										if (w.Enabled)
										{
											w.Enabled = !CS$<>8__locals2.CS$<>8__locals1.<>4__this.lockSpriteOrigin;
										}
									};
									w.PreDraw += delegate(SpriteBatch sb, float dTime)
									{
										Vector2 spritePos = new Vector2(30f, (float)CS$<>8__locals2.CS$<>8__locals1.<>4__this.GetOffsetY(CS$<>8__locals2.CS$<>8__locals1.limb.ActiveSprite));
										w.DrawPos = (spritePos + (CS$<>8__locals2.sprite.Origin + CS$<>8__locals2.sprite.SourceRect.Location.ToVector2()) * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom).Clamp(spritePos + CS$<>8__locals2.<DrawSpritesheetEditor>g__GetTopLeft|0() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom, spritePos + CS$<>8__locals2.<DrawSpritesheetEditor>g__GetBottomRight|2() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom);
										w.Refresh();
									};
								});
								CS$<>8__locals2.originWidget.Draw(spriteBatch, deltaTime);
								if (!this.lockSpritePosition && (CS$<>8__locals2.CS$<>8__locals1.limb.type != LimbType.Head || !this.character.IsHuman))
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(9, 1);
									defaultInterpolatedStringHandler2.AppendFormatted<int>(CS$<>8__locals2.CS$<>8__locals1.limb.Params.ID);
									defaultInterpolatedStringHandler2.AppendLiteral("_position");
									Widget positionWidget = this.GetLimbEditWidget(defaultInterpolatedStringHandler2.ToStringAndClear(), CS$<>8__locals2.CS$<>8__locals1.limb, widgetSize, WidgetShape.Rectangle, delegate(Widget w)
									{
										w.Refresh = delegate()
										{
											Widget w = w;
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(2, 2);
											defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("Position"));
											defaultInterpolatedStringHandler4.AppendLiteral(": ");
											defaultInterpolatedStringHandler4.AppendFormatted<Point>(CS$<>8__locals2.CS$<>8__locals1.limb.ActiveSprite.SourceRect.Location);
											w.Tooltip = defaultInterpolatedStringHandler4.ToStringAndClear();
										};
										w.Refresh();
										w.MouseHeld += delegate(float dTime)
										{
											w.DrawPos = PlayerInput.MousePosition;
											Sprite activeSprite = CS$<>8__locals2.CS$<>8__locals1.limb.ActiveSprite;
											Rectangle newRect = activeSprite.SourceRect;
											newRect.Location = new Point((int)((PlayerInput.MousePosition.X + (float)CS$<>8__locals2.CS$<>8__locals1.halfSize - 30f) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom), (int)((PlayerInput.MousePosition.Y + (float)CS$<>8__locals2.CS$<>8__locals1.halfSize - (float)CS$<>8__locals2.CS$<>8__locals1.<>4__this.GetOffsetY(activeSprite)) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom));
											activeSprite.SourceRect = newRect;
											if (CS$<>8__locals2.CS$<>8__locals1.limb.DamagedSprite != null)
											{
												CS$<>8__locals2.CS$<>8__locals1.limb.DamagedSprite.SourceRect = activeSprite.SourceRect;
											}
											CS$<>8__locals2.CS$<>8__locals1.<>4__this.TryUpdateLimbParam(CS$<>8__locals2.CS$<>8__locals1.limb, "sourcerect", newRect);
											Vector2 spritePos = new Vector2(30f, (float)CS$<>8__locals2.CS$<>8__locals1.<>4__this.GetOffsetY(activeSprite));
											Vector2 newOrigin = (CS$<>8__locals2.originWidget.DrawPos - spritePos - activeSprite.SourceRect.Location.ToVector2() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom;
											CS$<>8__locals2.CS$<>8__locals1.<>4__this.RecalculateOrigin(CS$<>8__locals2.CS$<>8__locals1.limb, new Vector2?(newOrigin));
											if (CS$<>8__locals2.CS$<>8__locals1.<>4__this.limbPairEditing)
											{
												CS$<>8__locals2.CS$<>8__locals1.<>4__this.UpdateOtherLimbs(CS$<>8__locals2.CS$<>8__locals1.limb, delegate(Limb otherLimb)
												{
													otherLimb.ActiveSprite.SourceRect = newRect;
													if (otherLimb.DamagedSprite != null)
													{
														otherLimb.DamagedSprite.SourceRect = newRect;
													}
													CS$<>8__locals2.CS$<>8__locals1.<>4__this.TryUpdateLimbParam(otherLimb, "sourcerect", newRect);
													CS$<>8__locals2.CS$<>8__locals1.<>4__this.RecalculateOrigin(otherLimb, new Vector2?(newOrigin));
												});
											}
										};
										w.PreDraw += delegate(SpriteBatch sb, float dTime)
										{
											w.Refresh();
										};
									});
									if (!positionWidget.IsControlled)
									{
										positionWidget.DrawPos = topLeft - new Vector2((float)CS$<>8__locals2.CS$<>8__locals1.halfSize);
									}
									positionWidget.Draw(spriteBatch, deltaTime);
								}
								if (!this.lockSpriteSize && (CS$<>8__locals2.CS$<>8__locals1.limb.type != LimbType.Head || !this.character.IsHuman))
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(5, 1);
									defaultInterpolatedStringHandler3.AppendFormatted<int>(CS$<>8__locals2.CS$<>8__locals1.limb.Params.ID);
									defaultInterpolatedStringHandler3.AppendLiteral("_size");
									Widget sizeWidget = this.GetLimbEditWidget(defaultInterpolatedStringHandler3.ToStringAndClear(), CS$<>8__locals2.CS$<>8__locals1.limb, widgetSize, WidgetShape.Rectangle, delegate(Widget w)
									{
										w.Refresh = delegate()
										{
											Widget w = w;
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(2, 2);
											defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("Size"));
											defaultInterpolatedStringHandler4.AppendLiteral(": ");
											defaultInterpolatedStringHandler4.AppendFormatted<Point>(CS$<>8__locals2.CS$<>8__locals1.limb.ActiveSprite.SourceRect.Size);
											w.Tooltip = defaultInterpolatedStringHandler4.ToStringAndClear();
										};
										w.Refresh();
										w.MouseHeld += delegate(float dTime)
										{
											w.DrawPos = PlayerInput.MousePosition;
											Sprite activeSprite = CS$<>8__locals2.CS$<>8__locals1.limb.ActiveSprite;
											Rectangle newRect = activeSprite.SourceRect;
											float offset_y = (float)activeSprite.SourceRect.Y * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom + (float)CS$<>8__locals2.CS$<>8__locals1.<>4__this.GetOffsetY(activeSprite);
											float offset_x = (float)activeSprite.SourceRect.X * CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom + 30f;
											int width = (int)((PlayerInput.MousePosition.X - (float)CS$<>8__locals2.CS$<>8__locals1.halfSize - offset_x) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom);
											int height = (int)((PlayerInput.MousePosition.Y - (float)CS$<>8__locals2.CS$<>8__locals1.halfSize - offset_y) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteSheetZoom);
											newRect.Size = new Point(width, height);
											activeSprite.SourceRect = newRect;
											activeSprite.size = new Vector2((float)width, (float)height);
											Vector2 colliderSize = new Vector2(ConvertUnits.ToSimUnits(width), ConvertUnits.ToSimUnits(height));
											if (CS$<>8__locals2.CS$<>8__locals1.<>4__this.recalculateCollider)
											{
												CS$<>8__locals2.CS$<>8__locals1.<>4__this.RecalculateCollider(CS$<>8__locals2.CS$<>8__locals1.limb, colliderSize);
											}
											CS$<>8__locals2.CS$<>8__locals1.<>4__this.RecalculateOrigin(CS$<>8__locals2.CS$<>8__locals1.limb, null);
											if (CS$<>8__locals2.CS$<>8__locals1.limb.DamagedSprite != null)
											{
												CS$<>8__locals2.CS$<>8__locals1.limb.DamagedSprite.SourceRect = activeSprite.SourceRect;
											}
											CS$<>8__locals2.CS$<>8__locals1.<>4__this.TryUpdateLimbParam(CS$<>8__locals2.CS$<>8__locals1.limb, "sourcerect", newRect);
											if (CS$<>8__locals2.CS$<>8__locals1.<>4__this.limbPairEditing)
											{
												CS$<>8__locals2.CS$<>8__locals1.<>4__this.UpdateOtherLimbs(CS$<>8__locals2.CS$<>8__locals1.limb, delegate(Limb otherLimb)
												{
													otherLimb.ActiveSprite.SourceRect = newRect;
													CS$<>8__locals2.CS$<>8__locals1.<>4__this.RecalculateOrigin(otherLimb, null);
													if (CS$<>8__locals2.CS$<>8__locals1.<>4__this.recalculateCollider)
													{
														CS$<>8__locals2.CS$<>8__locals1.<>4__this.RecalculateCollider(otherLimb, colliderSize);
													}
													if (otherLimb.DamagedSprite != null)
													{
														otherLimb.DamagedSprite.SourceRect = newRect;
													}
													CS$<>8__locals2.CS$<>8__locals1.<>4__this.TryUpdateLimbParam(otherLimb, "sourcerect", newRect);
												});
											}
										};
										w.PreDraw += delegate(SpriteBatch sb, float dTime)
										{
											w.Refresh();
										};
									});
									if (!sizeWidget.IsControlled)
									{
										sizeWidget.DrawPos = bottomRight + new Vector2((float)CS$<>8__locals2.CS$<>8__locals1.halfSize);
									}
									sizeWidget.Draw(spriteBatch, deltaTime);
								}
							}
							else if (isMouseOn && GUI.MouseOn == null && Widget.SelectedWidgets.None(null))
							{
								GUI.DrawString(spriteBatch, limbScreenPos + new Vector2(10f, -10f), CS$<>8__locals1.limb.Name, Color.White, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
							}
						}
						else
						{
							GUI.DrawRectangle(spriteBatch, rect, isMouseOn ? Color.White : Color.Gray, false, 0f, 1f);
							if (isMouseOn && GUI.MouseOn == null && Widget.SelectedWidgets.None(null))
							{
								GUI.DrawString(spriteBatch, limbScreenPos + new Vector2(10f, -10f), CS$<>8__locals1.limb.Name, Color.White, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
							}
						}
					}
				}
				offsetY += (int)((float)texture.Height * this.spriteSheetZoom);
			}
		}

		// Token: 0x06004988 RID: 18824 RVA: 0x0028ACC0 File Offset: 0x00288EC0
		private int GetTextureHeight(Sprite sprite)
		{
			int textureIndex = this.Textures.IndexOf(sprite.Texture);
			int height = 0;
			foreach (Texture2D t in this.Textures)
			{
				if (this.Textures.IndexOf(t) < textureIndex)
				{
					height += t.Height;
				}
			}
			return (int)((float)height * this.spriteSheetZoom);
		}

		// Token: 0x06004989 RID: 18825 RVA: 0x0028AD44 File Offset: 0x00288F44
		private int GetOffsetY(Sprite sprite)
		{
			return 20 + this.GetTextureHeight(sprite);
		}

		// Token: 0x0600498A RID: 18826 RVA: 0x0028AD50 File Offset: 0x00288F50
		private void RecalculateCollider(Limb l, Vector2 size)
		{
			float multiplier = 0.9f;
			l.body.SetSize(new Vector2(size.X, size.Y) * l.Scale * this.RagdollParams.TextureScale * multiplier);
			this.TryUpdateLimbParam(l, "radius", ConvertUnits.ToDisplayUnits(l.body.Radius / l.Params.Scale / this.RagdollParams.LimbScale / this.RagdollParams.TextureScale));
			this.TryUpdateLimbParam(l, "width", ConvertUnits.ToDisplayUnits(l.body.Width / l.Params.Scale / this.RagdollParams.LimbScale / this.RagdollParams.TextureScale));
			this.TryUpdateLimbParam(l, "height", ConvertUnits.ToDisplayUnits(l.body.Height / l.Params.Scale / this.RagdollParams.LimbScale / this.RagdollParams.TextureScale));
		}

		// Token: 0x0600498B RID: 18827 RVA: 0x0028AE70 File Offset: 0x00289070
		private void RecalculateOrigin(Limb l, Vector2? newOrigin = null)
		{
			Sprite activeSprite = l.ActiveSprite;
			if (this.lockSpriteOrigin)
			{
				activeSprite.Origin = (newOrigin ?? activeSprite.Origin);
				this.TryUpdateLimbParam(l, "origin", activeSprite.RelativeOrigin);
				return;
			}
			activeSprite.RelativeOrigin = activeSprite.RelativeOrigin;
		}

		// Token: 0x0600498C RID: 18828 RVA: 0x0028AED0 File Offset: 0x002890D0
		private void DrawSpritesheetJointEditor(SpriteBatch spriteBatch, float deltaTime, Limb limb, Vector2 limbScreenPos, float spriteRotation = 0f)
		{
			LimbJoint[] limbJoints = this.character.AnimController.LimbJoints;
			int k = 0;
			while (k < limbJoints.Length)
			{
				LimbJoint joint = limbJoints[k];
				Vector2 jointPos = Vector2.Zero;
				Vector2 anchorPosA = ConvertUnits.ToDisplayUnits(joint.LocalAnchorA);
				Vector2 anchorPosB = ConvertUnits.ToDisplayUnits(joint.LocalAnchorB);
				string anchorID;
				string otherID;
				if (joint.BodyA == limb.body.FarseerBody)
				{
					jointPos = anchorPosA;
					anchorID = "1";
					otherID = "2";
					goto IL_DA;
				}
				if (joint.BodyB == limb.body.FarseerBody)
				{
					jointPos = anchorPosB;
					anchorID = "2";
					otherID = "1";
					goto IL_DA;
				}
				IL_4E9:
				k++;
				continue;
				IL_DA:
				Vector2 tformedJointPos = jointPos / joint.Scale / limb.TextureScale * this.spriteSheetZoom;
				tformedJointPos.Y = -tformedJointPos.Y;
				tformedJointPos.X *= this.character.AnimController.Dir;
				tformedJointPos += limbScreenPos;
				Widget jointSelectionWidget = this.GetJointSelectionWidget(joint.Params.Name + " selection widget " + anchorID, joint, joint.Params.Name + " selection widget " + otherID);
				jointSelectionWidget.DrawPos = tformedJointPos;
				jointSelectionWidget.Draw(spriteBatch, deltaTime);
				Widget otherWidget = this.GetJointSelectionWidget(joint.Params.Name + " selection widget " + otherID, joint, joint.Params.Name + " selection widget " + anchorID);
				if (anchorID == "2")
				{
					bool isSelected = this.selectedJoints.Contains(joint);
					bool isHovered = jointSelectionWidget.IsSelected || otherWidget.IsSelected;
					if (isSelected || isHovered)
					{
						GUI.DrawLine(spriteBatch, jointSelectionWidget.DrawPos, otherWidget.DrawPos, jointSelectionWidget.Color, 0f, 2f);
					}
				}
				if (!this.selectedJoints.Contains(joint))
				{
					goto IL_4E9;
				}
				if (joint.LimitEnabled && this.jointCreationMode == CharacterEditorScreen.JointCreationMode.None)
				{
					this.DrawJointLimitWidgets(spriteBatch, limb, joint, tformedJointPos, false, true, false, joint.LimbB.Params.GetSpriteOrientation());
				}
				if (!jointSelectionWidget.IsControlled)
				{
					goto IL_4E9;
				}
				Vector2 input = ConvertUnits.ToSimUnits(this.scaledMouseSpeed);
				input.Y = -input.Y;
				input.X *= this.character.AnimController.Dir;
				input *= joint.Scale * limb.TextureScale / this.spriteSheetZoom;
				if (joint.BodyA != limb.body.FarseerBody)
				{
					goto IL_3EE;
				}
				joint.LocalAnchorA += input;
				Vector2 transformedValue = ConvertUnits.ToDisplayUnits(joint.LocalAnchorA / joint.Scale);
				this.TryUpdateJointParam(joint, "limb1anchor", transformedValue);
				if (this.copyJointSettings)
				{
					using (List<LimbJoint>.Enumerator enumerator = this.selectedJoints.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							LimbJoint i = enumerator.Current;
							i.LocalAnchorA = joint.LocalAnchorA;
							this.TryUpdateJointParam(i, "limb1anchor", transformedValue);
						}
						goto IL_4C4;
					}
					goto IL_3EE;
				}
				IL_4C4:
				if (this.limbPairEditing)
				{
					this.UpdateOtherJoints(limb, delegate(Limb otherLimb, LimbJoint otherJoint)
					{
						if (joint.BodyA == limb.body.FarseerBody && otherJoint.BodyA == otherLimb.body.FarseerBody)
						{
							otherJoint.LocalAnchorA = joint.LocalAnchorA;
							this.TryUpdateJointParam(otherJoint, "limb1anchor", ConvertUnits.ToDisplayUnits(joint.LocalAnchorA / joint.Scale));
							return;
						}
						if (joint.BodyB == limb.body.FarseerBody && otherJoint.BodyB == otherLimb.body.FarseerBody)
						{
							otherJoint.LocalAnchorB = joint.LocalAnchorB;
							this.TryUpdateJointParam(otherJoint, "limb2anchor", ConvertUnits.ToDisplayUnits(joint.LocalAnchorB / joint.Scale));
						}
					});
					goto IL_4E9;
				}
				goto IL_4E9;
				IL_3EE:
				if (joint.BodyB != limb.body.FarseerBody)
				{
					goto IL_4C4;
				}
				joint.LocalAnchorB += input;
				Vector2 transformedValue2 = ConvertUnits.ToDisplayUnits(joint.LocalAnchorB / joint.Scale);
				this.TryUpdateJointParam(joint, "limb2anchor", transformedValue2);
				if (this.copyJointSettings)
				{
					foreach (LimbJoint j in this.selectedJoints)
					{
						j.LocalAnchorB = joint.LocalAnchorB;
						this.TryUpdateJointParam(j, "limb2anchor", transformedValue2);
					}
					goto IL_4C4;
				}
				goto IL_4C4;
			}
		}

		// Token: 0x0600498D RID: 18829 RVA: 0x0028B3F0 File Offset: 0x002895F0
		private void DrawJointLimitWidgets(SpriteBatch spriteBatch, Limb limb, LimbJoint joint, Vector2 drawPos, bool autoFreeze, bool allowPairEditing, bool holdPosition, float rotationOffset = 0f)
		{
			bool clockWise = joint.Params.ClockWiseRotation;
			Color angleColor = (joint.UpperLimit - joint.LowerLimit > 0f) ? (GUIStyle.Green * 0.5f) : GUIStyle.Red;
			SpriteBatch spriteBatch2 = spriteBatch;
			Vector2 drawPos2 = drawPos;
			float value = MathHelper.ToDegrees(joint.UpperLimit);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
			defaultInterpolatedStringHandler.AppendFormatted(joint.Params.Name);
			defaultInterpolatedStringHandler.AppendLiteral(": ");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("UpperLimit"));
			this.DrawRadialWidget(spriteBatch2, drawPos2, value, defaultInterpolatedStringHandler.ToStringAndClear(), Color.Cyan, delegate(float angle)
			{
				joint.UpperLimit = MathHelper.ToRadians(angle);
				this.ValidateJoint(joint);
				angle = MathHelper.ToDegrees(joint.UpperLimit);
				this.TryUpdateJointParam(joint, "upperlimit", angle);
				if (this.copyJointSettings)
				{
					foreach (LimbJoint i in this.selectedJoints)
					{
						if (i.LimitEnabled != joint.LimitEnabled)
						{
							i.LimitEnabled = joint.LimitEnabled;
							this.TryUpdateJointParam(i, "limitenabled", i.LimitEnabled);
						}
						i.UpperLimit = joint.UpperLimit;
						this.TryUpdateJointParam(i, "upperlimit", angle);
					}
				}
				if (allowPairEditing && this.limbPairEditing)
				{
					this.UpdateOtherJoints(limb, delegate(Limb otherLimb, LimbJoint otherJoint)
					{
						if (this.IsMatchingLimb(limb, otherLimb, joint, otherJoint))
						{
							if (otherJoint.LimitEnabled != joint.LimitEnabled)
							{
								otherJoint.LimitEnabled = otherJoint.LimitEnabled;
								this.TryUpdateJointParam(otherJoint, "limitenabled", otherJoint.LimitEnabled);
							}
							otherJoint.UpperLimit = joint.UpperLimit;
							this.TryUpdateJointParam(otherJoint, "upperlimit", angle);
						}
					});
				}
				base.<DrawJointLimitWidgets>g__DrawAngle|0(20f, angleColor, 4f);
				base.<DrawJointLimitWidgets>g__DrawAngle|0(40f, Color.Cyan, 5f);
				GUI.DrawString(spriteBatch, drawPos, angle.FormatZeroDecimal(), Color.Black, new Color?(Color.Cyan), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
			}, 40f, 10, rotationOffset, clockWise, false, null, false, holdPosition, 1);
			SpriteBatch spriteBatch3 = spriteBatch;
			Vector2 drawPos3 = drawPos;
			float value2 = MathHelper.ToDegrees(joint.LowerLimit);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 2);
			defaultInterpolatedStringHandler2.AppendFormatted(joint.Params.Name);
			defaultInterpolatedStringHandler2.AppendLiteral(": ");
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(CharacterEditorScreen.GetCharacterEditorTranslation("LowerLimit"));
			this.DrawRadialWidget(spriteBatch3, drawPos3, value2, defaultInterpolatedStringHandler2.ToStringAndClear(), Color.Yellow, delegate(float angle)
			{
				joint.LowerLimit = MathHelper.ToRadians(angle);
				this.ValidateJoint(joint);
				angle = MathHelper.ToDegrees(joint.LowerLimit);
				this.TryUpdateJointParam(joint, "lowerlimit", angle);
				if (this.copyJointSettings)
				{
					foreach (LimbJoint i in this.selectedJoints)
					{
						if (i.LimitEnabled != joint.LimitEnabled)
						{
							i.LimitEnabled = joint.LimitEnabled;
							this.TryUpdateJointParam(i, "limitenabled", i.LimitEnabled);
						}
						i.LowerLimit = joint.LowerLimit;
						this.TryUpdateJointParam(i, "lowerlimit", angle);
					}
				}
				if (allowPairEditing && this.limbPairEditing)
				{
					this.UpdateOtherJoints(limb, delegate(Limb otherLimb, LimbJoint otherJoint)
					{
						if (this.IsMatchingLimb(limb, otherLimb, joint, otherJoint))
						{
							if (otherJoint.LimitEnabled != joint.LimitEnabled)
							{
								otherJoint.LimitEnabled = otherJoint.LimitEnabled;
								this.TryUpdateJointParam(otherJoint, "limitenabled", otherJoint.LimitEnabled);
							}
							otherJoint.LowerLimit = joint.LowerLimit;
							this.TryUpdateJointParam(otherJoint, "lowerlimit", angle);
						}
					});
				}
				base.<DrawJointLimitWidgets>g__DrawAngle|0(20f, angleColor, 4f);
				base.<DrawJointLimitWidgets>g__DrawAngle|0(25f, Color.Yellow, 5f);
				GUI.DrawString(spriteBatch, drawPos, angle.FormatZeroDecimal(), Color.Black, new Color?(Color.Yellow), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
			}, 25f, 10, rotationOffset, clockWise, false, null, false, holdPosition, 1);
		}

		// Token: 0x0600498E RID: 18830 RVA: 0x0028B5CC File Offset: 0x002897CC
		private void Nudge(Keys key)
		{
			switch (key)
			{
			case Keys.Left:
				using (List<Limb>.Enumerator enumerator = this.selectedLimbs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Limb limb = enumerator.Current;
						if (limb.type != LimbType.Head || !this.character.IsHuman)
						{
							Rectangle newRect = limb.ActiveSprite.SourceRect;
							bool resize = PlayerInput.KeyDown(Keys.LeftControl);
							if (resize)
							{
								if (this.lockSpriteSize)
								{
									return;
								}
								newRect.Width--;
							}
							else
							{
								if (this.lockSpritePosition)
								{
									return;
								}
								newRect.X--;
							}
							this.UpdateSourceRect(limb, newRect, resize);
						}
					}
					goto IL_2BC;
				}
				break;
			case Keys.Up:
				goto IL_218;
			case Keys.Right:
				break;
			case Keys.Down:
				goto IL_16B;
			default:
				goto IL_2BC;
			}
			using (List<Limb>.Enumerator enumerator2 = this.selectedLimbs.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					Limb limb2 = enumerator2.Current;
					if (limb2.type != LimbType.Head || !this.character.IsHuman)
					{
						Rectangle newRect2 = limb2.ActiveSprite.SourceRect;
						bool resize2 = PlayerInput.KeyDown(Keys.LeftControl);
						if (resize2)
						{
							if (this.lockSpriteSize)
							{
								return;
							}
							newRect2.Width++;
						}
						else
						{
							if (this.lockSpritePosition)
							{
								return;
							}
							newRect2.X++;
						}
						this.UpdateSourceRect(limb2, newRect2, resize2);
					}
				}
				goto IL_2BC;
			}
			IL_16B:
			using (List<Limb>.Enumerator enumerator3 = this.selectedLimbs.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					Limb limb3 = enumerator3.Current;
					if (limb3.type != LimbType.Head || !this.character.IsHuman)
					{
						Rectangle newRect3 = limb3.ActiveSprite.SourceRect;
						bool resize3 = PlayerInput.KeyDown(Keys.LeftControl);
						if (resize3)
						{
							if (this.lockSpriteSize)
							{
								return;
							}
							newRect3.Height++;
						}
						else
						{
							if (this.lockSpritePosition)
							{
								return;
							}
							newRect3.Y++;
						}
						this.UpdateSourceRect(limb3, newRect3, resize3);
					}
				}
				goto IL_2BC;
			}
			IL_218:
			foreach (Limb limb4 in this.selectedLimbs)
			{
				if (limb4.type != LimbType.Head || !this.character.IsHuman)
				{
					Rectangle newRect4 = limb4.ActiveSprite.SourceRect;
					bool resize4 = PlayerInput.KeyDown(Keys.LeftControl);
					if (resize4)
					{
						if (this.lockSpriteSize)
						{
							return;
						}
						newRect4.Height--;
					}
					else
					{
						if (this.lockSpritePosition)
						{
							return;
						}
						newRect4.Y--;
					}
					this.UpdateSourceRect(limb4, newRect4, resize4);
				}
			}
			IL_2BC:
			this.RagdollParams.StoreSnapshot();
		}

		// Token: 0x0600498F RID: 18831 RVA: 0x0028B8D4 File Offset: 0x00289AD4
		private void SetSpritesheetRestriction(bool value)
		{
			this.unrestrictSpritesheet = value;
			this.CalculateSpritesheetZoom();
			this.spriteSheetZoomBar.BarScroll = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(this.spriteSheetMinZoom, this.spriteSheetMaxZoom, this.spriteSheetZoom));
		}

		// Token: 0x06004990 RID: 18832 RVA: 0x0028B914 File Offset: 0x00289B14
		private void DrawRadialWidget(SpriteBatch spriteBatch, Vector2 drawPos, float value, LocalizedString toolTip, Color color, Action<float> onClick, float circleRadius = 30f, int widgetSize = 10, float rotationOffset = 0f, bool clockWise = true, bool displayAngle = true, bool? autoFreeze = null, bool wrapAnglePi = false, bool holdPosition = false, int rounding = 1)
		{
			if (!MathUtils.IsValid(value))
			{
				value = 0f;
			}
			float drawAngle = clockWise ? value : (-value);
			Vector2 widgetDrawPos = drawPos + VectorExtensions.Forward(MathHelper.ToRadians(drawAngle) + rotationOffset - 1.5707964f, circleRadius);
			GUI.DrawLine(spriteBatch, drawPos, widgetDrawPos, color, 0f, 1f);
			this.DrawWidget(spriteBatch, widgetDrawPos, CharacterEditorScreen.WidgetType.Rectangle, widgetSize, color, toolTip, delegate
			{
				GUI.DrawLine(spriteBatch, drawPos, widgetDrawPos, color, 0f, 3f);
				spriteBatch.DrawCircle(drawPos, circleRadius, 40, color, 1f);
				Vector2 d = PlayerInput.MousePosition - drawPos;
				float newAngle = clockWise ? (MathUtils.VectorToAngle(d) + 1.5707964f - rotationOffset) : (-MathUtils.VectorToAngle(d) - 1.5707964f + rotationOffset);
				value = MathHelper.ToDegrees(wrapAnglePi ? MathUtils.WrapAnglePi(newAngle) : MathUtils.WrapAngleTwoPi(newAngle));
				value = (float)Math.Round((double)(value / (float)rounding)) * (float)rounding;
				if (value >= 360f || value <= -360f)
				{
					value = 0f;
				}
				if (displayAngle)
				{
					GUI.DrawString(spriteBatch, drawPos, value.FormatZeroDecimal(), Color.Black, new Color?(color), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				}
				onClick(value);
			}, autoFreeze, holdPosition, delegate
			{
				if (!PlayerInput.PrimaryMouseButtonHeld())
				{
					SpriteBatch spriteBatch2 = spriteBatch;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(toolTip);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted(value.FormatZeroDecimal());
					defaultInterpolatedStringHandler.AppendLiteral(")");
					GUIComponent.DrawToolTip(spriteBatch2, defaultInterpolatedStringHandler.ToStringAndClear(), new Vector2(drawPos.X + 50f, drawPos.Y - (float)(widgetSize / 2) - 50f), null, null);
				}
			});
		}

		// Token: 0x06004991 RID: 18833 RVA: 0x0028BA54 File Offset: 0x00289C54
		private void DrawWidget(SpriteBatch spriteBatch, Vector2 drawPos, CharacterEditorScreen.WidgetType widgetType, int size, Color color, LocalizedString toolTip, Action onPressed, bool? autoFreeze = null, bool holdPosition = false, Action onHovered = null)
		{
			Rectangle drawRect = new Rectangle((int)drawPos.X - size / 2, (int)drawPos.Y - size / 2, size, size);
			Rectangle inputRect = drawRect;
			inputRect.Inflate((float)size * 0.75f, (float)size * 0.75f);
			bool isMouseOn = inputRect.Contains(PlayerInput.MousePosition);
			bool isSelected = isMouseOn && GUI.MouseOn == null && Widget.SelectedWidgets.None(null);
			if (widgetType != CharacterEditorScreen.WidgetType.Rectangle)
			{
				if (widgetType != CharacterEditorScreen.WidgetType.Circle)
				{
					throw new NotImplementedException(widgetType.ToString());
				}
				if (isSelected)
				{
					spriteBatch.DrawCircle(drawPos, (float)size * 0.7f, 40, color, 3f);
				}
				else
				{
					spriteBatch.DrawCircle(drawPos, (float)size * 0.5f, 40, color, 1f);
				}
			}
			else if (isSelected)
			{
				Rectangle rect = drawRect;
				rect.Inflate((float)size * 0.3f, (float)size * 0.3f);
				GUI.DrawRectangle(spriteBatch, rect, color, PlayerInput.PrimaryMouseButtonHeld(), 0f, 3f);
			}
			else
			{
				GUI.DrawRectangle(spriteBatch, drawRect, color, false, 0f, 1f);
			}
			if (isSelected)
			{
				if (onHovered == null)
				{
					GUIComponent.DrawToolTip(spriteBatch, toolTip, new Vector2((float)(drawRect.Right + 5), (float)(drawRect.Y - drawRect.Height / 2)), null, null);
				}
				else
				{
					onHovered();
				}
				if (PlayerInput.PrimaryMouseButtonHeld())
				{
					if (autoFreeze ?? this.autoFreeze)
					{
						this.isFrozen = true;
					}
					if (holdPosition)
					{
						this.character.AnimController.Collider.PhysEnabled = false;
					}
					onPressed();
				}
				else
				{
					this.isFrozen = this.freezeToggle.Selected;
					this.character.AnimController.Collider.PhysEnabled = true;
				}
				if (PlayerInput.PrimaryMouseButtonClicked())
				{
					this.SaveSnapshot();
				}
			}
		}

		// Token: 0x06004992 RID: 18834 RVA: 0x0028BC44 File Offset: 0x00289E44
		private Widget GetAnimationWidget(string name, Color innerColor, Color? outerColor = null, int size = 10, float sizeMultiplier = 2f, WidgetShape shape = WidgetShape.Rectangle, Action<Widget> initMethod = null)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.character.SpeciesName);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(this.character.AnimController.CurrentAnimationParams.AnimationType.ToString());
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			string id = defaultInterpolatedStringHandler.ToStringAndClear();
			Widget widget;
			if (!this.animationWidgets.TryGetValue(id, out widget))
			{
				int selectedSize = (int)Math.Round((double)((float)size * sizeMultiplier));
				widget = new Widget(id, size, shape)
				{
					TooltipOffset = new Vector2?(new Vector2((float)(selectedSize / 2 + 5), -10f)),
					Data = this.character.AnimController.CurrentAnimationParams
				};
				widget.MouseUp += delegate()
				{
					this.CurrentAnimation.StoreSnapshot();
				};
				widget.Color = innerColor;
				widget.SecondaryColor = outerColor;
				widget.PreUpdate += delegate(float dTime)
				{
					widget.Enabled = this.editAnimations;
					if (widget.Enabled)
					{
						AnimationParams data = widget.Data as AnimationParams;
						widget.Enabled = (data.AnimationType == this.character.AnimController.CurrentAnimationParams.AnimationType);
					}
				};
				widget.PostUpdate += delegate(float dTime)
				{
					widget.InputAreaMargin = (widget.IsControlled ? 1000 : 0);
					widget.Size = (widget.IsSelected ? selectedSize : size);
					widget.IsFilled = widget.IsControlled;
				};
				widget.PreDraw += delegate(SpriteBatch sp, float dTime)
				{
					if (!widget.IsControlled)
					{
						widget.Refresh();
					}
				};
				this.animationWidgets.Add(id, widget);
				if (initMethod != null)
				{
					initMethod(widget);
				}
			}
			return widget;
		}

		// Token: 0x06004993 RID: 18835 RVA: 0x0028BE50 File Offset: 0x0028A050
		private Widget GetJointSelectionWidget(string id, LimbJoint joint, string linkedId = null)
		{
			CharacterEditorScreen.<>c__DisplayClass246_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass246_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.joint = joint;
			Widget jointWidget;
			if (!this.jointSelectionWidgets.TryGetValue(id, out jointWidget))
			{
				jointWidget = CS$<>8__locals1.<GetJointSelectionWidget>g__CreateJointSelectionWidget|0(id, CS$<>8__locals1.joint);
				if (linkedId != null)
				{
					Widget linkedWidget;
					if (!this.jointSelectionWidgets.TryGetValue(linkedId, out linkedWidget))
					{
						linkedWidget = CS$<>8__locals1.<GetJointSelectionWidget>g__CreateJointSelectionWidget|0(linkedId, CS$<>8__locals1.joint);
					}
					jointWidget.LinkedWidget = linkedWidget;
					linkedWidget.LinkedWidget = jointWidget;
				}
			}
			return jointWidget;
		}

		// Token: 0x06004994 RID: 18836 RVA: 0x0028BEC0 File Offset: 0x0028A0C0
		private Widget GetLimbEditWidget(string ID, Limb limb, int size = 5, WidgetShape shape = WidgetShape.Rectangle, Action<Widget> initMethod = null)
		{
			CharacterEditorScreen.<>c__DisplayClass247_0 CS$<>8__locals1 = new CharacterEditorScreen.<>c__DisplayClass247_0();
			CS$<>8__locals1.size = size;
			CS$<>8__locals1.ID = ID;
			CS$<>8__locals1.shape = shape;
			CS$<>8__locals1.limb = limb;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.initMethod = initMethod;
			Widget widget;
			if (!this.limbEditWidgets.TryGetValue(CS$<>8__locals1.ID, out widget))
			{
				widget = CS$<>8__locals1.<GetLimbEditWidget>g__CreateLimbEditWidget|0();
				this.limbEditWidgets.Add(CS$<>8__locals1.ID, widget);
			}
			return widget;
		}

		// Token: 0x060049A5 RID: 18853 RVA: 0x0028C2AC File Offset: 0x0028A4AC
		[CompilerGenerated]
		private void <CreateCharacterSelectionPanel>g__HandleSpawnException|176_2(Identifier characterIdentifier, Exception e)
		{
			if (characterIdentifier != CharacterPrefab.HumanSpeciesName)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to spawn the character \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(characterIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, null, false, false);
				this.SpawnCharacter(CharacterPrefab.HumanSpeciesName, null);
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(33, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Failed to spawn the character \"");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(characterIdentifier);
			defaultInterpolatedStringHandler2.AppendLiteral("\".");
			throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear(), e);
		}

		// Token: 0x060049AC RID: 18860 RVA: 0x0028C3AC File Offset: 0x0028A5AC
		[CompilerGenerated]
		private void <ResetParamsEditor>g__CreateCloseButton|189_4(SerializableEntityEditor editor, Action onButtonClicked, float size = 1f)
		{
			if (editor == null)
			{
				return;
			}
			int height = 30;
			GUIFrame parent = new GUIFrame(new RectTransform(new Point(editor.Rect.Width, (int)((float)height * size * GUI.yScale)), editor.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), null, null)
			{
				CanBeFocused = false
			};
			new GUIButton(new RectTransform(new Vector2(0.9f), parent.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUICancelButton", new Color?(GUIStyle.Red)).OnClicked = delegate(GUIButton button, object data)
			{
				onButtonClicked();
				this.ResetParamsEditor();
				return true;
			};
			editor.AddCustomContent(parent, 0);
		}

		// Token: 0x060049AD RID: 18861 RVA: 0x0028C488 File Offset: 0x0028A688
		[CompilerGenerated]
		private void <ResetParamsEditor>g__CreateAddButtonAtLast|189_5(ParamsEditor editor, Action onButtonClicked, LocalizedString text)
		{
			if (editor == null)
			{
				return;
			}
			GUIFrame parentFrame = new GUIFrame(new RectTransform(new Point(editor.EditorBox.Rect.Width, (int)(50f * GUI.yScale)), editor.EditorBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(ParamsEditor.Color))
			{
				CanBeFocused = false
			};
			new GUIButton(new RectTransform(new Vector2(0.45f, 0.6f), parentFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), text, Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object data)
			{
				onButtonClicked();
				this.ResetParamsEditor();
				return true;
			};
		}

		// Token: 0x060049AE RID: 18862 RVA: 0x0028C568 File Offset: 0x0028A768
		[CompilerGenerated]
		private void <ResetParamsEditor>g__CreateAddButton|189_6(SerializableEntityEditor editor, Action onButtonClicked, LocalizedString text)
		{
			if (editor == null)
			{
				return;
			}
			GUIFrame parent = new GUIFrame(new RectTransform(new Point(editor.Rect.Width, (int)(60f * GUI.yScale)), editor.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null)
			{
				CanBeFocused = false
			};
			new GUIButton(new RectTransform(new Vector2(0.45f, 0.4f), parent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), text, Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object data)
			{
				onButtonClicked();
				this.ResetParamsEditor();
				return true;
			};
			editor.AddCustomContent(parent, editor.ContentCount);
		}

		// Token: 0x040025EF RID: 9711
		private Camera cam;

		// Token: 0x040025F0 RID: 9712
		private Character character;

		// Token: 0x040025F1 RID: 9713
		private Vector2 spawnPosition;

		// Token: 0x040025F2 RID: 9714
		private bool editCharacterInfo;

		// Token: 0x040025F3 RID: 9715
		private bool editRagdoll;

		// Token: 0x040025F4 RID: 9716
		private bool editAnimations;

		// Token: 0x040025F5 RID: 9717
		private bool editLimbs;

		// Token: 0x040025F6 RID: 9718
		private bool editJoints;

		// Token: 0x040025F7 RID: 9719
		private bool editIK;

		// Token: 0x040025F8 RID: 9720
		private bool drawSkeleton;

		// Token: 0x040025F9 RID: 9721
		private bool drawDamageModifiers;

		// Token: 0x040025FA RID: 9722
		private bool showParamsEditor;

		// Token: 0x040025FB RID: 9723
		private bool showSpritesheet;

		// Token: 0x040025FC RID: 9724
		private bool isFrozen;

		// Token: 0x040025FD RID: 9725
		private bool autoFreeze;

		// Token: 0x040025FE RID: 9726
		private bool limbPairEditing;

		// Token: 0x040025FF RID: 9727
		private bool uniformScaling;

		// Token: 0x04002600 RID: 9728
		private bool lockSpriteOrigin;

		// Token: 0x04002601 RID: 9729
		private bool lockSpritePosition;

		// Token: 0x04002602 RID: 9730
		private bool lockSpriteSize;

		// Token: 0x04002603 RID: 9731
		private bool recalculateCollider;

		// Token: 0x04002604 RID: 9732
		private bool copyJointSettings;

		// Token: 0x04002605 RID: 9733
		private bool showColliders;

		// Token: 0x04002606 RID: 9734
		private bool displayWearables;

		// Token: 0x04002607 RID: 9735
		private bool displayBackgroundColor;

		// Token: 0x04002608 RID: 9736
		private bool onlyShowSourceRectForSelectedLimbs;

		// Token: 0x04002609 RID: 9737
		private bool unrestrictSpritesheet;

		// Token: 0x0400260A RID: 9738
		private CharacterEditorScreen.JointCreationMode jointCreationMode;

		// Token: 0x0400260B RID: 9739
		private bool isDrawingLimb;

		// Token: 0x0400260C RID: 9740
		private Rectangle newLimbRect;

		// Token: 0x0400260D RID: 9741
		private Limb jointStartLimb;

		// Token: 0x0400260E RID: 9742
		private Limb jointEndLimb;

		// Token: 0x0400260F RID: 9743
		private Vector2? anchor1Pos;

		// Token: 0x04002610 RID: 9744
		private const float holdTime = 0.2f;

		// Token: 0x04002611 RID: 9745
		private double holdTimer;

		// Token: 0x04002612 RID: 9746
		private float spriteSheetZoom = 1f;

		// Token: 0x04002613 RID: 9747
		private float spriteSheetMinZoom = 0.25f;

		// Token: 0x04002614 RID: 9748
		private float spriteSheetMaxZoom = 1f;

		// Token: 0x04002615 RID: 9749
		private const int spriteSheetOffsetY = 20;

		// Token: 0x04002616 RID: 9750
		private const int spriteSheetOffsetX = 30;

		// Token: 0x04002617 RID: 9751
		private bool hideBodySheet;

		// Token: 0x04002618 RID: 9752
		private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

		// Token: 0x04002619 RID: 9753
		private Vector2 cameraOffset;

		// Token: 0x0400261A RID: 9754
		private readonly List<LimbJoint> selectedJoints = new List<LimbJoint>();

		// Token: 0x0400261B RID: 9755
		private readonly List<Limb> selectedLimbs = new List<Limb>();

		// Token: 0x0400261C RID: 9756
		private readonly HashSet<Character> editedCharacters = new HashSet<Character>();

		// Token: 0x0400261D RID: 9757
		private bool isEndlessRunner;

		// Token: 0x0400261E RID: 9758
		private Rectangle spriteSheetRect;

		// Token: 0x0400261F RID: 9759
		private const string screenTextTag = "CharacterEditor.";

		// Token: 0x04002620 RID: 9760
		private Vector2 scaledMouseSpeed;

		// Token: 0x04002621 RID: 9761
		private int min;

		// Token: 0x04002622 RID: 9762
		private int max;

		// Token: 0x04002623 RID: 9763
		private readonly CharacterEditorScreen.WallGroup[] wallGroups = new CharacterEditorScreen.WallGroup[3];

		// Token: 0x04002624 RID: 9764
		private bool wallCollisionsEnabled;

		// Token: 0x04002625 RID: 9765
		private int characterIndex = -1;

		// Token: 0x04002626 RID: 9766
		private Identifier currentCharacterIdentifier;

		// Token: 0x04002627 RID: 9767
		private Identifier selectedJob = Identifier.Empty;

		// Token: 0x04002628 RID: 9768
		private List<Identifier> visibleSpecies;

		// Token: 0x04002629 RID: 9769
		private IEnumerable<CharacterFile> vanillaCharacters;

		// Token: 0x0400262A RID: 9770
		private static Vector2 innerScale = new Vector2(0.95f, 0.95f);

		// Token: 0x0400262B RID: 9771
		private GUILayoutGroup rightArea;

		// Token: 0x0400262C RID: 9772
		private GUILayoutGroup leftArea;

		// Token: 0x0400262D RID: 9773
		private GUIFrame centerArea;

		// Token: 0x0400262E RID: 9774
		private GUITextBlock totalMassText;

		// Token: 0x0400262F RID: 9775
		private GUIFrame characterSelectionPanel;

		// Token: 0x04002630 RID: 9776
		private GUIFrame fileEditPanel;

		// Token: 0x04002631 RID: 9777
		private GUIFrame modesPanel;

		// Token: 0x04002632 RID: 9778
		private GUIFrame buttonsPanel;

		// Token: 0x04002633 RID: 9779
		private GUIFrame optionsPanel;

		// Token: 0x04002634 RID: 9780
		private GUIFrame minorModesPanel;

		// Token: 0x04002635 RID: 9781
		private GUIFrame ragdollControls;

		// Token: 0x04002636 RID: 9782
		private GUIFrame jointControls;

		// Token: 0x04002637 RID: 9783
		private GUIFrame animationControls;

		// Token: 0x04002638 RID: 9784
		private GUIFrame limbControls;

		// Token: 0x04002639 RID: 9785
		private GUIFrame spriteSheetControls;

		// Token: 0x0400263A RID: 9786
		private GUIFrame backgroundColorPanel;

		// Token: 0x0400263B RID: 9787
		private GUIDropDown animSelection;

		// Token: 0x0400263C RID: 9788
		private GUITickBox freezeToggle;

		// Token: 0x0400263D RID: 9789
		private GUITickBox animTestPoseToggle;

		// Token: 0x0400263E RID: 9790
		private GUITickBox showCollidersToggle;

		// Token: 0x0400263F RID: 9791
		private GUIScrollBar jointScaleBar;

		// Token: 0x04002640 RID: 9792
		private GUIScrollBar limbScaleBar;

		// Token: 0x04002641 RID: 9793
		private GUIScrollBar spriteSheetZoomBar;

		// Token: 0x04002642 RID: 9794
		private GUITickBox copyJointsToggle;

		// Token: 0x04002643 RID: 9795
		private GUITickBox recalculateColliderToggle;

		// Token: 0x04002644 RID: 9796
		private GUIFrame resetSpriteOrientationButtonParent;

		// Token: 0x04002645 RID: 9797
		private GUITickBox characterInfoToggle;

		// Token: 0x04002646 RID: 9798
		private GUITickBox ragdollToggle;

		// Token: 0x04002647 RID: 9799
		private GUITickBox animsToggle;

		// Token: 0x04002648 RID: 9800
		private GUITickBox limbsToggle;

		// Token: 0x04002649 RID: 9801
		private GUITickBox paramsToggle;

		// Token: 0x0400264A RID: 9802
		private GUITickBox jointsToggle;

		// Token: 0x0400264B RID: 9803
		private GUITickBox spritesheetToggle;

		// Token: 0x0400264C RID: 9804
		private GUITickBox skeletonToggle;

		// Token: 0x0400264D RID: 9805
		private GUITickBox lightsToggle;

		// Token: 0x0400264E RID: 9806
		private GUITickBox damageModifiersToggle;

		// Token: 0x0400264F RID: 9807
		private GUITickBox ikToggle;

		// Token: 0x04002650 RID: 9808
		private GUITickBox lockSpriteOriginToggle;

		// Token: 0x04002651 RID: 9809
		private GUIFrame extraRagdollControls;

		// Token: 0x04002652 RID: 9810
		private GUIButton createJointButton;

		// Token: 0x04002653 RID: 9811
		private GUIButton createLimbButton;

		// Token: 0x04002654 RID: 9812
		private GUIButton deleteSelectedButton;

		// Token: 0x04002655 RID: 9813
		private GUIButton duplicateLimbButton;

		// Token: 0x04002656 RID: 9814
		private CharacterEditorScreen.ToggleButton modesToggle;

		// Token: 0x04002657 RID: 9815
		private CharacterEditorScreen.ToggleButton minorModesToggle;

		// Token: 0x04002658 RID: 9816
		private CharacterEditorScreen.ToggleButton buttonsPanelToggle;

		// Token: 0x04002659 RID: 9817
		private CharacterEditorScreen.ToggleButton optionsToggle;

		// Token: 0x0400265A RID: 9818
		private CharacterEditorScreen.ToggleButton characterPanelToggle;

		// Token: 0x0400265B RID: 9819
		private CharacterEditorScreen.ToggleButton fileEditToggle;

		// Token: 0x0400265C RID: 9820
		private Vector2[] corners = new Vector2[4];

		// Token: 0x0400265D RID: 9821
		private List<Texture2D> textures;

		// Token: 0x0400265E RID: 9822
		private List<string> texturePaths;

		// Token: 0x0400265F RID: 9823
		private Dictionary<string, Widget> animationWidgets = new Dictionary<string, Widget>();

		// Token: 0x04002660 RID: 9824
		private Dictionary<string, Widget> jointSelectionWidgets = new Dictionary<string, Widget>();

		// Token: 0x04002661 RID: 9825
		private Dictionary<string, Widget> limbEditWidgets = new Dictionary<string, Widget>();

		// Token: 0x02001151 RID: 4433
		private enum JointCreationMode
		{
			// Token: 0x04005B87 RID: 23431
			None,
			// Token: 0x04005B88 RID: 23432
			Select,
			// Token: 0x04005B89 RID: 23433
			Create
		}

		// Token: 0x02001152 RID: 4434
		private class WallGroup
		{
			// Token: 0x06008FC6 RID: 36806 RVA: 0x003BACA7 File Offset: 0x003B8EA7
			public WallGroup(List<MapEntity> entities)
			{
				this.Entities = entities;
			}

			// Token: 0x06008FC7 RID: 36807 RVA: 0x003BACB8 File Offset: 0x003B8EB8
			public CharacterEditorScreen.WallGroup Clone()
			{
				List<MapEntity> clones = new List<MapEntity>();
				this.Entities.ForEachMod(delegate(MapEntity w)
				{
					clones.Add(w.Clone());
				});
				return new CharacterEditorScreen.WallGroup(clones);
			}

			// Token: 0x04005B8A RID: 23434
			public readonly List<MapEntity> Entities;
		}

		// Token: 0x02001153 RID: 4435
		private enum Direction
		{
			// Token: 0x04005B8C RID: 23436
			Left,
			// Token: 0x04005B8D RID: 23437
			Right
		}

		// Token: 0x02001154 RID: 4436
		private class ToggleButton
		{
			// Token: 0x17001C9D RID: 7325
			// (get) Token: 0x06008FC8 RID: 36808 RVA: 0x003BACF8 File Offset: 0x003B8EF8
			// (set) Token: 0x06008FC9 RID: 36809 RVA: 0x003BAD00 File Offset: 0x003B8F00
			public float OpenState { get; private set; } = 1f;

			// Token: 0x17001C9E RID: 7326
			// (get) Token: 0x06008FCA RID: 36810 RVA: 0x003BAD09 File Offset: 0x003B8F09
			// (set) Token: 0x06008FCB RID: 36811 RVA: 0x003BAD11 File Offset: 0x003B8F11
			public bool IsHidden
			{
				get
				{
					return this.isHidden;
				}
				set
				{
					this.isHidden = value;
					this.RefreshToggleButtonState();
				}
			}

			// Token: 0x06008FCC RID: 36812 RVA: 0x003BAD20 File Offset: 0x003B8F20
			public ToggleButton(RectTransform rectT, CharacterEditorScreen.Direction dir)
			{
				this.toggleButton = new GUIButton(rectT, Alignment.Center, "UIToggleButton", null)
				{
					OnClicked = delegate(GUIButton button, object data)
					{
						this.IsHidden = !this.IsHidden;
						return true;
					}
				};
				this.dir = dir;
				this.RefreshToggleButtonState();
			}

			// Token: 0x06008FCD RID: 36813 RVA: 0x003BAD7C File Offset: 0x003B8F7C
			public void RefreshToggleButtonState()
			{
				foreach (GUIComponent child in this.toggleButton.Children)
				{
					CharacterEditorScreen.Direction direction = this.dir;
					if (direction != CharacterEditorScreen.Direction.Left)
					{
						if (direction == CharacterEditorScreen.Direction.Right)
						{
							child.SpriteEffects = (this.isHidden ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
						}
					}
					else
					{
						child.SpriteEffects = (this.isHidden ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
					}
				}
			}

			// Token: 0x06008FCE RID: 36814 RVA: 0x003BADFC File Offset: 0x003B8FFC
			public void UpdateOpenState(float deltaTime, Vector2 hiddenPos, RectTransform panel)
			{
				panel.AbsoluteOffset = new Vector2(MathHelper.SmoothStep(hiddenPos.X, 0f, this.OpenState), (float)panel.AbsoluteOffset.Y).ToPoint();
				this.OpenState = (this.isHidden ? Math.Max(this.OpenState - deltaTime * 5f, 0f) : Math.Min(this.OpenState + deltaTime * 5f, 1f));
			}

			// Token: 0x04005B8E RID: 23438
			public readonly CharacterEditorScreen.Direction dir;

			// Token: 0x04005B8F RID: 23439
			public readonly GUIButton toggleButton;

			// Token: 0x04005B91 RID: 23441
			private bool isHidden;
		}

		// Token: 0x02001155 RID: 4437
		private enum WidgetType
		{
			// Token: 0x04005B93 RID: 23443
			Rectangle,
			// Token: 0x04005B94 RID: 23444
			Circle
		}
	}
}
