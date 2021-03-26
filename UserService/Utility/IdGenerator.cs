using AutoNumber;
using AutoNumber.Interfaces;

namespace API.Utility
{
    public class IdGenerator
    {
        private static IUniqueIdGenerator _generator;
        private readonly string connectionString = "DefaultEndpointsProtocol=https;AccountName=moneymoonapistorage;AccountKey=hUpCU11DlF4TpyzolNmWoNhr+2tzrloKefvTlIsgW9I0nrhX/85CxxwJ49U4lCg1C3Cm5zuKstT3jtcStXp7xw==;EndpointSuffix=core.windows.net";
        private IdGenerator()
        {
            var blobStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);

            var blobOptimisticDataStore = new BlobOptimisticDataStore(blobStorageAccount, "unique-ids");

            _generator = new UniqueIdGenerator(blobOptimisticDataStore);
        }

        public static string GenerateId(string scopeName)
        {
            if(_generator == null)
            {
                new IdGenerator();
            }

            return _generator.NextId(scopeName).ToString();
        }

    }
}
