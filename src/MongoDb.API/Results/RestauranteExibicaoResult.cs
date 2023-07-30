namespace MongoDb.API.Results
{
    public class RestauranteExibicaoResult
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public int Cozinha { get; set; }
        public EnderecoExibicaoResult Endereco { get; set; }
    }
}
