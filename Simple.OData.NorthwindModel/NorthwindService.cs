using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Services;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;

namespace Simple.OData.NorthwindModel
{
    public class NorthwindService : DataService<NorthwindEntities>
    {
        public static void InitializeService(DataServiceConfiguration config)
        {
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");

            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
            config.UseVerboseErrors = true;
        }

        protected override void HandleException(HandleExceptionArgs args)
        {
            base.HandleException(args);
        }

        [WebGet]
        public int ParseInt(string number)
        {
            return int.Parse(number);
        }

        [WebGet]
        public string ReturnString(string text)
        {
            return text;
        }

        [WebGet]
        public IQueryable<int> ReturnIntCollection(int count)
        {
            var numbers = new List<int>();
            for (var index = 1; index <= count; index++)
            {
                numbers.Add(index);
            }
            return numbers.AsQueryable();
        }

        [WebGet]
        public long PassThroughLong(long number)
        {
            return number;
        }

        [WebGet]
        public DateTime PassThroughDateTime(DateTime dateTime)
        {
            return dateTime;
        }

        [WebGet]
        public Guid PassThroughGuid(Guid guid)
        {
            return guid;
        }

        [WebGet]
        public IQueryable<Address> ReturnAddressCollection(int count)
        {
            var address = new Address {City = "Oslo", Country = "Norway", Region = "Oslo", PostalCode = "1234"};
            var addresses = new List<Address>();
            for (var index = 1; index <= count; index++)
            {
                addresses.Add(address);
            }
            return addresses.AsQueryable();
        }
    }
}

