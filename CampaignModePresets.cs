using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002CD RID: 717
	internal static class CampaignModePresets
	{
		// Token: 0x06003CB7 RID: 15543 RVA: 0x0022BDF0 File Offset: 0x00229FF0
		static CampaignModePresets()
		{
			if (File.Exists(CampaignModePresets.fileListPath))
			{
				XDocument xdocument = XMLExtensions.TryLoadXml(CampaignModePresets.fileListPath);
				XElement docRoot = (xdocument != null) ? xdocument.Root : null;
				if (docRoot != null)
				{
					List<CampaignSettings> presetList = new List<CampaignSettings>();
					Dictionary<Identifier, CampaignSettingDefinitions> tempDefinitions = new Dictionary<Identifier, CampaignSettingDefinitions>();
					foreach (XElement element in docRoot.Elements())
					{
						Identifier name = element.NameAsIdentifier();
						if (name == "campaignsettings")
						{
							presetList.Add(new CampaignSettings(element));
						}
						else if (name == "CampaignSettingDefinitions")
						{
							foreach (XElement subElement in element.Elements())
							{
								tempDefinitions.Add(subElement.NameAsIdentifier(), new CampaignSettingDefinitions(subElement));
							}
						}
					}
					CampaignModePresets.List = presetList.ToImmutableArray<CampaignSettings>();
					CampaignModePresets.definitions = tempDefinitions.ToImmutableDictionary<Identifier, CampaignSettingDefinitions>();
					return;
				}
			}
			CampaignModePresets.List = ImmutableArray<CampaignSettings>.Empty;
		}

		// Token: 0x06003CB8 RID: 15544 RVA: 0x0022BF28 File Offset: 0x0022A128
		public static bool TryGetAttribute(Identifier propertyName, Identifier attributeName, out XAttribute attribute)
		{
			attribute = null;
			CampaignSettingDefinitions definition;
			XAttribute att;
			if (CampaignModePresets.definitions.TryGetValue(propertyName, out definition) && definition.Attributes.TryGetValue(attributeName, out att))
			{
				attribute = att;
				return true;
			}
			return false;
		}

		// Token: 0x04001F52 RID: 8018
		public static readonly ImmutableArray<CampaignSettings> List;

		// Token: 0x04001F53 RID: 8019
		private static readonly ImmutableDictionary<Identifier, CampaignSettingDefinitions> definitions;

		// Token: 0x04001F54 RID: 8020
		private static readonly string fileListPath = Path.Combine("Data", "campaignsettings.xml");
	}
}
