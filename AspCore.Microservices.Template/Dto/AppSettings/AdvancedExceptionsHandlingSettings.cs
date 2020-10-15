namespace AspCore.Microservices.Template.Dto.AppSettings
{
	/// <summary>
	/// Advanced exceptions handling settings
	/// </summary>
	public class AdvancedExceptionsHandlingSettings
	{
		/// <summary>
		/// Save POST request body as json if errors thrown while process
		/// </summary>
		public bool SaveRequestBodyOnErrors { get; set; }
	}
}