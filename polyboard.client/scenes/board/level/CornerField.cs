using Godot;
using System;

public partial class CornerField : Field
{
	
	public override void _Ready()
	{
		SetPositions();
		base._Ready();
		Texture2D texture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/CornerFieldSelectionBorder1.png");
			_border.Texture=texture;
		
	}
	  public override void SetPositions()
	{
		fieldMeshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		_border = GetNode<Sprite3D>("Border");
		_border.Transform= _border.Transform.Rotated(Vector3.Right, Mathf.DegToRad(90));
		_area = GetNode<Area3D>("Area3D");
		if (fieldMeshInstance != null)
		{
			
			
			var mesh = fieldMeshInstance.Mesh;
				var boundingBox = mesh.GetAabb();
				bRCL = boundingBox.Position + boundingBox.Size;
				bRCG = fieldMeshInstance.ToGlobal(bRCL);
		}
		_border.Visible=false;
		
			
		if(FieldId==0)
		{
			
			
			positions.Add(new Vector3(bRCG.X - 3.5f, 0.5f, bRCG.Z - 2.5f));
		positions.Add(new Vector3(bRCG.X - 3.5f, 0.5f, bRCG.Z - 1.5f));
		positions.Add(new Vector3(bRCG.X - 2.5f, 0.5f, bRCG.Z - 1.5f));
		positions.Add(new Vector3(bRCG.X - 3.5f, 0.5f, bRCG.Z - 0.5f));
		positions.Add(new Vector3(bRCG.X - 2.5f, 0.5f, bRCG.Z - 0.5f));
		positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z - 0.5f));
		
	   }
		else if(FieldId==10)
		{
			
			
		positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z - 3.5f));
		positions.Add(new Vector3(bRCG.X + 2.5f, 0.5f, bRCG.Z - 3.5f));
		positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z - 3.5f));
		positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z - 3.5f));
		positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z - 2.5f));
		positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z - 1.5f));
		
		}
		else if(FieldId==20)
		{
		positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z + 3.5f));
		positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z + 2.5f));
		positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z + 1.5f));
		positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z + 0.5f));
		positions.Add(new Vector3(bRCG.X + 2.5f, 0.5f, bRCG.Z + 0.5f));
		positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z + 0.5f));
		}
		else if(FieldId==30)
		{
		positions.Add(new Vector3(bRCG.X - 3.5f, 0.5f, bRCG.Z + 3.5f));
		positions.Add(new Vector3(bRCG.X - 2.5f, 0.5f, bRCG.Z + 3.5f));
		positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z + 3.5f));
		positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z + 3.5f));
		positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z + 2.5f));
		positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z + 1.5f));
		}
	}
	// Called when the node enters the scene tree for the first time.
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
