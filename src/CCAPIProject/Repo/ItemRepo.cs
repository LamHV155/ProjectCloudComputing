using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;

namespace CCAPIProject.Repo
{
    public class ItemRepo : IItemRepo
    {
        private readonly IAmazonDynamoDB dynamoDB;
        public ItemRepo(IAmazonDynamoDB dynamoDB)
        {
            this.dynamoDB = dynamoDB;
        }


        public async Task AddNewItem(string tableName, JArray item)
        {
            var tableRepo = new TableRepo(new AmazonDynamoDBClient());
            Document tableAttr = await tableRepo.GetTableAttr(tableName);
            List<string> attribute = new List<string>{};
            List<string> type = new List<string>{};
            List<string> key = new List<string>{};

            if(tableAttr != null)
            {           
                var c = tableAttr["attr"].AsDocument().GetAttributeNames();
                foreach(var c1 in c)
                {
                    attribute.Add(c1);
                    var t = tableAttr["attr"].AsDocument()[c1].AsDocument()["type"];
                    var k = tableAttr["attr"].AsDocument()[c1].AsDocument()["key"];
                    type.Add(t);
                    key.Add(k);
                }                                        
            }
            List<Models.Attribute> attr = new List<Models.Attribute>{};
            for(int i = 0; i<attribute.Count; i++)
            {
                attr.Add(new Models.Attribute{attrName=attribute[i], type=type[i], key=key[i]});
            }
            Amazon.DynamoDBv2.DocumentModel.Table table = Amazon.DynamoDBv2.DocumentModel.Table.LoadTable(dynamoDB, tableName);
            var itm = new Document();
            bool flag = false;
            foreach(var value in item)
            {
                if(attribute.Contains(value["key"].ToString()) is true)
                {
                    switch (type[attribute.IndexOf(value["key"].ToString())])
                    {
                        case "N":
                            itm[value["key"].ToString()] = (float)value["value"];
                            break;
                        case "B":
                            byte[] byteArray = Encoding.ASCII.GetBytes(value["value"].ToString());
                            MemoryStream stream = new MemoryStream( byteArray );
                            itm[value["key"].ToString()] = stream;
                            break;
                        default:
                            itm[value["key"].ToString()] = value["value"].ToString();
                            break;
                    }
                }
                else{
                    flag = true;
                    attribute.Add(value["key"].ToString());
                    if(IsNumeric(value["value"].ToString()) is true)
                    {
                        itm[value["key"].ToString()] = (float)value["value"];
                        type.Add("N");
                        attr.Add(new Models.Attribute{attrName=value["key"].ToString(), type="N", key="n"});
                    }
                    else{
                        itm[value["key"].ToString()] = value["value"].ToString();
                        type.Add("S");
                        attr.Add(new Models.Attribute{attrName=value["key"].ToString(), type="S", key="n"});
                    }
                    
                }
            }
            await table.PutItemAsync(itm);
            
            if(flag is true)
            {           
                var ar = JArray.FromObject(attr);
                var obj = new JObject{
                    new JProperty("attr", ar)
                };
                await tableRepo.UpdateAttributeAndType(tableName, obj);
            }
            
        }
        


        private bool IsNumeric(string value)
        {
            try
            {
                double number;
                bool result = double.TryParse(value, out number);
                return result;
            }catch{};
            
            return false; 

        }
        
        public async Task RemoveItem(string tableName, string hashKey, string rangeKey = null)
        {
            Table table = Table.LoadTable(dynamoDB, tableName);
             if(rangeKey is null)
             {
                 try{
                    await table.DeleteItemAsync(hashKey);
                 }
                 catch{
                    await table.DeleteItemAsync(double.Parse(hashKey));
                }          
                
             }
             else
             {
                bool hKeyIsNumeric = IsNumeric(hashKey);     
                bool rKeyIsNumeric = IsNumeric(rangeKey);
                if(hKeyIsNumeric && rKeyIsNumeric)
                {
                    await table.DeleteItemAsync(double.Parse(hashKey), double.Parse(rangeKey));
                }else if(!hKeyIsNumeric && !rKeyIsNumeric)
                {
                    await table.DeleteItemAsync(hashKey, rangeKey);
                }else if(hKeyIsNumeric && !rKeyIsNumeric)
                {
                    await table.DeleteItemAsync(double.Parse(hashKey), rangeKey);
                }else
                {
                     await table.DeleteItemAsync(hashKey, double.Parse(rangeKey));
                }
             }
        }
   

        public async Task<Document> GetItem(string tableName, string hashKey, string rangeKey)
        {
            var tableRepo = new TableRepo(new AmazonDynamoDBClient());
            Document tableAttr = await tableRepo.GetTableAttr(tableName);
            List<string> attribute = new List<string>{};
            if(tableAttr != null)
            {           
                var c = tableAttr["attr"].AsDocument().GetAttributeNames();
                foreach(var c1 in c)
                {
                    attribute.Add(c1);
                }                                        
            }
            
            Table table = Table.LoadTable(dynamoDB,tableName);
            GetItemOperationConfig config = new GetItemOperationConfig
            {
                AttributesToGet = attribute,
                ConsistentRead = true
            };
            if(rangeKey != null){
                return await table.GetItemAsync(hashKey, rangeKey, config);
            } 
            return await table.GetItemAsync(hashKey, config);
        }

        public async Task UpdateItem(string tableName, JArray item)
        {
            var tableRepo = new TableRepo(dynamoDB);
            Document tableAttr = await tableRepo.GetTableAttr(tableName);
            List<string> attribute = new List<string>{};
            List<string> type = new List<string>{};
            List<string> key = new List<string>{};

            if(tableAttr != null)
            {           
                var c = tableAttr["attr"].AsDocument().GetAttributeNames();
                foreach(var c1 in c)
                {
                    attribute.Add(c1);
                    var t = tableAttr["attr"].AsDocument()[c1].AsDocument()["type"];
                    var k = tableAttr["attr"].AsDocument()[c1].AsDocument()["key"];
                    type.Add(t);
                    key.Add(k);
                }                                        
            }

            Table table = Table.LoadTable(dynamoDB, tableName);
            var itm = new Document();
            foreach(var value in item)
            {
                switch (type[attribute.IndexOf(value["key"].ToString())])
                {
                    case "N":
                        itm[value["key"].ToString()] = (float)value["value"];
                        break;
                    case "B":
                        byte[] byteArray = Encoding.ASCII.GetBytes(value["value"].ToString());
                        MemoryStream stream = new MemoryStream( byteArray );
                        itm[value["key"].ToString()] = stream;
                        break;
                    default:
                        itm[value["key"].ToString()] = value["value"].ToString();
                        break;
                }
            }
            UpdateItemOperationConfig config = new UpdateItemOperationConfig
            {
                ReturnValues = ReturnValues.AllNewAttributes
            };
           await table.UpdateItemAsync(itm, config);
        }


        public async Task<List<Dictionary<string, string>>> GetItems(string tableName){
             var res = await dynamoDB.ScanAsync(new ScanRequest
            {
                TableName = tableName
            });
            if(res != null && res.Items != null)
            {
                List<Dictionary<string, string>> table = new List<Dictionary<string, string>>();
                foreach(var itm in res.Items)
                {
                    var item = new Dictionary<string, string>();
                    var keys = itm.Keys;
                    foreach(var key in keys){
                        try{
                            item[key] = itm[key].S;                       
                        }
                        catch{
                             item[key] = itm[key].N;
                        }                  
                    }
                    table.Add(item);
                }
                return table;
            } 
            return new List<Dictionary<string, string>>();
        }
    }
}