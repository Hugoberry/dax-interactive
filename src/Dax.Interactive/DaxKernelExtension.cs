// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;

namespace Dax.Interactive;

public class DaxKernelExtension : IKernelExtension
{
    public Task OnLoadAsync(Kernel kernel)
    {
        if (kernel is CompositeKernel compositeKernel)
        {

                compositeKernel
                    .AddKernelConnector(new ConnectDaxCommand());

            KernelInvocationContext.Current?.Display(
                new HtmlString(@"<details><summary>Query Tabular models.</summary>
    <p>This extension adds support for connecting to Tabular models using the <code>#!connect dax</code> magic command. For more information, run a cell using the <code>#!sql</code> magic command.</p>
    </details>"),
                "text/html");

        }

        return Task.CompletedTask;
    }
}