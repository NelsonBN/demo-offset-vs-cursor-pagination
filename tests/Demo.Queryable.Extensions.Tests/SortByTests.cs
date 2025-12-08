namespace Demo.Queryable.Extensions.Tests;

public sealed class SortByTests
{
    private readonly IQueryable<TestPerson> _testData = TestPerson.QueryableData;


    [Fact]
    public void When_Given_PlusPrefix_And_StringProperty_Should_Sort_Ascending()
    {
        // Arrange & Act
        var act = _testData.SortBy("+Name").ToList();

        // Assert
        act.Should().BeInAscendingOrder(s => s.Name);
    }

    [Fact]
    public void When_Given_MinusPrefix_And_GuidProperty_Should_Sort_Descending()
    {
        // Arrange & Act
        var act = _testData.SortBy("-Id").ToList();

        // Assert
        act.Should().BeInDescendingOrder(s => s.Id);
    }

    [Fact]
    public void When_Given_UIntProperty_Should_Sort_Ascending()
    {
        // Arrange & Act
        var act = _testData.SortBy("Age").ToList();

        // Assert
        act.Should().BeInAscendingOrder(s => s.Age);
    }

    [Fact]
    public void When_Given_BoolProperty_Should_Sort_Descending()
    {
        // Arrange & Act
        var act = _testData.SortBy("-Married").ToList();

        // Assert
        act.Should().BeInDescendingOrder(s => s.Married);
    }

    [Fact]
    public void When_Given_DoubleProperty_Should_Sort_Descending()
    {
        // Arrange & Act
        var act = _testData.SortBy("-Salary").ToList();

        // Assert
        act.Should().BeInDescendingOrder(s => s.Salary);
    }

    [Fact]
    public void When_Given_PropertyName_Uppercase_Should_Be_Case_Insensitive()
    {
        // Arrange & Act
        var act = _testData.SortBy("+NAME").ToList();

        // Assert
        act.Should().BeInAscendingOrder(s => s.Name);
    }

    [Fact]
    public void When_Given_SameProperty_MultipleTimes_With_Different_Directions_Should_Sort_Correctly()
    { // This test ensures that caching doesn't interfere with sorting
        // Arrange & Act
        var ordered1 = _testData.SortBy("+Salary").ToList();
        var ordered2 = _testData.SortBy("-Salary").ToList();
        var ordered3 = _testData.SortBy("Salary").ToList();
        var ordered4 = _testData.SortBy("+Salary").ToList();
        var ordered5 = _testData.SortBy("-Salary").ToList();

        // Assert
        ordered1.Should().BeInAscendingOrder(s => s.Salary);
        ordered2.Should().BeInDescendingOrder(s => s.Salary);
        ordered3.Should().BeInAscendingOrder(s => s.Salary);
        ordered4.Should().BeInAscendingOrder(s => s.Salary);
        ordered5.Should().BeInDescendingOrder(s => s.Salary);
    }

    [Fact]
    public void When_Sort_Same_Property_With_Different_Directions_InParallel_Should_Work_Correctly()
    {
        // Arrange
        var sorts = new[] { "+Salary", "-Salary", "Salary", "Salary", "-salary", "SALARY", "-salary", "+salary" };

        // Act
        var results = sorts
            .AsParallel()
            .Select(sort => _testData.SortBy(sort).ToList()).ToList();

        // Assert
        results[0].Should().BeInAscendingOrder(s => s.Salary); // +Salary
        results[1].Should().BeInDescendingOrder(s => s.Salary); // -Salary
        results[2].Should().BeInAscendingOrder(s => s.Salary); // Salary
        results[3].Should().BeInAscendingOrder(s => s.Salary); // Salary
        results[4].Should().BeInDescendingOrder(s => s.Salary); // -salary
        results[5].Should().BeInAscendingOrder(s => s.Salary); // SALARY
        results[6].Should().BeInDescendingOrder(s => s.Salary); // -salary
        results[7].Should().BeInAscendingOrder(s => s.Salary); // +salary
    }
}
