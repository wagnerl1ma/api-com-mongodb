using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDb.API.Domain.Enums;
using MongoDb.API.Data.ValueObjects;
using MongoDb.API.Domain.Models;

namespace MongoDb.API.Data.Mappings
{
    public class RestauranteMapping
    {
        [BsonRepresentation(BsonType.ObjectId)] // anotacao para incluir o id automatico no mongoDb
        public string Id { get; set; }
        public string Nome { get; set; }
        public CozinhaEnum Cozinha { get; set; }
        public EnderecoMapping Endereco { get; set; }
    }

    public static class RestauranteMappingExtensao
    {
        public static Restaurante ConverterParaDomain(this RestauranteMapping document)
        {
            var restaurante = new Restaurante(document.Id, document.Nome, document.Cozinha);
            var endereco = new Endereco(document.Endereco.Logradouro, document.Endereco.Numero, document.Endereco.Cidade, document.Endereco.UF, document.Endereco.Cep);
            restaurante.AtribuirEndereco(endereco);

            return restaurante;
        }
    }
}
