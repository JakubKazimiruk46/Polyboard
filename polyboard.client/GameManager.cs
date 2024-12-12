using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class GameManager : Node3D
{
	[Export]
	public NodePath dieNodePath1;
	[Export]
	public NodePath dieNodePath2;
	[Export]
	public NodePath boardPath; 
	[Export]
	public NodePath playersContainerPath; // Ścieżka do węzła Players
	[Export]
	public NodePath masterCameraPath;
	[Export]
	public NodePath diceCameraPath;
	[Export]
	public NodePath notificationPanelPath;
	[Export]
	public NodePath notificationLabelPath;

	private Board board;
	private Camera3D masterCamera;
	private Camera3D diceCamera;
	private Label notificationLabel;
	private Panel notificationPanel;
	
	private RigidBody3D dieNode1; // Typ RigidBody3D
	private RigidBody3D dieNode2; // Typ RigidBody3D

	// Lista pionków graczy
	private List<Figurehead> players = new List<Figurehead>();
	private int currentPlayerIndex = 0;

	// Wyniki rzutu
	private int? die1Result = null;
	private int? die2Result = null;
	private int totalSteps = 0; 

	public override void _Ready()
	{
		// Inicjalizacja kamer
		masterCamera = GetNodeOrNull<Camera3D>(masterCameraPath);
		diceCamera = GetNodeOrNull<Camera3D>(diceCameraPath);
		if (masterCamera == null || diceCamera == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono jednej z kamer. Sprawdź ścieżki.");
		}
		else
		{
			GD.Print("Ustawiono kamerę Master shot jako domyślną.");
			masterCamera.Current = true;
		}

		// Inicjalizacja powiadomień
		notificationPanel = GetNodeOrNull<Panel>(notificationPanelPath);
		notificationLabel = GetNodeOrNull<Label>(notificationLabelPath);
		if (notificationPanel != null) notificationPanel.Visible = false;

		// Inicjalizacja planszy
		board = GetNodeOrNull<Board>(boardPath);
		if (board == null)
		{
			GD.PrintErr("Nie znaleziono planszy.");
		}
		else
		{
			GD.Print("Plansza została poprawnie załadowana.");
		}

		// Inicjalizacja pionków graczy
		Node playersContainer = GetNodeOrNull(playersContainerPath);
		if (playersContainer == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono węzła Players. Sprawdź, czy 'playersContainerPath' jest ustawiony w edytorze.");
		}
		else
		{
			foreach (Node child in playersContainer.GetChildren())
			{
				if (child is Figurehead fh)
				{
					players.Add(fh);
				}
			}

			if (players.Count == 0)
			{
				GD.PrintErr("Błąd: Nie znaleziono żadnych pionków w węźle Players.");
			}
			else
			{
				GD.Print($"Znaleziono {players.Count} graczy.");
			}
		}

		// Inicjalizacja kostek
		dieNode1 = GetNodeOrNull<RigidBody3D>(dieNodePath1);
		dieNode2 = GetNodeOrNull<RigidBody3D>(dieNodePath2);

		if (dieNode1 == null || dieNode2 == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono jednej z kostek.");
		}
		else
		{
			// Zakładając, że die.gd rozszerza RigidBody3D i ma sygnał 'roll_finished'
			dieNode1.Connect("roll_finished", new Callable(this, nameof(OnDie1RollFinished)));
			dieNode2.Connect("roll_finished", new Callable(this, nameof(OnDie2RollFinished)));
			GD.Print("Kostki podłączone.");
		}
	}

	private void ShowNotification(string message, float duration = 3f)
	{
		if (notificationLabel == null)
			return;

		notificationLabel.Text = message;
		notificationPanel.Visible = true;
		notificationLabel.Visible = true;

		var timer = GetTree().CreateTimer(duration);
		timer.Connect("timeout", new Callable(this, nameof(HideNotification)));
	}

	private void HideNotification()
	{
		if (notificationLabel != null)
		{
			notificationPanel.Visible = false;
			notificationLabel.Visible = false;
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Klawisz "ui_accept" (Enter/Spacja) - rozpocznij rzut kostkami aktualnego gracza
		if (@event.IsActionPressed("ui_accept"))
		{
			StartDiceRollForCurrentPlayer();
		}
	}

	private void StartDiceRollForCurrentPlayer()
	{
		SwitchToDiceCamera();
		// Rzuć kostkami
		if (dieNode1 != null && dieNode2 != null)
		{
			GD.Print("Rzut kostkami dla gracza: " + (currentPlayerIndex + 1));
			dieNode1.Call("_roll");
			dieNode2.Call("_roll");
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
		foreach (Field field in board.GetFields())
		{
			field.isMouseEventEnabled = false;
		}

		if (die1Result.HasValue && die2Result.HasValue)
		{
			totalSteps += die1Result.Value + die2Result.Value;
			GD.Print($"Łączna suma oczek: {totalSteps}");
			ShowNotification($"Łączna suma oczek: {totalSteps}");

			// Opcjonalnie, jeśli wcześniej przełączano na kamerę TP po rzucie, możesz przełączyć na inną kamerę lub pozostawić bieżącą
			// SwitchToMasterCamera();

			// Sprawdź dublet
			if (die1Result.Value == die2Result.Value)
			{
				GD.Print("Dublet! Kolejny rzut.");
				ShowNotification("Dublet! Powtórz rzut.", 5f);
				// Reset wyników kostek (ale NIE totalSteps, bo sumujemy dalej)
				die1Result = null;
				die2Result = null;
				// Kolejny rzut
				StartDiceRollForCurrentPlayer();
			}
			else
			{
				// Ruch pionka aktualnego gracza
				MoveCurrentPlayerPawnSequentially(totalSteps);
			}
		}
	}

	private async void MoveCurrentPlayerPawnSequentially(int steps)
	{
		if (currentPlayerIndex >= players.Count)
		{
			GD.PrintErr("Błąd: Indeks aktualnego gracza jest poza zakresem.");
			return;
		}

		Figurehead currentPlayer = players[currentPlayerIndex];

		await currentPlayer.MovePawnSequentially(steps, board);

		// Po zakończeniu ruchu
		EndTurn();
	}

	private void EndTurn()
	{
		// Zresetuj wartości
		die1Result = null;
		die2Result = null;
		totalSteps = 0;

		// Następny gracz
		currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

		// Powrót kamery na master shot
		SwitchToMasterCamera();
		foreach (Field field in board.GetFields())
		{
			field.isMouseEventEnabled = true;
		}

		GD.Print("Zakończono turę gracza. Teraz tura gracza: " + (currentPlayerIndex + 1));
		ShowNotification($"Tura gracza: {currentPlayerIndex + 1}");
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
