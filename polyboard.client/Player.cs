using Godot;

public class Player
{
	public string Name { get; set; }
	public Figurehead Pawn { get; set; }
	public int CurrentFieldId { get; set; }

	public Player(string name, Figurehead pawn, int startFieldId = 0)
	{
		Name = name;
		Pawn = pawn;
		CurrentFieldId = startFieldId;
	}
}
