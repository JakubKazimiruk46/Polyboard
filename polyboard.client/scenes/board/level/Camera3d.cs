using Godot;
using System;

public partial class Camera3d : Camera3D
{
	private Camera3D camera;
	private Vector3 boardCenter = new Vector3(0, 0, 0); 
	private Vector2 lastMousePosition;
	private float rotationSpeed = 0.1f;
	private bool isMiddleButtonPressed = false;
	private float currentRotation = 0f;  
	private float targetRotation = 0f;   
	private float rotationLerpSpeed = 5.0f;  

	private Vector3 initialCameraPosition; 
	private Vector3 initialCameraRotation; 

	private float cameraDistance = 10f; 

	public override void _Ready()
	{
		camera = GetNodeOrNull<Camera3D>("/root/Level/Camera3D");
		if (camera == null)
		{
			GD.PrintErr("Nie znaleziono kamery!");
		}

		
		initialCameraPosition = GlobalTransform.Origin;
		initialCameraRotation = GlobalTransform.Basis.GetEuler();
	
		targetRotation = initialCameraRotation.Y;

		cameraDistance = initialCameraPosition.DistanceTo(boardCenter);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Middle))
		{
			if (!isMiddleButtonPressed)
			{
				isMiddleButtonPressed = true;
				lastMousePosition = GetViewport().GetMousePosition();
			}

			Vector2 currentMousePosition = GetViewport().GetMousePosition();
			Vector2 mouseDelta = currentMousePosition - lastMousePosition;

			if (mouseDelta != Vector2.Zero)
			{
				targetRotation -= mouseDelta.X * rotationSpeed;

				lastMousePosition = currentMousePosition;
			}
		}
		else
		{
			isMiddleButtonPressed = false;
		}

		currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, (float)delta * rotationLerpSpeed);

		ApplySmoothRotation();
	}

	private void ApplySmoothRotation()
	{
		Vector3 offset = new Vector3(
			Mathf.Sin(currentRotation) * cameraDistance,
			initialCameraPosition.Y, 
			Mathf.Cos(currentRotation) * cameraDistance  
		);

		Vector3 newCameraPosition = boardCenter + offset;

		Transform3D transform = GlobalTransform;
		transform.Origin = newCameraPosition;
		transform = transform.LookingAt(boardCenter, Vector3.Up);
		GlobalTransform = transform;
	}
}
