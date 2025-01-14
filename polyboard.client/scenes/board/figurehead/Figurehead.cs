using Godot;
using System;
using System.Threading.Tasks;

public partial class Figurehead : CharacterBody3D
{
	public int CurrentPositionIndex { get; set; } = 0;
	[Export]
	public NodePath walkSoundPlayerPath; // Ścieżka do AudioStreamPlayer3D

	[Export]
	public int StartingECTS = 100; // Początkowa ilość ECTS

	private AudioStreamPlayer3D walkSoundPlayer;

	// Właściwość ECTS
	public int ECTS { get; private set; }

	public override void _Ready()
	{
		walkSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>(walkSoundPlayerPath);
		if (walkSoundPlayer == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono AudioStreamPlayer3D. Sprawdź walkSoundPlayerPath.");
		}

		// Inicjalizacja ECTS
		ECTS = StartingECTS;

		// Aktualizacja UI na starcie
		UpdateECTSUI();
	}

	// Metoda wywoływana przez GameManager po obliczeniu kroków
	public async Task MovePawnSequentially(int steps, Board board)
	{
		
		PlayWalkSound();
		int initialPosition = CurrentPositionIndex;
		int targetIndex = CurrentPositionIndex + steps;
		bool passedCorner = false;
	

		// Zakładamy, że plansza ma 40 pól
		for (int i = 1; i <= steps; i++)
		{
			CurrentPositionIndex = (initialPosition + i) % 40;

			// Sprawdzenie, czy gracz przeszedł przez CornerField0
			if (CurrentPositionIndex == 0)
			{
				AddECTS(200);
				GD.Print($"Gracz {Name} przeszedł przez CornerField0 i otrzymał 200 ECTS.");
				ShowECTSUpdate();
			}

			Field nextField = board.GetFieldById(CurrentPositionIndex);
			if (nextField == null || nextField.positions.Count == 0)
			{
				GD.PrintErr("Błąd: Nie znaleziono pola docelowego lub brak pozycji na polu.");
				StopWalkSound();
				return;
			}
			int freeIndex = nextField.occupied.FindIndex(occupied => !occupied);
			Vector3 nextPosition = nextField.positions[freeIndex];
			nextField.occupied[freeIndex]=true;
			Tween tween = CreateTween();
			tween.TweenProperty(this, "global_position", nextPosition, 0.5f)
				 .SetTrans(Tween.TransitionType.Linear)
				 .SetEase(Tween.EaseType.InOut);
			await ToSignal(tween, "finished");
		}

		StopWalkSound();
		board.StepOnField(CurrentPositionIndex);

		// Możliwość przyznania ECTS po zakończeniu ruchu
		OnFieldLanded(board.GetFieldById(CurrentPositionIndex));
	}
	public int GetCurrentPositionIndex()
	{
		return CurrentPositionIndex;
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

	// Metoda obsługująca zdarzenia po lądowaniu na polu
	private void OnFieldLanded(Field field)
	{
		// Przykład: Przyznaj ECTS za lądowanie na określonych polach
		if (field.ECTSReward > 0)
		{
			AddECTS(field.ECTSReward);
			GD.Print($"Gracz {Name} otrzymał {field.ECTSReward} ECTS za lądowanie na polu {field.Name}.");
			ShowECTSUpdate();
		}

		// Możesz również obsługiwać wydatki ECTS tutaj
		// np. jeśli gracz kupuje coś na polu
	}

	// Metody do zarządzania ECTS
	public void AddECTS(int amount)
	{
		ECTS += amount;
		GD.Print($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
		// Aktualizacja UI
		UpdateECTSUI();
	}

	public bool SpendECTS(int amount)
	{
		if (ECTS >= amount)
		{
			ECTS -= amount;
			GD.Print($"Gracz {Name} wydał {amount} ECTS. Pozostało: {ECTS}");
			// Aktualizacja UI
			UpdateECTSUI();
			return true;
		}
		else
		{
			GD.Print($"Gracz {Name} nie ma wystarczającej ilości ECTS.");
			return false;
		}
	}

	// Metody do aktualizacji UI
	private void UpdateECTSUI()
	{
		// Zakładamy, że masz referencję do GameManager lub innego komponentu odpowiedzialnego za UI
		// Możesz użyć sygnałów lub innej metody komunikacji, aby poinformować GameManager o zmianie ECTS
		// Przykład:
		// EmitSignal(nameof(ECTSChanged), ECTS);
	}

	private void ShowECTSUpdate()
	{
		// Możesz dodać animację lub powiadomienie o aktualizacji ECTS
		// Przykład:
		// ShowNotification($"ECTS: {ECTS}", 2f);
	}
	
}
