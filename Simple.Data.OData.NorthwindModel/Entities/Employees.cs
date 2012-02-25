using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("EmployeeID")]
    public class Employees
    {
        private int? _reportsTo;

        public int EmployeeID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string TitleOfCourtesy { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? HireDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string HomePhone { get; set; }
        public string Extension { get; set; }
        public byte[] Photo { get; set; }
        public string Notes { get; set; }
        public int? ReportsTo
        {
            get { return _reportsTo; }
            set { this.Superior = NorthwindContext.Instance.SetEmployeeSuperior(this, value); _reportsTo = value; }
        }
        public string PhotoPath { get; set; }

        public Employees Superior { get; set; }
        public ICollection<Employees> Subordinates { get; private set; }
        public ICollection<Orders> Orders { get; private set; }
        public ICollection<Territories> Territories { get; private set; }

        public Employees()
        {
            this.Subordinates = new List<Employees>();
            this.Orders = new List<Orders>();
            this.Territories = new List<Territories>();
        }
    }
}