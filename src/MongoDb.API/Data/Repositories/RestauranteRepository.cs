using MongoDb.API.Data.Mappings;
using MongoDb.API.Domain.Models;
using MongoDB.Driver;

namespace MongoDb.API.Data.Repositories
{
    public class RestauranteRepository
    {
        IMongoCollection<RestauranteMapping> _restaurantes;

        public RestauranteRepository(MongoDbContext mongoDB)
        {
            _restaurantes = mongoDB.DbContext.GetCollection<RestauranteMapping>("restaurantes");
        }

        public void Inserir(Restaurante restaurante)
        {
            var document = new RestauranteMapping
            {
                Nome = restaurante.Nome,
                Cozinha = restaurante.Cozinha,
                Endereco = new EnderecoMapping
                {
                    Logradouro = restaurante.Endereco.Logradouro,
                    Numero = restaurante.Endereco.Numero,
                    Cidade = restaurante.Endereco.Cidade,
                    Cep = restaurante.Endereco.Cep,
                    UF = restaurante.Endereco.UF
                }
            };

            _restaurantes.InsertOne(document);
        }
    }
}
