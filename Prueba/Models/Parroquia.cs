namespace Prueba.Models
{
    public partial class Parroquia
    {
        public int IdParroquia { get; set; }
        public int IdMunicipio { get; set; }
        public string Parroquia1 { get; set; } = null!;
        public short Urbana { get; set; }
    }
}
