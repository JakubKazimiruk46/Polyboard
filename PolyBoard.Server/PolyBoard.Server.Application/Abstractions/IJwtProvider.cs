using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Abstractions;

public interface IJwtProvider
{
    string Generate(User user);
}