using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donor.Commands;

public class DeleteDonorCommand : IRequest<Result<DonorRespModel>>
{
    public int Id { get; set; }
}
