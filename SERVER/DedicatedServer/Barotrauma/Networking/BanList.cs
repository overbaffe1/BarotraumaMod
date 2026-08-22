using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;

namespace Barotrauma.Networking
{
	// Token: 0x02000366 RID: 870
	[NullableContext(1)]
	[Nullable(0)]
	internal class BanList
	{
		// Token: 0x06003352 RID: 13138 RVA: 0x0015A6BC File Offset: 0x001588BC
		private void LoadLegacyBanList()
		{
			if (!File.Exists("Data/bannedplayers.txt"))
			{
				return;
			}
			string[] lines;
			try
			{
				lines = File.ReadAllLines("Data/bannedplayers.txt", null, false);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to open the list of banned players in Data/bannedplayers.txt", e, null, false, false);
				return;
			}
			foreach (string line in lines)
			{
				string[] separatedLine = line.Split(',', StringSplitOptions.None);
				if (separatedLine.Length >= 2)
				{
					string name = separatedLine[0];
					string endpointStr = separatedLine[1];
					DateTime? expirationTime = null;
					if (separatedLine.Length > 2 && !string.IsNullOrEmpty(separatedLine[2]))
					{
						DateTime parsedTime;
						if (DateTime.TryParse(separatedLine[2], out parsedTime))
						{
							expirationTime = new DateTime?(DateTime.SpecifyKind(parsedTime, DateTimeKind.Local));
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(149, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to parse the ban duration of \"");
							defaultInterpolatedStringHandler.AppendFormatted(name);
							defaultInterpolatedStringHandler.AppendLiteral("\" (");
							defaultInterpolatedStringHandler.AppendFormatted(separatedLine[2]);
							defaultInterpolatedStringHandler.AppendLiteral(") from the legacy ban list file (text file which has now been changed to XML). Considering the ban permanent.");
							string error = defaultInterpolatedStringHandler.ToStringAndClear();
							DebugConsole.ThrowError(error, null, null, false, false);
							GameServer.AddPendingMessageToOwner(error, ChatMessageType.Error);
						}
					}
					string reason = (separatedLine.Length > 3) ? string.Join(",", separatedLine.Skip(3)) : "";
					Option<SerializableDateTime> serializableExpirationTime = (expirationTime != null) ? Option<SerializableDateTime>.Some(new SerializableDateTime(expirationTime.Value)) : Option<SerializableDateTime>.None();
					AccountId accountId;
					Address address;
					if (AccountId.Parse(endpointStr).TryUnwrap(out accountId))
					{
						this.bannedPlayers.Add(new BannedPlayer(name, accountId, reason, serializableExpirationTime));
					}
					else if (Address.Parse(endpointStr).TryUnwrap(out address))
					{
						this.bannedPlayers.Add(new BannedPlayer(name, address, reason, serializableExpirationTime));
					}
				}
			}
			this.Save();
			File.Delete("Data/bannedplayers.txt", true);
		}

		// Token: 0x06003353 RID: 13139 RVA: 0x0015A8A0 File Offset: 0x00158AA0
		private void LoadBanList()
		{
			XDocument doc = XMLExtensions.TryLoadXml("Data/bannedplayers.xml");
			if (((doc != null) ? doc.Root : null) == null)
			{
				return;
			}
			List<BannedPlayer> list = this.bannedPlayers;
			IEnumerable<XElement> source = doc.Root.Elements();
			Func<XElement, Option<BannedPlayer>> selector;
			if ((selector = BanList.<>O.<0>__loadFromElement) == null)
			{
				selector = (BanList.<>O.<0>__loadFromElement = new Func<XElement, Option<BannedPlayer>>(BanList.<LoadBanList>g__loadFromElement|3_0));
			}
			list.AddRange(source.Select(selector).NotNone<BannedPlayer>());
		}

		// Token: 0x06003354 RID: 13140 RVA: 0x0015A902 File Offset: 0x00158B02
		private void RemoveExpired()
		{
			this.bannedPlayers.RemoveAll((BannedPlayer bp) => bp.Expired);
		}

