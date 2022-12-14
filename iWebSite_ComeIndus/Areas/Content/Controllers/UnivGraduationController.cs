using iWebSite_ComeIndus.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iWebSite_ComeIndus.Areas.Content.Models;
using System.Data;

namespace iWebSite_ComeIndus.Areas.Content.Controllers
{
    [Area(areaName: "Content")]

    public class UnivGraduationController : _BaseController
    {
        public IActionResult Index()
        {
            //return View("ShowUnivGraduation");
            //return View("GraduationFromDiffYear");
            return DiffYear();
        }

        public IActionResult DiffCountry()
        {
            //目前登入權限
            var StatusNo = getUserStatusNo();

            //權限
            ViewData["StatusNo"] = StatusNo;
            return View("GraduationFromDiffCountry", getTime());
        }

        public IActionResult DiffYear()
        {
            //目前登入權限
            var StatusNo = getUserStatusNo();

            //權限
            ViewData["StatusNo"] = StatusNo;
            return View("GraduationFromDiffYear", getTime());
        }

       
        //--------------------------

        private List<TimeModel> getTime()
        {
            var sqlStr = string.Format("select [GraduationYear] from GraduationYear");
            var data = _DB_GetData(sqlStr);

            List<TimeModel> years = new List<TimeModel>();
            foreach (DataRow Row in data.Rows)
            {
                TimeModel model = new TimeModel();
                model.Year = Row.ItemArray.GetValue(0).ToString();

                years.Add(model);
            }

            return years;
        }

        /// <summary>
        /// GetDB 畢業人數
        /// </summary>
        /// <param name="countryNo"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private DataTable getGradData(string countryNo="-1", string year="0") 
        {
            string sqlStr = "";

            if(countryNo == "-1")
            {
                sqlStr = string.Format(
                    "select [CountryNo], [DeptName], [GraduationNumber], [GraduationYear]" +
                    "from[dbo].[Department] as a " +
                    "inner join( " +
                    "select[CountryDeptNo],[CountryNo], [DeptNo] " +
                    "from[dbo].[CountryDepartment] " +
                    ") as b " +
                    "on b.DeptNo = a.DeptNo " +
                    "inner join( " +
                    "select * " +
                    "from[dbo].[Graduation] " +
                    "where GraduationYear = {0} " +
                    ") as c " +
                    "on b.CountryDeptNo = c.CountryDeptNo", SqlVal2(year));
            }
            else
            {
                sqlStr = string.Format(
                    "select [CountryNo], [DeptName], [GraduationNumber], [GraduationYear]" +
                    "from[dbo].[Department] as a " +
                    "inner join( " +
                    "select[CountryDeptNo],[CountryNo], [DeptNo] " +
                    "from[dbo].[CountryDepartment] " +
                    "where CountryNo = {0} " +
                    ") as b " +
                    "on b.DeptNo = a.DeptNo " +
                    "inner join( " +
                    "select * " +
                    "from[dbo].[Graduation] " +
                    "where GraduationYear = {1} " +
                    ") as c " +
                    "on b.CountryDeptNo = c.CountryDeptNo", SqlVal2(countryNo), SqlVal2(year));
            }
            

            return _DB_GetData(sqlStr);

            /*
            CountryDeptModel model = new CountryDeptModel();

            foreach (DataRow gradRow in countryGradData.Rows)
            {
                model.DeptName.Add(gradRow.ItemArray.GetValue(0).ToString());
                model.GraduationNumber.Add((int)gradRow.ItemArray.GetValue(1));
            }

            return model;
            */
        }

        /// <summary>
        /// 同年不同國
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet()]
        public Dictionary<string, CountryDeptModel> GraduationFromDiffCountry(string year="0")
        {
            
            var data = getGradData("-1", year);
            Dictionary<string, CountryDeptModel> graduationData = new Dictionary<string, CountryDeptModel>();

            foreach (DataRow row in data.Rows)
            {
                string countryNo = row.ItemArray.GetValue(0).ToString();
                
                if(!graduationData.ContainsKey(countryNo))
                {
                    graduationData[countryNo] = new CountryDeptModel();
                }
               
                graduationData[countryNo].DeptName.Add(row.ItemArray.GetValue(1).ToString());
                graduationData[countryNo].GraduationNumber.Add((int)row.ItemArray.GetValue(2));
            }

            return graduationData; 
        }

        /// <summary>
        /// 同國不同年
        /// </summary>
        /// <param name="year"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        [HttpGet()]
        public CountryDeptModel GraduationFromDiffYear(string year = "0", string country="")
        {
            
            CountryDeptModel model = new CountryDeptModel();
            
            string countryNo = country;
            var gradData = getGradData(countryNo, year);
            
            foreach (DataRow gradRow in gradData.Rows)
            {
                model.DeptName.Add(gradRow.ItemArray.GetValue(1).ToString());
                model.GraduationNumber.Add((int)gradRow.ItemArray.GetValue(2));
            }
            
            model.CountryNo = countryNo;
            

            return model;
        }

        /// <summary>
        /// 下載
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet()]
        public IActionResult Download(string Year = "-1", string Country = "-1")
        {
            //是否存在cookies(登入且是會員才可以下載Excel)
            if (!string.IsNullOrEmpty(Request.Cookies["account"]) && !string.IsNullOrEmpty(Request.Cookies["userName"]))
            {
                //SQL Insert Member
                var sqlStr = string.Format("select StatusNo from [dbo].[Member] where Account = {0}", SqlVal2(Request.Cookies["account"]));

                //SQL Check
                var data = _DB_GetData(sqlStr);

                //資料庫內是否有此帳號
                if (data.Rows.Count > 0)
                {
                    //~/Content/UnivGraduation/Download?Year=2011,2012&Country=1,8
                    return RedirectToAction("UnivGraduation", "excel", new UnivGraduationModel() { Year = Year, Country = Country });
                }
                else
                {
                    return Redirect("~/Home/Error");
                }
            }
            else
            {
                return Redirect("~/Home/Error");
            }
            
            //return RedirectToAction("UnivGraduation", "excel", new UnivGraduationModel() { Year = Year, Country = Country});
        }
    }
}
