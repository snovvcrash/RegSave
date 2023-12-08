# RegSave

A .NET application that will dump SAM, SYSTEM, SECURITY registry keys from a remote host to a remote path of your choosing.

## Help

```
C:\>RegSave.exe --help

  -t, --Target        Required. Remote machine name
  -o, --OutputPath    Registry hives output directory path
  --backup            (Default: false) Use REG_OPTION_BACKUP_RESTORE flag for RegOpenKeyEx
  --acl               (Default: false) Show ACL for registry key SYSTEM\ControlSet001\Control\SecurePipeServers\winreg
                      and exit
  --help              Display this help screen.
```

## Demo

![demo.png](/assets/demo.png)

## References

- [mpgn/BackupOperatorToDA](https://github.com/mpgn/BackupOperatorToDA)