		// Token: 0x06003355 RID: 13141 RVA: 0x0015A92F File Offset: 0x00158B2F
		public bool IsBanned(Endpoint endpoint, out string reason)
		{
			return this.IsBanned(endpoint.Address, out reason);
		}

		// Token: 0x06003356 RID: 13142 RVA: 0x0015A940 File Offset: 0x00158B40
		public bool IsBanned(Address address, out string reason)
		{
			this.RemoveExpired();
			if (address.IsLocalHost)
			{
				reason = string.Empty;
				return false;
			}
			BannedPlayer bannedPlayer = this.bannedPlayers.Find(delegate(BannedPlayer bp)
			{
				Address adr;
				return bp.AddressOrAccountId.TryGet(out adr) && address.Equals(adr);
			});
			reason = (((bannedPlayer != null) ? bannedPlayer.Reason : null) ?? string.Empty);
			return bannedPlayer != null;
		}

		// Token: 0x06003357 RID: 13143 RVA: 0x0015A9AC File Offset: 0x00158BAC
		public bool IsBanned(AccountId accountId, out string reason)
		{
			this.RemoveExpired();
			BannedPlayer bannedPlayer = this.bannedPlayers.Find(delegate(BannedPlayer bp)
			{
				AccountId id;
				return bp.AddressOrAccountId.TryGet(out id) && accountId.Equals(id);
			}) ?? this.bannedPlayers.Find(delegate(BannedPlayer bp)
			{
				Address adr;
				if (bp.AddressOrAccountId.TryGet(out adr))
				{
					SteamP2PAddress steamAdr = adr as SteamP2PAddress;
					if (steamAdr != null)
					{
						return steamAdr.SteamId.Equals(accountId);
					}
				}
				return false;
			});
			reason = (((bannedPlayer != null) ? bannedPlayer.Reason : null) ?? string.Empty);
			return bannedPlayer != null;
		}

		// Token: 0x06003358 RID: 13144 RVA: 0x0015AA1C File Offset: 0x00158C1C
		public bool IsBanned(AccountInfo accountInfo, out string reason)
		{
			AccountId accountId;
			if (accountInfo.AccountId.TryUnwrap(out accountId) && this.IsBanned(accountId, out reason))
			{
				return true;
			}
			foreach (AccountId otherId in accountInfo.OtherMatchingIds)
			{
				if (this.IsBanned(otherId, out reason))
				{
					return true;
				}
			}
			reason = "";
			return false;
		}

		// Token: 0x06003359 RID: 13145 RVA: 0x0015AA79 File Offset: 0x00158C79
		public void BanPlayer(string name, Endpoint endpoint, string reason, TimeSpan? duration)
		{
			this.BanPlayer(name, endpoint.Address, reason, duration);
		}

		// Token: 0x0600335A RID: 13146 RVA: 0x0015AA90 File Offset: 0x00158C90
		public void BanPlayer(string name, Either<Address, AccountId> addressOrAccountId, string reason, TimeSpan? duration)
		{
			Address address;
			if (addressOrAccountId.TryGet(out address) && address.IsLocalHost)
			{
				DebugConsole.AddWarning("Cannot ban localhost (" + address.StringRepresentation + ")", null);
				return;
			}
			BannedPlayer existingBan = this.bannedPlayers.Find((BannedPlayer bp) => bp.AddressOrAccountId == addressOrAccountId);
			if (existingBan != null)
			{
				this.bannedPlayers.Remove(existingBan);
			}
			string logMsg = "Banned " + name;
			if (!string.IsNullOrEmpty(reason))
			{
				logMsg = logMsg + ", reason: " + reason;
			}
			if (duration != null)
			{
				logMsg = logMsg + ", duration: " + duration.Value.ToString();
			}
			DebugConsole.Log(logMsg);
			Option<SerializableDateTime> expirationTime = Option<SerializableDateTime>.None();
			if (duration != null)
			{
				expirationTime = Option<SerializableDateTime>.Some(new SerializableDateTime(DateTime.Now + duration.Value));
			}
			this.bannedPlayers.Add(new BannedPlayer(name, addressOrAccountId, reason, expirationTime));
			this.Save();
		}

