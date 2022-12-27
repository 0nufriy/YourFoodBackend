namespace YourFoodBackend.Model
{
    public class PassInfo
    {
        public int PassId { get; set; }
        public int Price { get; set; }
        public string passName { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public List<FoodInfo> foodInfo { get; set; }

    }
}
