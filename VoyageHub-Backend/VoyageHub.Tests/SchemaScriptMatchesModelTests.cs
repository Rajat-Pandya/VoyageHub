using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using VoyageHub.Data;
using Xunit;

namespace VoyageHub.Tests;

public class SchemaScriptMatchesModelTests
{
    [Fact]
    public void AzureSchemaScript_ContainsAllMappedTablesAndColumns()
    {
        var repoRoot = FindRepoRoot();
        var scriptPath = Path.Combine(repoRoot, "database", "VoyageHub_Schema.azure.sql");
        Assert.True(File.Exists(scriptPath), $"Azure schema script not found at: {scriptPath}");

        var sql = File.ReadAllText(scriptPath);
        var parsedTables = ParseCreateTables(sql);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SchemaCheck;Trusted_Connection=True;")
            .Options;

        using var context = new AppDbContext(options);
        var missing = new List<string>();

        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (string.IsNullOrWhiteSpace(tableName))
            {
                continue;
            }

            if (!parsedTables.TryGetValue(tableName, out var columns))
            {
                missing.Add($"Table '{tableName}'");
                continue;
            }

            var storeId = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
            foreach (var prop in entityType.GetProperties())
            {
                var colName = prop.GetColumnName(storeId);
                if (!string.IsNullOrWhiteSpace(colName) && !columns.Contains(colName))
                {
                    missing.Add($"{tableName}.{colName}");
                }
            }
        }

        Assert.True(missing.Count == 0, "Missing tables/columns in database/VoyageHub_Schema.azure.sql: " + string.Join(", ", missing));
    }

    private static Dictionary<string, HashSet<string>> ParseCreateTables(string sql)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var tableRegex = new Regex(
            @"CREATE\s+TABLE\s+(?:dbo\.)?\[?(?<tableName>[A-Za-z0-9_]+)\]?\s*\((?<body>.*?)\)\s*;",
            RegexOptions.IgnoreCase | RegexOptions.Singleline,
            TimeSpan.FromSeconds(2));

        foreach (Match match in tableRegex.Matches(sql))
        {
            var tableName = match.Groups["tableName"].Value;
            var body = match.Groups["body"].Value;
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rawLine in body.Split('\n'))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("--", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
                    Regex.IsMatch(line, @"^CHECK\s*\(", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                {
                    continue;
                }

                var colMatch = Regex.Match(line, @"^\[?(?<col>[A-Za-z0-9_]+)\]?\s+", RegexOptions.None, TimeSpan.FromSeconds(1));
                if (colMatch.Success)
                {
                    cols.Add(colMatch.Groups["col"].Value);
                }
            }

            result[tableName] = cols;
        }

        return result;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "database", "VoyageHub_Schema.azure.sql")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing database/VoyageHub_Schema.azure.sql.");
    }
}
