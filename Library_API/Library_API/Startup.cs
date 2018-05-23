﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Services;
using Library_API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Library_API
{
    public class Startup
    {
        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ////services.AddMvc();
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
                // setupAction.InputFormatters.Add(new XmlSerializerInputFormatter());
                 // we can use xml serializer but it dosent support datetime n various other property, we have to handle it
            });



            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
        ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            ////app.Run(async (context) =>
            ////{
            ////    await context.Response.WriteAsync("Hello World!");
            ////});

            AutoMapper.Mapper.Initialize(con =>
            {
                con.CreateMap<Author, AuthorDto>()
                  .ForMember("Name", dest => dest.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                  .ForMember(dest => dest.Age, option => option.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

                con.CreateMap<Book, BookDto>();

                con.CreateMap<AuthorCreationDto, Author>();
                con.CreateMap<BookCreationDto, Book>();
                con.CreateMap<BookUpdationDto, Book>();
                con.CreateMap<Book, BookUpdationDto>();
            });


            libraryContext.EnsureSeedDataForContext();

            app.UseMvc();
        }
    }
}
