using System;
using API.Interfaces;

namespace API.Data;

public class UnitOfWork : IUnitOfWork
{

    private readonly DataContext context;
    public UnitOfWork(IUserRepository userRepository, IMessageRepository messageRepository, ILikesRepository likesRepository, DataContext context)
    {
        UserRepository = userRepository;
        MessageRepository = messageRepository;
        LikesRepository = likesRepository;
        this.context = context;
    }

    public IUserRepository UserRepository { get; init; }

    public IMessageRepository MessageRepository { get; init; }

    public ILikesRepository LikesRepository { get; init; }

    public async Task<bool> Complete()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
