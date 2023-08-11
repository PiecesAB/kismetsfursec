using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DanmakuMeshSample : MonoBehaviour {

    [System.Serializable]
    public struct Bullet
    {
        public Vector3 initialPosition;
        public Vector3 currentPosition;
        public void ChangeAllVelocity(Vector3 v)
        {
            for (int i = 0; i < velocities.Length; i++)
            {
                velocities[i] += v;
            }
        }
        public Rect imageSize;
        public Vector3[] velocities;
        public float[] velocityChangeTimes;
        public Vector3[] angularVelocity;
        public float[] angvelChangeTimes;
        public Vector3[] accelerations;
        public float[] accelChangeTimes;
        public Gradient[] color;
        public float[] colorCycleTime;
        public Rect colliderBox;
        public float damage;
    }
    public List<Bullet> bullets;
    public float timeOfBirth;
    public int bulletCap;
    public float advance;
    public Transform player;
    public bool collideWithDamage;
    public bool destroyAllWithOneHit;
    

    // Use this for initialization
    void Start() {
        //test
        //Bullet aaa = bullets[0];
        Bullet[] nevv = new Bullet[bulletCap];
        /*for (int i = 0; i <= bulletCap-1; i++)
        {
            nevv[i] = aaa;
            nevv[i].initialPosition = i*3f*new Vector3(Mathf.Cos(i/16f*Mathf.PI), Mathf.Sin(i / 16f * Mathf.PI), 0);
            nevv[i].velocities = new Vector3[] { Vector3.zero };
            nevv[i].velocityChangeTimes = new float[] { Mathf.Infinity };
            nevv[i].accelerations = new Vector3[] { nevv[i].initialPosition, -nevv[i].initialPosition };
            nevv[i].accelChangeTimes = new float[] {2f,2f+float.Epsilon };
            Gradient gg = new Gradient();
            gg.colorKeys = new GradientColorKey[] { new GradientColorKey(Color.HSVToRGB((i / 36f) % 1f, 1, 1), 0f) , new GradientColorKey(Color.HSVToRGB(((i+18f) / 36f) % 1f, 1, 1), 1f/2f), new GradientColorKey(Color.HSVToRGB((i / 36f) % 1f, 1, 1), 1f) };
            nevv[i].colorCycleTime = new float[] {2,2,2,2 };
            nevv[i].color = new Gradient[] { gg,gg,gg,gg};
            nevv[i].imageSize = nevv[i].colliderBox = new Rect(0,0,8f + (float)i / 10f, 8f + (float)i / 10f);
            nevv[i].damage = 0.5f;
        }*/
        bullets = new List<Bullet>(nevv);
    }
	
    public int VelAccelChangeValue(Bullet b,bool accel)
    {
        double z = DoubleTime.ScaledTimeSinceLoad - timeOfBirth + advance;
        float r = accel?b.velocityChangeTimes.Sum(): b.accelChangeTimes.Sum();
        float x = 0;
        for (int e3 = 0; e3< (accel ? b.velocityChangeTimes.Length : b.accelChangeTimes.Length); e3++)
        {
            x += accel?b.velocityChangeTimes[e3]:b.accelChangeTimes[e3];
            if (z%r < x)
            {
                return e3;
            }
        }
        return 0;
    }

    // Update is called once per frame
    void Update () {

        Mesh m = new Mesh();
        m.name = "A Generated Bullet Screen";

        Vector3[] vert = new Vector3[bulletCap * 4];
        Vector2[] uvs = new Vector2[bulletCap * 4];
        int[] tris = new int[bulletCap * 6];
        Color[] cols = new Color[bulletCap * 4];

        int i = 0;
        //GetComponent<PolygonCollider2D>().pathCount = bullets.Count;

        for (int gg = 0;gg<bullets.Count; gg++)
        {
            Bullet b = bullets[gg];
            int i6 = i * 6;
            int i4 = i * 4;
            bool nongone = true;

            b.ChangeAllVelocity(b.accelerations[VelAccelChangeValue(b,false)]*Time.timeScale*0.01666666f);
            b.currentPosition += b.velocities[VelAccelChangeValue(b,true)] * Time.timeScale * 0.01666666f;
            bullets[gg] = b;

            if (player.position.x <= transform.position.x + b.currentPosition.x - b.colliderBox.position.x + (b.colliderBox.size.x / 2) &&
                player.position.x >= transform.position.x + b.currentPosition.x - b.colliderBox.position.x - (b.colliderBox.size.x / 2) &&
                player.position.y <= transform.position.y + b.currentPosition.y - b.colliderBox.position.y + (b.colliderBox.size.y / 2) &&
                player.position.y >= transform.position.y + b.currentPosition.y - b.colliderBox.position.y - (b.colliderBox.size.y / 2))
            {
                bullets.RemoveAt(gg);
                if (player.GetComponent<KHealth>() != null)
                {
                    player.GetComponent<KHealth>().ChangeHealth(-b.damage,"energy bullet");
                }
                gg--;
                nongone = false;
            }

            if (nongone)
            {
                Vector3 blt = b.currentPosition /*+ transform.localPosition*/;

                vert[i4] = new Vector3(b.imageSize.xMin, b.imageSize.yMin) + blt;
                vert[i4 + 1] = new Vector3(b.imageSize.xMax, b.imageSize.yMin) + blt;
                vert[i4 + 2] = new Vector3(b.imageSize.xMax, b.imageSize.yMax) + blt;
                vert[i4 + 3] = new Vector3(b.imageSize.xMin, b.imageSize.yMax) + blt;

                uvs[i4] = Vector2.zero;
                uvs[i4 + 1] = Vector2.right;
                uvs[i4 + 2] = Vector2.one;
                uvs[i4 + 3] = Vector2.up;

                tris[i6] = i4;
                tris[i6 + 1] = i4 + 1;
                tris[i6 + 2] = i4 + 2;
                tris[i6 + 3] = i4;
                tris[i6 + 4] = i4 + 2;
                tris[i6 + 5] = i4 + 3;

                for (int x = 0; x <= 3; x++)
                {
                    cols[i4 + x] = b.color[x].Evaluate((float)(((DoubleTime.ScaledTimeSinceLoad - timeOfBirth) / b.colorCycleTime[x]) % 1f));
                }

                /*GetComponent<PolygonCollider2D>().SetPath( //REALLY LAGGY OMG
                    i,
                    new Vector2[4] 
                    { (Vector2)blt+new Vector2(b.colliderBox.x+b.colliderBox.width/2,b.colliderBox.y+b.colliderBox.height/2),
                    (Vector2)blt+new Vector2(b.colliderBox.x+b.colliderBox.width/2,b.colliderBox.y-b.colliderBox.height/2),
                    (Vector2)blt+new Vector2(b.colliderBox.x-b.colliderBox.width/2,b.colliderBox.y-b.colliderBox.height/2),
                    (Vector2)blt+new Vector2(b.colliderBox.x-b.colliderBox.width/2,b.colliderBox.y+b.colliderBox.height/2)
                    });*/

                i++;
            }
        }

        m.vertices = vert;
        m.uv = uvs;
        m.triangles = tris;
        m.colors = cols;
        //m.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = m;
        //m.colors = new Color[] { Color.white, Color.white, Color.red, Color.red };
    }
}
