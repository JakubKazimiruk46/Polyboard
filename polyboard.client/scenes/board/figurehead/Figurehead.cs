using Godot;
using System;
using System.Threading.Tasks;

public partial class Figurehead : CharacterBody3D
{
	public int CurrentPositionIndex { get; set; } = 0;

	[Export]
	public NodePath walkSoundPlayerPath; // Ścieżka do AudioStreamPlayer3D

	private AudioStreamPlayer3D walkSoundPlayer;

	public override void _Ready()
	{
		walkSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>(walkSoundPlayerPath);
		if (walkSoundPlayer == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono AudioStreamPlayer3D. Sprawdź walkSoundPlayerPath.");
		}
	}

	// Metoda wywoływana przez GameManager po obliczeniu kroków
	public async Task MovePawnSequentially(int steps, Board board)
	{
		int targetIndex = CurrentPositionIndex + steps;
		targetIndex = targetIndex % 40; // plansza 40 pól

		PlayWalkSound();

		while (CurrentPositionIndex != targetIndex)
		{
			CurrentPositionIndex = (CurrentPositionIndex + 1) % 40;

			Field nextField = board.GetFieldById(CurrentPositionIndex);
			if (nextField == null || nextField.positions.Count == 0)
			{
				GD.PrintErr("Błąd: Nie znaleziono pola docelowego lub brak pozycji na polu.");
				StopWalkSound();
				return;
			}
			Vector3 nextPosition = nextField.positions[0];
			Tween tween = CreateTween();
			tween.TweenProperty(this, "global_position", nextPosition, 0.5f)
				 .SetTrans(Tween.TransitionType.Linear)
				 .SetEase(Tween.EaseType.InOut);
			await ToSignal(tween, "finished");
		}

		StopWalkSound();
		board.ShowFieldTexture(CurrentPositionIndex);
	}

	private void PlayWalkSound()
	{
		if (walkSoundPlayer != null)
		{
			walkSoundPlayer.Play();
		}
	}

	private void StopWalkSound()
	{
		if (walkSoundPlayer != null && walkSoundPlayer.Playing)
		{
			walkSoundPlayer.Stop();
		}
	}
}
