using System.ComponentModel.DataAnnotations;

namespace CasinoGodsAPI.Models
{
    public class Dealer
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public float profit { get; set; } = 0;
        public bool active { get; set; } = false;
        public static int freeDealers { get; set; } = -10;

    }
}
