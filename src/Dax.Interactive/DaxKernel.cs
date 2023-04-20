// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// using System;
using System.Linq;
using Microsoft.Identity.Client;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Formatting.TabularData;
using Enumerable = System.Linq.Enumerable;
using System.CommandLine.Parsing;
using System;

namespace Dax.Interactive;

public class DaxKernel :
    Kernel,
    IKernelCommandHandler<SubmitCode>
{
    private readonly string _connectionString;
    private bool _isConnected;
    private string _accessToken;
    /// <summary>
    /// The set of query result lists to save for sharing later.
    /// The key will be the name of the value.
    /// The value is a list of result sets (multiple if multiple queries are ran as a batch)
    /// </summary>
    protected Dictionary<string, IReadOnlyCollection<TabularDataResource>> QueryResults { get; } = new();
    /// <summary>
    /// Used to store incoming variables passed in via #!share
    /// </summary>
    private readonly Dictionary<string, object> _variables = new(System.StringComparer.Ordinal);
    public DaxKernel(string name, string connectionString) : base(name, "dax")
    {
        _connectionString = connectionString;
    }

    private async Task<AdomdConnection> GetConnection(KernelInvocationContext context)
    {
        var connection = new AdomdConnection();
        try
        {
            connection.ConnectionString = _connectionString;
            connection.Open();
            _isConnected = true;
        }
        catch (ArgumentException ex)
        {
            var token = await MsalHelper.AcquireTokenInteractiveAsync((string)null, (string)null, context.CancellationToken);
            connection.ConnectionString = $"{_connectionString};Password={token.AccessToken};";
            connection.Open();
            _isConnected = false;
            throw new Exception("Error connecting to the tabular model", ex);
        }
        catch (Exception ex)
        {
            _isConnected = false;
            throw new Exception("Error connecting to the server", ex);
        }
        connection.Open();
        return connection;
    }

    public virtual async Task HandleAsync(
        SubmitCode submitCode,
        KernelInvocationContext context)
    {



        AdomdCommand cmd = new AdomdCommand(submitCode.Code);
        cmd.Connection = await GetConnection(context);

        await Task.Run(() =>
        {

            using (var reader = cmd.ExecuteReader())
            {
                var results = new List<TabularDataResource>();
                var values = new object[reader.FieldCount];
                var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
                DaxKernelUtils.AliasDuplicateColumnNames(names);

                // holds the result of a single statement within the query
                var table = new List<(string, object)[]>();

                while (reader.Read())
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
        });

    }

    protected virtual void StoreQueryResults(IReadOnlyCollection<TabularDataResource> results, ParseResult commandKernelChooserParseResult)
    {
    }

}
