namespace Demo.Queryable.Extensions.Tests;

public sealed class CursorTests
{
    private readonly IQueryable<TestPerson> _testData = TestPerson.QueryableData;

    [Theory]
    [InlineData("")]
    [InlineData("TestCursor123")]
    [InlineData(" ")]
    [InlineData("232")]
    [InlineData("KeyValue")]
    public void When_Encoding_And_Decoding_Cursor_Should_Return_Original_KeyValue(string originalKeyValue)
    {
        // Arrange & Act
        var encoded = CursorUtils.Encode(originalKeyValue);
        var result = CursorUtils.TryDecode(encoded, out var cursor);

        // Assert
        result.Should().BeTrue();
        cursor?.KeyValue.Should().Be(originalKeyValue);
        cursor?.TargetValue.Should().BeNull();
    }

    [Theory]
    [InlineData("KeyValue", "TargetValue")]
    [InlineData("", null)]
    [InlineData("TestCursor123", null)]
    [InlineData("", "SortValue456")]
    [InlineData("TestCursor123", "SortValue456")]
    [InlineData("Test11Cursor123", null)]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(" ", null)]
    [InlineData("232", " ")]
    public void When_Encoding_And_Decoding_Cursor_Should_Return_Original_Values(string originalKeyValue, string? originalSortValue)
    {
        // Arrange & Act
        var encoded = CursorUtils.Encode(originalKeyValue, originalSortValue);
        (var keyValue, var targetValue) = encoded.Decode()!.Value;

        // Assert
        keyValue.Should().Be(originalKeyValue);
        targetValue.Should().Be(originalSortValue);
    }

    [Fact]
    public void When_Cursor_Is_Null_On_Decode_Should_Return_Null_KeyValue_And_SortValue()
    {
        // Arrange & Act
        string? query = null;
        var result = CursorUtils.TryDecode(query, out var cursor);

        // Assert
        result.Should().BeTrue();
        cursor?.KeyValue.Should().BeEmpty();
        cursor?.TargetValue.Should().BeNull();
    }

    [Fact]
    public void When_TryDecode_And_Decoding_Invalid_Query_Should_Return_False_And_NullCursor()
    {
        // Arrange
        var invalidQuery = "Invalid@@@Cursor###";

        // Act
        var result = invalidQuery.TryDecode(out var cursor);

        // Assert
        result.Should().BeFalse();
        cursor.Should().BeNull();
    }

    [Fact]
    public void When_Decode_And_Invalid_Query_Should_Throw_FormatException()
    {
        // Arrange
        var invalidQuery = "Invalid@@@Cursor###";

        // Act
        var act = () => { invalidQuery.Decode(); };

        // Assert
        act.Should()
            .Throw<FormatException>()
            .WithMessage("Invalid cursor format");
    }

    [Fact]
    public void Decoding_Using_Implicit_Operator_Should_Return_Original_Values()
    {
        // Arrange
        var originalKeyValue = "TestCursor123";
        var originalSortValue = "SortValue456";

        // Act
        var encoded = CursorUtils.Encode(originalKeyValue, originalSortValue);
        Cursor? cursor = encoded; // Implicit conversion

        // Assert
        cursor?.KeyValue.Should().Be(originalKeyValue);
        cursor?.TargetValue.Should().Be(originalSortValue);
    }

    [Fact]
    public void When_TryDoce_With_Null_Query_Should_Return_NullCursor()
    {
        // Arrange
        string? query = null;

        // Act
        var result = CursorUtils.TryDecode(query, out var cursor);

        // Assert
        result.Should().BeTrue();
        cursor.Should().BeNull();
    }

    [Fact]
    public void Create_Cursor_Only_With_LeyValue()
    {
        // Arrange
        var originalKeyValue = "OnlyKeyValue";

        // Act
        var cursor = new Cursor(originalKeyValue);

        // Assert
        cursor.KeyValue.Should().Be(originalKeyValue);
        cursor.TargetValue.Should().BeNull();
    }

    [Fact]
    public void Create_Cursor_With_KeyValue_And_TargetValue()
    {
        // Arrange
        var originalKeyValue = "KeyValue";
        var originalTargetValue = "TargetValue";

        // Act
        var cursor = new Cursor(originalKeyValue, originalTargetValue);

        // Assert
        cursor.KeyValue.Should().Be(originalKeyValue);
        cursor.TargetValue.Should().Be(originalTargetValue);
    }


    [Fact]
    public void Given_NullEnumerable_When_Encode_Should_Return_Null()
    {
        // Arrange
        IEnumerable<string>? data = null;

        // Act
        var result = data.EncodeCursor(s => s);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Given_EmptyEnumerable_When_Encode_Should_Return_Null()
    {
        // Arrange
        var data = Enumerable.Empty<string>();

        // Act
        var result = data.EncodeCursor(s => s);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Given_NonEmptyEnumerable_When_Encode_Should_Return_EncodedCursor()
    {
        // Arrange
        var lastItem = _testData.Last();
        var keyValue = lastItem.Id.ToString();
        var targetValue = lastItem.Age.ToString();

        // Act
        var result = _testData.EncodeCursor(s => s.Id, "age");
        var decoded = result.Decode()!.Value;

        // Assert
        decoded.KeyValue.Should().Be(keyValue);
        decoded.TargetValue.Should().Be(targetValue);
    }

    [Fact]
    public void Given_NonEmptyEnumerable_When_Encode_Without_TargetSelector_Should_Return_EncodedCursor()
    {
        // Arrange
        var lastItem = _testData.Last();
        var keyValue = lastItem.Id.ToString();

        // Act
        var result = _testData.EncodeCursor(s => s.Id);
        var decoded = result.Decode()!.Value;

        // Assert
        decoded.KeyValue.Should().Be(keyValue);
        decoded.TargetValue.Should().BeNull();
    }

    [Fact]
    public void Given_ListOfPrimitiveValues_When_Use_EncodeCursor_Should_Encode_Only_KeyValue()
    {
        // Arrange
        var data = new List<int> { 1, 2, 3, 4, 5 };
        var expectedKeyValue = "5";

        // Act
        var result = data.EncodeCursor();
        var decoded = result.Decode()!.Value;

        // Assert
        decoded.KeyValue.Should().Be(expectedKeyValue);
        decoded.TargetValue.Should().BeNull();
    }

    [Fact]
    public void Given_EmptyListOfPrimitiveValues_When_Use_EncodeCursor_Should_Return_Null()
    {
        // Arrange
        var data = new List<int>();

        // Act
        var result = data.EncodeCursor();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Given_NullListOfPrimitiveValues_When_Use_EncodeCursor_Should_Return_Null()
    {
        // Arrange
        List<int>? data = null;

        // Act
        var result = data.EncodeCursor();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Given_ListOfNullablePrimitiveValues_When_Use_EncodeCursor_Should_Return_Null()
    {
        // Arrange
        var data = new List<int?> { 1, 2, null, 4, 5, null };

        // Act
        var result = data.EncodeCursor();

        // Assert
        result.Should().BeNull();
    }
}
