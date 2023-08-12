using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoDb.API.Data.Mappings
{
    [BsonIgnoreExtraElements]
    public class RestauranteAvaliacaoMapping
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        public string Id { get; set; }
        public double MediaEstrelas { get; set; }
        public List<RestauranteMapping> Restaurante { get; set; }
        public List<AvaliacaoMapping> Avaliacoes { get; set; }
    }
}
