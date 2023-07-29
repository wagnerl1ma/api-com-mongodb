using MongoDb.API.Data.Mappings;
using MongoDb.API.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace MongoDb.API.Data
{
    public class MongoDbContext
    {
        public IMongoDatabase DbContext{ get; }

        public MongoDbContext(IConfiguration configuration)
        {
            try
            {
                var client = new MongoClient(configuration["ConnectionString"]);
                DbContext = client.GetDatabase(configuration["NomeBanco"]);
                MapClasses();
            }
            catch (Exception ex)
            {
                throw new MongoException("Não foi possivel se conectar ao MongoDB", ex);
            }
        }

        private void MapClasses()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(RestauranteMapping)))
            {
                BsonClassMap.RegisterClassMap<RestauranteMapping>(i =>
                {
                    i.AutoMap();
                    i.MapIdMember(c => c.Id);
                    i.MapMember(c => c.Cozinha).SetSerializer(new EnumSerializer<CozinhaEnum>(BsonType.Int32)); //Mapeamento para o Enum gravar no banco como Int32
                    i.SetIgnoreExtraElements(true); // se tiver algo no banco que nao está mapeado no código, irá ignorar e nao irá dar erro
                });
            }
        }
    }
}
