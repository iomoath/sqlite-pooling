namespace Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public User Author { get; set; }
    }
}
