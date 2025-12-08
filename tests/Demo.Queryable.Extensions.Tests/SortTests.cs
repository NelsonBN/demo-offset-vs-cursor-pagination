namespace Demo.Queryable.Extensions.Tests;

public sealed class SortTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void When_TryParse_And_Query_Isnt_Defined_Should_Return_False_And_DefaultSort(string? field)
    {
        // Arrange & Act
        var result = field.TryParseSort(out var sort);

        // Assert
        result.Should().BeFalse();
        sort.PropertyName.ToString().Should().BeEmpty();
        sort.IsAscending.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void When_Parse_And_Sort_Isnt_Defined_Should_Return_Default_And_IsAscending_True(string? field)
    {
        // Arrange & Act
        var (propertyName, isAscending) = field.ParseSort("xpto");

        // Assert
        propertyName.ToString().Should().Be("xpto");
        isAscending.Should().BeTrue();
    }

    [Fact]
    public void When_Parse_With_Default_And_Sort_Is_Defined_Should_Return_Field()
    {
        // Arrange & Act
        var (propertyName, isAscending) = "-abc".ParseSort("xpto");

        // Assert
        propertyName.ToString().Should().Be("abc");
        isAscending.Should().BeFalse();
    }

    [Theory]
    [InlineData("  ", "  ", true)]
    [InlineData(" +Name ", " +Name ", true)]
    [InlineData(" -Date ", " -Date ", true)]
    [InlineData(" - Date ", " - Date ", true)]
    [InlineData("+Name", "Name", true)]
    [InlineData("-Date", "Date", false)]
    [InlineData("Age", "Age", true)]
    [InlineData("+", "", true)]
    [InlineData("+test", "test", true)]
    [InlineData("+ test", " test", true)]
    [InlineData("+name ", "name ", true)]
    [InlineData("-", "", false)]
    [InlineData("-test", "test", false)]
    [InlineData("- test", " test", false)]
    [InlineData("-name ", "name ", false)]
    [InlineData("  test  ", "  test  ", true)]
    public void TestCases_ParseSort(string? field, string expectedPropertyName, bool expectedIsAscending)
    {
        // Arrange & Act
        var (propertyName, isAscending) = field.ParseSort();

        // Assert
        propertyName.ToString().Should().Be(expectedPropertyName);
        isAscending.Should().Be(expectedIsAscending);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void When_Sort_Isnt_Defined_Should_Throw_ArgumentException(string? field)
    {
        // Arrange & Act
        var act = () => { field.ParseSort(); };

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Sort needs to be defined (Parameter 'sort')");
    }


    [Fact]
    public void When_TryParse_PassingType_And_NonexistentProperty_Should_Throw_ArgumentException()
    {
        // Arrange & Act
        var result = "+NonExistentProperty".TryParseSort<TestPerson>(out var sort);

        // Assert
        result.Should().BeFalse();
        sort.PropertyName.ToString().Should().BeEmpty();
        sort.IsAscending.Should().BeTrue();
        sort.PropertyInfo.Should().BeNull();
    }

    [Fact]
    public void When_Parse_PassingType_And_NonexistentProperty_Should_Throw_ArgumentException()
    {
        // Arrange & Act
        var act = () => { "+NonExistentProperty".ParseSort<TestPerson>(); };

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Property '+NonExistentProperty' not found on type 'TestPerson' (Parameter 'sort')");
    }

    [Fact]
    public void When_TryParse_PassingType_And_Property_Existent_Should_Return_True_Sort_And_PropertyInfo()
    {
        // Arrange & Act
        var result = "+name".TryParseSort<TestPerson>(out var sort);

        // Assert
        result.Should().BeTrue();
        sort.PropertyName.ToString().Should().Be("name");
        sort.IsAscending.Should().BeTrue();
        sort.PropertyInfo.Name.Should().Be(nameof(TestPerson.Name));
    }

    [Fact]
    public void When_Parse_PassingType_And_Property_Existent_Should_Return_Sort_And_PropertyInfo()
    {
        // Arrange & Act
        (var propertyName, var isAscending, var propertyInfo) = "-age".ParseSort<TestPerson>();

        // Assert
        propertyName.ToString().Should().Be("age");
        isAscending.Should().BeFalse();
        propertyInfo.Name.Should().Be(nameof(TestPerson.Age));
    }

    [Fact]
    public void Parse_String_To_Sort()
    {
        // Arrange
        var field = "-Salary";

        // Act
        Sort sort = field;

        // Assert
        sort.PropertyName.ToString().Should().Be("Salary");
        sort.IsAscending.Should().BeFalse();
    }

    [Fact]
    public void Parse_String_To_SortTyped()
    {
        // Arrange
        var field = "-Salary";

        // Act
        Sort<TestPerson> sort = field;

        // Assert
        sort.PropertyName.ToString().Should().Be("Salary");
        sort.IsAscending.Should().BeFalse();
        sort.PropertyInfo.Name.Should().Be(nameof(TestPerson.Salary));
    }
}
