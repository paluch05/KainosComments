using FunctionApp1.Model;
using FunctionApp1.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;

namespace FunctionApp1.Endpoints
{
    public class AddFunction
    {
        [FunctionName("AddComment")]
        public static async Task<IActionResult> AddComment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Comment")]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Comments",
                collectionName: "Comment",
                ConnectionStringSetting = "CosmosDbConnectionString")]
            IDocumentClient documentClient, 
            ILogger log)
        {
            log.LogInformation("Creating a new comment");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(); 
            AddCommentRequest addCommentRequest; 

            try
            {
                addCommentRequest =
                    JsonConvert.DeserializeObject<AddCommentRequest>(
                        requestBody); 
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new
                { reason = "Your JSON format is incorrect" }); 
            }

            var addCommentRequestValidator = new AddCommentRequestValidator();
            var validationResult = addCommentRequestValidator.Validate(addCommentRequest);
            if (!validationResult.IsValid)
            {
                log.LogInformation(validationResult.ToString());
                return new BadRequestObjectResult(new { reason = "Invalid data" });
            }

            var comment = new Comment
            {
                Author = addCommentRequest.Author,
                Text = addCommentRequest.Text,
                CreationDate = DateTime.UtcNow
            };

            Document createResponse;
            var commentCollectionUri = UriFactory.CreateDocumentCollectionUri("Comments", "Comment");
            try
            {
                createResponse = await documentClient.CreateDocumentAsync(commentCollectionUri, comment);
            }
            catch (Exception e)
            {
                log.LogInformation("Unable to create a document");
                return new InternalServerErrorResult();
            }

            log.LogInformation("Comment successfully created.");
            return new OkObjectResult(new { id = createResponse.Id });
        }
    }
}
