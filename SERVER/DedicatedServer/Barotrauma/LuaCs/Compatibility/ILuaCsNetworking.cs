using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Compatibility
{
	// Token: 0x0200048D RID: 1165
	internal interface ILuaCsNetworking : ILuaCsShim, IService, IDisposable
	{
		// Token: 0x06003E29 RID: 15913
		void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData);

		// Token: 0x17001062 RID: 4194
		// (get) Token: 0x06003E2A RID: 15914
		// (set) Token: 0x06003E2B RID: 15915
		ushort LastClientListUpdateID { get; set; }

		// Token: 0x06003E2C RID: 15916
		void HttpRequest(string url, LuaCsAction callback, string data = null, string method = "POST", string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x06003E2D RID: 15917
		void HttpPost(string url, LuaCsAction callback, string data, string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x06003E2E RID: 15918
		void HttpGet(string url, LuaCsAction callback, Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x06003E2F RID: 15919
		void RequestGetHTTP(string url, LuaCsAction callback, Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x06003E30 RID: 15920
		void RequestPostHTTP(string url, LuaCsAction callback, string data, string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null);

		// Token: 0x06003E31 RID: 15921
		void Receive(string netId, LuaCsAction action);

		// Token: 0x17001063 RID: 4195
		// (get) Token: 0x06003E32 RID: 15922
		// (set) Token: 0x06003E33 RID: 15923
		int FileSenderMaxPacketsPerUpdate { get; set; }

		// Token: 0x06003E34 RID: 15924
		void ClientWriteLobby(Client client);

		// Token: 0x06003E35 RID: 15925
		void UpdateClientPermissions(Client client);

		// Token: 0x06003E36 RID: 15926
		IWriteMessage Start();

		// Token: 0x06003E37 RID: 15927
		void Send(IWriteMessage mesage, NetworkConnection connection = null, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable);
	}
}
