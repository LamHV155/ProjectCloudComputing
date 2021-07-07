using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using CCAPIProject.Repo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Amazon.DynamoDBv2.DocumentModel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CCAPIProject
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if(request.Resource.Contains("item"))
            {
                if(request.HttpMethod == "POST" && request.QueryStringParameters.ContainsKey("tableName"))
                {
                    
                    var obj = JsonConvert.DeserializeObject(request.Body);
                    var data = JObject.FromObject(obj);
                    var itemRepo = new ItemRepo(new AmazonDynamoDBClient());
                    try{
                         await itemRepo.AddNewItem(request.QueryStringParameters["tableName"], (JArray)data["item"]);
                         return new APIGatewayProxyResponse{
                             StatusCode = 200
                         };
                    }catch{}  
                }
                else if(request.HttpMethod == "PUT" && request.QueryStringParameters.ContainsKey("tableName"))
                {
                    var obj = JsonConvert.DeserializeObject(request.Body);
                    var data = JObject.FromObject(obj);
                    var itemRepo = new ItemRepo(new AmazonDynamoDBClient());
                    try{
                         await itemRepo.UpdateItem(request.QueryStringParameters["tableName"], (JArray)data["item"]);
                         return new APIGatewayProxyResponse{
                             StatusCode = 200
                         };
                    }catch{}  
                }
                else if(request.HttpMethod == "DELETE" && request.QueryStringParameters.ContainsKey("tableName"))
                {
                     var itemRepo = new ItemRepo(new AmazonDynamoDBClient());
                     try{
                         if(request.QueryStringParameters.ContainsKey("rangeKey"))
                         {
                              await itemRepo.RemoveItem(request.QueryStringParameters["tableName"], request.QueryStringParameters["hashKey"],request.QueryStringParameters["rangeKey"]);
                         }
                         else{
                             await itemRepo.RemoveItem(request.QueryStringParameters["tableName"], request.QueryStringParameters["hashKey"], null);
                         }
                          return await Task.FromResult(new APIGatewayProxyResponse{
                             StatusCode = 200});
                     }catch{}
                }
                else if(request.HttpMethod == "GET" && request.QueryStringParameters.ContainsKey("tableName") && request.QueryStringParameters.ContainsKey("hashKey"))
                {
                    var itemRepo = new ItemRepo(new AmazonDynamoDBClient());
                    var item = new Document();
                    if(request.QueryStringParameters.ContainsKey("rangeKey") is true){
                         item = await itemRepo.GetItem(request.QueryStringParameters["tableName"], request.QueryStringParameters["hashKey"], request.QueryStringParameters["rangeKey"]);
                    }else{
                         item = await itemRepo.GetItem(request.QueryStringParameters["tableName"], request.QueryStringParameters["hashKey"], null);
                    }                  
                    return new APIGatewayProxyResponse{
                        StatusCode = 200,
                        Body = JsonConvert.SerializeObject(item)
                    };
                }
                 return await Task.FromResult(new APIGatewayProxyResponse{
                             StatusCode = 404});
            }
            if(request.HttpMethod == "GET" && request.Resource.Contains("getattr"))
            {
                var tableRepo = new TableRepo(new AmazonDynamoDBClient());
                var attr = await tableRepo.GetTableAttr(request.QueryStringParameters["tableName"]);
                return new APIGatewayProxyResponse{
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(attr)
                };
            }
            else if(request.HttpMethod == "GET" && request.Resource.Contains("gettable"))
            {
                var itemRepo = new ItemRepo(new AmazonDynamoDBClient());
                var table = await itemRepo.GetItems(request.QueryStringParameters["tableName"]);
                return new APIGatewayProxyResponse{
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(table)
                };
            }
            else if(request.HttpMethod == "GET")
            {
                var tableRepo = new TableRepo(new AmazonDynamoDBClient());
                var tables = await tableRepo.GetTablesAsync();
                return new APIGatewayProxyResponse {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(tables)
                };
            }
            else if(request.HttpMethod == "POST")
            {
                var table = JsonConvert.DeserializeObject<Dtos.CreateTableDto>(request.Body);
                if(table == null) return new APIGatewayProxyResponse{StatusCode = 400};

                var tableRepo = new TableRepo(new AmazonDynamoDBClient());
                if(await tableRepo.CreateTableAsync(table))
                {
                    return new APIGatewayProxyResponse{StatusCode = 200, Body="OK"};
                }else{
                    return new APIGatewayProxyResponse{StatusCode = 400};
                }
                
            }
            else if(request.HttpMethod == "DELETE")
            {
                if(request.QueryStringParameters.ContainsKey("tableName"))
                {
                    var tableName = request.QueryStringParameters["tableName"];
                    var tableRepo = new TableRepo(new AmazonDynamoDBClient());
                    if(await tableRepo.RemoveTableAsync(tableName))
                    {
                         return new APIGatewayProxyResponse{StatusCode = 200};
                    }else{
                        return new APIGatewayProxyResponse{StatusCode = 400};
                    }
                }
                 return new APIGatewayProxyResponse{StatusCode = 400};
            }

            return await Task.FromResult(new APIGatewayProxyResponse {
                StatusCode = 404
            });
           
        }
    }
}
