using UnityEngine;
using System.IO;
using Environment = System.Environment;

public class ScreenShotManager : Singleton<ScreenShotManager>
{
	//WARNING: Sólo funciona en Windows! 
	private string route = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/My Games/Replanet/Screenshots/";

	public void TakeScreenShot()
	{
		if (!Directory.Exists(route))
		{
			Directory.CreateDirectory(route);
		}

		ScreenCapture.CaptureScreenshot(route + "Screenshot" + ".jpg");
	}
}
