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
	[Export] public NodePath ectsUIContainerPath;
	[Export] public NodePath playersUIContainerPath;
	[Export] public NodePath playerInitializerPath;
	[Export] public NodePath notificationServicePath;
	[Export] public float turnTimeLimit = 60.0f; // czas tury w sekundach

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
	private List<Label> playerNameLabels = new List<Label>();
	private List<Label> playerECTSLabels = new List<Label>();
	private List<bool> playerBankruptcyStatus = new List<bool>();
	private Timer turnTimer;
	private Label turnTimerLabel;
	private NotificationService notificationService;
	public bool regularRoll = true;
	//public dla wymian
	public List<Figurehead> Players
	{
		get { return players; }
	}

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
		if (doubleSoundPlayer == null || doubleSoundPlayer == null || doubleSoundPlayer == null || bankruptSoundPlayer == null ||
			reviveSoundPlayer == null)
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
		
		for (int i = players.Count; i < 4; i++){
			CanvasItem playerUINodeToHide = playersUIContainer.GetChild<CanvasItem>(i);
			
			if (playerUINodeToHide != null){
				playerUINodeToHide.Visible = false;
			}
		}
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
			notificationService.ShowNotification($"Gra zakończona! Gracz {players[lastActivePlayerIndex].Name} wygrywa!", NotificationService.NotificationType.Normal, 10f);
			GD.Print($"Gra zakończona! Gracz {players[lastActivePlayerIndex].Name} wygrywa!");
			rollButton.Disabled = true;
			endTurnButton.Disabled = true;
		}
		else if (activePlayers == 0)
		{
			notificationService.ShowNotification("Gra zakończona! Wszyscy gracze zbankrutowali!", NotificationService.NotificationType.Normal, 10f);
			GD.Print("Gra zakończona! Wszyscy gracze zbankrutowali!");
			rollButton.Disabled = true;
			endTurnButton.Disabled = true;
		}
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
			await currentPlayer.MovePawnSequentially(steps, board);
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
