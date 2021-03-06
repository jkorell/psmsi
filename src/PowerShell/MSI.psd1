# Copyright (C) Microsoft Corporation. All rights reserved.
#
# THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
# KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
# IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
# PARTICULAR PURPOSE.

@{
GUID = '0D5A8A07-2E6E-432F-86E2-605D4EEAA9A8'
Author = 'Heath Stewart'
CompanyName = 'Microsoft Corporation'
Description = 'Exposes Windows Installer functionality to Windows PowerShell'
Copyright = 'Copyright Microsoft Corporation'
ModuleVersion = '2.3.1.1'
PowerShellVersion = '2.0'
CLRVersion = '2.0'
ModuleToProcess = 'MSI.psm1'
NestedModules = 'Microsoft.Tools.WindowsInstaller.PowerShell.dll'
FormatsToProcess = 'MSI.formats.ps1xml'
TypesToProcess = 'MSI.types.ps1xml'
RequiredAssemblies = 'Microsoft.Tools.WindowsInstaller.PowerShell.dll', 'Microsoft.Deployment.WindowsInstaller.dll', 'Microsoft.Deployment.WindowsInstaller.Package.dll'
FileList = 'about_MSI.help.txt', 'Microsoft.Tools.WindowsInstaller.PowerShell.dll-Help.xml'
PrivateData = @{
  PSData = @{
    Tags = @('WindowsInstaller', 'MSI', 'MSP')
    LicenseUri = 'http://bit.ly/psmsi-license'
    ProjectUri = 'http://bit.ly/psmsi-home'
    IronUri = 'http://bit.ly/psmsi-icon'
  }
}
}
