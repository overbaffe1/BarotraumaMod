using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using FluentResults;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000421 RID: 1057
	internal class LuaGame : IReusableService, IService, IDisposable
	{
		// Token: 0x17000FCC RID: 4044
		// (get) Token: 0x06003B97 RID: 15255 RVA: 0x0018A15A File Offset: 0x0018835A
		public bool IsSingleplayer
		{
			get
			{
				return GameMain.IsSingleplayer;
			}
		}

		// Token: 0x17000FCD RID: 4045
		// (get) Token: 0x06003B98 RID: 15256 RVA: 0x0018A161 File Offset: 0x00188361
		public bool IsMultiplayer
		{
			get
			{
				return GameMain.IsMultiplayer;
			}
		}

		// Token: 0x17000FCE RID: 4046
		// (get) Token: 0x06003B99 RID: 15257 RVA: 0x0018A168 File Offset: 0x00188368
		public string SaveFolder
		{
			get
			{
				if (!string.IsNullOrEmpty(GameSettings.CurrentConfig.SavePath))
				{
					return GameSettings.CurrentConfig.SavePath;
				}
				return SaveUtil.DefaultSaveFolder;
			}
		}

		// Token: 0x17000FCF RID: 4047
		// (get) Token: 0x06003B9A RID: 15258 RVA: 0x0018A18B File Offset: 0x0018838B
		public GameServer Server
		{
			get
			{
				return GameMain.Server;
			}
		}

		// Token: 0x17000FD0 RID: 4048
		// (get) Token: 0x06003B9B RID: 15259 RVA: 0x0018A192 File Offset: 0x00188392
		public bool IsDedicated
		{
			get
			{
				return GameMain.Server.ServerPeer is LidgrenServerPeer;
			}
		}

		// Token: 0x17000FD1 RID: 4049
		// (get) Token: 0x06003B9C RID: 15260 RVA: 0x0018A1A6 File Offset: 0x001883A6
		public bool Paused
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000FD2 RID: 4050
		// (get) Token: 0x06003B9D RID: 15261 RVA: 0x0018A1A9 File Offset: 0x001883A9
		public ServerSettings ServerSettings
		{
			get
			{
				return GameMain.Server.ServerSettings;
			}
		}

		// Token: 0x17000FD3 RID: 4051
		// (get) Token: 0x06003B9E RID: 15262 RVA: 0x0018A1B5 File Offset: 0x001883B5
		public RespawnManager RespawnManager
		{
			get
			{
				return GameMain.Server.RespawnManager;
			}
		}

		// Token: 0x17000FD4 RID: 4052
		// (get) Token: 0x06003B9F RID: 15263 RVA: 0x0018A1C1 File Offset: 0x001883C1
		public List<DebugConsole.Command> Commands
		{
			get
			{
				return DebugConsole.Commands;
			}
		}

		// Token: 0x17000FD5 RID: 4053
		// (get) Token: 0x06003BA0 RID: 15264 RVA: 0x0018A1C8 File Offset: 0x001883C8
		// (set) Token: 0x06003BA1 RID: 15265 RVA: 0x0018A1CF File Offset: 0x001883CF
		public int MapEntityUpdateInterval
		{
			get
			{
				return MapEntity.MapEntityUpdateInterval;
			}
			set
			{
				MapEntity.MapEntityUpdateInterval = value;
			}
		}

		// Token: 0x17000FD6 RID: 4054
		// (get) Token: 0x06003BA2 RID: 15266 RVA: 0x0018A1D7 File Offset: 0x001883D7
		// (set) Token: 0x06003BA3 RID: 15267 RVA: 0x0018A1DA File Offset: 0x001883DA
		public int GapUpdateInterval
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		// Token: 0x17000FD7 RID: 4055
		// (get) Token: 0x06003BA4 RID: 15268 RVA: 0x0018A1DC File Offset: 0x001883DC
		// (set) Token: 0x06003BA5 RID: 15269 RVA: 0x0018A1E3 File Offset: 0x001883E3
		public int PoweredUpdateInterval
		{
			get
			{
				return MapEntity.PoweredUpdateInterval;
			}
			set
			{
				MapEntity.PoweredUpdateInterval = value;
			}
		}

		// Token: 0x17000FD8 RID: 4056
		// (get) Token: 0x06003BA6 RID: 15270 RVA: 0x0018A1EB File Offset: 0x001883EB
		// (set) Token: 0x06003BA7 RID: 15271 RVA: 0x0018A1F2 File Offset: 0x001883F2
		public int CharacterUpdateInterval
		{
			get
			{
				return Character.CharacterUpdateInterval;
			}
			set
			{
				Character.CharacterUpdateInterval = value;
			}
		}

		// Token: 0x06003BA8 RID: 15272 RVA: 0x0018A1FA File Offset: 0x001883FA
		public void AddPriorityItem(Item item)
		{
			this.UpdatePriorityItems.Add(item);
		}

		// Token: 0x06003BA9 RID: 15273 RVA: 0x0018A209 File Offset: 0x00188409
		public void RemovePriorityItem(Item item)
		{
			this.UpdatePriorityItems.Remove(item);
		}

		// Token: 0x06003BAA RID: 15274 RVA: 0x0018A218 File Offset: 0x00188418
		public void ClearPriorityItem()
		{
			this.UpdatePriorityItems.Clear();
		}

		// Token: 0x06003BAB RID: 15275 RVA: 0x0018A225 File Offset: 0x00188425
		public void AddPriorityCharacter(Character character)
		{
			this.UpdatePriorityCharacters.Add(character);
		}

		// Token: 0x06003BAC RID: 15276 RVA: 0x0018A234 File Offset: 0x00188434
		public void RemovePriorityCharacter(Character character)
		{
			this.UpdatePriorityCharacters.Remove(character);
		}

		// Token: 0x06003BAD RID: 15277 RVA: 0x0018A243 File Offset: 0x00188443
		public void ClearPriorityCharacter()
		{
			this.UpdatePriorityCharacters.Clear();
		}

		// Token: 0x17000FD9 RID: 4057
		// (get) Token: 0x06003BAE RID: 15278 RVA: 0x0018A250 File Offset: 0x00188450
		public bool RoundStarted
		{
			get
			{
				if (GameMain.IsSingleplayer)
				{
					return GameMain.GameSession != null && GameMain.GameSession.IsRunning;
				}
				GameServer server = GameMain.Server;
				return server != null && server.GameStarted;
			}
		}

		// Token: 0x17000FDA RID: 4058
		// (get) Token: 0x06003BAF RID: 15279 RVA: 0x0018A27D File Offset: 0x0018847D
		public GameSession GameSession
		{
			get
			{
				return GameMain.GameSession;
			}
		}

		// Token: 0x17000FDB RID: 4059
		// (get) Token: 0x06003BB0 RID: 15280 RVA: 0x0018A284 File Offset: 0x00188484
		public NetLobbyScreen NetLobbyScreen
		{
			get
			{
				return GameMain.NetLobbyScreen;
			}
		}

		// Token: 0x17000FDC RID: 4060
		// (get) Token: 0x06003BB1 RID: 15281 RVA: 0x0018A28B File Offset: 0x0018848B
		public GameScreen GameScreen
		{
			get
			{
				return GameMain.GameScreen;
			}
		}

		// Token: 0x17000FDD RID: 4061
		// (get) Token: 0x06003BB2 RID: 15282 RVA: 0x0018A292 File Offset: 0x00188492
		public World World
		{
			get
			{
				return GameMain.World;
			}
		}

		// Token: 0x17000FDE RID: 4062
		// (get) Token: 0x06003BB3 RID: 15283 RVA: 0x0018A299 File Offset: 0x00188499
		public ServerPeer Peer
		{
			get
			{
				return GameMain.Server.ServerPeer;
			}
		}

		// Token: 0x06003BB4 RID: 15284 RVA: 0x0018A2A8 File Offset: 0x001884A8
		public LuaGame(IConsoleCommandsService consoleCommands)
		{
			UserData.RegisterType(typeof(GameSettings), InteropAccessMode.Default, null);
			this.Settings = UserData.CreateStatic(typeof(GameSettings));
			this._consoleCommands = consoleCommands;
		}

		// Token: 0x06003BB5 RID: 15285 RVA: 0x0018A2FF File Offset: 0x001884FF
		public void OverrideTraitors(bool o)
		{
			this.overrideTraitors = o;
		}

		// Token: 0x06003BB6 RID: 15286 RVA: 0x0018A308 File Offset: 0x00188508
		public void OverrideRespawnSub(bool o)
		{
			this.overrideRespawnSub = o;
		}

		// Token: 0x06003BB7 RID: 15287 RVA: 0x0018A311 File Offset: 0x00188511
		public void AllowWifiChat(bool o)
		{
			this.allowWifiChat = o;
		}

		// Token: 0x06003BB8 RID: 15288 RVA: 0x0018A31A File Offset: 0x0018851A
		public void OverrideSignalRadio(bool o)
		{
			this.overrideSignalRadio = o;
		}

		// Token: 0x06003BB9 RID: 15289 RVA: 0x0018A323 File Offset: 0x00188523
		public void DisableSpamFilter(bool o)
		{
			this.disableSpamFilter = o;
		}

		// Token: 0x06003BBA RID: 15290 RVA: 0x0018A32C File Offset: 0x0018852C
		public void DisableDisconnectCharacter(bool o)
		{
			this.disableDisconnectCharacter = o;
		}

		// Token: 0x06003BBB RID: 15291 RVA: 0x0018A335 File Offset: 0x00188535
		public void EnableControlHusk(bool o)
		{
			this.enableControlHusk = o;
		}

		// Token: 0x06003BBC RID: 15292 RVA: 0x0018A33E File Offset: 0x0018853E
		public static void Explode(Vector2 pos, float range = 100f, float force = 30f, float damage = 30f, float structureDamage = 30f, float itemDamage = 30f, float empStrength = 0f, float ballastFloraStrength = 0f)
		{
			new Explosion(range, force, damage, structureDamage, itemDamage, empStrength, ballastFloraStrength).Explode(pos, null, null);
		}

		// Token: 0x06003BBD RID: 15293 RVA: 0x0018A358 File Offset: 0x00188558
		public static string SpawnItem(string name, Vector2 pos, bool inventory = false, Character character = null)
		{
			string error;
			DebugConsole.SpawnItem(new string[]
			{
				name,
				inventory ? "inventory" : "cursor"
			}, pos, character, out error);
			return error;
		}

		// Token: 0x06003BBE RID: 15294 RVA: 0x0018A38B File Offset: 0x0018858B
		public static ContentPackage[] GetEnabledContentPackages()
		{
			return ContentPackageManager.EnabledPackages.All.ToArray<ContentPackage>();
		}

		// Token: 0x06003BBF RID: 15295 RVA: 0x0018A398 File Offset: 0x00188598
		public static ItemPrefab GetItemPrefab(string itemNameOrId)
		{
			return (MapEntityPrefab.Find(itemNameOrId, null, false) ?? MapEntityPrefab.Find(null, itemNameOrId, false)) as ItemPrefab;
		}

		// Token: 0x06003BC0 RID: 15296 RVA: 0x0018A3C0 File Offset: 0x001885C0
		public static Submarine GetRespawnSub()
		{
			if (GameMain.Server.RespawnManager == null)
			{
				return null;
			}
			return GameMain.Server.RespawnManager.GetShuttle(CharacterTeamType.Team1);
		}

		// Token: 0x06003BC1 RID: 15297 RVA: 0x0018A3E0 File Offset: 0x001885E0
		public static Steering GetSubmarineSteering(Submarine sub)
		{
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine == sub)
				{
					Steering steering = item.GetComponent<Steering>();
					if (steering != null)
					{
						return steering;
					}
				}
			}
			return null;
		}

		// Token: 0x06003BC2 RID: 15298 RVA: 0x0018A448 File Offset: 0x00188648
		public static WifiComponent GetWifiComponent(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<WifiComponent>();
		}

		// Token: 0x06003BC3 RID: 15299 RVA: 0x0018A455 File Offset: 0x00188655
		public static LightComponent GetLightComponent(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<LightComponent>();
		}

		// Token: 0x06003BC4 RID: 15300 RVA: 0x0018A462 File Offset: 0x00188662
		public static CustomInterface GetCustomInterface(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<CustomInterface>();
		}

		// Token: 0x06003BC5 RID: 15301 RVA: 0x0018A46F File Offset: 0x0018866F
		public static Fabricator GetFabricatorComponent(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<Fabricator>();
		}

		// Token: 0x06003BC6 RID: 15302 RVA: 0x0018A47C File Offset: 0x0018867C
		public static Holdable GetHoldableComponent(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<Holdable>();
		}

		// Token: 0x06003BC7 RID: 15303 RVA: 0x0018A489 File Offset: 0x00188689
		public static void ExecuteCommand(string command)
		{
			DebugConsole.ExecuteCommand(command);
		}

		// Token: 0x06003BC8 RID: 15304 RVA: 0x0018A491 File Offset: 0x00188691
		public static Signal CreateSignal(string value, int stepsTaken = 1, Character sender = null, Item source = null, float power = 0f, float strength = 1f)
		{
			return new Signal(value, stepsTaken, sender, source, power, strength);
		}

		// Token: 0x06003BC9 RID: 15305 RVA: 0x0018A4A0 File Offset: 0x001886A0
		public void RemoveCommand(string name)
		{
			this._consoleCommands.RemoveCommand(name);
			for (int i = DebugConsole.Commands.Count - 1; i >= 0; i--)
			{
				foreach (Identifier cmdname in DebugConsole.Commands[i].Names)
				{
					if (cmdname == name)
					{
						DebugConsole.Commands.RemoveAt(i);
					}
				}
			}
		}

		// Token: 0x06003BCA RID: 15306 RVA: 0x0018A510 File Offset: 0x00188710
		public void AddCommand(string name, string help, LuaCsAction onExecute, LuaCsFunc getValidArgs = null, bool isCheat = false)
		{
			this._consoleCommands.RegisterCommand(name, help, delegate(string[] args)
			{
				onExecute(new object[]
				{
					args
				});
			}, delegate
			{
				if (getValidArgs == null)
				{
					return null;
				}
				object validArgs = getValidArgs(Array.Empty<object>());
				DynValue luaValue = validArgs as DynValue;
				if (luaValue != null)
				{
					return luaValue.ToObject<string[][]>();
				}
				return (string[][])validArgs;
			}, false);
		}

		// Token: 0x06003BCB RID: 15307 RVA: 0x0018A558 File Offset: 0x00188758
		public void AddCommand(string name, LuaCsAction onExecute, LuaCsFunc getValidArgs = null, bool isCheat = false)
		{
			this._consoleCommands.RegisterCommand(name, "", delegate(string[] args)
			{
				onExecute(new object[]
				{
					args
				});
			}, delegate
			{
				if (getValidArgs == null)
				{
					return null;
				}
				object validArgs = getValidArgs(Array.Empty<object>());
				DynValue luaValue = validArgs as DynValue;
				if (luaValue != null)
				{
					return luaValue.ToObject<string[][]>();
				}
				return (string[][])validArgs;
			}, false);
		}

		// Token: 0x17000FDF RID: 4063
		// (get) Token: 0x06003BCC RID: 15308 RVA: 0x0018A5A3 File Offset: 0x001887A3
		public bool IsDisposed
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x06003BCD RID: 15309 RVA: 0x0018A5AC File Offset: 0x001887AC
		public void AssignOnExecute(string names, object onExecute)
		{
			DebugConsole.AssignOnExecute(names, delegate(string[] args)
			{
				LuaCsSetup.Instance.LuaScriptManagementService.CallFunctionSafe(onExecute, new object[]
				{
					args
				});
			});
		}

		// Token: 0x06003BCE RID: 15310 RVA: 0x0018A5D8 File Offset: 0x001887D8
		public void SaveGame(string path)
		{
			if (!LuaCsFile.CanWriteToPath(path))
			{
				throw new ScriptRuntimeException("Saving files to " + path + " is disallowed.");
			}
			SaveUtil.SaveGame(CampaignDataPath.CreateRegular(path), false);
		}

		// Token: 0x06003BCF RID: 15311 RVA: 0x0018A604 File Offset: 0x00188804
		public void LoadGame(string path)
		{
			SaveUtil.LoadGame(CampaignDataPath.CreateRegular(path));
		}

		// Token: 0x06003BD0 RID: 15312 RVA: 0x0018A611 File Offset: 0x00188811
		public void LoadCampaign(string path, Client client = null)
		{
			MultiPlayerCampaign.LoadCampaign(CampaignDataPath.CreateRegular(path), client);
		}

		// Token: 0x06003BD1 RID: 15313 RVA: 0x0018A61F File Offset: 0x0018881F
		public static void SendMessage(string msg, ChatMessageType? messageType = null, Client sender = null, Character character = null)
		{
			GameMain.Server.SendChatMessage(msg, messageType, sender, character, PlayerConnectionChangeType.None, ChatMode.None);
		}

		// Token: 0x06003BD2 RID: 15314 RVA: 0x0018A631 File Offset: 0x00188831
		public static void SendTraitorMessage(WriteOnlyMessage message, Client client)
		{
			GameMain.Server.SendTraitorMessage(message, client);
		}

		// Token: 0x06003BD3 RID: 15315 RVA: 0x0018A640 File Offset: 0x00188840
		public static void SendDirectChatMessage(string sendername, string text, Character sender, ChatMessageType messageType = ChatMessageType.Private, Client client = null, string iconStyle = "")
		{
			ChatMessage cm = ChatMessage.Create(sendername, text, messageType, sender, null, PlayerConnectionChangeType.None, null);
			cm.IconStyle = iconStyle;
			GameMain.Server.SendDirectChatMessage(cm, client);
		}

		// Token: 0x06003BD4 RID: 15316 RVA: 0x0018A677 File Offset: 0x00188877
		public static void SendDirectChatMessage(ChatMessage chatMessage, Client client)
		{
			GameMain.Server.SendDirectChatMessage(chatMessage, client);
		}

		// Token: 0x06003BD5 RID: 15317 RVA: 0x0018A685 File Offset: 0x00188885
		public static void Log(string message, ServerLog.MessageType type)
		{
			GameServer.Log(message, type);
		}

		// Token: 0x06003BD6 RID: 15318 RVA: 0x0018A68E File Offset: 0x0018888E
		public static void DispatchRespawnSub()
		{
			GameMain.Server.RespawnManager.DispatchShuttle(GameMain.Server.RespawnManager.GetTeamSpecificState(CharacterTeamType.Team1));
		}

		// Token: 0x06003BD7 RID: 15319 RVA: 0x0018A6AF File Offset: 0x001888AF
		public static GameServer.TryStartGameResult StartGame()
		{
			return GameMain.Server.TryStartGame();
		}

		// Token: 0x06003BD8 RID: 15320 RVA: 0x0018A6BB File Offset: 0x001888BB
		public static void EndGame()
		{
			GameMain.Server.EndGame(CampaignMode.TransitionType.None, false, null);
		}

		// Token: 0x06003BD9 RID: 15321 RVA: 0x0018A6CC File Offset: 0x001888CC
		public void AssignOnClientRequestExecute(string names, LuaCsAction onExecute)
		{
			this._consoleCommands.AssignOnClientRequestExecute(names, delegate(Client client, Vector2 position, string[] args)
			{
				onExecute(new object[]
				{
					client,
					position,
					args
				});
			});
		}

		// Token: 0x06003BDA RID: 15322 RVA: 0x0018A6FE File Offset: 0x001888FE
		public void Stop()
		{
			this.MapEntityUpdateInterval = 1;
			this.CharacterUpdateInterval = 1;
			this._consoleCommands.RemoveRegisteredCommands();
		}

		// Token: 0x06003BDB RID: 15323 RVA: 0x0018A719 File Offset: 0x00188919
		public Result Reset()
		{
			this.Stop();
			return Result.Ok();
		}

		// Token: 0x06003BDC RID: 15324 RVA: 0x0018A726 File Offset: 0x00188926
		public void Dispose()
		{
			this.Stop();
		}

		// Token: 0x04001D66 RID: 7526
		public bool? ForceVoice;

		// Token: 0x04001D67 RID: 7527
		public bool? ForceLocalVoice;

		// Token: 0x04001D68 RID: 7528
		public DynValue Settings;

		// Token: 0x04001D69 RID: 7529
		public bool allowWifiChat;

		// Token: 0x04001D6A RID: 7530
		public bool overrideTraitors;

		// Token: 0x04001D6B RID: 7531
		public bool overrideRespawnSub;

		// Token: 0x04001D6C RID: 7532
		public bool overrideSignalRadio;

		// Token: 0x04001D6D RID: 7533
		public bool disableSpamFilter;

		// Token: 0x04001D6E RID: 7534
		public bool disableDisconnectCharacter;

		// Token: 0x04001D6F RID: 7535
		public bool enableControlHusk;

		// Token: 0x04001D70 RID: 7536
		public HashSet<Item> UpdatePriorityItems = new HashSet<Item>();

		// Token: 0x04001D71 RID: 7537
		public HashSet<Character> UpdatePriorityCharacters = new HashSet<Character>();

		// Token: 0x04001D72 RID: 7538
		private readonly IConsoleCommandsService _consoleCommands;
	}
}
