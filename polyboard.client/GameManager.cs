using Godot;
using System;
using System.Collections.Generic;

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
	[Export] public NodePath rollButtonPath;
	[Export] public NodePath endTurnButtonPath;
	[Export] public NodePath ectsUIContainerPath; // Ścieżka do kontenera ECTS w UI
	[Export] public NodePath playersUIContainerPath; // Ścieżka do UI/Players

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
	private Button rollButton;
	private Button endTurnButton;
	private VBoxContainer ectsUIContainer; // Kontener ECTS w UI

	// Listy do przechowywania referencji do labeli w UI odpowiadających graczom:
	private List<Label> playerNameLabels = new List<Label>();
	private List<Label> playerECTSLabels = new List<Label>();

	private enum GameState { WaitingForInput, RollingDice, MovingPawn, EndTurn }
	private GameState currentState = GameState.WaitingForInput;

	public override void _Ready()
	{
		InitCameras();
		InitNotifications();
		InitBoard();
		InitPlayers();
		InitDice();
		InitRollButton();
		InitEndTurnButton();
		InitPlayersUI();
		SetAllPlayersOnStart();
	}
	
	public Figurehead getCurrentPlayer()
	{
		return players[currentPlayerIndex];
	}
	
	public Field getCurrentField(int position)
	{
		return board.GetFieldById(position);
	}
	
	private void InitCameras()
	{
		masterCamera = GetNodeOrNull<Camera3D>(masterCameraPath);
		diceCamera = GetNodeOrNull<Camera3D>(diceCameraPath);
		if (masterCamera == null || diceCamera == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono jednej z kamer. Sprawdź ścieżki.");
		}
		else
		{
			SetActiveCamera(masterCamera);
		}
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
			GD.PrintErr("Błąd: Nie znaleziono planszy.");
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
			GD.PrintErr("Błąd: Nie znaleziono węzła Players (3D). Sprawdź, czy 'playersContainerPath' jest ustawiony w edytorze.");
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

	private void InitRollButton()
	{
		rollButton = GetNodeOrNull<Button>(rollButtonPath);
		if (rollButton == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono przycisku do rzucania kostkami.");
			return;
		}
		rollButton.Connect("pressed", new Callable(this, nameof(OnRollButtonPressed)));
		rollButton.Visible = true;
	}

	private void InitEndTurnButton()
	{
		endTurnButton = GetNodeOrNull<Button>(endTurnButtonPath);
		if (endTurnButton == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono przycisku do zakończenia tury.");
			return;
		}
		endTurnButton.Connect("pressed", new Callable(this, nameof(OnEndTurnButtonPressed)));
		endTurnButton.Visible = false; // Początkowo przycisk jest niewidoczny
	}

	private void InitPlayersUI()
	{
		Node playersUIContainer = GetNodeOrNull(playersUIContainerPath);
		if (playersUIContainer == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono kontenera UI/Players. Sprawdź 'playersUIContainerPath'.");
			return;
		}

		// Załóżmy, że ilość dzieci w playersUIContainer odpowiada ilości graczy.
		// Każdy PlayerX ma strukturę MarginContainer > VBoxContainer > Label (nazwa) i HBoxContainer > Label (ECTS).
		for (int i = 0; i < players.Count; i++)
		{
			Node playerUINode = playersUIContainer.GetChild(i);
			if (playerUINode == null)
			{
				GD.PrintErr($"Błąd: Nie znaleziono węzła UI dla gracza {i+1}");
				continue;
			}

			var marginContainer = playerUINode.GetNodeOrNull<MarginContainer>("MarginContainer");
			if (marginContainer == null)
			{
				GD.PrintErr($"Błąd: Węzeł UI Player{i+1} nie posiada oczekiwanej struktury (MarginContainer).");
				continue;
			}

			var vBoxContainer = marginContainer.GetNodeOrNull<VBoxContainer>("VBoxContainer");
			if (vBoxContainer == null)
			{
				GD.PrintErr($"Błąd: Węzeł UI Player{i+1} nie posiada oczekiwanej struktury (VBoxContainer).");
				continue;
			}

			// Pierwszy label to nazwa gracza
			var nameLabel = vBoxContainer.GetNodeOrNull<Label>("Label");
			if (nameLabel == null)
			{
				GD.PrintErr($"Błąd: Nie znaleziono Label z nazwą gracza (Player{i+1}).");
				continue;
			}

			// W HBoxContainer mamy TextureRect i Label od ECTS
			var hBoxContainer = vBoxContainer.GetNodeOrNull<HBoxContainer>("HBoxContainer");
			if (hBoxContainer == null)
			{
				GD.PrintErr($"Błąd: Nie znaleziono HBoxContainer dla ECTS w Player{i+1}.");
				continue;
			}

			Label ectsLabel = null;
			// Szukamy Label w HBoxContainer (może mieć różną nazwę, zakładamy "Label")
			foreach (Node child in hBoxContainer.GetChildren())
			{
				if (child is Label lbl)
				{
					ectsLabel = lbl;
					break;
				}
			}

			if (ectsLabel == null)
			{
				GD.PrintErr($"Błąd: Nie znaleziono Label ECTS w Player{i+1}.");
				continue;
			}

			// Ustawiamy nazwy i ECTS
			nameLabel.Text = players[i].Name;
			ectsLabel.Text = players[i].ECTS.ToString();

			playerNameLabels.Add(nameLabel);
			playerECTSLabels.Add(ectsLabel);
		}
	}

	private void UpdateECTSUI(int playerIndex)
	{
		if (playerIndex < 0 || playerIndex >= playerECTSLabels.Count)
		{
			GD.PrintErr("Błąd: Indeks gracza poza zakresem podczas aktualizacji ECTS UI.");
			return;
		}

		playerECTSLabels[playerIndex].Text = players[playerIndex].ECTS.ToString();
	}

	private void SetAllPlayersOnStart()
	{
		if (board == null)
		{
			GD.PrintErr("Board is not initialized. Cannot set players on start.");
			return;
		}
		for (int i = 0; i < players.Count; i++)
		{
			Figurehead player = players[i];
			Vector3? startPosition = board.GetPositionForPawn(0, i % board.GetFieldById(0).positions.Count);
			if (!startPosition.HasValue)
			{
				GD.PrintErr($"Błąd: Nie znaleziono pozycji startowej dla gracza {i + 1}.");
				continue;
			}
			player.CurrentPositionIndex = 0;
			player.GlobalPosition = startPosition.Value;
			GD.Print($"Gracz {players[i].Name} ustawiony na pozycji startowej: {startPosition.Value}");

			// Aktualizacja ECTS UI na starcie
			UpdateECTSUI(i);
		}
		GD.Print("Wszyscy gracze zostali ustawieni na polu startowym.");
	}

	private void OnRollButtonPressed()
	{
		if (currentState == GameState.WaitingForInput)
		{
			StartDiceRollForCurrentPlayer();
		}
	}

	private void OnEndTurnButtonPressed()
	{
		if (currentState == GameState.WaitingForInput)
		{
			EndTurn();
		}
	}

	private void StartDiceRollForCurrentPlayer()
	{
		if (dieNode1 == null || dieNode2 == null)
		{
			GD.PrintErr("Nie można rzucić kostkami: kostki nie są zainicjalizowane.");
			return;
		}
		currentState = GameState.RollingDice;
		BlockBoardInteractions();
		SwitchToDiceCamera();
		rollButton.Visible = false;
		endTurnButton.Visible = false; // Ukrycie przycisku zakończenia tury podczas rzutu

		string currentPlayerName = GetCurrentPlayerName();
		ShowNotification($"Gracz {currentPlayerName} rzuca kostkami...", 2f);
		GD.Print($"Rzut kostkami dla gracza: {currentPlayerName}");
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
			return;
		}
		int rollSum = die1Result.Value + die2Result.Value;
		totalSteps = rollSum;
		ShowNotification($"Łączna suma oczek: {totalSteps}", 3f);
		GD.Print($"Łączna suma oczek: {totalSteps}");
		SwitchToMasterCamera();
		MoveCurrentPlayerPawnSequentially(totalSteps);

		if (die1Result.Value == die2Result.Value)
		{
			// Dublet
			GD.Print("Dublet! Kolejny rzut po ruchu.");
			ShowNotification("Dublet! Powtórz rzut po ruchu.", 5f);
		}
		else
		{
			GD.Print("Nie wyrzucono dubletu. Przygotowanie do zakończenia tury.");
			ShowNotification("Nie wyrzucono dubletu. Możesz zakończyć turę.", 3f);
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
		endTurnButton.Visible = false; // Upewniamy się, że przycisk jest ukryty podczas ruchu
		await currentPlayer.MovePawnSequentially(steps, board);

		// Aktualizacja ECTS UI po ruchu
		UpdateECTSUI(currentPlayerIndex);

		if (die1Result.HasValue && die2Result.HasValue && die1Result.Value == die2Result.Value)
		{
			PrepareForNextRoll();
		}
		else
		{
			currentState = GameState.WaitingForInput;
		}
	}

	private void PrepareForNextRoll()
	{
		die1Result = null;
		die2Result = null;
		totalSteps = 0;
		UnblockBoardInteractions();
		currentState = GameState.WaitingForInput;
		rollButton.Visible = true; // Kolejny rzut
		endTurnButton.Visible = false;
		string currentPlayerName = GetCurrentPlayerName();
		ShowNotification($"Gracz {currentPlayerName}, rzuć ponownie!", 2f);
		GD.Print($"Gracz {currentPlayerName} może wykonać kolejny rzut.");
	}

	private void EndTurn()
	{
		die1Result = null;
		die2Result = null;
		totalSteps = 0;
		currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
		UnblockBoardInteractions();
		currentState = GameState.WaitingForInput;
		rollButton.Visible = true;
		endTurnButton.Visible = false;
		string nextPlayerName = GetCurrentPlayerName();
		ShowNotification($"Tura gracza: {nextPlayerName}", 2f);
		GD.Print($"Zakończono turę gracza. Teraz tura gracza: {nextPlayerName}");
	}

	private void SwitchToMasterCamera()
	{
		SetActiveCamera(masterCamera);
	}

	private void SwitchToDiceCamera()
	{
		SetActiveCamera(diceCamera);
	}

	private void SetActiveCamera(Camera3D cameraToActivate)
	{
		if (masterCamera != null) masterCamera.Current = false;
		if (diceCamera != null) diceCamera.Current = false;
		if (cameraToActivate != null)
		{
			cameraToActivate.Current = true;
			GD.Print($"Przełączono na kamerę: {cameraToActivate.Name}");
		}
		else
		{
			GD.PrintErr("Błąd: Próba aktywacji nieistniejącej kamery.");
		}
	}

	private void ShowNotification(string message, float duration = 3f)
	{
		if (notificationLabel == null || notificationPanel == null) return;
		notificationLabel.Text = message;
		notificationPanel.Visible = true;
		notificationLabel.Visible = true;
		var timer = GetTree().CreateTimer(duration);
		timer.Connect("timeout", new Callable(this, nameof(HideNotification)));
	}

	private void HideNotification()
	{
		if (notificationLabel == null || notificationPanel == null) return;
		notificationPanel.Visible = false;
		notificationLabel.Visible = false;
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

	public void UpdateECTS(int playerIndex)
	{
		UpdateECTSUI(playerIndex);
	}

	public override void _Process(double delta)
	{
		// Możesz tutaj dodać kod reagujący na zmiany ECTS itp.
	}

	/// <summary>
	/// Pobiera nazwę aktualnego gracza.
	/// </summary>
	/// <returns>Nazwa aktualnego gracza.</returns>
	private string GetCurrentPlayerName()
	{
		if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
		{
			GD.PrintErr("Błąd: Indeks aktualnego gracza poza zakresem podczas pobierania nazwy.");
			return "Nieznany";
		}
		return players[currentPlayerIndex].Name;
	}
	
		// Do kart
	public int GetCurrentPlayerIndex()
	{
		return currentPlayerIndex;
	}

	public void AddEctsToPlayer(int playerIndex, int amount)
	{
		if (playerIndex >= 0 && playerIndex < players.Count)
		{
			players[playerIndex].AddECTS(amount);
			UpdateECTSUI(playerIndex);
		}
	}
}
