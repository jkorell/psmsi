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

using Microsoft.Deployment.WindowsInstaller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Management.Automation;

namespace Microsoft.Tools.WindowsInstaller.PowerShell.Commands
{
    /// <summary>
    /// Functional tests for <see cref="ClearSourceCommand"/>.
    /// </summary>
    /// <remarks>
    /// It seems - at least with Windows Installer 5.0 - that APIs which modify the source list use RPC with msiexec.exe
    /// which my current mock registry implementation can't mock, so modification tests are attempted against an unmanaged product
    /// if one is installed.
    /// </remarks>
    [TestClass]
    public class SourceCommandTests : TestBase
    {
        [TestMethod]
        public void ModifyProductSource()
        {
            PSObject obj = null;

            // First attempt to see if there are any unmanaged products installed.
            using (var p = CreatePipeline("get-msiproductinfo -context userunmanaged"))
            {
                var output = p.Invoke();
                if (null != output && 0 < output.Count())
                {
                    obj = output[0];
                }
                else
                {
                    Assert.Inconclusive("There are no unmanaged products installed with which to test source list modifications.");
                }
            }

            // Retain the original source locations so we can attempt to restore it.
            var product = obj.As<ProductInstallation>();
            var original = product.SourceList.ToArray();

            try
            {
                using (var p = CreatePipeline(string.Format(@"$Input | add-msisource -path '{0}' -passthru", this.TestContext.DeploymentDirectory)))
                {
                    p.Input.Write(obj);
                    var output = p.Invoke();

                    Assert.IsNotNull(output);
                    Assert.AreEqual<int>(original.Length + 1, output.Count());
                }

                using (var p = CreatePipeline(@"$Input | add-msisource -path 'ShouldNotExist.txt' -passthru"))
                {
                    ExceptionAssert.Throws<CmdletInvocationException, ItemNotFoundException>(() =>
                    {
                        p.Input.Write(obj);
                        var output = p.Invoke();
                    });
                }

                var path = Path.Combine(this.TestContext.DeploymentDirectory, "Example.txt");
                using (var p = CreatePipeline(string.Format(@"$Input | add-msisource -path '{0}' -passthru", path)))
                {
                    p.Input.Write(obj);
                    var output = p.Invoke();

                    // Should return the previous number of source locations we already registered.
                    Assert.IsNotNull(output);
                    Assert.AreEqual<int>(original.Length + 1, output.Count());
                    Assert.AreEqual<int>(1, p.Error.Count);
                }

                using (var p = CreatePipeline(@"$Input | clear-msisource; $Input | get-msisource"))
                {
                    p.Input.Write(obj);
                    var output = p.Invoke();

                    Assert.IsTrue(null == output || 0 == output.Count());
                }

                var paths = new string[original.Length + 1];
                paths[0] = this.TestContext.DeploymentDirectory;
                original.CopyTo(paths, 1);

                using (var p = CreatePipeline(string.Format(@"$Input | add-msisource '{0}' -passthru", product.ProductCode)))
                {
                    p.Input.Write(paths, true);
                    var output = p.Invoke();

                    Assert.IsNotNull(output);
                    Assert.AreEqual<int>(paths.Length, output.Count());
                }

                using (var p = CreatePipeline(string.Format(@"$Input | remove-msisource -path '{0}' -passthru", this.TestContext.DeploymentDirectory)))
                {
                    p.Input.Write(obj);
                    var output = p.Invoke();

                    Assert.IsNotNull(output);
                    Assert.AreEqual<int>(original.Length, output.Count());
                }
            }
            finally
            {
                // Restore only the original source locations.
                product.SourceList.ClearNetworkSources();
                product.SourceList.ClearUrlSources();

                foreach (var source in original)
                {
                    product.SourceList.Add(source);
                }
            }
        }
    }
}
