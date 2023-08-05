using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDb.API.Data.ValueObjects;

namespace MongoDb.API.Data.Mappings
{
    public class AvaliacaoMapping
    {
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? RestauranteId { get; set; }
        public int Estrelas { get; set; }
        public string? Comentario { get; set; }
    }

    public static class AvaliacaoSchemaExtensao
    {
        public static Avaliacao ConverterParaDomain(this AvaliacaoMapping document)
        {
            return new Avaliacao(document.Estrelas, document.Comentario);
        }
    }
}
