namespace MongoDb.API.Data.Enums
{
    public enum CozinhaEnum
    {
        Brasileira = 1,
        Italiana = 2,
        Arabe = 3,
        Japonesa = 4,
        FastFood = 5
    }

    public static class ECozinhaHelper
    {
        public static CozinhaEnum ConverterDeInteiro(int valor)
        {
            if (Enum.TryParse(valor.ToString(), out CozinhaEnum cozinha))
                return cozinha;

            throw new ArgumentOutOfRangeException("cozinha");
        }
    }
}
