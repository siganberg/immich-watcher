namespace Siganberg.ImmichWatcher.Tests.Integration.Helpers;

[CollectionDefinition(nameof(TestServerCollection))]
public class TestServerCollection : ICollectionFixture<TestServer>;