using Godot;
using System;
using System.Threading.Tasks;

public partial class Figurehead : CharacterBody3D
{
	public int CurrentPositionIndex { get; set; } = 0;
	private Board board;
	
	private Sprite2D textureDisplay;
	

	[Export]
	public NodePath dieNodePath1; // Ścieżka do pierwszej kostki
	[Export]
	public NodePath dieNodePath2; // Ścieżka do drugiej kostki

	[Export]
	public NodePath masterCameraPath; // Ścieżka do kamery Master shot
	[Export]
	public NodePath tpCameraPath;     // Ścieżka do kamery TP
	[Export]
	public NodePath diceCameraPath;    // Ścieżka do kamery Kostki

	private Camera3D masterCamera;
	private Camera3D tpCamera;
	private Camera3D diceCamera;

	private int? die1Result = null;
	private int? die2Result = null;
	private int totalSteps = 0; // Przechowuje łączną sumę kroków dla bieżącej tury

	public override void _Ready()
	{
		// Pobierz kamery
		masterCamera = GetNodeOrNull<Camera3D>(masterCameraPath);
		tpCamera = GetNodeOrNull<Camera3D>(tpCameraPath);
		diceCamera = GetNodeOrNull<Camera3D>(diceCameraPath);
		textureDisplay = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/FieldCard");

		if (masterCamera == null || tpCamera == null || diceCamera == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono jednej z kamer. Sprawdź ścieżki.");
		}
		else
		{
			GD.Print("Ustawiono kamerę Master shot jako domyślną.");
			masterCamera.Current = true; // Ustaw kamerę Master shot jako domyślną
		}

		// Pobierz pierwszą kostkę
		Node dieNode1 = GetNodeOrNull(dieNodePath1);
		if (dieNode1 == null)
		{
			GD.PrintErr($"Błąd: Nie znaleziono pierwszej kostki pod ścieżką '{dieNodePath1}'. Upewnij się, że dieNodePath1 jest ustawiony poprawnie.");
		}
		else
		{
			GD.Print("Pierwsza kostka została wykryta.");
			dieNode1.Connect("roll_finished", new Callable(this, nameof(OnDie1RollFinished)));
		}

		// Pobierz drugą kostkę
		Node dieNode2 = GetNodeOrNull(dieNodePath2);
		if (dieNode2 == null)
		{
			GD.PrintErr($"Błąd: Nie znaleziono drugiej kostki pod ścieżką '{dieNodePath2}'. Upewnij się, że dieNodePath2 jest ustawiony poprawnie.");
		}
		else
		{
			GD.Print("Druga kostka została wykryta.");
			dieNode2.Connect("roll_finished", new Callable(this, nameof(OnDie2RollFinished)));
		}

		// Pobierz planszę
		board = GetTree().Root.GetNode<Board>("Level/Board");
		if (board == null)
		{
			GD.PrintErr("Nie znaleziono planszy.");
		}
		else
		{
			GD.Print("Plansza została poprawnie załadowana.");
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept"))
		{
			SwitchToDiceCamera(); // Przełącz na kamerę Kostki
			textureDisplay.Visible = false;
		}
	}

	private void OnDie1RollFinished(int value)
	{
		GD.Print($"Wylosowano dla pierwszej kostki: {value}");
		die1Result = value;
		CheckAndMovePawn();
	}

	private void OnDie2RollFinished(int value)
	{
		GD.Print($"Wylosowano dla drugiej kostki: {value}");
		die2Result = value;
		CheckAndMovePawn();
	}

	private void CheckAndMovePawn()
	{
		if (die1Result.HasValue && die2Result.HasValue)
		{
			totalSteps += die1Result.Value + die2Result.Value;
			GD.Print($"Łączna suma oczek: {totalSteps}");

			// Przełączenie kamery na TP po rzucie
			SwitchToTPCamera();

			// Jeśli obie kostki mają tę samą wartość, wykonaj dodatkowy rzut
			if (die1Result.Value == die2Result.Value)
			{
				GD.Print("Wylosowano tę samą wartość na obu kostkach! Powtórzenie rzutu.");
				die1Result = null;
				die2Result = null;
				ReRollDice(); // Powtórz rzut, aby dodać do sumy
			}
			else
			{
				// Wykonaj ruch i zresetuj wartości po zakończeniu tury
				MovePawnSequentially(totalSteps);
				ResetRoll();
			}
		}
	}

	private void ReRollDice()
	{
		SwitchToDiceCamera();
		Node dieNode1 = GetNodeOrNull(dieNodePath1);
		Node dieNode2 = GetNodeOrNull(dieNodePath2);
		if (dieNode1 != null && dieNode2 != null)
		{
			GD.Print("Rozpoczęcie powtórnego rzutu kostkami.");
			dieNode1.Call("_roll");
			dieNode2.Call("_roll");
		}
		else
		{
			GD.PrintErr("Błąd: Nie można wykonać powtórnego rzutu. Jedna z kostek jest nieprawidłowa.");
		}
	}

	private void ResetRoll()
	{
		die1Result = null;
		die2Result = null;
		totalSteps = 0; // Zresetuj sumę kroków po zakończeniu tury
		GD.Print("Zakończono turę i zresetowano wartości kostek.");
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
			GD.Print($"Przemieszczenie pionka na pozycję {CurrentPositionIndex}.");
			Tween tween = CreateTween();
			tween.TweenProperty(this, "global_position", nextPosition, 0.3f)
				 .SetTrans(Tween.TransitionType.Linear)
				 .SetEase(Tween.EaseType.InOut);
			await ToSignal(tween, "finished");
		}
		board.ShowFieldTexture(targetIndex);

		// Przełącz z powrotem na kamerę Master shot po zakończeniu ruchu
		GD.Print("Przełączanie kamery z powrotem na Master shot po zakończeniu ruchu.");
		SwitchToMasterCamera();
	}

	private void SwitchToTPCamera()
	{
		if (tpCamera != null)
		{
			GD.Print("Przełączono na kamerę TP.");
			tpCamera.Current = true;
		}
		else
		{
			GD.PrintErr("Błąd: Kamera TP jest nieprawidłowa.");
		}
	}

	private void SwitchToMasterCamera()
	{
		if (masterCamera != null)
		{
			GD.Print("Przełączono na kamerę Master shot.");
			masterCamera.Current = true;
		}
		else
		{
			GD.PrintErr("Błąd: Kamera Master shot jest nieprawidłowa.");
		}
	}

	private void SwitchToDiceCamera()
	{
		if (diceCamera != null)
		{
			GD.Print("Przełączono na kamerę Kostki.");
			diceCamera.Current = true;
		}
		else
		{
			GD.PrintErr("Błąd: Kamera Kostki jest nieprawidłowa.");
		}
		
	}
}
