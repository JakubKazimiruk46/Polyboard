using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.DTO;

public class UserAchievementDTO
{
    public AchievementDetailsDTO Achievement { get; set; }
    public decimal Progress { get; set; }
}