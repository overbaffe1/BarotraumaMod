using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003E8 RID: 1000
	internal class ConsoleCommandsService : IConsoleCommandsService, IService, IDisposable
	{
		// Token: 0x06003968 RID: 14696 RVA: 0x0017F6EC File Offset: 0x0017D8EC
		public void Dispose()
		{
			if (!ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
			{
				return;
			}
			foreach (DebugConsole.Command cmd in this._registeredCommands.ToImmutableArray<DebugConsole.Command>())
			{
				DebugConsole.Commands.Remove(cmd);
			}
			this._registeredCommands.Clear();
		}

		// Token: 0x17000FA5 RID: 4005
		// (get) Token: 0x06003969 RID: 14697 RVA: 0x0017F745 File Offset: 0x0017D945
		// (set) Token: 0x0600396A RID: 14698 RVA: 0x0017F752 File Offset: 0x0017D952
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
			private set
			{
				ModUtils.Threading.SetBool(ref this._isDisposed, value);
			}
		}

		// Token: 0x0600396B RID: 14699 RVA: 0x0017F760 File Offset: 0x0017D960
		public void RegisterCommand(string name, string help, Action<string[]> onExecute, Func<string[][]> getValidArgs = null, bool isCheat = false)
		{
			IService.CheckDisposed(this);
			if (DebugConsole.Commands.Any((DebugConsole.Command cmd) => cmd.Names.Contains(name)))
			{
				LuaCsSetup.Instance.Logger.LogWarning("Registering console command " + name + " more than once!");
			}
			DebugConsole.Command cmd2 = new DebugConsole.Command(name, help, onExecute, getValidArgs, isCheat);
			this._registeredCommands.Add(cmd2);
			DebugConsole.Commands.Add(cmd2);
		}

		// Token: 0x0600396C RID: 14700 RVA: 0x0017F7E8 File Offset: 0x0017D9E8
		public void AssignOnExecute(string names, Action<string[]> onExecute)
		{
			DebugConsole.Command matchingCommand = DebugConsole.Commands.Find((DebugConsole.Command c) => c.Names.Intersect(names.Split('|', StringSplitOptions.None).ToIdentifiers()).Any<Identifier>());
			if (matchingCommand == null)
			{
				throw new Exception("AssignOnExecute failed. Command matching the name(s) \"" + names + "\" not found.");
			}
			matchingCommand.OnExecute = onExecute;
		}

		// Token: 0x0600396D RID: 14701 RVA: 0x0017F840 File Offset: 0x0017DA40
		public void AssignOnClientRequestExecute(string names, Action<Client, Vector2, string[]> onClientRequestExecute)
		{
			DebugConsole.Command matchingCommand = DebugConsole.Commands.Find((DebugConsole.Command c) => c.Names.Intersect(names.Split('|', StringSplitOptions.None).ToIdentifiers()).Any<Identifier>());
			if (matchingCommand == null)
			{
				throw new Exception("AssignOnClientRequestExecute failed. Command matching the name(s) \"" + names + "\" not found.");
			}
			matchingCommand.OnClientRequestExecute = onClientRequestExecute;
		}

		// Token: 0x0600396E RID: 14702 RVA: 0x0017F898 File Offset: 0x0017DA98
		public void RemoveCommand(string name)
		{
			IService.CheckDisposed(this);
			this._registeredCommands.RemoveAll((DebugConsole.Command cmd) => cmd.Names.Contains(name));
			DebugConsole.Commands.RemoveAll((DebugConsole.Command cmd) => cmd.Names.Contains(name));
		}

		// Token: 0x0600396F RID: 14703 RVA: 0x0017F8E8 File Offset: 0x0017DAE8
		public void RemoveRegisteredCommands()
		{
			IService.CheckDisposed(this);
			foreach (DebugConsole.Command cmd in this._registeredCommands.ToImmutableArray<DebugConsole.Command>())
			{
				DebugConsole.Commands.Remove(cmd);
			}
			this._registeredCommands.Clear();
		}

		// Token: 0x04001CDC RID: 7388
		private readonly List<DebugConsole.Command> _registeredCommands = new List<DebugConsole.Command>();

		// Token: 0x04001CDD RID: 7389
		private int _isDisposed;
	}
}
