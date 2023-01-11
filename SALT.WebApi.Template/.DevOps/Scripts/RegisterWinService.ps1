[string]$currentDir = (Get-Location).Path;
[string]$exe = $currentDir + "\SALT.WebApi.Example.exe";
[System.Security.AccessControl.FileSystemSecurity]$acl = Get-Acl $currentDir;
[System.Security.AccessControl.FileSystemAccessRule]$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("System", "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow");
$acl.SetAccessRule($accessRule);
$acl | Set-Acl $currentDir;

New-Service -Name "SALT.WebApi.Example" -BinaryPathName $exe -Description "Example service" -DisplayName "SALT.WebApi.Example" -StartupType Automatic