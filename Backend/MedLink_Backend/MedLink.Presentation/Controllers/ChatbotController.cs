using MedLink.Application.Constants;
using MedLink.Application.DTOs.Common.ChatbotDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedLink.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = Roles.User)]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Sends a message to the MedLink AI health assistant. The assistant knows the
        /// patient's own medical history and, when latitude/longitude are given, nearby
        /// verified providers. Text only (no voice).
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage([FromBody] ChatbotRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _chatbotService.SendMessageAsync(CurrentUserId, request);
            return Ok(result);
        }

        /// <summary>
        /// Clears the current conversation memory and starts a new chat.
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> ResetConversation()
        {
            await _chatbotService.ResetConversationAsync(CurrentUserId);
            return NoContent();
        }
    }
}
