using System;
using System.Linq;
using System.Threading.Tasks;
using FunctionApp1.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Endpoints
{
    class GetByIdFunction
    {
        [FunctionName("GetCommentById")]
        public static async Task<IActionResult> GetCommentById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Comment/{id}")]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Comments",
                collectionName: "Comment",
                ConnectionStringSetting = "CosmosDbConnectionString")]
            DocumentClient documentClient,
            ILogger log, string id)
        {
            log.LogInformation("Getting comment by id");
            Comment documentResponse;

            var commentCollectionUri = UriFactory.CreateDocumentCollectionUri("Comments", "Comment");

            try
            {
                documentResponse = documentClient.CreateDocumentQuery<Comment>(commentCollectionUri)
                    .Where(c => c.Id == id).AsEnumerable().First();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new {reason = "Comment with given id: " + id + " does not exist"});
            }

            log.LogInformation($"Comment with given id: {documentResponse.Id}");

            return new OkObjectResult(documentResponse);
        }
    }
}