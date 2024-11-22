using Godot;
using System;
using System.Threading.Tasks;

public partial class Figurehead : CharacterBody3D
{
	public int CurrentPositionIndex { get; set; } = 0;
	private Board board;
	[Export]
	public NodePath dieNodePath;

	public override void _Ready()
	{
		Node dieNode = GetNodeOrNull(dieNodePath);
		if (dieNode == null)
		{
			GD.PrintErr($"Błąd: Nie znaleziono obiektu kostki pod ścieżką '{dieNodePath}'. Upewnij się, że dieNodePath jest ustawiony poprawnie.");
			return;
		}
		dieNode.Connect("roll_finished", new Callable(this, nameof(OnDieRollFinished)));
		board = GetTree().Root.GetNode<Board>("Level/Board");

		if (board == null)
		{
			GD.PrintErr("Nie znaleziono planszy.");
		}
	}

	private void OnDieRollFinished(int value)
	{
		GD.Print($"Wylosowano: {value}");
		MovePawnSequentially(value);
	}

	public async void MovePawnSequentially(int steps)
	{
		int targetIndex = CurrentPositionIndex + steps;

		if (targetIndex >= 40)
		{
			targetIndex = targetIndex % 40;
		}


		
		while (CurrentPositionIndex != targetIndex)
		{

			CurrentPositionIndex = (CurrentPositionIndex + 1) % 40;

			Field nextField = board.GetFieldById(CurrentPositionIndex);
			if (nextField == null || nextField.positions.Count == 0)
			{
				GD.PrintErr("Błąd: Nie znaleziono pola docelowego lub brak pozycji na polu.");
				return;
			}
			Vector3 nextPosition = nextField.positions[0];
			Tween tween = CreateTween();
			tween.TweenProperty(this, "global_position", nextPosition, 0.3f)
				 .SetTrans(Tween.TransitionType.Linear)
				 .SetEase(Tween.EaseType.InOut);
			await ToSignal(tween, "finished");
		}
		board.ShowFieldTexture(targetIndex);
		
	}
}
