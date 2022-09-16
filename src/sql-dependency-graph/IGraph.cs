namespace sql_dependency_graph;

public interface IGraph
{
    List<Package> Find(List<Package> packages, string entrypointToken);

    List<Package> BuildDependency(List<Package> packages, Dictionary<string, List<string>> withDependency);
}