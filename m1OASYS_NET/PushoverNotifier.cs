using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace m1OASYS_NET
{
    public static class PushoverNotifier
    {
        public static async Task<bool> SendAsync(
            string token,
            string userKey,
            string message)
        {
            try
            {
                using (HttpClient client =
                    new HttpClient())
                {
                    var values =
                        new Dictionary<string, string>
                        {
                            { "token", token },
                            { "user", userKey },
                            { "message", message }
                        };

                    var content =
                        new FormUrlEncodedContent(values);

                    HttpResponseMessage response =
                        await client.PostAsync(
                            "https://api.pushover.net/1/messages.json",
                            content);

                    string result =
    await response.Content.ReadAsStringAsync();

                    //System.Windows.Forms.MessageBox.Show($"HTTP {(int)response.StatusCode}\r\n\r\n{result}");

                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
