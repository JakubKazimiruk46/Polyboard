using Godot;
using System;

namespace Services {
	public partial class DataTransferService : Node
	{
		private static DataTransferService _instance;

		public static DataTransferService Instance => _instance;

		public string CurrentLobbyId { get; set; }

		public override void _Ready()
		{
			if (_instance == null)
			{
				_instance = this;
				GD.Print("DataTransferService initialized.");
			}
			else
			{
				QueueFree();
			}
		}
	}

}
