using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiniJSON;
using Newtonsoft.Json;

public class JsonSerializer : ISerializer
{
	public void Serialize(TrackerEvent t, string filename)
	{

		string path = Application.dataPath + "/Resources/" + filename + ".json";
		Dictionary<string, object> dict = new Dictionary<string, object>();
		if (File.Exists(path))
		{
			string fileText = File.ReadAllText(path);
			dict = Json.Deserialize(fileText) as Dictionary<string, object>;
		}

		// Comprobamos que existe la IDGame
		if (!dict.ContainsKey(t.commonContent.idGame))
		{
			Dictionary<string, object> newGame = new Dictionary<string, object>();
			dict.Add(t.commonContent.idGame, newGame);
		}
		Dictionary<string, object> gameDict = dict[t.commonContent.idGame] as Dictionary<string, object>;

		// Comprobamos que existe la IDUSER
		if (!gameDict.ContainsKey(t.commonContent.idUser))
		{
			Dictionary<string, object> newUser = new Dictionary<string, object>();
			gameDict.Add(t.commonContent.idUser, newUser);
		}

		Dictionary<string, object> userDict = gameDict[t.commonContent.idUser] as Dictionary<string, object>;

		// Comprobamos que existe la IDSESSION
		if (!userDict.ContainsKey(t.commonContent.idSession))
		{
			Dictionary<string, object> newSession = new Dictionary<string, object>();
			userDict.Add(t.commonContent.idSession, newSession);
		}

		Dictionary<string, object> sessionDict = userDict[t.commonContent.idSession] as Dictionary<string, object>;

		if (!sessionDict.ContainsKey(t.commonContent.idEvent))
		{
			sessionDict.Add(t.commonContent.idEvent, CreateJsonEvent(t.commonContent));

			string JSONString = JsonConvert.SerializeObject(dict, Formatting.Indented);
			File.WriteAllText(path, JSONString);
		}
	}

	public Dictionary<string, object> CreateJsonEvent(CommonContent cc)
	{
		Dictionary<string, object> newEvent = new Dictionary<string, object>();
		foreach (var field in typeof(CommonContent).GetFields())
		{
			if (field.Name != "idGame" && field.Name != "idSession" && field.Name != "idUser" && field.Name != "idEvent")
			{
				newEvent.Add(field.Name, field.GetValue(cc));
			}
		}
		return newEvent;
	}
}
