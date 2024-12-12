using Godot;
using System.Collections.Generic;

public partial class Field : Node3D
{
	[Export]
	public int FieldId { get; set; }

	public List<Vector3> Positions { get; private set; } = new List<Vector3>();

	[Export]
	public bool IsMouseEventEnabled { get; set; } = true;

	protected MeshInstance3D fieldMeshInstance;
	protected Sprite3D _border;
	protected Area3D _area;
	protected Sprite2D viewDetailsDialog;

	private static int nextId = 0;

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
			GD.PrintErr("Błąd: Nie znaleziono Sprite2D do wyświetlania dialogu detali.");
		}

		SetPositions();

		// Upewnij się, że węzeł Area3D istnieje w scenie
		_area = GetNodeOrNull<Area3D>("Area3D");
		if (_area != null)
		{
			_area.Connect("mouse_entered", new Callable(this, nameof(OnMouseEntered)));
			_area.Connect("mouse_exited", new Callable(this, nameof(OnMouseExited)));
			_area.Connect("input_event", new Callable(this, nameof(OnInputEvent)));
		}
		else
		{
			GD.PrintErr("Błąd: Nie znaleziono Area3D w Field.");
		}
	}

	// Oznacz metodę jako virtual, aby umożliwić jej nadpisanie
	public virtual void SetPositions()
	{
		fieldMeshInstance = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
		_border = GetNodeOrNull<Sprite3D>("Border");
		if (_border != null)
		{
			_border.Visible = false;
		}

		if (fieldMeshInstance != null)
		{
			var mesh = fieldMeshInstance.Mesh;
			var boundingBox = mesh.GetAabb();
			Vector3 bRCL = boundingBox.Position + boundingBox.Size;
			Vector3 bRCG = fieldMeshInstance.ToGlobal(bRCL);

			// Przykładowe ustawienie pozycji dla różnych pól
			// Możesz dostosować te wartości do swojej planszy
			if (FieldId >= 1 && FieldId <= 9)
			{
				Positions.Add(new Vector3(bRCG.X + 2.5f, 0.5f, bRCG.Z - 1.5f));
				Positions.Add(new Vector3(bRCG.X + 2.5f, 0.5f, bRCG.Z - 0.5f));
				// Dodaj więcej pozycji w razie potrzeby
			}
			// Kontynuuj dla innych pól
		}
	}

	protected void OnInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
	{
		if (!IsMouseEventEnabled) return;

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
			if (_border != null)
			{
				_border.Visible = true;
			}
		}
		else if (@event is InputEventMouseButton)
		{
			if (_border != null)
			{
				_border.Visible = false;
			}
		}
	}

	public void ShowDetailsDialog()
	{
		if (viewDetailsDialog != null)
		{
			Texture2D detailsTexture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/ViewDetailsDialog.png");
			if (detailsTexture != null)
			{
				viewDetailsDialog.Texture = detailsTexture;

				Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

				float scaleFactorX = viewportSize.X / 2500f;
				float scaleFactorY = viewportSize.Y / 1080f;
				float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY);
				Vector2 scale = new Vector2(scaleFactor, scaleFactor);

				viewDetailsDialog.Scale = scale;
				viewDetailsDialog.Visible = true;
			}
			else
			{
				GD.PrintErr("Błąd: Nie można załadować tekstury dialogu detali.");
			}
		}
	}

	protected void OnMouseEntered()
	{
		if (!IsMouseEventEnabled) return;
		if (_border != null)
		{
			_border.Visible = true;
		}
	}

	protected void OnMouseExited()
	{
		if (!IsMouseEventEnabled) return;
		if (_border != null)
		{
			_border.Visible = false;
		}
		if (viewDetailsDialog != null)
		{
			viewDetailsDialog.Visible = false;
		}
	}
}
