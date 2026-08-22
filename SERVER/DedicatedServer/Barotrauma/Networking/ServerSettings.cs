using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma.Networking
{
	// Token: 0x02000377 RID: 887
	internal class ServerSettings : ISerializableEntity
	{
		// Token: 0x06003502 RID: 13570 RVA: 0x0016F4E8 File Offset: 0x0016D6E8
		public void UpdateFlag(ServerSettings.NetFlags flag)
		{
			this.LastUpdateIdForFlag[flag] = GameMain.NetLobbyScreen.LastUpdateID + 1;
		}

		// Token: 0x06003503 RID: 13571 RVA: 0x0016F504 File Offset: 0x0016D704
		public ServerSettings.NetFlags UnsentFlags()
		{
			return (from k in this.LastUpdateIdForFlag.Keys
			where NetIdUtils.IdMoreRecent(this.LastUpdateIdForFlag[k], GameMain.NetLobbyScreen.LastUpdateID)
			select k).Aggregate(ServerSettings.NetFlags.None, (ServerSettings.NetFlags f1, ServerSettings.NetFlags f2) => f1 | f2);
		}

		// Token: 0x06003504 RID: 13572 RVA: 0x0016F552 File Offset: 0x0016D752
		private bool IsFlagRequired(Client c, ServerSettings.NetFlags flag)
		{
			return NetIdUtils.IdMoreRecent(this.LastUpdateIdForFlag[flag], c.LastRecvLobbyUpdate) || !c.InitialLobbyUpdateSent;
		}

		// Token: 0x06003505 RID: 13573 RVA: 0x0016F578 File Offset: 0x0016D778
		public ServerSettings.NetFlags GetRequiredFlags(Client c)
		{
			return (from k in this.LastUpdateIdForFlag.Keys
			where this.IsFlagRequired(c, k)
			select k).Aggregate(ServerSettings.NetFlags.None, (ServerSettings.NetFlags f1, ServerSettings.NetFlags f2) => f1 | f2);
		}

		// Token: 0x06003506 RID: 13574 RVA: 0x0016F5DC File Offset: 0x0016D7DC
		public void ForcePropertyUpdate()
		{
			this.UpdateFlag(ServerSettings.NetFlags.Properties);
			foreach (ServerSettings.NetPropertyData property in this.netProperties.Values)
			{
				property.ForceUpdate();
			}
		}

		// Token: 0x06003507 RID: 13575 RVA: 0x0016F63C File Offset: 0x0016D83C
		private void WriteNetProperties(IWriteMessage outMsg, Client c)
		{
			foreach (uint key in this.netProperties.Keys)
			{
				ServerSettings.NetPropertyData property = this.netProperties[key];
				property.SyncValue();
				if (NetIdUtils.IdMoreRecent(property.LastUpdateID, c.LastRecvLobbyUpdate) || !c.InitialLobbyUpdateSent)
				{
					outMsg.WriteUInt32(key);
					this.netProperties[key].Write(outMsg, null);
				}
			}
			outMsg.WriteUInt32(0U);
		}

		// Token: 0x06003508 RID: 13576 RVA: 0x0016F6DC File Offset: 0x0016D8DC
		public void ServerAdminWrite(IWriteMessage outMsg, Client c)
		{
			c.LastSentServerSettingsUpdate = this.LastUpdateIdForFlag[ServerSettings.NetFlags.Properties];
			this.WriteNetProperties(outMsg, c);
			this.WriteMonsterEnabled(outMsg, null);
			this.BanList.ServerAdminWrite(outMsg, c);
		}

		// Token: 0x06003509 RID: 13577 RVA: 0x0016F710 File Offset: 0x0016D910
		public void ServerWrite(IWriteMessage outMsg, Client c)
		{
			ServerSettings.NetFlags requiredFlags = this.GetRequiredFlags(c);
			outMsg.WriteByte((byte)requiredFlags);
			outMsg.WriteByte((byte)this.PlayStyle);
			outMsg.WriteByte((byte)this.MaxPlayers);
			outMsg.WriteBoolean(this.HasPassword);
			outMsg.WriteBoolean(this.IsPublic);
			outMsg.WriteBoolean(this.AllowFileTransfers);
			outMsg.WritePadBits();
			outMsg.WriteRangedInteger(this.TickRate, 1, 60);
			if (requiredFlags.HasFlag(ServerSettings.NetFlags.Properties))
			{
				this.WriteExtraCargo(outMsg);
				this.WritePerks(outMsg);
			}
			if (requiredFlags.HasFlag(ServerSettings.NetFlags.HiddenSubs))
			{
				this.WriteHiddenSubs(outMsg);
			}
			if (NetIdUtils.IdMoreRecent(this.LastUpdateIdForFlag[ServerSettings.NetFlags.Properties], c.LastRecvServerSettingsUpdate))
			{
				outMsg.WriteBoolean(true);
				outMsg.WritePadBits();
				this.ServerAdminWrite(outMsg, c);
				return;
			}
			outMsg.WriteBoolean(false);
			outMsg.WritePadBits();
		}

		// Token: 0x0600350A RID: 13578 RVA: 0x0016F7F8 File Offset: 0x0016D9F8
		public void ReadPerks(IReadMessage incMsg, Client c)
		{
			if (!ServerSettings.<ReadPerks>g__HasPermissionToChangePerks|12_0(c))
			{
				return;
			}
			if (!this.ReadPerks(incMsg))
			{
				return;
			}
			this.UpdateFlag(ServerSettings.NetFlags.Properties);
			this.SaveSettings();
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			ushort lastUpdateID = netLobbyScreen.LastUpdateID;
			netLobbyScreen.LastUpdateID = lastUpdateID + 1;
		}

		// Token: 0x0600350B RID: 13579 RVA: 0x0016F83C File Offset: 0x0016DA3C
		public void ServerRead(IReadMessage incMsg, Client c)
		{
			if (!c.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings))
			{
				return;
			}
			ServerSettings.NetFlags flags = (ServerSettings.NetFlags)incMsg.ReadByte();
			bool changed = false;
			if (flags.HasFlag(ServerSettings.NetFlags.Properties))
			{
				bool propertiesChanged = this.ReadExtraCargo(incMsg);
				uint count = incMsg.ReadUInt32();
				int i = 0;
				while ((long)i < (long)((ulong)count))
				{
					uint key = incMsg.ReadUInt32();
					if (this.netProperties.ContainsKey(key))
					{
						object prevValue = this.netProperties[key].Value;
						this.netProperties[key].Read(incMsg);
						if (!this.netProperties[key].PropEquals(prevValue, this.netProperties[key]))
						{
							string str = NetworkMember.ClientLogName(c, null);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
							defaultInterpolatedStringHandler.AppendLiteral(" changed ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.netProperties[key].Name);
							string str2 = defaultInterpolatedStringHandler.ToStringAndClear();
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(4, 1);
							defaultInterpolatedStringHandler2.AppendLiteral(" to ");
							defaultInterpolatedStringHandler2.AppendFormatted<object>(this.netProperties[key].Value);
							GameServer.Log(str + str2 + defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
						}
						propertiesChanged = true;
					}
					else
					{
						uint size = incMsg.ReadVariableUInt32();
						incMsg.BitPosition += (int)(8U * size);
					}
					i++;
				}
				bool changedMonsterSettings = incMsg.ReadBoolean();
				incMsg.ReadPadBits();
				propertiesChanged = (propertiesChanged || changedMonsterSettings);
				if (changedMonsterSettings)
				{
					this.ReadMonsterEnabled(incMsg);
				}
				propertiesChanged |= this.BanList.ServerAdminRead(incMsg, c);
				if (propertiesChanged)
				{
					this.UpdateFlag(ServerSettings.NetFlags.Properties);
					GameMain.Server.RefreshPvpTeamAssignments(false, false);
				}
				changed = (changed || propertiesChanged);
			}
			if (flags.HasFlag(ServerSettings.NetFlags.HiddenSubs))
			{
				this.ReadHiddenSubs(incMsg);
				changed |= true;
				this.UpdateFlag(ServerSettings.NetFlags.HiddenSubs);
			}
			if (flags.HasFlag(ServerSettings.NetFlags.Misc))
			{
				List<Identifier> missionTypes = new List<Identifier>(GameMain.NetLobbyScreen.MissionTypes);
				Identifier addedMissionType = incMsg.ReadIdentifier();
				Identifier removedMissionType = incMsg.ReadIdentifier();
				if (!addedMissionType.IsEmpty)
				{
					missionTypes.Add(addedMissionType);
				}
				if (!removedMissionType.IsEmpty)
				{
					missionTypes.Remove(removedMissionType);
				}
				GameMain.NetLobbyScreen.MissionTypes = missionTypes;
				this.TraitorDangerLevel = this.TraitorDangerLevel + (int)incMsg.ReadByte() - 1;
				changed |= true;
				this.UpdateFlag(ServerSettings.NetFlags.Misc);
			}
			if (flags.HasFlag(ServerSettings.NetFlags.LevelSeed))
			{
				GameMain.NetLobbyScreen.LevelSeed = incMsg.ReadString();
				changed |= true;
				this.UpdateFlag(ServerSettings.NetFlags.LevelSeed);
			}
			if (changed)
			{
				if (this.KarmaPreset == "custom")
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember != null)
					{
						KarmaManager karmaManager = networkMember.KarmaManager;
						if (karmaManager != null)
						{
							karmaManager.SaveCustomPreset();
						}
					}
					NetworkMember networkMember2 = GameMain.NetworkMember;
					if (networkMember2 != null)
					{
						KarmaManager karmaManager2 = networkMember2.KarmaManager;
						if (karmaManager2 != null)
						{
							karmaManager2.Save();
						}
					}
				}
				this.SaveSettings();
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				ushort lastUpdateID = netLobbyScreen.LastUpdateID;
				netLobbyScreen.LastUpdateID = lastUpdateID + 1;
			}
		}

		// Token: 0x0600350C RID: 13580 RVA: 0x0016FB28 File Offset: 0x0016DD28
		public void SaveSettings()
		{
			XDocument doc = new XDocument(new object[]
			{
				new XElement("serversettings")
			});
			doc.Root.SetAttributeValue("port", this.Port);
			if (this.QueryPort != 0)
			{
				doc.Root.SetAttributeValue("queryport", this.QueryPort);
			}
			doc.Root.SetAttributeValue("password", this.password ?? "");
			doc.Root.SetAttributeValue("enableupnp", this.EnableUPnP);
			doc.Root.SetAttributeValue("autorestart", this.autoRestart);
			doc.Root.SetAttributeValue("HiddenSubs", string.Join(",", this.HiddenSubs));
			doc.Root.SetAttributeValue("AllowedRandomMissionTypes", string.Join<Identifier>(",", this.AllowedRandomMissionTypes));
			doc.Root.SetAttributeValue("AllowedClientNameChars", string.Join(",", this.AllowedClientNameChars.Select(delegate(Range<int> c)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted<int>(c.Start);
				defaultInterpolatedStringHandler.AppendLiteral("-");
				defaultInterpolatedStringHandler.AppendFormatted<int>(c.End);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			})));
			SerializableProperty.SerializeProperties(this, doc.Root, true, false);
			doc.Root.Add(this.CampaignSettings.Save());
			doc.Root.SetAttributeValue("DisabledMonsters", string.Join(",", from kvp in this.MonsterEnabled
			where !kvp.Value
			select kvp.Key.Value));
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = true
			};
			using (XmlWriter writer = XmlWriter.Create("serversettings.xml", settings))
			{
				doc.SaveSafe(writer);
			}
			if (this.KarmaPreset == "custom")
			{
				GameServer server = GameMain.Server;
				if (server != null)
				{
					KarmaManager karmaManager = server.KarmaManager;
					if (karmaManager != null)
					{
						karmaManager.SaveCustomPreset();
					}
				}
			}
			GameServer server2 = GameMain.Server;
			if (server2 == null)
			{
				return;
			}
			KarmaManager karmaManager2 = server2.KarmaManager;
			if (karmaManager2 == null)
			{
				return;
			}
			karmaManager2.Save();
		}

		// Token: 0x0600350D RID: 13581 RVA: 0x0016FDAC File Offset: 0x0016DFAC
		private void LoadSettings()
		{
			XDocument doc = null;
			if (File.Exists("serversettings.xml"))
			{
				doc = XMLExtensions.TryLoadXml("serversettings.xml");
			}
			if (doc == null)
			{
				doc = new XDocument(new object[]
				{
					new XElement("serversettings")
				});
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, doc.Root);
			if (this.serverName.IsNullOrEmpty())
			{
				this.ServerName = doc.Root.GetAttributeString("name", "");
			}
			if (this.ServerName.Length > NetConfig.ServerNameMaxLength)
			{
				this.ServerName = this.ServerName.Substring(0, NetConfig.ServerNameMaxLength);
			}
			if (this.ServerMessageText.IsNullOrEmpty())
			{
				this.ServerMessageText = doc.Root.GetAttributeString("ServerMessage", "");
			}
			if (string.IsNullOrEmpty(doc.Root.GetAttributeString("losmode", "")))
			{
				this.LosMode = GameSettings.CurrentConfig.Graphics.LosMode;
			}
			if (string.IsNullOrEmpty(doc.Root.GetAttributeString("language", "")))
			{
				this.Language = ServerLanguageOptions.PickLanguage(GameSettings.CurrentConfig.Language);
			}
			this.AutoRestart = doc.Root.GetAttributeBool("autorestart", false);
			this.AllowSubVoting = (this.SubSelectionMode == SelectionMode.Vote);
			this.AllowModeVoting = (this.ModeSelectionMode == SelectionMode.Vote);
			GameMain.NetLobbyScreen.SetTraitorProbability(this.traitorProbability);
			this.HiddenSubs.UnionWith(doc.Root.GetAttributeStringArray("HiddenSubs", Array.Empty<string>(), true, false));
			if (this.HiddenSubs.Any<string>())
			{
				this.UpdateFlag(ServerSettings.NetFlags.HiddenSubs);
			}
			this.SelectedSubmarine = this.SelectNonHiddenSubmarine(this.SelectedSubmarine);
			string[] defaultAllowedClientNameChars = new string[]
			{
				"32-33",
				"38-46",
				"48-57",
				"65-90",
				"91",
				"93",
				"95-122",
				"192-255",
				"384-591",
				"1024-1279",
				"4352-4607",
				"44032-55215",
				"19968-21327",
				"21329-40959",
				"13312-19903",
				"131072-173791",
				"173824-178207",
				"178208-183983",
				"63744-64255",
				"194560-195103"
			};
			string[] allowedClientNameCharsStr = doc.Root.GetAttributeStringArray("AllowedClientNameChars", defaultAllowedClientNameChars, true, false);
			if (doc.Root.GetAttributeString("AllowedClientNameChars", "") == "65-90,97-122,48-59")
			{
				allowedClientNameCharsStr = defaultAllowedClientNameChars;
			}
			foreach (string allowedClientNameCharRange in allowedClientNameCharsStr)
			{
				string[] splitRange = allowedClientNameCharRange.Split('-', StringSplitOptions.None);
				if (splitRange.Length == 0 || splitRange.Length > 2)
				{
					DebugConsole.ThrowError("Error in server settings - " + allowedClientNameCharRange + " is not a valid range for characters allowed in client names.", null, null, false, false);
				}
				else
				{
					int min = -1;
					if (!int.TryParse(splitRange[0], out min))
					{
						DebugConsole.ThrowError("Error in server settings - " + allowedClientNameCharRange + " is not a valid range for characters allowed in client names.", null, null, false, false);
					}
					else
					{
						int max = min;
						if (splitRange.Length == 2 && !int.TryParse(splitRange[1], out max))
						{
							DebugConsole.ThrowError("Error in server settings - " + allowedClientNameCharRange + " is not a valid range for characters allowed in client names.", null, null, false, false);
						}
						else
						{
							if (min > max)
							{
								int num = max;
								max = min;
								min = num;
							}
							if (min > -1 && max > -1)
							{
								this.AllowedClientNameChars.Add(new Range<int>(min, max));
							}
						}
					}
				}
			}
			this.AllowedRandomMissionTypes = doc.Root.GetAttributeIdentifierArray("AllowedRandomMissionTypes", MissionPrefab.GetAllMultiplayerSelectableMissionTypes().ToArray<Identifier>(), true).ToList<Identifier>();
			GameMain.NetLobbyScreen.SelectedModeIdentifier = this.GameModeIdentifier;
			if (this.AllowedRandomMissionTypes.Contains(Tags.MissionTypeAll))
			{
				this.AllowedRandomMissionTypes = MissionPrefab.GetAllMultiplayerSelectableMissionTypes().ToList<Identifier>();
			}
			this.AllowedRandomMissionTypes = this.AllowedRandomMissionTypes.Distinct<Identifier>().ToList<Identifier>();
			this.ValidateMissionTypes();
			GameMain.NetLobbyScreen.SetBotSpawnMode(this.BotSpawnMode);
			GameMain.NetLobbyScreen.SetBotCount(this.BotCount);
			if (this.MonsterEnabled == null)
			{
				this.MonsterEnabled = (from p in CharacterPrefab.Prefabs
				select new ValueTuple<Identifier, bool>(p.Identifier, true)).ToDictionary<Identifier, bool>();
			}
			Identifier[] disabledMonsters = doc.Root.GetAttributeIdentifierArray("DisabledMonsters", Array.Empty<Identifier>(), true);
			foreach (Identifier disabledMonster in disabledMonsters)
			{
				if (this.MonsterEnabled.ContainsKey(disabledMonster))
				{
					this.MonsterEnabled[disabledMonster] = false;
				}
			}
			foreach (XElement element in doc.Root.Elements())
			{
				Identifier identifier = element.Name.ToIdentifier<XName>();
				if (identifier == "CampaignSettings")
				{
					this.CampaignSettings = new CampaignSettings(element);
				}
			}
			HashSet<Identifier> selectedCoalitionPerks = this.SelectedCoalitionPerks.ToHashSet<Identifier>();
			HashSet<Identifier> selectedSeparatistsPerks = this.SelectedSeparatistsPerks.ToHashSet<Identifier>();
			foreach (DisembarkPerkPrefab prefab in DisembarkPerkPrefab.Prefabs)
			{
				if (prefab.Cost == 0)
				{
					selectedSeparatistsPerks.Add(prefab.Identifier);
					selectedCoalitionPerks.Add(prefab.Identifier);
				}
			}
			this.SelectedCoalitionPerks = selectedCoalitionPerks.ToArray<Identifier>();
			this.SelectedSeparatistsPerks = selectedSeparatistsPerks.ToArray<Identifier>();
		}

		// Token: 0x0600350E RID: 13582 RVA: 0x00170368 File Offset: 0x0016E568
		public string SelectNonHiddenSubmarine(string current = null)
		{
			if (current == null)
			{
				current = GameMain.NetLobbyScreen.SelectedSub.Name;
			}
			if (!this.HiddenSubs.Contains(current))
			{
				return current;
			}
			SubmarineInfo[] candidates = (from s in GameMain.NetLobbyScreen.GetSubList()
			where !this.HiddenSubs.Contains(s.Name)
			select s).ToArray<SubmarineInfo>();
			if (candidates.Any<SubmarineInfo>())
			{
				GameMain.NetLobbyScreen.SelectedSub = candidates.GetRandom(Rand.RandSync.Unsynced);
				return GameMain.NetLobbyScreen.SelectedSub.Name;
			}
			this.HiddenSubs.Remove(current);
			return current;
		}

		// Token: 0x0600350F RID: 13583 RVA: 0x001703F4 File Offset: 0x0016E5F4
		public void LoadClientPermissions()
		{
			this.ClientPermissions.Clear();
			if (!File.Exists(ServerSettings.ClientPermissionsFile))
			{
				return;
			}
			XDocument doc = XMLExtensions.TryLoadXml(ServerSettings.ClientPermissionsFile);
			if (doc == null)
			{
				return;
			}
			foreach (XElement clientElement in doc.Root.Elements())
			{
				string clientName = clientElement.GetAttributeString("name", "");
				string text;
				if ((text = clientElement.GetAttributeString("address", null)) == null)
				{
					text = (clientElement.GetAttributeString("endpoint", null) ?? clientElement.GetAttributeString("ip", ""));
				}
				string addressStr = text;
				string accountIdStr = clientElement.GetAttributeString("accountid", null) ?? clientElement.GetAttributeString("steamid", "");
				if (string.IsNullOrWhiteSpace(clientName))
				{
					DebugConsole.ThrowError("Error in " + ServerSettings.ClientPermissionsFile + " - all clients must have a name.", null, null, false, false);
				}
				else if (string.IsNullOrWhiteSpace(addressStr) && string.IsNullOrWhiteSpace(accountIdStr))
				{
					DebugConsole.ThrowError("Error in " + ServerSettings.ClientPermissionsFile + " - all clients must have an endpoint or a Steam ID.", null, null, false, false);
				}
				else
				{
					ClientPermissions permissions = Barotrauma.Networking.ClientPermissions.None;
					HashSet<DebugConsole.Command> permittedCommands = new HashSet<DebugConsole.Command>();
					if (clientElement.Attribute("preset") != null)
					{
						goto IL_29F;
					}
					string permissionsStr = clientElement.GetAttributeString("permissions", "");
					if (permissionsStr.Equals("all", StringComparison.OrdinalIgnoreCase))
					{
						using (IEnumerator enumerator2 = Enum.GetValues(typeof(ClientPermissions)).GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								object obj = enumerator2.Current;
								ClientPermissions permission = (ClientPermissions)obj;
								permissions |= permission;
							}
							goto IL_1DD;
						}
					}
					if (!Enum.TryParse<ClientPermissions>(permissionsStr, out permissions))
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error in ",
							ServerSettings.ClientPermissionsFile,
							" - \"",
							permissionsStr,
							"\" is not a valid client permission."
						}), null, null, false, false);
						continue;
					}
					IL_1DD:
					if (permissions.HasFlag(Barotrauma.Networking.ClientPermissions.ConsoleCommands))
					{
						using (IEnumerator<XElement> enumerator3 = clientElement.Elements().GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								XElement commandElement = enumerator3.Current;
								if (commandElement.Name.ToString().Equals("command", StringComparison.OrdinalIgnoreCase))
								{
									string commandName = commandElement.GetAttributeString("name", "");
									DebugConsole.Command command = DebugConsole.FindCommand(commandName);
									if (command == null)
									{
										command = new DebugConsole.Command(commandName, "", delegate(string[] _)
										{
										}, null, true);
									}
									permittedCommands.Add(command);
								}
							}
							goto IL_330;
						}
						goto IL_29F;
					}
					IL_330:
					if (permissions.HasFlag(Barotrauma.Networking.ClientPermissions.ConsoleCommands))
					{
						foreach (XElement commandElement2 in clientElement.Elements())
						{
							if (commandElement2.Name.ToString().Equals("command", StringComparison.OrdinalIgnoreCase))
							{
								string commandName2 = commandElement2.GetAttributeString("name", "");
								DebugConsole.Command command2 = DebugConsole.FindCommand(commandName2);
								if (command2 == null)
								{
									DebugConsole.ThrowError(string.Concat(new string[]
									{
										"Error in ",
										ServerSettings.ClientPermissionsFile,
										" - \"",
										commandName2,
										"\" is not a valid console command."
									}), null, null, false, false);
								}
								else
								{
									permittedCommands.Add(command2);
								}
							}
						}
					}
					if (!string.IsNullOrEmpty(accountIdStr))
					{
						AccountId accountId;
						if (AccountId.Parse(accountIdStr).TryUnwrap(out accountId))
						{
							this.ClientPermissions.Add(new ServerSettings.SavedClientPermission(clientName, accountId, permissions, permittedCommands));
							continue;
						}
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error in ",
							ServerSettings.ClientPermissionsFile,
							" - \"",
							accountIdStr,
							"\" is not a valid account ID."
						}), null, null, false, false);
						continue;
					}
					else
					{
						Address address;
						if (Address.Parse(addressStr).TryUnwrap(out address))
						{
							this.ClientPermissions.Add(new ServerSettings.SavedClientPermission(clientName, address, permissions, permittedCommands));
							continue;
						}
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error in ",
							ServerSettings.ClientPermissionsFile,
							" - \"",
							addressStr,
							"\" is not a valid endpoint."
						}), null, null, false, false);
						continue;
					}
					IL_29F:
					string presetName = clientElement.GetAttributeString("preset", "");
					PermissionPreset preset = PermissionPreset.List.Find((PermissionPreset p) => p.DisplayName == presetName);
					if (preset == null)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Failed to restore saved permissions to the client \"",
							clientName,
							"\". Permission preset \"",
							presetName,
							"\" not found."
						}), null, null, false, false);
						break;
					}
					permissions = preset.Permissions;
					permittedCommands = preset.PermittedCommands.ToHashSet<DebugConsole.Command>();
					goto IL_330;
				}
			}
		}

		// Token: 0x06003510 RID: 13584 RVA: 0x00170960 File Offset: 0x0016EB60
		public void SaveClientPermissions()
		{
			GameServer.Log("Saving client permissions", ServerLog.MessageType.ServerMessage);
			XDocument doc = new XDocument(new object[]
			{
				new XElement("ClientPermissions")
			});
			using (List<ServerSettings.SavedClientPermission>.Enumerator enumerator = this.ClientPermissions.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ServerSettings.SavedClientPermission clientPermission = enumerator.Current;
					PermissionPreset matchingPreset = PermissionPreset.List.Find((PermissionPreset p) => p.MatchesPermissions(clientPermission.Permissions, clientPermission.PermittedCommands));
					if (matchingPreset == null || !(matchingPreset.Identifier == "None"))
					{
						XElement clientElement = new XElement("Client", new XAttribute("name", clientPermission.Name));
						AccountId accountId;
						clientElement.Add(clientPermission.AddressOrAccountId.TryGet(out accountId) ? new XAttribute("accountid", accountId.StringRepresentation) : new XAttribute("address", ((T)clientPermission.AddressOrAccountId).StringRepresentation));
						clientElement.Add((matchingPreset == null) ? new XAttribute("permissions", clientPermission.Permissions.ToString()) : new XAttribute("preset", matchingPreset.DisplayName));
						if (clientPermission.Permissions.HasFlag(Barotrauma.Networking.ClientPermissions.ConsoleCommands))
						{
							foreach (DebugConsole.Command command in clientPermission.PermittedCommands)
							{
								clientElement.Add(new XElement("command", new XAttribute("name", command.Names[0])));
							}
						}
						doc.Root.Add(clientElement);
					}
				}
			}
			try
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.NewLineOnAttributes = true;
				using (XmlWriter writer = XmlWriter.Create(ServerSettings.ClientPermissionsFile, settings))
				{
					doc.SaveSafe(writer);
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Saving client permissions to " + ServerSettings.ClientPermissionsFile + " failed", e, null, false, false);
			}
		}

		// Token: 0x17000E95 RID: 3733
		// (get) Token: 0x06003511 RID: 13585 RVA: 0x00170C30 File Offset: 0x0016EE30
		public string Name
		{
			get
			{
				return "ServerSettings";
			}
		}

		// Token: 0x17000E96 RID: 3734
		// (get) Token: 0x06003512 RID: 13586 RVA: 0x00170C37 File Offset: 0x0016EE37
		// (set) Token: 0x06003513 RID: 13587 RVA: 0x00170C3F File Offset: 0x0016EE3F
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06003514 RID: 13588 RVA: 0x00170C48 File Offset: 0x0016EE48
		private void InitProjSpecific()
		{
			this.LoadSettings();
			this.LoadClientPermissions();
		}

		// Token: 0x06003515 RID: 13589 RVA: 0x00170C58 File Offset: 0x0016EE58
		public ServerSettings(NetworkMember networkMember, string serverName, int port, int queryPort, int maxPlayers, bool isPublic, bool enableUPnP, IPAddress listenIp)
		{
			this.ServerLog = new ServerLog(serverName);
			this.BanList = new BanList();
			this.ExtraCargo = new Dictionary<ItemPrefab, int>();
			this.HiddenSubs = new HashSet<string>();
			PermissionPreset.List.Clear();
			PermissionPreset.LoadAll(ServerSettings.PermissionPresetFile);
			PermissionPreset.LoadAll(ServerSettings.PermissionPresetFileCustom);
			this.InitProjSpecific();
			this.ServerName = serverName;
			this.ListenIPAddress = listenIp;
			this.Port = port;
			this.QueryPort = queryPort;
			this.EnableUPnP = enableUPnP;
			this.MaxPlayers = maxPlayers;
			this.IsPublic = isPublic;
			this.netProperties = new Dictionary<uint, ServerSettings.NetPropertyData>();
			using (MD5 md5 = MD5.Create())
			{
				List<SerializableProperty> saveProperties = SerializableProperty.GetProperties<Serialize>(this);
				foreach (SerializableProperty property in saveProperties)
				{
					string typeName = SerializableProperty.GetSupportedTypeName(property.PropertyType);
					if ((typeName != null || property.PropertyType.IsEnum) && property.GetAttribute<DoNotSyncOverNetwork>() == null)
					{
						ServerSettings.NetPropertyData netPropertyData = new ServerSettings.NetPropertyData(this, property, typeName);
						uint key = ToolBoxCore.IdentifierToUint32Hash(netPropertyData.Name, md5);
						if (key == 0U)
						{
							key += 1U;
						}
						if (this.netProperties.ContainsKey(key))
						{
							string[] array = new string[7];
							array[0] = "Hashing collision in ServerSettings.netProperties: ";
							int num = 1;
							ServerSettings.NetPropertyData netPropertyData3 = this.netProperties[key];
							array[num] = ((netPropertyData3 != null) ? netPropertyData3.ToString() : null);
							array[2] = " has same key as ";
							array[3] = property.Name;
							array[4] = " (";
							array[5] = key.ToString();
							array[6] = ")";
							throw new Exception(string.Concat(array));
						}
						this.netProperties.Add(key, netPropertyData);
					}
				}
				List<SerializableProperty> karmaProperties = SerializableProperty.GetProperties<Serialize>(networkMember.KarmaManager);
				foreach (SerializableProperty property2 in karmaProperties)
				{
					object value = property2.GetValue(networkMember.KarmaManager);
					if (value != null)
					{
						string typeName2 = SerializableProperty.GetSupportedTypeName(value.GetType());
						if (typeName2 != null || property2.PropertyType.IsEnum)
						{
							ServerSettings.NetPropertyData netPropertyData2 = new ServerSettings.NetPropertyData(networkMember.KarmaManager, property2, typeName2);
							uint key2 = ToolBoxCore.IdentifierToUint32Hash(netPropertyData2.Name, md5);
							if (this.netProperties.ContainsKey(key2))
							{
								string[] array2 = new string[7];
								array2[0] = "Hashing collision in ServerSettings.netProperties: ";
								int num2 = 1;
								ServerSettings.NetPropertyData netPropertyData4 = this.netProperties[key2];
								array2[num2] = ((netPropertyData4 != null) ? netPropertyData4.ToString() : null);
								array2[2] = " has same key as ";
								array2[3] = property2.Name;
								array2[4] = " (";
								array2[5] = key2.ToString();
								array2[6] = ")";
								throw new Exception(string.Concat(array2));
							}
							this.netProperties.Add(key2, netPropertyData2);
						}
					}
				}
			}
		}

		// Token: 0x17000E97 RID: 3735
		// (get) Token: 0x06003516 RID: 13590 RVA: 0x00171074 File Offset: 0x0016F274
		// (set) Token: 0x06003517 RID: 13591 RVA: 0x0017107C File Offset: 0x0016F27C
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string ServerName
		{
			get
			{
				return this.serverName;
			}
			set
			{
				string newName = value;
				if (newName.Length > NetConfig.ServerNameMaxLength)
				{
					newName = newName.Substring(0, NetConfig.ServerNameMaxLength);
				}
				if (this.serverName == newName)
				{
					return;
				}
				if (newName.IsNullOrWhiteSpace())
				{
					return;
				}
				this.serverName = newName;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000E98 RID: 3736
		// (get) Token: 0x06003518 RID: 13592 RVA: 0x001710CB File Offset: 0x0016F2CB
		// (set) Token: 0x06003519 RID: 13593 RVA: 0x001710D4 File Offset: 0x0016F2D4
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string ServerMessageText
		{
			get
			{
				return this.serverMessageText;
			}
			set
			{
				string val = value;
				if (val.Length > NetConfig.ServerMessageMaxLength)
				{
					val = val.Substring(0, NetConfig.ServerMessageMaxLength);
				}
				if (this.serverMessageText == val)
				{
					return;
				}
				GameServer server = GameMain.Server;
				if (server != null)
				{
					server.SendChatMessage(TextManager.AddPunctuation(':', new LocalizedString[]
					{
						TextManager.Get("servermotd"),
						val
					}).Value, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
				}
				this.serverMessageText = val;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000E99 RID: 3737
		// (get) Token: 0x0600351A RID: 13594 RVA: 0x0017115C File Offset: 0x0016F35C
		// (set) Token: 0x0600351B RID: 13595 RVA: 0x00171164 File Offset: 0x0016F364
		public Dictionary<Identifier, bool> MonsterEnabled { get; private set; }

		// Token: 0x17000E9A RID: 3738
		// (get) Token: 0x0600351C RID: 13596 RVA: 0x0017116D File Offset: 0x0016F36D
		// (set) Token: 0x0600351D RID: 13597 RVA: 0x00171175 File Offset: 0x0016F375
		public Dictionary<ItemPrefab, int> ExtraCargo { get; private set; }

		// Token: 0x17000E9B RID: 3739
		// (get) Token: 0x0600351E RID: 13598 RVA: 0x0017117E File Offset: 0x0016F37E
		// (set) Token: 0x0600351F RID: 13599 RVA: 0x00171186 File Offset: 0x0016F386
		public HashSet<string> HiddenSubs { get; set; }

		// Token: 0x17000E9C RID: 3740
		// (get) Token: 0x06003520 RID: 13600 RVA: 0x0017118F File Offset: 0x0016F38F
		// (set) Token: 0x06003521 RID: 13601 RVA: 0x00171197 File Offset: 0x0016F397
		public List<ServerSettings.SavedClientPermission> ClientPermissions { get; private set; } = new List<ServerSettings.SavedClientPermission>();

		// Token: 0x17000E9D RID: 3741
		// (get) Token: 0x06003522 RID: 13602 RVA: 0x001711A0 File Offset: 0x0016F3A0
		// (set) Token: 0x06003523 RID: 13603 RVA: 0x001711A8 File Offset: 0x0016F3A8
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool IsPublic { get; set; }

		// Token: 0x17000E9E RID: 3742
		// (get) Token: 0x06003524 RID: 13604 RVA: 0x001711B1 File Offset: 0x0016F3B1
		// (set) Token: 0x06003525 RID: 13605 RVA: 0x001711B9 File Offset: 0x0016F3B9
		[Serialize(20, IsPropertySaveable.Yes, "", "", false)]
		public int TickRate
		{
			get
			{
				return this.tickRate;
			}
			set
			{
				this.tickRate = MathHelper.Clamp(value, 1, 60);
			}
		}

		// Token: 0x17000E9F RID: 3743
		// (get) Token: 0x06003526 RID: 13606 RVA: 0x001711CA File Offset: 0x0016F3CA
		// (set) Token: 0x06003527 RID: 13607 RVA: 0x001711D2 File Offset: 0x0016F3D2
		[Serialize(150, IsPropertySaveable.Yes, "Maximum amount of lag compensation for firing weapons, in milliseconds. E.g. when a client fires a gun, the server will be notified about it with some latency, and checks if it hit anything in the past (at the time the shot was taken), up to this limit. The largest allowed lag compensation is 500 milliseconds.", "", false)]
		public int MaxLagCompensation
		{
			get
			{
				return this.maxLagCompensation;
			}
			set
			{
				this.maxLagCompensation = MathHelper.Clamp(value, 0, 500);
			}
		}

		// Token: 0x17000EA0 RID: 3744
		// (get) Token: 0x06003528 RID: 13608 RVA: 0x001711E6 File Offset: 0x0016F3E6
		public float MaxLagCompensationSeconds
		{
			get
			{
				return (float)this.maxLagCompensation / 1000f;
			}
		}

		// Token: 0x17000EA1 RID: 3745
		// (get) Token: 0x06003529 RID: 13609 RVA: 0x001711F5 File Offset: 0x0016F3F5
		// (set) Token: 0x0600352A RID: 13610 RVA: 0x001711FD File Offset: 0x0016F3FD
		[Serialize(true, IsPropertySaveable.Yes, "Do clients need to be authenticated (e.g. based on Steam ID or an EGS ownership token). Can be disabled if you for example want to play the game in a local network without a connection to external services.", "", false)]
		public bool RequireAuthentication { get; set; }

		// Token: 0x17000EA2 RID: 3746
		// (get) Token: 0x0600352B RID: 13611 RVA: 0x00171206 File Offset: 0x0016F406
		// (set) Token: 0x0600352C RID: 13612 RVA: 0x0017120E File Offset: 0x0016F40E
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool RandomizeSeed { get; set; }

		// Token: 0x17000EA3 RID: 3747
		// (get) Token: 0x0600352D RID: 13613 RVA: 0x00171217 File Offset: 0x0016F417
		// (set) Token: 0x0600352E RID: 13614 RVA: 0x0017121F File Offset: 0x0016F41F
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool UseRespawnShuttle { get; private set; }

		// Token: 0x17000EA4 RID: 3748
		// (get) Token: 0x0600352F RID: 13615 RVA: 0x00171228 File Offset: 0x0016F428
		// (set) Token: 0x06003530 RID: 13616 RVA: 0x00171230 File Offset: 0x0016F430
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float RespawnInterval { get; private set; }

		// Token: 0x17000EA5 RID: 3749
		// (get) Token: 0x06003531 RID: 13617 RVA: 0x00171239 File Offset: 0x0016F439
		// (set) Token: 0x06003532 RID: 13618 RVA: 0x00171241 File Offset: 0x0016F441
		[Serialize(180f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxTransportTime { get; private set; }

		// Token: 0x17000EA6 RID: 3750
		// (get) Token: 0x06003533 RID: 13619 RVA: 0x0017124A File Offset: 0x0016F44A
		// (set) Token: 0x06003534 RID: 13620 RVA: 0x00171252 File Offset: 0x0016F452
		[Serialize(0.2f, IsPropertySaveable.Yes, "", "", false)]
		public float MinRespawnRatio { get; private set; }

		// Token: 0x17000EA7 RID: 3751
		// (get) Token: 0x06003535 RID: 13621 RVA: 0x0017125B File Offset: 0x0016F45B
		// (set) Token: 0x06003536 RID: 13622 RVA: 0x00171263 File Offset: 0x0016F463
		[Serialize(20f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillLossPercentageOnDeath { get; private set; }

		// Token: 0x17000EA8 RID: 3752
		// (get) Token: 0x06003537 RID: 13623 RVA: 0x0017126C File Offset: 0x0016F46C
		// (set) Token: 0x06003538 RID: 13624 RVA: 0x00171274 File Offset: 0x0016F474
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillLossPercentageOnImmediateRespawn { get; private set; }

		// Token: 0x17000EA9 RID: 3753
		// (get) Token: 0x06003539 RID: 13625 RVA: 0x0017127D File Offset: 0x0016F47D
		// (set) Token: 0x0600353A RID: 13626 RVA: 0x00171285 File Offset: 0x0016F485
		[Serialize(100f, IsPropertySaveable.Yes, "", "", false)]
		public float ReplaceCostPercentage { get; private set; }

		// Token: 0x17000EAA RID: 3754
		// (get) Token: 0x0600353B RID: 13627 RVA: 0x0017128E File Offset: 0x0016F48E
		// (set) Token: 0x0600353C RID: 13628 RVA: 0x00171296 File Offset: 0x0016F496
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowBotTakeoverOnPermadeath { get; private set; }

		// Token: 0x17000EAB RID: 3755
		// (get) Token: 0x0600353D RID: 13629 RVA: 0x0017129F File Offset: 0x0016F49F
		// (set) Token: 0x0600353E RID: 13630 RVA: 0x001712A7 File Offset: 0x0016F4A7
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool IronmanMode { get; private set; }

		// Token: 0x17000EAC RID: 3756
		// (get) Token: 0x0600353F RID: 13631 RVA: 0x001712B0 File Offset: 0x0016F4B0
		public bool IronmanModeActive
		{
			get
			{
				return this.IronmanMode && this.respawnMode == RespawnMode.Permadeath;
			}
		}

		// Token: 0x17000EAD RID: 3757
		// (get) Token: 0x06003540 RID: 13632 RVA: 0x001712C5 File Offset: 0x0016F4C5
		// (set) Token: 0x06003541 RID: 13633 RVA: 0x001712CD File Offset: 0x0016F4CD
		[Serialize(60f, IsPropertySaveable.Yes, "", "", false)]
		public float AutoRestartInterval { get; set; }

		// Token: 0x17000EAE RID: 3758
		// (get) Token: 0x06003542 RID: 13634 RVA: 0x001712D6 File Offset: 0x0016F4D6
		// (set) Token: 0x06003543 RID: 13635 RVA: 0x001712DE File Offset: 0x0016F4DE
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool StartWhenClientsReady { get; set; }

		// Token: 0x17000EAF RID: 3759
		// (get) Token: 0x06003544 RID: 13636 RVA: 0x001712E7 File Offset: 0x0016F4E7
		// (set) Token: 0x06003545 RID: 13637 RVA: 0x001712EF File Offset: 0x0016F4EF
		[Serialize(PvpTeamSelectionMode.PlayerPreference, IsPropertySaveable.Yes, "", "", false)]
		public PvpTeamSelectionMode PvpTeamSelectionMode { get; private set; }

		// Token: 0x17000EB0 RID: 3760
		// (get) Token: 0x06003546 RID: 13638 RVA: 0x001712F8 File Offset: 0x0016F4F8
		// (set) Token: 0x06003547 RID: 13639 RVA: 0x00171300 File Offset: 0x0016F500
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		public int PvpAutoBalanceThreshold { get; private set; }

		// Token: 0x17000EB1 RID: 3761
		// (get) Token: 0x06003548 RID: 13640 RVA: 0x00171309 File Offset: 0x0016F509
		// (set) Token: 0x06003549 RID: 13641 RVA: 0x00171311 File Offset: 0x0016F511
		[Serialize(0.8f, IsPropertySaveable.Yes, "", "", false)]
		public float StartWhenClientsReadyRatio { get; private set; }

		// Token: 0x17000EB2 RID: 3762
		// (get) Token: 0x0600354A RID: 13642 RVA: 0x0017131A File Offset: 0x0016F51A
		// (set) Token: 0x0600354B RID: 13643 RVA: 0x00171322 File Offset: 0x0016F522
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float PvPStunResist { get; private set; }

		// Token: 0x17000EB3 RID: 3763
		// (get) Token: 0x0600354C RID: 13644 RVA: 0x0017132B File Offset: 0x0016F52B
		// (set) Token: 0x0600354D RID: 13645 RVA: 0x00171333 File Offset: 0x0016F533
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool PvPSpawnMonsters { get; private set; }

		// Token: 0x17000EB4 RID: 3764
		// (get) Token: 0x0600354E RID: 13646 RVA: 0x0017133C File Offset: 0x0016F53C
		// (set) Token: 0x0600354F RID: 13647 RVA: 0x00171344 File Offset: 0x0016F544
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool PvPSpawnWrecks { get; private set; }

		// Token: 0x17000EB5 RID: 3765
		// (get) Token: 0x06003550 RID: 13648 RVA: 0x0017134D File Offset: 0x0016F54D
		// (set) Token: 0x06003551 RID: 13649 RVA: 0x00171355 File Offset: 0x0016F555
		[Serialize("Random", IsPropertySaveable.Yes, "", "", false)]
		public Identifier Biome { get; private set; }

		// Token: 0x17000EB6 RID: 3766
		// (get) Token: 0x06003552 RID: 13650 RVA: 0x0017135E File Offset: 0x0016F55E
		// (set) Token: 0x06003553 RID: 13651 RVA: 0x00171366 File Offset: 0x0016F566
		[Serialize("Random", IsPropertySaveable.Yes, "", "", false)]
		public Identifier SelectedOutpostName { get; private set; }

		// Token: 0x17000EB7 RID: 3767
		// (get) Token: 0x06003554 RID: 13652 RVA: 0x0017136F File Offset: 0x0016F56F
		// (set) Token: 0x06003555 RID: 13653 RVA: 0x00171377 File Offset: 0x0016F577
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowSpectating
		{
			get
			{
				return this.allowSpectating;
			}
			private set
			{
				if (this.allowSpectating == value)
				{
					return;
				}
				this.allowSpectating = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EB8 RID: 3768
		// (get) Token: 0x06003556 RID: 13654 RVA: 0x00171391 File Offset: 0x0016F591
		// (set) Token: 0x06003557 RID: 13655 RVA: 0x00171399 File Offset: 0x0016F599
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowAFK
		{
			get
			{
				return this.allowAFK;
			}
			private set
			{
				if (this.allowAFK == value)
				{
					return;
				}
				this.allowAFK = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EB9 RID: 3769
		// (get) Token: 0x06003558 RID: 13656 RVA: 0x001713B3 File Offset: 0x0016F5B3
		// (set) Token: 0x06003559 RID: 13657 RVA: 0x001713BB File Offset: 0x0016F5BB
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool SaveServerLogs { get; private set; }

		// Token: 0x17000EBA RID: 3770
		// (get) Token: 0x0600355A RID: 13658 RVA: 0x001713C4 File Offset: 0x0016F5C4
		// (set) Token: 0x0600355B RID: 13659 RVA: 0x001713CC File Offset: 0x0016F5CC
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowModDownloads { get; private set; } = true;

		// Token: 0x17000EBB RID: 3771
		// (get) Token: 0x0600355C RID: 13660 RVA: 0x001713D5 File Offset: 0x0016F5D5
		// (set) Token: 0x0600355D RID: 13661 RVA: 0x001713DD File Offset: 0x0016F5DD
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowFileTransfers { get; private set; }

		// Token: 0x17000EBC RID: 3772
		// (get) Token: 0x0600355E RID: 13662 RVA: 0x001713E6 File Offset: 0x0016F5E6
		// (set) Token: 0x0600355F RID: 13663 RVA: 0x001713EE File Offset: 0x0016F5EE
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowRemoteCampaignInteractions { get; private set; }

		// Token: 0x17000EBD RID: 3773
		// (get) Token: 0x06003560 RID: 13664 RVA: 0x001713F7 File Offset: 0x0016F5F7
		// (set) Token: 0x06003561 RID: 13665 RVA: 0x001713FF File Offset: 0x0016F5FF
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool VoiceChatEnabled
		{
			get
			{
				return this.voiceChatEnabled;
			}
			set
			{
				if (this.voiceChatEnabled == value)
				{
					return;
				}
				this.voiceChatEnabled = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EBE RID: 3774
		// (get) Token: 0x06003562 RID: 13666 RVA: 0x00171419 File Offset: 0x0016F619
		// (set) Token: 0x06003563 RID: 13667 RVA: 0x00171421 File Offset: 0x0016F621
		[Serialize(PlayStyle.Casual, IsPropertySaveable.Yes, "", "", false)]
		public PlayStyle PlayStyle
		{
			get
			{
				return this.playstyleSelection;
			}
			set
			{
				this.playstyleSelection = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EBF RID: 3775
		// (get) Token: 0x06003564 RID: 13668 RVA: 0x00171431 File Offset: 0x0016F631
		// (set) Token: 0x06003565 RID: 13669 RVA: 0x00171439 File Offset: 0x0016F639
		[Serialize(LosMode.Transparent, IsPropertySaveable.Yes, "", "", false)]
		public LosMode LosMode { get; set; }

		// Token: 0x17000EC0 RID: 3776
		// (get) Token: 0x06003566 RID: 13670 RVA: 0x00171442 File Offset: 0x0016F642
		// (set) Token: 0x06003567 RID: 13671 RVA: 0x0017144A File Offset: 0x0016F64A
		[Serialize(EnemyHealthBarMode.ShowAll, IsPropertySaveable.Yes, "", "", false)]
		public EnemyHealthBarMode ShowEnemyHealthBars { get; set; }

		// Token: 0x17000EC1 RID: 3777
		// (get) Token: 0x06003568 RID: 13672 RVA: 0x00171453 File Offset: 0x0016F653
		// (set) Token: 0x06003569 RID: 13673 RVA: 0x00171460 File Offset: 0x0016F660
		[Serialize(800, IsPropertySaveable.Yes, "", "", false)]
		public int LinesPerLogFile
		{
			get
			{
				return this.ServerLog.LinesPerFile;
			}
			set
			{
				this.ServerLog.LinesPerFile = value;
			}
		}

		// Token: 0x17000EC2 RID: 3778
		// (get) Token: 0x0600356A RID: 13674 RVA: 0x0017146E File Offset: 0x0016F66E
		// (set) Token: 0x0600356B RID: 13675 RVA: 0x00171476 File Offset: 0x0016F676
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool AutoRestart
		{
			get
			{
				return this.autoRestart;
			}
			set
			{
				this.autoRestart = value;
				this.AutoRestartTimer = (this.autoRestart ? this.AutoRestartInterval : 0f);
			}
		}

		// Token: 0x17000EC3 RID: 3779
		// (get) Token: 0x0600356C RID: 13676 RVA: 0x0017149A File Offset: 0x0016F69A
		public bool HasPassword
		{
			get
			{
				return !string.IsNullOrEmpty(this.password);
			}
		}

		// Token: 0x17000EC4 RID: 3780
		// (get) Token: 0x0600356D RID: 13677 RVA: 0x001714AA File Offset: 0x0016F6AA
		// (set) Token: 0x0600356E RID: 13678 RVA: 0x001714B2 File Offset: 0x0016F6B2
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowVoteKick { get; set; }

		// Token: 0x17000EC5 RID: 3781
		// (get) Token: 0x0600356F RID: 13679 RVA: 0x001714BB File Offset: 0x0016F6BB
		// (set) Token: 0x06003570 RID: 13680 RVA: 0x001714C3 File Offset: 0x0016F6C3
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowEndVoting { get; set; }

		// Token: 0x17000EC6 RID: 3782
		// (get) Token: 0x06003571 RID: 13681 RVA: 0x001714CC File Offset: 0x0016F6CC
		// (set) Token: 0x06003572 RID: 13682 RVA: 0x001714D4 File Offset: 0x0016F6D4
		[Serialize(RespawnMode.MidRound, IsPropertySaveable.Yes, "", "", false)]
		public RespawnMode RespawnMode
		{
			get
			{
				return this.respawnMode;
			}
			set
			{
				if (this.respawnMode == value)
				{
					return;
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.GameStarted && networkMember.IsServer)
				{
					return;
				}
				this.respawnMode = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EC7 RID: 3783
		// (get) Token: 0x06003573 RID: 13683 RVA: 0x00171513 File Offset: 0x0016F713
		// (set) Token: 0x06003574 RID: 13684 RVA: 0x0017151B File Offset: 0x0016F71B
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int BotCount { get; set; }

		// Token: 0x17000EC8 RID: 3784
		// (get) Token: 0x06003575 RID: 13685 RVA: 0x00171524 File Offset: 0x0016F724
		// (set) Token: 0x06003576 RID: 13686 RVA: 0x0017152C File Offset: 0x0016F72C
		[Serialize(16, IsPropertySaveable.Yes, "", "", false)]
		public int MaxBotCount { get; set; }

		// Token: 0x17000EC9 RID: 3785
		// (get) Token: 0x06003577 RID: 13687 RVA: 0x00171535 File Offset: 0x0016F735
		// (set) Token: 0x06003578 RID: 13688 RVA: 0x0017153D File Offset: 0x0016F73D
		[Serialize(BotSpawnMode.Normal, IsPropertySaveable.Yes, "", "", false)]
		public BotSpawnMode BotSpawnMode { get; set; }

		// Token: 0x17000ECA RID: 3786
		// (get) Token: 0x06003579 RID: 13689 RVA: 0x00171546 File Offset: 0x0016F746
		// (set) Token: 0x0600357A RID: 13690 RVA: 0x0017154E File Offset: 0x0016F74E
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DisableBotConversations { get; set; }

		// Token: 0x17000ECB RID: 3787
		// (get) Token: 0x0600357B RID: 13691 RVA: 0x00171557 File Offset: 0x0016F757
		// (set) Token: 0x0600357C RID: 13692 RVA: 0x0017155F File Offset: 0x0016F75F
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float SelectedLevelDifficulty
		{
			get
			{
				return this.selectedLevelDifficulty;
			}
			set
			{
				this.selectedLevelDifficulty = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000ECC RID: 3788
		// (get) Token: 0x0600357D RID: 13693 RVA: 0x00171577 File Offset: 0x0016F777
		// (set) Token: 0x0600357E RID: 13694 RVA: 0x0017157F File Offset: 0x0016F77F
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowDisguises { get; set; }

		// Token: 0x17000ECD RID: 3789
		// (get) Token: 0x0600357F RID: 13695 RVA: 0x00171588 File Offset: 0x0016F788
		// (set) Token: 0x06003580 RID: 13696 RVA: 0x00171590 File Offset: 0x0016F790
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowRewiring { get; set; }

		// Token: 0x17000ECE RID: 3790
		// (get) Token: 0x06003581 RID: 13697 RVA: 0x00171599 File Offset: 0x0016F799
		// (set) Token: 0x06003582 RID: 13698 RVA: 0x001715A1 File Offset: 0x0016F7A1
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowImmediateItemDelivery { get; set; }

		// Token: 0x17000ECF RID: 3791
		// (get) Token: 0x06003583 RID: 13699 RVA: 0x001715AA File Offset: 0x0016F7AA
		// (set) Token: 0x06003584 RID: 13700 RVA: 0x001715B2 File Offset: 0x0016F7B2
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool LockAllDefaultWires { get; set; }

		// Token: 0x17000ED0 RID: 3792
		// (get) Token: 0x06003585 RID: 13701 RVA: 0x001715BB File Offset: 0x0016F7BB
		// (set) Token: 0x06003586 RID: 13702 RVA: 0x001715C3 File Offset: 0x0016F7C3
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowLinkingWifiToChat { get; set; }

		// Token: 0x17000ED1 RID: 3793
		// (get) Token: 0x06003587 RID: 13703 RVA: 0x001715CC File Offset: 0x0016F7CC
		// (set) Token: 0x06003588 RID: 13704 RVA: 0x001715D4 File Offset: 0x0016F7D4
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowFriendlyFire { get; set; }

		// Token: 0x17000ED2 RID: 3794
		// (get) Token: 0x06003589 RID: 13705 RVA: 0x001715DD File Offset: 0x0016F7DD
		// (set) Token: 0x0600358A RID: 13706 RVA: 0x001715E5 File Offset: 0x0016F7E5
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowDragAndDropGive { get; set; }

		// Token: 0x17000ED3 RID: 3795
		// (get) Token: 0x0600358B RID: 13707 RVA: 0x001715EE File Offset: 0x0016F7EE
		// (set) Token: 0x0600358C RID: 13708 RVA: 0x001715F6 File Offset: 0x0016F7F6
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DestructibleOutposts { get; set; }

		// Token: 0x17000ED4 RID: 3796
		// (get) Token: 0x0600358D RID: 13709 RVA: 0x001715FF File Offset: 0x0016F7FF
		// (set) Token: 0x0600358E RID: 13710 RVA: 0x00171607 File Offset: 0x0016F807
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool KillableNPCs { get; set; }

		// Token: 0x17000ED5 RID: 3797
		// (get) Token: 0x0600358F RID: 13711 RVA: 0x00171610 File Offset: 0x0016F810
		// (set) Token: 0x06003590 RID: 13712 RVA: 0x00171618 File Offset: 0x0016F818
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool BanAfterWrongPassword { get; set; }

		// Token: 0x17000ED6 RID: 3798
		// (get) Token: 0x06003591 RID: 13713 RVA: 0x00171621 File Offset: 0x0016F821
		// (set) Token: 0x06003592 RID: 13714 RVA: 0x00171629 File Offset: 0x0016F829
		[Serialize(3, IsPropertySaveable.Yes, "", "", false)]
		public int MaxPasswordRetriesBeforeBan { get; private set; }

		// Token: 0x17000ED7 RID: 3799
		// (get) Token: 0x06003593 RID: 13715 RVA: 0x00171632 File Offset: 0x0016F832
		// (set) Token: 0x06003594 RID: 13716 RVA: 0x0017163A File Offset: 0x0016F83A
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool EnableDoSProtection { get; private set; }

		// Token: 0x17000ED8 RID: 3800
		// (get) Token: 0x06003595 RID: 13717 RVA: 0x00171643 File Offset: 0x0016F843
		// (set) Token: 0x06003596 RID: 13718 RVA: 0x0017164B File Offset: 0x0016F84B
		[Serialize(4000, IsPropertySaveable.Yes, "", "", false)]
		public int MaxPacketAmount { get; private set; }

		// Token: 0x17000ED9 RID: 3801
		// (get) Token: 0x06003597 RID: 13719 RVA: 0x00171654 File Offset: 0x0016F854
		// (set) Token: 0x06003598 RID: 13720 RVA: 0x0017165C File Offset: 0x0016F85C
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SelectedSubmarine { get; set; }

		// Token: 0x17000EDA RID: 3802
		// (get) Token: 0x06003599 RID: 13721 RVA: 0x00171665 File Offset: 0x0016F865
		// (set) Token: 0x0600359A RID: 13722 RVA: 0x0017166D File Offset: 0x0016F86D
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SelectedShuttle { get; set; }

		// Token: 0x17000EDB RID: 3803
		// (get) Token: 0x0600359B RID: 13723 RVA: 0x00171676 File Offset: 0x0016F876
		// (set) Token: 0x0600359C RID: 13724 RVA: 0x0017167E File Offset: 0x0016F87E
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float TraitorProbability
		{
			get
			{
				return this.traitorProbability;
			}
			set
			{
				if (MathUtils.NearlyEqual(this.traitorProbability, value, 0.0001f))
				{
					return;
				}
				this.traitorProbability = MathHelper.Clamp(value, 0f, 1f);
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EDC RID: 3804
		// (get) Token: 0x0600359D RID: 13725 RVA: 0x001716B1 File Offset: 0x0016F8B1
		// (set) Token: 0x0600359E RID: 13726 RVA: 0x001716BC File Offset: 0x0016F8BC
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		public int TraitorDangerLevel
		{
			get
			{
				return this.traitorDangerLevel;
			}
			set
			{
				int clampedValue = MathHelper.Clamp(value, 1, 3);
				if (this.traitorDangerLevel == clampedValue)
				{
					return;
				}
				this.traitorDangerLevel = clampedValue;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EDD RID: 3805
		// (get) Token: 0x0600359F RID: 13727 RVA: 0x001716EA File Offset: 0x0016F8EA
		// (set) Token: 0x060035A0 RID: 13728 RVA: 0x001716F2 File Offset: 0x0016F8F2
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		public int TraitorsMinPlayerCount
		{
			get
			{
				return this.traitorsMinPlayerCount;
			}
			set
			{
				this.traitorsMinPlayerCount = MathHelper.Clamp(value, 1, NetConfig.MaxPlayers);
			}
		}

		// Token: 0x17000EDE RID: 3806
		// (get) Token: 0x060035A1 RID: 13729 RVA: 0x00171706 File Offset: 0x0016F906
		// (set) Token: 0x060035A2 RID: 13730 RVA: 0x0017170E File Offset: 0x0016F90E
		[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
		public float MinPercentageOfPlayersForTraitorAccusation { get; set; }

		// Token: 0x17000EDF RID: 3807
		// (get) Token: 0x060035A3 RID: 13731 RVA: 0x00171717 File Offset: 0x0016F917
		// (set) Token: 0x060035A4 RID: 13732 RVA: 0x0017171F File Offset: 0x0016F91F
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public LanguageIdentifier Language { get; set; }

		// Token: 0x17000EE0 RID: 3808
		// (get) Token: 0x060035A5 RID: 13733 RVA: 0x00171728 File Offset: 0x0016F928
		// (set) Token: 0x060035A6 RID: 13734 RVA: 0x00171730 File Offset: 0x0016F930
		[Serialize(SelectionMode.Manual, IsPropertySaveable.Yes, "", "", false)]
		public SelectionMode SubSelectionMode
		{
			get
			{
				return this.subSelectionMode;
			}
			set
			{
				this.subSelectionMode = value;
				this.AllowSubVoting = (this.subSelectionMode == SelectionMode.Vote);
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EE1 RID: 3809
		// (get) Token: 0x060035A7 RID: 13735 RVA: 0x0017174F File Offset: 0x0016F94F
		// (set) Token: 0x060035A8 RID: 13736 RVA: 0x00171757 File Offset: 0x0016F957
		[Serialize(SelectionMode.Manual, IsPropertySaveable.Yes, "", "", false)]
		public SelectionMode ModeSelectionMode
		{
			get
			{
				return this.modeSelectionMode;
			}
			set
			{
				this.modeSelectionMode = value;
				this.AllowModeVoting = (this.modeSelectionMode == SelectionMode.Vote);
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17000EE2 RID: 3810
		// (get) Token: 0x060035A9 RID: 13737 RVA: 0x00171776 File Offset: 0x0016F976
		// (set) Token: 0x060035AA RID: 13738 RVA: 0x0017177E File Offset: 0x0016F97E
		public BanList BanList { get; private set; }

		// Token: 0x17000EE3 RID: 3811
		// (get) Token: 0x060035AB RID: 13739 RVA: 0x00171787 File Offset: 0x0016F987
		// (set) Token: 0x060035AC RID: 13740 RVA: 0x0017178F File Offset: 0x0016F98F
		[Serialize(0.6f, IsPropertySaveable.Yes, "", "", false)]
		public float EndVoteRequiredRatio { get; private set; }

		// Token: 0x17000EE4 RID: 3812
		// (get) Token: 0x060035AD RID: 13741 RVA: 0x00171798 File Offset: 0x0016F998
		// (set) Token: 0x060035AE RID: 13742 RVA: 0x001717A0 File Offset: 0x0016F9A0
		[Serialize(0.6f, IsPropertySaveable.Yes, "", "", false)]
		public float VoteRequiredRatio { get; private set; }

		// Token: 0x17000EE5 RID: 3813
		// (get) Token: 0x060035AF RID: 13743 RVA: 0x001717A9 File Offset: 0x0016F9A9
		// (set) Token: 0x060035B0 RID: 13744 RVA: 0x001717B1 File Offset: 0x0016F9B1
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float VoteTimeout { get; private set; }

		// Token: 0x17000EE6 RID: 3814
		// (get) Token: 0x060035B1 RID: 13745 RVA: 0x001717BA File Offset: 0x0016F9BA
		// (set) Token: 0x060035B2 RID: 13746 RVA: 0x001717C2 File Offset: 0x0016F9C2
		[Serialize(0.6f, IsPropertySaveable.Yes, "", "", false)]
		public float KickVoteRequiredRatio { get; private set; }

		// Token: 0x17000EE7 RID: 3815
		// (get) Token: 0x060035B3 RID: 13747 RVA: 0x001717CB File Offset: 0x0016F9CB
		// (set) Token: 0x060035B4 RID: 13748 RVA: 0x001717D3 File Offset: 0x0016F9D3
		[Serialize(120f, IsPropertySaveable.Yes, "", "", false)]
		public float DisallowKickVoteTime { get; private set; }

		// Token: 0x17000EE8 RID: 3816
		// (get) Token: 0x060035B5 RID: 13749 RVA: 0x001717DC File Offset: 0x0016F9DC
		// (set) Token: 0x060035B6 RID: 13750 RVA: 0x001717E4 File Offset: 0x0016F9E4
		[Serialize(300f, IsPropertySaveable.Yes, "", "", false)]
		public float KillDisconnectedTime { get; set; }

		// Token: 0x17000EE9 RID: 3817
		// (get) Token: 0x060035B7 RID: 13751 RVA: 0x001717ED File Offset: 0x0016F9ED
		// (set) Token: 0x060035B8 RID: 13752 RVA: 0x001717F5 File Offset: 0x0016F9F5
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		public float DespawnDisconnectedPermadeathTime { get; private set; }

		// Token: 0x17000EEA RID: 3818
		// (get) Token: 0x060035B9 RID: 13753 RVA: 0x001717FE File Offset: 0x0016F9FE
		// (set) Token: 0x060035BA RID: 13754 RVA: 0x00171806 File Offset: 0x0016FA06
		[Serialize(600f, IsPropertySaveable.Yes, "", "", false)]
		public float KickAFKTime { get; private set; }

		// Token: 0x17000EEB RID: 3819
		// (get) Token: 0x060035BB RID: 13755 RVA: 0x0017180F File Offset: 0x0016FA0F
		// (set) Token: 0x060035BC RID: 13756 RVA: 0x00171817 File Offset: 0x0016FA17
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float MinimumMidRoundSyncTimeout { get; private set; } = 30f;

		// Token: 0x17000EEC RID: 3820
		// (get) Token: 0x060035BD RID: 13757 RVA: 0x00171820 File Offset: 0x0016FA20
		// (set) Token: 0x060035BE RID: 13758 RVA: 0x00171828 File Offset: 0x0016FA28
		[Serialize(120f, IsPropertySaveable.Yes, "", "", false)]
		public float RoundStartSyncDuration { get; private set; } = 120f;

		// Token: 0x17000EED RID: 3821
		// (get) Token: 0x060035BF RID: 13759 RVA: 0x00171831 File Offset: 0x0016FA31
		// (set) Token: 0x060035C0 RID: 13760 RVA: 0x00171839 File Offset: 0x0016FA39
		[Serialize(15f, IsPropertySaveable.Yes, "", "", false)]
		public float EventRemovalTime { get; private set; } = 15f;

		// Token: 0x17000EEE RID: 3822
		// (get) Token: 0x060035C1 RID: 13761 RVA: 0x00171842 File Offset: 0x0016FA42
		// (set) Token: 0x060035C2 RID: 13762 RVA: 0x0017184A File Offset: 0x0016FA4A
		[Serialize(20f, IsPropertySaveable.Yes, "", "", false)]
		public float OldReceivedEventKickTime { get; private set; } = 20f;

		// Token: 0x17000EEF RID: 3823
		// (get) Token: 0x060035C3 RID: 13763 RVA: 0x00171853 File Offset: 0x0016FA53
		// (set) Token: 0x060035C4 RID: 13764 RVA: 0x0017185B File Offset: 0x0016FA5B
		[Serialize(40f, IsPropertySaveable.Yes, "", "", false)]
		public float OldEventKickTime { get; private set; } = 40f;

		// Token: 0x17000EF0 RID: 3824
		// (get) Token: 0x060035C5 RID: 13765 RVA: 0x00171864 File Offset: 0x0016FA64
		// (set) Token: 0x060035C6 RID: 13766 RVA: 0x0017186C File Offset: 0x0016FA6C
		[Serialize(60f, IsPropertySaveable.Yes, "", "", false)]
		public float TimeoutThresholdNotInGame { get; private set; } = 60f;

		// Token: 0x17000EF1 RID: 3825
		// (get) Token: 0x060035C7 RID: 13767 RVA: 0x00171875 File Offset: 0x0016FA75
		// (set) Token: 0x060035C8 RID: 13768 RVA: 0x0017187D File Offset: 0x0016FA7D
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		public float TimeoutThresholdInGame { get; private set; } = 10f;

		// Token: 0x17000EF2 RID: 3826
		// (get) Token: 0x060035C9 RID: 13769 RVA: 0x00171886 File Offset: 0x0016FA86
		// (set) Token: 0x060035CA RID: 13770 RVA: 0x0017188E File Offset: 0x0016FA8E
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool KarmaEnabled
		{
			get
			{
				return this.karmaEnabled;
			}
			set
			{
				this.karmaEnabled = value;
			}
		}

		// Token: 0x17000EF3 RID: 3827
		// (get) Token: 0x060035CB RID: 13771 RVA: 0x00171897 File Offset: 0x0016FA97
		// (set) Token: 0x060035CC RID: 13772 RVA: 0x001718A0 File Offset: 0x0016FAA0
		[Serialize("default", IsPropertySaveable.Yes, "", "", false)]
		public string KarmaPreset
		{
			get
			{
				return this.karmaPreset;
			}
			set
			{
				if (this.karmaPreset == value)
				{
					return;
				}
				if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember != null)
					{
						KarmaManager karmaManager = networkMember.KarmaManager;
						if (karmaManager != null)
						{
							karmaManager.SelectPreset(value);
						}
					}
				}
				this.karmaPreset = value;
			}
		}

		// Token: 0x17000EF4 RID: 3828
		// (get) Token: 0x060035CD RID: 13773 RVA: 0x001718F2 File Offset: 0x0016FAF2
		// (set) Token: 0x060035CE RID: 13774 RVA: 0x001718FA File Offset: 0x0016FAFA
		[Serialize("sandbox", IsPropertySaveable.Yes, "", "", false)]
		public Identifier GameModeIdentifier { get; set; }

		// Token: 0x17000EF5 RID: 3829
		// (get) Token: 0x060035CF RID: 13775 RVA: 0x00171903 File Offset: 0x0016FB03
		// (set) Token: 0x060035D0 RID: 13776 RVA: 0x0017193C File Offset: 0x0016FB3C
		[Serialize("All", IsPropertySaveable.Yes, "", "", false)]
		public string MissionTypes
		{
			get
			{
				return string.Join<Identifier>(",", from t in this.AllowedRandomMissionTypes
				select t.ToIdentifier<Identifier>());
			}
			set
			{
				this.AllowedRandomMissionTypes = (from t in value.Split(",", StringSplitOptions.None)
				select t.ToIdentifier()).Distinct<Identifier>().ToList<Identifier>();
				this.ValidateMissionTypes();
			}
		}

		// Token: 0x17000EF6 RID: 3830
		// (get) Token: 0x060035D1 RID: 13777 RVA: 0x0017198F File Offset: 0x0016FB8F
		// (set) Token: 0x060035D2 RID: 13778 RVA: 0x00171997 File Offset: 0x0016FB97
		[Serialize(8, IsPropertySaveable.Yes, "", "", false)]
		public int MaxPlayers
		{
			get
			{
				return this.maxPlayers;
			}
			set
			{
				this.maxPlayers = MathHelper.Clamp(value, 0, NetConfig.MaxPlayers);
			}
		}

		// Token: 0x17000EF7 RID: 3831
		// (get) Token: 0x060035D3 RID: 13779 RVA: 0x001719AB File Offset: 0x0016FBAB
		// (set) Token: 0x060035D4 RID: 13780 RVA: 0x001719B3 File Offset: 0x0016FBB3
		public List<Identifier> AllowedRandomMissionTypes { get; private set; }

		// Token: 0x17000EF8 RID: 3832
		// (get) Token: 0x060035D5 RID: 13781 RVA: 0x001719BC File Offset: 0x0016FBBC
		// (set) Token: 0x060035D6 RID: 13782 RVA: 0x001719C4 File Offset: 0x0016FBC4
		[Serialize(3600f, IsPropertySaveable.Yes, "", "", false)]
		public float AutoBanTime { get; private set; }

		// Token: 0x17000EF9 RID: 3833
		// (get) Token: 0x060035D7 RID: 13783 RVA: 0x001719CD File Offset: 0x0016FBCD
		// (set) Token: 0x060035D8 RID: 13784 RVA: 0x001719D5 File Offset: 0x0016FBD5
		[Serialize(86400f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxAutoBanTime { get; private set; }

		// Token: 0x17000EFA RID: 3834
		// (get) Token: 0x060035D9 RID: 13785 RVA: 0x001719DE File Offset: 0x0016FBDE
		// (set) Token: 0x060035DA RID: 13786 RVA: 0x001719E6 File Offset: 0x0016FBE6
		[Serialize(LootedMoneyDestination.Bank, IsPropertySaveable.Yes, "", "", false)]
		public LootedMoneyDestination LootedMoneyDestination { get; set; }

		// Token: 0x17000EFB RID: 3835
		// (get) Token: 0x060035DB RID: 13787 RVA: 0x001719EF File Offset: 0x0016FBEF
		// (set) Token: 0x060035DC RID: 13788 RVA: 0x001719F7 File Offset: 0x0016FBF7
		[Serialize(999999, IsPropertySaveable.Yes, "", "", false)]
		public int MaximumMoneyTransferRequest { get; set; }

		// Token: 0x17000EFC RID: 3836
		// (get) Token: 0x060035DD RID: 13789 RVA: 0x00171A00 File Offset: 0x0016FC00
		// (set) Token: 0x060035DE RID: 13790 RVA: 0x00171A08 File Offset: 0x0016FC08
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float NewCampaignDefaultSalary { get; set; }

		// Token: 0x17000EFD RID: 3837
		// (get) Token: 0x060035DF RID: 13791 RVA: 0x00171A11 File Offset: 0x0016FC11
		// (set) Token: 0x060035E0 RID: 13792 RVA: 0x00171A19 File Offset: 0x0016FC19
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool TrackOpponentInPvP { get; set; }

		// Token: 0x17000EFE RID: 3838
		// (get) Token: 0x060035E1 RID: 13793 RVA: 0x00171A22 File Offset: 0x0016FC22
		// (set) Token: 0x060035E2 RID: 13794 RVA: 0x00171A2A File Offset: 0x0016FC2A
		[Serialize(7, IsPropertySaveable.Yes, "", "", false)]
		public int DisembarkPointAllowance { get; set; }

		// Token: 0x17000EFF RID: 3839
		// (get) Token: 0x060035E3 RID: 13795 RVA: 0x00171A33 File Offset: 0x0016FC33
		// (set) Token: 0x060035E4 RID: 13796 RVA: 0x00171A3B File Offset: 0x0016FC3B
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[DoNotSyncOverNetwork]
		public Identifier[] SelectedCoalitionPerks { get; set; } = Array.Empty<Identifier>();

		// Token: 0x17000F00 RID: 3840
		// (get) Token: 0x060035E5 RID: 13797 RVA: 0x00171A44 File Offset: 0x0016FC44
		// (set) Token: 0x060035E6 RID: 13798 RVA: 0x00171A4C File Offset: 0x0016FC4C
		[Serialize(200, IsPropertySaveable.Yes, "", "", false)]
		public int WinScorePvP { get; set; }

		// Token: 0x17000F01 RID: 3841
		// (get) Token: 0x060035E7 RID: 13799 RVA: 0x00171A55 File Offset: 0x0016FC55
		// (set) Token: 0x060035E8 RID: 13800 RVA: 0x00171A5D File Offset: 0x0016FC5D
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[DoNotSyncOverNetwork]
		public Identifier[] SelectedSeparatistsPerks { get; set; } = Array.Empty<Identifier>();

		// Token: 0x17000F02 RID: 3842
		// (get) Token: 0x060035E9 RID: 13801 RVA: 0x00171A66 File Offset: 0x0016FC66
		// (set) Token: 0x060035EA RID: 13802 RVA: 0x00171A6E File Offset: 0x0016FC6E
		public CampaignSettings CampaignSettings { get; set; } = CampaignSettings.Empty;

		// Token: 0x17000F03 RID: 3843
		// (get) Token: 0x060035EB RID: 13803 RVA: 0x00171A77 File Offset: 0x0016FC77
		// (set) Token: 0x060035EC RID: 13804 RVA: 0x00171A7F File Offset: 0x0016FC7F
		public bool AllowSubVoting
		{
			get
			{
				return this.allowSubVoting;
			}
			set
			{
				if (value == this.allowSubVoting)
				{
					return;
				}
				this.allowSubVoting = value;
			}
		}

		// Token: 0x17000F04 RID: 3844
		// (get) Token: 0x060035ED RID: 13805 RVA: 0x00171A92 File Offset: 0x0016FC92
		// (set) Token: 0x060035EE RID: 13806 RVA: 0x00171A9A File Offset: 0x0016FC9A
		public bool AllowModeVoting
		{
			get
			{
				return this.allowModeVoting;
			}
			set
			{
				if (value == this.allowModeVoting)
				{
					return;
				}
				this.allowModeVoting = value;
			}
		}

		// Token: 0x060035EF RID: 13807 RVA: 0x00171AAD File Offset: 0x0016FCAD
		public void SetPassword(string password)
		{
			this.password = (string.IsNullOrEmpty(password) ? null : password);
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			server.ClearRecentlyDisconnectedClients();
		}

		// Token: 0x060035F0 RID: 13808 RVA: 0x00171AD0 File Offset: 0x0016FCD0
		public static byte[] SaltPassword(byte[] password, int salt)
		{
			byte[] saltedPw = new byte[password.Length * 2];
			for (int i = 0; i < password.Length; i++)
			{
				saltedPw[i * 2] = password[i];
				saltedPw[i * 2 + 1] = (byte)(salt >> 8 * (i % 4) & 255);
			}
			return NetUtility.ComputeSHAHash(saltedPw);
		}

		// Token: 0x060035F1 RID: 13809 RVA: 0x00171B20 File Offset: 0x0016FD20
		public bool IsPasswordCorrect(byte[] input, int salt)
		{
			if (!this.HasPassword)
			{
				return true;
			}
			byte[] saltedPw = ServerSettings.SaltPassword(Encoding.UTF8.GetBytes(this.password), salt);
			return saltedPw.SequenceEqual(input);
		}

		// Token: 0x17000F05 RID: 3845
		// (get) Token: 0x060035F2 RID: 13810 RVA: 0x00171B5F File Offset: 0x0016FD5F
		// (set) Token: 0x060035F3 RID: 13811 RVA: 0x00171B67 File Offset: 0x0016FD67
		public List<Range<int>> AllowedClientNameChars { get; private set; } = new List<Range<int>>();

		// Token: 0x060035F4 RID: 13812 RVA: 0x00171B70 File Offset: 0x0016FD70
		private void InitMonstersEnabled()
		{
			if (this.MonsterEnabled == null || this.MonsterEnabled.Count != CharacterPrefab.Prefabs.Count<CharacterPrefab>())
			{
				this.MonsterEnabled = (from p in CharacterPrefab.Prefabs
				select new ValueTuple<Identifier, bool>(p.Identifier, true)).ToDictionary<Identifier, bool>();
			}
		}

		// Token: 0x060035F5 RID: 13813 RVA: 0x00171BD0 File Offset: 0x0016FDD0
		private static IReadOnlyList<Identifier> ExtractAndSortKeys(IReadOnlyDictionary<Identifier, bool> monsterEnabled)
		{
			return (from k in monsterEnabled.Keys
			orderby CharacterPrefab.Prefabs[k].UintIdentifier
			select k).ToImmutableArray<Identifier>();
		}

		// Token: 0x060035F6 RID: 13814 RVA: 0x00171C08 File Offset: 0x0016FE08
		public bool ReadMonsterEnabled(IReadMessage inc)
		{
			bool changed = false;
			this.InitMonstersEnabled();
			IReadOnlyList<Identifier> monsterNames = ServerSettings.ExtractAndSortKeys(this.MonsterEnabled);
			uint receivedMonsterCount = inc.ReadVariableUInt32();
			if ((long)monsterNames.Count != (long)((ulong)receivedMonsterCount))
			{
				inc.BitPosition += (int)receivedMonsterCount;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Expected monster count ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(monsterNames.Count);
				defaultInterpolatedStringHandler.AppendLiteral(", got ");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(receivedMonsterCount);
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			else
			{
				foreach (Identifier s in monsterNames)
				{
					bool prevEnabled;
					this.MonsterEnabled.TryGetValue(s, out prevEnabled);
					this.MonsterEnabled[s] = inc.ReadBoolean();
					changed |= (prevEnabled != this.MonsterEnabled[s]);
				}
			}
			inc.ReadPadBits();
			return changed;
		}

		// Token: 0x060035F7 RID: 13815 RVA: 0x00171D0C File Offset: 0x0016FF0C
		public void WriteMonsterEnabled(IWriteMessage msg, Dictionary<Identifier, bool> monsterEnabled = null)
		{
			this.InitMonstersEnabled();
			if (monsterEnabled == null)
			{
				monsterEnabled = this.MonsterEnabled;
			}
			IReadOnlyList<Identifier> monsterNames = ServerSettings.ExtractAndSortKeys(monsterEnabled);
			msg.WriteVariableUInt32((uint)monsterNames.Count);
			foreach (Identifier s in monsterNames)
			{
				msg.WriteBoolean(monsterEnabled[s]);
			}
			msg.WritePadBits();
		}

		// Token: 0x060035F8 RID: 13816 RVA: 0x00171D84 File Offset: 0x0016FF84
		public bool ReadExtraCargo(IReadMessage msg)
		{
			bool changed = false;
			uint count = msg.ReadUInt32();
			if (this.ExtraCargo == null || (ulong)count != (ulong)((long)this.ExtraCargo.Count))
			{
				changed = true;
			}
			Dictionary<ItemPrefab, int> extraCargo = new Dictionary<ItemPrefab, int>();
			int i = 0;
			while ((long)i < (long)((ulong)count))
			{
				Identifier prefabIdentifier = msg.ReadIdentifier();
				byte amount = msg.ReadByte();
				ItemPrefab itemPrefab = MapEntityPrefab.Find(null, prefabIdentifier, false) as ItemPrefab;
				if (itemPrefab != null && amount > 0 && this.ExtraCargo.Keys.Count<ItemPrefab>() < 20 && (!this.ExtraCargo.ContainsKey(itemPrefab) || this.ExtraCargo[itemPrefab] < 10))
				{
					if (changed || !this.ExtraCargo.ContainsKey(itemPrefab) || this.ExtraCargo[itemPrefab] != (int)amount)
					{
						changed = true;
					}
					extraCargo.Add(itemPrefab, (int)amount);
				}
				i++;
			}
			if (changed)
			{
				this.ExtraCargo = extraCargo;
			}
			return changed;
		}

		// Token: 0x060035F9 RID: 13817 RVA: 0x00171E68 File Offset: 0x00170068
		public void WriteExtraCargo(IWriteMessage msg)
		{
			if (this.ExtraCargo == null)
			{
				msg.WriteUInt32(0U);
				return;
			}
			msg.WriteUInt32((uint)this.ExtraCargo.Count);
			foreach (KeyValuePair<ItemPrefab, int> kvp in this.ExtraCargo)
			{
				msg.WriteIdentifier(kvp.Key.Identifier);
				msg.WriteByte((byte)kvp.Value);
			}
		}

		// Token: 0x060035FA RID: 13818 RVA: 0x00171EF8 File Offset: 0x001700F8
		public void WritePerks(IWriteMessage msg)
		{
			List<DisembarkPerkPrefab> coalitionPerks = ServerSettings.<WritePerks>g__GetPerks|493_0(this.SelectedCoalitionPerks);
			msg.WriteVariableUInt32((uint)coalitionPerks.Count);
			foreach (DisembarkPerkPrefab perk in coalitionPerks)
			{
				msg.WriteUInt32(perk.UintIdentifier);
			}
			List<DisembarkPerkPrefab> separatistsPerks = ServerSettings.<WritePerks>g__GetPerks|493_0(this.SelectedSeparatistsPerks);
			msg.WriteVariableUInt32((uint)separatistsPerks.Count);
			foreach (DisembarkPerkPrefab perk2 in separatistsPerks)
			{
				msg.WriteUInt32(perk2.UintIdentifier);
			}
		}

		// Token: 0x060035FB RID: 13819 RVA: 0x00171FC0 File Offset: 0x001701C0
		public bool ReadPerks(IReadMessage msg)
		{
			uint coalitionCount = msg.ReadVariableUInt32();
			Identifier[] newCoalitionPerks = new Identifier[coalitionCount];
			int i = 0;
			while ((long)i < (long)((ulong)coalitionCount))
			{
				uint id = msg.ReadUInt32();
				DisembarkPerkPrefab prefab = DisembarkPerkPrefab.Prefabs.Find((DisembarkPerkPrefab p) => p.UintIdentifier == id);
				if (prefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Perk not found: ");
					defaultInterpolatedStringHandler.AppendFormatted<uint>(id);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					newCoalitionPerks[i] = prefab.Identifier;
				}
				i++;
			}
			uint separatistsCount = msg.ReadVariableUInt32();
			Identifier[] newSeparatistsPerks = new Identifier[separatistsCount];
			int j = 0;
			while ((long)j < (long)((ulong)separatistsCount))
			{
				uint id = msg.ReadUInt32();
				DisembarkPerkPrefab prefab2 = DisembarkPerkPrefab.Prefabs.Find((DisembarkPerkPrefab p) => p.UintIdentifier == id);
				if (prefab2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Perk not found: ");
					defaultInterpolatedStringHandler2.AppendFormatted<uint>(id);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					newSeparatistsPerks[j] = prefab2.Identifier;
				}
				j++;
			}
			bool changed = !this.SelectedCoalitionPerks.SequenceEqual(newCoalitionPerks) || !this.SelectedSeparatistsPerks.SequenceEqual(newSeparatistsPerks);
			this.SelectedCoalitionPerks = newCoalitionPerks;
			this.SelectedSeparatistsPerks = newSeparatistsPerks;
			return changed;
		}

		// Token: 0x060035FC RID: 13820 RVA: 0x00172148 File Offset: 0x00170348
		public void ReadHiddenSubs(IReadMessage msg)
		{
			IReadOnlyList<SubmarineInfo> subList = GameMain.NetLobbyScreen.GetSubList();
			this.HiddenSubs.Clear();
			uint count = msg.ReadVariableUInt32();
			int i = 0;
			while ((long)i < (long)((ulong)count))
			{
				int index = (int)msg.ReadUInt16();
				if (index < subList.Count)
				{
					string submarineName = subList[index].Name;
					this.HiddenSubs.Add(submarineName);
				}
				i++;
			}
			this.SelectNonHiddenSubmarine(null);
		}

		// Token: 0x060035FD RID: 13821 RVA: 0x001721B4 File Offset: 0x001703B4
		public void WriteHiddenSubs(IWriteMessage msg)
		{
			IReadOnlyList<SubmarineInfo> subList = GameMain.NetLobbyScreen.GetSubList();
			msg.WriteVariableUInt32((uint)this.HiddenSubs.Count);
			using (HashSet<string>.Enumerator enumerator = this.HiddenSubs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string submarineName = enumerator.Current;
					msg.WriteUInt16((ushort)subList.FindIndex((SubmarineInfo s) => s.Name.Equals(submarineName, StringComparison.OrdinalIgnoreCase)));
				}
			}
		}

		// Token: 0x060035FE RID: 13822 RVA: 0x00172240 File Offset: 0x00170440
		public void UpdateServerListInfo(Action<Identifier, object> setter)
		{
			ServerSettings.<>c__DisplayClass497_0 CS$<>8__locals1;
			CS$<>8__locals1.setter = setter;
			ServerSettings.<UpdateServerListInfo>g__set|497_0("ServerName", this.ServerName, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("MaxPlayers", this.MaxPlayers, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("HasPassword", this.HasPassword, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("message", this.ServerMessageText, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("version", GameMain.Version, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("playercount", GameMain.NetworkMember.ConnectedClients.Count, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("contentpackages", from p in ContentPackageManager.EnabledPackages.All
			where p.HasMultiplayerSyncedContent
			select p, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("modeselectionmode", this.ModeSelectionMode, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("subselectionmode", this.SubSelectionMode, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("voicechatenabled", this.VoiceChatEnabled, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("allowspectating", this.AllowSpectating, ref CS$<>8__locals1);
			RespawnMode respawnMode = this.RespawnMode;
			bool flag = respawnMode - RespawnMode.MidRound <= 1;
			ServerSettings.<UpdateServerListInfo>g__set|497_0("allowrespawn", flag, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("traitors", this.TraitorProbability.ToString(CultureInfo.InvariantCulture), ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("friendlyfireenabled", this.AllowFriendlyFire, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("karmaenabled", this.KarmaEnabled, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("gamestarted", GameMain.NetworkMember.GameStarted, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("gamemode", this.GameModeIdentifier, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("playstyle", this.PlayStyle, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("language", this.Language.ToString(), ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|497_0("eoscrossplay", EosInterface.Core.IsInitialized, ref CS$<>8__locals1);
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			if (((netLobbyScreen != null) ? netLobbyScreen.SelectedSub : null) != null)
			{
				ServerSettings.<UpdateServerListInfo>g__set|497_0("submarine", GameMain.NetLobbyScreen.SelectedSub.Name, ref CS$<>8__locals1);
			}
			if (SteamClient.IsLoggedOn)
			{
				NetPingLocation? netPingLocation;
				string pingLocation = (SteamNetworkingUtils.LocalPingLocation != null) ? netPingLocation.GetValueOrDefault().ToString() : null;
				if (!pingLocation.IsNullOrEmpty())
				{
					ServerSettings.<UpdateServerListInfo>g__set|497_0("steampinglocation", pingLocation, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x060035FF RID: 13823 RVA: 0x001724D2 File Offset: 0x001706D2
		private void ValidateMissionTypes()
		{
			this.ValidateMissionTypes(MissionPrefab.CoOpMissionClasses.Values);
			this.ValidateMissionTypes(MissionPrefab.PvPMissionClasses.Values);
		}

		// Token: 0x06003600 RID: 13824 RVA: 0x001724F4 File Offset: 0x001706F4
		private void ValidateMissionTypes(IEnumerable<Type> availableMissionClasses)
		{
			if (this.AllowedRandomMissionTypes.Contains(Tags.MissionTypeAll))
			{
				return;
			}
			if (MissionPrefab.GetAllMultiplayerSelectableMissionTypes().None((Identifier missionType) => MissionPrefab.Prefabs.Any(delegate(MissionPrefab p)
			{
				Identifier type = p.Type;
				return type == missionType && this.AllowedRandomMissionTypes.Contains(p.Type) && availableMissionClasses.Contains(p.MissionClass);
			})))
			{
				MissionPrefab matchingMission = MissionPrefab.Prefabs.First((MissionPrefab p) => availableMissionClasses.Contains(p.MissionClass));
				if (matchingMission == null)
				{
					DebugConsole.ThrowError("No missions found for any of the available mission classes (" + string.Join<Type>(",", availableMissionClasses) + ")", null, null, false, false);
					return;
				}
				this.AllowedRandomMissionTypes.Add(matchingMission.Type);
			}
		}

		// Token: 0x06003601 RID: 13825 RVA: 0x00172594 File Offset: 0x00170794
		// Note: this type is marked as 'beforefieldinit'.
		static ServerSettings()
		{
			ReadOnlySpan<char> str = "Data";
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			ServerSettings.ClientPermissionsFile = str + new ReadOnlySpan<char>(ref directorySeparatorChar) + "clientpermissions.xml";
			ServerSettings.SubmarineSeparatorChar = '|';
			ReadOnlySpan<char> str2 = "Data";
			char directorySeparatorChar2 = Path.DirectorySeparatorChar;
			ServerSettings.PermissionPresetFile = str2 + new ReadOnlySpan<char>(ref directorySeparatorChar2) + "permissionpresets.xml";
			ReadOnlySpan<char> str3 = "Data";
			char directorySeparatorChar3 = Path.DirectorySeparatorChar;
			ServerSettings.PermissionPresetFileCustom = str3 + new ReadOnlySpan<char>(ref directorySeparatorChar3) + "permissionpresets_player.xml";
		}

		// Token: 0x06003603 RID: 13827 RVA: 0x00172648 File Offset: 0x00170848
		[CompilerGenerated]
		internal static bool <ReadPerks>g__HasPermissionToChangePerks|12_0(Client client)
		{
			if (client.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings))
			{
				return true;
			}
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			bool isPvP = ((netLobbyScreen != null) ? netLobbyScreen.SelectedMode : null) == GameModePreset.PvP;
			CharacterTeamType preferredTeam = client.PreferredTeam;
			bool flag = preferredTeam - CharacterTeamType.Team1 <= 1;
			bool hasSelectedTeam = flag;
			NetworkMember networkMember = GameMain.NetworkMember;
			ImmutableArray<Client>? immutableArray;
			if (networkMember == null)
			{
				immutableArray = null;
			}
			else
			{
				IReadOnlyList<Client> connectedClients = networkMember.ConnectedClients;
				immutableArray = ((connectedClients != null) ? new ImmutableArray<Client>?((from c in connectedClients
				where c != client
				select c).ToImmutableArray<Client>()) : null);
			}
			ImmutableArray<Client> otherClients = immutableArray ?? ImmutableArray<Client>.Empty;
			if (!isPvP)
			{
				return !otherClients.Any((Client c) => c.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings));
			}
			if (!hasSelectedTeam)
			{
				return false;
			}
			return !(from c in otherClients
			where c.PreferredTeam == client.PreferredTeam
			select c).Any((Client c) => c.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings));
		}

		// Token: 0x06003605 RID: 13829 RVA: 0x00172790 File Offset: 0x00170990
		[CompilerGenerated]
		internal static List<DisembarkPerkPrefab> <WritePerks>g__GetPerks|493_0(Identifier[] perkIdentifiers)
		{
			List<DisembarkPerkPrefab> perks = new List<DisembarkPerkPrefab>();
			foreach (Identifier perk in perkIdentifiers)
			{
				DisembarkPerkPrefab prefab;
				if (DisembarkPerkPrefab.Prefabs.TryGet(perk, out prefab))
				{
					perks.Add(prefab);
				}
			}
			return perks;
		}

		// Token: 0x06003606 RID: 13830 RVA: 0x001727D3 File Offset: 0x001709D3
		[CompilerGenerated]
		internal static void <UpdateServerListInfo>g__set|497_0(string key, object obj, ref ServerSettings.<>c__DisplayClass497_0 A_2)
		{
			A_2.setter(key.ToIdentifier(), obj);
		}

		// Token: 0x04001A44 RID: 6724
		public static readonly string ClientPermissionsFile;

		// Token: 0x04001A45 RID: 6725
		public static readonly char SubmarineSeparatorChar;

		// Token: 0x04001A46 RID: 6726
		public readonly Dictionary<ServerSettings.NetFlags, ushort> LastUpdateIdForFlag = (from f in (ServerSettings.NetFlags[])Enum.GetValues(typeof(ServerSettings.NetFlags))
		select new ValueTuple<ServerSettings.NetFlags, ushort>(f, 1)).ToDictionary<ServerSettings.NetFlags, ushort>();

		// Token: 0x04001A47 RID: 6727
		public const int PacketLimitMin = 1200;

		// Token: 0x04001A48 RID: 6728
		public const int PacketLimitWarning = 3500;

		// Token: 0x04001A49 RID: 6729
		public const int PacketLimitDefault = 4000;

		// Token: 0x04001A4A RID: 6730
		public const int PacketLimitMax = 10000;

		// Token: 0x04001A4B RID: 6731
		public const string SettingsFile = "serversettings.xml";

		// Token: 0x04001A4C RID: 6732
		public static readonly string PermissionPresetFile;

		// Token: 0x04001A4D RID: 6733
		public static readonly string PermissionPresetFileCustom;

		// Token: 0x04001A4E RID: 6734
		public bool ServerDetailsChanged;

		// Token: 0x04001A50 RID: 6736
		private readonly Dictionary<uint, ServerSettings.NetPropertyData> netProperties;

		// Token: 0x04001A51 RID: 6737
		private string serverName = string.Empty;

		// Token: 0x04001A52 RID: 6738
		private string serverMessageText;

		// Token: 0x04001A53 RID: 6739
		public int Port;

		// Token: 0x04001A54 RID: 6740
		public int QueryPort;

		// Token: 0x04001A55 RID: 6741
		public IPAddress ListenIPAddress;

		// Token: 0x04001A56 RID: 6742
		public bool EnableUPnP;

		// Token: 0x04001A57 RID: 6743
		public ServerLog ServerLog;

		// Token: 0x04001A59 RID: 6745
		public const int MaxExtraCargoItemsOfType = 10;

		// Token: 0x04001A5A RID: 6746
		public const int MaxExtraCargoItemTypes = 20;

		// Token: 0x04001A5D RID: 6749
		private float selectedLevelDifficulty;

		// Token: 0x04001A5E RID: 6750
		private string password;

		// Token: 0x04001A5F RID: 6751
		public float AutoRestartTimer;

		// Token: 0x04001A60 RID: 6752
		private bool autoRestart;

		// Token: 0x04001A61 RID: 6753
		private int maxPlayers;

		// Token: 0x04001A64 RID: 6756
		public const int DefaultTickRate = 20;

		// Token: 0x04001A65 RID: 6757
		private int tickRate = 20;

		// Token: 0x04001A66 RID: 6758
		private int maxLagCompensation = 150;

		// Token: 0x04001A7C RID: 6780
		private bool allowSpectating;

		// Token: 0x04001A7D RID: 6781
		private bool allowAFK;

		// Token: 0x04001A82 RID: 6786
		private bool voiceChatEnabled;

		// Token: 0x04001A83 RID: 6787
		private PlayStyle playstyleSelection;

		// Token: 0x04001A88 RID: 6792
		private RespawnMode respawnMode;

		// Token: 0x04001A9C RID: 6812
		private float traitorProbability;

		// Token: 0x04001A9D RID: 6813
		private int traitorDangerLevel;

		// Token: 0x04001A9E RID: 6814
		private int traitorsMinPlayerCount;

		// Token: 0x04001AA1 RID: 6817
		private SelectionMode subSelectionMode;

		// Token: 0x04001AA2 RID: 6818
		private SelectionMode modeSelectionMode;

		// Token: 0x04001AB3 RID: 6835
		private bool karmaEnabled;

		// Token: 0x04001AB4 RID: 6836
		private string karmaPreset = "default";

		// Token: 0x04001AC2 RID: 6850
		private bool allowSubVoting;

		// Token: 0x04001AC3 RID: 6851
		private bool allowModeVoting;

		// Token: 0x02000C29 RID: 3113
		private class NetPropertyData
		{
			// Token: 0x17001622 RID: 5666
			// (get) Token: 0x06006345 RID: 25413 RVA: 0x00211757 File Offset: 0x0020F957
			// (set) Token: 0x06006346 RID: 25414 RVA: 0x0021175F File Offset: 0x0020F95F
			public ushort LastUpdateID { get; private set; }

			// Token: 0x06006347 RID: 25415 RVA: 0x00211768 File Offset: 0x0020F968
			public void SyncValue()
			{
				if (!this.PropEquals(this.lastSyncedValue, this.Value))
				{
					this.LastUpdateID = GameMain.NetLobbyScreen.LastUpdateID;
					this.lastSyncedValue = this.Value;
				}
			}

			// Token: 0x06006348 RID: 25416 RVA: 0x0021179C File Offset: 0x0020F99C
			public void ForceUpdate()
			{
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				ushort lastUpdateID = netLobbyScreen.LastUpdateID;
				netLobbyScreen.LastUpdateID = lastUpdateID + 1;
				this.LastUpdateID = lastUpdateID;
			}

			// Token: 0x17001623 RID: 5667
			// (get) Token: 0x06006349 RID: 25417 RVA: 0x002117C5 File Offset: 0x0020F9C5
			public Identifier Name
			{
				get
				{
					return this.property.Name.ToIdentifier();
				}
			}

			// Token: 0x17001624 RID: 5668
			// (get) Token: 0x0600634A RID: 25418 RVA: 0x002117D7 File Offset: 0x0020F9D7
			// (set) Token: 0x0600634B RID: 25419 RVA: 0x002117EA File Offset: 0x0020F9EA
			public object Value
			{
				get
				{
					return this.property.GetValue(this.parentObject);
				}
				set
				{
					this.property.SetValue(this.parentObject, value);
				}
			}

			// Token: 0x0600634C RID: 25420 RVA: 0x002117FE File Offset: 0x0020F9FE
			public NetPropertyData(object parentObject, SerializableProperty property, string typeString)
			{
				this.property = property;
				this.typeString = typeString;
				this.parentObject = parentObject;
			}

			// Token: 0x0600634D RID: 25421 RVA: 0x0021181C File Offset: 0x0020FA1C
			public bool PropEquals(object a, object b)
			{
				string a2 = this.typeString;
				if (!(a2 == "float"))
				{
					if (!(a2 == "int"))
					{
						if (!(a2 == "bool"))
						{
							if (!(a2 == "Enum"))
							{
								return a == b || string.Equals((a != null) ? a.ToString() : null, (b != null) ? b.ToString() : null, StringComparison.OrdinalIgnoreCase);
							}
							Enum ea = a as Enum;
							if (ea == null)
							{
								return false;
							}
							Enum eb = b as Enum;
							return eb != null && ea.Equals(eb);
						}
						else
						{
							if (!(a is bool))
							{
								return false;
							}
							bool ba = (bool)a;
							if (b is bool)
							{
								bool bb = (bool)b;
								return ba == bb;
							}
							return false;
						}
					}
					else
					{
						if (!(a is int))
						{
							return false;
						}
						int ia = (int)a;
						if (b is int)
						{
							int ib = (int)b;
							return ia == ib;
						}
						return false;
					}
				}
				else
				{
					if (!(a is float))
					{
						return false;
					}
					float fa = (float)a;
					if (b is float)
					{
						float fb = (float)b;
						return MathUtils.NearlyEqual(fa, fb, 0.0001f);
					}
					return false;
				}
			}

			// Token: 0x0600634E RID: 25422 RVA: 0x00211948 File Offset: 0x0020FB48
			public void Read(IReadMessage msg)
			{
				int oldPos = msg.BitPosition;
				uint size = msg.ReadVariableUInt32();
				string text = this.typeString;
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
						if (!(text == "int"))
						{
							goto IL_2BF;
						}
						if (size == 4U)
						{
							this.property.SetValue(this.parentObject, msg.ReadInt32());
							return;
						}
						break;
					case 4:
					case 6:
					case 8:
						goto IL_2BF;
					case 5:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c != 'f')
							{
								goto IL_2BF;
							}
							if (!(text == "float"))
							{
								goto IL_2BF;
							}
							if (size == 4U)
							{
								this.property.SetValue(this.parentObject, msg.ReadSingle());
								return;
							}
						}
						else
						{
							if (!(text == "color"))
							{
								goto IL_2BF;
							}
							if (size == 4U)
							{
								byte r = msg.ReadByte();
								byte g = msg.ReadByte();
								byte b = msg.ReadByte();
								byte a = msg.ReadByte();
								this.property.SetValue(this.parentObject, new Microsoft.Xna.Framework.Color(r, g, b, a));
								return;
							}
						}
						break;
					}
					case 7:
						switch (text[6])
						{
						case '2':
							if (!(text == "vector2"))
							{
								goto IL_2BF;
							}
							if (size == 8U)
							{
								float x = msg.ReadSingle();
								float y = msg.ReadSingle();
								this.property.SetValue(this.parentObject, new Vector2(x, y));
								return;
							}
							break;
						case '3':
							if (!(text == "vector3"))
							{
								goto IL_2BF;
							}
							if (size == 12U)
							{
								float x = msg.ReadSingle();
								float y = msg.ReadSingle();
								float z = msg.ReadSingle();
								this.property.SetValue(this.parentObject, new Vector3(x, y, z));
								return;
							}
							break;
						case '4':
							if (!(text == "vector4"))
							{
								goto IL_2BF;
							}
							if (size == 16U)
							{
								float x = msg.ReadSingle();
								float y = msg.ReadSingle();
								float z = msg.ReadSingle();
								float w = msg.ReadSingle();
								this.property.SetValue(this.parentObject, new Vector4(x, y, z, w));
								return;
							}
							break;
						default:
							goto IL_2BF;
						}
						break;
					case 9:
						if (!(text == "rectangle"))
						{
							goto IL_2BF;
						}
						if (size == 16U)
						{
							int ix = msg.ReadInt32();
							int iy = msg.ReadInt32();
							int width = msg.ReadInt32();
							int height = msg.ReadInt32();
							this.property.SetValue(this.parentObject, new Rectangle(ix, iy, width, height));
							return;
						}
						break;
					default:
						goto IL_2BF;
					}
					msg.BitPosition += (int)(8U * size);
					return;
				}
				IL_2BF:
				msg.BitPosition = oldPos;
				string incVal = msg.ReadString();
				this.property.TrySetValue(this.parentObject, incVal);
			}

			// Token: 0x0600634F RID: 25423 RVA: 0x00211C48 File Offset: 0x0020FE48
			public void Write(IWriteMessage msg, object overrideValue = null)
			{
				if (overrideValue == null)
				{
					overrideValue = this.Value;
				}
				string text = this.typeString;
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
						if (text == "int")
						{
							msg.WriteVariableUInt32(4U);
							msg.WriteInt32((int)overrideValue);
							return;
						}
						break;
					case 5:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c == 'f')
							{
								if (text == "float")
								{
									msg.WriteVariableUInt32(4U);
									msg.WriteSingle((float)overrideValue);
									return;
								}
							}
						}
						else if (text == "color")
						{
							msg.WriteVariableUInt32(4U);
							msg.WriteByte(((Microsoft.Xna.Framework.Color)overrideValue).R);
							msg.WriteByte(((Microsoft.Xna.Framework.Color)overrideValue).G);
							msg.WriteByte(((Microsoft.Xna.Framework.Color)overrideValue).B);
							msg.WriteByte(((Microsoft.Xna.Framework.Color)overrideValue).A);
							return;
						}
						break;
					}
					case 7:
						switch (text[6])
						{
						case '2':
							if (text == "vector2")
							{
								msg.WriteVariableUInt32(8U);
								msg.WriteSingle(((Vector2)overrideValue).X);
								msg.WriteSingle(((Vector2)overrideValue).Y);
								return;
							}
							break;
						case '3':
							if (text == "vector3")
							{
								msg.WriteVariableUInt32(12U);
								msg.WriteSingle(((Vector3)overrideValue).X);
								msg.WriteSingle(((Vector3)overrideValue).Y);
								msg.WriteSingle(((Vector3)overrideValue).Z);
								return;
							}
							break;
						case '4':
							if (text == "vector4")
							{
								msg.WriteVariableUInt32(16U);
								msg.WriteSingle(((Vector4)overrideValue).X);
								msg.WriteSingle(((Vector4)overrideValue).Y);
								msg.WriteSingle(((Vector4)overrideValue).Z);
								msg.WriteSingle(((Vector4)overrideValue).W);
								return;
							}
							break;
						}
						break;
					case 9:
						if (text == "rectangle")
						{
							msg.WriteVariableUInt32(16U);
							msg.WriteInt32(((Rectangle)overrideValue).X);
							msg.WriteInt32(((Rectangle)overrideValue).Y);
							msg.WriteInt32(((Rectangle)overrideValue).Width);
							msg.WriteInt32(((Rectangle)overrideValue).Height);
							return;
						}
						break;
					}
				}
				string strVal = overrideValue.ToString();
				msg.WriteString(strVal);
			}

			// Token: 0x04003BA4 RID: 15268
			private object lastSyncedValue;

			// Token: 0x04003BA6 RID: 15270
			private readonly SerializableProperty property;

			// Token: 0x04003BA7 RID: 15271
			private readonly string typeString;

			// Token: 0x04003BA8 RID: 15272
			private readonly object parentObject;
		}

		// Token: 0x02000C2A RID: 3114
		[Flags]
		public enum NetFlags : byte
		{
			// Token: 0x04003BAA RID: 15274
			None = 0,
			// Token: 0x04003BAB RID: 15275
			Properties = 4,
			// Token: 0x04003BAC RID: 15276
			Misc = 8,
			// Token: 0x04003BAD RID: 15277
			LevelSeed = 16,
			// Token: 0x04003BAE RID: 15278
			HiddenSubs = 32
		}

		// Token: 0x02000C2B RID: 3115
		public class SavedClientPermission
		{
			// Token: 0x06006350 RID: 25424 RVA: 0x00211EF2 File Offset: 0x002100F2
			public SavedClientPermission(string name, Either<Address, AccountId> addressOrAccountId, ClientPermissions permissions, IEnumerable<DebugConsole.Command> permittedCommands)
			{
				this.Name = name;
				this.AddressOrAccountId = addressOrAccountId;
				this.Permissions = permissions;
				this.PermittedCommands = permittedCommands.ToImmutableHashSet<DebugConsole.Command>();
			}

			// Token: 0x04003BAF RID: 15279
			public readonly Either<Address, AccountId> AddressOrAccountId;

			// Token: 0x04003BB0 RID: 15280
			public readonly string Name;

			// Token: 0x04003BB1 RID: 15281
			public readonly ImmutableHashSet<DebugConsole.Command> PermittedCommands;

			// Token: 0x04003BB2 RID: 15282
			public readonly ClientPermissions Permissions;
		}
	}
}
