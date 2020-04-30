﻿/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class MaitreDTests
    {
        [Theory]
        [InlineData(new[]    { 12 },  new int[0])]
        [InlineData(new[] { 8, 11 },  new int[0])]
        [InlineData(new[] { 2, 11 }, new[] { 2 })]
        public void Accept(int[] tableSeats, int[] reservedSeats)
        {
            var tables =
                tableSeats.Select(s => new Table(TableType.Communal, s));
            var sut = new MaitreD(tables);

            var rs = reservedSeats.Select(Some.Reservation.WithQuantity);
            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(rs, r);

            Assert.True(actual);
        }

        [Fact]
        public void Reject()
        {
            var sut = new MaitreD(
                new Table(TableType.Communal, 6),
                new Table(TableType.Communal, 6));

            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(Array.Empty<Reservation>(), r);

            Assert.False(actual);
        }
    }
}