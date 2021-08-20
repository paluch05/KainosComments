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

namespace FunctionApp1
{
    public static class UpdateByIdFunction
    {
        [FunctionName("UpdateCommentById")]
        public static async Task<IActionResult> UpdateCommentById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Comment/{id}")]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Comments",
                collectionName: "Comment",
                ConnectionStringSetting = "CosmosDbConnectionString")]
            DocumentClient documentClient,
            ILogger log, string id)
        {
            log.LogInformation("Updating comment by id");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UpdateCommentRequest updateCommentRequest;

            try
            {
                updateCommentRequest =
                    JsonConvert.DeserializeObject<UpdateCommentRequest>(
                        requestBody);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new
                { reason = "Your JSON format is incorrect" });
            }

            var updateCommentRequestValidator = new UpdateCommentRequestValidator();
            var validationResult = updateCommentRequestValidator.Validate(updateCommentRequest);

            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors);
            }

            var documentUri = UriFactory.CreateDocumentUri("Comments", "Comment", id);
            var readDocumentResponse = await documentClient.ReadDocumentAsync(documentUri,
               new RequestOptions { PartitionKey = new PartitionKey(id) });
            var readDocument = readDocumentResponse.Resource;

            readDocument.SetPropertyValue("Text", updateCommentRequest.Text);

            var response = await documentClient.ReplaceDocumentAsync(documentUri, readDocument);

            return new OkObjectResult(new
            {
                id = readDocumentResponse.Resource.Id,
                text = response.Resource.GetPropertyValue<string>("Text")
            });
        }
    }
}
