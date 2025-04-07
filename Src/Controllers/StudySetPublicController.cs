using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Src.Services;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudySetPublicController : ControllerBase
    {
        protected readonly SetPublicService service;
        public StudySetPublicController(IMongoClient mongoClient, IConfiguration config)
        {
            service = new(mongoClient, config);
        }

        [HttpGet]
        public ActionResult<List<StudySetPublic>> Find(string keyword)
        {
            var result = service.FindByName(keyword);

            return new ActionResult<List<StudySetPublic>>(result);
        }

        [HttpGet]
        public ActionResult<List<StudySetPublic>> GetAll()
        {
            var result = service.GetAll().OrderByDescending(s => s.TimePushed).ToList();

            return new ActionResult<List<StudySetPublic>>(result);
        }

        //[HttpDelete]
        //public ActionResult Delete(string idSet)
        //{
        //    var countDeleted = service.Remove(idSet);

        //    if (countDeleted > 0) return Ok(countDeleted);
        //    else return BadRequest("Remove faild");
        //}
    }
}
