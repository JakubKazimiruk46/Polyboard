using Godot;
using System;
using System.Threading.Tasks; // Dodaj to, aby korzystać z Task

public partial class Figurehead : CharacterBody3D
{
	private int currentPosition = 0; // Aktualna pozycja pionka
	private const int TotalFields = 40; // Liczba pól na planszy

	// Lista pozycji dla 40 pól
	private Vector3[] fieldPositions = new Vector3[TotalFields];

	[Export]
	public NodePath dieNodePath;

	public override void _Ready()
	{
		// Zainicjalizuj pozycje pól
		InitializeFieldPositions();

		// Spróbuj pobrać referencję do obiektu kostki
		Node dieNode = GetNodeOrNull(dieNodePath);

		if (dieNode == null)
		{
			GD.PrintErr($"Błąd: Nie znaleziono obiektu kostki pod ścieżką '{dieNodePath}'. Upewnij się, że dieNodePath jest ustawiony poprawnie.");
			return;
		}

		// Połącz sygnał roll_finished z metodą OnDieRollFinished
		dieNode.Connect("roll_finished", new Callable(this, nameof(OnDieRollFinished)));
	}

	private void OnDieRollFinished(int value)
	{
		GD.Print($"Wylosowano: {value}");
		MovePiece(value);
	}

	private async void MovePiece(int rolledValue)
	{
		int stepsToMove = rolledValue;

		while (stepsToMove > 0)
		{
			int nextPosition = currentPosition + 1;

			// Jeśli przekroczyliśmy liczbę pól, wracamy do początku
			if (nextPosition >= TotalFields)
			{
				nextPosition = 0;
			}

			Vector3 endPosition = CalculateTargetPosition(nextPosition);

			// Animuj ruch do następnego pola
			await MoveToPosition(endPosition);

			currentPosition = nextPosition;
			stepsToMove--;
		}
	}

	private async Task MoveToPosition(Vector3 endPosition)
	{
		// Utwórz Tween do animacji ruchu
		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", endPosition, 0.5f)
			 .SetTrans(Tween.TransitionType.Linear)
			 .SetEase(Tween.EaseType.InOut);

		// Poczekaj, aż Tween zakończy animację
		await ToSignal(tween, "finished");
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
			float x = (i < 10) ? -boardSize / 2 + fieldWidth * (i + 0.5f) :
					   (i < 20) ? boardSize / 2 :
					   (i < 30) ? boardSize / 2 - fieldWidth * (i - 19.5f) :
								   -boardSize / 2;

			float z = (i < 10) ? boardSize / 2 :
					   (i < 20) ? boardSize / 2 - fieldWidth * (i - 9.5f) :
					   (i < 30) ? -boardSize / 2 :
								   -boardSize / 2 + fieldWidth * (i - 29.5f);

			fieldPositions[i] = new Vector3(x, fieldHeight, z);
		}
	}
}
