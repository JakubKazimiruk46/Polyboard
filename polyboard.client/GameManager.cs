using Godot;
using System;
using System.Collections.Generic;
using Polyboard.Enums;
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
	[Export] public NodePath rollButtonPath;
	[Export] public NodePath endTurnButtonPath;
	[Export] public NodePath ectsUIContainerPath;
	[Export] public NodePath playersUIContainerPath;
	[Export] public NodePath playersStatsContainerPath;
	[Export] public NodePath playerInitializerPath;
	[Export] public NodePath notificationServicePath;
	[Export] public float turnTimeLimit = 60.0f; // czas tury w sekundach
	[Export] public int maxRounds = 30; // Maksymalna liczba rund, po której gra się kończy (ustaw 0 dla braku limitu)
	[Export] public NodePath gameEndScreenPath; // Ścieżka do ekranu końcowego
	[Export] public NodePath returnToMenuButtonPath; // Przycisk powrotu do menu
	[Export] public NodePath gameResultsContainerPath; // Kontener na wyniki końcowe

	private Label roundLabel;
	private Label playerLabel;
	private Board board;
	private Camera3D masterCamera;
	private Camera3D diceCamera;
	private Label notificationLabel;
	private Panel notificationPanel;
	private RigidBody3D dieNode1;
	private RigidBody3D dieNode2;
	public List<Figurehead> players = new List<Figurehead>();
	public int currentPlayerIndex = 0;
	private int? die1Result = null;
	private int? die2Result = null;
	private int totalSteps = 0;
	private Button rollButton;
	private Button endTurnButton;
	private VBoxContainer ectsUIContainer;
	private bool isMovementInProgress = false;
	private AudioStreamPlayer3D doubleSoundPlayer;
	private AudioStreamPlayer3D gainECTSSoundPlayer;
	private AudioStreamPlayer3D nextTurnSoundPlayer;
	private AudioStreamPlayer3D bankruptSoundPlayer;
	private AudioStreamPlayer3D reviveSoundPlayer;
	private AudioStreamPlayer3D walkingSoundPlayer;
	private List<Label> playerNameLabels = new List<Label>();
	private List<Label> playerECTSLabels = new List<Label>();
	private List<bool> playerBankruptcyStatus = new List<bool>();
	private Timer turnTimer;
	private Label turnTimerLabel;
	private NotificationService notificationService;
	private int currentRound = 1;
	private PackedScene gameEndScreenScene;
	private CanvasLayer gameEndScreen;
	private Button returnToMenuButton;
	private VBoxContainer gameResultsContainer;
	private Node achievementManager;
	private MoveHistory moveHistory;

	private bool isGameOver = false;

	//public dla wymian
	public List<Figurehead> Players
	{
		get { return players; }
	}
	
	public Godot.Collections.Array GetPlayers()
	{
		var array = new Godot.Collections.Array();
		foreach (var player in players)
		{
			array.Add(player);
		}
		return array;
	}
	
	public bool IsSpecialRollHappening = false;
	public bool IsLastRegularRollDouble = false;

	[Signal]
	public delegate void DicesStoppedRollingEventHandler();

	[Signal]
	public delegate void SpecialRollEndedEventHandler();

	private int lastDiceTotal;
	public int GetLastDiceTotal()
	{
		return lastDiceTotal;
	}

	private enum GameState
	{
		WaitingForInput,
		RollingDice,
		MovingPawn,
		EndTurn
	}

	private DiceRollMode diceRollMode = DiceRollMode.ForMovement;
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
	InitSoundPlayers();
	InitTurnTimer();
	InitGameEndComponents();
	InitMoveHistory(); 
	SetAllPlayersOnStart();
	InitAchievementManager
	StartTurnTimer();
	playerLabel.Text = GetCurrentPlayerName();
}


