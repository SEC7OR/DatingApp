using System;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext context;
    private readonly IMapper mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = context.Messages
            .OrderByDescending(m => m.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.Username && m.RecipientDeleted == false),
            "Outbox" => query.Where(m => m.Sender.UserName == messageParams.Username && m.SenderDeleted == false),
            _ => query.Where(m => m.Recipient.UserName == messageParams.Username && m.DateRead == null && m.RecipientDeleted == false),
        };

        var messages = query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider);

        return await PagedList<MessageDTO>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);

    }

    public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        var messages = await context.Messages
            .Include(m => m.Sender)
                .ThenInclude(m => m.Photos)
            .Include(m => m.Recipient)
                .ThenInclude(m => m.Photos)
            .Where(m => m.RecipientUsername == currentUsername 
                && m.RecipientDeleted == false 
                && m.SenderUsername == recipientUsername ||
                m.SenderUsername == currentUsername 
                && m.SenderDeleted == false 
                && m.RecipientUsername == recipientUsername)
            .OrderBy(m => m.MessageSent)
            .ToListAsync();

        var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();
        if (unreadMessages.Any())
        {
            unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        return mapper.Map<IEnumerable<MessageDTO>>(messages);
    }

    public async Task<bool> SaveAllASync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
