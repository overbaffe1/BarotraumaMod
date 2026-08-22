using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004FE RID: 1278
	internal class ConsoleCommandsService : IConsoleCommandsService, IService, IDisposable
	{
		// Token: 0x060052A8 RID: 21160 RVA: 0x002C465C File Offset: 0x002C285C
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

		// Token: 0x170014EE RID: 5358
		// (get) Token: 0x060052A9 RID: 21161 RVA: 0x002C46B5 File Offset: 0x002C28B5
		// (set) Token: 0x060052AA RID: 21162 RVA: 0x002C46C2 File Offset: 0x002C28C2
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

		// Token: 0x060052AB RID: 21163 RVA: 0x002C46D0 File Offset: 0x002C28D0
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

		// Token: 0x060052AC RID: 21164 RVA: 0x002C4758 File Offset: 0x002C2958
		public void AssignOnExecute(string names, Action<string[]> onExecute)
		{
			DebugConsole.Command matchingCommand = DebugConsole.Commands.Find((DebugConsole.Command c) => c.Names.Intersect(names.Split('|', StringSplitOptions.None).ToIdentifiers()).Any<Identifier>());
			if (matchingCommand == null)
			{
				throw new Exception("AssignOnExecute failed. Command matching the name(s) \"" + names + "\" not found.");
			}
			matchingCommand.OnExecute = onExecute;
		}

		// Token: 0x060052AD RID: 21165 RVA: 0x002C47B0 File Offset: 0x002C29B0
		public void RemoveCommand(string name)
		{
			IService.CheckDisposed(this);
			this._registeredCommands.RemoveAll((DebugConsole.Command cmd) => cmd.Names.Contains(name));
			DebugConsole.Commands.RemoveAll((DebugConsole.Command cmd) => cmd.Names.Contains(name));
		}

		// Token: 0x060052AE RID: 21166 RVA: 0x002C4800 File Offset: 0x002C2A00
		public void RemoveRegisteredCommands()
		{
			IService.CheckDisposed(this);
			foreach (DebugConsole.Command cmd in this._registeredCommands.ToImmutableArray<DebugConsole.Command>())
			{
				DebugConsole.Commands.Remove(cmd);
			}
			this._registeredCommands.Clear();
		}

		// Token: 0x04002BC8 RID: 11208
		private readonly List<DebugConsole.Command> _registeredCommands = new List<DebugConsole.Command>();

		// Token: 0x04002BC9 RID: 11209
		private int _isDisposed;
	}
}
