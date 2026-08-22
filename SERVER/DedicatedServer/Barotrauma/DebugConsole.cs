using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000017 RID: 23
	internal static class DebugConsole
	{
		// Token: 0x0600032D RID: 813 RVA: 0x00018924 File Offset: 0x00016B24
		public static void Update()
		{
			List<string> queuedCommands = DebugConsole.QueuedCommands;
			lock (queuedCommands)
			{
				while (DebugConsole.QueuedCommands.Count > 0)
				{
					DebugConsole.ExecuteCommand(DebugConsole.QueuedCommands[0]);
					DebugConsole.QueuedCommands.RemoveAt(0);
				}
			}
		}

		// Token: 0x0600032E RID: 814 RVA: 0x00018988 File Offset: 0x00016B88
		public static void UpdateCommandLine(int maxTime)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			int consoleWidth = 0;
			if (!Console.IsOutputRedirected)
			{
				consoleWidth = Console.WindowWidth;
				if (consoleWidth < 5)
				{
					consoleWidth = 5;
				}
				int consoleHeight = Console.WindowHeight;
				if (consoleHeight < 5)
				{
				}
			}
			if (DebugConsole.queuedMessages.Count > 0)
			{
				if (!Console.IsOutputRedirected)
				{
					Console.CursorLeft = 0;
				}
				ColoredText msg;
				while (DebugConsole.queuedMessages.TryDequeue(out msg))
				{
					DebugConsole.Messages.Add(msg);
					if (GameSettings.CurrentConfig.SaveDebugConsoleLogs || GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.unsavedMessages.Add(msg);
						if (DebugConsole.unsavedMessages.Count >= DebugConsole.messagesPerFile)
						{
							DebugConsole.SaveLogs();
							DebugConsole.unsavedMessages.Clear();
						}
					}
					string msgTxt = msg.Text;
					if (msg.IsCommand)
					{
						DebugConsole.commandMemory.Add(msgTxt);
					}
					if (!Console.IsOutputRedirected)
					{
						int paddingLen = consoleWidth - msg.Text.Length % consoleWidth - 1;
						msgTxt += new string(' ', (paddingLen > 0) ? paddingLen : 0);
						Console.ForegroundColor = XnaToConsoleColor.Convert(msg.Color);
					}
					Console.WriteLine(msgTxt);
					if (sw.ElapsedMilliseconds >= (long)maxTime)
					{
						break;
					}
				}
				if (!Console.IsOutputRedirected)
				{
					DebugConsole.RewriteInputToCommandLine(DebugConsole.input);
				}
			}
			if (DebugConsole.Messages.Count > 300)
			{
				DebugConsole.Messages.RemoveRange(0, DebugConsole.Messages.Count - 300);
			}
			if (!Console.IsOutputRedirected && !Console.IsInputRedirected)
			{
				bool rewriteInput = false;
				while (Console.KeyAvailable)
				{
					if (sw.ElapsedMilliseconds >= (long)maxTime)
					{
						rewriteInput = false;
						break;
					}
					rewriteInput = true;
					ConsoleKeyInfo key = Console.ReadKey(true);
					ConsoleKey key2 = key.Key;
					if (key2 <= ConsoleKey.Tab)
					{
						if (key2 == ConsoleKey.Backspace)
						{
							if (DebugConsole.input.Length > 0)
							{
								DebugConsole.input = DebugConsole.input.Substring(0, DebugConsole.input.Length - 1);
							}
							DebugConsole.ResetAutoComplete();
							DebugConsole.memoryIndex = -1;
							continue;
						}
						if (key2 == ConsoleKey.Tab)
						{
							if (DebugConsole.input.Length > 0)
							{
								DebugConsole.input = DebugConsole.AutoComplete(DebugConsole.input, 0);
								DebugConsole.memoryIndex = -1;
								continue;
							}
							continue;
						}
					}
					else
					{
						if (key2 == ConsoleKey.Enter)
						{
							List<string> queuedCommands = DebugConsole.QueuedCommands;
							lock (queuedCommands)
							{
								DebugConsole.QueuedCommands.Add(DebugConsole.input);
							}
							DebugConsole.input = "";
							DebugConsole.memoryIndex = -1;
							continue;
						}
						switch (key2)
						{
						case ConsoleKey.LeftArrow:
							DebugConsole.input = DebugConsole.AutoComplete(DebugConsole.input, -1);
							continue;
						case ConsoleKey.UpArrow:
							DebugConsole.memoryIndex--;
							if (DebugConsole.memoryIndex < 0)
							{
								DebugConsole.memoryIndex = DebugConsole.commandMemory.Count - 1;
							}
							if (DebugConsole.memoryIndex >= DebugConsole.commandMemory.Count)
							{
								DebugConsole.memoryIndex = DebugConsole.commandMemory.Count - 1;
							}
							if (DebugConsole.memoryIndex >= 0)
							{
								DebugConsole.input = DebugConsole.commandMemory[DebugConsole.memoryIndex];
								continue;
							}
							continue;
						case ConsoleKey.RightArrow:
							DebugConsole.input = DebugConsole.AutoComplete(DebugConsole.input, 1);
							continue;
						case ConsoleKey.DownArrow:
							DebugConsole.memoryIndex++;
							if (DebugConsole.memoryIndex < 0)
							{
								DebugConsole.memoryIndex = 0;
							}
							if (DebugConsole.memoryIndex >= DebugConsole.commandMemory.Count)
							{
								DebugConsole.memoryIndex = 0;
							}
							if (DebugConsole.commandMemory.Count > 0)
							{
								DebugConsole.input = DebugConsole.commandMemory[DebugConsole.memoryIndex];
								continue;
							}
							continue;
						}
					}
					if (key.KeyChar != '\0')
					{
						ReadOnlySpan<char> str = DebugConsole.input;
						char keyChar = key.KeyChar;
						DebugConsole.input = str + new ReadOnlySpan<char>(ref keyChar);
						DebugConsole.memoryIndex = -1;
					}
					DebugConsole.ResetAutoComplete();
				}
				if (rewriteInput)
				{
					DebugConsole.RewriteInputToCommandLine(DebugConsole.input);
				}
			}
			sw.Stop();
		}

		// Token: 0x0600032F RID: 815 RVA: 0x00018D64 File Offset: 0x00016F64
		private static void WriteAndResetLine(string txt)
		{
			int consoleWidth = Console.BufferWidth;
			int linesWritten = 0;
			while (txt.Length > consoleWidth)
			{
				linesWritten++;
				Console.Write(txt.Substring(0, consoleWidth));
				txt = txt.Substring(consoleWidth);
			}
			Console.Write(txt);
			if (txt.Length == consoleWidth)
			{
				Console.Write(' ');
				Console.CursorLeft--;
				linesWritten++;
			}
			Console.CursorTop -= linesWritten;
		}

		// Token: 0x06000330 RID: 816 RVA: 0x00018DD0 File Offset: 0x00016FD0
		private static void RewriteInputToCommandLine(string input)
		{
			if (Console.WindowWidth == 0 || Console.WindowHeight == 0)
			{
				return;
			}
			int consoleWidth = Math.Max(Console.BufferWidth, 5);
			try
			{
				string tmpInput = input;
				while (tmpInput.Length >= consoleWidth)
				{
					tmpInput = tmpInput.Substring(consoleWidth);
				}
				string ln = (input.Length > 0) ? DebugConsole.AutoComplete(input, 0) : "";
				while (ln.Length >= consoleWidth)
				{
					ln = ln.Substring(consoleWidth);
				}
				ln += new string(' ', consoleWidth - ln.Length % consoleWidth);
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.CursorLeft = 0;
				DebugConsole.WriteAndResetLine(ln);
				Console.ForegroundColor = ConsoleColor.White;
				Console.CursorLeft = 0;
				DebugConsole.WriteAndResetLine(tmpInput);
				Console.CursorLeft = input.Length % consoleWidth;
			}
			catch (Exception e)
			{
				string errorMsg = string.Concat(new string[]
				{
					"Failed to write input to command line (window width: ",
					Console.WindowWidth.ToString(),
					", window height: ",
					Console.WindowHeight.ToString(),
					")\n",
					e.Message,
					"\n",
					e.StackTrace.CleanupStackTrace()
				});
				GameAnalyticsManager.AddErrorEventOnce("DebugConsole.RewriteInputToCommandLine", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			}
		}

		// Token: 0x06000331 RID: 817 RVA: 0x00018F0C File Offset: 0x0001710C
		public static void Clear()
		{
			ColoredText msg;
			while (DebugConsole.queuedMessages.TryDequeue(out msg))
			{
				DebugConsole.Messages.Add(msg);
				if (GameSettings.CurrentConfig.SaveDebugConsoleLogs || GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.unsavedMessages.Add(msg);
					if (DebugConsole.unsavedMessages.Count >= DebugConsole.messagesPerFile)
					{
						DebugConsole.SaveLogs();
						DebugConsole.unsavedMessages.Clear();
					}
				}
			}
			if (DebugConsole.Messages.Count > 300)
			{
				DebugConsole.Messages.RemoveRange(0, DebugConsole.Messages.Count - 300);
			}
		}

		// Token: 0x06000332 RID: 818 RVA: 0x00018FA4 File Offset: 0x000171A4
		private static Client FindClient(string arg)
		{
			Client client = GameMain.Server.ConnectedClients.Find((Client c) => Homoglyphs.Compare(c.Name, arg));
			int id;
			if (int.TryParse(arg, out id) && client == null)
			{
				client = GameMain.Server.ConnectedClients.Find((Client c) => (int)c.SessionId == id);
			}
			Address address;
			if (Address.Parse(arg).TryUnwrap(out address) && client == null)
			{
				client = GameMain.Server.ConnectedClients.Find((Client c) => c.AddressMatches(address));
			}
			AccountId argAccountId;
			if (AccountId.Parse(arg).TryUnwrap(out argAccountId) && client == null)
			{
				client = GameMain.Server.ConnectedClients.Find((Client c) => c.AccountId.ValueEquals(argAccountId));
			}
			return client;
		}

		// Token: 0x06000333 RID: 819 RVA: 0x00019084 File Offset: 0x00017284
		public static void AssignOnClientRequestExecute(string names, Action<Client, Vector2, string[]> onClientRequestExecute)
		{
			DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Intersect(names.Split('|', StringSplitOptions.None).ToIdentifiers()).Any<Identifier>());
			if (matchingCommand == null)
			{
				throw new Exception("AssignOnClientRequestExecute failed. Command matching the name(s) \"" + names + "\" not found.");
			}
			matchingCommand.OnClientRequestExecute = onClientRequestExecute;
		}

		// Token: 0x06000334 RID: 820 RVA: 0x000190DC File Offset: 0x000172DC
		private static void InitProjectSpecific()
		{
			DebugConsole.AssignOnExecute("botcount", delegate(string[] args)
			{
				if (args.Length < 1 || GameMain.Server == null)
				{
					return;
				}
				int botCount = GameMain.Server.ServerSettings.BotCount;
				int.TryParse(args[0], out botCount);
				GameMain.NetLobbyScreen.SetBotCount(botCount);
				DebugConsole.NewMessage("Set the number of bots to " + botCount.ToString(), new Color?(Color.White), false);
			});
			DebugConsole.AssignOnClientRequestExecute("botcount", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				if (args.Length < 1 || GameMain.Server == null)
				{
					return;
				}
				int botCount = GameMain.Server.ServerSettings.BotCount;
				int.TryParse(args[0], out botCount);
				GameMain.NetLobbyScreen.SetBotCount(botCount);
				DebugConsole.NewMessage(client.Name + " set the number of bots to " + botCount.ToString(), new Color?(Color.White), false);
				GameMain.Server.SendConsoleMessage("Set the number of bots to " + botCount.ToString(), client, null);
			});
			DebugConsole.AssignOnExecute("botspawnmode", delegate(string[] args)
			{
				if (args.Length < 1 || GameMain.Server == null)
				{
					return;
				}
				BotSpawnMode spawnMode;
				if (Enum.TryParse<BotSpawnMode>(args[0], true, out spawnMode))
				{
					GameMain.NetLobbyScreen.SetBotSpawnMode(spawnMode);
					DebugConsole.NewMessage("Set bot spawn mode to " + spawnMode.ToString(), new Color?(Color.White), false);
					return;
				}
				DebugConsole.NewMessage("\"" + args[0] + "\" is not a valid bot spawn mode. (Valid modes are Fill and Normal)", new Color?(Color.White), false);
			});
			DebugConsole.AssignOnClientRequestExecute("botspawnmode", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				if (args.Length < 1 || GameMain.Server == null)
				{
					return;
				}
				BotSpawnMode spawnMode;
				if (Enum.TryParse<BotSpawnMode>(args[0], true, out spawnMode))
				{
					GameMain.NetLobbyScreen.SetBotSpawnMode(spawnMode);
					DebugConsole.NewMessage(client.Name + " set bot spawn mode to " + spawnMode.ToString(), new Color?(Color.White), false);
					GameMain.Server.SendConsoleMessage("Set bot spawn mode to " + spawnMode.ToString(), client, null);
					return;
				}
				GameMain.Server.SendConsoleMessage("\"" + args[0] + "\" is not a valid bot spawn mode. (Valid modes are Fill and Normal)", client, new Color?(Color.Red));
			});
			DebugConsole.AssignOnExecute("killdisconnectedtimer", delegate(string[] args)
			{
				if (args.Length < 1 || GameMain.Server == null)
				{
					return;
				}
				float seconds;
				if (float.TryParse(args[0], out seconds))
				{
					seconds = Math.Max(0f, seconds);
					DebugConsole.NewMessage("Set kill disconnected timer to " + ToolBox.SecondsToReadableTime(seconds), new Color?(Color.White), false);
					GameMain.Server.ServerSettings.KillDisconnectedTime = seconds;
					return;
				}
				DebugConsole.NewMessage("\"" + args[0] + "\" is not a valid duration.", new Color?(Color.White), false);
			});
			DebugConsole.AssignOnClientRequestExecute("killdisconnectedtimer", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				if (args.Length < 1 || GameMain.Server == null)
				{
					return;
				}
				float seconds;
				if (float.TryParse(args[0], out seconds))
				{
					seconds = Math.Max(0f, seconds);
					GameMain.Server.SendConsoleMessage("Set kill disconnected timer to " + ToolBox.SecondsToReadableTime(seconds).Value, client, null);
					DebugConsole.NewMessage(client.Name + " set kill disconnected timer to " + ToolBox.SecondsToReadableTime(seconds), new Color?(Color.White), false);
					GameMain.Server.ServerSettings.KillDisconnectedTime = seconds;
					return;
				}
				GameMain.Server.SendConsoleMessage("\"" + args[0] + "\" is not a valid duration.", client, null);
			});
			DebugConsole.AssignOnExecute("autorestart", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				bool enabled = GameMain.Server.ServerSettings.AutoRestart;
				if (args.Length != 0)
				{
					bool.TryParse(args[0], out enabled);
				}
				else
				{
					enabled = !enabled;
				}
				if (enabled != GameMain.Server.ServerSettings.AutoRestart)
				{
					if (GameMain.Server.ServerSettings.AutoRestartInterval <= 0f)
					{
						GameMain.Server.ServerSettings.AutoRestartInterval = 10f;
					}
					GameMain.Server.ServerSettings.AutoRestartTimer = GameMain.Server.ServerSettings.AutoRestartInterval;
					GameMain.Server.ServerSettings.AutoRestart = enabled;
					NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
					ushort lastUpdateID = netLobbyScreen.LastUpdateID;
					netLobbyScreen.LastUpdateID = lastUpdateID + 1;
				}
				DebugConsole.NewMessage(GameMain.Server.ServerSettings.AutoRestart ? "Automatic restart enabled." : "Automatic restart disabled.", new Color?(Color.White), false);
			});
			DebugConsole.AssignOnExecute("autorestartinterval", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				int parsedInt;
				if (args.Length != 0 && int.TryParse(args[0], out parsedInt))
				{
					if (parsedInt >= 0)
					{
						GameMain.Server.ServerSettings.AutoRestart = true;
						GameMain.Server.ServerSettings.AutoRestartInterval = (float)parsedInt;
						if (GameMain.Server.ServerSettings.AutoRestartTimer >= GameMain.Server.ServerSettings.AutoRestartInterval)
						{
							GameMain.Server.ServerSettings.AutoRestartTimer = GameMain.Server.ServerSettings.AutoRestartInterval;
						}
						DebugConsole.NewMessage("Autorestart interval set to " + GameMain.Server.ServerSettings.AutoRestartInterval.ToString() + " seconds.", new Color?(Color.White), false);
					}
					else
					{
						GameMain.Server.ServerSettings.AutoRestart = false;
						DebugConsole.NewMessage("Autorestart disabled.", new Color?(Color.White), false);
					}
					NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
					ushort lastUpdateID = netLobbyScreen.LastUpdateID;
					netLobbyScreen.LastUpdateID = lastUpdateID + 1;
				}
			});
			DebugConsole.AssignOnExecute("autorestarttimer", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				int parsedInt;
				if (args.Length != 0 && int.TryParse(args[0], out parsedInt))
				{
					ushort lastUpdateID;
					if (parsedInt >= 0)
					{
						GameMain.Server.ServerSettings.AutoRestart = true;
						GameMain.Server.ServerSettings.AutoRestartTimer = (float)parsedInt;
						if (GameMain.Server.ServerSettings.AutoRestartInterval <= GameMain.Server.ServerSettings.AutoRestartTimer)
						{
							GameMain.Server.ServerSettings.AutoRestartInterval = GameMain.Server.ServerSettings.AutoRestartTimer;
						}
						NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
						lastUpdateID = netLobbyScreen.LastUpdateID;
						netLobbyScreen.LastUpdateID = lastUpdateID + 1;
						DebugConsole.NewMessage("Autorestart timer set to " + GameMain.Server.ServerSettings.AutoRestartTimer.ToString() + " seconds.", new Color?(Color.White), false);
					}
					else
					{
						GameMain.Server.ServerSettings.AutoRestart = false;
						DebugConsole.NewMessage("Autorestart disabled.", new Color?(Color.White), false);
					}
					NetLobbyScreen netLobbyScreen2 = GameMain.NetLobbyScreen;
					lastUpdateID = netLobbyScreen2.LastUpdateID;
					netLobbyScreen2.LastUpdateID = lastUpdateID + 1;
				}
			});
			DebugConsole.AssignOnExecute("startwhenclientsready", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				bool enabled = GameMain.Server.ServerSettings.StartWhenClientsReady;
				if (args.Length != 0)
				{
					bool.TryParse(args[0], out enabled);
				}
				else
				{
					enabled = !enabled;
				}
				if (enabled != GameMain.Server.ServerSettings.StartWhenClientsReady)
				{
					GameMain.Server.ServerSettings.StartWhenClientsReady = enabled;
					NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
					ushort lastUpdateID = netLobbyScreen.LastUpdateID;
					netLobbyScreen.LastUpdateID = lastUpdateID + 1;
				}
				DebugConsole.NewMessage(GameMain.Server.ServerSettings.StartWhenClientsReady ? "Enabled starting the round automatically when clients are ready." : "Disabled starting the round automatically when clients are ready.", new Color?(Color.White), false);
			});
			DebugConsole.AssignOnExecute("spawn|spawncharacter", delegate(string[] args)
			{
				DebugConsole.SpawnCharacter(args, Vector2.Zero, false);
			});
			DebugConsole.AssignOnExecute("spawnnpc", delegate(string[] args)
			{
				DebugConsole.SpawnCharacter(args, Vector2.Zero, true);
			});
			DebugConsole.AssignOnClientRequestExecute("spawn|spawncharacter", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				DebugConsole.SpawnCharacter(args, cursorPos, false);
			});
			DebugConsole.AssignOnClientRequestExecute("spawnnpc", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				DebugConsole.SpawnCharacter(args, cursorPos, true);
			});
			DebugConsole.AssignOnExecute("giveperm", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				if (args.Length < 1)
				{
					DebugConsole.NewMessage("giveperm [id/steamid/endpoint/name]: Grants administrative permissions to the player with the specified client.", new Color?(Color.Cyan), false);
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				DebugConsole.NewMessage("Valid permissions are:", new Color?(Color.White), false);
				foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
				{
					DebugConsole.NewMessage(" - " + ((ClientPermissions)obj).ToString(), new Color?(Color.White), false);
				}
				DebugConsole.ShowQuestionPrompt("Permission to grant to \"" + client.Name + "\"?", delegate(string perm)
				{
					ClientPermissions permission = ClientPermissions.None;
					if (!Enum.TryParse<ClientPermissions>(perm, true, out permission))
					{
						DebugConsole.NewMessage(perm + " is not a valid permission!", new Color?(Color.Red), false);
						return;
					}
					if (permission == ClientPermissions.None)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 2);
						defaultInterpolatedStringHandler.AppendLiteral("No permissions were given to ");
						defaultInterpolatedStringHandler.AppendFormatted(client.Name);
						defaultInterpolatedStringHandler.AppendLiteral(". Did you mean \"revokeperm ");
						defaultInterpolatedStringHandler.AppendFormatted(client.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" All\"?");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
						return;
					}
					client.GivePermission(permission);
					GameMain.Server.UpdateClientPermissions(client);
					DebugConsole.NewMessage(string.Concat(new string[]
					{
						"Granted ",
						perm,
						" permissions to ",
						client.Name,
						"."
					}), new Color?(Color.White), false);
				}, args, 1);
			});
			DebugConsole.AssignOnExecute("revokeperm", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				if (args.Length < 1)
				{
					DebugConsole.NewMessage("revokeperm [id/steamid/endpoint/name]: Revokes administrative permissions to the player with the specified client.", new Color?(Color.Cyan), false);
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				if (client.Connection == GameMain.Server.OwnerConnection)
				{
					DebugConsole.NewMessage("Cannot revoke permissions from the server owner!", new Color?(Color.Red), false);
					return;
				}
				if (args.Length < 2)
				{
					DebugConsole.NewMessage("Valid permissions are:", new Color?(Color.White), false);
					foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
					{
						DebugConsole.NewMessage(" - " + ((ClientPermissions)obj).ToString(), new Color?(Color.White), false);
					}
				}
				DebugConsole.ShowQuestionPrompt("Permission to revoke from \"" + client.Name + "\"?", delegate(string perm)
				{
					ClientPermissions permission = ClientPermissions.None;
					if (!Enum.TryParse<ClientPermissions>(perm, true, out permission))
					{
						DebugConsole.NewMessage(perm + " is not a valid permission!", new Color?(Color.Red), false);
						return;
					}
					client.RemovePermission(permission);
					GameMain.Server.UpdateClientPermissions(client);
					DebugConsole.NewMessage(string.Concat(new string[]
					{
						"Revoked ",
						perm,
						" permissions from ",
						client.Name,
						"."
					}), new Color?(Color.White), false);
				}, args, 1);
			});
			DebugConsole.AssignOnExecute("giverank", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				if (args.Length < 1)
				{
					DebugConsole.NewMessage("giverank [id/steamid/endpoint/name] [rank]: Assigns a specific rank (= a set of administrative permissions) to the player with the specified client ID.", new Color?(Color.Cyan), false);
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				if (client.Connection == GameMain.Server.OwnerConnection)
				{
					DebugConsole.NewMessage("Cannot modify the rank of the server owner!", new Color?(Color.Red), false);
					return;
				}
				DebugConsole.NewMessage("Valid ranks are:", new Color?(Color.White), false);
				foreach (PermissionPreset permissionPreset in PermissionPreset.List)
				{
					DebugConsole.NewMessage(" - " + permissionPreset.DisplayName, new Color?(Color.White), false);
				}
				DebugConsole.ShowQuestionPrompt("Rank to grant to \"" + client.Name + "\"?", delegate(string rank)
				{
					PermissionPreset preset = PermissionPreset.List.Find((PermissionPreset p) => p.DisplayName.Equals(rank, StringComparison.OrdinalIgnoreCase));
					if (preset == null)
					{
						DebugConsole.ThrowError("Rank \"" + rank + "\" not found.", null, null, false, false);
						return;
					}
					client.SetPermissions(preset.Permissions, preset.PermittedCommands);
					GameMain.Server.UpdateClientPermissions(client);
					DebugConsole.NewMessage("Assigned the rank \"" + preset.DisplayName + "\" to " + client.Name + ".", new Color?(Color.White), false);
				}, args, 1);
			});
			DebugConsole.AssignOnExecute("givecommandperm", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				if (args.Length < 1)
				{
					DebugConsole.NewMessage("givecommandperm [id/steamid/endpoint/name]: Gives the specified client the permission to use the specified console commands.", new Color?(Color.Cyan), false);
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				DebugConsole.ShowQuestionPrompt("Console command permissions to grant to \"" + client.Name + "\"? You may enter multiple commands separated with a space, or \"all\" to allow using any console command.", delegate(string commandsStr)
				{
					Identifier[] splitCommands = (from s in commandsStr.Split(' ', StringSplitOptions.None)
					select s.Trim()).ToIdentifiers().ToArray<Identifier>();
					bool giveAll = splitCommands.Length != 0 && splitCommands[0] == "all";
					List<DebugConsole.Command> grantedCommands = new List<DebugConsole.Command>();
					if (giveAll)
					{
						grantedCommands.AddRange(DebugConsole.commands);
					}
					else
					{
						Identifier[] array = splitCommands;
						for (int i = 0; i < array.Length; i++)
						{
							Identifier command = array[i];
							DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(command));
							if (matchingCommand == null)
							{
								DebugConsole.ThrowError("Could not find the command \"" + command.ToString() + "\"!", null, null, false, false);
							}
							else
							{
								grantedCommands.Add(matchingCommand);
							}
						}
					}
					client.GivePermission(ClientPermissions.ConsoleCommands);
					client.SetPermissions(client.Permissions, client.PermittedConsoleCommands.Union(grantedCommands).Distinct<DebugConsole.Command>().ToList<DebugConsole.Command>());
					GameMain.Server.UpdateClientPermissions(client);
					if (giveAll)
					{
						DebugConsole.NewMessage("Gave the client \"" + client.Name + "\" the permission to use all console commands.", new Color?(Color.White), false);
						return;
					}
					if (grantedCommands.Count > 0)
					{
						string[] array2 = new string[5];
						array2[0] = "Gave the client \"";
						array2[1] = client.Name;
						array2[2] = "\" the permission to use console commands ";
						array2[3] = string.Join<Identifier>(", ", from c in grantedCommands
						select c.Names[0]);
						array2[4] = ".";
						DebugConsole.NewMessage(string.Concat(array2), new Color?(Color.White), false);
					}
				}, args, 1);
			});
			DebugConsole.AssignOnExecute("revokecommandperm", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				if (args.Length < 1)
				{
					DebugConsole.NewMessage("revokecommandperm [id/steamid/endpoint/name]: Revokes permission to use the specified console commands from the specified client.", new Color?(Color.Cyan), false);
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				if (client.Connection == GameMain.Server.OwnerConnection)
				{
					DebugConsole.NewMessage("Cannot revoke command permissions from the server owner!", new Color?(Color.Red), false);
					return;
				}
				DebugConsole.ShowQuestionPrompt("Console command permissions to revoke from \"" + client.Name + "\"? You may enter multiple commands separated with a space.", delegate(string commandsStr)
				{
					Identifier[] splitCommands = commandsStr.ToIdentifiers(" ").ToArray<Identifier>();
					List<DebugConsole.Command> revokedCommands = new List<DebugConsole.Command>();
					bool revokeAll = splitCommands.Length != 0 && splitCommands[0] == "all";
					if (revokeAll)
					{
						revokedCommands.AddRange(DebugConsole.commands);
					}
					else
					{
						Identifier[] array = splitCommands;
						for (int i = 0; i < array.Length; i++)
						{
							Identifier command = array[i];
							DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(command));
							if (matchingCommand == null)
							{
								DebugConsole.ThrowError("Could not find the command \"" + command.ToString() + "\"!", null, null, false, false);
							}
							else
							{
								revokedCommands.Add(matchingCommand);
							}
						}
					}
					client.SetPermissions(client.Permissions, client.PermittedConsoleCommands.Except(revokedCommands).ToList<DebugConsole.Command>());
					GameMain.Server.UpdateClientPermissions(client);
					if (revokeAll)
					{
						DebugConsole.NewMessage("Revoked \"" + client.Name + "\"'s permission to use console commands.", new Color?(Color.White), false);
						return;
					}
					if (revokedCommands.Any<DebugConsole.Command>())
					{
						string[] array2 = new string[5];
						array2[0] = "Revoked \"";
						array2[1] = client.Name;
						array2[2] = "\"'s permission to use the console commands ";
						array2[3] = string.Join<Identifier>(", ", from c in revokedCommands
						select c.Names[0]);
						array2[4] = ".";
						DebugConsole.NewMessage(string.Concat(array2), new Color?(Color.White), false);
					}
				}, args, 1);
			});
			DebugConsole.AssignOnExecute("showperm", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				if (args.Length < 1)
				{
					DebugConsole.NewMessage("showperm [id/steamid/endpoint/name]: Shows the current administrative permissions of the specified client.", new Color?(Color.Cyan), false);
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				if (client.Permissions == ClientPermissions.None)
				{
					DebugConsole.NewMessage(client.Name + " has no special permissions.", new Color?(Color.White), false);
					return;
				}
				DebugConsole.NewMessage(client.Name + " has the following permissions:", new Color?(Color.White), false);
				foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
				{
					ClientPermissions permission = (ClientPermissions)obj;
					if (permission != ClientPermissions.None && client.HasPermission(permission))
					{
						DebugConsole.NewMessage("   - " + TextManager.Get("ClientPermission." + permission.ToString()), new Color?(Color.White), false);
					}
				}
				if (client.HasPermission(ClientPermissions.ConsoleCommands))
				{
					if (client.PermittedConsoleCommands.Count == 0)
					{
						DebugConsole.NewMessage("No permitted console commands:", new Color?(Color.White), false);
						return;
					}
					DebugConsole.NewMessage("Permitted console commands:", new Color?(Color.White), false);
					foreach (DebugConsole.Command permittedCommand in client.PermittedConsoleCommands)
					{
						DebugConsole.NewMessage("   - " + permittedCommand.Names[0].ToString(), new Color?(Color.White), false);
					}
				}
			});
			DebugConsole.AssignOnExecute("togglekarma", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				GameMain.Server.ServerSettings.KarmaEnabled = !GameMain.Server.ServerSettings.KarmaEnabled;
				DebugConsole.NewMessage(GameMain.Server.ServerSettings.KarmaEnabled ? "Karma system enabled." : "Karma system disabled.", new Color?(Color.LightGreen), false);
			});
			DebugConsole.AssignOnClientRequestExecute("togglekarma", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				GameMain.Server.ServerSettings.KarmaEnabled = !GameMain.Server.ServerSettings.KarmaEnabled;
				DebugConsole.NewMessage((GameMain.Server.ServerSettings.KarmaEnabled ? "Karma system enabled by " : "Karma system disabled by ") + client.Name, new Color?(Color.LightGreen), false);
				GameMain.Server.SendConsoleMessage(GameMain.Server.ServerSettings.KarmaEnabled ? "Karma system enabled." : "Karma system disabled.", client, null);
			});
			DebugConsole.AssignOnExecute("resetkarma", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Name == args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				client.Karma = 100f;
				DebugConsole.NewMessage("Set the karma of the client \"" + args[0] + "\" to 100.", new Color?(Color.LightGreen), false);
			});
			DebugConsole.AssignOnClientRequestExecute("resetkarma", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				Client targetClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Name == args[0]);
				if (targetClient == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				targetClient.Karma = 100f;
				GameMain.Server.SendDirectChatMessage("Set the karma of the client \"" + args[0] + "\" to 100.", client, ChatMessageType.Server);
				DebugConsole.NewMessage(string.Concat(new string[]
				{
					"Client \"",
					client.Name,
					"\" set the karma of \"",
					args[0],
					"\" to 100."
				}), new Color?(Color.LightGreen), false);
			});
			DebugConsole.AssignOnExecute("setkarma", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length < 2)
				{
					return;
				}
				Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Name == args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				float karmaValue;
				if (!float.TryParse(args[1], out karmaValue) || karmaValue < 0f || karmaValue > 100f)
				{
					DebugConsole.ThrowError("\"" + args[1] + "\" is not a valid karma value. You need to enter a number between 0-100.", null, null, false, false);
					return;
				}
				client.Karma = karmaValue;
				DebugConsole.NewMessage(string.Concat(new string[]
				{
					"Set the karma of the client \"",
					args[0],
					"\" to ",
					karmaValue.ToString(),
					"."
				}), new Color?(Color.LightGreen), false);
			});
			DebugConsole.AssignOnClientRequestExecute("setkarma", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (GameMain.Server == null || args.Length < 2)
				{
					return;
				}
				Client targetClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Name == args[0]);
				if (targetClient == null)
				{
					GameMain.Server.SendDirectChatMessage("Client \"" + args[0] + "\" not found.", client, ChatMessageType.Server);
					return;
				}
				float karmaValue;
				if (!float.TryParse(args[1], out karmaValue) || karmaValue < 0f || karmaValue > 100f)
				{
					GameMain.Server.SendDirectChatMessage("\"" + args[1] + "\" is not a valid karma value. You need to enter a number between 0-100.", client, ChatMessageType.Server);
					return;
				}
				targetClient.Karma = karmaValue;
				GameMain.Server.SendDirectChatMessage(string.Concat(new string[]
				{
					"Set the karma of the client \"",
					args[0],
					"\" to ",
					karmaValue.ToString(),
					"."
				}), client, ChatMessageType.Server);
				DebugConsole.NewMessage(string.Concat(new string[]
				{
					"Client \"",
					client.Name,
					"\" set the karma of \"",
					args[0],
					"\" to ",
					karmaValue.ToString(),
					"."
				}), new Color?(Color.LightGreen), false);
			});
			DebugConsole.AssignOnExecute("showkarma", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				DebugConsole.NewMessage("***************", new Color?(Color.Cyan), false);
				foreach (Client c in GameMain.Server.ConnectedClients)
				{
					DebugConsole.NewMessage(string.Concat(new string[]
					{
						"- ",
						c.SessionId.ToString(),
						": ",
						c.Name,
						(c.Character != null) ? (" playing " + c.Character.LogName) : "",
						", ",
						c.Karma.ToString()
					}), new Color?(Color.Cyan), false);
				}
				DebugConsole.NewMessage("***************", new Color?(Color.Cyan), false);
			});
			DebugConsole.AssignOnClientRequestExecute("showkarma", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				GameMain.Server.SendConsoleMessage("***************", client, null);
				foreach (Client c in GameMain.Server.ConnectedClients)
				{
					GameMain.Server.SendConsoleMessage(string.Concat(new string[]
					{
						"- ",
						c.SessionId.ToString(),
						": ",
						c.Name,
						(c.Character != null) ? (" playing " + c.Character.LogName) : "",
						", ",
						c.Karma.ToString()
					}), client, null);
				}
				GameMain.Server.SendConsoleMessage("***************", client, null);
			});
			DebugConsole.AssignOnExecute("togglekarmatestmode|karmatestmode", delegate(string[] args)
			{
				GameServer server = GameMain.Server;
				if (((server != null) ? server.KarmaManager : null) == null)
				{
					return;
				}
				GameMain.Server.KarmaManager.TestMode = !GameMain.Server.KarmaManager.TestMode;
				DebugConsole.NewMessage(GameMain.Server.KarmaManager.TestMode ? "Karma test mode enabled." : "Karma test mode disabled.", new Color?(Color.LightGreen), false);
			});
			DebugConsole.AssignOnClientRequestExecute("togglekarmatestmode|karmatestmode", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				GameServer server = GameMain.Server;
				if (((server != null) ? server.KarmaManager : null) == null)
				{
					return;
				}
				GameMain.Server.KarmaManager.TestMode = !GameMain.Server.KarmaManager.TestMode;
				DebugConsole.NewMessage(GameMain.Server.KarmaManager.TestMode ? ("Karma test mode enabled by " + client.Name + ".") : ("Karma test mode disabled by " + client.Name + "."), new Color?(Color.LightGreen), false);
				GameMain.Server.SendDirectChatMessage(GameMain.Server.KarmaManager.TestMode ? "Karma test mode enabled." : "Karma test mode disabled.", client, ChatMessageType.Server);
			});
			DebugConsole.AssignOnExecute("banaddress", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				Address address;
				if (!Address.Parse(args[0]).TryUnwrap(out address))
				{
					return;
				}
				Func<Client, bool> <>9__140;
				DebugConsole.ShowQuestionPrompt("Reason for banning the endpoint \"" + args[0] + "\"? (c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					DebugConsole.ShowQuestionPrompt("Enter the duration of the ban (leave empty to ban permanently, or use the format \"[days] d [hours] h\") (c to cancel)", delegate(string duration)
					{
						if (duration == "c" || duration == "C")
						{
							return;
						}
						TimeSpan? banDuration = null;
						if (!string.IsNullOrWhiteSpace(duration))
						{
							TimeSpan parsedBanDuration;
							if (!DebugConsole.TryParseTimeSpan(duration, out parsedBanDuration))
							{
								DebugConsole.ThrowError("\"" + duration + "\" is not a valid ban duration. Use the format \"[days] d [hours] h\", \"[days] d\" or \"[hours] h\".", null, null, false, false);
								return;
							}
							banDuration = new TimeSpan?(parsedBanDuration);
						}
						IEnumerable<Client> connectedClients = GameMain.Server.ConnectedClients;
						Func<Client, bool> predicate;
						if ((predicate = <>9__140) == null)
						{
							predicate = (<>9__140 = ((Client c) => c.AddressMatches(address)));
						}
						List<Client> clients = connectedClients.Where(predicate).ToList<Client>();
						if (clients.Count == 0)
						{
							GameMain.Server.ServerSettings.BanList.BanPlayer("Unnamed", address, reason, banDuration);
							return;
						}
						foreach (Client cl in clients)
						{
							GameMain.Server.BanClient(cl, reason, banDuration);
						}
					}, null, -1);
				}, null, -1);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("mute", "mute [name]: Prevent the client from speaking through the voice chat.", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Name == args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				client.Muted = true;
				GameMain.Server.SendDirectChatMessage(TextManager.Get("MutedByServer").Value, client, ChatMessageType.MessageBox);
			}, delegate()
			{
				if (GameMain.Server == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.Server.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("unmute", "unmute [name]: Allow the client to speak through the voice chat.", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Name == args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				client.Muted = false;
				GameMain.Server.SendDirectChatMessage(TextManager.Get("UnmutedByServer").Value, client, ChatMessageType.MessageBox);
			}, delegate()
			{
				if (GameMain.Server == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.Server.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.AssignOnExecute("netstats", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				GameMain.Server.ShowNetStats = !GameMain.Server.ShowNetStats;
			});
			DebugConsole.AssignOnExecute("setclientcharacter", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				if (args.Length < 2)
				{
					DebugConsole.ThrowError("Invalid parameters. The command should be formatted as \"setclientcharacter [client] [character]\". If the names consist of multiple words, you should surround them with quotation marks.", null, null, false, false);
					return;
				}
				Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Name == args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
				}
				Character character = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false);
				GameMain.Server.SetClientCharacter(client, character);
				client.SpectateOnly = false;
			});
			DebugConsole.AssignOnExecute("difficulty|leveldifficulty", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length < 1)
				{
					return;
				}
				float difficulty;
				if (float.TryParse(args[0], out difficulty))
				{
					DebugConsole.NewMessage("Set level difficulty setting to " + MathHelper.Clamp(difficulty, 0f, 100f).ToString(), new Color?(Color.White), false);
					GameMain.NetLobbyScreen.SetLevelDifficulty(difficulty);
					return;
				}
				DebugConsole.NewMessage(args[0] + " is not a valid difficulty setting (enter a value between 0-100)", new Color?(Color.Red), false);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("clientlist", "clientlist: List all the clients connected to the server.", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				DebugConsole.NewMessage("***************", new Color?(Color.Cyan), false);
				foreach (Client c in GameMain.Server.ConnectedClients)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 6);
					defaultInterpolatedStringHandler.AppendLiteral("- ");
					defaultInterpolatedStringHandler.AppendFormatted<byte>(c.SessionId);
					defaultInterpolatedStringHandler.AppendLiteral(": ");
					defaultInterpolatedStringHandler.AppendFormatted(c.Name);
					defaultInterpolatedStringHandler.AppendFormatted((c.Character != null) ? (" playing " + c.Character.LogName) : "");
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted(c.Connection.Endpoint.StringRepresentation);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<Option<AccountId>>(c.Connection.AccountInfo.AccountId);
					defaultInterpolatedStringHandler.AppendLiteral(", ping ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(c.Ping);
					defaultInterpolatedStringHandler.AppendLiteral(" ms");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Cyan), false);
				}
				DebugConsole.NewMessage("***************", new Color?(Color.Cyan), false);
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("clientlist", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				GameMain.Server.SendConsoleMessage("***************", client, new Color?(Color.Cyan));
				foreach (Client c in GameMain.Server.ConnectedClients)
				{
					GameServer server = GameMain.Server;
					string[] array = new string[7];
					array[0] = "- ";
					array[1] = c.SessionId.ToString();
					array[2] = ": ";
					array[3] = c.Name;
					array[4] = ", ";
					array[5] = c.Connection.Endpoint.StringRepresentation;
					int num = 6;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
					defaultInterpolatedStringHandler.AppendLiteral(", ping ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(c.Ping);
					defaultInterpolatedStringHandler.AppendLiteral(" ms");
					array[num] = defaultInterpolatedStringHandler.ToStringAndClear();
					server.SendConsoleMessage(string.Concat(array), client, new Color?(Color.Cyan));
				}
				GameMain.Server.SendConsoleMessage("***************", client, new Color?(Color.Cyan));
			});
			DebugConsole.commands.Add(new DebugConsole.Command("enablecheats", "enablecheats: Enables cheat commands and disables Steam achievements during this play session.", delegate(string[] args)
			{
				DebugConsole.CheatsEnabled = true;
				AchievementManager.CheatsEnabled = true;
				DebugConsole.NewMessage("Enabled cheat commands.", new Color?(Color.Red), false);
				DebugConsole.NewMessage("Steam achievements have been disabled during this play session.", new Color?(Color.Red), false);
				GameServer server = GameMain.Server;
				if (server == null)
				{
					return;
				}
				server.UpdateCheatsEnabled();
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("enablecheats", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				DebugConsole.CheatsEnabled = true;
				AchievementManager.CheatsEnabled = true;
				DebugConsole.NewMessage("Cheat commands have been enabled by \"" + client.Name + "\".", new Color?(Color.Red), false);
				DebugConsole.NewMessage("Steam achievements have been disabled during this play session.", new Color?(Color.Red), false);
				GameServer server = GameMain.Server;
				if (server == null)
				{
					return;
				}
				server.UpdateCheatsEnabled();
			});
			DebugConsole.AssignOnExecute("triggertraitorevent", delegate(string[] args)
			{
				GameServer server = GameMain.Server;
				if (((server != null) ? server.TraitorManager : null) == null)
				{
					DebugConsole.ThrowError("Could not start a traitor event. TraitorManager hasn't been created.", null, null, false, false);
					return;
				}
				if (args.Length != 0)
				{
					Identifier traitorEventId = args[0].ToIdentifier();
					EventPrefab prefab;
					if (EventPrefab.Prefabs.TryGet(traitorEventId, out prefab))
					{
						TraitorEventPrefab traitorEventPrefab = prefab as TraitorEventPrefab;
						if (traitorEventPrefab != null)
						{
							GameServer server2 = GameMain.Server;
							if (server2 == null)
							{
								goto IL_A1;
							}
							TraitorManager traitorManager2 = server2.TraitorManager;
							if (traitorManager2 == null)
							{
								goto IL_A1;
							}
							traitorManager2.ForceTraitorEvent(traitorEventPrefab);
							goto IL_A1;
						}
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find a traitor event prefab with the identifier \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(traitorEventId);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return;
				}
				IL_A1:
				GameServer server3 = GameMain.Server;
				TraitorManager traitorManager = (server3 != null) ? server3.TraitorManager : null;
				if (traitorManager != null)
				{
					traitorManager.Enabled = true;
					traitorManager.SkipStartDelay();
				}
			});
			DebugConsole.commands.Add(new DebugConsole.Command("traitorlist", "traitorlist: List all the traitors and their current objectives.", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				DebugConsole.<InitProjectSpecific>g__CreateTraitorList|13_45(delegate(string msg)
				{
					DebugConsole.NewMessage(msg, new Color?(Color.Cyan), false);
				});
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("traitorlist", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				DebugConsole.<InitProjectSpecific>g__CreateTraitorList|13_45(delegate(string msg)
				{
					GameMain.Server.SendDirectChatMessage(msg, client, ChatMessageType.Server);
				});
			});
			DebugConsole.AssignOnClientRequestExecute("debugevent", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				EventManager eventManager = (gameSession != null) ? gameSession.EventManager : null;
				if (eventManager != null && args.Length != 0)
				{
					Event ev2 = eventManager.ActiveEvents.FirstOrDefault(delegate(Event ev)
					{
						EventPrefab prefab = ev.Prefab;
						Identifier? identifier;
						Identifier? identifier2;
						if (prefab == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							identifier2 = new Identifier?(prefab.Identifier);
						}
						identifier = identifier2;
						return identifier == args[0];
					});
					if (ev2 == null)
					{
						GameMain.Server.SendConsoleMessage("Event \"" + args[0] + "\" not found.", client, null);
						return;
					}
					GameMain.Server.SendConsoleMessage(ev2.GetDebugInfo(), client, null);
				}
			});
			DebugConsole.commands.Add(new DebugConsole.Command("debugjobassignment", "debugjobassignment: Shows information about how jobs were assigned for the most recent round.", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				foreach (string debugMsg in GameMain.Server.JobAssignmentDebugLog)
				{
					DebugConsole.NewMessage(debugMsg, new Color?(Color.Cyan), false);
				}
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("debugjobassignment", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				foreach (string debugMsg in GameMain.Server.JobAssignmentDebugLog)
				{
					GameMain.Server.SendConsoleMessage(debugMsg, client, null);
				}
			});
			DebugConsole.commands.Add(new DebugConsole.Command("setpassword|setserverpassword|password", "setpassword [password]: Changes the password of the server that's being hosted.", delegate(string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				GameMain.Server.ServerSettings.SetPassword((args.Length != 0) ? args[0] : "");
				DebugConsole.NewMessage(GameMain.Server.ServerSettings.HasPassword ? "Changed the server password." : "Removed password protection from the server.", null, false);
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("setpassword", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				if (GameMain.Server == null)
				{
					return;
				}
				GameMain.Server.ServerSettings.SetPassword((args.Length != 0) ? args[0] : "");
				DebugConsole.NewMessage(client.Name + " " + (GameMain.Server.ServerSettings.HasPassword ? (" changed the server password to \"" + args[0] + "\".") : " removed password protection from the server."), null, false);
				GameMain.Server.SendChatMessage(TextManager.GetWithVariable(GameMain.Server.ServerSettings.HasPassword ? "PasswordChangedByClient" : "PasswordRemovedByClient", "[clientname]", client.Name, FormatCapitals.No).Value, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("setmaxplayers|maxplayers", "setmaxplayers [max players]: Sets the maximum player count of the server that's being hosted.", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				int maxPlayers;
				if (!int.TryParse(args[0], out maxPlayers))
				{
					DebugConsole.NewMessage(args[0] + " is not a valid player count.", null, false);
					return;
				}
				if (maxPlayers > NetConfig.MaxPlayers)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(130, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Setting the maximum amount of players to ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(maxPlayers);
					defaultInterpolatedStringHandler.AppendLiteral(" failed due to exceeding the limit of ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(NetConfig.MaxPlayers);
					defaultInterpolatedStringHandler.AppendLiteral(" players per server. Using the maximum of ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(NetConfig.MaxPlayers);
					defaultInterpolatedStringHandler.AppendLiteral(" instead.");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					maxPlayers = NetConfig.MaxPlayers;
				}
				GameMain.Server.ServerSettings.MaxPlayers = maxPlayers;
				DebugConsole.NewMessage("Set the maximum player count to " + maxPlayers.ToString() + ".", null, false);
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("setmaxplayers", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				int maxPlayers;
				if (!int.TryParse(args[0], out maxPlayers))
				{
					GameMain.Server.SendConsoleMessage(args[0] + " is not a valid player count.", client, new Color?(Color.Red));
					return;
				}
				if (maxPlayers > NetConfig.MaxPlayers)
				{
					GameServer server = GameMain.Server;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(130, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Setting the maximum amount of players to ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(maxPlayers);
					defaultInterpolatedStringHandler.AppendLiteral(" failed due to exceeding the limit of ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(NetConfig.MaxPlayers);
					defaultInterpolatedStringHandler.AppendLiteral(" players per server. Using the maximum of ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(NetConfig.MaxPlayers);
					defaultInterpolatedStringHandler.AppendLiteral(" instead.");
					server.SendConsoleMessage(defaultInterpolatedStringHandler.ToStringAndClear(), client, new Color?(Color.Red));
					maxPlayers = NetConfig.MaxPlayers;
				}
				GameMain.Server.ServerSettings.MaxPlayers = maxPlayers;
				DebugConsole.NewMessage(client.Name + " set the maximum player count to " + maxPlayers.ToString() + ".", null, false);
				GameMain.Server.SendConsoleMessage("Set the maximum player count to " + maxPlayers.ToString() + ".", client, null);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("restart|reset", "restart/reset: Close and restart the server.", delegate(string[] args)
			{
				DebugConsole.NewMessage("*****************", new Color?(Color.Lime), false);
				DebugConsole.NewMessage("RESTARTING SERVER", new Color?(Color.Lime), false);
				DebugConsole.NewMessage("*****************", new Color?(Color.Lime), false);
				GameServer.Log("Console command \"restart\" executed: closing the server...", ServerLog.MessageType.ServerMessage);
				GameMain.Instance.CloseServer();
				Program.TryStartChildServerRelay(GameMain.Instance.CommandLineArgs);
				GameMain.Instance.StartServer();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("exit|quit|close", "exit/quit/close: Exit the application.", delegate(string[] args)
			{
				GameMain.ShouldRun = false;
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("say", "say [message]: Send a global chat message. When issued through the server command line, displays \"HOST\" as the sender.", delegate(string[] args)
			{
				string text = string.Join(" ", args);
				text = "HOST: " + text;
				GameMain.Server.SendChatMessage(text, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("say", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				string text = string.Join(" ", args);
				text = client.Name + ": " + text;
				if (GameMain.Server.OwnerConnection != null && client.Connection == GameMain.Server.OwnerConnection)
				{
					text = "[HOST] " + text;
				}
				GameMain.Server.SendChatMessage(text, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("msg", "msg [message]: Send a chat message with no sender specified.", delegate(string[] args)
			{
				string text = string.Join(" ", args);
				GameMain.Server.SendChatMessage(text, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("msg", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				string text = string.Join(" ", args);
				GameMain.Server.SendChatMessage(text, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("servername", "servername [name]: Change the name of the server.", delegate(string[] args)
			{
				GameMain.Server.ServerName = string.Join(" ", args);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("servermsg", "servermsg [message]: Change the message displayed in the server lobby.", delegate(string[] args)
			{
				GameMain.Server.ServerSettings.ServerMessageText = string.Join(" ", args);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("seed|levelseed", "seed/levelseed: Changes the level seed for the next round.", delegate(string[] args)
			{
				GameMain.NetLobbyScreen.LevelSeed = string.Join(" ", args);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("randomizeseed", "randomizeseed: Toggles level seed randomization on/off.", delegate(string[] args)
			{
				GameMain.Server.ServerSettings.RandomizeSeed = !GameMain.Server.ServerSettings.RandomizeSeed;
				DebugConsole.NewMessage((GameMain.Server.ServerSettings.RandomizeSeed ? "Enabled" : "Disabled") + " level seed randomization.", new Color?(Color.Cyan), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("gamemode", "gamemode [name]/[index]: Select the game mode for the next round. The parameter can either be the name or the index number of the game mode (0 = sandbox, 1 = mission, etc).", delegate(string[] args)
			{
				int index = -1;
				if (string.Join("", args).Trim().Length <= 0)
				{
					DebugConsole.NewMessage("Current gamemode is " + GameMain.NetLobbyScreen.GameModes[GameMain.NetLobbyScreen.SelectedModeIndex].Name, new Color?(Color.Cyan), false);
					return;
				}
				if (int.TryParse(string.Join(" ", args), out index))
				{
					if (index > 0 && index < GameMain.NetLobbyScreen.GameModes.Length && GameMain.NetLobbyScreen.GameModes[index] == GameModePreset.MultiPlayerCampaign)
					{
						MultiPlayerCampaign.StartCampaignSetup();
						return;
					}
					GameMain.NetLobbyScreen.SelectedModeIndex = index;
					DebugConsole.NewMessage("Set gamemode to " + GameMain.NetLobbyScreen.GameModes[GameMain.NetLobbyScreen.SelectedModeIndex].Name, new Color?(Color.Cyan), false);
					return;
				}
				else
				{
					string modeName = string.Join(" ", args);
					if (modeName.Equals("campaign", StringComparison.OrdinalIgnoreCase))
					{
						MultiPlayerCampaign.StartCampaignSetup();
						return;
					}
					GameModePreset gameMode = GameModePreset.List.Find((GameModePreset gm) => gm.Name.ToLower() == modeName.ToLower());
					if (gameMode == null)
					{
						DebugConsole.ThrowError("Game mode \"" + modeName + "\" not found!", null, null, false, false);
						return;
					}
					GameMain.NetLobbyScreen.SelectedModeIdentifier = gameMode.Identifier;
					DebugConsole.NewMessage("Set gamemode to " + gameMode.Name, new Color?(Color.Cyan), false);
					return;
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from gm in GameModePreset.List
				select gm.Name.Value).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("mission", "mission [name]: Select the mission type for the next round.", delegate(string[] args)
			{
				GameMain.NetLobbyScreen.MissionTypes = args.ToIdentifiers();
				DebugConsole.NewMessage("Set mission to " + string.Join<Identifier>(",", GameMain.NetLobbyScreen.MissionTypes), new Color?(Color.Cyan), false);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from id in MissionPrefab.GetAllMultiplayerSelectableMissionTypes()
				select id.Value).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("sub|submarine", "submarine [name]: Select the submarine for the next round.", delegate(string[] args)
			{
				SubmarineInfo sub = GameMain.NetLobbyScreen.GetSubList().Find((SubmarineInfo s) => s.Name.Equals(string.Join(" ", args), StringComparison.OrdinalIgnoreCase));
				if (sub != null)
				{
					GameMain.NetLobbyScreen.SelectedSub = sub;
				}
				sub = GameMain.NetLobbyScreen.SelectedSub;
				DebugConsole.NewMessage("Selected sub: " + sub.Name + (sub.HasTag(SubmarineTag.Shuttle) ? " (shuttle)" : ""), new Color?(Color.Cyan), false);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from s in SubmarineInfo.SavedSubmarines
				select s.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("shuttle", "shuttle [name]: Select the specified submarine as the respawn shuttle for the next round.", delegate(string[] args)
			{
				SubmarineInfo shuttle = GameMain.NetLobbyScreen.GetSubList().Find((SubmarineInfo s) => s.Name.ToLower() == string.Join(" ", args).ToLower());
				if (shuttle != null)
				{
					GameMain.NetLobbyScreen.SelectedShuttle = shuttle;
				}
				shuttle = GameMain.NetLobbyScreen.SelectedShuttle;
				DebugConsole.NewMessage("Selected shuttle: " + shuttle.Name + (shuttle.HasTag(SubmarineTag.Shuttle) ? "" : " (not shuttle)"), new Color?(Color.Cyan), false);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from s in SubmarineInfo.SavedSubmarines
				select s.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.AssignOnExecute("respawnnow", delegate(string[] args)
			{
				GameServer server = GameMain.Server;
				if (((server != null) ? server.RespawnManager : null) == null)
				{
					return;
				}
				GameMain.Server.RespawnManager.ForceRespawn();
			});
			DebugConsole.commands.Add(new DebugConsole.Command("startgame|startround|start", "start/startgame/startround: Start a new round.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.GameScreen)
				{
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
				if (mpCampaign != null && GameMain.NetLobbyScreen.SelectedMode == GameModePreset.MultiPlayerCampaign)
				{
					MultiPlayerCampaign.LoadCampaign(GameMain.GameSession.DataPath, null);
					return;
				}
				if (GameMain.NetLobbyScreen.SelectedMode == GameModePreset.MultiPlayerCampaign)
				{
					MultiPlayerCampaign.StartCampaignSetup();
					return;
				}
				GameServer.TryStartGameResult result = GameMain.Server.TryStartGame();
				if (result != GameServer.TryStartGameResult.Success)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to start a new round: ");
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("TryStartGameError.");
					defaultInterpolatedStringHandler2.AppendFormatted<GameServer.TryStartGameResult>(result);
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()));
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Yellow), false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("endgame|endround|end", "end/endgame/endround: End the current round.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.NetLobbyScreen)
				{
					return;
				}
				GameMain.Server.EndGame(CampaignMode.TransitionType.None, false, null);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("entitydata", "", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				Entity ent = Entity.FindEntityByID(Convert.ToUInt16(args[0]));
				if (ent != null)
				{
					DebugConsole.NewMessage(ent.ToString(), new Color?(Color.Lime), false);
				}
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("banaddress|banip", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				Address address;
				if (!Address.Parse(args[0]).TryUnwrap(out address))
				{
					return;
				}
				List<Client> clients = (from c in GameMain.Server.ConnectedClients
				where c.AddressMatches(address)
				select c).ToList<Client>();
				TimeSpan? duration = null;
				if (args.Length > 1)
				{
					double durationSeconds;
					if (!double.TryParse(args[1], out durationSeconds))
					{
						GameMain.Server.SendConsoleMessage("\"" + args[1] + "\" is not a valid ban duration.", client, new Color?(Color.Red));
						return;
					}
					if (durationSeconds > 0.0)
					{
						duration = new TimeSpan?(TimeSpan.FromSeconds(durationSeconds));
					}
				}
				string reason = "";
				if (args.Length > 2)
				{
					reason = string.Join(" ", args.Skip(2));
				}
				if (clients.Count == 0)
				{
					GameMain.Server.ServerSettings.BanList.BanPlayer("Unnamed", address, reason, duration);
					return;
				}
				foreach (Client cl in clients)
				{
					GameMain.Server.BanClient(cl, reason, duration);
				}
			});
			DebugConsole.commands.Add(new DebugConsole.Command("unban", "unban [name]: Unban a specific client.", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				string clientName = string.Join(" ", args);
				GameMain.Server.UnbanPlayer(clientName);
			}, delegate()
			{
				if (GameMain.Server == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from name in GameMain.Server.ServerSettings.BanList.BannedNames
				where !string.IsNullOrEmpty(name)
				select name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("unbanaddress", "unbanaddress [endpoint]: Unban a specific endpoint.", delegate(string[] args)
			{
				if (GameMain.Server == null || args.Length == 0)
				{
					return;
				}
				Endpoint endpoint;
				if (Endpoint.Parse(args[0]).TryUnwrap(out endpoint))
				{
					GameMain.Server.UnbanPlayer(endpoint);
				}
			}, delegate()
			{
				if (GameMain.Server == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from ep in GameMain.Server.ServerSettings.BanList.BannedAddresses
				select ep.ToString()).ToArray<string>();
				return array;
			}, false));
			DebugConsole.AssignOnClientRequestExecute("eventmanager", delegate(Client client, Vector2 cursorPos, string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.EventManager : null) != null)
				{
					GameMain.GameSession.EventManager.Enabled = !GameMain.GameSession.EventManager.Enabled;
					DebugConsole.NewMessage(GameMain.GameSession.EventManager.Enabled ? "Event manager on" : "Event manager off", new Color?(Color.White), false);
				}
			});
			DebugConsole.AssignOnClientRequestExecute("spawnitem", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				string errorMsg;
				DebugConsole.SpawnItem(args, cursorWorldPos, client.Character, out errorMsg);
				if (!string.IsNullOrWhiteSpace(errorMsg))
				{
					GameMain.Server.SendConsoleMessage(errorMsg, client, new Color?(Color.Red));
				}
			});
			DebugConsole.AssignOnClientRequestExecute("give", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (client.Character == null)
				{
					GameMain.Server.SendConsoleMessage("No character is selected!", client, new Color?(Color.Red));
					return;
				}
				if (args.Length == 0)
				{
					GameMain.Server.SendConsoleMessage("Please give the name or identifier of the item to spawn.", client, new Color?(Color.Red));
					return;
				}
				List<string> modifiedArgs = new List<string>(args);
				modifiedArgs.Insert(1, "inventory");
				string errorMsg;
				DebugConsole.SpawnItem(modifiedArgs.ToArray(), cursorWorldPos, client.Character, out errorMsg);
				if (!string.IsNullOrWhiteSpace(errorMsg))
				{
					GameMain.Server.SendConsoleMessage(errorMsg, client, new Color?(Color.Red));
				}
			});
			DebugConsole.AssignOnClientRequestExecute("disablecrewai", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				HumanAIController.DisableCrewAI = true;
				DebugConsole.NewMessage("Crew AI disabled by \"" + client.Name + "\"", new Color?(Color.White), false);
				GameMain.Server.SendConsoleMessage("Crew AI disabled", client, null);
			});
			DebugConsole.AssignOnClientRequestExecute("enablecrewai", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				HumanAIController.DisableCrewAI = false;
				DebugConsole.NewMessage("Crew AI enabled by \"" + client.Name + "\"", new Color?(Color.White), false);
				GameMain.Server.SendConsoleMessage("Crew AI enabled", client, null);
			});
			DebugConsole.AssignOnClientRequestExecute("botcount", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 1 || GameMain.Server == null)
				{
					return;
				}
				int botCount = GameMain.Server.ServerSettings.BotCount;
				int.TryParse(args[0], out botCount);
				GameMain.NetLobbyScreen.SetBotCount(botCount);
				DebugConsole.NewMessage("\"" + client.Name + "\" set the number of bots to " + botCount.ToString(), new Color?(Color.White), false);
				GameMain.Server.SendConsoleMessage("Set the number of bots to " + botCount.ToString(), client, null);
			});
			DebugConsole.AssignOnClientRequestExecute("botspawnmode", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 1 || GameMain.Server == null)
				{
					return;
				}
				BotSpawnMode spawnMode;
				if (Enum.TryParse<BotSpawnMode>(args[0], true, out spawnMode))
				{
					GameMain.NetLobbyScreen.SetBotSpawnMode(spawnMode);
					DebugConsole.NewMessage("\"" + client.Name + "\" set bot spawn mode to " + spawnMode.ToString(), new Color?(Color.White), false);
					GameMain.Server.SendConsoleMessage("Set bot spawn mode to " + spawnMode.ToString(), client, null);
					return;
				}
				GameMain.Server.SendConsoleMessage("\"" + args[0] + "\" is not a valid bot spawn mode. (Valid modes are Fill and Normal)", client, new Color?(Color.Red));
			});
			DebugConsole.AssignOnExecute("teleportcharacter|teleport", delegate(string[] args)
			{
				Submarine mainSub = Submarine.MainSub;
				DebugConsole.TeleportCharacter((mainSub != null) ? mainSub.WorldPosition : Vector2.Zero, Character.Controlled, args);
			});
			DebugConsole.AssignOnClientRequestExecute("teleportcharacter|teleport", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				DebugConsole.TeleportCharacter(cursorWorldPos, client.Character, args);
			});
			DebugConsole.AssignOnClientRequestExecute("teleportsub", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (Submarine.MainSub == null || Level.Loaded == null)
				{
					return;
				}
				Submarine submarineToTeleport = Submarine.MainSub;
				if (args.Length > 1)
				{
					foreach (Submarine sub in from s in Submarine.Loaded
					where s.PhysicsBody.BodyType == BodyType.Dynamic
					select s)
					{
						if (sub.Info.Name + "_" + sub.TeamID.ToString() == args[1])
						{
							submarineToTeleport = sub;
							break;
						}
					}
				}
				if (args.Length == 0 || args[0].Equals("cursor", StringComparison.OrdinalIgnoreCase))
				{
					submarineToTeleport.SetPosition(cursorWorldPos, null, true);
					return;
				}
				if (args[0].Equals("start", StringComparison.OrdinalIgnoreCase))
				{
					submarineToTeleport.SetPosition(Level.Loaded.StartPosition - Vector2.UnitY * (float)submarineToTeleport.Borders.Height, null, true);
					return;
				}
				if (args[0].Equals("end", StringComparison.OrdinalIgnoreCase))
				{
					submarineToTeleport.SetPosition(Level.Loaded.EndPosition - Vector2.UnitY * (float)submarineToTeleport.Borders.Height, null, true);
					return;
				}
				if (args[0].Equals("endoutpost", StringComparison.OrdinalIgnoreCase))
				{
					submarineToTeleport.SetPosition(Level.Loaded.EndExitPosition - Vector2.UnitY * (float)submarineToTeleport.Borders.Height, null, true);
					DockingPort submarineDockingPort = DockingPort.List.FirstOrDefault((DockingPort d) => d.Item.Submarine == submarineToTeleport);
					Level loaded = Level.Loaded;
					if (((loaded != null) ? loaded.EndOutpost : null) == null)
					{
						DebugConsole.NewMessage("Can't teleport the sub to the end outpost (no outpost at the end of the level).", new Color?(Color.Red), false);
						return;
					}
					DockingPort outpostDockingPort = DockingPort.List.FirstOrDefault((DockingPort d) => d.Item.Submarine == Level.Loaded.EndOutpost);
					if (submarineDockingPort != null && outpostDockingPort != null)
					{
						submarineDockingPort.Dock(outpostDockingPort);
					}
				}
			});
			DebugConsole.AssignOnClientRequestExecute("togglecampaignteleport", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
				if (mpCampaign == null)
				{
					GameMain.Server.SendConsoleMessage("No campaign active.", client, new Color?(Color.Red));
					return;
				}
				mpCampaign.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions);
				GameMain.GameSession.Map.AllowDebugTeleport = !GameMain.GameSession.Map.AllowDebugTeleport;
				DebugConsole.NewMessage(client.Name + (GameMain.GameSession.Map.AllowDebugTeleport ? " enabled" : " disabled") + " teleportation on the campaign map.", new Color?(Color.White), false);
				GameMain.Server.SendConsoleMessage((GameMain.GameSession.Map.AllowDebugTeleport ? "Enabled" : "Disabled") + " teleportation on the campaign map.", client, null);
			});
			DebugConsole.AssignOnClientRequestExecute("godmode", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				DebugConsole.<>c__DisplayClass13_24 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass13_24();
				CS$<>8__locals1.args = args;
				CS$<>8__locals1.client = client;
				CS$<>8__locals1.godmodeStateOnFirstCharacter = null;
				DebugConsole.HandleCommandForCrewOrSingleCharacter(CS$<>8__locals1.args, new Action<Character>(CS$<>8__locals1.<InitProjectSpecific>g__ToggleGodMode|163), CS$<>8__locals1.client);
			});
			DebugConsole.AssignOnClientRequestExecute("godmode_mainsub", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (Submarine.MainSub == null)
				{
					return;
				}
				Submarine.MainSub.GodMode = !Submarine.MainSub.GodMode;
				DebugConsole.NewMessage((Submarine.MainSub.GodMode ? "Mainsub godmode turned on by \"" : "Mainsub godmode turned off by \"") + client.Name + "\"", new Color?(Color.White), false);
				GameMain.Server.SendConsoleMessage(Submarine.MainSub.GodMode ? "Mainsub godmode on" : "Mainsub godmode off", client, null);
			});
			DebugConsole.AssignOnClientRequestExecute("giveaffliction", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					return;
				}
				string affliction = args[0];
				AfflictionPrefab afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab a) => a.Identifier == affliction);
				if (afflictionPrefab == null)
				{
					afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab a) => a.Name.Equals(affliction, StringComparison.OrdinalIgnoreCase));
				}
				if (afflictionPrefab == null)
				{
					GameMain.Server.SendConsoleMessage("Affliction \"" + affliction + "\" not found.", client, new Color?(Color.Red));
					return;
				}
				float afflictionStrength;
				if (!float.TryParse(args[1], out afflictionStrength))
				{
					GameMain.Server.SendConsoleMessage("\"" + args[1] + "\" is not a valid affliction strength.", client, new Color?(Color.Red));
					return;
				}
				bool relativeStrength = false;
				if (args.Length > 4)
				{
					bool.TryParse(args[4], out relativeStrength);
				}
				Character targetCharacter = (args.Length <= 2) ? client.Character : DebugConsole.FindMatchingCharacter(args.Skip(2).ToArray<string>(), false, null, false);
				if (targetCharacter != null)
				{
					Limb targetLimb = targetCharacter.AnimController.MainLimb;
					if (args.Length > 3)
					{
						targetLimb = targetCharacter.AnimController.Limbs.FirstOrDefault((Limb l) => l.type.ToString().Equals(args[3], StringComparison.OrdinalIgnoreCase));
					}
					if (relativeStrength)
					{
						afflictionStrength *= targetCharacter.MaxVitality / afflictionPrefab.MaxStrength;
					}
					targetCharacter.CharacterHealth.ApplyAffliction(targetLimb ?? targetCharacter.AnimController.MainLimb, afflictionPrefab.Instantiate(afflictionStrength, null), true, false, true);
				}
			});
			DebugConsole.AssignOnClientRequestExecute("healme", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				bool healAll = args.Length != 0 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase);
				if (client.Character != null)
				{
					DebugConsole.HealCharacter(client.Character, healAll, client);
				}
			});
			DebugConsole.AssignOnClientRequestExecute("heal", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				bool healAll = args.Length > 1 && args[1].Equals("all", StringComparison.OrdinalIgnoreCase);
				DebugConsole.HandleCommandForCrewOrSingleCharacter(args, delegate(Character targetCharacter)
				{
					DebugConsole.HealCharacter(targetCharacter, healAll, client);
				}, client);
			});
			DebugConsole.AssignOnClientRequestExecute("revive", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				Character revivedCharacter = (args.Length == 0) ? client.Character : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (revivedCharacter == null)
				{
					return;
				}
				revivedCharacter.Revive(true, false);
				if (GameMain.Server != null)
				{
					foreach (Client c in GameMain.Server.ConnectedClients)
					{
						if (c.Character == revivedCharacter)
						{
							ServerSettings serverSettings = GameMain.Server.ServerSettings;
							if (serverSettings != null && serverSettings.IronmanModeActive)
							{
								GameSession gameSession = GameMain.GameSession;
								MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
								if (mpCampaign != null)
								{
									CharacterCampaignData characterToRestore = mpCampaign.RestoreSingleCharacterFromBackup(c);
									if (characterToRestore != null)
									{
										characterToRestore.CharacterInfo.PermanentlyDead = false;
										mpCampaign.SaveSingleCharacter(characterToRestore, true);
									}
								}
							}
							GameMain.Server.SetClientCharacter(c, revivedCharacter);
							break;
						}
					}
				}
			});
			DebugConsole.AssignOnClientRequestExecute("givetalent", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				Character targetCharacter = (args.Length >= 2) ? DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false) : client.Character;
				if (targetCharacter == null)
				{
					return;
				}
				TalentPrefab talentPrefab = TalentPrefab.TalentPrefabs.Find((TalentPrefab c) => c.Identifier == args[0] || c.DisplayName.Equals(args[0], StringComparison.OrdinalIgnoreCase));
				if (talentPrefab == null)
				{
					GameMain.Server.SendConsoleMessage("Couldn't find the talent \"" + args[0] + "\".", client, new Color?(Color.Red));
					return;
				}
				targetCharacter.GiveTalent(talentPrefab, true);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Talent \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(talentPrefab.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral("\" given to \"");
				defaultInterpolatedStringHandler.AppendFormatted(targetCharacter.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" by \"");
				defaultInterpolatedStringHandler.AppendFormatted(client.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
				GameServer server = GameMain.Server;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Gave talent \"");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(talentPrefab.DisplayName);
				defaultInterpolatedStringHandler2.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler2.AppendFormatted(targetCharacter.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("\".");
				server.SendConsoleMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), client, null);
			});
			DebugConsole.AssignOnClientRequestExecute("unlocktalents", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				Character targetCharacter = (args.Length >= 2) ? DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false) : Character.Controlled;
				if (targetCharacter == null)
				{
					return;
				}
				List<TalentTree> talentTrees = new List<TalentTree>();
				if (args.Length == 0 || args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
				{
					talentTrees.AddRange(TalentTree.JobTalentTrees);
				}
				else
				{
					JobPrefab job = JobPrefab.Prefabs.Find((JobPrefab jp) => jp.Name != null && jp.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
					if (job == null)
					{
						GameMain.Server.SendConsoleMessage("Failed to find the job \"" + args[0] + "\".", client, new Color?(Color.Red));
						return;
					}
					TalentTree talentTree;
					if (!TalentTree.JobTalentTrees.TryGet(job.Identifier, out talentTree))
					{
						GameMain.Server.SendConsoleMessage("No talents configured for the job \"" + args[0] + "\".", client, new Color?(Color.Red));
						return;
					}
					talentTrees.Add(talentTree);
				}
				foreach (TalentTree talentTree2 in talentTrees)
				{
					foreach (Identifier talentId in talentTree2.AllTalentIdentifiers)
					{
						TalentPrefab talentPrefab;
						if (TalentPrefab.TalentPrefabs.TryGet(talentId, out talentPrefab))
						{
							targetCharacter.GiveTalent(talentPrefab, true);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Talent \"");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(talentPrefab.DisplayName);
							defaultInterpolatedStringHandler.AppendLiteral("\" given to \"");
							defaultInterpolatedStringHandler.AppendFormatted(targetCharacter.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\" by \"");
							defaultInterpolatedStringHandler.AppendFormatted(client.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\".");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
							GameServer server = GameMain.Server;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Gave talent \"");
							defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(talentPrefab.DisplayName);
							defaultInterpolatedStringHandler2.AppendLiteral("\" to \"");
							defaultInterpolatedStringHandler2.AppendFormatted(targetCharacter.Name);
							defaultInterpolatedStringHandler2.AppendLiteral("\".");
							server.SendConsoleMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), client, null);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(19, 1);
							defaultInterpolatedStringHandler3.AppendLiteral("Unlocked talent \"");
							defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(talentPrefab.DisplayName);
							defaultInterpolatedStringHandler3.AppendLiteral("\".");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), null, false);
						}
					}
				}
			});
			DebugConsole.AssignOnClientRequestExecute("freeze", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (client.Character != null)
				{
					client.Character.AnimController.Frozen = !client.Character.AnimController.Frozen;
				}
			});
			DebugConsole.AssignOnClientRequestExecute("ragdoll", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				Character ragdolledCharacter = (args.Length == 0) ? client.Character : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (ragdolledCharacter != null)
				{
					ragdolledCharacter.IsForceRagdolled = !ragdolledCharacter.IsForceRagdolled;
				}
			});
			DebugConsole.AssignOnClientRequestExecute("explosion", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				float range = 500f;
				float force = 10f;
				float damage = 50f;
				float structureDamage = 20f;
				float itemDamage = 100f;
				float empStrength = 0f;
				float ballastFloraStrength = 50f;
				if (args.Length != 0)
				{
					float.TryParse(args[0], out range);
				}
				if (args.Length > 1)
				{
					float.TryParse(args[1], out force);
				}
				if (args.Length > 2)
				{
					float.TryParse(args[2], out damage);
				}
				if (args.Length > 3)
				{
					float.TryParse(args[3], out structureDamage);
				}
				if (args.Length > 4)
				{
					float.TryParse(args[4], out itemDamage);
				}
				if (args.Length > 5)
				{
					float.TryParse(args[5], out empStrength);
				}
				if (args.Length > 6)
				{
					float.TryParse(args[6], out ballastFloraStrength);
				}
				new Explosion(range, force, damage, structureDamage, itemDamage, empStrength, ballastFloraStrength).Explode(cursorWorldPos, null, null);
			});
			DebugConsole.AssignOnClientRequestExecute("kill", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				Character killedCharacter = (args.Length == 0) ? client.Character : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (killedCharacter == null)
				{
					GameMain.Server.SendConsoleMessage("Could not find the specified character.", client, new Color?(Color.Red));
				}
				if (killedCharacter != null)
				{
					killedCharacter.Kill(CauseOfDeathType.Unknown, null, false, true);
				}
			});
			DebugConsole.AssignOnClientRequestExecute("control", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				Character character = DebugConsole.FindMatchingCharacter(args, true, client, false);
				if (character != null)
				{
					GameMain.Server.SetClientCharacter(client, character);
					client.SpectateOnly = false;
					return;
				}
				GameMain.Server.SendConsoleMessage("Could not find the specified character.", client, new Color?(Color.Red));
			});
			DebugConsole.AssignOnClientRequestExecute("freecam", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (!client.UsingFreeCam)
				{
					GameMain.Server.SendConsoleMessage(client.Name + ": Entering freecam mode", client, new Color?(Color.Yellow));
					Character currentCharacter = client.Character;
					client.PreviousCharacter = new WeakReference<Character>(currentCharacter);
					client.UsingFreeCam = true;
					client.SpectateOnly = true;
					GameMain.Server.SetClientCharacter(client, null);
					return;
				}
				Character prevCharacter = null;
				if (client.PreviousCharacter != null && client.PreviousCharacter.TryGetTarget(out prevCharacter) && prevCharacter != null && !prevCharacter.IsDead && !prevCharacter.Removed)
				{
					GameMain.Server.SendConsoleMessage(client.Name + ": Exiting freecam mode", client, new Color?(Color.Yellow));
					client.UsingFreeCam = false;
					GameMain.Server.SetClientCharacter(client, prevCharacter);
					client.SpectateOnly = false;
					return;
				}
				GameMain.Server.SendConsoleMessage(client.Name + ": Could not regain control of the previous character (dead or removed).", client, new Color?(Color.Red));
			});
			DebugConsole.AssignOnClientRequestExecute("difficulty|leveldifficulty", delegate(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (GameMain.Server == null || args.Length < 1)
				{
					return;
				}
				float difficulty;
				if (float.TryParse(args[0], out difficulty))
				{
					GameMain.Server.SendConsoleMessage("Set level difficulty setting to " + MathHelper.Clamp(difficulty, 0f, 100f).ToString(), client, null);
					DebugConsole.NewMessage("Client \"" + client.Name + "\" set level difficulty setting to " + MathHelper.Clamp(difficulty, 0f, 100f).ToString(), new Color?(Color.White), false);
					GameMain.NetLobbyScreen.SetLevelDifficulty(difficulty);
					return;
				}
				GameMain.Server.SendConsoleMessage(args[0] + " is not a valid difficulty setting (enter a value between 0-100)", client, new Color?(Color.Red));
				DebugConsole.NewMessage(args[0] + " is not a valid difficulty setting (enter a value between 0-100)", new Color?(Color.Red), false);
			});
			DebugConsole.AssignOnClientRequestExecute("giveperm", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				string perm = string.Join("", args.Skip(1));
				ClientPermissions permission = ClientPermissions.None;
				if (!Enum.TryParse<ClientPermissions>(perm, true, out permission))
				{
					GameMain.Server.SendConsoleMessage(perm + " is not a valid permission!", senderClient, new Color?(Color.Red));
					return;
				}
				if (permission == ClientPermissions.None)
				{
					GameServer server = GameMain.Server;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 2);
					defaultInterpolatedStringHandler.AppendLiteral("No permissions were given to ");
					defaultInterpolatedStringHandler.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler.AppendLiteral(". Did you mean \"revokeperm ");
					defaultInterpolatedStringHandler.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" All\"?");
					server.SendConsoleMessage(defaultInterpolatedStringHandler.ToStringAndClear(), senderClient, null);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(62, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("No permissions were given to ");
					defaultInterpolatedStringHandler2.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(". Did you mean \"revokeperm ");
					defaultInterpolatedStringHandler2.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" All\"?");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
					return;
				}
				client.GivePermission(permission);
				GameMain.Server.UpdateClientPermissions(client);
				GameMain.Server.SendConsoleMessage(string.Concat(new string[]
				{
					"Granted ",
					perm,
					" permissions to ",
					client.Name,
					"."
				}), senderClient, null);
				DebugConsole.NewMessage(string.Concat(new string[]
				{
					senderClient.Name,
					" granted ",
					perm,
					" permissions to ",
					client.Name,
					"."
				}), new Color?(Color.White), false);
			});
			DebugConsole.AssignOnClientRequestExecute("revokeperm", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				if (client.Connection == GameMain.Server.OwnerConnection)
				{
					GameMain.Server.SendConsoleMessage("Cannot revoke permissions from the server owner!", senderClient, new Color?(Color.Red));
					return;
				}
				string perm = string.Join("", args.Skip(1));
				ClientPermissions permission = ClientPermissions.None;
				if (!Enum.TryParse<ClientPermissions>(perm, true, out permission))
				{
					GameMain.Server.SendConsoleMessage(perm + " is not a valid permission!", senderClient, new Color?(Color.Red));
					return;
				}
				client.RemovePermission(permission);
				GameMain.Server.UpdateClientPermissions(client);
				GameMain.Server.SendConsoleMessage(string.Concat(new string[]
				{
					"Revoked ",
					perm,
					" permissions from ",
					client.Name,
					"."
				}), senderClient, null);
				DebugConsole.NewMessage(string.Concat(new string[]
				{
					senderClient.Name,
					" revoked ",
					perm,
					" permissions from ",
					client.Name,
					"."
				}), new Color?(Color.White), false);
			});
			DebugConsole.AssignOnClientRequestExecute("giverank", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				if (client.Connection == GameMain.Server.OwnerConnection)
				{
					GameMain.Server.SendConsoleMessage("Cannot modify the rank of the server owner!", senderClient, new Color?(Color.Red));
					return;
				}
				string rank = string.Join("", args.Skip(1));
				PermissionPreset preset = PermissionPreset.List.Find((PermissionPreset p) => p.DisplayName.Equals(rank, StringComparison.OrdinalIgnoreCase));
				if (preset == null)
				{
					GameMain.Server.SendConsoleMessage("Rank \"" + rank + "\" not found.", senderClient, new Color?(Color.Red));
					return;
				}
				client.SetPermissions(preset.Permissions, preset.PermittedCommands);
				GameMain.Server.UpdateClientPermissions(client);
				GameServer server = GameMain.Server;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Assigned the rank \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(preset.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral("\" to ");
				defaultInterpolatedStringHandler.AppendFormatted(client.Name);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				server.SendConsoleMessage(defaultInterpolatedStringHandler.ToStringAndClear(), senderClient, null);
				DebugConsole.NewMessage(senderClient.Name + " granted  the rank \"" + preset.DisplayName + "\" to " + client.Name + ".", new Color?(Color.White), false);
			});
			DebugConsole.AssignOnClientRequestExecute("givecommandperm", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					GameMain.Server.SendConsoleMessage("Client \"" + args[0] + "\" not found.", senderClient, new Color?(Color.Red));
					return;
				}
				if (client.Connection == GameMain.Server.OwnerConnection)
				{
					GameMain.Server.SendConsoleMessage("Cannot modify the command permissions of the server owner!", senderClient, new Color?(Color.Red));
					return;
				}
				List<DebugConsole.Command> grantedCommands = new List<DebugConsole.Command>();
				Identifier[] splitCommands = (from s in args.Skip(1)
				select s.Trim()).ToIdentifiers().ToArray<Identifier>();
				bool giveAll = splitCommands.Length != 0 && splitCommands[0] == "all";
				if (giveAll)
				{
					grantedCommands.AddRange(DebugConsole.commands);
				}
				else
				{
					Identifier[] array = splitCommands;
					for (int i = 0; i < array.Length; i++)
					{
						Identifier command = array[i];
						DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(command));
						if (matchingCommand == null)
						{
							GameMain.Server.SendConsoleMessage("Could not find the command \"" + command.ToString() + "\"!", senderClient, new Color?(Color.Red));
						}
						else
						{
							grantedCommands.Add(matchingCommand);
						}
					}
				}
				client.GivePermission(ClientPermissions.ConsoleCommands);
				client.SetPermissions(client.Permissions, client.PermittedConsoleCommands.Union(grantedCommands).Distinct<DebugConsole.Command>().ToList<DebugConsole.Command>());
				GameMain.Server.UpdateClientPermissions(client);
				if (giveAll)
				{
					GameMain.Server.SendConsoleMessage("Gave the client \"" + client.Name + "\" the permission to use all console commands.", senderClient, null);
					return;
				}
				if (grantedCommands.Count > 0)
				{
					GameServer server = GameMain.Server;
					string[] array2 = new string[5];
					array2[0] = "Gave the client \"";
					array2[1] = client.Name;
					array2[2] = "\" the permission to use console commands ";
					array2[3] = string.Join<Identifier>(", ", from c in grantedCommands
					select c.Names[0]);
					array2[4] = ".";
					server.SendConsoleMessage(string.Concat(array2), senderClient, null);
				}
			});
			DebugConsole.AssignOnClientRequestExecute("revokecommandperm", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				if (client.Connection == GameMain.Server.OwnerConnection)
				{
					GameMain.Server.SendConsoleMessage("Cannot revoke command permissions from the server owner!", senderClient, new Color?(Color.Red));
					return;
				}
				List<DebugConsole.Command> revokedCommands = new List<DebugConsole.Command>();
				Identifier[] splitCommands = (from s in args.Skip(1)
				select s.Trim()).ToIdentifiers().ToArray<Identifier>();
				bool revokeAll = splitCommands.Length != 0 && splitCommands[0] == "all";
				if (revokeAll)
				{
					revokedCommands.AddRange(DebugConsole.commands);
				}
				else
				{
					Identifier[] array = splitCommands;
					for (int i = 0; i < array.Length; i++)
					{
						Identifier command = array[i];
						DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(command));
						if (matchingCommand == null)
						{
							GameMain.Server.SendConsoleMessage("Could not find the command \"" + command.ToString() + "\"!", senderClient, new Color?(Color.Red));
						}
						else
						{
							revokedCommands.Add(matchingCommand);
						}
					}
				}
				client.SetPermissions(client.Permissions, client.PermittedConsoleCommands.Except(revokedCommands).ToList<DebugConsole.Command>());
				if (client.PermittedConsoleCommands.Count == 0)
				{
					client.RemovePermission(ClientPermissions.ConsoleCommands);
				}
				GameMain.Server.UpdateClientPermissions(client);
				GameServer server = GameMain.Server;
				string[] array2 = new string[5];
				array2[0] = "Revoked \"";
				array2[1] = client.Name;
				array2[2] = "\"'s permission to use the console commands ";
				array2[3] = string.Join<Identifier>(", ", from c in revokedCommands
				select c.Names[0]);
				array2[4] = ".";
				server.SendConsoleMessage(string.Concat(array2), senderClient, null);
				if (revokeAll)
				{
					GameMain.Server.SendConsoleMessage("Revoked \"" + client.Name + "\"'s permission to use console commands.", senderClient, null);
					return;
				}
				if (revokedCommands.Count > 0)
				{
					GameServer server2 = GameMain.Server;
					string[] array3 = new string[5];
					array3[0] = "Revoked \"";
					array3[1] = client.Name;
					array3[2] = "\"'s permission to use the console commands ";
					array3[3] = string.Join<Identifier>(", ", from c in revokedCommands
					select c.Names[0]);
					array3[4] = ".";
					server2.SendConsoleMessage(string.Concat(array3), senderClient, null);
				}
			});
			DebugConsole.AssignOnClientRequestExecute("showperm", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 1)
				{
					GameMain.Server.SendConsoleMessage("showperm [id]: Shows the current administrative permissions of the client with the specified client ID.", senderClient, null);
					return;
				}
				Client client = DebugConsole.FindClient(args[0]);
				if (client == null)
				{
					DebugConsole.ThrowError("Client \"" + args[0] + "\" not found.", null, null, false, false);
					return;
				}
				if (client.Permissions == ClientPermissions.None)
				{
					GameMain.Server.SendConsoleMessage(client.Name + " has no special permissions.", senderClient, null);
					return;
				}
				GameMain.Server.SendConsoleMessage(client.Name + " has the following permissions:", senderClient, null);
				foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
				{
					ClientPermissions permission = (ClientPermissions)obj;
					if (permission != ClientPermissions.None && client.HasPermission(permission))
					{
						GameServer server = GameMain.Server;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 1);
						defaultInterpolatedStringHandler.AppendLiteral("   - ");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("ClientPermission." + permission.ToString()));
						server.SendConsoleMessage(defaultInterpolatedStringHandler.ToStringAndClear(), senderClient, null);
					}
				}
				if (client.HasPermission(ClientPermissions.ConsoleCommands))
				{
					if (client.PermittedConsoleCommands.Count == 0)
					{
						GameMain.Server.SendConsoleMessage("No permitted console commands:", senderClient, null);
						return;
					}
					GameMain.Server.SendConsoleMessage("Permitted console commands:", senderClient, null);
					foreach (DebugConsole.Command permittedCommand in client.PermittedConsoleCommands)
					{
						GameMain.Server.SendConsoleMessage("   - " + permittedCommand.Names[0].ToString(), senderClient, null);
					}
				}
			});
			DebugConsole.AssignOnClientRequestExecute("setclientcharacter", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					GameMain.Server.SendConsoleMessage("Invalid parameters. The command should be formatted as \"setclientcharacter [client] [character]\". If the names consist of multiple words, you should surround them with quotation marks.", senderClient, new Color?(Color.Red));
					DebugConsole.ThrowError("Invalid parameters. The command should be formatted as \"setclientcharacter [client] [character]\". If the names consist of multiple words, you should surround them with quotation marks.", null, null, false, false);
					return;
				}
				Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Name == args[0]);
				if (client == null)
				{
					GameMain.Server.SendConsoleMessage("Client \"" + args[0] + "\" not found.", senderClient, new Color?(Color.Red));
					return;
				}
				Character character = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false);
				GameMain.Server.SetClientCharacter(client, character);
				client.SpectateOnly = false;
			});
			DebugConsole.AssignOnClientRequestExecute("money", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
				if (campaign == null)
				{
					GameMain.Server.SendConsoleMessage("No campaign active!", senderClient, new Color?(Color.Red));
					return;
				}
				Character targetCharacter = null;
				if (args.Length >= 2)
				{
					targetCharacter = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false);
				}
				int money;
				if (int.TryParse(args[0], out money))
				{
					Wallet wallet = (targetCharacter == null) ? campaign.Bank : targetCharacter.Wallet;
					wallet.Give(money);
					GameAnalyticsManager.AddMoneyGainedEvent(money, GameAnalyticsManager.MoneySource.Cheat, "console");
					return;
				}
				GameMain.Server.SendConsoleMessage("\"" + args[0] + "\" is not a valid numeric value.", senderClient, new Color?(Color.Red));
			});
			DebugConsole.AssignOnClientRequestExecute("showmoney", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
				if (campaign == null)
				{
					GameMain.Server.SendConsoleMessage("No campaign active!", senderClient, new Color?(Color.Red));
					return;
				}
				StringBuilder sb = new StringBuilder();
				StringBuilder stringBuilder = sb;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral("Bank: ");
				appendInterpolatedStringHandler.AppendFormatted<int>(campaign.Bank.Balance);
				stringBuilder2.Append(ref appendInterpolatedStringHandler);
				foreach (Client client in GameMain.Server.ConnectedClients)
				{
					if (client.Character != null)
					{
						sb.Append(Environment.NewLine);
						stringBuilder = sb;
						StringBuilder stringBuilder3 = stringBuilder;
						appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder);
						appendInterpolatedStringHandler.AppendFormatted(client.Name);
						appendInterpolatedStringHandler.AppendLiteral(": ");
						appendInterpolatedStringHandler.AppendFormatted<int>(client.Character.Wallet.Balance);
						stringBuilder3.Append(ref appendInterpolatedStringHandler);
					}
				}
				GameMain.Server.SendConsoleMessage(sb.ToString(), senderClient, null);
			});
			DebugConsole.AssignOnClientRequestExecute("campaigndestination|setcampaigndestination", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					GameMain.Server.SendConsoleMessage("No campaign active!", senderClient, new Color?(Color.Red));
					return;
				}
				int destinationIndex = -1;
				if (args.Length < 1 || !int.TryParse(args[0], out destinationIndex))
				{
					return;
				}
				if (destinationIndex < 0 || destinationIndex >= campaign.Map.CurrentLocation.Connections.Count)
				{
					GameMain.Server.SendConsoleMessage("Index out of bounds!", senderClient, new Color?(Color.Red));
					return;
				}
				Location location = campaign.Map.CurrentLocation.Connections[destinationIndex].OtherLocation(campaign.Map.CurrentLocation);
				campaign.Map.SelectLocation(location);
				GameMain.Server.SendConsoleMessage(location.DisplayName.Value + " selected.", senderClient, null);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("tags|taglist", "tags: list all the tags used in the game", delegate(string[] args)
			{
				IEnumerable<Identifier> tagList = MapEntityPrefab.List.SelectMany((MapEntityPrefab p) => from t in p.Tags
				select t).Distinct<Identifier>();
				foreach (Identifier tag in tagList)
				{
					DebugConsole.NewMessage(tag.Value, new Color?(Color.Yellow), false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("sendchatmessage", "sendchatmessage [sendername] [message] [type] [r] [g] [b] [a]: Sends a chat message with specified type and color.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					return;
				}
				ChatMessageType chatMessageType = ChatMessageType.Default;
				Color? chatMessageColor = null;
				int result;
				if (args.Length >= 3 && int.TryParse(args[2], out result))
				{
					chatMessageType = (ChatMessageType)result;
				}
				int r;
				int g;
				int b;
				int a;
				if (args.Length >= 7 && int.TryParse(args[3], out r) && int.TryParse(args[4], out g) && int.TryParse(args[5], out b) && int.TryParse(args[6], out a))
				{
					chatMessageColor = new Color?(new Color(r, g, b, a));
				}
				foreach (Client client in GameMain.Server.ConnectedClients)
				{
					GameMain.Server.SendDirectChatMessage(ChatMessage.Create(args[0], args[1], chatMessageType, null, null, PlayerConnectionChangeType.None, chatMessageColor), client);
				}
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("setskill", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					GameServer server = GameMain.Server;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Missing arguments. Expected at least 2 but got ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(args.Length);
					defaultInterpolatedStringHandler.AppendLiteral(" (skill, level, name)");
					server.SendConsoleMessage(defaultInterpolatedStringHandler.ToStringAndClear(), senderClient, new Color?(Color.Red));
					return;
				}
				Identifier skillIdentifier = args[0].ToIdentifier();
				string levelString = args[1];
				Character character = (args.Length >= 3) ? DebugConsole.FindMatchingCharacter(args.Skip(2).ToArray<string>(), false, null, false) : senderClient.Character;
				bool flag;
				if (character == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = character.Info;
					flag = (((info != null) ? info.Job : null) != null);
				}
				if (!flag)
				{
					GameMain.Server.SendConsoleMessage("Character is not valid.", senderClient, new Color?(Color.Red));
					return;
				}
				bool isMax = levelString.Equals("max", StringComparison.OrdinalIgnoreCase);
				float level;
				if (!float.TryParse(levelString, NumberStyles.Number, CultureInfo.InvariantCulture, out level) && !isMax)
				{
					GameMain.Server.SendConsoleMessage(levelString + " is not a valid level. Expected number or \"max\".", senderClient, new Color?(Color.Red));
					return;
				}
				if (isMax)
				{
					level = 100f;
				}
				if (skillIdentifier == "all")
				{
					foreach (Skill skill in character.Info.Job.GetSkills())
					{
						GameMain.NetworkMember.CreateEntityEvent(character, new Character.UpdateSkillsEventData(skill.Identifier, true));
						character.Info.SetSkillLevel(skill.Identifier, level, false);
					}
					GameServer server2 = GameMain.Server;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Set all ");
					defaultInterpolatedStringHandler2.AppendFormatted(character.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("'s skills to ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(level);
					server2.SendConsoleMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), senderClient, null);
					return;
				}
				character.Info.SetSkillLevel(skillIdentifier, level, false);
				GameMain.NetworkMember.CreateEntityEvent(character, new Character.UpdateSkillsEventData(skillIdentifier, true));
				GameServer server3 = GameMain.Server;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(17, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("Set ");
				defaultInterpolatedStringHandler3.AppendFormatted(character.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("'s ");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(skillIdentifier);
				defaultInterpolatedStringHandler3.AppendLiteral(" level to ");
				defaultInterpolatedStringHandler3.AppendFormatted<float>(level);
				server3.SendConsoleMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), senderClient, null);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("setsalary", "setsalary [0-100] [character/default]: Sets the salary of a certain character or the default salary to a percentage.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Missing arguments. Expected at least 2 but got ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(args.Length);
					defaultInterpolatedStringHandler.AppendLiteral(" (amount, character)");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
				if (mpCampaign == null)
				{
					DebugConsole.NewMessage("No campaign active.", new Color?(Color.Red), false);
					return;
				}
				int amount;
				if (!int.TryParse(args[0], out amount))
				{
					DebugConsole.NewMessage(args[0] + " is not a valid amount.", new Color?(Color.Red), false);
					return;
				}
				if (args[1].Equals("default", StringComparison.OrdinalIgnoreCase))
				{
					mpCampaign.Bank.SetRewardDistribution(amount);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Set the default salary to ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(amount);
					defaultInterpolatedStringHandler2.AppendLiteral("%");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.White), false);
					return;
				}
				Character character = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false);
				if (character == null)
				{
					DebugConsole.NewMessage("Character not found \"" + args[1] + "\".", new Color?(Color.Red), false);
					return;
				}
				character.Wallet.SetRewardDistribution(amount);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(18, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Set ");
				defaultInterpolatedStringHandler3.AppendFormatted(character.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("'s salary to ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(amount);
				defaultInterpolatedStringHandler3.AppendLiteral("%");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Color?(Color.White), false);
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("setsalary", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (args.Length < 2)
				{
					GameServer server = GameMain.Server;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Missing arguments. Expected at least 2 but got ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(args.Length);
					defaultInterpolatedStringHandler.AppendLiteral(" (amount, character)");
					server.SendConsoleMessage(defaultInterpolatedStringHandler.ToStringAndClear(), senderClient, new Color?(Color.Red));
					return;
				}
				if (!CampaignMode.AllowedToManageWallets(senderClient))
				{
					GameMain.Server.SendConsoleMessage("You are not allowed to manage wallets.", senderClient, new Color?(Color.Red));
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
				if (mpCampaign == null)
				{
					GameMain.Server.SendConsoleMessage("No campaign active.", senderClient, new Color?(Color.Red));
					return;
				}
				int amount;
				if (!int.TryParse(args[0], out amount))
				{
					GameMain.Server.SendConsoleMessage(args[0] + " is not a valid amount.", senderClient, new Color?(Color.Red));
					return;
				}
				if (args[1].Equals("default", StringComparison.OrdinalIgnoreCase))
				{
					mpCampaign.Bank.SetRewardDistribution(amount);
					GameServer server2 = GameMain.Server;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Set the default salary to ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(amount);
					defaultInterpolatedStringHandler2.AppendLiteral("%");
					server2.SendConsoleMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), senderClient, null);
					return;
				}
				Character character = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false);
				if (character == null)
				{
					GameMain.Server.SendConsoleMessage("Character not found \"" + args[1] + "\".", senderClient, new Color?(Color.Red));
					return;
				}
				character.Wallet.SetRewardDistribution(amount);
				GameServer server3 = GameMain.Server;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Set ");
				defaultInterpolatedStringHandler3.AppendFormatted(character.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("'s salary to ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(amount);
				defaultInterpolatedStringHandler3.AppendLiteral("%.");
				server3.SendConsoleMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), senderClient, null);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("readycheck", "Commence a ready check.", delegate(string[] args)
			{
				if (Screen.Selected != GameMain.GameScreen || GameMain.NetworkMember == null)
				{
					DebugConsole.NewMessage("Ready checks cannot be commenced in the lobby.", new Color?(Color.Red), false);
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				CrewManager crewManager = (gameSession != null) ? gameSession.CrewManager : null;
				if (crewManager != null && crewManager.ActiveReadyCheck == null)
				{
					ReadyCheck.StartReadyCheck("", null);
					DebugConsole.NewMessage("Attempted to commence a ready check.", new Color?(Color.Green), false);
					return;
				}
				DebugConsole.NewMessage("A ready check is already running.", new Color?(Color.Red), false);
			}, null, false));
			DebugConsole.AssignOnClientRequestExecute("readycheck", delegate(Client senderClient, Vector2 cursorWorldPos, string[] args)
			{
				if (Screen.Selected == GameMain.GameScreen && GameMain.NetworkMember != null)
				{
					GameSession gameSession = GameMain.GameSession;
					bool? flag;
					if (gameSession == null)
					{
						flag = null;
					}
					else
					{
						GameMode gameMode = gameSession.GameMode;
						flag = ((gameMode != null) ? new bool?(gameMode.IsSinglePlayer) : null);
					}
					bool? flag2 = flag;
					if (!flag2.GetValueOrDefault(true))
					{
						GameSession gameSession2 = GameMain.GameSession;
						CrewManager crewManager = (gameSession2 != null) ? gameSession2.CrewManager : null;
						if (crewManager != null && crewManager.ActiveReadyCheck == null)
						{
							ReadyCheck.StartReadyCheck(senderClient.Name, senderClient);
							GameMain.Server.SendConsoleMessage("Attempted to commence a ready check.", senderClient, null);
							return;
						}
						GameMain.Server.SendConsoleMessage("A ready check is already running.", senderClient, null);
						return;
					}
				}
				GameMain.Server.SendConsoleMessage("Ready checks cannot be commenced in the lobby.", senderClient, null);
			});
		}

		// Token: 0x06000335 RID: 821 RVA: 0x0001A608 File Offset: 0x00018808
		public static void ServerRead(IReadMessage inc, Client sender)
		{
			string consoleCommand = inc.ReadString();
			float cursorX = inc.ReadSingle();
			float cursorY = inc.ReadSingle();
			if (DebugConsole.rateLimiter.IsLimitReached(sender))
			{
				return;
			}
			DebugConsole.ExecuteClientCommand(sender, new Vector2(cursorX, cursorY), consoleCommand);
		}

		// Token: 0x06000336 RID: 822 RVA: 0x0001A648 File Offset: 0x00018848
		public static void ExecuteClientCommand(Client client, Vector2 cursorWorldPos, string command)
		{
			if (GameMain.Server == null)
			{
				return;
			}
			if (string.IsNullOrWhiteSpace(command))
			{
				return;
			}
			if (!client.HasPermission(ClientPermissions.ConsoleCommands) && client.Connection != GameMain.Server.OwnerConnection)
			{
				GameMain.Server.SendConsoleMessage("You are not permitted to use console commands!", client, new Color?(Color.Red));
				GameServer.Log(NetworkMember.ClientLogName(client, null) + " attempted to execute the console command \"" + command + "\" without a permission to use console commands.", ServerLog.MessageType.ConsoleUsage);
				return;
			}
			string[] splitCommand = ToolBox.SplitCommand(command);
			DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(splitCommand[0].ToIdentifier()));
			if (matchingCommand != null && !client.PermittedConsoleCommands.Contains(matchingCommand) && client.Connection != GameMain.Server.OwnerConnection)
			{
				GameMain.Server.SendConsoleMessage("You are not permitted to use the command\"" + matchingCommand.Names[0].ToString() + "\"!", client, new Color?(Color.Red));
				GameServer.Log(NetworkMember.ClientLogName(client, null) + " attempted to execute the console command \"" + command + "\" without a permission to use the command.", ServerLog.MessageType.ConsoleUsage);
				return;
			}
			if (matchingCommand == null)
			{
				GameMain.Server.SendConsoleMessage("Command \"" + splitCommand[0] + "\" not found.", client, new Color?(Color.Red));
				return;
			}
			if (!MathUtils.IsValid(cursorWorldPos))
			{
				GameMain.Server.SendConsoleMessage("Could not execute command \"" + command + "\" - invalid cursor position.", client, new Color?(Color.Red));
				DebugConsole.NewMessage(NetworkMember.ClientLogName(client, null) + " attempted to execute the console command \"" + command + "\" with invalid cursor position.", new Color?(Color.White), false);
				return;
			}
			try
			{
				matchingCommand.ServerExecuteOnClientRequest(client, cursorWorldPos, splitCommand.Skip(1).ToArray<string>());
				GameServer.Log(string.Concat(new string[]
				{
					"Console command \"",
					command,
					"\" executed by ",
					NetworkMember.ClientLogName(client, null),
					"."
				}), ServerLog.MessageType.ConsoleUsage);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Executing the command \"",
					matchingCommand.Names[0].ToString(),
					"\" by request from \"",
					NetworkMember.ClientLogName(client, null),
					"\" failed."
				}), e, null, false, false);
			}
		}

		// Token: 0x06000337 RID: 823 RVA: 0x0001A8A4 File Offset: 0x00018AA4
		private static void ShowHelpMessage(DebugConsole.Command command)
		{
			DebugConsole.NewMessage(command.Names[0].Value, new Color?(Color.Cyan), false);
			DebugConsole.NewMessage(command.Help, new Color?(Color.Gray), false);
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000338 RID: 824 RVA: 0x0001A8EB File Offset: 0x00018AEB
		public static List<DebugConsole.Command> Commands
		{
			get
			{
				return DebugConsole.commands;
			}
		}

		// Token: 0x06000339 RID: 825 RVA: 0x0001A8F4 File Offset: 0x00018AF4
		public static void AssignOnExecute(string names, Action<string[]> onExecute)
		{
			DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Intersect(names.Split('|', StringSplitOptions.None).ToIdentifiers()).Any<Identifier>());
			if (matchingCommand == null)
			{
				throw new Exception("AssignOnExecute failed. Command matching the name(s) \"" + names + "\" not found.");
			}
			matchingCommand.OnExecute = onExecute;
		}

		// Token: 0x0600033A RID: 826 RVA: 0x0001A94C File Offset: 0x00018B4C
		unsafe static DebugConsole()
		{
			DebugConsole.commands.Add(new DebugConsole.Command("help", "", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					using (List<DebugConsole.Command>.Enumerator enumerator = DebugConsole.commands.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							DebugConsole.Command c2 = enumerator.Current;
							if (!string.IsNullOrEmpty(c2.Help))
							{
								DebugConsole.ShowHelpMessage(c2);
							}
						}
						return;
					}
				}
				Func<Identifier, bool> <>9__123;
				DebugConsole.Command matchingCommand = DebugConsole.commands.Find(delegate(DebugConsole.Command c)
				{
					ImmutableArray<Identifier> names = c.Names;
					Func<Identifier, bool> predicate;
					if ((predicate = <>9__123) == null)
					{
						predicate = (<>9__123 = ((Identifier name) => name == args[0]));
					}
					return names.Any(predicate);
				});
				if (matchingCommand == null)
				{
					DebugConsole.NewMessage("Command " + args[0] + " not found.", new Color?(Color.Red), false);
					return;
				}
				DebugConsole.ShowHelpMessage(matchingCommand);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = (from n in DebugConsole.commands.SelectMany((DebugConsole.Command c) => c.Names)
				select n.Value).ToArray<string>();
				array[1] = Array.Empty<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("items|itemlist", "itemlist: List all the item prefabs available for spawning.", delegate(string[] args)
			{
				DebugConsole.<.cctor>g__printMapEntityPrefabs|35_2<ItemPrefab>(ItemPrefab.Prefabs);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("itemassemblies", "itemassemblies: List all the item assemblies available for spawning.", delegate(string[] args)
			{
				DebugConsole.<.cctor>g__printMapEntityPrefabs|35_2<ItemAssemblyPrefab>(ItemAssemblyPrefab.Prefabs);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("netstats", "netstats: Toggles the visibility of the network statistics UI.", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null)
				{
					return;
				}
				GameMain.NetworkMember.ShowNetStats = !GameMain.NetworkMember.ShowNetStats;
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("spawn|spawncharacter", "spawn [creaturename/jobname] [near/inside/outside/cursor] [team] [add to crew (true/false)] [name]: Spawn a creature at a random spawnpoint (use the second parameter to only select spawnpoints near/inside/outside the submarine). You can also enter the name of a job (e.g. \"Mechanic\") to spawn a character with a specific job and the appropriate equipment.", null, delegate()
			{
				string[] creatureAndJobNames = (from s in (from p in CharacterPrefab.Prefabs
				select p.Identifier.Value).Concat(from p in JobPrefab.Prefabs
				select p.Identifier.Value)
				orderby s
				select s).ToArray<string>();
				string[][] array = new string[4][];
				array[0] = creatureAndJobNames.ToArray<string>();
				array[1] = new string[]
				{
					"near",
					"inside",
					"outside",
					"cursor"
				};
				array[2] = (from v in Enum.GetValues<CharacterTeamType>()
				select v.ToString()).ToArray<string>();
				array[3] = new string[]
				{
					"true",
					"false"
				};
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("give|giveitem", "give|giveitem [itemname/itemidentifier] [amount] [condition] [quality]: Spawn an item in the inventory of the controlled character", delegate(string[] args)
			{
				if (Character.Controlled == null)
				{
					DebugConsole.ThrowError("No character is selected!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please give the name or identifier of the item to spawn.", null, null, false, false);
					return;
				}
				List<string> modifiedArgs = new List<string>(args);
				modifiedArgs.Insert(1, "inventory");
				DebugConsole.TrySpawnItem(modifiedArgs.ToArray());
			}, () => new string[][]
			{
				DebugConsole.GetItemNameOrIdParams().ToArray<string>(),
				new string[]
				{
					"1"
				},
				new string[]
				{
					"100"
				},
				DebugConsole.ItemQualityNames.ToArray<string>()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("spawnnpc", "spawnnpc [any/npcsetidentifier] [npcidentifier] [near/inside/outside/cursor] [team (0-3)] [add to crew (true/false)]: Spawns an pre-configured NPC at a random spawnpoint. (Use the third parameter to select a specific set of spawnpoints.)", null, delegate()
			{
				string[][] array = new string[5][];
				array[0] = "any".ToEnumerable<string>().Union(from p in NPCSet.Sets
				select p.Identifier.Value into s
				orderby s
				select s).ToArray<string>();
				array[1] = (from p in NPCSet.Sets.SelectMany((NPCSet set) => set.Humans)
				select p.Identifier.Value into s
				orderby s
				select s).ToArray<string>();
				array[2] = new string[]
				{
					"near",
					"inside",
					"outside",
					"cursor"
				};
				array[3] = (from v in Enum.GetValues<CharacterTeamType>()
				select v.ToString()).ToArray<string>();
				array[4] = new string[]
				{
					"true",
					"false"
				};
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("spawnitem", "spawnitem [itemname/itemidentifier] [cursor/inventory/cargo/random/[name]] [amount] [condition] [quality]: Spawn an item at the position of the cursor, in the inventory of the controlled character, in the inventory of the client with the given name, or at a random spawnpoint if the location parameter is omitted or \"random\".", delegate(string[] args)
			{
				DebugConsole.TrySpawnItem(args);
			}, () => new string[][]
			{
				DebugConsole.GetItemNameOrIdParams().ToArray<string>(),
				DebugConsole.GetSpawnPosParams().ToArray<string>(),
				new string[]
				{
					"1"
				},
				new string[]
				{
					"100"
				},
				DebugConsole.ItemQualityNames.ToArray<string>()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("disablecrewai", "disablecrewai: Disable the AI of the NPCs in the crew.", delegate(string[] args)
			{
				HumanAIController.DisableCrewAI = true;
				DebugConsole.NewMessage("Crew AI disabled", new Color?(Color.Red), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("enablecrewai", "enablecrewai: Enable the AI of the NPCs in the crew.", delegate(string[] args)
			{
				HumanAIController.DisableCrewAI = false;
				DebugConsole.NewMessage("Crew AI enabled", new Color?(Color.Green), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("disableenemyai", "disableenemyai: Disable the AI of the Enemy characters (monsters).", delegate(string[] args)
			{
				EnemyAIController.DisableEnemyAI = true;
				DebugConsole.NewMessage("Enemy AI disabled", new Color?(Color.Red), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("enableenemyai", "enableenemyai: Enable the AI of the Enemy characters (monsters).", delegate(string[] args)
			{
				EnemyAIController.DisableEnemyAI = false;
				DebugConsole.NewMessage("Enemy AI enabled", new Color?(Color.Green), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("triggertraitorevent|starttraitoreventimmediately", "triggertraitorevent [eventidentifier]: Skip the initial delay of the traitor events and start one immediately. You can optionally specify which event to start (otherwise a random event is chosen).", null, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from p in EventPrefab.Prefabs
				where p is TraitorEventPrefab
				select p.Identifier.ToString()).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("botcount", "botcount [x]: Set the number of bots in the crew in multiplayer.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("botspawnmode", "botspawnmode [fill/normal]: Set how bots are spawned in the multiplayer.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("killdisconnectedtimer", "killdisconnectedtimer [seconds]: Set the time after which disconnect players' characters get automatically killed.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autorestart", "autorestart [true/false]: Enable or disable round auto-restart.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autorestartinterval", "autorestartinterval [seconds]: Set how long the server waits between rounds before automatically starting a new one. If set to 0, autorestart is disabled.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autorestarttimer", "autorestarttimer [seconds]: Set the current autorestart countdown to the specified value.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("startwhenclientsready", "startwhenclientsready [true/false]: Enable or disable automatically starting the round when clients are ready to start.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("giveperm", "giveperm [id/steamid/endpoint/name]: Grants administrative permissions to the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from ClientPermissions v in Enum.GetValues(typeof(ClientPermissions))
				select v.ToString()).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("revokeperm", "revokeperm [id/steamid/endpoint/name]: Revokes administrative permissions from the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from ClientPermissions v in Enum.GetValues(typeof(ClientPermissions))
				select v.ToString()).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("giverank", "giverank [id/steamid/endpoint/name]: Assigns a specific rank (= a set of administrative permissions) to the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from pp in PermissionPreset.List
				select pp.DisplayName.Value).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("givecommandperm", "givecommandperm [id/steamid/endpoint/name]: Gives the specified client the permission to use the specified console commands.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from c in DebugConsole.commands
				select c.Names.First<Identifier>().Value).Union(new string[]
				{
					"All"
				}).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("revokecommandperm", "revokecommandperm [id/steamid/endpoint/name]: Revokes permission to use the specified console commands from the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from c in DebugConsole.commands
				select c.Names.First<Identifier>().Value).Union(new string[]
				{
					"All"
				}).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("showperm", "showperm [id/steamid/endpoint/name]: Shows the current administrative permissions of the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("respawnnow", "respawnnow: Trigger a respawn immediately if there are any clients waiting to respawn.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("showkarma", "showkarma: Show the current karma values of the players.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglekarma", "togglekarma: Toggle the karma system on/off.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("resetkarma", "resetkarma [client]: Resets the karma value of the specified client to 100.", null, delegate()
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ConnectedClients : null) == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("setkarma", "setkarma [client] [0-100]: Sets the karma of the specified client to the specified value.", null, delegate()
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ConnectedClients : null) == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = new string[]
				{
					"50"
				};
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglekarmatestmode", "togglekarmatestmode: Toggle the karma test mode on/off. When test mode is enabled, clients get notified when their karma value changes (including the reason for the increase/decrease) and the server doesn't ban clients whose karma decreases below the ban threshold.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("kick", "kick [name]: Kick a player out of the server.", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null || args.Length == 0)
				{
					return;
				}
				string playerName = string.Join(" ", args);
				DebugConsole.ShowQuestionPrompt("Reason for kicking \"" + playerName + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					GameMain.NetworkMember.KickPlayer(playerName, reason);
				}, null, -1);
			}, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("kickid", "kickid [id]: Kick the player with the specified client ID out of the server.  You can see the IDs of the clients using the command \"clientlist\".", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null || args.Length == 0)
				{
					return;
				}
				int id;
				int.TryParse(args[0], out id);
				Client client = GameMain.NetworkMember.ConnectedClients.Find((Client c) => (int)c.SessionId == id);
				if (client == null)
				{
					DebugConsole.ThrowError("Client id \"" + id.ToString() + "\" not found.", null, null, false, false);
					return;
				}
				DebugConsole.ShowQuestionPrompt("Reason for kicking \"" + client.Name + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					GameMain.NetworkMember.KickPlayer(client.Name, reason);
				}, null, -1);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("ban", "ban [name]: Kick and ban the player from the server.", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null || args.Length == 0)
				{
					return;
				}
				string clientName = string.Join(" ", args);
				DebugConsole.ShowQuestionPrompt("Reason for banning \"" + clientName + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					DebugConsole.ShowQuestionPrompt("Enter the duration of the ban (leave empty to ban permanently, or use the format \"[days] d [hours] h\") (Enter c to cancel)", delegate(string duration)
					{
						if (duration == "c" || duration == "C")
						{
							return;
						}
						TimeSpan? banDuration = null;
						if (!string.IsNullOrWhiteSpace(duration))
						{
							TimeSpan parsedBanDuration;
							if (!DebugConsole.TryParseTimeSpan(duration, out parsedBanDuration))
							{
								DebugConsole.ThrowError("\"" + duration + "\" is not a valid ban duration. Use the format \"[days] d [hours] h\", \"[days] d\" or \"[hours] h\".", null, null, false, false);
								return;
							}
							banDuration = new TimeSpan?(parsedBanDuration);
						}
						GameMain.NetworkMember.BanPlayer(clientName, reason, banDuration);
					}, null, -1);
				}, null, -1);
			}, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("banid", "banid [id]: Kick and ban the player with the specified client ID from the server. You can see the IDs of the clients using the command \"clientlist\".", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null || args.Length == 0)
				{
					return;
				}
				int id;
				int.TryParse(args[0], out id);
				Client client = GameMain.NetworkMember.ConnectedClients.Find((Client c) => (int)c.SessionId == id);
				if (client == null)
				{
					DebugConsole.ThrowError("Client id \"" + id.ToString() + "\" not found.", null, null, false, false);
					return;
				}
				DebugConsole.ShowQuestionPrompt("Reason for banning \"" + client.Name + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					DebugConsole.ShowQuestionPrompt("Enter the duration of the ban (leave empty to ban permanently, or use the format \"[days] d [hours] h\") (c to cancel)", delegate(string duration)
					{
						if (duration == "c" || duration == "C")
						{
							return;
						}
						TimeSpan? banDuration = null;
						if (!string.IsNullOrWhiteSpace(duration))
						{
							TimeSpan parsedBanDuration;
							if (!DebugConsole.TryParseTimeSpan(duration, out parsedBanDuration))
							{
								DebugConsole.ThrowError("\"" + duration + "\" is not a valid ban duration. Use the format \"[days] d [hours] h\", \"[days] d\" or \"[hours] h\".", null, null, false, false);
								return;
							}
							banDuration = new TimeSpan?(parsedBanDuration);
						}
						GameMain.NetworkMember.BanPlayer(client.Name, reason, banDuration);
					}, null, -1);
				}, null, -1);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("banaddress|banip", "banaddress [endpoint]: Ban the IP address/SteamID from the server.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("teleportcharacter|teleport", "teleport [character name] [location]: Teleport the specified character to a location , or the position of the cursor if location is omitted. If the name parameter is omitted, the controlled character will be teleported.", null, () => new string[][]
			{
				DebugConsole.ListCharacterNames(Character.Controlled != null, true),
				DebugConsole.ListAvailableLocations()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("monstersignoreplayer", "Toggle if monsters should ignore the player character (and their equipment) when targeting.", delegate(string[] args)
			{
				DebugConsole.ToggleEnemyAITargetingRestrictions(EnemyTargetingRestrictions.PlayerCharacters);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("monstersignoresub", "Toggle if monsters should ignore the player submarines when targeting.", delegate(string[] args)
			{
				DebugConsole.ToggleEnemyAITargetingRestrictions(EnemyTargetingRestrictions.PlayerSubmarines);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("monstersrestoretargets", "Remove any targeting restrictions from monsters.", delegate(string[] args)
			{
				DebugConsole.ToggleEnemyAITargetingRestrictions(EnemyTargetingRestrictions.None);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("monstertargetingrestrictions", "monstertargetingrestrictions [restrictions]: Set targeting restrictions for all monsters. Supports multiple options comma-separated: 'monsterargetingrestrictions PlayerCharacters,PlayerSubmarines'. Use 'None' to remove all restrictions.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ToggleEnemyAITargetingRestrictions(EnemyAIController.TargetingRestrictions);
					return;
				}
				EnemyTargetingRestrictions restrictions;
				if (Enum.TryParse<EnemyTargetingRestrictions>(args[0], true, out restrictions))
				{
					DebugConsole.ToggleEnemyAITargetingRestrictions(restrictions);
					return;
				}
				DebugConsole.NewMessage("Failed to parse argument '" + args[0] + "'", new Color?(Color.Red), false);
			}, () => new string[][]
			{
				Enum.GetNames(typeof(EnemyTargetingRestrictions))
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("listlocations|locations", "listlocations: List all the locations in the level: subs, outposts, ruins, caves.", delegate(string[] args)
			{
				string[] availableLocations = DebugConsole.ListAvailableLocations();
				DebugConsole.NewMessage("***************", new Color?(Color.Cyan), false);
				foreach (string location in availableLocations)
				{
					DebugConsole.NewMessage(location, new Color?(Color.Cyan), false);
				}
				DebugConsole.NewMessage("***************", new Color?(Color.Cyan), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("godmode", "godmode [character name] [remove afflictions (true/false)]: Toggle character godmode. Makes the targeted character invulnerable to damage. If the name parameter is omitted, the controlled character will receive godmode.", delegate(string[] args)
			{
				DebugConsole.<>c__DisplayClass35_7 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass35_7();
				CS$<>8__locals1.args = args;
				CS$<>8__locals1.godmodeStateOnFirstCharacter = null;
				DebugConsole.HandleCommandForCrewOrSingleCharacter(CS$<>8__locals1.args, new Action<Character>(CS$<>8__locals1.<.cctor>g__ToggleGodMode|161), null);
			}, () => new string[][]
			{
				DebugConsole.ListCharacterNames(Character.Controlled != null, true)
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("godmode_mainsub", "godmode_mainsub: Toggle submarine godmode. Makes the main submarine invulnerable to damage.", delegate(string[] args)
			{
				if (Submarine.MainSub == null)
				{
					return;
				}
				Submarine.MainSub.GodMode = !Submarine.MainSub.GodMode;
				DebugConsole.NewMessage(Submarine.MainSub.GodMode ? "Godmode on" : "Godmode off", new Color?(Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("growthdelay", "growthdelay: Sets how long it takes for planters to attempt to advance a plant's growth.", delegate(string[] args)
			{
				float value;
				if (args.Length != 0 && float.TryParse(args[0], out value))
				{
					Planter.GrowthTickDelay = value;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Growth delay set to ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(value);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Green), false);
					return;
				}
				DebugConsole.NewMessage("Invalid value.", new Color?(Color.Red), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("lock", "lock: Lock movement of the main submarine.", delegate(string[] args)
			{
				Submarine.LockX = !Submarine.LockX;
				Submarine.LockY = Submarine.LockX;
				DebugConsole.NewMessage(Submarine.LockX ? "Submarine movement locked." : "Submarine movement unlocked.", new Color?(Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("lockx", "lockx: Lock horizontal movement of the main submarine.", delegate(string[] args)
			{
				Submarine.LockX = !Submarine.LockX;
				DebugConsole.NewMessage(Submarine.LockX ? "Horizontal submarine movement locked." : "Horizontal submarine movement unlocked.", new Color?(Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("locky", "locky: Lock vertical movement of the main submarine.", delegate(string[] args)
			{
				Submarine.LockY = !Submarine.LockY;
				DebugConsole.NewMessage(Submarine.LockY ? "Vertical submarine movement locked." : "Vertical submarine movement unlocked.", new Color?(Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("dumpids", "", delegate(string[] args)
			{
				try
				{
					int count = (args.Length == 0) ? 10 : int.Parse(args[0]);
					Entity.DumpIds(count, (args.Length >= 2) ? args[1] : null);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to dump ids", e, null, false, false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("dumptofile", "findentityids [filename]: Outputs the contents of the debug console into a text file in the game folder. If the filename argument is omitted, \"consoleOutput.txt\" is used as the filename.", delegate(string[] args)
			{
				string filename = "consoleOutput.txt";
				if (args.Length != 0)
				{
					filename = string.Join(" ", args);
				}
				File.WriteAllLines(filename, (from m in DebugConsole.Messages
				select m.Text).ToArray<string>(), null, true);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("findentityids", "findentityids [entityname]", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				foreach (MapEntity mapEntity in MapEntity.MapEntityList)
				{
					if (mapEntity.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase))
					{
						DebugConsole.ThrowError(mapEntity.ID.ToString() + ": " + mapEntity.Name.ToString(), null, null, false, false);
					}
				}
				foreach (Character character in Character.CharacterList)
				{
					if (!character.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase))
					{
						Identifier speciesName = character.SpeciesName;
						if (!(speciesName == args[0]))
						{
							continue;
						}
					}
					DebugConsole.ThrowError(character.ID.ToString() + ": " + character.Name.ToString(), null, null, false, false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("giveaffliction", "giveaffliction [affliction name] [affliction strength] [character name] [limb type] [use relative strength]: Add an affliction to a character. If the name parameter is omitted, the affliction is added to the controlled character.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					if (args.Length == 1)
					{
						DebugConsole.ThrowError("Must give a strength value!", null, null, false, false);
					}
					return;
				}
				string affliction = args[0];
				AfflictionPrefab afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab a) => a.Identifier == affliction);
				if (afflictionPrefab == null)
				{
					afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab a) => a.Name.Equals(affliction, StringComparison.OrdinalIgnoreCase));
				}
				if (afflictionPrefab == null)
				{
					DebugConsole.ThrowError("Affliction \"" + affliction + "\" not found.", null, null, false, false);
					return;
				}
				float afflictionStrength;
				if (!float.TryParse(args[1], out afflictionStrength))
				{
					DebugConsole.ThrowError("\"" + args[1] + "\" is not a valid affliction strength.", null, null, false, false);
					return;
				}
				bool relativeStrength = false;
				if (args.Length > 4)
				{
					bool.TryParse(args[4], out relativeStrength);
				}
				Character targetCharacter = (args.Length <= 2) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args.Skip(2).ToArray<string>(), false, null, false);
				if (targetCharacter != null)
				{
					Limb targetLimb = targetCharacter.AnimController.MainLimb;
					if (args.Length > 3)
					{
						targetLimb = targetCharacter.AnimController.Limbs.FirstOrDefault((Limb l) => l.type.ToString().Equals(args[3], StringComparison.OrdinalIgnoreCase));
					}
					if (relativeStrength)
					{
						afflictionStrength *= targetCharacter.MaxVitality / afflictionPrefab.MaxStrength;
					}
					targetCharacter.CharacterHealth.ApplyAffliction(targetLimb ?? targetCharacter.AnimController.MainLimb, afflictionPrefab.Instantiate(afflictionStrength, null), true, false, true);
				}
			}, delegate()
			{
				string[][] array = new string[4][];
				array[0] = (from a in AfflictionPrefab.Prefabs
				select a.Name.Value).ToArray<string>().Concat(from a in AfflictionPrefab.Prefabs
				select a.Identifier.Value).ToArray<string>();
				array[1] = new string[]
				{
					"1"
				};
				array[2] = DebugConsole.ListCharacterNames(false, false);
				array[3] = Enum.GetNames(typeof(LimbType)).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("healme", "healme [all]: Restore controlled character to full health. By default only heals common afflictions such as physical damage and blood loss: use the \"all\" argument to heal everything, including poisonings/addictions/etc.", delegate(string[] args)
			{
				bool healAll = args.Length != 0 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase);
				if (Character.Controlled != null)
				{
					DebugConsole.HealCharacter(Character.Controlled, healAll, null);
				}
			}, () => new string[][]
			{
				new string[]
				{
					"all"
				}
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("heal", "heal [character name] [all]: Restore the specified character to full health. If the name parameter is omitted, the controlled character will be healed. By default only heals common afflictions such as physical damage and blood loss: use the \"all\" argument to heal everything, including poisonings/addictions/etc.", delegate(string[] args)
			{
				bool healAll = args.Length > 1 && args[1].Equals("all", StringComparison.OrdinalIgnoreCase);
				DebugConsole.HandleCommandForCrewOrSingleCharacter(args, delegate(Character targetCharacter)
				{
					DebugConsole.HealCharacter(targetCharacter, healAll, null);
				}, null);
			}, () => new string[][]
			{
				DebugConsole.ListCharacterNames(true, true),
				new string[]
				{
					"all"
				}
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("listsuitabletreatments", "listsuitabletreatments [character name]: List which items are the most suitable for treating the specified character. Useful for debugging medic AI.", delegate(string[] args)
			{
				Character character = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (character != null)
				{
					Dictionary<Identifier, float> treatments = new Dictionary<Identifier, float>();
					character.CharacterHealth.GetSuitableTreatments(treatments, null, null, false, true, false, 0f);
					foreach (KeyValuePair<Identifier, float> treatment in from t in treatments
					orderby t.Value descending
					select t)
					{
						Color color = Color.White;
						DebugConsole.NewMessage(((int)treatment.Value).ToString() + ": " + treatment.Key.ToString(), new Color?(color), false);
					}
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("revive", "revive [character name]: Bring the specified character back from the dead. If the name parameter is omitted, the controlled character will be revived.", delegate(string[] args)
			{
				Character revivedCharacter = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (revivedCharacter == null)
				{
					return;
				}
				revivedCharacter.Revive(true, false);
				if (GameMain.Server != null)
				{
					foreach (Client c in GameMain.Server.ConnectedClients)
					{
						if (c.Character == revivedCharacter)
						{
							GameServer server = GameMain.Server;
							ServerSettings serverSettings = (server != null) ? server.ServerSettings : null;
							if (serverSettings != null && serverSettings.IronmanModeActive)
							{
								GameSession gameSession = GameMain.GameSession;
								MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
								if (mpCampaign != null)
								{
									CharacterCampaignData characterToRestore = mpCampaign.RestoreSingleCharacterFromBackup(c);
									if (characterToRestore != null)
									{
										characterToRestore.CharacterInfo.PermanentlyDead = false;
										mpCampaign.SaveSingleCharacter(characterToRestore, true);
									}
								}
							}
							GameMain.Server.SetClientCharacter(c, revivedCharacter);
							break;
						}
					}
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("freeze", "", delegate(string[] args)
			{
				if (Character.Controlled != null)
				{
					Character.Controlled.AnimController.Frozen = !Character.Controlled.AnimController.Frozen;
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("ragdoll", "ragdoll [character name]: Force-ragdoll the specified character. If the name parameter is omitted, the controlled character will be ragdolled.", delegate(string[] args)
			{
				Character ragdolledCharacter = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (ragdolledCharacter != null)
				{
					ragdolledCharacter.IsForceRagdolled = !ragdolledCharacter.IsForceRagdolled;
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("freecamera|freecam", "freecam: Detach the camera from the controlled character.", delegate(string[] args)
			{
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("eventmanager", "eventmanager: Toggle event manager on/off. No new random events are created when the event manager is disabled.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.EventManager : null) != null)
				{
					GameMain.GameSession.EventManager.Enabled = !GameMain.GameSession.EventManager.Enabled;
					DebugConsole.NewMessage(GameMain.GameSession.EventManager.Enabled ? "Event manager on" : "Event manager off", new Color?(Color.White), false);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("triggerevent", "triggerevent [identifier]: Trigger an event based on identifier.", delegate(string[] args)
			{
				List<EventPrefab> allEventPrefabsWithId = (from prefab in EventSet.GetAllEventPrefabs()
				where prefab.Identifier != Identifier.Empty
				select prefab).ToList<EventPrefab>();
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.EventManager : null) != null && args.Length != 0)
				{
					string eventPrefabId = args[0];
					if (eventPrefabId == "all")
					{
						using (IEnumerator<EventPrefab> enumerator = (from e in allEventPrefabsWithId
						where e.EventType == typeof(ScriptedEvent)
						select e).GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								EventPrefab eventPrefab = enumerator.Current;
								Event newEvent = eventPrefab.CreateInstance(GameMain.GameSession.EventManager.RandomSeed);
								if (newEvent == null)
								{
									DebugConsole.NewMessage("Could not initialize event " + eventPrefabId + " because level did not meet requirements", null, false);
									return;
								}
								GameMain.GameSession.EventManager.ActivateEvent(newEvent);
							}
							goto IL_21F;
						}
					}
					EventPrefab eventPrefab2 = allEventPrefabsWithId.Find((EventPrefab prefab) => prefab.Identifier == eventPrefabId);
					if (eventPrefab2 is TraitorEventPrefab)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(83, 1);
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventPrefab2.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(" is a traitor event. You need to use the 'triggertraitorevent' command to start it.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						return;
					}
					if (eventPrefab2 == null)
					{
						DebugConsole.NewMessage("Failed to trigger event because " + eventPrefabId + " is not a valid event identifier.", new Color?(Color.Red), false);
						return;
					}
					Event newEvent2 = eventPrefab2.CreateInstance(GameMain.GameSession.EventManager.RandomSeed);
					if (newEvent2 == null)
					{
						DebugConsole.NewMessage("Could not initialize event " + eventPrefabId + " because level did not meet requirements", null, false);
						return;
					}
					GameMain.GameSession.EventManager.ActivateEvent(newEvent2);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Initialized event ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(eventPrefab2.Identifier);
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Aqua), false);
					return;
				}
				IL_21F:
				DebugConsole.NewMessage("Failed to trigger event", new Color?(Color.Red), false);
			}, delegate()
			{
				List<EventPrefab> eventPrefabs = (from prefab in EventSet.GetAllEventPrefabs()
				where prefab.Identifier != Identifier.Empty
				select prefab).ToList<EventPrefab>();
				string[][] array = new string[1][];
				array[0] = (from id in (from prefab in eventPrefabs
				select prefab.Identifier).Distinct<Identifier>()
				select id.Value).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugevent", "debugevent [identifier]: outputs debug info about a specific event that's currently active. Mainly intended for debugging events in multiplayer: in single player, the same information is available by enabling debugdraw.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please specify the identifier of the event you want to debug.", null, null, false, false);
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				EventManager eventManager = (gameSession != null) ? gameSession.EventManager : null;
				if (eventManager != null)
				{
					Event ev2 = eventManager.ActiveEvents.FirstOrDefault(delegate(Event ev)
					{
						EventPrefab prefab = ev.Prefab;
						Identifier? identifier;
						Identifier? identifier2;
						if (prefab == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							identifier2 = new Identifier?(prefab.Identifier);
						}
						identifier = identifier2;
						return identifier == args[0];
					});
					if (ev2 == null)
					{
						DebugConsole.ThrowError("Event \"" + args[0] + "\" not found.", null, null, false, false);
						return;
					}
					string info = ev2.GetDebugInfo();
					RichTextData.GetRichTextData(info, out info);
					DebugConsole.NewMessage(info, null, false);
				}
			}, delegate()
			{
				GameSession gameSession = GameMain.GameSession;
				IEnumerable<EventPrefab> eventPrefabs;
				if (((gameSession != null) ? gameSession.EventManager : null) == null || GameMain.GameSession.EventManager.ActiveEvents.None(null))
				{
					eventPrefabs = from prefab in EventSet.GetAllEventPrefabs()
					where prefab.Identifier != Identifier.Empty
					select prefab;
				}
				else
				{
					eventPrefabs = from e in GameMain.GameSession.EventManager.ActiveEvents
					select e.Prefab;
				}
				string[][] array = new string[1][];
				array[0] = ((from ev in eventPrefabs
				select ev.Identifier.ToString()).ToArray<string>() ?? Array.Empty<string>());
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("unlockmission", "unlockmission [identifier/tag]: Unlocks a mission in a random adjacent level.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("The unlockmission command is only usable in the campaign mode.", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please enter the identifier or a tag of the mission you want to unlock.", null, null, false, false);
					return;
				}
				Location currentLocation = campaign.Map.CurrentLocation;
				if (MissionPrefab.Prefabs.Any((MissionPrefab p) => p.Identifier == args[0]))
				{
					currentLocation.UnlockMissionByIdentifier(args[0].ToIdentifier(), null);
				}
				else
				{
					currentLocation.UnlockMissionByTag(args[0].ToIdentifier(), null, null);
				}
				MultiPlayerCampaign mpCampaign = campaign as MultiPlayerCampaign;
				if (mpCampaign != null)
				{
					mpCampaign.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions);
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from p in MissionPrefab.Prefabs
				select p.Identifier.ToString()).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setcampaignmetadata", "setcampaignmetadata [identifier] [value]: Sets the specified campaign metadata value.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("The setcampaignmetadata command is only usable in the campaign mode.", null, null, false, false);
					return;
				}
				if (args.Length < 2)
				{
					DebugConsole.ThrowError("Please specify an identifier and a value.", null, null, false, false);
					return;
				}
				float floatVal;
				if (float.TryParse(args[1], out floatVal))
				{
					SetDataAction.PerformOperation(campaign.CampaignMetadata, args[0].ToIdentifier(), floatVal, SetDataAction.OperationType.Set);
					return;
				}
				SetDataAction.PerformOperation(campaign.CampaignMetadata, args[0].ToIdentifier(), args[1], SetDataAction.OperationType.Set);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setskill", "setskill [all/identifier] [max/level] [character]: Set your skill level.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Missing arguments. Expected at least 2 but got ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(args.Length);
					defaultInterpolatedStringHandler.AppendLiteral(" (skill, level, name)");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
					return;
				}
				Identifier skillIdentifier = args[0].ToIdentifier();
				string levelString = args[1];
				Character character = (args.Length >= 3) ? DebugConsole.FindMatchingCharacter(args.Skip(2).ToArray<string>(), false, null, false) : Character.Controlled;
				bool flag;
				if (character == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = character.Info;
					flag = (((info != null) ? info.Job : null) != null);
				}
				if (!flag)
				{
					DebugConsole.NewMessage("Character is not valid.", new Color?(Color.Red), false);
					return;
				}
				bool isMax = levelString.Equals("max", StringComparison.OrdinalIgnoreCase);
				float level;
				if (!float.TryParse(levelString, NumberStyles.Number, CultureInfo.InvariantCulture, out level) && !isMax)
				{
					DebugConsole.NewMessage(levelString + " is not a valid level. Expected number or \"max\".", new Color?(Color.Red), false);
					return;
				}
				if (isMax)
				{
					level = 100f;
				}
				if (skillIdentifier == "all")
				{
					foreach (Skill skill in character.Info.Job.GetSkills())
					{
						character.Info.SetSkillLevel(skill.Identifier, level, false);
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Set all ");
					defaultInterpolatedStringHandler2.AppendFormatted(character.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("'s skills to ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(level);
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Green), false);
					return;
				}
				character.Info.SetSkillLevel(skillIdentifier, level, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(17, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("Set ");
				defaultInterpolatedStringHandler3.AppendFormatted(character.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("'s ");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(skillIdentifier);
				defaultInterpolatedStringHandler3.AppendLiteral(" level to ");
				defaultInterpolatedStringHandler3.AppendFormatted<float>(level);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Color?(Color.Green), false);
			}, delegate()
			{
				string[][] array = new string[3][];
				int num = 0;
				Character controlled = Character.Controlled;
				string[] array2;
				if (controlled == null)
				{
					array2 = null;
				}
				else
				{
					CharacterInfo info = controlled.Info;
					if (info == null)
					{
						array2 = null;
					}
					else
					{
						Job job = info.Job;
						if (job == null)
						{
							array2 = null;
						}
						else
						{
							IEnumerable<Skill> skills = job.GetSkills();
							if (skills == null)
							{
								array2 = null;
							}
							else
							{
								array2 = (from skill in skills
								select skill.Identifier.Value).ToArray<string>();
							}
						}
					}
				}
				array[num] = (array2 ?? Array.Empty<string>());
				array[1] = new string[]
				{
					"max"
				};
				array[2] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("water|editwater", "water/editwater: Toggle water editing. Allows adding water into rooms by holding the left mouse button and removing it by holding the right mouse button.", delegate(string[] args)
			{
				Hull.EditWater = !Hull.EditWater;
				DebugConsole.NewMessage(Hull.EditWater ? "Water editing on" : "Water editing off", new Color?(Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("givetalent", "givetalent [talent] [player]: give the talent to the specified character. If the character argument is omitted, the talent is given to the controlled character.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				Character character = (args.Length >= 2) ? DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false) : Character.Controlled;
				if (character != null)
				{
					TalentPrefab talentPrefab = TalentPrefab.TalentPrefabs.Find((TalentPrefab c) => c.Identifier == args[0] || c.DisplayName.Equals(args[0], StringComparison.OrdinalIgnoreCase));
					if (talentPrefab == null)
					{
						DebugConsole.ThrowError("Couldn't find the talent \"" + args[0] + "\".", null, null, false, false);
						return;
					}
					character.GiveTalent(talentPrefab, true);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Gave talent \"");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(talentPrefab.DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
					defaultInterpolatedStringHandler.AppendFormatted(character.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
				}
			}, delegate()
			{
				List<string> talentNames = new List<string>();
				foreach (TalentPrefab talent in TalentPrefab.TalentPrefabs)
				{
					talentNames.Add(talent.DisplayName.Value);
				}
				string[][] array = new string[2][];
				array[0] = (from id in talentNames
				select id).ToArray<string>();
				array[1] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("unlocktalents", "unlocktalents [all/[jobname]] [character]: give the specified character all the talents of the specified class", delegate(string[] args)
			{
				Character character = (args.Length >= 2) ? DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false) : Character.Controlled;
				if (character == null)
				{
					return;
				}
				List<TalentTree> talentTrees = new List<TalentTree>();
				if (args.Length == 0 || args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
				{
					talentTrees.AddRange(TalentTree.JobTalentTrees);
				}
				else
				{
					JobPrefab job = JobPrefab.Prefabs.Find((JobPrefab jp) => jp.Name != null && jp.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
					if (job == null)
					{
						DebugConsole.ThrowError("Failed to find the job \"" + args[0] + "\".", null, null, false, false);
						return;
					}
					TalentTree talentTree;
					if (!TalentTree.JobTalentTrees.TryGet(job.Identifier, out talentTree))
					{
						DebugConsole.ThrowError("No talents configured for the job \"" + args[0] + "\".", null, null, false, false);
						return;
					}
					talentTrees.Add(talentTree);
				}
				foreach (TalentTree talentTree2 in talentTrees)
				{
					foreach (Identifier talentId in talentTree2.AllTalentIdentifiers)
					{
						character.GiveTalent(talentId, true);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unlocked talent \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(talentId);
						defaultInterpolatedStringHandler.AppendLiteral("\".");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					}
				}
			}, delegate()
			{
				List<string> availableArgs = new List<string>
				{
					"All"
				};
				availableArgs.AddRange(from j in JobPrefab.Prefabs
				select j.Name.Value);
				string[][] array = new string[2][];
				array[0] = availableArgs.ToArray();
				array[1] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("giveexperience", "giveexperience [amount] [character]: Give experience to character.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Missing arguments. Expected at least 1 but got ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(args.Length);
					defaultInterpolatedStringHandler.AppendLiteral(" (experience, name)");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					return;
				}
				string experienceString = args[0];
				Character character = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false) ?? Character.Controlled;
				if (((character != null) ? character.Info : null) == null)
				{
					DebugConsole.NewMessage("Character is not valid.", null, false);
					return;
				}
				int experience;
				if (int.TryParse(experienceString, NumberStyles.Number, CultureInfo.InvariantCulture, out experience))
				{
					character.Info.GiveExperience(experience);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Gave ");
					defaultInterpolatedStringHandler2.AppendFormatted(character.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(experience);
					defaultInterpolatedStringHandler2.AppendLiteral(" experience");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
					return;
				}
				DebugConsole.NewMessage(experienceString + " is not a valid value. Expected number.", null, false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = new string[]
				{
					"100"
				};
				array[1] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("fire|editfire", "fire/editfire: Allows putting up fires by left clicking.", delegate(string[] args)
			{
				Hull.EditFire = !Hull.EditFire;
				DebugConsole.NewMessage(Hull.EditFire ? "Fire spawning on" : "Fire spawning off", new Color?(Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("explosion", "explosion [range] [force] [damage] [structuredamage] [item damage] [emp strength] [ballast flora strength]: Creates an explosion at the position of the cursor.", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("showseed|showlevelseed", "showseed: Show the seed of the current level.", delegate(string[] args)
			{
				if (Level.Loaded == null)
				{
					DebugConsole.ThrowError("No level loaded.", null, null, false, false);
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				Map map = (gameSession != null) ? gameSession.Map : null;
				if (map != null)
				{
					DebugConsole.NewMessage("Map seed: " + map.Seed, null, false);
				}
				DebugConsole.NewMessage("Level seed: " + Level.Loaded.Seed, null, false);
				DebugConsole.NewMessage("Level generation params: " + Level.Loaded.GenerationParams.Identifier.ToString(), null, false);
				string str = "Adjacent locations: ";
				Location startLocation = Level.Loaded.StartLocation;
				string str2 = ((startLocation != null) ? startLocation.Type.Identifier : "none".ToIdentifier()).ToString();
				string str3 = ", ";
				Location startLocation2 = Level.Loaded.StartLocation;
				DebugConsole.NewMessage(str + str2 + str3 + ((startLocation2 != null) ? startLocation2.Type.Identifier : "none".ToIdentifier()).ToString(), null, false);
				DebugConsole.NewMessage("Mirrored: " + Level.Loaded.Mirrored.ToString(), null, false);
				string str4 = "Level size: ";
				Point size = Level.Loaded.Size;
				string str5 = size.X.ToString();
				string str6 = "x";
				size = Level.Loaded.Size;
				DebugConsole.NewMessage(str4 + str5 + str6 + size.Y.ToString(), null, false);
				string str7 = "Minimum main path width: ";
				LevelData levelData = Level.Loaded.LevelData;
				DebugConsole.NewMessage(str7 + (((levelData != null) ? ((levelData.MinMainPathWidth != null) ? levelData.MinMainPathWidth.GetValueOrDefault().ToString() : null) : null) ?? "unknown"), null, false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("teleportsub", "teleportsub [start/end/endoutpost/cursor] [submarine_team]: Teleport the submarine to the position of the cursor, or the start or end of the level. The 'endoutpost' argument also automatically docks the sub with the outpost at the end of the level. WARNING: does not take outposts into account, so often leads to physics glitches. Only use for debugging.", delegate(string[] args)
			{
				if (Submarine.MainSub == null)
				{
					return;
				}
				Submarine submarineToTeleport = Submarine.MainSub;
				if (args.Length > 1)
				{
					foreach (Submarine sub in from s in Submarine.Loaded
					where s.PhysicsBody.BodyType == BodyType.Dynamic
					select s)
					{
						if (sub.Info.Name + "_" + sub.TeamID.ToString() == args[1])
						{
							submarineToTeleport = sub;
							break;
						}
					}
				}
				if (args.Length == 0 || args[0].Equals("cursor", StringComparison.OrdinalIgnoreCase))
				{
					DebugConsole.ThrowError("Cannot teleport the sub to the position of the cursor. Use \"start\" or \"end\", or execute the command as a client.", null, null, false, false);
					return;
				}
				if (args[0].Equals("start", StringComparison.OrdinalIgnoreCase))
				{
					if (Level.Loaded == null)
					{
						DebugConsole.NewMessage("Can't teleport the sub to the start of the level (no level loaded).", new Color?(Color.Red), false);
						return;
					}
					Vector2 pos = Level.Loaded.StartPosition;
					if (Level.Loaded.StartOutpost != null)
					{
						pos -= Vector2.UnitY * (float)(submarineToTeleport.Borders.Height + Level.Loaded.StartOutpost.Borders.Height) / 2f;
					}
					submarineToTeleport.SetPosition(pos, null, true);
					return;
				}
				else
				{
					if (!args[0].Equals("end", StringComparison.OrdinalIgnoreCase))
					{
						if (args[0].Equals("endoutpost", StringComparison.OrdinalIgnoreCase))
						{
							Level loaded = Level.Loaded;
							if (((loaded != null) ? loaded.EndOutpost : null) == null)
							{
								DebugConsole.NewMessage("Can't teleport the sub to the end outpost (no outpost at the end of the level).", new Color?(Color.Red), false);
								return;
							}
							submarineToTeleport.SetPosition(Level.Loaded.EndExitPosition - Vector2.UnitY * (float)submarineToTeleport.Borders.Height, null, true);
							DockingPort submarineDockingPort = DockingPort.List.FirstOrDefault((DockingPort d) => d.Item.Submarine == submarineToTeleport);
							DockingPort outpostDockingPort = DockingPort.List.FirstOrDefault((DockingPort d) => d.Item.Submarine == Level.Loaded.EndOutpost);
							if (submarineDockingPort != null && outpostDockingPort != null)
							{
								submarineDockingPort.Dock(outpostDockingPort);
							}
						}
						return;
					}
					if (Level.Loaded == null)
					{
						DebugConsole.NewMessage("Can't teleport the sub to the end of the level (no level loaded).", new Color?(Color.Red), false);
						return;
					}
					Vector2 pos2 = Level.Loaded.EndPosition;
					if (Level.Loaded.EndOutpost != null)
					{
						pos2 -= Vector2.UnitY * (float)(submarineToTeleport.Borders.Height + Level.Loaded.EndOutpost.Borders.Height) / 2f;
					}
					submarineToTeleport.SetPosition(pos2, null, true);
					return;
				}
			}, () => new string[][]
			{
				new string[]
				{
					"start",
					"end",
					"endoutpost",
					"cursor"
				},
				DebugConsole.ListAvailableSubmarines()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("showreputation", "showreputation: List the current reputation values.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign != null)
				{
					DebugConsole.NewMessage("Reputation:", null, false);
					using (IEnumerator<Faction> enumerator = campaign.Factions.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Faction faction = enumerator.Current;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 2);
							defaultInterpolatedStringHandler.AppendLiteral(" - ");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(faction.Prefab.Name);
							defaultInterpolatedStringHandler.AppendLiteral(": ");
							defaultInterpolatedStringHandler.AppendFormatted<float>(faction.Reputation.Value);
							DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
						}
						return;
					}
				}
				DebugConsole.ThrowError("Could not show reputation (no active campaign).", null, null, false, false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("setlocationreputation", "setlocationreputation [value]: Set the reputation in the current location to the specified value.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("Could not set location reputation (no active campaign).", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					return;
				}
				float reputation;
				if (!float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out reputation))
				{
					DebugConsole.ThrowError("Could not set location reputation (" + args[0] + " is not a valid reputation value).", null, null, false, false);
					return;
				}
				Reputation reputation2 = campaign.Map.CurrentLocation.Reputation;
				if (reputation2 == null)
				{
					return;
				}
				reputation2.SetReputation(reputation);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setreputation", "setreputation [faction] [value]: Set the reputation of a cation to the specified value.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					DebugConsole.ThrowError("Insufficient arguments (expected 2)", null, null, false, false);
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("Could not set faction reputation (no active campaign).", null, null, false, false);
					return;
				}
				Faction faction = campaign.Factions.FirstOrDefault((Faction f) => f.Prefab.Identifier == args[0]);
				if (faction == null)
				{
					DebugConsole.ThrowError("Could not set faction reputation (faction " + args[0] + " not found).", null, null, false, false);
					return;
				}
				float reputation;
				if (float.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out reputation))
				{
					faction.Reputation.SetReputation(reputation);
					return;
				}
				DebugConsole.ThrowError("Could not set faction reputation (" + args[1] + " is not a valid reputation value).", null, null, false, false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = (from f in FactionPrefab.Prefabs
				select f.Identifier.Value).ToArray<string>();
				int num = 1;
				GameSession gameSession = GameMain.GameSession;
				string[] array2;
				if (gameSession == null)
				{
					array2 = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					if (campaign == null)
					{
						array2 = null;
					}
					else
					{
						array2 = (from f in campaign.Factions
						select f.Prefab.Identifier.ToString()).ToArray<string>();
					}
				}
				array[num] = (array2 ?? Array.Empty<string>());
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("fixitems", "fixitems: Repairs all items and restores them to full condition.", delegate(string[] args)
			{
				foreach (Item it in Item.ItemList)
				{
					if (it.GetComponent<GeneticMaterial>() == null)
					{
						it.Condition = it.MaxCondition;
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("fixhulls|fixwalls", "fixwalls/fixhulls: Fixes all walls.", delegate(string[] args)
			{
				List<Structure> walls = new List<Structure>(Structure.WallList);
				foreach (Structure w in walls)
				{
					try
					{
						for (int i = 0; i < w.SectionCount; i++)
						{
							w.AddDamage(i, -100000f, null, true, false);
						}
					}
					catch (InvalidOperationException e)
					{
						string errorMsg = "Error while executing the fixhulls command.\n" + e.StackTrace.CleanupStackTrace();
						GameAnalyticsManager.AddErrorEventOnce("DebugConsole.FixHulls", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("maxupgrades", "maxupgrades [category] [prefab]: Maxes out all upgrades or only specific one if given arguments.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				UpgradeManager upgradeManager2;
				if (gameSession == null)
				{
					upgradeManager2 = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					upgradeManager2 = ((campaign != null) ? campaign.UpgradeManager : null);
				}
				UpgradeManager upgradeManager = upgradeManager2;
				if (upgradeManager == null)
				{
					DebugConsole.ThrowError("This command can only be used in campaign.", null, null, false, false);
					return;
				}
				string categoryIdentifier = null;
				string prefabIdentifier = null;
				int num = args.Length;
				if (num != 1)
				{
					if (num == 2)
					{
						categoryIdentifier = args[0];
						prefabIdentifier = args[1];
					}
				}
				else
				{
					categoryIdentifier = args[0];
				}
				foreach (UpgradeCategory category in UpgradeCategory.Categories)
				{
					if (string.IsNullOrWhiteSpace(categoryIdentifier) || !(category.Identifier != categoryIdentifier))
					{
						foreach (UpgradePrefab prefab in UpgradePrefab.Prefabs)
						{
							if (prefab.UpgradeCategories.Contains(category) && (string.IsNullOrWhiteSpace(prefabIdentifier) || !(prefab.Identifier != prefabIdentifier)))
							{
								int targetLevel = prefab.GetMaxLevelForCurrentSub() - upgradeManager.GetRealUpgradeLevel(prefab, category);
								for (int i = 0; i < targetLevel; i++)
								{
									upgradeManager.TryPurchaseUpgrade(prefab, category, true, null);
								}
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
								defaultInterpolatedStringHandler.AppendLiteral("Upgraded ");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(category.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral(".");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral(" by ");
								defaultInterpolatedStringHandler.AppendFormatted<int>(targetLevel);
								defaultInterpolatedStringHandler.AppendLiteral(" levels.");
								DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.DarkGreen), false);
							}
						}
					}
				}
				DebugConsole.NewMessage("Start a new round to apply the upgrades.", new Color?(Color.Lime), false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = (from i in (from c in UpgradeCategory.Categories
				select c.Identifier).Distinct<Identifier>()
				select i.Value).ToArray<string>();
				array[1] = (from i in (from c in UpgradePrefab.Prefabs
				select c.Identifier).Distinct<Identifier>()
				select i.Value).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("power", "power: Immediately powers up the submarine's nuclear reactor.", delegate(string[] args)
			{
				Item reactorItem = Item.ItemList.Find((Item i) => i.GetComponent<Reactor>() != null);
				if (reactorItem == null)
				{
					return;
				}
				Reactor reactor = reactorItem.GetComponent<Reactor>();
				reactor.PowerUpImmediately();
				if (GameMain.Server != null)
				{
					reactorItem.CreateServerEvent<Reactor>(reactor);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("oxygen|air", "oxygen/air: Replenishes the oxygen levels in every room to 100%.", delegate(string[] args)
			{
				foreach (Hull hull in Hull.HullList)
				{
					hull.OxygenPercentage = 100f;
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("kill", "kill [character]: Immediately kills the specified character.", delegate(string[] args)
			{
				Character killedCharacter = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (killedCharacter != null)
				{
					killedCharacter.Kill(CauseOfDeathType.Unknown, null, false, true);
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("killmonsters", "killmonsters: Immediately kills all AI-controlled enemies in the level.", delegate(string[] args)
			{
				foreach (Character c in Character.CharacterList)
				{
					EnemyAIController enemyAI = c.AIController as EnemyAIController;
					if (enemyAI != null && enemyAI.PetBehavior == null)
					{
						c.SetAllDamage(200f, 0f, 0f);
					}
				}
				foreach (Hull hull in Hull.HullList)
				{
					BallastFloraBehavior ballastFlora = hull.BallastFlora;
					if (ballastFlora != null)
					{
						ballastFlora.Kill();
					}
				}
				foreach (Submarine sub in Submarine.Loaded)
				{
					WreckAI wreckAI = sub.WreckAI;
					if (wreckAI != null)
					{
						wreckAI.Kill();
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("killall", "killall: Immediately kills all characters in the level.", delegate(string[] args)
			{
				foreach (Character c in Character.CharacterList)
				{
					c.Kill(CauseOfDeathType.Unknown, null, false, true);
					DebugConsole.NewMessage("Killed " + c.DisplayName + ".", null, false);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("despawnnow", "despawnnow [character]: Immediately despawns the specified dead character. If the character argument is omitted, all dead characters are despawned.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					using (List<Character>.Enumerator enumerator = (from c in Character.CharacterList
					where c.IsDead
					select c).ToList<Character>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Character c2 = enumerator.Current;
							c2.DespawnNow(true);
						}
						return;
					}
				}
				Character character = DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (character != null)
				{
					character.DespawnNow(true);
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				where c.IsDead
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setclientcharacter", "setclientcharacter [client name] [character name]: Gives the client control of the specified character.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("campaigninfo|campaignstatus", "campaigninfo: Display information about the state of the currently active campaign.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("No campaign active!", null, null, false, false);
					return;
				}
				campaign.LogState();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("campaigndestination|setcampaigndestination", "campaigndestination [index]: Set the location to head towards in the currently active campaign.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
				CampaignMode campaign = gameMode as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("No campaign active!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					int i = 0;
					foreach (LocationConnection connection in campaign.Map.CurrentLocation.Connections)
					{
						DebugConsole.NewMessage("     " + i.ToString() + ". " + connection.OtherLocation(campaign.Map.CurrentLocation).DisplayName, new Color?(Color.White), false);
						i++;
					}
					DebugConsole.ShowQuestionPrompt("Select a destination (0 - " + (campaign.Map.CurrentLocation.Connections.Count - 1).ToString() + "):", delegate(string selectedDestination)
					{
						int destinationIndex2 = -1;
						if (!int.TryParse(selectedDestination, out destinationIndex2))
						{
							return;
						}
						if (destinationIndex2 < 0 || destinationIndex2 >= campaign.Map.CurrentLocation.Connections.Count)
						{
							DebugConsole.NewMessage("Index out of bounds!", new Color?(Color.Red), false);
							return;
						}
						Location location2 = campaign.Map.CurrentLocation.Connections[destinationIndex2].OtherLocation(campaign.Map.CurrentLocation);
						campaign.Map.SelectLocation(location2);
						DebugConsole.NewMessage(location2.DisplayName + " selected.", new Color?(Color.White), false);
					}, null, -1);
					return;
				}
				int destinationIndex = -1;
				if (!int.TryParse(args[0], out destinationIndex))
				{
					return;
				}
				if (destinationIndex < 0 || destinationIndex >= campaign.Map.CurrentLocation.Connections.Count)
				{
					DebugConsole.NewMessage("Index out of bounds!", new Color?(Color.Red), false);
					return;
				}
				Location location = campaign.Map.CurrentLocation.Connections[destinationIndex].OtherLocation(campaign.Map.CurrentLocation);
				campaign.Map.SelectLocation(location);
				DebugConsole.NewMessage(location.DisplayName + " selected.", new Color?(Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglecampaignteleport", "Toggle on/off teleportation between campaign locations by double clicking on the campaign map.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) == null)
				{
					DebugConsole.ThrowError("No campaign active.", null, null, false, false);
					return;
				}
				GameMain.GameSession.Map.AllowDebugTeleport = !GameMain.GameSession.Map.AllowDebugTeleport;
				DebugConsole.NewMessage((GameMain.GameSession.Map.AllowDebugTeleport ? "Enabled" : "Disabled") + " teleportation on the campaign map.", new Color?(Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("money", "money [amount] [character]: Gives the specified amount of money to the crew when a campaign is active.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					return;
				}
				Character targetCharacter = null;
				if (args.Length >= 2)
				{
					targetCharacter = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false);
				}
				int money;
				if (int.TryParse(args[0], out money))
				{
					Wallet wallet = (targetCharacter == null || GameMain.IsSingleplayer) ? campaign.Bank : targetCharacter.Wallet;
					wallet.Give(money);
					GameAnalyticsManager.AddMoneyGainedEvent(money, GameAnalyticsManager.MoneySource.Cheat, "console");
					return;
				}
				DebugConsole.ThrowError("\"" + args[0] + "\" is not a valid numeric value.", null, null, false, false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = new string[]
				{
					string.Empty
				};
				array[1] = (from c in Character.CharacterList
				select c.Name).Distinct<string>().ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("showmoney", "showmoney: Shows the amount of money in everyones wallet.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("No campaign active!", null, null, false, false);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Bank: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(campaign.Bank.Balance);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("skipeventcooldown", "skipeventcooldown: Skips the currently active event cooldown and triggers pending monster spawns immediately.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession == null)
				{
					return;
				}
				EventManager eventManager = gameSession.EventManager;
				if (eventManager == null)
				{
					return;
				}
				eventManager.SkipEventCooldown();
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("ballastflora", "infectballast [options]: Infect ballasts and control its growth.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("No action specified.", null, null, false, false);
					return;
				}
				string primaryAction = (args.Length != 0) ? args[0] : "";
				string secondaryArgument = (args.Length > 1) ? args[1] : "";
				if (Submarine.MainSub == null)
				{
					DebugConsole.ThrowError("No submarine loaded.", null, null, false, false);
					return;
				}
				if (primaryAction.Equals("infect", StringComparison.OrdinalIgnoreCase))
				{
					List<Pump> pumps = new List<Pump>();
					foreach (Item item in Submarine.MainSub.GetItems(true))
					{
						if (item.CurrentHull != null && item.HasTag(Tags.Ballast))
						{
							Pump pump = item.GetComponent<Pump>();
							if (pump != null && item.CurrentHull.BallastFlora == null)
							{
								pumps.Add(pump);
							}
						}
					}
					if (pumps.Any<Pump>())
					{
						BallastFloraPrefab prefab = string.IsNullOrWhiteSpace(secondaryArgument) ? BallastFloraPrefab.Prefabs.First<BallastFloraPrefab>() : BallastFloraPrefab.Find(secondaryArgument.ToIdentifier());
						if (prefab == null)
						{
							DebugConsole.ThrowError("No such behavior: " + secondaryArgument, null, null, false, false);
							return;
						}
						Pump random = pumps.GetRandomUnsynced<Pump>();
						random.InfectBallast(prefab.Identifier, true);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Infected ");
						defaultInterpolatedStringHandler.AppendFormatted(random.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" with ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(" in ");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(random.Item.CurrentHull.DisplayName);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Green), false);
						return;
					}
					else
					{
						DebugConsole.ThrowError("No available pumps to infect on this submarine.", null, null, false, false);
					}
				}
				if (primaryAction.Equals("growthwarp", StringComparison.OrdinalIgnoreCase))
				{
					int value;
					if (int.TryParse(secondaryArgument, out value))
					{
						foreach (Hull hull in from h in Hull.HullList
						where h.BallastFlora != null
						select h)
						{
							BallastFloraBehavior bs = hull.BallastFlora;
							bs.GrowthWarps = value;
						}
						DebugConsole.NewMessage("Accelerating growth...", new Color?(Color.Green), false);
						return;
					}
					DebugConsole.ThrowError("Invalid integer \"" + secondaryArgument + "\".", null, null, false, false);
				}
			}, delegate()
			{
				string[] primaries = new string[]
				{
					"infect",
					"growthwarp"
				};
				string[] identifiers = (from i in (from bfp in BallastFloraPrefab.Prefabs
				select bfp.Identifier).Distinct<Identifier>()
				select i.Value).ToArray<string>();
				return new string[][]
				{
					primaries,
					identifiers
				};
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setdifficulty|forcedifficulty", "difficulty [0-100]. Leave the parameter empty to disable.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					Level.ForcedDifficulty = null;
					DebugConsole.NewMessage("Forced difficulty level disabled.", new Color?(Color.Green), false);
					return;
				}
				float difficulty;
				if (float.TryParse(args[0], out difficulty))
				{
					Level.ForcedDifficulty = new float?(difficulty);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Set the difficulty level to ");
					defaultInterpolatedStringHandler.AppendFormatted<float?>(Level.ForcedDifficulty);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Yellow), false);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("difficulty|leveldifficulty", "difficulty [0-100]: Change the level difficulty setting in the server lobby.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autoitemplacerdebug|outfitdebug", "autoitemplacerdebug: Toggle automatic item placer debug info on/off. The automatically placed items are listed in the debug console at the start of a round.", delegate(string[] args)
			{
				AutoItemPlacer.OutputDebugInfo = !AutoItemPlacer.OutputDebugInfo;
				DebugConsole.NewMessage((AutoItemPlacer.OutputDebugInfo ? "Enabled" : "Disabled") + " automatic item placer logging.", new Color?(Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("verboselogging", "verboselogging: Toggle verbose console logging on/off. When on, additional debug information is written to the debug console.", delegate(string[] args)
			{
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.VerboseLogging = !GameSettings.CurrentConfig.VerboseLogging;
				GameSettings.SetCurrentConfig(config);
				DebugConsole.NewMessage((GameSettings.CurrentConfig.VerboseLogging ? "Enabled" : "Disabled") + " verbose logging.", new Color?(Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("listtasks", "listtasks: Lists all asynchronous tasks currently in the task pool.", delegate(string[] args)
			{
				TaskPool.ListTasks(delegate(string line)
				{
					DebugConsole.NewMessage(line, null, false);
				});
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("listcoroutines", "listcoroutines: Lists all coroutines currently running.", delegate(string[] args)
			{
				CoroutineManager.ListCoroutines();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("calculatehashes", "calculatehashes [content package name]: Show the MD5 hashes of the files in the selected content package. If the name parameter is omitted, the first content package is selected.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					ContentPackageManager.EnabledPackages.Core.CalculateHash(true, null, null);
					return;
				}
				string packageName = string.Join(" ", args);
				ContentPackage package = ContentPackageManager.EnabledPackages.All.FirstOrDefault((ContentPackage p) => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));
				if (package == null)
				{
					DebugConsole.ThrowError("Content package \"" + packageName + "\" not found.", null, null, false, false);
					return;
				}
				package.CalculateHash(true, null, null);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from cp in ContentPackageManager.EnabledPackages.All
				select cp.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("simulatedlatency", "simulatedlatency [minimumlatencyseconds] [randomlatencyseconds]: applies a simulated latency to network messages. Useful for simulating real network conditions when testing the multiplayer locally.", delegate(string[] args)
			{
				if (args.Count<string>() < 2 || GameMain.NetworkMember == null)
				{
					return;
				}
				float minimumLatency;
				if (!float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out minimumLatency))
				{
					DebugConsole.ThrowError(args[0] + " is not a valid latency value.", null, null, false, false);
					return;
				}
				float randomLatency;
				if (!float.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out randomLatency))
				{
					DebugConsole.ThrowError(args[1] + " is not a valid latency value.", null, null, false, false);
					return;
				}
				if (GameMain.NetworkMember != null)
				{
					GameMain.NetworkMember.SimulatedMinimumLatency = minimumLatency;
					GameMain.NetworkMember.SimulatedRandomLatency = randomLatency;
				}
				DebugConsole.NewMessage(string.Concat(new string[]
				{
					"Set simulated minimum latency to ",
					minimumLatency.ToString(CultureInfo.InvariantCulture),
					" and random latency to ",
					randomLatency.ToString(CultureInfo.InvariantCulture),
					"."
				}), new Color?(Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("simulatedloss", "simulatedloss [lossratio]: applies simulated packet loss to network messages. For example, a value of 0.1 would mean 10% of the packets are dropped. Useful for simulating real network conditions when testing the multiplayer locally.", delegate(string[] args)
			{
				if (args.Count<string>() < 1 || GameMain.NetworkMember == null)
				{
					return;
				}
				float loss;
				if (!float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out loss))
				{
					DebugConsole.ThrowError(args[0] + " is not a valid loss ratio.", null, null, false, false);
					return;
				}
				if (GameMain.NetworkMember != null)
				{
					GameMain.NetworkMember.SimulatedLoss = loss;
				}
				DebugConsole.NewMessage("Set simulated packet loss to " + ((int)(loss * 100f)).ToString() + "%.", new Color?(Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("simulatedduplicateschance", "simulatedduplicateschance [duplicateratio]: simulates packet duplication in network messages. For example, a value of 0.1 would mean there's a 10% chance a packet gets sent twice. Useful for simulating real network conditions when testing the multiplayer locally.", delegate(string[] args)
			{
				if (args.Count<string>() < 1 || GameMain.NetworkMember == null)
				{
					return;
				}
				float duplicates;
				if (!float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out duplicates))
				{
					DebugConsole.ThrowError(args[0] + " is not a valid duplicate ratio.", null, null, false, false);
					return;
				}
				if (GameMain.NetworkMember != null)
				{
					GameMain.NetworkMember.SimulatedDuplicatesChance = duplicates;
				}
				DebugConsole.NewMessage("Set packet duplication to " + ((int)(duplicates * 100f)).ToString() + "%.", new Color?(Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("startitems|startitemset", "start item set identifier", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("No start item set identifier defined!", null, null, false, false);
					return;
				}
				AutoItemPlacer.DefaultStartItemSet = args[0].ToIdentifier();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Start item set changed to \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(AutoItemPlacer.DefaultStartItemSet);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("control", "control [character name]: Start controlling the specified character (client-only).", null, () => new string[][]
			{
				DebugConsole.ListCharacterNames(false, false)
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("los", "Toggle the line of sight effect on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("lighting|lights", "Toggle lighting on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("ambientlight", "ambientlight [color]: Change the color of the ambient light in the level.", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugdraw", "Toggle the debug drawing mode on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugwiring", "Toggle the wiring debug mode on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugdrawlocalization", "Toggle the localization debug drawing mode on/off (client-only). Colors all text that hasn't been fetched from a localization file magenta, making it easier to spot hard-coded or missing texts.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("debugdrawlos", "Toggle the los debug drawing mode on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("togglevoicechatfilters", "Toggle the radio/muffle filters in the voice chat (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglehud|hud", "Toggle the character HUD (inventories, icons, buttons, etc) on/off (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("toggleupperhud", "Toggle the upper part of the ingame HUD (chatbox, crewmanager) on/off (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("toggleitemhighlights", "Toggle the item highlight effect on/off (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglecharacternames", "Toggle the names hovering above characters on/off (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("followsub", "Toggle whether the camera should follow the nearest submarine (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("toggleaitargets|aitargets", "Toggle the visibility of AI targets (= targets that enemies can detect and attack/escape from) (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugai", "Toggle the ai debug mode on/off (works properly only in single player).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("devmode", "Toggle the dev mode on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("showmonsters", "Permanently unlocks all the monsters in the character editor. Use \"hidemonsters\" to undo.", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("hidemonsters", "Permanently hides in the character editor all the monsters that haven't been encountered in the game. Use \"showmonsters\" to undo.", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("loslightingfreecam", "Toggles line of sight effect, lighting, and enables freecam mode. (client-only)", null, null, true));
			DebugConsole.InitProjectSpecific();
			DebugConsole.commands.Sort((DebugConsole.Command c1, DebugConsole.Command c2) => c1.Names.First<Identifier>().CompareTo(c2.Names.First<Identifier>()));
		}

		// Token: 0x0600033B RID: 827 RVA: 0x0001BEF8 File Offset: 0x0001A0F8
		private static void HealCharacter(Character healedCharacter, bool healAll, Client targetClient = null)
		{
			healedCharacter.SetAllDamage(0f, 0f, 0f);
			healedCharacter.Oxygen = 100f;
			healedCharacter.Bloodloss = 0f;
			healedCharacter.SetStun(0f, true, false);
			if (healAll)
			{
				healedCharacter.CharacterHealth.RemoveAllAfflictions();
			}
			string characterNameText = (healedCharacter == Character.Controlled) ? (healedCharacter.Name + " (you)") : healedCharacter.Name;
			string text = healAll ? ("Healed " + characterNameText + ": all afflictions") : ("Healed " + characterNameText + ": damage and common afflictions");
			DebugConsole.NewMessage(text, new Color?(Color.Yellow), false);
			if (targetClient != null)
			{
				GameMain.Server.SendConsoleMessage(text, targetClient, null);
			}
		}

		// Token: 0x0600033C RID: 828 RVA: 0x0001BFBC File Offset: 0x0001A1BC
		public static string AutoComplete(string command, int increment = 1)
		{
			string[] splitCommand = ToolBox.SplitCommand(command);
			string[] args = splitCommand.Skip(1).ToArray<string>();
			if (args.Length != 0 || (splitCommand.Length != 0 && command.Last<char>() == ' '))
			{
				DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(splitCommand[0].ToIdentifier()));
				if (matchingCommand == null || matchingCommand.GetValidArgs == null)
				{
					return command;
				}
				int autoCompletedArgIndex = (args.Length != 0 && command.Last<char>() != ' ') ? (args.Length - 1) : args.Length;
				string[][] allArgs = matchingCommand.GetValidArgs();
				if (allArgs == null || allArgs.GetLength(0) < autoCompletedArgIndex + 1)
				{
					return command;
				}
				if (string.IsNullOrEmpty(DebugConsole.currentAutoCompletedCommand))
				{
					DebugConsole.currentAutoCompletedCommand = ((autoCompletedArgIndex > args.Length - 1) ? " " : args.Last<string>());
				}
				string[] validArgs = (from arg in allArgs[autoCompletedArgIndex]
				where DebugConsole.currentAutoCompletedCommand.Trim().Length <= arg.Length && arg.Substring(0, DebugConsole.currentAutoCompletedCommand.Trim().Length).ToLower() == DebugConsole.currentAutoCompletedCommand.Trim().ToLower()
				select arg).ToArray<string>();
				validArgs = validArgs.Concat(from arg in allArgs[autoCompletedArgIndex]
				where arg.ToLower().Contains(DebugConsole.currentAutoCompletedCommand.Trim().ToLower()) && !validArgs.Contains(arg)
				select arg).ToArray<string>();
				if (validArgs.Length == 0)
				{
					return command;
				}
				DebugConsole.currentAutoCompletedIndex = MathUtils.PositiveModulo(DebugConsole.currentAutoCompletedIndex + increment, validArgs.Length);
				string autoCompletedArg = validArgs[DebugConsole.currentAutoCompletedIndex];
				if (autoCompletedArg.Contains(' '))
				{
					autoCompletedArg = "\"" + autoCompletedArg + "\"";
				}
				for (int i = 0; i < splitCommand.Length; i++)
				{
					if (splitCommand[i].Contains(' '))
					{
						splitCommand[i] = "\"" + splitCommand[i] + "\"";
					}
				}
				string separator = " ";
				IEnumerable<string> values;
				if (autoCompletedArgIndex < args.Length)
				{
					values = splitCommand.Take(splitCommand.Length - 1);
				}
				else
				{
					IEnumerable<string> splitCommand2 = splitCommand;
					values = splitCommand2;
				}
				return string.Join(separator, values) + " " + autoCompletedArg;
			}
			else
			{
				if (string.IsNullOrWhiteSpace(DebugConsole.currentAutoCompletedCommand))
				{
					DebugConsole.currentAutoCompletedCommand = command;
				}
				List<Identifier> matchingCommands = new List<Identifier>();
				foreach (DebugConsole.Command c2 in DebugConsole.commands)
				{
					foreach (Identifier name in c2.Names)
					{
						if (DebugConsole.currentAutoCompletedCommand.Length <= name.Value.Length && name.StartsWith(DebugConsole.currentAutoCompletedCommand))
						{
							matchingCommands.Add(name);
						}
					}
				}
				if (matchingCommands.Count == 0)
				{
					return command;
				}
				DebugConsole.currentAutoCompletedIndex = MathUtils.PositiveModulo(DebugConsole.currentAutoCompletedIndex + increment, matchingCommands.Count);
				return matchingCommands[DebugConsole.currentAutoCompletedIndex].Value;
			}
		}

		// Token: 0x0600033D RID: 829 RVA: 0x0001C2B4 File Offset: 0x0001A4B4
		public static void ResetAutoComplete()
		{
			DebugConsole.currentAutoCompletedCommand = "";
			DebugConsole.currentAutoCompletedIndex = 0;
		}

		// Token: 0x0600033E RID: 830 RVA: 0x0001C2C8 File Offset: 0x0001A4C8
		public static void ExecuteCommand(string inputtedCommands)
		{
			if (string.IsNullOrWhiteSpace(inputtedCommands) || inputtedCommands == "\\" || inputtedCommands == "\n")
			{
				return;
			}
			string[] commandsToExecute = inputtedCommands.Split("\n", StringSplitOptions.None);
			foreach (string command in commandsToExecute)
			{
				if (DebugConsole.activeQuestionCallback != null)
				{
					DebugConsole.NewCommand(command, null);
					DebugConsole.QuestionCallback temp = DebugConsole.activeQuestionCallback;
					DebugConsole.activeQuestionCallback = null;
					temp(command);
					return;
				}
				if (string.IsNullOrWhiteSpace(command) || command == "\\")
				{
					return;
				}
				string[] splitCommand = ToolBox.SplitCommand(command);
				if (splitCommand.Length == 0)
				{
					DebugConsole.ThrowError("Failed to execute command \"" + command + "\"!", null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("DebugConsole.ExecuteCommand:LengthZero", GameAnalyticsManager.ErrorSeverity.Error, "Failed to execute command \"" + command + "\"!");
					return;
				}
				Identifier firstCommand = splitCommand[0].ToIdentifier();
				if (firstCommand != "admin")
				{
					DebugConsole.NewCommand(command, null);
				}
				bool commandFound = false;
				foreach (DebugConsole.Command c in DebugConsole.commands)
				{
					if (c.Names.Contains(firstCommand))
					{
						c.Execute(splitCommand.Skip(1).ToArray<string>());
						commandFound = true;
						break;
					}
				}
				if (!commandFound)
				{
					DebugConsole.ThrowError("Command \"" + splitCommand[0] + "\" not found.", null, null, false, false);
				}
			}
		}

		// Token: 0x0600033F RID: 831 RVA: 0x0001C45C File Offset: 0x0001A65C
		private static string[] ListAvailableLocations()
		{
			List<string> locationNames = new List<string>();
			foreach (Submarine submarine in Submarine.Loaded)
			{
				locationNames.Add(submarine.Info.Name);
			}
			if (Level.Loaded != null)
			{
				foreach (Level.Cave cave in Level.Loaded.Caves)
				{
					string caveName = cave.CaveGenerationParams.Name;
					int index = 1;
					for (;;)
					{
						List<string> list = locationNames;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
						defaultInterpolatedStringHandler.AppendFormatted(caveName);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(index);
						if (!list.Contains(defaultInterpolatedStringHandler.ToStringAndClear()))
						{
							break;
						}
						index++;
					}
					List<string> list2 = locationNames;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler2.AppendFormatted(caveName);
					defaultInterpolatedStringHandler2.AppendLiteral("_");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(index);
					list2.Add(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
			}
			if (Submarine.MainSub != null)
			{
				locationNames.Add("mainsub");
			}
			locationNames.Add("cursor");
			return locationNames.ToArray();
		}

		// Token: 0x06000340 RID: 832 RVA: 0x0001C5B4 File Offset: 0x0001A7B4
		private static string[] ListAvailableSubmarines()
		{
			List<string> submarineNames = new List<string>();
			foreach (Submarine submarine in from s in Submarine.Loaded
			where s.PhysicsBody.BodyType == BodyType.Dynamic
			select s)
			{
				submarineNames.Add(submarine.Info.Name + "_" + submarine.TeamID.ToString());
			}
			return submarineNames.ToArray();
		}

		// Token: 0x06000341 RID: 833 RVA: 0x0001C658 File Offset: 0x0001A858
		private static bool TryFindTeleportPosition(string locationName, out Vector2 teleportPosition)
		{
			Submarine mainSub = Submarine.MainSub;
			if (mainSub != null && string.Equals(locationName, "mainsub", StringComparison.InvariantCultureIgnoreCase))
			{
				WayPoint randomWaypoint = DebugConsole.<TryFindTeleportPosition>g__GetRandomWaypoint|42_0(mainSub.GetWaypoints(false));
				if (randomWaypoint != null)
				{
					teleportPosition = randomWaypoint.WorldPosition;
					return true;
				}
				DebugConsole.LogError("No waypoints found in the main sub!", null, null);
			}
			foreach (Submarine submarine in Submarine.Loaded)
			{
				if (string.Equals(submarine.Info.Name, locationName, StringComparison.InvariantCultureIgnoreCase))
				{
					WayPoint randomWaypoint2 = DebugConsole.<TryFindTeleportPosition>g__GetRandomWaypoint|42_0(submarine.GetWaypoints(false));
					if (randomWaypoint2 != null)
					{
						teleportPosition = randomWaypoint2.WorldPosition;
						return true;
					}
					DebugConsole.LogError("No waypoints found in sub " + submarine.Info.Name + "!", null, null);
				}
			}
			Level loadedLevel = Level.Loaded;
			if (loadedLevel != null)
			{
				ValueTuple<string, int> valueTuple = DebugConsole.<TryFindTeleportPosition>g__SplitIndex|42_1(locationName);
				string locationNameNoIndex = valueTuple.Item1;
				int locationIndex = valueTuple.Item2;
				int caveIndex = 1;
				foreach (Level.Cave cave in loadedLevel.Caves)
				{
					if (string.Equals(cave.CaveGenerationParams.Name, locationNameNoIndex, StringComparison.InvariantCultureIgnoreCase))
					{
						if (caveIndex != locationIndex)
						{
							caveIndex++;
						}
						else
						{
							WayPoint randomWaypoint3 = DebugConsole.<TryFindTeleportPosition>g__GetRandomWaypoint|42_0(cave.Tunnels.GetRandom(Rand.RandSync.Unsynced).WayPoints);
							if (randomWaypoint3 != null)
							{
								teleportPosition = randomWaypoint3.WorldPosition;
								return true;
							}
							DebugConsole.LogError("No waypoints found in cave " + cave.CaveGenerationParams.Name + "!", null, null);
						}
					}
				}
			}
			teleportPosition = Vector2.Zero;
			return false;
		}

		// Token: 0x06000342 RID: 834 RVA: 0x0001C848 File Offset: 0x0001AA48
		private static TFile GetSubmarineFile<TFile>(string submarineName) where TFile : BaseSubFile
		{
			List<TFile> submarineFiles = DebugConsole.GetContentFiles<TFile>();
			using (List<TFile>.Enumerator enumerator = submarineFiles.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TFile file = enumerator.Current;
					SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo i) => i.FilePath == file.Path.Value);
					if (matchingSub != null && string.Equals(matchingSub.Name, submarineName, StringComparison.InvariantCultureIgnoreCase))
					{
						return file;
					}
				}
			}
			return default(TFile);
		}

		// Token: 0x06000343 RID: 835 RVA: 0x0001C8E0 File Offset: 0x0001AAE0
		private static List<TFile> GetContentFiles<TFile>() where TFile : ContentFile
		{
			return ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<TFile>()).ToList<TFile>();
		}

		// Token: 0x06000344 RID: 836 RVA: 0x0001C920 File Offset: 0x0001AB20
		private static List<TFile> GetSubmarineFiles<TFile>() where TFile : BaseSubFile
		{
			return (from f in DebugConsole.GetContentFiles<TFile>()
			orderby f.UintIdentifier
			select f).ToList<TFile>();
		}

		// Token: 0x06000345 RID: 837 RVA: 0x0001C960 File Offset: 0x0001AB60
		private static ContentFile GetContentFile(string path)
		{
			List<ContentFile> contentFiles = DebugConsole.GetContentFiles<ContentFile>();
			return contentFiles.FirstOrDefault((ContentFile file) => string.Equals(file.Path.Value, path, StringComparison.InvariantCultureIgnoreCase));
		}

		// Token: 0x06000346 RID: 838 RVA: 0x0001C994 File Offset: 0x0001AB94
		private static string[] ListContentFilePaths()
		{
			List<string> contentFilePaths = new List<string>();
			List<ContentFile> contentFiles = DebugConsole.GetContentFiles<ContentFile>();
			foreach (ContentFile contentFile in contentFiles)
			{
				contentFilePaths.Add(contentFile.Path.Value);
			}
			return contentFilePaths.ToArray();
		}

		// Token: 0x06000347 RID: 839 RVA: 0x0001CA00 File Offset: 0x0001AC00
		private static string[] ListSubmarineFileNames<TFile>() where TFile : BaseSubFile
		{
			List<string> submarineFileNames = new List<string>();
			List<TFile> submarineFiles = DebugConsole.GetSubmarineFiles<TFile>();
			using (List<TFile>.Enumerator enumerator = submarineFiles.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TFile file = enumerator.Current;
					SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo i) => i.FilePath == file.Path.Value);
					if (matchingSub != null)
					{
						submarineFileNames.Add(matchingSub.Name);
					}
				}
			}
			return submarineFileNames.ToArray();
		}

		// Token: 0x06000348 RID: 840 RVA: 0x0001CA90 File Offset: 0x0001AC90
		private static IOrderedEnumerable<Character> SortSpawnedSpecies(IEnumerable<Character> characterList)
		{
			return from c in characterList
			orderby c.IsDead, c.IsHuman descending, c.Name
			select c;
		}

		// Token: 0x06000349 RID: 841 RVA: 0x0001CB0A File Offset: 0x0001AD0A
		private static string[] ListCharacterNames(bool includeMeArgument = false, bool includeCrewArgument = false)
		{
			return DebugConsole.GetCharacterNames(includeMeArgument, includeCrewArgument);
		}

		// Token: 0x0600034A RID: 842 RVA: 0x0001CB14 File Offset: 0x0001AD14
		private static string[] GetCharacterNames(bool includeMeArgument = false, bool includeCrewArgument = false)
		{
			List<string> characterNames = new List<string>();
			if (includeMeArgument)
			{
				characterNames.Add("/me");
			}
			if (includeCrewArgument)
			{
				characterNames.Add("/crew");
			}
			characterNames.AddRange(from c in DebugConsole.SortSpawnedSpecies(Character.CharacterList)
			select c.Name);
			return characterNames.ToArray();
		}

		// Token: 0x0600034B RID: 843 RVA: 0x0001CB7D File Offset: 0x0001AD7D
		private static string[] GetSpawnedSpeciesNames()
		{
			return (from c in DebugConsole.SortSpawnedSpecies(Character.CharacterList)
			select c.SpeciesName.Value).Distinct<string>().ToArray<string>();
		}

		// Token: 0x0600034C RID: 844 RVA: 0x0001CBB8 File Offset: 0x0001ADB8
		private static IEnumerable<Character> FindMatchingSpecies(string[] args)
		{
			if (args.Length == 0)
			{
				return Array.Empty<Character>();
			}
			string speciesName = args[0].ToLowerInvariant();
			return DebugConsole.FindMatchingSpecies(speciesName);
		}

		// Token: 0x0600034D RID: 845 RVA: 0x0001CBE0 File Offset: 0x0001ADE0
		private static IEnumerable<Character> FindMatchingSpecies(string speciesName)
		{
			return Character.CharacterList.FindAll((Character c) => c.SpeciesName.Value.Equals(speciesName, StringComparison.OrdinalIgnoreCase));
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0001CC10 File Offset: 0x0001AE10
		private static void HandleCommandForCrewOrSingleCharacter(string[] args, Action<Character> action, Client targetClient = null)
		{
			if (args.Length != 0 && args.First<string>() == "/crew")
			{
				using (ImmutableHashSet<Character>.Enumerator enumerator = GameSession.GetSessionCrewCharacters(CharacterType.Both).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character crewCharacter = enumerator.Current;
						action(crewCharacter);
					}
					return;
				}
			}
			Character targetCharacter = (args.Length == 0 || args.First<string>() == "/me") ? (((targetClient != null) ? targetClient.Character : null) ?? Character.Controlled) : DebugConsole.FindMatchingCharacter(args, false, null, false);
			if (targetCharacter == null)
			{
				return;
			}
			action(targetCharacter);
		}

		// Token: 0x0600034F RID: 847 RVA: 0x0001CCBC File Offset: 0x0001AEBC
		private static Character FindMatchingCharacter(string[] args, bool ignoreRemotePlayers = false, Client allowedRemotePlayer = null, bool botsOnly = false)
		{
			if (args.Length == 0)
			{
				return null;
			}
			List<Character> matchingCharacters = null;
			string characterName = null;
			int characterIndex = -1;
			foreach (string arg in args)
			{
				if (arg == "/me")
				{
					Client allowedRemotePlayer2 = allowedRemotePlayer;
					return ((allowedRemotePlayer2 != null) ? allowedRemotePlayer2.Character : null) ?? Character.Controlled;
				}
				int possibleIndex;
				if (matchingCharacters == null || matchingCharacters.None(null))
				{
					string possibleCharacterName = (arg != null) ? arg.ToLowerInvariant() : null;
					matchingCharacters = Character.CharacterList.FindAll(delegate(Character c)
					{
						if (!c.Name.Equals(possibleCharacterName, StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						if (c.IsRemotePlayer && ignoreRemotePlayers)
						{
							Client allowedRemotePlayer3 = allowedRemotePlayer;
							return ((allowedRemotePlayer3 != null) ? allowedRemotePlayer3.Character : null) == c;
						}
						return true;
					});
					if (botsOnly)
					{
						matchingCharacters = matchingCharacters.FindAll((Character c) => c is AICharacter);
					}
					if (matchingCharacters.Any<Character>())
					{
						characterName = possibleCharacterName;
					}
				}
				else if (characterName != null && int.TryParse(arg, out possibleIndex))
				{
					characterIndex = possibleIndex;
				}
			}
			if (matchingCharacters == null || matchingCharacters.None(null))
			{
				DebugConsole.NewMessage("No matching character found!", new Color?(Color.Red), false);
				return null;
			}
			matchingCharacters = (from c in matchingCharacters
			orderby c.IsDead, c.IsHuman descending
			select c).ToList<Character>();
			if (characterIndex == -1)
			{
				if (matchingCharacters.Count > 1)
				{
					DebugConsole.NewMessage("Found multiple matching characters. Use \"[charactername] [0-" + (matchingCharacters.Count - 1).ToString() + "]\" to choose a specific character.", new Color?(Color.LightGray), false);
				}
				return matchingCharacters[0];
			}
			if (characterIndex < 0 || characterIndex >= matchingCharacters.Count)
			{
				DebugConsole.ThrowError("Character index out of range. Select an index between 0 and " + (matchingCharacters.Count - 1).ToString(), null, null, false, false);
				return null;
			}
			return matchingCharacters[characterIndex];
		}

		// Token: 0x06000350 RID: 848 RVA: 0x0001CEC0 File Offset: 0x0001B0C0
		private static void TeleportCharacter(Vector2 cursorWorldPos, Character controlledCharacter, string[] args)
		{
			if (Screen.Selected != GameMain.GameScreen)
			{
				DebugConsole.NewMessage("Cannot teleport a character in the menu or the editor screens.", new Color?(Color.Yellow), false);
				return;
			}
			Character targetCharacter = controlledCharacter;
			string locationNameArgument = "";
			string text = args.FirstOrDefault<string>();
			string firstArgument = ((text != null) ? text.ToLowerInvariant() : null) ?? string.Empty;
			if (args.Length != 0)
			{
				string lastArgument = args.Last<string>();
				if (!(firstArgument == "/me") && !(firstArgument == "/crew"))
				{
					string[] availableLocations = DebugConsole.ListAvailableLocations();
					if (args.Length > 1 || availableLocations.None((string locationName) => string.Equals(locationName, lastArgument, StringComparison.OrdinalIgnoreCase)))
					{
						targetCharacter = DebugConsole.FindMatchingCharacter(args, false, null, false);
					}
				}
				int num;
				if (args.Count<string>() > 1 && (targetCharacter == null || (!targetCharacter.Name.Equals(lastArgument, StringComparison.OrdinalIgnoreCase) && !int.TryParse(lastArgument, out num))))
				{
					locationNameArgument = lastArgument;
				}
			}
			if (firstArgument == "/crew")
			{
				using (ImmutableHashSet<Character>.Enumerator enumerator = GameSession.GetSessionCrewCharacters(CharacterType.Both).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character crewCharacter = enumerator.Current;
						DebugConsole.TeleportSpecificCharacter(crewCharacter, locationNameArgument, cursorWorldPos);
					}
					return;
				}
			}
			DebugConsole.TeleportSpecificCharacter(targetCharacter, locationNameArgument, cursorWorldPos);
		}

		// Token: 0x06000351 RID: 849 RVA: 0x0001D01C File Offset: 0x0001B21C
		private static void TeleportSpecificCharacter(Character targetCharacter, string locationNameArgument, Vector2 defaultWorldPosition)
		{
			Vector2 worldPosition = defaultWorldPosition;
			if (!string.IsNullOrWhiteSpace(locationNameArgument) && !string.Equals(locationNameArgument, "cursor", StringComparison.InvariantCultureIgnoreCase))
			{
				Vector2 teleportPosition;
				if (!DebugConsole.TryFindTeleportPosition(locationNameArgument, out teleportPosition))
				{
					DebugConsole.ThrowError("No teleport position for location \"" + locationNameArgument + "\" was found.", null, null, false, false);
					return;
				}
				worldPosition = teleportPosition;
			}
			if (targetCharacter != null)
			{
				targetCharacter.TeleportTo(worldPosition);
				targetCharacter.AnimController.BodyInRest = false;
				return;
			}
			DebugConsole.NewMessage("Invalid arguments", new Color?(Color.Yellow), false);
		}

		// Token: 0x06000352 RID: 850 RVA: 0x0001D098 File Offset: 0x0001B298
		private static void SpawnCharacter(string[] args, Vector2 cursorWorldPos, bool usePreConfiguredNPC = false)
		{
			DebugConsole.<>c__DisplayClass59_0 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass59_0();
			CS$<>8__locals1.args = args;
			CS$<>8__locals1.cursorWorldPos = cursorWorldPos;
			CS$<>8__locals1.characterArgumentCount = 1;
			if (usePreConfiguredNPC)
			{
				CS$<>8__locals1.characterArgumentCount = 2;
			}
			if (CS$<>8__locals1.args.Length < CS$<>8__locals1.characterArgumentCount)
			{
				return;
			}
			for (int i = 0; i < CS$<>8__locals1.characterArgumentCount; i++)
			{
				if (string.IsNullOrWhiteSpace(CS$<>8__locals1.args[i]))
				{
					return;
				}
			}
			CS$<>8__locals1.job = null;
			CS$<>8__locals1.isHuman = true;
			if (!usePreConfiguredNPC)
			{
				string characterLowerCase = CS$<>8__locals1.args[0].ToLowerInvariant();
				if (!JobPrefab.Prefabs.ContainsKey(characterLowerCase))
				{
					CS$<>8__locals1.job = JobPrefab.Prefabs.Find((JobPrefab jp) => jp.Name != null && jp.Name.Equals(characterLowerCase, StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					CS$<>8__locals1.job = JobPrefab.Prefabs[characterLowerCase];
				}
				CS$<>8__locals1.isHuman = (CS$<>8__locals1.job != null || characterLowerCase == CharacterPrefab.HumanSpeciesName);
			}
			Vector2 spawnPosition;
			CS$<>8__locals1.<SpawnCharacter>g__ParseOptionalArgs|1(out spawnPosition, out CS$<>8__locals1.spawnPoint, out CS$<>8__locals1.teamType, out CS$<>8__locals1.addToCrew, out CS$<>8__locals1.renameCharacter);
			if (usePreConfiguredNPC)
			{
				DebugConsole.<>c__DisplayClass59_2 CS$<>8__locals3 = new DebugConsole.<>c__DisplayClass59_2();
				CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals1;
				Identifier npcSetIdentifier = CS$<>8__locals3.CS$<>8__locals1.args[0].ToIdentifier();
				CS$<>8__locals3.humanPrefabIdentifier = CS$<>8__locals3.CS$<>8__locals1.args[1].ToIdentifier();
				DebugConsole.<>c__DisplayClass59_2 CS$<>8__locals4 = CS$<>8__locals3;
				HumanPrefab humanPrefab;
				if (!(npcSetIdentifier == "any"))
				{
					humanPrefab = NPCSet.Get(npcSetIdentifier, CS$<>8__locals3.humanPrefabIdentifier, true, null);
				}
				else
				{
					humanPrefab = NPCSet.Sets.SelectMany((NPCSet set) => set.Humans).FirstOrDefault((HumanPrefab human) => human.Identifier == CS$<>8__locals3.humanPrefabIdentifier);
				}
				CS$<>8__locals4.humanPrefab = humanPrefab;
				if (CS$<>8__locals3.humanPrefab != null)
				{
					Entity.Spawner.AddCharacterToSpawnQueue(CharacterPrefab.HumanSpeciesName, spawnPosition, CS$<>8__locals3.humanPrefab.CreateCharacterInfo(Rand.RandSync.Unsynced), delegate(Character newCharacter)
					{
						newCharacter.HumanPrefab = CS$<>8__locals3.humanPrefab;
						CS$<>8__locals3.CS$<>8__locals1.<SpawnCharacter>g__SetTeamAndCrew|0(newCharacter);
						CS$<>8__locals3.humanPrefab.GiveItems(newCharacter, newCharacter.Submarine, CS$<>8__locals3.CS$<>8__locals1.spawnPoint, Rand.RandSync.Unsynced, true);
						CS$<>8__locals3.humanPrefab.InitializeCharacter(newCharacter, null);
						newCharacter.LoadTalents();
						GameMain.NetworkMember.CreateEntityEvent(newCharacter, default(Character.UpdateTalentsEventData));
					});
					return;
				}
			}
			else
			{
				if (CS$<>8__locals1.isHuman)
				{
					int variant = (CS$<>8__locals1.job != null) ? Rand.Range(0, CS$<>8__locals1.job.Variants, Rand.RandSync.ServerAndClient) : 0;
					CharacterInfo characterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", CS$<>8__locals1.job, variant, Rand.RandSync.Unsynced, default(Identifier));
					Entity.Spawner.AddCharacterToSpawnQueue(CharacterPrefab.HumanSpeciesName, spawnPosition, characterInfo, delegate(Character newCharacter)
					{
						if (CS$<>8__locals1.renameCharacter != null)
						{
							if (CS$<>8__locals1.renameCharacter.Length > 31)
							{
								CS$<>8__locals1.renameCharacter = CS$<>8__locals1.renameCharacter.Substring(0, 32);
							}
							newCharacter.Info.Name = CS$<>8__locals1.renameCharacter;
						}
						base.<SpawnCharacter>g__SetTeamAndCrew|0(newCharacter);
						GameSession gameSession = GameMain.GameSession;
						newCharacter.GiveJobItems(((gameSession != null) ? gameSession.GameMode : null) is PvPMode, CS$<>8__locals1.spawnPoint);
						newCharacter.GiveIdCardTags(CS$<>8__locals1.spawnPoint, false);
						newCharacter.Info.StartItemsGiven = true;
					});
					return;
				}
				CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(CS$<>8__locals1.args[0].ToIdentifier());
				if (prefab != null)
				{
					Entity.Spawner.AddCharacterToSpawnQueue(CS$<>8__locals1.args[0].ToIdentifier(), spawnPosition, prefab.HasCharacterInfo ? new CharacterInfo(prefab.Identifier, "", "", null, 0, Rand.RandSync.Unsynced, default(Identifier)) : null, new Action<Character>(CS$<>8__locals1.<SpawnCharacter>g__SetTeamAndCrew|0));
				}
			}
		}

		// Token: 0x06000353 RID: 851 RVA: 0x0001D36C File Offset: 0x0001B56C
		private static IEnumerable<string> GetSpawnPosParams()
		{
			return new DebugConsole.<GetSpawnPosParams>d__60(-2);
		}

		// Token: 0x06000354 RID: 852 RVA: 0x0001D375 File Offset: 0x0001B575
		private static IEnumerable<string> GetItemNameOrIdParams()
		{
			return new DebugConsole.<GetItemNameOrIdParams>d__61(-2);
		}

		// Token: 0x06000355 RID: 853 RVA: 0x0001D380 File Offset: 0x0001B580
		private static void TrySpawnItem(string[] args)
		{
			try
			{
				string errorMsg;
				DebugConsole.SpawnItem(args, Vector2.Zero, null, out errorMsg);
				if (!string.IsNullOrWhiteSpace(errorMsg))
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
			}
			catch (Exception e)
			{
				string errorMsg2 = "Failed to spawn an item. Arguments: \"" + string.Join(" ", args) + "\".";
				DebugConsole.ThrowError(errorMsg2, e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("DebugConsole.SpawnItem:Error", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
				{
					errorMsg2,
					"\n",
					e.Message,
					"\n",
					e.StackTrace.CleanupStackTrace()
				}));
			}
		}

		// Token: 0x06000356 RID: 854 RVA: 0x0001D428 File Offset: 0x0001B628
		public static void SpawnItem(string[] args, Vector2 cursorPos, Character controlledCharacter, out string errorMsg)
		{
			DebugConsole.<>c__DisplayClass64_0 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass64_0();
			CS$<>8__locals1.args = args;
			errorMsg = "";
			if (CS$<>8__locals1.args.Length < 1)
			{
				return;
			}
			Vector2? spawnPos = null;
			Inventory spawnInventory = null;
			CS$<>8__locals1.itemNameOrId = CS$<>8__locals1.args[0].ToLowerInvariant();
			ItemPrefab itemPrefab = (MapEntityPrefab.FindByName(CS$<>8__locals1.itemNameOrId) ?? MapEntityPrefab.FindByIdentifier(CS$<>8__locals1.itemNameOrId.ToIdentifier())) as ItemPrefab;
			if (itemPrefab == null)
			{
				errorMsg = "Item \"" + CS$<>8__locals1.itemNameOrId + "\" not found!";
				ItemPrefab matching = ItemPrefab.Prefabs.Find((ItemPrefab me) => me.Name.StartsWith(CS$<>8__locals1.itemNameOrId, StringComparison.OrdinalIgnoreCase) && me != null);
				if (matching != null)
				{
					string str = errorMsg;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
					defaultInterpolatedStringHandler.AppendLiteral(" Did you mean \"");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(matching.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\"?");
					errorMsg = str + defaultInterpolatedStringHandler.ToStringAndClear();
					if (matching.Name.Contains(" ", StringComparison.Ordinal))
					{
						string str2 = errorMsg;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(95, 1);
						defaultInterpolatedStringHandler2.AppendLiteral(" Please note that you should surround multi-word names with quotation marks (e.q. spawnitem \"");
						defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(matching.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("\")");
						errorMsg = str2 + defaultInterpolatedStringHandler2.ToStringAndClear();
					}
				}
				return;
			}
			int amount = 1;
			CS$<>8__locals1.conditionPrc = 100;
			CS$<>8__locals1.itemQuality = 0;
			string spawnLocation;
			int spawnLocationIndex;
			if (CS$<>8__locals1.<SpawnItem>g__TryGetSpawnPosParam|0(out spawnLocation, out spawnLocationIndex))
			{
				if (!(spawnLocation == "cursor"))
				{
					if (!(spawnLocation == "inventory"))
					{
						if (!(spawnLocation == "cargo"))
						{
							if (!(spawnLocation == "random"))
							{
								Character matchingCharacter = DebugConsole.FindMatchingCharacter(CS$<>8__locals1.args.Skip(1).Take(1).ToArray<string>(), false, null, false);
								if (matchingCharacter != null)
								{
									spawnInventory = matchingCharacter.Inventory;
								}
							}
						}
						else
						{
							WayPoint wp = WayPoint.GetRandom(SpawnType.Cargo, null, Submarine.MainSub, false, null, false);
							spawnPos = new Vector2?((wp == null) ? Vector2.Zero : wp.WorldPosition);
						}
					}
					else
					{
						spawnInventory = ((controlledCharacter != null) ? controlledCharacter.Inventory : null);
					}
				}
				else
				{
					spawnPos = new Vector2?(cursorPos);
				}
				if (CS$<>8__locals1.args.Length > spawnLocationIndex + 1)
				{
					if (!int.TryParse(CS$<>8__locals1.args[spawnLocationIndex + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
					{
						amount = 1;
					}
					amount = Math.Min(amount, 100);
				}
				if (CS$<>8__locals1.args.Length > spawnLocationIndex + 2 && !int.TryParse(CS$<>8__locals1.args[spawnLocationIndex + 2], NumberStyles.Any, CultureInfo.InvariantCulture, out CS$<>8__locals1.conditionPrc))
				{
					CS$<>8__locals1.conditionPrc = 100;
				}
				if (CS$<>8__locals1.args.Length > spawnLocationIndex + 3)
				{
					for (int i = 0; i <= 3; i++)
					{
						if (CS$<>8__locals1.args[spawnLocationIndex + 3].ToLowerInvariant() == DebugConsole.ItemQualityNames[i])
						{
							CS$<>8__locals1.itemQuality = i;
						}
					}
				}
			}
			float itemCondition = itemPrefab.Health * Math.Clamp((float)CS$<>8__locals1.conditionPrc / 100f, 0f, 1f);
			if ((spawnPos == null || spawnPos == Vector2.Zero) && spawnInventory == null)
			{
				WayPoint wp2 = WayPoint.GetRandom(SpawnType.Human, null, Submarine.MainSub, false, null, false);
				spawnPos = new Vector2?((wp2 == null) ? Vector2.Zero : wp2.WorldPosition);
			}
			for (int j = 0; j < amount; j++)
			{
				if (spawnPos != null)
				{
					if (Entity.Spawner == null || Entity.Spawner.Removed)
					{
						new Item(itemPrefab, spawnPos.Value, null, 0, true);
					}
					else
					{
						EntitySpawner spawner = Entity.Spawner;
						if (spawner != null)
						{
							spawner.AddItemToSpawnQueue(itemPrefab, spawnPos.Value, new float?(itemCondition), new int?(CS$<>8__locals1.itemQuality), null);
						}
					}
				}
				else if (spawnInventory != null)
				{
					if (Entity.Spawner == null)
					{
						Item spawnedItem = new Item(itemPrefab, Vector2.Zero, null, 0, true);
						spawnInventory.TryPutItem(spawnedItem, null, spawnedItem.AllowedSlots, true, false, true);
						CS$<>8__locals1.<SpawnItem>g__onItemSpawned|3(spawnedItem);
					}
					else
					{
						EntitySpawner spawner2 = Entity.Spawner;
						if (spawner2 != null)
						{
							ItemPrefab itemPrefab2 = itemPrefab;
							Inventory inventory = spawnInventory;
							Action<Item> onSpawned = new Action<Item>(CS$<>8__locals1.<SpawnItem>g__onItemSpawned|3);
							spawner2.AddItemToSpawnQueue(itemPrefab2, inventory, null, null, onSpawned, true, false, InvSlotType.None);
						}
					}
				}
			}
		}

		// Token: 0x06000357 RID: 855 RVA: 0x0001D85C File Offset: 0x0001BA5C
		public static void AddSafeError(string error)
		{
			DebugConsole.LogError(error, null, null);
		}

		// Token: 0x06000358 RID: 856 RVA: 0x0001D87C File Offset: 0x0001BA7C
		public static void LogError(string msg, Color? color = null, ContentPackage contentPackage = null)
		{
			msg = DebugConsole.AddContentPackageInfoToMessage(msg, contentPackage);
			Color value = color.GetValueOrDefault();
			if (color == null)
			{
				value = Color.Red;
				color = new Color?(value);
			}
			DebugConsole.NewMessage(msg, color.Value, false, true);
		}

		// Token: 0x06000359 RID: 857 RVA: 0x0001D8C0 File Offset: 0x0001BAC0
		public static void NewCommand(string command, Color? color = null)
		{
			Color value = color.GetValueOrDefault();
			if (color == null)
			{
				value = Color.White;
				color = new Color?(value);
			}
			DebugConsole.NewMessage(command, color.Value, true, false);
		}

		// Token: 0x0600035A RID: 858 RVA: 0x0001D8FB File Offset: 0x0001BAFB
		public static void NewMessage(LocalizedString msg, Color? color = null, bool debugOnly = false)
		{
			DebugConsole.NewMessage(msg.Value, color, debugOnly);
		}

		// Token: 0x0600035B RID: 859 RVA: 0x0001D90C File Offset: 0x0001BB0C
		public static void NewMessage(string msg, Color? color = null, bool debugOnly = false)
		{
			Color value = color.GetValueOrDefault();
			if (color == null)
			{
				value = Color.White;
				color = new Color?(value);
			}
			if (!debugOnly)
			{
				DebugConsole.NewMessage(msg, color.Value, false, false);
			}
		}

		// Token: 0x0600035C RID: 860 RVA: 0x0001D94C File Offset: 0x0001BB4C
		private static void NewMessage(string msg, Color color, bool isCommand, bool isError)
		{
			if (string.IsNullOrEmpty(msg))
			{
				return;
			}
			ColoredText newMsg = new ColoredText(msg, color, isCommand, isError);
			DebugConsole.queuedMessages.Enqueue(newMsg);
			DebugConsole.MessageHandler.Invoke(newMsg);
		}

		// Token: 0x0600035D RID: 861 RVA: 0x0001D984 File Offset: 0x0001BB84
		public static void ShowQuestionPrompt(string question, DebugConsole.QuestionCallback onAnswered, string[] args = null, int argCount = -1)
		{
			if (args != null && args.Length > argCount)
			{
				onAnswered(args[argCount]);
				return;
			}
			DebugConsole.NewMessage("   >>" + question, new Color?(Color.Cyan), false);
			DebugConsole.activeQuestionCallback = (DebugConsole.QuestionCallback)Delegate.Combine(DebugConsole.activeQuestionCallback, onAnswered);
		}

		// Token: 0x0600035E RID: 862 RVA: 0x0001D9D4 File Offset: 0x0001BBD4
		private static bool TryParseTimeSpan(string s, out TimeSpan timeSpan)
		{
			timeSpan = default(TimeSpan);
			if (string.IsNullOrWhiteSpace(s))
			{
				return false;
			}
			string currNum = "";
			foreach (char c in s)
			{
				if (char.IsDigit(c))
				{
					ReadOnlySpan<char> str = currNum;
					char c2 = c;
					currNum = str + new ReadOnlySpan<char>(ref c2);
				}
				else if (!char.IsWhiteSpace(c))
				{
					int parsedNum;
					if (!int.TryParse(currNum, out parsedNum) || parsedNum < 0)
					{
						return false;
					}
					try
					{
						if (c <= 'h')
						{
							if (c == 'd')
							{
								timeSpan += new TimeSpan(parsedNum, 0, 0, 0, 0);
								goto IL_108;
							}
							if (c == 'h')
							{
								timeSpan += new TimeSpan(0, parsedNum, 0, 0, 0);
								goto IL_108;
							}
						}
						else
						{
							if (c == 'm')
							{
								timeSpan += new TimeSpan(0, 0, parsedNum, 0, 0);
								goto IL_108;
							}
							if (c == 's')
							{
								timeSpan += new TimeSpan(0, 0, 0, parsedNum, 0);
								goto IL_108;
							}
						}
						return false;
						IL_108:;
					}
					catch (ArgumentOutOfRangeException)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 3);
						defaultInterpolatedStringHandler.AppendFormatted<int>(parsedNum);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<char>(c);
						defaultInterpolatedStringHandler.AppendLiteral(" exceeds the maximum supported time span. Using the maximum time span ");
						defaultInterpolatedStringHandler.AppendFormatted<TimeSpan>(TimeSpan.MaxValue);
						defaultInterpolatedStringHandler.AppendLiteral(" instead.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						timeSpan = TimeSpan.MaxValue;
						return true;
					}
					currNum = "";
				}
			}
			return true;
		}

		// Token: 0x0600035F RID: 863 RVA: 0x0001DB84 File Offset: 0x0001BD84
		public static DebugConsole.Command FindCommand(string commandName)
		{
			return DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(commandName.ToIdentifier()));
		}

		// Token: 0x06000360 RID: 864 RVA: 0x0001DBB4 File Offset: 0x0001BDB4
		public static void Log(LocalizedString message)
		{
			DebugConsole.Log((message != null) ? message.Value : null);
		}

		// Token: 0x06000361 RID: 865 RVA: 0x0001DBC7 File Offset: 0x0001BDC7
		public static void Log(string message)
		{
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage(message, new Color?(Color.Gray), false);
			}
		}

		// Token: 0x06000362 RID: 866 RVA: 0x0001DBE6 File Offset: 0x0001BDE6
		public static void ThrowErrorLocalized(LocalizedString error, Exception e = null, ContentPackage contentPackage = null, bool createMessageBox = false, bool appendStackTrace = false)
		{
			DebugConsole.ThrowError(error.Value, e, contentPackage, createMessageBox, appendStackTrace);
		}

		// Token: 0x06000363 RID: 867 RVA: 0x0001DBF8 File Offset: 0x0001BDF8
		public static void ThrowError(string error, Exception e = null, ContentPackage contentPackage = null, bool createMessageBox = false, bool appendStackTrace = false)
		{
			error = DebugConsole.AddContentPackageInfoToMessage(error, contentPackage);
			if (e != null)
			{
				error = error + " {" + e.Message + "}\n";
				if (e.StackTrace != null)
				{
					error += e.StackTrace.CleanupStackTrace();
				}
				if (e.InnerException != null)
				{
					Exception innermost = e.GetInnermost();
					error = error + "\n\nInner exception: " + innermost.Message + "\n";
					if (innermost.StackTrace != null)
					{
						error += innermost.StackTrace.CleanupStackTrace();
					}
				}
			}
			else if (appendStackTrace && Environment.StackTrace != null)
			{
				error = error + "\n" + Environment.StackTrace.CleanupStackTrace();
			}
			DebugConsole.LogError(error, null, null);
		}

		// Token: 0x06000364 RID: 868 RVA: 0x0001DCBA File Offset: 0x0001BEBA
		public static void ThrowErrorAndLogToGA(string gaIdentifier, string errorMsg)
		{
			DebugConsole.ThrowError(errorMsg, null, null, false, false);
			GameAnalyticsManager.AddErrorEventOnce(gaIdentifier, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
		}

		// Token: 0x06000365 RID: 869 RVA: 0x0001DCCE File Offset: 0x0001BECE
		public static void ThrowErrorOnce(string identifier, string errorMsg, Exception e = null)
		{
			if (DebugConsole.loggedErrorIdentifiers.Contains(identifier))
			{
				return;
			}
			DebugConsole.ThrowError(errorMsg, e, null, false, false);
			DebugConsole.loggedErrorIdentifiers.Add(identifier);
		}

		// Token: 0x06000366 RID: 870 RVA: 0x0001DCF4 File Offset: 0x0001BEF4
		public static void AddWarning(string warning, ContentPackage contentPackage = null)
		{
			warning = DebugConsole.AddContentPackageInfoToMessage("WARNING: " + warning, contentPackage);
			DebugConsole.NewMessage(warning, new Color?(Color.Yellow), false);
		}

		// Token: 0x06000367 RID: 871 RVA: 0x0001DD1A File Offset: 0x0001BF1A
		private static string AddContentPackageInfoToMessage(string message, ContentPackage contentPackage)
		{
			if (contentPackage == null)
			{
				return message;
			}
			return "[" + contentPackage.Name + "] " + message;
		}

		// Token: 0x06000368 RID: 872 RVA: 0x0001DD38 File Offset: 0x0001BF38
		public static void SaveLogs()
		{
			if (DebugConsole.unsavedMessages.Count == 0)
			{
				return;
			}
			if (!Directory.Exists("ConsoleLogs"))
			{
				try
				{
					Directory.CreateDirectory("ConsoleLogs", false);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to create a folder for debug console logs", e, null, false, false);
					return;
				}
			}
			string fileName = "DebugConsoleLog_";
			fileName += "Server_";
			fileName = fileName + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString();
			ImmutableHashSet<char> invalidChars = Path.GetInvalidFileNameCharsCrossPlatform();
			foreach (char invalidChar in invalidChars)
			{
				fileName = fileName.Replace(invalidChar.ToString(), "");
			}
			string filePath = Path.Combine(new string[]
			{
				"ConsoleLogs",
				fileName
			});
			if (File.Exists(filePath + ".txt"))
			{
				int fileNum = 2;
				while (File.Exists(filePath + " (" + fileNum.ToString() + ")"))
				{
					fileNum++;
				}
				filePath = filePath + " (" + fileNum.ToString() + ")";
			}
			try
			{
				File.WriteAllLines(filePath + ".txt", from l in DebugConsole.unsavedMessages
				select "[" + l.Time + "] " + l.Text, null, false);
			}
			catch (Exception e2)
			{
				DebugConsole.unsavedMessages.Clear();
				DebugConsole.ThrowError("Saving debug console log to " + filePath + " failed", e2, null, false, false);
			}
		}

		// Token: 0x06000369 RID: 873 RVA: 0x0001DEF8 File Offset: 0x0001C0F8
		private static void ToggleEnemyAITargetingRestrictions(EnemyTargetingRestrictions restrictions)
		{
			if (restrictions == EnemyTargetingRestrictions.None)
			{
				EnemyAIController.TargetingRestrictions = EnemyTargetingRestrictions.None;
			}
			else if (EnemyAIController.TargetingRestrictions.HasFlag(restrictions))
			{
				EnemyAIController.TargetingRestrictions &= ~restrictions;
			}
			else
			{
				EnemyAIController.TargetingRestrictions |= restrictions;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Monster targeting restrictions is now '");
			defaultInterpolatedStringHandler.AppendFormatted<EnemyTargetingRestrictions>(EnemyAIController.TargetingRestrictions);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Yellow), false);
		}

		// Token: 0x0600036A RID: 874 RVA: 0x0001DF87 File Offset: 0x0001C187
		public static void DeactivateCheats()
		{
			Hull.EditWater = false;
			Hull.EditFire = false;
			EnemyAIController.DisableEnemyAI = false;
			HumanAIController.DisableCrewAI = false;
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0001DFA4 File Offset: 0x0001C1A4
		[CompilerGenerated]
		internal static void <InitProjectSpecific>g__CreateTraitorList|13_45(Action<string> createMessage)
		{
			if (GameMain.Server == null)
			{
				return;
			}
			TraitorManager traitorManager = GameMain.Server.TraitorManager;
			if (traitorManager == null || traitorManager.ActiveEvents.None(null))
			{
				createMessage("There are no traitors at the moment.");
				return;
			}
			createMessage("Traitors:");
			foreach (TraitorManager.ActiveTraitorEvent ev in traitorManager.ActiveEvents)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 3);
				defaultInterpolatedStringHandler.AppendLiteral(" - ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(ev.TraitorEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<TraitorEvent.State>(ev.TraitorEvent.CurrentState);
				defaultInterpolatedStringHandler.AppendLiteral("): ");
				defaultInterpolatedStringHandler.AppendFormatted(ev.Traitor.Name);
				string msg = defaultInterpolatedStringHandler.ToStringAndClear();
				if (ev.TraitorEvent.SecondaryTraitors.Any<Client>())
				{
					msg = msg + " secondary traitors: " + string.Join(", ", from t in ev.TraitorEvent.SecondaryTraitors
					select t.Name);
				}
				createMessage(msg);
			}
		}

		// Token: 0x0600036C RID: 876 RVA: 0x0001E100 File Offset: 0x0001C300
		[CompilerGenerated]
		internal static void <.cctor>g__printMapEntityPrefabs|35_2<T>(IEnumerable<T> prefabs) where T : MapEntityPrefab
		{
			DebugConsole.NewMessage("***************", new Color?(Color.Cyan), false);
			foreach (T prefab in prefabs)
			{
				if (!prefab.Name.IsNullOrEmpty())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
					defaultInterpolatedStringHandler.AppendLiteral("- ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(prefab.Name);
					string text = defaultInterpolatedStringHandler.ToStringAndClear();
					if (prefab.Tags.Any<Identifier>())
					{
						text = text + " (" + string.Join<Identifier>(", ", prefab.Tags) + ")";
					}
					ImmutableHashSet<Identifier> allowedLinks = prefab.AllowedLinks;
					if (allowedLinks != null && allowedLinks.Any<Identifier>())
					{
						text = text + ", Links: " + string.Join<Identifier>(", ", prefab.AllowedLinks);
					}
					DebugConsole.NewMessage(text, new Color?((prefab.ContentPackage == ContentPackageManager.VanillaCorePackage) ? Color.Cyan : Color.Purple), false);
				}
			}
			DebugConsole.NewMessage("***************", new Color?(Color.Cyan), false);
		}

		// Token: 0x0600036D RID: 877 RVA: 0x0001E250 File Offset: 0x0001C450
		[CompilerGenerated]
		internal static WayPoint <TryFindTeleportPosition>g__GetRandomWaypoint|42_0(IReadOnlyList<WayPoint> waypoints)
		{
			if (waypoints.None(null))
			{
				return null;
			}
			if (waypoints.Any((WayPoint point) => point.SpawnType == SpawnType.Human))
			{
				return waypoints.GetRandom((WayPoint point) => point.SpawnType == SpawnType.Human, Rand.RandSync.Unsynced);
			}
			if (waypoints.Any((WayPoint point) => point.SpawnType == SpawnType.Path))
			{
				return waypoints.GetRandom((WayPoint point) => point.SpawnType == SpawnType.Path, Rand.RandSync.Unsynced);
			}
			return waypoints.GetRandom(Rand.RandSync.Unsynced);
		}

		// Token: 0x0600036E RID: 878 RVA: 0x0001E30C File Offset: 0x0001C50C
		[CompilerGenerated]
		internal static ValueTuple<string, int> <TryFindTeleportPosition>g__SplitIndex|42_1(string caveName)
		{
			string[] splitName = caveName.Split('_', StringSplitOptions.None);
			if (splitName.Length == 1)
			{
				return new ValueTuple<string, int>(splitName[0], -1);
			}
			return new ValueTuple<string, int>(splitName[0], int.Parse(splitName[1]));
		}

		// Token: 0x04000180 RID: 384
		private static readonly RateLimiter rateLimiter = new RateLimiter(50, 5, new ValueTuple<RateLimitAction, RateLimitPunishment>[]
		{
			new ValueTuple<RateLimitAction, RateLimitPunishment>(RateLimitAction.OnLimitReached, RateLimitPunishment.Announce),
			new ValueTuple<RateLimitAction, RateLimitPunishment>(RateLimitAction.OnLimitDoubled, RateLimitPunishment.Kick)
		});

		// Token: 0x04000181 RID: 385
		public static List<string> QueuedCommands = new List<string>();

		// Token: 0x04000182 RID: 386
		private static string input = "";

		// Token: 0x04000183 RID: 387
		private static int memoryIndex = -1;

		// Token: 0x04000184 RID: 388
		private static List<string> commandMemory = new List<string>();

		// Token: 0x04000185 RID: 389
		private static readonly ConcurrentQueue<ColoredText> queuedMessages = new ConcurrentQueue<ColoredText>();

		// Token: 0x04000186 RID: 390
		public static readonly NamedEvent<ColoredText> MessageHandler = new NamedEvent<ColoredText>();

		// Token: 0x04000187 RID: 391
		private const int MaxMessages = 300;

		// Token: 0x04000188 RID: 392
		public static readonly List<ColoredText> Messages = new List<ColoredText>();

		// Token: 0x04000189 RID: 393
		private static DebugConsole.QuestionCallback activeQuestionCallback;

		// Token: 0x0400018A RID: 394
		private static readonly List<DebugConsole.Command> commands = new List<DebugConsole.Command>();

		// Token: 0x0400018B RID: 395
		private static string currentAutoCompletedCommand;

		// Token: 0x0400018C RID: 396
		private static int currentAutoCompletedIndex;

		// Token: 0x0400018D RID: 397
		public static bool CheatsEnabled;

		// Token: 0x0400018E RID: 398
		private static readonly List<ColoredText> unsavedMessages = new List<ColoredText>();

		// Token: 0x0400018F RID: 399
		private static readonly int messagesPerFile = 800;

		// Token: 0x04000190 RID: 400
		public const string SavePath = "ConsoleLogs";

		// Token: 0x04000191 RID: 401
		private static WeakReference<Character> previousControlledCharacter;

		// Token: 0x04000192 RID: 402
		private static ImmutableArray<string> ItemQualityNames = ImmutableCollectionsMarshal.AsImmutableArray<string>(new string[]
		{
			"normal",
			"good",
			"excellent",
			"masterwork"
		});

		// Token: 0x04000193 RID: 403
		private static readonly HashSet<string> loggedErrorIdentifiers = new HashSet<string>();

		// Token: 0x02000555 RID: 1365
		public class Command
		{
			// Token: 0x06004966 RID: 18790 RVA: 0x001CF834 File Offset: 0x001CDA34
			public void ServerExecuteOnClientRequest(Client client, Vector2 cursorWorldPos, string[] args)
			{
				if (!DebugConsole.CheatsEnabled && this.IsCheat)
				{
					DebugConsole.NewMessage(string.Concat(new string[]
					{
						"Client \"",
						client.Name,
						"\" attempted to use the command \"",
						this.Names[0].ToString(),
						"\". Cheats must be enabled using \"enablecheats\" before the command can be used."
					}), new Color?(Color.Red), false);
					GameMain.Server.SendConsoleMessage("You need to enable cheats using the command \"enablecheats\" before you can use the command \"" + this.Names[0].ToString() + "\".", client, new Color?(Color.Red));
					DebugConsole.NewMessage("Enabling cheats will disable Steam achievements during this play session.", new Color?(Color.Red), false);
					GameMain.Server.SendConsoleMessage("Enabling cheats will disable Steam achievements during this play session.", client, new Color?(Color.Red));
					return;
				}
				if (this.OnClientRequestExecute != null)
				{
					this.OnClientRequestExecute(client, cursorWorldPos, args);
					return;
				}
				if (this.OnExecute == null)
				{
					return;
				}
				this.OnExecute(args);
			}

			// Token: 0x06004967 RID: 18791 RVA: 0x001CF94C File Offset: 0x001CDB4C
			public Command(string name, string help, Action<string[]> onExecute, Func<string[][]> getValidArgs = null, bool isCheat = false)
			{
				this.Names = name.Split('|', StringSplitOptions.None).ToIdentifiers().ToImmutableArray<Identifier>();
				this.Help = help;
				this.OnExecute = onExecute;
				this.GetValidArgs = getValidArgs;
				this.IsCheat = isCheat;
			}

			// Token: 0x06004968 RID: 18792 RVA: 0x001CF99C File Offset: 0x001CDB9C
			public void Execute(string[] args)
			{
				if (this.OnExecute == null)
				{
					return;
				}
				if (!false && !DebugConsole.CheatsEnabled && this.IsCheat)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 1);
					defaultInterpolatedStringHandler.AppendLiteral("You need to enable cheats using the command \"enablecheats\" before you can use the command \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Names.First<Identifier>());
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
					DebugConsole.NewMessage("Enabling cheats will disable Steam achievements during this play session.", new Color?(Color.Red), false);
					return;
				}
				this.OnExecute(args);
			}

			// Token: 0x06004969 RID: 18793 RVA: 0x001CFA34 File Offset: 0x001CDC34
			public override int GetHashCode()
			{
				return this.Names.First<Identifier>().GetHashCode();
			}

			// Token: 0x040025AB RID: 9643
			public Action<Client, Vector2, string[]> OnClientRequestExecute;

			// Token: 0x040025AC RID: 9644
			public readonly ImmutableArray<Identifier> Names;

			// Token: 0x040025AD RID: 9645
			public readonly string Help;

			// Token: 0x040025AE RID: 9646
			public Action<string[]> OnExecute;

			// Token: 0x040025AF RID: 9647
			public Func<string[][]> GetValidArgs;

			// Token: 0x040025B0 RID: 9648
			public readonly bool IsCheat;
		}

		// Token: 0x02000556 RID: 1366
		public struct ErrorCatcher : IDisposable
		{
			// Token: 0x170013B8 RID: 5048
			// (get) Token: 0x0600496A RID: 18794 RVA: 0x001CFA5A File Offset: 0x001CDC5A
			public IReadOnlyList<ColoredText> Errors
			{
				get
				{
					return this.errors;
				}
			}

			// Token: 0x0600496B RID: 18795 RVA: 0x001CFA64 File Offset: 0x001CDC64
			private ErrorCatcher(Identifier handlerId)
			{
				this.handlerId = handlerId;
				this.wasConsoleOpen = false;
				this.errors = new List<ColoredText>();
				List<ColoredText> errs = this.errors;
				DebugConsole.MessageHandler.Register(handlerId, delegate(ColoredText msg)
				{
					if (!msg.IsError)
					{
						return;
					}
					errs.Add(msg);
				});
			}

			// Token: 0x0600496C RID: 18796 RVA: 0x001CFAB3 File Offset: 0x001CDCB3
			public static DebugConsole.ErrorCatcher Create()
			{
				return new DebugConsole.ErrorCatcher(ToolBox.RandomSeed(25).ToIdentifier());
			}

			// Token: 0x0600496D RID: 18797 RVA: 0x001CFAC6 File Offset: 0x001CDCC6
			public void Dispose()
			{
				if (this.handlerId.IsEmpty)
				{
					return;
				}
				DebugConsole.MessageHandler.Deregister(this.handlerId);
				this.handlerId = Identifier.Empty;
			}

			// Token: 0x040025B1 RID: 9649
			private readonly List<ColoredText> errors;

			// Token: 0x040025B2 RID: 9650
			private readonly bool wasConsoleOpen;

			// Token: 0x040025B3 RID: 9651
			private Identifier handlerId;
		}

		// Token: 0x02000557 RID: 1367
		// (Invoke) Token: 0x0600496F RID: 18799
		public delegate void QuestionCallback(string answer);
	}
}
