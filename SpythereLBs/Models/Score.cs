namespace SpythereGamesServices;

public class Scores
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int GameId { get; set; }
    public long Score { get; set; }
    public DateTime CreatedAt { get; set; }    
}