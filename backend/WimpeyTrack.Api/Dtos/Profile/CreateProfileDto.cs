using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Profile;

public class CreateProfileDto
{
    public string FullName { get; set; }= string.Empty;
    public string StaffNumber { get; set; } = string.Empty;
    public string BusinessUnit { get; set; } = string.Empty;
    public string DepartmentSiteName { get; set; } = string.Empty;
    public string VehicleFuelType { get; set; } = string.Empty;
    public int VehicleEngineSize {get; set;}
    public string VehicleRegistration { get; set; } = string.Empty;
    public string VehicleMake { get; set; } = string.Empty;
    public string HomePostcode { get; set; } = string.Empty;
    public int HomeLocationId { get; set; }
}