using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Playground.Pages;

public class SandboxModel : PageModel
{
    [BindProperty]
    public string? Name { get; set; }

    public string? Message { get; private set; }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        var displayName = string.IsNullOrWhiteSpace(Name) ? "friend" : Name.Trim();
        Message = $"Hello, {displayName}! Welcome to the playground.";
    }
}
