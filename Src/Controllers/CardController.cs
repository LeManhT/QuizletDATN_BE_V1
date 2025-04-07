using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Utility;
using System.Net;
using User = Quizlet_App_Server.Src.Models.User;

namespace Quizlet_App_Server.Src.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CardController : ControllerExtend<User>
    {
        private readonly UserService userService;
        private readonly IWebHostEnvironment webHostEnvironment;
        public CardController(AppConfigResource setting
                            , IMongoClient mongoClient
                            , IConfiguration config
                            , IWebHostEnvironment webHostEnvironment)
            : base(setting, mongoClient)
        {
            userService = new(mongoClient, config);
            this.webHostEnvironment = webHostEnvironment;
        }
        [AllowAnonymous]
        private User UpdateDocumentsUser(User existingUser)
        {
            return userService.UpdateDocumentsUser(existingUser);
        }
        [HttpPost]
        public ActionResult<UserRespone> Create(string userId, [FromBody] FlashCardDTO cardReq)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }
            FlashCard newCard = new(cardReq);

            StudySet setOwner = existingUser.Documents.StudySets.Find(x => x.Id == cardReq.IdSetOwner);
            if (setOwner != null)
            {
                newCard.IdSetOwner = setOwner.Id;
            }
            else
            {
                newCard.IdSetOwner = string.Empty;
            }

            if (setOwner != null && setOwner.Id.Equals(cardReq.IdSetOwner))
            {
                setOwner.Cards.Add(newCard);
            }
            else
            {
                newCard.IdSetOwner = string.Empty;
                existingUser.Documents.FlashCards.Add(newCard);
            }

            var result = UpdateDocumentsUser(existingUser);

            UserRespone respone = new UserRespone(existingUser);
            return new ActionResult<UserRespone>(respone);
        }

        [HttpPut]
        public ActionResult<UserRespone> UpdateInfo(string userId, string cardId, [FromBody] FlashCardDTO req)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // find card in document
            FlashCard cardRequire = existingUser.Documents.GetAllCards().Find(x => x.Id == cardId);

            if (cardRequire == null)
            {
                return BadRequest("card not found");
            }

            // update card
            cardRequire.Term = req.Term;
            cardRequire.Definition = req.Definition;


            // update card
            var result = UpdateDocumentsUser(existingUser);

            UserRespone respone = new UserRespone(existingUser);
            return new ActionResult<UserRespone>(respone);
        }

        [HttpDelete]
        public ActionResult<UserRespone> Delete(string userId, string cardId)
        {
            User existingUser = userService.FindById(userId);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // find and remove card in document
            FlashCard cardRequire = existingUser.Documents.GetAllCards().Find(card => card.Id.Equals(cardId));

            if (cardRequire == null)
            {
                return NotFound("Not found card");
            }

            if (cardRequire.IdSetOwner.IsNullOrEmpty())
            {
                existingUser.Documents.FlashCards.Remove(cardRequire);
            }
            else
            {
                StudySet setOwner = existingUser.Documents.GetAllSets().Find(set => set.Id.Equals(cardRequire.IdSetOwner));
                setOwner?.Cards.Remove(cardRequire);
            }

            // delete card
            var result = UpdateDocumentsUser(existingUser);

            UserRespone respone = new UserRespone(existingUser);
            return new ActionResult<UserRespone>(respone);
        }

        /*[HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            FileStream stream;
            if(file.Length > 0)
            {
                string path = Path.Combine(webHostEnvironment.WebRootPath, file.FileName);
                stream = new FileStream(Path.Combine(path), FileMode.Open);

                await System.Threading.Tasks.Task.Run(() =>
                {
                    Upload(stream, file.FileName);
                });

                return Ok("Uploading");
            }
            else
                return BadRequest("Upload failed");
        }*/

        private async void Upload(FileStream stream, string fileName)
        {
            //authentication
            var auth = new FirebaseAuthProvider(new FirebaseConfig(FBConfig.ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(FBConfig.AuthEmail, FBConfig.AuthPassword);

            var cancellation = new CancellationTokenSource();

            // Constructr FirebaseStorage, path to where you want to upload the file and Put it there
            var task = new FirebaseStorage(
                FBConfig.Bucket,

                 new FirebaseStorageOptions
                 {
                     AuthTokenAsyncFactory = () => System.Threading.Tasks.Task.FromResult(a.FirebaseToken),
                     ThrowOnCancel = true,
                 })
                .Child("data")
                .Child("random")
                .Child(fileName)
                .PutAsync(stream, cancellation.Token);

            try
            {
                string link = await task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
