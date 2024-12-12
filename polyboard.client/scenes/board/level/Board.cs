using Godot;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public partial class Board : StaticBody3D
{
	private Vector3 targetPosition; 
	private List<Field> fields = new List<Field>();
	private List<Player> players = new List<Player>();
	private int currentPlayerIndex = 0;
	private Sprite2D textureDisplay;
	private Sprite2D randomCard;
	private Random random = new Random();

	[Export]
	public NodePath RollDiceButtonPath; // Ścieżka do przycisku "Rzuć Kostką"

	// Public properties
	public List<Player> Players => players;
	public int CurrentPlayerIndex => currentPlayerIndex;

	public override void _Ready()
	{
		randomCard = GetNodeOrNull<Sprite2D>("../CanvasLayer/TextureRect2/RandomCard");
		textureDisplay = GetNodeOrNull<Sprite2D>("../CanvasLayer/TextureRect2/FieldCard");
		if (textureDisplay == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono Sprite2D do wyświetlania tekstur.");
		}
		targetPosition = GlobalPosition;

		foreach (Node child in GetChildren())
		{
			if (child is Field field)
			{
				fields.Add(field);
			}
		}

		// Inicjalizacja graczy
		InitializePlayers();

		if (players.Count > 0)
		{
			MovePawn(players[currentPlayerIndex].Pawn, players[currentPlayerIndex].CurrentFieldId, 0);
			GD.Print($"Rozpoczęto grę z graczem: {players[currentPlayerIndex].Name}");
		}

		// Połącz przycisk z metodą
		Button rollDiceButton = GetNodeOrNull<Button>(RollDiceButtonPath);
		if (rollDiceButton != null)
		{
			rollDiceButton.Pressed += RollDice;
		}
		else
		{
			GD.PrintErr("Nie znaleziono przycisku RollDiceButton.");
		}

		// Rozpocznij grę
		StartGame();
	}

	private void InitializePlayers()
	{
		// Przykładowa inicjalizacja dwóch graczy
		Figurehead pawn1 = GetNodeOrNull<Figurehead>("../Figurehead1");
		Figurehead pawn2 = GetNodeOrNull<Figurehead>("../Figurehead2");

		if (pawn1 == null || pawn2 == null)
		{
			GD.PrintErr("Nie znaleziono pionków graczy.");
			return;
		}

		players.Add(new Player("Gracz 1", pawn1));
		players.Add(new Player("Gracz 2", pawn2));
	}

	public List<Field> GetFields()
	{
		return fields;
	}

	public void ShowFieldTexture(int fieldId)
	{
		randomCard.Visible = false;
		textureDisplay.Visible = false;
		if (fieldId == 2 || fieldId == 17 || fieldId == 33)
		{
			ShowRandomCard("community");
			return;
		}
		else if (fieldId == 7 || fieldId == 22 || fieldId == 36)
		{
			ShowRandomCard("chance");
			return;
		}
		string textureName = $"Field{fieldId}";

		Texture2D fieldTexture = ResourceLoader.Load<Texture2D>($"res://scenes/board/level/textures/{textureName}.png");
		if (fieldTexture != null)
		{
			textureDisplay.Texture = fieldTexture;

			Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

			float scaleFactorX = viewportSize.X / 3000f;
			float scaleFactorY = viewportSize.Y / 1250f;
			float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY);
			Vector2 scale = new Vector2(scaleFactor, scaleFactor);

			textureDisplay.Scale = scale;
			textureDisplay.Visible = true;
		}
		else
		{
			GD.PrintErr($"Błąd: Nie udało się załadować tekstury {textureName}.");
		}
	}

	public async Task ShowRandomCard(string cardType)
	{
		randomCard.Visible = false;
		textureDisplay.Visible = false;
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float scaleFactorX = viewportSize.X / 2500f;
		float scaleFactorY = viewportSize.Y / 1080f;
		float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY);
		Vector2 scale = new Vector2(scaleFactor, scaleFactor);

		randomCard.Scale = scale;
		Texture2D cardTexture = null;
		int number = random.Next(2, 17);

		if (cardType == "community")
		{
			cardTexture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/community_chest/community_1.png");
		}
		else
		{
			cardTexture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/chances/chance_1.png");
		}

		if (cardTexture != null)
		{
			randomCard.Texture = cardTexture;
			randomCard.Visible = true;
			Tween tween = CreateTween();
			float duration = 1.2f;

			tween.TweenProperty(randomCard, "rotation_degrees", 360 * 2, duration)
				.SetTrans(Tween.TransitionType.Circ)
				.SetEase(Tween.EaseType.InOut);

			await ToSignal(tween, "finished");

			randomCard.RotationDegrees = 0;

			if (cardType == "community")
			{
				string textureName = $"community_{number}";
				cardTexture = ResourceLoader.Load<Texture2D>($"res://scenes/board/level/textures/community_chest/{textureName}.png");
			}
			else
			{
				string textureName = $"chance_{number}";
				cardTexture = ResourceLoader.Load<Texture2D>($"res://scenes/board/level/textures/chances/{textureName}.png");
			}
			randomCard.Texture = cardTexture;
			randomCard.Visible = true;
		}
	}

	public async void RollDice()
	{
		// Wywołaj metodę do rzutu kostką na wszystkich kostkach
		// Zakładamy, że masz dwa węzły kostek pod Board
		Node dieNode1 = GetNodeOrNull("Die1");
		Node dieNode2 = GetNodeOrNull("Die2");

		if (dieNode1 is RigidBody3D die1 && dieNode2 is RigidBody3D die2)
		{
			die1.Call("_roll");
			die2.Call("_roll");
		}
		else
		{
			GD.PrintErr("Nie znaleziono kostek do rzutu.");
		}
	}

	public async void NextTurn()
	{
		currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
		GD.Print($"Teraz jest tura: {players[currentPlayerIndex].Name}");
		
		// Włącz przycisk "Rzuć Kostką" dla nowego gracza
		EnableRollDiceButton();
		
		// Opcjonalnie: Wyświetl powiadomienie o początku tury
		ShowNotification($"Teraz jest tura: {players[currentPlayerIndex].Name}");
	}

	public async void StartGame()
	{
		if (players.Count > 0)
		{
			GD.Print($"Gra rozpoczęta. Pierwszy gracz: {players[currentPlayerIndex].Name}");
			// Możesz wywołać interfejs dla pierwszego gracza tutaj
		}
		else
		{
			GD.PrintErr("Brak graczy w grze.");
		}
	}

	private async Task HandleFieldAction(int fieldId)
	{
		if (fieldId == 2 || fieldId == 17 || fieldId == 33)
		{
			await ShowRandomCard("community");
			// Dodaj logikę dla karty "Community Chest"
			GD.Print("Wykonaj akcję 'Community Chest'");
		}
		else if (fieldId == 7 || fieldId == 22 || fieldId == 36)
		{
			await ShowRandomCard("chance");
			// Dodaj logikę dla karty "Chance"
			GD.Print("Wykonaj akcję 'Chance'");
		}
		else
		{
			// Możesz dodać inne akcje dla różnych pól
			GD.Print($"Pole {fieldId} nie wymaga specjalnej akcji.");
		}
	}

	public async Task MovePlayer(Player player, int steps)
	{
		int newFieldId = (player.CurrentFieldId + steps) % fields.Count;
		player.CurrentFieldId = newFieldId;
		GD.Print($"{player.Name} przesuwa się na pole {newFieldId}");

		await MovePawn(player.Pawn, newFieldId, 0);
		ShowFieldTexture(newFieldId);
		await HandleFieldAction(newFieldId);

		CheckWinCondition(player); // Sprawdzenie warunków zwycięstwa

		NextTurn();
	}

	private async Task MovePlayerSequentially(Player player, int steps)
	{
		int targetIndex = player.CurrentFieldId + steps;

		if (targetIndex >= 40)
		{
			targetIndex = targetIndex % 40;
		}

		while (player.CurrentFieldId != targetIndex)
		{
			player.CurrentFieldId = (player.CurrentFieldId + 1) % fields.Count;
			await MovePawn(player.Pawn, player.CurrentFieldId, 0);
			ShowFieldTexture(player.CurrentFieldId);
			// Możesz dodać krótkie opóźnienie, aby ruch był widoczny
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		}

		CheckWinCondition(player);
		NextTurn();
	}

	private void CheckWinCondition(Player player)
	{
		if (player.CurrentFieldId == fields.Count - 1)
		{
			GD.Print($"{player.Name} wygrał grę!");
			ShowNotification($"{player.Name} wygrał grę!", 5f);
			// Możesz dodać tutaj logikę zatrzymania gry lub restartu
		}
	}

	public async Task MovePawn(Figurehead pawn, int fieldId, int positionIndex)
	{
		Vector3? targetPos = GetPositionForPawn(fieldId, positionIndex);
		if (targetPos.HasValue)
		{
			Tween tween = CreateTween();
			tween.TweenProperty(pawn, "global_position", targetPos.Value, 0.5f)
				 .SetTrans(Tween.TransitionType.Linear)
				 .SetEase(Tween.EaseType.InOut);
			await ToSignal(tween, "finished");
		}
		else
		{
			GD.PrintErr("Nie udało się przesunąć pionka: nieprawidłowe pole lub indeks pozycji.");
		}
	}

	public Vector3? GetPositionForPawn(int fieldId, int positionIndex)
	{
		Field field = fields.Find(f => f.FieldId == fieldId);
		if (field != null && field.Positions.Count > positionIndex)
		{
			return field.Positions[positionIndex];
		}
		GD.PrintErr("Błąd: Nie znaleziono pola lub indeks pozycji jest nieprawidłowy.");
		return null;
	}

	public Field GetFieldById(int fieldId)
	{
		return fields.Find(f => f.FieldId == fieldId);
	}

	private void ShowNotification(string message, float duration = 3f)
	{
		Node canvas = GetNodeOrNull("/root/Level/CanvasLayer");
		if (canvas != null)
		{
			Label notificationLabel = canvas.GetNodeOrNull<Label>("NotificationPanel/NotificationLabel");
			Panel notificationPanel = canvas.GetNodeOrNull<Panel>("NotificationPanel");
			if (notificationLabel != null && notificationPanel != null)
			{
				notificationLabel.Text = message;
				notificationPanel.Visible = true;
				notificationLabel.Visible = true;

				// Ukryj powiadomienie po określonym czasie
				var timer = GetTree().CreateTimer(duration);
				timer.Connect("timeout", new Callable(this, nameof(HideNotification)));
			}
			else
			{
				GD.PrintErr("Nie znaleziono NotificationPanel lub NotificationLabel.");
			}
		}
		else
		{
			GD.PrintErr("Nie znaleziono CanvasLayer.");
		}
	}

	private void HideNotification()
	{
		Node canvas = GetNodeOrNull("/root/Level/CanvasLayer");
		if (canvas != null)
		{
			Label notificationLabel = canvas.GetNodeOrNull<Label>("NotificationPanel/NotificationLabel");
			Panel notificationPanel = canvas.GetNodeOrNull<Panel>("NotificationPanel");
			if (notificationLabel != null && notificationPanel != null)
			{
				notificationPanel.Visible = false;
				notificationLabel.Visible = false;
			}
		}
	}

	private void EnableRollDiceButton()
	{
		Button rollDiceButton = GetNodeOrNull<Button>(RollDiceButtonPath);
		if (rollDiceButton != null)
		{
			rollDiceButton.Disabled = false;
		}
	}
}
