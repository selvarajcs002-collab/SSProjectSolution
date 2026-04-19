namespace SSProjectSolution.Request
{
    public class UserUpdateRequest
    {
        public string Mode { get; set; } = "UPDATE";
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
