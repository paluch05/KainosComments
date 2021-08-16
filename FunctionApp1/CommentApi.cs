using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using FunctionApp1.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp1
{
    public static class CommentApi
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
                    {reason = "Your JSON format is incorrect"}); // 400, format zly np bez {
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
                return new ObjectResult(new {id = createResponse.Resource.Id});
            }

            return new BadRequestObjectResult(new
                {reason = "One of your values is null"}); // jezeli ktorys jest null, idzie prosto tu
        }

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

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(); // zamiana body z req na string z json
            UpdateCommentRequest updateCommentRequest; // dekoracja zmiennej

            try
            {
                updateCommentRequest = JsonConvert.DeserializeObject<UpdateCommentRequest>(
                    requestBody); // deserializacja str z json na obj kl addcomreq
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new
                    {reason = "Your JSON format is incorrect"}); // 400, format zly np bez {
            }

            if (!string.IsNullOrEmpty(updateCommentRequest.Text) && updateCommentRequest.Text.Length <= 200)
            {
                var comment = new Comment
                {
                    Id = id,
                    Author = updateCommentRequest.Author,
                    Text = updateCommentRequest.Text,
                    CreationDate = DateTime.UtcNow
                };

                var commentCollectionUri = UriFactory.CreateDocumentCollectionUri("Comments", "Comment");

                var updateResponse = await documentClient.UpsertDocumentAsync(commentCollectionUri, comment);
                var upserted = updateResponse.Resource;

                if (upserted == null)
                {
                    return new InternalServerErrorResult();
                }

                log.LogInformation("Comment successfully updated.");
                return new OkObjectResult(new
                {
                    id = upserted.Id,
                    text = comment.Text,
                    author = comment.Author
                });

            }

            return new BadRequestObjectResult(new
                {reason = "Your comment can not be null or have more than 200 chars"});
        }

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
                    new RequestOptions {PartitionKey = new PartitionKey(id)});
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