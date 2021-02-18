using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class WebhooksController : ApiController
    {
        
        [HttpGet]
        public HttpResponseMessage Get()
        {
            /*try { */
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(HttpContext.Current.Request.QueryString["hub.challenge"])
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            return response;
            /*}*/
            /*catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, " Data Not Found");
                
            }*/
        }
        

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] JsonData data)
        {
            try
            {
                var entry = data.Entry.FirstOrDefault();
                var change = entry?.Changes.FirstOrDefault();
                if (change == null) return new HttpResponseMessage(HttpStatusCode.BadRequest);

                //Generate user access token here https://developers.facebook.com/tools/accesstoken/
                const string token = "123";

                var leadUrl = $"https://graph.facebook.com/v9.0/{change.Value.LeadGenId}?access_token={token}";
                var formUrl = $"https://graph.facebook.com/v9.0/{change.Value.FormId}?access_token={token}";

                using (var httpClientLead = new HttpClient())
                {
                    var response = await httpClientLead.GetStringAsync(formUrl);
                    if (!string.IsNullOrEmpty(response))
                    {
                        var jsonObjLead = JsonConvert.DeserializeObject<LeadFormData>(response);
                        //jsonObjLead.Name contains the lead ad name

                        //If response is valid get the field data
                        using (var httpClientFields = new HttpClient())
                        {
                            var responseFields = await httpClientFields.GetStringAsync(leadUrl);
                            if (!string.IsNullOrEmpty(responseFields))
                            {
                                var jsonObjFields = JsonConvert.DeserializeObject<LeadData>(responseFields);
                                //jsonObjFields.FieldData contains the field value
                            }
                        }
                    }
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error-->{ex.Message}");
                Trace.WriteLine($"StackTrace-->{ex.StackTrace}");
                return new HttpResponseMessage(HttpStatusCode.BadGateway);
            }
        }

       
    }
}