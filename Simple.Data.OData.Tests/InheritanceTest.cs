using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Simple.Data.OData.Tests
{
    public class InheritanceTest : TestBase
    {
        [Fact]
        public void FindAllTransports()
        {
            IEnumerable<dynamic> transports = _db.Transport.All();

            Assert.NotEmpty(transports);
            foreach (var transport in transports)
            {
                if (transport.TransportID == 1)
                {
                    Assert.Equal("Titanic", transport.ShipName);
                }
                else if (transport.TransportID == 2)
                {
                    Assert.Equal("123456", transport.TruckNumber);
                }
            }
        }

        [Fact]
        public void FindAllShips()
        {
            IEnumerable<dynamic> ships = _db.Ships.All();

            Assert.Equal(1, ships.Count());
            Assert.Equal("Titanic", ships.First().ShipName);
        }

        [Fact]
        public void FindOneShip()
        {
            IEnumerable<dynamic> ships = _db.Ships.FindAll(_db.Ships.ShipName == "Titanic");

            Assert.Equal(1, ships.Count());
            Assert.Equal("Titanic", ships.First().ShipName);
        }


        [Fact]
        public void InsertShip()
        {
            var ship = _db.Ships.Insert(ShipName: "Test1");

            Assert.Equal("Test1", ship.ShipName);
        }

        [Fact]
        public void InsertTruck()
        {
            var truck = _db.Trucks.Insert(TruckNumber: "Test2");

            Assert.Equal("Test2", truck.TruckNumber);
        }

        [Fact]
        public void UpdateShip()
        {
            var ship = _db.Ships.Insert(ShipName: "Test1");
            _db.Ships.UpdateByTransportID(TransportID: ship.TransportID, ShipName: "Test2");
            ship = _db.Transport.FindByTransportID(ship.TransportID);

            Assert.Equal("Test2", ship.ShipName);
        }

        [Fact]
        public void DeleteShip()
        {
            var ship = _db.Ships.Insert(ShipName: "Test1");
            var count = _db.Transport.All().Count();
            _db.Transport.DeleteByTransportID(ship.TransportID);

            Assert.Equal(count-1, _db.Transport.All().Count());
        }

        [Fact]
        public void BatchInsert()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Ships.Insert(ShipName: "Test1");
                tx.Trucks.Insert(TruckNumber: "Test2");
                tx.Commit();
            }

            bool shipFound = false;
            bool truckFound = false;
            IEnumerable<dynamic> transports = _db.Transports.All();
            foreach (var transport in transports)
            {
                foreach (var item in transport)
                {
                    if (item.Key == "ShipName")
                        shipFound = item.Value == "Test1";
                    else if (item.Key == "TruckNumber")
                        truckFound = item.Value == "Test2";
                }
            }
            Assert.True(shipFound);
            Assert.True(truckFound);
        }

        [Fact]
        public void BatchUpdate()
        {
            var ship = _db.Ships.Insert(ShipName: "Test1");
            var truck = _db.Trucks.Insert(TruckNumber: "Test2");

            using (var tx = _db.BeginTransaction())
            {
                tx.Ships.UpdateByTransportID(TransportID : ship.TransportID, ShipName: "Test3");
                tx.Trucks.UpdateByTransportID(TransportID: truck.TransportID, TruckNumber: "Test4");
                tx.Commit();
            }

            bool shipFound = false;
            bool truckFound = false;
            IEnumerable<dynamic> transports = _db.Transports.All();
            foreach (var transport in transports)
            {
                foreach (var item in transport)
                {
                    if (item.Key == "ShipName")
                        shipFound = item.Value == "Test3";
                    else if (item.Key == "TruckNumber")
                        truckFound = item.Value == "Test4";
                }
            }
            Assert.True(shipFound);
            Assert.True(truckFound);
        }
    }
}