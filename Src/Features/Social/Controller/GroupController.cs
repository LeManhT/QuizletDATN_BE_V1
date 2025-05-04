using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Features.Social.Service;

namespace Quizlet_App_Server.Src.Features.Social.Controller
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GroupController : ControllerBase
    {
        private readonly GroupService _groupService;
        private readonly IMapper _mapper;
        private readonly ILogger<GroupController> _logger;

        public GroupController(GroupService groupService, IMapper mapper, ILogger<GroupController> logger)
        {
            _groupService = groupService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] ConversationDTO groupDto)
        {
            try
            {
                var group = _mapper.Map<Conversation>(groupDto);
                var result = await _groupService.CreateGroupAsync(group);
                return Ok(_mapper.Map<ConversationDTO>(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGroup([FromQuery] string groupId)
        {
            try
            {
                var group = await _groupService.GetGroupByIdAsync(groupId);
                return Ok(_mapper.Map<ConversationDTO>(group));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserGroups([FromQuery] string userId)
        {
            try
            {
                var groups = await _groupService.GetUserGroupsAsync(userId);
                return Ok(_mapper.Map<List<ConversationDTO>>(groups));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user groups");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("members")]
        public async Task<IActionResult> AddMemberToGroup([FromQuery] string groupId, [FromBody] GroupMemberDTO memberDto)
        {
            try
            {
                var member = _mapper.Map<GroupMember>(memberDto);
                var result = await _groupService.AddMemberToGroupAsync(groupId, member);

                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest("Failed to add member to group");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member to group");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveMemberFromGroup([FromQuery] string groupId, [FromQuery] string userId)
        {
            try
            {
                var result = await _groupService.RemoveMemberFromGroupAsync(groupId, userId);

                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest("Failed to remove member from group");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member from group");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupMessages(
            [FromQuery] string groupId,
            [FromQuery] int limit = 50,
            [FromQuery] long? beforeTimestamp = null)
        {
            try
            {
                var messages = await _groupService.GetGroupMessagesAsync(groupId, limit, beforeTimestamp);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group messages");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateGroup([FromQuery] string groupId, [FromBody] ConversationDTO groupDto)
        {
            try
            {
                if (groupId != groupDto.Id)
                {
                    return BadRequest("Group ID mismatch");
                }

                var group = _mapper.Map<Conversation>(groupDto);
                var result = await _groupService.UpdateGroupAsync(group);

                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest("Failed to update group");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetGroupMembers([FromQuery] string groupId)
        {
            try
            {
                var members = await _groupService.GetGroupMembersAsync(groupId);
                return Ok(_mapper.Map<List<GroupMemberDTO>>(members));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group members");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteGroup([FromQuery] string groupId)
        {
            try
            {
                var result = await _groupService.DeleteGroupAsync(groupId);

                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest("Failed to delete group");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateMemberRole(
            [FromQuery] string groupId,
            [FromQuery] string userId,
            [FromQuery] string role)
        {
            try
            {
                var result = await _groupService.UpdateGroupMemberRoleAsync(groupId, userId, role);

                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest("Failed to update member role");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member role");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
