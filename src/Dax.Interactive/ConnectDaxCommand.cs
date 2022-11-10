// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Connection;

namespace Dax.Interactive
{
    public class ConnectDaxCommand : ConnectKernelCommand
    {
        public ConnectDaxCommand()
            : base("dax", "Connects to a tabular model")
        {
            Add(ConnectionStringArgument);
        }

        public Argument<string> ConnectionStringArgument { get; } =
            new("connectionString", "The connection string used to connect to the tabular model");

        public override Task<Kernel> ConnectKernelAsync(
            KernelInvocationContext context,
            InvocationContext commandLineContext)
        {
            var connectionString = commandLineContext.ParseResult.GetValueForArgument(ConnectionStringArgument);
            var connector = new DaxKernelConnector(connectionString);
            var localName = commandLineContext.ParseResult.GetValueForOption(KernelNameOption);
            return connector.CreateKernelAsync(localName);
        }
    }
}