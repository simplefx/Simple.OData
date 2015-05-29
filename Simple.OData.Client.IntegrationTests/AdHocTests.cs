using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Simple.OData.Client.Tests
{
    class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    class Document
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public ICollection<DocumentLine> Lines { get; set; }
    }
    class DocumentLine
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
    }

    public class AdHocTests
    {
        [Fact]
        public async Task Top2()
        {
            var serviceUri = "http://bmservice.solucioneslive.com/Test/";
            var client = new ODataClient(new ODataClientSettings(serviceUri) { PayloadFormat = ODataPayloadFormat.Json });

            var result = await client
                .For<User>()
                .Function("Top2")
                .FindEntriesAsync();

            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
        }

        [Fact]
        public async Task InsertDocumentWithMultipleLines()
        {
            var serviceUri = "http://bmservice.solucioneslive.com/Test/";
            var client = new ODataClient(new ODataClientSettings(serviceUri) { PayloadFormat = ODataPayloadFormat.Json });

            var clientName = Guid.NewGuid().ToString();
            var line1 = new { ProductName = "Test1" };
            var line2 = new { ProductName = "Test2" };
            var line3 = new { ProductName = "Test3" };

            var batch = new ODataBatch(serviceUri);

            batch += x => x.For<DocumentLine>().Set(line1).InsertEntryAsync();
            batch += x => x.For<DocumentLine>().Set(line2).InsertEntryAsync();
            batch += x => x.For<DocumentLine>().Set(line3).InsertEntryAsync();
            batch += x => x.For<Document>().Set(new
            {
                ClientName = clientName,
                Lines = new[] { line1, line2, line3 }
            }).InsertEntryAsync();

            await batch.ExecuteAsync();

            var result = await client
                .For<Document>()
                .Filter(x => x.ClientName == clientName)
                .Expand(x => x.Lines)
                .FindEntryAsync();

            Assert.Equal(3, result.Lines.Count);
        }

        [Fact]
        public async Task CountOverlappingAssessments()
        {
            var serviceUri = "http://upscalability.azurewebsites.net/odata/";
            var client = new ODataClient(new ODataClientSettings(serviceUri)
            {
                PayloadFormat = ODataPayloadFormat.Json,
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
            });

            var result = await client
                .For("AssessmentTools")
                .Action("CountOverlappingAssessments")
                .Set(new Dictionary<string, object>
                    {
                        { "AssessmentToolId", new Guid("29fbb98d-3e51-4011-b4c0-2fa3bc0ad3a9") },
                        { "StartDate", DateTime.Parse("2014-12-01") },
                        { "EndDate", DateTime.Parse("2014-12-31") }
                    })
                .ExecuteAsScalarAsync<int>();

            Assert.Equal(1, result);
        }

        public class AssessmentTool
        {
        }

        [Fact]
        public async Task CountOverlappingAssessments_typed()
        {
            var serviceUri = "http://upscalability.azurewebsites.net/odata/";
            var client = new ODataClient(new ODataClientSettings(serviceUri)
            {
                PayloadFormat = ODataPayloadFormat.Json,
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
            });

            var result = await client
                .For<AssessmentTool>()
                .Action("CountOverlappingAssessments")
                .Set(new
                {
                    AssessmentToolId = new Guid("29fbb98d-3e51-4011-b4c0-2fa3bc0ad3a9"),
                    StartDate = DateTime.Parse("2014-12-01"),
                    EndDate = DateTime.Parse("2014-12-31"),
                })
                .ExecuteAsScalarAsync<int>();

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task SuccessFactorsMetadata()
        {
            var serviceUri = "https://api8.successfactors.com/odata/v2/";
            var client = new ODataClient(new ODataClientSettings(serviceUri)
            {
                Credentials = new NetworkCredential("reganbr@purduetest", "Secret1"),
                AfterResponse = AfterResponse,
            });

            var metadata = await client.GetMetadataAsStringAsync();
            Assert.NotNull(metadata);

            var x = ODataDynamic.Expression;
            var territories = await client.For(x.Territory).FindEntriesAsync() as IEnumerable<dynamic>;
            Assert.True(territories.Count() > 0);
        }

        [Fact]
        public async Task Sap()
        {
            var serviceUri = "http://d72w29y1.dxq8kf.pertino.net:8010/sap/opu/odata/sap/ZHR_SRV/";
            var client = new ODataClient(new ODataClientSettings(serviceUri)
            {
                Credentials = new NetworkCredential("developer1", "WELCOME1"),
            });

            var metadata = await client.GetMetadataAsStringAsync();
            Assert.NotNull(metadata);

            var x = ODataDynamic.Expression;
            var data = await client.For(x.EmployeeSet).FindEntriesAsync() as IEnumerable<dynamic>;
            Assert.True(data.Count() > 0);
        }

        private static void AfterResponse(HttpResponseMessage responseMessage)
        {
            if (responseMessage.RequestMessage.RequestUri.LocalPath.EndsWith("$metadata"))
            {
                var metadata = responseMessage.Content.ReadAsStringAsync().Result;
                var doc = new XmlDocument();
                doc.LoadXml(metadata);

                var manager = new XmlNamespaceManager(doc.NameTable);
                manager.AddNamespace("ns", "http://schemas.microsoft.com/ado/2008/09/edm");
                var nodes = doc.SelectNodes("//ns:Annotation", manager);
                for (var i = nodes.Count - 1; i >= 0; i--)
                {
                    nodes[i].ParentNode.RemoveChild(nodes[i]);
                }

                nodes = doc.SelectNodes("//ns:Property[@CollectionKind]", manager);
                for (var i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Attributes.Remove(nodes[i].Attributes["CollectionKind"]);
                }

                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    metadata = stringWriter.GetStringBuilder().ToString();
                }

                var byteArray = Encoding.UTF8.GetBytes(metadata);
                var stream = new MemoryStream(byteArray);
                stream.Position = 0;
                var content = new StreamContent(stream);
                foreach (var header in responseMessage.Content.Headers)
                {
                    if (header.Key != "Content-Length")
                        content.Headers.Add(header.Key, header.Value);
                }
                content.Headers.Add("Content-Length", metadata.Length.ToString());
                responseMessage.Content = content;
            }
        }

        class ClientProductSku
        {
            public int ClientId { get; set; }
            public int PartNo { get; set; }
            public int? ProductId { get; set; }
            public int StatusId { get; set; }
            public Product Product { get; set; }
        }

        class Product
        {
            public int Id { get; set; }
            public int ClientId { get; set; }
            public int ManufacturerId { get; set; }
            public ProductCategory ProductCategory { get; set; }
        }

        class ProductCategory
        {
            public int ClientId { get; set; }
            public int? ProductId { get; set; }
            public int? CategoryId { get; set; }
            public int SortOrder { get; set; }
            public Category Category { get; set; }
        }

        class Category
        {
            public int Id { get; set; }
            public int ClientId { get; set; }
            public string Code { get; set; }
        }

        [Fact]
        public async Task Issue92()
        {
            var reader = new StreamReader(@"D:\Projects\Temp\Issue92\metadata.xml");
            var metadata = reader.ReadToEnd();
            reader.Close();

            var client = new ODataClient(new ODataClientSettings()
            {
                UrlBase = "http://localhost.com/",
                MetadataDocument = metadata,
            });

            var clientId = 1;

            var annotations = new ODataFeedAnnotations();
            var items =
                client.For<ClientProductSku>()
                    .Filter(x => x.ClientId == clientId)
                    .Expand("Product/ProductCategory/Category")
                    .Select("PartNo", "ClientId", "Product/ProductCategory/Category/Code")
                    .FindEntriesAsync(annotations)
                    .Result;
        }

        [Fact]
        public async Task Issue92Typed()
        {
            var reader = new StreamReader(@"D:\Projects\Temp\Issue92\metadata.xml");
            var metadata = reader.ReadToEnd();
            reader.Close();

            var client = new ODataClient(new ODataClientSettings()
            {
                UrlBase = "http://localhost.com/",
                MetadataDocument = metadata,
            });

            var clientId = 1;

            var annotations = new ODataFeedAnnotations();
            var items =
                client.For<ClientProductSku>()
                    .Filter(x => x.ClientId == clientId)
                    .Expand(x => x.Product.ProductCategory.Category)
                    .Select(x => new { x.PartNo, x.ClientId, x.Product.ProductCategory.Category.Code})
                    .FindEntriesAsync(annotations)
                    .Result;
        }

        class QuestionRule
        {
            public QuestionRule()
            {
                this.Id = Guid.NewGuid();
            }
            public Guid Id { get; set; }
        }

        class Question
        {
            public Question()
            {
                this.Id = Guid.NewGuid();
                this.QuestionRules = new List<QuestionRule>()
                {
                    new QuestionRule(),
                };
            }
            public Guid Id { get; set; }
            public IList<QuestionRule> QuestionRules { get; set; }
        }

        class Section
        {
            public Section()
            {
                this.Id = Guid.NewGuid();
                this.Questions = new List<Question>()
                {
                    new Question(),
                };
            }
            public Guid Id { get; set; }
            public IList<Question> Questions { get; set; }
        }

        class Revision
        {
            public Revision()
            {
                this.Id = Guid.NewGuid();
                this.Sections = new List<Section>()
                {
                    new Section(),
                };
            }
            public Guid Id { get; set; }
            public IList<Section> Sections { get; set; }
        }

        [Fact]
        public async Task Issue100()
        {
            var reader = new StreamReader(@"D:\Projects\Temp\Issue100\metadata.xml");
            var metadata = reader.ReadToEnd();
            reader.Close();

            var client = new ODataClient(new ODataClientSettings()
            {
                UrlBase = "http://localhost.com/",
                MetadataDocument = metadata,
            });
            var batch = new ODataBatch(client);

            var entity = new Revision();

            batch += c => c.For<Revision>().Set(entity).InsertEntryAsync();
            foreach (var section in entity.Sections)
            {
                batch += c => c.For<Section>().Set(section).InsertEntryAsync();
                foreach (var question in section.Questions)
                {
                    batch += c => c.For<Question>().Set(question).InsertEntryAsync();
                    foreach (var rule in question.QuestionRules)
                    {
                        batch += c => c.For<QuestionRule>().Set(rule).InsertEntryAsync();
                    }
                }
            }

            await batch.ExecuteAsync();
        }

        [Fact]
        public async Task Issue110()
        {
            var reader = new StreamReader(@"D:\Projects\Temp\Issue100\metadata.xml");
            var metadata = reader.ReadToEnd();
            reader.Close();

            var client = new ODataClient(new ODataClientSettings()
            {
                UrlBase = "http://localhost.com/",
                MetadataDocument = metadata,
            });

            var annotations = new ODataFeedAnnotations();
            var result = await client
                .For<ClientProductSku>()
                .Function("GetExportProductFull")
                .Set(new { clientId = 1, skuTypeIdToExclude=2, skuStatusIdToExclude =3 })
                .Expand("Product")
                .FindEntriesAsync(new Uri("http://localhost.com"), annotations);
        }
    }
}