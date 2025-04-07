using Godot;
using System;

public partial class NotificationService : Node
{
	public enum NotificationType
	{
		Normal,
		Error
	}
	
	static private Node? GetNotifications()
	{
		return (Engine.GetMainLoop() as SceneTree)?.Root?.GetNode("/root/Notifications");
	}

	public void ShowNotification(string message, NotificationType type = NotificationType.Normal, float duration = 3f)
	{
		var notifications = GetNotifications();
		if (notifications != null)
		{
			if (type == NotificationType.Normal)
				notifications.Call("show_notification", message, duration);
			else
				notifications.Call("show_error", message, duration);
		}
		else
		{
			GD.PrintErr("NotificationLayer singleton not found. Make sure it's added as an Autoload.");
			if (type == NotificationType.Error)
				GD.PrintErr(message);
		}
	}
}
