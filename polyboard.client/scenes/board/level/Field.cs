using Godot;
using System;
using System.Collections.Generic;

public partial class Field : Node3D
{
	protected MeshInstance3D fieldMeshInstance;
	protected Vector3 bRCL; //dolny prawy rog - lokalna pozycja
	protected Vector3 bRCG; //dolny prawy rog - globalna pozycja
	public List<Vector3> positions=new List<Vector3>(6);
	protected Sprite3D _border;
	protected Area3D _area;
	protected static int nextId = 0;
	public int FieldId;
	protected Sprite2D viewDetailsDialog;

	public Field()
	{
		FieldId = nextId;
		nextId++;
	}
	public override void _Ready()
	{
		
		viewDetailsDialog = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect/ViewDetailsDialog");
		if (viewDetailsDialog == null)
	{
		GD.PrintErr("Błąd: Nie znaleziono Sprite2D do wyświetlania dialogu wyswietlenia detali.");
	}
		SetPositions();
		_area.Connect("mouse_entered", new Callable(this, nameof(OnMouseEntered))); 
		_area.Connect("mouse_exited", new Callable(this, nameof(OnMouseExited)));
		_area.Connect("input_event", new Callable(this, nameof(OnInputEvent)));
		
	}
	public virtual void SetPositions()
	{
		fieldMeshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		_border = GetNode<Sprite3D>("Border");
		_area = GetNode<Area3D>("Area3D");
		if(_border==null || _area==null)
		{
			GD.PrintErr("Nie znaleziono obiektow do wyswietlania krawedzi pola ");
		}
		_border.Visible = false;
		
		if (fieldMeshInstance != null)
		{
			
			
			var mesh = fieldMeshInstance.Mesh;
				var boundingBox = mesh.GetAabb();
				bRCL = boundingBox.Position + boundingBox.Size;
				bRCG = fieldMeshInstance.ToGlobal(bRCL);

			
			if(FieldId>=1 && FieldId<=9)
			{
				positions.Add(new Vector3(bRCG.X+2.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X+2.5f, 0.5f, bRCG.Z-0.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z-0.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z-0.5f));
				
			}
			else if(FieldId>=11 && FieldId<=19)
			{
				
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+2.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+2.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+0.5f));
			}
			else if(FieldId>=21 && FieldId<=30)
			{
				positions.Add(new Vector3(bRCG.X-2.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-2.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z+0.5f));
			}
			else if(FieldId>=31 && FieldId<=40)
			{
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-2.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-2.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-0.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-0.5f));
			}
				
				
				
		}
	}
	 protected void OnInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx) 
	{ 
		if (@event is InputEventMouseButton mouseButton)
		{
		if (mouseButton.Pressed && mouseButton.ButtonIndex ==MouseButton.Left)
		{
			var parent = GetParent() as Board;
			parent?.ShowFieldTexture(FieldId);
		}
		}
		else if (@event is InputEventMouseMotion) 
	{ 
		ShowDetailsDialog();
		_border.Visible = true; 
	} else if (@event is InputEventMouseButton) 
	{ 
		_border.Visible = false;
	}
	
	}
	
	public void ShowDetailsDialog()
	{
		Texture2D detailsTexture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/ViewDetailsDialog.png");
		viewDetailsDialog.Texture=detailsTexture;
		
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
			
		float scaleFactorX = viewportSize.X / 2500f;  
		float scaleFactorY = viewportSize.Y / 1080f;  
		float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
		Vector2 scale = new Vector2(scaleFactor, scaleFactor);
			
		viewDetailsDialog.Scale = scale; 
		viewDetailsDialog.Visible = true; 	
	}
	
	protected void OnMouseEntered()
	 { 
		_border.Visible = true; 
	 }
	
	 protected void OnMouseExited()
	{ 
		_border.Visible = false;
		viewDetailsDialog.Visible=false;
	}
	



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Any additional logic that you want to execute every frame
	}
}
