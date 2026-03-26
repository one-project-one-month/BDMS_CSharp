using BDMS.Domain.Features.Certificate.Models;
using BDMS.Domain.Features.Certificate.Queries;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Certificate.Handlers
{
    public class GetAllCertificatesHandler : IRequestHandler<GetAllCertificatesQuery, Result<List<CertificateRespModel>>>
    {
        private readonly ICertificateService _certificateService;
        public GetAllCertificatesHandler(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        public async Task<Result<List<CertificateRespModel>>> Handle(GetAllCertificatesQuery request, CancellationToken cancellation)
        {
            return await _certificateService.GetAllCertificates();
        }
    }
}