		// Token: 0x0600335B RID: 13147 RVA: 0x0015ABA4 File Offset: 0x00158DA4
		public void UnbanPlayer(Endpoint endpoint)
		{
			this.UnbanPlayer(endpoint.Address);
		}

		// Token: 0x0600335C RID: 13148 RVA: 0x0015ABB8 File Offset: 0x00158DB8
		public void UnbanPlayer(Either<Address, AccountId> addressOrAccountId)
		{
			BannedPlayer player = this.bannedPlayers.Find((BannedPlayer bp) => bp.AddressOrAccountId == addressOrAccountId);
			if (player == null)
			{
				string str = "Could not unban endpoint \"";
				Either<Address, AccountId> addressOrAccountId2 = addressOrAccountId;
				DebugConsole.Log(str + ((addressOrAccountId2 != null) ? addressOrAccountId2.ToString() : null) + "\". Matching player not found.");
				return;
			}
			this.RemoveBan(player);
		}

		// Token: 0x0600335D RID: 13149 RVA: 0x0015AC1B File Offset: 0x00158E1B
		private void RemoveBan(BannedPlayer banned)
		{
			DebugConsole.Log("Removing ban from " + banned.Name);
			GameServer.Log("Removing ban from " + banned.Name, ServerLog.MessageType.ServerMessage);
			this.bannedPlayers.Remove(banned);
			this.Save();
		}

		// Token: 0x0600335E RID: 13150 RVA: 0x0015AC5C File Offset: 0x00158E5C
		public void Save()
		{
			GameServer.Log("Saving banlist", ServerLog.MessageType.ServerMessage);
			GameServer server = GameMain.Server;
			if (server != null)
			{
				ServerSettings serverSettings = server.ServerSettings;
				if (serverSettings != null)
				{
					serverSettings.UpdateFlag(ServerSettings.NetFlags.Properties);
				}
			}
			this.RemoveExpired();
			XDocument doc = new XDocument(new object[]
			{
				new XElement("bannedplayers")
			});
			IEnumerable<BannedPlayer> source = this.bannedPlayers;
			Func<BannedPlayer, XElement> selector;
			if ((selector = BanList.<>O.<1>__saveToElement) == null)
			{
				selector = (BanList.<>O.<1>__saveToElement = new Func<BannedPlayer, XElement>(BanList.<Save>g__saveToElement|14_0));
			}
			source.Select(selector).ForEach(new Action<XElement>(doc.Root.Add));
			doc.SaveSafe("Data/bannedplayers.xml", SaveOptions.None, false, 0);
		}

