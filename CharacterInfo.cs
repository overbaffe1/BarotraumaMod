using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000029 RID: 41
	internal class CharacterInfo
	{
		// Token: 0x170001CF RID: 463
		// (get) Token: 0x0600064A RID: 1610 RVA: 0x0003765C File Offset: 0x0003585C
		// (set) Token: 0x0600064B RID: 1611 RVA: 0x00037664 File Offset: 0x00035864
		public int CrewListIndex { get; set; } = int.MaxValue;

		// Token: 0x0600064C RID: 1612 RVA: 0x00037670 File Offset: 0x00035870
		public static void Init()
		{
			GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("InfoAreaPortraitBG");
			CharacterInfo.infoAreaPortraitBG = ((componentStyle != null) ? componentStyle.GetDefaultSprite() : null);
			new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(833, 298, 142, 98)), null, 0f);
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x000376CC File Offset: 0x000358CC
		public GUIComponent CreateInfoFrame(GUIFrame frame, bool returnParent, Sprite permissionIcon = null)
		{
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.874f, 0.58f), frame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.05f)
			}, false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			GUILayoutGroup headerArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.4f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUICustomComponent(new RectTransform(new Vector2(0.425f, 1f), headerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				this.DrawInfoFrameCharacterIcon(sb, component.Rect);
			}, null);
			GUIFont font = (paddedFrame.Rect.Width < 280) ? GUIStyle.SmallFont : GUIStyle.Font;
			GUILayoutGroup headerTextArea = new GUILayoutGroup(new RectTransform(new Vector2(0.575f, 1f), headerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			Color? nameColor = null;
			if (this.Job != null)
			{
				nameColor = new Color?(this.Job.Prefab.UIColor);
			}
			GUITextBlock characterNameBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.25f), headerTextArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ToolBox.LimitString(this.Name, GUIStyle.Font, headerTextArea.Rect.Width), nameColor, GUIStyle.Font, Alignment.Left, false, "", null)
			{
				ForceUpperCase = ForceUpperCase.Yes,
				Padding = Vector4.Zero
			};
			if (permissionIcon != null)
			{
				Point iconSize = permissionIcon.SourceRect.Size;
				int iconWidth = (int)((float)characterNameBlock.Rect.Height / (float)iconSize.Y * (float)iconSize.X);
				new GUIImage(new RectTransform(new Point(iconWidth, characterNameBlock.Rect.Height), characterNameBlock.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
				{
					AbsoluteOffset = new Point(-iconWidth - 2, 0)
				}, permissionIcon, null, GUIImage.ScalingMode.None).IgnoreLayoutGroups = true;
			}
			if (this.Job != null)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.25f), headerTextArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.Job.Name, new Color?(this.Job.Prefab.UIColor), font, Alignment.Left, false, "", null).Padding = Vector4.Zero;
			}
			if (this.PersonalityTrait != null)
			{
				RectTransform rectT = new RectTransform(new Vector2(1f, 0.25f), headerTextArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = TextManager.AddPunctuation(':', new LocalizedString[]
				{
					TextManager.Get("PersonalityTrait"),
					this.PersonalityTrait.DisplayName
				});
				GUIFont font2 = font;
				new GUITextBlock(rectT, text, null, font2, Alignment.Left, false, "", null).Padding = Vector4.Zero;
			}
			GUIButton manageTalentButton = new GUIButton(new RectTransform(new Vector2(1f, 0.25f), headerTextArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ClientPermission.ManageBotTalents"), Alignment.Center, "GUIButtonSmall", null)
			{
				Enabled = false,
				UserData = "managebottalentsbutton",
				TextBlock = 
				{
					AutoScaleHorizontal = true
				}
			};
			if (TalentMenu.CanManageTalents(this))
			{
				manageTalentButton.Enabled = true;
			}
			Character character;
			if (this.Job != null)
			{
				character = this.Character;
				if (character == null || !character.IsDead)
				{
					GUILayoutGroup skillsArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.63f), paddedFrame.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.BottomCenter), null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
					{
						Stretch = true
					};
					List<Skill> skills = this.Job.GetSkills().ToList<Skill>();
					skills.Sort((Skill s1, Skill s2) => -s1.Level.CompareTo(s2.Level));
					RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), skillsArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text2 = TextManager.AddPunctuation(':', new LocalizedString[]
					{
						TextManager.Get("skills"),
						string.Empty
					});
					GUIFont font2 = font;
					new GUITextBlock(rectT2, text2, null, font2, Alignment.Left, false, "", null).Padding = Vector4.Zero;
					using (List<Skill>.Enumerator enumerator = skills.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Skill skill = enumerator.Current;
							Color textColor = Color.White * (0.5f + skill.Level / 200f);
							GUITextBlock skillName = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), skillsArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), skill.DisplayName, new Color?(textColor), font, Alignment.Left, false, "", null)
							{
								Padding = Vector4.Zero
							};
							float modifiedSkillLevel = skill.Level;
							if (this.Character != null)
							{
								modifiedSkillLevel = this.Character.GetSkillLevel(skill.Identifier);
							}
							if (!MathUtils.NearlyEqual(MathF.Round(modifiedSkillLevel), MathF.Round(skill.Level), 0.0001f))
							{
								int skillChange = (int)MathF.Round(modifiedSkillLevel - skill.Level);
								string changeText = ((skillChange > 0) ? "+" : "") + skillChange.ToString();
								RectTransform rectT3 = new RectTransform(new Vector2(1f, 1f), skillName.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
								defaultInterpolatedStringHandler.AppendFormatted<int>((int)skill.Level);
								defaultInterpolatedStringHandler.AppendLiteral(" (");
								defaultInterpolatedStringHandler.AppendFormatted(changeText);
								defaultInterpolatedStringHandler.AppendLiteral(")");
								new GUITextBlock(rectT3, defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(textColor), font, Alignment.CenterRight, false, "", null);
							}
							else
							{
								new GUITextBlock(new RectTransform(new Vector2(1f, 1f), skillName.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ((int)skill.Level).ToString(), new Color?(textColor), font, Alignment.CenterRight, false, "", null);
							}
						}
						goto IL_924;
					}
				}
			}
			character = this.Character;
			if (character != null && character.IsDead)
			{
				GUILayoutGroup deadArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.63f), paddedFrame.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.BottomCenter), null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true
				};
				LocalizedString left = TextManager.Get("deceased") + "\n";
				AfflictionPrefab affliction = this.Character.CauseOfDeath.Affliction;
				LocalizedString deadDescription = left + (((affliction != null) ? affliction.CauseOfDeathDescription : null) ?? TextManager.Get("CauseOfDeath." + this.Character.CauseOfDeath.Type.ToString()));
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), deadArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), deadDescription, new Color?(GUIStyle.Red), font, Alignment.TopLeft, false, "", null).Padding = Vector4.Zero;
			}
			IL_924:
			if (returnParent)
			{
				return frame;
			}
			return paddedFrame;
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x00038020 File Offset: 0x00036220
		private void DrawInfoFrameCharacterIcon(SpriteBatch sb, Rectangle componentRect)
		{
			if (this.HeadSprite == null)
			{
				return;
			}
			Vector2 targetAreaSize = componentRect.Size.ToVector2();
			float scale = Math.Min(targetAreaSize.X / this._headSprite.size.X, targetAreaSize.Y / this._headSprite.size.Y);
			this.DrawIcon(sb, componentRect.Location.ToVector2() + this._headSprite.size / 2f * scale, targetAreaSize, false);
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x000380B4 File Offset: 0x000362B4
		public GUIFrame CreateCharacterFrame(GUIComponent parent, string text, object userData)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(parent.Rect.Width, 40), parent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				IsFixedSize = false
			}, "ListBoxElement", null)
			{
				UserData = userData
			};
			Color? textColor = null;
			if (this.Job != null)
			{
				textColor = new Color?(this.Job.Prefab.UIColor);
			}
			GUITextBlock textBlock = new GUITextBlock(new RectTransform(Vector2.One, frame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(40, 0)
			}, text, textColor, GUIStyle.SmallFont, Alignment.Left, false, "", null);
			new GUICustomComponent(new RectTransform(new Point(frame.Rect.Height, frame.Rect.Height), frame.RectTransform, Anchor.CenterLeft, null, ScaleBasis.Normal, false)
			{
				IsFixedSize = false
			}, delegate(SpriteBatch sb, GUICustomComponent component)
			{
				this.DrawIcon(sb, component.Rect.Center.ToVector2(), component.Rect.Size.ToVector2(), false);
			}, null);
			return frame;
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x000381E4 File Offset: 0x000363E4
		private void GetDisguisedSprites(IdCard idCard)
		{
			if (idCard.Item.Tags == string.Empty)
			{
				return;
			}
			if (idCard.StoredOwnerAppearance.JobPrefab == null || idCard.StoredOwnerAppearance.Portrait == null)
			{
				ImmutableDictionary<Identifier, string> readTags = (from s in idCard.Item.Tags.Split(',', StringSplitOptions.None)
				where s.Contains(':')
				select s.Split(':', StringSplitOptions.None) into s
				select new ValueTuple<Identifier, string>(s[0].ToIdentifier(), s[1])).ToImmutableDictionary<Identifier, string>();
				if (readTags.None(null))
				{
					return;
				}
				if (idCard.StoredOwnerAppearance.JobPrefab == null)
				{
					idCard.StoredOwnerAppearance.ExtractJobPrefab(readTags);
				}
				if (idCard.StoredOwnerAppearance.Portrait == null)
				{
					idCard.StoredOwnerAppearance.ExtractAppearance(this, idCard);
				}
			}
			if (idCard.StoredOwnerAppearance.JobPrefab != null)
			{
				this.disguisedJobIcon = idCard.StoredOwnerAppearance.JobPrefab.Icon;
				this.disguisedJobColor = idCard.StoredOwnerAppearance.JobPrefab.UIColor;
			}
			this.disguisedPortrait = idCard.StoredOwnerAppearance.Portrait;
			this.disguisedSheetIndex = new Vector2?(idCard.StoredOwnerAppearance.SheetIndex);
			this.disguisedAttachmentSprites = idCard.StoredOwnerAppearance.Attachments;
			this.disguisedHairColor = idCard.StoredOwnerAppearance.HairColor;
			this.disguisedFacialHairColor = idCard.StoredOwnerAppearance.FacialHairColor;
			this.disguisedSkinColor = idCard.StoredOwnerAppearance.SkinColor;
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x0003838C File Offset: 0x0003658C
		public static Point CalculateOffset(Sprite sprite, Point offset)
		{
			return sprite.SourceRect.Size * offset;
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x000383B0 File Offset: 0x000365B0
		public void CalculateHeadPosition(Sprite sprite)
		{
			if (sprite == null)
			{
				return;
			}
			Vector2 sheetIndex = this.Head.SheetIndex;
			Point location = CharacterInfo.CalculateOffset(sprite, this.Head.SheetIndex.ToPoint());
			sprite.SourceRect = new Rectangle(location, sprite.SourceRect.Size);
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x00038404 File Offset: 0x00036604
		public void DrawBackground(SpriteBatch spriteBatch)
		{
			if (CharacterInfo.infoAreaPortraitBG == null)
			{
				return;
			}
			CharacterInfo.infoAreaPortraitBG.Draw(spriteBatch, HUDLayoutSettings.BottomRightInfoArea.Location.ToVector2(), Color.White, Vector2.Zero, 0f, new Vector2((float)HUDLayoutSettings.BottomRightInfoArea.Width / (float)CharacterInfo.infoAreaPortraitBG.SourceRect.Width, (float)HUDLayoutSettings.BottomRightInfoArea.Height / (float)CharacterInfo.infoAreaPortraitBG.SourceRect.Height), SpriteEffects.None, null);
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x00038490 File Offset: 0x00036690
		public void DrawForeground(SpriteBatch spriteBatch)
		{
			if (this.Character != null)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) is MultiPlayerCampaign)
				{
					int xfraction = (int)((float)HUDLayoutSettings.BottomRightInfoArea.Width * 0.2f);
					int yoffset = GUI.IntScale(6f);
					int walletAmount = this.Character.Wallet.Balance;
					LocalizedString str = (walletAmount >= 1000000) ? TextManager.Get("crewwallet.balance.toomuchtoshow") : TextManager.FormatCurrency(walletAmount, true);
					Vector2 size = GUIStyle.Font.MeasureString(str, false);
					int barHeight = GUI.IntScale(18f);
					Rectangle barRect = new Rectangle((int)((float)HUDLayoutSettings.BottomRightInfoArea.X + (float)xfraction / 2.5f), HUDLayoutSettings.BottomRightInfoArea.Bottom - barHeight - yoffset, HUDLayoutSettings.BottomRightInfoArea.Width - xfraction, barHeight);
					float textScale = Math.Max(0.1f, Math.Min((float)barRect.Width / size.X, (float)barRect.Height / size.Y)) - 0.01f;
					GUIStyle.WalletPortraitBG.Draw(spriteBatch, barRect, Color.White, SpriteEffects.None);
					int iconSize = GUI.IntScale(28f);
					int iconXOffset = iconSize / 2;
					Rectangle iconRect = new Rectangle(barRect.Right - iconXOffset, barRect.Top - iconSize / 4, iconSize, iconSize);
					GUIStyle.CrewWalletIconSmall.Draw(spriteBatch, iconRect, Color.White, SpriteEffects.None);
					float num;
					float num2;
					(size * textScale).Deconstruct(out num, out num2);
					float scaledTextSizeX = num;
					float scaledTextSizeY = num2;
					GUIStyle.Font.DrawString(spriteBatch, str, new Vector2((float)(barRect.Right - iconXOffset) - scaledTextSizeX - (float)GUI.IntScale(4f), (float)barRect.Center.Y - scaledTextSizeY / 2f), GUIStyle.TextColorNormal, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f, Alignment.TopLeft);
					return;
				}
			}
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x00038670 File Offset: 0x00036870
		private void SetHeadEffect(SpriteBatch spriteBatch)
		{
			ref Effect ptr = ref this.headEffectParameters.Effect;
			if (ptr == null)
			{
				ptr = GameMain.GameScreen.ThresholdTintEffect;
			}
			ref Dictionary<string, object> ptr2 = ref this.headEffectParameters.Params;
			if (ptr2 == null)
			{
				ptr2 = new Dictionary<string, object>();
			}
			this.headEffectParameters.Params["xBaseTexture"] = this.HeadSprite.Texture;
			Dictionary<string, object> @params = this.headEffectParameters.Params;
			string key = "xTintMaskTexture";
			Sprite sprite = this.tintMask;
			@params[key] = (((sprite != null) ? sprite.Texture : null) ?? GUI.WhiteTexture);
			this.headEffectParameters.Params["xCutoffTexture"] = GUI.WhiteTexture;
			this.headEffectParameters.Params["baseToCutoffSizeRatio"] = 1f;
			this.headEffectParameters.Params["highlightThreshold"] = this.tintHighlightThreshold;
			this.headEffectParameters.Params["highlightMultiplier"] = this.tintHighlightMultiplier;
			spriteBatch.SwapEffect(this.headEffectParameters);
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x00038788 File Offset: 0x00036988
		private void SetAttachmentEffect(SpriteBatch spriteBatch, WearableSprite attachment)
		{
			if (!this.attachmentEffectParameters.ContainsKey(attachment.Type))
			{
				this.attachmentEffectParameters.Add(attachment.Type, new SpriteBatch.EffectWithParams(GameMain.GameScreen.ThresholdTintEffect, new Dictionary<string, object>()));
			}
			Dictionary<string, object> parameters = this.attachmentEffectParameters[attachment.Type].Params;
			parameters["xBaseTexture"] = attachment.Sprite.Texture;
			parameters["xTintMaskTexture"] = GUI.WhiteTexture;
			parameters["xCutoffTexture"] = GUI.WhiteTexture;
			parameters["baseToCutoffSizeRatio"] = 1f;
			parameters["highlightThreshold"] = this.tintHighlightThreshold;
			parameters["highlightMultiplier"] = this.tintHighlightMultiplier;
			spriteBatch.SwapEffect(this.attachmentEffectParameters[attachment.Type]);
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x00038874 File Offset: 0x00036A74
		private Color GetAttachmentColor(WearableSprite attachment, Color hairColor, Color facialHairColor)
		{
			WearableType type = attachment.Type;
			if (type == WearableType.Hair)
			{
				return hairColor;
			}
			if (type - WearableType.Beard > 1)
			{
				return Color.White;
			}
			return facialHairColor;
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x000388A0 File Offset: 0x00036AA0
		public void DrawIcon(SpriteBatch spriteBatch, Vector2 screenPos, Vector2 targetAreaSize, bool flip = false)
		{
			Sprite headSprite = this.HeadSprite;
			if (headSprite != null)
			{
				SpriteEffects spriteEffects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				Effect currEffect = spriteBatch.GetCurrentEffect();
				float scale = Math.Min(targetAreaSize.X / headSprite.size.X, targetAreaSize.Y / headSprite.size.Y);
				headSprite.SourceRect = new Rectangle(CharacterInfo.CalculateOffset(headSprite, this.Head.SheetIndex.ToPoint()), headSprite.SourceRect.Size);
				this.SetHeadEffect(spriteBatch);
				Vector2 origin = headSprite.Origin;
				if (flip)
				{
					origin.X = headSprite.size.X - origin.X;
				}
				Sprite sprite = headSprite;
				Vector2 origin2 = origin;
				float scale2 = scale;
				sprite.Draw(spriteBatch, screenPos, this.Head.SkinColor, origin2, 0f, scale2, spriteEffects, null);
				if (this.AttachmentSprites != null)
				{
					float depthStep = 1E-06f;
					foreach (WearableSprite attachment in this.AttachmentSprites)
					{
						this.SetAttachmentEffect(spriteBatch, attachment);
						this.DrawAttachmentSprite(spriteBatch, attachment, headSprite, new Vector2?(this.Head.SheetIndex), screenPos, scale, depthStep, new Color?(this.GetAttachmentColor(attachment, this.Head.HairColor, this.Head.FacialHairColor)), spriteEffects);
						depthStep += depthStep;
					}
				}
				spriteBatch.SwapEffect(currEffect, null);
			}
		}

		// Token: 0x06000659 RID: 1625 RVA: 0x00038A30 File Offset: 0x00036C30
		public void DrawJobIcon(SpriteBatch spriteBatch, Rectangle area, bool evaluateDisguise = false)
		{
			if (evaluateDisguise && this.IsDisguised)
			{
				return;
			}
			Sprite sprite;
			if (this.IsDisguisedAsAnother && evaluateDisguise)
			{
				sprite = this.disguisedJobIcon;
			}
			else
			{
				Job job = this.Job;
				if (job == null)
				{
					sprite = null;
				}
				else
				{
					JobPrefab prefab = job.Prefab;
					sprite = ((prefab != null) ? prefab.Icon : null);
				}
			}
			Sprite icon = sprite;
			if (icon == null)
			{
				return;
			}
			Color iconColor = (!this.IsDisguisedAsAnother || !evaluateDisguise) ? this.Job.Prefab.UIColor : this.disguisedJobColor;
			icon.Draw(spriteBatch, area.Center.ToVector2(), iconColor, 0f, Math.Min((float)area.Width / (float)icon.SourceRect.Width, (float)area.Height / (float)icon.SourceRect.Height), SpriteEffects.None, null);
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x00038AF8 File Offset: 0x00036CF8
		private void DrawAttachmentSprite(SpriteBatch spriteBatch, WearableSprite attachment, Sprite head, Vector2? sheetIndex, Vector2 drawPos, float scale, float depthStep, Color? color = null, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			if (attachment.InheritSourceRect)
			{
				if (attachment.SheetIndex != null)
				{
					attachment.Sprite.SourceRect = new Rectangle(CharacterInfo.CalculateOffset(head, attachment.SheetIndex.Value), head.SourceRect.Size);
				}
				else if (sheetIndex != null)
				{
					attachment.Sprite.SourceRect = new Rectangle(CharacterInfo.CalculateOffset(head, sheetIndex.Value.ToPoint()), head.SourceRect.Size);
				}
				else
				{
					attachment.Sprite.SourceRect = head.SourceRect;
				}
			}
			Vector2 origin;
			if (attachment.InheritOrigin)
			{
				origin = head.Origin;
				attachment.Sprite.Origin = origin;
				if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally))
				{
					origin.X = head.size.X - origin.X;
				}
				if (spriteEffects.HasFlag(SpriteEffects.FlipVertically))
				{
					origin.Y = head.size.Y - origin.Y;
				}
			}
			else
			{
				origin = attachment.Sprite.Origin;
				if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally))
				{
					origin.X = attachment.Sprite.size.X - origin.X;
				}
				if (spriteEffects.HasFlag(SpriteEffects.FlipVertically))
				{
					origin.Y = attachment.Sprite.size.Y - origin.Y;
				}
			}
			float depth = attachment.Sprite.Depth;
			if (attachment.InheritLimbDepth)
			{
				depth = head.Depth - depthStep;
			}
			Sprite sprite = attachment.Sprite;
			Color color2 = color ?? Color.White;
			Vector2 origin2 = origin;
			float rotate = 0f;
			float? depth2 = new float?(depth);
			sprite.Draw(spriteBatch, drawPos, color2, origin2, rotate, scale, spriteEffects, depth2);
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x00038CEC File Offset: 0x00036EEC
		public static CharacterInfo ClientRead(Identifier speciesName, IReadMessage inc, bool requireJobPrefabFound = true)
		{
			ushort infoID = inc.ReadUInt16();
			string newName = inc.ReadString();
			string originalName = inc.ReadString();
			bool renamingEnabled = inc.ReadBoolean();
			BotStatus botStatus = (BotStatus)inc.ReadByte();
			int salary = inc.ReadInt32();
			int tagCount = (int)inc.ReadByte();
			HashSet<Identifier> tagSet = new HashSet<Identifier>();
			for (int i = 0; i < tagCount; i++)
			{
				tagSet.Add(inc.ReadIdentifier());
			}
			int hairIndex = (int)inc.ReadByte();
			int beardIndex = (int)inc.ReadByte();
			int moustacheIndex = (int)inc.ReadByte();
			int faceAttachmentIndex = (int)inc.ReadByte();
			Color skinColor = inc.ReadColorR8G8B8();
			Color hairColor = inc.ReadColorR8G8B8();
			Color facialHairColor = inc.ReadColorR8G8B8();
			Identifier npcId = inc.ReadIdentifier();
			Identifier factionId = inc.ReadIdentifier();
			float minReputationToHire = 0f;
			if (!factionId.IsEmpty)
			{
				minReputationToHire = inc.ReadSingle();
			}
			uint jobIdentifier = inc.ReadUInt32();
			int variant = (int)inc.ReadByte();
			JobPrefab jobPrefab = null;
			Dictionary<Identifier, float> skillLevels = new Dictionary<Identifier, float>();
			if (jobIdentifier > 0U)
			{
				jobPrefab = JobPrefab.Prefabs.Find((JobPrefab jp) => jp.UintIdentifier == jobIdentifier);
				if (jobPrefab == null && requireJobPrefabFound)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(98, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error while reading ");
					defaultInterpolatedStringHandler.AppendFormatted("CharacterInfo");
					defaultInterpolatedStringHandler.AppendLiteral(" received from the server: could not find a job prefab with the identifier \"");
					defaultInterpolatedStringHandler.AppendFormatted<uint>(jobIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				byte skillCount = inc.ReadByte();
				for (int j = 0; j < (int)skillCount; j++)
				{
					Identifier skillIdentifier = inc.ReadIdentifier();
					float skillLevel = inc.ReadSingle();
					skillLevels.Add(skillIdentifier, skillLevel);
				}
			}
			CharacterInfo ch = new CharacterInfo(speciesName, newName, originalName, jobPrefab, variant, Rand.RandSync.Unsynced, npcId)
			{
				ID = infoID,
				MinReputationToHire = new ValueTuple<Identifier, float>(factionId, minReputationToHire),
				RenamingEnabled = renamingEnabled
			};
			ch.BotStatus = botStatus;
			ch.Salary = salary;
			ch.RecreateHead(tagSet.ToImmutableHashSet<Identifier>(), hairIndex, beardIndex, moustacheIndex, faceAttachmentIndex);
			ch.Head.SkinColor = skinColor;
			ch.Head.HairColor = hairColor;
			ch.Head.FacialHairColor = facialHairColor;
			ch.SetPersonalityTrait();
			Job job = ch.Job;
			if (job != null)
			{
				job.OverrideSkills(skillLevels);
			}
			ch.ExperiencePoints = inc.ReadInt32();
			ch.AdditionalTalentPoints = inc.ReadRangedInteger(0, 100);
			ch.PermanentlyDead = inc.ReadBoolean();
			ch.TalentRefundPoints = inc.ReadInt32();
			ch.TalentResetCount = inc.ReadInt32();
			return ch;
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x00038F74 File Offset: 0x00037174
		public void CreateIcon(RectTransform rectT)
		{
			this.LoadHeadAttachments();
			new GUICustomComponent(rectT, delegate(SpriteBatch sb, GUICustomComponent component)
			{
				this.DrawIcon(sb, component.Rect.Center.ToVector2(), component.Rect.Size.ToVector2(), false);
			}, null);
		}

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x0600065D RID: 1629 RVA: 0x00038F90 File Offset: 0x00037190
		// (set) Token: 0x0600065E RID: 1630 RVA: 0x00038F98 File Offset: 0x00037198
		public CharacterInfo.HeadInfo Head
		{
			get
			{
				return this.head;
			}
			set
			{
				if (this.head != value && value != null)
				{
					this.head = value;
					this.HeadSprite = null;
					this.AttachmentSprites = null;
					this.hairs = null;
					this.beards = null;
					this.moustaches = null;
					this.faceAttachments = null;
				}
			}
		}

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x0600065F RID: 1631 RVA: 0x00038FD8 File Offset: 0x000371D8
		public bool IsMale
		{
			get
			{
				CharacterInfo.HeadInfo headInfo = this.head;
				bool? flag;
				if (headInfo == null)
				{
					flag = null;
				}
				else
				{
					CharacterInfo.HeadPreset preset = headInfo.Preset;
					if (preset == null)
					{
						flag = null;
					}
					else
					{
						ImmutableHashSet<Identifier> tagSet = preset.TagSet;
						flag = ((tagSet != null) ? new bool?(tagSet.Contains(this.maleIdentifier)) : null);
					}
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
		}

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000660 RID: 1632 RVA: 0x0003903C File Offset: 0x0003723C
		public bool IsFemale
		{
			get
			{
				CharacterInfo.HeadInfo headInfo = this.head;
				bool? flag;
				if (headInfo == null)
				{
					flag = null;
				}
				else
				{
					CharacterInfo.HeadPreset preset = headInfo.Preset;
					if (preset == null)
					{
						flag = null;
					}
					else
					{
						ImmutableHashSet<Identifier> tagSet = preset.TagSet;
						flag = ((tagSet != null) ? new bool?(tagSet.Contains(this.femaleIdentifier)) : null);
					}
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
		}

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06000661 RID: 1633 RVA: 0x0003909E File Offset: 0x0003729E
		public CharacterInfoPrefab Prefab
		{
			get
			{
				return CharacterPrefab.Prefabs[this.SpeciesName].CharacterInfoPrefab;
			}
		}

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x06000662 RID: 1634 RVA: 0x000390B5 File Offset: 0x000372B5
		// (set) Token: 0x06000663 RID: 1635 RVA: 0x000390BD File Offset: 0x000372BD
		public BotStatus BotStatus
		{
			get
			{
				return this.botStatus;
			}
			set
			{
				this.botStatus = value;
				if (this.botStatus == BotStatus.ActiveService && this.character == null)
				{
					this.PendingSpawnToActiveService = true;
				}
			}
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x06000664 RID: 1636 RVA: 0x000390DE File Offset: 0x000372DE
		public bool IsOnReserveBench
		{
			get
			{
				return this.BotStatus == BotStatus.ReserveBench;
			}
		}

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x06000665 RID: 1637 RVA: 0x000390E9 File Offset: 0x000372E9
		public bool HasNickname
		{
			get
			{
				return this.Name != this.OriginalName;
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000666 RID: 1638 RVA: 0x000390FC File Offset: 0x000372FC
		// (set) Token: 0x06000667 RID: 1639 RVA: 0x00039104 File Offset: 0x00037304
		public string OriginalName { get; private set; }

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000668 RID: 1640 RVA: 0x00039110 File Offset: 0x00037310
		public HumanPrefab HumanPrefab
		{
			get
			{
				ValueTuple<Identifier, Identifier> humanPrefabIds = this.HumanPrefabIds;
				Identifier identifier = default(Identifier);
				if (humanPrefabIds.Item1 == identifier)
				{
					Identifier identifier2 = default(Identifier);
					if (humanPrefabIds.Item2 == identifier2)
					{
						return null;
					}
				}
				if (this._humanPrefab == null)
				{
					this._humanPrefab = NPCSet.Get(this.HumanPrefabIds.Item1, this.HumanPrefabIds.Item2, true, null);
				}
				return this._humanPrefab;
			}
		}

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000669 RID: 1641 RVA: 0x00039188 File Offset: 0x00037388
		public string DisplayName
		{
			get
			{
				if (this.Character == null || !this.Character.HideFace)
				{
					this.IsDisguised = (this.IsDisguisedAsAnother = false);
					return this.Name;
				}
				if (GameMain.NetworkMember != null && !GameMain.NetworkMember.ServerSettings.AllowDisguises)
				{
					this.IsDisguised = (this.IsDisguisedAsAnother = false);
					return this.Name;
				}
				if (this.Character.Inventory != null)
				{
					Item idCard = this.Character.Inventory.GetItemInLimbSlot(InvSlotType.Card);
					string text;
					if (idCard == null)
					{
						text = null;
					}
					else
					{
						IdCard component = idCard.GetComponent<IdCard>();
						text = ((component != null) ? component.OwnerName : null);
					}
					return text ?? "???";
				}
				return "???";
			}
		}

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x0600066A RID: 1642 RVA: 0x0003923A File Offset: 0x0003743A
		public Identifier SpeciesName { get; }

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x0600066B RID: 1643 RVA: 0x00039242 File Offset: 0x00037442
		// (set) Token: 0x0600066C RID: 1644 RVA: 0x0003924A File Offset: 0x0003744A
		public Character Character
		{
			get
			{
				return this.character;
			}
			set
			{
				this.character = value;
				if (this.character != null)
				{
					this.PendingSpawnToActiveService = false;
				}
			}
		}

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x0600066D RID: 1645 RVA: 0x00039262 File Offset: 0x00037462
		// (set) Token: 0x0600066E RID: 1646 RVA: 0x0003926A File Offset: 0x0003746A
		public int ExperiencePoints { get; private set; }

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x0600066F RID: 1647 RVA: 0x00039273 File Offset: 0x00037473
		// (set) Token: 0x06000670 RID: 1648 RVA: 0x0003927B File Offset: 0x0003747B
		public int TalentRefundPoints
		{
			get
			{
				return this.talentRefundPoints;
			}
			set
			{
				this.talentRefundPoints = MathHelper.Max(value, 0);
			}
		}

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x06000671 RID: 1649 RVA: 0x0003928A File Offset: 0x0003748A
		// (set) Token: 0x06000672 RID: 1650 RVA: 0x00039292 File Offset: 0x00037492
		public HashSet<Identifier> UnlockedTalents { get; private set; } = new HashSet<Identifier>();

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x06000673 RID: 1651 RVA: 0x0003929B File Offset: 0x0003749B
		// (set) Token: 0x06000674 RID: 1652 RVA: 0x000392A3 File Offset: 0x000374A3
		public HashSet<Identifier> ResettableExtraTalents { get; private set; } = new HashSet<Identifier>();

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06000675 RID: 1653 RVA: 0x000392AC File Offset: 0x000374AC
		// (set) Token: 0x06000676 RID: 1654 RVA: 0x000392B4 File Offset: 0x000374B4
		public int TalentResetCount
		{
			get
			{
				return this.talentResetCount;
			}
			set
			{
				this.talentResetCount = MathHelper.Max(value, 0);
			}
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x000392C4 File Offset: 0x000374C4
		public IEnumerable<Identifier> GetUnlockedTalentsInTree()
		{
			TalentTree talentTree;
			if (!TalentTree.JobTalentTrees.TryGet(this.Job.Prefab.Identifier, out talentTree))
			{
				return Enumerable.Empty<Identifier>();
			}
			return from t in this.UnlockedTalents
			where talentTree.TalentIsInTree(t)
			select t;
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x00039318 File Offset: 0x00037518
		public IEnumerable<Identifier> GetUnlockedTalentsOutsideTree()
		{
			TalentTree talentTree;
			if (!TalentTree.JobTalentTrees.TryGet(this.Job.Prefab.Identifier, out talentTree))
			{
				return Enumerable.Empty<Identifier>();
			}
			return from t in this.UnlockedTalents
			where !talentTree.TalentIsInTree(t)
			select t;
		}

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000679 RID: 1657 RVA: 0x0003936A File Offset: 0x0003756A
		// (set) Token: 0x0600067A RID: 1658 RVA: 0x00039372 File Offset: 0x00037572
		public int AdditionalTalentPoints
		{
			get
			{
				return this.additionalTalentPoints;
			}
			set
			{
				this.additionalTalentPoints = MathHelper.Clamp(value, 0, 100);
			}
		}

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x0600067B RID: 1659 RVA: 0x00039383 File Offset: 0x00037583
		// (set) Token: 0x0600067C RID: 1660 RVA: 0x000393AD File Offset: 0x000375AD
		public Sprite HeadSprite
		{
			get
			{
				if (this._headSprite == null)
				{
					this.LoadHeadSprite();
				}
				if (this._headSprite != null)
				{
					this.CalculateHeadPosition(this._headSprite);
				}
				return this._headSprite;
			}
			private set
			{
				if (this._headSprite != null)
				{
					this._headSprite.Remove();
				}
				this._headSprite = value;
			}
		}

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x0600067D RID: 1661 RVA: 0x000393C9 File Offset: 0x000375C9
		// (set) Token: 0x0600067E RID: 1662 RVA: 0x000393DF File Offset: 0x000375DF
		public Sprite Portrait
		{
			get
			{
				if (this.portrait == null)
				{
					this.LoadHeadSprite();
				}
				return this.portrait;
			}
			private set
			{
				if (this.portrait != null)
				{
					this.portrait.Remove();
				}
				this.portrait = value;
			}
		}

		// Token: 0x0600067F RID: 1663 RVA: 0x000393FC File Offset: 0x000375FC
		public void CheckDisguiseStatus(bool handleBuff, IdCard idCard = null)
		{
			if (this.Character == null)
			{
				return;
			}
			string currentlyDisplayedName = this.DisplayName;
			this.IsDisguised = (currentlyDisplayedName == "???");
			this.IsDisguisedAsAnother = (!this.IsDisguised && currentlyDisplayedName != this.Name);
			if (this.IsDisguisedAsAnother)
			{
				AfflictionPrefab afflictionPrefab;
				if (handleBuff && AfflictionPrefab.Prefabs.TryGet("disguised", out afflictionPrefab))
				{
					this.Character.CharacterHealth.ApplyAffliction(null, afflictionPrefab.Instantiate(100f, null), true, false, true);
				}
				if (idCard == null)
				{
					CharacterInventory inventory = this.Character.Inventory;
					IdCard idCard2;
					if (inventory == null)
					{
						idCard2 = null;
					}
					else
					{
						Item itemInLimbSlot = inventory.GetItemInLimbSlot(InvSlotType.Card);
						idCard2 = ((itemInLimbSlot != null) ? itemInLimbSlot.GetComponent<IdCard>() : null);
					}
					idCard = idCard2;
				}
				if (idCard != null)
				{
					this.GetDisguisedSprites(idCard);
					return;
				}
			}
			this.disguisedJobIcon = null;
			this.disguisedPortrait = null;
			if (handleBuff)
			{
				this.Character.CharacterHealth.ReduceAfflictionOnAllLimbs("disguised".ToIdentifier(), 100f, null, null);
			}
		}

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06000680 RID: 1664 RVA: 0x000394F7 File Offset: 0x000376F7
		// (set) Token: 0x06000681 RID: 1665 RVA: 0x0003950D File Offset: 0x0003770D
		public List<WearableSprite> AttachmentSprites
		{
			get
			{
				if (this.attachmentSprites == null)
				{
					this.LoadAttachmentSprites();
				}
				return this.attachmentSprites;
			}
			private set
			{
				if (this.attachmentSprites != null)
				{
					this.attachmentSprites.ForEach(delegate(WearableSprite s)
					{
						Sprite sprite = s.Sprite;
						if (sprite == null)
						{
							return;
						}
						sprite.Remove();
					});
				}
				this.attachmentSprites = value;
			}
		}

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x06000682 RID: 1666 RVA: 0x00039548 File Offset: 0x00037748
		// (set) Token: 0x06000683 RID: 1667 RVA: 0x00039550 File Offset: 0x00037750
		public ContentXElement CharacterConfigElement { get; set; }

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x06000684 RID: 1668 RVA: 0x00039559 File Offset: 0x00037759
		// (set) Token: 0x06000685 RID: 1669 RVA: 0x00039561 File Offset: 0x00037761
		public NPCPersonalityTrait PersonalityTrait { get; private set; }

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x06000686 RID: 1670 RVA: 0x0003956A File Offset: 0x0003776A
		public static int HighestManualOrderPriority
		{
			get
			{
				return 3;
			}
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x00039570 File Offset: 0x00037770
		public int GetManualOrderPriority(Order order)
		{
			if (order != null && order.AssignmentPriority < 100 && this.CurrentOrders.Any<Order>())
			{
				int orderPriority = CharacterInfo.HighestManualOrderPriority;
				int i = 0;
				while (i < this.CurrentOrders.Count && order.AssignmentPriority < this.CurrentOrders[i].AssignmentPriority)
				{
					orderPriority--;
					i++;
				}
				return Math.Max(orderPriority, 1);
			}
			return CharacterInfo.HighestManualOrderPriority;
		}

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x06000688 RID: 1672 RVA: 0x000395DD File Offset: 0x000377DD
		public List<Order> CurrentOrders { get; } = new List<Order>();

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06000689 RID: 1673 RVA: 0x000395E5 File Offset: 0x000377E5
		// (set) Token: 0x0600068A RID: 1674 RVA: 0x000395ED File Offset: 0x000377ED
		public List<Identifier> SpriteTags { get; private set; }

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x0600068B RID: 1675 RVA: 0x000395F8 File Offset: 0x000377F8
		// (set) Token: 0x0600068C RID: 1676 RVA: 0x00039684 File Offset: 0x00037884
		public RagdollParams Ragdoll
		{
			get
			{
				if (this.ragdoll == null)
				{
					Identifier speciesName = this.SpeciesName;
					this.ragdoll = (this.CharacterConfigElement.GetAttributeBool("humanoid", speciesName == CharacterPrefab.HumanSpeciesName) ? RagdollParams.GetDefaultRagdollParams<HumanRagdollParams>(this.SpeciesName, this.CharacterConfigElement, this.CharacterConfigElement.ContentPackage) : RagdollParams.GetDefaultRagdollParams<FishRagdollParams>(this.SpeciesName, this.CharacterConfigElement, this.CharacterConfigElement.ContentPackage));
				}
				return this.ragdoll;
			}
			set
			{
				this.ragdoll = value;
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x0600068D RID: 1677 RVA: 0x0003968D File Offset: 0x0003788D
		public bool IsAttachmentsLoaded
		{
			get
			{
				return this.Head.HairIndex > -1 && this.Head.BeardIndex > -1 && this.Head.MoustacheIndex > -1 && this.Head.FaceAttachmentIndex > -1;
			}
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x000396C9 File Offset: 0x000378C9
		public IEnumerable<ContentXElement> GetValidAttachmentElements(IEnumerable<ContentXElement> elements, CharacterInfo.HeadPreset headPreset, WearableType? wearableType = null)
		{
			return this.FilterElements(elements, headPreset.TagSet, wearableType);
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x000396D9 File Offset: 0x000378D9
		public int CountValidAttachmentsOfType(WearableType wearableType)
		{
			return this.GetValidAttachmentElements(this.Wearables, this.Head.Preset, new WearableType?(wearableType)).Count<ContentXElement>();
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x00039700 File Offset: 0x00037900
		private void GetName(Rand.RandSync randSync, out string name)
		{
			ContentXElement nameElement = this.CharacterConfigElement.GetChildElement("names") ?? this.CharacterConfigElement.GetChildElement("name");
			ContentPath namesXmlFile = ((nameElement != null) ? nameElement.GetAttributeContentPath("path") : null) ?? ContentPath.Empty;
			XElement namesXml;
			if (!namesXmlFile.IsNullOrEmpty())
			{
				XDocument doc = XMLExtensions.TryLoadXml(namesXmlFile);
				namesXml = doc.Root;
			}
			else
			{
				namesXml = new XElement("names", new XAttribute("format", "[firstname] [lastname]"));
				ContentXElement contentXElement = null;
				string text;
				if (!(nameElement == contentXElement))
				{
					ContentPath attributeContentPath = nameElement.GetAttributeContentPath("firstname");
					text = this.ReplaceVars(((attributeContentPath != null) ? attributeContentPath.Value : null) ?? "");
				}
				else
				{
					text = string.Empty;
				}
				string firstNamesPath = text;
				contentXElement = null;
				string text2;
				if (!(nameElement == contentXElement))
				{
					ContentPath attributeContentPath2 = nameElement.GetAttributeContentPath("lastname");
					text2 = this.ReplaceVars(((attributeContentPath2 != null) ? attributeContentPath2.Value : null) ?? "");
				}
				else
				{
					text2 = string.Empty;
				}
				string lastNamesPath = text2;
				if (File.Exists(firstNamesPath) && File.Exists(lastNamesPath))
				{
					string[] firstNames = File.ReadAllLines(firstNamesPath, null, true);
					string[] lastNames = File.ReadAllLines(lastNamesPath, null, true);
					namesXml.Add(from n in firstNames
					select new XElement("firstname", new XAttribute("value", n)));
					namesXml.Add(from n in lastNames
					select new XElement("lastname", new XAttribute("value", n)));
				}
				else
				{
					XDocument doc2 = XMLExtensions.TryLoadXml("Content/Characters/Human/names.xml");
					namesXml = doc2.Root;
				}
			}
			name = namesXml.GetAttributeString("format", "");
			Dictionary<Identifier, List<string>> entries = new Dictionary<Identifier, List<string>>();
			foreach (XElement subElement in namesXml.Elements())
			{
				Identifier elemName = subElement.NameAsIdentifier();
				if (!entries.ContainsKey(elemName))
				{
					entries.Add(elemName, new List<string>());
				}
				ImmutableHashSet<Identifier> identifiers = subElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
				if (identifiers.IsSubsetOf(this.Head.Preset.TagSet))
				{
					entries[elemName].Add(subElement.GetAttributeString("value", ""));
				}
			}
			foreach (Identifier i in entries.Keys)
			{
				string text3 = name;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(i);
				defaultInterpolatedStringHandler.AppendLiteral("]");
				name = text3.Replace(defaultInterpolatedStringHandler.ToStringAndClear(), entries[i].GetRandom(randSync), StringComparison.OrdinalIgnoreCase);
			}
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x000399F0 File Offset: 0x00037BF0
		private static void LoadTagsBackwardsCompatibility(XElement element, HashSet<Identifier> tags)
		{
			Identifier gender = element.GetAttributeIdentifier("gender", "");
			int headSpriteId = element.GetAttributeInt("headspriteid", -1);
			if (!gender.IsEmpty)
			{
				tags.Add(gender);
			}
			if (headSpriteId > 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
				defaultInterpolatedStringHandler.AppendLiteral("head");
				defaultInterpolatedStringHandler.AppendFormatted<int>(headSpriteId);
				tags.Add(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
			}
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x00039A60 File Offset: 0x00037C60
		private static bool ElementHasSpecifierTags(XElement element)
		{
			return element.GetAttributeBool("specifiertags", element.GetAttributeBool("genders", element.GetAttributeBool("races", false)));
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x00039A84 File Offset: 0x00037C84
		public CharacterInfo(Identifier speciesName, string name = "", string originalName = "", Either<Job, JobPrefab> jobOrJobPrefab = null, int variant = 0, Rand.RandSync randSync = Rand.RandSync.Unsynced, Identifier npcIdentifier = default(Identifier))
		{
			Option.UnspecifiedNone none = Option.None;
			this.LastRewardDistribution = none;
			this.SavedStatValues = new Dictionary<StatTypes, List<SavedStatValue>>();
			this.LastResistanceMultiplierSkillLossDeath = 1f;
			this.LastResistanceMultiplierSkillLossRespawn = 1f;
			base..ctor();
			JobPrefab jobPrefab = null;
			Job job = null;
			if (jobOrJobPrefab != null)
			{
				jobOrJobPrefab.TryGet(out job);
				jobOrJobPrefab.TryGet(out jobPrefab);
			}
			this.ID = CharacterInfo.idCounter;
			CharacterInfo.idCounter += 1;
			if (CharacterInfo.idCounter == 0)
			{
				CharacterInfo.idCounter += 1;
			}
			this.SpeciesName = speciesName;
			this.SpriteTags = new List<Identifier>();
			CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(this.SpeciesName);
			this.CharacterConfigElement = ((characterPrefab != null) ? characterPrefab.ConfigElement : null);
			ContentXElement characterConfigElement = this.CharacterConfigElement;
			ContentXElement contentXElement = null;
			if (characterConfigElement == contentXElement)
			{
				return;
			}
			this.HasSpecifierTags = CharacterInfo.ElementHasSpecifierTags(this.CharacterConfigElement);
			if (this.HasSpecifierTags)
			{
				ContentXElement characterConfigElement2 = this.CharacterConfigElement;
				string key = "haircolors";
				ValueTuple<Color, float>[] array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(Color.WhiteSmoke, 100f)
				};
				this.HairColors = characterConfigElement2.GetAttributeTupleArray<Color, float>(key, array).ToImmutableArray<ValueTuple<Color, float>>();
				ContentXElement characterConfigElement3 = this.CharacterConfigElement;
				string key2 = "facialhaircolors";
				array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(Color.WhiteSmoke, 100f)
				};
				this.FacialHairColors = characterConfigElement3.GetAttributeTupleArray<Color, float>(key2, array).ToImmutableArray<ValueTuple<Color, float>>();
				ContentXElement characterConfigElement4 = this.CharacterConfigElement;
				string key3 = "skincolors";
				array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(new Color(255, 215, 200, 255), 100f)
				};
				this.SkinColors = characterConfigElement4.GetAttributeTupleArray<Color, float>(key3, array).ToImmutableArray<ValueTuple<Color, float>>();
				CharacterInfoPrefab prefab = this.Prefab;
				CharacterInfo.HeadPreset headPreset = (prefab != null) ? prefab.Heads.GetRandom(randSync) : null;
				if (headPreset == null)
				{
					DebugConsole.ThrowError("Failed to find a head preset!", null, null, false, false);
				}
				this.Head = new CharacterInfo.HeadInfo(this, headPreset, 0, 0, 0, 0);
				this.SetAttachments(randSync);
				this.SetColors(randSync);
				this.Job = (job ?? ((jobPrefab == null) ? Job.Random(false, Rand.RandSync.Unsynced) : new Job(jobPrefab, false, randSync, variant, Array.Empty<Skill>())));
				if (!string.IsNullOrEmpty(name))
				{
					this.Name = name;
				}
				else
				{
					this.Name = this.GetRandomName(randSync);
				}
				this.TryLoadNameAndTitle(npcIdentifier);
				this.SetPersonalityTrait();
				this.Salary = this.CalculateSalary(0, 1f);
			}
			this.OriginalName = ((!string.IsNullOrEmpty(originalName)) ? originalName : this.Name);
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x00039D8C File Offset: 0x00037F8C
		private void SetPersonalityTrait()
		{
			this.PersonalityTrait = NPCPersonalityTrait.GetRandom(this.Name + string.Concat<Identifier>(from tag in this.Head.Preset.TagSet
			orderby tag
			select tag));
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x00039DE8 File Offset: 0x00037FE8
		public string GetRandomName(Rand.RandSync randSync)
		{
			string name;
			this.GetName(randSync, out name);
			return name;
		}

		// Token: 0x06000696 RID: 1686 RVA: 0x00039DFF File Offset: 0x00037FFF
		public void SetNameBasedOnJob()
		{
			if (this.Job == null)
			{
				return;
			}
			this.Name = this.Job.Name.Value;
			this.OriginalName = this.Name;
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00039E2C File Offset: 0x0003802C
		public static Color SelectRandomColor([TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})] in ImmutableArray<ValueTuple<Color, float>> array, Rand.RandSync randSync)
		{
			return ToolBox.SelectWeightedRandom<ValueTuple<Color, float>>(array, (from p in array
			select p.Item2).ToArray<float>(), randSync).Item1;
		}

		// Token: 0x06000698 RID: 1688 RVA: 0x00039E80 File Offset: 0x00038080
		private void SetAttachments(Rand.RandSync randSync)
		{
			CharacterInfo.<>c__DisplayClass179_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.randSync = randSync;
			this.LoadHeadAttachments();
			this.Head.HairIndex = this.<SetAttachments>g__pickRandomIndex|179_0(this.Hairs, ref CS$<>8__locals1);
			this.Head.BeardIndex = this.<SetAttachments>g__pickRandomIndex|179_0(this.Beards, ref CS$<>8__locals1);
			this.Head.MoustacheIndex = this.<SetAttachments>g__pickRandomIndex|179_0(this.Moustaches, ref CS$<>8__locals1);
			this.Head.FaceAttachmentIndex = this.<SetAttachments>g__pickRandomIndex|179_0(this.FaceAttachments, ref CS$<>8__locals1);
		}

		// Token: 0x06000699 RID: 1689 RVA: 0x00039F08 File Offset: 0x00038108
		private void SetColors(Rand.RandSync randSync)
		{
			this.Head.HairColor = CharacterInfo.SelectRandomColor(this.HairColors, randSync);
			this.Head.FacialHairColor = CharacterInfo.SelectRandomColor(this.FacialHairColors, randSync);
			this.Head.SkinColor = CharacterInfo.SelectRandomColor(this.SkinColors, randSync);
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x00039F5C File Offset: 0x0003815C
		private bool IsColorValid(in Color clr)
		{
			Color color = clr;
			if (color.R == 0)
			{
				color = clr;
				if (color.G == 0)
				{
					color = clr;
					return color.B > 0;
				}
			}
			return true;
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x00039F9C File Offset: 0x0003819C
		public void CheckColors()
		{
			if (!this.IsColorValid(this.Head.HairColor))
			{
				this.Head.HairColor = CharacterInfo.SelectRandomColor(this.HairColors, Rand.RandSync.Unsynced);
			}
			if (!this.IsColorValid(this.Head.FacialHairColor))
			{
				this.Head.FacialHairColor = CharacterInfo.SelectRandomColor(this.FacialHairColors, Rand.RandSync.Unsynced);
			}
			if (!this.IsColorValid(this.Head.SkinColor))
			{
				this.Head.SkinColor = CharacterInfo.SelectRandomColor(this.SkinColors, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x0003A028 File Offset: 0x00038228
		public CharacterInfo(ContentXElement infoElement, Identifier npcIdentifier = default(Identifier))
		{
			Option.UnspecifiedNone none = Option.None;
			this.LastRewardDistribution = none;
			this.SavedStatValues = new Dictionary<StatTypes, List<SavedStatValue>>();
			this.LastResistanceMultiplierSkillLossDeath = 1f;
			this.LastResistanceMultiplierSkillLossRespawn = 1f;
			base..ctor();
			this.ID = CharacterInfo.idCounter;
			CharacterInfo.idCounter += 1;
			this.Name = infoElement.GetAttributeString("name", "");
			this.OriginalName = infoElement.GetAttributeString("originalname", null);
			this.Salary = infoElement.GetAttributeInt("salary", 1000);
			this.ExperiencePoints = infoElement.GetAttributeInt("experiencepoints", 0);
			this.AdditionalTalentPoints = infoElement.GetAttributeInt("additionaltalentpoints", 0);
			this.TalentResetCount = infoElement.GetAttributeInt("talentResetCount", 0);
			HashSet<Identifier> tags = infoElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			CharacterInfo.LoadTagsBackwardsCompatibility(infoElement, tags);
			this.SpeciesName = infoElement.GetAttributeIdentifier("speciesname", "");
			this.PermanentlyDead = infoElement.GetAttributeBool("permanentlydead", false);
			this.BotStatus = (infoElement.GetAttributeBool("IsOnReserveBench", false) ? BotStatus.ReserveBench : BotStatus.ActiveService);
			this.RenamingEnabled = infoElement.GetAttributeBool("renamingenabled", false);
			Identifier identifier = this.SpeciesName;
			if (identifier.IsEmpty)
			{
				throw new InvalidOperationException("SpeciesName not defined");
			}
			CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(this.SpeciesName);
			ContentXElement element = (characterPrefab != null) ? characterPrefab.ConfigElement : null;
			ContentXElement contentXElement = null;
			if (element == contentXElement)
			{
				return;
			}
			this.CharacterConfigElement = element;
			this.HasSpecifierTags = CharacterInfo.ElementHasSpecifierTags(this.CharacterConfigElement);
			if (this.HasSpecifierTags)
			{
				this.RecreateHead(tags.ToImmutableHashSet<Identifier>(), infoElement.GetAttributeInt("hairindex", -1), infoElement.GetAttributeInt("beardindex", -1), infoElement.GetAttributeInt("moustacheindex", -1), infoElement.GetAttributeInt("faceattachmentindex", -1));
				ContentXElement characterConfigElement = this.CharacterConfigElement;
				string key = "haircolors";
				ValueTuple<Color, float>[] array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(Color.WhiteSmoke, 100f)
				};
				this.HairColors = characterConfigElement.GetAttributeTupleArray<Color, float>(key, array).ToImmutableArray<ValueTuple<Color, float>>();
				ContentXElement characterConfigElement2 = this.CharacterConfigElement;
				string key2 = "facialhaircolors";
				array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(Color.WhiteSmoke, 100f)
				};
				this.FacialHairColors = characterConfigElement2.GetAttributeTupleArray<Color, float>(key2, array).ToImmutableArray<ValueTuple<Color, float>>();
				ContentXElement characterConfigElement3 = this.CharacterConfigElement;
				string key3 = "skincolors";
				array = new ValueTuple<Color, float>[]
				{
					new ValueTuple<Color, float>(new Color(255, 215, 200, 255), 100f)
				};
				this.SkinColors = characterConfigElement3.GetAttributeTupleArray<Color, float>(key3, array).ToImmutableArray<ValueTuple<Color, float>>();
				CharacterInfo.HeadInfo headInfo = this.Head;
				string key4 = "skincolor";
				Color transparent = Color.Transparent;
				headInfo.SkinColor = infoElement.GetAttributeColor(key4, transparent);
				CharacterInfo.HeadInfo headInfo2 = this.Head;
				string key5 = "haircolor";
				transparent = Color.Transparent;
				headInfo2.HairColor = infoElement.GetAttributeColor(key5, transparent);
				CharacterInfo.HeadInfo headInfo3 = this.Head;
				string key6 = "facialhaircolor";
				transparent = Color.Transparent;
				headInfo3.FacialHairColor = infoElement.GetAttributeColor(key6, transparent);
				this.CheckColors();
				this.TryLoadNameAndTitle(npcIdentifier);
				if (string.IsNullOrEmpty(this.Name))
				{
					ContentXElement nameElement = this.CharacterConfigElement.GetChildElement("names");
					contentXElement = null;
					if (nameElement != contentXElement)
					{
						this.GetName(Rand.RandSync.ServerAndClient, out this.Name);
					}
				}
			}
			if (string.IsNullOrEmpty(this.OriginalName))
			{
				this.OriginalName = this.Name;
			}
			this.StartItemsGiven = infoElement.GetAttributeBool("startitemsgiven", false);
			Identifier personalityName = infoElement.GetAttributeIdentifier("personality", "");
			if (personalityName != Identifier.Empty)
			{
				NPCPersonalityTrait trait;
				if (!NPCPersonalityTrait.Traits.TryGet(personalityName, out trait))
				{
					PrefabCollection<NPCPersonalityTrait> traits = NPCPersonalityTrait.Traits;
					identifier = " ".ToIdentifier();
					if (!traits.TryGet(personalityName.Replace(identifier, Identifier.Empty), out trait))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error in CharacterInfo \"");
						defaultInterpolatedStringHandler.AppendFormatted(this.OriginalName);
						defaultInterpolatedStringHandler.AppendLiteral("\": could not find a personality trait with the identifier \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(personalityName);
						defaultInterpolatedStringHandler.AppendLiteral("\".");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						goto IL_4A2;
					}
				}
				this.PersonalityTrait = trait;
			}
			IL_4A2:
			this.HumanPrefabIds = new ValueTuple<Identifier, Identifier>(infoElement.GetAttributeIdentifier("npcsetid", Identifier.Empty), infoElement.GetAttributeIdentifier("npcid", Identifier.Empty));
			this.MissionsCompletedSinceDeath = infoElement.GetAttributeInt("missionscompletedsincedeath", 0);
			this.UnlockedTalents = new HashSet<Identifier>();
			this.MinReputationToHire = new ValueTuple<Identifier, float>(infoElement.GetAttributeIdentifier("factionId", Identifier.Empty), infoElement.GetAttributeFloat("minreputation", 0f));
			foreach (ContentXElement subElement in infoElement.Elements())
			{
				bool jobCreated = false;
				Identifier elementName = subElement.Name.ToIdentifier<XName>();
				if (elementName == "job" && !jobCreated)
				{
					this.Job = new Job(subElement);
				}
				else
				{
					if (elementName == "savedstatvalues")
					{
						using (IEnumerator<ContentXElement> enumerator2 = subElement.Elements().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								ContentXElement cxe = enumerator2.Current;
								XElement savedStat = cxe;
								string statTypeString = savedStat.GetAttributeString("stattype", "").ToLowerInvariant();
								StatTypes statType;
								if (!Enum.TryParse<StatTypes>(statTypeString, true, out statType))
								{
									DebugConsole.ThrowError("Invalid stat type type \"" + statTypeString + "\" when loading character data in CharacterInfo!", null, null, false, false);
								}
								else
								{
									float value = savedStat.GetAttributeFloat("statvalue", 0f);
									if (value != 0f)
									{
										Identifier statIdentifier = savedStat.GetAttributeIdentifier("statidentifier", Identifier.Empty);
										if (statIdentifier.IsEmpty)
										{
											DebugConsole.ThrowError("Stat identifier not specified for Stat Value when loading character data in CharacterInfo!", null, null, false, false);
											return;
										}
										bool removeOnDeath = savedStat.GetAttributeBool("removeondeath", true);
										this.ChangeSavedStatValue(statType, value, statIdentifier, removeOnDeath, float.MaxValue, false);
									}
								}
							}
							continue;
						}
					}
					if (elementName == "talents")
					{
						Version version = subElement.GetAttributeVersion("version", GameMain.Version);
						foreach (ContentXElement cxe2 in subElement.Elements())
						{
							XElement talentElement = cxe2;
							identifier = talentElement.Name.ToIdentifier<XName>();
							if (!(identifier != "talent"))
							{
								Identifier talentIdentifier = talentElement.GetAttributeIdentifier("identifier", Identifier.Empty);
								if (!(talentIdentifier == Identifier.Empty))
								{
									TalentPrefab prefab;
									if (TalentPrefab.TalentPrefabs.TryGet(talentIdentifier, out prefab))
									{
										foreach (TalentMigration migration in prefab.Migrations)
										{
											migration.TryApply(version, this);
										}
									}
									this.UnlockedTalents.Add(talentIdentifier);
									if (talentElement.GetAttributeBool("resettable", false))
									{
										this.ResettableExtraTalents.Add(talentIdentifier);
									}
								}
							}
						}
					}
				}
			}
			this.TalentRefundPoints = infoElement.GetAttributeInt("refundpoints", 0);
			int loadedLastRewardDistribution = infoElement.GetAttributeInt("lastrewarddistribution", -1);
			if (loadedLastRewardDistribution >= 0)
			{
				this.LastRewardDistribution = Option.Some<int>(loadedLastRewardDistribution);
			}
			this.LoadHeadAttachments();
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x0003A868 File Offset: 0x00038A68
		private void TryLoadNameAndTitle(Identifier npcIdentifier)
		{
			if (!npcIdentifier.IsEmpty)
			{
				this.Title = TextManager.Get("npctitle." + npcIdentifier.ToString());
				string nameTag = "charactername." + npcIdentifier.ToString();
				if (TextManager.ContainsTag(nameTag))
				{
					this.Name = TextManager.Get(nameTag).Value;
				}
			}
		}

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x0600069E RID: 1694 RVA: 0x0003A8D1 File Offset: 0x00038AD1
		public IReadOnlyList<ContentXElement> Hairs
		{
			get
			{
				return this.hairs;
			}
		}

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x0600069F RID: 1695 RVA: 0x0003A8D9 File Offset: 0x00038AD9
		public IReadOnlyList<ContentXElement> Beards
		{
			get
			{
				return this.beards;
			}
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x060006A0 RID: 1696 RVA: 0x0003A8E1 File Offset: 0x00038AE1
		public IReadOnlyList<ContentXElement> Moustaches
		{
			get
			{
				return this.moustaches;
			}
		}

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x060006A1 RID: 1697 RVA: 0x0003A8E9 File Offset: 0x00038AE9
		public IReadOnlyList<ContentXElement> FaceAttachments
		{
			get
			{
				return this.faceAttachments;
			}
		}

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x060006A2 RID: 1698 RVA: 0x0003A8F4 File Offset: 0x00038AF4
		public IEnumerable<ContentXElement> Wearables
		{
			get
			{
				if (this.wearables == null)
				{
					ContentXElement attachments = this.CharacterConfigElement.GetChildElement("HeadAttachments");
					ContentXElement contentXElement = null;
					if (attachments != contentXElement)
					{
						this.wearables = attachments.GetChildElements("Wearable");
					}
				}
				return this.wearables;
			}
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x0003A93E File Offset: 0x00038B3E
		public int GetIdentifier()
		{
			return this.GetIdentifierHash(this.Name);
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x0003A94C File Offset: 0x00038B4C
		public int GetIdentifierUsingOriginalName()
		{
			return this.GetIdentifierHash(this.OriginalName);
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x0003A95C File Offset: 0x00038B5C
		private int GetIdentifierHash(string name)
		{
			int id = ToolBox.StringToInt(name + string.Join<Identifier>("", from s in this.Head.Preset.TagSet
			orderby s
			select s));
			id ^= this.Head.HairIndex << 12;
			id ^= this.Head.BeardIndex << 18;
			id ^= this.Head.MoustacheIndex << 24;
			id ^= this.Head.FaceAttachmentIndex << 30;
			if (this.Job != null)
			{
				id ^= ToolBox.StringToInt(this.Job.Prefab.Identifier.Value);
			}
			return id;
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x0003AA20 File Offset: 0x00038C20
		public IEnumerable<ContentXElement> FilterElements(IEnumerable<ContentXElement> elements, ImmutableHashSet<Identifier> tags, WearableType? targetType = null)
		{
			if (elements == null)
			{
				return null;
			}
			return elements.Where(delegate(ContentXElement w)
			{
				WearableType type;
				if (targetType != null && Enum.TryParse<WearableType>(w.GetAttributeString("type", ""), true, out type))
				{
					WearableType wearableType = type;
					WearableType? targetType2 = targetType;
					if (!(wearableType == targetType2.GetValueOrDefault() & targetType2 != null))
					{
						return false;
					}
				}
				HashSet<Identifier> t = w.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
				CharacterInfo.LoadTagsBackwardsCompatibility(w, t);
				return t.IsSubsetOf(tags);
			});
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x0003AA58 File Offset: 0x00038C58
		public void RecreateHead(ImmutableHashSet<Identifier> tags, int hairIndex, int beardIndex, int moustacheIndex, int faceAttachmentIndex)
		{
			CharacterInfo.HeadPreset headPreset = this.Prefab.Heads.FirstOrDefault((CharacterInfo.HeadPreset h) => h.TagSet.SetEquals(tags));
			if (headPreset == null)
			{
				if (tags.Count == 1)
				{
					headPreset = this.Prefab.Heads.FirstOrDefault((CharacterInfo.HeadPreset h) => h.TagSet.Contains(tags.First<Identifier>()));
				}
				if (headPreset == null)
				{
					headPreset = this.Prefab.Heads.GetRandomUnsynced<CharacterInfo.HeadPreset>();
				}
			}
			this.head = new CharacterInfo.HeadInfo(this, headPreset, hairIndex, beardIndex, moustacheIndex, faceAttachmentIndex);
			this.ReloadHeadAttachments();
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x0003AAEF File Offset: 0x00038CEF
		public string ReplaceVars(string str)
		{
			if (this.Head == null)
			{
				return str;
			}
			return this.Prefab.ReplaceVars(str, this.Head.Preset);
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x0003AB14 File Offset: 0x00038D14
		public void RecreateHead(MultiplayerPreferences characterSettings)
		{
			if (characterSettings.HairIndex == -1 && characterSettings.BeardIndex == -1 && characterSettings.MoustacheIndex == -1 && characterSettings.FaceAttachmentIndex == -1)
			{
				this.SetAttachments(Rand.RandSync.Unsynced);
				characterSettings.HairIndex = this.Head.HairIndex;
				characterSettings.BeardIndex = this.Head.BeardIndex;
				characterSettings.MoustacheIndex = this.Head.MoustacheIndex;
				characterSettings.FaceAttachmentIndex = this.Head.FaceAttachmentIndex;
			}
			this.RecreateHead(characterSettings.TagSet.ToImmutableHashSet<Identifier>(), characterSettings.HairIndex, characterSettings.BeardIndex, characterSettings.MoustacheIndex, characterSettings.FaceAttachmentIndex);
			this.Head.SkinColor = CharacterInfo.<RecreateHead>g__ChooseColor|206_0(this.SkinColors, characterSettings.SkinColor);
			this.Head.HairColor = CharacterInfo.<RecreateHead>g__ChooseColor|206_0(this.HairColors, characterSettings.HairColor);
			this.Head.FacialHairColor = CharacterInfo.<RecreateHead>g__ChooseColor|206_0(this.FacialHairColors, characterSettings.FacialHairColor);
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x0003AC10 File Offset: 0x00038E10
		public void RecreateHead(CharacterInfo.HeadInfo headInfo)
		{
			this.RecreateHead(headInfo.Preset.TagSet, headInfo.HairIndex, headInfo.BeardIndex, headInfo.MoustacheIndex, headInfo.FaceAttachmentIndex);
			this.Head.SkinColor = headInfo.SkinColor;
			this.Head.HairColor = headInfo.HairColor;
			this.Head.FacialHairColor = headInfo.FacialHairColor;
			this.CheckColors();
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x0003AC7F File Offset: 0x00038E7F
		public void RefreshHead()
		{
			this.ReloadHeadAttachments();
			this.RefreshHeadSprites();
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x0003AC90 File Offset: 0x00038E90
		private void LoadHeadSpriteProjectSpecific(ContentXElement limbElement)
		{
			ContentXElement maskElement = limbElement.GetChildElement("tintmask");
			ContentXElement contentXElement = null;
			if (maskElement != contentXElement)
			{
				ContentPath tintMaskPath = maskElement.GetAttributeContentPath("texture");
				if (!tintMaskPath.IsNullOrEmpty())
				{
					this.VerifySpriteTagsLoaded();
					this.tintMask = new Sprite(maskElement, "", Limb.GetSpritePath(tintMaskPath, this), false, 1f);
					this.tintHighlightThreshold = maskElement.GetAttributeFloat("highlightthreshold", 0.6f);
					this.tintHighlightMultiplier = maskElement.GetAttributeFloat("highlightmultiplier", 0.8f);
				}
			}
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x0003AD1A File Offset: 0x00038F1A
		public void VerifySpriteTagsLoaded()
		{
			if (!this.spriteTagsLoaded)
			{
				this.LoadSpriteTags();
			}
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x0003AD2A File Offset: 0x00038F2A
		private void LoadHeadSprite()
		{
			this.LoadHeadElement(true, true);
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x0003AD34 File Offset: 0x00038F34
		private void LoadSpriteTags()
		{
			this.LoadHeadElement(false, true);
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x0003AD40 File Offset: 0x00038F40
		private void LoadHeadElement(bool loadHeadSprite, bool loadHeadSpriteTags)
		{
			RagdollParams ragdollParams = this.Ragdoll;
			ContentXElement contentXElement = (ragdollParams != null) ? ragdollParams.MainElement : null;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				return;
			}
			foreach (ContentXElement limbElement in this.Ragdoll.MainElement.Elements())
			{
				if (limbElement.GetAttributeString("type", string.Empty).Equals("head", StringComparison.OrdinalIgnoreCase))
				{
					ContentXElement spriteElement = limbElement.GetChildElement("sprite");
					contentXElement = null;
					if (!(spriteElement == contentXElement))
					{
						ContentPath attributeContentPath = spriteElement.GetAttributeContentPath("texture");
						string spritePath = (attributeContentPath != null) ? attributeContentPath.Value : null;
						if (!string.IsNullOrEmpty(spritePath))
						{
							spritePath = this.ReplaceVars(spritePath);
							string fileName = Path.GetFileNameWithoutExtension(spritePath);
							if (!string.IsNullOrEmpty(fileName))
							{
								foreach (string file in Directory.GetFiles(Path.GetDirectoryName(spritePath)))
								{
									if (file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
									{
										string fileWithoutTags = Path.GetFileNameWithoutExtension(file);
										fileWithoutTags = fileWithoutTags.Split(new char[]
										{
											'[',
											']'
										}).First<string>();
										if (!(fileWithoutTags != fileName))
										{
											if (loadHeadSprite)
											{
												this.HeadSprite = new Sprite(spriteElement, "", file, false, 1f);
												this.Portrait = new Sprite(spriteElement, "", file, false, 1f)
												{
													RelativeOrigin = Vector2.Zero
												};
											}
											if (loadHeadSpriteTags)
											{
												this.SpriteTags = (from id in file.Split(new char[]
												{
													'[',
													']'
												}).Skip(1)
												select id.ToIdentifier()).ToList<Identifier>();
												if (this.SpriteTags.Any<Identifier>())
												{
													this.SpriteTags.RemoveAt(this.SpriteTags.Count - 1);
												}
												this.spriteTagsLoaded = true;
												break;
											}
											break;
										}
									}
								}
								if (loadHeadSprite)
								{
									this.LoadHeadSpriteProjectSpecific(limbElement);
									break;
								}
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x060006B1 RID: 1713 RVA: 0x0003AF88 File Offset: 0x00039188
		public void LoadHeadAttachments()
		{
			if (this.Wearables != null)
			{
				if (this.hairs == null)
				{
					float commonness = 0.1f;
					this.hairs = CharacterInfo.AddEmpty(this.FilterElements(this.wearables, this.head.Preset.TagSet, new WearableType?(WearableType.Hair)), WearableType.Hair, commonness);
				}
				if (this.beards == null)
				{
					this.beards = CharacterInfo.AddEmpty(this.FilterElements(this.wearables, this.head.Preset.TagSet, new WearableType?(WearableType.Beard)), WearableType.Beard, 1f);
				}
				if (this.moustaches == null)
				{
					this.moustaches = CharacterInfo.AddEmpty(this.FilterElements(this.wearables, this.head.Preset.TagSet, new WearableType?(WearableType.Moustache)), WearableType.Moustache, 1f);
				}
				if (this.faceAttachments == null)
				{
					this.faceAttachments = CharacterInfo.AddEmpty(this.FilterElements(this.wearables, this.head.Preset.TagSet, new WearableType?(WearableType.FaceAttachment)), WearableType.FaceAttachment, 1f);
				}
			}
		}

		// Token: 0x060006B2 RID: 1714 RVA: 0x0003B090 File Offset: 0x00039290
		public static List<ContentXElement> AddEmpty(IEnumerable<ContentXElement> elements, WearableType type, float commonness = 1f)
		{
			ContentXElement emptyElement = new XElement("EmptyWearable", new object[]
			{
				type.ToString(),
				new XAttribute("commonness", commonness)
			}).FromPackage(null);
			List<ContentXElement> list = new List<ContentXElement>
			{
				emptyElement
			};
			list.AddRange(elements);
			return list;
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x0003B0F8 File Offset: 0x000392F8
		public ContentXElement GetRandomElement(IEnumerable<ContentXElement> elements)
		{
			IEnumerable<ContentXElement> filtered = elements.Where(new Func<ContentXElement, bool>(this.IsWearableAllowed));
			if (filtered.Count<ContentXElement>() == 0)
			{
				return null;
			}
			ContentXElement element = ToolBox.SelectWeightedRandom<ContentXElement>(filtered.ToList<ContentXElement>(), CharacterInfo.GetWeights(filtered).ToList<float>(), Rand.RandSync.Unsynced);
			ContentXElement contentXElement = null;
			if (!(element == contentXElement))
			{
				Identifier identifier = element.NameAsIdentifier();
				if (!(identifier == "Empty"))
				{
					return element;
				}
			}
			return null;
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x0003B160 File Offset: 0x00039360
		private bool IsWearableAllowed(ContentXElement element)
		{
			string spriteName = element.GetChildElement("sprite").GetAttributeString("name", string.Empty);
			return this.IsAllowed(this.Head.HairElement, spriteName) && this.IsAllowed(this.Head.BeardElement, spriteName) && this.IsAllowed(this.Head.MoustacheElement, spriteName) && this.IsAllowed(this.Head.FaceAttachment, spriteName);
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x0003B1EC File Offset: 0x000393EC
		private bool IsAllowed(XElement element, string spriteName)
		{
			if (element != null)
			{
				string[] disallowed = element.GetAttributeStringArray("disallow", Array.Empty<string>(), true, false);
				if (disallowed.Any((string s) => spriteName.Contains(s)))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x0003B233 File Offset: 0x00039433
		public static bool IsValidIndex(int index, List<ContentXElement> list)
		{
			return index >= 0 && index < list.Count;
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x0003B244 File Offset: 0x00039444
		private static IEnumerable<float> GetWeights(IEnumerable<ContentXElement> elements)
		{
			return from h in elements
			select h.GetAttributeFloat("commonness", 1f);
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x0003B26C File Offset: 0x0003946C
		private void LoadAttachmentSprites()
		{
			if (this.attachmentSprites == null)
			{
				this.attachmentSprites = new List<WearableSprite>();
			}
			if (!this.IsAttachmentsLoaded)
			{
				this.LoadHeadAttachments();
			}
			ContentXElement faceAttachment = this.Head.FaceAttachment;
			if (faceAttachment != null)
			{
				faceAttachment.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					this.attachmentSprites.Add(new WearableSprite(s, WearableType.FaceAttachment));
				});
			}
			ContentXElement beardElement = this.Head.BeardElement;
			if (beardElement != null)
			{
				beardElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					this.attachmentSprites.Add(new WearableSprite(s, WearableType.Beard));
				});
			}
			ContentXElement moustacheElement = this.Head.MoustacheElement;
			if (moustacheElement != null)
			{
				moustacheElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					this.attachmentSprites.Add(new WearableSprite(s, WearableType.Moustache));
				});
			}
			ContentXElement hairElement = this.Head.HairElement;
			if (hairElement == null)
			{
				return;
			}
			hairElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
			{
				this.attachmentSprites.Add(new WearableSprite(s, WearableType.Hair));
			});
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x0003B34C File Offset: 0x0003954C
		public int CalculateSalary(int baseSalary = 0, float salaryMultiplier = 1f)
		{
			if (this.Name == null || this.Job == null)
			{
				return 0;
			}
			int salary = 0;
			foreach (Skill skill in this.Job.GetSkills())
			{
				salary += (int)(skill.Level * skill.PriceMultiplier);
			}
			return (int)((float)baseSalary + (float)salary * this.Job.Prefab.PriceMultiplier * salaryMultiplier);
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x0003B3D8 File Offset: 0x000395D8
		public void ApplySkillGain(Identifier skillIdentifier, float baseGain, bool gainedFromAbility = false, float maxGain = 2f, bool forceNotification = false)
		{
			float skillLevel = this.Job.GetSkillLevel(skillIdentifier);
			float skillDivider = MathF.Pow(Math.Max(skillLevel, 15f), SkillSettings.Current.SkillIncreaseExponent);
			this.IncreaseSkillLevel(skillIdentifier, Math.Min(baseGain / skillDivider, maxGain), gainedFromAbility, forceNotification);
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x0003B424 File Offset: 0x00039624
		public void IncreaseSkillLevel(Identifier skillIdentifier, float increase, bool gainedFromAbility = false, bool forceNotification = false)
		{
			if (this.Job == null || (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient) || this.Character == null)
			{
				return;
			}
			if (this.Job.Prefab.Identifier == "assistant")
			{
				increase *= SkillSettings.Current.AssistantSkillIncreaseMultiplier;
			}
			increase *= 1f + this.Character.GetStatValue(StatTypes.SkillGainSpeed, true);
			increase = this.GetSkillSpecificGain(increase, skillIdentifier);
			float prevLevel = this.Job.GetSkillLevel(skillIdentifier);
			this.Job.IncreaseSkillLevel(skillIdentifier, increase, this.Character.HasAbilityFlag(AbilityFlags.GainSkillPastMaximum));
			float newLevel = this.Job.GetSkillLevel(skillIdentifier);
			if ((int)newLevel > (int)prevLevel)
			{
				float extraLevel = this.Character.GetStatValue(StatTypes.ExtraLevelGain, true);
				this.Job.IncreaseSkillLevel(skillIdentifier, extraLevel, this.Character.HasAbilityFlag(AbilityFlags.GainSkillPastMaximum));
				float increaseSinceLastSkillPoint = MathHelper.Max(increase, 1f);
				AbilitySkillGain abilitySkillGain = new AbilitySkillGain(increaseSinceLastSkillPoint, skillIdentifier, this.Character, gainedFromAbility);
				this.Character.CheckTalents(AbilityEffectType.OnGainSkillPoint, abilitySkillGain);
				foreach (Character character in Character.GetFriendlyCrew(this.Character))
				{
					character.CheckTalents(AbilityEffectType.OnAllyGainSkillPoint, abilitySkillGain);
				}
			}
			this.OnSkillChanged(skillIdentifier, prevLevel, newLevel, forceNotification);
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x0003B590 File Offset: 0x00039790
		private float GetSkillSpecificGain(float increase, Identifier skillIdentifier)
		{
			StatTypes statType;
			if (CharacterInfo.skillGainStatValues.TryGetValue(skillIdentifier, out statType))
			{
				increase *= 1f + this.Character.GetStatValue(statType, true);
			}
			return increase;
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x0003B5C4 File Offset: 0x000397C4
		public void SetSkillLevel(Identifier skillIdentifier, float level, bool forceNotification = false)
		{
			if (this.Job == null)
			{
				return;
			}
			Skill skill = this.Job.GetSkill(skillIdentifier);
			if (skill == null)
			{
				this.Job.IncreaseSkillLevel(skillIdentifier, level, false);
				this.OnSkillChanged(skillIdentifier, 0f, level, forceNotification);
				return;
			}
			float prevLevel = skill.Level;
			skill.Level = level;
			this.OnSkillChanged(skillIdentifier, prevLevel, skill.Level, forceNotification);
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x0003B624 File Offset: 0x00039824
		private void OnSkillChanged(Identifier skillIdentifier, float prevLevel, float newLevel, bool forceNotification)
		{
			if (this.TeamID == CharacterTeamType.FriendlyNPC)
			{
				return;
			}
			if (Character.Controlled != null && Character.Controlled.TeamID != this.TeamID)
			{
				return;
			}
			bool specialIncrease = Math.Abs(newLevel - prevLevel) >= 1f;
			if ((int)newLevel <= (int)prevLevel)
			{
				if (forceNotification)
				{
					float change = newLevel - prevLevel;
					if (Math.Abs(change) > 0.01f)
					{
						string sign = (change > 0f) ? "+" : "-";
						Character character = this.Character;
						if (character == null)
						{
							return;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 3);
						defaultInterpolatedStringHandler.AppendFormatted(sign);
						defaultInterpolatedStringHandler.AppendFormatted<double>(Math.Round((double)change, 2));
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted(TextManager.Get("SkillName." + skillIdentifier.ToString()).Value);
						character.AddMessage(defaultInterpolatedStringHandler.ToStringAndClear(), specialIncrease ? GUIStyle.Orange : GUIStyle.Green, this.Character == Character.Controlled, default(Identifier), null, 3f);
					}
				}
				return;
			}
			Character controlled = Character.Controlled;
			if (controlled != null)
			{
				Item selectedItem = controlled.SelectedItem;
				if (selectedItem != null)
				{
					selectedItem.OnPlayerSkillsChanged();
				}
			}
			int increase = Math.Max((int)newLevel - (int)prevLevel, 1);
			Character character2 = this.Character;
			if (character2 == null)
			{
				return;
			}
			character2.AddMessage("+[value] " + TextManager.Get("SkillName." + skillIdentifier.ToString()).Value, specialIncrease ? GUIStyle.Orange : GUIStyle.Green, this.Character == Character.Controlled, skillIdentifier, new int?(increase), 3f);
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x0003B7D8 File Offset: 0x000399D8
		public void GiveExperience(int amount)
		{
			int prevAmount = this.ExperiencePoints;
			AbilityExperienceGainMultiplier experienceGainMultiplier = new AbilityExperienceGainMultiplier(1f);
			AbilityExperienceGainMultiplier abilityExperienceGainMultiplier = experienceGainMultiplier;
			float value = abilityExperienceGainMultiplier.Value;
			Character character = this.Character;
			abilityExperienceGainMultiplier.Value = value + ((character != null) ? character.GetStatValue(StatTypes.ExperienceGainMultiplier, true) : 0f);
			amount = (int)((float)amount * experienceGainMultiplier.Value);
			if (amount < 0)
			{
				return;
			}
			this.ExperiencePoints += amount;
			this.OnExperienceChanged(prevAmount, this.ExperiencePoints);
		}

		// Token: 0x060006C0 RID: 1728 RVA: 0x0003B84C File Offset: 0x00039A4C
		public void SetExperience(int newExperience)
		{
			if (newExperience < 0)
			{
				return;
			}
			int prevAmount = this.ExperiencePoints;
			this.ExperiencePoints = newExperience;
			this.OnExperienceChanged(prevAmount, this.ExperiencePoints);
		}

		// Token: 0x060006C1 RID: 1729 RVA: 0x0003B879 File Offset: 0x00039A79
		public int GetTotalTalentPoints()
		{
			return this.GetCurrentLevel() + this.AdditionalTalentPoints;
		}

		// Token: 0x060006C2 RID: 1730 RVA: 0x0003B888 File Offset: 0x00039A88
		public int GetAvailableTalentPoints()
		{
			return Math.Max(this.GetTotalTalentPoints() - this.GetUnlockedTalentsInTree().Count<Identifier>(), 0);
		}

		// Token: 0x060006C3 RID: 1731 RVA: 0x0003B8A2 File Offset: 0x00039AA2
		public float GetProgressTowardsNextLevel()
		{
			return (float)(this.ExperiencePoints - this.GetExperienceRequiredForCurrentLevel()) / (float)(this.GetExperienceRequiredToLevelUp() - this.GetExperienceRequiredForCurrentLevel());
		}

		// Token: 0x060006C4 RID: 1732 RVA: 0x0003B8C4 File Offset: 0x00039AC4
		public int GetExperienceRequiredForCurrentLevel()
		{
			int experienceRequired;
			this.GetCurrentLevel(out experienceRequired);
			return experienceRequired;
		}

		// Token: 0x060006C5 RID: 1733 RVA: 0x0003B8DC File Offset: 0x00039ADC
		public int GetExperienceRequiredToLevelUp()
		{
			int experienceRequired;
			int level = this.GetCurrentLevel(out experienceRequired);
			return experienceRequired + CharacterInfo.ExperienceRequiredPerLevel(level);
		}

		// Token: 0x060006C6 RID: 1734 RVA: 0x0003B8FC File Offset: 0x00039AFC
		public int GetExperienceRequiredForLevel(int level)
		{
			int currentLevel = this.GetCurrentLevel();
			if (currentLevel >= level)
			{
				return 0;
			}
			int required = 0;
			for (int i = 0; i < level; i++)
			{
				required += CharacterInfo.ExperienceRequiredPerLevel(i);
			}
			return required - this.ExperiencePoints;
		}

		// Token: 0x060006C7 RID: 1735 RVA: 0x0003B938 File Offset: 0x00039B38
		public int GetCurrentLevel()
		{
			int num;
			return this.GetCurrentLevel(out num);
		}

		// Token: 0x060006C8 RID: 1736 RVA: 0x0003B950 File Offset: 0x00039B50
		private int GetCurrentLevel(out int experienceRequired)
		{
			int level = 0;
			experienceRequired = 0;
			while (experienceRequired + CharacterInfo.ExperienceRequiredPerLevel(level) <= this.ExperiencePoints)
			{
				experienceRequired += CharacterInfo.ExperienceRequiredPerLevel(level);
				level++;
			}
			return Math.Max(level, 0);
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x0003B98B File Offset: 0x00039B8B
		public static int ExperienceRequiredPerLevel(int level)
		{
			return 450 + 500 * level;
		}

		// Token: 0x060006CA RID: 1738 RVA: 0x0003B99C File Offset: 0x00039B9C
		private void OnExperienceChanged(int prevAmount, int newAmount)
		{
			if (Character.Controlled != null && Character.Controlled.TeamID != this.TeamID)
			{
				return;
			}
			TabMenu tabMenuInstance = GameSession.TabMenuInstance;
			if (tabMenuInstance != null)
			{
				tabMenuInstance.OnExperienceChanged(this.Character);
			}
			if (newAmount > prevAmount)
			{
				int increase = newAmount - prevAmount;
				Character character = this.Character;
				if (character == null)
				{
					return;
				}
				character.AddMessage("+[value] " + TextManager.Get("experienceshort").Value, GUIStyle.Blue, this.Character == Character.Controlled, "exp".ToIdentifier(), new int?(increase), 3f);
			}
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x0003BA38 File Offset: 0x00039C38
		public void RefundTalents()
		{
			if (this.TalentRefundPoints <= 0)
			{
				return;
			}
			List<Identifier> talentsFromOutsideTree = this.GetUnlockedTalentsOutsideTree().ToList<Identifier>();
			foreach (Identifier resettableExtraTalent in this.ResettableExtraTalents)
			{
				talentsFromOutsideTree.Remove(resettableExtraTalent);
			}
			this.UnlockedTalents.Clear();
			this.SavedStatValues.Clear();
			Character character = this.Character;
			if (character != null)
			{
				character.ResetTalents(this.talentResetCount);
			}
			int num = this.TalentRefundPoints;
			this.TalentRefundPoints = num - 1;
			this.talentResetCount++;
			if (this.Character == null)
			{
				talentsFromOutsideTree.ForEach(delegate(Identifier talentId)
				{
					this.UnlockedTalents.Add(talentId);
				});
			}
			else
			{
				talentsFromOutsideTree.ForEach(delegate(Identifier talentId)
				{
					this.Character.GiveTalent(talentId, true);
				});
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null)
			{
				return;
			}
			networkMember.CreateEntityEvent(this.Character, default(Character.ConfirmRefundEventData));
		}

		// Token: 0x060006CC RID: 1740 RVA: 0x0003BB40 File Offset: 0x00039D40
		public void AddRefundPoints(int newRefundPoints)
		{
			this.TalentRefundPoints += newRefundPoints;
			this.ShowTalentResetPopupOnOpen = true;
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x0003BB58 File Offset: 0x00039D58
		public void Rename(string newName)
		{
			if (string.IsNullOrEmpty(newName))
			{
				return;
			}
			newName = Client.SanitizeName(newName, 32);
			foreach (Item item in Item.ItemList)
			{
				if (item.HasTag("identitycard".ToIdentifier()) || item.HasTag("despawncontainer".ToIdentifier()))
				{
					string[] array = item.Tags.Split(',', StringSplitOptions.None);
					int i = 0;
					while (i < array.Length)
					{
						string tag = array[i];
						string[] splitTag = tag.Split(":", StringSplitOptions.None);
						if (splitTag.Length >= 2 && !(splitTag[0] != "name") && !(splitTag[1] != this.Name))
						{
							item.ReplaceTag(tag, "name:" + newName);
							IdCard idCard = item.GetComponent<IdCard>();
							if (idCard != null)
							{
								idCard.OwnerName = newName;
								break;
							}
							break;
						}
						else
						{
							i++;
						}
					}
				}
			}
			this.Name = newName;
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x0003BC6C File Offset: 0x00039E6C
		public void ResetName()
		{
			this.Name = this.OriginalName;
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x0003BC7C File Offset: 0x00039E7C
		public XElement Save(XElement parentElement)
		{
			XElement charElement = new XElement("Character");
			XContainer xcontainer = charElement;
			object[] array = new object[22];
			array[0] = new XAttribute("name", this.Name);
			array[1] = new XAttribute("originalname", this.OriginalName);
			array[2] = new XAttribute("speciesname", this.SpeciesName);
			array[3] = new XAttribute("tags", string.Join<Identifier>(",", this.Head.Preset.TagSet));
			array[4] = new XAttribute("salary", this.Salary);
			array[5] = new XAttribute("experiencepoints", this.ExperiencePoints);
			array[6] = new XAttribute("additionaltalentpoints", this.AdditionalTalentPoints);
			array[7] = new XAttribute("talentResetCount", this.TalentResetCount);
			array[8] = new XAttribute("hairindex", this.Head.HairIndex);
			array[9] = new XAttribute("beardindex", this.Head.BeardIndex);
			array[10] = new XAttribute("moustacheindex", this.Head.MoustacheIndex);
			array[11] = new XAttribute("faceattachmentindex", this.Head.FaceAttachmentIndex);
			array[12] = new XAttribute("skincolor", XMLExtensions.ColorToString(this.Head.SkinColor));
			array[13] = new XAttribute("haircolor", XMLExtensions.ColorToString(this.Head.HairColor));
			array[14] = new XAttribute("facialhaircolor", XMLExtensions.ColorToString(this.Head.FacialHairColor));
			array[15] = new XAttribute("startitemsgiven", this.StartItemsGiven);
			int num = 16;
			XName name = "personality";
			NPCPersonalityTrait personalityTrait = this.PersonalityTrait;
			array[num] = new XAttribute(name, (personalityTrait != null) ? personalityTrait.Identifier : Identifier.Empty);
			array[17] = new XAttribute("refundpoints", this.TalentRefundPoints);
			array[18] = new XAttribute("lastrewarddistribution", this.LastRewardDistribution.Match((int value) => value, () => -1).ToString());
			array[19] = new XAttribute("permanentlydead", this.PermanentlyDead);
			array[20] = new XAttribute("IsOnReserveBench", this.IsOnReserveBench);
			array[21] = new XAttribute("renamingenabled", this.RenamingEnabled);
			xcontainer.Add(array);
			ValueTuple<Identifier, Identifier> humanPrefabIds = this.HumanPrefabIds;
			Identifier identifier = default(Identifier);
			if (!(humanPrefabIds.Item1 != identifier))
			{
				Identifier identifier2 = default(Identifier);
				if (!(humanPrefabIds.Item2 != identifier2))
				{
					goto IL_3AC;
				}
			}
			charElement.Add(new object[]
			{
				new XAttribute("npcsetid", this.HumanPrefabIds.Item1),
				new XAttribute("npcid", this.HumanPrefabIds.Item2)
			});
			IL_3AC:
			charElement.Add(new XAttribute("missionscompletedsincedeath", this.MissionsCompletedSinceDeath));
			if (!this.MinReputationToHire.Item1.IsEmpty)
			{
				charElement.Add(new object[]
				{
					new XAttribute("factionId", this.MinReputationToHire.Item1),
					new XAttribute("minreputation", this.MinReputationToHire.Item2)
				});
			}
			if (this.Character != null && this.Character.AnimController.CurrentHull != null)
			{
				charElement.Add(new XAttribute("hull", this.Character.AnimController.CurrentHull.ID));
			}
			this.Job.Save(charElement);
			XElement savedStatElement = new XElement("savedstatvalues");
			foreach (KeyValuePair<StatTypes, List<SavedStatValue>> statValuePair in this.SavedStatValues)
			{
				foreach (SavedStatValue savedStat in statValuePair.Value)
				{
					if (savedStat.StatValue != 0f)
					{
						savedStatElement.Add(new XElement("savedstatvalue", new object[]
						{
							new XAttribute("stattype", statValuePair.Key.ToString()),
							new XAttribute("statidentifier", savedStat.StatIdentifier),
							new XAttribute("statvalue", savedStat.StatValue),
							new XAttribute("removeondeath", savedStat.RemoveOnDeath)
						}));
					}
				}
			}
			XElement talentElement = new XElement("Talents");
			talentElement.Add(new XAttribute("version", GameMain.Version.ToString()));
			foreach (Identifier talentIdentifier in this.UnlockedTalents)
			{
				talentElement.Add(new XElement("Talent", new object[]
				{
					new XAttribute("identifier", talentIdentifier),
					new XAttribute("resettable", this.ResettableExtraTalents.Contains(talentIdentifier))
				}));
			}
			charElement.Add(savedStatElement);
			charElement.Add(talentElement);
			if (parentElement != null)
			{
				parentElement.Add(charElement);
			}
			return charElement;
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x0003C358 File Offset: 0x0003A558
		public static void SaveOrders(XElement parentElement, params Order[] orders)
		{
			if (parentElement == null || orders == null || orders.None(null))
			{
				return;
			}
			int priorityIncrease = 0;
			List<LinkedSubmarine> linkedSubs = CharacterInfo.GetLinkedSubmarines();
			int j = 0;
			while (j < orders.Length)
			{
				Order orderInfo = orders[j];
				Order order = orderInfo;
				if (order == null)
				{
					goto IL_45;
				}
				Identifier identifier = order.Identifier;
				if (identifier == Identifier.Empty)
				{
					goto IL_45;
				}
				int? linkedSubIndex = null;
				bool targetAvailableInNextLevel = true;
				if (order.TargetSpatialEntity != null)
				{
					Submarine entitySub = order.TargetSpatialEntity.Submarine;
					bool isOutside = entitySub == null;
					bool canBeOnLinkedSub = !isOutside && Submarine.MainSub != null && entitySub != Submarine.MainSub && linkedSubs.Any<LinkedSubmarine>();
					bool isOnConnectedLinkedSub = false;
					if (canBeOnLinkedSub)
					{
						for (int i = 0; i < linkedSubs.Count; i++)
						{
							LinkedSubmarine ls = linkedSubs[i];
							if (ls.LoadSub && ls.Sub == entitySub)
							{
								linkedSubIndex = new int?(i);
								isOnConnectedLinkedSub = Submarine.MainSub.GetConnectedSubs().Contains(entitySub);
								break;
							}
						}
					}
					if (isOutside)
					{
						goto IL_13F;
					}
					GameSession gameSession = GameMain.GameSession;
					CampaignMode campaignMode = (gameSession != null) ? gameSession.Campaign : null;
					if (campaignMode != null && campaignMode.SwitchedSubsThisRound)
					{
						goto IL_13F;
					}
					bool flag = isOnConnectedLinkedSub || (Submarine.MainSub != null && entitySub == Submarine.MainSub);
					IL_140:
					targetAvailableInNextLevel = flag;
					if (targetAvailableInNextLevel)
					{
						goto IL_1DE;
					}
					if (!order.Prefab.CanBeGeneralized)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(155, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Trying to save an order (");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(order.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(") targeting an entity that won't be connected to the main sub in the next level. The order requires a target so it won't be saved.");
						DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
						priorityIncrease++;
						goto IL_507;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(147, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Saving an order (");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(order.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral(") targeting an entity that won't be connected to the main sub in the next level. The order will be saved as a generalized version.");
					DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
					goto IL_1DE;
					IL_13F:
					flag = false;
					goto IL_140;
				}
				IL_1DE:
				if (orderInfo.ManualPriority < 1)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(60, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("Error saving an order (");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(order.Identifier);
					defaultInterpolatedStringHandler3.AppendLiteral(") - the order priority is less than 1");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
					priorityIncrease++;
				}
				else
				{
					XElement orderElement = new XElement("order", new object[]
					{
						new XAttribute("id", order.Identifier),
						new XAttribute("priority", orderInfo.ManualPriority + priorityIncrease),
						new XAttribute("targettype", (int)order.TargetType)
					});
					if (orderInfo.Option != Identifier.Empty)
					{
						orderElement.Add(new XAttribute("option", orderInfo.Option));
					}
					if (order.OrderGiver != null)
					{
						XContainer xcontainer = orderElement;
						XName name = "ordergiver";
						CharacterInfo info = order.OrderGiver.Info;
						xcontainer.Add(new XAttribute(name, (info != null) ? new int?(info.GetIdentifier()) : null));
					}
					ISpatialEntity targetSpatialEntity = order.TargetSpatialEntity;
					Submarine targetSub = (targetSpatialEntity != null) ? targetSpatialEntity.Submarine : null;
					if (targetSub != null)
					{
						if (Submarine.MainSub != null && targetSub == Submarine.MainSub)
						{
							orderElement.Add(new XAttribute("onmainsub", true));
						}
						else if (linkedSubIndex != null)
						{
							orderElement.Add(new XAttribute("linkedsubindex", linkedSubIndex));
						}
					}
					switch (order.TargetType)
					{
					case Order.OrderTargetType.Entity:
						if (targetAvailableInNextLevel)
						{
							Entity e = order.TargetEntity;
							if (e != null)
							{
								orderElement.Add(new XAttribute("targetid", (uint)e.ID));
							}
						}
						break;
					case Order.OrderTargetType.Position:
						if (targetAvailableInNextLevel)
						{
							OrderTarget ot = order.TargetSpatialEntity as OrderTarget;
							if (ot != null)
							{
								XElement orderTargetElement = new XElement("ordertarget");
								Vector2 position = ot.WorldPosition;
								if (ot.Hull != null)
								{
									orderTargetElement.Add(new XAttribute("hullid", (uint)ot.Hull.ID));
									position -= ot.Hull.WorldPosition;
								}
								orderTargetElement.Add(new XAttribute("position", XMLExtensions.Vector2ToString(position)));
								orderElement.Add(orderTargetElement);
							}
						}
						break;
					case Order.OrderTargetType.WallSection:
						if (targetAvailableInNextLevel)
						{
							Structure s = order.TargetEntity as Structure;
							if (s != null && order.WallSectionIndex != null)
							{
								orderElement.Add(new XAttribute("structureid", s.ID));
								orderElement.Add(new XAttribute("wallsectionindex", order.WallSectionIndex.Value));
							}
						}
						break;
					}
					parentElement.Add(orderElement);
				}
				IL_507:
				j++;
				continue;
				IL_45:
				DebugConsole.ThrowError("Error saving an order - the order or its identifier is null", null, null, false, false);
				priorityIncrease++;
				goto IL_507;
			}
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x0003C87C File Offset: 0x0003AA7C
		public static void SaveOrderData(CharacterInfo characterInfo, XElement parentElement)
		{
			List<Order> currentOrders = new List<Order>(characterInfo.CurrentOrders);
			currentOrders.Sort((Order x, Order y) => y.ManualPriority.CompareTo(x.ManualPriority));
			CharacterInfo.SaveOrders(parentElement, currentOrders.ToArray());
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x0003C8C6 File Offset: 0x0003AAC6
		public void SaveOrderData()
		{
			this.OrderData = new XElement("orders");
			CharacterInfo.SaveOrderData(this, this.OrderData);
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x0003C8EC File Offset: 0x0003AAEC
		public static void ApplyOrderData(Character character, XElement orderData)
		{
			if (character == null)
			{
				return;
			}
			List<Order> orders = CharacterInfo.LoadOrders(orderData);
			foreach (Order order in orders)
			{
				character.SetOrder(order, true, false, true);
			}
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x0003C948 File Offset: 0x0003AB48
		public void ApplyOrderData()
		{
			CharacterInfo.ApplyOrderData(this.Character, this.OrderData);
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x0003C95C File Offset: 0x0003AB5C
		public static List<Order> LoadOrders(XElement ordersElement)
		{
			List<Order> orders = new List<Order>();
			if (ordersElement == null)
			{
				return orders;
			}
			CharacterInfo.<>c__DisplayClass255_0 CS$<>8__locals1;
			CS$<>8__locals1.priorityIncrease = 0;
			CS$<>8__locals1.linkedSubs = CharacterInfo.GetLinkedSubmarines();
			foreach (XElement orderElement in ordersElement.GetChildElements("order", StringComparison.OrdinalIgnoreCase))
			{
				CharacterInfo.<>c__DisplayClass255_1 CS$<>8__locals2;
				CS$<>8__locals2.orderElement = orderElement;
				Order order = null;
				CharacterInfo.<>c__DisplayClass255_2 CS$<>8__locals3;
				CS$<>8__locals3.orderIdentifier = CS$<>8__locals2.orderElement.GetAttributeString("id", "");
				if (!OrderPrefab.Prefabs.TryGet(CS$<>8__locals3.orderIdentifier, out CS$<>8__locals3.orderPrefab))
				{
					DebugConsole.ThrowError("Error loading a previously saved order - can't find an order prefab with the identifier \"" + CS$<>8__locals3.orderIdentifier + "\"", null, null, false, false);
					int priorityIncrease = CS$<>8__locals1.priorityIncrease;
					CS$<>8__locals1.priorityIncrease = priorityIncrease + 1;
				}
				else
				{
					Order.OrderTargetType targetType = (Order.OrderTargetType)CS$<>8__locals2.orderElement.GetAttributeInt("targettype", 0);
					Character orderGiver = null;
					XAttribute orderGiverIdAttribute = CS$<>8__locals2.orderElement.GetAttribute("ordergiver", StringComparison.OrdinalIgnoreCase);
					if (orderGiverIdAttribute != null)
					{
						int orderGiverInfoId = orderGiverIdAttribute.GetAttributeInt(0);
						orderGiver = Character.CharacterList.FirstOrDefault(delegate(Character c)
						{
							CharacterInfo info = c.Info;
							return info != null && info.GetIdentifier() == orderGiverInfoId;
						});
					}
					Entity targetEntity = null;
					switch (targetType)
					{
					case Order.OrderTargetType.Entity:
					{
						ushort targetId = (ushort)CS$<>8__locals2.orderElement.GetAttributeUInt("targetid", 0U);
						if (!CharacterInfo.<LoadOrders>g__GetTargetEntity|255_0(targetId, out targetEntity, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3))
						{
							continue;
						}
						ItemComponent targetComponent = CS$<>8__locals3.orderPrefab.GetTargetItemComponent(targetEntity as Item);
						order = new Order(CS$<>8__locals3.orderPrefab, targetEntity, targetComponent, orderGiver, false);
						break;
					}
					case Order.OrderTargetType.Position:
					{
						XElement orderTargetElement = CS$<>8__locals2.orderElement.GetChildElement("ordertarget", StringComparison.OrdinalIgnoreCase);
						Vector2 position = orderTargetElement.GetAttributeVector2("position", Vector2.Zero);
						ushort hullId = (ushort)orderTargetElement.GetAttributeUInt("hullid", 0U);
						if (!CharacterInfo.<LoadOrders>g__GetTargetEntity|255_0(hullId, out targetEntity, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3))
						{
							continue;
						}
						Hull targetPositionHull = targetEntity as Hull;
						if (targetPositionHull == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Error loading a previously saved order (");
							defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals3.orderIdentifier);
							defaultInterpolatedStringHandler.AppendLiteral(") - entity with the ID ");
							defaultInterpolatedStringHandler.AppendFormatted<ushort>(hullId);
							defaultInterpolatedStringHandler.AppendLiteral(" is of type ");
							defaultInterpolatedStringHandler.AppendFormatted<Type>((targetEntity != null) ? targetEntity.GetType() : null);
							defaultInterpolatedStringHandler.AppendLiteral(" instead of Hull");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
							int priorityIncrease = CS$<>8__locals1.priorityIncrease;
							CS$<>8__locals1.priorityIncrease = priorityIncrease + 1;
							continue;
						}
						OrderTarget orderTarget = new OrderTarget(targetPositionHull.WorldPosition + position, targetPositionHull, false);
						order = new Order(CS$<>8__locals3.orderPrefab, orderTarget, orderGiver);
						break;
					}
					case Order.OrderTargetType.WallSection:
					{
						ushort structureId = (ushort)CS$<>8__locals2.orderElement.GetAttributeInt("structureid", 0);
						if (!CharacterInfo.<LoadOrders>g__GetTargetEntity|255_0(structureId, out targetEntity, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3))
						{
							continue;
						}
						int wallSectionIndex = CS$<>8__locals2.orderElement.GetAttributeInt("wallsectionindex", 0);
						Structure targetStructure = targetEntity as Structure;
						if (targetStructure == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(96, 3);
							defaultInterpolatedStringHandler2.AppendLiteral("Error loading a previously saved order (");
							defaultInterpolatedStringHandler2.AppendFormatted(CS$<>8__locals3.orderIdentifier);
							defaultInterpolatedStringHandler2.AppendLiteral(") - entity with the ID ");
							defaultInterpolatedStringHandler2.AppendFormatted<ushort>(structureId);
							defaultInterpolatedStringHandler2.AppendLiteral(" is of type ");
							defaultInterpolatedStringHandler2.AppendFormatted<Type>((targetEntity != null) ? targetEntity.GetType() : null);
							defaultInterpolatedStringHandler2.AppendLiteral(" instead of Structure");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
							int priorityIncrease = CS$<>8__locals1.priorityIncrease;
							CS$<>8__locals1.priorityIncrease = priorityIncrease + 1;
							continue;
						}
						order = new Order(CS$<>8__locals3.orderPrefab, targetStructure, new int?(wallSectionIndex), orderGiver);
						break;
					}
					}
					Identifier orderOption = CS$<>8__locals2.orderElement.GetAttributeIdentifier("option", "");
					int manualPriority = CS$<>8__locals2.orderElement.GetAttributeInt("priority", 0) + CS$<>8__locals1.priorityIncrease;
					Order orderInfo = order.WithOption(orderOption).WithManualPriority(manualPriority);
					orders.Add(orderInfo);
				}
			}
			return orders;
		}

		// Token: 0x060006D6 RID: 1750 RVA: 0x0003CD70 File Offset: 0x0003AF70
		private static List<LinkedSubmarine> GetLinkedSubmarines()
		{
			return (from ls in Entity.GetEntities().OfType<LinkedSubmarine>()
			where ls.Submarine == Submarine.MainSub
			select ls into e
			orderby e.ID
			select e).ToList<LinkedSubmarine>();
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x0003CDD4 File Offset: 0x0003AFD4
		private static ushort GetOffsetId(Submarine parentSub, ushort id)
		{
			if (parentSub != null)
			{
				IdRemap idRemap = new IdRemap(parentSub.Info.SubmarineElement, (int)parentSub.IdOffset);
				return idRemap.GetOffsetId((int)id);
			}
			return id;
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x0003CE04 File Offset: 0x0003B004
		public static void ApplyHealthData(Character character, XElement healthData, Func<AfflictionPrefab, bool> afflictionPredicate = null)
		{
			if (healthData != null && character != null)
			{
				character.CharacterHealth.Load(healthData, afflictionPredicate);
			}
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x0003CE19 File Offset: 0x0003B019
		public void ReloadHeadAttachments()
		{
			this.ResetLoadedAttachments();
			this.LoadHeadAttachments();
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x0003CE27 File Offset: 0x0003B027
		private void ResetAttachmentIndices()
		{
			this.Head.ResetAttachmentIndices();
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x0003CE34 File Offset: 0x0003B034
		private void ResetLoadedAttachments()
		{
			this.hairs = null;
			this.beards = null;
			this.moustaches = null;
			this.faceAttachments = null;
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x0003CE52 File Offset: 0x0003B052
		public void ClearCurrentOrders()
		{
			this.CurrentOrders.Clear();
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x0003CE5F File Offset: 0x0003B05F
		public void Remove()
		{
			this.Character = null;
			this.HeadSprite = null;
			this.Portrait = null;
			this.AttachmentSprites = null;
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x0003CE7D File Offset: 0x0003B07D
		private void RefreshHeadSprites()
		{
			this._headSprite = null;
			this.LoadHeadSprite();
			this.CalculateHeadPosition(this._headSprite);
			List<WearableSprite> list = this.attachmentSprites;
			if (list != null)
			{
				list.Clear();
			}
			this.LoadAttachmentSprites();
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x0003CEB0 File Offset: 0x0003B0B0
		public void ClearSavedStatValues()
		{
			foreach (StatTypes statType in this.SavedStatValues.Keys)
			{
			}
			this.SavedStatValues.Clear();
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x0003CF0C File Offset: 0x0003B10C
		public void ClearSavedStatValues(StatTypes statType)
		{
			this.SavedStatValues.Remove(statType);
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x0003CF1C File Offset: 0x0003B11C
		public void RemoveSavedStatValuesOnDeath()
		{
			foreach (StatTypes statType in this.SavedStatValues.Keys)
			{
				foreach (SavedStatValue savedStatValue in this.SavedStatValues[statType])
				{
					if (savedStatValue.RemoveOnDeath && !MathUtils.NearlyEqual(savedStatValue.StatValue, 0f, 0.0001f))
					{
						savedStatValue.StatValue = 0f;
					}
				}
			}
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x0003CFD8 File Offset: 0x0003B1D8
		public void ResetSavedStatValue(Identifier statIdentifier)
		{
			foreach (StatTypes statType in this.SavedStatValues.Keys)
			{
				foreach (SavedStatValue savedStatValue in this.SavedStatValues[statType])
				{
					if (CharacterInfo.<ResetSavedStatValue>g__MatchesIdentifier|269_0(savedStatValue.StatIdentifier, statIdentifier) && !MathUtils.NearlyEqual(savedStatValue.StatValue, 0f, 0.0001f))
					{
						savedStatValue.StatValue = 0f;
					}
				}
			}
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x0003D0A4 File Offset: 0x0003B2A4
		public float GetSavedStatValue(StatTypes statType)
		{
			List<SavedStatValue> statValues;
			if (this.SavedStatValues.TryGetValue(statType, out statValues))
			{
				return statValues.Sum((SavedStatValue v) => v.StatValue);
			}
			return 0f;
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x0003D0EC File Offset: 0x0003B2EC
		public float GetSavedStatValue(StatTypes statType, Identifier statIdentifier)
		{
			List<SavedStatValue> statValues;
			if (this.SavedStatValues.TryGetValue(statType, out statValues))
			{
				return (from value in statValues
				where ToolBox.StatIdentifierMatches(value.StatIdentifier, statIdentifier)
				select value).Sum((SavedStatValue v) => v.StatValue);
			}
			return 0f;
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x0003D152 File Offset: 0x0003B352
		public float GetSavedStatValueWithAll(StatTypes statType, Identifier statIdentifier)
		{
			return this.GetSavedStatValue(statType, Tags.StatIdentifierTargetAll) + this.GetSavedStatValue(statType, statIdentifier);
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x0003D169 File Offset: 0x0003B369
		public float GetSavedStatValueWithBotsInMp(StatTypes statType, Identifier statIdentifier)
		{
			return this.GetSavedStatValueWithBotsInMp(statType, statIdentifier, GameSession.GetSessionCrewCharacters(CharacterType.Bot));
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x0003D17C File Offset: 0x0003B37C
		public float GetSavedStatValueWithBotsInMp(StatTypes statType, Identifier statIdentifier, IReadOnlyCollection<Character> bots)
		{
			float statValue = this.GetSavedStatValue(statType, statIdentifier);
			if (GameMain.NetworkMember == null)
			{
				return statValue;
			}
			foreach (Character bot in bots)
			{
				int botStatValue = (int)bot.Info.GetSavedStatValue(statType, statIdentifier);
				statValue = Math.Max(statValue, (float)botStatValue);
			}
			return statValue;
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x0003D1E8 File Offset: 0x0003B3E8
		public void ChangeSavedStatValue(StatTypes statType, float value, Identifier statIdentifier, bool removeOnDeath, float maxValue = 3.4028235E+38f, bool setValue = false)
		{
			if (!this.SavedStatValues.ContainsKey(statType))
			{
				this.SavedStatValues.Add(statType, new List<SavedStatValue>());
			}
			SavedStatValue savedStat = this.SavedStatValues[statType].FirstOrDefault(delegate(SavedStatValue s)
			{
				Identifier statIdentifier2 = s.StatIdentifier;
				return statIdentifier2 == statIdentifier;
			});
			if (savedStat != null)
			{
				float prevValue = savedStat.StatValue;
				savedStat.StatValue = (setValue ? value : MathHelper.Min(savedStat.StatValue + value, maxValue));
				bool changed = !MathUtils.NearlyEqual(savedStat.StatValue, prevValue, 0.0001f);
			}
			else
			{
				this.SavedStatValues[statType].Add(new SavedStatValue(statIdentifier, MathHelper.Min(value, maxValue), removeOnDeath));
			}
		}

		// Token: 0x060006ED RID: 1773 RVA: 0x0003D3BC File Offset: 0x0003B5BC
		[CompilerGenerated]
		private int <SetAttachments>g__pickRandomIndex|179_0(IReadOnlyList<ContentXElement> list, ref CharacterInfo.<>c__DisplayClass179_0 A_2)
		{
			ContentXElement[] elems = this.GetValidAttachmentElements(list, this.Head.Preset, null).ToArray<ContentXElement>();
			float[] weights = CharacterInfo.GetWeights(elems).ToArray<float>();
			return list.IndexOf(ToolBox.SelectWeightedRandom<ContentXElement>(elems, weights, A_2.randSync));
		}

		// Token: 0x060006EE RID: 1774 RVA: 0x0003D40C File Offset: 0x0003B60C
		[CompilerGenerated]
		internal static Color <RecreateHead>g__ChooseColor|206_0([TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})] in ImmutableArray<ValueTuple<Color, float>> availableColors, Color chosenColor)
		{
			if (!availableColors.Any(([TupleElementNames(new string[]
			{
				"Color",
				"Commonness"
			})] ValueTuple<Color, float> c) => c.Item1 == chosenColor))
			{
				return CharacterInfo.SelectRandomColor(availableColors, Rand.RandSync.Unsynced);
			}
			return chosenColor;
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x0003D4BC File Offset: 0x0003B6BC
		[CompilerGenerated]
		internal static bool <LoadOrders>g__GetTargetEntity|255_0(ushort targetId, out Entity targetEntity, ref CharacterInfo.<>c__DisplayClass255_0 A_2, ref CharacterInfo.<>c__DisplayClass255_1 A_3, ref CharacterInfo.<>c__DisplayClass255_2 A_4)
		{
			targetEntity = null;
			if (targetId == 0)
			{
				return true;
			}
			Submarine parentSub = null;
			if (A_3.orderElement.GetAttributeBool("onmainsub", false))
			{
				parentSub = Submarine.MainSub;
			}
			else
			{
				int linkedSubIndex = A_3.orderElement.GetAttributeInt("linkedsubindex", -1);
				if (linkedSubIndex >= 0 && linkedSubIndex < A_2.linkedSubs.Count)
				{
					LinkedSubmarine linkedSub = A_2.linkedSubs[linkedSubIndex];
					if (linkedSub != null && linkedSub.LoadSub)
					{
						parentSub = linkedSub.Sub;
					}
				}
			}
			if (parentSub != null)
			{
				targetId = CharacterInfo.GetOffsetId(parentSub, targetId);
				targetEntity = Entity.FindEntityByID(targetId);
				return targetEntity != null;
			}
			if (!A_4.orderPrefab.CanBeGeneralized)
			{
				DebugConsole.ThrowError("Error loading a previously saved order (" + A_4.orderIdentifier + "). Can't find the parent sub of the target entity. The order requires a target so it can't be loaded at all.", null, null, false, false);
				int priorityIncrease = A_2.priorityIncrease;
				A_2.priorityIncrease = priorityIncrease + 1;
				return false;
			}
			DebugConsole.AddWarning("Trying to load a previously saved order (" + A_4.orderIdentifier + "). Can't find the parent sub of the target entity. The order doesn't require a target so a more generic version of the order will be loaded instead.", null);
			return true;
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x0003D5A8 File Offset: 0x0003B7A8
		[CompilerGenerated]
		internal static bool <ResetSavedStatValue>g__MatchesIdentifier|269_0(Identifier statIdentifier, Identifier identifier)
		{
			if (statIdentifier == identifier)
			{
				return true;
			}
			int index = identifier.IndexOf('*');
			return index > -1 && statIdentifier.StartsWith(identifier[new Range(0, index)]);
		}

		// Token: 0x04000342 RID: 834
		private static Sprite infoAreaPortraitBG;

		// Token: 0x04000343 RID: 835
		public bool LastControlled;

		// Token: 0x04000345 RID: 837
		private Sprite disguisedPortrait;

		// Token: 0x04000346 RID: 838
		private List<WearableSprite> disguisedAttachmentSprites;

		// Token: 0x04000347 RID: 839
		private Vector2? disguisedSheetIndex;

		// Token: 0x04000348 RID: 840
		private Sprite disguisedJobIcon;

		// Token: 0x04000349 RID: 841
		private Color disguisedJobColor;

		// Token: 0x0400034A RID: 842
		private Color disguisedHairColor;

		// Token: 0x0400034B RID: 843
		private Color disguisedFacialHairColor;

		// Token: 0x0400034C RID: 844
		private Color disguisedSkinColor;

		// Token: 0x0400034D RID: 845
		private Sprite tintMask;

		// Token: 0x0400034E RID: 846
		private float tintHighlightThreshold;

		// Token: 0x0400034F RID: 847
		private float tintHighlightMultiplier;

		// Token: 0x04000350 RID: 848
		public bool ShowTalentResetPopupOnOpen = true;

		// Token: 0x04000351 RID: 849
		private SpriteBatch.EffectWithParams headEffectParameters;

		// Token: 0x04000352 RID: 850
		private Dictionary<WearableType, SpriteBatch.EffectWithParams> attachmentEffectParameters = new Dictionary<WearableType, SpriteBatch.EffectWithParams>();

		// Token: 0x04000353 RID: 851
		private CharacterInfo.HeadInfo head;

		// Token: 0x04000354 RID: 852
		private readonly Identifier maleIdentifier = "Male".ToIdentifier();

		// Token: 0x04000355 RID: 853
		private readonly Identifier femaleIdentifier = "Female".ToIdentifier();

		// Token: 0x04000356 RID: 854
		public XElement InventoryData;

		// Token: 0x04000357 RID: 855
		public XElement HealthData;

		// Token: 0x04000358 RID: 856
		public XElement OrderData;

		// Token: 0x04000359 RID: 857
		public bool PermanentlyDead;

		// Token: 0x0400035A RID: 858
		public bool RenamingEnabled;

		// Token: 0x0400035B RID: 859
		private BotStatus botStatus = BotStatus.ActiveService;

		// Token: 0x0400035C RID: 860
		public bool PendingSpawnToActiveService;

		// Token: 0x0400035D RID: 861
		private static ushort idCounter = 1;

		// Token: 0x0400035E RID: 862
		private const string disguiseName = "???";

		// Token: 0x04000360 RID: 864
		public string Name;

		// Token: 0x04000361 RID: 865
		public LocalizedString Title;

		// Token: 0x04000362 RID: 866
		[TupleElementNames(new string[]
		{
			"NpcSetIdentifier",
			"NpcIdentifier"
		})]
		public ValueTuple<Identifier, Identifier> HumanPrefabIds;

		// Token: 0x04000363 RID: 867
		private HumanPrefab _humanPrefab;

		// Token: 0x04000365 RID: 869
		private Character character;

		// Token: 0x04000366 RID: 870
		public Job Job;

		// Token: 0x04000367 RID: 871
		public int Salary;

		// Token: 0x04000369 RID: 873
		private int talentRefundPoints;

		// Token: 0x0400036C RID: 876
		private int talentResetCount;

		// Token: 0x0400036D RID: 877
		[TupleElementNames(new string[]
		{
			"factionId",
			"reputation"
		})]
		public ValueTuple<Identifier, float> MinReputationToHire;

		// Token: 0x0400036E RID: 878
		public const int MaxAdditionalTalentPoints = 100;

		// Token: 0x0400036F RID: 879
		private int additionalTalentPoints;

		// Token: 0x04000370 RID: 880
		private Sprite _headSprite;

		// Token: 0x04000371 RID: 881
		public bool OmitJobInMenus;

		// Token: 0x04000372 RID: 882
		private Sprite portrait;

		// Token: 0x04000373 RID: 883
		public bool IsDisguised;

		// Token: 0x04000374 RID: 884
		public bool IsDisguisedAsAnother;

		// Token: 0x04000375 RID: 885
		private List<WearableSprite> attachmentSprites;

		// Token: 0x04000377 RID: 887
		public bool StartItemsGiven;

		// Token: 0x04000378 RID: 888
		public bool IsNewHire;

		// Token: 0x04000379 RID: 889
		public CauseOfDeath CauseOfDeath;

		// Token: 0x0400037A RID: 890
		public CharacterTeamType TeamID;

		// Token: 0x0400037C RID: 892
		public const int MaxCurrentOrders = 3;

		// Token: 0x0400037E RID: 894
		public ushort ID;

		// Token: 0x04000380 RID: 896
		public readonly bool HasSpecifierTags;

		// Token: 0x04000381 RID: 897
		private RagdollParams ragdoll;

		// Token: 0x04000382 RID: 898
		[TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})]
		public readonly ImmutableArray<ValueTuple<Color, float>> HairColors;

		// Token: 0x04000383 RID: 899
		[TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})]
		public readonly ImmutableArray<ValueTuple<Color, float>> FacialHairColors;

		// Token: 0x04000384 RID: 900
		[TupleElementNames(new string[]
		{
			"Color",
			"Commonness"
		})]
		public readonly ImmutableArray<ValueTuple<Color, float>> SkinColors;

		// Token: 0x04000385 RID: 901
		public int MissionsCompletedSinceDeath;

		// Token: 0x04000386 RID: 902
		public Option<int> LastRewardDistribution;

		// Token: 0x04000387 RID: 903
		private List<ContentXElement> hairs;

		// Token: 0x04000388 RID: 904
		private List<ContentXElement> beards;

		// Token: 0x04000389 RID: 905
		private List<ContentXElement> moustaches;

		// Token: 0x0400038A RID: 906
		private List<ContentXElement> faceAttachments;

		// Token: 0x0400038B RID: 907
		private IEnumerable<ContentXElement> wearables;

		// Token: 0x0400038C RID: 908
		private bool spriteTagsLoaded;

		// Token: 0x0400038D RID: 909
		private static readonly ImmutableDictionary<Identifier, StatTypes> skillGainStatValues = new Dictionary<Identifier, StatTypes>
		{
			{
				new Identifier("helm"),
				StatTypes.HelmSkillGainSpeed
			},
			{
				new Identifier("weapons"),
				StatTypes.WeaponsSkillGainSpeed
			},
			{
				new Identifier("medical"),
				StatTypes.MedicalSkillGainSpeed
			},
			{
				new Identifier("electrical"),
				StatTypes.ElectricalSkillGainSpeed
			},
			{
				new Identifier("mechanical"),
				StatTypes.MechanicalSkillGainSpeed
			}
		}.ToImmutableDictionary<Identifier, StatTypes>();

		// Token: 0x0400038E RID: 910
		private const int BaseExperienceRequired = 450;

		// Token: 0x0400038F RID: 911
		private const int AddedExperienceRequiredPerLevel = 500;

		// Token: 0x04000390 RID: 912
		public readonly Dictionary<StatTypes, List<SavedStatValue>> SavedStatValues;

		// Token: 0x04000391 RID: 913
		public float LastResistanceMultiplierSkillLossDeath;

		// Token: 0x04000392 RID: 914
		public float LastResistanceMultiplierSkillLossRespawn;

		// Token: 0x020006CC RID: 1740
		public class AppearanceCustomizationMenu : IDisposable
		{
			// Token: 0x060066C9 RID: 26313 RVA: 0x00347FDF File Offset: 0x003461DF
			public AppearanceCustomizationMenu(CharacterInfo info, GUIComponent parent, bool hasIcon = true)
			{
				this.CharacterInfo = info;
				this.parentComponent = parent;
				this.HasIcon = hasIcon;
				this.RecreateFrameContents();
			}

			// Token: 0x060066CA RID: 26314 RVA: 0x00348014 File Offset: 0x00346214
			public void RecreateFrameContents()
			{
				CharacterInfo.AppearanceCustomizationMenu.<>c__DisplayClass10_0 CS$<>8__locals1 = new CharacterInfo.AppearanceCustomizationMenu.<>c__DisplayClass10_0();
				CS$<>8__locals1.<>4__this = this;
				CS$<>8__locals1.info = this.CharacterInfo;
				this.HeadSelectionList = null;
				this.parentComponent.ClearChildren();
				this.ClearSprites();
				float contentWidth = this.HasIcon ? 0.75f : 1f;
				GUIListBox listBox = new GUIListBox(new RectTransform(new Vector2(contentWidth, 1f), this.parentComponent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					CanBeFocused = false,
					CanTakeKeyBoardFocus = false
				};
				CS$<>8__locals1.content = listBox.Content;
				CS$<>8__locals1.info.LoadHeadAttachments();
				if (this.HasIcon)
				{
					CS$<>8__locals1.info.CreateIcon(new RectTransform(new Vector2(0.25f, 1f), this.parentComponent.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
					{
						RelativeOffset = new Vector2(-0.01f, 0f)
					});
				}
				RectTransform menuCategoryRT = CS$<>8__locals1.<RecreateFrameContents>g__createItemRectTransform|0(CS$<>8__locals1.info.Prefab.MenuCategoryVar, 1f);
				CS$<>8__locals1.menuCategoryContainer = new GUILayoutGroup(menuCategoryRT, true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				foreach (Identifier tag in (from t in CS$<>8__locals1.info.Prefab.VarTags[CS$<>8__locals1.info.Prefab.MenuCategoryVar]
				orderby t.Value
				select t).Reverse<Identifier>())
				{
					CS$<>8__locals1.<RecreateFrameContents>g__createMenuCategoryButton|1(tag);
				}
				CS$<>8__locals1.attachmentSliders = new List<GUIScrollBar>();
				CS$<>8__locals1.<RecreateFrameContents>g__createAttachmentSlider|2(CS$<>8__locals1.info.Head.HairIndex, WearableType.Hair);
				CS$<>8__locals1.<RecreateFrameContents>g__createAttachmentSlider|2(CS$<>8__locals1.info.Head.BeardIndex, WearableType.Beard);
				CS$<>8__locals1.<RecreateFrameContents>g__createAttachmentSlider|2(CS$<>8__locals1.info.Head.MoustacheIndex, WearableType.Moustache);
				CS$<>8__locals1.<RecreateFrameContents>g__createAttachmentSlider|2(CS$<>8__locals1.info.Head.FaceAttachmentIndex, WearableType.FaceAttachment);
				if (CS$<>8__locals1.info.CountValidAttachmentsOfType(WearableType.Hair) > 0)
				{
					CS$<>8__locals1.<RecreateFrameContents>g__createColorSelector|3("Customization.HairColor".ToIdentifier(), CS$<>8__locals1.info.HairColors, () => CS$<>8__locals1.info.Head.HairColor, delegate(Color color)
					{
						CS$<>8__locals1.info.Head.HairColor = color;
					});
				}
				if (CS$<>8__locals1.info.CountValidAttachmentsOfType(WearableType.Moustache) > 0 || CS$<>8__locals1.info.CountValidAttachmentsOfType(WearableType.Beard) > 0)
				{
					CS$<>8__locals1.<RecreateFrameContents>g__createColorSelector|3("Customization.FacialHairColor".ToIdentifier(), CS$<>8__locals1.info.FacialHairColors, () => CS$<>8__locals1.info.Head.FacialHairColor, delegate(Color color)
					{
						CS$<>8__locals1.info.Head.FacialHairColor = color;
					});
				}
				CS$<>8__locals1.<RecreateFrameContents>g__createColorSelector|3("Customization.SkinColor".ToIdentifier(), CS$<>8__locals1.info.SkinColors, () => CS$<>8__locals1.info.Head.SkinColor, delegate(Color color)
				{
					CS$<>8__locals1.info.Head.SkinColor = color;
				});
				this.RandomizeButton = new GUIButton(new RectTransform(Vector2.One * 0.12f, this.parentComponent.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Smallest)
				{
					RelativeOffset = new Vector2(0.01f, 0.005f)
				}, Alignment.Center, "RandomizeButton", null)
				{
					OnClicked = delegate(GUIButton button, object o)
					{
						CharacterInfo.HeadPreset headPreset = CS$<>8__locals1.info.Prefab.Heads.GetRandom(Rand.RandSync.Unsynced);
						CS$<>8__locals1.info.Head = new CharacterInfo.HeadInfo(CS$<>8__locals1.info, headPreset, 0, 0, 0, 0);
						CS$<>8__locals1.info.SetAttachments(Rand.RandSync.Unsynced);
						CS$<>8__locals1.info.SetColors(Rand.RandSync.Unsynced);
						CS$<>8__locals1.<>4__this.RecreateFrameContents();
						CS$<>8__locals1.info.RefreshHead();
						Action<CharacterInfo.AppearanceCustomizationMenu> onHeadSwitch = CS$<>8__locals1.<>4__this.OnHeadSwitch;
						if (onHeadSwitch != null)
						{
							onHeadSwitch(CS$<>8__locals1.<>4__this);
						}
						List<GUIScrollBar> attachmentSliders = CS$<>8__locals1.attachmentSliders;
						Action<GUIScrollBar> action;
						if ((action = CS$<>8__locals1.<>9__17) == null)
						{
							action = (CS$<>8__locals1.<>9__17 = delegate(GUIScrollBar s)
							{
								GUIScrollBar.OnMovedHandler onSliderMoved = CS$<>8__locals1.<>4__this.OnSliderMoved;
								if (onSliderMoved == null)
								{
									return;
								}
								onSliderMoved(s, s.BarScroll);
							});
						}
						attachmentSliders.ForEach(action);
						return false;
					}
				};
				listBox.ForceLayoutRecalculation();
				foreach (GUILayoutGroup childLayoutGroup in listBox.Content.GetAllChildren<GUILayoutGroup>())
				{
					childLayoutGroup.Recalculate();
				}
			}

			// Token: 0x060066CB RID: 26315 RVA: 0x0034841C File Offset: 0x0034661C
			private bool OpenHeadSelection(GUIButton button, object userData)
			{
				Identifier selectedCategory = (Identifier)userData;
				CharacterInfo info = this.CharacterInfo;
				if (info.HeadSprite == null)
				{
					DebugConsole.ThrowError("Head Selection: the head sprite is null! Failed to open the head selection.", null, null, false, false);
					return false;
				}
				float characterHeightWidthRatio = info.HeadSprite.size.Y / info.HeadSprite.size.X;
				if (this.HeadSelectionList == null)
				{
					this.HeadSelectionList = new GUIListBox(new RectTransform(new Point(this.parentComponent.Rect.Width, (int)((float)this.parentComponent.Rect.Width * characterHeightWidthRatio * 0.6f)), GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false)
					{
						AbsoluteOffset = new Point(this.parentComponent.Rect.Right - this.parentComponent.Rect.Width, button.Rect.Bottom)
					}, false, null, "", true, false);
				}
				this.HeadSelectionList.Visible = true;
				this.HeadSelectionList.Content.ClearChildren();
				this.ClearSprites();
				this.parentComponent.RectTransform.SizeChanged += delegate()
				{
					if (this.parentComponent != null)
					{
						GUIListBox headSelectionList = this.HeadSelectionList;
						if (((headSelectionList != null) ? headSelectionList.RectTransform : null) != null && button != null)
						{
							this.HeadSelectionList.RectTransform.Resize(new Point(this.parentComponent.Rect.Width, (int)((float)this.parentComponent.Rect.Width * characterHeightWidthRatio * 0.6f)), true);
							this.HeadSelectionList.RectTransform.AbsoluteOffset = new Point(this.parentComponent.Rect.Right - this.parentComponent.Rect.Width, button.Rect.Bottom);
							return;
						}
					}
				};
				GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(1.25f, 1.25f), this.HeadSelectionList.ContentBackground.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "OuterGlow", new Color?(Color.Black));
				guiframe.UserData = "outerglow";
				guiframe.CanBeFocused = false;
				ContentXElement mainElement = info.Ragdoll.MainElement;
				ContentXElement contentXElement;
				if (mainElement == null)
				{
					contentXElement = null;
				}
				else
				{
					contentXElement = mainElement.Elements().FirstOrDefault((ContentXElement e) => e.GetAttributeString("type", "").Equals("head", StringComparison.OrdinalIgnoreCase));
				}
				ContentXElement headElement = contentXElement;
				ContentXElement contentXElement2 = null;
				if (headElement == contentXElement2)
				{
					DebugConsole.ThrowError("Head Selection: the head element is null in " + info.ragdoll.FileName + "! Failed to open the head selection.", null, null, false, false);
					return false;
				}
				ContentXElement headSpriteElement = headElement.GetChildElement("sprite");
				ContentPath spritePathWithTags = headSpriteElement.GetAttributeContentPath("texture");
				ContentXElement characterConfigElement = info.CharacterConfigElement;
				ImmutableArray<CharacterInfo.HeadPreset> heads = info.Prefab.Heads;
				if (new ImmutableArray<CharacterInfo.HeadPreset>?(heads) != null)
				{
					GUILayoutGroup row = null;
					int itemsInRow = 0;
					ImmutableArray<CharacterInfo.HeadPreset> immutableArray = heads;
					Func<CharacterInfo.HeadPreset, bool> <>9__2;
					Func<CharacterInfo.HeadPreset, bool> predicate;
					if ((predicate = <>9__2) == null)
					{
						predicate = (<>9__2 = ((CharacterInfo.HeadPreset h) => h.TagSet.Contains(selectedCategory)));
					}
					foreach (CharacterInfo.HeadPreset head in immutableArray.Where(predicate))
					{
						string spritePath = info.Prefab.ReplaceVars(spritePathWithTags.Value, head);
						if (File.Exists(spritePath))
						{
							Sprite headSprite = new Sprite(headSpriteElement, "", spritePath, false, 1f);
							headSprite.SourceRect = new Rectangle(CharacterInfo.CalculateOffset(headSprite, head.SheetIndex.ToPoint()), headSprite.SourceRect.Size);
							this.characterSprites.Add(headSprite);
							if (itemsInRow >= 4 || row == null)
							{
								row = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.333f), this.HeadSelectionList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
								{
									UserData = head.MenuCategory,
									Visible = true
								};
								itemsInRow = 0;
							}
							GUIButton btn = new GUIButton(new RectTransform(new Vector2(0.25f, 1f), row.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "ListBoxElementSquare", null)
							{
								OutlineColor = Color.White * 0.5f,
								PressedColor = Color.White * 0.5f,
								UserData = head,
								OnClicked = new GUIButton.OnClickedHandler(this.SwitchHead),
								Selected = (info.Head.Preset == head),
								Visible = true
							};
							new GUIImage(new RectTransform(Vector2.One, btn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), headSprite, true, null);
							itemsInRow++;
						}
					}
				}
				return false;
			}

			// Token: 0x060066CC RID: 26316 RVA: 0x00348908 File Offset: 0x00346B08
			private bool SwitchHead(GUIButton button, object obj)
			{
				CharacterInfo info = this.CharacterInfo;
				CharacterInfo.HeadPreset headPreset = obj as CharacterInfo.HeadPreset;
				if (info.Head.Preset != headPreset)
				{
					info.Head = new CharacterInfo.HeadInfo(info, headPreset, info.Head.HairIndex, info.Head.BeardIndex, info.Head.MoustacheIndex, info.Head.FaceAttachmentIndex)
					{
						SkinColor = info.Head.SkinColor,
						HairColor = info.Head.HairColor,
						FacialHairColor = info.Head.FacialHairColor
					};
					info.ReloadHeadAttachments();
				}
				this.RecreateFrameContents();
				Action<CharacterInfo.AppearanceCustomizationMenu> onHeadSwitch = this.OnHeadSwitch;
				if (onHeadSwitch != null)
				{
					onHeadSwitch(this);
				}
				return true;
			}

			// Token: 0x060066CD RID: 26317 RVA: 0x003489BC File Offset: 0x00346BBC
			private bool SwitchAttachment(GUIScrollBar scrollBar, WearableType type)
			{
				CharacterInfo info = this.CharacterInfo;
				int index = (int)Math.Round((double)scrollBar.BarScrollValue);
				switch (type)
				{
				case WearableType.Hair:
					info.Head.HairIndex = index;
					break;
				case WearableType.Beard:
					info.Head.BeardIndex = index;
					break;
				case WearableType.Moustache:
					info.Head.MoustacheIndex = index;
					break;
				case WearableType.FaceAttachment:
					info.Head.FaceAttachmentIndex = index;
					break;
				default:
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Wearable type not implemented: ");
					defaultInterpolatedStringHandler.AppendFormatted<WearableType>(type);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return false;
				}
				}
				info.RefreshHead();
				GUIScrollBar.OnMovedHandler onSliderMoved = this.OnSliderMoved;
				if (onSliderMoved != null)
				{
					onSliderMoved(scrollBar, scrollBar.BarScroll);
				}
				return true;
			}

			// Token: 0x060066CE RID: 26318 RVA: 0x00348A80 File Offset: 0x00346C80
			public void Update()
			{
				if (this.HeadSelectionList != null && PlayerInput.PrimaryMouseButtonDown() && !GUI.IsMouseOn(this.HeadSelectionList))
				{
					this.HeadSelectionList.Visible = false;
				}
			}

			// Token: 0x060066CF RID: 26319 RVA: 0x00348AAA File Offset: 0x00346CAA
			public void AddToGUIUpdateList()
			{
				GUIListBox headSelectionList = this.HeadSelectionList;
				if (headSelectionList == null)
				{
					return;
				}
				headSelectionList.AddToGUIUpdateList(false, 0);
			}

			// Token: 0x060066D0 RID: 26320 RVA: 0x00348AC0 File Offset: 0x00346CC0
			private void ClearSprites()
			{
				foreach (Sprite sprite in this.characterSprites)
				{
					sprite.Remove();
				}
				this.characterSprites.Clear();
			}

			// Token: 0x060066D1 RID: 26321 RVA: 0x00348B20 File Offset: 0x00346D20
			public void Dispose()
			{
				this.ClearSprites();
				if (this.HeadSelectionList != null)
				{
					this.HeadSelectionList.RectTransform.Parent = null;
					this.HeadSelectionList = null;
				}
			}

			// Token: 0x040037E6 RID: 14310
			public readonly CharacterInfo CharacterInfo;

			// Token: 0x040037E7 RID: 14311
			public GUIListBox HeadSelectionList;

			// Token: 0x040037E8 RID: 14312
			public bool HasIcon = true;

			// Token: 0x040037E9 RID: 14313
			public GUIScrollBar.OnMovedHandler OnSliderMoved;

			// Token: 0x040037EA RID: 14314
			public GUIScrollBar.OnMovedHandler OnSliderReleased;

			// Token: 0x040037EB RID: 14315
			public Action<CharacterInfo.AppearanceCustomizationMenu> OnHeadSwitch;

			// Token: 0x040037EC RID: 14316
			private readonly GUIComponent parentComponent;

			// Token: 0x040037ED RID: 14317
			private readonly List<Sprite> characterSprites = new List<Sprite>();

			// Token: 0x040037EE RID: 14318
			public GUIButton RandomizeButton;
		}

		// Token: 0x020006CD RID: 1741
		public class HeadInfo
		{
			// Token: 0x170019C8 RID: 6600
			// (get) Token: 0x060066D2 RID: 26322 RVA: 0x00348B48 File Offset: 0x00346D48
			// (set) Token: 0x060066D3 RID: 26323 RVA: 0x00348B50 File Offset: 0x00346D50
			public int HairIndex { get; set; }

			// Token: 0x060066D4 RID: 26324 RVA: 0x00348B5C File Offset: 0x00346D5C
			public void SetHairWithHatIndex()
			{
				if (this.CharacterInfo.Hairs == null)
				{
					if (this.HairIndex == -1)
					{
						DebugConsole.AddWarning("Setting \"hairWithHatIndex\" before \"Hairs\" are defined!", null);
					}
					this.hairWithHatIndex = new int?(this.HairIndex);
					return;
				}
				ContentXElement hairElement = this.HairElement;
				this.hairWithHatIndex = new int?((hairElement != null) ? hairElement.GetAttributeInt("replacewhenwearinghat", this.HairIndex) : -1);
				int? num = this.hairWithHatIndex;
				int num2 = 0;
				if (!(num.GetValueOrDefault() < num2 & num != null))
				{
					num = this.hairWithHatIndex;
					num2 = this.CharacterInfo.Hairs.Count;
					if (!(num.GetValueOrDefault() >= num2 & num != null))
					{
						return;
					}
				}
				this.hairWithHatIndex = new int?(this.HairIndex);
			}

			// Token: 0x170019C9 RID: 6601
			// (get) Token: 0x060066D5 RID: 26325 RVA: 0x00348C21 File Offset: 0x00346E21
			public Vector2 SheetIndex
			{
				get
				{
					return this.Preset.SheetIndex;
				}
			}

			// Token: 0x170019CA RID: 6602
			// (get) Token: 0x060066D6 RID: 26326 RVA: 0x00348C30 File Offset: 0x00346E30
			public ContentXElement HairElement
			{
				get
				{
					if (this.CharacterInfo.Hairs == null)
					{
						return null;
					}
					if (this.HairIndex >= this.CharacterInfo.Hairs.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Hair index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.HairIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.Hairs.ElementAtOrDefault(this.HairIndex);
				}
			}

			// Token: 0x170019CB RID: 6603
			// (get) Token: 0x060066D7 RID: 26327 RVA: 0x00348CE4 File Offset: 0x00346EE4
			public ContentXElement HairWithHatElement
			{
				get
				{
					if (this.hairWithHatIndex == null)
					{
						this.SetHairWithHatIndex();
					}
					if (this.CharacterInfo.Hairs == null)
					{
						return null;
					}
					int? num = this.hairWithHatIndex;
					int count = this.CharacterInfo.Hairs.Count;
					if (num.GetValueOrDefault() >= count & num != null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Hair with hat index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int?>(this.hairWithHatIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.Hairs.ElementAtOrDefault(this.hairWithHatIndex.Value);
				}
			}

			// Token: 0x170019CC RID: 6604
			// (get) Token: 0x060066D8 RID: 26328 RVA: 0x00348DC8 File Offset: 0x00346FC8
			public ContentXElement BeardElement
			{
				get
				{
					if (this.CharacterInfo.Beards == null)
					{
						return null;
					}
					if (this.BeardIndex >= this.CharacterInfo.Beards.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Beard index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.BeardIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.Beards.ElementAtOrDefault(this.BeardIndex);
				}
			}

			// Token: 0x170019CD RID: 6605
			// (get) Token: 0x060066D9 RID: 26329 RVA: 0x00348E7C File Offset: 0x0034707C
			public ContentXElement MoustacheElement
			{
				get
				{
					if (this.CharacterInfo.Moustaches == null)
					{
						return null;
					}
					if (this.MoustacheIndex >= this.CharacterInfo.Moustaches.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Moustache index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.MoustacheIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.Moustaches.ElementAtOrDefault(this.MoustacheIndex);
				}
			}

			// Token: 0x170019CE RID: 6606
			// (get) Token: 0x060066DA RID: 26330 RVA: 0x00348F30 File Offset: 0x00347130
			public ContentXElement FaceAttachment
			{
				get
				{
					if (this.CharacterInfo.FaceAttachments == null)
					{
						return null;
					}
					if (this.FaceAttachmentIndex >= this.CharacterInfo.FaceAttachments.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Face attachment index out of range (character: ");
						CharacterInfo characterInfo = this.CharacterInfo;
						defaultInterpolatedStringHandler.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", index: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.FaceAttachmentIndex);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					return this.CharacterInfo.FaceAttachments.ElementAtOrDefault(this.FaceAttachmentIndex);
				}
			}

			// Token: 0x060066DB RID: 26331 RVA: 0x00348FE4 File Offset: 0x003471E4
			public HeadInfo(CharacterInfo characterInfo, CharacterInfo.HeadPreset headPreset, int hairIndex = 0, int beardIndex = 0, int moustacheIndex = 0, int faceAttachmentIndex = 0)
			{
				this.CharacterInfo = characterInfo;
				this.Preset = headPreset;
				this.HairIndex = hairIndex;
				this.BeardIndex = beardIndex;
				this.MoustacheIndex = moustacheIndex;
				this.FaceAttachmentIndex = faceAttachmentIndex;
			}

			// Token: 0x060066DC RID: 26332 RVA: 0x00349019 File Offset: 0x00347219
			public void ResetAttachmentIndices()
			{
				this.HairIndex = -1;
				this.BeardIndex = -1;
				this.MoustacheIndex = -1;
				this.FaceAttachmentIndex = -1;
			}

			// Token: 0x040037EF RID: 14319
			public readonly CharacterInfo CharacterInfo;

			// Token: 0x040037F0 RID: 14320
			public readonly CharacterInfo.HeadPreset Preset;

			// Token: 0x040037F2 RID: 14322
			private int? hairWithHatIndex;

			// Token: 0x040037F3 RID: 14323
			public int BeardIndex;

			// Token: 0x040037F4 RID: 14324
			public int MoustacheIndex;

			// Token: 0x040037F5 RID: 14325
			public int FaceAttachmentIndex;

			// Token: 0x040037F6 RID: 14326
			public Color HairColor;

			// Token: 0x040037F7 RID: 14327
			public Color FacialHairColor;

			// Token: 0x040037F8 RID: 14328
			public Color SkinColor;
		}

		// Token: 0x020006CE RID: 1742
		public class HeadPreset : ISerializableEntity
		{
			// Token: 0x170019CF RID: 6607
			// (get) Token: 0x060066DD RID: 26333 RVA: 0x00349037 File Offset: 0x00347237
			public Identifier MenuCategory
			{
				get
				{
					return this.TagSet.First((Identifier t) => this.characterInfoPrefab.VarTags[this.characterInfoPrefab.MenuCategoryVar].Contains(t));
				}
			}

			// Token: 0x170019D0 RID: 6608
			// (get) Token: 0x060066DE RID: 26334 RVA: 0x00349050 File Offset: 0x00347250
			// (set) Token: 0x060066DF RID: 26335 RVA: 0x00349058 File Offset: 0x00347258
			public ImmutableHashSet<Identifier> TagSet { get; private set; }

			// Token: 0x170019D1 RID: 6609
			// (get) Token: 0x060066E0 RID: 26336 RVA: 0x00349061 File Offset: 0x00347261
			// (set) Token: 0x060066E1 RID: 26337 RVA: 0x00349074 File Offset: 0x00347274
			[Serialize("", IsPropertySaveable.No, "", "", false)]
			public string Tags
			{
				get
				{
					return string.Join<Identifier>(",", this.TagSet);
				}
				private set
				{
					this.TagSet = (from s in value.Split(",", StringSplitOptions.None)
					select s.ToIdentifier() into id
					where !id.IsEmpty
					select id).ToImmutableHashSet<Identifier>();
				}
			}

			// Token: 0x170019D2 RID: 6610
			// (get) Token: 0x060066E2 RID: 26338 RVA: 0x003490E0 File Offset: 0x003472E0
			// (set) Token: 0x060066E3 RID: 26339 RVA: 0x003490E8 File Offset: 0x003472E8
			[Serialize("0,0", IsPropertySaveable.No, "", "", false)]
			public Vector2 SheetIndex { get; private set; }

			// Token: 0x170019D3 RID: 6611
			// (get) Token: 0x060066E4 RID: 26340 RVA: 0x003490F1 File Offset: 0x003472F1
			public string Name
			{
				get
				{
					return "Head Preset " + this.Tags;
				}
			}

			// Token: 0x170019D4 RID: 6612
			// (get) Token: 0x060066E5 RID: 26341 RVA: 0x00349103 File Offset: 0x00347303
			// (set) Token: 0x060066E6 RID: 26342 RVA: 0x0034910B File Offset: 0x0034730B
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

			// Token: 0x060066E7 RID: 26343 RVA: 0x00349114 File Offset: 0x00347314
			public HeadPreset(CharacterInfoPrefab charInfoPrefab, XElement element)
			{
				this.characterInfoPrefab = charInfoPrefab;
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
				this.DetermineTagsFromLegacyFormat(element);
			}

			// Token: 0x060066E8 RID: 26344 RVA: 0x00349138 File Offset: 0x00347338
			private void DetermineTagsFromLegacyFormat(XElement element)
			{
				string headId = element.GetAttributeString("id", "");
				string gender = element.GetAttributeString("gender", "");
				string race = element.GetAttributeString("race", "");
				if (!headId.IsNullOrEmpty())
				{
					this.<DetermineTagsFromLegacyFormat>g__addTag|21_0("head" + headId);
				}
				if (!gender.IsNullOrEmpty())
				{
					this.<DetermineTagsFromLegacyFormat>g__addTag|21_0(gender);
				}
				if (!race.IsNullOrEmpty())
				{
					this.<DetermineTagsFromLegacyFormat>g__addTag|21_0(race);
				}
			}

			// Token: 0x060066EA RID: 26346 RVA: 0x003491D2 File Offset: 0x003473D2
			[CompilerGenerated]
			private void <DetermineTagsFromLegacyFormat>g__addTag|21_0(string tag)
			{
				this.TagSet = this.TagSet.Add(tag.ToIdentifier());
			}

			// Token: 0x040037F9 RID: 14329
			private readonly CharacterInfoPrefab characterInfoPrefab;
		}
	}
}