private void InitMoveHistory()
{
	moveHistory = GetNodeOrNull<MoveHistory>("/root/Level/MoveHistory");
	if (moveHistory == null)
	{
		GD.PrintErr("Błąd: Nie znaleziono komponentu MoveHistory.");
	}
	else
	{
		GD.Print("System historii ruchów zainicjalizowany.");
		
		// Dodaj sygnał DicesStoppedRolling
		if (!IsConnected(SignalName.DicesStoppedRolling, new Callable(moveHistory, nameof(MoveHistory.OnDicesStoppedRolling))))
		{
			Connect(SignalName.DicesStoppedRolling, new Callable(moveHistory, nameof(MoveHistory.OnDicesStoppedRolling)));
			GD.Print("Sygnał DicesStoppedRolling połączony z MoveHistory.");
		}
	}
}

	private void InitAchievementManager()
	{
		achievementManager = GetNodeOrNull<Node>("/root/Level/GameManager/AchievementManager");
		if (achievementManager == null)
		{
			GD.Print("Nie znaleziono AchievementManagera.");
		}
		GD.Print("jest git");
	}
	private void InitTurnTimer()
	{
		turnTimer = new Timer();
		turnTimer.WaitTime = turnTimeLimit;
		turnTimer.OneShot = true;
		turnTimer.Connect("timeout", new Callable(this, nameof(OnTurnTimerTimeout)));
		AddChild(turnTimer);

		turnTimerLabel = GetNodeOrNull<Label>("/root/Level/UI/TimeAndRounds/HBoxContainer/TurnTimerLabel");
		roundLabel = GetNodeOrNull<Label>("/root/Level/UI/TimeAndRounds/HBoxContainer/MarginContainer2/VBoxContainer/RoundCount");
		playerLabel = GetNodeOrNull<Label>("/root/Level/UI/TimeAndRounds/HBoxContainer/MarginContainer3/VBoxContainer/PlayerLabel");
	}

	private void OnTurnTimerTimeout()
	{
		notificationService.ShowNotification($"Czas gracza {GetCurrentPlayerName()} upłynął!", NotificationService.NotificationType.Normal, 2f);
		EndTurn();
	}

	private void StartTurnTimer()
	{
		turnTimer.Start();
		notificationService.ShowNotification($"Tura gracza {GetCurrentPlayerName()}: 60 sekund", NotificationService.NotificationType.Normal, 2f);
	}

	private void StopTurnTimer()
	{
		turnTimer.Stop();
	}

	public override void _Process(double delta)
	{
		// Aktualizacja wyświetlania czasu, jeśli timer jest aktywny
		if (turnTimer != null && turnTimer.TimeLeft > 0 && turnTimerLabel != null)
		{
			turnTimerLabel.Text = $"CZAS TURY: {Math.Ceiling(turnTimer.TimeLeft)}";
		}
	}
	
	public Figurehead getCurrentPlayer()
	{
		return players[currentPlayerIndex];
	}

	public Field getCurrentField(int position)
	{
		return board.GetFieldById(position);
	}
	
	
	

	
	private void InitSoundPlayers()
	{
		doubleSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/DoubleSound");
		gainECTSSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/GainECTSSound");
		nextTurnSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/NextTurnSound");
		bankruptSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/BankruptSound");
		reviveSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/ReviveSound");
		walkingSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/WalkingSound");
		if (doubleSoundPlayer == null || gainECTSSoundPlayer == null || nextTurnSoundPlayer == null || bankruptSoundPlayer == null ||
			reviveSoundPlayer == null || walkingSoundPlayer == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono jednego z odtwarzaczy dźwięku. Sprawdź ścieżki.");
		}
	}

	private void InitCameras()
	{
		masterCamera = GetNodeOrNull<Camera3D>(masterCameraPath);
		diceCamera = GetNodeOrNull<Camera3D>(diceCameraPath);
		if (masterCamera == null || diceCamera == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono jednej z kamer. Sprawdź ścieżki.", NotificationService.NotificationType.Error);
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
		notificationService = GetNodeOrNull<NotificationService>("/root/NotificationService");
		if (notificationPanel != null) notificationPanel.Visible = false;
	}

	private void InitBoard()
	{
		board = GetNodeOrNull<Board>(boardPath);
		if (board == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono planszy.", NotificationService.NotificationType.Error);
		}
		else
		{
			GD.Print("Plansza została poprawnie załadowana.");
		}
	}

	private void InitPlayers()
	{
		GD.Print($"Szukanie PlayerInitializer pod ścieżką: {playerInitializerPath}");
		Node playerInitializer = GetNodeOrNull(playerInitializerPath);
		if (playerInitializer == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono PlayerInitializer w scenie!", NotificationService.NotificationType.Error);
			return;
		}

		playerInitializer.Call("initialize_players");

		Node playersContainer = GetNodeOrNull(playersContainerPath);
		if (playersContainer == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono węzła Players (3D).", NotificationService.NotificationType.Error);
			return;
		}

		foreach (Node child in playersContainer.GetChildren())
		{
			GD.Print($"Sprawdzam węzeł: {child.Name}");
			if (child is Figurehead fh)
			{
				players.Add(fh);
				playerBankruptcyStatus.Add(false);
			}
		}


		if (players.Count == 0)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono żadnych pionków.", NotificationService.NotificationType.Error);
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
			notificationService.ShowNotification("Błąd: Nie znaleziono jednej z kostek.", NotificationService.NotificationType.Error);
			return;
		}

		dieNode1.Connect("roll_finished", new Callable(this, nameof(OnDie1RollFinished)));
		dieNode2.Connect("roll_finished", new Callable(this, nameof(OnDie2RollFinished)));
		GD.Print("Kostki podłączone.");
	}

	public void InitRollButton()
	{
		rollButton = GetNodeOrNull<Button>(rollButtonPath);
		if (rollButton == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono przycisku do rzucania kostkami.", NotificationService.NotificationType.Error);
			return;
		}

		// Create a custom style for the roll button
		var buttonStyle = new StyleBoxFlat();
		buttonStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
		buttonStyle.BorderWidthBottom = 3;
		buttonStyle.BorderWidthLeft = 3;
		buttonStyle.BorderWidthRight = 3;
		buttonStyle.BorderWidthTop = 3;
		buttonStyle.BorderColor = new Color("#62ff45");
		buttonStyle.CornerRadiusBottomLeft = 8;
		buttonStyle.CornerRadiusBottomRight = 8;
		buttonStyle.CornerRadiusTopLeft = 8;
		buttonStyle.CornerRadiusTopRight = 8;
		buttonStyle.ContentMarginLeft = 15;
		buttonStyle.ContentMarginRight = 15;
		buttonStyle.ContentMarginTop = 8;
		buttonStyle.ContentMarginBottom = 8;

		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.95f);
		hoverStyle.BorderWidthBottom = 3;
		hoverStyle.BorderWidthLeft = 3;
		hoverStyle.BorderWidthRight = 3;
		hoverStyle.BorderWidthTop = 3;
		hoverStyle.BorderColor = new Color("#62ff45");
		hoverStyle.CornerRadiusBottomLeft = 8;
		hoverStyle.CornerRadiusBottomRight = 8;
		hoverStyle.CornerRadiusTopLeft = 8;
		hoverStyle.CornerRadiusTopRight = 8;
		hoverStyle.ContentMarginLeft = 15;
		hoverStyle.ContentMarginRight = 15;
		hoverStyle.ContentMarginTop = 8;
		hoverStyle.ContentMarginBottom = 8;

		rollButton.AddThemeStyleboxOverride("normal", buttonStyle);
		rollButton.AddThemeStyleboxOverride("hover", hoverStyle);
		rollButton.AddThemeStyleboxOverride("pressed", hoverStyle);

		rollButton.AddThemeColorOverride("font_color", new Color(1, 1, 1));
		rollButton.AddThemeColorOverride("font_hover_color", new Color("#FFFFFF"));

		rollButton.AddThemeFontSizeOverride("font_size", 20);

		rollButton.Connect("pressed", new Callable(this, nameof(OnRollButtonPressed)));
		rollButton.Visible = true;
	}

	private void InitEndTurnButton()
	{
		endTurnButton = GetNodeOrNull<Button>(endTurnButtonPath);
		if (endTurnButton == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono przycisku do zakończenia tury.", NotificationService.NotificationType.Error);
			return;
		}

		var buttonStyle = new StyleBoxFlat();
		buttonStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
		buttonStyle.BorderWidthBottom = 3;
		buttonStyle.BorderWidthLeft = 3;
		buttonStyle.BorderWidthRight = 3;
		buttonStyle.BorderWidthTop = 3;
		buttonStyle.BorderColor = new Color("#ff4545");
		buttonStyle.CornerRadiusBottomLeft = 8;
		buttonStyle.CornerRadiusBottomRight = 8;
		buttonStyle.CornerRadiusTopLeft = 8;
		buttonStyle.CornerRadiusTopRight = 8;
		buttonStyle.ContentMarginLeft = 15;
		buttonStyle.ContentMarginRight = 15;
		buttonStyle.ContentMarginTop = 8;
		buttonStyle.ContentMarginBottom = 8;

		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.95f);
		hoverStyle.BorderWidthBottom = 3;
		hoverStyle.BorderWidthLeft = 3;
		hoverStyle.BorderWidthRight = 3;
		hoverStyle.BorderWidthTop = 3;
		hoverStyle.BorderColor = new Color("#ff4545");
		hoverStyle.CornerRadiusBottomLeft = 8;
		hoverStyle.CornerRadiusBottomRight = 8;
		hoverStyle.CornerRadiusTopLeft = 8;
		hoverStyle.CornerRadiusTopRight = 8;
		hoverStyle.ContentMarginLeft = 15;
		hoverStyle.ContentMarginRight = 15;
		hoverStyle.ContentMarginTop = 8;
		hoverStyle.ContentMarginBottom = 8;


		endTurnButton.AddThemeStyleboxOverride("normal", buttonStyle);
		endTurnButton.AddThemeStyleboxOverride("hover", hoverStyle);
		endTurnButton.AddThemeStyleboxOverride("pressed", hoverStyle);

		endTurnButton.AddThemeColorOverride("font_color", new Color(1, 1, 1));
		endTurnButton.AddThemeColorOverride("font_hover_color", new Color("#FFFFFF"));

		endTurnButton.AddThemeFontSizeOverride("font_size", 20);

		endTurnButton.Connect("pressed", new Callable(this, nameof(OnEndTurnButtonPressed)));
		endTurnButton.Visible = false;
	}

	private void InitPlayersUI()
	{
		Node playersUIContainer = GetNodeOrNull(playersUIContainerPath);
		if (playersUIContainer == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono kontenera UI/Players.", NotificationService.NotificationType.Error);
			return;
		}

		for (int i = 0; i < players.Count; i++)
		{
			Node playerUINode = playersUIContainer.GetChild(i);
			if (playerUINode == null)
			{
				notificationService.ShowNotification($"Błąd: Nie znaleziono węzła UI dla gracza {i + 1}", NotificationService.NotificationType.Error);
				continue;
			}

			var marginContainer = playerUINode.GetNodeOrNull("MarginContainer");
			if (marginContainer == null)
			{
				notificationService.ShowNotification($"Błąd: Nieprawidłowa struktura UI (MarginContainer).", NotificationService.NotificationType.Error);
				continue;
			}

			var vBoxContainer = marginContainer.GetNodeOrNull("VBoxContainer");
			if (vBoxContainer == null)
			{
				notificationService.ShowNotification($"Błąd: Nieprawidłowa struktura UI (VBoxContainer).", NotificationService.NotificationType.Error);
				continue;
			}

			var nameLabel = vBoxContainer.GetNodeOrNull<Label>("Label");
			if (nameLabel == null)
			{
				notificationService.ShowNotification($"Błąd: Nie znaleziono Label z nazwą gracza.", NotificationService.NotificationType.Error);
				continue;
			}

			var hBoxContainer = vBoxContainer.GetNodeOrNull("HBoxContainer");
			if (hBoxContainer == null)
			{
				notificationService.ShowNotification($"Błąd: Nie znaleziono HBoxContainer dla ECTS.", NotificationService.NotificationType.Error);
				continue;
			}

			Label ectsLabel = null;
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
				notificationService.ShowNotification($"Błąd: Nie znaleziono Label ECTS.", NotificationService.NotificationType.Error);
				continue;
			}

			nameLabel.Text = players[i].Name;
			ectsLabel.Text = players[i].ECTS.ToString();
			playerNameLabels.Add(nameLabel);
			playerECTSLabels.Add(ectsLabel);
		}

		for (int i = players.Count; i < 4; i++)
		{
			CanvasItem playerUINodeToHide = playersUIContainer.GetChild<CanvasItem>(i);

			if (playerUINodeToHide != null)
			{
				playerUINodeToHide.Visible = false;
			}
		}
		
		CanvasItem PlayerStatsPanel = GetNodeOrNull<CanvasItem>(playersStatsContainerPath);
		if (playersStatsContainerPath == null)
		{
			notificationService.ShowNotification("Błąd: Nie znaleziono kontenera playerStatsContainer.", NotificationService.NotificationType.Error);
			return;
		}
		
		PlayerStatsPanel.Visible = false;
	}

	// Metoda inicjalizująca komponenty ekranu końca gry
	private void InitGameEndComponents()
	{
		// Ładowanie sceny ekranu końcowego
		gameEndScreenScene = ResourceLoader.Load<PackedScene>("res://scenes/end_game/end_game_screen.tscn");

		// Jeśli scena nie istnieje, tworzymy specjalny CanvasLayer jako tymczasowe rozwiązanie
		if (gameEndScreenScene == null)
		{
			GD.Print("Ekran końcowy nie został znaleziony. Tworzenie tymczasowego ekranu.");
			gameEndScreen = new CanvasLayer();
			gameEndScreen.Name = "EndGameScreen";
			gameEndScreen.Layer = 10; // Wysoki numer warstwy, żeby był nad wszystkim
			gameEndScreen.Visible = false;

			// Panel zajmujący cały ekran
			var panel = new Panel();
			panel.AnchorRight = 1;
			panel.AnchorBottom = 1;

			gameEndScreen.AddChild(panel);

			// Kontener dla elementów UI
			var container = new VBoxContainer();
			container.AnchorRight = 1;
			container.AnchorBottom = 1;
			container.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			container.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			container.Alignment = BoxContainer.AlignmentMode.Center;
			panel.AddChild(container);

			// Etykieta końca gry
			var endGameLabel = new Label();
			endGameLabel.Text = "KONIEC GRY";
			endGameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			endGameLabel.AddThemeConstantOverride("font_size", 42);
			container.AddChild(endGameLabel);

			// Kontener na wyniki
			gameResultsContainer = new VBoxContainer();
			gameResultsContainer.Name = "ResultsContainer";
			gameResultsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			gameResultsContainer.CustomMinimumSize = new Vector2(400, 300);
			container.AddChild(gameResultsContainer);

			// Przycisk powrotu do menu
			returnToMenuButton = new Button();
			returnToMenuButton.Name = "ReturnToMenuButton";
			returnToMenuButton.Text = "POWRÓT DO MENU";
			returnToMenuButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
			returnToMenuButton.CustomMinimumSize = new Vector2(200, 50);
			returnToMenuButton.Connect("pressed", new Callable(this, nameof(OnReturnToMenuPressed)));
			container.AddChild(returnToMenuButton);

			AddChild(gameEndScreen);
		}
		else
		{
			// Używanie istniejącej sceny
			gameEndScreen = gameEndScreenScene.Instantiate<CanvasLayer>();
			gameEndScreen.Visible = false;
			AddChild(gameEndScreen);

			returnToMenuButton = gameEndScreen.GetNodeOrNull<Button>(returnToMenuButtonPath);
			if (returnToMenuButton != null)
			{
				returnToMenuButton.Connect("pressed", new Callable(this, nameof(OnReturnToMenuPressed)));
			}

			gameResultsContainer = gameEndScreen.GetNodeOrNull<VBoxContainer>(gameResultsContainerPath);
		}

		GD.Print("Komponenty ekranu końcowego zostały zainicjalizowane.");
	}

	public void UpdateECTSUI(int playerIndex)
	{
		if (playerIndex < 0 || playerIndex >= playerECTSLabels.Count)
		{
			notificationService.ShowNotification("Błąd: Indeks gracza poza zakresem podczas aktualizacji ECTS UI.",
				NotificationService.NotificationType.Error);
			return;
		}

		playerECTSLabels[playerIndex].Text = players[playerIndex].ECTS.ToString();
		CheckForBankruptcy(playerIndex);
	}

	private void CheckForBankruptcy(int playerIndex)
	{
		if (playerIndex < 0 || playerIndex >= players.Count)
		{
			return;
		}

		Figurehead player = players[playerIndex];
		if (player.ECTS <= 0 && !playerBankruptcyStatus[playerIndex])
		{
			DeclarePlayerBankrupt(playerIndex);
		}
	}

