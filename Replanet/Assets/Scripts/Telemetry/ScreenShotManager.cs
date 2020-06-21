using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiniJSON;
using Newtonsoft.Json;

public class ScreenShotManager : Singleton<ScreenShotManager>
{
	private string screenshotRoute;
	private string camerainfoRoute;
	private string camerainfoFile;

	private void Start()
	{
		screenshotRoute = Application.dataPath + "/Resources/Screenshots/";
		camerainfoRoute = Application.dataPath + "/Resources/";
		camerainfoFile = "camerainfo.json";
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			TakeScreenShot();
		}
	}

	public void TakeScreenShot()
	{
		if (!Directory.Exists(screenshotRoute))
		{
			Directory.CreateDirectory(screenshotRoute);
		}

		string screenshotName = "Screenshot_Level_" + Globals.actualLevel.ToString() + "TimeStamp_" +  Time.time.ToString();

		ScreenCapture.CaptureScreenshot(screenshotRoute + screenshotName + ".jpg");
		SaveCameraInfo(screenshotName);
	}

	public void SaveCameraInfo(string screenshotName)
	{
		//Se comprueba si el directorio donde se pretenden guardar los datos existe y si no es así se genera.
		if (!Directory.Exists(camerainfoRoute))
		{
			Directory.CreateDirectory(camerainfoRoute);
		}

		//Comprobamos si el diccionario ya existe.
		Dictionary<string, object> dict = new Dictionary<string, object>();
		if (File.Exists(camerainfoRoute + camerainfoFile))
		{
			string fileText = File.ReadAllText(camerainfoRoute + camerainfoFile);
			dict = Json.Deserialize(fileText) as Dictionary<string, object>;
		}

		//Añade las posiciones de la cámara a un diccionario para su posterior uso
		Dictionary<string, object> CameraPositions = new Dictionary<string, object>();
		CameraPositions.Add("X", Camera.main.transform.position.x.ToString());
		CameraPositions.Add("Y", Camera.main.transform.position.y.ToString());
		CameraPositions.Add("Z", Camera.main.transform.position.z.ToString());

		//Añade las rotaciones de la cámara a un diccionario para su posterior uso.
		Dictionary<string, object> CameraRatations = new Dictionary<string, object>();
		CameraRatations.Add("X", Camera.main.transform.rotation.x.ToString());
		CameraRatations.Add("Y", Camera.main.transform.rotation.y.ToString());
		CameraRatations.Add("Z", Camera.main.transform.rotation.z.ToString());
		CameraRatations.Add("W", Camera.main.transform.rotation.w.ToString());

		//Un nuevo diccionario que engloba el nivel, las posiciones y las rotaciones de la cámara.
		Dictionary<string, object> newEvent = new Dictionary<string, object>();
		newEvent.Add("Level", Globals.actualLevel.ToString());
		newEvent.Add("Camera_Position", CameraPositions);
		newEvent.Add("Camera_Rotation", CameraRatations);

		//Al diccionario original, añadimos el diccionario anterior dentro de uno nuevo, que será el nombre de la captura.
		dict.Add(screenshotName, newEvent);

		//Finalmente guardamos el resultado en la ruta esperada.
		string JSONString = JsonConvert.SerializeObject(dict, Formatting.Indented);
		File.WriteAllText(camerainfoRoute + camerainfoFile, JSONString);
	}
}
