namespace SpythereGamesServices;

public class Player
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public required string Platform { get; set; }
    public required string ExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Score> Scores { get; set; } = new();
}