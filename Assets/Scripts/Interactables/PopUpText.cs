using UnityEngine;
using TMPro;

public class PopUpText : MonoBehaviour, Iinteractable
{
    [SerializeField] private TextMeshProUGUI textObj;
    [SerializeField] private string text;


    private void Start()
    {
        textObj.gameObject.SetActive(false);
    }

    public void Interact(GameObject Instigator)
    {
        textObj.gameObject.SetActive(true);
        textObj.text = text;
    }

}
