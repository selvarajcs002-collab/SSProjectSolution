namespace SSProjectSolution.Request
{
    public class UserSaveRequest
    {
        public string Mode { get; set; } = "INSERT";
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
