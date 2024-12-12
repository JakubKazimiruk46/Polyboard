using Godot;
using System;

public partial class CornerField : Field
{
	public override void _Ready()
	{
		base._Ready(); // Najpierw wywołaj metodę bazową
		SetPositions(); // Następnie nadpisz pozycje
		Texture2D texture = ResourceLoader.Load<Texture2D>("res://scenes/board/level/textures/CornerFieldSelectionBorder1.png");
		if (_border != null && texture != null)
		{
			_border.Texture = texture;
		}
		else
		{
			GD.PrintErr("Błąd: Nie można załadować tekstury dla CornerField lub _border jest null.");
		}
	}

	public override void SetPositions()
	{
		if (fieldMeshInstance == null)
		{
			fieldMeshInstance = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
		}

		if (_border == null)
		{
			_border = GetNodeOrNull<Sprite3D>("Border");
		}

		if (_border != null)
		{
			_border.Transform = _border.Transform.Rotated(Vector3.Right, Mathf.DegToRad(90));
			_border.Visible = false;
		}

		if (_area == null)
		{
			_area = GetNodeOrNull<Area3D>("Area3D");
		}

		if (fieldMeshInstance != null)
		{
			var mesh = fieldMeshInstance.Mesh;
			var boundingBox = mesh.GetAabb();

			// Deklaracja zmiennych lokalnych
			Vector3 bRCL = boundingBox.Position + boundingBox.Size;
			Vector3 bRCG = fieldMeshInstance.ToGlobal(bRCL);

			// Wyczyść poprzednie pozycje, jeśli to konieczne
			Positions.Clear();

			if (FieldId == 0)
			{
				Positions.Add(new Vector3(bRCG.X - 3.5f, 0.5f, bRCG.Z - 2.5f));
				Positions.Add(new Vector3(bRCG.X - 3.5f, 0.5f, bRCG.Z - 1.5f));
				Positions.Add(new Vector3(bRCG.X - 2.5f, 0.5f, bRCG.Z - 1.5f));
				Positions.Add(new Vector3(bRCG.X - 3.5f, 0.5f, bRCG.Z - 0.5f));
				Positions.Add(new Vector3(bRCG.X - 2.5f, 0.5f, bRCG.Z - 0.5f));
				Positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z - 0.5f));
			}
			else if (FieldId == 10)
			{
				Positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z - 3.5f));
				Positions.Add(new Vector3(bRCG.X + 2.5f, 0.5f, bRCG.Z - 3.5f));
				Positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z - 3.5f));
				Positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z - 3.5f));
				Positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z - 2.5f));
				Positions.Add(new Vector3(bRCG.X + 0.5f, 0.5f, bRCG.Z - 1.5f));
			}
			else if (FieldId == 20)
			{
				Positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z + 3.5f));
				Positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z + 2.5f));
				Positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z + 1.5f));
				Positions.Add(new Vector3(bRCG.X + 3.5f, 0.5f, bRCG.Z + 0.5f));
				Positions.Add(new Vector3(bRCG.X + 2.5f, 0.5f, bRCG.Z + 0.5f));
				Positions.Add(new Vector3(bRCG.X + 1.5f, 0.5f, bRCG.Z + 0.5f));
			}
			else if (FieldId == 30)
			{
				Positions.Add(new Vector3(bRCG.X - 3.5f, 0.5f, bRCG.Z + 3.5f));
				Positions.Add(new Vector3(bRCG.X - 2.5f, 0.5f, bRCG.Z + 3.5f));
				Positions.Add(new Vector3(bRCG.X - 1.5f, 0.5f, bRCG.Z + 3.5f));
				Positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z + 3.5f));
				Positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z + 2.5f));
				Positions.Add(new Vector3(bRCG.X - 0.5f, 0.5f, bRCG.Z + 1.5f));
			}
		}
	}
}
