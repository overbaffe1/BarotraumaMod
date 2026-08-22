using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000126 RID: 294
	[NullableContext(1)]
	[Nullable(0)]
	public class ServerListFilters
	{
		// Token: 0x060027DF RID: 10207 RVA: 0x001BC050 File Offset: 0x001BA250
		[NullableContext(2)]
		private ServerListFilters(XElement elem)
		{
			if (elem == null)
			{
				return;
			}
			foreach (XAttribute attr in elem.Attributes())
			{
				this.attributes.Add(attr.NameAsIdentifier(), attr.Value);
			}
		}

		// Token: 0x060027E0 RID: 10208 RVA: 0x001BC0C4 File Offset: 0x001BA2C4
		[NullableContext(2)]
		public static void Init(XElement elem)
		{
			ServerListFilters.Instance = new ServerListFilters(elem);
		}

		// Token: 0x060027E1 RID: 10209 RVA: 0x001BC0D4 File Offset: 0x001BA2D4
		public void SaveTo(XElement elem)
		{
			foreach (KeyValuePair<Identifier, string> kvp in this.attributes)
			{
				elem.Add(new XAttribute(kvp.Key.Value, kvp.Value));
			}
		}

		// Token: 0x060027E2 RID: 10210 RVA: 0x001BC148 File Offset: 0x001BA348
		public bool GetAttributeBool(Identifier key, bool def)
		{
			string val;
			bool result;
			if (this.attributes.TryGetValue(key, out val) && bool.TryParse(val, out result))
			{
				return result;
			}
			return def;
		}

		// Token: 0x060027E3 RID: 10211 RVA: 0x001BC174 File Offset: 0x001BA374
		[NullableContext(0)]
		public T GetAttributeEnum<T>(Identifier key, T def) where T : struct, Enum
		{
			string val;
			T result;
			if (this.attributes.TryGetValue(key, out val) && Enum.TryParse<T>(val, true, out result))
			{
				return result;
			}
			return def;
		}

		// Token: 0x060027E4 RID: 10212 RVA: 0x001BC1A0 File Offset: 0x001BA3A0
		public LanguageIdentifier[] GetAttributeLanguageIdentifierArray(Identifier key, LanguageIdentifier[] def)
		{
			string val;
			if (!this.attributes.TryGetValue(key, out val))
			{
				return def;
			}
			return (from s in val.Split(",", StringSplitOptions.None)
			select s.Trim() into s
			where !s.IsNullOrWhiteSpace()
			select s.ToLanguageIdentifier()).ToArray<LanguageIdentifier>();
		}

		// Token: 0x060027E5 RID: 10213 RVA: 0x001BC23C File Offset: 0x001BA43C
		public void SetAttribute(Identifier key, string val)
		{
			this.attributes[key] = val;
		}

		// Token: 0x17000A53 RID: 2643
		// (get) Token: 0x060027E6 RID: 10214 RVA: 0x001BC24B File Offset: 0x001BA44B
		// (set) Token: 0x060027E7 RID: 10215 RVA: 0x001BC252 File Offset: 0x001BA452
		public static ServerListFilters Instance { get; private set; } = new ServerListFilters(null);

		// Token: 0x0400144E RID: 5198
		private readonly Dictionary<Identifier, string> attributes = new Dictionary<Identifier, string>();
	}
}
