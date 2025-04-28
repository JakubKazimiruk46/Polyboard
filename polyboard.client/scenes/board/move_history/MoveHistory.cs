using Godot;
using System;
using System.Collections.Generic;

public partial class MoveHistory : Node
{
	[Export] public NodePath historyContainerPath;
	[Export] public NodePath moveHistoryLabelPath;
	[Export] public int maxHistoryEntries = 5;
	
	private VBoxContainer historyContainer;
	private Label moveHistoryLabel;
	private List<string> moveHistory = new List<string>();
	private GameManager gameManager;
	
	public override void _Ready()
	{
		historyContainer = GetNodeOrNull<VBoxContainer>(historyContainerPath);
		moveHistoryLabel = GetNodeOrNull<Label>(moveHistoryLabelPath);
		gameManager = GetNodeOrNull<GameManager>("/root/Level/GameManager");
		
		if (gameManager == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono GameManager.");
			return;
		}
		
		if (historyContainer == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono kontenera historii ruchów.");
			return;
		}
		
		if (moveHistoryLabel == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono etykiety historii ruchów.");
			return;
		}
		
		// Utwórz stylizowany panel dla historii
		CreateHistoryPanel();
		
		// Połącz sygnały
		gameManager.Connect("DicesStoppedRolling", new Callable(this, nameof(OnDicesStoppedRolling)));
		
		// Ukryj etykietę historii na początku, jeśli jest pusta
		UpdateHistoryDisplay();
	}
	
	private void CreateHistoryPanel()
	{
		// Utwórz styl dla panelu
		var panelStyle = new StyleBoxFlat();
		panelStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
		panelStyle.BorderWidthBottom = 2;
		panelStyle.BorderWidthLeft = 2;
		panelStyle.BorderWidthRight = 2;
		panelStyle.BorderWidthTop = 2;
		panelStyle.BorderColor = new Color("#62ff45");
		panelStyle.CornerRadiusBottomLeft = 8;
		panelStyle.CornerRadiusBottomRight = 8;
		panelStyle.CornerRadiusTopLeft = 8;
		panelStyle.CornerRadiusTopRight = 8;
		panelStyle.ContentMarginLeft = 10;
		panelStyle.ContentMarginRight = 10;
		panelStyle.ContentMarginTop = 8;
		panelStyle.ContentMarginBottom = 8;
		
		// Zastosuj styl do kontenera (jeśli to Panel)
		var containerParent = historyContainer.GetParent();
		if (containerParent is Panel panel)
		{
			panel.AddThemeStyleboxOverride("panel", panelStyle);
		}
		
		// Ustaw styl czcionki dla etykiety
		moveHistoryLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
		moveHistoryLabel.AddThemeFontSizeOverride("font_size", 18);
	}
	
	// Metoda wywoływana, gdy kostki przestają się toczyć
	public void OnDicesStoppedRolling()
	{
		if (gameManager == null) return;
		
		// Pobierz dane o ruchu
		Figurehead currentPlayer = gameManager.getCurrentPlayer();
		int diceTotal = gameManager.GetLastDiceTotal();
		int currentPosition = currentPlayer.GetCurrentPositionIndex();
		Board board = GetNodeOrNull<Board>("/root/Level/Board");
		
		if (board == null) return;
		
		Field currentField = board.GetFieldById(currentPosition);
		string fieldName = currentField != null ? currentField.Name : $"Pole {currentPosition}";
		
		// Utwórz wpis historii
		string historyEntry = $"Tura {gameManager.GetCurrentRound()}: {currentPlayer.Name} wyrzucił {diceTotal} i wylądował na {fieldName}";
		
		// Dodaj wpis do historii
		AddHistoryEntry(historyEntry);
	}
	
	// Metoda dodająca wpis do historii ruchów
	public void AddHistoryEntry(string entry)
	{
		// Dodaj nowy wpis na początek listy
		moveHistory.Insert(0, entry);
		
		// Ogranicz liczbę wpisów
		if (moveHistory.Count > maxHistoryEntries)
		{
			moveHistory.RemoveAt(moveHistory.Count - 1);
		}
		
		// Aktualizuj wyświetlanie
		UpdateHistoryDisplay();
	}
	
	// Utwórz nowy wpis o wykonanej akcji (np. zakup pola)
	public void AddActionEntry(string playerName, string action)
	{
		string entry = $"Tura {gameManager.GetCurrentRound()}: {playerName} {action}";
		AddHistoryEntry(entry);
	}
	
	// Metoda aktualizująca wyświetlanie historii
	private void UpdateHistoryDisplay()
	{
		// Usuń poprzednie etykiety
		foreach (var child in historyContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		// Pokaż kontener tylko jeśli są wpisy
		if (moveHistory.Count == 0)
		{
			return;
		}
		
		// Dodaj nowe etykiety dla każdego wpisu
		foreach (var entry in moveHistory)
		{
			var entryLabel = new Label();
			entryLabel.Text = entry;
			entryLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
			entryLabel.AddThemeFontSizeOverride("font_size", 16);
			historyContainer.AddChild(entryLabel);
		}
	}
}
