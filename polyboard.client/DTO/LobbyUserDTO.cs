using Godot;
using System;
namespace DTO{
	public partial class LobbyUserDTO : Node
	{
		public Guid Id { get; set; }
		public string ConnectionId { get; set; }
		public string Username { get; set; }
		public bool IsReady { get; set; }
	}
}
