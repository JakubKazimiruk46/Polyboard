using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class Figurehead : CharacterBody3D
{
	public int CurrentPositionIndex { get; set; } = 0;
	[Export]
	public NodePath walkSoundPlayerPath;
	[Export]
	public NodePath pawnInstancePath;
	[Export]
	public int StartingECTS = 1000;
	[Export]
	public Color playerColor;
	private AudioStreamPlayer3D walkSoundPlayer;
	private AnimationPlayer animationPlayer;
	public List<bool> ownedFields=new List<bool>(40);
	
	private bool hasAnimation=false;
	public int ECTS { get; private set; }
	
	// Dodane stałe do obsługi planszy
	private const int TOTAL_FIELDS = 40;
	private const int START_FIELD = 0;

	public override void _Ready()
	{
		ownedFields = new List<bool>(new bool[40]);
		walkSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>(walkSoundPlayerPath);
		if (walkSoundPlayer == null)
		{
			ShowError("Błąd: Nie znaleziono AudioStreamPlayer3D. Sprawdź walkSoundPlayerPath.");
			GD.PrintErr("Błąd: Nie znaleziono AudioStreamPlayer3D. Sprawdź walkSoundPlayerPath.");
		}
		var pawnInstance = GetNodeOrNull<Node>(pawnInstancePath);
		if (pawnInstance == null)
		{
			ShowError($"Błąd: Nie znaleziono instancji pionka pod ścieżką {pawnInstancePath}.");
			GD.PrintErr($"Błąd: Nie znaleziono instancji pionka pod ścieżką {pawnInstancePath}.");
			return;
		}
		 animationPlayer = pawnInstance.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (animationPlayer != null)
		{
			ShowNotification("Znaleziono AnimationPlayer w instancji pionka.");
			GD.Print("Znaleziono AnimationPlayer w instancji pionka.");
		}
		else
		{
			GD.Print("Brak AnimationPlayer w instancji pionka.");
		}
		ECTS = StartingECTS;
		UpdateECTSUI();
	}
	
	public Godot.Collections.Array GetOwnedFieldsAsArray()
	{
		Godot.Collections.Array array = new Godot.Collections.Array();
		foreach (bool field in ownedFields)
		{
			array.Add(field);
		}
		return array;
	}
	
	// Nowa metoda do przesuwania pionka o określoną liczbę pól (może być ujemna)
	public async Task MoveByFields(int fieldCount, Board board)
	{
		PlayWalkSound();
		PlayWalkAnimation();
		int initialPosition = CurrentPositionIndex;
		int steps = Math.Abs(fieldCount); // Ilość kroków do wykonania
		int direction = Math.Sign(fieldCount); // 1 dla ruchu w przód, -1 dla ruchu w tył

		// Dla każdego kroku
		for (int i = 1; i <= steps; i++)
		{
			// Obliczanie nowej pozycji z uwzględnieniem kierunku i zawijania planszy
			int newPosition = initialPosition + (i * direction);
			
			// Obsługa przekroczenia granic planszy
			if (newPosition >= TOTAL_FIELDS)
			{
				newPosition %= TOTAL_FIELDS;
				// Przyznawanie ECTS za przejście przez start przy ruchu w przód
				if (direction > 0)
				{
					AddECTS(200);
					GD.Print($"Gracz {Name} przeszedł przez pole startowe i otrzymał 200 ECTS.");
					ShowNotification($"Gracz {Name} przeszedł przez pole startowe i otrzymał 200 ECTS.");
					ShowECTSUpdate();
				}
			}
			else if (newPosition < 0)
			{
				newPosition = TOTAL_FIELDS + (newPosition % TOTAL_FIELDS);
			}

			CurrentPositionIndex = newPosition;

			// Aktualizacja pozycji na planszy
			Field nextField = board.GetFieldById(CurrentPositionIndex);
			if (nextField == null || nextField.positions.Count == 0)
			{
				GD.PrintErr($"Błąd: Nie znaleziono pola docelowego lub brak pozycji na polu {CurrentPositionIndex}.");
				ShowError($"Błąd: Nie znaleziono pola docelowego lub brak pozycji na polu {CurrentPositionIndex}.");
				StopWalkSound();
				return;
			}

			// Znajdź wolną pozycję na polu
			int freeIndex = nextField.occupied.FindIndex(occupied => !occupied);
			if (freeIndex == -1)
			{
				GD.PrintErr($"Błąd: Brak wolnych pozycji na polu {CurrentPositionIndex}.");
				ShowError($"Błąd: Brak wolnych pozycji na polu {CurrentPositionIndex}.");
				StopWalkSound();
				return;
			}

			// Oznacz pozycję jako zajętą i wykonaj ruch
			nextField.occupied[freeIndex] = true;
			Vector3 nextPosition = nextField.positions[freeIndex];

			// Animacja ruchu
			Tween tween = CreateTween();
			tween.TweenProperty(this, "global_position", nextPosition, 0.5f)
				 .SetTrans(Tween.TransitionType.Linear)
				 .SetEase(Tween.EaseType.InOut);
				
				if (CurrentPositionIndex == 0 || CurrentPositionIndex == 10 || CurrentPositionIndex == 20 || CurrentPositionIndex == 30)
			{
			Vector3 currentRotation = RotationDegrees;
			Vector3 newRotation = currentRotation + new Vector3(0, 90, 0);

			
			tween.TweenProperty(this, "rotation_degrees", newRotation, 0.5f)
				 .SetTrans(Tween.TransitionType.Linear)
				 .SetEase(Tween.EaseType.InOut);
			}
			await ToSignal(tween, "finished");
		}

		StopWalkSound();
		StopWalkAnimation();
		// Wywołaj odpowiednie akcje po zakończeniu ruchu
		board.StepOnField(CurrentPositionIndex);
		OnFieldLanded(board.GetFieldById(CurrentPositionIndex));
	}

	// Zmodyfikowana metoda MovePawnSequentially, która teraz wykorzystuje MoveByFields
	public async Task MovePawnSequentially(int steps, Board board)
	{
		await MoveByFields(steps, board);
	}

	// Metoda pomocnicza do cofania pionka
	public async Task MoveBackward(int steps, Board board)
	{
		await MoveByFields(-steps, board);
	}

	// Metoda do przenoszenia pionka na konkretne pole
	public async Task MoveToField(int targetFieldId, Board board)
	{
		int currentPos = CurrentPositionIndex;
		int stepsForward = (targetFieldId - currentPos + TOTAL_FIELDS) % TOTAL_FIELDS;
		int stepsBackward = (currentPos - targetFieldId + TOTAL_FIELDS) % TOTAL_FIELDS;

		// Wybierz krótszą drogę
		if (stepsForward <= stepsBackward)
		{
			await MoveByFields(stepsForward, board);
		}
		else
		{
			await MoveByFields(-stepsBackward, board);
		}
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
	
	 private void PlayWalkAnimation()
	{
		if (animationPlayer != null  && !animationPlayer.IsPlaying())
		{
			
			animationPlayer.Play("ArmatureAction");
		}
	}

	private void StopWalkAnimation()
	{
		if (animationPlayer != null && animationPlayer.IsPlaying())
		{
			animationPlayer.Stop();
		}
	}

	private void OnFieldLanded(Field field)
	{
		if (field.ECTSReward > 0)
		{
			AddECTS(field.ECTSReward);
			GD.Print($"Gracz {Name} otrzymał {field.ECTSReward} ECTS za lądowanie na polu {field.Name}.");
			ShowNotification($"Gracz {Name} otrzymał {field.ECTSReward} ECTS za lądowanie na polu {field.Name}.");
			ShowECTSUpdate();
		}
	}

	public void AddECTS(int amount)
	{
		ECTS += amount;
		GD.Print($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
		ShowNotification($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
		UpdateECTSUI();
	}

	public bool SpendECTS(int amount)
	{
		if (ECTS >= amount)
		{
			ECTS -= amount;
			GD.Print($"Gracz {Name} wydał {amount} ECTS. Pozostało: {ECTS}");
			ShowNotification($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
			UpdateECTSUI();
			return true;
		}
		GD.Print($"Gracz {Name} nie ma wystarczającej ilości ECTS.");
		ShowNotification($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
		return false;
	}

	private void UpdateECTSUI()
	{
		// Implementacja aktualizacji UI
	}

	public void ShowECTSUpdate()
	{
		// Implementacja pokazywania aktualizacji ECTS
	}

	public int GetCurrentPositionIndex()
	{
		return CurrentPositionIndex;
	}

	//TODO DRY! if exactly the same function is needed in multiple classes it should be a separate service injected to them
	public void ShowNotification(string message, float duration = 3f)
	{
		var notifications = GetNode<Node>("/root/Notifications");
		if (notifications != null)
		{
			notifications.Call("show_notification", message, duration);
		}
		else
		{
			GD.PrintErr("NotificationLayer singleton not found. Make sure it's added as an Autoload.");
		}
	}

	public void ShowError(string message, float duration = 4f)
	{
		var notifications = GetNode<Node>("/root/Notifications");
		if (notifications != null)
		{
			notifications.Call("show_error", message, duration);
		}
		else
		{
			GD.PrintErr("NotificationLayer singleton not found. Make sure it's added as an Autoload.");
			GD.PrintErr(message);
		}
	}

}
