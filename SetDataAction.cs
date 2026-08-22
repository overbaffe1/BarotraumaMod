using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A2 RID: 674
	internal class SetDataAction : EventAction
	{
		// Token: 0x06003AB0 RID: 15024 RVA: 0x00220303 File Offset: 0x0021E503
		public SetDataAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x17000F75 RID: 3957
		// (get) Token: 0x06003AB1 RID: 15025 RVA: 0x0022030D File Offset: 0x0021E50D
		// (set) Token: 0x06003AB2 RID: 15026 RVA: 0x00220315 File Offset: 0x0021E515
		[Serialize(SetDataAction.OperationType.Set, IsPropertySaveable.Yes, "Do you want to set the metadata to a specific value, multiply it, or add to it.", "", false)]
		public SetDataAction.OperationType Operation { get; set; }

		// Token: 0x17000F76 RID: 3958
		// (get) Token: 0x06003AB3 RID: 15027 RVA: 0x0022031E File Offset: 0x0021E51E
		// (set) Token: 0x06003AB4 RID: 15028 RVA: 0x00220326 File Offset: 0x0021E526
		[Serialize(null, IsPropertySaveable.Yes, "Depending on the operation, the value you want to set the metadata to, multiply it with, or add to it.", "", false)]
		public string Value { get; set; }

		// Token: 0x17000F77 RID: 3959
		// (get) Token: 0x06003AB5 RID: 15029 RVA: 0x0022032F File Offset: 0x0021E52F
		// (set) Token: 0x06003AB6 RID: 15030 RVA: 0x00220337 File Offset: 0x0021E537
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the metadata to set. Can be any arbitrary identifier, e.g. itemscollected, my_custom_event_state, specialnpckilled...", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x06003AB7 RID: 15031 RVA: 0x00220340 File Offset: 0x0021E540
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003AB8 RID: 15032 RVA: 0x00220348 File Offset: 0x0021E548
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003AB9 RID: 15033 RVA: 0x00220354 File Offset: 0x0021E554
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

		// Token: 0x06003ABA RID: 15034 RVA: 0x002203B0 File Offset: 0x0021E5B0
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

		// Token: 0x06003ABB RID: 15035 RVA: 0x0022050C File Offset: 0x0021E70C
		private static float? ConvertValueToFloat(object value)
		{
			if (value is float || value is int)
			{
				return (float?)Convert.ChangeType(value, typeof(float));
			}
			return null;
		}

		// Token: 0x06003ABC RID: 15036 RVA: 0x00220548 File Offset: 0x0021E748
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

		// Token: 0x06003ABD RID: 15037 RVA: 0x00220578 File Offset: 0x0021E778
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

		// Token: 0x04001E25 RID: 7717
		private bool isFinished;

		// Token: 0x02000F2C RID: 3884
		public enum OperationType
		{
			// Token: 0x040054D5 RID: 21717
			Set,
			// Token: 0x040054D6 RID: 21718
			Multiply,
			// Token: 0x040054D7 RID: 21719
			Add
		}
	}
}
