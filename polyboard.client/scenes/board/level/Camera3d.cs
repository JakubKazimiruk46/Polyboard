using Godot;

public partial class Camera3d : Camera3D
{
	private Vector3 boardCenter = new Vector3(0, 0, 0); // Center of the board
	private Vector2 lastMousePosition;
	private bool isMiddleButtonPressed = false;
	
	private float rotationSpeed = 0.005f; // Smaller value for smooth control
	private float rotationLerpSpeed = 10.0f; // Smoothing speed

	private float currentYaw = 0f;   // Horizontal rotation (Y axis)
	private float targetYaw = 0f;

	private float cameraDistance = 10f; // Distance of the camera from the board

	public override void _Ready()
	{
		// Initialize the camera's yaw from the current rotation
		Transform3D transform = GlobalTransform;
		Vector3 euler = transform.Basis.GetEuler();
		currentYaw = targetYaw = euler.Y;

		// Calculate the camera's distance to the board center
		cameraDistance = GlobalTransform.Origin.DistanceTo(boardCenter);
	}

	public override void _Process(double delta)
	{
		HandleMouseInput();
		SmoothlyUpdateCameraPosition((float)delta);
	}

	private void HandleMouseInput()
	{
		if (Input.IsMouseButtonPressed(MouseButton.Middle))
		{
			if (!isMiddleButtonPressed)
			{
				// When the middle mouse button is pressed, start tracking
				isMiddleButtonPressed = true;
				lastMousePosition = GetViewport().GetMousePosition();
			}

			// Calculate mouse movement (delta)
			Vector2 currentMousePosition = GetViewport().GetMousePosition();
			Vector2 mouseDelta = currentMousePosition - lastMousePosition;

			if (mouseDelta != Vector2.Zero)
			{
				// Adjust the target yaw based on horizontal mouse movement
				targetYaw -= mouseDelta.X * rotationSpeed;

				lastMousePosition = currentMousePosition;
			}
		}
		else
		{
			isMiddleButtonPressed = false;
		}
	}

	private void SmoothlyUpdateCameraPosition(float delta)
	{
		// Smoothly interpolate current yaw towards target yaw
		currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, delta * rotationLerpSpeed);

		// Calculate the camera's new position based on yaw
		Vector3 offset = new Vector3(
			Mathf.Sin(currentYaw) * cameraDistance,
			GlobalTransform.Origin.Y, // Maintain the current vertical position
			Mathf.Cos(currentYaw) * cameraDistance
		);

		Vector3 newCameraPosition = boardCenter + offset;

		// Update the camera's transform to reflect the new position and look at the board center
		Transform3D transform = GlobalTransform;
		transform.Origin = newCameraPosition;
		transform = transform.LookingAt(boardCenter, Vector3.Up);
		GlobalTransform = transform;
	}
}
