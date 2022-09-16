namespace Prueba.Models
{
    public class Usuario
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password => "Pass1234_";
    }
}
