namespace CasinoGodsAPI.DTOs
{
    public class GamesListDTO   //DTO DO PRZESYLANIA INFO O DOSTEPNYCH GRACH
    {
        public string jwt { get; set; } = string.Empty;
        public List<string> gameNames { get; set; }
    }
}
