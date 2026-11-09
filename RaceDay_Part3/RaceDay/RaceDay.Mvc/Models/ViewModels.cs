using RaceDay.Contracts;

namespace RaceDay.Mvc.Models;

public class CategoriesPageViewModel
{
    public int EventId { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
    public CreateCategoryRequest NewCategory { get; set; } = new();
}

public class ProfilePageViewModel
{
    public ProfileDto Profile { get; set; } = new();
    public UpdateProfileRequest Form { get; set; } = new();
}
