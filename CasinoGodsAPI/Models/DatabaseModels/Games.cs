namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class Games
    {
        public string Name { get; set; }
        public ICollection<Tables> Tables { get; set; }
        public Games()
        {
            Tables = new HashSet<Tables>();
        }
    }
}
