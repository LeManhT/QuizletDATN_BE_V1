using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.Services;
using Quizlet_App_Server.Src.Models;

namespace Quizlet_App_Server.Src.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudySetController : ControllerExtend<User>
    {
        protected readonly UserService userService;
        protected readonly SetPublicService setPublicService;
        public StudySetController(AppConfigResource setting, IMongoClient mongoClient, IConfiguration config)
            : base(setting, mongoClient)
        {
            userService = new(mongoClient, config);
            setPublicService = new(mongoClient, config);
        }
        [HttpGet]
        public ActionResult<StudySetShareView> ShareView(string userId, string setId)
        {
            var user = userService.FindById(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var setInfo = user.Documents.GetAllSets().Find(s => s.Id.Equals(setId));
            if (setInfo == null)
            {
                return NotFound("Set not found in user's document");
            }

            StudySetShareView setSharing = new StudySetShareView(userId, user.UserName/*, user.Avatar*/, setInfo);
            return new ActionResult<StudySetShareView>(setSharing);

        }
        [HttpPost]
        public ActionResult<UserRespone> Create(string userId, [FromBody] StudySetDTO req)
        {
            User userExisting = userService.FindById(userId);
            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder folderOwner = userExisting.Documents.Folders.Find(x => x.Id.Equals(req.IdFolderOwner));

            StudySet newSet = new(userId, req);

            if (folderOwner != null && folderOwner.Id.Equals(req.IdFolderOwner))
            {
                if (folderOwner.StudySets.Any(set => set.Name.Equals(req.Name)))
                {
                    return BadRequest("Has exist other study set same name in folder");
                }
                else
                {
                    folderOwner.StudySets.Add(newSet);
                }
            }
            else
            {
                // study set in outside
                if (userExisting.Documents.StudySets.Any(set => set.Name.Equals(req.Name)))
                {
                    return BadRequest("Has exist other study set same name in here");
                }
                else
                {
                    newSet.IdFolderOwner = string.Empty;
                    userExisting.Documents.StudySets.Add(newSet);
                }
            }

            userService.UpdateDocumentsUser(userExisting);

            // update achievement
            userExisting.CollectionStorage.CreateSetCount++;
            userService.UpdateCollectionStorage(userExisting);

            var task = userExisting.Achievement.TaskList.Find(t => t.Id == 204);
            if (task != null && userExisting.CollectionStorage.CreateSetCount == 1)
            {
                bool wasCompleted = task.Status >= Models.TaskStatus.Completed;
                task.Progress++;
                userService.UpdateAchievement(userId, userExisting.Achievement);

                if (!wasCompleted && task.Status >= Models.TaskStatus.Completed)
                    //userService.UpdateScore(userExisting, task.Score ?? 0);
                    userService.CompleteNewTask(userExisting, task.Id ?? -1);

            }
            //--

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpPost]
        public ActionResult EnablePublic(string userId, string setId)
        {
            User userExisting = userService.FindById(userId);
            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            StudySet studySet = userExisting.Documents.GetAllSets().Find(s => s.Id.Equals(setId));
            if (studySet == null)
            {
                return NotFound("Study set not found in user's document");
            }

            studySet.IsPublic = true;
            userService.UpdateDocumentsUser(userExisting);

            var result = setPublicService.InsertOne(userId, userExisting.UserName, studySet);
            if (result != null) return Ok("Enable public successful");
            else return BadRequest("Something wrong");
        }
        [HttpPost]
        public ActionResult DisablePublic(string userId, string setId)
        {
            User userExisting = userService.FindById(userId);
            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            StudySet studySet = userExisting.Documents.GetAllSets().Find(s => s.Id.Equals(setId));
            if (studySet == null)
            {
                return NotFound("Study set not found in user's document");
            }

            studySet.IsPublic = false;
            userService.UpdateDocumentsUser(userExisting);

            var result = setPublicService.Remove(studySet.Id);
            if (result > 0) return Ok("Disable public successful");
            else return BadRequest("Something wrong");
        }
        [HttpPost]
        public ActionResult<User> AddToManyFolders(string userId, string setId, [FromBody] List<string> listIdFolders)
        {
            User userExisting = userService.FindById(userId);
            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            StudySet set = userExisting.Documents.GetAllSets().Find(s => s.Id.Equals(setId));
            if (set == null)
            {
                return NotFound("Set not found in user's document");
            }

            foreach (var folder in userExisting.Documents.Folders)
            {
                if (listIdFolders.Contains(folder.Id))
                {
                    var setClone = set.Clone();

                    folder.AddNewSet(setClone);
                    listIdFolders.Remove(folder.Id);
                }
            }

            var result = userService.UpdateDocumentsUser(userExisting);

            return new ActionResult<User>(result);
        }

        [HttpPut]
        public ActionResult<UserRespone> UpdateInfo(string userId, string setId, [FromBody] StudySetDTO req)
        {
            User userExisting = userService.FindById(userId);
            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            bool setFound = false;
            bool moveToNewFolder = false;
            var allSets = userExisting.Documents.GetAllSets();
            var allFolders = userExisting.Documents.Folders;

            StudySet setUpdating = userExisting.Documents.GetAllSets().Find(set => set.Id.Equals(setId));

            if (setUpdating == null)
            {
                return BadRequest("Set not found");
            }

            // update set
            setUpdating.Name = req.Name;
            setUpdating.Description = req.Description;
            setUpdating.IsPublic = req.IsPublic;

            List<FlashCard> tempCards = new List<FlashCard>();
            foreach (var card in req.AllNewCards)
            {
                if (card.Id.IsNullOrEmpty())
                {
                    //setUpdating.AddNewCard(card);
                    FlashCard newCard = new FlashCard(card);
                    newCard.IdSetOwner = setUpdating.Id;
                    tempCards.Add(newCard);
                }
                else
                {
                    FlashCard cardReference = setUpdating.Cards.Find(c => c.Id.Equals(card.Id));
                    if (cardReference != null)
                    {
                        cardReference.UpdateInfo(card);

                        tempCards.Add(cardReference);
                    }
                }
            }
            // update set
            setUpdating.Cards = tempCards;
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpPost]
        public ActionResult<UserRespone> InsertNewCard(string userId, string setId, [FromBody] List<FlashCardDTO> newCards)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            StudySet studySet = userExisting.Documents.GetAllSets().Find(s => s.Id.Equals(setId));
            if (studySet == null)
            {
                return BadRequest("Not found study set in user's document");
            }

            foreach (var cardDTO in newCards)
            {
                studySet.AddNewCard(cardDTO);
            }

            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpDelete]
        public ActionResult<UserRespone> RemoveCard(string userId, string setId, string cardId)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            StudySet studySet = userExisting.Documents.GetAllSets().Find(s => s.Id.Equals(setId));
            if (studySet == null)
            {
                return BadRequest("Not found study set in user's document");
            }

            studySet.Cards.RemoveAll(c => c.Id.Equals(cardId));

            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpDelete]
        public ActionResult<UserRespone> Delete(string userId, string setId)
        {
            User userExisting = userService.FindById(userId);
            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            StudySet setReq = userExisting.Documents.GetAllSets().Find(set => set.Id.Equals(setId));
            Folder folderOwner = userExisting.Documents.GetFolderOwnerOfSet(setId);

            // delete set require
            if (folderOwner != null)
            {
                folderOwner.StudySets.Remove(setReq);
            }
            else
            {
                userExisting.Documents.StudySets.Remove(setReq);
            }
            if (setReq.IsPublic)
            {
                setPublicService.Remove(setReq.Id);
            }
            // update documents
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
    }
}
