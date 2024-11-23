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
	
	

	public override void _Ready()
{
	
	 textureDisplay = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/FieldCard");
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
	
	public void ShowFieldTexture(int fieldId)
	{
		
		string textureName = $"Field{fieldId}";
		
		Texture2D fieldTexture = ResourceLoader.Load<Texture2D>($"res://scenes/board/level/textures/{textureName}.png");
		if (fieldTexture != null)
		{
			textureDisplay.Texture = fieldTexture;
			float offsetX = 10; 
			float offsetY =10 ; 
			float textureWidth = fieldTexture.GetSize().X;
			float textureHeight = fieldTexture.GetSize().Y;
			Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
			
			float scaleFactorX = viewportSize.X / 1920f;  
			float scaleFactorY = viewportSize.Y / 1080f;  
			float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
			Vector2 scale = new Vector2(scaleFactor, scaleFactor);
			
			textureDisplay.Scale = scale; 
			
			float posX = viewportSize.X - (textureWidth*scaleFactorX/2) - offsetX;
			float posY = offsetY +(textureHeight*scaleFactorY/2);
			
			textureDisplay.Position = new Vector2(posX, posY);
			textureDisplay.Visible = true; 
			 
		}
		else
		{
			GD.PrintErr($"Błąd: Nie udało się załadować tekstury {textureName}.");
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
