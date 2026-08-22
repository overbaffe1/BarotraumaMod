using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using FluentResults;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003EA RID: 1002
	[HarmonyPatch]
	internal class HarmonyEventPatchesService : ISystem, IReusableService, IService, IDisposable
	{
		// Token: 0x17000FA7 RID: 4007
		// (get) Token: 0x06003991 RID: 14737 RVA: 0x001806E2 File Offset: 0x0017E8E2
		// (set) Token: 0x06003992 RID: 14738 RVA: 0x001806EA File Offset: 0x0017E8EA
		public bool IsDisposed { get; private set; }

		// Token: 0x06003993 RID: 14739 RVA: 0x001806F3 File Offset: 0x0017E8F3
		public Result Reset()
		{
			this.Unpatch();
			this.Patch();
			return Result.Ok();
		}

		// Token: 0x06003994 RID: 14740 RVA: 0x00180706 File Offset: 0x0017E906
		public HarmonyEventPatchesService(IEventService eventService, ILoggerService loggerService)
		{
			HarmonyEventPatchesService._eventService = eventService;
			HarmonyEventPatchesService._loggerService = loggerService;
			this.Harmony = new Harmony("LuaCsForBarotrauma.Events");
			this.Patch();
		}

		// Token: 0x06003995 RID: 14741 RVA: 0x00180730 File Offset: 0x0017E930
		private void Patch()
		{
			Harmony harmony = this.Harmony;
			if (harmony != null)
			{
				harmony.PatchAll(typeof(HarmonyEventPatchesService));
			}
			Harmony harmony2 = this.Harmony;
			if (harmony2 == null)
			{
				return;
			}
			harmony2.PatchAll(typeof(HarmonyEventPatchesService.Patch_StartGame_End));
		}

		// Token: 0x06003996 RID: 14742 RVA: 0x00180767 File Offset: 0x0017E967
		private void Unpatch()
		{
			Harmony harmony = this.Harmony;
			if (harmony == null)
			{
				return;
			}
			harmony.UnpatchSelf();
		}

		// Token: 0x06003997 RID: 14743 RVA: 0x00180779 File Offset: 0x0017E979
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

		// Token: 0x06003998 RID: 14744 RVA: 0x001807AF File Offset: 0x0017E9AF
		[HarmonyPatch(typeof(GameSession), "EndRound")]
		[HarmonyPrefix]
		public static void GameSession_EndRound_Pre()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventRoundEnded>(delegate(IEventRoundEnded x)
			{
				x.OnRoundEnd();
			});
		}

		// Token: 0x06003999 RID: 14745 RVA: 0x001807DB File Offset: 0x0017E9DB
		[HarmonyPatch(typeof(GameSession), "LoadPreviousSave")]
		[HarmonyPrefix]
		public static void GameSession_LoadPreviousSave_Pre()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventRoundEnded>(delegate(IEventRoundEnded x)
			{
				x.OnRoundEnd();
			});
		}

		// Token: 0x0600399A RID: 14746 RVA: 0x00180808 File Offset: 0x0017EA08
		[HarmonyPatch(typeof(GameSession), "EndMissions")]
		[HarmonyPostfix]
		public static void GameSession_EndMission_Post(GameSession __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventMissionsEnded>(delegate(IEventMissionsEnded x)
			{
				x.OnMissionsEnded(__instance.Missions.ToList<Mission>());
			});
		}

		// Token: 0x0600399B RID: 14747 RVA: 0x0018083C File Offset: 0x0017EA3C
		[HarmonyPatch(typeof(Screen), "Select")]
		[HarmonyPostfix]
		public static void Screen_Selected_Post(Screen __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventScreenSelected>(delegate(IEventScreenSelected x)
			{
				x.OnScreenSelected(__instance);
			});
		}

		// Token: 0x0600399C RID: 14748 RVA: 0x0018086D File Offset: 0x0017EA6D
		[HarmonyPatch(typeof(ContentPackageManager.PackageSource), "Refresh")]
		[HarmonyPostfix]
		public static void PackageSource_Refresh_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventAllPackageListChanged>(delegate(IEventAllPackageListChanged x)
			{
				x.OnAllPackageListChanged(ContentPackageManager.CorePackages, ContentPackageManager.RegularPackages);
			});
		}

		// Token: 0x0600399D RID: 14749 RVA: 0x0018089C File Offset: 0x0017EA9C
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

		// Token: 0x0600399E RID: 14750 RVA: 0x001808FD File Offset: 0x0017EAFD
		[HarmonyPatch(typeof(ContentPackageManager.EnabledPackages), "SetCore")]
		[HarmonyPostfix]
		public static void EnabledPackages_SetCore_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventEnabledPackageListChanged>(delegate(IEventEnabledPackageListChanged sub)
			{
				sub.OnEnabledPackageListChanged(ContentPackageManager.EnabledPackages.Core, ContentPackageManager.EnabledPackages.Regular);
			});
		}

		// Token: 0x0600399F RID: 14751 RVA: 0x00180929 File Offset: 0x0017EB29
		[HarmonyPatch(typeof(ContentPackageManager.EnabledPackages), "SetRegular")]
		[HarmonyPostfix]
		public static void EnabledPackages_SetRegular_Post()
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventEnabledPackageListChanged>(delegate(IEventEnabledPackageListChanged sub)
			{
				sub.OnEnabledPackageListChanged(ContentPackageManager.EnabledPackages.Core, ContentPackageManager.EnabledPackages.Regular);
			});
		}

		// Token: 0x060039A0 RID: 14752 RVA: 0x00180958 File Offset: 0x0017EB58
		[HarmonyPatch(typeof(GameServer), "ReadDataMessage")]
		[HarmonyPrefix]
		public static bool GameServer_ReadDataMessage_Pre(NetworkConnection sender, IReadMessage inc)
		{
			int prevBitPosition = inc.BitPosition;
			ClientPacketHeader header = (ClientPacketHeader)inc.ReadByte();
			bool? skip = null;
			HarmonyEventPatchesService._eventService.PublishEvent<IEventClientRawNetMessageReceived>(delegate(IEventClientRawNetMessageReceived x)
			{
				bool? flag = x.OnReceivedClientNetMessage(inc, header, sender);
				skip = ((flag != null) ? flag : skip);
			});
			if (skip.GetValueOrDefault())
			{
				return false;
			}
			inc.BitPosition = prevBitPosition;
			return true;
		}

		// Token: 0x060039A1 RID: 14753 RVA: 0x001809D8 File Offset: 0x0017EBD8
		[HarmonyPatch(typeof(GameServer), "OnInitializationComplete")]
		[HarmonyPostfix]
		public static void GameServer_OnInitializationComplete_Post(GameServer __instance)
		{
			Client client = __instance.ConnectedClients.LastOrDefault<Client>();
			if (client == null)
			{
				return;
			}
			HarmonyEventPatchesService._eventService.PublishEvent<IEventClientConnected>(delegate(IEventClientConnected x)
			{
				x.OnClientConnected(client);
			});
		}

		// Token: 0x060039A2 RID: 14754 RVA: 0x00180A1C File Offset: 0x0017EC1C
		[HarmonyPatch(typeof(GameServer), "DisconnectClient", new Type[]
		{
			typeof(Client),
			typeof(PeerDisconnectPacket)
		})]
		[HarmonyPrefix]
		public static void GameServer_DisconnectClient_Pre(Client client, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (client == null)
			{
				return;
			}
			HarmonyEventPatchesService._eventService.PublishEvent<IEventClientDisconnected>(delegate(IEventClientDisconnected x)
			{
				x.OnClientDisconnected(client);
			});
		}

		// Token: 0x060039A3 RID: 14755 RVA: 0x00180A58 File Offset: 0x0017EC58
		[HarmonyPatch(typeof(GameServer), "AssignJobs")]
		[HarmonyPostfix]
		public static void GameServer_AssignJobs_Post(List<Client> unassigned)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventJobsAssigned>(delegate(IEventJobsAssigned x)
			{
				x.OnJobsAssigned(unassigned);
			});
		}

		// Token: 0x060039A4 RID: 14756 RVA: 0x00180A8C File Offset: 0x0017EC8C
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

		// Token: 0x060039A5 RID: 14757 RVA: 0x00180AC0 File Offset: 0x0017ECC0
		[HarmonyPatch(typeof(Character), "KillProjSpecific")]
		[HarmonyPostfix]
		public static void Character_Kill_Post(Character __instance, Affliction causeOfDeathAffliction, CauseOfDeathType causeOfDeath)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventCharacterDeath>(delegate(IEventCharacterDeath x)
			{
				x.OnCharacterDeath(__instance, causeOfDeathAffliction, causeOfDeath);
			});
		}

		// Token: 0x060039A6 RID: 14758 RVA: 0x00180B00 File Offset: 0x0017ED00
		[HarmonyPatch(typeof(Character), "GiveJobItems")]
		[HarmonyPostfix]
		public static void Character_GiveJobItems_Post(Character __instance, WayPoint spawnPoint, bool isPvPMode)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventGiveCharacterJobItems>(delegate(IEventGiveCharacterJobItems x)
			{
				x.OnGiveCharacterJobItems(__instance, spawnPoint, isPvPMode);
			});
		}

		// Token: 0x060039A7 RID: 14759 RVA: 0x00180B40 File Offset: 0x0017ED40
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

		// Token: 0x060039A8 RID: 14760 RVA: 0x00180BF0 File Offset: 0x0017EDF0
		[HarmonyPatch(typeof(Affliction), "Update")]
		[HarmonyPostfix]
		public static void Affliction_Update_Post(Affliction __instance, CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventAfflictionUpdate>(delegate(IEventAfflictionUpdate x)
			{
				x.OnAfflictionUpdate(__instance, characterHealth, targetLimb, deltaTime);
			});
		}

		// Token: 0x060039A9 RID: 14761 RVA: 0x00180C38 File Offset: 0x0017EE38
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

		// Token: 0x060039AA RID: 14762 RVA: 0x00180DF4 File Offset: 0x0017EFF4
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

		// Token: 0x060039AB RID: 14763 RVA: 0x00180E28 File Offset: 0x0017F028
		[HarmonyPatch(typeof(Item), "Remove")]
		[HarmonyPostfix]
		public static void Item_Remove_Post(Item __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventItemRemoved>(delegate(IEventItemRemoved x)
			{
				x.OnItemRemoved(__instance);
			});
		}

		// Token: 0x060039AC RID: 14764 RVA: 0x00180E5C File Offset: 0x0017F05C
		[HarmonyPatch(typeof(Item), "Remove")]
		[HarmonyPostfix]
		public static void Item_ShallowRemove_Post(Item __instance)
		{
			HarmonyEventPatchesService._eventService.PublishEvent<IEventItemRemoved>(delegate(IEventItemRemoved x)
			{
				x.OnItemRemoved(__instance);
			});
		}

		// Token: 0x060039AD RID: 14765 RVA: 0x00180E90 File Offset: 0x0017F090
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

		// Token: 0x060039AE RID: 14766 RVA: 0x00180F2C File Offset: 0x0017F12C
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

		// Token: 0x060039AF RID: 14767 RVA: 0x00180F94 File Offset: 0x0017F194
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

		// Token: 0x060039B0 RID: 14768 RVA: 0x00181000 File Offset: 0x0017F200
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

		// Token: 0x060039B1 RID: 14769 RVA: 0x00181087 File Offset: 0x0017F287
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

		// Token: 0x04001CE7 RID: 7399
		private static IEventService _eventService;

		// Token: 0x04001CE8 RID: 7400
		private static ILoggerService _loggerService;

		// Token: 0x04001CE9 RID: 7401
		private readonly Harmony Harmony;

		// Token: 0x02000CA0 RID: 3232
		[HarmonyPatch]
		private class Patch_StartGame_End
		{
			// Token: 0x060064E5 RID: 25829 RVA: 0x002164E4 File Offset: 0x002146E4
			private static MethodBase TargetMethod()
			{
				MethodInfo original = AccessTools.Method(typeof(GameServer), "StartGame", null, null);
				return AccessTools.EnumeratorMoveNext(original);
			}

			// Token: 0x060064E6 RID: 25830 RVA: 0x00216510 File Offset: 0x00214710
			[HarmonyPostfix]
			private static void Postfix(object __instance, bool __result)
			{
				if (!__result)
				{
					return;
				}
				IEnumerator<CoroutineStatus> enumerator = __instance as IEnumerator<CoroutineStatus>;
				if (enumerator == null)
				{
					return;
				}
				if (enumerator.Current == CoroutineStatus.Success)
				{
					HarmonyEventPatchesService._eventService.PublishEvent<IEventRoundStarted>(delegate(IEventRoundStarted x)
					{
						x.OnRoundStart();
					});
				}
			}
		}
	}
}
