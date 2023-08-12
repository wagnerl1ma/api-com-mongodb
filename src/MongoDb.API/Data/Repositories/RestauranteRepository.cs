using MongoDb.API.Data.Mappings;
using MongoDb.API.Data.ValueObjects;
using MongoDb.API.Domain.Enums;
using MongoDb.API.Domain.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDb.API.Data.Repositories
{
    public class RestauranteRepository
    {
        IMongoCollection<RestauranteMapping> _restaurantes;
        IMongoCollection<AvaliacaoMapping> _avaliacoes;

        public RestauranteRepository(MongoDbContext mongoDB)
        {
            _restaurantes = mongoDB.DbContext.GetCollection<RestauranteMapping>("restaurantes");
            _avaliacoes = mongoDB.DbContext.GetCollection<AvaliacaoMapping>("avaliacoes");
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

        public bool AlterarCompleto(Restaurante restaurante)
        {
            var document = new RestauranteMapping
            {
                Id = restaurante.Id,
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

            var resultado = _restaurantes.ReplaceOne(x => x.Id == document.Id, document);

            return resultado.ModifiedCount > 0;
        }

        public bool AlterarCozinha(string id, CozinhaEnum cozinha)
        {
            var atualizacao = Builders<RestauranteMapping>.Update.Set(x => x.Cozinha, cozinha); // usando o Set para alterar somente um campo

            var resultado = _restaurantes.UpdateOne(x => x.Id == id, atualizacao);

            return resultado.ModifiedCount > 0;
        }

        public IEnumerable<Restaurante> ObterPorNome(string nome)
        {
            var restaurantes = new List<Restaurante>();

            _restaurantes.AsQueryable()
                .Where(x => x.Nome.ToLower().Contains(nome.ToLower()))
                .ToList()
                .ForEach(d => restaurantes.Add(d.ConverterParaDomain()));

            return restaurantes;
        }

        public void Avaliar(string restauranteId, Avaliacao avaliacao)
        {
            var document = new AvaliacaoMapping
            {
                RestauranteId = restauranteId,
                Estrelas = avaliacao.Estrelas,
                Comentario = avaliacao.Comentario
            };

            _avaliacoes.InsertOne(document);
        }

        public async Task<Dictionary<Restaurante, double>> ObterTop3()
        {
            var retorno = new Dictionary<Restaurante, double>();

            var top3 = _avaliacoes.Aggregate()
                .Group(_ => _.RestauranteId, g => new { RestauranteId = g.Key, MediaEstrelas = g.Average(a => a.Estrelas) }) // agrupando por RestauranteId e retonar a médida de estrelas (MediaEstrelas)
                .SortByDescending(_ => _.MediaEstrelas) // Ordenar pela MediaEstrelas decrescente
                .Limit(3); // pega os 3 primeiros


            // traz os restaurantes junto com as avaliacoes
            await top3.ForEachAsync(x =>
            {
                var restaurante = ObterPorId(x.RestauranteId);

                _avaliacoes.AsQueryable()
                    .Where(a => a.RestauranteId == x.RestauranteId)
                    .ToList()
                    .ForEach(a => restaurante.InserirAvaliacao(a.ConverterParaDomain()));

                retorno.Add(restaurante, x.MediaEstrelas);
            });

            return retorno;
        }

        public Dictionary<Restaurante, double> ObterTop3ComLookup() // com lookup as consultas sao mais rápidas
        {
            var retorno = new Dictionary<Restaurante, double>();

            var top3 = _avaliacoes.Aggregate()
            .Group(_ => _.RestauranteId, g => new { RestauranteId = g.Key, MediaEstrelas = g.Average(a => a.Estrelas) })
            .SortByDescending(_ => _.MediaEstrelas) // Ordenar pela MediaEstrelas decrescente
            .Limit(3) // pega os 3 primeiros
            .Lookup<RestauranteMapping, RestauranteAvaliacaoMapping>("restaurantes", "RestauranteId", "Id", "Restaurante")
            .Lookup<AvaliacaoMapping, RestauranteAvaliacaoMapping>("avaliacoes", "Id", "RestauranteId", "Avaliacoes");

            foreach (var top in top3.ToList())
            {
                if (!top.Restaurante.Any())
                    return retorno;

                var restaurante = new Restaurante(top.Id, top.Restaurante[0].Nome, top.Restaurante[0].Cozinha);

                var endereco = new Endereco(
                    top.Restaurante[0].Endereco.Logradouro,
                    top.Restaurante[0].Endereco.Numero,
                    top.Restaurante[0].Endereco.Cidade,
                    top.Restaurante[0].Endereco.UF,
                    top.Restaurante[0].Endereco.Cep);

                restaurante.AtribuirEndereco(endereco);

                top.Avaliacoes.ForEach(a => restaurante.InserirAvaliacao(a.ConverterParaDomain()));
                retorno.Add(restaurante, top.MediaEstrelas);
            }

            return retorno;
        }

        public (long, long) Remover(string restauranteId) // retornando 2 parametros com tuplas
        {
            var resultadoAvaliacoes = _avaliacoes.DeleteMany(_ => _.RestauranteId == restauranteId); //deletando avaliacoes
            var resultadoRestaurante = _restaurantes.DeleteOne(_ => _.Id == restauranteId); //deletando restaurante

            return (resultadoRestaurante.DeletedCount, resultadoAvaliacoes.DeletedCount); // retornando os resultados do delete de restaurante e avaliacoes
        }

        public async Task<IEnumerable<Restaurante>> ObterPorBuscaTextual(string texto)
        {
            var restaurantes = new List<Restaurante>();

            var filter = Builders<RestauranteMapping>.Filter.Text(texto); //Text = $text 

            await _restaurantes
                .AsQueryable()
                .Where(x => filter.Inject())
                .ForEachAsync(d => restaurantes.Add(d.ConverterParaDomain()));

            return restaurantes;
        }
    }
}
