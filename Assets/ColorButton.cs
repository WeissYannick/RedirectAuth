using UnityEngine;

public class ColorButton : MonoBehaviour
{

    public void DrawRed()
    {
        GetComponent<PressableButton>().ColorButton(Color.red);
    }
    
    public void DrawWhite()
    {
        GetComponent<PressableButton>().ColorButton(Color.white);
    }
}
