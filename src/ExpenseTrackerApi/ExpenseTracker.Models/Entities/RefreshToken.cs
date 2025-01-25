namespace Models.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public int userId { get; set; }
    public DateTime ExpiryDate { get; set; }
}
