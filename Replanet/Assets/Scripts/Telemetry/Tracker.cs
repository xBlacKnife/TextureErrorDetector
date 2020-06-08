using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

//NOTA PARA EL DESARROLLO: La mayoria son de timestamp dentro del nivel, y eventos genericos de inicio, fin, etc esos serán nuestros tracker assets,
//Los eventos una vez creados se envian al sistema de persistencia y estos los almacena en caso de que el, sistema de persistencia esté activado

//COSAS A TENER EN CUENTA:
//SI LOS EVENTOS SON MUY GENERICOS PUES PUEDE SERIALIZAR EL SERIALIZER, EN CASO DE SER RAROS HAY QUE LLAMAR AL .ToJSON O .ToCSV DEL EVENTO
//SE PUEDEN CREAR TODOS LOS TRACKEVENT COMO NOS DE LA GANA PARA CREAR LOS EVENTOS, NO PEDIR AL SERVIDOR DE VUELTA, MAS VIEN MIRAR EL CODIGO


public class Tracker : Singleton<Tracker>
{

	#region Métodos Monobehaviour

	public override void Awake()
	{
		base.Awake();
		if (UseTrackerSystem)
		{
			string time = System.DateTime.Now.ToString();

			SHA256 SHA256Hash;
			SHA256Hash = SHA256.Create();

			idGame_ = GameName;
			idSession_ = time;
			idUser_ = IDUser;

			Init();
		}
	}

	#endregion

	#region Variables de la clase y listas
	//Una lista que contiene el tipo de persistencia y si está activadp esto nos permite añadir al tracker
	//mas sistemas de persistencia sin tener que retocar el código, simplemente cuando se genere el evento
	//hacemos el send y le flush de los sistemas de persistencia activos.
	List<KeyValuePair<IPersistence, bool>> persistenceSystems;

    //Eventos que queremos comprobar. Cuando nos llegue un trackEvent lo pasamos por aqui y vemos si se acepta, 
    //el que lo acepte puede crearlo y hacer lo que sea con el, tendremos todos los trackerassets pero unos activados
    //y otros desactivados. Esto permitirá activar en tiempo de ejecución desde el editor o desede el propio juego
    List<KeyValuePair<ITrackerAsset, bool>> TrackerAssets;

    private string idGame_, idSession_, idUser_;
	#endregion

	#region Variables de configuración desde el editor
	[Header("Tracker Configuration")]
	public bool UseTrackerSystem = true;
	public string GameName;
    public string IDUser;

    [Header("Persistance types")]
    public bool ServerPersistance_ = false;
    public bool FilePersistance_ = true;

    [Header("Tracker assets")]
    public bool Progression_ = true;
    public bool LevelTimeStamp_ = true;
    public bool Resource_ = true;

    [Header("Serialization types")]
    public bool CSVFile_ = true;
    public bool JSONFile_ = false;
    #endregion

    #region start/end tracker methods
    /// <summary>
    /// Inicializamos todos los sistemas de persistencia pertinentes y los trackerevents
    /// </summary>
    public void Init() {

        DontDestroyOnLoad(gameObject);

		persistenceSystems = new List<KeyValuePair<IPersistence, bool>>();

        persistenceSystems.Add(new KeyValuePair<IPersistence, bool>(new ServerPersistence(CSVFile_, JSONFile_), ServerPersistance_));
        persistenceSystems.Add(new KeyValuePair<IPersistence, bool>(new FilePersistence(CSVFile_, JSONFile_), FilePersistance_));

		TrackerAssets = new List<KeyValuePair<ITrackerAsset, bool>>();

        TrackerAssets.Add(new KeyValuePair<ITrackerAsset, bool>(new TimeStampTracker(), LevelTimeStamp_));
        TrackerAssets.Add(new KeyValuePair<ITrackerAsset, bool>(new ResourceTracker(), Resource_));
        TrackerAssets.Add(new KeyValuePair<ITrackerAsset, bool>(new ProgressionTracker(), Progression_));
    }

    /// <summary>
    /// Cierra el sistema de tracking realizando un último flush con todos los sistemas de persistencia para asegurarnos
    /// de que esta todo guardado y almacenado.
    /// </summary>
    public void End() {
        for (int i = 0; i < persistenceSystems.Count; i++) {
            if(persistenceSystems[i].Value)
				persistenceSystems[i].Key.Flush();
        }
    }
	#endregion

	#region Getters/Setters

	public string getIDGame()
	{
		return idGame_;
	}

	public string getIDSession()
	{
		return idSession_;
	}

	public string getIDUser()
	{
		return idUser_;
	}

	#endregion

	#region tracker events methods

