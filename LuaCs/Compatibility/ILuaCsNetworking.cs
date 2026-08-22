using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Compatibility
{
	// Token: 0x02000570 RID: 1392
	internal interface ILuaCsNetworking : ILuaCsShim, IService, IDisposable
	{
		// Token: 0x060055E6 RID: 21990
		void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData);

		// Token: 0x17001538 RID: 5432
		// (get) Token: 0x060055E7 RID: 21991
		// (set) Token: 0x060055E8 RID: 21992
		ushort LastClientListUpdateID { get; set; }

		// Token: 0x060055E9 RID: 21993
		void HttpRequest(string url, LuaCsAction callback, string data = null, string method = "POST", string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x060055EA RID: 21994
		void HttpPost(string url, LuaCsAction callback, string data, string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x060055EB RID: 21995
		void HttpGet(string url, LuaCsAction callback, Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x060055EC RID: 21996
		void RequestGetHTTP(string url, LuaCsAction callback, Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x060055ED RID: 21997
		void RequestPostHTTP(string url, LuaCsAction callback, string data, string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x060055EE RID: 21998
		void Receive(string netId, LuaCsAction action);

		// Token: 0x060055EF RID: 21999
		void Send(IWriteMessage mesage, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable);
	}
}
