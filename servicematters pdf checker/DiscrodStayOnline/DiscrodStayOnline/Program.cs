using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace DiscrodStayOnline
{
    internal class Program
    {
        public static void Main (string[] args) 
        {
            var cts = new System.Threading.CancellationToken();
            
            var a = new ClientWebSocket();
            a.ConnectAsync(new Uri("wss://gateway.discord.gg/?v=10&encoding=json"), cts).Wait();
            StringBuilder message = new StringBuilder();
            string c = "{\\\"op\\\":2,\\\"d\\\":{\\\"token\\\":\\\"OTkwNzExMDQ1MjUxMjg5MTE4.G4G9RS.u3LYjL0BKaMc-Q5GdYs9iDnEc6OgXGXYtL6_FQ\\\",\\\"capabilities\\\":509,\\\"properties\\\":{\\\"os\\\":\\\"Windows\\\",\\\"browser\\\":\\\"Chrome\\\",\\\"device\\\":\\\"\\\",\\\"system_locale\\\":\\\"ru-RU\\\",\\\"browser_user_agent\\\":\\\"Mozilla\\/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit\\/537.36 (KHTML, like Gecko) Chrome\\/103.0.0.0 Safari\\/537.36\\\",\\\"browser_version\\\":\\\"103.0.0.0\\\",\\\"os_version\\\":\\\"10\\\",\\\"referrer\\\":\\\"\\\",\\\"referring_domain\\\":\\\"\\\",\\\"referrer_current\\\":\\\"\\\",\\\"referring_domain_current\\\":\\\"\\\",\\\"release_channel\\\":\\\"stable\\\",\\\"client_build_number\\\":135402,\\\"client_event_source\\\":null},\\\"presence\\\":{\\\"status\\\":\\\"online\\\",\\\"since\\\":0,\\\"activities\\\":[],\\\"afk\\\":false},\\\"compress\\\":false,\\\"client_state\\\":{\\\"guild_hashes\\\":{},\\\"highest_last_message_id\\\":\\\"0\\\",\\\"read_state_version\\\":0,\\\"user_guild_settings_version\\\":-1,\\\"user_settings_version\\\":-1}}}";
            var sendBuffer = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(c.ToString()));
            a.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cts).Wait();
            string c1 = "{\"op\":4,\"d\":{\"guild_id\":null,\"channel_id\":null,\"self_mute\":true,\"self_deaf\":false,\"self_video\":false}}";
            sendBuffer = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(c1.ToString()));
            a.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cts).Wait();
            string c2 = "{\"op\":1,\"d\":3}";
            sendBuffer = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(c2.ToString()));
            a.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cts).Wait();
        }




    }
}
