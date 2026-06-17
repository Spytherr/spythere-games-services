namespace SpythereGamesServices;

public class Score
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int GameId { get; set; }
    public long Value { get; set; }
    public DateTime SubmittedAt { get; set; }    
}