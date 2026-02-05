namespace WimpeyTrack.Api.Providers;

public record ProfileSnapshot(
    string FullName,
    string StaffNumber,
    string BusinessUnit,
    string DepartmentSiteName,
    string VehicleFuelType,
    int VehicleEngineSize,
    string VehicleRegistration,
    string VehicleMake,
    string HomePostcode,
    int HomeLocationId
);