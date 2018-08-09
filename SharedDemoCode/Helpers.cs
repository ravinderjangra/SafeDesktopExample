using Microsoft.Win32;
using SafeApp;
using SafeApp.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace App
{
    public static class Helpers
    {
        public static string UrlFormat(string appId, string encodedString, bool toAuthenticator)
        {
            var scheme = toAuthenticator ? "safe-auth" : $"{appId}";
            return $"{scheme}://{encodedString}";
        }

        public static string GetRequestData(string url)
        {
            return new Uri(url).PathAndQuery.Replace("/", "");
        }

        public static async Task<(uint, string)> GenerateEncodedAppRequestAsync(string appId)
        {
            Console.WriteLine("Generating application authentication request");
            var authReq = new AuthReq
            {
                AppContainer = true,
                App = new AppExchangeInfo { Id = appId, Scope = "", Name = "SAFE Console", Vendor = "MaidSafe.net Ltd" },
                Containers = new List<ContainerPermissions> { new ContainerPermissions { ContName = "_publicNames", Access = { Insert = true } } }
            };

            return await Session.EncodeAuthReqAsync(authReq);
        }

        public static void RegisterAppProtocol(string appPath, string appId)  //appPath = full path to your application
        {
            Console.WriteLine("Registering Apps URL Protocol in Registry");
            //open App's protocol's subkey
            RegistryKey Mainkey = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);

            char[] padding = { '=' };
            string appUrl = "safe-" + Convert.ToBase64String(appId.ToUtfBytes().ToArray())
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_');

            var key = Mainkey.OpenSubKey(appUrl, true);

            if (key == null)  //if the protocol is not registered yet...we register it
            {
                key = Mainkey.CreateSubKey(appUrl);
                key.SetValue(string.Empty, "URL: dotUrlRegister Protocol");
                key.SetValue("URL Protocol", string.Empty);

                key = key.CreateSubKey(@"shell\open\command");
                key.SetValue(string.Empty, appPath + " " + "%1");
                //%1 represents the argument - this tells windows to open this program with an argument / parameter
            }
            key.Close();
        }

        public static string GetRandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
