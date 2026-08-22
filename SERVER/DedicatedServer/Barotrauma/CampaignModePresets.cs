using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001E0 RID: 480
	internal static class CampaignModePresets
	{
		// Token: 0x060022B5 RID: 8885 RVA: 0x000E8D90 File Offset: 0x000E6F90
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

		// Token: 0x060022B6 RID: 8886 RVA: 0x000E8EC8 File Offset: 0x000E70C8
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

		// Token: 0x040010B6 RID: 4278
		public static readonly ImmutableArray<CampaignSettings> List;

		// Token: 0x040010B7 RID: 4279
		private static readonly ImmutableDictionary<Identifier, CampaignSettingDefinitions> definitions;

		// Token: 0x040010B8 RID: 4280
		private static readonly string fileListPath = Path.Combine("Data", "campaignsettings.xml");
	}
}
