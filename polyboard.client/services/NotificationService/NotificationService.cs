using Godot;
using System;

public partial class NotificationService : Node
{
	public enum NotificationType
	{
		Normal,
		Error,
		Achievement,
		Progress
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
			else if (type == NotificationType.Error)
				notifications.Call("show_error", message, duration);
			else if (type == NotificationType.Achievement)
				notifications.Call("show_achievement", message, duration);
			else
				notifications.Call("show_progress", message, duration);
		}
		else
		{
			GD.PrintErr("NotificationLayer singleton not found. Make sure it's added as an Autoload.");
			if (type == NotificationType.Error)
				GD.PrintErr(message);
		}
	}
	public void ShowAchievement(string achievementName)
	{
		string message = $"Osiągnięcie zdobyte: {achievementName}";
		ShowNotification(message, NotificationType.Achievement, 4.0f);
	}

	public void ShowProgress(string achievementName, float percent)
	{
		string message = $"Postęp: {achievementName} - {Mathf.FloorToInt(percent)}%";
		ShowNotification(message, NotificationType.Progress, 2.5f);
	}
}
