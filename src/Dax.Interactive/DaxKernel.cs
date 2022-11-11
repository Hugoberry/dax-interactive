// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Formatting.TabularData;
using Enumerable = System.Linq.Enumerable;
using System.CommandLine.Parsing;


namespace Dax.Interactive
{
    public class DaxKernel :
        Kernel,
        IKernelCommandHandler<SubmitCode>
    {
        private readonly string _connectionString;
        /// <summary>
        /// The set of query result lists to save for sharing later.
        /// The key will be the name of the value.
        /// The value is a list of result sets (multiple if multiple queries are ran as a batch)
        /// </summary>
        protected Dictionary<string, IReadOnlyCollection<TabularDataResource>> QueryResults { get; } = new();
        /// <summary>
        /// Used to store incoming variables passed in via #!share
        /// </summary>
        private readonly Dictionary<string, object> _variables = new(StringComparer.Ordinal);
        public DaxKernel(string name, string connectionString) : base(name, "dax")
        {
            _connectionString = connectionString;
        }


        public virtual async Task HandleAsync(
            SubmitCode submitCode,
            KernelInvocationContext context)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();

                using (var dbCommand = new OleDbCommand(submitCode.Code, connection))
                {

                    using (var reader = dbCommand.ExecuteReader())
                    {
                        var results = new List<TabularDataResource>();
                        var values = new object[reader.FieldCount];
                        var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
                        DaxKernelUtils.AliasDuplicateColumnNames(names);

                        // holds the result of a single statement within the query
                        var table = new List<(string, object)[]>();

                        while (await reader.ReadAsync())
                        {
                            reader.GetValues(values);
                            var row = new (string, object)[values.Length];
                            for (var i = 0; i < values.Length; i++)
                            {
                                row[i] = (names[i], values[i]);
                            }

                            table.Add(row);

                        }

                        var tabularDataResource = table.ToTabularDataResource();
                        results.Add(tabularDataResource);
                        var explorer = DataExplorer.CreateDefault(tabularDataResource);
                        context.Display(explorer);
                        StoreQueryResults(results, submitCode.KernelChooserParseResult);
                    }
                }
            }

        }

        protected virtual void StoreQueryResults(IReadOnlyCollection<TabularDataResource> results, ParseResult commandKernelChooserParseResult)
        {
        }

    }
}