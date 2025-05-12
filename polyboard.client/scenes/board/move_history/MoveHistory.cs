using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MoveHistory : Node
{
	[Export] public NodePath historyContainerPath;
	[Export] public NodePath moveHistoryLabelPath;
	[Export] public int maxHistoryEntries = 12; // widocznych na panelu bocznym

	private VBoxContainer historyContainer;
	private Label moveHistoryLabel;
	private Control moveHistoryPanel;
	private ScrollContainer scrollContainer;
	private GameManager gameManager;

	// Nowe wskaźniki
	private Label roundIndicatorLabel;
	private Label timerIndicatorLabel;
	private Label playerIndicatorLabel;

	// Pełna historia wszystkich ruchów
	private List<HistoryEntry> fullHistory = new List<HistoryEntry>();

	// Filtrowanie
	private string filterPlayer = "";
	private ActionType? filterType = null;

	private bool isHistoryVisible = true;

	public override void _Ready()
	{
		historyContainer = GetNode("Panel/MarginContainer/VBoxContainer/ScrollContainer/HistoryContainer") as VBoxContainer;
		moveHistoryLabel = GetNode("Panel/MarginContainer/VBoxContainer/MoveHistoryLabel") as Label;
		moveHistoryPanel = GetNode("Panel") as Control;
		scrollContainer = GetNode("Panel/MarginContainer/VBoxContainer/ScrollContainer") as ScrollContainer;
		gameManager = GetNode("/root/Level/GameManager") as GameManager;

		if (historyContainer == null || moveHistoryLabel == null || moveHistoryPanel == null || scrollContainer == null || gameManager == null)
		{
			GD.PrintErr("MoveHistory: Nie znaleziono wymaganych komponentów UI lub GameManagera.");
			return;
		}

		CreateHistoryPanel();

		// Połącz sygnały
		gameManager.Connect("DicesStoppedRolling", new Callable(this, nameof(OnDicesStoppedRolling)));

		// Wstępny wpis
		AddHistoryEntry(new HistoryEntry
		{
			TurnNumber = 1,
			PlayerName = "",
			Type = ActionType.Other,
			Description = "Gra rozpoczęta! Rzuć kostkami, aby wykonać ruch.",
			Timestamp = DateTime.Now
		});

		isHistoryVisible = true;
		moveHistoryPanel.Visible = isHistoryVisible;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.Tab)
		{
			ToggleHistoryVisibility();
			GetViewport().SetInputAsHandled();
		}
	}

	public override void _Process(double delta)
	{
		// Aktualizuj wskaźniki co klatkę
		UpdateIndicators();
	}

	public void ToggleHistoryVisibility()
	{
		isHistoryVisible = !isHistoryVisible;
		moveHistoryPanel.Visible = isHistoryVisible;
		var notificationService = GetNode("/root/NotificationService");
		if (notificationService != null)
		{
			notificationService.Call("ShowNotification",
				isHistoryVisible ? "Historia ruchów widoczna" : "Historia ruchów ukryta",
				0, 1.5f);
		}
	}

	private void CreateHistoryPanel()
	{
		// Styl panelu
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
		var panel = GetNode("Panel") as Panel;
		panel?.AddThemeStyleboxOverride("panel", panelStyle);

		moveHistoryLabel.AddThemeColorOverride("font_color", new Color("#62ff45"));
		moveHistoryLabel.AddThemeFontSizeOverride("font_size", 18);

		if (scrollContainer != null)
		{
			scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
			scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.Auto;
			scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			var scrollStyle = new StyleBoxFlat();
			scrollStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
			scrollStyle.CornerRadiusBottomRight = 4;
			scrollStyle.CornerRadiusTopRight = 4;
			scrollStyle.ContentMarginLeft = 2;
			scrollStyle.ContentMarginRight = 2;
			scrollContainer.GetVScrollBar().AddThemeStyleboxOverride("grabber", scrollStyle);
		}

		// Info o Tab
		var tabInfoLabel = new Label();
		tabInfoLabel.Name = "TabInfoLabel";
		tabInfoLabel.Text = "(Tab – ukryj/pokaż)";
		tabInfoLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
		tabInfoLabel.AddThemeFontSizeOverride("font_size", 12);
		tabInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;

		var vboxContainer = GetNode("Panel/MarginContainer/VBoxContainer") as VBoxContainer;
		if (vboxContainer != null && vboxContainer.GetNodeOrNull("TabInfoLabel") == null)
		{
			vboxContainer.AddChild(tabInfoLabel);
			vboxContainer.MoveChild(tabInfoLabel, 1);
		}

		// Dodaj wskaźniki na górze panelu
		roundIndicatorLabel = new Label();
		roundIndicatorLabel.Name = "RoundIndicatorLabel";
		roundIndicatorLabel.Text = "Runda: ?";
		roundIndicatorLabel.AddThemeColorOverride("font_color", new Color("#ffd700"));
		roundIndicatorLabel.AddThemeFontSizeOverride("font_size", 14);

		timerIndicatorLabel = new Label();
		timerIndicatorLabel.Name = "TimerIndicatorLabel";
		timerIndicatorLabel.Text = "Czas tury: ?";
		timerIndicatorLabel.AddThemeColorOverride("font_color", new Color("#45b6ff"));
		timerIndicatorLabel.AddThemeFontSizeOverride("font_size", 14);

		playerIndicatorLabel = new Label();
		playerIndicatorLabel.Name = "PlayerIndicatorLabel";
		playerIndicatorLabel.Text = "Gracz: ?";
		playerIndicatorLabel.AddThemeColorOverride("font_color", new Color("#a66cff"));
		playerIndicatorLabel.AddThemeFontSizeOverride("font_size", 14);

		// Dodaj wskaźniki na początek VBoxContainer
		if (vboxContainer != null)
		{
			vboxContainer.AddChild(roundIndicatorLabel);
			vboxContainer.MoveChild(roundIndicatorLabel, 0);
			vboxContainer.AddChild(timerIndicatorLabel);
			vboxContainer.MoveChild(timerIndicatorLabel, 1);
			vboxContainer.AddChild(playerIndicatorLabel);
			vboxContainer.MoveChild(playerIndicatorLabel, 2);
		}
	}

	private void UpdateIndicators()
	{
		if (gameManager == null) return;
		// Runda
		roundIndicatorLabel.Text = $"Runda: {gameManager.GetCurrentRound()}";
		// Czas tury
		var turnTimerLabel = gameManager.GetNodeOrNull<Label>("/root/Level/UI/TimeAndRounds/HBoxContainer/TurnTimerLabel");
		if (turnTimerLabel != null)
			timerIndicatorLabel.Text = turnTimerLabel.Text;
		else
			timerIndicatorLabel.Text = "Czas tury: ?";
		// Aktywny gracz
		playerIndicatorLabel.Text = $"Gracz: {gameManager.getCurrentPlayer().Name}";
	}

	// --- Nowy model wpisu historii ---
	public enum ActionType
	{
		Move,
		Buy,
		BuildHouse,
		BuildHotel,
		Sell,
		Trade,
		Card,
		Bankruptcy,
		EndTurn,
		Other
	}

	public class HistoryEntry
	{
		public int TurnNumber;
		public string PlayerName;
		public ActionType Type;
		public string Description;
		public DateTime Timestamp;
		public Dictionary<string, object> Details = new Dictionary<string, object>();
	}

	// --- Dodawanie wpisów ---

	public void AddHistoryEntry(HistoryEntry entry)
	{
		fullHistory.Insert(0, entry);
		if (fullHistory.Count > 1000) // limit pełnej historii
			fullHistory.RemoveAt(fullHistory.Count - 1);
		UpdateHistoryDisplay();
	}

	// Skrót – uproszczony wpis
	public void AddSimpleEntry(string text)
	{
		AddHistoryEntry(new HistoryEntry
		{
			TurnNumber = gameManager?.GetCurrentRound() ?? 1,
			PlayerName = "",
			Type = ActionType.Other,
			Description = text,
			Timestamp = DateTime.Now
		});
	}

	// Dodawanie akcji z typem
	public void AddActionEntry(string playerName, string action, ActionType type = ActionType.Other, Dictionary<string, object> details = null)
	{
		AddHistoryEntry(new HistoryEntry
		{
			TurnNumber = gameManager?.GetCurrentRound() ?? 1,
			PlayerName = playerName,
			Type = type,
			Description = action,
			Timestamp = DateTime.Now,
			Details = details ?? new Dictionary<string, object>()
		});
	}

	// --- Obsługa zdarzeń gry ---

	public void OnDicesStoppedRolling()
	{
		if (gameManager == null) return;
		Figurehead currentPlayer = gameManager.getCurrentPlayer();
		int diceTotal = gameManager.GetLastDiceTotal();
		int currentPosition = currentPlayer.CurrentPositionIndex;
		Board board = GetNode("/root/Level/Board") as Board;
		Field currentField = board?.GetFieldById(currentPosition);
		string fieldName = currentField != null ? currentField.Name : $"Pole {currentPosition}";
		AddHistoryEntry(new HistoryEntry
		{
			TurnNumber = gameManager.GetCurrentRound(),
			PlayerName = currentPlayer.Name,
			Type = ActionType.Move,
			Description = $"wyrzucił {diceTotal} i wylądował na {fieldName}",
			Timestamp = DateTime.Now
		});
	}

	// --- Filtrowanie ---

	public void SetPlayerFilter(string playerName)
	{
		filterPlayer = playerName;
		UpdateHistoryDisplay();
	}

	public void SetTypeFilter(ActionType? type)
	{
		filterType = type;
		UpdateHistoryDisplay();
	}

	public void ClearFilters()
	{
		filterPlayer = "";
		filterType = null;
		UpdateHistoryDisplay();
	}

	// --- Wyświetlanie historii ---

	private void UpdateHistoryDisplay()
	{
		foreach (var child in historyContainer.GetChildren())
			child.QueueFree();

		// Filtrowanie
		IEnumerable<HistoryEntry> filtered = fullHistory;
		if (!string.IsNullOrEmpty(filterPlayer))
			filtered = filtered.Where(e => e.PlayerName == filterPlayer);
		if (filterType.HasValue)
			filtered = filtered.Where(e => e.Type == filterType.Value);

		// Ogranicz do maxHistoryEntries
		var entries = filtered.Take(maxHistoryEntries);

		foreach (var entry in entries)
		{
			var entryLabel = new Label();
			entryLabel.Text = FormatEntry(entry);
			entryLabel.AddThemeColorOverride("font_color", GetColorForType(entry.Type));
			entryLabel.AddThemeFontSizeOverride("font_size", 16);
			entryLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
			entryLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			// Ikona (opcjonalnie)
			var hbox = new HBoxContainer();
			var icon = new TextureRect();
			icon.Texture = GetIconForType(entry.Type);
			icon.CustomMinimumSize = new Vector2(18, 18);
			hbox.AddChild(icon);
			hbox.AddChild(entryLabel);
			historyContainer.AddChild(hbox);

			// Tooltip ze szczegółami
			if (entry.Details.Count > 0)
				entryLabel.TooltipText = string.Join("\n", entry.Details.Select(kv => $"{kv.Key}: {kv.Value}"));
		}

		CallDeferred(nameof(ScrollToTop));
	}

	private string FormatEntry(HistoryEntry entry)
	{
		string prefix = $"Tura {entry.TurnNumber}: ";
		string player = !string.IsNullOrEmpty(entry.PlayerName) ? $"{entry.PlayerName} " : "";
		return $"{prefix}{player}{entry.Description}";
	}

	private Color GetColorForType(ActionType type)
	{
		switch (type)
		{
			case ActionType.Move: return new Color("#62ff45");
			case ActionType.Buy: return new Color("#45b6ff");
			case ActionType.BuildHouse: return new Color("#ffd700");
			case ActionType.BuildHotel: return new Color("#ff9900");
			case ActionType.Sell: return new Color("#bbbbbb");
			case ActionType.Trade: return new Color("#a66cff");
			case ActionType.Card: return new Color("#ff45a6");
			case ActionType.Bankruptcy: return new Color("#ff4545");
			case ActionType.EndTurn: return new Color("#cccccc");
			default: return new Color(1, 1, 1);
		}
	}

	private Texture2D GetIconForType(ActionType type)
	{
		// Podmień ścieżki na własne zasoby!
		string path = type switch
		{
			ActionType.Move => "res://ui/icons/move.png",
			ActionType.Buy => "res://ui/icons/buy.png",
			ActionType.BuildHouse => "res://ui/icons/house.png",
			ActionType.BuildHotel => "res://ui/icons/hotel.png",
			ActionType.Sell => "res://ui/icons/sell.png",
			ActionType.Trade => "res://ui/icons/trade.png",
			ActionType.Card => "res://ui/icons/card.png",
			ActionType.Bankruptcy => "res://ui/icons/bankrupt.png",
			ActionType.EndTurn => "res://ui/icons/endturn.png",
			_ => "res://ui/icons/other.png"
		};
		return ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;
	}

	private void ScrollToTop()
	{
		if (scrollContainer != null && scrollContainer.GetVScrollBar() != null)
			scrollContainer.GetVScrollBar().Value = 0;
	}

	// --- Czyszczenie historii ---

	public void ClearHistory()
	{
		fullHistory.Clear();
		UpdateHistoryDisplay();
		AddSimpleEntry("Historia została wyczyszczona.");
		var notificationService = GetNode("/root/NotificationService");
		notificationService?.Call("ShowNotification", "Historia ruchów została wyczyszczona", 0, 1.5f);
	}

	// Usunięto funkcję eksportu historii!

	public List<HistoryEntry> GetFullHistory() => new List<HistoryEntry>(fullHistory);
}
