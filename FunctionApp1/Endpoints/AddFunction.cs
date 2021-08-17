using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using FunctionApp1.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp1.Endpoints
{
    class AddFunction
    {
        [FunctionName("AddComment")]
        public static async Task<IActionResult> AddComment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Comment")]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Comments",
                collectionName: "Comment",
                ConnectionStringSetting = "CosmosDbConnectionString")]
            DocumentClient documentClient, // dodaje doc do cosm
            ILogger log)
        {
            log.LogInformation("Creating a new comment");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(); // zamiana body z req na string z json
            AddCommentRequest addCommentRequest; // dekoracja zmiennej

            try
            {
                addCommentRequest =
                    JsonConvert.DeserializeObject<AddCommentRequest>(
                        requestBody); // deserializacja str z json na obj kl addcomreq
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new
                { reason = "Your JSON format is incorrect" }); // 400, format zly np bez {
            }

            if (!string.IsNullOrEmpty(addCommentRequest.Author) && !string.IsNullOrEmpty(addCommentRequest.Text))
            {
                var comment = new Comment
                {
                    Author = addCommentRequest.Author,
                    Text = addCommentRequest.Text,
                    CreationDate = DateTime.UtcNow
                };

                var commentCollectionUri = UriFactory.CreateDocumentCollectionUri("Comments", "Comment");

                var createResponse = await documentClient.CreateDocumentAsync(commentCollectionUri, comment);
                if (createResponse.Resource == null)
                {
                    return new InternalServerErrorResult();
                }

                log.LogInformation("Comment successfully created.");
                return new OkObjectResult(new { id = createResponse.Resource.Id });
            }

            return new BadRequestObjectResult(new
            { reason = "One of your values is null" }); // jezeli ktorys jest null, idzie prosto tu
        }
    }
}
