namespace sql_dependency_graph;
using System.Text.RegularExpressions;
public class Graph : IGraph
{
    public List<Package> Find(List<Package> packages, string entrypointToken)
    {
        var comparer = NodeComparer.OrdinalIgnoreCase;
        var map = new Dictionary<string, HashSet<Node>>(StringComparer.OrdinalIgnoreCase);
        var rootSources = new List<Node>();
        var schemas = string.Join("|", packages.SelectMany(p => p.Scripts?.Where(s => !string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Name.Split('.')[0])).Distinct());
        var re = new Regex($"[^\\w_@#](?<schema>{schemas})[\\]]?\\.[\\[]?(?<source>[\\w\\d\\$]*)[\\]]?", RegexOptions.IgnoreCase);
        foreach (var pkg in packages)
        {
            foreach (var script in pkg.Scripts)
            {
                var sources = script.Body;
                var sourcesNoCommentedRows = Regex.Replace(sources, @"(--.*)", string.Empty);

                var destinationFileName = script.Name;
                var nameParts = destinationFileName.Split('.');
                var destination = new Node
                {
                    FileName = destinationFileName,
                    SchemaAndObjectName = $"{nameParts[0]}.{nameParts[1]}"
                };
                destination.Packages.Add(pkg.Name);

                if (sources.IndexOf(entrypointToken, StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    rootSources.Add(destination);
                }

                var matches = re.Matches(sourcesNoCommentedRows);
                foreach (Match m in matches)
                {
                    var schema = m.Groups["schema"].Value;
                    var source = m.Groups["source"].Value;
                    var schemaAndObject = $"{schema}.{source}";
                    if (!map.ContainsKey(schemaAndObject))
                    {
                        map.Add(schemaAndObject, new HashSet<Node>(comparer));
                    }

                    map[schemaAndObject].Add(destination);
                    map[schemaAndObject].FirstOrDefault(d => comparer.Equals(d, destination))?.Packages.Add(pkg.Name);
                }
            }
        }

        var withDependency = new Dictionary<string, List<string>>();
        if (rootSources.Count == 0)
        {
            foreach (var pkg in packages)
            {
                withDependency.Add(pkg.Name, Enumerable.Empty<string>().ToList());
            }
            return BuildDependency(packages, withDependency);
        }


        var nodes = new Dictionary<Node, int>(new NodeComparer());
        int depth = 0;
        foreach (var rootSource in rootSources)
        {
            navigateGraph(map, rootSource, nodes, depth);
        }
        foreach (var pkg in packages)
        {
            var fileNamesWithinPackage = nodes.Where(n => n.Key.Packages.Any(p => p.Equals(pkg.Name, StringComparison.InvariantCultureIgnoreCase))).OrderBy(n => n.Value).ThenBy(n => n.Key.FileName).Select(n => n.Key.FileName);
            withDependency.Add(pkg.Name, fileNamesWithinPackage.ToList());
        }
        return BuildDependency(packages, withDependency);
    }

    public List<Package> BuildDependency(List<Package> packages, Dictionary<string, List<string>> withDependency)
    {
        var result = new List<Package>();
        foreach (var eachPackage in packages)
        {
            var dependency = new Dependency();
            var scripts = eachPackage.Scripts.OrderBy(s => withDependency[eachPackage.Name].IndexOf(s.Name));

            foreach (var eachScript in scripts)
            {
                if (withDependency[eachPackage.Name].Contains(eachScript.Name))
                {
                    dependency.WithDependency.Add(eachScript);
                }
                else
                {
                    dependency.WithoutDependency.Add(eachScript);
                }
            }
            eachPackage.Dependency = dependency;
            result.Add(eachPackage);
        }
        return result;
    }

    private static void navigateGraph(Dictionary<string, HashSet<Node>> map, Node source, IDictionary<Node, int> visitedNodes, int depth)
    {
        if (visitedNodes.ContainsKey(source))
        {
            visitedNodes[source] = Math.Min(visitedNodes[source], depth);
            return;
        }
        visitedNodes.Add(source, depth);
        if (!map.ContainsKey(source.SchemaAndObjectName))
        {
            return;
        }
        var destinations = map[source.SchemaAndObjectName];
        foreach (var destination in destinations)
        {
            if (visitedNodes.ContainsKey(destination))
            {
                continue;
            }
            navigateGraph(map, destination, visitedNodes, ++depth);
        }
        return;
    }
}