		// Token: 0x0600335F RID: 13151 RVA: 0x0015AD00 File Offset: 0x00158F00
		public void ServerAdminWrite(IWriteMessage outMsg, Client c)
		{
			try
			{
				if (outMsg == null)
				{
					throw new ArgumentException("OutMsg was null");
				}
				if (GameMain.Server == null)
				{
					throw new Exception("GameMain.Server was null");
				}
				if (!c.HasPermission(ClientPermissions.Ban))
				{
					outMsg.WriteBoolean(false);
					outMsg.WritePadBits();
				}
				else
				{
					outMsg.WriteBoolean(true);
					outMsg.WriteBoolean(c.Connection == GameMain.Server.OwnerConnection);
					outMsg.WritePadBits();
					outMsg.WriteVariableUInt32((uint)this.bannedPlayers.Count);
					for (int i = 0; i < this.bannedPlayers.Count; i++)
					{
						BannedPlayer bannedPlayer = this.bannedPlayers[i];
						outMsg.WriteString(bannedPlayer.Name);
						outMsg.WriteUInt32(bannedPlayer.UniqueIdentifier);
						outMsg.WriteBoolean(bannedPlayer.ExpirationTime.IsSome());
						outMsg.WritePadBits();
						SerializableDateTime expirationTime;
						if (bannedPlayer.ExpirationTime.TryUnwrap(out expirationTime))
						{
							double hoursFromNow = (expirationTime.ToUtcValue() - DateTime.UtcNow).TotalHours;
							outMsg.WriteDouble(hoursFromNow);
						}
						outMsg.WriteString(bannedPlayer.Reason ?? "");
						if (c.Connection == GameMain.Server.OwnerConnection)
						{
							Address endpoint;
							if (bannedPlayer.AddressOrAccountId.TryGet(out endpoint))
							{
								outMsg.WriteBoolean(true);
								outMsg.WritePadBits();
								outMsg.WriteString(endpoint.StringRepresentation);
							}
							else
							{
								outMsg.WriteBoolean(false);
								outMsg.WritePadBits();
								outMsg.WriteString(((U)bannedPlayer.AddressOrAccountId).StringRepresentation);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				string str = "Error while writing banlist. {";
				Exception ex = e;
				string errorMsg = str + ((ex != null) ? ex.ToString() : null) + "}\n" + e.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce("Banlist.ServerAdminWrite", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				throw;
			}
		}

		// Token: 0x06003360 RID: 13152 RVA: 0x0015AED8 File Offset: 0x001590D8
		public bool ServerAdminRead(IReadMessage incMsg, Client c)
		{
			if (!c.HasPermission(ClientPermissions.Ban))
			{
				uint removeCount = incMsg.ReadVariableUInt32();
				incMsg.BitPosition += (int)(removeCount * 4U * 8U);
				return false;
			}
			uint removeCount2 = incMsg.ReadVariableUInt32();
			int i = 0;
			while ((long)i < (long)((ulong)removeCount2))
			{
				uint id = incMsg.ReadUInt32();
				BannedPlayer bannedPlayer = this.bannedPlayers.Find((BannedPlayer p) => p.UniqueIdentifier == id);
				if (bannedPlayer != null && c.HasPermission(ClientPermissions.Unban))
				{
					string[] array = new string[6];
					array[0] = NetworkMember.ClientLogName(c, null);
					array[1] = " unbanned ";
					array[2] = bannedPlayer.Name;
					array[3] = " (";
					int num = 4;
					Either<Address, AccountId> addressOrAccountId = bannedPlayer.AddressOrAccountId;
					array[num] = ((addressOrAccountId != null) ? addressOrAccountId.ToString() : null);
					array[5] = ")";
					GameServer.Log(string.Concat(array), ServerLog.MessageType.ConsoleUsage);
					this.RemoveBan(bannedPlayer);
				}
				i++;
			}
			return removeCount2 > 0U;
		}

		// Token: 0x17000E4E RID: 3662
		// (get) Token: 0x06003361 RID: 13153 RVA: 0x0015AFBB File Offset: 0x001591BB
		public IReadOnlyList<BannedPlayer> BannedPlayers
		{
			get
			{
				return this.bannedPlayers;
			}
		}

		// Token: 0x17000E4F RID: 3663
		// (get) Token: 0x06003362 RID: 13154 RVA: 0x0015AFC3 File Offset: 0x001591C3
		public IEnumerable<string> BannedNames
		{
			get
			{
				return from bp in this.bannedPlayers
				select bp.Name;
			}
		}

		// Token: 0x17000E50 RID: 3664
		// (get) Token: 0x06003363 RID: 13155 RVA: 0x0015AFEF File Offset: 0x001591EF
		public IEnumerable<Either<Address, AccountId>> BannedAddresses
		{
			get
			{
				return from bp in this.bannedPlayers
				select bp.AddressOrAccountId;
			}
		}

		// Token: 0x06003364 RID: 13156 RVA: 0x0015B01B File Offset: 0x0015921B
		private void InitProjectSpecific()
		{
			if (!File.Exists("Data/bannedplayers.xml"))
			{
				this.LoadLegacyBanList();
			}
			else
			{
				this.LoadBanList();
			}
			this.RemoveExpired();
		}

		// Token: 0x06003365 RID: 13157 RVA: 0x0015B03D File Offset: 0x0015923D
		public BanList()
		{
			this.bannedPlayers = new List<BannedPlayer>();
			this.InitProjectSpecific();
		}

		// Token: 0x06003366 RID: 13158 RVA: 0x0015B058 File Offset: 0x00159258
		[CompilerGenerated]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		internal static Option<BannedPlayer> <LoadBanList>g__loadFromElement|3_0(XElement element)
		{
			Option<AccountId> accountId = AccountId.Parse(element.GetAttributeString("accountid", ""));
			Option<Address> address = Address.Parse(element.GetAttributeString("address", ""));
			string name = element.GetAttributeString("name", "");
			string reason = element.GetAttributeString("reason", "");
			Option<SerializableDateTime> expirationTime = Option<SerializableDateTime>.None();
			string expirationTimeStr = element.GetAttributeString("expirationtime", "");
			ulong binaryDateTime;
			if (ulong.TryParse(expirationTimeStr, out binaryDateTime) && binaryDateTime > 0UL)
			{
				expirationTime = Option<SerializableDateTime>.Some(new SerializableDateTime(DateTime.FromBinary((long)binaryDateTime), SerializableTimeZone.LocalTimeZone));
			}
			expirationTime = expirationTime.Fallback(SerializableDateTime.Parse(expirationTimeStr));
			if (accountId.IsNone() && address.IsNone())
			{
				return Option<BannedPlayer>.None();
			}
			AccountId accId;
			Either<Address, AccountId> either;
			if (!accountId.TryUnwrap(out accId))
			{
				Address addr;
				if (!address.TryUnwrap(out addr))
				{
					throw new InvalidCastException();
				}
				either = addr;
			}
			else
			{
				either = accId;
			}
			Either<Address, AccountId> addressOrAccountId = either;
			return Option<BannedPlayer>.Some(new BannedPlayer(name, addressOrAccountId, reason, expirationTime));
		}

		// Token: 0x06003367 RID: 13159 RVA: 0x0015B15C File Offset: 0x0015935C
		[CompilerGenerated]
		internal static XElement <Save>g__saveToElement|14_0(BannedPlayer bannedPlayer)
		{
			XElement retVal = new XElement("ban");
			retVal.SetAttributeValue("name", bannedPlayer.Name);
			retVal.SetAttributeValue("reason", bannedPlayer.Reason);
			AccountId accountId;
			Address address;
			if (bannedPlayer.AddressOrAccountId.TryGet(out accountId))
			{
				retVal.SetAttributeValue("accountid", accountId.StringRepresentation);
			}
			else if (bannedPlayer.AddressOrAccountId.TryGet(out address))
			{
				retVal.SetAttributeValue("address", address.StringRepresentation);
			}
			SerializableDateTime expirationTime;
			if (bannedPlayer.ExpirationTime.TryUnwrap(out expirationTime))
			{
				retVal.SetAttributeValue("expirationtime", expirationTime.ToLocalValue().ToBinary());
			}
			return retVal;
		}

		// Token: 0x04001973 RID: 6515
		private const string SavePath = "Data/bannedplayers.xml";

		// Token: 0x04001974 RID: 6516
		private const string LegacySavePath = "Data/bannedplayers.txt";

		// Token: 0x04001975 RID: 6517
		private readonly List<BannedPlayer> bannedPlayers;

		// Token: 0x02000BA8 RID: 2984
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040039F8 RID: 14840
			[Nullable(0)]
			public static Func<XElement, Option<BannedPlayer>> <0>__loadFromElement;

			// Token: 0x040039F9 RID: 14841
			[Nullable(0)]
			public static Func<BannedPlayer, XElement> <1>__saveToElement;
		}
	}
}
