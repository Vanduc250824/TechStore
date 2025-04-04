public class EditAccountViewModel
{
    public string Id { get; set; }
    
    public string Username { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string FullName { get; set; }
    public string Address { get; set; }

    public List<string> AvailableRoles { get; set; } // Danh sách vai trò có sẵn
    public string SelectedRole { get; set; } // Vai trò được chọn
}
