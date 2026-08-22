using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x020000EF RID: 239
	internal abstract class EditableParams : ISerializableEntity
	{
		// Token: 0x17000785 RID: 1925
		// (get) Token: 0x06001922 RID: 6434 RVA: 0x000C5C25 File Offset: 0x000C3E25
		// (set) Token: 0x06001923 RID: 6435 RVA: 0x000C5C2D File Offset: 0x000C3E2D
		public bool IsLoaded { get; protected set; }

		// Token: 0x17000786 RID: 1926
		// (get) Token: 0x06001924 RID: 6436 RVA: 0x000C5C36 File Offset: 0x000C3E36
		// (set) Token: 0x06001925 RID: 6437 RVA: 0x000C5C3E File Offset: 0x000C3E3E
		public string Name { get; private set; }

		// Token: 0x17000787 RID: 1927
		// (get) Token: 0x06001926 RID: 6438 RVA: 0x000C5C47 File Offset: 0x000C3E47
		// (set) Token: 0x06001927 RID: 6439 RVA: 0x000C5C4F File Offset: 0x000C3E4F
		public string FileName { get; private set; }

		// Token: 0x17000788 RID: 1928
		// (get) Token: 0x06001928 RID: 6440 RVA: 0x000C5C58 File Offset: 0x000C3E58
		// (set) Token: 0x06001929 RID: 6441 RVA: 0x000C5C60 File Offset: 0x000C3E60
		public string FileNameWithoutExtension { get; private set; }

		// Token: 0x17000789 RID: 1929
		// (get) Token: 0x0600192A RID: 6442 RVA: 0x000C5C69 File Offset: 0x000C3E69
		// (set) Token: 0x0600192B RID: 6443 RVA: 0x000C5C71 File Offset: 0x000C3E71
		public string Folder { get; private set; }

		// Token: 0x1700078A RID: 1930
		// (get) Token: 0x0600192C RID: 6444 RVA: 0x000C5C7A File Offset: 0x000C3E7A
		// (set) Token: 0x0600192D RID: 6445 RVA: 0x000C5C82 File Offset: 0x000C3E82
		public ContentPath Path { get; protected set; } = ContentPath.Empty;

		// Token: 0x1700078B RID: 1931
		// (get) Token: 0x0600192E RID: 6446 RVA: 0x000C5C8B File Offset: 0x000C3E8B
		// (set) Token: 0x0600192F RID: 6447 RVA: 0x000C5C93 File Offset: 0x000C3E93
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; protected set; }

		// Token: 0x1700078C RID: 1932
		// (get) Token: 0x06001930 RID: 6448 RVA: 0x000C5C9C File Offset: 0x000C3E9C
		// (set) Token: 0x06001931 RID: 6449 RVA: 0x000C5CC0 File Offset: 0x000C3EC0
		private XDocument Doc
		{
			get
			{
				if (!this.IsLoaded)
				{
					DebugConsole.ThrowError("[Params] Not loaded!", null, null, false, false);
					return new XDocument();
				}
				return this.doc;
			}
			set
			{
				this.doc = value;
			}
		}

		// Token: 0x1700078D RID: 1933
		// (get) Token: 0x06001932 RID: 6450 RVA: 0x000C5CCC File Offset: 0x000C3ECC
		public virtual ContentXElement MainElement
		{
			get
			{
				ContentXElement contentXElement = this.rootElement;
				if (((contentXElement != null) ? contentXElement.Element : null) != this.doc.Root)
				{
					this.rootElement = this.doc.Root.FromPackage(this.Path.ContentPackage);
				}
				return this.rootElement;
			}
		}

		// Token: 0x1700078E RID: 1934
		// (get) Token: 0x06001933 RID: 6451 RVA: 0x000C5D1F File Offset: 0x000C3F1F
		// (set) Token: 0x06001934 RID: 6452 RVA: 0x000C5D27 File Offset: 0x000C3F27
		public ContentXElement OriginalElement { get; protected set; }

		// Token: 0x06001935 RID: 6453 RVA: 0x000C5D30 File Offset: 0x000C3F30
		protected ContentXElement CreateElement(string name, params object[] attrs)
		{
			return new XElement(name, attrs).FromPackage(this.Path.ContentPackage);
		}

		// Token: 0x06001936 RID: 6454 RVA: 0x000C5D4E File Offset: 0x000C3F4E
		protected virtual string GetName()
		{
			return System.IO.Path.GetFileNameWithoutExtension(this.Path.Value).FormatCamelCaseWithSpaces();
		}

		// Token: 0x06001937 RID: 6455 RVA: 0x000C5D65 File Offset: 0x000C3F65
		protected virtual bool Deserialize(XElement element = null)
		{
			if (element == null)
			{
				element = this.MainElement;
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			return this.SerializableProperties != null;
		}

		// Token: 0x06001938 RID: 6456 RVA: 0x000C5D8D File Offset: 0x000C3F8D
		protected virtual bool Serialize(XElement element = null)
		{
			if (element == null)
			{
				element = this.MainElement;
			}
			if (element == null)
			{
				DebugConsole.ThrowError("[EditableParams] The XML element is null! Failed to save the parameters.", null, null, false, false);
				return false;
			}
			SerializableProperty.SerializeProperties(this, element, true, false);
			return true;
		}

		// Token: 0x06001939 RID: 6457 RVA: 0x000C5DBC File Offset: 0x000C3FBC
		protected virtual bool Load(ContentPath file)
		{
			this.UpdatePath(file);
			this.doc = XMLExtensions.TryLoadXml(this.Path);
			if (this.doc == null)
			{
				DebugConsole.ThrowError("[EditableParams] The document is null! Failed to load the parameters.", null, file.ContentPackage, false, false);
				return false;
			}
			ContentXElement mainElement = this.MainElement;
			ContentXElement contentXElement = null;
			if (mainElement == contentXElement)
			{
				DebugConsole.ThrowError("[EditableParams] The main element is null! Failed to load the parameters.", null, file.ContentPackage, false, false);
				return false;
			}
			this.IsLoaded = this.Deserialize(this.MainElement);
			this.OriginalElement = new XElement(this.MainElement).FromPackage(this.MainElement.ContentPackage);
			return this.IsLoaded;
		}

		// Token: 0x0600193A RID: 6458 RVA: 0x000C5E6C File Offset: 0x000C406C
		protected virtual void UpdatePath(ContentPath fullPath)
		{
			this.Path = fullPath;
			this.Name = this.GetName();
			this.FileName = Barotrauma.IO.Path.GetFileName(this.Path.Value);
			this.FileNameWithoutExtension = Barotrauma.IO.Path.GetFileNameWithoutExtension(this.Path.Value);
			this.Folder = Barotrauma.IO.Path.GetDirectoryName(this.Path.Value);
		}

		// Token: 0x0600193B RID: 6459 RVA: 0x000C5ED0 File Offset: 0x000C40D0
		public virtual bool Save(string fileNameWithoutExtension = null, XmlWriterSettings settings = null)
		{
			if (!Directory.Exists(this.Folder))
			{
				Directory.CreateDirectory(this.Folder, false);
			}
			this.OriginalElement = this.MainElement;
			this.Serialize(null);
			if (settings == null)
			{
				settings = new XmlWriterSettings
				{
					Indent = true,
					OmitXmlDeclaration = true,
					NewLineOnAttributes = true
				};
			}
			if (fileNameWithoutExtension != null)
			{
				this.UpdatePath(ContentPath.FromRaw(this.Path.ContentPackage, System.IO.Path.Combine(this.Folder, fileNameWithoutExtension + ".xml")));
			}
			using (XmlWriter writer = XmlWriter.Create(this.Path.Value, settings))
			{
				this.Doc.WriteTo(writer);
				writer.Flush();
			}
			return true;
		}

		// Token: 0x0600193C RID: 6460 RVA: 0x000C5F9C File Offset: 0x000C419C
		public virtual bool Reset(bool forceReload = false)
		{
			if (forceReload)
			{
				return this.Load(this.Path);
			}
			return this.Deserialize(this.OriginalElement);
		}

		// Token: 0x04000C14 RID: 3092
		protected ContentXElement rootElement;

		// Token: 0x04000C15 RID: 3093
		protected XDocument doc;
	}
}
