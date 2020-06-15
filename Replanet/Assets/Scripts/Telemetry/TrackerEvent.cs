using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System;

/// <summary>
/// Los tipos de eventos que se generan pueden ser los siguientes
/// -Progression son aquellos que tienen que ver con el jeugo de forma directa, por ejemplo empezar la sesión, empezar el nivel etc
/// -TimeStampEvent Son aquellos que se refieren a un contador dentro del propio mundo o nivel distintos a eventos genéricos del jueg
/// -ResourceEvent son aquellos que hacen referencia a la interacción con específicos del juego
/// </summary>
public enum EventType { Progression, Time_Stamp_Event, Resource_Event } //No creo que usemos resource event

/// <summary>
/// Los distintos eventos que controlaremos son los siguientes
/// </summary>
public enum EventName { 
                        START_LEVEL, 
                        BONE_FOUND, 
                        NEED_BONE, 
                        LEVEL_CLICKS, 
                        FINISH_LEVEL,
                        ENTRY_SELECTOR,         //Con respecto al documento realmente no necesitamos almacenar el nivel al que debería ir porque no aporta información.
                        EXIT_SELECTOR 
                       }

public struct CommonContent
{
    public EventType eventType_;                //Tipo de evento, Obligatorio
    public EventName eventName_;                //Nombre identificativo del evento, Obligatorio
    public string TimeStamp;                     //De no tratarse de un evento que mida cliks  su valor será de -1
    public string idGame;                       //Crear con SHA o MD5 o mirar colo lo crea unity, Obligatorio
    public string idSession;                    //Crear con SHA o MD5, Obligatorio
    public string idUser;                       //Crear con SHA o MD5 si no es un juego que se necesite login, Obligatorio
    public string idEvent;                      //Se crea con SHA256 en funcion del resto de parámetros, Obligatorio
    public int clicks;                          //De no tratarse de un evento que mida clicks su valor será de -1
    public int level;                           //Número de nivel en el que se produce el evento
}

public class TrackerEvent
{
    public CommonContent commonContent;

    /// <summary>
    /// Genera una hash con SHA256 para crear un id unico de evento. Para ello se usa el resto de valores del commonContent
    /// </summary>
    public void setEventID() {
        SHA256 SHA256Hash;
        SHA256Hash = SHA256.Create();
        //Generamos un string que usaremos para hacer el hash
        string concat = commonContent.clicks.ToString() +
                        commonContent.eventName_.ToString() +
                        commonContent.eventType_.ToString() +
                        commonContent.idGame.ToString() +
                        commonContent.idSession.ToString() +
                        commonContent.idUser.ToString() +
                        commonContent.level.ToString() +
                        commonContent.TimeStamp.ToString() +
                        System.DateTime.Now.ToString();


        //Creamos la hash
        byte[] aux = SHA256Hash.ComputeHash(Encoding.Default.GetBytes(concat));//Obtenemos los bytes que genera MD5 al crear la hash      
        System.Text.StringBuilder sb = new System.Text.StringBuilder(); //Creamos un constructor de string
        for (int i = 0; i < aux.Length; ++i) { sb.Append(aux[i].ToString("x2")); } //Generamos el string que forma el conjunto de bytes que componen la hash
        commonContent.idEvent =  sb.ToString(); //Lo volcamos al valor que queremos guardar
    }

    #region caso de serialización especial se usarán estos métodos
    /// <summary>
    /// En caso de ser un evento mas complejo implementará su propia manera de serializar a CSV
    /// </summary>
    /// <returns>String: contenido del CommonContent convertido a string</returns>
    public string ToCSV() { return null; }

    /// <summary>
    /// En caso de ser un evento mas complejo implementará su propia manera de serializar a JSON
    /// </summary>
    /// <returns>String: contenido del CommonContent convertido a string</returns>
    public string ToJson() { return null; }
    #endregion
}
