using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Field : Node3D
{
	public bool isMouseEventEnabled = true;
	protected MeshInstance3D fieldMeshInstance;
	protected Vector3 bRCL; // dolny prawy rog - lokalna pozycja
	protected Vector3 bRCG; // dolny prawy rog - globalna pozycja
	public List<Vector3> positions = new List<Vector3>(6);
	public List<Vector3> buildPositions=new List<Vector3>(5);
	public List<bool> occupied= new List<bool>(6);
	public List<bool> buildOccupied=new List<bool>(5);
	public Vector3 buildCameraPosition;
	protected Sprite3D _border;
	protected Area3D _area;
	protected Sprite3D _ownerBorder;
	protected static int nextId = 0;
	public int FieldId;
	public List<Node3D> builtHouses = new List<Node3D>();
	public bool isHotel = false;
	public string Name; // Dodano nazwę pola
	public int ECTSReward = 0; // Ilość ECTS przyznawana za lądowanie na tym polu
	protected Sprite2D viewDetailsDialog;
	protected AudioStreamPlayer3D constructionSoundPlayer;
	protected AudioStreamPlayer3D hotelConstructionSoundPlayer;
	public Figurehead Owner;
	public int OwnerId;
	public bool owned = false;
	public int houseCost;
	public int hotelCost;
	public int fieldCost;
	public List<int> rentCost = new List<int>(6);
	private GameManager gameManager;
	
	public void PayRent(Figurehead player, Field field)
	{
		if(field.isHotel == true)
		{
			player.SpendECTS(rentCost[5]);
			Owner.AddECTS(rentCost[5]);
		}
		else
		{
			int houses = field.CheckHouseQuantity(field);
			player.SpendECTS(rentCost[houses]);
			Owner.AddECTS(rentCost[houses]);
		}
		gameManager.UpdateECTSUI(gameManager.GetCurrentPlayerIndex());
		gameManager.UpdateECTSUI(OwnerId);
	}
	
	public void BuyField(Figurehead player, Field field)
	{
		int id = gameManager.GetCurrentPlayerIndex();
		player.SpendECTS(field.fieldCost);
		gameManager.UpdateECTSUI(id);
		field.Owner = player;
		field.OwnerId = id;
		field.owned = true;
		player.ownedFields[field.FieldId]=true;
		GD.Print(player.Name,"Kupił pole o numerze Id ",field.FieldId);
		GD.Print(player.Name);
		GD.Print("Nowy owner pola: ",field.Owner.Name);
		GD.Print(field.Owner.playerColor);
		 // Pobranie referencji do obiektu ramki
		_ownerBorder = GetNodeOrNull<Sprite3D>("OwnerBorder");
	
		if (_ownerBorder == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono OwnerBorder!");
			return;
		}

	// Ustawienie koloru i widoczności ramki
	_ownerBorder.Modulate = field.Owner.playerColor;
	_ownerBorder.Visible = true;
	}

	public void RemoveOwner(Figurehead player, Field field)	{
		
		int id = gameManager.GetCurrentPlayerIndex();
		if(id == field.OwnerId){
			player.AddECTS((field.fieldCost)/2);
			gameManager.UpdateECTSUI(id);
			//TODO zmienic na bank jak bedzie system zastawu
			field.Owner = null;
			field.OwnerId = 0;
			field.owned = false;
			player.ownedFields[field.FieldId]=false;
			//GD.Print(player.Name,"Kupił pole o numerze Id ",field.FieldId);
			//GD.Print(player.Name);
			//GD.Print("Nowy owner pola: ",field.Owner.Name);
			//GD.Print(field.Owner.playerColor);
			// Pobranie referencji do obiektu ramki
			_ownerBorder = GetNodeOrNull<Sprite3D>("OwnerBorder");
		
			if (_ownerBorder == null)
			{
				GD.PrintErr("Błąd: Nie znaleziono OwnerBorder!");
				return;
			}

		// Ustawienie koloru i widoczności ramki
		//TODO usunąć?
		//_ownerBorder.Modulate = ;
		_ownerBorder.Visible = false;
		}
	}
	
	public int CheckHouseQuantity(Field field)
	{
		int i=0;
		while(i<=field.buildOccupied.Count)
		{
			if(field.buildOccupied[i] == false)
				break;
			i++;
		}
		return i;
	}
	
	public string GetUserNickname(Field field)
	{
		return field.Owner.Name;
	}
	public Field()
	{
		Owner=null;
		FieldId = nextId;
		nextId++;

		// Ustawienie nazwy pola na podstawie FieldId
		Name = $"Field{FieldId}";
		
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
		gameManager = GetNode<GameManager>("/root/Level/GameManager");
		viewDetailsDialog = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect/ViewDetailsDialog");
		constructionSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/ConstructionSound");
		if (viewDetailsDialog == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono Sprite2D do wyświetlania dialogu wyswietlenia detali.");
		}
		SetPositions();
		_area.Connect("mouse_entered", new Callable(this, nameof(OnMouseEntered)));
		_area.Connect("mouse_exited", new Callable(this, nameof(OnMouseExited)));
		_area.Connect("input_event", new Callable(this, nameof(OnInputEvent)));

		// Ustawienie ECTSReward dla CornerField0
		if (FieldId == 0)
		{
			ECTSReward = 200;
		}
	}

	public virtual void SetPositions()
	{
		fieldMeshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		_border = GetNode<Sprite3D>("Border");
		_area = GetNode<Area3D>("Area3D");
		_ownerBorder = GetNodeOrNull<Sprite3D>("OwnerBorder");
		if (_border == null || _area == null || _ownerBorder == null)
		{
			GD.PrintErr("Nie znaleziono obiektów do wyświetlania krawędzi pola.");
		}
		_border.Visible = false;
		_ownerBorder.Visible = false;

		if (fieldMeshInstance != null)
		{
			var mesh = fieldMeshInstance.Mesh;
			var boundingBox = mesh.GetAabb();
			bRCL = boundingBox.Position + boundingBox.Size;
			bRCG = fieldMeshInstance.ToGlobal(bRCL);

			// Przykładowe ustawienie pozycji na podstawie FieldId
			// Możesz dostosować to według swojej logiki planszy
			if (FieldId >= 1 && FieldId <= 9)
			{
				positions.Add(new Vector3(bRCG.X + 2.5f, 0.5f, bRCG.Z - 1.5f));
				positions.Add(new Vector3(bRCG.X + 2.5f, 0.5f, bRCG.Z - 0.5f));
				positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z - 1.5f));
				positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z - 0.5f));
				positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z - 1.5f));
				positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z - 0.5f));
				buildPositions.Add(new Vector3(bRCG.X+3.6f, 0.28f, bRCG.Z-0.3f));
				buildPositions.Add(new Vector3(bRCG.X+3.6f, 0.28f, bRCG.Z-0.8f));
				buildPositions.Add(new Vector3(bRCG.X+3.6f, 0.28f, bRCG.Z-1.3f));
				buildPositions.Add(new Vector3(bRCG.X+3.6f, 0.28f, bRCG.Z-1.8f));
				buildPositions.Add(new Vector3(bRCG.X+3.65f, 0.28f, bRCG.Z-1.0f));
				buildCameraPosition=new Vector3(bRCG.X-1.0f,0.8f,bRCG.Z-1.0f);
			}
			else if (FieldId >= 11 && FieldId <= 19)
			{
				positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z + 2.5f));
				positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z + 2.5f));
				positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z + 1.5f));
				positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z + 1.5f));
				positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z + 0.5f));
				positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z + 0.5f));
				buildPositions.Add(new Vector3(bRCG.X+0.3f, 0.28f, bRCG.Z+3.55f));
				buildPositions.Add(new Vector3(bRCG.X+0.8f, 0.28f, bRCG.Z+3.55f));
				buildPositions.Add(new Vector3(bRCG.X+1.3f, 0.28f, bRCG.Z+3.55f));
				buildPositions.Add(new Vector3(bRCG.X+1.8f, 0.28f, bRCG.Z+3.55f));
				buildPositions.Add(new Vector3(bRCG.X+1.0f, 0.28f, bRCG.Z+3.65f));
				buildCameraPosition=new Vector3(bRCG.X+1.0f,0.8f,bRCG.Z-1.0f);
			}
			else if (FieldId >= 21 && FieldId <= 30)
			{
				positions.Add(new Vector3(bRCG.X - 2.5f, 0.5f, bRCG.Z + 1.5f));
				positions.Add(new Vector3(bRCG.X - 2.5f, 0.5f, bRCG.Z + 0.5f));
				positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z + 1.5f));
				positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z + 0.5f));
				positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z + 1.5f));
				positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z + 0.5f));
				buildPositions.Add(new Vector3(bRCG.X-3.55f, 0.28f, bRCG.Z+0.3f));
				buildPositions.Add(new Vector3(bRCG.X-3.55f, 0.28f, bRCG.Z+0.8f));
				buildPositions.Add(new Vector3(bRCG.X-3.55f, 0.28f, bRCG.Z+1.3f));
				buildPositions.Add(new Vector3(bRCG.X-3.55f, 0.28f, bRCG.Z+1.8f));
				buildPositions.Add(new Vector3(bRCG.X-3.65f, 0.28f, bRCG.Z+1.0f));
				buildCameraPosition=new Vector3(bRCG.X+1.0f,0.8f,bRCG.Z+1.0f);
			}
			else if (FieldId >= 31 && FieldId <= 40)
			{
				positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z - 2.5f));
				positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z - 2.5f));
				positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z - 1.5f));
				positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z - 1.5f));
				positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z - 0.5f));
				positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z - 0.5f));
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
			if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
			{
				var parent = GetParent() as Board;
				parent?.ShowFieldTexture(FieldId);
			}
		}
		else if (@event is InputEventMouseMotion)
		{
			ShowDetailsDialog();
			_border.Visible = true;
		}
		else if (@event is InputEventMouseButton)
		{
			_border.Visible = false;
		}
	}
	public void RemoveAllHouses()
	{
	foreach (var house in builtHouses)
	{
		if (house != null)
		{
			house.QueueFree(); // Usuwanie obiektu z gry
		}
	}
	builtHouses.Clear(); // Wyczyszczenie listy
	for (int i = 0; i < buildOccupied.Count; i++)
	{
		buildOccupied[i] = false; // Resetowanie stanu zajętości
	}
	}
	public async void BuildingHouse(int FieldId)
	{
		await BuildHouse(FieldId);
	}
	
	private async Task BuildHouse(int FieldId)
	{
	HashSet<int> invalidFieldIds = new HashSet<int> { 0, 2, 4, 5, 7, 10, 12, 15, 17, 20, 22, 25, 28, 30, 33, 35, 36, 38 };
	if (invalidFieldIds.Contains(FieldId))
	{
		return;
	}
	if (isHotel)
	{
		GD.Print("Hotel już jest zbudowany. Nie można budować więcej domków.");
		return;
	}
	 int freeIndex = buildOccupied.FindIndex(occupied => !occupied);
	if (freeIndex == -1 || freeIndex == 4)
	{
		BuildHotel(FieldId);
		GD.Print("Nie ma wolnego miejsca na budowę domku.");
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
		builtHouses.Add(homeInstance);
		homeInstance.RotationDegrees = new Vector3(0, -270, 0);
		homeInstance.Scale = new Vector3(0.01f, 0.01f, 0.01f);
		Vector3 defaultHouseScale = new Vector3(0.5f, 0.5f, 0.5f);
		AddChild(homeInstance);
		AddChild(puffInstance);
		homeInstance.GlobalPosition = buildPositions[freeIndex];
		puffInstance.GlobalPosition = buildPositions[freeIndex];
		puffInstance.Scale = new Vector3(1.0f, 1.0f, 1.0f);
		
		buildOccupied[freeIndex] = true;

		Timer timer = new Timer();
		timer.WaitTime = 3.0f;
		timer.OneShot = true;
		GetTree().Root.AddChild(timer);

		var buildCamera = new Camera3D();
		GetTree().Root.AddChild(buildCamera);
		buildCamera.GlobalPosition = buildCameraPosition;
		buildCamera.LookAt(buildPositions[freeIndex], Vector3.Up);
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
		
		puffInstance.QueueFree();
		
		timer.WaitTime=1.5f;
		timer.Start();
		await ToSignal(timer, "timeout");
		
		
		
		
		buildCamera.QueueFree();
		return;
	}
	else
	{
		ShowError("Nie udało się stworzyć sceny domku.");
		GD.PrintErr("Nie udało się stworzyć sceny domku.");
		return;
	}
	}

	public async Task BuildHotel(int FieldId)
	{
		RemoveAllHouses();
		HashSet<int> invalidFieldIds = new HashSet<int> { 0, 2, 4, 5, 7, 10, 12, 15, 17, 20, 22, 25, 28, 30, 33, 35, 36, 38 };
	if (invalidFieldIds.Contains(FieldId))
	{
	return;
	}
		var hotelScene=GD.Load<PackedScene>("res://scenes/board/buildings/hotel.tscn");
		var puffScene = GD.Load<PackedScene>("res://scenes/board/buildings/puff.tscn");
		
	if (hotelScene == null)
	{
		ShowError("Nie udało się załadować sceny hotelu.");
		GD.PrintErr("Nie udało się załadować sceny hotelu.");
		return;
	}
	
	if (puffScene == null)
	{
		ShowError("Nie udało się załadować sceny efektu");
		GD.PrintErr("Nie udało się załadować sceny efektu");
		return;
	}
	
	var hotelInstance = hotelScene.Instantiate() as Node3D;
	var puffInstance = puffScene.Instantiate<Node3D>();
	
	 if (hotelInstance != null && puffInstance != null)
	{
		
		hotelInstance.RotationDegrees = new Vector3(0, 0, 0);
		hotelInstance.Scale=new Vector3(0.01f,0.01f,0.01f);
		Vector3 defaultHotelScale = new Vector3(0.45f, 0.45f, 0.45f);
		AddChild(hotelInstance);
		AddChild(puffInstance);
		hotelInstance.GlobalPosition = buildPositions[4];
		puffInstance.GlobalPosition = buildPositions[4];
		isHotel = true;
		puffInstance.Scale = new Vector3(2.0f, 2.0f, 2.0f);

		
		Timer timer = new Timer();
		timer.WaitTime = 3.0f;
		timer.OneShot = true;
		GetTree().Root.AddChild(timer);

		var buildCamera = new Camera3D();
		GetTree().Root.AddChild(buildCamera);
		buildCamera.GlobalPosition = buildCameraPosition;
		buildCamera.LookAt(buildPositions[4], Vector3.Up);
		buildCamera.Current = true;

		timer.Start();
		PlayHotelConstructionSound();
		Tween tween = CreateTween();
		tween.TweenProperty(hotelInstance, "scale", defaultHotelScale, 1.5f)
			 .SetTrans(Tween.TransitionType.Linear)
			 .SetEase(Tween.EaseType.InOut);

		
		await ToSignal(tween, "finished");
		await ToSignal(timer, "timeout");
		StopConstructionSound();
		
		puffInstance.QueueFree();
		
		timer.WaitTime=1.5f;
		timer.Start();
		await ToSignal(timer, "timeout");
		
		
		
		
		buildCamera.QueueFree();
		return;

	}
	else
	{
		ShowError("Nie udało się stworzyć sceny hotelu.");
		GD.PrintErr("Nie udało się stworzyć sceny hotelu.");
	}
	}

	public void ShowDetailsDialog()
	{
		Texture2D detailsTexture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/ViewDetailsDialog.png");
		viewDetailsDialog.Texture = detailsTexture;

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

private void PlayConstructionSound()
	{
		if (constructionSoundPlayer != null)
		{
			constructionSoundPlayer.Play();
			GD.Print("Odtwarzanie dźwięku budowania.");
		}
		else
		{
			ShowError("Błąd: AudioStreamPlayer3D nie jest zainicjalizowany.");
			GD.PrintErr("Błąd: AudioStreamPlayer3D nie jest zainicjalizowany.");
		}
	}
private void PlayHotelConstructionSound()
	{
		if (hotelConstructionSoundPlayer != null)
		{
			hotelConstructionSoundPlayer.Play();
			GD.Print("Mission passed.");
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
	
	protected void OnMouseExited()
	{
		if (!isMouseEventEnabled) return;
		_border.Visible = false;
		viewDetailsDialog.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Any additional logic that you want to execute every frame
	}

	//TODO merge in one function. also is it needed? Single responsibility principle! - field should not be responsible for UI notification
	public void ShowNotification(string message, float duration = 3f)
	{
		var notifications = GetNode<Node>("/root/Notifications");
		if (notifications != null)
		{
			notifications.Call("show_notification", message, duration);
		}
		else
		{
			GD.PrintErr("NotificationLayer singleton not found. Make sure it's added as an Autoload.");
		}
	}

	public void ShowError(string message, float duration = 4f)
	{
		var notifications = GetNode<Node>("/root/Notifications");
		if (notifications != null)
		{
			notifications.Call("show_error", message, duration);
		}
		else
		{
			GD.PrintErr("NotificationLayer singleton not found. Make sure it's added as an Autoload.");
			GD.PrintErr(message);
		}
	}
}
