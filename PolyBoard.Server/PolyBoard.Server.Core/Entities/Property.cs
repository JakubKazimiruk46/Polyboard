using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class Property : IEntity
    {
        //Main table structure
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<RentPrice> Prices { get; set; } = [];

        //Secondary Relations
        public List<Transaction>? InvolvedInTransactions { get; set; }
    }
}
