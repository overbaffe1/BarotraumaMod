using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Barotrauma.Steam;
using FluentResults;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000500 RID: 1280
	[HarmonyPatch]
	internal class HarmonyEventPatchesService : ISystem, IReusableService, IService, IDisposable
	{
		// Token: 0x170014F0 RID: 5360
		// (get) Token: 0x060052D0 RID: 21200 RVA: 0x002C55FA File Offset: 0x002C37FA
		// (set) Token: 0x060052D1 RID: 21201 RVA: 0x002C5602 File Offset: 0x002C3802
		public bool IsDisposed { get; private set; }

		// Token: 0x060052D2 RID: 21202 RVA: 0x002C560B File Offset: 0x002C380B
		public Result Reset()
		{
			this.Unpatch();
			this.Patch();
			return Result.Ok();
		}

		// Token: 0x060052D3 RID: 21203 RVA: 0x002C561E File Offset: 0x002C381E
		public HarmonyEventPatchesService(IEventService eventService, ILoggerService loggerService)
		{
			HarmonyEventPatchesService._eventService = eventService;
			HarmonyEventPatchesService._loggerService = loggerService;
			this.Harmony = new Harmony("LuaCsForBarotrauma.Events");
			this.Patch();
		}

		// Token: 0x060052D4 RID: 21204 RVA: 0x002C5648 File Offset: 0x002C3848
		private void Patch()
		{
			Harmony harmony = this.Harmony;
			if (harmony == null)
			{
				return;
			}
			harmony.PatchAll(typeof(HarmonyEventPatchesService));
		}

		// Token: 0x060052D5 RID: 21205 RVA: 0x002C5664 File Offset: 0x002C3864
		private void Unpatch()
		{
			Harmony harmony = this.Harmony;
			if (harmony == null)
			{
				return;
			}
			harmony.UnpatchSelf();
		}

		// Token: 0x060052D6 RID: 21206 RVA: 0x002C5676 File Offset: 0x002C3876
		[HarmonyPatch(typeof(CoroutineManager), "Update")]
		[HarmonyPostfix]
		public static void CoroutineManager_Update_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventUpdate>(delegate(IEventUpdate x)
			{
				x.OnUpdate((double)CoroutineManager.DeltaTime);
			});
			HarmonyEventPatchesService._loggerService.ProcessLogs();
		}

		// Token: 0x060052D7 RID: 21207 RVA: 0x002C56AC File Offset: 0x002C38AC
		[HarmonyPatch(typeof(GameSession), "StartRound", new Type[]
		{
			typeof(LevelData),
			typeof(bool),
			typeof(SubmarineInfo),
			typeof(SubmarineInfo)
		})]
		[HarmonyPostfix]
		public static void GameSession_StartRound_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventRoundStarted>(delegate(IEventRoundStarted x)
			{
				x.OnRoundStart();
			});
		}

		// Token: 0x060052D8 RID: 21208 RVA: 0x002C56D8 File Offset: 0x002C38D8
		[HarmonyPatch(typeof(GameSession), "EndRound")]
		[HarmonyPrefix]
		public static void GameSession_EndRound_Pre()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventRoundEnded>(delegate(IEventRoundEnded x)
			{
				x.OnRoundEnd();
			});
		}

		// Token: 0x060052D9 RID: 21209 RVA: 0x002C5704 File Offset: 0x002C3904
		[HarmonyPatch(typeof(GameSession), "LoadPreviousSave")]
		[HarmonyPrefix]
		public static void GameSession_LoadPreviousSave_Pre()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventRoundEnded>(delegate(IEventRoundEnded x)
			{
				x.OnRoundEnd();
			});
		}

		// Token: 0x060052DA RID: 21210 RVA: 0x002C5730 File Offset: 0x002C3930
		[HarmonyPatch(typeof(GameSession), "EndMissions")]
		[HarmonyPostfix]
		public static void GameSession_EndMission_Post(GameSession __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventMissionsEnded>(delegate(IEventMissionsEnded x)
			{
				x.OnMissionsEnded(__instance.Missions.ToList<Mission>());
			});
		}

		// Token: 0x060052DB RID: 21211 RVA: 0x002C5764 File Offset: 0x002C3964
		[HarmonyPatch(typeof(Screen), "Select")]
		[HarmonyPostfix]
		public static void Screen_Selected_Post(Screen __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventScreenSelected>(delegate(IEventScreenSelected x)
			{
				x.OnScreenSelected(__instance);
			});
		}

		// Token: 0x060052DC RID: 21212 RVA: 0x002C5795 File Offset: 0x002C3995
		[HarmonyPatch(typeof(MainMenuScreen), "StartGame")]
		[HarmonyPostfix]
		public static void MainMenuScreen_StartGame_Pre(Screen __instance)
		{
			LuaCsSetup.Instance.SetRunState(RunState.Running);
		}

		// Token: 0x060052DD RID: 21213 RVA: 0x002C57A2 File Offset: 0x002C39A2
		[HarmonyPatch(typeof(MainMenuScreen), "LoadGame")]
		[HarmonyPostfix]
		public static void MainMenuScreen_LoadGame_Pre(Screen __instance)
		{
			LuaCsSetup.Instance.SetRunState(RunState.Running);
		}

		// Token: 0x060052DE RID: 21214 RVA: 0x002C57AF File Offset: 0x002C39AF
		[HarmonyPatch(typeof(MutableWorkshopMenu), "Apply")]
		[HarmonyPostfix]
		public static void MutableWorkshopMenu_Apply_Post(Screen __instance)
		{
			LuaCsSetup.Instance.PromptCSharpMods(delegate(bool selection)
			{
			}, false);
		}

		// Token: 0x060052DF RID: 21215 RVA: 0x002C57DB File Offset: 0x002C39DB
		[HarmonyPatch(typeof(ContentPackageManager.PackageSource), "Refresh")]
		[HarmonyPostfix]
		public static void PackageSource_Refresh_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventAllPackageListChanged>(delegate(IEventAllPackageListChanged x)
			{
				x.OnAllPackageListChanged(ContentPackageManager.CorePackages, ContentPackageManager.RegularPackages);
			});
		}

		// Token: 0x060052E0 RID: 21216 RVA: 0x002C5808 File Offset: 0x002C3A08
		[HarmonyPatch(typeof(ContentPackageManager), "Init")]
		[HarmonyPostfix]
		public static void ContentPackageManager_Init_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventAllPackageListChanged>(delegate(IEventAllPackageListChanged x)
			{
				x.OnAllPackageListChanged(ContentPackageManager.CorePackages, ContentPackageManager.RegularPackages);
			});
			HarmonyEventPatchesService._eventService.PublishEvent<IEventEnabledPackageListChanged>(delegate(IEventEnabledPackageListChanged sub)
			{
				sub.OnEnabledPackageListChanged(ContentPackageManager.EnabledPackages.Core, ContentPackageManager.EnabledPackages.Regular);
			});
		}

		// Token: 0x060052E1 RID: 21217 RVA: 0x002C5869 File Offset: 0x002C3A69
		[HarmonyPatch(typeof(ContentPackageManager.EnabledPackages), "SetCore")]
		[HarmonyPostfix]
		public static void EnabledPackages_SetCore_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventEnabledPackageListChanged>(delegate(IEventEnabledPackageListChanged sub)
			{
				sub.OnEnabledPackageListChanged(ContentPackageManager.EnabledPackages.Core, ContentPackageManager.EnabledPackages.Regular);
			});
		}

		// Token: 0x060052E2 RID: 21218 RVA: 0x002C5895 File Offset: 0x002C3A95
		[HarmonyPatch(typeof(ContentPackageManager.EnabledPackages), "SetRegular")]
		[HarmonyPostfix]
		public static void EnabledPackages_SetRegular_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventEnabledPackageListChanged>(delegate(IEventEnabledPackageListChanged sub)
			{
				sub.OnEnabledPackageListChanged(ContentPackageManager.EnabledPackages.Core, ContentPackageManager.EnabledPackages.Regular);
			});
		}

		// Token: 0x060052E3 RID: 21219 RVA: 0x002C58C4 File Offset: 0x002C3AC4
		[HarmonyPatch(typeof(GameClient), "ReadDataMessage")]
		[HarmonyPrefix]
		public static bool GameClient_ReadDataMessage_Pre(IReadMessage inc)
		{
			int prevBitPosition = inc.BitPosition;
			ServerPacketHeader header = (ServerPacketHeader)inc.ReadByte();
			bool? skip = null;
			HarmonyEventPatchesService._eventService.PublishEvent<IEventServerRawNetMessageReceived>(delegate(IEventServerRawNetMessageReceived x)
			{
				bool? flag = x.OnReceivedServerNetMessage(inc, header);
				skip = ((flag != null) ? flag : skip);
			});
			if (skip.GetValueOrDefault())
			{
				return false;
			}
			inc.BitPosition = prevBitPosition;
			return true;
		}

		// Token: 0x060052E4 RID: 21220 RVA: 0x002C593C File Offset: 0x002C3B3C
		[HarmonyPatch(typeof(SubEditorScreen), "Select", new Type[]
		{

		})]
		[HarmonyPostfix]
		public static void SubEditorScreen_Selected_Post(Screen __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventScreenSelected>(delegate(IEventScreenSelected x)
			{
				x.OnScreenSelected(__instance);
			});
		}

		// Token: 0x060052E5 RID: 21221 RVA: 0x002C5970 File Offset: 0x002C3B70
		[HarmonyPatch(typeof(PlayerInput), "Update")]
		[HarmonyPrefix]
		public static void PlayerInput_Update_Pre(double deltaTime)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventKeyUpdate>(delegate(IEventKeyUpdate x)
			{
				x.OnKeyUpdate(deltaTime);
			});
		}

		// Token: 0x060052E6 RID: 21222 RVA: 0x002C59A4 File Offset: 0x002C3BA4
		[HarmonyPatch(typeof(DebugConsole), "IsCommandPermitted")]
		[HarmonyPrefix]
		public static bool DebugConsole_IsCommandPermitted(Identifier command, ref bool __result)
		{
			DebugConsole.Command c = DebugConsole.FindCommand(command.Value);
			if (DebugConsole.Commands.IndexOf(c) >= LuaCsSetup.DebugConsoleCommandVanillaIndex)
			{
				__result = true;
				return false;
			}
			return true;
		}

		// Token: 0x060052E7 RID: 21223 RVA: 0x002C59D8 File Offset: 0x002C3BD8
		[HarmonyPatch(typeof(Character), "Create", new Type[]
		{
			typeof(CharacterPrefab),
			typeof(Vector2),
			typeof(string),
			typeof(CharacterInfo),
			typeof(ushort),
			typeof(bool),
			typeof(bool),
			typeof(bool),
			typeof(RagdollParams),
			typeof(bool)
		})]
		[HarmonyPostfix]
		public static void Character_Create_Post(Character __result)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventCharacterCreated>(delegate(IEventCharacterCreated x)
			{
				x.OnCharacterCreated(__result);
			});
		}

		// Token: 0x060052E8 RID: 21224 RVA: 0x002C5A0C File Offset: 0x002C3C0C
		[HarmonyPatch(typeof(Character), "KillProjSpecific")]
		[HarmonyPostfix]
		public static void Character_Kill_Post(Character __instance, Affliction causeOfDeathAffliction, CauseOfDeathType causeOfDeath)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventCharacterDeath>(delegate(IEventCharacterDeath x)
			{
				x.OnCharacterDeath(__instance, causeOfDeathAffliction, causeOfDeath);
			});
		}

		// Token: 0x060052E9 RID: 21225 RVA: 0x002C5A4C File Offset: 0x002C3C4C
		[HarmonyPatch(typeof(Character), "GiveJobItems")]
		[HarmonyPostfix]
		public static void Character_GiveJobItems_Post(Character __instance, WayPoint spawnPoint, bool isPvPMode)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventGiveCharacterJobItems>(delegate(IEventGiveCharacterJobItems x)
			{
				x.OnGiveCharacterJobItems(__instance, spawnPoint, isPvPMode);
			});
		}

		// Token: 0x060052EA RID: 21226 RVA: 0x002C5A8C File Offset: 0x002C3C8C
		[HarmonyPatch(typeof(Character), "DamageLimb")]
		[HarmonyPrefix]
		public static bool Character_DamageLimb_Pre(AttackResult __result, Character __instance, Vector2 worldPosition, Limb hitLimb, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, Character attacker, float damageMultiplier, bool allowStacking, float penetration, bool shouldImplode, bool ignoreDamageOverlay, bool recalculateVitality)
		{
			AttackResult? result = null;
			HarmonyEventPatchesService._eventService.PublishEvent<IEventCharacterDamageLimb>(delegate(IEventCharacterDamageLimb x)
			{
				result = x.OnCharacterDamageLimb(__instance, worldPosition, hitLimb, afflictions, stun, playSound, attackImpulse, attacker, damageMultiplier, allowStacking, penetration, shouldImplode);
			});
			if (result != null)
			{
				__result = result.Value;
				return false;
			}
			return true;
		}

		// Token: 0x060052EB RID: 21227 RVA: 0x002C5B3C File Offset: 0x002C3D3C
		[HarmonyPatch(typeof(Affliction), "Update")]
		[HarmonyPostfix]
		public static void Affliction_Update_Post(Affliction __instance, CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventAfflictionUpdate>(delegate(IEventAfflictionUpdate x)
			{
				x.OnAfflictionUpdate(__instance, characterHealth, targetLimb, deltaTime);
			});
		}

		// Token: 0x060052EC RID: 21228 RVA: 0x002C5B84 File Offset: 0x002C3D84
		[HarmonyPatch(typeof(Connection), "SendSignal")]
		[HarmonyPostfix]
		public static void Connection_SendSignal_Post(Connection __instance, Signal signal)
		{
			foreach (Wire wire in __instance.Wires)
			{
				Connection recipient = wire.OtherConnection(__instance);
				if (recipient != null)
				{
					HarmonyEventPatchesService._eventService.PublishEvent<IEventSignalReceived>(delegate(IEventSignalReceived x)
					{
						x.OnSignalReceived(signal, recipient);
					});
					HarmonyEventPatchesService._eventService.Call("signalReceived." + recipient.Item.Prefab.Identifier.ToString(), new object[]
					{
						signal,
						recipient
					});
				}
			}
			using (List<CircuitBoxConnection>.Enumerator enumerator2 = __instance.CircuitBoxConnections.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					CircuitBoxConnection connection = enumerator2.Current;
					HarmonyEventPatchesService._eventService.PublishEvent<IEventSignalReceived>(delegate(IEventSignalReceived x)
					{
						x.OnSignalReceived(signal, connection.Connection);
					});
					HarmonyEventPatchesService._eventService.Call("signalReceived." + connection.Connection.Item.Prefab.Identifier.ToString(), new object[]
					{
						signal,
						connection.Connection
					});
				}
			}
		}

		// Token: 0x060052ED RID: 21229 RVA: 0x002C5D40 File Offset: 0x002C3F40
		[HarmonyPatch(typeof(Item), MethodType.Constructor, new Type[]
		{
			typeof(Rectangle),
			typeof(ItemPrefab),
			typeof(Submarine),
			typeof(bool),
			typeof(ushort)
		})]
		[HarmonyPostfix]
		public static void Item_Ctor_Post(Item __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventItemCreated>(delegate(IEventItemCreated x)
			{
				x.OnItemCreated(__instance);
			});
		}

		// Token: 0x060052EE RID: 21230 RVA: 0x002C5D74 File Offset: 0x002C3F74
		[HarmonyPatch(typeof(Item), "Remove")]
		[HarmonyPostfix]
		public static void Item_Remove_Post(Item __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventItemRemoved>(delegate(IEventItemRemoved x)
			{
				x.OnItemRemoved(__instance);
			});
		}

		// Token: 0x060052EF RID: 21231 RVA: 0x002C5DA8 File Offset: 0x002C3FA8
		[HarmonyPatch(typeof(Item), "Remove")]
		[HarmonyPostfix]
		public static void Item_ShallowRemove_Post(Item __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventItemRemoved>(delegate(IEventItemRemoved x)
			{
				x.OnItemRemoved(__instance);
			});
		}

		// Token: 0x060052F0 RID: 21232 RVA: 0x002C5DDC File Offset: 0x002C3FDC
		[HarmonyPatch(typeof(Item), "Use")]
		[HarmonyPrefix]
		public static bool Item_Use_Pre(Item __instance, Character user, Limb targetLimb, Entity useTarget)
		{
			if (__instance.RequireAimToUse && (user == null || !user.IsKeyDown(InputType.Aim)))
			{
				return true;
			}
			if (__instance.Condition <= 0f)
			{
				return true;
			}
			bool? result = null;
			HarmonyEventPatchesService._eventService.PublishEvent<IEventItemUse>(delegate(IEventItemUse x)
			{
				result = x.OnItemUsed(__instance, user, targetLimb, useTarget);
			});
			return !result.GetValueOrDefault();
		}

		// Token: 0x060052F1 RID: 21233 RVA: 0x002C5E78 File Offset: 0x002C4078
		[HarmonyPatch(typeof(Item), "SecondaryUse")]
		[HarmonyPrefix]
		public static bool Item_SecondaryUse_Pre(Item __instance, Character character)
		{
			if (__instance.Condition <= 0f)
			{
				return true;
			}
			bool? result = null;
			HarmonyEventPatchesService._eventService.PublishEvent<IEventItemSecondaryUse>(delegate(IEventItemSecondaryUse x)
			{
				result = x.OnItemSecondaryUsed(__instance, character);
			});
			return !result.GetValueOrDefault();
		}

		// Token: 0x060052F2 RID: 21234 RVA: 0x002C5EE0 File Offset: 0x002C40E0
		[HarmonyPatch(typeof(Inventory), "PutItem")]
		[HarmonyPrefix]
		public static bool Inventory_PutItem_Prefix(Inventory __instance, Item item, int i, Character user, bool removeItem)
		{
			bool? result = null;
			HarmonyEventPatchesService._eventService.PublishEvent<IEventInventoryPutItem>(delegate(IEventInventoryPutItem x)
			{
				result = x.OnInventoryPutItem(__instance, item, user, i, removeItem);
			});
			return !result.GetValueOrDefault();
		}

		// Token: 0x060052F3 RID: 21235 RVA: 0x002C5F4C File Offset: 0x002C414C
		[HarmonyPatch(typeof(Inventory), "TrySwapping")]
		[HarmonyPrefix]
		public static bool Inventory_TrySwapping_Prefix(Inventory __instance, Item item, int index, Character user, bool swapWholeStack, ref bool __result)
		{
			if (!__instance.AllowSwappingContainedItems)
			{
				return false;
			}
			bool? result = null;
			HarmonyEventPatchesService._eventService.PublishEvent<IEventInventoryItemSwap>(delegate(IEventInventoryItemSwap x)
			{
				result = x.OnInventoryItemSwap(__instance, item, user, index, swapWholeStack);
			});
			if (result != null)
			{
				__result = result.Value;
				return false;
			}
			return true;
		}

		// Token: 0x060052F4 RID: 21236 RVA: 0x002C5FD3 File Offset: 0x002C41D3
		public void Dispose()
		{
			this.IsDisposed = true;
			Harmony harmony = this.Harmony;
			if (harmony == null)
			{
				return;
			}
			harmony.UnpatchSelf();
		}

		// Token: 0x04002BD3 RID: 11219
		private static IEventService _eventService;

		// Token: 0x04002BD4 RID: 11220
		private static ILoggerService _loggerService;

		// Token: 0x04002BD5 RID: 11221
		private readonly Harmony Harmony;
	}
}
