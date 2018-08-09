using SafeApp;
#if SAFE_APP_MOCK
using SafeApp.MockAuthBindings;
#endif
using SafeApp.Utilities;
using System;
using System.Threading.Tasks;

namespace App.Network
{
    public class Authentication
    {
        public static string AppId { get; set; }

        public static async Task RequestAuthentication(string appId)
        {
            Console.WriteLine("Requesting authentication from Peruse");
            AppId = appId;

#if SAFE_APP_MOCK
            //Generating random credentials
            var location = Helpers.GetRandomString(10);
            var password = Helpers.GetRandomString(10);
            var invitation = Helpers.GetRandomString(15);
            var authenticator = await Authenticator.CreateAccountAsync(location, password, invitation);
            authenticator = await Authenticator.LoginAsync(location, password);

            //Authentication and Logging
            var (_, reqMsg) = await Helpers.GenerateEncodedAppRequestAsync(appId);
            var ipcReq = await authenticator.DecodeIpcMessageAsync(reqMsg);
            var authIpcReq = ipcReq as AuthIpcReq;
            var resMsg = await authenticator.EncodeAuthRespAsync(authIpcReq, true);
            var ipcResponse = await Session.DecodeIpcMessageAsync(resMsg);
            var authResponse = ipcResponse as AuthIpcMsg;
            DataOperations.InitilizeSession(await Session.AppRegisteredAsync(appId, authResponse.AuthGranted));
            await DataOperations.PerformMDataOperations();
#else
            var encodedReq = await Helpers.GenerateEncodedAppRequestAsync(appId);
            var url = Helpers.UrlFormat(appId, encodedReq.Item2, true);
            System.Diagnostics.Process.Start(url);
#endif
        }

        public static async Task ProcessAuthenticationResponse(string authResponse)
        {
            try
            {
                var encodedRequest = Helpers.GetRequestData(authResponse);
                var decodeResult = await Session.DecodeIpcMessageAsync(encodedRequest);
                if (decodeResult.GetType() == typeof(AuthIpcMsg))
                {
                    var ipcMsg = decodeResult as AuthIpcMsg;
                    Console.WriteLine("Auth Reqest Granted from Authenticator");
                    //Create session object
                    if (ipcMsg != null)
                    {
                        DataOperations.InitilizeSession(await Session.AppRegisteredAsync(AppId, ipcMsg.AuthGranted));
                        await DataOperations.PerformMDataOperations();
                    }
                }
                else
                {
                    Console.WriteLine("Auth Request is not Granted");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }
    }
}
