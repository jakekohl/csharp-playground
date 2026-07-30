using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UserEntity = Playground.Data.User;

namespace Playground.Pages;

public class UsersModel : PageModel
{
    private const int ActorUserId = 1;

    public List<UserEntity> Users { get; private set; } = [];

    public string? StatusMessage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public UserEntity? EditingUser { get; private set; }

    [BindProperty]
    public UserFormInput Input { get; set; } = new();

    public void OnGet(int? editId)
    {
        LoadUsers();

        if (editId is int id)
        {
            EditingUser = UserEntity.ReadSingle(id);
            if (EditingUser is null)
            {
                ErrorMessage = $"No user found with UserID {id}.";
                return;
            }

            Input = new UserFormInput
            {
                UserId = EditingUser.UserId,
                DisplayName = EditingUser.DisplayName,
                Email = EditingUser.Email,
                InActive = EditingUser.IsInactive,
            };
        }
    }

    public IActionResult OnPostCreate()
    {
        if (!TryValidateModel(Input, nameof(Input)))
        {
            ErrorMessage = "Display name and email are required.";
            LoadUsers();
            return Page();
        }

        try
        {
            var created = UserEntity.Create(ActorUserId, Input.DisplayName.Trim(), Input.Email.Trim(), Input.InActive);
            TempData["StatusMessage"] = $"Created user #{created.UserId} ({created.DisplayName}).";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            LoadUsers();
            return Page();
        }
    }

    public IActionResult OnPostUpdate()
    {
        if (Input.UserId is null or <= 0)
        {
            ErrorMessage = "Select a user to update.";
            LoadUsers();
            return Page();
        }

        if (!TryValidateModel(Input, nameof(Input)))
        {
            ErrorMessage = "Display name and email are required.";
            EditingUser = UserEntity.ReadSingle(Input.UserId.Value);
            LoadUsers();
            return Page();
        }

        try
        {
            var updated = UserEntity.Update(
                Input.UserId.Value,
                ActorUserId,
                Input.DisplayName.Trim(),
                Input.Email.Trim(),
                Input.InActive);

            if (updated is null)
            {
                ErrorMessage = $"No user found with UserID {Input.UserId}.";
                LoadUsers();
                return Page();
            }

            TempData["StatusMessage"] = $"Updated user #{updated.UserId} ({updated.DisplayName}).";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            EditingUser = UserEntity.ReadSingle(Input.UserId.Value);
            LoadUsers();
            return Page();
        }
    }

    public IActionResult OnPostInactivate(int id)
    {
        try
        {
            var user = UserEntity.Inactivate(id, ActorUserId);
            TempData["StatusMessage"] = user is null
                ? $"No user found with UserID {id}."
                : $"Inactivated user #{user.UserId} ({user.DisplayName}).";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        try
        {
            var deleted = UserEntity.Delete(id);
            TempData["StatusMessage"] = deleted
                ? $"Deleted user #{id}."
                : $"No user found with UserID {id}.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    private void LoadUsers()
    {
        try
        {
            Users = UserEntity.List();
            StatusMessage = TempData["StatusMessage"] as string;
            ErrorMessage ??= TempData["ErrorMessage"] as string;
        }
        catch (Exception ex)
        {
            Users = [];
            ErrorMessage = $"Could not load users: {ex.Message}";
        }
    }

    public class UserFormInput
    {
        public int? UserId { get; set; }

        [Required, StringLength(50)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(50)]
        public string Email { get; set; } = string.Empty;

        public bool InActive { get; set; }
    }
}
