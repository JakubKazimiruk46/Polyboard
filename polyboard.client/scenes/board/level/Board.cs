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
				if (field.FieldId == 1)
				{
					field.Name = "Katedra Leśnictwa";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 60;
					field.rentCost = new List<int> {2, 10, 30, 90, 160, 250};
				}
				else if (field.FieldId == 3)
				{
					field.Name = "Katedra Drzewek";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 60;
					field.rentCost = new List<int> {2, 20, 60, 180, 320, 450};
				}
				else if (field.FieldId == 5)
				{
					field.Name = "Akademik Alfa";
					field.fieldCost = 200;
					field.rentCost = new List<int> {25, 50, 100, 200};
				}
				else if (field.FieldId == 6)
				{
					field.Name = "Katedra Ekonomii";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 100;
					field.rentCost = new List<int> {6, 30, 90, 270, 400, 550};
				}
				else if (field.FieldId == 8)
				{
					field.Name = "Katedra Marketingu";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 100;
					field.rentCost = new List<int> {6, 30, 90, 270, 400, 550};
				}
				else if (field.FieldId == 9)
				{
					field.Name = "Katedra Logistyki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 120;
					field.rentCost = new List<int> {8, 40, 100, 300, 450, 600};
				}
				else if (field.FieldId == 11)
				{
					field.Name = "Katedra Grafiki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 140;
					field.rentCost = new List<int> {10, 50, 150, 450, 625, 750};
				}
				else if (field.FieldId == 12)
				{
					field.Name = "Centrum Nowoczesnego Kształcenia";
					field.fieldCost = 150;
				}
				else if (field.FieldId == 13)
				{
					field.Name = "Katedra Urbanistyki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 140;
					field.rentCost = new List<int> {10, 50, 150, 450, 625, 750};
				}
				else if (field.FieldId == 14)
				{
					field.Name = "Katedra Architektury Wnętrz";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 160;
					field.rentCost = new List<int> {10, 50, 150, 450, 625, 750};
				}
				else if (field.FieldId == 15)
				{
					field.Name = "Akademik Beta";
					field.fieldCost = 200;
					field.rentCost = new List<int> {25, 50, 100, 200};
				}
				else if (field.FieldId == 16)
				{
					field.Name = "Katedra Geotechniki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 180;
					field.rentCost = new List<int> {14, 70, 200, 550, 750, 950};
				}
				else if (field.FieldId == 18)
				{
					field.Name = "Katedra Budownictwa";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 180;
					field.rentCost = new List<int> {14, 70, 200, 550, 750, 950};
				}
				else if (field.FieldId == 19)
				{
					field.Name = "Katedra Konstrukcji Budowlanych";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 200;
					field.rentCost = new List<int> {16, 80, 220, 600, 800, 1000};
				}
				else if (field.FieldId == 21)
				{
					field.Name = "Katedra Biotechnologii";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 220;
					field.rentCost = new List<int> {18, 90, 250, 700, 875, 1050};
				}
				else if (field.FieldId == 21)
				{
					field.Name = "Katedra Ciepłownictwa";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 220;
					field.rentCost = new List<int> {18, 90, 250, 700, 875, 1050};
				}
				else if (field.FieldId == 23)
				{
					field.Name = "Katedra Ciepłownictwa";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 220;
					field.rentCost = new List<int> {18, 90, 250, 700, 875, 1050};
				}
				else if (field.FieldId == 24)
				{
					field.Name = "Katedra Inżynierii Rolnej";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 240;
					field.rentCost = new List<int> {20, 100, 300, 750, 925, 1110};
				}
				else if (field.FieldId == 25)
				{
					field.Name = "Akademik Gamma";
					field.fieldCost = 200;
					field.rentCost = new List<int> {25, 50, 100, 200};
				}
				else if (field.FieldId == 26)
				{
					field.Name = "Katedra Mechaniki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 260;
					field.rentCost = new List<int> {22, 110, 330, 800, 975, 1150};
				}
				else if (field.FieldId == 27)
				{
					field.Name = "Katedra Budowy Maszyn";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 260;
					field.rentCost = new List<int> {22, 110, 330, 800, 975, 1150};
				}
				else if (field.FieldId == 28)
				{
					field.Name = "Klub Gwint";
					field.fieldCost = 150;
				}
				else if (field.FieldId == 29)
				{
					field.Name = "Katedra Inżynierii Biomedycznej";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 280;
					field.rentCost = new List<int> {24, 120, 360, 850, 1025, 1200};
				}
				else if (field.FieldId == 31)
				{
					field.Name = "Katedra Automatyki i Robotyki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 300;
					field.rentCost = new List<int> {26, 130, 390, 900, 1100, 1275};
				}
				else if (field.FieldId == 32)
				{
					field.Name = "Katedra Elektrotechniki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 300;
					field.rentCost = new List<int> {26, 130, 390, 900, 1100, 1275};
				}
				else if (field.FieldId == 34)
				{
					field.Name = "Katedra Fotoniki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 320;
					field.rentCost = new List<int> {28, 150, 450, 1000, 1200, 1400};
				}
				else if (field.FieldId == 35)
				{
					field.Name = "Akademik Delta";
					field.fieldCost = 200;
					field.rentCost = new List<int> {25, 50, 100, 200};
				}
				else if (field.FieldId == 37)
				{
					field.Name = "Katedra Oprogramowania";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 350;
					field.rentCost = new List<int> {35, 175, 500, 1100, 1300, 1500};
				}
				else if (field.FieldId == 39)
				{
					field.Name = "Katedra Matematyki";
					field.houseCost = 150;
					field.hotelCost = 150;
					field.fieldCost = 400;
					field.rentCost = new List<int> {50, 200, 600, 1400, 1700, 2000};
				}
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
			field = gameManager.getCurrentField(fieldId);
			if(field.owned == true)
			{
				string nickname = field.GetUserNickname(field);
				ownerNickname.Text = $"właściciel:\n{nickname}";
			}
			else
			{
				ownerNickname.Text = "Pole nie ma właściciela";
			}
			ownerNickname.Visible = true;
			ownerNicknameView.Visible = true;
			textureDisplay.Scale = new Vector2(0, 0);
			textureDisplay.Visible = true;

			Tween tween = CreateTween();
			tween.TweenProperty(textureDisplay, "scale", scale, 0.15f)
				.SetTrans(Tween.TransitionType.Linear)
				.SetEase(Tween.EaseType.InOut);

			await ToSignal(tween, "finished");
		}
	}
	
	public void HideFieldTexture()
{
	Tween tween = CreateTween();
	tween.TweenProperty(textureDisplay, "scale", Vector2.Zero, 0.15f)
		.SetTrans(Tween.TransitionType.Linear)
		.SetEase(Tween.EaseType.InOut);
   
	ownerNickname.Visible = false;
	ownerNicknameView.Visible = false;
	
	tween.TweenCallback(Callable.From(() => {
		textureDisplay.Visible = false;
	}));
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
