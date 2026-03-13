using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donations.Queries;

public class GetDonationByIdQuery : IRequest<Result<DonationRespModel>>
{
    public int Id { get; set; }
}
