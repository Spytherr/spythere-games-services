namespace SpythereGamesServices;

public class Player
{
    public int Id { get; set; }
    public string DisplayName { get; set; }
    public string Platform { get; set; }
    public string ExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Scores> Scores { get; set; }
}