using Godot;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public partial class Board : StaticBody3D
{
	private Vector3 targetPosition;
	private List<Field> fields = new List<Field>();
	private Sprite2D textureDisplay;
	private Sprite2D randomCard;
	private Sprite2D step_on_card;
	private Button endTurnButton;
	private GameManager gameManager;
	private CanvasLayer BuyCard;
	private TextureRect cardView;
	private Timer buyTime;

	private readonly Dictionary<(string type, int number), int> cardEffects = new Dictionary<(string type, int number), int>
	{
		// Community cards effects
		{ ("community", 4), -150 },
		{ ("community", 5), -50 },
		{ ("community", 6), -100 },
		{ ("community", 7), 50 },
		{ ("community", 8), 150 },
		{ ("community", 9), -100 },
		{ ("community", 10), 200 },
		{ ("community", 11), 200 },
		{ ("community", 12), 200 },
		{ ("community", 13), -100 },
		{ ("community", 15), 200 },
		{ ("community", 16), -50 },

		// Chance cards effects
		{ ("chance", 2), 100 },
		{ ("chance", 3), 150 },
		{ ("chance", 6), 100 },
		{ ("chance", 11), -150 },
		{ ("chance", 12), -150 },
		{ ("chance", 13), -50 },
		{ ("chance", 16), 100 },
	};

	public override void _Ready()
	{
		step_on_card = GetNodeOrNull<Sprite2D>("/root/Level/BuyCard/HBoxContainer/FieldView/TextureRect/FieldToBuy");
		randomCard = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect2/RandomCard");
		textureDisplay = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect2/FieldCard");
		cardView = GetNodeOrNull<TextureRect>("/root/Level/BuyCard/HBoxContainer/FieldView/TextureRect");
		buyTime = GetNodeOrNull<Timer>("/root/Level/BuyCard/Timer");
		endTurnButton = GetNodeOrNull<Button>("/root/Level/UI/ZakończTure");
		gameManager = GetNode<GameManager>("/root/Level/GameManager");

		if (textureDisplay == null || step_on_card == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono wymaganych komponentów Sprite2D.");
			return;
		}

		targetPosition = GlobalPosition;
		BuyCard = GetTree().Root.GetNode<CanvasLayer>("Level/BuyCard");
		
		foreach (Node child in GetChildren())
		{
			if (child is Field field)
			{
				fields.Add(field);
			}
		}
	}

	public List<Field> GetFields()
	{
		return fields;
	}

	public async void ShowFieldTexture(int fieldId)
	{
		randomCard.Visible = false;
		textureDisplay.Visible = false;

		if (fieldId == 2 || fieldId == 17 || fieldId == 33)
		{
			await ShowRandomCard("community");
			return;
		}
		else if (fieldId == 7 || fieldId == 22 || fieldId == 36)
		{
			await ShowRandomCard("chance");
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
			float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
			Vector2 scale = new Vector2(scaleFactor, scaleFactor);

			textureDisplay.Scale = new Vector2(0, 0);
			textureDisplay.Visible = true;

			Tween tween = CreateTween();
			tween.TweenProperty(textureDisplay, "scale", scale, 0.15f)
				.SetTrans(Tween.TransitionType.Linear)
				.SetEase(Tween.EaseType.InOut);

			await ToSignal(tween, "finished");
		}
	}

	public void StepOnField(int fieldId)
	{
		if (fieldId == 2 || fieldId == 17 || fieldId == 33)
		{
			ShowRandomCard("community");
		}
		else if (fieldId == 7 || fieldId == 22 || fieldId == 36)
		{
			ShowRandomCard("chance");
		}
		else if (fieldId == 4 || fieldId == 38 || fieldId == 20 || fieldId == 30 || fieldId == 10)
		{
			endTurnButton.Visible = true;
		}
		else
		{
			BuyField(fieldId);
		}
	}

	public void BuyField(int fieldId)
	{
		buyTime.Start();
		randomCard.Visible = false;

		string textureName = $"Field{fieldId}";
		Texture2D fieldTexture = ResourceLoader.Load<Texture2D>($"res://scenes/board/level/textures/{textureName}.png");

		if (fieldTexture != null)
		{
			step_on_card.Texture = fieldTexture;
			Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
			float scaleFactorX = viewportSize.X / 3000f;
			float scaleFactorY = viewportSize.Y / 1250f;
			float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
			Vector2 scale = new Vector2(scaleFactor, scaleFactor);

			step_on_card.Scale = scale;
			cardView.Scale = scale;
			BuyCard.Visible = true;
		}
	}

	public async Task ShowRandomCard(string cardType)
	{
		randomCard.Visible = false;
		textureDisplay.Visible = false;

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float scaleFactorX = viewportSize.X / 2500f;
		float scaleFactorY = viewportSize.Y / 1080f;
		float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
		Vector2 scale = new Vector2(scaleFactor, scaleFactor);

		randomCard.Scale = scale;
		Random random = new Random();
		int cardNumber = random.Next(2, 17);

		// Show card back first
		string initialTexturePath = $"res://scenes/board/level/textures/{(cardType == "community" ? "community_chest/community_1" : "chances/chance_1")}.png";
		var cardTexture = ResourceLoader.Load<Texture2D>(initialTexturePath);

		if (cardTexture != null)
		{
			randomCard.Texture = cardTexture;
			randomCard.Visible = true;

			// Animate card flip
			Tween tween = CreateTween();
			tween.TweenProperty(randomCard, "rotation_degrees", 360 * 2, 1.2f)
				.SetTrans(Tween.TransitionType.Circ)
				.SetEase(Tween.EaseType.InOut);

			await ToSignal(tween, "finished");

			randomCard.RotationDegrees = 0;

			// Show actual card
			string finalTexturePath = $"res://scenes/board/level/textures/{(cardType == "community" ? "community_chest/community" : "chances/chance")}_{cardNumber}.png";
			cardTexture = ResourceLoader.Load<Texture2D>(finalTexturePath);
			randomCard.Texture = cardTexture;
			randomCard.Visible = true;

			// Process card effect
			ProcessCardEffect(cardType, cardNumber);
		}
	}

	private void ProcessCardEffect(string cardType, int cardNumber)
	{
		if (cardEffects.TryGetValue((cardType, cardNumber), out int ectsAmount))
		{
			var currentPlayerIndex = gameManager.GetCurrentPlayerIndex();
			if (ectsAmount > 0)
			{
				gameManager.AddEctsToPlayer(currentPlayerIndex, ectsAmount);
				GD.Print($"Karta {cardType} {cardNumber}: Gracz otrzymał {ectsAmount} ECTS");
			}
			else
			{
				gameManager.AddEctsToPlayer(currentPlayerIndex, ectsAmount);
				GD.Print($"Karta {cardType} {cardNumber}: Gracz stracił {-ectsAmount} ECTS");
			}
		}
	}

	public Vector3? GetPositionForPawn(int fieldId, int positionIndex)
	{
		Field field = fields.Find(f => f.FieldId == fieldId);
		if (field != null && positionIndex >= 0 && positionIndex < field.positions.Count)
		{
			return field.positions[positionIndex];
		}
		return null;
	}

	public async void MovePawn(Figurehead pawn, int fieldId, int positionIndex)
	{
		Vector3? targetPosition = GetPositionForPawn(fieldId, positionIndex);
		if (targetPosition.HasValue)
		{
			Tween tween = CreateTween();
			tween.TweenProperty(pawn, "global_position", targetPosition.Value, 0.5f)
				.SetTrans(Tween.TransitionType.Linear)
				.SetEase(Tween.EaseType.InOut);
			await ToSignal(tween, "finished");
		}
	}

	public Field GetFieldById(int fieldId)
	{
		return fields.Find(field => field.FieldId == fieldId);
	}

	public override void _Process(double delta)
	{
		// Process logic if needed
	}
}
