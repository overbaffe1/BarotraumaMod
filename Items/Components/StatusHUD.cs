using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005E1 RID: 1505
	internal class StatusHUD : ItemComponent
	{
		// Token: 0x170018A7 RID: 6311
		// (get) Token: 0x060061CD RID: 25037 RVA: 0x0032E4AF File Offset: 0x0032C6AF
		// (set) Token: 0x060061CE RID: 25038 RVA: 0x0032E4B7 File Offset: 0x0032C6B7
		[Serialize(500f, IsPropertySaveable.No, "How close to a target the user must be to see their health data (in pixels).", "", false)]
		public float Range { get; private set; }

		// Token: 0x170018A8 RID: 6312
		// (get) Token: 0x060061CF RID: 25039 RVA: 0x0032E4C0 File Offset: 0x0032C6C0
		// (set) Token: 0x060061D0 RID: 25040 RVA: 0x0032E4C8 File Offset: 0x0032C6C8
		[Serialize(50f, IsPropertySaveable.No, "The range within which the health info texts fades out.", "", false)]
		public float FadeOutRange { get; private set; }

		// Token: 0x170018A9 RID: 6313
		// (get) Token: 0x060061D1 RID: 25041 RVA: 0x0032E4D1 File Offset: 0x0032C6D1
		// (set) Token: 0x060061D2 RID: 25042 RVA: 0x0032E4D9 File Offset: 0x0032C6D9
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool ThermalGoggles { get; private set; }

		// Token: 0x170018AA RID: 6314
		// (get) Token: 0x060061D3 RID: 25043 RVA: 0x0032E4E2 File Offset: 0x0032C6E2
		// (set) Token: 0x060061D4 RID: 25044 RVA: 0x0032E4EA File Offset: 0x0032C6EA
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DebugWiring { get; private set; }

		// Token: 0x170018AB RID: 6315
		// (get) Token: 0x060061D5 RID: 25045 RVA: 0x0032E4F3 File Offset: 0x0032C6F3
		// (set) Token: 0x060061D6 RID: 25046 RVA: 0x0032E4FB File Offset: 0x0032C6FB
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowDeadCharacters { get; private set; }

		// Token: 0x170018AC RID: 6316
		// (get) Token: 0x060061D7 RID: 25047 RVA: 0x0032E504 File Offset: 0x0032C704
		// (set) Token: 0x060061D8 RID: 25048 RVA: 0x0032E50C File Offset: 0x0032C70C
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowTexts { get; private set; }

		// Token: 0x170018AD RID: 6317
		// (get) Token: 0x060061D9 RID: 25049 RVA: 0x0032E515 File Offset: 0x0032C715
		// (set) Token: 0x060061DA RID: 25050 RVA: 0x0032E51D File Offset: 0x0032C71D
		[Serialize("72,119,72,120", IsPropertySaveable.No, "", "", false)]
		public Color OverlayColor { get; private set; }

		// Token: 0x170018AE RID: 6318
		// (get) Token: 0x060061DB RID: 25051 RVA: 0x0032E526 File Offset: 0x0032C726
		public IEnumerable<Character> VisibleCharacters
		{
			get
			{
				if (this.equipper == null || this.equipper.Removed)
				{
					return Enumerable.Empty<Character>();
				}
				return this.visibleCharacters;
			}
		}

		// Token: 0x060061DC RID: 25052 RVA: 0x0032E549 File Offset: 0x0032C749
		public override void OnItemLoaded()
		{
			this.isEquippable = (this.item.GetComponent<Pickable>() != null);
			if (!this.isEquippable)
			{
				this.IsActive = true;
			}
		}

		// Token: 0x060061DD RID: 25053 RVA: 0x0032E570 File Offset: 0x0032C770
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			Entity refEntity = this.equipper;
			if (this.isEquippable)
			{
				if (this.equipper == null || this.equipper.Removed)
				{
					this.IsActive = false;
					return;
				}
			}
			else
			{
				refEntity = this.item;
			}
			if (this.equipper != null && this.equipper == Character.Controlled && this.DebugWiring)
			{
				ConnectionPanel.DebugWiringEnabledUntil = Timing.TotalTimeUnpaused + 0.5;
			}
			this.thermalEffectState += deltaTime;
			this.thermalEffectState %= 10000f;
			if (this.updateTimer > 0f)
			{
				this.updateTimer -= deltaTime;
				return;
			}
			this.visibleCharacters.Clear();
			foreach (Character c in Character.CharacterList)
			{
				if (c != this.equipper && c.Enabled && !c.Removed && (this.ShowDeadCharacters || !c.IsDead) && !c.InDetectable)
				{
					float dist = Vector2.DistanceSquared(refEntity.WorldPosition, c.WorldPosition);
					if (dist < this.Range * this.Range)
					{
						Vector2 diff = c.WorldPosition - refEntity.WorldPosition;
						if (Submarine.CheckVisibility(refEntity.SimPosition, refEntity.SimPosition + ConvertUnits.ToSimUnits(diff), false, false, true, true, true, null) == null)
						{
							this.visibleCharacters.Add(c);
						}
					}
				}
			}
			this.updateTimer = 0.5f;
		}

		// Token: 0x060061DE RID: 25054 RVA: 0x0032E71C File Offset: 0x0032C91C
		public override void Equip(Character character)
		{
			this.updateTimer = 0f;
			this.equipper = character;
			this.IsActive = true;
		}

		// Token: 0x060061DF RID: 25055 RVA: 0x0032E737 File Offset: 0x0032C937
		public override void Unequip(Character character)
		{
			this.equipper = null;
			this.IsActive = false;
		}

		// Token: 0x060061E0 RID: 25056 RVA: 0x0032E747 File Offset: 0x0032C947
		public override void Drop(Character dropper, bool setTransform = true)
		{
			this.Unequip(dropper);
		}

		// Token: 0x060061E1 RID: 25057 RVA: 0x0032E750 File Offset: 0x0032C950
		public override void DrawHUD(SpriteBatch spriteBatch, Character character)
		{
			if (character == null)
			{
				return;
			}
			base.DrawHUD(spriteBatch, character);
			if (this.OverlayColor.A > 0)
			{
				GUIStyle.UIGlow.Draw(spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), this.OverlayColor, SpriteEffects.None);
			}
			if (this.ShowTexts)
			{
				Character closestCharacter = null;
				float closestDist = float.PositiveInfinity;
				foreach (Character c in this.visibleCharacters)
				{
					if (c != character && c.Enabled && !c.Removed)
					{
						float dist = Vector2.DistanceSquared(GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition), c.WorldPosition);
						if (dist < closestDist)
						{
							closestCharacter = c;
							closestDist = dist;
						}
					}
				}
				if (closestCharacter != null)
				{
					float dist2 = Vector2.Distance(GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition), closestCharacter.WorldPosition);
					this.DrawCharacterInfo(spriteBatch, closestCharacter, 1f - MathHelper.Max((dist2 - (this.Range - this.FadeOutRange)) / this.FadeOutRange, 0f));
				}
			}
			if (this.ThermalGoggles)
			{
				Entity refEntity = this.equipper;
				if (!this.isEquippable || refEntity == null)
				{
					refEntity = this.item;
				}
				StatusHUD.DrawThermalOverlay(spriteBatch, refEntity, character, this.OverlayColor, this.Range, this.thermalEffectState, this.ShowDeadCharacters);
			}
		}

		// Token: 0x060061E2 RID: 25058 RVA: 0x0032E8CC File Offset: 0x0032CACC
		public static void DrawThermalOverlay(SpriteBatch spriteBatch, Entity refEntity, Character user, Color overlayColor, float range, float effectState, bool showDeadCharacters)
		{
			spriteBatch.End();
			float colorIntensityBase = 0.5f;
			float colorIntensityVariance = 0.05f;
			GameMain.LightManager.SolidColorEffect.Parameters["color"].SetValue(overlayColor.ToVector4() * (colorIntensityBase + MathF.Sin(effectState) * colorIntensityVariance));
			GameMain.LightManager.SolidColorEffect.CurrentTechnique = GameMain.LightManager.SolidColorEffect.Techniques["SolidColorBlur"];
			GameMain.LightManager.SolidColorEffect.Parameters["blurDistance"].SetValue(0.01f + MathF.Sin(effectState) * 0.005f);
			GameMain.LightManager.SolidColorEffect.CurrentTechnique.Passes[0].Apply();
			SpriteSortMode sortMode = SpriteSortMode.Deferred;
			BlendState additive = BlendState.Additive;
			SamplerState samplerState = null;
			DepthStencilState depthStencilState = null;
			RasterizerState rasterizerState = null;
			Matrix? transformMatrix = new Matrix?(Screen.Selected.Cam.Transform);
			spriteBatch.Begin(sortMode, additive, samplerState, depthStencilState, rasterizerState, GameMain.LightManager.SolidColorEffect, transformMatrix);
			foreach (Character c in Character.CharacterList)
			{
				if (c != user && c.Enabled && !c.Removed && !c.Params.HideInThermalGoggles && (showDeadCharacters || !c.IsDead))
				{
					float dist = Vector2.DistanceSquared(refEntity.WorldPosition, c.WorldPosition);
					if (dist <= range * range)
					{
						Sprite pingCircle = GUIStyle.UIThermalGlow.Value.Sprite;
						foreach (Limb limb in c.AnimController.Limbs)
						{
							if (limb.Mass >= 0.5f || limb == c.AnimController.MainLimb)
							{
								float noise = PerlinNoise.GetPerlin((effectState + (float)limb.Params.ID + (float)c.ID) * 0.01f, (effectState + (float)limb.Params.ID + (float)c.ID) * 0.02f);
								float noise2 = PerlinNoise.GetPerlin((effectState + (float)limb.Params.ID + (float)c.ID) * 0.01f, (effectState + (float)limb.Params.ID + (float)c.ID) * 0.008f);
								Vector2 spriteScale = ConvertUnits.ToDisplayUnits(limb.body.GetSize()) / pingCircle.size * (noise * 0.5f + 2f);
								Vector2 drawPos = new Vector2(limb.body.DrawPosition.X + (noise - 0.5f) * 100f, -limb.body.DrawPosition.Y + (noise2 - 0.5f) * 100f);
								pingCircle.Draw(spriteBatch, drawPos, 0f, Math.Max(spriteScale.X, spriteScale.Y), SpriteEffects.None);
							}
						}
					}
				}
			}
			spriteBatch.End();
			SpriteSortMode sortMode2 = SpriteSortMode.Deferred;
			BlendState nonPremultiplied = BlendState.NonPremultiplied;
			SamplerState samplerState2 = null;
			DepthStencilState depthStencilState2 = null;
			RasterizerState rasterizerState2 = null;
			Effect effect = null;
			transformMatrix = null;
			spriteBatch.Begin(sortMode2, nonPremultiplied, samplerState2, depthStencilState2, rasterizerState2, effect, transformMatrix);
		}

		// Token: 0x060061E3 RID: 25059 RVA: 0x0032EC20 File Offset: 0x0032CE20
		private void DrawCharacterInfo(SpriteBatch spriteBatch, Character target, float alpha = 1f)
		{
			Vector2 hudPos = GameMain.GameScreen.Cam.WorldToScreen(target.DrawPosition);
			hudPos += Vector2.UnitX * 50f;
			List<LocalizedString> texts = new List<LocalizedString>();
			List<Color> textColors = new List<Color>();
			texts.Add((target.Info == null) ? target.DisplayName : target.Info.DisplayName);
			Color nameColor = GUIStyle.TextColorNormal;
			if (Character.Controlled != null && target.TeamID != Character.Controlled.TeamID)
			{
				nameColor = ((target.TeamID == CharacterTeamType.FriendlyNPC) ? Color.SkyBlue : GUIStyle.Red);
			}
			textColors.Add(nameColor);
			if (target.IsDead)
			{
				texts.Add(TextManager.Get("Deceased"));
				textColors.Add(GUIStyle.Red);
				if (target.CauseOfDeath != null)
				{
					List<LocalizedString> list = texts;
					AfflictionPrefab affliction3 = target.CauseOfDeath.Affliction;
					LocalizedString item;
					if ((item = ((affliction3 != null) ? affliction3.CauseOfDeathDescription : null)) == null)
					{
						item = TextManager.AddPunctuation(':', new LocalizedString[]
						{
							TextManager.Get("CauseOfDeath"),
							TextManager.Get("CauseOfDeath." + target.CauseOfDeath.Type.ToString())
						});
					}
					list.Add(item);
					textColors.Add(GUIStyle.Red);
				}
			}
			else
			{
				if (target.ShouldShowCustomInteractText)
				{
					texts.Add(target.CustomInteractHUDText);
					textColors.Add(GUIStyle.Green);
				}
				Character character = this.equipper;
				if (((character != null) ? character.FocusedCharacter : null) == target)
				{
					if (!target.IsIncapacitated && target.IsPet)
					{
						EnemyAIController enemyAI = target.AIController as EnemyAIController;
						if (enemyAI != null && enemyAI.PetBehavior.CanPlayWith(Character.Controlled))
						{
							texts.Add(CharacterHUD.GetCachedHudText("PlayHint", InputType.Use));
							textColors.Add(GUIStyle.Green);
						}
					}
					if (target.CanBeHealedBy(this.equipper, false))
					{
						texts.Add(CharacterHUD.GetCachedHudText("HealHint", InputType.Health));
						textColors.Add(GUIStyle.Green);
					}
					if (target.CanBeDraggedBy(Character.Controlled))
					{
						texts.Add(CharacterHUD.GetCachedHudText("GrabHint", InputType.Grab));
						textColors.Add(GUIStyle.Green);
					}
				}
				if (target.IsUnconscious)
				{
					texts.Add(TextManager.Get("Unconscious"));
					textColors.Add(GUIStyle.Orange);
				}
				if (target.Stun > 0.01f)
				{
					texts.Add(TextManager.Get("Stunned"));
					textColors.Add(GUIStyle.Orange);
				}
				if (target.NeedsOxygen)
				{
					int oxygenTextIndex = MathHelper.Clamp((int)Math.Floor((double)((1f - target.Oxygen / 100f) * (float)StatusHUD.OxygenTexts.Length)), 0, StatusHUD.OxygenTexts.Length - 1);
					texts.Add(StatusHUD.OxygenTexts[oxygenTextIndex]);
					textColors.Add(Color.Lerp(GUIStyle.Red, GUIStyle.Green, target.Oxygen / 100f));
				}
				if (target.Bleeding > 0f)
				{
					int bleedingTextIndex = MathHelper.Clamp((int)Math.Floor((double)(target.Bleeding / 100f * (float)StatusHUD.BleedingTexts.Length)), 0, StatusHUD.BleedingTexts.Length - 1);
					texts.Add(StatusHUD.BleedingTexts[bleedingTextIndex]);
					textColors.Add(Color.Lerp(GUIStyle.Orange, GUIStyle.Red, target.Bleeding / 100f));
				}
				IReadOnlyCollection<Affliction> allAfflictions = target.CharacterHealth.GetAllAfflictions();
				Dictionary<AfflictionPrefab, float> combinedAfflictionStrengths = new Dictionary<AfflictionPrefab, float>();
				foreach (Affliction affliction in allAfflictions)
				{
					if (affliction.Strength > 0f && (affliction.Strength >= affliction.Prefab.ShowInHealthScannerThreshold || (!target.IsHuman && !target.IsOnPlayerTeam && (!(affliction.Prefab.AfflictionType != AfflictionPrefab.PoisonType) || !(affliction.Prefab.AfflictionType != AfflictionPrefab.ParalysisType)))))
					{
						if (combinedAfflictionStrengths.ContainsKey(affliction.Prefab))
						{
							Dictionary<AfflictionPrefab, float> dictionary = combinedAfflictionStrengths;
							AfflictionPrefab prefab = affliction.Prefab;
							dictionary[prefab] += affliction.Strength;
						}
						else
						{
							combinedAfflictionStrengths[affliction.Prefab] = affliction.Strength;
						}
					}
				}
				foreach (AfflictionPrefab affliction2 in combinedAfflictionStrengths.Keys)
				{
					texts.Add(TextManager.AddPunctuation(':', new LocalizedString[]
					{
						affliction2.Name,
						Math.Max((int)combinedAfflictionStrengths[affliction2], 1).ToString() + " %"
					}));
					textColors.Add(Color.Lerp(GUIStyle.Orange, GUIStyle.Red, combinedAfflictionStrengths[affliction2] / affliction2.MaxStrength));
				}
			}
			GUI.DrawString(spriteBatch, hudPos, texts[0].Value, textColors[0] * alpha, new Color?(Color.Black * 0.7f * alpha), 2, GUIStyle.SubHeadingFont, ForceUpperCase.No);
			hudPos.X += 5f * GUI.Scale;
			hudPos.Y += GUIStyle.SubHeadingFont.MeasureString(texts[0].Value, false).Y;
			hudPos.X = (float)((int)hudPos.X);
			hudPos.Y = (float)((int)hudPos.Y);
			for (int i = 1; i < texts.Count; i++)
			{
				GUI.DrawString(spriteBatch, hudPos, texts[i], textColors[i] * alpha, new Color?(Color.Black * 0.7f * alpha), 2, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				hudPos.Y += (float)((int)GUIStyle.SubHeadingFont.MeasureString(texts[i].Value, false).Y);
			}
		}

		// Token: 0x060061E4 RID: 25060 RVA: 0x0032F29C File Offset: 0x0032D49C
		public StatusHUD(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x04003267 RID: 12903
		private static readonly LocalizedString[] BleedingTexts = new LocalizedString[]
		{
			TextManager.Get("MinorBleeding"),
			TextManager.Get("Bleeding"),
			TextManager.Get("HeavyBleeding"),
			TextManager.Get("CatastrophicBleeding")
		};

		// Token: 0x04003268 RID: 12904
		private static readonly LocalizedString[] OxygenTexts = new LocalizedString[]
		{
			TextManager.Get("OxygenNormal"),
			TextManager.Get("OxygenReduced"),
			TextManager.Get("OxygenLow"),
			TextManager.Get("NotBreathing")
		};

		// Token: 0x04003270 RID: 12912
		private readonly List<Character> visibleCharacters = new List<Character>();

		// Token: 0x04003271 RID: 12913
		private const float UpdateInterval = 0.5f;

		// Token: 0x04003272 RID: 12914
		private float updateTimer;

		// Token: 0x04003273 RID: 12915
		private Character equipper;

		// Token: 0x04003274 RID: 12916
		private bool isEquippable;

		// Token: 0x04003275 RID: 12917
		private float thermalEffectState;
	}
}
