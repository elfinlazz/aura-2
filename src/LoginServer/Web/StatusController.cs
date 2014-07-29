// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json;
using SharpExpress;
using System.IO;
using System.Net.Mime;

namespace Aura.Login.Web
{
	public class StatusController : IController
	{
		public void Index(Request req, Response res)
		{
			var status = this.CreateStatusText();

			res.ContentType = MediaTypeNames.Text.Plain;
			res.Send(status);
		}

		private string CreateStatusText()
		{
			using (var sw = new StringWriter())
			using (var jsonWriter = new JsonTextWriter(sw))
			{
				jsonWriter.WriteStartObject();
				{
					// Login
					jsonWriter.WritePropertyName("login");
					jsonWriter.WriteStartObject();
					{
						jsonWriter.WritePropertyName("port");
						jsonWriter.WriteValue(LoginServer.Instance.Conf.Login.Port);
					}
					jsonWriter.WriteEndObject();

					// Servers
					jsonWriter.WritePropertyName("servers");
					jsonWriter.WriteStartObject();
					{
						foreach (var server in LoginServer.Instance.ServerList.List)
						{
							// Channels
							jsonWriter.WritePropertyName(server.Name);
							jsonWriter.WriteStartObject();
							{
								foreach (var channel in server.Channels)
								{
									// Channel
									jsonWriter.WritePropertyName(channel.Key);
									jsonWriter.WriteStartObject();
									{
										jsonWriter.WritePropertyName("host");
										jsonWriter.WriteValue(channel.Value.Host);

										jsonWriter.WritePropertyName("port");
										jsonWriter.WriteValue(channel.Value.Port);

										jsonWriter.WritePropertyName("online");
										jsonWriter.WriteValue(channel.Value.Users);

										jsonWriter.WritePropertyName("onlineMax");
										jsonWriter.WriteValue(channel.Value.MaxUsers);

										jsonWriter.WritePropertyName("state");
										jsonWriter.WriteValue(channel.Value.State);
									}
								}
							}
						}
					}
					jsonWriter.WriteEndObject();
				}
				jsonWriter.WriteEndObject();

				return sw.ToString();
			}
		}
	}
}
