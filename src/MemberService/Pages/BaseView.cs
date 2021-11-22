using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;


namespace MemberService.Pages
{
    public abstract class BaseView<T> : RazorPage<T>
    {
    }
}
