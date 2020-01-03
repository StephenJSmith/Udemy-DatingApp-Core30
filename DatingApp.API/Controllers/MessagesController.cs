using System;
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

namespace DatingApp.API.Controllers
{
  [ServiceFilter(typeof(LogUserActivity))]
  [Authorize]
  [ApiController]
  [Route("api/users/{userId}/[controller]")]
  public class MessagesController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;

    public MessagesController(IDatingRepository repo, IMapper mapper)
    {
      _mapper = mapper;
      _repo = repo;
    }

    [HttpGet("{id}", Name = "GetMessage")]
    public async Task<IActionResult> GetMessage(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }

      var messageFromRepo = await _repo.GetMessage(id);
      if (messageFromRepo == null)
      {
        return NotFound();
      }

      return Ok(messageFromRepo);
    }

    [HttpGet]
    public async Task<IActionResult> GetMessagesForUser(int userId,
      [FromQuery]MessageParams msgParams)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }

      msgParams.UserId = userId;
      var messagesFromRepo = await _repo.GetMessagesForUser(msgParams);
      var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

      Response.AddPagination(messagesFromRepo.CurrentPage,
        msgParams.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

      return Ok(messages);
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }

      var messagesFromRepo = await _repo.GetMessageThread(userId, recipientId);
      var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

      return Ok(messageThread);
    }

    private int IEnumerable<T>(IEnumerable<Message> messagesFromRepo)
    {
      throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage(
        int userId,
        MessageForCreationDto dto)
    {
      var sender = await _repo.GetUser(userId);
      if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }

      dto.SenderId = userId;
      var recipient = await _repo.GetUser(dto.RecipientId);
      if (recipient == null)
      {
        return BadRequest("Could not find recipient");
      }

      var message = _mapper.Map<Message>(dto);
      _repo.Add(message);


      if (await _repo.SaveAll())
      {
        var messageToReturn = _mapper.Map<MessageToReturnDto>(message);

        return CreatedAtRoute("GetMessage", new { userId, id = message.Id }, messageToReturn);
      }

      throw new System.Exception("Creating the message failed on save");
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> DeleteMessage(int id, int userId)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }

      var messageFromRepo = await _repo.GetMessage(id);

      if (messageFromRepo.SenderId == userId)
      {
        messageFromRepo.SenderDeleted = true;
      }

      if (messageFromRepo.RecipientId == userId)
      {
        messageFromRepo.RecipientDeleted = true;
      }

      if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
      {
        _repo.Delete(messageFromRepo);
      }

      if (await _repo.SaveAll())
      {
        return NoContent();
      }

      throw new Exception("Error deleting the message");
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult> MarkMessagesAsRead(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }

      var messageFromRepo = await _repo.GetMessage(id);
      if (messageFromRepo.RecipientId != userId)
      {
          return Unauthorized();
      }

      messageFromRepo.IsRead = true;
      messageFromRepo.DateRead = DateTime.Now;

      return NoContent();
    }
  }
}