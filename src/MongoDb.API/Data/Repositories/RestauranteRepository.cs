using MongoDb.API.Data.Mappings;
using MongoDb.API.Data.ValueObjects;
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

        public async Task<IEnumerable<Restaurante>> ObterTodos()
        {
            var restaurantes = new List<Restaurante>();

            await _restaurantes.AsQueryable().ForEachAsync(d =>
            {
                var r = new Restaurante(d.Id.ToString(), d.Nome, d.Cozinha);
                var e = new Endereco(d.Endereco.Logradouro, d.Endereco.Numero, d.Endereco.Cidade, d.Endereco.UF, d.Endereco.Cep);
                r.AtribuirEndereco(e);
                restaurantes.Add(r);
            });

            return restaurantes;
        }

        public Restaurante ObterPorId(string id)
        {
            var document = _restaurantes.AsQueryable().FirstOrDefault(x => x.Id == id);

            if (document == null)
                return null;

            return document.ConverterParaDomain();
        }
    }
}