	//    -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	//    |                                                                                                                                                                                         |
	//    |    MUY IMPORTANTE:NO DAR LOS IDENTIFICADORES A MANO A LOS EVENTOS. PARA ESO ESTA EL METODO SET EVENT IDENTIFICATORS Y DEBE HACERSE DESPUES DE INICIALIZAR EL RESTO DEL COMMONCONTENT    |
	//    |                                                                                                                                                                                         |
	//    -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Crea el evento con los parámetros indicados y lo crea, posteriormente lo
	/// enviará a los sistemas de persistencia para almacenarlos y poder hacer un flush
	/// </summary>
	/// <param name="eventName">Tipo de evento generado</param>
	/// <param name="time">Momento en el que se produce el evento</param>
	/// <param name="level">Nivel en el que se produce el evento</param>
	public void TrackEvent(EventName eventName, EventType eventType, string time, int level) {       
       TrackerEvent t = new TrackerEvent(); ;

        t.commonContent.eventName_ = eventName;
        t.commonContent.eventType_ = eventType;
        t.commonContent.TimeStamp = time;
        t.commonContent.clicks = -1;
        t.commonContent.level = level;
        setEventIdentificators(ref t);
        

        //Comprobamos si es aceptado el evento
        if (checkAccepted(t)) { PersistanceSend(t); }
    }

    /// <summary>
    /// Crea el evento con los parámetros indicados y lo crea, posteriormente lo
    /// enviará a los sistemas de persistencia para almacenarlos y poder hacer un flush
    /// </summary>
    /// <param name="eventName">Tipo de evento generado</param>
    /// <param name="time">Momento en el que se produce el evento</param>
    public void TrackEvent(EventName eventName, EventType eventType, string time)
    {
        TrackerEvent t = new TrackerEvent();
        //LOS ID DE JUEGO SESIÓN Y USUARIO LOS TOMAREMOS DE LA INSTANCIA DEL GAMEMANAGER
        t.commonContent.eventName_ = eventName;
        t.commonContent.eventType_ = eventType;
        t.commonContent.TimeStamp = time;
        t.commonContent.clicks = -1;
        t.commonContent.level = -1;
        setEventIdentificators(ref t);


        //Comprobamos si es aceptado el evento
        if (checkAccepted(t)) { PersistanceSend(t); }
	}

    /// <summary>
    /// Crea el evento con los parámetros indicados y lo crea, posteriormente lo
    /// enviará a los sistemas de persistencia para almacenarlos y poder hacer un flush
    /// </summary>
    /// <param name="eventName">Tipo de evento generado</param>
    /// <param name="level">Nivel en el que se produce el evento</param>
    /// <param name="clicks">Numero de clics registrados</param>
    public void TrackerEvent(EventName eventName, EventType eventType, int level, int clicks) 
    {
        TrackerEvent t = new TrackerEvent();

        t.commonContent.eventName_ = eventName;
        t.commonContent.eventType_ = eventType;
        t.commonContent.TimeStamp = "-1";
        t.commonContent.clicks = clicks;
        t.commonContent.level = level;
        setEventIdentificators(ref t);

        //Comprobamos si es aceptado el evento
        if (checkAccepted(t)) { PersistanceSend(t); }
	}
    #endregion

    #region aux methods
    /// <summary>
    /// comprueba si el evento es aceptado por alguno de los tracker assets
    /// </summary>
    /// <param name="t">el evento al que se aplica la comprobación de aceptación</param>
    /// <returns>true si es aceptado, false en caso contrario</returns>
    private bool checkAccepted(TrackerEvent t) {

        int i = 0;
        bool accepted = false;
        while (i < TrackerAssets.Count && !accepted)
        {
            if (TrackerAssets[i].Value) accepted = TrackerAssets[i].Key.accept(t);
            if (!accepted) i++;
        }

        return accepted;
    }

    /// <summary>
    /// Envia al sistema de persistencia el evento para que lo almacene
    /// </summary>
    /// <param name="t">El evento que queremos almacenar en el sistema de persistencia</param>
    private void PersistanceSend(TrackerEvent t) {
        for (int i = 0; i < persistenceSystems.Count; i++) {
            if (persistenceSystems[i].Value) persistenceSystems[i].Key.Send(t);
        }
    }

    /// <summary>
    /// Realiza el Flush de los sistemas de persistencia activos
    /// </summary>
    public void PersistanceFlush()
    {
        for (int i = 0; i < persistenceSystems.Count; i++)
        {
            if (persistenceSystems[i].Value) 
                persistenceSystems[i].Key.Flush();
        }
    }

    /// <summary>
    /// Crea los identificadores necesarios para el evento
    /// </summary>
    /// <param name="t">evento al que se le crean los identificadores</param>
    private void setEventIdentificators(ref TrackerEvent t) {
        t.commonContent.idUser = idUser_;
        t.commonContent.idSession = idSession_;
        t.commonContent.idGame = idGame_;
        t.setEventID();
    }
    #endregion
}

