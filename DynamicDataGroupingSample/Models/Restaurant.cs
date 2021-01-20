namespace DynamicDataGroupingSample
{
    public class Restaurant
    {
        public Restaurant(string name, string category, string type, string country)
        {
            Id = name;
            Name = name;
            Category = category;
            Type = type;
            Country = country;
        }

       public string Id { get; }
       public string Name { get; }
       public string Category { get; }
       public string Type { get; }
       public string Country { get; }
    }
}
