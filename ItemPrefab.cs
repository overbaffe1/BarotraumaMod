using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000C9 RID: 201
	internal class ItemPrefab : MapEntityPrefab, IImplementsVariants<ItemPrefab>
	{
		// Token: 0x17000675 RID: 1653
		// (get) Token: 0x060019F2 RID: 6642 RVA: 0x00106ADA File Offset: 0x00104CDA
		// (set) Token: 0x060019F3 RID: 6643 RVA: 0x00106AE2 File Offset: 0x00104CE2
		public ImmutableDictionary<Identifier, ImmutableArray<DecorativeSprite>> UpgradeOverrideSprites { get; private set; }

		// Token: 0x17000676 RID: 1654
		// (get) Token: 0x060019F4 RID: 6644 RVA: 0x00106AEB File Offset: 0x00104CEB
		// (set) Token: 0x060019F5 RID: 6645 RVA: 0x00106AF3 File Offset: 0x00104CF3
		public ImmutableArray<BrokenItemSprite> BrokenSprites { get; private set; }

		// Token: 0x17000677 RID: 1655
		// (get) Token: 0x060019F6 RID: 6646 RVA: 0x00106AFC File Offset: 0x00104CFC
		// (set) Token: 0x060019F7 RID: 6647 RVA: 0x00106B04 File Offset: 0x00104D04
		public ImmutableArray<DecorativeSprite> DecorativeSprites { get; private set; }

		// Token: 0x17000678 RID: 1656
		// (get) Token: 0x060019F8 RID: 6648 RVA: 0x00106B0D File Offset: 0x00104D0D
		// (set) Token: 0x060019F9 RID: 6649 RVA: 0x00106B15 File Offset: 0x00104D15
		public ImmutableArray<ContainedItemSprite> ContainedSprites { get; private set; }

		// Token: 0x17000679 RID: 1657
		// (get) Token: 0x060019FA RID: 6650 RVA: 0x00106B1E File Offset: 0x00104D1E
		// (set) Token: 0x060019FB RID: 6651 RVA: 0x00106B26 File Offset: 0x00104D26
		public ImmutableDictionary<int, ImmutableArray<DecorativeSprite>> DecorativeSpriteGroups { get; private set; }

		// Token: 0x1700067A RID: 1658
		// (get) Token: 0x060019FC RID: 6652 RVA: 0x00106B2F File Offset: 0x00104D2F
		// (set) Token: 0x060019FD RID: 6653 RVA: 0x00106B37 File Offset: 0x00104D37
		public Sprite InventoryIcon { get; private set; }

		// Token: 0x1700067B RID: 1659
		// (get) Token: 0x060019FE RID: 6654 RVA: 0x00106B40 File Offset: 0x00104D40
		// (set) Token: 0x060019FF RID: 6655 RVA: 0x00106B48 File Offset: 0x00104D48
		public Sprite MinimapIcon { get; private set; }

		// Token: 0x1700067C RID: 1660
		// (get) Token: 0x06001A00 RID: 6656 RVA: 0x00106B51 File Offset: 0x00104D51
		// (set) Token: 0x06001A01 RID: 6657 RVA: 0x00106B59 File Offset: 0x00104D59
		public Sprite UpgradePreviewSprite { get; private set; }

		// Token: 0x1700067D RID: 1661
		// (get) Token: 0x06001A02 RID: 6658 RVA: 0x00106B62 File Offset: 0x00104D62
		// (set) Token: 0x06001A03 RID: 6659 RVA: 0x00106B6A File Offset: 0x00104D6A
		public Sprite InfectedSprite { get; private set; }

		// Token: 0x1700067E RID: 1662
		// (get) Token: 0x06001A04 RID: 6660 RVA: 0x00106B73 File Offset: 0x00104D73
		// (set) Token: 0x06001A05 RID: 6661 RVA: 0x00106B7B File Offset: 0x00104D7B
		public Sprite DamagedInfectedSprite { get; private set; }

		// Token: 0x1700067F RID: 1663
		// (get) Token: 0x06001A06 RID: 6662 RVA: 0x00106B84 File Offset: 0x00104D84
		// (set) Token: 0x06001A07 RID: 6663 RVA: 0x00106B8C File Offset: 0x00104D8C
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.No, "", "", false)]
		public Color InventoryIconColor { get; protected set; }

		// Token: 0x17000680 RID: 1664
		// (get) Token: 0x06001A08 RID: 6664 RVA: 0x00106B95 File Offset: 0x00104D95
		// (set) Token: 0x06001A09 RID: 6665 RVA: 0x00106B9D File Offset: 0x00104D9D
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string ImpactSoundTag { get; private set; }

		// Token: 0x17000681 RID: 1665
		// (get) Token: 0x06001A0A RID: 6666 RVA: 0x00106BA6 File Offset: 0x00104DA6
		// (set) Token: 0x06001A0B RID: 6667 RVA: 0x00106BAE File Offset: 0x00104DAE
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowInStatusMonitor { get; private set; }

		// Token: 0x06001A0C RID: 6668 RVA: 0x00106BB8 File Offset: 0x00104DB8
		private void ParseSubElementsClient(ContentXElement element, ItemPrefab variantOf)
		{
			this.UpgradePreviewSprite = null;
			this.UpgradePreviewScale = 1f;
			this.InventoryIcon = null;
			this.MinimapIcon = null;
			this.InfectedSprite = null;
			this.DamagedInfectedSprite = null;
			Dictionary<Identifier, List<DecorativeSprite>> upgradeOverrideSprites = new Dictionary<Identifier, List<DecorativeSprite>>();
			List<BrokenItemSprite> brokenSprites = new List<BrokenItemSprite>();
			List<DecorativeSprite> decorativeSprites = new List<DecorativeSprite>();
			List<ContainedItemSprite> containedSprites = new List<ContainedItemSprite>();
			Dictionary<int, List<DecorativeSprite>> decorativeSpriteGroups = new Dictionary<int, List<DecorativeSprite>>();
			List<DamageModifier> wearableDamageModifiers = new List<DamageModifier>();
			Dictionary<Identifier, float> wearableSkillModifiers = new Dictionary<Identifier, float>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.LocalName.ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 8:
						if (text == "wearable")
						{
							foreach (ContentXElement wearableSubElement in subElement.Elements())
							{
								string a = wearableSubElement.Name.LocalName.ToLowerInvariant();
								if (!(a == "damagemodifier"))
								{
									if (a == "skillmodifier")
									{
										Identifier skillIdentifier = wearableSubElement.GetAttributeIdentifier("skillidentifier", Identifier.Empty);
										float skillValue = wearableSubElement.GetAttributeFloat("skillvalue", 0f);
										if (wearableSkillModifiers.ContainsKey(skillIdentifier))
										{
											Dictionary<Identifier, float> dictionary = wearableSkillModifiers;
											Identifier key = skillIdentifier;
											dictionary[key] += skillValue;
										}
										else
										{
											wearableSkillModifiers.TryAdd(skillIdentifier, skillValue);
										}
									}
								}
								else
								{
									wearableDamageModifiers.Add(new DamageModifier(wearableSubElement, this.Name.Value + ", Wearable", false));
								}
							}
						}
						break;
					case 11:
						if (text == "minimapicon")
						{
							string iconFolder = this.GetTexturePath(subElement, variantOf);
							this.MinimapIcon = new Sprite(subElement, iconFolder, "", true, 1f);
						}
						break;
					case 12:
						if (text == "brokensprite")
						{
							string brokenSpriteFolder = this.GetTexturePath(subElement, variantOf);
							Sprite sprite = new Sprite(subElement, brokenSpriteFolder, "", true, 1f);
							float attributeFloat = subElement.GetAttributeFloat("maxcondition", 0f);
							bool attributeBool = subElement.GetAttributeBool("fadein", false);
							ContentXElement contentXElement = subElement;
							string key2 = "offset";
							Point zero = Point.Zero;
							BrokenItemSprite brokenSprite = new BrokenItemSprite(sprite, attributeFloat, attributeBool, contentXElement.GetAttributePoint(key2, zero));
							if (brokenSprite.FadeIn && brokenSprite.MaxConditionPercentage <= 0f)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(98, 1);
								defaultInterpolatedStringHandler.AppendLiteral("Potential error in item ");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral(": a broken sprite that's set to fade in despite the max condition being 0.");
								DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + " The sprite cannot fade in if it's set to only appear when the item is fully broken.", base.ContentPackage);
							}
							int spriteIndex = 0;
							int i = 0;
							while (i < brokenSprites.Count && brokenSprites[i].MaxConditionPercentage < brokenSprite.MaxConditionPercentage)
							{
								spriteIndex = i;
								i++;
							}
							brokenSprites.Insert(spriteIndex, brokenSprite);
						}
						break;
					case 13:
						if (text == "inventoryicon")
						{
							string iconFolder2 = this.GetTexturePath(subElement, variantOf);
							this.InventoryIcon = new Sprite(subElement, iconFolder2, "", true, 1f);
						}
						break;
					case 14:
						if (text == "infectedsprite")
						{
							string iconFolder3 = this.GetTexturePath(subElement, variantOf);
							this.InfectedSprite = new Sprite(subElement, iconFolder3, "", true, 1f);
						}
						break;
					case 15:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c == 'u')
							{
								if (text == "upgradeoverride")
								{
									List<DecorativeSprite> sprites = new List<DecorativeSprite>();
									foreach (ContentXElement decorSprite in subElement.Elements())
									{
										Identifier key = decorSprite.NameAsIdentifier();
										if (key == "decorativesprite")
										{
											sprites.Add(new DecorativeSprite(decorSprite, "", "", false));
										}
									}
									upgradeOverrideSprites.Add(subElement.GetAttributeIdentifier("identifier", Identifier.Empty), sprites);
								}
							}
						}
						else if (text == "containedsprite")
						{
							string containedSpriteFolder = this.GetTexturePath(subElement, variantOf);
							ContainedItemSprite containedSprite = new ContainedItemSprite(subElement, containedSpriteFolder, true);
							if (containedSprite.Sprite != null)
							{
								containedSprites.Add(containedSprite);
							}
						}
						break;
					}
					case 16:
						if (text == "decorativesprite")
						{
							string decorativeSpriteFolder = this.GetTexturePath(subElement, variantOf);
							DecorativeSprite decorativeSprite = null;
							int groupID;
							if (subElement.GetAttribute("texture") == null)
							{
								groupID = subElement.GetAttributeInt("randomgroupid", 0);
							}
							else
							{
								decorativeSprite = new DecorativeSprite(subElement, decorativeSpriteFolder, "", true);
								decorativeSprites.Add(decorativeSprite);
								groupID = decorativeSprite.RandomGroupID;
							}
							if (!decorativeSpriteGroups.ContainsKey(groupID))
							{
								decorativeSpriteGroups.Add(groupID, new List<DecorativeSprite>());
							}
							decorativeSpriteGroups[groupID].Add(decorativeSprite);
						}
						break;
					case 20:
						if (text == "upgradepreviewsprite")
						{
							string iconFolder4 = this.GetTexturePath(subElement, variantOf);
							this.UpgradePreviewSprite = new Sprite(subElement, iconFolder4, "", true, 1f);
							this.UpgradePreviewScale = subElement.GetAttributeFloat("scale", 1f);
						}
						break;
					case 21:
						if (text == "damagedinfectedsprite")
						{
							string iconFolder5 = this.GetTexturePath(subElement, variantOf);
							this.DamagedInfectedSprite = new Sprite(subElement, iconFolder5, "", true, 1f);
						}
						break;
					}
				}
			}
			this.wearableDamageModifiers = wearableDamageModifiers.ToImmutableList<DamageModifier>();
			this.wearableSkillModifiers = wearableSkillModifiers.ToImmutableDictionary<Identifier, float>();
			this.UpgradeOverrideSprites = (from kvp in upgradeOverrideSprites
			select new ValueTuple<Identifier, ImmutableArray<DecorativeSprite>>(kvp.Key, kvp.Value.ToImmutableArray<DecorativeSprite>())).ToImmutableDictionary<Identifier, ImmutableArray<DecorativeSprite>>();
			this.BrokenSprites = brokenSprites.ToImmutableArray<BrokenItemSprite>();
			this.DecorativeSprites = decorativeSprites.ToImmutableArray<DecorativeSprite>();
			this.ContainedSprites = containedSprites.ToImmutableArray<ContainedItemSprite>();
			this.DecorativeSpriteGroups = (from kvp in decorativeSpriteGroups
			select new ValueTuple<int, ImmutableArray<DecorativeSprite>>(kvp.Key, kvp.Value.ToImmutableArray<DecorativeSprite>())).ToImmutableDictionary<int, ImmutableArray<DecorativeSprite>>();
			foreach (Item item in Item.ItemList)
			{
				if (item.Prefab == this)
				{
					item.InitSpriteStates();
				}
			}
		}

		// Token: 0x06001A0D RID: 6669 RVA: 0x00107300 File Offset: 0x00105500
		public bool CanCharacterBuy()
		{
			return this.DefaultPrice != null && (!this.DefaultPrice.RequiresUnlock || (Character.Controlled != null && Character.Controlled.HasStoreAccessForItem(this)));
		}

		// Token: 0x06001A0E RID: 6670 RVA: 0x00107330 File Offset: 0x00105530
		public LocalizedString GetTooltip(Character character)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler.AppendFormatted(GUIStyle.TextColorBright.ToStringHex());
			defaultInterpolatedStringHandler.AppendLiteral("‖");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
			LocalizedString tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
			if (!base.Description.IsNullOrEmpty())
			{
				LocalizedString left = tooltip;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("\n");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(base.Description);
				tooltip = left + defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			if (this.wearableDamageModifiers.Any<DamageModifier>() || this.wearableSkillModifiers.Any<KeyValuePair<Identifier, float>>())
			{
				Wearable.AddTooltipInfo(this.wearableDamageModifiers, this.wearableSkillModifiers, ref tooltip);
			}
			if (new ImmutableArray<SkillRequirementHint>?(this.SkillRequirementHints) != null && this.SkillRequirementHints.Any<SkillRequirementHint>())
			{
				tooltip += this.GetSkillRequirementHints(character);
			}
			return tooltip;
		}

		// Token: 0x06001A0F RID: 6671 RVA: 0x00107444 File Offset: 0x00105644
		public override void UpdatePlacing(Camera cam)
		{
			if (PlayerInput.SecondaryMouseButtonClicked())
			{
				MapEntityPrefab.Selected = null;
				return;
			}
			Item potentialContainer = MapEntity.GetPotentialContainer(cam.ScreenToWorld(PlayerInput.MousePosition), null);
			Vector2 position = Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false);
			if (!base.ResizeHorizontal && !base.ResizeVertical)
			{
				if (PlayerInput.PrimaryMouseButtonClicked() && GUI.MouseOn == null)
				{
					Item item = new Item(new Rectangle((int)position.X, (int)position.Y, (int)(this.Sprite.size.X * base.Scale), (int)(this.Sprite.size.Y * base.Scale)), this, Submarine.MainSub, true, 0)
					{
						Submarine = Submarine.MainSub
					};
					item.SetTransform(ConvertUnits.ToSimUnits((Submarine.MainSub == null) ? item.Position : (item.Position - Submarine.MainSub.Position)), 0f, true, true, null);
					Door component = item.GetComponent<Door>();
					if (component != null)
					{
						component.RefreshLinkedGap();
					}
					item.FindHull();
					item.Submarine = Submarine.MainSub;
					if (PlayerInput.IsShiftDown())
					{
						bool? flag;
						if (potentialContainer == null)
						{
							flag = null;
						}
						else
						{
							ItemInventory ownInventory = potentialContainer.OwnInventory;
							flag = ((ownInventory != null) ? new bool?(ownInventory.TryPutItem(item, Character.Controlled, null, true, false, true)) : null);
						}
						bool? flag2 = flag;
						if (flag2.GetValueOrDefault())
						{
							SoundPlayer.PlayUISound(GUISoundType.PickItem);
						}
					}
					SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>
					{
						item
					}, false, true));
					MapEntityPrefab.placePosition = Vector2.Zero;
					return;
				}
			}
			else
			{
				Vector2 placeSize = this.Size * base.Scale;
				if (MapEntityPrefab.placePosition == Vector2.Zero)
				{
					if (PlayerInput.PrimaryMouseButtonHeld() && GUI.MouseOn == null)
					{
						MapEntityPrefab.placePosition = position;
					}
				}
				else
				{
					if (base.ResizeHorizontal)
					{
						placeSize.X = Math.Max(position.X - MapEntityPrefab.placePosition.X, this.Size.X);
					}
					if (base.ResizeVertical)
					{
						placeSize.Y = Math.Max(MapEntityPrefab.placePosition.Y - position.Y, this.Size.Y);
					}
					if (PlayerInput.PrimaryMouseButtonReleased())
					{
						Item item2 = new Item(new Rectangle((int)MapEntityPrefab.placePosition.X, (int)MapEntityPrefab.placePosition.Y, (int)placeSize.X, (int)placeSize.Y), this, Submarine.MainSub, true, 0);
						MapEntityPrefab.placePosition = Vector2.Zero;
						item2.Submarine = Submarine.MainSub;
						item2.SetTransform(ConvertUnits.ToSimUnits((Submarine.MainSub == null) ? item2.Position : (item2.Position - Submarine.MainSub.Position)), 0f, true, true, null);
						item2.FindHull();
						SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>
						{
							item2
						}, false, true));
						return;
					}
				}
			}
			if (potentialContainer != null)
			{
				potentialContainer.IsHighlighted = true;
			}
		}

		// Token: 0x06001A10 RID: 6672 RVA: 0x00107740 File Offset: 0x00105940
		public override void DrawPlacing(SpriteBatch spriteBatch, Camera cam)
		{
			Vector2 position = Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false);
			if (!base.ResizeHorizontal && !base.ResizeVertical)
			{
				this.Sprite.Draw(spriteBatch, new Vector2(position.X, -position.Y) + this.Sprite.size / 2f * base.Scale, base.SpriteColor, 0f, base.Scale, SpriteEffects.None, null);
				return;
			}
			Vector2 placeSize = this.Size * base.Scale;
			if (MapEntityPrefab.placePosition != Vector2.Zero)
			{
				if (base.ResizeHorizontal)
				{
					placeSize.X = Math.Max(position.X - MapEntityPrefab.placePosition.X, placeSize.X);
				}
				if (base.ResizeVertical)
				{
					placeSize.Y = Math.Max(MapEntityPrefab.placePosition.Y - position.Y, placeSize.Y);
				}
				position = MapEntityPrefab.placePosition;
			}
			Sprite sprite = this.Sprite;
			if (sprite == null)
			{
				return;
			}
			Vector2 position2 = new Vector2(position.X, -position.Y);
			Vector2 targetSize = placeSize;
			float rotation = 0f;
			Color? color = new Color?(base.SpriteColor);
			sprite.DrawTiled(spriteBatch, position2, targetSize, rotation, null, color, null, null, null);
		}

		// Token: 0x06001A11 RID: 6673 RVA: 0x001078B0 File Offset: 0x00105AB0
		public override void DrawPlacing(SpriteBatch spriteBatch, Rectangle placeRect, float scale = 1f, float rotation = 0f, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			if (!base.ResizeHorizontal && !base.ResizeVertical)
			{
				this.sprite.Draw(spriteBatch, new Vector2((float)placeRect.Center.X, (float)(-(float)(placeRect.Y - placeRect.Height / 2))), base.SpriteColor * 0.8f, rotation, scale, spriteEffects ^ this.sprite.effects, null);
				return;
			}
			Vector2 position = placeRect.Location.ToVector2();
			Vector2 placeSize = placeRect.Size.ToVector2();
			Sprite sprite = this.sprite;
			if (sprite == null)
			{
				return;
			}
			Vector2 position2 = new Vector2(position.X, -position.Y);
			Vector2 targetSize = placeSize;
			Vector2? textureScale = new Vector2?(Vector2.One * scale);
			Color? color = new Color?(base.SpriteColor * 0.8f);
			sprite.DrawTiled(spriteBatch, position2, targetSize, spriteEffects ^ this.sprite.effects, rotation, null, color, null, textureScale, null);
		}

		// Token: 0x06001A12 RID: 6674 RVA: 0x001079CC File Offset: 0x00105BCC
		public LocalizedString GetSkillRequirementHints(Character character)
		{
			LocalizedString text = "";
			if (new ImmutableArray<SkillRequirementHint>?(this.SkillRequirementHints) != null && this.SkillRequirementHints.Any<SkillRequirementHint>() && character != null)
			{
				Color orange = GUIStyle.Orange;
				string str = "\n\n";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler.AppendFormatted(orange.ToStringHex());
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("requiredrepairskills"));
				defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
				text = str + defaultInterpolatedStringHandler.ToStringAndClear();
				foreach (SkillRequirementHint hint in this.SkillRequirementHints)
				{
					int skillLevel = (int)character.GetSkillLevel(hint.Skill);
					Color levelColor = GUIStyle.Yellow;
					if ((float)skillLevel >= hint.Level)
					{
						levelColor = GUIStyle.Green;
					}
					else if ((float)skillLevel < hint.Level / 2f)
					{
						levelColor = GUIStyle.Red;
					}
					text += "\n" + hint.GetFormattedText(skillLevel, levelColor.ToStringHex());
				}
			}
			return text;
		}

		// Token: 0x17000682 RID: 1666
		// (get) Token: 0x06001A13 RID: 6675 RVA: 0x00107B2C File Offset: 0x00105D2C
		// (set) Token: 0x06001A14 RID: 6676 RVA: 0x00107B34 File Offset: 0x00105D34
		public Vector2 Size { get; private set; }

		// Token: 0x17000683 RID: 1667
		// (get) Token: 0x06001A15 RID: 6677 RVA: 0x00107B3D File Offset: 0x00105D3D
		public PriceInfo DefaultPrice
		{
			get
			{
				return this.defaultPrice;
			}
		}

		// Token: 0x17000684 RID: 1668
		// (get) Token: 0x06001A16 RID: 6678 RVA: 0x00107B45 File Offset: 0x00105D45
		// (set) Token: 0x06001A17 RID: 6679 RVA: 0x00107B4D File Offset: 0x00105D4D
		private ImmutableDictionary<Identifier, PriceInfo> StorePrices { get; set; }

		// Token: 0x17000685 RID: 1669
		// (get) Token: 0x06001A18 RID: 6680 RVA: 0x00107B58 File Offset: 0x00105D58
		public bool CanBeBought
		{
			get
			{
				if (this.DefaultPrice != null && this.DefaultPrice.CanBeBought)
				{
					return true;
				}
				if (this.StorePrices != null)
				{
					return this.StorePrices.Any((KeyValuePair<Identifier, PriceInfo> p) => p.Value.CanBeBought);
				}
				return false;
			}
		}

		// Token: 0x17000686 RID: 1670
		// (get) Token: 0x06001A19 RID: 6681 RVA: 0x00107BB0 File Offset: 0x00105DB0
		public bool CanBeSold
		{
			get
			{
				return this.DefaultPrice != null;
			}
		}

		// Token: 0x17000687 RID: 1671
		// (get) Token: 0x06001A1A RID: 6682 RVA: 0x00107BBB File Offset: 0x00105DBB
		// (set) Token: 0x06001A1B RID: 6683 RVA: 0x00107BC3 File Offset: 0x00105DC3
		public ImmutableArray<Rectangle> Triggers { get; private set; }

		// Token: 0x17000688 RID: 1672
		// (get) Token: 0x06001A1C RID: 6684 RVA: 0x00107BCC File Offset: 0x00105DCC
		public bool IsOverride
		{
			get
			{
				return ItemPrefab.Prefabs.IsOverride(this);
			}
		}

		// Token: 0x17000689 RID: 1673
		// (get) Token: 0x06001A1D RID: 6685 RVA: 0x00107BD9 File Offset: 0x00105DD9
		// (set) Token: 0x06001A1E RID: 6686 RVA: 0x00107BE1 File Offset: 0x00105DE1
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x1700068A RID: 1674
		// (get) Token: 0x06001A1F RID: 6687 RVA: 0x00107BEA File Offset: 0x00105DEA
		// (set) Token: 0x06001A20 RID: 6688 RVA: 0x00107BF2 File Offset: 0x00105DF2
		public ImmutableArray<DeconstructItem> DeconstructItems { get; private set; }

		// Token: 0x1700068B RID: 1675
		// (get) Token: 0x06001A21 RID: 6689 RVA: 0x00107BFB File Offset: 0x00105DFB
		// (set) Token: 0x06001A22 RID: 6690 RVA: 0x00107C03 File Offset: 0x00105E03
		public ImmutableDictionary<uint, FabricationRecipe> FabricationRecipes { get; private set; }

		// Token: 0x1700068C RID: 1676
		// (get) Token: 0x06001A23 RID: 6691 RVA: 0x00107C0C File Offset: 0x00105E0C
		// (set) Token: 0x06001A24 RID: 6692 RVA: 0x00107C14 File Offset: 0x00105E14
		public float DeconstructTime { get; private set; }

		// Token: 0x1700068D RID: 1677
		// (get) Token: 0x06001A25 RID: 6693 RVA: 0x00107C1D File Offset: 0x00105E1D
		// (set) Token: 0x06001A26 RID: 6694 RVA: 0x00107C25 File Offset: 0x00105E25
		public float DeconstructTimeInOutposts { get; private set; }

		// Token: 0x1700068E RID: 1678
		// (get) Token: 0x06001A27 RID: 6695 RVA: 0x00107C2E File Offset: 0x00105E2E
		// (set) Token: 0x06001A28 RID: 6696 RVA: 0x00107C36 File Offset: 0x00105E36
		public bool AllowDeconstruct { get; private set; }

		// Token: 0x1700068F RID: 1679
		// (get) Token: 0x06001A29 RID: 6697 RVA: 0x00107C3F File Offset: 0x00105E3F
		// (set) Token: 0x06001A2A RID: 6698 RVA: 0x00107C47 File Offset: 0x00105E47
		public ImmutableArray<PreferredContainer> PreferredContainers { get; private set; }

		// Token: 0x17000690 RID: 1680
		// (get) Token: 0x06001A2B RID: 6699 RVA: 0x00107C50 File Offset: 0x00105E50
		// (set) Token: 0x06001A2C RID: 6700 RVA: 0x00107C58 File Offset: 0x00105E58
		public ImmutableArray<SkillRequirementHint> SkillRequirementHints { get; private set; }

		// Token: 0x17000691 RID: 1681
		// (get) Token: 0x06001A2D RID: 6701 RVA: 0x00107C61 File Offset: 0x00105E61
		// (set) Token: 0x06001A2E RID: 6702 RVA: 0x00107C69 File Offset: 0x00105E69
		public SwappableItem SwappableItem { get; private set; }

		// Token: 0x17000692 RID: 1682
		// (get) Token: 0x06001A2F RID: 6703 RVA: 0x00107C72 File Offset: 0x00105E72
		// (set) Token: 0x06001A30 RID: 6704 RVA: 0x00107C7A File Offset: 0x00105E7A
		private ImmutableDictionary<Identifier, ItemPrefab.CommonnessInfo> LevelCommonness { get; set; }

		// Token: 0x17000693 RID: 1683
		// (get) Token: 0x06001A31 RID: 6705 RVA: 0x00107C83 File Offset: 0x00105E83
		// (set) Token: 0x06001A32 RID: 6706 RVA: 0x00107C8B File Offset: 0x00105E8B
		public ImmutableDictionary<Identifier, ItemPrefab.FixedQuantityResourceInfo> LevelQuantity { get; private set; }

		// Token: 0x17000694 RID: 1684
		// (get) Token: 0x06001A33 RID: 6707 RVA: 0x00107C94 File Offset: 0x00105E94
		public override bool CanSpriteFlipX
		{
			get
			{
				return this.canSpriteFlipX;
			}
		}

		// Token: 0x17000695 RID: 1685
		// (get) Token: 0x06001A34 RID: 6708 RVA: 0x00107C9C File Offset: 0x00105E9C
		public override bool CanSpriteFlipY
		{
			get
			{
				return this.canSpriteFlipY;
			}
		}

		// Token: 0x17000696 RID: 1686
		// (get) Token: 0x06001A35 RID: 6709 RVA: 0x00107CA4 File Offset: 0x00105EA4
		// (set) Token: 0x06001A36 RID: 6710 RVA: 0x00107CAC File Offset: 0x00105EAC
		public bool? AllowAsExtraCargo { get; private set; }

		// Token: 0x17000697 RID: 1687
		// (get) Token: 0x06001A37 RID: 6711 RVA: 0x00107CB5 File Offset: 0x00105EB5
		// (set) Token: 0x06001A38 RID: 6712 RVA: 0x00107CBD File Offset: 0x00105EBD
		public bool RandomDeconstructionOutput { get; private set; }

		// Token: 0x17000698 RID: 1688
		// (get) Token: 0x06001A39 RID: 6713 RVA: 0x00107CC6 File Offset: 0x00105EC6
		// (set) Token: 0x06001A3A RID: 6714 RVA: 0x00107CCE File Offset: 0x00105ECE
		public int RandomDeconstructionOutputAmount { get; private set; }

		// Token: 0x17000699 RID: 1689
		// (get) Token: 0x06001A3B RID: 6715 RVA: 0x00107CD7 File Offset: 0x00105ED7
		public override Sprite Sprite
		{
			get
			{
				return this.sprite;
			}
		}

		// Token: 0x1700069A RID: 1690
		// (get) Token: 0x06001A3C RID: 6716 RVA: 0x00107CDF File Offset: 0x00105EDF
		public override string OriginalName { get; }

		// Token: 0x1700069B RID: 1691
		// (get) Token: 0x06001A3D RID: 6717 RVA: 0x00107CE7 File Offset: 0x00105EE7
		public override LocalizedString Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x1700069C RID: 1692
		// (get) Token: 0x06001A3E RID: 6718 RVA: 0x00107CEF File Offset: 0x00105EEF
		public override ImmutableHashSet<Identifier> Tags
		{
			get
			{
				return this.tags;
			}
		}

		// Token: 0x1700069D RID: 1693
		// (get) Token: 0x06001A3F RID: 6719 RVA: 0x00107CF7 File Offset: 0x00105EF7
		public override ImmutableHashSet<Identifier> AllowedLinks
		{
			get
			{
				return this.allowedLinks;
			}
		}

		// Token: 0x1700069E RID: 1694
		// (get) Token: 0x06001A40 RID: 6720 RVA: 0x00107CFF File Offset: 0x00105EFF
		public override MapEntityCategory Category
		{
			get
			{
				return this.category;
			}
		}

		// Token: 0x1700069F RID: 1695
		// (get) Token: 0x06001A41 RID: 6721 RVA: 0x00107D07 File Offset: 0x00105F07
		public override ImmutableHashSet<string> Aliases
		{
			get
			{
				return this.aliases;
			}
		}

		// Token: 0x170006A0 RID: 1696
		// (get) Token: 0x06001A42 RID: 6722 RVA: 0x00107D0F File Offset: 0x00105F0F
		// (set) Token: 0x06001A43 RID: 6723 RVA: 0x00107D17 File Offset: 0x00105F17
		[Serialize(120f, IsPropertySaveable.No, "", "", false)]
		public float InteractDistance { get; private set; }

		// Token: 0x170006A1 RID: 1697
		// (get) Token: 0x06001A44 RID: 6724 RVA: 0x00107D20 File Offset: 0x00105F20
		// (set) Token: 0x06001A45 RID: 6725 RVA: 0x00107D28 File Offset: 0x00105F28
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float InteractPriority { get; private set; }

		// Token: 0x170006A2 RID: 1698
		// (get) Token: 0x06001A46 RID: 6726 RVA: 0x00107D31 File Offset: 0x00105F31
		// (set) Token: 0x06001A47 RID: 6727 RVA: 0x00107D39 File Offset: 0x00105F39
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool InteractThroughWalls { get; private set; }

		// Token: 0x170006A3 RID: 1699
		// (get) Token: 0x06001A48 RID: 6728 RVA: 0x00107D42 File Offset: 0x00105F42
		// (set) Token: 0x06001A49 RID: 6729 RVA: 0x00107D4A File Offset: 0x00105F4A
		[Serialize(false, IsPropertySaveable.No, "Hides the condition bar displayed at the bottom of the inventory slot the item is in.", "", false)]
		public bool HideConditionBar { get; set; }

		// Token: 0x170006A4 RID: 1700
		// (get) Token: 0x06001A4A RID: 6730 RVA: 0x00107D53 File Offset: 0x00105F53
		// (set) Token: 0x06001A4B RID: 6731 RVA: 0x00107D5B File Offset: 0x00105F5B
		[Serialize(false, IsPropertySaveable.No, "Hides the condition displayed in the item's tooltip.", "", false)]
		public bool HideConditionInTooltip { get; set; }

		// Token: 0x170006A5 RID: 1701
		// (get) Token: 0x06001A4C RID: 6732 RVA: 0x00107D64 File Offset: 0x00105F64
		// (set) Token: 0x06001A4D RID: 6733 RVA: 0x00107D6C File Offset: 0x00105F6C
		[Serialize("", IsPropertySaveable.No, "If set, the item's tooltip displays if the given fabrication recipe has been unlocked or not. The actual unlocking of the recipe should be handled in a status effect.", "", false)]
		public Identifier[] UnlockedRecipeInToolTip { get; set; }

		// Token: 0x170006A6 RID: 1702
		// (get) Token: 0x06001A4E RID: 6734 RVA: 0x00107D75 File Offset: 0x00105F75
		// (set) Token: 0x06001A4F RID: 6735 RVA: 0x00107D7D File Offset: 0x00105F7D
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool RequireBodyInsideTrigger { get; private set; }

		// Token: 0x170006A7 RID: 1703
		// (get) Token: 0x06001A50 RID: 6736 RVA: 0x00107D86 File Offset: 0x00105F86
		// (set) Token: 0x06001A51 RID: 6737 RVA: 0x00107D8E File Offset: 0x00105F8E
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool RequireCursorInsideTrigger { get; private set; }

		// Token: 0x170006A8 RID: 1704
		// (get) Token: 0x06001A52 RID: 6738 RVA: 0x00107D97 File Offset: 0x00105F97
		// (set) Token: 0x06001A53 RID: 6739 RVA: 0x00107D9F File Offset: 0x00105F9F
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool RequireCampaignInteract { get; private set; }

		// Token: 0x170006A9 RID: 1705
		// (get) Token: 0x06001A54 RID: 6740 RVA: 0x00107DA8 File Offset: 0x00105FA8
		// (set) Token: 0x06001A55 RID: 6741 RVA: 0x00107DB0 File Offset: 0x00105FB0
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool FocusOnSelected { get; private set; }

		// Token: 0x170006AA RID: 1706
		// (get) Token: 0x06001A56 RID: 6742 RVA: 0x00107DB9 File Offset: 0x00105FB9
		// (set) Token: 0x06001A57 RID: 6743 RVA: 0x00107DC1 File Offset: 0x00105FC1
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float OffsetOnSelected { get; private set; }

		// Token: 0x170006AB RID: 1707
		// (get) Token: 0x06001A58 RID: 6744 RVA: 0x00107DCA File Offset: 0x00105FCA
		// (set) Token: 0x06001A59 RID: 6745 RVA: 0x00107DD2 File Offset: 0x00105FD2
		[Serialize(false, IsPropertySaveable.No, "Should the character who's selected the item grab it (hold their hand on it, the same way as they do when repairing)? Defaults to true on items that have an ItemContainer component.", "", false)]
		public bool GrabWhenSelected { get; set; }

		// Token: 0x170006AC RID: 1708
		// (get) Token: 0x06001A5A RID: 6746 RVA: 0x00107DDB File Offset: 0x00105FDB
		// (set) Token: 0x06001A5B RID: 6747 RVA: 0x00107DE3 File Offset: 0x00105FE3
		[Serialize(true, IsPropertySaveable.No, "Are AI characters allowed to deselect the item when they're idling (and wander off?).", "", false)]
		public bool AllowDeselectWhenIdling { get; private set; }

		// Token: 0x170006AD RID: 1709
		// (get) Token: 0x06001A5C RID: 6748 RVA: 0x00107DEC File Offset: 0x00105FEC
		// (set) Token: 0x06001A5D RID: 6749 RVA: 0x00107DF4 File Offset: 0x00105FF4
		[Serialize(100f, IsPropertySaveable.No, "", "", false)]
		public float Health
		{
			get
			{
				return this.health;
			}
			private set
			{
				this.health = Math.Min(value, 1000000f);
			}
		}

		// Token: 0x170006AE RID: 1710
		// (get) Token: 0x06001A5E RID: 6750 RVA: 0x00107E07 File Offset: 0x00106007
		// (set) Token: 0x06001A5F RID: 6751 RVA: 0x00107E0F File Offset: 0x0010600F
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowSellingWhenBroken { get; private set; }

		// Token: 0x170006AF RID: 1711
		// (get) Token: 0x06001A60 RID: 6752 RVA: 0x00107E18 File Offset: 0x00106018
		// (set) Token: 0x06001A61 RID: 6753 RVA: 0x00107E20 File Offset: 0x00106020
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowStealingAlways { get; private set; }

		// Token: 0x170006B0 RID: 1712
		// (get) Token: 0x06001A62 RID: 6754 RVA: 0x00107E29 File Offset: 0x00106029
		// (set) Token: 0x06001A63 RID: 6755 RVA: 0x00107E31 File Offset: 0x00106031
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool Indestructible { get; private set; }

		// Token: 0x170006B1 RID: 1713
		// (get) Token: 0x06001A64 RID: 6756 RVA: 0x00107E3A File Offset: 0x0010603A
		// (set) Token: 0x06001A65 RID: 6757 RVA: 0x00107E42 File Offset: 0x00106042
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByExplosions { get; private set; }

		// Token: 0x170006B2 RID: 1714
		// (get) Token: 0x06001A66 RID: 6758 RVA: 0x00107E4B File Offset: 0x0010604B
		// (set) Token: 0x06001A67 RID: 6759 RVA: 0x00107E53 File Offset: 0x00106053
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByContainedItemExplosions { get; private set; }

		// Token: 0x170006B3 RID: 1715
		// (get) Token: 0x06001A68 RID: 6760 RVA: 0x00107E5C File Offset: 0x0010605C
		// (set) Token: 0x06001A69 RID: 6761 RVA: 0x00107E64 File Offset: 0x00106064
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float ExplosionDamageMultiplier { get; private set; }

		// Token: 0x170006B4 RID: 1716
		// (get) Token: 0x06001A6A RID: 6762 RVA: 0x00107E6D File Offset: 0x0010606D
		// (set) Token: 0x06001A6B RID: 6763 RVA: 0x00107E75 File Offset: 0x00106075
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float ItemDamageMultiplier { get; private set; }

		// Token: 0x170006B5 RID: 1717
		// (get) Token: 0x06001A6C RID: 6764 RVA: 0x00107E7E File Offset: 0x0010607E
		// (set) Token: 0x06001A6D RID: 6765 RVA: 0x00107E86 File Offset: 0x00106086
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByProjectiles { get; private set; }

		// Token: 0x170006B6 RID: 1718
		// (get) Token: 0x06001A6E RID: 6766 RVA: 0x00107E8F File Offset: 0x0010608F
		// (set) Token: 0x06001A6F RID: 6767 RVA: 0x00107E97 File Offset: 0x00106097
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByMeleeWeapons { get; private set; }

		// Token: 0x170006B7 RID: 1719
		// (get) Token: 0x06001A70 RID: 6768 RVA: 0x00107EA0 File Offset: 0x001060A0
		// (set) Token: 0x06001A71 RID: 6769 RVA: 0x00107EA8 File Offset: 0x001060A8
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByRepairTools { get; private set; }

		// Token: 0x170006B8 RID: 1720
		// (get) Token: 0x06001A72 RID: 6770 RVA: 0x00107EB1 File Offset: 0x001060B1
		// (set) Token: 0x06001A73 RID: 6771 RVA: 0x00107EB9 File Offset: 0x001060B9
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByMonsters { get; private set; }

		// Token: 0x170006B9 RID: 1721
		// (get) Token: 0x06001A74 RID: 6772 RVA: 0x00107EC2 File Offset: 0x001060C2
		// (set) Token: 0x06001A75 RID: 6773 RVA: 0x00107ECA File Offset: 0x001060CA
		[Serialize(false, IsPropertySaveable.No, "If true, submarine impacts will trigger OnImpact effects. Only applies to items with a null or non-dynamic physics body - items with dynamic bodies always react to impacts.", "", false)]
		public bool ReceiveSubmarineImpacts { get; set; }

		// Token: 0x170006BA RID: 1722
		// (get) Token: 0x06001A76 RID: 6774 RVA: 0x00107ED3 File Offset: 0x001060D3
		// (set) Token: 0x06001A77 RID: 6775 RVA: 0x00107EDB File Offset: 0x001060DB
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float OnDamagedThreshold { get; set; }

		// Token: 0x170006BB RID: 1723
		// (get) Token: 0x06001A78 RID: 6776 RVA: 0x00107EE4 File Offset: 0x001060E4
		// (set) Token: 0x06001A79 RID: 6777 RVA: 0x00107EEC File Offset: 0x001060EC
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float SonarSize { get; private set; }

		// Token: 0x170006BC RID: 1724
		// (get) Token: 0x06001A7A RID: 6778 RVA: 0x00107EF5 File Offset: 0x001060F5
		// (set) Token: 0x06001A7B RID: 6779 RVA: 0x00107EFD File Offset: 0x001060FD
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool UseInHealthInterface { get; private set; }

		// Token: 0x170006BD RID: 1725
		// (get) Token: 0x06001A7C RID: 6780 RVA: 0x00107F06 File Offset: 0x00106106
		// (set) Token: 0x06001A7D RID: 6781 RVA: 0x00107F0E File Offset: 0x0010610E
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DisableItemUsageWhenSelected { get; private set; }

		// Token: 0x170006BE RID: 1726
		// (get) Token: 0x06001A7E RID: 6782 RVA: 0x00107F17 File Offset: 0x00106117
		// (set) Token: 0x06001A7F RID: 6783 RVA: 0x00107F1F File Offset: 0x0010611F
		[Serialize("metalcrate", IsPropertySaveable.No, "", "", false)]
		public string CargoContainerIdentifier { get; private set; }

		// Token: 0x170006BF RID: 1727
		// (get) Token: 0x06001A80 RID: 6784 RVA: 0x00107F28 File Offset: 0x00106128
		// (set) Token: 0x06001A81 RID: 6785 RVA: 0x00107F30 File Offset: 0x00106130
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool UseContainedSpriteColor { get; private set; }

		// Token: 0x170006C0 RID: 1728
		// (get) Token: 0x06001A82 RID: 6786 RVA: 0x00107F39 File Offset: 0x00106139
		// (set) Token: 0x06001A83 RID: 6787 RVA: 0x00107F41 File Offset: 0x00106141
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool UseContainedInventoryIconColor { get; private set; }

		// Token: 0x170006C1 RID: 1729
		// (get) Token: 0x06001A84 RID: 6788 RVA: 0x00107F4A File Offset: 0x0010614A
		// (set) Token: 0x06001A85 RID: 6789 RVA: 0x00107F52 File Offset: 0x00106152
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float AddedRepairSpeedMultiplier { get; private set; }

		// Token: 0x170006C2 RID: 1730
		// (get) Token: 0x06001A86 RID: 6790 RVA: 0x00107F5B File Offset: 0x0010615B
		// (set) Token: 0x06001A87 RID: 6791 RVA: 0x00107F63 File Offset: 0x00106163
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float AddedPickingSpeedMultiplier { get; private set; }

		// Token: 0x170006C3 RID: 1731
		// (get) Token: 0x06001A88 RID: 6792 RVA: 0x00107F6C File Offset: 0x0010616C
		// (set) Token: 0x06001A89 RID: 6793 RVA: 0x00107F74 File Offset: 0x00106174
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool CannotRepairFail { get; private set; }

		// Token: 0x170006C4 RID: 1732
		// (get) Token: 0x06001A8A RID: 6794 RVA: 0x00107F7D File Offset: 0x0010617D
		// (set) Token: 0x06001A8B RID: 6795 RVA: 0x00107F85 File Offset: 0x00106185
		[Serialize(null, IsPropertySaveable.No, "", "", false)]
		public string EquipConfirmationText { get; set; }

		// Token: 0x170006C5 RID: 1733
		// (get) Token: 0x06001A8C RID: 6796 RVA: 0x00107F8E File Offset: 0x0010618E
		// (set) Token: 0x06001A8D RID: 6797 RVA: 0x00107F96 File Offset: 0x00106196
		[Serialize(true, IsPropertySaveable.No, "Can the item be rotated in the submarine editor?", "", false)]
		public bool AllowRotatingInEditor { get; set; }

		// Token: 0x170006C6 RID: 1734
		// (get) Token: 0x06001A8E RID: 6798 RVA: 0x00107F9F File Offset: 0x0010619F
		// (set) Token: 0x06001A8F RID: 6799 RVA: 0x00107FA7 File Offset: 0x001061A7
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool ShowContentsInTooltip { get; private set; }

		// Token: 0x170006C7 RID: 1735
		// (get) Token: 0x06001A90 RID: 6800 RVA: 0x00107FB0 File Offset: 0x001061B0
		// (set) Token: 0x06001A91 RID: 6801 RVA: 0x00107FB8 File Offset: 0x001061B8
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool CanFlipX { get; private set; }

		// Token: 0x170006C8 RID: 1736
		// (get) Token: 0x06001A92 RID: 6802 RVA: 0x00107FC1 File Offset: 0x001061C1
		// (set) Token: 0x06001A93 RID: 6803 RVA: 0x00107FC9 File Offset: 0x001061C9
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool CanFlipY { get; private set; }

		// Token: 0x170006C9 RID: 1737
		// (get) Token: 0x06001A94 RID: 6804 RVA: 0x00107FD2 File Offset: 0x001061D2
		// (set) Token: 0x06001A95 RID: 6805 RVA: 0x00107FDA File Offset: 0x001061DA
		[Serialize(0.01f, IsPropertySaveable.No, "", "", false)]
		public float MinScale { get; private set; }

		// Token: 0x170006CA RID: 1738
		// (get) Token: 0x06001A96 RID: 6806 RVA: 0x00107FE3 File Offset: 0x001061E3
		// (set) Token: 0x06001A97 RID: 6807 RVA: 0x00107FEB File Offset: 0x001061EB
		[Serialize(10f, IsPropertySaveable.No, "", "", false)]
		public float MaxScale { get; private set; }

		// Token: 0x170006CB RID: 1739
		// (get) Token: 0x06001A98 RID: 6808 RVA: 0x00107FF4 File Offset: 0x001061F4
		// (set) Token: 0x06001A99 RID: 6809 RVA: 0x00107FFC File Offset: 0x001061FC
		[Serialize(false, IsPropertySaveable.No, "Bots avoid rooms with dangerous items in them.", "", false)]
		public bool IsDangerous { get; private set; }

		// Token: 0x170006CC RID: 1740
		// (get) Token: 0x06001A9A RID: 6810 RVA: 0x00108005 File Offset: 0x00106205
		// (set) Token: 0x06001A9B RID: 6811 RVA: 0x0010800D File Offset: 0x0010620D
		[Serialize(1, IsPropertySaveable.No, "", "", false)]
		public int MaxStackSize
		{
			get
			{
				return this.maxStackSize;
			}
			private set
			{
				this.maxStackSize = MathHelper.Clamp(value, 1, 63);
			}
		}

		// Token: 0x170006CD RID: 1741
		// (get) Token: 0x06001A9C RID: 6812 RVA: 0x0010801E File Offset: 0x0010621E
		// (set) Token: 0x06001A9D RID: 6813 RVA: 0x00108026 File Offset: 0x00106226
		[Serialize(-1, IsPropertySaveable.No, "Maximum stack size when the item is in a character inventory.", "", false)]
		public int MaxStackSizeCharacterInventory
		{
			get
			{
				return this.maxStackSizeCharacterInventory;
			}
			private set
			{
				this.maxStackSizeCharacterInventory = Math.Min(value, 63);
			}
		}

		// Token: 0x170006CE RID: 1742
		// (get) Token: 0x06001A9E RID: 6814 RVA: 0x00108036 File Offset: 0x00106236
		// (set) Token: 0x06001A9F RID: 6815 RVA: 0x0010803E File Offset: 0x0010623E
		[Serialize(-1, IsPropertySaveable.No, "Maximum stack size when the item is inside a holdable or wearable item. If not set, defaults to MaxStackSizeCharacterInventory.", "", false)]
		public int MaxStackSizeHoldableOrWearableInventory
		{
			get
			{
				return this.maxStackSizeHoldableOrWearableInventory;
			}
			private set
			{
				this.maxStackSizeHoldableOrWearableInventory = Math.Min(value, 63);
			}
		}

		// Token: 0x06001AA0 RID: 6816 RVA: 0x00108050 File Offset: 0x00106250
		public int GetMaxStackSize(Inventory inventory)
		{
			ItemInventory i = inventory as ItemInventory;
			int num;
			if (i != null)
			{
				Entity owner = inventory.Owner;
				Item it = owner as Item;
				if (it != null)
				{
					num = (int)it.StatManager.GetAdjustedValueAdditive(ItemTalentStats.ExtraStackSize, (float)i.ExtraStackSize);
					goto IL_B0;
				}
			}
			else
			{
				CharacterInventory j = inventory as CharacterInventory;
				if (j != null)
				{
					Entity owner = inventory.Owner;
					Character character = owner as Character;
					if (character != null)
					{
						CharacterInfo <info>5__2 = character.Info;
						if (<info>5__2 != null)
						{
							num = j.ExtraStackSize + EnumExtensions.GetIndividualFlags<MapEntityCategory>(this.Category).Sum((MapEntityCategory c) => (int)<info>5__2.GetSavedStatValueWithAll(StatTypes.InventoryExtraStackSize, c.ToIdentifier<MapEntityCategory>()));
							goto IL_B0;
						}
					}
				}
				else if (inventory == null)
				{
					num = 0;
					goto IL_B0;
				}
			}
			num = inventory.ExtraStackSize;
			IL_B0:
			int extraStackSize = num;
			if (inventory is CharacterInventory && this.maxStackSizeCharacterInventory > 0)
			{
				return ItemPrefab.<GetMaxStackSize>g__MaxStackWithExtra|360_0(this.maxStackSizeCharacterInventory, extraStackSize);
			}
			Item item = ((inventory != null) ? inventory.Owner : null) as Item;
			if (item != null)
			{
				Holdable component = item.GetComponent<Holdable>();
				if ((component != null && !component.Attachable) || item.GetComponent<Wearable>() != null)
				{
					if (this.maxStackSizeHoldableOrWearableInventory > 0)
					{
						return ItemPrefab.<GetMaxStackSize>g__MaxStackWithExtra|360_0(this.maxStackSizeHoldableOrWearableInventory, extraStackSize);
					}
					if (this.maxStackSizeCharacterInventory > 0)
					{
						return ItemPrefab.<GetMaxStackSize>g__MaxStackWithExtra|360_0(this.maxStackSizeCharacterInventory, extraStackSize);
					}
				}
			}
			return ItemPrefab.<GetMaxStackSize>g__MaxStackWithExtra|360_0(this.maxStackSize, extraStackSize);
		}

		// Token: 0x170006CF RID: 1743
		// (get) Token: 0x06001AA1 RID: 6817 RVA: 0x0010819C File Offset: 0x0010639C
		// (set) Token: 0x06001AA2 RID: 6818 RVA: 0x001081A4 File Offset: 0x001063A4
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowDroppingOnSwap { get; private set; }

		// Token: 0x170006D0 RID: 1744
		// (get) Token: 0x06001AA3 RID: 6819 RVA: 0x001081AD File Offset: 0x001063AD
		// (set) Token: 0x06001AA4 RID: 6820 RVA: 0x001081B5 File Offset: 0x001063B5
		public ImmutableHashSet<Identifier> AllowDroppingOnSwapWith { get; private set; }

		// Token: 0x170006D1 RID: 1745
		// (get) Token: 0x06001AA5 RID: 6821 RVA: 0x001081BE File Offset: 0x001063BE
		// (set) Token: 0x06001AA6 RID: 6822 RVA: 0x001081C6 File Offset: 0x001063C6
		[Serialize(false, IsPropertySaveable.No, "If enabled, the item is not transferred when the player transfers items between subs.", "", false)]
		public bool DontTransferBetweenSubs { get; private set; }

		// Token: 0x170006D2 RID: 1746
		// (get) Token: 0x06001AA7 RID: 6823 RVA: 0x001081CF File Offset: 0x001063CF
		// (set) Token: 0x06001AA8 RID: 6824 RVA: 0x001081D7 File Offset: 0x001063D7
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowHealthBar { get; private set; }

		// Token: 0x170006D3 RID: 1747
		// (get) Token: 0x06001AA9 RID: 6825 RVA: 0x001081E0 File Offset: 0x001063E0
		// (set) Token: 0x06001AAA RID: 6826 RVA: 0x001081E8 File Offset: 0x001063E8
		[Serialize(1f, IsPropertySaveable.No, "How much the bots prioritize this item when they seek for items. For example, bots prioritize less exosuit than the other diving suits. Defaults to 1. Note that there's also a specific CombatPriority for items that can be used as weapons.", "", false)]
		public float BotPriority { get; private set; }

		// Token: 0x170006D4 RID: 1748
		// (get) Token: 0x06001AAB RID: 6827 RVA: 0x001081F1 File Offset: 0x001063F1
		// (set) Token: 0x06001AAC RID: 6828 RVA: 0x001081F9 File Offset: 0x001063F9
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowNameInHealthBar { get; private set; }

		// Token: 0x170006D5 RID: 1749
		// (get) Token: 0x06001AAD RID: 6829 RVA: 0x00108202 File Offset: 0x00106402
		// (set) Token: 0x06001AAE RID: 6830 RVA: 0x0010820A File Offset: 0x0010640A
		[Serialize(false, IsPropertySaveable.No, "Should the bots shoot at this item with turret or not? Disabled by default.", "", false)]
		public bool IsAITurretTarget { get; private set; }

		// Token: 0x170006D6 RID: 1750
		// (get) Token: 0x06001AAF RID: 6831 RVA: 0x00108213 File Offset: 0x00106413
		// (set) Token: 0x06001AB0 RID: 6832 RVA: 0x0010821B File Offset: 0x0010641B
		[Serialize(1f, IsPropertySaveable.No, "How much the bots prioritize shooting this item with turrets? Defaults to 1. Distance to the target affects the decision making.", "", false)]
		public float AITurretPriority { get; private set; }

		// Token: 0x170006D7 RID: 1751
		// (get) Token: 0x06001AB1 RID: 6833 RVA: 0x00108224 File Offset: 0x00106424
		// (set) Token: 0x06001AB2 RID: 6834 RVA: 0x0010822C File Offset: 0x0010642C
		[Serialize(1f, IsPropertySaveable.No, "How much the bots prioritize shooting this item with slow turrets, like railguns? Defaults to 1. Not used if AITurretPriority is 0. Distance to the target affects the decision making.", "", false)]
		public float AISlowTurretPriority { get; private set; }

		// Token: 0x170006D8 RID: 1752
		// (get) Token: 0x06001AB3 RID: 6835 RVA: 0x00108235 File Offset: 0x00106435
		// (set) Token: 0x06001AB4 RID: 6836 RVA: 0x0010823D File Offset: 0x0010643D
		[Serialize(float.PositiveInfinity, IsPropertySaveable.No, "The max distance at which the bots are allowed to target the items. Defaults to infinity.", "", false)]
		public float AITurretTargetingMaxDistance { get; private set; }

		// Token: 0x170006D9 RID: 1753
		// (get) Token: 0x06001AB5 RID: 6837 RVA: 0x00108246 File Offset: 0x00106446
		// (set) Token: 0x06001AB6 RID: 6838 RVA: 0x0010824E File Offset: 0x0010644E
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, taking items from this container is never considered stealing.", "", false)]
		public bool AllowStealingContainedItems { get; private set; }

		// Token: 0x170006DA RID: 1754
		// (get) Token: 0x06001AB7 RID: 6839 RVA: 0x00108257 File Offset: 0x00106457
		// (set) Token: 0x06001AB8 RID: 6840 RVA: 0x0010825F File Offset: 0x0010645F
		[Serialize("255,255,255,255", IsPropertySaveable.No, "Used in circuit box to set the color of the nodes.", "", false)]
		public Color SignalComponentColor { get; private set; }

		// Token: 0x170006DB RID: 1755
		// (get) Token: 0x06001AB9 RID: 6841 RVA: 0x00108268 File Offset: 0x00106468
		// (set) Token: 0x06001ABA RID: 6842 RVA: 0x00108270 File Offset: 0x00106470
		[Serialize(false, IsPropertySaveable.No, "If enabled, the player is unable to open the middle click menu when this item is selected.", "", false)]
		public bool DisableCommandMenuWhenSelected { get; set; }

		// Token: 0x06001ABB RID: 6843 RVA: 0x0010827C File Offset: 0x0010647C
		protected override Identifier DetermineIdentifier(XElement element)
		{
			Identifier identifier = base.DetermineIdentifier(element);
			string originalName = element.GetAttributeString("name", "");
			if (identifier.IsEmpty && !string.IsNullOrEmpty(originalName))
			{
				string categoryStr = element.GetAttributeString("category", "Misc");
				MapEntityCategory category;
				if (Enum.TryParse<MapEntityCategory>(categoryStr, true, out category) && category.HasFlag(MapEntityCategory.Legacy))
				{
					identifier = ItemPrefab.GenerateLegacyIdentifier(originalName);
				}
			}
			return identifier;
		}

		// Token: 0x06001ABC RID: 6844 RVA: 0x001082EE File Offset: 0x001064EE
		public static Identifier GenerateLegacyIdentifier(string name)
		{
			return ("legacyitem_" + name.Replace(" ", "")).ToIdentifier();
		}

		// Token: 0x06001ABD RID: 6845 RVA: 0x00108310 File Offset: 0x00106510
		public ItemPrefab(ContentXElement element, ItemFile file) : base(element, file)
		{
			this.originalElement = element;
			this.ConfigElement = element;
			this.OriginalName = element.GetAttributeString("name", "");
			this.name = this.OriginalName;
			this.VariantOf = element.VariantOf();
			if (!this.VariantOf.IsEmpty)
			{
				return;
			}
			this.ParseConfigElement(null);
		}

		// Token: 0x06001ABE RID: 6846 RVA: 0x00108389 File Offset: 0x00106589
		public string GetTexturePath(ContentXElement subElement, ItemPrefab variantOf)
		{
			if (!subElement.DoesAttributeReferenceFileNameAlone("texture"))
			{
				return "";
			}
			return Path.GetDirectoryName(((variantOf != null) ? variantOf.ContentFile.Path : null) ?? this.ContentFile.Path);
		}

		// Token: 0x06001ABF RID: 6847 RVA: 0x001083C4 File Offset: 0x001065C4
		private void ParseConfigElement(ItemPrefab variantOf)
		{
			string categoryStr = this.ConfigElement.GetAttributeString("category", "Misc");
			MapEntityCategory category;
			this.category = (Enum.TryParse<MapEntityCategory>(categoryStr, true, out category) ? category : MapEntityCategory.Misc);
			Identifier nameIdentifier = this.ConfigElement.GetAttributeIdentifier("nameidentifier", "");
			string fallbackNameIdentifier = this.ConfigElement.GetAttributeString("fallbacknameidentifier", "");
			string[] array = new string[2];
			int num = 0;
			string text;
			if (!nameIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler.AppendLiteral("EntityName.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(nameIdentifier);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("EntityName.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				text = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			array[num] = text;
			array[1] = "EntityName." + fallbackNameIdentifier;
			this.name = TextManager.Get(array);
			if (!string.IsNullOrEmpty(this.OriginalName))
			{
				this.name = this.name.Fallback(this.OriginalName, true);
			}
			if (category == MapEntityCategory.Wrecked)
			{
				this.name = TextManager.GetWithVariable("wreckeditemformat", "[name]", this.name, FormatCapitals.No);
			}
			this.name = GeneticMaterial.TryCreateName(this, this.ConfigElement);
			this.aliases = (this.ConfigElement.GetAttributeStringArray("aliases", null, true) ?? this.ConfigElement.GetAttributeStringArray("Aliases", Array.Empty<string>(), true)).ToImmutableHashSet<string>().Add(this.OriginalName.ToLowerInvariant());
			List<Rectangle> triggers = new List<Rectangle>();
			List<DeconstructItem> deconstructItems = new List<DeconstructItem>();
			Dictionary<uint, FabricationRecipe> fabricationRecipes = new Dictionary<uint, FabricationRecipe>();
			Dictionary<Identifier, float> treatmentSuitability = new Dictionary<Identifier, float>();
			Dictionary<Identifier, PriceInfo> storePrices = new Dictionary<Identifier, PriceInfo>();
			List<PreferredContainer> preferredContainers = new List<PreferredContainer>();
			this.DeconstructTime = 1f;
			this.DeconstructTimeInOutposts = this.DeconstructTime;
			if (this.ConfigElement.GetAttribute("allowasextracargo") != null)
			{
				this.AllowAsExtraCargo = new bool?(this.ConfigElement.GetAttributeBool("allowasextracargo", false));
			}
			List<Identifier> tags = this.ConfigElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToList<Identifier>();
			if (this.ConfigElement.Descendants().Any(delegate(ContentXElement e)
			{
				Identifier identifier = e.NameAsIdentifier();
				return identifier == "lightcomponent";
			}))
			{
				tags.Add("light".ToIdentifier());
			}
			this.tags = tags.ToImmutableHashSet<Identifier>();
			if (this.ConfigElement.GetAttribute("cargocontainername") != null)
			{
				DebugConsole.ThrowError("Error in item prefab \"" + this.ToString() + "\" - cargo container should be configured using the item's identifier, not the name.", null, this.ConfigElement.ContentPackage, false, false);
			}
			SerializableProperty.DeserializeProperties(this, this.ConfigElement);
			base.LoadDescription(this.ConfigElement);
			List<SkillRequirementHint> skillRequirementHints = new List<SkillRequirementHint>();
			foreach (ContentXElement skillRequirementHintElement in this.ConfigElement.GetChildElements("SkillRequirementHint"))
			{
				skillRequirementHints.Add(new SkillRequirementHint(skillRequirementHintElement));
			}
			if (skillRequirementHints.Any<SkillRequirementHint>())
			{
				this.SkillRequirementHints = skillRequirementHints.ToImmutableArray<SkillRequirementHint>();
			}
			Identifier[] allowDroppingOnSwapWith = this.ConfigElement.GetAttributeIdentifierArray("allowdroppingonswapwith", Array.Empty<Identifier>(), true);
			this.AllowDroppingOnSwapWith = allowDroppingOnSwapWith.ToImmutableHashSet<Identifier>();
			this.AllowDroppingOnSwap = allowDroppingOnSwapWith.Any<Identifier>();
			Dictionary<Identifier, ItemPrefab.CommonnessInfo> levelCommonness = new Dictionary<Identifier, ItemPrefab.CommonnessInfo>();
			Dictionary<Identifier, ItemPrefab.FixedQuantityResourceInfo> levelQuantity = new Dictionary<Identifier, ItemPrefab.FixedQuantityResourceInfo>();
			List<FabricationRecipe> loadedRecipes = new List<FabricationRecipe>();
			foreach (ContentXElement subElement in this.ConfigElement.Elements())
			{
				string text2 = subElement.Name.ToString().ToLowerInvariant();
				if (text2 != null)
				{
					switch (text2.Length)
					{
					case 5:
					{
						if (!(text2 == "price"))
						{
							continue;
						}
						if (subElement.GetAttribute("baseprice") != null)
						{
							using (List<PriceInfo>.Enumerator enumerator3 = PriceInfo.CreatePriceInfos(subElement, out this.defaultPrice).GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									PriceInfo priceInfo = enumerator3.Current;
									if (!priceInfo.StoreIdentifier.IsEmpty)
									{
										if (storePrices.ContainsKey(priceInfo.StoreIdentifier))
										{
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(71, 2);
											defaultInterpolatedStringHandler3.AppendLiteral("Error in item prefab \"");
											defaultInterpolatedStringHandler3.AppendFormatted<ItemPrefab>(this);
											defaultInterpolatedStringHandler3.AppendLiteral("\": price for the store \"");
											defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(priceInfo.StoreIdentifier);
											defaultInterpolatedStringHandler3.AppendLiteral("\" defined more than once.");
											DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), base.ContentPackage);
											storePrices[priceInfo.StoreIdentifier] = priceInfo;
										}
										else
										{
											storePrices.Add(priceInfo.StoreIdentifier, priceInfo);
										}
									}
								}
								continue;
							}
						}
						if (subElement.GetAttribute("buyprice") == null)
						{
							continue;
						}
						Identifier locationType = subElement.GetAttributeIdentifier("locationtype", "");
						if (locationType.IsEmpty)
						{
							continue;
						}
						if (storePrices.ContainsKey(locationType))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(79, 2);
							defaultInterpolatedStringHandler4.AppendLiteral("Error in item prefab \"");
							defaultInterpolatedStringHandler4.AppendFormatted<ItemPrefab>(this);
							defaultInterpolatedStringHandler4.AppendLiteral("\": price for the location type \"");
							defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(locationType);
							defaultInterpolatedStringHandler4.AppendLiteral("\" defined more than once.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler4.ToStringAndClear(), base.ContentPackage);
							storePrices[locationType] = new PriceInfo(subElement);
							continue;
						}
						storePrices.Add(locationType, new PriceInfo(subElement));
						continue;
					}
					case 6:
					{
						if (!(text2 == "sprite"))
						{
							continue;
						}
						string spriteFolder = this.GetTexturePath(subElement, variantOf);
						this.canSpriteFlipX = subElement.GetAttributeBool("canflipx", true);
						this.canSpriteFlipY = subElement.GetAttributeBool("canflipy", true);
						this.sprite = new Sprite(subElement, spriteFolder, "", true, 1f);
						if (subElement.GetAttribute("sourcerect") == null && subElement.GetAttribute("sheetindex") == null)
						{
							DebugConsole.ThrowError("Warning - sprite sourcerect not configured for item \"" + this.ToString() + "\"!", null, this.ConfigElement.ContentPackage, false, false);
						}
						this.Size = this.Sprite.size;
						if (subElement.GetAttribute("name") == null && !this.Name.IsNullOrWhiteSpace())
						{
							this.Sprite.Name = this.Name.Value;
						}
						this.Sprite.EntityIdentifier = this.Identifier;
						continue;
					}
					case 7:
					{
						if (!(text2 == "trigger"))
						{
							continue;
						}
						Rectangle trigger = new Rectangle(0, 0, 10, 10)
						{
							X = subElement.GetAttributeInt("x", 0),
							Y = subElement.GetAttributeInt("y", 0),
							Width = subElement.GetAttributeInt("width", 0),
							Height = subElement.GetAttributeInt("height", 0)
						};
						triggers.Add(trigger);
						continue;
					}
					case 8:
					case 12:
					case 15:
					case 16:
						continue;
					case 9:
						if (!(text2 == "fabricate"))
						{
							continue;
						}
						break;
					case 10:
						if (!(text2 == "fabricable"))
						{
							continue;
						}
						break;
					case 11:
						if (!(text2 == "deconstruct"))
						{
							continue;
						}
						this.DeconstructTime = subElement.GetAttributeFloat("time", 1f);
						this.DeconstructTimeInOutposts = subElement.GetAttributeFloat("timeinoutposts", this.DeconstructTime);
						this.AllowDeconstruct = true;
						this.RandomDeconstructionOutput = subElement.GetAttributeBool("chooserandom", false);
						this.RandomDeconstructionOutputAmount = subElement.GetAttributeInt("amount", 1);
						foreach (ContentXElement cxe in subElement.Elements())
						{
							XElement itemElement = cxe;
							if (itemElement.Attribute("name") != null)
							{
								DebugConsole.ThrowError("Error in item config \"" + this.ToString() + "\" - use item identifiers instead of names to configure the deconstruct items.", null, this.ConfigElement.ContentPackage, false, false);
							}
							else
							{
								DeconstructItem deconstructItem = new DeconstructItem(itemElement, this.Identifier);
								if (deconstructItem.ItemIdentifier.IsEmpty)
								{
									DebugConsole.ThrowError("Error in item config \"" + this.ToString() + "\" - deconstruction output contains an item with no identifier.", null, this.ConfigElement.ContentPackage, false, false);
								}
								else
								{
									deconstructItems.Add(deconstructItem);
								}
							}
						}
						this.RandomDeconstructionOutputAmount = Math.Min(this.RandomDeconstructionOutputAmount, deconstructItems.Count);
						continue;
					case 13:
					{
						char c = text2[0];
						if (c != 'l')
						{
							if (c != 's')
							{
								continue;
							}
							if (!(text2 == "swappableitem"))
							{
								continue;
							}
							this.SwappableItem = new SwappableItem(subElement);
							continue;
						}
						else
						{
							if (!(text2 == "levelresource"))
							{
								continue;
							}
							using (IEnumerator<ContentXElement> enumerator5 = subElement.GetChildElements("commonness").GetEnumerator())
							{
								while (enumerator5.MoveNext())
								{
									ContentXElement cxe2 = enumerator5.Current;
									XElement levelCommonnessElement = cxe2;
									Identifier levelName = levelCommonnessElement.GetAttributeIdentifier("leveltype", "");
									if (!levelCommonnessElement.GetAttributeBool("fixedquantity", false))
									{
										if (!levelCommonness.ContainsKey(levelName))
										{
											levelCommonness.Add(levelName, new ItemPrefab.CommonnessInfo(levelCommonnessElement));
										}
									}
									else if (!levelQuantity.ContainsKey(levelName))
									{
										levelQuantity.Add(levelName, new ItemPrefab.FixedQuantityResourceInfo(levelCommonnessElement.GetAttributeInt("clusterquantity", 0), levelCommonnessElement.GetAttributeInt("clustersize", 0), levelCommonnessElement.GetAttributeBool("isislandspecific", false), levelCommonnessElement.GetAttributeBool("allowatstart", true)));
									}
								}
								continue;
							}
							goto IL_BC6;
						}
						break;
					}
					case 14:
						if (!(text2 == "fabricableitem"))
						{
							continue;
						}
						break;
					case 17:
						if (!(text2 == "suitabletreatment"))
						{
							continue;
						}
						goto IL_BC6;
					case 18:
					{
						if (!(text2 == "preferredcontainer"))
						{
							continue;
						}
						PreferredContainer preferredContainer = new PreferredContainer(subElement);
						if (preferredContainer.Primary.Count != 0 || preferredContainer.Secondary.Count != 0)
						{
							preferredContainers.Add(preferredContainer);
							continue;
						}
						if (variantOf == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(75, 2);
							defaultInterpolatedStringHandler5.AppendLiteral("Error in item prefab \"");
							defaultInterpolatedStringHandler5.AppendFormatted(this.ToString());
							defaultInterpolatedStringHandler5.AppendLiteral("\": preferred container has no preferences defined (");
							defaultInterpolatedStringHandler5.AppendFormatted<ContentXElement>(subElement);
							defaultInterpolatedStringHandler5.AppendLiteral(").");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), null, this.ConfigElement.ContentPackage, false, false);
							continue;
						}
						continue;
					}
					default:
						continue;
					}
					FabricationRecipe newRecipe = new FabricationRecipe(subElement, this.Identifier);
					FabricationRecipe prevRecipe;
					if (fabricationRecipes.TryGetValue(newRecipe.RecipeHash, out prevRecipe))
					{
						ContentPackage packageToLog = (variantOf.ContentPackage != null && variantOf.ContentPackage != ContentPackageManager.VanillaCorePackage) ? variantOf.ContentPackage : this.GetParentModPackageOrThisPackage();
						int prevRecipeIndex = loadedRecipes.IndexOf(prevRecipe);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(177, 3);
						defaultInterpolatedStringHandler6.AppendLiteral("Error in item prefab \"");
						defaultInterpolatedStringHandler6.AppendFormatted(this.ToString());
						defaultInterpolatedStringHandler6.AppendLiteral("\": ");
						defaultInterpolatedStringHandler6.AppendLiteral("Fabrication recipe #");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(loadedRecipes.Count + 1);
						defaultInterpolatedStringHandler6.AppendLiteral(" has the same hash as recipe #");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(prevRecipeIndex + 1);
						defaultInterpolatedStringHandler6.AppendLiteral(". This is most likely caused by identical, duplicate recipes. ");
						defaultInterpolatedStringHandler6.AppendLiteral("This will cause issues with fabrication.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler6.ToStringAndClear(), packageToLog);
					}
					else
					{
						fabricationRecipes.Add(newRecipe.RecipeHash, newRecipe);
					}
					loadedRecipes.Add(newRecipe);
					continue;
					IL_BC6:
					if (subElement.GetAttribute("name") != null)
					{
						DebugConsole.ThrowError("Error in item prefab \"" + this.ToString() + "\" - suitable treatments should be defined using item identifiers, not item names.", null, this.ConfigElement.ContentPackage, false, false);
					}
					Identifier treatmentIdentifier = subElement.GetAttributeIdentifier("identifier", subElement.GetAttributeIdentifier("type", Identifier.Empty));
					float suitability = subElement.GetAttributeFloat("suitability", 0f);
					treatmentSuitability.Add(treatmentIdentifier, suitability);
				}
			}
			ContentXElement configElement = this.ConfigElement;
			string key = "Size";
			Vector2 size = this.Size;
			this.Size = configElement.GetAttributeVector2(key, size);
			this.ParseSubElementsClient(this.ConfigElement, variantOf);
			this.Triggers = triggers.ToImmutableArray<Rectangle>();
			this.DeconstructItems = deconstructItems.ToImmutableArray<DeconstructItem>();
			this.FabricationRecipes = fabricationRecipes.ToImmutableDictionary<uint, FabricationRecipe>();
			this.treatmentSuitability = treatmentSuitability.ToImmutableDictionary<Identifier, float>();
			this.StorePrices = storePrices.ToImmutableDictionary<Identifier, PriceInfo>();
			this.PreferredContainers = preferredContainers.ToImmutableArray<PreferredContainer>();
			this.LevelCommonness = levelCommonness.ToImmutableDictionary<Identifier, ItemPrefab.CommonnessInfo>();
			this.LevelQuantity = levelQuantity.ToImmutableDictionary<Identifier, ItemPrefab.FixedQuantityResourceInfo>();
			ContentXElement childElement = this.ConfigElement.GetChildElement("Holdable");
			ContentXElement contentXElement = null;
			bool canFlipYByDefault = childElement == contentXElement;
			this.CanFlipY = this.ConfigElement.GetAttributeBool("CanFlipY", canFlipYByDefault);
			if (storePrices.Any<KeyValuePair<Identifier, PriceInfo>>() && this.defaultPrice == null)
			{
				this.defaultPrice = new PriceInfo(this.GetMinPrice().GetValueOrDefault(), false, 0, 0, true, 0, 1f, false, false, null);
			}
			this.HideConditionInTooltip = this.ConfigElement.GetAttributeBool("hideconditionintooltip", this.HideConditionBar);
			if (categoryStr.Equals("Thalamus", StringComparison.OrdinalIgnoreCase))
			{
				this.category = MapEntityCategory.Wrecked;
				base.Subcategory = "Thalamus";
			}
			if (this.Sprite == null)
			{
				DebugConsole.ThrowError("Item \"" + this.ToString() + "\" has no sprite!", null, this.ConfigElement.ContentPackage, false, false);
				this.sprite = new Sprite(TextureLoader.PlaceHolderTexture, null, null, 0f, null)
				{
					Origin = TextureLoader.PlaceHolderTexture.Bounds.Size.ToVector2() / 2f
				};
				this.Size = this.Sprite.size;
				this.Sprite.EntityIdentifier = this.Identifier;
			}
			if (this.Identifier == Identifier.Empty)
			{
				DebugConsole.ThrowError("Item prefab \"" + this.ToString() + "\" has no identifier. All item prefabs have a unique identifier string that's used to differentiate between items during saving and loading.", null, this.ConfigElement.ContentPackage, false, false);
			}
			this.allowedLinks = this.ConfigElement.GetAttributeIdentifierArray("allowedlinks", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			ContentXElement configElement2 = this.ConfigElement;
			string key2 = "GrabWhenSelected";
			childElement = this.ConfigElement.GetChildElement("ItemContainer");
			contentXElement = null;
			bool def;
			if (childElement != contentXElement)
			{
				ContentXElement childElement2 = this.ConfigElement.GetChildElement("Body");
				ContentXElement contentXElement2 = null;
				def = (childElement2 == contentXElement2);
			}
			else
			{
				def = false;
			}
			this.GrabWhenSelected = configElement2.GetAttributeBool(key2, def);
		}

		// Token: 0x06001AC0 RID: 6848 RVA: 0x00109344 File Offset: 0x00107544
		public ItemPrefab.CommonnessInfo? GetCommonnessInfo(Level level)
		{
			ItemPrefab.CommonnessInfo? levelCommonnessInfo = this.<GetCommonnessInfo>g__GetValueOrNull|418_0(level.GenerationParams.Identifier);
			ItemPrefab.CommonnessInfo? biomeCommonnessInfo = this.<GetCommonnessInfo>g__GetValueOrNull|418_0(level.LevelData.Biome.Identifier);
			ItemPrefab.CommonnessInfo? defaultCommonnessInfo = this.<GetCommonnessInfo>g__GetValueOrNull|418_0(Identifier.Empty);
			if (levelCommonnessInfo != null)
			{
				if (levelCommonnessInfo == null)
				{
					return null;
				}
				return new ItemPrefab.CommonnessInfo?(levelCommonnessInfo.GetValueOrDefault().WithInheritedCommonness(new ItemPrefab.CommonnessInfo?[]
				{
					biomeCommonnessInfo,
					defaultCommonnessInfo
				}));
			}
			else if (biomeCommonnessInfo != null)
			{
				if (biomeCommonnessInfo == null)
				{
					return null;
				}
				return new ItemPrefab.CommonnessInfo?(biomeCommonnessInfo.GetValueOrDefault().WithInheritedCommonness(defaultCommonnessInfo));
			}
			else
			{
				if (defaultCommonnessInfo != null)
				{
					return defaultCommonnessInfo;
				}
				return null;
			}
		}

		// Token: 0x06001AC1 RID: 6849 RVA: 0x00109418 File Offset: 0x00107618
		public float GetTreatmentSuitability(Identifier treatmentIdentifier)
		{
			float suitability;
			if (!this.treatmentSuitability.TryGetValue(treatmentIdentifier, out suitability))
			{
				return 0f;
			}
			return suitability;
		}

		// Token: 0x06001AC2 RID: 6850 RVA: 0x0010943C File Offset: 0x0010763C
		public PriceInfo GetPriceInfo(Location.StoreInfo store)
		{
			if (store == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to get price info for \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" with a null store parameter!\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				string message = defaultInterpolatedStringHandler.ToStringAndClear();
				DebugConsole.AddWarning(message, base.ContentPackage);
				GameAnalyticsManager.AddErrorEventOnce("ItemPrefab.GetPriceInfo:StoreParameterNull", GameAnalyticsManager.ErrorSeverity.Error, message);
				return null;
			}
			PriceInfo storePriceInfo;
			if (!store.Identifier.IsEmpty && this.StorePrices != null && this.StorePrices.TryGetValue(store.Identifier, out storePriceInfo))
			{
				return storePriceInfo;
			}
			return this.DefaultPrice;
		}

		// Token: 0x06001AC3 RID: 6851 RVA: 0x001094E4 File Offset: 0x001076E4
		public bool CanBeBoughtFrom(Location.StoreInfo store, out PriceInfo priceInfo)
		{
			ItemPrefab.<>c__DisplayClass421_0 CS$<>8__locals1 = new ItemPrefab.<>c__DisplayClass421_0();
			priceInfo = this.GetPriceInfo(store);
			ItemPrefab.<>c__DisplayClass421_0 CS$<>8__locals2 = CS$<>8__locals1;
			Identifier? faction;
			if (store == null)
			{
				Identifier? identifier = null;
				faction = identifier;
			}
			else
			{
				Faction faction2 = store.Location.Faction;
				if (faction2 == null)
				{
					Identifier? identifier = null;
					faction = identifier;
				}
				else
				{
					faction = new Identifier?(faction2.Prefab.Identifier);
				}
			}
			CS$<>8__locals2.faction = faction;
			ItemPrefab.<>c__DisplayClass421_0 CS$<>8__locals3 = CS$<>8__locals1;
			Identifier? secondaryFaction;
			if (store == null)
			{
				Identifier? identifier = null;
				secondaryFaction = identifier;
			}
			else
			{
				Faction secondaryFaction2 = store.Location.SecondaryFaction;
				if (secondaryFaction2 == null)
				{
					Identifier? identifier = null;
					secondaryFaction = identifier;
				}
				else
				{
					secondaryFaction = new Identifier?(secondaryFaction2.Prefab.Identifier);
				}
			}
			CS$<>8__locals3.secondaryFaction = secondaryFaction;
			PriceInfo priceInfo2 = priceInfo;
			if (priceInfo2 != null && priceInfo2.CanBeBought)
			{
				float? num;
				if (store == null)
				{
					num = null;
				}
				else
				{
					LevelData levelData = store.Location.LevelData;
					num = ((levelData != null) ? new float?(levelData.Difficulty) : null);
				}
				float? num2 = num;
				if (num2.GetValueOrDefault() >= (float)priceInfo.MinLevelDifficulty)
				{
					if (!priceInfo.RequiredFaction.IsEmpty)
					{
						ItemPrefab.<>c__DisplayClass421_0 CS$<>8__locals4 = CS$<>8__locals1;
						Identifier? identifier = new Identifier?(priceInfo.RequiredFaction);
						if (!(CS$<>8__locals4.faction == identifier))
						{
							ItemPrefab.<>c__DisplayClass421_0 CS$<>8__locals5 = CS$<>8__locals1;
							Identifier? identifier2 = new Identifier?(priceInfo.RequiredFaction);
							if (!(CS$<>8__locals5.secondaryFaction == identifier2))
							{
								return false;
							}
						}
					}
					return !priceInfo.MinReputation.Any<KeyValuePair<Identifier, float>>() || priceInfo.MinReputation.Any(delegate(KeyValuePair<Identifier, float> p)
					{
						Identifier? identifier3 = new Identifier?(p.Key);
						if (!(CS$<>8__locals1.faction == identifier3))
						{
							Identifier? identifier4 = new Identifier?(p.Key);
							return CS$<>8__locals1.secondaryFaction == identifier4;
						}
						return true;
					});
				}
			}
			return false;
		}

		// Token: 0x06001AC4 RID: 6852 RVA: 0x00109648 File Offset: 0x00107848
		public bool CanBeBoughtFrom(Location location)
		{
			Location location2 = location;
			if (((location2 != null) ? location2.Stores : null) == null)
			{
				return false;
			}
			Func<KeyValuePair<Identifier, float>, bool> <>9__0;
			foreach (KeyValuePair<Identifier, Location.StoreInfo> store in location.Stores)
			{
				PriceInfo priceInfo = this.GetPriceInfo(store.Value);
				if (priceInfo != null && priceInfo.CanBeBought && location.LevelData.Difficulty >= (float)priceInfo.MinLevelDifficulty)
				{
					if (priceInfo.MinReputation.Any<KeyValuePair<Identifier, float>>())
					{
						IEnumerable<KeyValuePair<Identifier, float>> minReputation = priceInfo.MinReputation;
						Func<KeyValuePair<Identifier, float>, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = delegate(KeyValuePair<Identifier, float> p)
							{
								Location location3 = location;
								Identifier? identifier;
								Identifier? identifier2;
								if (location3 == null)
								{
									identifier = null;
									identifier2 = identifier;
								}
								else
								{
									Faction faction = location3.Faction;
									if (faction == null)
									{
										identifier = null;
										identifier2 = identifier;
									}
									else
									{
										identifier2 = new Identifier?(faction.Prefab.Identifier);
									}
								}
								identifier = identifier2;
								Identifier? identifier3 = new Identifier?(p.Key);
								if (!(identifier == identifier3))
								{
									Location location4 = location;
									Identifier? identifier4;
									Identifier? identifier5;
									if (location4 == null)
									{
										identifier4 = null;
										identifier5 = identifier4;
									}
									else
									{
										Faction secondaryFaction = location4.SecondaryFaction;
										if (secondaryFaction == null)
										{
											identifier4 = null;
											identifier5 = identifier4;
										}
										else
										{
											identifier5 = new Identifier?(secondaryFaction.Prefab.Identifier);
										}
									}
									identifier4 = identifier5;
									Identifier? identifier6 = new Identifier?(p.Key);
									return identifier4 == identifier6;
								}
								return true;
							});
						}
						if (!minReputation.Any(predicate))
						{
							continue;
						}
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001AC5 RID: 6853 RVA: 0x00109738 File Offset: 0x00107938
		public int? GetMinPrice()
		{
			int? minPrice = null;
			if (this.StorePrices != null && this.StorePrices.Any<KeyValuePair<Identifier, PriceInfo>>())
			{
				minPrice = new int?(this.StorePrices.Values.Min((PriceInfo p) => p.Price));
			}
			if (minPrice != null)
			{
				if (this.DefaultPrice == null)
				{
					return new int?(minPrice.Value);
				}
				int? num = minPrice;
				int price = this.DefaultPrice.Price;
				if (!(num.GetValueOrDefault() < price & num != null))
				{
					return new int?(this.DefaultPrice.Price);
				}
				return minPrice;
			}
			else
			{
				PriceInfo priceInfo = this.DefaultPrice;
				if (priceInfo == null)
				{
					return null;
				}
				return new int?(priceInfo.Price);
			}
		}

		// Token: 0x06001AC6 RID: 6854 RVA: 0x0010980C File Offset: 0x00107A0C
		public ImmutableDictionary<Identifier, PriceInfo> GetBuyPricesUnder(int maxCost = 0)
		{
			Dictionary<Identifier, PriceInfo> prices = new Dictionary<Identifier, PriceInfo>();
			if (this.StorePrices != null)
			{
				foreach (KeyValuePair<Identifier, PriceInfo> storePrice in this.StorePrices)
				{
					PriceInfo priceInfo = storePrice.Value;
					if (priceInfo != null && priceInfo.CanBeBought && (priceInfo.Price < maxCost || maxCost == 0))
					{
						prices.Add(storePrice.Key, priceInfo);
					}
				}
			}
			return prices.ToImmutableDictionary<Identifier, PriceInfo>();
		}

		// Token: 0x06001AC7 RID: 6855 RVA: 0x0010989C File Offset: 0x00107A9C
		public ImmutableDictionary<Identifier, PriceInfo> GetSellPricesOver(int minCost = 0, bool sellingImportant = true)
		{
			Dictionary<Identifier, PriceInfo> prices = new Dictionary<Identifier, PriceInfo>();
			if (!this.CanBeSold && sellingImportant)
			{
				return prices.ToImmutableDictionary<Identifier, PriceInfo>();
			}
			foreach (KeyValuePair<Identifier, PriceInfo> storePrice in this.StorePrices)
			{
				PriceInfo priceInfo = storePrice.Value;
				if (priceInfo != null && priceInfo.Price > minCost)
				{
					prices.Add(storePrice.Key, priceInfo);
				}
			}
			return prices.ToImmutableDictionary<Identifier, PriceInfo>();
		}

		// Token: 0x06001AC8 RID: 6856 RVA: 0x0010992C File Offset: 0x00107B2C
		public static ItemPrefab Find(string name, Identifier identifier)
		{
			if (string.IsNullOrEmpty(name) && identifier.IsEmpty)
			{
				throw new ArgumentException("Both name and identifier cannot be null.");
			}
			if (identifier.IsEmpty)
			{
				identifier = ItemPrefab.GenerateLegacyIdentifier(name);
			}
			ItemPrefab prefab;
			ItemPrefab.Prefabs.TryGet(identifier, out prefab);
			if (prefab == null && !string.IsNullOrEmpty(name))
			{
				string lowerCaseName = name.ToLowerInvariant();
				prefab = ItemPrefab.Prefabs.Find((ItemPrefab me) => me.Aliases != null && me.Aliases.Contains(lowerCaseName));
			}
			if (prefab == null)
			{
				prefab = ItemPrefab.Prefabs.Find((ItemPrefab me) => me.Aliases != null && me.Aliases.Contains(identifier.Value));
			}
			if (prefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error loading item - item prefab \"");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("\" (identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\") not found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			return prefab;
		}

		// Token: 0x06001AC9 RID: 6857 RVA: 0x00109A38 File Offset: 0x00107C38
		public bool IsContainerPreferred(Item item, ItemContainer targetContainer, out bool isPreferencesDefined, out bool isSecondary, bool requireConditionRequirement = false, bool checkTransferConditions = false)
		{
			isPreferencesDefined = this.PreferredContainers.Any<PreferredContainer>();
			isSecondary = false;
			if (!isPreferencesDefined)
			{
				return true;
			}
			if (this.PreferredContainers.Any((PreferredContainer pc) => (!requireConditionRequirement || ItemPrefab.<IsContainerPreferred>g__HasConditionRequirement|427_2(pc)) && ItemPrefab.IsItemConditionAcceptable(item, pc) && ItemPrefab.IsContainerPreferred(pc.Primary, targetContainer) && (!checkTransferConditions || ItemPrefab.CanBeTransferred(item.Prefab.Identifier, pc, targetContainer))))
			{
				return true;
			}
			isSecondary = true;
			return this.PreferredContainers.Any((PreferredContainer pc) => (!requireConditionRequirement || ItemPrefab.<IsContainerPreferred>g__HasConditionRequirement|427_2(pc)) && ItemPrefab.IsItemConditionAcceptable(item, pc) && ItemPrefab.IsContainerPreferred(pc.Secondary, targetContainer));
		}

		// Token: 0x06001ACA RID: 6858 RVA: 0x00109AB8 File Offset: 0x00107CB8
		public bool IsContainerPreferred(Item item, Identifier[] identifiersOrTags, out bool isPreferencesDefined, out bool isSecondary)
		{
			isPreferencesDefined = this.PreferredContainers.Any<PreferredContainer>();
			isSecondary = false;
			if (!isPreferencesDefined)
			{
				return true;
			}
			if (this.PreferredContainers.Any((PreferredContainer pc) => ItemPrefab.IsItemConditionAcceptable(item, pc) && ItemPrefab.IsContainerPreferred(pc.Primary, identifiersOrTags)))
			{
				return true;
			}
			isSecondary = true;
			return this.PreferredContainers.Any((PreferredContainer pc) => ItemPrefab.IsItemConditionAcceptable(item, pc) && ItemPrefab.IsContainerPreferred(pc.Secondary, identifiersOrTags));
		}

		// Token: 0x06001ACB RID: 6859 RVA: 0x00109B26 File Offset: 0x00107D26
		private static bool IsItemConditionAcceptable(Item item, PreferredContainer pc)
		{
			return item.ConditionPercentage >= pc.MinCondition && item.ConditionPercentage <= pc.MaxCondition;
		}

		// Token: 0x06001ACC RID: 6860 RVA: 0x00109B4C File Offset: 0x00107D4C
		private static bool CanBeTransferred(Identifier item, PreferredContainer pc, ItemContainer targetContainer)
		{
			return pc.AllowTransfersHere && (!pc.TransferOnlyOnePerContainer || targetContainer.Inventory.AllItems.None((Item i) => i.Prefab.Identifier == item));
		}

		// Token: 0x06001ACD RID: 6861 RVA: 0x00109B98 File Offset: 0x00107D98
		public static bool IsContainerPreferred(IEnumerable<Identifier> preferences, ItemContainer c)
		{
			return preferences.Any((Identifier id) => c.Item.Prefab.Identifier == id || c.Item.HasTag(id));
		}

		// Token: 0x06001ACE RID: 6862 RVA: 0x00109BC4 File Offset: 0x00107DC4
		public static bool IsContainerPreferred(IEnumerable<Identifier> preferences, IEnumerable<Identifier> ids)
		{
			return ids.Any((Identifier id) => preferences.Contains(id));
		}

		// Token: 0x06001ACF RID: 6863 RVA: 0x00109BF0 File Offset: 0x00107DF0
		protected override void CreateInstance(Rectangle rect)
		{
			throw new InvalidOperationException("Can't call ItemPrefab.CreateInstance");
		}

		// Token: 0x06001AD0 RID: 6864 RVA: 0x00109BFC File Offset: 0x00107DFC
		public override void Dispose()
		{
			Item.RemoveByPrefab(this);
		}

		// Token: 0x170006DC RID: 1756
		// (get) Token: 0x06001AD1 RID: 6865 RVA: 0x00109C04 File Offset: 0x00107E04
		public Identifier VariantOf { get; }

		// Token: 0x170006DD RID: 1757
		// (get) Token: 0x06001AD2 RID: 6866 RVA: 0x00109C0C File Offset: 0x00107E0C
		// (set) Token: 0x06001AD3 RID: 6867 RVA: 0x00109C14 File Offset: 0x00107E14
		public ItemPrefab ParentPrefab { get; set; }

		// Token: 0x06001AD4 RID: 6868 RVA: 0x00109C20 File Offset: 0x00107E20
		public void InheritFrom(ItemPrefab parent)
		{
			ItemPrefab.<>c__DisplayClass442_0 CS$<>8__locals1 = new ItemPrefab.<>c__DisplayClass442_0();
			CS$<>8__locals1.parent = parent;
			CS$<>8__locals1.<>4__this = this;
			this.ConfigElement = this.originalElement.CreateVariantXML(CS$<>8__locals1.parent.ConfigElement, new VariantExtensions.VariantXMLChecker(CS$<>8__locals1.<InheritFrom>g__CheckXML|0));
			this.ParseConfigElement(CS$<>8__locals1.parent);
		}

		// Token: 0x06001AD5 RID: 6869 RVA: 0x00109C75 File Offset: 0x00107E75
		public ContentPackage GetParentModPackageOrThisPackage()
		{
			if (this.ParentPrefab != null && this.ParentPrefab.ContentPackage != ContentPackageManager.VanillaCorePackage)
			{
				return this.ParentPrefab.ContentPackage;
			}
			return base.ContentPackage;
		}

		// Token: 0x06001AD6 RID: 6870 RVA: 0x00109CA4 File Offset: 0x00107EA4
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" (identifier: ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06001AD8 RID: 6872 RVA: 0x00109D00 File Offset: 0x00107F00
		[CompilerGenerated]
		internal static int <GetMaxStackSize>g__MaxStackWithExtra|360_0(int maxStackSize, int extraStackSize)
		{
			extraStackSize = Math.Max(extraStackSize, 0);
			if (maxStackSize == 1)
			{
				return Math.Min(maxStackSize, 63);
			}
			return Math.Min(maxStackSize + extraStackSize, 63);
		}

		// Token: 0x06001AD9 RID: 6873 RVA: 0x00109D24 File Offset: 0x00107F24
		[CompilerGenerated]
		private ItemPrefab.CommonnessInfo? <GetCommonnessInfo>g__GetValueOrNull|418_0(Identifier identifier)
		{
			ItemPrefab.CommonnessInfo info;
			if (this.LevelCommonness.TryGetValue(identifier, out info))
			{
				return new ItemPrefab.CommonnessInfo?(info);
			}
			return null;
		}

		// Token: 0x06001ADA RID: 6874 RVA: 0x00109D51 File Offset: 0x00107F51
		[CompilerGenerated]
		internal static bool <IsContainerPreferred>g__HasConditionRequirement|427_2(PreferredContainer pc)
		{
			return pc.MinCondition > 0f || pc.MaxCondition < 100f;
		}

		// Token: 0x04000D59 RID: 3417
		public float UpgradePreviewScale = 1f;

		// Token: 0x04000D5A RID: 3418
		private IReadOnlyList<DamageModifier> wearableDamageModifiers;

		// Token: 0x04000D5B RID: 3419
		private IReadOnlyDictionary<Identifier, float> wearableSkillModifiers;

		// Token: 0x04000D5F RID: 3423
		public static readonly PrefabCollection<ItemPrefab> Prefabs = new PrefabCollection<ItemPrefab>();

		// Token: 0x04000D60 RID: 3424
		public const float DefaultInteractDistance = 120f;

		// Token: 0x04000D62 RID: 3426
		private PriceInfo defaultPrice;

		// Token: 0x04000D65 RID: 3429
		private ImmutableDictionary<Identifier, float> treatmentSuitability;

		// Token: 0x04000D66 RID: 3430
		private readonly ContentXElement originalElement;

		// Token: 0x04000D72 RID: 3442
		private bool canSpriteFlipX;

		// Token: 0x04000D73 RID: 3443
		private bool canSpriteFlipY;

		// Token: 0x04000D77 RID: 3447
		private Sprite sprite;

		// Token: 0x04000D79 RID: 3449
		private LocalizedString name;

		// Token: 0x04000D7A RID: 3450
		private ImmutableHashSet<Identifier> tags;

		// Token: 0x04000D7B RID: 3451
		private ImmutableHashSet<Identifier> allowedLinks;

		// Token: 0x04000D7C RID: 3452
		private MapEntityCategory category;

		// Token: 0x04000D7D RID: 3453
		private ImmutableHashSet<string> aliases;

		// Token: 0x04000D8B RID: 3467
		private float health;

		// Token: 0x04000DAA RID: 3498
		private int maxStackSize;

		// Token: 0x04000DAB RID: 3499
		private int maxStackSizeCharacterInventory;

		// Token: 0x04000DAC RID: 3500
		private int maxStackSizeHoldableOrWearableInventory;

		// Token: 0x02000AA2 RID: 2722
		public readonly struct CommonnessInfo
		{
			// Token: 0x17001A98 RID: 6808
			// (get) Token: 0x060075ED RID: 30189 RVA: 0x003772AF File Offset: 0x003754AF
			public float Commonness
			{
				get
				{
					return this.commonness;
				}
			}

			// Token: 0x17001A99 RID: 6809
			// (get) Token: 0x060075EE RID: 30190 RVA: 0x003772B7 File Offset: 0x003754B7
			public float AbyssCommonness
			{
				get
				{
					return this.abyssCommonness.GetValueOrDefault();
				}
			}

			// Token: 0x17001A9A RID: 6810
			// (get) Token: 0x060075EF RID: 30191 RVA: 0x003772C4 File Offset: 0x003754C4
			public float CaveCommonness
			{
				get
				{
					float? num = this.caveCommonness;
					if (num == null)
					{
						return this.Commonness;
					}
					return num.GetValueOrDefault();
				}
			}

			// Token: 0x17001A9B RID: 6811
			// (get) Token: 0x060075F0 RID: 30192 RVA: 0x003772EF File Offset: 0x003754EF
			public bool CanAppear
			{
				get
				{
					return this.Commonness > 0f || this.AbyssCommonness > 0f || this.CaveCommonness > 0f;
				}
			}

			// Token: 0x060075F1 RID: 30193 RVA: 0x00377320 File Offset: 0x00375520
			public CommonnessInfo(XElement element)
			{
				this.commonness = Math.Max((element != null) ? element.GetAttributeFloat("commonness", 0f) : 0f, 0f);
				float? abyssCommonness = null;
				XAttribute abyssCommonnessAttribute = ((element != null) ? element.GetAttribute("abysscommonness", StringComparison.OrdinalIgnoreCase) : null) ?? ((element != null) ? element.GetAttribute("abyss", StringComparison.OrdinalIgnoreCase) : null);
				if (abyssCommonnessAttribute != null)
				{
					abyssCommonness = new float?(Math.Max(abyssCommonnessAttribute.GetAttributeFloat(0f), 0f));
				}
				this.abyssCommonness = abyssCommonness;
				float? caveCommonness = null;
				XAttribute caveCommonnessAttribute = ((element != null) ? element.GetAttribute("cavecommonness", StringComparison.OrdinalIgnoreCase) : null) ?? ((element != null) ? element.GetAttribute("cave", StringComparison.OrdinalIgnoreCase) : null);
				if (caveCommonnessAttribute != null)
				{
					caveCommonness = new float?(Math.Max(caveCommonnessAttribute.GetAttributeFloat(0f), 0f));
				}
				this.caveCommonness = caveCommonness;
			}

			// Token: 0x060075F2 RID: 30194 RVA: 0x00377408 File Offset: 0x00375608
			public CommonnessInfo(float commonness, float? abyssCommonness, float? caveCommonness)
			{
				this.commonness = commonness;
				this.abyssCommonness = ((abyssCommonness != null) ? new float?(Math.Max(abyssCommonness.Value, 0f)) : null);
				this.caveCommonness = ((caveCommonness != null) ? new float?(Math.Max(caveCommonness.Value, 0f)) : null);
			}

			// Token: 0x060075F3 RID: 30195 RVA: 0x0037747C File Offset: 0x0037567C
			public ItemPrefab.CommonnessInfo WithInheritedCommonness(ItemPrefab.CommonnessInfo? parentInfo)
			{
				float num = this.commonness;
				float? num2 = this.abyssCommonness;
				float? num3 = (num2 != null) ? num2 : ((parentInfo != null) ? parentInfo.GetValueOrDefault().abyssCommonness : null);
				num2 = this.caveCommonness;
				return new ItemPrefab.CommonnessInfo(num, num3, (num2 != null) ? num2 : ((parentInfo != null) ? parentInfo.GetValueOrDefault().caveCommonness : null));
			}

			// Token: 0x060075F4 RID: 30196 RVA: 0x003774FC File Offset: 0x003756FC
			public ItemPrefab.CommonnessInfo WithInheritedCommonness(params ItemPrefab.CommonnessInfo?[] parentInfos)
			{
				ItemPrefab.CommonnessInfo info = this;
				foreach (ItemPrefab.CommonnessInfo parentInfo in parentInfos)
				{
					info = info.WithInheritedCommonness(parentInfo);
				}
				return info;
			}

			// Token: 0x060075F5 RID: 30197 RVA: 0x00377532 File Offset: 0x00375732
			public float GetCommonness(Level.TunnelType tunnelType)
			{
				if (tunnelType == Level.TunnelType.Cave)
				{
					return this.CaveCommonness;
				}
				return this.Commonness;
			}

			// Token: 0x04004514 RID: 17684
			public readonly float commonness;

			// Token: 0x04004515 RID: 17685
			public readonly float? abyssCommonness;

			// Token: 0x04004516 RID: 17686
			public readonly float? caveCommonness;
		}

		// Token: 0x02000AA3 RID: 2723
		public readonly struct FixedQuantityResourceInfo
		{
			// Token: 0x060075F6 RID: 30198 RVA: 0x00377545 File Offset: 0x00375745
			public FixedQuantityResourceInfo(int clusterQuantity, int clusterSize, bool isIslandSpecific, bool allowAtStart)
			{
				this.ClusterQuantity = clusterQuantity;
				this.ClusterSize = clusterSize;
				this.IsIslandSpecific = isIslandSpecific;
				this.AllowAtStart = allowAtStart;
			}

			// Token: 0x04004517 RID: 17687
			public readonly int ClusterQuantity;

			// Token: 0x04004518 RID: 17688
			public readonly int ClusterSize;

			// Token: 0x04004519 RID: 17689
			public readonly bool IsIslandSpecific;

			// Token: 0x0400451A RID: 17690
			public readonly bool AllowAtStart;
		}
	}
}