private void DeclarePlayerBankrupt(int playerIndex)
{
	if (playerIndex < 0 || playerIndex >= players.Count)
	{
		return;
	}

	Figurehead player = players[playerIndex];
	playerBankruptcyStatus[playerIndex] = true;

	if (playerIndex < playerNameLabels.Count)
	{
		playerNameLabels[playerIndex].Text = $"{player.Name} (BANKRUT)";
		playerNameLabels[playerIndex].AddThemeColorOverride("font_color", new Color(1, 0, 0));
		player.Visible = false;
	}

	PlaySound(bankruptSoundPlayer);
	notificationService.ShowNotification($"Gracz {player.Name} zbankrutował! Koniec gry dla tego gracza.",
		NotificationService.NotificationType.Normal,
		5f);
	GD.Print($"Gracz {player.Name} zbankrutował! Koniec gry dla tego gracza.");
	
	// Dodaj wpis do historii ruchów
	if (moveHistory != null)
	{
		moveHistory.AddActionEntry(player.Name, "zbankrutował!");
	}

	if (playerIndex == currentPlayerIndex)
	{
		EndTurn();
	}

	CheckForGameOver();
}

	private void CheckForGameOver()
	{
		if (isGameOver) return;

		int activePlayers = 0;
		int lastActivePlayerIndex = -1;
		for (int i = 0; i < playerBankruptcyStatus.Count; i++)
		{
			if (!playerBankruptcyStatus[i])
			{
				activePlayers++;
				lastActivePlayerIndex = i;
			}
		}

		if (activePlayers == 1)
		{
			EndGameWithWinner(lastActivePlayerIndex);
		}
		else if (activePlayers == 0)
		{
			EndGameWithNoWinner();
		}
	}

	private void EndGameWithWinner(int winnerIndex)
	{
		if (isGameOver) return;
		isGameOver = true;

		DisableGameControls();

		notificationService.ShowNotification(
			$"Gra zakończona! Gracz {players[winnerIndex].Name} wygrywa!",
			NotificationService.NotificationType.Normal,
			5f
		);
		
		GD.Print($"Gra zakończona! Gracz {players[winnerIndex].Name} wygrywa!");
		achievementManager.Call("track_game_win",currentRound);
		ShowEndGameScreen($"Gracz {players[winnerIndex].Name} wygrywa!");
	}

	private void EndGameWithNoWinner()
	{
		if (isGameOver) return;
		isGameOver = true;

		DisableGameControls();

		notificationService.ShowNotification(
			"Gra zakończona! Wszyscy gracze zbankrutowali!",
			NotificationService.NotificationType.Normal,
			5f
		);

		GD.Print("Gra zakończona! Wszyscy gracze zbankrutowali!");
		ShowEndGameScreen("Wszyscy gracze zbankrutowali! Remis!");
	}

	private void EndGameByRoundLimit()
	{
		if (isGameOver) return;
		isGameOver = true;

		DisableGameControls();

		// Znajdujemy gracza z największą ilością ECTS
		int winnerIndex = FindRichestPlayer();

		notificationService.ShowNotification(
			$"Osiągnięto limit {maxRounds} rund! Gracz {players[winnerIndex].Name} wygrywa z największą ilością ECTS!",
			NotificationService.NotificationType.Normal,
			5f
		);

		GD.Print($"Osiągnięto limit {maxRounds} rund! Gracz {players[winnerIndex].Name} wygrywa!");
		ShowEndGameScreen($"Limit rund osiągnięty! Gracz {players[winnerIndex].Name} wygrywa!");
	}

	private int FindRichestPlayer()
	{
		int richestPlayerIndex = -1;
		int maxECTS = -1;

		for (int i = 0; i < players.Count; i++)
		{
			if (!playerBankruptcyStatus[i] && players[i].ECTS > maxECTS)
			{
				maxECTS = players[i].ECTS;
				richestPlayerIndex = i;
			}
		}

		return richestPlayerIndex;
	}

	private void DisableGameControls()
	{
		rollButton.Disabled = true;
		endTurnButton.Disabled = true;
		StopTurnTimer();

		// Wyłączamy interakcje z planszą
		if (board != null)
		{
			foreach (Field field in board.GetFields())
			{
				field.isMouseEventEnabled = false;
			}
		}
	}

	private void ShowEndGameScreen(string resultMessage)
	{
		// Upewnij się, że ekran końcowy jest inicjalizowany
		if (gameEndScreen == null)
		{
			InitGameEndComponents();
		}

		// Wyświetl ekran końcowy
		gameEndScreen.Visible = true;

		// Przygotuj tablicę wyników graczy
		Godot.Collections.Array playerResults = new Godot.Collections.Array();

		// Sortuj graczy wg ilości ECTS (malejąco)
		var sortedPlayers = new List<(Figurehead player, int index)>();
		for (int i = 0; i < players.Count; i++)
		{
			sortedPlayers.Add((players[i], i));
		}
		sortedPlayers.Sort((a, b) => b.player.ECTS.CompareTo(a.player.ECTS));

		// Dodaj dane każdego gracza do tablicy wyników
		foreach (var (player, index) in sortedPlayers)
		{
			var playerResult = new Godot.Collections.Dictionary();
			playerResult.Add("name", player.Name);
			playerResult.Add("ects", player.ECTS);
			playerResult.Add("bankrupt", playerBankruptcyStatus[index]);
			playerResults.Add(playerResult);
		}

		// Wywołaj metodę set_results z ekranu końcowego
		gameEndScreen.Call("set_results", resultMessage, playerResults);
	}

	// Metoda do obsługi przycisku powrotu do menu
	private void OnReturnToMenuPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/main_menu/main_menu.tscn");
	}

	private void SetAllPlayersOnStart()
	{
		if (board == null)
		{
			notificationService.ShowNotification("Board is not initialized. Cannot set players on start.", NotificationService.NotificationType.Error);
			return;
		}

		for (int i = 0; i < players.Count; i++)
		{
			Figurehead player = players[i];
			Vector3? startPosition = board.GetPositionForPawn(0, i % board.GetFieldById(0).positions.Count);
			if (!startPosition.HasValue)
			{
				notificationService.ShowNotification($"Błąd: Nie znaleziono pozycji startowej dla gracza {i + 1}.",
					NotificationService.NotificationType.Error);
				continue;
			}

			player.CurrentPositionIndex = 0;
			player.GlobalPosition = startPosition.Value;
			GD.Print($"Gracz {players[i].Name} ustawiony na pozycji startowej: {startPosition.Value}");
			UpdateECTSUI(i);
		}

		GD.Print("Wszyscy gracze zostali ustawieni na polu startowym.");
	}

