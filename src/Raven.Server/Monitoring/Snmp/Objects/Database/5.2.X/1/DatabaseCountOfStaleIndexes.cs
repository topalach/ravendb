using System.Linq;
using Lextm.SharpSnmpLib;
using Raven.Server.Documents;
using Raven.Server.ServerWide.Context;

namespace Raven.Server.Monitoring.Snmp.Objects.Database
{
    public class DatabaseCountOfStaleIndexes : DatabaseScalarObjectBase<Gauge32>
    {
        public DatabaseCountOfStaleIndexes(string databaseName, DatabasesLandlord landlord, int index)
            : base(databaseName, landlord, SnmpOids.Databases.CountOfStaleIndexes, index)
        {
        }

        protected override Gauge32 GetData(DocumentDatabase database)
        {
            using (database.DocumentsStorage.ContextPool.AllocateOperationContext(out DocumentsOperationContext context))
            using (context.OpenReadTransaction())
            {
                var count = database
                    .IndexStore
                    .GetIndexes()
                    .Count(x => x.IsStale(context));

                return new Gauge32(count);
            }
        }
    }
}
