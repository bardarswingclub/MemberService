namespace MemberService.Pages.Payments;

using System.Linq.Expressions;
using System.Threading.Tasks;

using MemberService.Auth;
using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanListPayments))]
public class IndexModel : PageModel
{
    private readonly MemberContext _database;

    public IndexModel(MemberContext database)
    {
        _database = database;
    }

    [BindProperty(SupportsGet = true)]
    public int Skip { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Take { get; set; } = 50;

    [BindProperty(SupportsGet = true)]
    public string Query { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Method { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? After { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? Before { get; set; }

    public IReadOnlyCollection<Payment> Payments { get; set; }

    public async Task OnGetAsync()
    {
        Payments = await _database.Payments
            .Include(p => p.User)
            .Include(p => p.EventSignup)
            .Where(Search(Query))
            .Where(FilterMethod(Method))
            .Where(FilterAfter(After))
            .Where(FilterBefore(Before))
            .OrderByDescending(p => p.PayedAtUtc)
            .Skip(Skip)
            .Take(Take)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetRowsAsync()
    {
        await OnGetAsync();

        if (!Payments.Any())
        {
            return NotFound();
        }

        return Partial("_rows", this);
    }

    private static Expression<Func<Payment, bool>> Search(string query)
        => string.IsNullOrWhiteSpace(query)
            ? (p => true)
            : (p => p.Description.Contains(query)
                || p.User.NameMatches(query)
                || p.StripeChargeId.Contains(query)
                || p.VippsOrderId.Contains(query)
                || p.EventSignup.Event.Title.Contains(query));

    private static Expression<Func<Payment, bool>> FilterMethod(string method)
        => method switch
        {
            "Vipps" => p => p.VippsOrderId != null,
            "Stripe" => p => p.StripeChargeId != null,
            "Manual" => p => p.ManualPayment != null,
            _ => p => true
        };

    private static Expression<Func<Payment, bool>> FilterAfter(DateTime? date)
        => date is null
            ? (p => true)
            : (p => p.PayedAtUtc >= date);

    private static Expression<Func<Payment, bool>> FilterBefore(DateTime? date)
        => date is null
            ? (p => true)
            : (p => p.PayedAtUtc.Date <= date);
}
