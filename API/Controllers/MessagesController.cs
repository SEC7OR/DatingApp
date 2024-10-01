using System;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class MessagesController : BaseApiController
{
    private readonly IMessageRepository messageRepository;
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;
    public MessagesController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper)
    {
        this.messageRepository = messageRepository;
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
        var username = User.GetUsername();
        if (username == createMessageDTO.RecipientUsername.ToLower())
        {
            return BadRequest("You cannot message yourself");
        }
        var sender = await userRepository.GetUserByUsernameAsync(username);
        var recipient = await userRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

        if (sender == null || recipient == null)
        {
            return BadRequest("Cannot send message at this time");
        }

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDTO.Content
        };

        messageRepository.AddMessage(message);

        if (await messageRepository.SaveAllASync())
        {
            return Ok(mapper.Map<MessageDTO>(message));
        }

        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await messageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(messages);

        return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();

        return Ok(await messageRepository.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();

        var message = await messageRepository.GetMessage(id);

        if (message == null)
        {
            return BadRequest("Cannot delete this message");
        }

        if (message.SenderUsername != username && message.RecipientUsername != username)
        {
            return Forbid();
        }

        if (message.SenderUsername == username)
        {
            message.SenderDeleted = true;
        }

        if (message.RecipientUsername == username)
        {
            message.RecipientDeleted = true;
        }

        if (message.SenderDeleted && message.RecipientDeleted)
        {
            messageRepository.DeleteMessage(message);
        }

        if (await messageRepository.SaveAllASync())
        {
            return Ok();
        }

        return BadRequest("Problem deleting the message");
    }

}
