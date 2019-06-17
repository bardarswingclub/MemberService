using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface IPartialRenderer
    {
        Task<string> RenderPartial<TModel>(string partialName, TModel model);
    }
}
