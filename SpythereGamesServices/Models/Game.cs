namespace SpythereGamesServices;

public class Game
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Score> Scores { get; set; } = new();
}