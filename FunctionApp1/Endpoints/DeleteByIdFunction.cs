using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Endpoints
{
    class DeleteByIdFunction
    {
        [FunctionName("DeleteCommentById")]
        public static async Task<IActionResult> DeleteCommentById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Comment/{id}")]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Comments",
                collectionName: "Comment",
                ConnectionStringSetting = "CosmosDbConnectionString")]
            DocumentClient documentClient,
            ILogger log, string id)
        {
            log.LogInformation("Deleting comment by id");
            ResourceResponse<Document> response;

            try
            {
                var documentUri = UriFactory.CreateDocumentUri("Comments", "Comment", id);
                response = await documentClient.DeleteDocumentAsync(documentUri,
                    new RequestOptions { PartitionKey = new PartitionKey(id) });
            }
            catch
            {
                return new BadRequestObjectResult((new { reason = $"Comment with id: {id} does not exist." }));
            }

            log.LogInformation($"Comment with given id: {id} successfully deleted");
            return new NoContentResult();
        }
    }
}
