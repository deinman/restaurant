﻿/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [ApiController, Route("calendar")]
    public class CalendarController
    {
        private readonly Table table;

        public CalendarController(Table table)
        {
            this.table = table;
        }

        [HttpGet("{year}")]
        public ActionResult Get(int year)
        {
            var daysInYear = new GregorianCalendar().GetDaysInYear(year);
            var firstDay = new DateTime(year, 1, 1);
            var days = Enumerable.Range(0, daysInYear)
                .Select(i => MakeDay(firstDay, i))
                .ToArray();
            return new OkObjectResult(
                new CalendarDto { Year = year, Days = days });
        }

        [HttpGet("{year}/{month}")]
        public ActionResult Get(int year, int month)
        {
            var daysInMonth =
                new GregorianCalendar().GetDaysInMonth(year, month);
            var firstDay = new DateTime(year, month, 1);
            var days = Enumerable.Range(0, daysInMonth)
                .Select(i => MakeDay(firstDay, i))
                .ToArray();
            return new OkObjectResult(
                new CalendarDto
                {
                    Year = year,
                    Month = month,
                    Days = days
                });
        }

        [HttpGet("{year}/{month}/{day}")]
#pragma warning disable CA1801 // Review unused parameters
        public ActionResult Get(int year, int month, int day)
#pragma warning restore CA1801 // Review unused parameters
        {
            return new OkObjectResult(
                new CalendarDto
                {
                    Year = DateTime.Now.Year,
                    Month = DateTime.Now.Month,
                    Day = DateTime.Now.Day
                });
        }

        private DayDto MakeDay(DateTime origin, int days)
        {
            return new DayDto
            {
                Date = origin.AddDays(days)
                    .ToString("o", CultureInfo.InvariantCulture),
                MaximumPartySize = table.Seats
            };
        }
    }
}
