using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace FunctionApp1.Endpoints
{
    public class DeleteByIdFunction
    {
        [FunctionName("DeleteCommentById")]
        public static async Task<IActionResult> DeleteCommentById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Comment/{id}")]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Comments",
                collectionName: "Comment",
                ConnectionStringSetting = "CosmosDbConnectionString")]
            IDocumentClient documentClient,
            ILogger log, string id)
        {
            log.LogInformation("Deleting comment by id");

            try
            {
                var documentUri = UriFactory.CreateDocumentUri("Comments", "Comment", id);

                ResourceResponse<Document> response = await documentClient.DeleteDocumentAsync(documentUri,
                    new RequestOptions { PartitionKey = new PartitionKey(id) });

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new BadRequestObjectResult((new { reason = $"Comment with id: {id} does not exist." }));
                }
            }
            catch (Exception e)
            {
                return new InternalServerErrorResult();
            }

            log.LogInformation($"Comment with given id: {id} successfully deleted");

            return new NoContentResult();
        }
    }
}
