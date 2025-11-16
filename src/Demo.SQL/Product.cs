namespace Demo.SQL;

public record Product(
    int Id,
    string Name,
    int Quantity,
    Guid Code);
