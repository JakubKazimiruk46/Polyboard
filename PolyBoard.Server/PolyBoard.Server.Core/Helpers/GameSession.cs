using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Core.Helpers
{
    //To też obiekt pomocniczy zarządzający daną sesją, narazie to szkic na 100% się ta klasa rozrośnie
    // ewentualnie jak bedzie czas wydzieli się to do jakiegoś distributed cache w redisie
    public sealed class GameSession
    {
        public Guid Id { get; init; } // jakieś Id połączenia tu może, to technicznie trzeba sie dowiedziec jak dokładnie powinno to dzialac
        public int CurrentTurn { get; set; } // Numer aktualnej tury
        public ICollection<Player> Players { get; set; } // Lista graczy
        public Map Map { get; set; }
        public ICollection<Turn> Turns { get; set; } // Lista wszystkich wydarzeń w grze
        public Player CurrentPlayer { get; set; } // Nawigacja do aktualnego gracza

        // Metoda do rozpoczęcia nowej gry
        public void StartGame(List<Player> players, Map map)
        {
            Players = players;
            Map = map;
            CurrentTurn = 1;
        }

        // Metoda do zakończenia gry
        public void EndGame(Player winner)
        {
        }
    }


}
