using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000027 RID: 39
	internal class Character : Entity, IDamageable, ISerializableEntity, IClientSerializable, INetSerializable, IServerPositionSync, IServerSerializable
	{
		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000438 RID: 1080 RVA: 0x00024DED File Offset: 0x00022FED
		// (set) Token: 0x06000439 RID: 1081 RVA: 0x00024DF5 File Offset: 0x00022FF5
		public bool IsVisible { get; private set; } = true;

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x0600043A RID: 1082 RVA: 0x00024DFE File Offset: 0x00022FFE
		// (set) Token: 0x0600043B RID: 1083 RVA: 0x00024E06 File Offset: 0x00023006
		public bool ShowInteractionLabels { get; private set; }

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x0600043C RID: 1084 RVA: 0x00024E0F File Offset: 0x0002300F
		// (set) Token: 0x0600043D RID: 1085 RVA: 0x00024E18 File Offset: 0x00023018
		public static Character Controlled
		{
			get
			{
				return Character.controlled;
			}
			set
			{
				if (Character.controlled == value && Character.controlled == null)
				{
					return;
				}
				if (Character.controlled != value)
				{
					CharacterHealth.OpenHealthWindow = null;
					if (Character.controlled != null && value == null)
					{
						Screen selected = Screen.Selected;
						Camera camera = (selected != null) ? selected.Cam : null;
						if (camera != null)
						{
							camera.TargetPos = Vector2.Zero;
						}
						LightManager.ViewTarget = null;
					}
				}
				Character.controlled = value;
				if (Character.controlled != null)
				{
					Character.controlled.Enabled = true;
					Character.controlled.AnimController.Frozen = false;
				}
			}
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x0600043E RID: 1086 RVA: 0x00024E9B File Offset: 0x0002309B
		public Dictionary<object, HUDProgressBar> HUDProgressBars
		{
			get
			{
				return this.hudProgressBars;
			}
		}

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x0600043F RID: 1087 RVA: 0x00024EA3 File Offset: 0x000230A3
		// (set) Token: 0x06000440 RID: 1088 RVA: 0x00024EAB File Offset: 0x000230AB
		public float BlurStrength
		{
			get
			{
				return this.blurStrength;
			}
			set
			{
				this.blurStrength = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x06000441 RID: 1089 RVA: 0x00024EC3 File Offset: 0x000230C3
		// (set) Token: 0x06000442 RID: 1090 RVA: 0x00024ECB File Offset: 0x000230CB
		public float DistortStrength
		{
			get
			{
				return this.distortStrength;
			}
			set
			{
				this.distortStrength = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000443 RID: 1091 RVA: 0x00024EE3 File Offset: 0x000230E3
		// (set) Token: 0x06000444 RID: 1092 RVA: 0x00024EEB File Offset: 0x000230EB
		public float RadialDistortStrength
		{
			get
			{
				return this.radialDistortStrength;
			}
			set
			{
				this.radialDistortStrength = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000445 RID: 1093 RVA: 0x00024F03 File Offset: 0x00023103
		// (set) Token: 0x06000446 RID: 1094 RVA: 0x00024F0B File Offset: 0x0002310B
		public float ChromaticAberrationStrength
		{
			get
			{
				return this.chromaticAberrationStrength;
			}
			set
			{
				this.chromaticAberrationStrength = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000447 RID: 1095 RVA: 0x00024F23 File Offset: 0x00023123
		// (set) Token: 0x06000448 RID: 1096 RVA: 0x00024F2B File Offset: 0x0002312B
		public Color GrainColor { get; set; }

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000449 RID: 1097 RVA: 0x00024F34 File Offset: 0x00023134
		// (set) Token: 0x0600044A RID: 1098 RVA: 0x00024F3C File Offset: 0x0002313C
		public float GrainStrength
		{
			get
			{
				return this.grainStrength;
			}
			set
			{
				this.grainStrength = Math.Max(0f, value);
			}
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x0600044B RID: 1099 RVA: 0x00024F50 File Offset: 0x00023150
		// (set) Token: 0x0600044C RID: 1100 RVA: 0x00024F98 File Offset: 0x00023198
		public float CollapseEffectStrength
		{
			get
			{
				Level loaded = Level.Loaded;
				float? num;
				if (loaded == null)
				{
					num = null;
				}
				else
				{
					LevelRenderer renderer = loaded.Renderer;
					num = ((renderer != null) ? new float?(renderer.CollapseEffectStrength) : null);
				}
				float? num2 = num;
				return num2.GetValueOrDefault();
			}
			set
			{
				Level loaded = Level.Loaded;
				if (((loaded != null) ? loaded.Renderer : null) == null)
				{
					return;
				}
				if (Character.Controlled == this)
				{
					float strength = MathHelper.Clamp(value, 0f, 1f);
					Level.Loaded.Renderer.CollapseEffectStrength = strength;
					LevelRenderer renderer = Level.Loaded.Renderer;
					Submarine submarine = base.Submarine;
					renderer.CollapseEffectOrigin = ((submarine != null) ? submarine.WorldPosition : this.WorldPosition);
					Screen.Selected.Cam.Shake = Math.Max(MathF.Pow(strength, 3f) * 100f, Screen.Selected.Cam.Shake);
					Screen.Selected.Cam.Rotation = strength * (PerlinNoise.GetPerlin((float)Timing.TotalTime * 0.01f, (float)Timing.TotalTime * 0.05f) - 0.5f);
					Level.Loaded.Renderer.ChromaticAberrationStrength = value * 50f;
				}
			}
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x0600044D RID: 1101 RVA: 0x0002508C File Offset: 0x0002328C
		// (set) Token: 0x0600044E RID: 1102 RVA: 0x000250D3 File Offset: 0x000232D3
		public float CameraShake
		{
			get
			{
				Screen selected = Screen.Selected;
				float? num;
				if (selected == null)
				{
					num = null;
				}
				else
				{
					Camera cam = selected.Cam;
					num = ((cam != null) ? new float?(cam.Shake) : null);
				}
				float? num2 = num;
				return num2.GetValueOrDefault();
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				if (this != Character.Controlled)
				{
					return;
				}
				Screen selected = Screen.Selected;
				if (((selected != null) ? selected.Cam : null) != null)
				{
					Screen.Selected.Cam.Shake = value;
				}
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x0600044F RID: 1103 RVA: 0x0002510A File Offset: 0x0002330A
		public IEnumerable<ParticleEmitter> BloodEmitters
		{
			get
			{
				return this.bloodEmitters;
			}
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000450 RID: 1104 RVA: 0x00025112 File Offset: 0x00023312
		public IEnumerable<ParticleEmitter> DamageEmitters
		{
			get
			{
				return this.damageEmitters;
			}
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x06000451 RID: 1105 RVA: 0x0002511A File Offset: 0x0002331A
		public IEnumerable<ParticleEmitter> GibEmitters
		{
			get
			{
				return this.gibEmitters;
			}
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x06000452 RID: 1106 RVA: 0x00025122 File Offset: 0x00023322
		public static bool IsMouseOnUI
		{
			get
			{
				return GUI.MouseOn != null || (Barotrauma.Inventory.IsMouseOnInventory && !Barotrauma.Inventory.DraggingItemToWorld);
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x06000453 RID: 1107 RVA: 0x0002513E File Offset: 0x0002333E
		public IEnumerable<Character.ObjectiveEntity> ActiveObjectiveEntities
		{
			get
			{
				return this.activeObjectiveEntities;
			}
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x00025148 File Offset: 0x00023348
		public void ControlLocalPlayer(float deltaTime, Camera cam, bool moveCam = true)
		{
			if (Character.DisableControls || GUI.InputBlockingMenuOpen)
			{
				foreach (Key key in this.keys)
				{
					if (key != null)
					{
						key.Reset();
					}
				}
				if (GUI.InputBlockingMenuOpen || ConversationAction.IsDialogOpen)
				{
					this.cursorPosition = this.Position + PlayerInput.MouseSpeed.ClampLength(10f);
				}
			}
			else
			{
				this.wasFiring |= (this.keys[2].Held && this.keys[25].Held);
				for (int i = 0; i < this.keys.Length; i++)
				{
					this.keys[i].SetState();
				}
				if (Barotrauma.Inventory.IsMouseOnInventory && !this.keys[2].Held && CharacterHUD.ShouldDrawInventory(this))
				{
					this.<ControlLocalPlayer>g__ResetInputIfPrimaryMouse|81_1(InputType.Use);
					this.<ControlLocalPlayer>g__ResetInputIfPrimaryMouse|81_1(InputType.Shoot);
					this.<ControlLocalPlayer>g__ResetInputIfPrimaryMouse|81_1(InputType.Select);
				}
				this.ShowInteractionLabels = this.keys[36].Held;
				if (this.ShowInteractionLabels)
				{
					this.focusedItem = InteractionLabelManager.HoveredItem;
				}
				if (this.wasFiring && !this.keys[25].Held)
				{
					if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Shoot] == GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Select])
					{
						this.keys[0].Reset();
					}
					if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Shoot] == GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Use])
					{
						this.keys[1].Reset();
					}
					this.wasFiring = false;
				}
				float targetOffsetAmount = 0f;
				if (moveCam)
				{
					if (!this.IsProtectedFromPressure && (this.AnimController.CurrentHull == null || this.AnimController.CurrentHull.LethalPressure > 0f))
					{
						this.pressureEffectTimer += deltaTime;
						if (this.pressureEffectTimer > 1f)
						{
							float pressure = (this.AnimController.CurrentHull == null) ? 100f : this.AnimController.CurrentHull.LethalPressure;
							float zoomInEffectStrength = MathHelper.Clamp(pressure / 100f, 0f, 1f);
							cam.Zoom = MathHelper.Lerp(cam.Zoom, cam.DefaultZoom + Math.Max(pressure, 10f) / 150f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced), zoomInEffectStrength);
						}
					}
					else
					{
						this.pressureEffectTimer = 0f;
					}
					if (this.IsHumanoid)
					{
						cam.OffsetAmount = 250f;
					}
					else
					{
						cam.OffsetAmount = MathHelper.Clamp(this.Mass, 250f, 1500f);
					}
				}
				this.UpdateLocalCursor(cam);
				if (this.IsKeyHit(InputType.ToggleRun))
				{
					this.ToggleRun = !this.ToggleRun;
				}
				Vector2 mouseSimPos = ConvertUnits.ToSimUnits(this.cursorPosition);
				if (GUI.PauseMenuOpen)
				{
					targetOffsetAmount = (cam.OffsetAmount = 0f);
				}
				else
				{
					Item item3 = LightManager.ViewTarget as Item;
					if (item3 != null && item3.Prefab.FocusOnSelected)
					{
						targetOffsetAmount = (cam.OffsetAmount = item3.Prefab.OffsetOnSelected * item3.OffsetOnSelectedMultiplier);
					}
					else
					{
						float? num = (from holdable in this.HeldItems.SelectMany((Item item) => item.GetComponents<Holdable>())
						where holdable.Aimable
						select holdable).MaxOrNull((Holdable holdable) => holdable.CameraAimOffset);
						if (num != null)
						{
							float maxOffset = num.GetValueOrDefault();
							if (maxOffset > 0f && this.IsKeyDown(InputType.Aim))
							{
								targetOffsetAmount = (cam.OffsetAmount = maxOffset);
								goto IL_566;
							}
						}
						if (this.SelectedItem != null && this.ViewTarget == null && !this.IsIncapacitated && this.SelectedItem.Components.Any((ItemComponent ic) => ((ic != null) ? ic.GuiFrame : null) != null && ic.ShouldDrawHUD(this)))
						{
							targetOffsetAmount = (cam.OffsetAmount = 0f);
							this.cursorPosition = this.Position + PlayerInput.MouseSpeed.ClampLength(10f);
						}
						else if (!GameSettings.CurrentConfig.EnableMouseLook)
						{
							targetOffsetAmount = (cam.OffsetAmount = 0f);
						}
						else if (LightManager.ViewTarget == this)
						{
							if (GUI.PauseMenuOpen || this.IsIncapacitated)
							{
								if (deltaTime > 0f)
								{
									targetOffsetAmount = (cam.OffsetAmount = 0f);
								}
							}
							else if (Character.IsMouseOnUI)
							{
								targetOffsetAmount = cam.OffsetAmount;
							}
							else if (Vector2.DistanceSquared(this.AnimController.Limbs[0].SimPosition, mouseSimPos) > 1f)
							{
								Body body = Submarine.CheckVisibility(this.AnimController.Limbs[0].SimPosition, mouseSimPos, false, false, true, true, true, null);
								Structure structure = ((body != null) ? body.UserData : null) as Structure;
								float sightDist = Submarine.LastPickedFraction;
								if (((body != null) ? body.UserData : null) is Structure && !((Structure)body.UserData).CastShadow)
								{
									sightDist = 1f;
								}
								targetOffsetAmount = Math.Max(250f, sightDist * 500f);
							}
						}
					}
				}
				IL_566:
				cam.OffsetAmount = MathHelper.Lerp(cam.OffsetAmount, targetOffsetAmount, 0.05f);
				this.DoInteractionUpdate(deltaTime, mouseSimPos);
			}
			if (!GUI.InputBlockingMenuOpen && this.SelectedItem != null)
			{
				if (!this.SelectedItem.ActiveHUDs.Any((ItemComponent ic) => ic.GuiFrame != null && ic.CloseByClickingOutsideGUIFrame && HUD.CloseHUD(ic.GuiFrame.Rect)))
				{
					Item item2 = this.ViewTarget as Item;
					if (item2 == null || !item2.Prefab.FocusOnSelected || !PlayerInput.KeyHit(Microsoft.Xna.Framework.Input.Keys.Escape))
					{
						goto IL_622;
					}
				}
				if (GameMain.Client != null)
				{
					this.EmulateInput(InputType.Deselect);
				}
				this.focusedItem = null;
				this.FocusedCharacter = null;
				this.findFocusedTimer = 0.2f;
				this.SelectedItem = null;
			}
			IL_622:
			Character.DisableControls = false;
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x00025780 File Offset: 0x00023980
		public void UpdateLocalCursor(Camera cam)
		{
			this.cursorPosition = cam.ScreenToWorld(PlayerInput.MousePosition);
			Hull currentHull = this.AnimController.CurrentHull;
			if (((currentHull != null) ? currentHull.Submarine : null) != null)
			{
				this.cursorPosition -= this.AnimController.CurrentHull.Submarine.DrawPosition;
			}
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x000257DD File Offset: 0x000239DD
		public void EmulateInput(InputType input)
		{
			this.keys[(int)input].Hit = true;
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x000257F0 File Offset: 0x000239F0
		private void UpdateInteractablesInRange()
		{
			this.previousInteractablesInRange.Clear();
			this.previousInteractablesInRange.AddRange(this.interactablesInRange);
			this.interactablesInRange.Clear();
			IEnumerable<MapEntity> entityList = Submarine.VisibleEntities ?? Item.ItemList;
			foreach (MapEntity entity in entityList)
			{
				Item item = entity as Item;
				if (item != null && (item.body == null || item.body.Enabled) && item.ParentInventory == null && (!item.Prefab.RequireCampaignInteract || item.CampaignInteractionType != CampaignMode.InteractionType.None))
				{
					SubEditorScreen subEditorScreen = Screen.Selected as SubEditorScreen;
					if ((subEditorScreen == null || !subEditorScreen.WiringMode || item.GetComponent<ConnectionPanel>() != null) && this.CanInteractWith(item, true))
					{
						this.interactablesInRange.Add(item);
					}
				}
			}
			if (!this.interactablesInRange.SequenceEqual(this.previousInteractablesInRange))
			{
				InteractionLabelManager.RefreshInteractablesInRange(this.interactablesInRange);
			}
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x000258FC File Offset: 0x00023AFC
		public Item FindClosestItem(List<Item> itemCollection, Vector2 simPosition, float aimAssistModifier = 0f)
		{
			if (base.Submarine != null)
			{
				simPosition += base.Submarine.SimPosition;
			}
			this.debugInteractablesInRange.Clear();
			this.debugInteractablesAtCursor.Clear();
			this.debugInteractablesNearCursor.Clear();
			bool draggingItemToWorld = Barotrauma.Inventory.DraggingItemToWorld;
			float aimAssistAmount = (this.SelectedItem == null) ? (100f * aimAssistModifier) : 1f;
			Vector2 displayPosition = ConvertUnits.ToDisplayUnits(simPosition);
			Item closestItem = null;
			float closestItemDistance = Math.Max(aimAssistAmount, 2f);
			foreach (Item item in itemCollection)
			{
				if (!draggingItemToWorld || (item.OwnInventory != null && item.OwnInventory.Container.AllowDragAndDrop && item.OwnInventory.CanBePut(Barotrauma.Inventory.DraggingItems.First<Item>()) && this.CanAccessInventory(item.OwnInventory, CharacterInventory.AccessLevel.AllowBotsAndPets)))
				{
					float distanceToItem = float.PositiveInfinity;
					Rectangle transformedTrigger;
					if (item.IsInsideTrigger(displayPosition, out transformedTrigger))
					{
						this.debugInteractablesAtCursor.Add(item);
						distanceToItem = Math.Abs((float)transformedTrigger.Center.X - displayPosition.X) / (float)transformedTrigger.Width + Math.Abs((float)transformedTrigger.Y - (float)transformedTrigger.Height / 2f - displayPosition.Y) / (float)transformedTrigger.Height;
						distanceToItem *= MathHelper.Lerp(0.05f, 2f, (float)(transformedTrigger.Width + transformedTrigger.Height) / 250f);
					}
					else if (!item.Prefab.RequireCursorInsideTrigger)
					{
						Rectangle itemDisplayRect = new Rectangle(item.InteractionRect.X, item.InteractionRect.Y - item.InteractionRect.Height, item.InteractionRect.Width, item.InteractionRect.Height);
						if (itemDisplayRect.Contains(displayPosition))
						{
							this.debugInteractablesAtCursor.Add(item);
							distanceToItem = Math.Abs((float)itemDisplayRect.Center.X - displayPosition.X) / (float)itemDisplayRect.Width + Math.Abs((float)itemDisplayRect.Center.Y - displayPosition.Y) / (float)itemDisplayRect.Height;
							distanceToItem *= MathHelper.Lerp(0.05f, 2f, (float)(itemDisplayRect.Width + itemDisplayRect.Height) / 250f);
						}
						else
						{
							if (closestItemDistance < 2f)
							{
								continue;
							}
							Vector2 rectIntersectionPoint = new Vector2(MathHelper.Clamp(displayPosition.X, (float)itemDisplayRect.X, (float)itemDisplayRect.Right), MathHelper.Clamp(displayPosition.Y, (float)itemDisplayRect.Y, (float)itemDisplayRect.Bottom));
							distanceToItem = 2f + Vector2.Distance(rectIntersectionPoint, displayPosition);
						}
					}
					if (distanceToItem <= closestItemDistance && this.CanInteractWith(item, true))
					{
						this.debugInteractablesNearCursor.Add(new ValueTuple<Item, float>(item, 1f - distanceToItem / (100f * aimAssistModifier)));
						closestItem = item;
						closestItemDistance = distanceToItem;
					}
				}
			}
			return closestItem;
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x00025C34 File Offset: 0x00023E34
		private Character FindCharacterAtPosition(Vector2 mouseSimPos, float maxDist = 150f)
		{
			Character closestCharacter = null;
			maxDist = ConvertUnits.ToSimUnits(maxDist);
			float closestDist = maxDist;
			foreach (Character c in Character.CharacterList)
			{
				if (this.CanInteractWith(c, 200f, false, false))
				{
					AnimController animController = c.AnimController;
					if (animController != null && !animController.SimplePhysicsEnabled)
					{
						float dist = c.GetDistanceToClosestLimb(mouseSimPos);
						if (dist < closestDist || (c.CampaignInteractionType != CampaignMode.InteractionType.None && closestCharacter != null && closestCharacter.CampaignInteractionType == CampaignMode.InteractionType.None && dist * 0.9f < closestDist))
						{
							closestCharacter = c;
							closestDist = dist;
						}
					}
				}
			}
			return closestCharacter;
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x00025CE4 File Offset: 0x00023EE4
		public bool ShouldLockHud()
		{
			if (this != Character.controlled)
			{
				return false;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) != null && GameMain.GameSession.Campaign.ShowCampaignUI)
			{
				return true;
			}
			Item selectedItem = this.SelectedItem;
			Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
			if (this.SelectedItem != null && ((controller != null) ? controller.User : null) == this && controller.HideHUD)
			{
				Item selectedItem2 = this.SelectedItem;
				Character character;
				if (selectedItem2 == null)
				{
					character = null;
				}
				else
				{
					ConnectionPanel component = selectedItem2.GetComponent<ConnectionPanel>();
					character = ((component != null) ? component.User : null);
				}
				return character != this;
			}
			return false;
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x00025D7C File Offset: 0x00023F7C
		public static void AddAllToGUIUpdateList()
		{
			for (int i = 0; i < Character.CharacterList.Count; i++)
			{
				Character.CharacterList[i].AddToGUIUpdateList();
			}
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x00025DAE File Offset: 0x00023FAE
		public virtual void AddToGUIUpdateList()
		{
			if (Character.controlled == this)
			{
				CharacterHUD.AddToGUIUpdateList(this);
				this.CharacterHealth.AddToGUIUpdateList();
			}
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x00025DCC File Offset: 0x00023FCC
		public void DoVisibilityCheck(Camera cam)
		{
			this.IsVisible = false;
			if (!this.Enabled || this.AnimController.SimplePhysicsEnabled)
			{
				return;
			}
			foreach (Limb limb in this.AnimController.Limbs)
			{
				float maxExtent = ConvertUnits.ToDisplayUnits(limb.body.GetMaxExtent());
				if (limb.LightSource != null)
				{
					maxExtent = Math.Max(limb.LightSource.Range, maxExtent);
				}
				if (limb.body.DrawPosition.X >= (float)cam.WorldView.X - maxExtent && limb.body.DrawPosition.X <= (float)cam.WorldView.Right + maxExtent && limb.body.DrawPosition.Y >= (float)(cam.WorldView.Y - cam.WorldView.Height) - maxExtent && limb.body.DrawPosition.Y <= (float)cam.WorldView.Y + maxExtent)
				{
					this.IsVisible = true;
					return;
				}
			}
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x00025EDD File Offset: 0x000240DD
		public void Draw(SpriteBatch spriteBatch, Camera cam)
		{
			if (!this.Enabled)
			{
				return;
			}
			this.AnimController.Draw(spriteBatch, cam, this.InvisibleTimer > 0f);
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x00025F02 File Offset: 0x00024102
		public void DrawHUD(SpriteBatch spriteBatch, Camera cam, bool drawHealth = true)
		{
			CharacterHUD.Draw(spriteBatch, this, cam);
			if (drawHealth && !CharacterHUD.IsCampaignInterfaceOpen)
			{
				this.CharacterHealth.DrawHUD(spriteBatch);
			}
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x00025F24 File Offset: 0x00024124
		public void DrawGUIMessages(SpriteBatch spriteBatch, Camera cam)
		{
			if (this.info == null || !this.Enabled || this.InvisibleTimer > 0f)
			{
				return;
			}
			Vector2 messagePos = this.DrawPosition;
			messagePos.Y += this.hudInfoHeight;
			messagePos = cam.WorldToScreen(messagePos) - Vector2.UnitY * (float)GUI.IntScale(60f);
			foreach (Character.GUIMessage message in this.guiMessages)
			{
				if (message.Timer >= 0f)
				{
					Vector2 drawPos = messagePos + Vector2.UnitX * ((float)GUI.IntScale(60f) - message.Size.X);
					drawPos = new Vector2((float)((int)drawPos.X), (float)((int)drawPos.Y));
					float alpha = MathHelper.SmoothStep(1f, 0f, message.Timer / message.Lifetime);
					GUI.DrawString(spriteBatch, drawPos, message.Text, message.Color * alpha, null, 0, null, ForceUpperCase.Inherit);
					messagePos -= Vector2.UnitY * message.Size.Y * 1.2f;
				}
			}
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x00026088 File Offset: 0x00024288
		public virtual void DrawFront(SpriteBatch spriteBatch, Camera cam)
		{
			if (this.Enabled && this.InvisibleTimer <= 0f)
			{
				AnimController animController = this.AnimController;
				if (animController != null && !animController.SimplePhysicsEnabled)
				{
					if (GameMain.DebugDraw)
					{
						this.AnimController.DebugDraw(spriteBatch);
					}
					if (GUI.DisableHUD)
					{
						return;
					}
					if (Character.Controlled != null && Character.Controlled != this && base.Submarine != null && Character.Controlled.Submarine == base.Submarine && GameSettings.CurrentConfig.Graphics.LosMode != LosMode.None)
					{
						GameSession gameSession = GameMain.GameSession;
						if (!(((gameSession != null) ? gameSession.GameMode : null) is PvPMode))
						{
							float yPos = Character.Controlled.AnimController.FloorY - 1.5f;
							if (Character.Controlled.AnimController.Stairs != null)
							{
								yPos = Character.Controlled.AnimController.Stairs.SimPosition.Y - (float)Character.Controlled.AnimController.Stairs.RectHeight * 0.5f;
							}
							if (this.AnimController.FloorY < yPos)
							{
								return;
							}
						}
					}
					Vector2 pos = this.DrawPosition;
					pos.Y += this.hudInfoHeight;
					float paddingBelowCeiling = 30f;
					if (this.CurrentHull != null && this.DrawPosition.Y + 78f > (float)this.CurrentHull.WorldRect.Y - paddingBelowCeiling)
					{
						float lowerAmount = this.DrawPosition.Y + 78f - ((float)this.CurrentHull.WorldRect.Y - paddingBelowCeiling);
						this.hudInfoHeight = MathHelper.Lerp(this.hudInfoHeight, 78f - lowerAmount, 0.1f);
						this.hudInfoHeight = Math.Max(this.hudInfoHeight, 20f);
					}
					else
					{
						this.hudInfoHeight = MathHelper.Lerp(this.hudInfoHeight, 78f, 0.1f);
					}
					pos.Y = -pos.Y;
					if (this == Character.controlled)
					{
						if (!Character.DebugDrawInteract)
						{
							goto IL_B5A;
						}
						Vector2 cursorPos = cam.ScreenToWorld(PlayerInput.MousePosition);
						cursorPos.Y = -cursorPos.Y;
						foreach (Item item in this.debugInteractablesAtCursor)
						{
							GUI.DrawLine(spriteBatch, cursorPos, new Vector2(item.DrawPosition.X, -item.DrawPosition.Y), Color.LightGreen, 0f, 4f);
						}
						foreach (Item item2 in this.debugInteractablesInRange)
						{
							GUI.DrawLine(spriteBatch, new Vector2(this.DrawPosition.X, -this.DrawPosition.Y), new Vector2(item2.DrawPosition.X, -item2.DrawPosition.Y), Color.White * 0.1f, 0f, 4f);
						}
						using (List<ValueTuple<Item, float>>.Enumerator enumerator3 = this.debugInteractablesNearCursor.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								ValueTuple<Item, float> valueTuple = enumerator3.Current;
								Item item3 = valueTuple.Item1;
								float dist = valueTuple.Item2;
								GUI.DrawLine(spriteBatch, cursorPos, new Vector2(item3.DrawPosition.X, -item3.DrawPosition.Y), ToolBox.GradientLerp(dist, new Color[]
								{
									GUIStyle.Red,
									GUIStyle.Orange,
									GUIStyle.Green
								}), 0f, 2f);
							}
							goto IL_B5A;
						}
					}
					float hoverRange = 300f;
					float fadeOutRange = 200f;
					float cursorDist = Vector2.Distance(this.WorldPosition, cam.ScreenToWorld(PlayerInput.MousePosition));
					float hudInfoAlpha = (this.CampaignInteractionType == CampaignMode.InteractionType.None) ? MathHelper.Clamp(1f - (cursorDist - (hoverRange - fadeOutRange)) / fadeOutRange, 0.2f, 1f) : 1f;
					GameSession gameSession2 = GameMain.GameSession;
					float nameTextAlpha = (((gameSession2 != null) ? gameSession2.GameMode : null) is PvPMode) ? 1f : hudInfoAlpha;
					if (!GUI.DisableCharacterNames && this.hudInfoVisible && (Character.controlled == null || this != Character.controlled.FocusedCharacter || this.IsPet) && cam.Zoom > 0.4f)
					{
						if (this.info != null)
						{
							LocalizedString name = this.Info.DisplayName;
							if (Character.controlled == null && name != this.Info.Name)
							{
								name += " " + TextManager.Get("Disguised");
							}
							else if (this.Info.Title != null && this.TeamID != CharacterTeamType.Team1)
							{
								name += '\n' + this.Info.Title;
							}
							Vector2 nameSize = GUIStyle.Font.MeasureString(name, false);
							Vector2 namePos = new Vector2(pos.X, pos.Y - 5f - 5f / cam.Zoom) - nameSize * 0.5f / cam.Zoom;
							Color nameColor = this.GetNameColor();
							Vector2 screenSize = new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight);
							Vector2 viewportSize = new Vector2((float)cam.WorldView.Width, (float)cam.WorldView.Height);
							namePos.X -= (float)cam.WorldView.X;
							namePos.Y += (float)cam.WorldView.Y;
							namePos *= screenSize / viewportSize;
							namePos.X = (float)Math.Floor((double)namePos.X);
							namePos.Y = (float)Math.Floor((double)namePos.Y);
							namePos *= viewportSize / screenSize;
							namePos.X += (float)cam.WorldView.X;
							namePos.Y -= (float)cam.WorldView.Y;
							if (this.CampaignInteractionType != CampaignMode.InteractionType.None && this.AllowCustomInteract)
							{
								GUIComponentStyle iconStyle = GUIStyle.GetComponentStyle("CampaignInteractionBubble." + this.CampaignInteractionType.ToString());
								if (iconStyle != null)
								{
									Limb limb = this.AnimController.GetLimb(LimbType.Head, true, false, false);
									Vector2? vector;
									if (limb == null)
									{
										vector = null;
									}
									else
									{
										PhysicsBody body = limb.body;
										vector = ((body != null) ? new Vector2?(body.DrawPosition) : null);
									}
									Vector2 headPos = vector ?? (this.DrawPosition + Vector2.UnitY * 100f);
									Vector2 iconPos = headPos;
									iconPos.Y = -iconPos.Y;
									nameColor = iconStyle.Color;
									UISprite icon = iconStyle.Sprites[GUIComponent.ComponentState.None].First<UISprite>();
									float iconScale = 30f / icon.Sprite.size.X / cam.Zoom * GUI.Scale;
									icon.Sprite.Draw(spriteBatch, iconPos + new Vector2(-35f, -25f), iconStyle.Color * hudInfoAlpha, 0f, iconScale, SpriteEffects.None, null);
								}
							}
							GUIStyle.Font.DrawString(spriteBatch, name, namePos + new Vector2(1f / cam.Zoom, 1f / cam.Zoom), Color.Black, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0.001f, Alignment.TopLeft);
							GUIStyle.Font.DrawString(spriteBatch, name, namePos, nameColor * nameTextAlpha, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f, Alignment.TopLeft);
							if (GameMain.DebugDraw)
							{
								GUIStyle.Font.DrawString(spriteBatch, this.ID.ToString(), namePos - new Vector2(0f, 20f), Color.White, ForceUpperCase.Inherit, false);
							}
						}
						EnemyAIController enemyAIController = this.AIController as EnemyAIController;
						PetBehavior petBehavior = (enemyAIController != null) ? enemyAIController.PetBehavior : null;
						if (petBehavior != null && !this.IsDead && !this.IsUnconscious)
						{
							PetBehavior.StatusIndicatorType petStatus = petBehavior.GetCurrentStatusIndicatorType();
							if (petStatus != PetBehavior.StatusIndicatorType.None)
							{
								GUIComponentStyle iconStyle2 = GUIStyle.GetComponentStyle("PetIcon." + petStatus.ToString());
								if (iconStyle2 != null)
								{
									Limb limb2 = this.AnimController.GetLimb(LimbType.Head, true, false, false);
									Vector2? vector2;
									if (limb2 == null)
									{
										vector2 = null;
									}
									else
									{
										PhysicsBody body2 = limb2.body;
										vector2 = ((body2 != null) ? new Vector2?(body2.DrawPosition) : null);
									}
									Vector2 headPos2 = vector2 ?? (this.DrawPosition + Vector2.UnitY * 100f);
									Vector2 iconPos2 = headPos2;
									iconPos2.Y = -iconPos2.Y;
									UISprite icon2 = iconStyle2.Sprites[GUIComponent.ComponentState.None].First<UISprite>();
									float iconScale2 = 30f / icon2.Sprite.size.X / cam.Zoom;
									icon2.Sprite.Draw(spriteBatch, iconPos2 + new Vector2(-35f, -25f), iconStyle2.Color * hudInfoAlpha, 0f, iconScale2, SpriteEffects.None, null);
								}
							}
						}
					}
					if (this.IsDead)
					{
						return;
					}
					NetworkMember networkMember = GameMain.NetworkMember;
					EnemyHealthBarMode healthBarMode = (networkMember != null) ? networkMember.ServerSettings.ShowEnemyHealthBars : GameSettings.CurrentConfig.ShowEnemyHealthBars;
					if (healthBarMode != EnemyHealthBarMode.ShowAll)
					{
						if (Character.Controlled != null)
						{
							if (HumanAIController.IsFriendly(Character.Controlled, this, false, false))
							{
								HumanAIController humanAi = this.AIController as HumanAIController;
								if (humanAi == null)
								{
									goto IL_A47;
								}
								AIObjectiveCombat combatObjective = humanAi.ObjectiveManager.CurrentObjective as AIObjectiveCombat;
								if (combatObjective == null || !HumanAIController.IsFriendly(Character.Controlled, combatObjective.Enemy, false, false))
								{
									goto IL_A47;
								}
							}
							return;
						}
						if (!this.IsOnPlayerTeam)
						{
							return;
						}
					}
					IL_A47:
					if (this.Params.ShowHealthBar && this.CharacterHealth.DisplayedVitality < this.MaxVitality * 0.98f && this.hudInfoVisible && this.AIState != AIState.PlayDead && this.AIState != AIState.Hiding)
					{
						hudInfoAlpha = Math.Max(hudInfoAlpha, Math.Min(this.CharacterHealth.DamageOverlayTimer, 1f));
						Vector2 healthBarPos = new Vector2(pos.X - 50f, -pos.Y);
						GUI.DrawProgressBar(spriteBatch, healthBarPos, new Vector2(100f, 15f), this.CharacterHealth.DisplayedVitality / this.MaxVitality, Color.Lerp(GUIStyle.Red, GUIStyle.Green, this.CharacterHealth.DisplayedVitality / this.MaxVitality) * 0.8f * hudInfoAlpha, new Color(0.5f, 0.57f, 0.6f, 1f) * hudInfoAlpha, 0f);
					}
					IL_B5A:
					if (this.textlessSpeechBubble != null)
					{
						Vector2 iconPos3 = pos - Vector2.UnitY * 5f;
						GUIStyle.SpeechBubbleIcon.Value.Sprite.Draw(spriteBatch, iconPos3, this.textlessSpeechBubble.Color * Math.Min(this.textlessSpeechBubble.LifeTime, 1f), 0f, Math.Min(this.textlessSpeechBubble.LifeTime, 1f), SpriteEffects.None, null);
					}
					return;
				}
			}
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x00026C98 File Offset: 0x00024E98
		public void ShowSpeechBubble(Color color, string text)
		{
			if (!GameSettings.CurrentConfig.ChatSpeechBubbles)
			{
				this.ShowTextlessSpeechBubble(1f, color);
				return;
			}
			float duration = MathHelper.Lerp(1f, 8f, Math.Min((float)text.Length / 100f, 1f));
			Character.speechBubbles.Add(new Character.SpeechBubble(this, duration, color, text));
			this.textlessSpeechBubble = null;
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x00026D00 File Offset: 0x00024F00
		public void ShowTextlessSpeechBubble(float duration, Color color)
		{
			if (Character.speechBubbles.Any((Character.SpeechBubble sb) => sb.Character == this))
			{
				return;
			}
			if (this.textlessSpeechBubble == null)
			{
				this.textlessSpeechBubble = new Character.SpeechBubble(this, duration, color, "");
				return;
			}
			this.textlessSpeechBubble.Color = color;
			this.textlessSpeechBubble.LifeTime = Math.Max(this.textlessSpeechBubble.LifeTime, duration);
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00026D6C File Offset: 0x00024F6C
		public static void DrawSpeechBubbles(SpriteBatch spriteBatch, Camera cam)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, null, null, new Matrix?(cam.Transform));
			foreach (Character.SpeechBubble bubble in Character.speechBubbles)
			{
				Vector2 iconPos = Timing.Interpolate(bubble.PrevPosition, bubble.Position);
				iconPos += Vector2.UnitY * bubble.MoveUpAmount;
				if (bubble.Submarine != null)
				{
					iconPos += bubble.Submarine.DrawPosition;
				}
				float alpha = 1f;
				float mouseDist = Vector2.Distance(cam.WorldToScreen(iconPos), PlayerInput.MousePosition);
				float textSize = bubble.TextSize.Length();
				if (mouseDist < textSize)
				{
					alpha *= Math.Max(mouseDist / textSize, 0.5f);
				}
				iconPos.Y = -iconPos.Y;
				UISprite speechBubbleIconSliced = GUIStyle.SpeechBubbleIconSliced.Value;
				if (speechBubbleIconSliced != null)
				{
					Vector2 bubbleSize = bubble.TextSize + Vector2.One * (float)GUI.IntScale(15f);
					speechBubbleIconSliced.Draw(spriteBatch, new RectangleF(iconPos - bubbleSize / 2f, bubbleSize), bubble.Color * Math.Min(bubble.LifeTime, 1f) * alpha, SpriteEffects.None, null);
				}
				Vector2 pos = iconPos - bubble.TextSize / 2f;
				string sanitizedValue = bubble.Text.SanitizedValue;
				Color color = bubble.Color * Math.Min(bubble.LifeTime, 1f) * alpha;
				ImmutableArray<RichTextData>? richTextData = bubble.RichTextData;
				GUIFont smallFont = GUIStyle.SmallFont;
				GUI.DrawStringWithColors(spriteBatch, pos, sanitizedValue, color, richTextData, null, 0, smallFont, 0f);
			}
			spriteBatch.End();
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x00026F70 File Offset: 0x00025170
		public Color GetNameColor()
		{
			CharacterTeamType team = this.teamID;
			CharacterInfo characterInfo = this.Info;
			if (characterInfo != null && characterInfo.IsDisguisedAsAnother)
			{
				Item itemInLimbSlot = this.Inventory.GetItemInLimbSlot(InvSlotType.Card);
				IdCard idCard = (itemInLimbSlot != null) ? itemInLimbSlot.GetComponent<IdCard>() : null;
				if (idCard != null)
				{
					if (team == CharacterTeamType.Team2 && idCard.TeamID != CharacterTeamType.Team2)
					{
						team = CharacterTeamType.Team1;
					}
					else if (team == CharacterTeamType.Team1 && idCard.TeamID == CharacterTeamType.Team2)
					{
						team = CharacterTeamType.Team2;
					}
				}
			}
			Character character = Character.Controlled;
			CharacterTeamType valueOrDefault;
			if (character == null)
			{
				GameClient client = GameMain.Client;
				CharacterTeamType? characterTeamType;
				if (client == null)
				{
					characterTeamType = null;
				}
				else
				{
					Client myClient = client.MyClient;
					characterTeamType = ((myClient != null) ? new CharacterTeamType?(myClient.TeamID) : null);
				}
				CharacterTeamType? characterTeamType2 = characterTeamType;
				valueOrDefault = characterTeamType2.GetValueOrDefault(CharacterTeamType.Team1);
			}
			else
			{
				valueOrDefault = character.TeamID;
			}
			CharacterTeamType myTeam = valueOrDefault;
			Color nameColor = GUIStyle.TextColorNormal;
			if (this.TeamID == CharacterTeamType.FriendlyNPC)
			{
				nameColor = (this.UniqueNameColor ?? Color.SkyBlue);
			}
			else if (team != myTeam)
			{
				nameColor = GUIStyle.Red;
			}
			return nameColor;
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x00027070 File Offset: 0x00025270
		public void AddMessage(string rawText, Color color, bool playSound, Identifier identifier = default(Identifier), int? value = null, float lifetime = 3f)
		{
			Character.GUIMessage existingMessage = null;
			float delay = 0f;
			if (this.guiMessages.Any<Character.GUIMessage>())
			{
				delay = this.guiMessages.Min((Character.GUIMessage m) => m.Timer) - 0.5f;
				if (delay < 0f)
				{
					delay = -delay;
					if (this.guiMessages.Count > 5)
					{
						(from m in this.guiMessages
						where m.Timer < 0f
						select m).ForEach(delegate(Character.GUIMessage m)
						{
							m.Timer *= 0.9f;
						});
					}
				}
				else
				{
					delay = 0f;
				}
			}
			if (identifier != null)
			{
				existingMessage = this.guiMessages.Find((Character.GUIMessage m) => m.Identifier == identifier && m.Timer < m.Lifetime * 0.5f);
			}
			if (existingMessage == null || value == null)
			{
				Character.GUIMessage newMessage = new Character.GUIMessage(rawText, color, delay, identifier, value, lifetime);
				this.guiMessages.Insert(0, newMessage);
				if (playSound)
				{
					if (delay > 0f)
					{
						newMessage.PlaySound = true;
						return;
					}
					SoundPlayer.PlayUISound(GUISoundType.UIMessage);
					return;
				}
			}
			else
			{
				existingMessage.Value += value.Value;
			}
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x000271C4 File Offset: 0x000253C4
		public HUDProgressBar UpdateHUDProgressBar(object linkedObject, Vector2 worldPosition, float progress, Color emptyColor, Color fullColor, string textTag = "")
		{
			if (Character.controlled != this)
			{
				return null;
			}
			HUDProgressBar progressBar;
			if (!this.hudProgressBars.TryGetValue(linkedObject, out progressBar))
			{
				progressBar = new HUDProgressBar(worldPosition, base.Submarine, emptyColor, fullColor, textTag);
				this.hudProgressBars.Add(linkedObject, progressBar);
			}
			else
			{
				progressBar.TextTag = textTag;
			}
			progressBar.WorldPosition = worldPosition;
			progressBar.FadeTimer = Math.Max(progressBar.FadeTimer, 1f);
			progressBar.Progress = progress;
			return progressBar;
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x0002723C File Offset: 0x0002543C
		public void PlaySound(CharacterSound.SoundType soundType, float soundIntervalFactor = 1f, float maxInterval = 0f)
		{
			if (base.Removed)
			{
				return;
			}
			if (this.sounds == null || this.sounds.Count == 0)
			{
				return;
			}
			if (this.soundChannel != null && this.soundChannel.IsPlaying)
			{
				return;
			}
			SoundManager soundManager = GameMain.SoundManager;
			if (soundManager == null || soundManager.Disabled)
			{
				return;
			}
			if (this.soundTimer > this.Params.SoundInterval * soundIntervalFactor)
			{
				return;
			}
			if (this.Params.SoundInterval - this.soundTimer < maxInterval)
			{
				return;
			}
			this.matchingSounds.Clear();
			foreach (CharacterSound s in this.sounds)
			{
				if (s.Type == soundType && (s.TagSet.None(null) || (this.info != null && s.TagSet.IsSubsetOf(this.info.Head.Preset.TagSet))))
				{
					this.matchingSounds.Add(s);
				}
			}
			CharacterSound selectedSound = this.matchingSounds.GetRandomUnsynced<CharacterSound>();
			if (((selectedSound != null) ? selectedSound.Sound : null) == null)
			{
				return;
			}
			Sound sound = selectedSound.Sound;
			Vector2 worldPosition = this.AnimController.WorldPosition;
			float? volume = new float?(selectedSound.Volume);
			float? range = new float?(selectedSound.Range);
			Hull currentHull = this.CurrentHull;
			bool ignoreMuffling = selectedSound.IgnoreMuffling;
			this.soundChannel = SoundPlayer.PlaySound(sound, worldPosition, volume, range, null, currentHull, ignoreMuffling, false);
			this.soundTimer = this.Params.SoundInterval;
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x000273D0 File Offset: 0x000255D0
		public void AddActiveObjectiveEntity(Entity entity, Sprite sprite, Color? color = null)
		{
			if (this.activeObjectiveEntities.Any((Character.ObjectiveEntity aoe) => aoe.Entity == entity))
			{
				return;
			}
			Character.ObjectiveEntity objectiveEntity = new Character.ObjectiveEntity(entity, sprite, color);
			this.activeObjectiveEntities.Add(objectiveEntity);
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x00027420 File Offset: 0x00025620
		public void RemoveActiveObjectiveEntity(Entity entity)
		{
			Character.ObjectiveEntity found = this.activeObjectiveEntities.Find((Character.ObjectiveEntity aoe) => aoe.Entity == entity);
			if (found == null)
			{
				return;
			}
			this.activeObjectiveEntities.Remove(found);
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x00027463 File Offset: 0x00025663
		public CharacterSound GetSound(Func<CharacterSound, bool> predicate = null, bool random = false)
		{
			if (!random)
			{
				return this.sounds.FirstOrDefault(predicate);
			}
			return this.sounds.GetRandomUnsynced(predicate);
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x00027484 File Offset: 0x00025684
		public void ClientWriteInput(in SegmentTableWriter<ClientNetSegment> segmentTableWriter, IWriteMessage msg)
		{
			segmentTableWriter.StartNewSegment(ClientNetSegment.CharacterInput);
			if (this.memInput.Count > 60)
			{
				this.memInput.RemoveRange(60, this.memInput.Count - 60);
			}
			msg.WriteUInt16(this.LastNetworkUpdateID);
			byte inputCount = Math.Min((byte)this.memInput.Count, 60);
			msg.WriteByte(inputCount);
			for (int i = 0; i < (int)inputCount; i++)
			{
				msg.WriteRangedInteger((int)this.memInput[i].states, 0, 65535);
				msg.WriteUInt16(this.memInput[i].intAim);
				if (this.memInput[i].states.HasFlag(Character.InputNetFlags.Select) || this.memInput[i].states.HasFlag(Character.InputNetFlags.Deselect) || this.memInput[i].states.HasFlag(Character.InputNetFlags.Use) || this.memInput[i].states.HasFlag(Character.InputNetFlags.Health) || this.memInput[i].states.HasFlag(Character.InputNetFlags.Grab))
				{
					msg.WriteUInt16(this.memInput[i].interact);
				}
			}
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x0002760C File Offset: 0x0002580C
		public virtual void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			Character.IEventData eventData = extraData as Character.IEventData;
			if (eventData == null)
			{
				throw new Exception("Malformed character event: expected Character.IEventData");
			}
			msg.WriteRangedInteger((int)eventData.EventType, 0, 18);
			if (eventData is Character.InventoryStateEventData)
			{
				Character.InventoryStateEventData inventoryStateEventData = (Character.InventoryStateEventData)eventData;
				this.Inventory.ClientEventWrite(msg, inventoryStateEventData);
				return;
			}
			if (!(eventData is Character.TreatmentEventData))
			{
				if (!(eventData is Character.ConfirmRefundEventData) && !(eventData is Character.CharacterStatusEventData))
				{
					if (eventData is Character.UpdateTalentsEventData)
					{
						msg.WriteUInt16((ushort)this.characterTalents.Count);
						using (List<CharacterTalent>.Enumerator enumerator = this.characterTalents.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								CharacterTalent unlockedTalent = enumerator.Current;
								msg.WriteUInt32(unlockedTalent.Prefab.UintIdentifier);
							}
							return;
						}
					}
					throw new Exception("Malformed character event: did not expect " + eventData.GetType().Name);
				}
				return;
			}
			msg.WriteBoolean(this.AnimController.Anim == AnimController.Animation.CPR);
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x00027718 File Offset: 0x00025918
		public void ClientReadPosition(IReadMessage msg, float sendingTime)
		{
			bool facingRight = this.AnimController.Dir > 0f;
			this.lastRecvPositionUpdateTime = (float)NetTime.Now;
			this.AnimController.Frozen = false;
			this.Enabled = true;
			if (this.DisabledByEvent && !base.Removed)
			{
				this.DisabledByEvent = false;
			}
			ushort networkUpdateID = 0;
			if (msg.ReadBoolean())
			{
				networkUpdateID = msg.ReadUInt16();
			}
			else
			{
				bool aimInput = msg.ReadBoolean();
				this.keys[2].Held = aimInput;
				this.keys[2].SetState(false, aimInput);
				bool shootInput = msg.ReadBoolean();
				this.keys[25].Held = shootInput;
				this.keys[25].SetState(false, shootInput);
				bool useInput = msg.ReadBoolean();
				this.keys[1].Held = useInput;
				this.keys[1].SetState(false, useInput);
				if (this.AnimController is HumanoidAnimController)
				{
					bool crouching = msg.ReadBoolean();
					this.keys[10].Held = crouching;
					this.keys[10].SetState(false, crouching);
				}
				else
				{
					FishAnimController fishAnim = this.AnimController as FishAnimController;
					if (fishAnim != null)
					{
						fishAnim.Reverse = msg.ReadBoolean();
					}
				}
				bool attackInput = msg.ReadBoolean();
				this.keys[7].Held = attackInput;
				this.keys[7].SetState(false, attackInput);
				double aimAngle = (double)msg.ReadUInt16() / 65535.0 * 2.0 * 3.141592653589793;
				this.cursorPosition = this.AimRefPosition + new Vector2((float)Math.Cos(aimAngle), (float)Math.Sin(aimAngle)) * 500f;
				bool ragdollInput = msg.ReadBoolean();
				this.keys[15].Held = ragdollInput;
				this.keys[15].SetState(false, ragdollInput);
				facingRight = msg.ReadBoolean();
			}
			bool entitySelected = msg.ReadBoolean();
			Character selectedCharacter = null;
			Item selectedItem = null;
			Item selectedSecondaryItem = null;
			AnimController.Animation animation = AnimController.Animation.None;
			if (entitySelected)
			{
				ushort characterID = msg.ReadUInt16();
				ushort itemID = msg.ReadUInt16();
				ushort secondaryItemID = msg.ReadUInt16();
				selectedCharacter = (Entity.FindEntityByID(characterID) as Character);
				selectedItem = (Entity.FindEntityByID(itemID) as Item);
				selectedSecondaryItem = (Entity.FindEntityByID(secondaryItemID) as Item);
				if (characterID != 0)
				{
					bool doingCpr = msg.ReadBoolean();
					if (doingCpr && this.SelectedCharacter != null)
					{
						animation = AnimController.Animation.CPR;
					}
				}
			}
			Vector2 pos = new Vector2(msg.ReadSingle(), msg.ReadSingle());
			float MaxVel = 64f;
			Vector2 linearVelocity = new Vector2(msg.ReadRangedSingle(-MaxVel, MaxVel, 12), msg.ReadRangedSingle(-MaxVel, MaxVel, 12));
			linearVelocity = NetConfig.Quantize(linearVelocity, -MaxVel, MaxVel, 12);
			Vector2 targetMovement = new Vector2(msg.ReadRangedSingle(-20f, 20f, 12), msg.ReadRangedSingle(-20f, 20f, 12));
			targetMovement = NetConfig.Quantize(targetMovement, -20f, 20f, 12);
			bool fixedRotation = msg.ReadBoolean();
			float? rotation = null;
			float? angularVelocity = null;
			if (!fixedRotation)
			{
				rotation = new float?(msg.ReadSingle());
				angularVelocity = new float?(msg.ReadSingle());
			}
			bool ignorePlatforms = msg.ReadBoolean();
			bool readStatus = msg.ReadBoolean();
			if (readStatus)
			{
				this.ReadStatus(msg);
				bool isEnemyAi = msg.ReadBoolean();
				if (isEnemyAi)
				{
					byte aiState = msg.ReadByte();
					EnemyAIController enemyAi = this.AIController as EnemyAIController;
					if (enemyAi != null)
					{
						enemyAi.State = (AIState)aiState;
					}
					else
					{
						DebugConsole.AddWarning("Received enemy AI data for a character with no EnemyAIController. Ignoring...", null);
					}
					bool isPet = msg.ReadBoolean();
					if (isPet)
					{
						byte happiness = msg.ReadByte();
						byte hunger = msg.ReadByte();
						EnemyAIController enemyAIController = this.AIController as EnemyAIController;
						if (enemyAIController != null)
						{
							PetBehavior petBehavior = enemyAIController.PetBehavior;
							if (petBehavior != null)
							{
								petBehavior.Happiness = (float)happiness / 255f * petBehavior.MaxHappiness;
								petBehavior.Hunger = (float)hunger / 255f * petBehavior.MaxHunger;
								goto IL_3DF;
							}
						}
						DebugConsole.AddWarning("Received pet AI data for a character with no PetBehavior. Ignoring...", null);
					}
				}
			}
			IL_3DF:
			msg.ReadPadBits();
			int index = 0;
			if (GameMain.Client.Character == this)
			{
				CharacterStateInfo posInfo = new CharacterStateInfo(pos, rotation, networkUpdateID, facingRight ? Direction.Right : Direction.Left, selectedCharacter, selectedItem, selectedSecondaryItem, targetMovement, animation, ignorePlatforms);
				while (index < this.memState.Count && NetIdUtils.IdMoreRecent(posInfo.ID, this.memState[index].ID))
				{
					index++;
				}
				this.memState.Insert(index, posInfo);
				return;
			}
			CharacterStateInfo posInfo2 = new CharacterStateInfo(pos, rotation, linearVelocity, angularVelocity, sendingTime, facingRight ? Direction.Right : Direction.Left, selectedCharacter, selectedItem, selectedSecondaryItem, targetMovement, animation, ignorePlatforms);
			while (index < this.memState.Count && posInfo2.Timestamp > this.memState[index].Timestamp)
			{
				index++;
			}
			this.memState.Insert(index, posInfo2);
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00027BE4 File Offset: 0x00025DE4
		public virtual void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			Character.EventType eventType = (Character.EventType)msg.ReadRangedInteger(0, 18);
			switch (eventType)
			{
			case Character.EventType.InventoryState:
				if (this.Inventory == null)
				{
					string errorMsg = "Received an inventory update message for an entity with no inventory ([name], removed: " + base.Removed.ToString() + ")";
					DebugConsole.ThrowError(errorMsg.Replace("[name]", this.Name), null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("CharacterNetworking.ClientRead:NoInventory" + this.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg.Replace("[name]", this.SpeciesName.Value));
					msg.ReadUInt16();
					byte inventoryItemCount = msg.ReadByte();
					for (int i = 0; i < (int)inventoryItemCount; i++)
					{
						msg.ReadUInt16();
					}
				}
				else
				{
					this.Inventory.ClientEventRead(msg);
				}
				break;
			case Character.EventType.Control:
			{
				bool myCharacter = msg.ReadBoolean();
				byte ownerID = msg.ReadByte();
				bool renamingEnabled = msg.ReadBoolean();
				this.ResetNetState();
				if (myCharacter)
				{
					if (Character.controlled != null)
					{
						this.LastNetworkUpdateID = Character.controlled.LastNetworkUpdateID;
					}
					if (!this.IsDead)
					{
						Character.Controlled = this;
					}
					this.IsRemotePlayer = false;
					GameMain.Client.HasSpawned = true;
					GameMain.Client.Character = this;
					GameMain.LightManager.LosEnabled = true;
					GameMain.LightManager.LosAlpha = 1f;
					GameMain.Client.WaitForNextRoundRespawn = null;
				}
				else
				{
					if (Character.controlled == this)
					{
						Character.Controlled = null;
					}
					GameClient client = GameMain.Client;
					if (((client != null) ? client.Character : null) == this)
					{
						GameMain.Client.Character = null;
					}
					this.IsRemotePlayer = (ownerID > 0);
				}
				if (this.info != null)
				{
					this.info.RenamingEnabled = renamingEnabled;
				}
				break;
			}
			case Character.EventType.Status:
				this.ReadStatus(msg);
				this.GodMode = msg.ReadBoolean();
				break;
			case Character.EventType.SetAttackTarget:
			case Character.EventType.ExecuteAttack:
			{
				int attackLimbIndex = (int)msg.ReadByte();
				ushort targetEntityID = msg.ReadUInt16();
				int targetLimbIndex = (int)msg.ReadByte();
				float targetX = msg.ReadSingle();
				float targetY = msg.ReadSingle();
				Vector2 targetSimPos = new Vector2(targetX, targetY);
				if (attackLimbIndex != 255 && targetEntityID != 0 && !base.Removed)
				{
					if (attackLimbIndex >= this.AnimController.Limbs.Length)
					{
						if (!GameMain.Client.MidRoundSyncing)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Received invalid ");
							defaultInterpolatedStringHandler.AppendFormatted((eventType == Character.EventType.SetAttackTarget) ? "SetAttackTarget" : "ExecuteAttack");
							defaultInterpolatedStringHandler.AppendLiteral(" message. Limb index out of bounds (character: ");
							defaultInterpolatedStringHandler.AppendFormatted(this.Name);
							defaultInterpolatedStringHandler.AppendLiteral(", limb index: ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(attackLimbIndex);
							defaultInterpolatedStringHandler.AppendLiteral(", limb count: ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(this.AnimController.Limbs.Length);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							string errorMsg2 = defaultInterpolatedStringHandler.ToStringAndClear();
							DebugConsole.ThrowError(errorMsg2, null, null, false, false);
							GameAnalyticsManager.AddErrorEventOnce("Character.ClientEventRead:AttackLimbOutOfBounds", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
						}
					}
					else
					{
						Limb attackLimb = this.AnimController.Limbs[attackLimbIndex];
						Limb targetLimb = null;
						IDamageable targetEntity = Entity.FindEntityByID(targetEntityID) as IDamageable;
						if (targetEntity == null && eventType == Character.EventType.SetAttackTarget)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(71, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("Received invalid SetAttackTarget message. Target entity not found (ID ");
							defaultInterpolatedStringHandler2.AppendFormatted<ushort>(targetEntityID);
							defaultInterpolatedStringHandler2.AppendLiteral(")");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
							GameAnalyticsManager.AddErrorEventOnce("Character.ClientEventRead:TargetNotFound", GameAnalyticsManager.ErrorSeverity.Error, "Received invalid SetAttackTarget message. Target entity not found.");
						}
						else
						{
							Character targetCharacter = targetEntity as Character;
							if (targetCharacter != null && targetLimbIndex != 255)
							{
								if (targetLimbIndex >= targetCharacter.AnimController.Limbs.Length)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(107, 4);
									defaultInterpolatedStringHandler3.AppendLiteral("Received invalid ");
									defaultInterpolatedStringHandler3.AppendFormatted((eventType == Character.EventType.SetAttackTarget) ? "SetAttackTarget" : "ExecuteAttack");
									defaultInterpolatedStringHandler3.AppendLiteral(" message. Target limb index out of bounds (target character: ");
									defaultInterpolatedStringHandler3.AppendFormatted(targetCharacter.Name);
									defaultInterpolatedStringHandler3.AppendLiteral(", limb index: ");
									defaultInterpolatedStringHandler3.AppendFormatted<int>(targetLimbIndex);
									defaultInterpolatedStringHandler3.AppendLiteral(", limb count: ");
									defaultInterpolatedStringHandler3.AppendFormatted<int>(targetCharacter.AnimController.Limbs.Length);
									defaultInterpolatedStringHandler3.AppendLiteral(")");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(107, 4);
									defaultInterpolatedStringHandler4.AppendLiteral("Received invalid ");
									defaultInterpolatedStringHandler4.AppendFormatted((eventType == Character.EventType.SetAttackTarget) ? "SetAttackTarget" : "ExecuteAttack");
									defaultInterpolatedStringHandler4.AppendLiteral(" message. Target limb index out of bounds (target character: ");
									defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(targetCharacter.SpeciesName);
									defaultInterpolatedStringHandler4.AppendLiteral(", limb index: ");
									defaultInterpolatedStringHandler4.AppendFormatted<int>(targetLimbIndex);
									defaultInterpolatedStringHandler4.AppendLiteral(", limb count: ");
									defaultInterpolatedStringHandler4.AppendFormatted<int>(targetCharacter.AnimController.Limbs.Length);
									defaultInterpolatedStringHandler4.AppendLiteral(")");
									string errorMsgWithoutName = defaultInterpolatedStringHandler4.ToStringAndClear();
									GameAnalyticsManager.AddErrorEventOnce("Character.ClientEventRead:TargetLimbOutOfBounds", GameAnalyticsManager.ErrorSeverity.Error, errorMsgWithoutName);
									break;
								}
								targetLimb = targetCharacter.AnimController.Limbs[targetLimbIndex];
							}
							if (((attackLimb != null) ? attackLimb.attack : null) != null && Character.Controlled != this)
							{
								if (eventType == Character.EventType.SetAttackTarget)
								{
									this.SetAttackTarget(attackLimb, targetEntity, targetSimPos);
									this.PlaySound(CharacterSound.SoundType.Attack, 1f, 3f);
								}
								else
								{
									AttackResult attackResult;
									attackLimb.ExecuteAttack(targetEntity, targetLimb, out attackResult);
								}
							}
						}
					}
				}
				break;
			}
			case Character.EventType.AssignCampaignInteraction:
			{
				byte campaignInteractionType = msg.ReadByte();
				bool requireConsciousness = msg.ReadBoolean();
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaignMode = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaignMode != null)
				{
					campaignMode.AssignNPCMenuInteraction(this, (CampaignMode.InteractionType)campaignInteractionType);
				}
				this.RequireConsciousnessForCustomInteract = requireConsciousness;
				break;
			}
			case Character.EventType.ObjectiveManagerState:
			{
				AIObjectiveManager.ObjectiveType msgType = (AIObjectiveManager.ObjectiveType)msg.ReadRangedInteger(0, 2);
				if (msgType != AIObjectiveManager.ObjectiveType.None)
				{
					bool validData = msg.ReadBoolean();
					if (validData)
					{
						if (msgType == AIObjectiveManager.ObjectiveType.Order)
						{
							uint orderPrefabUintIdentifier = msg.ReadUInt32();
							OrderPrefab orderPrefab = OrderPrefab.Prefabs.Find((OrderPrefab p) => p.UintIdentifier == orderPrefabUintIdentifier);
							Identifier option = Identifier.Empty;
							if (orderPrefab.HasOptions)
							{
								int optionIndex = msg.ReadRangedInteger(-1, orderPrefab.AllOptions.Length);
								if (optionIndex > -1)
								{
									option = orderPrefab.AllOptions[optionIndex];
								}
							}
							GameSession gameSession2 = GameMain.GameSession;
							if (gameSession2 != null)
							{
								CrewManager crewManager = gameSession2.CrewManager;
								if (crewManager != null)
								{
									crewManager.SetOrderHighlight(this, orderPrefab.Identifier, option);
								}
							}
						}
						else if (msgType == AIObjectiveManager.ObjectiveType.Objective)
						{
							Identifier identifier = msg.ReadIdentifier();
							Identifier option2 = msg.ReadIdentifier();
							ushort objectiveTargetEntityId = msg.ReadUInt16();
							Entity objectiveTargetEntity = Entity.FindEntityByID(objectiveTargetEntityId);
							GameSession gameSession3 = GameMain.GameSession;
							if (gameSession3 != null)
							{
								CrewManager crewManager2 = gameSession3.CrewManager;
								if (crewManager2 != null)
								{
									crewManager2.CreateObjectiveIcon(this, identifier, option2, objectiveTargetEntity);
								}
							}
						}
					}
				}
				break;
			}
			case Character.EventType.TeamChange:
			{
				byte newTeamId = msg.ReadByte();
				this.ChangeTeam((CharacterTeamType)newTeamId);
				break;
			}
			case Character.EventType.AddToCrew:
				GameMain.GameSession.CrewManager.AddCharacter(this);
				Character.<ClientEventRead>g__ReadItemTeamChange|113_0(msg, true);
				break;
			case Character.EventType.UpdateExperience:
			{
				int experienceAmount = msg.ReadInt32();
				int additionalTalentPoints = msg.ReadInt32();
				if (this.info != null)
				{
					this.info.SetExperience(experienceAmount);
					this.info.AdditionalTalentPoints = additionalTalentPoints;
				}
				break;
			}
			case Character.EventType.UpdateTalents:
			{
				ushort talentCount = msg.ReadUInt16();
				for (int j = 0; j < (int)talentCount; j++)
				{
					bool addedThisRound = msg.ReadBoolean();
					uint talentIdentifier = msg.ReadUInt32();
					this.GiveTalent(talentIdentifier, addedThisRound);
				}
				break;
			}
			case Character.EventType.UpdateSkills:
			{
				Identifier skillIdentifier = msg.ReadIdentifier();
				if (!skillIdentifier.IsEmpty)
				{
					bool forceNotification = msg.ReadBoolean();
					float skillLevel = msg.ReadSingle();
					CharacterInfo characterInfo = this.info;
					if (characterInfo != null)
					{
						characterInfo.SetSkillLevel(skillIdentifier, skillLevel, forceNotification);
					}
				}
				break;
			}
			case Character.EventType.UpdateMoney:
			{
				int moneyAmount = msg.ReadInt32();
				this.SetMoney(moneyAmount);
				break;
			}
			case Character.EventType.UpdatePermanentStats:
			{
				byte savedStatValueCount = msg.ReadByte();
				StatTypes statType = (StatTypes)msg.ReadByte();
				CharacterInfo characterInfo2 = this.info;
				if (characterInfo2 != null)
				{
					characterInfo2.ClearSavedStatValues(statType);
				}
				for (int k = 0; k < (int)savedStatValueCount; k++)
				{
					Identifier statIdentifier = msg.ReadIdentifier();
					float statValue = msg.ReadSingle();
					bool removeOnDeath = msg.ReadBoolean();
					CharacterInfo characterInfo3 = this.info;
					if (characterInfo3 != null)
					{
						characterInfo3.ChangeSavedStatValue(statType, statValue, statIdentifier, removeOnDeath, float.MaxValue, true);
					}
				}
				break;
			}
			case Character.EventType.RemoveFromCrew:
				GameMain.GameSession.CrewManager.RemoveCharacter(this, true, true);
				Character.<ClientEventRead>g__ReadItemTeamChange|113_0(msg, false);
				break;
			case Character.EventType.LatchOntoTarget:
			{
				bool attached = msg.ReadBoolean();
				if (attached)
				{
					Vector2 characterSimPos = new Vector2(msg.ReadSingle(), msg.ReadSingle());
					Vector2 attachSurfaceNormal = new Vector2(msg.ReadSingle(), msg.ReadSingle());
					Vector2 attachPos = new Vector2(msg.ReadSingle(), msg.ReadSingle());
					int attachWallIndex = msg.ReadInt32();
					ushort attachTargetId = msg.ReadUInt16();
					EnemyAIController enemyAIController = this.AIController as EnemyAIController;
					if (enemyAIController != null)
					{
						LatchOntoAI latchOntoAi = enemyAIController.LatchOntoAI;
						if (latchOntoAi != null)
						{
							Entity attachTargetEntity = Entity.FindEntityByID(attachTargetId);
							Character attachTargetCharacter = attachTargetEntity as Character;
							if (attachTargetCharacter == null)
							{
								Structure attachTargetStructure = attachTargetEntity as Structure;
								if (attachTargetStructure == null)
								{
									List<VoronoiCell> allLevelWalls = Level.Loaded.GetAllCells();
									if (attachWallIndex >= 0 && attachWallIndex <= allLevelWalls.Count)
									{
										latchOntoAi.SetAttachTarget(allLevelWalls[attachWallIndex]);
									}
								}
								else
								{
									latchOntoAi.SetAttachTarget(attachTargetStructure, attachPos, attachSurfaceNormal);
								}
							}
							else
							{
								latchOntoAi.SetAttachTarget(attachTargetCharacter);
							}
							latchOntoAi.AttachToBody(attachPos, new Vector2?(attachSurfaceNormal), new Vector2?(characterSimPos));
						}
					}
				}
				else
				{
					EnemyAIController enemyAIController = this.AIController as EnemyAIController;
					if (enemyAIController != null)
					{
						LatchOntoAI latchOntoAi2 = enemyAIController.LatchOntoAI;
						if (latchOntoAi2 != null)
						{
							latchOntoAi2.DeattachFromBody(false, 0f);
						}
					}
				}
				break;
			}
			case Character.EventType.UpdateTalentRefundPoints:
			{
				int refundPoints = msg.ReadInt32();
				if (this.info != null)
				{
					if (refundPoints > this.info.TalentRefundPoints)
					{
						this.info.ShowTalentResetPopupOnOpen = true;
					}
					this.info.TalentRefundPoints = refundPoints;
				}
				break;
			}
			case Character.EventType.ConfirmTalentRefund:
			{
				CharacterInfo characterInfo4 = this.Info;
				if (characterInfo4 != null)
				{
					characterInfo4.RefundTalents();
				}
				break;
			}
			}
			msg.ReadPadBits();
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x000285E0 File Offset: 0x000267E0
		public static Character ReadSpawnData(IReadMessage inc)
		{
			DebugConsole.Log("Reading character spawn data");
			if (GameMain.Client == null)
			{
				return null;
			}
			bool noInfo = inc.ReadBoolean();
			ushort id = inc.ReadUInt16();
			string speciesName = inc.ReadString();
			string seed = inc.ReadString();
			Vector2 position = new Vector2(inc.ReadSingle(), inc.ReadSingle());
			bool enabled = inc.ReadBoolean();
			bool disabledByEvent = inc.ReadBoolean();
			DebugConsole.Log("Received spawn data for " + speciesName);
			Character character = null;
			if (noInfo)
			{
				try
				{
					character = Character.Create(speciesName, position, seed, null, id, false, true, true, null, true, true);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to spawn character " + speciesName, e, null, false, false);
					throw;
				}
				bool containsStatusData = inc.ReadBoolean();
				if (containsStatusData)
				{
					character.ReadStatus(inc);
				}
			}
			else
			{
				int ownerId = inc.ReadBoolean() ? ((int)inc.ReadByte()) : -1;
				float humanPrefabHealthMultiplier = inc.ReadSingle();
				int balance = inc.ReadInt32();
				int rewardDistribution = inc.ReadRangedInteger(0, 100);
				byte teamID = inc.ReadByte();
				bool hasAi = inc.ReadBoolean();
				Identifier infoSpeciesName = inc.ReadIdentifier();
				CharacterInfo info = CharacterInfo.ClientRead(infoSpeciesName, inc, true);
				try
				{
					character = Character.Create(speciesName, position, seed, info, id, ownerId > 0 && (int)GameMain.Client.SessionId != ownerId, hasAi, true, null, true, true);
				}
				catch (Exception e2)
				{
					DebugConsole.ThrowError("Failed to spawn character " + speciesName, e2, null, false, false);
					throw;
				}
				character.TeamID = (CharacterTeamType)teamID;
				character.CampaignInteractionType = (CampaignMode.InteractionType)inc.ReadByte();
				if (character.CampaignInteractionType == CampaignMode.InteractionType.Store)
				{
					character.MerchantIdentifier = inc.ReadIdentifier();
				}
				character.Faction = inc.ReadIdentifier();
				character.HumanPrefabHealthMultiplier = humanPrefabHealthMultiplier;
				character.Wallet.Balance = balance;
				character.Wallet.RewardDistribution = rewardDistribution;
				if (character.CampaignInteractionType != CampaignMode.InteractionType.None)
				{
					CampaignMode campaignMode = GameMain.GameSession.GameMode as CampaignMode;
					if (campaignMode != null)
					{
						campaignMode.AssignNPCMenuInteraction(character, character.CampaignInteractionType);
					}
				}
				int orderCount = (int)inc.ReadByte();
				for (int i = 0; i < orderCount; i++)
				{
					uint orderPrefabUintIdentifier = inc.ReadUInt32();
					Entity targetEntity = Entity.FindEntityByID(inc.ReadUInt16());
					Character orderGiver = inc.ReadBoolean() ? (Entity.FindEntityByID(inc.ReadUInt16()) as Character) : null;
					int orderOptionIndex = (int)inc.ReadByte();
					int orderPriority = (int)inc.ReadByte();
					OrderTarget targetPosition = null;
					if (inc.ReadBoolean())
					{
						float x = inc.ReadSingle();
						float y = inc.ReadSingle();
						Hull hull = Entity.FindEntityByID(inc.ReadUInt16()) as Hull;
						targetPosition = new OrderTarget(new Vector2(x, y), hull, true);
					}
					OrderPrefab orderPrefab = OrderPrefab.Prefabs.Find((OrderPrefab p) => p.UintIdentifier == orderPrefabUintIdentifier);
					if (orderPrefab != null)
					{
						ItemComponent component = orderPrefab.GetTargetItemComponent(targetEntity as Item);
						if (!orderPrefab.MustSetTarget || (targetEntity != null && component != null) || targetPosition != null)
						{
							Order order = (targetPosition == null) ? new Order(orderPrefab, targetEntity, component, orderGiver, false) : new Order(orderPrefab, targetPosition, orderGiver);
							order = order.WithOption((orderOptionIndex >= 0 && orderOptionIndex < orderPrefab.Options.Length) ? orderPrefab.Options[orderOptionIndex] : Identifier.Empty).WithManualPriority(orderPriority).WithOrderGiver(orderGiver);
							character.SetOrder(order, true, false, true);
						}
						else
						{
							DebugConsole.AddSafeError(string.Concat(new string[]
							{
								"Could not set order \"",
								orderPrefab.Identifier.ToString(),
								"\" for character \"",
								character.Name,
								"\" because required target entity was not found."
							}));
						}
					}
					else
					{
						DebugConsole.ThrowError("Invalid order prefab index - index (" + orderPrefabUintIdentifier.ToString() + ") out of bounds.", null, null, false, false);
					}
				}
				bool containsStatusData2 = inc.ReadBoolean();
				if (containsStatusData2)
				{
					character.ReadStatus(inc);
				}
				if (character.IsHuman && character.TeamID != CharacterTeamType.FriendlyNPC && character.TeamID != CharacterTeamType.None)
				{
					CharacterInfo duplicateCharacterInfo = GameMain.GameSession.CrewManager.GetCharacterInfos(true).FirstOrDefault((CharacterInfo c) => c.ID == info.ID);
					GameMain.GameSession.CrewManager.RemoveCharacterInfo(duplicateCharacterInfo);
					if (character.isDead)
					{
						GameMain.GameSession.CrewManager.AddCharacterInfo(character.info);
					}
					else
					{
						GameMain.GameSession.CrewManager.AddCharacter(character);
					}
				}
				if ((int)GameMain.Client.SessionId == ownerId)
				{
					GameMain.Client.HasSpawned = true;
					GameMain.Client.Character = character;
					if (!character.IsDead)
					{
						Character.Controlled = character;
					}
					GameMain.LightManager.LosEnabled = true;
					GameMain.LightManager.LosAlpha = 1f;
					GameMain.NetLobbyScreen.CampaignCharacterDiscarded = false;
					character.memInput.Clear();
					character.memState.Clear();
					character.memLocalState.Clear();
				}
			}
			if (disabledByEvent)
			{
				character.DisabledByEvent = true;
			}
			else
			{
				character.Enabled = (Character.Controlled == character || enabled);
			}
			return character;
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00028AFC File Offset: 0x00026CFC
		private void ReadStatus(IReadMessage msg)
		{
			bool isDead = msg.ReadBoolean();
			if (isDead)
			{
				CauseOfDeathType causeOfDeathType = (CauseOfDeathType)msg.ReadRangedInteger(0, Enum.GetValues(typeof(CauseOfDeathType)).Length - 1);
				AfflictionPrefab causeOfDeathAffliction = null;
				if (causeOfDeathType == CauseOfDeathType.Affliction)
				{
					uint afflictionId = msg.ReadUInt32();
					AfflictionPrefab afflictionPrefab = AfflictionPrefab.Prefabs.Find((AfflictionPrefab p) => p.UintIdentifier == afflictionId);
					if (afflictionPrefab == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error in CharacterNetworking.ReadStatus: affliction not found (id ");
						defaultInterpolatedStringHandler.AppendFormatted<uint>(afflictionId);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
						causeOfDeathType = CauseOfDeathType.Unknown;
						GameAnalyticsManager.AddErrorEventOnce("CharacterNetworking.ReadStatus:AfflictionNotFound", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					}
					else
					{
						causeOfDeathAffliction = afflictionPrefab;
					}
				}
				Character killer = Entity.FindEntityByID(msg.ReadUInt16()) as Character;
				bool containsAfflictionData = msg.ReadBoolean();
				if (!this.IsDead)
				{
					if (causeOfDeathType == CauseOfDeathType.Pressure || causeOfDeathAffliction == AfflictionPrefab.Pressure)
					{
						this.Implode(true);
					}
					else
					{
						this.Kill(causeOfDeathType, (causeOfDeathAffliction != null) ? causeOfDeathAffliction.Instantiate(1f, killer) : null, true, true);
					}
				}
				if (containsAfflictionData)
				{
					this.CharacterHealth.ClientRead(msg);
					this.CharacterHealth.ForceUpdateVisuals();
				}
			}
			else
			{
				if (this.IsDead)
				{
					this.Revive(true, false);
				}
				this.CharacterHealth.ClientRead(msg);
			}
			byte severedLimbCount = msg.ReadByte();
			for (int i = 0; i < (int)severedLimbCount; i++)
			{
				int severedJointIndex = (int)msg.ReadByte();
				if (severedJointIndex < 0 || severedJointIndex >= this.AnimController.LimbJoints.Length)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(99, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in CharacterNetworking.ReadStatus: severed joint index out of bounds (index: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(severedJointIndex);
					defaultInterpolatedStringHandler2.AppendLiteral(", joint count: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(this.AnimController.LimbJoints.Length);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					string errorMsg2 = defaultInterpolatedStringHandler2.ToStringAndClear();
					GameAnalyticsManager.AddErrorEventOnce("CharacterNetworking.ReadStatus:JointIndexOutOfBounts", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
				}
				else
				{
					this.AnimController.SeverLimbJoint(this.AnimController.LimbJoints[severedJointIndex]);
				}
			}
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x06000472 RID: 1138 RVA: 0x00028CFC File Offset: 0x00026EFC
		public override ContentPackage ContentPackage
		{
			get
			{
				CharacterPrefab prefab = this.Prefab;
				if (prefab == null)
				{
					return null;
				}
				return prefab.ContentPackage;
			}
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x00028D0F File Offset: 0x00026F0F
		private void UpdateLimbLightSource(Limb limb)
		{
			if (limb.LightSource != null)
			{
				limb.LightSource.Enabled = this.enabled;
			}
		}

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000474 RID: 1140 RVA: 0x00028D2A File Offset: 0x00026F2A
		// (set) Token: 0x06000475 RID: 1141 RVA: 0x00028D40 File Offset: 0x00026F40
		public bool Enabled
		{
			get
			{
				return this.enabled && !base.Removed;
			}
			set
			{
				if (this.initialized && value == this.enabled)
				{
					return;
				}
				this.initialized = true;
				if (base.Removed)
				{
					this.enabled = false;
					return;
				}
				this.enabled = value;
				foreach (Limb limb in this.AnimController.Limbs)
				{
					if (!limb.IsSevered)
					{
						if (limb.body != null)
						{
							limb.body.Enabled = this.enabled;
						}
						this.UpdateLimbLightSource(limb);
					}
				}
				foreach (Item item in this.HeldItems)
				{
					if (item.body != null)
					{
						if (!this.enabled)
						{
							item.body.Enabled = false;
						}
						else
						{
							Holdable component = item.GetComponent<Holdable>();
							if (component != null && component.IsActive)
							{
								item.body.Enabled = true;
							}
						}
					}
				}
				this.AnimController.Collider.Enabled = value;
			}
		}

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000476 RID: 1142 RVA: 0x00028E54 File Offset: 0x00027054
		// (set) Token: 0x06000477 RID: 1143 RVA: 0x00028E5C File Offset: 0x0002705C
		public bool DisabledByEvent
		{
			get
			{
				return this.disabledByEvent;
			}
			set
			{
				if (value == this.disabledByEvent)
				{
					return;
				}
				this.disabledByEvent = value;
				if (this.disabledByEvent)
				{
					this.Enabled = false;
					Character.CharacterList.Remove(this);
					if (base.AiTarget != null)
					{
						AITarget.List.Remove(base.AiTarget);
					}
				}
				else
				{
					if (!Character.CharacterList.Contains(this))
					{
						Character.CharacterList.Add(this);
					}
					if (base.AiTarget != null && !AITarget.List.Contains(base.AiTarget))
					{
						AITarget.List.Add(base.AiTarget);
					}
				}
				if (this.Inventory != null)
				{
					foreach (Item item in this.Inventory.FindAllItems(null, true, null))
					{
						item.IsActive = !this.disabledByEvent;
					}
				}
			}
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000478 RID: 1144 RVA: 0x00028F50 File Offset: 0x00027150
		public bool IsRemotelyControlled
		{
			get
			{
				if (GameMain.NetworkMember == null)
				{
					return false;
				}
				if (GameMain.NetworkMember.IsClient)
				{
					return this != Character.Controlled;
				}
				return this.IsRemotePlayer;
			}
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x06000479 RID: 1145 RVA: 0x00028F79 File Offset: 0x00027179
		// (set) Token: 0x0600047A RID: 1146 RVA: 0x00028F81 File Offset: 0x00027181
		public bool IsRemotePlayer { get; set; }

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x0600047B RID: 1147 RVA: 0x00028F8A File Offset: 0x0002718A
		public bool IsLocalPlayer
		{
			get
			{
				return Character.Controlled == this;
			}
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x0600047C RID: 1148 RVA: 0x00028F94 File Offset: 0x00027194
		public bool IsPlayer
		{
			get
			{
				return this.IsLocalPlayer || this.IsRemotePlayer;
			}
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x0600047D RID: 1149 RVA: 0x00028FA8 File Offset: 0x000271A8
		public bool IsCommanding
		{
			get
			{
				if (!this.IsPlayer)
				{
					HumanAIController humanAIController = this.AIController as HumanAIController;
					if (humanAIController != null)
					{
						ShipCommandManager shipCommandManager = humanAIController.ShipCommandManager;
						if (shipCommandManager != null)
						{
							return shipCommandManager.Active;
						}
					}
					return false;
				}
				return true;
			}
		}

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x0600047E RID: 1150 RVA: 0x00028FE0 File Offset: 0x000271E0
		public bool IsBot
		{
			get
			{
				if (!this.IsPlayer)
				{
					AIController aicontroller = this.AIController;
					return aicontroller is HumanAIController && aicontroller.Enabled;
				}
				return false;
			}
		}

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x0600047F RID: 1151 RVA: 0x00029010 File Offset: 0x00027210
		public bool IsAIControlled
		{
			get
			{
				if (!this.IsPlayer)
				{
					AIController aicontroller = this.AIController;
					return aicontroller != null && aicontroller.Enabled;
				}
				return false;
			}
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000480 RID: 1152 RVA: 0x00029039 File Offset: 0x00027239
		// (set) Token: 0x06000481 RID: 1153 RVA: 0x00029041 File Offset: 0x00027241
		public bool IsEscorted { get; set; }

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x06000482 RID: 1154 RVA: 0x0002904C File Offset: 0x0002724C
		public Identifier JobIdentifier
		{
			get
			{
				CharacterInfo characterInfo = this.Info;
				Identifier? identifier;
				if (characterInfo == null)
				{
					identifier = null;
				}
				else
				{
					Job job = characterInfo.Job;
					identifier = ((job != null) ? new Identifier?(job.Prefab.Identifier) : null);
				}
				Identifier? identifier2 = identifier;
				if (identifier2 == null)
				{
					return Identifier.Empty;
				}
				return identifier2.GetValueOrDefault();
			}
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x06000483 RID: 1155 RVA: 0x000290A8 File Offset: 0x000272A8
		// (set) Token: 0x06000484 RID: 1156 RVA: 0x000290BA File Offset: 0x000272BA
		public bool DoesBleed
		{
			get
			{
				return this.Params.Health.DoesBleed;
			}
			set
			{
				this.Params.Health.DoesBleed = value;
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000485 RID: 1157 RVA: 0x000290CD File Offset: 0x000272CD
		// (set) Token: 0x06000486 RID: 1158 RVA: 0x000290D5 File Offset: 0x000272D5
		public bool IsContainable { get; set; }

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x06000487 RID: 1159 RVA: 0x000290DE File Offset: 0x000272DE
		public Dictionary<Identifier, SerializableProperty> SerializableProperties
		{
			get
			{
				return this.Properties;
			}
		}

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x06000488 RID: 1160 RVA: 0x000290E6 File Offset: 0x000272E6
		public Key[] Keys
		{
			get
			{
				return this.keys;
			}
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000489 RID: 1161 RVA: 0x000290EE File Offset: 0x000272EE
		// (set) Token: 0x0600048A RID: 1162 RVA: 0x000290F8 File Offset: 0x000272F8
		public HumanPrefab HumanPrefab
		{
			get
			{
				return this.humanPrefab;
			}
			set
			{
				if (this.humanPrefab == value)
				{
					return;
				}
				this.humanPrefab = value;
				if (this.humanPrefab != null)
				{
					this.HumanPrefabHealthMultiplier = this.humanPrefab.HealthMultiplier;
					if (GameMain.NetworkMember != null)
					{
						this.HumanPrefabHealthMultiplier *= this.humanPrefab.HealthMultiplierInMultiplayer;
						return;
					}
				}
				else
				{
					this.HumanPrefabHealthMultiplier = 1f;
				}
			}
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x0600048B RID: 1163 RVA: 0x0002915C File Offset: 0x0002735C
		// (set) Token: 0x0600048C RID: 1164 RVA: 0x00029196 File Offset: 0x00027396
		public Identifier Faction
		{
			get
			{
				Identifier? identifier = this.faction;
				if (identifier != null)
				{
					return identifier.GetValueOrDefault();
				}
				HumanPrefab humanPrefab = this.HumanPrefab;
				if (humanPrefab == null)
				{
					return Identifier.Empty;
				}
				return humanPrefab.Faction;
			}
			set
			{
				this.faction = new Identifier?(value);
			}
		}

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x0600048D RID: 1165 RVA: 0x000291A4 File Offset: 0x000273A4
		// (set) Token: 0x0600048E RID: 1166 RVA: 0x000291AC File Offset: 0x000273AC
		public CharacterTeamType TeamID
		{
			get
			{
				return this.teamID;
			}
			set
			{
				this.teamID = value;
				if (this.info != null)
				{
					this.info.TeamID = value;
				}
			}
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x0600048F RID: 1167 RVA: 0x000291CC File Offset: 0x000273CC
		public CharacterTeamType OriginalTeamID
		{
			get
			{
				CharacterTeamType? characterTeamType = this.originalTeamID;
				if (characterTeamType == null)
				{
					return this.teamID;
				}
				return characterTeamType.GetValueOrDefault();
			}
		}

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x06000490 RID: 1168 RVA: 0x000291F7 File Offset: 0x000273F7
		// (set) Token: 0x06000491 RID: 1169 RVA: 0x00029205 File Offset: 0x00027405
		public Wallet Wallet
		{
			get
			{
				this.ThrowIfAccessingWalletsInSingleplayer();
				return this.wallet;
			}
			set
			{
				this.ThrowIfAccessingWalletsInSingleplayer();
				this.wallet = value;
			}
		}

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x06000492 RID: 1170 RVA: 0x00029214 File Offset: 0x00027414
		// (set) Token: 0x06000493 RID: 1171 RVA: 0x0002921C File Offset: 0x0002741C
		public bool AllowPlayDead { get; set; }

		// Token: 0x06000494 RID: 1172 RVA: 0x00029228 File Offset: 0x00027428
		public void EvaluatePlayDeadProbability(float? probability = null)
		{
			CharacterParams.AIParams aiParams = this.Params.AI;
			if (aiParams != null)
			{
				if (probability != null)
				{
					aiParams.PlayDeadProbability = probability.Value;
				}
				this.AllowPlayDead = (Rand.Value(Rand.RandSync.Unsynced) <= aiParams.PlayDeadProbability);
				return;
			}
			if (probability != null)
			{
				this.AllowPlayDead = (Rand.Value(Rand.RandSync.Unsynced) <= probability.Value);
			}
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00029293 File Offset: 0x00027493
		private void ThrowIfAccessingWalletsInSingleplayer()
		{
			if ((GameMain.NetworkMember == null || GameMain.IsSingleplayer) && this.IsPlayer)
			{
				throw new InvalidOperationException("Tried to access crew wallets in singleplayer. Use CampaignMode.Bank or CampaignMode.GetWallet instead.");
			}
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x000292B6 File Offset: 0x000274B6
		public void SetOriginalTeamAndChangeTeam(CharacterTeamType newTeam, bool processImmediately = false)
		{
			this.TryRemoveTeamChange("original");
			this.currentTeamChange = new ActiveTeamChange(newTeam, ActiveTeamChange.TeamChangePriorities.Base, false);
			this.TryAddNewTeamChange("original", this.currentTeamChange);
			if (processImmediately)
			{
				this.UpdateTeam();
			}
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x000292F0 File Offset: 0x000274F0
		private void ChangeTeam(CharacterTeamType newTeam)
		{
			if (newTeam == this.teamID)
			{
				return;
			}
			CharacterTeamType valueOrDefault = this.originalTeamID.GetValueOrDefault();
			if (this.originalTeamID == null)
			{
				valueOrDefault = this.teamID;
				this.originalTeamID = new CharacterTeamType?(valueOrDefault);
			}
			this.TeamID = newTeam;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.AIController is HumanAIController)
			{
				Order order = OrderPrefab.Dismissal.CreateInstance(OrderPrefab.OrderTargetType.Entity, this, false).WithManualPriority(CharacterInfo.HighestManualOrderPriority);
				this.SetOrder(order, true, false, false);
			}
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x0002937D File Offset: 0x0002757D
		public bool HasTeamChange(string identifier)
		{
			return this.activeTeamChanges.ContainsKey(identifier);
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x0002938C File Offset: 0x0002758C
		public bool TryAddNewTeamChange(string identifier, ActiveTeamChange newTeamChange)
		{
			bool success = this.activeTeamChanges.TryAdd(identifier, newTeamChange);
			if (success && this.currentTeamChange == null)
			{
				this.SetOriginalTeamAndChangeTeam(this.TeamID, false);
			}
			return success;
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x000293C0 File Offset: 0x000275C0
		public bool TryRemoveTeamChange(string identifier)
		{
			ActiveTeamChange removedTeamChange;
			if (this.activeTeamChanges.TryGetValue(identifier, out removedTeamChange) && this.currentTeamChange == removedTeamChange)
			{
				this.currentTeamChange = this.activeTeamChanges["original"];
			}
			return this.activeTeamChanges.Remove(identifier);
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x00029408 File Offset: 0x00027608
		public void UpdateTeam()
		{
			if (this.currentTeamChange == null)
			{
				return;
			}
			ActiveTeamChange bestTeamChange = this.currentTeamChange;
			foreach (KeyValuePair<string, ActiveTeamChange> desiredTeamChange in this.activeTeamChanges)
			{
				if (bestTeamChange.TeamChangePriority < desiredTeamChange.Value.TeamChangePriority)
				{
					bestTeamChange = desiredTeamChange.Value;
				}
			}
			if (this.TeamID != bestTeamChange.DesiredTeamId)
			{
				this.ChangeTeam(bestTeamChange.DesiredTeamId);
				this.currentTeamChange = bestTeamChange;
				if (bestTeamChange.AggressiveBehavior && this.AIController is HumanAIController)
				{
					Order order = OrderPrefab.Prefabs["fightintruders"].CreateInstance(OrderPrefab.OrderTargetType.Entity, this, false).WithManualPriority(CharacterInfo.HighestManualOrderPriority);
					this.SetOrder(order, true, false, false);
				}
			}
		}

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x0600049C RID: 1180 RVA: 0x000294E4 File Offset: 0x000276E4
		public bool IsOnPlayerTeam
		{
			get
			{
				return this.teamID == CharacterTeamType.Team1 || (this.teamID == CharacterTeamType.Team2 && !this.IsFriendlyNPCTurnedHostile);
			}
		}

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x0600049D RID: 1181 RVA: 0x00029508 File Offset: 0x00027708
		public bool IsOriginallyOnPlayerTeam
		{
			get
			{
				CharacterTeamType? characterTeamType = this.originalTeamID;
				if (characterTeamType != null)
				{
					CharacterTeamType valueOrDefault = characterTeamType.GetValueOrDefault();
					if (valueOrDefault - CharacterTeamType.Team1 <= 1)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x0600049E RID: 1182 RVA: 0x0002953C File Offset: 0x0002773C
		public bool IsFriendlyNPCTurnedHostile
		{
			get
			{
				bool flag = this.originalTeamID.GetValueOrDefault() == CharacterTeamType.FriendlyNPC;
				bool flag2 = flag;
				if (flag2)
				{
					CharacterTeamType characterTeamType = this.teamID;
					bool flag3 = characterTeamType == CharacterTeamType.None || characterTeamType == CharacterTeamType.Team2;
					flag2 = flag3;
				}
				return flag2;
			}
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x0600049F RID: 1183 RVA: 0x00029574 File Offset: 0x00027774
		public bool IsInstigator
		{
			get
			{
				CombatAction combatAction = this.CombatAction;
				return combatAction != null && combatAction.IsInstigator;
			}
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x060004A0 RID: 1184 RVA: 0x00029593 File Offset: 0x00027793
		public IEnumerable<Character.Attacker> LastAttackers
		{
			get
			{
				return this.lastAttackers;
			}
		}

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x060004A1 RID: 1185 RVA: 0x0002959B File Offset: 0x0002779B
		public Character LastAttacker
		{
			get
			{
				Character.Attacker attacker = this.lastAttackers.LastOrDefault<Character.Attacker>();
				if (attacker == null)
				{
					return null;
				}
				return attacker.Character;
			}
		}

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x060004A2 RID: 1186 RVA: 0x000295B3 File Offset: 0x000277B3
		// (set) Token: 0x060004A3 RID: 1187 RVA: 0x000295BB File Offset: 0x000277BB
		public Character LastOrderedCharacter { get; private set; }

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x060004A4 RID: 1188 RVA: 0x000295C4 File Offset: 0x000277C4
		// (set) Token: 0x060004A5 RID: 1189 RVA: 0x000295CC File Offset: 0x000277CC
		public Character SecondLastOrderedCharacter { get; private set; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x060004A6 RID: 1190 RVA: 0x000295D5 File Offset: 0x000277D5
		public Dictionary<ItemPrefab, double> ItemSelectedDurations
		{
			get
			{
				return this.itemSelectedDurations;
			}
		}

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x060004A7 RID: 1191 RVA: 0x000295DD File Offset: 0x000277DD
		// (set) Token: 0x060004A8 RID: 1192 RVA: 0x000295E5 File Offset: 0x000277E5
		public float InvisibleTimer { get; set; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x060004A9 RID: 1193 RVA: 0x000295EE File Offset: 0x000277EE
		public Identifier SpeciesName
		{
			get
			{
				CharacterParams @params = this.Params;
				if (@params == null)
				{
					return "null".ToIdentifier();
				}
				return @params.SpeciesName;
			}
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x0002960A File Offset: 0x0002780A
		public Identifier GetBaseCharacterSpeciesName()
		{
			return this.Prefab.GetBaseCharacterSpeciesName(this.SpeciesName);
		}

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x060004AB RID: 1195 RVA: 0x00029620 File Offset: 0x00027820
		public Identifier Group
		{
			get
			{
				HumanPrefab prefab = this.HumanPrefab;
				if (prefab == null || prefab.Group.IsEmpty)
				{
					return this.Params.Group;
				}
				return prefab.Group;
			}
		}

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x060004AC RID: 1196 RVA: 0x00029659 File Offset: 0x00027859
		public bool IsHumanoid
		{
			get
			{
				return this.Params.Humanoid;
			}
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x060004AD RID: 1197 RVA: 0x00029666 File Offset: 0x00027866
		public bool IsMachine
		{
			get
			{
				return this.Params.IsMachine;
			}
		}

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x060004AE RID: 1198 RVA: 0x00029673 File Offset: 0x00027873
		public bool IsHusk
		{
			get
			{
				return this.Params.Husk;
			}
		}

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x060004AF RID: 1199 RVA: 0x00029680 File Offset: 0x00027880
		public bool IsDisguisedAsHusk
		{
			get
			{
				return this.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.DisguisedAsHuskType, true) > 0f;
			}
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x060004B0 RID: 1200 RVA: 0x0002969A File Offset: 0x0002789A
		public bool IsHuskInfected
		{
			get
			{
				return this.CharacterHealth.GetActiveAfflictionTags().Contains(Tags.HuskInfected);
			}
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x060004B1 RID: 1201 RVA: 0x000296B1 File Offset: 0x000278B1
		public bool IsMale
		{
			get
			{
				CharacterInfo characterInfo = this.info;
				return characterInfo != null && characterInfo.IsMale;
			}
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x060004B2 RID: 1202 RVA: 0x000296C4 File Offset: 0x000278C4
		public bool IsFemale
		{
			get
			{
				CharacterInfo characterInfo = this.info;
				return characterInfo != null && characterInfo.IsFemale;
			}
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x060004B3 RID: 1203 RVA: 0x000296D7 File Offset: 0x000278D7
		public string BloodDecalName
		{
			get
			{
				return this.Params.BloodDecal;
			}
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x060004B4 RID: 1204 RVA: 0x000296E4 File Offset: 0x000278E4
		// (set) Token: 0x060004B5 RID: 1205 RVA: 0x000296F1 File Offset: 0x000278F1
		public bool CanSpeak
		{
			get
			{
				return this.Params.CanSpeak;
			}
			set
			{
				this.Params.CanSpeak = value;
			}
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x060004B6 RID: 1206 RVA: 0x000296FF File Offset: 0x000278FF
		// (set) Token: 0x060004B7 RID: 1207 RVA: 0x0002970C File Offset: 0x0002790C
		public bool NeedsAir
		{
			get
			{
				return this.Params.NeedsAir;
			}
			set
			{
				this.Params.NeedsAir = value;
			}
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x060004B8 RID: 1208 RVA: 0x0002971A File Offset: 0x0002791A
		// (set) Token: 0x060004B9 RID: 1209 RVA: 0x00029727 File Offset: 0x00027927
		public bool NeedsWater
		{
			get
			{
				return this.Params.NeedsWater;
			}
			set
			{
				this.Params.NeedsWater = value;
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x060004BA RID: 1210 RVA: 0x00029735 File Offset: 0x00027935
		public bool NeedsOxygen
		{
			get
			{
				return this.NeedsAir || (this.NeedsWater && !this.AnimController.InWater);
			}
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x060004BB RID: 1211 RVA: 0x00029759 File Offset: 0x00027959
		// (set) Token: 0x060004BC RID: 1212 RVA: 0x00029766 File Offset: 0x00027966
		public float Noise
		{
			get
			{
				return this.Params.Noise;
			}
			set
			{
				this.Params.Noise = value;
			}
		}

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x060004BD RID: 1213 RVA: 0x00029774 File Offset: 0x00027974
		// (set) Token: 0x060004BE RID: 1214 RVA: 0x00029781 File Offset: 0x00027981
		public float Visibility
		{
			get
			{
				return this.Params.Visibility;
			}
			set
			{
				this.Params.Visibility = value;
			}
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x060004BF RID: 1215 RVA: 0x0002978F File Offset: 0x0002798F
		// (set) Token: 0x060004C0 RID: 1216 RVA: 0x000297AB File Offset: 0x000279AB
		public float MaxPerceptionDistance
		{
			get
			{
				CharacterParams.AIParams ai = this.Params.AI;
				if (ai == null)
				{
					return 0f;
				}
				return ai.MaxPerceptionDistance;
			}
			set
			{
				if (this.Params.AI != null)
				{
					this.Params.AI.MaxPerceptionDistance = value;
				}
			}
		}

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x060004C1 RID: 1217 RVA: 0x000297CB File Offset: 0x000279CB
		// (set) Token: 0x060004C2 RID: 1218 RVA: 0x000297D3 File Offset: 0x000279D3
		public bool IsTraitor { get; set; }

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x060004C3 RID: 1219 RVA: 0x000297DC File Offset: 0x000279DC
		public bool IsHuman
		{
			get
			{
				Identifier speciesName = this.SpeciesName;
				return speciesName == CharacterPrefab.HumanSpeciesName;
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x060004C4 RID: 1220 RVA: 0x000297FC File Offset: 0x000279FC
		public List<Order> CurrentOrders
		{
			get
			{
				CharacterInfo characterInfo = this.Info;
				if (characterInfo == null)
				{
					return null;
				}
				return characterInfo.CurrentOrders;
			}
		}

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x060004C5 RID: 1221 RVA: 0x0002980F File Offset: 0x00027A0F
		public bool IsDismissed
		{
			get
			{
				return this.GetCurrentOrderWithTopPriority() == null;
			}
		}

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x060004C6 RID: 1222 RVA: 0x0002981A File Offset: 0x00027A1A
		// (set) Token: 0x060004C7 RID: 1223 RVA: 0x00029822 File Offset: 0x00027A22
		public Entity ViewTarget { get; set; }

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x060004C8 RID: 1224 RVA: 0x0002982C File Offset: 0x00027A2C
		public Vector2 AimRefPosition
		{
			get
			{
				if (this.ViewTarget == null)
				{
					return this.AnimController.AimSourcePos;
				}
				Vector2 viewTargetWorldPos = this.ViewTarget.WorldPosition;
				Item targetItem = this.ViewTarget as Item;
				if (targetItem != null)
				{
					Turret turret = targetItem.GetComponent<Turret>();
					if (turret != null)
					{
						viewTargetWorldPos = new Vector2((float)targetItem.WorldRect.X + turret.TransformedBarrelPos.X, (float)targetItem.WorldRect.Y - turret.TransformedBarrelPos.Y);
					}
				}
				return this.Position + (viewTargetWorldPos - this.WorldPosition);
			}
		}

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x060004C9 RID: 1225 RVA: 0x000298C0 File Offset: 0x00027AC0
		// (set) Token: 0x060004CA RID: 1226 RVA: 0x000298C8 File Offset: 0x00027AC8
		public CharacterInfo Info
		{
			get
			{
				return this.info;
			}
			set
			{
				if (this.info != null && this.info != value)
				{
					this.info.Remove();
				}
				this.info = value;
				if (this.info != null)
				{
					this.info.Character = this;
				}
			}
		}

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x060004CB RID: 1227 RVA: 0x00029901 File Offset: 0x00027B01
		public Identifier VariantOf
		{
			get
			{
				return this.Prefab.VariantOf;
			}
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x060004CC RID: 1228 RVA: 0x00029910 File Offset: 0x00027B10
		public string Name
		{
			get
			{
				if (this.info == null || string.IsNullOrWhiteSpace(this.info.Name))
				{
					return this.SpeciesName.Value;
				}
				return this.info.Name;
			}
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x060004CD RID: 1229 RVA: 0x00029954 File Offset: 0x00027B54
		public string DisplayName
		{
			get
			{
				if (this.IsPet)
				{
					EnemyAIController enemyAIController = this.AIController as EnemyAIController;
					if (enemyAIController != null)
					{
						PetBehavior petBehavior = enemyAIController.PetBehavior;
						if (petBehavior != null)
						{
							string petName = petBehavior.GetTagName();
							if (!string.IsNullOrEmpty(petName))
							{
								return petName;
							}
						}
					}
				}
				if (this.info != null && !string.IsNullOrWhiteSpace(this.info.Name))
				{
					return this.info.Name;
				}
				LocalizedString displayName = this.Params.DisplayName;
				if (displayName.IsNullOrWhiteSpace())
				{
					if (this.Params.SpeciesTranslationOverride.IsEmpty)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Character.");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.SpeciesName);
						displayName = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Character.");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Params.SpeciesTranslationOverride);
						displayName = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
				}
				if (!displayName.IsNullOrWhiteSpace())
				{
					return displayName.Value;
				}
				return this.Name;
			}
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x060004CE RID: 1230 RVA: 0x00029A64 File Offset: 0x00027C64
		public string LogName
		{
			get
			{
				if (GameMain.NetworkMember != null && !GameMain.NetworkMember.ServerSettings.AllowDisguises)
				{
					return this.Name;
				}
				if (this.info == null || string.IsNullOrWhiteSpace(this.info.Name))
				{
					return this.SpeciesName.Value;
				}
				return this.info.Name + ((this.info.DisplayName != this.info.Name) ? (" (as " + this.info.DisplayName + ")") : "");
			}
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x060004CF RID: 1231 RVA: 0x00029B07 File Offset: 0x00027D07
		// (set) Token: 0x060004D0 RID: 1232 RVA: 0x00029B18 File Offset: 0x00027D18
		public bool HideFace
		{
			get
			{
				return this.hideFaceTimer > 0f;
			}
			set
			{
				bool wasHidden = this.HideFace;
				this.hideFaceTimer = MathHelper.Clamp(this.hideFaceTimer + (value ? 1f : -0.5f), 0f, 10f);
				bool isHidden = this.HideFace;
				if (isHidden != wasHidden && this.info != null && this.info.IsDisguisedAsAnother != isHidden)
				{
					this.info.CheckDisguiseStatus(true, null);
				}
			}
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x060004D1 RID: 1233 RVA: 0x00029B85 File Offset: 0x00027D85
		public string ConfigPath
		{
			get
			{
				return this.Params.File.Path.Value;
			}
		}

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x00029B9C File Offset: 0x00027D9C
		public float Mass
		{
			get
			{
				return this.AnimController.Mass;
			}
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x060004D3 RID: 1235 RVA: 0x00029BA9 File Offset: 0x00027DA9
		// (set) Token: 0x060004D4 RID: 1236 RVA: 0x00029BB1 File Offset: 0x00027DB1
		public CharacterInventory Inventory { get; private set; }

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x060004D5 RID: 1237 RVA: 0x00029BBA File Offset: 0x00027DBA
		// (set) Token: 0x060004D6 RID: 1238 RVA: 0x00029BC2 File Offset: 0x00027DC2
		public bool DisableInteract { get; set; }

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x060004D7 RID: 1239 RVA: 0x00029BCB File Offset: 0x00027DCB
		// (set) Token: 0x060004D8 RID: 1240 RVA: 0x00029BD3 File Offset: 0x00027DD3
		public bool DisableFocusingOnEntities { get; set; }

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x060004D9 RID: 1241 RVA: 0x00029BDC File Offset: 0x00027DDC
		// (set) Token: 0x060004DA RID: 1242 RVA: 0x00029BE4 File Offset: 0x00027DE4
		public LocalizedString CustomInteractHUDText { get; private set; }

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x060004DB RID: 1243 RVA: 0x00029BF0 File Offset: 0x00027DF0
		public bool AllowCustomInteract
		{
			get
			{
				if (CampaignMode.HostileFactionDisablesInteraction(this.CampaignInteractionType))
				{
					HumanAIController humanAi = this.AIController as HumanAIController;
					if (humanAi != null && humanAi.IsInHostileFaction())
					{
						return false;
					}
				}
				return (!this.RequireConsciousnessForCustomInteract || (!this.IsIncapacitated && this.Stun <= 0f)) && !base.Removed;
			}
		}

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x060004DC RID: 1244 RVA: 0x00029C4C File Offset: 0x00027E4C
		public bool ShouldShowCustomInteractText
		{
			get
			{
				if (!this.CustomInteractHUDText.IsNullOrEmpty() && this.AllowCustomInteract)
				{
					HumanAIController humanAi = this.AIController as HumanAIController;
					return humanAi == null || humanAi.AllowCampaignInteraction();
				}
				return false;
			}
		}

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x060004DD RID: 1245 RVA: 0x00029C87 File Offset: 0x00027E87
		// (set) Token: 0x060004DE RID: 1246 RVA: 0x00029C96 File Offset: 0x00027E96
		public bool LockHands
		{
			get
			{
				return this.lockHandsTimer > 0f;
			}
			set
			{
				this.lockHandsTimer = MathHelper.Clamp(this.lockHandsTimer + (value ? 1f : -0.5f), 0f, 10f);
				if (value)
				{
					this.SelectedCharacter = null;
				}
				HintManager.OnHandcuffed(this);
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x060004DF RID: 1247 RVA: 0x00029CD3 File Offset: 0x00027ED3
		public bool AllowInput
		{
			get
			{
				return !base.Removed && !this.IsIncapacitated && this.Stun <= 0f;
			}
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x060004E0 RID: 1248 RVA: 0x00029CF7 File Offset: 0x00027EF7
		public bool CanMove
		{
			get
			{
				return (this.AnimController.InWater || this.AnimController.CanWalk) && this.AllowInput;
			}
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x060004E1 RID: 1249 RVA: 0x00029D20 File Offset: 0x00027F20
		public bool CanInteract
		{
			get
			{
				return this.AllowInput && this.Params.CanInteract && !this.LockHands;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x060004E2 RID: 1250 RVA: 0x00029D42 File Offset: 0x00027F42
		public bool CanEat
		{
			get
			{
				return !this.IsHumanoid && this.Params.CanEat && this.AllowInput && this.AnimController.GetLimb(LimbType.Head, true, false, false) != null;
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x060004E3 RID: 1251 RVA: 0x00029D76 File Offset: 0x00027F76
		public bool CanClimb
		{
			get
			{
				return this.Params.CanClimb && this.CanInteract;
			}
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060004E4 RID: 1252 RVA: 0x00029D8D File Offset: 0x00027F8D
		// (set) Token: 0x060004E5 RID: 1253 RVA: 0x00029D95 File Offset: 0x00027F95
		public Vector2 CursorPosition
		{
			get
			{
				return this.cursorPosition;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.cursorPosition = value;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x060004E6 RID: 1254 RVA: 0x00029DA7 File Offset: 0x00027FA7
		// (set) Token: 0x060004E7 RID: 1255 RVA: 0x00029DAF File Offset: 0x00027FAF
		public Vector2 SmoothedCursorPosition { get; private set; }

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x060004E8 RID: 1256 RVA: 0x00029DB8 File Offset: 0x00027FB8
		public Vector2 CursorWorldPosition
		{
			get
			{
				if (base.Submarine != null)
				{
					return this.cursorPosition + base.Submarine.Position;
				}
				return this.cursorPosition;
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060004E9 RID: 1257 RVA: 0x00029DDF File Offset: 0x00027FDF
		// (set) Token: 0x060004EA RID: 1258 RVA: 0x00029DE7 File Offset: 0x00027FE7
		public Character FocusedCharacter { get; set; }

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060004EB RID: 1259 RVA: 0x00029DF0 File Offset: 0x00027FF0
		// (set) Token: 0x060004EC RID: 1260 RVA: 0x00029DF8 File Offset: 0x00027FF8
		public Character SelectedCharacter
		{
			get
			{
				return this.selectedCharacter;
			}
			set
			{
				if (value == this.selectedCharacter)
				{
					return;
				}
				if (this.selectedCharacter != null)
				{
					this.selectedCharacter.selectedBy = null;
					foreach (Character otherCharacter in Character.CharacterList)
					{
						if (otherCharacter != this && otherCharacter.selectedCharacter == this.selectedCharacter)
						{
							this.selectedCharacter.selectedBy = otherCharacter;
							break;
						}
					}
				}
				CharacterHUD.RecreateHudTextsIfControlling(this);
				this.selectedCharacter = value;
				if (this.selectedCharacter != null)
				{
					this.selectedCharacter.selectedBy = this;
				}
				this.CharacterHealth.SetHealthBarVisibility(value == null);
				if (this.IsLocalPlayer && !GUI.IsUltrawide && GUI.IsHUDScaled)
				{
					if (value != null)
					{
						ChatBox.AutoHideChatBox();
					}
					else
					{
						ChatBox.ResetChatBoxOpenState();
					}
				}
				bool flag;
				if (!GameMain.IsSingleplayer)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					flag = (networkMember != null && networkMember.IsServer);
				}
				else
				{
					flag = true;
				}
				bool isServerOrSingleplayer = flag;
				this.CheckTalents(AbilityEffectType.OnLootCharacter, new AbilityCharacterLoot(value));
				if (this.IsPlayer && isServerOrSingleplayer && value != null && value.IsDead)
				{
					Wallet grabbedWallet = value.Wallet;
					if (grabbedWallet != null)
					{
						int balance = grabbedWallet.Balance;
						if (balance > 0)
						{
							SinglePlayerCampaign spCampaign = GameMain.GameSession.Campaign as SinglePlayerCampaign;
							if (spCampaign != null)
							{
								spCampaign.Bank.Give(balance);
							}
							grabbedWallet.Deduct(balance);
						}
					}
				}
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060004ED RID: 1261 RVA: 0x00029F5C File Offset: 0x0002815C
		// (set) Token: 0x060004EE RID: 1262 RVA: 0x00029F64 File Offset: 0x00028164
		public Character SelectedBy
		{
			get
			{
				return this.selectedBy;
			}
			set
			{
				if (this.selectedBy != null)
				{
					this.selectedBy.selectedCharacter = null;
				}
				this.selectedBy = value;
				if (this.selectedBy != null)
				{
					this.selectedBy.selectedCharacter = this;
				}
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060004EF RID: 1263 RVA: 0x00029F98 File Offset: 0x00028198
		public IEnumerable<Item> HeldItems
		{
			get
			{
				Character.<get_HeldItems>d__390 <get_HeldItems>d__ = new Character.<get_HeldItems>d__390(-2);
				<get_HeldItems>d__.<>4__this = this;
				return <get_HeldItems>d__;
			}
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x00029FB8 File Offset: 0x000281B8
		public bool IsDualWieldingRangedWeapons()
		{
			int rangedItemCount = 0;
			foreach (Item item in this.HeldItems)
			{
				if (item.GetComponent<RangedWeapon>() != null)
				{
					rangedItemCount++;
				}
				if (rangedItemCount > 1)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060004F1 RID: 1265 RVA: 0x0002A018 File Offset: 0x00028218
		// (set) Token: 0x060004F2 RID: 1266 RVA: 0x0002A020 File Offset: 0x00028220
		public float LowPassMultiplier
		{
			get
			{
				return this.lowPassMultiplier;
			}
			set
			{
				this.lowPassMultiplier = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060004F3 RID: 1267 RVA: 0x0002A038 File Offset: 0x00028238
		// (set) Token: 0x060004F4 RID: 1268 RVA: 0x0002A040 File Offset: 0x00028240
		public float ObstructVisionAmount
		{
			get
			{
				return this.obstructVisionAmount;
			}
			set
			{
				this.obstructVisionAmount = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x060004F5 RID: 1269 RVA: 0x0002A058 File Offset: 0x00028258
		// (set) Token: 0x060004F6 RID: 1270 RVA: 0x0002A067 File Offset: 0x00028267
		public bool ObstructVision
		{
			get
			{
				return this.obstructVisionAmount > 0.01f;
			}
			set
			{
				this.obstructVisionAmount = (value ? 0.5f : 0f);
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060004F7 RID: 1271 RVA: 0x0002A07E File Offset: 0x0002827E
		// (set) Token: 0x060004F8 RID: 1272 RVA: 0x0002A086 File Offset: 0x00028286
		public float PressureProtection
		{
			get
			{
				return this.pressureProtection;
			}
			set
			{
				this.pressureProtection = Math.Max(value, this.pressureProtection);
				this.pressureProtectionLastSet = Timing.TotalTime;
			}
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x060004F9 RID: 1273 RVA: 0x0002A0A5 File Offset: 0x000282A5
		public bool InPressure
		{
			get
			{
				return this.CurrentHull == null || this.CurrentHull.LethalPressure > 0f;
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x060004FA RID: 1274 RVA: 0x0002A0C3 File Offset: 0x000282C3
		public AnimController.Animation Anim
		{
			get
			{
				AnimController animController = this.AnimController;
				if (animController == null)
				{
					return AnimController.Animation.None;
				}
				return animController.Anim;
			}
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x060004FB RID: 1275 RVA: 0x0002A0D6 File Offset: 0x000282D6
		public bool IsIncapacitated
		{
			get
			{
				return this.IsUnconscious || this.CharacterHealth.IsParalyzed;
			}
		}

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x060004FC RID: 1276 RVA: 0x0002A0ED File Offset: 0x000282ED
		public bool IsUnconscious
		{
			get
			{
				return this.CharacterHealth.IsUnconscious;
			}
		}

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x060004FD RID: 1277 RVA: 0x0002A0FC File Offset: 0x000282FC
		public bool IsHandcuffed
		{
			get
			{
				return this.IsHuman && this.HasEquippedItem(Tags.HandLockerItem, true, null);
			}
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x060004FE RID: 1278 RVA: 0x0002A128 File Offset: 0x00028328
		public bool IsPet
		{
			get
			{
				return this.Params.IsPet;
			}
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x060004FF RID: 1279 RVA: 0x0002A135 File Offset: 0x00028335
		// (set) Token: 0x06000500 RID: 1280 RVA: 0x0002A142 File Offset: 0x00028342
		public float Oxygen
		{
			get
			{
				return this.CharacterHealth.OxygenAmount;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.CharacterHealth.OxygenAmount = MathHelper.Clamp(value, -100f, 100f);
			}
		}

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000501 RID: 1281 RVA: 0x0002A168 File Offset: 0x00028368
		// (set) Token: 0x06000502 RID: 1282 RVA: 0x0002A170 File Offset: 0x00028370
		public float OxygenAvailable
		{
			get
			{
				return this.oxygenAvailable;
			}
			set
			{
				this.oxygenAvailable = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x06000503 RID: 1283 RVA: 0x0002A188 File Offset: 0x00028388
		public float HullOxygenPercentage
		{
			get
			{
				Hull currentHull = this.CurrentHull;
				if (currentHull == null)
				{
					return 0f;
				}
				return currentHull.OxygenPercentage;
			}
		}

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x06000504 RID: 1284 RVA: 0x0002A19F File Offset: 0x0002839F
		// (set) Token: 0x06000505 RID: 1285 RVA: 0x0002A1A7 File Offset: 0x000283A7
		public bool UseHullOxygen { get; set; } = true;

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x06000506 RID: 1286 RVA: 0x0002A1B0 File Offset: 0x000283B0
		// (set) Token: 0x06000507 RID: 1287 RVA: 0x0002A1D8 File Offset: 0x000283D8
		public float Stun
		{
			get
			{
				if (!this.IsRagdolled || this.AnimController.IsHangingWithRope)
				{
					return this.CharacterHealth.Stun;
				}
				return 1f;
			}
			set
			{
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					return;
				}
				this.SetStun(value, true, false);
			}
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000508 RID: 1288 RVA: 0x0002A1F7 File Offset: 0x000283F7
		// (set) Token: 0x06000509 RID: 1289 RVA: 0x0002A1FF File Offset: 0x000283FF
		public CharacterHealth CharacterHealth { get; private set; }

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x0600050A RID: 1290 RVA: 0x0002A208 File Offset: 0x00028408
		public float Vitality
		{
			get
			{
				return this.CharacterHealth.Vitality;
			}
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x0600050B RID: 1291 RVA: 0x0002A215 File Offset: 0x00028415
		public float Health
		{
			get
			{
				return this.Vitality;
			}
		}

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x0600050C RID: 1292 RVA: 0x0002A21D File Offset: 0x0002841D
		public float HealthPercentage
		{
			get
			{
				return this.CharacterHealth.HealthPercentage;
			}
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x0600050D RID: 1293 RVA: 0x0002A22A File Offset: 0x0002842A
		public float MaxVitality
		{
			get
			{
				return this.CharacterHealth.MaxVitality;
			}
		}

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x0600050E RID: 1294 RVA: 0x0002A237 File Offset: 0x00028437
		public float MaxHealth
		{
			get
			{
				return this.MaxVitality;
			}
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x0600050F RID: 1295 RVA: 0x0002A23F File Offset: 0x0002843F
		public bool WasFullHealth
		{
			get
			{
				return this.CharacterHealth.WasInFullHealth;
			}
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000510 RID: 1296 RVA: 0x0002A24C File Offset: 0x0002844C
		public AIState AIState
		{
			get
			{
				EnemyAIController enemyAI = this.AIController as EnemyAIController;
				if (enemyAI == null)
				{
					return AIState.Idle;
				}
				return enemyAI.State;
			}
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x06000511 RID: 1297 RVA: 0x0002A270 File Offset: 0x00028470
		public bool IsLatched
		{
			get
			{
				EnemyAIController enemyAI = this.AIController as EnemyAIController;
				return enemyAI != null && enemyAI.LatchOntoAI != null && enemyAI.LatchOntoAI.IsAttached;
			}
		}

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000512 RID: 1298 RVA: 0x0002A2A1 File Offset: 0x000284A1
		public float EmpVulnerability
		{
			get
			{
				return this.Params.Health.EmpVulnerability;
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000513 RID: 1299 RVA: 0x0002A2B3 File Offset: 0x000284B3
		public float PoisonVulnerability
		{
			get
			{
				return this.Params.Health.PoisonVulnerability;
			}
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000514 RID: 1300 RVA: 0x0002A2C5 File Offset: 0x000284C5
		public bool IsFlipped
		{
			get
			{
				return this.AnimController.IsFlipped;
			}
		}

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x06000515 RID: 1301 RVA: 0x0002A2D2 File Offset: 0x000284D2
		// (set) Token: 0x06000516 RID: 1302 RVA: 0x0002A2DF File Offset: 0x000284DF
		public float Bloodloss
		{
			get
			{
				return this.CharacterHealth.BloodlossAmount;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.CharacterHealth.BloodlossAmount = value;
			}
		}

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x06000517 RID: 1303 RVA: 0x0002A2F6 File Offset: 0x000284F6
		public float Bleeding
		{
			get
			{
				return this.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.BleedingType, true);
			}
		}

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000518 RID: 1304 RVA: 0x0002A309 File Offset: 0x00028509
		// (set) Token: 0x06000519 RID: 1305 RVA: 0x0002A32F File Offset: 0x0002852F
		public float SpeechImpediment
		{
			get
			{
				if (!this.CanSpeak || this.IsUnconscious || this.IsKnockedDown)
				{
					return 100f;
				}
				return this.speechImpediment;
			}
			set
			{
				if (value < this.speechImpediment)
				{
					return;
				}
				this.speechImpedimentSet = true;
				this.speechImpediment = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x0600051A RID: 1306 RVA: 0x0002A358 File Offset: 0x00028558
		// (set) Token: 0x0600051B RID: 1307 RVA: 0x0002A360 File Offset: 0x00028560
		public float TextChatVolume
		{
			get
			{
				return this.textChatVolume;
			}
			set
			{
				this.textChatVolume = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x0600051C RID: 1308 RVA: 0x0002A378 File Offset: 0x00028578
		// (set) Token: 0x0600051D RID: 1309 RVA: 0x0002A380 File Offset: 0x00028580
		public float PressureTimer { get; private set; }

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x0600051E RID: 1310 RVA: 0x0002A389 File Offset: 0x00028589
		// (set) Token: 0x0600051F RID: 1311 RVA: 0x0002A391 File Offset: 0x00028591
		public float DisableImpactDamageTimer { get; set; }

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000520 RID: 1312 RVA: 0x0002A39A File Offset: 0x0002859A
		// (set) Token: 0x06000521 RID: 1313 RVA: 0x0002A3A2 File Offset: 0x000285A2
		public bool IgnoreMeleeWeapons { get; set; }

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x06000522 RID: 1314 RVA: 0x0002A3AC File Offset: 0x000285AC
		public float CurrentSpeed
		{
			get
			{
				AnimController animController = this.AnimController;
				float? num;
				if (animController == null)
				{
					num = null;
				}
				else
				{
					PhysicsBody collider = animController.Collider;
					num = ((collider != null) ? new float?(collider.LinearVelocity.Length()) : null);
				}
				float? num2 = num;
				return num2.GetValueOrDefault();
			}
		}

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06000523 RID: 1315 RVA: 0x0002A3FC File Offset: 0x000285FC
		// (set) Token: 0x06000524 RID: 1316 RVA: 0x0002A404 File Offset: 0x00028604
		public Item SelectedItem
		{
			get
			{
				return this._selectedItem;
			}
			set
			{
				Item prevSelectedItem = this._selectedItem;
				this._selectedItem = value;
				if (value != null)
				{
					this.CheckTalents(AbilityEffectType.OnItemSelected, new AbilityItemSelected(value));
				}
				HintManager.OnSetSelectedItem(this, prevSelectedItem, this._selectedItem);
				if (this.IsLocalPlayer)
				{
					Item selectedItem = this._selectedItem;
					if (selectedItem != null)
					{
						Fabricator component = selectedItem.GetComponent<Fabricator>();
						if (component != null)
						{
							component.RefreshSelectedItem();
						}
					}
					if (this._selectedItem != null)
					{
						GameSession gameSession = GameMain.GameSession;
						if (gameSession != null)
						{
							CrewManager crewManager = gameSession.CrewManager;
							if (crewManager != null)
							{
								crewManager.AutoHideCrewList();
							}
						}
					}
					else
					{
						GameSession gameSession2 = GameMain.GameSession;
						if (gameSession2 != null)
						{
							CrewManager crewManager2 = gameSession2.CrewManager;
							if (crewManager2 != null)
							{
								crewManager2.ResetCrewListOpenState();
							}
						}
					}
					Item selectedItem2 = this._selectedItem;
					if (selectedItem2 != null)
					{
						CircuitBox component2 = selectedItem2.GetComponent<CircuitBox>();
						if (component2 != null)
						{
							component2.OnViewUpdateProjSpecific();
						}
					}
				}
				if (prevSelectedItem != null && (this._selectedItem == null || this._selectedItem != prevSelectedItem) && this.itemSelectedTime > 0.0)
				{
					double selectedDuration = Timing.TotalTime - this.itemSelectedTime;
					if (this.itemSelectedDurations.ContainsKey(prevSelectedItem.Prefab))
					{
						Dictionary<ItemPrefab, double> dictionary = this.itemSelectedDurations;
						ItemPrefab prefab = prevSelectedItem.Prefab;
						dictionary[prefab] += selectedDuration;
					}
					else
					{
						this.itemSelectedDurations.Add(prevSelectedItem.Prefab, selectedDuration);
					}
					this.itemSelectedTime = 0.0;
				}
				if (this._selectedItem != null && (prevSelectedItem == null || prevSelectedItem != this._selectedItem))
				{
					this.itemSelectedTime = Timing.TotalTime;
				}
				if (prevSelectedItem != this._selectedItem && prevSelectedItem != null && prevSelectedItem.OnDeselect != null)
				{
					prevSelectedItem.OnDeselect(this);
				}
			}
		}

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000525 RID: 1317 RVA: 0x0002A585 File Offset: 0x00028785
		// (set) Token: 0x06000526 RID: 1318 RVA: 0x0002A58D File Offset: 0x0002878D
		public Item SelectedSecondaryItem { get; set; }

		// Token: 0x06000527 RID: 1319 RVA: 0x0002A596 File Offset: 0x00028796
		public void ReleaseSecondaryItem()
		{
			this.SelectedSecondaryItem = null;
		}

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000528 RID: 1320 RVA: 0x0002A59F File Offset: 0x0002879F
		public bool HasSelectedAnyItem
		{
			get
			{
				return this.SelectedItem != null || this.SelectedSecondaryItem != null;
			}
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x0002A5B4 File Offset: 0x000287B4
		public bool IsAnySelectedItem(Item item)
		{
			return item == this.SelectedItem || item == this.SelectedSecondaryItem;
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0002A5CA File Offset: 0x000287CA
		public bool HasSelectedAnotherSecondaryItem(Item item)
		{
			return this.SelectedSecondaryItem != null && this.SelectedSecondaryItem != item;
		}

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x0600052B RID: 1323 RVA: 0x0002A5E2 File Offset: 0x000287E2
		// (set) Token: 0x0600052C RID: 1324 RVA: 0x0002A5EA File Offset: 0x000287EA
		public Item FocusedItem
		{
			get
			{
				return this.focusedItem;
			}
			set
			{
				this.focusedItem = value;
			}
		}

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x0600052D RID: 1325 RVA: 0x0002A5F3 File Offset: 0x000287F3
		// (set) Token: 0x0600052E RID: 1326 RVA: 0x0002A5FB File Offset: 0x000287FB
		public Item PickingItem { get; set; }

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x0600052F RID: 1327 RVA: 0x0002A604 File Offset: 0x00028804
		public virtual AIController AIController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000530 RID: 1328 RVA: 0x0002A607 File Offset: 0x00028807
		// (set) Token: 0x06000531 RID: 1329 RVA: 0x0002A60F File Offset: 0x0002880F
		public bool IsDead
		{
			get
			{
				return this.isDead;
			}
			set
			{
				if (this.isDead == value)
				{
					return;
				}
				if (value)
				{
					this.Kill(CauseOfDeathType.Unknown, null, false, true);
					return;
				}
				this.Revive(true, false);
			}
		}

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000532 RID: 1330 RVA: 0x0002A631 File Offset: 0x00028831
		// (set) Token: 0x06000533 RID: 1331 RVA: 0x0002A639 File Offset: 0x00028839
		public bool EnableDespawn { get; set; } = true;

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000534 RID: 1332 RVA: 0x0002A642 File Offset: 0x00028842
		// (set) Token: 0x06000535 RID: 1333 RVA: 0x0002A64A File Offset: 0x0002884A
		public CauseOfDeath CauseOfDeath { get; private set; }

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000536 RID: 1334 RVA: 0x0002A653 File Offset: 0x00028853
		public CauseOfDeathType CauseOfDeathType
		{
			get
			{
				CauseOfDeath causeOfDeath = this.CauseOfDeath;
				if (causeOfDeath == null)
				{
					return CauseOfDeathType.None;
				}
				return causeOfDeath.Type;
			}
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000537 RID: 1335 RVA: 0x0002A666 File Offset: 0x00028866
		public bool CanBeSelected
		{
			get
			{
				return !base.Removed;
			}
		}

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000538 RID: 1336 RVA: 0x0002A671 File Offset: 0x00028871
		public bool IsDraggable
		{
			get
			{
				return !base.Removed || this.AnimController.Draggable;
			}
		}

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000539 RID: 1337 RVA: 0x0002A688 File Offset: 0x00028888
		public bool CanAim
		{
			get
			{
				if (this.SelectedItem != null)
				{
					Controller component = this.SelectedItem.GetComponent<Controller>();
					if (component == null || !component.AllowAiming)
					{
						return false;
					}
				}
				if (!this.IsKnockedDownOrRagdolled)
				{
					return !this.IsRagdolled || this.AnimController.IsHoldingToRope;
				}
				return false;
			}
		}

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x0600053A RID: 1338 RVA: 0x0002A6D4 File Offset: 0x000288D4
		public bool InWater
		{
			get
			{
				AnimController animController = this.AnimController;
				return animController != null && animController.InWater;
			}
		}

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x0600053B RID: 1339 RVA: 0x0002A6F3 File Offset: 0x000288F3
		public bool IsLowInOxygen
		{
			get
			{
				return this.CharacterHealth.OxygenAmount < 100f;
			}
		}

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x0600053C RID: 1340 RVA: 0x0002A707 File Offset: 0x00028907
		// (set) Token: 0x0600053D RID: 1341 RVA: 0x0002A714 File Offset: 0x00028914
		public bool Unkillable
		{
			get
			{
				return this.CharacterHealth.Unkillable;
			}
			set
			{
				this.CharacterHealth.Unkillable = value;
			}
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x0600053E RID: 1342 RVA: 0x0002A722 File Offset: 0x00028922
		// (set) Token: 0x0600053F RID: 1343 RVA: 0x0002A72F File Offset: 0x0002892F
		public bool UseHealthWindow
		{
			get
			{
				return this.CharacterHealth.UseHealthWindow;
			}
			set
			{
				this.CharacterHealth.UseHealthWindow = value;
			}
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000540 RID: 1344 RVA: 0x0002A740 File Offset: 0x00028940
		public override Vector2 SimPosition
		{
			get
			{
				AnimController animController = this.AnimController;
				if (((animController != null) ? animController.Collider : null) == null)
				{
					if (!this.accessRemovedCharacterErrorShown)
					{
						string errorMsg = string.Concat(new string[]
						{
							"Attempted to access a potentially removed character. Character: [name], id: ",
							this.ID.ToString(),
							", removed: ",
							base.Removed.ToString(),
							"."
						});
						if (this.AnimController == null)
						{
							errorMsg += " AnimController == null";
						}
						else if (this.AnimController.Collider == null)
						{
							errorMsg += " AnimController.Collider == null";
						}
						errorMsg = errorMsg + "\n" + Environment.StackTrace.CleanupStackTrace();
						DebugConsole.NewMessage(errorMsg.Replace("[name]", this.Name), new Color?(Color.Red), false);
						GameAnalyticsManager.AddErrorEventOnce("Character.SimPosition:AccessRemoved", GameAnalyticsManager.ErrorSeverity.Error, errorMsg.Replace("[name]", this.SpeciesName.Value) + "\n" + Environment.StackTrace.CleanupStackTrace());
						this.accessRemovedCharacterErrorShown = true;
					}
					return Vector2.Zero;
				}
				return this.AnimController.Collider.SimPosition;
			}
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000541 RID: 1345 RVA: 0x0002A86D File Offset: 0x00028A6D
		public override Vector2 Position
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.SimPosition);
			}
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000542 RID: 1346 RVA: 0x0002A87A File Offset: 0x00028A7A
		public override Vector2 DrawPosition
		{
			get
			{
				if (this.AnimController.MainLimb == null)
				{
					return Vector2.Zero;
				}
				return this.AnimController.MainLimb.body.DrawPosition;
			}
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000543 RID: 1347 RVA: 0x0002A8A4 File Offset: 0x00028AA4
		public bool IsInFriendlySub
		{
			get
			{
				return base.Submarine != null && base.Submarine.TeamID == this.TeamID;
			}
		}

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000544 RID: 1348 RVA: 0x0002A8C3 File Offset: 0x00028AC3
		public bool IsInPlayerSub
		{
			get
			{
				return base.Submarine != null && base.Submarine.Info.IsPlayer;
			}
		}

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000545 RID: 1349 RVA: 0x0002A8DF File Offset: 0x00028ADF
		public bool InPlayerSubmarine
		{
			get
			{
				return this.IsInPlayerSub;
			}
		}

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06000546 RID: 1350 RVA: 0x0002A8E7 File Offset: 0x00028AE7
		// (set) Token: 0x06000547 RID: 1351 RVA: 0x0002A8F4 File Offset: 0x00028AF4
		public float AITurretPriority
		{
			get
			{
				return this.Params.AITurretPriority;
			}
			private set
			{
				this.Params.AITurretPriority = value;
			}
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x0002A904 File Offset: 0x00028B04
		public static Character Create(CharacterInfo characterInfo, Vector2 position, string seed, ushort id = 0, bool isRemotePlayer = false, bool hasAi = true, RagdollParams ragdoll = null, bool spawnInitialItems = true)
		{
			return Character.Create(characterInfo.SpeciesName, position, seed, characterInfo, id, isRemotePlayer, hasAi, true, ragdoll, spawnInitialItems, true);
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x0002A92C File Offset: 0x00028B2C
		public static Character Create(string speciesName, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isRemotePlayer = false, bool hasAi = true, bool createNetworkEvent = true, RagdollParams ragdoll = null, bool throwErrorIfNotFound = true, bool spawnInitialItems = true)
		{
			if (speciesName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
			{
				speciesName = Path.GetFileNameWithoutExtension(speciesName);
			}
			return Character.Create(speciesName.ToIdentifier(), position, seed, characterInfo, id, isRemotePlayer, hasAi, createNetworkEvent, ragdoll, throwErrorIfNotFound, spawnInitialItems);
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x0002A96C File Offset: 0x00028B6C
		public static Character Create(Identifier speciesName, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isRemotePlayer = false, bool hasAi = true, bool createNetworkEvent = true, RagdollParams ragdoll = null, bool throwErrorIfNotFound = true, bool spawnInitialItems = true)
		{
			CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(speciesName);
			if (prefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to create character \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
				defaultInterpolatedStringHandler.AppendLiteral("\". Matching prefab not found.\n");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace;
				if (throwErrorIfNotFound)
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
				else
				{
					DebugConsole.AddWarning(errorMsg, null);
				}
				return null;
			}
			return Character.Create(prefab, position, seed, characterInfo, id, isRemotePlayer, hasAi, createNetworkEvent, ragdoll, spawnInitialItems);
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x0002A9F0 File Offset: 0x00028BF0
		public static Character Create(CharacterPrefab prefab, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isRemotePlayer = false, bool hasAi = true, bool createNetworkEvent = true, RagdollParams ragdoll = null, bool spawnInitialItems = true)
		{
			Character newCharacter;
			if (prefab.Identifier != CharacterPrefab.HumanSpeciesName || hasAi)
			{
				AICharacter aiCharacter = new AICharacter(prefab, position, seed, characterInfo, id, isRemotePlayer, ragdoll, spawnInitialItems);
				AIController ai = (prefab.Identifier == CharacterPrefab.HumanSpeciesName || aiCharacter.Params.UseHumanAI) ? new HumanAIController(aiCharacter) : new EnemyAIController(aiCharacter, seed);
				aiCharacter.SetAI(ai);
				newCharacter = aiCharacter;
			}
			else
			{
				newCharacter = new Character(prefab, position, seed, characterInfo, id, isRemotePlayer, ragdoll, spawnInitialItems);
			}
			return newCharacter;
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x0002AA74 File Offset: 0x00028C74
		protected Character(CharacterPrefab prefab, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isRemotePlayer = false, RagdollParams ragdollParams = null, bool spawnInitialItems = true) : base(null, id)
		{
			this.wallet = new Wallet(Option<Character>.Some(this));
			GameSession gameSession = GameMain.GameSession;
			Wallet wallet;
			if (gameSession == null)
			{
				wallet = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				wallet = ((campaign != null) ? campaign.Bank : null);
			}
			Wallet bank = wallet;
			if (bank != null)
			{
				this.wallet.SetRewardDistribution(bank.RewardDistribution);
			}
			this.Seed = seed;
			this.Prefab = prefab;
			MTRandom random = new MTRandom(ToolBox.StringToInt(seed));
			this.IsRemotePlayer = isRemotePlayer;
			this.oxygenAvailable = 100f;
			this.aiTarget = new AITarget(this);
			this.lowPassMultiplier = 1f;
			this.Properties = SerializableProperty.GetProperties(this);
			this.Params = new CharacterParams(prefab.ContentFile as CharacterFile);
			this.Info = characterInfo;
			Identifier speciesName = prefab.Identifier;
			Identifier npcIdentifier = this.VariantOf;
			if (npcIdentifier == CharacterPrefab.HumanSpeciesName || speciesName == CharacterPrefab.HumanSpeciesName)
			{
				npcIdentifier = this.VariantOf;
				if (!npcIdentifier.IsEmpty)
				{
					DebugConsole.ThrowError("The variant system does not yet support humans, sorry. It does support other humanoids though!", null, this.Prefab.ContentPackage, false, false);
				}
				if (characterInfo == null)
				{
					Identifier humanSpeciesName = CharacterPrefab.HumanSpeciesName;
					string name2 = "";
					string originalName = "";
					Either<Job, JobPrefab> jobOrJobPrefab = null;
					int variant = 0;
					Rand.RandSync randSync = Rand.RandSync.Unsynced;
					npcIdentifier = default(Identifier);
					this.Info = new CharacterInfo(humanSpeciesName, name2, originalName, jobOrJobPrefab, variant, randSync, npcIdentifier);
				}
			}
			if (this.Info != null)
			{
				this.teamID = this.Info.TeamID;
				this.Info.IsNewHire = false;
			}
			ValueTuple<Identifier, Identifier>? valueTuple = (characterInfo != null) ? new ValueTuple<Identifier, Identifier>?(characterInfo.HumanPrefabIds) : null;
			if (valueTuple != null)
			{
				ValueTuple<Identifier, Identifier> valueOrDefault = valueTuple.GetValueOrDefault();
				npcIdentifier = valueOrDefault.Item1;
				if (!npcIdentifier.IsEmpty)
				{
					Identifier npcIdentifier2 = valueOrDefault.Item2;
					if (!npcIdentifier2.IsEmpty)
					{
						this.HumanPrefab = characterInfo.HumanPrefab;
					}
				}
			}
			this.keys = new Key[Enum.GetNames(typeof(InputType)).Length];
			for (int i = 0; i < Enum.GetNames(typeof(InputType)).Length; i++)
			{
				this.keys[i] = new Key((InputType)i);
			}
			ContentXElement mainElement = prefab.ConfigElement;
			this.InitProjSpecific(mainElement);
			List<ContentXElement> inventoryElements = new List<ContentXElement>();
			List<float> inventoryCommonness = new List<float>();
			List<ContentXElement> healthElements = new List<ContentXElement>();
			List<float> healthCommonness = new List<float>();
			foreach (ContentXElement subElement in mainElement.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "inventory"))
				{
					if (!(a == "health"))
					{
						if (a == "statuseffect")
						{
							StatusEffect statusEffect = StatusEffect.Load(subElement, this.Name);
							if (statusEffect != null)
							{
								if (!this.statusEffects.ContainsKey(statusEffect.type))
								{
									this.statusEffects.Add(statusEffect.type, new List<StatusEffect>());
								}
								this.statusEffects[statusEffect.type].Add(statusEffect);
							}
						}
					}
					else
					{
						healthElements.Add(subElement);
						healthCommonness.Add(subElement.GetAttributeFloat("commonness", 1f));
					}
				}
				else
				{
					inventoryElements.Add(subElement);
					inventoryCommonness.Add(subElement.GetAttributeFloat("commonness", 1f));
				}
			}
			if (this.Params.VariantFile != null)
			{
				ContentXElement paramsMainElement = this.Params.MainElement;
				if (paramsMainElement != null)
				{
					ContentXElement overrideElement = this.Params.VariantFile.GetRootExcludingOverride().FromPackage(paramsMainElement.ContentPackage);
					ContentXElement childElement = overrideElement.GetChildElement("inventory");
					ContentXElement contentXElement = null;
					if (childElement != contentXElement)
					{
						inventoryElements.Clear();
						inventoryCommonness.Clear();
						foreach (ContentXElement subElement2 in overrideElement.GetChildElements("inventory"))
						{
							string a2 = subElement2.Name.ToString().ToLowerInvariant();
							if (a2 == "inventory")
							{
								inventoryElements.Add(subElement2);
								inventoryCommonness.Add(subElement2.GetAttributeFloat("commonness", 1f));
							}
						}
					}
					childElement = overrideElement.GetChildElement("health");
					contentXElement = null;
					if (childElement != contentXElement)
					{
						healthElements.Clear();
						healthCommonness.Clear();
						foreach (ContentXElement subElement3 in overrideElement.GetChildElements("health"))
						{
							healthElements.Add(subElement3);
							healthCommonness.Add(subElement3.GetAttributeFloat("commonness", 1f));
						}
					}
				}
			}
			if (inventoryElements.Count > 0)
			{
				this.Inventory = new CharacterInventory((inventoryElements.Count == 1) ? inventoryElements[0] : ToolBox.SelectWeightedRandom<ContentXElement>(inventoryElements, inventoryCommonness, random), this, spawnInitialItems);
			}
			if (healthElements.Count == 0)
			{
				this.CharacterHealth = new CharacterHealth(this);
			}
			else
			{
				ContentXElement selectedHealthElement = (healthElements.Count == 1) ? healthElements[0] : ToolBox.SelectWeightedRandom<ContentXElement>(healthElements, healthCommonness, random);
				ContentXElement limbHealthElement = selectedHealthElement;
				if (this.Params.VariantFile != null)
				{
					ContentXElement childElement = limbHealthElement.GetChildElement("limb");
					ContentXElement contentXElement = null;
					if (childElement == contentXElement)
					{
						limbHealthElement = this.Params.OriginalElement.GetChildElement("health");
					}
				}
				this.CharacterHealth = new CharacterHealth(selectedHealthElement, this, limbHealthElement);
			}
			if (this.Params.Husk)
			{
				Identifier nonHuskedSpeciesName = this.Params.NonHuskedSpecies;
				if (!nonHuskedSpeciesName.IsEmpty || this.Params.UseHuskAppendage)
				{
					AfflictionPrefab matchingAffliction = null;
					foreach (AfflictionPrefabHusk huskPrefab in AfflictionPrefab.Prefabs.OfType<AfflictionPrefabHusk>())
					{
						if (!huskPrefab.HuskedSpeciesName.IsEmpty)
						{
							Identifier nonHuskedSpecies = nonHuskedSpeciesName;
							if (nonHuskedSpeciesName.IsEmpty)
							{
								nonHuskedSpecies = AfflictionHusk.GetNonHuskedSpeciesName(this.Params, huskPrefab);
							}
							if (huskPrefab.TargetSpecies.Contains(nonHuskedSpecies))
							{
								nonHuskedSpeciesName = nonHuskedSpecies;
								matchingAffliction = huskPrefab;
								break;
							}
						}
					}
					if (matchingAffliction == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(339, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Cannot find a husk infection that matches ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
						defaultInterpolatedStringHandler.AppendLiteral("! Please make sure that the speciesname is added as 'targets' in the husk affliction prefab definition! ");
						defaultInterpolatedStringHandler.AppendLiteral("If the name of the character doesn't match the default pattern ('Crawlerhusk', 'Humanhusk', etc), you'll also need to define the non-husked species with ");
						defaultInterpolatedStringHandler.AppendFormatted("NonHuskedSpecies");
						defaultInterpolatedStringHandler.AppendLiteral(" attribute in the character config file.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
						nonHuskedSpeciesName = (this.IsHumanoid ? CharacterPrefab.HumanSpeciesName : "crawler".ToIdentifier());
						speciesName = nonHuskedSpeciesName;
					}
				}
				if (ragdollParams == null)
				{
					Identifier npcIdentifier2 = prefab.VariantOf;
					if (npcIdentifier2 == null)
					{
						Identifier name = this.Params.UseHuskAppendage ? nonHuskedSpeciesName : speciesName;
						ragdollParams = (this.IsHumanoid ? RagdollParams.GetDefaultRagdollParams<HumanRagdollParams>(name, this.Params, this.Prefab.ContentPackage) : RagdollParams.GetDefaultRagdollParams<FishRagdollParams>(name, this.Params, this.Prefab.ContentPackage));
					}
				}
				if (this.Params.HasInfo && this.info == null)
				{
					Identifier speciesName2 = nonHuskedSpeciesName;
					string name3 = "";
					string originalName2 = "";
					Either<Job, JobPrefab> jobOrJobPrefab2 = null;
					int variant2 = 0;
					Rand.RandSync randSync2 = Rand.RandSync.Unsynced;
					Identifier npcIdentifier2 = default(Identifier);
					this.info = new CharacterInfo(speciesName2, name3, originalName2, jobOrJobPrefab2, variant2, randSync2, npcIdentifier2);
				}
			}
			else if (this.Params.HasInfo && this.info == null)
			{
				Identifier speciesName3 = speciesName;
				string name4 = "";
				string originalName3 = "";
				Either<Job, JobPrefab> jobOrJobPrefab3 = null;
				int variant3 = 0;
				Rand.RandSync randSync3 = Rand.RandSync.Unsynced;
				Identifier npcIdentifier2 = default(Identifier);
				this.info = new CharacterInfo(speciesName3, name4, originalName3, jobOrJobPrefab3, variant3, randSync3, npcIdentifier2);
			}
			if (this.IsHumanoid)
			{
				this.AnimController = new HumanoidAnimController(this, seed, ragdollParams as HumanRagdollParams)
				{
					TargetDir = Direction.Right
				};
			}
			else
			{
				this.AnimController = new FishAnimController(this, seed, ragdollParams as FishRagdollParams);
				this.PressureProtection = 2.1474836E+09f;
			}
			this.CharacterHealth.CheckForErrors();
			this.AnimController.SetPosition(ConvertUnits.ToSimUnits(position), false, true, false, true);
			this.AnimController.FindHull(null, true, true);
			if (this.AnimController.CurrentHull != null)
			{
				base.Submarine = this.AnimController.CurrentHull.Submarine;
			}
			this.IsContainable = prefab.ConfigElement.GetAttributeBool("IsContainable", this.Mass < 35f);
			Character.CharacterList.Add(this);
			this.Enabled = (GameMain.NetworkMember == null);
			if (this.info != null)
			{
				this.LoadHeadAttachments();
			}
			this.ApplyStatusEffects(ActionType.OnSpawn, 1f);
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0002B55C File Offset: 0x0002975C
		private void InitProjSpecific(ContentXElement mainElement)
		{
			this.soundTimer = Rand.Range(0f, this.Params.SoundInterval, Rand.RandSync.Unsynced);
			this.sounds = new List<CharacterSound>();
			this.Params.Sounds.ForEach(delegate(CharacterParams.SoundParams s)
			{
				this.sounds.Add(new CharacterSound(s));
			});
			foreach (ContentXElement subElement in mainElement.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "damageemitter"))
				{
					if (!(a == "bloodemitter"))
					{
						if (a == "gibemitter")
						{
							this.gibEmitters.Add(new ParticleEmitter(subElement));
						}
					}
					else
					{
						this.bloodEmitters.Add(new ParticleEmitter(subElement));
					}
				}
				else
				{
					this.damageEmitters.Add(new ParticleEmitter(subElement));
				}
			}
			this.hudProgressBars = new Dictionary<object, HUDProgressBar>();
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x0002B664 File Offset: 0x00029864
		public void ReloadHead(int? headId = null, int hairIndex = -1, int beardIndex = -1, int moustacheIndex = -1, int faceAttachmentIndex = -1)
		{
			if (this.Info == null)
			{
				return;
			}
			Limb head = this.AnimController.GetLimb(LimbType.Head, true, false, false);
			if (head == null)
			{
				return;
			}
			HashSet<Identifier> tags = this.Info.Head.Preset.TagSet.ToHashSet<Identifier>();
			if (headId != null)
			{
				tags.RemoveWhere((Identifier t) => t.StartsWith("variant"));
				HashSet<Identifier> hashSet = tags;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("variant");
				defaultInterpolatedStringHandler.AppendFormatted<int>(headId.Value);
				hashSet.Add(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
			}
			CharacterInfo.HeadInfo oldHeadInfo = this.Info.Head;
			this.Info.RecreateHead(tags.ToImmutableHashSet<Identifier>(), hairIndex, beardIndex, moustacheIndex, faceAttachmentIndex);
			if (hairIndex == -1)
			{
				this.Info.Head.HairIndex = oldHeadInfo.HairIndex;
			}
			if (beardIndex == -1)
			{
				this.Info.Head.BeardIndex = oldHeadInfo.BeardIndex;
			}
			if (moustacheIndex == -1)
			{
				this.Info.Head.MoustacheIndex = oldHeadInfo.MoustacheIndex;
			}
			if (faceAttachmentIndex == -1)
			{
				this.Info.Head.FaceAttachmentIndex = oldHeadInfo.FaceAttachmentIndex;
			}
			this.Info.Head.SkinColor = oldHeadInfo.SkinColor;
			this.Info.Head.HairColor = oldHeadInfo.HairColor;
			this.Info.Head.FacialHairColor = oldHeadInfo.FacialHairColor;
			this.Info.CheckColors();
			head.RecreateSprites();
			this.LoadHeadAttachments();
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x0002B7F8 File Offset: 0x000299F8
		public void LoadHeadAttachments()
		{
			if (this.Info == null)
			{
				return;
			}
			if (this.AnimController == null)
			{
				return;
			}
			Limb head = this.AnimController.GetLimb(LimbType.Head, true, false, false);
			if (head == null)
			{
				return;
			}
			head.OtherWearables.ForEach(delegate(WearableSprite w)
			{
				Sprite sprite = w.Sprite;
				if (sprite == null)
				{
					return;
				}
				sprite.Remove();
			});
			head.OtherWearables.Clear();
			ContentXElement contentXElement = this.info.Head.FaceAttachment;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				this.info.Head.FaceAttachmentIndex = 0;
			}
			ContentXElement faceAttachment = this.Info.Head.FaceAttachment;
			if (faceAttachment != null)
			{
				faceAttachment.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					head.OtherWearables.Add(new WearableSprite(s, WearableType.FaceAttachment));
				});
			}
			contentXElement = this.info.Head.BeardElement;
			contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				this.info.Head.BeardIndex = 0;
			}
			ContentXElement beardElement = this.Info.Head.BeardElement;
			if (beardElement != null)
			{
				beardElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					head.OtherWearables.Add(new WearableSprite(s, WearableType.Beard));
				});
			}
			contentXElement = this.info.Head.MoustacheElement;
			contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				this.info.Head.MoustacheIndex = 0;
			}
			ContentXElement moustacheElement = this.Info.Head.MoustacheElement;
			if (moustacheElement != null)
			{
				moustacheElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					head.OtherWearables.Add(new WearableSprite(s, WearableType.Moustache));
				});
			}
			contentXElement = this.info.Head.HairElement;
			contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				this.info.Head.HairIndex = 0;
			}
			ContentXElement hairElement = this.Info.Head.HairElement;
			if (hairElement != null)
			{
				hairElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					head.OtherWearables.Add(new WearableSprite(s, WearableType.Hair));
				});
			}
			CharacterInfo.HeadInfo head2 = this.info.Head;
			ContentXElement contentXElement3;
			if (head2 == null)
			{
				contentXElement3 = null;
			}
			else
			{
				ContentXElement hairWithHatElement = head2.HairWithHatElement;
				contentXElement3 = ((hairWithHatElement != null) ? hairWithHatElement.GetChildElement("sprite") : null);
			}
			contentXElement = contentXElement3;
			contentXElement2 = null;
			if (contentXElement != contentXElement2)
			{
				head.HairWithHatSprite = new WearableSprite(this.info.Head.HairWithHatElement.GetChildElement("sprite"), WearableType.Hair);
			}
			head.EnableHuskSprite = this.Params.Husk;
			head.LoadHerpesSprite();
			head.UpdateWearableTypesToHide();
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0002BA88 File Offset: 0x00029C88
		public bool IsKeyHit(InputType inputType)
		{
			return this.keys[(int)inputType].Hit;
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x0002BA98 File Offset: 0x00029C98
		public bool IsKeyDown(InputType inputType)
		{
			if (inputType == InputType.Up || inputType == InputType.Down || inputType == InputType.Left || inputType == InputType.Right)
			{
				Affliction invertControls = this.CharacterHealth.GetAfflictionOfType("invertcontrols".ToIdentifier(), true);
				if (invertControls != null)
				{
					switch (inputType)
					{
					case InputType.Up:
						inputType = InputType.Down;
						break;
					case InputType.Down:
						inputType = InputType.Up;
						break;
					case InputType.Left:
						inputType = InputType.Right;
						break;
					case InputType.Right:
						inputType = InputType.Left;
						break;
					}
				}
			}
			return (this == Character.Controlled && inputType == InputType.Run && this.ToggleRun) || this.keys[(int)inputType].Held;
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x0002BB1E File Offset: 0x00029D1E
		public void SetInput(InputType inputType, bool hit, bool held)
		{
			this.keys[(int)inputType].Hit = hit;
			this.keys[(int)inputType].Held = held;
			this.keys[(int)inputType].SetState(hit, held);
		}

		// Token: 0x06000553 RID: 1363 RVA: 0x0002BB4B File Offset: 0x00029D4B
		public void ClearInput(InputType inputType)
		{
			this.keys[(int)inputType].Hit = false;
			this.keys[(int)inputType].Held = false;
		}

		// Token: 0x06000554 RID: 1364 RVA: 0x0002BB6C File Offset: 0x00029D6C
		public void ClearInputs()
		{
			if (this.keys == null)
			{
				return;
			}
			foreach (Key key in this.keys)
			{
				key.Hit = false;
				key.Held = false;
			}
		}

		// Token: 0x06000555 RID: 1365 RVA: 0x0002BBAC File Offset: 0x00029DAC
		public override string ToString()
		{
			return this.SpeciesName.Value;
		}

		// Token: 0x06000556 RID: 1366 RVA: 0x0002BBC8 File Offset: 0x00029DC8
		public void GiveJobItems(bool isPvPMode, WayPoint spawnPoint = null)
		{
			if (this.info == null)
			{
				return;
			}
			ValueTuple<Identifier, Identifier> humanPrefabIds = this.info.HumanPrefabIds;
			Identifier identifier = default(Identifier);
			if (!(humanPrefabIds.Item1 != identifier))
			{
				Identifier identifier2 = default(Identifier);
				if (!(humanPrefabIds.Item2 != identifier2))
				{
					goto IL_102;
				}
			}
			HumanPrefab prefab = this.info.HumanPrefab;
			if (prefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(99, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to give job items for the character \"");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" - could not find human prefab with the id \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.info.HumanPrefabIds.Item2);
				defaultInterpolatedStringHandler.AppendLiteral("\" from \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.info.HumanPrefabIds.Item1);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			else if (prefab.GiveItems(this, ((spawnPoint != null) ? spawnPoint.Submarine : null) ?? base.Submarine, spawnPoint, Rand.RandSync.Unsynced, true))
			{
				return;
			}
			IL_102:
			Job job = this.info.Job;
			if (job == null)
			{
				return;
			}
			job.GiveJobItems(this, isPvPMode, spawnPoint);
		}

		// Token: 0x06000557 RID: 1367 RVA: 0x0002BCF0 File Offset: 0x00029EF0
		public void GiveIdCardTags(WayPoint spawnPoint, bool createNetworkEvent = false)
		{
			CharacterInfo characterInfo = this.info;
			if (((characterInfo != null) ? characterInfo.Job : null) == null || spawnPoint == null)
			{
				return;
			}
			foreach (Item item in this.Inventory.AllItems)
			{
				IdCard idCard = (item != null) ? item.GetComponent<IdCard>() : null;
				if (idCard != null && !(idCard.OwnerName != this.info.Name))
				{
					foreach (string s in spawnPoint.IdCardTags)
					{
						item.AddTag(s);
					}
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
					{
						Item item2 = item;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 1);
						defaultInterpolatedStringHandler.AppendLiteral("id_");
						defaultInterpolatedStringHandler.AppendFormatted<CharacterTeamType>(this.TeamID);
						item2.AddTag(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
					}
					if (createNetworkEvent)
					{
						NetworkMember networkMember = GameMain.NetworkMember;
						if (networkMember != null && networkMember.IsServer)
						{
							GameMain.NetworkMember.CreateEntityEvent(item, new Item.ChangePropertyEventData(item.SerializableProperties["Tags".ToIdentifier()], item));
						}
					}
				}
			}
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x0002BE3C File Offset: 0x0002A03C
		public float GetSkillLevel(Identifier skillIdentifier)
		{
			CharacterInfo characterInfo = this.Info;
			if (((characterInfo != null) ? characterInfo.Job : null) == null)
			{
				return 0f;
			}
			float skillLevel = this.Info.Job.GetSkillLevel(skillIdentifier);
			StatTypes statType;
			if (Character.overrideStatTypes.TryGetValue(skillIdentifier, out statType))
			{
				float skillOverride = this.GetStatValue(statType, true);
				if (skillOverride > skillLevel)
				{
					skillLevel = skillOverride;
				}
			}
			foreach (Affliction affliction in this.CharacterHealth.GetAllAfflictions())
			{
				skillLevel *= affliction.GetSkillMultiplier();
			}
			float skillValue;
			if (skillIdentifier != null && this.wearableSkillModifiers.TryGetValue(skillIdentifier, out skillValue))
			{
				skillLevel += skillValue;
			}
			StatTypes skillStatType = Character.GetSkillStatType(skillIdentifier);
			if (skillStatType != StatTypes.None)
			{
				skillLevel += this.GetStatValue(skillStatType, true);
			}
			return Math.Max(skillLevel, 0f);
		}

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x06000559 RID: 1369 RVA: 0x0002BF20 File Offset: 0x0002A120
		// (set) Token: 0x0600055A RID: 1370 RVA: 0x0002BF28 File Offset: 0x0002A128
		public Vector2? OverrideMovement { get; set; }

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x0600055B RID: 1371 RVA: 0x0002BF31 File Offset: 0x0002A131
		// (set) Token: 0x0600055C RID: 1372 RVA: 0x0002BF39 File Offset: 0x0002A139
		public bool ForceRun { get; set; }

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x0600055D RID: 1373 RVA: 0x0002BF42 File Offset: 0x0002A142
		public bool IsClimbing
		{
			get
			{
				return this.AnimController.IsClimbing;
			}
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0002BF50 File Offset: 0x0002A150
		public Vector2 GetTargetMovement()
		{
			Vector2 targetMovement = Vector2.Zero;
			if (this.OverrideMovement != null)
			{
				targetMovement = this.OverrideMovement.Value;
			}
			else
			{
				if (this.IsKeyDown(InputType.Left))
				{
					targetMovement.X -= 1f;
				}
				if (this.IsKeyDown(InputType.Right))
				{
					targetMovement.X += 1f;
				}
				if (this.IsKeyDown(InputType.Up))
				{
					targetMovement.Y += 1f;
				}
				if (this.IsKeyDown(InputType.Down))
				{
					targetMovement.Y -= 1f;
				}
			}
			bool run = false;
			if ((this.IsKeyDown(InputType.Run) && this.AnimController.ForceSelectAnimationType == AnimationType.NotDefined) || this.ForceRun)
			{
				run = this.CanRun;
			}
			return this.ApplyMovementLimits(targetMovement, this.AnimController.GetCurrentSpeed(run));
		}

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x0600055F RID: 1375 RVA: 0x0002C024 File Offset: 0x0002A224
		public bool CanRun
		{
			get
			{
				if (!this.DisableRunning && this.CanRunWhileDragging())
				{
					HumanoidAnimController humanoidAnimController = this.AnimController as HumanoidAnimController;
					if ((humanoidAnimController == null || !humanoidAnimController.Crouching) && !this.AnimController.IsMovingBackwards && !this.HasAbilityFlag(AbilityFlags.MustWalk))
					{
						return !this.AnimController.IsHoldingToRope;
					}
				}
				return false;
			}
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000560 RID: 1376 RVA: 0x0002C07E File Offset: 0x0002A27E
		// (set) Token: 0x06000561 RID: 1377 RVA: 0x0002C097 File Offset: 0x0002A297
		public bool DisableRunning
		{
			get
			{
				return this.disableRunningLastSet > Timing.TotalTime - 0.1;
			}
			set
			{
				if (value)
				{
					this.disableRunningLastSet = Timing.TotalTime;
				}
			}
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x0002C0A8 File Offset: 0x0002A2A8
		public bool CanRunWhileDragging()
		{
			Character character = this.selectedCharacter;
			return character == null || !character.IsDraggable || ((this.selectedCharacter.IsIncapacitated || this.selectedCharacter.Stun > 0f) && this.HasAbilityFlag(AbilityFlags.MoveNormallyWhileDragging));
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x0002C0F4 File Offset: 0x0002A2F4
		public Vector2 ApplyMovementLimits(Vector2 targetMovement, float currentSpeed)
		{
			if (this.AnimController.InWater)
			{
				float length = targetMovement.Length();
				if (length > 0f)
				{
					targetMovement /= length;
				}
			}
			targetMovement *= currentSpeed;
			float maxSpeed = this.ApplyTemporarySpeedLimits(currentSpeed);
			targetMovement.X = MathHelper.Clamp(targetMovement.X, -maxSpeed, maxSpeed);
			targetMovement.Y = MathHelper.Clamp(targetMovement.Y, -maxSpeed, maxSpeed);
			this.SpeedMultiplier = Math.Max(0f, this.greatestPositiveSpeedMultiplier - (1f - this.greatestNegativeSpeedMultiplier));
			targetMovement *= this.SpeedMultiplier;
			this.ResetSpeedMultiplier();
			return targetMovement;
		}

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06000564 RID: 1380 RVA: 0x0002C19A File Offset: 0x0002A39A
		// (set) Token: 0x06000565 RID: 1381 RVA: 0x0002C1A2 File Offset: 0x0002A3A2
		public float SpeedMultiplier { get; private set; } = 1f;

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06000566 RID: 1382 RVA: 0x0002C1AB File Offset: 0x0002A3AB
		// (set) Token: 0x06000567 RID: 1383 RVA: 0x0002C1B3 File Offset: 0x0002A3B3
		public float PropulsionSpeedMultiplier
		{
			get
			{
				return this.propulsionSpeedMultiplier;
			}
			set
			{
				this.propulsionSpeedMultiplier = value;
				this.propulsionSpeedMultiplierLastSet = Timing.TotalTime;
			}
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x0002C1C7 File Offset: 0x0002A3C7
		public void StackSpeedMultiplier(float val)
		{
			this.greatestNegativeSpeedMultiplier = Math.Min(val, this.greatestNegativeSpeedMultiplier);
			this.greatestPositiveSpeedMultiplier = Math.Max(val, this.greatestPositiveSpeedMultiplier);
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x0002C1ED File Offset: 0x0002A3ED
		public void ResetSpeedMultiplier()
		{
			this.greatestPositiveSpeedMultiplier = 1f;
			this.greatestNegativeSpeedMultiplier = 1f;
			if (Timing.TotalTime > this.propulsionSpeedMultiplierLastSet + 0.1)
			{
				this.propulsionSpeedMultiplier = 1f;
			}
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x0600056A RID: 1386 RVA: 0x0002C227 File Offset: 0x0002A427
		// (set) Token: 0x0600056B RID: 1387 RVA: 0x0002C22F File Offset: 0x0002A42F
		public float HealthMultiplier { get; private set; } = 1f;

		// Token: 0x0600056C RID: 1388 RVA: 0x0002C238 File Offset: 0x0002A438
		public void StackHealthMultiplier(float val)
		{
			this.greatestNegativeHealthMultiplier = Math.Min(val, this.greatestNegativeHealthMultiplier);
			this.greatestPositiveHealthMultiplier = Math.Max(val, this.greatestPositiveHealthMultiplier);
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x0002C25E File Offset: 0x0002A45E
		private void CalculateHealthMultiplier()
		{
			this.HealthMultiplier = this.greatestPositiveHealthMultiplier - (1f - this.greatestNegativeHealthMultiplier);
			this.greatestPositiveHealthMultiplier = 1f;
			this.greatestNegativeHealthMultiplier = 1f;
		}

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x0600056E RID: 1390 RVA: 0x0002C28F File Offset: 0x0002A48F
		// (set) Token: 0x0600056F RID: 1391 RVA: 0x0002C297 File Offset: 0x0002A497
		public float HumanPrefabHealthMultiplier { get; private set; } = 1f;

		// Token: 0x06000570 RID: 1392 RVA: 0x0002C2A0 File Offset: 0x0002A4A0
		public float GetTemporarySpeedReduction()
		{
			if (!this.Params.Health.ApplyMovementPenalties)
			{
				return 0f;
			}
			float reduction = 0f;
			reduction = this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.RightFoot, false, false, false), reduction, 0.8f);
			reduction = this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.LeftFoot, false, false, false), reduction, 0.8f);
			if (this.AnimController is HumanoidAnimController)
			{
				if (this.AnimController.InWater)
				{
					reduction = this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.RightHand, false, false, false), reduction, 0.8f);
					reduction = this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.LeftHand, false, false, false), reduction, 0.8f);
				}
			}
			else
			{
				int totalTailLimbs = 0;
				int destroyedTailLimbs = 0;
				foreach (Limb limb in this.AnimController.Limbs)
				{
					if (limb.type == LimbType.Tail)
					{
						totalTailLimbs++;
						if (limb.IsSevered)
						{
							destroyedTailLimbs++;
						}
					}
				}
				if (destroyedTailLimbs > 0)
				{
					reduction += MathHelper.Lerp(0f, this.AnimController.InWater ? 1f : 0.5f, (float)destroyedTailLimbs / (float)totalTailLimbs);
				}
			}
			return Math.Clamp(reduction, 0f, 1f);
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0002C3E0 File Offset: 0x0002A5E0
		private float CalculateMovementPenalty(Limb limb, float sum, float max = 0.8f)
		{
			if (!this.Params.Health.ApplyMovementPenalties)
			{
				return 0f;
			}
			if (limb != null)
			{
				sum += MathHelper.Lerp(0f, max, this.CharacterHealth.GetLimbDamage(limb, AfflictionPrefab.DamageType));
			}
			return Math.Clamp(sum, 0f, 1f);
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0002C438 File Offset: 0x0002A638
		public float GetRightHandPenalty()
		{
			return this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.RightHand, false, false, false), 0f, 1f);
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x0002C459 File Offset: 0x0002A659
		public float GetLeftHandPenalty()
		{
			return this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.LeftHand, false, false, false), 0f, 1f);
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x0002C47C File Offset: 0x0002A67C
		public float GetLegPenalty(float startSum = 0f)
		{
			float sum = startSum;
			foreach (Limb limb in this.AnimController.Limbs)
			{
				LimbType type = limb.type;
				if (type - LimbType.LeftFoot <= 1)
				{
					sum += this.CalculateMovementPenalty(limb, sum, 0.5f);
				}
			}
			return Math.Clamp(sum, 0f, 1f);
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0002C4DC File Offset: 0x0002A6DC
		public float ApplyTemporarySpeedLimits(float speed)
		{
			float max;
			if (this.AnimController is HumanoidAnimController)
			{
				max = (this.AnimController.InWater ? 0.5f : 0.8f);
			}
			else
			{
				max = (this.AnimController.InWater ? 0.9f : 0.5f);
			}
			speed *= 1f - MathHelper.Lerp(0f, max, this.GetTemporarySpeedReduction());
			return speed;
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x0002C548 File Offset: 0x0002A748
		public void Control(float deltaTime, Camera cam)
		{
			this.ViewTarget = null;
			if (!this.AllowInput)
			{
				return;
			}
			if (Character.Controlled == this || (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer))
			{
				this.SmoothedCursorPosition = this.cursorPosition;
			}
			else
			{
				Vector2 smoothedCursorDiff = this.cursorPosition - this.SmoothedCursorPosition;
				smoothedCursorDiff = NetConfig.InterpolateCursorPositionError(smoothedCursorDiff);
				this.SmoothedCursorPosition = this.cursorPosition - smoothedCursorDiff;
			}
			bool aiControlled = this is AICharacter && Character.Controlled != this && !this.IsRemotePlayer;
			NetworkMember networkMember = GameMain.NetworkMember;
			bool controlledByServer = networkMember != null && networkMember.IsClient && this.IsRemotelyControlled;
			if (!aiControlled && !controlledByServer)
			{
				Vector2 targetMovement = this.GetTargetMovement();
				this.AnimController.TargetMovement = targetMovement;
				Item selectedItem = this.SelectedItem;
				Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
				if (controller == null || !controller.ControlCharacterPose)
				{
					this.AnimController.IgnorePlatforms = (this.AnimController.TargetMovement.Y < -0.1f);
				}
			}
			HumanoidAnimController humanAnimController = this.AnimController as HumanoidAnimController;
			if (humanAnimController != null)
			{
				humanAnimController.Crouching = (humanAnimController.ForceSelectAnimationType == AnimationType.Crouch || this.IsKeyDown(InputType.Crouch));
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					humanAnimController.ForceSelectAnimationType = AnimationType.NotDefined;
				}
			}
			if (!aiControlled && !this.AnimController.IsUsingItem && this.AnimController.Anim != AnimController.Animation.CPR && (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient || Character.Controlled == this) && ((!this.IsClimbing && this.AnimController.OnGround) || (this.IsClimbing && this.IsKeyDown(InputType.Aim))) && !this.AnimController.InWater)
			{
				if (!this.FollowCursor)
				{
					this.AnimController.TargetDir = Direction.Right;
				}
				else if (this.AnimController is HumanoidAnimController)
				{
					if (this.CursorPosition.X < this.AnimController.Collider.Position.X - 40f)
					{
						this.AnimController.TargetDir = Direction.Left;
					}
					else if (this.CursorPosition.X > this.AnimController.Collider.Position.X + 40f)
					{
						this.AnimController.TargetDir = Direction.Right;
					}
				}
			}
			if (aiControlled && this.Stun <= 0f && !this.IsKnockedDownOrRagdolled && !this.LockHands && this.ShouldAvoidStayingAttachedToController())
			{
				this.SelectedItem = null;
			}
			if (GameMain.NetworkMember != null)
			{
				if (GameMain.NetworkMember.IsServer)
				{
					if (!aiControlled)
					{
						if (this.dequeuedInput.HasFlag(Character.InputNetFlags.FacingLeft))
						{
							this.AnimController.TargetDir = Direction.Left;
						}
						else
						{
							this.AnimController.TargetDir = Direction.Right;
						}
					}
				}
				else if (GameMain.NetworkMember.IsClient && Character.Controlled != this && this.memState.Count > 0)
				{
					this.AnimController.TargetDir = this.memState[0].Direction;
				}
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && Character.Controlled != this && this.IsKeyDown(InputType.Aim))
			{
				Limb attackLimb2 = this.currentAttackTarget.AttackLimb;
				Attack attack = (attackLimb2 != null) ? attackLimb2.attack : null;
				if (attack != null && attack.Ranged)
				{
					EnemyAIController enemyAi = this.AIController as EnemyAIController;
					if (enemyAi != null)
					{
						enemyAi.AimRangedAttack(attack, this.currentAttackTarget.DamageTarget as Entity);
					}
				}
			}
			if (this.attackCoolDown > 0f)
			{
				this.attackCoolDown -= deltaTime;
			}
			else if (this.IsKeyDown(InputType.Attack) && !this.IsAttachedToController())
			{
				if (this.IsPlayer)
				{
					float dist = -1f;
					Vector2 attackPos = this.SimPosition + ConvertUnits.ToSimUnits(this.cursorPosition - this.Position);
					List<Body> ignoredBodies = (from l in this.AnimController.Limbs
					select l.body.FarseerBody).ToList<Body>();
					ignoredBodies.Add(this.AnimController.Collider.FarseerBody);
					Body body = Submarine.PickBody(this.SimPosition, attackPos, ignoredBodies, new Category?(Category.Cat1 | Category.Cat2), true, null, false);
					IDamageable attackTarget = null;
					if (body != null)
					{
						attackPos = Submarine.LastPickedPosition;
						Submarine sub = body.UserData as Submarine;
						if (sub != null)
						{
							body = Submarine.PickBody(this.SimPosition - ((Submarine)body.UserData).SimPosition, attackPos - ((Submarine)body.UserData).SimPosition, ignoredBodies, new Category?(Category.Cat1), true, null, false);
							if (body != null)
							{
								attackPos = Submarine.LastPickedPosition + sub.SimPosition;
								attackTarget = (body.UserData as IDamageable);
							}
						}
						else
						{
							IDamageable damageable = body.UserData as IDamageable;
							if (damageable != null)
							{
								attackTarget = damageable;
							}
							else
							{
								Limb limb = body.UserData as Limb;
								if (limb != null)
								{
									attackTarget = limb.character;
								}
							}
						}
					}
					IEnumerable<AttackContext> currentContexts = this.GetAttackContexts();
					IEnumerable<Limb> attackLimbs = from l in this.AnimController.Limbs
					where l.attack != null
					select l;
					bool hasAttacksWithoutRootForce = attackLimbs.Any((Limb l) => !l.attack.HasRootForce);
					IEnumerable<Limb> validLimbs = attackLimbs.Where(delegate(Limb l)
					{
						if (l.IsSevered || l.IsStuck)
						{
							return false;
						}
						if (l.Disabled)
						{
							return false;
						}
						Attack attack2 = l.attack;
						if (attack2.CoolDownTimer > 0f)
						{
							return false;
						}
						if (hasAttacksWithoutRootForce && attack2.HasRootForce)
						{
							return false;
						}
						if (!attack2.IsValidContext(currentContexts))
						{
							return false;
						}
						if (attackTarget != null)
						{
							if (!attack2.IsValidTarget(attackTarget as Entity))
							{
								return false;
							}
							ISerializableEntity se = attackTarget as ISerializableEntity;
							if (se != null && se is Character && attack2.Conditionals.Any((PropertyConditional c) => !c.TargetSelf && !c.Matches(se)))
							{
								return false;
							}
						}
						return !attack2.Conditionals.Any((PropertyConditional c) => c.TargetSelf && !c.Matches(this));
					});
					IOrderedEnumerable<Limb> sortedLimbs = from l in validLimbs
					orderby Vector2.DistanceSquared(ConvertUnits.ToDisplayUnits(l.SimPosition), this.cursorPosition)
					select l;
					Limb attackLimb = sortedLimbs.FirstOrDefault<Limb>();
					if (attackLimb != null)
					{
						Character targetCharacter = attackTarget as Character;
						if (targetCharacter != null)
						{
							dist = ConvertUnits.ToDisplayUnits(Vector2.Distance(Submarine.LastPickedPosition, attackLimb.SimPosition));
							foreach (Limb limb2 in targetCharacter.AnimController.Limbs)
							{
								if (!limb2.IsSevered && !limb2.Removed)
								{
									float tempDist = ConvertUnits.ToDisplayUnits(Vector2.Distance(limb2.SimPosition, attackLimb.SimPosition));
									if (tempDist < dist)
									{
										dist = tempDist;
									}
								}
							}
						}
						AttackResult attackResult;
						attackLimb.UpdateAttack(deltaTime, attackPos, attackTarget, out attackResult, dist, null);
						if (!attackLimb.attack.IsRunning)
						{
							this.attackCoolDown = 1f;
						}
					}
				}
				else
				{
					networkMember = GameMain.NetworkMember;
					if (networkMember != null && networkMember.IsClient && Character.Controlled != this)
					{
						Entity entity = this.currentAttackTarget.DamageTarget as Entity;
						if (entity != null && entity.Removed)
						{
							this.currentAttackTarget = default(Character.AttackTargetData);
						}
						Limb attackLimb3 = this.currentAttackTarget.AttackLimb;
						if (attackLimb3 != null)
						{
							AttackResult attackResult2;
							attackLimb3.UpdateAttack(deltaTime, this.currentAttackTarget.AttackPos, this.currentAttackTarget.DamageTarget, out attackResult2, -1f, null);
						}
					}
				}
			}
			if (this.Inventory != null)
			{
				if (this.IsKeyHit(InputType.DropItem))
				{
					Screen selected = Screen.Selected;
					if (selected != null && !selected.IsEditor && CharacterHUD.ShouldDrawInventory(this))
					{
						foreach (Item item in this.HeldItems)
						{
							if (this.CanInteractWith(item, true))
							{
								Item selectedItem2 = this.SelectedItem;
								if (((selectedItem2 != null) ? selectedItem2.OwnInventory : null) != null && !this.SelectedItem.OwnInventory.Locked && this.SelectedItem.OwnInventory.CanBePut(item))
								{
									this.SelectedItem.OwnInventory.TryPutItem(item, this, null, true, false, true);
									break;
								}
								item.Drop(this, true, true);
								break;
							}
						}
					}
				}
				if (Character.<Control>g__CanUseItemsWhenSelected|642_0(this.SelectedItem) && Character.<Control>g__CanUseItemsWhenSelected|642_0(this.SelectedSecondaryItem))
				{
					foreach (Item item2 in this.HeldItems)
					{
						this.<Control>g__tryUseItem|642_1(item2, deltaTime);
					}
					foreach (Item item3 in this.Inventory.AllItems)
					{
						Wearable component = item3.GetComponent<Wearable>();
						if (component != null && component.AllowUseWhenWorn && this.HasEquippedItem(item3, null, null))
						{
							this.<Control>g__tryUseItem|642_1(item3, deltaTime);
						}
					}
				}
			}
			if (this.SelectedItem != null)
			{
				this.<Control>g__tryUseItem|642_1(this.SelectedItem, deltaTime);
			}
			if (this.SelectedCharacter != null && (!this.SelectedCharacter.CanBeSelected || (Vector2.DistanceSquared(this.SelectedCharacter.WorldPosition, this.WorldPosition) > 40000f && this.SelectedCharacter.GetDistanceToClosestLimb(this.GetRelativeSimPosition(this.selectedCharacter, new Vector2?(this.WorldPosition))) > ConvertUnits.ToSimUnits(200f))))
			{
				this.DeselectCharacter();
			}
			if (this.IsRemotelyControlled && this.keys != null)
			{
				foreach (Key key in this.keys)
				{
					key.ResetHit();
				}
			}
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0002CEE0 File Offset: 0x0002B0E0
		public void SetAttackTarget(Limb attackLimb, IDamageable damageTarget, Vector2 attackPos)
		{
			this.currentAttackTarget = new Character.AttackTargetData
			{
				AttackLimb = attackLimb,
				DamageTarget = damageTarget,
				AttackPos = attackPos
			};
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0002CF14 File Offset: 0x0002B114
		private Limb GetSeeingLimb()
		{
			Limb result;
			if ((result = this.AnimController.GetLimb(LimbType.Head, true, false, false)) == null)
			{
				result = (this.AnimController.GetLimb(LimbType.Torso, true, false, false) ?? this.AnimController.MainLimb);
			}
			return result;
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x0002CF4C File Offset: 0x0002B14C
		public bool CanSeeTarget(ISpatialEntity target, ISpatialEntity seeingEntity = null, bool seeThroughWindows = false, bool checkFacing = false)
		{
			if (seeingEntity == null)
			{
				ISpatialEntity spatialEntity;
				if (!this.AnimController.SimplePhysicsEnabled)
				{
					ISpatialEntity seeingLimb = this.GetSeeingLimb();
					spatialEntity = seeingLimb;
				}
				else
				{
					spatialEntity = this;
				}
				seeingEntity = spatialEntity;
			}
			Character targetCharacter = target as Character;
			if (targetCharacter != null)
			{
				return ISpatialEntity.IsCharacterVisible(targetCharacter, seeingEntity, seeThroughWindows, checkFacing);
			}
			return ISpatialEntity.CheckVisibility(target, seeingEntity, seeThroughWindows, checkFacing);
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x0002CF98 File Offset: 0x0002B198
		public bool IsFacing(Vector2 targetWorldPos)
		{
			return (this.AnimController.Dir > 0f && targetWorldPos.X > this.WorldPosition.X) || (this.AnimController.Dir < 0f && targetWorldPos.X < this.WorldPosition.X);
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x0002CFF3 File Offset: 0x0002B1F3
		public bool HasItem(Item item, bool requireEquipped = false, InvSlotType? slotType = null)
		{
			if (!requireEquipped)
			{
				return item.IsOwnedBy(this);
			}
			return this.HasEquippedItem(item, slotType, null);
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x0002D00C File Offset: 0x0002B20C
		public bool HasEquippedItem(Item item, InvSlotType? slotType = null, Func<InvSlotType, bool> predicate = null)
		{
			if (this.Inventory == null)
			{
				return false;
			}
			for (int i = 0; i < this.Inventory.Capacity; i++)
			{
				InvSlotType slot = this.Inventory.SlotTypes[i];
				if (predicate == null || predicate(slot))
				{
					if (slotType != null)
					{
						if (!slotType.Value.HasFlag(slot))
						{
							goto IL_61;
						}
					}
					else if (slot == InvSlotType.Any)
					{
						goto IL_61;
					}
					if (this.Inventory.GetItemAt(i) == item)
					{
						return true;
					}
				}
				IL_61:;
			}
			return false;
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x0002D090 File Offset: 0x0002B290
		public bool HasEquippedItem(Identifier tagOrIdentifier, bool allowBroken = true, InvSlotType? slotType = null)
		{
			if (this.Inventory == null)
			{
				return false;
			}
			int i = 0;
			while (i < this.Inventory.Capacity)
			{
				if (slotType != null)
				{
					if (slotType.Value.HasFlag(this.Inventory.SlotTypes[i]))
					{
						goto IL_51;
					}
				}
				else if (this.Inventory.SlotTypes[i] != InvSlotType.Any)
				{
					goto IL_51;
				}
				IL_90:
				i++;
				continue;
				IL_51:
				Item item = this.Inventory.GetItemAt(i);
				if (item != null && (allowBroken || item.Condition > 0f) && (item.Prefab.Identifier == tagOrIdentifier || item.HasTag(tagOrIdentifier)))
				{
					return true;
				}
				goto IL_90;
			}
			return false;
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0002D144 File Offset: 0x0002B344
		public Item GetEquippedItem(Identifier tagOrIdentifier = default(Identifier), InvSlotType? slotType = null)
		{
			if (this.Inventory == null)
			{
				return null;
			}
			int i = 0;
			while (i < this.Inventory.Capacity)
			{
				if (slotType != null)
				{
					if (slotType.Value.HasFlag(this.Inventory.SlotTypes[i]))
					{
						goto IL_4E;
					}
				}
				else if (this.Inventory.SlotTypes[i] != InvSlotType.Any)
				{
					goto IL_4E;
				}
				IL_86:
				i++;
				continue;
				IL_4E:
				Item item = this.Inventory.GetItemAt(i);
				if (item != null && (tagOrIdentifier.IsEmpty || item.Prefab.Identifier == tagOrIdentifier || item.HasTag(tagOrIdentifier)))
				{
					return item;
				}
				goto IL_86;
			}
			return null;
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x0002D1F0 File Offset: 0x0002B3F0
		public bool HasHandsFull([TupleElementNames(new string[]
		{
			"leftHandItem",
			"rightHandItem"
		})] out ValueTuple<Item, Item> items)
		{
			InvSlotType? slotType = new InvSlotType?(InvSlotType.LeftHand);
			Item leftHandItem = this.GetEquippedItem(default(Identifier), slotType);
			slotType = new InvSlotType?(InvSlotType.RightHand);
			Item rightHandItem = this.GetEquippedItem(default(Identifier), slotType);
			items = new ValueTuple<Item, Item>(leftHandItem, rightHandItem);
			return leftHandItem != null && rightHandItem != null;
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0002D24A File Offset: 0x0002B44A
		public bool TryPutItem(Item item, IEnumerable<InvSlotType> allowedSlots)
		{
			return this.Inventory.TryPutItem(item, this, allowedSlots, true, false, true);
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x0002D25D File Offset: 0x0002B45D
		public bool TryPutItemInBag(Item item)
		{
			return item != null && item.AllowedSlots.Contains(InvSlotType.Bag) && this.TryPutItem(item, CharacterInventory.BagSlot);
		}

		// Token: 0x06000582 RID: 1410 RVA: 0x0002D282 File Offset: 0x0002B482
		public bool TryPutItemInAnySlot(Item item)
		{
			return item != null && item.AllowedSlots.Contains(InvSlotType.Any) && this.TryPutItem(item, CharacterInventory.AnySlot);
		}

		// Token: 0x06000583 RID: 1411 RVA: 0x0002D2A4 File Offset: 0x0002B4A4
		public bool Unequip(Item item)
		{
			if (!this.HasEquippedItem(item, null, null))
			{
				return false;
			}
			if (!item.IsInteractable(this))
			{
				return false;
			}
			if (!this.TryPutItemInAnySlot(item) && !this.TryPutItemInBag(item))
			{
				item.Drop(this, true, true);
			}
			return true;
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x0002D2F0 File Offset: 0x0002B4F0
		public bool CanAccessInventory(Inventory inventory, CharacterInventory.AccessLevel accessLevel = CharacterInventory.AccessLevel.AllowBotsAndPets)
		{
			if (!this.CanInteract || inventory.Locked)
			{
				return false;
			}
			Character inventoryOwner = inventory.Owner as Character;
			if (inventoryOwner != null)
			{
				return inventoryOwner.IsInventoryAccessibleTo(this, accessLevel) && (inventoryOwner == this || this.CanInteractWith(inventoryOwner, 200f, true, false));
			}
			Item item = inventory.Owner as Item;
			if (item != null)
			{
				if (!this.CanInteractWith(item, true))
				{
					foreach (MapEntity linkedEntity in item.linkedTo)
					{
						Item linkedItem = linkedEntity as Item;
						if (linkedItem != null && linkedItem.DisplaySideBySideWhenLinked && this.CanInteractWith(linkedItem, true))
						{
							return true;
						}
					}
					return false;
				}
				ItemInventory itemInventory = inventory as ItemInventory;
				ItemContainer container = (itemInventory != null) ? itemInventory.Container : null;
				if (container != null && !container.HasRequiredItems(this, false, null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x0002D3EC File Offset: 0x0002B5EC
		public bool CanBeHealedBy(Character character, bool checkFriendlyTeam = true)
		{
			return !character.IsClimbing && !this.DisableHealthWindow && this.UseHealthWindow && character.CanInteract && (!checkFriendlyTeam || this.IsFriendly(character) || this.CanBeDraggedBy(character)) && character.CanInteractWith(this, 160f, false, false);
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x0002D440 File Offset: 0x0002B640
		public bool CanBeDraggedBy(Character character)
		{
			return this.IsDraggable && (this.IsKnockedDownOrRagdolled || this.LockHands || (this.IsPet && character.IsOnFriendlyTeam(this)) || (this.IsBot && character.TeamID == this.TeamID));
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x0002D494 File Offset: 0x0002B694
		public bool IsInventoryAccessibleTo(Character character, CharacterInventory.AccessLevel accessLevel = CharacterInventory.AccessLevel.AllowBotsAndPets)
		{
			Character.<>c__DisplayClass661_0 CS$<>8__locals1;
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			if (base.Removed || this.Inventory == null)
			{
				return false;
			}
			if (!this.Inventory.AccessibleWhenAlive && !this.IsDead)
			{
				return CS$<>8__locals1.character == this && this.Inventory.AccessibleByOwner;
			}
			if (CS$<>8__locals1.character == this)
			{
				return true;
			}
			if (this.IsKnockedDownOrRagdolled || this.LockHands)
			{
				return true;
			}
			bool result;
			switch (accessLevel)
			{
			case CharacterInventory.AccessLevel.OnlyIfIncapacitated:
				result = false;
				break;
			case CharacterInventory.AccessLevel.AllowBotsAndPets:
				result = ((this.IsBot && this.<IsInventoryAccessibleTo>g__IsOnSameTeam|661_0(ref CS$<>8__locals1)) || this.<IsInventoryAccessibleTo>g__IsFriendlyPet|661_1(ref CS$<>8__locals1));
				break;
			case CharacterInventory.AccessLevel.AllowFriendly:
				result = (this.<IsInventoryAccessibleTo>g__IsOnSameTeam|661_0(ref CS$<>8__locals1) || this.<IsInventoryAccessibleTo>g__IsFriendlyPet|661_1(ref CS$<>8__locals1));
				break;
			default:
				throw new NotImplementedException();
			}
			return result;
		}

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000588 RID: 1416 RVA: 0x0002D564 File Offset: 0x0002B764
		private Stopwatch StopWatch
		{
			get
			{
				Stopwatch result;
				if ((result = this.sw) == null)
				{
					result = (this.sw = new Stopwatch());
				}
				return result;
			}
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x0002D58C File Offset: 0x0002B78C
		public bool FindItem(ref int itemIndex, out Item targetItem, IEnumerable<Identifier> identifiers = null, bool ignoreBroken = true, IEnumerable<Item> ignoredItems = null, IEnumerable<Identifier> ignoredContainerIdentifiers = null, Func<Item, bool> customPredicate = null, Func<Item, float> customPriorityFunction = null, float maxItemDistance = 10000f, ISpatialEntity positionalReference = null)
		{
			if (HumanAIController.DebugAI)
			{
				this.StopWatch.Restart();
			}
			if (itemIndex == 0)
			{
				this._foundItem = null;
				this._selectedItemPriority = 0f;
			}
			int itemsPerFrame = this.IsOnPlayerTeam ? 100 : 10;
			int checkedItemCount = 0;
			int i = 0;
			while (i < itemsPerFrame && itemIndex < Item.ItemList.Count)
			{
				checkedItemCount++;
				Item item = Item.ItemList[itemIndex];
				if (item.IsInteractable(this) && (ignoredItems == null || !ignoredItems.Contains(item)) && item.Submarine != null && item.Submarine.TeamID == this.TeamID && item.CurrentHull != null && (!ignoreBroken || item.Condition > 0f) && (base.Submarine == null || base.Submarine.IsEntityFoundOnThisSub(item, true, false, false)) && (customPredicate == null || customPredicate(item)) && (identifiers == null || !identifiers.None((Identifier id) => item.Prefab.Identifier == id || item.HasTag(id))) && (ignoredContainerIdentifiers == null || item.Container == null || !ignoredContainerIdentifiers.Contains(item.ContainerIdentifier)) && !this.IsItemTakenBySomeoneElse(item))
				{
					Entity rootInventoryOwner = item.GetRootInventoryOwner();
					Item ownerItem = rootInventoryOwner as Item;
					if (ownerItem == null || ownerItem.IsInteractable(this))
					{
						float itemPriority = (customPriorityFunction != null) ? customPriorityFunction(item) : 1f;
						if (itemPriority > 0f)
						{
							Vector2 itemPos = (rootInventoryOwner ?? item).WorldPosition;
							Vector2 refPos = (positionalReference != null) ? positionalReference.WorldPosition : this.WorldPosition;
							float distanceFactor = AIObjective.GetDistanceFactor(refPos, itemPos, 0f, 5f, maxItemDistance, 1f);
							itemPriority *= distanceFactor;
							if (itemPriority > this._selectedItemPriority)
							{
								this._selectedItemPriority = itemPriority;
								this._foundItem = item;
							}
						}
					}
				}
				i++;
				itemIndex++;
			}
			targetItem = this._foundItem;
			bool completed = itemIndex >= Item.ItemList.Count - 1;
			if (HumanAIController.DebugAI && checkedItemCount > 0 && targetItem != null && this.StopWatch.ElapsedMilliseconds > 1L)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 5);
				defaultInterpolatedStringHandler.AppendLiteral("Went through ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(checkedItemCount);
				defaultInterpolatedStringHandler.AppendLiteral(" of total ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(Item.ItemList.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" items. Found item ");
				defaultInterpolatedStringHandler.AppendFormatted(targetItem.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" in ");
				defaultInterpolatedStringHandler.AppendFormatted<long>(this.StopWatch.ElapsedMilliseconds);
				defaultInterpolatedStringHandler.AppendLiteral(" ms. Completed: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(completed);
				string msg = defaultInterpolatedStringHandler.ToStringAndClear();
				if (this.StopWatch.ElapsedMilliseconds > 5L)
				{
					DebugConsole.ThrowError(msg, null, null, false, false);
				}
				else
				{
					DebugConsole.AddWarning(msg, null);
				}
			}
			return completed;
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x0002D8D9 File Offset: 0x0002BAD9
		public bool IsItemTakenBySomeoneElse(Item item)
		{
			return item.FindParentInventory(delegate(Inventory i)
			{
				if (i.Owner != this)
				{
					Character owner = i.Owner as Character;
					if (owner != null && !owner.IsDead)
					{
						return !owner.Removed;
					}
				}
				return false;
			}) != null;
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x0002D8F0 File Offset: 0x0002BAF0
		public bool CanInteractWith(Character c, float maxDist = 200f, bool checkVisibility = true, bool skipDistanceCheck = false)
		{
			if (c == this || base.Removed || !c.Enabled || !c.CanBeSelected || c.InvisibleTimer > 0f)
			{
				return false;
			}
			if (!c.CharacterHealth.UseHealthWindow && !c.IsDraggable && (c.onCustomInteract == null || !c.AllowCustomInteract))
			{
				return false;
			}
			if (!skipDistanceCheck)
			{
				maxDist = Math.Max(ConvertUnits.ToSimUnits(maxDist), c.AnimController.Collider.GetMaxExtent());
				if (Vector2.DistanceSquared(this.SimPosition, c.SimPosition) > maxDist * maxDist && Vector2.DistanceSquared(this.SimPosition, c.AnimController.MainLimb.SimPosition) > maxDist * maxDist)
				{
					return false;
				}
			}
			return !checkVisibility || this.CanSeeTarget(c, null, false, false);
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x0002D9B8 File Offset: 0x0002BBB8
		public bool CanInteractWith(Item item, bool checkLinked = true)
		{
			float num;
			return this.CanInteractWith(item, out num, checkLinked);
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x0002D9D0 File Offset: 0x0002BBD0
		public bool CanInteractWith(Item item, out float distanceToItem, bool checkLinked)
		{
			distanceToItem = -1f;
			bool hidden = item.IsHidden;
			if (Screen.Selected == GameMain.SubEditorScreen)
			{
				hidden = false;
			}
			Controller controller = item.GetComponent<Controller>();
			if (controller != null && this.IsAnySelectedItem(item) && controller.IsAttachedUser(this))
			{
				return true;
			}
			if (!this.CanInteract || hidden || !item.IsInteractable(this))
			{
				return false;
			}
			if (item.ParentInventory != null)
			{
				return this.CanAccessInventory(item.ParentInventory, CharacterInventory.AccessLevel.AllowBotsAndPets);
			}
			Wire wire = item.GetComponent<Wire>();
			if (wire != null && item.GetComponent<ConnectionPanel>() == null)
			{
				if (wire.Locked)
				{
					return false;
				}
				if (wire.HiddenInGame && Screen.Selected == GameMain.GameScreen)
				{
					return false;
				}
				Connection connection = wire.Connections[0];
				if (((connection != null) ? connection.Item : null) != null && this.SelectedItem == wire.Connections[0].Item)
				{
					return wire.Connections[1] == null;
				}
				Connection connection2 = wire.Connections[1];
				if (((connection2 != null) ? connection2.Item : null) != null && this.SelectedItem == wire.Connections[1].Item)
				{
					return wire.Connections[0] == null;
				}
				Item selectedItem = this.SelectedItem;
				bool? flag;
				if (selectedItem == null)
				{
					flag = null;
				}
				else
				{
					ConnectionPanel component = selectedItem.GetComponent<ConnectionPanel>();
					flag = ((component != null) ? new bool?(component.DisconnectedWires.Contains(wire)) : null);
				}
				bool? flag2 = flag;
				if (flag2.GetValueOrDefault())
				{
					return wire.Connections[0] == null && wire.Connections[1] == null;
				}
			}
			if (checkLinked && item.DisplaySideBySideWhenLinked)
			{
				foreach (MapEntity linked in item.linkedTo)
				{
					Item linkedItem = linked as Item;
					if (linkedItem != null)
					{
						Inventory parentInventory = linkedItem.ParentInventory;
						float distToLinked;
						if (((parentInventory != null) ? parentInventory.Owner : null) != item && this.CanInteractWith(linkedItem, out distToLinked, false))
						{
							distanceToItem = distToLinked;
							return true;
						}
					}
				}
			}
			if (item.InteractDistance == 0f && !item.Prefab.Triggers.Any<Rectangle>())
			{
				return false;
			}
			Pickable pickableComponent = item.GetComponent<Pickable>();
			if (pickableComponent != null && pickableComponent.Picker != this && pickableComponent.Picker != null && !pickableComponent.Picker.IsDead)
			{
				return false;
			}
			Item selectedItem2 = this.SelectedItem;
			Item item2;
			if (selectedItem2 == null)
			{
				item2 = null;
			}
			else
			{
				RemoteController component2 = selectedItem2.GetComponent<RemoteController>();
				item2 = ((component2 != null) ? component2.TargetItem : null);
			}
			if (item2 == item)
			{
				return true;
			}
			CharacterInventory inventory = this.Inventory;
			Item heldItem = (inventory != null) ? inventory.GetItemInLimbSlot(InvSlotType.RightHand) : null;
			Item item3;
			if (heldItem == null)
			{
				item3 = null;
			}
			else
			{
				RemoteController component3 = heldItem.GetComponent<RemoteController>();
				item3 = ((component3 != null) ? component3.TargetItem : null);
			}
			if (item3 == item)
			{
				return true;
			}
			CharacterInventory inventory2 = this.Inventory;
			Item heldItem2 = (inventory2 != null) ? inventory2.GetItemInLimbSlot(InvSlotType.LeftHand) : null;
			Item item4;
			if (heldItem2 == null)
			{
				item4 = null;
			}
			else
			{
				RemoteController component4 = heldItem2.GetComponent<RemoteController>();
				item4 = ((component4 != null) ? component4.TargetItem : null);
			}
			if (item4 == item)
			{
				return true;
			}
			Vector2 characterDirection = Vector2.Transform(Vector2.UnitY, Matrix.CreateRotationZ(this.AnimController.Collider.Rotation));
			Vector2 upperBodyPosition = this.Position + characterDirection * 20f;
			Vector2 lowerBodyPosition = this.Position - characterDirection * 60f;
			if (base.Submarine != null)
			{
				upperBodyPosition += base.Submarine.Position;
				lowerBodyPosition += base.Submarine.Position;
			}
			bool insideTrigger = item.IsInsideTrigger(upperBodyPosition) || item.IsInsideTrigger(lowerBodyPosition);
			if (item.Prefab.Triggers.Length > 0 && !insideTrigger && item.Prefab.RequireBodyInsideTrigger)
			{
				return false;
			}
			Rectangle itemDisplayRect = new Rectangle(item.InteractionRect.X, item.InteractionRect.Y - item.InteractionRect.Height, item.InteractionRect.Width, item.InteractionRect.Height);
			Vector2 playerDistanceCheckPosition = (lowerBodyPosition.Y < upperBodyPosition.Y) ? Vector2.Clamp(itemDisplayRect.Center.ToVector2(), lowerBodyPosition, upperBodyPosition) : Vector2.Clamp(itemDisplayRect.Center.ToVector2(), upperBodyPosition, lowerBodyPosition);
			if (itemDisplayRect.Contains(playerDistanceCheckPosition))
			{
				distanceToItem = 0f;
			}
			else
			{
				Vector2 rectIntersectionPoint = new Vector2(MathHelper.Clamp(playerDistanceCheckPosition.X, (float)itemDisplayRect.X, (float)itemDisplayRect.Right), MathHelper.Clamp(playerDistanceCheckPosition.Y, (float)itemDisplayRect.Y, (float)itemDisplayRect.Bottom));
				distanceToItem = Vector2.Distance(rectIntersectionPoint, playerDistanceCheckPosition);
			}
			float interactDistance = item.InteractDistance;
			if (this.SelectedSecondaryItem != null || item.IsSecondaryItem)
			{
				HumanoidAnimController c = this.AnimController as HumanoidAnimController;
				if (c != null)
				{
					float armLength = 0.75f * ConvertUnits.ToDisplayUnits(c.ArmLength);
					interactDistance = Math.Min(interactDistance, armLength);
				}
			}
			if (distanceToItem > interactDistance && item.InteractDistance > 0f)
			{
				return false;
			}
			Vector2 itemPosition = Character.<CanInteractWith>g__GetPosition|671_1(base.Submarine, item, item.SimPosition);
			if (this.SelectedSecondaryItem != null && !item.IsSecondaryItem)
			{
				if (controller != null && controller.Direction != Direction.None && controller.Direction != this.AnimController.Direction)
				{
					return false;
				}
				Controller selectedController = this.SelectedSecondaryItem.GetComponent<Controller>();
				if (selectedController != null && selectedController.ControlCharacterPose)
				{
					float threshold = ConvertUnits.ToSimUnits(40f);
					if (this.AnimController.Direction == Direction.Left && this.SimPosition.X + threshold < itemPosition.X)
					{
						return false;
					}
					if (this.AnimController.Direction == Direction.Right && this.SimPosition.X - threshold > itemPosition.X)
					{
						return false;
					}
				}
			}
			bool closeEnoughToIgnoreVisibilityCheck = distanceToItem <= 0.1f;
			if (!item.Prefab.InteractThroughWalls && Screen.Selected != GameMain.SubEditorScreen && !insideTrigger && !closeEnoughToIgnoreVisibilityCheck)
			{
				Body body = Submarine.CheckVisibility(this.SimPosition, itemPosition, true, false, true, true, true, null);
				bool itemCenterVisible = Character.<CanInteractWith>g__CheckBody|671_0(body, item);
				if (itemCenterVisible || !item.Prefab.RequireCursorInsideTrigger)
				{
					return itemCenterVisible;
				}
				foreach (Rectangle trigger in item.Prefab.Triggers)
				{
					Rectangle transformTrigger = item.TransformTrigger(trigger, false);
					RectangleF simRect = new RectangleF(ConvertUnits.ToSimUnits(transformTrigger.X), ConvertUnits.ToSimUnits(transformTrigger.Y - transformTrigger.Height), ConvertUnits.ToSimUnits(transformTrigger.Width), ConvertUnits.ToSimUnits(transformTrigger.Height));
					simRect.Location = Character.<CanInteractWith>g__GetPosition|671_1(base.Submarine, item, simRect.Location);
					Vector2 closest = ToolBox.GetClosestPointOnRectangle(simRect, this.SimPosition);
					Body triggerBody = Submarine.CheckVisibility(this.SimPosition, closest, true, false, true, true, true, null);
					if (Character.<CanInteractWith>g__CheckBody|671_0(triggerBody, item))
					{
						return true;
					}
				}
			}
			return true;
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0002E0A8 File Offset: 0x0002C2A8
		public void SetCustomInteract(Action<Character, Character> onCustomInteract, LocalizedString hudText)
		{
			this.onCustomInteract = onCustomInteract;
			this.CustomInteractHUDText = hudText;
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x0002E0B8 File Offset: 0x0002C2B8
		public void SelectCharacter(Character character)
		{
			if (character == null || character == this)
			{
				return;
			}
			this.SelectedCharacter = character;
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x0002E0C9 File Offset: 0x0002C2C9
		public void DeselectCharacter()
		{
			if (this.SelectedCharacter == null)
			{
				return;
			}
			if (!this.SelectedCharacter.AllowInput)
			{
				AnimController animController = this.SelectedCharacter.AnimController;
				if (animController != null)
				{
					animController.ResetPullJoints(null);
				}
			}
			this.SelectedCharacter = null;
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x0002E100 File Offset: 0x0002C300
		public void DoInteractionUpdate(float deltaTime, Vector2 mouseSimPos)
		{
			if (this.IsAIControlled)
			{
				return;
			}
			if (this.DisableInteract)
			{
				this.DisableInteract = false;
				return;
			}
			if (!this.CanInteract)
			{
				if (!this.IsAttachedToController())
				{
					this.SelectedItem = null;
				}
				this.SelectedSecondaryItem = null;
				this.focusedItem = null;
				if (!this.AllowInput)
				{
					this.FocusedCharacter = null;
					if (this.SelectedCharacter != null)
					{
						this.DeselectCharacter();
					}
					return;
				}
			}
			Character focusedCharacter;
			if (this.IsLocalPlayer)
			{
				if (!Character.IsMouseOnUI && (this.ViewTarget == null || this.ViewTarget == this) && !this.DisableFocusingOnEntities)
				{
					if (this.findFocusedTimer <= 0f || Screen.Selected == GameMain.SubEditorScreen)
					{
						if (!PlayerInput.PrimaryMouseButtonHeld() || Barotrauma.Inventory.DraggingItemToWorld)
						{
							if (CharacterHealth.OpenHealthWindow != null)
							{
								this.FocusedCharacter = null;
							}
							else
							{
								this.FocusedCharacter = ((this.CanInteract || this.CanEat) ? this.FindCharacterAtPosition(mouseSimPos, 150f) : null);
								if (this.FocusedCharacter != null && !this.CanSeeTarget(this.FocusedCharacter, null, false, false))
								{
									this.FocusedCharacter = null;
								}
							}
							float aimAssist = GameSettings.CurrentConfig.AimAssistAmount * (this.AnimController.InWater ? 1.5f : 1f);
							if (this.HeldItems.Any(delegate(Item it)
							{
								bool? flag;
								if (it == null)
								{
									flag = null;
								}
								else
								{
									Wire component = it.GetComponent<Wire>();
									flag = ((component != null) ? new bool?(component.IsActive) : null);
								}
								bool? flag2 = flag;
								return flag2.GetValueOrDefault();
							}))
							{
								aimAssist = 0f;
							}
							this.UpdateInteractablesInRange();
							if (!this.ShowInteractionLabels)
							{
								this.focusedItem = (this.CanInteract ? this.FindClosestItem(this.interactablesInRange, mouseSimPos, aimAssist) : null);
							}
							if (this.focusedItem != null)
							{
								if (this.focusedItem.CampaignInteractionType == CampaignMode.InteractionType.None)
								{
									focusedCharacter = this.FocusedCharacter;
									if (focusedCharacter == null || !focusedCharacter.IsPet || Vector2.DistanceSquared(this.focusedItem.SimPosition, mouseSimPos) >= Vector2.DistanceSquared(this.FocusedCharacter.SimPosition, mouseSimPos))
									{
										goto IL_1EE;
									}
								}
								this.FocusedCharacter = null;
							}
							IL_1EE:
							this.findFocusedTimer = 0.05f;
						}
						else
						{
							if (this.focusedItem != null && !this.CanInteractWith(this.focusedItem, true))
							{
								this.focusedItem = null;
							}
							if (this.FocusedCharacter != null && !this.CanInteractWith(this.FocusedCharacter, 200f, true, false))
							{
								this.FocusedCharacter = null;
							}
						}
					}
				}
				else
				{
					this.FocusedCharacter = null;
					this.focusedItem = null;
				}
				this.findFocusedTimer -= deltaTime;
				this.DisableFocusingOnEntities = false;
			}
			Limb head = this.AnimController.GetLimb(LimbType.Head, true, false, false);
			bool headInWater = (head == null) ? this.AnimController.InWater : head.InWater;
			Item selectedSecondaryItem = this.SelectedSecondaryItem;
			Ladder currentLadder = (selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Ladder>() : null;
			if ((this.SelectedSecondaryItem == null || currentLadder != null) && !headInWater && Screen.Selected != GameMain.SubEditorScreen)
			{
				bool climbInput = this.IsKeyDown(InputType.Up) || this.IsKeyDown(InputType.Down);
				bool isControlled = Character.Controlled == this;
				Ladder nearbyLadder = null;
				if (isControlled || climbInput)
				{
					float minDist = float.PositiveInfinity;
					foreach (Ladder ladder in Ladder.List)
					{
						float dist;
						if (ladder != currentLadder && (currentLadder == null || ladder.Item.WorldPosition.Y > currentLadder.Item.WorldPosition.Y == this.IsKeyDown(InputType.Up)) && this.CanInteractWith(ladder.Item, out dist, false) && dist < minDist)
						{
							nearbyLadder = ladder;
							if (isControlled)
							{
								ladder.Item.IsHighlighted = true;
								break;
							}
							break;
						}
					}
				}
				if (nearbyLadder != null && climbInput && nearbyLadder.Select(this))
				{
					this.SelectedSecondaryItem = nearbyLadder.Item;
				}
			}
			bool selectInputSameAsDeselect = GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Select] == GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Deselect];
			if (this.SelectedCharacter != null && (this.IsKeyHit(InputType.Grab) || this.IsKeyHit(InputType.Health)))
			{
				this.DeselectCharacter();
				return;
			}
			if (this.FocusedCharacter != null && this.IsKeyHit(InputType.Grab) && this.FocusedCharacter.CanBeDraggedBy(this) && (this.CanInteract || (this.FocusedCharacter.IsDead && this.CanEat)))
			{
				this.SelectCharacter(this.FocusedCharacter);
				return;
			}
			focusedCharacter = this.FocusedCharacter;
			if (focusedCharacter != null && !focusedCharacter.IsIncapacitated && this.IsKeyHit(InputType.Use) && this.FocusedCharacter.IsPet && this.CanInteract)
			{
				(this.FocusedCharacter.AIController as EnemyAIController).PetBehavior.Play(this);
				return;
			}
			if (this.FocusedCharacter != null && this.IsKeyHit(InputType.Health) && this.FocusedCharacter.CanBeHealedBy(this, true))
			{
				if (this.FocusedCharacter == this.SelectedCharacter)
				{
					this.DeselectCharacter();
					if (Character.Controlled == this)
					{
						CharacterHealth.OpenHealthWindow = null;
						return;
					}
				}
				else
				{
					this.SelectCharacter(this.FocusedCharacter);
					if (Character.Controlled == this)
					{
						HealingCooldown.PutOnCooldown();
						CharacterHealth.OpenHealthWindow = this.FocusedCharacter.CharacterHealth;
						return;
					}
				}
			}
			else
			{
				if (this.FocusedCharacter != null && this.IsKeyHit(InputType.Use) && this.FocusedCharacter.onCustomInteract != null && this.FocusedCharacter.AllowCustomInteract)
				{
					this.FocusedCharacter.onCustomInteract(this.FocusedCharacter, this);
					return;
				}
				if (this.IsKeyHit(InputType.Deselect) && this.SelectedItem != null && (this.focusedItem == null || this.focusedItem == this.SelectedItem || !selectInputSameAsDeselect))
				{
					this.SelectedItem = null;
					CharacterHealth.OpenHealthWindow = null;
					return;
				}
				if (this.IsKeyHit(InputType.Deselect) && this.SelectedSecondaryItem != null && this.SelectedSecondaryItem.GetComponent<Ladder>() == null && (this.focusedItem == null || this.focusedItem == this.SelectedSecondaryItem || !selectInputSameAsDeselect))
				{
					this.ReleaseSecondaryItem();
					CharacterHealth.OpenHealthWindow = null;
					return;
				}
				if (this.IsKeyHit(InputType.Health) && this.SelectedItem != null)
				{
					this.SelectedItem = null;
					return;
				}
				if (this.focusedItem != null)
				{
					if (Barotrauma.Inventory.DraggingItemToWorld)
					{
						return;
					}
					if (selectInputSameAsDeselect)
					{
						this.keys[24].Reset();
					}
					bool canInteract = this.focusedItem.TryInteract(this, false, false, false);
					if (Character.Controlled == this)
					{
						this.focusedItem.IsHighlighted = true;
						if (canInteract)
						{
							CharacterHealth.OpenHealthWindow = null;
						}
					}
				}
			}
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0002E758 File Offset: 0x0002C958
		public static void UpdateAnimAll(float deltaTime)
		{
			foreach (Character c in Character.CharacterList)
			{
				if (c.Enabled && !c.AnimController.Frozen)
				{
					c.AnimController.UpdateAnimations(deltaTime);
				}
			}
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0002E7C4 File Offset: 0x0002C9C4
		public static void UpdateAll(float deltaTime, Camera cam)
		{
			if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
			{
				foreach (Character c in Character.CharacterList)
				{
					if ((c is AICharacter || c.IsRemotePlayer) && !c.IsRemotePlayer)
					{
						if (c.IsLocalPlayer || (c.IsBot && !c.IsDead))
						{
							c.Enabled = true;
						}
						else if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
						{
							float closestPlayerDist = c.GetDistanceToClosestPlayer();
							if (closestPlayerDist > c.Params.DisableDistance)
							{
								c.Enabled = false;
								if (c.IsDead && c.AIController is EnemyAIController)
								{
									EntitySpawner spawner = Entity.Spawner;
									if (spawner != null)
									{
										spawner.AddEntityToRemoveQueue(c);
									}
								}
							}
							else if (closestPlayerDist < c.Params.DisableDistance * 0.9f)
							{
								c.Enabled = true;
							}
						}
						else if (Submarine.MainSub != null)
						{
							float distSqr = Vector2.DistanceSquared(Submarine.MainSub.WorldPosition, c.WorldPosition);
							if (Character.Controlled != null)
							{
								distSqr = Math.Min(distSqr, Vector2.DistanceSquared(Character.Controlled.WorldPosition, c.WorldPosition));
							}
							else
							{
								distSqr = Math.Min(distSqr, Vector2.DistanceSquared(GameMain.GameScreen.Cam.GetPosition(), c.WorldPosition));
							}
							if (distSqr > MathUtils.Pow2(c.Params.DisableDistance))
							{
								c.Enabled = false;
								if (c.IsDead && c.AIController is EnemyAIController)
								{
									EntitySpawner spawner2 = Entity.Spawner;
									if (spawner2 != null)
									{
										spawner2.AddEntityToRemoveQueue(c);
									}
								}
							}
							else if (distSqr < MathUtils.Pow2(c.Params.DisableDistance * 0.9f))
							{
								c.Enabled = true;
							}
						}
					}
				}
			}
			Character.characterUpdateTick++;
			if (Character.characterUpdateTick % Character.CharacterUpdateInterval == 0)
			{
				for (int i = 0; i < Character.CharacterList.Count; i++)
				{
					if (!LuaCsSetup.Instance.Game.UpdatePriorityCharacters.Contains(Character.CharacterList[i]))
					{
						Character.CharacterList[i].Update(deltaTime * (float)Character.CharacterUpdateInterval, cam);
					}
				}
			}
			foreach (Character character in LuaCsSetup.Instance.Game.UpdatePriorityCharacters)
			{
				if (!character.Removed)
				{
					character.Update(deltaTime, cam);
				}
			}
			Character.UpdateSpeechBubbles(deltaTime);
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x0002EAA0 File Offset: 0x0002CCA0
		private static void UpdateSpeechBubbles(float deltaTime)
		{
			int i = Character.speechBubbles.Count - 1;
			while (i >= 0)
			{
				Character.SpeechBubble bubble = Character.speechBubbles[i];
				bubble.LifeTime -= deltaTime;
				if (bubble.LifeTime <= 0f)
				{
					goto IL_4E;
				}
				Character character = bubble.Character;
				if (character != null && character.Removed)
				{
					goto IL_4E;
				}
				bubble.PrevPosition = bubble.Position;
				Vector2 desiredPos = bubble.GetDesiredPosition();
				Vector2 diff = desiredPos - bubble.Position;
				float dist = diff.Length();
				if (dist < 1f)
				{
					bubble.Moving = false;
				}
				else if (dist > 100f || bubble.Moving)
				{
					Vector2 moveAmount = diff / dist * MathHelper.Clamp(dist * 5f, 0f, 1000f) * deltaTime;
					moveAmount.Y *= 0.1f;
					bubble.Position += moveAmount;
					bubble.Moving = true;
				}
				bubble.MoveUpAmount += deltaTime * 5f;
				for (int j = i + 1; j < Character.speechBubbles.Count; j++)
				{
					Character.SpeechBubble otherBubble = Character.speechBubbles[j];
					if (Math.Abs(bubble.Position.X - otherBubble.Position.X) < (bubble.TextSize.X + otherBubble.TextSize.X) / 2f && Math.Abs(bubble.Position.Y - otherBubble.Position.Y) < (bubble.TextSize.Y + otherBubble.TextSize.Y) / 2f + 10f)
					{
						bubble.Position += Vector2.UnitY * deltaTime * 50f;
					}
				}
				IL_1E9:
				i--;
				continue;
				IL_4E:
				Character.speechBubbles.RemoveAt(i);
				goto IL_1E9;
			}
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x0002ECA4 File Offset: 0x0002CEA4
		public virtual void Update(float deltaTime, Camera cam)
		{
			this.UpdateProjSpecific(deltaTime, cam);
			if (this.TextChatVolume > 0f)
			{
				this.TextChatVolume -= 0.2f * deltaTime;
			}
			if (this.InvisibleTimer > 0f)
			{
				if (Character.Controlled != null && Character.Controlled != this)
				{
					Affliction affliction = Character.Controlled.CharacterHealth.GetAffliction("psychosis", true);
					if (((affliction != null) ? affliction.Strength : 0f) > 0f)
					{
						goto IL_87;
					}
				}
				this.InvisibleTimer = Math.Min(this.InvisibleTimer, 1f);
				IL_87:
				this.InvisibleTimer -= deltaTime;
			}
			this.KnockbackCooldownTimer -= deltaTime;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && this == Character.Controlled && !this.isSynced)
			{
				return;
			}
			this.UpdateDespawn(deltaTime, true);
			if (!this.Enabled)
			{
				return;
			}
			if (Level.Loaded != null && (this.WorldPosition.Y < -1000000f || (base.Submarine != null && base.Submarine.WorldPosition.Y < -1000000f)))
			{
				this.Enabled = false;
				this.Kill(CauseOfDeathType.Pressure, null, false, true);
				return;
			}
			this.ApplyStatusEffects(ActionType.Always, deltaTime);
			this.PreviousHull = this.CurrentHull;
			this.CurrentHull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
			this.obstructVisionAmount = Math.Max(this.obstructVisionAmount - deltaTime, 0f);
			if (this.Inventory != null && Vector2.DistanceSquared(this.lastInventoryItemSetTransformPosition, this.Position) > 0.1f)
			{
				foreach (Item item in this.Inventory.GetAllItems(false))
				{
					if (item.body != null && !item.body.Enabled)
					{
						item.SetTransform(this.SimPosition, 0f, true, true, base.Submarine);
					}
				}
				this.lastInventoryItemSetTransformPosition = this.Position;
			}
			this.HideFace = false;
			this.IgnoreMeleeWeapons = false;
			this.UpdateSightRange(deltaTime);
			this.UpdateSoundRange(deltaTime);
			this.UpdateAttackers(deltaTime);
			for (int i = 0; i < this.characterTalents.Count; i++)
			{
				this.characterTalents[i].UpdateTalent(deltaTime);
			}
			if (this.IsDead)
			{
				return;
			}
			if (GameMain.NetworkMember != null)
			{
				this.UpdateNetInput();
			}
			else
			{
				this.AnimController.Frozen = false;
			}
			this.DisableImpactDamageTimer -= deltaTime;
			if (!this.speechImpedimentSet)
			{
				this.speechImpediment = 0f;
			}
			this.speechImpedimentSet = false;
			if (this.NeedsAir)
			{
				if (!this.IsProtectedFromPressure && (this.AnimController.CurrentHull == null || this.AnimController.CurrentHull.LethalPressure >= 80f))
				{
					if (this.PressureTimer > this.CharacterHealth.PressureKillDelay * 0.1f)
					{
						this.CharacterHealth.ApplyAffliction(this.AnimController.MainLimb, new Affliction(AfflictionPrefab.OrganDamage, this.PressureTimer / 10f * deltaTime), true, false, true);
					}
					if (this.CharacterHealth.PressureKillDelay <= 0f)
					{
						this.PressureTimer = 100f;
					}
					else
					{
						this.PressureTimer += ((this.AnimController.CurrentHull == null) ? 100f : this.AnimController.CurrentHull.LethalPressure) / this.CharacterHealth.PressureKillDelay * deltaTime;
					}
					if (this.PressureTimer >= 100f)
					{
						if (Character.Controlled == this)
						{
							cam.Zoom = 5f;
						}
						if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
						{
							this.Implode(false);
							if (this.IsDead)
							{
								return;
							}
						}
					}
				}
				else
				{
					this.PressureTimer = 0f;
				}
			}
			else if ((GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) && !this.IsProtectedFromPressure)
			{
				Level loaded = Level.Loaded;
				float realWorldDepth = (loaded != null) ? loaded.GetRealWorldDepth(this.WorldPosition.Y) : 0f;
				if (this.PressureProtection < realWorldDepth && realWorldDepth > this.CharacterHealth.CrushDepth && (this.AnimController.CurrentHull == null || this.AnimController.CurrentHull.LethalPressure >= 80f))
				{
					this.Implode(false);
					if (this.IsDead)
					{
						return;
					}
				}
			}
			this.ApplyStatusEffects(this.AnimController.InWater ? ActionType.InWater : ActionType.NotInWater, deltaTime);
			this.ApplyStatusEffects(ActionType.OnActive, deltaTime);
			if (this.aiTarget != null && Timing.TotalTime > this.aiTarget.InDetectableSetTime + 0.10000000149011612)
			{
				this.aiTarget.InDetectable = false;
			}
			this.UpdateControlled(deltaTime, cam);
			if (this.NeedsOxygen)
			{
				this.UpdateOxygen(deltaTime);
			}
			this.CalculateHealthMultiplier();
			this.CharacterHealth.Update(deltaTime);
			if (this.IsIncapacitated)
			{
				this.Stun = Math.Max(5f, this.Stun);
				this.AnimController.ResetPullJoints(null);
				this.SelectedItem = (this.SelectedSecondaryItem = null);
				return;
			}
			this.UpdateAIChatMessages(deltaTime);
			bool wasRagdolled = this.IsRagdolled;
			if (this.IsForceRagdolled)
			{
				this.IsRagdolled = this.IsForceRagdolled;
			}
			else if (this != Character.Controlled)
			{
				wasRagdolled = this.IsRagdolled;
				this.IsRagdolled = this.IsKeyDown(InputType.Ragdoll);
				if (this.IsRagdolled && !this.IsPlayer)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember == null || !networkMember.IsClient)
					{
						this.ClearInput(InputType.Ragdoll);
					}
				}
			}
			else
			{
				bool tooFastToUnragdoll = this.<Update>g__bodyMovingTooFast|679_1(this.AnimController.Collider) || this.<Update>g__bodyMovingTooFast|679_1(this.AnimController.MainLimb.body);
				if (this.ragdollingLockTimer > 0f)
				{
					this.ragdollingLockTimer -= deltaTime;
				}
				else if (!tooFastToUnragdoll)
				{
					this.IsRagdolled = this.IsKeyDown(InputType.Ragdoll);
					if (wasRagdolled != this.IsRagdolled && !this.AnimController.IsHangingWithRope)
					{
						this.ragdollingLockTimer = 0.2f;
					}
				}
				this.SetInput(InputType.Ragdoll, false, this.IsRagdolled);
			}
			if (!wasRagdolled && this.IsRagdolled && !this.AnimController.IsHangingWithRope)
			{
				this.CheckTalents(AbilityEffectType.OnRagdoll);
			}
			this.lowPassMultiplier = MathHelper.Lerp(this.lowPassMultiplier, 1f, 0.1f);
			if (this.IsRagdolled || !this.CanMove)
			{
				HumanoidAnimController humanAnimController = this.AnimController as HumanoidAnimController;
				if (humanAnimController != null)
				{
					humanAnimController.Crouching = false;
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				bool isControlledByRemotelyByServer = networkMember != null && networkMember.IsClient && this.IsRemotelyControlled;
				if (this.IsRagdolled && !isControlledByRemotelyByServer)
				{
					this.AnimController.IgnorePlatforms = true;
				}
				this.AnimController.ResetPullJoints(null);
				if (this.IsAttachedToController())
				{
					if (!this.IsKeyDown(InputType.Ragdoll))
					{
						goto IL_6F6;
					}
					if (GameMain.NetworkMember != null)
					{
						networkMember = GameMain.NetworkMember;
						if (networkMember == null || !networkMember.IsServer)
						{
							goto IL_6F6;
						}
					}
				}
				this.SelectedItem = null;
				IL_6F6:
				this.SelectedSecondaryItem = null;
				this.SelectedCharacter = null;
				return;
			}
			this.Control(deltaTime, cam);
			if (this.IsRemotePlayer)
			{
				Vector2 mouseSimPos = ConvertUnits.ToSimUnits(this.cursorPosition);
				this.DoInteractionUpdate(deltaTime, mouseSimPos);
			}
			if (this.<Update>g__MustDeselect|679_0(this.SelectedItem))
			{
				this.SelectedItem = null;
			}
			if (this.<Update>g__MustDeselect|679_0(this.SelectedSecondaryItem))
			{
				this.ReleaseSecondaryItem();
			}
			if (!this.IsDead)
			{
				this.LockHands = false;
			}
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x0002F424 File Offset: 0x0002D624
		private void UpdateControlled(float deltaTime, Camera cam)
		{
			if (Character.controlled != this)
			{
				return;
			}
			this.ControlLocalPlayer(deltaTime, cam, true);
			LightManager.ViewTarget = this;
			CharacterHUD.Update(deltaTime, this, cam);
			if (this.hudProgressBars.Any<KeyValuePair<object, HUDProgressBar>>())
			{
				foreach (KeyValuePair<object, HUDProgressBar> progressBar in this.hudProgressBars)
				{
					if (progressBar.Value.FadeTimer <= 0f)
					{
						this.progressBarRemovals.Add(progressBar);
					}
					else
					{
						progressBar.Value.Update(deltaTime);
					}
				}
				if (this.progressBarRemovals.Any<KeyValuePair<object, HUDProgressBar>>())
				{
					this.progressBarRemovals.ForEach(delegate(KeyValuePair<object, HUDProgressBar> pb)
					{
						this.hudProgressBars.Remove(pb.Key);
					});
					this.progressBarRemovals.Clear();
				}
			}
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x0002F500 File Offset: 0x0002D700
		private void UpdateProjSpecific(float deltaTime, Camera cam)
		{
			foreach (Character.GUIMessage message in this.guiMessages)
			{
				bool wasPending = message.Timer < 0f;
				message.Timer += deltaTime;
				if (wasPending && message.Timer >= 0f && message.PlaySound)
				{
					SoundPlayer.PlayUISound(GUISoundType.UIMessage);
				}
			}
			this.guiMessages.RemoveAll((Character.GUIMessage m) => m.Timer >= m.Lifetime);
			if (this.textlessSpeechBubble != null)
			{
				this.textlessSpeechBubble.LifeTime -= deltaTime;
				if (this.textlessSpeechBubble.LifeTime <= 0f)
				{
					this.textlessSpeechBubble = null;
				}
			}
			if (!this.enabled)
			{
				return;
			}
			if (!this.IsIncapacitated)
			{
				if (this.soundTimer > 0f)
				{
					this.soundTimer -= deltaTime;
				}
				else
				{
					EnemyAIController enemyAI = this.AIController as EnemyAIController;
					if (enemyAI != null)
					{
						AIState state = enemyAI.State;
						if (state <= AIState.Freeze)
						{
							if (state != AIState.Attack)
							{
								if (state == AIState.Freeze)
								{
									goto IL_1C3;
								}
							}
							else
							{
								if (Rand.Value(Rand.RandSync.Unsynced) > 0.5f)
								{
									this.PlaySound(CharacterSound.SoundType.Attack, 1f, 0f);
									goto IL_1C3;
								}
								this.PlaySound(CharacterSound.SoundType.Idle, 1f, 0f);
								goto IL_1C3;
							}
						}
						else if (state == AIState.PlayDead || state == AIState.Hiding)
						{
							goto IL_1C3;
						}
						PetBehavior petBehavior = enemyAI.PetBehavior;
						if (petBehavior != null && (petBehavior.Happiness < petBehavior.UnhappyThreshold || petBehavior.Hunger > petBehavior.HungryThreshold))
						{
							this.PlaySound(CharacterSound.SoundType.Unhappy, 1f, 0f);
						}
						else
						{
							this.PlaySound(CharacterSound.SoundType.Idle, 1f, 0f);
						}
					}
				}
			}
			IL_1C3:
			if (this.info != null || this.Vitality < this.MaxVitality * 0.98f || this.IsPet)
			{
				this.hudInfoTimer -= deltaTime;
				if (this.hudInfoTimer <= 0f)
				{
					if (Character.controlled == null)
					{
						this.hudInfoVisible = true;
					}
					else if (this.WorldPosition.X < (float)cam.WorldView.X || this.WorldPosition.X > (float)cam.WorldView.Right || this.WorldPosition.Y > (float)cam.WorldView.Y || this.WorldPosition.Y < (float)(cam.WorldView.Y - cam.WorldView.Height))
					{
						this.hudInfoVisible = false;
					}
					else
					{
						this.hudInfoVisible = Character.controlled.CanSeeTarget(this, Character.controlled.ViewTarget, false, false);
					}
					this.hudInfoTimer = Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced);
				}
			}
			this.CharacterHealth.UpdateClientSpecific(deltaTime);
			if (Character.controlled == this)
			{
				this.CharacterHealth.UpdateHUD(deltaTime);
			}
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x0002F808 File Offset: 0x0002DA08
		private void SetOrderProjSpecific(Order order)
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null)
			{
				return;
			}
			CrewManager crewManager = gameSession.CrewManager;
			if (crewManager == null)
			{
				return;
			}
			crewManager.AddCurrentOrderIcon(this, order);
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x0002F828 File Offset: 0x0002DA28
		public void AddAttacker(Character character, float damage)
		{
			Character.Attacker attacker = this.lastAttackers.FirstOrDefault((Character.Attacker a) => a.Character == character);
			if (attacker != null)
			{
				this.lastAttackers.Remove(attacker);
			}
			else
			{
				attacker = new Character.Attacker
				{
					Character = character
				};
			}
			if (this.lastAttackers.Count > 4)
			{
				this.lastAttackers.RemoveRange(0, this.lastAttackers.Count - 4);
			}
			attacker.Damage += damage;
			this.lastAttackers.Add(attacker);
		}

		// Token: 0x0600059A RID: 1434 RVA: 0x0002F8C0 File Offset: 0x0002DAC0
		public void ForgiveAttacker(Character character)
		{
			int index;
			if ((index = this.lastAttackers.FindIndex((Character.Attacker a) => a.Character == character)) >= 0)
			{
				this.lastAttackers.RemoveAt(index);
			}
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x0002F904 File Offset: 0x0002DB04
		public float GetDamageDoneByAttacker(Character otherCharacter)
		{
			if (otherCharacter == null)
			{
				return 0f;
			}
			float dmg = 0f;
			Character.Attacker attacker = this.LastAttackers.LastOrDefault((Character.Attacker a) => a.Character == otherCharacter);
			if (attacker != null)
			{
				dmg = attacker.Damage;
			}
			return dmg;
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x0002F958 File Offset: 0x0002DB58
		private void UpdateAttackers(float deltaTime)
		{
			foreach (Character.Attacker enemy in this.LastAttackers)
			{
				float cumulativeDamage = enemy.Damage;
				if (cumulativeDamage > 0f)
				{
					float reduction = deltaTime;
					if (cumulativeDamage < 2f)
					{
						reduction *= 0.5f;
					}
					enemy.Damage = Math.Max(0f, enemy.Damage - reduction);
				}
			}
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x0002F9D8 File Offset: 0x0002DBD8
		private void UpdateOxygen(float deltaTime)
		{
			if (this.NeedsAir && Timing.TotalTime > this.pressureProtectionLastSet + 0.1)
			{
				this.pressureProtection = 0f;
			}
			if (this.NeedsWater)
			{
				float waterAvailable = 100f;
				if (!this.AnimController.InWater && this.CurrentHull != null)
				{
					waterAvailable = this.CurrentHull.WaterPercentage;
				}
				this.OxygenAvailable += MathHelper.Clamp(waterAvailable - this.oxygenAvailable, -deltaTime * 50f, deltaTime * 50f);
			}
			else
			{
				float hullAvailableOxygen = 0f;
				if (!this.AnimController.HeadInWater && this.AnimController.CurrentHull != null)
				{
					if (this.OxygenAvailable * 0.98f < this.AnimController.CurrentHull.OxygenPercentage && this.UseHullOxygen)
					{
						this.AnimController.CurrentHull.Oxygen -= 700f * deltaTime;
					}
					hullAvailableOxygen = this.AnimController.CurrentHull.OxygenPercentage;
				}
				this.OxygenAvailable += MathHelper.Clamp(hullAvailableOxygen - this.oxygenAvailable, -deltaTime * 50f, deltaTime * 50f);
			}
			this.UseHullOxygen = true;
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x0002FB11 File Offset: 0x0002DD11
		protected float GetDistanceToClosestPlayer()
		{
			return (float)Math.Sqrt((double)this.GetDistanceSqrToClosestPlayer());
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x0002FB20 File Offset: 0x0002DD20
		protected float GetDistanceSqrToClosestPlayer()
		{
			float distSqr = float.MaxValue;
			foreach (Character otherCharacter in Character.CharacterList)
			{
				if (otherCharacter != this && otherCharacter.IsRemotePlayer)
				{
					distSqr = Math.Min(distSqr, Vector2.DistanceSquared(otherCharacter.WorldPosition, this.WorldPosition));
					if (otherCharacter.ViewTarget != null)
					{
						distSqr = Math.Min(distSqr, Vector2.DistanceSquared(otherCharacter.ViewTarget.WorldPosition, this.WorldPosition));
					}
				}
			}
			if (this == Character.Controlled)
			{
				return 0f;
			}
			if (Character.controlled != null)
			{
				distSqr = Math.Min(distSqr, Vector2.DistanceSquared(Character.Controlled.WorldPosition, this.WorldPosition));
			}
			distSqr = Math.Min(distSqr, Vector2.DistanceSquared(GameMain.GameScreen.Cam.Position, this.WorldPosition));
			return distSqr;
		}

		// Token: 0x060005A0 RID: 1440 RVA: 0x0002FC10 File Offset: 0x0002DE10
		public float GetDistanceToClosestLimb(Vector2 simPos)
		{
			float closestDist = float.MaxValue;
			foreach (Limb limb in this.AnimController.Limbs)
			{
				if (!limb.IsSevered)
				{
					float dist = Vector2.Distance(simPos, limb.SimPosition);
					dist -= limb.body.GetMaxExtent();
					closestDist = Math.Min(closestDist, dist);
					if (closestDist <= 0f)
					{
						return 0f;
					}
				}
			}
			return closestDist;
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x0002FC80 File Offset: 0x0002DE80
		private void UpdateDespawn(float deltaTime, bool createNetworkEvents = true)
		{
			if (!this.EnableDespawn)
			{
				return;
			}
			if (GameMain.NetworkMember != null && !GameMain.NetworkMember.IsServer)
			{
				return;
			}
			if (this.IsDead)
			{
				CauseOfDeath causeOfDeath = this.CauseOfDeath;
				if (causeOfDeath != null && causeOfDeath.Type == CauseOfDeathType.Disconnected)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.Campaign : null) != null)
					{
						return;
					}
				}
				if (this.SelectedBy != null)
				{
					this.despawnTimer = 0f;
					return;
				}
				float despawnDelay = (float)GameSettings.CurrentConfig.CorpseDespawnDelay;
				float despawnPriority = 1f;
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.GameMode : null) is PvPMode)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (((networkMember != null) ? networkMember.RespawnManager : null) != null)
					{
						despawnDelay = (float)GameSettings.CurrentConfig.CorpseDespawnDelayPvP;
						goto IL_14C;
					}
				}
				int subCorpseCount = 0;
				if (base.Submarine != null)
				{
					subCorpseCount = Character.CharacterList.Count((Character c) => c.IsDead && c.Submarine == base.Submarine);
					if (subCorpseCount < GameSettings.CurrentConfig.CorpsesPerSubDespawnThreshold)
					{
						return;
					}
				}
				if (subCorpseCount > GameSettings.CurrentConfig.CorpsesPerSubDespawnThreshold)
				{
					despawnPriority += (float)(subCorpseCount - GameSettings.CurrentConfig.CorpsesPerSubDespawnThreshold) / (float)GameSettings.CurrentConfig.CorpsesPerSubDespawnThreshold;
				}
				float distToClosestPlayer = this.GetDistanceToClosestPlayer();
				if (distToClosestPlayer > this.Params.DisableDistance)
				{
					this.despawnTimer = Math.Max(this.despawnTimer, despawnDelay - 60f);
				}
				if (this.AIController is EnemyAIController)
				{
					despawnPriority *= 2f;
				}
				IL_14C:
				this.despawnTimer += deltaTime * despawnPriority;
				if (this.despawnTimer < despawnDelay)
				{
					return;
				}
				this.Despawn(true);
				return;
			}
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x0002FDFC File Offset: 0x0002DFFC
		private void Despawn(bool createNetworkEvents = true)
		{
			Character.<>c__DisplayClass693_0 CS$<>8__locals1 = new Character.<>c__DisplayClass693_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.createNetworkEvents = createNetworkEvents;
			if (!this.EnableDespawn)
			{
				return;
			}
			CS$<>8__locals1.despawnContainerId = (this.IsHuman ? Tags.DespawnContainer : this.Params.DespawnContainer);
			GameSession gameSession = GameMain.GameSession;
			bool flag;
			if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				flag = (((networkMember != null) ? networkMember.RespawnManager : null) != null);
			}
			else
			{
				flag = false;
			}
			bool pvpWithRespawning = flag;
			if (!CS$<>8__locals1.despawnContainerId.IsEmpty && !pvpWithRespawning)
			{
				CauseOfDeath causeOfDeath = this.CauseOfDeath;
				if (causeOfDeath == null || causeOfDeath.Type != CauseOfDeathType.Disconnected)
				{
					ItemPrefab itemPrefab;
					if ((itemPrefab = (MapEntityPrefab.FindByIdentifier(CS$<>8__locals1.despawnContainerId) as ItemPrefab)) == null)
					{
						itemPrefab = (ItemPrefab.Prefabs.Find((ItemPrefab me) => ((me != null) ? me.Tags : null) != null && me.Tags.Contains(CS$<>8__locals1.despawnContainerId)) ?? (MapEntityPrefab.FindByIdentifier("metalcrate".ToIdentifier()) as ItemPrefab));
					}
					ItemPrefab containerPrefab = itemPrefab;
					if (containerPrefab == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(124, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Could not spawn a container for a despawned character's items. No item with the tag \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.despawnContainerId);
						defaultInterpolatedStringHandler.AppendLiteral("\" or the identifier \"metalcrate\" found.");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
						return;
					}
					EntitySpawner spawner = Entity.Spawner;
					if (spawner == null)
					{
						return;
					}
					ItemPrefab itemPrefab2 = containerPrefab;
					Vector2 worldPosition = this.WorldPosition;
					Action<Item> onSpawned = new Action<Item>(CS$<>8__locals1.<Despawn>g__onItemContainerSpawned|1);
					spawner.AddItemToSpawnQueue(itemPrefab2, worldPosition, null, null, onSpawned);
					return;
				}
			}
			Entity.Spawner.AddEntityToRemoveQueue(this);
		}

		// Token: 0x060005A3 RID: 1443 RVA: 0x0002FF7C File Offset: 0x0002E17C
		public void DespawnNow(bool createNetworkEvents = true)
		{
			this.Despawn(createNetworkEvents);
			for (int i = 0; i < 2; i++)
			{
				Entity.Spawner.Update(createNetworkEvents);
			}
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x0002FFA8 File Offset: 0x0002E1A8
		public static void RemoveByPrefab(CharacterPrefab prefab)
		{
			if (Character.CharacterList == null)
			{
				return;
			}
			List<Character> list = new List<Character>(Character.CharacterList);
			foreach (Character character in list)
			{
				if (character.Prefab == prefab)
				{
					character.Remove();
				}
			}
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x00030014 File Offset: 0x0002E214
		private void UpdateSightRange(float deltaTime)
		{
			if (this.aiTarget == null)
			{
				return;
			}
			float minRange = Math.Clamp((float)Math.Sqrt((double)this.Mass) * this.Visibility, 250f, 1000f);
			float massFactor = (float)Math.Sqrt((double)(this.Mass / 20f));
			float targetRange = Math.Min(minRange + massFactor * this.AnimController.Collider.LinearVelocity.Length() * 2f * this.Visibility, this.maxAIRange);
			targetRange *= 1f + this.GetStatValue(StatTypes.SightRangeMultiplier, true);
			float newRange = MathHelper.SmoothStep(this.aiTarget.SightRange, targetRange, deltaTime * this.aiTargetChangeSpeed);
			if (!float.IsNaN(newRange))
			{
				this.aiTarget.SightRange = newRange;
			}
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x000300D8 File Offset: 0x0002E2D8
		private void UpdateSoundRange(float deltaTime)
		{
			if (this.aiTarget == null)
			{
				return;
			}
			if (this.IsDead)
			{
				this.aiTarget.SoundRange = 0f;
				return;
			}
			float massFactor = (float)Math.Sqrt((double)(this.Mass / 10f));
			float targetRange = Math.Min(massFactor * this.AnimController.Collider.LinearVelocity.Length() * 2f * this.Noise, this.maxAIRange);
			float speechImpedimentMultiplier = 1f - this.SpeechImpediment / 100f;
			if (this.TextChatVolume > 0f)
			{
				targetRange = Math.Max(targetRange, this.TextChatVolume * 0.5f * 2000f * speechImpedimentMultiplier);
			}
			if (this.IsPlayer)
			{
				float voipAmplitude = 0f;
				targetRange = Math.Max(targetRange, voipAmplitude * 1.5f * 2000f * speechImpedimentMultiplier);
			}
			targetRange *= 1f + this.GetStatValue(StatTypes.SoundRangeMultiplier, true);
			targetRange = Math.Min(targetRange, this.maxAIRange);
			float newRange = MathHelper.SmoothStep(this.aiTarget.SoundRange, targetRange, deltaTime * this.aiTargetChangeSpeed);
			if (!float.IsNaN(newRange))
			{
				this.aiTarget.SoundRange = newRange;
			}
		}

		// Token: 0x060005A7 RID: 1447 RVA: 0x00030200 File Offset: 0x0002E400
		public bool CanHearCharacter(Character speaker)
		{
			if (speaker == null || speaker.SpeechImpediment > 100f)
			{
				return false;
			}
			if (speaker == this)
			{
				return true;
			}
			ChatMessageType messageType = (ChatMessage.CanUseRadio(speaker, false) && ChatMessage.CanUseRadio(this, false)) ? ChatMessageType.Radio : ChatMessageType.Default;
			return !string.IsNullOrEmpty(ChatMessage.ApplyDistanceEffect("message", messageType, speaker, this));
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x00030254 File Offset: 0x0002E454
		public void SetOrder(Order order, bool isNewOrder, bool speak = true, bool force = false)
		{
			Character orderGiver = (order != null) ? order.OrderGiver : null;
			if (!force && orderGiver != null && !this.CanHearCharacter(orderGiver))
			{
				return;
			}
			if (order != null && order.AutoDismiss)
			{
				OrderCategory? category = order.Category;
				if (category != null)
				{
					OrderCategory valueOrDefault = category.GetValueOrDefault();
					if (valueOrDefault != OrderCategory.Movement)
					{
						if (valueOrDefault != OrderCategory.Operate || order.TargetEntity == null)
						{
							goto IL_1E5;
						}
						using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Character character = enumerator.Current;
								if (character != this && character.TeamID == this.TeamID && character.AIController is HumanAIController && HumanAIController.IsActive(character) && character.Info != null)
								{
									foreach (Order currentOrder in character.CurrentOrders)
									{
										if (currentOrder != null && currentOrder.Category.GetValueOrDefault() == OrderCategory.Operate)
										{
											Identifier identifier = currentOrder.Identifier;
											Identifier identifier2 = order.Identifier;
											if (!(identifier != identifier2) && currentOrder.TargetEntity == order.TargetEntity && currentOrder.AutoDismiss)
											{
												character.SetOrder(currentOrder.GetDismissal(), isNewOrder, speak, force);
												break;
											}
										}
									}
								}
							}
							goto IL_1E5;
						}
					}
					Order orderToReplace = null;
					if (this.CurrentOrders != null)
					{
						foreach (Order currentOrder2 in this.CurrentOrders)
						{
							if (currentOrder2 != null && currentOrder2.Category.GetValueOrDefault() == OrderCategory.Movement)
							{
								orderToReplace = currentOrder2;
								break;
							}
						}
					}
					if (orderToReplace != null && orderToReplace.AutoDismiss)
					{
						this.SetOrder(orderToReplace.GetDismissal(), isNewOrder, speak, force);
					}
				}
			}
			IL_1E5:
			this.RemoveDuplicateOrders(order);
			this.AddCurrentOrder(order);
			bool flag;
			if (orderGiver != null)
			{
				Identifier identifier = order.Identifier;
				flag = (identifier != "dismissed");
			}
			else
			{
				flag = false;
			}
			if (flag && isNewOrder)
			{
				AbilityOrderedCharacter abilityOrderedCharacter = new AbilityOrderedCharacter(this);
				orderGiver.CheckTalents(AbilityEffectType.OnGiveOrder, abilityOrderedCharacter);
				if (order.OrderGiver.LastOrderedCharacter != this)
				{
					order.OrderGiver.SecondLastOrderedCharacter = order.OrderGiver.LastOrderedCharacter;
					order.OrderGiver.LastOrderedCharacter = this;
				}
			}
			HumanAIController humanAI = this.AIController as HumanAIController;
			if (humanAI != null)
			{
				humanAI.SetOrder(order, speak);
			}
			this.SetOrderProjSpecific(order);
		}

		// Token: 0x060005A9 RID: 1449 RVA: 0x000304FC File Offset: 0x0002E6FC
		private void AddCurrentOrder(Order newOrder)
		{
			if (this.CurrentOrders == null)
			{
				return;
			}
			if (newOrder != null)
			{
				Identifier identifier = newOrder.Identifier;
				if (!(identifier == "dismissed"))
				{
					for (int i = 0; i < this.CurrentOrders.Count; i++)
					{
						Order orderInfo = this.CurrentOrders[i];
						if (orderInfo.ManualPriority <= newOrder.ManualPriority)
						{
							this.CurrentOrders[i] = orderInfo.WithManualPriority(orderInfo.ManualPriority - 1);
						}
					}
					this.CurrentOrders.RemoveAll((Order order) => order.ManualPriority <= 0);
					this.CurrentOrders.Add(newOrder);
					this.CurrentOrders.Sort((Order x, Order y) => y.ManualPriority.CompareTo(x.ManualPriority));
					return;
				}
			}
			if (!(newOrder.Option != Identifier.Empty))
			{
				this.CurrentOrders.Clear();
				return;
			}
			if (this.CurrentOrders.Any((Order o) => o.MatchesDismissedOrder(newOrder.Option)))
			{
				Order dismissedOrderInfo = this.CurrentOrders.First((Order o) => o.MatchesDismissedOrder(newOrder.Option));
				int dismissedOrderPriority = dismissedOrderInfo.ManualPriority;
				this.CurrentOrders.Remove(dismissedOrderInfo);
				for (int j = 0; j < this.CurrentOrders.Count; j++)
				{
					Order orderInfo2 = this.CurrentOrders[j];
					if (orderInfo2.ManualPriority < dismissedOrderPriority)
					{
						this.CurrentOrders[j] = orderInfo2.WithManualPriority(orderInfo2.ManualPriority + 1);
					}
				}
				return;
			}
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x000306C4 File Offset: 0x0002E8C4
		private void RemoveDuplicateOrders(Order order)
		{
			if (this.CurrentOrders == null)
			{
				return;
			}
			int? priorityOfRemoved = null;
			for (int i = this.CurrentOrders.Count - 1; i >= 0; i--)
			{
				Order orderInfo = this.CurrentOrders[i];
				Identifier identifier = order.Identifier;
				Identifier identifier2 = orderInfo.Identifier;
				if (identifier == identifier2)
				{
					priorityOfRemoved = new int?(orderInfo.ManualPriority);
					this.CurrentOrders.RemoveAt(i);
					break;
				}
			}
			if (priorityOfRemoved == null)
			{
				return;
			}
			for (int j = 0; j < this.CurrentOrders.Count; j++)
			{
				Order orderInfo2 = this.CurrentOrders[j];
				if (orderInfo2.ManualPriority < priorityOfRemoved.Value)
				{
					this.CurrentOrders[j] = orderInfo2.WithManualPriority(orderInfo2.ManualPriority + 1);
				}
			}
			this.CurrentOrders.RemoveAll((Order o) => o.ManualPriority <= 0);
			this.CurrentOrders.Sort((Order x, Order y) => y.ManualPriority.CompareTo(x.ManualPriority));
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x000307F1 File Offset: 0x0002E9F1
		public Order GetCurrentOrderWithTopPriority()
		{
			return this.GetCurrentOrder(delegate(Order orderInfo)
			{
				if (orderInfo == null)
				{
					return false;
				}
				Identifier identifier = orderInfo.Identifier;
				return !(identifier == "dismissed") && orderInfo.ManualPriority >= 1;
			});
		}

		// Token: 0x060005AC RID: 1452 RVA: 0x00030818 File Offset: 0x0002EA18
		public Order GetCurrentOrder(Order order)
		{
			return this.GetCurrentOrder((Order orderInfo) => orderInfo.MatchesOrder(order));
		}

		// Token: 0x060005AD RID: 1453 RVA: 0x00030844 File Offset: 0x0002EA44
		private Order GetCurrentOrder(Func<Order, bool> predicate)
		{
			if (this.CurrentOrders != null && this.CurrentOrders.Any(predicate))
			{
				return this.CurrentOrders.First(predicate);
			}
			return null;
		}

		// Token: 0x060005AE RID: 1454 RVA: 0x0003086A File Offset: 0x0002EA6A
		public void DisableLine(Identifier identifier)
		{
			if (identifier != Identifier.Empty)
			{
				this.prevAiChatMessages[identifier] = (float)Timing.TotalTime;
			}
		}

		// Token: 0x060005AF RID: 1455 RVA: 0x0003088C File Offset: 0x0002EA8C
		public void DisableLine(string identifier)
		{
			this.DisableLine(identifier.ToIdentifier());
		}

		// Token: 0x060005B0 RID: 1456 RVA: 0x0003089C File Offset: 0x0002EA9C
		public void Speak(string message, ChatMessageType? messageType = null, float delay = 0f, Identifier identifier = default(Identifier), float minDurationBetweenSimilar = 0f)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (string.IsNullOrEmpty(message))
			{
				return;
			}
			if (this.SpeechImpediment >= 100f)
			{
				return;
			}
			if (this.prevAiChatMessages.ContainsKey(identifier) && (double)this.prevAiChatMessages[identifier] < Timing.TotalTime - (double)minDurationBetweenSimilar)
			{
				this.prevAiChatMessages.Remove(identifier);
			}
			if (minDurationBetweenSimilar > 0f && !(identifier == Identifier.Empty) && (this.aiChatMessageQueue.Any((AIChatMessage m) => m.Identifier == identifier) || this.prevAiChatMessages.ContainsKey(identifier)))
			{
				return;
			}
			this.aiChatMessageQueue.Add(new AIChatMessage(message, messageType, identifier, delay));
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x00030988 File Offset: 0x0002EB88
		public void SendSinglePlayerMessage(AIChatMessage message, bool canUseRadio, WifiComponent radio)
		{
			if (message.MessageType == null)
			{
				message.MessageType = new ChatMessageType?(canUseRadio ? ChatMessageType.Radio : ChatMessageType.Default);
			}
			GameSession gameSession = GameMain.GameSession;
			CrewManager crewManager = (gameSession != null) ? gameSession.CrewManager : null;
			if (crewManager != null && crewManager.IsSinglePlayer)
			{
				string modifiedMessage = ChatMessage.ApplyDistanceEffect(message.Message, message.MessageType.Value, this, Character.Controlled);
				if (!string.IsNullOrEmpty(modifiedMessage))
				{
					crewManager.AddSinglePlayerChatMessage(this.Name, modifiedMessage, message.MessageType.Value, this);
				}
				if (canUseRadio)
				{
					Signal s = new Signal(modifiedMessage, 0, this, radio.Item, 0f, 1f);
					radio.TransmitSignal(s, true);
				}
			}
			this.ShowSpeechBubble(ChatMessage.MessageColor[(int)message.MessageType.Value], message.Message);
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x00030A58 File Offset: 0x0002EC58
		private void UpdateAIChatMessages(float deltaTime)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			List<AIChatMessage> sentMessages = new List<AIChatMessage>();
			foreach (AIChatMessage message in this.aiChatMessageQueue)
			{
				message.SendDelay -= deltaTime;
				if (message.SendDelay <= 0f)
				{
					WifiComponent radio;
					bool canUseRadio = ChatMessage.CanUseRadio(this, out radio, false);
					if (message.MessageType == null)
					{
						message.MessageType = new ChatMessageType?(canUseRadio ? ChatMessageType.Radio : ChatMessageType.Default);
					}
					this.SendSinglePlayerMessage(message, canUseRadio, radio);
					sentMessages.Add(message);
				}
			}
			foreach (AIChatMessage sent in sentMessages)
			{
				sent.SendTime = Timing.TotalTime;
				this.aiChatMessageQueue.Remove(sent);
				if (sent.Identifier != Identifier.Empty)
				{
					this.prevAiChatMessages[sent.Identifier] = (float)sent.SendTime;
				}
			}
			if (this.prevAiChatMessages.Count > 100)
			{
				HashSet<Identifier> toRemove = new HashSet<Identifier>();
				foreach (KeyValuePair<Identifier, float> prevMessage in this.prevAiChatMessages)
				{
					if ((double)prevMessage.Value < Timing.TotalTime - 60.0)
					{
						toRemove.Add(prevMessage.Key);
					}
				}
				foreach (Identifier identifier in toRemove)
				{
					this.prevAiChatMessages.Remove(identifier);
				}
			}
		}

		// Token: 0x060005B3 RID: 1459 RVA: 0x00030C58 File Offset: 0x0002EE58
		public void ForceSay(LocalizedString messageToSay, bool sayInRadio, bool removeQuotes = false, float delay = 0f)
		{
			if (messageToSay.IsNullOrEmpty() || this.SpeechImpediment >= 100f || this.IsDead)
			{
				return;
			}
			if (removeQuotes)
			{
				messageToSay = new TrimLString(messageToSay, TrimLString.Mode.Both, new char[]
				{
					'"',
					'”',
					'“',
					' '
				});
			}
			ChatMessageType messageType = ChatMessageType.Default;
			WifiComponent radio;
			bool canUseRadio = ChatMessage.CanUseRadio(this, out radio, false);
			if (canUseRadio && sayInRadio)
			{
				messageType = ChatMessageType.Radio;
			}
			CoroutineManager.Invoke(delegate
			{
				if (GameMain.Client == null)
				{
					AIChatMessage message = new AIChatMessage(messageToSay.Value, new ChatMessageType?(messageType), default(Identifier), 0f);
					this.SendSinglePlayerMessage(message, canUseRadio, radio);
				}
			}, delay);
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x00030D01 File Offset: 0x0002EF01
		public void SetAllDamage(float damageAmount, float bleedingDamageAmount, float burnDamageAmount)
		{
			this.CharacterHealth.SetAllDamage(damageAmount, bleedingDamageAmount, burnDamageAmount);
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x00030D14 File Offset: 0x0002EF14
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true)
		{
			return this.ApplyAttack(attacker, worldPosition, attack, deltaTime, impulseDirection, playSound, null, 0f);
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x00030D38 File Offset: 0x0002EF38
		public AttackResult ApplyAttack(Character attacker, Vector2 worldPosition, Attack attack, float deltaTime, Vector2 impulseDirection, bool playSound = false, Limb targetLimb = null, float penetration = 0f)
		{
			if (base.Removed)
			{
				string errorMsg = "Tried to apply an attack to a removed character ([name]).\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg.Replace("[name]", this.Name), null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Character.ApplyAttack:RemovedCharacter", GameAnalyticsManager.ErrorSeverity.Error, errorMsg.Replace("[name]", this.SpeciesName.Value));
				return default(AttackResult);
			}
			Limb limbHit = targetLimb;
			float impulseMagnitude = (attack.TargetImpulse + attack.TargetForce) * attack.ImpactMultiplier * deltaTime;
			Vector2 attackImpulse = Vector2.Zero;
			if (Math.Abs(impulseMagnitude) > 0f)
			{
				impulseDirection = ((impulseDirection.LengthSquared() > 0.0001f) ? Vector2.Normalize(impulseDirection) : Vector2.UnitX);
				attackImpulse = impulseDirection * impulseMagnitude;
			}
			AbilityAttackData attackData = new AbilityAttackData(attack, this, attacker);
			IEnumerable<Affliction> attackAfflictions;
			if (attackData.Afflictions != null)
			{
				attackAfflictions = attackData.Afflictions.Union(attack.Afflictions.Keys);
			}
			else
			{
				attackAfflictions = attack.Afflictions.Keys;
			}
			float damageMultiplier = attack.DamageMultiplier * attackData.DamageMultiplier;
			AttackResult attackResult = (targetLimb == null) ? this.AddDamage(worldPosition, attackAfflictions, attack.Stun, playSound, attackImpulse, out limbHit, attacker, damageMultiplier) : this.DamageLimb(worldPosition, targetLimb, attackAfflictions, attack.Stun, playSound, attackImpulse, attacker, damageMultiplier, true, penetration + attackData.AddedPenetration, attackData.ShouldImplode, false, true);
			if (attacker != null)
			{
				AbilityAttackResult abilityAttackResult = new AbilityAttackResult(attackResult);
				attacker.CheckTalents(AbilityEffectType.OnAttackResult, abilityAttackResult);
				this.CheckTalents(AbilityEffectType.OnAttackedResult, abilityAttackResult);
			}
			if (limbHit == null)
			{
				return default(AttackResult);
			}
			Vector2 forceWorld = (attack.TargetImpulseWorld + attack.TargetForceWorld) * attack.ImpactMultiplier;
			if (attacker != null)
			{
				forceWorld.X *= attacker.AnimController.Dir;
			}
			PhysicsBody body = limbHit.body;
			if (body != null)
			{
				body.ApplyLinearImpulse(forceWorld * deltaTime, 64f);
			}
			Limb mainLimb = limbHit.character.AnimController.MainLimb;
			if (limbHit != mainLimb)
			{
				PhysicsBody body2 = mainLimb.body;
				if (body2 != null)
				{
					body2.ApplyLinearImpulse(forceWorld * deltaTime, 64f);
				}
			}
			this.TrySeverLimbJoints(limbHit, attack.SeverLimbsProbability, attackResult.Damage, attacker == null || attacker.IsHuman || attacker.IsPlayer, false, attacker);
			return attackResult;
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x00030F78 File Offset: 0x0002F178
		public void TrySeverLimbJoints(Limb targetLimb, float severLimbsProbability, float damage, bool allowBeheading, bool ignoreSeveranceProbabilityModifier = false, Character attacker = null)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (damage > 0f && damage < targetLimb.Params.MinSeveranceDamage)
			{
				return;
			}
			if (!this.IsDead)
			{
				if (!allowBeheading && targetLimb.type == LimbType.Head)
				{
					return;
				}
				if (!targetLimb.CanBeSeveredAlive)
				{
					return;
				}
			}
			bool wasSevered = false;
			float random = Rand.Value(Rand.RandSync.Unsynced);
			foreach (LimbJoint joint in this.AnimController.LimbJoints)
			{
				if (joint.CanBeSevered)
				{
					Limb referenceLimb = (targetLimb.type == LimbType.Head && targetLimb.Params.ID == 0) ? joint.LimbA : joint.LimbB;
					if (referenceLimb == targetLimb)
					{
						float probability = severLimbsProbability;
						if (!this.IsDead && !ignoreSeveranceProbabilityModifier)
						{
							probability *= joint.Params.SeveranceProbabilityModifier;
						}
						if (probability > 0f && random <= probability)
						{
							bool severed = this.AnimController.SeverLimbJoint(joint);
							if (!wasSevered)
							{
								wasSevered = severed;
							}
							if (severed)
							{
								Limb otherLimb = (joint.LimbA == targetLimb) ? joint.LimbB : joint.LimbA;
								otherLimb.body.ApplyLinearImpulse(targetLimb.LinearVelocity * targetLimb.Mass, 32f);
								if (attacker != null)
								{
									List<StatusEffect> statusEffectList;
									if (this.statusEffects.TryGetValue(ActionType.OnSevered, out statusEffectList))
									{
										foreach (StatusEffect statusEffect in statusEffectList)
										{
											statusEffect.SetUser(attacker);
										}
									}
									List<StatusEffect> limbStatusEffectList;
									if (targetLimb.StatusEffects.TryGetValue(ActionType.OnSevered, out limbStatusEffectList))
									{
										foreach (StatusEffect statusEffect2 in limbStatusEffectList)
										{
											statusEffect2.SetUser(attacker);
										}
									}
								}
								this.ApplyStatusEffects(ActionType.OnSevered, 1f);
								targetLimb.ApplyStatusEffects(ActionType.OnSevered, 1f);
							}
						}
					}
				}
			}
			if (wasSevered)
			{
				EnemyAIController enemyAI = targetLimb.character.AIController as EnemyAIController;
				if (enemyAI != null)
				{
					enemyAI.ReevaluateAttacks();
				}
			}
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x000311B8 File Offset: 0x0002F3B8
		public AttackResult AddDamage(Vector2 worldPosition, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2? attackImpulse = null, Character attacker = null, float damageMultiplier = 1f)
		{
			Limb limb;
			return this.AddDamage(worldPosition, afflictions, stun, playSound, attackImpulse ?? Vector2.Zero, out limb, attacker, damageMultiplier);
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x000311F0 File Offset: 0x0002F3F0
		public AttackResult AddDamage(Vector2 worldPosition, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, out Limb hitLimb, Character attacker = null, float damageMultiplier = 1f)
		{
			hitLimb = null;
			if (base.Removed)
			{
				return default(AttackResult);
			}
			float closestDistance = 0f;
			foreach (Limb limb in this.AnimController.Limbs)
			{
				float distance = Vector2.DistanceSquared(worldPosition, limb.WorldPosition);
				if (hitLimb == null || distance < closestDistance)
				{
					hitLimb = limb;
					closestDistance = distance;
				}
			}
			return this.DamageLimb(worldPosition, hitLimb, afflictions, stun, playSound, attackImpulse, attacker, damageMultiplier, true, 0f, false, false, true);
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x00031278 File Offset: 0x0002F478
		public void RecordKill(Character target)
		{
			AbilityCharacterKill abilityCharacterKill = new AbilityCharacterKill(target, this);
			foreach (Character attackerCrewmember in Character.GetFriendlyCrew(this))
			{
				attackerCrewmember.CheckTalents(AbilityEffectType.OnCrewKillCharacter, abilityCharacterKill);
			}
			this.CheckTalents(AbilityEffectType.OnKillCharacter, abilityCharacterKill);
			if (!this.IsOnPlayerTeam)
			{
				return;
			}
			CreatureMetrics.RecordKill(target.SpeciesName);
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x000312EC File Offset: 0x0002F4EC
		public AttackResult DamageLimb(Vector2 worldPosition, Limb hitLimb, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, Character attacker = null, float damageMultiplier = 1f, bool allowStacking = true, float penetration = 0f, bool shouldImplode = false, bool ignoreDamageOverlay = false, bool recalculateVitality = true)
		{
			if (base.Removed)
			{
				return default(AttackResult);
			}
			this.SetStun(stun, false, false);
			if (attacker != null && attacker != this && attacker.IsOnPlayerTeam && GameMain.NetworkMember != null && !GameMain.NetworkMember.ServerSettings.AllowFriendlyFire && attacker.TeamID == this.TeamID)
			{
				if (afflictions.None((Affliction a) => a.Prefab.IsBuff))
				{
					return default(AttackResult);
				}
			}
			Vector2 dir = hitLimb.WorldPosition - worldPosition;
			if (attackImpulse.LengthSquared() > 0f)
			{
				Vector2 diff = dir;
				if (diff == Vector2.Zero)
				{
					diff = Rand.Vector(1f, Rand.RandSync.Unsynced);
				}
				Vector2 hitPos = hitLimb.SimPosition + ConvertUnits.ToSimUnits(diff);
				hitLimb.body.ApplyLinearImpulse(attackImpulse, hitPos, 32f);
				Limb mainLimb = hitLimb.character.AnimController.MainLimb;
				if (hitLimb != mainLimb)
				{
					mainLimb.body.ApplyLinearImpulse(attackImpulse, hitPos, 64f);
				}
			}
			bool wasDead = this.IsDead;
			Vector2 simPos = hitLimb.SimPosition + ConvertUnits.ToSimUnits(dir);
			AttackResult attackResult = hitLimb.AddDamage(simPos, afflictions, playSound, damageMultiplier, penetration, attacker);
			this.CharacterHealth.ApplyDamage(hitLimb, attackResult, allowStacking, recalculateVitality);
			if (shouldImplode)
			{
				this.Implode(false);
			}
			if (attacker != this)
			{
				bool wasDamageOverlayVisible = this.CharacterHealth.ShowDamageOverlay;
				if (ignoreDamageOverlay)
				{
					this.CharacterHealth.ShowDamageOverlay = false;
				}
				Character.OnAttackedHandler onAttacked = this.OnAttacked;
				if (onAttacked != null)
				{
					onAttacked(attacker, attackResult);
				}
				this.OnAttackedProjSpecific(attacker, attackResult, stun);
				this.CharacterHealth.ShowDamageOverlay = wasDamageOverlayVisible;
				if (!wasDead)
				{
					this.TryAdjustAttackerSkill(attacker, attackResult);
				}
			}
			if (attackResult.Damage > 0f)
			{
				this.LastDamage = attackResult;
				if (attacker != null && attacker != this && !attacker.Removed)
				{
					this.AddAttacker(attacker, attackResult.Damage);
					if (this.IsOnPlayerTeam)
					{
						CreatureMetrics.AddEncounter(attacker.SpeciesName);
					}
					if (attacker.IsOnPlayerTeam)
					{
						CreatureMetrics.AddEncounter(this.SpeciesName);
					}
				}
				this.ApplyStatusEffects(ActionType.OnDamaged, 1f);
				hitLimb.ApplyStatusEffects(ActionType.OnDamaged, 1f);
			}
			if (this.Params.UseBossHealthBar && Character.Controlled != null)
			{
				CharacterTeamType characterTeamType = Character.Controlled.teamID;
				CharacterTeamType? characterTeamType2 = (attacker != null) ? new CharacterTeamType?(attacker.teamID) : null;
				if (characterTeamType == characterTeamType2.GetValueOrDefault() & characterTeamType2 != null)
				{
					CharacterHUD.ShowBossHealthBar(this, attackResult.Damage);
				}
			}
			return attackResult;
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x00031584 File Offset: 0x0002F784
		private void OnAttackedProjSpecific(Character attacker, AttackResult attackResult, float stun)
		{
			if (this.IsDead)
			{
				return;
			}
			if (attacker != null)
			{
				if (attackResult.Damage <= 0.01f)
				{
					return;
				}
			}
			else if (attackResult.Damage <= 1f)
			{
				return;
			}
			if (this.AIState != AIState.PlayDead)
			{
				this.PlaySound(CharacterSound.SoundType.Damage, 1f, 2f);
			}
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x000315D8 File Offset: 0x0002F7D8
		public void TryAdjustAttackerSkill(Character attacker, AttackResult attackResult)
		{
			Character.<>c__DisplayClass724_0 CS$<>8__locals1;
			CS$<>8__locals1.attacker = attacker;
			if (CS$<>8__locals1.attacker == null)
			{
				return;
			}
			if (!CS$<>8__locals1.attacker.IsOnPlayerTeam)
			{
				return;
			}
			if (!(this.AIController is EnemyAIController) && this.TeamID == CS$<>8__locals1.attacker.TeamID)
			{
				return;
			}
			float weaponDamage = 0f;
			float medicalDamage = 0f;
			foreach (Affliction affliction in attackResult.Afflictions)
			{
				if (!affliction.Prefab.IsBuff && (!this.Params.IsMachine || affliction.Prefab.AffectMachines) && !this.Params.Health.ImmunityIdentifiers.Contains(affliction.Identifier))
				{
					if (affliction.Prefab.AfflictionType == AfflictionPrefab.PoisonType || affliction.Prefab.AfflictionType == AfflictionPrefab.ParalysisType)
					{
						if (!this.Params.Health.PoisonImmunity)
						{
							float relativeVitality = this.MaxVitality / 100f;
							float dmg = affliction.Strength;
							if (relativeVitality > 0f)
							{
								dmg /= relativeVitality;
							}
							if (this.PoisonVulnerability > 0f)
							{
								dmg /= this.PoisonVulnerability;
							}
							float strength = this.MaxVitality;
							if (this.Params.AI != null)
							{
								strength = this.Params.AI.CombatStrength;
							}
							float vitalityFactor = MathHelper.Lerp(0.5f, 2f, MathUtils.InverseLerp(0f, 1000f, strength));
							dmg *= vitalityFactor;
							medicalDamage += dmg * affliction.Prefab.MedicalSkillGain;
						}
					}
					else
					{
						medicalDamage += affliction.GetVitalityDecrease(null) * affliction.Prefab.MedicalSkillGain;
					}
					weaponDamage += affliction.GetVitalityDecrease(null) * affliction.Prefab.WeaponsSkillGain;
				}
			}
			if (medicalDamage > 0f)
			{
				Character.<TryAdjustAttackerSkill>g__IncreaseSkillLevel|724_0(Tags.MedicalSkill, medicalDamage, ref CS$<>8__locals1);
			}
			if (weaponDamage > 0f)
			{
				Character.<TryAdjustAttackerSkill>g__IncreaseSkillLevel|724_0(Tags.WeaponsSkill, weaponDamage, ref CS$<>8__locals1);
			}
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x00031824 File Offset: 0x0002FA24
		public void TryAdjustHealerSkill(Character healer, float healthChange = 0f, Affliction affliction = null)
		{
			if (healer == null)
			{
				return;
			}
			bool isEnemy = this.AIController is EnemyAIController || this.TeamID != healer.TeamID;
			if (isEnemy)
			{
				return;
			}
			float medicalGain = healthChange;
			AfflictionPrefab afflictionPrefab = (affliction != null) ? affliction.Prefab : null;
			if (afflictionPrefab != null && afflictionPrefab.IsBuff && (!this.Params.IsMachine || affliction.Prefab.AffectMachines))
			{
				medicalGain += affliction.Strength * affliction.Prefab.MedicalSkillGain;
			}
			if (medicalGain > 0f)
			{
				CharacterInfo characterInfo = healer.Info;
				if (characterInfo == null)
				{
					return;
				}
				characterInfo.ApplySkillGain(Tags.MedicalItem, medicalGain * SkillSettings.Current.SkillIncreasePerFriendlyHealed, false, 2f, false);
			}
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x060005BF RID: 1471 RVA: 0x000318D4 File Offset: 0x0002FAD4
		public bool IsKnockedDownOrRagdolled
		{
			get
			{
				return (this.IsRagdolled && !this.AnimController.IsHangingWithRope) || this.IsKnockedDown;
			}
		}

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x060005C0 RID: 1472 RVA: 0x000318F3 File Offset: 0x0002FAF3
		public bool IsKnockedDown
		{
			get
			{
				return this.CharacterHealth.StunTimer > 1f || this.IsIncapacitated;
			}
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x00031910 File Offset: 0x0002FB10
		public void SetStun(float newStun, bool allowStunDecrease = false, bool isNetworkMessage = false)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && !isNetworkMessage)
			{
				return;
			}
			if (Screen.Selected != GameMain.GameScreen)
			{
				return;
			}
			if ((double)newStun < 0.016666666666666666 && this.Stun <= 0f)
			{
				return;
			}
			if (this.GodMode)
			{
				this.CharacterHealth.Stun = 0f;
				return;
			}
			if (newStun > 0f && this.Params.Health.StunImmunity && (this.EmpVulnerability <= 0f || this.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.EMPType, false) <= 0f))
			{
				return;
			}
			if (newStun > 0f)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode && this.IsHuman)
					{
						newStun = Math.Max(0f, newStun - newStun * networkMember.ServerSettings.PvPStunResist);
					}
				}
			}
			if ((newStun <= this.Stun && !allowStunDecrease) || !MathUtils.IsValid(newStun))
			{
				return;
			}
			if (Math.Sign(newStun) != Math.Sign(this.Stun))
			{
				this.AnimController.ResetPullJoints(null);
			}
			this.CharacterHealth.Stun = newStun;
			if (newStun > 0f)
			{
				if (!this.IsAttachedToController())
				{
					this.SelectedItem = null;
				}
				this.SelectedSecondaryItem = null;
				if (this.SelectedCharacter != null)
				{
					this.DeselectCharacter();
				}
			}
			this.HealthUpdateInterval = 0f;
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x00031A78 File Offset: 0x0002FC78
		public void ApplyStatusEffects(ActionType actionType, float deltaTime)
		{
			if (actionType == ActionType.OnEating)
			{
				float eatingRegen = this.Params.Health.HealthRegenerationWhenEating;
				if (eatingRegen > 0f)
				{
					this.CharacterHealth.ReduceAfflictionOnAllLimbs(AfflictionPrefab.DamageType, eatingRegen * deltaTime, null, null);
				}
			}
			List<StatusEffect> statusEffectList;
			if (this.statusEffects.TryGetValue(actionType, out statusEffectList))
			{
				foreach (StatusEffect statusEffect in statusEffectList)
				{
					if (statusEffect.type != ActionType.OnDamaged || (statusEffect.HasRequiredAfflictions(this.LastDamage) && (!statusEffect.OnlyWhenDamagedByPlayer || (this.LastAttacker != null && this.LastAttacker.IsPlayer))))
					{
						if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyItems) || statusEffect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
						{
							this.targets.Clear();
							statusEffect.AddNearbyTargets(this.WorldPosition, this.targets);
							statusEffect.Apply(actionType, deltaTime, this, this.targets, null);
						}
						else if (statusEffect.targetLimbs != null)
						{
							LimbType[] targetLimbs = statusEffect.targetLimbs;
							for (int i = 0; i < targetLimbs.Length; i++)
							{
								LimbType limbType = targetLimbs[i];
								if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
								{
									foreach (Limb limb in this.AnimController.Limbs)
									{
										if (!limb.IsSevered && limb.type == limbType)
										{
											Character.<ApplyStatusEffects>g__ApplyToLimb|732_0(actionType, deltaTime, statusEffect, this, limb);
										}
									}
								}
								else if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
								{
									Limb limb2 = this.AnimController.GetLimb(limbType, true, false, false);
									if (limb2 != null)
									{
										Character.<ApplyStatusEffects>g__ApplyToLimb|732_0(actionType, deltaTime, statusEffect, this, limb2);
									}
								}
								else if (statusEffect.HasTargetType(StatusEffect.TargetType.LastLimb))
								{
									Limb limb3 = this.AnimController.Limbs.LastOrDefault((Limb l) => l.type == limbType && !l.IsSevered && !l.Hidden);
									if (limb3 != null)
									{
										Character.<ApplyStatusEffects>g__ApplyToLimb|732_0(actionType, deltaTime, statusEffect, this, limb3);
									}
								}
							}
						}
						else if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
						{
							foreach (Limb limb4 in this.AnimController.Limbs)
							{
								if (!limb4.IsSevered)
								{
									Character.<ApplyStatusEffects>g__ApplyToLimb|732_0(actionType, deltaTime, statusEffect, this, limb4);
								}
							}
						}
						if (statusEffect.HasTargetType(StatusEffect.TargetType.This) || statusEffect.HasTargetType(StatusEffect.TargetType.Character))
						{
							statusEffect.Apply(actionType, deltaTime, this, this, null);
						}
						if (statusEffect.HasTargetType(StatusEffect.TargetType.Hull) && this.CurrentHull != null)
						{
							statusEffect.Apply(actionType, deltaTime, this, this.CurrentHull, null);
						}
					}
				}
				if (actionType != ActionType.OnDamaged && actionType != ActionType.OnSevered)
				{
					foreach (Limb limb5 in this.AnimController.Limbs)
					{
						limb5.ApplyStatusEffects(actionType, deltaTime);
					}
				}
			}
			if (actionType != ActionType.OnActive)
			{
				this.CharacterHealth.ApplyAfflictionStatusEffects(actionType);
			}
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x00031DB4 File Offset: 0x0002FFB4
		private void Implode(bool isNetworkMessage = false)
		{
			if (this.CharacterHealth.Unkillable || this.GodMode || this.IsDead)
			{
				return;
			}
			NetworkMember networkMember;
			if (!isNetworkMessage)
			{
				networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return;
				}
			}
			this.CharacterHealth.ApplyAffliction(null, new Affliction(AfflictionPrefab.Pressure, AfflictionPrefab.Pressure.MaxStrength), true, false, true);
			networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsClient || isNetworkMessage)
			{
				this.Kill(CauseOfDeathType.Pressure, null, true, true);
			}
			if (this.IsDead)
			{
				this.BreakJoints();
			}
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x00031E48 File Offset: 0x00030048
		public void BreakJoints()
		{
			Vector2 centerOfMass = this.AnimController.GetCenterOfMass();
			foreach (Limb limb in this.AnimController.Limbs)
			{
				if (!limb.IsSevered)
				{
					limb.AddDamage(limb.SimPosition, 500f, 0f, 0f, false);
					Vector2 diff = centerOfMass - limb.SimPosition;
					if (!MathUtils.IsValid(diff))
					{
						string[] array = new string[7];
						array[0] = "Attempted to apply an invalid impulse to a limb in Character.BreakJoints (";
						int num = 1;
						Vector2 vector = diff;
						array[num] = vector.ToString();
						array[2] = "). Limb position: ";
						array[3] = limb.SimPosition.ToString();
						array[4] = ", center of mass: ";
						int num2 = 5;
						vector = centerOfMass;
						array[num2] = vector.ToString();
						array[6] = ".";
						string errorMsg = string.Concat(array);
						DebugConsole.ThrowError(errorMsg, null, null, false, false);
						GameAnalyticsManager.AddErrorEventOnce("Ragdoll.GetCenterOfMass", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
						return;
					}
					if (!(diff == Vector2.Zero))
					{
						limb.body.ApplyLinearImpulse(diff * 50f, 64f);
					}
				}
			}
			this.ImplodeFX();
			foreach (LimbJoint joint in this.AnimController.LimbJoints)
			{
				if (joint.LimbA.type != LimbType.Head && joint.LimbB.type != LimbType.Head && joint.revoluteJoint != null)
				{
					joint.revoluteJoint.LimitEnabled = false;
				}
			}
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x00031FDC File Offset: 0x000301DC
		private void ImplodeFX()
		{
			Vector2 centerOfMass = this.AnimController.GetCenterOfMass();
			SoundPlayer.PlaySound("implode", this.WorldPosition, null, null, null);
			for (int i = 0; i < 10; i++)
			{
				Particle p = GameMain.ParticleManager.CreateParticle("waterblood", this.WorldPosition + Rand.Vector(5f, Rand.RandSync.Unsynced), Rand.Vector(10f, Rand.RandSync.Unsynced), 0f, null, 0f, null);
				if (p != null)
				{
					p.Size *= 2f;
				}
				GameMain.ParticleManager.CreateParticle("bubbles", ConvertUnits.ToDisplayUnits(centerOfMass) + Rand.Vector(5f, Rand.RandSync.Unsynced), new Vector2(Rand.Range(-50f, 50f, Rand.RandSync.Unsynced), Rand.Range(-100f, 50f, Rand.RandSync.Unsynced)), 0f, null, 0f, null);
				GameMain.ParticleManager.CreateParticle("gib", this.WorldPosition + Rand.Vector(Rand.Range(0f, 50f, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced), Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced), Rand.Range(200f, 700f, Rand.RandSync.Unsynced), null, 0f, null);
			}
			for (int j = 0; j < 30; j++)
			{
				GameMain.ParticleManager.CreateParticle("heavygib", this.WorldPosition + Rand.Vector(Rand.Range(0f, 50f, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced), Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced), Rand.Range(50f, 500f, Rand.RandSync.Unsynced), null, 0f, null);
			}
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x0003219C File Offset: 0x0003039C
		public void TurnIntoHusk(AfflictionPrefabHusk huskInfection = null, bool? playDead = null)
		{
			if (huskInfection == null)
			{
				huskInfection = (AfflictionPrefab.HuskInfection as AfflictionPrefabHusk);
			}
			if (huskInfection == null)
			{
				DebugConsole.ThrowError("Cannot turn " + this.Name + " into husk, because husk infection was not found!", null, AfflictionPrefab.Prefabs.First<AfflictionPrefab>().ContentPackage, false, false);
				return;
			}
			float startStrength = Rand.Range(Math.Max(huskInfection.MaxStrength - 2f, huskInfection.ActiveThreshold), huskInfection.MaxStrength, Rand.RandSync.Unsynced);
			startStrength *= this.MaxVitality / 100f;
			this.CharacterHealth.ApplyAffliction(this.AnimController.MainLimb, huskInfection.Instantiate(startStrength, null), true, false, true);
			if (playDead != null)
			{
				this.AllowPlayDead = playDead.Value;
			}
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x00032254 File Offset: 0x00030454
		public bool IsAttachedToController()
		{
			if (this.SelectedItem == null)
			{
				return false;
			}
			Controller controller = this.SelectedItem.GetComponent<Controller>();
			return controller != null && controller.IsAttachedUser(this);
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x00032284 File Offset: 0x00030484
		public bool ShouldAvoidStayingAttachedToController()
		{
			if (!this.IsAttachedToController())
			{
				return false;
			}
			Deconstructor deconstructor = this.SelectedItem.GetComponent<Deconstructor>();
			if (deconstructor != null)
			{
				return true;
			}
			if (this.IsHuman)
			{
				Character carryingCharacter = this.SelectedItem.GetRootInventoryOwner() as Character;
				if (carryingCharacter != null && this.TeamID != carryingCharacter.TeamID)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x000322DC File Offset: 0x000304DC
		public void Kill(CauseOfDeathType causeOfDeath, Affliction causeOfDeathAffliction, bool isNetworkMessage = false, bool log = true)
		{
			if (this.IsDead || this.CharacterHealth.Unkillable || this.GodMode || base.Removed)
			{
				return;
			}
			this.HealthUpdateInterval = 0f;
			NetworkMember networkMember;
			if (!isNetworkMessage)
			{
				networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return;
				}
			}
			this.AnimController.Frozen = false;
			Character killer = (causeOfDeathAffliction != null) ? causeOfDeathAffliction.Source : null;
			if (this.IsBot)
			{
				foreach (Item item in this.Inventory.AllItems)
				{
					Character equipper = item.Equipper;
					if (equipper != null && equipper.IsPlayer)
					{
						if (item.GetComponents<ItemContainer>().Any((ItemContainer ic) => ic.BlameEquipperForDeath()))
						{
							killer = item.Equipper;
							HumanAIController humanAi = this.AIController as HumanAIController;
							if (humanAi != null)
							{
								humanAi.OnAttacked(killer, new AttackResult(this.MaxVitality, null));
								break;
							}
							break;
						}
					}
				}
			}
			this.CauseOfDeath = new CauseOfDeath(causeOfDeath, (causeOfDeathAffliction != null) ? causeOfDeathAffliction.Prefab : null, killer, this.LastDamageSource);
			if (this.info != null)
			{
				this.info.LastResistanceMultiplierSkillLossDeath = this.GetAbilityResistance(Tags.SkillLossDeathResistance);
				this.info.LastResistanceMultiplierSkillLossRespawn = this.GetAbilityResistance(Tags.SkillLossRespawnResistance);
			}
			this.isDead = true;
			this.ApplyStatusEffects(ActionType.OnBroken, 1f);
			networkMember = GameMain.NetworkMember;
			if (networkMember != null)
			{
				ServerSettings serverSettings = networkMember.ServerSettings;
				if (serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath && GameMain.Client.Character == this)
				{
					CharacterInfo characterInfo = GameMain.Client.CharacterInfo;
					if (characterInfo != null)
					{
						characterInfo.PermanentlyDead = true;
					}
				}
			}
			GameMain.GameSession.RefreshAnyOpenPlayerInfo();
			if (GameAnalyticsManager.SendUserStatistics)
			{
				CharacterPrefab prefab = this.Prefab;
				if (((prefab != null) ? prefab.ContentPackage : null) == ContentPackageManager.VanillaCorePackage && GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
				{
					string causeOfDeathStr = (causeOfDeathAffliction == null) ? causeOfDeath.ToString() : causeOfDeathAffliction.Prefab.Identifier.Value.Replace(" ", "");
					string characterType = Character.<Kill>g__GetCharacterType|739_1(this);
					GameAnalyticsManager.AddDesignEvent("Kill:" + characterType + ":" + causeOfDeathStr);
					if (this.CauseOfDeath.Killer != null)
					{
						GameAnalyticsManager.AddDesignEvent("Kill:" + characterType + ":Killer:" + Character.<Kill>g__GetCharacterType|739_1(this.CauseOfDeath.Killer));
					}
					if (this.CauseOfDeath.DamageSource != null)
					{
						string damageSourceStr = this.CauseOfDeath.DamageSource.ToString();
						Item damageSourceItem = this.CauseOfDeath.DamageSource as Item;
						if (damageSourceItem != null)
						{
							damageSourceStr = damageSourceItem.ToString();
						}
						GameAnalyticsManager.AddDesignEvent("Kill:" + characterType + ":DamageSource:" + damageSourceStr);
					}
				}
			}
			Character.OnDeathHandler onDeath = this.OnDeath;
			if (onDeath != null)
			{
				onDeath(this, this.CauseOfDeath);
			}
			if (this.CauseOfDeath.Type != CauseOfDeathType.Disconnected)
			{
				AbilityCharacterKiller abilityCharacterKiller = new AbilityCharacterKiller(this.CauseOfDeath.Killer);
				this.CheckTalents(AbilityEffectType.OnDieToCharacter, abilityCharacterKiller);
				Character killer2 = this.CauseOfDeath.Killer;
				if (killer2 != null)
				{
					killer2.RecordKill(this);
				}
			}
			if (GameMain.GameSession != null && Screen.Selected == GameMain.GameScreen)
			{
				AchievementManager.OnCharacterKilled(this, this.CauseOfDeath);
			}
			this.KillProjSpecific(causeOfDeath, causeOfDeathAffliction, log);
			if (this.info != null)
			{
				this.info.CauseOfDeath = this.CauseOfDeath;
				this.info.MissionsCompletedSinceDeath = 0;
			}
			this.AnimController.movement = Vector2.Zero;
			this.AnimController.TargetMovement = Vector2.Zero;
			if (!this.LockHands && causeOfDeath != CauseOfDeathType.Disconnected)
			{
				foreach (Item heldItem in this.HeldItems.ToList<Item>())
				{
					Wearable wearable = heldItem.GetComponent<Wearable>();
					if (wearable == null || !wearable.IsActive)
					{
						heldItem.Drop(this, true, true);
					}
				}
			}
			this.SelectedItem = (this.SelectedSecondaryItem = null);
			this.SelectedCharacter = null;
			this.AnimController.ResetPullJoints(null);
			if (this.AnimController.LimbJoints != null)
			{
				foreach (LimbJoint joint in this.AnimController.LimbJoints)
				{
					if (joint.revoluteJoint != null)
					{
						joint.revoluteJoint.MotorEnabled = false;
					}
				}
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null)
			{
				return;
			}
			gameSession.KillCharacter(this);
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x0003277C File Offset: 0x0003097C
		private void KillProjSpecific(CauseOfDeathType causeOfDeath, Affliction causeOfDeathAffliction, bool log)
		{
			HintManager.OnCharacterKilled(this);
			if (GameMain.NetworkMember != null && Character.controlled == this)
			{
				LocalizedString chatMessage = (this.CauseOfDeath.Type == CauseOfDeathType.Affliction) ? this.CauseOfDeath.Affliction.SelfCauseOfDeathDescription : TextManager.Get(new string[]
				{
					"Self_CauseOfDeathDescription." + this.CauseOfDeath.Type.ToString(),
					"Self_CauseOfDeathDescription.Damage"
				});
				if (GameMain.Client != null)
				{
					chatMessage += " " + TextManager.Get("DeathChatNotification");
				}
				RespawnManager.ShowDeathPromptIfNeeded(1f);
				GameMain.NetworkMember.AddChatMessage(chatMessage.Value, ChatMessageType.Dead, "", null, null, PlayerConnectionChangeType.None, null);
				GameMain.LightManager.LosEnabled = false;
				Character.controlled = null;
				Screen selected = Screen.Selected;
				Camera cam = (selected != null) ? selected.Cam : null;
				if (cam != null)
				{
					cam.TargetPos = Vector2.Zero;
					cam.MovementLockTimer = 2f;
					LightManager.ViewTarget = null;
				}
			}
			this.PlaySound(CharacterSound.SoundType.Die, 1f, 0f);
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x000328A4 File Offset: 0x00030AA4
		public void Revive(bool removeAfflictions = true, bool createNetworkEvent = false)
		{
			if (base.Removed)
			{
				DebugConsole.ThrowError("Attempting to revive an already removed character\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			AITarget aiTarget = this.aiTarget;
			if (aiTarget != null)
			{
				aiTarget.Remove();
			}
			this.aiTarget = new AITarget(this);
			if (removeAfflictions)
			{
				this.CharacterHealth.RemoveAllAfflictions();
				this.SetAllDamage(0f, 0f, 0f);
				this.Bloodloss = 0f;
				this.SetStun(0f, true, false);
			}
			this.Oxygen = 100f;
			this.isDead = false;
			if (this.info != null)
			{
				this.info.CauseOfDeath = null;
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null)
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					if (serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath)
					{
						this.info.PermanentlyDead = false;
					}
				}
			}
			foreach (LimbJoint joint in this.AnimController.LimbJoints)
			{
				RevoluteJoint revoluteJoint = joint.revoluteJoint;
				if (revoluteJoint != null)
				{
					revoluteJoint.MotorEnabled = true;
				}
				joint.Enabled = true;
				joint.IsSevered = false;
			}
			foreach (Limb limb in this.AnimController.Limbs)
			{
				if (limb.LightSource != null)
				{
					limb.LightSource.Color = limb.InitialLightSourceColor;
				}
				limb.body.Enabled = true;
				limb.IsSevered = false;
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				gameSession.ReviveCharacter(this);
			}
			if (createNetworkEvent)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsServer)
				{
					GameMain.NetworkMember.CreateEntityEvent(this, default(Character.CharacterStatusEventData));
				}
			}
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00032A58 File Offset: 0x00030C58
		public override void Remove()
		{
			if (base.Removed)
			{
				DebugConsole.ThrowError("Attempting to remove an already removed character\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			DebugConsole.Log(string.Concat(new string[]
			{
				"Removing character ",
				this.Name,
				" (ID: ",
				this.ID.ToString(),
				")"
			}));
			GameClient client = GameMain.Client;
			ClientPeer clientPeer = (client != null) ? client.ClientPeer : null;
			if (clientPeer != null && clientPeer.IsActive)
			{
				CharacterInventory inventory = this.Inventory;
				if (inventory != null)
				{
					inventory.ApplyReceivedState();
				}
			}
			base.Remove();
			foreach (Item heldItem in this.HeldItems.ToList<Item>())
			{
				heldItem.Drop(this, true, true);
			}
			CharacterInfo characterInfo = this.info;
			if (characterInfo != null)
			{
				characterInfo.Remove();
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				CrewManager crewManager = gameSession.CrewManager;
				if (crewManager != null)
				{
					crewManager.KillCharacter(this, false);
				}
			}
			if (Character.Controlled == this)
			{
				Character.Controlled = null;
			}
			Character.CharacterList.Remove(this);
			foreach (Projectile attachedProjectile in this.AttachedProjectiles.ToList<Projectile>())
			{
				attachedProjectile.Unstick();
			}
			this.Latchers.ForEachMod(delegate(LatchOntoAI l)
			{
				if (l != null)
				{
					l.DeattachFromBody(true, 0f);
				}
			});
			this.Latchers.Clear();
			if (this.Inventory != null)
			{
				foreach (Item item in this.Inventory.AllItems)
				{
					EntitySpawner spawner = Entity.Spawner;
					if (spawner != null)
					{
						spawner.AddItemToRemoveQueue(item);
					}
				}
			}
			this.itemSelectedDurations.Clear();
			this.DisposeProjSpecific();
			AITarget aiTarget = this.aiTarget;
			if (aiTarget != null)
			{
				aiTarget.Remove();
			}
			AnimController animController = this.AnimController;
			if (animController != null)
			{
				animController.Remove();
			}
			CharacterHealth characterHealth = this.CharacterHealth;
			if (characterHealth != null)
			{
				characterHealth.Remove();
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c.FocusedCharacter == this)
				{
					c.FocusedCharacter = null;
				}
				if (c.SelectedCharacter == this)
				{
					c.SelectedCharacter = null;
				}
			}
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x00032D10 File Offset: 0x00030F10
		private void DisposeProjSpecific()
		{
			if (Character.controlled == this)
			{
				Character.controlled = null;
				Screen selected = Screen.Selected;
				if (((selected != null) ? selected.Cam : null) != null)
				{
					Screen.Selected.Cam.TargetPos = Vector2.Zero;
					LightManager.ViewTarget = null;
				}
			}
			this.sounds.Clear();
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.CrewManager : null) != null && GameMain.GameSession.CrewManager.GetCharacters().Contains(this))
			{
				GameMain.GameSession.CrewManager.RemoveCharacter(this, false, true);
			}
			GameClient client = GameMain.Client;
			if (((client != null) ? client.Character : null) == this)
			{
				GameMain.Client.Character = null;
			}
			if (LightManager.ViewTarget == this)
			{
				LightManager.ViewTarget = null;
			}
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x00032DCC File Offset: 0x00030FCC
		public void TeleportTo(Vector2 worldPos)
		{
			this.CurrentHull = null;
			this.AnimController.CurrentHull = null;
			base.Submarine = null;
			this.AnimController.SetPosition(ConvertUnits.ToSimUnits(worldPos), false, true, false, true);
			this.AnimController.FindHull(new Vector2?(worldPos), true, false);
			this.CurrentHull = this.AnimController.CurrentHull;
			HumanAIController humanAI = this.AIController as HumanAIController;
			if (humanAI != null)
			{
				IndoorsSteeringManager pathSteering = humanAI.PathSteering;
				if (pathSteering == null)
				{
					return;
				}
				pathSteering.ResetPath();
			}
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x00032E4C File Offset: 0x0003104C
		public static void SaveInventory(Inventory inventory, XElement parentElement)
		{
			if (inventory == null || parentElement == null)
			{
				return;
			}
			IEnumerable<Item> items = inventory.AllItems.Distinct<Item>();
			foreach (Item item in items)
			{
				item.Submarine = inventory.Owner.Submarine;
				XElement itemElement = item.Save(parentElement);
				List<int> slotIndices = inventory.FindIndices(item);
				itemElement.Add(new XAttribute("i", string.Join<int>(",", slotIndices)));
				foreach (ItemContainer container in item.GetComponents<ItemContainer>())
				{
					XElement childInvElement = new XElement("inventory");
					itemElement.Add(childInvElement);
					Character.SaveInventory(container.Inventory, childInvElement);
				}
			}
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x00032F4C File Offset: 0x0003114C
		public void SaveInventory()
		{
			Inventory inventory = this.Inventory;
			CharacterInfo characterInfo = this.Info;
			Character.SaveInventory(inventory, (characterInfo != null) ? characterInfo.InventoryData : null);
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x00032F6B File Offset: 0x0003116B
		public void SpawnInventoryItems(Inventory inventory, ContentXElement itemData)
		{
			this.SpawnInventoryItemsRecursive(inventory, itemData, new List<Item>());
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x00032F7C File Offset: 0x0003117C
		private void SpawnInventoryItemsRecursive(Inventory inventory, ContentXElement element, List<Item> extraDuffelBags)
		{
			foreach (ContentXElement itemElement in element.Elements())
			{
				Item newItem = Item.Load(itemElement, inventory.Owner.Submarine, true, IdRemap.DiscardId);
				if (newItem != null)
				{
					if (!MathUtils.NearlyEqual(newItem.Condition, newItem.MaxCondition, 0.0001f) && GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
					{
						newItem.CreateStatusEvent(true);
					}
					int[] slotIndices = itemElement.GetAttributeIntArray("i", new int[1]);
					if (!slotIndices.Any<int>())
					{
						DebugConsole.ThrowError("Invalid inventory data in character \"" + this.Name + "\" - no slot indices found", null, null, false, false);
					}
					else
					{
						bool canBePutInOriginalInventory;
						if (slotIndices[0] >= inventory.Capacity)
						{
							canBePutInOriginalInventory = false;
							for (int m = 0; m < inventory.Capacity; m++)
							{
								if (inventory.CanBePutInSlot(newItem, m, false))
								{
									slotIndices[0] = m;
									canBePutInOriginalInventory = true;
									break;
								}
							}
						}
						else
						{
							canBePutInOriginalInventory = inventory.CanBePutInSlot(newItem, slotIndices[0], true);
						}
						if (canBePutInOriginalInventory)
						{
							inventory.TryPutItem(newItem, slotIndices[0], false, false, null, true, false, true);
							newItem.ParentInventory = inventory;
							for (int j = 0; j < inventory.Capacity; j++)
							{
								if (slotIndices.Contains(j))
								{
									if (!inventory.GetItemsAt(j).Contains(newItem))
									{
										inventory.ForceToSlot(newItem, j);
									}
								}
								else if (inventory.FindIndices(newItem).Contains(j))
								{
									inventory.ForceRemoveFromSlot(newItem, j);
								}
							}
						}
						else
						{
							if (extraDuffelBags.None((Item i) => i.OwnInventory.CanBePut(newItem)))
							{
								ItemPrefab duffelBagPrefab = MapEntityPrefab.FindByIdentifier("duffelbag".ToIdentifier()) as ItemPrefab;
								if (duffelBagPrefab != null)
								{
									Hull hull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
									Submarine mainSub = Submarine.MainSubs.FirstOrDefault((Submarine s) => s.TeamID == this.TeamID);
									if ((hull == null || hull.Submarine != mainSub) && mainSub != null)
									{
										WayPoint wp = WayPoint.GetRandom(SpawnType.Cargo, null, mainSub, false, null, false) ?? WayPoint.GetRandom(SpawnType.Human, null, mainSub, false, null, false);
										if (wp != null)
										{
											hull = Hull.FindHull(wp.WorldPosition, null, true, true);
										}
									}
									Item newDuffelBag = new Item(duffelBagPrefab, (hull != null) ? CargoManager.GetCargoPos(hull, duffelBagPrefab) : this.Position, ((hull != null) ? hull.Submarine : null) ?? base.Submarine, 0, true);
									extraDuffelBags.Add(newDuffelBag);
								}
							}
							for (int k = 0; k < extraDuffelBags.Count; k++)
							{
								Item duffelBag = extraDuffelBags[k];
								for (int l = 0; l < duffelBag.OwnInventory.Capacity; l++)
								{
									if (duffelBag.OwnInventory.TryPutItem(newItem, l, false, false, null, true, false, true))
									{
										newItem.ParentInventory = duffelBag.OwnInventory;
										break;
									}
								}
							}
						}
						int itemContainerIndex = 0;
						List<ItemContainer> itemContainers = newItem.GetComponents<ItemContainer>().ToList<ItemContainer>();
						foreach (ContentXElement childInvElement in itemElement.Elements())
						{
							if (itemContainerIndex >= itemContainers.Count)
							{
								break;
							}
							if (childInvElement.Name.ToString().Equals("inventory", StringComparison.OrdinalIgnoreCase))
							{
								this.SpawnInventoryItemsRecursive(itemContainers[itemContainerIndex].Inventory, childInvElement, extraDuffelBags);
								itemContainerIndex++;
							}
						}
					}
				}
			}
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x00033360 File Offset: 0x00031560
		public IEnumerable<AttackContext> GetAttackContexts()
		{
			this.currentContexts.Clear();
			if (this.AnimController.InWater)
			{
				this.currentContexts.Add(AttackContext.Water);
			}
			else
			{
				this.currentContexts.Add(AttackContext.Ground);
			}
			if (this.CurrentHull == null)
			{
				this.currentContexts.Add(AttackContext.Outside);
			}
			else
			{
				this.currentContexts.Add(AttackContext.Inside);
			}
			return this.currentContexts;
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x000333CC File Offset: 0x000315CC
		public List<Hull> GetVisibleHulls()
		{
			this.visibleHulls.Clear();
			this.tempList.Clear();
			if (this.CurrentHull != null)
			{
				this.visibleHulls.Add(this.CurrentHull);
				IEnumerable<Hull> adjacentHulls = this.CurrentHull.GetConnectedHulls(true, new int?(1), false);
				float maxDistance = 1000f;
				Func<Gap, bool> <>9__1;
				foreach (Hull hull in adjacentHulls)
				{
					IEnumerable<Gap> connectedGaps = hull.ConnectedGaps;
					Func<Gap, bool> predicate;
					if ((predicate = <>9__1) == null)
					{
						predicate = (<>9__1 = ((Gap g) => g.Open > 0.9f && g.linkedTo.Contains(this.CurrentHull) && (double)Vector2.DistanceSquared(g.WorldPosition, this.WorldPosition) < Math.Pow((double)(maxDistance / 2f), 2.0)));
					}
					if (connectedGaps.Any(predicate) && (double)Vector2.DistanceSquared(hull.WorldPosition, this.WorldPosition) < Math.Pow((double)maxDistance, 2.0))
					{
						this.visibleHulls.Add(hull);
					}
				}
				Func<Gap, bool> <>9__2;
				this.visibleHulls.AddRange(this.CurrentHull.GetLinkedEntities<Hull>(this.tempList, null, delegate(Hull h)
				{
					if (adjacentHulls.Contains(h))
					{
						return false;
					}
					IEnumerable<Gap> connectedGaps2 = h.ConnectedGaps;
					Func<Gap, bool> predicate2;
					if ((predicate2 = <>9__2) == null)
					{
						predicate2 = (<>9__2 = ((Gap g) => g.Open > 0.9f && (double)Vector2.DistanceSquared(g.WorldPosition, this.WorldPosition) < Math.Pow((double)(maxDistance / 2f), 2.0) && this.CanSeeTarget(g, null, false, false)));
					}
					return connectedGaps2.Any(predicate2) && (double)Vector2.DistanceSquared(h.WorldPosition, this.WorldPosition) < Math.Pow((double)maxDistance, 2.0);
				}));
			}
			return this.visibleHulls;
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x00033510 File Offset: 0x00031710
		public Vector2 GetRelativeSimPosition(ISpatialEntity target, Vector2? worldPos = null)
		{
			return Submarine.GetRelativeSimPosition(this, target, worldPos);
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x060005D6 RID: 1494 RVA: 0x0003351A File Offset: 0x0003171A
		public bool IsCaptain
		{
			get
			{
				return this.HasJob("captain");
			}
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x060005D7 RID: 1495 RVA: 0x00033527 File Offset: 0x00031727
		public bool IsEngineer
		{
			get
			{
				return this.HasJob("engineer");
			}
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x060005D8 RID: 1496 RVA: 0x00033534 File Offset: 0x00031734
		public bool IsMechanic
		{
			get
			{
				return this.HasJob("mechanic");
			}
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x060005D9 RID: 1497 RVA: 0x00033541 File Offset: 0x00031741
		public bool IsMedic
		{
			get
			{
				return this.HasJob("medicaldoctor");
			}
		}

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x060005DA RID: 1498 RVA: 0x0003354E File Offset: 0x0003174E
		public bool IsSecurity
		{
			get
			{
				return this.HasJob("securityofficer") || this.HasJob("vipsecurityofficer") || this.HasJob("outpostsecurityofficer");
			}
		}

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x060005DB RID: 1499 RVA: 0x00033577 File Offset: 0x00031777
		public bool IsAssistant
		{
			get
			{
				return this.HasJob("assistant");
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x060005DC RID: 1500 RVA: 0x00033584 File Offset: 0x00031784
		public bool IsWatchman
		{
			get
			{
				return this.HasJob("watchman");
			}
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x060005DD RID: 1501 RVA: 0x00033591 File Offset: 0x00031791
		public bool IsVip
		{
			get
			{
				return this.HasJob("prisoner");
			}
		}

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x060005DE RID: 1502 RVA: 0x0003359E File Offset: 0x0003179E
		public bool IsPrisoner
		{
			get
			{
				return this.HasJob("prisoner");
			}
		}

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x060005DF RID: 1503 RVA: 0x000335AB File Offset: 0x000317AB
		public bool IsKiller
		{
			get
			{
				return this.HasJob("killer");
			}
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x060005E0 RID: 1504 RVA: 0x000335B8 File Offset: 0x000317B8
		// (set) Token: 0x060005E1 RID: 1505 RVA: 0x000335C0 File Offset: 0x000317C0
		public Color? UniqueNameColor { get; set; }

		// Token: 0x060005E2 RID: 1506 RVA: 0x000335CC File Offset: 0x000317CC
		public bool HasJob(string identifier)
		{
			CharacterInfo characterInfo = this.Info;
			Identifier? identifier2;
			Identifier? identifier3;
			if (characterInfo == null)
			{
				identifier2 = null;
				identifier3 = identifier2;
			}
			else
			{
				Job job = characterInfo.Job;
				if (job == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					identifier3 = new Identifier?(job.Prefab.Identifier);
				}
			}
			identifier2 = identifier3;
			return identifier2 == identifier;
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x0003361C File Offset: 0x0003181C
		public bool HasJob(Identifier identifier)
		{
			CharacterInfo characterInfo = this.Info;
			Identifier? identifier2;
			Identifier? identifier3;
			if (characterInfo == null)
			{
				identifier2 = null;
				identifier3 = identifier2;
			}
			else
			{
				Job job = characterInfo.Job;
				if (job == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					identifier3 = new Identifier?(job.Prefab.Identifier);
				}
			}
			identifier2 = identifier3;
			Identifier? identifier4 = new Identifier?(identifier);
			return identifier2 == identifier4;
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x060005E4 RID: 1508 RVA: 0x00033672 File Offset: 0x00031872
		public bool IsProtectedFromPressure
		{
			get
			{
				if (!this.IsImmuneToPressure)
				{
					float num = this.PressureProtection;
					Level loaded = Level.Loaded;
					return num >= ((loaded != null) ? loaded.GetRealWorldDepth(this.WorldPosition.Y) : 1f);
				}
				return true;
			}
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x060005E5 RID: 1509 RVA: 0x000336A9 File Offset: 0x000318A9
		public bool IsImmuneToPressure
		{
			get
			{
				return !this.NeedsAir || this.HasAbilityFlag(AbilityFlags.ImmuneToPressure) || this.GodMode;
			}
		}

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x060005E6 RID: 1510 RVA: 0x000336C4 File Offset: 0x000318C4
		public IReadOnlyCollection<CharacterTalent> CharacterTalents
		{
			get
			{
				return this.characterTalents;
			}
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x000336CC File Offset: 0x000318CC
		public void ResetTalents(int talentPointReduction)
		{
			this.characterTalents.Clear();
			this.abilityResistances.Clear();
			this.abilityFlags = AbilityFlags.None;
			this.CharacterHealth.RemoveAfflictions((Affliction affliction) => affliction.Prefab.AfflictionType == Tags.AfflictionTypeTalentBuff);
			this.statValues.Clear();
			for (int i = 0; i < talentPointReduction; i++)
			{
				int currentLevel = this.info.GetCurrentLevel();
				if (currentLevel <= 0)
				{
					break;
				}
				this.info.SetExperience(this.info.ExperiencePoints - CharacterInfo.ExperienceRequiredPerLevel(currentLevel));
			}
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x00033764 File Offset: 0x00031964
		public void LoadTalents()
		{
			List<Identifier> toBeRemoved = null;
			foreach (Identifier talent in this.info.UnlockedTalents)
			{
				if (!this.GiveTalent(talent, false))
				{
					DebugConsole.AddWarning(this.Name + " had talent that did not exist! Removing talent from CharacterInfo.", null);
					if (toBeRemoved == null)
					{
						toBeRemoved = new List<Identifier>();
					}
					toBeRemoved.Add(talent);
				}
			}
			if (toBeRemoved != null)
			{
				foreach (Identifier removeTalent in toBeRemoved)
				{
					this.Info.UnlockedTalents.Remove(removeTalent);
				}
			}
		}

		// Token: 0x060005E9 RID: 1513 RVA: 0x00033834 File Offset: 0x00031A34
		public bool GiveTalent(Identifier talentIdentifier, bool addingFirstTime = true)
		{
			TalentPrefab talentPrefab = TalentPrefab.TalentPrefabs.Find((TalentPrefab c) => c.Identifier == talentIdentifier);
			if (talentPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to add talent by identifier ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(talentIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(" to character ");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral(", but no such talent exists.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return false;
			}
			return this.GiveTalent(talentPrefab, addingFirstTime);
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x000338C8 File Offset: 0x00031AC8
		public bool GiveTalent(uint talentIdentifier, bool addingFirstTime = true)
		{
			TalentPrefab talentPrefab = TalentPrefab.TalentPrefabs.Find((TalentPrefab c) => c.UintIdentifier == talentIdentifier);
			if (talentPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to add talent by identifier ");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(talentIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(" to character ");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral(", but no such talent exists.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return false;
			}
			return this.GiveTalent(talentPrefab, addingFirstTime);
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x0003395C File Offset: 0x00031B5C
		public bool GiveTalent(TalentPrefab talentPrefab, bool addingFirstTime = true)
		{
			if (this.info == null)
			{
				return false;
			}
			this.info.UnlockedTalents.Add(talentPrefab.Identifier);
			if (this.characterTalents.Any((CharacterTalent t) => t.Prefab == talentPrefab))
			{
				return false;
			}
			CharacterTalent characterTalent = new CharacterTalent(talentPrefab, this);
			this.characterTalents.Add(characterTalent);
			characterTalent.ActivateTalent(addingFirstTime);
			characterTalent.AddedThisRound = addingFirstTime;
			if (addingFirstTime)
			{
				this.OnTalentGiven(talentPrefab);
				string str = "TalentUnlocked:";
				Job job = this.info.Job;
				string eventID = str + ((job != null) ? job.Prefab.Identifier : "None".ToIdentifier()).ToString() + ":" + talentPrefab.Identifier.ToString();
				GameSession gameSession = GameMain.GameSession;
				double? num;
				if (gameSession == null)
				{
					num = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					num = ((campaign != null) ? new double?(campaign.TotalPlayTime) : null);
				}
				double? num2 = num;
				GameAnalyticsManager.AddDesignEvent(eventID, num2.GetValueOrDefault());
			}
			return true;
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00033A88 File Offset: 0x00031C88
		public bool HasTalent(Identifier identifier)
		{
			return this.info != null && this.info.UnlockedTalents.Contains(identifier);
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x00033AA5 File Offset: 0x00031CA5
		public bool IsTalentLocked(Identifier talentIdentifier)
		{
			return this.info == null || this.Info.GetSavedStatValue(StatTypes.LockedTalents, talentIdentifier) >= 1f;
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x00033ACC File Offset: 0x00031CCC
		public bool HasUnlockedAllTalents()
		{
			TalentTree talentTree;
			if (TalentTree.JobTalentTrees.TryGet(this.Info.Job.Prefab.Identifier, out talentTree))
			{
				foreach (TalentSubTree talentSubTree in talentTree.TalentSubTrees)
				{
					foreach (TalentOption talentOption in talentSubTree.TalentOptionStages)
					{
						if (!talentOption.HasMaxTalents(this.info.UnlockedTalents))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x00033B52 File Offset: 0x00031D52
		public bool HasTalents()
		{
			return this.characterTalents.Any<CharacterTalent>();
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x00033B60 File Offset: 0x00031D60
		public void CheckTalents(AbilityEffectType abilityEffectType, AbilityObject abilityObject)
		{
			foreach (CharacterTalent characterTalent in this.CharacterTalents)
			{
				characterTalent.CheckTalent(abilityEffectType, abilityObject);
			}
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x00033BB0 File Offset: 0x00031DB0
		public void CheckTalents(AbilityEffectType abilityEffectType)
		{
			foreach (CharacterTalent characterTalent in this.characterTalents)
			{
				characterTalent.CheckTalent(abilityEffectType, null);
			}
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x00033C04 File Offset: 0x00031E04
		private void OnTalentGiven(TalentPrefab talentPrefab)
		{
			if (!talentPrefab.IsHiddenExtraTalent)
			{
				this.AddMessage(TextManager.Get("talentname." + talentPrefab.Identifier.ToString()).Value, GUIStyle.Yellow, this == Character.Controlled, default(Identifier), null, 3f);
			}
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00033C70 File Offset: 0x00031E70
		public bool IsInSameRoomAs(Character character)
		{
			if (character == this)
			{
				return true;
			}
			if (character.CurrentHull == null || this.CurrentHull == null)
			{
				return false;
			}
			if (character.Submarine != base.Submarine)
			{
				return false;
			}
			if (character.CurrentHull == this.CurrentHull)
			{
				return true;
			}
			this.sameRoomHulls.Clear();
			this.CurrentHull.GetLinkedEntities<Hull>(this.sameRoomHulls, null, null);
			this.sameRoomHulls.Add(this.CurrentHull);
			return this.sameRoomHulls.Contains(character.CurrentHull);
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00033D00 File Offset: 0x00031F00
		public static IEnumerable<Character> GetFriendlyCrew(Character character)
		{
			if (character == null)
			{
				return Enumerable.Empty<Character>();
			}
			return from c in Character.CharacterList
			where c.Info != null && !c.IsDead && !c.IsPet && HumanAIController.IsFriendly(character, c, true, false)
			select c;
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x00033D40 File Offset: 0x00031F40
		public bool HasRecipeForItem(Identifier recipeIdentifier)
		{
			return (GameMain.GameSession != null && GameMain.GameSession.HasUnlockedRecipe(this, recipeIdentifier)) || this.characterTalents.Any((CharacterTalent t) => t.UnlockedRecipes.Contains(recipeIdentifier));
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00033D90 File Offset: 0x00031F90
		public bool HasStoreAccessForItem(ItemPrefab prefab)
		{
			foreach (CharacterTalent talent in this.characterTalents)
			{
				foreach (Identifier unlockedItem in talent.UnlockedStoreItems)
				{
					if (prefab.Tags.Contains(unlockedItem))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00033E30 File Offset: 0x00032030
		public void GiveMoney(int amount)
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign == null)
			{
				return;
			}
			if (amount <= 0)
			{
				return;
			}
			Wallet wallet = campaign.Wallet;
			int prevAmount = wallet.Balance;
			wallet.Give(amount);
			this.OnMoneyChanged(prevAmount, wallet.Balance);
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x00033E7C File Offset: 0x0003207C
		public void SetMoney(int amount)
		{
			if (this.Wallet == null)
			{
				return;
			}
			if (amount == this.Wallet.Balance)
			{
				return;
			}
			int prevAmount = this.Wallet.Balance;
			this.Wallet.Balance = amount;
			this.OnMoneyChanged(prevAmount, this.Wallet.Balance);
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00033ECB File Offset: 0x000320CB
		private void OnMoneyChanged(int prevAmount, int newAmount)
		{
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x00033ED0 File Offset: 0x000320D0
		public float GetStatValue(StatTypes statType, bool includeSaved = true)
		{
			if (this.Info == null)
			{
				return 0f;
			}
			float statValue = 0f;
			float value;
			if (this.statValues.TryGetValue(statType, out value))
			{
				statValue += value;
			}
			if (this.CharacterHealth != null)
			{
				statValue += this.CharacterHealth.GetStatValue(statType);
			}
			if (includeSaved)
			{
				statValue += this.Info.GetSavedStatValue(statType);
			}
			float wearableValue;
			if (this.wearableStatValues.TryGetValue(statType, out wearableValue))
			{
				statValue += wearableValue;
			}
			foreach (Item heldItem in this.HeldItems)
			{
				Holdable holdable = heldItem.GetComponent<Holdable>();
				float holdableValue;
				if (holdable != null && holdable.HoldableStatValues.TryGetValue(statType, out holdableValue))
				{
					statValue += holdableValue;
				}
			}
			return statValue;
		}

		// Token: 0x060005FB RID: 1531 RVA: 0x00033FA0 File Offset: 0x000321A0
		public void OnWearablesChanged()
		{
			HashSet<Wearable> handledWearables = new HashSet<Wearable>();
			this.wearableStatValues.Clear();
			this.wearableSkillModifiers.Clear();
			for (int i = 0; i < this.Inventory.Capacity; i++)
			{
				if (this.Inventory.SlotTypes[i] != InvSlotType.Any && this.Inventory.SlotTypes[i] != InvSlotType.LeftHand && this.Inventory.SlotTypes[i] != InvSlotType.RightHand)
				{
					Item itemAt = this.Inventory.GetItemAt(i);
					Wearable wearable = (itemAt != null) ? itemAt.GetComponent<Wearable>() : null;
					if (wearable != null && !handledWearables.Contains(wearable))
					{
						handledWearables.Add(wearable);
						foreach (KeyValuePair<StatTypes, float> statValuePair in wearable.WearableStatValues)
						{
							if (this.wearableStatValues.ContainsKey(statValuePair.Key))
							{
								Dictionary<StatTypes, float> dictionary = this.wearableStatValues;
								StatTypes key = statValuePair.Key;
								dictionary[key] += statValuePair.Value;
							}
							else
							{
								this.wearableStatValues.Add(statValuePair.Key, statValuePair.Value);
							}
						}
						foreach (KeyValuePair<Identifier, float> skillModifier in wearable.SkillModifiers)
						{
							if (this.wearableSkillModifiers.ContainsKey(skillModifier.Key))
							{
								Dictionary<Identifier, float> dictionary2 = this.wearableSkillModifiers;
								Identifier key2 = skillModifier.Key;
								dictionary2[key2] += skillModifier.Value;
							}
							else
							{
								this.wearableSkillModifiers.Add(skillModifier.Key, skillModifier.Value);
							}
						}
					}
				}
			}
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x00034184 File Offset: 0x00032384
		public void ChangeStat(StatTypes statType, float value)
		{
			if (this.statValues.ContainsKey(statType))
			{
				Dictionary<StatTypes, float> dictionary = this.statValues;
				dictionary[statType] += value;
				return;
			}
			this.statValues.Add(statType, value);
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x000341C8 File Offset: 0x000323C8
		private static StatTypes GetSkillStatType(Identifier skillIdentifier)
		{
			string a = skillIdentifier.Value.ToLowerInvariant();
			if (a == "electrical")
			{
				return StatTypes.ElectricalSkillBonus;
			}
			if (a == "helm")
			{
				return StatTypes.HelmSkillBonus;
			}
			if (a == "mechanical")
			{
				return StatTypes.MechanicalSkillBonus;
			}
			if (a == "medical")
			{
				return StatTypes.MedicalSkillBonus;
			}
			if (!(a == "weapons"))
			{
				return StatTypes.None;
			}
			return StatTypes.WeaponsSkillBonus;
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x00034230 File Offset: 0x00032430
		public void AddAbilityFlag(AbilityFlags abilityFlag)
		{
			this.abilityFlags |= abilityFlag;
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x00034240 File Offset: 0x00032440
		public void RemoveAbilityFlag(AbilityFlags abilityFlag)
		{
			this.abilityFlags &= ~abilityFlag;
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x00034251 File Offset: 0x00032451
		public bool HasAbilityFlag(AbilityFlags abilityFlag)
		{
			return this.abilityFlags.HasFlag(abilityFlag) || this.CharacterHealth.HasFlag(abilityFlag);
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x0003427C File Offset: 0x0003247C
		public float GetAbilityResistance(Identifier resistanceId)
		{
			float resistance = 0f;
			bool hadResistance = false;
			foreach (KeyValuePair<TalentResistanceIdentifier, float> keyValuePair in this.abilityResistances)
			{
				TalentResistanceIdentifier talentResistanceIdentifier;
				float num;
				keyValuePair.Deconstruct(out talentResistanceIdentifier, out num);
				TalentResistanceIdentifier key = talentResistanceIdentifier;
				float value = num;
				Identifier resistanceIdentifier = key.ResistanceIdentifier;
				if (resistanceIdentifier == resistanceId)
				{
					resistance += value;
					hadResistance = true;
				}
			}
			if (!hadResistance)
			{
				return 1f;
			}
			return Math.Max(0f, resistance);
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x00034314 File Offset: 0x00032514
		public float GetAbilityResistance(AfflictionPrefab affliction)
		{
			float resistance = 0f;
			bool hadResistance = false;
			foreach (KeyValuePair<TalentResistanceIdentifier, float> keyValuePair in this.abilityResistances)
			{
				TalentResistanceIdentifier talentResistanceIdentifier;
				float num;
				keyValuePair.Deconstruct(out talentResistanceIdentifier, out num);
				TalentResistanceIdentifier key = talentResistanceIdentifier;
				float value = num;
				Identifier resistanceIdentifier = key.ResistanceIdentifier;
				if (!(resistanceIdentifier == affliction.AfflictionType))
				{
					Identifier resistanceIdentifier2 = key.ResistanceIdentifier;
					if (!(resistanceIdentifier2 == affliction.Identifier))
					{
						continue;
					}
				}
				resistance += value;
				hadResistance = true;
			}
			if (!hadResistance)
			{
				return 1f;
			}
			return Math.Max(0f, resistance);
		}

		// Token: 0x06000603 RID: 1539 RVA: 0x000343C8 File Offset: 0x000325C8
		public void ChangeAbilityResistance(TalentResistanceIdentifier identifier, float value)
		{
			if (!MathUtils.IsValid(value))
			{
				return;
			}
			if (this.abilityResistances.ContainsKey(identifier))
			{
				Dictionary<TalentResistanceIdentifier, float> dictionary = this.abilityResistances;
				dictionary[identifier] *= value;
				return;
			}
			this.abilityResistances.Add(identifier, value);
		}

		// Token: 0x06000604 RID: 1540 RVA: 0x00034413 File Offset: 0x00032613
		public void RemoveAbilityResistance(TalentResistanceIdentifier identifier)
		{
			this.abilityResistances.Remove(identifier);
		}

		// Token: 0x06000605 RID: 1541 RVA: 0x00034422 File Offset: 0x00032622
		public bool IsFriendly(Character other)
		{
			return Character.IsFriendly(this, other);
		}

		// Token: 0x06000606 RID: 1542 RVA: 0x0003442B File Offset: 0x0003262B
		public static bool IsFriendly(Character me, Character other)
		{
			return Character.IsOnFriendlyTeam(me, other) && Character.IsSameSpeciesOrGroup(me, other);
		}

		// Token: 0x06000607 RID: 1543 RVA: 0x00034440 File Offset: 0x00032640
		public static bool IsOnFriendlyTeam(CharacterTeamType myTeam, CharacterTeamType otherTeam)
		{
			if (myTeam == otherTeam)
			{
				return true;
			}
			bool result;
			switch (myTeam)
			{
			case CharacterTeamType.None:
				result = (otherTeam == CharacterTeamType.FriendlyNPC);
				break;
			case CharacterTeamType.Team1:
			case CharacterTeamType.Team2:
				result = (otherTeam == CharacterTeamType.FriendlyNPC);
				break;
			case CharacterTeamType.FriendlyNPC:
			{
				bool flag = otherTeam - CharacterTeamType.Team1 <= 1;
				result = flag;
				break;
			}
			default:
				result = true;
				break;
			}
			return result;
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x0003448C File Offset: 0x0003268C
		public static bool IsOnFriendlyTeam(Character me, Character other)
		{
			return Character.IsOnFriendlyTeam(me.TeamID, other.TeamID);
		}

		// Token: 0x06000609 RID: 1545 RVA: 0x0003449F File Offset: 0x0003269F
		public bool IsOnFriendlyTeam(Character other)
		{
			return Character.IsOnFriendlyTeam(this.TeamID, other.TeamID);
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x000344B2 File Offset: 0x000326B2
		public bool IsOnFriendlyTeam(CharacterTeamType otherTeam)
		{
			return Character.IsOnFriendlyTeam(this.TeamID, otherTeam);
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x000344C0 File Offset: 0x000326C0
		public bool IsSameSpeciesOrGroup(Character other)
		{
			return Character.IsSameSpeciesOrGroup(this, other);
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x000344CC File Offset: 0x000326CC
		public static bool IsSameSpeciesOrGroup(Character me, Character other)
		{
			Identifier speciesName = other.SpeciesName;
			Identifier speciesName2 = me.SpeciesName;
			return speciesName == speciesName2 || CharacterParams.CompareGroup(me.Group, other.Group);
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x00034505 File Offset: 0x00032705
		public bool MatchesSpeciesNameOrGroup(Identifier speciesNameOrGroup)
		{
			return this.Prefab.MatchesSpeciesNameOrGroup(speciesNameOrGroup);
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x00034513 File Offset: 0x00032713
		public void StopClimbing()
		{
			this.AnimController.StopClimbing();
			this.ReleaseSecondaryItem();
		}

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x0600060F RID: 1551 RVA: 0x00034526 File Offset: 0x00032726
		// (set) Token: 0x06000610 RID: 1552 RVA: 0x0003452E File Offset: 0x0003272E
		public float HealthUpdateInterval
		{
			get
			{
				return this.healthUpdateInterval;
			}
			set
			{
				this.healthUpdateInterval = MathHelper.Clamp(value, 0f, this.IsDead ? NetConfig.MaxHealthUpdateIntervalDead : NetConfig.MaxHealthUpdateInterval);
				this.healthUpdateTimer = Math.Min(this.healthUpdateTimer, this.healthUpdateInterval);
			}
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000611 RID: 1553 RVA: 0x0003456C File Offset: 0x0003276C
		public List<CharacterStateInfo> MemState
		{
			get
			{
				return this.memState;
			}
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x00034574 File Offset: 0x00032774
		public List<CharacterStateInfo> MemLocalState
		{
			get
			{
				return this.memLocalState;
			}
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x0003457C File Offset: 0x0003277C
		public void ResetNetState()
		{
			this.memInput.Clear();
			this.memState.Clear();
			this.memLocalState.Clear();
			this.LastNetworkUpdateID = 0;
			this.LastProcessedID = 0;
		}

		// Token: 0x06000614 RID: 1556 RVA: 0x000345B0 File Offset: 0x000327B0
		private void UpdateNetInput()
		{
			if (GameMain.Client != null)
			{
				if (this != Character.Controlled)
				{
					if (GameMain.Client.EndCinematic != null && GameMain.Client.EndCinematic.Running)
					{
						this.AnimController.Frozen = true;
						this.memState.Clear();
						return;
					}
					if ((double)this.lastRecvPositionUpdateTime < NetTime.Now - (double)NetConfig.FreezeCharacterIfPositionDataMissingDelay)
					{
						this.AnimController.Frozen = true;
						this.memState.Clear();
						if ((double)this.lastRecvPositionUpdateTime < NetTime.Now - (double)NetConfig.DisableCharacterIfPositionDataMissingDelay)
						{
							this.Enabled = false;
							return;
						}
					}
				}
				else
				{
					CharacterStateInfo posInfo = new CharacterStateInfo(this.SimPosition, new float?(this.AnimController.Collider.Rotation), this.LastNetworkUpdateID, this.AnimController.TargetDir, this.SelectedCharacter, this.SelectedItem, this.SelectedSecondaryItem, this.AnimController.TargetMovement, this.AnimController.Anim, false);
					this.memLocalState.Add(posInfo);
					Character.InputNetFlags newInput = Character.InputNetFlags.None;
					if (this.IsKeyDown(InputType.Left))
					{
						newInput |= Character.InputNetFlags.Left;
					}
					if (this.IsKeyDown(InputType.Right))
					{
						newInput |= Character.InputNetFlags.Right;
					}
					if (this.IsKeyDown(InputType.Up))
					{
						newInput |= Character.InputNetFlags.Up;
					}
					if (this.IsKeyDown(InputType.Down))
					{
						newInput |= Character.InputNetFlags.Down;
					}
					if (this.IsKeyDown(InputType.Run) || this.ToggleRun)
					{
						newInput |= Character.InputNetFlags.Run;
					}
					if (this.IsKeyDown(InputType.Crouch))
					{
						newInput |= Character.InputNetFlags.Crouch;
					}
					if (this.IsKeyHit(InputType.Select))
					{
						newInput |= Character.InputNetFlags.Select;
					}
					if (this.IsKeyHit(InputType.Deselect))
					{
						newInput |= Character.InputNetFlags.Deselect;
					}
					if (this.IsKeyHit(InputType.Health))
					{
						newInput |= Character.InputNetFlags.Health;
					}
					if (this.IsKeyHit(InputType.Grab))
					{
						newInput |= Character.InputNetFlags.Grab;
					}
					if (this.IsKeyDown(InputType.Use))
					{
						newInput |= Character.InputNetFlags.Use;
					}
					if (this.IsKeyDown(InputType.Aim))
					{
						newInput |= Character.InputNetFlags.Aim;
					}
					if (this.IsKeyDown(InputType.Shoot))
					{
						newInput |= Character.InputNetFlags.Shoot;
					}
					if (this.IsKeyDown(InputType.Attack))
					{
						newInput |= Character.InputNetFlags.Attack;
					}
					if (this.IsKeyDown(InputType.Ragdoll))
					{
						newInput |= Character.InputNetFlags.Ragdoll;
					}
					if (this.AnimController.Dir < 0f)
					{
						newInput |= Character.InputNetFlags.FacingLeft;
					}
					Vector2 relativeCursorPos = this.cursorPosition - this.AimRefPosition;
					relativeCursorPos.Normalize();
					ushort intAngle = (ushort)(65535.0 * Math.Atan2((double)relativeCursorPos.Y, (double)relativeCursorPos.X) / 6.283185307179586);
					Character.NetInputMem newMem = new Character.NetInputMem
					{
						states = newInput,
						intAim = intAngle
					};
					if (this.FocusedCharacter != null && this.FocusedCharacter.CampaignInteractionType != CampaignMode.InteractionType.None && newMem.states.HasFlag(Character.InputNetFlags.Use))
					{
						newMem.interact = this.FocusedCharacter.ID;
					}
					else
					{
						if (newMem.states.HasFlag(Character.InputNetFlags.Use))
						{
							Character focusedCharacter = this.FocusedCharacter;
							if (focusedCharacter != null && focusedCharacter.IsPet)
							{
								newMem.interact = this.FocusedCharacter.ID;
								goto IL_373;
							}
						}
						if (this.focusedItem != null && !Barotrauma.Inventory.DraggingItemToWorld && !newMem.states.HasFlag(Character.InputNetFlags.Grab) && !newMem.states.HasFlag(Character.InputNetFlags.Health))
						{
							newMem.interact = this.focusedItem.ID;
						}
						else if (this.FocusedCharacter != null)
						{
							newMem.interact = this.FocusedCharacter.ID;
						}
					}
					IL_373:
					this.memInput.Insert(0, newMem);
					this.LastNetworkUpdateID += 1;
					if (this.memInput.Count > 60)
					{
						this.memInput.RemoveRange(60, this.memInput.Count - 60);
						return;
					}
				}
			}
			else
			{
				this.AnimController.Frozen = false;
			}
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x00034A17 File Offset: 0x00032C17
		[CompilerGenerated]
		private void <ControlLocalPlayer>g__ResetInputIfPrimaryMouse|81_1(InputType inputType)
		{
			if (GameSettings.CurrentConfig.KeyMap.Bindings[inputType].MouseButton == MouseButton.PrimaryMouse)
			{
				this.keys[(int)inputType].Reset();
			}
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x00034A68 File Offset: 0x00032C68
		[CompilerGenerated]
		internal static void <ClientEventRead>g__ReadItemTeamChange|113_0(IReadMessage msg, bool allowStealing)
		{
			Character.ItemTeamChange itemTeamChange = INetSerializableStruct.Read<Character.ItemTeamChange>(msg);
			foreach (ushort itemID in itemTeamChange.ItemIds)
			{
				Item item = Entity.FindEntityByID(itemID) as Item;
				if (item != null)
				{
					item.AllowStealing = allowStealing;
					WifiComponent wifiComponent = item.GetComponent<WifiComponent>();
					if (wifiComponent != null)
					{
						wifiComponent.TeamID = itemTeamChange.TeamId;
					}
					IdCard idCard = item.GetComponent<IdCard>();
					if (idCard != null)
					{
						idCard.TeamID = itemTeamChange.TeamId;
						idCard.SubmarineSpecificID = 0;
					}
				}
			}
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x00034B37 File Offset: 0x00032D37
		[CompilerGenerated]
		internal static bool <Control>g__CanUseItemsWhenSelected|642_0(Item item)
		{
			return item == null || !item.Prefab.DisableItemUsageWhenSelected;
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00034B4C File Offset: 0x00032D4C
		[CompilerGenerated]
		private void <Control>g__tryUseItem|642_1(Item item, float deltaTime)
		{
			if (this.IsKeyDown(InputType.Aim) || !item.RequireAimToSecondaryUse)
			{
				item.SecondaryUse(deltaTime, this);
			}
			if (this.IsKeyDown(InputType.Use) && !item.IsShootable && (!item.RequireAimToUse || this.IsKeyDown(InputType.Aim)))
			{
				item.Use(deltaTime, this, null, null, null);
			}
			if (this.IsKeyDown(InputType.Shoot) && item.IsShootable)
			{
				if (!item.RequireAimToUse || this.IsKeyDown(InputType.Aim))
				{
					item.Use(deltaTime, this, null, null, null);
					return;
				}
				if (item.RequireAimToUse && !this.IsKeyDown(InputType.Aim))
				{
					HintManager.OnShootWithoutAiming(this, item);
				}
			}
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x00034BE6 File Offset: 0x00032DE6
		[CompilerGenerated]
		private bool <IsInventoryAccessibleTo>g__IsOnSameTeam|661_0(ref Character.<>c__DisplayClass661_0 A_1)
		{
			return A_1.character.TeamID == this.teamID;
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x00034BFB File Offset: 0x00032DFB
		[CompilerGenerated]
		private bool <IsInventoryAccessibleTo>g__IsFriendlyPet|661_1(ref Character.<>c__DisplayClass661_0 A_1)
		{
			return this.IsPet && A_1.character.IsFriendly(this);
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x00034C4C File Offset: 0x00032E4C
		[CompilerGenerated]
		internal static bool <CanInteractWith>g__CheckBody|671_0(Body body, Item item)
		{
			if (body == null)
			{
				return true;
			}
			Item item2;
			if ((item2 = (body.UserData as Item)) == null)
			{
				ItemComponent itemComponent = body.UserData as ItemComponent;
				item2 = ((itemComponent != null) ? itemComponent.Item : null);
			}
			Item otherItem = item2;
			if (otherItem != item)
			{
				ItemComponent itemComponent2 = body.UserData as ItemComponent;
				if (((itemComponent2 != null) ? itemComponent2.Item : null) != item)
				{
					Door door = (otherItem != null) ? otherItem.GetComponent<Door>() : null;
					if (door == null || !door.IsOpen)
					{
						Fixture lastPickedFixture = Submarine.LastPickedFixture;
						if (((lastPickedFixture != null) ? lastPickedFixture.UserData : null) as Item != item)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x00034CD8 File Offset: 0x00032ED8
		[CompilerGenerated]
		internal static Vector2 <CanInteractWith>g__GetPosition|671_1(Submarine submarine, Item item, Vector2 simPosition)
		{
			Vector2 position = simPosition;
			Submarine submarine2 = item.Submarine;
			Vector2 itemSubPos = (submarine2 != null) ? submarine2.SimPosition : Vector2.Zero;
			Vector2 subPos = (submarine != null) ? submarine.SimPosition : Vector2.Zero;
			if (submarine == null && item.Submarine != null)
			{
				position += itemSubPos;
			}
			else if (submarine != null && item.Submarine == null)
			{
				position -= subPos;
			}
			else if (submarine != item.Submarine && submarine != null)
			{
				position += itemSubPos;
				position -= subPos;
			}
			return position;
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x00034D58 File Offset: 0x00032F58
		[CompilerGenerated]
		private bool <Update>g__bodyMovingTooFast|679_1(PhysicsBody body)
		{
			return body.LinearVelocity.LengthSquared() > 64f || (!this.InWater && body.LinearVelocity.Y < -5f);
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x00034D98 File Offset: 0x00032F98
		[CompilerGenerated]
		private bool <Update>g__MustDeselect|679_0(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (this.IsAIControlled && !this.CanInteract && this.IsAttachedToController())
			{
				return false;
			}
			if (!this.CanInteractWith(item, true))
			{
				return true;
			}
			bool hasSelectableComponent = false;
			foreach (ItemComponent component in item.Components)
			{
				if (component.CanBeSelected && component.HasRequiredItems(this, false, null))
				{
					hasSelectableComponent = true;
					break;
				}
			}
			return !hasSelectableComponent;
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x00034E5B File Offset: 0x0003305B
		[CompilerGenerated]
		internal static void <TryAdjustAttackerSkill>g__IncreaseSkillLevel|724_0(Identifier skill, float damage, ref Character.<>c__DisplayClass724_0 A_2)
		{
			CharacterInfo characterInfo = A_2.attacker.Info;
			if (characterInfo == null)
			{
				return;
			}
			characterInfo.ApplySkillGain(skill, damage * SkillSettings.Current.SkillIncreasePerHostileDamage, false, 1f, false);
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x00034E88 File Offset: 0x00033088
		[CompilerGenerated]
		internal static void <ApplyStatusEffects>g__ApplyToLimb|732_0(ActionType actionType, float deltaTime, StatusEffect statusEffect, Character character, Limb limb)
		{
			statusEffect.sourceBody = limb.body;
			statusEffect.Apply(actionType, deltaTime, character, limb, null);
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x00034EB8 File Offset: 0x000330B8
		[CompilerGenerated]
		internal static string <Kill>g__GetCharacterType|739_1(Character character)
		{
			if (character.IsPlayer)
			{
				return "Player";
			}
			if (character.AIController is EnemyAIController)
			{
				return "Enemy" + character.SpeciesName.ToString();
			}
			if (character.AIController is HumanAIController && character.TeamID == CharacterTeamType.Team2)
			{
				return "EnemyHuman";
			}
			if (character.Info != null && character.TeamID == CharacterTeamType.Team1)
			{
				return "AICrew";
			}
			if (character.Info != null && character.TeamID == CharacterTeamType.FriendlyNPC)
			{
				return "FriendlyNPC";
			}
			return "Unknown";
		}

		// Token: 0x0400027E RID: 638
		public static bool DisableControls;

		// Token: 0x0400027F RID: 639
		public static bool DebugDrawInteract;

		// Token: 0x04000280 RID: 640
		protected float soundTimer;

		// Token: 0x04000281 RID: 641
		protected float hudInfoTimer = 1f;

		// Token: 0x04000282 RID: 642
		protected bool hudInfoVisible;

		// Token: 0x04000283 RID: 643
		private float findFocusedTimer;

		// Token: 0x04000284 RID: 644
		protected float lastRecvPositionUpdateTime;

		// Token: 0x04000285 RID: 645
		private const float DefaultHudInfoHeight = 78f;

		// Token: 0x04000286 RID: 646
		private float hudInfoHeight = 78f;

		// Token: 0x04000287 RID: 647
		private List<CharacterSound> sounds;

		// Token: 0x04000288 RID: 648
		public bool ExternalHighlight;

		// Token: 0x0400028B RID: 651
		private static Character controlled;

		// Token: 0x0400028C RID: 652
		private Dictionary<object, HUDProgressBar> hudProgressBars;

		// Token: 0x0400028D RID: 653
		private readonly List<KeyValuePair<object, HUDProgressBar>> progressBarRemovals = new List<KeyValuePair<object, HUDProgressBar>>();

		// Token: 0x0400028E RID: 654
		private float blurStrength;

		// Token: 0x0400028F RID: 655
		private float distortStrength;

		// Token: 0x04000290 RID: 656
		private float radialDistortStrength;

		// Token: 0x04000291 RID: 657
		private float chromaticAberrationStrength;

		// Token: 0x04000293 RID: 659
		private float grainStrength;

		// Token: 0x04000294 RID: 660
		private readonly List<ParticleEmitter> bloodEmitters = new List<ParticleEmitter>();

		// Token: 0x04000295 RID: 661
		private readonly List<ParticleEmitter> damageEmitters = new List<ParticleEmitter>();

		// Token: 0x04000296 RID: 662
		private readonly List<ParticleEmitter> gibEmitters = new List<ParticleEmitter>();

		// Token: 0x04000297 RID: 663
		private List<Character.GUIMessage> guiMessages = new List<Character.GUIMessage>();

		// Token: 0x04000298 RID: 664
		private readonly List<Character.ObjectiveEntity> activeObjectiveEntities = new List<Character.ObjectiveEntity>();

		// Token: 0x04000299 RID: 665
		private static readonly List<Character.SpeechBubble> speechBubbles = new List<Character.SpeechBubble>();

		// Token: 0x0400029A RID: 666
		private Character.SpeechBubble textlessSpeechBubble;

		// Token: 0x0400029B RID: 667
		private float pressureEffectTimer;

		// Token: 0x0400029C RID: 668
		private readonly List<Item> previousInteractablesInRange = new List<Item>();

		// Token: 0x0400029D RID: 669
		private readonly List<Item> interactablesInRange = new List<Item>();

		// Token: 0x0400029E RID: 670
		private bool wasFiring;

		// Token: 0x0400029F RID: 671
		private readonly List<Item> debugInteractablesInRange = new List<Item>();

		// Token: 0x040002A0 RID: 672
		private readonly List<Item> debugInteractablesAtCursor = new List<Item>();

		// Token: 0x040002A1 RID: 673
		[TupleElementNames(new string[]
		{
			"item",
			"dist"
		})]
		private readonly List<ValueTuple<Item, float>> debugInteractablesNearCursor = new List<ValueTuple<Item, float>>();

		// Token: 0x040002A2 RID: 674
		private readonly List<CharacterSound> matchingSounds = new List<CharacterSound>();

		// Token: 0x040002A3 RID: 675
		private SoundChannel soundChannel;

		// Token: 0x040002A4 RID: 676
		public static readonly List<Character> CharacterList = new List<Character>();

		// Token: 0x040002A5 RID: 677
		public static int CharacterUpdateInterval = 1;

		// Token: 0x040002A6 RID: 678
		private static int characterUpdateTick = 1;

		// Token: 0x040002A7 RID: 679
		public const float MaxHighlightDistance = 150f;

		// Token: 0x040002A8 RID: 680
		public const float MaxDragDistance = 200f;

		// Token: 0x040002A9 RID: 681
		private bool initialized;

		// Token: 0x040002AA RID: 682
		private bool enabled;

		// Token: 0x040002AB RID: 683
		private bool disabledByEvent;

		// Token: 0x040002AC RID: 684
		public Hull PreviousHull;

		// Token: 0x040002AD RID: 685
		public Hull CurrentHull;

		// Token: 0x040002B1 RID: 689
		public readonly Dictionary<Identifier, SerializableProperty> Properties;

		// Token: 0x040002B2 RID: 690
		protected Key[] keys;

		// Token: 0x040002B3 RID: 691
		private HumanPrefab humanPrefab;

		// Token: 0x040002B4 RID: 692
		private Identifier? faction;

		// Token: 0x040002B5 RID: 693
		private CharacterTeamType teamID;

		// Token: 0x040002B6 RID: 694
		private CharacterTeamType? originalTeamID;

		// Token: 0x040002B7 RID: 695
		private Wallet wallet;

		// Token: 0x040002B8 RID: 696
		public readonly HashSet<LatchOntoAI> Latchers = new HashSet<LatchOntoAI>();

		// Token: 0x040002B9 RID: 697
		public readonly HashSet<Projectile> AttachedProjectiles = new HashSet<Projectile>();

		// Token: 0x040002BA RID: 698
		protected readonly Dictionary<string, ActiveTeamChange> activeTeamChanges = new Dictionary<string, ActiveTeamChange>();

		// Token: 0x040002BB RID: 699
		protected ActiveTeamChange currentTeamChange;

		// Token: 0x040002BC RID: 700
		private const string OriginalChangeTeamIdentifier = "original";

		// Token: 0x040002BE RID: 702
		public bool IsCriminal;

		// Token: 0x040002BF RID: 703
		public bool IsActingOffensively;

		// Token: 0x040002C0 RID: 704
		public bool IsHostileEscortee;

		// Token: 0x040002C1 RID: 705
		public CombatAction CombatAction;

		// Token: 0x040002C2 RID: 706
		public readonly AnimController AnimController;

		// Token: 0x040002C3 RID: 707
		private Vector2 cursorPosition;

		// Token: 0x040002C4 RID: 708
		protected float oxygenAvailable;

		// Token: 0x040002C5 RID: 709
		public readonly string Seed;

		// Token: 0x040002C6 RID: 710
		protected Item focusedItem;

		// Token: 0x040002C7 RID: 711
		private Character selectedCharacter;

		// Token: 0x040002C8 RID: 712
		private Character selectedBy;

		// Token: 0x040002C9 RID: 713
		private const int maxLastAttackerCount = 4;

		// Token: 0x040002CA RID: 714
		private readonly List<Character.Attacker> lastAttackers = new List<Character.Attacker>();

		// Token: 0x040002CD RID: 717
		public Entity LastDamageSource;

		// Token: 0x040002CE RID: 718
		public AttackResult LastDamage;

		// Token: 0x040002CF RID: 719
		private readonly Dictionary<ItemPrefab, double> itemSelectedDurations = new Dictionary<ItemPrefab, double>();

		// Token: 0x040002D0 RID: 720
		private double itemSelectedTime;

		// Token: 0x040002D2 RID: 722
		public readonly CharacterPrefab Prefab;

		// Token: 0x040002D3 RID: 723
		public readonly CharacterParams Params;

		// Token: 0x040002D5 RID: 725
		public LocalizedString TraitorCurrentObjective = "";

		// Token: 0x040002D6 RID: 726
		private float attackCoolDown;

		// Token: 0x040002D7 RID: 727
		private readonly Dictionary<ActionType, List<StatusEffect>> statusEffects = new Dictionary<ActionType, List<StatusEffect>>();

		// Token: 0x040002D9 RID: 729
		private CharacterInfo info;

		// Token: 0x040002DA RID: 730
		private float hideFaceTimer;

		// Token: 0x040002DB RID: 731
		private Vector2 lastInventoryItemSetTransformPosition;

		// Token: 0x040002E0 RID: 736
		private Action<Character, Character> onCustomInteract;

		// Token: 0x040002E1 RID: 737
		public ConversationAction ActiveConversation;

		// Token: 0x040002E2 RID: 738
		public bool RequireConsciousnessForCustomInteract = true;

		// Token: 0x040002E3 RID: 739
		private float lockHandsTimer;

		// Token: 0x040002E6 RID: 742
		private float lowPassMultiplier;

		// Token: 0x040002E7 RID: 743
		private float obstructVisionAmount;

		// Token: 0x040002E8 RID: 744
		private double pressureProtectionLastSet;

		// Token: 0x040002E9 RID: 745
		private float pressureProtection;

		// Token: 0x040002EA RID: 746
		public const float KnockbackCooldown = 5f;

		// Token: 0x040002EB RID: 747
		public float KnockbackCooldownTimer;

		// Token: 0x040002EC RID: 748
		private float ragdollingLockTimer;

		// Token: 0x040002ED RID: 749
		public bool IsRagdolled;

		// Token: 0x040002EE RID: 750
		public bool IsForceRagdolled;

		// Token: 0x040002EF RID: 751
		public bool FollowCursor = true;

		// Token: 0x040002F2 RID: 754
		public bool DisableHealthWindow;

		// Token: 0x040002F3 RID: 755
		private bool speechImpedimentSet;

		// Token: 0x040002F4 RID: 756
		private float speechImpediment;

		// Token: 0x040002F5 RID: 757
		private float textChatVolume;

		// Token: 0x040002F9 RID: 761
		private Item _selectedItem;

		// Token: 0x040002FC RID: 764
		private bool isDead;

		// Token: 0x040002FF RID: 767
		public bool GodMode;

		// Token: 0x04000300 RID: 768
		public CampaignMode.InteractionType CampaignInteractionType;

		// Token: 0x04000301 RID: 769
		public Identifier MerchantIdentifier;

		// Token: 0x04000302 RID: 770
		private bool accessRemovedCharacterErrorShown;

		// Token: 0x04000303 RID: 771
		public HashSet<Identifier> MarkedAsLooted = new HashSet<Identifier>();

		// Token: 0x04000304 RID: 772
		public Character.OnDeathHandler OnDeath;

		// Token: 0x04000305 RID: 773
		public Character.OnAttackedHandler OnAttacked;

		// Token: 0x04000306 RID: 774
		private static readonly ImmutableDictionary<Identifier, StatTypes> overrideStatTypes = new Dictionary<Identifier, StatTypes>
		{
			{
				new Identifier("helm"),
				StatTypes.HelmSkillOverride
			},
			{
				new Identifier("medical"),
				StatTypes.MedicalSkillOverride
			},
			{
				new Identifier("weapons"),
				StatTypes.WeaponsSkillOverride
			},
			{
				new Identifier("electrical"),
				StatTypes.ElectricalSkillOverride
			},
			{
				new Identifier("mechanical"),
				StatTypes.MechanicalSkillOverride
			}
		}.ToImmutableDictionary<Identifier, StatTypes>();

		// Token: 0x04000309 RID: 777
		private double disableRunningLastSet;

		// Token: 0x0400030A RID: 778
		public bool ToggleRun;

		// Token: 0x0400030B RID: 779
		private float greatestNegativeSpeedMultiplier = 1f;

		// Token: 0x0400030C RID: 780
		private float greatestPositiveSpeedMultiplier = 1f;

		// Token: 0x0400030E RID: 782
		private double propulsionSpeedMultiplierLastSet;

		// Token: 0x0400030F RID: 783
		private float propulsionSpeedMultiplier;

		// Token: 0x04000310 RID: 784
		private float greatestNegativeHealthMultiplier = 1f;

		// Token: 0x04000311 RID: 785
		private float greatestPositiveHealthMultiplier = 1f;

		// Token: 0x04000314 RID: 788
		private const float cursorFollowMargin = 40f;

		// Token: 0x04000315 RID: 789
		private Character.AttackTargetData currentAttackTarget;

		// Token: 0x04000316 RID: 790
		private Stopwatch sw;

		// Token: 0x04000317 RID: 791
		private float _selectedItemPriority;

		// Token: 0x04000318 RID: 792
		private Item _foundItem;

		// Token: 0x04000319 RID: 793
		private float despawnTimer;

		// Token: 0x0400031A RID: 794
		private readonly float maxAIRange = 20000f;

		// Token: 0x0400031B RID: 795
		private readonly float aiTargetChangeSpeed = 5f;

		// Token: 0x0400031C RID: 796
		private readonly List<AIChatMessage> aiChatMessageQueue = new List<AIChatMessage>();

		// Token: 0x0400031D RID: 797
		private readonly Dictionary<Identifier, float> prevAiChatMessages = new Dictionary<Identifier, float>();

		// Token: 0x0400031E RID: 798
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x0400031F RID: 799
		private readonly HashSet<AttackContext> currentContexts = new HashSet<AttackContext>();

		// Token: 0x04000320 RID: 800
		private readonly List<Hull> visibleHulls = new List<Hull>();

		// Token: 0x04000321 RID: 801
		private readonly HashSet<Hull> tempList = new HashSet<Hull>();

		// Token: 0x04000323 RID: 803
		private readonly List<CharacterTalent> characterTalents = new List<CharacterTalent>();

		// Token: 0x04000324 RID: 804
		private readonly HashSet<Hull> sameRoomHulls = new HashSet<Hull>();

		// Token: 0x04000325 RID: 805
		private readonly Dictionary<StatTypes, float> statValues = new Dictionary<StatTypes, float>();

		// Token: 0x04000326 RID: 806
		private readonly Dictionary<StatTypes, float> wearableStatValues = new Dictionary<StatTypes, float>();

		// Token: 0x04000327 RID: 807
		private readonly Dictionary<Identifier, float> wearableSkillModifiers = new Dictionary<Identifier, float>();

		// Token: 0x04000328 RID: 808
		private AbilityFlags abilityFlags;

		// Token: 0x04000329 RID: 809
		private readonly Dictionary<TalentResistanceIdentifier, float> abilityResistances = new Dictionary<TalentResistanceIdentifier, float>();

		// Token: 0x0400032A RID: 810
		private Character.InputNetFlags dequeuedInput;

		// Token: 0x0400032B RID: 811
		private Character.InputNetFlags prevDequeuedInput;

		// Token: 0x0400032C RID: 812
		public ushort LastNetworkUpdateID;

		// Token: 0x0400032D RID: 813
		public ushort LastProcessedID;

		// Token: 0x0400032E RID: 814
		private readonly List<Character.NetInputMem> memInput = new List<Character.NetInputMem>();

		// Token: 0x0400032F RID: 815
		private readonly List<CharacterStateInfo> memState = new List<CharacterStateInfo>();

		// Token: 0x04000330 RID: 816
		private readonly List<CharacterStateInfo> memLocalState = new List<CharacterStateInfo>();

		// Token: 0x04000331 RID: 817
		public float healthUpdateTimer;

		// Token: 0x04000332 RID: 818
		private float healthUpdateInterval;

		// Token: 0x04000333 RID: 819
		public bool isSynced;

		// Token: 0x02000686 RID: 1670
		private class GUIMessage
		{
			// Token: 0x17001990 RID: 6544
			// (get) Token: 0x060065F8 RID: 26104 RVA: 0x0034682A File Offset: 0x00344A2A
			// (set) Token: 0x060065F9 RID: 26105 RVA: 0x00346834 File Offset: 0x00344A34
			public int Value
			{
				get
				{
					return this._value;
				}
				set
				{
					this._value = value;
					this.Text = this.RawText.Replace("[value]", this._value.ToString());
					this.Size = GUIStyle.Font.MeasureString(this.Text, false);
				}
			}

			// Token: 0x060065FA RID: 26106 RVA: 0x00346888 File Offset: 0x00344A88
			public GUIMessage(string rawText, Color color, float delay, Identifier identifier = default(Identifier), int? value = null, float lifeTime = 3f)
			{
				this.Text = rawText;
				this.RawText = rawText;
				if (value != null)
				{
					this.Text = rawText.Replace("[value]", value.Value.ToString());
					this.Value = value.Value;
				}
				this.Timer = -delay;
				this.Size = GUIStyle.Font.MeasureString(this.Text, false);
				this.Color = color;
				this.Identifier = identifier;
				this.Lifetime = lifeTime;
			}

			// Token: 0x0400372A RID: 14122
			public string RawText;

			// Token: 0x0400372B RID: 14123
			public Identifier Identifier;

			// Token: 0x0400372C RID: 14124
			public string Text;

			// Token: 0x0400372D RID: 14125
			private int _value;

			// Token: 0x0400372E RID: 14126
			public Color Color;

			// Token: 0x0400372F RID: 14127
			public float Lifetime;

			// Token: 0x04003730 RID: 14128
			public float Timer;

			// Token: 0x04003731 RID: 14129
			public Vector2 Size;

			// Token: 0x04003732 RID: 14130
			public bool PlaySound;
		}

		// Token: 0x02000687 RID: 1671
		public class ObjectiveEntity
		{
			// Token: 0x060065FB RID: 26107 RVA: 0x0034691C File Offset: 0x00344B1C
			public ObjectiveEntity(Entity entity, Sprite sprite, Color? color = null)
			{
				this.Entity = entity;
				this.Sprite = sprite;
				if (color != null)
				{
					this.Color = color.Value;
					return;
				}
				this.Color = Color.White;
			}

			// Token: 0x04003733 RID: 14131
			public Entity Entity;

			// Token: 0x04003734 RID: 14132
			public Sprite Sprite;

			// Token: 0x04003735 RID: 14133
			public Color Color;
		}

		// Token: 0x02000688 RID: 1672
		private sealed class SpeechBubble
		{
			// Token: 0x17001991 RID: 6545
			// (get) Token: 0x060065FC RID: 26108 RVA: 0x00346954 File Offset: 0x00344B54
			// (set) Token: 0x060065FD RID: 26109 RVA: 0x0034695C File Offset: 0x00344B5C
			public ImmutableArray<RichTextData>? RichTextData { get; private set; }

			// Token: 0x060065FE RID: 26110 RVA: 0x00346968 File Offset: 0x00344B68
			public SpeechBubble(Character character, float lifeTime, Color color, string text = "")
			{
				RichString richStr = RichString.Rich(text, null);
				this.Text = ToolBox.WrapText(richStr.SanitizedValue, (float)GUI.IntScale(300f), GUIStyle.SmallFont.GetFontForStr(text), 1f);
				this.TextSize = GUIStyle.SmallFont.MeasureString(this.Text, false);
				this.RichTextData = richStr.RichTextData;
				this.Character = character;
				this.Position = this.GetDesiredPosition();
				this.Submarine = character.Submarine;
				this.LifeTime = lifeTime;
				this.Color = color;
			}

			// Token: 0x060065FF RID: 26111 RVA: 0x00346A10 File Offset: 0x00344C10
			public Vector2 GetDesiredPosition()
			{
				return this.Character.Position + Vector2.UnitY * 100f;
			}

			// Token: 0x04003736 RID: 14134
			public float LifeTime;

			// Token: 0x04003737 RID: 14135
			public Vector2 PrevPosition;

			// Token: 0x04003738 RID: 14136
			public Vector2 Position;

			// Token: 0x04003739 RID: 14137
			public Vector2 DrawPosition;

			// Token: 0x0400373A RID: 14138
			public float MoveUpAmount;

			// Token: 0x0400373B RID: 14139
			public readonly RichString Text;

			// Token: 0x0400373D RID: 14141
			public readonly Character Character;

			// Token: 0x0400373E RID: 14142
			public readonly Submarine Submarine;

			// Token: 0x0400373F RID: 14143
			public readonly Vector2 TextSize;

			// Token: 0x04003740 RID: 14144
			public Color Color;

			// Token: 0x04003741 RID: 14145
			public bool Moving;
		}

		// Token: 0x02000689 RID: 1673
		public class Attacker
		{
			// Token: 0x04003742 RID: 14146
			public Character Character;

			// Token: 0x04003743 RID: 14147
			public float Damage;
		}

		// Token: 0x0200068A RID: 1674
		// (Invoke) Token: 0x06006602 RID: 26114
		public delegate void OnDeathHandler(Character character, CauseOfDeath causeOfDeath);

		// Token: 0x0200068B RID: 1675
		// (Invoke) Token: 0x06006606 RID: 26118
		public delegate void OnAttackedHandler(Character attacker, AttackResult attackResult);

		// Token: 0x0200068C RID: 1676
		private struct AttackTargetData
		{
			// Token: 0x17001992 RID: 6546
			// (get) Token: 0x06006609 RID: 26121 RVA: 0x00346A39 File Offset: 0x00344C39
			// (set) Token: 0x0600660A RID: 26122 RVA: 0x00346A41 File Offset: 0x00344C41
			public Limb AttackLimb { readonly get; set; }

			// Token: 0x17001993 RID: 6547
			// (get) Token: 0x0600660B RID: 26123 RVA: 0x00346A4A File Offset: 0x00344C4A
			// (set) Token: 0x0600660C RID: 26124 RVA: 0x00346A52 File Offset: 0x00344C52
			public IDamageable DamageTarget { readonly get; set; }

			// Token: 0x17001994 RID: 6548
			// (get) Token: 0x0600660D RID: 26125 RVA: 0x00346A5B File Offset: 0x00344C5B
			// (set) Token: 0x0600660E RID: 26126 RVA: 0x00346A63 File Offset: 0x00344C63
			public Vector2 AttackPos { readonly get; set; }
		}

		// Token: 0x0200068D RID: 1677
		public enum EventType
		{
			// Token: 0x04003748 RID: 14152
			InventoryState,
			// Token: 0x04003749 RID: 14153
			Control,
			// Token: 0x0400374A RID: 14154
			Status,
			// Token: 0x0400374B RID: 14155
			Treatment,
			// Token: 0x0400374C RID: 14156
			SetAttackTarget,
			// Token: 0x0400374D RID: 14157
			ExecuteAttack,
			// Token: 0x0400374E RID: 14158
			AssignCampaignInteraction,
			// Token: 0x0400374F RID: 14159
			ObjectiveManagerState,
			// Token: 0x04003750 RID: 14160
			TeamChange,
			// Token: 0x04003751 RID: 14161
			AddToCrew,
			// Token: 0x04003752 RID: 14162
			UpdateExperience,
			// Token: 0x04003753 RID: 14163
			UpdateTalents,
			// Token: 0x04003754 RID: 14164
			UpdateSkills,
			// Token: 0x04003755 RID: 14165
			UpdateMoney,
			// Token: 0x04003756 RID: 14166
			UpdatePermanentStats,
			// Token: 0x04003757 RID: 14167
			RemoveFromCrew,
			// Token: 0x04003758 RID: 14168
			LatchOntoTarget,
			// Token: 0x04003759 RID: 14169
			UpdateTalentRefundPoints,
			// Token: 0x0400375A RID: 14170
			ConfirmTalentRefund,
			// Token: 0x0400375B RID: 14171
			MinValue = 0,
			// Token: 0x0400375C RID: 14172
			MaxValue = 18
		}

		// Token: 0x0200068E RID: 1678
		private interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x17001995 RID: 6549
			// (get) Token: 0x0600660F RID: 26127
			Character.EventType EventType { get; }
		}

		// Token: 0x0200068F RID: 1679
		public readonly struct InventoryStateEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001996 RID: 6550
			// (get) Token: 0x06006610 RID: 26128 RVA: 0x00346A6C File Offset: 0x00344C6C
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.InventoryState;
				}
			}

			// Token: 0x06006611 RID: 26129 RVA: 0x00346A6F File Offset: 0x00344C6F
			public InventoryStateEventData(Range slotRange)
			{
				this.SlotRange = slotRange;
			}

			// Token: 0x0400375D RID: 14173
			public readonly Range SlotRange;
		}

		// Token: 0x02000690 RID: 1680
		public readonly struct ControlEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001997 RID: 6551
			// (get) Token: 0x06006612 RID: 26130 RVA: 0x00346A78 File Offset: 0x00344C78
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.Control;
				}
			}

			// Token: 0x06006613 RID: 26131 RVA: 0x00346A7B File Offset: 0x00344C7B
			public ControlEventData(Client owner)
			{
				this.Owner = owner;
			}

			// Token: 0x0400375E RID: 14174
			public readonly Client Owner;
		}

		// Token: 0x02000691 RID: 1681
		public struct CharacterStatusEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001998 RID: 6552
			// (get) Token: 0x06006614 RID: 26132 RVA: 0x00346A84 File Offset: 0x00344C84
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.Status;
				}
			}
		}

		// Token: 0x02000692 RID: 1682
		public struct TreatmentEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001999 RID: 6553
			// (get) Token: 0x06006615 RID: 26133 RVA: 0x00346A87 File Offset: 0x00344C87
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.Treatment;
				}
			}
		}

		// Token: 0x02000693 RID: 1683
		private interface IAttackEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700199A RID: 6554
			// (get) Token: 0x06006616 RID: 26134
			Limb AttackLimb { get; }

			// Token: 0x1700199B RID: 6555
			// (get) Token: 0x06006617 RID: 26135
			IDamageable TargetEntity { get; }

			// Token: 0x1700199C RID: 6556
			// (get) Token: 0x06006618 RID: 26136
			Limb TargetLimb { get; }

			// Token: 0x1700199D RID: 6557
			// (get) Token: 0x06006619 RID: 26137
			Vector2 TargetSimPos { get; }
		}

		// Token: 0x02000694 RID: 1684
		public struct SetAttackTargetEventData : Character.IAttackEventData, Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700199E RID: 6558
			// (get) Token: 0x0600661A RID: 26138 RVA: 0x00346A8A File Offset: 0x00344C8A
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.SetAttackTarget;
				}
			}

			// Token: 0x1700199F RID: 6559
			// (get) Token: 0x0600661B RID: 26139 RVA: 0x00346A8D File Offset: 0x00344C8D
			public readonly Limb AttackLimb { get; }

			// Token: 0x170019A0 RID: 6560
			// (get) Token: 0x0600661C RID: 26140 RVA: 0x00346A95 File Offset: 0x00344C95
			public readonly IDamageable TargetEntity { get; }

			// Token: 0x170019A1 RID: 6561
			// (get) Token: 0x0600661D RID: 26141 RVA: 0x00346A9D File Offset: 0x00344C9D
			public readonly Limb TargetLimb { get; }

			// Token: 0x170019A2 RID: 6562
			// (get) Token: 0x0600661E RID: 26142 RVA: 0x00346AA5 File Offset: 0x00344CA5
			public readonly Vector2 TargetSimPos { get; }

			// Token: 0x0600661F RID: 26143 RVA: 0x00346AAD File Offset: 0x00344CAD
			public SetAttackTargetEventData(Limb attackLimb, IDamageable targetEntity, Limb targetLimb, Vector2 targetSimPos)
			{
				this.AttackLimb = attackLimb;
				this.TargetEntity = targetEntity;
				this.TargetLimb = targetLimb;
				this.TargetSimPos = targetSimPos;
			}
		}

		// Token: 0x02000695 RID: 1685
		public struct ExecuteAttackEventData : Character.IAttackEventData, Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019A3 RID: 6563
			// (get) Token: 0x06006620 RID: 26144 RVA: 0x00346ACC File Offset: 0x00344CCC
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.ExecuteAttack;
				}
			}

			// Token: 0x170019A4 RID: 6564
			// (get) Token: 0x06006621 RID: 26145 RVA: 0x00346ACF File Offset: 0x00344CCF
			public readonly Limb AttackLimb { get; }

			// Token: 0x170019A5 RID: 6565
			// (get) Token: 0x06006622 RID: 26146 RVA: 0x00346AD7 File Offset: 0x00344CD7
			public readonly IDamageable TargetEntity { get; }

			// Token: 0x170019A6 RID: 6566
			// (get) Token: 0x06006623 RID: 26147 RVA: 0x00346ADF File Offset: 0x00344CDF
			public readonly Limb TargetLimb { get; }

			// Token: 0x170019A7 RID: 6567
			// (get) Token: 0x06006624 RID: 26148 RVA: 0x00346AE7 File Offset: 0x00344CE7
			public readonly Vector2 TargetSimPos { get; }

			// Token: 0x06006625 RID: 26149 RVA: 0x00346AEF File Offset: 0x00344CEF
			public ExecuteAttackEventData(Limb attackLimb, IDamageable targetEntity, Limb targetLimb, Vector2 targetSimPos)
			{
				this.AttackLimb = attackLimb;
				this.TargetEntity = targetEntity;
				this.TargetLimb = targetLimb;
				this.TargetSimPos = targetSimPos;
			}
		}

		// Token: 0x02000696 RID: 1686
		public struct AssignCampaignInteractionEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019A8 RID: 6568
			// (get) Token: 0x06006626 RID: 26150 RVA: 0x00346B0E File Offset: 0x00344D0E
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.AssignCampaignInteraction;
				}
			}
		}

		// Token: 0x02000697 RID: 1687
		public struct ObjectiveManagerStateEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019A9 RID: 6569
			// (get) Token: 0x06006627 RID: 26151 RVA: 0x00346B11 File Offset: 0x00344D11
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.ObjectiveManagerState;
				}
			}

			// Token: 0x06006628 RID: 26152 RVA: 0x00346B14 File Offset: 0x00344D14
			public ObjectiveManagerStateEventData(AIObjectiveManager.ObjectiveType objectiveType)
			{
				this.ObjectiveType = objectiveType;
			}

			// Token: 0x04003767 RID: 14183
			public readonly AIObjectiveManager.ObjectiveType ObjectiveType;
		}

		// Token: 0x02000698 RID: 1688
		public readonly struct LatchedOntoTargetEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019AA RID: 6570
			// (get) Token: 0x06006629 RID: 26153 RVA: 0x00346B1D File Offset: 0x00344D1D
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.LatchOntoTarget;
				}
			}

			// Token: 0x0600662A RID: 26154 RVA: 0x00346B24 File Offset: 0x00344D24
			private LatchedOntoTargetEventData(Character character, Vector2 attachSurfaceNormal, Vector2 attachPos)
			{
				this.TargetCharacterID = 0;
				this.TargetStructureID = 0;
				this.TargetLevelWallIndex = -1;
				this.AttachSurfaceNormal = Vector2.Zero;
				this.AttachPos = Vector2.Zero;
				this.CharacterSimPos = character.SimPosition;
				this.IsLatched = true;
				this.AttachSurfaceNormal = attachSurfaceNormal;
				this.AttachPos = attachPos;
			}

			// Token: 0x0600662B RID: 26155 RVA: 0x00346B7D File Offset: 0x00344D7D
			public LatchedOntoTargetEventData(Character character, Character targetCharacter, Vector2 attachSurfaceNormal, Vector2 attachPos)
			{
				this = new Character.LatchedOntoTargetEventData(character, attachSurfaceNormal, attachPos);
				this.TargetCharacterID = targetCharacter.ID;
			}

			// Token: 0x0600662C RID: 26156 RVA: 0x00346B95 File Offset: 0x00344D95
			public LatchedOntoTargetEventData(Character character, Structure targetStructure, Vector2 attachSurfaceNormal, Vector2 attachPos)
			{
				this = new Character.LatchedOntoTargetEventData(character, attachSurfaceNormal, attachPos);
				this.TargetStructureID = targetStructure.ID;
			}

			// Token: 0x0600662D RID: 26157 RVA: 0x00346BAD File Offset: 0x00344DAD
			public LatchedOntoTargetEventData(Character character, VoronoiCell levelWall, Vector2 attachSurfaceNormal, Vector2 attachPos)
			{
				this = new Character.LatchedOntoTargetEventData(character, attachSurfaceNormal, attachPos);
				this.TargetLevelWallIndex = Level.Loaded.GetAllCells().IndexOf(levelWall);
			}

			// Token: 0x0600662E RID: 26158 RVA: 0x00346BCF File Offset: 0x00344DCF
			public LatchedOntoTargetEventData()
			{
				this.TargetCharacterID = 0;
				this.TargetStructureID = 0;
				this.TargetLevelWallIndex = -1;
				this.AttachSurfaceNormal = Vector2.Zero;
				this.AttachPos = Vector2.Zero;
				this.CharacterSimPos = Vector2.Zero;
				this.IsLatched = false;
			}

			// Token: 0x04003768 RID: 14184
			public readonly bool IsLatched;

			// Token: 0x04003769 RID: 14185
			public readonly ushort TargetCharacterID;

			// Token: 0x0400376A RID: 14186
			public readonly ushort TargetStructureID;

			// Token: 0x0400376B RID: 14187
			public readonly int TargetLevelWallIndex;

			// Token: 0x0400376C RID: 14188
			public readonly Vector2 AttachSurfaceNormal;

			// Token: 0x0400376D RID: 14189
			public readonly Vector2 AttachPos;

			// Token: 0x0400376E RID: 14190
			public readonly Vector2 CharacterSimPos;
		}

		// Token: 0x02000699 RID: 1689
		private struct TeamChangeEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019AB RID: 6571
			// (get) Token: 0x0600662F RID: 26159 RVA: 0x00346C0E File Offset: 0x00344E0E
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.TeamChange;
				}
			}
		}

		// Token: 0x0200069A RID: 1690
		[NetworkSerialize(197)]
		public readonly struct ItemTeamChange : INetSerializableStruct, IEquatable<Character.ItemTeamChange>
		{
			// Token: 0x06006630 RID: 26160 RVA: 0x00346C11 File Offset: 0x00344E11
			public ItemTeamChange(CharacterTeamType TeamId, ImmutableArray<ushort> ItemIds)
			{
				this.TeamId = TeamId;
				this.ItemIds = ItemIds;
			}

			// Token: 0x170019AC RID: 6572
			// (get) Token: 0x06006631 RID: 26161 RVA: 0x00346C21 File Offset: 0x00344E21
			// (set) Token: 0x06006632 RID: 26162 RVA: 0x00346C29 File Offset: 0x00344E29
			public CharacterTeamType TeamId { get; set; }

			// Token: 0x170019AD RID: 6573
			// (get) Token: 0x06006633 RID: 26163 RVA: 0x00346C32 File Offset: 0x00344E32
			// (set) Token: 0x06006634 RID: 26164 RVA: 0x00346C3A File Offset: 0x00344E3A
			public ImmutableArray<ushort> ItemIds { get; set; }

			// Token: 0x06006635 RID: 26165 RVA: 0x00346C44 File Offset: 0x00344E44
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ItemTeamChange");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006636 RID: 26166 RVA: 0x00346C90 File Offset: 0x00344E90
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("TeamId = ");
				builder.Append(this.TeamId.ToString());
				builder.Append(", ItemIds = ");
				builder.Append(this.ItemIds.ToString());
				return true;
			}

			// Token: 0x06006637 RID: 26167 RVA: 0x00346CEC File Offset: 0x00344EEC
			[CompilerGenerated]
			public static bool operator !=(Character.ItemTeamChange left, Character.ItemTeamChange right)
			{
				return !(left == right);
			}

			// Token: 0x06006638 RID: 26168 RVA: 0x00346CF8 File Offset: 0x00344EF8
			[CompilerGenerated]
			public static bool operator ==(Character.ItemTeamChange left, Character.ItemTeamChange right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006639 RID: 26169 RVA: 0x00346D02 File Offset: 0x00344F02
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<CharacterTeamType>.Default.GetHashCode(this.<TeamId>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<ItemIds>k__BackingField);
			}

			// Token: 0x0600663A RID: 26170 RVA: 0x00346D2B File Offset: 0x00344F2B
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Character.ItemTeamChange && this.Equals((Character.ItemTeamChange)obj);
			}

			// Token: 0x0600663B RID: 26171 RVA: 0x00346D43 File Offset: 0x00344F43
			[CompilerGenerated]
			public bool Equals(Character.ItemTeamChange other)
			{
				return EqualityComparer<CharacterTeamType>.Default.Equals(this.<TeamId>k__BackingField, other.<TeamId>k__BackingField) && EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<ItemIds>k__BackingField, other.<ItemIds>k__BackingField);
			}

			// Token: 0x0600663C RID: 26172 RVA: 0x00346D75 File Offset: 0x00344F75
			[CompilerGenerated]
			public void Deconstruct(out CharacterTeamType TeamId, out ImmutableArray<ushort> ItemIds)
			{
				TeamId = this.TeamId;
				ItemIds = this.ItemIds;
			}
		}

		// Token: 0x0200069B RID: 1691
		public struct AddToCrewEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019AE RID: 6574
			// (get) Token: 0x0600663D RID: 26173 RVA: 0x00346D8B File Offset: 0x00344F8B
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.AddToCrew;
				}
			}

			// Token: 0x0600663E RID: 26174 RVA: 0x00346D8F File Offset: 0x00344F8F
			public AddToCrewEventData(CharacterTeamType teamType, IEnumerable<Item> inventoryItems)
			{
				this.ItemTeamChange = new Character.ItemTeamChange(teamType, (from it in inventoryItems
				select it.ID).ToImmutableArray<ushort>());
			}

			// Token: 0x04003771 RID: 14193
			public readonly Character.ItemTeamChange ItemTeamChange;
		}

		// Token: 0x0200069C RID: 1692
		public struct RemoveFromCrewEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019AF RID: 6575
			// (get) Token: 0x0600663F RID: 26175 RVA: 0x00346DC7 File Offset: 0x00344FC7
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.RemoveFromCrew;
				}
			}

			// Token: 0x06006640 RID: 26176 RVA: 0x00346DCB File Offset: 0x00344FCB
			public RemoveFromCrewEventData(CharacterTeamType teamType, IEnumerable<Item> inventoryItems)
			{
				this.ItemTeamChange = new Character.ItemTeamChange(teamType, (from it in inventoryItems
				select it.ID).ToImmutableArray<ushort>());
			}

			// Token: 0x04003772 RID: 14194
			public readonly Character.ItemTeamChange ItemTeamChange;
		}

		// Token: 0x0200069D RID: 1693
		public struct UpdateExperienceEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019B0 RID: 6576
			// (get) Token: 0x06006641 RID: 26177 RVA: 0x00346E03 File Offset: 0x00345003
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateExperience;
				}
			}
		}

		// Token: 0x0200069E RID: 1694
		public struct UpdateTalentsEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019B1 RID: 6577
			// (get) Token: 0x06006642 RID: 26178 RVA: 0x00346E07 File Offset: 0x00345007
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateTalents;
				}
			}
		}

		// Token: 0x0200069F RID: 1695
		public struct UpdateSkillsEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019B2 RID: 6578
			// (get) Token: 0x06006643 RID: 26179 RVA: 0x00346E0B File Offset: 0x0034500B
			public readonly Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateSkills;
				}
			}

			// Token: 0x06006644 RID: 26180 RVA: 0x00346E0F File Offset: 0x0034500F
			public UpdateSkillsEventData(Identifier skillIdentifier, bool forceNotification)
			{
				this.SkillIdentifier = skillIdentifier;
				this.ForceNotification = forceNotification;
			}

			// Token: 0x04003773 RID: 14195
			public readonly bool ForceNotification;

			// Token: 0x04003774 RID: 14196
			public readonly Identifier SkillIdentifier;
		}

		// Token: 0x020006A0 RID: 1696
		private struct UpdateMoneyEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019B3 RID: 6579
			// (get) Token: 0x06006645 RID: 26181 RVA: 0x00346E1F File Offset: 0x0034501F
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateMoney;
				}
			}
		}

		// Token: 0x020006A1 RID: 1697
		public struct UpdatePermanentStatsEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019B4 RID: 6580
			// (get) Token: 0x06006646 RID: 26182 RVA: 0x00346E23 File Offset: 0x00345023
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdatePermanentStats;
				}
			}

			// Token: 0x06006647 RID: 26183 RVA: 0x00346E27 File Offset: 0x00345027
			public UpdatePermanentStatsEventData(StatTypes statType)
			{
				this.StatType = statType;
			}

			// Token: 0x04003775 RID: 14197
			public readonly StatTypes StatType;
		}

		// Token: 0x020006A2 RID: 1698
		public struct UpdateRefundPointsEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019B5 RID: 6581
			// (get) Token: 0x06006648 RID: 26184 RVA: 0x00346E30 File Offset: 0x00345030
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateTalentRefundPoints;
				}
			}
		}

		// Token: 0x020006A3 RID: 1699
		public struct ConfirmRefundEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170019B6 RID: 6582
			// (get) Token: 0x06006649 RID: 26185 RVA: 0x00346E34 File Offset: 0x00345034
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.ConfirmTalentRefund;
				}
			}
		}

		// Token: 0x020006A4 RID: 1700
		[Flags]
		private enum InputNetFlags : ushort
		{
			// Token: 0x04003777 RID: 14199
			None = 0,
			// Token: 0x04003778 RID: 14200
			Left = 1,
			// Token: 0x04003779 RID: 14201
			Right = 2,
			// Token: 0x0400377A RID: 14202
			Up = 4,
			// Token: 0x0400377B RID: 14203
			Down = 8,
			// Token: 0x0400377C RID: 14204
			FacingLeft = 16,
			// Token: 0x0400377D RID: 14205
			Run = 32,
			// Token: 0x0400377E RID: 14206
			Crouch = 64,
			// Token: 0x0400377F RID: 14207
			Select = 128,
			// Token: 0x04003780 RID: 14208
			Use = 256,
			// Token: 0x04003781 RID: 14209
			Aim = 512,
			// Token: 0x04003782 RID: 14210
			Attack = 1024,
			// Token: 0x04003783 RID: 14211
			Ragdoll = 2048,
			// Token: 0x04003784 RID: 14212
			Health = 4096,
			// Token: 0x04003785 RID: 14213
			Grab = 8192,
			// Token: 0x04003786 RID: 14214
			Deselect = 16384,
			// Token: 0x04003787 RID: 14215
			Shoot = 32768,
			// Token: 0x04003788 RID: 14216
			MaxVal = 65535
		}

		// Token: 0x020006A5 RID: 1701
		private struct NetInputMem
		{
			// Token: 0x04003789 RID: 14217
			public Character.InputNetFlags states;

			// Token: 0x0400378A RID: 14218
			public ushort intAim;

			// Token: 0x0400378B RID: 14219
			public ushort interact;

			// Token: 0x0400378C RID: 14220
			public ushort networkUpdateID;
		}
	}
}
