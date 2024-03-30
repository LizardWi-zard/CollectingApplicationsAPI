using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;
using System.Xml.Linq;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System;

namespace CollectingApplicationsAPI
{
    public class ApplicationsCaller : IGetApplication
    {
        const string connectionString = "ConnectionStrings:ConnectionString";

        const string selectAllQuery = "SELECT json_agg(\"Applications\") FROM \"Applications\"";

        const string createApplicationQuery = "INSERT INTO \"Applications\" (\"id\", \"Author\", \"Activity\",\"Name\", \"Description\", \"Outline\") " +
            "VALUES(@id, @Author, @Activity, @Name, @Description, @Outline)";

        const string createApplicationStatusQuery = "INSERT INTO \"ApplicationsStatus\" (\"id\", \"Status\", \"EditTime\") VALUES (@id, @Status, @EditTime)";
        const string deleteApplicationQuery = "DELETE FROM \"Applications\"  \"ApplicationsStatus\" WHERE \"id\" = @id AND NOT EXISTS (SELECT 1 FROM \"ApplicationsStatus\" WHERE \"ApplicationsStatus\".\"id\" = @id AND \"ApplicationsStatus\".\"Status\" = 'Submitted')";
        const string updateApplicationQuery = "UPDATE \"Applications\" AS a SET \"Activity\" = @Activity, \"Name\" = @Name, \"Description\" = @Description, \"Outline\" = @Outline FROM \"ApplicationsStatus\" AS s WHERE a.\"id\" = @id AND s.\"id\" = a.\"id\" AND s.\"Status\" = 'Unsubmitted' RETURNING a.*;";

        const string submitApplicationQuery = "UPDATE \"ApplicationsStatus\" SET \"Status\" = 'Submitted' WHERE \"id\" = @id AND \"Status\" != 'Submitted'";
        const string getApplicationByUuidQuery = "SELECT json_agg(\"Applications\") FROM \"Applications\" WHERE \"id\" = @id";
        const string getActivitiesQuery = "SELECT json_agg(\"Activities\") FROM \"Activities\"";
        const string getApplicationsSubmittedAfterQuery = "SELECT json_agg(\"Applications\") FROM \"Applications\" WHERE \"id\" IN (SELECT \"id\" FROM \"ApplicationsStatus\" WHERE \"Status\" = 'Submitted' AND \"EditTime\" > @EditTime)";
        const string getApplicationsUnsubmittedOlderQuery = "SELECT json_agg(\"Applications\") FROM \"Applications\" WHERE \"id\" IN (SELECT \"id\" FROM \"ApplicationsStatus\" WHERE \"Status\" = 'Unsubmitted' AND \"EditTime\" < @EditTime)";
        const string getUsersUnsubmittedApplicationQuery = "SELECT json_agg(\"Applications\") FROM \"Applications\" WHERE \"id\" IN (SELECT \"id\" FROM \"ApplicationsStatus\" WHERE \"Status\" = 'Unsubmitted' AND \"id\" = @id)";

        private readonly IConfiguration _configuration;

        public ApplicationsCaller(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<DbResponse> CreateApplication(Application application)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            application.Id = Guid.NewGuid();

            var affected = await connection.ExecuteAsync(createApplicationQuery,
                          new { application.Id, application.Author, application.Activity, application.Name, application.Description, application.Outline });

            if (affected == 0)
            {
                return new DbResponse() { Status = HttpStatusCode.Conflict };
            }

            affected = await connection.ExecuteAsync(createApplicationStatusQuery,
                         new { application.Id, Status = "Unsubmitted", EditTime = DateTime.Now });

            if (affected == 0)
            {
                return new DbResponse() { Status = HttpStatusCode.Conflict };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = application };
        }

        public async Task<DbResponse> DeleteApplication(Guid id)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var affected = await connection.ExecuteAsync(deleteApplicationQuery,
            new { id = id });

            if (affected == 0)
            {
                return new DbResponse() { Status = HttpStatusCode.Conflict };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = true };
        }

        public async Task<DbResponse> EditApplication(Guid id, Application application)
        {

            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            application.Id = id;

            var affected = await connection.ExecuteAsync(updateApplicationQuery,
                           new { application.Author, application.Activity, application.Name, application.Description, application.Outline, application.Id });

            if (affected == 0)
            {
                return new DbResponse() { Status = HttpStatusCode.Conflict };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = application };

        }

        public async Task<DbResponse> GetActivities()
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var activities = await connection.QueryAsync<string>(getActivitiesQuery);

            if (activities == null)
            {
                return new DbResponse() { Status = HttpStatusCode.NotFound };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = activities.ToArray()[0] };
        }

        public class Activities
        {
            public Activities(string act, string desc)
            {
                Activity = act;
                Description = desc;

            }

            public string Activity { get; set; }

            public string Description { get; set; }
        }

        public async Task<DbResponse> GetAllApplications()
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var applications = await connection.QueryAsync<string>(selectAllQuery);

            if (applications == null)
            {
                return new DbResponse() { Status = HttpStatusCode.NotFound };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = applications.ToArray()[0] };
        }

        public async Task<DbResponse> GetApplicationById(Guid id)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var applications = await connection.QueryAsync<string>(getApplicationByUuidQuery,
                new {id});


            if (applications == null)
            {
                return new DbResponse() { Status = HttpStatusCode.NotFound };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = applications.ToArray()[0] };
        }

        public async Task<DbResponse> GetApplicationsSubmittedAfter(DateTime dateTime)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var applications = await connection.QueryAsync<string>(getApplicationsSubmittedAfterQuery,
               new { EditTime = dateTime });

            if (applications == null)
            {
                return new DbResponse() { Status = HttpStatusCode.NotFound };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = applications.ToArray()[0] };
        }

        public async Task<DbResponse> GetApplicationsUnsubmittedOlder(DateTime dateTime)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var applications = await connection.QueryAsync<string>(getApplicationsUnsubmittedOlderQuery,
               new { EditTime = dateTime });

            if (applications == null)
            {
                return new DbResponse() { Status = HttpStatusCode.NotFound };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = applications.ToArray()[0] };
        }

        public async Task<DbResponse> GetUsersUnsubmittedApplication(Guid id)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var applications = await connection.QueryAsync<string>(getUsersUnsubmittedApplicationQuery,
                new { id = id });


            if (applications == null)
            {
                return new DbResponse() { Status = HttpStatusCode.NotFound };
            }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = applications.ToArray()[0] };
        }

        public async Task<DbResponse> SubmittingApplication(Guid id)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var affected = await connection.ExecuteAsync(submitApplicationQuery,
               new { id });

           if (affected == 0)
           {
               return new DbResponse() { Status = HttpStatusCode.Conflict };
           }

            return new DbResponse() { Status = HttpStatusCode.OK, Data = true };
        }

    }
}
