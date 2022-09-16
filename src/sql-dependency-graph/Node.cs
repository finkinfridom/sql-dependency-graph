namespace sql_dependency_graph;
public class Node
{
    public Node()
    {
        Packages = new HashSet<string>();
    }
    public string FileName { get; set; }

    public string SchemaAndObjectName { get; set; }

    public HashSet<string> Packages { get; set; }
}
public class NodeComparer : IEqualityComparer<Node>
{
    public static NodeComparer OrdinalIgnoreCase
    {
        get
        {
            return new NodeComparer();
        }
    }
    public bool Equals(Node one, Node two)
    {
        if (one == null || two == null)
        {
            return false;
        }
        return StringComparer.InvariantCultureIgnoreCase.Equals(one.SchemaAndObjectName, two.SchemaAndObjectName);
    }

    public int GetHashCode(Node item)
    {
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(item.SchemaAndObjectName);
    }
}