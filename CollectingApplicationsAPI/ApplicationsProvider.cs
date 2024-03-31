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
using CollectingApplicationsAPI.Model;

namespace CollectingApplicationsAPI.Model
{
    public class ApplicationsProvider : IApplicationProvider
    {
        const string connectionString = "ConnectionStrings:ConnectionString";

        const string selectAllQuery = "SELECT json_agg(applications) AS json_data FROM (SELECT \"Applications\".\"id\", \"Applications\".\"Author\", \"Applications\".\"Activity\",\"Applications\".\"Name\",\"Applications\".\"Description\", \"Applications\".\"Outline\" FROM \"Applications\") AS applications;";
        const string createApplicationQuery = "INSERT INTO \"Applications\" (\"id\", \"Author\", \"Activity\",\"Name\", \"Description\", \"Outline\", \"Status\", \"EditTime\") VALUES(@id, @Author, @Activity, @Name, @Description, @Outline, @Status, @EditTime)";
        const string deleteApplicationQuery = "DELETE FROM \"Applications\" WHERE \"id\" = @id AND NOT EXISTS (SELECT 1 FROM \"Applications\" WHERE \"Applications\".\"id\" = @id AND \"Applications\".\"Status\" = 'Submitted')";
        const string updateApplicationQuery = "UPDATE \"Applications\" AS a SET \"Activity\" = @Activity, \"Name\" = @Name, \"Description\" = @Description, \"Outline\" = @Outline, \"EditTime\" = @EditTime  FROM \"Applications\" AS s WHERE a.\"id\" = @id AND s.\"id\" = a.\"id\" AND s.\"Status\" = 'Unsubmitted' RETURNING a.*;";
        const string submitApplicationQuery = "UPDATE \"Applications\" SET \"Status\" = 'Submitted' WHERE \"id\" = @id AND \"Status\" != 'Submitted'";
        const string getApplicationByUuidQuery = "SELECT json_agg(applications) AS json_data FROM (SELECT \"Applications\".\"id\", \"Applications\".\"Author\", \"Applications\".\"Activity\", \"Applications\".\"Name\", \"Applications\".\"Description\",\"Applications\".\"Outline\" FROM \"Applications\" WHERE \"Applications\".\"id\" = @id) AS applications;\r\n";
        const string getActivitiesQuery = "SELECT json_agg(\"Activities\") FROM \"Activities\"";
        const string getApplicationsSubmittedAfterQuery = "SELECT json_agg(applications) AS json_data FROM (SELECT \"Applications\".\"id\", \"Applications\".\"Author\", \"Applications\".\"Activity\", \"Applications\".\"Name\", \"Applications\".\"Description\",\"Applications\".\"Outline\" FROM \"Applications\" WHERE  \"Applications\".\"Status\" = 'Submitted' AND \"Applications\".\"EditTime\" > @EditTime ) AS applications;";
        const string getApplicationsUnsubmittedOlderQuery = "SELECT json_agg(applications) AS json_data FROM (SELECT \"Applications\".\"id\", \"Applications\".\"Author\", \"Applications\".\"Activity\", \"Applications\".\"Name\", \"Applications\".\"Description\",\"Applications\".\"Outline\" FROM \"Applications\" WHERE  \"Applications\".\"Status\" = 'Unsubmitted' AND \"Applications\".\"EditTime\" < @EditTime ) AS applications;";
        const string getUnsubmittedApplicationsFromUser = "SELECT json_agg(applications) AS json_data FROM ( SELECT \"Applications\".\"id\", \"Applications\".\"Author\", \"Applications\".\"Activity\", \"Applications\".\"Name\",  \"Applications\".\"Description\", \"Applications\".\"Outline\" FROM \"Applications\"  WHERE  \"Applications\".\"Status\" = 'Unsubmitted' AND \"Applications\".\"Author\" = @Author LIMIT 1) AS applications;";

        const string getNumberOfUnsubmittedApplicationsFromUser = "SELECT COUNT(*) FROM \"Applications\" WHERE \"Applications\".\"Status\" = 'Unsubmitted' AND \"Applications\".\"Author\" = @Author";

        private readonly IConfiguration _configuration;

        public ApplicationsProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<DbResponse> CreateApplication(Application application)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var unsubmittedCount = await connection.QueryAsync<int>(getNumberOfUnsubmittedApplicationsFromUser, new { Author = application.Author });

            if (unsubmittedCount == null)
            {
                return new DbResponse() { Status = HttpStatusCode.NotFound };
            }

            if (unsubmittedCount.First() == 0) 
            {
                application.Id = Guid.NewGuid();

                var affected = await connection.ExecuteAsync(createApplicationQuery,
                              new { application.Id, application.Author, application.Activity, application.Name, application.Description, application.Outline, application.Status, application.EditTime});

                if (affected == 0)
                {
                    return new DbResponse() { Status = HttpStatusCode.Conflict };
                }

                return new DbResponse() { Status = HttpStatusCode.OK, Data = application };
            }

            return new DbResponse() { Status = HttpStatusCode.BadRequest, Data = "Unsubmitted applications already exist" };
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
                           new { application.Author, application.Activity, application.Name, application.Description, application.Outline, application.Id, application.Status, application.EditTime });

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

        public async Task<DbResponse> GetUsersUnsubmittedApplication(Guid author)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>(connectionString));

            var applications = await connection.QueryAsync<string>(getUnsubmittedApplicationsFromUser,
                new { Author = author });

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
