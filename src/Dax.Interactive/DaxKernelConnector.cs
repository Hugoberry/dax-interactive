// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Connection;

namespace Dax.Interactive;

public class DaxKernelConnector : IKernelConnector
{
    public DaxKernelConnector(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public Task<Kernel> CreateKernelAsync(string kernelName)
    {
        var kernel = new DaxKernel(
            $"sql-{kernelName}",
            ConnectionString);

        return Task.FromResult<Kernel>(kernel);
    }
}