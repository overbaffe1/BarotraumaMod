using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005BA RID: 1466
	internal class Deconstructor : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x1700171E RID: 5918
		// (get) Token: 0x06005BA2 RID: 23458 RVA: 0x002ED7E6 File Offset: 0x002EB9E6
		public GUIButton ActivateButton
		{
			get
			{
				return this.activateButton;
			}
		}

		// Token: 0x1700171F RID: 5919
		// (get) Token: 0x06005BA3 RID: 23459 RVA: 0x002ED7EE File Offset: 0x002EB9EE
		// (set) Token: 0x06005BA4 RID: 23460 RVA: 0x002ED7F6 File Offset: 0x002EB9F6
		[Serialize("DeconstructorDeconstruct", IsPropertySaveable.Yes, "", "", false)]
		public string ActivateButtonText { get; set; }

		// Token: 0x17001720 RID: 5920
		// (get) Token: 0x06005BA5 RID: 23461 RVA: 0x002ED7FF File Offset: 0x002EB9FF
		// (set) Token: 0x06005BA6 RID: 23462 RVA: 0x002ED807 File Offset: 0x002EBA07
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string InfoText { get; set; }

		// Token: 0x17001721 RID: 5921
		// (get) Token: 0x06005BA7 RID: 23463 RVA: 0x002ED810 File Offset: 0x002EBA10
		// (set) Token: 0x06005BA8 RID: 23464 RVA: 0x002ED818 File Offset: 0x002EBA18
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float InfoAreaWidth { get; set; }

		// Token: 0x17001722 RID: 5922
		// (get) Token: 0x06005BA9 RID: 23465 RVA: 0x002ED821 File Offset: 0x002EBA21
		// (set) Token: 0x06005BAA RID: 23466 RVA: 0x002ED829 File Offset: 0x002EBA29
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool ShowOutput { get; set; }

		// Token: 0x17001723 RID: 5923
		// (get) Token: 0x06005BAB RID: 23467 RVA: 0x002ED832 File Offset: 0x002EBA32
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06005BAC RID: 23468 RVA: 0x002ED835 File Offset: 0x002EBA35
		protected override void OnResolutionChanged()
		{
			this.OnItemLoadedProjSpecific();
		}

		// Token: 0x06005BAD RID: 23469 RVA: 0x002ED840 File Offset: 0x002EBA40
		protected override void CreateGUI()
		{
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.88f), base.GuiFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				Stretch = true,
				RelativeSpacing = 0.08f
			};
			RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.07f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MinSize = new Point(0, GUI.IntScale(25f));
			RichString text = this.item.Prefab.Name;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectTransform, text, null, subHeadingFont, Alignment.Left, false, "", null);
			guitextBlock.TextAlignment = Alignment.Center;
			guitextBlock.AutoScaleHorizontal = true;
			GUIFrame topFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.375f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup inputLabelArea = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.15f), topFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUILayoutGroup queueLabelLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.43f, 1f), inputLabelArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			RectTransform rectT = new RectTransform(Vector2.One, queueLabelLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("deconstructor.inputqueue");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock queueLabel = new GUITextBlock(rectT, text2, null, subHeadingFont, Alignment.Left, false, "", null)
			{
				Padding = Vector4.Zero
			};
			queueLabel.RectTransform.Resize(new Point((int)queueLabel.Font.MeasureString(queueLabel.Text, false).X, queueLabel.RectTransform.Rect.Height), true);
			new GUIFrame(new RectTransform(Vector2.One, queueLabelLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			GUILayoutGroup inputLabelLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.57f, 1f), inputLabelArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			RectTransform rectT2 = new RectTransform(Vector2.One, inputLabelLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get(new string[]
			{
				"deconstructor.input",
				"uilabel.input"
			});
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock inputLabel = new GUITextBlock(rectT2, text3, null, subHeadingFont, Alignment.Left, false, "", null)
			{
				Padding = Vector4.Zero
			};
			inputLabel.RectTransform.Resize(new Point((int)inputLabel.Font.MeasureString(inputLabel.Text, false).X, inputLabel.RectTransform.Rect.Height), true);
			new GUIFrame(new RectTransform(Vector2.One, inputLabelLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), topFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			this.inputInventoryHolder = new GUIFrame(new RectTransform(new Vector2(0.7f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUICustomComponent(new RectTransform(Vector2.One, this.inputInventoryHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawOverLay), null).CanBeFocused = false;
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 0.8f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.CenterLeft);
			this.activateButton = new GUIButton(new RectTransform(new Vector2(0.95f, 0.8f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("DeconstructorDeconstruct"), Alignment.Center, "DeviceButton", null)
			{
				UserData = UIHighlightAction.ElementId.DeconstructButton,
				TextBlock = 
				{
					AutoScaleHorizontal = true
				},
				OnClicked = new GUIButton.OnClickedHandler(this.OnActivateButtonClicked)
			};
			this.inSufficientPowerWarning = new GUITextBlock(new RectTransform(Vector2.One, this.activateButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("DeconstructorNoPower"), new Color?(GUIStyle.Orange), null, Alignment.Center, true, "OuterGlow", new Color?(Color.Black))
			{
				HoverColor = Color.Black,
				IgnoreLayoutGroups = true,
				Visible = false,
				CanBeFocused = false,
				AutoScaleHorizontal = true
			};
			GUIFrame bottomFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.375f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup outputLabelArea = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.15f), bottomFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			RectTransform rectT3 = new RectTransform(new Vector2(0f, 1f), outputLabelArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = TextManager.Get("uilabel.output");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock outputLabel = new GUITextBlock(rectT3, text4, null, subHeadingFont, Alignment.Left, false, "", null)
			{
				Padding = Vector4.Zero
			};
			outputLabel.RectTransform.Resize(new Point((int)outputLabel.Font.MeasureString(outputLabel.Text, false).X, outputLabel.RectTransform.Rect.Height), true);
			new GUIFrame(new RectTransform(Vector2.One, outputLabelArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			GUILayoutGroup outputArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), bottomFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			this.outputInventoryHolder = new GUIFrame(new RectTransform(new Vector2(1f - this.InfoAreaWidth, 1f), outputArea.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), null, null);
			if (this.ShowOutput)
			{
				GUILayoutGroup outputDisplayLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
				GUILayoutGroup outDisplayTopGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.2f), outputDisplayLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				RectTransform rectT4 = new RectTransform(Vector2.One, outDisplayTopGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text5 = TextManager.Get("deconstructor.output");
				subHeadingFont = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT4, text5, null, subHeadingFont, Alignment.Left, false, "", null).Padding = Vector4.Zero;
				GUILayoutGroup outDisplayBottomGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.975f, 0.8f), outputDisplayLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				this.outputDisplayListBox = new GUIListBox(new RectTransform(new Vector2(1f, 1f), outDisplayBottomGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, null, null, true, false);
			}
			if (this.InfoAreaWidth >= 0f)
			{
				GUILayoutGroup infoAreaContainer = new GUILayoutGroup(new RectTransform(new Vector2(this.InfoAreaWidth, 0.8f), outputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.CenterLeft);
				this.infoArea = new GUITextBlock(new RectTransform(new Vector2(0.95f, 0.95f), infoAreaContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, true, "", null);
			}
			GUIButton guibutton = this.ActivateButton;
			guibutton.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guibutton.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent component)
			{
				this.activateButton.Enabled = true;
				if (string.IsNullOrEmpty(this.InfoText))
				{
					this.infoArea.Text = string.Empty;
				}
				else
				{
					this.infoArea.Text = TextManager.Get(this.InfoText).Fallback(this.InfoText, true);
				}
				if (this.IsActive)
				{
					this.activateButton.Text = TextManager.Get("DeconstructorCancel");
					this.infoArea.Text = string.Empty;
					return;
				}
				bool outputsFound = false;
				foreach (ValueTuple<Item, DeconstructItem> valueTuple in this.GetAvailableOutputs(true))
				{
					Item inputItem = valueTuple.Item1;
					DeconstructItem deconstructItem = valueTuple.Item2;
					outputsFound = true;
					if (!string.IsNullOrEmpty(deconstructItem.ActivateButtonText))
					{
						LocalizedString buttonText = TextManager.Get(deconstructItem.ActivateButtonText).Fallback(deconstructItem.ActivateButtonText, true);
						LocalizedString infoText = string.Empty;
						if (!string.IsNullOrEmpty(deconstructItem.InfoText))
						{
							infoText = TextManager.Get(deconstructItem.InfoText).Fallback(deconstructItem.InfoText, true);
						}
						GeneticMaterial component2 = inputItem.GetComponent<GeneticMaterial>();
						if (component2 != null)
						{
							component2.ModifyDeconstructInfo(this, ref buttonText, ref infoText);
						}
						this.activateButton.Text = buttonText;
						this.infoArea.Text = infoText;
						return;
					}
				}
				LocalizedString activateButtonText = TextManager.Get(this.ActivateButtonText);
				this.activateButton.Enabled = (outputsFound || !this.InputContainer.Inventory.IsEmpty());
				this.activateButton.Text = activateButtonText;
				if (!outputsFound && this.infoArea != null)
				{
					foreach (ValueTuple<Item, DeconstructItem> valueTuple2 in this.GetAvailableOutputs(false))
					{
						Item inputItem2 = valueTuple2.Item1;
						DeconstructItem deconstructItem2 = valueTuple2.Item2;
						LocalizedString infoText2 = string.Empty;
						if (deconstructItem2.RequiredOtherItem.Any<Identifier>() && !string.IsNullOrEmpty(deconstructItem2.InfoTextOnOtherItemMissing))
						{
							LocalizedString missingItemName = TextManager.Get("entityname." + deconstructItem2.RequiredOtherItem.First<Identifier>().ToString());
							infoText2 = TextManager.GetWithVariable(deconstructItem2.InfoTextOnOtherItemMissing, "[itemname]", missingItemName, FormatCapitals.No);
						}
						GeneticMaterial component3 = inputItem2.GetComponent<GeneticMaterial>();
						if (component3 != null)
						{
							component3.ModifyDeconstructInfo(this, ref activateButtonText, ref infoText2);
						}
						this.activateButton.Text = activateButtonText;
						this.infoArea.Text = infoText2;
					}
				}
			}));
		}

		// Token: 0x06005BAE RID: 23470 RVA: 0x002EE3C0 File Offset: 0x002EC5C0
		public override bool Select(Character character)
		{
			if (base.GuiFrame != null)
			{
				if (this.item.linkedTo.Count(delegate(MapEntity entity)
				{
					Item item = entity as Item;
					return item != null && item.DisplaySideBySideWhenLinked;
				}) == 1)
				{
					foreach (MapEntity linkedTo in this.item.linkedTo)
					{
						Item linkedItem = linkedTo as Item;
						if (linkedItem != null && linkedItem.DisplaySideBySideWhenLinked && linkedItem.Components.Any<ItemComponent>())
						{
							ItemContainer itemContainer = linkedItem.GetComponent<ItemContainer>();
							if (((itemContainer != null) ? itemContainer.GuiFrame : null) != null && !itemContainer.AllowUIOverlap)
							{
								int padding = (int)(8f * GUI.Scale);
								itemContainer.GuiFrame.RectTransform.AbsoluteOffset = new Point(base.GuiFrame.Rect.Width / -2 - padding, 0);
								base.GuiFrame.RectTransform.AbsoluteOffset = new Point(itemContainer.GuiFrame.Rect.Width / 2 + padding, 0);
							}
						}
					}
				}
			}
			return base.Select(character);
		}

		// Token: 0x06005BAF RID: 23471 RVA: 0x002EE508 File Offset: 0x002EC708
		private void RefreshOutputDisplay(ImmutableArray<Item> items)
		{
			if (this.outputDisplayListBox == null || this.inputContainer.Inventory == null)
			{
				return;
			}
			Deconstructor.<>c__DisplayClass30_0 CS$<>8__locals1;
			CS$<>8__locals1.itemCounts = new Dictionary<Identifier, int>();
			Dictionary<Identifier, GUIComponent> children = new Dictionary<Identifier, GUIComponent>();
			bool addQuestionMark = false;
			foreach (GUIComponent child in this.outputDisplayListBox.Content.Children)
			{
				object userData = child.UserData;
				if (userData is Identifier)
				{
					Identifier it = (Identifier)userData;
					children.Add(it, child);
				}
			}
			GUIComponent foundChild = this.outputDisplayListBox.Content.FindChild("UnknownItemOutput", false);
			if (foundChild != null)
			{
				this.outputDisplayListBox.RemoveChild(foundChild);
			}
			foreach (Item it2 in items)
			{
				if (it2.Prefab.RandomDeconstructionOutput)
				{
					addQuestionMark = true;
				}
				else
				{
					foreach (DeconstructItem deconstructItem in it2.Prefab.DeconstructItems)
					{
						if (deconstructItem.IsValidDeconstructor(this.item))
						{
							float percentageHealth = it2.Condition / it2.MaxCondition;
							if (percentageHealth >= deconstructItem.MinCondition && percentageHealth <= deconstructItem.MaxCondition)
							{
								Deconstructor.<RefreshOutputDisplay>g__RegisterItem|30_3(deconstructItem.ItemIdentifier, deconstructItem.Amount, ref CS$<>8__locals1);
							}
						}
					}
				}
			}
			foreach (KeyValuePair<Identifier, GUIComponent> keyValuePair in children)
			{
				Identifier identifier;
				GUIComponent guicomponent;
				keyValuePair.Deconstruct(out identifier, out guicomponent);
				Identifier it3 = identifier;
				GUIComponent child2 = guicomponent;
				if (!CS$<>8__locals1.itemCounts.ContainsKey(it3))
				{
					this.outputDisplayListBox.RemoveChild(child2);
				}
			}
			foreach (KeyValuePair<Identifier, int> keyValuePair2 in CS$<>8__locals1.itemCounts)
			{
				Identifier identifier;
				int num;
				keyValuePair2.Deconstruct(out identifier, out num);
				Identifier it4 = identifier;
				int amount = num;
				GUIComponent child3;
				if (!children.TryGetValue(it4, out child3))
				{
					child3 = Deconstructor.<RefreshOutputDisplay>g__CreateOutputDisplayItem|30_1(it4, this.outputDisplayListBox.Content);
				}
				if (child3 != null)
				{
					Deconstructor.<RefreshOutputDisplay>g__UpdateOutputDisplayItemCount|30_2(child3, amount);
				}
			}
			if (addQuestionMark)
			{
				Deconstructor.<RefreshOutputDisplay>g__CreateQuestionMark|30_0(this.outputDisplayListBox.Content);
			}
		}

		// Token: 0x06005BB0 RID: 23472 RVA: 0x002EE778 File Offset: 0x002EC978
		private void DrawOverLay(SpriteBatch spriteBatch, GUICustomComponent overlayComponent)
		{
			Deconstructor.<>c__DisplayClass31_0 CS$<>8__locals1;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			CS$<>8__locals1.<>4__this = this;
			overlayComponent.RectTransform.SetAsLastChild();
			ItemContainer itemContainer = this.inputContainer;
			bool flag;
			if (itemContainer == null)
			{
				flag = (null != null);
			}
			else
			{
				ItemInventory inventory = itemContainer.Inventory;
				flag = (((inventory != null) ? inventory.visualSlots : null) != null);
			}
			if (!flag)
			{
				return;
			}
			if (this.DeconstructItemsSimultaneously)
			{
				for (int i = 0; i < this.InputContainer.Inventory.Capacity; i++)
				{
					if (this.InputContainer.Inventory.GetItemAt(i) != null)
					{
						this.<DrawOverLay>g__DrawProgressBar|31_0(this.InputContainer.Inventory.visualSlots[i], ref CS$<>8__locals1);
					}
				}
				return;
			}
			this.<DrawOverLay>g__DrawProgressBar|31_0(this.inputContainer.Inventory.visualSlots.Last<VisualSlot>(), ref CS$<>8__locals1);
		}

		// Token: 0x06005BB1 RID: 23473 RVA: 0x002EE831 File Offset: 0x002ECA31
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			this.inSufficientPowerWarning.Visible = (this.IsActive && !this.HasPower);
		}

		// Token: 0x06005BB2 RID: 23474 RVA: 0x002EE854 File Offset: 0x002ECA54
		private bool OnActivateButtonClicked(GUIButton button, object obj)
		{
			if (!this.IsActive)
			{
				Item disallowedItem = this.inputContainer.Inventory.FindItem((Item i) => !i.AllowDeconstruct, false);
				if (disallowedItem != null && !this.DeconstructItemsSimultaneously)
				{
					int index = this.inputContainer.Inventory.FindIndex(disallowedItem);
					if (index >= 0 && index < this.inputContainer.Inventory.visualSlots.Length)
					{
						VisualSlot slot = this.inputContainer.Inventory.visualSlots[index];
						if (slot != null)
						{
							slot.ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.9f, 0.5f);
						}
					}
					return true;
				}
			}
			if (GameMain.Client != null)
			{
				this.pendingState = !this.IsActive;
				this.item.CreateClientEvent<Deconstructor>(this);
			}
			else
			{
				this.SetActive(!this.IsActive, Character.Controlled, false);
			}
			return true;
		}

		// Token: 0x06005BB3 RID: 23475 RVA: 0x002EE944 File Offset: 0x002ECB44
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.pendingState);
		}

		// Token: 0x06005BB4 RID: 23476 RVA: 0x002EE954 File Offset: 0x002ECB54
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			ushort userID = msg.ReadUInt16();
			Character user = (userID == 0) ? null : (Entity.FindEntityByID(userID) as Character);
			this.SetActive(msg.ReadBoolean(), user, false);
			this.progressTimer = msg.ReadSingle();
		}

		// Token: 0x17001724 RID: 5924
		// (get) Token: 0x06005BB5 RID: 23477 RVA: 0x002EE994 File Offset: 0x002ECB94
		public ItemContainer InputContainer
		{
			get
			{
				return this.inputContainer;
			}
		}

		// Token: 0x17001725 RID: 5925
		// (get) Token: 0x06005BB6 RID: 23478 RVA: 0x002EE99C File Offset: 0x002ECB9C
		public ItemContainer OutputContainer
		{
			get
			{
				return this.outputContainer;
			}
		}

		// Token: 0x17001726 RID: 5926
		// (get) Token: 0x06005BB7 RID: 23479 RVA: 0x002EE9A4 File Offset: 0x002ECBA4
		// (set) Token: 0x06005BB8 RID: 23480 RVA: 0x002EE9AC File Offset: 0x002ECBAC
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DeconstructItemsSimultaneously { get; set; }

		// Token: 0x17001727 RID: 5927
		// (get) Token: 0x06005BB9 RID: 23481 RVA: 0x002EE9B5 File Offset: 0x002ECBB5
		// (set) Token: 0x06005BBA RID: 23482 RVA: 0x002EE9BD File Offset: 0x002ECBBD
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 1000f)]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float DeconstructionSpeed { get; set; }

		// Token: 0x06005BBB RID: 23483 RVA: 0x002EE9C6 File Offset: 0x002ECBC6
		public Deconstructor(Item item, ContentXElement element) : base(item, element)
		{
			this.InitProjSpecific(element);
		}

		// Token: 0x06005BBC RID: 23484 RVA: 0x002EE9E7 File Offset: 0x002ECBE7
		private void InitProjSpecific(XElement element)
		{
			this.CreateGUI();
		}

		// Token: 0x06005BBD RID: 23485 RVA: 0x002EE9F0 File Offset: 0x002ECBF0
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			List<ItemContainer> containers = this.item.GetComponents<ItemContainer>().ToList<ItemContainer>();
			if (containers.Count < 2)
			{
				DebugConsole.ThrowError("Error in item \"" + this.item.Name + "\": Deconstructors must have two ItemContainer components!", null, null, false, false);
				return;
			}
			this.inputContainer = containers[0];
			this.outputContainer = containers[1];
			Identifier eventIdentifier = new Identifier("Deconstructor");
			this.inputContainer.OnContainedItemsChanged.RegisterOverwriteExisting(eventIdentifier, new Action<ItemContainer>(this.OnItemSlotsChanged));
			this.OnItemLoadedProjSpecific();
		}

		// Token: 0x06005BBE RID: 23486 RVA: 0x002EEA8C File Offset: 0x002ECC8C
		private void OnItemLoadedProjSpecific()
		{
			this.inputContainer.AllowUIOverlap = true;
			this.inputContainer.Inventory.RectTransform = this.inputInventoryHolder.RectTransform;
			this.outputContainer.AllowUIOverlap = true;
			this.outputContainer.Inventory.RectTransform = this.outputInventoryHolder.RectTransform;
			this.inputContainer.Inventory.Locked = this.IsActive;
		}

		// Token: 0x06005BBF RID: 23487 RVA: 0x002EEAFD File Offset: 0x002ECCFD
		private void OnItemSlotsChanged(ItemContainer container)
		{
			if (container.Inventory == null)
			{
				return;
			}
			this.RefreshOutputDisplay(container.Inventory.AllItems.ToImmutableArray<Item>());
		}

		// Token: 0x06005BC0 RID: 23488 RVA: 0x002EEB20 File Offset: 0x002ECD20
		public override void Update(float deltaTime, Camera cam)
		{
			this.MoveInputQueue();
			if (this.inputContainer == null || this.inputContainer.Inventory.IsEmpty())
			{
				this.SetActive(false, null, false);
				return;
			}
			if (!this.HasPower)
			{
				return;
			}
			Repairable repairable = this.item.GetComponent<Repairable>();
			if (repairable != null)
			{
				repairable.LastActiveTime = (float)Timing.TotalTime + 10f;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			this.progressTimer += deltaTime * Math.Min((this.powerConsumption <= 0f) ? 1f : base.Voltage, 2f);
			float tinkeringStrength = 0f;
			if (repairable.IsTinkering)
			{
				tinkeringStrength = repairable.TinkeringStrength;
			}
			float deconstructionSpeedModifier = this.userDeconstructorSpeedMultiplier * (1f + tinkeringStrength * 2.5f);
			float deconstructionSpeed = this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.DeconstructorSpeed, this.DeconstructionSpeed);
			if (this.DeconstructItemsSimultaneously)
			{
				float deconstructTime = 0f;
				foreach (Item targetItem3 in this.inputContainer.Inventory.AllItems)
				{
					Submarine submarine = this.item.Submarine;
					if (submarine == null)
					{
						goto IL_143;
					}
					SubmarineInfo info = submarine.Info;
					if (info == null || info.Type != SubmarineType.Outpost)
					{
						goto IL_143;
					}
					float num = targetItem3.Prefab.DeconstructTimeInOutposts;
					IL_15D:
					float itemDeconstructTime = num;
					float targetDeconstructTime = itemDeconstructTime / (deconstructionSpeed * deconstructionSpeedModifier);
					LinkedControllerCharacterComponent linkedCharacter = targetItem3.GetComponent<LinkedControllerCharacterComponent>();
					if (linkedCharacter != null)
					{
						targetDeconstructTime *= linkedCharacter.DeconstructTimeMultiplier;
					}
					deconstructTime += targetDeconstructTime;
					this.ApplyDeconstructionStatusEffects(targetItem3, ActionType.OnDeconstructing, deltaTime);
					continue;
					IL_143:
					num = targetItem3.Prefab.DeconstructTime;
					goto IL_15D;
				}
				this.progressState = Math.Min(this.progressTimer / deconstructTime, 1f);
				if (this.progressTimer > deconstructTime)
				{
					List<Item> items = this.inputContainer.Inventory.AllItems.ToList<Item>();
					using (List<Item>.Enumerator enumerator2 = items.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Item targetItem = enumerator2.Current;
							EntitySpawner spawner = Entity.Spawner;
							if ((spawner == null || !spawner.IsInRemoveQueue(targetItem)) && this.inputContainer.Inventory.AllItems.Contains(targetItem))
							{
								Func<Identifier, bool> <>9__1;
								List<DeconstructItem> validDeconstructItems = targetItem.Prefab.DeconstructItems.Where(delegate(DeconstructItem it)
								{
									if (!it.IsValidDeconstructor(this.item))
									{
										return false;
									}
									if (it.RequiredOtherItem.Length != 0)
									{
										IEnumerable<Identifier> requiredOtherItem = it.RequiredOtherItem;
										Func<Identifier, bool> predicate;
										if ((predicate = <>9__1) == null)
										{
											predicate = (<>9__1 = ((Identifier r) => items.Any((Item it) => it != targetItem && (it.HasTag(r) || it.Prefab.Identifier == r))));
										}
										return requiredOtherItem.Any(predicate);
									}
									return true;
								}).ToList<DeconstructItem>();
								this.ProcessItem(targetItem, items, validDeconstructItems, validDeconstructItems.Any<DeconstructItem>() || !targetItem.Prefab.DeconstructItems.Any<DeconstructItem>());
							}
						}
					}
					this.progressTimer = 0f;
					this.progressState = 0f;
					return;
				}
			}
			else
			{
				Item targetItem2 = this.inputContainer.Inventory.LastOrDefault();
				if (targetItem2 == null)
				{
					return;
				}
				this.ApplyDeconstructionStatusEffects(targetItem2, ActionType.OnDeconstructing, deltaTime);
				List<DeconstructItem> validDeconstructItems2 = (from it in targetItem2.Prefab.DeconstructItems
				where it.IsValidDeconstructor(this.item)
				select it).ToList<DeconstructItem>();
				Submarine submarine = this.item.Submarine;
				float num2;
				if (submarine != null)
				{
					SubmarineInfo info = submarine.Info;
					if (info != null && info.Type == SubmarineType.Outpost)
					{
						num2 = targetItem2.Prefab.DeconstructTimeInOutposts;
						goto IL_38E;
					}
				}
				num2 = targetItem2.Prefab.DeconstructTime;
				IL_38E:
				float itemDeconstructTime2 = num2;
				float deconstructTime2 = (!targetItem2.Prefab.DeconstructItems.Any<DeconstructItem>() || validDeconstructItems2.Any<DeconstructItem>()) ? (itemDeconstructTime2 / (deconstructionSpeed * deconstructionSpeedModifier)) : 1f;
				LinkedControllerCharacterComponent linkedCharacter2 = targetItem2.GetComponent<LinkedControllerCharacterComponent>();
				if (linkedCharacter2 != null)
				{
					deconstructTime2 *= linkedCharacter2.DeconstructTimeMultiplier;
				}
				this.progressState = Math.Min(this.progressTimer / deconstructTime2, 1f);
				if (this.progressTimer > deconstructTime2)
				{
					this.ProcessItem(targetItem2, this.inputContainer.Inventory.AllItemsMod, validDeconstructItems2, validDeconstructItems2.Any<DeconstructItem>() || !targetItem2.Prefab.DeconstructItems.Any<DeconstructItem>());
					this.progressTimer = 0f;
					this.progressState = 0f;
				}
			}
		}

		// Token: 0x06005BC1 RID: 23489 RVA: 0x002EEF90 File Offset: 0x002ED190
		private void ProcessItem(Item targetItem, IEnumerable<Item> inputItems, List<DeconstructItem> validDeconstructItems, bool allowRemove = true)
		{
			Deconstructor.<>c__DisplayClass62_0 CS$<>8__locals1 = new Deconstructor.<>c__DisplayClass62_0();
			CS$<>8__locals1.targetItem = targetItem;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.allowRemove = allowRemove;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			float amountMultiplier = 1f;
			if (this.user != null && !this.user.Removed)
			{
				AbilityDeconstructedItem abilityTargetItem = new AbilityDeconstructedItem(CS$<>8__locals1.targetItem, this.user);
				this.user.CheckTalents(AbilityEffectType.OnItemDeconstructed, abilityTargetItem);
				foreach (Character character in Character.GetFriendlyCrew(this.user))
				{
					character.CheckTalents(AbilityEffectType.OnItemDeconstructedByAlly, abilityTargetItem);
				}
				AbilityItemCreationMultiplier itemCreationMultiplier = new AbilityItemCreationMultiplier(CS$<>8__locals1.targetItem.Prefab, amountMultiplier);
				this.user.CheckTalents(AbilityEffectType.OnItemDeconstructedMaterial, itemCreationMultiplier);
				amountMultiplier = (float)((int)itemCreationMultiplier.Value);
			}
			if (CS$<>8__locals1.targetItem.Prefab.RandomDeconstructionOutput)
			{
				int amount = CS$<>8__locals1.targetItem.Prefab.RandomDeconstructionOutputAmount;
				List<int> deconstructItemIndexes = new List<int>();
				for (int k = 0; k < validDeconstructItems.Count; k++)
				{
					deconstructItemIndexes.Add(k);
				}
				List<float> commonness = (from i in validDeconstructItems
				select i.Commonness).ToList<float>();
				List<DeconstructItem> products = new List<DeconstructItem>();
				int j = 0;
				while (j < amount && deconstructItemIndexes.Count >= 1)
				{
					int itemIndex = ToolBox.SelectWeightedRandom<int>(deconstructItemIndexes, commonness, Rand.RandSync.Unsynced);
					products.Add(validDeconstructItems[itemIndex]);
					int removeIndex = deconstructItemIndexes.IndexOf(itemIndex);
					deconstructItemIndexes.RemoveAt(removeIndex);
					commonness.RemoveAt(removeIndex);
					j++;
				}
				using (List<DeconstructItem>.Enumerator enumerator2 = products.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						DeconstructItem deconstructProduct = enumerator2.Current;
						CS$<>8__locals1.<ProcessItem>g__CreateDeconstructProduct|0(deconstructProduct, inputItems, (int)(amountMultiplier * (float)deconstructProduct.Amount));
					}
					goto IL_22B;
				}
			}
			foreach (DeconstructItem deconstructProduct2 in validDeconstructItems)
			{
				CS$<>8__locals1.<ProcessItem>g__CreateDeconstructProduct|0(deconstructProduct2, inputItems, (int)(amountMultiplier * (float)deconstructProduct2.Amount));
			}
			IL_22B:
			if (CS$<>8__locals1.targetItem.Prefab.ContentPackage == ContentPackageManager.VanillaCorePackage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
			{
				string str = "ItemDeconstructed:";
				GameSession gameSession = GameMain.GameSession;
				string text;
				if (gameSession == null)
				{
					text = null;
				}
				else
				{
					GameMode gameMode = gameSession.GameMode;
					text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
				}
				GameAnalyticsManager.AddDesignEvent(str + (text ?? "none") + ":" + CS$<>8__locals1.targetItem.Prefab.Identifier.ToString());
			}
			CS$<>8__locals1.should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventItemDeconstructed>(delegate(IEventItemDeconstructed x)
			{
				bool? flag = x.OnItemDeconstructed(CS$<>8__locals1.targetItem, CS$<>8__locals1.<>4__this, CS$<>8__locals1.<>4__this.user, CS$<>8__locals1.allowRemove);
				CS$<>8__locals1.should = ((flag != null) ? flag : CS$<>8__locals1.should);
			});
			if (CS$<>8__locals1.should.GetValueOrDefault())
			{
				return;
			}
			if (CS$<>8__locals1.targetItem.AllowDeconstruct & CS$<>8__locals1.allowRemove)
			{
				this.ApplyDeconstructionStatusEffects(CS$<>8__locals1.targetItem, ActionType.OnDeconstructed, 1f);
				foreach (ItemContainer ic in CS$<>8__locals1.targetItem.GetComponents<ItemContainer>())
				{
					if (((ic != null) ? ic.Inventory : null) != null && !ic.RemoveContainedItemsOnDeconstruct)
					{
						foreach (Item outputItem in ic.Inventory.AllItemsMod)
						{
							CS$<>8__locals1.<ProcessItem>g__tryPutInOutputSlots|2(outputItem);
							if (this.RelocateOutputToMainSub && this.user != null)
							{
								HumanAIController humanAi = this.user.AIController as HumanAIController;
								if (humanAi != null)
								{
									humanAi.HandleRelocation(outputItem);
								}
							}
						}
					}
				}
				this.inputContainer.Inventory.RemoveItem(CS$<>8__locals1.targetItem);
				Entity.Spawner.AddItemToRemoveQueue(CS$<>8__locals1.targetItem);
				this.MoveInputQueue();
				this.PutItemsToLinkedContainer();
				return;
			}
			EntitySpawner spawner = Entity.Spawner;
			if (spawner != null && spawner.IsInRemoveQueue(CS$<>8__locals1.targetItem))
			{
				CS$<>8__locals1.targetItem.Drop(null, true, true);
				return;
			}
			CS$<>8__locals1.<ProcessItem>g__tryPutInOutputSlots|2(CS$<>8__locals1.targetItem);
		}

		// Token: 0x06005BC2 RID: 23490 RVA: 0x002EF41C File Offset: 0x002ED61C
		private void TryMoveItemToOutputContainers(Item spawnedItem)
		{
			for (int i = 0; i < this.outputContainer.Capacity; i++)
			{
				Item containedItem = this.outputContainer.Inventory.GetItemAt(i);
				bool combined = false;
				if (((containedItem != null) ? containedItem.OwnInventory : null) != null)
				{
					foreach (Item subItem in containedItem.ContainedItems.ToList<Item>())
					{
						if (subItem.Combine(spawnedItem, null))
						{
							combined = true;
							break;
						}
					}
				}
				if (!combined && containedItem != null && containedItem.Combine(spawnedItem, null))
				{
					break;
				}
			}
			this.PutItemsToLinkedContainer();
		}

		// Token: 0x06005BC3 RID: 23491 RVA: 0x002EF4D0 File Offset: 0x002ED6D0
		private void PutItemsToLinkedContainer()
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.outputContainer.Inventory.IsEmpty())
			{
				return;
			}
			foreach (MapEntity linkedTo in this.item.linkedTo)
			{
				Item linkedItem = linkedTo as Item;
				if (linkedItem != null)
				{
					if (linkedItem.GetComponent<Fabricator>() == null)
					{
						ItemContainer itemContainer = linkedItem.GetComponent<ItemContainer>();
						if (itemContainer != null)
						{
							this.outputContainer.Inventory.AllItemsMod.ForEach(delegate(Item containedItem)
							{
								itemContainer.Inventory.TryPutItem(containedItem, null, null, true, false, true);
							});
						}
					}
				}
			}
		}

		// Token: 0x06005BC4 RID: 23492 RVA: 0x002EF59C File Offset: 0x002ED79C
		private void ApplyDeconstructionStatusEffects(Item targetItem, ActionType type, float deltaTime)
		{
			Deconstructor.<>c__DisplayClass65_0 CS$<>8__locals1 = new Deconstructor.<>c__DisplayClass65_0();
			CS$<>8__locals1.<>4__this = this;
			LinkedControllerCharacterComponent linkedCharacterComponent = targetItem.GetComponent<LinkedControllerCharacterComponent>();
			CS$<>8__locals1.character = null;
			if (linkedCharacterComponent != null)
			{
				Character character = linkedCharacterComponent.Character;
				if (character != null && !character.Removed)
				{
					CS$<>8__locals1.character = linkedCharacterComponent.Character;
				}
			}
			Character character2 = CS$<>8__locals1.character;
			Limb limb = (character2 != null) ? character2.AnimController.Limbs.GetRandomUnsynced<Limb>() : null;
			if (this.user != null)
			{
				this.item.GetStatusEffectsOfType(type).ForEach(delegate(StatusEffect statusEffect)
				{
					statusEffect.SetUser(CS$<>8__locals1.<>4__this.user);
				});
				targetItem.GetStatusEffectsOfType(type).ForEach(delegate(StatusEffect statusEffect)
				{
					statusEffect.SetUser(CS$<>8__locals1.<>4__this.user);
				});
			}
			this.item.ApplyStatusEffects(type, deltaTime, CS$<>8__locals1.character, limb, targetItem, false, null);
			targetItem.ApplyStatusEffects(type, deltaTime, CS$<>8__locals1.character, limb, null, false, null);
			if (CS$<>8__locals1.character != null)
			{
				if (type == ActionType.OnDeconstructed)
				{
					CS$<>8__locals1.<ApplyDeconstructionStatusEffects>g__MoveItemsFromCharacterToOutput|3();
				}
				CS$<>8__locals1.character.ApplyStatusEffects(type, deltaTime);
				if (type == ActionType.OnDeconstructed)
				{
					CoroutineManager.Invoke(delegate
					{
						if (CS$<>8__locals1.character.Removed)
						{
							return;
						}
						base.<ApplyDeconstructionStatusEffects>g__MoveItemsFromCharacterToOutput|3();
						CS$<>8__locals1.character.Kill(CauseOfDeathType.Unknown, null, false, true);
						EntitySpawner spawner = Entity.Spawner;
						if (spawner == null)
						{
							return;
						}
						spawner.AddEntityToRemoveQueue(CS$<>8__locals1.character);
					}, 0.1f);
				}
			}
		}

		// Token: 0x06005BC5 RID: 23493 RVA: 0x002EF6B8 File Offset: 0x002ED8B8
		private void MoveInputQueue()
		{
			for (int i = this.inputContainer.Inventory.Capacity - 2; i >= 0; i--)
			{
				Item item;
				do
				{
					item = this.inputContainer.Inventory.GetItemAt(i);
				}
				while (item != null && this.inputContainer.Inventory.CanBePutInSlot(item, i + 1, false) && this.inputContainer.Inventory.TryPutItem(item, i + 1, false, false, null, true, false, true));
			}
		}

		// Token: 0x06005BC6 RID: 23494 RVA: 0x002EF72A File Offset: 0x002ED92A
		[return: TupleElementNames(new string[]
		{
			"item",
			"output"
		})]
		private IEnumerable<ValueTuple<Item, DeconstructItem>> GetAvailableOutputs(bool checkRequiredOtherItems = true)
		{
			Deconstructor.<GetAvailableOutputs>d__67 <GetAvailableOutputs>d__ = new Deconstructor.<GetAvailableOutputs>d__67(-2);
			<GetAvailableOutputs>d__.<>4__this = this;
			<GetAvailableOutputs>d__.<>3__checkRequiredOtherItems = checkRequiredOtherItems;
			return <GetAvailableOutputs>d__;
		}

		// Token: 0x06005BC7 RID: 23495 RVA: 0x002EF744 File Offset: 0x002ED944
		public void SetActive(bool active, Character user = null, bool createNetworkEvent = false)
		{
			this.PutItemsToLinkedContainer();
			this.user = user;
			this.RelocateOutputToMainSub = false;
			if (this.inputContainer.Inventory.IsEmpty())
			{
				active = false;
			}
			this.IsActive = active;
			this.userDeconstructorSpeedMultiplier = ((user != null) ? (1f + user.GetStatValue(StatTypes.DeconstructorSpeedMultiplier, true)) : 1f);
			if (!this.IsActive)
			{
				this.progressTimer = 0f;
				this.progressState = 0f;
			}
			else
			{
				HintManager.OnStartDeconstructing(user, this);
				Submarine submarine = base.Item.Submarine;
				if (submarine != null)
				{
					SubmarineInfo info = submarine.Info;
					if (info != null && info.IsOutpost && user != null && user.IsBot)
					{
						HintManager.OnItemMarkedForRelocation();
					}
				}
			}
			this.inputContainer.Inventory.Locked = this.IsActive;
		}

		// Token: 0x06005BC9 RID: 23497 RVA: 0x002EFAE0 File Offset: 0x002EDCE0
		[CompilerGenerated]
		internal static void <RefreshOutputDisplay>g__RegisterItem|30_3(Identifier identifier, int amount = 1, ref Deconstructor.<>c__DisplayClass30_0 A_2)
		{
			if (A_2.itemCounts.ContainsKey(identifier))
			{
				Dictionary<Identifier, int> itemCounts = A_2.itemCounts;
				itemCounts[identifier] += amount;
				return;
			}
			A_2.itemCounts.Add(identifier, amount);
		}

		// Token: 0x06005BCA RID: 23498 RVA: 0x002EFB24 File Offset: 0x002EDD24
		[CompilerGenerated]
		internal static void <RefreshOutputDisplay>g__CreateQuestionMark|30_0(GUIComponent parent)
		{
			GUIFrame itemFrame = new GUIFrame(new RectTransform(new Vector2(0.1f, 1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				UserData = "UnknownItemOutput",
				ToolTip = TextManager.Get("deconstructor.unknownitemsoutput")
			};
			GUIFrame questionMarkFrame = new GUIFrame(new RectTransform(Vector2.One, itemFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Smallest), "GUIFrameListBox", null)
			{
				CanBeFocused = false
			};
			RectTransform rectT = new RectTransform(Vector2.One, questionMarkFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			RichString text = "?";
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Center, false, "", null).CanBeFocused = false;
		}

		// Token: 0x06005BCB RID: 23499 RVA: 0x002EFC4C File Offset: 0x002EDE4C
		[CompilerGenerated]
		internal static GUIComponent <RefreshOutputDisplay>g__CreateOutputDisplayItem|30_1(Identifier identifier, GUIComponent parent)
		{
			ItemPrefab prefab = ItemPrefab.Find(null, identifier);
			if (prefab == null)
			{
				return null;
			}
			GUIFrame itemFrame = new GUIFrame(new RectTransform(new Vector2(0.1f, 1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				UserData = identifier,
				ToolTip = prefab.CreateTooltipText()
			};
			Sprite icon = prefab.InventoryIcon ?? prefab.Sprite;
			Color iconColor = (prefab.InventoryIcon == null) ? prefab.SpriteColor : prefab.InventoryIconColor;
			GUIImage itemIcon = new GUIImage(new RectTransform(Vector2.One, itemFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Smallest), icon, true, null)
			{
				Color = iconColor,
				CanBeFocused = false
			};
			RectTransform rectT = new RectTransform(new Vector2(0.5f, 0.5f), itemIcon.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal);
			RichString text = "";
			GUIFont font = GUIStyle.Font;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, font, Alignment.BottomRight, false, "", null);
			guitextBlock.UserData = "OutputItemCount";
			guitextBlock.Shadow = true;
			guitextBlock.CanBeFocused = false;
			guitextBlock.Padding = Vector4.Zero;
			guitextBlock.TextColor = Color.White;
			return itemFrame;
		}

		// Token: 0x06005BCC RID: 23500 RVA: 0x002EFDE0 File Offset: 0x002EDFE0
		[CompilerGenerated]
		internal static void <RefreshOutputDisplay>g__UpdateOutputDisplayItemCount|30_2(GUIComponent component, int count)
		{
			GUITextBlock textBlock = component.FindChild("OutputItemCount", true) as GUITextBlock;
			if (textBlock == null)
			{
				return;
			}
			textBlock.Text = TextManager.GetWithVariable("campaignstore.quantity", "[amount]", count.ToString(), FormatCapitals.No);
		}

		// Token: 0x06005BCD RID: 23501 RVA: 0x002EFE2C File Offset: 0x002EE02C
		[CompilerGenerated]
		private void <DrawOverLay>g__DrawProgressBar|31_0(VisualSlot slot, ref Deconstructor.<>c__DisplayClass31_0 A_2)
		{
			GUI.DrawRectangle(A_2.spriteBatch, new Rectangle(slot.Rect.X, slot.Rect.Y + (int)((float)slot.Rect.Height * (1f - this.progressState)), slot.Rect.Width, (int)((float)slot.Rect.Height * this.progressState)), GUIStyle.Green * 0.5f, true, 0f, 1f);
		}

		// Token: 0x04002EAD RID: 11949
		private GUIButton activateButton;

		// Token: 0x04002EAE RID: 11950
		private GUIComponent inputInventoryHolder;

		// Token: 0x04002EAF RID: 11951
		private GUIComponent outputInventoryHolder;

		// Token: 0x04002EB0 RID: 11952
		private GUIListBox outputDisplayListBox;

		// Token: 0x04002EB1 RID: 11953
		private GUIComponent inSufficientPowerWarning;

		// Token: 0x04002EB2 RID: 11954
		private bool pendingState;

		// Token: 0x04002EB3 RID: 11955
		private GUITextBlock infoArea;

		// Token: 0x04002EB8 RID: 11960
		private float progressTimer;

		// Token: 0x04002EB9 RID: 11961
		private float progressState;

		// Token: 0x04002EBA RID: 11962
		private Character user;

		// Token: 0x04002EBB RID: 11963
		private float userDeconstructorSpeedMultiplier = 1f;

		// Token: 0x04002EBC RID: 11964
		private const float TinkeringSpeedIncrease = 2.5f;

		// Token: 0x04002EBD RID: 11965
		private ItemContainer inputContainer;

		// Token: 0x04002EBE RID: 11966
		private ItemContainer outputContainer;

		// Token: 0x04002EBF RID: 11967
		public bool RelocateOutputToMainSub;
	}
}
