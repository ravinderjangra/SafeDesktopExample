using SafeApp;
using SafeApp.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Network
{
    public class DataOperations
    {
        private static Session _session { get; set; }
        private static MDataInfo _mdinfo;

        public static void InitilizeSession(Session session)
        {
            _session = session;
        }

        public static async Task InitializeLogging()
        {
            await Session.InitLoggingAsync(@"D:\GitHub\Exp\UrlRegister-Framework\UrlRegister-Framework\bin\x64\Debug");
        }

        internal static async Task PerformMDataOperations()
        {
            await CreateMutableData();
            await AddEntries();
            await ReadEntries();
            await UpdateEntry();
            await DeleteEntry();

            Console.WriteLine("\nUpdated MData Values");
            await ReadEntries();
        }

        private static async Task CreateMutableData()
        {
            const ulong tagType = 15010;
            _mdinfo = await _session.MDataInfoActions.RandomPublicAsync(tagType);

            var mDataPermissionSet = new PermissionSet { Insert = true, ManagePermissions = true, Read = true, Update = true, Delete = true };
            using (var permissionsH = await _session.MDataPermissions.NewAsync())
            {
                using (var appSignKeyH = await _session.Crypto.AppPubSignKeyAsync())
                {
                    await _session.MDataPermissions.InsertAsync(permissionsH, appSignKeyH, mDataPermissionSet);
                    await _session.MData.PutAsync(_mdinfo, permissionsH, NativeHandle.Zero);
                }
            }
        }

        private static async Task AddEntries()
        {
            using (var entryActionsH = await _session.MDataEntryActions.NewAsync())
            {
                for (int i = 0; i < 5; i++)
                {
                    var actKey = "key" + i;
                    var actValue = "value" + i;
                    var key = Encoding.ASCII.GetBytes(actKey).ToList();
                    var value = Encoding.ASCII.GetBytes(actValue).ToList();
                    await _session.MDataEntryActions.InsertAsync(entryActionsH, key, value);
                }
                await _session.MData.MutateEntriesAsync(_mdinfo, entryActionsH);
            }
        }
        
        private static async Task ReadEntries()
        {
            using (var entriesHandle = await _session.MDataEntries.GetHandleAsync(_mdinfo))
            {
                var entries = await _session.MData.ListEntriesAsync(entriesHandle);
                foreach (var entry in entries)
                {
                    var key = entry.Key.Val;
                    var value = entry.Value.Content;

                    if (value.Count == 0)
                        continue;

                    Console.WriteLine("Key : " + key.ToUtfString());
                    Console.WriteLine("Value : " + value.ToUtfString());
                }
            }
        }

        private static async Task UpdateEntry()
        {
            var keys = await _session.MData.ListKeysAsync(_mdinfo);
            var keyToUpdate = keys[new Random().Next(5)];
            var newValue = "NewDataValue";
            using (var entriesHandle = await _session.MDataEntryActions.NewAsync())
            {
                var value = await _session.MData.GetValueAsync(_mdinfo, keyToUpdate.Val);
                await _session.MDataEntryActions.UpdateAsync(entriesHandle, keyToUpdate.Val, newValue.ToUtfBytes(), value.Item2 + 1);
                await _session.MData.MutateEntriesAsync(_mdinfo, entriesHandle);
            }
        }

        private static async Task DeleteEntry()
        {
            var keys = await _session.MData.ListKeysAsync(_mdinfo);
            var keyToDelete = keys[new Random().Next(5)];
            using (var entriesHandle = await _session.MDataEntryActions.NewAsync())
            {
                var value = await _session.MData.GetValueAsync(_mdinfo, keyToDelete.Val);
                await _session.MDataEntryActions.DeleteAsync(entriesHandle, keyToDelete.Val, value.Item2 + 1);
                await _session.MData.MutateEntriesAsync(_mdinfo, entriesHandle);
            }
        }
    }
}
