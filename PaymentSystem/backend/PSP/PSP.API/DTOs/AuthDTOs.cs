namespace PSP.API.DTOs
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponse
    {
        public required string Token { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Role { get; set; }
    }
}
