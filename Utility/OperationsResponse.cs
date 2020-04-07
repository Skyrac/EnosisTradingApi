using System;

namespace API.Utility
{
    public class OperationsResponse<T>
    {
        public T Item { get; set; }
        public bool IsSuccessful { get; set; }

        public double RequestUnitsConsumed { get; set; }

        public Exception CosmosException { get; set; }
    }
}
