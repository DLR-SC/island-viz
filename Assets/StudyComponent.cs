using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyComponent : MonoBehaviour
{
    private static string likerScale = "(stimme.zu)1..2..3..4..5..6..7(nicht.zu)";
    private static string asq1 = "Ich bin zufrieden mit der Leichtigkeit,\n mit der sich die Aufgabe erfüllen ließ.";
    private static string asq2 = "Ich bin zufrieden mit der Zeit,\n die es gebraucht hat,\n um die Aufgabe zu erfüllen.";
    private static string asq3 = "Ich bin bei dieser Aufgabe mit\n den mir zur Verfügung stehenden\n Informationen zufrieden";

    private static string aufgabe1 = "Vergleiche das Gesamtsystem zu den Zeitpunkten 2 und 3.\n Nenne die hinzugekommenen Bundles.\n Nenne die Bundles, die gelöscht wurden..";
    private static string aufgabe2 = "Vergleiche das Bundle <b>Data Model</b> an den Zeitpunkten 3 und 4.\n Welche Packages wurden hinzugefügt?\n Welche Packages wurden gelöscht?\n Welche Dateien wurden in bestehenden Paketen hinzugefügt?\n Welche Dateien wurden gelöscht, deren Packages noch existieren?";
    private static string aufgabe3 = "Vergleiche das Gesamtsystem zu den Zeitpunkten 3 und 4.\n Nenne die hinzugekommenen Bundles.\n Nenne die Bundles, die gelöscht wurden.";
    private static string aufgabe4 = "Vergleiche das <b>Bundle Executor</b> an den Zeitpunkten 3 und 4.\n Welche Packages wurden hinzugefügt?\n Welche Packages wurden gelöscht?\n Welche Dateien wurden in bestehenden Paketen hinzugefügt?\n Welche Dateien wurden gelöscht, deren Packages noch existieren?";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            IslandVizUI.Instance.ActivateNotification(asq1 + "\n" + likerScale);
        }
        else if (Input.GetKeyDown("w"))
        {
            IslandVizUI.Instance.ActivateNotification(asq2 + "\n" + likerScale);
        }
        else if (Input.GetKeyDown("e"))
        {
            IslandVizUI.Instance.ActivateNotification(asq3 + "\n" + likerScale);
        }
        else if (Input.GetKeyDown("a"))
        {
            IslandVizUI.Instance.ActivateNotification(aufgabe1);
        }
        else if (Input.GetKeyDown("s"))
        {
            IslandVizUI.Instance.ActivateNotification(aufgabe2);
        }
        else if (Input.GetKeyDown("d"))
        {
            IslandVizUI.Instance.ActivateNotification(aufgabe3);
        }
        else if (Input.GetKeyDown("f"))
        {
            IslandVizUI.Instance.ActivateNotification(aufgabe4);
        }
        else if (Input.GetKeyDown("y"))
        {
            IslandVizUI.Instance.DeactivateNotification();
        }


    }
}