using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class CSVSerializer : ISerializer
{
    //La constructora servira para crear el CSV con los campos existentes
    public CSVSerializer()
    {

        //Estrutura donde guardaremos los elementos del CSV
        List<string[]> rowData = new List<string[]>();

        //Primero insertamos los tipos
        int length_ = typeof(CommonContent).GetFields().Length;
        string[] rowDataTemp = new string[length_];

        /*CAMPOS*/
        rowDataTemp[0] = "eventType_";
        rowDataTemp[1] = "eventName_";
        rowDataTemp[2] = "TimeStamp";
        rowDataTemp[3] = "idGme";
        rowDataTemp[4] = "idSession";
        rowDataTemp[5] = "idUser";
        rowDataTemp[6] = "idEvent";
        rowDataTemp[7] = "clicks";
        rowDataTemp[8] = "eventType_";

        //Lo metemos en el Script final
        rowData.Add(rowDataTemp);

        //Metemos el CSV
        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        //Aniadimos todos los campos a un StreamBuilder que se usara para crear el CSV

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));

        string path = "Assets/Resources/TrackedEvents.csv";

        //Guardamos el CSV
        StreamWriter outStream = System.IO.File.CreateText(path);
        outStream.WriteLine(sb);
        outStream.Close();
    }
    public void Serialize(TrackerEvent t, string filename)
    {

        //Struct que contiene todos los elementos a insertar
        CommonContent EventContent = t.commonContent;

        //Ruta del archivo
        filename = "Assets/Resources/"+filename+".csv";

        //String que contiene todos los campos del evento
        string CSVstring = (EventContent.eventType_.ToString() + "," + EventContent.eventName_.ToString() + "," + EventContent.TimeStamp.ToString() + ","
            + EventContent.idGame + "," + EventContent.idSession + "," + EventContent.idUser + "," + EventContent.idEvent + ","
            + EventContent.clicks.ToString() + "," + EventContent.level.ToString()+"\n");

        //Actualizasmos el CSV
        File.AppendAllText(filename, CSVstring);
    }
}
