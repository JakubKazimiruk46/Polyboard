using Godot;
using System;
using System.Collections.Generic;

namespace DTO{
	public partial class LobbyDetailsDTO : Node
	{
		public Guid Id { get; set; }
		public string LobbyName { get; set; }
		public string Status { get; set; }
		public List<LobbyUserDTO> ConnectedUsers { get; set; }
	}
}
