using Godot;
using System;
using System.Collections.Generic;

public partial class BankruptcyTest : Node
{
	[Export]
	public NodePath GameManagerPath { get; set; }
	
	// Referencje do elementów UI
	private Label resultsLabel;
	private Button runTestsButton;
	private Button resetButton;
	private VBoxContainer testResultsContainer;
	
	private GameManager gameManager;
	private Dictionary<string, bool> testResults = new Dictionary<string, bool>();
	
	public override void _Ready()
	{
		// Tworzenie interfejsu graficznego
		CreateUI();
		
		// Try to get the GameManager from the exported path
		if (!string.IsNullOrEmpty(GameManagerPath))
		{
			gameManager = GetNode<GameManager>(GameManagerPath);
		}
		
		// Fallback: try to find it in the scene
		if (gameManager == null)
		{
			gameManager = GetNode<GameManager>("/res/GameManager");
		}
	}
	
	private void CreateUI()
	{
		// Główny kontener na cały interfejs
		var mainContainer = new Control();
		mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		AddChild(mainContainer);
		
		// Panel z tłem
		var panel = new PanelContainer();
		panel.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
		panel.Position = new Vector2(0, 50);
		panel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		panel.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
		mainContainer.AddChild(panel);
		
		// Główny kontener pionowy
		var vbox = new VBoxContainer();
		vbox.CustomMinimumSize = new Vector2(400, 0);
		panel.AddChild(vbox);
		
		// Tytuł
		var titleLabel = new Label();
		titleLabel.Text = "System testowania bankructwa";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		vbox.AddChild(titleLabel);
		
		// Separator
		var separator1 = new HSeparator();
		vbox.AddChild(separator1);
		
		// Przyciski
		var buttonContainer = new HBoxContainer();
		buttonContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		vbox.AddChild(buttonContainer);
		
		runTestsButton = new Button();
		runTestsButton.Text = "Uruchom testy";
		runTestsButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		runTestsButton.Pressed += OnRunTestsPressed;
		buttonContainer.AddChild(runTestsButton);
		
		resetButton = new Button();
		resetButton.Text = "Resetuj wyniki";
		resetButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		resetButton.Pressed += OnResetPressed;
		buttonContainer.AddChild(resetButton);
		
		// Separator
		var separator2 = new HSeparator();
		vbox.AddChild(separator2);
		
		// Etykieta statusu
		resultsLabel = new Label();
		resultsLabel.Text = "Naciśnij 'Uruchom testy', aby rozpocząć testowanie";
		resultsLabel.HorizontalAlignment = HorizontalAlignment.Center;
		vbox.AddChild(resultsLabel);
		
		// Kontener na wyniki testów
		testResultsContainer = new VBoxContainer();
		vbox.AddChild(testResultsContainer);
	}
	
	private void OnRunTestsPressed()
	{
		RunTests();
	}
	
