using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using FarseerPhysics.Dynamics;
using FluentResults;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000534 RID: 1332
	internal class LuaGame : IReusableService, IService, IDisposable
	{
		// Token: 0x17001513 RID: 5395
		// (get) Token: 0x060054B3 RID: 21683 RVA: 0x002CE99B File Offset: 0x002CCB9B
		public bool IsSingleplayer
		{
			get
			{
				return GameMain.IsSingleplayer;
			}
		}

		// Token: 0x17001514 RID: 5396
		// (get) Token: 0x060054B4 RID: 21684 RVA: 0x002CE9A2 File Offset: 0x002CCBA2
		public bool IsMultiplayer
		{
			get
			{
				return GameMain.IsMultiplayer;
			}
		}

		// Token: 0x17001515 RID: 5397
		// (get) Token: 0x060054B5 RID: 21685 RVA: 0x002CE9A9 File Offset: 0x002CCBA9
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

		// Token: 0x17001516 RID: 5398
		// (get) Token: 0x060054B6 RID: 21686 RVA: 0x002CE9CC File Offset: 0x002CCBCC
		public GameClient Client
		{
			get
			{
				return GameMain.Client;
			}
		}

		// Token: 0x17001517 RID: 5399
		// (get) Token: 0x060054B7 RID: 21687 RVA: 0x002CE9D3 File Offset: 0x002CCBD3
		public bool Paused
		{
			get
			{
				GameMain instance = GameMain.Instance;
				return instance != null && instance.Paused;
			}
		}

		// Token: 0x17001518 RID: 5400
		// (get) Token: 0x060054B8 RID: 21688 RVA: 0x002CE9E5 File Offset: 0x002CCBE5
		public byte SessionId
		{
			get
			{
				return GameMain.Client.SessionId;
			}
		}

		// Token: 0x17001519 RID: 5401
		// (get) Token: 0x060054B9 RID: 21689 RVA: 0x002CE9F1 File Offset: 0x002CCBF1
		public byte MyID
		{
			get
			{
				return this.SessionId;
			}
		}

		// Token: 0x1700151A RID: 5402
		// (get) Token: 0x060054BA RID: 21690 RVA: 0x002CE9F9 File Offset: 0x002CCBF9
		public ChatMode ActiveChatMode
		{
			get
			{
				return GameMain.ActiveChatMode;
			}
		}

		// Token: 0x1700151B RID: 5403
		// (get) Token: 0x060054BB RID: 21691 RVA: 0x002CEA00 File Offset: 0x002CCC00
		public ChatBox ChatBox
		{
			get
			{
				if (GameMain.IsSingleplayer)
				{
					return GameMain.GameSession.CrewManager.ChatBox;
				}
				return GameMain.Client.ChatBox;
			}
		}

		// Token: 0x1700151C RID: 5404
		// (get) Token: 0x060054BC RID: 21692 RVA: 0x002CEA23 File Offset: 0x002CCC23
		public SoundManager SoundManager
		{
			get
			{
				return GameMain.SoundManager;
			}
		}

		// Token: 0x1700151D RID: 5405
		// (get) Token: 0x060054BD RID: 21693 RVA: 0x002CEA2A File Offset: 0x002CCC2A
		public LightManager LightManager
		{
			get
			{
				return GameMain.LightManager;
			}
		}

		// Token: 0x1700151E RID: 5406
		// (get) Token: 0x060054BE RID: 21694 RVA: 0x002CEA31 File Offset: 0x002CCC31
		public SubEditorScreen SubEditorScreen
		{
			get
			{
				return GameMain.SubEditorScreen;
			}
		}

		// Token: 0x1700151F RID: 5407
		// (get) Token: 0x060054BF RID: 21695 RVA: 0x002CEA38 File Offset: 0x002CCC38
		public MainMenuScreen MainMenuScreen
		{
			get
			{
				return GameMain.MainMenuScreen;
			}
		}

		// Token: 0x17001520 RID: 5408
		// (get) Token: 0x060054C0 RID: 21696 RVA: 0x002CEA3F File Offset: 0x002CCC3F
		public ParticleManager ParticleManager
		{
			get
			{
				return GameMain.ParticleManager;
			}
		}

		// Token: 0x17001521 RID: 5409
		// (get) Token: 0x060054C1 RID: 21697 RVA: 0x002CEA46 File Offset: 0x002CCC46
		public bool IsSubEditor
		{
			get
			{
				return Screen.Selected is SubEditorScreen;
			}
		}

		// Token: 0x17001522 RID: 5410
		// (get) Token: 0x060054C2 RID: 21698 RVA: 0x002CEA55 File Offset: 0x002CCC55
		public ServerSettings ServerSettings
		{
			get
			{
				return GameMain.Client.ServerSettings;
			}
		}

		// Token: 0x17001523 RID: 5411
		// (get) Token: 0x060054C3 RID: 21699 RVA: 0x002CEA61 File Offset: 0x002CCC61
		public RespawnManager RespawnManager
		{
			get
			{
				return GameMain.Client.RespawnManager;
			}
		}

		// Token: 0x17001524 RID: 5412
		// (get) Token: 0x060054C4 RID: 21700 RVA: 0x002CEA6D File Offset: 0x002CCC6D
		public List<DebugConsole.Command> Commands
		{
			get
			{
				return DebugConsole.Commands;
			}
		}

		// Token: 0x17001525 RID: 5413
		// (get) Token: 0x060054C5 RID: 21701 RVA: 0x002CEA74 File Offset: 0x002CCC74
		// (set) Token: 0x060054C6 RID: 21702 RVA: 0x002CEA7B File Offset: 0x002CCC7B
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

		// Token: 0x17001526 RID: 5414
		// (get) Token: 0x060054C7 RID: 21703 RVA: 0x002CEA83 File Offset: 0x002CCC83
		// (set) Token: 0x060054C8 RID: 21704 RVA: 0x002CEA86 File Offset: 0x002CCC86
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

		// Token: 0x17001527 RID: 5415
		// (get) Token: 0x060054C9 RID: 21705 RVA: 0x002CEA88 File Offset: 0x002CCC88
		// (set) Token: 0x060054CA RID: 21706 RVA: 0x002CEA8F File Offset: 0x002CCC8F
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

		// Token: 0x17001528 RID: 5416
		// (get) Token: 0x060054CB RID: 21707 RVA: 0x002CEA97 File Offset: 0x002CCC97
		// (set) Token: 0x060054CC RID: 21708 RVA: 0x002CEA9E File Offset: 0x002CCC9E
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

		// Token: 0x060054CD RID: 21709 RVA: 0x002CEAA6 File Offset: 0x002CCCA6
		public void AddPriorityItem(Item item)
		{
			this.UpdatePriorityItems.Add(item);
		}

		// Token: 0x060054CE RID: 21710 RVA: 0x002CEAB5 File Offset: 0x002CCCB5
		public void RemovePriorityItem(Item item)
		{
			this.UpdatePriorityItems.Remove(item);
		}

		// Token: 0x060054CF RID: 21711 RVA: 0x002CEAC4 File Offset: 0x002CCCC4
		public void ClearPriorityItem()
		{
			this.UpdatePriorityItems.Clear();
		}

		// Token: 0x060054D0 RID: 21712 RVA: 0x002CEAD1 File Offset: 0x002CCCD1
		public void AddPriorityCharacter(Character character)
		{
			this.UpdatePriorityCharacters.Add(character);
		}

		// Token: 0x060054D1 RID: 21713 RVA: 0x002CEAE0 File Offset: 0x002CCCE0
		public void RemovePriorityCharacter(Character character)
		{
			this.UpdatePriorityCharacters.Remove(character);
		}

		// Token: 0x060054D2 RID: 21714 RVA: 0x002CEAEF File Offset: 0x002CCCEF
		public void ClearPriorityCharacter()
		{
			this.UpdatePriorityCharacters.Clear();
		}

		// Token: 0x17001529 RID: 5417
		// (get) Token: 0x060054D3 RID: 21715 RVA: 0x002CEAFC File Offset: 0x002CCCFC
		public bool RoundStarted
		{
			get
			{
				if (GameMain.IsSingleplayer)
				{
					return GameMain.GameSession != null && GameMain.GameSession.IsRunning;
				}
				GameClient client = GameMain.Client;
				return client != null && client.GameStarted;
			}
		}

		// Token: 0x1700152A RID: 5418
		// (get) Token: 0x060054D4 RID: 21716 RVA: 0x002CEB29 File Offset: 0x002CCD29
		public GameSession GameSession
		{
			get
			{
				return GameMain.GameSession;
			}
		}

		// Token: 0x1700152B RID: 5419
		// (get) Token: 0x060054D5 RID: 21717 RVA: 0x002CEB30 File Offset: 0x002CCD30
		public NetLobbyScreen NetLobbyScreen
		{
			get
			{
				return GameMain.NetLobbyScreen;
			}
		}

		// Token: 0x1700152C RID: 5420
		// (get) Token: 0x060054D6 RID: 21718 RVA: 0x002CEB37 File Offset: 0x002CCD37
		public GameScreen GameScreen
		{
			get
			{
				return GameMain.GameScreen;
			}
		}

		// Token: 0x1700152D RID: 5421
		// (get) Token: 0x060054D7 RID: 21719 RVA: 0x002CEB3E File Offset: 0x002CCD3E
		public World World
		{
			get
			{
				return GameMain.World;
			}
		}

		// Token: 0x1700152E RID: 5422
		// (get) Token: 0x060054D8 RID: 21720 RVA: 0x002CEB45 File Offset: 0x002CCD45
		public ClientPeer Peer
		{
			get
			{
				return GameMain.Client.ClientPeer;
			}
		}

		// Token: 0x060054D9 RID: 21721 RVA: 0x002CEB54 File Offset: 0x002CCD54
		public LuaGame(IConsoleCommandsService consoleCommands)
		{
			UserData.RegisterType(typeof(GameSettings), InteropAccessMode.Default, null);
			this.Settings = UserData.CreateStatic(typeof(GameSettings));
			this._consoleCommands = consoleCommands;
		}

		// Token: 0x060054DA RID: 21722 RVA: 0x002CEBAB File Offset: 0x002CCDAB
		public void OverrideTraitors(bool o)
		{
			this.overrideTraitors = o;
		}

		// Token: 0x060054DB RID: 21723 RVA: 0x002CEBB4 File Offset: 0x002CCDB4
		public void OverrideRespawnSub(bool o)
		{
			this.overrideRespawnSub = o;
		}

		// Token: 0x060054DC RID: 21724 RVA: 0x002CEBBD File Offset: 0x002CCDBD
		public void AllowWifiChat(bool o)
		{
			this.allowWifiChat = o;
		}

		// Token: 0x060054DD RID: 21725 RVA: 0x002CEBC6 File Offset: 0x002CCDC6
		public void OverrideSignalRadio(bool o)
		{
			this.overrideSignalRadio = o;
		}

		// Token: 0x060054DE RID: 21726 RVA: 0x002CEBCF File Offset: 0x002CCDCF
		public void DisableSpamFilter(bool o)
		{
			this.disableSpamFilter = o;
		}

		// Token: 0x060054DF RID: 21727 RVA: 0x002CEBD8 File Offset: 0x002CCDD8
		public void DisableDisconnectCharacter(bool o)
		{
			this.disableDisconnectCharacter = o;
		}

		// Token: 0x060054E0 RID: 21728 RVA: 0x002CEBE1 File Offset: 0x002CCDE1
		public void EnableControlHusk(bool o)
		{
			this.enableControlHusk = o;
		}

		// Token: 0x060054E1 RID: 21729 RVA: 0x002CEBEA File Offset: 0x002CCDEA
		public static void Explode(Vector2 pos, float range = 100f, float force = 30f, float damage = 30f, float structureDamage = 30f, float itemDamage = 30f, float empStrength = 0f, float ballastFloraStrength = 0f)
		{
			new Explosion(range, force, damage, structureDamage, itemDamage, empStrength, ballastFloraStrength).Explode(pos, null, null);
		}

		// Token: 0x060054E2 RID: 21730 RVA: 0x002CEC04 File Offset: 0x002CCE04
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

		// Token: 0x060054E3 RID: 21731 RVA: 0x002CEC37 File Offset: 0x002CCE37
		public static ContentPackage[] GetEnabledContentPackages()
		{
			return ContentPackageManager.EnabledPackages.All.ToArray<ContentPackage>();
		}

		// Token: 0x060054E4 RID: 21732 RVA: 0x002CEC44 File Offset: 0x002CCE44
		public static ItemPrefab GetItemPrefab(string itemNameOrId)
		{
			return (MapEntityPrefab.Find(itemNameOrId, null, false) ?? MapEntityPrefab.Find(null, itemNameOrId, false)) as ItemPrefab;
		}

		// Token: 0x060054E5 RID: 21733 RVA: 0x002CEC6C File Offset: 0x002CCE6C
		public static Submarine GetRespawnSub()
		{
			if (GameMain.Client.RespawnManager == null)
			{
				return null;
			}
			return GameMain.Client.RespawnManager.GetShuttle(CharacterTeamType.Team1);
		}

		// Token: 0x060054E6 RID: 21734 RVA: 0x002CEC8C File Offset: 0x002CCE8C
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

		// Token: 0x060054E7 RID: 21735 RVA: 0x002CECF4 File Offset: 0x002CCEF4
		public static WifiComponent GetWifiComponent(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<WifiComponent>();
		}

		// Token: 0x060054E8 RID: 21736 RVA: 0x002CED01 File Offset: 0x002CCF01
		public static LightComponent GetLightComponent(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<LightComponent>();
		}

		// Token: 0x060054E9 RID: 21737 RVA: 0x002CED0E File Offset: 0x002CCF0E
		public static CustomInterface GetCustomInterface(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<CustomInterface>();
		}

		// Token: 0x060054EA RID: 21738 RVA: 0x002CED1B File Offset: 0x002CCF1B
		public static Fabricator GetFabricatorComponent(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<Fabricator>();
		}

		// Token: 0x060054EB RID: 21739 RVA: 0x002CED28 File Offset: 0x002CCF28
		public static Holdable GetHoldableComponent(Item item)
		{
			if (item == null)
			{
				return null;
			}
			return item.GetComponent<Holdable>();
		}

		// Token: 0x060054EC RID: 21740 RVA: 0x002CED35 File Offset: 0x002CCF35
		public static void ExecuteCommand(string command)
		{
			DebugConsole.ExecuteCommand(command);
		}

		// Token: 0x060054ED RID: 21741 RVA: 0x002CED3D File Offset: 0x002CCF3D
		public static Signal CreateSignal(string value, int stepsTaken = 1, Character sender = null, Item source = null, float power = 0f, float strength = 1f)
		{
			return new Signal(value, stepsTaken, sender, source, power, strength);
		}

		// Token: 0x060054EE RID: 21742 RVA: 0x002CED4C File Offset: 0x002CCF4C
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

		// Token: 0x060054EF RID: 21743 RVA: 0x002CEDBC File Offset: 0x002CCFBC
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

		// Token: 0x060054F0 RID: 21744 RVA: 0x002CEE04 File Offset: 0x002CD004
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

		// Token: 0x1700152F RID: 5423
		// (get) Token: 0x060054F1 RID: 21745 RVA: 0x002CEE4F File Offset: 0x002CD04F
		public bool IsDisposed
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x060054F2 RID: 21746 RVA: 0x002CEE58 File Offset: 0x002CD058
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

		// Token: 0x060054F3 RID: 21747 RVA: 0x002CEE84 File Offset: 0x002CD084
		public void SaveGame(string path)
		{
			if (!LuaCsFile.CanWriteToPath(path))
			{
				throw new ScriptRuntimeException("Saving files to " + path + " is disallowed.");
			}
			SaveUtil.SaveGame(CampaignDataPath.CreateRegular(path), false);
		}

		// Token: 0x060054F4 RID: 21748 RVA: 0x002CEEB0 File Offset: 0x002CD0B0
		public void LoadGame(string path)
		{
			SaveUtil.LoadGame(CampaignDataPath.CreateRegular(path));
		}

		// Token: 0x060054F5 RID: 21749 RVA: 0x002CEEBD File Offset: 0x002CD0BD
		public void Stop()
		{
			this.MapEntityUpdateInterval = 1;
			this.CharacterUpdateInterval = 1;
			this._consoleCommands.RemoveRegisteredCommands();
		}

		// Token: 0x060054F6 RID: 21750 RVA: 0x002CEED8 File Offset: 0x002CD0D8
		public Result Reset()
		{
			this.Stop();
			return Result.Ok();
		}

		// Token: 0x060054F7 RID: 21751 RVA: 0x002CEEE5 File Offset: 0x002CD0E5
		public void Dispose()
		{
			this.Stop();
		}

		// Token: 0x04002C4A RID: 11338
		public bool? ForceVoice;

		// Token: 0x04002C4B RID: 11339
		public bool? ForceLocalVoice;

		// Token: 0x04002C4C RID: 11340
		public DynValue Settings;

		// Token: 0x04002C4D RID: 11341
		public bool allowWifiChat;

		// Token: 0x04002C4E RID: 11342
		public bool overrideTraitors;

		// Token: 0x04002C4F RID: 11343
		public bool overrideRespawnSub;

		// Token: 0x04002C50 RID: 11344
		public bool overrideSignalRadio;

		// Token: 0x04002C51 RID: 11345
		public bool disableSpamFilter;

		// Token: 0x04002C52 RID: 11346
		public bool disableDisconnectCharacter;

		// Token: 0x04002C53 RID: 11347
		public bool enableControlHusk;

		// Token: 0x04002C54 RID: 11348
		public HashSet<Item> UpdatePriorityItems = new HashSet<Item>();

		// Token: 0x04002C55 RID: 11349
		public HashSet<Character> UpdatePriorityCharacters = new HashSet<Character>();

		// Token: 0x04002C56 RID: 11350
		private readonly IConsoleCommandsService _consoleCommands;
	}
}
