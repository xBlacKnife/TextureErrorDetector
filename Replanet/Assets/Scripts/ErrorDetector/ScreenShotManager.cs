using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiniJSON;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class ScreenShotManager : Singleton<ScreenShotManager>
{
	private string screenshotRoute;
	private string camerainfoRoute;
	private string camerainfoFile;
	private string referenceRoute;
	private string compareRoute;

	[HideInInspector]
	public bool originalPhotoCompareMode = false;

    private void Awake()
    {
		if (FindObjectsOfType<ScreenShotManager>().Length > 1)
			Destroy(this.gameObject);
	}

    private void Start()
	{
		screenshotRoute = Application.dataPath + "../../../Error Detector/Originales/";
		camerainfoRoute = Application.dataPath + "../../../Error Detector/";
		referenceRoute = Application.dataPath + "../../../ImageCompare/images/Original/";
		compareRoute = Application.dataPath + "../../../ImageCompare/images/Captures/";

		camerainfoFile = "camerainfo.json";
	}

	public void TakeCompareImages(string screenshotName)
    {
		if(originalPhotoCompareMode)
			TakeScreenShot(screenshotName, referenceRoute);
		else TakeScreenShot(screenshotName, compareRoute);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			TakeErrorScreenshot("TestImage" + Time.time.ToString());
		}
	}


	public void TakeErrorScreenshot(string screenshotName)
    {
		TakeScreenShot(screenshotName, screenshotRoute);
		SaveCameraInfo(screenshotName);
    }

	private void TakeScreenShot(string SN, string route)
	{
		if (!Directory.Exists(screenshotRoute))
		{
			Directory.CreateDirectory(screenshotRoute);
		}

		string screenshotName = "Screenshot_" + SN;

		ScreenCapture.CaptureScreenshot(route + screenshotName + ".jpg");
	}

	public void SaveCameraInfo(string screenshotName)
	{

		screenshotName = "Screenshot_" + screenshotName;

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

		//Añade la ruta de la imagen al diccionario.
		Dictionary<string, object> ImageDirectory = new Dictionary<string, object>();
		ImageDirectory.Add("Image", screenshotRoute + screenshotName + ".jpg");

		//Un nuevo diccionario que engloba el nivel, las posiciones y las rotaciones de la cámara, así como la ruta en la que se encuentra la imagen
		Dictionary<string, object> newEvent = new Dictionary<string, object>();
		newEvent.Add("Scene", SceneManager.GetActiveScene().name);
		newEvent.Add("Camera_Position", CameraPositions);
		newEvent.Add("Camera_Rotation", CameraRatations);
		newEvent.Add("Image_Directory", ImageDirectory);
		newEvent.Add("Width", Camera.main.pixelWidth);
		newEvent.Add("Height", Camera.main.pixelHeight);

		//Al diccionario original, añadimos el diccionario anterior dentro de uno nuevo, que será el nombre de la captura.
		dict.Add(screenshotName, newEvent);

		//Finalmente guardamos el resultado en la ruta esperada.
		string JSONString = JsonConvert.SerializeObject(dict, Formatting.Indented);
		File.WriteAllText(camerainfoRoute + camerainfoFile, JSONString);
	}
}
