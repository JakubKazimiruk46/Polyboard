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
	private Label ownerNickname;
	private Sprite2D step_on_card;
	private Button endTurnButton;
	private TextureButton tradeButton;
	private TextureButton buildButton;
	private GameManager gameManager;
	private CanvasLayer BuyCard;
	private TextureRect cardView;
	private Timer buyTime;
	private Field field;
	private PanelContainer ownerNicknameView;

	private readonly Dictionary<(string type, int number), (int ectsEffect, Func<Task> specialEffect)> cardEffects;

	public Board()
	{
		cardEffects = new Dictionary<(string type, int number), (int ectsEffect, Func<Task> specialEffect)>();
	}

	public override void _Ready()
	{
		InitializeComponents();
		InitializeCardEffects();
	}

	private void InitializeComponents()
	{
		step_on_card = GetNodeOrNull<Sprite2D>("/root/Level/BuyCard/HBoxContainer/FieldView/TextureRect/FieldToBuy");
		randomCard = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect2/RandomCard");
		textureDisplay = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect2/FieldCard");
		ownerNickname = GetNodeOrNull<Label>("/root/Level/CanvasLayer/OwnerNickname/owner_nickname");
		cardView = GetNodeOrNull<TextureRect>("/root/Level/BuyCard/HBoxContainer/FieldView/TextureRect");
		buyTime = GetNodeOrNull<Timer>("/root/Level/BuyCard/Timer");
		endTurnButton = GetNodeOrNull<Button>("/root/Level/UI/ZakończTure");
		tradeButton = GetNodeOrNull<TextureButton>("/root/Level/UI/HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer2/trade_button");
		buildButton = GetNodeOrNull<TextureButton>("/root/Level/UI/HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer3/build_button");
		gameManager = GetNode<GameManager>("/root/Level/GameManager");
		ownerNicknameView = GetNodeOrNull<PanelContainer>("/root/Level/CanvasLayer/OwnerNickname");
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

	private void InitializeCardEffects()
	{
		// Community cards effects
		cardEffects.Add(("community", 4), (-150, null));
		cardEffects.Add(("community", 5), (-50, null));
		cardEffects.Add(("community", 6), (-100, null));
		cardEffects.Add(("community", 7), (50, null));
		cardEffects.Add(("community", 8), (150, null));
		cardEffects.Add(("community", 9), (-100, null));
		cardEffects.Add(("community", 10), (200, null));
		cardEffects.Add(("community", 11), (200, null));
		cardEffects.Add(("community", 12), (200, null));
		cardEffects.Add(("community", 13), (-100, null));
		cardEffects.Add(("community", 15), (200, null));
		cardEffects.Add(("community", 16), (-50, null));

		// Chance cards effects
		cardEffects.Add(("chance", 2), (100, null));
		cardEffects.Add(("chance", 3), (150, null));
		cardEffects.Add(("chance", 5), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveByFields(-2, this);
		}));
		cardEffects.Add(("chance", 6), (100, null));
		cardEffects.Add(("chance", 7), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("chance", 11), (-150, null));
		cardEffects.Add(("chance", 12), (-150, null));
		cardEffects.Add(("chance", 13), (-50, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveByFields(-2, this);
		}));
	}

	private async Task ProcessCardEffect(string cardType, int cardNumber)
	{
		if (cardEffects.TryGetValue((cardType, cardNumber), out var effect))
		{
			// Hide end turn button during effect processing
			endTurnButton.Visible = false;
			
			var currentPlayerIndex = gameManager.GetCurrentPlayerIndex();

			// Apply ECTS effect
			if (effect.ectsEffect != 0)
			{
				gameManager.AddEctsToPlayer(currentPlayerIndex, effect.ectsEffect);
				
				if (effect.ectsEffect > 0)
				{
					GD.Print($"Karta {cardType} {cardNumber}: Gracz otrzymał {effect.ectsEffect} ECTS");
				}
				else
				{
					GD.Print($"Karta {cardType} {cardNumber}: Gracz stracił {-effect.ectsEffect} ECTS");
				}

				// Small delay for visual feedback
				await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			}

			// Execute special card effect (pawn movement) if it exists
			if (effect.specialEffect != null)
			{
				try
				{
					await effect.specialEffect();
					// Add delay after special effect
					await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
				}
				catch (Exception e)
				{
					GD.PrintErr($"Błąd podczas wykonywania specjalnego efektu karty: {e.Message}");
				}
			}

			// Show end turn button after all effects are processed
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
	}

	public async void StepOnField(int fieldId)
	{
		// Wait for any animations to complete
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

		// Hide end turn button initially
		endTurnButton.Visible = false;

		if (fieldId == 2 || fieldId == 17 || fieldId == 33)
		{
			await ShowRandomCard("community");
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
		else if (fieldId == 7 || fieldId == 22 || fieldId == 36)
		{
			await ShowRandomCard("chance");
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
		else if (fieldId == 4 || fieldId == 38 || fieldId == 20 || fieldId == 30 || fieldId == 10)
		{
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
		else
		{
			Figurehead currentFigureHead = gameManager.getCurrentPlayer();
			int current_position = currentFigureHead.GetCurrentPositionIndex();
			Field field = gameManager.getCurrentField(current_position);
			if(field.owned==false)
				BuyField(fieldId);
			else
			{
				endTurnButton.Visible = true;
				GD.Print(field.Owner.Name);
			}

		}
	}

	public void BuyField(int fieldId)
	{
		buyTime.Start();
		randomCard.Visible = false;
		tradeButton.Disabled = true;
		buildButton.Disabled = true;
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
		ownerNicknameView.Visible = false;
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float scaleFactorX = viewportSize.X / 2500f;
		float scaleFactorY = viewportSize.Y / 1080f;
		float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
		Vector2 scale = new Vector2(scaleFactor, scaleFactor);

		randomCard.Scale = scale;
		Random random = new Random();
		int cardNumber = random.Next(2, 17);

		string initialTexturePath = $"res://scenes/board/level/textures/{(cardType == "community" ? "community_chest/community_1" : "chances/chance_1")}.png";
		var cardTexture = ResourceLoader.Load<Texture2D>(initialTexturePath);

		if (cardTexture != null)
		{
			randomCard.Texture = cardTexture;
			randomCard.Visible = true;

			Tween tween = CreateTween();
			tween.TweenProperty(randomCard, "rotation_degrees", 360 * 2, 1.2f)
				.SetTrans(Tween.TransitionType.Circ)
				.SetEase(Tween.EaseType.InOut);

			await ToSignal(tween, "finished");

			randomCard.RotationDegrees = 0;

			string finalTexturePath = $"res://scenes/board/level/textures/{(cardType == "community" ? "community_chest/community" : "chances/chance")}_{cardNumber}.png";
			cardTexture = ResourceLoader.Load<Texture2D>(finalTexturePath);
			randomCard.Texture = cardTexture;
			randomCard.Visible = true;

			await ProcessCardEffect(cardType, cardNumber);
		}
	}

public async void ShowFieldTexture(int fieldId)
	{
		randomCard.Visible = false;
		textureDisplay.Visible = false;

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

	public async Task MovePawn(Figurehead pawn, int fieldId, int positionIndex)
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

	public Vector3? GetPositionForPawn(int fieldId, int positionIndex)
	{
		Field field = fields.Find(f => f.FieldId == fieldId);
		if (field != null && positionIndex >= 0 && positionIndex < field.positions.Count)
		{
			return field.positions[positionIndex];
		}
		return null;
	}

	public Field GetFieldById(int fieldId)
	{
		return fields.Find(field => field.FieldId == fieldId);
	}

	public List<Field> GetFields()
	{
		return fields;
	}
}
