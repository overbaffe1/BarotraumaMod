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
	// Token: 0x0200008A RID: 138
	internal class OrderPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x170004E0 RID: 1248
		// (get) Token: 0x060011D5 RID: 4565 RVA: 0x0009FFF2 File Offset: 0x0009E1F2
		public static OrderPrefab Dismissal
		{
			get
			{
				return OrderPrefab.Prefabs[OrderPrefab.DismissalIdentifier];
			}
		}

		// Token: 0x170004E1 RID: 1249
		// (get) Token: 0x060011D6 RID: 4566 RVA: 0x000A0003 File Offset: 0x0009E203
		public bool HasOptionSpecificTargetItems
		{
			get
			{
				return this.OptionTargetItems != null && this.OptionTargetItems.Any<KeyValuePair<Identifier, ImmutableArray<Identifier>>>();
			}
		}

		// Token: 0x170004E2 RID: 1250
		// (get) Token: 0x060011D7 RID: 4567 RVA: 0x000A001C File Offset: 0x0009E21C
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

		// Token: 0x170004E3 RID: 1251
		// (get) Token: 0x060011D8 RID: 4568 RVA: 0x000A0074 File Offset: 0x0009E274
		public bool IsReport
		{
			get
			{
				return this.TargetAllCharacters && !this.MustSetTarget;
			}
		}

		// Token: 0x170004E4 RID: 1252
		// (get) Token: 0x060011D9 RID: 4569 RVA: 0x000A008C File Offset: 0x0009E28C
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

		// Token: 0x170004E5 RID: 1253
		// (get) Token: 0x060011DA RID: 4570 RVA: 0x000A00CE File Offset: 0x0009E2CE
		public bool IsDismissal
		{
			get
			{
				return this.Identifier == OrderPrefab.DismissalIdentifier;
			}
		}

		// Token: 0x170004E6 RID: 1254
		// (get) Token: 0x060011DB RID: 4571 RVA: 0x000A00E0 File Offset: 0x0009E2E0
		public bool HasOptions
		{
			get
			{
				return this.Options.Length > 1;
			}
		}

		// Token: 0x170004E7 RID: 1255
		// (get) Token: 0x060011DC RID: 4572 RVA: 0x000A00F0 File Offset: 0x0009E2F0
		public OrderPrefab.OrderTargetType TargetType { get; }

		// Token: 0x170004E8 RID: 1256
		// (get) Token: 0x060011DD RID: 4573 RVA: 0x000A00F8 File Offset: 0x0009E2F8
		public int? WallSectionIndex { get; }

		// Token: 0x170004E9 RID: 1257
		// (get) Token: 0x060011DE RID: 4574 RVA: 0x000A0100 File Offset: 0x0009E300
		public bool IsIgnoreOrder
		{
			get
			{
				return this.Identifier == Tags.IgnoreThis || this.Identifier == Tags.UnignoreThis;
			}
		}

		// Token: 0x170004EA RID: 1258
		// (get) Token: 0x060011DF RID: 4575 RVA: 0x000A0126 File Offset: 0x0009E326
		public bool IsDeconstructOrder
		{
			get
			{
				return this.Identifier == Tags.DeconstructThis || this.Identifier == Tags.DontDeconstructThis;
			}
		}

		// Token: 0x170004EB RID: 1259
		// (get) Token: 0x060011E0 RID: 4576 RVA: 0x000A014C File Offset: 0x0009E34C
		public bool DrawIconWhenContained { get; }

		// Token: 0x170004EC RID: 1260
		// (get) Token: 0x060011E1 RID: 4577 RVA: 0x000A0154 File Offset: 0x0009E354
		public int AssignmentPriority { get; }

		// Token: 0x170004ED RID: 1261
		// (get) Token: 0x060011E2 RID: 4578 RVA: 0x000A015C File Offset: 0x0009E35C
		public bool ColoredWhenControllingGiver { get; }

		// Token: 0x170004EE RID: 1262
		// (get) Token: 0x060011E3 RID: 4579 RVA: 0x000A0164 File Offset: 0x0009E364
		public bool DisplayGiverInTooltip { get; }

		// Token: 0x060011E4 RID: 4580 RVA: 0x000A016C File Offset: 0x0009E36C
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

		// Token: 0x060011E5 RID: 4581 RVA: 0x000A0768 File Offset: 0x0009E968
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

		// Token: 0x060011E6 RID: 4582 RVA: 0x000A0823 File Offset: 0x0009EA23
		public bool HasAppropriateJob(Character character)
		{
			return this.HasSpecifiedJob(character, this.AppropriateJobs);
		}

		// Token: 0x060011E7 RID: 4583 RVA: 0x000A0837 File Offset: 0x0009EA37
		public bool HasPreferredJob(Character character)
		{
			return this.HasSpecifiedJob(character, this.PreferredJobs);
		}

		// Token: 0x060011E8 RID: 4584 RVA: 0x000A084C File Offset: 0x0009EA4C
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

		// Token: 0x060011E9 RID: 4585 RVA: 0x000A0A48 File Offset: 0x0009EC48
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

		// Token: 0x060011EA RID: 4586 RVA: 0x000A0B04 File Offset: 0x0009ED04
		public bool TryGetTargetItemComponent(Item item, out ItemComponent firstMatchingComponent)
		{
			firstMatchingComponent = this.GetTargetItemComponent(item);
			return firstMatchingComponent != null;
		}

		// Token: 0x060011EB RID: 4587 RVA: 0x000A0B14 File Offset: 0x0009ED14
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

		// Token: 0x060011EC RID: 4588 RVA: 0x000A0D04 File Offset: 0x0009EF04
		public List<Item> GetMatchingItems(bool mustBelongToPlayerSub, Character interactableFor = null, Identifier orderOption = default(Identifier))
		{
			Submarine submarine = (Character.Controlled != null && Character.Controlled.TeamID == CharacterTeamType.Team2 && Submarine.MainSubs.Length > 1) ? Submarine.MainSubs[1] : Submarine.MainSub;
			return this.GetMatchingItems(submarine, mustBelongToPlayerSub, null, interactableFor, orderOption);
		}

		// Token: 0x060011ED RID: 4589 RVA: 0x000A0D55 File Offset: 0x0009EF55
		public LocalizedString GetOptionName(string id)
		{
			return this.GetOptionName(id.ToIdentifier());
		}

		// Token: 0x060011EE RID: 4590 RVA: 0x000A0D63 File Offset: 0x0009EF63
		public LocalizedString GetOptionName(Identifier id)
		{
			if (this.OptionNames.ContainsKey(id))
			{
				return this.OptionNames[id];
			}
			return string.Empty;
		}

		// Token: 0x060011EF RID: 4591 RVA: 0x000A0D8A File Offset: 0x0009EF8A
		public LocalizedString GetOptionName(int index)
		{
			if (index < 0 || index >= this.Options.Length)
			{
				return null;
			}
			return this.OptionNames[this.Options[index]];
		}

		// Token: 0x060011F0 RID: 4592 RVA: 0x000A0DB8 File Offset: 0x0009EFB8
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

		// Token: 0x060011F1 RID: 4593 RVA: 0x000A0E18 File Offset: 0x0009F018
		public ImmutableArray<Identifier> GetTargetItems(Identifier option = default(Identifier))
		{
			ImmutableArray<Identifier> optionTargetItems;
			if (option.IsEmpty || !this.OptionTargetItems.TryGetValue(option, out optionTargetItems))
			{
				return this.TargetItems;
			}
			return optionTargetItems;
		}

		// Token: 0x060011F2 RID: 4594 RVA: 0x000A0E48 File Offset: 0x0009F048
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

		// Token: 0x060011F3 RID: 4595 RVA: 0x000A0F04 File Offset: 0x0009F104
		public static bool TargetItemsMatchItem(ImmutableArray<Identifier> targetItems, Item item)
		{
			return item != null && new ImmutableArray<Identifier>?(targetItems) != null && targetItems.Length > 0 && (targetItems.Contains(item.Prefab.Identifier) || item.HasTag(targetItems));
		}

		// Token: 0x060011F4 RID: 4596 RVA: 0x000A0F58 File Offset: 0x0009F158
		public override void Dispose()
		{
		}

		// Token: 0x060011F5 RID: 4597 RVA: 0x000A0F5C File Offset: 0x0009F15C
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

		// Token: 0x0400086C RID: 2156
		public static readonly PrefabCollection<OrderPrefab> Prefabs = new PrefabCollection<OrderPrefab>();

		// Token: 0x0400086D RID: 2157
		public static readonly Identifier DismissalIdentifier = "dismissed".ToIdentifier();

		// Token: 0x0400086E RID: 2158
		public readonly OrderCategory? Category;

		// Token: 0x0400086F RID: 2159
		public readonly Identifier CategoryIdentifier;

		// Token: 0x04000870 RID: 2160
		public readonly LocalizedString Name;

		// Token: 0x04000871 RID: 2161
		public readonly LocalizedString ContextualName;

		// Token: 0x04000872 RID: 2162
		public readonly Sprite SymbolSprite;

		// Token: 0x04000873 RID: 2163
		public readonly Type ItemComponentType;

		// Token: 0x04000874 RID: 2164
		public readonly bool CanTypeBeSubclass;

		// Token: 0x04000875 RID: 2165
		public readonly ImmutableArray<Identifier> TargetItems;

		// Token: 0x04000876 RID: 2166
		public readonly ImmutableArray<Identifier> RequireItems;

		// Token: 0x04000877 RID: 2167
		private readonly ImmutableDictionary<Identifier, ImmutableArray<Identifier>> OptionTargetItems;

		// Token: 0x04000878 RID: 2168
		private readonly Color? color;

		// Token: 0x04000879 RID: 2169
		public readonly bool TargetAllCharacters;

		// Token: 0x0400087A RID: 2170
		public bool TraitorModeOnly;

		// Token: 0x0400087B RID: 2171
		public readonly float FadeOutTime;

		// Token: 0x0400087C RID: 2172
		public readonly bool UseController;

		// Token: 0x0400087D RID: 2173
		public readonly ImmutableArray<Identifier> ControllerTags;

		// Token: 0x0400087E RID: 2174
		public readonly ImmutableArray<Identifier> AppropriateJobs;

		// Token: 0x0400087F RID: 2175
		public readonly ImmutableArray<Identifier> Options;

		// Token: 0x04000880 RID: 2176
		public readonly ImmutableArray<Identifier> HiddenOptions;

		// Token: 0x04000881 RID: 2177
		public readonly ImmutableArray<Identifier> AllOptions;

		// Token: 0x04000882 RID: 2178
		public readonly ListDictionary<Identifier, LocalizedString> OptionNames;

		// Token: 0x04000883 RID: 2179
		public readonly ImmutableDictionary<Identifier, Sprite> OptionSprites;

		// Token: 0x04000884 RID: 2180
		public readonly bool MustSetTarget;

		// Token: 0x04000885 RID: 2181
		public readonly bool CanBeGeneralized;

		// Token: 0x04000886 RID: 2182
		public readonly Identifier AppropriateSkill;

		// Token: 0x04000887 RID: 2183
		public readonly bool Hidden;

		// Token: 0x04000888 RID: 2184
		public readonly bool IgnoreAtOutpost;

		// Token: 0x04000889 RID: 2185
		public readonly bool MustManuallyAssign;

		// Token: 0x0400088A RID: 2186
		public readonly bool AutoDismiss;

		// Token: 0x0400088B RID: 2187
		public readonly ImmutableArray<Identifier> PreferredJobs;

		// Token: 0x0200082C RID: 2092
		public enum OrderTargetType
		{
			// Token: 0x04002EE5 RID: 12005
			Entity,
			// Token: 0x04002EE6 RID: 12006
			Position,
			// Token: 0x04002EE7 RID: 12007
			WallSection
		}
	}
}
