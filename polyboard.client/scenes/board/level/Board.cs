using Godot;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Threading.Tasks;


public partial class Board : StaticBody3D
{
	 
	private Vector3 targetPosition; 
	private List<Field> fields = new List<Field>();
	Figurehead figurehead;
	private Sprite2D textureDisplay;
	private Sprite2D randomCard;
	
	

	public override void _Ready()
{
	randomCard=GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect2/RandomCard");
	 textureDisplay = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect2/FieldCard");
	if (textureDisplay == null)
	{
		GD.PrintErr("Błąd: Nie znaleziono Sprite2D do wyświetlania tekstur.");
	}
	targetPosition = GlobalPosition;
	figurehead = GetTree().Root.GetNode<Figurehead>("Level/Figurehead");
	if (figurehead == null)
	{
		GD.PrintErr("Nie znaleziono pionka.");
	}
	

	foreach (Node child in GetChildren()) 
	{
		if (child is Field field)  
		{
			fields.Add(field); 
		}
	}

	if (figurehead != null)
	{
		MovePawn(figurehead, 0, 0);
		 
	}
	
}

public List<Field> GetFields()
	{
		return fields;
	}
	
	public void ShowFieldTexture(int fieldId)
	{
		randomCard.Visible=false;
		textureDisplay.Visible=false;
		if(fieldId==2 || fieldId==17 || fieldId==33)
		{
			ShowRandomCard("community");
			return;
		}
		else if(fieldId==7 || fieldId==22 || fieldId==36)
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
			
			float scaleFactorX = viewportSize.X / 2500f;  
			float scaleFactorY = viewportSize.Y / 1080f;  
			float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
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
		randomCard.Visible=false;
		textureDisplay.Visible=false;
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float scaleFactorX = viewportSize.X / 2500f;
		float scaleFactorY = viewportSize.Y / 1080f;
		float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
		Vector2 scale = new Vector2(scaleFactor, scaleFactor);

		randomCard.Scale = scale;
		Texture2D cardTexture=null;
		Random random = new Random();
		int number = random.Next(2, 17);
		
		if(cardType=="community")
		{
			cardTexture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/community_chest/community_1.png"); 
		}
		else
		{
			cardTexture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/chances/chance_1.png"); 
		}
		
		if (cardTexture != null)
		{
			randomCard.Texture=cardTexture;
			randomCard.Visible = true;
			Tween tween = CreateTween();
		float duration = 1.2f; 
	
		
	tween.TweenProperty(randomCard, "rotation_degrees", 360*2, duration )
		.SetTrans(Tween.TransitionType.Circ)
		.SetEase(Tween.EaseType.InOut);
	



			await ToSignal(tween, "finished");
		
	randomCard.RotationDegrees=0;	
			
	if(cardType=="community")
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
	

	
	public Vector3? GetPositionForPawn(int fieldId, int positionIndex)
	{
		Field field = fields.Find(f => f.FieldId == fieldId);
		if (field != null && positionIndex >= 0 && positionIndex < field.positions.Count)
		{
			return field.positions[positionIndex];
		}
		GD.PrintErr("Błąd: Nie znaleziono pola lub indeks pozycji jest nieprawidłowy.");
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
	else
	{
		GD.PrintErr("Nie udało się przesunąć pionka: nieprawidłowe pole lub indeks pozycji.");
	}
}

	
	public Field GetFieldById(int fieldId)
	{
		foreach (Field field in fields)
		{
			if (field.FieldId == fieldId)
			{
				return field;
			}
		}
		return null; 
	}
	
	

	
	


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
}
