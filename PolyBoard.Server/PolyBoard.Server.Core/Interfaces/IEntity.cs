namespace PolyBoard.Server.Core.Interfaces
{
    //jezeli jakas klasa implmentuje ten interfejs, to jest encją zapisywaną w bazie danych
    // 9 encji tako od buta jeszcze skins i skills to juz 11, jeszcze do N:N relacji i juz z 15
    //tylko nwm czy trzeba bedzie jakos to normalizowac z tym command patternem? ale wydaje sie to bez sensu wiec praise the lord ze nie
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}
