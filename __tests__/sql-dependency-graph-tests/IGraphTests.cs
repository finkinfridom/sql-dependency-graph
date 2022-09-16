using Xunit;

using sql_dependency_graph;

namespace sql_dependency_graph_tests;

public class IGraphTests : IDisposable
{
    private List<Package> packages;
    private SQLScript myTableScript = new SQLScript
    {
        Name = "dbo.MyTable.sql",
        Body = @"
                            use [master]
                            go
                            select 1 from dbo.MyOtherTable with(nolock)
                            where 1 = 1
                            go"
    };
    private SQLScript MyOtherTableScript = new SQLScript
    {
        Name = "dbo.MyOtherTable.sql",
        Body = @"
                            use [entrypointToken]
                            go
                            select 1 from dbo.MyThirdTable with(nolock)
                            where 1 = 1
                            go"
    };
    private Graph graph;

    public IGraphTests()
    {
        graph = new Graph();
        packages = new List<Package>();
        packages.Add(new Package
        {
            Name = "main"
        });
    }

    public void Dispose()
    {
        packages.Clear();
    }

    [Fact]
    public void DependencyShouldBeEmptyAtBeginning()
    {
        var firstPackage = packages.FirstOrDefault();
        Assert.Equal(0, firstPackage.Dependency.WithDependency.Count);
        Assert.Equal(0, firstPackage.Dependency.WithoutDependency.Count);
    }

    [Fact]
    public void MyTableScriptShouldBeInWithoutDependency()
    {
        var firstPackage = packages.FirstOrDefault();
        firstPackage.Scripts = new List<SQLScript>{
                myTableScript
            };
        graph.Find(packages, "entrypointToken");
        Assert.Equal(0, firstPackage.Dependency.WithDependency.Count);
        Assert.Equal(1, firstPackage.Dependency.WithoutDependency.Count);
        var firstScript = firstPackage.Dependency.WithoutDependency.FirstOrDefault();
        Assert.Equal(firstScript.Name, myTableScript.Name);
    }

    [Fact]
    public void ScriptsShouldBeInWithDependency()
    {
        var firstPackage = packages.FirstOrDefault();
        firstPackage.Scripts = new List<SQLScript>{
                myTableScript,
                MyOtherTableScript
            };
        graph.Find(packages, "entrypointToken");
        Assert.Equal(2, firstPackage.Dependency.WithDependency.Count);
        Assert.Equal(0, firstPackage.Dependency.WithoutDependency.Count);
    }
}