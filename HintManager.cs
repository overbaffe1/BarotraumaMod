using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200006E RID: 110
	internal static class HintManager
	{
		// Token: 0x17000418 RID: 1048
		// (get) Token: 0x06000FEB RID: 4075 RVA: 0x00098D83 File Offset: 0x00096F83
		public static bool Enabled
		{
			get
			{
				return !GameSettings.CurrentConfig.DisableInGameHints;
			}
		}

		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06000FEC RID: 4076 RVA: 0x00098D92 File Offset: 0x00096F92
		// (set) Token: 0x06000FED RID: 4077 RVA: 0x00098D99 File Offset: 0x00096F99
		private static HashSet<Identifier> HintIdentifiers { get; set; }

		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x06000FEE RID: 4078 RVA: 0x00098DA1 File Offset: 0x00096FA1
		private static Dictionary<Identifier, HashSet<Identifier>> HintTags { get; } = new Dictionary<Identifier, HashSet<Identifier>>();

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x06000FEF RID: 4079 RVA: 0x00098DA8 File Offset: 0x00096FA8
		[TupleElementNames(new string[]
		{
			"identifier",
			"option"
		})]
		private static Dictionary<Identifier, ValueTuple<Identifier, Identifier>> HintOrders { [return: TupleElementNames(new string[]
		{
			"identifier",
			"option"
		})] get; } = new Dictionary<Identifier, ValueTuple<Identifier, Identifier>>();

		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x06000FF0 RID: 4080 RVA: 0x00098DAF File Offset: 0x00096FAF
		private static HashSet<Identifier> HintsIgnoredThisRound { get; } = new HashSet<Identifier>();

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x06000FF1 RID: 4081 RVA: 0x00098DB6 File Offset: 0x00096FB6
		// (set) Token: 0x06000FF2 RID: 4082 RVA: 0x00098DBD File Offset: 0x00096FBD
		private static GUIMessageBox ActiveHintMessageBox { get; set; }

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x06000FF3 RID: 4083 RVA: 0x00098DC5 File Offset: 0x00096FC5
		// (set) Token: 0x06000FF4 RID: 4084 RVA: 0x00098DCC File Offset: 0x00096FCC
		private static Action OnUpdate { get; set; }

		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x06000FF5 RID: 4085 RVA: 0x00098DD4 File Offset: 0x00096FD4
		// (set) Token: 0x06000FF6 RID: 4086 RVA: 0x00098DDB File Offset: 0x00096FDB
		private static double TimeStoppedInteracting { get; set; }

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x06000FF7 RID: 4087 RVA: 0x00098DE3 File Offset: 0x00096FE3
		// (set) Token: 0x06000FF8 RID: 4088 RVA: 0x00098DEA File Offset: 0x00096FEA
		private static double TimeRoundStarted { get; set; }

		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x06000FF9 RID: 4089 RVA: 0x00098DF2 File Offset: 0x00096FF2
		// (set) Token: 0x06000FFA RID: 4090 RVA: 0x00098DF9 File Offset: 0x00096FF9
		private static int TimeBeforeReminders { get; set; }

		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x06000FFB RID: 4091 RVA: 0x00098E01 File Offset: 0x00097001
		// (set) Token: 0x06000FFC RID: 4092 RVA: 0x00098E08 File Offset: 0x00097008
		private static int ReminderCooldown { get; set; }

		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06000FFD RID: 4093 RVA: 0x00098E10 File Offset: 0x00097010
		// (set) Token: 0x06000FFE RID: 4094 RVA: 0x00098E17 File Offset: 0x00097017
		private static double TimeReminderLastDisplayed { get; set; }

		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x06000FFF RID: 4095 RVA: 0x00098E1F File Offset: 0x0009701F
		private static HashSet<Hull> BallastHulls { get; } = new HashSet<Hull>();

		// Token: 0x06001000 RID: 4096 RVA: 0x00098E28 File Offset: 0x00097028
		public static void Init()
		{
			if (File.Exists("hintmanager.xml"))
			{
				XDocument doc = XMLExtensions.TryLoadXml("hintmanager.xml");
				if (((doc != null) ? doc.Root : null) != null)
				{
					HintManager.HintIdentifiers = new HashSet<Identifier>();
					using (IEnumerator<XElement> enumerator = doc.Root.Elements().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							XElement element = enumerator.Current;
							HintManager.<Init>g__GetHintsRecursive|47_0(element, element.NameAsIdentifier());
						}
						return;
					}
				}
				DebugConsole.ThrowError("File \"hintmanager.xml\" is empty - cannot initialize the HintManager!", null, null, false, false);
				return;
			}
			DebugConsole.ThrowError("File \"hintmanager.xml\" is missing - cannot initialize the HintManager!", null, null, false, false);
		}

		// Token: 0x06001001 RID: 4097 RVA: 0x00098ECC File Offset: 0x000970CC
		public static void Update()
		{
			if (HintManager.HintIdentifiers == null || GameSettings.CurrentConfig.DisableInGameHints)
			{
				return;
			}
			if (GameMain.GameSession == null || !GameMain.GameSession.IsRunning)
			{
				return;
			}
			if (HintManager.ActiveHintMessageBox != null)
			{
				if (HintManager.ActiveHintMessageBox.Closed)
				{
					HintManager.ActiveHintMessageBox = null;
					HintManager.OnUpdate = null;
				}
				else
				{
					Action onUpdate = HintManager.OnUpdate;
					if (onUpdate == null)
					{
						return;
					}
					onUpdate();
					return;
				}
			}
			HintManager.CheckIsInteracting();
			HintManager.CheckIfDivingGearOutOfOxygen();
			HintManager.CheckHulls();
			HintManager.CheckReminders();
		}

		// Token: 0x06001002 RID: 4098 RVA: 0x00098F48 File Offset: 0x00097148
		public static void OnSetSelectedItem(Character character, Item oldItem, Item newItem)
		{
			if (oldItem == newItem)
			{
				return;
			}
			if (Character.Controlled != null && Character.Controlled == character && oldItem != null && !oldItem.IsLadder)
			{
				HintManager.TimeStoppedInteracting = Timing.TotalTime;
			}
			if (newItem == null)
			{
				return;
			}
			if (newItem.IsLadder)
			{
				return;
			}
			ConnectionPanel cp = newItem.GetComponent<ConnectionPanel>();
			if (cp != null && cp.User == character)
			{
				return;
			}
			HintManager.OnStartedInteracting(character, newItem);
		}

		// Token: 0x06001003 RID: 4099 RVA: 0x00098FA8 File Offset: 0x000971A8
		private static void OnStartedInteracting(Character character, Item item)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled || item == null)
			{
				return;
			}
			string hintIdentifierBase = "onstartedinteracting";
			if (item.Repairables.Any((Repairable r) => r.IsBelowRepairThreshold) && HintManager.DisplayHint((hintIdentifierBase + ".brokenitem").ToIdentifier(), true, null, null, null, null, null))
			{
				return;
			}
			if (item.Repairables.Any((Repairable r) => r.ShouldDrawHUD(character)))
			{
				return;
			}
			Submarine submarine = item.Submarine;
			bool flag;
			if (submarine == null)
			{
				flag = false;
			}
			else
			{
				SubmarineInfo info = submarine.Info;
				flag = (((info != null) ? new SubmarineType?(info.Type) : null).GetValueOrDefault() == SubmarineType.Outpost);
			}
			if (flag)
			{
				if (item.ContainedItems.Any((Item i) => !i.AllowStealing) && HintManager.DisplayHint((hintIdentifierBase + ".lootingisstealing").ToIdentifier(), true, null, null, null, null, null))
				{
					return;
				}
			}
			if (item.HasTag(Tags.Periscope))
			{
				if (item.GetConnectedComponents<Turret>(false, true, null).FirstOrDefault((Turret t) => t.Item.HasTag(Tags.Turret)) != null)
				{
					Identifier hintIdentifier2 = (hintIdentifierBase + ".turretperiscope").ToIdentifier();
					bool extendTextTag = true;
					ValueTuple<Identifier, LocalizedString>[] array = new ValueTuple<Identifier, LocalizedString>[2];
					int num = 0;
					Identifier item2 = "[shootkey]".ToIdentifier();
					GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
					array[num] = new ValueTuple<Identifier, LocalizedString>(item2, keyMap.KeyBindText(InputType.Shoot));
					int num2 = 1;
					Identifier item3 = "[deselectkey]".ToIdentifier();
					keyMap = GameSettings.CurrentConfig.KeyMap;
					array[num2] = new ValueTuple<Identifier, LocalizedString>(item3, keyMap.KeyBindText(InputType.Deselect));
					if (HintManager.DisplayHint(hintIdentifier2, extendTextTag, array, null, null, null, null))
					{
						return;
					}
				}
			}
			hintIdentifierBase += ".item";
			foreach (Identifier hintIdentifier in HintManager.HintIdentifiers)
			{
				HashSet<Identifier> hintTags;
				if (hintIdentifier.StartsWith(hintIdentifierBase) && HintManager.HintTags.TryGetValue(hintIdentifier, out hintTags) && item.HasTag(hintTags) && HintManager.DisplayHint(hintIdentifier, true, null, null, null, null, null))
				{
					break;
				}
			}
		}

		// Token: 0x06001004 RID: 4100 RVA: 0x00099228 File Offset: 0x00097428
		public static void OnStartRepairing(Character character, Repairable repairable)
		{
			if (repairable.ForceDeteriorationTimer > 0f && !character.IsTraitor)
			{
				CoroutineManager.Invoke(delegate
				{
					HintManager.DisplayHint("repairingsabotageditem".ToIdentifier(), true, null, null, null, null, null);
				}, 5f);
			}
		}

		// Token: 0x06001005 RID: 4101 RVA: 0x00099274 File Offset: 0x00097474
		public static void OnItemMarkedForRelocation()
		{
			HintManager.DisplayHint("onitemmarkedforrelocation".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x06001006 RID: 4102 RVA: 0x000992A0 File Offset: 0x000974A0
		public static void OnItemMarkedForDeconstruction(Character character)
		{
			if (character == Character.Controlled)
			{
				HintManager.DisplayHint("onitemmarkedfordeconstruction".ToIdentifier(), true, null, null, null, null, null);
			}
		}

		// Token: 0x06001007 RID: 4103 RVA: 0x000992D4 File Offset: 0x000974D4
		private static void CheckIsInteracting()
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			Character controlled = Character.Controlled;
			if (((controlled != null) ? controlled.SelectedItem : null) == null)
			{
				return;
			}
			Reactor reactor = Character.Controlled.SelectedItem.GetComponent<Reactor>();
			if (reactor != null && reactor.PowerOn)
			{
				ItemInventory ownInventory = Character.Controlled.SelectedItem.OwnInventory;
				IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
				if (containedItems != null)
				{
					if (containedItems.Count((Item i) => i.HasTag(Tags.ReactorFuel)) > 1)
					{
						HintManager.DisplayHint("onisinteracting.reactorwithextrarods".ToIdentifier(), true, null, null, null, null, null);
						return;
					}
				}
			}
		}

		// Token: 0x06001008 RID: 4104 RVA: 0x00099380 File Offset: 0x00097580
		public static void OnRoundStarted()
		{
			HintManager.Reset();
			HintManager.TimeRoundStarted = GameMain.GameScreen.GameTime;
			CoroutineHandle initRoundHandle = CoroutineManager.StartCoroutine(HintManager.<OnRoundStarted>g__InitRound|55_0(), "HintManager.InitRound");
			if (!HintManager.CanDisplayHints(false, false))
			{
				return;
			}
			CoroutineManager.StartCoroutine(HintManager.<OnRoundStarted>g__DisplayRoundStartedHints|55_1(initRoundHandle), "HintManager.DisplayRoundStartedHints");
		}

		// Token: 0x06001009 RID: 4105 RVA: 0x000993CC File Offset: 0x000975CC
		public static void OnRoundEnded()
		{
			HintManager.Reset();
		}

		// Token: 0x0600100A RID: 4106 RVA: 0x000993D4 File Offset: 0x000975D4
		private static void Reset()
		{
			CoroutineManager.StopCoroutines("HintManager.InitRound");
			CoroutineManager.StopCoroutines("HintManager.DisplayRoundStartedHints");
			if (HintManager.ActiveHintMessageBox != null)
			{
				GUIMessageBox.MessageBoxes.Remove(HintManager.ActiveHintMessageBox);
				HintManager.ActiveHintMessageBox = null;
			}
			HintManager.OnUpdate = null;
			HintManager.HintsIgnoredThisRound.Clear();
		}

		// Token: 0x0600100B RID: 4107 RVA: 0x00099424 File Offset: 0x00097624
		public static void OnSonarSpottedCharacter(Item sonar, Character spottedCharacter)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (sonar == null || sonar.Removed)
			{
				return;
			}
			if (spottedCharacter == null || spottedCharacter.Removed || spottedCharacter.IsDead)
			{
				return;
			}
			if (Character.Controlled.SelectedItem != sonar)
			{
				return;
			}
			if (HumanAIController.IsFriendly(Character.Controlled, spottedCharacter, false, false))
			{
				return;
			}
			HintManager.DisplayHint("onsonarspottedenemy".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x0600100C RID: 4108 RVA: 0x00099498 File Offset: 0x00097698
		public static void OnAfflictionDisplayed(Character character, List<Affliction> displayedAfflictions)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled || displayedAfflictions == null)
			{
				return;
			}
			foreach (Affliction affliction in displayedAfflictions)
			{
				if (((affliction != null) ? affliction.Prefab : null) != null && !affliction.Prefab.IsBuff && affliction.Prefab != AfflictionPrefab.OxygenLow)
				{
					if (affliction.Prefab == AfflictionPrefab.RadiationSickness)
					{
						Map map = GameMain.GameSession.Map;
						float? num;
						if (map == null)
						{
							num = null;
						}
						else
						{
							Radiation radiation = map.Radiation;
							num = ((radiation != null) ? new float?(radiation.DepthInRadiation(character)) : null);
						}
						float? num2 = num;
						if (num2.GetValueOrDefault() > 0f)
						{
							continue;
						}
					}
					if (affliction.Strength >= affliction.Prefab.ShowIconThreshold)
					{
						Identifier hintIdentifier = "onafflictiondisplayed".ToIdentifier();
						bool extendTextTag = true;
						ValueTuple<Identifier, LocalizedString>[] array = new ValueTuple<Identifier, LocalizedString>[1];
						int num3 = 0;
						Identifier item = "[key]".ToIdentifier();
						GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
						array[num3] = new ValueTuple<Identifier, LocalizedString>(item, keyMap.KeyBindText(InputType.Health));
						HintManager.DisplayHint(hintIdentifier, extendTextTag, array, affliction.Prefab.Icon, new Color?(CharacterHealth.GetAfflictionIconColor(affliction)), null, delegate
						{
							if (CharacterHealth.OpenHealthWindow == null)
							{
								return;
							}
							HintManager.ActiveHintMessageBox.Close();
						});
						break;
					}
				}
			}
		}

		// Token: 0x0600100D RID: 4109 RVA: 0x0009961C File Offset: 0x0009781C
		public static void OnShootWithoutAiming(Character character, Item item)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled)
			{
				return;
			}
			if (character.HasSelectedAnyItem || character.FocusedItem != null)
			{
				return;
			}
			if (item == null || !item.IsShootable || !item.RequireAimToUse)
			{
				return;
			}
			if (HintManager.TimeStoppedInteracting + 1.0 > Timing.TotalTime)
			{
				return;
			}
			if (GUI.MouseOn != null)
			{
				return;
			}
			CharacterInventory inventory = Character.Controlled.Inventory;
			if (((inventory != null) ? inventory.visualSlots : null) != null)
			{
				if (Character.Controlled.Inventory.visualSlots.Any((VisualSlot s) => s.InteractRect.Contains(PlayerInput.MousePosition)))
				{
					return;
				}
			}
			Identifier hintIdentifier = "onshootwithoutaiming".ToIdentifier();
			HashSet<Identifier> tags;
			if (!HintManager.HintTags.TryGetValue(hintIdentifier, out tags))
			{
				return;
			}
			if (!item.HasTag(tags))
			{
				return;
			}
			Identifier hintIdentifier2 = hintIdentifier;
			bool extendTextTag = true;
			ValueTuple<Identifier, LocalizedString>[] array = new ValueTuple<Identifier, LocalizedString>[1];
			int num = 0;
			Identifier item2 = "[key]".ToIdentifier();
			GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
			array[num] = new ValueTuple<Identifier, LocalizedString>(item2, keyMap.KeyBindText(InputType.Aim));
			HintManager.DisplayHint(hintIdentifier2, extendTextTag, array, null, null, null, delegate
			{
				if (character.SelectedItem == null && GUI.MouseOn == null && PlayerInput.KeyDown(InputType.Aim))
				{
					HintManager.ActiveHintMessageBox.Close();
				}
			});
		}

		// Token: 0x0600100E RID: 4110 RVA: 0x00099760 File Offset: 0x00097960
		public static void OnWeldingDoor(Character character, Door door)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled)
			{
				return;
			}
			if (door == null || door.Stuck < 20f)
			{
				return;
			}
			HintManager.DisplayHint("onweldingdoor".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x0600100F RID: 4111 RVA: 0x000997B0 File Offset: 0x000979B0
		public static void OnTryOpenStuckDoor(Character character)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled)
			{
				return;
			}
			HintManager.DisplayHint("ontryopenstuckdoor".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x06001010 RID: 4112 RVA: 0x000997F0 File Offset: 0x000979F0
		public static void OnShowCampaignInterface(CampaignMode.InteractionType interactionType)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (interactionType == CampaignMode.InteractionType.None)
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
			defaultInterpolatedStringHandler.AppendLiteral("onshowcampaigninterface.");
			defaultInterpolatedStringHandler.AppendFormatted<CampaignMode.InteractionType>(interactionType);
			Identifier hintIdentifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			HintManager.DisplayHint(hintIdentifier, true, null, null, null, null, delegate
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
				if (campaign != null && (campaign.ShowCampaignUI || campaign.ForceMapUI))
				{
					CampaignUI campaignUI = campaign.CampaignUI;
					if (campaignUI != null && campaignUI.SelectedTab == CampaignMode.InteractionType.Map)
					{
						return;
					}
				}
				HintManager.ActiveHintMessageBox.Close();
			});
		}

		// Token: 0x06001011 RID: 4113 RVA: 0x0009986C File Offset: 0x00097A6C
		public static void OnShowCommandInterface()
		{
			HintManager.IgnoreReminder("commandinterface");
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			HintManager.DisplayHint("onshowcommandinterface".ToIdentifier(), true, null, null, null, null, delegate
			{
				if (CrewManager.IsCommandInterfaceOpen)
				{
					return;
				}
				HintManager.ActiveHintMessageBox.Close();
			});
		}

		// Token: 0x06001012 RID: 4114 RVA: 0x000998CC File Offset: 0x00097ACC
		public static void OnShowHealthInterface()
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (CharacterHealth.OpenHealthWindow == null)
			{
				return;
			}
			HintManager.DisplayHint("onshowhealthinterface".ToIdentifier(), true, null, null, null, null, delegate
			{
				if (CharacterHealth.OpenHealthWindow != null)
				{
					return;
				}
				HintManager.ActiveHintMessageBox.Close();
			});
		}

		// Token: 0x06001013 RID: 4115 RVA: 0x00099927 File Offset: 0x00097B27
		public static void OnShowTabMenu()
		{
			HintManager.IgnoreReminder("tabmenu");
		}

		// Token: 0x06001014 RID: 4116 RVA: 0x00099934 File Offset: 0x00097B34
		public static void OnObtainedItem(Character character, Item item)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled || item == null)
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler.AppendLiteral("onobtaineditem.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(item.Prefab.Identifier);
			if (HintManager.DisplayHint(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), true, null, null, null, null, null))
			{
				return;
			}
			foreach (Identifier tag in item.GetTags())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("onobtaineditem.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(tag);
				if (HintManager.DisplayHint(defaultInterpolatedStringHandler2.ToStringAndClear().ToIdentifier(), true, null, null, null, null, null))
				{
					return;
				}
			}
			if ((item.HasTag(Tags.GeneticMaterial) && character.Inventory.FindItemByTag(Tags.GeneticMaterial, true) != null) || (item.HasTag(Tags.GeneticDevice) && character.Inventory.FindItemByTag(Tags.GeneticDevice, true) != null))
			{
				HintManager.DisplayHint("geneticmaterial.useinstructions".ToIdentifier(), true, null, null, null, null, null);
				return;
			}
		}

		// Token: 0x06001015 RID: 4117 RVA: 0x00099A7C File Offset: 0x00097C7C
		public static void OnStartDeconstructing(Character character, Deconstructor deconstructor)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled || deconstructor == null)
			{
				return;
			}
			if (deconstructor.InputContainer.Inventory.AllItems.All((Item it) => it.GetComponent<GeneticMaterial>() != null))
			{
				HintManager.DisplayHint("geneticmaterial.onrefiningorcombining".ToIdentifier(), true, null, null, null, null, null);
			}
		}

		// Token: 0x06001016 RID: 4118 RVA: 0x00099AF4 File Offset: 0x00097CF4
		public static void OnStoleItem(Character character, Item item)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled)
			{
				return;
			}
			if (item == null || item.AllowStealing || !item.StolenDuringRound)
			{
				return;
			}
			HintManager.DisplayHint("onstoleitem".ToIdentifier(), true, null, null, null, null, delegate
			{
				if (item == null || item.Removed || item.GetRootInventoryOwner() != character)
				{
					HintManager.ActiveHintMessageBox.Close();
				}
			});
		}

		// Token: 0x06001017 RID: 4119 RVA: 0x00099B7C File Offset: 0x00097D7C
		public static void OnHandcuffed(Character character)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled || !character.LockHands)
			{
				return;
			}
			HintManager.DisplayHint("onhandcuffed".ToIdentifier(), true, null, null, null, null, delegate
			{
				if (character != null && !character.Removed && character.LockHands)
				{
					return;
				}
				HintManager.ActiveHintMessageBox.Close();
			});
		}

		// Token: 0x06001018 RID: 4120 RVA: 0x00099BE4 File Offset: 0x00097DE4
		public static void OnRadioJammed(Item radioItem)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			CharacterInventory characterInventory = ((radioItem != null) ? radioItem.ParentInventory : null) as CharacterInventory;
			if (characterInventory == null)
			{
				return;
			}
			if (characterInventory.Owner != Character.Controlled)
			{
				return;
			}
			HintManager.DisplayHint("radiojammed".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x06001019 RID: 4121 RVA: 0x00099C40 File Offset: 0x00097E40
		public static void OnReactorOutOfFuel(Reactor reactor)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (reactor == null)
			{
				return;
			}
			Submarine submarine = reactor.Item.Submarine;
			bool flag;
			if (submarine == null)
			{
				flag = true;
			}
			else
			{
				SubmarineInfo info = submarine.Info;
				SubmarineType? submarineType = (info != null) ? new SubmarineType?(info.Type) : null;
				SubmarineType submarineType2 = SubmarineType.Player;
				flag = !(submarineType.GetValueOrDefault() == submarineType2 & submarineType != null);
			}
			if (flag || reactor.Item.Submarine.TeamID != Character.Controlled.TeamID)
			{
				return;
			}
			if (!HintManager.HasValidJob("engineer"))
			{
				return;
			}
			HintManager.DisplayHint("onreactoroutoffuel".ToIdentifier(), true, null, null, null, null, delegate
			{
				Reactor reactor2 = reactor;
				if (((reactor2 != null) ? reactor2.Item : null) != null && !reactor.Item.Removed && reactor.AvailableFuel < 1f)
				{
					return;
				}
				HintManager.ActiveHintMessageBox.Close();
			});
		}

		// Token: 0x0600101A RID: 4122 RVA: 0x00099D18 File Offset: 0x00097F18
		public static void OnAssignedAsTraitor()
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			HintManager.DisplayHint("assignedastraitor".ToIdentifier(), true, null, null, null, null, null);
			HintManager.DisplayHint("assignedastraitor2".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x0600101B RID: 4123 RVA: 0x00099D6C File Offset: 0x00097F6C
		public static void OnAvailableTransition(CampaignMode.TransitionType transitionType)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (transitionType == CampaignMode.TransitionType.None)
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 1);
			defaultInterpolatedStringHandler.AppendLiteral("onavailabletransition.");
			defaultInterpolatedStringHandler.AppendFormatted<CampaignMode.TransitionType>(transitionType);
			HintManager.DisplayHint(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x0600101C RID: 4124 RVA: 0x00099DC5 File Offset: 0x00097FC5
		public static void OnShowSubInventory(Item item)
		{
			if (((item != null) ? item.Prefab : null) == null)
			{
				return;
			}
			if (item.Prefab.Identifier == "toolbelt")
			{
				HintManager.IgnoreReminder("toolbelt");
			}
		}

		// Token: 0x0600101D RID: 4125 RVA: 0x00099DF7 File Offset: 0x00097FF7
		public static void OnChangeCharacter()
		{
			HintManager.IgnoreReminder("characterchange");
		}

		// Token: 0x0600101E RID: 4126 RVA: 0x00099E04 File Offset: 0x00098004
		public static void OnCharacterUnconscious(Character character)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled)
			{
				return;
			}
			if (character.IsDead)
			{
				return;
			}
			if (character.CharacterHealth != null && character.Vitality < character.CharacterHealth.MinVitality)
			{
				return;
			}
			HintManager.DisplayHint("oncharacterunconscious".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x0600101F RID: 4127 RVA: 0x00099E68 File Offset: 0x00098068
		public static void OnCharacterKilled(Character character)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled)
			{
				return;
			}
			if (GameMain.IsMultiplayer)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.CrewManager : null) == null)
			{
				return;
			}
			if (GameMain.GameSession.CrewManager.GetCharacters().None((Character c) => !c.IsDead))
			{
				return;
			}
			HintManager.DisplayHint("oncharacterkilled".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x06001020 RID: 4128 RVA: 0x00099EF8 File Offset: 0x000980F8
		private static void OnStartedControlling()
		{
			HintManager.<>c__DisplayClass79_0 CS$<>8__locals1 = new HintManager.<>c__DisplayClass79_0();
			if (Level.IsLoadedOutpost)
			{
				return;
			}
			Character controlled = Character.Controlled;
			bool flag;
			if (controlled == null)
			{
				flag = (null != null);
			}
			else
			{
				CharacterInfo info = controlled.Info;
				if (info == null)
				{
					flag = (null != null);
				}
				else
				{
					Job job = info.Job;
					flag = (((job != null) ? job.Prefab : null) != null);
				}
			}
			if (!flag)
			{
				return;
			}
			HintManager.<>c__DisplayClass79_0 CS$<>8__locals2 = CS$<>8__locals1;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
			defaultInterpolatedStringHandler.AppendLiteral("onstartedcontrolling.job.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(Character.Controlled.Info.Job.Prefab.Identifier);
			CS$<>8__locals2.hintIdentifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			HintManager.DisplayHint(CS$<>8__locals1.hintIdentifier, true, null, Character.Controlled.Info.Job.Prefab.Icon, new Color?(Character.Controlled.Info.Job.Prefab.UIColor), delegate
			{
				ValueTuple<Identifier, Identifier> orderInfo;
				if (!HintManager.HintOrders.TryGetValue(CS$<>8__locals1.hintIdentifier, out orderInfo))
				{
					return;
				}
				OrderPrefab orderPrefab = OrderPrefab.Prefabs[orderInfo.Item1];
				if (orderPrefab == null)
				{
					return;
				}
				Item targetEntity = null;
				ItemComponent targetItem = null;
				if (orderPrefab.MustSetTarget)
				{
					targetEntity = orderPrefab.GetMatchingItems(true, Character.Controlled, orderInfo.Item2).FirstOrDefault<Item>();
					if (targetEntity == null)
					{
						return;
					}
					targetItem = orderPrefab.GetTargetItemComponent(targetEntity);
				}
				Order order = new Order(orderPrefab, orderInfo.Item2, targetEntity, targetItem, Character.Controlled, false).WithManualPriority(CharacterInfo.HighestManualOrderPriority);
				GameMain.GameSession.CrewManager.SetCharacterOrder(Character.Controlled, order, true);
			}, null);
		}

		// Token: 0x06001021 RID: 4129 RVA: 0x00099FDC File Offset: 0x000981DC
		public static void OnAutoPilotPathUpdated(Steering steering)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (!HintManager.HasValidJob("captain"))
			{
				return;
			}
			bool flag;
			if (steering == null)
			{
				flag = (null != null);
			}
			else
			{
				Item item = steering.Item;
				if (item == null)
				{
					flag = (null != null);
				}
				else
				{
					Submarine submarine = item.Submarine;
					flag = (((submarine != null) ? submarine.Info : null) != null);
				}
			}
			if (!flag)
			{
				return;
			}
			if (steering.Item.Submarine.Info.Type != SubmarineType.Player)
			{
				return;
			}
			if (steering.Item.Submarine.TeamID != Character.Controlled.TeamID)
			{
				return;
			}
			if (!steering.AutoPilot || steering.MaintainPos)
			{
				return;
			}
			SteeringPath steeringPath = steering.SteeringPath;
			bool flag2;
			if (steeringPath == null)
			{
				flag2 = true;
			}
			else
			{
				WayPoint currentNode = steeringPath.CurrentNode;
				Level.TunnelType? tunnelType;
				if (currentNode == null)
				{
					tunnelType = null;
				}
				else
				{
					Level.Tunnel tunnel = currentNode.Tunnel;
					tunnelType = ((tunnel != null) ? new Level.TunnelType?(tunnel.Type) : null);
				}
				Level.TunnelType? tunnelType2 = tunnelType;
				Level.TunnelType tunnelType3 = Level.TunnelType.MainPath;
				flag2 = !(tunnelType2.GetValueOrDefault() == tunnelType3 & tunnelType2 != null);
			}
			if (flag2)
			{
				return;
			}
			if (!steering.SteeringPath.Finished && steering.SteeringPath.NextNode != null)
			{
				return;
			}
			if (steering.LevelStartSelected && (Level.Loaded.StartOutpost == null || !steering.Item.Submarine.AtStartExit))
			{
				return;
			}
			if (steering.LevelEndSelected && (Level.Loaded.EndOutpost == null || !steering.Item.Submarine.AtEndExit))
			{
				return;
			}
			HintManager.DisplayHint("onautopilotreachedoutpost".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x06001022 RID: 4130 RVA: 0x0009A150 File Offset: 0x00098350
		public static void OnStatusEffectApplied(ItemComponent component, ActionType actionType, Character character)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character != Character.Controlled)
			{
				return;
			}
			if (!(component is Repairable) || actionType != ActionType.OnFailure)
			{
				return;
			}
			HintManager.DisplayHint("onrepairfailed".ToIdentifier(), true, null, null, null, null, null);
		}

		// Token: 0x06001023 RID: 4131 RVA: 0x0009A19C File Offset: 0x0009839C
		public static void OnActiveOrderAdded(Order order)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (order == null)
			{
				return;
			}
			Identifier identifier = order.Identifier;
			if (identifier == "reportballastflora")
			{
				Hull h = order.TargetEntity as Hull;
				if (h != null)
				{
					Submarine submarine = h.Submarine;
					CharacterTeamType? characterTeamType = (submarine != null) ? new CharacterTeamType?(submarine.TeamID) : null;
					CharacterTeamType teamID = Character.Controlled.TeamID;
					if (characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null)
					{
						HintManager.DisplayHint("onballastflorainfected".ToIdentifier(), true, null, null, null, null, null);
					}
				}
			}
			identifier = order.Identifier;
			if (identifier == "deconstructitems" && Item.DeconstructItems.None(null))
			{
				HintManager.DisplayHint("ondeconstructorder".ToIdentifier(), true, null, null, null, null, null);
			}
		}

		// Token: 0x06001024 RID: 4132 RVA: 0x0009A27C File Offset: 0x0009847C
		public static void OnSetOrder(Character character, Order order)
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (character == null || order == null)
			{
				return;
			}
			if (order.OrderGiver == Character.Controlled)
			{
				Identifier identifier = order.Identifier;
				if (identifier == "deconstructitems" && Item.DeconstructItems.None(null))
				{
					HintManager.DisplayHint("ondeconstructorder".ToIdentifier(), true, null, null, null, null, null);
				}
			}
		}

		// Token: 0x06001025 RID: 4133 RVA: 0x0009A2E8 File Offset: 0x000984E8
		private static void CheckIfDivingGearOutOfOxygen()
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			Item divingGear = Character.Controlled.GetEquippedItem(Tags.DivingGear, new InvSlotType?(InvSlotType.OuterClothes));
			Item divingGear2 = divingGear;
			if (((divingGear2 != null) ? divingGear2.OwnInventory : null) == null)
			{
				return;
			}
			if (divingGear.GetContainedItemConditionPercentage() > 0f)
			{
				return;
			}
			HintManager.DisplayHint("ondivinggearoutofoxygen".ToIdentifier(), true, null, null, null, null, delegate
			{
				if (divingGear == null || divingGear.Removed || Character.Controlled == null || !Character.Controlled.HasEquippedItem(divingGear, null, null) || divingGear.GetContainedItemConditionPercentage() > 0f)
				{
					HintManager.ActiveHintMessageBox.Close();
				}
			});
		}

		// Token: 0x06001026 RID: 4134 RVA: 0x0009A374 File Offset: 0x00098574
		private static void CheckHulls()
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (Character.Controlled.CurrentHull == null)
			{
				return;
			}
			if (HumanAIController.IsBallastFloraNoticeable(Character.Controlled, Character.Controlled.CurrentHull) && HintManager.<CheckHulls>g__IsOnFriendlySub|85_0() && HintManager.DisplayHint("onballastflorainfected".ToIdentifier(), true, null, null, null, null, null))
			{
				return;
			}
			foreach (Gap gap in Character.Controlled.CurrentHull.ConnectedGaps)
			{
				if (gap.ConnectedDoor != null && !gap.ConnectedDoor.Impassable && Vector2.DistanceSquared(Character.Controlled.WorldPosition, gap.ConnectedDoor.Item.WorldPosition) <= 160000f)
				{
					if (!gap.IsRoomToRoom)
					{
						if (HintManager.<CheckHulls>g__IsWearingDivingSuit|85_1() && !Character.Controlled.IsProtectedFromPressure && HintManager.DisplayHint("divingsuitwarning".ToIdentifier(), false, null, null, null, null, null))
						{
							break;
						}
					}
					else
					{
						foreach (MapEntity me in gap.linkedTo)
						{
							if (me != Character.Controlled.CurrentHull)
							{
								Hull adjacentHull = me as Hull;
								if (adjacentHull != null && HintManager.<CheckHulls>g__IsOnFriendlySub|85_0() && !HintManager.<CheckHulls>g__IsWearingDivingSuit|85_1())
								{
									if (adjacentHull.LethalPressure > 5f && HintManager.DisplayHint("onadjacenthull.highpressure".ToIdentifier(), true, null, null, null, null, null))
									{
										return;
									}
									if (adjacentHull.WaterPercentage > 75f && !HintManager.BallastHulls.Contains(adjacentHull) && HintManager.DisplayHint("onadjacenthull.highwaterpercentage".ToIdentifier(), true, null, null, null, null, null))
									{
										return;
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06001027 RID: 4135 RVA: 0x0009A5A4 File Offset: 0x000987A4
		private static void CheckReminders()
		{
			if (!HintManager.CanDisplayHints(true, true))
			{
				return;
			}
			if (Level.Loaded == null)
			{
				return;
			}
			if (GameMain.GameScreen.GameTime < HintManager.TimeRoundStarted + (double)HintManager.TimeBeforeReminders)
			{
				return;
			}
			if (GameMain.GameScreen.GameTime < HintManager.TimeReminderLastDisplayed + (double)HintManager.ReminderCooldown)
			{
				return;
			}
			string hintIdentifierBase = "reminder";
			if (GameMain.GameSession.GameMode.IsSinglePlayer && HintManager.DisplayHint((hintIdentifierBase + ".characterchange").ToIdentifier(), true, null, null, null, null, null))
			{
				HintManager.TimeReminderLastDisplayed = GameMain.GameScreen.GameTime;
				return;
			}
			GameSettings.Config.KeyMapping keyMap;
			if (Level.Loaded.Type != LevelData.LevelType.Outpost)
			{
				Identifier hintIdentifier = (hintIdentifierBase + ".commandinterface").ToIdentifier();
				bool extendTextTag = true;
				ValueTuple<Identifier, LocalizedString>[] array = new ValueTuple<Identifier, LocalizedString>[1];
				int num = 0;
				Identifier item = "[commandkey]".ToIdentifier();
				keyMap = GameSettings.CurrentConfig.KeyMap;
				array[num] = new ValueTuple<Identifier, LocalizedString>(item, keyMap.KeyBindText(InputType.Command));
				if (HintManager.DisplayHint(hintIdentifier, extendTextTag, array, null, null, null, delegate
				{
					if (!CrewManager.IsCommandInterfaceOpen)
					{
						return;
					}
					HintManager.ActiveHintMessageBox.Close();
				}))
				{
					HintManager.TimeReminderLastDisplayed = GameMain.GameScreen.GameTime;
					return;
				}
			}
			Identifier hintIdentifier2 = (hintIdentifierBase + ".tabmenu").ToIdentifier();
			bool extendTextTag2 = true;
			ValueTuple<Identifier, LocalizedString>[] array2 = new ValueTuple<Identifier, LocalizedString>[1];
			int num2 = 0;
			Identifier item2 = "[infotabkey]".ToIdentifier();
			keyMap = GameSettings.CurrentConfig.KeyMap;
			array2[num2] = new ValueTuple<Identifier, LocalizedString>(item2, keyMap.KeyBindText(InputType.InfoTab));
			if (HintManager.DisplayHint(hintIdentifier2, extendTextTag2, array2, null, null, null, delegate
			{
				if (!GameSession.IsTabMenuOpen)
				{
					return;
				}
				HintManager.ActiveHintMessageBox.Close();
			}))
			{
				HintManager.TimeReminderLastDisplayed = GameMain.GameScreen.GameTime;
				return;
			}
			CharacterInventory inventory = Character.Controlled.Inventory;
			Identifier? identifier;
			Identifier? identifier2;
			if (inventory == null)
			{
				identifier = null;
				identifier2 = identifier;
			}
			else
			{
				Item itemInLimbSlot = inventory.GetItemInLimbSlot(InvSlotType.Bag);
				if (itemInLimbSlot == null)
				{
					identifier = null;
					identifier2 = identifier;
				}
				else
				{
					ItemPrefab prefab = itemInLimbSlot.Prefab;
					if (prefab == null)
					{
						identifier = null;
						identifier2 = identifier;
					}
					else
					{
						identifier2 = new Identifier?(prefab.Identifier);
					}
				}
			}
			identifier = identifier2;
			if (identifier == "toolbelt" && HintManager.DisplayHint((hintIdentifierBase + ".toolbelt").ToIdentifier(), true, null, null, null, null, null))
			{
				HintManager.TimeReminderLastDisplayed = GameMain.GameScreen.GameTime;
				return;
			}
		}

		// Token: 0x06001028 RID: 4136 RVA: 0x0009A7EC File Offset: 0x000989EC
		private static bool DisplayHint(Identifier hintIdentifier, bool extendTextTag = true, [TupleElementNames(new string[]
		{
			"Tag",
			"Value"
		})] ValueTuple<Identifier, LocalizedString>[] variables = null, Sprite icon = null, Color? iconColor = null, Action onDisplay = null, Action onUpdate = null)
		{
			if (hintIdentifier == Identifier.Empty)
			{
				return false;
			}
			if (!HintManager.HintIdentifiers.Contains(hintIdentifier))
			{
				return false;
			}
			if (IgnoredHints.Instance.Contains(hintIdentifier))
			{
				return false;
			}
			if (HintManager.HintsIgnoredThisRound.Contains(hintIdentifier))
			{
				return false;
			}
			Identifier identifier;
			if (!extendTextTag)
			{
				identifier = hintIdentifier;
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 1);
				defaultInterpolatedStringHandler.AppendLiteral("hint.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(hintIdentifier);
				identifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			}
			Identifier textTag = identifier;
			LocalizedString text;
			if (variables != null && variables.Length != 0)
			{
				text = TextManager.GetWithVariables(textTag, variables);
			}
			else
			{
				text = TextManager.Get(textTag);
			}
			if (text.IsNullOrEmpty())
			{
				return false;
			}
			HintManager.HintsIgnoredThisRound.Add(hintIdentifier);
			HintManager.ActiveHintMessageBox = new GUIMessageBox(hintIdentifier, TextManager.ParseInputTypes(text, false), icon);
			if (iconColor != null)
			{
				HintManager.ActiveHintMessageBox.IconColor = iconColor.Value;
			}
			HintManager.OnUpdate = onUpdate;
			SoundPlayer.PlayUISound(GUISoundType.UIMessage);
			HintManager.ActiveHintMessageBox.InnerFrame.Flash(new Color?(iconColor ?? Color.Orange), 0.75f, false, false, null);
			if (onDisplay != null)
			{
				onDisplay();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("HintManager:");
			GameSession gameSession = GameMain.GameSession;
			Identifier? identifier2;
			if (gameSession == null)
			{
				identifier2 = null;
			}
			else
			{
				GameMode gameMode = gameSession.GameMode;
				if (gameMode == null)
				{
					identifier2 = null;
				}
				else
				{
					GameModePreset preset = gameMode.Preset;
					identifier2 = ((preset != null) ? new Identifier?(preset.Identifier) : null);
				}
			}
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(identifier2 ?? "none".ToIdentifier());
			defaultInterpolatedStringHandler2.AppendLiteral(":HintDisplayed:");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(hintIdentifier);
			GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler2.ToStringAndClear());
			return true;
		}

		// Token: 0x06001029 RID: 4137 RVA: 0x0009A9BD File Offset: 0x00098BBD
		public static bool OnDontShowAgain(GUITickBox tickBox)
		{
			HintManager.IgnoreHint((Identifier)tickBox.UserData, tickBox.Selected);
			return true;
		}

		// Token: 0x0600102A RID: 4138 RVA: 0x0009A9D6 File Offset: 0x00098BD6
		private static void IgnoreHint(Identifier hintIdentifier, bool ignore = true)
		{
			if (hintIdentifier.IsEmpty)
			{
				return;
			}
			if (!HintManager.HintIdentifiers.Contains(hintIdentifier))
			{
				return;
			}
			if (ignore)
			{
				IgnoredHints.Instance.Add(hintIdentifier);
				return;
			}
			IgnoredHints.Instance.Remove(hintIdentifier);
		}

		// Token: 0x0600102B RID: 4139 RVA: 0x0009AA0A File Offset: 0x00098C0A
		private static void IgnoreReminder(string reminderIdentifier)
		{
			HintManager.HintsIgnoredThisRound.Add(("reminder." + reminderIdentifier).ToIdentifier());
		}

		// Token: 0x0600102C RID: 4140 RVA: 0x0009AA28 File Offset: 0x00098C28
		public unsafe static bool OnDisableHints(GUITickBox tickBox)
		{
			GameSettings.Config config = *GameSettings.CurrentConfig;
			config.DisableInGameHints = tickBox.Selected;
			GameSettings.SetCurrentConfig(config);
			GameSettings.SaveCurrentConfig();
			return true;
		}

		// Token: 0x0600102D RID: 4141 RVA: 0x0009AA5C File Offset: 0x00098C5C
		private static bool CanDisplayHints(bool requireGameScreen = true, bool requireControllingCharacter = true)
		{
			if (HintManager.HintIdentifiers == null)
			{
				return false;
			}
			if (GameSettings.CurrentConfig.DisableInGameHints)
			{
				return false;
			}
			if (HintManager.ActiveHintMessageBox != null)
			{
				return false;
			}
			if (requireControllingCharacter && Character.Controlled == null)
			{
				return false;
			}
			GameSession gameSession = GameMain.GameSession;
			GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
			return (gameMode is CampaignMode || gameMode is MissionMode) && !ObjectiveManager.AnyObjectives && (!requireGameScreen || Screen.Selected == GameMain.GameScreen);
		}

		// Token: 0x0600102E RID: 4142 RVA: 0x0009AAD4 File Offset: 0x00098CD4
		private static bool HasValidJob(string jobIdentifier)
		{
			if (GameMain.GameSession.GameMode.IsSinglePlayer)
			{
				return true;
			}
			if (Character.Controlled.HasJob(jobIdentifier))
			{
				return true;
			}
			foreach (Character c in GameMain.GameSession.CrewManager.GetCharacters())
			{
				if (c != null && c.IsRemotePlayer && !c.IsUnconscious && !c.IsDead && !c.Removed && c.HasJob(jobIdentifier))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001030 RID: 4144 RVA: 0x0009ABA4 File Offset: 0x00098DA4
		[CompilerGenerated]
		internal static void <Init>g__GetHintsRecursive|47_0(XElement element, Identifier identifier)
		{
			if (!element.HasElements)
			{
				HintManager.HintIdentifiers.Add(identifier);
				Identifier[] tags = element.GetAttributeIdentifierArray("tags", null, true);
				if (tags != null)
				{
					HintManager.HintTags.TryAdd(identifier, tags.ToHashSet<Identifier>());
				}
				Identifier orderIdentifier = element.GetAttributeIdentifier("order", Identifier.Empty);
				if (orderIdentifier != Identifier.Empty)
				{
					Identifier orderOption = element.GetAttributeIdentifier("orderoption", Identifier.Empty);
					HintManager.HintOrders.Add(identifier, new ValueTuple<Identifier, Identifier>(orderIdentifier, orderOption));
				}
				return;
			}
			if (element.Name.ToString().Equals("reminder"))
			{
				HintManager.TimeBeforeReminders = element.GetAttributeInt("timebeforereminders", HintManager.TimeBeforeReminders);
				HintManager.ReminderCooldown = element.GetAttributeInt("remindercooldown", HintManager.ReminderCooldown);
			}
			foreach (XElement childElement in element.Elements())
			{
				XElement element2 = childElement;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<XName>(childElement.Name);
				HintManager.<Init>g__GetHintsRecursive|47_0(element2, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
			}
		}

		// Token: 0x06001031 RID: 4145 RVA: 0x0009ACE4 File Offset: 0x00098EE4
		[CompilerGenerated]
		internal static IEnumerable<CoroutineStatus> <OnRoundStarted>g__InitRound|55_0()
		{
			return new HintManager.<<OnRoundStarted>g__InitRound|55_0>d(-2);
		}

		// Token: 0x06001032 RID: 4146 RVA: 0x0009ACFC File Offset: 0x00098EFC
		[CompilerGenerated]
		internal static IEnumerable<CoroutineStatus> <OnRoundStarted>g__DisplayRoundStartedHints|55_1(CoroutineHandle initRoundHandle)
		{
			HintManager.<<OnRoundStarted>g__DisplayRoundStartedHints|55_1>d <<OnRoundStarted>g__DisplayRoundStartedHints|55_1>d = new HintManager.<<OnRoundStarted>g__DisplayRoundStartedHints|55_1>d(-2);
			<<OnRoundStarted>g__DisplayRoundStartedHints|55_1>d.<>3__initRoundHandle = initRoundHandle;
			return <<OnRoundStarted>g__DisplayRoundStartedHints|55_1>d;
		}

		// Token: 0x06001033 RID: 4147 RVA: 0x0009AD19 File Offset: 0x00098F19
		[CompilerGenerated]
		internal static bool <CheckHulls>g__IsWearingDivingSuit|85_1()
		{
			return Character.Controlled.GetEquippedItem(Tags.HeavyDivingGear, new InvSlotType?(InvSlotType.OuterClothes)) != null;
		}

		// Token: 0x06001034 RID: 4148 RVA: 0x0009AD34 File Offset: 0x00098F34
		[CompilerGenerated]
		internal static bool <CheckHulls>g__IsOnFriendlySub|85_0()
		{
			Submarine sub = Character.Controlled.Submarine;
			return sub != null && (sub.TeamID == Character.Controlled.TeamID || sub.TeamID == CharacterTeamType.FriendlyNPC);
		}

		// Token: 0x040007F3 RID: 2035
		private const string HintManagerFile = "hintmanager.xml";
	}
}
