using UnityEngine;

public class Billboard : MonoBehaviour
{   
    [SerializeField]
    Sprite[]sprites;
    public void SetSprite(int index){
        this.GetComponent<SpriteRenderer>().sprite = sprites[index];
    }


    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform.position, Vector3.up);
    }
}
