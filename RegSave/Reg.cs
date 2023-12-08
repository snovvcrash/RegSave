using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using Microsoft.Win32;

using static RegSave.Interop;

namespace RegSave
{
    internal class Reg
    {
        static readonly UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        static readonly int KEY_READ = 0x20019;
        //static int KEY_ALL_ACCESS = 0xF003F;
        static readonly int REG_OPTION_OPEN_LINK = 0x0008;
        static readonly int REG_OPTION_BACKUP_RESTORE = 0x0004;
        //static int KEY_QUERY_VALUE = 0x1;

        static readonly Dictionary<string, string> KNOWN_SIDS = new Dictionary<string, string>()
        {
            { "S-1-5-19", "LocalService" },
            { "S-1-5-32-544", @"BUILTIN\Administrators" },
            { "S-1-5-32-551", @"BUILTIN\Backup Operators" }
        };

        public static void ExportRegKey(string remoteMachine, string key, string outputFilePath, bool backupOperators = false)
        {
            var result = RegConnectRegistry($@"\\{remoteMachine}", HKEY_LOCAL_MACHINE, out UIntPtr hHKLM);
            if (result != 0)
            {
                Console.WriteLine($"[-] RegConnectRegistry: {result}");
                return;
            }

            UIntPtr hKey;
            if (backupOperators)
                result = RegOpenKeyEx(hHKLM, key, REG_OPTION_BACKUP_RESTORE | REG_OPTION_OPEN_LINK, KEY_READ, out hKey);
            else
                result = RegOpenKeyEx(hHKLM, key, REG_OPTION_OPEN_LINK, KEY_READ, out hKey);

            if (result != 0)
            {
                Console.WriteLine($"[-] RegOpenKeyEx: {result}");
                return;
            }
            result = RegSaveKey(hKey, outputFilePath, IntPtr.Zero);
            if (result != 0)
            {
                Console.WriteLine($"[-] RegSaveKey: {result}");
                RegCloseKey(hKey);
                return;
            }

            RegCloseKey(hKey);
            Console.WriteLine($@"[+] Exported \\{remoteMachine}\HKLM\{key} to {outputFilePath}");
        }

        public static void GetAcl(string remoteMachine, string registryPath)
        {
            RegistryKey remoteKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, remoteMachine);
            RegistryKey subKey = remoteKey.OpenSubKey(registryPath);

            if (subKey == null)
            {
                Console.WriteLine($"[-] OpenSubKey: {registryPath}");
                remoteKey.Close();
                return;
            }

            DisplayAccessRules(subKey.GetAccessControl());

            subKey.Close();
            remoteKey.Close();
        }

        private static void DisplayAccessRules(RegistrySecurity registrySecurity)
        {
            AuthorizationRuleCollection accessRules = registrySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

            foreach (RegistryAccessRule rule in accessRules)
            {
                try
                {
                    Console.WriteLine($"[*] Identity: {KNOWN_SIDS[rule.IdentityReference.ToString()]}");
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine($"[*] Identity: {rule.IdentityReference}");
                }
                Console.WriteLine($@"   \_ Access Type: {rule.AccessControlType}");
                Console.WriteLine($@"   \_ Registry Rights: {rule.RegistryRights}");
                Console.WriteLine($@"   \_ Inherited: {rule.IsInherited}");
                Console.WriteLine();
            }
        }
    }
}
