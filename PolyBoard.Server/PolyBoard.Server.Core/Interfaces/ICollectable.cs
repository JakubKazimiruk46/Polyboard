using PolyBoard.Server.Core.Helpers;

namespace PolyBoard.Server.Core.Interfaces
{
    // jezeli jakas klasa implementuje ten interfejs to tego typu obiekt może być posiadany przez gracza (chociaż nie musi)
    public interface ICollectable
    {
        Player? Owner { get; set; }
    }
}
