using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocelot.Library.Infrastructure.Requester;
using Ocelot.Library.Infrastructure.Responder;
using Ocelot.Library.Infrastructure.Services;

namespace Ocelot.Library.Middleware
{
    public class HttpRequesterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpRequester _requester;
        private readonly IHttpResponder _responder;
        private readonly IScopedRequestDataRepository _scopedRequestDataRepository;

        public HttpRequesterMiddleware(RequestDelegate next, 
            IHttpRequester requester, 
            IHttpResponder responder,
            IScopedRequestDataRepository scopedRequestDataRepository)
        {
            _next = next;
            _requester = requester;
            _responder = responder;
            _scopedRequestDataRepository = scopedRequestDataRepository;
        }

        public async Task Invoke(HttpContext context)
        {
            var downstreamUrl = _scopedRequestDataRepository.Get<string>("DownstreamUrl");

            if (downstreamUrl.IsError)
            {
                await _responder.CreateNotFoundResponse(context);
                return;
            }

            var response = await _requester
                .GetResponse(context.Request.Method, downstreamUrl.Data, context.Request.Body, 
                context.Request.Headers, context.Request.Cookies, context.Request.Query, context.Request.ContentType);

            await _responder.CreateResponse(context, response);

            await _next.Invoke(context);
        }
    }
}