using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace CluedIn.ExternalSearch.Providers.Web.Model
{
	/// <summary>
	/// A serialization-safe DTO that captures the fields of a <see cref="RestResponse"/> using
	/// only primitive types.  RestSharp types such as <c>ContentType</c>, <c>HeaderParameter</c>,
	/// and <c>HttpHeaderValueCollection</c> all lack parameterless constructors and cannot be
	/// round-tripped by Newtonsoft.Json when TypeNameHandling is enabled in CluedIn Core's
	/// JsonSerializer.  By storing only strings / enums / Uri this class is fully deserializable.
	/// </summary>
	public class WebRestResponse
	{
		public WebRestResponse()
		{
		}

		public WebRestResponse(RestResponse response)
		{
			this.ContentType        = response.ContentType;
			this.ContentLength      = response.ContentLength;
			this.ContentEncoding    = [..response.ContentEncoding];
			this.Content            = response.Content;
			this.StatusCode         = response.StatusCode;
			this.StatusDescription  = response.StatusDescription;
			this.RawBytes           = response.RawBytes;
			this.ResponseUri        = response.ResponseUri;
			this.Server             = response.Server;
			this.ResponseStatus     = response.ResponseStatus;
			this.ErrorMessage       = response.ErrorMessage;
			this.ErrorException     = response.ErrorException;

			if (response.Headers != null)
			{
				foreach (var h in response.Headers)
					Headers.Add(new HeaderDto { Name = h.Name, Value = h.Value });
			}

            if (response.Cookies == null) return;

            foreach (var c in response.Cookies)
            {
                if (c is Cookie cookie)
                    Cookies.Add(new CookieDto { Name = cookie.Name, Value = cookie.Value });
            }
        }

		public string            ContentType       { get; set; }
		public long?             ContentLength     { get; set; }
		public List<string>      ContentEncoding   { get; set; } = [];
		public string            Content           { get; set; }
		public HttpStatusCode    StatusCode        { get; set; }
		public string            StatusDescription { get; set; }
		public byte[]            RawBytes          { get; set; }
		public Uri               ResponseUri       { get; set; }
		public string            Server            { get; set; }
		public ResponseStatus    ResponseStatus    { get; set; }
		public string            ErrorMessage      { get; set; }
		public Exception         ErrorException    { get; set; }
		public List<HeaderDto>   Headers           { get; set; } = [];
		public List<CookieDto>   Cookies           { get; set; } = [];

		/// <summary>
		/// Reconstructs a <see cref="RestResponse"/> from the stored primitive fields so that
		/// it can be passed to APIs that require a <see cref="RestResponse"/> instance (e.g.
		/// <c>OrganizationWebsiteParser.Parse</c>).
		/// </summary>
		public RestResponse ToRestResponse()
		{
			var r = new RestResponse
			{
                ContentType = ContentType,
                ContentLength = ContentLength,
                ContentEncoding = ContentEncoding,
                Content = Content,
                StatusCode = StatusCode,
                StatusDescription = StatusDescription,
                RawBytes = RawBytes,
                ResponseUri = ResponseUri,
                Server = Server,
                ResponseStatus = ResponseStatus,
                ErrorMessage = ErrorMessage,
                ErrorException = ErrorException,
            };

			return r;
		}

		public class HeaderDto
		{
			public string Name  { get; set; }
			public string Value { get; set; }
		}

		public class CookieDto
		{
			public string Name  { get; set; }
			public string Value { get; set; }
		}
	}
}
