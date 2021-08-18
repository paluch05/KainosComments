using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using FunctionApp1.Endpoints;
using FunctionApp1.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

namespace TestProject1
{
    public class AddCommentTest
    {
      
        // 1
        [Fact]
        public async Task ShouldAddComment()
        {
            // arrange
            string json =
            "{ \"author\": \"asd\", \"text\": \"Lorem ipsum dol.\"}";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            var reqMock = new Mock<HttpRequest>();
            reqMock.SetupGet(request => request.Body).Returns(stream);

            var document = new Document(){Id = "abc"};

            var documentClientMock = new Mock<IDocumentClient>();
            documentClientMock
                .Setup(client => client.CreateDocumentAsync(It.IsAny<Uri>(), It.IsAny<Comment>(), It.IsAny<RequestOptions>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceResponse<Document>(document)));


            var logMock = new Mock<ILogger>();
            //act
           IActionResult addCommentResult = await AddFunction.AddComment(reqMock.Object, documentClientMock.Object, logMock.Object);
           OkObjectResult okObjectResult = (OkObjectResult) addCommentResult;

            //assert
            okObjectResult.Value.Should().BeEquivalentTo(new
            {
                id = "abc"
            });
        }

        // 1. Happy path - OkObjectResult
        // 2. Blad w JSON-ie (np. brakujacy nawias) - BadRequestObjectResult
        // 3.Invalid data (3a. za dlugi komentarz + 3b. nulle + 3c empty string) - BadRequestObjectResult
        // 4 "Nic sie nie zwrocilo z CosmosDb" - InternalServerErrorResult

        // 3b.
        [Fact]
        public void ShouldReturnBadRequestWhenPassedNullValues()
        {

        }

        // 2.
        [Fact]
        public void ShouldReturnBadRequestWhenPassedWrongJSON()
        {

        }

        // 3a.
        [Fact]
        public void ShouldReturnBadRequestWhenValueIsBiggerThanExpected()
        {

        }

        // 4.

    }
}
