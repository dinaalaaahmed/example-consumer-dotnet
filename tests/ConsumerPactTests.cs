using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using PactNet;
using Consumer;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text.Json.Nodes;
using PactNet.Matchers;
using FluentAssertions;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace tests
{
    public class ConsumerPactTests
    {
        private IPactBuilderV3 pact;
        // private readonly int port = 9222;

        private readonly List<object> products;
        private readonly string productId = "3";
        public ConsumerPactTests(ITestOutputHelper output)
        {

            products = new List<object>()
            {
                new { id = "27", name = "burger", type = "food" }
            };

            var Config = new PactConfig
            {
                PactDir = Path.Join("..", "..", "..", "..", "pacts"),
                Outputters = new List<IOutput> { new XunitOutput(output), new ConsoleOutput() },
                LogLevel = PactLogLevel.Debug
            };

            pact = Pact.V3("pactflow-example-consumer-dotnet", "pactflow-example-provider-dotnet", Config).WithHttpInteractions();
        }

        [Fact]
        public async Task RetrieveProducts()
        {
            // Arrange
            pact.UponReceiving("A request to get products")
                        .Given("products exist")
                        .WithRequest(HttpMethod.Get, "/products")
                    .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(Match.MinType(products[0],1));

            await pact.VerifyAsync(async ctx =>
            {
                // Act
                var consumer = new ProductClient();
                List<Product> result = await consumer.GetProducts(ctx.MockServerUri.ToString().TrimEnd('/'));
                // Assert
                result.Should().NotBeNull();
                result.Should().HaveCount(1);
                Assert.Equal("27",result[0].id);
                Assert.Equal("burger",result[0].name);
                Assert.Equal("food",result[0].type);
            });
        }
        
        [Fact]
        public async Task ProductNotFound()
        {
            // Arrange
            pact.UponReceiving("A request to get product")
                .Given("no products exist")
                .WithRequest(HttpMethod.Get, $"/products/{productId}")
                .WillRespond()
                .WithStatus(HttpStatusCode.NotFound);
            
            await pact.VerifyAsync(async ctx =>
            {
                // Act
                var consumer = new ProductClient();
                Product? result = await consumer.GetProduct(ctx.MockServerUri.ToString().TrimEnd('/'), productId);
                // Assert
                result.Should().BeNull();
            });
        }
    }
}
