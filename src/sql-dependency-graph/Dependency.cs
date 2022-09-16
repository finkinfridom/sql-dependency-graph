namespace sql_dependency_graph;
public class Dependency
{
    public List<SQLScript> WithDependency { get; }
    public List<SQLScript> WithoutDependency { get; }

    public Dependency()
    {
        WithDependency = new List<SQLScript>();
        WithoutDependency = new List<SQLScript>();
    }
}