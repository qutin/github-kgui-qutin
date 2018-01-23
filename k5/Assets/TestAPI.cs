using UnityEngine;
using System.Collections;

public class TestAPI : MonoBehaviour {

    private float rotAngle = 0;
    private Vector2 pivotPoint;
    void OnGUI()
    {
        pivotPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        GUIUtility.RotateAroundPivot(rotAngle, pivotPoint);
        if (GUI.Button(new Rect(Screen.width / 2 , Screen.height / 2 - 25, 50, 50), "Rotate"))
            rotAngle += 10;
        GUIUtility.RotateAroundPivot(-rotAngle, pivotPoint);
        GUI.TextField(new Rect(Screen.width/2, Screen.height/2 + 30, 50, 20), "test feild");
    }
}
