using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public partial class Figurehead : CharacterBody3D
{
	public int CurrentPositionIndex { get; set; } = 0;
	[Export] public NodePath walkSoundPlayerPath;
	[Export] public NodePath pawnInstancePath;
	[Export] public int StartingECTS = 1000;
	[Export] public Color playerColor;
	private AudioStreamPlayer3D walkSoundPlayer;
	private AnimationPlayer animationPlayer;
	private NotificationService notificationService;
	public List<bool> ownedFields = new List<bool>(40);
	public bool hasLoan=false;
	public int Loan=0;

	private bool hasAnimation = false;
	public int ECTS { get; private set; }

	// Dodane stałe do obsługi planszy
	private const int TOTAL_FIELDS = 40;
	private const int START_FIELD = 0;

	public override void _Ready()
	{
		ownedFields = new List<bool>(new bool[40]);
		walkSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>(walkSoundPlayerPath);
		notificationService = GetNodeOrNull<NotificationService>("/root/NotificationService");
		if (walkSoundPlayer == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono AudioStreamPlayer3D. Sprawdź walkSoundPlayerPath.",
				NotificationService.NotificationType.Error);
			GD.PrintErr("Błąd: Nie znaleziono AudioStreamPlayer3D. Sprawdź walkSoundPlayerPath.");
		}
		var pawnInstance = GetNodeOrNull<Node>(pawnInstancePath);
		if (pawnInstance == null)
		{
			notificationService.ShowNotification($"Błąd: Nie znaleziono instancji pionka pod ścieżką {pawnInstancePath}.",
				NotificationService.NotificationType.Error);
			GD.PrintErr($"Błąd: Nie znaleziono instancji pionka pod ścieżką {pawnInstancePath}.");
			return;
		}
		animationPlayer = pawnInstance.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (animationPlayer != null)
		{
			notificationService.ShowNotification("Znaleziono AnimationPlayer w instancji pionka.", NotificationService.NotificationType.Error);
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
					notificationService.ShowNotification($"Gracz {Name} przeszedł przez pole startowe i otrzymał 200 ECTS.");
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
				notificationService.ShowNotification($"Błąd: Nie znaleziono pola docelowego lub brak pozycji na polu {CurrentPositionIndex}.",
					NotificationService.NotificationType.Error);
				StopWalkSound();
				return;
			}

			// Znajdź wolną pozycję na polu
			int freeIndex = nextField.occupied.FindIndex(occupied => !occupied);
			if (freeIndex == -1)
			{
				GD.PrintErr($"Błąd: Brak wolnych pozycji na polu {CurrentPositionIndex}.");
				notificationService.ShowNotification($"Błąd: Brak wolnych pozycji na polu {CurrentPositionIndex}.",
					NotificationService.NotificationType.Error);
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
				var currentRotation = RotationDegrees;
				var newRotation = currentRotation + new Vector3(0, -90, 0);


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
		if (animationPlayer != null && !animationPlayer.IsPlaying())
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
			notificationService.ShowNotification($"Gracz {Name} otrzymał {field.ECTSReward} ECTS za lądowanie na polu {field.Name}.");
			ShowECTSUpdate();
		}
	}

	public void AddECTS(int amount)
	{
		ECTS += amount;
		GD.Print($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
		notificationService.ShowNotification($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
		UpdateECTSUI();
	}

	public bool SpendECTS(int amount)
	{
		if (ECTS >= amount)
		{
			ECTS -= amount;
			GD.Print($"Gracz {Name} wydał {amount} ECTS. Pozostało: {ECTS}");
			notificationService.ShowNotification($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
			UpdateECTSUI();
			return true;
		}
		GD.Print($"Gracz {Name} nie ma wystarczającej ilości ECTS.");
		notificationService.ShowNotification($"Gracz {Name} otrzymał {amount} ECTS. Łączna ilość: {ECTS}");
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

	public bool OwnsField(int fieldId)
	{
		if (fieldId < 0 || fieldId >= ownedFields.Count)
			return false;

		return ownedFields[fieldId];
	}

	public void AcquireField(int fieldId)
	{
		if (fieldId < 0)
			return;

		while (fieldId >= ownedFields.Count)
		{
			ownedFields.Add(false);
		}

		ownedFields[fieldId] = true;
		GD.Print($"Gracz {Name} nabył własność pola {fieldId}");
	}

	public void LoseField(int fieldId)
	{
		if (fieldId < 0 || !ownedFields.Any() || fieldId >= ownedFields.Count)
			return;

		ownedFields[fieldId] = false;
		GD.Print($"Gracz {Name} utracił własność pola {fieldId}");
	}

	public List<int> GetOwnedFieldIds()
	{
		List<int> result = new List<int>();

		for (int i = 0; i < ownedFields.Count; i++)
		{
			if (ownedFields[i])
			{
				result.Add(i);
			}
		}

		return result;
	}
	public bool CheckIfOwnsField(int fieldId)
	{
		if (fieldId < 0 || fieldId >= ownedFields.Count)
			return false;

		return ownedFields[fieldId];
	}

	public int GetOwnedFieldsCount()
	{
		return ownedFields.Count(field => field);
	}

	public bool CanAfford(int amount)
	{
		return ECTS >= amount;
	}

	public void ShowTradeNotification(string partnerName, string gainedItem, string lostItem)
	{
		string message;

		if (!string.IsNullOrEmpty(gainedItem) && !string.IsNullOrEmpty(lostItem))
		{
			message = $"Wymiana z graczem {partnerName}: Otrzymano {gainedItem}, oddano {lostItem}";
		}
		else if (!string.IsNullOrEmpty(gainedItem))
		{
			message = $"Wymiana z graczem {partnerName}: Otrzymano {gainedItem}";
		}
		else if (!string.IsNullOrEmpty(lostItem))
		{
			message = $"Wymiana z graczem {partnerName}: Oddano {lostItem}";
		}
		else
		{
			message = $"Zakończono wymianę z graczem {partnerName}";
		}

		notificationService.ShowNotification(message, NotificationService.NotificationType.Normal,4F);
	}

}
