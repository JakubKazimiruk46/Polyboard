using Godot;
using System.Collections.Generic;
using System;
using System.Collections;


public partial class Board : StaticBody3D
{
	

	public List<Vector3> Fields = new List<Vector3>();  // Lista pozycji pól na obrzeżach
	public float FieldSize = 1.0f;  // Rozmiar standardowego pola
	public float CornerFieldSize;  // Rozmiar pola narożnego

	public override void _Ready()
	{
		CornerFieldSize = FieldSize * 2;  // Narożne pola są dwa razy większe
		GenerateFields();
	}

	// Tworzenie pól na obrzeżach planszy
	private void GenerateFields()
	{
		// Dolny rząd (od lewego narożnika do prawego)
		for (int x = 0; x < 11; x++)
		{
			float size = (x == 0 || x == 10) ? CornerFieldSize : FieldSize;  // Rogi
			Fields.Add(new Vector3(x * FieldSize, 0, 0));
		}

		// Prawa kolumna (od dołu do góry)
		for (int y = 1; y < 11; y++)
		{
			float size = (y == 10) ? CornerFieldSize : FieldSize;  // Górny prawy róg
			Fields.Add(new Vector3(10 * FieldSize, 0, y * FieldSize));
		}

		// Górny rząd (od prawej do lewej)
		for (int x = 9; x >= 0; x--)
		{
			float size = (x == 0) ? CornerFieldSize : FieldSize;  // Górny lewy róg
			Fields.Add(new Vector3(x * FieldSize, 0, 10 * FieldSize));
		}

		// Lewa kolumna (od góry do dołu)
		for (int y = 9; y > 0; y--)
		{
			Fields.Add(new Vector3(0, 0, 0.2f+y * FieldSize));
		}
	}

	// Pobieranie pozycji pola o podanym indeksie
	public Vector3 GetFieldPosition(int index)
	{
		return Fields[index % Fields.Count];
	}

	public int GetTotalFields()
	{
		return Fields.Count;
	}

	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
