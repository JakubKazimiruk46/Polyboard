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
}
