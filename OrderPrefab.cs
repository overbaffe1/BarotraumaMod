using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000190 RID: 400
	internal class OrderPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000BFA RID: 3066
		// (get) Token: 0x06002EDD RID: 11997 RVA: 0x001F2E8A File Offset: 0x001F108A
		public static OrderPrefab Dismissal
		{
			get
			{
				return OrderPrefab.Prefabs[OrderPrefab.DismissalIdentifier];
			}
		}

		// Token: 0x17000BFB RID: 3067
		// (get) Token: 0x06002EDE RID: 11998 RVA: 0x001F2E9B File Offset: 0x001F109B
		public bool HasOptionSpecificTargetItems
		{
			get
			{
				return this.OptionTargetItems != null && this.OptionTargetItems.Any<KeyValuePair<Identifier, ImmutableArray<Identifier>>>();
			}
		}

		// Token: 0x17000BFC RID: 3068
		// (get) Token: 0x06002EDF RID: 11999 RVA: 0x001F2EB4 File Offset: 0x001F10B4
		public Color Color
		{
			get
			{
				if (this.color != null)
				{
					return this.color.Value;
				}
				if (OrderCategoryIcon.OrderCategoryIcons.ContainsKey(this.CategoryIdentifier))
				{
					return OrderCategoryIcon.OrderCategoryIcons[this.Category.ToIdentifier<OrderCategory?>()].Color;
				}
				return Color.White;
			}
		}

		// Token: 0x17000BFD RID: 3069
		// (get) Token: 0x06002EE0 RID: 12000 RVA: 0x001F2F0C File Offset: 0x001F110C
		public bool IsReport
		{
			get
			{
				return this.TargetAllCharacters && !this.MustSetTarget;
			}
		}

		// Token: 0x17000BFE RID: 3070
		// (get) Token: 0x06002EE1 RID: 12001 RVA: 0x001F2F24 File Offset: 0x001F1124
		public bool IsVisibleAsReportButton
		{
			get
			{
				if (!this.IsReport || this.Hidden || this.SymbolSprite == null)
				{
					return false;
				}
				if (this.TraitorModeOnly)
				{
					GameSession gameSession = GameMain.GameSession;
					return gameSession != null && gameSession.TraitorsEnabled;
				}
				return true;
			}
		}

		// Token: 0x17000BFF RID: 3071
		// (get) Token: 0x06002EE2 RID: 12002 RVA: 0x001F2F66 File Offset: 0x001F1166
		public bool IsDismissal
		{
			get
			{
				return this.Identifier == OrderPrefab.DismissalIdentifier;
			}
		}

		// Token: 0x17000C00 RID: 3072
		// (get) Token: 0x06002EE3 RID: 12003 RVA: 0x001F2F78 File Offset: 0x001F1178
		public bool HasOptions
		{
			get
			{
				return this.Options.Length > 1;
			}
		}

		// Token: 0x17000C01 RID: 3073
		// (get) Token: 0x06002EE4 RID: 12004 RVA: 0x001F2F88 File Offset: 0x001F1188
		public OrderPrefab.OrderTargetType TargetType { get; }

		// Token: 0x17000C02 RID: 3074
		// (get) Token: 0x06002EE5 RID: 12005 RVA: 0x001F2F90 File Offset: 0x001F1190
		public int? WallSectionIndex { get; }

		// Token: 0x17000C03 RID: 3075
		// (get) Token: 0x06002EE6 RID: 12006 RVA: 0x001F2F98 File Offset: 0x001F1198
		public bool IsIgnoreOrder
		{
			get
			{
				return this.Identifier == Tags.IgnoreThis || this.Identifier == Tags.UnignoreThis;
			}
		}

		// Token: 0x17000C04 RID: 3076
		// (get) Token: 0x06002EE7 RID: 12007 RVA: 0x001F2FBE File Offset: 0x001F11BE
		public bool IsDeconstructOrder
		{
			get
			{
				return this.Identifier == Tags.DeconstructThis || this.Identifier == Tags.DontDeconstructThis;
			}
		}

		// Token: 0x17000C05 RID: 3077
		// (get) Token: 0x06002EE8 RID: 12008 RVA: 0x001F2FE4 File Offset: 0x001F11E4
		public bool DrawIconWhenContained { get; }

		// Token: 0x17000C06 RID: 3078
		// (get) Token: 0x06002EE9 RID: 12009 RVA: 0x001F2FEC File Offset: 0x001F11EC
		public int AssignmentPriority { get; }

		// Token: 0x17000C07 RID: 3079
		// (get) Token: 0x06002EEA RID: 12010 RVA: 0x001F2FF4 File Offset: 0x001F11F4
		public bool ColoredWhenControllingGiver { get; }

		// Token: 0x17000C08 RID: 3080
		// (get) Token: 0x06002EEB RID: 12011 RVA: 0x001F2FFC File Offset: 0x001F11FC
		public bool DisplayGiverInTooltip { get; }

		// Token: 0x06002EEC RID: 12012 RVA: 0x001F3004 File Offset: 0x001F1204
		public OrderPrefab(ContentXElement orderElement, OrdersFile file) : base(file, orderElement.GetAttributeIdentifier("identifier", ""))
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
			defaultInterpolatedStringHandler.AppendLiteral("OrderName.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			this.Name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(20, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("OrderNameContextual.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
			this.ContextualName = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()).Fallback(this.Name, true);
			string targetItemType = orderElement.GetAttributeString("targetitemtype", "");
			if (!string.IsNullOrWhiteSpace(targetItemType))
			{
				try
				{
					this.ItemComponentType = Type.GetType("Barotrauma.Items.Components." + targetItemType, true, true);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Error in the order definitions: item component type " + targetItemType + " not found", e, null, false, false);
				}
			}
			this.CanTypeBeSubclass = orderElement.GetAttributeBool("cantypebesubclass", false);
			this.color = orderElement.GetAttributeColor("color");
			this.FadeOutTime = orderElement.GetAttributeFloat("fadeouttime", 0f);
			this.UseController = orderElement.GetAttributeBool("usecontroller", false);
			this.ControllerTags = orderElement.GetAttributeIdentifierArray("controllertags", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.TargetAllCharacters = orderElement.GetAttributeBool("targetallcharacters", false);
			this.AppropriateJobs = orderElement.GetAttributeIdentifierArray("appropriatejobs", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.TraitorModeOnly = orderElement.GetAttributeBool("TraitorModeOnly", false);
			this.PreferredJobs = orderElement.GetAttributeIdentifierArray("preferredjobs", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.Options = orderElement.GetAttributeIdentifierArray("options", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.HiddenOptions = orderElement.GetAttributeIdentifierArray("hiddenoptions", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.AllOptions = this.Options.Concat(this.HiddenOptions).ToImmutableArray<Identifier>();
			Dictionary<Identifier, ImmutableArray<Identifier>> optionTargetItems = new Dictionary<Identifier, ImmutableArray<Identifier>>();
			string targetItems = orderElement.GetAttributeString("targetitems", "");
			if (targetItems != null && targetItems.Contains(';'))
			{
				string[] splitTargetItems = targetItems.Split(';', StringSplitOptions.None);
				List<Identifier> allTargetItems = new List<Identifier>();
				for (int k = 0; k < this.AllOptions.Length; k++)
				{
					Identifier[] optionTargetItemsSplit = (k < splitTargetItems.Length) ? splitTargetItems[k].ToIdentifiers(",").ToArray<Identifier>() : Array.Empty<Identifier>();
					allTargetItems.AddRange(optionTargetItemsSplit);
					optionTargetItems.Add(this.AllOptions[k], optionTargetItemsSplit.ToImmutableArray<Identifier>());
				}
				this.TargetItems = allTargetItems.ToImmutableArray<Identifier>();
			}
			else
			{
				this.TargetItems = orderElement.GetAttributeIdentifierArray("targetitems", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			}
			this.RequireItems = orderElement.GetAttributeIdentifierArray("requireitems", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.OptionTargetItems = optionTargetItems.ToImmutableDictionary<Identifier, ImmutableArray<Identifier>>();
			string category = orderElement.GetAttributeString("category", null);
			this.Category = ((!string.IsNullOrWhiteSpace(category)) ? new OrderCategory?(Enum.Parse<OrderCategory>(category, true)) : null);
			this.CategoryIdentifier = (((this.Category != null) ? this.Category.GetValueOrDefault().ToString() : null) ?? string.Empty).ToIdentifier();
			this.MustSetTarget = orderElement.GetAttributeBool("mustsettarget", false);
			this.CanBeGeneralized = (!this.MustSetTarget && orderElement.GetAttributeBool("canbegeneralized", true));
			this.AppropriateSkill = orderElement.GetAttributeIdentifier("appropriateskill", Identifier.Empty);
			this.Hidden = orderElement.GetAttributeBool("hidden", false);
			this.IgnoreAtOutpost = orderElement.GetAttributeBool("ignoreatoutpost", false);
			this.OptionNames = new ListDictionary<Identifier, LocalizedString>(TextManager.Get("OrderOptions." + this.Identifier.ToString()).Split(new char[]
			{
				',',
				'，'
			}), this.Options.Length, (int i) => this.Options[i]);
			ContentXElement spriteElement = orderElement.GetChildElement("sprite");
			ContentXElement contentXElement = null;
			if (spriteElement != contentXElement)
			{
				this.SymbolSprite = new Sprite(spriteElement, "", "", true, 1f);
			}
			Dictionary<Identifier, Sprite> optionSprites = new Dictionary<Identifier, Sprite>();
			if (new ImmutableArray<Identifier>?(this.Options) != null && this.Options.Length > 0)
			{
				ContentXElement childElement = orderElement.GetChildElement("optionsprites");
				IEnumerable<ContentXElement> optionSpriteElements = (childElement != null) ? childElement.GetChildElements("sprite") : null;
				if (optionSpriteElements != null && optionSpriteElements.Any<ContentXElement>())
				{
					int j = 0;
					while (j < this.Options.Length && j < optionSpriteElements.Count<ContentXElement>())
					{
						Sprite sprite = new Sprite(optionSpriteElements.ElementAt(j), "", "", true, 1f);
						optionSprites.Add(this.Options[j], sprite);
						j++;
					}
				}
			}
			this.OptionSprites = optionSprites.ToImmutableDictionary<Identifier, Sprite>();
			this.MustManuallyAssign = orderElement.GetAttributeBool("mustmanuallyassign", false);
			this.DrawIconWhenContained = orderElement.GetAttributeBool("displayiconwhencontained", false);
			this.AutoDismiss = orderElement.GetAttributeBool("autodismiss", this.Category.GetValueOrDefault() == OrderCategory.Operate || this.Category.GetValueOrDefault() == OrderCategory.Movement);
			this.AssignmentPriority = Math.Clamp(orderElement.GetAttributeInt("assignmentpriority", 100), 0, 100);
			this.ColoredWhenControllingGiver = orderElement.GetAttributeBool("coloredwhencontrollinggiver", false);
			this.DisplayGiverInTooltip = orderElement.GetAttributeBool("displaygiverintooltip", false);
		}

		// Token: 0x06002EED RID: 12013 RVA: 0x001F3600 File Offset: 0x001F1800
		private bool HasSpecifiedJob(Character character, IReadOnlyList<Identifier> jobs)
		{
			if (jobs == null || jobs.Count == 0)
			{
				return false;
			}
			Identifier? identifier;
			if (character == null)
			{
				identifier = null;
			}
			else
			{
				CharacterInfo info = character.Info;
				if (info == null)
				{
					identifier = null;
				}
				else
				{
					Job job = info.Job;
					if (job == null)
					{
						identifier = null;
					}
					else
					{
						JobPrefab prefab = job.Prefab;
						identifier = ((prefab != null) ? new Identifier?(prefab.Identifier) : null);
					}
				}
			}
			Identifier jobIdentifier = identifier ?? Identifier.Empty;
			if (jobIdentifier.IsEmpty)
			{
				return false;
			}
			for (int i = 0; i < jobs.Count; i++)
			{
				Identifier identifier2 = jobs[i];
				if (jobIdentifier == identifier2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002EEE RID: 12014 RVA: 0x001F36BB File Offset: 0x001F18BB
		public bool HasAppropriateJob(Character character)
		{
			return this.HasSpecifiedJob(character, this.AppropriateJobs);
		}

		// Token: 0x06002EEF RID: 12015 RVA: 0x001F36CF File Offset: 0x001F18CF
		public bool HasPreferredJob(Character character)
		{
			return this.HasSpecifiedJob(character, this.PreferredJobs);
		}

		// Token: 0x06002EF0 RID: 12016 RVA: 0x001F36E4 File Offset: 0x001F18E4
		public string GetChatMessage(string targetCharacterName, string targetRoomName, Entity targetEntity, bool givingOrderToSelf, Identifier orderOption = default(Identifier), bool isNewOrder = true)
		{
			if (this.TargetAllCharacters || isNewOrder || !(this.Identifier != "dismissed"))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted((givingOrderToSelf && !this.TargetAllCharacters) ? "OrderDialogSelf" : "OrderDialog");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				string messageTag = defaultInterpolatedStringHandler.ToStringAndClear();
				if (!orderOption.IsEmpty)
				{
					if (this.Identifier != "dismissed")
					{
						string str = messageTag;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 1);
						defaultInterpolatedStringHandler2.AppendLiteral(".");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(orderOption);
						messageTag = str + defaultInterpolatedStringHandler2.ToStringAndClear();
					}
					else
					{
						string[] splitOption = orderOption.Value.Split('.', StringSplitOptions.None);
						if (splitOption.Length != 0)
						{
							messageTag = messageTag + "." + splitOption[0];
						}
					}
				}
				LocalizedString targetEntityName = string.Empty;
				Item item = targetEntity as Item;
				if (item == null)
				{
					Hull hull = targetEntity as Hull;
					if (hull == null)
					{
						Structure structure = targetEntity as Structure;
						if (structure == null)
						{
							Character character = targetEntity as Character;
							if (character != null)
							{
								targetEntityName = character.DisplayName;
							}
						}
						else
						{
							targetEntityName = structure.Name;
						}
					}
					else
					{
						targetEntityName = hull.DisplayName;
					}
				}
				else
				{
					targetEntityName = item.Name;
				}
				return TextManager.GetWithVariables(messageTag, new ValueTuple<string, LocalizedString, FormatCapitals>[]
				{
					new ValueTuple<string, LocalizedString, FormatCapitals>("[name]", targetCharacterName ?? string.Empty, FormatCapitals.No),
					new ValueTuple<string, LocalizedString, FormatCapitals>("[target]", targetEntityName, FormatCapitals.No),
					new ValueTuple<string, LocalizedString, FormatCapitals>("[roomname]", targetRoomName ?? string.Empty, FormatCapitals.Yes)
				}).Fallback("", true).Value;
			}
			if (!givingOrderToSelf)
			{
				return TextManager.GetWithVariable("rearrangedorders", "[name]", targetCharacterName ?? string.Empty, FormatCapitals.No).Value;
			}
			return string.Empty;
		}

		// Token: 0x06002EF1 RID: 12017 RVA: 0x001F38E0 File Offset: 0x001F1AE0
		public ItemComponent GetTargetItemComponent(Item item)
		{
			if (((item != null) ? item.Components : null) == null || this.ItemComponentType == null)
			{
				return null;
			}
			foreach (ItemComponent component in item.Components)
			{
				Type componentType = (component != null) ? component.GetType() : null;
				if (componentType != null && (this.UseController || component.CanBeSelected))
				{
					if (componentType == this.ItemComponentType)
					{
						return component;
					}
					if (this.CanTypeBeSubclass && componentType.IsSubclassOf(this.ItemComponentType))
					{
						return component;
					}
				}
			}
			return null;
		}

		// Token: 0x06002EF2 RID: 12018 RVA: 0x001F399C File Offset: 0x001F1B9C
		public bool TryGetTargetItemComponent(Item item, out ItemComponent firstMatchingComponent)
		{
			firstMatchingComponent = this.GetTargetItemComponent(item);
			return firstMatchingComponent != null;
		}

		// Token: 0x06002EF3 RID: 12019 RVA: 0x001F39AC File Offset: 0x001F1BAC
		public List<Item> GetMatchingItems(Submarine submarine, bool mustBelongToPlayerSub, CharacterTeamType? requiredTeam = null, Character interactableFor = null, Identifier orderOption = default(Identifier))
		{
			List<Item> matchingItems = new List<Item>();
			if (submarine == null)
			{
				return matchingItems;
			}
			if (this.ItemComponentType != null || this.TargetItems.Any<Identifier>() || this.RequireItems.Any<Identifier>())
			{
				foreach (Item item in Item.ItemList)
				{
					ItemComponent itemComponent;
					if ((!this.RequireItems.Any<Identifier>() || OrderPrefab.TargetItemsMatchItem(this.RequireItems, item)) && (!this.TargetItems.Any<Identifier>() || this.TargetItemsMatchItem(item, orderOption)) && (!this.RequireItems.None(null) || !this.TargetItems.None(null) || this.TryGetTargetItemComponent(item, out itemComponent)))
					{
						if (mustBelongToPlayerSub)
						{
							Submarine submarine2 = item.Submarine;
							if (((submarine2 != null) ? submarine2.Info : null) != null && item.Submarine.Info.Type != SubmarineType.Player)
							{
								continue;
							}
						}
						if ((item.Submarine == submarine || submarine.DockedTo.Contains(item.Submarine)) && (requiredTeam == null || (item.Submarine != null && item.Submarine.TeamID == requiredTeam.Value)) && !item.NonInteractable && (!(this.ItemComponentType != null) || !item.Components.None((ItemComponent c) => c.GetType() == this.ItemComponentType)))
						{
							Controller controller = null;
							if ((!this.UseController || item.TryFindController(out controller, new ImmutableArray<Identifier>?(this.ControllerTags))) && (interactableFor == null || (item.IsInteractable(interactableFor) && (!this.UseController || controller.Item.IsInteractable(interactableFor)))))
							{
								matchingItems.Add(item);
							}
						}
					}
				}
			}
			return matchingItems;
		}

		// Token: 0x06002EF4 RID: 12020 RVA: 0x001F3B9C File Offset: 0x001F1D9C
		public List<Item> GetMatchingItems(bool mustBelongToPlayerSub, Character interactableFor = null, Identifier orderOption = default(Identifier))
		{
			Submarine submarine = (Character.Controlled != null && Character.Controlled.TeamID == CharacterTeamType.Team2 && Submarine.MainSubs.Length > 1) ? Submarine.MainSubs[1] : Submarine.MainSub;
			return this.GetMatchingItems(submarine, mustBelongToPlayerSub, null, interactableFor, orderOption);
		}

		// Token: 0x06002EF5 RID: 12021 RVA: 0x001F3BED File Offset: 0x001F1DED
		public LocalizedString GetOptionName(string id)
		{
			return this.GetOptionName(id.ToIdentifier());
		}

		// Token: 0x06002EF6 RID: 12022 RVA: 0x001F3BFB File Offset: 0x001F1DFB
		public LocalizedString GetOptionName(Identifier id)
		{
			if (this.OptionNames.ContainsKey(id))
			{
				return this.OptionNames[id];
			}
			return string.Empty;
		}

		// Token: 0x06002EF7 RID: 12023 RVA: 0x001F3C22 File Offset: 0x001F1E22
		public LocalizedString GetOptionName(int index)
		{
			if (index < 0 || index >= this.Options.Length)
			{
				return null;
			}
			return this.OptionNames[this.Options[index]];
		}

		// Token: 0x06002EF8 RID: 12024 RVA: 0x001F3C50 File Offset: 0x001F1E50
		public static Identifier GetDismissOrderOption(Order order)
		{
			Identifier option = order.Identifier;
			if (order.Option != Identifier.Empty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(option);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(order.Option);
				option = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			}
			return option;
		}

		// Token: 0x06002EF9 RID: 12025 RVA: 0x001F3CB0 File Offset: 0x001F1EB0
		public ImmutableArray<Identifier> GetTargetItems(Identifier option = default(Identifier))
		{
			ImmutableArray<Identifier> optionTargetItems;
			if (option.IsEmpty || !this.OptionTargetItems.TryGetValue(option, out optionTargetItems))
			{
				return this.TargetItems;
			}
			return optionTargetItems;
		}

		// Token: 0x06002EFA RID: 12026 RVA: 0x001F3CE0 File Offset: 0x001F1EE0
		public bool TargetItemsMatchItem(Item item, Identifier option = default(Identifier))
		{
			if (item == null)
			{
				return false;
			}
			if (this.Identifier == Tags.DeconstructThis && item.AllowDeconstruct)
			{
				if (item.AllowDeconstruct && !Item.DeconstructItems.Contains(item))
				{
					if (!item.Prefab.DeconstructItems.None(null))
					{
						if (!item.Prefab.DeconstructItems.Any(delegate(DeconstructItem deconstructItem)
						{
							if (!deconstructItem.RequiredOtherItem.None(null))
							{
								return false;
							}
							if (deconstructItem.RequiredDeconstructor.Length != 0)
							{
								return deconstructItem.RequiredDeconstructor.Any((Identifier d) => d != Tags.GeneticResearchStation);
							}
							return true;
						}))
						{
							goto IL_A0;
						}
					}
					return true;
				}
			}
			else if (this.Identifier == Tags.DontDeconstructThis && Item.DeconstructItems.Contains(item))
			{
				return true;
			}
			IL_A0:
			ImmutableArray<Identifier> targetItems = this.GetTargetItems(option);
			return OrderPrefab.TargetItemsMatchItem(targetItems, item);
		}

		// Token: 0x06002EFB RID: 12027 RVA: 0x001F3D9C File Offset: 0x001F1F9C
		public static bool TargetItemsMatchItem(ImmutableArray<Identifier> targetItems, Item item)
		{
			return item != null && new ImmutableArray<Identifier>?(targetItems) != null && targetItems.Length > 0 && (targetItems.Contains(item.Prefab.Identifier) || item.HasTag(targetItems));
		}

		// Token: 0x06002EFC RID: 12028 RVA: 0x001F3DF0 File Offset: 0x001F1FF0
		public override void Dispose()
		{
		}

		// Token: 0x06002EFD RID: 12029 RVA: 0x001F3DF4 File Offset: 0x001F1FF4
		public Order CreateInstance(OrderPrefab.OrderTargetType targetType, Character orderGiver = null, bool isAutonomous = false)
		{
			Order result;
			try
			{
				Order order;
				switch (targetType)
				{
				case OrderPrefab.OrderTargetType.Entity:
					order = new Order(this, null, null, orderGiver, isAutonomous);
					break;
				case OrderPrefab.OrderTargetType.Position:
					order = new Order(this, null, orderGiver);
					break;
				case OrderPrefab.OrderTargetType.WallSection:
					order = new Order(this, null, null, orderGiver);
					break;
				default:
					throw new NotImplementedException();
				}
				result = order;
			}
			catch (NotImplementedException e)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error creating a new Order instance: unexpected target type \"");
				defaultInterpolatedStringHandler.AppendFormatted<OrderPrefab.OrderTargetType>(targetType);
				defaultInterpolatedStringHandler.AppendLiteral("\".\n");
				defaultInterpolatedStringHandler.AppendFormatted(e.StackTrace.CleanupStackTrace());
				string msg = defaultInterpolatedStringHandler.ToStringAndClear();
				ContentPackage contentPackage = base.ContentPackage;
				DebugConsole.LogError(msg, null, contentPackage);
				result = null;
			}
			return result;
		}

		// Token: 0x04001866 RID: 6246
		public static readonly PrefabCollection<OrderPrefab> Prefabs = new PrefabCollection<OrderPrefab>();

		// Token: 0x04001867 RID: 6247
		public static readonly Identifier DismissalIdentifier = "dismissed".ToIdentifier();

		// Token: 0x04001868 RID: 6248
		public readonly OrderCategory? Category;

		// Token: 0x04001869 RID: 6249
		public readonly Identifier CategoryIdentifier;

		// Token: 0x0400186A RID: 6250
		public readonly LocalizedString Name;

		// Token: 0x0400186B RID: 6251
		public readonly LocalizedString ContextualName;

		// Token: 0x0400186C RID: 6252
		public readonly Sprite SymbolSprite;

		// Token: 0x0400186D RID: 6253
		public readonly Type ItemComponentType;

		// Token: 0x0400186E RID: 6254
		public readonly bool CanTypeBeSubclass;

		// Token: 0x0400186F RID: 6255
		public readonly ImmutableArray<Identifier> TargetItems;

		// Token: 0x04001870 RID: 6256
		public readonly ImmutableArray<Identifier> RequireItems;

		// Token: 0x04001871 RID: 6257
		private readonly ImmutableDictionary<Identifier, ImmutableArray<Identifier>> OptionTargetItems;

		// Token: 0x04001872 RID: 6258
		private readonly Color? color;

		// Token: 0x04001873 RID: 6259
		public readonly bool TargetAllCharacters;

		// Token: 0x04001874 RID: 6260
		public bool TraitorModeOnly;

		// Token: 0x04001875 RID: 6261
		public readonly float FadeOutTime;

		// Token: 0x04001876 RID: 6262
		public readonly bool UseController;

		// Token: 0x04001877 RID: 6263
		public readonly ImmutableArray<Identifier> ControllerTags;

		// Token: 0x04001878 RID: 6264
		public readonly ImmutableArray<Identifier> AppropriateJobs;

		// Token: 0x04001879 RID: 6265
		public readonly ImmutableArray<Identifier> Options;

		// Token: 0x0400187A RID: 6266
		public readonly ImmutableArray<Identifier> HiddenOptions;

		// Token: 0x0400187B RID: 6267
		public readonly ImmutableArray<Identifier> AllOptions;

		// Token: 0x0400187C RID: 6268
		public readonly ListDictionary<Identifier, LocalizedString> OptionNames;

		// Token: 0x0400187D RID: 6269
		public readonly ImmutableDictionary<Identifier, Sprite> OptionSprites;

		// Token: 0x0400187E RID: 6270
		public readonly bool MustSetTarget;

		// Token: 0x0400187F RID: 6271
		public readonly bool CanBeGeneralized;

		// Token: 0x04001880 RID: 6272
		public readonly Identifier AppropriateSkill;

		// Token: 0x04001881 RID: 6273
		public readonly bool Hidden;

		// Token: 0x04001882 RID: 6274
		public readonly bool IgnoreAtOutpost;

		// Token: 0x04001883 RID: 6275
		public readonly bool MustManuallyAssign;

		// Token: 0x04001884 RID: 6276
		public readonly bool AutoDismiss;

		// Token: 0x04001885 RID: 6277
		public readonly ImmutableArray<Identifier> PreferredJobs;

		// Token: 0x02000E74 RID: 3700
		public enum OrderTargetType
		{
			// Token: 0x04005245 RID: 21061
			Entity,
			// Token: 0x04005246 RID: 21062
			Position,
			// Token: 0x04005247 RID: 21063
			WallSection
		}
	}
}
