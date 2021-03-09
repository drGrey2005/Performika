using System;

namespace PerformikaLib
{
	public class AuthData
	{
		public string Token { get; set; }
		public string RefreshToken { get; set; }
		public DateTime Expiration { get; set; }
		public string AutorizationMethod { get; set; }
	}
}
