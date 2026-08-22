using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001B0 RID: 432
	internal class SetDataAction : EventAction
	{
		// Token: 0x06002004 RID: 8196 RVA: 0x000DA7E7 File Offset: 0x000D89E7
		public SetDataAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x1700092F RID: 2351
		// (get) Token: 0x06002005 RID: 8197 RVA: 0x000DA7F1 File Offset: 0x000D89F1
		// (set) Token: 0x06002006 RID: 8198 RVA: 0x000DA7F9 File Offset: 0x000D89F9
		[Serialize(SetDataAction.OperationType.Set, IsPropertySaveable.Yes, "Do you want to set the metadata to a specific value, multiply it, or add to it.", "", false)]
		public SetDataAction.OperationType Operation { get; set; }

		// Token: 0x17000930 RID: 2352
		// (get) Token: 0x06002007 RID: 8199 RVA: 0x000DA802 File Offset: 0x000D8A02
		// (set) Token: 0x06002008 RID: 8200 RVA: 0x000DA80A File Offset: 0x000D8A0A
		[Serialize(null, IsPropertySaveable.Yes, "Depending on the operation, the value you want to set the metadata to, multiply it with, or add to it.", "", false)]
		public string Value { get; set; }

		// Token: 0x17000931 RID: 2353
		// (get) Token: 0x06002009 RID: 8201 RVA: 0x000DA813 File Offset: 0x000D8A13
		// (set) Token: 0x0600200A RID: 8202 RVA: 0x000DA81B File Offset: 0x000D8A1B
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the metadata to set. Can be any arbitrary identifier, e.g. itemscollected, my_custom_event_state, specialnpckilled...", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x0600200B RID: 8203 RVA: 0x000DA824 File Offset: 0x000D8A24
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x0600200C RID: 8204 RVA: 0x000DA82C File Offset: 0x000D8A2C
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x0600200D RID: 8205 RVA: 0x000DA838 File Offset: 0x000D8A38
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			if (campaign != null)
			{
				object xmlValue = SetDataAction.ConvertXMLValue(this.Value);
				SetDataAction.PerformOperation(campaign.CampaignMetadata, this.Identifier, xmlValue, this.Operation);
			}
			this.isFinished = true;
		}

		// Token: 0x0600200E RID: 8206 RVA: 0x000DA894 File Offset: 0x000D8A94
		public static void PerformOperation(CampaignMetadata metadata, Identifier identifier, object value, SetDataAction.OperationType operation)
		{
			if (metadata == null)
			{
				return;
			}
			object currentValue = metadata.GetValue(identifier);
			float? originalValue = SetDataAction.ConvertValueToFloat(currentValue ?? 0);
			float? newValue = SetDataAction.ConvertValueToFloat(value);
			if ((originalValue == null || newValue == null) && operation != SetDataAction.OperationType.Set)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to perform numeric operations to a non number via SetDataAction (Existing: ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>((currentValue != null) ? currentValue.GetType() : null);
				defaultInterpolatedStringHandler.AppendLiteral(", New: ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(value.GetType());
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			switch (operation)
			{
			case SetDataAction.OperationType.Set:
				metadata.SetValue(identifier, value);
				return;
			case SetDataAction.OperationType.Multiply:
				metadata.SetValue(identifier, (originalValue * newValue).GetValueOrDefault());
				return;
			case SetDataAction.OperationType.Add:
				metadata.SetValue(identifier, (originalValue + newValue).GetValueOrDefault());
				return;
			default:
				return;
			}
		}

		// Token: 0x0600200F RID: 8207 RVA: 0x000DA9F0 File Offset: 0x000D8BF0
		private static float? ConvertValueToFloat(object value)
		{
			if (value is float || value is int)
			{
				return (float?)Convert.ChangeType(value, typeof(float));
			}
			return null;
		}

		// Token: 0x06002010 RID: 8208 RVA: 0x000DAA2C File Offset: 0x000D8C2C
		public static object ConvertXMLValue(string value)
		{
			bool b;
			if (bool.TryParse(value, out b))
			{
				return b;
			}
			float f;
			if (float.TryParse(value, out f))
			{
				return f;
			}
			return value;
		}

		// Token: 0x06002011 RID: 8209 RVA: 0x000DAA5C File Offset: 0x000D8C5C
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("SetDataAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Identifier: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Identifier.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Value: ");
			defaultInterpolatedStringHandler.AppendFormatted(SetDataAction.ConvertXMLValue(this.Value).ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Operation: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Operation.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000F3C RID: 3900
		private bool isFinished;

		// Token: 0x0200091E RID: 2334
		public enum OperationType
		{
			// Token: 0x0400320F RID: 12815
			Set,
			// Token: 0x04003210 RID: 12816
			Multiply,
			// Token: 0x04003211 RID: 12817
			Add
		}
	}
}
