using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;

    public float direction = 1f;

    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Debug.Log("Bulletのdirection: " + direction);

        // 球を右方向に飛ばす
        rb.velocity = new Vector2(direction * speed, 0);

        // 球を一定時間後に消す
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("弾が当たった相手: " + collision.gameObject.name);

        // 名前が Enemy のときだけ削除
        if (collision.gameObject.name == "Enemy")
        {
            Destroy(collision.gameObject);
        }
        // 球を消す
        Destroy(gameObject);

    }
}
