using BDMS.Domain.Features.Hospital.Commands;
using BDMS.Domain.Features.Hospital.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.Hospital;

public interface IHospitalService
{
    Task<Result<List<HospitalRespModel>>> GetAllHospitals();

    Task<Result<HospitalRespModel>> GetHospitalById(int hospitalId);

    Task<Result<HospitalRespModel>> DeleteHospital(int hospitalId);

    Task<Result<HospitalRespModel>> UpdateHospital(UpdateHospitalCommand reqModel);
}
