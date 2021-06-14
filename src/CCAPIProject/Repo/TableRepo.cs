using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
//using CCAPIProject.Models;
using CCAPIProject.Dtos;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json.Linq;

namespace CCAPIProject.Repo
{
    public class TableRepo : ITableRepo
    {
        private readonly IAmazonDynamoDB dynamoDB;
        public TableRepo(IAmazonDynamoDB dynamoDB)
        {
            this.dynamoDB =dynamoDB;
        }
        public async Task<Models.Table[]> GetTablesAsync()
        {
            var res = await dynamoDB.ScanAsync(new ScanRequest
            {
                TableName = "Table"
            });
            if(res != null && res.Items != null)
            {
                List<Models.Table> tbs = new List<Models.Table>();
                foreach(var item in res.Items)
                {
                    var tb = new Models.Table{tableName=item["tableName"].S};
                    try
                    {
                    
                        foreach (var name in item["attr"].M)
                        {
                            var a = new Models.Attribute{};
                            a.attrName = name.Key.ToString();
                            a.type = name.Value.M["type"].S.ToString();
                            a.key = name.Value.M["key"].S.ToString();
                            
                            if(a.key == "HashKey")
                            {
                                tb.hashKey = a.attrName;
                            }
                            else if(a.key == "RangeKey")
                            {
                                tb.rangeKey = a.attrName;
                            }
                        tb.attr.Add(a);
                        }
                    }catch{
                        tbs.Add(tb);
                        continue;
                    }
                    tbs.Add(tb);
                }
                return tbs.ToArray();

            }
            
            return Array.Empty<Models.Table>();
        }



        public async Task<bool> CreateTableAsync(CreateTableDto tableDto)
        {
            var request = new CreateTableRequest
            {
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = tableDto.partitionKey,
                        AttributeType = tableDto.partitionKeyType
                    } 
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = tableDto.partitionKey,
                        KeyType = "Hash"    //partition key
                    }  
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = tableDto.readCapacityUnits,
                    WriteCapacityUnits = tableDto.writeCapacityUnits
                },
                TableName = tableDto.tableName
            };
    //sortkey existed
            if(tableDto.sortKey != null)
            {
                request.AttributeDefinitions.Add( 
                    new AttributeDefinition
                    {
                        AttributeName = tableDto.sortKey,
                        AttributeType = tableDto.sortKeyType
                    });
                request.KeySchema.Add(
                    new KeySchemaElement
                    {
                        AttributeName = tableDto.sortKey,
                        KeyType = "Range"   //Sort key
                    });        
            }

           
            var response = await dynamoDB.CreateTableAsync(request);
            if(response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                 await Task.Run(()=>{

                    Table table = Table.LoadTable(dynamoDB, "Table");

                    var itm = new Document();
                    itm["tableName"] = tableDto.tableName;

                    var attr = new Document();
                    Dictionary<string, DynamoDBEntry> pKey= new Dictionary<string, DynamoDBEntry>();
                    pKey.Add("key", (DynamoDBEntry)"HashKey");
                    pKey.Add("type", (DynamoDBEntry)tableDto.partitionKeyType);
                    var docpKey = new Document(pKey);
                    attr[tableDto.partitionKey] = docpKey;
                    if(tableDto.sortKey != null){
                         Dictionary<string, DynamoDBEntry> rKey = new Dictionary<string, DynamoDBEntry>();
                        rKey.Add("key", (DynamoDBEntry)"RangeKey");
                        rKey.Add("type", (DynamoDBEntry)tableDto.sortKeyType);
                        var docrKey = new Document(rKey);
                        attr[tableDto.sortKey] = docrKey;
                    }
                    itm["attr"] = attr;
                         
                    table.PutItemAsync(itm);          
                });
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }  
    
    

        public async Task<bool> RemoveTableAsync(string tableName)
        {
            var itemRepo = new ItemRepo(new AmazonDynamoDBClient());
            var request = new DeleteTableRequest
            {
                TableName = tableName
            };

            var response = await dynamoDB.DeleteTableAsync(request);
            
            if(response.HttpStatusCode == System.Net.HttpStatusCode.OK){
                try{
                     await itemRepo.RemoveItem("Table", tableName, null);
                    return await Task.FromResult(true);
                }catch{}
            }
            return await Task.FromResult(false);
        }
    

        public async Task<Document> GetTableAttr(string tableName)
        {
            Table table = Table.LoadTable(dynamoDB,"Table");
            GetItemOperationConfig config = new GetItemOperationConfig
            {
                AttributesToGet = new List<string> {"attr"},
                ConsistentRead = true
            };
            Document doc =  await table.GetItemAsync(tableName, config);
            
            return doc;
        }
    
        public async Task UpdateAttributeAndType(string tableName, JObject data)
        {
            Document attr = new Document();

            foreach(var val in data["attr"])
            {
                Document TnK = new Document();
                TnK["type"] = val["type"].ToString();
                TnK["key"] = val["key"].ToString();
                attr[val["attrName"].ToString()]  = TnK;

            }

            Table table = Table.LoadTable(dynamoDB,"Table");
            var TableAttr = new Document();
            TableAttr["tableName"] = tableName;
            TableAttr["attr"] = attr;

            await table.PutItemAsync(TableAttr);
        }
    
    }
}