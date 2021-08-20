using System;
using System.IO;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
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

            var document = new Document() {Id = "abc"};

            var documentClientMock = new Mock<IDocumentClient>();
            documentClientMock
                .Setup(client => client.CreateDocumentAsync(It.IsAny<Uri>(), It.IsAny<Comment>(),
                    It.IsAny<RequestOptions>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceResponse<Document>(document)));


            var logMock = new Mock<ILogger>();
            //act
            IActionResult addCommentResult =
                await AddFunction.AddComment(reqMock.Object, documentClientMock.Object, logMock.Object);
            OkObjectResult okObjectResult = (OkObjectResult) addCommentResult;

            //assert
            okObjectResult.Value.Should().BeEquivalentTo(new
            {
                id = "abc"
            });
        }
        // 4 "Nic sie nie zwrocilo z CosmosDb" - InternalServerErrorResult

        // 3b.
        [Theory]
        [InlineData("{ \"author\": null, \"text\": null}")]
        [InlineData("{ \"author\": \"gggg\", \"text\": null}")]
        [InlineData("{ \"author\": null, \"text\": \"kkkkkk\"}")]

        public async Task ShouldReturnBadRequestWhenPassedNullValues(string json)
        {
                // arrange
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                MemoryStream stream = new MemoryStream(byteArray);
                var reqMock = new Mock<HttpRequest>();
                reqMock.SetupGet(request => request.Body).Returns(stream);

                var document = new Document() {Id = "abc"};

                var documentClientMock = new Mock<IDocumentClient>();
                documentClientMock
                    .Setup(client => client.CreateDocumentAsync(
                        It.IsAny<Uri>(), It.IsAny<Comment>(), It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new ResourceResponse<Document>(document)));

                var logMock = new Mock<ILogger>();
                //act
                IActionResult nullResult =
                    await AddFunction.AddComment(reqMock.Object, documentClientMock.Object, logMock.Object);
                BadRequestObjectResult badRequestObjectResult = (BadRequestObjectResult) nullResult;

                //assert
                badRequestObjectResult.Should().BeOfType<BadRequestObjectResult>();
            }

        [Theory]
        [InlineData("{ \"author\": \"\", \"text\": \"\"}")]
        [InlineData("{ \"author\": \"gggg\", \"text\": \"\"}")]
        [InlineData("{ \"author\": \"\", \"text\": \"kkkkkk\"}")]
        public async Task ShouldReturnBadRequestWhenPassedEmptyStrings(string json)
        {
                // arrange
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                MemoryStream stream = new MemoryStream(byteArray);
                var reqMock = new Mock<HttpRequest>();
                reqMock.SetupGet(request => request.Body).Returns(stream);

                var document = new Document() { Id = "abc" };

                var documentClientMock = new Mock<IDocumentClient>();
                documentClientMock
                    .Setup(client => client.CreateDocumentAsync(
                        It.IsAny<Uri>(), It.IsAny<Comment>(), It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new ResourceResponse<Document>(document)));


                var logMock = new Mock<ILogger>();
                //act
                IActionResult nullResult =
                    await AddFunction.AddComment(reqMock.Object, documentClientMock.Object, logMock.Object);
                BadRequestObjectResult badRequestObjectResult = (BadRequestObjectResult)nullResult;

                //assert
                badRequestObjectResult.Should().BeOfType<BadRequestObjectResult>();
            }

        // // 2.
        [Theory]
        [InlineData(" \"author\": \"gggg\", \"text\": \"jdkshfhuwhf\"}")]
        [InlineData(" {\"author\": \"gggg\", \"text\": \"jdkshfhuwhf\"")]
        [InlineData(" {\"author\": \"gggg\" \"text\": \"jdkshfhuwhf\"}")]
        [InlineData(" {\"author\" \"gggg\", \"text\": \"jdkshfhuwhf\"")]
        [InlineData(" {\"author\": \"gggg\", \"text\" \"jdkshfhuwhf\"")]

        public async Task ShouldReturnBadRequestWhenPassedWrongJson(string json)
        {
                // arrange
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                MemoryStream stream = new MemoryStream(byteArray);
                var reqMock = new Mock<HttpRequest>();
                reqMock.SetupGet(request => request.Body).Returns(stream);

                var document = new Document() {Id = "abc"};

                var documentClientMock = new Mock<IDocumentClient>();
                documentClientMock
                    .Setup(client => client.CreateDocumentAsync(
                        It.IsAny<Uri>(), It.IsAny<Comment>(), It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new ResourceResponse<Document>()));


                var logMock = new Mock<ILogger>();
                //act
                IActionResult addCommentResult =
                    await AddFunction.AddComment(reqMock.Object, documentClientMock.Object, logMock.Object);
                BadRequestObjectResult badRequestObjectResult = (BadRequestObjectResult) addCommentResult;

                //assert
                badRequestObjectResult.Should().BeOfType<BadRequestObjectResult>();
                badRequestObjectResult.Value.Should().BeEquivalentTo(new {reason = "Your JSON format is incorrect"});
        }

           string TooLongString(int x)
            {
                StringBuilder builder = new StringBuilder();
                string a = "a";
                for (int i = 0; i < x; i++)
                {
                    builder.Append(a);
                }
                return builder.ToString();
            }

            // 3a.
            [Fact]
            public async Task ShouldReturnBadRequestWhenTextValueIsBiggerThanExpected()
            {
                var tooLongComment = TooLongString(350);
                var json = $"{{\"text\": \"{tooLongComment}\", \"author\": \"some author\"}}";

                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                MemoryStream stream = new MemoryStream(byteArray);
                var reqMock = new Mock<HttpRequest>();
                reqMock.SetupGet(request => request.Body).Returns(stream);

                var document = new Document() { Id = "abc" };

                var documentClientMock = new Mock<IDocumentClient>();
                documentClientMock
                    .Setup(client => client.CreateDocumentAsync(
                        It.IsAny<Uri>(), It.IsAny<Comment>(), It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new ResourceResponse<Document>()));

            var logMock = new Mock<ILogger>();
                //act
                IActionResult addCommentResult =
                    await AddFunction.AddComment(reqMock.Object, documentClientMock.Object, logMock.Object);
                BadRequestObjectResult badRequestObjectResult = (BadRequestObjectResult)addCommentResult;

                //assert
                badRequestObjectResult.Should().BeOfType<BadRequestObjectResult>();
                badRequestObjectResult.Value.Should().BeEquivalentTo(new { reason = "Invalid data" });
            }

            [Fact]
            public async Task ShouldReturnBadRequestWhenAuthorValueIsBiggerThanExpected()
            {
                var tooLongAuthor = TooLongString(350);
                var json = $"{{\"text\": \"jdljkslad\", \"author\": \"{tooLongAuthor}\"}}";

                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                MemoryStream stream = new MemoryStream(byteArray);
                var reqMock = new Mock<HttpRequest>();
                reqMock.SetupGet(request => request.Body).Returns(stream);

                var document = new Document() {Id = "abc"};

                var documentClientMock = new Mock<IDocumentClient>();
                documentClientMock
                    .Setup(client => client.CreateDocumentAsync(
                        It.IsAny<Uri>(), It.IsAny<Comment>(), It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new ResourceResponse<Document>()));

                var logMock = new Mock<ILogger>();
                //act
                IActionResult addCommentResult =
                    await AddFunction.AddComment(reqMock.Object, documentClientMock.Object, logMock.Object);
                BadRequestObjectResult badRequestObjectResult = (BadRequestObjectResult) addCommentResult;

                //assert
                badRequestObjectResult.Should().BeOfType<BadRequestObjectResult>();
                badRequestObjectResult.Value.Should().BeEquivalentTo(new {reason = "Invalid data"});
            }

            [Fact]
            public async Task ShouldReturnInternalServerErrorWhenjds()
            {
            // arrange
            string json =
                "{ \"author\": \"asd\", \"text\": \"Lorem ipsum dol.\"}";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
                MemoryStream stream = new MemoryStream(byteArray);
                var reqMock = new Mock<HttpRequest>();
                reqMock.SetupGet(request => request.Body).Returns(stream);

                // var document = new Document() { Id = "abc" };

                var documentClientMock = new Mock<IDocumentClient>();
                documentClientMock
                    .Setup(client => client.CreateDocumentAsync(
                        It.IsAny<Uri>(), It.IsAny<Comment>(), It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Throws(new Exception());

                var logMock = new Mock<ILogger>();
                //act
                IActionResult nullResult =
                    await AddFunction.AddComment(reqMock.Object, documentClientMock.Object, logMock.Object);
                InternalServerErrorResult internalServerErrorResult= (InternalServerErrorResult)nullResult;

                //assert
               internalServerErrorResult.Should().BeOfType<InternalServerErrorResult>();
        }
    }
    }
