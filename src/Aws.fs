module Aws

open System.Threading.Tasks
open Amazon.DynamoDBv2.Model
open Amazon.SimpleNotificationService.Model

let private synchronous (task : Task<'a>) = task.Result


module DB =
    open System.Collections.Generic
    open Amazon.DynamoDBv2
    open Amazon.DynamoDBv2.DocumentModel
    
    let put item =
        result {
            let! tableName = Environment.tryGetEnvironmentVariable "TABLE_NAME"
            let client = new AmazonDynamoDBClient()
            let table = Table.LoadTable(client, tableName)
            
            item
            |> Newtonsoft.Json.JsonConvert.SerializeObject
            |> Document.FromJson
            |> table.PutItemAsync
            |> synchronous
            |> ignore
        }
        
    let query<'a> (CustomerId id) =
        result {
            let! tableName = Environment.tryGetEnvironmentVariable "TABLE_NAME"
            let client = new AmazonDynamoDBClient()
            let table = Table.LoadTable(client, tableName)
            let filter = QueryFilter("CustomerId", QueryOperator.Equal, [AttributeValue (id.ToString())] |> List)
            
            let search =
                QueryOperationConfig(
                    Limit = 1000,
                    Select = SelectValues.AllAttributes,
                    ConsistentRead = true,
                    Filter = filter)
                |> table.Query
                
            return seq {
                while not search.IsDone do
                    yield! search.GetNextSetAsync() |> synchronous |> seq
            }
            |> Seq.map (fun x -> x.ToJson() |> Newtonsoft.Json.JsonConvert.DeserializeObject<'a>)
        }
        
        
module Messages =
    open Amazon.SimpleNotificationService
    
    
    let private getTopic =
        function
        | BookingCreated _ -> "BOOKING_CREATED_TOPIC"
        >> Environment.tryGetEnvironmentVariable
        
    let private getBody =
        function
        | BookingCreated x -> x
        >> Newtonsoft.Json.JsonConvert.SerializeObject
    
    let publish msg =
        result {
            let client = new AmazonSimpleNotificationServiceClient()
            let! topic = getTopic msg
            PublishRequest(topic, getBody msg)
            |> client.PublishAsync
            |> synchronous
            |> ignore
        }