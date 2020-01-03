using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class DatingRepository : IDatingRepository
  {
    private readonly DataContext _context;

    public DatingRepository(DataContext context)
    {
      _context = context;
    }

    public void Add<T>(T entity) where T : class
    {
      _context.Add(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
      _context.Remove(entity);
    }

    public async Task<User> GetUser(int id)
    {
      var user = await _context.Users
        .Include(u => u.Photos)
        .FirstOrDefaultAsync(u => u.Id == id);

      return user;
    }

    public async Task<PagedList<User>> GetUsers(UserParams userParams)
    {
      var users = _context.Users.Include(u => u.Photos)
        .OrderByDescending(u => u.LastActive)
        .AsQueryable();
      users = users.Where(u => u.Id != userParams.UserId);
      users = users.Where(u => u.Gender == userParams.Gender);

      if (userParams.Likers)
      {
          var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
          users = users.Where(u => userLikers.Contains(u.Id));
      }

      if (userParams.Likees)
      {
          var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
          users = users.Where(u => userLikees.Contains(u.Id));
      }

      if (userParams.MinAge != 18 || userParams.MaxAge != 99)
      {
        var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
        var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

        users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
      }

      if (!string.IsNullOrEmpty(userParams.OrderBy))
      {
        users = userParams.OrderBy switch
        {
          "created" => users.OrderByDescending(u => u.Created),
          _ => users.OrderByDescending(u => u.LastActive),
        };
      }

      var pagedList = await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);

      return pagedList;
    }

    private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers) {
      var user = await _context.Users
        .Include(x => x.Likers)
        .Include(x => x.Likees)
        .FirstOrDefaultAsync(u => u.Id == id);

      if (likers)
      {
            return user.Likers
              .Where(u => u.LikeeId == id)
              .Select(i => i.LikerId);
      } else
      {
          return user.Likees
            .Where(u => u.LikerId == id)
            .Select(i => i.LikeeId);
      }
    }

    public async Task<Photo> GetPhoto(int id)
    {
      var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

      return photo;
    }

    public async Task<Photo> GetMainPhotoForUser(int userId)
    {
      return await _context.Photos
        .Where(u => u.UserId == userId)
        .FirstOrDefaultAsync(p => p.IsMain);
    }

    public async Task<bool> SaveAll()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Like> GetLike(int userId, int recipientId)
    {
      var likes = await _context.Likes.FirstOrDefaultAsync( u => 
        u.LikerId == userId && u.LikeeId == recipientId );

      return likes;
    }

    public async Task<Message> GetMessage(int id) {
      var messages = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);

      return messages;
    }

    public async Task<PagedList<Message>> GetMessagesForUser(MessageParams msgParams) {
      var messages = _context.Messages
        .Include(u => u.Sender).ThenInclude(sender => sender.Photos)
        .Include(u => u.Recipient).ThenInclude(recipient => recipient.Photos)
        .AsQueryable();

      switch(msgParams.MessageContainer) {
        case "Inbox": 
          messages = messages.Where(u => 
            u.RecipientId == msgParams.UserId && !u.RecipientDeleted);
          break;

        case "Outbox": 
          messages = messages.Where(u =>
            u.SenderId == msgParams.UserId && !u.SenderDeleted);
          break;

        default: 
          messages = messages.Where(u => 
            u.RecipientId == msgParams.UserId 
              && !u.RecipientDeleted && !u.IsRead);
          break;
      }

      messages = messages.OrderByDescending(d => d.MessageSent);
      var pagedList = await PagedList<Message>.CreateAsync(
        messages, msgParams.PageNumber, msgParams.PageSize);

      return pagedList;
    }

    public async Task<IEnumerable<Message>> GetMessageThread(
      int userId, int recipientId) {
      var messages = await _context.Messages
        .Include(u => u.Sender).ThenInclude(sender => sender.Photos)
        .Include(u => u.Recipient).ThenInclude(recipient => recipient.Photos)
        .Where(m => m.RecipientId == userId && !m.RecipientDeleted && m.SenderId == recipientId
          || m.RecipientId == recipientId && !m.SenderDeleted && m.SenderId == userId)
          .OrderByDescending(m => m.MessageSent)
          .ToListAsync();
          
      return messages;
    }
  }
}