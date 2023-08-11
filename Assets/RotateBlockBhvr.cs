using UnityEngine;
using System.Collections;

public class RotateBlockBhvr : MonoBehaviour {

    [Range(-180, 180)]
    public int angle;
    public Texture2D shatterBlock;

	// Use this for initialization
	void OnCollisionEnter2D(Collision2D coll)
    {
        if (!coll.rigidbody.isKinematic && GetComponent<Renderer>().isVisible)
        {
            FindObjectOfType<TestScriptRotateScene>().goalRotation = FindObjectOfType<TestScriptRotateScene>().gameObject.transform.eulerAngles.z + angle;
            for (int i=0; i <16; i+=4)
            {
                for (int j = 0; j < 16; j += 4)
                {
                    GameObject newPiece = new GameObject();
                    SpriteRenderer newSpr = newPiece.AddComponent<SpriteRenderer>();
                    newSpr.sprite = Sprite.Create(shatterBlock, new Rect(i, j, 4, 4), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect); ;
                    newSpr.sortingLayerName = "UI";
                    Rigidbody2D newRig = newPiece.AddComponent<Rigidbody2D>();
                    newRig.gravityScale = 50;
                    newRig.velocity = 300 * Fakerand.UnitCircle();
                    newPiece.transform.position = transform.position + new Vector3(i - 8, j - 8, 0);
                }

            }
            Destroy(gameObject);

        }
    }
	

}
