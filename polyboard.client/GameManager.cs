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
	[Export] public NodePath rollButtonPath;
	[Export] public NodePath endTurnButtonPath;
	[Export] public NodePath ectsUIContainerPath;
	[Export] public NodePath playersUIContainerPath;
	[Export] public NodePath playerInitializerPath;
	[Export] public NodePath notificationServicePath;
	[Export] public float turnTimeLimit = 60.0f; // czas tury w sekundach
	[Export] public int maxRounds = 30; // Maksymalna liczba rund, po której gra się kończy (ustaw 0 dla braku limitu)
	[Export] public NodePath gameEndScreenPath; // Ścieżka do ekranu końcowego
	[Export] public NodePath returnToMenuButtonPath; // Przycisk powrotu do menu
	[Export] public NodePath gameResultsContainerPath; // Kontener na wyniki końcowe

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
	private bool isGameOver = false;
	public bool regularRoll = true;

	private enum GameState
	{
		WaitingForInput,
		RollingDice,
		MovingPawn,
		EndTurn
	}

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
		SetAllPlayersOnStart();
		StartTurnTimer();
	}

	private void InitTurnTimer()
	{
		turnTimer = new Timer();
		turnTimer.WaitTime = turnTimeLimit;
		turnTimer.OneShot = true;
		turnTimer.Connect("timeout", new Callable(this, nameof(OnTurnTimerTimeout)));
		AddChild(turnTimer);

		turnTimerLabel = GetNodeOrNull<Label>("/root/Level/UI/TurnTimerLabel");
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
			turnTimerLabel.Text = $"Czas: {Math.Ceiling(turnTimer.TimeLeft)}s";
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
		
		for (int i = players.Count; i < 4; i++){
			CanvasItem playerUINodeToHide = playersUIContainer.GetChild<CanvasItem>(i);
			
			if (playerUINodeToHide != null){
				playerUINodeToHide.Visible = false;
			}
		}
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
			notificationService.ShowNotification("Błąd: Indeks gracza poza zakresem podczas aktualizacji ECTS UI.", NotificationService.NotificationType.Error);
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
		notificationService.ShowNotification($"Gracz {player.Name} zbankrutował! Koniec gry dla tego gracza.", NotificationService.NotificationType.Normal, 5f);
		GD.Print($"Gracz {player.Name} zbankrutował! Koniec gry dla tego gracza.");

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
				notificationService.ShowNotification($"Błąd: Nie znaleziono pozycji startowej dla gracza {i + 1}.", NotificationService.NotificationType.Error);
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
			
			if (maxRounds > 0 && currentRound > maxRounds)
			{
				EndGameByRoundLimit();
			}
		}
	}

	private void StartDiceRollForCurrentPlayer()
	{
		if (dieNode1 == null || dieNode2 == null)
		{
			notificationService.ShowNotification("Nie można rzucić kostkami: kostki nie są zainicjalizowane.");
			return;
		}

		StopTurnTimer();

		currentState = GameState.RollingDice;
		SetBoardInteractions(false);
		SwitchToDiceCamera();
		rollButton.Visible = false;
		endTurnButton.Visible = false;
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
		notificationService.ShowNotification($"Łączna suma oczek: {totalSteps}", NotificationService.NotificationType.Normal, 5f);
		GD.Print($"Łączna suma oczek: {totalSteps}");
		SwitchToMasterCamera();
		if (regularRoll)
		{
			MoveCurrentPlayerPawnSequentially(totalSteps);
			if (die1Result.Value == die2Result.Value)
			{
				GD.Print("Dublet! Kolejny rzut po ruchu.");
				PlaySound(doubleSoundPlayer);
				notificationService.ShowNotification("Dublet! Powtórz rzut po ruchu.", NotificationService.NotificationType.Normal, 5f);
			}
			else if (!regularRoll)
			{
				regularRoll = true;
				board.publicFee = totalSteps;
				board.publicFacilityDone = true;
			}
			else
			{
				GD.Print("Nie wyrzucono dubletu. Przygotowanie do zakończenia tury.");
				notificationService.ShowNotification("Nie wyrzucono dubletu. Możesz zakończyć turę.", NotificationService.NotificationType.Normal, 2f);
			}
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
		
		// Play walking sound before starting movement
		PlaySound(walkingSoundPlayer);
		
		await currentPlayer.MovePawnSequentially(steps, board);
		
		// Stop walking sound after movement is complete
		StopSound(walkingSoundPlayer);
		
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

		if (die1Result.HasValue && die2Result.HasValue)
		{
			if (die1Result.Value != die2Result.Value)
			{
				currentState = GameState.WaitingForInput;
				endTurnButton.Visible = true;
			}
			else
			{
				PrepareForNextRoll();
			}
		}
		UpdateECTSUI(currentPlayerIndex);
	}
	
	private void StopSound(AudioStreamPlayer3D player)
	{
		if (player != null && player.Playing)
		{
			player.Stop();
			GD.Print("Zatrzymano dźwięk.");
		}
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
		if (isMovementInProgress || isGameOver) return;

		StopTurnTimer(); // zatrzymujemy timer przed zmianą gracza

		die1Result = null;
		die2Result = null;
		totalSteps = 0;
		// Find next active (non-bankrupt) player
		FindNextActivePlayer();
		
		// Sprawdź, czy rozpoczyna się nowa runda
		if (currentPlayerIndex == 0)
		{
			UpdateRoundCounter();
		}
		
		SetBoardInteractions(true);
		currentState = GameState.WaitingForInput;
		rollButton.Visible = true;
		endTurnButton.Visible = false;
		string nextPlayerName = GetCurrentPlayerName();
		PlaySound(nextTurnSoundPlayer);
		notificationService.ShowNotification($"Tura gracza: {nextPlayerName}", NotificationService.NotificationType.Normal, 3f);
		GD.Print($"Zakończono turę gracza. Teraz tura gracza: {nextPlayerName}");

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
			notificationService.ShowNotification("Błąd: Indeks aktualnego gracza poza zakresem podczas pobierania nazwy.", NotificationService.NotificationType.Error);
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
}
