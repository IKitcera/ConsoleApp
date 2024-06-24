using ConsoleApp.Models.Interfaces;

namespace ConsoleApp.Models;

public record Product : IPrimaryEntity {
  public long Id { get; set; }
  public string Name { get; set; }
  public string Description { get; set; }
  public decimal Price { get; set; }
  
  public Product(long id, string name, string description, decimal price) {
    Id = id;
    Name = name;
    Description = description;
    Price = price;
  }
}
