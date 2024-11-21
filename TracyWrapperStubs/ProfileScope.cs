using System.Runtime.CompilerServices;
namespace TracyWrapper 
{

	public class ProfileScope : IDisposable
	{
		/// <summary> This method is a stub that does nothing. </summary>
		public ProfileScope(string name, uint color = ZoneC.DEFAULT, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "") { }

		/// <summary> This method is a stub that does nothing. </summary>
		public ProfileScope(string name, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "") { }


		/// <summary> This method is a stub that does nothing. </summary>
		public void Dispose() { }

	}
}