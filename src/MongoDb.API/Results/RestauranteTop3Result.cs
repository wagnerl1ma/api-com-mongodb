namespace MongoDb.API.Results
{
    public class RestauranteTop3Result
    {
        public string? Id { get; set; }
        public string? Nome { get; set; }
        public int Cozinha { get; set; }
        public string? Cidade { get; set; }
        public double Estrelas { get; set; }
    }
}
