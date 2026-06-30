[string]$CurrentDirectory = (Get-Location).Path;
[string]$AppName = [System.IO.Path]::GetFileName($CurrentDirectory);
[string]$ExecutablePath = Join-Path $CurrentDirectory "$AppName.exe";

if (-not (Test-Path $ExecutablePath)) {
    throw "Executable '$ExecutablePath' was not found. Run this script from the published application directory.";
}

[System.Security.AccessControl.FileSystemSecurity]$Acl = Get-Acl $CurrentDirectory;
[System.Security.AccessControl.FileSystemAccessRule]$AccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("System", "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow");
$Acl.SetAccessRule($AccessRule);
$Acl | Set-Acl $CurrentDirectory;

New-Service -Name $AppName -BinaryPathName $ExecutablePath -Description $AppName -DisplayName $AppName -StartupType Automatic;