	private void OnResetPressed()
	{
		testResults.Clear();
		UpdateResultsDisplay();
		
		// Wyczyść kontener wyników
		foreach (Node child in testResultsContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		resultsLabel.Text = "Wyniki zostały zresetowane";
	}
	
	private void RunTests()
	{
		if (gameManager == null)
		{
			resultsLabel.Text = "Błąd: Nie znaleziono GameManager! Ustaw GameManagerPath w inspektorze.";
			return;
		}
		
		resultsLabel.Text = "Wykonywanie testów...";
		
		// Wyczyść poprzednie wyniki
		testResults.Clear();
		foreach (Node child in testResultsContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		// Test 1: Verify bankruptcy detection
		TestBankruptcyDetection();
		
		// Test 2: Test player revival
		TestPlayerRevival();
		
		// Test 3: Test UI update on bankruptcy
		TestUIUpdateOnBankruptcy();
		
		// Test 4: Test turn skipping for bankrupt player
		TestTurnSkippingForBankruptPlayer();
		
		// Test 5: Test player revive UI update
		TestReviveUIUpdate();
		
		// Test 6: Test game over detection (single player left)
		TestGameOverDetectionSinglePlayerLeft();
		
		// Test 7: Test game over detection (all players bankrupt)
		TestGameOverDetectionAllPlayersBankrupt();
		
		// Test 8: Test FindNextActivePlayer logic
		TestFindNextActivePlayer();
		
		// Test 9: Test bankruptcy after field effect
		TestBankruptcyAfterFieldEffect();
		
		// Test 10: Test multiple bankruptcies
		TestMultipleBankruptcies();
		
		// Aktualizuj wyświetlanie wyników
		UpdateResultsDisplay();
		
		resultsLabel.Text = "Testy zakończone";
	}
	
	private void UpdateResultsDisplay()
	{
		foreach (var test in testResults)
		{
			var resultLabel = new Label();
			resultLabel.Text = $"{test.Key}: {(test.Value ? "ZALICZONY" : "NIEZALICZONY")}";
			resultLabel.Modulate = test.Value ? new Color(0, 1, 0) : new Color(1, 0, 0);
			testResultsContainer.AddChild(resultLabel);
		}
	}
	
	private void TestBankruptcyDetection()
	{
		// Get current player
		int playerIndex = gameManager.GetCurrentPlayerIndex();
		
		// Store original ECTS
		int originalECTS = gameManager.getCurrentPlayer().ECTS;
		
		// Bankrupt the player
		gameManager.AddEctsToPlayer(playerIndex, -originalECTS - 1);
		
		// Check if player is correctly marked as bankrupt
		bool isBankrupt = IsPlayerBankrupt(playerIndex);
		testResults["Test wykrywania bankructwa"] = isBankrupt;
		
		// Restore the original ECTS for next test
		gameManager.AddEctsToPlayer(playerIndex, originalECTS + 1);
	}
	
	private void TestPlayerRevival()
	{
		// Get current player
		int playerIndex = gameManager.GetCurrentPlayerIndex();
		
		// Store original ECTS
		int originalECTS = gameManager.getCurrentPlayer().ECTS;
		
		// Bankrupt the player
		gameManager.AddEctsToPlayer(playerIndex, -originalECTS - 1);
		
		// Now revive the player
		gameManager.AddEctsToPlayer(playerIndex, 100);
		
		// Check if player is correctly revived
		bool isRevived = !IsPlayerBankrupt(playerIndex);
		testResults["Test przywracania gracza"] = isRevived;
		
		// Restore the original ECTS
		gameManager.AddEctsToPlayer(playerIndex, originalECTS - 100 + 1);
	}
	
	private void TestUIUpdateOnBankruptcy()
	{
		// Get current player
		int playerIndex = gameManager.GetCurrentPlayerIndex();
		
		// Store original ECTS and player name
		int originalECTS = gameManager.getCurrentPlayer().ECTS;
		string originalPlayerName = gameManager.getCurrentPlayer().Name;
		
		// Store original label text and color
		string originalLabelText = GetPlayerNameLabelText(playerIndex);
		Color originalLabelColor = GetPlayerNameLabelColor(playerIndex);
		
		// Bankrupt the player
		gameManager.AddEctsToPlayer(playerIndex, -originalECTS - 1);
		
		// Check if UI was updated correctly
		string newLabelText = GetPlayerNameLabelText(playerIndex);
		Color newLabelColor = GetPlayerNameLabelColor(playerIndex);
		
		bool uiUpdated = newLabelText.Contains("BANKRUT") && !newLabelColor.Equals(originalLabelColor);
		testResults["Test aktualizacji UI po bankructwie"] = uiUpdated;
		
		// Restore player state
		gameManager.AddEctsToPlayer(playerIndex, originalECTS + 1);
	}
	
	private void TestTurnSkippingForBankruptPlayer()
	{
		// Get indices of current and next player
		int currentPlayerIndex = gameManager.GetCurrentPlayerIndex();
		int expectedNextPlayerIndex = (currentPlayerIndex + 1) % GetTotalPlayerCount();
		
		// Store original ECTS
		int originalECTS = gameManager.getCurrentPlayer().ECTS;
		
		// Bankrupt the current player
		gameManager.AddEctsToPlayer(currentPlayerIndex, -originalECTS - 1);
		
		// Simulate end turn (can't call directly since it's private)
		// We'll use reflection to call private FindNextActivePlayer method
		CallFindNextActivePlayer();
		
		// Check if current player index changed as expected
		int newPlayerIndex = gameManager.GetCurrentPlayerIndex();
		bool playerSkipped = newPlayerIndex == expectedNextPlayerIndex;
		
		testResults["Test pomijania zbankrutowanego gracza"] = playerSkipped;
		
		// Restore player state
		gameManager.AddEctsToPlayer(currentPlayerIndex, originalECTS + 1);
	}
	
	private void TestReviveUIUpdate()
	{
		// Get current player
		int playerIndex = gameManager.GetCurrentPlayerIndex();
		
		// Store original ECTS and player name
		int originalECTS = gameManager.getCurrentPlayer().ECTS;
		string originalPlayerName = gameManager.getCurrentPlayer().Name;
		
		// Store original label text and color
		string originalLabelText = GetPlayerNameLabelText(playerIndex);
		Color originalLabelColor = GetPlayerNameLabelColor(playerIndex);
		
		// Bankrupt the player
		gameManager.AddEctsToPlayer(playerIndex, -originalECTS - 1);
		
		// Now revive the player
		gameManager.AddEctsToPlayer(playerIndex, originalECTS + 100);
		
		// Check if UI was restored correctly
		string restoredLabelText = GetPlayerNameLabelText(playerIndex);
		Color restoredLabelColor = GetPlayerNameLabelColor(playerIndex);
		
		bool uiRestored = restoredLabelText.Equals(originalPlayerName) && restoredLabelColor.Equals(originalLabelColor);
		testResults["Test aktualizacji UI po przywróceniu gracza"] = uiRestored;
		
		// Restore original state
		gameManager.AddEctsToPlayer(playerIndex, -100);
	}
	
	private void TestGameOverDetectionSinglePlayerLeft()
	{
		// Save all players' original ECTS
		Dictionary<int, int> originalECTS = new Dictionary<int, int>();
		
		// Get total number of players
		int totalPlayers = GetTotalPlayerCount();
		
		// Store current player
		int currentPlayerIndex = gameManager.GetCurrentPlayerIndex();
		
		// Save original button states
		bool rollButtonOriginalState = GetRollButtonDisabledState();
		bool endTurnButtonOriginalState = GetEndTurnButtonDisabledState();
		
		// Make everyone bankrupt except the current player
		for (int i = 0; i < totalPlayers; i++)
		{
			if (i != currentPlayerIndex)
			{
				// Save original ECTS
				originalECTS[i] = GetPlayerECTS(i);
				
				// Bankrupt this player
				gameManager.AddEctsToPlayer(i, -originalECTS[i] - 1);
			}
		}
		
		// Check if game over was detected correctly
		bool buttonsDisabled = GetRollButtonDisabledState() && GetEndTurnButtonDisabledState();
		testResults["Test wykrywania końca gry (jeden gracz pozostał)"] = buttonsDisabled;
		
		// Restore all players' ECTS
		foreach (var pair in originalECTS)
		{
			gameManager.AddEctsToPlayer(pair.Key, pair.Value + 1);
		}
		
		// Restore button states by reflection if needed
		SetButtonDisabledStates(rollButtonOriginalState, endTurnButtonOriginalState);
	}
	
	private void TestGameOverDetectionAllPlayersBankrupt()
	{
		// Save all players' original ECTS
		Dictionary<int, int> originalECTS = new Dictionary<int, int>();
		
		// Get total number of players
		int totalPlayers = GetTotalPlayerCount();
		
		// Save original button states
		bool rollButtonOriginalState = GetRollButtonDisabledState();
		bool endTurnButtonOriginalState = GetEndTurnButtonDisabledState();
		
		// Make everyone bankrupt
		for (int i = 0; i < totalPlayers; i++)
		{
			// Save original ECTS
			originalECTS[i] = GetPlayerECTS(i);
			
			// Bankrupt this player
			gameManager.AddEctsToPlayer(i, -originalECTS[i] - 1);
		}
		
		// Check if game over was detected correctly
		bool buttonsDisabled = GetRollButtonDisabledState() && GetEndTurnButtonDisabledState();
		testResults["Test wykrywania końca gry (wszyscy zbankrutowali)"] = buttonsDisabled;
		
		// Restore all players' ECTS
		foreach (var pair in originalECTS)
		{
			gameManager.AddEctsToPlayer(pair.Key, pair.Value + 1);
		}
		
		// Restore button states by reflection if needed
		SetButtonDisabledStates(rollButtonOriginalState, endTurnButtonOriginalState);
	}
	
	private void TestFindNextActivePlayer()
	{
		// Store current player index
		int originalPlayerIndex = gameManager.GetCurrentPlayerIndex();
		int totalPlayers = GetTotalPlayerCount();
		
		// Store original ECTS for all players
		Dictionary<int, int> originalECTS = new Dictionary<int, int>();
		for (int i = 0; i < totalPlayers; i++)
		{
			originalECTS[i] = GetPlayerECTS(i);
		}
		
		// Make next player (and possibly more) bankrupt
		int nextPlayerIndex = (originalPlayerIndex + 1) % totalPlayers;
		int expectedNextActiveIndex = (nextPlayerIndex + 1) % totalPlayers;
		
		// Bankrupt the next player
		gameManager.AddEctsToPlayer(nextPlayerIndex, -originalECTS[nextPlayerIndex] - 1);
		
		// Call FindNextActivePlayer through reflection
		CallFindNextActivePlayer();
		
		// Check if it skipped the bankrupt player
		int newCurrentPlayerIndex = gameManager.GetCurrentPlayerIndex();
		bool skipWorked = newCurrentPlayerIndex == expectedNextActiveIndex;
		
		testResults["Test FindNextActivePlayer"] = skipWorked;
		
		// Restore player state
		for (int i = 0; i < totalPlayers; i++)
		{
			if (GetPlayerECTS(i) <= 0)
			{
				gameManager.AddEctsToPlayer(i, originalECTS[i] + 1);
			}
		}
		
		// Restore original player index
		SetCurrentPlayerIndex(originalPlayerIndex);
	}
	
	private void TestBankruptcyAfterFieldEffect()
	{
		// Simulate a situation where a player would go bankrupt after landing on a field
		// We'll simulate this by setting ECTS to 1 and then removing 2
		
		// Get current player
		int playerIndex = gameManager.GetCurrentPlayerIndex();
		
		// Store original ECTS
		int originalECTS = gameManager.getCurrentPlayer().ECTS;
		
		// Set ECTS to 1
		int targetECTS = 1;
		gameManager.AddEctsToPlayer(playerIndex, targetECTS - originalECTS);
		
		// Simulate field effect that causes bankruptcy
		gameManager.AddEctsToPlayer(playerIndex, -2);
		
		// Check if player is correctly marked as bankrupt
		bool isBankrupt = IsPlayerBankrupt(playerIndex);
		testResults["Test bankructwa po efekcie pola"] = isBankrupt;
		
		// Restore the original ECTS
		gameManager.AddEctsToPlayer(playerIndex, originalECTS);
	}
	
	private void TestMultipleBankruptcies()
	{
		// Get total number of players
		int totalPlayers = GetTotalPlayerCount();
		if (totalPlayers < 3)
		{
			testResults["Test wielu bankructw"] = false;
			return;
		}
		
		// Store original ECTS for all players
		Dictionary<int, int> originalECTS = new Dictionary<int, int>();
		for (int i = 0; i < totalPlayers; i++)
		{
			originalECTS[i] = GetPlayerECTS(i);
		}
		
		// Store current player index
		int originalCurrentPlayer = gameManager.GetCurrentPlayerIndex();
		
		// Bankrupt multiple players
		int bankruptCount = 0;
		for (int i = 0; i < totalPlayers; i++)
		{
			if (i != originalCurrentPlayer)
			{
				gameManager.AddEctsToPlayer(i, -originalECTS[i] - 1);
				bankruptCount++;
				
				// Only bankrupt two players for the test
				if (bankruptCount >= 2) break;
			}
		}
		
		// Check if bankruptcy status is correct for all players
		bool allCorrect = true;
		for (int i = 0; i < totalPlayers; i++)
		{
			bool shouldBeBankrupt = (i != originalCurrentPlayer) && bankruptCount > 0;
			if (shouldBeBankrupt) bankruptCount--;
			
			if (IsPlayerBankrupt(i) != shouldBeBankrupt)
			{
				allCorrect = false;
				break;
			}
		}
		
		testResults["Test wielu bankructw"] = allCorrect;
		
		// Restore all players' ECTS
		foreach (var pair in originalECTS)
		{
			if (GetPlayerECTS(pair.Key) <= 0)
			{
				gameManager.AddEctsToPlayer(pair.Key, pair.Value + 1);
			}
		}
	}
	
	// Helper method to check bankruptcy status
	private bool IsPlayerBankrupt(int playerIndex)
	{
		// We need to access private field via reflection
		var playerBankruptcyStatusField = gameManager.GetType().GetField(
			"playerBankruptcyStatus", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (playerBankruptcyStatusField != null)
		{
			var bankruptcyList = playerBankruptcyStatusField.GetValue(gameManager) as System.Collections.Generic.List<bool>;
			if (bankruptcyList != null && playerIndex < bankruptcyList.Count)
			{
				return bankruptcyList[playerIndex];
			}
		}
		
		return false;
	}
	
	// Helper methods using reflection to access GameManager private fields and methods
	
	private string GetPlayerNameLabelText(int playerIndex)
	{
		var playerNameLabelsField = gameManager.GetType().GetField(
			"playerNameLabels", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (playerNameLabelsField != null)
		{
			var labelsList = playerNameLabelsField.GetValue(gameManager) as System.Collections.Generic.List<Label>;
			if (labelsList != null && playerIndex < labelsList.Count)
			{
				return labelsList[playerIndex].Text;
			}
		}
		
		return string.Empty;
	}
	
	private Color GetPlayerNameLabelColor(int playerIndex)
	{
		var playerNameLabelsField = gameManager.GetType().GetField(
			"playerNameLabels", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (playerNameLabelsField != null)
		{
			var labelsList = playerNameLabelsField.GetValue(gameManager) as System.Collections.Generic.List<Label>;
			if (labelsList != null && playerIndex < labelsList.Count)
			{
				// Try to get the overridden color, if not present use default
				if (labelsList[playerIndex].HasThemeColorOverride("font_color"))
				{
					return labelsList[playerIndex].GetThemeColor("font_color", "Label");
				}
				else
				{
					return Colors.White; // Default color
				}
			}
		}
		
		return Colors.White;
	}
	
	private int GetTotalPlayerCount()
	{
		var playersField = gameManager.GetType().GetField(
			"players", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (playersField != null)
		{
			var playersList = playersField.GetValue(gameManager) as System.Collections.Generic.List<Figurehead>;
			if (playersList != null)
			{
				return playersList.Count;
			}
		}
		
		return 0;
	}
	
	private int GetPlayerECTS(int playerIndex)
	{
		var playersField = gameManager.GetType().GetField(
			"players", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (playersField != null)
		{
			var playersList = playersField.GetValue(gameManager) as System.Collections.Generic.List<Figurehead>;
			if (playersList != null && playerIndex < playersList.Count)
			{
				return playersList[playerIndex].ECTS;
			}
		}
		
		return 0;
	}
	
	private bool GetRollButtonDisabledState()
	{
		var rollButtonField = gameManager.GetType().GetField(
			"rollButton", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (rollButtonField != null)
		{
			var button = rollButtonField.GetValue(gameManager) as Button;
			if (button != null)
			{
				return button.Disabled;
			}
		}
		
		return false;
	}
	
	private bool GetEndTurnButtonDisabledState()
	{
		var endTurnButtonField = gameManager.GetType().GetField(
			"endTurnButton", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (endTurnButtonField != null)
		{
			var button = endTurnButtonField.GetValue(gameManager) as Button;
			if (button != null)
			{
				return button.Disabled;
			}
		}
		
		return false;
	}
	
	private void SetButtonDisabledStates(bool rollButtonDisabled, bool endTurnButtonDisabled)
	{
		var rollButtonField = gameManager.GetType().GetField(
			"rollButton", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		var endTurnButtonField = gameManager.GetType().GetField(
			"endTurnButton", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (rollButtonField != null)
		{
			var button = rollButtonField.GetValue(gameManager) as Button;
			if (button != null)
			{
				button.Disabled = rollButtonDisabled;
			}
		}
		
		if (endTurnButtonField != null)
		{
			var button = endTurnButtonField.GetValue(gameManager) as Button;
			if (button != null)
			{
				button.Disabled = endTurnButtonDisabled;
			}
		}
	}
	
	private void CallFindNextActivePlayer()
	{
		var methodInfo = gameManager.GetType().GetMethod(
			"FindNextActivePlayer", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (methodInfo != null)
		{
			methodInfo.Invoke(gameManager, null);
		}
	}
	
	private void SetCurrentPlayerIndex(int index)
	{
		var currentPlayerIndexField = gameManager.GetType().GetField(
			"currentPlayerIndex", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
		);
		
		if (currentPlayerIndexField != null)
		{
			currentPlayerIndexField.SetValue(gameManager, index);
		}
	}
}
