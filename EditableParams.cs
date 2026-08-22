using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001EB RID: 491
	internal abstract class EditableParams : ISerializableEntity
	{
		// Token: 0x17000DEC RID: 3564
		// (get) Token: 0x0600341F RID: 13343 RVA: 0x0020C4C9 File Offset: 0x0020A6C9
		// (set) Token: 0x06003420 RID: 13344 RVA: 0x0020C4D1 File Offset: 0x0020A6D1
		public bool IsLoaded { get; protected set; }

		// Token: 0x17000DED RID: 3565
		// (get) Token: 0x06003421 RID: 13345 RVA: 0x0020C4DA File Offset: 0x0020A6DA
		// (set) Token: 0x06003422 RID: 13346 RVA: 0x0020C4E2 File Offset: 0x0020A6E2
		public string Name { get; private set; }

		// Token: 0x17000DEE RID: 3566
		// (get) Token: 0x06003423 RID: 13347 RVA: 0x0020C4EB File Offset: 0x0020A6EB
		// (set) Token: 0x06003424 RID: 13348 RVA: 0x0020C4F3 File Offset: 0x0020A6F3
		public string FileName { get; private set; }

		// Token: 0x17000DEF RID: 3567
		// (get) Token: 0x06003425 RID: 13349 RVA: 0x0020C4FC File Offset: 0x0020A6FC
		// (set) Token: 0x06003426 RID: 13350 RVA: 0x0020C504 File Offset: 0x0020A704
		public string FileNameWithoutExtension { get; private set; }

		// Token: 0x17000DF0 RID: 3568
		// (get) Token: 0x06003427 RID: 13351 RVA: 0x0020C50D File Offset: 0x0020A70D
		// (set) Token: 0x06003428 RID: 13352 RVA: 0x0020C515 File Offset: 0x0020A715
		public string Folder { get; private set; }

		// Token: 0x17000DF1 RID: 3569
		// (get) Token: 0x06003429 RID: 13353 RVA: 0x0020C51E File Offset: 0x0020A71E
		// (set) Token: 0x0600342A RID: 13354 RVA: 0x0020C526 File Offset: 0x0020A726
		public ContentPath Path { get; protected set; } = ContentPath.Empty;

		// Token: 0x17000DF2 RID: 3570
		// (get) Token: 0x0600342B RID: 13355 RVA: 0x0020C52F File Offset: 0x0020A72F
		// (set) Token: 0x0600342C RID: 13356 RVA: 0x0020C537 File Offset: 0x0020A737
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; protected set; }

		// Token: 0x17000DF3 RID: 3571
		// (get) Token: 0x0600342D RID: 13357 RVA: 0x0020C540 File Offset: 0x0020A740
		// (set) Token: 0x0600342E RID: 13358 RVA: 0x0020C564 File Offset: 0x0020A764
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

		// Token: 0x17000DF4 RID: 3572
		// (get) Token: 0x0600342F RID: 13359 RVA: 0x0020C570 File Offset: 0x0020A770
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

		// Token: 0x17000DF5 RID: 3573
		// (get) Token: 0x06003430 RID: 13360 RVA: 0x0020C5C3 File Offset: 0x0020A7C3
		// (set) Token: 0x06003431 RID: 13361 RVA: 0x0020C5CB File Offset: 0x0020A7CB
		public ContentXElement OriginalElement { get; protected set; }

		// Token: 0x06003432 RID: 13362 RVA: 0x0020C5D4 File Offset: 0x0020A7D4
		protected ContentXElement CreateElement(string name, params object[] attrs)
		{
			return new XElement(name, attrs).FromPackage(this.Path.ContentPackage);
		}

		// Token: 0x06003433 RID: 13363 RVA: 0x0020C5F2 File Offset: 0x0020A7F2
		protected virtual string GetName()
		{
			return System.IO.Path.GetFileNameWithoutExtension(this.Path.Value).FormatCamelCaseWithSpaces();
		}

		// Token: 0x06003434 RID: 13364 RVA: 0x0020C609 File Offset: 0x0020A809
		protected virtual bool Deserialize(XElement element = null)
		{
			if (element == null)
			{
				element = this.MainElement;
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			return this.SerializableProperties != null;
		}

		// Token: 0x06003435 RID: 13365 RVA: 0x0020C631 File Offset: 0x0020A831
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

		// Token: 0x06003436 RID: 13366 RVA: 0x0020C660 File Offset: 0x0020A860
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

		// Token: 0x06003437 RID: 13367 RVA: 0x0020C710 File Offset: 0x0020A910
		protected virtual void UpdatePath(ContentPath fullPath)
		{
			this.Path = fullPath;
			this.Name = this.GetName();
			this.FileName = Barotrauma.IO.Path.GetFileName(this.Path.Value);
			this.FileNameWithoutExtension = Barotrauma.IO.Path.GetFileNameWithoutExtension(this.Path.Value);
			this.Folder = Barotrauma.IO.Path.GetDirectoryName(this.Path.Value);
		}

		// Token: 0x06003438 RID: 13368 RVA: 0x0020C774 File Offset: 0x0020A974
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

		// Token: 0x06003439 RID: 13369 RVA: 0x0020C840 File Offset: 0x0020AA40
		public virtual bool Reset(bool forceReload = false)
		{
			if (forceReload)
			{
				return this.Load(this.Path);
			}
			return this.Deserialize(this.OriginalElement);
		}

		// Token: 0x17000DF6 RID: 3574
		// (get) Token: 0x0600343A RID: 13370 RVA: 0x0020C863 File Offset: 0x0020AA63
		// (set) Token: 0x0600343B RID: 13371 RVA: 0x0020C86B File Offset: 0x0020AA6B
		public SerializableEntityEditor SerializableEntityEditor { get; protected set; }

		// Token: 0x0600343C RID: 13372 RVA: 0x0020C874 File Offset: 0x0020AA74
		public virtual void AddToEditor(ParamsEditor editor, int space = 0)
		{
			if (!this.IsLoaded)
			{
				DebugConsole.ThrowError("[Params] Not loaded!", null, null, false, false);
				return;
			}
			this.SerializableEntityEditor = new SerializableEntityEditor(editor.EditorBox.Content.RectTransform, this, false, true, "", 24, GUIStyle.LargeFont, true);
			if (space > 0)
			{
				new GUIFrame(new RectTransform(new Point(editor.EditorBox.Rect.Width, space), editor.EditorBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(ParamsEditor.Color)).CanBeFocused = false;
			}
		}

		// Token: 0x04001B39 RID: 6969
		protected ContentXElement rootElement;

		// Token: 0x04001B3A RID: 6970
		protected XDocument doc;
	}
}
