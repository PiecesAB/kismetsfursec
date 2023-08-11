using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingWall : MonoBehaviour {

    [System.Serializable]
    public struct CubeInfo
    {
        public Vector2 heightRange;
        public Vector2 widthRange;

        public CubeInfo(Vector2 _heightRange, Vector2 _widthRange)
        {
            heightRange = _heightRange;
            widthRange = _widthRange;
        }
    }

    public Mesh cubeMesh;
    public Material cubeMat;
    public float height;
    public float startRotation;
    [Range(-5f,5f)]
    public float rotationSpeed;
    public CubeInfo[] wall;
    public float poleWidth;
    public List<BoxCollider2D> cubes = new List<BoxCollider2D>();
    public Collider2D groundCollider;
    public GameObject smackEffect;
    public float damage = 10f;

    GameObject InitCube(CubeInfo cube)
    {
        GameObject c = new GameObject();
        c.transform.SetParent(transform);
        c.transform.localPosition = new Vector3((cube.widthRange.x + cube.widthRange.y) * 0.5f * poleWidth, (cube.heightRange.x + cube.heightRange.y) * 0.5f * height);
        c.transform.localScale = new Vector3((cube.widthRange.y - cube.widthRange.x) * poleWidth, (cube.heightRange.y - cube.heightRange.x) * height, 1);
        c.layer = 8;
        MeshFilter mf = c.AddComponent<MeshFilter>();
        mf.mesh = cubeMesh;
        MeshRenderer mr = c.AddComponent<MeshRenderer>();
        mr.material = cubeMat;
        mr.material.mainTextureScale = new Vector2(c.transform.localScale.x/4f, c.transform.localScale.y/32f);
        BoxCollider2D bc2 = c.AddComponent<BoxCollider2D>();
        bc2.isTrigger = true;
        RotatingWallCollider rcc = c.AddComponent<RotatingWallCollider>();
        rcc.mom = this;
        return c;
    }

	void Start () {
        for (int i = 0; i < wall.Length; i++)
        {
            GameObject cn = InitCube(wall[i]);
            cubes.Add(cn.GetComponent<BoxCollider2D>());
        }
        transform.localEulerAngles = new Vector3(startRotation, 0, 0);
	}

    public void Collided(Collider2D c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            GameObject sm = Instantiate(smackEffect, c.transform.position, Quaternion.identity);
            FollowThePlayer ft = Camera.main.GetComponent<FollowThePlayer>();
            if (ft != null)
            {
                ft.vibSpeed += 2f;
            }
            if (c.GetComponent<KHealth>() != null)
            {
                c.GetComponent<KHealth>().ChangeHealth(-damage, "fishwall");
            }
        }
    }

    void Update () {
        startRotation += rotationSpeed*Time.timeScale;
        startRotation %= 360;
        transform.localEulerAngles = new Vector3(startRotation, 0, 0);
        groundCollider.transform.localEulerAngles = -transform.localEulerAngles;
        if (Mathf.Repeat(-startRotation*Mathf.Sign(rotationSpeed),180) <= 5)
        {
            foreach (BoxCollider2D b in cubes)
            {
                b.enabled = true;
            }
        }
        else
        {
            foreach (BoxCollider2D b in cubes)
            {
                b.enabled = false;
            }
        }
    }
}
