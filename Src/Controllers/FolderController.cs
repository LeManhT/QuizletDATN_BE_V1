using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Services;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.Models;

namespace Quizlet_App_Server.Src.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FolderController : ControllerExtend<User>
    {
        protected readonly UserService userService;
        public FolderController(AppConfigResource setting, IMongoClient mongoClient, IConfiguration config)
            : base(setting, mongoClient)
        {
            userService = new(mongoClient, config);
        }
        [HttpGet]
        public ActionResult<FolderShareView> ShareView(string userId, string folderId)
        {
            var user = userService.FindById(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var folderInfo = user.Documents.Folders.Find(f => f.Id.Equals(folderId));
            if (folderId == null)
            {
                return NotFound("folder not found in user's document");
            }

            FolderShareView folderSharing = new FolderShareView(userId, user.UserName/*, user.Avatar*/, folderInfo);
            return new ActionResult<FolderShareView>(folderSharing);

        }
        [HttpPost]
        public ActionResult<UserRespone> Create(string userId, [FromBody] FolderDTO req)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }
            else if (userExisting.Documents.Folders.Any(folder => folder.Name.Equals(req.Name)))
            {
                return BadRequest("Has exist other folder same name");
            }

            Folder newFolder = new(req);
            userExisting.Documents.Folders.Add(newFolder);
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpPut]
        public ActionResult<UserRespone> UpdateInfo(string userId, string folderId, [FromBody] FolderDTO req)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }
            else if (userExisting.Documents.Folders.Count <= 0
                || !userExisting.Documents.Folders.Any(folder => folder.Id.Equals(folderId)))
            {
                return BadRequest("Not found folder in user's document");
            }

            if (userExisting.Documents.Folders.Any(folder => folder.Name.Equals(req.Name)))
            {
                return BadRequest("Has exist other folder same name");
            }

            // update folder
            Folder existFolder = userExisting.Documents.Folders.Find(folder => folder.Id == folderId);
            existFolder.Name = req.Name;
            existFolder.Description = req.Description;
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpPost]
        public ActionResult<UserRespone> InsertNewSet(string userId, string folderId, [FromBody] List<StudySetDTO> newSets)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder folder = userExisting.Documents.Folders.Find(f => f.Id.Equals(folderId));
            if (folder == null)
            {
                return BadRequest("Not found folder in user's document");
            }

            List<StudySet> tempList = new List<StudySet>();
            foreach (var s in newSets)
            {
                if (s.Id.IsNullOrEmpty())
                {
                    StudySet newSet = new StudySet(userId, s);
                    tempList.Add(newSet);
                    continue;
                }

                StudySet setExisting = folder.StudySets.Find(set => set.Id.Equals(s.Id));
                if (setExisting != null)
                {
                    setExisting.UpdateInfo(s);
                    tempList.Add(setExisting);
                }
            }

            folder.StudySets = tempList;
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpPost]
        public ActionResult<UserRespone> InsertSetExisting(string userId, string folderId, [FromBody] List<string> idSetExisting)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder folder = userExisting.Documents.Folders.Find(f => f.Id.Equals(folderId));
            if (folder == null)
            {
                return BadRequest("Not found folder in user's document");
            }

            var allSet = userExisting.Documents.GetAllSets();
            int countChecking = 0;
            List<StudySet> tempList = new List<StudySet>();
            tempList.AddRange(folder.StudySets);
            foreach (var set in allSet)
            {
                if (idSetExisting.Contains(set.Id))
                {
                    countChecking++;
                    // check this set has already exist in folder
                    if (tempList.Any(s => s.Id.Equals(set.Id))) continue;

                    // remove set in outside
                    if (set.IdFolderOwner.IsNullOrEmpty())
                    {
                        userExisting.Documents.StudySets.Remove(set);
                    }
                    //folder.AddNewSet(set);
                    set.IdFolderOwner = folder.Id;
                    tempList.Add(set);
                }

                if (countChecking == idSetExisting.Count) break;
            }

            folder.StudySets = tempList;
            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpDelete]
        public ActionResult<UserRespone> RemoveSet(string userId, string folderId, string setId)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }

            Folder folder = userExisting.Documents.Folders.Find(f => f.Id.Equals(folderId));
            if (folder == null)
            {
                return BadRequest("Not found folder in user's document");
            }

            int removed = folder.StudySets.RemoveAll(s => s.Id.Equals(setId));
            if (removed <= 0)
            {
                return BadRequest("Not found set");
            };

            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
        [HttpDelete]
        public ActionResult<UserRespone> Delete(string userId, string folderId)
        {
            User userExisting = userService.FindById(userId);

            if (userExisting == null)
            {
                return NotFound("User not found");
            }
            else if (userExisting.Documents.Folders.Count <= 0
                || !userExisting.Documents.Folders.Any(folder => folder.Id.Equals(folderId)))
            {
                return BadRequest("Not found folder in user's document");
            }

            // remove folder
            userExisting.Documents.Folders.RemoveAll(folder => folder.Id.Equals(folderId));

            userService.UpdateDocumentsUser(userExisting);

            UserRespone respone = new UserRespone(userExisting);
            return new ActionResult<UserRespone>(respone);
        }
    }
}