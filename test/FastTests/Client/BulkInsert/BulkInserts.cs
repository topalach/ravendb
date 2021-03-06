﻿using System;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Exceptions.Documents.BulkInsert;
using Raven.Client.Extensions;
using Raven.Client.Json;
using Sparrow;
using Xunit;

namespace FastTests.Client.BulkInsert
{
    public class BulkInserts : RavenTestBase
    {
        [Fact]
        public void SimpleBulkInsertShouldWork()
        {
            var fooBars = new[]
            {
                new FooBar { Name = "John Doe" },
                new FooBar { Name = "Jane Doe" },
                new FooBar { Name = "Mega John" },
                new FooBar { Name = "Mega Jane" }
            };

            using (var store = GetDocumentStore())
            {
                using (var bulkInsert = store.BulkInsert())
                {
                    bulkInsert.Store(fooBars[0]);
                    bulkInsert.Store(fooBars[1]);
                    bulkInsert.Store(fooBars[2]);
                    bulkInsert.Store(fooBars[3]);
                }

                using (var session = store.OpenSession())
                {
                    var doc1 = session.Load<FooBar>("FooBars/1-A");
                    var doc2 = session.Load<FooBar>("FooBars/2-A");
                    var doc3 = session.Load<FooBar>("FooBars/3-A");
                    var doc4 = session.Load<FooBar>("FooBars/4-A");

                    Assert.NotNull(doc1);
                    Assert.NotNull(doc2);
                    Assert.NotNull(doc3);
                    Assert.NotNull(doc4);

                    Assert.Equal("John Doe", doc1.Name);
                    Assert.Equal("Jane Doe", doc2.Name);
                    Assert.Equal("Mega John", doc3.Name);
                    Assert.Equal("Mega Jane", doc4.Name);
                }
            }
        }

        [Fact]
        public async Task AsyncSimpleBulkInsertShouldWork()
        {
            var fooBars = new[]
            {
                new FooBar { Name = "John Doe" },
                new FooBar { Name = "Jane Doe" },
                new FooBar { Name = "Mega John" },
                new FooBar { Name = "Mega Jane" }
            };

            using (var store = GetDocumentStore())
            {
                using (var bulkInsert = store.BulkInsert())
                {
                    await bulkInsert.StoreAsync(fooBars[0]);
                    await bulkInsert.StoreAsync(fooBars[1]);
                    await bulkInsert.StoreAsync(fooBars[2]);
                    await bulkInsert.StoreAsync(fooBars[3]);
                }

                using (var session = store.OpenSession())
                {
                    var doc1 = session.Load<FooBar>("FooBars/1-A");
                    var doc2 = session.Load<FooBar>("FooBars/2-A");
                    var doc3 = session.Load<FooBar>("FooBars/3-A");
                    var doc4 = session.Load<FooBar>("FooBars/4-A");

                    Assert.NotNull(doc1);
                    Assert.NotNull(doc2);
                    Assert.NotNull(doc3);
                    Assert.NotNull(doc4);

                    Assert.Equal("John Doe", doc1.Name);
                    Assert.Equal("Jane Doe", doc2.Name);
                    Assert.Equal("Mega John", doc3.Name);
                    Assert.Equal("Mega Jane", doc4.Name);
                }
            }
        }

        [Fact]
        public async Task KilledTooEarly()
        {
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                using (var store = GetDocumentStore())
                {
                    using (var bulkInsert = store.BulkInsert())
                    {
                        await bulkInsert.StoreAsync(new FooBar());
                        await bulkInsert.AbortAsync();
                        await bulkInsert.StoreAsync(new FooBar());
                    }
                }
            });

            if (exception is AggregateException ae)
                exception = ae.ExtractSingleInnerException();

            Assert.True(exception is BulkInsertAbortedException);
        }

        [Fact]
        public void ShouldNotAcceptIdsEndingWithPipeLine()
        {
            using (var store = GetDocumentStore())
            {
                using (var bulkInsert = store.BulkInsert())
                {
                    var exception = Assert.Throws<NotSupportedException>(() =>
                            bulkInsert.Store(new FooBar { Name = "John Doe" }, "foobars|"));
                    Assert.Contains("Document ids cannot end with '|', but was called with foobars|", exception.Message);
                }
            }
        }

        [Fact]
        public void CanModifyMetadataWithBulkInsert()
        {
            var expirationDate = DateTime.Today.AddYears(1).ToString(DefaultFormat.DateTimeOffsetFormatsToWrite);
            using (var store = GetDocumentStore())
            {
                using (var bulkInsert = store.BulkInsert())
                {
                    bulkInsert.Store(new FooBar
                    {
                        Name = "Jon Snow"
                    }, new MetadataAsDictionary
                    {
                        [Constants.Documents.Metadata.Expires] = expirationDate
                    });
                }

                using (var session = store.OpenSession())
                {
                    var entity = session.Load<FooBar>("FooBars/1-A");
                    var metadataExpirationDate = session.Advanced.GetMetadataFor(entity)[Constants.Documents.Metadata.Expires];

                    Assert.Equal(expirationDate, metadataExpirationDate);
                }
            }
        }

        private class FooBar
        {
            public string Name { get; set; }
        }
    }
}
