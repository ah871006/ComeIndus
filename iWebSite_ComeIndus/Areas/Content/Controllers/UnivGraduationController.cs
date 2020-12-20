﻿using iWebSite_ComeIndus.Controllers;
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
            return View("GraduationFromDiffYear");
        }

        public IActionResult DiffCountry()
        {
            return View("GraduationFromDiffCountry");
        }

        public IActionResult DiffYear()
        {
            return View("GraduationFromDiffYear");
        }

        //--------------------------

        /// <summary>
        /// GetDB 畢業人數
        /// </summary>
        /// <param name="countryNo"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private CountryGradModel getGradData(string countryNo="-1", string year="0") 
        {
            var sqlStr = string.Format(
                    "select [DeptName], [GraduationNumber], [GraduationYear] " +
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

            var countryGradData = _DB_GetData(sqlStr);

            CountryGradModel model = new CountryGradModel();

            foreach (DataRow gradRow in countryGradData.Rows)
            {
                model.DeptName.Add(gradRow.ItemArray.GetValue(0).ToString());
                model.GraduationNumber.Add((int)gradRow.ItemArray.GetValue(1));
            }

            return model;
        }

        /// <summary>
        /// 同年不同國
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet()]
        public Dictionary<string, CountryGradModel> GraduationFromDiffCountry(string year="0")
        {
            var sqlStr = string.Format("select [CountryNo], [CountryName] from Countries");
            var data = _DB_GetData(sqlStr);

            Dictionary<string, CountryGradModel> graduationData = new Dictionary<string, CountryGradModel>();

            foreach (DataRow row in data.Rows)
            {
                string countryNo = row.ItemArray.GetValue(0).ToString();
                string countryName = row.ItemArray.GetValue(1).ToString();

                graduationData[countryName] = getGradData(countryNo, year);
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
        public CountryGradModel GraduationFromDiffYear(string year = "0", string country="")
        {

            var sqlStr = string.Format("select [CountryNo], [CountryName] from Countries where CountryName = {0}", SqlVal2(country));
            var data = _DB_GetData(sqlStr);

            foreach (DataRow row in data.Rows)
            {
                string countryNo = row.ItemArray.GetValue(0).ToString();
                string countryName = row.ItemArray.GetValue(1).ToString();

                return getGradData(countryNo, year);
            }

            return new CountryGradModel();
        }

        /// <summary>
        /// 下載
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet()]
        public IActionResult Download(string Year = "-1", string Country = "-1")
        {
            //~/Content/UnivGraduation/Download?Year=2011,2012&Country=1,8
            return RedirectToAction("UnivGraduation", "excel", new UnivGraduationModel() { Year = Year, Country = Country});
        }
    }
}
