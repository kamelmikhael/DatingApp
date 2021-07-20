using System.Threading.Tasks;

namespace DatingApp.API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IMessageRepository MessageRepository { get; }
        ILikeRepository LikeRepository { get; }

        Task<bool> Complete();
        bool HasChanges();
    }
}