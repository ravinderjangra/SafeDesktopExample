using App;
using App.Network;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiveNetworkExample
{
    class Program
    {
        private static Mutex mutex = null;
        const string appName = "SafeConsoleApp";
        const string appId = "net.safe.console";
        private static bool _firstApplicationInstance;

        static async Task Main()
        {
            string[] args = Environment.GetCommandLineArgs();
            Console.WriteLine("SafeNetwork Console Application");

            if (IsApplicationFirstInstance())
            {
                //DataOperations.InitializeLogging();

                //args[0] is always the path to the application
                Helpers.RegisterAppProtocol(args[0], appId);
                //^the method posted before, that edits registry

                // Create a new pipe - it will return immediately and async wait for connections
                PipeComm.NamedPipeServerCreateServer();

                await Authentication.RequestAuthentication(appId);

                //DataOperations.GetAccountInformationAsyn();
            }
            else
            {

                // We are not the first instance, send the named pipe message with our payload and stop loading
                if (args.Length >= 2)
                {
                    // We are not the first instance, send the named pipe message with our payload and stop loading
                    var namedPipePayload = new NamedPipePayload
                    {
                        SignalQuit = false,
                        Arguments = args[1]
                    };

                    // Send the message
                    PipeComm.NamedPipeClientSendOptions(namedPipePayload);
                }
                // Close app
                return;
            }

            Console.ReadLine();
        }

        private static bool IsApplicationFirstInstance()
        {
            // Allow for multiple runs but only try and get the mutex once
            if (mutex == null)
            {
                mutex = new Mutex(true, appName, out _firstApplicationInstance);
            }
            return _firstApplicationInstance;
        }
    }
}
