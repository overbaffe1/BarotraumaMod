using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200050D RID: 1293
	public interface IConsoleCommandsService : IService, IDisposable
	{
		// Token: 0x060053E8 RID: 21480
		void RegisterCommand(string name, string help, Action<string[]> onExecute, Func<string[][]> getValidArgs = null, bool isCheat = false);

		// Token: 0x060053E9 RID: 21481
		void AssignOnExecute(string names, Action<string[]> onExecute);

		// Token: 0x060053EA RID: 21482
		void RemoveCommand(string name);

		// Token: 0x060053EB RID: 21483
		void RemoveRegisteredCommands();
	}
}
