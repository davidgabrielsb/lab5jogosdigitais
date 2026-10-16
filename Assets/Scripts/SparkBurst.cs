using UnityEngine;
public class SparkBurst : MonoBehaviour
{
    Vector3 velocity;float lifetime=.5f;
    public static void Emit(Vector3 position,Color color)
    {
        var prefab=Resources.Load<GameObject>("Prefabs/Brilho");
        if(prefab==null)return;
        for(int i=0;i<9;i++)
        {
            var go=Instantiate(prefab,position,Quaternion.identity);go.GetComponent<SpriteRenderer>().color=color;
            go.GetComponent<SparkBurst>().velocity=Quaternion.Euler(0,0,i*40)*Vector3.up*Random.Range(1f,3f);
        }
    }
    void Update(){lifetime-=Time.deltaTime;transform.position+=velocity*Time.deltaTime;velocity+=Vector3.down*Time.deltaTime*5;transform.localScale*=1-Time.deltaTime*2;if(lifetime<=0)Destroy(gameObject);}
}
