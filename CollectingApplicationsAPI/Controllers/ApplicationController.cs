using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CollectingApplicationsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly IGetApplication _getApplication;

        public ApplicationController(ILogger<ApplicationController> logger, IGetApplication getApplication)
        {
            _logger = logger;
            _getApplication = getApplication ?? throw new ArgumentNullException(nameof(getApplication));
        }

  /*      [HttpGet]
        public async Task<ActionResult<string>> GetAllApplications()
        {
            var response = await _getApplication.GetAllApplications();

            string output = (string)response.Data;

            return output;
        }*/

        [HttpGet("/Activities")]
        public async Task<ActionResult<string>> GetActivities()
        {
            var response = await _getApplication.GetActivities();

            string output = (string)response.Data;

            return output;
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreateApplication([FromBody] string json)
        {
            var application = new Application();

            try
            {
                application = JsonConvert.DeserializeObject<Application>(json);
            }
            catch(Exception ex)
            {
                return HttpStatusCode.BadRequest.ToString();
            }

            var correctGuid = Guid.NewGuid();
            var isGuid = Guid.TryParse(application?.Author.ToString(), out correctGuid);
            var correctActivity = (application.Activity != null && application.Activity == "Report" || application.Activity == "Masterclass" || application.Activity == "Discussion"); ;
            var correctName = (application.Name != null && application.Name.Length != 0 && application.Name.Length < 100);
            var correctDescription = (application.Description != null  && application.Description.Length < 300);
            var correctOutline = (application.Outline != null && application.Outline.Length != 0 && application.Outline.Length < 1000);

            DbResponse response = new DbResponse();

            if (isGuid && correctActivity && correctName && correctDescription && correctOutline)
            {
                if (application != null)
                {
                    response = await _getApplication.CreateApplication(application);
                }
            }
            else
            {
                return HttpStatusCode.BadRequest.ToString();
            }

            var output = response.Data;
           
            return JsonConvert.SerializeObject(output);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<string>> EditApplication([FromRoute] string id, [FromBody] string json)
        {
            var application = new Application();

            try
            {
                application = JsonConvert.DeserializeObject<Application>(json);
            }
            catch (Exception ex)
            {
                return HttpStatusCode.BadRequest.ToString();
            }

            var correctGuid = Guid.NewGuid();
            var isGuid = Guid.TryParse(application?.Author.ToString(), out correctGuid);
            var correctActivity = (application.Activity != null && application.Activity == "Report" || application.Activity == "Masterclass" || application.Activity == "Discussion"); ;
            var correctName = (application.Name != null && application.Name.Length != 0 && application.Name.Length < 100);
            var correctDescription = (application.Description != null && application.Description.Length < 300);
            var correctOutline = (application.Outline != null && application.Outline.Length != 0 && application.Outline.Length < 1000);

            DbResponse response = new DbResponse();

            if (isGuid && correctActivity && correctName && correctDescription && correctOutline)
            {
                if (application != null)
                {   
                    response = await _getApplication.EditApplication(Guid.Parse(id), application);
                }
            }
            else
            {
                return HttpStatusCode.BadRequest.ToString();
            }

            var output = response.Data;

            return JsonConvert.SerializeObject(output);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<HttpStatusCode>> DeleteDocument(string id)
        {
            var response = await _getApplication.DeleteApplication(Guid.Parse(id));

            return response.Status;
        }

        [HttpPost("{id}/submit")]
        public async Task<ActionResult<string>> SubmittingApplication(string id)
        {
            var isGuidParsed = Guid.TryParse(id, out var guid);

            if (!isGuidParsed)
            {
                return HttpStatusCode.BadRequest.ToString();
            }

            var response = await _getApplication.SubmittingApplication(guid);

            return response?.Data?.ToString() ?? HttpStatusCode.NotFound.ToString();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> GetApplicationById(string id)
        {
            var isGuidParsed = Guid.TryParse(id, out var guid);

            if (!isGuidParsed)
            {
                return HttpStatusCode.BadRequest.ToString();
            }

            var response = await _getApplication.GetApplicationById(guid);

            return response?.Data?.ToString() ?? HttpStatusCode.NotFound.ToString();
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetApplicationsSubmittedAfter([FromQuery] string? submittedAfter, [FromQuery] string? unsubmittedOlder)
        {
            var response = new DbResponse();
            string output;

            if (submittedAfter != null && unsubmittedOlder != null)
            {
                return "Unable to work with two filters";
            }
            if (submittedAfter != null && unsubmittedOlder == null)
            {
                response = await _getApplication.GetApplicationsSubmittedAfter(DateTime.Parse(submittedAfter));
            } 
            else if(unsubmittedOlder != null && submittedAfter == null)
            {
                response = await _getApplication.GetApplicationsUnsubmittedOlder(DateTime.Parse(unsubmittedOlder));
            }

            if (response.Status == HttpStatusCode.OK && response.Data == null)
            {
                return "{ }";
            }
            
            output = response.Data.ToString();

            return output;
        }

        [HttpGet("/users/{id}/currentapplication")]
        public async Task<ActionResult<string>> GetUsersUnsubmittedApplication(string id)
        {
            var response = await _getApplication.GetUsersUnsubmittedApplication(Guid.Parse(id));

            string output = response.Data.ToString();

            return output;
        }
    }
}
