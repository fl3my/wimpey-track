using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Profile;

public class CreateProfileDto
{
    [Required]
    public string FullName { get; set; }= string.Empty;
    [Required]
    public string StaffNumber { get; set; } = string.Empty;
    [Required]
    public string BusinessUnit { get; set; } = string.Empty;
    [Required]
    public string DepartmentSiteName { get; set; } = string.Empty;
    [Required]
    public string VehicleFuelType { get; set; } = string.Empty;
    [Required]
    public int VehicleEngineSize {get; set;}
    [Required]
    public string VehicleRegistration { get; set; } = string.Empty;
    [Required]
    public string VehicleMake { get; set; } = string.Empty;
    [Required]
    public int HomeLocationId { get; set; }
}