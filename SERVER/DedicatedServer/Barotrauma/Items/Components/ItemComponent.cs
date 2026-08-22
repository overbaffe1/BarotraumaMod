using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000497 RID: 1175
	internal class ItemComponent : ISerializableEntity
	{
		// Token: 0x06003FAC RID: 16300 RVA: 0x001996EC File Offset: 0x001978EC
		private bool LoadElemProjSpecific(ContentXElement subElement)
		{
			string a = subElement.Name.ToString().ToLowerInvariant();
			return a == "guiframe" || a == "sound";
		}

		// Token: 0x06003FAD RID: 16301 RVA: 0x00199727 File Offset: 0x00197927
		public virtual ItemComponent.IEventData ServerGetEventData()
		{
			return null;
		}

		// Token: 0x170010DB RID: 4315
		// (get) Token: 0x06003FAE RID: 16302 RVA: 0x0019972A File Offset: 0x0019792A
		// (set) Token: 0x06003FAF RID: 16303 RVA: 0x00199734 File Offset: 0x00197934
		public ItemComponent Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				if (this.parent == value)
				{
					return;
				}
				if (this.InheritParentIsActive)
				{
					if (this.parent != null)
					{
						ItemComponent itemComponent = this.parent;
						itemComponent.OnActiveStateChanged = (Action<bool>)Delegate.Remove(itemComponent.OnActiveStateChanged, new Action<bool>(this.SetActiveState));
					}
					if (value != null)
					{
						value.OnActiveStateChanged = (Action<bool>)Delegate.Combine(value.OnActiveStateChanged, new Action<bool>(this.SetActiveState));
					}
				}
				this.parent = value;
			}
		}

		// Token: 0x170010DC RID: 4316
		// (get) Token: 0x06003FB0 RID: 16304 RVA: 0x001997AE File Offset: 0x001979AE
		// (set) Token: 0x06003FB1 RID: 16305 RVA: 0x001997B6 File Offset: 0x001979B6
		[Serialize(true, IsPropertySaveable.No, "If this is a child component of another component, should this component inherit the IsActive state of the parent?", "", false)]
		public bool InheritParentIsActive { get; set; }

		// Token: 0x170010DD RID: 4317
		// (get) Token: 0x06003FB2 RID: 16306 RVA: 0x001997BF File Offset: 0x001979BF
		public virtual bool DontTransferInventoryBetweenSubs
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170010DE RID: 4318
		// (get) Token: 0x06003FB3 RID: 16307 RVA: 0x001997C2 File Offset: 0x001979C2
		public virtual bool DisallowSellingItemsFromContainer
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170010DF RID: 4319
		// (get) Token: 0x06003FB4 RID: 16308 RVA: 0x001997C5 File Offset: 0x001979C5
		// (set) Token: 0x06003FB5 RID: 16309 RVA: 0x001997CD File Offset: 0x001979CD
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "How long it takes to pick up the item (in seconds).", "", false)]
		public float PickingTime { get; set; }

		// Token: 0x170010E0 RID: 4320
		// (get) Token: 0x06003FB6 RID: 16310 RVA: 0x001997D6 File Offset: 0x001979D6
		// (set) Token: 0x06003FB7 RID: 16311 RVA: 0x001997DE File Offset: 0x001979DE
		[Serialize("", IsPropertySaveable.No, "What to display on the progress bar when this item is being picked.", "", false)]
		public string PickingMsg { get; set; }

		// Token: 0x170010E1 RID: 4321
		// (get) Token: 0x06003FB8 RID: 16312 RVA: 0x001997E7 File Offset: 0x001979E7
		// (set) Token: 0x06003FB9 RID: 16313 RVA: 0x001997EF File Offset: 0x001979EF
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; protected set; }

		// Token: 0x170010E2 RID: 4322
		// (get) Token: 0x06003FBA RID: 16314 RVA: 0x001997F8 File Offset: 0x001979F8
		// (set) Token: 0x06003FBB RID: 16315 RVA: 0x00199800 File Offset: 0x00197A00
		public virtual bool IsActive
		{
			get
			{
				return this.isActive;
			}
			set
			{
				if (value != this.IsActive)
				{
					Action<bool> onActiveStateChanged = this.OnActiveStateChanged;
					if (onActiveStateChanged != null)
					{
						onActiveStateChanged(value);
					}
				}
				this.isActive = value;
			}
		}

		// Token: 0x170010E3 RID: 4323
		// (get) Token: 0x06003FBC RID: 16316 RVA: 0x00199824 File Offset: 0x00197A24
		// (set) Token: 0x06003FBD RID: 16317 RVA: 0x0019982C File Offset: 0x00197A2C
		[Serialize(PropertyConditional.LogicalOperatorType.And, IsPropertySaveable.No, "", "", false)]
		public PropertyConditional.LogicalOperatorType IsActiveConditionalComparison { get; set; }

		// Token: 0x170010E4 RID: 4324
		// (get) Token: 0x06003FBE RID: 16318 RVA: 0x00199835 File Offset: 0x00197A35
		// (set) Token: 0x06003FBF RID: 16319 RVA: 0x00199840 File Offset: 0x00197A40
		public bool Drawable
		{
			get
			{
				return this.drawable;
			}
			set
			{
				if (value == this.drawable)
				{
					return;
				}
				if (!(this is IDrawableComponent))
				{
					DebugConsole.ThrowError("Couldn't make \"" + ((this != null) ? this.ToString() : null) + "\" drawable (the component doesn't implement the IDrawableComponent interface)", null, null, false, false);
					return;
				}
				this.drawable = value;
				if (this.drawable)
				{
					this.item.EnableDrawableComponent((IDrawableComponent)this);
					return;
				}
				this.item.DisableDrawableComponent((IDrawableComponent)this);
			}
		}

		// Token: 0x170010E5 RID: 4325
		// (get) Token: 0x06003FC0 RID: 16320 RVA: 0x001998B7 File Offset: 0x00197AB7
		// (set) Token: 0x06003FC1 RID: 16321 RVA: 0x001998BF File Offset: 0x00197ABF
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Can the item be picked up (or interacted with, if the pick action does something else than picking up the item).", "", false)]
		public bool CanBePicked
		{
			get
			{
				return this.canBePicked;
			}
			set
			{
				this.canBePicked = value;
			}
		}

		// Token: 0x170010E6 RID: 4326
		// (get) Token: 0x06003FC2 RID: 16322 RVA: 0x001998C8 File Offset: 0x00197AC8
		// (set) Token: 0x06003FC3 RID: 16323 RVA: 0x001998D0 File Offset: 0x00197AD0
		[Serialize(false, IsPropertySaveable.No, "Should the interface of the item (if it has one) be drawn when the item is equipped.", "", false)]
		public bool DrawHudWhenEquipped { get; protected set; }

		// Token: 0x170010E7 RID: 4327
		// (get) Token: 0x06003FC4 RID: 16324 RVA: 0x001998D9 File Offset: 0x00197AD9
		// (set) Token: 0x06003FC5 RID: 16325 RVA: 0x001998E1 File Offset: 0x00197AE1
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.OnlyByStatusEffectsAndNetwork, false)]
		public bool LockGuiFramePosition { get; set; }

		// Token: 0x170010E8 RID: 4328
		// (get) Token: 0x06003FC6 RID: 16326 RVA: 0x001998EA File Offset: 0x00197AEA
		// (set) Token: 0x06003FC7 RID: 16327 RVA: 0x001998F2 File Offset: 0x00197AF2
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.OnlyByStatusEffectsAndNetwork, false)]
		public Point GuiFrameOffset { get; set; }

		// Token: 0x170010E9 RID: 4329
		// (get) Token: 0x06003FC8 RID: 16328 RVA: 0x001998FB File Offset: 0x00197AFB
		// (set) Token: 0x06003FC9 RID: 16329 RVA: 0x00199903 File Offset: 0x00197B03
		[Serialize(false, IsPropertySaveable.No, "Can the item be selected by interacting with it.", "", false)]
		public bool CanBeSelected
		{
			get
			{
				return this.canBeSelected;
			}
			set
			{
				this.canBeSelected = value;
			}
		}

		// Token: 0x170010EA RID: 4330
		// (get) Token: 0x06003FCA RID: 16330 RVA: 0x0019990C File Offset: 0x00197B0C
		// (set) Token: 0x06003FCB RID: 16331 RVA: 0x00199914 File Offset: 0x00197B14
		[Serialize(false, IsPropertySaveable.No, "Can the item be combined with other items of the same type.", "", false)]
		public bool CanBeCombined
		{
			get
			{
				return this.canBeCombined;
			}
			set
			{
				this.canBeCombined = value;
			}
		}

		// Token: 0x170010EB RID: 4331
		// (get) Token: 0x06003FCC RID: 16332 RVA: 0x0019991D File Offset: 0x00197B1D
		// (set) Token: 0x06003FCD RID: 16333 RVA: 0x00199925 File Offset: 0x00197B25
		[Serialize(false, IsPropertySaveable.No, "Should the item be removed if combining it with an other item causes the condition of this item to drop to 0.", "", false)]
		public bool RemoveOnCombined
		{
			get
			{
				return this.removeOnCombined;
			}
			set
			{
				this.removeOnCombined = value;
			}
		}

		// Token: 0x170010EC RID: 4332
		// (get) Token: 0x06003FCE RID: 16334 RVA: 0x0019992E File Offset: 0x00197B2E
		// (set) Token: 0x06003FCF RID: 16335 RVA: 0x00199936 File Offset: 0x00197B36
		[Serialize(false, IsPropertySaveable.No, "Can the \"Use\" action of the item be triggered by characters or just other items/StatusEffects.", "", false)]
		public bool CharacterUsable
		{
			get
			{
				return this.characterUsable;
			}
			set
			{
				this.characterUsable = value;
			}
		}

		// Token: 0x170010ED RID: 4333
		// (get) Token: 0x06003FD0 RID: 16336 RVA: 0x0019993F File Offset: 0x00197B3F
		// (set) Token: 0x06003FD1 RID: 16337 RVA: 0x00199947 File Offset: 0x00197B47
		[Serialize(true, IsPropertySaveable.No, "Can the properties of the component be edited in-game (only applicable if the component has in-game editable properties).", "", false)]
		[Editable]
		public bool AllowInGameEditing { get; set; }

		// Token: 0x170010EE RID: 4334
		// (get) Token: 0x06003FD2 RID: 16338 RVA: 0x00199950 File Offset: 0x00197B50
		// (set) Token: 0x06003FD3 RID: 16339 RVA: 0x00199958 File Offset: 0x00197B58
		public InputType PickKey { get; protected set; }

		// Token: 0x170010EF RID: 4335
		// (get) Token: 0x06003FD4 RID: 16340 RVA: 0x00199961 File Offset: 0x00197B61
		// (set) Token: 0x06003FD5 RID: 16341 RVA: 0x00199969 File Offset: 0x00197B69
		public InputType SelectKey { get; protected set; }

		// Token: 0x170010F0 RID: 4336
		// (get) Token: 0x06003FD6 RID: 16342 RVA: 0x00199972 File Offset: 0x00197B72
		// (set) Token: 0x06003FD7 RID: 16343 RVA: 0x0019997A File Offset: 0x00197B7A
		[Serialize(false, IsPropertySaveable.No, "Should the item be deleted when it's used.", "", false)]
		public bool DeleteOnUse { get; set; }

		// Token: 0x170010F1 RID: 4337
		// (get) Token: 0x06003FD8 RID: 16344 RVA: 0x00199983 File Offset: 0x00197B83
		public Item Item
		{
			get
			{
				return this.item;
			}
		}

		// Token: 0x170010F2 RID: 4338
		// (get) Token: 0x06003FD9 RID: 16345 RVA: 0x0019998B File Offset: 0x00197B8B
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x170010F3 RID: 4339
		// (get) Token: 0x06003FDA RID: 16346 RVA: 0x00199993 File Offset: 0x00197B93
		// (set) Token: 0x06003FDB RID: 16347 RVA: 0x0019999B File Offset: 0x00197B9B
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "A text displayed next to the item when it's highlighted (generally instructs how to interact with the item, e.g. \"[Mouse1] Pick up\").", "ItemMsg", false)]
		public string Msg { get; set; }

		// Token: 0x170010F4 RID: 4340
		// (get) Token: 0x06003FDC RID: 16348 RVA: 0x001999A4 File Offset: 0x00197BA4
		// (set) Token: 0x06003FDD RID: 16349 RVA: 0x001999AC File Offset: 0x00197BAC
		public LocalizedString DisplayMsg { get; set; }

		// Token: 0x170010F5 RID: 4341
		// (get) Token: 0x06003FDE RID: 16350 RVA: 0x001999B5 File Offset: 0x00197BB5
		// (set) Token: 0x06003FDF RID: 16351 RVA: 0x001999BD File Offset: 0x00197BBD
		[Serialize(0f, IsPropertySaveable.No, "How useful the item is in combat? Used by AI to decide which item it should use as a weapon. For the sake of clarity, use a value between 0 and 100 (not forced). Note that there's also a generic BotPriority for all item prefabs.", "", false)]
		public float CombatPriority { get; private set; }

		// Token: 0x170010F6 RID: 4342
		// (get) Token: 0x06003FE0 RID: 16352 RVA: 0x001999C6 File Offset: 0x00197BC6
		// (set) Token: 0x06003FE1 RID: 16353 RVA: 0x001999CE File Offset: 0x00197BCE
		[Serialize(0, IsPropertySaveable.Yes, "", "", true)]
		public int ManuallySelectedSound { get; private set; }

		// Token: 0x170010F7 RID: 4343
		// (get) Token: 0x06003FE2 RID: 16354 RVA: 0x001999D7 File Offset: 0x00197BD7
		public float Speed
		{
			get
			{
				return this.item.Speed;
			}
		}

		// Token: 0x06003FE3 RID: 16355 RVA: 0x001999E4 File Offset: 0x00197BE4
		public ItemComponent(Item item, ContentXElement element)
		{
			ItemComponent.<>c__DisplayClass124_0 CS$<>8__locals1;
			CS$<>8__locals1.item = item;
			base..ctor();
			CS$<>8__locals1.<>4__this = this;
			this.item = CS$<>8__locals1.item;
			this.originalElement = element;
			this.name = element.Name.ToString();
			this.SerializableProperties = SerializableProperty.GetProperties(this);
			this.RequiredItems = new Dictionary<RelatedItem.RelationType, List<RelatedItem>>();
			this.SelectKey = InputType.Select;
			try
			{
				string selectKeyStr = element.GetAttributeString("selectkey", "Select");
				selectKeyStr = ToolBox.ConvertInputType(selectKeyStr);
				this.SelectKey = (InputType)Enum.Parse(typeof(InputType), selectKeyStr, true);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Invalid select key in " + ((element != null) ? element.ToString() : null) + "!", e, element.ContentPackage, false, false);
			}
			this.PickKey = InputType.Select;
			try
			{
				string pickKeyStr = element.GetAttributeString("pickkey", "Select");
				pickKeyStr = ToolBox.ConvertInputType(pickKeyStr);
				this.PickKey = (InputType)Enum.Parse(typeof(InputType), pickKeyStr, true);
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Invalid pick key in " + ((element != null) ? element.ToString() : null) + "!", e2, element.ContentPackage, false, false);
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.ParseMsg();
			string inheritRequiredSkillsFrom = element.GetAttributeString("inheritrequiredskillsfrom", "");
			if (!string.IsNullOrEmpty(inheritRequiredSkillsFrom))
			{
				ItemComponent component = CS$<>8__locals1.item.Components.Find((ItemComponent ic) => ic.Name.Equals(inheritRequiredSkillsFrom, StringComparison.OrdinalIgnoreCase));
				if (component == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(126, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Error in item \"");
					defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.item.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" - component \"");
					defaultInterpolatedStringHandler.AppendFormatted(this.name);
					defaultInterpolatedStringHandler.AppendLiteral("\" is set to inherit its required skills from \"");
					defaultInterpolatedStringHandler.AppendFormatted(inheritRequiredSkillsFrom);
					defaultInterpolatedStringHandler.AppendLiteral("\", but a component of that type couldn't be found.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				else
				{
					this.RequiredSkills = component.RequiredSkills;
				}
			}
			string inheritStatusEffectsFrom = element.GetAttributeString("inheritstatuseffectsfrom", "");
			if (!string.IsNullOrEmpty(inheritStatusEffectsFrom))
			{
				this.InheritStatusEffects = true;
				ItemComponent component2 = CS$<>8__locals1.item.Components.Find((ItemComponent ic) => ic.Name.Equals(inheritStatusEffectsFrom, StringComparison.OrdinalIgnoreCase));
				if (component2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(124, 3);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in item \"");
					defaultInterpolatedStringHandler2.AppendFormatted(CS$<>8__locals1.item.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("\" - component \"");
					defaultInterpolatedStringHandler2.AppendFormatted(this.name);
					defaultInterpolatedStringHandler2.AppendLiteral("\" is set to inherit its StatusEffects from \"");
					defaultInterpolatedStringHandler2.AppendFormatted(inheritStatusEffectsFrom);
					defaultInterpolatedStringHandler2.AppendLiteral("\", but a component of that type couldn't be found.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				else if (component2.statusEffectLists != null)
				{
					if (this.statusEffectLists == null)
					{
						this.statusEffectLists = new Dictionary<ActionType, List<StatusEffect>>();
					}
					foreach (KeyValuePair<ActionType, List<StatusEffect>> kvp in component2.statusEffectLists)
					{
						List<StatusEffect> effectList;
						if (!this.statusEffectLists.TryGetValue(kvp.Key, out effectList))
						{
							effectList = new List<StatusEffect>();
							this.statusEffectLists.Add(kvp.Key, effectList);
						}
						effectList.AddRange(kvp.Value);
					}
				}
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					switch (length)
					{
					case 8:
						if (!(text == "isactive"))
						{
							goto IL_5E9;
						}
						break;
					case 9:
					case 10:
					case 11:
						goto IL_5E9;
					case 12:
					{
						char c = text[0];
						if (c != 'r')
						{
							if (c != 's')
							{
								goto IL_5E9;
							}
							if (!(text == "statuseffect"))
							{
								goto IL_5E9;
							}
							if (this.statusEffectLists == null)
							{
								this.statusEffectLists = new Dictionary<ActionType, List<StatusEffect>>();
							}
							this.<.ctor>g__LoadStatusEffect|124_0(subElement, ref CS$<>8__locals1);
							continue;
						}
						else
						{
							if (!(text == "requireditem"))
							{
								goto IL_5E9;
							}
							goto IL_518;
						}
						break;
					}
					case 13:
					{
						char c = text[8];
						if (c != 'i')
						{
							if (c != 's')
							{
								goto IL_5E9;
							}
							if (!(text == "requiredskill"))
							{
								goto IL_5E9;
							}
							goto IL_526;
						}
						else
						{
							if (!(text == "requireditems"))
							{
								goto IL_5E9;
							}
							goto IL_518;
						}
						break;
					}
					case 14:
						if (!(text == "requiredskills"))
						{
							goto IL_5E9;
						}
						goto IL_526;
					default:
						if (length != 17)
						{
							if (length != 19)
							{
								goto IL_5E9;
							}
							if (!(text == "isactiveconditional"))
							{
								goto IL_5E9;
							}
						}
						else if (!(text == "activeconditional"))
						{
							goto IL_5E9;
						}
						break;
					}
					if (this.IsActiveConditionals == null)
					{
						this.IsActiveConditionals = new List<PropertyConditional>();
					}
					this.IsActiveConditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
					continue;
					IL_518:
					this.SetRequiredItems(subElement, true);
					continue;
					IL_526:
					if (subElement.GetAttribute("name") != null)
					{
						string[] array = new string[5];
						array[0] = "Error in item config \"";
						int num = 1;
						ContentPath configFilePath = CS$<>8__locals1.item.ConfigFilePath;
						array[num] = ((configFilePath != null) ? configFilePath.ToString() : null);
						array[2] = "\" - skill requirement in component ";
						array[3] = base.GetType().ToString();
						array[4] = " should use a skill identifier instead of the name of the skill.";
						DebugConsole.ThrowError(string.Concat(array), null, element.ContentPackage, false, false);
						continue;
					}
					Identifier skillIdentifier = subElement.GetAttributeIdentifier("identifier", "");
					this.RequiredSkills.Add(new Skill(skillIdentifier, (float)subElement.GetAttributeInt("level", 0)));
					continue;
				}
				IL_5E9:
				if (!this.LoadElemProjSpecific(subElement))
				{
					ItemComponent ic2 = ItemComponent.Load(subElement, CS$<>8__locals1.item, false);
					if (ic2 != null)
					{
						ic2.Parent = this;
						if (ic2.InheritParentIsActive)
						{
							ic2.IsActive = this.isActive;
							this.OnActiveStateChanged = (Action<bool>)Delegate.Combine(this.OnActiveStateChanged, new Action<bool>(ic2.SetActiveState));
						}
						CS$<>8__locals1.item.AddComponent(ic2);
					}
				}
			}
		}

		// Token: 0x06003FE4 RID: 16356 RVA: 0x0019A0C4 File Offset: 0x001982C4
		private void SetActiveState(bool isActive)
		{
			this.IsActive = isActive;
		}

		// Token: 0x06003FE5 RID: 16357 RVA: 0x0019A0D0 File Offset: 0x001982D0
		public void SetRequiredItems(ContentXElement element, bool allowEmpty = false)
		{
			bool returnEmpty = false;
			RelatedItem ri = RelatedItem.Load(element, returnEmpty, this.item.Name);
			if (ri == null)
			{
				if (!allowEmpty)
				{
					string[] array = new string[5];
					array[0] = "Error in item config \"";
					int num = 1;
					ContentPath configFilePath = this.item.ConfigFilePath;
					array[num] = ((configFilePath != null) ? configFilePath.ToString() : null);
					array[2] = "\" - component ";
					array[3] = base.GetType().ToString();
					array[4] = " requires an item with no identifiers.";
					DebugConsole.ThrowError(string.Concat(array), null, element.ContentPackage, false, false);
				}
				return;
			}
			if (ri.Identifiers.Count == 0)
			{
				this.DisabledRequiredItems.Add(ri);
				return;
			}
			if (!this.RequiredItems.ContainsKey(ri.Type))
			{
				this.RequiredItems.Add(ri.Type, new List<RelatedItem>());
			}
			this.RequiredItems[ri.Type].Add(ri);
		}

		// Token: 0x06003FE6 RID: 16358 RVA: 0x0019A1AC File Offset: 0x001983AC
		public virtual void Move(Vector2 amount, bool ignoreContacts = false)
		{
		}

		// Token: 0x06003FE7 RID: 16359 RVA: 0x0019A1AE File Offset: 0x001983AE
		public virtual bool Pick(Character picker)
		{
			return false;
		}

		// Token: 0x06003FE8 RID: 16360 RVA: 0x0019A1B1 File Offset: 0x001983B1
		public virtual bool Select(Character character)
		{
			return this.CanBeSelected;
		}

		// Token: 0x06003FE9 RID: 16361 RVA: 0x0019A1B9 File Offset: 0x001983B9
		public virtual void Drop(Character dropper, bool setTransform = true)
		{
		}

		// Token: 0x06003FEA RID: 16362 RVA: 0x0019A1BB File Offset: 0x001983BB
		public virtual bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			return false;
		}

		// Token: 0x170010F8 RID: 4344
		// (get) Token: 0x06003FEB RID: 16363 RVA: 0x0019A1BE File Offset: 0x001983BE
		public virtual bool UpdateWhenInactive
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170010F9 RID: 4345
		// (get) Token: 0x06003FEC RID: 16364 RVA: 0x0019A1C1 File Offset: 0x001983C1
		// (set) Token: 0x06003FED RID: 16365 RVA: 0x0019A1C9 File Offset: 0x001983C9
		[Serialize(false, IsPropertySaveable.No, "If true, the component will retain its normal functionality when the item reaches 0 condition.", "", false)]
		public bool UpdateWhenBroken { get; set; }

		// Token: 0x06003FEE RID: 16366 RVA: 0x0019A1D4 File Offset: 0x001983D4
		public virtual void Update(float deltaTime, Camera cam)
		{
			this.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
		}

		// Token: 0x06003FEF RID: 16367 RVA: 0x0019A1FB File Offset: 0x001983FB
		public virtual void UpdateBroken(float deltaTime, Camera cam)
		{
		}

		// Token: 0x06003FF0 RID: 16368 RVA: 0x0019A1FD File Offset: 0x001983FD
		public virtual bool Use(float deltaTime, Character character = null)
		{
			return this.characterUsable || character == null;
		}

		// Token: 0x06003FF1 RID: 16369 RVA: 0x0019A20D File Offset: 0x0019840D
		public virtual bool SecondaryUse(float deltaTime, Character character = null)
		{
			return false;
		}

		// Token: 0x06003FF2 RID: 16370 RVA: 0x0019A210 File Offset: 0x00198410
		public virtual void Equip(Character character)
		{
		}

		// Token: 0x06003FF3 RID: 16371 RVA: 0x0019A212 File Offset: 0x00198412
		public virtual void Unequip(Character character)
		{
		}

		// Token: 0x06003FF4 RID: 16372 RVA: 0x0019A214 File Offset: 0x00198414
		public virtual void ReceiveSignal(Signal signal, Connection connection)
		{
			string a = connection.Name;
			if (!(a == "activate") && !(a == "use") && !(a == "trigger_in"))
			{
				if (!(a == "toggle"))
				{
					if (!(a == "set_active") && !(a == "set_state"))
					{
						return;
					}
					this.IsActive = (signal.value != "0");
				}
				else if (signal.value != "0")
				{
					this.IsActive = !this.isActive;
					return;
				}
			}
			else if (signal.value != "0")
			{
				this.item.Use(1f, signal.sender, null, null, null);
				return;
			}
		}

		// Token: 0x06003FF5 RID: 16373 RVA: 0x0019A2DC File Offset: 0x001984DC
		public virtual bool Combine(Item item, Character user)
		{
			if (!this.canBeCombined || this.item.Prefab != item.Prefab || item.Condition <= 0f || this.item.Condition <= 0f || item.IsFullCondition || this.item.IsFullCondition)
			{
				return false;
			}
			float transferAmount = Math.Min(item.Condition, this.item.MaxCondition - this.item.Condition);
			if (MathUtils.NearlyEqual(transferAmount, 0f, 0.0001f))
			{
				return false;
			}
			if (this.removeOnCombined)
			{
				if (item.Condition - transferAmount <= 0f)
				{
					if (item.ParentInventory != null)
					{
						Character owner = item.ParentInventory.Owner as Character;
						if (owner != null && owner.HeldItems.Contains(item))
						{
							item.Unequip(owner);
						}
						item.ParentInventory.RemoveItem(item);
					}
					ItemComponent.<Combine>g__RemoveItem|145_0(item);
				}
				else
				{
					item.Condition -= transferAmount;
				}
				if (this.Item.Condition + transferAmount <= 0f)
				{
					if (this.Item.ParentInventory != null)
					{
						Character owner2 = this.Item.ParentInventory.Owner as Character;
						if (owner2 != null && owner2.HeldItems.Contains(this.Item))
						{
							this.Item.Unequip(owner2);
						}
						this.Item.ParentInventory.RemoveItem(this.Item);
					}
					ItemComponent.<Combine>g__RemoveItem|145_0(this.Item);
				}
				else
				{
					this.Item.Condition += transferAmount;
				}
			}
			else
			{
				this.Item.Condition += transferAmount;
				item.Condition -= transferAmount;
			}
			return true;
		}

		// Token: 0x06003FF6 RID: 16374 RVA: 0x0019A4A3 File Offset: 0x001986A3
		public void Remove()
		{
			if (this.delayedCorrectionCoroutine != null)
			{
				CoroutineManager.StopCoroutines(this.delayedCorrectionCoroutine);
				this.delayedCorrectionCoroutine = null;
			}
			this.RemoveComponentSpecific();
		}

		// Token: 0x06003FF7 RID: 16375 RVA: 0x0019A4C5 File Offset: 0x001986C5
		public void ShallowRemove()
		{
			this.ShallowRemoveComponentSpecific();
		}

		// Token: 0x06003FF8 RID: 16376 RVA: 0x0019A4CD File Offset: 0x001986CD
		protected virtual void ShallowRemoveComponentSpecific()
		{
			this.RemoveComponentSpecific();
		}

		// Token: 0x06003FF9 RID: 16377 RVA: 0x0019A4D5 File Offset: 0x001986D5
		protected virtual void RemoveComponentSpecific()
		{
		}

		// Token: 0x06003FFA RID: 16378 RVA: 0x0019A4D7 File Offset: 0x001986D7
		protected string GetTextureDirectory(ContentXElement subElement)
		{
			return this.item.Prefab.GetTexturePath(subElement, this.item.Prefab.ParentPrefab);
		}

		// Token: 0x06003FFB RID: 16379 RVA: 0x0019A4FC File Offset: 0x001986FC
		public bool HasRequiredSkills(Character character)
		{
			Skill temp;
			return this.HasRequiredSkills(character, out temp);
		}

		// Token: 0x06003FFC RID: 16380 RVA: 0x0019A514 File Offset: 0x00198714
		public bool HasRequiredSkills(Character character, out Skill insufficientSkill)
		{
			foreach (Skill skill in this.RequiredSkills)
			{
				float characterLevel = character.GetSkillLevel(skill.Identifier);
				if (characterLevel < skill.Level * this.GetSkillMultiplier())
				{
					insufficientSkill = skill;
					return false;
				}
			}
			insufficientSkill = null;
			return true;
		}

		// Token: 0x06003FFD RID: 16381 RVA: 0x0019A58C File Offset: 0x0019878C
		public virtual float GetSkillMultiplier()
		{
			return 1f;
		}

		// Token: 0x06003FFE RID: 16382 RVA: 0x0019A593 File Offset: 0x00198793
		public float DegreeOfSuccess(Character character)
		{
			return this.DegreeOfSuccess(character, this.RequiredSkills);
		}

		// Token: 0x06003FFF RID: 16383 RVA: 0x0019A5A4 File Offset: 0x001987A4
		public float DegreeOfSuccess(Character character, List<Skill> requiredSkills)
		{
			if (requiredSkills.Count == 0)
			{
				return 1f;
			}
			if (character == null)
			{
				string errorMsg = "ItemComponent.DegreeOfSuccess failed (character was null).\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("ItemComponent.DegreeOfSuccess:CharacterNull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return 0f;
			}
			float skillSuccessSum = 0f;
			for (int i = 0; i < requiredSkills.Count; i++)
			{
				float characterLevel = character.GetSkillLevel(requiredSkills[i].Identifier);
				skillSuccessSum += characterLevel - requiredSkills[i].Level;
			}
			float average = skillSuccessSum / (float)requiredSkills.Count;
			return (average + 100f) / 2f / 100f;
		}

		// Token: 0x06004000 RID: 16384 RVA: 0x0019A64D File Offset: 0x0019884D
		public virtual void FlipX(bool relativeToSub)
		{
		}

		// Token: 0x06004001 RID: 16385 RVA: 0x0019A64F File Offset: 0x0019884F
		public virtual void FlipY(bool relativeToSub)
		{
		}

		// Token: 0x06004002 RID: 16386 RVA: 0x0019A654 File Offset: 0x00198854
		public bool IsEmpty(Character user)
		{
			if (!this.HasRequiredContainedItems(user, false, null))
			{
				return true;
			}
			if (this.Item.OwnInventory != null)
			{
				return !this.Item.OwnInventory.AllItems.Any((Item i) => i.Condition > 0f);
			}
			return false;
		}

		// Token: 0x06004003 RID: 16387 RVA: 0x0019A6B4 File Offset: 0x001988B4
		public bool HasRequiredContainedItems(Character user, bool addMessage, LocalizedString msg = null)
		{
			if (!this.RequiredItems.ContainsKey(RelatedItem.RelationType.Contained))
			{
				return true;
			}
			if (this.item.OwnInventory == null)
			{
				return false;
			}
			foreach (RelatedItem ri in this.RequiredItems[RelatedItem.RelationType.Contained])
			{
				if (!ri.CheckRequirements(user, this.item))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06004004 RID: 16388 RVA: 0x0019A73C File Offset: 0x0019893C
		public virtual bool HasAccess(Character character)
		{
			if (character.IsBot && this.item.IgnoreByAI(character))
			{
				return false;
			}
			if (!this.item.IsInteractable(character))
			{
				return false;
			}
			if (this.RequiredItems.Count == 0)
			{
				return true;
			}
			List<RelatedItem> relatedItems;
			if (character.Inventory != null && this.RequiredItems.TryGetValue(RelatedItem.RelationType.Picked, out relatedItems))
			{
				foreach (RelatedItem relatedItem in relatedItems)
				{
					foreach (Item otherItem in character.Inventory.AllItems)
					{
						if (relatedItem.MatchesItem(otherItem))
						{
							IdCard idCard = otherItem.GetComponent<IdCard>();
							if (idCard == null || this.CheckIdCardAccess(relatedItem, idCard))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06004005 RID: 16389 RVA: 0x0019A840 File Offset: 0x00198A40
		private bool CheckIdCardAccess(RelatedItem relatedItem, IdCard idCard)
		{
			Submarine submarine = this.item.Submarine;
			if (submarine != null && !submarine.IsRespawnShuttle)
			{
				if (idCard.TeamID != CharacterTeamType.None && idCard.TeamID != this.item.Submarine.TeamID)
				{
					if (relatedItem.Identifiers.Any((Identifier id) => id != "idcard"))
					{
						GameSession gameSession = GameMain.GameSession;
						if (!(((gameSession != null) ? gameSession.GameMode : null) is PvPMode))
						{
							return false;
						}
						if (this.item.Submarine.TeamID != CharacterTeamType.FriendlyNPC && this.item.Submarine.TeamID != CharacterTeamType.None)
						{
							return false;
						}
						return true;
					}
				}
				if (idCard.SubmarineSpecificID != 0 && this.item.Submarine.SubmarineSpecificIDTag != idCard.SubmarineSpecificID)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06004006 RID: 16390 RVA: 0x0019A920 File Offset: 0x00198B20
		public virtual bool HasRequiredItems(Character character, bool addMessage, LocalizedString msg = null)
		{
			ItemComponent.<>c__DisplayClass162_0 CS$<>8__locals1 = new ItemComponent.<>c__DisplayClass162_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.msg = msg;
			if (this.RequiredItems.None(null))
			{
				return true;
			}
			if (character.Inventory == null)
			{
				return false;
			}
			CS$<>8__locals1.hasRequiredItems = false;
			bool canContinue = true;
			if (this.RequiredItems.ContainsKey(RelatedItem.RelationType.Equipped))
			{
				foreach (RelatedItem ri in this.RequiredItems[RelatedItem.RelationType.Equipped])
				{
					canContinue = CS$<>8__locals1.<HasRequiredItems>g__CheckItems|0(ri, character.HeldItems);
					if (!canContinue)
					{
						break;
					}
				}
			}
			if (canContinue && this.RequiredItems.ContainsKey(RelatedItem.RelationType.Picked))
			{
				foreach (RelatedItem ri2 in this.RequiredItems[RelatedItem.RelationType.Picked])
				{
					if (!CS$<>8__locals1.<HasRequiredItems>g__CheckItems|0(ri2, character.Inventory.AllItems))
					{
						break;
					}
				}
			}
			return CS$<>8__locals1.hasRequiredItems;
		}

		// Token: 0x06004007 RID: 16391 RVA: 0x0019AA3C File Offset: 0x00198C3C
		public void ApplyStatusEffects(ActionType type, float deltaTime, Character character = null, Limb targetLimb = null, Entity useTarget = null, Character user = null, Vector2? worldPosition = null, float attackMultiplier = 1f)
		{
			if (this.statusEffectLists == null)
			{
				return;
			}
			List<StatusEffect> statusEffects;
			if (!this.statusEffectLists.TryGetValue(type, out statusEffects))
			{
				return;
			}
			bool broken = this.item.Condition <= 0f;
			bool reducesCondition = false;
			foreach (StatusEffect effect in statusEffects)
			{
				if (!broken || effect.AllowWhenBroken || effect.type == ActionType.OnBroken)
				{
					if (user != null)
					{
						effect.SetUser(user);
					}
					effect.AttackMultiplier = attackMultiplier;
					Character c = character;
					if (user != null && effect.HasTargetType(StatusEffect.TargetType.Character) && !effect.HasTargetType(StatusEffect.TargetType.UseTarget))
					{
						c = user;
					}
					this.item.ApplyStatusEffect(effect, type, deltaTime, c, targetLimb, useTarget, false, false, worldPosition);
					effect.AttackMultiplier = 1f;
					reducesCondition |= effect.ReducesItemCondition();
				}
			}
			if (reducesCondition && user != null && type != ActionType.OnBroken)
			{
				foreach (ItemComponent ic in this.item.Components)
				{
					List<StatusEffect> brokenEffects;
					if (ic.statusEffectLists != null && ic.statusEffectLists.TryGetValue(ActionType.OnBroken, out brokenEffects))
					{
						foreach (StatusEffect brokenEffect in brokenEffects)
						{
							brokenEffect.SetUser(user);
						}
					}
				}
			}
		}

		// Token: 0x06004008 RID: 16392 RVA: 0x0019ABEC File Offset: 0x00198DEC
		public virtual void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			ContentXElement contentXElement = null;
			if (componentElement != contentXElement)
			{
				foreach (XAttribute attribute in componentElement.Attributes())
				{
					SerializableProperty property;
					if (this.SerializableProperties.TryGetValue(attribute.NameAsIdentifier(), out property))
					{
						if (!property.OverridePrefabValues && usePrefabValues)
						{
							if (!isItemSwap)
							{
								continue;
							}
							Editable attribute2 = property.GetAttribute<Editable>();
							if (attribute2 == null || !attribute2.TransferToSwappedItem)
							{
								continue;
							}
						}
						property.TrySetValue(this, attribute.Value);
					}
				}
				this.ParseMsg();
				this.OverrideRequiredItems(componentElement);
			}
			if (this.item.Submarine != null)
			{
				SerializableProperty.UpgradeGameVersion(this, this.originalElement, this.item.Submarine.Info.GameVersion);
			}
		}

		// Token: 0x06004009 RID: 16393 RVA: 0x0019ACC4 File Offset: 0x00198EC4
		public virtual void OnMapLoaded()
		{
		}

		// Token: 0x0600400A RID: 16394 RVA: 0x0019ACC6 File Offset: 0x00198EC6
		public virtual void OnItemLoaded()
		{
		}

		// Token: 0x0600400B RID: 16395 RVA: 0x0019ACC8 File Offset: 0x00198EC8
		public virtual void Clone(ItemComponent original)
		{
		}

		// Token: 0x0600400C RID: 16396 RVA: 0x0019ACCA File Offset: 0x00198ECA
		public virtual void OnScaleChanged()
		{
		}

		// Token: 0x0600400D RID: 16397 RVA: 0x0019ACCC File Offset: 0x00198ECC
		public virtual void OnInventoryChanged()
		{
		}

		// Token: 0x0600400E RID: 16398 RVA: 0x0019ACD0 File Offset: 0x00198ED0
		public static ItemComponent Load(ContentXElement element, Item item, bool errorMessages = true)
		{
			Identifier typeName = element.NameAsIdentifier();
			Type type;
			try
			{
				type = ReflectionUtils.GetDerivedNonAbstract<ItemComponent>().Append(typeof(ItemComponent)).FirstOrDefault((Type t) => t.Name == typeName);
				if (type == null)
				{
					if (errorMessages)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Could not find the component \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(typeName);
						defaultInterpolatedStringHandler.AppendLiteral("\" (");
						defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(item.Prefab.ContentFile.Path);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
					}
					return null;
				}
			}
			catch (Exception e)
			{
				if (errorMessages)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(34, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Could not find the component \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(typeName);
					defaultInterpolatedStringHandler2.AppendLiteral("\" (");
					defaultInterpolatedStringHandler2.AppendFormatted<ContentPath>(item.Prefab.ContentFile.Path);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), e, element.ContentPackage, false, false);
				}
				return null;
			}
			ConstructorInfo constructor;
			try
			{
				if (type != typeof(ItemComponent) && !type.IsSubclassOf(typeof(ItemComponent)))
				{
					return null;
				}
				constructor = type.GetConstructor(new Type[]
				{
					typeof(Item),
					typeof(ContentXElement)
				});
				if (constructor == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(53, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Could not find the constructor of the component \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(typeName);
					defaultInterpolatedStringHandler3.AppendLiteral("\" (");
					defaultInterpolatedStringHandler3.AppendFormatted<ContentPath>(item.Prefab.ContentFile.Path);
					defaultInterpolatedStringHandler3.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, element.ContentPackage, false, false);
					return null;
				}
			}
			catch (Exception e2)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(53, 2);
				defaultInterpolatedStringHandler4.AppendLiteral("Could not find the constructor of the component \"");
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(typeName);
				defaultInterpolatedStringHandler4.AppendLiteral("\" (");
				defaultInterpolatedStringHandler4.AppendFormatted<ContentPath>(item.Prefab.ContentFile.Path);
				defaultInterpolatedStringHandler4.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), e2, element.ContentPackage, false, false);
				return null;
			}
			ItemComponent ic = null;
			try
			{
				object[] lobject = new object[]
				{
					item,
					element
				};
				object component = constructor.Invoke(lobject);
				ic = (ItemComponent)component;
				ic.name = element.Name.ToString();
			}
			catch (TargetInvocationException e3)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(43, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("Error while loading component of the type ");
				defaultInterpolatedStringHandler5.AppendFormatted<Type>(type);
				defaultInterpolatedStringHandler5.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), e3.InnerException, element.ContentPackage, false, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(44, 2);
				defaultInterpolatedStringHandler6.AppendLiteral("ItemComponent.Load:TargetInvocationException");
				defaultInterpolatedStringHandler6.AppendFormatted(item.Name);
				defaultInterpolatedStringHandler6.AppendFormatted<XName>(element.Name);
				string identifier = defaultInterpolatedStringHandler6.ToStringAndClear();
				GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(43, 3);
				defaultInterpolatedStringHandler7.AppendLiteral("Error while loading entity of the type ");
				defaultInterpolatedStringHandler7.AppendFormatted<Type>(type);
				defaultInterpolatedStringHandler7.AppendLiteral(" (");
				defaultInterpolatedStringHandler7.AppendFormatted<Exception>(e3.InnerException);
				defaultInterpolatedStringHandler7.AppendLiteral(")\n");
				defaultInterpolatedStringHandler7.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, defaultInterpolatedStringHandler7.ToStringAndClear());
			}
			return ic;
		}

		// Token: 0x0600400F RID: 16399 RVA: 0x0019B090 File Offset: 0x00199290
		public virtual XElement Save(XElement parentElement)
		{
			XElement componentElement = new XElement(this.name);
			foreach (KeyValuePair<RelatedItem.RelationType, List<RelatedItem>> kvp in this.RequiredItems)
			{
				foreach (RelatedItem ri in kvp.Value)
				{
					XElement newElement = new XElement("requireditem");
					ri.Save(newElement);
					componentElement.Add(newElement);
				}
			}
			foreach (RelatedItem ri2 in this.DisabledRequiredItems)
			{
				XElement newElement2 = new XElement("requireditem");
				if (!ri2.Identifiers.IsEmpty || !this.RequiredItems.Any<KeyValuePair<RelatedItem.RelationType, List<RelatedItem>>>())
				{
					ri2.Save(newElement2);
					componentElement.Add(newElement2);
				}
			}
			SerializableProperty.SerializeProperties(this, componentElement, false, false);
			parentElement.Add(componentElement);
			return componentElement;
		}

		// Token: 0x06004010 RID: 16400 RVA: 0x0019B1D8 File Offset: 0x001993D8
		public virtual void Reset()
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, this.originalElement);
			if (this is Pickable)
			{
				this.canBePicked = true;
			}
			this.ParseMsg();
			this.OverrideRequiredItems(this.originalElement);
		}

		// Token: 0x06004011 RID: 16401 RVA: 0x0019B214 File Offset: 0x00199414
		private void OverrideRequiredItems(ContentXElement element)
		{
			Dictionary<RelatedItem.RelationType, List<RelatedItem>> prevRequiredItems = new Dictionary<RelatedItem.RelationType, List<RelatedItem>>(this.RequiredItems);
			this.RequiredItems.Clear();
			bool returnEmptyRequirements = false;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "requireditem" || a == "requireditems")
				{
					RelatedItem newRequiredItem = RelatedItem.Load(subElement, returnEmptyRequirements, this.item.Name);
					if (newRequiredItem != null)
					{
						RelatedItem prevRequiredItem = prevRequiredItems.ContainsKey(newRequiredItem.Type) ? prevRequiredItems[newRequiredItem.Type].Find((RelatedItem ri) => ri.JoinedIdentifiers == newRequiredItem.JoinedIdentifiers) : null;
						if (prevRequiredItem != null)
						{
							newRequiredItem.StatusEffects = prevRequiredItem.StatusEffects;
							newRequiredItem.Msg = prevRequiredItem.Msg;
							newRequiredItem.IsOptional = prevRequiredItem.IsOptional;
							newRequiredItem.IgnoreInEditor = prevRequiredItem.IgnoreInEditor;
						}
						if (!this.RequiredItems.ContainsKey(newRequiredItem.Type))
						{
							this.RequiredItems[newRequiredItem.Type] = new List<RelatedItem>();
						}
						this.RequiredItems[newRequiredItem.Type].Add(newRequiredItem);
					}
				}
			}
		}

		// Token: 0x06004012 RID: 16402 RVA: 0x0019B3C8 File Offset: 0x001995C8
		public virtual void ParseMsg()
		{
			LocalizedString msg = TextManager.Get(this.Msg);
			if (msg.Loaded)
			{
				msg = TextManager.ParseInputTypes(msg, false);
				this.DisplayMsg = msg;
				return;
			}
			this.DisplayMsg = this.Msg;
		}

		// Token: 0x06004013 RID: 16403 RVA: 0x0019B40A File Offset: 0x0019960A
		public virtual bool ValidateEventData(NetEntityEvent.IData data)
		{
			return true;
		}

		// Token: 0x06004014 RID: 16404 RVA: 0x0019B410 File Offset: 0x00199610
		protected T ExtractEventData<T>(NetEntityEvent.IData data) where T : ItemComponent.IEventData
		{
			T componentData;
			if (!this.TryExtractEventData<T>(data, out componentData))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(111, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Malformed item component state event for ");
				defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendLiteral("(item ID ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.item.ID);
				defaultInterpolatedStringHandler.AppendLiteral(", component type ");
				defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
				defaultInterpolatedStringHandler.AppendLiteral("): ");
				defaultInterpolatedStringHandler.AppendLiteral("could not extract ComponentData of type ");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(T).Name);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return componentData;
		}

		// Token: 0x06004015 RID: 16405 RVA: 0x0019B4D8 File Offset: 0x001996D8
		protected bool TryExtractEventData<T>(NetEntityEvent.IData data, out T componentData)
		{
			componentData = default(T);
			if (data is Item.ComponentStateEventData)
			{
				ItemComponent.IEventData componentData2 = ((Item.ComponentStateEventData)data).ComponentData;
				if (componentData2 is T)
				{
					T nestedData = (T)((object)componentData2);
					componentData = nestedData;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06004016 RID: 16406 RVA: 0x0019B51C File Offset: 0x0019971C
		protected AIObjectiveContainItem AIContainItems<T>(ItemContainer container, Character character, AIObjective currentObjective, int itemCount, bool equip, bool removeEmpty, bool spawnItemIfNotFound = false, bool dropItemOnDeselected = false) where T : ItemComponent
		{
			AIObjectiveContainItem containObjective = null;
			AIController aicontroller = character.AIController;
			HumanAIController aiController = aicontroller as HumanAIController;
			if (aiController != null)
			{
				containObjective = new AIObjectiveContainItem(character, container.ContainableItemIdentifiers, container, currentObjective.objectiveManager, 1f, spawnItemIfNotFound)
				{
					ItemCount = itemCount,
					Equip = equip,
					RemoveEmpty = removeEmpty,
					GetItemPriority = delegate(Item i)
					{
						Inventory parentInventory = i.ParentInventory;
						if (((parentInventory != null) ? parentInventory.Owner : null) is Item && ((Item)i.ParentInventory.Owner).GetComponent<T>() != null)
						{
							return 0f;
						}
						if (!container.ContainsItemsWithSameIdentifier(i))
						{
							return 0.5f;
						}
						return 1f;
					}
				};
				containObjective.Abandoned += delegate()
				{
					aiController.IgnoredItems.Add(container.Item);
				};
				if (dropItemOnDeselected)
				{
					currentObjective.Deselected += delegate()
					{
						if (containObjective == null)
						{
							return;
						}
						if (containObjective.IsCompleted)
						{
							return;
						}
						Item item = containObjective.ItemToContain;
						if (item != null && character.CanInteractWith(item, false))
						{
							item.Drop(character, true, true);
						}
					};
				}
				currentObjective.AddSubObjective(containObjective, false);
			}
			return containObjective;
		}

		// Token: 0x06004017 RID: 16407 RVA: 0x0019B600 File Offset: 0x00199800
		[CompilerGenerated]
		private void <.ctor>g__LoadStatusEffect|124_0(ContentXElement subElement, ref ItemComponent.<>c__DisplayClass124_0 A_2)
		{
			StatusEffect statusEffect = StatusEffect.Load(subElement, A_2.item.Name + ", " + base.GetType().Name);
			List<StatusEffect> effectList;
			if (!this.statusEffectLists.TryGetValue(statusEffect.type, out effectList))
			{
				effectList = new List<StatusEffect>();
				this.statusEffectLists.Add(statusEffect.type, effectList);
			}
			effectList.Add(statusEffect);
		}

		// Token: 0x06004018 RID: 16408 RVA: 0x0019B668 File Offset: 0x00199868
		[CompilerGenerated]
		internal static void <Combine>g__RemoveItem|145_0(Item item)
		{
			Screen selected = Screen.Selected;
			if (selected != null && selected.IsEditor)
			{
				if (item != null)
				{
					item.Remove();
					return;
				}
			}
			else
			{
				EntitySpawner spawner = Entity.Spawner;
				if (spawner == null)
				{
					return;
				}
				spawner.AddItemToRemoveQueue(item);
			}
		}

		// Token: 0x04001E73 RID: 7795
		protected Item item;

		// Token: 0x04001E74 RID: 7796
		protected string name;

		// Token: 0x04001E75 RID: 7797
		private bool isActive;

		// Token: 0x04001E76 RID: 7798
		protected bool characterUsable;

		// Token: 0x04001E77 RID: 7799
		protected bool canBePicked;

		// Token: 0x04001E78 RID: 7800
		protected bool canBeSelected;

		// Token: 0x04001E79 RID: 7801
		protected bool canBeCombined;

		// Token: 0x04001E7A RID: 7802
		protected bool removeOnCombined;

		// Token: 0x04001E7B RID: 7803
		public bool WasUsed;

		// Token: 0x04001E7C RID: 7804
		public bool WasSecondaryUsed;

		// Token: 0x04001E7D RID: 7805
		public readonly Dictionary<ActionType, List<StatusEffect>> statusEffectLists;

		// Token: 0x04001E7E RID: 7806
		public Dictionary<RelatedItem.RelationType, List<RelatedItem>> RequiredItems;

		// Token: 0x04001E7F RID: 7807
		public readonly List<RelatedItem> DisabledRequiredItems = new List<RelatedItem>();

		// Token: 0x04001E80 RID: 7808
		public readonly List<Skill> RequiredSkills = new List<Skill>();

		// Token: 0x04001E81 RID: 7809
		private ItemComponent parent;

		// Token: 0x04001E83 RID: 7811
		public readonly ContentXElement originalElement;

		// Token: 0x04001E84 RID: 7812
		protected const float CorrectionDelay = 1f;

		// Token: 0x04001E85 RID: 7813
		protected CoroutineHandle delayedCorrectionCoroutine;

		// Token: 0x04001E89 RID: 7817
		public Action<bool> OnActiveStateChanged;

		// Token: 0x04001E8A RID: 7818
		private bool drawable = true;

		// Token: 0x04001E8C RID: 7820
		public List<PropertyConditional> IsActiveConditionals;

		// Token: 0x04001E98 RID: 7832
		public readonly NamedEvent<ItemComponent.ItemUseInfo> OnUsed = new NamedEvent<ItemComponent.ItemUseInfo>();

		// Token: 0x04001E99 RID: 7833
		public readonly bool InheritStatusEffects;

		// Token: 0x04001E9B RID: 7835
		protected const float AIUpdateInterval = 0.2f;

		// Token: 0x04001E9C RID: 7836
		protected float aiUpdateTimer;

		// Token: 0x02000D86 RID: 3462
		public readonly struct ItemUseInfo : IEquatable<ItemComponent.ItemUseInfo>
		{
			// Token: 0x06006762 RID: 26466 RVA: 0x0021FFD0 File Offset: 0x0021E1D0
			public ItemUseInfo(Item Item, Character User)
			{
				this.Item = Item;
				this.User = User;
			}

			// Token: 0x1700165D RID: 5725
			// (get) Token: 0x06006763 RID: 26467 RVA: 0x0021FFE0 File Offset: 0x0021E1E0
			// (set) Token: 0x06006764 RID: 26468 RVA: 0x0021FFE8 File Offset: 0x0021E1E8
			public Item Item { get; set; }

			// Token: 0x1700165E RID: 5726
			// (get) Token: 0x06006765 RID: 26469 RVA: 0x0021FFF1 File Offset: 0x0021E1F1
			// (set) Token: 0x06006766 RID: 26470 RVA: 0x0021FFF9 File Offset: 0x0021E1F9
			public Character User { get; set; }

			// Token: 0x06006767 RID: 26471 RVA: 0x00220004 File Offset: 0x0021E204
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ItemUseInfo");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006768 RID: 26472 RVA: 0x00220050 File Offset: 0x0021E250
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Item = ");
				builder.Append(this.Item);
				builder.Append(", User = ");
				builder.Append(this.User);
				return true;
			}

			// Token: 0x06006769 RID: 26473 RVA: 0x00220085 File Offset: 0x0021E285
			[CompilerGenerated]
			public static bool operator !=(ItemComponent.ItemUseInfo left, ItemComponent.ItemUseInfo right)
			{
				return !(left == right);
			}

			// Token: 0x0600676A RID: 26474 RVA: 0x00220091 File Offset: 0x0021E291
			[CompilerGenerated]
			public static bool operator ==(ItemComponent.ItemUseInfo left, ItemComponent.ItemUseInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600676B RID: 26475 RVA: 0x0022009B File Offset: 0x0021E29B
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<Item>.Default.GetHashCode(this.<Item>k__BackingField) * -1521134295 + EqualityComparer<Character>.Default.GetHashCode(this.<User>k__BackingField);
			}

			// Token: 0x0600676C RID: 26476 RVA: 0x002200C4 File Offset: 0x0021E2C4
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ItemComponent.ItemUseInfo && this.Equals((ItemComponent.ItemUseInfo)obj);
			}

			// Token: 0x0600676D RID: 26477 RVA: 0x002200DC File Offset: 0x0021E2DC
			[CompilerGenerated]
			public bool Equals(ItemComponent.ItemUseInfo other)
			{
				return EqualityComparer<Item>.Default.Equals(this.<Item>k__BackingField, other.<Item>k__BackingField) && EqualityComparer<Character>.Default.Equals(this.<User>k__BackingField, other.<User>k__BackingField);
			}

			// Token: 0x0600676E RID: 26478 RVA: 0x0022010E File Offset: 0x0021E30E
			[CompilerGenerated]
			public void Deconstruct(out Item Item, out Character User)
			{
				Item = this.Item;
				User = this.User;
			}
		}

		// Token: 0x02000D87 RID: 3463
		public interface IEventData
		{
		}
	}
}
