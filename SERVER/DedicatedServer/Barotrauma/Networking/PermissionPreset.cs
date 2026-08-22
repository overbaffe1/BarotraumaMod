using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma.Networking
{
	// Token: 0x0200037D RID: 893
	internal class PermissionPreset
	{
		// Token: 0x0600360D RID: 13837 RVA: 0x00172D48 File Offset: 0x00170F48
		public PermissionPreset(XElement element)
		{
			this.Identifier = element.GetAttributeIdentifier("name", Identifier.Empty);
			this.DisplayName = TextManager.Get("permissionpresetname." + this.Identifier.ToString()).Fallback(this.Identifier.ToString(), true);
			this.Description = TextManager.Get("permissionpresetdescription." + this.Identifier.ToString()).Fallback(element.GetAttributeString("description", ""), true);
			string permissionsStr = element.GetAttributeString("permissions", "");
			if (!Enum.TryParse<ClientPermissions>(permissionsStr, out this.Permissions))
			{
				DebugConsole.ThrowErrorLocalized("Error in permission preset \"" + this.DisplayName + "\" - " + permissionsStr + " is not a valid permission!", null, null, false, false);
			}
			this.PermittedCommands = new HashSet<DebugConsole.Command>();
			if (this.Permissions.HasFlag(ClientPermissions.ConsoleCommands))
			{
				foreach (XElement subElement in element.Elements())
				{
					if (subElement.Name.ToString().Equals("command", StringComparison.OrdinalIgnoreCase))
					{
						string commandName = subElement.GetAttributeString("name", "");
						DebugConsole.Command command = DebugConsole.FindCommand(commandName);
						if (command == null)
						{
							DebugConsole.ThrowErrorLocalized("Error in permission preset \"" + this.DisplayName + "\" - " + commandName + "\" is not a valid console command.", null, null, false, false);
						}
						else
						{
							this.PermittedCommands.Add(command);
						}
					}
				}
			}
		}

		// Token: 0x0600360E RID: 13838 RVA: 0x00172F50 File Offset: 0x00171150
		public static void LoadAll(string file)
		{
			if (!File.Exists(file))
			{
				return;
			}
			XDocument doc = XMLExtensions.TryLoadXml(file);
			if (doc == null)
			{
				return;
			}
			foreach (XElement element in doc.Root.Elements())
			{
				PermissionPreset newPermissionPreset = new PermissionPreset(element);
				PermissionPreset existingPreset = PermissionPreset.List.FirstOrDefault((PermissionPreset p) => p.Identifier == newPermissionPreset.Identifier);
				if (existingPreset != null)
				{
					PermissionPreset.List.Remove(existingPreset);
					DebugConsole.AddWarning("The permission preset file " + file + " contains a permission preset that conflicts with another preset. Overriding the previous preset...", null);
				}
				PermissionPreset.List.Add(newPermissionPreset);
			}
		}

		// Token: 0x0600360F RID: 13839 RVA: 0x00173010 File Offset: 0x00171210
		public bool MatchesPermissions(ClientPermissions permissions, ISet<DebugConsole.Command> permittedConsoleCommands)
		{
			return permissions == this.Permissions && this.PermittedCommands.All(new Func<DebugConsole.Command, bool>(permittedConsoleCommands.Contains)) && permittedConsoleCommands.All(new Func<DebugConsole.Command, bool>(this.PermittedCommands.Contains));
		}

		// Token: 0x04001B01 RID: 6913
		public static readonly List<PermissionPreset> List = new List<PermissionPreset>();

		// Token: 0x04001B02 RID: 6914
		public readonly Identifier Identifier;

		// Token: 0x04001B03 RID: 6915
		public readonly LocalizedString DisplayName;

		// Token: 0x04001B04 RID: 6916
		public readonly LocalizedString Description;

		// Token: 0x04001B05 RID: 6917
		public readonly ClientPermissions Permissions;

		// Token: 0x04001B06 RID: 6918
		public readonly HashSet<DebugConsole.Command> PermittedCommands;
	}
}
