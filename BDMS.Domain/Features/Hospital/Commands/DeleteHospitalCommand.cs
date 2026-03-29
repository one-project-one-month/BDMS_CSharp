using BDMS.Domain.Features.Hospital.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Hospital.Commands;

public class DeleteHospitalCommand : IRequest<Result<HospitalRespModel>>
{
    public int Id { get; set; }
}
