using System;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003FA RID: 1018
	public interface IConsoleCommandsService : IService, IDisposable
	{
		// Token: 0x06003ACB RID: 15051
		void RegisterCommand(string name, string help, Action<string[]> onExecute, Func<string[][]> getValidArgs = null, bool isCheat = false);

		// Token: 0x06003ACC RID: 15052
		void AssignOnExecute(string names, Action<string[]> onExecute);

		// Token: 0x06003ACD RID: 15053
		void AssignOnClientRequestExecute(string names, Action<Client, Vector2, string[]> onClientRequestExecute);

		// Token: 0x06003ACE RID: 15054
		void RemoveCommand(string name);

		// Token: 0x06003ACF RID: 15055
		void RemoveRegisteredCommands();
	}
}
