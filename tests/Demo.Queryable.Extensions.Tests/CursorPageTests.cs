namespace Demo.Queryable.Extensions.Tests;

public sealed class CursorPageTests
{
    private readonly IQueryable<TestPerson> _testData = TestPerson.TestData;

    [Fact]
    public void When_NoCursorProvided_Should_Return_Entire_DataSet_Sorted()
    {
        // Arrange
        Cursor? cursor = null;
        var sort = "Age";
        var expected = _testData
            .OrderBy(p => p.Age)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();


        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_Sort_Is_Key_Should_Return_DataSet_Only_Sorted_By_Key()
    {
        // Arrange
        Cursor? cursor = null;
        var sort = "Id";
        var expected = _testData
            .OrderBy(p => p.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_Sort_Is_DescendingKey_Should_Return_DataSet_Only_Sorted_By_Key_Descending()
    {
        // Arrange
        Cursor? cursor = null;
        var sort = "-Id";
        var expected = _testData
            .OrderByDescending(p => p.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_Sort_Is_NonKey_Should_Return_DataSet_Sorted_By_AscendingField_Then_By_AscendingKey()
    {
        // Arrange
        Cursor? cursor = null;
        var sort = "Age";
        var expected = _testData
            .OrderBy(p => p.Age)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_Sort_Is_DescendingNonKey_Should_Return_DataSet_Sorted_By_DescendingField_Then_By_AscendingKey()
    {
        // Arrange
        Cursor? cursor = null;
        var sort = "-Age";
        var expected = _testData
            .OrderByDescending(p => p.Age)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_CursorProvided_Should_Return_DataSet_After_Cursor_Position()
    {
        // Arrange
        var sort = "Age";
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            "26");

        var expected = _testData
            .Where(p => p.Age > 26)
            .OrderBy(p => p.Age)
            .ThenBy(s => s.Id)
            .ToList();


        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();


        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_Field_Matches_With_Values_In_Dataset_Should_Return_Items_With_FieldValue_And_Greater_Key()
    {
        // Arrange
        var age = 22;
        var id = Guid.Parse("2975ca59-cd2b-4726-b6a9-cff2c8668059");

        var sort = "Age";
        var cursor = new Cursor(
            id.ToString(),
            age.ToString());

        var expected = _testData
            .Where(p =>
                p.Age > age ||
                (p.Age == age && p.Id.CompareTo(id) > 0))
            .OrderBy(p => p.Age)
            .ThenBy(s => s.Id)
            .ToList();


        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_DescendingSort_And_Field_Matches_With_Values_In_Dataset_Should_Return_Items_After_Field_Sorted_Descending()
    {
        // Arrange
        var age = 27;
        var id = Guid.Parse("cd2ed09f-6abb-4ffc-95f0-8fab3b996bb5");

        var sort = "-Age";
        var cursor = new Cursor(
            id.ToString(),
            age.ToString());

        var expected = _testData
            .Where(p => p.Age < age)
            .OrderByDescending(p => p.Age)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_Field_Is_Key_Should_Return_Items_After_Key_Sorted_Ascending()
    {
        // Arrange
        var id = Guid.Parse("83a4a35e-b914-4173-b6db-4d251ce2408b");

        var sort = "Id";
        var cursor = new Cursor(
            id.ToString(),
            id.ToString());

        var expected = _testData
            .Where(p => p.Id.CompareTo(id) > 0)
            .OrderBy(p => p.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_Field_Is_DescendingKey_Should_Return_Items_After_Key_Sorted_Descending()
    {
        // Arrange
        var id = Guid.Parse("83a4a35e-b914-4173-b6db-4d251ce2408b");

        var sort = "-Id";
        var cursor = new Cursor(
            id.ToString(),
            id.ToString());

        var expected = _testData
            .Where(p => p.Id.CompareTo(id) < 0)
            .OrderByDescending(p => p.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_ShortType_Field()
    {
        // Arrange
        var floor = (short)-2;
        var sort = nameof(TestPerson.Floor);
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            floor.ToString());

        var expected = _testData
            .Where(p => p.Floor > floor)
            .OrderBy(p => p.Floor)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_IntType_Field()
    {
        // Arrange
        var postCode = 24480;
        var sort = nameof(TestPerson.PostCode);
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            postCode.ToString());

        var expected = _testData
            .Where(p => p.PostCode > postCode)
            .OrderBy(p => p.PostCode)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_uintType_Field()
    {
        // Arrange
        var houseNumber = 55;
        var sort = nameof(TestPerson.HouseNumber);
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            houseNumber.ToString());

        var expected = _testData
            .Where(p => p.HouseNumber > houseNumber)
            .OrderBy(p => p.HouseNumber)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_LongType_Field()
    {
        // Arrange
        var score = 45678901234;
        var sort = nameof(TestPerson.Score);
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            score.ToString());

        var expected = _testData
            .Where(p => p.Score > score)
            .OrderBy(p => p.Score)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_UlongType_Field()
    {
        // Arrange
        var rank = 4321098764UL;
        var sort = nameof(TestPerson.Rank);
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            rank.ToString());

        var expected = _testData
            .Where(p => p.Rank > rank)
            .OrderBy(p => p.Rank)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_FloatType_Field()
    {
        // Arrange
        var height = 1.74f;
        var sort = nameof(TestPerson.Height);
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            height.ToString());

        var expected = _testData
            .Where(p => p.Height > height)
            .OrderBy(p => p.Height)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_DoubleType_Field()
    {
        // Arrange
        var sort = nameof(TestPerson.Weight);
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            "70.0");

        var expected = _testData
            .Where(p => p.Weight > 70.0)
            .OrderBy(p => p.Weight)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_CharType_Field()
    {
        // Arrange
        var code = 'B';
        var id = Guid.Parse("83a4a35e-b914-4173-b6db-4d251ce2408b");
        var sort = nameof(TestPerson.Code);
        var cursor = new Cursor(
            id.ToString(),
            code.ToString());

        var expected = _testData
            .Where(p =>
                p.Code > code ||
                (p.Code == code && p.Id.CompareTo(id) > 0))
            .OrderBy(p => p.Code)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_BoolType_Field()
    {
        // Arrange
        var married = false;
        var id = Guid.Parse("83a4a35e-b914-4173-b6db-4d251ce2408b");
        var sort = nameof(TestPerson.Married);
        var cursor = new Cursor(
            id.ToString(),
            married.ToString());

        var expected = _testData
            .Where(p =>
                p.Married.CompareTo(married) > 0 ||
                (p.Married == married && p.Id.CompareTo(id) > 0))
            .OrderBy(p => p.Married)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_BoolType_Field_Descending()
    {
        // Arrange
        var married = false;
        var id = Guid.Parse("83a4a35e-b914-4173-b6db-4d251ce2408b");
        var sort = "-" + nameof(TestPerson.Married);
        var cursor = new Cursor(
            id.ToString(),
            married.ToString());

        var expected = _testData
            .Where(p =>
                p.Married.CompareTo(married) < 0 ||
                (p.Married == married && p.Id.CompareTo(id) > 0))
            .OrderByDescending(p => p.Married)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }


    [Fact]
    public void Should_Page_By_StringType_Field()
    {
        // Arrange
        var name = "Max";
        var id = Guid.Parse("eff79a51-f929-414e-8b26-8b3f023a190f");
        var sort = nameof(TestPerson.Name);
        var cursor = new Cursor(
            id.ToString(),
            name.ToString());

        var expected = _testData
            .Where(p =>
                p.Name.CompareTo(name) > 0 ||
                (p.Name == name && p.Id.CompareTo(id) > 0))
            .OrderBy(p => p.Name)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_StringType_Field_Descending()
    {
        // Arrange
        var name = "Max";
        var id = Guid.Parse("eff79a51-f929-414e-8b26-8b3f023a190f");
        var sort = "-" + nameof(TestPerson.Name);
        var cursor = new Cursor(
            id.ToString(),
            name.ToString());

        var expected = _testData
            .Where(p =>
                p.Name.CompareTo(name) < 0 ||
                (p.Name == name && p.Id.CompareTo(id) > 0))
            .OrderByDescending(p => p.Name)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }


    [Fact]
    public void Should_Page_By_DateTimeType_Field()
    {
        // Arrange
        var sort = nameof(TestPerson.HiringDate);
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            "2020-01-01T00:00:00");

        var expected = _testData
            .Where(p => p.HiringDate > new DateTime(2020, 1, 1))
            .OrderBy(p => p.HiringDate)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_KeyValue_Is_Invalid_Should_Throw_Exception()
    {
        // Arrange
        var sort = "Age";
        var cursor = new Cursor(
            "InvalidGuid",
            "26");

        // Act
        var act = () => _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Invalid key value for property 'Id' (Parameter 'cursor')");
    }

    [Fact]
    public void When_TargetValue_Is_Invalid_Should_Throw_Exception()
    {
        // Arrange
        var sort = "Age";
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            "InvalidAge");

        // Act
        var act = () => _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Invalid field value for property 'Age' (Parameter 'cursor')");
    }

    [Fact]
    public void When_Field_Is_UnsupportedType_Should_Throw_Exception()
    {
        // Arrange
        var sort = "Extra";
        var cursor = new Cursor(
            Guid.Empty.ToString(),
            "SomeValue");

        // Act
        var act = () => _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        act.Should()
            .Throw<NotSupportedException>()
            .WithMessage("Unsupported field type for cursor 'Object'");
    }

    [Fact]
    public void When_TargetValueCursor_Is_Null_Should_Return_Items_With_Greater_Key()
    {
        // Arrange
        var id = Guid.Parse("2975ca59-cd2b-4726-b6a9-cff2c8668059");

        var sort = "id";
        var cursor = new Cursor(id.ToString());

        var expected = _testData
            .Where(p => p.Id.CompareTo(id) > 0)
            .OrderBy(p => p.Id)
            .ToList();


        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();


        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void When_TargetValueCursor_Is_Null_And_Sort_Is_DescendingId_Should_Return_Items_With_Greater_Key_Descending()
    {
        // Arrange
        var id = Guid.Parse("a1695dd4-c20f-408d-bc06-a50102bb98bf");

        var sort = "-id";
        var cursor = new Cursor(id.ToString());

        var expected = _testData
            .Where(p => p.Id.CompareTo(id) < 0)
            .OrderByDescending(p => p.Id)
            .ToList();


        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();


        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }


    [Fact]
    public void Should_Page_By_NullableString_Type_Field()
    {
        // Arrange
        var id = Guid.Parse("83a4a35e-b914-4173-b6db-4d251ce2408b");
        var notes = "Annual review pending";
        var sort = nameof(TestPerson.Notes);
        var cursor = new Cursor(
            id.ToString(),
            notes);

        var expected = _testData
            .Where(p =>
                (p.Notes != null && p.Notes.CompareTo(notes) > 0) ||
                (p.Notes == notes && p.Id.CompareTo(id) > 0))
            .OrderBy(p => p.Notes)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_NullableString_Type_Field_Descending()
    {
        // Arrange
        var id = Guid.Parse("a1695dd4-c20f-408d-bc06-a50102bb98bf");
        var notes = "Annual review pending";
        var sort =  "-" + nameof(TestPerson.Notes);
        var cursor = new Cursor(
            id.ToString(),
            notes);

        var expected = _testData
            .Where(p =>
                (p.Notes != null && p.Notes.CompareTo(notes) < 0) ||
                p.Notes == null ||
                (p.Notes == notes && p.Id.CompareTo(id) > 0))
            .OrderByDescending(p => p.Notes)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }


    [Fact]
    public void Should_Page_By_NullableInt_Type_Field()
    {
        // Arrange
        var id = Guid.Parse("83a4a35e-b914-4173-b6db-4d251ce2408b");
        var bonus = 5000;
        var sort = nameof(TestPerson.Bonus);
        var cursor = new Cursor(
            id.ToString(),
            bonus.ToString());

        var expected = _testData
            .Where(p =>
                (p.Bonus != null && p.Bonus.Value.CompareTo(bonus) > 0) ||
                (p.Bonus == bonus && p.Id.CompareTo(id) > 0))
            .OrderBy(p => p.Bonus)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }

    [Fact]
    public void Should_Page_By_NullableInt_Type_Field_Descending()
    {
        // Arrange
        var id = Guid.Parse("a1695dd4-c20f-408d-bc06-a50102bb98bf");
        var bonus = 1250;
        var sort =  "-" + nameof(TestPerson.Bonus);
        var cursor = new Cursor(
            id.ToString(),
            bonus.ToString());

        var expected = _testData
            .Where(p =>
                (p.Bonus != null && p.Bonus.Value.CompareTo(bonus) < 0) ||
                (p.Bonus == bonus && p.Id.CompareTo(id) > 0))
            .OrderByDescending(p => p.Bonus)
            .ThenBy(s => s.Id)
            .ToList();

        // Act
        var page = _testData.CursorPage(s => s.Id, sort, cursor).ToList();

        // Assert
        page.Should().HaveCount(expected.Count);
        page.Should().Equal(expected);
    }
}
