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
	

	public override void _Ready()
{
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
