namespace SpythereGamesServices;

public class Game
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconUrl { get; set; }
    public List<Scores> Scores { get; set; }
}