private void OnRollButtonPressed()
{
	if (currentState == GameState.WaitingForInput)
	{
		if (IsCurrentPlayerBankrupt())
		{
			SkipBankruptPlayer();
			return;
		}

		// Dodaj wpis do historii przed wykonaniem rzutu
		if (moveHistory != null)
		{
			moveHistory.AddActionEntry(GetCurrentPlayerName(), "przygotowuje się do rzutu kostkami");
		}

		StartDiceRollForCurrentPlayer();
	}
}

	private void OnEndTurnButtonPressed()
	{
		if (!isMovementInProgress && currentState == GameState.WaitingForInput)
		{
			EndTurn();
		}
	}

	private bool IsCurrentPlayerBankrupt()
	{
		if (currentPlayerIndex >= 0 && currentPlayerIndex < playerBankruptcyStatus.Count)
		{
			return playerBankruptcyStatus[currentPlayerIndex];
		}

		return false;
	}

	private void SkipBankruptPlayer()
	{
		GD.Print($"Skipping bankrupt player: {players[currentPlayerIndex].Name}");
		EndTurn();
	}


	// Metoda do aktualizacji licznika rund
	private void UpdateRoundCounter()
	{
		// Pełna runda kończy się, gdy wszyscy gracze wykonali swój ruch
		if (currentPlayerIndex == 0)
		{
			currentRound++;
			
			GD.Print($"Runda {currentRound}");
			roundLabel.Text = $"{currentRound}";
			if (maxRounds > 0 && currentRound > maxRounds)
			{
				EndGameByRoundLimit();
			}
		}
	}

	public void StartDiceRollForCurrentPlayer(DiceRollMode mode = DiceRollMode.ForMovement)
	{
		if (dieNode1 == null || dieNode2 == null)
		{
			notificationService.ShowNotification("Nie można rzucić kostkami: kostki nie są zainicjalizowane.");
			return;
		}

		die1Result = null;
		die2Result = null;

		StopTurnTimer();

		currentState = GameState.RollingDice;
		SetBoardInteractions(false);
		SwitchToDiceCamera();
		rollButton.Visible = false;
		endTurnButton.Visible = false;

		diceRollMode = mode;

		string currentPlayerName = GetCurrentPlayerName();
		notificationService.ShowNotification($"Gracz {currentPlayerName} rzuca kostkami...", NotificationService.NotificationType.Normal, 2f);
		GD.Print($"Rzut kostkami dla gracza: {currentPlayerName}");
		dieNode1.Call("_roll");
		dieNode2.Call("_roll");
	}

	private void OnDie1RollFinished(int value)
	{
		GD.Print($"Wynik pierwszej kostki: {value}");
		die1Result = value;

		if (BothDicesFinished())
			HandleBothDicesFinished();
	}

	private void OnDie2RollFinished(int value)
	{
		GD.Print($"Wynik drugiej kostki: {value}");
		die2Result = value;

		if (BothDicesFinished())
			HandleBothDicesFinished();
	}

	private bool BothDicesFinished()
	{
		if (!die1Result.HasValue || !die2Result.HasValue)
		{
			return false;
		}

		return true;
	}

