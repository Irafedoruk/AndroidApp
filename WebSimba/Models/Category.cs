namespace WebSimba.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }

        // Зв'язок один до багатьох з продуктами
        public List<Product> Products { get; set; }
    }
}
