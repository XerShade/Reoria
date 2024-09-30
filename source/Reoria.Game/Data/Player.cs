namespace Reoria.Game.Data;

[Serializable]
public class Player
{
    public Player(int id, string name)
    {
        this.Id = id;
        this.Name = name;
    }

    public int Id { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public float X { get; set; } = 0;
    public float Y { get; set; } = 0;
}
