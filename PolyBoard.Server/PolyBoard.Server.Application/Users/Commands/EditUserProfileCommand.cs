using MediatR;

public record EditUserProfileCommand{

    Guid UserId
    string? UserName = null,
    string? Email = null,
    string? NewPassword = null,
    string? CurrentPassword = null
}: IRequest<bool>