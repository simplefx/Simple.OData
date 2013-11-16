using System;
using System.Collections.Generic;

namespace Simple.OData.Client.Tests
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime HireDate { get; set; }

        public Employee Superior { get; set; }
        public Employee[] Subordinates { get; set; }
    }
}