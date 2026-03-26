using BDMS.Domain.Features.Certificate.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Certificate.Queries
{
    public class GetAllCertificatesQuery : IRequest<Result<List<CertificateRespModel>>>
    {
    }
}