private void HandleBothDicesFinished()
{
	lastDiceTotal = die1Result.Value + die2Result.Value;
	GD.Print($"Oba rzuty zakończone. Wynik sumaryczny: {lastDiceTotal}");
	
	// Emituj sygnał przed innymi operacjami
	EmitSignal(SignalName.DicesStoppedRolling);
	
	switch (diceRollMode)
	{
		case DiceRollMode.ForMovement:
			HandleRegularRoll(lastDiceTotal);
			break;

		case DiceRollMode.JustForDisplay:
			EmitSignal(SignalName.SpecialRollEnded);
			ShowDiceResultsOnly(lastDiceTotal);
			break;
	}
}

	private void ShowDiceResultsOnly(int total)
	{
		GD.Print($"Wyrzucono {die1Result}+{die2Result} = {total} (tylko podgląd)");
		SwitchToMasterCamera();
		SetBoardInteractions(true);

		AdjustGameStateAfterRoll();
	}

	private int CheckDiceResults()
	{
		int rollSum = die1Result.Value + die2Result.Value;

		return rollSum;
	}

	private void HandleRegularRoll(int totalSteps)
	{
		SwitchToMasterCamera();
		MoveCurrentPlayerPawnSequentially(totalSteps);
		if (die1Result.Value == die2Result.Value)
		{
			GD.Print("Dublet! Kolejny rzut po ruchu.");
			achievementManager.Call("track_dice_roll", true);
			IsLastRegularRollDouble = true;
			PlaySound(doubleSoundPlayer);
		}
		else
		{
			GD.Print("Nie wyrzucono dubletu. Przygotowanie do zakończenia tury.");
		}
	}

	private void PlaySound(AudioStreamPlayer3D player)
	{
		if (player != null)
		{
			player.Play();
			GD.Print("Odtwarzanie dźwięku.");
		}
		else
		{
			GD.PrintErr("Błąd: AudioStreamPlayer3D nie jest zainicjalizowany.");
		}
	}

	private async void MoveCurrentPlayerPawnSequentially(int steps)
	{
		if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
		{
			notificationService.ShowNotification("Błąd: Indeks aktualnego gracza poza zakresem.");
			return;
		}

		currentState = GameState.MovingPawn;
		Figurehead currentPlayer = players[currentPlayerIndex];
		endTurnButton.Visible = false;
		isMovementInProgress = true;
		await currentPlayer.MovePawnSequentially(steps, board); //zamien 12 na steps
		isMovementInProgress = false;
		// Poczekaj na zakończenie wszystkich animacji i efektów
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		// Check for bankruptcy after movement (ECTS might have changed during movement)
		CheckForBankruptcy(currentPlayerIndex);
		// If the player is now bankrupt, end their turn
		if (IsCurrentPlayerBankrupt())
		{
			die1Result = null;
			die2Result = null;
			currentState = GameState.WaitingForInput;
			EndTurn();
			return;
		}
		StartTurnTimer();

		AdjustGameStateAfterRoll();
	}

	private async void AdjustGameStateAfterRoll()
	{
		if (IsSpecialRollHappening)
		{
			await ToSignal(this, SignalName.SpecialRollEnded);
			IsSpecialRollHappening = false;
		}

		if (IsLastRegularRollDouble)
		{
			PrepareForNextRoll();
			IsLastRegularRollDouble = false;
		}
		else
		{
			currentState = GameState.WaitingForInput;
			endTurnButton.Visible = true;
		}
		UpdateECTSUI(currentPlayerIndex);

	}

	private void PrepareForNextRoll()
	{
		if (isMovementInProgress) return;

		StopTurnTimer(); // zatrzymujemy timer
		StartTurnTimer(); // resetujemy timer dla tego samego gracza

		die1Result = null;
		die2Result = null;
		totalSteps = 0;
		SetBoardInteractions(true);
		currentState = GameState.WaitingForInput;
		rollButton.Visible = true;
		endTurnButton.Visible = false;
		string currentPlayerName = GetCurrentPlayerName();
		notificationService.ShowNotification($"Gracz {currentPlayerName}, rzuć ponownie!", NotificationService.NotificationType.Normal, 2f);
		GD.Print($"Gracz {currentPlayerName} może wykonać kolejny rzut.");
	}

