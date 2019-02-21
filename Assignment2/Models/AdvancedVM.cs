using Assignment2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Assignment2.Models
{
    public class AdvancedVM
    {
        //Fields for report criteria
        public IEnumerable<SelectListItem> Departments { get; set; }
        public int SelectedDepartmentID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int counter { get; set; }

        //Fields for report data
        public lgdepartment department { get; set; }
        public List<IGrouping<string, DepartmentEmployees>> results { get; set; }
        public Dictionary<string, double> chartData { get; set; }
        public List<DepartmentEmployees> depEmployess { get; set; }
    }
    public class DepartmentEmployees
    {
        public string EmployeeFName { get; set; }
        public string EmployeeSName { get; set; }
        public string EmployeeTitle { get; set; }
        public string EmployeeEmail { get; set; }
        public decimal EmplyeeID { get; set; }
        public decimal deptNum { get; set; }

    }


}