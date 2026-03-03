namespace QuestFlag.Demo.WebApp.Client.State;

public class UserInfo
{
    public string? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string[] Roles { get; set; } = System.Array.Empty<string>();
}
