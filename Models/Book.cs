namespace WebApiFood.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public decimal? Rating { get; set; }

        public string? Status {get; set; }

    }
}
