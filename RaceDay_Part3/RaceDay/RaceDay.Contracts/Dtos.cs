using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

// ========== AUTH ==========
public class RegisterRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a role.")]
    [RegularExpression("^(Organiser|Participant)$", ErrorMessage = "Role must be Organiser or Participant.")]
    public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"
}

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

// ========== PROFILE ==========
public class ProfileDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}

// ========== EVENTS ==========
public class CreateEventRequest
{
    [Required(ErrorMessage = "Event name is required.")]
    [StringLength(100, ErrorMessage = "Event name cannot be longer than 100 characters.")]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
    public string? Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required.")]
    [Display(Name = "Event date")]
    public DateTime EventDate { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(150, ErrorMessage = "Location cannot be longer than 150 characters.")]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; }

    [Required(ErrorMessage = "Please choose an event type.")]
    [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "Event type must be Run, Walk or Cycle.")]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = string.Empty;
}

public class UpdateEventRequest
{
    [Required(ErrorMessage = "Event name is required.")]
    [StringLength(100, ErrorMessage = "Event name cannot be longer than 100 characters.")]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
    public string? Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required.")]
    [Display(Name = "Event date")]
    public DateTime EventDate { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(150, ErrorMessage = "Location cannot be longer than 150 characters.")]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; }

    [Required(ErrorMessage = "Please choose an event type.")]
    [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "Event type must be Run, Walk or Cycle.")]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = string.Empty;
}

public class EventDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int OrganiserId { get; set; }
    public string? OrganiserName { get; set; }
    public string? BannerUrl { get; set; }
    public int EnrolmentCount { get; set; }
}

// ========== CATEGORIES ==========
public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, ErrorMessage = "Category name cannot be longer than 100 characters.")]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty; // e.g. Under 20, Senior, 10km, 21km

    [StringLength(250, ErrorMessage = "Description cannot be longer than 250 characters.")]
    public string? Description { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// ========== ENROLMENTS ==========
public class CreateEnrolmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select an event.")]
    public int EventId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }
}

public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ParticipantId { get; set; }
    public string? ParticipantName { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
    public DateTime EnrolmentDate { get; set; }
}

public class UpdateEnrolmentStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

// ========== RESULTS ==========
public class CreateResultRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select an enrolment.")]
    public int EnrolmentId { get; set; }

    public TimeSpan FinishTime { get; set; }

    [Range(1, 100000, ErrorMessage = "Position must be 1 or greater.")]
    public int Position { get; set; }
}

public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public int ParticipantId { get; set; }
    public string? ParticipantName { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? EventDate { get; set; }
    public int TotalFinishers { get; set; } // finishers recorded for the same event
}

// ========== BLOB UPLOAD RESPONSE ==========
public class BlobUploadResponse
{
    public string Url { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
