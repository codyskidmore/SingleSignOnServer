using System.ComponentModel.DataAnnotations;

namespace idsrv4testaspid.Models
{
	public class ChangePasswordModel
	{
		[Display(Name = "Old Passord")]
		[DataType(DataType.Password)]
		public string OldPassword { get; set; }

		[Display(Name = "New Passord")]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Display(Name = "Compare New Passord")]
		[Compare("NewPassword")]
		[DataType(DataType.Password)]
		public string ConfirmNewPassword { get; set; }
	}
}