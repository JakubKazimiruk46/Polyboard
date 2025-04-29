using Godot;
using System;
using System.Collections.Generic;

public partial class MoveHistory : Node
{
	[Export] public NodePath historyContainerPath;
	[Export] public NodePath moveHistoryLabelPath;
	[Export] public int maxHistoryEntries = 10; // Zwiększam limit wpisów dla lepszej widoczności historii
	
	private VBoxContainer historyContainer;
	private Label moveHistoryLabel;
	private List<string> moveHistory = new List<string>();
	private GameManager gameManager;
	private Control moveHistoryPanel;
	private bool isHistoryVisible = false;
	private ScrollContainer scrollContainer;
	
	public override void _Ready()
	{
		// Znajdź referencje do węzłów - zaktualizowane ścieżki dla ScrollContainer
		historyContainer = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer/ScrollContainer/HistoryContainer");
		moveHistoryLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/MoveHistoryLabel");
		moveHistoryPanel = GetNode<Control>("Panel");
		scrollContainer = GetNode<ScrollContainer>("Panel/MarginContainer/VBoxContainer/ScrollContainer");
		gameManager = GetNode<GameManager>("/root/Level/GameManager");
		
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
		
		if (scrollContainer == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono kontenera przewijania.");
			return;
		}
		
		// Utwórz stylizowany panel dla historii
		CreateHistoryPanel();
		
		// Połącz sygnały
		gameManager.Connect("DicesStoppedRolling", new Callable(this, nameof(OnDicesStoppedRolling)));
		
		// Dodaj pierwszy wpis do historii - inicjalizacja gry
		AddHistoryEntry("Gra rozpoczęta! Rzuć kostkami, aby wykonać ruch.");
		
		// Ustaw panel jako widoczny na początku
		moveHistoryPanel.Visible = isHistoryVisible;
	}
	
	public override void _Input(InputEvent @event)
	{
		// Obsługa klawisza Tab do przełączania widoczności historii
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.Tab)
		{
			ToggleHistoryVisibility();
			GetViewport().SetInputAsHandled(); // Zapobiega propagacji zdarzenia
		}
	}
	
	// Metoda przełączania widoczności historii
	public void ToggleHistoryVisibility()
	{
		isHistoryVisible = !isHistoryVisible;
		moveHistoryPanel.Visible = isHistoryVisible;
		
		// Wyświetl powiadomienie o zmianie stanu historii
		var notificationService = GetNode<NotificationService>("/root/NotificationService");
		if (notificationService != null)
		{
			notificationService.ShowNotification(
				isHistoryVisible ? "Historia ruchów widoczna" : "Historia ruchów ukryta",
				NotificationService.NotificationType.Normal,
				1.5f
			);
		}
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
		
		// Zastosuj styl do panelu
		var panel = GetNode<Panel>("Panel");
		if (panel != null)
		{
			panel.AddThemeStyleboxOverride("panel", panelStyle);
		}
		
		// Ustaw styl czcionki dla etykiety
		moveHistoryLabel.AddThemeColorOverride("font_color", new Color("#62ff45")); // Kolor zielony
		moveHistoryLabel.AddThemeFontSizeOverride("font_size", 18);
		
		// Skonfiguruj ScrollContainer
		if (scrollContainer != null)
		{
			// Wyłącz scrollowanie poziome, włącz pionowe
			scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
			scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.Auto;
			
			// Ustaw właściwości rozmiaru
			scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			
			// Ustaw styl scrollbara
			var scrollStyle = new StyleBoxFlat();
			scrollStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
			scrollStyle.CornerRadiusBottomRight = 4;
			scrollStyle.CornerRadiusTopRight = 4;
			scrollStyle.ContentMarginLeft = 2;
			scrollStyle.ContentMarginRight = 2;
			
			// Zastosuj styl do scrollbara
			scrollContainer.GetVScrollBar().AddThemeStyleboxOverride("grabber", scrollStyle);
		}
		
		// Dodaj informację o klawiszu Tab
		var tabInfoLabel = new Label();
		tabInfoLabel.Text = "(Naciśnij Tab, aby ukryć/pokazać)";
		tabInfoLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
		tabInfoLabel.AddThemeFontSizeOverride("font_size", 12);
		tabInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
		
		// Dodaj etykietę do kontenera
		var vboxContainer = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer");
		if (vboxContainer != null)
		{
			// Sprawdź, czy etykieta już istnieje
			if (vboxContainer.GetNodeOrNull("TabInfoLabel") == null)
			{
				vboxContainer.AddChild(tabInfoLabel);
				vboxContainer.MoveChild(tabInfoLabel, 1); // Przenieś na pozycję po głównej etykiecie
			}
		}
	}
	
	// Metoda wywoływana, gdy kostki przestają się toczyć
	public void OnDicesStoppedRolling()
	{
		if (gameManager == null) return;
		
		GD.Print("OnDicesStoppedRolling wywołane");
		
		// Pobierz dane o ruchu
		Figurehead currentPlayer = gameManager.getCurrentPlayer();
		int diceTotal = gameManager.GetLastDiceTotal();
		int currentPosition = currentPlayer.CurrentPositionIndex;
		Board board = GetNode<Board>("/root/Level/Board");
		
		if (board == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono planszy.");
			return;
		}
		
		Field currentField = board.GetFieldById(currentPosition);
		string fieldName = currentField != null ? currentField.Name : $"Pole {currentPosition}";
		
		// Utwórz wpis historii
		string historyEntry = $"Tura {gameManager.GetCurrentRound()}: {currentPlayer.Name} wyrzucił {diceTotal} i wylądował na {fieldName}";
		
		// Dodaj wpis do historii
		AddHistoryEntry(historyEntry);
		GD.Print("Dodano wpis do historii: " + historyEntry);
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
		GD.Print("Dodano wpis akcji do historii: " + entry);
	}
	
	// Metoda aktualizująca wyświetlanie historii
	private void UpdateHistoryDisplay()
	{
		// Usuń poprzednie etykiety
		foreach (var child in historyContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		// Dodaj nowe etykiety dla każdego wpisu
		foreach (var entry in moveHistory)
		{
			var entryLabel = new Label();
			entryLabel.Text = entry;
			entryLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
			entryLabel.AddThemeFontSizeOverride("font_size", 16);
			entryLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart; // Włącz zawijanie tekstu
			entryLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill; // Pozwól etykiecie rozszerzać się w poziomie
			
			historyContainer.AddChild(entryLabel);
		}
		
		// Scroll do najnowszego wpisu (góra)
		if (scrollContainer != null && scrollContainer.GetVScrollBar() != null)
		{
			// Odłóż wykonanie na następną klatkę, aby dać czas na aktualizację layoutu
			CallDeferred(nameof(ScrollToTop));
		}
	}
	
	private void ScrollToTop()
	{
		// Przewiń do górnej krawędzi
		if (scrollContainer != null && scrollContainer.GetVScrollBar() != null)
		{
			scrollContainer.GetVScrollBar().Value = 0;
		}
	}
}
