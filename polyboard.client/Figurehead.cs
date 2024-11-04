using Godot;
using System;

public partial class Figurehead : CharacterBody3D
{
	private int currentPosition = 0; // Aktualna pozycja pionka
	private const int TotalFields = 40; // Liczba pól na planszy

	// Możesz dodać tu listę pozycji dla 40 pól
	private Vector3[] fieldPositions = new Vector3[TotalFields];

	public override void _Ready()
	{
		// Zainicjalizuj pozycje pól
		InitializeFieldPositions();
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_accept")) // "ui_accept" odpowiada domyślnie spacji
		{
			RollAndMove();
		}
	}

	private void RollAndMove()
	{
		Random random = new Random();
		int rolledValue = random.Next(1, 7); // Losuje wartość od 1 do 6
		GD.Print($"Rolled: {rolledValue}");

		MovePiece(rolledValue);
	}

	private void MovePiece(int rolledValue)
	{
		// Oblicz nową pozycję
		currentPosition += rolledValue;

		// Jeśli przekroczyliśmy liczbę pól, wracamy do początku (cyklicznie)
		if (currentPosition >= TotalFields) // Zmienna 40 powinna być zgodna z liczbą pól na planszy
		{
			currentPosition -= TotalFields; // Wracamy do początku
		}

		// Oblicz nową pozycję pionka
		Vector3 targetPosition = CalculateTargetPosition(currentPosition);

		// Ustaw pozycję pionka
		GlobalPosition = targetPosition; // Zakładając, że pionek ma właściwość GlobalPosition
	}

	private Vector3 CalculateTargetPosition(int position)
	{
		// Zwróć odpowiednią pozycję dla danego pola
		return fieldPositions[position];
	}

	private void InitializeFieldPositions()
	{
		// Ustawienia pozycji pól (możesz je dostosować według swoich potrzeb)
		float fieldWidth = 2.5f; // Przykładowa szerokość pola
		float fieldHeight = 1f; // Wysokość planszy
		float boardSize = 26f; // Rozmiar planszy

		// Ustawienia pozycji dla 40 pól
		for (int i = 0; i < TotalFields; i++)
		{
			// Przykładowe rozmieszczenie pól
			float x = (i < 10) ? -boardSize / 2 + fieldWidth * (i + 0.5f) : // Górna krawędź
					   (i < 20) ? boardSize / 2 : // Prawa krawędź
					   (i < 30) ? boardSize / 2 - fieldWidth * (i - 19.5f) : // Dolna krawędź
								   -boardSize / 2; // Lewa krawędź

			float z = (i < 10) ? boardSize / 2 : // Górna krawędź
					   (i < 20) ? boardSize / 2 - fieldWidth * (i - 9.5f) : // Prawa krawędź
					   (i < 30) ? -boardSize / 2 : // Dolna krawędź
								   -boardSize / 2 + fieldWidth * (i - 29.5f); // Lewa krawędź

			fieldPositions[i] = new Vector3(x, fieldHeight, z);
		}
	}
}
