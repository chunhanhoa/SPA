namespace QL_Spa.Models
{
    public class Chair
    {
        public int ChairId { get; set; }
        public string ChairName { get; set; }
        public bool IsAvailable { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
    }
}