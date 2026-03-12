using ASCOM.Alpaca;

namespace Sentinel.Data
{
	internal class UserService : IUserService
	{
		public async Task<bool> Authenticate(string username, string password)
		{
			return await Task.Run(() =>
			{
				try
				{
					return username == Program.settings.UserName && Hash.Validate(Program.settings.Password, password);
				}
				catch
				{
					return false;
				}
			}

			);
		}

		public bool UseAuth
		{
			get => Program.settings.UseAuth;
		}
	}
}