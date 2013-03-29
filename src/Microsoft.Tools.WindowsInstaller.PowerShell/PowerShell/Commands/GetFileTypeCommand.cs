﻿// Cmdlet to get the storage class type for a file, optionally passing
// the PSObject back through the pipeline with a new NoteProperty.
//
// Copyright (C) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;

namespace Microsoft.Tools.WindowsInstaller.PowerShell.Commands
{
    /// <summary>
    /// The Get-MSIFileType cmdlet.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "MSIFileType", DefaultParameterSetName = ParameterSet.Path)]
    public sealed class GetFileTypeCommand : ItemCommandBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="GetFileTypeCommand"/> class.
        /// </summary>
        public GetFileTypeCommand() : base()
        {
        }

        /// <summary>
        /// Processes the item enumerated by the base class.
        /// </summary>
        /// <param name="item">The <see cref="PSObject"/> to process.</param>
        protected override void ProcessItem(PSObject item)
        {
            if (this.PassThru)
            {
                this.WriteObject(item);
            }
            else if (null != item.Properties["MSIFileType"])
            {
                try
                {
                    // Return the file type from ETS.
                    this.WriteObject(item.Properties["MSIFileType"].Value);
                }
                catch (GetValueInvocationException ex)
                {
                    this.WriteError(ex.ErrorRecord);
                }
                catch (PSNotSupportedException ex)
                {
                    this.WriteError(ex.ErrorRecord);
                }
            }
        }
    }
}
