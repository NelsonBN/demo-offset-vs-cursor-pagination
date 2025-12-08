using System.Diagnostics;

namespace Demo.Queryable.Extensions.Tests;

[DebuggerDisplay("[{Id}] {Name} ({Age}) - {Salary}€")]
public record TestPerson
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public ushort Age { get; init; }
    public decimal Salary { get; init; }

    public short Floor { get; init; }

    public int PostCode { get; init; }
    public uint HouseNumber { get; init; }

    public long Score { get; init; }
    public ulong Rank { get; init; }

    public float Height { get; init; }
    public double Weight { get; init; }

    public bool Married { get; init; }
    public DateTime HiringDate { get; init; }
    public char Code { get; init; }

    public object? Extra { get; init; }

    public string? Notes { get; init; }
    public int? Bonus { get; init; }

    public static readonly IQueryable<TestPerson> TestData = new List<TestPerson>
    {
        new()
        {
            Id = Guid.Parse("eff79a51-f929-414e-8b26-8b3f023a190f"),
            Name = "Max",
            Age = 28,
            Married = false,
            Salary = 12_345.67m,
            Floor = 3,
            PostCode = 12345,
            HouseNumber = 12,
            Score = 1234567890,
            Rank = 9876543210,
            Height = 1.80f,
            Weight = 75.5,
            HiringDate = new DateTime(2020, 1, 15),
            Code = 'A',
            Extra = new { Comment = "Top performer" }
        },
        new()
        {
            Id = Guid.Parse("83a4a35e-b914-4173-b6db-4d251ce2408b"),
            Name = "Alice",
            Age = 30,
            Married = true,
            Salary = 5_4578.32m,
            Floor = 5,
            PostCode = 54321,
            HouseNumber = 34,
            Score = 2345678901,
            Rank = 8765432109,
            Height = 1.65f,
            Weight = 60.0,
            HiringDate = new DateTime(2019, 6, 1),
            Code = 'B',
            Notes = "Part-time",
        },
        new()
        {
            Id = Guid.Parse("759f492f-7302-46c9-9c36-d898200bee00"),
            Name = "Morgan",
            Age = 22,
            Married = true,
            Salary = 6_123.45m,
            Floor = 2,
            PostCode = 67890,
            HouseNumber = 56,
            Score = 3456789012,
            Rank = 7654321098,
            Height = 1.75f,
            Weight = 68.2,
            HiringDate = new DateTime(2021, 3, 20),
            Code = 'C',
            Bonus = 1500
        },
        new()
        {
            Id = Guid.Parse("3a0153a2-9979-449f-99ac-adb19891572e"),
            Name = "Charlie",
            Age = 24,
            Married = false,
            Salary = 7_890.12m,
            Floor = 4,
            PostCode = 13579,
            HouseNumber = 78,
            Score = 4567890123,
            Rank = 6543210987,
            Height = 1.82f,
            Weight = 80.0,
            HiringDate = new DateTime(2018, 11, 5),
            Code = 'D'
        },
        new()
        {
            Id = Guid.Parse("a1695dd4-c20f-408d-bc06-a50102bb98bf"),
            Name = "Bob",
            Age = 25,
            Married = false,
            Salary = 6_789.01m,
            Floor = 1,
            PostCode = 24680,
            HouseNumber = 90,
            Score = 5678901234,
            Rank = 5432109876,
            Height = 1.78f,
            Weight = 72.3,
            HiringDate = new DateTime(2022, 7, 30),
            Code = 'E',
            Notes = "Annual review pending"
        },
        new()
        {
            Id = Guid.Parse("2975ca59-cd2b-4726-b6a9-cff2c8668059"),
            Name = "Adrian",
            Age = 22,
            Married = false,
            Salary = 9_512.34m,
            Floor = 2,
            PostCode = 11223,
            HouseNumber = 45,
            Score = 6789012345,
            Rank = 4321098765,
            Height = 1.70f,
            Weight = 65.0,
            HiringDate = new DateTime(2021, 5, 10),
            Code = 'F'
        },
        new()
        {
            Id = Guid.Parse("9f0bca82-dc26-44dc-a38e-176b7e735fac"),
            Name = "Jordan",
            Age = 27,
            Married = false,
            Salary = 4_321.00m,
            Floor = 3,
            PostCode = 33445,
            HouseNumber = 67,
            Score = 7890123456,
            Rank = 3210987654,
            Height = 1.85f,
            Weight = 85.7,
            HiringDate = new DateTime(2017, 9, 25),
            Code = 'G'
        },
        new()
        {
            Id = Guid.Parse("cd2ed09f-6abb-4ffc-95f0-8fab3b996bb5"),
            Name = "Alex",
            Age = 27,
            Married = true,
            Salary = 3_954.54m,
            Floor = 4,
            PostCode = 55667,
            HouseNumber = 89,
            Score = 8901234567,
            Rank = 2109876543,
            Height = 1.68f,
            Weight = 59.8,
            HiringDate = new DateTime(2016, 12, 12),
            Code = 'H'
        },
    }.AsQueryable();
}
