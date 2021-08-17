using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using FunctionApp1.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp1.Endpoints
{
    class GetAllFunction
    {
        [FunctionName("GetAllComments")]
        public static async Task<IActionResult> GetAllComments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Comment")]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Comments",
                collectionName: "Comment",
                ConnectionStringSetting = "CosmosDbConnectionString")]
            DocumentClient documentClient,
            ILogger log)
        {
            log.LogInformation("Getting all comments");

            List<Comment> comments = new List<Comment>();
            string continuationToken = null;
            do
            {
                var commentCollectionUri = UriFactory.CreateDocumentCollectionUri("Comments", "Comment");

                var feed = await documentClient.ReadDocumentFeedAsync(commentCollectionUri,
                    new FeedOptions {RequestContinuation = continuationToken});
                if (feed.CurrentResourceQuotaUsage == null)
                {
                    return new InternalServerErrorResult();
                }

                continuationToken = feed.ResponseContinuation;

                foreach (Document document in feed)
                {
                    var comment = JsonConvert.DeserializeObject<Comment>(document.ToString());
                    comments.Add(comment);
                }
            } while (continuationToken != null);

            log.LogInformation("List of all comments: ");
            return new OkObjectResult(new {result = comments});
        }
    }
}