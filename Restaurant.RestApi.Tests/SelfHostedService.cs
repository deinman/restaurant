﻿/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class SelfHostedService : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReservationsRepository>();
                services.AddSingleton<IReservationsRepository>(
                    new FakeDatabase());

                services.RemoveAll<CalendarFlag>();
                services.AddSingleton(new CalendarFlag(true));
            });
        }

        [SuppressMessage(
            "Usage",
            "CA2234:Pass system uri objects instead of strings",
            Justification = "URL isn't passed as variable, but as literal.")]
        public async Task<HttpResponseMessage> PostReservation(
            object reservation)
        {
            var client = CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PostAsync("reservations", content);
        }

        public async Task<HttpResponseMessage> PutReservation(
            Uri address,
            object reservation)
        {
            var client = CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PutAsync(address, content);
        }

        public async Task<HttpResponseMessage> GetCurrentYear()
        {
            var yearAddress = await FindAddress("urn:year");
            return await CreateClient().GetAsync(yearAddress);
        }

        public async Task<HttpResponseMessage> GetCurrentMonth()
        {
            var monthAddress = await FindAddress("urn:month");
            return await CreateClient().GetAsync(monthAddress);
        }

        public async Task<HttpResponseMessage> GetCurrentDay()
        {
            var dayAddress = await FindAddress("urn:day");
            return await CreateClient().GetAsync(dayAddress);
        }

        private async Task<Uri> FindAddress(string rel)
        {
            var client = CreateClient();

            var homeResponse =
                await client.GetAsync(new Uri("", UriKind.Relative));
            homeResponse.EnsureSuccessStatusCode();
            var homeRepresentation =
                await homeResponse.ParseJsonContent<HomeDto>();
            var address =
                homeRepresentation.Links.Single(l => l.Rel == rel).Href;
            if (address is null)
                throw new InvalidOperationException(
                    $"Address for relationship typ {rel} not found.");

            return new Uri(address);
        }
    }
}