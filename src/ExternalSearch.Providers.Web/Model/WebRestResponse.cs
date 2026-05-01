using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace CluedIn.ExternalSearch.Providers.Web.Model
{
	public class WebRestResponse : RestResponse
	{
		public WebRestResponse()
		{
		}

		public WebRestResponse(RestResponse response)
		{
			this.ContentType       = response.ContentType;
			this.ContentLength     = response.ContentLength;
			this.ContentEncoding   = response.ContentEncoding;
			this.Content           = response.Content;
			this.StatusCode        = response.StatusCode;
			this.StatusDescription = response.StatusDescription;
			this.RawBytes          = response.RawBytes;
			this.ResponseUri       = response.ResponseUri;
			this.Server            = response.Server;
			this.Cookies           = response.Cookies;
			this.Headers           = response.Headers;
			this.ResponseStatus    = response.ResponseStatus;
			this.ErrorMessage      = response.ErrorMessage;
			this.ErrorException    = response.ErrorException;
		}
	}
}
