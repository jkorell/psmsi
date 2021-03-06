﻿// The MIT License (MIT)
//
// Copyright (c) Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Microsoft.Tools.WindowsInstaller.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Tools.WindowsInstaller.PowerShell.Commands
{
    /// <summary>
    /// The Edit-MSIPackage cmdlet.
    /// </summary>
    [Cmdlet(VerbsData.Edit, "MSIPackage", DefaultParameterSetName = ParameterSet.Path)]
    public sealed class EditPackageCommand : ItemCommandBase
    {
        private string orcaPath = null;

        /// <summary>
        /// Gets or sets whether to wait for Orca to close before processing the next item.
        /// </summary>
        [Parameter]
        public SwitchParameter Wait { get; set; }

        /// <summary>
        /// Ges the path to Orca if installed; otherwise, displays a warning.
        /// </summary>
        protected override void BeginProcessing()
        {
            this.orcaPath = ComponentSearcher.Find(ComponentSearcher.KnownComponent.Orca);
            if (string.IsNullOrEmpty(this.orcaPath))
            {
                this.WriteWarning(Resources.Error_OrcaAbsent);
            }
        }

        /// <summary>
        /// Attempts to open the item in Orca, if installed; otherwise, tries to invoke the "edit" verb on the package.
        /// </summary>
        /// <param name="item">The <see cref="PSObject"/> representing a package to open.</param>
        protected override void ProcessItem(PSObject item)
        {
            string path = item.GetPropertyValue<string>("PSPath");
            path = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);

            // Make sure the item is an MSI or MSP package.
            var type = FileInfo.GetFileTypeInternal(path);
            if (FileType.Package != type && FileType.Patch != type)
            {
                var message = string.Format(CultureInfo.CurrentCulture, Resources.Error_InvalidStorage, path);
                var ex = new PSInvalidOperationException(message);
                this.WriteError(ex.ErrorRecord);

                return;
            }

            var info = new ProcessStartInfo()
            {
                WorkingDirectory = System.IO.Path.GetDirectoryName(path),
            };

            if (!string.IsNullOrEmpty(this.orcaPath))
            {
                // Open in Orca, if installed.
                info.FileName = this.orcaPath;
                info.Arguments = "\"" + path + "\"";
            }
            else
            {
                // Try to use the edit verb instead.
                info.FileName = path;
                info.UseShellExecute = true;
                info.Verb = "edit";
            }

            Process process = null;
            try
            {
                process = Process.Start(info);
                if (this.Wait)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException || ex is Win32Exception)
                {
                    // Likely the "edit" verb is not supported so terminate.
                    var pse = new PSInvalidOperationException(ex.Message, ex);
                    this.ThrowTerminatingError(pse.ErrorRecord);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (null != process)
                {
                    process.Dispose();
                }
            }
        }
    }
}
