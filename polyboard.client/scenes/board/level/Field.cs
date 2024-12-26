using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Field : Node3D
{
	public bool isMouseEventEnabled = true;
	protected MeshInstance3D fieldMeshInstance;
	protected Vector3 bRCL; //dolny prawy rog - lokalna pozycja
	protected Vector3 bRCG; //dolny prawy rog - globalna pozycja
	public List<Vector3> positions=new List<Vector3>(6);
	public List<Vector3> buildPositions=new List<Vector3>(5);
	public List<bool> occupied= new List<bool>(6);
	public List<bool> buildOccupied=new List<bool>(5);
	public Vector3 buildCameraPosition;
	protected Sprite3D _border;
	protected Area3D _area;
	protected static int nextId = 0;
	public int FieldId;
	protected Sprite2D viewDetailsDialog;
	protected AudioStreamPlayer3D constructionSoundPlayer;

	public Field()
	{
		FieldId = nextId;
		nextId++;
	for (int i = 0; i < 6; i++)
	{
		occupied.Add(false);
	}
	for (int i = 0; i < 5; i++)
	{
		buildOccupied.Add(false);
	}
	}
	public override void _Ready()
	{
		
		viewDetailsDialog = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect/ViewDetailsDialog");
		constructionSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/ConstructionSound");
		if (constructionSoundPlayer == null)
	{
		GD.PrintErr("Błąd: Nie znaleziono odtwarzacza audio.");
	}
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
				buildPositions.Add(new Vector3(bRCG.X+3.6f, 0.28f, bRCG.Z-0.3f));
				buildPositions.Add(new Vector3(bRCG.X+3.6f, 0.28f, bRCG.Z-0.8f));
				buildPositions.Add(new Vector3(bRCG.X+3.6f, 0.28f, bRCG.Z-1.3f));
				buildPositions.Add(new Vector3(bRCG.X+3.6f, 0.28f, bRCG.Z-1.8f));
				buildPositions.Add(new Vector3(bRCG.X+3.65f, 0.28f, bRCG.Z-1.0f));
				buildCameraPosition=new Vector3(bRCG.X-1.0f,0.8f,bRCG.Z-1.0f);
		
				
			}
			else if(FieldId>=11 && FieldId<=19)
			{
				
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+2.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+2.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+0.5f));
				buildPositions.Add(new Vector3(bRCG.X+0.3f, 0.28f, bRCG.Z+3.55f));
				buildPositions.Add(new Vector3(bRCG.X+0.8f, 0.28f, bRCG.Z+3.55f));
				buildPositions.Add(new Vector3(bRCG.X+1.3f, 0.28f, bRCG.Z+3.55f));
				buildPositions.Add(new Vector3(bRCG.X+1.8f, 0.28f, bRCG.Z+3.55f));
				buildPositions.Add(new Vector3(bRCG.X+1.0f, 0.28f, bRCG.Z+3.65f));
				buildCameraPosition=new Vector3(bRCG.X+1.0f,0.8f,bRCG.Z-1.0f);
				
			}
			else if(FieldId>=21 && FieldId<=30)
			{
				positions.Add(new Vector3(bRCG.X-2.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-2.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z+0.5f));
				buildPositions.Add(new Vector3(bRCG.X-3.55f, 0.28f, bRCG.Z+0.3f));
				buildPositions.Add(new Vector3(bRCG.X-3.55f, 0.28f, bRCG.Z+1.8f));
				buildPositions.Add(new Vector3(bRCG.X-3.55f, 0.28f, bRCG.Z+1.3f));
				buildPositions.Add(new Vector3(bRCG.X-3.55f, 0.28f, bRCG.Z+1.8f));
				buildPositions.Add(new Vector3(bRCG.X-3.65f, 0.28f, bRCG.Z+1.0f));
				buildCameraPosition=new Vector3(bRCG.X+1.0f,0.8f,bRCG.Z+1.0f);
		
				
			}
			else if(FieldId>=31 && FieldId<=40)
			{
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-2.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-2.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-0.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-0.5f));
				buildPositions.Add(new Vector3(bRCG.X-0.3f, 0.28f, bRCG.Z-3.6f));
				buildPositions.Add(new Vector3(bRCG.X-0.8f, 0.28f, bRCG.Z-3.6f));
				buildPositions.Add(new Vector3(bRCG.X-1.3f, 0.28f, bRCG.Z-3.6f));
				buildPositions.Add(new Vector3(bRCG.X-1.8f, 0.28f, bRCG.Z-3.6f));
				buildPositions.Add(new Vector3(bRCG.X-1.0f, 0.28f, bRCG.Z-3.65f));
				buildCameraPosition=new Vector3(bRCG.X-1.0f,0.8f,bRCG.Z+1.0f);
				
			}
				
				
				
		}
	}
	 protected void OnInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx) 
	{ 
		if (!isMouseEventEnabled) return;
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
	
	public async Task BuildHouse(int FieldId)
{
	HashSet<int> invalidFieldIds = new HashSet<int> { 0, 2, 4, 5, 7, 10, 12, 15, 17, 20, 22, 25, 28, 30, 33, 35, 36, 38 };
	if (invalidFieldIds.Contains(FieldId))
	{
		return;
	}

	var houseScene = GD.Load<PackedScene>("res://scenes/board/buildings/house.tscn");
	var puffScene = GD.Load<PackedScene>("res://scenes/board/buildings/puff.tscn");
	if (houseScene == null)
	{
		GD.PrintErr("Nie udało się załadować sceny domu.");
		return;
	}
	if (puffScene == null)
	{
		GD.PrintErr("Nie udało się załadować sceny efektu");
		return;
	}

	var homeInstance = houseScene.Instantiate<Node3D>();
	var puffInstance = puffScene.Instantiate<Node3D>();
	if (homeInstance != null && puffInstance != null)
	{
		homeInstance.RotationDegrees = new Vector3(0, -270, 0);
		homeInstance.Scale = new Vector3(0.01f, 0.01f, 0.01f);
		Vector3 defaultHouseScale = new Vector3(0.5f, 0.5f, 0.5f);
		AddChild(homeInstance);
		AddChild(puffInstance);
		homeInstance.GlobalPosition = buildPositions[0];
		puffInstance.GlobalPosition = buildPositions[0];
		puffInstance.Scale = new Vector3(1.0f, 1.0f, 1.0f);

		Timer timer = new Timer();
		timer.WaitTime = 3.0f;
		timer.OneShot = true;
		GetTree().Root.AddChild(timer);

		var buildCamera = new Camera3D();
		GetTree().Root.AddChild(buildCamera);
		buildCamera.GlobalPosition = buildCameraPosition;
		buildCamera.LookAt(buildPositions[0], Vector3.Up);
		buildCamera.Current = true;

		timer.Start();
		PlayConstructionSound();
		Tween tween = CreateTween();
		tween.TweenProperty(homeInstance, "scale", defaultHouseScale, 1.5f)
			 .SetTrans(Tween.TransitionType.Linear)
			 .SetEase(Tween.EaseType.InOut);

		
		await ToSignal(tween, "finished");
		await ToSignal(timer, "timeout");
		StopConstructionSound();
		
		RemoveChild(puffInstance);
		puffInstance.QueueFree();
		
		timer.WaitTime=1.5f;
		timer.Start();
		await ToSignal(timer, "timeout");
		
		
		
		RemoveChild(buildCamera);
		buildCamera.QueueFree();
		return;
	}
	else
	{
		GD.PrintErr("Nie udało się stworzyć sceny domku.");
		return;
	}
}

	
	
	
	public void BuildHotel(int FieldId)
	{
		HashSet<int> invalidFieldIds = new HashSet<int> { 0, 2, 4, 5, 7, 10, 11, 12, 15, 17, 20, 22, 25, 28, 30, 33, 35, 36, 38 };
	if (invalidFieldIds.Contains(FieldId))
	{
	return;
	}
		var hotelScene=GD.Load<PackedScene>("res://scenes/board/buildings/hotel.tscn");
		var puffScene = GD.Load<PackedScene>("res://scenes/board/buildings/puff.tscn");
		if (hotelScene == null)
	{
		GD.PrintErr("Nie udało się załadować sceny hotelu.");
		return;
	}
	if (puffScene == null)
	{
		GD.PrintErr("Nie udało się załadować sceny efektu");
		return;
	}
	var hotelInstance = hotelScene.Instantiate() as Node3D;
	
	 if (hotelInstance != null)
	{
		
		hotelInstance.RotationDegrees = new Vector3(0, 0, 0);
		hotelInstance.Scale=new Vector3(0.45f,0.45f,0.45f);
		AddChild(hotelInstance);
		hotelInstance.GlobalPosition = buildPositions[4];

	}
	else
	{
		GD.PrintErr("Nie udało się stworzyć sceny domku.");
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
		if (!isMouseEventEnabled) return;
		_border.Visible = true; 
	 }
	
	 protected void OnMouseExited()
	{ 
		if (!isMouseEventEnabled) return;
		_border.Visible = false;
		viewDetailsDialog.Visible=false;
	}
	
	private void PlayConstructionSound()
	{
		if (constructionSoundPlayer != null)
		{
			constructionSoundPlayer.Play();
			GD.Print("Odtwarzanie dźwięku budowania.");
		}
		else
		{
			GD.PrintErr("Błąd: AudioStreamPlayer3D nie jest zainicjalizowany.");
		}
	}

	private void StopConstructionSound()
	{
		if (constructionSoundPlayer != null && constructionSoundPlayer.Playing)
		{
			constructionSoundPlayer.Stop();
			GD.Print("Zatrzymanie dźwięku budowania.");
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Any additional logic that you want to execute every frame
	}
}
