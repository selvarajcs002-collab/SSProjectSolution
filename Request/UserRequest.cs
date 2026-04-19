namespace SSProjectSolution.Request
{
    public class UserRequest
    {
        public string Mode { get; set; } = string.Empty; // 'INSERT' or 'UPDATE'
        public int? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
