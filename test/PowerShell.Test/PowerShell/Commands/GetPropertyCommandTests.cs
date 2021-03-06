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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Management.Automation;

namespace Microsoft.Tools.WindowsInstaller.PowerShell.Commands
{
    /// <summary>
    /// Tests for the <see cref="GetPropertyCommand"/> class.
    /// </summary>
    [TestClass]
    public class GetPropertyCommandTests : TestBase
    {
        [TestMethod]
        public void GetAllProperties()
        {
            using (var p = CreatePipeline(@"get-msiproperty -path example.msi"))
            {
                var output = p.Invoke();
                Assert.AreEqual<int>(0, p.Error.Count);
                Assert.AreEqual<int>(7, output.Count());
            }
        }

        [TestMethod]
        public void GetMatchingProperties()
        {
            using (var p = CreatePipeline(@"get-msiproperty Product*, UpgradeCode -path example.msi"))
            {
                var output = p.Invoke();
                Assert.AreEqual<int>(0, p.Error.Count);
                Assert.AreEqual<int>(5, output.Count());
            }
        }

        [TestMethod]
        public void GetAllPatchProperties()
        {
            using (var p = CreatePipeline(@"get-msiproperty -path example.msp"))
            {
                var output = p.Invoke();
                Assert.AreEqual<int>(0, p.Error.Count);
                Assert.AreEqual<int>(7, output.Count());
                Assert.IsTrue(output.Any(obj => "Update".Equals(obj.GetPropertyValue<string>("Value"))));
            }
        }

        [TestMethod]
        public void PassThruAllProperties()
        {
            using (var p = CreatePipeline(@"get-item example.msi | get-msiproperty -passthru"))
            {
                var output = p.Invoke();
                Assert.AreEqual<int>(0, p.Error.Count);
                Assert.AreEqual<int>(1, output.Count());

                var obj = output[0];
                Assert.IsInstanceOfType(obj.BaseObject, typeof(System.IO.FileInfo));

                var propertySet = obj.Members.Match("MSIProperties", PSMemberTypes.PropertySet).FirstOrDefault() as PSPropertySet;
                Assert.IsNotNull(propertySet);

                var properties = obj.Properties.Where(property => propertySet.ReferencedPropertyNames.Contains(property.Name));
                Assert.AreEqual<int>(7, properties.Count());
                Assert.AreEqual("{877EF582-78AF-4D84-888B-167FDC3BCC11}", output[0].GetPropertyValue<string>("ProductCode"));
            }
        }

        [TestMethod]
        public void PassThruPatchProperties()
        {
            using (var p = CreatePipeline(@"get-item example.msi | get-msiproperty PATCH* -patch example.msp -passthru"))
            {
                var output = p.Invoke();
                Assert.AreEqual<int>(1, output.Count());

                var obj = output[0];
                Assert.IsInstanceOfType(obj.BaseObject, typeof(System.IO.FileInfo));
                Assert.AreEqual("{FF63D787-26E2-49CA-8FAA-28B5106ABD3A}", obj.GetPropertyValue<string>("PATCHNEWPACKAGECODE"));
                Assert.AreEqual("TEST", obj.GetPropertyValue<string>("PATCHNEWSUMMARYCOMMENTS"));
                Assert.AreEqual("TEST", obj.GetPropertyValue<string>("PATCHNEWSUMMARYSUBJECT"));
            }
        }
    }
}
