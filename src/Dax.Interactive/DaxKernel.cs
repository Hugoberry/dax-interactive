// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.OleDb;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Formatting.TabularData;
using Enumerable = System.Linq.Enumerable;

namespace Dax.Interactive
{
    public class DaxKernel :
        Kernel,
        IKernelCommandHandler<SubmitCode>
    {
        private readonly string _connectionString;
        private IEnumerable<IEnumerable<IEnumerable<(string name, object value)>>> _tables;

        public DaxKernel(string name, string connectionString) : base(name, "dax")
        {
            _connectionString = connectionString;
        }

        private OleDbConnection OpenConnection()
        {
            return new OleDbConnection(_connectionString);
        }

        public virtual async Task HandleAsync(
            SubmitCode submitCode,
            KernelInvocationContext context)
        {
            using (var connection = OpenConnection())
            {
                connection.Open();

                using (var dbCommand = new OleDbCommand(submitCode.Code, connection))
                {

                    using (var reader = dbCommand.ExecuteReader())
                    {
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
                        var explorer = DataExplorer.CreateDefault(tabularDataResource);
                        context.Display(explorer);
                    }
                }
            }

        }

    }

    public class SqlRow : Dictionary<string, object>
    {
    }
}