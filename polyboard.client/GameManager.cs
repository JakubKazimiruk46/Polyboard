using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class GameManager : Node3D
{
	[Export] public NodePath dieNodePath1;
	[Export] public NodePath dieNodePath2;
	[Export] public NodePath boardPath; 
	[Export] public NodePath playersContainerPath;
	[Export] public NodePath masterCameraPath;
	[Export] public NodePath diceCameraPath;
	[Export] public NodePath notificationPanelPath;
	[Export] public NodePath notificationLabelPath;

	private Board board;
	private Camera3D masterCamera;
	private Camera3D diceCamera;
	private Label notificationLabel;
	private Panel notificationPanel;

	private RigidBody3D dieNode1;
	private RigidBody3D dieNode2;

	private List<Figurehead> players = new List<Figurehead>();
	private int currentPlayerIndex = 0;

	private int? die1Result = null;
	private int? die2Result = null;
	private int totalSteps = 0; 

	private enum GameState { WaitingForInput, RollingDice, MovingPawn, EndTurn }
	private GameState currentState = GameState.WaitingForInput;

	public override void _Ready()
	{
		InitCameras();
		InitNotifications();
		InitBoard();
		InitPlayers();
		InitDice();
	}

	private void InitCameras()
	{
		masterCamera = GetNodeOrNull<Camera3D>(masterCameraPath);
		diceCamera = GetNodeOrNull<Camera3D>(diceCameraPath);

		if (masterCamera == null || diceCamera == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono jednej z kamer. Sprawdź ścieżki.");
		}

		// Ustaw kamerę główną jako domyślną na start
		SetActiveCamera(masterCamera);
	}

	private void InitNotifications()
	{
		notificationPanel = GetNodeOrNull<Panel>(notificationPanelPath);
		notificationLabel = GetNodeOrNull<Label>(notificationLabelPath);

		if (notificationPanel != null) notificationPanel.Visible = false;
	}

	private void InitBoard()
	{
		board = GetNodeOrNull<Board>(boardPath);
		if (board == null)
		{
			GD.PrintErr("Nie znaleziono planszy.");
		}
		else
		{
			GD.Print("Plansza została poprawnie załadowana.");
		}
	}

	private void InitPlayers()
	{
		Node playersContainer = GetNodeOrNull(playersContainerPath);
		if (playersContainer == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono węzła Players.");
			return;
		}

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

	private void InitDice()
	{
		dieNode1 = GetNodeOrNull<RigidBody3D>(dieNodePath1);
		dieNode2 = GetNodeOrNull<RigidBody3D>(dieNodePath2);

		if (dieNode1 == null || dieNode2 == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono jednej z kostek.");
			return;
		}

		dieNode1.Connect("roll_finished", new Callable(this, nameof(OnDie1RollFinished)));
		dieNode2.Connect("roll_finished", new Callable(this, nameof(OnDie2RollFinished)));
		GD.Print("Kostki podłączone.");
	}

	public override void _Input(InputEvent @event)
	{
		// Klawisz "ui_accept" (Enter/Spacja) - rozpocznij rzut kostkami aktualnego gracza,
		// tylko jeśli aktualnie czekamy na input.
		if (@event.IsActionPressed("ui_accept") && currentState == GameState.WaitingForInput)
		{
			StartDiceRollForCurrentPlayer();
		}
	}

	private void StartDiceRollForCurrentPlayer()
	{
		if (dieNode1 == null || dieNode2 == null) return;

		currentState = GameState.RollingDice;
		BlockBoardInteractions();
		SetActiveCamera(diceCamera);

		GD.Print("Rzut kostkami dla gracza: " + (currentPlayerIndex + 1));
		dieNode1.Call("_roll");
		dieNode2.Call("_roll");
	}

	private void OnDie1RollFinished(int value)
	{
		GD.Print($"Wynik pierwszej kostki: {value}");
		die1Result = value;
		CheckDiceResults();
	}

	private void OnDie2RollFinished(int value)
	{
		GD.Print($"Wynik drugiej kostki: {value}");
		die2Result = value;
		CheckDiceResults();
	}

	private void CheckDiceResults()
	{
		if (!die1Result.HasValue || !die2Result.HasValue)
		{
			// Czekamy na obie kostki
			return;
		}

		// Obie kostki zakończyły rzut
		int rollSum = die1Result.Value + die2Result.Value;
		totalSteps += rollSum;

		ShowNotification($"Łączna suma oczek: {totalSteps}", 3f);

		// Sprawdź dublet
		if (die1Result.Value == die2Result.Value)
		{
			GD.Print("Dublet! Kolejny rzut.");
			ShowNotification("Dublet! Powtórz rzut.", 5f);

			// Reset wyników kostek na potrzeby kolejnego rzutu
			die1Result = null;
			die2Result = null;

			// Po krótkiej przerwie znów rzucamy
			var timer = GetTree().CreateTimer(2f);
			timer.Connect("timeout", new Callable(this, nameof(StartDiceRollForCurrentPlayer)));
		}
		else
		{
			// Przechodzimy do ruchu pionka
			SwitchToMasterCamera();
			MoveCurrentPlayerPawnSequentially(totalSteps);
		}
	}

	private async void MoveCurrentPlayerPawnSequentially(int steps)
	{
		if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
		{
			GD.PrintErr("Błąd: Indeks aktualnego gracza poza zakresem.");
			return;
		}

		currentState = GameState.MovingPawn;

		Figurehead currentPlayer = players[currentPlayerIndex];
		await currentPlayer.MovePawnSequentially(steps, board);

		// Po zakończeniu ruchu pionka - koniec tury
		EndTurn();
	}

	private void EndTurn()
	{
		// Reset wyników rzutu
		die1Result = null;
		die2Result = null;
		totalSteps = 0;

		// Następny gracz
		currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

		// Odblokuj interakcje z planszą
		UnblockBoardInteractions();

		currentState = GameState.WaitingForInput;

		GD.Print("Zakończono turę gracza. Teraz tura gracza: " + (currentPlayerIndex + 1));
		ShowNotification($"Tura gracza: {currentPlayerIndex + 1}");
	}

	private void BlockBoardInteractions()
	{
		if (board == null) return;
		foreach (Field field in board.GetFields())
		{
			field.isMouseEventEnabled = false;
		}
	}

	private void UnblockBoardInteractions()
	{
		if (board == null) return;
		foreach (Field field in board.GetFields())
		{
			field.isMouseEventEnabled = true;
		}
	}

	private void ShowNotification(string message, float duration = 3f)
	{
		if (notificationLabel == null || notificationPanel == null)
			return;

		notificationLabel.Text = message;
		notificationPanel.Visible = true;
		notificationLabel.Visible = true;

		var timer = GetTree().CreateTimer(duration);
		timer.Connect("timeout", new Callable(this, nameof(HideNotification)));
	}

	private void HideNotification()
	{
		if (notificationLabel == null || notificationPanel == null)
			return;

		notificationPanel.Visible = false;
		notificationLabel.Visible = false;
	}

	private void SwitchToMasterCamera()
	{
		SetActiveCamera(masterCamera);
	}

	private void SwitchToDiceCamera()
	{
		SetActiveCamera(diceCamera);
	}

	/// <summary>
	/// Ustawia wskazaną kamerę jako aktywną, dezaktywując inne.
	/// </summary>
	private void SetActiveCamera(Camera3D cameraToActivate)
	{
		if (masterCamera != null) masterCamera.Current = false;
		if (diceCamera != null) diceCamera.Current = false;

		if (cameraToActivate != null)
		{
			cameraToActivate.Current = true;
			GD.Print("Przełączono na kamerę: " + cameraToActivate.Name);
		}
		else
		{
			GD.PrintErr("Błąd: Próba aktywacji nieistniejącej kamery.");
		}
	}
}
