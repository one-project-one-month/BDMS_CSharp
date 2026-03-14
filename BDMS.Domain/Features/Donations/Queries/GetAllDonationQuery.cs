using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donation.Queries;

public class GetAllDonationQuery : IRequest<Result<List<DonationRespModel>>>
{

}
