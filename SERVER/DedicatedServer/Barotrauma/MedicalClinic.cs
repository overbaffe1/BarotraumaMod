using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000035 RID: 53
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class MedicalClinic
	{
		// Token: 0x0600069D RID: 1693 RVA: 0x0003FCA4 File Offset: 0x0003DEA4
		public void Update(float deltaTime)
		{
			this.processAfflictionChangesTimer -= deltaTime;
			if (this.processAfflictionChangesTimer <= 0f)
			{
				foreach (Character character in this.charactersWithAfflictionChanges)
				{
					ImmutableArray<MedicalClinic.NetAffliction> afflictions = this.GetAllAfflictions(character.CharacterHealth);
					foreach (MedicalClinic.AfflictionSubscriber sub in this.afflictionSubscribers.ToList<MedicalClinic.AfflictionSubscriber>())
					{
						if (sub.Expiry < DateTimeOffset.Now)
						{
							this.afflictionSubscribers.Remove(sub);
						}
						else if (sub.Target == character.Info)
						{
							this.ServerSend(new MedicalClinic.NetCrewMember(character.Info, afflictions), MedicalClinic.NetworkHeader.AFFLICTION_UPDATE, DeliveryMethod.Unreliable, sub.Subscriber, null);
						}
					}
				}
				this.charactersWithAfflictionChanges.Clear();
				this.processAfflictionChangesTimer = 1f;
			}
		}

		// Token: 0x0600069E RID: 1694 RVA: 0x0003FDCC File Offset: 0x0003DFCC
		public void ServerRead(IReadMessage inc, Client sender)
		{
			switch (inc.ReadByte())
			{
			case 0:
				this.ProcessRequestedAfflictions(inc, sender);
				return;
			case 1:
				break;
			case 2:
				this.RemoveClientSubscription(sender);
				return;
			case 3:
				this.ProcessRequestedPending(sender);
				return;
			case 4:
				this.ProcessNewAddition(inc, sender);
				return;
			case 5:
				this.ProcessNewRemoval(inc, sender);
				return;
			case 6:
				this.ProcessClearing(sender);
				break;
			case 7:
				this.ProcessHealing(sender);
				return;
			case 8:
				this.ProcessAddEverything(sender);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x0003FE50 File Offset: 0x0003E050
		private void ProcessNewAddition(IReadMessage inc, Client client)
		{
			if (this.rateLimiter.IsLimitReached(client))
			{
				return;
			}
			MedicalClinic.NetCrewMember newCrewMember = INetSerializableStruct.Read<MedicalClinic.NetCrewMember>(inc);
			this.InsertPendingCrewMember(newCrewMember);
			this.ServerSend(new NetCollection<MedicalClinic.NetCrewMember>(new MedicalClinic.NetCrewMember[]
			{
				newCrewMember
			}), MedicalClinic.NetworkHeader.ADD_PENDING, DeliveryMethod.Reliable, null, client);
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x0003FE9C File Offset: 0x0003E09C
		private void ProcessAddEverything(Client client)
		{
			if (this.rateLimiter.IsLimitReached(client))
			{
				return;
			}
			this.AddEverythingToPending();
			this.ServerSend(this.PendingHeals.ToNetCollection<MedicalClinic.NetCrewMember>(), MedicalClinic.NetworkHeader.ADD_PENDING, DeliveryMethod.Reliable, null, client);
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x0003FED0 File Offset: 0x0003E0D0
		private void RemoveClientSubscription(Client client)
		{
			foreach (MedicalClinic.AfflictionSubscriber sub in this.afflictionSubscribers.ToList<MedicalClinic.AfflictionSubscriber>())
			{
				if (sub.Subscriber == client || sub.Expiry < DateTimeOffset.Now)
				{
					this.afflictionSubscribers.Remove(sub);
				}
			}
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x0003FF4C File Offset: 0x0003E14C
		private void ProcessNewRemoval(IReadMessage inc, Client client)
		{
			if (this.rateLimiter.IsLimitReached(client))
			{
				return;
			}
			MedicalClinic.NetRemovedAffliction removed = INetSerializableStruct.Read<MedicalClinic.NetRemovedAffliction>(inc);
			this.RemovePendingAffliction(removed.CrewMember, removed.Affliction);
			this.ServerSend(removed, MedicalClinic.NetworkHeader.REMOVE_PENDING, DeliveryMethod.Reliable, null, client);
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x0003FF93 File Offset: 0x0003E193
		private void ProcessRequestedPending(Client client)
		{
			if (this.rateLimiter.IsLimitReached(client))
			{
				return;
			}
			this.ServerSend(this.PendingHeals.ToNetCollection<MedicalClinic.NetCrewMember>(), MedicalClinic.NetworkHeader.REQUEST_PENDING, DeliveryMethod.Reliable, client, null);
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x0003FFC0 File Offset: 0x0003E1C0
		private void ProcessHealing(Client client)
		{
			if (this.rateLimiter.IsLimitReached(client))
			{
				return;
			}
			MedicalClinic.HealRequestResult result = this.HealAllPending(false, client);
			this.ServerSend(new MedicalClinic.NetHealRequest
			{
				Result = result
			}, MedicalClinic.NetworkHeader.HEAL_PENDING, DeliveryMethod.Reliable, null, client);
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x00040005 File Offset: 0x0003E205
		private void ProcessClearing(Client client)
		{
			if (this.rateLimiter.IsLimitReached(client))
			{
				return;
			}
			if (!this.PendingHeals.Any<MedicalClinic.NetCrewMember>())
			{
				return;
			}
			this.ClearPendingHeals();
			this.ServerSend(null, MedicalClinic.NetworkHeader.CLEAR_PENDING, DeliveryMethod.Reliable, null, client);
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x00040038 File Offset: 0x0003E238
		private void ProcessRequestedAfflictions(IReadMessage inc, Client client)
		{
			if (this.rateLimiter.IsLimitReached(client))
			{
				return;
			}
			MedicalClinic.NetCrewMember crewMember = INetSerializableStruct.Read<MedicalClinic.NetCrewMember>(inc);
			CharacterInfo foundInfo = crewMember.FindCharacterInfo(MedicalClinic.GetCrewCharacters());
			ImmutableArray<MedicalClinic.NetAffliction> pendingAfflictions = ImmutableArray<MedicalClinic.NetAffliction>.Empty;
			int infoId = 0;
			if (foundInfo == null)
			{
				StringBuilder sb = new StringBuilder();
				foreach (CharacterInfo character in MedicalClinic.GetCrewCharacters())
				{
					StringBuilder stringBuilder = sb;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral(" - ");
					appendInterpolatedStringHandler.AppendFormatted(character.DisplayName);
					appendInterpolatedStringHandler.AppendLiteral(" (");
					appendInterpolatedStringHandler.AppendFormatted<ushort>(character.ID);
					appendInterpolatedStringHandler.AppendLiteral(")");
					stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find the requested crew member with ID ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(crewMember.CharacterInfoID);
				defaultInterpolatedStringHandler.AppendLiteral(".\n");
				defaultInterpolatedStringHandler.AppendFormatted<StringBuilder>(sb);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			if (foundInfo != null)
			{
				Character character2 = foundInfo.Character;
				if (character2 != null)
				{
					CharacterHealth health = character2.CharacterHealth;
					if (health != null)
					{
						pendingAfflictions = this.GetAllAfflictions(health);
						infoId = (int)foundInfo.ID;
					}
				}
			}
			INetSerializableStruct writeCrewMember = new MedicalClinic.NetCrewMember
			{
				CharacterInfoID = infoId,
				Afflictions = pendingAfflictions
			};
			if (foundInfo != null)
			{
				this.RemoveClientSubscription(client);
				this.afflictionSubscribers.Add(new MedicalClinic.AfflictionSubscriber(client, foundInfo, DateTimeOffset.Now.AddMinutes(1.0)));
			}
			this.ServerSend(writeCrewMember, MedicalClinic.NetworkHeader.REQUEST_AFFLICTIONS, DeliveryMethod.Unreliable, client, null);
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x000401D0 File Offset: 0x0003E3D0
		private IWriteMessage StartSending()
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(24);
			return msg;
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x000401EC File Offset: 0x0003E3EC
		[NullableContext(2)]
		private void ServerSend(INetSerializableStruct netStruct, MedicalClinic.NetworkHeader header, DeliveryMethod deliveryMethod, Client targetClient = null, Client reponseClient = null)
		{
			MedicalClinic.<>c__DisplayClass14_0 CS$<>8__locals1;
			CS$<>8__locals1.reponseClient = reponseClient;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.header = header;
			CS$<>8__locals1.netStruct = netStruct;
			CS$<>8__locals1.deliveryMethod = deliveryMethod;
			if (targetClient == null)
			{
				foreach (Client c in GameMain.Server.ConnectedClients)
				{
					this.<ServerSend>g__SendToClient|14_0(c, ref CS$<>8__locals1);
				}
				return;
			}
			this.<ServerSend>g__SendToClient|14_0(targetClient, ref CS$<>8__locals1);
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x00040278 File Offset: 0x0003E478
		public MedicalClinic(CampaignMode campaign)
		{
			this.campaign = campaign;
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x000402D8 File Offset: 0x0003E4D8
		private static bool IsOutpostInCombat()
		{
			Level loaded = Level.Loaded;
			if (loaded == null || loaded.Type != LevelData.LevelType.Outpost)
			{
				return false;
			}
			IEnumerable<Character> crew = (from c in MedicalClinic.GetCrewCharacters()
			where c.Character != null
			select c.Character).ToImmutableHashSet<Character>();
			foreach (Character npc in from c in Character.CharacterList
			where c.TeamID == CharacterTeamType.FriendlyNPC
			select c)
			{
				if (npc.IsInstigator)
				{
					goto IL_E6;
				}
				HumanAIController humanAIController = npc.AIController as HumanAIController;
				if (humanAIController == null)
				{
					goto IL_E6;
				}
				AIObjectiveManager objectiveManager = humanAIController.ObjectiveManager;
				if (objectiveManager == null)
				{
					goto IL_E6;
				}
				AIObjectiveCombat combatObjective = objectiveManager.CurrentObjective as AIObjectiveCombat;
				if (combatObjective == null)
				{
					goto IL_E6;
				}
				bool flag = crew.Contains(combatObjective.Enemy);
				IL_E7:
				bool isInCombatWithCrew = flag;
				if (isInCombatWithCrew)
				{
					return true;
				}
				continue;
				IL_E6:
				flag = false;
				goto IL_E7;
			}
			return false;
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x00040400 File Offset: 0x0003E600
		[NullableContext(2)]
		private MedicalClinic.HealRequestResult HealAllPending(bool force = false, Client client = null)
		{
			int totalCost = this.GetTotalCost();
			if (!force)
			{
				if (MedicalClinic.IsOutpostInCombat())
				{
					return MedicalClinic.HealRequestResult.Refused;
				}
				CampaignMode campaignMode = this.campaign;
				if (campaignMode == null || !campaignMode.TryPurchase(client, totalCost))
				{
					return MedicalClinic.HealRequestResult.InsufficientFunds;
				}
			}
			ImmutableArray<CharacterInfo> crew = MedicalClinic.GetCrewCharacters();
			foreach (MedicalClinic.NetCrewMember crewMember in this.PendingHeals)
			{
				CharacterInfo targetCharacter = crewMember.FindCharacterInfo(crew);
				Character character = (targetCharacter != null) ? targetCharacter.Character : null;
				if (character != null)
				{
					CharacterHealth health = character.CharacterHealth;
					if (health != null)
					{
						foreach (MedicalClinic.NetAffliction affliction in crewMember.Afflictions)
						{
							CharacterHealth characterHealth = health;
							Identifier identifier = affliction.Identifier;
							AfflictionPrefab prefab = affliction.Prefab;
							characterHealth.ReduceAfflictionOnAllLimbs(identifier, (prefab != null) ? prefab.MaxStrength : ((float)affliction.Strength), null, null);
						}
					}
				}
			}
			this.ClearPendingHeals();
			return MedicalClinic.HealRequestResult.Success;
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x0004050C File Offset: 0x0003E70C
		private void ClearPendingHeals()
		{
			this.PendingHeals.Clear();
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x0004051C File Offset: 0x0003E71C
		private void AddEverythingToPending()
		{
			foreach (CharacterInfo info in MedicalClinic.GetCrewCharacters())
			{
				Character character = info.Character;
				CharacterHealth health = (character != null) ? character.CharacterHealth : null;
				if (health != null)
				{
					ImmutableArray<MedicalClinic.NetAffliction> afflictions = this.GetAllAfflictions(health);
					if (afflictions.Length != 0)
					{
						this.InsertPendingCrewMember(new MedicalClinic.NetCrewMember(info, afflictions));
					}
				}
			}
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x00040580 File Offset: 0x0003E780
		private void RemovePendingAffliction(MedicalClinic.NetCrewMember crewMember, MedicalClinic.NetAffliction affliction)
		{
			foreach (MedicalClinic.NetCrewMember listMember in this.PendingHeals.ToList<MedicalClinic.NetCrewMember>())
			{
				this.PendingHeals.Remove(listMember);
				MedicalClinic.NetCrewMember pendingMember = listMember;
				if (pendingMember.CharacterEquals(crewMember))
				{
					List<MedicalClinic.NetAffliction> newAfflictions = new List<MedicalClinic.NetAffliction>();
					foreach (MedicalClinic.NetAffliction pendingAffliction in pendingMember.Afflictions)
					{
						if (!pendingAffliction.AfflictionEquals(affliction))
						{
							newAfflictions.Add(pendingAffliction);
						}
					}
					pendingMember.Afflictions = newAfflictions.ToImmutableArray<MedicalClinic.NetAffliction>();
				}
				if (pendingMember.Afflictions.Any<MedicalClinic.NetAffliction>())
				{
					this.PendingHeals.Add(pendingMember);
				}
			}
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x00040650 File Offset: 0x0003E850
		private void InsertPendingCrewMember(MedicalClinic.NetCrewMember crewMember)
		{
			MedicalClinic.NetCrewMember? netCrewMember = this.PendingHeals.FirstOrNull((MedicalClinic.NetCrewMember m) => m.CharacterEquals(crewMember));
			if (netCrewMember != null)
			{
				MedicalClinic.NetCrewMember foundHeal = netCrewMember.GetValueOrDefault();
				this.PendingHeals.Remove(foundHeal);
			}
			this.PendingHeals.Add(crewMember);
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x000406B1 File Offset: 0x0003E8B1
		public static bool IsHealable(Affliction affliction)
		{
			return affliction.Prefab.HealableInMedicalClinic && affliction.Strength > MedicalClinic.<IsHealable>g__GetShowTreshold|38_0(affliction);
		}

		// Token: 0x060006B1 RID: 1713 RVA: 0x000406D0 File Offset: 0x0003E8D0
		[NullableContext(0)]
		private ImmutableArray<MedicalClinic.NetAffliction> GetAllAfflictions([Nullable(1)] CharacterHealth health)
		{
			IEnumerable<Affliction> allAfflictions = health.GetAllAfflictions();
			Func<Affliction, bool> predicate;
			if ((predicate = MedicalClinic.<>O.<0>__IsHealable) == null)
			{
				predicate = (MedicalClinic.<>O.<0>__IsHealable = new Func<Affliction, bool>(MedicalClinic.IsHealable));
			}
			IEnumerable<Affliction> rawAfflictions = allAfflictions.Where(predicate);
			List<MedicalClinic.NetAffliction> afflictions = new List<MedicalClinic.NetAffliction>();
			using (IEnumerator<Affliction> enumerator = rawAfflictions.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Affliction affliction = enumerator.Current;
					MedicalClinic.NetAffliction? netAffliction2 = afflictions.FirstOrNull((MedicalClinic.NetAffliction netAffliction) => netAffliction.AfflictionEquals(affliction.Prefab));
					MedicalClinic.NetAffliction newAffliction;
					if (netAffliction2 != null)
					{
						MedicalClinic.NetAffliction foundAffliction = netAffliction2.GetValueOrDefault();
						afflictions.Remove(foundAffliction);
						foundAffliction.Strength += (ushort)affliction.Strength;
						foundAffliction.Price += (ushort)this.GetAdjustedPrice(MedicalClinic.<GetAllAfflictions>g__GetHealPrice|39_0(affliction));
						newAffliction = foundAffliction;
					}
					else
					{
						newAffliction = default(MedicalClinic.NetAffliction);
						newAffliction.SetAffliction(affliction, health);
						newAffliction.Price = (ushort)this.GetAdjustedPrice((int)newAffliction.Price);
					}
					afflictions.Add(newAffliction);
				}
			}
			return afflictions.ToImmutableArray<MedicalClinic.NetAffliction>();
		}

		// Token: 0x060006B2 RID: 1714 RVA: 0x000407F4 File Offset: 0x0003E9F4
		public static void OnAfflictionCountChanged(Character character)
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null)
			{
				return;
			}
			CampaignMode campaignMode = gameSession.Campaign;
			if (campaignMode == null)
			{
				return;
			}
			MedicalClinic medicalClinic = campaignMode.MedicalClinic;
			if (medicalClinic == null)
			{
				return;
			}
			medicalClinic.OnAfflictionCountChangedPrivate(character);
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x0004081A File Offset: 0x0003EA1A
		private void OnAfflictionCountChangedPrivate(Character character)
		{
			if (((character != null) ? character.Info : null) == null)
			{
				return;
			}
			this.charactersWithAfflictionChanges.Add(character);
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x00040838 File Offset: 0x0003EA38
		public int GetTotalCost()
		{
			return this.PendingHeals.SelectMany((MedicalClinic.NetCrewMember h) => h.Afflictions).Aggregate(0, (int current, MedicalClinic.NetAffliction affliction) => current + (int)affliction.Price);
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x00040894 File Offset: 0x0003EA94
		private int GetAdjustedPrice(int price)
		{
			CampaignMode campaignMode = this.campaign;
			Location location;
			if (campaignMode == null)
			{
				location = null;
			}
			else
			{
				Map map = campaignMode.Map;
				location = ((map != null) ? map.CurrentLocation : null);
			}
			Location currentLocation = location;
			if (currentLocation != null)
			{
				LocationType type = currentLocation.Type;
				if (type != null && type.HasOutpost)
				{
					return currentLocation.GetAdjustedHealCost(price);
				}
			}
			return int.MaxValue;
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x000408E2 File Offset: 0x0003EAE2
		public int GetBalance()
		{
			CampaignMode campaignMode = this.campaign;
			if (campaignMode == null)
			{
				return 0;
			}
			return campaignMode.GetBalance(null);
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x000408F8 File Offset: 0x0003EAF8
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static ImmutableArray<CharacterInfo> GetCrewCharacters()
		{
			return (from c in Character.CharacterList
			where c.Info != null && c.TeamID == CharacterTeamType.Team1
			select c.Info).ToImmutableArray<CharacterInfo>();
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x00040958 File Offset: 0x0003EB58
		[CompilerGenerated]
		private void <ServerSend>g__SendToClient|14_0(Client c, ref MedicalClinic.<>c__DisplayClass14_0 A_2)
		{
			MedicalClinic.MessageFlag flag = MedicalClinic.MessageFlag.Announce;
			if (A_2.reponseClient != null && A_2.reponseClient == c)
			{
				flag = MedicalClinic.MessageFlag.Response;
			}
			IWriteMessage msg = this.StartSending();
			msg.WriteByte((byte)A_2.header);
			msg.WriteByte((byte)flag);
			INetSerializableStruct netStruct = A_2.netStruct;
			if (netStruct != null)
			{
				netStruct.Write(msg);
			}
			GameMain.Server.ServerPeer.Send(msg, c.Connection, A_2.deliveryMethod, true);
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x000409C5 File Offset: 0x0003EBC5
		[CompilerGenerated]
		internal static float <IsHealable>g__GetShowTreshold|38_0(Affliction affliction)
		{
			return Math.Max(0f, Math.Min(affliction.Prefab.ShowIconToOthersThreshold, affliction.Prefab.ShowInHealthScannerThreshold));
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x000409EC File Offset: 0x0003EBEC
		[CompilerGenerated]
		internal static int <GetAllAfflictions>g__GetHealPrice|39_0(Affliction affliction)
		{
			return (int)((float)affliction.Prefab.BaseHealCost + affliction.Prefab.HealCostMultiplier * affliction.Strength);
		}

		// Token: 0x04000330 RID: 816
		private readonly RateLimiter rateLimiter = new RateLimiter(20, 5, new ValueTuple<RateLimitAction, RateLimitPunishment>[]
		{
			new ValueTuple<RateLimitAction, RateLimitPunishment>(RateLimitAction.OnLimitReached, RateLimitPunishment.Announce)
		});

		// Token: 0x04000331 RID: 817
		private readonly List<MedicalClinic.AfflictionSubscriber> afflictionSubscribers = new List<MedicalClinic.AfflictionSubscriber>();

		// Token: 0x04000332 RID: 818
		private const int RateLimitMaxRequests = 20;

		// Token: 0x04000333 RID: 819
		private const int RateLimitExpiry = 5;

		// Token: 0x04000334 RID: 820
		public readonly List<MedicalClinic.NetCrewMember> PendingHeals = new List<MedicalClinic.NetCrewMember>();

		// Token: 0x04000335 RID: 821
		[Nullable(2)]
		public Action OnUpdate;

		// Token: 0x04000336 RID: 822
		[Nullable(2)]
		private readonly CampaignMode campaign;

		// Token: 0x04000337 RID: 823
		private readonly HashSet<Character> charactersWithAfflictionChanges = new HashSet<Character>();

		// Token: 0x04000338 RID: 824
		private float processAfflictionChangesTimer;

		// Token: 0x04000339 RID: 825
		private const float ProcessAfflictionChangesInterval = 1f;

		// Token: 0x02000667 RID: 1639
		[Nullable(0)]
		private readonly struct AfflictionSubscriber : IEquatable<MedicalClinic.AfflictionSubscriber>
		{
			// Token: 0x06004E45 RID: 20037 RVA: 0x001E14BB File Offset: 0x001DF6BB
			public AfflictionSubscriber(Client Subscriber, CharacterInfo Target, DateTimeOffset Expiry)
			{
				this.Subscriber = Subscriber;
				this.Target = Target;
				this.Expiry = Expiry;
			}

			// Token: 0x170013E6 RID: 5094
			// (get) Token: 0x06004E46 RID: 20038 RVA: 0x001E14D2 File Offset: 0x001DF6D2
			// (set) Token: 0x06004E47 RID: 20039 RVA: 0x001E14DA File Offset: 0x001DF6DA
			public Client Subscriber { get; set; }

			// Token: 0x170013E7 RID: 5095
			// (get) Token: 0x06004E48 RID: 20040 RVA: 0x001E14E3 File Offset: 0x001DF6E3
			// (set) Token: 0x06004E49 RID: 20041 RVA: 0x001E14EB File Offset: 0x001DF6EB
			public CharacterInfo Target { get; set; }

			// Token: 0x170013E8 RID: 5096
			// (get) Token: 0x06004E4A RID: 20042 RVA: 0x001E14F4 File Offset: 0x001DF6F4
			// (set) Token: 0x06004E4B RID: 20043 RVA: 0x001E14FC File Offset: 0x001DF6FC
			public DateTimeOffset Expiry { get; set; }

			// Token: 0x06004E4C RID: 20044 RVA: 0x001E1508 File Offset: 0x001DF708
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("AfflictionSubscriber");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06004E4D RID: 20045 RVA: 0x001E1554 File Offset: 0x001DF754
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Subscriber = ");
				builder.Append(this.Subscriber);
				builder.Append(", Target = ");
				builder.Append(this.Target);
				builder.Append(", Expiry = ");
				builder.Append(this.Expiry.ToString());
				return true;
			}

			// Token: 0x06004E4E RID: 20046 RVA: 0x001E15BB File Offset: 0x001DF7BB
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.AfflictionSubscriber left, MedicalClinic.AfflictionSubscriber right)
			{
				return !(left == right);
			}

			// Token: 0x06004E4F RID: 20047 RVA: 0x001E15C7 File Offset: 0x001DF7C7
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.AfflictionSubscriber left, MedicalClinic.AfflictionSubscriber right)
			{
				return left.Equals(right);
			}

			// Token: 0x06004E50 RID: 20048 RVA: 0x001E15D1 File Offset: 0x001DF7D1
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Client>.Default.GetHashCode(this.<Subscriber>k__BackingField) * -1521134295 + EqualityComparer<CharacterInfo>.Default.GetHashCode(this.<Target>k__BackingField)) * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(this.<Expiry>k__BackingField);
			}

			// Token: 0x06004E51 RID: 20049 RVA: 0x001E1611 File Offset: 0x001DF811
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.AfflictionSubscriber && this.Equals((MedicalClinic.AfflictionSubscriber)obj);
			}

			// Token: 0x06004E52 RID: 20050 RVA: 0x001E162C File Offset: 0x001DF82C
			[CompilerGenerated]
			public bool Equals(MedicalClinic.AfflictionSubscriber other)
			{
				return EqualityComparer<Client>.Default.Equals(this.<Subscriber>k__BackingField, other.<Subscriber>k__BackingField) && EqualityComparer<CharacterInfo>.Default.Equals(this.<Target>k__BackingField, other.<Target>k__BackingField) && EqualityComparer<DateTimeOffset>.Default.Equals(this.<Expiry>k__BackingField, other.<Expiry>k__BackingField);
			}

			// Token: 0x06004E53 RID: 20051 RVA: 0x001E1681 File Offset: 0x001DF881
			[CompilerGenerated]
			public void Deconstruct(out Client Subscriber, out CharacterInfo Target, out DateTimeOffset Expiry)
			{
				Subscriber = this.Subscriber;
				Target = this.Target;
				Expiry = this.Expiry;
			}
		}

		// Token: 0x02000668 RID: 1640
		[NullableContext(0)]
		public enum NetworkHeader
		{
			// Token: 0x0400296A RID: 10602
			REQUEST_AFFLICTIONS,
			// Token: 0x0400296B RID: 10603
			AFFLICTION_UPDATE,
			// Token: 0x0400296C RID: 10604
			UNSUBSCRIBE_ME,
			// Token: 0x0400296D RID: 10605
			REQUEST_PENDING,
			// Token: 0x0400296E RID: 10606
			ADD_PENDING,
			// Token: 0x0400296F RID: 10607
			REMOVE_PENDING,
			// Token: 0x04002970 RID: 10608
			CLEAR_PENDING,
			// Token: 0x04002971 RID: 10609
			HEAL_PENDING,
			// Token: 0x04002972 RID: 10610
			ADD_EVERYTHING_TO_PENDING
		}

		// Token: 0x02000669 RID: 1641
		[NullableContext(0)]
		public enum AfflictionSeverity
		{
			// Token: 0x04002974 RID: 10612
			Low,
			// Token: 0x04002975 RID: 10613
			Medium,
			// Token: 0x04002976 RID: 10614
			High
		}

		// Token: 0x0200066A RID: 1642
		[NullableContext(0)]
		public enum MessageFlag
		{
			// Token: 0x04002978 RID: 10616
			Response,
			// Token: 0x04002979 RID: 10617
			Announce
		}

		// Token: 0x0200066B RID: 1643
		[NullableContext(0)]
		public enum HealRequestResult
		{
			// Token: 0x0400297B RID: 10619
			Unknown,
			// Token: 0x0400297C RID: 10620
			Success,
			// Token: 0x0400297D RID: 10621
			InsufficientFunds,
			// Token: 0x0400297E RID: 10622
			Refused
		}

		// Token: 0x0200066C RID: 1644
		[NullableContext(0)]
		[NetworkSerialize(51)]
		public readonly struct NetHealRequest : INetSerializableStruct, IEquatable<MedicalClinic.NetHealRequest>
		{
			// Token: 0x06004E54 RID: 20052 RVA: 0x001E169F File Offset: 0x001DF89F
			public NetHealRequest(MedicalClinic.HealRequestResult Result)
			{
				this.Result = Result;
			}

			// Token: 0x170013E9 RID: 5097
			// (get) Token: 0x06004E55 RID: 20053 RVA: 0x001E16A8 File Offset: 0x001DF8A8
			// (set) Token: 0x06004E56 RID: 20054 RVA: 0x001E16B0 File Offset: 0x001DF8B0
			public MedicalClinic.HealRequestResult Result { get; set; }

			// Token: 0x06004E57 RID: 20055 RVA: 0x001E16BC File Offset: 0x001DF8BC
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetHealRequest");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06004E58 RID: 20056 RVA: 0x001E1708 File Offset: 0x001DF908
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Result = ");
				builder.Append(this.Result.ToString());
				return true;
			}

			// Token: 0x06004E59 RID: 20057 RVA: 0x001E173D File Offset: 0x001DF93D
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.NetHealRequest left, MedicalClinic.NetHealRequest right)
			{
				return !(left == right);
			}

			// Token: 0x06004E5A RID: 20058 RVA: 0x001E1749 File Offset: 0x001DF949
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.NetHealRequest left, MedicalClinic.NetHealRequest right)
			{
				return left.Equals(right);
			}

			// Token: 0x06004E5B RID: 20059 RVA: 0x001E1753 File Offset: 0x001DF953
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<MedicalClinic.HealRequestResult>.Default.GetHashCode(this.<Result>k__BackingField);
			}

			// Token: 0x06004E5C RID: 20060 RVA: 0x001E1765 File Offset: 0x001DF965
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.NetHealRequest && this.Equals((MedicalClinic.NetHealRequest)obj);
			}

			// Token: 0x06004E5D RID: 20061 RVA: 0x001E177D File Offset: 0x001DF97D
			[CompilerGenerated]
			public bool Equals(MedicalClinic.NetHealRequest other)
			{
				return EqualityComparer<MedicalClinic.HealRequestResult>.Default.Equals(this.<Result>k__BackingField, other.<Result>k__BackingField);
			}

			// Token: 0x06004E5E RID: 20062 RVA: 0x001E1795 File Offset: 0x001DF995
			[CompilerGenerated]
			public void Deconstruct(out MedicalClinic.HealRequestResult Result)
			{
				Result = this.Result;
			}
		}

		// Token: 0x0200066D RID: 1645
		[NullableContext(0)]
		[NetworkSerialize(54)]
		public readonly struct NetRemovedAffliction : INetSerializableStruct, IEquatable<MedicalClinic.NetRemovedAffliction>
		{
			// Token: 0x06004E5F RID: 20063 RVA: 0x001E179F File Offset: 0x001DF99F
			public NetRemovedAffliction(MedicalClinic.NetCrewMember CrewMember, MedicalClinic.NetAffliction Affliction)
			{
				this.CrewMember = CrewMember;
				this.Affliction = Affliction;
			}

			// Token: 0x170013EA RID: 5098
			// (get) Token: 0x06004E60 RID: 20064 RVA: 0x001E17AF File Offset: 0x001DF9AF
			// (set) Token: 0x06004E61 RID: 20065 RVA: 0x001E17B7 File Offset: 0x001DF9B7
			public MedicalClinic.NetCrewMember CrewMember { get; set; }

			// Token: 0x170013EB RID: 5099
			// (get) Token: 0x06004E62 RID: 20066 RVA: 0x001E17C0 File Offset: 0x001DF9C0
			// (set) Token: 0x06004E63 RID: 20067 RVA: 0x001E17C8 File Offset: 0x001DF9C8
			public MedicalClinic.NetAffliction Affliction { get; set; }

			// Token: 0x06004E64 RID: 20068 RVA: 0x001E17D4 File Offset: 0x001DF9D4
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetRemovedAffliction");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06004E65 RID: 20069 RVA: 0x001E1820 File Offset: 0x001DFA20
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("CrewMember = ");
				builder.Append(this.CrewMember.ToString());
				builder.Append(", Affliction = ");
				builder.Append(this.Affliction.ToString());
				return true;
			}

			// Token: 0x06004E66 RID: 20070 RVA: 0x001E187C File Offset: 0x001DFA7C
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.NetRemovedAffliction left, MedicalClinic.NetRemovedAffliction right)
			{
				return !(left == right);
			}

			// Token: 0x06004E67 RID: 20071 RVA: 0x001E1888 File Offset: 0x001DFA88
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.NetRemovedAffliction left, MedicalClinic.NetRemovedAffliction right)
			{
				return left.Equals(right);
			}

			// Token: 0x06004E68 RID: 20072 RVA: 0x001E1892 File Offset: 0x001DFA92
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<MedicalClinic.NetCrewMember>.Default.GetHashCode(this.<CrewMember>k__BackingField) * -1521134295 + EqualityComparer<MedicalClinic.NetAffliction>.Default.GetHashCode(this.<Affliction>k__BackingField);
			}

			// Token: 0x06004E69 RID: 20073 RVA: 0x001E18BB File Offset: 0x001DFABB
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.NetRemovedAffliction && this.Equals((MedicalClinic.NetRemovedAffliction)obj);
			}

			// Token: 0x06004E6A RID: 20074 RVA: 0x001E18D3 File Offset: 0x001DFAD3
			[CompilerGenerated]
			public bool Equals(MedicalClinic.NetRemovedAffliction other)
			{
				return EqualityComparer<MedicalClinic.NetCrewMember>.Default.Equals(this.<CrewMember>k__BackingField, other.<CrewMember>k__BackingField) && EqualityComparer<MedicalClinic.NetAffliction>.Default.Equals(this.<Affliction>k__BackingField, other.<Affliction>k__BackingField);
			}

			// Token: 0x06004E6B RID: 20075 RVA: 0x001E1905 File Offset: 0x001DFB05
			[CompilerGenerated]
			public void Deconstruct(out MedicalClinic.NetCrewMember CrewMember, out MedicalClinic.NetAffliction Affliction)
			{
				CrewMember = this.CrewMember;
				Affliction = this.Affliction;
			}
		}

		// Token: 0x0200066E RID: 1646
		[NullableContext(2)]
		[Nullable(0)]
		public struct NetAffliction : INetSerializableStruct
		{
			// Token: 0x06004E6C RID: 20076 RVA: 0x001E1920 File Offset: 0x001DFB20
			[NullableContext(1)]
			public void SetAffliction(Affliction affliction, CharacterHealth characterHealth)
			{
				this.Identifier = affliction.Identifier;
				this.Strength = (ushort)Math.Ceiling((double)affliction.Strength);
				this.Price = (ushort)((float)affliction.Prefab.BaseHealCost + (float)this.Strength * affliction.Prefab.HealCostMultiplier);
				this.VitalityDecrease = (int)affliction.GetVitalityDecrease(characterHealth);
			}

			// Token: 0x170013EC RID: 5100
			// (get) Token: 0x06004E6D RID: 20077 RVA: 0x001E1984 File Offset: 0x001DFB84
			// (set) Token: 0x06004E6E RID: 20078 RVA: 0x001E19F8 File Offset: 0x001DFBF8
			public AfflictionPrefab Prefab
			{
				get
				{
					AfflictionPrefab cached = this.cachedPrefab;
					if (cached != null)
					{
						return cached;
					}
					foreach (AfflictionPrefab prefab in AfflictionPrefab.List)
					{
						if (prefab.Identifier == this.Identifier)
						{
							this.cachedPrefab = prefab;
							return prefab;
						}
					}
					return null;
				}
				set
				{
					this.cachedPrefab = value;
					this.Identifier = ((value != null) ? value.Identifier : Identifier.Empty);
					this.Strength = 0;
					this.Price = 0;
				}
			}

			// Token: 0x06004E6F RID: 20079 RVA: 0x001E1A25 File Offset: 0x001DFC25
			[NullableContext(1)]
			public readonly bool AfflictionEquals(AfflictionPrefab prefab)
			{
				return prefab.Identifier == this.Identifier;
			}

			// Token: 0x06004E70 RID: 20080 RVA: 0x001E1A38 File Offset: 0x001DFC38
			public readonly bool AfflictionEquals(MedicalClinic.NetAffliction affliction)
			{
				return affliction.Identifier == this.Identifier;
			}

			// Token: 0x04002982 RID: 10626
			[NetworkSerialize(59)]
			public Identifier Identifier;

			// Token: 0x04002983 RID: 10627
			[NetworkSerialize(62)]
			public ushort Strength;

			// Token: 0x04002984 RID: 10628
			[NetworkSerialize(65)]
			public int VitalityDecrease;

			// Token: 0x04002985 RID: 10629
			[NetworkSerialize(68)]
			public ushort Price;

			// Token: 0x04002986 RID: 10630
			private AfflictionPrefab cachedPrefab;
		}

		// Token: 0x0200066F RID: 1647
		[NullableContext(0)]
		public struct NetCrewMember : INetSerializableStruct, IEquatable<MedicalClinic.NetCrewMember>
		{
			// Token: 0x06004E71 RID: 20081 RVA: 0x001E1A4C File Offset: 0x001DFC4C
			[NullableContext(1)]
			public NetCrewMember(CharacterInfo info)
			{
				this.CharacterInfoID = (int)info.ID;
				this.Afflictions = ImmutableArray<MedicalClinic.NetAffliction>.Empty;
			}

			// Token: 0x06004E72 RID: 20082 RVA: 0x001E1A65 File Offset: 0x001DFC65
			public NetCrewMember([Nullable(1)] CharacterInfo info, ImmutableArray<MedicalClinic.NetAffliction> afflictions)
			{
				this = new MedicalClinic.NetCrewMember(info);
				this.Afflictions = afflictions;
			}

			// Token: 0x06004E73 RID: 20083 RVA: 0x001E1A78 File Offset: 0x001DFC78
			[NullableContext(2)]
			public readonly CharacterInfo FindCharacterInfo([Nullable(new byte[]
			{
				0,
				1
			})] ImmutableArray<CharacterInfo> crew)
			{
				foreach (CharacterInfo info in crew)
				{
					if ((int)info.ID == this.CharacterInfoID)
					{
						return info;
					}
				}
				return null;
			}

			// Token: 0x06004E74 RID: 20084 RVA: 0x001E1AB1 File Offset: 0x001DFCB1
			public readonly bool CharacterEquals(MedicalClinic.NetCrewMember crewMember)
			{
				return crewMember.CharacterInfoID == this.CharacterInfoID;
			}

			// Token: 0x06004E75 RID: 20085 RVA: 0x001E1AC4 File Offset: 0x001DFCC4
			[CompilerGenerated]
			public override readonly string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetCrewMember");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06004E76 RID: 20086 RVA: 0x001E1B10 File Offset: 0x001DFD10
			[CompilerGenerated]
			private readonly bool PrintMembers(StringBuilder builder)
			{
				builder.Append("CharacterInfoID = ");
				builder.Append(this.CharacterInfoID.ToString());
				builder.Append(", Afflictions = ");
				builder.Append(this.Afflictions.ToString());
				return true;
			}

			// Token: 0x06004E77 RID: 20087 RVA: 0x001E1B66 File Offset: 0x001DFD66
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.NetCrewMember left, MedicalClinic.NetCrewMember right)
			{
				return !(left == right);
			}

			// Token: 0x06004E78 RID: 20088 RVA: 0x001E1B72 File Offset: 0x001DFD72
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.NetCrewMember left, MedicalClinic.NetCrewMember right)
			{
				return left.Equals(right);
			}

			// Token: 0x06004E79 RID: 20089 RVA: 0x001E1B7C File Offset: 0x001DFD7C
			[CompilerGenerated]
			public override readonly int GetHashCode()
			{
				return EqualityComparer<int>.Default.GetHashCode(this.CharacterInfoID) * -1521134295 + EqualityComparer<ImmutableArray<MedicalClinic.NetAffliction>>.Default.GetHashCode(this.Afflictions);
			}

			// Token: 0x06004E7A RID: 20090 RVA: 0x001E1BA5 File Offset: 0x001DFDA5
			[CompilerGenerated]
			public override readonly bool Equals(object obj)
			{
				return obj is MedicalClinic.NetCrewMember && this.Equals((MedicalClinic.NetCrewMember)obj);
			}

			// Token: 0x06004E7B RID: 20091 RVA: 0x001E1BBD File Offset: 0x001DFDBD
			[CompilerGenerated]
			public readonly bool Equals(MedicalClinic.NetCrewMember other)
			{
				return EqualityComparer<int>.Default.Equals(this.CharacterInfoID, other.CharacterInfoID) && EqualityComparer<ImmutableArray<MedicalClinic.NetAffliction>>.Default.Equals(this.Afflictions, other.Afflictions);
			}

			// Token: 0x04002987 RID: 10631
			[NetworkSerialize(120)]
			public int CharacterInfoID;

			// Token: 0x04002988 RID: 10632
			[NetworkSerialize(123)]
			public ImmutableArray<MedicalClinic.NetAffliction> Afflictions;
		}

		// Token: 0x02000670 RID: 1648
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04002989 RID: 10633
			[Nullable(0)]
			public static Func<Affliction, bool> <0>__IsHealable;
		}
	}
}
