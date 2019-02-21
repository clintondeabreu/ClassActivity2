using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Assignment2.Models;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using Assignment2.Reports;

namespace Assignment2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Advanced()
        {
            AdvancedVM vm = new AdvancedVM();

            vm.Departments = GetDepartments(0);

            vm.DateFrom = new DateTime(1976, 01, 01); //1976-01-01
            vm.DateTo = new DateTime(2011, 12, 31);
            vm.counter = 0;
            return View(vm);
        }

        private SelectList GetDepartments(int selected)
        {
            using (HardwareDBEntities db = new HardwareDBEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;

                var departments = db.lgdepartments.Select(x => new SelectListItem
                {
                    Value = x.dept_num.ToString(),
                    Text = x.dept_name
                }).ToList();

                if (selected == 0)
                    return new SelectList(departments, "Value", "Text");
                else
                    return new SelectList(departments, "Value", "Text", selected);
            }
        }


        [HttpPost]
        public ActionResult Advanced(AdvancedVM vm)
        {
            using (HardwareDBEntities db = new HardwareDBEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                List<DepartmentEmployees> empList = new List<DepartmentEmployees>();

                foreach (var employee in db.lgemployees.Where(emp => vm.SelectedDepartmentID == emp.dept_num && vm.DateFrom <= emp.emp_hireDATETIME && vm.DateTo >= emp.emp_hireDATETIME))
                {
                    empList.Add(new DepartmentEmployees
                    {
                        EmployeeFName = employee.emp_fname,
                        EmployeeSName = employee.emp_lname,
                        EmployeeEmail = employee.emp_email,
                        EmployeeTitle = employee.emp_title,
                        EmplyeeID = employee.emp_num,
                        deptNum = employee.dept_num.Value
                    });
                }

                vm.Departments = GetDepartments(vm.SelectedDepartmentID);
                vm.department = db.lgdepartments.Where(x => x.dept_num == vm.SelectedDepartmentID).FirstOrDefault();
                //vm.department = db.dep.Where(x => x.BusinessEntityID == vm.SelectedVendorID).FirstOrDefault();

                vm.depEmployess = empList;
                vm.results = empList.GroupBy(g => g.EmployeeTitle).ToList();
                empList.ToList();

                vm.chartData = empList.GroupBy(g => g.EmployeeTitle).ToDictionary(g => g.Key, g => (double)g.Sum(v => v.deptNum));
                vm.counter = empList.Count();

                TempData["chartData"] = vm.chartData;
                TempData["records"] = empList.ToList();
                TempData["department"] = vm.department;

                return View(vm);
            }
        }


        public ActionResult EmployeeDepartmentChart()
        {
            var data = TempData["chartData"];
            return View(TempData["chartData"]);
        }

        private DepartmentEmployeeSum GetAdvancedDataSet()
        {
            DepartmentEmployeeSum data = new DepartmentEmployeeSum();

            data.Department.Clear();
            data.Employee.Rows.Clear();

            ////Add table (with only one record) to dataset for general vendor details to be shown on Crystal Report
            DataRow vrow = data.Department.NewRow();
            lgdepartment dept = (lgdepartment)TempData["Department"];
            vrow["Department_Num"] = dept.dept_num;
            vrow["Department_Name"] = dept.dept_name;
            vrow["Department_Phone"] = dept.dept_phone;
            data.Department.Rows.Add(vrow);

            ////Add table to dataset for general vendor details to be shown on Crystal Report
            foreach (var item in (IEnumerable<DepartmentEmployees>)TempData["records"])
            {
                DataRow row = data.Employee.NewRow();
                row["Emp_ID"] = item.EmplyeeID;
                row["Emp_FName"] = item.EmployeeFName;
                row["Emp_SName"] = item.EmployeeSName;
                row["Emp_DeptID"] = item.deptNum;
                row["Emp_Title"] = item.EmployeeTitle;
                data.Employee.Rows.Add(row);
            }

            //Reset TempData so that it can be accessed in upcoming calls to controller actions
            TempData["chartData"] = TempData["chartData"];
            TempData["records"] = TempData["records"];
            TempData["Department"] = TempData["Department"];
            return data;
        }

        public ActionResult ExportAdvancedPDF()
        {
            ReportDocument report = new ReportDocument();
            report.Load(Path.Combine(Server.MapPath("~/Reports/CrystalDepartment.rpt")));
            report.SetDataSource(GetAdvancedDataSet());
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf", "CrystalDepartment.pdf");
        }

        public ActionResult ExportAdvancedWord()
        {
            ReportDocument report = new ReportDocument();
            report.Load(Path.Combine(Server.MapPath("~/Reports/CrystalDepartment.rpt")));
            report.SetDataSource(GetAdvancedDataSet());
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.WordForWindows);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/msword", "CrystalDepartment.doc");
        }
    }
}