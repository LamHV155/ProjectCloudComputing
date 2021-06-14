namespace CCAPIProject.Dtos
{
    public class CreateTableDto
    {
        public string tableName { get; set; }
        public string partitionKey { get; set; }
        public string partitionKeyType { get; set; }
        public string sortKey { get; set; }
        public string sortKeyType { get; set; }
        public int readCapacityUnits { get; set; }
        public int writeCapacityUnits { get; set; }
    }
}