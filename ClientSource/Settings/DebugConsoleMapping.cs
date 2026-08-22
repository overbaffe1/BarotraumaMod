using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma.ClientSource.Settings
{
	// Token: 0x0200044B RID: 1099
	[NullableContext(1)]
	[Nullable(0)]
	public class DebugConsoleMapping
	{
		// Token: 0x170012A1 RID: 4769
		// (get) Token: 0x06004919 RID: 18713 RVA: 0x0027F01A File Offset: 0x0027D21A
		public IReadOnlyDictionary<KeyOrMouse, string> Bindings
		{
			get
			{
				return this.bindings;
			}
		}

		// Token: 0x0600491A RID: 18714 RVA: 0x0027F022 File Offset: 0x0027D222
		private DebugConsoleMapping()
		{
		}

		// Token: 0x0600491B RID: 18715 RVA: 0x0027F038 File Offset: 0x0027D238
		private DebugConsoleMapping(XElement element)
		{
			Dictionary<KeyOrMouse, string> bindings = new Dictionary<KeyOrMouse, string>();
			foreach (XElement subElement in element.Elements())
			{
				KeyOrMouse keyOrMouse = subElement.GetAttributeKeyOrMouse("key", MouseButton.None);
				if (!(keyOrMouse == MouseButton.None))
				{
					string command = subElement.GetAttributeString("command", "");
					if (!command.IsNullOrWhiteSpace())
					{
						bindings[keyOrMouse] = command;
					}
				}
			}
			this.bindings = bindings;
		}

		// Token: 0x0600491C RID: 18716 RVA: 0x0027F0DC File Offset: 0x0027D2DC
		[NullableContext(2)]
		public static void Init(XElement element)
		{
			if (element == null)
			{
				return;
			}
			DebugConsoleMapping.Instance = new DebugConsoleMapping(element);
		}

		// Token: 0x0600491D RID: 18717 RVA: 0x0027F0F0 File Offset: 0x0027D2F0
		public void SaveTo(XElement element)
		{
			this.Bindings.ForEach(delegate(KeyValuePair<KeyOrMouse, string> kvp)
			{
				element.Add(new XElement("Keybind", new object[]
				{
					new XAttribute("key", kvp.Key),
					new XAttribute("command", kvp.Value)
				}));
			});
		}

		// Token: 0x0600491E RID: 18718 RVA: 0x0027F121 File Offset: 0x0027D321
		public void Set(KeyOrMouse key, string command)
		{
			this.bindings[key] = command;
		}

		// Token: 0x0600491F RID: 18719 RVA: 0x0027F130 File Offset: 0x0027D330
		public void Remove(KeyOrMouse key)
		{
			this.bindings.Remove(key);
		}

		// Token: 0x170012A2 RID: 4770
		// (get) Token: 0x06004920 RID: 18720 RVA: 0x0027F13F File Offset: 0x0027D33F
		// (set) Token: 0x06004921 RID: 18721 RVA: 0x0027F146 File Offset: 0x0027D346
		public static DebugConsoleMapping Instance { get; private set; } = new DebugConsoleMapping();

		// Token: 0x040025EC RID: 9708
		private readonly Dictionary<KeyOrMouse, string> bindings = new Dictionary<KeyOrMouse, string>();
	}
}
