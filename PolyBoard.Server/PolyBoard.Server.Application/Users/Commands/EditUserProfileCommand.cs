using MediatR;

public class EditUserProfileCommand : IRequest<bool>{

    public Guid UserId {get; set;}
    public string UserName {get; set;}
    public string Email {get; set;}
    public string NewPassword {get; set;}
    public string CurrentPassword { get; set; }

}