private void EndTurn()
{
	if (isMovementInProgress) return;

	StopTurnTimer(); // zatrzymujemy timer przed zmianą gracza

	string previousPlayerName = GetCurrentPlayerName();
	
	die1Result = null;
	die2Result = null;
	totalSteps = 0;
	// Find next active (non-bankrupt) player
	FindNextActivePlayer();
	SetBoardInteractions(true);
	currentState = GameState.WaitingForInput;
	rollButton.Visible = true;
	endTurnButton.Visible = false;
	string nextPlayerName = GetCurrentPlayerName();
	playerLabel.Text = nextPlayerName;
	PlaySound(nextTurnSoundPlayer);
	notificationService.ShowNotification($"Tura gracza: {nextPlayerName}", NotificationService.NotificationType.Normal, 3f);
	GD.Print($"Zakończono turę gracza {previousPlayerName}. Teraz tura gracza: {nextPlayerName}");
	
	// Dodaj wpis do historii ruchów - upewnij się, że moveHistory nie jest null
	if (moveHistory != null)
	{
		moveHistory.AddActionEntry(previousPlayerName, "zakończył swoją turę");
		GD.Print("Dodano wpis o zakończeniu tury do historii ruchów.");
	}
	else
	{
		GD.PrintErr("Komponent MoveHistory jest null - nie można dodać wpisu do historii.");
	}
	
	UpdateRoundCounter();
	
	// Uruchamiamy timer dla nowego gracza
	StartTurnTimer();
}

	/// Find the next non-bankrupt player
	private void FindNextActivePlayer()
	{
		int originalIndex = currentPlayerIndex;
		int nextPlayerIndex;
		do
		{
			nextPlayerIndex = (currentPlayerIndex + 1) % players.Count;
			currentPlayerIndex = nextPlayerIndex;
			// If we've checked all players and returned to the original index, break to avoid infinite loop
			if (nextPlayerIndex == originalIndex)
			{
				break;
			}
		} while (playerBankruptcyStatus[currentPlayerIndex]);
		// If everyone is bankrupt, currentPlayerIndex will remain unchanged
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
			notificationService.ShowNotification("Błąd: Próba aktywacji nieistniejącej kamery.", NotificationService.NotificationType.Error);
		}
	}

	private void HideNotification()
	{
		if (notificationLabel == null || notificationPanel == null) return;
		notificationPanel.Visible = false;
		notificationLabel.Visible = false;
	}

	private void SetBoardInteractions(bool isInteractive)
	{
		if (board == null) return;
		foreach (Field field in board.GetFields())
		{
			field.isMouseEventEnabled = isInteractive;
		}
	}

	public void UpdateECTS(int playerIndex)
	{
		UpdateECTSUI(playerIndex);
	}

	private string GetCurrentPlayerName()
	{
		if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
		{
			notificationService.ShowNotification("Błąd: Indeks aktualnego gracza poza zakresem podczas pobierania nazwy.",
				NotificationService.NotificationType.Error);
			return "Nieznany";
		}
		return players[currentPlayerIndex].Name;
	}

	// Metody do obsługi kart
	public int GetCurrentPlayerIndex()
	{
		return currentPlayerIndex;
	}

	public void AddEctsToPlayer(int playerIndex, int amount)
	{
		if (playerIndex >= 0 && playerIndex < players.Count)
		{
			players[playerIndex].AddECTS(amount);
			PlaySound(gainECTSSoundPlayer);
			UpdateECTSUI(playerIndex);
			// If player was bankruptcy but got back to positive ECTS, remove bankruptcy status
			if (amount > 0 && playerBankruptcyStatus[playerIndex] && players[playerIndex].ECTS > 0)
			{
				RevivePlayer(playerIndex);
			}
		}
	}

	/// Method to revive a player if they gain ECTS after bankruptcy
	private void RevivePlayer(int playerIndex)
	{
		if (playerIndex < 0 || playerIndex >= players.Count)
		{
			return;
		}

		// Only revive if the player has positive ECTS
		if (players[playerIndex].ECTS > 0)
		{
			playerBankruptcyStatus[playerIndex] = false;
			// Update UI to remove bankrupt status
			if (playerIndex < playerNameLabels.Count)
			{
				playerNameLabels[playerIndex].Text = players[playerIndex].Name;
				// Reset text color
				playerNameLabels[playerIndex].RemoveThemeColorOverride("font_color");
			}

			PlaySound(reviveSoundPlayer);
			players[playerIndex].Visible = true;
			notificationService.ShowNotification($"Gracz {players[playerIndex].Name} wraca do gry!", NotificationService.NotificationType.Normal, 5f);
			GD.Print($"Gracz {players[playerIndex].Name} wraca do gry!");
		}
	}

	public void SubtractEctsFromPlayer(int playerIndex, int amount)
	{
		if (playerIndex >= 0 && playerIndex < players.Count)
		{
			GD.Print("...");
			players[playerIndex].SpendECTS(amount);
			UpdateECTSUI(playerIndex);
		}
	}

	public void process_trade(int player1Id, int player2Id, int field1Id, int field2Id, int player1Ects, int player2Ects)
	{
		GD.Print(
			$"Processing trade: P1({player1Id}) gives field {field1Id} and {player1Ects} ECTS to P2({player2Id}) for field {field2Id} and {player2Ects} ECTS");

		// Validate player IDs
		if (player1Id < 0 || player1Id >= players.Count || player2Id < 0 || player2Id >= players.Count)
		{
			notificationService.ShowNotification("Błąd wymiany: Nieprawidłowi gracze", NotificationService.NotificationType.Error);
			return;
		}

		Figurehead player1 = players[player1Id];
		Figurehead player2 = players[player2Id];

		// Check if either player is bankrupt
		if (playerBankruptcyStatus[player1Id] || playerBankruptcyStatus[player2Id])
		{
			notificationService.ShowNotification("Nie można handlować z bankrutem", NotificationService.NotificationType.Error);
			return;
		}

		// Check ECTS balances
		if (player1.ECTS < player1Ects)
		{
			notificationService.ShowNotification($"{player1.Name} nie ma wystarczającej ilości ECTS", NotificationService.NotificationType.Error);
			return;
		}

		if (player2.ECTS < player2Ects)
		{
			notificationService.ShowNotification($"{player2.Name} nie ma wystarczającej ilości ECTS", NotificationService.NotificationType.Error);
			return;
		}

		// Track exchange items for notifications
		string player1GainedItems = "";
		string player1LostItems = "";
		string player2GainedItems = "";
		string player2LostItems = "";

		// Process field exchange if applicable
		if (field1Id >= 0)
		{
			Field field1 = board.GetFieldById(field1Id);
			if (field1 != null && field1.owned && field1.Owner == player1)
			{
				// Transfer field1 from player1 to player2
				ProcessPropertyTransfer(field1, player1, player2);
				player1LostItems += field1.Name;
				player2GainedItems += field1.Name;
				GD.Print($"Pole {field1.Name} przekazane od {player1.Name} do {player2.Name}");
			}
		}

		if (field2Id >= 0)
		{
			Field field2 = board.GetFieldById(field2Id);
			if (field2 != null && field2.owned && field2.Owner == player2)
			{
				// Transfer field2 from player2 to player1
				ProcessPropertyTransfer(field2, player2, player1);
				player2LostItems += field2.Name;
				player1GainedItems += field2.Name;
				GD.Print($"Pole {field2.Name} przekazane od {player2.Name} do {player1.Name}");
			}
		}

		// Process ECTS exchange if applicable
		if (player1Ects > 0)
		{
			player1.SpendECTS(player1Ects);
			player2.AddECTS(player1Ects);

			if (player1LostItems.Length > 0) player1LostItems += " i ";
			player1LostItems += $"{player1Ects} ECTS";

			if (player2GainedItems.Length > 0) player2GainedItems += " i ";
			player2GainedItems += $"{player1Ects} ECTS";

			GD.Print($"{player1Ects} ECTS przekazane od {player1.Name} do {player2.Name}");
			UpdateECTSUI(player1Id);
			UpdateECTSUI(player2Id);
		}

		if (player2Ects > 0)
		{
			player2.SpendECTS(player2Ects);
			player1.AddECTS(player2Ects);

			if (player2LostItems.Length > 0) player2LostItems += " i ";
			player2LostItems += $"{player2Ects} ECTS";

			if (player1GainedItems.Length > 0) player1GainedItems += " i ";
			player1GainedItems += $"{player2Ects} ECTS";

			GD.Print($"{player2Ects} ECTS przekazane od {player2.Name} do {player1.Name}");
			UpdateECTSUI(player1Id);
			UpdateECTSUI(player2Id);
		}

		if (!string.IsNullOrEmpty(player1GainedItems) || !string.IsNullOrEmpty(player1LostItems))
		{
			// Show trade notifications using existing methods
			notificationService.ShowNotification($"Wymiana z {player2.Name}: Otrzymano {player1GainedItems}, oddano {player1LostItems}",
				NotificationService.NotificationType.Normal,
				4f);
			notificationService.ShowNotification($"Wymiana z {player1.Name}: Otrzymano {player2GainedItems}, oddano {player2LostItems}",
				NotificationService.NotificationType.Normal,
				4f);

			// Play success sound
			if (gainECTSSoundPlayer != null)
			{
				gainECTSSoundPlayer.Play();
			}

			notificationService.ShowNotification("Wymiana zakończona pomyślnie", NotificationService.NotificationType.Normal, 3f);
		}
		else
		{
			notificationService.ShowNotification("Nie wymieniono żadnych przedmiotów", NotificationService.NotificationType.Normal);
		}
	}

	private void ProcessPropertyTransfer(Field field, Figurehead fromPlayer, Figurehead toPlayer)
	{
		if (field == null || fromPlayer == null || toPlayer == null) return;

		int fromPlayerId = players.IndexOf(fromPlayer);
		int toPlayerId = players.IndexOf(toPlayer);

		if (fromPlayerId < 0 || toPlayerId < 0) return;

		// Remove buildings if present and refund half the cost to the seller
		int buildingRefund = 0;

		if (field.builtHouses.Count > 0 || field.isHotel)
		{
			// Count houses
			int houseCount = 0;
			foreach (bool occupied in field.buildOccupied)
			{
				if (occupied) houseCount++;
			}

			if (field.isHotel)
			{
				buildingRefund = field.hotelCost / 2;
			}
			else
			{
				buildingRefund = (houseCount * field.houseCost) / 2;
			}

			// Give refund to original owner
			if (buildingRefund > 0)
			{
				fromPlayer.AddECTS(buildingRefund);
				UpdateECTSUI(fromPlayerId);
			}

			// Remove all buildings
			field.RemoveAllHouses();
			field.isHotel = false;
		}

		fromPlayer.ownedFields[field.FieldId] = false;
		toPlayer.ownedFields[field.FieldId] = true;

		// Update field ownership
		field.Owner = toPlayer;
		field.OwnerId = toPlayerId;
		field.owned = true;

		// Update owner border color
		var ownerBorder = field.GetNodeOrNull<Sprite3D>("OwnerBorder");
		if (ownerBorder != null)
		{
			ownerBorder.Modulate = toPlayer.playerColor;
			ownerBorder.Visible = true;
		}

		// Report building refund if applicable
		if (buildingRefund > 0)
		{
			notificationService.ShowNotification(
				$"Budynki na polu {field.Name} zostały usunięte. Gracz {fromPlayer.Name} otrzymał zwrot {buildingRefund} ECTS.",
				NotificationService.NotificationType.Normal,
				3f
			);
		}
	}

	// Get player index from Figurehead reference
	public int GetPlayerIndex(Figurehead player)
	{
		if (player == null)
			return -1;

		return players.IndexOf(player);
	}
	public Figurehead GetPlayerById(int playerId)
	{
		if (playerId >= 0 && playerId < players.Count)
			return players[playerId];
		return null;
	}

	// Add to GameManager.cs
	public int GetPlayerECTS(int playerIndex)
	{
		if (playerIndex >= 0 && playerIndex < players.Count)
			return players[playerIndex].ECTS;
		return 0;
	}
	
	public int GetCurrentRound()
{
	return currentRound;
}

}
