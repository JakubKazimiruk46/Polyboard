using Microsoft.AspNetCore.SignalR;
using PolyBoard.Server.Core.Entities;
using System.Collections.Concurrent;

namespace PolyBoard.Server.Presentation.Hubs
{
    public class LobbyHub : Hub
    {

        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>(); // ConnectionId, Username
        private static ConcurrentDictionary<string, List<UserConnection>> Lobbies = new ConcurrentDictionary<string, List<UserConnection>>();
        private static ConcurrentDictionary<string, string> RoomAdmins = new ConcurrentDictionary<string, string>(); // Stores the admin for each room, GameRoom, Usermname

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined the server.");
        }

        public async Task JoinSpecificGameRoom(UserConnection conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.GameRoom);

            if (ConnectedUsers.ContainsKey(Context.ConnectionId))
                ConnectedUsers.TryAdd(Context.ConnectionId, conn.Username);

            // Add user to the lobby
            Lobbies.AddOrUpdate(conn.GameRoom,
                new List<UserConnection> { conn }, 
                (key, users) =>
                {
                    if (!users.Any(u => u.Username == conn.Username))
                        users.Add(conn); m
                    return users;
                });

            if (!RoomAdmins.ContainsKey(conn.GameRoom))
            {
                RoomAdmins[conn.GameRoom] = conn.Username;
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "admin", "You are now the admin of this lobby.");
            }

            await Clients.Group(conn.GameRoom).SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined {conn.GameRoom}");
        }

        // Method for admins to remove a specific user from a room
        public async Task RemoveUserFromLobby(string adminUsername, string usernameToRemove, string gameRoom)
        {
            if (RoomAdmins.TryGetValue(gameRoom, out string admin) && admin == adminUsername)
            {
                // Find the connection ID of the user to remove
                var userToRemove = Lobbies[gameRoom]?.FirstOrDefault(u => u.Username == usernameToRemove);

                if (userToRemove != null)
                {
                    // Remove the user from the SignalR group and the local lobby list
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameRoom);
                    Lobbies[gameRoom].Remove(userToRemove);

                    // Notify the group about the user removal
                    await Clients.Group(gameRoom).SendAsync("ReceiveMessage", "admin", $"{usernameToRemove} has been removed by the admin.");

                    // If the lobby is empty, clean up
                    if (!Lobbies[gameRoom].Any())
                    {
                        Lobbies.TryRemove(gameRoom, out _);
                        RoomAdmins.TryRemove(gameRoom, out _);
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", "error", "User not found in the lobby.");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "error", "Only the admin can remove users from the lobby.");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove the user from all rooms they belong to and update lobbies
            foreach (var room in Lobbies)
            {
                var searchedUser = ConnectedUsers.FirstOrDefault(u => u.Key == Context.ConnectionId);
                var user = room.Value.FirstOrDefault(u => u.Username == searchedUser.Value);
                if (user != null)
                {
                    room.Value.Remove(user);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Key);

                    // Notify remaining users in the group
                    await Clients.Group(room.Key).SendAsync("ReceiveMessage", "admin", $"{user.Username} has left {room.Key}");

                    // If the user was an admin and the room is not empty, reassign admin
                    if (RoomAdmins.TryGetValue(room.Key, out var currentAdmin) && currentAdmin == user.Username)
                    {
                        if (room.Value.Any())
                        {
                            // Assign a new admin
                            var newAdmin = room.Value.First();
                            RoomAdmins[room.Key] = newAdmin.Username;
                            await Clients.Group(room.Key).SendAsync("ReceiveMessage", "admin", $"{newAdmin.Username} is now the admin of {room.Key}");
                        }
                        else
                        {
                            // Clean up empty rooms
                            RoomAdmins.TryRemove(room.Key, out _);
                            Lobbies.TryRemove(room.Key, out _);
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}