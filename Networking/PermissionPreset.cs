using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma.Networking
{
	// Token: 0x02000479 RID: 1145
	internal class PermissionPreset
	{
		// Token: 0x06004DC9 RID: 19913 RVA: 0x002AB760 File Offset: 0x002A9960
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
						if (command != null)
						{
							this.PermittedCommands.Add(command);
						}
					}
				}
			}
		}

		// Token: 0x06004DCA RID: 19914 RVA: 0x002AB918 File Offset: 0x002A9B18
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

		// Token: 0x06004DCB RID: 19915 RVA: 0x002AB9D8 File Offset: 0x002A9BD8
		public bool MatchesPermissions(ClientPermissions permissions, ISet<DebugConsole.Command> permittedConsoleCommands)
		{
			return permissions == this.Permissions && this.PermittedCommands.All(new Func<DebugConsole.Command, bool>(permittedConsoleCommands.Contains)) && permittedConsoleCommands.All(new Func<DebugConsole.Command, bool>(this.PermittedCommands.Contains));
		}

		// Token: 0x040028EC RID: 10476
		public static readonly List<PermissionPreset> List = new List<PermissionPreset>();

		// Token: 0x040028ED RID: 10477
		public readonly Identifier Identifier;

		// Token: 0x040028EE RID: 10478
		public readonly LocalizedString DisplayName;

		// Token: 0x040028EF RID: 10479
		public readonly LocalizedString Description;

		// Token: 0x040028F0 RID: 10480
		public readonly ClientPermissions Permissions;

		// Token: 0x040028F1 RID: 10481
		public readonly HashSet<DebugConsole.Command> PermittedCommands;
	}
}
