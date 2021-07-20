using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            
            if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;
            
            var userId = resultContext.HttpContext.User.GetUserId();
            var unitOfWork = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();

            var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId);
            user.LastActiveAt = System.DateTime.UtcNow;
            await unitOfWork.Complete();
        }
    }
}