using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers {
    [ServiceFilter (typeof (LogUserActivity))]
    [Route ("api/users/{userId}/messages")]
    [ApiController]
    public class MessagesControlller : ControllerBase {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesControlller (IDatingRepository repo, IMapper mapper) {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet ("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage (int userId, int id) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            var messageFromRepo = await _repo.GetMessage (id);
            if (messageFromRepo == null) {
                return NotFound ();
            }
            return Ok (messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser (int userId, [FromQuery] MessageParams messageParams) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            messageParams.UserId = userId;
            var messageFromRepo = await _repo.GetMessagesForUser (messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>> (messageFromRepo);
            Response.AddPagination (messageFromRepo.CurrentPage, messageFromRepo.PageSize, messageFromRepo.TotalCount, messageFromRepo.TotalPage);
            return Ok (messages);
        }

        [HttpGet ("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread (int userId, int recipientId) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            var messagesFromRepo = await _repo.GetMessageThread (userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>> (messagesFromRepo);
            return Ok (messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage (int userId, MessageToCreateDto messageToCreateDto) {
            var sender = await _repo.GetUser (userId);
            if (sender.Id != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            messageToCreateDto.SenderId = userId;
            var recipient = _repo.GetUser (messageToCreateDto.RecipientId);
            if (recipient == null) { return BadRequest ("Could not find the user!"); }
            var message = _mapper.Map<Message> (messageToCreateDto);
            _repo.Add (message);

            if (await _repo.SaveAll ()) {
                var messageToReturn = _mapper.Map<MessageToReturnDto> (message);
                return CreatedAtRoute ("GetMessage", new { userId, id = message.Id }, messageToReturn);
            }
            throw new System.Exception ("Message creating failed on save!");
        }
        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId){
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
             var messagefromrepo = await _repo.GetMessage(id);
             if(messagefromrepo.SenderId == userId){
                 messagefromrepo.SenderDeleted = true;
             }
             if(messagefromrepo.RecipientId == userId){
                 messagefromrepo.RecipientDeleted = true;
             }
             if(messagefromrepo.SenderDeleted && messagefromrepo.RecipientDeleted){
                 _repo.Delete(messagefromrepo);
             }
             if(await _repo.SaveAll()){
                 return NoContent();
             }
             throw new System.Exception("Error deleteing message");
        }
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageRead(int userId, int id){
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            var message = await  _repo.GetMessage(id);
            if(message.RecipientId != userId){
                return Unauthorized();
            }
            message.IsRead = true;
            message.DateRead = System.DateTime.Now;
            await _repo.SaveAll();
            return NoContent();
        }
    }
}