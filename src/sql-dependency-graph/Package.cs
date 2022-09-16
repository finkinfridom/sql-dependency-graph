namespace sql_dependency_graph;
public class Package
{
    public string Name { get; set; }
    public List<SQLScript> Scripts { get; set; }

    public Dependency Dependency { get; set; }
    public Package()
    {
        Scripts = new List<SQLScript>();
        Dependency = new Dependency();
    }
}